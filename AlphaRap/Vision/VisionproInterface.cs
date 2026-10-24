using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Cognex.VisionPro;
using Cognex.VisionPro.CalibFix;
using Cognex.VisionPro.ToolBlock;

namespace AlphaRap
{
    /// <summary>一次视觉请求的执行状态；超时和取消后不再接受迟到结果。</summary>
    public enum VisionRunState { Running, Succeeded, Rejected, Faulted, TimedOut, Cancelled }

    /// <summary>单次运行的结果快照，坐标单位与本站 ToolBlock 输出一致。</summary>
    public sealed class VisionRunResult
    {
        public long RunId { get; private set; }
        public VisionRunState State { get; private set; }
        public string Error { get; private set; }
        public double X { get; private set; }
        public double Y { get; private set; }
        public double Angle { get; private set; }
        public bool IsFlowManaged { get; internal set; }

        /// <summary>记录运行编号、状态、错误和定位坐标。</summary>
        public VisionRunResult(long runId, VisionRunState state, string error = "", double x = 0, double y = 0, double angle = 0)
        {
            RunId = runId;
            State = state;
            Error = error;
            X = x;
            Y = y;
            Angle = angle;
        }
    }

    public abstract partial class VisionproInterface
    {
        public static List<VisionproInterface> VList = new List<VisionproInterface>();
        public CogToolBlock TB;
        public string TBPath;
        public bool IsLoadTBOk;
        public bool IsAccept;

        protected readonly object _operationLock = new object();
        private bool _isRunning;
        private int _editingThreadId;
        private long _nextRunId;
        private VisionRunResult _lastRunResult;
        public string LastError { get; private set; }
        /// <summary>已加载或成功保存的算法文件摘要，用于确认标定应用到的实际算法版本。</summary>
        public string ModelSignature { get; private set; }
        public event Action<VisionRunResult> RunCompleted;
        private bool _hasToolBlockDraft;
        private bool _toolBlockWasSaved;
        private long _toolBlockRevision;

        internal sealed class ToolBlockSnapshot
        {
            internal CogToolBlock ToolBlock;
            internal string Path, Signature;
            internal bool Loaded, Dirty, WasSaved;
            internal long Revision;
        }

        /// <summary>当前配方算法是否存在尚未由主保存接受的编辑。</summary>
        internal bool HasToolBlockDraft { get { lock (_operationLock) return _hasToolBlockDraft; } }

        /// <summary>内存算法的编辑版本，用于识别应用标定后是否再次修改算法。</summary>
        internal long ToolBlockRevision { get { lock (_operationLock) return _toolBlockRevision; } }

        /// <summary>复制算法草稿，供编辑取消和主保存暂存使用，不修改当前算法。</summary>
        internal ToolBlockSnapshot CaptureToolBlockSnapshot()
        {
            lock (_operationLock)
            {
                if (!CanModifyToolBlock()) throw new InvalidOperationException(MiddleLayer.LangMsg("Vision", "error_BusySave", "工位正在运行或编辑，不能保存 VPP。", "Station is running or being edited; VPP cannot be saved.", "La estación está ejecutándose o en edición; no se puede guardar el VPP."));
                return new ToolBlockSnapshot { ToolBlock = TB == null ? null : (CogToolBlock)CogSerializer.DeepCopyObject(TB), Path = TBPath,
                    Signature = ModelSignature, Loaded = IsLoadTBOk, Dirty = _hasToolBlockDraft, WasSaved = _toolBlockWasSaved, Revision = _toolBlockRevision };
            }
        }

        /// <summary>恢复进入编辑器前的内存算法，保留本站实例和此前尚未保存的草稿。</summary>
        internal void RestoreToolBlockSnapshot(ToolBlockSnapshot snapshot)
        {
            lock (_operationLock)
            {
                if (!CanModifyToolBlock()) throw new InvalidOperationException(MiddleLayer.LangMsg("Vision", "error_BusyDiscard", "工位正在运行或编辑，不能放弃修改。", "Station is running or being edited; changes cannot be discarded.", "La estación está ejecutándose o en edición; no se pueden descartar los cambios."));
                TB = snapshot.ToolBlock; TBPath = snapshot.Path; IsLoadTBOk = snapshot.Loaded;
                ModelSignature = snapshot.Signature; _hasToolBlockDraft = snapshot.Dirty;
                _toolBlockWasSaved = snapshot.WasSaved;
                _toolBlockRevision = snapshot.Revision;
                IsAccept = false; _lastRunResult = null; LastError = "";
            }
        }

        /// <summary>将编辑结果保留为当前配方草稿，等待主界面统一保存。</summary>
        internal void MarkToolBlockDraft()
        {
            lock (_operationLock)
            {
                _hasToolBlockDraft = true; ModelSignature = ""; IsAccept = false; _lastRunResult = null;
                _toolBlockRevision++;
            }
        }

        /// <summary>只把已捕获的算法写入事务暂存路径，文件摘要不发布到当前运行对象。</summary>
        internal static void WriteToolBlockSnapshot(ToolBlockSnapshot snapshot, string path)
        {
            CogSerializer.SaveObjectToFile(snapshot.ToolBlock, path);
            snapshot.Signature = ComputeFileSignature(path);
        }

