using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using DataEntities.Case;
using DataEntities.TaskFlowConfig;
using DataEntities.TaskFlowData;
using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using DevExpress.XtraEditors;
using TaskFlowTest.Service_References.IPSPServices;

namespace TaskFlowTest
{
    public partial class XFrmMain : DevExpress.XtraEditors.XtraForm
    {
        public XFrmMain()
        {
            InitializeComponent();
        }

        private void XFrmMain_Load(object sender, EventArgs e)
        {
            xgridResult.DataSource = new TestResultInfoSet();
        }

        private void xbeTaskChain_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {

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
    }
}