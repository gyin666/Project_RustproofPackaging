using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts.Report;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.Orm.DataEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.App;
using System.ComponentModel;

namespace VNRX.FXBZ.InstantStockReport.PlugIn
{
    [Description("即时库存统计报表")]
    public class Class1 : SysReportBaseService
    {
        private string[] materialRptTableNames;

        //线索转化分析表初始化
        public override void Initialize()
        {
            base.Initialize();

            //设置报表类型：简单报表
            this.ReportProperty.ReportType = Kingdee.BOS.Core.Report.ReportType.REPORTTYPE_NORMAL;
            //设置报表名称
            this.ReportProperty.ReportName = new Kingdee.BOS.LocaleValue("即时库存统计报表", base.Context.UserLocale.LCID);
            this.IsCreateTempTableByPlugin = true;
            this.ReportProperty.IsUIDesignerColumns = false;
            this.ReportProperty.IsGroupSummary = true;
            this.ReportProperty.SimpleAllCols = false;

            //设置报表主键字段名
            this.ReportProperty.IdentityFieldName = "FSeq";

        }

        //向临时表插入报表数据
        public override void BuilderReportSqlAndTempTable(IRptParams filter, string tableName)
        {
            base.BuilderReportSqlAndTempTable(filter, tableName);
            // -----------------------------------------------------------------------------------------------------------------------------------------------

            //生成中间临时表
            IDBService dbservice = ServiceHelper.GetService<IDBService>();
            materialRptTableNames = dbservice.CreateTemporaryTableName(this.Context, 10);
            string tmpTable1 = materialRptTableNames[0];
            string tmpTable2 = materialRptTableNames[1];
            string tmpTable3 = materialRptTableNames[2];
            string tmpTable4 = materialRptTableNames[3];
            string tmpTable5 = materialRptTableNames[4];
            string tmpTable6 = materialRptTableNames[5];
            string tmpTable7 = materialRptTableNames[6];
            string tmpTable8 = materialRptTableNames[7];
            string tmpTable9 = materialRptTableNames[8];

            //过滤条件：物料编码/仓库/批号
            DynamicObject dyFilter = filter.FilterParameter.CustomFilter;


            // 物料
            StringBuilder materialSql = new StringBuilder();
            if (dyFilter["F_scfg_MaterialIdFilter"] != null && ((DynamicObjectCollection)dyFilter["F_scfg_MaterialIdFilter"]).Count > 0)
            {
                //获取到多选基础资料中所有选中项
                DynamicObjectCollection cols1 = (DynamicObjectCollection)dyFilter["F_scfg_MaterialIdFilter"];
                int materialNum = 0;

                if (cols1.Count >= 1)
                {
                    materialSql.Append(" IN (");
                }

                foreach (DynamicObject saler in cols1)
                {
                    String salerNumber = Convert.ToString(((DynamicObject)saler["F_scfg_MaterialIdFilter"])["Id"]);
                    materialNum++;

                    if (cols1.Count == materialNum)
                    {
                        materialSql.Append(salerNumber + ")");
                    }
                    else
                    {
                        materialSql.Append(salerNumber + ", ");
                    }
                }
            }

            // 仓库
            StringBuilder stockSql = new StringBuilder();
            if (dyFilter["F_scfg_StockIdFilter"] != null && ((DynamicObjectCollection)dyFilter["F_scfg_StockIdFilter"]).Count > 0)
            {
                //获取到多选基础资料中所有选中项
                DynamicObjectCollection cols2 = (DynamicObjectCollection)dyFilter["F_scfg_StockIdFilter"];
                int stockNum = 0;

                if (cols2.Count >= 1)
                {
                    stockSql.Append(" IN (");
                }

                foreach (DynamicObject dept in cols2)
                {
                    String deptNumber = Convert.ToString(((DynamicObject)dept["F_scfg_StockIdFilter"])["Id"]);
                    stockNum++;

                    if (cols2.Count == stockNum)
                    {
                        stockSql.Append( deptNumber + ")");
                    }
                    else
                    {
                        stockSql.Append(deptNumber + ", ");
                    }
                }
            }

            // 批号
            StringBuilder batchSql = new StringBuilder();
            if (dyFilter["F_scfg_BatchFilter"] != null && ((DynamicObjectCollection)dyFilter["F_scfg_BatchFilter"]).Count > 0)
            {
                //获取到多选基础资料中所有选中项
                DynamicObjectCollection cols2 = (DynamicObjectCollection)dyFilter["F_scfg_BatchFilter"];
                int batchNum = 0;

                if (cols2.Count >= 1)
                {
                    batchSql.Append(" IN (");
                }

                foreach (DynamicObject dept in cols2)
                {
                    String deptNumber = Convert.ToString(((DynamicObject)dept["F_scfg_BatchFilter"])["Id"]);
                    batchNum++;

                    if (cols2.Count == batchNum)
                    {
                        batchSql.Append(deptNumber + ")");
                    }
                    else
                    {
                        batchSql.Append(deptNumber + ", ");
                    }
                }
            }

            
            // 20191225 增加简单生产入库单 简单生产领料单
            // ---------------------------------------------------------------------------------------------------------------
            // tmpTable1 存放所有仓库进库数量（除了条码拆装单）
            StringBuilder tmpSQL1 = new StringBuilder();
            tmpSQL1.AppendFormat(@"/*dialect*/ SELECT FMATERIALID, FSTOCKID, FLOT, FAUXPROPID, SUM(FREALQTY) PARTQTY, SUM(F_SCFG_M2NUM) PARTM2, SUM(F_SCFG_ZHANGNUM) PARTZHANG, SUM(F_SCFG_GENUM) PARTGE, SUM(F_scfg_MulNum) PARTMUL INTO {0}
                                                FROM
                                                    (SELECT FMATERIALID, FSTOCKID, FLOT, FAUXPROPID, FREALQTY, 0 F_SCFG_M2NUM, 0 F_SCFG_ZHANGNUM, 0 F_SCFG_GENUM, 0 F_scfg_MulNum FROM T_STK_INSTOCKENTRY SE LEFT JOIN t_STK_InStock S ON S.FID = SE.FID WHERE S.FDOCUMENTSTATUS IN ('B', 'C') 
                                                    UNION
                                                    SELECT FMATERIALID, IE.FSTOCKID, FLOT, FAUXPROPID, FREALQTY, F_SCFG_DECIMAL4, F_SCFG_DECIMAL6, F_SCFG_DECIMAL, F_scfg_MulNum FROM T_PRD_INSTOCKENTRY IE LEFT JOIN T_PRD_INSTOCK I ON IE.FID = I.FID WHERE I.F_SCFG_ISPACKAGE = 0 AND I.FDOCUMENTSTATUS IN ('B', 'C') 
                                                    UNION
                                                    SELECT FMATERIALID, FSTOCKID, FLOT, FAUXPROPID, FQTY, F_SCFG_M2NUM, F_SCFG_ZHANGNUM, F_SCFG_GENUM, F_scfg_MulNum FROM T_STK_MISCELLANEOUSENTRY MCE LEFT JOIN T_STK_MISCELLANEOUS MC ON MCE.FID = MC.FID WHERE MC.FDOCUMENTSTATUS IN ('B', 'C') 
                                                    UNION
                                                    SELECT FMATERIALID, FDESTSTOCKID, FLOT, FAUXPROPID, FQTY, F_SCFG_M2NUM, F_SCFG_ZHANGNUM, F_SCFG_GENUM, F_scfg_MulNum FROM T_STK_STKTRANSFERINENTRY SSE LEFT JOIN T_STK_STKTRANSFERIN SS ON SSE.FID = SS.FID WHERE SS.FDOCUMENTSTATUS IN ('B', 'C')
                                                    UNION 
                                                    SELECT ID.FMATERIALID, ID.FSTOCKID, ID.FLOT, FAUXPROPID, ID.FBASEQTY, 0 F_SCFG_M2NUM, 0 F_SCFG_ZHANGNUM, 0 F_SCFG_GENUM, 0 F_scfg_MulNum FROM T_STK_INVINITDETAIL ID LEFT JOIN T_STK_INVINIT I ON ID.FID = I.FID WHERE I.FDOCUMENTSTATUS IN ('B', 'C')
                                                    UNION
                                                    SELECT FMATERIALID, ISE.FSTOCKID, FLOT, FAUXPROPID, FBASEREALQTY, F_SCFG_M2NUM, F_SCFG_ZHANGNUM, F_SCFG_GENUM, F_SCFG_MULNUM FROM T_SP_INSTOCKENTRY ISE LEFT JOIN T_SP_INSTOCK I ON ISE.FID = I.FID WHERE I.FDOCUMENTSTATUS IN ('B', 'C')
                                                ) TMP
                                                WHERE TMP.FMATERIALID != 0 AND TMP.FSTOCKID != 0 AND TMP.FLOT != 0
                                                GROUP BY FMATERIALID, FSTOCKID, FLOT, FAUXPROPID ", tmpTable1);
            DBUtils.ExecuteDynamicObject(this.Context, tmpSQL1.ToString());

            // --------------------------------------------------------------------------------------------------------------------
            // tmpTable2 存放全部进库数量
            StringBuilder tmpSQL2 = new StringBuilder();
            tmpSQL2.AppendFormat(@"/*dialect*/ SELECT TMP.FMATERIALID, 
		                                            TMP.FSTOCKID, 
		                                            TMP.FLOT, 
                                                    TMP.FAUXPROPID,
		                                            (TMP.PARTQTY + ISNULL(TMP1.FQTY, 0)) ALLINQTY,
		                                            (TMP.PARTM2 + ISNULL(TMP1.F_SCFG_M2NUM, 0)) ALLM2IN,
		                                            (TMP.PARTZHANG + ISNULL(TMP1.F_SCFG_ZHANGNUM, 0)) ALLZHANGIN,
		                                            (TMP.PARTGE + ISNULL(TMP1.F_SCFG_GENUM, 0)) ALLGEIN,
		                                            (TMP.PARTMUL + ISNULL(TMP1.F_scfg_MulNum, 0)) ALLMULIN INTO {0} FROM {1} TMP
                                            LEFT JOIN (SELECT PE.FITEMID, PE.FLOT, BCM.FAUXPROPID, BCM.FQTY, F_SCFG_M2NUM, F_SCFG_ZHANGNUM, F_SCFG_GENUM, F_scfg_MulNum from t_UN_PackagingEntry PE LEFT JOIN T_BD_BARCODEMAIN BCM ON BCM.FBARCODE = PE.FENTRYBARCODE WHERE PE.FITEMID != 0 AND PE.FLOT != 0) TMP1
                                            ON TMP1.FITEMID = TMP.FMATERIALID AND TMP1.FLOT = TMP.FLOT AND TMP1.FAUXPROPID = TMP.FAUXPROPID ", tmpTable2, tmpTable1);
            DBUtils.ExecuteDynamicObject(this.Context, tmpSQL2.ToString());


            // --------------------------------------------------------------------------------------------------------------------
            // tmpTable3中存放部分仓库出数量
            StringBuilder tmpSQL3 = new StringBuilder();
            tmpSQL3.AppendFormat(@"/*dialect*/ SELECT FMATERIALID, FSTOCKID, FLOT, FAUXPROPID, SUM(FREALQTY) ALLOUTQTY, SUM(F_SCFG_M2NUM) ALLM2OUT, SUM(F_SCFG_ZHANGNUM) ALLZHANGOUT, SUM(F_SCFG_GENUM) ALLGEOUT, SUM(F_scfg_MulNum) ALLMULOUT INTO {0}
                                                FROM
                                                (SELECT OE.FMATERIALID, OE.FSTOCKID, OE.FLOT, FAUXPROPID, OE.FREALQTY, F_SCFG_M2NUM, F_SCFG_ZHANGNUM, F_SCFG_GENUM, F_scfg_MulNum FROM T_SAL_OUTSTOCKENTRY OE LEFT JOIN T_SAL_OUTSTOCK O ON OE.FID = O.FID WHERE O.FDOCUMENTSTATUS IN ('B', 'C') 
                                                 UNION
                                                 SELECT PE.FMATERIALID, PE.FSTOCKID, PE.FLOT, FAUXPROPID, PE.FACTUALQTY, 0 F_SCFG_M2NUM, 0 F_SCFG_ZHANGNUM, 0 F_SCFG_GENUM, 0 F_scfg_MulNum FROM T_PRD_PICKMTRLDATA PE LEFT JOIN T_PRD_PICKMTRL P ON PE.FID = P.FID WHERE P.FDOCUMENTSTATUS IN ('B', 'C') 
                                                 UNION
                                                 SELECT MDE.FMATERIALID, MDE.FSTOCKID, MDE.FLOT, FAUXPROPID, MDE.FQTY, F_SCFG_M2NUM, F_SCFG_ZHANGNUM, F_SCFG_GENUM, F_scfg_MulNum FROM T_STK_MISDELIVERYENTRY MDE LEFT JOIN T_STK_MISDELIVERY MD ON MDE.FID = MD.FID WHERE MD.FDOCUMENTSTATUS IN ('B', 'C') 
                                                 UNION
                                                 SELECT SSE.FMATERIALID, SSE.FSRCSTOCKID, SSE.FLOT, FAUXPROPID, SSE.FQTY, F_SCFG_M2NUM, F_SCFG_ZHANGNUM, F_SCFG_GENUM, F_scfg_MulNum FROM T_STK_STKTRANSFERINENTRY SSE LEFT JOIN T_STK_STKTRANSFERIN SS ON SSE.FID = SS.FID WHERE SS.FDOCUMENTSTATUS IN ('B', 'C')
                                                 UNION
                                                 SELECT FMATERIALID, FSTOCKID, FLOT, FAUXPROPID, FBASEACTUALQTY, F_SCFG_M2NUM, F_SCFG_ZHANGNUM, F_SCFG_GENUM, F_SCFG_MULNUM FROM T_SP_PICKMTRLDATA PMD LEFT JOIN T_SP_PICKMTRL PM ON PMD.FID = PM.FID WHERE PM.FDOCUMENTSTATUS IN ('B', 'C')
                                                ) TMP
                                                GROUP BY FMATERIALID, FSTOCKID, FLOT, FAUXPROPID ", tmpTable3);
            DBUtils.ExecuteDynamicObject(this.Context, tmpSQL3.ToString());

            //// ----------------------------------------------------------------------------------------------------------------------
            //// tmpTable4 中存放全部物料的出库数量
            //StringBuilder tmpSQL4 = new StringBuilder();
            //tmpSQL4.AppendFormat(@"/*dialect*/ SELECT TMP.FMATERIALID, 
            //                                            TMP.FSTOCKID, 
            //                                            TMP.FLOT, 
            //                                            (TMP.PARTOUTQTY + ISNULL(TMP1.F_SCFG_REALOUTQTY, 0)) ALLOUTQTY,
		          //                                      (TMP.PARTM2 + ISNULL(TMP1.F_SCFG_REALM2NUM, 0)) ALLM2OUT,
		          //                                      (TMP.PARTZHANG + ISNULL(TMP1.F_SCFG_REALZHANGNUM, 0)) ALLZHANGOUT,
		          //                                      (TMP.PARTGE + ISNULL(TMP1.F_SCFG_REALGENUM, 0)) ALLGEOUT,
		          //                                      (TMP.PARTMUL + ISNULL(TMP1.F_scfg_realMulNum, 0)) ALLMULOUT INTO {0} FROM {1} TMP
            //                                    LEFT JOIN (SELECT PE.FITEMID, PE.FLOT, PE.F_SCFG_REALOUTQTY, F_SCFG_REALM2NUM, F_SCFG_REALZHANGNUM, F_SCFG_REALGENUM, F_scfg_realMulNum FROM t_UN_PackagingEntry PE LEFT JOIN t_UN_Packaging P ON PE.FID = P.FID WHERE PE.FITEMID != 0 AND PE.FLOT != 0) TMP1
            //                                    ON TMP1.FITEMID = TMP.FMATERIALID AND TMP1.FLOT = TMP.FLOT ", tmpTable4, tmpTable3);
            //DBUtils.ExecuteDynamicObject(this.Context, tmpSQL4.ToString());

            // ----------------------------------------------------------------------------------------------------------------------
            // tmpTable5 中存放全部物料现有的库存
            StringBuilder tmpSQL5 = new StringBuilder();
            tmpSQL5.AppendFormat(@"/*dialect*/ SELECT TMP2.FMATERIALID, 
		                                                TMP2.FSTOCKID, 
		                                                TMP2.FLOT, 
                                                        CASE WHEN FF100001 IS NOT NULL AND FF100002 IS NOT NULL THEN (CASE WHEN FF100001 != '' THEN FF100001 ELSE FF100002 END) ELSE '' END AUXID,
		                                                (TMP2.ALLINQTY - ISNULL(TMP4.ALLOUTQTY, 0)) STOCKQTY,
		                                                (TMP2.ALLM2IN - ISNULL(TMP4.ALLM2OUT, 0)) M2NUM,
		                                                (TMP2.ALLZHANGIN - ISNULL(TMP4.ALLZHANGOUT, 0)) ZHANGNUM,
		                                                (TMP2.ALLGEIN - ISNULL(TMP4.ALLGEOUT, 0)) GENUM,
		                                                (TMP2.ALLMULIN - ISNULL(TMP4.ALLMULOUT, 0)) MULNUM
                                                INTO {0}
                                                FROM {1} TMP2 
                                                LEFT JOIN {2} TMP4 ON TMP2.FMATERIALID = TMP4.FMATERIALID AND TMP2.FSTOCKID = TMP4.FSTOCKID AND TMP2.FLOT = TMP4.FLOT AND TMP2.FAUXPROPID = TMP4.FAUXPROPID
                                                LEFT JOIN T_BD_FlexsItemDetailV FIDV ON FIDV.FID = TMP2.FAUXPROPID ", tmpTable5, tmpTable2, tmpTable3);
            DBUtils.ExecuteDynamicObject(this.Context, tmpSQL5.ToString());

            // ----------------------------------------------------------------------------------------------------------------------
            // 增加辅助计量单位携带 FAUXPROPID
            // tmpTable6 中存放多计量单位换算报表
            StringBuilder tmpSQL6 = new StringBuilder();
            tmpSQL6.AppendFormat(@"/*dialect*/ SELECT M.FNUMBER MATERIALNUMBER,
	                                       ML.FNAME MATERIALNAME,
	                                       ML.FSPECIFICATION MATERIALSPECIFICATION,
	                                       S.FNUMBER STOCKNUMBER,
	                                       SL.FNAME STOCKNAME,
	                                       L.FNUMBER LOTNUMBER,
                                           ISNULL(ADEL.FDATAVALUE, '') FDATAVALUE,
	                                       '公斤' BASICUNIT,
	                                       CONVERT(FLOAT,ROUND(TMP5.STOCKQTY, 2)) STOCKQTY1,
	                                       '平方米' M2,
	                                       CONVERT(FLOAT,ROUND(M2NUM, 2)) M2NUM1,
	                                       '个' GE,
	                                       CONVERT(FLOAT,ROUND(GENUM, 2)) GENUM1,
	                                       '张' ZHANG,
	                                       CONVERT(FLOAT,ROUND(ZHANGNUM, 2)) ZHANGNUM1,
	                                       '箱/卷/件' MUL,
	                                       MULNUM MULNUM1
                                    INTO {0}
                                    FROM {1} TMP5
                                    LEFT JOIN T_BD_MATERIAL M ON TMP5.FMATERIALID = M.FMATERIALID
                                    LEFT JOIN T_BD_MATERIAL_L ML ON TMP5.FMATERIALID = ML.FMATERIALID
                                    LEFT JOIN t_BD_Stock S ON TMP5.FSTOCKID = S.FSTOCKID
                                    LEFT JOIN t_BD_Stock_L SL ON TMP5.FSTOCKID = SL.FSTOCKID 
                                    LEFT JOIN T_BD_LOTMASTER L ON TMP5.FLOT = L.FLOTID
                                    LEFT JOIN T_BAS_ASSISTANTDATAENTRY_L ADEL ON ADEL.fentryid = TMP5.AUXID
                                    WHERE CONVERT(FLOAT,ROUND(TMP5.STOCKQTY, 2)) != 0 ", tmpTable6, tmpTable5);
            // 物料
            if (dyFilter["F_scfg_MaterialIdFilter"] != null && ((DynamicObjectCollection)dyFilter["F_scfg_MaterialIdFilter"]).Count > 0)
            {
                tmpSQL6.AppendLine(" AND TMP5.FMATERIALID ").Append(materialSql);
            }
            // 仓库
            if (dyFilter["F_scfg_StockIdFilter"] != null && ((DynamicObjectCollection)dyFilter["F_scfg_StockIdFilter"]).Count > 0)
            {
                tmpSQL6.AppendLine(" AND TMP5.FSTOCKID ").Append(stockSql);
            }
            // 批号
            if (dyFilter["F_scfg_BatchFilter"] != null && ((DynamicObjectCollection)dyFilter["F_scfg_BatchFilter"]).Count > 0)
            {
                tmpSQL6.AppendLine(" AND TMP5.FLOT ").Append(batchSql);
            }
            DBUtils.ExecuteDynamicObject(this.Context, tmpSQL6.ToString());

            
            // ---------------------------------------------------------------------------------------------------------------------
            // tmpTable7 获取各个仓库多计量单位小计
            StringBuilder tmpSQL7 = new StringBuilder();
            tmpSQL7.AppendFormat(@"/*dialect*/ SELECT MATERIALNUMBER, MATERIALNAME, MATERIALSPECIFICATION, STOCKNUMBER, STOCKNAME, SUM(STOCKQTY1) TOTALSTOCKQTY, SUM(M2NUM1) TOTALM2NUM, SUM(GENUM1) TOTALGENUM, SUM(ZHANGNUM1) TOTALZHANGNUM, SUM(MULNUM1) TOTALMULNUM INTO {0} FROM {1} GROUP BY MATERIALNUMBER, MATERIALNAME, MATERIALSPECIFICATION, STOCKNUMBER, STOCKNAME ", tmpTable7, tmpTable6);
            DBUtils.ExecuteDynamicObject(this.Context, tmpSQL7.ToString());

            // 计算总计
            // ----------------------------------------------------------------------------------------------------------------------
            StringBuilder tmpSQL8 = new StringBuilder();
            tmpSQL8.AppendFormat(@"/*dialect*/ SELECT SUM(STOCKQTY1) TOTALSTOCKQTY, SUM(M2NUM1) TOTALM2NUM, SUM(GENUM1) TOTALGENUM, SUM(ZHANGNUM1) TOTALZHANGNUM, SUM(MULNUM1) TOTALMULNUM INTO {0} FROM {1} ", tmpTable8, tmpTable6);
            DBUtils.ExecuteDynamicObject(this.Context, tmpSQL8.ToString());

            // 将仓库小计斤系插入总表中
            // ----------------------------------------------------------------------------------------------------------------------
            StringBuilder tmpSQL9 = new StringBuilder();
            tmpSQL9.AppendFormat(@"/*dialect*/ INSERT INTO {0} SELECT MATERIALNUMBER, MATERIALNAME, MATERIALSPECIFICATION, STOCKNUMBER, STOCKNAME + ' - 小计', '', '', '', TOTALSTOCKQTY, '', TOTALM2NUM, '', TOTALGENUM, '', TOTALZHANGNUM, '', TOTALMULNUM FROM {1} ", tmpTable6, tmpTable7);
            DBUtils.ExecuteDynamicObject(this.Context, tmpSQL9.ToString());

            // ----------------------------------------------------------------------------------------------------------------------
            StringBuilder tmpSQL11 = new StringBuilder();
            tmpSQL11.AppendFormat(@"/*dialect*/ INSERT INTO {0} SELECT '合计', '', '', '', '', '', '', '', TOTALSTOCKQTY, '', TOTALM2NUM, '', TOTALGENUM, '', TOTALZHANGNUM, '', TOTALMULNUM FROM {1} ", tmpTable6, tmpTable8);
            DBUtils.ExecuteDynamicObject(this.Context, tmpSQL11.ToString());

            // ------------------------------------------------------------------------------------------------------------------------
            // 将总体结果进行插入系统提供的tablename中
            StringBuilder tmpSQL10 = new StringBuilder();
            tmpSQL10.AppendFormat(@"/*dialect*/ SELECT ROW_NUMBER() OVER (ORDER BY MATERIALNUMBER, MATERIALNAME, MATERIALSPECIFICATION, STOCKNUMBER, STOCKNAME) AS FSeq, MATERIALNUMBER, MATERIALNAME, MATERIALSPECIFICATION, STOCKNUMBER, STOCKNAME, LOTNUMBER, FDATAVALUE, BASICUNIT, CONVERT(FLOAT,ROUND(STOCKQTY1, 2)) STOCKQTY2, M2, CONVERT(FLOAT,ROUND(M2NUM1, 2)) M2NUM2, GE, CONVERT(FLOAT,ROUND(GENUM1, 2)) GENUM2, ZHANG, CONVERT(FLOAT,ROUND(ZHANGNUM1, 2)) ZHANGNUM2, MUL, CONVERT(FLOAT,ROUND(MULNUM1, 2)) MULNUM2 INTO {0} FROM {1} ORDER BY MATERIALNUMBER, MATERIALNAME, MATERIALSPECIFICATION, STOCKNUMBER, STOCKNAME ", tableName, tmpTable6);
            DBUtils.ExecuteDynamicObject(this.Context, tmpSQL10.ToString());
        }

        //构建报表列
        public override ReportHeader GetReportHeaders(IRptParams filter)
        {
            ReportHeader header = new ReportHeader();

            // 物料编码
            var MATERIALNUMBER = header.AddChild("MATERIALNUMBER", new Kingdee.BOS.LocaleValue("物料编码"));
            MATERIALNUMBER.ColIndex = 0;
            MATERIALNUMBER.Width = 100;

            // 物料名称
            var MATERIALNAME = header.AddChild("MATERIALNAME", new Kingdee.BOS.LocaleValue("物料名称"));
            MATERIALNAME.ColIndex = 1;
            MATERIALNAME.Width = 150;

            // 规格型号
            var MATERIALSPECIFICATION = header.AddChild("MATERIALSPECIFICATION", new Kingdee.BOS.LocaleValue("规格型号"));
            MATERIALSPECIFICATION.ColIndex = 2;

            // 仓库编码
            var STOCKNUMBER = header.AddChild("STOCKNUMBER", new Kingdee.BOS.LocaleValue("仓库编码"));
            STOCKNUMBER.ColIndex = 3;

            // 仓库名称
            var STOCKNAME = header.AddChild("STOCKNAME", new Kingdee.BOS.LocaleValue("仓库名称"));
            STOCKNAME.ColIndex = 4;
            STOCKNAME.Width = 200;

            // 批号
            var LOTNUMBER = header.AddChild("LOTNUMBER", new Kingdee.BOS.LocaleValue("批号"));
            LOTNUMBER.ColIndex = 5;
            LOTNUMBER.Width = 100;

            var AUXNAME = header.AddChild("FDATAVALUE", new Kingdee.BOS.LocaleValue("辅助属性"));
            AUXNAME.ColIndex = 6;
            AUXNAME.Width = 200;

            // 基本单位
            var BASICUNIT = header.AddChild("BASICUNIT", new Kingdee.BOS.LocaleValue("基本单位"));
            BASICUNIT.ColIndex = 7;

            // 基本数量
            var STOCKQTY = header.AddChild("STOCKQTY2", new Kingdee.BOS.LocaleValue("基本数量"));
            STOCKQTY.ColIndex = 8;
            STOCKQTY.Width = 130;

            // 单位（平方米）
            var M2 = header.AddChild("M2", new Kingdee.BOS.LocaleValue("单位（平方米）"));
            M2.ColIndex = 9;

            // 平米数
            var M2NUM = header.AddChild("M2NUM2", new Kingdee.BOS.LocaleValue("平米数"));
            M2NUM.ColIndex = 10;
            M2NUM.Width = 130;

            // 单位（个）
            var GE = header.AddChild("GE", new Kingdee.BOS.LocaleValue("单位（个）"));
            GE.ColIndex = 11;

            // 个数
            var GENUM = header.AddChild("GENUM2", new Kingdee.BOS.LocaleValue("个数"));
            GENUM.ColIndex = 12;
            GENUM.Width = 130;

            //// 单位（件）
            //var JIAN = header.AddChild("JIAN", new Kingdee.BOS.LocaleValue("单位（件）"));
            //JIAN.ColIndex = 12;

            //// 件数
            //var JIANNUM = header.AddChild("JIANNUM1", new Kingdee.BOS.LocaleValue("件数"));
            //JIANNUM.ColIndex = 13;
            //JIANNUM.Width = 130;

            // 单位（张）
            var ZHANG = header.AddChild("ZHANG", new Kingdee.BOS.LocaleValue("单位（张）"));
            ZHANG.ColIndex = 13;

            // 张数
            var ZHANGNUM = header.AddChild("ZHANGNUM2", new Kingdee.BOS.LocaleValue("张数"));
            ZHANGNUM.ColIndex = 14;
            ZHANGNUM.Width = 130;

            // 单位（箱/卷/件）
            var MUL = header.AddChild("MUL", new Kingdee.BOS.LocaleValue("单位（箱/卷/件）"));
            MUL.ColIndex = 15;
            MUL.Width = 130;


            // 箱/卷/件数
            var MULNUM = header.AddChild("MULNUM2", new Kingdee.BOS.LocaleValue("箱/卷/件数"));
            MULNUM.ColIndex = 16;
            MULNUM.Width = 130;

            return header;
        }

        //准备报表的表头信息
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

                // 物料
                if (dyFilter["F_scfg_MaterialIdFilter"] != null && ((DynamicObjectCollection)dyFilter["F_scfg_MaterialIdFilter"]).Count > 0)
                {
                    StringBuilder tmpNameCol = new StringBuilder();
                    DynamicObjectCollection cols = (DynamicObjectCollection)dyFilter["F_scfg_MaterialIdFilter"];
                    foreach (DynamicObject obj in cols)
                    {
                        String tmpName = Convert.ToString(((DynamicObject)obj["F_scfg_MaterialIdFilter"])["Name"]);
                        tmpNameCol.Append(tmpName + "; ");
                    }

                    result.AddTitle("F_scfg_MaterialId", tmpNameCol.ToString());
                }
                else
                {
                    result.AddTitle("F_scfg_MaterialId", "全部物料");
                }

                // 仓库
                if (dyFilter["F_scfg_StockIdFilter"] != null && ((DynamicObjectCollection)dyFilter["F_scfg_StockIdFilter"]).Count > 0)
                {
                    StringBuilder tmpNameCol = new StringBuilder();
                    DynamicObjectCollection cols = (DynamicObjectCollection)dyFilter["F_scfg_StockIdFilter"];
                    foreach (DynamicObject obj in cols)
                    {
                        String tmpName = Convert.ToString(((DynamicObject)obj["F_scfg_StockIdFilter"])["Name"]);
                        tmpNameCol.Append(tmpName + "; ");
                    }

                    result.AddTitle("F_scfg_StockId", tmpNameCol.ToString());
                }
                else
                {
                    result.AddTitle("F_scfg_StockId", "全部仓库");
                }

                // 批号
                if (dyFilter["F_scfg_BatchFilter"] != null && ((DynamicObjectCollection)dyFilter["F_scfg_BatchFilter"]).Count > 0)
                {
                    StringBuilder tmpNameCol = new StringBuilder();
                    DynamicObjectCollection cols = (DynamicObjectCollection)dyFilter["F_scfg_BatchFilter"];
                    foreach (DynamicObject obj in cols)
                    {
                        String tmpName = Convert.ToString(((DynamicObject)obj["F_scfg_BatchFilter"])["Name"]);
                        tmpNameCol.Append(tmpName + "; ");
                    }

                    result.AddTitle("F_scfg_Batch", tmpNameCol.ToString());
                }
                else
                {
                    result.AddTitle("F_scfg_Batch", "全部批次");
                }
            }

            return result;
        }
    }
}