using System;
using System.Collections;
using DataEntities.TaskFlowData;

namespace TaskFlowTest.CalculateMethod
{
    public class Calculate
    {
        private ArrayList _operArrayList;
        private ArrayList _valueArrayList;
        private CalUtility _calUtility;
        private OperFactory _operFactory;
        private bool _bCheck;
        private string _sExpression;
        private TFTaskChain _taskChain;
        private TFNode _node;
        private object _objTriggerObject;

        /// <summary>
        /// 检测表达式
        /// </summary>
        /// <param name="sExpression"></param>
        private Calculate(string sExpression)
        {
            _sExpression = sExpression;
            _operArrayList = new ArrayList();
            _valueArrayList = new ArrayList();
            _calUtility = new CalUtility(_sExpression);
            _operFactory = new OperFactory();
            _bCheck = true; 
            _taskChain = null;
            _node = null;
            _objTriggerObject = null;
        }

        /// <summary>
        /// 计算表达式
        /// </summary>
        /// <param name="currentTaskChain"></param>
        /// <param name="node"></param>
        /// <param name="sExpression"></param>
        /// <param name="objTriggerObject"></param>
        private Calculate(TFTaskChain currentTaskChain, TFNode node, string sExpression, object objTriggerObject = null)
        {
            _sExpression = sExpression;
            _operArrayList = new ArrayList();
            _valueArrayList = new ArrayList();
            _calUtility = new CalUtility(_sExpression);
            _operFactory = new OperFactory();
            _bCheck = false;
            _taskChain = currentTaskChain;
            _node = node;
            _objTriggerObject = objTriggerObject;
        }

        private bool DoCal()
        {
            if (String.IsNullOrEmpty(_sExpression)) return true;//表达式为空时返回默认False
            string sTemp = _calUtility.GetItem();
            bool bLastIsValue = false;
            while (true)
            {
                if (_calUtility.IsValue(sTemp))
                {
                    _valueArrayList.Add(sTemp);
                    bLastIsValue = true;
                }
                else
                {
                    if (!bLastIsValue)
                    {
                        _operArrayList.Add(sTemp);
                    }
                    else
                    {
                        bLastIsValue = Cal(sTemp);
                    }
                }
                if (String.IsNullOrEmpty(sTemp)) break;
                sTemp = _calUtility.GetItem();
            }
            if (_valueArrayList.Count != 1)
            {
                var ex = new Exception();
                throw ex;
            }
            return Convert.ToBoolean(_valueArrayList[0].ToString());
        }

        private bool Cal(string sExpression)
        {
            if (String.IsNullOrEmpty(sExpression) && _operArrayList.Count == 0)
            {
                if (_valueArrayList.Count > 0)
                    _valueArrayList[_valueArrayList.Count - 1] = CalUtility.GetExpressionBoolValue(_valueArrayList[_valueArrayList.Count - 1].ToString().Trim(), _taskChain, _node, _objTriggerObject, _bCheck);
                return true;
            }
            if (_operArrayList.Count > 0)
            {
                if (_operArrayList[_operArrayList.Count - 1].ToString().Equals("(") && sExpression.Equals(")"))
                {
                    _operArrayList.RemoveAt(_operArrayList.Count - 1);
                    if (_operArrayList.Count > 0)
                    {
                        sExpression = _operArrayList[_operArrayList.Count - 1].ToString();
                        _operArrayList.RemoveAt(_operArrayList.Count - 1);
                    }
                    else
                    {
                        sExpression = String.Empty;
                    }
                    Cal(sExpression);
                    return true;
                }
                if (_calUtility.Compare(_operArrayList[_operArrayList.Count - 1].ToString(), sExpression))
                {
                    IOper p = _operFactory.CreateOper(_operArrayList[_operArrayList.Count - 1].ToString());
                    if (p.GetType() == typeof(OperNot))
                    {
                        bool b1 = CalUtility.GetExpressionBoolValue(_valueArrayList[_valueArrayList.Count - 1].ToString().Trim(), _taskChain, _node, _objTriggerObject, _bCheck);
                        bool b2 = CalUtility.GetExpressionBoolValue(_valueArrayList[_valueArrayList.Count - 1].ToString().Trim(), _taskChain, _node, _objTriggerObject, _bCheck);
                        _valueArrayList[_valueArrayList.Count - 1] = p.Oper(b1, b2);
                        _operArrayList.RemoveAt(_operArrayList.Count - 1);
                        return Cal(sExpression);
                    }
                    else
                    {
                        bool b1 = CalUtility.GetExpressionBoolValue(_valueArrayList[_valueArrayList.Count - 2].ToString().Trim(), _taskChain, _node, _objTriggerObject, _bCheck);
                        bool b2 = CalUtility.GetExpressionBoolValue(_valueArrayList[_valueArrayList.Count - 1].ToString().Trim(), _taskChain, _node, _objTriggerObject, _bCheck);
                        _valueArrayList[_valueArrayList.Count - 2] = p.Oper(b1, b2);
                        _operArrayList.RemoveAt(_operArrayList.Count - 1);
                        _valueArrayList.RemoveAt(_valueArrayList.Count - 1);
                        return Cal(sExpression);
                    }
                }
                if (!String.IsNullOrEmpty(sExpression)) _operArrayList.Add(sExpression);
            }
            else
            {
                if (!String.IsNullOrEmpty(sExpression)) _operArrayList.Add(sExpression);
            }
            return false;
        }

        /// <summary>
        /// 检测表达式
        /// </summary>
        /// <param name="sExpression"></param>
        /// <returns></returns>
        public static bool CheckCondition(string sExpression)
        {
            var calculate = new Calculate(CalUtility.ConvertToCode(sExpression));
            return calculate.DoCal();
        }

        /// <summary>
        /// 计算表达式
        /// </summary>
        /// <returns></returns>
        public static bool CalculateCondition(TFTaskChain currentTaskChain, TFNode node, string sExpression, object objTriggerObject = null)
        {
            var calculate = new Calculate(currentTaskChain, node, sExpression, objTriggerObject);
            return calculate.DoCal();
        }
    }
}