Attribute VB_Name = "AccessDatabaseSetup"
Option Compare Database
Option Explicit

' Import this module into a blank RM.accdb and run SetupRestaurantDatabase.
' Existing tables are left untouched, so it is also safe to run after importing data.
Public Sub SetupRestaurantDatabase()
    EnsureTable "Users", "CREATE TABLE Users (UserID COUNTER CONSTRAINT PK_Users PRIMARY KEY, UName TEXT(100), UPass TEXT(255), UserName TEXT(150), RoleId LONG)"
    EnsureTable "TblRole", "CREATE TABLE TblRole (RoleID COUNTER CONSTRAINT PK_TblRole PRIMARY KEY, RoleName TEXT(100))"
    EnsureTable "TblRoleAuther", "CREATE TABLE TblRoleAuther (RoleAutherID COUNTER CONSTRAINT PK_TblRoleAuther PRIMARY KEY, RoleID LONG, [Access] TEXT(100))"
    EnsureTable "Staff", "CREATE TABLE Staff (StaffID COUNTER CONSTRAINT PK_Staff PRIMARY KEY, StaffName TEXT(150), StaffPhone TEXT(50), RoleID LONG, StaffRole TEXT(100))"
    EnsureTable "Category", "CREATE TABLE Category (CategoryID COUNTER CONSTRAINT PK_Category PRIMARY KEY, CategoryName TEXT(150))"
    EnsureTable "Product", "CREATE TABLE Product (ProductID COUNTER CONSTRAINT PK_Product PRIMARY KEY, ProductName TEXT(150), ProductPrice CURRENCY, CategoryID LONG, ProductImage LONGBINARY)"
    EnsureTable "Tables", "CREATE TABLE Tables (Tid COUNTER CONSTRAINT PK_Tables PRIMARY KEY, TName TEXT(100))"
    EnsureTable "TblVender", "CREATE TABLE TblVender (VenderID COUNTER CONSTRAINT PK_TblVender PRIMARY KEY, [Name] TEXT(150), PhoneNO TEXT(50), [Address] LONGTEXT, OpeningBalance CURRENCY, [Description] LONGTEXT)"
    EnsureTable "TblExpenseHead", "CREATE TABLE TblExpenseHead (ExpenseID COUNTER CONSTRAINT PK_TblExpenseHead PRIMARY KEY, ExpenseHead TEXT(150))"
    EnsureTable "TblExpence", "CREATE TABLE TblExpence (ExpID COUNTER CONSTRAINT PK_TblExpence PRIMARY KEY, ExpDate DATETIME, ExpHead TEXT(150), PaymentType TEXT(100), Amount CURRENCY, Notes LONGTEXT, createdBy TEXT(100), ModifyBy TEXT(100))"
    EnsureTable "TblCashIn", "CREATE TABLE TblCashIn (CashID COUNTER CONSTRAINT PK_TblCashIn PRIMARY KEY, [DateTime] DATETIME, CashMode TEXT(100), Amount CURRENCY, Notes LONGTEXT, createBy TEXT(100), ModifyBy TEXT(100))"
    EnsureTable "TblMain", MainTableSql("PK_TblMain")
    EnsureTable "TblDetail", "CREATE TABLE TblDetail (DetailID COUNTER CONSTRAINT PK_TblDetail PRIMARY KEY, MainID LONG, ProID LONG, Qty LONG, Price CURRENCY, Amount CURRENCY)"
    EnsureTable "TblMainReturn", MainTableSql("PK_TblMainReturn", True)
    EnsureTable "TblDetailReturn", "CREATE TABLE TblDetailReturn (DetailID COUNTER CONSTRAINT PK_TblDetailReturn PRIMARY KEY, MainID LONG, ProID LONG, Qty LONG, Price CURRENCY, Amount CURRENCY)"
    EnsureTable "tblOrderLog", "CREATE TABLE tblOrderLog (LogID COUNTER CONSTRAINT PK_tblOrderLog PRIMARY KEY, mainid LONG, itemid LONG, qty LONG, ordercount LONG, isdeleted YESNO)"

    EnsureIndex "UX_Users_UName", "Users", "UName", True
    EnsureIndex "IX_Detail_MainID", "TblDetail", "MainID", False
    EnsureIndex "IX_OrderLog_MainID", "tblOrderLog", "mainid", False
    EnsureBillQuery
    SeedDefaults
    MsgBox "Restaurant database setup completed.", vbInformation
