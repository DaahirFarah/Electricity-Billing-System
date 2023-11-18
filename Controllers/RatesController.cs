using EBS.viewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace EBS.Controllers
{
    //[Authorize]
    public class RatesController : Controller
    {
        // Instance of the connection String
        private readonly string SecConn = ConfigurationManager.ConnectionStrings["SecConn"].ConnectionString;

        // GET: Rates
        public ActionResult Index()
        {
            rateWrapper wrapper = new rateWrapper();
            wrapper.rateList = GetAllRates();
            return View(wrapper);
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
        public ActionResult Create(rateWrapper model)
        {
            if (ModelState.IsValid)
            {
                InsertTarrif(model);
                return RedirectToAction("Index");
            }
            return View(model);
        }

        // GET Rate
        [HttpPost]
        public JsonResult GetRate(int id)
        {
            using (SqlConnection connection = new SqlConnection(SecConn))
            {

                connection.Open();
                string query = "SELECT * FROM Rates WHERE Id = @Id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Populate the invoice object
                            rateWrapper rate = new rateWrapper
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                UsageLevelName = reader["UsageLevelName"].ToString(),
                                UsageLevelNumberStarts = Convert.ToInt32(reader["UsageLevelNumberStarts"]),
                                UsageLevelNumberEnds = Convert.ToInt32(reader["UsageLevelNumberEnds"]),
                                Rate = Convert.ToDecimal(reader["Rate"])
                            };

                            // Return the Invoice Data as JSON
                            return Json(rate, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
            }

            // If no data found, return an empty JSON object
            return Json(new rateWrapper(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateRate(rateWrapper model)
        {
            if (ModelState.IsValid)
            {
                UpdateTarrif(model);
                return Json(new { success = true, message = "Rate Update Successfully!" });
            }
            return Json(new { success = false, message = "Rate Update Failed. Please Try Again!" });
        }

        [HttpPost]
        public JsonResult Delete (int id)
        {
            DeleteTarrif(id);
            return Json(new { success = true, message = "Rate Deleted Successfully!" });
        }

        // Fetching Rate Information From the Database
        private List<RateVM> GetAllRates()
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
                                UsageLevelNumberStarts = Convert.ToInt32(reader["UsageLevelNumberStarts"]),
                                UsageLevelNumberEnds = Convert.ToInt32(reader["UsageLevelNumberEnds"]),
                                Rate = Convert.ToDecimal(reader["Rate"])

                            });
                        }
                    }
                }
            }

            return rates;
        }
               
        // Insert Tarrif
        private void InsertTarrif(rateWrapper model)
        {
            using (SqlConnection connection = new SqlConnection(SecConn))
            {
                connection.Open();
                string Insertquery = "INSERT INTO Rates (UsageLevelName, UsageLevelNumberStarts, UsageLevelNumberEnds, Rate) VALUES"
                                   + "(@UsageLevelName, @UsageLevelNumberStarts, @UsageLevelNumberEnds, @Rate)";

                using (SqlCommand command = new SqlCommand(Insertquery, connection))
                {
                    command.Parameters.AddWithValue("@usageLevelName", model.UsageLevelName);
                    command.Parameters.AddWithValue("@UsageLevelNumberStarts", model.UsageLevelNumberStarts);
                    command.Parameters.AddWithValue("@UsageLevelNumberEnds", model.UsageLevelNumberEnds);
                    command.Parameters.AddWithValue("@Rate", model.Rate);

                    command.ExecuteNonQuery();
                }
            }
        }

        // Update Tarrif Info
        private void UpdateTarrif(rateWrapper model)
        {
            using (SqlConnection connection = new SqlConnection(SecConn))
            {
                connection.Open();
                string UpdateQuery = "UPDATE Rates SET UsageLevelName = @UsageLevelName, UsageLevelNumberStarts = @UsageLevelNumberStarts, "
                                   + " UsageLevelNumberEnds = @UsageLevelNumberEnds, Rate = @Rate WHERE Id = @Id";

                using (SqlCommand command = new SqlCommand(UpdateQuery, connection))
                {
                    command.Parameters.AddWithValue("@Id", model.Id);
                    command.Parameters.AddWithValue("@UsageLevelName", model.UsageLevelName);
                    command.Parameters.AddWithValue("@UsageLevelNumberStarts", model.UsageLevelNumberStarts);
                    command.Parameters.AddWithValue("@UsageLevelNumberEnds", model.UsageLevelNumberEnds);
                    command.Parameters.AddWithValue("@Rate", model.Rate);

                    command.ExecuteNonQuery();
                };
            }
        }

        // Delete Tarrif
        private void DeleteTarrif(int Id)
        {
            using(SqlConnection connection = new SqlConnection(SecConn))
            {
                connection.Open();
                string DeleteQuery = "DELETE FROM Rates WHERE Id = @Id";

                using(SqlCommand command = new SqlCommand(DeleteQuery, connection))
                {
                    command.Parameters.AddWithValue("Id", Id);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}