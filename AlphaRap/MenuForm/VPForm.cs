using System;
using System.Windows.Forms;
using System.Threading;
using AlphaRapLibrary;
using System.Threading.Tasks;
using System.Drawing;
using System.Data;
using System.Collections.Generic;
using Cognex.VisionPro.ToolBlock;

namespace AlphaRap
{
    public partial class VPForm : ModuleBaseForm
	{
		
		public VPForm()
        {
            InitializeComponent();

            H1_VFiducial.RecordDisplayList.Add(cogRecDisp_H1_Recipe);
            H1_VFiducial.RecordDisplayList.Add(MiddleLayer.MainF.cogRecordDisplay1);

            H1_VFiducial2.RecordDisplayList.Add(cogRecDisp_H1_Recipe);
            H1_VFiducial2.RecordDisplayList.Add(MiddleLayer.MainF.cogRecordDisplay1);

            H1_VFiducial3.RecordDisplayList.Add(cogRecDisp_H1_Recipe);
            H1_VFiducial3.RecordDisplayList.Add(MiddleLayer.MainF.cogRecordDisplay1);

            H1_VFiducial4.RecordDisplayList.Add(cogRecDisp_H1_Recipe);
            H1_VFiducial4.RecordDisplayList.Add(MiddleLayer.MainF.cogRecordDisplay1);

            H1_VCalibration.RecordDisplayList.Add(cogRecDisp_H1_Recipe);
		


		}
		//视觉像素引用

		public H1_Vision_Calibration H1_VCalibration = new H1_Vision_Calibration();
		public H1_Vision_Fiducial H1_VFiducial = new H1_Vision_Fiducial();
		public H1_Vision_Fiducial2 H1_VFiducial2 = new H1_Vision_Fiducial2();
		public H1_Vision_Fiducial3 H1_VFiducial3 = new H1_Vision_Fiducial3();
		public H1_Vision_Fiducial4 H1_VFiducial4 = new H1_Vision_Fiducial4();

		public H2_Vision_Calibration H2_VCalibration = new H2_Vision_Calibration();
		public H2_Vision_Fiducial H2_VFiducial = new H2_Vision_Fiducial();
		public H2_Vision_Fiducial2 H2_VFiducial2 = new H2_Vision_Fiducial2();
		public H2_Vision_Fiducial3 H2_VFiducial3 = new H2_Vision_Fiducial3();


		public H3_Vision_Calibration H3_VCalibration = new H3_Vision_Calibration();
		public H3_Vision_Fiducial H3_VFiducial = new H3_Vision_Fiducial();
		public H3_Vision_Fiducial2 H3_VFiducial2 = new H3_Vision_Fiducial2();
		public H3_Vision_Fiducial3 H3_VFiducial3 = new H3_Vision_Fiducial3();


		public H4_Vision_Calibration H4_VCalibration = new H4_Vision_Calibration();
		public H4_Vision_Fiducial H4_VFiducial = new H4_Vision_Fiducial();
		public H4_Vision_Fiducial2 H4_VFiducial2 = new H4_Vision_Fiducial2();
		public H4_Vision_Fiducial3 H4_VFiducial3 = new H4_Vision_Fiducial3();


		public struct VisionPostData
        {
            public int Point;
            public double X;
            public double Y;
            public double R;

            public double X_Low;
            public double X_Hi;
            public double Y_Low;
            public double Y_Hi;
            public double R_Low;
            public double R_Hi;

            public bool Enable;
        }
		
       
		
        public List<VisionPostData> dgv_H1_VisionData_List = new List<VisionPostData>();
        public void Getdgv_H1_VisionDataList()
        {
			dgv_H1_VisionData_List.Clear();
            DataTable H1dt = MiddleLayer.VPF.RecipeData.Tables["tb_H1_Visdata"];
            for (int i = 0; i < H1dt.Rows.Count; i++)
            {
                DataRow dr = H1dt.Rows[i];
                VisionPostData data = new VisionPostData { Point = i, X = Convert.ToDouble(dr[0]), Y = Convert.ToDouble(dr[1]), R = Convert.ToDouble(dr[2]), X_Low = Convert.ToDouble(dr[3]), X_Hi = Convert.ToDouble(dr[4]), Y_Low = Convert.ToDouble(dr[5]), Y_Hi = Convert.ToDouble(dr[6]),R_Low = Convert.ToDouble(dr[7]), R_Hi = Convert.ToDouble(dr[8]), Enable = Convert.ToBoolean(dr[9]) };
				dgv_H1_VisionData_List.Add(data);

            }
        }
        void stopLive()
        {
            cogRecDisp_H1_Recipe.StopLiveDisplay();
            cogRecDisp_H2_Recipe.StopLiveDisplay();
            cogRecDisp_H3_Recipe.StopLiveDisplay();
        }

        private void button14_Click_1(object sender, EventArgs e)
        {
            stopLive();
            H1_VCalibration.EditTB();
        }

        private void button16_Click(object sender, EventArgs e)
        {
            stopLive();
            PhotoGraph();
        }

        //像素拍照
        public bool PhotoGraph()
        {
            bool ret = false;
            //OB_light.On();
            Task.Factory.StartNew(() =>
            {
                H1_VCalibration.RunTB();
                while (!H1_VCalibration.RunTBOk()) { Thread.Sleep(2); }
                if (H1_VCalibration.IsAccept)
                {
                    RefreshDifferentThreadUI(txt_H1_CalibResult, () =>
                    {
                        txt_H1_CalibResult.BackColor = Color.Lime;
                        txt_H1_CalibResult.Text = "OK";
                        txt_H1_CalibPixelX.Text = H1_VCalibration.Calibration.x.ToString("F3");
                        txt_H1_CalibPixelY.Text = H1_VCalibration.Calibration.y.ToString("F3");
                        ret = true;
                    });
                }
                else
                {
                    RefreshDifferentThreadUI(txt_H1_CalibResult, () =>
                    {
                        txt_H1_CalibResult.BackColor = Color.Red;
                        txt_H1_CalibResult.Text = "NG";
                        txt_H1_CalibPixelX.Text = "Null";
                        txt_H1_CalibPixelY.Text = "Null";
                    });
                }
                //OB_light.Off();
            });
            return ret;
        }

        //后台线程引用
        public static void RefreshDifferentThreadUI(Control control, Action action)
        {
            if (control.InvokeRequired)
            {
                Action refreshUI = new Action(action);
                control.Invoke(refreshUI);
            }
            else
            {
                action.Invoke();
            }
        }
        //电机移动
        public bool Manual_GotoXYZ(double XPos, double YPos, double ZPos)
        {
       
            bool InPosition = false;
           
            return InPosition;
        }

