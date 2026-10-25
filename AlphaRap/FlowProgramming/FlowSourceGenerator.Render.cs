using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using NPSDK;

namespace AlphaRap.FlowProgramming
{
    public static partial class FlowSourceGenerator
    {
        /// <summary>生成同窗体 partial 或独立定义类，节点方法逐块重建同一模型。</summary>
        private static string Render(FlowProgram program, string project)
        {
            SourceType owner = FindSourceType(program, project);
            string suffix = Hash(Encoding.UTF8.GetBytes(program.ModuleId ?? "")).Substring(0, 16);
            string space = owner == null ? "AlphaRap.GeneratedFlows" : owner.Namespace;
            string name = owner == null ? "FlowDefinition_" + suffix : owner.Name;
            StringBuilder output = new StringBuilder();
            output.AppendLine(Header);
            output.AppendLine("// Main Save owns this file. Place handwritten changes in a separate Custom.cs or code module.");
            output.AppendLine("// Include GeneratedFlows sources in the host project and rebuild before use; the existing flow runtime schedules execution.");
            output.AppendLine("// Module: " + Comment(program.ModuleId));
            output.AppendLine("// Linked source: " + (owner == null ? "standalone provider; no source Form mapping" : Comment(owner.Namespace + "." + owner.Name)));
            if (owner != null) output.AppendLine("// Source file: " + Comment(Uri.UnescapeDataString(new Uri(Path.GetDirectoryName(Path.GetFullPath(project)) + Path.DirectorySeparatorChar).MakeRelativeUri(new Uri(owner.SourcePath)).ToString())));
            output.AppendLine();
            if (space.Length > 0) { output.AppendLine("namespace " + Qualified(space)); output.AppendLine("{"); }
            output.AppendLine("    " + (owner == null ? "internal static class " : "partial class ") + "@" + name);
            output.AppendLine("    {");
            output.AppendLine("        /// <summary>Build the saved flow definition for " + Summary(program.ModuleId) + "; does not run devices.</summary>");
            output.AppendLine("        public static global::NPSDK.FlowProgram CreateGeneratedFlowDefinition_" + suffix + "()");
            output.AppendLine("        {");
            output.AppendLine("            global::NPSDK.FlowProgram flow = new global::NPSDK.FlowProgram();");
            int sequence = 0;
            WriteFields(output, "flow", program, ref sequence, "Steps");
            output.AppendLine("            flow.Steps.Clear();");
            Dictionary<string, string> methodNames = new Dictionary<string, string>();
            foreach (FlowStep step in program.Steps)
            {
                string method = NodeMethod(step, program.ModuleId);
                if (methodNames.Values.Contains(method)) throw new InvalidOperationException("Duplicate generated node identity: " + step.Id);
                methodNames.Add(step.Id, method);
                output.AppendLine("            flow.Steps.Add(" + method + "());");
            }
            output.AppendLine("            return flow;");
            output.AppendLine("        }");
            foreach (FlowStep step in program.Steps)
            {
                output.AppendLine();
                output.AppendLine("        /// <summary>" + Summary(step.Caption) + ". Node: " + Summary(step.Id) + ".</summary>");
                output.AppendLine("        private static global::NPSDK.FlowStep " + methodNames[step.Id] + "()");
                output.AppendLine("        {");
                output.AppendLine("            global::NPSDK.FlowStep node = new global::NPSDK.FlowStep();");
                sequence = 0;
                WriteFields(output, "node", step, ref sequence, null);
                output.AppendLine("            return node;");
                output.AppendLine("        }");
            }
            output.AppendLine("    }");
            if (space.Length > 0) output.AppendLine("}");
            return output.ToString();
        }

        /// <summary>按 SDK 公共数据字段平铺生成赋值，新增绑定字段不会被遗漏。</summary>
        private static void WriteFields(StringBuilder output, string variable, object value, ref int sequence, string skip)
        {
            foreach (FieldInfo field in value.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance).OrderBy(field => field.MetadataToken))
            {
                if (field.Name == skip || field.IsInitOnly) continue;
                string target = variable + ".@" + field.Name;
                object current = field.GetValue(value);
                IList list = current as IList;
                if (list != null)
                {
                    output.AppendLine("            " + target + ".Clear();");
                    foreach (object item in list) output.AppendLine("            " + target + ".Add(" + WriteValue(output, item, ref sequence) + ");");
                    continue;
                }
                string expression = WriteValue(output, current, ref sequence);
                output.AppendLine("            " + target + " = " + expression + ";");
            }
        }

