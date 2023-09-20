using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBS.viewModels
{
    public class customerWrapper
    {
        public List<customerVM> customersList { get; set; }

        public customerWrapper()
        {
            // Initialize SelectedMeterID with a default value of 0
            SelectedMeterID = new List<int> { 0 };
        }


        [Key]
        [Required]
        [Display(Name = "Customer ID")]
        public int cID { get; set; }

        [Required(ErrorMessage = "Customer First name is required.")]
        [Display(Name = "First Name")]
        public string cFirstName { get; set; }

        [Required(ErrorMessage = "Customer Middle name is required.")]
        [Display(Name = "Middle Name")]
        public string cMidName { get; set; }

        [Required(ErrorMessage = "Customer Last name is required.")]
        [Display(Name = "Last Name")]
        public string cLastName { get; set; }

        [Required(ErrorMessage = "Customer Address is required.")]
        [Display(Name = "Address")]
        public string cAddress { get; set; }

        [Required(ErrorMessage = "Please Provide Customer Phone Number")]
        [Display(Name = "Phone Number")]
        //[RegularExpression(@"^\d{10}$", ErrorMessage = "Phone Number Should be 10 digits starting 061.")]
        public int cNumber { get; set; }

        [Display(Name = "Phone Number (Optional)")]
        //[RegularExpression(@"^\d{10}$", ErrorMessage = "Phone Number Should be 10 digits starting 061.")]
        public string cNumberOp { get; set; }

        [Required]
        public List<int> SelectedMeterID { get; set; }

        [Required(ErrorMessage = "Please Insert Meter ID")]
        [Display(Name = "Meter ID")]
        public int MeterID { get; set; }

        [Required(ErrorMessage = "Please Provide Customer Branch")]
        [Display(Name = "Branch")]
        public string Branch { get; set; }


        [Display(Name = "Balance")]
        public decimal Balance { get; set; }


    }
}