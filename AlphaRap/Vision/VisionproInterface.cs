using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Cognex.VisionPro;
using Cognex.VisionPro.CalibFix;
using Cognex.VisionPro.ImageProcessing;
using Cognex.VisionPro.ToolBlock;

namespace AlphaRap
{
    public abstract class VisionproInterface
    {
        public static List<VisionproInterface> VList = new List<VisionproInterface>();
        public CogToolBlock TB;
        public string TBPath;
        public bool IsLoadTBOk;
        public bool IsAccept;

        private Task RunTask;
        private Task LoadTBTask;

        public bool AutoVisionRunDisplay = true;
        public int VisionRunDisplayIndex = 0;
        public List<CogRecordDisplay> RecordDisplayList = new List<CogRecordDisplay>();
        public int RunLiveCCDIndex = 0;
        public double RunExposure = 10;
        public VisionproInterface()
        {
            VList.Add(this);
        }

        public virtual void VisionRun() { }

        public void LoadTB(string _TBPath)
        {
            TBPath = _TBPath;
            LoadTB();
        }
        public void SaveTB(string _TBPath)
        {
            TBPath = _TBPath;
            SaveTB();
        }
        public void LoadTB()
        {
            try
            {
				if (File.Exists(TBPath))
					TB = (CogToolBlock)CogSerializer.LoadObjectFromFile(TBPath);
                else
                {
                    if (TB == null)
                        TB = new CogToolBlock();
                    if (!Directory.Exists(Path.GetDirectoryName(TBPath)))
                        Directory.CreateDirectory(Path.GetDirectoryName(TBPath));
					SaveTB();
				}
                IsLoadTBOk = true;
            }
            catch (Exception e) { }
          
        }
        public void SaveTB()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
					if (!Directory.Exists(Path.GetDirectoryName(TBPath)))
						Directory.CreateDirectory(Path.GetDirectoryName(TBPath));
                    CogSerializer.SaveObjectToFile(TB, TBPath);
					LoadTB();
				}
                catch (Exception) { }
            });
        }

        public void EditTB()
        {
            if (IsLoadTBOk)
            {
                TBEditForm TBEditF = new TBEditForm(this);
                TBEditF.ShowDialog();
                TBEditF.Dispose();
            }
        }

        public void RunTB()
        {
            if (RunTask != null)
                if (RunTask.Status == TaskStatus.Running)
                    return;
            RunTask = Task.Factory.StartNew(() =>
            {
                IsAccept = false;
                try
                {
                    if (AutoVisionRunDisplay)
                        for (int i = 0; i < RecordDisplayList.Count; i++)
                        {
                            RecordDisplayList[i].Image = null;
                            RecordDisplayList[i].StaticGraphics.Clear();
                            RecordDisplayList[i].InteractiveGraphics.Clear();
                        }

                    VisionRun();

                    if (AutoVisionRunDisplay)
                        if (VisionRunDisplayIndex < ((CogToolBlock)TB).CreateLastRunRecord().SubRecords.Count)
                            for (int i = 0; i < RecordDisplayList.Count; i++)
                            {
                                RecordDisplayList[i].Record = ((CogToolBlock)TB).CreateLastRunRecord().SubRecords[VisionRunDisplayIndex];
                                RecordDisplayList[i].AutoFit = true;
                                RecordDisplayList[i].Fit();
                            }
                }
                catch (Exception)
                {
                    NPSDK.Alarm.Show("2009");
                }
            });
        }
        public bool RunTBOk()
        {
            if (RunTask != null)
                if (RunTask.Status == TaskStatus.RanToCompletion)
                    return true;
            return false;
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

        public bool SetCalibration(List<CalibrationData> CalibData, string ToolName)
        {
            try
            {
                CogCalibNPointToNPointTool CalibNPointTool = TB.Tools[ToolName] as CogCalibNPointToNPointTool;
                CalibNPointTool.Calibration.NumPoints = CalibData.Count;
                for (int i = 0; i < CalibData.Count; i++)
                {
                    CalibNPointTool.Calibration.SetRawCalibratedPointX(i, CalibData[i].MotorPosX);
                    CalibNPointTool.Calibration.SetRawCalibratedPointY(i, CalibData[i].MotorPosY);
                    CalibNPointTool.Calibration.SetUncalibratedPointX(i, CalibData[i].PixelX);
                    CalibNPointTool.Calibration.SetUncalibratedPointY(i, CalibData[i].PixelY);
                }
                CalibNPointTool.Calibration.Calibrate();
                SaveTB();
            }
            catch (Exception) { return false; }
            return true;
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

                    CogFrameGrabbers CCD_Graber = new Cognex.VisionPro.CogFrameGrabbers();
                    if (CCD_Graber == null)
                        throw new Exception("Failed to create the CogFrameGrabbers object.");
					

                    ICogAcqFifo Fifo = CCD_Graber[RunLiveCCDIndex].CreateAcqFifo(CCD_Graber[RunLiveCCDIndex].AvailableVideoFormats[0], CogAcqFifoPixelFormatConstants.Format8Grey, 0, true);
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


            //Cognex.VisionPro.ImageProcessing.CogIPOneImageTool ImgTool = new Cognex.VisionPro.ImageProcessing.CogIPOneImageTool();
            //Cognex.VisionPro.ImageProcessing.CogIPOneImageFlipRotate cflip = new Cognex.VisionPro.ImageProcessing.CogIPOneImageFlipRotate();

            //ICogIPOneImageOperatorParams ip = (ICogIPOneImageOperatorParams)cflip;
            //ImgTool.Operators.Add(ip);
            //cflip.OperationInPixelSpace = CogIPOneImageFlipRotateOperationConstants.FlipAndRotate90Deg;
            //ImgTool.InputImage = RecordDisplay.Image;
            //ImgTool.Run();
            //Cognex.VisionPro.CogImage8Grey cimg = (CogImage8Grey)ImgTool.OutputImage;
            //RecordDisplay.Image = ImgTool.OutputImage;


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
        /// <returns></returns>
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

			//Bitmap dsImage =(Bitmap)Crd.CreateContentBitmap(Cognex.VisionPro.Display.CogDisplayContentBitmapConstants.Image,null,0);

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
            //dsImage.Save("yuancd.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
            return dsImage;
        }
        #endregion 图片旋转函数



    }
}
