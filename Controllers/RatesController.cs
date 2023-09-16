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
    public class RatesController : Controller
    {
        // Instance of the connection String
        private readonly string SecConn = ConfigurationManager.ConnectionStrings["SecConn"].ConnectionString;

        // GET: Rates
        public ActionResult Index()
        {
            List<RateVM> rate = GetRates();
            return View(rate);
        }

        // GET: /Rates/Create
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        // POST: /Rates/Create
        [ValidateAntiForgeryToken]
        [HttpPost]
        [ActionName("Create")]
        public ActionResult Create(RateVM model)
        {
            if (ModelState.IsValid)
            {
                InsertTarrif(model);
                return RedirectToAction("Index");
            }
            return View(model);
        }

        // GET: Rates/Edit
        [HttpGet]
        public ActionResult Edit()
        {
            return View();
        }

        // GET: /Rates/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Edit")]
        public ActionResult EditConfirmed()
        {
            return View();
        }

        // GET: /Rates/Delete
        public ActionResult Delete()
        {
            return View();
        }

        // POST: /Rates/Delete
        [HttpPost]
        [ActionName("Delete")]
        public ActionResult DeleteConfirmed()
        {
            return View();
        }


        // Fetching Rate Information From the Database
        private List<RateVM> GetRates()
        {
            List<RateVM> rates = new List<RateVM>();

            using (SqlConnection Connection = new SqlConnection(SecConn))
            {
                Connection.Open();
                string query = "SELECT * FROM Rates";
                using (SqlCommand command = new SqlCommand(query, Connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            rates.Add(new RateVM
                            {
                               Id = Convert.ToInt32(reader["Id"]),
                               UsageLevelName = reader["UsageLevelName"].ToString(),
                                //UsageLevelNumber = Convert.ToInt32(reader["UsageLevelAmount"]),
                                UsageLevelNumber = Convert.ToInt32(reader["UsageLevelNumber"]),
                                Rate = Convert.ToDecimal(reader["Rate"])

                            });
                        }
                    }
                }
            }

            return rates;
        }

        //Get Rate By ID
        private RateVM GetRate(int Id)
        {
            using (SqlConnection connection = new SqlConnection(SecConn))
            {
                connection.Open();
                string query = "SELECT * FROM Rates WHERE Id = @Id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", Id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            return new RateVM
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                UsageLevelName = reader["UsageLevelName"].ToString(),
                                UsageLevelNumber = Convert.ToInt32(reader["UsageLevelNumber"]),
                                Rate = Convert.ToDecimal(reader["Rate"])

                            };
                        }
                    }
                }
            }

            return null;
        }

        // Insert Tarrif
        private void InsertTarrif(RateVM model)
        {
            using(SqlConnection connection = new SqlConnection(SecConn))
            {
                connection.Open();
                string Insertquery = "INSERT INTO Rates (UsageLevelName, UsageLevelNumber, Rate) VALUES"
                                   + "(@UsageLevelName, @UsageLevelNumber, @Rate)";

                using(SqlCommand command = new SqlCommand(Insertquery, connection))
                {
                    command.Parameters.AddWithValue("@usageLevelName", model.UsageLevelName);
                    command.Parameters.AddWithValue("@UsageLevelNumber", model.UsageLevelNumber);
                    command.Parameters.AddWithValue("@Rate", model.Rate);

                    command.ExecuteNonQuery();
                }
            }
        }


    }
}