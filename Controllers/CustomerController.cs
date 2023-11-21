using EBS.viewModels;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Runtime.Remoting.Lifetime;
using System.Web.Mvc;

namespace EBS.Controllers
{
    //[Authorize]
    public class CustomerController : Controller
    {

        private readonly string SecConn = ConfigurationManager.ConnectionStrings["SecConn"].ConnectionString;



        [Authorize]
        // GET: Customer
        public ActionResult Index()
        {
            customerWrapper wrapper = new customerWrapper();
            wrapper.customersList = GetAllCustomers();
            return View(wrapper);
        }

        // Remote Validation For Lock Number. The field is unique so it should be checked before inserting data into the appropriate tables
        [HttpPost]
        public JsonResult CheckLockNumber(string lockNumber)
        {
            bool lockExists = false;

            using (SqlConnection connection = new SqlConnection(SecConn))
            {
                connection.Open();

                // Create a SQL command to check if the lock number exists
                string query = "SELECT COUNT(*) FROM CustomerTbl WHERE lockNumber = @lockNumber";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@lockNumber", lockNumber);

                int count = (int)command.ExecuteScalar();

                lockExists = count > 0;
            }

            return Json(new { lockExists });
        }


        // GET Branches
        [HttpGet]
        public ActionResult GetBranches()
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

            customerWrapper model = new customerWrapper
            {
                SelectedBranch = Branch
            };

            return Json(Branch, JsonRequestBehavior.AllowGet);
        }

