using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NPSDK;
using NPSDK.Motion;

namespace AlphaRap.FlowProgramming
{
    /// <summary>外部应用拥有的资源登记；宿主只引用实例，不连接或释放外部设备。</summary>
    public static class FlowExternalResources
    {
        internal sealed class Entry
        {
            internal string Id, Name, Kind;
            internal object Instance;
            internal NP_PositionTable Points;
        }

        private static readonly Dictionary<string, Entry> entries = new Dictionary<string, Entry>(StringComparer.Ordinal);
        private static readonly object sync = new object();

        /// <summary>登记已创建的真实对象；稳定身份可跨显示改名保持，轴可同时提供原点位表。</summary>
        public static void Register(string id, string name, string kind, object instance, NP_PositionTable points = null)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("id");
            if (instance == null) throw new ArgumentNullException("instance");
            RequireRegistrationIdle();
            if (!new[] { "axis", "input", "output", "cylinder", "vision", "device", "data" }.Contains(kind)) throw new ArgumentException("kind");
            bool valid = kind == "axis" ? instance is NP_Motor : kind == "input" ? instance is NP_Input
                : kind == "output" ? instance is NP_Output : kind == "cylinder" ? instance is NP_Cylinder : instance is IFlowObject;
            if (!valid) throw new ArgumentException("The instance does not support resource kind: " + kind);
            if (points != null && kind != "axis") throw new ArgumentException("Only axis resources can bind a point table.");
            lock (sync)
            {
                if (entries.ContainsKey(id)) throw new InvalidOperationException(string.Format(FlowResourceText.T("externalDuplicate"), id));
                entries.Add(id, new Entry { Id = "external/" + Uri.EscapeDataString(id), Name = name ?? id, Kind = kind, Instance = instance, Points = points });
            }
        }

        /// <summary>重新创建外部对象后替换同一登记，下一次运行捕获当前实例；运行中禁止替换。</summary>
        public static void Replace(string id, object instance, NP_PositionTable points = null)
        {
            RequireRegistrationIdle();
            lock (sync)
            {
                Entry previous;
                if (!entries.TryGetValue(id, out previous)) throw new KeyNotFoundException(id);
                if (!FlowResourceGate.CanAccess(previous.Instance)) throw new InvalidOperationException(FlowText.T("busy"));
                entries.Remove(id);
                try { Register(id, previous.Name, previous.Kind, instance, points); }
                catch { entries.Add(id, previous); throw; }
            }
        }

        /// <summary>注销前检查全部节点依赖；应用继续拥有对象的释放职责。</summary>
        public static void Unregister(string id)
        {
            RequireRegistrationIdle();
            lock (sync)
            {
                Entry entry;
                if (!entries.TryGetValue(id, out entry)) return;
                FlowPrograms.RequireResourceUnused(entry.Id);
                if (!FlowResourceGate.CanAccess(entry.Instance)) throw new InvalidOperationException(FlowText.T("busy"));
                entries.Remove(id);
            }
        }

        /// <summary>返回登记快照；已释放的 UI 对象不再作为可用资源。</summary>
        internal static Entry[] Capture()
        {
            lock (sync) return entries.Values.Where(item => !(item.Instance is Control) || !((Control)item.Instance).IsDisposed).ToArray();
        }

        /// <summary>宿主初始化登记不依赖登录角色，已显示主窗体后只允许其 UI 线程修改登记。</summary>
        private static void RequireRegistrationIdle()
        {
            if (MiddleLayer.MainF != null && MiddleLayer.MainF.IsHandleCreated && MiddleLayer.MainF.InvokeRequired)
                throw new InvalidOperationException(FlowResourceText.T("registrationThread"));
            if (FlowPrograms.AnyRunning || SysPara.SystemMode != RunMode.IDLE || Flow_Module.Module_GetRunningStatus)
                throw new InvalidOperationException(FlowText.T("busy"));
        }
    }

    /// <summary>实际资源登记和引用检查的三语提示。</summary>
    internal static class FlowResourceText
    {
        internal static readonly Dictionary<string, string[]> Defaults = new Dictionary<string, string[]>
        {
            { "resourceUsed", new[] { "资源仍被以下流程节点引用：{0}", "The resource is still referenced by these flow nodes: {0}", "Estos nodos de flujo todavía usan el recurso: {0}" } },
            { "externalDuplicate", new[] { "外部资源身份已存在：{0}", "External resource identifier already exists: {0}", "Ya existe el identificador del recurso externo: {0}" } },
            { "registrationThread", new[] { "请在主界面线程登记或更换外部资源。", "Register or replace external resources on the main UI thread.", "Registre o sustituya recursos externos en el hilo de la interfaz principal." } }
        };

        /// <summary>取得固定底稿翻译；参数格式化由调用方完成。</summary>
        internal static string T(string key)
        {
            string[] words = Defaults[key];
            return UiTheme.IsDesignTime ? words[0] : MiddleLayer.LangMsg("EquipmentFlow", "resource." + key, words[0], words[1], words[2]);
        }
    }
}
