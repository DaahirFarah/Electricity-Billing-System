using EBS.viewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace EBS.Controllers
{
    [Authorize]
    public class MeterController : Controller
    {
        // Instance of the connection String
        private readonly string SecConn = ConfigurationManager.ConnectionStrings["SecConn"].ConnectionString;

        // GET: Meter
        public ActionResult Index()
        {
            MeterWrapper wrapper = new MeterWrapper();
            wrapper.meterList = GetMeters();
            return View(wrapper);
        }

        // GET: /Meter/Create
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        // POST: /Meter/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(MeterWrapper model)
        {
            if (ModelState.IsValid)
            {
                InsertMeter(model);
                return RedirectToAction("Index");
            }
            return View(model);
        }

        // POST: Get Meter Data
        public JsonResult GetMeterData(int id)
        {
            using (SqlConnection connection = new SqlConnection(SecConn))
            {

                connection.Open();
                string query = "SELECT * FROM Meters WHERE MeterID = @MeterID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MeterID", id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Populate the meter object
                            MeterWrapper meter = new MeterWrapper
                            {
                                MeterID = Convert.ToInt32(reader["MeterID"]),
                                SerialNumber = Convert.ToInt32(reader["SerialNumber"]),
                                Type = reader["Type"].ToString(),
                                Status = reader["Status"].ToString()
                            };

                            // Return the meter Data as JSON
                            return Json(meter, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
            }

            // If no data found, return an empty JSON object
            return Json(new MeterWrapper(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult UpdateMeter(MeterWrapper model)
        {
            if (ModelState.IsValid)
            {
                UpdateMeterMethod(model);
                return Json(new { success = true, message = "Meter Updated Successfully!" });
            }
            return Json(new { success = false, message = "Meter Update Failed!" });
        }

        // POST: Delete
        [HttpPost]
        public JsonResult Delete(int id)
        {
            DeleteMeter(id);
            return Json(new { success = true, message = "Meter Deleted Successfully!" });
        }

        // GET: /Meter/BulkInsert
        [HttpGet]
        public ActionResult BulkInsert()
        {
            return View();
        }

        // POST: /Meter/BulkInsert
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BulkInsert(List<MeterWrapper> model)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(SecConn))
                {
                    connection.Open();

                    // Start a SQL transaction
                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Create a SQL command for bulk insert within the transaction
                            using (SqlCommand command = connection.CreateCommand())
                            {
                                command.Transaction = transaction;
                                command.CommandText = "INSERT INTO Meters (SerialNumber, Type, Status) VALUES (@SerialNumber, @Type, @Status)";

                                foreach (var wrapper in model)
                                {
                                    // Set parameter values for each Meter
                                    command.Parameters.AddWithValue("@SerialNumber", wrapper.SerialNumber);
                                    command.Parameters.AddWithValue("@Type", wrapper.Type);
                                    command.Parameters.AddWithValue("@Status", wrapper.Status);

                                    // Execute the command for each Meter
                                    command.ExecuteNonQuery();

                                    // Clear parameters for the next iteration
                                    command.Parameters.Clear();
                                }
                            }

                            // Commit the transaction if everything is successful
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            // Roll back the transaction if an error occurs
                            transaction.Rollback();
                            ModelState.AddModelError("", "An Error Occured While Inserting Records, Please Try Again!" + ex.Message);

                        }
                    }
                }

                return RedirectToAction("Index"); // Redirect to a success page
            }
            catch (Exception ex)
            {
                // Handle the exception and provide appropriate error feedback to the user
                ModelState.AddModelError("", "An error occurred while inserting data: " + ex.Message);
                return View(model); // Show the input form with error messages
            }
        }



        // Fetching Meter Information From the Database
        private List<MeterVM> GetMeters()
        {
            List<MeterVM> meters = new List<MeterVM>();

            using (SqlConnection Connection = new SqlConnection(SecConn))
            {
                Connection.Open();
                string query = "SELECT * FROM Meters";
                using (SqlCommand command = new SqlCommand(query, Connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            meters.Add(new MeterVM
                            {
                                MeterID = Convert.ToInt32(reader["MeterID"]),
                                SerialNumber = Convert.ToInt32(reader["SerialNumber"]),
                                Type = reader["Type"].ToString(),
                                Status = reader["Status"].ToString()

                            });
                        }
                    }
                }
            }

            return meters;
        }


        // Insert Meter Data
        private void InsertMeter(MeterWrapper model)
        {
            using (SqlConnection connection = new SqlConnection(SecConn))
            {
                connection.Open();
                string Insertquery = "INSERT INTO Meters (SerialNumber, Type, Status) VALUES (@SerialNumber, @Type, @Status)";

                using (SqlCommand command = new SqlCommand(Insertquery, connection))
                {
                    command.Parameters.AddWithValue("@SerialNumber", model.SerialNumber);
                    command.Parameters.AddWithValue("@Type", model.Type);
                    command.Parameters.AddWithValue("@Status", model.Status);

                    command.ExecuteNonQuery();
                }
            }
        }

        // Update Meter Info
        private void UpdateMeterMethod(MeterWrapper model)
        {
            using (SqlConnection connection = new SqlConnection(SecConn))
            {
                connection.Open();
                string query = "UPDATE Meters SET SerialNumber = @SerialNumber, Type = @Type, Status = @Status WHERE MeterID = @MeterID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MeterID", model.MeterID);
                    command.Parameters.AddWithValue("@SerialNumber", model.SerialNumber);
                    command.Parameters.AddWithValue("@Type", model.Type);
                    command.Parameters.AddWithValue("@Status", model.Status);

                    command.ExecuteNonQuery();
                };
            }
        }

        // Delete Meter
        private void DeleteMeter(int id)
        {
            using (SqlConnection connection = new SqlConnection(SecConn))
            {
                connection.Open();
                string DeleteQuery = "DELETE FROM Meters WHERE MeterID = @MeterID";

                using (SqlCommand command = new SqlCommand(DeleteQuery, connection))
                {
                    command.Parameters.AddWithValue("MeterID", id);
                    command.ExecuteNonQuery();
                }
            }
        }

    }
}