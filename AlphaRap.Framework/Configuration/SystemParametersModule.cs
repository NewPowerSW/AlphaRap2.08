using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace AlphaRap.Configuration
{
    /// <summary>系统参数的业务模块，可在没有 Form、句柄或消息泵时独立加载和校验。</summary>
    public sealed class SystemParametersModule : IConfigurationModule
    {
        public string Id { get { return "system.parameters"; } }
        public ModuleConfiguration Configuration { get; private set; }
        public ConstantStore Constants { get; private set; }
        private string initializedDirectory;
        private bool disposed;

        /// <summary>建立独立作用域和兼容列，只创建内存模型。</summary>
        public SystemParametersModule(IEnumerable<BooleanSettingDefault> defaults)
        {
            BooleanSettingDefault[] settings = defaults.ToArray();
            var machine = new DataSet("NewDataSet");
            var recipe = new DataSet("NewDataSet");
            DataTable marker = machine.Tables.Add("MSet");
            marker.Columns.Add("ConstantsInitialized", typeof(bool)).DefaultValue = false;
            DataTable legacy = machine.Tables.Add("PSet");
            foreach (BooleanSettingDefault setting in settings.Where(item => item.PreserveLegacyColumn)) legacy.Columns.Add(setting.Key, typeof(bool)).DefaultValue = false;
            machine.Tables.Add(ConstantStore.CreateTable());
            recipe.Tables.Add(ConstantStore.CreateTable());
            Configuration = new ModuleConfiguration("SysSetting", machine, recipe);
            Constants = new ConstantStore(Configuration, settings);
            Configuration.SettingsLoaded += InitializeDefaults;
        }

        /// <summary>同一环境初始化幂等，初始化失败不标记成功。</summary>
        public void Initialize(ModuleContext context)
        {
            if (disposed) throw new ObjectDisposedException(nameof(SystemParametersModule));
            if (initializedDirectory == context.SettingsDirectory) return;
            Configuration.LoadSettings(context.SettingsDirectory);
            initializedDirectory = context.SettingsDirectory;
        }

        /// <summary>机器设置加载完成后迁移既有开关，不重新添加用户已删除的默认项。</summary>
        private void InitializeDefaults(object sender, EventArgs args) { Constants.InitializeDefaults(); }

        /// <summary>主保存只校验两个参数作用域，不提交草稿。</summary>
        public void ValidateConfiguration() { Constants.Validate("Setting"); Constants.Validate("Recipe"); }

        /// <summary>应用关闭时释放模型订阅；关闭参数窗口不会释放运行中的模块。</summary>
        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            Configuration.SettingsLoaded -= InitializeDefaults;
            Constants.Dispose();
        }
    }
}
