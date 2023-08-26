﻿using EBS.viewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace EBS.Controllers
{
    public class CustomerController : Controller
    {
        private readonly string SecConn = ConfigurationManager.ConnectionStrings["SecConn"].ConnectionString;

        // GET: Customer
        public ActionResult Index()
        {
            List<customerVM> customers = GetAllCustomers();
            return View(customers);
        }

        //GET: Register Customer
     
        public ActionResult Create()
        {
            return View();
        }

        //SET: Register Customer
        //[ValidateAntiForgeryToken]
        //[HttpPost]
        //[Display(Name = "Create")]
        //public ActionResult CreateConfirm(customerVM model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        InsertCustomer(model);
        //        return RedirectToAction("Index");
        //    }
        //    return View(model);
        //}


        ////GET: Update Customer
        //public ActionResult Edit(int id)
        //{
        //    customerVM customer = GetCustomerById(id);
        //    return View(customer);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[Display(Name = "Edit")]
        //public ActionResult EditConfirmConfirm(customerVM model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        UpdateCustomer(model);
        //        return RedirectToAction("Index");
        //    }
        //    return View(model);
        //}

        //public ActionResult Delete(int id)
        //{
        //    customerVM customer = GetCustomerById(id);
        //    return View(customer);
        //}

        //[HttpPost]
        //[ActionName("Delete")]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    DeleteCustomer(id);
        //    return RedirectToAction("Index");
        //}


        // Fetching Customers From the Database
        private List<customerVM> GetAllCustomers()
        {
            List<customerVM> customers = new List<customerVM>();

            using (SqlConnection connection = new SqlConnection(SecConn))
            {
                connection.Open();

                string query = "SELECT * FROM CustomerTbl";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            customers.Add(new customerVM
                            {
                                cID = Convert.ToInt32(reader["cID"]),
                                cFirstName = reader["cFirstName"].ToString(),
                                cMidName = reader["cMidName"].ToString(),
                                cLastName = reader["cLastName"].ToString(),
                                cAddress = reader["cAddress"].ToString(),
                                cNumber = Convert.ToInt32(reader["cNumber"]),
                                cNumberOp = reader["cNumberOp"] != DBNull.Value ? reader["cNumberOp"].ToString() : "N/A"

                            });
                        }
                    }
                }
            }

            return customers;
        }





    }
}