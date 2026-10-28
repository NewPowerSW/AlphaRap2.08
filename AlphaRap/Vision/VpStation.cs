using System;
using System.Collections.Generic;
using System.Globalization;      // NumberStyles / CultureInfo
using System.Text;              // StringBuilder
using System.Xml;
using Cognex.VisionPro;          // CogRecordDisplay / CogLine / CogColorConstants / CogFrameGrabbers

namespace AlphaRap
{
    /// <summary>相机配置（对应 VPForm 的 tabControl1 中的一页），属于机器配置，不随配方变化。</summary>
    public class VpCameraConfig
    {
        /// <summary>相机的显示名称。</summary>
        public string Name = "";
        /// <summary>首次创建时确定的存储目录，显示名称改变时保持不变。</summary>
        public string FolderName = "";
        public string StorageFolder { get { return string.IsNullOrEmpty(FolderName) ? Name : FolderName; } }
        /// <summary>
        /// 相机绑定键（一般是 Cognex 的 <c>ICogFrameGrabber.SerialNumber</c>，SN 为空时用 UniqueID）。
        /// 相机**只按 SN 绑定**（见 <see cref="CogCameraDevices"/>）：换 USB/网口、设备插拔、
        /// 枚举顺序变了都不会绑错；没选 SN 时这台相机不可取流，界面上会要求先选。
        /// </summary>
        public string CameraSN = "";
        /// <summary>
        /// 默认曝光：新建 VPP / 添加标定时作为初始值。取流使用 VPP 自己的曝光（<see cref="VpVppConfig.Exposure"/>）
        /// 或标定的曝光（<see cref="VpCalibration.Exposure"/>）。
        /// </summary>
        public double Exposure = 10;
        /// <summary>该相机下的 VPP 列表（每个 VPP = 相机页里 VPP 子 TabControl 的一页）。</summary>
        public List<VpVppConfig> Vpps = new List<VpVppConfig>();

        /// <summary>
        /// 取这台相机当前实际所在的采集通道（由 SN 现场解析）。返回 -1 表示没绑定 SN 或当前机器上没有这台，
        /// <paramref name="error"/> 给出可以直接显示给操作员的原因。
        /// </summary>
        public int ResolveCcdIndex(out string error)
        {
            if (string.IsNullOrEmpty(CameraSN))
            {
                error = MiddleLayer.LangMsg("Vision", "error_CameraNameA", "相机「", "Camera \"", "La cámara \"") + Name + MiddleLayer.LangMsg("Vision", "error_CameraNameB", "」还没有选择 SN：请在相机页的 SN 下拉框里选一台相机。", "\" has no SN. Select a camera in the SN list.", "\" no tiene SN. Seleccione una cámara en la lista SN.");
                return -1;
            }
            return CogCameraDevices.ResolveIndex(CameraSN, out error);
        }

        /// <summary>
        /// 按 VPP 名取补偿配置表，没有该 VPP 返回 null；各行包含 Item、Compensation、Min、Max。
        /// </summary>
        public List<VpCompLimitItem> GetCompLimits(string vppName)
        {
            if (string.IsNullOrEmpty(vppName)) return null;
            for (int i = 0; i < Vpps.Count; i++)
                if (Vpps[i] != null && Vpps[i].Name == vppName) return Vpps[i].CompLimits;
            return null;
        }

        /// <summary>按 VPP 名 + 行名取一行补偿限制（同名取第一行），没有返回 null。</summary>
        public VpCompLimitItem GetCompLimit(string vppName, string item)
        {
          
            for (int i = 0; i < Vpps.Count; i++)
            {
                if (Vpps[i] == null || Vpps[i].Name != vppName) continue;
                return Vpps[i].GetCompLimit(item);
            }
            return null;
        }
    }

    /// <summary>VPP 配置（对应相机页中 VPP 子页的一页），归属于创建它的相机。</summary>
    public class VpVppConfig
    {
        /// <summary>工位的持久化标识，改名时保持不变。</summary>
        public string Id = Guid.NewGuid().ToString("N");
        public string Name = "";
        public string FolderName = "";
        public string StorageFolder { get { return string.IsNullOrEmpty(FolderName) ? Name : FolderName; } }
        /// <summary>
        /// 本 VPP 取流用的曝光时间（各 VPP 独立，与标定曝光 <see cref="VpCalibration.Exposure"/> 互不影响）；
        /// 新建时继承相机的默认曝光。
        /// </summary>
        public double Exposure = 10;
        /// <summary>
        /// 视觉定位补偿配置表（VPP 页上可增删、改名），每行包含名称、补偿值与上下限。
        /// 定位工位按名称匹配限制，超限时拒绝结果并保留原始坐标。
        /// 补偿值只提供给应用流程读取，由应用流程决定如何使用。
        /// 外部通过 <see cref="VpCameraConfig.GetCompLimits"/> / <see cref="VpCameraConfig.GetCompLimit"/> 访问。
        /// </summary>
        public List<VpCompLimitItem> CompLimits = new List<VpCompLimitItem>();
        /// <summary>运行时对应的视觉站实例（删除 VPP 时需从 VisionproInterface.VList 移除）。</summary>
        public VpStation Station;

        // ---- 存图设置：RunAndWait 成功后自动存图，见 VpStation.SaveLastImage ----
        /// <summary>是否启用存图。</summary>
        public bool SaveImageEnabled = false;
        /// <summary>存图目录；空 = vpp 所在目录下的 Images。</summary>
        public string SaveImagePath = "";
        /// <summary>保存原图（采集根记录），可与截图同时启用。</summary>
        public bool SaveImageOriginal = false;
        /// <summary>保存截图（主界面显示的子记录），可与原图同时启用。</summary>
        public bool SaveImageSnapshot = false;
        /// <summary>是否按保留天数自动删除旧照片。</summary>
        public bool SaveImageAutoDelete = false;
        /// <summary>照片保留天数（启用自动删除时生效）。</summary>
        public int SaveImageKeepDays = 0;

        /// <summary>
        /// 补偿限制表为空时补三行默认值（X / Y / R），保留已经配置的业务键。
        /// </summary>
        public void EnsureCompLimits()
        {
            if (CompLimits.Count > 0) return;
            CompLimits.Add(new VpCompLimitItem("X", -1, 1));
            CompLimits.Add(new VpCompLimitItem("Y", -1, 1));
            CompLimits.Add(new VpCompLimitItem("R", -5, 5));
        }

        /// <summary>按行名取限制；没有返回 null（同名取第一行）。</summary>
        public VpCompLimitItem GetCompLimit(string item)
        {
            if (string.IsNullOrEmpty(item)) return null;
            for (int i = 0; i < CompLimits.Count; i++)
                if (CompLimits[i] != null && CompLimits[i].Item == item) return CompLimits[i];
            return null;
        }
    }

    /// <summary>补偿配置表的一行，单位与应用流程一致（X/Y 一般为 mm，角度为度）。</summary>
    public class VpCompLimitItem
    {
        /// <summary>稳定的项目键，界面显示翻译不改变此值。</summary>
        public string Item = "";
        /// <summary>应用流程自行读取和使用的补偿值，默认 0。</summary>
        public double Compensation = 0;
        public double Min = -1;
        public double Max = 1;

        /// <summary>创建使用默认补偿值和上下限的配置行。</summary>
        public VpCompLimitItem() { }
        /// <summary>按项目键和上下限创建配置行，补偿值默认为 0。</summary>
        public VpCompLimitItem(string item, double min, double max) { Item = item; Min = min; Max = max; }
    }

    /// <summary>
    /// 一台可用的采集设备（CogFrameGrabbers 中的一项）。界面上的「相机 SN」下拉框直接列这些对象，
    /// 用户选中即把这台设备的绑定键写进 <see cref="VpCameraConfig.CameraSN"/>。
    /// </summary>
    public sealed class CogCameraDevice
    {
        /// <summary>枚举下标（CogFrameGrabbers[Index]）。</summary>
        public int Index;
        /// <summary>设备名（<c>ICogFrameGrabber.Name</c>）。</summary>
        public string Name = "";
        /// <summary>序列号（<c>ICogFrameGrabber.SerialNumber</c>）；个别驱动给空串。</summary>
        public string SerialNumber = "";
        /// <summary>唯一 ID（<c>ICogFrameGrabber.UniqueID</c>），SN 为空时用它兜底。</summary>
        public string UniqueId = "";

