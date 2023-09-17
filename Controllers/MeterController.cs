﻿using EBS.viewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace EBS.Controllers
{
    public class MeterController : Controller
    {
        // Instance of the connection String
        private readonly string SecConn = ConfigurationManager.ConnectionStrings["SecConn"].ConnectionString;

        // GET: Meter
        public ActionResult Index()
        {
            List<MeterVM> meter = GetMeters();
            return View(meter);
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
        public ActionResult Create(MeterVM model)
        {
            if (ModelState.IsValid)
            {
                InsertMeter(model);
                return RedirectToAction("Index");
            }
            return View(model);
        }

        // GET: /Meter/Edit
        [HttpGet]
        public ActionResult Edit(int id)
        {
            MeterVM meter = GetMeterByID(id);
            return View(meter);
        }

        // POST: /Meter/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(MeterVM model)
        {
            if (ModelState.IsValid)
            {
                UpdateMeter(model);
                return RedirectToAction("Index");
            }
            return View(model);
        }

        // GET: /Meter/Delete
        [HttpGet]
        public ActionResult Delete(int id)
        {
            MeterVM meter = GetMeterByID(id);
            return View(meter);
        }

        // POST: /Meter/Delete
        [HttpPost]
        [ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            DeleteMeter(id);
            return RedirectToAction("Index");
        }

        // GET: /Meter/BulkInsert
        public ActionResult BulkInsert()
        {
            return View();
        }

        // POST: /Meter/BulkInsert
        public ActionResult BulkInsert(List<MeterVM> model)
        {
            if (ModelState.IsValid)
            {
                BulkInserting(model);
                return RedirectToAction("Index");
            }
            return View(model);
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

        //Get Meter By ID
        private MeterVM GetMeterByID(int Id)
        {
            using (SqlConnection connection = new SqlConnection(SecConn))
            {
                connection.Open();
                string query = "SELECT * FROM Meters WHERE MeterID = @MeterID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MeterID", Id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            return new MeterVM
                            {
                                MeterID = Convert.ToInt32(reader["MeterID"]),
                                SerialNumber = Convert.ToInt32(reader["SerialNumber"]),
                                Type = reader["Type"].ToString(),
                                Status = reader["Status"].ToString()
                            };
                        }
                    }
                }
            }

            return null;
        }

        // Insert Meter Data
        private void InsertMeter(MeterVM model)
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
        private void UpdateMeter(MeterVM model)
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
        private void DeleteMeter(int MeterId)
        {
            using (SqlConnection connection = new SqlConnection(SecConn))
            {
                connection.Open();
                string DeleteQuery = "DELETE FROM Meters WHERE MeterID = @MeterID";

                using (SqlCommand command = new SqlCommand(DeleteQuery, connection))
                {
                    command.Parameters.AddWithValue("MeterID", MeterId);
                    command.ExecuteNonQuery();
                }
            }
        }

        // Meter Bulk Insertion will be Handled by the following function
        private void BulkInserting(List<MeterVM> model)
        {
            List<MeterVM> met = new List<MeterVM>();

            using(SqlConnection connection = new SqlConnection(SecConn))
            {
                connection.Open();
                string bulkInsertion = "INSERT INTO Meters (SerialNumber, Type, Status) VALUES (@SerialNumber, @Type, @Status)";

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        foreach(var meter in model)
                        {
                            var command = new SqlCommand(bulkInsertion, connection, transaction);

                            command.Parameters.AddWithValue("@SerialNumber", meter.SerialNumber);
                            command.Parameters.AddWithValue("@Type", meter.Type);
                            command.Parameters.AddWithValue("@Status", meter.Status);

                            command.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        ModelState.AddModelError("", "An Error Occured While Inserting Records, Please Try Again!" + e.Message);
                    }
                }
            }

        }

    }
}