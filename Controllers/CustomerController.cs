using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EBS.Controllers
{
    public class CustomerController : Controller
    {
        // GET: Customer
        public ActionResult Index()
        {
            return View();
        }

        //GET: Register Customer
        [HttpGet]
        public ActionResult Create() 
        {
            return View();
        }

        //SET: Register Customer
        [ValidateAntiForgeryToken]
        [HttpPost]
        [Display(Name = "Create")]
        public ActionResult CreateConfirm()
        {
            return View();
        }


        //GET: Update Customer
        public ActionResult Edit()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Display(Name = "Edit")]
        public ActionResult EditConfirmConfirm()
        {
            return View();
        }
    }
}