using AlphaRapLibrary;
using NPSDK;
using System.Collections.Generic;

namespace AlphaRap
{
	public partial class ManualForm : ModuleBaseForm
	{
		internal readonly object OB_StoptButton_Light;
		public List<Adlink_Output> Conveyor_List = new List<Adlink_Output>();
		public ManualForm()
		{
			CheckForIllegalCrossThreadCalls = false;
			InitializeComponent();
			Conveyor_List.Add(OB_UpConveyor_Work_ConveyorForward);
			Conveyor_List.Add(OB_UpConveyor_Work_ConveyorReverse);
			Conveyor_List.Add(OB_UpConveyor_LocalMachineReady_SMEMA);
			Conveyor_List.Add(OB_UpConveyor_LocalMachineAvailable_SMEMA);
		}
	}
}
