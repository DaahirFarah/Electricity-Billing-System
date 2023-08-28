using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBS.viewModels
{
    public class invoiceVM
    {
        [Key]
        [Display(Name = "Invoice ID")]
        public int invoiceID { get; set; }

        [Display(Name = "Customer ID")]
        [Required(ErrorMessage = "Customer ID is required.")]
        public int cID { get; set; }

        [Display(Name = "Billing Period Started")]
        [Required(ErrorMessage = "Period Start Date is required")]
        public DateTime billingPeriodStarts { get; set; }

        [Display(Name = "Billing Period Ended")]
        [Required(ErrorMessage = "Period End Date is required")]
        public DateTime billingPeriodEnds { get; set; }

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

        [Display(Name = "Rate")]
        [Required(ErrorMessage = "Rate is required")]
        public decimal Rate { get; set; }


    }
}