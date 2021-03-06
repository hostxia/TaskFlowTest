﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DataEntities.Case;
using DataEntities.Element.Files;
using DataEntities.TaskFlowConfig;
using DevExpress.Xpo;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using TaskFlowTest.Utils;

namespace TaskFlowTest
{
    public partial class XFrmMain : XtraForm
    {
        private BasicCase _basicCase;
        private List<dynamic> _listDynamicCameFileOfficials;
        private List<dynamic> _listDynamicTaskChains;
        private TaskFlowTestHelper _taskFlowTestHelper;
        private TestResultInfoSet _testResultInfoSet;
        private Thread _thread;
        public XFrmMain()
        {
            InitializeComponent();
        }

        private void XFrmMain_Load(object sender, EventArgs e)
        {
            _testResultInfoSet = new TestResultInfoSet(xgridResult);
            SetControlEnabled(true);
        }

        private void InitLoad()
        {
            xgridResult.DataSource = _taskFlowTestHelper.TestResultInfoSet;

            xlueRelatedObjectType.Properties.DataSource = ComboxStruct.RelatedObjectType;
            xlueRelatedObjectType.Properties.DisplayMember = "DisplayMember";
            xlueRelatedObjectType.Properties.ValueMember = "DisplayValue";
            xlueRelatedObjectType.Properties.Columns.Clear();
            xlueRelatedObjectType.Properties.Columns.Add(new LookUpColumnInfo("DisplayMember"));
            xlueRelatedObjectType.Properties.ShowFooter = false;
            xlueRelatedObjectType.Properties.ShowHeader = false;
            xlueRelatedObjectType.Properties.ShowLines = false;

            _listDynamicTaskChains = new List<dynamic>();
            new XPQuery<TFCodeTaskChain>(new UnitOfWork()).Where(
                    w => w.s_TriggerType.Contains("Manual"))
                .Select(c => new { c.g_ID, c.s_Code, c.s_Name, c.n_TaskChainTypeID })
                .ToList()
                .ForEach(f =>
                {
                    dynamic expando = new ExpandoObject();
                    expando.g_ID = f.g_ID.ToString();
                    expando.s_Code = f.s_Code;
                    expando.s_Name = f.s_Name;
                    expando.n_TaskChainTypeID = f.n_TaskChainTypeID;
                    _listDynamicTaskChains.Add(expando);
                });
            xslueTaskChainCode.Properties.DataSource = _listDynamicTaskChains;
            xslueTaskChainCode.Properties.DisplayMember = "s_Code";
            xslueTaskChainCode.Properties.ValueMember = "g_ID";

            xslueCameFileOfficial.Properties.DisplayMember = "s_Name";
            xslueCameFileOfficial.Properties.ValueMember = "n_FileID";
            xslueCameFileOfficial.Enabled = false;

            xlueRelatedObjectType.EditValue = TaskFlowEnum.RelatedObjectType.Case.ToString();
        }

        private void SetControlEnabled(bool bEnabled)
        {
            if (!bEnabled && xchkClearInfo.Checked)
                _taskFlowTestHelper.TestResultInfoSet.Clear();
            layoutControlGroup2.Enabled = bEnabled;
            layoutControlGroup5.Enabled = bEnabled;
            xsbExport.Enabled = bEnabled;
            xchkClearInfo.Enabled = bEnabled;
            xsbTest.Enabled = bEnabled;
            xsbCancel.Enabled = !bEnabled;
        }

        private void xsbTest_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            SetControlEnabled(false);
            _thread = new Thread(() =>
            {
                _taskFlowTestHelper = new TaskFlowTestHelper(_testResultInfoSet);
                _taskFlowTestHelper.TestAndInitConnection(xteIPSAddress.Text.Trim());
                if (xchkCondition.Checked)
                    _taskFlowTestHelper.TestCondition();
                if (xchkSimulation.Checked)
                {
                    var sObjTypeName = string.Empty;
                    var sRelatedObjectId = "0";
                    if (xlueRelatedObjectType.EditValue.ToString() == TaskFlowEnum.RelatedObjectType.Case.ToString())
                    {
                        sObjTypeName = typeof(BasicCase).FullName;
                        sRelatedObjectId = _basicCase.n_CaseID.ToString();
                    }
                    else if (xlueRelatedObjectType.EditValue.ToString() ==
                             TaskFlowEnum.RelatedObjectType.CameFileOfficial.ToString())
                    {
                        sObjTypeName = typeof(InFile).FullName;
                        sRelatedObjectId = xslueCameFileOfficial.EditValue.ToString();
                    }
                    _taskFlowTestHelper.ExecuteTaskChain(Guid.Parse(xslueTaskChainCode.EditValue.ToString()), sObjTypeName, sRelatedObjectId);
                }
                Invoke(new Action(() => { SetControlEnabled(true); }));
            });
            _thread.Start();
        }

