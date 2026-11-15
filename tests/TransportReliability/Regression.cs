using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AlphaRap;
using AlphaRapLibrary;
using NPClient;
using NPSDK;

internal static class ReliabilityProbe
{
    /// <summary>先定位实际构建再启动测试，不初始化真实机器。</summary>
    [STAThread]
    private static int Main(string[] args)
    {
        AppDomain.CurrentDomain.AssemblyResolve += (sender, request) =>
        {
            string name = new AssemblyName(request.Name).Name;
            foreach (string ext in new[] { ".dll", ".exe" })
            {
                string file = Path.Combine(args[0], name + ext);
                if (File.Exists(file)) return Assembly.LoadFrom(file);
            }
            return null;
        };
        return ReliabilityCases.Run(args[1]);
    }
}

internal static class ReliabilityCases
{
    private static int passed;

    /// <summary>运行真正的 TCP 栈与协议适配器，外部通信只绑定回环地址。</summary>
    internal static int Run(string root)
    {
        Environment.CurrentDirectory = root; SysPara.LanguageDataDirectory = Path.Combine(root, "LanguageData");
        try
        {
            Check("TCP opens share one socket and explicit reconnect works", TcpOpen);
            Check("raw reads preserve actual byte counts and complete lines survive fragmentation", TcpFrames);
            Check("TCP cancellation revokes queued work and does not reconnect", TcpStop);
            Check("queue timeouts leave their owner alive", TcpQueue);
            Check("3D sends one program command and one measurement trigger", KeyenceSingle);
            Check("3D missing, negative and incorrect acknowledgements cannot trigger measurement", KeyenceAck);
            Check("3D numeric zero is data; malformed and nonfinite values fail", KeyenceValues);
            Check("3D timeout never resends a trigger", KeyenceTimeout);
            Check("3D stop interrupts a live transaction", KeyenceStop);
            Check("3D stale values remain on their old connection", KeyenceStale);
            Console.WriteLine("RESULT transport reliability: " + passed + "/10 passed; loopback only."); return 0;
        }
        catch(Exception error) { Console.Error.WriteLine(error); return 1; }
    }

    /// <summary>执行独立场景并报告。</summary>
    private static void Check(string name, Action action) { action(); passed++; Console.WriteLine("PASS " + name); }

    /// <summary>检查断开和重复打开不会泄漏旧端点。</summary>
    private static void TcpOpen()
    {
        using(var server = Reply("OK\r\n"))
        using(var tcp = new TCPCLient())
        {
            var tasks = Enumerable.Range(0, 8).Select(index => tcp.ConnectAsync("127.0.0.1", server.Port)).ToArray(); Task.WaitAll(tasks);
            Assert(tasks.All(task => task.Result.Succeeded) && tcp.ConnectStatus(), "Open failed");
            Assert(SpinWait.SpinUntil(() => server.Connections == 1, 1500), "Duplicate socket");
            tcp.Disconnect(); Assert(!tcp.ConnectStatus(), "Disconnect not reflected");
            Assert(tcp.Reconnect("127.0.0.1", server.Port), "Reconnect failed");
        }
    }

    /// <summary>实际收包长度和行边界独立于网络分包方式。</summary>
    private static void TcpFrames()
    {
        using(var server = new ScannerCases.Server(async (reader, stream, number, token) =>
        {
            Assert(await reader.ReadLineAsync() == "READ", "Missing command");
            await Write(stream,"A",token); await Task.Delay(15,token); await Write(stream,"B\r\nNEXT\n",token);
            while(await reader.ReadLineAsync() != null) { }
        }))
        using(var tcp = new TCPCLient())
        {
            Assert(tcp.Connect("127.0.0.1",server.Port) && tcp.Sent("READ\r\n"),"Send failed");
            Assert(tcp.ReceiveLineAsync().Result.Value == "AB" && tcp.ReceiveLineAsync().Result.Value == "NEXT","Partial frame accepted");
        }
        using(var server = Reply("XYZ"))
        using(var tcp = new TCPCLient())
        {
            tcp.Connect("127.0.0.1",server.Port); tcp.Sent("READ\n");
            Assert(Encoding.UTF8.GetString(tcp.ReceiveAsync().Result.Value) == "XYZ","Raw count padded or truncated");
        }
    }

