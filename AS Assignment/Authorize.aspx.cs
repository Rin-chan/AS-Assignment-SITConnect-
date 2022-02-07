using Google.Authenticator;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AS_Assignment
{
    public partial class Authorize : System.Web.UI.Page
    {
        string ASAssignmentDBConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ASAssignmentDBConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            string email = Request.QueryString["Email"];

            if (email == null)
            {
                Response.Redirect("Registration.aspx", true);
            }

        }

        private static string TwoFactorKey(string email)
        {
            // Removed to prevent leaking secret key
            return $"secretkey+{email}";
        }

        protected void submitBtn_Click(object sender, EventArgs e)
        {
            string email = Request.QueryString["Email"];
            string inputCode = userTB.Text.Trim();

            TwoFactorAuthenticator twoFactor = new TwoFactorAuthenticator();
            bool isValid = twoFactor.ValidateTwoFactorPIN(TwoFactorKey(email), inputCode);
            if (!isValid)
            {
                warningLB.Text = "Incorrect code";
                warningLB.ForeColor = Color.Red;
                return;
            }

            Session["LoggedIn"] = email;

            string guid = Guid.NewGuid().ToString();
            Session["AuthToken"] = guid;

            Response.Cookies.Add(new HttpCookie("AuthToken", guid));


            using (SqlConnection con = new SqlConnection(ASAssignmentDBConnectionString))
            {
                SqlCommand passtime = new SqlCommand("SELECT Email, PasswordChange, LastLogin FROM Account;", con);
                con.Open();
                using (SqlDataReader timereader = passtime.ExecuteReader())
                {
                    while (timereader.Read())
                    {
                        if (timereader.GetString(0).Trim() == email)
                        {
                            if ((DateTime.Now).Subtract(timereader.GetDateTime(1)).TotalMinutes >= 5)
                            {
                                timereader.Close();
                                con.Close();

                                bool message = true;
                                Response.Redirect(String.Format("ResetPassword.aspx?Message={0}", message), false);
                                return;
                            }
                            else
                            {
                                Response.Redirect(String.Format("Homepage.aspx?Date={0}", timereader.GetDateTime(2)), false);
                            }
                        }
                    }
                    timereader.Close();
                    con.Close();
                }
            }

            warningLB.Text = "Something went wrong, please try again.";
            warningLB.ForeColor = Color.Red;
            return;
        }
    }
}
