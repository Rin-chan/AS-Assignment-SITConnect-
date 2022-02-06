<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Authorize.aspx.cs" Inherits="AS_Assignment.Authorize" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>SITConnect - Authorize</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <table class="auto-style1">
                <tr>
                    <td>Security Code</td>
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
