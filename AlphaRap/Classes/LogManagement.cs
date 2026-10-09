using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class LogManagement
{
    //===================== Only need to modify this part ==========================
    //step1: Creat a new log type on the enumeration
    //step2: Add a title on the constructor (optional)

    public enum LogType
    {
        Alarm,
        Operation,
        MachineStatus,
        Production,
    }

    private LogManagement()
    {
        LogTitle.Add(LogType.Alarm, "Alarm Type,Alarm Code,Alarm Message");
        LogTitle.Add(LogType.Operation, "Action");
        LogTitle.Add(LogType.MachineStatus, "Status");
        LogTitle.Add(LogType.Production, "Production Data");

        foreach (var value in Enum.GetValues(typeof(LogType)))
        {
            Queue<LogData> queLogData = new Queue<LogData>();
            LogDataTemp.Add((LogType)value, queLogData);
        }
        Task.Factory.StartNew(ExecuteWriteLogToFile);
    }
    //==============================================================================

    public static LogManagement Instance = new LogManagement();
    private string SaveDirectory = System.IO.Directory.GetCurrentDirectory();
    private Dictionary<LogType, string> LogTitle = new Dictionary<LogType, string>();
    private Dictionary<LogType, Queue<LogData>> LogDataTemp = new Dictionary<LogType, Queue<LogData>>();
    private class LogData
    {
        public DateTime dateTime;
        public string logMessage;
    }

    public void SaveLog(LogType logType, string message)
    {
        LogData logData = new LogData();
        logData.dateTime = DateTime.Now;
        logData.logMessage = message;
        lock (LogDataTemp) { LogDataTemp[logType].Enqueue(logData); }
    }

    private void ExecuteWriteLogToFile()
    {
        while (true)
        {
            Thread.Sleep(1000);
            foreach (KeyValuePair<LogType, Queue<LogData>> queLogData in LogDataTemp)
                if (queLogData.Value.Count > 0)
                    WriteLogToFile(queLogData.Key, queLogData.Value);
        }
    }

    private void WriteLogToFile(LogType logType, Queue<LogData> queLogData)
    {
        LogData firstLogData = queLogData.Peek();
        string SavePath = $"{SaveDirectory}\\LogData\\{firstLogData.dateTime.ToString("yyyy")}\\{logType}\\{firstLogData.dateTime.ToString("yyyy_MM_dd")}.csv";
        if (!Directory.Exists(Path.GetDirectoryName(SavePath)))
            Directory.CreateDirectory(Path.GetDirectoryName(SavePath));

        //Write title
        if (!File.Exists(SavePath))
            if (LogTitle.ContainsKey(logType))
                File.AppendAllText(SavePath, $"Date Time,{LogTitle[logType]}{Environment.NewLine}", Encoding.UTF8);
            else
                File.AppendAllText(SavePath, $"Date Time,Log Data{Environment.NewLine}", Encoding.UTF8);

        //Write message
        try
        {
            using (FileStream fileStream = new FileStream(SavePath, FileMode.Append, FileAccess.Write, FileShare.None))
            {
                using (StreamWriter streamWriter = new StreamWriter(fileStream, Encoding.UTF8))
                {
                    string firstDate = firstLogData.dateTime.ToString("yyyyMMdd");
                    lock (LogDataTemp)
                    {
                        while ((queLogData.Count > 0) && (firstDate == queLogData.Peek().dateTime.ToString("yyyyMMdd")))
                        {
                            LogData logData = queLogData.Dequeue();
                            streamWriter.Write($"{logData.dateTime.ToString("yyyy/MM/dd HH:mm:ss")},{logData.logMessage}{Environment.NewLine}");
                            streamWriter.Flush();
                        }
                    }
                    streamWriter.Close();
                }
            }
        }
        catch (Exception) { }
    }
}
