namespace TaskFlowTest.CalculateMethod
{
    public class OperAnd : IOper
    {
        public object Oper(bool b1, bool b2)
        {
            return b1 && b2;
        }
    }

    public class OperOr : IOper
    {
        public object Oper(bool b1, bool b2)
        {
            return b1 || b2;
        }
    }

    public class OperEqual : IOper
    {
        public object Oper(bool b1, bool b2)
        {
            return b1 == b2;
        }
    }

    public class OperNot : IOper
    {
        public object Oper(bool b1, bool b2)
        {
            return !b1;
        }
    }
}