using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataEntities.Case;
using DataEntities.TaskFlowConfig;
using DataEntities.TaskFlowData;
using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using TaskFlowTest.CalculateMethod;
using TaskFlowTest.Service_References.IPSPServices;
using TaskFlowTest.Utils;

namespace TaskFlowTest
{
    public class TaskFlowTestHelper
    {
        public TestResultInfoSet TestResultInfoSet { get; }

        private UnitOfWork _unitOfWork;

        private ServiceClient _serviceClient;

        public TaskFlowTestHelper(TestResultInfoSet testResultInfo)
        {
            TestResultInfoSet = testResultInfo;
            _unitOfWork = new UnitOfWork();
        }

        private bool CheckCondition(string sExpression)
        {
            bool bResult = true;
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
            _unitOfWork.Query<TFCodeNode>().Where(n => !string.IsNullOrEmpty(n.s_PreCondition)).ToList().ForEach(n =>
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
            _unitOfWork.Query<TFCodeTask>().Where(t => !string.IsNullOrEmpty(t.s_FinishCondition)).ToList().ForEach(t =>
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
            _unitOfWork.Query<TFCodeNodeRelation>().Where(nr => !string.IsNullOrEmpty(nr.s_Condition)).ToList().ForEach(nr =>
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
                            {
                                sNodeCodeFrom = "开始";
                            }
                        }
                        var sNodeCodeTo = string.Empty;
                        if (codeNodeTo != null)
                        {
                            sNodeCodeTo = codeNodeTo.s_Code;
                            if (codeNodeTo.s_Type == Enums.NodeType.E.ToString())
                            {
                                sNodeCodeTo = "截止";
                            }
                        }

                        var sTaskNo = $"{sNodeCodeFrom} ----> {sNodeCodeTo}";
                        TestResultInfoSet.AddError(sErrorInfo,
                            codeTaskChain == null ? string.Empty : codeTaskChain.s_Code, sTaskNo);
                    }
                }


            });
            TestResultInfoSet.AddInfo("3. 完成检查：流向条件是否合法。");
        }
        public void TestCustomCondtion()
        {

        }
        #region 模拟流程
        public void GenerateTaskChain()
        {
            _serviceClient = new ServiceClient();
            //TriggerType: CaseSubmit, Case, CameFileOfficial
            //TriggerObjectFullName: DataEntities.Case.BasicCase
            var testCase = _unitOfWork.GetObjectByKey<BasicCase>(1);

            var htCustomCondition = new List<List<KeyValuePair<string, string>>>();
            CodeTaskRecursion(Guid.Empty, ref htCustomCondition);

            var result = new List<List<KeyValuePair<string, string>>>();
            GetFlowCustomCondtion(htCustomCondition[0], htCustomCondition[1], 1, htCustomCondition, ref result);

            foreach (var conditionGroup in result)
            {
                _serviceClient.ByServerAutoGenerateTopTaskChains("Case", "DataEntities.Case.BasicCase", 1, true);
            }
        }

        private void GetFlowCustomCondtion(List<KeyValuePair<string, string>> listValue1, List<KeyValuePair<string, string>> listValue2, int nIndex, List<List<KeyValuePair<string, string>>> htCustomCondition, ref List<List<KeyValuePair<string, string>>> result)
        {
            if (nIndex < htCustomCondition.Count)
            {
                result = new List<List<KeyValuePair<string, string>>>();
                for (int i = 0; i < listValue1.Count; i++)
                {
                    for (int j = 0; j < listValue2.Count; i++)
                    {
                        var listKeyValuePair = new List<KeyValuePair<string, string>>();
                        listKeyValuePair.Add(new KeyValuePair<string, string>(listValue1[i].Key, listValue1[i].Value));
                        listKeyValuePair.Add(new KeyValuePair<string, string>(listValue2[j].Key, listValue2[j].Value));
                        result.Add(listKeyValuePair);
                    }
                }

                GetFlowCustomCondtion(result[result.Count - 1], htCustomCondition[nIndex + 1], nIndex + 1, htCustomCondition, ref result);
            }
        }

        private void CodeTaskRecursion(Guid gCodeTaskChainID, ref List<List<KeyValuePair<string, string>>> htCustomCondition)
        {
            var codeTaskChain = _unitOfWork.GetObjectByKey<TFCodeTaskChain>(gCodeTaskChainID);

            foreach (var t in codeTaskChain.GetlistCodeTasks())
            {
                foreach (var aik in t.ListCodeActionInCodeTasks.Where(aik => aik.n_CodeActionID == 25).ToList())
                {
                    if (!htCustomCondition.Any(lk => lk.Select(k => k.Key).Contains(aik.s_ParamIn)))
                    {
                        var listKeyValuePair = new List<KeyValuePair<string, string>>();
                        foreach (var value in _unitOfWork.FindObject<TFCodeCustomCondition>(CriteriaOperator.Parse("s_Code = ?", aik.s_ParamIn)).s_Values.Split(',').ToList())
                        {
                            listKeyValuePair.Add(new KeyValuePair<string, string>(aik.s_ParamIn, value));
                        }
                        htCustomCondition.Add(listKeyValuePair);
                    }
                }
            }
            foreach (var c in codeTaskChain.GetListCodeTaskChains())
            {
                CodeTaskRecursion(c.g_ID, ref htCustomCondition);
            }
        }

        private void FinishTaskRecursion(int nCaseID)
        {
            var basicCase = new UnitOfWork().GetObjectByKey<BasicCase>(nCaseID);
            var listTaskChains = basicCase.GetRelateTaskChains();
            var listTasks = listTaskChains.SelectMany(c => c.GetListNodes().Select(n => n.GetTheOwnTask())).Where(t => t.s_State == "P" || t.s_State == "O").ToList();
            while (listTasks.Count > 0)
            {
                foreach (var task in listTasks)
                {
                    var tfNode = task.GetTheBelongNode();
                    var sFinishCondition = tfNode.GetTheCodeNode().GetTheOwnCodeTask().s_FinishCondition;
                    if (sFinishCondition.Contains("CTaskExistParamTye"))
                    {
                        if (task.GetListTaskParameters().All(p => p.s_ParaType != "TaskCheck"))
                        {
                            var bResultN = task.GetListItselfTaskChecks().Any(c => c.s_CheckReselt == "N");
                            //TODO 找到被审任务ID
                            var taskCheck = new TFTaskCheck(_unitOfWork)
                            {
                                g_ID = Guid.NewGuid(),
                                g_TaskID = task.g_ID,
                                n_Sequence = task.GetListItselfTaskChecks().Max(c => c.n_Sequence) + 1,
                                g_CheckTaskID = task.g_ID,
                                s_CheckReselt = bResultN ? "Y" : "N"
                            };
                            var taskParameter = new TFTaskParameter(_unitOfWork)
                            {
                                g_ID = Guid.NewGuid(),
                                g_TaskID = task.g_ID,
                                g_TaskChainID = task.GetTheTaskChain().g_ID,
                                n_CodeActionID = 6,
                                s_ParaType = "TaskCheck",
                                s_ParaValue = taskCheck.g_ID.ToString()
                            };
                            taskParameter.Save();
                            _unitOfWork.CommitChanges();
                        }
                    }
                    var operationInfo = _serviceClient.ByServerFinishTaskNode(tfNode.g_ID, 131);
                    if (!operationInfo.bOperationResult)
                    {
                        //TODO： 输出信息
                        return;
                    }

                }
            }
        }
        #endregion
    }
}
