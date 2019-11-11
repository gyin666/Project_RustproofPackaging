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
using System.Collections.Generic;
using Kingdee.BOS.Util;

namespace VNRX.FXBZ.BarCodeSplitBill.OperationPlugIn
{
    [Description("条码拆装单保存时换算并赋值")]
    public class SavePlugIn : AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("FEntity");
            e.FieldKeys.Add("FEntryBarCode");
            e.FieldKeys.Add("FEntity");
            e.FieldKeys.Add("FEntity");
            e.FieldKeys.Add("FEntity");
            e.FieldKeys.Add("FEntity");
        }

        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            // 条码拆装单保存时进行多维度单位换算
            if (this.FormOperation.Operation.Equals("Save"))
            {
                if (e.DataEntitys != null && e.DataEntitys.Count<DynamicObject>() > 0)
                {
                    foreach (DynamicObject item in e.DataEntitys)
                    {
                        // 获取条码拆装箱明细
                        DynamicObjectCollection barCodeEntry = item["UN_PackagingEntry"] as DynamicObjectCollection;

                        if (barCodeEntry != null && barCodeEntry.Count > 0)
                        {
                            double totalCount = barCodeEntry.Count;

                            // 遍历当前条码拆装单明细行
                            foreach (DynamicObject obj1 in barCodeEntry)
                            {
                                // 获取当前明细行的内码
                                long entryId = Convert.ToInt64(obj1["Id"]);

                                StringBuilder tmpSQL4 = new StringBuilder();
                                tmpSQL4.AppendFormat(@"/*dialect*/ UPDATE t_UN_PackagingEntry SET F_QSNC_TUONUM = {0} WHERE FENTRYID = {1} ", (1 / totalCount), entryId);
                                DBUtils.Execute(this.Context, tmpSQL4.ToString());

                                // 获取当前明细行物料的条形码，并根据条形码查询条码主档获取该物料的公斤数量
                                String barCode = Convert.ToString(obj1["FEntryBarCode"]);
                                StringBuilder tmpSQl1 = new StringBuilder();
                                tmpSQl1.AppendFormat(@"/*dialect*/ SELECT * FROM T_BD_BARCODEMAIN WHERE FBARCODE = '{0}' ", barCode);
                                DynamicObjectCollection col1 = DBUtils.ExecuteDynamicObject(this.Context, tmpSQl1.ToString());

                                if (col1 != null && col1.Count > 0)
                                {
                                    // 获取公斤数量
                                    double realWeight = Convert.ToDouble(col1[0]["FQTY"]);

                                    // 获得物料编码
                                    String materialId = Convert.ToString(col1[0]["FMATERIALID"]);

                                    StringBuilder tmpSQL2 = new StringBuilder();
                                    tmpSQL2.AppendFormat(@"/*dialect*/ SELECT * FROM T_BD_UNITCONVERTRATE UC LEFT JOIN T_BD_UNIT_L UL ON UL.FUNITID = UC.FCURRENTUNITID WHERE FMATERIALID = '{0}' ", materialId);
                                    DynamicObjectCollection col2 = DBUtils.ExecuteDynamicObject(this.Context, tmpSQL2.ToString());
                                    if (col2 != null && col2.Count > 0)
                                    {
                                        // 遍历当前物料的标准称重单位（公斤）与其他称重单位的转换参数
                                        foreach (DynamicObject obj2 in col2)
                                        {
                                            // 目标称重单位数量
                                            double rate1 = Convert.ToDouble(obj2["FCONVERTDENOMINATOR"]);
                                            // 公斤称重单位数量
                                            double rate2 = Convert.ToDouble(obj2["FCONVERTNUMERATOR"]);

                                            // 计算公斤数量转换为各个称重单位的数值
                                            double realOtherWeight = (realWeight / rate1) * rate2;

                                            StringBuilder tmpSQL3 = new StringBuilder();
                                            String where = "";
                                            switch (Convert.ToString(obj2["FNAME"]))
                                            {
                                                case "平方米":
                                                    where = "F_QSNC_M2NUM";
                                                    break;
                                                case "张":
                                                    where = "F_QSNC_ZHANGNUM";
                                                    break;
                                                case "个":
                                                    where = "F_QSNC_GENUM";
                                                    break;
                                                case "箱":
                                                    where = "F_QSNC_XIANGNUM";
                                                    break;
                                                case "件":
                                                    where = "F_QSNC_JIANNUM";
                                                    break;
                                                default:
                                                    break;
                                            }

                                            if (!String.IsNullOrWhiteSpace(where))
                                            {
                                                tmpSQL3.AppendFormat(@"/*dialect*/ UPDATE t_UN_PackagingEntry SET {0} = {1} WHERE FENTRYID = {2} ", where, realOtherWeight, entryId);
                                                DBUtils.Execute(this.Context, tmpSQL3.ToString());
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
    }
}
