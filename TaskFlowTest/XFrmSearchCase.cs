using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DataEntities.Case;
using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using TaskFlowTest.Service_References.IPSPServices;
using TaskFlowTest.Utils;

namespace TaskFlowTest
{
    public partial class XFrmSearchCase : XtraForm
    {
        private readonly List<int> _idList;
        private readonly ServiceClient _serviceClient;
        private readonly UnitOfWork _theUow;

        public XFrmSearchCase(UnitOfWork uow, List<int> idList, bool bMultiSelected,ServiceClient serviceClient)
        {
            InitializeComponent();
            Cases = new List<BasicCase>();
            _serviceClient = serviceClient;
            InitControls();

            _theUow = uow;
            _idList = idList;
            xgridViewPatentCase.OptionsSelection.MultiSelect = bMultiSelected;
            xgridViewTrademarkCase.OptionsSelection.MultiSelect = bMultiSelected;
            xgridViewCopyrightCase.OptionsSelection.MultiSelect = bMultiSelected;
            xgridViewDomainNameCase.OptionsSelection.MultiSelect = bMultiSelected;
            xgridViewOtherCase.OptionsSelection.MultiSelect = bMultiSelected;
        }

        public IList<BasicCase> Cases { get; private set; }

        private void xbtCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void xbtSelect_Click(object sender, EventArgs e)
        {
            GetSelectedCase();
            DialogResult = DialogResult.OK;
        }

        private void xbtPatentSearch_Click(object sender, EventArgs e)
        {
            var xpCases = SearchPatentCases(_theUow, _idList, xtePCaseSerial.Text.Trim(), xtePClientSerial.Text.Trim(),
                xtePCaseName.Text.Trim(), xtePAppNo.Text.Trim(), xtePClientCode.Text.Trim());
            xgridPatentCase.DataSource = xpCases;
            xgridViewPatentCase.RefreshData();
        }

        private void xbtTrademarkSearch_Click(object sender, EventArgs e)
        {
            var xpCases = SearchTrademarkCases(_theUow, _idList, xteTCaseSerial.Text, xteTClientSerial.Text,
                xteTCaseName.Text, xteTAppNo.Text, xteTClientName.Text);
            xgridTrademarkCase.DataSource = xpCases;
            xgridViewTrademarkCase.RefreshData();
        }

        private void xbtCopyrightSearch_Click(object sender, EventArgs e)
        {
            var xpCases = SearchCopyrightCases(_theUow, _idList, xteCCaseSerial.Text, xteCClientSerial.Text,
                xteCCaseName.Text, xteCClientName.Text);
            xgridCopyrightCase.DataSource = xpCases;
            xgridViewCopyrightCase.RefreshData();
        }

        private void xbtDomainNameSearch_Click(object sender, EventArgs e)
        {
            var xpCases = SearchDomainNameCases(_theUow, _idList, xteDCaseSerial.Text, xteDClientSerial.Text,
                xteDCaseName.Text, xteDClientName.Text);
            xgridDomainNameCase.DataSource = xpCases;
            xgridViewDomainNameCase.RefreshData();
        }

        private void xbtOtherCaseSearch_Click(object sender, EventArgs e)
        {
            var xpCases = SearchOtherCases(_theUow, _idList, xteOCaseSerial.Text, xteOClientSerial.Text,
                xteOCaseName.Text, xteOClientName.Text);
            xgridOtherCase.DataSource = xpCases;
            xgridViewOtherCase.RefreshData();
        }

        private void XFrmSearchCase_Load(object sender, EventArgs e)
        {
        }

        private void xgridCase_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right) return;


