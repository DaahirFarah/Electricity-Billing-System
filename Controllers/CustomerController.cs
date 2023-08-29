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
            //// Retrieve data from the database using ADO.NET
            //List<customerVM> data = GetAllCustomers();

            //// Create a new PDF document
            //PdfDocument document = new PdfDocument();
            //PdfPage page = document.AddPage();
            //XGraphics gfx = XGraphics.FromPdfPage(page);

            //// Define font and table settings
            //XFont headerFont = new XFont("Georgia", 7, XFontStyle.Bold);
            //XFont contentFont = new XFont("Georgia", 5);
            //int yPosition = 100;
            //int rowHeight = 30;

            //// Define column widths
            //int col1Width = 50;
            //int col2Width = 100;
            //int col3Width = 100;
            //int col4Width = 100;
            //int col5Width = 150;
            //int col6Width = 100;
            //int col7Width = 150;

            //// Draw column headers
            //gfx.DrawString("Customer ID", headerFont, XBrushes.Black, new XPoint(50, yPosition));
            //gfx.DrawString("First Name", headerFont, XBrushes.Black, new XPoint(50 + col1Width, yPosition));
            //gfx.DrawString("Mid Name", headerFont, XBrushes.Black, new XPoint(50 + col1Width + col2Width, yPosition));
            //gfx.DrawString("Last Name", headerFont, XBrushes.Black, new XPoint(50 + col1Width + col2Width + col3Width, yPosition));
            //gfx.DrawString("Address", headerFont, XBrushes.Black, new XPoint(50 + col1Width + col2Width + col3Width + col4Width, yPosition));
            //gfx.DrawString("Number", headerFont, XBrushes.Black, new XPoint(50 + col1Width + col2Width + col3Width + col4Width + col5Width, yPosition));
            //gfx.DrawString("Number (Op)", headerFont, XBrushes.Black, new XPoint(50 + col1Width + col2Width + col3Width + col4Width + col5Width + col6Width, yPosition));
            //yPosition += rowHeight;

            //// Draw data rows
            //foreach (var item in data)
            //{
            //    // Draw borders around the cells
            //    gfx.DrawRectangle(XPens.Black, new XRect(50, yPosition, col1Width, rowHeight));
            //    gfx.DrawRectangle(XPens.Black, new XRect(50 + col1Width, yPosition, col2Width, rowHeight));
            //    gfx.DrawRectangle(XPens.Black, new XRect(50 + col1Width + col2Width, yPosition, col3Width, rowHeight));
            //    gfx.DrawRectangle(XPens.Black, new XRect(50 + col1Width + col2Width + col3Width, yPosition, col4Width, rowHeight));
            //    gfx.DrawRectangle(XPens.Black, new XRect(50 + col1Width + col2Width + col3Width + col4Width, yPosition, col5Width, rowHeight));
            //    gfx.DrawRectangle(XPens.Black, new XRect(50 + col1Width + col2Width + col3Width + col4Width + col5Width, yPosition, col6Width, rowHeight));
            //    gfx.DrawRectangle(XPens.Black, new XRect(50 + col1Width + col2Width + col3Width + col4Width + col5Width + col6Width, yPosition, col7Width, rowHeight));

            //    // Draw data within the cells
            //    gfx.DrawString(item.cID.ToString(), contentFont, XBrushes.Black, new XPoint(50 + 5, yPosition + 5));
            //    gfx.DrawString(item.cFirstName, contentFont, XBrushes.Black, new XPoint(50 + col1Width + 5, yPosition + 5));
            //    gfx.DrawString(item.cMidName, contentFont, XBrushes.Black, new XPoint(50 + col1Width + col2Width + 5, yPosition + 5));
            //    gfx.DrawString(item.cLastName, contentFont, XBrushes.Black, new XPoint(50 + col1Width + col2Width + col3Width + 5, yPosition + 5));
            //    gfx.DrawString(item.cAddress, contentFont, XBrushes.Black, new XPoint(50 + col1Width + col2Width + col3Width + col4Width + 5, yPosition + 5));
            //    gfx.DrawString(item.cNumber.ToString(), contentFont, XBrushes.Black, new XPoint(50 + col1Width + col2Width + col3Width + col4Width + col5Width + 5, yPosition + 5));
            //    gfx.DrawString(item.cNumberOp.ToString(), contentFont, XBrushes.Black, new XPoint(50 + col1Width + col2Width + col3Width + col4Width + col5Width + col6Width + 5, yPosition + 5));

            //    // Move to the next row's Y-coordinate
            //    yPosition += rowHeight;
            //}

            //// Save the PDF to a memory stream
            //MemoryStream stream = new MemoryStream();
            //document.Save(stream, false);

            //// Return the PDF file to the client
            //return File(stream.ToArray(), "application/pdf", "Customers List.pdf");

            var data = GetAllCustomers();

            // Create a new PDF document
            MemoryStream memoryStream = new MemoryStream();
            Document document = new Document();
            PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
            document.Open();

            // Define font and table settings
            BaseFont baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            Font headerFont = new Font(baseFont, 14, Font.BOLD);
            Font contentFont = new Font(baseFont, 12);

            // Create a table
            PdfPTable table = new PdfPTable(7);
            table.DefaultCell.BorderWidth = 1;

            // Add header row
            table.AddCell(new PdfPCell(new Phrase("Customer ID", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("First Name", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("Mid Name", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("Last Name", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("Address", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("Number", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("Number (Op)", headerFont)));

            // Add data rows
            foreach (var item in data)
            {
                table.AddCell(new PdfPCell(new Phrase(item.cID.ToString(), contentFont)));
                table.AddCell(new PdfPCell(new Phrase(item.cFirstName, contentFont)));
                table.AddCell(new PdfPCell(new Phrase(item.cMidName, contentFont)));
                table.AddCell(new PdfPCell(new Phrase(item.cLastName, contentFont)));
                table.AddCell(new PdfPCell(new Phrase(item.cAddress, contentFont)));
                table.AddCell(new PdfPCell(new Phrase(item.cNumber.ToString(), contentFont)));
                table.AddCell(new PdfPCell(new Phrase(item.cNumberOp.ToString(), contentFont)));
            }

            // Add the table to the document
            document.Add(table);

            // Close the document
            document.Close();

            // Return the PDF file to the client
            return File(memoryStream.ToArray(), "application/pdf", "CustomersList.pdf");

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