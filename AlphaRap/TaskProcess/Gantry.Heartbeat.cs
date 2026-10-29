using NPSDK;

namespace AlphaRap.TaskProcess
{
    partial class Gantry
    {
        private Flow_Chart FlowAuto_Heart5_1;
        private Flow_Chart FlowAuto_Heart5_2;
        private Flow_Chart FlowAuto_Heart5_3;
        private Flow_Chart FlowAuto_Heart5_4;

        /// <summary>保留原生事件实例供首次导入，后续页面布局与节点集合由流程工程恢复。</summary>
        private void InitializeHeartbeatNodes()
        {
            this.FlowAuto_Heart5_1 = new Flow_Chart();
            this.FlowAuto_Heart5_2 = new Flow_Chart();
            this.FlowAuto_Heart5_3 = new Flow_Chart();
            this.FlowAuto_Heart5_4 = new Flow_Chart();
            this.FlowAuto_Heart5_1.AlarmCode = "4004";
            this.FlowAuto_Heart5_1.AlarmStaus = false;
            this.FlowAuto_Heart5_1.AlarmTimeOut = 3000;
            this.FlowAuto_Heart5_1.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.FlowAuto_Heart5_1.Casse1 = null;
            this.FlowAuto_Heart5_1.Casse2 = null;
            this.FlowAuto_Heart5_1.Casse3 = null;
            this.FlowAuto_Heart5_1.FlowChart_ElapsedMilliseconds = ((long)(75373));
            this.FlowAuto_Heart5_1.Font = new System.Drawing.Font("Segoe UI", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FlowAuto_Heart5_1.Location = new System.Drawing.Point(249, 143);
            this.FlowAuto_Heart5_1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.FlowAuto_Heart5_1.Name = "FlowAuto_Heart5_1";
            this.FlowAuto_Heart5_1.Next = this.FlowAuto_Heart5_2;
            this.FlowAuto_Heart5_1.Size = new System.Drawing.Size(276, 35);
            this.FlowAuto_Heart5_1.TabIndex = 37;
            this.FlowAuto_Heart5_1.text = "写入PLC心跳5.1";
            this.FlowAuto_Heart5_1.FlowChartRun += new NPSDK.Flow_Chart.FlowChartHandle(this.FlowAuto_Heart5_1_FlowChartRun);
            this.FlowAuto_Heart5_2.AlarmCode = "4004";
            this.FlowAuto_Heart5_2.AlarmStaus = false;
            this.FlowAuto_Heart5_2.AlarmTimeOut = 3000;
            this.FlowAuto_Heart5_2.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.FlowAuto_Heart5_2.Casse1 = null;
            this.FlowAuto_Heart5_2.Casse2 = null;
            this.FlowAuto_Heart5_2.Casse3 = null;
            this.FlowAuto_Heart5_2.FlowChart_ElapsedMilliseconds = ((long)(75372));
            this.FlowAuto_Heart5_2.Font = new System.Drawing.Font("Segoe UI", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FlowAuto_Heart5_2.Location = new System.Drawing.Point(249, 202);
            this.FlowAuto_Heart5_2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.FlowAuto_Heart5_2.Name = "FlowAuto_Heart5_2";
            this.FlowAuto_Heart5_2.Next = this.FlowAuto_Heart5_3;
            this.FlowAuto_Heart5_2.Size = new System.Drawing.Size(276, 35);
            this.FlowAuto_Heart5_2.TabIndex = 38;
            this.FlowAuto_Heart5_2.text = "时间间隔5.2";
            this.FlowAuto_Heart5_2.FlowChartRun += new NPSDK.Flow_Chart.FlowChartHandle(this.FlowAuto_Heart5_2_FlowChartRun);
            this.FlowAuto_Heart5_3.AlarmCode = "4004";
            this.FlowAuto_Heart5_3.AlarmStaus = false;
            this.FlowAuto_Heart5_3.AlarmTimeOut = 3000;
            this.FlowAuto_Heart5_3.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.FlowAuto_Heart5_3.Casse1 = null;
            this.FlowAuto_Heart5_3.Casse2 = null;
            this.FlowAuto_Heart5_3.Casse3 = null;
            this.FlowAuto_Heart5_3.FlowChart_ElapsedMilliseconds = ((long)(75371));
            this.FlowAuto_Heart5_3.Font = new System.Drawing.Font("Segoe UI", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FlowAuto_Heart5_3.Location = new System.Drawing.Point(249, 269);
            this.FlowAuto_Heart5_3.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.FlowAuto_Heart5_3.Name = "FlowAuto_Heart5_3";
            this.FlowAuto_Heart5_3.Next = this.FlowAuto_Heart5_4;
            this.FlowAuto_Heart5_3.Size = new System.Drawing.Size(276, 35);
            this.FlowAuto_Heart5_3.TabIndex = 39;
            this.FlowAuto_Heart5_3.text = "读取PLC心跳5.3";
            this.FlowAuto_Heart5_3.FlowChartRun += new NPSDK.Flow_Chart.FlowChartHandle(this.FlowAuto_Heart5_3_FlowChartRun);
            this.FlowAuto_Heart5_4.AlarmCode = "4004";
            this.FlowAuto_Heart5_4.AlarmStaus = false;
            this.FlowAuto_Heart5_4.AlarmTimeOut = 3000;
            this.FlowAuto_Heart5_4.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.FlowAuto_Heart5_4.Casse1 = null;
            this.FlowAuto_Heart5_4.Casse2 = null;
            this.FlowAuto_Heart5_4.Casse3 = null;
            this.FlowAuto_Heart5_4.FlowChart_ElapsedMilliseconds = ((long)(75374));
            this.FlowAuto_Heart5_4.Font = new System.Drawing.Font("Segoe UI", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FlowAuto_Heart5_4.Location = new System.Drawing.Point(567, 269);
            this.FlowAuto_Heart5_4.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.FlowAuto_Heart5_4.Name = "FlowAuto_Heart5_4";
            this.FlowAuto_Heart5_4.Next = this.FlowAuto_Heart5_1;
            this.FlowAuto_Heart5_4.Size = new System.Drawing.Size(183, 35);
            this.FlowAuto_Heart5_4.TabIndex = 41;
            this.FlowAuto_Heart5_4.text = "Loop5.4";
            this.FlowAuto_Heart5_4.FlowChartRun += new NPSDK.Flow_Chart.FlowChartHandle(this.FlowAuto_Heart5_4_FlowChartRun);
            this.heartbeatCanvas.RegisterLegacyNodes(FlowAuto_Heart5_1, FlowAuto_Heart5_2, FlowAuto_Heart5_3, FlowAuto_Heart5_4);
        }
    }
}
