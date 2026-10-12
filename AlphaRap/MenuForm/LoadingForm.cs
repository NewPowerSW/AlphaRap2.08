using System.Drawing;
using System.Windows.Forms;
using System.Threading;

namespace AlphaRap
{
    public partial class LoadingForm : Form
    {
        private string _Caption = string.Empty;
        public bool StopRefresh;
        public LoadingForm()
        {
            InitializeComponent();
            UiTheme.ApplyDpi(this);
        }

        public void SetCaption(string Caption)
        {
            _Caption = Caption;
        }

        /// <summary>
        /// 加载动画：**必须在后台线程里直接往控件表面上画**。
        /// 原因：Program.Main 里 Show() 之后主线程立刻进 InitialProject() 干活，
        /// **整个加载期间没有消息泵**，窗体和 PictureBox 都不会自己重绘 ——
        /// 图（Logo）和两行文字全靠这里 CreateGraphics() 强画上去，去掉就什么都看不见。
        /// </summary>
        public void RefreshUI()
        {
            Graphics g1 = picLogo.CreateGraphics();
            Graphics g2 = picText1.CreateGraphics();
            Graphics g3 = picText2.CreateGraphics();
            Font FontText1 = new System.Drawing.Font("Tahoma", 18, FontStyle.Bold);
            Font FontText2 = new System.Drawing.Font("Arial Black", 12, FontStyle.Regular);
            g1.Clear(Color.White);
            g1.DrawImage(picLogo.Image, 10, 10, picLogo.Width, picLogo.Height);

            int Count = 0;
            try
            {
                while (!StopRefresh)
                {
                    string LoadMessage = "Loading: " + _Caption + " ";
                    for (int i = 0; i <= (Count % 5); i++)
                    {
                        LoadMessage += " .";
                    }
                    g3.Clear(Color.White);
                    g3.DrawString(LoadMessage, FontText2, System.Drawing.Brushes.Black, 0, 0);
                    g2.DrawString("AlphaRap 1.0/ S D K 1.0.0.1", FontText1, System.Drawing.Brushes.DarkBlue, 10, 10);
                    Count++;
                    Thread.Sleep(100);
                }
            }
            finally
            {
                // 线程已退出（Main 里先 Join 再 Close），这里把 GDI 资源收掉，别泄漏
                try
                {
                    g1.Dispose();
                    g2.Dispose();
                    g3.Dispose();
                    FontText1.Dispose();
                    FontText2.Dispose();
                }
                catch { }
            }
        }
    }
}