        //电机移动2
        private bool MoveToXYPost(double XPost, double YPost, JTimer Timer)//XY移动
        {
            bool InPosition = false;
            
            return InPosition;
        }
        

        private void button13_Click(object sender, EventArgs e)
        {
            Add_MPoint();
            SysPara.items++;
        }

        //添加标定点位引用
        public bool Add_MPoint()
        {

            DialogResult reult = MessageBox.Show("Sure Add Calib Point?", "", MessageBoxButtons.YesNo, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);

            if (reult == DialogResult.No)
            {
                return false;
            }

            if (H1_VCalibration.IsAccept)
            {
                DataTable dt = RecipeData.Tables["tb_H1_VisCalib"];
                DataRow dr = dt.NewRow();
                dr[0] = H1_VCalibration.Calibration.x.ToString("F3");
                dr[1] = H1_VCalibration.Calibration.y.ToString("F3");
                //dr[2] = Convert.ToDouble(MiddleLayer.HardF.MTR_H1_X.GetCommandPosition().ToString("F3"));
                //dr[3] = Convert.ToDouble(MiddleLayer.HardF.MTR_H1_Y.GetCommandPosition().ToString("F3"));
                
                dt.Rows.Add(dr);
                return true;
            }
            else
            {
               
                MessageBox.Show("Please trigger fiducial calibration");
                return false;
            }
           
        }

        private void button12_Click(object sender, EventArgs e)
        {
            if (dgv_H1_Calibration.CurrentRow != null)
            {
                DataTable dt = RecipeData.Tables["tb_H1_VisCalib"];
                dt.Rows.RemoveAt(dgv_H1_Calibration.CurrentRow.Index);
            }

            SysPara.items++;
        }

        private void Replace_Click(object sender, EventArgs e)
        {
            if (dgv_H1_Calibration.CurrentRow != null)
            {
                DataTable dt = RecipeData.Tables["tb_H1_VisCalib"];
                DataRow dr = dt.Rows[dgv_H1_Calibration.CurrentRow.Index];
                dr[0] = H1_VCalibration.Calibration.x.ToString("F3");
                dr[1] = H1_VCalibration.Calibration.y.ToString("F3");
              
            }
            else
            {
                MessageBox.Show("Please trigger fiducial calibration");
            }

            SysPara.items++;
        }

        private void button18_Click(object sender, EventArgs e)
        {
            SetUpTheData();
        }

        //设置标定数据
        public bool SetUpTheData()
        {
            bool ret = false;
            if (RecipeData.Tables["tb_H1_VisCalib"].Rows.Count >= 9)
            {
                List<CalibrationData> CalibrationDataList = new List<CalibrationData>();
                for (int i = 0; i < RecipeData.Tables["tb_H1_VisCalib"].Rows.Count; i++)
                {
                    CalibrationData CalibData = new CalibrationData();
                    CalibData.PixelX = (double)RecipeData.Tables["tb_H1_VisCalib"].Rows[i][0];
                    CalibData.PixelY = (double)RecipeData.Tables["tb_H1_VisCalib"].Rows[i][1];
                    CalibData.MotorPosX = (double)RecipeData.Tables["tb_H1_VisCalib"].Rows[i][2];
                    CalibData.MotorPosY = (double)RecipeData.Tables["tb_H1_VisCalib"].Rows[i][3];
                    ret = true;
                    CalibrationDataList.Add(CalibData);
                }
                if (H1_VFiducial.SetCalibration(CalibrationDataList, "Calibration")&& H1_VFiducial2.SetCalibration(CalibrationDataList, "Calibration"))
                {
                    MessageBox.Show("Calibration successful");
                }
                else
                {
                    MessageBox.Show("Calibration fail");
                }
            }
            else
            {
                MessageBox.Show("At least 9 calibration points");
            }
            return ret;
        }

        private void Fiducial1_Setup_Click(object sender, EventArgs e)
        {
            stopLive();
            H1_VFiducial.EditTB();
        }

        private void Fiducial1_Snap_Click(object sender, EventArgs e)
        {
            stopLive();
            Task.Factory.StartNew(() =>
            {
                H1_VFiducial.RunTB();
                while (!H1_VFiducial.RunTBOk()) { Thread.Sleep(2); }
                if (H1_VFiducial.IsAccept)
                {
                    RefreshDifferentThreadUI(txt_H1_FiducialResult, () =>
                    {
                        txt_H1_FiducialResult.Text = "OK";
                        txt_H1_FiducialResult.BackColor = Color.Lime;
                        txt_H1_FiducialX.Text = H1_VFiducial.Fiducial.x.ToString("F3");
                        txt_H1_FiducialY.Text = H1_VFiducial.Fiducial.y.ToString("F3");
                        txt_H1_FiducialU.Text = (H1_VFiducial.Fiducial.u * 180 / 3.14).ToString("F3");
                    });
                }
                else
                {
                    RefreshDifferentThreadUI(txt_H1_FiducialResult, () =>
                    {
                        txt_H1_FiducialResult.BackColor = Color.Red;
                        txt_H1_FiducialResult.Text = "NG";
                        txt_H1_FiducialX.Text = "Null";
                        txt_H1_FiducialY.Text = "Null";
                    });
                }
                //OB_H1_CCDLight.Off();
            });
        }



     
      
        private void H1_VisionDataAdd_Click(object sender, EventArgs e)
        {

            DialogResult reult = MessageBox.Show("Sure Add Vision?", "", MessageBoxButtons.YesNo, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);

            if (reult == DialogResult.No)
            {
                return ;
            }

            if (H1_VFiducial.IsAccept)
            {
                DataTable dt = RecipeData.Tables["tb_H1_Visdata"];
                DataRow dr = dt.NewRow();
                dr[0] = H1_VFiducial.Fiducial.x.ToString("F3");
                dr[1] = H1_VFiducial.Fiducial.y.ToString("F3");
                dr[2] = (H1_VFiducial.Fiducial.u * 180 / 3.14).ToString("F3");
                dt.Rows.Add(dr);
                SysPara.items++;
                return ;
            }
            else
            {

                MessageBox.Show("Vision error!");
                return ;
            }
        }

