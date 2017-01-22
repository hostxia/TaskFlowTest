namespace TaskFlowTest.CalculateMethod
{
    public class OperFactory
    {
        public IOper CreateOper(string Oper)
        {
            if (Oper.Equals("&"))
            {
                IOper p = new OperAnd();
                return p;
            }
            if (Oper.Equals("|"))
            {
                IOper p = new OperOr();
                return p;
            }
            if (Oper.Equals("="))
            {
                IOper p = new OperEqual();
                return p;
            }
            if (Oper.Equals("!"))
            {
                IOper p = new OperNot();
                return p;
            }
            return null;
        }
    }
}