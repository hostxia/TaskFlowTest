using DataEntities.Config;
using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;

namespace TaskFlowTest.Utils
{
    public class IPSXPCollections
    {
        public static XPCollection<CodeCountry> Countries => new XPCollection<CodeCountry>(new UnitOfWork())
        {
            DisplayableProperties = "n_ID;s_Name;s_CountryCode",
            Sorting =
                new SortingCollection(new SortProperty("n_FrequentNo", SortingDirection.Ascending),
                    new SortProperty("s_Name", SortingDirection.Ascending))
        };

        public static XPCollection<CodeBusinessType> GetPatentBusinessTypes(string sIPType)
        {
            return new XPCollection<CodeBusinessType>(new UnitOfWork(), CriteriaOperator.Parse("s_IPType=?", sIPType));
        }
    }


}