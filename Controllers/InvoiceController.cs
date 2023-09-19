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
    [Authorize]
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
            invWrapper wrapper = new invWrapper();
            wrapper.invoiceList = GetAllInvoices();
            return View(wrapper);
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
        public ActionResult Create(invWrapper model)
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

        // Get Related Data From the db based on ID

        [HttpPost]
        public JsonResult GetRelatedData(int id)
        {
            // Create a connection to your SQL Server database
            using (SqlConnection connection = new SqlConnection(SecConn))
            {
                connection.Open();

                // Define your SQL query to retrieve user profile data based on the provided ID
                string query = "SELECT TOP 1 cur_Reading FROM InvoiceTbl WHERE cID = @cID ORDER BY invoiceID DESC";

                // Create a SqlCommand
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    // Add the ID parameter
                    cmd.Parameters.AddWithValue("@cID", id);

                    // Execute the query and read the data
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Populate the UserProfile object
                            invoiceVM invoice = new invoiceVM
                            {
                               cur_Reading = Convert.ToDecimal(reader["cur_Reading"]),
                            };

                            // Return the UserProfile as JSON
                            return Json(invoice, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
            }

            // If no data found, return an empty JSON object
            return Json(new invoiceVM(), JsonRequestBehavior.AllowGet);
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
            Paragraph title = new Paragraph("SEC Invoices History", new Font(baseFont, 18, Font.BOLD));
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
            return File(memoryStream.ToArray(), "application/pdf", "Invoices History.pdf");
        }

        // Helper method to add cell to table with specified content and font
        private void AddCellWithBorders(PdfPTable table, string content, Font font)
        {
            PdfPCell cell = new PdfPCell(new Phrase(content, font));
            cell.Padding = 5; // Add padding to the cell content
            cell.BorderWidth = 1; // Add cell borders with width 1
            table.AddCell(cell);
        }

        // Generating Individual Invoices 
        public ActionResult CustomerBill(int id)
        {
            // Simulate retrieving invoice data from your database based on invoiceId
            var invoice = GetInvoiceById(id);



            // Create a new document with a smaller page size
            Document document = new Document(PageSize.A5, 30, 30, 30, 30);

            // Specify the memory stream as the output
            MemoryStream memoryStream = new MemoryStream();
            PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);

            // Open the document for writing
            document.Open();

            // Add image as a logo at the top of the page
            string imagePath = Server.MapPath("~/Assets/_e407f44c-5341-4a3d-b20e-e7ae5a10e34e.jpg");
            Image image = Image.GetInstance(imagePath);
            image.ScaleToFit(100, 100); // Set the width and height of the logo
            image.Alignment = Element.ALIGN_CENTER;
            image.SpacingAfter = 20; // Add spacing after the image
            document.Add(image);

            // Add the title "Somali Electric Company"
            Font titleFont = FontFactory.GetFont("Times-Roman", 18);
            Paragraph title = new Paragraph("Somali Electric Company", titleFont);
            title.Alignment = Element.ALIGN_CENTER;
            document.Add(title);

            // Add the "Electricity Bill or Invoice" text
            Font subtitleFont = FontFactory.GetFont("Times-Roman", 14);
            Paragraph subtitle = new Paragraph("Invoice", subtitleFont);
            subtitle.Alignment = Element.ALIGN_CENTER;
            document.Add(subtitle);

            // Add current date (top right side)
            DateTime currentDate = DateTime.Now;
            string formattedDate = currentDate.ToString("yyyy-MM-dd");
            Paragraph dateParagraph = new Paragraph("Date: " + formattedDate);
            dateParagraph.Alignment = Element.ALIGN_RIGHT;
            dateParagraph.SpacingAfter = 5;
            document.Add(dateParagraph);

            // Add the invoice details
            Font contentFont = FontFactory.GetFont("Times-Roman", 12);
            contentFont.Color = BaseColor.BLACK;

            // Define line spacing
            float lineSpacing = 20f;

            // Add the invoice data to the document
            AddInvoiceLine(document, $"Reading Date: {invoice.reading_Date.ToString("dd/MM/yyyy")}", contentFont, lineSpacing);
            AddInvoiceLine(document, $"Invoice ID: {invoice.invoiceID}", contentFont, lineSpacing);
            AddInvoiceLine(document, $"Customer ID: {invoice.cID}", contentFont, lineSpacing);
            AddInvoiceLine(document, $"Previous Reading: {invoice.prev_Reading} (KwH)", contentFont, lineSpacing);
            AddInvoiceLine(document, $"Current Reading: {invoice.cur_Reading} (KwH)", contentFont, lineSpacing);
            AddInvoiceLine(document, $"Usage in (KwH): {invoice.reading_Value}", contentFont, lineSpacing);
            AddInvoiceLine(document, $"Rate: {invoice.Rate:C}", contentFont, lineSpacing);
            AddInvoiceLine(document, $"Total Amount: {invoice.total_Fee:C}", contentFont, lineSpacing);

            // Close the document
            document.Close();

            // Set the response content type and headers
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", $"attachment;filename=Invoice.pdf");

            // Write the PDF to the response stream
            Response.BinaryWrite(memoryStream.ToArray());
            Response.End();

            return View();
        }

        private void AddInvoiceLine(Document document, string line, Font font, float lineSpacing)
        {
            Paragraph paragraph = new Paragraph(line, font);
            paragraph.SpacingAfter = lineSpacing;
            document.Add(paragraph);
        }

        // Fetching Invoices From the Database
        private List<invoicevmList> GetAllInvoices()
        {
            List<invoicevmList> invoices = new List<invoicevmList>();

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
                            invoices.Add(new invoicevmList
                            {
                                invoiceID = Convert.ToInt32(reader["invoiceID"]),
                                cID = Convert.ToInt32(reader["cID"]),
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

      

        private void InsertInvoice(invWrapper model)
        {
            using (SqlConnection connection = new SqlConnection(SecConn))
            {
                connection.Open();

                // This code will retrieve all the different rates from the Rate table and every client's rate will be based on their usage
                string rateQuery = "SELECT UsageLevelNumber, Rate FROM Rates";
                List<int> usageLevel = new List<int>();            // This list will hold the usage level number retrieved from the database
                List<decimal> rate = new List<decimal>();         // This list will hold the rate fetched from the database

                using (SqlCommand commandRate = new SqlCommand(rateQuery, connection))
                {
                    using (SqlDataReader reader = commandRate.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Retrieve values from the database and add them to the lists
                            int intFromDatabase = reader.GetInt32(0);
                            decimal decimalFromDatabase = reader.GetDecimal(1);

                            usageLevel.Add(intFromDatabase);
                            rate.Add(decimalFromDatabase);
                        }
                    }
                }


                // This chunk of code retrieves the standing customer balance so that the total can be the usage fee + the current balance
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

                // this piece of code sets the value of the rate and total fee based on the client usage 
                for (int i = 0; i < usageLevel.Count; i++)
                {
                    if (model.reading_Value < usageLevel[i])
                    {
                        // Multiply SomePropertyToCompare by the corresponding value in decimalList
                        model.Rate = rate[i];
                        model.total_Fee = model.reading_Value * model.Rate;                      
                        break;

                    }
                }

                // Below this is the code that does the insertion operation and setting the balance to 0 after it is added to the total.
                string query = "INSERT INTO InvoiceTbl (cID, Rate, "
                             + "prev_Reading, cur_Reading, reading_Value, reading_Date, total_Fee) "
                             + "VALUES (@cID, @Rate, @prev_Reading, @cur_Reading,"
                             + "@reading_Value, @reading_Date, @total_Fee + @balance)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@cID", model.cID);
                    command.Parameters.AddWithValue("@Rate", model.Rate);
                    command.Parameters.AddWithValue("@prev_Reading", model.prev_Reading);
                    command.Parameters.AddWithValue("@cur_Reading", model.cur_Reading);
                    command.Parameters.AddWithValue("@reading_Value", model.reading_Value);
                    command.Parameters.AddWithValue("@reading_Date", SqlDbType.DateTime2).Value = model.reading_Date;
                    command.Parameters.AddWithValue("@total_Fee", model.total_Fee);
                    command.Parameters.AddWithValue("@balance", balance);

                    command.ExecuteNonQuery();


                    string updateBalanceQuery = "UPDATE CustomerTbl SET Balance = 0 WHERE cID = @cID";
                    using (SqlCommand updateBalanceCommand = new SqlCommand(updateBalanceQuery, connection))
                    {
                        
                        updateBalanceCommand.Parameters.AddWithValue("@cID", model.cID);
                        updateBalanceCommand.ExecuteNonQuery();
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

                string rateQuery = "SELECT UsageLevelNumber, Rate FROM Rates";
                List<int> usageLevel = new List<int>();            // This list will hold the usage level number retrieved from the database
                List<decimal> rate = new List<decimal>();         // This list will hold the rate fetched from the database

                using (SqlCommand commandRate = new SqlCommand(rateQuery, connection))
                {
                    using (SqlDataReader reader = commandRate.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Retrieve values from the database and add them to the lists
                            int intFromDatabase = reader.GetInt32(0);
                            decimal decimalFromDatabase = reader.GetDecimal(1);

                            usageLevel.Add(intFromDatabase);
                            rate.Add(decimalFromDatabase);
                        }
                    }
                }


                // This chunk of code retrieves the standing customer balance so that the total can be the usage fee + the current balance
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

                // this piece of code sets the value of the rate and total fee based on the client usage 
                for (int i = 0; i < usageLevel.Count; i++)
                {
                    if (model.reading_Value < usageLevel[i])
                    {
                        // Multiply SomePropertyToCompare by the corresponding value in decimalList
                        model.Rate = rate[i];
                        model.total_Fee = model.reading_Value * model.Rate;
                        break;

                    }
                }


                string query = "Update InvoiceTbl SET cID = @cID, Rate = @Rate,"
                             + "prev_Reading = @prev_Reading, cur_Reading = @cur_Reading,"
                             + "reading_Value = @reading_Value, reading_Date = @reading_Date, total_Fee = @total_Fee WHERE invoiceID = @invoiceID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@invoiceID", model.invoiceID);
                    command.Parameters.AddWithValue("@cID", model.cID);
                    command.Parameters.AddWithValue("@Rate", model.Rate);
                    command.Parameters.AddWithValue("@prev_Reading", model.prev_Reading);
                    command.Parameters.AddWithValue("@cur_Reading", model.cur_Reading);
                    command.Parameters.AddWithValue("@reading_Value", model.reading_Value);
                    command.Parameters.AddWithValue("@reading_Date", SqlDbType.DateTime2).Value = model.reading_Date;
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
