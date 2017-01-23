using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataEntities.Config;
using DataEntities.TaskFlowConfig;
using DataEntities.TaskFlowData;
using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using TaskFlowTest.CalculateMethod;
using TaskFlowTest.Service_References.IPSPServices;
using TaskFlowTest.Utils;

namespace TaskFlowTest
{
    public class TaskFlowTestHelper
    {
        public TaskFlowTestHelper(TestResultInfoSet testResultInfo)
        {
            TestResultInfoSet = testResultInfo;
        }

        public UnitOfWork UnitOfWork { get; set; }

        public TestResultInfoSet TestResultInfoSet { get; }

        public ServiceClient ServiceClient { get; private set; }

        public bool TestAndInitConnection(string sIpAndPort)
        {
            try
            {
                var sAddress = new StringBuilder();
                sAddress.Append("net.tcp://");
                sAddress.Append(sIpAndPort);
                sAddress.Append("/IPSService");
                ServiceClient = new ServiceClient("NetTcpBinding_IService", sAddress.ToString());
                var bSuccess = ServiceClient.ConnectSuccess();
                if (bSuccess)
                {
                    XpoDefault.DataLayer = XpoDefault.GetDataLayer(ServiceClient.GetConnectionString(),
                        AutoCreateOption.SchemaAlreadyExists);
                    UnitOfWork = new UnitOfWork(XpoDefault.DataLayer);
                }
                return bSuccess;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool CheckCondition(string sExpression)
        {
            var bResult = true;
            if (string.IsNullOrEmpty(sExpression.Trim())) return true;
            try
            {
                Calculate.CheckCondition(sExpression);
            }
            catch (Exception ex)
            {
                bResult = false;
            }
            return bResult;
        }

        public void TestCondition()
        {
            TestResultInfoSet.AddInfo("1. 开始检查：任务预产生条件是否合法......");
            UnitOfWork.Query<TFCodeNode>().Where(n => !string.IsNullOrEmpty(n.s_PreCondition)).ToList().ForEach(n =>
            {
                var sErrorInfo = string.Empty;
                try
                {
                    if (CheckCondition(n.s_PreCondition)) return;
                    sErrorInfo = $"任务预产生条件不合法！表达式：{CalUtility.ConvertToCondition(n.s_PreCondition)}";
                }
                catch (Exception e)
                {
                    sErrorInfo = e.Message + "\r\n" + e.Source + "\r\n" + e.StackTrace;
                }
                finally
                {
                    if (!string.IsNullOrEmpty(sErrorInfo))
                    {
                        var codeTaskChain = n.GetTheCodeTaskChain();
                        TestResultInfoSet.AddError(sErrorInfo,
                            codeTaskChain == null ? string.Empty : codeTaskChain.s_Code, n.s_Code);
                    }
                }
            });
            TestResultInfoSet.AddInfo("1. 完成检查：任务预产生条件是否合法。");

            TestResultInfoSet.AddInfo("2. 开始检查：任务完成条件是否合法......");
            UnitOfWork.Query<TFCodeTask>().Where(t => !string.IsNullOrEmpty(t.s_FinishCondition)).ToList().ForEach(t =>
            {
                var sErrorInfo = string.Empty;
                try
                {
                    if (CheckCondition(t.s_FinishCondition)) return;
                    sErrorInfo = $"任务完成条件不合法！表达式：{CalUtility.ConvertToCondition(t.s_FinishCondition)}";
                }
                catch (Exception e)
                {
                    sErrorInfo = e.Message + "\r\n" + e.Source + "\r\n" + e.StackTrace;
                }
                finally
                {
                    if (!string.IsNullOrEmpty(sErrorInfo))
                    {
                        var codeNode = t.GetTheBelongCodeNode();
                        var codeTaskChain = t.GetTheCodeTaskChain();
                        TestResultInfoSet.AddError(sErrorInfo,
                            codeTaskChain == null ? string.Empty : codeTaskChain.s_Code,
                            codeNode == null ? string.Empty : codeNode.s_Code);
                    }
                }
            });
            TestResultInfoSet.AddInfo("2. 完成检查：任务完成条件是否合法。");

            TestResultInfoSet.AddInfo("3. 开始检查：流向条件是否合法......");
            UnitOfWork.Query<TFCodeNodeRelation>()
                .Where(nr => !string.IsNullOrEmpty(nr.s_Condition))
                .ToList()
                .ForEach(nr =>
                {
                    var sErrorInfo = string.Empty;
                    try
                    {
                        if (CheckCondition(nr.s_Condition)) return;
                        sErrorInfo = $"流向条件不合法！表达式：{CalUtility.ConvertToCondition(nr.s_Condition)}";
                    }
                    catch (Exception e)
                    {
                        sErrorInfo = e.Message + "\r\n" + e.Source + "\r\n" + e.StackTrace;
                    }
                    finally
                    {
                        if (!string.IsNullOrEmpty(sErrorInfo))
                        {
                            var codeNodeFrom = nr.GetTheCodeNodeFrom();
                            var codeNodeTo = nr.GetTheCodeNodeTo();
                            TFCodeTaskChain codeTaskChain = null;
                            if (codeNodeFrom != null)
                                codeTaskChain = codeNodeFrom.GetTheCodeTaskChain();
                            if (codeNodeTo != null && codeTaskChain == null)
                                codeTaskChain = codeNodeTo.GetTheCodeTaskChain();
                            var sNodeCodeFrom = string.Empty;
                            if (codeNodeFrom != null)
                            {
                                sNodeCodeFrom = codeNodeFrom.s_Code;
                                if (codeNodeFrom.s_Type == Enums.NodeType.S.ToString())
                                    sNodeCodeFrom = "开始";
                            }
                            var sNodeCodeTo = string.Empty;
                            if (codeNodeTo != null)
                            {
                                sNodeCodeTo = codeNodeTo.s_Code;
                                if (codeNodeTo.s_Type == Enums.NodeType.E.ToString())
                                    sNodeCodeTo = "截止";
                            }

                            var sTaskNo = $"{sNodeCodeFrom} ----> {sNodeCodeTo}";
                            TestResultInfoSet.AddError(sErrorInfo,
                                codeTaskChain == null ? string.Empty : codeTaskChain.s_Code, sTaskNo);
                        }
                    }
                });
            TestResultInfoSet.AddInfo("3. 完成检查：流向条件是否合法。");
        }

        #region 模拟流程

        public void ExecuteTaskChain(Guid gCodeTaskChainGuid, string sRelatedObjectTypeName, string sRelatedObjectId)
        {
            TestResultInfoSet.AddInfo("1. 开始：分析任务链自定义条件分支......");
            var htCustomCondition = new List<List<KeyValuePair<string, string>>>();
            CodeTaskRecursion(gCodeTaskChainGuid, ref htCustomCondition);
            TestResultInfoSet.AddInfo("1. 完成：分析任务链自定义条件分支。");

            TestResultInfoSet.AddInfo("2. 开始：组织任务链所有分支条件流程......");

            var result = GetFlowCustomCondtion(htCustomCondition);
            TestResultInfoSet.AddInfo($"2. 完成：组织任务链所有分支条件流程。共有{result.Count}条分支");

            TestResultInfoSet.AddInfo("3. 开始：创建所有分支组合的任务链集合......");
            var listTaskChainNum = new List<string>();
            if (result.Count > 0)
            {
                foreach (var conditionGroup in result)
                {
                    var operationInfo = ServiceClient.ByServerManualGenerateTopTaskChain(gCodeTaskChainGuid,
                        sRelatedObjectTypeName, Convert.ToInt32(sRelatedObjectId), true);
                    if (!operationInfo.bOperationResult)
                    {
                        TestResultInfoSet.AddError($"任务链创建失败！原因：{operationInfo.sOperationMessage}");
                    }
                    else
                    {
                        AddCustomCondition(operationInfo.sOperationReturnObject, conditionGroup);
                        listTaskChainNum.Add(operationInfo.sOperationReturnObject);
                        TestResultInfoSet.AddInfo(
                            $"任务链创建成功！任务链编号：{operationInfo.sOperationReturnObject}；分支条件：{string.Join("; ", conditionGroup.Select(g => g.Key + ":" + g.Value).ToList())}",
                            operationInfo.sOperationReturnObject);
                    }
                }
            }
            else
            {
                var operationInfo = ServiceClient.ByServerManualGenerateTopTaskChain(gCodeTaskChainGuid,
                    sRelatedObjectTypeName, Convert.ToInt32(sRelatedObjectId), true);
                if (!operationInfo.bOperationResult)
                {
                    TestResultInfoSet.AddError($"任务链创建失败！原因：{operationInfo.sOperationMessage}");
                }
                else
                {
                    listTaskChainNum.Add(operationInfo.sOperationReturnObject);
                    TestResultInfoSet.AddInfo($"任务链创建成功！任务链编号：{operationInfo.sOperationReturnObject}", operationInfo.sOperationReturnObject);
                }
            }
            TestResultInfoSet.AddInfo("3. 完成：创建所有分支组合的任务链集合。");

            TestResultInfoSet.AddInfo("4. 开始：逐一完成任务......");
            foreach (var sTaskChainNum in listTaskChainNum)
                FinishTask(sTaskChainNum);
            TestResultInfoSet.AddInfo("4. 完成：逐一完成任务。");
        }

        private List<List<KeyValuePair<string, string>>> GetFlowCustomCondtion(List<List<KeyValuePair<string, string>>> htCustomCondition)
        {
            var listListExeFlow = new List<List<KeyValuePair<string, string>>>();
            if (htCustomCondition.Count >= 1)
                foreach (var keyValuePair in htCustomCondition[0])
                    listListExeFlow.Add(new List<KeyValuePair<string, string>> { keyValuePair });

            if (htCustomCondition.Count > 1)
            {
                foreach (var listkvStep in htCustomCondition)
                {
                    if (listkvStep == htCustomCondition[0]) continue;
                    var newResult = new List<List<KeyValuePair<string, string>>>();
                    foreach (var kvStep in listkvStep)
                    {
                        foreach (var listExeFlow in listListExeFlow)
                        {
                            var newExeFlow = new List<KeyValuePair<string, string>>();
                            newExeFlow.AddRange(listExeFlow);
                            newExeFlow.Add(new KeyValuePair<string, string>(kvStep.Key, kvStep.Value));
                            newResult.Add(newExeFlow);
                        }
                    }
                    listListExeFlow = newResult;
                }
            }
            return listListExeFlow;
        }

        private void CodeTaskRecursion(Guid gCodeTaskChainId,
            ref List<List<KeyValuePair<string, string>>> htCustomCondition)
        {
            var codeTaskChain = UnitOfWork.GetObjectByKey<TFCodeTaskChain>(gCodeTaskChainId);

            foreach (var t in codeTaskChain.GetlistCodeTasks())
                foreach (var aik in t.GetListCodeActionInCodeTasks().Where(aik => aik.n_CodeActionID == 25).ToList())
                    if (!htCustomCondition.Any(lk => lk.Select(k => k.Key).Contains(aik.s_ParamIn)))
                    {
                        var listKeyValuePair = new List<KeyValuePair<string, string>>();
                        foreach (
                            var value in
                            UnitOfWork.FindObject<TFCodeCustomCondition>(CriteriaOperator.Parse("s_Code = ?", aik.s_ParamIn))
                                .s_Values.Split(',')
                                .ToList())
                            listKeyValuePair.Add(new KeyValuePair<string, string>(aik.s_ParamIn, value));
                        htCustomCondition.Add(listKeyValuePair);
                    }
            foreach (var c in codeTaskChain.GetListCodeTaskChains())
                CodeTaskRecursion(c.g_ID, ref htCustomCondition);
        }

        private void AddTaskCheckParameter(TFTask checkTask)
        {
            var unitOfWork = (UnitOfWork)checkTask.Session;
            var codeCheckAction =
                unitOfWork.FindObject<TFCodeAction>(CriteriaOperator.Parse("s_FuncNameSpace = ?",
                    "TaskFlowAction.ActionMethod.MethodCheck"));
            if (codeCheckAction == null) return;
            var checkActionInTask =
                checkTask.GetListCodeActionInTasks().FirstOrDefault(e => e.n_CodeActionID == codeCheckAction.n_ID);
            if (checkActionInTask == null) return;
            var submitTask =
                unitOfWork.FindObject<TFNode>(CriteriaOperator.Parse("g_TaskChainGuid = ? AND s_CodeNodeCode = ?",
                    checkTask.GetTheTaskChain().g_ID, checkActionInTask.s_ParamIn)).GetTheOwnTask();
            if (submitTask == null) return;
            var taskParameters = submitTask.GetListTaskParameters(Enums.TaskParameterType.TaskCheck.ToString());
            var taskCheck = new TFTaskCheck(unitOfWork)
            {
                g_ID = Guid.NewGuid(),
                g_TaskID = submitTask.g_ID,
                n_Sequence = submitTask.GetListItselfTaskChecks().Max(c => c.n_Sequence) + 1,
                g_CheckTaskID = checkTask.g_ID,
                s_CheckReselt = taskParameters.Any() ? "Y" : "N"
            };

            var taskParameter = new TFTaskParameter(unitOfWork)
            {
                g_ID = Guid.NewGuid(),
                g_TaskID = checkTask.g_ID,
                g_TaskChainID = checkTask.GetTheTaskChain().g_ID,
                n_CodeActionID = checkActionInTask.n_CodeActionID,
                s_ParaType = TaskParameterType.TaskCheck.ToString(),
                s_ParaValue = taskCheck.g_ID.ToString()
            };
            taskParameter.Save();
            unitOfWork.CommitChanges();
        }

        private void AddCustomCondition(string sNum, List<KeyValuePair<string, string>> listCondition)
        {
            var gTaskChainId = Guid.Parse(UnitOfWork.ExecuteScalar($"select g_id from tf_taskchain where n_num = '{sNum}'").ToString());
            var taskChain = UnitOfWork.GetObjectByKey<TFTaskChain>(gTaskChainId);
            var unitOfWork = (UnitOfWork)taskChain.Session;
            foreach (var keyValuePair in listCondition)
            {
                var customConditionInTaskChain = new TFCustomConditionInTaskChain(unitOfWork)
                {
                    g_ID = Guid.NewGuid(),
                    n_CustomConditionID =
                        unitOfWork.FindObject<TFCodeCustomCondition>(CriteriaOperator.Parse("s_Code = ?",
                            keyValuePair.Key)).n_ID,
                    g_TaskChainID = taskChain.g_ID,
                    s_Values = keyValuePair.Value
                };
                customConditionInTaskChain.Save();
            }
            unitOfWork.CommitChanges();
        }

        private void FinishTask(string sTaskChainNum)
        {
            var nUserId = UnitOfWork.FindObject<CodeEmployee>(CriteriaOperator.Parse("s_UserName = ?", "administrator")).n_ID;
            var gTaskChainId = Guid.Parse(UnitOfWork.ExecuteScalar($"select g_id from tf_taskchain where n_num = '{sTaskChainNum}'").ToString());
            var taskChain = UnitOfWork.GetObjectByKey<TFTaskChain>(gTaskChainId);
            if (taskChain == null)
            {
                TestResultInfoSet.AddError("未找到任务链！", sTaskChainNum);
                return;
            }
            var listTasks =
                taskChain.GetListNodes()
                    .Select(n => n.GetTheOwnTask())
                    .Where(t => t != null && (t.s_State == "P" || t.s_State == "O"))
                    .ToList();
            while (listTasks.Count > 0)
            {
                foreach (var task in listTasks)
                {
                    AddTaskCheckParameter(task);
                    var tfNode = task.GetTheBelongNode();
                    var operationInfo = ServiceClient.ByServerFinishTaskNode(tfNode.g_ID, nUserId);
                    if (!operationInfo.bOperationResult)
                    {
                        var sCondtion = CalUtility.ConvertToCondition(tfNode.GetTheCodeNode().GetTheOwnCodeTask().s_FinishCondition);
                        TestResultInfoSet.AddError($"完成任务失败！原因：{operationInfo.sOperationMessage}；完成任务条件：{sCondtion}", sTaskChainNum, task.n_Num.ToString());
                        return;
                    }
                    else
                    {
                        TestResultInfoSet.AddInfo($"任务完成！完成信息：{operationInfo.sOperationReturnObject}", sTaskChainNum, task.n_Num.ToString());
                    }

                }
                listTasks = new UnitOfWork().GetObjectByKey<TFTaskChain>(taskChain.g_ID).GetListNodes()
                        .Select(n => n.GetTheOwnTask())
                        .Where(t => t != null && (t.s_State == "P" || t.s_State == "O"))
                        .ToList();
            }
        }

        #endregion
    }
}