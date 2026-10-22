using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace AlphaRap.FlowRuntime
{
    public enum ValueKind { Text, Number, Integer, Boolean }
    public enum DeviceKind { Input, Module, Vision, Output }

    public sealed class FlowDefinition
    {
        public int SchemaVersion = 1;
        public string Version = "1";
        public string StartNode = "Start";
        public bool RequireLocalSave = true;
        public List<FlowNode> Nodes = new List<FlowNode>();
        public List<SimDevice> Devices = new List<SimDevice>();
    }

    public sealed class FlowNode
    {
        public string Id = "";
        public string Title = "";
        public string ActionId = "";
        public string TargetId = "";
        public string Next = "";
        public string FalseNext = "";
        public int TimeoutMs = 3000;
        public string AlarmCode = "SIM_ACTION";
        public List<InputBinding> Inputs = new List<InputBinding>();
    }

    public sealed class InputBinding
    {
        public string Name = "";
        public string Source = "";
        public string Value = "";
    }

    public sealed class SimDevice
    {
        public string Id { get; set; }
        public DeviceKind Kind { get; set; }
        public bool State { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Angle { get; set; }
        public int DelayMs { get; set; }
    }

    public static class FlowFiles
    {
        /// <summary>读取受限 XML 流程；不解析外部实体。</summary>
        public static FlowDefinition Load(string path)
        {
            var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null, MaxCharactersInDocument = 2000000 };
            using (var reader = XmlReader.Create(path, settings))
                return (FlowDefinition)new XmlSerializer(typeof(FlowDefinition)).Deserialize(reader);
        }

        /// <summary>先写临时文件，再原子替换流程文件。</summary>
        public static void Save(string path, FlowDefinition definition)
        {
            string full = Path.GetFullPath(path);
            Directory.CreateDirectory(Path.GetDirectoryName(full));
            string temporary = full + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write))
                {
                    new XmlSerializer(typeof(FlowDefinition)).Serialize(stream, definition);
                    stream.Flush(true);
                }
                if (File.Exists(full)) File.Replace(temporary, full, null);
                else File.Move(temporary, full);
            }
            finally { if (File.Exists(temporary)) File.Delete(temporary); }
        }

        /// <summary>复制配置，隔离运行快照与编辑中的对象。</summary>
        public static FlowDefinition Snapshot(FlowDefinition definition)
        {
            var serializer = new XmlSerializer(typeof(FlowDefinition));
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, definition);
                stream.Position = 0;
                return (FlowDefinition)serializer.Deserialize(stream);
            }
        }

        /// <summary>将界面底稿转换为确定的协议值，数值不受当前显示语言影响。</summary>
        public static object Parse(string value, ValueKind kind)
        {
            if (kind == ValueKind.Text) return value ?? "";
            if (kind == ValueKind.Boolean) return bool.Parse(value);
            if (kind == ValueKind.Integer) return int.Parse(value, CultureInfo.InvariantCulture);
            double number = double.Parse(value, CultureInfo.InvariantCulture);
            if (double.IsNaN(number) || double.IsInfinity(number)) throw new FormatException("Finite number required");
            return number;
        }
    }
}
