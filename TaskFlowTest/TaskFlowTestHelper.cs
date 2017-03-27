using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using DataEntities.Config;
using DataEntities.TaskFlowConfig;
using DataEntities.TaskFlowData;
using DevExpress.Data.Filtering;
using DevExpress.Images;
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

        public bool TestAndInitConnection(string sIp)
        {
            try
            {
                ServiceClient = CreateServcie(sIp);
                var bSuccess = ServiceClient.ConnectSuccess();
                if (!bSuccess) return false;
                XpoDefault.DataLayer = XpoDefault.GetDataLayer(ServiceClient.GetConnectionString(), AutoCreateOption.None);
                UnitOfWork = new UnitOfWork(XpoDefault.DataLayer);
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static ServiceClient CreateServcie(string sIp)
        {
            var sAddress = new StringBuilder();
            sAddress.Append("net.tcp://");
            sAddress.Append(sIp + ":1989");
            sAddress.Append("/IPSService");
            return new ServiceClient("NetTcpBinding_IService", sAddress.ToString());
        }

        #region 检查条件合法性
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
            foreach (var n in UnitOfWork.Query<TFCodeNode>().Where(n => !string.IsNullOrEmpty(n.s_PreCondition)).ToList())
            {
                var sErrorInfo = string.Empty;
                try
                {
                    if (CheckCondition(n.s_PreCondition)) continue;
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
                            codeTaskChain == null ? string.Empty : codeTaskChain.s_Code, n.s_Code, n.s_PreCondition);
                    }
                }
            }
            TestResultInfoSet.AddInfo("1. 完成检查：任务预产生条件是否合法。");

            TestResultInfoSet.AddInfo("2. 开始检查：任务完成条件是否合法......");
            foreach (var t in UnitOfWork.Query<TFCodeTask>().Where(t => !string.IsNullOrEmpty(t.s_FinishCondition)).ToList())
            {
                var sErrorInfo = string.Empty;
                try
                {
                    if (CheckCondition(t.s_FinishCondition)) continue;
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
                            codeNode == null ? string.Empty : codeNode.s_Code, t.s_FinishCondition);
                    }
                }
            }
            TestResultInfoSet.AddInfo("2. 完成检查：任务完成条件是否合法。");

            TestResultInfoSet.AddInfo("3. 开始检查：流向条件是否合法......");
            foreach (var nr in UnitOfWork.Query<TFCodeNodeRelation>().Where(nr => !string.IsNullOrEmpty(nr.s_Condition)).ToList())
            {
                var sErrorInfo = string.Empty;
                try
                {
                    if (CheckCondition(nr.s_Condition)) continue;
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
                            codeTaskChain == null ? string.Empty : codeTaskChain.s_Code, sTaskNo, nr.s_Condition);
                    }
                }
            }
            TestResultInfoSet.AddInfo("3. 完成检查：流向条件是否合法。");
        }
        #endregion

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
            var listTaskChainNum = new Hashtable();
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
                        listTaskChainNum.Add(operationInfo.sOperationReturnObject, conditionGroup);
                        TestResultInfoSet.AddInfo(
                            $"任务链创建成功！任务链：({operationInfo.sOperationReturnObject})；分支条件：{string.Join("; ", conditionGroup.Select(g => g.Key + ":" + g.Value).ToList())}",
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
                    listTaskChainNum.Add(operationInfo.sOperationReturnObject, null);
                    TestResultInfoSet.AddInfo($"任务链创建成功！任务链编号：{operationInfo.sOperationReturnObject}", operationInfo.sOperationReturnObject);
                }
            }
            TestResultInfoSet.AddInfo("3. 完成：创建所有分支组合的任务链集合。");

            TestResultInfoSet.AddInfo("4. 开始：逐一完成任务......");
            foreach (DictionaryEntry sTaskChainNum in listTaskChainNum)
            {
                FinishTask((string)sTaskChainNum.Key, (List<KeyValuePair<string, string>>)sTaskChainNum.Value);
            }
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
            try
            {
                var codeTaskChain = UnitOfWork.GetObjectByKey<TFCodeTaskChain>(gCodeTaskChainId);

                foreach (var t in codeTaskChain.GetlistCodeTasks())
                    foreach (var aik in t.GetListCodeActionInCodeTasks().Where(aik => aik.n_CodeActionID == 25).ToList())//TODO Hardcode
                    {
                        if (string.IsNullOrWhiteSpace(aik.s_ParamIn)) continue;
                        foreach (var sConditionCode in aik.s_ParamIn.Split(',').Where(s => !string.IsNullOrWhiteSpace(s)).ToList())
                        {
                            if (!htCustomCondition.Any(lk => lk.Select(k => k.Key).Contains(sConditionCode)))
                            {
                                var listKeyValuePair = new List<KeyValuePair<string, string>>();
                                foreach (
                                    var value in
                                    UnitOfWork.FindObject<TFCodeCustomCondition>(CriteriaOperator.Parse("s_Code = ?", sConditionCode))
                                        .s_Values.Split(',')
                                        .ToList())
                                    listKeyValuePair.Add(new KeyValuePair<string, string>(sConditionCode, value));
                                htCustomCondition.Add(listKeyValuePair);
                            }
                        }
                    }
                foreach (var c in codeTaskChain.GetListCodeTaskChains())
                    CodeTaskRecursion(c.g_ID, ref htCustomCondition);
            }
            catch (Exception e)
            {
                TestResultInfoSet.AddError("无法组织任务链自定义条件分支！请检查任务上自定义条件输入参数是否正确。");
                throw;
            }
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
            TFTask submitTask = null;
            try
            {
                submitTask =
    unitOfWork.FindObject<TFNode>(CriteriaOperator.Parse("g_TaskChainGuid = ? AND s_CodeNodeCode = ?",
        checkTask.GetTheTaskChain().g_ID, checkActionInTask.s_ParamIn)).GetTheOwnTask();
            }
            catch (Exception e)
            {
                TestResultInfoSet.AddError($"未找到指定的待审任务：{checkActionInTask.s_ParamIn}", string.Empty, string.Empty, e.ToString());
            }
            if (submitTask == null) return;
            var taskCheck = new TFTaskCheck(unitOfWork)
            {
                g_ID = Guid.NewGuid(),
                g_TaskID = submitTask.g_ID,
                n_Sequence = submitTask.GetListOthersTaskChecks().Max(c => c.n_Sequence) == null ? 1 : submitTask.GetListOthersTaskChecks().Max(c => c.n_Sequence) + 1,
                g_CheckTaskID = checkTask.g_ID,
                s_CheckReselt = submitTask.GetListOthersTaskChecks().Any() ? "Y" : "N"
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
            if (listCondition == null) return;
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

        private void FinishTask(string sTaskChainNum, List<KeyValuePair<string, string>> listCondition)
        {
            var listEmployee = UnitOfWork.Query<CodeEmployee>().Select(e => new { e.n_ID, e.s_Name, e.s_UserName });
            var listTeam = UnitOfWork.Query<CodeTeam>().Select(t => new { t.n_ID, t.s_Name });
            var nUserId = listEmployee.First(e => e.s_UserName != null && e.s_UserName.ToLower() == "administrator").n_ID;
            var gTaskChainId = Guid.Parse(UnitOfWork.ExecuteScalar($"select g_id from tf_taskchain where n_num = '{sTaskChainNum}'").ToString());
            var taskChain = UnitOfWork.GetObjectByKey<TFTaskChain>(gTaskChainId);
            if (taskChain == null)
            {
                TestResultInfoSet.AddError("未找到任务链！", sTaskChainNum);
                return;
            }
            var htCreatedTaskNums = new Hashtable();
            var unFinishTaskChains = new List<TFTaskChain>();
            unFinishTaskChains.Add(taskChain);
            var finishTaskChains = new UnitOfWork().GetObjectByKey<TFTaskChain>(taskChain.g_ID).GetListClusterTaskChains();
            var newCreatedTaskChains = finishTaskChains.Where(f => !unFinishTaskChains.Exists(u => u.g_ID == f.g_ID)).ToList();
            newCreatedTaskChains.ForEach(c => AddCustomCondition(c.n_Num.ToString(), listCondition));

            var listTasks =
                taskChain.GetListClusterTaskChains().SelectMany(c => c.GetListNodes()).Where(n => n.s_Mode != "P")
                    .Select(n => n.GetTheOwnTask())
                    .Where(t => t != null && (t.s_State == "P" || t.s_State == "O"))
                    .ToList();
            while (listTasks.Count > 0)
            {
                foreach (var task in listTasks)
                {
                    unFinishTaskChains = new UnitOfWork().GetObjectByKey<TFTaskChain>(taskChain.g_ID).GetListClusterTaskChains();
                    var sEndDateString = task.dt_EndDate <= new DateTime(1900, 1, 1) ? string.Empty : task.dt_EndDate.ToShortDateString();
                    var executor = listEmployee.FirstOrDefault(e => e.n_ID == task.n_ExecutorID);
                    var sExecutor = executor == null ? string.Empty : executor.s_Name;
                    var team = listTeam.FirstOrDefault(t => t.n_ID == task.n_ExecutePositionID);
                    var sTeam = team == null ? string.Empty : team.s_Name;

                    if (htCreatedTaskNums.ContainsKey(task.n_Num))
                    {
                        htCreatedTaskNums[task.n_Num] = Convert.ToInt32(htCreatedTaskNums[task.n_Num]) + 1;
                    }
                    else
                    {
                        htCreatedTaskNums.Add(task.n_Num, 1);
                    }
                    if (Convert.ToInt32(htCreatedTaskNums[task.n_Num]) >= 3)
                    {
                        TestResultInfoSet.AddWarning($"完成任务失败！原因：该任务被重复打开了{htCreatedTaskNums[task.n_Num]}次", $"({sTaskChainNum}){taskChain.s_Name}", $"({task.n_Num}){task.s_Name}", $"结束日期：{sEndDateString} 执行人：{sExecutor} 执行岗位：{sTeam}");
                        return;
                    }
                    AddTaskCheckParameter(task);
                    var tfNode = task.GetTheBelongNode();
                    var operationInfo = ServiceClient.ByServerFinishTaskNode(tfNode.g_ID, nUserId);
                    var sCondtion = CalUtility.ConvertToCondition(tfNode.GetTheCodeNode().GetTheOwnCodeTask().s_FinishCondition);
                    if (!operationInfo.bOperationResult)
                    {
                        TestResultInfoSet.AddError($"完成任务失败！原因：{operationInfo.sOperationMessage} 完成条件：{sCondtion}", $"({sTaskChainNum}){taskChain.s_Name}", $"({task.n_Num}){task.s_Name}", $"结束日期：{sEndDateString} 执行人：{sExecutor} 执行岗位：{sTeam}");
                        return;
                    }
                    if (!string.IsNullOrEmpty(operationInfo.sOperationReturnObject.Replace("\"", "").Trim()))
                    {
                        TestResultInfoSet.AddError($"完成任务失败！原因：{operationInfo.sOperationReturnObject} 完成条件：{sCondtion}", $"({sTaskChainNum}){taskChain.s_Name}", $"({task.n_Num}){task.s_Name}", $"结束日期：{sEndDateString} 执行人：{sExecutor} 执行岗位：{sTeam}");
                        return;
                    }
                    var sInfo = string.Empty;
                    if (string.IsNullOrEmpty(sExecutor) && string.IsNullOrEmpty(sTeam))
                    {
                        sInfo += " 该任务没有执行人。";
                    }
                    if (string.IsNullOrEmpty(sEndDateString))
                    {
                        sInfo += "该任务没有结束日期。";
                    }
                    if (sInfo != string.Empty)
                    {
                        TestResultInfoSet.AddWarning($"任务完成！{sInfo}", $"({sTaskChainNum}){taskChain.s_Name}", $"({task.n_Num}){task.s_Name}", $"结束日期：{sEndDateString} 执行人：{sExecutor} 执行岗位：{sTeam}");
                    }
                    else
                    {
                        TestResultInfoSet.AddInfo("任务完成！", $"({sTaskChainNum}){taskChain.s_Name}", $"({task.n_Num}){task.s_Name}", $"结束日期：{sEndDateString} 执行人：{sExecutor} 执行岗位：{sTeam}");
                    }
                    finishTaskChains = new UnitOfWork().GetObjectByKey<TFTaskChain>(taskChain.g_ID).GetListClusterTaskChains();
                    newCreatedTaskChains = finishTaskChains.Where(f => !unFinishTaskChains.Exists(u => u.g_ID == f.g_ID)).ToList();
                    newCreatedTaskChains.ForEach(c => AddCustomCondition(c.n_Num.ToString(), listCondition));
                }

                listTasks = new UnitOfWork().GetObjectByKey<TFTaskChain>(taskChain.g_ID).GetListClusterTaskChains().SelectMany(c => c.GetListNodes()).Where(n => n.s_Mode != "P")
                        .Select(n => n.GetTheOwnTask())
                        .Where(t => t != null && (t.s_State == "P" || t.s_State == "O"))
                        .ToList();
            }
        }

        #endregion
    }
}