using System;
using System.Linq;
using System.Text;
using Cognex.VisionPro;
using Cognex.VisionPro.CalibFix;
using Cognex.VisionPro.ToolBlock;

namespace AlphaRap
{
    public static partial class VpAlgorithmTemplates
    {
        /// <summary>连接实际九点模型与旋转中心工具，并输出机械坐标中心；未标定的模型不能运行旋转。</summary>
        private static void ConnectRotationPrerequisite(CogToolBlock block)
        {
            CogToolBlock rotation = (CogToolBlock)block.Tools[RotationToolName];
            AddInput(rotation, "CalibrationModel", typeof(CogCalibNPointToNPoint), null);
            Connect(rotation, "CalibrationModel", block.Tools["Calibration"], "Calibration");
            foreach (string name in new[] { "MachineCenterX", "MachineCenterY" })
            {
                AddOutput(rotation, name, typeof(double), double.NaN); AddOutput(block, name, typeof(double), double.NaN);
                Connect(block.Outputs, name, rotation, name);
            }
            SetScript(rotation, "RotationCenter.txt");
            block.UserData[TemplateKey] = "Calibration/4";
        }

        /// <summary>升级标准第三版旋转工具，保持精定位参数、采样数组和九点模型。</summary>
        private static bool TryUpgradeRotationV3(CogToolBlock original, out CogToolBlock upgraded)
        {
            upgraded = null;
            if (original.Tools.Count != 11 || original.Inputs.Count != 16 || original.Outputs.Count != 13 || original.DisabledTools.Count != 0 || !ScriptMatchesResource(original, "Calibration.txt")) return false;
            CogToolBlock rotation = original.Tools.Contains(RotationToolName) ? original.Tools[RotationToolName] as CogToolBlock : null;
            if (rotation == null || rotation.Inputs.Count != 4 || rotation.Outputs.Count != 4 || rotation.Tools.Count != 0 || rotation.DisabledTools.Count != 0) return false;
            if (rotation.DataBindings.Count != 4 || rotation.Inputs.DataBindings.Count != 0 || rotation.Outputs.DataBindings.Count != 0 || !IsLegacyRotationScript(rotation)) return false;
            if (!HasPorts(rotation.Inputs, new[] { "PixelX", "PixelY", "AnglesDegrees", "Language" }, new[] { typeof(double[]), typeof(double[]), typeof(double[]), typeof(int) })) return false;
            if (!HasPorts(rotation.Outputs, new[] { "CenterX", "CenterY", "RmsError", "Valid" }, new[] { typeof(double), typeof(double), typeof(double), typeof(bool) })) return false;
            if (!HasStockRotationConnections(original, rotation)) return false;
            if (!(original.Tools["Calibration"] is CogCalibNPointToNPointTool)) return false;
            CogToolBlock copy = (CogToolBlock)CogSerializer.DeepCopyObject(original);
            try { ConnectRotationPrerequisite(copy); ApplyDescriptions(copy); ApplyMessages(copy); upgraded = copy; return true; }
            catch { copy.Dispose(); throw; }
        }

        /// <summary>旋转输入和结果仍须连接标准端口，避免升级修改已由应用人员重新接线的范例。</summary>
        private static bool HasStockRotationConnections(CogToolBlock root, CogToolBlock rotation)
        {
            foreach (CogDataBinding binding in rotation.DataBindings)
            {
                string input = TerminalName(rotation, binding.DestinationPath, true);
                string expected = input == "Language" ? "Language" : "Rotation" + input;
                if (!ReferenceEquals(binding.Source, root.Inputs) || TerminalName(root.Inputs, binding.SourcePath, false) != expected) return false;
            }
            var outputs = root.Outputs.DataBindings.Cast<CogDataBinding>().Where(binding => ReferenceEquals(binding.Source, rotation)).ToArray();
            if (outputs.Length != 4) return false;
            foreach (CogDataBinding binding in outputs)
            {
                string output = TerminalName(rotation, binding.SourcePath, false);
                string expected = output == "Valid" ? "RotationValid" : output;
                if (TerminalName(root.Outputs, binding.DestinationPath, true) != expected) return false;
            }
            return true;
        }

        /// <summary>识别已发布第二、第三版的标准旋转脚本，不覆盖用户计算逻辑。</summary>
        private static bool IsLegacyRotationScript(CogToolBlock rotation)
        {
            if (rotation.Script == null) return false;
            string source = rotation.Script.Source.Replace("\r\n", "\n").Trim();
            using (var hash = System.Security.Cryptography.SHA256.Create())
                return BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(source))).Replace("-", "") == "83686190921E4E00DCEAD16BCD7A8658D884E0F64EA8385EF2C59F4FD027F793";
        }

        /// <summary>在算法编辑、九点重算和样本改变后撤销旋转发布，保留采样数组供查看或清空。</summary>
        internal static void InvalidateRotation(CogToolBlock block, bool samplesChanged)
        {
            if (!IsBuiltIn(block) || !block.Tools.Contains(RotationToolName)) return;
            if (samplesChanged) block.UserData["Rotation.SampleContext"] = "";
            block.UserData["Rotation.ResultContext"] = "";
            CogToolBlock rotation = block.Tools[RotationToolName] as CogToolBlock;
            if (rotation == null) return;
            if (rotation.Outputs.Contains("Valid")) rotation.Outputs["Valid"].Value = false;
            if (block.Outputs.Contains("RotationValid")) block.Outputs["RotationValid"].Value = false;
            foreach (string name in new[] { "CenterX", "CenterY", "RmsError", "MachineCenterX", "MachineCenterY" })
            {
                if (rotation.Outputs.Contains(name)) rotation.Outputs[name].Value = double.NaN;
                if (block.Outputs.Contains(name)) block.Outputs[name].Value = double.NaN;
            }
        }
    }
}
