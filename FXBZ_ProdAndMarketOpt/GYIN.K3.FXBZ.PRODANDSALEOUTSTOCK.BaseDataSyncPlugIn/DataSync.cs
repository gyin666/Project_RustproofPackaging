using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GYIN.K3.FXBZ.PROCANDSALEOUTSTOCK.App;
using GYIN.K3.FXBZ.PROCANDSALEOUTSTOCK.ServiceHelper;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Metadata.FormElement;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Core;
using Kingdee.BOS.App.Data;

namespace GYIN.K3.FXBZ.PRODANDSALEOUTSTOCK.BaseDataSyncPlugIn
{
    [Description("基础资料供应商、客户数据保存、提交、审核、反审核、禁用、返禁用、删除时，同步操作辅助资料对应数据")]
    public class DataSync : AbstractOperationServicePlugIn
    {
        string dataType = "";//辅助资料类别
        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            if (e.DataEntitys != null && e.DataEntitys.Count<DynamicObject>() > 0)
            {
                foreach (DynamicObject item in e.DataEntitys)
                {
                    if (Convert.ToString(item.DynamicObjectType).Equals("Supplier"))
                    {
                        dataType = "01";//供应商//图省事儿，先写死。^_^
                    }
                    else {
                        dataType = "dhkh";//订货客户
                    }
                    dataSync(item);
                }
            }
        }
        //创建目标数据对象
        private IBillView CreateBillView(Kingdee.BOS.Context ctx)
        {
            FormMetadata meta = MetaDataServiceHelper.Load(ctx, "BOS_ASSISTANTDATA_DETAIL") as FormMetadata;
            Form form = meta.BusinessInfo.GetForm();
            Type type = Type.GetType("Kingdee.BOS.Web.Import.ImportBillView,Kingdee.BOS.Web");
            var billView = (IDynamicFormViewService)Activator.CreateInstance(type);
            BillOpenParameter openParam = CreateOpenParameter(ctx, meta);
            var provider = form.GetFormServiceProvider();
            billView.Initialize(openParam, provider);
            return billView as IBillView;
        }
        private BillOpenParameter CreateOpenParameter(Kingdee.BOS.Context ctx, FormMetadata meta)
        {
            Form form = meta.BusinessInfo.GetForm();
            BillOpenParameter openParam = new BillOpenParameter(form.Id, meta.GetLayoutInfo().Id);
            openParam.Context = ctx;
            openParam.ServiceName = form.FormServiceName;
            openParam.PageId = Guid.NewGuid().ToString();
            openParam.FormMetaData = meta;
            openParam.Status = OperationStatus.ADDNEW;
            openParam.PkValue = null;
            openParam.CreateFrom = CreateFrom.Default;
            openParam.GroupId = "";
            openParam.ParentId = 0;
            openParam.DefaultBillTypeId = "";
            openParam.DefaultBusinessFlowId = "";
            openParam.SetCustomParameter("ShowConfirmDialogWhenChangeOrg", false);
            List<AbstractDynamicFormPlugIn> plugs = form.CreateFormPlugIns();
            openParam.SetCustomParameter(FormConst.PlugIns, plugs);
            PreOpenFormEventArgs args = new PreOpenFormEventArgs(ctx, openParam);
            return openParam;
        }
        private void FillBillPropertys(Kingdee.BOS.Context ctx, IBillView billView, DynamicObject obj)
        {
            IDynamicFormViewService dynamicFormView = billView as IDynamicFormViewService;
            dynamicFormView.UpdateValue("FId", 0, dataType);
            dynamicFormView.UpdateValue("F_PAEZ_Number", 0, Convert.ToString(obj["Id"]));//用于关联辅助资料表记录
            dynamicFormView.UpdateValue("FNumber", 0, Convert.ToString(obj["Number"]));
            dynamicFormView.UpdateValue("FDataValue", 0, Convert.ToString(obj["Name"]));
        }
        //数据同步
        public void dataSync(DynamicObject obj)
        {
            IBillView billView = this.CreateBillView(this.Context);
            ((IBillViewService)billView).LoadData();
            DynamicFormViewPlugInProxy eventProxy = billView.GetService<DynamicFormViewPlugInProxy>();
            eventProxy.FireOnLoad();      
            this.FillBillPropertys(this.Context, billView, obj);
            IOperationResult saveResult = null;
            object[] primaryKeys = null;
            IOperationResult auditResult = null;
            if (this.FormOperation.Operation.Equals("Save"))//保存
            {
                if (isExsit(Convert.ToInt64(obj["Id"])))
                {
                    updateAssistData(Convert.ToInt64(obj["Id"]), Convert.ToString(obj["Number"]), Convert.ToString(obj["Name"]));  //存在则修改
                }
                else {//不存则新增
                      // 调用保存操作
                    OperateOption saveOption = OperateOption.Create();
                    // 调用保存操作
                    saveResult = BusinessDataServiceHelper.Save(this.Context, billView.BillBusinessInfo, billView.Model.DataObject, saveOption, "Save");
                }
            }
            else if (this.FormOperation.Operation.Equals("Submit"))//提交
            {
                updateAssistDataStatus(Convert.ToInt64(obj["Id"]), "B");
            }
            else if (this.FormOperation.Operation.Equals("Audit"))//审核
            {
                updateAssistDataStatus(Convert.ToInt64(obj["Id"]), "C");
            }
            else if (this.FormOperation.Operation.Equals("UnAudit"))//反审核
            {
                 if (getExsitOldData(Convert.ToString(obj["Number"])) != null)//判断辅助资料表是否有对应的老数据
                {
                    DynamicObject oo = getExsitOldData(Convert.ToString(obj["Number"]));
                    updateAssistDataOld(Convert.ToString(oo["fentryid"]), Convert.ToString(obj["Number"]));//如果存在则把外键编码字段关联上
                 }
                updateAssistDataStatus(Convert.ToInt64(obj["Id"]), "D");
            }
            else if (this.FormOperation.Operation.Equals("Forbid"))//禁用
            {
                updateAssistForbidStatus(Convert.ToInt64(obj["Id"]), "B");
            }
            else if (this.FormOperation.Operation.Equals("Enable"))//反禁用
            {
                updateAssistForbidStatus(Convert.ToInt64(obj["Id"]), "A");
            }
            else if (this.FormOperation.Operation.Equals("Delete"))//删除
            {
                deleteData(Convert.ToInt64(obj["Id"]));
            }         
        }
        //判断辅助资料记录是否存在
        public bool isExsit(long id) {
            bool flag = false;
            string strSQL = string.Format("/*dialect*/SELECT * from T_BAS_ASSISTANTDATAENTRY WHERE F_PAEZ_Number='{0}'", id);
            // string sql = "select * from t_BD_Supplier where fsupplierid='" + fsupplierid + "'";
            DynamicObjectCollection supplyerObjectCol = DBUtils.ExecuteDynamicObject(this.Context, strSQL);
            if (supplyerObjectCol != null && supplyerObjectCol.Count > 0)
            {
                flag = true;
            }
            return flag;
        }