        /// <summary>全部配置文件提交成功后接受算法摘要，不重新加载或替换实际算法对象。</summary>
        internal void AcceptToolBlockSnapshot(ToolBlockSnapshot snapshot)
        {
            lock (_operationLock)
            {
                TBPath = snapshot.Path; ModelSignature = snapshot.Signature; _hasToolBlockDraft = false;
                _toolBlockWasSaved = true;
                IsAccept = false; _lastRunResult = null; LastError = "";
            }
        }

        /// <summary>确认当前内存算法来自指定文件，避免配方或目录变化后继续使用旧路径的算法。</summary>
        public bool IsLoadedFrom(string path)
        {
            lock (_operationLock)
            {
                if (!IsLoadTBOk || TB == null || string.IsNullOrEmpty(TBPath)) return false;
                try { return string.Equals(Path.GetFullPath(TBPath), Path.GetFullPath(path), StringComparison.OrdinalIgnoreCase); }
                catch (ArgumentException) { return false; }
                catch (NotSupportedException) { return false; }
                catch (PathTooLongException) { return false; }
            }
        }

        /// <summary>是否仍有底层运行或编辑占用本站；超时不会提前释放正在执行的算法。</summary>
        public bool IsBusy
        {
            get { lock (_operationLock) return _isRunning || _editingThreadId != 0; }
        }

        /// <summary>最近一次请求的不可变结果快照；尚未运行时为 null。</summary>
        public VisionRunResult LastRunResult
        {
            get { lock (_operationLock) return _lastRunResult; }
        }

        public bool AutoVisionRunDisplay = true;
        public int VisionRunDisplayIndex = 0;
        public List<CogRecordDisplay> RecordDisplayList = new List<CogRecordDisplay>();
        /// <summary>实时取流用的采集通道（CogFrameGrabbers 的下标）：由相机 SN 现场解析后写入，不是配置值。</summary>
        public int RunLiveCCDIndex = 0;
        /// <summary>
        /// 实时取流用的相机绑定键（Cognex SN / UniqueID，见 <see cref="CogCameraDevices"/>）。
        /// 相机只按 SN 绑定：非空时按 SN 解析当前通道（换口 / 插拔 / 枚举顺序变化都不会绑错）；
        /// 为空表示这台相机还没选 SN，取流时直接报错而不是盲取通道。
        /// </summary>
        public string RunLiveCCDSN = "";
        public double RunExposure = 10;

        /// <summary>
        /// 界面显示名（动态相机填"相机 / VPP"）。为空时退回类名，
        /// 老的硬编码工位（H1_Vision_Fiducial 之类）什么都不用改。
        /// </summary>
        public string DisplayName;

        /// <summary>
        /// vpp 存放子目录：为空时使用类名；相机下的 VPP 为"相机名\VPP名"，每个 VPP 各占一个目录。
        /// </summary>
        public string VppFolderName;

        /// <summary>vpp 相对子目录：VppFolderName，为空时为类名。</summary>
        public string VppSubFolder
        {
            get { return string.IsNullOrEmpty(VppFolderName) ? GetType().Name : VppFolderName; }
        }

        /// <summary>界面显示名：优先 DisplayName，其次类名。</summary>
        public string StationName
        {
            get { return string.IsNullOrEmpty(DisplayName) ? GetType().Name : DisplayName; }
        }

        /// <summary>vpp 完整路径：{VisionData}\{VppFolderName}\{配方名}.vpp。</summary>
        public string GetVppPath(string recipeName)
        {
            return string.Format(@"{0}\{1}\{2}.vpp", SysPara.VisionFileDirectory, VppSubFolder, recipeName);
        }
        public VisionproInterface()
        {
            VList.Add(this);
        }

        public virtual void VisionRun() { }

        /// <summary>加载指定 VPP，失败时记录原因并使本站不可运行。</summary>
        public void LoadTB(string path)
        {
            string error;
            TryLoadTB(path, out error);
        }

        /// <summary>保存到指定路径；成功后更新本站路径。</summary>
        public void SaveTB(string path)
        {
            string error;
            TrySaveTB(path, out error);
        }

        /// <summary>重新加载当前 VPP；缺失文件不会自动新建或沿用旧算法。</summary>
        public void LoadTB()
        {
            LoadTB(TBPath);
        }