        private void H1_VisionDataReplace_Click(object sender, EventArgs e)
        {
            if (dgv_H1_VisionData.CurrentRow != null)
            {
                DataTable dt = RecipeData.Tables["tb_H1_Visdata"];
                DataRow dr = dt.Rows[dgv_H1_VisionData.CurrentRow.Index];
                dr[0] = H1_VFiducial.Fiducial.x.ToString("F3");
                dr[1] = H1_VFiducial.Fiducial.y.ToString("F3");
                dr[2] = (H1_VFiducial.Fiducial.u * 180 / 3.14).ToString("F3");
                SysPara.items++;
            }
            else
            {
                MessageBox.Show("Vision error!");
            }

           
        }

        private void H1_VisionDataDelete_Click(object sender, EventArgs e)
        {
            if (dgv_H1_VisionData.CurrentRow != null)
            {
                DataTable dt = RecipeData.Tables["tb_H1_Visdata"];
                dt.Rows.RemoveAt(dgv_H1_VisionData.CurrentRow.Index);
            }

            SysPara.items++;
        }
/// <summary>
/// ///////////////////////////////////////////////////////////////////////////////
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>
        private void H2_Live_Click(object sender, EventArgs e)
        {

            H2_VFiducial.RunLiveCCDIndex = MiddleLayer.VPF.GetRecipeValue("Pset", "Screw1CCDNum");
            H2_VFiducial.RunExposure = MiddleLayer.VPF.GetRecipeValue("Pset", "Screw1CCDLight");

            stopLive();
            H2_VFiducial.RunLive(cogRecDisp_H2_Recipe);
        }

        private void H2_LightOn_Click(object sender, EventArgs e)
        {

        }
         
        private void H2_LightOff_Click(object sender, EventArgs e)
        {

        }

        private void DownCalibration_Set_Click(object sender, EventArgs e)
        {
            stopLive();
            H2_VCalibration.EditTB();
        }

        private void DownCalibration_Snap_Click(object sender, EventArgs e)
        {
            bool ret = false;
           
            Task.Factory.StartNew(() =>
            {
                H2_VCalibration.RunTB();
                while (!H2_VCalibration.RunTBOk()) { Thread.Sleep(2); }
                if (H2_VCalibration.IsAccept)
                {
                    RefreshDifferentThreadUI(txt_H2_CalibResult, () =>
                    {
                        txt_H2_CalibResult.BackColor = Color.Lime;
                        txt_H2_CalibResult.Text = "OK";
                        txt_H2_CalibPixelX.Text = H2_VCalibration.Calibration.x.ToString("F3");
                        txt_H2_CalibPixelY.Text = H2_VCalibration.Calibration.y.ToString("F3");
                        ret = true;
                    });
                }
                else
                {
                    RefreshDifferentThreadUI(txt_H2_CalibResult, () =>
                    {
                        txt_H2_CalibResult.BackColor = Color.Red;
                        txt_H2_CalibResult.Text = "NG";
                        txt_H2_CalibPixelX.Text = "Null";
                        txt_H2_CalibPixelY.Text = "Null";
                    });
                }
              
            });
        }

        private void DownCalibration_Goto_Click(object sender, EventArgs e)
        {
            if (dgv_H2_Calibration.CurrentRow != null)
            {
                DataTable dt = RecipeData.Tables["tb_H2_VisCalib"];
                DataRow dr = dt.Rows[dgv_H2_Calibration.CurrentRow.Index];


                //ExecuteManual(() => Manual_GotoXYZR((double)dr[2], (double)dr[3], MiddleLayer.HardF.MTR_H1_Z.GetEncoderPosition(), MiddleLayer.HardF.MTR_H1_U.GetEncoderPosition()));
            }
        }

        private void DownCalibration_Add_Click(object sender, EventArgs e)
        {
            DialogResult reult = MessageBox.Show("Sure Add Calib2 Point?", "", MessageBoxButtons.YesNo, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);

            if (reult == DialogResult.No)
            {
                return ;
            }

            if (H2_VCalibration.IsAccept)
            {
                DataTable dt = RecipeData.Tables["tb_H2_VisCalib"];
                DataRow dr = dt.NewRow();
                dr[0] = H2_VCalibration.Calibration.x.ToString("F3");
                dr[1] = H2_VCalibration.Calibration.y.ToString("F3");
                //dr[2] = Convert.ToDouble(MiddleLayer.HardF.MTR_H1_X.GetCommandPosition().ToString("F3"));
                //dr[3] = Convert.ToDouble(MiddleLayer.HardF.MTR_H1_Y.GetCommandPosition().ToString("F3"));
                //dr[4] = Convert.ToDouble(MiddleLayer.HardF.MTR_H1_Z.GetCommandPosition().ToString("F3"));

                dt.Rows.Add(dr);
                SysPara.items++;
                return ;
            }
            else
            {

                MessageBox.Show("Please trigger fiducial calibration");
                return ;
            }
        }

        private void DownCalibration_Delete_Click(object sender, EventArgs e)
        {
            if (dgv_H2_Calibration.CurrentRow != null)
            {
                DataTable dt = RecipeData.Tables["tb_H2_VisCalib"];
                dt.Rows.RemoveAt(dgv_H2_Calibration.CurrentRow.Index);
            }

            SysPara.items++;
        }

       

        private void DownCalibration_Calibration_Click(object sender, EventArgs e)
        {
            bool ret = false;
            if (RecipeData.Tables["tb_H2_VisCalib"].Rows.Count >= 9)
            {
                List<CalibrationData> CalibrationDataList = new List<CalibrationData>();
                for (int i = 0; i < RecipeData.Tables["tb_H2_VisCalib"].Rows.Count; i++)
                {
                    CalibrationData CalibData = new CalibrationData();
                    CalibData.PixelX = (double)RecipeData.Tables["tb_H2_VisCalib"].Rows[i][0];
                    CalibData.PixelY = (double)RecipeData.Tables["tb_H2_VisCalib"].Rows[i][1];
                    CalibData.MotorPosX = (double)RecipeData.Tables["tb_H2_VisCalib"].Rows[i][2];
                    CalibData.MotorPosY = (double)RecipeData.Tables["tb_H2_VisCalib"].Rows[i][3];
                    ret = true;
                    CalibrationDataList.Add(CalibData);
                }
                if (H2_VFiducial.SetCalibration(CalibrationDataList, "Calibration")&& H2_VFiducial2.SetCalibration(CalibrationDataList, "Calibration")
                    &&H2_VFiducial3.SetCalibration(CalibrationDataList, "Calibration"))
                {
                    MessageBox.Show("Calibration successful");
                }
                else
                {
                    MessageBox.Show("Calibration fail");
                }
            }
            else
            {
                MessageBox.Show("At least 9 calibration points");
            }
            
        }

        private void DownFiducial1_Setup_Click(object sender, EventArgs e)
        {
            stopLive();
            H2_VFiducial.EditTB();
        }

