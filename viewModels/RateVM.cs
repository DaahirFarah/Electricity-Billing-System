﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBS.viewModels
{
    public class RateVM
    {
        [Required(ErrorMessage = "Rate ID is required")]
        [Display(Name = "Rate ID")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Please Insert Usage Level Name")]
        [Display(Name = "Usage Level Name")]
        public string UsageLevelName { get; set; }

        [Required(ErrorMessage = "Please Insert Usage Level Range Start ")]
        [Display(Name = "Usage Level Starts (Kw/H)")]
        public int UsageLevelNumberStarts { get; set; }
        
        [Required(ErrorMessage = "Please Insert Usage Level Range End")]
        [Display(Name = "Usage Level Ends (Kw/H)")]
        public int UsageLevelNumberEnds { get; set; }

        [Required(ErrorMessage = "Please Provide the Rate")]
        [Display(Name = "Rate")]
        public decimal Rate { get; set; }


        [Display(Name = "Rate ID")]
        public int RateID { get; set; }

        [Display(Name = "Fixed Fee ($)")]
        public int SpecialFixedFee { get; set; }

        [Display(Name = "Special Rate ($)")]
        public decimal SpecialRate { get; set; }
    }
}