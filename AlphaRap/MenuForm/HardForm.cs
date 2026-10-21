using System;
using System.Windows.Forms;
using AlphaRapLibrary;

namespace AlphaRap
{

    public partial class HardForm : ModuleBaseForm
    {


        public HardForm()
        {
            InitializeComponent();
            if (!UiTheme.IsDesignTime) MiddleLayer.RegisterAndApplyLanguage(this, Name);

        }

        public void StopAllMotor()
        {
            MiddleLayer.StopAllMotor();
        }

        public void SaveHardData()
        {
        }
    }
}