        private void xsbExport_Click(object sender, EventArgs e)
        {
            if (saveFileDialog.ShowDialog() != DialogResult.OK) return;
            xgvResult.ExportToXlsx(saveFileDialog.FileName);
        }

        private void xlueRelatedObjectType_EditValueChanged(object sender, EventArgs e)
        {
            if (xlueRelatedObjectType.EditValue == null) return;
            xslueCameFileOfficial.Text = string.Empty;
            xbeCase.Text = string.Empty;
            if (xlueRelatedObjectType.EditValue.ToString() == TaskFlowEnum.RelatedObjectType.Case.ToString())
                xslueCameFileOfficial.Enabled = false;
            if (xlueRelatedObjectType.EditValue.ToString() == TaskFlowEnum.RelatedObjectType.CameFileOfficial.ToString())
                xslueCameFileOfficial.Enabled = true;
        }

        private void xbeCase_ButtonClick(object sender, ButtonPressedEventArgs e)
        {
            if (_taskFlowTestHelper == null) return;
            var idList = new List<int>();
            var frm = new XFrmSearchCase(_taskFlowTestHelper.UnitOfWork, idList, false, xteIPSAddress.Text.Trim());
            if (frm.ShowDialog() != DialogResult.OK) return;
            if (frm.Cases.Count <= 0)
            {
                _basicCase = null;
                xbeCase.Text = string.Empty;
                if (xlueRelatedObjectType.EditValue.ToString() ==
                    TaskFlowEnum.RelatedObjectType.CameFileOfficial.ToString())
                {
                    _listDynamicCameFileOfficials = new List<dynamic>();
                    xslueCameFileOfficial.Properties.DataSource = _listDynamicCameFileOfficials;
                }
            }
            else
            {
                _basicCase = frm.Cases[0];
                xbeCase.Text = frm.Cases[0].s_CaseSerial;
                if (xlueRelatedObjectType.EditValue.ToString() !=
                    TaskFlowEnum.RelatedObjectType.CameFileOfficial.ToString()) return;
                _listDynamicCameFileOfficials = new List<dynamic>();
                string sSqlQueryOfficialCameFile =
                    $"SELECT T_MainFiles.n_FileID,T_MainFiles.s_Name,TCase_Base.s_CaseSerial FROM dbo.T_MainFiles LEFT JOIN dbo.T_FileInCase ON dbo.T_MainFiles.n_FileID = dbo.T_FileInCase.n_FileID LEFT JOIN dbo.TCase_Base ON dbo.T_FileInCase.n_CaseID = dbo.TCase_Base.n_CaseID WHERE s_IOType = 'I' AND s_ClientGov = 'O' AND TCase_Base.s_CaseSerial = '{frm.Cases[0].s_CaseSerial}'";
                var dataOfficialCameFile = _taskFlowTestHelper.UnitOfWork.ExecuteQuery(sSqlQueryOfficialCameFile);
                foreach (var row in dataOfficialCameFile.ResultSet[0].Rows)
                {
                    if (row.Values[0] == null || row.Values[1] == null) continue;
                    dynamic expando = new ExpandoObject();
                    expando.n_FileID = row.Values[0].ToString();
                    expando.s_Name = row.Values[2] == null ? "" : row.Values[2] + " / " + row.Values[1];
                    _listDynamicCameFileOfficials.Add(expando);
                }
                xslueCameFileOfficial.Properties.DataSource = _listDynamicCameFileOfficials;
            }
        }

        private void xsbTestConnection_Click(object sender, EventArgs e)
        {
            try
            {
                _taskFlowTestHelper = new TaskFlowTestHelper(_testResultInfoSet);
                if (_taskFlowTestHelper.TestAndInitConnection(xteIPSAddress.Text.Trim()))
                {
                    InitLoad();
                    XtraMessageBox.Show(this, "连接成功！");
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(this, ex.Message + "\r\n" + ex.Source + "\r\n" + ex.StackTrace);
            }
        }

        private void xsbCancel_Click(object sender, EventArgs e)
        {
            if (XtraMessageBox.Show("是否停止测试？", "停止测试", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            _thread.Abort();
            SetControlEnabled(true);
        }
    }
}