        private void DownFiducial1_Snap_Click(object sender, EventArgs e)
        {
            //stopLive();
            //Task.Factory.StartNew(() =>
            //{
            //    H2_VFiducial.RunTB();
            //    while (!H2_VFiducial.RunTBOk()) { Thread.Sleep(2); }
            //    if (H2_VFiducial.IsAccept)
            //    {
            //        RefreshDifferentThreadUI(txt_H2_FiducialResult, () =>
            //        {
            //            txt_H2_FiducialResult.Text = "OK";
            //            txt_H2_FiducialResult.BackColor = Color.Lime;
            //            txt_H2_FiducialX.Text = H2_VFiducial.Fiducial.x.ToString("F3");
            //            txt_H2_FiducialY.Text = H2_VFiducial.Fiducial.y.ToString("F3");
            //            txt_H2_FiducialR.Text = (H2_VFiducial.Fiducial.u * 180 / 3.14).ToString("F3");
            //        });
            //    }
            //    else
            //    {
            //        RefreshDifferentThreadUI(txt_H2_FiducialResult, () =>
            //        {
            //            txt_H2_FiducialResult.BackColor = Color.Red;
            //            txt_H2_FiducialResult.Text = "NG";
            //            txt_H2_FiducialX.Text = "Null";
            //            txt_H2_FiducialY.Text = "Null";
            //        });
            //    }
             
            //});
        }

        private void button4_Click(object sender, EventArgs e)
        {
            DialogResult reult = MessageBox.Show("Sure Add Vision?", "", MessageBoxButtons.YesNo, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);

            if (reult == DialogResult.No)
            {
                return;
            }

            if (H2_VFiducial.IsAccept)
            {
                DataTable dt = RecipeData.Tables["tb_H2_Visdata"];
                DataRow dr = dt.NewRow();
                dr[0] = H2_VFiducial.Fiducial.x.ToString("F3");
                dr[1] = H2_VFiducial.Fiducial.y.ToString("F3");
                dr[2] = (H2_VFiducial.Fiducial.u * 180 / 3.14).ToString("F3");
                //dr[3] = Convert.ToDouble(MiddleLayer.HardF.MTR_H1_X.GetCommandPosition().ToString("F3"));
                //dr[4] = Convert.ToDouble(MiddleLayer.HardF.MTR_H1_Y.GetCommandPosition().ToString("F3"));
                //dr[5] = Convert.ToDouble(MiddleLayer.HardF.MTR_H1_U.GetCommandPosition().ToString("F3"));

                dt.Rows.Add(dr);
                SysPara.items++;
                return;
            }
            else
            {

                MessageBox.Show("Vision error!");
                return;
            }
        }

        private void H2_VisionDataReplace_Click(object sender, EventArgs e)
        {
            //if (dgv_H2_VisionData.CurrentRow != null)
            //{
            //    DataTable dt = RecipeData.Tables["tb_H2_Visdata"];
            //    DataRow dr = dt.Rows[dgv_H2_VisionData.CurrentRow.Index];
            //    dr[0] = H2_VFiducial.Fiducial.x.ToString("F3");
            //    dr[1] = H2_VFiducial.Fiducial.y.ToString("F3");
            //    dr[2] = (H2_VFiducial.Fiducial.u * 180 / 3.14).ToString("F3");
            //    //dr[3] = Convert.ToDouble(MiddleLayer.HardF.MTR_H1_X.GetCommandPosition().ToString("F3"));
            //    //dr[4] = Convert.ToDouble(MiddleLayer.HardF.MTR_H1_Y.GetCommandPosition().ToString("F3"));
            //    //dr[5] = Convert.ToDouble(MiddleLayer.HardF.MTR_H1_U.GetCommandPosition().ToString("F3"));

            //    SysPara.items++;
            //}
            //else
            //{
            //    MessageBox.Show("Vision error!");
            //}

        }

        private void H2_VisionDataDelete_Click(object sender, EventArgs e)
        {
            //if (dgv_H2_VisionData.CurrentRow != null)
            //{
            //    DataTable dt = RecipeData.Tables["tb_H2_Visdata"];
            //    dt.Rows.RemoveAt(dgv_H2_VisionData.CurrentRow.Index);
            //}

            //SysPara.items++;
        }
        /// <summary>
        /// ///////////////////////////////////////////////////
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Fiducial2_Setup_Click(object sender, EventArgs e)
        {
           // stopLive();
            H1_VFiducial2.EditTB();
        }

        private void Fiducial2_Snap_Click(object sender, EventArgs e)
        {
            //stopLive();
            Task.Factory.StartNew(() =>
            {
                H1_VFiducial2.RunTB();
                while (!H1_VFiducial2.RunTBOk()) { Thread.Sleep(2); }
                if (H1_VFiducial2.IsAccept)
                {
                    RefreshDifferentThreadUI(txt_H1_Fiducial2Result, () =>
                    {
                        txt_H1_Fiducial2Result.Text = "OK";
                        txt_H1_Fiducial2Result.BackColor = Color.Lime;
                        txt_H1_Fiducial2X.Text = H1_VFiducial2.Fiducial.x.ToString("F3");
                        txt_H1_Fiducial2Y.Text = H1_VFiducial2.Fiducial.y.ToString("F3");
                        txt_H1_Fiducial2U.Text = (H1_VFiducial2.Fiducial.u * 180 / 3.14).ToString("F3");
                    });
                }
                else
                {
                    RefreshDifferentThreadUI(txt_H1_Fiducial2Result, () =>
                    {
                        txt_H1_Fiducial2Result.BackColor = Color.Red;
                        txt_H1_Fiducial2Result.Text = "NG";
                        txt_H1_Fiducial2X.Text = "Null";
                        txt_H1_Fiducial2Y.Text = "Null";
                    });
                }
               
            });
        }

        private void H1_VisionHeadDataAdd_Click(object sender, EventArgs e)
        {
            DialogResult reult = MessageBox.Show("Sure Add Vision?", "", MessageBoxButtons.YesNo, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);

            if (reult == DialogResult.No)
            {
                return;
            }

            if (H1_VFiducial2.IsAccept)
            {
                DataTable dt = RecipeData.Tables["tb_H1_HeadCalib"];
                DataRow dr = dt.NewRow();
                dr[0] = H1_VFiducial2.Fiducial.x.ToString("F3");
                dr[1] = H1_VFiducial2.Fiducial.y.ToString("F3");
                dr[2] = (H1_VFiducial2.Fiducial.u * 180 / 3.14).ToString("F3");
             
                dt.Rows.Add(dr);
                SysPara.items++;
                return;
            }
            else
            {

                MessageBox.Show("Vision error!");
                return;
            }
        }

