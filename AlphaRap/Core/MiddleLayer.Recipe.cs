using static AlphaRap.FrameworkLanguage;
using System;
using System.IO;
using System.Windows.Forms;
using AlphaRap.Classes;
using AlphaRapLibrary;

namespace AlphaRap
{
    partial class MiddleLayer
    {
        private static bool recipeReady = true;
        /// <summary>空闲时加载模块配方、托盘列与视觉算法，保存最终选中的配方位置。</summary>
        public static bool OpenRecipe(string RecipePath)
        {
            if (SysPara.SystemMode != RunMode.IDLE || FlowProgramming.FlowPrograms.AnyRunning)
            {
                MessageBox.Show(LangMsg("MiddleLayer", "msg_RunningRecipe", "设备运行中，不能切换配方。", "Cannot change recipes while the machine is running.", "No se puede cambiar de receta mientras la máquina está funcionando."),
                    LangMsg("MiddleLayer", "msg_RecipeTitle", "切换配方", "Change recipe", "Cambiar receta"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            foreach (VisionproInterface station in VisionproInterface.VList)
            {
                if (!station.IsBusy) continue;
                MessageBox.Show(LangMsg("MiddleLayer", "msg_VisionBusy", "请先结束视觉运行或编辑：", "Finish the vision operation or editing first: ", "Finalice primero la operación o edición de visión: ") + station.StationName,
                    LangMsg("MiddleLayer", "msg_RecipeTitle", "切换配方", "Change recipe", "Cambiar receta"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (Configuration.ProjectConfiguration.HasModuleRecipeChanges)
            {
                MessageBox.Show(LangMsg("Modules", "pendingRecipe", "模块配方有未保存修改，请先通过主保存处理。", "Module recipes have unsaved changes. Use the main Save button first.", "Hay cambios sin guardar en recetas de módulos. Use Guardar principal primero."),
                    LangMsg("MiddleLayer", "msg_RecipeTitle", "切换配方", "Change recipe", "Cambiar receta"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (HardF != null && HardF.Resources != null && HardF.Resources.HasRecipeChanges())
            {
                MessageBox.Show(Configuration.HardwareText.T("pendingRecipe"),Configuration.HardwareText.T("title"));
                return false;
            }
            string oldRecipeName = SysPara.RecipeName;
            if (VPF != null && VPF.HasRecipeDraft)
            {
                MessageBox.Show(LangMsg("VPForm", "vpMsg_RecipeDraft", "视觉算法有未保存修改，请先使用主界面【保存】；选择否可放弃修改。", "Vision algorithms have unsaved changes. Use the main Save button first; choose No to discard.", "Hay algoritmos de visión sin guardar. Use Guardar principal; elija No para descartar."),
                    LangMsg("MiddleLayer", "msg_RecipeTitle", "切换配方", "Change recipe", "Cambiar receta"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            try
            {
                string path = Path.GetFullPath(RecipePath);
                string directory = Path.GetDirectoryName(path);
                string name = Path.GetFileNameWithoutExtension(path);
                Configuration.ProjectConfiguration.ChangeRecipe(path, () =>
                {
                    SysPara.RecipeDataDirectory = directory;
                    SysPara.RecipeName = name;
                    SysPara.FilePath = path;
                    recipeReady = true;
                }, new[] { PrepareRecipeSelection(directory, name) });
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message, LangMsg("Common", "msg_SystemTitle", "系统提示", "System message", "Mensaje del sistema"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return false;
            }
            RefreshRecipeTray();
            try { VPF?.OnRecipeChanged(); }
            catch (Exception error) { OperationLog.Write("ProductManagerForm", error.ToString()); }
            if (oldRecipeName != SysPara.RecipeName)
                OperationLog.Write("ProductManagerForm", "切换配方：" + oldRecipeName + " → " + SysPara.RecipeName);
            return true;
        }

        /// <summary>保留机器设置的其他键，在同一持久事务中提交配方选择并验证写入结果。</summary>
        private static Configuration.ConfigurationFile PrepareRecipeSelection(string directory, string name)
        {
            string target = Path.GetFullPath("MachineSetup.ini");
            string storedDirectory = directory.Replace(Directory.GetCurrentDirectory() + "\\", ".\\");
            return new Configuration.ConfigurationFile(target, temporary =>
            {
                if (File.Exists(target)) File.Copy(target, temporary);
                using (var ini = new IniFile(temporary))
                {
                    ini.WriteString("MachineSetup", "RecipeName", name);
                    ini.WriteString("PathSetup", "RecipeDirectory", storedDirectory);
                    if (ini.ReadString("MachineSetup", "RecipeName", "") != name || ini.ReadString("PathSetup", "RecipeDirectory", "") != storedDirectory)
                        throw new IOException(LangMsg("MiddleLayer", "msg_RecipeSelectionWrite", "无法保存配方选择。", "Cannot persist the recipe selection.", "No se puede guardar la selección de receta."));
                }
            });
        }

        /// <summary>重建随配方列变化的托盘界面，显示刷新失败不打断配方加载。</summary>
        private static void RefreshRecipeTray()
        {
            if (PlatF == null) return;
            try { PlatF.RebuildTrayTable(); }
            catch (Exception error) { System.Diagnostics.Debug.WriteLine(error); }
        }
    }
}
