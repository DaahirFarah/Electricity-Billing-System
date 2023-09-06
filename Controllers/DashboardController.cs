using EBS.viewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EBS.Controllers
{
    public class DashboardController : Controller
    {
        // GET: Dashboard
        public ActionResult Index()
        {
            List<customerVM> data = GetData();
            return View(data);
        }


        private List<customerVM> GetData()
        {
            List<customerVM> customers = new List<customerVM>();



            return customers;
        }
    }
}