using System;
using System.Linq;
using DataEntities.Case;
using DataEntities.Case.Patents;
using DataEntities.Case.Trademarks;
using DataEntities.Config;
using DataEntities.Config.Right;
using DataEntities.Contact.Demand;
using DataEntities.Element.Fee;
using DevExpress.Data.Filtering;
using DevExpress.Xpo;

namespace TaskFlowTest.ConditionMethod
{
    #region 公用
    /// <summary>
    /// 执行存储过程
    /// </summary>
    public class CExecProcs : IConditionBaseCase
    {
        public bool TestCondition(string sConditionParameter)
        {
            if (string.IsNullOrEmpty(sConditionParameter)) throw new Exception();

            var listParameters = sConditionParameter.Split(new char[] { ',', '，' }).ToList();
            if (listParameters.Count != 3) throw new Exception();
            string sType = listParameters[1].Trim();
            if (sType != "案件ID" && sType != "任务ID") throw new Exception();
            return true;
        }

        public bool Condition(DataEntities.TaskFlowData.TFTask task, DataEntities.Case.BasicCase baseCase, string sConditionParameter)
        {
            var listParameters = sConditionParameter.Split(new char[] { ',', '，' }).ToList();
            if (listParameters.Count != 3) return false;
            object objType = null;
            object objID = null;
            switch (listParameters[1].Trim())
            {
                case "案件ID":
                    objType = "Case";
                    objID = baseCase.n_CaseID;
                    break;
                case "任务ID":
                    objType = "Task";
                    objID = task.g_ID;
                    break;
                default:
                    return false;
            }
            OperandValue[] operandValueArray = { new OperandValue(objType), new OperandValue(objID), new OperandValue(listParameters[2].Trim()) };
            var theUow = new UnitOfWork();
            var resultSet = theUow.ExecuteSproc(listParameters[0].Trim(), operandValueArray);
            return Convert.ToBoolean(resultSet.ResultSet[0].Rows[0].Values[0]);
        }
    }
    /// <summary>
    /// 已结案
    /// </summary>
    public class CCIsClosedCase : IConditionBaseCase
    {
        public bool Condition(DataEntities.TaskFlowData.TFTask task, DataEntities.Case.BasicCase baseCase, string sConditionParameter)
        {
            return baseCase.s_CaseStatus == "结案";//硬编码处理，IPS中默认的处理方式，暂不判断英文
        }

        public bool TestCondition(string sConditionParameter)
        {
            return true;
        }
    }
    /// <summary>
    /// 内内案
    /// </summary>
    public class CCIsInternalInternalCase : IConditionBaseCase
    {
        public bool Condition(DataEntities.TaskFlowData.TFTask task, DataEntities.Case.BasicCase baseCase, string sConditionParameter)
        {
            return baseCase.s_FlowDirection == "II";
        }

        public bool TestCondition(string sConditionParameter)
        {
            return true;
        }
    }
    /// <summary>
    /// 内外案
    /// </summary>
    public class CCIsInternalOutCase : IConditionBaseCase
    {
        public bool Condition(DataEntities.TaskFlowData.TFTask task, DataEntities.Case.BasicCase baseCase, string sConditionParameter)
        {
            return baseCase.s_FlowDirection == "IO";
        }

        public bool TestCondition(string sConditionParameter)
        {
            return true;
        }
    }
    /// <summary>
    /// 外内案
    /// </summary>
    public class CCIsOutInternalCase : IConditionBaseCase
    {
        public bool Condition(DataEntities.TaskFlowData.TFTask task, DataEntities.Case.BasicCase baseCase, string sConditionParameter)
        {
            return baseCase.s_FlowDirection == "OI";
        }

        public bool TestCondition(string sConditionParameter)
        {
            return true;
        }
    }
    /// <summary>
    /// 外外案
    /// </summary>
    public class CCIsOutOutCase : IConditionBaseCase
    {
        public bool Condition(DataEntities.TaskFlowData.TFTask task, DataEntities.Case.BasicCase baseCase, string sConditionParameter)
        {
            return baseCase.s_FlowDirection == "OO";
        }

        public bool TestCondition(string sConditionParameter)
        {
            return true;
        }
    }
    /// <summary>
    /// 案件部门
    /// </summary>
    public class CCCaseDepartment : IConditionBaseCase
    {
        public bool Condition(DataEntities.TaskFlowData.TFTask task, DataEntities.Case.BasicCase baseCase, string sConditionParameter)
        {
            var uow = new UnitOfWork();
            var xpcDepartment = new XPCollection<Department>(uow) { Criteria = new InOperator("s_Name", sConditionParameter.Split(',')) };
            if (xpcDepartment.Where(p => p.n_ID == baseCase.n_DepartmentID).Count() != 0)
            {
                return true;
            }
            return false;
        }

