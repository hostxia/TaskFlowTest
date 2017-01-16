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

namespace TaskFlowTest
{
    public partial class XFrmMain : DevExpress.XtraEditors.XtraForm
    {
        private UnitOfWork _unitOfWork;

        private IPSPServices.ServiceClient _serviceClient;

        public XFrmMain()
        {
            InitializeComponent();
        }

        private void XFrmMain_Load(object sender, EventArgs e)
        {
            _unitOfWork = new UnitOfWork();
        }

        private void xbeTaskChain_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {

        }

        private void xsbTest_Click(object sender, EventArgs e)
        {
            if (xchkCondition.Checked)
                TestCondition();
        }

        private void TestCondition()
        {
            //var assembly = Assembly.LoadFrom("IPSpacePlusClient/TaskFlowGenerator.dll");
            //var type = assembly.GetType("TaskFlowGenerator.CalculateMethod.Calculate");
            //var methodInfo = type.GetMethod("CheckCondition", BindingFlags.Static | BindingFlags.Instance);
            //var xpcNodeRelation = new XPCollection<TFCode_NodeRelation>(_unitOfWork);
            //foreach (var nodeRelation in xpcNodeRelation)
            //{
            //    object[] objPara = new object[1];
            //    objPara[0] = nodeRelation.s_Condition;
            //    var objInstanse = assembly.CreateInstance("TaskFlowGenerator.CalculateMethod.Calculate", true, BindingFlags.Instance | BindingFlags.NonPublic, null, objPara, null, null);
            //    var bIsRight = (bool)methodInfo.Invoke(null, objPara);
            //}
        }

        private void GenerateTaskChain()
        {
            _serviceClient = new IPSPServices.ServiceClient();
            //TriggerType: CaseSubmit, Case, CameFileOfficial
            //TriggerObjectFullName: DataEntities.Case.BasicCase
            var testCase = _unitOfWork.GetObjectByKey<BasicCase>(1);

            var htCustomCondition = new List<List<KeyValuePair<string, string>>>();
            CodeTaskRecursion(Guid.Empty, ref htCustomCondition);

            var result = new List<List<KeyValuePair<string, string>>>();
            GetFlowCustomCondtion(htCustomCondition[0], htCustomCondition[1], 1, htCustomCondition, ref result);

            foreach (var conditionGroup in result)
            {
                _serviceClient.ByServerAutoGenerateTopTaskChains("Case", "DataEntities.Case.BasicCase", 1, true);
            }
        }

        private void GetFlowCustomCondtion(List<KeyValuePair<string, string>> listValue1, List<KeyValuePair<string, string>> listValue2, int nIndex, List<List<KeyValuePair<string, string>>> htCustomCondition, ref List<List<KeyValuePair<string, string>>> result)
        {
            if (nIndex < htCustomCondition.Count)
            {
                result = new List<List<KeyValuePair<string, string>>>();
                for (int i = 0; i < listValue1.Count; i++)
                {
                    for (int j = 0; j < listValue2.Count; i++)
                    {
                        var listKeyValuePair = new List<KeyValuePair<string, string>>();
                        listKeyValuePair.Add(new KeyValuePair<string, string>(listValue1[i].Key, listValue1[i].Value));
                        listKeyValuePair.Add(new KeyValuePair<string, string>(listValue2[j].Key, listValue2[j].Value));
                        result.Add(listKeyValuePair);
                    }
                }

                GetFlowCustomCondtion(result[result.Count - 1], htCustomCondition[nIndex + 1], nIndex + 1, htCustomCondition, ref result);
            }
        }

        private void CodeTaskRecursion(Guid gCodeTaskChainID, ref List<List<KeyValuePair<string, string>>> htCustomCondition)
        {
            var codeTaskChain = _unitOfWork.GetObjectByKey<TFCodeTaskChain>(gCodeTaskChainID);

            foreach (var t in codeTaskChain.GetlistCodeTasks())
            {
                foreach (var aik in t.ListCodeActionInCodeTasks.Where(aik => aik.n_CodeActionID == 25).ToList())
                {
                    if (!htCustomCondition.Any(lk => lk.Select(k => k.Key).Contains(aik.s_ParamIn)))
                    {
                        var listKeyValuePair = new List<KeyValuePair<string, string>>();
                        foreach (var value in _unitOfWork.FindObject<TFCodeCustomCondition>(CriteriaOperator.Parse("s_Code = ?", aik.s_ParamIn)).s_Values.Split(',').ToList())
                        {
                            listKeyValuePair.Add(new KeyValuePair<string, string>(aik.s_ParamIn, value));
                        }
                        htCustomCondition.Add(listKeyValuePair);
                    }
                }
            }
            foreach (var c in codeTaskChain.GetListCodeTaskChains())
            {
                CodeTaskRecursion(c.g_ID, ref htCustomCondition);
            }
        }

        private void FinishTaskRecursion(int nCaseID)
        {
            var basicCase = new UnitOfWork().GetObjectByKey<BasicCase>(nCaseID);
            var listTaskChains = basicCase.GetRelateTaskChains();
            var listTasks = listTaskChains.SelectMany(c => c.GetListNodes().Select(n => n.GetTheOwnTask())).Where(t => t.s_State == "P" || t.s_State == "O").ToList();
            while (listTasks.Count > 0)
            {
                foreach (var task in listTasks)
                {
                    var tfNode = task.GetTheBelongNode();
                    var sFinishCondition = tfNode.GetTheCodeNode().GetTheOwnCodeTask().s_FinishCondition;
                    if (sFinishCondition.Contains("CTaskExistParamTye"))
                    {
                        if (task.GetListTaskParameters().All(p => p.s_ParaType != "TaskCheck"))
                        {
                            var bResultN = task.GetListItselfTaskChecks().Any(c => c.s_CheckReselt == "N");
                            //TODO 找到被审任务ID
                            var taskCheck = new TFTaskCheck(_unitOfWork)
                            {
                                g_ID = Guid.NewGuid(),
                                g_TaskID = task.g_ID,
                                n_Sequence = task.GetListItselfTaskChecks().Max(c => c.n_Sequence) + 1,
                                g_CheckTaskID = task.g_ID,
                                s_CheckReselt = bResultN ? "Y" : "N"
                            };
                            var taskParameter = new TFTaskParameter(_unitOfWork)
                            {
                                g_ID = Guid.NewGuid(),
                                g_TaskID = task.g_ID,
                                g_TaskChainID = task.GetTheTaskChain().g_ID,
                                n_CodeActionID = 6,
                                s_ParaType = "TaskCheck",
                                s_ParaValue = taskCheck.g_ID.ToString()
                            };
                            taskParameter.Save();
                            _unitOfWork.CommitChanges();
                        }
                    }
                    var operationInfo = _serviceClient.ByServerFinishTaskNode(tfNode.g_ID, 131);
                    if (!operationInfo.bOperationResult)
                    {
                        //TODO： 输出信息
                        return;
                    }

                }
            }
        }
    }
}