using System;
using System.Collections.Generic;
using DevExpress.XtraGrid;

namespace TaskFlowTest
{
    public struct TestResultInfo
    {
        public DateTime CreateTime { get; set; }
        public InfoType InfoType { get; set; }
        public string InfoTypeString => InfoType.ToString();
        public string Content { get; set; }
        public string TaskChainNo { get; set; }
        public string TaskNo { get; set; }
        public string Note { get; set; }
    }

    public enum InfoType
    {
        Info,
        Warning,
        Error
    }

    public class TestResultInfoSet : List<TestResultInfo>
    {
        private readonly GridControl _gridControl;

        public TestResultInfoSet(GridControl gridControl)
        {
            _gridControl = gridControl;
            _gridControl.Invoke(new Action(() => { _gridControl.DataSource = this; }));
        }

        public TestResultInfoSet Add(InfoType infoType, string sContent, string sTaskChainNo = null,
            string sTaskNo = null, string sNote = null, DateTime dtCreateTime = new DateTime())
        {
            Add(new TestResultInfo
            {
                InfoType = infoType,
                Content = sContent,
                CreateTime = dtCreateTime == new DateTime() ? DateTime.Now : dtCreateTime,
                TaskChainNo = sTaskChainNo,
                TaskNo = sTaskNo,
                Note = sNote
            });
            _gridControl.Invoke(new Action(() => { _gridControl.RefreshDataSource(); }));
            return this;
        }

        public TestResultInfoSet AddError(string sContent, string sTaskChainNo = null, string sTaskNo = null,
            string sNote = null, DateTime dtCreateTime = new DateTime())
        {
            return Add(InfoType.Error, sContent, sTaskChainNo, sTaskNo, sNote, dtCreateTime);
        }

        public TestResultInfoSet AddWarning(string sContent, string sTaskChainNo = null, string sTaskNo = null,
            string sNote = null, DateTime dtCreateTime = new DateTime())
        {
            return Add(InfoType.Warning, sContent, sTaskChainNo, sTaskNo, sNote, dtCreateTime);
        }

        public TestResultInfoSet AddInfo(string sContent, string sTaskChainNo = null, string sTaskNo = null,
            string sNote = null, DateTime dtCreateTime = new DateTime())
        {
            return Add(InfoType.Info, sContent, sTaskChainNo, sTaskNo, sNote, dtCreateTime);
        }
    }
}