        public bool TestCondition(string sConditionParameter)
        {
            var uow = new UnitOfWork();
            var xpcDepartment = new XPCollection<Department>(uow) { Criteria = new InOperator("s_Name", sConditionParameter.Split(',')) };
            if ((xpcDepartment.Count != sConditionParameter.Split(',').Count())) throw new Exception();
            return true;
        }
    }

    /// <summary>
    /// 申请人国家
    /// </summary>
    public class CCCaseApplicantCountry : IConditionBaseCase
    {
        public bool Condition(DataEntities.TaskFlowData.TFTask task, DataEntities.Case.BasicCase baseCase, string sConditionParameter)
        {
            var uow = new UnitOfWork();
            var xpcCodeCountry = new XPCollection<CodeCountry>(uow) { Criteria = new InOperator("s_Name", sConditionParameter.Split(',')) };
            if (xpcCodeCountry.Count(p => p.n_ID == baseCase.n_AppCountry) != 0) return true;
            return false;
        }

        public bool TestCondition(string sConditionParameter)
        {
            var uow = new UnitOfWork();
            var xpcCodeCountry = new XPCollection<CodeCountry>(uow) { Criteria = new InOperator("s_Name", sConditionParameter.Split(',')) };
            if ((xpcCodeCountry.Count != sConditionParameter.Split(',').Count())) throw new Exception();
            return true;
        }
    }

    /// <summary>
    /// 注册国家
    /// </summary>
    public class CCCaseRegCountry : IConditionBaseCase
    {
        public bool Condition(DataEntities.TaskFlowData.TFTask task, DataEntities.Case.BasicCase baseCase, string sConditionParameter)
        {
            var uow = new UnitOfWork();
            var xpcCodeCountry = new XPCollection<CodeCountry>(uow) { Criteria = new InOperator("s_Name", sConditionParameter.Split(',')) };
            if (xpcCodeCountry.Where(p => p.n_ID == baseCase.n_RegCountry).Count() != 0)
            {
                return true;
            }
            return false;
        }

        public bool TestCondition(string sConditionParameter)
        {
            var uow = new UnitOfWork();
            var xpcCodeCountry = new XPCollection<CodeCountry>(uow) { Criteria = new InOperator("s_Name", sConditionParameter.Split(',')) };
            if ((xpcCodeCountry.Count != sConditionParameter.Split(',').Count())) throw new Exception();
            return true;
        }
    }

    /// <summary>
    /// 业务类型
    /// </summary>
    public class CCCaseBusinessType : IConditionBaseCase
    {
        public bool Condition(DataEntities.TaskFlowData.TFTask task, DataEntities.Case.BasicCase baseCase, string sConditionParameter)
        {
            var uow = new UnitOfWork();
            var codeBusinessType = new XPQuery<CodeBusinessType>(uow).Single(b => b.n_ID == baseCase.n_BusinessTypeID);
            var sArray = sConditionParameter.Split(',');
            return sArray.Any(s => codeBusinessType.s_Name == s);
        }

        public bool TestCondition(string sConditionParameter)
        {
            if (string.IsNullOrEmpty(sConditionParameter)) throw new Exception();
            return true;
        }
    }

    /// <summary>
    /// 要求代码
    /// </summary>
    public class CCCaseDemandCode : IConditionBaseCase
    {
        public bool Condition(DataEntities.TaskFlowData.TFTask task, DataEntities.Case.BasicCase baseCase, string sConditionParameter)
        {
            var uow = new UnitOfWork();
            var xpcCodeDemand = new XPCollection<CodeDemand>(uow) { Criteria = new InOperator("s_SysDemand", sConditionParameter.Split(',')) };
            if ((xpcCodeDemand.Count != sConditionParameter.Split(',').Count())) return true;
            var sSql = " (TheCase.n_CaseID=" + baseCase.n_CaseID + " AND s_ModuleType='Case' )";//案件要求
            //if (baseCase.TheClient != null) sSql += " or (TheClient.n_ClientID=" + baseCase.TheClient.n_ClientID + " AND s_ModuleType='Client') ";//客户名称
            var criteria = CriteriaOperator.Parse(sSql);
            //var listAppID = baseCase.Applicants.Cast<DataEntities.Case.Applicant>().Select(a => a.n_ApplicantID).ToList();//申请人
            //if (listAppID.Count > 0) criteria = CriteriaOperator.Or(criteria, CriteriaOperator.And(new InOperator("TheApplicant.n_AppID", listAppID), CriteriaOperator.Parse("s_ModuleType='Applicant'")));
            var listCodeDemandID = new XPCollection<Demand>(baseCase.Session, criteria).Select(d => d.n_SysDemandID).ToList();
            if (listCodeDemandID.Count <= 0) return false;
            return listCodeDemandID.Select(codeDemandID => uow.GetObjectByKey<CodeDemand>(codeDemandID)).Any(codeDemand => codeDemand != null && sConditionParameter.Split(',').ToList().Contains(codeDemand.s_SysDemand));
        }