            if (xtabSearchPanel.SelectedTabPage == xtabPagePatent)
            {
                var hitInfo = xgridViewPatentCase.CalcHitInfo(e.Location);
                if (hitInfo.HitTest == GridHitTest.RowIndicator || hitInfo.HitTest == GridHitTest.Column
                    || hitInfo.HitTest == GridHitTest.Footer || hitInfo.HitTest == GridHitTest.RowEdge
                    || hitInfo.HitTest == GridHitTest.ColumnEdge)
                    return;

                GetSelectedCaseFromGrid(xgridViewPatentCase);
            }
            else if (xtabSearchPanel.SelectedTabPage == xtabPageTrademark)
            {
                var hitInfo = xgridViewTrademarkCase.CalcHitInfo(e.Location);
                if (hitInfo.HitTest == GridHitTest.RowIndicator || hitInfo.HitTest == GridHitTest.Column
                    || hitInfo.HitTest == GridHitTest.Footer || hitInfo.HitTest == GridHitTest.RowEdge
                    || hitInfo.HitTest == GridHitTest.ColumnEdge)
                    return;

                GetSelectedCaseFromGrid(xgridViewTrademarkCase);
            }
            else if (xtabSearchPanel.SelectedTabPage == xtabPageCopyright)
            {
                var hitInfo = xgridViewCopyrightCase.CalcHitInfo(e.Location);
                if (hitInfo.HitTest == GridHitTest.RowIndicator || hitInfo.HitTest == GridHitTest.Column
                    || hitInfo.HitTest == GridHitTest.Footer || hitInfo.HitTest == GridHitTest.RowEdge
                    || hitInfo.HitTest == GridHitTest.ColumnEdge)
                    return;

                GetSelectedCaseFromGrid(xgridViewCopyrightCase);
            }
            else if (xtabSearchPanel.SelectedTabPage == xtabPageDomainName)
            {
                var hitInfo = xgridViewDomainNameCase.CalcHitInfo(e.Location);
                if (hitInfo.HitTest == GridHitTest.RowIndicator || hitInfo.HitTest == GridHitTest.Column
                    || hitInfo.HitTest == GridHitTest.Footer || hitInfo.HitTest == GridHitTest.RowEdge
                    || hitInfo.HitTest == GridHitTest.ColumnEdge)
                    return;

                GetSelectedCaseFromGrid(xgridViewDomainNameCase);
            }
            else
            {
                var hitInfo = xgridViewOtherCase.CalcHitInfo(e.Location);
                if (hitInfo.HitTest == GridHitTest.RowIndicator || hitInfo.HitTest == GridHitTest.Column
                    || hitInfo.HitTest == GridHitTest.Footer || hitInfo.HitTest == GridHitTest.RowEdge
                    || hitInfo.HitTest == GridHitTest.ColumnEdge)
                    return;

                GetSelectedCaseFromGrid(xgridViewOtherCase);
            }