End Sub

Private Function MainTableSql(ByVal pkName As String, Optional ByVal isReturn As Boolean = False) As String
    Dim extra As String
    If isReturn Then extra = ", InvoiceID LONG, Reson LONGTEXT"
    MainTableSql = "CREATE TABLE " & IIf(isReturn, "TblMainReturn", "TblMain") & _
        " (MainID COUNTER CONSTRAINT " & pkName & " PRIMARY KEY, [Date] DATETIME, [Time] TEXT(30)," & _
        " TableName TEXT(100), WaiterName TEXT(150), [Status] TEXT(50), OrderType TEXT(50)," & _
        " Total CURRENCY, Recieved CURRENCY, [Change] CURRENCY, DriverID LONG, CustName TEXT(150)," & _
        " CustPhone TEXT(50), PaidDateTime DATETIME, IsPrint YESNO, IsPrintUnPaid YESNO," & _
        " IsOrderPrint YESNO" & extra & ")"
End Function

Private Sub EnsureBillQuery()
    On Error Resume Next
    CurrentDb.QueryDefs.Delete "usp_GetBill"
    On Error GoTo 0
    Dim sqlText As String
    sqlText = "PARAMETERS [@InvoiceId] Long;" & _
        " SELECT CStr(m.[Date]) AS [Date], m.[Time], m.OrderType, m.CustName, m.TableName," & _
        " m.WaiterName, p.ProductName, CStr(d.Price) AS Price, CStr(d.Qty) AS Qty," & _
        " CStr(d.Amount) AS Amount, CStr(m.Total) AS Total, CStr(m.Recieved) AS Received," & _
        " CStr(m.[Change]) AS Changed, CStr(m.MainID) AS InvoiceNo, m.CustPhone" & _
        " FROM (TblMain AS m INNER JOIN TblDetail AS d ON m.MainID=d.MainID)" & _
        " INNER JOIN Product AS p ON d.ProID=p.ProductID WHERE m.MainID=[@InvoiceId];"
    CurrentDb.CreateQueryDef "usp_GetBill", sqlText
End Sub

Private Sub EnsureTable(ByVal tableName As String, ByVal ddl As String)
    If Not TableExists(tableName) Then CurrentDb.Execute ddl, dbFailOnError
End Sub

Private Function TableExists(ByVal tableName As String) As Boolean
    Dim td As DAO.TableDef
    On Error Resume Next
    Set td = CurrentDb.TableDefs(tableName)
    TableExists = (Err.Number = 0)
    Err.Clear
End Function

Private Sub EnsureIndex(ByVal indexName As String, ByVal tableName As String, ByVal fieldName As String, ByVal uniqueValue As Boolean)
    On Error Resume Next
    CurrentDb.Execute "CREATE " & IIf(uniqueValue, "UNIQUE ", "") & "INDEX " & indexName & _
                      " ON " & tableName & " (" & fieldName & ")", dbFailOnError
    Err.Clear
End Sub

Private Sub SeedDefaults()
    If DCount("*", "TblRole") = 0 Then CurrentDb.Execute "INSERT INTO TblRole (RoleName) VALUES ('Administrator')"
    If DCount("*", "Users") = 0 Then
        CurrentDb.Execute "INSERT INTO Users (UName, UPass, UserName, RoleId) VALUES ('admin','admin','Administrator',1)"
    End If
End Sub

