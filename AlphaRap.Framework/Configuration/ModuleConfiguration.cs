using System;
using System.Data;
using System.IO;

namespace AlphaRap.Configuration
{
    /// <summary>无窗口的模块参数数据，稳定存储名与页面标题分离。</summary>
    public sealed class ModuleConfiguration
    {
        public string StorageName { get; private set; }
        public DataSet Settings { get; private set; }
        public DataSet Recipe { get; private set; }
        public event EventHandler SettingsLoaded;
        public Action<DataSet> ValidateRecipe { get; set; }
        public bool HasRecipeSnapshot { get; private set; }
        public bool HasRecipeDraft { get { return HasRecipeSnapshot && Recipe.HasChanges(); } }

        /// <summary>使用已有表实例，绑定、Original 版本和表事件始终引用同一数据源。</summary>
        public ModuleConfiguration(string storageName, DataSet settings, DataSet recipe)
        {
            if (string.IsNullOrWhiteSpace(storageName) || storageName == "." || storageName == ".." || storageName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                throw new ArgumentException(ModuleText.InvalidStorage);
            StorageName = storageName;
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            Recipe = recipe ?? throw new ArgumentNullException(nameof(recipe));
            foreach (DataTable table in Recipe.Tables) table.Namespace = StorageName;
        }

        /// <summary>以稳定存储名定位机器参数，不使用翻译后的窗口标题。</summary>
        public string SettingsPath(string directory) { return Path.Combine(directory ?? ".", StorageName + ".xml"); }

        /// <summary>先独立读取设置，成功后更新原表；读失败保留内存草稿。</summary>
        public void LoadSettings(string directory)
        {
            using (DataSet loaded = Settings.Clone())
            {
                if (File.Exists(SettingsPath(directory))) loaded.ReadXml(SettingsPath(directory));
                EnsureRows(loaded);
                Apply(Settings, loaded);
            }
            SettingsLoaded?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>只加载本模块命名空间，其他模块同名表及机器设置保持不变。</summary>
        public void LoadRecipe(string path)
        {
            using (RecipeChange change = PrepareRecipe(path)) change.Apply();
        }

        /// <summary>预读本模块的新配方，不改变当前数据或 Original 版本。</summary>
        public RecipeChange PrepareRecipe(string path)
        {
            return PrepareRecipeContent(File.Exists(path) ? File.ReadAllBytes(path) : null);
        }

        /// <summary>从整份切换共用的字节快照准备本模块，避免逐模块重复读取变动中的文件。</summary>
        internal RecipeChange PrepareRecipeContent(byte[] content)
        {
            DataSet previous = Recipe.Copy();
            bool previousSnapshot = HasRecipeSnapshot;
            DataSet loaded = Recipe.Clone();
            try
            {
                foreach (DataTable table in loaded.Tables) table.Namespace = StorageName;
                if (content != null) using (var source = new MemoryStream(content)) loaded.ReadXml(source, XmlReadMode.IgnoreSchema);
                EnsureRows(loaded);
                ValidateRecipe?.Invoke(loaded);
                return new RecipeChange(() => { Apply(Recipe, loaded); HasRecipeSnapshot = true; }, () => { Recipe.Clear(); Recipe.Merge(previous, false, MissingSchemaAction.Add); HasRecipeSnapshot = previousSnapshot; },
                    () => { previous.Dispose(); loaded.Dispose(); });
            }
            catch { previous.Dispose(); loaded.Dispose(); throw; }
        }

        /// <summary>主保存成功后登记当前配方基线，之后的编辑参与切换拦截。</summary>
        internal void AcceptRecipeSaved() { Recipe.AcceptChanges(); HasRecipeSnapshot = true; }

        /// <summary>普通参数表补齐默认行，显式允许为空的明细表不增加占位数据。</summary>
        private static void EnsureRows(DataSet data)
        {
            foreach (DataTable table in data.Tables)
                if (table.Rows.Count == 0 && !Equals(table.ExtendedProperties["AllowEmpty"], true)) table.Rows.Add(table.NewRow());
        }

        /// <summary>在同一表实例上替换内容，全部加载完成后发布 Original 版本。</summary>
        private static void Apply(DataSet target, DataSet loaded)
        {
            target.Clear();
            target.Merge(loaded, false, MissingSchemaAction.Add);
            target.AcceptChanges();
        }
    }
}
