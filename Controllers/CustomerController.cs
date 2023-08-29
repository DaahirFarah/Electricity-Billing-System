using EBS.viewModels;
using iTextSharp.text;
using iTextSharp.text.pdf;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

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

        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        //SET: Register Customer
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(customerVM model)
        {
            if (ModelState.IsValid)
            {
                InsertCustomer(model);
                return RedirectToAction("Index");
            }
            return View(model);
        }


        ////GET: Update Customer
        public ActionResult Edit(int id)
        {
            customerVM customer = GetCustomerById(id);
            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Display(Name = "Edit")]
        public ActionResult Edit(customerVM model)
        {
            if (ModelState.IsValid)
            {
                UpdateCustomer(model);
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public ActionResult Delete(int id)
        {
            customerVM customer = GetCustomerById(id);
            return View(customer);
        }

        [HttpPost]
        [ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            DeleteCustomer(id);
            return RedirectToAction("Index");
        }

        // This action handles exporting customers data from the database using a library called PdfSharp. 
        // This actionResult allows the user to easily download the list of customers in a pdf format 
        public ActionResult GeneratePDF()
        {
           
            var data = GetAllCustomers();

            // Create a new PDF document
            MemoryStream memoryStream = new MemoryStream();
            Document document = new Document();
            PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
            document.Open();

            // Define font and table settings
            BaseFont baseFont = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            Font headerFont = new Font(baseFont, 14, Font.BOLD);
            Font contentFont = new Font(baseFont, 12);

            // Add image as a logo at the top of the page
            string imagePath = Server.MapPath("~/Assets/_e407f44c-5341-4a3d-b20e-e7ae5a10e34e.jpg");
            Image image = Image.GetInstance(imagePath);
            image.ScaleToFit(100, 100); // Set the width and height of the logo
            image.Alignment = Element.ALIGN_CENTER;
            image.SpacingAfter = 20; // Add spacing after the image
            document.Add(image);

            // Add image before the title (centered)
            //string imagePath = Server.MapPath("~/Assets/_e407f44c-5341-4a3d-b20e-e7ae5a10e34e.jpg"); 
            //Image image = Image.GetInstance(imagePath);
            //image.Alignment = Element.ALIGN_CENTER;
            //image.SpacingAfter = 20; // Add spacing after the image
            //document.Add(image);

            // Create title
            Paragraph title = new Paragraph("SEC Customers Data", new Font(baseFont, 18, Font.BOLD));
            title.Alignment = Element.ALIGN_CENTER;
            title.SpacingAfter = 10; // Add spacing after the title
            document.Add(title);

            // Add current date (top right side)
            DateTime currentDate = DateTime.Now;
            string formattedDate = currentDate.ToString("yyyy-MM-dd");
            Paragraph dateParagraph = new Paragraph("Date: " + formattedDate, new Font(baseFont, 10));
            dateParagraph.Alignment = Element.ALIGN_RIGHT;
            dateParagraph.SpacingAfter = 4;
            document.Add(dateParagraph);

            // Create a table
            PdfPTable table = new PdfPTable(7);
            table.WidthPercentage = 100; // Set table width to 100% of the page width
            table.DefaultCell.BorderWidth = 1; // Add cell borders with width 1

            // Add column headers with borders
            AddCellWithBorders(table, "Customer ID", headerFont);
            AddCellWithBorders(table, "First Name", headerFont);
            AddCellWithBorders(table, "Mid Name", headerFont);
            AddCellWithBorders(table, "Last Name", headerFont);
            AddCellWithBorders(table, "Address", headerFont);
            AddCellWithBorders(table, "Number", headerFont);
            AddCellWithBorders(table, "Number (Op)", headerFont);

            // Add data rows with borders
            foreach (var item in data)
            {
                AddCellWithBorders(table, item.cID.ToString(), contentFont);
                AddCellWithBorders(table, item.cFirstName, contentFont);
                AddCellWithBorders(table, item.cMidName, contentFont);
                AddCellWithBorders(table, item.cLastName, contentFont);
                AddCellWithBorders(table, item.cAddress, contentFont);
                AddCellWithBorders(table, item.cNumber.ToString(), contentFont);
                AddCellWithBorders(table, item.cNumberOp.ToString(), contentFont);
            }

            // Add the table to the document
            document.Add(table);

            // Close the document
            document.Close();

            // Return the PDF file to the client
            return File(memoryStream.ToArray(), "application/pdf", "Customers Data.pdf");
        }

        // Helper method to add cell to table with specified content and font
        private void AddCellWithBorders(PdfPTable table, string content, Font font)
        {
            PdfPCell cell = new PdfPCell(new Phrase(content, font));
            cell.Padding = 5; // Add padding to the cell content
            cell.BorderWidth = 1; // Add cell borders with width 1
            table.AddCell(cell);
        }



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


        // Inserting Customers To the Database
        private void InsertCustomer(customerVM model)
        {
            using (SqlConnection connection = new SqlConnection(SecConn))
            {
                connection.Open();

                string query = "INSERT INTO CustomerTbl (cFirstName, cMidName, cLastName, cAddress, cNumber, cNumberOp) VALUES (@cFirstName, @cMidName, @cLastName, @cAddress, @cNumber, @cNumberOp)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@cFirstName", model.cFirstName);
                    command.Parameters.AddWithValue("@cMidName", model.cMidName);
                    command.Parameters.AddWithValue("@cLastName", model.cLastName);
                    command.Parameters.AddWithValue("@cAddress", model.cAddress);
                    command.Parameters.AddWithValue("@cNumber", SqlDbType.Int).Value = model.cNumber;
                    if (string.IsNullOrWhiteSpace(model.cNumberOp))
                    {
                        command.Parameters.AddWithValue("@cNumberOp", DBNull.Value);
                    }
                    else
                    {
                        command.Parameters.AddWithValue("@cNumberOp", model.cNumberOp);
                    }

                    command.ExecuteNonQuery();

                }
            }
        }


        // Method that retrieves the ID of the Customer to be updated
        private customerVM GetCustomerById(int cID)
        {
            using (SqlConnection connection = new SqlConnection(SecConn))
            {
                connection.Open();

                string query = "SELECT * FROM CustomerTbl WHERE cID = @cID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@cID", cID);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new customerVM
                            {
                                cID = Convert.ToInt32(reader["cID"]),
                                cFirstName = reader["cFirstName"].ToString(),
                                cMidName = reader["cMidName"].ToString(),
                                cLastName = reader["cLastName"].ToString(),
                                cAddress = reader["cAddress"].ToString(),
                                cNumber = Convert.ToInt32(reader["cNumber"]),
                                cNumberOp = reader["cNumberOp"].ToString()

                            };
                        }
                    }
                }
            }

            return null; 
        }


        // Update Customer Information Logic
        private void UpdateCustomer(customerVM model)
        {
            using (SqlConnection connection = new SqlConnection(SecConn))
            {
                connection.Open();

                string query = "UPDATE CustomerTbl SET cFirstName = @cFirstName, cMidName = @cMidName, cLastName = @cLastName, cAddress = @cAddress, cNumber = @cNumber, cNumberOp = @cNumberOp WHERE cID = @cID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@cID", model.cID);
                    command.Parameters.AddWithValue("@cFirstName", model.cFirstName);
                    command.Parameters.AddWithValue("@cMidName", model.cMidName);
                    command.Parameters.AddWithValue("@cLastName", model.cLastName);
                    command.Parameters.AddWithValue("@cAddress", model.cAddress);
                    command.Parameters.AddWithValue("@cNumber", SqlDbType.Int).Value = model.cNumber;
                    if (string.IsNullOrWhiteSpace(model.cNumberOp))
                    {
                        command.Parameters.AddWithValue("@cNumberOp", DBNull.Value);
                    }
                    else
                    {
                        command.Parameters.AddWithValue("@cNumberOp", model.cNumberOp);
                    }

                    command.ExecuteNonQuery();
                }
            }
        }

        // Customer Information Deletion Logic for the Delete ActionResult
        private void DeleteCustomer(int id)
        {
            using (SqlConnection connection = new SqlConnection(SecConn))
            {
                connection.Open();

                string query = "DELETE FROM CustomerTbl WHERE cID = @cID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@cID", id);

                    command.ExecuteNonQuery();
                }
            }
        }

    }
}