        public bool TestCondition(string sConditionParameter)
        {
            var uow = new UnitOfWork();
            var xpcCodeDemand = new XPCollection<CodeDemand>(uow) { Criteria = new InOperator("s_SysDemand", sConditionParameter.Split(',')) };
            if ((xpcCodeDemand.Count != sConditionParameter.Split(',').Count())) throw new Exception();
            return true;
        }
    }

    /// <summary>
    /// 存在优先权
    /// </summary>
    public class CCIfExistPrority : IConditionBaseCase
    {
        public bool Condition(DataEntities.TaskFlowData.TFTask task, DataEntities.Case.BasicCase baseCase, string sConditionParameter)
        {
            if (baseCase is ExtendedPatent)
            {
                var extendedPatent = baseCase as ExtendedPatent;
                extendedPatent.Priorities.Reload();
                if (extendedPatent.Priorities.Count > 0)
                    return true;
            }
            if (baseCase is ExtendedTrademark)
            {
                var extendedTrademark = baseCase as ExtendedTrademark;
                extendedTrademark.Priorities.Reload();
                if (extendedTrademark.Priorities.Count > 0)
                    return true;
            }
            return false;
        }

        public bool TestCondition(string sConditionParameter)
        {
            return true;
        }
    }

    /// <summary>
    /// 存在费用
    /// </summary>
    public class CCIfExistFee : IConditionBaseCase
    {
        public bool Condition(DataEntities.TaskFlowData.TFTask task, DataEntities.Case.BasicCase baseCase, string sConditionParameter)
        {
            var xpCollection = baseCase.FeeInCases;
            return xpCollection.Cast<FeeInCase>().Any(i => i.IsMainCase == "Y" && i.TheFee.s_Status[3] == 'N');
        }

        public bool TestCondition(string sConditionParameter)
        {
            return true;
        }
    }

    /// <summary>
    /// 存在代理人
    /// </summary>
    public class CCIfExistAttorney : IConditionBaseCase
    {
        public bool Condition(DataEntities.TaskFlowData.TFTask task, DataEntities.Case.BasicCase baseCase, string sConditionParameter)
        {
            if (baseCase.n_FirstAttorney >= 0 || baseCase.n_SecondAttorney >= 0)
                return true;
            return false;
        }

        public bool TestCondition(string sConditionParameter)
        {
            return true;
        }
    }

    /// <summary>
    /// 存在角色及处理人
    /// </summary>
    public class CCCheckCaseRole : IConditionBaseCase
    {
        public bool Condition(DataEntities.TaskFlowData.TFTask task, DataEntities.Case.BasicCase baseCase, string sConditionParameter)
        {
            var listArray = sConditionParameter.Split(',').ToList();
            if (!listArray.Any()) return false;
            var listCaseAttorney = baseCase.CaseAttorneys.Cast<CaseAttorney>().ToList();
            if (!listCaseAttorney.Any()) return false;
            var uow = new UnitOfWork();
            foreach (var sCaseRoleName in listArray)
            {
                var codeCaseRole = uow.FindObject<CodeCaseRole>(CriteriaOperator.Parse("s_Name = ?", sCaseRoleName));
                if (codeCaseRole == null) return false;
                var caseAttorney = listCaseAttorney.FirstOrDefault(f => f.n_CaseRoleID == codeCaseRole.n_ID);
                if (caseAttorney == null || caseAttorney.TheAttorney == null) return false;
            }
            return true;
        }

        public bool TestCondition(string sConditionParameter)
        {
            return true;
        }
    }

    /// <summary>
    /// 案件是分案
    /// </summary>
    public class CCIfDivisionCase : IConditionBaseCase
    {
        public bool Condition(DataEntities.TaskFlowData.TFTask task, DataEntities.Case.BasicCase baseCase, string sConditionParameter)
        {
            if (baseCase == null) return false;
            switch (baseCase.s_IPType)
            {
                case "P":
                    return ((BasicPatent)baseCase).b_DivisionalCaseFlag;
                    break;
                case "T":
                    return ((ExtendedTrademark)baseCase).b_DivisionalCaseFlag;
                    break;
            }
            return false;
        }

        public bool TestCondition(string sConditionParameter)
        {
            return true;
        }
    }
    #endregion

