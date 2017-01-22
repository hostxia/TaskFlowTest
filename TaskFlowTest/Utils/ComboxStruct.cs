
using System;
using System.Data;
using System.Reflection;
using DevExpress.Xpo;
namespace TaskFlowTest.Utils
{
    public partial class ComboxStruct
    {
        public ComboxStruct(string displayMember, string displayValue)
        {
            DisplayMember = displayMember;
            DisplayValue = displayValue;
        }
        public string DisplayMember { get; set; }
        public string DisplayValue { get; set; }

        public static ComboxStruct[] RelatedObjectType =
{
            new ComboxStruct("案件", TaskFlowEnum.RelatedObjectType.Case.ToString()),
            new ComboxStruct("官方来文", TaskFlowEnum.RelatedObjectType.CameFileOfficial.ToString()),
        };
    }
}