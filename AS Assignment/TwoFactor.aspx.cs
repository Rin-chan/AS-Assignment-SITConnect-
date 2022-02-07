using Google.Authenticator;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AS_Assignment
{
    public partial class TwoFactor : System.Web.UI.Page
    {
        string ASAssignmentDBConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ASAssignmentDBConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            string email = Request.QueryString["Email"];

            if (email == null)
            {
                Response.Redirect("Registration.aspx", true);
            }

            TwoFactorAuthenticator twoFactor = new TwoFactorAuthenticator();
            var setupInfo = twoFactor.GenerateSetupCode("SITConnect", email, TwoFactorKey(email), false, 3);
            authenTB.Text = setupInfo.ManualEntryKey;
            qrCode.ImageUrl = setupInfo.QrCodeSetupImageUrl;
            emailTB.Text = email;
        }

        private static string TwoFactorKey(string email)
        {
            //Removed to prevent leaking secret key
            return $"";
        }

        protected void submitBtn_Click(object sender, EventArgs e)
        {
            string email = Request.QueryString["Email"];
            string inputCode = HttpUtility.HtmlEncode(userTB.Text.Trim());

            TwoFactorAuthenticator twoFactor = new TwoFactorAuthenticator();
            bool isValid = twoFactor.ValidateTwoFactorPIN(TwoFactorKey(email), inputCode);
            if (!isValid)
            {
                warningLB.Text = "Incorrect code";
                warningLB.ForeColor = Color.Red;
                return;
            }

            try
            {
                using (SqlConnection con = new SqlConnection(ASAssignmentDBConnectionString))
                {
                    SqlCommand command = new SqlCommand("SELECT Email FROM Account;", con);
                    con.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (reader.GetString(0).Trim() == email)
                            {
                                reader.Close();
                                con.Close();

                                SqlCommand updatesql = new SqlCommand("UPDATE Account SET TwoFactor=@TwoFactor WHERE Email=@Email", con);
                                updatesql.Parameters.AddWithValue("@Email", email);
                                updatesql.Parameters.AddWithValue("@TwoFactor", 1);
                                updatesql.Connection = con;
                                try
                                {
                                    con.Open();
                                    updatesql.ExecuteNonQuery();
                                }
                                catch (Exception ex)
                                {
                                    throw new Exception(ex.ToString());
                                }
                                finally
                                {
                                    con.Close();
                                }

                                Response.Redirect("Login.aspx", false);
                                return;
                            }
                        }

                        warningLB.Text = "An error occured. Please try again.";
                        warningLB.ForeColor = Color.Red;

                        reader.Close();
                        con.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

        }
    }
}
