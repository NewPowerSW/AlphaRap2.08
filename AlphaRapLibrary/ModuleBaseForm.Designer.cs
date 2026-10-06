namespace AlphaRapLibrary
{
    partial class ModuleBaseForm
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
            this.SettingData = new System.Data.DataSet();
            this.RecipeData = new System.Data.DataSet();
            ((System.ComponentModel.ISupportInitialize)(this.SettingData)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.RecipeData)).BeginInit();
            this.SuspendLayout();
            // 
            // SettingData
            // 
            this.SettingData.DataSetName = "NewDataSet";
            // 
            // RecipeData
            // 
            this.RecipeData.DataSetName = "NewDataSet";
            // 
            // ModuleBaseForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(1067, 562);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "ModuleBaseForm";
            this.Text = "ModuleBaseForm";
            ((System.ComponentModel.ISupportInitialize)(this.SettingData)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.RecipeData)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        public System.Data.DataSet SettingData;
        public System.Data.DataSet RecipeData;
    }
}