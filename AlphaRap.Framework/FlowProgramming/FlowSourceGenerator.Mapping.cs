using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using NPSDK;

namespace AlphaRap.FlowProgramming
{
    public static partial class FlowSourceGenerator
    {
        private sealed class SourceType
        {
            public string Namespace;
            public string Name;
            public string SourcePath;
        }

        /// <summary>核对项目编译项中的真实 partial Form；动态基类页只生成独立定义类。</summary>
        private static SourceType FindSourceType(FlowProgram module, string project)
        {
            if (string.IsNullOrEmpty(project)) return null;
            string fullName = (module.ModuleId ?? "").Split('/')[0];
            if (fullName == typeof(Flow_BaseForm).FullName || fullName.IndexOf('+') >= 0 || fullName.IndexOf('`') >= 0) return null;
            int separator = fullName.LastIndexOf('.');
            string space = separator < 0 ? "" : fullName.Substring(0, separator);
            string name = fullName.Substring(separator + 1);
            if (!IsIdentifier(name) || space.Split('.').Any(part => part.Length > 0 && !IsIdentifier(part))) return null;
            XmlDocument document = new XmlDocument();
            document.Load(project);
            List<string> matches = new List<string>();
            foreach (XmlElement item in document.SelectNodes("//*[local-name()='Compile' and @Include]"))
            {
                string include = item.GetAttribute("Include");
                if (include.IndexOfAny(new[] { '*', '?', '$' }) >= 0 || include.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase)) continue;
                string path = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(project), include));
                if (!File.Exists(path) || path.StartsWith(Path.Combine(Path.GetDirectoryName(project), "GeneratedFlows") + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) continue;
                string code = StripLiterals(File.ReadAllText(path));
                Match declaration = Regex.Match(code, @"\bpartial\s+class\s+@?" + Regex.Escape(name) + @"\b(?!\s*<)");
                if (!declaration.Success) continue;
                MatchCollection namespaces = Regex.Matches(code, @"\bnamespace\s+([\p{L}\p{N}_@.]+)\s*(?:\{|;)");
                bool namespaceMatches = (space.Length == 0 && namespaces.Count == 0) || (namespaces.Count == 1 && namespaces[0].Groups[1].Value.Replace("@", "") == space && namespaces[0].Index < declaration.Index);
                if (!namespaceMatches) continue;
                int expectedDepth = namespaces.Count == 1 && namespaces[0].Value.TrimEnd().EndsWith("{", StringComparison.Ordinal) ? 1 : 0;
                string prefix = code.Substring(0, declaration.Index);
                if (prefix.Count(character => character == '{') - prefix.Count(character => character == '}') == expectedDepth) matches.Add(path);
            }
            if (matches.Count == 0) return null;
            return new SourceType { Namespace = space, Name = name, SourcePath = matches.OrderBy(path => path, StringComparer.OrdinalIgnoreCase).First() };
        }

        /// <summary>移除字符串和注释再匹配类型声明，避免把注释示例当作源码归属。</summary>
        private static string StripLiterals(string code)
        {
            return Regex.Replace(code, @"@""(?:""""|[^""])*""|""(?:\\.|[^""\\])*""|'(?:\\.|[^'\\])*'|//[^\r\n]*|/\*[\s\S]*?\*/",
                match => new string(match.Value.Select(character => character == '\r' || character == '\n' ? character : ' ').ToArray()));
        }

        /// <summary>校验 C# 标识符；生成时统一使用 @ 转义保留字。</summary>
        private static bool IsIdentifier(string name)
        {
            return !string.IsNullOrEmpty(name) && Regex.IsMatch(name, @"^[\p{L}_][\p{L}\p{N}\p{Mn}\p{Mc}\p{Pc}\p{Cf}_]*$");
        }
    }
}