        private void H1_VisionHeadDataReplace_Click(object sender, EventArgs e)
        {


			if (dgv_H1_VisionData2.CurrentRow != null)
			{
				DataTable dt = RecipeData.Tables["tb_H1_HeadCalib"];
				DataRow dr = dt.Rows[dgv_H1_VisionData2.CurrentRow.Index];
				dr[0] = H1_VFiducial2.Fiducial.x.ToString("F3");
				dr[1] = H1_VFiducial2.Fiducial.y.ToString("F3");
				dr[2] = (H1_VFiducial2.Fiducial.u * 180 / 3.14).ToString("F3");

				SysPara.items++;
			}
			else
			{
				MessageBox.Show("Vision error!");
			}
		}

		private void H1_VisionHeadDataDelete_Click(object sender, EventArgs e)
        {
            if (dgv_H1_VisionData2.CurrentRow != null)
            {
                DataTable dt = RecipeData.Tables["tb_H1_HeadCalib"];
                dt.Rows.RemoveAt(dgv_H1_VisionData2.CurrentRow.Index);
            }

            SysPara.items++;
        }

        

       
       /// <summary>
       /// /////////////////////////////
       /// </summary>
       /// <param name="sender"></param>
       /// <param name="e"></param>
        private void DownFiducial2_Setup_Click(object sender, EventArgs e)
        {
            stopLive();
            H2_VFiducial.EditTB();
        }

      

        private void H2_VisionData2Add_Click(object sender, EventArgs e)
        {
            DialogResult reult = MessageBox.Show("Sure Add Vision?", "", MessageBoxButtons.YesNo, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);

            if (reult == DialogResult.No)
            {
                return;
            }

            if (H2_VFiducial2.IsAccept)
            {
                DataTable dt = RecipeData.Tables["tb_H2_Visdata2"];
                DataRow dr = dt.NewRow();
                dr[0] = H2_VFiducial2.Fiducial.x.ToString("F3");
                dr[1] = H2_VFiducial2.Fiducial.y.ToString("F3");
                if ((H2_VFiducial2.Fiducial.u * 180 / 3.14) < 0)
                {
                    txt_H2_FiducialU.Text = (H2_VFiducial2.Fiducial.u * (180 / 3.14) + 180).ToString("F3");

                }
                else
                {
                    txt_H2_FiducialU.Text = (H2_VFiducial2.Fiducial.u * (180 / 3.14)).ToString("F3");


                }

                dt.Rows.Add(dr);
                SysPara.items++;
                return;
            }
            else
            {

                MessageBox.Show("Vision error!");
                return;
            }
        }

        private void H2_VisionData2Replace_Click(object sender, EventArgs e)
        {
            if (dgv_H2_VisionData.CurrentRow != null)
            {
                

                    DataTable dt = RecipeData.Tables["tb_H2_Visdata2"];
                    DataRow dr = dt.Rows[dgv_H2_VisionData.CurrentRow.Index];
                    dr[0] = H2_VFiducial.Fiducial.x.ToString("F3");
                    dr[1] = H2_VFiducial.Fiducial.y.ToString("F3");

                    dr[2] = (H2_VFiducial.Fiducial.u * (180 / 3.14)).ToString("F3");

                SysPara.items++;
            }
            else
            {
                MessageBox.Show("Vision error!");
            }

        }

        private void H2_VisionData2Delete_Click(object sender, EventArgs e)
        {
            if (dgv_H2_VisionData.CurrentRow != null)
            {
                DataTable dt = RecipeData.Tables["tb_H2_Visdata2"];
                dt.Rows.RemoveAt(dgv_H2_VisionData.CurrentRow.Index);
            }

            SysPara.items++;
        }
     
        private void Fiducial3_Setup_Click(object sender, EventArgs e)
        {
            stopLive();
            H1_VFiducial3.EditTB();
        }

        private void Fiducial3_Snap_Click(object sender, EventArgs e)
        {
            stopLive();
            Task.Factory.StartNew(() =>
            {
                H1_VFiducial3.RunTB();
                while (!H1_VFiducial3.RunTBOk()) { Thread.Sleep(2); }
                if (H1_VFiducial3.IsAccept)
                {
                    RefreshDifferentThreadUI(txt_H1_FiducialResult3, () =>
                    {
                        txt_H1_FiducialResult3.Text = "OK";
                        txt_H1_FiducialResult3.BackColor = Color.Lime;
                        txt_H1_FiducialX3.Text = H1_VFiducial3.Fiducial.x.ToString("F3");
                        txt_H1_FiducialY3.Text = H1_VFiducial3.Fiducial.y.ToString("F3");
                        txt_H1_FiducialU3.Text = (H1_VFiducial3.Fiducial.u * 180 / 3.14).ToString("F3");
                    });
                }
                else
                {
                    RefreshDifferentThreadUI(txt_H1_FiducialResult, () =>
                    {
                        txt_H1_FiducialResult3.BackColor = Color.Red;
                        txt_H1_FiducialResult3.Text = "NG";
                        txt_H1_FiducialX3.Text = "Null";
                        txt_H1_FiducialY3.Text = "Null";
                    });
                }
                //OB_H1_CCDLight.Off();
            });
        }

        private void H1_VisionCheckAdd_Click(object sender, EventArgs e)
        {
            DialogResult reult = MessageBox.Show("Sure Add Vision?", "", MessageBoxButtons.YesNo, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);

            if (reult == DialogResult.No)
            {
                return;
            }

            if (H1_VFiducial3.IsAccept)
            {
                DataTable dt = RecipeData.Tables["tb_H1_VisionCheck"];
                DataRow dr = dt.NewRow();
                dr[0] = H1_VFiducial3.Fiducial.x.ToString("F3");
                dr[1] = H1_VFiducial3.Fiducial.y.ToString("F3");
                dr[2] = H1_VFiducial3.Fiducial.u.ToString("F3");
                dt.Rows.Add(dr);
                SysPara.items++;
                return;
            }
            else
            {

                MessageBox.Show("Vision error!");
                return;
            }
        }

        private void H1_VisionCheckReplace_Click(object sender, EventArgs e)
        {
            if (dgv_H1_VisionData3.CurrentRow != null)
            {
                DataTable dt = RecipeData.Tables["tb_H1_Visdata3"];
                DataRow dr = dt.Rows[dgv_H1_VisionData3.CurrentRow.Index];
                dr[0] = H1_VFiducial3.Fiducial.x.ToString("F3");
                dr[1] = H1_VFiducial3.Fiducial.y.ToString("F3");
                dr[2] = H1_VFiducial3.Fiducial.u.ToString("F3");
                SysPara.items++;
            }
            else
            {
                MessageBox.Show("Vision error!");
            }

        }

