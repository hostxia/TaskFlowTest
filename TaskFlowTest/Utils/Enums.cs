namespace TaskFlowTest.Utils
{
    public class Enums
    {
        /// <summary>
        /// 窗体状态
        /// </summary>
        public enum DetailFormStatus
        {
            ReadOnly,//只读
            View,    //查看
            New,     //新建 
            Edit,    //编辑
        }

        /// <summary>
        /// 任务链配置状态
        /// </summary>
        public enum CodeTaskChainState
        {
            D,//未启用
            E,//启用
            I,//作废
        }

        /// <summary>
        /// 任务链状态
        /// </summary>
        public enum TaskChainState
        {
            P,//未完成
            F,//完成
            C,//关闭
            O,//打开
        }

        /// <summary>
        /// 任务链触发范围
        /// </summary>
        public enum TaskChainTriggerScope
        {
            P,//未结案的案件
            C,//已结案的案件
            A,//所有案件
        }

        /// <summary>
        /// 任务链触发类型
        /// </summary>
        public enum TaskChainTriggerType
        {
            Client,//登记客户       
            Case,//保存案件
            CaseSubmit,//新案提交
            CameFileOfficial,//登记官方来文
            Manual,//手工创建
            Task,//任务触发创建
        }

        /// <summary>
        /// 关联对象类型
        /// </summary>
        public enum RelatedObjectType
        {
            Client,//登记客户       
            Case,//登记案件
            CameFileOfficial,//登记官方来文
        }

        /// <summary>
        /// 节点模式
        /// </summary>
        public enum NodeMode
        {
            N,//正常
            P,//预产生
        }

        /// <summary>
        /// 节点类型
        /// </summary>
        public enum NodeType
        {
            S,//起始节点
            T,//任务
            C,//任务链
            E,//截止节点
        }

        /// <summary>
        /// 任务状态
        /// </summary>
        public enum TaskState
        {
            P,//未完成
            F,//完成
            C,//关闭
            O,//打开
        }

        public enum ReadState
        {
            U,//未读
            R,//已读
        }

        /// <summary>
        /// 任务执行分类
        /// </summary>
        public enum TaskExecutorType
        {
            E,//执行人
            R,//执行角色
            P,//执行岗位
            S,//储存过程
        }

        /// <summary>
        /// 案件类型
        /// </summary>
        public enum IPType
        {
            P,//专利
            T,//商标
            C,//版权
            D,//域名
            O//其他案
        }

        /// <summary>
        /// 条件类型
        /// </summary>
        public enum ConditionType
        {
            Common,
            CaseCommon,
            CasePatent,
            CaseTrademark,
            CaseCopyright,
            CaseDomain,
            CaseOther,
        }

        public enum Modules
        {
            /// <summary>
            /// 任务链分类配置
            /// </summary>
            TaskChainType,
            /// <summary>
            /// 任务类型配置
            /// </summary>
            TaskType,
            /// <summary>
            /// 任务紧急程度
            /// </summary>
            TaskUrgency,
            /// <summary>
            /// 要求配置
            /// </summary>
            DemandType,
            /// <summary>
            /// 动作配置
            /// </summary>
            Action,
            /// <summary>
            /// 期限配置
            /// </summary>
            Deadline,
            /// <summary>
            /// 自定义条件配置
            /// </summary>
            CustomCondition,
            /// <summary>
            /// 提醒配置
            /// </summary>
            Remind,
            /// <summary>
            /// 提醒模版配置
            /// </summary>
            RemindTemplate,
            /// <summary>
            /// 任务自定义查询配置
            /// </summary>
            TaskSearchCondition,
            /// <summary>
            /// 自动控制要求配置
            /// </summary>
            AutoDemand,
            /// <summary>
            /// 工作项配置
            /// </summary>
            WorkItem,
            /// <summary>
            /// 工作项分类配置
            /// </summary>
            WorkItemType,
        }

        /// <summary>
        /// 执行时机
        /// </summary>
        public enum ExecuteTime
        {
            B,//该任务创建前
            C,//该任务创建后
            F,//该任务完成后
        }

        /// <summary>
        /// 基准日所属对象
        /// </summary>
        public enum BaseDateObjectType
        {
            /// <summary>
            /// 案件
            /// </summary>
            Case,
            /// <summary>
            /// 来文
            /// </summary>
            InFile,
            /// <summary>
            /// 文件
            /// </summary>
            BaseFile,
            /// <summary>
            /// 专利
            /// </summary>
            Patent,
            /// <summary>
            /// 商标
            /// </summary>
            Trademark,
            /// <summary>
            /// 任务链
            /// </summary>
            TaskChain,
            /// <summary>
            /// 任务
            /// </summary>
            Task,
        }

        /// <summary>
        /// 期限分类（类型）
        /// </summary>
        public enum DeadlineType
        {
            /// <summary>
            /// 官方期限
            /// </summary>
            Official,
            /// <summary>
            /// 客户期限
            /// </summary>
            Client,
            /// <summary>
            /// 内部期限
            /// </summary>
            Internal,
            /// <summary>
            /// 个人期限
            /// </summary>
            Personal
        }

        /// <summary>
        /// 基准日计算规则
        /// </summary>
        public enum BaseDateCalcRule
        {
            /// <summary>
            /// +月+天
            /// </summary>
            AddMonthDay,
            /// <summary>
            /// +天+月
            /// </summary>
            AddDayMonth,
        }

        /// <summary>
        /// 期限取值方式
        /// </summary>
        public enum DeadlineValueRule
        {
            /// <summary>
            /// 取最早时间
            /// </summary>
            EarliestTime,
            /// <summary>
            /// 去最晚时间
            /// </summary>
            LatestTime,
            /// <summary>
            /// 通过存储过程
            /// </summary>
            StoredProcedure,
        }

        /// <summary>
        /// 日期类型
        /// </summary>
        public enum DateType
        {
            Year,//年
            Month,//月
            Week,//周
            Day,//日
        }

        public enum TaskDateCalcRule
        {
            Deadline,//基于期限
            TaskDate,//基于任务日期
        }

        public enum TaskDateType
        {
            StartDate,//开始日期
            EndDate,//结束日期
            FinishTime,//完成时间
            CreateTime,//创建时间
        }

        public enum TaskParameterType
        {
            /// <summary>
            /// 审核记录
            /// </summary>
            TaskCheck,
            /// <summary>
            /// 专利
            /// </summary>
            Patent,
            /// <summary>
            /// 发文
            /// </summary>
            OutFile,
            /// <summary>
            /// 来文
            /// </summary>
            InFile,
            /// <summary>
            /// 其他文
            /// </summary>
            OtherFile,
            /// <summary>
            /// 工时
            /// </summary>
            TimeSheet,
            /// <summary>
            /// 账单
            /// </summary>
            Bill,
            /// <summary>
            /// 外部账单
            /// </summary>
            AbroadBill,
            /// <summary>
            /// 邮件
            /// </summary>
            Email,
        }

        public enum TaskTriggerTaskChainRelatedObject
        {
            SourceRelatedObject,//同源任务链关联对象
            RelativeSS,//相关案件-双申
            RelativeIDS,//相关案件-IDS
            RelativeDAS,//相关案件-DAS
            RelativePCTI,//相关案件-PCT国家
            RelativeTB,//相关案件-同标
            RelativeDivisional,//相关案件-分案
        }

        /// <summary>
        /// 提醒触发类型
        /// </summary>
        public enum ReminderTriggerType
        {
            /// <summary>
            /// 任务提醒
            /// </summary>
            Task,
            /// <summary>
            /// 官方来文提醒
            /// </summary>
            CameFileOfficial,
            /// <summary>
            /// 客户来文
            /// </summary>
            CameFileClient,
        }

        public enum RemindType
        {
            I,//立即
            T,//到期
            C,//循环
        }

        public enum TaskImmediatelyRemindType
        {
            N,//任务产生
            A,//任务指派
            F,//任务完成
            C,//任务关闭
            O,//任务打开
            U,//任务未分配
            S,//任务应开始日期为空
            E,//任务应截止日期为空
        }

        public enum CameFileImmediatelyRemindType
        {
            N,//来文登记
            D,//来文分发
        }

        public enum RemindValidState
        {
            Y,//提醒有效
            N,//提醒作废
        }

        public enum RemindState
        {
            Y,//提醒完成
            N,//提醒未完成
        }

        public enum RemindModeType
        {
            P,//弹窗提醒
            M,//邮件提醒
            W,//微信提醒
        }

        public enum RemindRecordState
        {
            S,//提醒发送成功
            F,//提醒发送失败
            U,//未读
            R,//已读
            C,//已处理
        }

        public enum RemindObjectType
        {
            U,//用户
            P,//岗位
            R,//角色
            D,//部门
            E,//任务执行人
            C,//来文创建人
        }

        /// <summary>
        /// 自动控制要求的数据类型
        /// </summary>
        public enum AutoDemandDataType
        {
            /// <summary>
            /// 字符串
            /// </summary>
            S,
            /// <summary>
            /// 数字
            /// </summary>
            N,
            /// <summary>
            /// 日期时间
            /// </summary>
            D,
        }

        /// <summary>
        /// 工作项转收费产生方式
        /// </summary>
        public enum WorkItemConvertToFeeMethod
        {
            A,//自动转费
            M,//手动调整
        }

        public enum OrgRelatedObjectType
        {
            Root,//根节点
            Common,//通用信息
            Agency,//代理机构信息
        }
    }
}