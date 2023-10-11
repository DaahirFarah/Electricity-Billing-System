﻿using EBS.viewModels;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.Mvc;

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

        [HttpPost]
        public JsonResult GetInvoiceData(int id)
        {
            using (SqlConnection connection = new SqlConnection(SecConn))
            {

                connection.Open();
                string query = "SELECT * FROM InvoiceTbl WHERE invoiceID = @invoiceID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@invoiceID", id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Populate the invoice object
                            invWrapper invoice = new invWrapper
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

                            // Return the Invoice Data as JSON
                            return Json(invoice, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
            }

            // If no data found, return an empty JSON object
            return Json(new invWrapper(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UpdateInvoice(invWrapper model)
        {
            if (ModelState.IsValid)
            {
                UpdateInvoiceMethod(model);
                return Json(new { success = true, message = "Invoice Updated Successfully!" });
            }
            return Json(new { success = false, message = "Invoice Update Failed. Try Again!" });
        }

        [HttpPost]
        public JsonResult Delete(int id)
        {
            DeleteInvoice(id);
            return Json(new { success = true, message = "Invoice Deleted Successfuly" });
        }

        // Get each customer's information and reading data from the db who is not billed based on their branch and pass it to the model
        [HttpPost]
        public JsonResult GetCustomerBillInfo(string branch)
        {
            List<invoiceVM> invoice = new List<invoiceVM>();
            using (SqlConnection connection = new SqlConnection(SecConn))
            {
                connection.Open();
                // String that holds the query to get the customers that are not billed yet and their latest meter reading based on the selected branch
                string query = ";WITH CTE AS (\r\n    SELECT \r\n        C.cID,\r\n        C.cFirstName, \r\n        C.cMidName, \r\n        C.cLastName, \r\n        C.Balance,\r\n        COALESCE(I.cur_Reading, 0) AS cur_Reading,\r\n        ROW_NUMBER() OVER (PARTITION BY C.cID ORDER BY I.invoiceID DESC) AS rn\r\n    FROM CustomerTbl C\r\n    LEFT JOIN InvoiceTbl I ON C.cID = I.cID\r\n    WHERE C.isBilledThisMonth = 0\r\n    AND C.Branch = @branch\r\n)\r\nSELECT cID, cFirstName, cMidName, cLastName, cur_Reading, Balance\r\nFROM CTE\r\nWHERE rn = 1";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@branch", branch);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            invoice.Add(new invoiceVM
                            {
                                cID = Convert.ToInt32(reader["cID"]),
                                balance = Convert.ToDecimal(reader["Balance"]),
                                prev_Reading = Convert.ToDecimal(reader["cur_Reading"]),
                                cFirstName = reader["cFirstName"].ToString(),
                                cMidName = reader["cMidName"].ToString(),
                                cLastName = reader["cLastName"].ToString(),

                            });
                        }
                    }
                }
            }

            return Json(invoice, JsonRequestBehavior.AllowGet);
        }

        // GET Branches
        [HttpGet]
        public List<string> GetBranches()
        {
            List<string> Branch = new List<string>();
            string query = "SELECT BranchName FROM Branches";

            using (SqlConnection connection = new SqlConnection(SecConn))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string branch = (string)reader["BranchName"];
                    Branch.Add(branch);
                }
            }



            return Branch;
        }

        // Get Related Data From the db based on ID
        [HttpPost]
        public JsonResult GetRelatedData(int id)
        {
            // Define a variable to hold the result
            invoiceVM result = new invoiceVM();

            using (SqlConnection connection = new SqlConnection(SecConn))
            {
                connection.Open();

                // Retrieve cur_Reading from InvoiceTbl
                string queryReading = "SELECT TOP 1 cur_Reading FROM InvoiceTbl WHERE cID = @cID ORDER BY invoiceID DESC";
                using (SqlCommand cmdReading = new SqlCommand(queryReading, connection))
                {
                    cmdReading.Parameters.AddWithValue("@cID", id);

                    using (SqlDataReader reader = cmdReading.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            result.cur_Reading = Convert.ToDecimal(reader["cur_Reading"]);
                        }
                    }
                }

                // Retrieve Balance from CustomerTbl
                string queryBalance = "SELECT Balance FROM CustomerTbl WHERE cID = @cID";
                using (SqlCommand cmdBalance = new SqlCommand(queryBalance, connection))
                {
                    cmdBalance.Parameters.AddWithValue("@cID", id);

                    using (SqlDataReader reader = cmdBalance.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            result.balance = Convert.ToDecimal(reader["Balance"]);
                        }
                    }
                }
            }

            // Return the result as JSON
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // Get Rates for bulk insertion 
        public JsonResult GetRate(decimal usage)
        {
            // variable to hold the rate fetched from database
            decimal Rate = 0;
            using (SqlConnection connection = new SqlConnection(SecConn))
            {
                connection.Open();
                // Retrieve the usage level and rate for each record
                string rateQuery = "SELECT UsageLevelNumber, Rate FROM Rates";
                List<int> usageLevel = new List<int>();
                List<decimal> rate = new List<decimal>();


                using (SqlCommand commandRate = new SqlCommand(rateQuery, connection))
                {
                    using (SqlDataReader reader = commandRate.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int intFromDatabase = reader.GetInt32(0);
                            decimal decimalFromDatabase = reader.GetDecimal(1);

                            usageLevel.Add(intFromDatabase);
                            rate.Add(decimalFromDatabase);
                        }
                    }
                    for (int i = 0; i < usageLevel.Count; i++)
                    {
                        if (usage < usageLevel[i])
                        {
                            Rate = rate[i];
                            break;
                        }
                    }
                }
            }

            return Json(Rate, JsonRequestBehavior.AllowGet);
        }

        // GET: /Invoices/BulkInsert
        [HttpGet]
        public ActionResult BulkInsert()
        {
            // Create invoiceVM
            invoiceVM model = new invoiceVM();

            // Populate model properties  
            model.SelectedBranch = GetBranches();

            // Create list with single item
            var modelList = new List<invoiceVM>();
            modelList.Add(model);

            return View(modelList);
        }

        // POST: /Invoices/BulkInsert
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult BulkInsert(List<invoiceVM> models)
        {
            using (SqlConnection connection = new SqlConnection(SecConn))
            {
                connection.Open();

                // Start a SQL transaction
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        foreach (var wrapper in models)
                        {
                            // Retrieve the usage level and rate for each record
                            string rateQuery = "SELECT UsageLevelNumber, Rate FROM Rates";
                            List<int> usageLevel = new List<int>();
                            List<decimal> rate = new List<decimal>();

                            using (SqlCommand commandRate = new SqlCommand(rateQuery, connection, transaction))
                            {
                                using (SqlDataReader reader = commandRate.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        int intFromDatabase = reader.GetInt32(0);
                                        decimal decimalFromDatabase = reader.GetDecimal(1);

                                        usageLevel.Add(intFromDatabase);
                                        rate.Add(decimalFromDatabase);
                                    }

                                    // Set the date to 25 of the month if the date field is empty
                                    if (wrapper.reading_Date == DateTime.MinValue)
                                    {
                                        DateTime currentDate = DateTime.Now;
                                        DateTime desiredDate = new DateTime(currentDate.Year, currentDate.Month, 28);
                                        wrapper.reading_Date = desiredDate;
                                    }
                                }
                            }

                            // Retrieve the customer's balance for each record
                            string balanceQuery = "SELECT BALANCE FROM CustomerTbl WHERE cID = @cID";
                            using (SqlCommand commandBalance = new SqlCommand(balanceQuery, connection, transaction))
                            {
                                commandBalance.Parameters.AddWithValue("@cID", wrapper.cID);
                                object balanceResult = commandBalance.ExecuteScalar();
                                if (balanceResult != null && balanceResult != DBNull.Value)
                                {
                                    wrapper.balance = Convert.ToDecimal(balanceResult);
                                }
                            }

                            // Calculate the rate and total fee for each record
                            for (int i = 0; i < usageLevel.Count; i++)
                            {
                                if (wrapper.reading_Value < usageLevel[i])
                                {
                                    wrapper.Rate = rate[i];
                                    wrapper.total_Fee = wrapper.reading_Value * wrapper.Rate;
                                    break;
                                }
                            }

                            // Insert the record into the database
                            string query = "INSERT INTO InvoiceTbl (cID, Rate, prev_Reading, cur_Reading, reading_Value, reading_Date, total_Fee, Status) "
                                         + "VALUES (@cID, @Rate, @prev_Reading, @cur_Reading, @reading_Value, @reading_Date, @total_Fee + @balance, @Status)";

                            using (SqlCommand command = new SqlCommand(query, connection, transaction))
                            {
                                string Status = "Unpaid";
                                wrapper.Status = Status;

                                command.Parameters.AddWithValue("@cID", wrapper.cID);
                                command.Parameters.AddWithValue("@Rate", wrapper.Rate);
                                command.Parameters.AddWithValue("@prev_Reading", wrapper.prev_Reading);
                                command.Parameters.AddWithValue("@cur_Reading", wrapper.cur_Reading);
                                command.Parameters.AddWithValue("@reading_Value", wrapper.reading_Value);
                                command.Parameters.AddWithValue("@reading_Date", SqlDbType.DateTime2).Value = wrapper.reading_Date;
                                command.Parameters.AddWithValue("@total_Fee", wrapper.total_Fee);
                                command.Parameters.AddWithValue("@balance", wrapper.balance);
                                command.Parameters.AddWithValue("@Status", wrapper.Status);

                                command.ExecuteNonQuery();
                            }
                        }

                        // Commit the transaction if everything is successful
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        // Roll back the transaction if an error occurs
                        transaction.Rollback();
                        ModelState.AddModelError("", "An Error Occurred While Inserting Records, Please Try Again!" + ex.Message);
                    }
                }

                // Update the customer's balance after the transaction is committed
                foreach (var wrapper in models)
                {
                    string updateBalanceQuery = "UPDATE CustomerTbl SET Balance = 0 WHERE cID = @cID";
                    using (SqlCommand updateBalanceCommand = new SqlCommand(updateBalanceQuery, connection))
                    {
                        updateBalanceCommand.Parameters.AddWithValue("@cID", wrapper.cID);
                        updateBalanceCommand.ExecuteNonQuery();
                    }
                }

                foreach (var wrapper in models)
                {
                    string billMarkquery = "UPDATE CustomerTbl SET isBilledThisMonth = 1 WHERE cID = @cID";
                    using (SqlCommand updateBalanceCommand = new SqlCommand(billMarkquery, connection))
                    {
                        updateBalanceCommand.Parameters.AddWithValue("@cID", wrapper.cID);
                        updateBalanceCommand.ExecuteNonQuery();
                    }
                }

                return RedirectToAction("Index");
            }

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

        private invWrapper GetInvoiceById(int invoiceID)
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
                            return new invWrapper
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
        private void UpdateInvoiceMethod(invWrapper model)
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


                string query = "UPDATE InvoiceTbl SET cID = @cID, Rate = @Rate,"
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
