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
using Kingdee.BOS.Contracts.Report;
using Kingdee.BOS.Core.Report;

namespace GYIN.K3.FXBZ.PRODANDSALEOUTSTOCK.StaffMDStatPlugIn
{
    [Description("员工物耗统计月报表")]
    public class StaffMDStat : SysReportBaseService
    {
        //报表初始化   
        public override void Initialize()
        {
            base.Initialize();

            //设置报表类型：简单报表
            this.ReportProperty.ReportType = Kingdee.BOS.Core.Report.ReportType.REPORTTYPE_NORMAL;
            //设置报表名称
            this.ReportProperty.ReportName = new Kingdee.BOS.LocaleValue("员工物耗统计报表", base.Context.UserLocale.LCID);
            this.IsCreateTempTableByPlugin = true;
            this.ReportProperty.IsUIDesignerColumns = false;
            this.ReportProperty.IsGroupSummary = true;
            this.ReportProperty.SimpleAllCols = false;

            //设置报表主键字段名
            this.ReportProperty.IdentityFieldName = "FSeq";
        }

        public override void BuilderReportSqlAndTempTable(IRptParams filter, string tableName)
        {
            base.BuilderReportSqlAndTempTable(filter, tableName);
            DynamicObject dyFilter = filter.FilterParameter.CustomFilter;

            string deptId = "";//班组
            string staffId = "";//组织
            if (dyFilter["F_scfg_DeptFilter"] != null)
            {
                deptId = Convert.ToString(dyFilter["F_scfg_DeptFilter_Id"]);
            }
            if (dyFilter["F_scfg_StaffFilter"] != null)//员工姓名
            {
                staffId = Convert.ToString(dyFilter["F_scfg_StaffFilter_Id"]);
            }
           string sql = string.Format(@"");
          //  createTable(tableName);
           // insertTableNew(tableName, supplyer, orgId);
            //selectTable(tableName);
            // dropTable(tableName);
           DBUtils.Execute(this.Context, sql);
        }
    }
}
