<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Registration.aspx.cs" Inherits="AS_Assignment.Registration" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>SITConnect - Registration</title>
    <script type="text/javascript">
        function pwdValidate() {
            var str = document.getElementById('<%=pwdTB.ClientID %>').value;

            if (str.length < 12) {
                document.getElementById("pwdLB").innerHTML = "Password length must be at least 12 characters.";
                document.getElementById("pwdLB").style.color = "Red";
                document.getElementById("submitBtn").disabled = true;
                return ("too_short");
            }
            else if (str.search(/[0-9]/) == -1) {
                document.getElementById("pwdLB").innerHTML = "Password require at least 1 number.";
                document.getElementById("pwdLB").style.color = "Red";
                document.getElementById("submitBtn").disabled = true;
                return ("no_number");
            }
            else if (str.search(/[A-Z]/) == -1) {
                document.getElementById("pwdLB").innerHTML = "Password require at least 1 uppercase.";
                document.getElementById("pwdLB").style.color = "Red";
                document.getElementById("submitBtn").disabled = true;
                return ("no_upper");
            }
            else if (str.search(/[a-z]/) == -1) {
                document.getElementById("pwdLB").innerHTML = "Password require at least 1 lowercase.";
                document.getElementById("pwdLB").style.color = "Red";
                document.getElementById("submitBtn").disabled = true;
                return ("no_lower");
            }
            else if (str.search(/[^a-zA-Z0-9]/) == -1) {
                document.getElementById('pwdLB').innerHTML = "Password require at least 1 special character";
                document.getElementById('pwdLB').style.color = "Red";
                document.getElementById("submitBtn").disabled = true;
                return ("no_special");
            }
            document.getElementById("pwdLB").innerHTML = "Excellent";
            document.getElementById("pwdLB").style.color = "Green";
            document.getElementById("submitBtn").disabled = false;
        }
    </script>
</head>

<body>
    <h1><center>Registration</center></h1>
    <form id="form1" runat="server">
        <div>
            <table class="auto-style1">
                <tr>
                    <td>First Name</td>
                    <td>
                        <asp:TextBox ID="fNameTB" runat="server"></asp:TextBox>
                    </td>
                    <td>
                        <asp:Label ID="fNameLB" runat="server"></asp:Label>
                    </td>
                </tr>
                <tr>
                    <td>Last Name</td>
                    <td>
                        <asp:TextBox ID="lNameTB" runat="server"></asp:TextBox>
                    </td>
                    <td>
                        <asp:Label ID="lNameLB" runat="server"></asp:Label>
                    </td>
                </tr>
                <tr>
                    <td>Credit Card Info</td>
                    <td>
                        <asp:TextBox ID="creditTB" runat="server"></asp:TextBox>
                    </td>
                    <td>
                        <asp:Label ID="creditLB" runat="server"></asp:Label>
                    </td>
                </tr>
                <tr>
                    <td>Email Address</td>
                    <td>
                        <asp:TextBox ID="emailTB" runat="server" TextMode="Email"></asp:TextBox>
                    </td>
                    <td>
                        <asp:Label ID="emailLB" runat="server"></asp:Label>
                    </td>
                </tr>
                <tr>
                    <td>Password</td>
                    <td>
                        <asp:TextBox ID="pwdTB" runat="server" TextMode="Password" onKeyUp="javascript:pwdValidate()"></asp:TextBox>
                    </td>
                    <td>
                        <asp:Label ID="pwdLB" runat="server"></asp:Label>
                    </td>
                </tr>
                <tr>
                    <td>Date of Birth</td>
                    <td>
                        <asp:TextBox ID="birthTB" runat="server" TextMode="Date"></asp:TextBox>
                    </td>
                    <td>
                        <asp:Label ID="birthLB" runat="server"></asp:Label>
                    </td>
                </tr>
                <tr>
                    <td>Photo</td>
                    <td>
                        <asp:FileUpload ID="photo" runat="server" accept=".jpg,.png"/>
                    </td>
                    <td>
                        <asp:Label ID="photoLB" runat="server"></asp:Label>
                    </td>
                </tr>
            </table>
        </div>
        <p>
            <asp:Button ID="submitBtn" runat="server" OnClick="submitClick" Text="Submit" disabled="true"/>
        </p>
    </form>
</body>
</html>
