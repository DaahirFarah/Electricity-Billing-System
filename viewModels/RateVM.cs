using System;
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

        [Required(ErrorMessage = "Please Insert Usage Level Amount")]
        [Display(Name = "Usage Amount (Kw/H)")]
        public int UsageLevelNumber { get; set; }

        [Required(ErrorMessage = "Please Provide the Rate")]
        [Display(Name = "Rate")]
        public decimal Rate { get; set; }
    }
}