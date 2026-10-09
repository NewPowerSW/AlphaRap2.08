using System;
using System.Collections.Generic;
using System.Xml;
using Cognex.VisionPro;          // CogRecordDisplay / CogLine / CogColorConstants

namespace AlphaRap
{
    /// <summary>相机配置（对应 VPForm 的 tabControl1 中的一页），属于机器配置，不随配方变化。</summary>
    public class VpCameraConfig
    {
        /// <summary>相机名（也是该相机所有 vpp 的一级目录名）。</summary>
        public string Name = "";
        /// <summary>Cognex 采集通道号（CogFrameGrabbers 的下标）。</summary>
        public int CameraIndex = 0;
        /// <summary>
        /// 默认曝光：新建 VPP / 添加标定时作为初始值。取流使用 VPP 自己的曝光（<see cref="VpVppConfig.Exposure"/>）
        /// 或标定的曝光（<see cref="VpCalibration.Exposure"/>）。
        /// </summary>
        public double Exposure = 10;
        /// <summary>该相机下的 VPP 列表（每个 VPP = 相机页里 VPP 子 TabControl 的一页）。</summary>
        public List<VpVppConfig> Vpps = new List<VpVppConfig>();

        /// <summary>
        /// 按 VPP 名取补偿限制表，没有该 VPP 返回 null。用法：cam.GetCompLimits("VPP1")，遍历 Item/Min/Max。
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
        public string Name = "";
        /// <summary>
        /// 本 VPP 取流用的曝光时间（各 VPP 独立，与标定曝光 <see cref="VpCalibration.Exposure"/> 互不影响）；
        /// 新建时继承相机的默认曝光。
        /// </summary>
        public double Exposure = 10;
        /// <summary>
        /// 视觉定位补偿限制表（VPP 页上可增删、改名），每行 = 名称 + 下限 + 上限。
        /// 视觉补偿量按名称匹配行后夹到 [Min, Max]，防止异常定位使机器偏移。
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
        /// 补偿限制表为空时补三行默认值（X / Y / 角度）。仅在读取无 Comp 节点的配置和首次建页时调用。
        /// </summary>
        public void EnsureCompLimits()
        {
            if (CompLimits.Count > 0) return;
            CompLimits.Add(new VpCompLimitItem("X", -1, 1));
            CompLimits.Add(new VpCompLimitItem("Y", -1, 1));
            CompLimits.Add(new VpCompLimitItem("角度", -5, 5));
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

    /// <summary>补偿限制表的一行：名称 + 上下限，单位与视觉输出一致（X/Y 一般为 mm，角度为度）。</summary>
    public class VpCompLimitItem
    {
        public string Item = "";
        public double Min = -1;
        public double Max = 1;

        public VpCompLimitItem() { }
        public VpCompLimitItem(string item, double min, double max) { Item = item; Min = min; Max = max; }
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
    public class VpStation : VisionproInterface
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
        public bool RunAndWait(int timeoutMs)
        {
            RunTB();
            DateTime dead = DateTime.Now.AddMilliseconds(timeoutMs);
            while (DateTime.Now < dead)
            {
                if (RunTBOk()) break;
                System.Threading.Thread.Sleep(10);
            }
            if (!RunTBOk()) return false;
            if (!IsAccept) return false;

            if (Config != null && Config.SaveImageEnabled) SaveLastImage();
            return true;
        }

        /// <summary>按行名（补偿限制表"项目"列）取本 VPP 的补偿限制；没有该行返回 false。</summary>
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
        public string SaveLastImage()
        {
            if (Config == null || !Config.SaveImageEnabled) return "";
            if (TB == null || !IsLoadTBOk) return "";
            if (!Config.SaveImageOriginal && !Config.SaveImageSnapshot) return "";

            try
            {
                Cognex.VisionPro.ICogRecord root = TB.CreateLastRunRecord();   // CreateLastRunRecord 返回的是 ICogRecord 接口
                if (root == null) return "";

                string dir = SaveImageDirectory();
                if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);
                string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
                string last = "";

                if (Config.SaveImageOriginal)
                    last = WriteImageFile(root.Content as Cognex.VisionPro.ICogImage, dir, stamp + "_original.bmp");
                if (Config.SaveImageSnapshot && root.SubRecords.Count > VisionRunDisplayIndex)
                    last = WriteImageFile(root.SubRecords[VisionRunDisplayIndex].Content as Cognex.VisionPro.ICogImage, dir, stamp + "_snap.bmp");

                CleanOldImages(dir);
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
        private string SaveImageDirectory()
        {
            if (!string.IsNullOrEmpty(Config.SaveImagePath)) return Config.SaveImagePath;
            try
            {
                return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(GetVppPath(SysPara.RecipeName)), "Images");
            }
            catch (Exception) { return ".\\Images"; }
        }

        /// <summary>
        /// 启用自动删除时，按保留天数删除存图目录中的旧 *.bmp（每次存图时执行；天数 &lt;= 0 时不删除）。
        /// </summary>
        private void CleanOldImages(string dir)
        {
            if (!Config.SaveImageAutoDelete) return;
            if (Config.SaveImageKeepDays <= 0) return;
            try
            {
                DateTime dead = DateTime.Now.AddDays(-Config.SaveImageKeepDays);
                string[] files = System.IO.Directory.GetFiles(dir, "*.bmp");
                for (int i = 0; i < files.Length; i++)
                    if (System.IO.File.GetLastWriteTime(files[i]) < dead)
                        System.IO.File.Delete(files[i]);
            }
            catch (Exception) { }
        }

        /// <summary>把另一个 CogRecordDisplay 绑定到本站，拍照和实时画面也会显示到它上面（按引用去重）。</summary>
        public void BindDisplay(Cognex.VisionPro.CogRecordDisplay crd)
        {
            if (crd == null) return;
            for (int i = 0; i < RecordDisplayList.Count; i++)
                if (ReferenceEquals(RecordDisplayList[i], crd)) return;
            RecordDisplayList.Add(crd);
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

        /// <summary>指定 vpp 目录名。</summary>
        public void SetVppFolder(string folderName)
        {
            VppFolderName = folderName ?? "";
            DisplayName = string.IsNullOrEmpty(VppFolderName) ? GetType().Name : VppFolderName;
        }

        /// <summary>重设所属相机和 VPP（相机或 VPP 改名后调用，使 vpp 指向新目录）。</summary>
        public void SetOwner(string cameraName, string vppName)
        {
            CameraName = cameraName ?? "";
            VppName = vppName ?? "";

            // vpp 路径：VisionData\{相机名}\{VPP名}\{配方名}.vpp
            SetVppFolder(CameraName + "\\" + VppName);
        }

        /// <summary>
        /// 按工位类型读取 ToolBlock 输出并做补偿限制。取输出失败时抛出异常，由 RunTB() 报警（2009）。
        /// </summary>
        public override void VisionRun()
        {
            TB.Run();
            if (TB.RunStatus.Result == Cognex.VisionPro.CogToolResultConstants.Accept)
            {
                if (Kind == VpStationKind.Calibration)
                {
                    Calibration.x = (double)GetOutput("x");
                    Calibration.y = (double)GetOutput("y");
                }
                else if (Kind == VpStationKind.Fiducial)
                {
                    Fiducial.x = (double)GetOutput("x");
                    Fiducial.y = (double)GetOutput("y");
                    Fiducial.u = (double)GetOutput("A");
                }

                // 按 VPP 页的补偿限制表（X / Y / 角度）夹取结果，超限的值设为边界；没有对应行或未关联配置时不限制。
                string r;
                if (Kind == VpStationKind.Calibration)
                {
                    ClampComp("X", ref Calibration.x, out r);
                    ClampComp("Y", ref Calibration.y, out r);
                }
                else if (Kind == VpStationKind.Fiducial)
                {
                    ClampComp("X", ref Fiducial.x, out r);
                    ClampComp("Y", ref Fiducial.y, out r);
                    ClampComp("角度", ref Fiducial.u, out r);
                }

                IsAccept = true;
            }
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
    }

    /// <summary>VPForm.Cameras.xml 的全部内容。</summary>
    public class VpConfigFile
    {
        public List<VpCameraConfig> Cameras = new List<VpCameraConfig>();
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
    }

    /// <summary>
    /// 相机 / VPP / 标定配置的读写，保存在独立文件 {SettingDataDirectory}\VPForm.Cameras.xml（不使用 ModuleBaseForm.SettingData）。
    /// </summary>
    public static class VpConfigStore
    {
        public static string FilePath
        {
            get
            {
                string dir = SysPara.SettingDataDirectory;
                if (string.IsNullOrEmpty(dir)) dir = @".\ModuleData\SettingData";
                return System.IO.Path.Combine(dir, "VPForm.Cameras.xml");
            }
        }

        public static VpConfigFile Load()
        {
            VpConfigFile data = new VpConfigFile();

            try
            {
                string path = FilePath;
                if (!System.IO.File.Exists(path)) return data;

                XmlDocument doc = new XmlDocument();
                doc.Load(path);

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
                        cam.CameraIndex = ReadIntAttr(cn, "CameraIndex", 0);
                        cam.Exposure = ReadDoubleAttr(cn, "Exposure", 10);

                        foreach (XmlNode vn in cn.ChildNodes)
                        {
                            string vname = Attr(vn, "Name");
                            if (vname == "") continue;
                            VpVppConfig vpp = new VpVppConfig();
                            vpp.Name = vname;
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
                            // 补偿限制：按 XML（Vpp/Comp[@Item/@Min/@Max]）整表重建；没有任何 Comp 节点时补默认三行
                            vpp.CompLimits.Clear();
                            foreach (XmlNode ln in vn.ChildNodes)
                            {
                                if (ln.Name != "Comp") continue;
                                string ci = Attr(ln, "Item");
                                if (ci == "") continue;
                                VpCompLimitItem item = new VpCompLimitItem();
                                item.Item = ci;
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

                        foreach (XmlNode pn in cn.ChildNodes)
                            calib.Points.Add(new VpCalibPoint(
                                ReadDoubleAttr(pn, "PixelX", 0),
                                ReadDoubleAttr(pn, "PixelY", 0),
                                ReadDoubleAttr(pn, "MotorPosX", 0),
                                ReadDoubleAttr(pn, "MotorPosY", 0)));

                        data.Calibrations[camName] = calib;
                    }
            }
            catch (Exception) { }

            return data;
        }

        public static void Save(VpConfigFile data)
        {
            if (data == null) return;

            try
            {
                string path = FilePath;
                string dir = System.IO.Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir) && !System.IO.Directory.Exists(dir))
                    System.IO.Directory.CreateDirectory(dir);

                XmlDocument doc = new XmlDocument();
                XmlElement root = doc.CreateElement("VpConfig");
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
                        ce.SetAttribute("CameraIndex", cam.CameraIndex.ToString());
                        ce.SetAttribute("Exposure", cam.Exposure.ToString("F3"));
                        camRoot.AppendChild(ce);

                        for (int j = 0; j < cam.Vpps.Count; j++)
                        {
                            if (cam.Vpps[j] == null || string.IsNullOrEmpty(cam.Vpps[j].Name)) continue;
                            XmlElement ve = doc.CreateElement("Vpp");
                            ve.SetAttribute("Name", cam.Vpps[j].Name);
                            ve.SetAttribute("Exposure", cam.Vpps[j].Exposure.ToString("F3"));
                            if (cam.Vpps[j].SaveImageEnabled)
                            {
                                ve.SetAttribute("SaveImage", "True");
                                ve.SetAttribute("SaveImagePath", cam.Vpps[j].SaveImagePath ?? "");
                                if (cam.Vpps[j].SaveImageOriginal) ve.SetAttribute("SaveImageOriginal", "True");
                                if (cam.Vpps[j].SaveImageSnapshot) ve.SetAttribute("SaveImageSnapshot", "True");
                                if (cam.Vpps[j].SaveImageAutoDelete) ve.SetAttribute("SaveImageAutoDelete", "True");
                                ve.SetAttribute("SaveImageKeepDays", cam.Vpps[j].SaveImageKeepDays.ToString());
                            }
                            ce.AppendChild(ve);

                            // 视觉定位补偿限制（X / Y / Angle）
                            if (cam.Vpps[j].CompLimits != null)
                                for (int k = 0; k < cam.Vpps[j].CompLimits.Count; k++)
                                {
                                    VpCompLimitItem it = cam.Vpps[j].CompLimits[k];
                                    if (it == null || string.IsNullOrEmpty(it.Item)) continue;
                                    XmlElement le = doc.CreateElement("Comp");
                                    le.SetAttribute("Item", it.Item);
                                    le.SetAttribute("Min", it.Min.ToString("F4"));
                                    le.SetAttribute("Max", it.Max.ToString("F4"));
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
                        ce.SetAttribute("Exposure", kv.Value.Exposure.ToString("F3"));
                        calRoot.AppendChild(ce);

                        for (int i = 0; i < kv.Value.Points.Count; i++)
                        {
                            VpCalibPoint p = kv.Value.Points[i];
                            if (p == null) continue;
                            XmlElement pe = doc.CreateElement("Point");
                            pe.SetAttribute("PixelX", p.PixelX.ToString("F4"));
                            pe.SetAttribute("PixelY", p.PixelY.ToString("F4"));
                            pe.SetAttribute("MotorPosX", p.MotorPosX.ToString("F4"));
                            pe.SetAttribute("MotorPosY", p.MotorPosY.ToString("F4"));
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

                // 以 UTF-8 编码写入
                System.Xml.XmlWriterSettings ws = new System.Xml.XmlWriterSettings();
                ws.Indent = true;
                ws.Encoding = new System.Text.UTF8Encoding(false);
                using (System.Xml.XmlWriter w = System.Xml.XmlWriter.Create(path, ws))
                    doc.Save(w);
            }
            catch (Exception) { }
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
            return int.TryParse(Attr(n, name), out v) ? v : def;
        }

        private static double ReadDoubleAttr(XmlNode n, string name, double def)
        {
            double v;
            return double.TryParse(Attr(n, name), out v) ? v : def;
        }

        private static bool ReadBoolAttr(XmlNode n, string name, bool def)
        {
            bool v;
            return bool.TryParse(Attr(n, name), out v) ? v : def;
        }
    }
}
