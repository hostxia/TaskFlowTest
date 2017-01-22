using System.Collections.Generic;
using TaskFlowTest.Utils;

namespace TaskFlowTest.ConditionMethod
{
    public class ConditionClassData
    {
        public static List<ConditionClass> ListAllCondition = new List<ConditionClass>()
        {
            #region Common - Start with 'C'
            new ConditionClass{ e_ConditionType = Enums.ConditionType.Common, s_Code = "CTrue", s_Name = "真", s_Description = "真，测试流程用"},
            new ConditionClass{ e_ConditionType = Enums.ConditionType.Common, s_Code = "CFalse", s_Name = "假", s_Description = "假，测试流程用"},
            new ConditionClass{ e_ConditionType = Enums.ConditionType.Common, s_Code = "CExecProcs[]", s_Name = "执行存储过程[]", s_Description = "执行指定的存储过程。存储过程的传入参数为案件ID或任务ID，返回结果为布尔类型值，表示该条件是否满足。示例：执行存储过程[储存过程名称,案件ID/任务ID,参数]"},
            new ConditionClass{ e_ConditionType = Enums.ConditionType.Common, s_Code = "CNodeMode[]", s_Name = "节点模式[]", s_Description = "判断指定节点的模式"},
            new ConditionClass{ e_ConditionType = Enums.ConditionType.Common, s_Code = "CTaskState[]", s_Name = "任务状态[]", s_Description = "判断指定任务的状态"},
            new ConditionClass{ e_ConditionType = Enums.ConditionType.Common, s_Code = "CTaskChainState[]", s_Name = "子任务链状态[]", s_Description = "判断指定子任务链状态"},
            new ConditionClass{ e_ConditionType = Enums.ConditionType.Common, s_Code = "CTaskCheckResult[]", s_Name = "任务审核结果[]", s_Description = "任务审核结果说明。示例：任务审核结果[XXXX,审核未通过]"},
            new ConditionClass{ e_ConditionType = Enums.ConditionType.Common, s_Code = "CChainExistParamTye[]", s_Name = "本链存在类型参数[]", s_Description = "本链下存在指定类型的参数。示例：本链存在类型参数[TaskCheck]"},
            new ConditionClass{ e_ConditionType = Enums.ConditionType.Common, s_Code = "CTaskExistParamTye[]", s_Name = "任务存在类型参数[]", s_Description = "指定任务下存在指定类型的参数。示例：任务存在类型参数[XXXX,TaskCheck]"},
            new ConditionClass{ e_ConditionType = Enums.ConditionType.Common, s_Code = "CCustomCondition[]", s_Name = "自定义条件[]", s_Description = "自定义条件判断。示例：自定义条件[XXXX,是客户责任]"},
            #endregion

            #region CaseCommon - Start with 'CC'
            new ConditionClass{ e_ConditionType = Enums.ConditionType.CaseCommon, s_Code = "CCIsClosedCase", s_Name = "已结案", s_Description = "判断当前案件的案件状态是否是“结案”"},
            new ConditionClass{ e_ConditionType = Enums.ConditionType.CaseCommon, s_Code = "CCIsInternalInternalCase", s_Name = "内内案", s_Description = "判断当前案件的案件流向是否为内到内。示例：内内案"},
            new ConditionClass{ e_ConditionType = Enums.ConditionType.CaseCommon, s_Code = "CCIsInternalOutCase", s_Name = "内外案", s_Description = "判断当前案件的案件流向是否为内到外。示例：内外案"},
            new ConditionClass{ e_ConditionType = Enums.ConditionType.CaseCommon, s_Code = "CCIsOutInternalCase", s_Name = "外内案", s_Description = "判断当前案件的案件流向是否为外到内。示例：外内案"},
            new ConditionClass{ e_ConditionType = Enums.ConditionType.CaseCommon, s_Code = "CCIsOutOutCase", s_Name = "外外案", s_Description = "判断当前案件的案件流向是否为外到外。示例：外外案"},
            new ConditionClass{ e_ConditionType = Enums.ConditionType.CaseCommon, s_Code = "CCCaseDepartment[]", s_Name = "案件部门[]", s_Description = "判断当前案件的部门，可以填写多个，用英文逗号“,”分割，多个部门之间为“或”的关系。示例：案件部门[流程部,代理部]"},
            new ConditionClass{ e_ConditionType = Enums.ConditionType.CaseCommon, s_Code = "CCCaseApplicantCountry[]", s_Name = "申请人国家[]", s_Description = "判断当前案件的申请人国家，可以填写多个，用英文逗号“,”分割，多个申请国家之间为“或”的关系。示例：申请人国家[中国,美国]"},
            new ConditionClass{ e_ConditionType = Enums.ConditionType.CaseCommon, s_Code = "CCCaseRegCountry[]", s_Name = "注册国家[]", s_Description = "判断当前案件的申请国家，可以填写多个，用英文逗号“,”分割，多个申请国家之间为“或”的关系。示例：注册国家[中国,美国]"},
            new ConditionClass{ e_ConditionType = Enums.ConditionType.CaseCommon, s_Code = "CCCaseBusinessType[]", s_Name = "业务类型[]", s_Description = "判断当前案件的业务类型，可以填写多个，用英文逗号“,”分割，多个业务类型之间为或关系。示例：业务类型[申请案,转让案]"},
            new ConditionClass{ e_ConditionType = Enums.ConditionType.CaseCommon, s_Code = "CCCaseDemandCode[]", s_Name = "要求代码[]", s_Description = "判断当前案件的要求，可以填写多个，用英文逗号“,”分割，多个要求代码之间为“或”的关系。示例：要求代码[代码A,代码B]"},
            new ConditionClass{ e_ConditionType = Enums.ConditionType.CaseCommon, s_Code = "CCIfExistPrority", s_Name = "存在优先权", s_Description = "判断当前案件是否存在优先权。示例：存在优先权"},
            new ConditionClass{ e_ConditionType = Enums.ConditionType.CaseCommon, s_Code = "CCIfExistFee", s_Name = "存在费用", s_Description = "判断当前案件是否存在未入账单的费用。示例：存在费用"},
            new ConditionClass{ e_ConditionType = Enums.ConditionType.CaseCommon, s_Code = "CCIfExistAttorney", s_Name = "存在代理人", s_Description = "判断当前案件是否存在第一或第二代理人。示例：存在代理人"},
            new ConditionClass{ e_ConditionType = Enums.ConditionType.CaseCommon, s_Code = "CCCheckCaseRole[]", s_Name = "存在角色及处理人[]", s_Description = "判断当前案件的是否存在指定角色及对应处理人，多个角色用英文逗号“,”分割。示例：存在处理人[流程部-经理,代理部-部门经理]"},
            new ConditionClass{ e_ConditionType = Enums.ConditionType.CaseCommon, s_Code = "CCIfDivisionCase", s_Name = "案件是分案", s_Description = "判断当前案件是否是分案。示例：案件是分案"},
	        #endregion

            #region CasePatent
		    new ConditionClass{ e_ConditionType = Enums.ConditionType.CasePatent, s_Code = "CPPatentType[]", s_Name = "专利类型[]", s_Description = "判断当前案件的专利类型，可以填写多个，用英文逗号“,”分割，多个专利类型之间为或关系。示例：专利类型[发明,实用新型]"},
            new ConditionClass{ e_ConditionType = Enums.ConditionType.CasePatent, s_Code = "CPPatentIsCPC", s_Name = "专利电子申请", s_Description = "判断当前案件的申请方式是否为专利的电子申请，示例：专利电子申请"},
            new ConditionClass{ e_ConditionType = Enums.ConditionType.CasePatent, s_Code = "CPPatentClaimCount[]", s_Name = "权利要求项数大于[]", s_Description = "判断当前案件的权利要求项数。示例：权利要求项数大于[30]"},
            new ConditionClass{ e_ConditionType = Enums.ConditionType.CasePatent, s_Code = "CPPatentManualPages[]", s_Name = "说明书页数大于[]", s_Description = "判断当前案件的说明书页数。示例：说明书页数大于[30]"},
	        #endregion

            #region CaseTrademark
		    new ConditionClass{ e_ConditionType = Enums.ConditionType.CaseTrademark, s_Code = "CTTrademarkType[]", s_Name = "商标类型[]", s_Description = "判断当前案件的商标类型"},
            new ConditionClass{ e_ConditionType = Enums.ConditionType.CaseTrademark, s_Code = "CTTrademarkIsCPC", s_Name = "商标电子申请", s_Description = "判断当前案件的申请方式是否为商标的电子申请，示例：商标电子申请"},
	        #endregion

            #region CaseCopyright
		 
	        #endregion

            #region CaseDomain
		 
	        #endregion

            #region CaseOther
		 
	        #endregion
        };

    }
}