    /// <summary>停止关闭真正挂起的读取，并撤销旧队列。</summary>
    private static void TcpStop()
    {
        var entered = Signal();
        using(var server = new ScannerCases.Server(async (reader,stream,number,token) => { Assert(await reader.ReadLineAsync() == "READ","Missing command"); entered.TrySetResult(true); while(await reader.ReadLineAsync()!=null) { } }))
        using(var tcp = new TCPCLient())
        {
            tcp.Connect("127.0.0.1",server.Port); tcp.Sent("READ\n"); var read = tcp.ReceiveLineAsync(10000); Wait(entered.Task);
            var queued = tcp.SendAsync(Encoding.ASCII.GetBytes("STALE\n"),10000); Wait(tcp.DisconnectAsync()); Wait(read); Wait(queued);
            Assert(read.Result.Status == CommunicationStatus.Cancelled && queued.Result.Status == CommunicationStatus.Cancelled && !tcp.ConnectStatus(),"Old work survived stop");
            Assert(tcp.ReceiveAsync().Result.Status == CommunicationStatus.NotConnected,"Automatic reconnect");
        }
    }

    /// <summary>排队请求超时不会中断占有连接的事务。</summary>
    private static void TcpQueue()
    {
        var entered = Signal();
        using(var server = new ScannerCases.Server(async(reader,stream,number,token)=>{ Assert(await reader.ReadLineAsync()=="READ","Missing command");entered.TrySetResult(true);await Task.Delay(150,token);await Write(stream,"OWNER\n",token);while(await reader.ReadLineAsync()!=null) { } }))
        using(var tcp = new TCPCLient())
        {
            tcp.Connect("127.0.0.1",server.Port);tcp.Sent("READ\n");var owner=tcp.ReceiveLineAsync();Wait(entered.Task);
            Assert(tcp.ReceiveLineAsync(20).Result.Status==CommunicationStatus.Timeout,"Queue timeout ignored");
            Assert(owner.Result.Value=="OWNER" && tcp.ConnectStatus(),"Owner killed by another timeout");
        }
    }

    /// <summary>每个位置映射一次零基 EXW，确认后才发送一次 T1。</summary>
    private static void KeyenceSingle()
    {
        int switches=0,triggers=0;
        using(var server=new ScannerCases.Server(async(reader,stream,number,token)=>
        {
            string command;
            while((command=await reader.ReadLineAsync())!=null)
            {
                if(command=="EXW,2") { Interlocked.Increment(ref switches); await Write(stream,"ACK2\r\n",token); }
                else if(command=="T1") {Interlocked.Increment(ref triggers);await Write(stream,"T1+1.25\r\n",token);}
                else throw new InvalidOperationException("Unexpected command: "+command);
            }
        }))
        {
            var device=Device(server);device.ProgramAck="ACK{position}";device.Open();
            Assert(device.TriggerAsync(3).Result.Value=="+1.25" && switches==1 && triggers==1,"Duplicate or unordered trigger"); device.Close();
        }
    }

    /// <summary>没有现场确认格式或收到错误应答时，程序不猜测切换成功。</summary>
    private static void KeyenceAck()
    {
        foreach(string acknowledgement in new[]{"", "WRONG\n", "ER,EXW\n"})
        {
            int triggers=0;
            using(var server=new ScannerCases.Server(async(reader,stream,number,token)=>
            {
                string command;
                while((command=await reader.ReadLineAsync())!=null) { if(command=="T1")Interlocked.Increment(ref triggers);else await Write(stream,acknowledgement,token); }
            }))
            {
                var device=Device(server);device.Open();if(acknowledgement!="")device.ProgramAck="ACK";
                var result=device.TriggerAsync(1).Result;
                Assert(!result.Succeeded && result.Value==null && triggers==0,"Unconfirmed program triggered measurement");device.Close();
            }
        }
    }