        /// <summary>放弃编辑：未保存的新算法直接撤销；磁盘重载失败时保留编辑对象供继续处理。</summary>
        public bool TryDiscardTBChanges(out string error)
        {
            lock (_operationLock)
            {
                if (!CanModifyToolBlock()) return FailOperation(MiddleLayer.LangMsg("Vision", "error_BusyDiscard", "工位正在运行或编辑，不能放弃修改。", "Station is running or being edited; changes cannot be discarded.", "La estación está ejecutándose o en edición; no se pueden descartar los cambios."), out error);
                CogToolBlock edited = TB;
                bool wasLoaded = IsLoadTBOk;
                string previousSignature = ModelSignature;
                bool wasDirty = _hasToolBlockDraft;
                bool wasSaved = _toolBlockWasSaved;
                if (!File.Exists(TBPath))
                {
                    if (_toolBlockWasSaved) return FailOperation(MiddleLayer.LangMsg("Vision", "error_DiscardMissing", "已保存的 VPP 文件不存在或不可读，算法草稿已保留：", "The saved VPP file is missing or unreadable; the algorithm draft was preserved: ", "El archivo VPP guardado no existe o no se puede leer; se conservó el borrador del algoritmo: ") + TBPath, out error);
                    TB = null;
                    IsLoadTBOk = false;
                    ModelSignature = "";
                    _hasToolBlockDraft = false;
                    IsAccept = false;
                    _lastRunResult = null;
                    LastError = error = "";
                    return true;
                }
                if (TryLoadTB(TBPath, out error)) return true;
                TB = edited;
                IsLoadTBOk = wasLoaded;
                ModelSignature = previousSignature;
                _hasToolBlockDraft = wasDirty;
                _toolBlockWasSaved = wasSaved;
                return false;
            }
        }

        /// <summary>加载生产 VPP；运行中拒绝切换，加载失败清除旧算法及旧运行结果。</summary>
        public bool TryLoadTB(string path, out string error)
        {
            lock (_operationLock)
            {
                if (!CanModifyToolBlock()) return FailOperation(MiddleLayer.LangMsg("Vision", "error_BusyLoad", "工位正在运行或编辑，不能加载 VPP。", "Station is running or being edited; VPP cannot be loaded.", "La estación está ejecutándose o en edición; no se puede cargar el VPP."), out error);
                TB = null;
                TBPath = path;
                IsLoadTBOk = false;
                ModelSignature = "";
                _hasToolBlockDraft = false;
                _toolBlockWasSaved = false;
                IsAccept = false;
                _lastRunResult = null;
                try
                {
                    if (!File.Exists(path)) return FailOperation(MiddleLayer.LangMsg("Vision", "error_FileMissing", "VPP 文件不存在：", "VPP file does not exist: ", "El archivo VPP no existe: ") + path, out error);
                    TB = CogSerializer.LoadObjectFromFile(path) as CogToolBlock;
                    if (TB == null) return FailOperation(MiddleLayer.LangMsg("Vision", "error_InvalidTB", "文件不是有效的 ToolBlock：", "File is not a valid ToolBlock: ", "El archivo no es un ToolBlock válido: ") + path, out error);
                    ModelSignature = ComputeFileSignature(path);
                    _hasToolBlockDraft = false;
                    _toolBlockWasSaved = true;
                    IsLoadTBOk = true;
                    LastError = error = "";
                    return true;
                }
                catch (Exception ex) { return FailOperation(MiddleLayer.LangMsg("Vision", "error_Load", "加载 VPP 失败：", "VPP load failed: ", "Error al cargar VPP: ") + path + "；" + ex.Message, out error); }
            }
        }

        /// <summary>显式创建供编辑的新 ToolBlock；已有文件按正常加载处理，不复制旧算法。</summary>
        public bool TryCreateTB(string path, out string error)
        {
            lock (_operationLock)
            {
                if (!CanModifyToolBlock()) return FailOperation(MiddleLayer.LangMsg("Vision", "error_BusyCreate", "工位正在运行或编辑，不能新建 VPP。", "Station is running or being edited; VPP cannot be created.", "La estación está ejecutándose o en edición; no se puede crear un VPP."), out error);
                if (File.Exists(path)) return TryLoadTB(path, out error);
                TB = new CogToolBlock();
                TBPath = path;
                IsLoadTBOk = true;
                ModelSignature = "";
                _hasToolBlockDraft = true;
                _toolBlockWasSaved = false;
                IsAccept = false;
                _lastRunResult = null;
                LastError = error = "";
                return true;
            }
        }

        /// <summary>同步保存当前 VPP，不重新加载或替换正在编辑的 ToolBlock。</summary>
        public void SaveTB()
        {
            string error;
            TrySaveTB(out error);
        }

        /// <summary>同步保存当前 VPP，返回实际写盘结果。</summary>
        public bool TrySaveTB(out string error)
        {
            return TrySaveTB(TBPath, out error);
        }

        /// <summary>先写同目录临时文件，再替换目标；写盘失败保留原文件。</summary>
        public bool TrySaveTB(string path, out string error)
        {
            lock (_operationLock)
            {
                if (!CanModifyToolBlock()) return FailOperation(MiddleLayer.LangMsg("Vision", "error_BusySave", "工位正在运行或编辑，不能保存 VPP。", "Station is running or being edited; VPP cannot be saved.", "La estación está ejecutándose o en edición; no se puede guardar el VPP."), out error);
                if (!IsLoadTBOk || TB == null) return FailOperation(MiddleLayer.LangMsg("Vision", "error_NotLoadedSave", "ToolBlock 未加载，不能保存。", "ToolBlock is not loaded; cannot save.", "ToolBlock no está cargado; no se puede guardar."), out error);
                string temp = null;
                try
                {
                    string fullPath = Path.GetFullPath(path);
                    Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                    temp = fullPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
                    CogSerializer.SaveObjectToFile(TB, temp);
                    string signature = ComputeFileSignature(temp);
                    if (File.Exists(fullPath)) File.Replace(temp, fullPath, null);
                    else File.Move(temp, fullPath);
                    TBPath = fullPath;
                    ModelSignature = signature;
                    _hasToolBlockDraft = false;
                    _toolBlockWasSaved = true;
                    IsAccept = false;
                    _lastRunResult = null;
                    LastError = error = "";
                    return true;
                }
                catch (Exception ex) { return FailOperation(MiddleLayer.LangMsg("Vision", "error_Save", "保存 VPP 失败：", "VPP save failed: ", "Error al guardar VPP: ") + path + "；" + ex.Message, out error); }
                finally { DeleteTempFile(temp); }
            }
        }