        /// <summary>绑定用的键：优先 SN，其次 UniqueID。</summary>
        public string BindKey
        {
            get { return string.IsNullOrEmpty(SerialNumber) ? UniqueId : SerialNumber; }
        }

        /// <summary>下拉框只显示 SN（没有 SN 时显示 UniqueID / 无序列号），索引不对外暴露。</summary>
        public override string ToString()
        {
            string id = string.IsNullOrEmpty(SerialNumber) ? UniqueId : SerialNumber;
            if (string.IsNullOrEmpty(id)) id = "(无序列号)";
            return id;
        }
    }

    /// <summary>
    /// 采集设备枚举 + 按 SN 绑定。<c>CogFrameGrabbers</c> 只能按下标取，
    /// 所以"按 SN 绑定"= 运行时把配置里的 SN 解析成**当前**下标：
    /// 设备插拔、换口、驱动枚举顺序变化都不会再绑错相机。
    /// </summary>
    public static class CogCameraDevices
    {
        /// <summary>最近一次枚举 / 解析的错误信息（成功时为 null）。</summary>
        public static string LastError;

        private static readonly object _lock = new object();
        private static List<CogCameraDevice> _cache;
        private static DateTime _cacheTime = DateTime.MinValue;
        /// <summary>缓存时间：界面下拉框会反复取，避免每次都去枚举驱动（枚举较慢且会打开设备）。</summary>
        private const double CacheSeconds = 3;

        /// <summary>枚举当前可用的全部采集设备（含 SN），带短缓存。</summary>
        public static List<CogCameraDevice> Enumerate()
        {
            return Enumerate(false);
        }

        /// <summary>上一次枚举的结果；从没枚举过时为 null（界面据此避免开机就扫描设备）。</summary>
        public static List<CogCameraDevice> Cached
        {
            get { return _cache; }
        }

        /// <summary>
        /// 枚举当前可用的全部采集设备；<paramref name="refresh"/> = true 时忽略缓存重新扫描。
        /// 枚举会打开驱动层对象，必须成对释放，所以返回前一律 Dispose，避免反复扫描泄漏句柄。
        /// </summary>
        public static List<CogCameraDevice> Enumerate(bool refresh)
        {
            lock (_lock)
            {
                if (!refresh && _cache != null && (DateTime.Now - _cacheTime).TotalSeconds < CacheSeconds)
                    return _cache;

                List<CogCameraDevice> list = new List<CogCameraDevice>();
                LastError = null;
                CogFrameGrabbers grabbers = null;
                try
                {
                    grabbers = new CogFrameGrabbers();
                    for (int i = 0; i < grabbers.Count; i++)
                    {
                        ICogFrameGrabber fg = grabbers[i];
                        if (fg == null) continue;

                        CogCameraDevice d = new CogCameraDevice();
                        d.Index = i;
                        d.Name = ReadSafe(delegate { return fg.Name; });
                        d.SerialNumber = ReadSafe(delegate { return fg.SerialNumber; });
                        d.UniqueId = ReadSafe(delegate { return fg.UniqueID; });
                        list.Add(d);
                    }
                }
                catch (Exception ex)
                {
                    // 没装 VisionPro 驱动 / 没接相机时枚举会抛异常：按"没有可用设备"处理，不打断界面
                    LastError = ex.Message;
                }
                finally
                {
                    // 释放失败不能影响枚举结果，单独吞掉
                    if (grabbers != null)
                    {
                        try { grabbers.Dispose(); }
                        catch (Exception ex) { System.Diagnostics.Debug.WriteLine("[CogCameraDevices] Dispose → " + ex.Message); }
                    }
                }

                _cache = list;
                _cacheTime = DateTime.Now;
                return list;
            }
        }

        /// <summary>按绑定键（SN / UniqueID / 设备名）在列表里找设备；找不到返回 null。</summary>
        public static CogCameraDevice Find(List<CogCameraDevice> devices, string bindKey)
        {
            if (devices == null || string.IsNullOrEmpty(bindKey)) return null;
            string key = bindKey.Trim();
            for (int i = 0; i < devices.Count; i++)
                if (SameText(devices[i].SerialNumber, key)
                    || SameText(devices[i].UniqueId, key)
                    || SameText(devices[i].Name, key))
                    return devices[i];
            return null;
        }

        /// <summary>
        /// 把配置里的 SN 解析成当前采集通道（<c>CogFrameGrabbers</c> 的下标）。
        /// 返回 -1 表示没配 SN 或当前机器上找不到，<paramref name="error"/> 给出原因。
        /// </summary>
        public static int ResolveIndex(string bindKey, out string error)
        {
            error = null;
            if (string.IsNullOrEmpty(bindKey))
            {
                error = MiddleLayer.LangMsg("Vision", "error_SelectSN", "相机还没有选择 SN。", "Camera SN has not been selected.", "No se ha seleccionado el SN de cámara.");
                return -1;
            }

            CogCameraDevice d = Find(Enumerate(), bindKey);
            if (d != null) return d.Index;

            error = MiddleLayer.LangMsg("Vision", "error_SNMissingA", "未找到序列号为「", "No camera found with serial number \"", "No se encontró una cámara con número de serie \"") + bindKey + MiddleLayer.LangMsg("Vision", "error_SNMissingB", "」的相机", "\".", "\".");
            if (grabberCountHint() > 0) error += MiddleLayer.LangMsg("Vision", "error_DeviceCountA", "（当前只枚举到 ", " (devices discovered: ", " (dispositivos detectados: ") + grabberCountHint() + MiddleLayer.LangMsg("Vision", "error_DeviceCountB", " 台设备）", ")", ")");
            if (!string.IsNullOrEmpty(LastError)) error += "：" + LastError;
            return -1;
        }

        /// <summary>当前枚举到的设备数量（用于报错文字，取缓存不重新扫描）。</summary>
        private static int grabberCountHint()
        {
            List<CogCameraDevice> list = _cache;
            return (list == null) ? 0 : list.Count;
        }

