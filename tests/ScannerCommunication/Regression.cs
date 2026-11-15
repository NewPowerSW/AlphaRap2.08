using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AlphaRap;
using AlphaRap.FlowProgramming;
using AlphaRapLibrary;
using NPSDK;

internal static class ScannerProbe
{
    /// <summary>先注册实际构建的程序集位置，再加载设备测试，不初始化机器或外部硬件。</summary>
    [STAThread]
    private static int Main(string[] args)
    {
        AppDomain.CurrentDomain.AssemblyResolve += (sender, request) =>
        {
            string name = new AssemblyName(request.Name).Name;
            foreach (string extension in new[] { ".dll", ".exe" })
            {
                string path = Path.Combine(args[0], name + extension);
                if (File.Exists(path)) return Assembly.LoadFrom(path);
            }
            return null;
        };
        return ScannerCases.Run(args[1]);
    }
}

internal static class ScannerCases
{
    private static int passed;
    private static readonly List<ScannerKeyenceTcp> scanners = new List<ScannerKeyenceTcp>();

    /// <summary>执行真实设备实例、流程入口及仅本地 TCP 服务器的离线回归。</summary>
    internal static int Run(string output)
    {
        Environment.CurrentDirectory = output;
        SysPara.LanguageDataDirectory = Path.Combine(output, "LanguageData");
        SysPara.SettingDataDirectory = Path.Combine(output, "Settings");
        ModuleManager.SettingDataDirectory = SysPara.SettingDataDirectory;
        try
        {
            Check("construction and invalid settings never connect", InvalidSettings);
            Check("concurrent opens share one connection", DuplicateOpen);
            Check("hostnames and refused connections retain typed outcomes", ConnectionOutcomes);
            Check("split multibyte data and split CRLF form complete barcodes", SplitFrames);
            Check("consecutive scans cannot consume an old coalesced barcode", OldBarcode);
            Check("ERROR and empty replies are rejected", Rejected);
            Check("partial response times out without publishing a barcode", PartialTimeout);
            Check("EOF rejects a partial barcode and fault requires explicit reopen", Disconnect);
            Check("frame length is a validated byte limit", FrameLimit);
            Check("cancellation interrupts a real pending read", Cancellation);
            Check("stop revokes queued scans and allows explicit restart", StopAndRestart);
            Check("queue timeout and cancellation preserve the active request", QueueLimits);
            Check("manual send owns its reply and preserves coalesced lines", ManualIo);
            Check("endpoint edits withdraw an in-flight response", EndpointChange);
            Check("close races cannot publish a delayed connection", ConnectCloseRace);
            Check("flow methods reject errors and retain the configured instance", FlowIntegration);
            Check("legacy scope propagates cancellation to synchronous scanning", LegacyCancellation);
            Check("diagnostics change language without changing configuration", Languages);
            Console.WriteLine("RESULT scanner communication: " + passed + "/18 passed; loopback only.");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
        finally { foreach (var scanner in scanners) scanner.Close(); }
    }

    private sealed class Server : IDisposable
    {
        private readonly TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
        private readonly CancellationTokenSource shutdown = new CancellationTokenSource();
        private readonly ConcurrentBag<TcpClient> peers = new ConcurrentBag<TcpClient>();
        private readonly ConcurrentBag<Task> workers = new ConcurrentBag<Task>();
        private readonly ConcurrentQueue<Exception> errors = new ConcurrentQueue<Exception>();
        private readonly Func<StreamReader, NetworkStream, int, CancellationToken, Task> script;
        private readonly Task accepting;
        internal int Connections;
        internal int Port { get { return ((IPEndPoint)listener.LocalEndpoint).Port; } }

        /// <summary>启动只绑定回环地址的协议脚本服务器。</summary>
        internal Server(Func<StreamReader, NetworkStream, int, CancellationToken, Task> action)
        {
            script = action; listener.Start(); accepting = AcceptAsync();
        }

        /// <summary>持续接收独立连接，脚本异常由测试退出时核对。</summary>
        private async Task AcceptAsync()
        {
            try
            {
                while (!shutdown.IsCancellationRequested)
                {
                    TcpClient client = await listener.AcceptTcpClientAsync().ConfigureAwait(false);
                    peers.Add(client); int number = Interlocked.Increment(ref Connections);
                    workers.Add(ServeAsync(client, number));
                }
            }
            catch (Exception error) when (error is SocketException || error is ObjectDisposedException) { }
        }

        /// <summary>为一个连接执行脚本，关闭及取消属于预期清理。</summary>
        private async Task ServeAsync(TcpClient client, int number)
        {
            try
            {
                using (client)
                using (NetworkStream stream = client.GetStream())
                using (var reader = new StreamReader(stream, Encoding.Default))
                    await script(reader, stream, number, shutdown.Token).ConfigureAwait(false);
            }
            catch (Exception error) when (error is IOException || error is SocketException || error is ObjectDisposedException || error is OperationCanceledException) { }
            catch (Exception error) { errors.Enqueue(error); }
        }

        /// <summary>停止所有本地脚本并观察任务，避免离线测试遗留后台连接。</summary>
        public void Dispose()
        {
            shutdown.Cancel(); listener.Stop();
            foreach (TcpClient peer in peers) peer.Close();
            Assert(accepting.Wait(2000), "Server accept task leaked");
            Assert(Task.WaitAll(workers.ToArray(), 2000), "Server script leaked");
            shutdown.Dispose();
            Exception error;
            if (errors.TryDequeue(out error)) throw new InvalidOperationException("Server script failed", error);
        }
    }

    /// <summary>创建同一个实际实例，配置只使用本地测试端口。</summary>
    private static ScannerKeyenceTcp Scanner(Server server, int timeout = 1000, int maximum = 100)
    {
        var scanner = new ScannerKeyenceTcp("OfflineScanner") { IP = "127.0.0.1", Port = server.Port, TimeOut = timeout, length = maximum };
        scanners.Add(scanner); return scanner;
    }

    /// <summary>执行一项场景并输出明确结果。</summary>
    private static void Check(string name, Action action) { action(); passed++; Console.WriteLine("PASS " + name); }

    /// <summary>校验配置不会创建连接或改写输入值。</summary>
    private static void InvalidSettings()
    {
        var scanner = new ScannerKeyenceTcp("Unopened"); scanners.Add(scanner);
        Assert(!scanner.IsConnected && scanner.ScanOnceAsync().Result.Status == ScannerStatus.NotConnected, "Constructor connected hardware");
        foreach (int timeout in new[] { 0, -1, 60001 })
        {
            scanner.TimeOut = timeout;
            Assert(scanner.ConnectAsync().Result.Status == ScannerStatus.InvalidConfiguration && scanner.TimeOut == timeout, "Timeout silently changed");
        }
        scanner.TimeOut = 1000; scanner.length = 0;
        Assert(scanner.ConnectAsync().Result.Status == ScannerStatus.InvalidConfiguration, "Invalid frame size accepted");
        scanner.length = 100; scanner.Port = 70000;
        Assert(scanner.ConnectAsync().Result.Status == ScannerStatus.InvalidConfiguration, "Invalid port accepted");
    }

    /// <summary>多个打开请求共享一个连接，没有重复握手。</summary>
    private static void DuplicateOpen()
    {
        using (var server = new Server(async (reader, stream, number, token) => { while (await reader.ReadLineAsync() != null) { } }))
        {
            var scanner = Scanner(server);
            Task<ScannerResult>[] opens = Enumerable.Range(0, 12).Select(index => scanner.ConnectAsync()).ToArray();
            Task.WaitAll(opens);
            Assert(opens.All(task => task.Result.Succeeded) && scanner.IsConnected, "Duplicate open failed");
            Until(() => server.Connections == 1); scanner.Close();
        }
    }

    /// <summary>保留主机名支持，连接被拒绝不会伪装为成功或留下坏连接。</summary>
    private static void ConnectionOutcomes()
    {
        using (var server = new Server(async (reader, stream, number, token) => { while (await reader.ReadLineAsync() != null) { } }))
        {
            var scanner = Scanner(server); scanner.IP = "localhost";
            Assert(scanner.ConnectAsync().Result.Succeeded && scanner.IsConnected, "Hostname configuration regressed"); scanner.Close();
            int oldPort = server.Port;
            var probe = new TcpListener(IPAddress.Loopback, 0); probe.Start(); int refusedPort = ((IPEndPoint)probe.LocalEndpoint).Port; probe.Stop();
            scanner.IP = "127.0.0.1"; scanner.Port = refusedPort;
            var refused = scanner.ConnectAsync().Result;
            Assert((refused.Status == ScannerStatus.TransportError || refused.Status == ScannerStatus.Timeout) && !refused.CommandMayHaveExecuted && !scanner.IsConnected,
                "Refused connection result: " + refused.Status + ", uncertain=" + refused.CommandMayHaveExecuted + ", connected=" + scanner.IsConnected);
            scanner.Port = oldPort; Assert(scanner.ConnectAsync().Result.Succeeded, "Refused connection poisoned explicit reconnect"); scanner.Close();
        }
    }

    /// <summary>逐字节发送包含多字节字符的条码，CRLF 分包不丢字节。</summary>
    private static void SplitFrames()
    {
        using (var server = new Server(async (reader, stream, number, token) =>
        {
            Assert(await reader.ReadLineAsync() == "LON", "Unexpected trigger");
            foreach (byte value in Encoding.Default.GetBytes("ABC中123\r\n")) { await stream.WriteAsync(new[] { value }, 0, 1, token); await Task.Delay(2, token); }
            Assert(await reader.ReadLineAsync() == "LOFF", "Scan did not send stop");
            while (await reader.ReadLineAsync() != null) { }
        }))
        {
            var scanner = Scanner(server); scanner.Open();
            Assert(scanner.ScanOnceAsync().Result.Value == "ABC中123", "Split barcode truncated or decoded per packet");
            Assert(scanner.IsConnected && !scanner.IsRunning, "Successful connection status lost"); scanner.Close();
        }
    }

    /// <summary>同包的旧条码留在旧连接，下一次触发会建立新连接。</summary>
    private static void OldBarcode()
    {
        using (var server = new Server(async (reader, stream, number, token) =>
        {
            Assert(await reader.ReadLineAsync() == "LON", "Trigger missing");
            await Write(stream, "CODE" + number + "\r\nOLD\r\n", token);
            while (await reader.ReadLineAsync() != null) { }
        }))
        {
            var scanner = Scanner(server); scanner.Open();
            Assert(scanner.ScanOnceAsync().Result.Value == "CODE1", "First result invalid");
            Assert(scanner.ScanOnceAsync().Result.Value == "CODE2" && server.Connections == 2, "Old coalesced barcode reused"); scanner.Close();
        }
    }

    /// <summary>设备明确失败和空条码都不产生生产结果。</summary>
    private static void Rejected()
    {
        foreach (string reply in new[] { "ERROR\r\n", "\r\n" })
        using (var server = Reply(reply))
        {
            var scanner = Scanner(server); scanner.Open(); var result = scanner.ScanOnceAsync().Result;
            Assert(result.Status == ScannerStatus.Rejected && result.Value == null && !scanner.IsConnected, "Failure became barcode"); scanner.Close();
        }
    }

    /// <summary>没有终止符的半包必须等待，超时后关闭并保持故障状态。</summary>
    private static void PartialTimeout()
    {
        using (var server = Reply("PARTIAL"))
        {
            var scanner = Scanner(server, 100); scanner.Open();
            Stopwatch elapsed = Stopwatch.StartNew(); var result = scanner.ScanOnceAsync().Result;
            Assert(result.Status == ScannerStatus.Timeout && result.Value == null && result.CommandMayHaveExecuted && elapsed.ElapsedMilliseconds < 1500, "Partial data published or timeout blocked");
            Assert(scanner.ConnectionState == ScannerConnectionState.Faulted && scanner.ScanOnceAsync().Result.Status == ScannerStatus.NotConnected, "Automatic retry after timeout"); scanner.Close();
        }
    }

    /// <summary>读到 EOF 不接受半条码，显式重开后可以使用新连接。</summary>
    private static void Disconnect()
    {
        using (var server = new Server(async (reader, stream, number, token) =>
        {
            Assert(await reader.ReadLineAsync() == "LON", "Trigger missing");
            await Write(stream, number == 1 ? "HALF" : "NEW\n", token);
            if (number == 1) return;
            while (await reader.ReadLineAsync() != null) { }
        }))
        {
            var scanner = Scanner(server); scanner.Open();
            Assert(scanner.ScanOnceAsync().Result.Status == ScannerStatus.Disconnected, "EOF accepted partial barcode");
            Assert(scanner.ScanOnce() == "ERROR", "Legacy sentinel changed");
            scanner.Open(); Assert(scanner.ScanOnceAsync().Result.Value == "NEW", "Explicit recovery failed"); scanner.Close();
        }
    }

    /// <summary>上限按字节校验，等于上限允许，多一个字节拒绝。</summary>
    private static void FrameLimit()
    {
        foreach (string value in new[] { "1234\r", "12345\r" })
        using (var server = Reply(value))
        {
            var scanner = Scanner(server, maximum: 4); scanner.Open(); var result = scanner.ScanOnceAsync().Result;
            Assert(value.Length == 5 ? result.Value == "1234" : result.Status == ScannerStatus.ProtocolError && result.Value == null, "Frame byte bound invalid"); scanner.Close();
        }
    }

    /// <summary>在服务器确认触发后取消，检验真实挂起网络读取的退出。</summary>
    private static void Cancellation()
    {
        var entered = Signal(); var stopped = Signal();
        using (var server = new Server(async (reader, stream, number, token) =>
        {
            Assert(await reader.ReadLineAsync() == "LON", "Trigger missing"); entered.TrySetResult(true);
            if (await reader.ReadLineAsync() == "LOFF") stopped.TrySetResult(true);
        }))
        using (var stop = new CancellationTokenSource())
        {
            var scanner = Scanner(server, 10000); scanner.Open(); Task<ScannerResult> scan = scanner.ScanOnceAsync(stop.Token);
            Wait(entered.Task); Stopwatch elapsed = Stopwatch.StartNew(); stop.Cancel(); Wait(scan);
            Assert(scan.Result.Status == ScannerStatus.Cancelled && scan.Result.CommandMayHaveExecuted && !scanner.IsRunning && elapsed.ElapsedMilliseconds < 1500, "Cancellation did not exit actual I/O");
            Wait(stopped.Task); scanner.Close();
        }
    }

    /// <summary>整机停止入口所用设备停止可撤销当前和排队扫码，停止完成前不释放占用。</summary>
    private static void StopAndRestart()
    {
        var entered = Signal();
        using (var server = new Server(async (reader, stream, number, token) =>
        {
            Assert(await reader.ReadLineAsync() == "LON", "Trigger missing");
            if (number == 1) entered.TrySetResult(true); else await Write(stream, "RESTART\r\n", token);
            while (await reader.ReadLineAsync() != null) { }
        }))
        {
            var scanner = Scanner(server, 10000); scanner.Open(); var active = scanner.ScanOnceAsync(); Wait(entered.Task);
            Task<ScannerResult>[] queued = Enumerable.Range(0, 6).Select(index => scanner.ScanOnceAsync()).ToArray();
            Task stop = scanner.StopAsync(); Task repeated = scanner.StopAsync(); Wait(stop); Wait(repeated); Wait(active); Task.WaitAll(queued);
            Assert(active.Result.Status == ScannerStatus.Cancelled && queued.All(task => task.Result.Status == ScannerStatus.Cancelled), "Old queue survived stop");
            Assert(!scanner.IsConnected && !scanner.IsRunning && scanner.ConnectionState == ScannerConnectionState.Disconnected, "Stop released too early");
            scanner.Open(); Assert(scanner.ScanOnceAsync().Result.Value == "RESTART", "Restart failed"); scanner.Close();
        }
    }

    /// <summary>排队请求的时限和取消不关闭正在扫码的另一条请求。</summary>
    private static void QueueLimits()
    {
        var entered = Signal();
        using (var server = new Server(async (reader, stream, number, token) =>
        {
            Assert(await reader.ReadLineAsync() == "LON", "Trigger missing"); entered.TrySetResult(true);
            await Task.Delay(250, token); await Write(stream, "OWNER\r\n", token);
            while (await reader.ReadLineAsync() != null) { }
        }))
        {
            var scanner = Scanner(server); scanner.Open(); var owner = scanner.ScanOnceAsync(); Wait(entered.Task);
            scanner.TimeOut = 30; var timed = scanner.ScanOnceAsync().Result;
            Assert(timed.Status == ScannerStatus.Timeout && !timed.CommandMayHaveExecuted, "Queue timeout closed owner");
            scanner.TimeOut = 1000;
            using (var cancel = new CancellationTokenSource(30)) Assert(scanner.ScanOnceAsync(cancel.Token).Result.Status == ScannerStatus.Cancelled, "Queue cancellation ignored");
            Assert(owner.Result.Value == "OWNER" && scanner.IsConnected, "Owner response lost"); scanner.Close();
        }
    }

    /// <summary>原始命令保留其接收权，同包多行分次读取，扫码不能抢走手动应答。</summary>
    private static void ManualIo()
    {
        using (var server = new Server(async (reader, stream, number, token) =>
        {
            string command = await reader.ReadLineAsync();
            await Write(stream, command == "READ" ? "FIRST\r\nSECOND\r\n" : "SCAN\r\n", token);
            while (await reader.ReadLineAsync() != null) { }
        }))
        {
            var scanner = Scanner(server); scanner.Open(); Assert(scanner.Send("READ") == "", "Raw send failed");
            Assert(scanner.ScanOnceAsync().Result.Status == ScannerStatus.Busy && scanner.Send("OTHER") != "", "Raw reply was stolen");
            Assert(scanner.ReceiveAsync().Result.Value == "FIRST" && scanner.ReceiveAsync().Result.Value == "SECOND", "Coalesced lines lost");
            Assert(scanner.ScanOnceAsync().Result.Value == "SCAN", "Manual data contaminated next scan");
            scanner.TimeOut = 50; ScannerResult idle = scanner.ReceiveAsync().Result;
            Assert(idle.Status == ScannerStatus.Timeout && !idle.CommandMayHaveExecuted, "Idle receive reported a new uncertain command"); scanner.Close();
        }
    }

    /// <summary>参数编辑期间旧端点返回的数据不能发布到新的设备配置。</summary>
    private static void EndpointChange()
    {
        var entered = Signal(); var release = Signal();
        using (var first = new Server(async (reader, stream, number, token) =>
        {
            Assert(await reader.ReadLineAsync() == "LON", "Trigger missing"); entered.TrySetResult(true); await release.Task;
            await Write(stream, "OLD\r\n", token); while (await reader.ReadLineAsync() != null) { }
        }))
        using (var second = Reply("NEW\r\n"))
        {
            var scanner = Scanner(first); scanner.Open(); var scan = scanner.ScanOnceAsync(); Wait(entered.Task);
            scanner.Port = second.Port; release.TrySetResult(true);
            Assert(scan.Result.Status == ScannerStatus.Cancelled && scan.Result.Value == null, "Old endpoint response published");
            scanner.Open(); Assert(scanner.ScanOnceAsync().Result.Value == "NEW", "New endpoint failed"); scanner.Close();
        }
    }

    /// <summary>立即关闭与正在进行的异步连接竞态，结束后不允许迟到连接恢复状态。</summary>
    private static void ConnectCloseRace()
    {
        using (var server = new Server(async (reader, stream, number, token) => { while (await reader.ReadLineAsync() != null) { } }))
        {
            var scanner = Scanner(server);
            for (int round = 0; round < 12; round++)
            {
                Task<ScannerResult> open = scanner.ConnectAsync(); Wait(scanner.StopAsync()); Wait(open);
                Assert(!scanner.IsConnected && scanner.ConnectionState == ScannerConnectionState.Disconnected, "Late open restored connection");
            }
        }
    }

    /// <summary>真实框架调用同一个设备实例，ERROR 以及接收超时都进入故障而不是输出。</summary>
    private static void FlowIntegration()
    {
        using (var server = Reply("ERROR\r\n"))
        {
            var scanner = Scanner(server); scanner.Open();
            var resource = new FlowResource { Id = "scanner", Kind = "device", Instance = scanner, Methods = scanner.GetFlowMethods().ToList() };
            var resources = new FlowResources(); resources.Items.Add(resource.Id, resource);
            Assert(resource.Methods.Count(method => method.Id == "scanner.once") == 1, "Scanner helper methods duplicated actions");
            Assert(!resource.Methods.Any(method => method.Id.Contains("ScanOnceAsync") || method.Id.Contains("ConnectAsync")), "Transport helpers leaked into tools");
            bool failed = false;
            try { new FlowActions(resources).Execute(new FlowStep { Id = "scan", ExecutionMode = "Action", Target = resource.Id, Action = "scanner.once" }, new FlowExecutionContext(), CancellationToken.None).GetAwaiter().GetResult(); }
            catch (ScannerCommunicationException error) { failed = error.Status == ScannerStatus.Rejected; }
            Assert(failed && ReferenceEquals(resource.Instance, scanner), "ERROR became a successful flow output"); scanner.Close();
        }
        using (var server = Reply(""))
        {
            var scanner = Scanner(server, 50); scanner.Open();
            FlowMethod receive = scanner.GetFlowMethods().Single(method => method.Id == "device.receive"); bool failed = false;
            try { receive.Invoke(new FlowCall { Token = CancellationToken.None }).GetAwaiter().GetResult(); }
            catch (ScannerCommunicationException error) { failed = error.Status == ScannerStatus.Timeout; }
            Assert(failed, "Receive failure became an empty flow value"); scanner.Close();
        }
    }

    /// <summary>旧同步扫码入口继承当前运行器令牌，并在停止时抛出取消。</summary>
    private static void LegacyCancellation()
    {
        var entered = Signal();
        using (var server = new Server(async (reader, stream, number, token) =>
        {
            Assert(await reader.ReadLineAsync() == "LON", "Trigger missing"); entered.TrySetResult(true);
            while (await reader.ReadLineAsync() != null) { }
        }))
        using (var stop = new CancellationTokenSource())
        {
            var scanner = Scanner(server, 10000); scanner.Open();
            Task<string> scan;
            using (Flow_Chart.UseExecutionToken(stop.Token)) scan = Task.Run(() => scanner.ScanOnce());
            Wait(entered.Task); stop.Cancel(); bool cancelled = false;
            try { scan.GetAwaiter().GetResult(); } catch (OperationCanceledException) { cancelled = true; }
            Assert(cancelled && !scanner.IsRunning, "Legacy cancellation became ERROR string"); scanner.Close();
        }
    }

    /// <summary>三语诊断通过现有语言入口，现有设备参数不改变。</summary>
    private static void Languages()
    {
        var scanner = new ScannerKeyenceTcp("NamesAreData") { IP = "127.0.0.1", Port = 12345, TimeOut = 4321, length = 321 };
        scanners.Add(scanner); var messages = new List<string>();
        foreach (LanguageType language in new[] { LanguageType.Chinese, LanguageType.English, LanguageType.Español })
        {
            SysPara.LanguageShow = language; messages.Add(scanner.ScanOnceAsync().Result.Message);
            Assert(scanner.IP == "127.0.0.1" && scanner.Port == 12345 && scanner.TimeOut == 4321 && scanner.length == 321 && scanner.DeviceName == "NamesAreData", "Language switch changed settings");
        }
        Assert(messages.Distinct().Count() == 3, "Missing scanner translations");
    }

    /// <summary>构造一台在触发后发送指定数据并保持连接的测试扫码枪。</summary>
    private static Server Reply(string text)
    {
        return new Server(async (reader, stream, number, token) =>
        {
            string command;
            while ((command = await reader.ReadLineAsync()) != null)
                if (command == "LON") await Write(stream, text, token);
        });
    }

    /// <summary>写入原有协议编码的数据，不模拟 .NET 读取完成条件。</summary>
    private static Task Write(NetworkStream stream, string text, CancellationToken token)
    {
        byte[] data = Encoding.Default.GetBytes(text); return stream.WriteAsync(data, 0, data.Length, token);
    }

    /// <summary>建立不会内联执行测试后续逻辑的服务器阶段信号。</summary>
    private static TaskCompletionSource<bool> Signal() { return new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously); }

    /// <summary>确认实际异步操作及时结束，不仅检查发出过取消请求。</summary>
    private static void Wait(Task task) { Assert(task.Wait(2500), "Operation did not complete"); task.GetAwaiter().GetResult(); }

    /// <summary>等待异步服务器到达期望状态。</summary>
    private static void Until(Func<bool> test) { Assert(SpinWait.SpinUntil(test, 2000), "Expected server state not reached"); }

    /// <summary>失败时终止当前离线场景。</summary>
    private static void Assert(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
}
