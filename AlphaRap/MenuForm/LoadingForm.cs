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
            //picLogo.Image = AlphaRap.Properties.Resources.NPlogotm1;
        }

        public void SetCaption(string Caption)
        {
            _Caption = Caption;
        }

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
    }
}
