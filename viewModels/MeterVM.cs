using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBS.viewModels
{
    public class MeterVM
    {
        [Required(ErrorMessage = "Please Provide Meter ID")]
        [Display(Name = "Meter ID")]
        public int MeterID { get; set; }

        [Required(ErrorMessage = "Please Provide Customer ID")]
        [Display(Name = "Customer ID")]
        public int cID { get; set; }

        [Required(ErrorMessage = "Please Provide Locj Number")]
        [Display(Name = "Lock Number")]
        public int lockNumber { get; set; }

        [Required(ErrorMessage = "Please Provide Meter Type")]
        [Display(Name = "Meter Type")]
        public string Type { get; set; }
              
        [Required(ErrorMessage = "Please Provide Meter Status")]
        [Display(Name = "Meter Status")]
        public string Status { get; set; }
    }
}