        private void H1_VisionDelete_Click(object sender, EventArgs e)
        {
            if (dgv_H1_VisionData3.CurrentRow != null)
            {
                DataTable dt = RecipeData.Tables["tb_H1_VisionCheck"];
                dt.Rows.RemoveAt(dgv_H1_VisionData3.CurrentRow.Index);
            }

            SysPara.items++;
        }

        private void dgv_H1_VisionData_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            SysPara.items++;
        }

        private void button40_Click(object sender, EventArgs e)
        {
            H3_VFiducial.RunLiveCCDIndex = MiddleLayer.VPF.GetRecipeValue("Pset", "Screw2CCDNum");
            H3_VFiducial.RunExposure = MiddleLayer.VPF.GetRecipeValue("Pset", "Screw2CCDLight");
            stopLive();
            H3_VFiducial.RunLive(cogRecDisp_H3_Recipe);
        }

        private void button68_Click(object sender, EventArgs e)
        {

            H4_VFiducial.RunLiveCCDIndex = MiddleLayer.VPF.GetRecipeValue("Pset", "UnloadCCDNum");
            H4_VFiducial.RunExposure = MiddleLayer.VPF.GetRecipeValue("Pset", "UnloadCCDLight");

            stopLive();
           
        }

        private void button39_Click(object sender, EventArgs e)
        {
            stopLive();
            H3_VCalibration.EditTB();
        }

        private void button38_Click(object sender, EventArgs e)
        {
            bool ret = false;

            Task.Factory.StartNew(() =>
            {
                H3_VCalibration.RunTB();
                while (!H3_VCalibration.RunTBOk()) { Thread.Sleep(2); }
                if (H3_VCalibration.IsAccept)
                {
                    RefreshDifferentThreadUI(txt_H3_CalibResult, () =>
                    {
                        txt_H3_CalibResult.BackColor = Color.Lime;
                        txt_H3_CalibResult.Text = "OK";
                        txt_H3_CalibPixelX.Text = H3_VCalibration.Calibration.x.ToString("F3");
                        txt_H3_CalibPixelY.Text = H3_VCalibration.Calibration.y.ToString("F3");
                        ret = true;
                    });
                }
                else
                {
                    RefreshDifferentThreadUI(txt_H3_CalibResult, () =>
                    {
                        txt_H3_CalibResult.BackColor = Color.Red;
                        txt_H3_CalibResult.Text = "NG";
                        txt_H3_CalibPixelX.Text = "Null";
                        txt_H3_CalibPixelY.Text = "Null";
                    });
                }

            });
        }

        private void button37_Click(object sender, EventArgs e)
        {
            DialogResult reult = MessageBox.Show("Sure Add Calib3 Point?", "", MessageBoxButtons.YesNo, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);

            if (reult == DialogResult.No)
            {
                return;
            }

            if (H3_VCalibration.IsAccept)
            {
                DataTable dt = RecipeData.Tables["tb_H3_VisCalib"];
                DataRow dr = dt.NewRow();
                dr[0] = H3_VCalibration.Calibration.x.ToString("F3");
                dr[1] = H3_VCalibration.Calibration.y.ToString("F3");
                //dr[2] = Convert.ToDouble(MiddleLayer.HardF.MTR_H1_X.GetCommandPosition().ToString("F3"));
                //dr[3] = Convert.ToDouble(MiddleLayer.HardF.MTR_H1_Y.GetCommandPosition().ToString("F3"));
                //dr[4] = Convert.ToDouble(MiddleLayer.HardF.MTR_H1_Z.GetCommandPosition().ToString("F3"));

                dt.Rows.Add(dr);
                SysPara.items++;
                return;
            }
            else
            {

                MessageBox.Show("Please trigger fiducial calibration");
                return;
            }
        }

        private void button36_Click(object sender, EventArgs e)
        {
            if (dgv_H3_Calibration.CurrentRow != null)
            {
                DataTable dt = RecipeData.Tables["tb_H3_VisCalib"];
                dt.Rows.RemoveAt(dgv_H3_Calibration.CurrentRow.Index);
            }

            SysPara.items++;
        }

       

        private void button35_Click(object sender, EventArgs e)
        {
            bool ret = false;
            if (RecipeData.Tables["tb_H3_VisCalib"].Rows.Count >= 9)
            {
                List<CalibrationData> CalibrationDataList = new List<CalibrationData>();
                for (int i = 0; i < RecipeData.Tables["tb_H3_VisCalib"].Rows.Count; i++)
                {
                    CalibrationData CalibData = new CalibrationData();
                    CalibData.PixelX = (double)RecipeData.Tables["tb_H3_VisCalib"].Rows[i][0];
                    CalibData.PixelY = (double)RecipeData.Tables["tb_H3_VisCalib"].Rows[i][1];
                    CalibData.MotorPosX = (double)RecipeData.Tables["tb_H3_VisCalib"].Rows[i][2];
                    CalibData.MotorPosY = (double)RecipeData.Tables["tb_H3_VisCalib"].Rows[i][3];
                    ret = true;
                    CalibrationDataList.Add(CalibData);
                }
                if (H3_VFiducial.SetCalibration(CalibrationDataList, "Calibration"))
                {
                    MessageBox.Show("Calibration successful");
                }
                else
                {
                    MessageBox.Show("Calibration fail");
                }
            }
            else
            {
                MessageBox.Show("At least 9 calibration points");
            }

        }

        private void button20_Click(object sender, EventArgs e)
        {
            stopLive();
            H3_VFiducial.EditTB();
        }

        

        private void button10_Click_1(object sender, EventArgs e)
        {
            if (dgv_H3_VisionData.CurrentRow != null)
            {


                DataTable dt = RecipeData.Tables["tb_H3_Visdata"];
                DataRow dr = dt.Rows[dgv_H3_VisionData.CurrentRow.Index];
                dr[0] = H3_VFiducial.Fiducial.x.ToString("F3");
                dr[1] = H3_VFiducial.Fiducial.y.ToString("F3");

                dr[2] = (H3_VFiducial.Fiducial.u * (180 / 3.14)).ToString("F3");

                SysPara.items++;
            }
            else
            {
                MessageBox.Show("Vision error!");
            }

        }