        // GET Meter Types
        [HttpGet]
        public ActionResult GetMeterType()
        {
            List<string> Type = new List<string>();
            string query = "SELECT Type FROM MeterType";

            using (SqlConnection connection = new SqlConnection(SecConn))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string type = (string)reader["Type"];
                    Type.Add(type);
                }
            }

            customerWrapper model = new customerWrapper
            {
                SelectedBranch = Type
            };

            return Json(Type, JsonRequestBehavior.AllowGet);
        }

        //SET: Register Customer
        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult Create(customerWrapper model)
        {
            if (ModelState.IsValid)
            {
                InsertCustomer(model);
                return Json(new { success = true, message = "Customer Added Successfully!" });
            }
            return Json(new { success = false, message = "Insertion Failed. Please Try Again!" });
        }


        [HttpPost]
        public JsonResult GetCustomerData(int id)
        {
            using (SqlConnection connection = new SqlConnection(SecConn))
            {

                connection.Open();
                string query = "SELECT * FROM CustomerTbl WHERE cID = @cID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@cID", id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Populate the customer object
                            customerWrapper wrapper = new customerWrapper
                            {
                                cID = Convert.ToInt32(reader["cID"]),
                                cFirstName = reader["cFirstName"].ToString(),
                                cMidName = reader["cMidName"].ToString(),
                                cLastName = reader["cLastName"].ToString(),
                                cAddress = reader["cAddress"].ToString(),
                                cNumber = Convert.ToInt32(reader["cNumber"]),
                                cNumberOp = reader["cNumberOp"].ToString(),
                                Branch = reader["Branch"].ToString(),
                                Balance = Convert.ToDecimal(reader["Balance"]),
                                lockNumber = Convert.ToInt32(reader["lockNumber"]),
                                Type = reader["Type"].ToString()
                            };

                            // Return the Invoice Data as JSON
                            return Json(wrapper, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
            }

            // If no data found, return an empty JSON object
            return Json(new customerWrapper(), JsonRequestBehavior.AllowGet);
        }

        // POST:Update Customer
        [HttpPost]
        public JsonResult UpdateCustomer(customerWrapper model)
        {
            if (ModelState.IsValid)
            {
                UpdateCustomerMethod(model);
                return Json(new { success = true, message = "Customer Info Updated Successfully!" });
            }
            return Json(new { success = false, message = "Customer Update Failed. Try Again!" });
        }

        [HttpPost]
        public JsonResult Delete(int id)
        {
            DeleteCustomer(id);
            return Json(new { success = true, message = "Customer Deleted Successfuly" });
        }


        // This action handles exporting customers data from the database using a library called iTextSharp. 
        // This actionResult allows the user to easily download the list of customers in a pdf format 
        public ActionResult GenerateCustomerList()
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
            dateParagraph.SpacingAfter = 5;
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
            return File(memoryStream.ToArray(), "application/pdf", "Customers List.pdf");
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
        public ActionResult CustomerInfo(int id)
        {
            // Simulate retrieving invoice data from your database based on invoiceId
            var customer = GetCustomerById(id);



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
            Paragraph subtitle = new Paragraph("Customer Information", subtitleFont);
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
            AddInvoiceLine(document, $"Customer ID: {customer.cID.ToString()}", contentFont, lineSpacing);
            AddInvoiceLine(document, $"First Name: {customer.cFirstName}", contentFont, lineSpacing);
            AddInvoiceLine(document, $"Middle Name: {customer.cMidName}", contentFont, lineSpacing);
            AddInvoiceLine(document, $"Last Name: {customer.cLastName}", contentFont, lineSpacing);
            AddInvoiceLine(document, $"Address: {customer.cAddress}", contentFont, lineSpacing);
            AddInvoiceLine(document, $"Phone Number: {customer.cNumber.ToString()}", contentFont, lineSpacing);
            AddInvoiceLine(document, $"Phone Number (Optional): {customer.cNumberOp.ToString()}", contentFont, lineSpacing);
            AddInvoiceLine(document, $"Balance ($): {customer.Balance.ToString()}", contentFont, lineSpacing);


            // Close the document
            document.Close();

            // Set the response content type and headers
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", $"attachment;filename=Customer Info.pdf");

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
                                cNumberOp = reader["cNumberOp"] != DBNull.Value ? reader["cNumberOp"].ToString() : "N/A",
                                Branch = reader["Branch"].ToString(),
                                Balance = Convert.ToDecimal(reader["Balance"]),
                                lockNumber = Convert.ToInt32(reader["lockNumber"]),
                                Type = reader["Type"].ToString(),
                                RegisteredOn = Convert.ToDateTime(reader["RegisteredOn"]),

                            });
                        }
                    }
                }
            }

            return customers;
        }


        // Inserting Customers To the Database
        private void InsertCustomer(customerWrapper model)
        {
           
            using (SqlConnection connection = new SqlConnection(SecConn))
            {
                connection.Open();

                DateTime registerDate = DateTime.Now;
                int isbilled = 0;

                model.RegisteredOn = registerDate;
                model.isBilledThisMonth = isbilled;

                // This variable captures the selected meterID for the customer and then it will be the one to have the value that will go to the db

                string query = "INSERT INTO CustomerTbl (cFirstName, cMidName, cLastName, cAddress, cNumber, cNumberOp, Branch, Type, lockNumber, Balance, RegisteredOn, isBilledThisMonth) VALUES (@cFirstName, @cMidName, @cLastName, @cAddress, @cNumber, @cNumberOp, @Branch, @Type, @lockNumber, 0, @RegisteredOn, @isBilledThisMonth)";
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
                    command.Parameters.AddWithValue("@Branch", model.Branch);
                    command.Parameters.AddWithValue("@Type", model.Type);
                    command.Parameters.AddWithValue("@lockNumber", model.lockNumber);
                    command.Parameters.AddWithValue("@RegisteredOn", model.RegisteredOn);
                    command.Parameters.AddWithValue("isBilledThisMonth", model.isBilledThisMonth);

                    command.ExecuteNonQuery();

                    // Retrive the customer id from customers table using the lock number
                    int lNum = model.lockNumber;

                    string idquery = "SELECT cID FROM CustomerTbl WHERE lockNumber = @lNum";
                   
                        using (SqlCommand commandd = new SqlCommand(idquery, connection))
                        {
                            commandd.Parameters.AddWithValue("@lNum", lNum);

                            using (SqlDataReader reader = commandd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    // Retrieve the cID value from the result set if needed.
                                    int cID = reader.GetInt32(0);
                                model.cID = cID;                                   
                                }
                            }
                        }
                    
                    
                    // Keep your eyes peeled for this part as cID might possibly throw an error
                    string InsertToMeter = "INSERT INTO Meters (cID, lockNumber, Type, Status) VALUES (@cID, @lockNumber, @Type, 'Active')";
                    using (SqlCommand commandStatus = new SqlCommand(InsertToMeter, connection))
                    {
                        commandStatus.Parameters.AddWithValue("@cID", model.cID);
                        commandStatus.Parameters.AddWithValue("@lockNumber", model.lockNumber);
                        commandStatus.Parameters.AddWithValue("@Type", model.Type);
                        commandStatus.ExecuteNonQuery();
                    }

                }
            }
        }


        // This method retrieves the data of a customer based on their ID and then it can be used to update customer data or even delete that customer
        private customerWrapper GetCustomerById(int cID)
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
                            return new customerWrapper
                            {
                                cID = Convert.ToInt32(reader["cID"]),
                                cFirstName = reader["cFirstName"].ToString(),
                                cMidName = reader["cMidName"].ToString(),
                                cLastName = reader["cLastName"].ToString(),
                                cAddress = reader["cAddress"].ToString(),
                                cNumber = Convert.ToInt32(reader["cNumber"]),
                                cNumberOp = reader["cNumberOp"].ToString(),
                                Branch = reader["Branch"].ToString(),
                                Balance = Convert.ToDecimal(reader["Balance"])

                            };
                        }
                    }
                }
            }

            return null;
        }


        // Update Customer Information Logic
        private void UpdateCustomerMethod(customerWrapper model)
        {
            using (SqlConnection connection = new SqlConnection(SecConn))
            {
                connection.Open();

                string query = "UPDATE CustomerTbl SET cFirstName = @cFirstName, cMidName = @cMidName, cLastName = @cLastName, cAddress = @cAddress, cNumber = @cNumber, cNumberOp = @cNumberOp, Branch = @Branch, Type = @Type, lockNumber = @lockNumber WHERE cID = @cID";
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
                    command.Parameters.AddWithValue("@Branch", model.Branch);
                    command.Parameters.AddWithValue("@Type", model.Type);
                    command.Parameters.AddWithValue("@lockNumber", model.lockNumber);

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

                // Delete the customer
                string deleteCustomerQuery = "DELETE FROM CustomerTbl WHERE cID = @cID";
                using (SqlCommand deleteCustomerCommand = new SqlCommand(deleteCustomerQuery, connection))
                {
                    deleteCustomerCommand.Parameters.AddWithValue("@cID", id);
                    deleteCustomerCommand.ExecuteNonQuery();
                }
            }
        }

    }
}