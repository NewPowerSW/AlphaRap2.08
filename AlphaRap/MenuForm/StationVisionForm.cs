using System;
using System.Collections.Generic;
using System.Data;

namespace AlphaRap
{
    public partial class StationVisionForm : VPForm
    {
        /// <summary>保留本站配方表和原视觉工位身份。</summary>
        public StationVisionForm()
        {
            InitializeComponent();
            if (UiTheme.IsDesignTime) return;
            InitializeRecipeSchema();
            H1_VCalibration = new VpStation("Camera1", VpStationKind.Calibration, 3);
            H1_VFiducial = new VpStation("Camera1", VpStationKind.Fiducial, 3);
            H1_VFiducial2 = new VpStation("Camera1", VpStationKind.Fiducial, 3);
            H1_VFiducial3 = new VpStation("Camera1", VpStationKind.Fiducial, 3);
            H1_VFiducial4 = new VpStation("Camera1", VpStationKind.Fiducial, 3);
        }

        /// <summary>旧配方使用本站的固定定位工位。</summary>
        protected override VpStation DefaultProductionStation { get { return H1_VFiducial; } }

        /// <summary>载入本站判据后刷新视觉页面。</summary>
        public override void OnRecipeChanged()
        {
            string error;
            TryLoadVisionCriteria(out error);
            base.OnRecipeChanged();
        }

        /// <summary>校验本站配方判据及框架视觉运行条件。</summary>
        public override bool CanStartProduction(out string error)
        {
            VisionPostData criteria;
            return TryGetProductionVisionData(out criteria, out error) && base.CanStartProduction(out error);
        }

        public readonly VpStation H1_VCalibration;
        public readonly VpStation H1_VFiducial;
        public readonly VpStation H1_VFiducial2;
        public readonly VpStation H1_VFiducial3;
        public readonly VpStation H1_VFiducial4;

		public struct VisionPostData
        {
            public int Point;
            public double X;
            public double Y;
            public double R;

            public double X_Low;
            public double X_Hi;
            public double Y_Low;
            public double Y_Hi;
            public double R_Low;
            public double R_Hi;

            public bool Enable;
        }

        /// <summary>
        /// 生产流程用的视觉判据（TaskProcess/Gantry.cs 读取 dgv_H1_VisionData_List[0]），
        /// 数据来自配方表 tb_H1_Visdata，由 <see cref="Getdgv_H1_VisionDataList"/> 加载。
        /// </summary>
        public List<VisionPostData> dgv_H1_VisionData_List = new List<VisionPostData>();
        private string _visionDataError;

        /// <summary>把配方表 tb_H1_Visdata 刷进 <see cref="dgv_H1_VisionData_List"/>。</summary>
        public void Getdgv_H1_VisionDataList()
        {
            string error;
            TryLoadVisionCriteria(out error);
        }

        /// <summary>校验并整体替换配方视觉判据；失败时清空快照，防止沿用前一配方。</summary>
        public bool TryLoadVisionCriteria(out string error)
        {
            try
            {
                DataTable table = RecipeData.Tables["tb_H1_Visdata"];
                if (table == null || table.Rows.Count == 0) throw new InvalidOperationException(ApplicationHost.Text("VPForm", "vpError_NoCriteria", "配方缺少视觉判据 tb_H1_Visdata。", "Recipe is missing vision criteria tb_H1_Visdata.", "La receta no contiene los criterios de visión tb_H1_Visdata."));
                List<VisionPostData> values = new List<VisionPostData>();
                for (int i = 0; i < table.Rows.Count; i++) values.Add(ReadVisionCriterion(table.Rows[i], i));
                dgv_H1_VisionData_List = values;
                _visionDataError = error = "";
                return true;
            }
            catch (Exception ex)
            {
                dgv_H1_VisionData_List = new List<VisionPostData>();
                _visionDataError = error = ex.Message;
                return false;
            }
        }

        /// <summary>按列名读取一行生产判据，启用时上下限必须有效。</summary>
        private static VisionPostData ReadVisionCriterion(DataRow row, int index)
        {
            VisionPostData data = new VisionPostData { Point = index, Enable = Convert.ToBoolean(row["Enable"]) };
            if (!data.Enable) return data;
            data.X = ReadVisionNumber(row, "X");
            data.Y = ReadVisionNumber(row, "Y");
            data.R = ReadVisionNumber(row, "U");
            data.X_Low = ReadVisionNumber(row, "X_Low");
            data.X_Hi = ReadVisionNumber(row, "X_Hi");
            data.Y_Low = ReadVisionNumber(row, "Y_Low");
            data.Y_Hi = ReadVisionNumber(row, "Y_Hi");
            data.R_Low = ReadVisionNumber(row, "U_Low");
            data.R_Hi = ReadVisionNumber(row, "U_Hi");
            if (data.X_Low >= data.X_Hi || data.Y_Low >= data.Y_Hi || data.R_Low >= data.R_Hi)
                throw new InvalidOperationException(ApplicationHost.Text("VPForm", "vpError_RowA", "第 ", "Row ", "Fila ") + (index + 1) + ApplicationHost.Text("VPForm", "vpError_RowB", " 行视觉判据下限必须小于上限。", ": vision lower limit must be below the upper limit.", ": el límite inferior de visión debe ser menor que el superior."));
            return data;
        }

        /// <summary>读取配方中的有限数值，空值和非数值禁止进入生产判定。</summary>
        private static double ReadVisionNumber(DataRow row, string column)
        {
            double value = Convert.ToDouble(row[column]);
            if (double.IsNaN(value) || double.IsInfinity(value)) throw new InvalidOperationException(ApplicationHost.Text("VPForm", "vpError_CriteriaFinite", "视觉判据不是有限数值：", "Vision criterion is not finite: ", "El criterio de visión no es un número finito: ") + column);
            return value;
        }

        /// <summary>动态定位由本站补偿限制判定；内置兼容工位读取配方判据快照。</summary>
        public bool TryGetProductionVisionData(out VisionPostData data, out string error)
        {
            data = new VisionPostData();
            if (HasDynamicProductionBinding) { error = ""; return true; }
            error = _visionDataError;
            List<VisionPostData> values = dgv_H1_VisionData_List;
            if (values == null || values.Count == 0)
            {
                if (string.IsNullOrEmpty(error)) error = ApplicationHost.Text("VPForm", "vpError_CriteriaNotLoaded", "生产视觉判据尚未加载。", "Production vision criteria are not loaded.", "Los criterios de visión de producción no están cargados.");
                return false;
            }
            data = values[0];
            error = "";
            return true;
        }
    }
}
