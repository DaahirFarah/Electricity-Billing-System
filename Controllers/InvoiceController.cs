using EBS.viewModels;
using iTextSharp.text.pdf;
using iTextSharp.text;
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
using System.Collections;

namespace EBS.Controllers
{
    public class InvoiceController : Controller
    {
        // This variable holds the balance so that it can be accessed in all methods
        decimal balance;
        int cID;

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

        //POST: Update Invoice
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Display(Name = "Edit")]
        public ActionResult Edit(invoiceVM model)
        {
            if (ModelState.IsValid)
            {
                UpdateInvoice(model);
                return RedirectToAction("Index");
            }

            return View(model);
        }

        public ActionResult Delete(int id)
        {
            invoiceVM invoice = GetInvoiceById(id);
            return View(invoice);
        }

        [HttpPost]
        [ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            DeleteInvoice(id);
            return RedirectToAction("Index");
        }

        // This action handles exporting Invoices data from the database using a library called iTextSharp. 
        // This actionResult allows the user to easily download the list of Invoices in a pdf format 
        public ActionResult GenerateInvoice()
        {

            var data = GetAllInvoices();

            // Create a new PDF document
            MemoryStream memoryStream = new MemoryStream();
            Document document = new Document();
            PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
            document.Open();

            // Define font and table settings
            BaseFont baseFont = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            Font headerFont = new Font(baseFont, 12, Font.BOLD);
            Font contentFont = new Font(baseFont, 10);

            // Add image as a logo at the top of the page
            string imagePath = Server.MapPath("~/Assets/_e407f44c-5341-4a3d-b20e-e7ae5a10e34e.jpg");
            Image image = Image.GetInstance(imagePath);
            image.ScaleToFit(100, 100); // Set the width and height of the logo
            image.Alignment = Element.ALIGN_CENTER;
            image.SpacingAfter = 20; // Add spacing after the image
            document.Add(image);

            // Create title
            Paragraph title = new Paragraph("SEC Invoices Data", new Font(baseFont, 18, Font.BOLD));
            title.Alignment = Element.ALIGN_CENTER;
            title.SpacingAfter = 10; // Add spacing after the title
            document.Add(title);

            // Add current date (top right side)
            DateTime currentDate = DateTime.Now;
            string formattedDate = currentDate.ToString("yyyy-MM-dd");
            Paragraph dateParagraph = new Paragraph("Date: " + formattedDate, new Font(baseFont, 10));
            dateParagraph.Alignment = Element.ALIGN_RIGHT;
            dateParagraph.SpacingAfter = 5;
            document.Add(dateParagraph);

            // Create a table
            PdfPTable table = new PdfPTable(10); // Use 10 columns for your data
            table.WidthPercentage = 100; // Set table width to 100% of the page width
            table.SetWidths(new float[] { 1, 1, 1.4f, 1.4f, 1.2f, 1.2f, 1.3f, 1.1f, 1.2f, 1.7f }); // Adjust column widths
            table.DefaultCell.BorderWidth = 1; // Add cell borders with width 1

            // Add column headers with borders
            AddCellWithBorders(table, "Inv.ID", headerFont);
            AddCellWithBorders(table, "cID", headerFont);
            AddCellWithBorders(table, "B.P.Start", headerFont);
            AddCellWithBorders(table, "B.P.End", headerFont);
            AddCellWithBorders(table, "Prev.Reading", headerFont);
            AddCellWithBorders(table, "Cur.Reading", headerFont);
            AddCellWithBorders(table, "R.Date", headerFont);
            AddCellWithBorders(table, "Rate ($)", headerFont);
            AddCellWithBorders(table, "Usage (KwH)", headerFont);
            AddCellWithBorders(table, "Amount ($)", headerFont);

            //// Add data rows with borders
            // Add data rows with borders
            foreach (var item in data)
            {
                AddCellWithBorders(table, item.invoiceID.ToString(), contentFont);
                AddCellWithBorders(table, item.cID.ToString(), contentFont);
                AddCellWithBorders(table, item.billingPeriodStarts.ToString("yyyy-MM-dd"), contentFont);
                AddCellWithBorders(table, item.billingPeriodEnds.ToString("yyyy-MM-dd"), contentFont);
                AddCellWithBorders(table, item.prev_Reading.ToString(), contentFont);
                AddCellWithBorders(table, item.cur_Reading.ToString(), contentFont);
                AddCellWithBorders(table, item.reading_Date.ToString("yyyy-MM-dd"), contentFont);
                AddCellWithBorders(table, item.Rate.ToString(), contentFont);
                AddCellWithBorders(table, item.reading_Value.ToString(), contentFont);
                AddCellWithBorders(table, item.total_Fee.ToString(), contentFont);
            }
            // Add the table to the document
            document.Add(table);

            // Close the document
            document.Close();

            // Return the PDF file to the client
            return File(memoryStream.ToArray(), "application/pdf", "Invoices Data List.pdf");
        }

