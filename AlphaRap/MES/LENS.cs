using AlphaRapLibrary;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace AlphaRap.MES
{
    public partial class LENS : ModuleBaseForm
    {
        public string Post_Message = string.Empty;

        #region 产品序列号校验接口  ProductSerialNumberVerificationinterface
        /// <summary>
        /// Url
        /// </summary>
        string Product_SN_Verification_URL
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_Product_SN_Verification_URL");
        }

        /// <summary>
        /// 资源编码
        /// </summary>
        string Product_SN_Verification_resource
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_Product_SN_Verification_Resource");
        }

        /// <summary>
        /// 工厂代码
        /// </summary>
        string Product_SN_Verification_Site
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_Product_SN_Verification_site");
        }

        /// <summary>
        /// 工序编码
        /// </summary>
        string Product_SN_Verification_operation
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_Product_SN_Verification_operation");
        }

        /// <summary>
        /// 产品序列号
        /// </summary>
        string Product_SN_Verification_SN
        {
            get;
            set;
        }
        /// <summary>
        /// 班次
        /// </summary>
        string Product_SN_Verification_item
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_Product_SN_Verification_item");
        }

        public bool Product_SN_Verification()
        {
            StringBuilder str = new StringBuilder();

            str.Append("{");
            str.Append("\"" + "site" + "\"" + ":" + "\"" + Product_SN_Verification_Site + "\"" + ",");
            str.Append("\"" + "item" + "\"" + ":" + "\"" + Product_SN_Verification_item + "\"" + ",");
            str.Append("\"" + "resource" + "\"" + ":" + "\"" + Product_SN_Verification_resource + "\"" + ",");
            str.Append("\"" + "sn" + "\"" + ":" + "\"" + Product_SN_Verification_SN + "\"" + ",");
            str.Append("\"" + "operation" + "\"" + ":" + "\"" + Product_SN_Verification_operation + "\"");
            str.Append("}");
            MiddleLayer.DataF.SaveMesLog(" Product_SN_Verification:" + str.ToString());

            return ReturnValue(Post(Product_SN_Verification_URL, str.ToString(), out Post_Message));
        }
        #endregion

        #region 物料启动SN接口  MaterialStartupSNInterface
        /// <summary>
        /// Url
        /// </summary>
        string MaterialStartup_SN_URL
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_MaterialStartup_SN_URL");
        }

        /// <summary>
        /// 工厂
        /// </summary>
        string MaterialStartup_SN_Site
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_MaterialStartup_SN_Site");
        }

        /// <summary>
        /// 物料
        /// </summary>
        string MaterialStartup_SN_Item
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_MaterialStartup_SN_Item");
        }

        /// <summary>
        /// 资源
        /// </summary>
        string MaterialStartup_SN_resource
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_MaterialStartup_SN_resource");
        }

        /// <summary>
        /// 产品序列号
        /// </summary>
        string MaterialStartup_SN_SN
        {
            get;
            set;
        }

        /// <summary>
        /// 工序
        /// </summary>
        string MaterialStartup_SN_operation
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_MaterialStartup_SN_operation");
        }

        public bool MaterialStartupSNInterface()
        {
            try
            {
                StringBuilder str = new StringBuilder();

                str.Append("{");
                str.Append("\"" + "site" + "\"" + ":" + "\"" + MaterialStartup_SN_Site + "\"" + ",");
                str.Append("\"" + "item" + "\"" + ":" + "\"" + MaterialStartup_SN_Item + "\"" + ",");
                str.Append("\"" + "resource" + "\"" + ":" + "\"" + MaterialStartup_SN_resource + "\"" + ",");
                str.Append("\"" + "sn" + "\"" + ":" + "\"" + MaterialStartup_SN_SN + "\"" + ",");
                str.Append("\"" + "operation" + "\"" + ":" + "\"" + MaterialStartup_SN_operation + "\"");
                str.Append("}");
                MiddleLayer.DataF.SaveMesLog(" MaterialStartupSNInterface:" + str.ToString());

                return ReturnValue(Post(MaterialStartup_SN_URL, str.ToString(), out Post_Message));
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        #region 产品序列号开始接口  Product SN start interface
        /// <summary>
        /// Url
        /// </summary>
        string Product_SN_Start_Url
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_Product_SN_Start_Url");
        }

        /// <summary>
        /// 工厂
        /// </summary>
        string Product_SN_Start_Site
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_Product_SN_Start_Site");
        }

        /// <summary>
        /// 班次
        /// </summary>
        string Product_SN_Start_Shift
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_Product_SN_Start_Shift");
        }

        /// <summary>
        /// 资源
        /// </summary>
        string Product_SN_Start_Resource
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_Product_SN_Start_Resource");
        } 

        /// <summary>
        /// 产品序列号
        /// </summary>
        public string Product_SN_Start_SN
        {
            get;
            set;
        }

        /// <summary>
        /// 工序
        /// </summary>
        string Product_SN_Start_Operation
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_Product_SN_Start_Operation");
        }
        public bool ReturnValue(string ReturnString)
        {
            return ReturnString.Contains("true") ? true : false;
        }

        public bool Product_SN_Start_Interface()
        {
            StringBuilder str = new StringBuilder();

            str.Append("{");
            str.Append("\"" + "site" + "\"" + ":" + " " + "\"" + Product_SN_Start_Site + "\"" + ",");
            str.Append("\"" + "resource" + "\"" + ":" + " " + "\"" + Product_SN_Start_Resource + "\"" + ",");
            str.Append("\"" + "shift" + "\"" + ":" + " " + "\"" + Product_SN_Start_Shift + "\"" + ",");
            str.Append("\"" + "sn" + "\"" + ":" + " " + "\"" + Product_SN_Start_SN.Trim() + "\"" + ",");
            str.Append("\"" + "operation" + "\"" + ":" + " " + "\"" + Product_SN_Start_Operation + "\"");

            str.Append("}");
            MiddleLayer.DataF.SaveMesLog("Product_SN_Start_Interface:" + str.ToString());

            return ReturnValue(Post(Product_SN_Start_Url, str.ToString(), out Post_Message));
        }
        #endregion

        #region 综合过站接口  Integrated transit interface
        /// <summary>
        /// Url
        /// </summary>
        public string Integrated_Transit_Url
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_Integrated_Transit_Url");
        }

        /// <summary>
        /// 工厂
        /// </summary>
        public string Integrated_Transit_Site
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_Integrated_Transit_Site");
        }

        /// <summary>
        /// 班次
        /// </summary>
        public string Integrated_Transit_Mo
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_Integrated_Transit_Mo");
        }

        /// <summary>
        /// 物料编号
        /// </summary>
        public string Integrated_Transit_Item
        {
            get;
            set;
        }

        /// <summary>
        /// 物料版本
        /// </summary>
        public string Integrated_Transit_ItemRevision
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_Integrated_Transit_ItemRevision");
        }

        /// <summary>
        /// 工序号码
        /// </summary>
        public string Integrated_Transit_Operation
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_Integrated_Transit_Operation");
        }
        /// <summary>
        /// 设备号码
        /// </summary>
        public string Integrated_Transit_Resource
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_Integrated_Transit_Resource");
        }

        /// <summary>
        /// 单片条码
        /// </summary>
        public string Integrated_Transit_Sn
        {
            get;
            set;
        }
        /// <summary>
        /// 班次
        /// </summary>
        string Integrated_Transit_Shift
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_Integrated_Transit_Shift");
        }
        public Dictionary<string, string> dcParameterList = new Dictionary<string, string>();
        /// <summary>
        /// BOM原材料物料号码
        /// </summary>
        public string Integrated_Transit_bomComponent
        {
            get;
            set;
        }
        /// <summary>
        /// BOM原材料物料装配数量
        /// </summary>
        public string Integrated_Transit_assembleqty
        {
            get;
            set;
        }
        /// <summary>
        /// 数据收集组名称
        /// </summary>
        public string Integrated_Transit_dcGroupName
        {
            get;
            set;
        }
        /// <summary>
        /// 数据收集参数列表字段名称
        /// </summary>
        public string Integrated_Transit_dcParameterListname
        {
            get;
            set;
        }

        /// <summary>
        /// 数据收集参数列表字段数值
        /// </summary>
        public string Integrated_Transit_dcParameterListvalue
        {
            get;
            set;
        }
        /// <summary>
        /// 过账条码列表字段名称
        /// </summary>
        public string Integrated_Transit_postSnListname
        {
            get;
            set;
        }
        /// <summary>
        /// 过账条码列表字段数值
        /// </summary>
        public string Integrated_Transit_postSnListvalue
        {
            get;
            set;
        }
        /// <summary>
        /// NCCode列表NC Code
        /// </summary>
        public string Integrated_Transit_testResult_ncCode
        {
            get;
            set;
        }
        readonly object Lock_Integrated=new object();
        public bool Integrated_Transit_Interface()
        {
            lock(Lock_Integrated)
            {
                StringBuilder str = new StringBuilder();

                str.Append("{");
                str.Append("\"" + "site" + "\"" + ":" + "\"" + Integrated_Transit_Site + "\"" + ",");
                str.Append("\"" + "mo" + "\"" + ":" + "\"" + "" + "\"" + ",");
                str.Append("\"" + "item" + "\"" + ":" + "\"" + Integrated_Transit_Item + "\"" + ",");
                str.Append("\"" + "itemRevision" + "\"" + ":" + "\"" + Integrated_Transit_ItemRevision + "\"" + ",");
                str.Append("\"" + "operation" + "\"" + ":" + "\"" + Integrated_Transit_Operation + "\"" + ",");
                str.Append("\"" + "resource" + "\"" + ":" + "\"" + Integrated_Transit_Resource + "\"" + ",");
                str.Append("\"" + "sn" + "\"" + ":" + "\"" + Integrated_Transit_Sn.Trim() + "\"" + ",");
                str.Append("\"" + "shift" + "\"" + ":" + "\"" + Integrated_Transit_Shift + "\"" + ",");
                str.Append("\"" + "assembleComponents" + "\"" + ":" + "[");

                str.Append("],");
                str.Append("\"" + "dcList" + "\"" + ":" + "[");
                str.Append("{");
                str.Append("\"" + "dcGroupName" + "\"" + ":" + "\"" + Integrated_Transit_dcGroupName + "\"" + ",");
                str.Append("\"" + "dcParameterList" + "\"" + ":" + "[");

                int index = 0;
                foreach (KeyValuePair<string, string> item in dcParameterList)
                {
                    index++;

                    if (index >= dcParameterList.Count)
                    {
                        str.Append("{" + "\"" + "name" + "\"" + ":" + "\"" + item.Key + "\"" + "," + "\"" + "value" + "\"" + ":" + "\"" + item.Value + "\"" + "}");
                    }
                    else
                    {
                        str.Append("{" + "\"" + "name" + "\"" + ":" + "\"" + item.Key + "\"" + "," + "\"" + "value" + "\"" + ":" + "\"" + item.Value + "\"" + "}" + ",");
                    }
                };
                dcParameterList.Clear();
                str.Append("]");
                str.Append("}");
                str.Append("],");
                str.Append("\"" + "postSnList" + "\"" + ":" + "[");
                str.Append("],");
                str.Append("\"" + "testResult" + "\"" + ":" + "[");
                str.Append("{");
                str.Append("\"" + "ncCode" + "\"" + ":" + "\"" + Integrated_Transit_testResult_ncCode + "\"");
                str.Append("}");
                str.Append("]");
                str.Append("}");

                MiddleLayer.DataF.SaveMesLog("Integrated_Transit_Interface:" + str.ToString());

                return ReturnValue(Post(Integrated_Transit_Url, str.ToString(), out Post_Message));
            }
        }
        #endregion

        #region 物料条码检查接口  Material barcode inspection interface
        /// <summary>
        /// Url
        /// </summary>
        string Material_Barcode_Inspection_Url
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_Material_Barcode_Inspection_Url");
        }

        /// <summary>
        /// 工厂代码
        /// </summary>
        string Material_Barcode_Inspection_Site
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_Material_Barcode_Inspection_Site");
        }

        /// <summary>
        /// 蓝牙板条码
        /// </summary>
        string Material_Barcode_Inspection_Bluetooth
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_Material_Barcode_Inspection_Bluetooth");
        }

        /// <summary>
        /// 资源
        /// </summary>
        string Material_Barcode_Inspection_Resource
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_Material_Barcode_Inspection_Resource");
        }

        /// <summary>
        /// 产品序列号
        /// </summary>
        string Material_Barcode_Inspection_Sn
        {
            get;
            set;
        }

        /// <summary>
        /// 摄像头条码
        /// </summary>
        string Material_Barcode_Inspection_Camera
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_Material_Barcode_Inspection_Camera");
        }
        /// <summary>
        /// 工序编码
        /// </summary>
        string Material_Barcode_Inspection_Operation
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_Material_Barcode_Inspection_Operation");
        }

        public bool Material_Barcode_Inspection_interface()
        {
            StringBuilder str = new StringBuilder();

            str.Append("{");
            str.Append("\"" + "site" + "\"" + ":" + "\"" + Material_Barcode_Inspection_Site + "\"" + ",");
            str.Append("\"" + "bluetooth" + "\"" + ":" + "\"" + Material_Barcode_Inspection_Bluetooth + "\"" + ",");
            str.Append("\"" + "resource" + "\"" + ":" + "\"" + Material_Barcode_Inspection_Resource + "\"" + ",");
            str.Append("\"" + "sn" + "\"" + ":" + "\"" + Material_Barcode_Inspection_Sn + "\"" + ",");
            str.Append("\"" + "camera" + "\"" + ":" + "\"" + Material_Barcode_Inspection_Camera + "\"" + ",");
            str.Append("\"" + "operation" + "\"" + ":" + "\"" + Material_Barcode_Inspection_Operation + "\"");
            str.Append("}");
            MiddleLayer.DataF.SaveMesLog("Material_Barcode_Inspection_interface:" + str.ToString());

            return ReturnValue(Post(Material_Barcode_Inspection_Url, str.ToString(), out Post_Message));
        }