        private static bool SameText(string a, string b)
        {
            return !string.IsNullOrEmpty(a) && string.Equals(a.Trim(), b, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>单个属性读取失败（驱动不支持）时返回空串，不影响其它字段。</summary>
        private static string ReadSafe(Func<string> getter)
        {
            try { return getter() ?? ""; }
            catch (Exception) { return ""; }
        }
    }

    /// <summary>工位类型：决定 ToolBlock 运行后取哪些输出，以及十字线大小。</summary>
    public enum VpStationKind
    {
        /// <summary>标定：取 x / y，十字线按整幅大图（2592x1944）画。</summary>
        Calibration,
        /// <summary>基准点 Fiducial：取 x / y / A，十字线按 1280x960 画。</summary>
        Fiducial,
        /// <summary>自定义：只跑 ToolBlock，不自动取结果（动态新增的 VPP 用这个）。</summary>
        Custom
    }

    /// <summary>
    /// 视觉站：运行 vpp 中的 ToolBlock 并按 <see cref="VpStationKind"/> 取结果。
    /// 构造参数决定 vpp 目录（工位名，或"相机名\VPP名"）、工位类型和默认 CCD 号（运行时被配方 Pset 覆盖）。
    /// </summary>
    public partial class VpStation : VisionproInterface
    {
        /// <summary>所属相机名（固定工位时等于工位名）。</summary>
        public string CameraName = "";
        /// <summary>VPP 名（固定工位时为空）。</summary>
        public string VppName = "";

        /// <summary>
        /// 本站的 VPP 配置（建站时赋值），补偿限制从这里读取；固定工位为 null，此时不做补偿限制。
        /// </summary>
        public VpVppConfig Config;

        /// <summary>工位类型（取哪些输出 + 十字线大小）。</summary>
        public VpStationKind Kind = VpStationKind.Custom;

        /// <summary>基准点结果（x / y / u）。</summary>
        public Pos4D Fiducial = new Pos4D();
        /// <summary>标定结果（x / y）。</summary>
        public Pos4D Calibration = new Pos4D();

        // ================= 拍照 / 补偿限制的对外接口 =================
        // 用法：
        //     VpStation st = MiddleLayer.VPF.GetVppStation("Camera1", "VPP1");
        //     if (st.RunAndWait(3000)) { /* st.Fiducial / st.GetOutput("输出名") / st.CompLimits ... */ }
        // 行名为 VPP 页补偿限制表"项目"列的内容（默认 X / Y / 角度）。

        /// <summary>本 VPP 的补偿限制表；未关联配置时为 null。</summary>
        public List<VpCompLimitItem> CompLimits
        {
            get { return Config == null ? null : Config.CompLimits; }
        }

        /// <summary>
        /// 拍照并等待完成（异步的 <see cref="VisionproInterface.RunTB"/> + 轮询结果）。
        /// 返回 true 表示在 timeoutMs 内完成且 ToolBlock 结果为 Accept；启用存图时成功后自动存图并清理过期照片。
        /// </summary>
        [AlphaRap.FlowProgramming.FlowToolIgnore]
        public bool RunAndWait(int timeoutMs)
        {
            if (timeoutMs <= 0) throw new ArgumentOutOfRangeException("timeoutMs");
            long runId;
            if (!TryRunTB(out runId)) return false;
            System.Diagnostics.Stopwatch elapsed = System.Diagnostics.Stopwatch.StartNew();
            while (elapsed.ElapsedMilliseconds < timeoutMs)
            {
                VisionRunResult pending = LastRunResult;
                if (pending == null || pending.RunId != runId) return false;
                if (pending.State != VisionRunState.Running) break;
                System.Threading.Thread.Sleep(10);
            }
            TimeoutRun(runId);
            VisionRunResult result = LastRunResult;
            if (result == null || result.RunId != runId || result.State != VisionRunState.Succeeded) return false;

            if (Config != null && Config.SaveImageEnabled) SaveLastImage();
            return true;
        }

        /// <summary>按行名（补偿限制表"项目"列）取本 VPP 的补偿限制；没有该行返回 false。</summary>
        [AlphaRap.FlowProgramming.FlowToolIgnore]
        public bool TryGetCompLimit(string item, out double min, out double max)
        {
            min = 0; max = 0;
            if (Config == null || string.IsNullOrEmpty(item)) return false;
            VpCompLimitItem it = Config.GetCompLimit(item);
            if (it == null) return false;
            min = it.Min; max = it.Max;
            return true;
        }

        /// <summary>
        /// 单轴补偿夹取：把 value 夹到该行的 [Min, Max]。返回 true 表示超限（value 已设为边界值，reason 如 "X&gt;Max"）；
        /// 未超限或没有该行时返回 false。OK/NG 判定由调用方决定。
        /// </summary>
        [AlphaRap.FlowProgramming.FlowToolIgnore]
        public bool ClampComp(string item, ref double value, out string reason)
        {
            reason = "";
            double min, max;
            if (!TryGetCompLimit(item, out min, out max)) return false;
            if (value < min) { value = min; reason = item + "<Min"; return true; }
            if (value > max) { value = max; reason = item + ">Max"; return true; }
            return false;
        }

        /// <summary>
        /// 按本 VPP 的存图设置保存最近一次运行的图像，返回最后一个文件的全路径（未启用或无图时返回 ""）。
        /// 原图为采集根记录，截图为主界面显示的子记录（<see cref="VisionproInterface.VisionRunDisplayIndex"/>）；
        /// 保存后按保留天数清理旧照片。
        /// </summary>
        [AlphaRap.FlowProgramming.FlowToolIgnore]
        public string SaveLastImage() { return SaveLastImage(Config); }

        /// <summary>按已捕获的存图设置保存最近图像，不读取其他节点的设置或覆盖页面草稿。</summary>
        private string SaveLastImage(VpVppConfig settings)
        {
            if (settings == null || !settings.SaveImageEnabled) return "";
            if (TB == null || !IsLoadTBOk) return "";
            if (!settings.SaveImageOriginal && !settings.SaveImageSnapshot) return "";

            try
            {
                Cognex.VisionPro.ICogRecord root = TB.CreateLastRunRecord();   // CreateLastRunRecord 返回的是 ICogRecord 接口
                if (root == null) return "";

                string dir = SaveImageDirectory(settings);
                if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);
                string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
                string last = "";

                if (settings.SaveImageOriginal)
                    last = WriteImageFile(root.Content as Cognex.VisionPro.ICogImage, dir, stamp + "_original.bmp");
                if (settings.SaveImageSnapshot && root.SubRecords.Count > VisionRunDisplayIndex)
                    last = WriteImageFile(root.SubRecords[VisionRunDisplayIndex].Content as Cognex.VisionPro.ICogImage, dir, stamp + "_snap.bmp");

                CleanOldImages(dir, settings);
                return last;
            }
            catch (Exception) { return ""; }
        }

        /// <summary>把一张图写成 bmp；失败返回 ""。</summary>
        private string WriteImageFile(Cognex.VisionPro.ICogImage img, string dir, string fileName)
        {
            try
            {
                if (img == null) return "";
                string file = System.IO.Path.Combine(dir, fileName);
                // CogImageFile / CogImageFileModeConstants 位于 Cognex.VisionPro.ImageFile 命名空间
                using (Cognex.VisionPro.ImageFile.CogImageFile f = new Cognex.VisionPro.ImageFile.CogImageFile())
                {
                    f.Open(file, Cognex.VisionPro.ImageFile.CogImageFileModeConstants.Write);
                    f.Append(img);
                    f.Close();
                }
                return file;
            }
            catch (Exception) { return ""; }
        }

        /// <summary>存图目录：VPP 配置中的路径，未设置时为 vpp 所在目录下的 Images。</summary>
        private string SaveImageDirectory(VpVppConfig settings)
        {
            if (!string.IsNullOrEmpty(settings.SaveImagePath)) return settings.SaveImagePath;
            try
            {
                return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(GetVppPath(SysPara.RecipeName)), "Images");
            }
            catch (Exception) { return ".\\Images"; }
        }

        /// <summary>
        /// 启用自动删除时，按保留天数删除存图目录中的旧 *.bmp（每次存图时执行；天数 &lt;= 0 时不删除）。
        /// </summary>
        private void CleanOldImages(string dir, VpVppConfig settings)
        {
            if (!settings.SaveImageAutoDelete) return;
            if (settings.SaveImageKeepDays <= 0) return;
            try
            {
                DateTime dead = DateTime.Now.AddDays(-settings.SaveImageKeepDays);
                string[] files = System.IO.Directory.GetFiles(dir, "*.bmp");
                for (int i = 0; i < files.Length; i++)
                    if (System.IO.File.GetLastWriteTime(files[i]) < dead)
                        System.IO.File.Delete(files[i]);
            }
            catch (Exception) { }
        }

        /// <summary>把另一个 CogRecordDisplay 绑定到本站，拍照和实时画面也会显示到它上面（按引用去重）。</summary>
        [AlphaRap.FlowProgramming.FlowToolIgnore]
        public void BindDisplay(Cognex.VisionPro.CogRecordDisplay crd)
        {
            if (crd == null) return;
            lock (RecordDisplayList)
            {
                RecordDisplayList.RemoveAll(display => display == null || display.IsDisposed);
                if (RecordDisplayList.Contains(crd)) return;
                RecordDisplayList.Add(crd);
            }
        }

        /// <summary>
        /// 十字线所在图像的宽高（按相机实际分辨率设置），未设置时按 Kind 取默认值（标定 2592x1944，基准点 1280x960）。
        /// </summary>
        public double CrossWidth = 0;
        public double CrossHeight = 0;

        /// <summary>固定工位：工位名即 vpp 目录名。</summary>
        public VpStation(string stationName, VpStationKind kind)
            : this(stationName, kind, 0)
        {
        }

        public VpStation(string stationName, VpStationKind kind, int defaultCcdIndex)
        {
            Kind = kind;
            ApplyKindDefaults();

            SetVppFolder(stationName);
            CameraName = stationName ?? "";

            RunLiveCCDIndex = defaultCcdIndex;
            VisionRunDisplayIndex = 0;
            AutoVisionRunDisplay = true;
        }

