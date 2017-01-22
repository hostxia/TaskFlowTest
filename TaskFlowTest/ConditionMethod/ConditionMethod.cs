using System;
using System.Linq;
using DataEntities.TaskFlowConfig;
using DataEntities.TaskFlowData;
using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using TaskFlowTest.Utils;

namespace TaskFlowTest.ConditionMethod
{
    public class CTrue : IConditionTask
    {
        public bool TestCondition(string sConditionParameter)
        {
            return true;
        }

        public bool Condition(TFTask task, TFTaskChain taskChain, string sConditionParameter)
        {
            return true;
        }
    }

    public class CFalse : IConditionTask
    {
        public bool TestCondition(string sConditionParameter)
        {
            return true;
        }

        public bool Condition(TFTask task, TFTaskChain taskChain, string sConditionParameter)
        {
            return false;
        }
    }

    public class CNodeMode : IConditionTask
    {
        public bool TestCondition(string sConditionParameter)
        {
            var sArray = sConditionParameter.Split(',');
            if (sArray.Count() != 2) throw new Exception();
            var sMode = sArray[1];
            if (sMode != "正常" && sMode != "预产生") throw new Exception();
            return true;
        }

        public bool Condition(TFTask task, TFTaskChain taskChain, string sConditionParameter)
        {
            var sArray = sConditionParameter.Split(',');
            var sNodeCode = sArray[0];
            var sNodeMode = sArray[1];
            var node = taskChain.GetListNodes().FirstOrDefault(n => n.s_CodeNodeCode == sNodeCode);
            if (node == null) return false;
            if (sNodeMode == "正常" && node.s_Mode == "N") return true;
            if (sNodeMode == "预产生" && node.s_Mode == "P") return true;
            return false;
        }
    }

    public class CTaskState : IConditionTask
    {
        public bool TestCondition(string sConditionParameter)
        {
            var sArray = sConditionParameter.Split(',');
            if (sArray.Count() != 2) throw new Exception();
            var sState = sArray[1];
            if (sState != "未完成" && sState != "完成" && sState != "关闭" && sState != "打开") throw new Exception();
            return true;
        }

        public bool Condition(TFTask task, TFTaskChain taskChain, string sConditionParameter)
        {
            var sArray = sConditionParameter.Split(',');
            var sNodeCode = sArray[0];
            var sTaskState = sArray[1];
            var node = taskChain.GetListNodes().FirstOrDefault(n => n.s_CodeNodeCode == sNodeCode);
            if (node == null || node.s_Type != Enums.NodeType.T.ToString()) return false;
            var nodeTask = node.GetTheOwnTask();
            if (sTaskState == "未完成" && nodeTask.s_State == "P") return true;
            if (sTaskState == "完成" && nodeTask.s_State == "F") return true;
            if (sTaskState == "关闭" && nodeTask.s_State == "C") return true;
            if (sTaskState == "打开" && nodeTask.s_State == "O") return true;
            return false;
        }
    }

    public class CTaskChainState : IConditionTask
    {
        public bool TestCondition(string sConditionParameter)
        {
            var sArray = sConditionParameter.Split(',');
            if (sArray.Count() != 2) throw new Exception();
            var sState = sArray[1];
            if (sState != "未完成" && sState != "完成" && sState != "关闭" && sState != "打开") throw new Exception();
            return true;
        }

        public bool Condition(TFTask task, TFTaskChain taskChain, string sConditionParameter)
        {
            var sArray = sConditionParameter.Split(',');
            var sNodeCode = sArray[0];
            var sTaskChainState = sArray[1];
            var node = taskChain.GetListNodes().FirstOrDefault(n => n.s_CodeNodeCode == sNodeCode);
            if (node == null || node.s_Type != Enums.NodeType.T.ToString()) return false;
            var nodeTaskChain = node.GetTheOwnTaskChain();
            if (sTaskChainState == "未完成" && nodeTaskChain.s_State == "P") return true;
            if (sTaskChainState == "完成" && nodeTaskChain.s_State == "F") return true;
            if (sTaskChainState == "关闭" && nodeTaskChain.s_State == "C") return true;
            if (sTaskChainState == "打开" && nodeTaskChain.s_State == "O") return true;
            return false;
        }
    }

    public class CTaskCheckResult : IConditionTask
    {
        public bool TestCondition(string sConditionParameter)
        {
            var ss = sConditionParameter.Split(',');
            if (ss.Length < 2 || (ss[1] != "审核通过" && ss[1] != "审核未通过")) throw new Exception();
            return true;
        }