            DialogResult = DialogResult.OK;
        }

        private void InitControls()
        {
            xluePCountry.DataSource =
                xlueTCountry.DataSource =
                    xlueCCountry.DataSource =
                        xlueDCountry.DataSource = xlueOCountry.DataSource = IPSXPCollections.Countries;

            xluePCountry.DisplayMember =
                xlueTCountry.DisplayMember =
                    xlueCCountry.DisplayMember = xlueDCountry.DisplayMember = xlueOCountry.DisplayMember = "s_Name";
            xluePCountry.ValueMember =
                xlueTCountry.ValueMember =
                    xlueCCountry.ValueMember = xlueDCountry.ValueMember = xlueOCountry.ValueMember = "n_ID";

            xluePatentBusinessType.DataSource = IPSXPCollections.GetPatentBusinessTypes("P");
            xluePatentBusinessType.DisplayMember = "s_Name";
            xluePatentBusinessType.ValueMember = "n_ID";

            xlueTrademarkBusinessType.DataSource = IPSXPCollections.GetPatentBusinessTypes("T");
            xlueTrademarkBusinessType.DisplayMember = "s_Name";
            xlueTrademarkBusinessType.ValueMember = "n_ID";

            xlueCopyrightBusinessType.DataSource = IPSXPCollections.GetPatentBusinessTypes("C");
            xlueCopyrightBusinessType.DisplayMember = "s_Name";
            xlueCopyrightBusinessType.ValueMember = "n_ID";
            xlueDomainNameBusinessType.DataSource = IPSXPCollections.GetPatentBusinessTypes("D");
            xlueDomainNameBusinessType.DisplayMember = "s_Name";
            xlueDomainNameBusinessType.ValueMember = "n_ID";
            xlueOtherCaseBusinessType.DataSource = IPSXPCollections.GetPatentBusinessTypes("U");
            xlueOtherCaseBusinessType.DisplayMember = "s_Name";
            xlueOtherCaseBusinessType.ValueMember = "n_ID";
        }

        private void GetSelectedCase()
        {
            if (xtabSearchPanel.SelectedTabPage == xtabPagePatent)
                GetSelectedCaseFromGrid(xgridViewPatentCase);
            else if (xtabSearchPanel.SelectedTabPage == xtabPageTrademark)
                GetSelectedCaseFromGrid(xgridViewTrademarkCase);
            else if (xtabSearchPanel.SelectedTabPage == xtabPageCopyright)
                GetSelectedCaseFromGrid(xgridViewCopyrightCase);
            else if (xtabSearchPanel.SelectedTabPage == xtabPageDomainName)
                GetSelectedCaseFromGrid(xgridViewDomainNameCase);
            else
                GetSelectedCaseFromGrid(xgridViewOtherCase);
        }

        private void GetSelectedCaseFromGrid(GridView xgridView)
        {
            IList<int> casesId =
                xgridView.GetSelectedRows().Select(h => (int) ((dynamic) xgridView.GetRow(h)).n_CaseID).ToList();
            if (casesId.Count == 0) return;

            Cases = new XPCollection<BasicCase>(_theUow, new InOperator("n_CaseID", casesId)).ToList();
        }

        public List<dynamic> SearchPatentCases(UnitOfWork uow, List<int> excludedCaseIDs, string caseSerial,
            string clientSerial, string caseName, string applicationNum, string clientName)
        {
            var strSearchCondition = new StringBuilder("1 = 1");
            strSearchCondition.AppendFormat(
                " AND TCase_Base.s_CaseSerial LIKE '%{0}%' AND TCase_Base.s_ClientSerial LIKE '%{1}%' AND TCase_Base.s_CaseName LIKE '%{2}%'",
                caseSerial.Trim(), clientSerial.Trim(), caseName.Trim());

            if (applicationNum.Trim() != string.Empty)
                strSearchCondition.AppendFormat(" AND TPCase_LawInfo.s_AppNo LIKE '%{0}%'", applicationNum.Trim());
            if (clientName.Trim() != string.Empty)
                strSearchCondition.AppendFormat(" AND TCstmr_Client.s_Name LIKE '%{0}%'", clientName.Trim());
            if (excludedCaseIDs.Count > 0)
                strSearchCondition.AppendFormat(" AND TCase_Base.n_CaseID NOT IN ({0})",
                    string.Join(",", excludedCaseIDs));

            var sFields = string.Join(",",
                xgridViewPatentCase.Columns.Cast<GridColumn>().Where(c => c.Visible).Select(c => c.FieldName));
            var listPatents = _serviceClient.GetPatents(strSearchCondition.ToString(), "TCase_Base.s_CaseSerial",
                sFields, false);
            return listPatents.Cast<dynamic>().ToList();
        }

        public List<dynamic> SearchTrademarkCases(UnitOfWork uow, List<int> excludedCaseIDs, string caseSerial,
            string clientSerial, string caseName, string applicationNum, string clientName)
        {
            var strSearchCondition = new StringBuilder("1 = 1");
            strSearchCondition.AppendFormat(
                " AND TCase_Base.s_CaseSerial LIKE '%{0}%' AND TCase_Base.s_ClientSerial LIKE '%{1}%' AND TCase_Base.s_CaseName LIKE '%{2}%'",
                caseSerial.Trim(), clientSerial.Trim(), caseName.Trim());

            if (applicationNum.Trim() != string.Empty)
                strSearchCondition.AppendFormat(" AND TTCase_LawInfo.s_AppNo LIKE '%{0}%'", applicationNum.Trim());
            if (clientName.Trim() != string.Empty)
                strSearchCondition.AppendFormat(" AND TCstmr_Client.s_Name LIKE '%{0}%'", clientName.Trim());
            if (excludedCaseIDs.Count > 0)
                strSearchCondition.AppendFormat(" AND TCase_Base.n_CaseID NOT IN ({0})",
                    string.Join(",", excludedCaseIDs));

            var sFields = string.Join(",", xgridViewTrademarkCase.Columns.Where(c => c.Visible).Select(c => c.FieldName));
            var listTrademarks = _serviceClient.GetTrademarks(strSearchCondition.ToString(), "TCase_Base.s_CaseSerial",
                sFields, false);
            return listTrademarks.Cast<dynamic>().ToList();
        }

        public List<dynamic> SearchCopyrightCases(UnitOfWork uow, List<int> excludedCaseIDs, string caseSerial,
            string clientSerial, string caseName, string clientName)
        {
            var strSearchCondition = new StringBuilder("1 = 1");
            strSearchCondition.AppendFormat(
                " AND TCase_Base.s_CaseSerial LIKE '%{0}%' AND TCase_Base.s_ClientSerial LIKE '%{1}%' AND TCase_Base.s_CaseName LIKE '%{2}%'",
                caseSerial.Trim(), clientSerial.Trim(), caseName.Trim());

            if (clientName.Trim() != string.Empty)
                strSearchCondition.AppendFormat(" AND TCstmr_Client.s_Name LIKE '%{0}%'", clientName.Trim());
            if (excludedCaseIDs.Count > 0)
                strSearchCondition.AppendFormat(" AND TCase_Base.n_CaseID NOT IN ({0})",
                    string.Join(",", excludedCaseIDs));

            var sFields = string.Join(",",
                xgridViewCopyrightCase.Columns.Cast<GridColumn>().Where(c => c.Visible).Select(c => c.FieldName));
            var listCopyrights = _serviceClient.GetCopyRights(strSearchCondition.ToString(), "TCase_Base.s_CaseSerial",
                sFields, false);
            return listCopyrights.Cast<dynamic>().ToList();
        }

        public List<dynamic> SearchDomainNameCases(UnitOfWork uow, List<int> excludedCaseIDs, string caseSerial,
            string clientSerial, string caseName, string clientName)
        {
            var strSearchCondition = new StringBuilder("1 = 1");
            strSearchCondition.AppendFormat(
                " AND TCase_Base.s_CaseSerial LIKE '%{0}%' AND TCase_Base.s_ClientSerial LIKE '%{1}%' AND TCase_Base.s_CaseName LIKE '%{2}%'",
                caseSerial.Trim(), clientSerial.Trim(), caseName.Trim());

            if (clientName.Trim() != string.Empty)
                strSearchCondition.AppendFormat(" AND TCstmr_Client.s_Name LIKE '%{0}%'", clientName.Trim());
            if (excludedCaseIDs.Count > 0)
                strSearchCondition.AppendFormat(" AND TCase_Base.n_CaseID NOT IN ({0})",
                    string.Join(",", excludedCaseIDs));

            var sFields = string.Join(",",
                xgridViewDomainNameCase.Columns.Where(c => c.Visible).Select(c => c.FieldName));
            var listDomains = _serviceClient.GetDomains(strSearchCondition.ToString(), "TCase_Base.s_CaseSerial",
                sFields, false);
            return listDomains.Cast<dynamic>().ToList();
        }

        public List<dynamic> SearchOtherCases(UnitOfWork uow, List<int> excludedCaseIDs, string caseSerial,
            string clientSerial, string caseName, string clientName)
        {
            var strSearchCondition = new StringBuilder("1 = 1");
            strSearchCondition.AppendFormat(
                " AND TCase_Base.s_CaseSerial LIKE '%{0}%' AND TCase_Base.s_ClientSerial LIKE '%{1}%' AND TCase_Base.s_CaseName LIKE '%{2}%'",
                caseSerial.Trim(), clientSerial.Trim(), caseName.Trim());

            if (clientName.Trim() != string.Empty)
                strSearchCondition.AppendFormat(" AND TCstmr_Client.s_Name LIKE '%{0}%'", clientName.Trim());
            if (excludedCaseIDs.Count > 0)
                strSearchCondition.AppendFormat(" AND TCase_Base.n_CaseID NOT IN ({0})",
                    string.Join(",", excludedCaseIDs));

            var sFields = string.Join(",",
                xgridViewOtherCase.Columns.Cast<GridColumn>().Where(c => c.Visible).Select(c => c.FieldName));
            var listOtherCases = _serviceClient.GetOtherCases(strSearchCondition.ToString(), "TCase_Base.s_CaseSerial",
                sFields, false);
            return listOtherCases.Cast<dynamic>().ToList();
        }
    }
}