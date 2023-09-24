using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EBS.viewModels
{
    public class DashboardVM
    {
        // I am using this view model to hold the overall statistics data retrieved from the db and then am gonna use it to display that data in the Dashboard page

        public int Customers { get; set; }
        public decimal Revenue { get; set; }
        public int Users { get; set; }
        public int Payments { get; set; }
        public int Invoices { get; set; }
        public int Meters { get; set; }
        public int ActiveMeters { get; set; }
        public int InactiveMeters { get; set; }
        public decimal Balances { get; set; }
    }
}