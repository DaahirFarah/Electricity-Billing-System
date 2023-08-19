using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EBS.Controllers
{
    public class UserController : Controller
    {
        // GET: User
        public ActionResult Index()
        {
            return View();
        }

        // GET: Create User
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }


        // SET: Create User
        [ValidateAntiForgeryToken]
        [HttpPost]
        [Display(Name = "Create")]
        public ActionResult CreateConfirmed()
        {
            return View();
        }

        // GET: Edit User
        [HttpGet]
        public ActionResult Edit()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Display(Name = "Edit")]
        public ActionResult EditConfirmed()
        {
            return View();
        }
    }
}