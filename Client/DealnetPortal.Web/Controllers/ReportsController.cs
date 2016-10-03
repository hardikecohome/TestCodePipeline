using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Globalization;
using System.Threading.Tasks;
using System.Web.Mvc;
using DealnetPortal.Web.Infrastructure;

namespace DealnetPortal.Web.Controllers
{
    [AuthFromContext]
    public class ReportsController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult WorkItems()
        {
            return this.Json(new[]
            {

                new[] { "1654", "User1", "Waiting on Documents","1654", "user1@email.com", "555-555-1234", "2015/10/01"},
                new[] { "1655", "User2", "Waiting on Documents","1655", "user2@email.com", "555-555-1234", "2015/10/01"},
                new[] { "1655", "User3", "Waiting on Documents","1656", "user3@email.com", "555-555-1234", "2015/10/01"},

            }, JsonRequestBehavior.AllowGet);
        }

        //[HttpGet]

        //public ActionResult DealFlowOverview(DealFlowType type)
        //{
        //    var rand = new Random();
        //    var labels = new List<string>();
        //    var datasets = new List<object>();
        //    var data = new List<int>();
        //    switch (type)
        //    {
        //        case DealFlowType.Month:
        //            {
        //                for (int i = 1; i < DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month); i++)
        //                {
        //                    data.Add(rand.Next(1, 100));
        //                    labels.Add(i.ToString());
        //                }
        //                datasets.Add(new {data });
        //                break;
        //            }
        //        case DealFlowType.Week:
        //            {
        //                foreach (var day in DateTimeFormatInfo.CurrentInfo.DayNames)
        //                {
        //                    data.Add(rand.Next(1, 100));
        //                    labels.Add(day);
        //                }
        //                datasets.Add(new {data });
        //                break;
        //            }
        //        case DealFlowType.Year:
        //            {
        //                foreach (var month in DateTimeFormatInfo.CurrentInfo.MonthNames)
        //                {
        //                    if (string.IsNullOrEmpty(month))
        //                    {
        //                        continue;
        //                    }
        //                    data.Add(rand.Next(1, 100));
        //                    labels.Add(month);
        //                }
        //                datasets.Add(new {data });
        //                break;
        //            }
        //    }

        //    return this.Json(new { labels, datasets }, JsonRequestBehavior.AllowGet);
        //}


        //public enum DealFlowType
        //{
        //    Week,
        //    Month,
        //    Year
        //}
    }
}
