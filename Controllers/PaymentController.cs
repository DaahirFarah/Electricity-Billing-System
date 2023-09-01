using EBS.viewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

namespace EBS.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        // ConnectionString Instance
        private readonly string SecConn = ConfigurationManager.ConnectionStrings["SecConn"].ConnectionString;

        // GET: Payment
        public ActionResult Index()
        {
            List<payVM> payments = GetAllPayments();
            return View(payments);
        }

        //GET: Record Payment
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        //SET: Record Payment
        [ValidateAntiForgeryToken]
        [HttpPost]
        [Display(Name = "Create")]
        public ActionResult Create(payVM model)
        {
            if (ModelState.IsValid)
            {
                InsertPayment(model);
                return RedirectToAction("Index");
            }
            return View(model);
        }


        //GET: Update Payment
        [HttpGet]
        public ActionResult Edit(int id)
        {
            payVM payment = GetPaymentByID(id);
            return View(payment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Display(Name = "Edit")]
        public ActionResult Edit(payVM model)
        {
            if (ModelState.IsValid)
            {
                UpdatePayment(model);
                return RedirectToAction("Index");
            }
            return View(model);
        }

        // GET: Delete
        public ActionResult Delete(int id)
        {
            payVM payment = GetPaymentByID(id);
            return View(payment);
        }

        //POST: Delete Invoice
        [HttpPost]
        [ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            DeletePayment(id);
            return RedirectToAction("Index");
        }


        // Logic for retrieving payment data from the database
        public List<payVM> GetAllPayments()
        {
            List<payVM> payments = new List<payVM>();

            using (SqlConnection connection = new SqlConnection(SecConn))
            {
                connection.Open();
                string query = "SELECT * FROM PaymentTbl";

                using(SqlCommand command = new SqlCommand(query, connection))
                {
                    using(SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            payments.Add(new payVM()
                            {
                                payID = Convert.ToInt32(reader["payID"]),
                                cID = Convert.ToInt32(reader["cID"]),
                                invoiceID = Convert.ToInt32(reader["invoiceID"]),
                                paidAmount = Convert.ToDecimal(reader["paidAmount"]),
                                totalFee = Convert.ToDecimal(reader["totalFee"]),
                                payMethod = Convert.ToString(reader["payMethod"]),
                                payDate = Convert.ToDateTime(reader["payDate"]),

                            });
                        }
                    }
                }
            }


            return payments;
        }


        // Method that holds payment insertion logic

        private void InsertPayment(payVM model)
        {
            using (SqlConnection connection = new SqlConnection(SecConn))
            {
                connection.Open();
                string query = "INSERT INTO PaymentTbl (cID, invoiceID, paidAmount,"
                             + "totalFee, payMethod, payDate ) "
                             + "VALUES (@cID, @invoiceID, @paidAmount, @totalFee, @payMethod, @payDate)";
                           

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@cID", model.cID);
                    command.Parameters.AddWithValue("@invoiceID", model.invoiceID);
                    command.Parameters.AddWithValue("@paidAmount", model.paidAmount);
                    command.Parameters.AddWithValue("@totalFee", model.totalFee);
                    command.Parameters.AddWithValue("@payMethod", model.payMethod);
                    command.Parameters.AddWithValue("@payDate", model.payDate);
                   

                    command.ExecuteNonQuery();

                    // Calculate the difference between paidAmount and totalFee
                    decimal balanceDifference = model.totalFee - model.paidAmount;

                    // Update CustomerTbl with the balance difference
                    string updateBalanceQuery = "UPDATE CustomerTbl SET Balance = Balance + @balanceDifference WHERE cID = @cID";
                    using (SqlCommand updateBalanceCommand = new SqlCommand(updateBalanceQuery, connection))
                    {
                        updateBalanceCommand.Parameters.AddWithValue("@balanceDifference", balanceDifference);
                        updateBalanceCommand.Parameters.AddWithValue("@cID", model.cID);
                        updateBalanceCommand.ExecuteNonQuery();
                    }
                }
            }
        }


        // Retrieving Payments by ID for updating
        private payVM GetPaymentByID(int payID)
        {
            using (SqlConnection connection = new SqlConnection(SecConn))
            {
                connection.Open();
                string query = "SELECT * FROM PaymentTbl WHERE payID = @payID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@payID", payID);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            return new payVM
                            {
                                payID = Convert.ToInt32(reader["payID"]),
                                cID = Convert.ToInt32(reader["cID"]),
                                invoiceID = Convert.ToInt32(reader["invoiceID"]),                       
                                paidAmount = Convert.ToDecimal(reader["paidAmount"]),
                                totalFee = Convert.ToDecimal(reader["totalFee"]),
                                payMethod = Convert.ToString(reader["payMethod"]),
                                payDate = Convert.ToDateTime(reader["payDate"])
                                
                            };
                        }
                    }
                }
            }

            return null;
        }

        // Update Payment Logic
        private void UpdatePayment(payVM model)
        {
            using (SqlConnection connection = new SqlConnection(SecConn))
            {
                connection.Open();
                string query = "Update PaymentTbl SET cID = @cID, invoiceID = @invoiceID, paidAmount = @paidAmount,"
                             + "totalFee = @totalFee, payMethod = @payMethod, payDate = @payDate WHERE payID = @payID";
                             


                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@payID", model.payID);
                    command.Parameters.AddWithValue("@cID", model.cID);
                    command.Parameters.AddWithValue("@invoiceID", model.invoiceID);
                    command.Parameters.AddWithValue("@paidAmount", model.paidAmount);
                    command.Parameters.AddWithValue("@totalFee", model.totalFee);
                    command.Parameters.AddWithValue("@payMethod", model.payMethod);
                    command.Parameters.AddWithValue("@payDate", model.payDate);


                    command.ExecuteNonQuery();

                    // Calculate the difference between paidAmount and totalFee
                    decimal balanceDifference = model.totalFee - model.paidAmount;

                    // Update CustomerTbl with the balance difference
                    string updateBalanceQuery = "UPDATE CustomerTbl SET Balance = @balanceDifference WHERE cID = @cID";
                    using (SqlCommand updateBalanceCommand = new SqlCommand(updateBalanceQuery, connection))
                    {

                        updateBalanceCommand.Parameters.AddWithValue("@balanceDifference", balanceDifference);
                        updateBalanceCommand.Parameters.AddWithValue("@cID", model.cID);
                        updateBalanceCommand.ExecuteNonQuery();
                    }
                }
            }
        }


        // Delete Payment Logic
        private void DeletePayment(int id)
        {
            
            using (SqlConnection connection = new SqlConnection(SecConn))
            {
                connection.Open();
                string query = "DELETE FROM PaymentTbl WHERE payID = @payID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@payID", id);

                    command.ExecuteNonQuery();
                }
            }
        }
    }
}