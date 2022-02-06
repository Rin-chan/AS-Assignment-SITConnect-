<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TwoFactor.aspx.cs" Inherits="AS_Assignment.TwoFactor" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>SITConnect - Setting up 2FA</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <table class="auto-style1">
                <tr>
                    <td>Email</td>
                    <td>
                        <asp:TextBox ID="emailTB" runat="server" ReadOnly="True"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td>QR Code Image</td>
                    <td>
                        <asp:Image ID="qrCode" runat="server" />
                    </td>
                </tr>
                <tr>
                    <td>Authentication Code</td>
                    <td>
                        <asp:TextBox ID="authenTB" runat="server" ReadOnly="True"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td>User Input Code</td>
                    <td>
                        <asp:TextBox ID="userTB" runat="server"></asp:TextBox>
                    </td>
                </tr>
            </table>

            <asp:Label ID="warningLB" runat="server"></asp:Label>

            <br />

            <asp:Button ID="submitBtn" runat="server" Text="Submit" OnClick="submitBtn_Click" />
        </div>
    </form>
</body>
</html>
