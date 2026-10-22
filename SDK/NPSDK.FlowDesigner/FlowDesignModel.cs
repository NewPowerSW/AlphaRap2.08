using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace NPSDK.Design
{
    public enum FlowComparison
    {
        Always,
        IsTrue,
        IsFalse,
        Equal,
        NotEqual,
        Greater,
        Less,
        InRange,
        NotEmpty
    }

    public enum FlowUnmetAction
    {
        Wait,
        Case1,
        Case2,
        Case3,
        End
    }

    public sealed class FlowDesignDocument
    {
        public int Version = 1;
        public string Entry = "Scan";
        public string OutputDirectory = "";
        public List<FlowDesignNode> Nodes = new List<FlowDesignNode>();
    }

    public sealed class FlowDesignNode
    {
        public string Id = "";
        public string Text = "";
        public int X;
        public int Y;
        public string MethodName = "IO.Read";
        public string Target = "";
        public string Next = "";
        public string Case1 = "";
        public string Case2 = "";
        public string Case3 = "";
        public bool CanPause = true;
        public int Delay;
        public string ConditionSource = "";
        public FlowComparison Comparison;
        public string Expected = "";
        public FlowUnmetAction WhenFalse;
        public int Timeout;
        public string AlarmCode = "";
        public string AlarmContent = "";
        public List<FlowDesignParameter> Parameters = new List<FlowDesignParameter>();
    }

    public sealed class FlowDesignParameter
    {
        public string Name = "";
        public string Type = "String";
        public string Source = "";
        public string Value = "";
        /// <summary>复制参数底稿，运行与编辑不共享可变映射。</summary>
        public FlowDesignParameter Copy()
        {
            return (FlowDesignParameter)MemberwiseClone();
        }
    }

    public sealed class FlowDesignIssue : Exception
    {
        public readonly string Key;
        public readonly object[] Values;
        /// <summary>携带可翻译键与业务参数。</summary>
        public FlowDesignIssue(string key, params object[] values) : base(key + ": " + string.Join(", ", values))
        {
            Key = key;
            Values = values;
        }
    }

    public static class FlowDesignFiles
    {
        /// <summary>读取无外部实体的配置文件。</summary>
        public static FlowDesignDocument Load(string path)
        {
            using (var reader = XmlReader.Create(path, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null, MaxCharactersInDocument = 2000000 }))
                return (FlowDesignDocument)new XmlSerializer(typeof(FlowDesignDocument)).Deserialize(reader);
        }

        /// <summary>复制完整文档，防止运行时配置被编辑改变。</summary>
        public static FlowDesignDocument Copy(FlowDesignDocument document)
        {
            var serializer = new XmlSerializer(typeof(FlowDesignDocument));
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, document);
                stream.Position = 0;
                return (FlowDesignDocument)serializer.Deserialize(stream);
            }
        }

        /// <summary>原子保存画布坐标、方法、参数、判据与 SDK 跳转引用。</summary>
        public static void Save(string path, FlowDesignDocument document)
        {
            path = Path.GetFullPath(path);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            string temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write))
                {
                    new XmlSerializer(typeof(FlowDesignDocument)).Serialize(stream, document);
                    stream.Flush(true);
                }

                if (File.Exists(path))
                    File.Replace(temporary, path, path + ".bak");
                else
                    File.Move(temporary, path);
            }
            finally
            {
                if (File.Exists(temporary))
                    File.Delete(temporary);
            }
        }

        /// <summary>按声明类型解析常量，存储不受界面语言影响。</summary>
        public static object Parse(string type, string value)
        {
            if (type == "String")
                return value ?? "";
            if (type == "Boolean")
                return bool.Parse(value);
            if (type == "Int32")
                return int.Parse(value, CultureInfo.InvariantCulture);
            if (type != "Double")
                throw new FormatException(type);
            double result = double.Parse(value, CultureInfo.InvariantCulture);
            if (double.IsNaN(result) || double.IsInfinity(result))
                throw new FormatException(value);
            return result;
        }
    }
}
