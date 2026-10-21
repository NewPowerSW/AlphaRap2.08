using System;
using System.IO;
using System.Windows.Forms;
using AlphaRap.Classes;
using AlphaRapLibrary;

namespace AlphaRap
{
    partial class MiddleLayer
    {
        /// <summary>空闲时加载模块配方、托盘列与视觉算法，保存最终选中的配方位置。</summary>
        public static bool OpenRecipe(string RecipePath)
        {
            if (SysPara.SystemMode != RunMode.IDLE)
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

            bool loaded = false;
            string oldRecipeName = SysPara.RecipeName;
            try
            {
                SysPara.RecipeDataDirectory = Path.GetDirectoryName(RecipePath);
                SysPara.RecipeName = Path.GetFileNameWithoutExtension(RecipePath);
                SysPara.FilePath = Path.Combine(SysPara.RecipeDataDirectory, SysPara.RecipeName + ".xml");
                foreach (ModuleBaseForm module in ModuleManager.ModuleList) module.ReadRecipeData(SysPara.FilePath);
                if (HardF != null && !ModuleManager.ModuleList.Contains(HardF)) HardF.ReadRecipeData(SysPara.FilePath);
                RefreshRecipeTray();
                loaded = true;
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message, LangMsg("Common", "msg_SystemTitle", "系统提示", "System message", "Mensaje del sistema"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                SysPara.RecipeDataDirectory = ".\\ModuleData\\RecipeData";
                SysPara.RecipeName = "Recipe";
                SysPara.FilePath = Path.Combine(SysPara.RecipeDataDirectory, "Recipe.xml");
            }

            OpenVision();
            using (IniFile ini = new IniFile(".\\MachineSetup.ini"))
            {
                ini.WriteString("MachineSetup", "RecipeName", SysPara.RecipeName);
                ini.WriteString("PathSetup", "RecipeDirectory", SysPara.RecipeDataDirectory.Replace(Directory.GetCurrentDirectory() + "\\", ".\\"));
            }
            if (loaded && oldRecipeName != SysPara.RecipeName)
                OperationLog.Write("ProductManagerForm", "切换配方：" + oldRecipeName + " → " + SysPara.RecipeName);
            return loaded;
        }

        /// <summary>按当前配方加载所有视觉站，刷新视觉判据和页面路径。</summary>
        public static void OpenVision()
        {
            foreach (VisionproInterface station in VisionproInterface.VList)
                station.LoadTB(Path.GetFullPath(station.GetVppPath(SysPara.RecipeName)));
            if (VPF == null) return;
            VPF.Getdgv_H1_VisionDataList();
            VPF.OnRecipeChanged();
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
