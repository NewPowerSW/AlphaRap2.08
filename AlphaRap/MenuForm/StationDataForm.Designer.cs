namespace AlphaRap
{
    partial class StationDataForm
    {
        /// <summary>构建应用使用的固定日志组件和 CSV 字段。</summary>
        private void InitializeComponent()
        {
            DataSave.DataFormat dataFormat1 = new DataSave.DataFormat();
            DataSave.DataFormat dataFormat2 = new DataSave.DataFormat();
            DataSave.DataFormat dataFormat3 = new DataSave.DataFormat();
            DataSave.DataFormat dataFormat4 = new DataSave.DataFormat();
            DataSave.DataFormat dataFormat5 = new DataSave.DataFormat();
            DataSave.DataFormat dataFormat6 = new DataSave.DataFormat();
            DataSave.DataFormat dataFormat7 = new DataSave.DataFormat();
            DataSave.DataFormat dataFormat8 = new DataSave.DataFormat();
            DataSave.DataFormat dataFormat9 = new DataSave.DataFormat();
            DataSave.DataFormat dataFormat10 = new DataSave.DataFormat();
            DataSave.DataFormat dataFormat11 = new DataSave.DataFormat();
            DataSave.DataFormat dataFormat12 = new DataSave.DataFormat();
            DataSave.DataFormat dataFormat13 = new DataSave.DataFormat();
            DataSave.DataFormat dataFormat14 = new DataSave.DataFormat();
            DataSave.DataFormat dataFormat15 = new DataSave.DataFormat();
            DataSave.DataFormat dataFormat16 = new DataSave.DataFormat();
            DataSave.DataFormat dataFormat17 = new DataSave.DataFormat();
            DataSave.DataFormat dataFormat18 = new DataSave.DataFormat();
            DataSave.DataFormat dataFormat19 = new DataSave.DataFormat();
            DataSave.DataFormat dataFormat20 = new DataSave.DataFormat();
            DataSave.DataFormat dataFormat21 = new DataSave.DataFormat();
            this.MESLOG = new DataSave.TypeCollection1();
            this.Alarm = new DataSave.TypeCollection1();
            this.LogRun = new DataSave.TypeCollection1();
            this.LogError = new DataSave.TypeCollection1();
            this.UserLogin = new DataSave.TypeCollection1();
            this.UserOperationLog = new DataSave.TypeCollection1();
            //
            // MESLOG
            //
            dataFormat1.Name = "Time";
            dataFormat2.Name = "Content";
            this.MESLOG.Items.Add(dataFormat1);
            this.MESLOG.Items.Add(dataFormat2);
            this.MESLOG.SavePath = "D:\\Log\\MESLOG";
            this.MESLOG.Text = "MESLOG";
            //
            // Alarm
            //
            dataFormat3.Name = "Time";
            dataFormat4.Name = "User";
            dataFormat5.Name = "RecipeID";
            dataFormat6.Name = "RecipeName";
            dataFormat7.Name = "Mode";
            dataFormat8.Name = "Type";
            dataFormat9.Name = "Code";
            dataFormat10.Name = "Content";
            this.Alarm.Items.Add(dataFormat3);
            this.Alarm.Items.Add(dataFormat4);
            this.Alarm.Items.Add(dataFormat5);
            this.Alarm.Items.Add(dataFormat6);
            this.Alarm.Items.Add(dataFormat7);
            this.Alarm.Items.Add(dataFormat8);
            this.Alarm.Items.Add(dataFormat9);
            this.Alarm.Items.Add(dataFormat10);
            this.Alarm.SavePath = "D:\\Log\\Alarm";
            this.Alarm.Text = "Alarm";
            //
            // LogRun
            //
            dataFormat11.Name = "Time";
            dataFormat12.Name = "Value";
            this.LogRun.Items.Add(dataFormat11);
            this.LogRun.Items.Add(dataFormat12);
            this.LogRun.SavePath = "D:\\Log\\LogRun";
            this.LogRun.Text = "LogRun";
            //
            // LogError
            //
            dataFormat13.Name = "Time";
            dataFormat14.Name = "Content";
            this.LogError.Items.Add(dataFormat13);
            this.LogError.Items.Add(dataFormat14);
            this.LogError.SavePath = "D:\\Log\\LogError";
            this.LogError.Text = "LogError";
            //
            // UserLogin
            //
            dataFormat15.Name = "Time";
            dataFormat16.Name = "UserName";
            dataFormat17.Name = "UserPermission";
            this.UserLogin.Items.Add(dataFormat15);
            this.UserLogin.Items.Add(dataFormat16);
            this.UserLogin.Items.Add(dataFormat17);
            this.UserLogin.SavePath = "D:\\Log\\UserLogin";
            this.UserLogin.Text = "UserLogin";
            //
            // UserOperationLog
            //
            dataFormat18.Name = "Time";
            dataFormat19.Name = "UserID";
            dataFormat20.Name = "UserPermission";
            dataFormat21.Name = "Action";
            this.UserOperationLog.Items.Add(dataFormat18);
            this.UserOperationLog.Items.Add(dataFormat19);
            this.UserOperationLog.Items.Add(dataFormat20);
            this.UserOperationLog.Items.Add(dataFormat21);
            this.UserOperationLog.SavePath = "D:\\Log\\OperationLog";
            this.UserOperationLog.Text = "OperationLog";
            this.Name = "DataForm";
            this.Text = "DataForm";
        }

        private DataSave.TypeCollection1 MESLOG;
        private DataSave.TypeCollection1 Alarm;
        private DataSave.TypeCollection1 LogRun;
        private DataSave.TypeCollection1 LogError;
        private DataSave.TypeCollection1 UserLogin;
        private DataSave.TypeCollection1 UserOperationLog;
    }
}
