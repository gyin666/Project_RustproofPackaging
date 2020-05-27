using Kingdee.BOS;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.Operation;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Interaction;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.Core.Metadata.ConvertElement;
using Kingdee.BOS.Core.Metadata.ConvertElement.ServiceArgs;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.App;
using Kingdee.BOS.App.Data;
using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Kingdee.BOS.Core.Bill.PlugIn;

namespace GYIN.FXBZ.PPBOM.PlugIn
{
    [Description("生产用料清单保存操作结束后，从生产订单获得工时计算及定额数据并赋值")]
    public class getValueFromPrdMo : AbstractBillPlugIn
    {
        public override void BeforeSave(Kingdee.BOS.Core.Bill.PlugIn.Args.BeforeSaveEventArgs e)
        {
            base.BeforeSave(e);
            String FMaterialID = Convert.ToString(this.View.Model.DataObject["MaterialID"]);//产品编码
            String FNumber = Convert.ToString(this.View.Model.DataObject["FBOMID"]);//BOM版本 
            String MoBillNO = Convert.ToString(this.View.Model.DataObject["MOBillNO"]);//生產订单编号
            if (!String.IsNullOrEmpty(FMaterialID.Trim()) && !String.IsNullOrEmpty(FNumber.Trim()))
            {
                DynamicObject obj = getBomDataObj(FMaterialID, FNumber);
                if (obj != null)
                {
                    //紙品定額 
                    this.View.Model.SetValue("F_scfg_Dianyun", obj["F_SCFG_DIANYUN"]);
                    this.View.Model.SetValue("F_scfg_Yinshuashangban", obj["F_SCFG_YINSHUASHANGBAN"]);
                    this.View.Model.SetValue("F_scfg_Yinshua", obj["F_SCFG_YINSHUA"]);
                    this.View.Model.SetValue("F_scfg_Tubu", obj["F_SCFG_TUBU"]);
                    this.View.Model.SetValue("F_scfg_Fuhe1", obj["F_SCFG_FUHE1"]);
                    this.View.Model.SetValue("F_scfg_Fuhe1", obj["F_SCFG_FUHE1"]);
                    this.View.Model.SetValue("F_scfg_Fuhe2", obj["F_SCFG_FUHE2"]);
                    this.View.Model.SetValue("F_scfg_Fuhe3", obj["F_SCFG_FUHE3"]);
                    this.View.Model.SetValue("F_scfg_Fenqie", obj["F_SCFG_FENQIE"]);
                    this.View.Model.SetValue("F_scfg_Fenqiezl", obj["F_SCFG_FENQIEZL"]);
                    this.View.Model.SetValue("F_scfg_Duigui", obj["F_SCFG_DUIGUI"]);
                    this.View.Model.SetValue("F_scfg_Duancai", obj["F_SCFG_DUANCAI"]);
                    this.View.Model.SetValue("F_scfg_Zhuangdai", obj["F_SCFG_ZHUANGDAI"]);
                    this.View.Model.SetValue("F_scfg_Kuntuofang", obj["F_SCFG_KUNTUOFANG"]);
                    this.View.Model.SetValue("F_scfg_ttf", obj["F_SCFG_TTF"]);
                    this.View.Model.SetValue("F_scfg_Qiezhi", obj["F_SCFG_QIEZHI"]);
                    this.View.Model.SetValue("F_scfg_Tiaozhi", obj["F_SCFG_TIAOZHI"]);
                    this.View.Model.SetValue("F_scfg_Zhuangxiang", obj["F_SCFG_ZHUANGXIANG"]);
                    this.View.Model.SetValue("F_scfg_Juanshi", obj["F_SCFG_JUANSHI"]);
                    this.View.Model.SetValue("F_scfg_Xiezi", obj["F_SCFG_XIEZI"]);
                    this.View.Model.SetValue("F_scfg_tuozhijuan", obj["F_SCFG_TUOZHIJUAN"]);
                    this.View.Model.SetValue("F_scfg_Lifangjuan", obj["F_SCFG_LIFANGJUAN"]);
                    this.View.Model.SetValue("F_scfg_Tubuliang", obj["F_SCFG_TUBULIANG"]);
                    this.View.Model.SetValue("F_scfg_feheyibian", obj["F_SCFG_FEHEYIBIAN"]);
                    this.View.Model.SetValue("F_scfg_feheerbian", obj["F_SCFG_FEHEERBIAN"]);
                    this.View.Model.SetValue("F_scfg_fehesanbian", obj["F_SCFG_FEHESANBIAN"]);
                    //膜品定額 
                    this.View.Model.SetValue("F_scfg_Chuimogai", obj["F_scfg_Chuimogai"]);
                    this.View.Model.SetValue("F_scfg_Chuimowei", obj["F_scfg_Chuimowei"]);
                    this.View.Model.SetValue("F_scfg_Sgdcg", obj["F_scfg_Sgdcg"]);
                    this.View.Model.SetValue("F_scfg_Sgdcw", obj["F_scfg_Sgdcw"]);
                    this.View.Model.SetValue("F_scfg_Tanbugai", obj["F_scfg_Tanbugai"]);
                    this.View.Model.SetValue("F_scfg_Tanbuwei", obj["F_scfg_Tanbuwei"]);
                    this.View.Model.SetValue("F_scfg_Yaomogai", obj["F_scfg_Yaomogai"]);
                    this.View.Model.SetValue("F_scfg_Yaomowei", obj["F_scfg_Yaomowei"]);
                    this.View.Model.SetValue("F_scfg_Rehe", obj["F_scfg_Rehe"]);
                    this.View.Model.SetValue("F_scfg_Zhidai", obj["F_scfg_Zhidai"]);
                    this.View.Model.SetValue("F_scfg_Jieli", obj["F_scfg_Jieli"]);
                    this.View.Model.SetValue("F_scfg_Qiechanrao", obj["F_scfg_Qiechanrao"]);
                    this.View.Model.SetValue("F_scfg_Chejiao", obj["F_scfg_Chejiao"]);
                    this.View.Model.SetValue("F_scfg_Yinshuami", obj["F_scfg_Yinshuami"]);
                    this.View.Model.SetValue("F_scfg_Diepian", obj["F_scfg_Diepian"]);
                    this.View.Model.SetValue("F_scfg_Zxdd", obj["F_scfg_Zxdd"]);
                    this.View.Model.SetValue("F_scfg_Reshousuo", obj["F_scfg_Reshousuo"]);
                    this.View.Model.SetValue("F_scfg_Ztbz", obj["F_scfg_Ztbz"]);
                    this.View.Model.SetValue("F_scfg_Mlzt", obj["F_scfg_Mlzt"]);
                    this.View.Model.SetValue("F_scfg_ttf1", obj["F_scfg_ttf1"]);
                    this.View.Model.SetValue("F_scfg_BaoA", obj["F_scfg_BaoA"]);
                    this.View.Model.SetValue("F_scfg_Zd", obj["F_scfg_Zd"]);
                }
                //生产用料清单子项明细
                DynamicObjectCollection col1 = this.View.Model.DataObject["PPBomEntry"] as DynamicObjectCollection;
                // 遍历物料明细行
                for (int i = 0; i < col1.Count; i++)
                {
                    // 获取当前行的物料编码信息
                    DynamicObject materialObj = col1[i]["MaterialID"] as DynamicObject;
                    if (materialObj != null)
                    {
                        // 获取当前物料的内码
                        string materialId = Convert.ToString(materialObj["Id"]);
                        DynamicObject obj1 = getWorkHourDataObj(MoBillNO, materialId);
                        this.View.Model.SetValue("F_scfg_Dianyun", obj1["F_SCFG_DIANYUN11"],i);
                        this.View.Model.SetValue("F_scfg_Yinshuashangban", obj["F_SCFG_YINSHUASHANGBAN"], i);
                        this.View.Model.SetValue("F_scfg_Yinshua", obj["F_SCFG_YINSHUA11"], i);
                        this.View.Model.SetValue("F_scfg_Tubu", obj["F_SCFG_TUBU11"], i);
                        this.View.Model.SetValue("F_scfg_Fuhe1", obj["F_SCFG_FUHE111"], i);
                        this.View.Model.SetValue("F_scfg_Fuhe1", obj["F_SCFG_FUHE111"], i);
                        this.View.Model.SetValue("F_scfg_Fuhe2", obj["F_SCFG_FUHE211"], i);
                        this.View.Model.SetValue("F_scfg_Fuhe3", obj["F_SCFG_FUHE311"], i);
                        this.View.Model.SetValue("F_scfg_Fenqie", obj["F_SCFG_FENQIE11"], i);
                        this.View.Model.SetValue("F_scfg_Fenqiezl", obj["F_SCFG_FENQIEZL11"], i);
                    }
                }
            }
        }



