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
using System.Net;
using System.IO;
using System.Web.Script.Serialization;
using System.Web.Script.Services;

namespace AS_Assignment
{
    public class MyObject
    {
        public string success { get; set; }
        public List<string> ErrorMessage { get; set; }
    }

    public partial class Login : System.Web.UI.Page
    {
        string ASAssignmentDBConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ASAssignmentDBConn"].ConnectionString;
        static string finalHash;
        static string salt;
        byte[] Key;
        byte[] IV;


        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void loginBtn_Click(object sender, EventArgs e)
        {
            var todaydate = DateTime.Now;
            try
            {
                using (SqlConnection con = new SqlConnection(ASAssignmentDBConnectionString))
                {
                    SqlCommand command = new SqlCommand("SELECT Email, Lockout, LastLogin, LockoutTime FROM Account;", con);
                    con.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (reader.GetString(0).Trim() == HttpUtility.HtmlEncode(emailTB.Text.Trim()))
                            {
                                string pwd = HttpUtility.HtmlEncode(pwdTB.Text.ToString().Trim());
                                string email = HttpUtility.HtmlEncode(emailTB.Text.ToString().Trim());
                                SHA512Managed hashing = new SHA512Managed();
                                string dbHash = getDBHash(email);
                                string dbSalt = getDBSalt(email);

                                if (dbSalt != null && dbSalt.Length > 0 && dbHash != null && dbHash.Length > 0)
                                {
                                    string pwdWithSalt = pwd + dbSalt;
                                    byte[] hashWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(pwdWithSalt));
                                    string userHash = Convert.ToBase64String(hashWithSalt);
                                    if (userHash.Equals(dbHash))
                                    {
                                        var previousLogin = reader.GetDateTime(2).ToString();

                                        if (reader.GetInt32(1) >= 3)
                                        {
                                            var lockouttime = reader.GetDateTime(3);

                                            if (todaydate.Subtract(lockouttime).TotalMinutes >= 2)
                                            {
                                                reader.Close();
                                                con.Close();

                                                SqlCommand newupdate = new SqlCommand("UPDATE Account SET Lockout=@Lockout WHERE Email=@Email", con);
                                                newupdate.Parameters.AddWithValue("@Email", email);
                                                newupdate.Parameters.AddWithValue("@Lockout", 0);
                                                newupdate.Connection = con;
                                                try
                                                {
                                                    con.Open();
                                                    newupdate.ExecuteNonQuery();
                                                }
                                                catch (Exception ex)
                                                {
                                                    throw new Exception(ex.ToString());
                                                }
                                                finally
                                                {
                                                    con.Close();
                                                }
                                            }
                                            else
                                            {
                                                var lastLogin = "This account has been locked out. Last login attempt: " + reader.GetDateTime(2).ToString();
                                                warningLB.Text = lastLogin;
                                                warningLB.ForeColor = Color.Red;

                                                reader.Close();
                                                con.Close();

                                                SqlCommand newupdate = new SqlCommand("UPDATE Account SET LastLogin=@LastLogin WHERE Email=@Email", con);
                                                newupdate.Parameters.AddWithValue("@Email", email);
                                                newupdate.Parameters.AddWithValue("@LastLogin", todaydate);
                                                newupdate.Connection = con;
                                                try
                                                {
                                                    con.Open();
                                                    newupdate.ExecuteNonQuery();
                                                }
                                                catch (Exception ex)
                                                {
                                                    throw new Exception(ex.ToString());
                                                }
                                                finally
                                                {
                                                    con.Close();
                                                }

                                                return;
                                            }

                                        }
                                        else
                                        {
                                            reader.Close();
                                            con.Close();

                                            SqlCommand newupdate = new SqlCommand("UPDATE Account SET Lockout=@Lockout WHERE Email=@Email", con);
                                            newupdate.Parameters.AddWithValue("@Email", email);
                                            newupdate.Parameters.AddWithValue("@Lockout", 0);
                                            newupdate.Connection = con;
                                            try
                                            {
                                                con.Open();
                                                newupdate.ExecuteNonQuery();
                                            }
                                            catch (Exception ex)
                                            {
                                                throw new Exception(ex.ToString());
                                            }
                                            finally
                                            {
                                                con.Close();
                                            }
                                        }

                                        SqlCommand updatesql = new SqlCommand("UPDATE Account SET LastLogin=@LastLogin WHERE Email=@Email", con);
                                        updatesql.Parameters.AddWithValue("@Email", email);
                                        updatesql.Parameters.AddWithValue("@LastLogin", todaydate);
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

                                        if (ValidateCaptcha())
                                        {
                                            SqlCommand qrtime = new SqlCommand("SELECT Email, TwoFactor FROM Account;", con);
                                            con.Open();
                                            using (SqlDataReader qrreader = qrtime.ExecuteReader())
                                            {
                                                while (qrreader.Read())
                                                {
                                                    if (qrreader.GetString(0).Trim() == HttpUtility.HtmlEncode(emailTB.Text.Trim()))
                                                    {
                                                        if (qrreader.GetInt32(1) == 0)
                                                        {
                                                            qrreader.Close();
                                                            con.Close();

                                                            Response.Redirect(String.Format("TwoFactor.aspx?Email={0}", email), false);
                                                            return;
                                                        }
                                                        else
                                                        {
                                                            qrreader.Close();
                                                            con.Close();
                                                            break;
                                                        }
                                                    }
                                                }
                                                qrreader.Close();
                                                con.Close();
                                            }

                                            Response.Redirect(String.Format("Authorize.aspx?Email={0}", email), false);

                                        }
                                        else
                                        {
                                            warningLB.Text = "An issue occured. Please try again later.";
                                            warningLB.ForeColor = Color.Red;
                                        }

                                        return;
                                    }
                                    else
                                    {
                                        var lockout = 0;
                                        if (reader.GetInt32(1) != 0)
                                        {
                                            lockout = reader.GetInt32(1);
                                        }
                                        lockout += 1;

                                        reader.Close();
                                        con.Close();

                                        SqlCommand updatesql = new SqlCommand("UPDATE Account SET Lockout=@Lockout, LastLogin=@LastLogin, LockoutTime=@LockoutTime WHERE Email=@Email", con);
                                        updatesql.Parameters.AddWithValue("@Email", email);
                                        updatesql.Parameters.AddWithValue("@Lockout", lockout);
                                        updatesql.Parameters.AddWithValue("@LastLogin", todaydate);
                                        updatesql.Parameters.AddWithValue("@LockoutTime", todaydate);
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
                                        finally {
                                            con.Close();
                                            warningLB.Text = "Wrong email or password.";
                                            warningLB.ForeColor = Color.Red;
                                        }

                                        return;
                                    }

                                }
                            }
                        }
                        warningLB.Text = "Wrong email or password.";
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

        protected string getDBHash(string email)
        {
            string h = null;
            SqlConnection connection = new SqlConnection(ASAssignmentDBConnectionString);
            string sql = "SELECT Password_Hash FROM Account WHERE Email=@Email";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Email", email);
            try
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        if (reader["Password_Hash"] != null)
                        {
                            if (reader["Password_Hash"] != DBNull.Value)
                            {
                                h = reader["Password_Hash"].ToString();
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally { connection.Close(); }
            return h;
        }

        protected string getDBSalt(string email)
        {
            string s = null;
            SqlConnection connection = new SqlConnection(ASAssignmentDBConnectionString);
            string sql = "SELECT Password_Salt FROM Account WHERE Email=@Email";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Email", email);
            try
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["Password_Salt"] != null)
                        {
                            if (reader["Password_Salt"] != DBNull.Value)
                            {
                                s = reader["Password_Salt"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally { connection.Close(); }
            return s;
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
                //ICryptoTransform decryptTransform = cipher.CreateDecryptor();
                byte[] plainText = Encoding.UTF8.GetBytes(data);
                cipherText = encryptTransform.TransformFinalBlock(plainText, 0,
               plainText.Length);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally { }
            return cipherText;
        }

        public bool ValidateCaptcha()
        {
            bool result = true;

            string captchaResponse = Request.Form["recaptcha"];

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://www.google.com/recaptcha/api/siteverify?secret=6Lfc-lweAAAAAKPro82g45xMVrze8zMMolGJatIN &response=" + captchaResponse);

            try
            {
                using (WebResponse wResponse = req.GetResponse())
                {
                    using (StreamReader readStream = new StreamReader(wResponse.GetResponseStream()))
                    {
                        string jsonResponse = readStream.ReadToEnd();

                        warningLB.Text = jsonResponse.ToString();
                        warningLB.ForeColor = Color.Red;

                        JavaScriptSerializer js = new JavaScriptSerializer();
                        MyObject jsonObject = js.Deserialize<MyObject>(jsonResponse);
                        result = Convert.ToBoolean(jsonObject.success);
                    }
                }
                return result;
            }
            catch (WebException ex)
            {
                throw ex;
            }
        }

        protected void error404_Click(object sender, EventArgs e)
        {
            Response.Redirect("LoginABC.aspx");
        }

        protected void error403_Click(object sender, EventArgs e)
        {
            throw new HttpException(403, "Unauthorized");
        }

        protected void error500_Click(object sender, EventArgs e)
        {
            throw new HttpException(500, "Server error");
        }

        protected void errorGeneric_Click(object sender, EventArgs e)
        {
            throw new HttpException(400, "Bad request");
        }

        protected void registerBtn_Click(object sender, EventArgs e)
        {
            Response.Redirect("Registration.aspx", false);
        }
    }
}
