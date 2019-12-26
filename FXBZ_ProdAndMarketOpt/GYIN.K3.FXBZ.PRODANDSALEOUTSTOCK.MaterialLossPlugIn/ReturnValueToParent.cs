using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Kingdee.BOS;
using Kingdee.BOS.Util;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;


namespace GYIN.K3.FXBZ.PRODANDSALEOUTSTOCK.MaterialLossPlugIn
{
    [Description("选择生产订单点击确定返回并赋值到父页面")]
    class ReturnValueToParent : AbstractDynamicFormPlugIn
    {
        public override void ButtonClick(ButtonClickEventArgs e)
        {
            // 用户点击确定按钮
            if (e.Key.EqualsIgnoreCase("F_JD_OK"))
            {

            }
        }
     }
}
