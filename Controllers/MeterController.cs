using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

namespace EBS.Controllers
{
    public class MeterController : Controller
    {
        // GET: Meter
        public ActionResult Index()
        {
            return View();
        }

        //GET: Register Meter
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        //SET: Register Meter
        [ValidateAntiForgeryToken]
        [HttpPost]
        [Display(Name = "Create")]
        public ActionResult CreateConfirm()
        {
            return View();
        }


        //GET: Update Meter
        public ActionResult Edit()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Display(Name = "Edit")]
        public ActionResult EditConfirm()
        {
            return View();
        }

        
    }
}