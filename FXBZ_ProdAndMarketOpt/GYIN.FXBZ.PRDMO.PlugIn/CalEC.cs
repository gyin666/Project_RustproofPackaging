using Kingdee.BOS;
using Kingdee.BOS.App;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Orm.DataEntity;
using System;
using System.ComponentModel;
using System.Data;
using System.Linq;

namespace GYIN.FXBZ.PRDMO.PlugIn
{
    [Description("物料值更新计算延长米")]
    public class CalEC : AbstractBillPlugIn
    {
        public override void DataChanged(DataChangedEventArgs e)
        {
            string a = e.Field.Key.ToUpperInvariant();
            bool flag = a == "MaterialName" && e.NewValue != null;
            if (flag)
            {
                DynamicObject dynamicObject = this.Model.GetValue("MaterialName", e.Row) as DynamicObject;
                string FMaterialID = Convert.ToString(dynamicObject["Id"]);
                string strSQL = string.Format("/*dialect*/select F_SCFG_EC from T_ENG_BOM where fmaterialid='{0}'", FMaterialID);
                DynamicObjectCollection dynamicObjectCollection = DBUtils.ExecuteDynamicObject(base.Context, strSQL);
                if (dynamicObjectCollection != null && dynamicObjectCollection.Count() > 0)
                {
                    double EC = Convert.ToDouble(dynamicObjectCollection[0]["F_SCFG_EC"]);//延长米系数
                    double num = Convert.ToDouble(dynamicObjectCollection[0]["Qty"]);//数量
                    double ycm = EC * num;//延长米
                    this.Model.SetValue("F_scfg_EC", EC, e.Row);
                    this.Model.SetValue("F_scfg_Qty1", ycm, e.Row);
                }
            }
        }
    }
}
