using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using NPSDK.Motion;

namespace AlphaRap.Configuration
{
    public static class HardwareControlTypes
    {
        private static readonly object sync = new object();
        private static readonly Dictionary<string,Type[]> cache = new Dictionary<string,Type[]>();
        private static int assemblyVersion;
        private static int cachedVersion = -1;

        /// <summary>新程序集加载后使类型目录失效，允许后续加载的业务控件自动出现。</summary>
        static HardwareControlTypes()
        {
            AppDomain.CurrentDomain.AssemblyLoad += AssemblyLoaded;
        }

        /// <summary>列出已加载程序集内匹配资源方向的公开、可无参创建控件，不实例化或连接设备。</summary>
        public static Type[] Available(string kind)
        {
            BaseType(kind);
            lock (sync)
            {
                int version = Volatile.Read(ref assemblyVersion);
                if (cachedVersion != version) RefreshCache(version);
                return (Type[])cache[kind].Clone();
            }
        }

        /// <summary>只在程序集集合变化时扫描一次，并按轴、输入和输出分别缓存候选类型。</summary>
        private static void RefreshCache(int version)
        {
            Type[] available = AppDomain.CurrentDomain.GetAssemblies().SelectMany(LoadedTypes)
                .Where(type => type.IsVisible && !type.IsAbstract && !type.ContainsGenericParameters
                    && type.GetConstructor(Type.EmptyTypes) != null).Distinct().ToArray();
            cache.Clear();
            foreach (string kind in new[] { "Axis","Input","Output" })
            {
                Type basis = BaseType(kind);
                cache[kind] = available.Where(basis.IsAssignableFrom)
                    .OrderBy(type => type == basis ? "" : type.FullName,StringComparer.Ordinal).ToArray();
            }
            cachedVersion = version;
        }

        /// <summary>仅标记缓存失效，程序集加载事件不创建控件或扫描反射目录。</summary>
        private static void AssemblyLoaded(object sender, AssemblyLoadEventArgs args)
        {
            Interlocked.Increment(ref assemblyVersion);
        }

        /// <summary>类型身份只包含程序集简单名称及完整类型名，程序集版本升级不改变已保存绑定。</summary>
        public static string Identity(Type type)
        {
            return type.Assembly.GetName().Name + "|" + type.FullName;
        }

        /// <summary>空身份使用原生控件；缺失或方向不符的派生类型明确报错，不回退为另一驱动。</summary>
        public static Type Resolve(string kind, string identity)
        {
            if (string.IsNullOrEmpty(identity)) return BaseType(kind);
            Type[] matches = Available(kind).Where(type => Identity(type) == identity).ToArray();
            if (matches.Length == 1) return matches[0];
            throw new InvalidOperationException(string.Format(HardwareText.T("controlTypeMissing"), identity));
        }

        /// <summary>按资源方向匹配 SDK 具体父类，输入与输出不能互换。</summary>
        private static Type BaseType(string kind)
        {
            if (kind == "Axis") return typeof(NP_Motor);
            if (kind == "Input") return typeof(NP_Input);
            if (kind == "Output") return typeof(NP_Output);
            throw new ArgumentException("kind");
        }

        /// <summary>保留部分加载成功的类型，动态程序集及不可枚举程序集不参与目录。</summary>
        private static IEnumerable<Type> LoadedTypes(Assembly assembly)
        {
            if (assembly.IsDynamic) return new Type[0];
            try { return assembly.GetTypes(); }
            catch (ReflectionTypeLoadException error) { return error.Types.Where(type => type != null); }
            catch (NotSupportedException) { return new Type[0]; }
        }
    }
}
