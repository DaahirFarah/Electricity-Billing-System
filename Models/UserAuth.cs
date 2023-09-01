using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace EBS.Models
{
    public class UserAuth
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["SecConn"].ConnectionString;

        public bool AuthenticateUser(string username, string password)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("SELECT Password FROM Users WHERE Username = @Username", connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    var storedPassword = (string)command.ExecuteScalar();

                    if (storedPassword != null && storedPassword == password)
                    {
                        FormsAuthentication.SetAuthCookie(username, false);
                        return true;
                    }
                }
            }
            return false;
        }

        public void SignOut()
        {
            FormsAuthentication.SignOut();
        }
    }
}