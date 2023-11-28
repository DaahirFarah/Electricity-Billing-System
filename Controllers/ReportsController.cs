using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EBS.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly string SecConn = ConfigurationManager.ConnectionStrings["SecConn"].ConnectionString;

        // GET: Reports
        public ActionResult Index()
        {
            int toggleState;

            using (SqlConnection conn = new SqlConnection(SecConn))
            {
                conn.Open();

                string sql = "SELECT UseFixedFee FROM SpecialRates";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    toggleState = (int)cmd.ExecuteScalar();
                }

            }

            int intState = toggleState;

            ViewBag.ToggleState = intState;

            return View();
        }

        // Price method changing is handled by this action
        public JsonResult priceChanger(int status)
        {


            using (SqlConnection conn = new SqlConnection(SecConn))
            {
                conn.Open();

                string query = "UPDATE SpecialRates SET UseFixedFee = @status";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@status", status);
                    cmd.ExecuteNonQuery();
                }
            }

            var success = true; // set to false if update fails

            return Json(new
            {
                success = success,
                message = "Pricing method changed successfully!"
            });
        }

    }
}