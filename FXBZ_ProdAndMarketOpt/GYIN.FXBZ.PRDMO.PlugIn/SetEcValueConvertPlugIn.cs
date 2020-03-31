using Kingdee.BOS;
using Kingdee.BOS.App;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using System;
using System.ComponentModel;
using System.Data;
using System.Linq;

namespace GYIN.FXBZ.PRDMO.PlugIn
{
    [Description("销售订单下推生产订单携带延长米系数并计算延长米")]
     public class SetEcValueConvertPlugIn : AbstractConvertPlugIn
    {
        public override void AfterConvert(AfterConvertEventArgs e)
        {
            base.AfterConvert(e);
            ServiceHelper.GetService<IMetaDataService>();
            ExtendedDataEntity[] array = e.Result.FindByEntityKey("FBillHead");
            for (int i = 0; i < array.Length; i++)
            {
                DynamicObjectCollection dynamicObjectCollection = array[i].DataEntity["TreeEntity"] as DynamicObjectCollection;
                for (int p = 0; p < dynamicObjectCollection.Count(); p++)
                {
                    string FMaterialId = Convert.ToString(dynamicObjectCollection[p]["MaterialId_Id"]);//物料编码
                    DynamicObject bomObj = dynamicObjectCollection[p]["BomId"] as DynamicObject;
                    string FNumber = Convert.ToString(bomObj["Number"]);//BOM版本
                    if (!String.IsNullOrEmpty(FMaterialId.Trim())&&!String.IsNullOrEmpty(FNumber.Trim()))
                    {
                        DynamicObject obj = getdataObj(FMaterialId, FNumber);
                        if (obj != null)
                        {
                            double EC = Convert.ToDouble(obj["F_SCFG_EC"]);//延长米系数
                            dynamicObjectCollection[p]["F_scfg_EC"] = EC;
                            double num = Convert.ToDouble(dynamicObjectCollection[0]["Qty"]);//数量
                            double ycm = EC * num;
                            dynamicObjectCollection[p]["F_scfg_Qty1"] = ycm;//延长米
                            array[i]["F_scfg_MaterialId"] = FMaterialId;//物料编码
                            if (Convert.ToDouble(obj["F_SCFG_DIANYUN"])!=0.00)
                            {
                                array[i]["F_scfg_Dianyun"] = getWorkHourData(ycm, Convert.ToDouble(obj["F_SCFG_DIANYUN"]));//电晕工时
                            }
                            if (Convert.ToDouble(obj["F_SCFG_YINSHUASHANGBAN"]) != 0.00)
                            {
                                array[i]["F_scfg_Yinshuashangban"] = getWorkHourData(ycm, Convert.ToDouble(obj["F_SCFG_YINSHUASHANGBAN"]));//印刷上版(min)
                            }
                            if (Convert.ToDouble(obj["F_SCFG_YINSHUA"]) != 0.00)
                            {
                                array[i]["F_scfg_Yinshua"] = getWorkHourData(ycm, Convert.ToDouble(obj["F_SCFG_YINSHUA"]));//印刷(m/min)
                            }
                            if (Convert.ToDouble(obj["F_SCFG_TUBU"]) != 0.00)
                            {
                                array[i]["F_scfg_Tubu"] = getWorkHourData(ycm, Convert.ToDouble(obj["F_SCFG_TUBU"]));//涂布(m/min)
                            }
                            if (Convert.ToDouble(obj["F_SCFG_FUHE1"]) != 0.00)
                            {
                                array[i]["F_scfg_Fuhe1"] = getWorkHourData(ycm, Convert.ToDouble(obj["F_SCFG_FUHE1"]));//复合一(m/min)
                            }
                            if (Convert.ToDouble(obj["F_SCFG_FUHE2"]) != 0.00)
                            {
                                array[i]["F_scfg_Fuhe2"] = getWorkHourData(ycm, Convert.ToDouble(obj["F_SCFG_FUHE2"]));//复合二(m/min)
                            }
                            if (Convert.ToDouble(obj["F_SCFG_FUHE3"]) != 0.00)
                            {
                                array[i]["F_scfg_Fuhe3"] = getWorkHourData(ycm, Convert.ToDouble(obj["F_SCFG_FUHE3"]));//复合三(m/min)
                            }
                            if (Convert.ToDouble(obj["F_SCFG_FENQIE"]) != 0.00)
                            {
                                array[i]["F_scfg_Fenqie"] = getWorkHourData(ycm, Convert.ToDouble(obj["F_SCFG_FENQIE"]));//分切(m/min)
                            }
                            if (Convert.ToDouble(obj["F_SCFG_FENQIEZL"]) != 0.00)
                            {
                                array[i]["F_scfg_Fenqiezl"] = getWorkHourFQData(num, Convert.ToDouble(obj["F_SCFG_FENQIEZL"]));//分切重量(kg/h)
                            }
                        }
                    }
                }
            }
        }
        //获得上游销售订单明细物料延长米系数
        public DynamicObject getdataObj(string FMaterialId, string FNumber)
        {
            DynamicObject obj = null;
            string strSQL = string.Format(@"/*dialect*/SELECT F_SCFG_EC,F_SCFG_DIANYUN,F_SCFG_YINSHUASHANGBAN,F_SCFG_YINSHUA,F_SCFG_TUBU,F_SCFG_FUHE1,F_SCFG_FUHE2,F_SCFG_FUHE3,F_SCFG_FENQIE,F_SCFG_FENQIEZL FROM T_ENG_BOM WHERE FMATERIALID = '{0}' and  FNUMBER = '{1}'", FMaterialId, FNumber);
            DynamicObjectCollection dataCol = DBUtils.ExecuteDynamicObject(this.Context, strSQL);
            if (dataCol != null && dataCol.Count > 0)
            {
                obj = dataCol[0];
            }
            return obj;
        }
        //计算工时
        public double getWorkHourData(double ycm, double whType) {
            return ycm / whType /60;
        }
        //分切重量计算工时
        public double getWorkHourFQData(double num, double whType)
        {
            return num / whType;
        }
    }
}