        /// <summary>相机下的 VPP：vpp 目录为"相机名\VPP名"（类型为 Custom）。</summary>
        public VpStation(string cameraName, string vppName)
            : this(cameraName, vppName, VpStationKind.Custom)
        {
        }

        /// <summary>
        /// 相机下的 VPP：vpp 目录为"相机名\VPP名"，并指定工位类型。标定站需传 <see cref="VpStationKind.Calibration"/>，
        /// VisionRun() 才会填写 <see cref="Calibration"/> 结果。
        /// </summary>
        public VpStation(string cameraName, string vppName, VpStationKind kind)
        {
            CameraName = cameraName ?? "";
            VppName = vppName ?? "";

            Kind = kind;
            ApplyKindDefaults();

            SetVppFolder(CameraName + "\\" + VppName);

            VisionRunDisplayIndex = 0;
            AutoVisionRunDisplay = true;
        }

        /// <summary>按工位类型设置默认十字线尺寸（未手动设置时生效）。</summary>
        private void ApplyKindDefaults()
        {
            if (Kind == VpStationKind.Calibration)
            {
                // 标定：2592x1944
                CrossWidth = 2592;
                CrossHeight = 1944;
            }
            else
            {
                // 基准点：1280x960
                CrossWidth = 1280;
                CrossHeight = 960;
            }
        }

        /// <summary>空闲时调整工位的输出契约；生产定位必须读取 x、y、A。</summary>
        [AlphaRap.FlowProgramming.FlowToolIgnore]
        public bool TrySetKind(VpStationKind kind, out string error)
        {
            lock (_operationLock)
            {
                error = "";
                if (IsBusy) { error = MiddleLayer.LangMsg("Vision", "error_StationBusy", "工位正在运行或编辑。", "Station is running or being edited.", "La estación está ejecutándose o en edición."); return false; }
                Kind = kind;
                IsAccept = false;
                ApplyKindDefaults();
                return true;
            }
        }

        /// <summary>检查生产定位算法是否声明数值类型的 x、y、A 输出。</summary>
        [AlphaRap.FlowProgramming.FlowToolIgnore]
        public bool ValidateFiducialOutputs(out string error)
        {
            lock (_operationLock)
            {
                error = "";
                if (TB == null || !IsLoadTBOk) { error = MiddleLayer.LangMsg("Vision", "error_PositioningNotLoaded", "定位算法尚未加载。", "Positioning algorithm is not loaded.", "El algoritmo de posicionamiento no está cargado."); return false; }
                foreach (string name in new[] { "x", "y", "A" })
                {
                    try
                    {
                        Type type = TB.Outputs[name].ValueType;
                        if (type == typeof(double) || type == typeof(float) || type == typeof(int) || type == typeof(long)
                            || type == typeof(short) || type == typeof(decimal)) continue;
                        error = MiddleLayer.LangMsg("Vision", "error_OutputType", "定位输出必须为数值类型：", "Positioning output must be numeric: ", "La salida de posicionamiento debe ser numérica: ") + name;
                        return false;
                    }
                    catch (Exception) { error = MiddleLayer.LangMsg("Vision", "error_OutputMissing", "定位算法缺少输出：", "Positioning output is missing: ", "Falta una salida de posicionamiento: ") + name; return false; }
                }
                return true;
            }
        }

        /// <summary>指定 vpp 目录名。</summary>
        [AlphaRap.FlowProgramming.FlowToolIgnore]
        public void SetVppFolder(string folderName)
        {
            VppFolderName = folderName ?? "";
            DisplayName = string.IsNullOrEmpty(VppFolderName) ? GetType().Name : VppFolderName;
        }

        /// <summary>更新显示归属；指定存储目录时，改名保持原算法路径。</summary>
        [AlphaRap.FlowProgramming.FlowToolIgnore]
        public void SetOwner(string cameraName, string vppName, string storageFolder = null)
        {
            CameraName = cameraName ?? "";
            VppName = vppName ?? "";

            // vpp 路径：VisionData\{相机名}\{VPP名}\{配方名}.vpp
            SetVppFolder(storageFolder ?? CameraName + "\\" + VppName);
            DisplayName = CameraName + " / " + VppName;
        }

        /// <summary>
        /// 按工位类型读取有限数值输出；补偿超限直接拒绝，保留原始坐标供诊断。
        /// </summary>
        public override void VisionRun()
        {
            IsAccept = false;
            TB.Run();
            if (TB.RunStatus.Result != Cognex.VisionPro.CogToolResultConstants.Accept) return;
            if (Kind == VpStationKind.Custom) { IsAccept = true; return; }

            double x = ReadPositionOutput("x");
            double y = ReadPositionOutput("y");
            double angle = Kind == VpStationKind.Fiducial ? ReadPositionOutput("A") : 0;
            if (Kind == VpStationKind.Calibration)
            {
                Calibration.x = x;
                Calibration.y = y;
            }
            else { Fiducial.x = x; Fiducial.y = y; Fiducial.u = angle; }

            ValidateCompensation("X", x);
            ValidateCompensation("Y", y);
            if (Kind == VpStationKind.Fiducial) ValidateCompensation(Config != null && Config.GetCompLimit("R") != null ? "R" : "角度", angle);
            IsAccept = true;
        }

        /// <summary>读取定位数值，缺失、非数值或无穷值均视为算法错误。</summary>
        private double ReadPositionOutput(string name)
        {
            object output = GetOutput(name);
            if (output == null) throw new InvalidOperationException(MiddleLayer.LangMsg("Vision", "error_NoOutput", "缺少视觉输出：", "Missing vision output: ", "Falta una salida de visión: ") + name);
            double value = Convert.ToDouble(output, CultureInfo.InvariantCulture);
            if (double.IsNaN(value) || double.IsInfinity(value)) throw new InvalidOperationException(MiddleLayer.LangMsg("Vision", "error_NonFinite", "视觉输出不是有限数值：", "Vision output is not finite: ", "La salida de visión no es un número finito: ") + name);
            return value;
        }

        /// <summary>检查已配置的补偿限制；无对应项时保持调用方自定义判定，非法限制或超限返回运行错误。</summary>
        private void ValidateCompensation(string name, double value)
        {
            double min, max;
            if (!TryGetCompLimit(name, out min, out max))
            {
                if (Config != null && Kind == VpStationKind.Fiducial) throw new InvalidOperationException(MiddleLayer.LangMsg("Vision", "error_NoLimit", "缺少定位补偿限制：", "Missing positioning compensation limit: ", "Falta el límite de compensación de posicionamiento: ") + name);
                return;
            }
            if (double.IsNaN(min) || double.IsInfinity(min) || double.IsNaN(max) || double.IsInfinity(max) || min > max)
                throw new InvalidOperationException(MiddleLayer.LangMsg("Vision", "error_InvalidLimit", "补偿限制无效：", "Invalid compensation limit: ", "Límite de compensación no válido: ") + name);
            if (value < min || value > max) throw new InvalidOperationException(MiddleLayer.LangMsg("Vision", "error_LimitExceeded", "视觉补偿超限：", "Vision compensation exceeds limit: ", "La compensación de visión supera el límite: ") + name + "=" + value);
        }

        /// <summary>捕获本次定位坐标，生产流程不再读取随后可能改变的公共位置字段。</summary>
        protected override VisionRunResult CaptureRunResult(long runId)
        {
            if (!IsAccept) return base.CaptureRunResult(runId);
            Pos4D position = Kind == VpStationKind.Calibration ? Calibration : Fiducial;
            return new VisionRunResult(runId, VisionRunState.Succeeded, "", position.x, position.y, position.u);
        }

        /// <summary>在图像中心画十字线（位置为 CrossWidth / CrossHeight 的一半）。</summary>
        public override void CreatCentrelLine(CogRecordDisplay Crd)
        {
            if (Crd == null) return;

            double w = CrossWidth > 0 ? CrossWidth : 1280;
            double h = CrossHeight > 0 ? CrossHeight : 960;

            CogLine vline = new CogLine();
            CogLine hline = new CogLine();
            vline.Color = CogColorConstants.Red;
            hline.Color = CogColorConstants.Red;
            vline.SetFromStartXYEndXY(w / 2, 0, w / 2, h);
            hline.SetFromStartXYEndXY(0, h / 2, w, h / 2);
            Crd.InteractiveGraphics.Add(vline, "vline", true);
            Crd.InteractiveGraphics.Add(hline, "hline", true);
        }
    }

