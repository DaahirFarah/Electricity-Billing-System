using EBS.viewModels;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using System.Data;
using Newtonsoft.Json;

namespace EBS.Controllers
{
    //[Authorize]
    public class PaymentController : Controller
    {
        // ConnectionString Instance
        private readonly string SecConn = ConfigurationManager.ConnectionStrings["SecConn"].ConnectionString;

        // GET: Payment
        public ActionResult Index()
        {
            payWrapper wrapper = new payWrapper();
            wrapper.pay = GetAllPayments();
            return View(wrapper);
        }

        //GET: Record Payment
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        //SET: Record Payment
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(payWrapper model)
        {
            if (ModelState.IsValid)
            {
                InsertPayment(model);
                return RedirectToAction("Index");
            }
            return View(model);
        }

        // POST: GET Payment Info From Database
        public JsonResult GetPaymentData(int id)
        {
            using (SqlConnection connection = new SqlConnection(SecConn))
            {

                connection.Open();
                string query = "SELECT * FROM PaymentTbl WHERE payID = @payID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@payID", id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Populate the invoice object
                            payWrapper pay = new payWrapper
                            {
                                payID = Convert.ToInt32(reader["payID"]),
                                cID = Convert.ToInt32(reader["cID"]),
                                invoiceID = Convert.ToInt32(reader["invoiceID"]),
                                paidAmount = Convert.ToDecimal(reader["paidAmount"]),
                                totalFee = Convert.ToDecimal(reader["totalFee"]),
                                payMethod = Convert.ToString(reader["payMethod"]),
                                payDate = Convert.ToDateTime(reader["payDate"])
                            };

                            // Return the Invoice Data as JSON
                            return Json(pay, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
            }

            // If no data found, return an empty JSON object
            return Json(new payWrapper(), JsonRequestBehavior.AllowGet);
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


        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UpdatePayment(payWrapper model)
        {
            if (ModelState.IsValid)
            {
                UpdatePaymentMethod(model);
                return Json(new { success = true, message = "Payment Updated Successfully!" });
            }
            return Json(new { success = false, message = "Payment Update Failed. Please Try Again!" });
        }

        // POST: Delete Payment
        [HttpPost]
        public JsonResult Delete(int id)
        {
            DeletePayment(id);
            return Json(new { success = true, message = "Payment Deleted Successfully!" });
        }

        // Get Unpaid Invoices Data For Recording 
        [HttpPost]
        public JsonResult GetBillInfo(string branch)
        {
            List<payVM> payment = new List<payVM>();
            using (SqlConnection connection = new SqlConnection(SecConn))
            {
                connection.Open();
                // String that holds the query to get the customers that are not billed yet and their latest meter reading based on the selected branch
                string query = ";WITH CTE AS (\r\n    SELECT\r\n        C.cID,\r\n        C.cFirstName,\r\n        C.cMidName,\r\n        C.cLastName,\r\n        I.invoiceID,\r\n        I.total_Fee\r\n    FROM CustomerTbl C\r\n    JOIN InvoiceTbl I ON C.cID = I.cID\r\n    WHERE I.Status = 'Unpaid' AND C.Branch = @branch \r\n)\r\nSELECT\r\n    cID,\r\n    cFirstName,\r\n    cMidName,\r\n    cLastName,\r\n    invoiceID,\r\n    total_Fee\r\nFROM CTE;";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@branch", branch);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            payment.Add(new payVM
                            {
                                cID = Convert.ToInt32(reader["cID"]),
                                invoiceID = Convert.ToInt32(reader["invoiceID"]),
                                totalFee = Convert.ToDecimal(reader["total_Fee"]),
                                cFirstName = reader["cFirstName"].ToString(),
                                cMidName = reader["cMidName"].ToString(),
                                cLastName = reader["cLastName"].ToString(),

                            });
                        }
                    }
                }
            }

