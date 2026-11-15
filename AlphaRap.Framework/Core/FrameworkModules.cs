using System;
using System.Collections.Generic;
using System.Linq;
using AlphaRap.Configuration;

namespace AlphaRap
{
    /// <summary>不依赖窗口的应用模块，Id 是稳定身份，初始化不启动生产。</summary>
    public interface IFrameworkModule : IDisposable
    {
        string Id { get; }
        /// <summary>加载应用范围的数据和资源，失败由注册宿主处理。</summary>
        void Initialize(ModuleContext context);
    }

    /// <summary>参与主保存、放弃修改及 ProductManagerForm 配方切换的模块能力。</summary>
    public interface IConfigurationModule : IFrameworkModule
    {
        ModuleConfiguration Configuration { get; }
        /// <summary>只校验内存草稿，不写文件或提交版本。</summary>
        void ValidateConfiguration();
    }

    /// <summary>应用提供的配置环境，不读取窗口标题或全局路径。</summary>
    public sealed class ModuleContext
    {
        public string SettingsDirectory { get; private set; }
        /// <summary>固定本次宿主使用的机器配置目录。</summary>
        public ModuleContext(string settingsDirectory) { SettingsDirectory = System.IO.Path.GetFullPath(settingsDirectory); }
    }

    /// <summary>显式管理无窗口模块的注册、初始化及反序释放。</summary>
    public sealed class ModuleRegistry : IDisposable
    {
        private readonly List<IFrameworkModule> modules = new List<IFrameworkModule>();
        private bool disposed;
        public IReadOnlyList<IFrameworkModule> Modules { get { return modules.ToArray(); } }

        /// <summary>按能力类型查找已注册实例，不创建模块或页面。</summary>
        public T Find<T>() where T : class { return modules.OfType<T>().FirstOrDefault(); }

        /// <summary>同一实例幂等注册，重复 Id 拒绝；初始化失败不发布半成品。</summary>
        public T Register<T>(T module, ModuleContext context) where T : IFrameworkModule
        {
            if (disposed) throw new ObjectDisposedException(nameof(ModuleRegistry));
            if (module == null) throw new ArgumentNullException(nameof(module));
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (modules.Contains(module)) return module;
            if (string.IsNullOrWhiteSpace(module.Id)) throw new ArgumentException(ModuleText.InvalidId);
            if (modules.Any(item => item.Id == module.Id)) throw new InvalidOperationException(string.Format(ModuleText.DuplicateId, module.Id));
            try { module.Initialize(context); }
            catch (Exception initialization)
            {
                try { module.Dispose(); }
                catch (Exception cleanup) { throw new AggregateException(initialization, cleanup); }
                throw;
            }
            modules.Add(module);
            return module;
        }

        /// <summary>释放所有已注册模块，单个释放失败不会跳过其余模块。</summary>
        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            var errors = new List<Exception>();
            foreach (IFrameworkModule module in modules.AsEnumerable().Reverse())
                try { module.Dispose(); } catch (Exception error) { errors.Add(error); }
            modules.Clear();
            if (errors.Count > 0) throw new AggregateException(errors);
        }
    }

    internal static class ModuleText
    {
        internal static string InvalidId { get { return FrameworkLanguage.LangMsg("Modules", "invalidId", "模块标识不能为空。", "Module ID is required.", "Se requiere el ID del módulo."); } }
        internal static string DuplicateId { get { return FrameworkLanguage.LangMsg("Modules", "duplicateId", "模块标识重复：{0}", "Duplicate module ID: {0}", "ID de módulo duplicado: {0}"); } }
        internal static string InvalidStorage { get { return FrameworkLanguage.LangMsg("Modules", "invalidStorage", "模块配置名称无效。", "Invalid module configuration name.", "Nombre de configuración de módulo no válido."); } }
        internal static string AlreadyBound { get { return FrameworkLanguage.LangMsg("Modules", "alreadyBound", "页面已绑定业务模块。", "The page already has a business module.", "La página ya tiene un módulo de negocio."); } }
    }
}
