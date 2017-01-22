using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using DataEntities.TaskFlowData;
using TaskFlowTest.ConditionMethod;
using TaskFlowTest.Utils;

namespace TaskFlowTest.CalculateMethod
{
    public class CalUtility
    {
        #region 表达式解析
        StringBuilder sbExpression;
        private int _iCurrent;
        private int _iCount;

        public CalUtility(string sExpression)
        {
            sbExpression = new StringBuilder(sExpression == null ? "" : sExpression.Trim());
            _iCount = sbExpression.Length;
        }

        public string GetItem()
        {
            if (_iCurrent == _iCount) return "";
            char chTemp = sbExpression[_iCurrent];
            while (chTemp == ' ')
            {
                _iCurrent++;
                chTemp = sbExpression[_iCurrent];
            }
            bool b = IsValue(chTemp);
            if (!b)
            {
                _iCurrent++;
                return chTemp.ToString();
            }
            string strTmp = "";
            while (IsValue(chTemp) == b && _iCurrent < _iCount && chTemp != ' ')
            {
                chTemp = sbExpression[_iCurrent];
                if (IsValue(chTemp) == b)
                    strTmp += chTemp;
                else
                    break;
                _iCurrent++;
            }
            return strTmp.Trim();
        }

        /// <summary>
        /// 判断字符是否为运算符
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public bool IsValue(char c)
        {
            if (c == '(' || c == ')' || c == '&' || c == '|' || c == '=' || c == '!')
            {
                return false;
            }
            return true;
        }
        public bool IsValue(string c)
        {
            if (c.Equals("")) return false;
            string sTemp = c[0].ToString();
            if (sTemp == "(" || sTemp == ")" || sTemp == "&" || sTemp == "|" || sTemp == "=" || sTemp == "!")
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 比较运算符优先级
        /// </summary>
        /// <param name="str1"></param>
        /// <param name="str2"></param>
        /// <returns></returns>
        public bool Compare(string str1, string str2)
        {
            if (str1 == ")") return true;
            if (str2 == "(") return false;
            return GetPriority(str1) >= GetPriority(str2);
        }

        /// <summary>
        /// 获取运算符优先级
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public int GetPriority(string str)
        {
            if (str.Equals(""))
            {
                return -1;
            }
            if (str.Equals("("))
            {
                return 0;
            }
            if (str.Equals("&") || str.Equals("|") || str.Equals("="))
            {
                return 3;
            }
            if (str.Equals("!"))
            {
                return 4;
            }
            if (str.Equals(")"))
            {
                return 0;
            }
            return 0;
        }

        /// <summary>
        /// 判断表达式中是否只有单个条件
        /// </summary>
        /// <param name="sExpression"></param>
        /// <returns></returns>
        public static bool IsSingleExpression(string sExpression)
        {
            sExpression = ConvertPunctuationCNToEN(sExpression);
            if (sExpression.Contains("(") || sExpression.Contains(")") || sExpression.Contains("=")
                || sExpression.Contains("&") || sExpression.Contains("|") || sExpression.Contains("!"))
                return false;
            if (sExpression.Contains(" ")) return false;
            return true;
        }
        #endregion

        #region 表达式代码化及逆转

        /// <summary>
        /// 将条件转换成编码
        /// </summary>
        /// <param name="enumConditionType"></param>
        /// <param name="sIPType"></param>
        /// <param name="sCondtionString"></param>
        /// <returns></returns>
        public static string ConvertToCode(string sCondtionString)
        {
            sCondtionString = ConvertPunctuationCNToEN(sCondtionString);
            var sbCondition = new StringBuilder(sCondtionString.Trim());
            int iCount = sbCondition.Length;
            var sbConditionCode = new StringBuilder();
            var sbTemp = new StringBuilder();
            for (int i = 0; i < iCount; i++)
            {
                if (!new[] { "&", "|", "!", "(", ")", " " }.Contains(sCondtionString[i].ToString()))
                {
                    sbTemp.Append(sCondtionString[i].ToString());
                }
                else
                {
                    if (sbTemp.Length > 0)
                    {
                        sbConditionCode.Append(SingleConvertToCode(sbTemp.ToString()));
                        sbTemp.Clear();
                    }
                    sbConditionCode.Append(sCondtionString[i]);
                }
            }
            if (sbTemp.Length > 0)
            {
                sbConditionCode.Append(SingleConvertToCode(sbTemp.ToString()));
                sbTemp.Clear();
            }
            return sbConditionCode.ToString();
        }

        /// <summary>
        /// 将编码转换成条件
        /// </summary>
        /// <param name="enumConditionType"></param>
        /// <param name="sIPType"></param>
        /// <param name="sCondtionCodeString"></param>
        /// <returns></returns>
        public static string ConvertToCondition(string sCondtionCodeString)
        {
            if (sCondtionCodeString == null) return "";
            var sbConditionCode = new StringBuilder(sCondtionCodeString.Trim());
            int iCount = sbConditionCode.Length;
            var sbCondition = new StringBuilder();
            var sbTemp = new StringBuilder();
            for (int i = 0; i < iCount; i++)
            {
                if (!new[] { "&", "|", "!", "(", ")", " " }.Contains(sbConditionCode[i].ToString()))
                {
                    sbTemp.Append(sbConditionCode[i].ToString());
                }
                else
                {
                    if (sbTemp.Length > 0)
                    {
                        sbCondition.Append(SingleConvertToCondition(sbTemp.ToString()));
                        sbTemp.Clear();
                    }
                    sbCondition.Append(sbConditionCode[i]);
                }
            }
            if (sbTemp.Length > 0)
            {
                sbCondition.Append(SingleConvertToCondition(sbTemp.ToString()));
                sbTemp.Clear();
                //sbCondition.Append(sbConditionCode[sbConditionCode.Length - 1]);
            }
            return sbCondition.ToString();
        }

        /// <summary>
        /// 将中文符号统一转成英文符号
        /// </summary>
        /// <param name="sCondtionString"></param>
        /// <returns></returns>
        private static string ConvertPunctuationCNToEN(string sCondtionString)
        {
            sCondtionString = sCondtionString.Replace('（', '(');
            sCondtionString = sCondtionString.Replace('）', ')');
            sCondtionString = sCondtionString.Replace('！', '!');
            sCondtionString = sCondtionString.Replace('【', '[');
            sCondtionString = sCondtionString.Replace('】', ']');
            sCondtionString = sCondtionString.Replace('，', ',');

            sCondtionString = sCondtionString.Replace("（", "(");
            sCondtionString = sCondtionString.Replace("）", ")");
            sCondtionString = sCondtionString.Replace("！", "!");
            sCondtionString = sCondtionString.Replace("，", ",");

            sCondtionString = sCondtionString.Replace("true", "True");
            sCondtionString = sCondtionString.Replace("false", "False");
            return sCondtionString;
        }

        /// <summary>
        /// 将单个条件条件转成单个编码
        /// </summary>
        /// <param name="enumConditionType"></param>
        /// <param name="sIPType"></param>
        /// <param name="sCondtionString"></param>
        /// <returns></returns>
        private static string SingleConvertToCode(string sCondtionString)
        {
            if (sCondtionString.Contains("[") && sCondtionString.Contains("]"))
            {
                string sConditionParameter = Regex.Match(sCondtionString, @"(?<=\[).*(?=])").Value;
                sCondtionString = Regex.Match(sCondtionString, @".*(?=\[.*])").Value + "[]";
                ConditionClassData.ListAllCondition.Where(c => c.s_Name == sCondtionString).ToList().ForEach(c => sCondtionString = sCondtionString.Replace(c.s_Name, c.s_Code));
                sCondtionString = sCondtionString.Insert(sCondtionString.IndexOf("[") + 1, sConditionParameter);
            }
            else
            {
                ConditionClassData.ListAllCondition.Where(c => c.s_Name == sCondtionString).ToList().ForEach(c => sCondtionString = sCondtionString.Replace(c.s_Name, c.s_Code));
            }
            return sCondtionString;
        }

        /// <summary>
        /// 将单个编码转成单个条件
        /// </summary>
        /// <param name="enumConditionType"></param>
        /// <param name="sIPType"></param>
        /// <param name="sCondtionCodeString"></param>
        /// <returns></returns>
        private static string SingleConvertToCondition(string sCondtionCodeString)
        {
            if (sCondtionCodeString.Contains("[") && sCondtionCodeString.Contains("]"))
            {
                string sConditionParameter = Regex.Match(sCondtionCodeString, @"(?<=\[).*(?=])").Value;
                sCondtionCodeString = Regex.Match(sCondtionCodeString, @".*(?=\[.*])").Value + "[]";
                ConditionClassData.ListAllCondition.Where(c => c.s_Code == sCondtionCodeString).ToList().ForEach(c => sCondtionCodeString = sCondtionCodeString.Replace(c.s_Code, c.s_Name));
                sCondtionCodeString = sCondtionCodeString.Insert(sCondtionCodeString.IndexOf("[") + 1, sConditionParameter);
            }
            else
            {
                ConditionClassData.ListAllCondition.Where(c => c.s_Code == sCondtionCodeString).ToList().ForEach(c => sCondtionCodeString = sCondtionCodeString.Replace(c.s_Code, c.s_Name));
            }
            return sCondtionCodeString;
        }
        #endregion

        #region 计算代码化表达式的返回值

        /// <summary>
        /// 获取各条件及存储过程的返回值
        /// </summary>
        /// <param name="sParameter"></param>
        /// <param name="taskChain"></param>
        /// <param name="node"></param>
        /// <param name="objTriggerObject"></param>
        /// <param name="bCheck"></param>
        /// <returns></returns>
        public static bool GetExpressionBoolValue(string sParameter, TFTaskChain taskChain, TFNode node, object objTriggerObject, bool bCheck = false)
        {
            if (sParameter == true.ToString() || sParameter == false.ToString()) return Convert.ToBoolean(sParameter);//处理True/False常量
            string sConditionCodeName;
            string sConditionParameter = null;
            if (sParameter.Contains("[") && sParameter.Contains("]"))
            {
                sConditionCodeName = Regex.Match(sParameter, @".*(?=\[.*])").Value + "[]";
                sConditionParameter = Regex.Match(sParameter, @"(?<=\[).*(?=])").Value;
            }
            else
            {
                sConditionCodeName = sParameter;
            }
            string sConditionMethodName = sConditionCodeName.Replace("[]", "");
            //获取各条件参数的返回值
            return Convert.ToBoolean(GetParameterBoolValue(sConditionMethodName, sConditionParameter, taskChain, node, objTriggerObject, bCheck));
        }

        /// <summary>
        /// 通过反射获取各参数条件的返回值
        /// </summary>
        /// <param name="sConditionName"></param>
        /// <param name="sConditionParameter"></param>
        /// <param name="taskChain"></param>
        /// <param name="node"></param>
        /// <param name="objTriggerObject"></param>
        /// <param name="bCheck"></param>
        /// <returns></returns>
        private static bool GetParameterBoolValue(string sConditionName, string sConditionParameter, TFTaskChain taskChain, TFNode node, object objTriggerObject, bool bCheck = false)
        {
            try
            {
                Assembly objAssembly = Assembly.Load("TaskFlowTest");
                var classInstance = objAssembly.CreateInstance("TaskFlowTest.ConditionMethod." + sConditionName);
                if (bCheck) return ((IConditionBase)classInstance).TestCondition(sConditionParameter);
                if (classInstance is IConditionTask) return ((IConditionTask)classInstance).Condition(GetTask(node), GetTaskChain(taskChain, node), sConditionParameter);
                if (classInstance is IConditionBaseCase) return ((IConditionBaseCase)classInstance).Condition(GetTask(node), GetBaseCase(taskChain, node, objTriggerObject), sConditionParameter);
                if (classInstance is IConditionInFile) return ((IConditionInFile)classInstance).Condition(GetTask(node), GetInFile(taskChain, node, objTriggerObject), sConditionParameter);
                throw new System.NotImplementedException();
                //Assembly objAssembly = Assembly.LoadFile(Path.Combine(Application.StartupPath, "TaskGeneration.dll"));
                //Type classType = objAssembly.GetType("TaskGeneration." + sConditionName);
                //var classInstance = classType.Assembly.CreateInstance("TaskGeneration." + sConditionName);
                //MethodInfo conditionMethodInfo = classType.GetMethod("Condition");
                //object objResult = conditionMethodInfo.Invoke(classInstance, new object[] { objParameter });
                //return Convert.ToBoolean(objResult);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取任务
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private static TFTask GetTask(TFNode node)
        {
            if (node == null || node.s_Type != Enums.NodeType.T.ToString()) return null;
            var task = node.GetTheOwnTask();
            if (task == null) node.GetTheOwnTaskInTransaction();
            return task;
        }

        /// <summary>
        /// 获取任务链
        /// </summary>
        /// <param name="currentTaskChain"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        private static TFTaskChain GetTaskChain(TFTaskChain currentTaskChain, TFNode node)
        {
            if (currentTaskChain != null) return currentTaskChain;
            return node != null ? node.GetTheTaskChain() : null;
        }

        /// <summary>
        /// 获取当前主相关案件
        /// </summary>
        /// <param name="currentTaskChain"></param>
        /// <param name="node"></param>
        /// <param name="objTriggerObject"></param>
        /// <returns></returns>
        public static DataEntities.Case.BasicCase GetBaseCase(TFTaskChain currentTaskChain, TFNode node, object objTriggerObject)
        {
            var baseCase = objTriggerObject as DataEntities.Case.BasicCase;
            if (baseCase != null) return baseCase;
            if (currentTaskChain != null)
            {
                var taskChain = GetTaskChain(currentTaskChain, node);
                if (taskChain != null) baseCase = taskChain.GetRelatedCase();
            }
            if (baseCase != null) return baseCase;
            var inFile = GetInFile(currentTaskChain, node, objTriggerObject);
            if (inFile != null) baseCase = inFile.TheMainCase;
            return baseCase;
        }

        /// <summary>
        /// 获取当前官方来文
        /// </summary>
        /// <param name="currentTaskChain"></param>
        /// <param name="node"></param>
        /// <param name="objTriggerObject"></param>
        /// <returns></returns>
        public static DataEntities.Element.Files.InFile GetInFile(TFTaskChain currentTaskChain, TFNode node, object objTriggerObject)
        {
            var inFile = objTriggerObject as DataEntities.Element.Files.InFile;
            if (inFile != null) return inFile;
            var taskChain = GetTaskChain(currentTaskChain, node);
            if (taskChain != null) inFile = taskChain.GetRelateInFile();
            return inFile;
        }

        #endregion
    }
}