        //获得纸品定额和膜品定额数据
        public DynamicObject getBomDataObj(string FMaterialId, string FNumber)
        {
            DynamicObject obj = null;
            string strSQL = string.Format(@"/*dialect*/SELECT * FROM T_ENG_BOM WHERE FMATERIALID = '{0}' and  FNUMBER = '{1}'", FMaterialId, FNumber);
            DynamicObjectCollection dataCol = DBUtils.ExecuteDynamicObject(this.Context, strSQL);
            if (dataCol != null && dataCol.Count > 0)
            {
                obj = dataCol[0];
            }
            return obj;
        }

        //获得工时计算数据
        public DynamicObject getWorkHourDataObj(string MoBillNO, string FMaterialId)
        {
            DynamicObject obj = null;
            string strSQL = string.Format(@"/*dialect*/SELECT trm.F_SCFG_DIANYUN11,F_SCFG_YINSHUASHANGBAN11,F_SCFG_YINSHUA11,F_SCFG_TUBU11,F_SCFG_FUHE111,F_SCFG_FUHE211,F_SCFG_FUHE311,F_SCFG_FENQIE11,F_SCFG_FENQIEZL11 FROM T_PRD_MO trm
INNER JOIN T_PRD_MOENTRY trme
on trm.FID = trme.FID
where trm.FBILLNO='{0}'
and trm.F_SCFG_MATERIALID11='{1}'", MoBillNO, FMaterialId);
            DynamicObjectCollection dataCol = DBUtils.ExecuteDynamicObject(this.Context, strSQL);
            if (dataCol != null && dataCol.Count > 0)
            {
                obj = dataCol[0];
            }
            return obj;
        }
    }
}
