using Kingdee.BOS.Orm.DataEntity;
using System;
using System.ComponentModel;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts.Report;
using Kingdee.BOS.Core.Report;
using System.Text;

namespace GYIN.K3.FXBZ.PRODANDSALEOUTSTOCK.StaffMDStatPlugIn
{
    [Description("排产计划报表")]
    public class StaffMDStat : SysReportBaseService
    {
        //报表初始化   
        public override void Initialize()
        {
            base.Initialize();

            //设置报表类型：简单报表
            this.ReportProperty.ReportType = Kingdee.BOS.Core.Report.ReportType.REPORTTYPE_NORMAL;
            //设置报表名称
            this.ReportProperty.ReportName = new Kingdee.BOS.LocaleValue("排产计划报表", base.Context.UserLocale.LCID);
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

            StringBuilder sql = new StringBuilder();
            sql.AppendFormat(@"/*dialect*/SELECT * INTO {0} ", tableName).Append("\n");
            sql.AppendLine(" FROM (").Append("\n");
            sql.AppendLine(" SELECT ROW_NUMBER() OVER (ORDER BY 通知单号,通知单厂家) AS FSeq,").Append("\n");
            sql = sql.AppendFormat(@"通知单号,通知单厂家,工序说明,型号规格,计划产量,交货日期,延长米,原纸规格,原纸数量,布板规格,布板数量,包材规格,包材数量,开始时间,结束时间,计划工时,包装方式,备注 
FROM (SELECT tmp0.FMONUMBER 通知单号,tmp1.FCREATEDATE 创建日期,tmp0.FOPERDESCRIPTION 工序说明,tmp1.FMOBILLNO + '-' + CONVERT (CHAR, tmp1.khbm) 通知单厂家,tmp1.FSPECIFICATION 型号规格,tmp0.FOPERQTY 计划产量,tmp1.F_SCFG_DATETIME1 交货日期,tmp1.F_scfg_Qty1 延长米,tmp1.yzgg 原纸规格,tmp2.yzsl 原纸数量,tmp3.bbgg 布板规格,tmp4.bbsl 布板数量,tmp5.bcgg 包材规格,tmp6.bcsl 包材数量,NULL 开始时间,NULL 结束时间,NULL 计划工时,tmp1.fdatavalue 包装方式,NULL 备注
FROM
(
SELECT a.FMONUMBER,a.FMOENTRYSEQ,cl.FOPERDESCRIPTION,FOPERQTY
FROM 
	T_SFC_OPERPLANNING a
INNER JOIN T_SFC_OPERPLANNINGSEQ b ON a.fid = b.fid
INNER JOIN T_SFC_OPERPLANNINGDETAIL c ON b.FENTRYID = c.FENTRYID
INNER JOIN T_SFC_OPERPLANNINGDETAIL_L cl on c.FDETAILID = cl.FDETAILID
WHERE 1=1
--and a.FMONUMBER='12105'
) tmp0
INNER JOIN(
--原纸规格
		SELECT
			tpp.FMOBILLNO,tbml0.FSPECIFICATION,tbml.FSPECIFICATION AS yzgg,CONVERT (CHAR, tbc.FNUMBER) khbm,tpp.F_SCFG_DATETIME1,tpp.F_scfg_Qty1,tbal.fdatavalue,tpp.FCREATEDATE
			FROM
				T_PRD_PPBOM tpp
			LEFT JOIN T_PRD_PPBOMENTRY tppe ON tpp.fid = tppe.fid
      LEFT JOIN T_BD_MATERIAL tbm0 ON tpp.FMATERIALID = tbm0.FMATERIALID
      INNER JOIN T_BD_MATERIAL_L tbml0 ON tbm0.FMATERIALID = tbml0.FMATERIALID
			LEFT JOIN T_BD_MATERIAL tbm ON tppe.FMATERIALID = tbm.FMATERIALID
			INNER JOIN T_BD_MATERIAL_L tbml ON tbm.FMATERIALID = tbml.FMATERIALID
      LEFT JOIN T_BD_CUSTOMER tbc --客户
      ON tbc.FCUSTID = tpp.F_SCFG_BASE
      LEFT JOIN T_BAS_ASSISTANTDATAENTRY_L tbal --辅助资料
      ON tpp.F_SCFG_ASSISTANT = tbal.fentryid
			WHERE
				tbm.FNUMBER LIKE '01.01%' 
     --   and tpp.FMOBILLNO ='12105'
) tmp1 
ON tmp0.FMONUMBER=tmp1.FMOBILLNO
INNER JOIN
--数量
	(SELECT
				tpp.FMOBILLNO,
				SUM (tppe.FMUSTQTY) yzsl
			FROM
				T_PRD_PPBOM tpp
			LEFT JOIN T_PRD_PPBOMENTRY tppe ON tpp.fid = tppe.fid
			LEFT JOIN T_BD_MATERIAL tbm ON tppe.FMATERIALID = tbm.FMATERIALID
			LEFT JOIN T_BD_MATERIAL_L tbml ON tbm.FMATERIALID = tbml.FMATERIALID
			WHERE
				tbm.FNUMBER LIKE '01.01%' 
     --   and tpp.FMOBILLNO ='12105'
			GROUP BY
				tpp.FMOBILLNO) tmp2
ON tmp1.FMOBILLNO=tmp2.FMOBILLNO

INNER JOIN(
--布板规格
		SELECT
			tpp.FMOBILLNO,tbml.FSPECIFICATION AS bbgg
			FROM
				T_PRD_PPBOM tpp
			LEFT JOIN T_PRD_PPBOMENTRY tppe ON tpp.fid = tppe.fid
			LEFT JOIN T_BD_MATERIAL tbm ON tppe.FMATERIALID = tbm.FMATERIALID
			INNER JOIN T_BD_MATERIAL_L tbml ON tbm.FMATERIALID = tbml.FMATERIALID
			WHERE
				tbm.FNUMBER LIKE '01.02%' 
     --   and tpp.FMOBILLNO ='12105'
) tmp3 
ON tmp2.FMOBILLNO=tmp3.FMOBILLNO
LEFT JOIN
--布板数量
	(SELECT
				tpp.FMOBILLNO,
				SUM (tppe.FMUSTQTY) bbsl
			FROM
				T_PRD_PPBOM tpp
			LEFT JOIN T_PRD_PPBOMENTRY tppe ON tpp.fid = tppe.fid
			LEFT JOIN T_BD_MATERIAL tbm ON tppe.FMATERIALID = tbm.FMATERIALID
			LEFT JOIN T_BD_MATERIAL_L tbml ON tbm.FMATERIALID = tbml.FMATERIALID
			WHERE
				tbm.FNUMBER LIKE '01.02%' 
    --    and tpp.FMOBILLNO ='12105'
			GROUP BY
				tpp.FMOBILLNO) tmp4
ON tmp3.FMOBILLNO=tmp4.FMOBILLNO

INNER JOIN(
--包材规格
		SELECT
			tpp.FMOBILLNO,tbml.FSPECIFICATION AS bcgg
			FROM
				T_PRD_PPBOM tpp
			LEFT JOIN T_PRD_PPBOMENTRY tppe ON tpp.fid = tppe.fid
			LEFT JOIN T_BD_MATERIAL tbm ON tppe.FMATERIALID = tbm.FMATERIALID
			INNER JOIN T_BD_MATERIAL_L tbml ON tbm.FMATERIALID = tbml.FMATERIALID
			WHERE
				tbm.FNUMBER LIKE '03%' 
   --     and tpp.FMOBILLNO ='12105'
) tmp5 
ON tmp4.FMOBILLNO=tmp5.FMOBILLNO
INNER JOIN
--包材数量
	(SELECT
				tpp.FMOBILLNO,
				SUM (tppe.FMUSTQTY) bcsl
			FROM
				T_PRD_PPBOM tpp
			LEFT JOIN T_PRD_PPBOMENTRY tppe ON tpp.fid = tppe.fid
			LEFT JOIN T_BD_MATERIAL tbm ON tppe.FMATERIALID = tbm.FMATERIALID
			LEFT JOIN T_BD_MATERIAL_L tbml ON tbm.FMATERIALID = tbml.FMATERIALID
			WHERE
				tbm.FNUMBER LIKE '03%' 
    --    and tpp.FMOBILLNO ='12105'
			GROUP BY
				tpp.FMOBILLNO) tmp6
ON tmp5.FMOBILLNO=tmp6.FMOBILLNO
WHERE 1=1
");
            if (dyFilter["F_scfg_prodIdFilterStr"] != null)
            {
                string prodId = Convert.ToString(dyFilter["F_scfg_prodIdFilterStr"]);//生产通知单号
                sql.AppendFormat(" and tmp0.FMONUMBER='{0}'", prodId);
            }
            //判断统计日期是否有效
            if (dyFilter["F_scfg_CreateDateFilter"] != null)
            {
             string createDateStart = Convert.ToDateTime(dyFilter["F_scfg_CreateDateFilter"]).ToString("yyyy-MM-dd 00:00:00");
             string createDateEnd = Convert.ToDateTime(dyFilter["F_scfg_CreateDateFilter"]).ToString("yyyy-MM-dd 23:59:59");
                sql.AppendFormat(" AND tmp1.FCREATEDATE >= '{0}' ", createDateStart);
                sql.AppendFormat(" AND tmp1.FCREATEDATE <= '{0}' ", createDateEnd);
            }
            sql.AppendLine(") tmp");
          //  sql.AppendLine(" ORDER BY tmp.通知单号,tmp.通知单厂家 DESC");
            DBUtils.Execute(this.Context, sql.ToString());
        }
        //构建报表列
        public override ReportHeader GetReportHeaders(IRptParams filter)
        {
            ReportHeader header = new ReportHeader();
            //通知单厂家
            var tzdcj = header.AddChild("通知单厂家", new Kingdee.BOS.LocaleValue("通知单/厂家"));
            tzdcj.ColIndex = 0;

            //工序说明
            var gxsm = header.AddChild("工序说明", new Kingdee.BOS.LocaleValue("工序说明"));
            gxsm.ColIndex = 1;

            //型号规格
            var xhgg = header.AddChild("型号规格", new Kingdee.BOS.LocaleValue("型号规格"));
            xhgg.ColIndex = 2;

            //计划产量
            var quota = header.AddChild("计划产量", new Kingdee.BOS.LocaleValue("计划产量"));
            quota.ColIndex = 3;

            //交货日期
            var jhrq = header.AddChild("交货日期", new Kingdee.BOS.LocaleValue("交货日期"));
            jhrq.ColIndex = 4;
            //延长米
            var ycm = header.AddChild("延长米", new Kingdee.BOS.LocaleValue("延长米"));
            ycm.ColIndex = 5;
            //原纸规格
            var yzgg = header.AddChild("原纸规格", new Kingdee.BOS.LocaleValue("原纸规格"));
            yzgg.ColIndex = 6;
            //原纸数量
            var yzsl = header.AddChild("原纸数量", new Kingdee.BOS.LocaleValue("原纸数量"));
            yzsl.ColIndex = 7;
            return header;
            //布板规格
            var bbgg = header.AddChild("布板规格", new Kingdee.BOS.LocaleValue("布板规格"));
            bbgg.ColIndex = 8;
            //布板数量
            var bbsl = header.AddChild("布板数量", new Kingdee.BOS.LocaleValue("布板数量"));
            bbsl.ColIndex = 9;
            //包材规格
            var bcgg = header.AddChild("包材规格", new Kingdee.BOS.LocaleValue("包材规格"));
            bcgg.ColIndex = 10;
            //包材数量
            var bcsl = header.AddChild("包材数量", new Kingdee.BOS.LocaleValue("包材数量"));
            bcsl.ColIndex = 11;
            //开始时间
            var kssj = header.AddChild("开始时间", new Kingdee.BOS.LocaleValue("开始时间"));
            kssj.ColIndex = 12;
            //结束时间
            var jssj = header.AddChild("结束时间", new Kingdee.BOS.LocaleValue("结束时间"));
            jssj.ColIndex = 13;
              //计划工时
            var jhgs = header.AddChild("计划工时", new Kingdee.BOS.LocaleValue("计划工时"));
            jhgs.ColIndex = 14;
            //包装方式
            var bzfs = header.AddChild("包装方式", new Kingdee.BOS.LocaleValue("包装方式"));
            bzfs.ColIndex = 15;
            //备注
            var bz = header.AddChild("备注", new Kingdee.BOS.LocaleValue("备注"));
            bz.ColIndex = 16;
            return header;
        }

        //反写报表过滤信息
        public override ReportTitles GetReportTitles(IRptParams filter)
        {
            var result = base.GetReportTitles(filter);
            DynamicObject dyFilter = filter.FilterParameter.CustomFilter;

            if (dyFilter != null)
            {
                if (result == null)
                {
                    result = new ReportTitles();
                }
                //反写过滤条件
                if (dyFilter["F_scfg_prodIdFilterStr"] != null)
                {
                    result.AddTitle("F_scfg_prodId", Convert.ToString(dyFilter["F_scfg_prodIdFilterStr"]));
                }
                else
                {
                    result.AddTitle("F_scfg_prodId", "全部");
                }
                if (dyFilter["F_scfg_CreateDateFilter"] != null)
                {
                    result.AddTitle("F_scfg_Date", Convert.ToString(dyFilter["F_scfg_CreateDateFilter"]));
                }
                else
                {
                    result.AddTitle("F_scfg_Date", DateTime.Now.ToShortDateString());
                }
            }

            return result;
        }
    }
}
