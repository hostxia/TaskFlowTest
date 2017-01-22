using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TaskFlowTest
{
    public class TestResultInfo
    {
        public DateTime CreateTime { get; set; }
        public InfoType InfoType { get; set; }
        public string InfoTypeString => InfoType.ToString();
        public string Content { get; set; }
        public string TaskChainNo { get; set; }
        public string TaskNo { get; set; }
    }

    public enum InfoType
    {
        Info,
        Warning,
        Error
    }

    public class TestResultInfoSet : BindingList<TestResultInfo>
    {
        public TestResultInfoSet Add(InfoType infoType, string sContent, string sTaskChainNo = null, string sTaskNo = null, DateTime dtCreateTime = new DateTime())
        {
            Add(new TestResultInfo
            {
                InfoType = infoType,
                Content = sContent,
                CreateTime = dtCreateTime == new DateTime() ? DateTime.Now : dtCreateTime,
                TaskChainNo = sTaskChainNo,
                TaskNo = sTaskNo
            });
            Application.DoEvents();
            return this;
        }

        public TestResultInfoSet AddError(string sContent, string sTaskChainNo = null, string sTaskNo = null, DateTime dtCreateTime = new DateTime())
        {
            return Add(InfoType.Error, sContent, sTaskChainNo, sTaskNo, dtCreateTime);
        }

        public TestResultInfoSet AddWarning(string sContent, string sTaskChainNo = null, string sTaskNo = null, DateTime dtCreateTime = new DateTime())
        {
            return Add(InfoType.Warning, sContent, sTaskChainNo, sTaskNo, dtCreateTime);
        }

        public TestResultInfoSet AddInfo(string sContent, string sTaskChainNo = null, string sTaskNo = null, DateTime dtCreateTime = new DateTime())
        {
            return Add(InfoType.Info, sContent, sTaskChainNo, sTaskNo, dtCreateTime);
        }
    }
}