            return Json(payment, JsonRequestBehavior.AllowGet);
        }

        // Bulk Insertion  For Payments. upto Five at a time
        // GET: /Meter/BulkInsertion
        [HttpGet]
        public ActionResult BulkInsert()
        {
            // Create invoiceVM
            payVM model = new payVM();

            // Populate model properties  
            model.SelectedBranch = GetBranches();

            // Create list with single item
            var modelList = new List<payVM>();
            modelList.Add(model);

            return View(modelList);
        }

        // POST: Meter/BulkInsertion
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult BulkInsert(List<payVM> models)
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
                                command.CommandText = "INSERT INTO PaymentTbl (cID, invoiceID, paidAmount, totalFee, payMethod, payDate )"
                                                    + "VALUES (@cID, @invoiceID, @paidAmount, @totalFee, @payMethod, @payDate)";

                                foreach (var wrapper in models)
                                {
                                    if (wrapper.payDate == DateTime.MinValue)
                                    {
                                        DateTime currentDate = DateTime.Now;
                                        DateTime desiredDate = new DateTime(currentDate.Year, currentDate.Month, 28);
                                        wrapper.payDate = desiredDate;
                                    }

                                    command.Parameters.AddWithValue("@cID", wrapper.cID);
                                    command.Parameters.AddWithValue("@invoiceID", wrapper.invoiceID);
                                    command.Parameters.AddWithValue("@paidAmount", wrapper.paidAmount);
                                    command.Parameters.AddWithValue("@totalFee", wrapper.totalFee);
                                    command.Parameters.AddWithValue("@payMethod", wrapper.payMethod);
                                    command.Parameters.Add("@payDate", SqlDbType.DateTime2).Value = wrapper.payDate;

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
                            ModelState.AddModelError("", "An Error Occurred While Inserting Records, Please Try Again!" + ex.Message);
                        }
                    }

                    // Now, outside the transaction, calculate the balance and update CustomerTbl
                    foreach (var wrapper in models)
                    {
                        decimal balanceDifference = wrapper.totalFee - wrapper.paidAmount;
                        string updateBalanceQuery = "UPDATE CustomerTbl SET Balance = Balance + @balanceDifference WHERE cID = @cID";

                        using (SqlCommand updateBalanceCommand = new SqlCommand(updateBalanceQuery, connection))
                        {
                            updateBalanceCommand.Parameters.AddWithValue("@balanceDifference", balanceDifference);
                            updateBalanceCommand.Parameters.AddWithValue("@cID", wrapper.cID);
                            updateBalanceCommand.ExecuteNonQuery();
                        }

                    }
                    foreach (var wrapper in models)
                    {

                        string updateStatusQuery = "UPDATE InvoiceTbl SET Status = 'Paid' WHERE invoiceID = @invoiceID";

                        using (SqlCommand updateStatusCommand = new SqlCommand(updateStatusQuery, connection))
                        {
                            updateStatusCommand.Parameters.AddWithValue("@invoiceID", wrapper.invoiceID);
                            updateStatusCommand.ExecuteNonQuery();
                        }
                    }
                }

                return RedirectToAction("Index"); // Redirect to a success page
            }
            catch (Exception ex)
            {
                // Handle the exception and provide appropriate error feedback to the user
                ModelState.AddModelError("", "An error occurred while inserting data: " + ex.Message);
                return View(models); // Show the input form with error messages
            }
        }

        // print receipt for specific customer (one at a time).
        // Generating Individual Receipts 
        public ActionResult GenerateReceipt(string modelJson)
        {
            payVM model = JsonConvert.DeserializeObject<payVM>(modelJson);
            using (MemoryStream memoryStream = new MemoryStream())
            {
                if (model.payDate == DateTime.Now)
                {
                    DateTime curentDate = DateTime.Now;
                    DateTime desiredDate = new DateTime(curentDate.Year, curentDate.Month, 28);
                    model.payDate = desiredDate;
                }

                Document document = new Document(PageSize.A5, 30, 30, 30, 30);
                PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);

                document.Open();

                // Add image as a logo at the top of the page
                string imagePath = Server.MapPath("~/Assets/_e407f44c-5341-4a3d-b20e-e7ae5a10e34e.jpg");
                Image image = Image.GetInstance(imagePath);
                image.ScaleToFit(100, 100);
                image.Alignment = Element.ALIGN_CENTER;
                image.SpacingAfter = 20;
                document.Add(image);

                // Add the title "Somali Electric Company"
                Font titleFont = FontFactory.GetFont("Times-Roman", 18);
                Paragraph title = new Paragraph("Somali Electric Company", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                document.Add(title);

                // Add the "Electricity Bill or Invoice" text
                Font subtitleFont = FontFactory.GetFont("Times-Roman", 14);
                Paragraph subtitle = new Paragraph("Receipt", subtitleFont);
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
                float lineSpacing = 20f;

                AddInvoiceLine(document, $"Payment Date:            {model.payDate.ToString("dd/MM/yyyy")}", contentFont, lineSpacing);
                AddInvoiceLine(document, $"Invoice ID:              {model.invoiceID}", contentFont, lineSpacing);
                AddInvoiceLine(document, $"Customer ID:             {model.cID}", contentFont, lineSpacing);
                AddInvoiceLine(document, $"Customer Name:           {model.fullName}", contentFont, lineSpacing);
                AddInvoiceLine(document, $"Paid Amount:             {model.paidAmount:C}", contentFont, lineSpacing);
                AddInvoiceLine(document, $"Total Amount:            {model.totalFee:C}", contentFont, lineSpacing);
                AddInvoiceLine(document, $"Pay Method:              {model.payMethod}", contentFont, lineSpacing);

                document.Close();

                // Set the response content type and headers for download
                Response.Clear();
                Response.ContentType = "application/pdf";
                Response.AddHeader("content-disposition", "attachment;filename=CustomerBill.pdf");
                Response.BinaryWrite(memoryStream.ToArray());
                Response.Flush();

                return new EmptyResult();
            }
        }

        // Retrieve the Invoice Data related to the an ID
        [HttpPost]
        public JsonResult GetRelatedData(int id)
        {
            using (SqlConnection connection = new SqlConnection(SecConn))
            {
                connection.Open();

                string invDataQuery = "SELECT cID, total_Fee FROM InvoiceTbl WHERE invoiceID = @invoiceID";

                // Create a SqlCommand
                using (SqlCommand cmd = new SqlCommand(invDataQuery, connection))
                {
                    // Add the ID parameter
                    cmd.Parameters.AddWithValue("@invoiceID", id);

                    // Execute the query and read the data
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            payVM payment = new payVM
                            {
                                cID = Convert.ToInt32(reader["cID"]),
                                totalFee = Convert.ToDecimal(reader["total_Fee"]),
                            };

                            return Json(payment, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
            }

            // If no data found, return an empty JSON object
            return Json(new payVM(), JsonRequestBehavior.AllowGet);
        }


        // Logic for retrieving payment data from the database
        public List<payVM> GetAllPayments()
        {
            List<payVM> payments = new List<payVM>();

            using (SqlConnection connection = new SqlConnection(SecConn))
            {
                connection.Open();
                string query = "SELECT * FROM PaymentTbl";

                using(SqlCommand command = new SqlCommand(query, connection))
                {
                    using(SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            payments.Add(new payVM()
                            {
                                payID = Convert.ToInt32(reader["payID"]),
                                cID = Convert.ToInt32(reader["cID"]),
                                invoiceID = Convert.ToInt32(reader["invoiceID"]),
                                paidAmount = Convert.ToDecimal(reader["paidAmount"]),
                                totalFee = Convert.ToDecimal(reader["totalFee"]),
                                payMethod = Convert.ToString(reader["payMethod"]),
                                payDate = Convert.ToDateTime(reader["payDate"]),

                            });
                        }
                    }
                }
            }


            return payments;
        }

        // This action handles exporting Invoices data from the database using a library called iTextSharp. 
        // This actionResult allows the user to easily download the list of Invoices in a pdf format 
        public ActionResult GeneratePaymentHistory()
        {

            var data = GetAllPayments();

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
            Paragraph title = new Paragraph("SEC Payment History", new Font(baseFont, 18, Font.BOLD));
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
            PdfPTable table = new PdfPTable(6); // Use 10 columns for your data
            table.WidthPercentage = 100; // Set table width to 100% of the page width
            table.DefaultCell.BorderWidth = 1; // Add cell borders with width 1

            // Add column headers with borders
            AddCellWithBorders(table, "Payment ID", headerFont);
            AddCellWithBorders(table, "Cus. ID", headerFont);
            AddCellWithBorders(table, "Inv. ID", headerFont);
            AddCellWithBorders(table, "Paid ($)", headerFont);
            AddCellWithBorders(table, "Method", headerFont);
            AddCellWithBorders(table, "Payment Date", headerFont);

            //// Add data rows with borders
            // Add data rows with borders
            foreach (var item in data)
            {
                AddCellWithBorders(table, item.payID.ToString(), contentFont);
                AddCellWithBorders(table, item.cID.ToString(), contentFont);
                AddCellWithBorders(table, item.invoiceID.ToString(), contentFont);
                AddCellWithBorders(table, item.paidAmount.ToString(), contentFont);
                AddCellWithBorders(table, item.payMethod.ToString(), contentFont);
                AddCellWithBorders(table, item.payDate.ToString("yyyy-MM-dd"), contentFont);

            }
            // Add the table to the document
            document.Add(table);

            // Close the document
            document.Close();

            // Return the PDF file to the client
            return File(memoryStream.ToArray(), "application/pdf", "Payment History.pdf");
        }

        // Helper method to add cell to table with specified content and font
        private void AddCellWithBorders(PdfPTable table, string content, Font font)
        {
            PdfPCell cell = new PdfPCell(new Phrase(content, font));
            cell.Padding = 5; // Add padding to the cell content
            cell.BorderWidth = 1; // Add cell borders with width 1
            table.AddCell(cell);
        }


        // Receipt Generation
        // Generating Individual Invoices 
        public ActionResult Receipt(int id)
        {
            // Simulate retrieving invoice data from your database based on invoiceId
            var invoice = GetPaymentByID(id);



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
            Paragraph subtitle = new Paragraph("Receipt", subtitleFont);
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
            AddInvoiceLine(document, $"Reading Date: {invoice.payID.ToString()}", contentFont, lineSpacing);
            AddInvoiceLine(document, $"Invoice ID: {invoice.invoiceID}", contentFont, lineSpacing);
            AddInvoiceLine(document, $"Customer ID: {invoice.cID}", contentFont, lineSpacing);
            AddInvoiceLine(document, $"Paid Amount ($): {invoice.paidAmount}", contentFont, lineSpacing);
            AddInvoiceLine(document, $"Total Fee ($): {invoice.totalFee}", contentFont, lineSpacing);
            AddInvoiceLine(document, $"Payment Method: {invoice.payMethod}", contentFont, lineSpacing);
            AddInvoiceLine(document, $"Payment Date: {invoice.payDate.ToString("dd/MM/yyyy")}", contentFont, lineSpacing);
            

            // Close the document
            document.Close();

            // Set the response content type and headers
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", $"attachment;filename=Receipt.pdf");

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



        // Method that holds payment insertion logic

        private void InsertPayment(payWrapper model)
        {
            using (SqlConnection connection = new SqlConnection(SecConn))
            {
                connection.Open();
                string query = "INSERT INTO PaymentTbl (cID, invoiceID, paidAmount,"
                             + "totalFee, payMethod, payDate ) "
                             + "VALUES (@cID, @invoiceID, @paidAmount, @totalFee, @payMethod, @payDate)";
                           

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@cID", model.cID);
                    command.Parameters.AddWithValue("@invoiceID", model.invoiceID);
                    command.Parameters.AddWithValue("@paidAmount", model.paidAmount);
                    command.Parameters.AddWithValue("@totalFee", model.totalFee);
                    command.Parameters.AddWithValue("@payMethod", model.payMethod);
                    command.Parameters.AddWithValue("@payDate", SqlDbType.DateTime2).Value = model.payDate;

                    command.ExecuteNonQuery();

                    // Calculate the difference between paidAmount and totalFee
                    decimal balanceDifference = model.totalFee - model.paidAmount;

                    // Update CustomerTbl with the balance difference
                    string updateBalanceQuery = "UPDATE CustomerTbl SET Balance = Balance + @balanceDifference WHERE cID = @cID";
                    using (SqlCommand updateBalanceCommand = new SqlCommand(updateBalanceQuery, connection))
                    {
                        updateBalanceCommand.Parameters.AddWithValue("@balanceDifference", balanceDifference);
                        updateBalanceCommand.Parameters.AddWithValue("@cID", model.cID);
                        updateBalanceCommand.ExecuteNonQuery();
                    }
                }
            }
        }


        // Retrieving Payments by ID for updating
        private payVM GetPaymentByID(int payID)
        {
            using (SqlConnection connection = new SqlConnection(SecConn))
            {
                connection.Open();
                string query = "SELECT * FROM PaymentTbl WHERE payID = @payID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@payID", payID);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            return new payVM
                            {
                                payID = Convert.ToInt32(reader["payID"]),
                                cID = Convert.ToInt32(reader["cID"]),
                                invoiceID = Convert.ToInt32(reader["invoiceID"]),                       
                                paidAmount = Convert.ToDecimal(reader["paidAmount"]),
                                totalFee = Convert.ToDecimal(reader["totalFee"]),
                                payMethod = Convert.ToString(reader["payMethod"]),
                                payDate = Convert.ToDateTime(reader["payDate"])
                                
                            };
                        }
                    }
                }
            }

            return null;
        }

        // Update Payment Logic
        private void UpdatePaymentMethod(payWrapper model)
        {
            using (SqlConnection connection = new SqlConnection(SecConn))
            {
                connection.Open();
                string query = "Update PaymentTbl SET cID = @cID, invoiceID = @invoiceID, paidAmount = @paidAmount,"
                             + "totalFee = @totalFee, payMethod = @payMethod, payDate = @payDate WHERE payID = @payID";
                             


                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@payID", model.payID);
                    command.Parameters.AddWithValue("@cID", model.cID);
                    command.Parameters.AddWithValue("@invoiceID", model.invoiceID);
                    command.Parameters.AddWithValue("@paidAmount", model.paidAmount);
                    command.Parameters.AddWithValue("@totalFee", model.totalFee);
                    command.Parameters.AddWithValue("@payMethod", model.payMethod);
                    command.Parameters.AddWithValue("@payDate", SqlDbType.DateTime2).Value = model.payDate;


                    command.ExecuteNonQuery();

                    // Calculate the difference between paidAmount and totalFee
                    decimal balanceDifference = model.totalFee - model.paidAmount;

                    // Update CustomerTbl with the balance difference
                    string updateBalanceQuery = "UPDATE CustomerTbl SET Balance = @balanceDifference WHERE cID = @cID";
                    using (SqlCommand updateBalanceCommand = new SqlCommand(updateBalanceQuery, connection))
                    {

                        updateBalanceCommand.Parameters.AddWithValue("@balanceDifference", balanceDifference);
                        updateBalanceCommand.Parameters.AddWithValue("@cID", model.cID);
                        updateBalanceCommand.ExecuteNonQuery();
                    }
                }
            }
        }


        // Delete Payment Logic
        private void DeletePayment(int id)
        {
            
            using (SqlConnection connection = new SqlConnection(SecConn))
            {
                connection.Open();
                string query = "DELETE FROM PaymentTbl WHERE payID = @payID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@payID", id);

                    command.ExecuteNonQuery();
                }
            }
        }
    }
}