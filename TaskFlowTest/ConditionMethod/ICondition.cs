using DataEntities.Case;
using DataEntities.Element.Files;
using DataEntities.TaskFlowData;

namespace TaskFlowTest.ConditionMethod
{
    public interface IConditionBase
    {
        bool TestCondition(string sConditionParameter);
    }

    public interface IConditionTask : IConditionBase
    {
        bool Condition(TFTask task, TFTaskChain taskChain, string sConditionParameter);
    }

    public interface IConditionBaseCase : IConditionBase
    {
        bool Condition(TFTask task, BasicCase baseCase, string sConditionParameter);
    }

    public interface IConditionInFile : IConditionBase
    {
        bool Condition(TFTask task, InFile inFile, string sConditionParameter);
    }
}