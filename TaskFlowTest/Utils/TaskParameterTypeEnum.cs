using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskFlowTest.Utils
{
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
        /// <summary>
        /// 年费
        /// </summary>
        AnnualFee,
    }
}
