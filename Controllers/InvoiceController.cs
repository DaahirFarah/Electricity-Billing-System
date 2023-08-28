using EBS.viewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

namespace EBS.Controllers
{
    public class InvoiceController : Controller
    {
        // ConnectionString Instance
        private readonly string SecConn = ConfigurationManager.ConnectionStrings["SecConn"].ConnectionString;


        // GET: Invoice
        public ActionResult Index()
        {
            List<invoiceVM> invoices = GetAllInvoices();
            return View(invoices);
        }

        //GET: Register Invoice
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        //SET: Register Invoice
        [ValidateAntiForgeryToken]
        [HttpPost]
        [Display(Name = "Create")]
        public ActionResult CreateConfirm()
        {
            return View();
        }


        //GET: Update Invoice
        public ActionResult Edit()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Display(Name = "Edit")]
        public ActionResult EditConfirm()
        {
            return View();
        }

        
        // Fetching Invoices From the Database
        private List<invoiceVM> GetAllInvoices()
        {
            List<invoiceVM> invoices = new List<invoiceVM>();

            using (SqlConnection Connection = new SqlConnection(SecConn))
            {
                Connection.Open();
                string query = "SELECT * FROM InvoiceTbl";
                using (SqlCommand command = new SqlCommand(query, Connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            invoices.Add(new invoiceVM
                            {
                                invoiceID = Convert.ToInt32(reader["invoiceID"]),
                                cID = Convert.ToInt32(reader["cID"]),
                                billingPeriodStarts = Convert.ToDateTime(reader["billingPeriodStarts"]),
                                billingPeriodEnds = Convert.ToDateTime(reader["billingPeriodEnds"]),
                                prev_Reading = Convert.ToDecimal(reader["prev_Reading"]),
                                cur_Reading = Convert.ToDecimal(reader["cur_Reading"]),
                                reading_Date = Convert.ToDateTime(reader["reading_Date"]),
                                Rate = Convert.ToDecimal(reader["Rate"]),
                                reading_Value = Convert.ToDecimal(reader["reading_Value"]),
                                total_Fee = Convert.ToDecimal(reader["total_Fee"]),
                                
                            });
                        }
                    }
                }
            }

            return invoices;
        }
    }
}