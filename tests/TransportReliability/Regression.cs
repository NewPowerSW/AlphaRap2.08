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
            Check("serial stop retains actual IO and closing work", SerialStop);
            Check("late serial open is closed again before releasing occupancy", SerialLateOpen);
            Check("serial queued timeout leaves owner alive", SerialQueue);
            Check("serial close errors block reuse until an explicit retry", SerialCloseFailure);
            Check("Modbus vectors enforce CRC station function and lengths", ModbusVectors);
            Check("resistance frames reject partial stale and nonfinite values", ResistanceFrames);
            Check("legacy serial utilities reject invalid hex and missing data", SerialUtilities);
            Check("device UI remains responsive and closes active IO", DeviceUiCases.Responsive);
            Check("disposed UI retains actual IO and rejects late delivery", DeviceUiCases.Disposal);
            Check("device cancellation cleanup retains global occupancy", DeviceUiCases.CancellationCleanup);
            Check("busy modules and stop guards reject late device commands", DeviceUiCases.BusyAndStopGuard);
            Check("automatic connections on one page do not drop devices", DeviceUiCases.ParallelAutoOpen);
            Check("machine stop withdraws pending UI delivery", DeviceUiCases.LatePresentation);
            Check("stop retains cleanup work without cancelling it", DeviceUiCases.CleanupSurvivesStop);
            Console.WriteLine("RESULT transport reliability: " + passed + "/24 passed; loopback, fake serial and offline UI only."); return 0;
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

    /// <summary>创建真实的串口协调器，IO 动作用离线阻塞函数替代。</summary>
    private static object SerialCore()
    {
        return Activator.CreateInstance(typeof(Keyence3DTcp).Assembly.GetType("AlphaRap.SerialOperation",true),true);
    }

    /// <summary>通过实际协调器执行可控 IO，不连接任何物理串口。</summary>
    private static Task<CommunicationResult<int>> SerialRun(object core,Func<CancellationToken,int> action,Action close,int timeout,CancellationToken token,bool writes=false)
    {
        var method=core.GetType().GetMethod("RunAsync",BindingFlags.Instance|BindingFlags.NonPublic).MakeGenericMethod(typeof(int));
        return (Task<CommunicationResult<int>>)method.Invoke(core,new object[]{action,close,timeout,token,writes});
    }

    /// <summary>调用实际关闭协调入口。</summary>
    private static Task SerialStopTask(object core,Action close)
    {
        return (Task)core.GetType().GetMethod("StopAsync",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(core,new object[]{close});
    }

    /// <summary>读取包含驱动及清理的真实占用。</summary>
    private static bool SerialBusy(object core){return (bool)core.GetType().GetProperty("IsBusy",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(core);}

    /// <summary>IO 返回后关闭仍阻塞时，停止和新操作都不能提前完成。</summary>
    private static void SerialStop()
    {
        object core=SerialCore();
        using(var entered=new ManualResetEventSlim())
        using(var readRelease=new ManualResetEventSlim())
        using(var closeEntered=new ManualResetEventSlim())
        using(var closeRelease=new ManualResetEventSlim())
        {
            Action close=()=>{readRelease.Set();closeEntered.Set();closeRelease.Wait();};
            var active=SerialRun(core,token=>{entered.Set();readRelease.Wait();return 42;},close,5000,CancellationToken.None,true);
            Assert(entered.Wait(1500),"Serial IO did not enter");int count=0;
            var queued=SerialRun(core,token=>Interlocked.Increment(ref count),()=>{},5000,CancellationToken.None);
            Task stop=SerialStopTask(core,close);Assert(closeEntered.Wait(1500),"Close not dispatched");
            Assert(SerialBusy(core) && !stop.Wait(30) && !active.IsCompleted,"Serial occupancy released before close");
            closeRelease.Set();Wait(stop);Wait(active);Wait(queued);
            Assert(active.Result.Status==CommunicationStatus.Cancelled && active.Result.CommandMayHaveExecuted && queued.Result.Status==CommunicationStatus.Cancelled && count==0 && !SerialBusy(core),"Serial old work survived stop");
        }
    }

    /// <summary>原生 Open 忽略取消并迟到返回时，必须再次关闭新打开的句柄。</summary>
    private static void SerialLateOpen()
    {
        object core=SerialCore();int opened=0,closed=0;
        using(var entered=new ManualResetEventSlim())
        using(var release=new ManualResetEventSlim())
        using(var earlyClose=new ManualResetEventSlim())
        using(var cancel=new CancellationTokenSource())
        {
            Action close=()=>{Interlocked.Exchange(ref opened,0);Interlocked.Increment(ref closed);earlyClose.Set();};
            var task=SerialRun(core,token=>{entered.Set();release.Wait();Interlocked.Exchange(ref opened,1);return 1;},close,5000,cancel.Token);
            Assert(entered.Wait(1500),"Open did not start");cancel.Cancel();Assert(earlyClose.Wait(1500),"Early close missing");
            Assert(SerialBusy(core) && !task.IsCompleted,"Blocked native Open was hidden");
            release.Set();Wait(task);Assert(task.Result.Status==CommunicationStatus.Cancelled && opened==0 && closed>=2 && !SerialBusy(core),"Late Open left a live port");
        }
    }

    /// <summary>串口队列超时不关闭正在使用端口的任务。</summary>
    private static void SerialQueue()
    {
        object core=SerialCore();int closed=0;
        using(var entered=new ManualResetEventSlim())
        using(var release=new ManualResetEventSlim())
        {
            var owner=SerialRun(core,token=>{entered.Set();release.Wait();return 42;},()=>Interlocked.Increment(ref closed),5000,CancellationToken.None);
            Assert(entered.Wait(1500),"Serial owner not started");
            var queued=SerialRun(core,token=>0,()=>Interlocked.Increment(ref closed),20,CancellationToken.None);
            Wait(queued);Assert(queued.Result.Status==CommunicationStatus.Timeout && closed==0 && !owner.IsCompleted,"Queue timeout closed owner");
            release.Set();Wait(owner);Assert(owner.Result.Succeeded && owner.Result.Value==42,"Owner failed");
        }
    }

    /// <summary>关闭失败后保留故障占用，显式成功重试才允许新操作。</summary>
    private static void SerialCloseFailure()
    {
        object core=SerialCore();
        var failed=SerialRun(core,token=>{throw new InvalidDataException("Bad frame");},()=>{throw new IOException("Close failed");},1000,CancellationToken.None);
        bool threw=false;try{failed.GetAwaiter().GetResult();}catch(IOException){threw=true;}
        Assert(threw && SerialBusy(core),"Cleanup error hidden");
        var next=SerialRun(core,token=>1,()=>{},1000,CancellationToken.None);Wait(next);
        Assert(next.Result.Status==CommunicationStatus.TransportError,"Failed port reused");
        Wait(SerialStopTask(core,()=>{}));var retry=SerialRun(core,token=>7,()=>{},1000,CancellationToken.None);Wait(retry);
        Assert(retry.Result.Value==7 && !SerialBusy(core),"Explicit cleanup retry failed");
    }

    /// <summary>使用固定已知线缆帧向量校验 CRC 顺序和数据边界。</summary>
    private static void ModbusVectors()
    {
        Type protocol=typeof(Keyence3DTcp).Assembly.GetType("AlphaRap.ModbusRtuFrame",true);
        var request=(byte[])protocol.GetMethod("Request",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{(byte)1,(ushort)0,(ushort)1});
        Assert(request.SequenceEqual(new byte[]{1,3,0,0,0,1,0x84,0x0A}),"Wrong request CRC order");
        var decode=protocol.GetMethod("Decode",BindingFlags.Static|BindingFlags.NonPublic);
        byte[] valid={1,3,2,0,1,0x79,0x84};
        Assert(((byte[])decode.Invoke(null,new object[]{valid,(byte)1,(ushort)1})).SequenceEqual(new byte[]{0,1}),"Valid response rejected");
        foreach(byte[] bad in new[]{new byte[]{1,3,2,0,1,0,0},new byte[]{2,3,2,0,1,0x79,0x84},new byte[]{1,3,2,0},new byte[]{1,3,2,0,1,0x79,0x84,0}})
        {
            bool rejected=false;try{decode.Invoke(null,new object[]{bad,(byte)1,(ushort)1});}catch(TargetInvocationException error){rejected=error.InnerException is InvalidDataException;}
            Assert(rejected,"Bad Modbus frame accepted");
        }
    }

    /// <summary>电阻计数据先收齐，再校验全部通道，旧端口或配置缓存不算新结果。</summary>
    private static void ResistanceFrames()
    {
        var meter=new HIOKI_RM3545("OfflineMeter"){ChannelNum=2};
        var capture=typeof(HIOKI_RM3545).GetMethod("CaptureText",BindingFlags.Instance|BindingFlags.NonPublic);
        var reset=typeof(HIOKI_RM3545).GetMethod("ResetResult",BindingFlags.Instance|BindingFlags.NonPublic);
        bool valid=true;meter.strResult="9,9";Assert(meter.GetFinalResult(ref valid).Count==0 && !valid,"Saved stale result accepted");
        capture.Invoke(meter,new object[]{meter.MyCom,"1,",0L});Assert(meter.GetFinalResult(ref valid).Count==0 && !valid,"Partial resistance accepted");
        capture.Invoke(meter,new object[]{meter.MyCom,"2\r\n",0L});Assert(meter.GetFinalResult(ref valid).SequenceEqual(new[]{1.0,2.0}) && valid,"Complete resistance failed");
        using(var old=new System.IO.Ports.SerialPort())capture.Invoke(meter,new object[]{old,"8,8\n",0L});
        Assert(meter.GetFinalResult(ref valid).SequenceEqual(new[]{1.0,2.0}),"Old port replaced result");
        reset.Invoke(meter,null);
        capture.Invoke(meter,new object[]{meter.MyCom,"8,8\n",0L});Assert(meter.GetFinalResult(ref valid).Count==0 && !valid,"Old epoch republished resistance");
        capture.Invoke(meter,new object[]{meter.MyCom,"NaN,2\n",1L});Assert(meter.GetFinalResult(ref valid).Count==0 && !valid,"NaN resistance accepted");
        meter.Close();meter.MyCom.Dispose();
    }

    /// <summary>旧工具的无效配置和半字节不访问实际 COM，FFU 无数据不伪装成零风速。</summary>
    private static void SerialUtilities()
    {
        using(var port=new AlphaRap.Classes.NPSerialPort())
        {
            Assert(!port.Connect("unused",9600,0,0,1) && port.LastStatus==CommunicationStatus.InvalidConfiguration,"Invalid serial config used");
            Assert(port.strToHexByte("00 FF A1").SequenceEqual(new byte[]{0,255,161}),"Hex conversion failed");
            Assert(!port.Write_StringToHex("ABC") && !port.Write_StringToHex("GG"),"Invalid hex sent");
            Assert(port.readChar()==null && port.LastStatus==CommunicationStatus.NotConnected,"Closed port invented data");
        }
        var ffu=new AlphaRap.Classes.FFUCom("OfflineFFU");bool failed=false;
        try{ffu.GetFFUSpeed();}catch(DeviceCommunicationException){failed=true;}
        Assert(failed,"Missing FFU response became zero speed");ffu.Close();
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