    /// <summary>标定点：像素坐标 ↔ 电机坐标（对应 CogCalibNPointToNPointTool 的一个点对）。</summary>
    public class VpCalibPoint
    {
        public double PixelX;
        public double PixelY;
        public double MotorPosX;
        public double MotorPosY;

        public VpCalibPoint() { }

        public VpCalibPoint(double pixelX, double pixelY, double motorPosX, double motorPosY)
        {
            
            PixelX = pixelX;
            PixelY = pixelY;
            MotorPosX = motorPosX;
            MotorPosY = motorPosY;
        }
    }

    /// <summary>
    /// 相机级标定（一台相机一份，该相机下所有 VPP 共用）：
    ///   · <see cref="Station"/>：拍标定图、给出像素坐标
    ///   · <see cref="Points"/>：标定点（保存在相机配置中，不随配方变化）
    ///   · <see cref="ApplyToStations"/>：把标定点写入各 VPP ToolBlock 中名为 <see cref="ToolName"/> 的 CogCalibNPointToNPointTool
    /// </summary>
    public class VpCalibration
    {
        /// <summary>标定 vpp 的子目录名（在相机目录下）：VisionData\{相机名}\{FolderName}\{配方}.vpp</summary>
        public string FolderName = "Calibration";
        /// <summary>各 VPP 的 ToolBlock 里那个标定工具的名字。</summary>
        public string ToolName = "Calibration";
        /// <summary>标定专用曝光（与各 VPP 的曝光独立），用于标定站的实时取流。</summary>
        public double Exposure = 10;
        /// <summary>标定点。</summary>
        public List<VpCalibPoint> Points = new List<VpCalibPoint>();
        /// <summary>跑起来的视觉站（Kind = Calibration）。</summary>
        public VpStation Station;

        /// <summary>
        /// 最后一次成功「应用到本相机」时的点位指纹（见 <see cref="ComputeSignature"/>）。
        /// 与当前点位不一致即表示「已修改、未应用」；缺少发布记录时不推断为已应用。
        /// </summary>
        public string AppliedSignature = "none";
        public string AppliedRecipe = "";
        public string AppliedCameraSN = "";
        public Dictionary<string, string> AppliedModels = new Dictionary<string, string>(StringComparer.Ordinal);

        /// <summary>确认点位、相机、配方与全部目标算法版本均与发布记录一致。</summary>
        public bool IsAppliedTo(VpCameraConfig camera, string recipe)
        {
            if (camera == null || !IsApplied || camera.Vpps.Count == 0) return false;
            if (AppliedRecipe != recipe || AppliedCameraSN != camera.CameraSN) return false;
            if (AppliedModels.Count != camera.Vpps.Count) return false;
            foreach (VpVppConfig vpp in camera.Vpps)
            {
                string signature;
                VpStation station = vpp.Station;
                if (station == null || !station.IsLoadedFrom(station.GetVppPath(recipe))) return false;
                if (!AppliedModels.TryGetValue(vpp.Id, out signature) || string.IsNullOrEmpty(signature)) return false;
                if (signature != station.ModelSignature) return false;
            }
            return true;
        }

        /// <summary>仅在全部目标已写盘后记录当前配方、相机与算法版本。</summary>
        public void MarkApplied(VpCameraConfig camera, string recipe)
        {
            AppliedSignature = "none";
            AppliedModels.Clear();
            if (camera == null || string.IsNullOrWhiteSpace(camera.CameraSN) || string.IsNullOrWhiteSpace(recipe))
                throw new InvalidOperationException(MiddleLayer.LangMsg("Vision", "error_PublishContext", "发布标定前必须选择相机 SN 和配方。", "Select a camera SN and recipe before publishing calibration.", "Seleccione un SN de cámara y una receta antes de publicar la calibración."));
            foreach (VpVppConfig vpp in camera.Vpps)
            {
                if (vpp.Station == null || string.IsNullOrEmpty(vpp.Station.ModelSignature))
                    throw new InvalidOperationException(MiddleLayer.LangMsg("Vision", "error_TargetUnsaved", "标定目标尚未保存：", "Calibration target has not been saved: ", "El destino de calibración no está guardado: ") + vpp.Name);
                if (!vpp.Station.IsLoadedFrom(vpp.Station.GetVppPath(recipe)))
                    throw new InvalidOperationException(MiddleLayer.LangMsg("Vision", "error_TargetRecipe", "标定目标不是当前配方：", "Calibration target belongs to another recipe: ", "El destino de calibración pertenece a otra receta: ") + vpp.Name);
                AppliedModels.Add(vpp.Id, vpp.Station.ModelSignature);
            }
            AppliedRecipe = recipe;
            AppliedCameraSN = camera.CameraSN;
            AppliedSignature = ComputeSignature();
        }

        /// <summary>记录本次应用的内存算法版本，主保存暂存成功后再替换成实际文件摘要。</summary>
        internal void MarkAppliedDraft(VpCameraConfig camera, string recipe)
        {
            AppliedSignature = "none";
            AppliedModels.Clear();
            foreach (VpVppConfig vpp in camera.Vpps)
                AppliedModels.Add(vpp.Id, "draft:" + vpp.Station.ToolBlockRevision.ToString(CultureInfo.InvariantCulture));
            AppliedRecipe = recipe; AppliedCameraSN = camera.CameraSN; AppliedSignature = ComputeSignature();
        }

        /// <summary>取当前点位指纹（工具名 + 每个点的四个坐标），用于判断点位是否改动过。</summary>
        public string ComputeSignature()
        {
            StringBuilder text = new StringBuilder();
            text.Append(ToolName ?? "").Append('|');

            for (int i = 0; i < Points.Count; i++)
            {
                VpCalibPoint p = Points[i];
                if (p == null) continue;
                text.Append(p.PixelX.ToString("F4", CultureInfo.InvariantCulture)).Append(',')
                    .Append(p.PixelY.ToString("F4", CultureInfo.InvariantCulture)).Append(',')
                    .Append(p.MotorPosX.ToString("F4", CultureInfo.InvariantCulture)).Append(',')
                    .Append(p.MotorPosY.ToString("F4", CultureInfo.InvariantCulture)).Append(';');
            }

            return ShortHash(text.ToString());
        }

        /// <summary>点位是否与最后一次应用的一致；false 表示需要重新点【应用到本相机】。</summary>
        public bool IsApplied
        {
            get { return AppliedSignature == ComputeSignature(); }
        }