    #region 专利案
    /// <summary>
    /// 专利类型
    /// </summary>
    public class CPPatentType : IConditionBaseCase
    {
        public bool Condition(DataEntities.TaskFlowData.TFTask task, DataEntities.Case.BasicCase baseCase, string sConditionParameter)
        {
            var uow = new UnitOfWork();
            var patent = baseCase as ExtendedPatent;
            if (patent == null || patent.n_PatentTypeID == 0) return false;
            var codePatentType = new XPQuery<CodePTCType>(uow).Single(b => b.n_ID == patent.n_PatentTypeID);
            var sArray = sConditionParameter.Split(',');
            return sArray.Any(s => codePatentType.s_Name == s);
        }

        public bool TestCondition(string sConditionParameter)
        {
            if (string.IsNullOrEmpty(sConditionParameter)) throw new Exception();
            return true;
        }
    }
    /// <summary>
    /// 电子申请
    /// </summary>
    public class CPPatentIsCPC : IConditionBaseCase
    {
        public bool Condition(DataEntities.TaskFlowData.TFTask task, DataEntities.Case.BasicCase basicCase, string sConditionParameter)
        {
            if (basicCase is ExtendedPatent)
            {
                var extendedPatent = basicCase as ExtendedPatent;
                if (extendedPatent.s_IsRegOnline == "Y")
                    return true;
            }
            return false;
        }

        public bool TestCondition(string sConditionParameter)
        {
            return true;
        }
    }
    /// <summary>
    /// 权利要求项数大于
    /// </summary>
    public class CPPatentClaimCount : IConditionBaseCase
    {
        public bool Condition(DataEntities.TaskFlowData.TFTask task, DataEntities.Case.BasicCase basicCase, string sConditionParameter)
        {
            if (basicCase != null)
            {
                var basicPatent = basicCase as BasicPatent;
                if (basicPatent != null && basicPatent.TheLawInfo != null)
                {
                    if (basicPatent.TheLawInfo.n_ClaimCount > Convert.ToInt32(sConditionParameter))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool TestCondition(string sConditionParameter)
        {
            int nClaimCount = 0;
            Int32.TryParse(sConditionParameter, out nClaimCount);
            if (nClaimCount <= 0) throw new Exception();
            return true;
        }
    }
    /// <summary>
    /// 说明书页数大于
    /// </summary>
    public class CPPatentManualPages : IConditionBaseCase
    {
        public bool Condition(DataEntities.TaskFlowData.TFTask task, DataEntities.Case.BasicCase basicCase, string sConditionParameter)
        {
            if (basicCase != null)
            {
                var basicPatent = basicCase as BasicPatent;
                if (basicPatent != null && basicPatent.TheLawInfo != null)
                {
                    if (basicPatent.TheLawInfo.n_ManualPages > Convert.ToInt32(sConditionParameter))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool TestCondition(string sConditionParameter)
        {
            int nManualPages = 0;
            Int32.TryParse(sConditionParameter, out nManualPages);
            if (nManualPages <= 0) throw new Exception();
            return true;
        }
    }
    #endregion

    #region 商标案
    /// <summary>
    /// 商标类型
    /// </summary>
    public class CTTrademarkType : IConditionBaseCase
    {
        public bool Condition(DataEntities.TaskFlowData.TFTask task, DataEntities.Case.BasicCase baseCase, string sConditionParameter)
        {
            var uow = new UnitOfWork();
            var trademark = baseCase as ExtendedTrademark;
            if (trademark == null || string.IsNullOrWhiteSpace(trademark.s_TrademarkType)) return false;
            var xpos = new XPCollection(uow, typeof(CodePTCType))
                {
                    Criteria = CriteriaOperator.And(CriteriaOperator.Parse("s_IPType='T'"), new InOperator("n_ID", trademark.s_TrademarkType.Split(',')))
                };
            if (xpos.Count <= 0)
            {
                return false;
            }
            var sArray = sConditionParameter.Split(',');
            return xpos.Cast<CodePTCType>().Any(codePatentType => sArray.Any(s => codePatentType.s_Name == s));
        }

        public bool TestCondition(string sConditionParameter)
        {
            if (string.IsNullOrEmpty(sConditionParameter)) throw new Exception();
            return true;
        }
    }

    /// <summary>
    /// 电子申请
    /// </summary>
    public class CTTrademarkIsCPC : IConditionBaseCase
    {
        public bool Condition(DataEntities.TaskFlowData.TFTask task, DataEntities.Case.BasicCase basicCase, string sConditionParameter)
        {
            if (basicCase is ExtendedTrademark)
            {
                var extendedTrademark = basicCase as ExtendedTrademark;
                if (extendedTrademark.s_IsRegOnline == "Y")
                    return true;
            }
            return false;
        }

        public bool TestCondition(string sConditionParameter)
        {
            return true;
        }
    }
    #endregion

    #region 版权案

    #endregion

    #region 域名案

    #endregion

    #region 其他案

    #endregion
}