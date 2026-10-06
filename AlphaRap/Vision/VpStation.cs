using System;
using System.Collections.Generic;
using System.Xml;
using Cognex.VisionPro;          // CogRecordDisplay / CogLine / CogColorConstants

namespace AlphaRap
{
    /// <summary>
    /// 一台相机 = VPForm 的 tabControl1 里的一页。
    /// 相机是"机器配置"，不随配方变（所以存 SettingData 而不是 RecipeData）。
    /// </summary>
    public class VpCameraConfig
    {
        /// <summary>相机名（也是该相机所有 vpp 的一级目录名）。</summary>
        public string Name = "";
        /// <summary>Cognex 采集通道号（CogFrameGrabbers 的下标）。</summary>
        public int CameraIndex = 0;
        /// <summary>
        /// **默认曝光**：新建 VPP / 添加标定时继承它作为初始值。
        /// 注意它本身不参与取流 —— 实时显示用的是"当前 VPP 自己的曝光"（<see cref="VpVppConfig.Exposure"/>），
        /// 标定用的是标定自己的（<see cref="VpCalibration.Exposure"/>）。
        /// </summary>
        public double Exposure = 10;
        /// <summary>该相机下的 VPP 列表（每个 VPP = 相机页里 VPP 子 TabControl 的一页）。</summary>
        public List<VpVppConfig> Vpps = new List<VpVppConfig>();

        /// <summary>
        /// 外部访问补偿限制表的入口：**拿着 VpCameraConfig 的实例**按 VPP 名取整张表
        /// （行是用户在 VPP 页上自己增删的）。没有这个 VPP 返回 null。
        /// 用法：cam.GetCompLimits("VPP1") → 遍历 Item/Min/Max 做补偿夹取。
        /// </summary>
        public List<VpCompLimitItem> GetCompLimits(string vppName)
        {
            if (string.IsNullOrEmpty(vppName)) return null;
            for (int i = 0; i < Vpps.Count; i++)
                if (Vpps[i] != null && Vpps[i].Name == vppName) return Vpps[i].CompLimits;
            return null;
        }

        /// <summary>
        /// 外部访问补偿限制的便捷入口：按 VPP 名 + 行名取**一行**限制
        /// （同名取第一行）。没有返回 null。
        /// </summary>
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

    /// <summary>
    /// 一个 VPP = 相机页里 VPP 子页的一页。
    /// VPP 归属于相机：它只能挂在创建它的那台相机下，不提供跨相机共用。
    /// </summary>
    public class VpVppConfig
    {
        public string Name = "";
        /// <summary>
        /// 本 VPP 实时取流用的曝光时间 —— **每个 VPP 各管各的**。
        /// 标定另有自己的一份（<see cref="VpCalibration.Exposure"/>），互不影响。
        /// 新建 VPP 时从相机的默认曝光继承（<see cref="VpCameraConfig.Exposure"/>）。
        /// </summary>
        public double Exposure = 10;
        /// <summary>
        /// 视觉定位补偿限制表：**用户自定义的若干行**（VPP 页上可增删行、可改名），
        /// 每行 = 名称 + 下限 + 上限。视觉算出的补偿量按名称对上行后，先夹到 [Min, Max] 再交给流程，
        /// 防止坏定位把机器带偏。
        /// 外部不直接摸 Vpps：统一从 <see cref="VpCameraConfig.GetCompLimits"/> / <see cref="VpCameraConfig.GetCompLimit"/> 进来。
        /// </summary>
        public List<VpCompLimitItem> CompLimits = new List<VpCompLimitItem>();
        /// <summary>运行时对应的视觉站实例（删除时要从 VisionproInterface.VList 里摘掉）。</summary>
        public VpStation Station;

        // ---- 存图（跟着 VPP 走）：RunAndWait 成功后自动存，见 VpStation.SaveLastImage ----
        /// <summary>存图启用。false = 不存。</summary>
        public bool SaveImageEnabled = false;
        /// <summary>存图目录；空 = vpp 所在目录下的 Images。</summary>
        public string SaveImagePath = "";
        /// <summary>存**原图**（采集根记录）。与截图可同时勾，两张都存。</summary>
        public bool SaveImageOriginal = false;
        /// <summary>存**截图**（主界面显示的那条子记录）。与原图可同时勾，两张都存。</summary>
        public bool SaveImageSnapshot = false;
        /// <summary>**自动删除**开关：勾上才按保留天数清理旧照片；不勾 = 永不删除。</summary>
        public bool SaveImageAutoDelete = false;
        /// <summary>照片实际保存的天数（配合自动删除使用）。</summary>
        public int SaveImageKeepDays = 0;

        /// <summary>
        /// 补偿限制表为空时补三行默认值（X / Y / 角度）—— 只在**读老配置**和**第一次建页**时兜底，
        /// 平时空表是合法状态（用户把行删光了就不该再冒出来）。
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

