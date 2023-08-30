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
        public ActionResult CreateConfirm()
        {
            return View();
        }


        ////GET: Update Payment
        //public ActionResult Edit()
        //{
        //    return View();
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[Display(Name = "Edit")]
        //public ActionResult EditConfirmConfirm()
        //{
        //    return View();
        //}

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

    }
}