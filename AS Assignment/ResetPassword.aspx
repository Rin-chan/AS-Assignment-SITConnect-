<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ResetPassword.aspx.cs" Inherits="AS_Assignment.ForgotPassword" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>SITConnect - Reset Password</title>
    <script type="text/javascript">
        function newpwdValidate() {
            var str = document.getElementById('<%=newpwdTB.ClientID %>').value;

            if (str.length < 12) {
                document.getElementById("pwdLB").innerHTML = "Password length must be at least 12 characters.";
                document.getElementById("pwdLB").style.color = "Red";
                document.getElementById("changeBtn").disabled = true;
                return ("too_short");
            }
            else if (str.search(/[0-9]/) == -1) {
                document.getElementById("pwdLB").innerHTML = "Password require at least 1 number.";
                document.getElementById("pwdLB").style.color = "Red";
                document.getElementById("changeBtn").disabled = true;
                return ("no_number");
            }
            else if (str.search(/[A-Z]/) == -1) {
                document.getElementById("pwdLB").innerHTML = "Password require at least 1 uppercase.";
                document.getElementById("pwdLB").style.color = "Red";
                document.getElementById("changeBtn").disabled = true;
                return ("no_upper");
            }
            else if (str.search(/[a-z]/) == -1) {
                document.getElementById("pwdLB").innerHTML = "Password require at least 1 lowercase.";
                document.getElementById("pwdLB").style.color = "Red";
                document.getElementById("changeBtn").disabled = true;
                return ("no_lower");
            }
            else if (str.search(/[^a-zA-Z0-9]/) == -1) {
                document.getElementById('pwdLB').innerHTML = "Password require at least 1 special character";
                document.getElementById('pwdLB').style.color = "Red";
                document.getElementById("changeBtn").disabled = true;
                return ("no_special");
            }
            document.getElementById("pwdLB").innerHTML = "Excellent";
            document.getElementById("pwdLB").style.color = "Green";
            document.getElementById("changeBtn").disabled = false;
        }
    </script>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:Label ID="messageLB" runat="server"></asp:Label>
            <table class="auto-style1">
                <tr>
                    <td>Old Password</td>
                    <td>
                        <asp:TextBox ID="oldpwdTB" runat="server" TextMode="Password"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td>New Password</td>
                    <td>
                        <asp:TextBox ID="newpwdTB" runat="server" TextMode="Password" onKeyUp="javascript:newpwdValidate()"></asp:TextBox>
                    </td>
                    <td>
                        <asp:Label ID="pwdLB" runat="server"></asp:Label>
                    </td>
                </tr>
            </table>
        </div>

        <asp:Label ID="warningLB" runat="server"></asp:Label>

        <br />
        <br />

        <asp:Button ID="changeBtn" runat="server" Text="Change Password" OnClick="changeBtn_Click" Enabled="False"/>

    </form>
</body>
</html>
