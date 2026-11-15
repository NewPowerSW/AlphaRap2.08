using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AlphaRap;
using AlphaRap.PLC;
using AlphaRap.FlowProgramming;
using AlphaRapLibrary;
using NPSDK;
using System.Collections.Generic;
using System.ComponentModel;

internal static class PlcProbe
{
    private static int passed;
    private static string testRoot, appDirectory;
    private static string mode = "normal";
    private static PlcRequest lastWrite;
    private static int counter;

    /// <summary>运行真实会话源代码和独立假驱动进程；原生探测只访问回环地址。</summary>
    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Length == 1 && args[0] == "--orphan-probe")
        {
            var orphanSession = new PlcSession(99, Assembly.GetExecutingAssembly().Location);
            orphanSession.Connect("block", 502, 4000, 1, CancellationToken.None);
            var orphanWorker = (Process)typeof(PlcSession).GetField("worker", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(orphanSession);
            File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "orphan-pid"), orphanWorker.Id.ToString());
            orphanSession.Execute(new PlcRequest { Command = PlcCommand.Read, NetId = 99, Element = 0x35, Count = 2 }, 60000, CancellationToken.None);
            return 0;
        }
        if (args.Length == 4 && args[0] == PlcWorker.Switch)
        {
            PlcWorker.WatchParent(int.Parse(args[3]));
            PlcWorker.Run(args[1], args[2], FakeDriver);
            return 0;
        }
        appDirectory = args[0]; testRoot = args[1];
        Environment.CurrentDirectory = testRoot;
        SysPara.LanguageDataDirectory = Path.Combine(testRoot, "LanguageData");
        try
        {
            Check("typed read distinguishes zero and minus999 from errors", TypedRead);
            Check("word, bit, float and string payloads preserve sizes", Payloads);
            Check("invalid input never reaches the driver", InvalidInputs);
            Check("all instances share one serialized connection", SharedSession);
            Check("queue deadline and cancellation do not interrupt the owner", QueueLimits);
            Check("no response times out and requires explicit reconnect", TimeoutAndLateReply);
            Check("stop interrupts a permanently blocked native operation", StopBlocked);
            Check("interrupted writes are unknown and never replayed", UnknownWrite);
            Check("disconnect revokes queued operations and interrupts active I/O", Disconnect);
            Check("reconnect revokes queued work from the old endpoint", ReconnectGeneration);
            Check("worker crash becomes a communication fault", WorkerCrash);
            Check("execution token scopes propagate and restore", TokenScopes);
            Check("managed action and manual action receive stop tokens", HostTokens);
            Check("blocked worker exits if its parent crashes", ParentExit);
            Check("communication diagnostics support three languages", Languages);
            Check("real application worker and vendor DLL stop without replies", NativeLoopback);
            Console.WriteLine("PLC communication: " + passed + "/16 passed");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
        finally { INOVANCE.CloseAll(); }
    }

    /// <summary>模拟驱动的协议应答，可独立阻塞、崩溃或延迟，不访问任何外部设备。</summary>
    private static PlcReply FakeDriver(PlcRequest request)
    {
        if (request.Command == PlcCommand.Connect)
        {
            mode = request.Host; counter = 0;
            if (mode == "connect-block") Thread.Sleep(Timeout.Infinite);
            return new PlcReply { Code = mode == "refused" ? 0 : 1 };
        }
        if (mode == "crash") Environment.Exit(7);
        if (mode == "block" || mode == "write-block")
        {
            File.WriteAllText(Path.Combine(Environment.CurrentDirectory, "entered-" + request.NetId), request.Command.ToString());
            Thread.Sleep(Timeout.Infinite);
        }
        if (mode == "slow") Thread.Sleep(200);
        if (mode == "late") Thread.Sleep(800);
        if (mode == "error") return new PlcReply { Code = -17 };
        if (request.Command == PlcCommand.Write) { lastWrite = request; return new PlcReply { Code = 1 }; }
        byte[] data = new byte[PlcRequest.MaxBytes];
        int value = request.Address == 999 ? -999 : request.Address == 0 ? 0 : ++counter;
        Buffer.BlockCopy(BitConverter.GetBytes(value), 0, data, 0, 4);
        if (mode == "payload" && lastWrite != null)
        {
            Buffer.BlockCopy(lastWrite.Data, 0, data, 0, lastWrite.Data.Length);
            if (request.Address == 77) Buffer.BlockCopy(BitConverter.GetBytes(lastWrite.Count), 0, data, 0, 4);
        }
        return new PlcReply { Code = 1, Data = data };
    }

    /// <summary>运行一个独立场景，输出断言结果。</summary>
    private static void Check(string name, Action test)
    {
        test(); passed++; Console.WriteLine("PASS " + name);
    }

    /// <summary>为每个场景连接独立编号，并保留同编号的实际共享实例。</summary>
    private static INOVANCE Open(int id, string host)
    {
        var plc = new INOVANCE(id);
        Assert(plc.Connect(host, timeoutMs: 4000).Succeeded, "Cannot connect fake driver " + host);
        return plc;
    }

    /// <summary>检查成功状态和数据独立，失败值不覆盖为零或哨兵。</summary>
    private static void TypedRead()
    {
        var plc = Open(1, "normal");
        Assert(plc.ReadPlcResult("D0", 2).Value == "0", "Zero value lost");
        Assert(plc.ReadPlcResult("D999", 2).Value == "-999", "Signed value lost");
        Assert(plc.ReadPlc("D999", 2) == "-999" && plc.ReadErrorMessage == "", "Legacy signed value was a fault");
        Assert(plc.Connect("error").Succeeded, "Error driver handshake");
        var result = plc.ReadPlcResult("D0", 2);
        Assert(result.Status == PlcStatus.DriverError && result.DriverCode == -17 && result.Value == null, "Driver error became a value");
    }

    /// <summary>核对写入字数及小端数据，防止多写相邻地址。</summary>
    private static void Payloads()
    {
        var plc = Open(2, "payload");
        Assert(plc.WritePlc("D20", "-32768,32767", "int16"), "Int16 write");
        Assert(plc.ReadPlcResult("D20", 2, "int16").Value == "-32768,32767", "Int16 payload");
        Assert(plc.ReadPlcResult("D77", 2).Value == "2", "Int16 word count");
        Assert(plc.WritePlc("D20", "2147483647,-2147483648"), "Int32 range");
        Assert(plc.ReadPlcResult("D20", 4).Value == "2147483647,-2147483648", "Int32 payload");
        Assert(plc.ReadPlcResult("D77", 2).Value == "4", "Int32 word count");
        Assert(plc.WritePlc("M0", "1,0", "bit") && plc.ReadPlcResult("M0", 2, "bit").Value == "1,0", "Bit payload");
        Assert(plc.ReadPlcResult("D77", 2).Value == "2", "Bit count");
        Assert(plc.WritePlc("D20", "1.25", "float") && plc.ReadPlcResult("D20", 2, "float").Value == "1.25", "Float payload");
        Assert(plc.WritePlcString("D20", "abc") && plc.ReadPlcResult("D77", 2).Value == "2", "String word count");
        Assert(plc.ReadPlcString("D20", 2) == "abc", "String padding");
        Assert(plc.WritePlcString("D20", "abcd") && plc.ReadPlcString("D20", 2) == "abcd", "String without terminator");
    }

    /// <summary>非法参数在本地拒绝，未连接状态不会误报为有效 OFF。</summary>
    private static void InvalidInputs()
    {
        var plc = new INOVANCE(3);
        foreach (string address in new[] { "", "6000", "Q1", "D-1", "D2147483647" })
            Assert(plc.ReadPlcResult(address, 2).Status == PlcStatus.InvalidArgument, "Accepted " + address);
        Assert(plc.ReadPlcResult("D1", 1).Status == PlcStatus.InvalidArgument, "Partial Int32 read");
        Assert(plc.ReadPlcResult("D1", 8001).Status == PlcStatus.InvalidArgument, "Oversized read");
        Assert(plc.ReadPlcResult("D1", 2).Status == PlcStatus.NotConnected, "Disconnected read became zero");
        foreach (string value in new[] { "32768", "-32769", "1.1", "", "0," })
            Assert(plc.WritePlcResult("D1", value, "int16").Status == PlcStatus.InvalidArgument, "Accepted Int16 " + value);
        Assert(plc.WritePlcResult("M0", "2", "bit").Status == PlcStatus.InvalidArgument, "Accepted non-bit");
        Assert(plc.WritePlcResult("D0", "NaN", "float").Status == PlcStatus.InvalidArgument, "Accepted NaN");
    }

    /// <summary>跨对象并发读写保持每条请求与应答对应。</summary>
    private static void SharedSession()
    {
        Open(4, "normal");
        var first = new INOVANCE(4); var second = new INOVANCE(4);
        Task<string>[] reads = Enumerable.Range(0, 20).Select(index => Task.Run(() => (index % 2 == 0 ? first : second).ReadPlcResult("D1", 2, timeoutMs: 4000).RequireValue(CancellationToken.None))).ToArray();
        Task.WaitAll(reads);
        Assert(reads.Select(task => int.Parse(task.Result)).OrderBy(value => value).SequenceEqual(Enumerable.Range(1, 20)), "Requests crossed or duplicate session created");
    }

    /// <summary>排队超时或取消不会杀死正在使用连接的另一条请求。</summary>
    private static void QueueLimits()
    {
        var plc = Open(5, "slow");
        var owner = Task.Run(() => plc.ReadPlcResult("D1", 2));
        Thread.Sleep(50);
        Assert(plc.ReadPlcResult("D1", 2, timeoutMs: 25).Status == PlcStatus.Timeout, "Queue deadline ignored");
        using (var stop = new CancellationTokenSource(25))
            Assert(plc.ReadPlcResult("D1", 2, token: stop.Token).Status == PlcStatus.Cancelled, "Queued cancellation ignored");
        Assert(owner.Result.Succeeded && plc.ReadPlcResult("D1", 2).Value == "2", "Queue failure killed owner");
    }

    /// <summary>超时回收旧进程，下一次必须重连且不会读取旧响应。</summary>
    private static void TimeoutAndLateReply()
    {
        var plc = Open(6, "late");
        Stopwatch watch = Stopwatch.StartNew();
        Assert(plc.ReadPlcResult("D1", 2, timeoutMs: 80).Status == PlcStatus.Timeout, "Read never timed out");
        Assert(watch.ElapsedMilliseconds < 1500, "Timeout cleanup blocked");
        Assert(plc.ReadPlcResult("D1", 2).Status == PlcStatus.NotConnected, "Automatic reconnect");
        Assert(plc.Connect("normal").Succeeded && plc.ReadPlcResult("D1", 2).Value == "1", "Late response contaminated reconnect");
        Assert(new INOVANCE(66).Connect("connect-block", timeoutMs: 80).Status == PlcStatus.Timeout, "Connect timeout ignored");
        Assert(new INOVANCE(67).Connect("refused", timeoutMs: 2000).Status == PlcStatus.DriverError, "Refused connect became success");
    }

    /// <summary>令牌中断已进入阻塞驱动的读操作，确认旧进程真正退出。</summary>
    private static void StopBlocked()
    {
        var plc = Open(7, "block");
        using (var stop = new CancellationTokenSource())
        {
            var read = Task.Run(() => plc.ReadPlcResult("D1", 2, timeoutMs: 10000, token: stop.Token));
            WaitEntered(7);
            Stopwatch watch = Stopwatch.StartNew(); stop.Cancel();
            Assert(read.Wait(1500) && read.Result.Status == PlcStatus.Cancelled && watch.ElapsedMilliseconds < 1500, "Blocked stop failed");
            Assert(plc.ReadPlcResult("D1", 2).Status == PlcStatus.NotConnected, "Cancelled session reused");
        }
    }

    /// <summary>取消已发送写入时只报告未知，不自动重发或重新连接。</summary>
    private static void UnknownWrite()
    {
        var plc = Open(8, "write-block");
        using (var stop = new CancellationTokenSource())
        {
            var write = Task.Run(() => plc.WritePlcResult("D1", "1", timeoutMs: 10000, token: stop.Token));
            WaitEntered(8); stop.Cancel();
            Assert(write.Wait(1500) && write.Result.Status == PlcStatus.Cancelled && write.Result.WriteOutcomeUnknown && !write.Result.Succeeded, "Write falsely confirmed");
            Assert(plc.WritePlcResult("D1", "1").Status == PlcStatus.NotConnected, "Write replayed");
        }
    }

    /// <summary>显式断开会废弃排队数据操作并取消当前的管道等待。</summary>
    private static void Disconnect()
    {
        var plc = Open(9, "block");
        var active = Task.Run(() => plc.ReadPlcResult("D1", 2, timeoutMs: 10000));
        WaitEntered(9);
        var queued = Task.Run(() => plc.WritePlcResult("D1", "1", timeoutMs: 3000));
        Thread.Sleep(40);
        Assert(INOVANCE.Exit_ETH(9), "Disconnect failed");
        Assert(active.Result.Status == PlcStatus.Cancelled && queued.Result.Status == PlcStatus.Cancelled && !queued.Result.WriteOutcomeUnknown, "Queued work survived disconnect");
    }

    /// <summary>显式换接设备时，旧队列中的命令不能发到新设备。</summary>
    private static void ReconnectGeneration()
    {
        var plc = Open(10, "slow");
        var active = Task.Run(() => plc.ReadPlcResult("D1", 2));
        Thread.Sleep(40);
        var queued = Task.Run(() => plc.WritePlcResult("D1", "1", timeoutMs: 3000));
        Thread.Sleep(40);
        Assert(plc.Connect("normal").Succeeded, "Explicit reconnect failed");
        Assert(active.Result.Status == PlcStatus.Cancelled && queued.Result.Status == PlcStatus.Cancelled, "Stale generation survived");
        Assert(plc.ReadPlcResult("D1", 2).Value == "1", "Wrong fresh session");
    }

    /// <summary>子进程退出作为通信故障，不影响父进程与其他 PLC 会话。</summary>
    private static void WorkerCrash()
    {
        var plc = Open(11, "crash");
        Assert(plc.ReadPlcResult("D1", 2).Status == PlcStatus.WorkerError, "Crash not isolated");
        Assert(new INOVANCE(4).ReadPlcResult("D1", 2).Succeeded, "Another session was killed");
    }

    /// <summary>托管流程和手动任务使用的令牌通过异步调用链传递，嵌套退出后不泄漏。</summary>
    private static void TokenScopes()
    {
        using (var outer = new CancellationTokenSource())
        using (var inner = new CancellationTokenSource())
        {
            Assert(!Flow_Chart.CurrentExecutionToken.CanBeCanceled, "Initial token leaked");
            using (Flow_Chart.UseExecutionToken(outer.Token))
            {
                Assert(Task.Run(() => Flow_Chart.CurrentExecutionToken == outer.Token).Result, "Token did not flow to worker task");
                using (Flow_Chart.UseExecutionToken(inner.Token)) Assert(Flow_Chart.CurrentExecutionToken == inner.Token, "Nested token ignored");
                Assert(Flow_Chart.CurrentExecutionToken == outer.Token, "Outer token not restored");
            }
            Assert(!Flow_Chart.CurrentExecutionToken.CanBeCanceled, "Token leaked outside action");
        }
    }

    private sealed class DesignerContext : LicenseContext
    {
        public override LicenseUsageMode UsageMode { get { return LicenseUsageMode.Designtime; } }
    }

    /// <summary>通过真实框架对象方法和手动动作入口，检验同步驱动的环境停止令牌。</summary>
    private static void HostTokens()
    {
        var resources = new FlowResources();
        CancellationToken observed = CancellationToken.None;
        var method = FlowMethods.Sync("token.read", "读取", "Read", "Leer", new FlowField[0], new FlowField[0], call =>
        {
            observed = Flow_Chart.CurrentExecutionToken;
            observed.WaitHandle.WaitOne(2000);
            observed.ThrowIfCancellationRequested();
            return new Dictionary<string, object>();
        }, false);
        resources.Items.Add("token", new FlowResource { Id = "token", Kind = "device", Instance = new object(), Methods = new List<FlowMethod> { method } });
        using (var stop = new CancellationTokenSource())
        {
            Task action = new FlowActions(resources).Execute(new FlowStep { Id = "token-step", ExecutionMode = "Action", Target = "token", Action = method.Id }, new FlowExecutionContext(), stop.Token);
            Assert(SpinWait.SpinUntil(() => observed.CanBeCanceled, 1500), "Managed action did not receive ambient token");
            stop.Cancel();
            try { action.GetAwaiter().GetResult(); } catch (OperationCanceledException) { }
            Assert(action.IsCanceled && observed == stop.Token, "Managed action could not stop synchronous I/O");
        }
        LicenseContext previous = LicenseManager.CurrentContext;
        LicenseManager.CurrentContext = new DesignerContext();
        try
        {
            using (var module = new ModuleBaseForm())
            using (var entered = new ManualResetEventSlim())
            {
                SysPara.SystemMode = RunMode.IDLE;
                module.ExecuteManual(() =>
                {
                    observed = Flow_Chart.CurrentExecutionToken; entered.Set();
                    observed.WaitHandle.WaitOne(2000); observed.ThrowIfCancellationRequested();
                });
                Assert(entered.Wait(1500) && observed == module.StopManualTask.Token, "Manual action lost ambient token");
                module.RequestManualStop().GetAwaiter().GetResult();
                Assert(SpinWait.SpinUntil(() => !module.IsManualRun(), 1500), "Manual cancellation did not complete");
                Assert(observed.IsCancellationRequested, "Manual driver lost cancellation");
            }
        }
        finally { LicenseManager.CurrentContext = previous; }
    }

    /// <summary>检查固定诊断三语，并将语言文件隔离于测试目录。</summary>
    private static void Languages()
    {
        string[] messages = new string[3];
        int index = 0;
        foreach (LanguageType language in new[] { LanguageType.Chinese, LanguageType.English, LanguageType.Español })
        {
            SysPara.LanguageShow = language;
            messages[index++] = new PlcResult<bool>(PlcStatus.Timeout, unknown: true).Message;
        }
        Assert(messages.Distinct().Count() == 3 && messages.All(message => !message.Contains("{0}")), "Missing translation");
    }

    /// <summary>模拟父进程崩溃，确认看门线程能结束阻塞在驱动里的子进程。</summary>
    private static void ParentExit()
    {
        string marker = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "orphan-pid");
        Process child = null;
        using (Process parent = Process.Start(new ProcessStartInfo(Assembly.GetExecutingAssembly().Location, "--orphan-probe")
            { UseShellExecute = false, CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden }))
        {
            try
            {
                Assert(SpinWait.SpinUntil(() => File.Exists(marker) && new FileInfo(marker).Length > 0, 3000), "Orphan probe did not start");
                child = Process.GetProcessById(int.Parse(File.ReadAllText(marker)));
                WaitEntered(99);
                parent.Kill(); Assert(parent.WaitForExit(1500), "Parent did not exit");
                Assert(child.WaitForExit(2000), "Blocked worker survived parent exit");
            }
            finally
            {
                if (!parent.HasExited) { parent.Kill(); parent.WaitForExit(1500); }
                if (child != null) { if (!child.HasExited) child.Kill(); child.Dispose(); }
            }
        }
    }

    /// <summary>真实 EXE 子进程调用现有原生 DLL，仅连接不回包的本地 TCP 服务器。</summary>
    private static void NativeLoopback()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0); listener.Start();
        var session = new PlcSession(0, Path.Combine(appDirectory, "AlphaRap.exe"));
        TcpClient peer = null;
        try
        {
            Task<TcpClient> accepted = listener.AcceptTcpClientAsync();
            var connected = session.Connect("127.0.0.1", ((IPEndPoint)listener.LocalEndpoint).Port, 4000, 1, CancellationToken.None);
            Assert(connected.Succeeded, "Real worker cannot load native DLL: " + connected.Status + " " + connected.Detail);
            Assert(accepted.Wait(1500), "Vendor did not open loopback connection"); peer = accepted.Result;
            using (var stop = new CancellationTokenSource())
            {
                var read = Task.Run(() => session.Execute(new PlcRequest { Command = PlcCommand.Read, NetId = 0, Element = 0x35, Address = 6000, Count = 2 }, 10000, stop.Token));
                byte[] received = new byte[512]; peer.GetStream().ReadTimeout = 2000;
                Assert(peer.GetStream().Read(received, 0, received.Length) > 0, "Native read did not send a request");
                stop.Cancel();
                Assert(read.Wait(1500) && read.Result.Status == PlcStatus.Cancelled, "Real native operation did not stop");
                Assert(ConnectionClosed(peer, received), "Worker retained old TCP connection");
            }
            peer.Dispose(); peer = null;
            Task<TcpClient> acceptedAgain = listener.AcceptTcpClientAsync();
            Assert(session.Connect("127.0.0.1", ((IPEndPoint)listener.LocalEndpoint).Port, 4000, 1, CancellationToken.None).Succeeded, "Native reconnect failed");
            Assert(acceptedAgain.Wait(1500), "Native reconnect did not open connection"); peer = acceptedAgain.Result;
            Stopwatch timer = Stopwatch.StartNew();
            PlcResult<byte[]> timeout = session.Execute(new PlcRequest { Command = PlcCommand.Read, NetId = 0, Element = 0x35, Address = 6000, Count = 2 }, 150, CancellationToken.None);
            Assert(timeout.Status == PlcStatus.Timeout && timer.ElapsedMilliseconds < 1500, "Real native timeout was not bounded");
        }
        finally { session.Disconnect(); if (peer != null) peer.Dispose(); listener.Stop(); }
    }

    /// <summary>确认回环连接已关闭，正常 EOF 和 Windows 连接重置都表示旧连接消失。</summary>
    private static bool ConnectionClosed(TcpClient peer, byte[] buffer)
    {
        try { return peer.GetStream().Read(buffer, 0, buffer.Length) == 0; }
        catch (IOException error)
        {
            var socket = error.InnerException as SocketException;
            return socket != null && (socket.SocketErrorCode == SocketError.ConnectionReset || socket.SocketErrorCode == SocketError.ConnectionAborted);
        }
    }

    /// <summary>等待假驱动实际进入阻塞点，避免只测试取消尚未启动的任务。</summary>
    private static void WaitEntered(int id)
    {
        string marker = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "entered-" + id);
        Assert(SpinWait.SpinUntil(() => File.Exists(marker), 2000), "Driver did not enter request " + id);
    }

    /// <summary>失败时终止当前离线场景。</summary>
    private static void Assert(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
}