    /// <summary>
    /// 补偿限制表的一**行**：名称 + 上下限。名称由用户在表格里填（增删行自由），
    /// Min/Max 单位与视觉输出一致（X/Y 一般为 mm、角度为度）。
    /// </summary>
    public class VpCompLimitItem
    {
        public string Item = "";
        public double Min = -1;
        public double Max = 1;

        public VpCompLimitItem() { }
        public VpCompLimitItem(string item, double min, double max) { Item = item; Min = min; Max = max; }
    }

    /// <summary>
    /// 工位类型：决定 ToolBlock 跑完之后从哪些输出里取结果、以及十字线画多大。
    /// 这三种就是原来 17 个类里**唯一有区别**的地方。
    /// </summary>
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
    /// 唯一的视觉站类 —— 全工程 17 个 H?_Vision_* 类合并成这一个。
    ///
    /// 为什么能合并：那 17 个类逐字比对下来，**除了类名、构造函数里默认的 RunLiveCCDIndex、
    /// 以及 Calibration/Fiducial 取哪几个输出以外，代码完全一样**。
    ///   · 类名            → 变成构造函数的第一个参数（同时就是 vpp 目录名）
    ///   · 默认 CCD 号      → 第三个参数（运行时反正会被配方 Pset 里的值覆盖）
    ///   · 取哪些输出/十字线 → VpStationKind
    ///
    /// 这样"数量"就不再受类数量限制了：加相机、加 VPP 只是多 new 一个实例，
    /// 视觉侧不需要再写任何新类。
    ///
    /// 兼容现场已有 vpp：老工位把 VppFolderName 设成原来那个类名，
    /// 所以 vpp 路径仍然是 VisionData\H1_Vision_Fiducial\配方.vpp，一字节没变。
    /// </summary>
    public class VpStation : VisionproInterface
    {
        /// <summary>动态相机用：所属相机名。老工位这里等于工位名。</summary>
        public string CameraName = "";
        /// <summary>动态相机用：VPP 名。老工位为空。</summary>
        public string VppName = "";

        /// <summary>
        /// 建站时回填的本站 VPP 配置（<see cref="VpStation.CreateStation"/> 里赋值）。
        /// 补偿限制的方法直接读它 —— 调用时**零查找**，不用再穿过 VPForm / 相机列表去找。
        /// 老工位（不走 CreateStation 的）这里是 null，相关方法自动降级为"不限制"。
        /// </summary>
        public VpVppConfig Config;

        /// <summary>工位类型（取哪些输出 + 十字线大小）。</summary>
        public VpStationKind Kind = VpStationKind.Custom;

        /// <summary>基准点结果（x / y / u）。</summary>
        public Pos4D Fiducial = new Pos4D();
        /// <summary>标定结果（x / y）。</summary>
        public Pos4D Calibration = new Pos4D();

        // ================= 拍照 / 补偿限制：外部就用下面这几个成员 =================
        //
        // 一条龙就两行（判定逻辑自己写，这里只负责"拍到图"和"把限制给你"）：
        //     VpStation st = MiddleLayer.VPF.GetVppStation("Camera1", "VPP1");
        //     if (st.RunAndWait(3000)) { /* st.Fiducial / st.GetOutput("输出名") / st.CompLimits ... */ }
        //
        // 行名 = VPP 页补偿限制表"项目"列填的字（默认三行 X / Y / 角度）。

        /// <summary>本 VPP 的补偿限制表（实例直接拿，不用再穿配置找）；建站没回填配置时为 null。</summary>
        public List<VpCompLimitItem> CompLimits
        {
            get { return Config == null ? null : Config.CompLimits; }
        }

        /// <summary>
        /// 拍照并**等它跑完**（RunTB + RunTBOk 合成这一个函数）：
        /// <see cref="VisionproInterface.RunTB"/> 是异步的（内部开线程），这里替你轮询到结束。
        /// 返回 true = 在 timeoutMs 内跑完且 TB 判定 Accept。
        /// 存图启用时（<see cref="VpVppConfig.SaveImageEnabled"/>）成功后自动按 VPP 的设置存图并清理过期照片。
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

        /// <summary>按行名取本 VPP 的补偿限制（表里没有这一行返回 false）。行名 = 补偿限制表"项目"列。</summary>
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
        /// 单轴补偿夹取：表里有这一行就把 value 夹进 [Min, Max]。
        /// 返回 true = **超限**（值已被贴到边界，reason 记原因，如 "X&gt;Max"）；false = 没超限或表里没有这行（不限制）。
        /// 怎么判 OK/NG 由调用方逻辑自己定 —— 这里只做"夹"。
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
        /// 把最近一次运行的结果图按本 VPP 的存图设置存盘。返回最后一个文件全路径；没启用 / 没图返回 ""。
        /// 原图 / 截图**各存各的、可同时勾**：原图 = 根记录（采集图），截图 = 主界面显示的那条子记录
        /// （<see cref="VisionproInterface.VisionRunDisplayIndex"/>）。存完按"自动删除 + 保留天数"清理旧照片。
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
                // 注意命名空间：CogImageFile / CogImageFileModeConstants 在 Cognex.VisionPro.ImageFile 里，不在 Cognex.VisionPro
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

