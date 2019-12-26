using Kingdee.BOS;
using Kingdee.BOS.App;
using Kingdee.BOS.App.Core;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.Operation;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.Interaction;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.ConvertElement;
using Kingdee.BOS.Core.Metadata.ConvertElement.ServiceArgs;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Util;
using System.Collections.Generic;
using Kingdee.BOS.Core.Metadata.FormElement;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.Validation;
using System.Data.SqlClient;
using Kingdee.BOS.Core.Metadata.FieldElement;


namespace VNRX.FXBZ.ProInStock.PlugIn
{
    [Description("生产入库单审核后进行物料动态单位换算维护")]
    public class CreateConvertBill : AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("FEntity");
            e.FieldKeys.Add("FReqBillNo");
            e.FieldKeys.Add("FMaterialId");
            e.FieldKeys.Add("FBaseRealQty");
            e.FieldKeys.Add("F_scfg_Decimal4");
            e.FieldKeys.Add("F_scfg_Decimal");
            e.FieldKeys.Add("F_scfg_Decimal6");
        }

        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            if (this.FormOperation.Operation.Equals("Audit"))
            {
                if (e.DataEntitys != null && e.DataEntitys.Count<DynamicObject>() > 0)
                {
                    foreach (DynamicObject item in e.DataEntitys)
                    {
                        // 获生产入库明细行
                        DynamicObjectCollection inStockEntry = item["Entity"] as DynamicObjectCollection;
                        
                        if (inStockEntry != null && inStockEntry.Count() > 0)
                        {
                            // 遍历明细行中的每一个物料
                            foreach (DynamicObject obj in inStockEntry)
                            {
                                // 获取该张生产入库上游的销售订单
                                String orderBillNo = Convert.ToString(obj["ReqBillNo"]);
                                
                                if (!String.IsNullOrWhiteSpace(orderBillNo))
                                {
                                    // 物料编码
                                    long materialId = Convert.ToInt64(((DynamicObject)obj["MaterialId"])["Id"]);
                                    // 公斤数量
                                    double realInWeight = Convert.ToDouble(obj["BaseRealQty"]);

                                    // 平米数量
                                    double m2Weight = Convert.ToDouble(obj["F_scfg_Decimal4"]);
                                    // 张数量
                                    double zhangWeight = Convert.ToDouble(obj["F_scfg_Decimal6"]);
                                    // 个数量
                                    double geWeight = Convert.ToDouble(obj["F_scfg_Decimal"]);

                                    if (realInWeight != 0)
                                    {
                                        if (m2Weight != 0)
                                        {
                                            double m2Rate = m2Weight / realInWeight;
                                            if (convertIsExist(orderBillNo, materialId, "平方米"))
                                            {
                                                // 存在则更新
                                                updateConvertRate(orderBillNo, materialId, "平方米", m2Rate);
                                            }
                                            else
                                            {
                                                // 不存在，则建立
                                                createNewConvertBill(orderBillNo, materialId, "平方米", m2Rate);
                                            }
                                        }

                                        if (zhangWeight != 0)
                                        {
                                            double zhangRate = zhangWeight / realInWeight;
                                            if (convertIsExist(orderBillNo, materialId, "张"))
                                            {
                                                // 存在则更新
                                                updateConvertRate(orderBillNo, materialId, "张", zhangRate);
                                            }
                                            else
                                            {
                                                // 不存在，则建立
                                                createNewConvertBill(orderBillNo, materialId, "张", zhangRate);
                                            }
                                        }

                                        if (geWeight != 0)
                                        {
                                            double geRate = geWeight / realInWeight;
                                            if (convertIsExist(orderBillNo, materialId, "个"))
                                            {
                                                // 存在则更新
                                                updateConvertRate(orderBillNo, materialId, "个", geRate);
                                            }
                                            else
                                            {
                                                // 不存在，则建立
                                                createNewConvertBill(orderBillNo, materialId, "个", geRate);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        // 判断当前目标单位转换是否已经存在
        private bool convertIsExist(String orderBillNo, long materialId, String unit)
        {
            bool flag = false;

            StringBuilder tmpSQL1 = new StringBuilder();
            if (unit.Equals("平方米"))
            {
                tmpSQL1.AppendFormat(@"/*dialect*/ SELECT * FROM T_scfg_MaterialConvert MC WHERE MC.FORDERNO = '{0}' AND MC.FMATERIALNUMBER = '{1}' AND MC.FUNITID IN (SELECT TOP 1 FUNITID FROM T_BD_UNIT_L WHERE FNAME = '平方米' AND FUNITID = 108176) ", orderBillNo, materialId);
            }
            else
            {
                tmpSQL1.AppendFormat(@"/*dialect*/ SELECT * FROM T_scfg_MaterialConvert MC WHERE MC.FORDERNO = '{0}' AND MC.FMATERIALNUMBER = '{1}' AND MC.FUNITID IN (SELECT TOP 1 FUNITID FROM T_BD_UNIT_L WHERE FNAME = '{2}') ", orderBillNo, materialId, unit);
            }
            DynamicObjectCollection col1 = DBUtils.ExecuteDynamicObject(this.Context, tmpSQL1.ToString());

            if (col1 != null && col1.Count > 0)
            {
                flag = true;
            }

            return flag;
        }

        // 换算关系更新方法
        private void updateConvertRate(String orderBillNo,  long materialId, String unit, double rate)
        {
            StringBuilder tmpSQL2 = new StringBuilder();
            if (unit.Equals("平方米"))
            {
                tmpSQL2.AppendFormat(@"/*dialect*/ UPDATE T_scfg_MaterialConvert SET FQTY = {0} WHERE FORDERNO = '{1}' AND FMATERIALNUMBER = '{2}' AND FUNITID IN (SELECT TOP 1 FUNITID FROM T_BD_UNIT_L WHERE FNAME = '平方米' AND FUNITID = 108176) ", rate, orderBillNo, materialId);
            }
            else
            {
                tmpSQL2.AppendFormat(@"/*dialect*/ UPDATE T_scfg_MaterialConvert SET FQTY = {0} WHERE FORDERNO = '{1}' AND FMATERIALNUMBER = '{2}' AND FUNITID IN (SELECT TOP 1 FUNITID FROM T_BD_UNIT_L WHERE FNAME = '{3}') ", rate, orderBillNo, materialId, unit);
            }

            DBUtils.Execute(this.Context, tmpSQL2.ToString());
        }


        // 物料动态换算新增单据
        private void createNewConvertBill(String orderBillNo, long materialId, String unit, double rate)
        {
            IBillView billView = this.CreateBillView();
            ((IBillViewService)billView).LoadData();
            DynamicFormViewPlugInProxy eventProxy = billView.GetService<DynamicFormViewPlugInProxy>();
            eventProxy.FireOnLoad();
            this.FillBillPropertys(billView, orderBillNo, materialId, unit, rate);
            OperateOption saveOption = OperateOption.Create();
            this.SaveCheckBill(billView, saveOption);
        }

        private void FillBillPropertys(IBillView billView, String orderBillNo, long materialId, String unit, double rate)
        {
            IDynamicFormViewService dynamicFormView = billView as IDynamicFormViewService;

            // 物料编码
            dynamicFormView.SetItemValueByID("FMaterialNumber", materialId, 0);
            // 销售订单号
            dynamicFormView.UpdateValue("FOrderNo", 0, orderBillNo);
            // 目标计量单位
            StringBuilder tmpSQL3 = new StringBuilder();
            if (unit.Equals("平方米"))
            {
                tmpSQL3.AppendFormat(@"/*dialect*/ SELECT TOP 1 FUNITID FROM T_BD_UNIT_L WHERE FNAME = '平方米' AND FUNITID = 108176 ");
            }
            if (unit.Equals("张"))
            {
                tmpSQL3.AppendFormat(@"/*dialect*/ SELECT TOP 1 FUNITID FROM T_BD_UNIT_L WHERE FNAME = '张' ");
            }
            if (unit.Equals("个"))
            {
                tmpSQL3.AppendFormat(@"/*dialect*/ SELECT TOP 1 FUNITID FROM T_BD_UNIT_L WHERE FNAME = '个' ");
            }
            DynamicObjectCollection col3 = DBUtils.ExecuteDynamicObject(this.Context, tmpSQL3.ToString());
            if (col3 != null && col3.Count > 0)
            {
                dynamicFormView.SetItemValueByID("FUnitID", Convert.ToInt64(col3[0]["FUNITID"]), 0);
            }

            // 目标单位数量
            dynamicFormView.UpdateValue("FQty", 0, rate);
            
        }

        private void SaveCheckBill(IBillView billView, OperateOption saveOption)
        {
            // 设置FormId
            Form form = billView.BillBusinessInfo.GetForm();
            if (form.FormIdDynamicProperty != null)
            {
                form.FormIdDynamicProperty.SetValue(billView.Model.DataObject, form.Id);
            }

            // 调用保存操作
            IOperationResult saveResult = BusinessDataServiceHelper.Save(this.Context, billView.BillBusinessInfo, billView.Model.DataObject, saveOption, "Save");
            if (saveResult.IsSuccess)
            {
                object[] primaryKeys = saveResult.SuccessDataEnity.Select(u => u.GetPrimaryKeyValue()).ToArray();

                // 提交
                OperateOption submitOption = OperateOption.Create();
                IOperationResult submitResult = BusinessDataServiceHelper.Submit(this.Context, billView.BillBusinessInfo, primaryKeys, "Submit", submitOption);
                if (submitResult.IsSuccess)
                {
                    // 审核
                    OperateOption auditOption = OperateOption.Create();
                    IOperationResult auditResult = BusinessDataServiceHelper.Audit(this.Context, billView.BillBusinessInfo, primaryKeys, auditOption);
                    
                }
            }

        }
        
        private IBillView CreateBillView()
        {
            FormMetadata meta = MetaDataServiceHelper.Load(this.Context, "scfg_MaterialConvertRate") as FormMetadata;
            Form form = meta.BusinessInfo.GetForm();
            Type type = Type.GetType("Kingdee.BOS.Web.Import.ImportBillView,Kingdee.BOS.Web");
            var billView = (IDynamicFormViewService)Activator.CreateInstance(type);
            BillOpenParameter openParam = CreateOpenParameter(meta);
            var provider = form.GetFormServiceProvider();
            billView.Initialize(openParam, provider);
            return billView as IBillView;
        }

        private BillOpenParameter CreateOpenParameter(FormMetadata meta)
        {
            Form form = meta.BusinessInfo.GetForm();
            BillOpenParameter openParam = new BillOpenParameter(form.Id, meta.GetLayoutInfo().Id);
            openParam.Context = this.Context;
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
            PreOpenFormEventArgs args = new PreOpenFormEventArgs(this.Context, openParam);

            return openParam;
        }
    }
}