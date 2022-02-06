<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Homepage.aspx.cs" Inherits="AS_Assignment.Homepage" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>SITConnect - Homepage</title>
</head>
<body>
    <form id="form1" runat="server">
        <h1><center>Welcome to Home</center></h1>
        <div>
            
            <asp:Label ID="messageLB" runat="server" EnableViewState="False"></asp:Label>
            
            <br />
            <asp:Button ID="logoutBtn" runat="server" Text="Logout" EnableViewState="False" OnClick="logoutBtn_Click" />
            <asp:Button ID="changeBtn" runat="server" Text="Change Password" EnableViewState="False" OnClick="changeBtn_Click" />
            <br />


        </div>
    </form>
</body>
</html>
