using AlphaRap;
using Cognex.VisionPro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlphaRap
{
    public class H3_Vision_Fiducial2 : VisionproInterface
    {
        public AlphaRap.Pos4D Fiducial = new AlphaRap.Pos4D();

        public H3_Vision_Fiducial2()
        {
            VisionRunDisplayIndex = 0;
            RunLiveCCDIndex = 2;
        }
        
        public override void VisionRun()
        {
            TB.Run();
            if (TB.RunStatus.Result == Cognex.VisionPro.CogToolResultConstants.Accept)
            {
                Fiducial.x = (double)GetOutput("x");
                Fiducial.y = (double)GetOutput("y");
                Fiducial.u = (double)GetOutput("A");
                IsAccept = true;
            }
        }
        public override void CreatCentrelLine(CogRecordDisplay Crd)
        {
            CogLine vline = new CogLine();
            CogLine hline = new CogLine();
            vline.Color = CogColorConstants.Red;
            hline.Color = CogColorConstants.Red;
            vline.SetFromStartXYEndXY(640, 0, 640, 960);
            hline.SetFromStartXYEndXY(0, 480, 1280, 480);          
            Crd.InteractiveGraphics.Add(vline, "vline", true);
            Crd.InteractiveGraphics.Add(hline, "hline", true);
        }
    }
}