        public bool Condition(TFTask task, TFTaskChain taskChain, string sConditionParameter)
        {
            var ss = sConditionParameter.Split(',');
            if (ss.Length < 2)
            {
                return false;
            }
            var uow = new UnitOfWork();
            var rootFormerNode = uow.FindObject<TFNode>(CriteriaOperator.Parse("g_TaskChainGuid = ? AND s_CodeNodeCode = ?", taskChain.g_ID, ss[0]));
            if (rootFormerNode == null) return false;
            var rootFormerTask = rootFormerNode.GetTheOwnTask();
            TFTaskCheck taskCheck = rootFormerTask.GetListOthersTaskChecks().OrderByDescending(c => c.n_Sequence).FirstOrDefault();
            if (taskCheck == null) return false;
            if (ss[1] == "审核通过" && taskCheck.s_CheckReselt == "Y") return true;
            if (ss[1] == "审核未通过" && taskCheck.s_CheckReselt == "N") return true;
            return false;
        }
    }

    /// <summary>
    /// 自定义条件判断
    /// </summary>
    public class CCustomCondition : IConditionTask
    {
        public bool TestCondition(string sConditionParameter)
        {
            if (string.IsNullOrWhiteSpace(sConditionParameter) || !sConditionParameter.Contains(",")) throw new Exception();
            var ss = sConditionParameter.Split(',');
            var uow = new UnitOfWork();
            var customCondition = uow.FindObject<TFCodeCustomCondition>(CriteriaOperator.Parse("s_Code = ?", ss[0]));
            if (customCondition == null)
            {
                throw new Exception();
            }
            return true;
        }

        public bool Condition(TFTask task, TFTaskChain taskChain, string sConditionParameter)
        {
            if (taskChain == null) return false;
            var ss = sConditionParameter.Split(',');
            if (ss.Length < 2) return false;
            var uow = new UnitOfWork();
            var customCondition = uow.FindObject<TFCodeCustomCondition>(CriteriaOperator.Parse("s_Code = ?", ss[0]));
            if (customCondition == null) return false;
            var conditionInChain = uow.FindObject<TFCustomConditionInTaskChain>(CriteriaOperator.Parse("g_TaskChainID = ? AND n_CustomConditionID = ?", taskChain.g_ID, customCondition.n_ID));
            if (conditionInChain == null) return false;
            return ("," + conditionInChain.s_Values + ",").Contains("," + ss[1] + ",");
        }
    }

    /// <summary>
    /// 本链存在指定类型参数
    /// </summary>
    public class CChainExistParamTye : IConditionTask
    {
        public bool TestCondition(string sConditionParameter)
        {
            Enums.TaskParameterType paramType;
            if (string.IsNullOrWhiteSpace(sConditionParameter) || !Enum.TryParse<Enums.TaskParameterType>(sConditionParameter, out paramType)) throw new Exception();
            return true;
        }

        public bool Condition(TFTask task, TFTaskChain taskChain, string sConditionParameter)
        {
            if (string.IsNullOrWhiteSpace(sConditionParameter))
            {
                return false;
            }
            var uow = new UnitOfWork();
            var taskParam = uow.FindObject<TFTaskParameter>(CriteriaOperator.Parse("g_TaskChainID = ? AND s_ParaType = ?", taskChain.g_ID, sConditionParameter));
            if (taskParam == null)
            {
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// 指定任务下存在指定类型参数
    /// </summary>
    public class CTaskExistParamTye : IConditionTask
    {
        public bool TestCondition(string sConditionParameter)
        {
            if (string.IsNullOrWhiteSpace(sConditionParameter) || !sConditionParameter.Contains(",")) throw new Exception();
            var ss = sConditionParameter.Split(',');
            Enums.TaskParameterType paramType;
            if (string.IsNullOrWhiteSpace(ss[0]) || !Enum.TryParse<Enums.TaskParameterType>(ss[1], out paramType)) throw new Exception();
            return true;
        }

        public bool Condition(TFTask task, TFTaskChain taskChain, string sConditionParameter)
        {
            var ss = sConditionParameter.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (ss.Length < 2)
            {
                return false;
            }
            var uow = new UnitOfWork();
            var specifiedTask = uow.FindObject<TFNode>(CriteriaOperator.Parse("g_TaskChainGuid = ? AND s_CodeNodeCode = ?", taskChain.g_ID, ss[0])).GetTheOwnTask();
            if (specifiedTask == null)
            {
                return false;
            }
            var taskParam = uow.FindObject<TFTaskParameter>(CriteriaOperator.Parse("g_TaskID = ? AND s_ParaType = ?", specifiedTask.g_ID, ss[1]));
            if (taskParam == null)
            {
                return false;
            }

            return true;
        }
    }
}