using DevExpress.Xpo.DB;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.Xpo;

namespace TaskFlowTest
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            XpoDefault.DataLayer = XpoDefault.GetDataLayer(ConfigurationManager.ConnectionStrings["Connection"].ConnectionString, AutoCreateOption.None);
            DevExpress.Data.CurrencyDataController.DisableThreadingProblemsDetection = true;
            Application.Run(new XFrmMain());
        }
    }
}
