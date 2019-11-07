using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.Operation;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Rpc;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace GYIN.K3.FXBZ.PROCANDSALEOUTSTOCK.Contracts
{
    /// <summary>
    /// 服务契约
    /// </summary>
    [RpcServiceError]
    [ServiceContract]
    public interface ICommonService
    {
        /// <summary>
        /// 暂存单据
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="FormID"></param>
        /// <param name="dyObject"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        IOperationResult DraftBill(Context ctx, string FormID, DynamicObject[] dyObject);

        /// <summary>
        /// 保存单据
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="FormID"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        IOperationResult SaveBill(Context ctx, string FormID, DynamicObject[] dyObject);

        /// <summary>
        /// 批量保存单据(另外一种保存服务的调用)
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="FormID"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        IOperationResult BatchSaveBill(Context ctx, string FormID, DynamicObject[] dyObject);

        /// <summary>
        /// 提交单据
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="FormID"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        IOperationResult SubmitBill(Context ctx, string FormID, object[] ids);


        /// <summary>
        /// 提交单据到工作流
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="FormID"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        IOperationResult SubmitWorkFlowBill(Context ctx, string FormID, string id);



        /// <summary>
        /// 审核单据
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="FormID"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        IOperationResult AuditBill(Context ctx, string FormID, object[] ids);


        /// <summary>
        /// 转换业务对象装填
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="tableName">状态字段所在物理表名</param>
        /// <param name="fieldName">状态字段名</param>
        /// <param name="fieldValue">要转换成的状态值</param>
        /// <param name="pkFieldName">物料表主键列名</param>
        /// <param name="pkFieldValues">主键值集合</param>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void setState(Context ctx, string tableName, string fieldName, string fieldValue, string pkFieldName, object[] pkFieldValues);





        /// <summary>
        /// 下推 按照单据内码ID集合
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="SourceFormId"></param>
        /// <param name="TargetFormId"></param>
        /// <param name="sourceBillIds"></param>
        ///  <param name="TargetBillTypeId">目标单据类型</param>
        ///  <param name="operateOption">携带参数</param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ConvertOperationResult ConvertBills(Context ctx, string SourceFormId, string TargetFormId, List<long> sourceBillIds,String targetBillTypeId, OperateOption operateOption);



        /// <summary>
        /// 构建单据数据包
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="FormID">业务对象标识</param>
        /// <param name="fillBillPropertys">填写单据内容</param>
        /// <param name="BillTypeId">单据类型ID</param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        DynamicObject installBillPackage(Context ctx, string FormID, Action<IDynamicFormViewService> fillBillPropertys, string BillTypeId);

        /// <summary>
        /// 根据合同号判断是否到全货
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="ContractNo">合同号</param>
        /// <param name="orp">是否反写采购合同到全日期，默认 0,1为反写</param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        Boolean isSendAllMaterial(Context ctx, string ContractNo,string orp);

        /// <summary>
        /// 根据合同号找到最后入库、收货通知单日期日期
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="ContractNo">合同号</param>
        /// <param name="o"></param>
        /// <returns>List list(0) 是入库单审核日期，list(1) 收入通知单审核日期</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        List<DateTime> getDateListByContract(Context ctx, string ContractNo, string o);
        DynamicObject getContractObject(Context ctx, string billNo);

        /// <summary>
        /// 后台调用单据转换生成目标单
        /// </summary>
        /// <param name="ctx">上下文</param>
        /// <param name="option">单据转换参数</param>
        /// <param name="SourceFormId">原单据</param>
        /// <param name="TargetFormId">目标单据</param>
        /// <param name="targetBillTypeId">目标单据类型</param>
        /// <param name="SourceEntryEntityKey">分录标识</param>
        /// <returns></returns>
    //  [OperationContract]
    // [FaultContract(typeof(ServiceFault))]
    // DynamicObject[] ConvertBillsWithEntry(Context ctx, List<ConvertOption> option, string SourceFormId, string TargetFormId, String targetBillTypeId,string SourceEntryEntityKey, OperateOption operateOption);
    }
}
