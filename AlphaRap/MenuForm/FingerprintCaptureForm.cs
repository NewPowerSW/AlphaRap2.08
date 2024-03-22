using DPFP;
using DPFP.Processing;
using DPFP.Verification;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace AlphaRap
{
    delegate void Function();   // a simple delegate for marshalling calls from event handlers to the GUI thread

    public partial class FingerprintCaptureForm : Form, DPFP.Capture.EventHandler
    {
        Template tmp;
        //声明一个字段来存储方法中参数
        // public Prose _proInfo;
        //声明一个委托
        public delegate void Prose(string strInfo);
        public event Prose FingerPrinfInfo;
        public delegate void OnTemplateEventHandler(int UserIndex);
        public event OnTemplateEventHandler OnTemplate;
        private bool IsVerifcation = false;
        private Verification Verificator;
        private List<clsTemplate> lstTemplate = new List<clsTemplate>();
        private bool IsEnroll = false;
        public Enrollment Enroller;
        public string RegisterName = string.Empty;
        public bool InitFingerPintF = false;
        public string strInfo = string.Empty;
        protected class clsTemplate
        {
            public int UserIndex = -1;
            public Template template = new Template();
        }

        public FingerprintCaptureForm()
        {
            InitializeComponent();
            //Init();
            //InitFingerprintF();
        }
        public bool InitFingerprintF()
        {
            try
            {
                Verificator = new Verification();
                Enroller = new Enrollment();
                InitFingerPintF = true;
            }
            catch (Exception ex)
            {
                if (SysPara.LanguageShow == LanguageType.Chinese)
                {
                    //MessageBox.Show("指纹初始化失败,请检查驱动是否安装!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    InitFingerPintF = false;
                }
                else if (SysPara.LanguageShow == LanguageType.English)
                {
                    //MessageBox.Show("Fingerprint Initialization Failed,Please Check Whether The Driver Installation!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    InitFingerPintF = false;
                }
            }
            return InitFingerPintF;
        }
        protected virtual void Init()
        {
            try
            {
                Capturer = new DPFP.Capture.Capture();				// Create a capture operation.

                if (null != Capturer)
                    Capturer.EventHandler = this;					// Subscribe for capturing events.
            }
            catch
            {
                MessageBox.Show("Can't initiate fingerprint capture operation!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected virtual void Process(DPFP.Sample Sample)
        {
            if (IsVerifcation)
            {
                // Process the sample and create a feature set for the enrollment purpose.
                DPFP.FeatureSet features = ExtractFeatures(Sample, DPFP.Processing.DataPurpose.Verification);
                if (features != null)
                {
                    for (int i = 0; i < lstTemplate.Count; i++)
                    {
                        DPFP.Verification.Verification.Result result = new DPFP.Verification.Verification.Result();
                        Verificator.Verify(features, lstTemplate[i].template, ref result);
                        if (result.Verified)
                        {
                            OnTemplate(lstTemplate[i].UserIndex);
                            break;
                        }
                        else if (i > 0)
                        {
                            if (SysPara.LanguageShow == LanguageType.Chinese)
                            {
                                strInfo = "指纹未注册,请重试 !";
                                FingerPrinfInfo(strInfo);
                            }
                            else if (SysPara.LanguageShow == LanguageType.English)
                            {
                                strInfo = "Fingerprint not registered,Please try again !";
                                FingerPrinfInfo(strInfo);

                            }
                        }
                    }
                }
            }

            if (IsEnroll)
            {
                // Process the sample and create a feature set for the enrollment purpose.
                DPFP.FeatureSet features = ExtractFeatures(Sample, DPFP.Processing.DataPurpose.Enrollment);
                // Draw fingerprint sample image.
                DrawPicture(ConvertSampleToBitmap(Sample));
                // Check quality of the sample and add to enroller if it's good
                if (features != null)
                {
                    try
                    {
                        Enroller.AddFeatures(features);     // Add feature set to template.
                    }
                    finally
                    {
                        // Check if template has been created.
                        switch (Enroller.TemplateStatus)
                        {
                            case DPFP.Processing.Enrollment.Status.Ready:   // report success and stop capturing
                                try
                                {
                                    tmp = Enroller.Template;

                                    string FilePath = string.Format("{0}\\Fingerprint\\{1}.fpt", System.IO.Directory.GetCurrentDirectory(), RegisterName);
                                    if (!Directory.Exists(Path.GetDirectoryName(FilePath)))
                                        Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                                    using (FileStream fs = File.Open(FilePath, FileMode.Create, FileAccess.Write))
                                    { Enroller.Template.Serialize(fs); }
                                    RefreshDifferentThreadUI(this, () => { this.Close(); });
                                }
                                catch (Exception)
                                {
                                    Enroller.Clear();
                                    SetStatus("Enroller serialize failed!");
                                }
                                break;
                            case DPFP.Processing.Enrollment.Status.Failed:  // report failure and restart capturing
                                Enroller.Clear();
                                SetStatus("Registration failed, please try again!");
                                break;
                            case DPFP.Processing.Enrollment.Status.Insufficient:
                                SetStatus(String.Format("Fingerprint samples needed: {0}", Enroller.FeaturesNeeded));
                                break;
                        }
                    }
                }
            }
        }

        public void ClearTemplate()
        {
            lstTemplate.Clear();
        }

        public void AddTemplate(int UserIndex, string TemplateName)
        {
            try
            {
                string FilePath = string.Format("{0}\\Fingerprint\\{1}.fpt", System.IO.Directory.GetCurrentDirectory(), TemplateName);
                if (File.Exists(FilePath))
                {
                    using (FileStream fs = File.OpenRead(FilePath))
                    {
                        DPFP.Template template = new DPFP.Template(fs);
                        clsTemplate NewTemplate = new clsTemplate();
                        NewTemplate.UserIndex = UserIndex;
                        NewTemplate.template = template;
                        lstTemplate.Add(NewTemplate);
                    }
                }
            }
            catch (Exception) { }
        }

        public bool StartVerification()
        {
            IsEnroll = false;
            IsVerifcation = true;
            Start();
            return IsVerifcation;
        }

        public void StopVerification()
        {
            IsVerifcation = false;
            Stop();
        }

        protected void Start()
        {
            if (null != Capturer)
            {
                try
                {
                    Capturer.StartCapture();
                }
                catch { }
            }
        }

        protected void Stop()
        {
            if (null != Capturer)
            {
                try
                {
                    Capturer.StopCapture();
                }
                catch { }
            }
        }

        #region Form Event Handlers:

        private void FingerprintCaptureForm_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                Start();
                SetStatus("Please press your finger!");
                Enroller.Clear();
                IsEnroll = true;
            }
            else
            {
                Stop();
                IsEnroll = false;
            }
        }

        private void FingerprintCaptureForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Visible = false;
            e.Cancel = true;
        }

        private void CaptureForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Stop();
        }
        #endregion

        #region EventHandler Members:

        public void OnComplete(object Capture, string ReaderSerialNumber, DPFP.Sample Sample)
        {
            Process(Sample);
        }

        public void OnFingerGone(object Capture, string ReaderSerialNumber)
        {

        }

        public void OnFingerTouch(object Capture, string ReaderSerialNumber)
        {

        }

        public void OnReaderConnect(object Capture, string ReaderSerialNumber)
        {

        }

        public void OnReaderDisconnect(object Capture, string ReaderSerialNumber)
        {

        }

        public void OnSampleQuality(object Capture, string ReaderSerialNumber, DPFP.Capture.CaptureFeedback CaptureFeedback)
        {

        }
        #endregion

        protected Bitmap ConvertSampleToBitmap(DPFP.Sample Sample)
        {
            DPFP.Capture.SampleConversion Convertor = new DPFP.Capture.SampleConversion();  // Create a sample convertor.
            Bitmap bitmap = null;                                                           // TODO: the size doesn't matter
            Convertor.ConvertToPicture(Sample, ref bitmap);                                 // TODO: return bitmap as a result
            return bitmap;
        }

        protected DPFP.FeatureSet ExtractFeatures(DPFP.Sample Sample, DPFP.Processing.DataPurpose Purpose)
        {
            DPFP.Processing.FeatureExtraction Extractor = new DPFP.Processing.FeatureExtraction();  // Create a feature extractor
            DPFP.Capture.CaptureFeedback feedback = DPFP.Capture.CaptureFeedback.None;
            DPFP.FeatureSet features = new DPFP.FeatureSet();
            Extractor.CreateFeatureSet(Sample, Purpose, ref feedback, ref features);            // TODO: return features as a result?
            if (feedback == DPFP.Capture.CaptureFeedback.Good)
                return features;
            else
                return null;
        }

        protected void SetStatus(string status)
        {
            this.Invoke(new Function(delegate ()
            {
                StatusLine.Text = status;
            }));
        }

        private void DrawPicture(Bitmap bitmap)
        {
            this.Invoke(new Function(delegate ()
            {
                Picture.Image = new Bitmap(bitmap, Picture.Size);   // fit the image into the picture box
            }));
        }

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
        private DPFP.Capture.Capture Capturer;
    }
}