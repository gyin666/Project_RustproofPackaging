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

namespace VNRX.FXBZ.SaleOutStockBill.OperationPlugIn
{
    [Description("销售出库单审核时反写上游条码拆装单中实出数量")]
    public class AuditPlugIn : AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            // 包装单号
            e.FieldKeys.Add("F_scfg_PackageCode");

            // 单据体对象
            e.FieldKeys.Add("FEntity");

            // 物料编码
            e.FieldKeys.Add("FMaterialID");
        }

        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            // 条码拆装单保存时进行多维度单位换算
            if (this.FormOperation.Operation.Equals("Audit"))
            {
                if (e.DataEntitys != null && e.DataEntitys.Count<DynamicObject>() > 0)
                {
                    foreach (DynamicObject item in e.DataEntitys)
                    {
                        // 获取销售出库单明细
                        DynamicObjectCollection outEntry = item["SAL_OUTSTOCKENTRY"] as DynamicObjectCollection;

                        if (outEntry != null && outEntry.Count > 0)
                        {
                            foreach (DynamicObject obj1 in outEntry)
                            {
                                //获取当前明细行包装单号
                                String packageNo = Convert.ToString(obj1["F_scfg_PackageCode"]);
                                if (!String.IsNullOrWhiteSpace(packageNo))
                                {
                                    // 获取当前明细行物料的id
                                    DynamicObject materialObj = obj1["MaterialID"] as DynamicObject;

                                    if (obj1 != null)
                                    {
                                        // 获取当前行的物料
                                        long materialId = Convert.ToInt64(materialObj["Id"]);
                                        // 获取当前物料的各个计量单位的重量
                                        double m2Num = Convert.ToDouble(obj1["F_scfg_M2Num"]);
                                        double zhangNum = Convert.ToDouble(obj1["F_scfg_ZhangNum"]);
                                        double geNum = Convert.ToDouble(obj1["F_scfg_GeNum"]);
                                        double xiangNum = Convert.ToDouble(obj1["F_scfg_XiangNum"]);
                                        double jianNum = Convert.ToDouble(obj1["F_scfg_JianNum"]);

                                        // 若包装单号不为空，则查找相同包装单号的条码拆装单
                                        String tmpSQL1 = String.Format(@"/*dialect*/ UPDATE t_UN_PackagingEntry SET F_SCFG_REALM2NUM = F_SCFG_REALM2NUM + {0}, F_SCFG_REALZHANGNUM = F_SCFG_REALZHANGNUM + {1}, F_SCFG_REALGENUM = F_SCFG_REALGENUM + {2}, F_SCFG_REALXIANGNUM = F_SCFG_REALXIANGNUM + {3}, F_SCFG_REALJIANNUM = F_SCFG_REALJIANNUM + {4} WHERE FID = (SELECT FID FROM t_UN_Packaging WHERE FPACKAGING = '{5}') AND FITEMID = {6} ", m2Num, zhangNum, geNum, xiangNum, jianNum, packageNo, materialId);
                                        DBUtils.Execute(this.Context, tmpSQL1.ToString());
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
