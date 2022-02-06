using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace AS_Assignment
{
    public partial class Registration : System.Web.UI.Page
    {
        string ASAssignmentDBConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ASAssignmentDBConn"].ConnectionString;
        static string finalHash;
        static string salt;
        byte[] Key;
        byte[] IV;

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void submitClick(object sender, EventArgs e)
        {
            string pwd = HttpUtility.HtmlEncode(pwdTB.Text.ToString().Trim());

            int scores = checkPassword(pwd);
            if (scores < 4)
            {
                return;
            }

            //Generate random "salt"
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] saltByte = new byte[8];

            //Fills array of bytes with a cryptographically strong sequence of random values.
            rng.GetBytes(saltByte);
            salt = Convert.ToBase64String(saltByte);
            
            SHA512Managed hashing = new SHA512Managed();
            
            string pwdWithSalt = pwd + salt;
            byte[] plainHash = hashing.ComputeHash(Encoding.UTF8.GetBytes(pwd));
            byte[] hashWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(pwdWithSalt));
            
            finalHash = Convert.ToBase64String(hashWithSalt);
            
            RijndaelManaged cipher = new RijndaelManaged();
            cipher.GenerateKey();
            Key = cipher.Key;
            IV = cipher.IV;
            
            createAccount();
        }

        private int checkPassword(string password)
        {
            int score = 0;

            // Score 0 very weak!
            if (password.Length < 8)
            {
                return 1;
            }
            else
            {
                score++;
            }

            // Score 2 Weak
            if (Regex.IsMatch(password, "[a-z]"))
            {
                score++;
            }

            // Score 3 Medium
            if (Regex.IsMatch(password, "[A-Z]"))
            {
                score++;
            }

            // Score 4 Strong
            if (Regex.IsMatch(password, "[0-9]"))
            {
                score++;
            }

            // Score 5 Excellent
            if (Regex.IsMatch(password, "[^a-zA-Z0-9]"))
            {
                score++;
            }

            return score;
        }

        protected void createAccount()
        {

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
                            if (reader.GetString(0).Trim() == HttpUtility.HtmlEncode(emailTB.Text.ToString().Trim()))
                            {
                                emailLB.Text = "Email is currently in use.";
                                emailLB.ForeColor = Color.Red;
                                return;
                            }
                        }

                    reader.Close();
                    con.Close();
                    }

                    var todayDate = DateTime.Now;
                    using (SqlCommand cmd = new SqlCommand("INSERT INTO Account VALUES(@First_Name, @Last_Name, @Password_Salt, @Email, @Password_Hash, @DOB, @Credit_Card, @IV, @Key, @Photo, @Lockout, @LastLogin, @LockoutTime, @LastPassword_Salt, @LastPassword_Hash, @SecondPassword_Salt, @SecondPassword_Hash, @PasswordChange, @TwoFactor)"))
                {
                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@First_Name", HttpUtility.HtmlEncode(fNameTB.Text.Trim()));
                            cmd.Parameters.AddWithValue("@Last_Name", HttpUtility.HtmlEncode(lNameTB.Text.Trim()));
                            cmd.Parameters.AddWithValue("@Credit_Card", encryptData(HttpUtility.HtmlEncode(creditTB.Text.Trim())));
                            cmd.Parameters.AddWithValue("@Password_Hash", finalHash);
                            cmd.Parameters.AddWithValue("@Password_Salt", salt);
                            cmd.Parameters.AddWithValue("@DOB", HttpUtility.HtmlEncode(birthTB.Text.Trim()));
                            cmd.Parameters.AddWithValue("@Email", HttpUtility.HtmlEncode(emailTB.Text.Trim()));
                            cmd.Parameters.AddWithValue("@IV", Convert.ToBase64String(IV));
                            cmd.Parameters.AddWithValue("@Key", Convert.ToBase64String(Key));
                            cmd.Parameters.AddWithValue("@Photo", photo.FileContent);
                            cmd.Parameters.AddWithValue("@Lockout", 0);
                            cmd.Parameters.AddWithValue("@LastLogin", todayDate);
                            cmd.Parameters.AddWithValue("@LockoutTime", todayDate);
                            cmd.Parameters.AddWithValue("@LastPassword_Salt", "");
                            cmd.Parameters.AddWithValue("@LastPassword_Hash", "");
                            cmd.Parameters.AddWithValue("@SecondPassword_Salt", "");
                            cmd.Parameters.AddWithValue("@SecondPassword_Hash", "");
                            cmd.Parameters.AddWithValue("@PasswordChange", todayDate);
                            cmd.Parameters.AddWithValue("@TwoFactor", 0);
                            cmd.Connection = con;
                            con.Open();
                            cmd.ExecuteNonQuery();
                            con.Close();
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            var email = HttpUtility.HtmlEncode(emailTB.Text.Trim());
            Response.Redirect(String.Format("TwoFactor.aspx?Email={0}", email), true);
        }

        protected byte[] encryptData(string data)
        {
            byte[] cipherText = null;
            try
            {
                RijndaelManaged cipher = new RijndaelManaged();
                cipher.IV = IV;
                cipher.Key = Key;
                ICryptoTransform encryptTransform = cipher.CreateEncryptor();
                byte[] plainText = Encoding.UTF8.GetBytes(data);
                cipherText = encryptTransform.TransformFinalBlock(plainText, 0, plainText.Length);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally { }
            return cipherText;
        }

    }
}