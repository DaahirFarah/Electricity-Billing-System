using EBS.Models;
using EBS.viewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EBS.Controllers
{
    [AllowAnonymous]
    public class LoginController : Controller
    {
        private readonly UserAuth userRepository = new UserAuth();

        // Display the login form
        public ActionResult Login()
        {
            return View();
        }

        // Process the login request
        [HttpPost]
        public ActionResult Login(UserVm model, string returnUrl)
        {
            if (ModelState.IsValid && userRepository.AuthenticateUser(model.Username, model.Password))
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                else
                {
                    return RedirectToAction("Index", "Dashboard");
                }
            }

            ModelState.AddModelError("", "Invalid username or password");
            return View(model);
        }

        // Logout action
        public ActionResult Logout()
        {
            userRepository.SignOut();
            return RedirectToAction("Login");
        }
    }
}