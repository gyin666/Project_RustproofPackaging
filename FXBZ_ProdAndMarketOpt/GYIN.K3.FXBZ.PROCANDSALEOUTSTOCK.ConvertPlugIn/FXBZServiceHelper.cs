﻿
using GYIN.K3.FXBZ.PROCANDSALEOUTSTOCK.Contracts;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.Operation;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GYIN.K3.FXBZ.PROCANDSALEOUTSTOCK.ServiceHelper
{
    public class FXBZServiceHelper
    {
        /// <summary>
        /// 暂存
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="FormID"></param>
        /// <param name="dyObject"></param>
        /// <returns></returns>
        public static IOperationResult Draft(Context ctx, string FormID, DynamicObject[] dyObject)
        {
            ICommonService service = ServiceFactory.GetService<ICommonService>(ctx);
           
            return service.DraftBill(ctx, FormID, dyObject);
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="FormID"></param>
        /// <param name="dyObject"></param>
        /// <returns></returns>
        public static IOperationResult Save(Context ctx, string FormID, DynamicObject[] dyObject)
        {
            ICommonService service = ServiceFactory.GetService<ICommonService>(ctx);
            IOperationResult saveResult = service.SaveBill(ctx, FormID, dyObject);
            return saveResult;
        }

        /// <summary>
        /// 另一种服务保存
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="FormID"></param>
        /// <param name="dyObject"></param>
        /// <returns></returns>
        public static IOperationResult BatchSave(Context ctx, string FormID, DynamicObject[] dyObject)
        {
            ICommonService service = ServiceFactory.GetService<ICommonService>(ctx);
            IOperationResult saveResult = service.BatchSaveBill(ctx, FormID, dyObject);
            return saveResult;
        }



        /// <summary>
        /// 业务对象提交
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="FormID">业务对象标识</param>
        /// <param name="ids">业务对象ID集合</param>
        /// <returns></returns>
        public static IOperationResult Submit(Context ctx, string formID, Object[] ids)
        {
            ICommonService service = ServiceFactory.GetService<ICommonService>(ctx);
            IOperationResult submitResult = service.SubmitBill(ctx, formID, ids);
            return submitResult;
        }



        /// <summary>
        /// 业务对象提交
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="FormID">业务对象标识</param>
        /// <param name="id">业务对象ID集合</param>
        /// <returns></returns>
        public static IOperationResult SubmitWorkFlow(Context ctx, string formID, string ids)
        {
            ICommonService service = ServiceFactory.GetService<ICommonService>(ctx);
            IOperationResult submitResult = service.SubmitWorkFlowBill(ctx, formID, ids);
            return submitResult;
        }


        /// <summary>
        /// 审核业务对象
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="formID">业务对象标识</param>
        /// <param name="ids">业务对象ID集合</param>
        /// <returns></returns>
        public static IOperationResult Audit(Context ctx, string formID, Object[] ids)
        {
            ICommonService service = ServiceFactory.GetService<ICommonService>(ctx);
            IOperationResult auditResult = service.AuditBill(ctx, formID, ids);
            return auditResult;
        }

        /// <summary>
        /// 业务对象状态转换
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="tableName"></param>
        /// <param name="fieldName"></param>
        /// <param name="fieldValue"></param>
        /// <param name="pkFieldName"></param>
        /// <param name="pkFieldValues"></param>
        public static void setState(Context ctx, string tableName, string fieldName, string fieldValue, string pkFieldName, object[] pkFieldValues)
        {
            ICommonService service = ServiceFactory.GetService<ICommonService>(ctx);
            service.setState(ctx, tableName, fieldName, fieldValue, pkFieldName, pkFieldValues);
        }
        /// <summary>
        /// 整单批量下推
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="SourceFormId"></param>
        /// <param name="TargetFormId"></param>
        /// <param name="sourceBillIds"></param>
        ///  <param name="TargetBillTypeId">目标单据类型</param>
        /// <returns></returns>
        public static ConvertOperationResult ConvertBills(Context ctx, string SourceFormId, string TargetFormId, List<long> sourceBillIds,String TargetBillTypeId, OperateOption operateOption)
        {
            ICommonService service = ServiceFactory.GetService<ICommonService>(ctx);
            return service.ConvertBills(ctx, SourceFormId, TargetFormId, sourceBillIds, TargetBillTypeId,   operateOption);
        }

        /// 整单带分录批量下推
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="SourceFormId"></param>
        /// <param name="TargetFormId"></param>
        /// <param name="sourceBillIds"></param>
        ///  <param name="TargetBillTypeId">目标单据类型</param>
        /// <returns></returns>
      //  public static DynamicObject[] ConvertBillsWithEntry(Context ctx, List<ConvertOption> option, string SourceFormId, string TargetFormId, string targetBillTypeId, string SourceEntryEntityKey, OperateOption operateOption)
      //  {
          //  ICommonService service = ServiceFactory.GetService<ICommonService>(ctx);
           // return service.ConvertBillsWithEntry(ctx,option,SourceFormId,TargetFormId,targetBillTypeId, SourceEntryEntityKey,operateOption);
     //   }
        /// <summary>
        /// 构建业务对象数据包
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="FormID">对象标识</param>
        /// <param name="fillBillPropertys">填充业务对象属性委托对象</param>
        /// <returns></returns>
        public static DynamicObject CreateBillMode(Context ctx, string FormID, Action<IDynamicFormViewService> fillBillPropertys)
        {
            ICommonService service = ServiceFactory.GetService<ICommonService>(ctx);
            DynamicObject model = service.installBillPackage(ctx, FormID, fillBillPropertys, "");
            return model;
        }
        public static Boolean isSendAllMaterial(Context ctx, string ContractNo, string orp)
        {
            ICommonService service = ServiceFactory.GetService<ICommonService>(ctx);
            return service.isSendAllMaterial(ctx, ContractNo,orp);
        }

        public static List<DateTime> getDateListByContract(Context ctx, string ContractNo, string o)
        {
            ICommonService service = ServiceFactory.GetService<ICommonService>(ctx);
            return service.getDateListByContract(ctx, ContractNo, o);
        }

        public static DynamicObject getContractObject(Context ctx, string billNo) {
            ICommonService service = ServiceFactory.GetService<ICommonService>(ctx);
            return service.getContractObject(ctx, billNo);
        }

        //private interface ICommonService
        //{
        //}
    }
}
