using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBS.viewModels
{
    public class invoiceVM
    {
        public invoiceVM()
        {
            SelectedBranch = new List<string> { "" };
        }

        [Display(Name = "Branch")]
        public List<string> SelectedBranch { get; set;}

        [Key]
        [Display(Name = "Invoice ID")]
        public int invoiceID { get; set; }

        [Display(Name = "Customer ID")]
        [Required(ErrorMessage = "Customer ID is required.")]
        public int cID { get; set; }

        [Display(Name = "Previous Reading")]
        [Required(ErrorMessage = "Previous Reading is required")]
        public decimal prev_Reading { get; set; }

        [Display(Name = "Current Reading")]
        [Required(ErrorMessage = "Current Reading is required")]
        public decimal cur_Reading { get; set; }

        [Display(Name = "Usage (KwH)")]
        [Required(ErrorMessage = "Usage Amount is required")]
        public decimal reading_Value { get; set; }

        [Display(Name = "Total Fee ($)")]
        [Required(ErrorMessage = "Fee is required")]
        public decimal total_Fee { get; set; }

        [Display(Name = "Reading Date")]
        [Required(ErrorMessage = "Reading Date is required")]
        public DateTime reading_Date { get; set; }

        [Display(Name = "Reading Date")]
        [Required(ErrorMessage = "Reading Date is required")]
        public DateTime selectedDate { get; set; }

        [Display(Name = "Rate")]
        [Required(ErrorMessage = "Rate is required")]
        public decimal Rate { get; set; }

        public decimal balance { get; set; }

        public decimal charge { get; set; }

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

        public string customerName { get; set; }

        public string Status { get; set; }

    }
}