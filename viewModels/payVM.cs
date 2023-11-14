using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBS.viewModels
{
    public class payVM
    {
        public payVM()
        {
            SelectedBranch = new List<string> { "" };
            payDate = DateTime.Now;
        }

        [Display(Name = "Branch")]
        public List<string> SelectedBranch { get; set; }

        [Display(Name = "Payment ID")]
        [Key]
        public int payID { get; set; }

        [Display(Name = "Customer ID")]
        [Required(ErrorMessage = "This is required.")]
        public int cID { get; set; }

        [Display(Name = "Invoice ID")]
        [Required(ErrorMessage = "This is required.")]
        public int invoiceID { get; set; }

        [Display(Name = "Paid Amount ($)")]
        [Required(ErrorMessage = "This is required.")]
        public decimal paidAmount { get; set; }

        [Display(Name = "Total Fee ($)")]
        [Required(ErrorMessage = "This is required.")]
        public decimal totalFee { get; set; }

        [Display(Name = "Payment Method")]
        [Required(ErrorMessage = "This is required.")]
        public string payMethod { get; set; }

        [Display(Name = "Payment Date")]
        [Required(ErrorMessage = "This is required.")]
        public DateTime payDate { get; set; }

        [Display(Name = "Payment Date")]
        [Required(ErrorMessage = "Reading Date is required")]
        public DateTime selectedDate { get; set; }

        public string cFirstName { get; set; }
        public string cMidName { get; set; }
        public string cLastName { get; set; }
        public string cFullName
        {
            get
            {
                // Concatenate the first name, middle name, and last name with spaces
                return $"{cFirstName} {cMidName} {cLastName}";
            }
            set
            {

            }
        }

        public string fullName { get; set; }

        public decimal Balance { get; set; }
    }
}