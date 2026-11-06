using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using NPSDK;
namespace NPSDK
{
    public partial class Flow_BaseForm : Form
    {
      
        public Flow_BaseForm()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            NPSDK.Flow_Module.FlowChart_ModuleList.Add(this);
        }


        public bool StopStatus = false;
        public bool PauseStatus = false;
        public bool RunningStatus = false;
        public bool InitialStatus = false;


        public virtual void Initial() { InitialStatus = true; }
        public virtual void StartRun() { }
        public virtual void StopRun() { }
        public virtual void PauseRun() { }

        public virtual void ContinusAftePause() { }



    }



}
