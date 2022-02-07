<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="AS_Assignment.Login" ValidateRequest = "false"%>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>SITConnect - Login</title>
    <style type="text/css">
        .auto-style1 {
            width: 100%;
        }
    </style>
    <script src="https://www.google.com/recaptcha/api.js?render=6Lfc-lweAAAAAJZ5XaLa0TRGdRfUOQsTGBOyA-hp"></script>
</head>
<body>
    <h1><center>Login</center></h1>

    <form id="form1" runat="server">
        <div>
            <table class="auto-style1">
                <tr>
                    <td>Email</td>
                    <td>
                        <asp:TextBox ID="emailTB" runat="server"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td>Password</td>
                    <td>
                        <asp:TextBox ID="pwdTB" runat="server" TextMode="Password"></asp:TextBox>
                    </td>
                </tr>
            </table>
        </div>
        <asp:Label ID="warningLB" runat="server"></asp:Label>

        <br />
        <br />

        <asp:Button ID="loginBtn" runat="server" Text="Login" OnClick="loginBtn_Click" />
        <asp:Button ID="registerBtn" runat="server" Text="Register" OnClick="registerBtn_Click"/>

        <br />
        <br />

        <input type="hidden" value="" id="recaptcha" name="recaptcha"/>

        <asp:Button ID="error404" runat="server" Text="404" OnClick="error404_Click"/>
        <asp:Button ID="error403" runat="server" Text="403" OnClick="error403_Click"/>
        <asp:Button ID="error500" runat="server" Text="500" OnClick="error500_Click"/>
        <asp:Button ID="errorGeneric" runat="server" Text="Generic" OnClick="errorGeneric_Click"/>
    </form>

    <script>
        grecaptcha.ready(function () {
            grecaptcha.execute('6Lfc-lweAAAAAJZ5XaLa0TRGdRfUOQsTGBOyA-hp', { action: 'submit' }).then(function (token) {
                document.getElementById("recaptcha").value = token;
            });
        });
    </script>

</body>
</html>
