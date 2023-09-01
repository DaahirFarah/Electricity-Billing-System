using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBS.viewModels
{
    public class UserVm
    {
        [Required(ErrorMessage = "Please Enter Your Username")]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Please Enter Your password")]
        [Display(Name = "Password")]
        public string Password { get; set; }
    }
}