    /// <summary>高度只接受一个有限数值，不把非数值或多个字段合并。</summary>
    private static void KeyenceValues()
    {
        foreach(string frame in new[]{"T10\n","T1NaN\n","T1Infinity\n","T11,2\n","ER,T1\n"})
        using(var server=Reply(frame))
        {
            var device=Device(server);device.Open();var result=device.TriggerAsync(0).Result;
            Assert(frame=="T10\n" ? result.Succeeded && result.Value=="0" : !result.Succeeded && result.Value==null,"Invalid height: "+frame);device.Close();
        }
    }

    /// <summary>无应答时只触发一次，失败后必须显式重连。</summary>
    private static void KeyenceTimeout()
    {
        int triggers=0;
        using(var server=new ScannerCases.Server(async(reader,stream,number,token)=>{string command;while((command=await reader.ReadLineAsync())!=null){if(command=="T1")Interlocked.Increment(ref triggers);}}))
        {
            var device=Device(server);device.TimeOut=100;device.Open();var result=device.TriggerAsync(0).Result;
            Assert(result.Status==CommunicationStatus.Timeout && result.CommandMayHaveExecuted && triggers==1,"Trigger replayed or timeout hidden");
            Assert(device.TriggerAsync(0).Result.Status==CommunicationStatus.NotConnected,"Auto-reconnect after failure");device.Close();
        }
    }

    /// <summary>取消发生在控制器确认收到了触发之后，验证实际网络等待退出。</summary>
    private static void KeyenceStop()
    {
        var entered=Signal();
        using(var server=new ScannerCases.Server(async(reader,stream,number,token)=>{Assert(await reader.ReadLineAsync()=="T1","Missing trigger");entered.TrySetResult(true);while(await reader.ReadLineAsync()!=null) { }}))
        {
            var device=Device(server);device.TimeOut=10000;device.Open();var request=device.TriggerAsync(0);Wait(entered.Task);
            device.StopRunning();Wait(request);
            Assert(request.Result.Status==CommunicationStatus.Cancelled && !device.IsRunning && !device.IsConnected,"Stop did not retire transaction");device.Close();
        }
    }

    /// <summary>第二次测量不会消费第一条连接上的附加值。</summary>
    private static void KeyenceStale()
    {
        using(var server=new ScannerCases.Server(async(reader,stream,number,token)=>{Assert(await reader.ReadLineAsync()=="T1","Missing trigger");await Write(stream,"T1"+number+"\nT1999\n",token);while(await reader.ReadLineAsync()!=null) { }}))
        {
            var device=Device(server);device.Open();Assert(device.TriggerAsync(0).Result.Value=="1" && device.TriggerAsync(0).Result.Value=="2","Stale height reused");device.Close();
        }
    }

    /// <summary>创建仅访问本地服务器的真实 3D 设备对象。</summary>
    private static Keyence3DTcp Device(ScannerCases.Server server) { return new Keyence3DTcp("Offline3D"){IP="127.0.0.1",Port=server.Port,TimeOut=1000}; }

    /// <summary>为每条请求发送固定行，不使用虚拟驱动替代真实网络栈。</summary>
    private static ScannerCases.Server Reply(string response)
    {
        return new ScannerCases.Server(async(reader,stream,number,token)=>{while(await reader.ReadLineAsync()!=null)await Write(stream,response,token);});
    }

    /// <summary>按原 UTF-8 协议写入应答。</summary>
    private static Task Write(NetworkStream stream,string text,CancellationToken token) {byte[] data=Encoding.UTF8.GetBytes(text);return stream.WriteAsync(data,0,data.Length,token);}

    /// <summary>建立不会内联运行后续场景的阶段信号。</summary>
    private static TaskCompletionSource<bool> Signal(){return new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);}

    /// <summary>在有限时间内等待真正操作结束。</summary>
    private static void Wait(Task task){Assert(task.Wait(2500),"Operation did not finish");task.GetAwaiter().GetResult();}

    /// <summary>失败立即终止测试。</summary>
    private static void Assert(bool condition,string message){if(!condition)throw new InvalidOperationException(message);}
}
