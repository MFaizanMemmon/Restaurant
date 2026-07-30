# Microsoft Access setup

1. Create a blank Access database named `RM.accdb`.
2. In Access press `Alt+F11`, choose **File > Import File**, and import
   `AccessDatabaseSetup.bas`.
3. Put the cursor inside `SetupRestaurantDatabase` and press `F5`.
4. Close Access and copy `RM.accdb` into the project root beside
   `Restaurant.csproj`.
5. Build and run the project. Visual Studio copies `RM.accdb` to the output
   folder automatically.

The initial login for a new database is `admin` / `admin`. Change it after the
first login. If you import existing tables/data, the setup routine preserves
them and only creates missing tables and the `usp_GetBill` saved query.

The setup also creates the staff roles required by POS:

- `Waiter` for dine-in waiter selection
- `Driver` for delivery order assignment

The application requires Microsoft Access Database Engine 2016 (ACE OLE DB
16.0) with the same 32/64-bit architecture as the compiled application.
