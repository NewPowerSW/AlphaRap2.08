namespace AlphaRap
{
    partial class DataForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
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
			this.data1 = new DataSave.Data();
			this.MESLOG = new DataSave.TypeCollection1();
			this.Alarm = new DataSave.TypeCollection1();
			this.LogRun = new DataSave.TypeCollection1();
			this.LogError = new DataSave.TypeCollection1();
			this.UserLogin = new DataSave.TypeCollection1();
			this.SuspendLayout();
			// 
			// data1
			// 
			this.data1.BackColor = System.Drawing.Color.White;
			this.data1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.data1.Location = new System.Drawing.Point(0, 0);
			this.data1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.data1.Name = "data1";
			this.data1.ParameterSet.Add(this.MESLOG);
			this.data1.ParameterSet.Add(this.Alarm);
			this.data1.ParameterSet.Add(this.LogRun);
			this.data1.ParameterSet.Add(this.LogError);
			this.data1.ParameterSet.Add(this.UserLogin);
			this.data1.Size = new System.Drawing.Size(1224, 740);
			this.data1.TabIndex = 0;
			this.data1.Load += new System.EventHandler(this.data1_Load);
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
			// DataForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1224, 740);
			this.Controls.Add(this.data1);
			this.Name = "DataForm";
			this.Text = "DataForm";
			this.ResumeLayout(false);

        }

        #endregion

        private DataSave.Data data1;
        public DataSave.TypeCollection1 MESLOG;
        public DataSave.TypeCollection1 Alarm;
        public DataSave.TypeCollection1 LogRun;
        public DataSave.TypeCollection1 LogError;
		public DataSave.TypeCollection1 UserLogin;
	}
}