        /// <summary>判断当前线程能否修改算法；编辑期间仅编辑器所在的线程可以保存或重载。</summary>
        private bool CanModifyToolBlock()
        {
            return !_isRunning && (_editingThreadId == 0 || _editingThreadId == Thread.CurrentThread.ManagedThreadId);
        }

        /// <summary>记录可显示的操作错误，返回 false 供调用方提前退出。</summary>
        private bool FailOperation(string message, out string error)
        {
            LastError = error = message;
            System.Diagnostics.Trace.WriteLine("[Vision] " + StationName + "：" + message);
            return false;
        }

        /// <summary>清理保存失败留下的临时文件，清理错误写入诊断输出。</summary>
        private static void DeleteTempFile(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            try { if (File.Exists(path)) File.Delete(path); }
            catch (Exception ex) { System.Diagnostics.Trace.WriteLine("[Vision] 清理临时文件：" + ex.Message); }
        }

        /// <summary>计算算法文件 SHA-256 摘要，文件读取失败交给加载或保存操作处理。</summary>
        private static string ComputeFileSignature(string path)
        {
            using (System.Security.Cryptography.SHA256 hash = System.Security.Cryptography.SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
                return BitConverter.ToString(hash.ComputeHash(stream)).Replace("-", "");
        }

        /// <summary>独占编辑本站算法，编辑期间拒绝运行。</summary>
        public void EditTB()
        {
            lock (_operationLock)
            {
                if (_isRunning || _editingThreadId != 0 || !IsLoadTBOk || TB == null) return;
                _editingThreadId = Thread.CurrentThread.ManagedThreadId;
            }
            try { using (TBEditForm editor = new TBEditForm(this)) editor.ShowDialog(); }
            finally { lock (_operationLock) _editingThreadId = 0; }
        }

        /// <summary>异步运行一次算法；忙碌或未加载时拒绝启动，原因见 LastError。</summary>
        public void RunTB()
        {
            long runId;
            TryRunTB(out runId);
        }

        /// <summary>原子地占用本站并返回运行编号；拒绝请求不会复用此前结果。</summary>
        public bool TryRunTB(out long runId)
        {
            lock (_operationLock)
            {
                runId = 0;
                string error;
                if (_isRunning || _editingThreadId != 0) return FailOperation(MiddleLayer.LangMsg("Vision", "error_BusyCapture", "工位忙碌，不能重复拍照。", "Station is busy; cannot capture again.", "La estación está ocupada; no se puede repetir la captura."), out error);
                if (!IsLoadTBOk || TB == null) return FailOperation(MiddleLayer.LangMsg("Vision", "error_NotLoadedPath", "ToolBlock 未加载：", "ToolBlock is not loaded: ", "ToolBlock no está cargado: ") + TBPath, out error);
                runId = ++_nextRunId;
                long requestId = runId;
                IsAccept = false;
                LastError = "";
                _isRunning = true;
                _lastRunResult = new VisionRunResult(runId, VisionRunState.Running);
                try { Task.Run(() => ExecuteRun(requestId)); return true; }
                catch (Exception ex)
                {
                    _isRunning = false;
                    _lastRunResult = new VisionRunResult(runId, VisionRunState.Faulted, ex.Message);
                    runId = 0;
                    return FailOperation(MiddleLayer.LangMsg("Vision", "error_Start", "启动视觉任务失败：", "Could not start vision task: ", "No se pudo iniciar la tarea de visión: ") + ex.Message, out error);
                }
            }
        }

        /// <summary>执行算法并发布同编号结果，显示错误不改变算法的成功或失败状态。</summary>
        private void ExecuteRun(long runId)
        {
            VisionRunResult result;
            ICogRecord record = null;
            try
            {
                VisionRun();
                result = CaptureRunResult(runId);
                try { if (AutoVisionRunDisplay) record = TB.CreateLastRunRecord(); }
                catch (Exception ex) { System.Diagnostics.Trace.WriteLine("[Vision] 获取图像记录：" + ex.Message); }
            }
            catch (Exception ex) { result = new VisionRunResult(runId, VisionRunState.Faulted, ex.Message); }

            lock (_operationLock)
            {
                _isRunning = false;
                IsAccept = false;
                if (_lastRunResult.RunId != runId || _lastRunResult.State != VisionRunState.Running) return;
                result.IsFlowManaged = flowRunId == runId;
                _lastRunResult = result;
                IsAccept = result.State == VisionRunState.Succeeded;
                LastError = result.Error;
            }
            QueueRunDisplay(runId, record);
            try { if (RunCompleted != null) RunCompleted(result); }
            catch (Exception ex) { System.Diagnostics.Trace.WriteLine("[Vision] 运行完成通知：" + ex.Message); }
        }

        /// <summary>生成算法结果快照；定位工位覆盖此方法以记录坐标。</summary>
        protected virtual VisionRunResult CaptureRunResult(long runId)
        {
            return new VisionRunResult(runId, IsAccept ? VisionRunState.Succeeded : VisionRunState.Rejected,
                IsAccept ? "" : MiddleLayer.LangMsg("Vision", "error_Rejected", "ToolBlock 判定 NG。", "ToolBlock result is NG.", "El resultado de ToolBlock es NG."));
        }

        /// <summary>使指定请求超时失效；底层任务退出前仍保持工位忙碌。</summary>
        public void TimeoutRun(long runId)
        {
            InvalidateRun(runId, VisionRunState.TimedOut, MiddleLayer.LangMsg("Vision", "error_TimedOut", "视觉运行超时。", "Vision operation timed out.", "La operación de visión agotó el tiempo de espera."));
        }

        /// <summary>取消指定请求的结果接收，阻止迟到结果被生产流程采用。</summary>
        public void CancelRun(long runId)
        {
            InvalidateRun(runId, VisionRunState.Cancelled, MiddleLayer.LangMsg("Vision", "error_Cancelled", "视觉运行已取消。", "Vision operation cancelled.", "Operación de visión cancelada."));
        }

        /// <summary>仅将仍在运行的同编号请求置为终止状态。</summary>
        private void InvalidateRun(long runId, VisionRunState state, string error)
        {
            lock (_operationLock)
            {
                if (_lastRunResult == null || _lastRunResult.RunId != runId) return;
                if (_lastRunResult.State != VisionRunState.Running) return;
                _lastRunResult = new VisionRunResult(runId, state, error);
                IsAccept = false;
                LastError = error;
            }
        }

        /// <summary>是否已有终止状态；成功判定请读取 LastRunResult.State 或 IsAccept。</summary>
        public bool RunTBOk()
        {
            VisionRunResult result = LastRunResult;
            return result != null && result.State != VisionRunState.Running;
        }

        /// <summary>把图像记录投递到各显示控件所属 UI 线程，已销毁或未建句柄的控件直接跳过。</summary>
        private void QueueRunDisplay(long runId, ICogRecord record)
        {
            if (record == null || VisionRunDisplayIndex < 0 || VisionRunDisplayIndex >= record.SubRecords.Count) return;
            CogRecordDisplay[] displays;
            lock (RecordDisplayList) displays = RecordDisplayList.ToArray();
            foreach (CogRecordDisplay display in displays)
            {
                if (display == null || display.IsDisposed || !display.IsHandleCreated) continue;
                try { display.BeginInvoke(new Action(() => RenderRunDisplay(display, runId, record))); }
                catch (Exception ex) { System.Diagnostics.Trace.WriteLine("[Vision] 投递图像：" + ex.Message); }
            }
        }

        /// <summary>显示仍属当前请求的记录，拒绝超时、取消及旧请求的迟到画面。</summary>
        private void RenderRunDisplay(CogRecordDisplay display, long runId, ICogRecord record)
        {
            VisionRunResult result = LastRunResult;
            if (display.IsDisposed || result == null || result.RunId != runId) return;
            if (result.State == VisionRunState.TimedOut || result.State == VisionRunState.Cancelled) return;
            try
            {
                display.StaticGraphics.Clear();
                display.InteractiveGraphics.Clear();
                display.Record = record.SubRecords[VisionRunDisplayIndex];
                display.AutoFit = true;
                display.Fit();
            }
            catch (Exception ex) { System.Diagnostics.Trace.WriteLine("[Vision] 显示图像：" + ex.Message); }
        }

        public object GetOutput(string OutputName)
        {
            try
            {
                return TB.Outputs[OutputName].Value;
            }
            catch (Exception) { return null; }
        }

        public void ShowRecordDisplay(CogRecordDisplay RecordDisplay, int RecordDisplayIndex)
        {
            if (IsLoadTBOk)
            {
                RecordDisplay.Image = null;
                RecordDisplay.StaticGraphics.Clear();
                RecordDisplay.InteractiveGraphics.Clear();

                if (RecordDisplayIndex < ((CogToolBlock)TB).CreateLastRunRecord().SubRecords.Count)
                {
                    RecordDisplay.Record = ((CogToolBlock)TB).CreateLastRunRecord().SubRecords[RecordDisplayIndex];
                    RecordDisplay.AutoFit = true;
                    RecordDisplay.Fit();
                }
            }
        }

        public void SaveImage(string FilePath, int RecordDisplayIndex)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    if (RecordDisplayIndex < ((CogToolBlock)TB).CreateLastRunRecord().SubRecords.Count)
                    {
                        if (!Directory.Exists(Path.GetDirectoryName(FilePath)))
                            Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                    }
                }
                catch (Exception) { }
            });
        }

        /// <summary>用临时标定对象预检点位（不碰任何 ToolBlock），返回可标定性、残差、比例与镜像标志。</summary>
        public static CalibApplyInfo CheckCalibration(List<CalibrationData> data)
        {
            CalibApplyInfo info = new CalibApplyInfo();
            if (data == null) { info.Error = MiddleLayer.LangMsg("Vision", "error_NoPoints", "没有标定点", "No calibration points", "No hay puntos de calibración"); return info; }

            info.PointCount = data.Count;
            if (data.Count < 3) { info.Error = MiddleLayer.LangMsg("Vision", "error_TooFewPoints", "标定点不足 3 个，无法计算标定", "At least 3 points are required to calculate calibration.", "Se requieren al menos 3 puntos para calcular la calibración."); return info; }

            if (!RunTempCalibration(data, info)) return info;

            EstimateScale(data, out info.ScalePerPixel, out info.Mirrored);
            return info;
        }

        /// <summary>在临时 CogCalibNPointToNPoint 上算一次标定，把残差与可标定性写进 info。</summary>
        private static bool RunTempCalibration(List<CalibrationData> data, CalibApplyInfo info)
        {
            try
            {
                CogCalibNPointToNPoint calib = new CogCalibNPointToNPoint();
                FillCalibrationPoints(calib, data);
                calib.Calibrate();

                info.Calibrated = calib.Calibrated;
                info.Rmse = calib.ComputedRMSError;
                if (info.Calibrated) return true;

                info.Error = MiddleLayer.LangMsg("Vision", "error_InvalidPoints", "点位算不出有效标定（可能全部共线或重复）", "Cannot calculate a valid calibration; points may be collinear or duplicated.", "No se puede calcular una calibración válida; los puntos pueden ser colineales o duplicados.");
                return false;
            }
            catch (Exception ex)
            {
                info.Error = ex.Message;
                return false;
            }
        }

        /// <summary>把标定点写进 Cognex 标定对象：电机坐标写 Calibrated，像素坐标写 Uncalibrated。</summary>
        private static void FillCalibrationPoints(CogCalibNPointToNPoint calib, List<CalibrationData> data)
        {
            calib.NumPoints = data.Count;
            for (int i = 0; i < data.Count; i++)
            {
                calib.SetRawCalibratedPointX(i, data[i].MotorPosX);
                calib.SetRawCalibratedPointY(i, data[i].MotorPosY);
                calib.SetUncalibratedPointX(i, data[i].PixelX);
                calib.SetUncalibratedPointY(i, data[i].PixelY);
            }
        }

        /// <summary>按相距最远的两点估算 电机单位/像素 比例，并判断像素与电机是否互为镜像。</summary>
        private static void EstimateScale(List<CalibrationData> data, out double scale, out bool mirrored)
        {
            scale = 0;
            mirrored = false;

            int a, b;
            FindFarthestPixelPair(data, out a, out b);
            if (a < 0) return;

            double pixelX = data[a].PixelX - data[b].PixelX;
            double pixelY = data[a].PixelY - data[b].PixelY;
            double motorX = data[a].MotorPosX - data[b].MotorPosX;
            double motorY = data[a].MotorPosY - data[b].MotorPosY;

            double pixelLength = Math.Sqrt(pixelX * pixelX + pixelY * pixelY);
            if (pixelLength <= 0) return;

            scale = Math.Sqrt(motorX * motorX + motorY * motorY) / pixelLength;
            if (double.IsNaN(scale) || double.IsInfinity(scale)) { scale = 0; return; }

            mirrored = IsMirrored(data);
        }

        /// <summary>找像素空间相距最远的一对点；点不足时返回 a = -1。</summary>
        private static void FindFarthestPixelPair(List<CalibrationData> data, out int a, out int b)
        {
            a = -1;
            b = -1;
            double best = 0;

            for (int i = 0; i < data.Count; i++)
                for (int j = i + 1; j < data.Count; j++)
                {
                    double dx = data[i].PixelX - data[j].PixelX;
                    double dy = data[i].PixelY - data[j].PixelY;
                    double distance = dx * dx + dy * dy;
                    if (distance <= best) continue;
                    best = distance;
                    a = i;
                    b = j;
                }
        }

        /// <summary>用首个不共线的三点比较像素与电机的有向面积符号，符号相反即为镜像。</summary>
        private static bool IsMirrored(List<CalibrationData> data)
        {
            for (int i = 0; i + 2 < data.Count; i++)
            {
                double pixelArea = Area(data[i].PixelX, data[i].PixelY,
                                        data[i + 1].PixelX, data[i + 1].PixelY,
                                        data[i + 2].PixelX, data[i + 2].PixelY);
                double motorArea = Area(data[i].MotorPosX, data[i].MotorPosY,
                                        data[i + 1].MotorPosX, data[i + 1].MotorPosY,
                                        data[i + 2].MotorPosX, data[i + 2].MotorPosY);

                if (Math.Abs(pixelArea) < 1e-6 || Math.Abs(motorArea) < 1e-6) continue;
                return pixelArea * motorArea < 0;
            }
            return false;
        }

        /// <summary>三点构成的三角形有向面积。</summary>
        private static double Area(double x1, double y1, double x2, double y2, double x3, double y3)
        {
            return (x2 - x1) * (y3 - y1) - (x3 - x1) * (y2 - y1);
        }

        public bool SetCalibration(List<CalibrationData> CalibData, string ToolName)
        {
            CalibApplyInfo info;
            return SetCalibration(CalibData, ToolName, out info);
        }

        /// <summary>把标定点写入 TB 里名为 ToolName 的标定工具并保存；失败时通过 info 返回具体原因。</summary>
        public bool SetCalibration(List<CalibrationData> CalibData, string ToolName, out CalibApplyInfo info)
        {
            return ApplyCalibration(CalibData, ToolName, true, out info);
        }

        /// <summary>把标定结果应用到算法内存草稿，由主界面统一保存文件。</summary>
        internal bool SetCalibrationDraft(List<CalibrationData> points, string toolName, out CalibApplyInfo info)
        {
            return ApplyCalibration(points, toolName, false, out info);
        }

        /// <summary>在副本上完成标定后替换算法，显式导出调用可同时写盘。</summary>
        private bool ApplyCalibration(List<CalibrationData> CalibData, string ToolName, bool save, out CalibApplyInfo info)
        {
            info = new CalibApplyInfo();
            if (CalibData != null) info.PointCount = CalibData.Count;
            if (CalibData == null || CalibData.Count < 3) { info.Error = MiddleLayer.LangMsg("Vision", "error_MinPoints", "标定点不足 3 个", "Fewer than 3 calibration points", "Hay menos de 3 puntos de calibración"); return false; }
            lock (_operationLock)
            {
                if (_isRunning || _editingThreadId != 0) { info.Error = MiddleLayer.LangMsg("Vision", "error_BusyCalibrate", "工位正在运行或编辑，不能应用标定。", "Station is running or being edited; calibration cannot be applied.", "La estación está ejecutándose o en edición; no se puede aplicar la calibración."); return false; }
                CogCalibNPointToNPointTool tool;
                if (!TryGetCalibTool(ToolName, out tool, info)) return false;
                CogToolBlock original = TB;
                try
                {
                    CogToolBlock candidate = (CogToolBlock)CogSerializer.DeepCopyObject(original);
                    tool = (CogCalibNPointToNPointTool)candidate.Tools[ToolName];
                    FillCalibrationPoints(tool.Calibration, CalibData);
                    tool.Calibration.Calibrate();
                    info.Calibrated = tool.Calibration.Calibrated;
                    info.Rmse = tool.Calibration.ComputedRMSError;
                    if (!info.Calibrated) { info.Error = MiddleLayer.LangMsg("Vision", "error_InvalidCalibration", "标定点无效，未修改原算法。", "Invalid calibration points; original algorithm was preserved.", "Puntos de calibración no válidos; se conservó el algoritmo original."); return false; }
                    TB = candidate;
                    string error;
                    if (save && !TrySaveTB(out error)) { TB = original; info.Error = error; return false; }
                    if (!save) MarkToolBlockDraft();
                    IsAccept = false;
                    _lastRunResult = null;
                    info.Ok = true;
                    return true;
                }
                catch (Exception ex) { TB = original; info.Error = ex.Message; return false; }
            }
        }

        /// <summary>取 TB 里的标定工具；TB 未加载或工具不存在时把原因写进 info。</summary>
        private bool TryGetCalibTool(string toolName, out CogCalibNPointToNPointTool tool, CalibApplyInfo info)
        {
            tool = null;
            if (TB == null || !IsLoadTBOk) { info.Error = MiddleLayer.LangMsg("Vision", "error_NotLoaded", "ToolBlock 未加载", "ToolBlock is not loaded", "ToolBlock no está cargado"); return false; }
            if (string.IsNullOrEmpty(toolName)) { info.Error = MiddleLayer.LangMsg("Vision", "error_ToolNameEmpty", "标定工具名为空", "Calibration tool name is empty", "El nombre de la herramienta de calibración está vacío"); return false; }

            try { tool = TB.Tools[toolName] as CogCalibNPointToNPointTool; }
            catch (Exception) { tool = null; }

            if (tool != null) return true;

            info.Error = MiddleLayer.LangMsg("Vision", "error_NoCalibToolA", "TB 里没有名为「", "ToolBlock has no CogCalibNPointToNPointTool named \"", "ToolBlock no contiene una herramienta CogCalibNPointToNPointTool llamada \"") + toolName + MiddleLayer.LangMsg("Vision", "error_NoCalibToolB", "」的 CogCalibNPointToNPointTool", "\".", "\".");
            return false;
        }

        public void RunLive(CogRecordDisplay RecordDisplay)
        {
            if (RecordDisplay.LiveDisplayRunning)
            {
                RecordDisplay.StopLiveDisplay();
                RecordDisplay.StaticGraphics.Clear();
                RecordDisplay.InteractiveGraphics.Clear();
            }
            else
            {
                try
                {
                    RecordDisplay.Image = null;
                    RecordDisplay.StaticGraphics.Clear();
                    RecordDisplay.InteractiveGraphics.Clear();
                    CreatCentrelLine(RecordDisplay);

                    CogAcqFifoTool myCogAcqFifoTool = new CogAcqFifoTool();

                    RecordDisplay.Image = myCogAcqFifoTool.OutputImage;

                    // 有 SN 就按 SN 现场解析通道；一个 SN 都没配时退回 RunLiveCCDIndex（老配置），不要盲目取通道
                    string bindError = "";
                    int ccdIndex = string.IsNullOrEmpty(RunLiveCCDSN)
                        ? RunLiveCCDIndex
                        : CogCameraDevices.ResolveIndex(RunLiveCCDSN, out bindError);
                    if (ccdIndex < 0)
                    {
                        MessageBox.Show(string.IsNullOrEmpty(bindError) ? MiddleLayer.LangMsg("Vision", "error_CameraUnavailable", "相机不可用。", "Camera unavailable.", "Cámara no disponible.") : bindError);
                        return;
                    }

                    CogFrameGrabbers CCD_Graber = new Cognex.VisionPro.CogFrameGrabbers();
                    if (CCD_Graber == null)
                        throw new Exception("Failed to create the CogFrameGrabbers object.");
                    if (ccdIndex >= CCD_Graber.Count)
                        throw new Exception(MiddleLayer.LangMsg("Vision", "error_ChannelRangeA", "相机 SN 解析到的通道 ", "Camera SN resolved to channel ", "El SN de cámara corresponde al canal ") + ccdIndex + MiddleLayer.LangMsg("Vision", "error_ChannelRangeB", " 超出范围（当前只有 ", " which is out of range (available devices: ", " fuera de rango (dispositivos disponibles: ")
                            + CCD_Graber.Count + MiddleLayer.LangMsg("Vision", "error_ChannelRangeC", " 台设备）。请重新选择相机 SN。", "). Select the camera SN again.", "). Seleccione de nuevo el SN de cámara."));

                    ICogFrameGrabber frameGrabber = CCD_Graber[ccdIndex];
                    ICogAcqFifo Fifo = frameGrabber.CreateAcqFifo(frameGrabber.AvailableVideoFormats[0], CogAcqFifoPixelFormatConstants.Format8Grey, 0, true);
                    ICogAcqExposure Exposure = Fifo.OwnedExposureParams;
                    Exposure.Exposure = RunExposure;
                    Fifo.Prepare();
                    RecordDisplay.StopLiveDisplay();
                    RecordDisplay.StartLiveDisplay(Fifo);
                    RecordDisplay.AutoFit = true;
                }
                catch(Exception e)
                {
                    MessageBox.Show(e.ToString());
                }
            }
        }

        public virtual void CreatCentrelLine(CogRecordDisplay Crd)
        {
            //2592 ,  1944
            CogLine vline = new CogLine();
            CogLine hline = new CogLine();
            vline.Color = CogColorConstants.Red;
            hline.Color = CogColorConstants.Red;
            vline.SetFromStartXYEndXY(1296, 0, 1296, 1944);
            hline.SetFromStartXYEndXY(0, 972, 2592, 972);
            Crd.InteractiveGraphics.Add(vline, "vline", true);
            Crd.InteractiveGraphics.Add(hline, "hline", true);
        }

        #region 图片旋转函数
        /// <summary>
        /// 以逆时针为方向对图像进行旋转
        /// </summary>
        /// <param name="b">位图流</param>
        /// <param name="angle">旋转角度[0,360](前台给的)</param>
        public static Bitmap Rotate(Bitmap b, int angle, CogRecordDisplay Crd)
        {
            angle = angle % 360;
            //弧度转换
            double radian = angle * Math.PI / 180.0;
            double cos = Math.Cos(radian);
            double sin = Math.Sin(radian);
            //原图的宽和高
            int w = b.Width;
            int h = b.Height;
            int W = (int)(Math.Max(Math.Abs(w * cos - h * sin), Math.Abs(w * cos + h * sin)));
            int H = (int)(Math.Max(Math.Abs(w * sin - h * cos), Math.Abs(w * sin + h * cos)));
            //目标位图
            Bitmap dsImage = new Bitmap(W, H);
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(dsImage);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bilinear;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

			//计算偏移量
			System.Drawing.Point Offset = new System.Drawing.Point((W - w) / 2, (H - h) / 2);
            //构造图像显示区域：让图像的中心与窗口的中心点一致
            Rectangle rect = new Rectangle(Offset.X, Offset.Y, w, h);
			System.Drawing.Point center = new System.Drawing.Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
            g.TranslateTransform(center.X, center.Y);
            g.RotateTransform(360 - angle);
            //恢复图像在水平和垂直方向的平移
            g.TranslateTransform(-center.X, -center.Y);
            g.DrawImage(b, rect);
            //重至绘图的所有变换
            g.ResetTransform();
            g.Save();
            g.Dispose();
            return dsImage;
        }
        #endregion 图片旋转函数
    }
}
