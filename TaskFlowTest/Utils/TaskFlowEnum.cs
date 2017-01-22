namespace TaskFlowTest.Utils
{
    public class TaskFlowEnum
    {
        public enum RelatedObjectType
        {
            Client,//登记客户       
            Case,//登记案件
            CameFileOfficial,//登记官方来文
        }

        public enum OperationType
        {
            RollBack,//回滚
        }
    }
}