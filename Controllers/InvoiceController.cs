using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

namespace EBS.Controllers
{
    public class InvoiceController : Controller
    {
        // GET: Invoice
        public ActionResult Index()
        {
            return View();
        }

        //GET: Register Invoice
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        //SET: Register Invoice
        [ValidateAntiForgeryToken]
        [HttpPost]
        [Display(Name = "Create")]
        public ActionResult CreateConfirm()
        {
            return View();
        }


        //GET: Update Invoice
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