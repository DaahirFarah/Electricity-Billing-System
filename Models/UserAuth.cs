using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Security;
using BCrypt.Net;

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
                    var storedPasswordHash = (string)command.ExecuteScalar();

                    if (storedPasswordHash != null && BCrypt.Net.BCrypt.Verify(password, storedPasswordHash))
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

        public string HashPassword(string password)
        {
            // Hash a password using BCrypt
            return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt());
        }
    }
}