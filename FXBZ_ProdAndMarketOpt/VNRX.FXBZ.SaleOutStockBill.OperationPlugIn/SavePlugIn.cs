using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.SystemParameter.PlugIn.Args;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Core.Bill.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using System.ComponentModel;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Util;

namespace VNRX.FXBZ.SaleOutStockBill.OperationPlugIn
{
    [Description("销售出库单保存前，根据各个物料的标准重量进行多计量单位重量的换算并进行赋值")]
    public class SavePlugIn : AbstractBillPlugIn
    {
        public override void BeforeSave(Kingdee.BOS.Core.Bill.PlugIn.Args.BeforeSaveEventArgs e)
        {
            // 获取销售出库单明细行
            DynamicObjectCollection col1 = this.View.Model.DataObject["SAL_OUTSTOCKENTRY"] as DynamicObjectCollection;

            // 遍历物料明细行
            for (int i = 0; i < col1.Count; i++)
            {
                // 获取当前行的物料编码信息
                DynamicObject materialObj = col1[i]["MaterialID"] as DynamicObject;
                if (materialObj != null)
                {
                    // 获取当前物料的内码
                    long materialId = Convert.ToInt64(materialObj["Id"]);

                    // 获取当前物料的包装单号
                    String packNo = Convert.ToString(col1[i]["F_QSNC_PackageNum"]);

                    // 若不为空，则是进行打包的物料
                    if (!String.IsNullOrWhiteSpace(packNo))
                    {
                        // 根据包装单号查找对应的条码包装记录，将明细行中的各计量单位数量字段取出，赋值到销售出库单明细行，对应物料的对应数量字段
                        
                        // 根据包装单号获取上游条码拆装单中该物料的信息
                        String tmpSQL1 = String.Format(@"/*dialect*/ SELECT * FROM t_UN_PackagingEntry PE LEFT JOIN t_UN_Packaging P ON PE.FID = P.FID WHERE P.FPACKAGING = '{0}' AND PE.FITEMID = {1} ", packNo, materialId);
                        DynamicObjectCollection col2 = DBUtils.ExecuteDynamicObject(this.Context, tmpSQL1);

                        if (col2 != null && col2.Count > 0)
                        {
                            // 将条码拆装单中的各个计量单位的数值赋值到销售出库单
                            this.View.Model.SetValue("F_QSNC_M2NUM", col2[0]["F_QSNC_M2NUM"], i); // F_QSNC_M2NUM
                            this.View.Model.SetValue("F_QSNC_ZHANGNUM", col2[0]["F_QSNC_ZHANGNUM"], i); // F_QSNC_ZHANGNUM
                            this.View.Model.SetValue("F_QSNC_GENUM", col2[0]["F_QSNC_GENUM"], i); // F_QSNC_GENUM
                            this.View.Model.SetValue("F_QSNC_XIANGNUM", col2[0]["F_QSNC_XIANGNUM"], i); // F_QSNC_XIANGNUM
                            this.View.Model.SetValue("F_QSNC_JIANNUM", col2[0]["F_QSNC_JIANNUM"], i); // F_QSNC_JIANNUM
                        }
                    }
                    else
                    {
                        // 根据物料条码查找条码主档中的数量字段得到公斤数，根据物料单位换算关系计算5个计量单位的数量，并赋值到各个字段上
                        double realWeight = Convert.ToDouble(col1[i]["FREALQTY"]);

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
                                // 标准计量称重单位数量
                                double rate2 = Convert.ToDouble(obj2["FCONVERTNUMERATOR"]);

                                // 计算公斤数量转换为各个称重单位的数值
                                double realOtherWeight = (realWeight / rate2) * rate1;

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
                                    this.View.Model.SetValue(where, col2[0][where], i);
                                }
                            }
                        }
                    }
                }
            }
            base.BeforeSave(e);
        }
    }
}