        /// <summary>存图目录：VPP 配置里填了就用填的；没填用 vpp 所在目录下的 Images。</summary>
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
        /// 定时删照片：**勾了"自动删除"才清**，按保留天数删存图目录里的旧 *.bmp
        /// （每次存图顺带扫一遍，不用额外线程）；天数 &lt;= 0 时不删，防误删。
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

        /// <summary>
        /// 开放接口：把**别的** CogRecordDisplay 绑到本站上 —— 拍照/实时都会往它刷画面（与主界面共用显示并列，按引用去重）。
        /// </summary>
        public void BindDisplay(Cognex.VisionPro.CogRecordDisplay crd)
        {
            if (crd == null) return;
            for (int i = 0; i < RecordDisplayList.Count; i++)
                if (ReferenceEquals(RecordDisplayList[i], crd)) return;
            RecordDisplayList.Add(crd);
        }

        /// <summary>
        /// 十字线所在图像的宽高。按工位摄像头的实际分辨率设，
        /// 不设就按 Kind 取默认值（标定=2592x1944，基准点=1280x960），与合并前的行为一致。
        /// </summary>
        public double CrossWidth = 0;
        public double CrossHeight = 0;

        /// <summary>老式用法：一个工位一个名字（传原来那个类名，vpp 目录就完全不变）。</summary>
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

        /// <summary>动态相机用：vpp 归属到"相机名\VPP名"（类型默认 Custom）。</summary>
        public VpStation(string cameraName, string vppName)
            : this(cameraName, vppName, VpStationKind.Custom)
        {
        }

        /// <summary>
        /// 动态相机用：vpp 归属到"相机名\VPP名"，并指定工位类型。
        /// **标定站必须传 <see cref="VpStationKind.Calibration"/>** ——
        /// `VisionRun()` 只在 Kind 是 Calibration / Fiducial 时才往
        /// <see cref="Calibration"/> / <see cref="Fiducial"/> 里填结果；
        /// 传 Custom 的话 <c>Calibration.x/y</c> 永远是 0，「拍标定点」就白拍了。
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

        /// <summary>按工位类型填默认的十字线尺寸（只在没手工设过的时候生效）。</summary>
        private void ApplyKindDefaults()
        {
            if (Kind == VpStationKind.Calibration)
            {
                // 老的 Calibration 类没有重写 CreatCentrelLine → 用的是基类那套 2592x1944
                CrossWidth = 2592;
                CrossHeight = 1944;
            }
            else
            {
                // 老的 Fiducial 类重写的就是 1280x960
                CrossWidth = 1280;
                CrossHeight = 960;
            }
        }

        /// <summary>
        /// 直接指定 vpp 目录名。
        /// 老工位传原来那个类名 → vpp 路径与合并前完全一致。
        /// </summary>
        public void SetVppFolder(string folderName)
        {
            VppFolderName = folderName ?? "";
            DisplayName = string.IsNullOrEmpty(VppFolderName) ? GetType().Name : VppFolderName;
        }

        /// <summary>重设归属（相机/VPP 改名后要调一次，否则 vpp 还指向老目录）。</summary>
        public void SetOwner(string cameraName, string vppName)
        {
            CameraName = cameraName ?? "";
            VppName = vppName ?? "";

            // vpp 路径：VisionData\{相机名}\{VPP名}\{配方名}.vpp
            SetVppFolder(CameraName + "\\" + VppName);
        }

        /// <summary>
        /// 逻辑与原来的 H1_Vision_Fiducial.VisionRun() 等**逐行等价**：
        /// 故意不加 try/catch —— 取输出失败时原来就会抛出、由 RunTB() 那边报警（2009），
        /// 这里吞掉反而会把异常状态当成正常结果。
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

                // 补偿限制**在这里生效**：结果出来后按 VPP 页限制表的行（X / Y / 角度）夹取，
                // 超限的值贴到边界。表里没有对应行 = 不限制；老工位（没挂 Config）也不限制。
                // 这样外部读 Fiducial / Calibration 时拿到的就已经是夹过的值。
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