        //获取辅助资料表对应的老数据对象
        public DynamicObject getExsitOldData(string fnumber)
        {
            DynamicObject obj = null;
            string strSQL = string.Format(@"/*dialect*/SELECT tbae.FID,tbae.fentryid,tbae.fnumber,tbael.FDATAVALUE,tbae.F_PAEZ_Number,tbae.FDOCUMENTSTATUS,tbae.FFORBIDSTATUS
from T_BAS_ASSISTANTDATAENTRY tbae   --需要同步
LEFT JOIN
T_BAS_ASSISTANTDATAENTRY_L tbael     --需要同步
ON tbae.fentryid=tbael.fentryid
LEFT JOIN T_BAS_ASSISTANTDATA tba
ON tba.fid=tbae.FID
LEFT JOIN T_BAS_ASSISTANTDATA_L tbal
ON tba.fid=tbal.fid
WHERE tbal.fname='供应商'
and tbae.fnumber='{0}'", fnumber);
            DynamicObjectCollection supplyerObjectCol = DBUtils.ExecuteDynamicObject(this.Context, strSQL);
            if (supplyerObjectCol != null && supplyerObjectCol.Count > 0)
            {
                obj = supplyerObjectCol[0];
            }
            return obj;
        }
        //修改辅助资料数据
        public void updateAssistData(long id, string fnumber, string fname)
        {    
            string strSQL = string.Format("/*dialect*/UPDATE T_BAS_ASSISTANTDATAENTRY SET fnumber = '{0}' WHERE F_PAEZ_Number='{1}'", fnumber, id);
            DBUtils.Execute(this.Context, strSQL);
            string strSQL1 = string.Format("/*dialect*/UPDATE T_BAS_ASSISTANTDATAENTRY_L SET fdatavalue = '{0}' WHERE fentryid = (SELECT fentryid from T_BAS_ASSISTANTDATAENTRY WHERE F_PAEZ_Number='{1}')", fname, id);
            DBUtils.Execute(this.Context, strSQL1);
        }


        //修改辅助资料历史数据，存入外键编码字段
        public void updateAssistDataOld(string id, string fnumber)
        {
            string entryId = "";
            string strSQL = string.Format(@"/*dialect*/SELECT tbs.fmasterid,tbs.FNUMBER,tbs.FSUPPLIERID,tbs.FUSEORGID,tbs.FCREATEORGID,tbsl.fname FROM T_BD_Supplier tbs
LEFT JOIN T_BD_SUPPLIER_L tbsl
ON tbs.fsupplierid=tbsl.fsupplierid
WHERE tbs.FNUMBER='{0}'", fnumber);
            DynamicObjectCollection supplyerObjectCol = DBUtils.ExecuteDynamicObject(this.Context, strSQL);
            if (supplyerObjectCol != null && supplyerObjectCol.Count > 0)
            {
                entryId = Convert.ToString(supplyerObjectCol[0]["fmasterid"]);
            }
            string strSQL1 = string.Format("/*dialect*/UPDATE T_BAS_ASSISTANTDATAENTRY SET F_PAEZ_Number = '{0}' WHERE fentryid='{1}'", entryId, id);
            DBUtils.Execute(this.Context, strSQL1);
        }







        //修改辅助资料数据状态
        public void updateAssistDataStatus(long id, string status)
        {
            string strSQL = string.Format("/*dialect*/UPDATE T_BAS_ASSISTANTDATAENTRY SET FDOCUMENTSTATUS = '{0}' WHERE F_PAEZ_Number='{1}'", status, id);
            DBUtils.Execute(this.Context, strSQL);         
        }
        //修改辅助资料禁用状态
        public void updateAssistForbidStatus(long id, string status)
        {
            string strSQL = string.Format("/*dialect*/UPDATE T_BAS_ASSISTANTDATAENTRY SET FFORBIDSTATUS = '{0}' WHERE F_PAEZ_Number='{1}'", status, id);
            DBUtils.Execute(this.Context, strSQL);
        }
        //删除数据
        public void deleteData(long id) {
            string strSQL = string.Format("/*dialect*/delete from T_BAS_ASSISTANTDATAENTRY  WHERE F_PAEZ_Number='{0}'", id);
            DBUtils.Execute(this.Context, strSQL);
        }
    }
}