        // Helper method to add cell to table with specified content and font
        private void AddCellWithBorders(PdfPTable table, string content, Font font)
        {
            PdfPCell cell = new PdfPCell(new Phrase(content, font));
            cell.Padding = 5; // Add padding to the cell content
            cell.BorderWidth = 1; // Add cell borders with width 1
            table.AddCell(cell);
        }


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

      

        private void InsertInvoice(invoiceVM model)
        {
            using (SqlConnection connection = new SqlConnection(SecConn))
            {
                connection.Open();

                string balanceQuery = "SELECT BALANCE FROM CustomerTbl WHERE cID = @cID";

                cID = model.cID;

                using (SqlCommand commandBalance = new SqlCommand(balanceQuery, connection))
                {
                    commandBalance.Parameters.AddWithValue("@cID", model.cID);
                    object balanceResult = commandBalance.ExecuteScalar();
                    if (balanceResult != null && balanceResult != DBNull.Value)
                    {
                        balance = Convert.ToDecimal(balanceResult);
                        
                    }
                }

                string query = "INSERT INTO InvoiceTbl (cID, Rate, billingPeriodStarts,"
                             + "billingPeriodEnds, prev_Reading, cur_Reading, reading_Value, reading_Date, total_Fee) "
                             + "VALUES (@cID, @Rate, @billingPeriodStarts, @billingPeriodEnds, @prev_Reading, @cur_Reading,"
                             + "@reading_Value, @reading_Date, @total_Fee + @balance)";

                using (SqlCommand command = new SqlCommand(query, connection))
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
                    command.Parameters.AddWithValue("@balance", balance);

                    command.ExecuteNonQuery();


                    string updateBalanceQuery = "UPDATE CustomerTbl SET Balance = 0 WHERE cID = @cID";
                    using (SqlCommand updateBalanceCommand = new SqlCommand(updateBalanceQuery, connection))
                    {
                        
                        updateBalanceCommand.Parameters.AddWithValue("@cID", model.cID);
                        updateBalanceCommand.ExecuteNonQuery();
                    }

                    string balancehisq = "INSERT INTO BalanceHistory (cID, Balance) VALUES(@cID, @balance)";
                    using (SqlCommand comm = new SqlCommand(balancehisq, connection))
                    {
                        comm.Parameters.AddWithValue("@cID", model.cID);
                        comm.Parameters.AddWithValue("@Balance", balance);
                        comm.ExecuteNonQuery();
                    }
                }
            }

        }

        private invoiceVM GetInvoiceById(int invoiceID)
        {
            using (SqlConnection connection = new SqlConnection(SecConn))
            {
                connection.Open();
                string query = "SELECT * FROM InvoiceTbl WHERE invoiceID = @invoiceID";
                using (SqlCommand command = new SqlCommand(query, connection))
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

        // Update Invoice Logic
        private void UpdateInvoice(invoiceVM model)
        {
            using (SqlConnection connection = new SqlConnection(SecConn))
            {
                connection.Open();
                string query = "Update InvoiceTbl SET cID = @cID, Rate = @Rate, billingPeriodStarts = @billingPeriodStarts,"
                             + "billingPeriodEnds = @billingPeriodEnds, prev_Reading = @prev_Reading, cur_Reading = @cur_Reading,"
                             + "reading_Value = @reading_Value, reading_Date = @reading_Date, total_Fee = @total_Fee WHERE invoiceID = @invoiceID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@invoiceID", model.invoiceID);
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
                };
            }
        }

        // Delete Invoice Logic 





        private void DeleteInvoice(int id)
        {
            using (SqlConnection connection = new SqlConnection(SecConn))
            {
                connection.Open();
                string query = "DELETE FROM InvoiceTbl WHERE invoiceID = @invoiceID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@invoiceID", id);
                    command.ExecuteNonQuery();
                }
            }
        }


        // Method to retrieve Balance from CustomerTbl
        public ActionResult GetBalance(string cID)
        {
            decimal balance = 0;

            using (SqlConnection connection = new SqlConnection(SecConn))
            {
                connection.Open();

                string query = "SELECT Balance FROM CustomerTbl WHERE cID = @cID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@cID", cID);

                    object result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        balance = Convert.ToDecimal(result);
                    }
                }
            }

            return Json(new { balance = balance }, JsonRequestBehavior.AllowGet);
        }

    }
}
