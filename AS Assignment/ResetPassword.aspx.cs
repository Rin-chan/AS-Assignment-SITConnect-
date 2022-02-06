using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AS_Assignment
{
    public partial class ForgotPassword : System.Web.UI.Page
    {
        string ASAssignmentDBConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ASAssignmentDBConn"].ConnectionString;
        static string finalHash;
        static string salt;
        byte[] Key;
        byte[] IV;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["LoggedIn"] != null && Session["AuthToken"] != null && Request.Cookies["AuthToken"] != null)
            {
                if (!Session["AuthToken"].ToString().Equals(Request.Cookies["AuthToken"].Value))
                {
                    Response.Redirect("Login.aspx", false);
                }
                else
                {
                    if (Request.QueryString["Message"] != null)
                    {
                        messageLB.Text = "<h1>Your password has expired. Please change your password.</h1>";
                    }
                }
            }
            else
            {
                Response.Redirect("Login.aspx", false);
            }
        }

        protected void changeBtn_Click(object sender, EventArgs e)
        {
            string newpwd = HttpUtility.HtmlEncode(newpwdTB.Text.ToString().Trim());

            int scores = checkPassword(newpwd);
            if (scores < 4)
            {
                warningLB.Text = "Password does not meet minimum requirement.";
                warningLB.ForeColor = Color.Red;
                return;
            }

            changePassword();
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

        protected void changePassword()
        {

            try
            {
                using (SqlConnection con = new SqlConnection(ASAssignmentDBConnectionString))
                {
                    SqlCommand command = new SqlCommand("SELECT Email, Password_Salt, Password_Hash, LastPassword_Salt, LastPassword_Hash, PasswordChange FROM Account;", con);
                    con.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (reader.GetString(0).Trim() == Session["LoggedIn"].ToString())
                            {
                                string oldpwd = HttpUtility.HtmlEncode(oldpwdTB.Text.ToString().Trim());
                                string newpwd = HttpUtility.HtmlEncode(newpwdTB.Text.ToString().Trim());
                                SHA512Managed hashing = new SHA512Managed();

                                string checkdbHash = getDBHash(0);
                                string checkdbSalt = getDBSalt(0);
                                string checkpwdWithSalt = oldpwd + checkdbSalt;
                                byte[] checkhashWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(checkpwdWithSalt));
                                string checkuserHash = Convert.ToBase64String(checkhashWithSalt);
                                if (checkuserHash.Equals(checkdbHash))
                                {
                                    bool matching = matchDB(newpwd);
                                    var todayDate = DateTime.Now;

                                    if (matching)
                                    {
                                        warningLB.Text = "Password cannot be reused for 2 generation.";
                                        warningLB.ForeColor = Color.Red;

                                        reader.Close();
                                        con.Close();
                                        return;
                                    }
                                    
                                    if (todayDate.Subtract(reader.GetDateTime(5)).TotalMinutes <= 2)
                                    {
                                        warningLB.Text = "Password cannot be changed for " + Math.Round((2 - todayDate.Subtract(reader.GetDateTime(5)).TotalMinutes), 1) + " minutes.";
                                        warningLB.ForeColor = Color.Red;

                                        reader.Close();
                                        con.Close();
                                        return;
                                    }

                                    //Generate random "salt"
                                    RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
                                    byte[] saltByte = new byte[8];

                                    //Fills array of bytes with a cryptographically strong sequence of random values.
                                    rng.GetBytes(saltByte);
                                    salt = Convert.ToBase64String(saltByte);

                                    string pwdWithSalt = newpwd + salt;
                                    byte[] plainHash = hashing.ComputeHash(Encoding.UTF8.GetBytes(newpwd));
                                    byte[] hashWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(pwdWithSalt));

                                    finalHash = Convert.ToBase64String(hashWithSalt);

                                    string oldsalt = reader.GetString(1).Trim();
                                    string oldhash = reader.GetString(2).Trim();
                                    string lastsalt = reader.GetString(3).Trim();
                                    string lasthash = reader.GetString(4).Trim();

                                    reader.Close();
                                    con.Close();
                                    SqlCommand newupdate = new SqlCommand("UPDATE Account SET Password_Salt=@Password_Salt, Password_Hash=@Password_Hash, LastPassword_Salt=@LastPassword_Salt, LastPassword_Hash=@LastPassword_Hash, SecondPassword_Salt=@SecondPassword_Salt, SecondPassword_Hash=@SecondPassword_Hash, PasswordChange=@PasswordChange WHERE Email=@Email", con);
                                    newupdate.Parameters.AddWithValue("@Email", Session["LoggedIn"].ToString());
                                    newupdate.Parameters.AddWithValue("@Password_Salt", salt);
                                    newupdate.Parameters.AddWithValue("@Password_Hash", finalHash);
                                    newupdate.Parameters.AddWithValue("@LastPassword_Salt", oldsalt);
                                    newupdate.Parameters.AddWithValue("@LastPassword_Hash", oldhash);
                                    newupdate.Parameters.AddWithValue("@SecondPassword_Salt", lastsalt);
                                    newupdate.Parameters.AddWithValue("@SecondPassword_Hash", lasthash);
                                    newupdate.Parameters.AddWithValue("@PasswordChange", todayDate);
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

                                    Response.Redirect("Homepage.aspx", false);
                                    return;
                                }
                                warningLB.Text = "Old Password is incorrect.";
                                warningLB.ForeColor = Color.Red;

                                reader.Close();
                                con.Close();
                                return;
                            }
                        }

                        warningLB.Text = "An error has occured. Please try again later.";
                        warningLB.ForeColor = Color.Red;

                        reader.Close();
                        con.Close();
                        return;
                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            Response.Redirect("Homepage.aspx", false);
        }

        protected string getDBHash(int type)
        {
            // Type meanings for reference
            // 0 = Currently used password (Password_Hash, Password_Salt)
            // 1 = 2nd most recent password (LastPassword_Hash, LastPassword_Salt)
            // 2 = 3rd recent password (SecondPassword_Hash, SecondPassword_Salt)

            string email = Session["LoggedIn"].ToString();
            SqlConnection connection = new SqlConnection(ASAssignmentDBConnectionString);
            string h = null;

            if (type == 0)
            {
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
            }
            else if (type == 1)
            {
                string sql = "SELECT LastPassword_Hash FROM Account WHERE Email=@Email";
                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@Email", email);
                try
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            if (reader["LastPassword_Hash"] != null)
                            {
                                if (reader["LastPassword_Hash"] != DBNull.Value)
                                {
                                    h = reader["LastPassword_Hash"].ToString();
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
            }
            else if (type == 2)
            {
                string sql = "SELECT SecondPassword_Hash FROM Account WHERE Email=@Email";
                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@Email", email);
                try
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            if (reader["SecondPassword_Hash"] != null)
                            {
                                if (reader["SecondPassword_Hash"] != DBNull.Value)
                                {
                                    h = reader["SecondPassword_Hash"].ToString();
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
            }
         
            return h;
        }

        protected string getDBSalt(int type)
        {
            // Type meanings for reference
            // 0 = Currently used password (Password_Hash, Password_Salt)
            // 1 = 2nd most recent password (LastPassword_Hash, LastPassword_Salt)
            // 2 = 3rd recent password (SecondPassword_Hash, SecondPassword_Salt)

            string email = Session["LoggedIn"].ToString();
            string s = null;
            SqlConnection connection = new SqlConnection(ASAssignmentDBConnectionString);

            if (type == 0)
            {
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
            }
            else if (type == 1)
            {
                string sql = "SELECT LastPassword_Salt FROM Account WHERE Email=@Email";
                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@Email", email);
                try
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (reader["LastPassword_Salt"] != null)
                            {
                                if (reader["LastPassword_Salt"] != DBNull.Value)
                                {
                                    s = reader["LastPassword_Salt"].ToString();
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
            }
            else if (type == 2)
            {
                string sql = "SELECT SecondPassword_Salt FROM Account WHERE Email=@Email";
                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@Email", email);
                try
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (reader["SecondPassword_Salt"] != null)
                            {
                                if (reader["SecondPassword_Salt"] != DBNull.Value)
                                {
                                    s = reader["SecondPassword_Salt"].ToString();
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
            }

            return s;
        }

        protected Boolean matchDB(string newpwd)
        {
            bool matching = false;

            string olddbHash = getDBHash(0);
            string olddbSalt = getDBSalt(0);
            string lastdbHash = getDBHash(1);
            string lastdbSalt = getDBSalt(1);
            string seconddbHash = getDBHash(2);
            string seconddbSalt = getDBSalt(2);

            SHA512Managed hashing = new SHA512Managed();

            string oldpwdWithSalt = newpwd + olddbSalt;
            byte[] oldhashWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(oldpwdWithSalt));
            string olduserHash = Convert.ToBase64String(oldhashWithSalt);
            if (olduserHash.Equals(olddbHash))
            {
                matching = true;
            }

            string lastpwdWithSalt = newpwd + lastdbSalt;
            byte[] lasthashWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(lastpwdWithSalt));
            string lastuserHash = Convert.ToBase64String(lasthashWithSalt);
            if (lastuserHash.Equals(lastdbHash))
            {
                matching = true;
            }

            string secondpwdWithSalt = newpwd + seconddbSalt;
            byte[] secondhashWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(secondpwdWithSalt));
            string seconduserHash = Convert.ToBase64String(secondhashWithSalt);
            if (seconduserHash.Equals(seconddbHash))
            {
                matching = true;
            }

            return matching;
        }
    }
}