        /// <summary>
        /// 居中十字线，尺寸走 CrossWidth/CrossHeight。
        /// 合并前 Fiducial 类写死 640/480（1280x960 的一半）、基类写死 1296/972（2592x1944 的一半），
        /// 这里统一成"宽高的一半"，两者结果一致。
        /// </summary>
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
    /// 相机级标定模块 —— **绑定到整台相机，不属于任何一个 VPP**。
    ///
    /// 与 legacy 的对应关系（从旧代码里整理出来的，旧代码已删除，留作对照）：
    ///   · `H?_Vision_Calibration` 那个工位（拍标定图、给像素坐标） → 本模块的 <see cref="Station"/>
    ///   · 旧 `SetUpTheData()` 把点灌进各 VPP 的 TB                → <see cref="ApplyToStations"/>
    ///   · 配方表 `tb_H?_VisCalib`（PixelX/PixelY/MotorPosX/MotorPosY） → <see cref="Points"/>
    ///     （改成存相机配置：标定是机器属性，不该随型号变。
    ///      注意：**旧表不会再被自动迁移**，需要时请手工把点誊到界面上的标定点表格里。）
    ///   · 各 VPP 的 TB 里名为 "Calibration" 的 CogCalibNPointToNPointTool → <see cref="ToolName"/>
    ///
    /// 为什么不做成"每个 VPP 一份标定"：一台相机的像素↔电机关系只有一个，
    /// 该相机下所有 VPP 共用；分开存只会让同一台相机的多份标定互相打架。
    /// </summary>
    public class VpCalibration
    {
        /// <summary>标定 vpp 的子目录名（在相机目录下）：VisionData\{相机名}\{FolderName}\{配方}.vpp</summary>
        public string FolderName = "Calibration";
        /// <summary>各 VPP 的 ToolBlock 里那个标定工具的名字。</summary>
        public string ToolName = "Calibration";
        /// <summary>
        /// **标定专用曝光** —— 与各 VPP 的曝光互不影响（标定板常常需要不同的亮度）。
        /// 目前只作用于标定站自己的实时取流；将来接"拍标定点时套进 TB 采集工具"也用这个值。
        /// </summary>
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
        /// <summary>相机名 → 标定模块。**内置相机页也在里面**（键用 H1_Camera / H2_Camera / H3_Camera）。</summary>
        public Dictionary<string, VpCalibration> Calibrations = new Dictionary<string, VpCalibration>();

        public VpCalibration GetCalibration(string cameraName)
        {
            if (string.IsNullOrEmpty(cameraName)) return null;
            VpCalibration c;
            return Calibrations.TryGetValue(cameraName, out c) ? c : null;
        }
    }

    /// <summary>
    /// 相机 / VPP / 标定 的持久化。
    ///
    /// **单独存一个 XML 文件，不塞进 ModuleBaseForm.SettingData。**
    /// 原因是 SettingData 的读写要经过 ReadSettingData() → SettingData.Clear() → ReadXml 这一串，
    /// 表的注册时机稍有不慎就会被整表丢掉；而这个列表是"机器配置"，丢了相机就得重配。
    /// 自己管一个文件，路径固定、内容看得见、随时能手改，出问题也好查。
    ///
    /// 文件：{SettingDataDirectory}\VPForm.Cameras.xml
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
                            // 老配置里没有 Vpp/@Exposure → 继承相机的曝光，行为与升级前一致
                            vpp.Exposure = ReadDoubleAttr(vn, "Exposure", cam.Exposure);
                            // 存图设置（缺省 = 不存图）
                            vpp.SaveImageEnabled = ReadBoolAttr(vn, "SaveImage", false);
                            string imgPath = Attr(vn, "SaveImagePath");
                            if (imgPath != "") vpp.SaveImagePath = imgPath;
                            vpp.SaveImageOriginal = ReadBoolAttr(vn, "SaveImageOriginal", false);
                            vpp.SaveImageSnapshot = ReadBoolAttr(vn, "SaveImageSnapshot", false);
                            vpp.SaveImageAutoDelete = ReadBoolAttr(vn, "SaveImageAutoDelete", false);
                            vpp.SaveImageKeepDays = ReadIntAttr(vn, "SaveImageKeepDays", 0);
                            // 补偿限制：**以 XML 为准整表重建**（Vpp/Comp[@Item/@Min/@Max]）。
                            // 绝不能"先补默认三行、再按名字把 XML 的值覆盖回去"——
                            // 那样删掉的行会被默认行顶回来、改名/新增的行会因为找不到同名默认行而被丢掉，
                            // 表现就是"增删改都不生效"。只有**老配置一行 Comp 都没有**时才补默认三行。
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
                        // 老配置里没有 Calibration/@Exposure → 退回该相机的默认曝光（再没有就用 10）
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

                // 直接写 UTF-8（工程里 XMLExpand.WriteUnicodeXML 写的是 encoding="unicode"，
                // 那种文件 Python / 部分工具读不了，这里不学它）
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
