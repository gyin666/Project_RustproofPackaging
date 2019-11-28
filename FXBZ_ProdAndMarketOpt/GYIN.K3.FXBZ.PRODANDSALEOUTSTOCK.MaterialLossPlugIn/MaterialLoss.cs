using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Util;
using System;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using System.Text;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.App.Data;
using System.ComponentModel;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.Metadata;
using System.Collections.Generic;

namespace GYIN.K3.FXBZ.PRODANDSALEOUTSTOCK.MaterialLossPlugIn
{
    [Description("物耗分配单选择生产订单数据类")]
    public class MaterialLoss : AbstractBillPlugIn
    {
        public override void ButtonClick(ButtonClickEventArgs e)
        {
            //自定义打开生产订单列表按钮
            if (e.Key.EqualsIgnoreCase("F_QZNX_Button"))
            {
                ListShowParameter lstShowParam = new ListShowParameter();
                BillShowParameter billShowParam = new BillShowParameter();
                billShowParam.FormId = "PRD_MO";
                billShowParam.Status = OperationStatus.EDIT;
                //Dictionary<string, string> dic = new Dictionary<string, string>();
                //dic.Add("mo_number", "dskfjalfjasd");
                billShowParam.PKey = "100012";
               // billShowParam.CustomParams.Add("FBillNo", "11019");
                this.View.ShowForm(billShowParam);
                //billShowParam.setCustomParams = dic;
                //lstShowParam.FormId = "PRD_MO";          
                //lstShowParam.OpenStyle.ShowType = Kingdee.BOS.Core.DynamicForm.ShowType.Default;
                //this.View.ShowForm(lstShowParam);
               }




            }





        //值更新事件触发获取各分录页签的数据
        public override void DataChanged(DataChangedEventArgs e)
        {
            base.DataChanged(e);
            string a = e.Field.Key.ToUpperInvariant();
            bool flag = a == "F_QZNX_FBILLBO" && e.NewValue != null;
            if (flag)
            {
               // String fbillNo = Convert.ToString(this.View.Model.GetValue("F_QZNX_FBILLBO"));//生产通知单号
                DynamicObject billObj = this.View.Model.GetValue("F_QZNX_FBILLBO") as DynamicObject;
                string fbillNo = billObj["Id"].ToString();
                DynamicObjectCollection col1 = getLingliaoCol(fbillNo);
                 Entity entity = this.Model.BusinessInfo.GetEntity("FEntity");//材料消耗页签
                int i = 0;
                foreach (var col in col1)
                {               
                    this.Model.CreateNewEntryRow(entity, i);
                    this.Model.SetValue("F_QZNX_MaterialId", Convert.ToString(col["FMATERIALID"]), i);
                    this.Model.SetValue("F_QZNX_ActualWaste", Convert.ToString(col["FACTUALQTY"]), i);
                    i = i + 1;
                }
                base.View.UpdateView("FEntity");
                DynamicObjectCollection col2 = getLingliaoCol(fbillNo);
                Entity entity1 = this.Model.BusinessInfo.GetEntity("Finished_GoodsEntity");//成品页签
                int j = 0;
                foreach (var col in col1)
                {
                    this.Model.CreateNewEntryRow(entity1, j);
                    this.Model.SetValue("F_QZNX_PlanNum", Convert.ToString(col["PlanningQty"]), j);
                    this.Model.SetValue("F_QZNX_Qty1", Convert.ToString(col["FQUAQTY"]), j);
                    this.Model.SetValue("F_QZNX_PieceNum", Convert.ToString(col["F_SCFG_JIANNUM"]), j);
                    this.Model.SetValue("F_QZNX_Num", Convert.ToString(col["F_SCFG_GENUM"]), j);
                    this.Model.SetValue("F_QZNX_SquareMetreNum", Convert.ToString(col["F_SCFG_M2NUM"]), j);
                    this.Model.SetValue("F_QZNX_SheetsNum", Convert.ToString(col["F_SCFG_ZHANGNUM"]), j);
                    this.Model.SetValue("F_QZNX_BoxNum", Convert.ToString(col["F_SCFG_XIANGNUM"]), j);
                    j = j + 1;
                }
                base.View.UpdateView("Finished_GoodsEntity");
            }
          }
        //获取领料单数据集合
        //param：生产订单号
        public DynamicObjectCollection getLingliaoCol(string fbillNo)
        {
            string strSQL = string.Format(@"/*dialect*/SELECT tpp.FBILLNO,tppd.FMOBILLNO,tppd.FMATERIALID,tppd.FACTUALQTY from T_PRD_PICKMTRL tpp --生产领料单
LEFT JOIN T_PRD_PICKMTRLDATA tppd  --领料单明细
ON tpp.fid = tppd.fid
where tppd.FMOBILLNO='{0}'", fbillNo);
            return DBUtils.ExecuteDynamicObject(this.Context, strSQL);
        }

        //获取汇报单数据集合
        //param：生产订单号
        public DynamicObjectCollection getHuibaoCol(string fbillNo)
        {
            string strSQL = string.Format(@"/*dialect*/SELECT tso.FBILLNO,tsoe.FMONUMBER,tsoe.FMOROWNUMBER,tsoe.FOPERNUMBER,tsoe.PlanningQty,tsoea.FQUAQTY,tsoe.F_SCFG_M2NUM,tsoe.F_SCFG_ZHANGNUM,tsoe.F_SCFG_GENUM,tsoe.F_SCFG_XIANGNUM,tsoe.F_SCFG_JIANNUM from  T_SFC_OPTRPT tso --工序汇报单
LEFT JOIN T_SFC_OPTRPTENTRY tsoe --工序汇报单汇总
ON tso.fid=tsoe.fid
LEFT JOIN T_SFC_OPTRPTENTRY_A tsoea
ON tsoe.FENTRYID=tsoea.FENTRYID
where tsoe.FMONUMBER='{0}'", fbillNo);
            return DBUtils.ExecuteDynamicObject(this.Context, strSQL);
        }


    }
    }

