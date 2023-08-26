using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Management.Instrumentation;
using System.Web;

namespace EBS.viewModels
{
    public class cutomerVM
    {
        [Key]
        [Required]
        [Display(Name = "Customer ID")]
        public string cID { get; set; }

        [Required(ErrorMessage = "Customer First name is required.")]
        [Display(Name = "Customer First Name")]
        public string cFirstName { get;}

        [Required(ErrorMessage = "Customer Middle name is required.")]
        [Display(Name = "Customer Middle Name")]
        public string cMidName { get; }

        [Required(ErrorMessage = "Customer Last name is required.")]
        [Display(Name = "Customer Last Name")]
        public string cLastName { get; }

        [Required(ErrorMessage = "Customer Address is required.")]
        [Display(Name = "Customer Address")]
        public string cAddress { get; set; }

        [Required(ErrorMessage = "Customer Number is required.")]
        [Display(Name = "Customer Phone Number")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone Number Should be 10 digits starting 061.")]
        public int cNumber { get; set; }

        [Display(Name = "Customer Phone Number (Optional)")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone Number Should be 10 digits starting 061.")]
        public int cNumberOp { get; set; }


    }
}