        /// <summary>把字符串压成 32 位十六进制摘要，用作点位指纹。</summary>
        private static string ShortHash(string text)
        {
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(text ?? ""));
                StringBuilder hex = new StringBuilder(hash.Length * 2);
                for (int i = 0; i < hash.Length; i++) hex.Append(hash[i].ToString("x2"));
                return hex.ToString();
            }
        }
    }

    /// <summary>VPForm.Cameras.xml 的全部内容。</summary>
    public class VpConfigFile
    {
        public List<VpCameraConfig> Cameras = new List<VpCameraConfig>();
        /// <summary>生产定位使用的 VPP 标识；空表示显式使用兼容内置工位。</summary>
        public string ProductionVppId = "";
        /// <summary>被从界面移除的内置相机页名字，重启后继续隐藏。</summary>
        public List<string> HiddenPages = new List<string>();
        /// <summary>相机名 → 标定模块。</summary>
        public Dictionary<string, VpCalibration> Calibrations = new Dictionary<string, VpCalibration>();

        public VpCalibration GetCalibration(string cameraName)
        {
            if (string.IsNullOrEmpty(cameraName)) return null;
            VpCalibration c;
            return Calibrations.TryGetValue(cameraName, out c) ? c : null;
        }

        /// <summary>按持久化标识查找 VPP 及其所属相机，找不到时不使用其他工位代替。</summary>
        public VpVppConfig FindVpp(string id, out VpCameraConfig camera)
        {
            camera = null;
            if (string.IsNullOrEmpty(id)) return null;
            foreach (VpCameraConfig item in Cameras)
            {
                foreach (VpVppConfig vpp in item.Vpps)
                {
                    if (!string.Equals(vpp.Id, id, StringComparison.Ordinal)) continue;
                    camera = item;
                    return vpp;
                }
            }
            return null;
        }

        /// <summary>拒绝重复或空工位标识，避免生产绑定出现歧义。</summary>
        public void ValidateStationIds()
        {
            HashSet<string> ids = new HashSet<string>(StringComparer.Ordinal);
            HashSet<string> cameraNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            HashSet<string> cameraFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (VpCameraConfig camera in Cameras)
            {
                ValidateStorageFolder(camera.StorageFolder);
                if (!cameraNames.Add(camera.Name) || !cameraFolders.Add(camera.StorageFolder))
                    throw new InvalidOperationException(MiddleLayer.LangMsg("Vision", "error_DuplicateCamera", "相机名称或存储目录重复：", "Duplicate camera name or storage folder: ", "Nombre de cámara o carpeta de almacenamiento duplicados: ") + camera.Name);
                HashSet<string> names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                HashSet<string> folders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (VpVppConfig vpp in camera.Vpps)
                {
                    ValidateStorageFolder(vpp.StorageFolder);
                    if (string.IsNullOrWhiteSpace(vpp.Id) || !ids.Add(vpp.Id))
                        throw new InvalidOperationException(MiddleLayer.LangMsg("Vision", "error_DuplicateId", "VPP 标识为空或重复：", "Empty or duplicate VPP ID: ", "ID de VPP vacío o duplicado: ") + camera.Name + " / " + vpp.Name);
                    if (!names.Add(vpp.Name) || !folders.Add(vpp.StorageFolder))
                        throw new InvalidOperationException(MiddleLayer.LangMsg("Vision", "error_DuplicateVpp", "VPP 名称或存储目录重复：", "Duplicate VPP name or storage folder: ", "Nombre de VPP o carpeta de almacenamiento duplicados: ") + camera.Name + " / " + vpp.Name);
                }
                VpCalibration calibration = GetCalibration(camera.Name);
                if (calibration == null) continue;
                ValidateStorageFolder(calibration.FolderName);
                if (folders.Contains(calibration.FolderName)) throw new InvalidOperationException(MiddleLayer.LangMsg("Vision", "error_SharedFolder", "标定与 VPP 不能共用存储目录：", "Calibration and VPP must not share a storage folder: ", "La calibración y el VPP no pueden compartir carpeta: ") + camera.Name);
            }
        }

        /// <summary>存储目录必须是单个有效目录名，不能逃出视觉数据根目录。</summary>
        private static void ValidateStorageFolder(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || name == "." || name == ".." || name != name.Trim()
                || name.EndsWith(".") || name.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) >= 0)
                throw new InvalidOperationException(MiddleLayer.LangMsg("Vision", "error_InvalidFolder", "视觉存储目录无效：", "Invalid vision storage folder: ", "Carpeta de almacenamiento de visión no válida: ") + name);
        }
    }

    /// <summary>
    /// 相机 / VPP / 标定配置的读写，保存在独立文件 {SettingDataDirectory}\VPForm.Cameras.xml（不使用 ModuleBaseForm.SettingData）。
    ///
    /// 健壮性约定（都是"参数加载不上"的根因）：
    /// · 数字一律按**不受系统区域设置影响**的格式读写（小数点永远是 '.'），
    ///   避免在西班牙语/德语等"逗号小数点"系统上 "10.000" 被读成 10000；
    /// · 读取失败**不再静默返回空表**：把坏文件备份成 *.bad-时间戳，并通过 error 上报；
    /// · 保存先写临时文件再替换，写一半断电也不会留下半截文件。
    /// </summary>
    public static class VpConfigStore
    {
        /// <summary>最近一次保存的错误（成功时为 null）。</summary>
        public static string LastSaveError;

        public static string FilePath
        {
            get
            {
                string dir = SysPara.SettingDataDirectory;
                if (string.IsNullOrEmpty(dir)) dir = @".\ModuleData\SettingData";
                return System.IO.Path.Combine(dir, "VPForm.Cameras.xml");
            }
        }

        /// <summary>
        /// 读配置。<paramref name="error"/> 为 null 表示完全读取成功；非 null 时是失败原因
        /// （此时返回的是**已经解析出来的那部分**，调用方应提示用户并避免覆盖磁盘文件）。
        /// </summary>
        public static VpConfigFile Load(out string error)
        {
            VpConfigFile data = new VpConfigFile();
            error = null;

            string path = FilePath;
            try
            {
                if (!System.IO.File.Exists(path)) return data;

                XmlDocument doc = new XmlDocument();
                doc.Load(path);

                if (doc.SelectSingleNode("VpConfig") == null)
                    throw new XmlException(MiddleLayer.LangMsg("Vision", "error_XmlRoot", "根节点不是 VpConfig", "XML root must be VpConfig", "La raíz XML debe ser VpConfig"));
                data.ProductionVppId = Attr(doc.DocumentElement, "ProductionVppId");

                XmlNode hidden = doc.SelectSingleNode("VpConfig/HiddenPages");
                if (hidden != null)
                    foreach (XmlNode n in hidden.ChildNodes)
                    {
                        string v = Attr(n, "Name");
                        if (v != "" && !data.HiddenPages.Contains(v)) data.HiddenPages.Add(v);
                    }

                XmlNode camRoot = doc.SelectSingleNode("VpConfig/Cameras");
                if (camRoot != null)
                    foreach (XmlNode cn in camRoot.ChildNodes)
                    {
                        string name = Attr(cn, "Name");
                        if (name == "") continue;

                        VpCameraConfig cam = new VpCameraConfig();
                        cam.Name = name;
                        cam.FolderName = Attr(cn, "Folder");
                        if (string.IsNullOrEmpty(cam.FolderName)) cam.FolderName = name;
                        cam.CameraSN = Attr(cn, "SN");
                        cam.Exposure = ReadDoubleAttr(cn, "Exposure", 10);

                        foreach (XmlNode vn in cn.ChildNodes)
                        {
                            string vname = Attr(vn, "Name");
                            if (vname == "") continue;
                            VpVppConfig vpp = new VpVppConfig();
                            vpp.Name = vname;
                            vpp.FolderName = Attr(vn, "Folder");
                            if (string.IsNullOrEmpty(vpp.FolderName)) vpp.FolderName = vname;
                            string id = Attr(vn, "Id");
                            if (!string.IsNullOrEmpty(id)) vpp.Id = id;
                            // 没有 Vpp/@Exposure 时继承相机的曝光
                            vpp.Exposure = ReadDoubleAttr(vn, "Exposure", cam.Exposure);
                            // 存图设置（缺省 = 不存图）
                            vpp.SaveImageEnabled = ReadBoolAttr(vn, "SaveImage", false);
                            string imgPath = Attr(vn, "SaveImagePath");
                            if (imgPath != "") vpp.SaveImagePath = imgPath;
                            vpp.SaveImageOriginal = ReadBoolAttr(vn, "SaveImageOriginal", false);
                            vpp.SaveImageSnapshot = ReadBoolAttr(vn, "SaveImageSnapshot", false);
                            vpp.SaveImageAutoDelete = ReadBoolAttr(vn, "SaveImageAutoDelete", false);
                            vpp.SaveImageKeepDays = ReadIntAttr(vn, "SaveImageKeepDays", 0);
                            // 补偿配置按 XML 整表读取；旧配置缺少补偿值时使用 0。
                            vpp.CompLimits.Clear();
                            foreach (XmlNode ln in vn.ChildNodes)
                            {
                                if (ln.Name != "Comp") continue;
                                string ci = Attr(ln, "Item");
                                if (ci == "") continue;
                                VpCompLimitItem item = new VpCompLimitItem();
                                item.Item = ci;
                                item.Compensation = ReadDoubleAttr(ln, "Compensation", 0);
                                item.Min = ReadDoubleAttr(ln, "Min", 0);
                                item.Max = ReadDoubleAttr(ln, "Max", 0);
                                vpp.CompLimits.Add(item);
                            }
                            vpp.EnsureCompLimits();
                            cam.Vpps.Add(vpp);
                        }
                        data.Cameras.Add(cam);
                    }

                XmlNode calRoot = doc.SelectSingleNode("VpConfig/Calibrations");
                if (calRoot != null)
                    foreach (XmlNode cn in calRoot.ChildNodes)
                    {
                        string camName = Attr(cn, "Camera");
                        if (camName == "") continue;

                        VpCalibration calib = new VpCalibration();
                        string folder = Attr(cn, "FolderName");
                        if (folder != "") calib.FolderName = folder;
                        string tool = Attr(cn, "ToolName");
                        if (tool != "") calib.ToolName = tool;
                        // 没有 Calibration/@Exposure 时使用相机的默认曝光（未设置时为 10）
                        double camExp = 10;
                        for (int i = 0; i < data.Cameras.Count; i++)
                            if (data.Cameras[i] != null && data.Cameras[i].Name == camName) { camExp = data.Cameras[i].Exposure; break; }
                        calib.Exposure = ReadDoubleAttr(cn, "Exposure", camExp);

                        foreach (XmlNode pn in cn.SelectNodes("Point"))
                            calib.Points.Add(new VpCalibPoint(
                                ReadDoubleAttr(pn, "PixelX", 0),
                                ReadDoubleAttr(pn, "PixelY", 0),
                                ReadDoubleAttr(pn, "MotorPosX", 0),
                                ReadDoubleAttr(pn, "MotorPosY", 0)));

                        // 缺少发布上下文的配置保留点位，但不推断目标算法已应用。
                        string applied = Attr(cn, "Applied");
                        calib.AppliedSignature = string.IsNullOrEmpty(applied) ? "none" : applied;
                        calib.AppliedRecipe = Attr(cn, "AppliedRecipe");
                        calib.AppliedCameraSN = Attr(cn, "AppliedSN");
                        foreach (XmlNode target in cn.SelectNodes("Target"))
                            calib.AppliedModels.Add(Attr(target, "VppId"), Attr(target, "Model"));

                        data.Calibrations[camName] = calib;
                    }
                data.ValidateStationIds();
            }
            catch (Exception ex)
            {
                // 不再静默吞掉：备份坏文件（防止随后的保存把原始参数冲掉），并把原因交给界面显示
                error = MiddleLayer.LangMsg("Vision", "error_ReadA", "读取 ", "Read ", "Leer ") + System.IO.Path.GetFileName(path) + MiddleLayer.LangMsg("Vision", "error_FileFailed", " 失败：", " failed: ", " falló: ") + ex.Message;
                string bak = BackupBrokenFile(path);
                if (!string.IsNullOrEmpty(bak)) error += MiddleLayer.LangMsg("Vision", "error_Backup", "\r\n原文件已备份为：", "\r\nOriginal file backed up to: ", "\r\nCopia de seguridad del archivo original: ") + bak;
                System.Diagnostics.Debug.WriteLine("[VPForm] " + error);
            }

            return data;
        }

        /// <summary>读配置（不关心失败原因）。</summary>
        public static VpConfigFile Load()
        {
            string error;
            return Load(out error);
        }

        /// <summary>把读不通的配置文件复制一份 *.bad-时间戳，返回备份路径（失败返回 null）。</summary>
        private static string BackupBrokenFile(string path)
        {
            try
            {
                if (!System.IO.File.Exists(path)) return null;
                string bak = System.IO.Path.Combine(
                    System.IO.Path.GetDirectoryName(path) ?? ".",
                    System.IO.Path.GetFileNameWithoutExtension(path)
                        + ".bad-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".xml");
                System.IO.File.Copy(path, bak, true);
                return bak;
            }
            catch (Exception) { return null; }
        }

        /// <summary>复制完整视觉配置与草稿值，保留实际视觉站引用供放弃修改时恢复。</summary>
        internal static VpConfigFile Copy(VpConfigFile data)
        {
            VpConfigFile result = new VpConfigFile { ProductionVppId = data.ProductionVppId, HiddenPages = new List<string>(data.HiddenPages) };
            foreach (VpCameraConfig camera in data.Cameras)
            {
                VpCameraConfig copy = new VpCameraConfig { Name = camera.Name, FolderName = camera.FolderName, CameraSN = camera.CameraSN, Exposure = camera.Exposure };
                foreach (VpVppConfig vpp in camera.Vpps)
                {
                    VpVppConfig item = new VpVppConfig { Id = vpp.Id, Name = vpp.Name, FolderName = vpp.FolderName, Exposure = vpp.Exposure, Station = vpp.Station,
                        SaveImageEnabled = vpp.SaveImageEnabled, SaveImagePath = vpp.SaveImagePath, SaveImageOriginal = vpp.SaveImageOriginal,
                        SaveImageSnapshot = vpp.SaveImageSnapshot, SaveImageAutoDelete = vpp.SaveImageAutoDelete, SaveImageKeepDays = vpp.SaveImageKeepDays };
                    foreach (VpCompLimitItem limit in vpp.CompLimits)
                        item.CompLimits.Add(new VpCompLimitItem(limit.Item, limit.Min, limit.Max) { Compensation = limit.Compensation });
                    copy.Vpps.Add(item);
                }
                result.Cameras.Add(copy);
            }
            foreach (KeyValuePair<string, VpCalibration> pair in data.Calibrations)
            {
                VpCalibration source = pair.Value;
                VpCalibration copy = new VpCalibration { FolderName = source.FolderName, ToolName = source.ToolName, Exposure = source.Exposure, Station = source.Station,
                    AppliedSignature = source.AppliedSignature, AppliedRecipe = source.AppliedRecipe, AppliedCameraSN = source.AppliedCameraSN,
                    AppliedModels = new Dictionary<string, string>(source.AppliedModels, StringComparer.Ordinal) };
                foreach (VpCalibPoint point in source.Points)
                    copy.Points.Add(new VpCalibPoint { PixelX = point.PixelX, PixelY = point.PixelY, MotorPosX = point.MotorPosX, MotorPosY = point.MotorPosY });
                result.Calibrations.Add(pair.Key, copy);
            }
            return result;
        }

        /// <summary>显式写入独立视觉配置文件；程序配置编辑由主保存事务调用 Write。</summary>
        public static void Save(VpConfigFile data)
        {
            if (data == null) return;
            LastSaveError = null;
            string path = FilePath;
            string tmp = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                Write(data, tmp);
                ReplaceFile(tmp, path);
            }
            catch (Exception ex)
            {
                LastSaveError = MiddleLayer.LangMsg("Vision", "error_WriteA", "写入 ", "Write ", "Escribir ") + System.IO.Path.GetFileName(path) + MiddleLayer.LangMsg("Vision", "error_FileFailed", " 失败：", " failed: ", " falló: ") + ex.Message;
                System.Diagnostics.Debug.WriteLine("[VPForm] " + LastSaveError);
                try { if (System.IO.File.Exists(tmp)) System.IO.File.Delete(tmp); } catch (Exception) { }
            }
        }

        /// <summary>只将指定配置快照写入暂存路径；失败直接抛出供主保存整体回滚。</summary>
        internal static void Write(VpConfigFile data, string path)
        {
                data.ValidateStationIds();
                string dir = System.IO.Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir) && !System.IO.Directory.Exists(dir))
                    System.IO.Directory.CreateDirectory(dir);

                XmlDocument doc = new XmlDocument();
                XmlElement root = doc.CreateElement("VpConfig");
                root.SetAttribute("ProductionVppId", data.ProductionVppId ?? "");
                doc.AppendChild(root);

                XmlElement camRoot = doc.CreateElement("Cameras");
                root.AppendChild(camRoot);
                if (data.Cameras != null)
                    for (int i = 0; i < data.Cameras.Count; i++)
                    {
                        VpCameraConfig cam = data.Cameras[i];
                        if (cam == null || string.IsNullOrEmpty(cam.Name)) continue;

                        XmlElement ce = doc.CreateElement("Camera");
                        ce.SetAttribute("Name", cam.Name);
                        ce.SetAttribute("Folder", cam.StorageFolder);
                        // 相机只按 SN 绑定，不再保存枚举下标
                        ce.SetAttribute("SN", cam.CameraSN ?? "");
                        ce.SetAttribute("Exposure", cam.Exposure.ToString("F3", CultureInfo.InvariantCulture));
                        camRoot.AppendChild(ce);

                        for (int j = 0; j < cam.Vpps.Count; j++)
                        {
                            if (cam.Vpps[j] == null || string.IsNullOrEmpty(cam.Vpps[j].Name)) continue;
                            XmlElement ve = doc.CreateElement("Vpp");
                            ve.SetAttribute("Id", cam.Vpps[j].Id);
                            ve.SetAttribute("Name", cam.Vpps[j].Name);
                            ve.SetAttribute("Folder", cam.Vpps[j].StorageFolder);
                            ve.SetAttribute("Exposure", cam.Vpps[j].Exposure.ToString("F3", CultureInfo.InvariantCulture));
                                ve.SetAttribute("SaveImage", cam.Vpps[j].SaveImageEnabled.ToString());
                                ve.SetAttribute("SaveImagePath", cam.Vpps[j].SaveImagePath ?? "");
                                if (cam.Vpps[j].SaveImageOriginal) ve.SetAttribute("SaveImageOriginal", "True");
                                if (cam.Vpps[j].SaveImageSnapshot) ve.SetAttribute("SaveImageSnapshot", "True");
                                if (cam.Vpps[j].SaveImageAutoDelete) ve.SetAttribute("SaveImageAutoDelete", "True");
                                ve.SetAttribute("SaveImageKeepDays", cam.Vpps[j].SaveImageKeepDays.ToString(CultureInfo.InvariantCulture));
                            ce.AppendChild(ve);

                            // 视觉定位补偿限制（X / Y / Angle）
                            if (cam.Vpps[j].CompLimits != null)
                                for (int k = 0; k < cam.Vpps[j].CompLimits.Count; k++)
                                {
                                    VpCompLimitItem it = cam.Vpps[j].CompLimits[k];
                                    if (it == null || string.IsNullOrEmpty(it.Item)) continue;
                                    XmlElement le = doc.CreateElement("Comp");
                                    le.SetAttribute("Item", it.Item);
                                    le.SetAttribute("Compensation", it.Compensation.ToString("R", CultureInfo.InvariantCulture));
                                    le.SetAttribute("Min", it.Min.ToString("F4", CultureInfo.InvariantCulture));
                                    le.SetAttribute("Max", it.Max.ToString("F4", CultureInfo.InvariantCulture));
                                    ve.AppendChild(le);
                                }
                        }
                    }

                XmlElement calRoot = doc.CreateElement("Calibrations");
                root.AppendChild(calRoot);
                if (data.Calibrations != null)
                    foreach (KeyValuePair<string, VpCalibration> kv in data.Calibrations)
                    {
                        if (string.IsNullOrEmpty(kv.Key) || kv.Value == null) continue;

                        XmlElement ce = doc.CreateElement("Calibration");
                        ce.SetAttribute("Camera", kv.Key);
                        ce.SetAttribute("FolderName", kv.Value.FolderName);
                        ce.SetAttribute("ToolName", kv.Value.ToolName);
                        ce.SetAttribute("Exposure", kv.Value.Exposure.ToString("F3", CultureInfo.InvariantCulture));
                        ce.SetAttribute("Applied", kv.Value.AppliedSignature ?? "");
                        ce.SetAttribute("AppliedRecipe", kv.Value.AppliedRecipe ?? "");
                        ce.SetAttribute("AppliedSN", kv.Value.AppliedCameraSN ?? "");
                        calRoot.AppendChild(ce);
                        foreach (KeyValuePair<string, string> target in kv.Value.AppliedModels)
                        {
                            XmlElement te = doc.CreateElement("Target");
                            te.SetAttribute("VppId", target.Key);
                            te.SetAttribute("Model", target.Value);
                            ce.AppendChild(te);
                        }

                        for (int i = 0; i < kv.Value.Points.Count; i++)
                        {
                            VpCalibPoint p = kv.Value.Points[i];
                            if (p == null) continue;
                            XmlElement pe = doc.CreateElement("Point");
                            pe.SetAttribute("PixelX", p.PixelX.ToString("F4", CultureInfo.InvariantCulture));
                            pe.SetAttribute("PixelY", p.PixelY.ToString("F4", CultureInfo.InvariantCulture));
                            pe.SetAttribute("MotorPosX", p.MotorPosX.ToString("F4", CultureInfo.InvariantCulture));
                            pe.SetAttribute("MotorPosY", p.MotorPosY.ToString("F4", CultureInfo.InvariantCulture));
                            ce.AppendChild(pe);
                        }
                    }

                XmlElement hideRoot = doc.CreateElement("HiddenPages");
                root.AppendChild(hideRoot);
                if (data.HiddenPages != null)
                    for (int i = 0; i < data.HiddenPages.Count; i++)
                    {
                        if (string.IsNullOrEmpty(data.HiddenPages[i])) continue;
                        XmlElement pe = doc.CreateElement("Page");
                        pe.SetAttribute("Name", data.HiddenPages[i]);
                        hideRoot.AppendChild(pe);
                    }

                // 只写调用方提供的事务暂存文件，正式文件由主保存统一替换。
                System.Xml.XmlWriterSettings ws = new System.Xml.XmlWriterSettings();
                ws.Indent = true;
                ws.Encoding = new System.Text.UTF8Encoding(false);
                using (System.Xml.XmlWriter w = System.Xml.XmlWriter.Create(path, ws))
                    doc.Save(w);
        }

        /// <summary>用同目录临时文件替换正式文件；替换失败保留原配置并向调用方报告。</summary>
        private static void ReplaceFile(string tmp, string path)
        {
            if (System.IO.File.Exists(path)) System.IO.File.Replace(tmp, path, null);
            else System.IO.File.Move(tmp, path);
        }

        private static string Attr(XmlNode n, string name)
        {
            try
            {
                if (n == null || n.Attributes == null) return "";
                XmlAttribute a = n.Attributes[name];
                return (a == null) ? "" : a.Value;
            }
            catch (Exception) { return ""; }
        }

        private static int ReadIntAttr(XmlNode n, string name, int def)
        {
            int v;
            return int.TryParse(Attr(n, name), NumberStyles.Integer, CultureInfo.InvariantCulture, out v) ? v : def;
        }

        private static double ReadDoubleAttr(XmlNode n, string name, double def)
        {
            double v;
            return TryParseFileDouble(Attr(n, name), out v) ? v : def;
        }

        private static bool ReadBoolAttr(XmlNode n, string name, bool def)
        {
            bool v;
            return bool.TryParse(Attr(n, name), out v) ? v : def;
        }

        /// <summary>
        /// 解析配置文件里的数字：先按 InvariantCulture（新文件总是用 '.' 小数点），
        /// 读不到再按当前区域（兼容旧版本在"逗号小数点"系统上写出的文件）。
        /// 只用 NumberStyles.Float：不允许千分位，避免 "10,5" 被当成 105。
        /// </summary>
        public static bool TryParseFileDouble(string text, out double value)
        {
            value = 0;
            string s = (text ?? "").Trim();
            if (s.Length == 0) return false;

            if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                return !double.IsNaN(value) && !double.IsInfinity(value);
            return double.TryParse(s, NumberStyles.Float, CultureInfo.CurrentCulture, out value)
                && !double.IsNaN(value) && !double.IsInfinity(value);
        }

        /// <summary>
        /// 解析界面输入框里的数字：先按当前区域（用户习惯），再按 InvariantCulture（允许用 '.' 输入）。
        /// 同样不接受千分位。
        /// </summary>
        public static bool TryParseInputDouble(string text, out double value)
        {
            value = 0;
            string s = (text ?? "").Trim();
            if (s.Length == 0) return false;

            if (double.TryParse(s, NumberStyles.Float, CultureInfo.CurrentCulture, out value))
                return !double.IsNaN(value) && !double.IsInfinity(value);
            return double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out value)
                && !double.IsNaN(value) && !double.IsInfinity(value);
        }
    }
}
