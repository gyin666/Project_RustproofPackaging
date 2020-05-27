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
    [Description("（简单生产入库）销售出库单保存前，根据各个物料的标准重量进行多计量单位重量的换算并进行赋值")]
    public class SetValuePlugIn : AbstractBillPlugIn
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
                    // 获取当前物料行的批号
                    String lotNo = Convert.ToString(col1[i]["Lot_Text"]);
                
                    // 根据物料条码查找条码主档中的数量字段得到公斤数，根据动态换算关系计算5个计量单位的数量，并赋值到各个字段上
                    StringBuilder tmpSQL2 = new StringBuilder();
                    tmpSQL2.AppendFormat(@"/*dialect*/ SELECT * FROM T_scfg_MaterialConvert MC LEFT JOIN T_BD_UNIT_L UL ON UL.FUNITID = MC.FUNITID WHERE MC.FMATERIALNUMBER = '{0}' AND F_SCFG_LOTNO = '{1}' ", materialId, lotNo);//批号  对应简单生产入库的出库
                    DynamicObjectCollection col2 = DBUtils.ExecuteDynamicObject(this.Context, tmpSQL2.ToString());
                    // 销售出库单中的物料出库数量
                    double realWeight = Convert.ToDouble(col1[i]["RealQty"]);
                    if (col2 != null && col2.Count > 0)
                    {
                        // 遍历当前物料的标准称重单位（公斤）与其他称重单位的转换参数
                        foreach (DynamicObject obj2 in col2)
                        {
                            // 目标称重单位数量
                            double rate1 = Convert.ToDouble(obj2["FQTY"]);

                                    // 计算公斤数量转换为各个称重单位的数值
                                    double realOtherWeight = Math.Round((double)(realWeight * rate1), 2, MidpointRounding.AwayFromZero);


                                    StringBuilder tmpSQL3 = new StringBuilder();
                                    String where = "";
                                    String key = "";
                                    switch (Convert.ToString(obj2["FNAME"]))
                                    {
                                        case "平方米":
                                            where = "F_SCFG_M2NUM";
                                            key = "M2NUM";
                                            break;
                                        case "张":
                                            where = "F_SCFG_ZHANGNUM";
                                            key = "ZHANGNUM";
                                            break;
                                        case "个":
                                            where = "F_SCFG_GENUM";
                                            key = "GENUM";
                                            break;
                                        case "箱":
                                        where = "F_SCFG_MULNUM";
                                        key = "MULNUM";
                                    break;
                                default:
                                    break;
                            }
                            if (!String.IsNullOrWhiteSpace(where) && !String.IsNullOrWhiteSpace(key))
                            {
                                this.View.Model.SetValue(where, Convert.ToDouble(realOtherWeight), i);
                            }           
                            if (where.Equals("F_SCFG_MULNUM"))
                            {
                                this.View.Model.SetValue(where, Convert.ToDouble(Math.Ceiling(realOtherWeight)), i);
                            }
                        }
                    }
                }
            }
            base.BeforeSave(e);
        }
    }
}
