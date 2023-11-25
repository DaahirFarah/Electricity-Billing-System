using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EBS.viewModels
{
    public class customerVM
    {
        public customerVM()
        {
            // Initialize SelectedMeterID with a default value of 0
            SelectedMeterID = new List<int> { 0 };
            SelectedType = new List<string> { "" };
            SelectedBranch = new List<string> { "" };
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
        public int cNumber { get; set; }

        [Display(Name = "Phone Number (Optional)")]
        public string cNumberOp { get; set; }

        [Required]
        public List<int> SelectedMeterID { get; set; }

        [Required(ErrorMessage = "Please Insert Meter ID")]
        [Display(Name = "Meter ID")]
        public int MeterID { get; set; }

        [Required(ErrorMessage = "Please Insert Lock Number")]
        [Display(Name = "Lock Number")]
        public int lockNumber { get; set; }

        public List<string> SelectedType { get; set; }

        [Required]
        [Display(Name = "Meter Type")]
        public string Type { get; set; }

        public List<string> SelectedBranch { get; set; }

        [Required(ErrorMessage = "Please Provide Customer Branch")]
        [Display(Name = "Branch")]
        public string Branch { get; set; }

        [Display(Name = "Balance")]
        public decimal Balance { get; set; }

        [Display(Name = "Is Organization")]
        public string isOrg { get; set; }

        [Display(Name = "Org Name")]
        public string Name { get; set; }

        public DateTime RegisteredOn { get; set; }

        public int isBilledThisMonth { get; set; }
    }
}