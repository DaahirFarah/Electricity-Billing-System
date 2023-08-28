using EBS.viewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
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
        public ActionResult Create(invoiceVM model)
        {
            if (ModelState.IsValid)
            {
                InsertInvoice(model);
                return RedirectToAction("Index");
            }
            return View(model);
        }


        //GET: Update Invoice
        public ActionResult Edit(int id)
        {
            invoiceVM invoice = GetInvoiceById(id);
            return View(invoice);
        }

        // POST: Update Invoice
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[Display(Name = "Edit")]
        //public ActionResult Edit(invoiceVM model)
        //{
        //    UpdateInvoice(model);
        //    return View(model);
        //}

        
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

        // Inserting Invoices Into the Database
        private void InsertInvoice(invoiceVM model)
        {
            using(SqlConnection connection = new SqlConnection(SecConn))
            {
                connection.Open();
                string query = "INSERT INTO InvoiceTbl (cID, Rate, billingPeriodStarts,"
                             + "billingPeriodEnds, prev_Reading, cur_Reading, reading_Value, reading_Date, total_Fee) "
                             + "VALUES (@cID, @Rate, @billingPeriodStarts, @billingPeriodEnds, @prev_Reading, @cur_Reading,"
                             + "@reading_Value, @reading_Date, @total_Fee)";

                using(SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@cID", model.cID);
                    command.Parameters.AddWithValue("@Rate", model.Rate);
                    command.Parameters.AddWithValue("@billingPeriodStarts", model.billingPeriodStarts);
                    command.Parameters.AddWithValue("@billingPeriodEnds", model.billingPeriodEnds);
                    command.Parameters.AddWithValue("@prev_Reading", model.prev_Reading);
                    command.Parameters.AddWithValue("@cur_Reading", model.cur_Reading);
                    command.Parameters.AddWithValue("@reading_Value", model.reading_Value);
                    command.Parameters.AddWithValue("@reading_Date", model.reading_Date);
                    command.Parameters.AddWithValue("@total_Fee", model.total_Fee);

                    command.ExecuteNonQuery();
                }
            }
        }

        // Retrieving Invoices by ID for updating
        private invoiceVM GetInvoiceById(int invoiceID)
        {
            using (SqlConnection connection = new SqlConnection(SecConn))
            {
                connection.Open();
                string query = "SELECT * FROM InvoiceTbl WHERE invoiceID = @invoiceID";
                using(SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@invoiceID", invoiceID);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            return new invoiceVM
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

                            };
                        }
                    }
                }
            }

            return null;
        }

    }
}