#endregion
#region 物料上料卸料接口  Material loading and unloading interface

        /// <summary>
        /// 工厂代码
        /// </summary>
        string Material_Loading_Unloading_Site
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_Material_Loading_Unloading_Site");
        }
        /// <summary>
        /// 主物料
        /// </summary>
        string Material_Loading_Unloading_Item
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_Material_Loading_Unloading_Item");
        }
        /// <summary>
        /// 物料1
        /// </summary>
        string Material_Loading_Unloading_Mocde1
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_Material_Loading_Unloading_Mocde1");
        }
        /// <summary>
        /// 物料批次1
        /// </summary>
        string Material_Loading_Unloading_Slot1
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_Material_Loading_Unloading_Slot1");
        }

        /// <summary>
        ///有效期1
        /// </summary>
        string Material_Loading_Unloading_Pvdate1
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_Material_Loading_Unloading_Pvdate1");
        }

        /// <summary>
        /// 数量1
        /// </summary>
        string Material_Loading_Unloading_Qty1
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_Material_Loading_Unloading_Qty1");
        }

        /// <summary>
        ///  物料2
        /// </summary>
        string Material_Loading_Unloading_Mocde2
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_Material_Loading_Unloading_Mocde2");
        }
        /// <summary>
        /// 物料批次2
        /// </summary>
        string Material_Loading_Unloading_Slot2
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_Material_Loading_Unloading_Slot2");
        }
        /// <summary>
        /// 有效期2
        /// </summary>
        string Material_Loading_Unloading_Pvdate2
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_Material_Loading_Unloading_Pvdate2");
        }
        /// <summary>
        /// 数量2
        /// </summary>
        string Material_Loading_Unloading_Qty2
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_Material_Loading_Unloading_Qty2");
        }
        /// <summary>
        /// 资源
        /// </summary>
        string Material_Loading_Unloading_Resource
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_Material_Loading_Unloading_Resource");
        }
        /// <summary>
        /// 上料/下料（1/0）
        /// </summary>
        string Material_Loading_Unloading_Function
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_Material_Loading_Unloading_Function");
        }
        /// <summary>
        /// 工序编码
        /// </summary>
        string Material_Loading_Unloading_Operation
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_Material_Loading_Unloading_Operation");
        }
        public bool Material_loading_Unloading_Interface()
        {
            StringBuilder str = new StringBuilder();

            str.Append("{");
            str.Append("\"" + "site" + "\"" + ":" + "\"" + Material_Loading_Unloading_Site + "\"" + ",");
            str.Append("\"" + "item	" + "\"" + ":" + "\"" + Material_Loading_Unloading_Item + "\"" + ",");
            str.Append("\"" + "slList	" + "\"" + ":" + "[" + ",");
            str.Append("{");
            str.Append("\"" + "mcode" + "\"" + ":" + "\"" + Material_Loading_Unloading_Mocde1 + "\"" + ",");
            str.Append("\"" + "slot" + "\"" + ":" + "\"" + Material_Loading_Unloading_Slot1 + "\"" + ",");
            str.Append("\"" + "pvdate" + "\"" + ":" + "\"" + Material_Loading_Unloading_Pvdate1 + "\"" + ",");
            str.Append("\"" + "qty" + "\"" + ":" + "\"" + Material_Loading_Unloading_Qty1 + "\"");
            str.Append("},");
            str.Append("{");
            str.Append("\"" + "mcode" + "\"" + ":" + "\"" + Material_Loading_Unloading_Mocde2 + "\"" + ",");
            str.Append("\"" + "slot" + "\"" + ":" + "\"" + Material_Loading_Unloading_Slot2 + "\"" + ",");
            str.Append("\"" + "pvdate" + "\"" + ":" + "\"" + Material_Loading_Unloading_Pvdate2 + "\"" + ",");
            str.Append("\"" + "qty" + "\"" + ":" + "\"" + Material_Loading_Unloading_Qty2 + "\"");
            str.Append("}");
            str.Append("],");
            str.Append("\"" + "resource" + "\"" + ":" + "\"" + Material_Loading_Unloading_Resource + "\"" + ",");
            str.Append("\"" + "function" + "\"" + ":" + "\"" + Material_Loading_Unloading_Function + "\"" + ",");
            str.Append("\"" + "operation" + "\"" + ":" + "\"" + Material_Loading_Unloading_Operation + "\"");
            str.Append("}");
            MiddleLayer.DataF.SaveMesLog("Material_Barcode_Inspection_interface:" + str.ToString());

            return ReturnValue(Post(Material_Barcode_Inspection_Url, str.ToString(), out Post_Message));
        }
        #endregion

        #region 电测明细查询接口  Electrical measurement details query interface
        /// <summary>
        /// Url
        /// </summary>
        string Electrical_Measurement_Query_Url
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_Electrical_Measurement_Query_Url");
        }

        /// <summary>
        /// 资源编码
        /// </summary>
        string Electrical_Measurement_Query_Site
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_Electrical_Measurement_Query_Site");
        }

        /// <summary>
        /// 工厂代码
        /// </summary>
        string Electrical_Measurement_Query_Operation
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_Electrical_Measurement_Query_Operation");
        }

        /// <summary>
        /// 工序编码
        /// </summary>
        string Electrical_Measurement_Query_Sn
        {
            get;
            set;
        }

        public bool Electrical_Measurement_Query()
        {
            StringBuilder str = new StringBuilder();

            str.Append("{");
            str.Append("\"" + "site" + "\"" + ":" + "\"" + Electrical_Measurement_Query_Site + "\"" + ",");
            str.Append("\"" + "sn" + "\"" + ":" + "\"" + Electrical_Measurement_Query_Sn + "\"" + ",");
            str.Append("\"" + "operation" + "\"" + ":" + "\"" + Electrical_Measurement_Query_Operation + "\"");
            str.Append("}");
            MiddleLayer.DataF.SaveMesLog(" Electrical_Measurement_Query:" + str.ToString());
            return ReturnValue(Post(Electrical_Measurement_Query_Url, str.ToString(), out Post_Message));
        }
        #endregion

        #region BT测试数据检测及上传接口  BT test data detection and upload interface
        /// <summary>
        /// Url
        /// </summary>
        string BTtext_Detection_Upload_Url
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_BTtext_Detection_Upload_Url");
        }
        /// <summary>
        /// 物料编号
        /// </summary>
        string BTtext_Detection_Upload_Item
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_BTtext_Detection_Upload_Item");
        }
        /// <summary>
        /// 工厂编号
        /// </summary>
        string BTtext_Detection_Upload_Site
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_BTtext_Detection_Upload_Site");
        }

        /// <summary>
        /// 资源编码
        /// </summary>
        string BTtext_Detection_Upload_Resource
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_BTtext_Detection_Upload_Resource");
        }

        /// <summary>
        /// 条码链表
        /// </summary>
        string BTtext_Detection_Upload_PostSNList
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_BTtext_Detection_Upload_PostSNList");
        }
        /// <summary>
        /// 数据收集组名称
        /// </summary>
        string BTtext_Detection_Upload_DcGroupName
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_BTtext_Detection_Upload_DcGroupName");
        }

        /// <summary>
        /// 数据收集参数列表字段名称1
        /// </summary>
        string BTtext_Detection_Upload_DcParameListName1
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_BTtext_Detection_Upload_DcParameListName1");
        }
        /// <summary>
        /// 数据收集参数列表字段数值1
        /// </summary>
        string BTtext_Detection_Upload_DcParameListValue1
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_BTtext_Detection_Upload_DcParameListValue1");
        }
        /// <summary>
        ///数据收集参数列表字段名称2
        /// </summary>
        string BTtext_Detection_Upload_DcParameListName2
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_BTtext_Detection_Upload_DcParameListName2");
        }
        /// <summary>
        /// 数据收集参数列表字段数值2
        /// </summary>
        string BTtext_Detection_Upload_DcParameListValue2
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_BTtext_Detection_Upload_DcParameListValue2");
        }
        /// <summary>
        /// 物料版本
        /// </summary>
        string BTtext_Detection_Upload_ItemRevision
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_BTtext_Detection_Upload_ItemRevision");
        }
        /// <summary>
        /// 班次
        /// </summary>
        string BTtext_Detection_Upload_Shift
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_BTtext_Detection_Upload_Shift");
        }

        /// <summary>
        /// 产品序列号
        /// </summary>
        string BTtext_Detection_Upload_SN
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_BTtext_Detection_Upload_SN");
        }
        /// <summary>
        /// 测试结果
        /// </summary>
        string BTtext_Detection_Upload_TestResult
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_BTtext_Detection_Upload_TestResult");
        }
        /// <summary>
        /// 工序编码
        /// </summary>
        string BTtext_Detection_Upload_Operation
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_BTtext_Detection_Upload_Operation");
        }
        string BTtext_Detection_Upload_AssembleComponents
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_BTtext_Detection_Upload_AssembleComponents");
        }

        public bool BT_test_data_detection_upload()
        {
            StringBuilder str = new StringBuilder();

            str.Append("{");
            str.Append("\"" + "site" + "\"" + ":" + "\"" + BTtext_Detection_Upload_Site + "\"" + ",");
            str.Append("\"" + "item" + "\"" + ":" + "\"" + BTtext_Detection_Upload_Item + "\"" + ",");
            str.Append("\"" + "resource" + "\"" + ":" + "\"" + BTtext_Detection_Upload_Resource + "\"" + ",");
            str.Append("\"" + "postSnList" + "\"" + ":" + BTtext_Detection_Upload_PostSNList + ",");
            str.Append("\"" + "dcList" + "\"" + ":" + "[");
            str.Append("{");
            str.Append("\"" + "dcGroupName" + "\"" + ":" + "\"" + BTtext_Detection_Upload_DcGroupName + "\"" + ",");
            str.Append("\"" + "dcParameterList" + "\"" + ":" + "[");
            str.Append("{");
            str.Append("\"" + "name" + "\"" + ":" + "\"" + BTtext_Detection_Upload_DcParameListName1 + "\"" + ",");
            str.Append("\"" + "value" + "\"" + ":" + "\"" + BTtext_Detection_Upload_DcParameListValue1 + "\"");
            str.Append("},");
            str.Append("{");
            str.Append("\"" + "name" + "\"" + ":" + "\"" + BTtext_Detection_Upload_DcParameListName2 + "\"" + ",");
            str.Append("\"" + "value" + "\"" + ":" + "\"" + BTtext_Detection_Upload_DcParameListValue2 + "\"");
            str.Append("}");
            str.Append("]");
            str.Append("}");
            str.Append("],");
            str.Append("\"" + "itemRevision" + "\"" + ":" + "\"" + BTtext_Detection_Upload_ItemRevision + "\"" + ",");
            str.Append("\"" + "shift" + "\"" + ":" + "\"" + BTtext_Detection_Upload_Shift + "\"" + ",");
            str.Append("\"" + "sn" + "\"" + ":" + "\"" + BTtext_Detection_Upload_SN + "\"" + ",");
            str.Append("\"" + "testResult" + "\"" + ":" + BTtext_Detection_Upload_TestResult + ",");
            str.Append("\"" + "operation" + "\"" + ":" + "\"" + BTtext_Detection_Upload_Operation + "\"" + ",");
            str.Append("\"" + "assembleComponents" + "\"" + ":" + "\"" + BTtext_Detection_Upload_AssembleComponents + "\"" + ",");
            str.Append("}");

            MiddleLayer.DataF.SaveMesLog(" BT_test_data_detection_upload:" + str.ToString());
            return ReturnValue(Post(BTtext_Detection_Upload_Url, str.ToString(), out Post_Message));
        }
        #endregion

        #region 玻璃FPC条码一致性校验接口  Glass FPC barcode consistency verification interface
        /// <summary>
        /// Url
        /// </summary>
        string Glass_FPC_SN_Consistency_Verification_Url
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_Glass_FPC_SN_Consistency_Verification_Url");
        }

        public bool Glass_FPC_SN_Consistency_Verification()
        {
            StringBuilder str = new StringBuilder();

            str.Append("{");
            str.Append("\"" + "glass" + "\"" + ":" + "\"" + Electrical_Measurement_Query_Site + "\"" + ",");
            str.Append("\"" + "fpc" + "\"" + ":" + "\"" + Electrical_Measurement_Query_Sn + "\"" + ",");
            str.Append("\"" + "site" + "\"" + ":" + "\"" + Electrical_Measurement_Query_Operation + "\"");
            str.Append("}");
            MiddleLayer.DataF.SaveMesLog(" Glass_FPC_SN_Consistency_Verification:" + str.ToString());
            return ReturnValue(Post(Glass_FPC_SN_Consistency_Verification_Url, str.ToString(), out Post_Message));
        }
        #endregion

        #region Tray条码绑定  Tray barcode Binding interface
        /// <summary>
        /// Url
        /// </summary>
        public string TrayBarcodeBinding_URL
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "TrayBarcodeBinding_URL");
        }

        /// <summary>
        ///工厂编码
        /// </summary>
        public string TrayBarcodeBinding_Site
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "TrayBarcodeBinding_Site");
        }

        /// <summary>
        /// 托盘编号
        /// </summary>
        public string TrayBarcodeBinding_TrayNumber
        {
            get;
            set;
        }

        /// <summary>
        /// 物料条码list
        /// </summary>

        Dictionary<string, string> TrayBarcodeBinding_dataList = new Dictionary<string, string>();

        public bool Tray_Barcode_Binding_interface()
        {
            StringBuilder str = new StringBuilder();

            str.Append("{");
            str.Append("\"" + "site" + "\"" + ":" + "\"" + TrayBarcodeBinding_Site + "\"" + ",");
            str.Append("\"" + "trayNumber" + "\"" + ":" + "\"" + TrayBarcodeBinding_TrayNumber + "\"" + ",");
            str.Append("\"" + "dataList" + "\"" + ":" + "[");
            str.Append("{");
            int index = 0;
            foreach (KeyValuePair<string, string> item in TrayBarcodeBinding_dataList)
            {
                index++;
                if (index >= TrayBarcodeBinding_dataList.Count)
                {
                    str.Append("\"" + item.Key + "\"" + ":" + "\"" + item.Value + "\"");
                }
                else
                {
                    str.Append("\"" + item.Key + "\"" + ":" + "\"" + item.Value + "\"" + ",");
                }
            }
            str.Append("}");
            str.Append("]");
            str.Append("}");
            MiddleLayer.DataF.SaveMesLog(" Tray_Barcode_Binding_interface:" + str.ToString());
            return ReturnValue(Post(TrayBarcodeBinding_URL, str.ToString(), out Post_Message));
        }
        #endregion

        #region Tray盘条码解绑  Tray disk barcode unbinding
        /// <summary>
        /// Url
        /// </summary>
        public string TrayDiskBarcodeUnbinding_Url
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "TrayDiskBarcodeUnbinding_Url");
        }

        /// <summary>
        /// 工厂编码
        /// </summary>
        public string TrayDiskBarcodeUnbinding_Site
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "TrayDiskBarcodeUnbinding_Site");
        }

        /// <summary>
        /// 托盘编号
        /// </summary>
        public string TrayDiskBarcodeUnbinding_TrayNumber
        {
            get;
            set;
        }

        public bool TrayDisk_BarcodeUnbinding_interface()
        {
            StringBuilder str = new StringBuilder();

            str.Append("{");
            str.Append("\"" + "site" + "\"" + ":" + "\"" + TrayDiskBarcodeUnbinding_Site + "\"" + ",");
            str.Append("\"" + "trayNumber" + "\"" + ":" + "\"" + TrayDiskBarcodeUnbinding_TrayNumber + "\"");

            str.Append("}");

            MiddleLayer.DataF.SaveMesLog(" TrayDisk_BarcodeUnbinding_interface:" + str.ToString());
            return ReturnValue(Post(TrayDiskBarcodeUnbinding_Url, str.ToString(), out Post_Message));
        }
        #endregion

        #region Tary盘信息查询  Tary disk information query
        /// <summary>
        /// Url
        /// </summary>
        public string TaryDiskInformationQuery_Url
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "TaryDiskInformationQuery_Url");
        }

        /// <summary>
        /// 工厂编码
        /// </summary>
        public string TaryDiskInformationQuery_Site
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "TaryDiskInformationQuery_Site");
        }

        /// <summary>
        /// 托盘编号
        /// </summary>
        public string TaryDiskInformationQuery_TrayNumber
        {
            get;
            set;
        }

        public bool TaryDiskInformation_Interface()
        {
            StringBuilder str = new StringBuilder();
            str.Append("{");
            str.Append("\"" + "site" + "\"" + ":" + "\"" + TaryDiskInformationQuery_Site + "\"" + ",");
            str.Append("\"" + "trayNumber" + "\"" + ":" + "\"" + TaryDiskInformationQuery_TrayNumber + "\"");
            str.Append("}");
            MiddleLayer.DataF.SaveMesLog(" TaryDiskInformation_Interface:" + str.ToString());
            return ReturnValue(Post(TaryDiskInformationQuery_Url, str.ToString(), out Post_Message));
        }
        #endregion

        #region Tray开始接口  Tray start interface
        /// <summary>
        /// Url
        /// </summary>
        public string TrayStartInterface_Url
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "TrayStartInterface_Url");
        }

        /// <summary>
        /// 工厂编码
        /// </summary>
        public string TrayStartInterface_Site
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "TrayStartInterface_Site");
        }

        /// <summary>
        /// 托盘编号
        /// </summary>
        public string TrayStartInterface_TrayNumber
        {
            get;
            set;
        }
        /// <summary>
        /// 设备编号
        /// </summary>
        public string TrayStartInterface_Resource
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "TrayStartInterface_Resource");
        }
        public bool Tray_Start_Interface()
        {
            StringBuilder str = new StringBuilder();
            str.Append("{");
            str.Append("\"" + "site" + "\"" + ":" + "\"" + TrayStartInterface_Site + "\"" + ",");
            str.Append("\"" + "resource" + "\"" + ":" + "\"" + TrayStartInterface_Resource + "\"" + ",");
            str.Append("\"" + "trayNumber" + "\"" + ":" + "\"" + TrayStartInterface_TrayNumber + "\"");
            str.Append("}");
            MiddleLayer.DataF.SaveMesLog(" TrayStartInterface_Url:" + str.ToString());
            return ReturnValue(Post(TrayStartInterface_Url, str.ToString(), out Post_Message));
        }
        #endregion

        #region Tray盘完成  Tray disk completed interface
        /// <summary>
        /// Url
        /// </summary>
        public string TrayDiskCompleted_Url
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "TrayDiskCompleted_Url");
        }

        /// <summary>
        /// 工厂编码
        /// </summary>
        public string TrayDiskCompleted_Site
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "TrayDiskCompleted_Site");
        }

        /// <summary>
        /// 托盘编号
        /// </summary>
        public string TrayDiskCompleted_Resource
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "TrayDiskCompleted_Resource");
        }
        /// <summary>
        /// 设备编号
        /// </summary>
        public string TrayDiskCompleted_Dclist
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "TrayDiskCompleted_Dclist");
        }
        /// <summary>
        /// 工厂编码
        /// </summary>
        public string TrayDiskCompleted_TrayNumber
        {
            get;
            set;
        }

        /// <summary>
        /// 托盘编号
        /// </summary>
        public string TrayDiskCompleted_Operation
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "TrayDiskCompleted_Operation");
        }
        /// <summary>
        /// 设备编号
        /// </summary>
        public string TrayDiskCompleted_ResultList
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "TrayDiskCompleted_Dclist");
        }
        public bool Tray_Disk_Completed_Interface()
        {
            StringBuilder str = new StringBuilder();
            str.Append("{");
            str.Append("\"" + "site" + "\"" + ":" + "\"" + TrayDiskCompleted_Site + "\"" + ",");
            str.Append("\"" + "resource" + "\"" + ":" + "\"" + TrayDiskCompleted_Resource + "\"" + ",");
            str.Append("\"" + "dcList" + "\"" + ":" + "\"" + TrayDiskCompleted_Dclist + "\"");

            str.Append("\"" + "trayNumber" + "\"" + ":" + "\"" + TrayDiskCompleted_TrayNumber + "\"" + ",");
            str.Append("\"" + "operation" + "\"" + ":" + "\"" + TrayDiskCompleted_Operation + "\"" + ",");
            str.Append("\"" + "resultList" + "\"" + ":" + "\"" + TrayDiskCompleted_ResultList + "\"");
            str.Append("}");
            MiddleLayer.DataF.SaveMesLog(" Tray_Disk_Completed_Interface:" + str.ToString());
            return ReturnValue(Post(TrayDiskCompleted_Url, str.ToString(), out Post_Message));
        }
        #endregion

        #region Tray盘压合开始接口  Tray disc pressing start interface
        /// <summary>
        /// Url
        /// </summary>
        public string TrayDiscPressingStart_Url
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "TrayDiscPressingStart_Url");
        }

        /// <summary>
        /// 工厂编码
        /// </summary>
        public string TrayDiscPressingStart_Site
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "TrayDiscPressingStart_Site");
        }

        /// <summary>
        /// 托盘编号
        /// </summary>
        public string TrayDiscPressingStart_Resource
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "TrayDiscPressingStart_Resource");
        }
        /// <summary>
        /// 设备编号
        /// </summary>
        public string TrayDiscPressingStart_Operation
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "TrayDiscPressingStart_Operation");
        }
        /// <summary>
        /// 托盘条码
        /// </summary>
        public string TrayDiscPressingStart_TrayNumber
        {
            get;
            set;
        }

        public bool TrayDisc_pressingStart_Interface()
        {
            StringBuilder str = new StringBuilder();
            str.Append("{");
            str.Append("\"" + "site" + "\"" + ":" + "\"" + TrayDiscPressingStart_Site + "\"" + ",");
            str.Append("\"" + "resource" + "\"" + ":" + "\"" + TrayDiscPressingStart_Resource + "\"" + ",");
            str.Append("\"" + "trayNumber" + "\"" + ":" + "\"" + TrayDiscPressingStart_Operation + "\"" + ",");
            str.Append("\"" + "operation" + "\"" + ":" + "\"" + TrayDiscPressingStart_TrayNumber + "\"" + ",");
            str.Append("}");
            MiddleLayer.DataF.SaveMesLog(" TrayDisc_pressingStart_Interface" + ":" + str.ToString());
            return ReturnValue(Post(TrayDiscPressingStart_Url, str.ToString(), out Post_Message));
        }
        #endregion

        #region Tray盘压合完成接口  Tray plate press-fitting completes the interface
        /// <summary>
        /// Url
        /// </summary>
        public string Tray_PressCompletes_URL
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "Tray_PressCompletes_URL");
        }

        /// <summary>
        /// 工厂编码
        /// </summary>
        public string Tray_PressCompletes_Site
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "Tray_PressCompletes_Site");
        }

        /// <summary>
        /// 托盘编号
        /// </summary>
        public string Tray_PressCompletes_Resource
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "Tray_PressCompletes_Resource");
        }
        /// <summary>
        /// 设备编号
        /// </summary>
        public string Tray_PressCompletes_DcGroupName
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "Tray_PressCompletes_DcGroupName");
        }
        /// <summary>
        /// 托盘条码
        /// </summary>
        public string Tray_PressCompletes_TrayNumber
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "Tray_PressCompletes_TrayNumber");
        }
        /// <summary>
        /// 设备编号
        /// </summary>
        public string Tray_PressCompletes_OPeration
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "Tray_PressCompletes_OPeration");
        }
        /// <summary>
        /// 托盘条码
        /// </summary>
        public string Tray_PressCompletes_ResultList
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "Tray_PressCompletes_ResultList");
        }
        /// <summary>
        /// 设备编号
        /// </summary>
        public string Tray_PressCompletes_AssembleComponents
        {
            get => MiddleLayer.MesF.GetSettingValue("Mset", "Tray_PressCompletes_AssembleComponents");
        }
        /// <summary>
        /// 物料条码list
        /// </summary>
        Dictionary<string, string> Tray_PressCompletes_DcParameterlist = new Dictionary<string, string>();

        public bool Tray_PressCompletes_Interface()
        {
            StringBuilder str = new StringBuilder();
            str.Append("{");
            str.Append("\"" + "site" + "\"" + ":" + "\"" + Tray_PressCompletes_Site + "\"" + ",");
            str.Append("\"" + "resource" + "\"" + ":" + "\"" + Tray_PressCompletes_Resource + "\"" + ",");
            str.Append("\"" + "dcList" + "\"" + ":" + "[");
            str.Append("{");
            str.Append("\"" + "dcGroupName" + "\"" + ":" + "\"" + Tray_PressCompletes_DcGroupName + "\"" + ",");
            str.Append("\"" + "dcParameterList" + "\"" + ":" + "[");
            str.Append("{");
            int index = 0;
            foreach (KeyValuePair<string, string> item in Tray_PressCompletes_DcParameterlist)
            {
                index++;
                if (index >= Tray_PressCompletes_DcParameterlist.Count)
                {
                    str.Append("\"" + item.Key + "\"" + ":" + "\"" + item.Value + "\"");
                }
                else
                {
                    str.Append("\"" + item.Key + "\"" + ":" + "\"" + item.Value + "\"" + ",");
                }
            }
            str.Append("}");
            str.Append("]");
            str.Append("}");
            str.Append("],");
            str.Append("\"" + "trayNumber" + "\"" + ":" + "\"" + Tray_PressCompletes_TrayNumber + "\"" + ",");
            str.Append("\"" + "operation" + "\"" + ":" + "\"" + Tray_PressCompletes_OPeration + "\"" + ",");
            str.Append("\"" + "resultList" + "\"" + ":" + Tray_PressCompletes_ResultList + ",");
            str.Append("\"" + "assembleComponents" + "\"" + ":" + Tray_PressCompletes_AssembleComponents);
            str.Append("}");

            MiddleLayer.DataF.SaveMesLog(" Tray_PressCompletes_Interface" + ":" + str.ToString());
            return ReturnValue(Post(Tray_PressCompletes_URL, str.ToString(), out Post_Message));
        }
        #endregion

        public LENS()
        {
            InitializeComponent();
        }
        readonly Object OBJ = new object();
        /// <summary>
        /// Post上传
        /// </summary>
        /// <param name="Url">Url地址</param>
        /// <param name="jsonParas">上传的JSOn数据</param>
        private string Post(string Url, string jsonParas, out string Post_Message)
        {
            lock (OBJ)
            {
                string strURL = Url;
                //创建一个HTTP请求  
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(strURL);
                //Post请求方式  
                request.Method = "POST";
                //内容类型
                request.ContentType = "application/json";
                //设置响应时间
                request.Timeout = 30000;

                //设置参数，并进行URL编码           
                byte[] payload = Encoding.ASCII.GetBytes(jsonParas);
                //设置请求的ContentLength   
                request.ContentLength = payload.Length;
                //发送请求，获得请求流 
                Stream writer;
                try
                {
                    writer = request.GetRequestStream();//获取用于写入请求数据的Stream对象
                }
                catch (Exception ex)
                {
                    MiddleLayer.DataF.AddLogError(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": " + ex.ToString());
                    writer = null;
                    Console.Write("连接服务器失败!");
                }
                //将请求参数写入流

                writer.Write(payload, 0, payload.Length);
                writer.Close();//关闭请求流
                MiddleLayer.DataF.SaveMesLog("Send" + jsonParas);
                HttpWebResponse response;
                try
                {
                    //获得响应流
                    response = (HttpWebResponse)request.GetResponse();
                }
                catch (WebException ex)
                {
                    response = ex.Response as HttpWebResponse;
                }
                Stream s = response.GetResponseStream();
                StreamReader sRead = new StreamReader(s);
                string postContent = sRead.ReadToEnd();
                sRead.Close();
                textBox24.Text = postContent;
                MiddleLayer.DataF.SaveMesLog("Receive" + postContent);
                Post_Message = postContent;
                return postContent;//返回Json数据
            }
        }

        public string parseJsonOfTerminal(string jsonText, string JsonNode)
        {
            try
            {
                JObject jObj = JObject.Parse(jsonText);
                return jObj[JsonNode].ToString();
            }
            catch (Exception)
            {
                return "false";
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            Product_SN_Verification_SN = MiddleLayer.MesF.GetSettingValue("Mset", "textBox_Product_SN_Verification_SN");
            Product_SN_Verification();
            textBox24.Text = Post_Message;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Product_SN_Start_SN = MiddleLayer.MesF.GetSettingValue("Mset", "textBox_Product_SN_Start_SN");
            bool r = Product_SN_Start_Interface();
            textBox19.Text = MiddleLayer.MesF.Post_Message;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Integrated_Transit_Sn = MiddleLayer.MesF.GetSettingValue("Mset", "textBox_Integrated_Transit_Sn");
            Integrated_Transit_Item = "L5519DB";//left is L5519DA,Right is L5519DB
            Integrated_Transit_dcGroupName = "HL_ST_TESTGROUP1";
            dcParameterList.Add("SeqNo", "1");//序号
            dcParameterList.Add("TestName", "STTEST");//测试名称
            dcParameterList.Add("TestValue", "33");//测试值
            dcParameterList.Add("StandardValue", "35");//标准值
            dcParameterList.Add("UpperLimit", "36.2");//最大值
            dcParameterList.Add("LowerLimit", "25.32");//最小值
            dcParameterList.Add("TestResult", "true");//测试结果

            Integrated_Transit_testResult_ncCode = "TEST_PASS";//OK is TEST_PASS Fail is Q-ST
            bool r = Integrated_Transit_Interface();
            textBox10.Text = MiddleLayer.MesF.Post_Message;
        }
	}
}
