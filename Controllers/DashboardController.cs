using EBS.viewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EBS.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly string SecConn = ConfigurationManager.ConnectionStrings["SecConn"].ConnectionString;


        // GET: Dashboard
        public ActionResult Index()
        {
            DashboardVM data = GetData();
            return View(data);
        }


        private DashboardVM GetData()
        {
            DashboardVM dashboard = new DashboardVM();
            using (SqlConnection connection = new SqlConnection(SecConn))
            {
                connection.Open();

                // Below commands retrieve the number of records in each of those tables and then stores them in the corresponding properties in the viewModel
                SqlCommand Customerscommand = new SqlCommand("SELECT COUNT(cFirstName) AS CountOfCustomers FROM CustomerTbl", connection);
                dashboard.Customers = (int)Customerscommand.ExecuteScalar();
               
                SqlCommand Invoicescommand = new SqlCommand("SELECT COUNT(invoiceID) AS CountOfInvoices FROM InvoiceTbl", connection);
                dashboard.Invoices = (int)Invoicescommand.ExecuteScalar();
               
                SqlCommand Userscommand = new SqlCommand("SELECT COUNT(Username) AS usersCount FROM Users", connection);
                dashboard.Users = (int)Userscommand.ExecuteScalar();
               
                SqlCommand paycommand = new SqlCommand("SELECT COUNT(payID) AS payCount FROM PaymentTbl", connection);
                dashboard.Payments = (int)paycommand.ExecuteScalar();

                SqlCommand meterCommand = new SqlCommand("SELECT COUNT(MeterID) AS MeterCount FROM Meters", connection);
                dashboard.Meters = (int)meterCommand.ExecuteScalar();

                SqlCommand activeCommand = new SqlCommand("SELECT COUNT(Status) AS ActiveMeters FROM Meters WHERE Status = 'Active'", connection);
                dashboard.ActiveMeters = (int)activeCommand.ExecuteScalar();
                
                SqlCommand InactiveCommand = new SqlCommand("SELECT COUNT(Status) AS InactiveMeters FROM Meters WHERE Status = 'Inactive'", connection);
                dashboard.InactiveMeters = (int)InactiveCommand.ExecuteScalar();

                // these will retrieve the sum of $Total Fees and the sum of standing Balances

                SqlCommand totalFeecommand = new SqlCommand("SELECT SUM(totalFee) AS SumOfRevenue FROM PaymentTbl", connection);
                dashboard.Revenue = (decimal)totalFeecommand.ExecuteScalar();
            

                SqlCommand balancecommand = new SqlCommand("SELECT SUM(Balance) AS SumOfBalance FROM CustomerTbl", connection);
                dashboard.Balances = (decimal)balancecommand.ExecuteScalar();
          

            }


            return dashboard;
        }
    }
}