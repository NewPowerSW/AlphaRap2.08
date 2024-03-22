using AlphaRap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlphaRap
{
    public class H3_Vision_Calibration : VisionproInterface
    {
        public AlphaRap.Pos4D Calibration = new AlphaRap.Pos4D();

        public H3_Vision_Calibration()
        {
            VisionRunDisplayIndex = 0;
            RunLiveCCDIndex = 2;
        }

        [STAThread]
        public override void VisionRun()
        {
            TB.Run();
            if (TB.RunStatus.Result == Cognex.VisionPro.CogToolResultConstants.Accept)
            {
                Calibration.x = (double)GetOutput("x");
                Calibration.y = (double)GetOutput("y");
                IsAccept = true;
            }
        }
    }
}
