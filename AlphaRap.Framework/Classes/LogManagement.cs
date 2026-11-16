using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AlphaRap;

/// <summary>兼容日志入口，写入由应用宿主观察并在关闭时排空。</summary>
public sealed class LogManagement
{
    public enum LogType { Alarm, Operation, MachineStatus, Production }
    private static readonly Lazy<LogManagement> instance = new Lazy<LogManagement>(() => new LogManagement());
    public static LogManagement Instance { get { return instance.Value; } }
    private readonly object sync = new object();
    private readonly Queue<LogData> pending = new Queue<LogData>();
    private readonly AutoResetEvent signal = new AutoResetEvent(false);
    private readonly string directory = Directory.GetCurrentDirectory();
    private Task work = Task.FromResult(0);
    private bool stopping;
    private sealed class LogData { internal DateTime Time; internal LogType Type; internal string Message; }
    /// <summary>只创建内存状态，首次写入再启动日志任务。</summary>
    private LogManagement() { }

    /// <summary>登记原始日志，有界积压在无法继续保存时明确报错。</summary>
    public void SaveLog(LogType type, string message)
    {
        lock (sync)
        {
            if (pending.Count >= 50000) throw new IOException(FrameworkLanguage.LangMsg("LogManagement", "full", "日志积压已满，请检查磁盘写入。", "The log queue is full. Check disk writes.", "La cola de registros está llena. Compruebe la escritura en disco."));
            pending.Enqueue(new LogData { Time = DateTime.Now, Type = type, Message = message });
            if (work.IsCompleted) StartWriter();
        }
        signal.Set();
    }
    /// <summary>启动唯一写入者，停止回调不阻塞调用线程。</summary>
    private void StartWriter()
    {
        stopping = false;
        work = Task.Factory.StartNew(WriteLoop, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        ApplicationTasks.Global.Track("LegacyLogs", work, () => { RequestStop(); return Task.FromResult(0); });
    }
    /// <summary>请求排空后退出，真实写入未完成前任务不会结束。</summary>
    private void RequestStop() { lock (sync) stopping = true; signal.Set(); }
    /// <summary>没有访问过旧日志入口时不额外创建后台任务。</summary>
    public static Task StopIfCreatedAsync() { return instance.IsValueCreated ? instance.Value.StopAsync() : Task.FromResult(0); }
    /// <summary>等待真实排空，先前写入失败时可在修复磁盘后重试。</summary>
    public Task StopAsync()
    {
        lock (sync)
        {
            if (work.IsCompleted && pending.Count > 0) StartWriter();
            stopping = true; signal.Set(); return work;
        }
    }
    /// <summary>同一类型同一天的日志成批落盘，成功后才从内存队列移除。</summary>
    private void WriteLoop()
    {
        while (true)
        {
            LogData[] batch;
            lock (sync)
            {
                if (pending.Count == 0 && stopping) return;
                LogData first = pending.Count == 0 ? null : pending.Peek();
                batch = first == null ? new LogData[0] : pending.TakeWhile(item => item.Type == first.Type && item.Time.Date == first.Time.Date).Take(1000).ToArray();
            }
            if (batch.Length == 0) { signal.WaitOne(1000); continue; }
            WriteBatch(batch);
            lock (sync) for (int i = 0; i < batch.Length; i++) pending.Dequeue();
        }
    }
    /// <summary>保留旧日志目录和原消息格式；写入异常由任务监督器记录。</summary>
    private void WriteBatch(LogData[] batch)
    {
        LogData first = batch[0];
        string path = Path.Combine(directory, "LogData", first.Time.ToString("yyyy"), first.Type.ToString(), first.Time.ToString("yyyy_MM_dd") + ".csv");
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        bool header = !File.Exists(path);
        using (var writer = new StreamWriter(path, true, new UTF8Encoding(true)))
        {
            if (header) writer.WriteLine("Date Time," + (first.Type == LogType.Alarm ? "Alarm Type,Alarm Code,Alarm Message" : first.Type == LogType.MachineStatus ? "Status" : first.Type == LogType.Production ? "Production Data" : "Action"));
            foreach (LogData item in batch) writer.WriteLine(item.Time.ToString("yyyy/MM/dd HH:mm:ss") + "," + item.Message);
        }
    }
}