        /// <summary>输出可编译的字面量或具名局部对象，不嵌入序列化正文或 Base64。</summary>
        private static string WriteValue(StringBuilder output, object value, ref int sequence)
        {
            if (value == null) return "null";
            if (value is string) return Literal((string)value);
            if (value is bool) return (bool)value ? "true" : "false";
            if (value is int) return ((int)value).ToString(CultureInfo.InvariantCulture);
            if (value is long) return ((long)value).ToString(CultureInfo.InvariantCulture) + "L";
            if (value is double)
            {
                double number = (double)value;
                if (double.IsNaN(number)) return "double.NaN";
                if (double.IsPositiveInfinity(number)) return "double.PositiveInfinity";
                if (double.IsNegativeInfinity(number)) return "double.NegativeInfinity";
                return number.ToString("R", CultureInfo.InvariantCulture) + "D";
            }
            if (value is float) return ((float)value).ToString("R", CultureInfo.InvariantCulture) + "F";
            if (value is decimal) return ((decimal)value).ToString(CultureInfo.InvariantCulture) + "M";
            if (value.GetType().IsEnum) return "(" + TypeName(value.GetType()) + ")" + Convert.ToInt64(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
            if (value.GetType().Namespace != "NPSDK") throw new InvalidOperationException("Unsupported flow source data: " + value.GetType().FullName);
            string variable = "value" + (++sequence).ToString(CultureInfo.InvariantCulture);
            string type = TypeName(value.GetType());
            output.AppendLine("            " + type + " " + variable + " = new " + type + "();");
            WriteFields(output, variable, value, ref sequence, null);
            return variable;
        }

        /// <summary>给节点方法使用稳定动作标识和节点摘要，改业务标题不改变身份。</summary>
        private static string NodeMethod(FlowStep step, string moduleId)
        {
            string action = string.IsNullOrEmpty(step.Action) ? step.ExecutionMode : step.Action;
            string safe = new string((action ?? "Node").Select(character => char.IsLetterOrDigit(character) || character == '_' ? character : '_').ToArray());
            if (safe.Length > 48) safe = safe.Substring(0, 48);
            return "CreateNode_" + safe + "_" + Hash(Encoding.UTF8.GetBytes((moduleId ?? "") + "\0" + (step.Id ?? ""))).Substring(0, 16);
        }

        /// <summary>生成带全局限定和逐段转义的 SDK 数据类型名。</summary>
        private static string TypeName(Type type) { return "global::" + Qualified(type.FullName.Replace('+', '.')); }

        /// <summary>逐段转义 namespace 和类型名中的 C# 保留字。</summary>
        private static string Qualified(string name) { return string.Join(".", name.Split('.').Select(part => "@" + part)); }

        /// <summary>转义引号、换行及控制字符，完整保留用户源码和业务值。</summary>
        private static string Literal(string value)
        {
            StringBuilder result = new StringBuilder("\"");
            foreach (char character in value)
            {
                switch (character)
                {
                    case '\\': result.Append("\\\\"); break;
                    case '"': result.Append("\\\""); break;
                    case '\r': result.Append("\\r"); break;
                    case '\n': result.Append("\\n"); break;
                    case '\t': result.Append("\\t"); break;
                    default:
                        if (char.IsControl(character) || character == '\u2028' || character == '\u2029' || char.IsSurrogate(character)) result.Append("\\u" + ((int)character).ToString("x4", CultureInfo.InvariantCulture));
                        else result.Append(character);
                        break;
                }
            }
            return result.Append('"').ToString();
        }

        /// <summary>将业务标题用于单行注释，避免换行影响生成代码。</summary>
        private static string Comment(string value) { return new string((value ?? "").Select(character => char.IsControl(character) || character == '\u2028' || character == '\u2029' ? ' ' : character).ToArray()); }

        /// <summary>将业务标题转义为 XML 功能注释。</summary>
        private static string Summary(string value)
        {
            return Comment(value).Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
        }
    }
}
