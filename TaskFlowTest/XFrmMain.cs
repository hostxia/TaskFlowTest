using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DataEntities.Case;
using DataEntities.TaskFlowConfig;
using DevExpress.Xpo;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using TaskFlowTest.Utils;

namespace TaskFlowTest
{
    public partial class XFrmMain : XtraForm
    {
        private List<dynamic> _listDynamicCameFileOfficials;
        private List<dynamic> _listDynamicTaskChains;
        private BasicCase _basicCase;

        public XFrmMain()
        {
            InitializeComponent();
        }

        private void XFrmMain_Load(object sender, EventArgs e)
        {
            InitLoad();
        }

        private void InitLoad()
        {
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
                    w => w.s_State == "E" && w.s_TriggerType.Contains("Manual"))
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

        private void xsbTest_Click(object sender, EventArgs e)
        {
            xsbTest.Enabled = false;
            xsbExport.Enabled = false;
            Task.Run(() =>
            {
                var taskFlowTestHelper = new TaskFlowTestHelper(new TestResultInfoSet());
                Invoke(new Action(() => { xgridResult.DataSource = taskFlowTestHelper.TestResultInfoSet; }));
                if (xchkCondition.Checked)
                    taskFlowTestHelper.TestCondition();
                //if (xchkSimulation.Checked)
                //taskFlowTestHelper.GenerateTaskChain();
            }).ContinueWith(t =>
            {
                Invoke(new Action(() =>
                {
                    xsbTest.Enabled = true;
                    xsbExport.Enabled = true;
                }));
            });
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
            var theUow = new UnitOfWork();
            var idList = new List<int>();
            var frm = new XFrmSearchCase(theUow, idList, false);
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
                var dataOfficialCameFile = theUow.ExecuteQuery(sSqlQueryOfficialCameFile);
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
    }
}