        private void button12_Click_1(object sender, EventArgs e)
        {
            DialogResult reult = MessageBox.Show("Sure Add Vision3?", "", MessageBoxButtons.YesNo, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);

            if (reult == DialogResult.No)
            {
                return;
            }

            if (H3_VFiducial.IsAccept)
            {
                DataTable dt = RecipeData.Tables["tb_H3_Visdata"];
                DataRow dr = dt.NewRow();
                dr[0] = H3_VFiducial.Fiducial.x.ToString("F3");
                dr[1] = H3_VFiducial.Fiducial.y.ToString("F3");
                dr[2]=(H3_VFiducial.Fiducial.u * (180 / 3.14)).ToString("F3");

                dt.Rows.Add(dr);
                SysPara.items++;
                return;
            }
            else
            {

                MessageBox.Show("Vision error!");
                return;
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            if (dgv_H3_VisionData.CurrentRow != null)
            {
                DataTable dt = RecipeData.Tables["tb_H3_Visdata"];
                dt.Rows.RemoveAt(dgv_H3_VisionData.CurrentRow.Index);
            }

            SysPara.items++;
        }

        private void button67_Click(object sender, EventArgs e)
        {
            stopLive();
            H4_VCalibration.EditTB();
        }

        private void button65_Click(object sender, EventArgs e)
        {
            DialogResult reult = MessageBox.Show("Sure Add Calib4 Point?", "", MessageBoxButtons.YesNo, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);

            if (reult == DialogResult.No)
            {
                return;
            }

            if (H4_VCalibration.IsAccept)
            {
                DataTable dt = RecipeData.Tables["tb_H4_VisCalib"];
                DataRow dr = dt.NewRow();
                dr[0] = H4_VCalibration.Calibration.x.ToString("F3");
                dr[1] = H4_VCalibration.Calibration.y.ToString("F3");
                //dr[2] = Convert.ToDouble(MiddleLayer.HardF.MTR_H1_X.GetCommandPosition().ToString("F3"));
                //dr[3] = Convert.ToDouble(MiddleLayer.HardF.MTR_H1_Y.GetCommandPosition().ToString("F3"));
                //dr[4] = Convert.ToDouble(MiddleLayer.HardF.MTR_H1_Z.GetCommandPosition().ToString("F3"));

                dt.Rows.Add(dr);
                SysPara.items++;
                return;
            }
            else
            {

                MessageBox.Show("Please trigger fiducial calibration");
                return;
            }
        }

        private void button63_Click(object sender, EventArgs e)
        {
            bool ret = false;
            if (RecipeData.Tables["tb_H4_VisCalib"].Rows.Count >= 9)
            {
                List<CalibrationData> CalibrationDataList = new List<CalibrationData>();
                for (int i = 0; i < RecipeData.Tables["tb_H4_VisCalib"].Rows.Count; i++)
                {
                    CalibrationData CalibData = new CalibrationData();
                    CalibData.PixelX = (double)RecipeData.Tables["tb_H4_VisCalib"].Rows[i][0];
                    CalibData.PixelY = (double)RecipeData.Tables["tb_H4_VisCalib"].Rows[i][1];
                    CalibData.MotorPosX = (double)RecipeData.Tables["tb_H4_VisCalib"].Rows[i][2];
                    CalibData.MotorPosY = (double)RecipeData.Tables["tb_H4_VisCalib"].Rows[i][3];
                    ret = true;
                    CalibrationDataList.Add(CalibData);
                }
                if (H4_VFiducial.SetCalibration(CalibrationDataList, "Calibration"))
                {
                    MessageBox.Show("Calibration successful");
                }
                else
                {
                    MessageBox.Show("Calibration fail");
                }
            }
            else
            {
                MessageBox.Show("At least 9 calibration points");
            }
        }

        private void button50_Click(object sender, EventArgs e)
        {
          //  stopLive();
            H4_VFiducial.EditTB();
        }

       

        private void button46_Click(object sender, EventArgs e)
        {
            DialogResult reult = MessageBox.Show("Sure Add Vision4?", "", MessageBoxButtons.YesNo, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);

            if (reult == DialogResult.No)
            {
                return;
            }

            if (H4_VFiducial.IsAccept)
            {
                DataTable dt = RecipeData.Tables["tb_H4_Visdata"];
                DataRow dr = dt.NewRow();
                dr[0] = H4_VFiducial.Fiducial.x.ToString("F3");
                dr[1] = H4_VFiducial.Fiducial.y.ToString("F3");
                dr[2] = (H4_VFiducial.Fiducial.u * (180 / 3.14)).ToString("F3");

                dt.Rows.Add(dr);
                SysPara.items++;
                return;
            }
            else
            {

                MessageBox.Show("Vision error!");
                return;
            }
        }

       
     

        private void button3_Click(object sender, EventArgs e)
        {
            MiddleLayer.VPF.H1_VFiducial2.TB.Inputs["Index"].Value = 6;
        }

       

        private void button19_Click(object sender, EventArgs e)
        {
            stopLive();
            H1_VFiducial4.EditTB();
        }

        private void button13_Click_1(object sender, EventArgs e)
        {
            stopLive();
            Task.Factory.StartNew(() =>
            {
                H1_VFiducial4.RunTB();
                while (!H1_VFiducial4.RunTBOk()) { Thread.Sleep(2); }
                if (H1_VFiducial4.IsAccept)
                {
                    RefreshDifferentThreadUI(txt_H1_FiducialResult4, () =>
                    {
                        txt_H1_FiducialResult4.Text = "OK";
                        txt_H1_FiducialResult4.BackColor = Color.Lime;
                        txt_H1_FiducialX4.Text = H1_VFiducial4.Fiducial.x.ToString("F3");
                        txt_H1_FiducialY4.Text = H1_VFiducial4.Fiducial.y.ToString("F3");
                        txt_H1_FiducialU4.Text = (H1_VFiducial4.Fiducial.u * 180 / 3.14).ToString("F3");
                    });
                }
                else
                {
                    RefreshDifferentThreadUI(txt_H1_FiducialResult4, () =>
                    {
                        txt_H1_FiducialResult4.BackColor = Color.Red;
                        txt_H1_FiducialResult4.Text = "NG";
                        txt_H1_FiducialX4.Text = "Null";
                        txt_H1_FiducialY4.Text = "Null";
                    });
                }
             
            });
        }

        private void button7_Click(object sender, EventArgs e)
        {

            DialogResult reult = MessageBox.Show("Do you want to Calibration Screw1 vision?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
            if (reult == DialogResult.No)
            {
                return;

            }
            

            CogToolBlock Screw1_1Tool = H2_VFiducial.TB.Tools["Tb_Pos"] as CogToolBlock;
            CogToolBlock Screw1_1Tool_Snap = Screw1_1Tool.Tools["Tb_CalculateSrewRelativePos"] as CogToolBlock;


            CogToolBlock Screw1_2Tool = H2_VFiducial.TB.Tools["Tb_Pos2"] as CogToolBlock;
            CogToolBlock Screw1_2Tool_Snap = Screw1_2Tool.Tools["Tb_CalculateSrewRelativePos"] as CogToolBlock;

            CogToolBlock Screw1_3Tool = H2_VFiducial.TB.Tools["Tb_Pos3"] as CogToolBlock;
            CogToolBlock Screw1_3Tool_Snap = Screw1_3Tool.Tools["Tb_CalculateSrewRelativePos"] as CogToolBlock;
            

            H2_VFiducial.SaveTB();
        }

        private void button8_Click(object sender, EventArgs e)
        {

            DialogResult reult = MessageBox.Show("Do you want to Calibration Screw2 vision?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
            if (reult == DialogResult.No)
            {
                return;

            }
			
            CogToolBlock Screw2_1Tool = H3_VFiducial.TB.Tools["Tb_Pos"] as CogToolBlock;
            CogToolBlock Screw2_1Tool_Snap = Screw2_1Tool.Tools["Tb_CalculateSrewRelativePos"] as CogToolBlock;


            CogToolBlock Screw2_2Tool = H3_VFiducial.TB.Tools["Tb_Pos2"] as CogToolBlock;
            CogToolBlock Screw2_2Tool_Snap = Screw2_2Tool.Tools["Tb_CalculateSrewRelativePos"] as CogToolBlock;

            CogToolBlock Screw2_3Tool = H3_VFiducial.TB.Tools["Tb_Pos3"] as CogToolBlock;
            CogToolBlock Screw2_3Tool_Snap = Screw2_3Tool.Tools["Tb_CalculateSrewRelativePos"] as CogToolBlock;
            
            H3_VFiducial.SaveTB();

        }
		
        private void button3_Click_1(object sender, EventArgs e)
        {
            bool UserTrayB = MiddleLayer.SystemS.GetSettingValue("MSet", "UserTrayB");

            if (UserTrayB)
            {
                MiddleLayer.VPF.H4_VFiducial.TB.Inputs["ModelIndex"].Value = 1;
            }
            else
            {
                MiddleLayer.VPF.H4_VFiducial.TB.Inputs["ModelIndex"].Value = 2;
            }
        }

		private void dgv_H2_Calibration_CellContentClick(object sender, DataGridViewCellEventArgs e)
		{

		}

		private void button17_Click(object sender, EventArgs e)
		{
			stopLive();
			Task.Factory.StartNew(() =>
			{
				H3_VFiducial.RunTB();
				while (!H3_VFiducial.RunTBOk()) { Thread.Sleep(2); }
				if (H3_VFiducial.IsAccept)
				{
					RefreshDifferentThreadUI(txt_H1_FiducialResult, () =>
					{
						txt_H3_FiducialResult.Text = "OK";
						txt_H3_FiducialResult.BackColor = Color.Lime;
						txt_H3_FiducialX.Text = H3_VFiducial.Fiducial.x.ToString("F3");
						txt_H3_FiducialY.Text = H3_VFiducial.Fiducial.y.ToString("F3");
						txt_H3_FiducialU.Text = (H3_VFiducial.Fiducial.u * 180 / 3.14).ToString("F3");
					});
				}
				else
				{
					RefreshDifferentThreadUI(txt_H1_FiducialResult, () =>
					{
						txt_H3_FiducialResult.BackColor = Color.Red;
						txt_H3_FiducialResult.Text = "NG";
						txt_H3_FiducialX.Text = "Null";
						txt_H3_FiducialY.Text = "Null";
					});
				}
				
			});
		}

		private void button33_Click(object sender, EventArgs e)
		{
			if (dgv_H3_Calibration.CurrentRow != null)
			{
				DataTable dt = RecipeData.Tables["tb_H3_VisCalib"];
				DataRow dr = dt.Rows[dgv_H3_Calibration.CurrentRow.Index];
				dr[0] = H3_VCalibration.Calibration.x.ToString("F3");
				dr[1] = H3_VCalibration.Calibration.y.ToString("F3");

			}
			else
			{
				MessageBox.Show("Please trigger fiducial calibration");
			}

			SysPara.items++;
		}

		private void button2_Click(object sender, EventArgs e)
		{
			    end_IL_000c:;
			
				NPSDK.Alarm.Show("4013");
				goto end_IL_000c;
		
		}
		private void button5_Click(object sender, EventArgs e)
		{
			DialogResult reult = MessageBox.Show("Sure Add Vision?", "", MessageBoxButtons.YesNo, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);

			if (reult == DialogResult.No)
			{
				return;
			}

			if (H1_VFiducial4.IsAccept)
			{
				DataTable dt = RecipeData.Tables["tb_H1_Visdata4"];
				DataRow dr = dt.NewRow();
				dr[0] = H1_VFiducial4.Fiducial.x.ToString("F3");
				dr[1] = H1_VFiducial4.Fiducial.y.ToString("F3");
				dr[2] = (H1_VFiducial4.Fiducial.u * 180 / 3.14).ToString("F3");

				dt.Rows.Add(dr);
				SysPara.items++;
				return;
			}
			else
			{

				MessageBox.Show("Vision error!");
				return;
			}
		}

		private void button3_Click_2(object sender, EventArgs e)
		{
			if (dgv_H1_VisionData4.CurrentRow != null)
			{
				DataTable dt = RecipeData.Tables["tb_H1_Visdata4"];
				DataRow dr = dt.Rows[dgv_H1_VisionData4.CurrentRow.Index];
				dr[0] = H1_VFiducial4.Fiducial.x.ToString("F3");
				dr[1] = H1_VFiducial4.Fiducial.y.ToString("F3");
				dr[2] = H1_VFiducial4.Fiducial.u.ToString("F3");
				SysPara.items++;
			}
			else
			{
				MessageBox.Show("Vision error!");
			}
		}

		private void button4_Click_1(object sender, EventArgs e)
		{
			if (dgv_H1_VisionData3.CurrentRow != null)
			{
				DataTable dt = RecipeData.Tables["tb_H1_VisionCheck"];
				dt.Rows.RemoveAt(dgv_H1_VisionData3.CurrentRow.Index);
			}

			SysPara.items++;
		}

		
	}
}
