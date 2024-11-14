using CrystalDecisions.CrystalReports.Engine;
using Restaurant.View;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace Restaurant.Model
{
    public partial class FormPOS : Form
    {
        private System.Timers.Timer _searchTimer;
        private const int DebounceDelay = 300; // Delay in milliseconds
        public FormPOS()
        {
            InitializeComponent();
            _searchTimer = new System.Timers.Timer(DebounceDelay);
            _searchTimer.AutoReset = false; // Ensures the timer runs only once per delay
            _searchTimer.Elapsed += async (s, e) => await PerformSearchAsync(txtSearch.Text.Trim().ToLower());
        }
        public int MainID = 0;
        public string OrderType = "";
        public int DriverID = 0;
        public string CustomerName = "";
        public string CustomerPhone = "";
        public bool IsKotSuccess { get; set; } = false;

        private async void FormPOS_Load(object sender, EventArgs e)
        {
            guna2DataGridView1.BorderStyle = BorderStyle.FixedSingle;
            AddCategory();
            ProductPanel.Controls.Clear();
            await LoadProductsAsync();
        }
        private void btnexit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private async void AddCategory()
        {
            string qry = "SELECT CategoryName FROM Category order by CategoryName asc";
            DataTable dt = new DataTable();

            // Use asynchronous data retrieval
            using (SqlConnection con = new SqlConnection(MainClass.con.ConnectionString))
            using (SqlCommand cmd = new SqlCommand(qry, con))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                await con.OpenAsync();
                await Task.Run(() => da.Fill(dt));
            }

            // Avoid updating UI controls directly in a background thread
            CategoryPanel.Invoke((MethodInvoker)delegate
            {
                CategoryPanel.Controls.Clear();

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        Guna.UI2.WinForms.Guna2Button b = new Guna.UI2.WinForms.Guna2Button
                        {
                            FillColor = Color.FromArgb(50, 55, 89),
                            Size = new Size(134, 45),
                            ButtonMode = Guna.UI2.WinForms.Enums.ButtonMode.RadioButton,
                            Text = row["CategoryName"].ToString()
                        };

                        // Event for click
                        b.Click += b_Click;

                        CategoryPanel.Controls.Add(b);
                    }
                }
            });
        }


        private void b_Click(object sender, EventArgs e)
        {
            Guna.UI2.WinForms.Guna2Button b = (Guna.UI2.WinForms.Guna2Button)sender;
            string categoryText = b.Text.Trim().ToLower();

            if (categoryText == "all categories")
            {
                txtSearch.Text = ""; // Clear the search text
                return;
            }

            // Ensure the UI operations are performed on the UI thread
            if (ProductPanel.InvokeRequired)
            {
                ProductPanel.Invoke(new Action(() =>
                {
                    FilterProductsByCategory(categoryText);
                }));
            }
            else
            {
                FilterProductsByCategory(categoryText);
            }
        }

        private void FilterProductsByCategory(string categoryText)
        {
            // Suspend layout updates to improve performance
            ProductPanel.SuspendLayout();

            try
            {
                // Precompute lower-case category text for comparison
                foreach (var item in ProductPanel.Controls.OfType<UsProduct>())
                {
                    // Use 'IndexOf' for case-insensitive comparison
                    bool isVisible = item.PCategory.IndexOf(categoryText, StringComparison.OrdinalIgnoreCase) >= 0;
                    if (item.Visible != isVisible)
                    {
                        item.Visible = isVisible;
                    }
                }
            }
            finally
            {
                // Resume layout updates
                ProductPanel.ResumeLayout();
            }
        }



        private async Task AddItemsAsync(string id, string ProID, string name, string cat, string price, Image Pimage)
        {
            var w = new UsProduct()
            {
                PName = name,
                pPrice = price,
                PCategory = cat,
                PIameg = Pimage,
                id = Convert.ToInt32(ProID)
            };

            // Ensure UI updates are performed on the main thread
            if (ProductPanel.InvokeRequired)
            {
                ProductPanel.Invoke(new Action(() => ProductPanel.Controls.Add(w)));
            }
            else
            {
                ProductPanel.Controls.Add(w);
            }

            w.onSelect += async (ss, ee) =>
            {
                var wdg = (UsProduct)ss;

                // Determine if the click was a right-click
                bool isRightClick = ee is MouseEventArgs mouseEventArgs && mouseEventArgs.Button == MouseButtons.Right;


                int quantityChange = isRightClick ? -1 : 1; // Subtract on right click, add otherwise


                bool productFound = false;

                foreach (DataGridViewRow item in guna2DataGridView1.Rows)
                {
                    if (Convert.ToInt32(item.Cells["dvgProID"].Value) == wdg.id)
                    {
                        int currentQty = int.Parse(item.Cells["dvgQty"].Value.ToString());
                        int newQty = currentQty + quantityChange;

                        if (newQty <= 0)
                        {
                            if (isRightClick)
                            {
                                if (!CheckIfPrinted(MainID))
                                {
                                    using (CustomMessageBox passwordBox = new CustomMessageBox())
                                    {
                                        if (passwordBox.ShowDialog() == DialogResult.OK)
                                        {
                                            string enteredPassword = passwordBox.Password;

                                            // Validate the entered password
                                            if (enteredPassword != "Cook ro cook")
                                            {
                                                MessageBox.Show("Password is worg");
                                                return;
                                            }
                                            else
                                            {
                                                // Remove the row if the quantity is zero or less
                                                guna2DataGridView1.Rows.Remove(item);
                                            }

                                        }
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("You cant delete bcz un paid bill is printed");
                                }
                                
                            }

                        }
                        else
                        {
                            // Update quantity and price
                            item.Cells["dvgQty"].Value = newQty.ToString();
                            item.Cells["dvgAmount"].Value = newQty * double.Parse(item.Cells["dvgPrice"].Value.ToString());
                        }

                        productFound = true;
                        break; // Exit the loop once the product is found and updated
                    }
                }

                // Add new row if product not found and the quantity change is positive
                if (!productFound && quantityChange > 0)
                {
                    if (!CheckIfPrinted(MainID))
                    {
                        guna2DataGridView1.Rows.Add(new object[] { 0, 0, wdg.id, wdg.PName, 1, wdg.pPrice, wdg.pPrice });
                    }
                    else
                    {
                        MessageBox.Show("Un paid bill in printed you can't add");
                    }
                }

                GetTotal(); // Update the total amount
               // txtSearch.Text = string.Empty;
                txtSearch.Focus();
            };
        }

        private async Task LoadProductsAsync()
        {
            string query = "SELECT Product.ProductID, Product.ProductName, Product.ProductPrice, Product.ProductImage, Category.CategoryName " +
                           "FROM Product " +
                           "INNER JOIN Category ON Product.CategoryID = Category.CategoryID Order by ProductName asc";

            using (SqlCommand cmd = new SqlCommand(query, MainClass.con))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                DataTable dt = new DataTable();
                await Task.Run(() => da.Fill(dt)); // Fill the DataTable asynchronously

                // Clear existing controls from the panel
                if (ProductPanel.InvokeRequired)
                {
                    ProductPanel.Invoke(new Action(() => ProductPanel.Controls.Clear()));
                }
                else
                {
                    ProductPanel.Controls.Clear();
                }

                foreach (DataRow row in dt.Rows)
                {
                    int productId = Convert.ToInt32(row["ProductID"]);
                    string productName = row["ProductName"].ToString();
                    string categoryName = row["CategoryName"].ToString();
                    string productPrice = row["ProductPrice"].ToString();

                    byte[] imageArray = row["ProductImage"] as byte[];
                    Image productImage = null;

                    if (imageArray != null && imageArray.Length > 0)
                    {
                        using (MemoryStream ms = new MemoryStream(imageArray))
                        {
                            productImage = await Task.Run(() => Image.FromStream(ms)); // Load image asynchronously
                        }
                    }

                    // Add item to the panel
                    await AddItemsAsync(productId.ToString(), productId.ToString(), productName, categoryName, productPrice, productImage);
                }
            }

            // Optionally, you can call a method to update the UI or perform any additional tasks
            // UpdateProductPanel(); // If needed
        }





        private async void txtSearch_TextChanged(object sender, EventArgs e)
        {
            // Reset the timer to debounce the search
            _searchTimer.Stop();
            _searchTimer.Start();
        }

        private async Task PerformSearchAsync(string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                // Show all items if the search text is empty
                await Task.Run(() =>
                {
                    foreach (var item in ProductPanel.Controls.OfType<UsProduct>())
                    {
                        if (item.InvokeRequired)
                        {
                            item.Invoke(new Action(() => item.Visible = true));
                        }
                        else
                        {
                            item.Visible = true;
                        }
                    }
                });
                return;
            }

            var visibilityResults = new ConcurrentDictionary<UsProduct, bool>();

            await Task.Run(() =>
            {
                Parallel.ForEach(ProductPanel.Controls.OfType<UsProduct>(), item =>
                {
                    bool matches = !string.IsNullOrEmpty(item.PName) &&
                                    item.PName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;
                    visibilityResults[item] = matches;
                });
            });

            // Update UI in a single operation
            ProductPanel.BeginInvoke(new Action(() =>
            {
                foreach (var kvp in visibilityResults)
                {
                    if (kvp.Key.InvokeRequired)
                    {
                        kvp.Key.Invoke(new Action(() => kvp.Key.Visible = kvp.Value));
                    }
                    else
                    {
                        kvp.Key.Visible = kvp.Value;
                    }
                }
            }));
        }

        private void GetTotal()
        {

            //
            int count = 0;

            foreach (DataGridViewRow row in guna2DataGridView1.Rows)
            {
                count++;
                row.Cells[0].Value = count;
            }


            // LbLToTal Amount Show POS Screen
            double tot = 0;
            lbltotal.Text = "";
            foreach (DataGridViewRow item in guna2DataGridView1.Rows)
            {
                tot += double.Parse(item.Cells["dvgAmount"].Value.ToString());
            }
            lbltotal.Text = tot.ToString("N2");
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            MainID = 0;
            lblTable.Text = "";
            lblWaiter.Text = "";
            lblTable.Visible = false;
            lblWaiter.Visible = false;
            guna2DataGridView1.Rows.Clear();
            MainID = 0;
            lbltotal.Text = "00";
            OrderType = "";
            btnKOT.Enabled = true;
            lblDriverName.Text = "";
            btnNew.Checked = true;
            btnTakeaway.Checked = false;
            btnDelivery.Checked = false;
            btnDil.Checked = false;
            dvgDelete.Visible = true;

        }

        private void btnDelivery_Click(object sender, EventArgs e)
        {
            btnNew.Checked = false;
            btnKOT.Enabled = true;
            lblTable.Text = "";
            lblWaiter.Text = "";
            lblTable.Visible = false;
            lblWaiter.Visible = false;
            OrderType = "Delievery";

            FormAddCustomer frm = new FormAddCustomer();
            frm.MainID = MainID;
            frm.OrderType = OrderType;
            MainClass.BlurBackground(frm);

            if (frm.txtName.Text != "")// as take away did not a driver
            {
                DriverID = frm.DriverID;
                lblDriverName.Text = "Customer Name: " + frm.txtName.Text + " Phone: " + frm.txtPhone.Text + " Driver: " + frm.cbDriver.Text;
                lblDriverName.Visible = true;
                CustomerName = frm.txtName.Text;
                CustomerPhone = frm.txtPhone.Text;


            }
        }

        private void btnDil_Click(object sender, EventArgs e)
        {
            btnNew.Checked = false;
            btnKOT.Enabled = true;
            OrderType = "Din IN";
            lblDriverName.Visible = false;
            // Need to create form For Table Selection And Waiter selection
            FormTableSelect frm = new FormTableSelect();
            MainClass.BlurBackground(frm);
            if (frm.TableName != "")
            {
                lblTable.Text = frm.TableName;
                lblTable.Visible = true;
            }
            else
            {
                lblTable.Text = "";
                lblTable.Visible = false;
            }

            FormWaiterSelect frm2 = new FormWaiterSelect();
            MainClass.BlurBackground(frm2);
            if (frm2.WaiterName != "")
            {
                lblWaiter.Text = frm2.WaiterName;
                lblWaiter.Visible = true;
            }
            else
            {
                lblWaiter.Text = "";
                lblWaiter.Visible = false;
            }
        }

        private void btnKOT_Click_1(object sender, EventArgs e)
        {
            string qry1 = ""; // Main Table
            string qry2 = ""; // Detail Table

            int DetailID = 0;

            if (OrderType == "")
            {
                MessageBox.Show("Please Select Order Type");
                return;
            }

            if (guna2DataGridView1.Rows.Count < 0)
            {

                MessageBox.Show("Please add Items");
                return;
            }

            if (MainID == 0)
            {
                if (!IsTableNameAvailable(lblTable.Text))
                {
                    MessageBox.Show("this table already reserved");
                    return;
                }
            }

            if (MainID == 0) // Insert
            {
                qry1 = @"INSERT INTO TblMain (Date, Time, TableName, WaiterName, Status, OrderType, Total, Recieved, Change, DriverID, CustName, CustPhone)
                 VALUES (@Date, @Time, @TableName, @WaiterName, @Status, @OrderType, @Total, @Recieved, @Change, @DriverID, @CustName, @CustPhone);
                 SELECT SCOPE_IDENTITY();";
            }
            else // Update
            {
                qry1 = @"UPDATE TblMain
                 SET TableName = @TableName ,WaiterName = @WaiterName,Status = @Status,OrderType = @OrderType, 
                    Total = @Total,Recieved = @Recieved, Change = @Change,IsOrderPrint = @IsOrderPrint,
                    Time = @Time
                 WHERE MainID = @ID;";


            }

            SqlCommand cmd = new SqlCommand(qry1, MainClass.con);

            if (MainID != 0)
            {
                cmd.Parameters.AddWithValue("@ID", MainID);
            }
            cmd.Parameters.AddWithValue("@Date", DateTime.Now.Date);
            cmd.Parameters.AddWithValue("@Time", DateTime.Now.ToShortTimeString());
            cmd.Parameters.AddWithValue("@TableName", lblTable.Text);
            cmd.Parameters.AddWithValue("@WaiterName", lblWaiter.Text);
            cmd.Parameters.AddWithValue("@Status", "Complete");
            cmd.Parameters.AddWithValue("@OrderType", OrderType); // Make sure OrderType is provided and not null
            cmd.Parameters.AddWithValue("@Total", Convert.ToDouble(lbltotal.Text));
            cmd.Parameters.AddWithValue("@Recieved", Convert.ToDouble(0));
            cmd.Parameters.AddWithValue("@Change", Convert.ToDouble(0));
            cmd.Parameters.AddWithValue("@DriverID", DriverID);
            cmd.Parameters.AddWithValue("@CustName", CustomerName);
            cmd.Parameters.AddWithValue("@CustPhone", CustomerPhone);
                
            cmd.Parameters.AddWithValue("@IsOrderPrint", 0);

            if (MainClass.con.State == ConnectionState.Closed) { MainClass.con.Open(); }
            if (MainID == 0) { MainID = Convert.ToInt32(cmd.ExecuteScalar()); } else { cmd.ExecuteNonQuery(); }
            if (MainClass.con.State == ConnectionState.Open) { MainClass.con.Close(); }

            bool operationSuccessful = true;
            ProcessOrderChanges();

            string deleteQuery = "DELETE FROM TblDetail WHERE MainID = @MainID";

            // First, delete the existing records
            using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, MainClass.con))
            {
                deleteCmd.Parameters.AddWithValue("@MainID", MainID);

                try
                {
                    if (MainClass.con.State == ConnectionState.Closed)
                        MainClass.con.Open();

                    deleteCmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    operationSuccessful = false;
                    MessageBox.Show("Error occurred during deletion: " + ex.Message, "Error");
                }
                finally
                {
                    if (MainClass.con.State == ConnectionState.Open)
                        MainClass.con.Close();
                }
            }

            foreach (DataGridViewRow row in guna2DataGridView1.Rows)
            {
                // Ensure that the row is not the new row placeholder
                if (row.IsNewRow) continue;

                DetailID = Convert.ToInt32(row.Cells["dvgid"].Value);
                DetailID = 0;

                if (DetailID == 0)
                {
                    qry2 = @"INSERT INTO TblDetail (MainID, ProID, Qty, Price, Amount) 
                  VALUES (@MainID, @ProID, @Qty, @Price, @Amount)";
                }
                //else
                //{
                //  qry2 = @"UPDATE TblDetail 
                //  SET ProID = @ProID, Qty = @Qty, Price = @Price, Amount = @Amount
                //  WHERE DetailID = @ID";
                //}

                using (SqlCommand cmd2 = new SqlCommand(qry2, MainClass.con))
                {
                    if (DetailID != 0)
                    {
                        cmd2.Parameters.AddWithValue("@ID", DetailID);
                    }
                    cmd2.Parameters.AddWithValue("@MainID", MainID);
                    cmd2.Parameters.AddWithValue("@ProID", Convert.ToInt32(row.Cells["DvgProID"].Value));
                    cmd2.Parameters.AddWithValue("@Price", Convert.ToDouble(row.Cells["DvgPrice"].Value));

                    int qty;
                    if (int.TryParse(row.Cells["DvgQty"].Value?.ToString(), out qty))
                    {
                        cmd2.Parameters.AddWithValue("@Qty", qty);
                    }
                    else
                    {
                        qty = 0; // or any default value you prefer
                        cmd2.Parameters.AddWithValue("@Qty", qty);
                    }

                    cmd2.Parameters.AddWithValue("@Amount", Convert.ToDouble(row.Cells["DvgAmount"].Value));

                    try
                    {
                        if (MainClass.con.State == ConnectionState.Closed)
                            MainClass.con.Open();

                        cmd2.ExecuteNonQuery();

                    }
                    catch (Exception ex)
                    {
                        operationSuccessful = false;
                        MessageBox.Show("Error occurred: " + ex.Message, "Error");
                    }
                    finally
                    {
                        if (MainClass.con.State == ConnectionState.Open)
                            MainClass.con.Close();
                    }
                }
            }

            if (operationSuccessful)
            {
                MessageBox.Show("Operation has been successfully done", "Notification");
            }

            // Clear DataGridView and reset labelsa
            guna2DataGridView1.Rows.Clear();
            lblTable.Text = "";
            lblWaiter.Text = "";
            lblTable.Visible = false;
            lblWaiter.Visible = false;
            lbltotal.Text = "00";
            lblDriverName.Text = "";
            OrderType = "";
            //FormKitchenView kv = new FormKitchenView();
            //kv.ShowDialog();
            FormBillList frm = new FormBillList();
            frm.ShowDialog();
            if (frm.MainID > 0)
            {
                id = frm.MainID;
                MainID = frm.MainID;
                LoadEntries();
                if (CheckIfPrinted(MainID))
                {
                    dvgDelete.Visible = false;
                }
                else
                {
                    dvgDelete.Visible = true;
                }


            }
            else
            {
                MainID = 0;
                btnDelivery.Checked = false;
                btnDil.Checked = false;
                btnTakeaway.Checked = false;
            }

        }


        public int CountItemsByMainID(int mainID)
        {
            int itemCount = 0;

            // Define the SQL query to count items by MainID
            string query = "SELECT COUNT(*) FROM tblDetail WHERE MainID = @MainID";

            // Using SqlConnection to connect to the database
            using (SqlConnection conn = new SqlConnection(MainClass.con_string))
            {
                // Create a SqlCommand object with the query and connection
                SqlCommand cmd = new SqlCommand(query, conn);

                // Add the MainID parameter to the command
                cmd.Parameters.AddWithValue("@MainID", mainID);

                try
                {
                    // Open the database connection
                    conn.Open();

                    // Execute the query and get the count
                    itemCount = (int)cmd.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    // Handle any errors here
                    Console.WriteLine("Error: " + ex.Message);
                }
                finally
                {
                    // Ensure the connection is closed even if there is an exception
                    conn.Close();
                }
            }

            return itemCount;
        }

        private bool IsTableNameAvailable(string tableNameToValidate)
        {
            string query = @"SELECT COUNT(*) 
                     FROM TblMain 
                     WHERE Status = 'Complete' 
                       AND TableName <> '' 
                       AND TableName = @TableName";

            using (SqlCommand cmd = new SqlCommand(query, MainClass.con))
            {
                cmd.Parameters.Add(new SqlParameter("@TableName", SqlDbType.VarChar)).Value = tableNameToValidate;

                try
                {
                    if (MainClass.con.State == ConnectionState.Closed)
                        MainClass.con.Open();

                    int count = (int)cmd.ExecuteScalar();

                    return count == 0; // Return true if no rows match the criteria (i.e., table name is available)
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error occurred during query execution: " + ex.Message, "Error");
                    return false; // Return false in case of an error
                }
                finally
                {
                    if (MainClass.con.State == ConnectionState.Open)
                        MainClass.con.Close();
                }
            }
        }

        private void ProcessOrderChanges()
        {
            int maxCategoryId = 0;

            // Retrieve the maximum order count for the given mainId
            using (SqlConnection con = new SqlConnection(MainClass.con.ConnectionString))
            {
                string maxCategoryQuery = "SELECT MAX(ordercount) FROM tblOrderLog WHERE mainid = @MainId";
                using (SqlCommand cmd = new SqlCommand(maxCategoryQuery, con))
                {
                    cmd.Parameters.AddWithValue("@MainId", MainID);
                    con.Open();
                    object result = cmd.ExecuteScalar();
                    if (result != DBNull.Value)
                    {
                        maxCategoryId = Convert.ToInt32(result);
                    }
                }
            }


            // Iterate through each row in the DataGridView
            foreach (DataGridViewRow row in guna2DataGridView1.Rows)
            {
                if (row.IsNewRow) continue;

                int itemId = Convert.ToInt32(row.Cells["DvgProID"].Value);
                int newQty = Convert.ToInt32(row.Cells["DvgQty"].Value);

                // Check the existing entry in tblOrderLog
                string checkLogQuery = @"
                   SELECT SUM(qty) AS qty, ordercount
                    FROM tblOrderLog
                    WHERE mainid = @MainID AND itemid = @ItemID AND isdeleted = 0
                    GROUP BY itemid, ordercount, logid
                    ORDER BY logid DESC
                ";

                using (SqlCommand checkCmd = new SqlCommand(checkLogQuery, MainClass.con))
                {
                    checkCmd.Parameters.AddWithValue("@MainID", MainID);
                    checkCmd.Parameters.AddWithValue("@ItemID", itemId);

                    try
                    {
                        if (MainClass.con.State == ConnectionState.Closed)
                            MainClass.con.Open();

                        SqlDataReader reader = checkCmd.ExecuteReader();

                        int lastQty = 0;
                        // int lastOrderCount = 0;
                        bool exists = reader.Read();
                        if (exists)
                        {
                            lastQty = Convert.ToInt32(reader["qty"]);
                            //lastOrderCount = Convert.ToInt32(reader["ordercount"]);
                        }


                        reader.Close();

                        // Determine the new order count
                        //int newOrderCount = exists ? lastOrderCount + 1 : 1;

                        // Determine if we need to log an addition or reduction
                        if (exists)
                        {
                            if (newQty > lastQty)
                            {
                                // Quantity increased
                                LogOrderChange(MainID, itemId, newQty - lastQty, maxCategoryId + 1, false);
                            }
                            else if (newQty < lastQty)
                            {
                                // Quantity decreased
                                LogOrderChange(MainID, itemId, -(lastQty - newQty), maxCategoryId + 1, false);
                            }
                        }
                        else
                        {
                            // New item, log addition
                            LogOrderChange(MainID, itemId, newQty, maxCategoryId + 1, false);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error occurred: " + ex.Message, "Error");
                    }
                    finally
                    {
                        if (MainClass.con.State == ConnectionState.Open)
                            MainClass.con.Close();
                    }
                }
            }

            // Check for deletions (items that were logged but are not in DataGridView)

            string deleteCheckQuery = @"
                SELECT DISTINCT itemid, qty, ordercount
                FROM tblOrderLog
                WHERE mainid = @MainID AND isdeleted = 0";

            using (SqlCommand deleteCheckCmd = new SqlCommand(deleteCheckQuery, MainClass.con))
            {
                deleteCheckCmd.Parameters.AddWithValue("@MainID", MainID);

                try
                {
                    if (MainClass.con.State == ConnectionState.Closed)
                        MainClass.con.Open();

                    SqlDataReader reader = deleteCheckCmd.ExecuteReader();

                    List<int> existingItems = new List<int>();
                    Dictionary<int, int> existingQuantities = new Dictionary<int, int>();
                    Dictionary<int, int> existingOrderCounts = new Dictionary<int, int>();

                    while (reader.Read())
                    {
                        int existingItemId = Convert.ToInt32(reader["itemid"]);
                        int existingQty = Convert.ToInt32(reader["qty"]);
                        int existingOrderCount = Convert.ToInt32(reader["ordercount"]);

                        existingItems.Add(existingItemId);
                        existingQuantities[existingItemId] = existingQty;
                        existingOrderCounts[existingItemId] = existingOrderCount;
                    }

                    reader.Close();

                    foreach (int itemId in existingItems)
                    {
                        bool itemInGrid = false;
                        foreach (DataGridViewRow row in guna2DataGridView1.Rows)
                        {
                            if (row.IsNewRow) continue;

                            int gridItemId = Convert.ToInt32(row.Cells["DvgProID"].Value);
                            if (itemId == gridItemId)
                            {
                                itemInGrid = true;
                                break;
                            }
                        }

                        if (!itemInGrid)
                        {
                            // Item is not in DataGridView, so it has been deleted
                            int previousQty = existingQuantities[itemId];
                            int lastOrderCount = existingOrderCounts[itemId];
                            SoftDeleteItem(MainID, itemId);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error occurred: " + ex.Message, "Error");
                }
                finally
                {
                    if (MainClass.con.State == ConnectionState.Open)
                        MainClass.con.Close();
                }
            }
        }
        private void SoftDeleteItem(int mainId, int itemId)
        {
            string softDeleteQuery = @"UPDATE tblOrderLog 
                               SET isdeleted = 1 
                               WHERE mainid = @MainID AND itemid = @ItemID";

            using (SqlCommand cmd = new SqlCommand(softDeleteQuery, MainClass.con))
            {
                cmd.Parameters.AddWithValue("@MainID", mainId);
                cmd.Parameters.AddWithValue("@ItemID", itemId);

                try
                {
                    if (MainClass.con.State == ConnectionState.Closed)
                        MainClass.con.Open();

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                       // MessageBox.Show("Item marked as deleted successfully.", "Success");
                    }
                    else
                    {
                        MessageBox.Show("No matching item found to mark as deleted.", "Warning");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error occurred during soft deletion: " + ex.Message, "Error");
                }
                finally
                {
                    if (MainClass.con.State == ConnectionState.Open)
                        MainClass.con.Close();
                }
            }
        }

        private void LogOrderChange(int mainId, int itemId, int qty, int orderCount, bool isDeleted)
        {
            string logQuery = @"INSERT INTO tblOrderLog (mainid, itemid, qty, ordercount, isdeleted)
                        VALUES (@MainID, @ItemID, @Qty, @OrderCount, @IsDeleted)";

            using (SqlCommand cmd = new SqlCommand(logQuery, MainClass.con))
            {
                cmd.Parameters.AddWithValue("@MainID", mainId);
                cmd.Parameters.AddWithValue("@ItemID", itemId);
                cmd.Parameters.AddWithValue("@Qty", qty);
                cmd.Parameters.AddWithValue("@OrderCount", orderCount);
                cmd.Parameters.AddWithValue("@IsDeleted", isDeleted ? 1 : 0);

                try
                {
                    if (MainClass.con.State == ConnectionState.Closed)
                        MainClass.con.Open();

                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error occurred during logging: " + ex.Message, "Error");
                }
                finally
                {
                    if (MainClass.con.State == ConnectionState.Open)
                        MainClass.con.Close();
                }
            }
        }


        public int id = 0;
        private void btnBilList_Click(object sender, EventArgs e)
        {
            FormBillList frm = new FormBillList();
            MainClass.BlurBackground(frm);

            if (frm.MainID > 0)
            {
                id = frm.MainID;
                MainID = frm.MainID;
                LoadEntries();

                if (CheckIfPrinted(MainID))
                {
                    dvgDelete.Visible = false;
                }
                else
                {
                    dvgDelete.Visible = true;
                }

            }

        }
        public void LoadEntries()
        {
            string Qry2 = @"SELECT 0,d.DetailID,p.ProductID,p.ProductName,d.Qty,d.Price,d.Amount,m.OrderType,m.TableName,m.waiterName,m.CustName,CustPhone from TblMain m INNER JOIN TblDetail d ON m.MainID = d.MainID INNER JOIN Product p ON p.ProductID = d.ProID WHERE m.MainID = " + id + " ";

            SqlCommand cmd = new SqlCommand(Qry2, MainClass.con);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            OrderType = dt.Rows[0]["OrderType"].ToString();
            if (dt.Rows[0]["OrderType"].ToString() == "Din IN")
            {
                btnTakeaway.Checked = false;
                btnDelivery.Checked = false;
                btnDil.Checked = true;
                lblWaiter.Visible = false;
                lblTable.Visible = false;
            }
            else if (dt.Rows[0]["OrderType"].ToString() == "Take Away")
            {
                btnDil.Checked = false;
                btnDelivery.Checked = false;
                btnTakeaway.Checked = true;
                lblWaiter.Visible = true;
                lblTable.Visible = true;
                lblDriverName.Visible = true;

            }
            else
            {
                btnDelivery.Checked = false;
                btnDil.Checked = false;
                btnDelivery.Checked = true;
                lblWaiter.Visible = true;
                lblTable.Visible = true;
                lblDriverName.Visible = true;
            }

            guna2DataGridView1.Rows.Clear();

            foreach (DataRow item in dt.Rows)
            {

                // Extract data from DataRow

                lblTable.Text = item["TableName"].ToString();
                lblWaiter.Text = item["WaiterName"].ToString();
                lblDriverName.Text = dt.Rows[0]["CustName"].ToString() + " " + dt.Rows[0]["CustPhone"].ToString();
                //lblDriverName.Text = item[""].ToString();
                lblWaiter.Visible = true;
                lblTable.Visible = true;
                string Detailid = item["DetailID"].ToString();
                string Proid = item["ProductID"].ToString();
                string ProName = item["ProductName"].ToString();
                string qty = item["Qty"].ToString();
                string Price = item["Price"].ToString();
                string Amount = item["Amount"].ToString();

                // Add row to DataGridView
                object[] obj = { 0, Detailid, Proid, ProName, qty, Price, Amount };
                guna2DataGridView1.Rows.Add(obj);

            }
            GetTotal();
        }

        private void btnCheckOut_Click(object sender, EventArgs e)
        {



            if (OrderType == "")
            {
                MessageBox.Show("Please Select Order Type");
                return;
            }



            string qry1 = ""; // Main Table
            string qry2 = ""; // Detail Table

            int DetailID = 0;

            if (OrderType == "")
            {
                MessageBox.Show("Please Select Order Type");
                return;
            }

            if (guna2DataGridView1.Rows.Count < 0)
            {

                MessageBox.Show("Please add Items");
                return;
            }

            if (MainID == 0) // Insert
            {
                qry1 = @"INSERT INTO TblMain (Date, Time, TableName, WaiterName, Status, OrderType, Total, Recieved, Change, DriverID, CustName, CustPhone)
                 VALUES (@Date, @Time, @TableName, @WaiterName, @Status, @OrderType, @Total, @Recieved, @Change, @DriverID, @CustName, @CustPhone);
                 SELECT SCOPE_IDENTITY();";
            }
            else // Update
            {
                qry1 = @"UPDATE TblMain
                 SET Status = @Status,OrderType = @OrderType, Total = @Total,Recieved = @Recieved, Change = @Change
                 WHERE MainID = @ID;";


            }

            SqlCommand cmd = new SqlCommand(qry1, MainClass.con);

            if (MainID != 0)
            {
                cmd.Parameters.AddWithValue("@ID", MainID);
            }
            cmd.Parameters.AddWithValue("@Date", DateTime.Now.Date);
            cmd.Parameters.AddWithValue("@Time", DateTime.Now.ToShortTimeString());
            cmd.Parameters.AddWithValue("@TableName", lblTable.Text);
            cmd.Parameters.AddWithValue("@WaiterName", lblWaiter.Text);
            cmd.Parameters.AddWithValue("@Status", "Complete");
            //if (OrderType == "Take Away")
            //{
            //    cmd.Parameters.AddWithValue("@Status", "Paid");


            //}
            //else
            //{
            //    cmd.Parameters.AddWithValue("@Status", "Complete");
            //}


            cmd.Parameters.AddWithValue("@OrderType", OrderType); // Make sure OrderType is provided and not null
            cmd.Parameters.AddWithValue("@Total", Convert.ToDouble(lbltotal.Text));
            cmd.Parameters.AddWithValue("@Recieved", Convert.ToDouble(0));
            cmd.Parameters.AddWithValue("@Change", Convert.ToDouble(0));
            cmd.Parameters.AddWithValue("@DriverID", DriverID);
            cmd.Parameters.AddWithValue("@CustName", CustomerName);
            cmd.Parameters.AddWithValue("@CustPhone", CustomerPhone);

            if (MainClass.con.State == ConnectionState.Closed) { MainClass.con.Open(); }
            if (MainID == 0) { MainID = Convert.ToInt32(cmd.ExecuteScalar()); } else { cmd.ExecuteNonQuery(); }
            if (MainClass.con.State == ConnectionState.Open) { MainClass.con.Close(); }

            //bool operationSuccessful = true;
            //ProcessOrderChanges();
            //if (OrderType == "Take Away")
            //{
            //    OrderPrint(MainID);

            //    UpdatePrintStatusOrder(MainID);
            //}

            string deleteQuery = "DELETE FROM TblDetail WHERE MainID = @MainID";

            // First, delete the existing records
            using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, MainClass.con))
            {
                deleteCmd.Parameters.AddWithValue("@MainID", MainID);

                try
                {
                    if (MainClass.con.State == ConnectionState.Closed)
                        MainClass.con.Open();

                    deleteCmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    //operationSuccessful = false;
                    MessageBox.Show("Error occurred during deletion: " + ex.Message, "Error");
                }
                finally
                {
                    if (MainClass.con.State == ConnectionState.Open)
                        MainClass.con.Close();
                }
            }

            foreach (DataGridViewRow row in guna2DataGridView1.Rows)
            {
                // Ensure that the row is not the new row placeholder
                if (row.IsNewRow) continue;

                DetailID = Convert.ToInt32(row.Cells["dvgid"].Value);
                DetailID = 0;

                if (DetailID == 0)
                {
                    qry2 = @"INSERT INTO TblDetail (MainID, ProID, Qty, Price, Amount) 
                  VALUES (@MainID, @ProID, @Qty, @Price, @Amount)";
                }
                //else
                //{
                //  qry2 = @"UPDATE TblDetail 
                //  SET ProID = @ProID, Qty = @Qty, Price = @Price, Amount = @Amount
                //  WHERE DetailID = @ID";
                //}

                using (SqlCommand cmd2 = new SqlCommand(qry2, MainClass.con))
                {
                    if (DetailID != 0)
                    {
                        cmd2.Parameters.AddWithValue("@ID", DetailID);
                    }
                    cmd2.Parameters.AddWithValue("@MainID", MainID);
                    cmd2.Parameters.AddWithValue("@ProID", Convert.ToInt32(row.Cells["DvgProID"].Value));
                    cmd2.Parameters.AddWithValue("@Price", Convert.ToDouble(row.Cells["DvgPrice"].Value));

                    int qty;
                    if (int.TryParse(row.Cells["DvgQty"].Value?.ToString(), out qty))
                    {
                        cmd2.Parameters.AddWithValue("@Qty", qty);
                    }
                    else
                    {
                        qty = 0; // or any default value you prefer
                        cmd2.Parameters.AddWithValue("@Qty", qty);
                    }

                    cmd2.Parameters.AddWithValue("@Amount", Convert.ToDouble(row.Cells["DvgAmount"].Value));

                    try
                    {
                        if (MainClass.con.State == ConnectionState.Closed)
                            MainClass.con.Open();

                        cmd2.ExecuteNonQuery();

                    }
                    catch (Exception ex)
                    {
                        //operationSuccessful = false;
                        MessageBox.Show("Error occurred: " + ex.Message, "Error");
                    }
                    finally
                    {
                        if (MainClass.con.State == ConnectionState.Open)
                            MainClass.con.Close();
                    }
                }
            }
            if (CheckIfPrinted(MainID))
            {

                ProcessOrderChanges();


            }
            //OrderPrint(MainID);
            //UpdatePrintStatusOrder(MainID);

            FormCheckOut from = new FormCheckOut();
            from.MainID = MainID;
            from.amt = Convert.ToDouble(lbltotal.Text);
            MainClass.BlurBackground(from);

            MainID = 0;
            guna2DataGridView1.Rows.Clear();
            lblTable.Text = "";
            lblWaiter.Text = "";
            lblTable.Visible = false;
            lblWaiter.Visible = false;
            lbltotal.Text = "00";
            FormPaidBill pd = new FormPaidBill();
            pd.ShowDialog();
            OrderType = "";
            btnKOT.Enabled = true;
            btnNew_Click(null, null);
        }



        private bool CheckIfPrinted(int id)
        {
            string query = "SELECT IsNull(IsPrintUnPaid,0) FROM TblMain WHERE MainID = @ID";

            try
            {
                using (SqlConnection con = new SqlConnection(MainClass.con.ConnectionString))
                {
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@ID", id);

                    con.Open();
                    object result = cmd.ExecuteScalar();
                    con.Close();

                    // Check if the result is not null and convert to boolean
                    return result != null && Convert.ToBoolean(result);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while checking print status: " + ex.Message);
                return false; // Default to not printed in case of error
            }
        }



        private void UpdatePrintStatusOrder(int mainId)
        {
            string query = "UPDATE TblMain SET IsOrderPrint = 1 WHERE MainID = @ID";

            try
            {
                using (SqlConnection con = new SqlConnection(MainClass.con.ConnectionString))
                {
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@ID", mainId);

                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while updating print status: " + ex.Message);
            }
        }

        private void OrderPrint(int mainId)
        {
            int maxCategoryId = 0;

            // Retrieve the maximum order count for the given mainId
            using (SqlConnection con = new SqlConnection(MainClass.con.ConnectionString))
            {
                string maxCategoryQuery = "SELECT MAX(ordercount) FROM tblOrderLog WHERE mainid = @MainId";
                using (SqlCommand cmd = new SqlCommand(maxCategoryQuery, con))
                {
                    cmd.Parameters.AddWithValue("@MainId", mainId);
                    con.Open();
                    object result = cmd.ExecuteScalar();
                    if (result != DBNull.Value)
                    {
                        maxCategoryId = Convert.ToInt32(result);
                    }
                }
            }

            //for (int i = 1; i <= maxCategoryId; i++)  // Ensure loop includes maxCategoryId
            //{
            var reportDocument = new ReportDocument();
            string reportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\Reportss\crptOrderReports.rpt");

            // First Print: CategoryID = 8
            using (SqlConnection con = new SqlConnection(MainClass.con.ConnectionString))
            {
                string query = @"
                            SELECT 
                                m.MainID AS 'InvoiceNo',
                                m.Date,
                                m.Time,
                                m.OrderType,
                                m.CustName,
                                m.TableName,
                                m.WaiterName,
                                p.ProductName,  
                                d.Qty,
                                p.ProductPrice,
                                p.ProductPrice * d.qty AS 'TotalPrice',
                                m.Total,
                                m.Recieved AS 'Received',
                                m.Change AS 'Changed',
                                d.ordercount
                            FROM
                                TblMain m
                                INNER JOIN tblOrderLog d ON m.MainID = d.MainID
                                INNER JOIN Product p ON p.ProductID = d.itemid
                            WHERE
                                m.MainID = @InvoiceId AND d.ordercount = @OrderCount and IsDeleted = 0
                                AND (p.CategoryID = @CategoryId OR (@CategoryId IS NULL AND p.CategoryID <> 8))";

                reportDocument.Load(reportPath);

                // Print for CategoryID = 8
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@InvoiceId", mainId);
                    cmd.Parameters.AddWithValue("@CategoryId", 8);
                    cmd.Parameters.AddWithValue("@OrderCount", maxCategoryId);

                    var billDataSet = new DSBill();
                    var adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(billDataSet, "BillDT");

                    // Check if data exists in the DataTable before printing
                    if (billDataSet.Tables["BillDT"].Rows.Count > 0)
                    {
                        reportDocument.SetDataSource(billDataSet.Tables["BillDT"]);

                        // Set up printer settings
                        var printerSettings = new PrinterSettings();
                        var pageSettings = new PageSettings
                        {
                            PaperSize = new PaperSize("Thermal", 350, 700),
                            Margins = new Margins(0, 0, 0, 0)
                        };

                        reportDocument.PrintOptions.PrinterName = printerSettings.PrinterName;
                        reportDocument.PrintOptions.PaperSize = (CrystalDecisions.Shared.PaperSize)pageSettings.PaperSize.RawKind;

                        // Print the report only if data exists
                        reportDocument.PrintToPrinter(printerSettings, pageSettings, false);
                    }

                }

                // Print for CategoryID != 8
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@InvoiceId", mainId);
                    cmd.Parameters.AddWithValue("@CategoryId", DBNull.Value);  // Null condition for CategoryID != 8
                    cmd.Parameters.AddWithValue("@OrderCount", maxCategoryId);

                    var billDataSet = new DSBill();
                    var adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(billDataSet, "BillDT");

                    // Check if data exists in the DataTable before printing
                    if (billDataSet.Tables["BillDT"].Rows.Count > 0)
                    {
                        reportDocument.SetDataSource(billDataSet.Tables["BillDT"]);

                        // Set up printer settings
                        var printerSettings = new PrinterSettings();
                        var pageSettings = new PageSettings
                        {
                            PaperSize = new PaperSize("Thermal", 350, 700),
                            Margins = new Margins(0, 0, 0, 0)
                        };

                        reportDocument.PrintOptions.PrinterName = printerSettings.PrinterName;
                        reportDocument.PrintOptions.PaperSize = (CrystalDecisions.Shared.PaperSize)pageSettings.PaperSize.RawKind;

                        // Print the report only if data exists
                        reportDocument.PrintToPrinter(printerSettings, pageSettings, false);
                    }

                }

            }
            //}
        }

        private void btnHold_Click(object sender, EventArgs e)
        {
            string qry1 = ""; // Main Table
            string qry2 = ""; // Detail Table

            int DetailID = 0;

            if (OrderType == "")
            {
                MessageBox.Show("Please Select Order Type");
                return;
            }

            if (MainID == 0) // Insert
            {
                qry1 = @"INSERT INTO TblMain (Date, Time, TableName, WaiterName, Status, OrderType, Total, Recieved, Change)
                 VALUES (@Date, @Time, @TableName, @WaiterName, @Status, @OrderType, @Total, @Recieved, @Change,@DriverID,@CustName,@CustPhone);
                 SELECT SCOPE_IDENTITY();";
            }
            else // Update
            {
                qry1 = @"UPDATE TblMain
                 SET Status = @Status, Total = @Total, Recieved = @Recieved, Change = @Change
                 WHERE MainID = @ID;";
            }

            SqlCommand cmd = new SqlCommand(qry1, MainClass.con);

            if (MainID != 0)
            {
                cmd.Parameters.AddWithValue("@ID", MainID);
            }
            cmd.Parameters.AddWithValue("@Date", DateTime.Now.Date);
            cmd.Parameters.AddWithValue("@Time", DateTime.Now.ToShortTimeString());
            cmd.Parameters.AddWithValue("@TableName", lblTable.Text);
            cmd.Parameters.AddWithValue("@WaiterName", lblWaiter.Text);
            cmd.Parameters.AddWithValue("@Status", "Hold");
            cmd.Parameters.AddWithValue("@OrderType", OrderType); // Make sure OrderType is provided and not null
            cmd.Parameters.AddWithValue("@Total", Convert.ToDouble(lbltotal.Text));
            cmd.Parameters.AddWithValue("@Recieved", Convert.ToDouble(0));
            cmd.Parameters.AddWithValue("@Change", Convert.ToDouble(0));
            cmd.Parameters.AddWithValue("@DriverID", DriverID);
            cmd.Parameters.AddWithValue("@CustName", CustomerName);
            cmd.Parameters.AddWithValue("@CustPhone", CustomerPhone);

            if (MainClass.con.State == ConnectionState.Closed) { MainClass.con.Open(); }
            if (MainID == 0) { MainID = Convert.ToInt32(cmd.ExecuteScalar()); } else { cmd.ExecuteNonQuery(); }
            if (MainClass.con.State == ConnectionState.Open) { MainClass.con.Close(); }

            foreach (DataGridViewRow row in guna2DataGridView1.Rows)
            {
                DetailID = Convert.ToInt32(row.Cells["dvgid"].Value);

                if (DetailID != 0)
                {
                    qry2 = @"INSERT INTO TblDetail (MainID, ProID, Qty, Price, Amount) 
                          VALUES (@MainID, @ProID, @Qty, @Price, @Amount)";
                }
                else
                {
                    qry2 = @"UPDATE TblDetail 
                          SET ProID = @ProID, Qty = @Qty, Price = @Price, Amount = @Amount
                          WHERE DetailID = @ID";
                }

                SqlCommand cmd2 = new SqlCommand(qry2, MainClass.con);
                if (DetailID != 0)
                {
                    cmd2.Parameters.AddWithValue("@ID", DetailID);
                }
                cmd2.Parameters.AddWithValue("@MainID", MainID);
                cmd2.Parameters.AddWithValue("@ProID", Convert.ToInt32(row.Cells["DvgProID"].Value));
                cmd2.Parameters.AddWithValue("@Price", Convert.ToDouble(row.Cells["DvgPrice"].Value));

                int qty;
                if (int.TryParse(row.Cells["DvgQty"].Value?.ToString(), out qty))
                {
                    cmd2.Parameters.AddWithValue("@Qty", qty);
                }
                else
                {
                    qty = 0; // or any default value you prefer
                    cmd2.Parameters.AddWithValue("@Qty", qty);
                }

                cmd2.Parameters.AddWithValue("@Amount", Convert.ToDouble(row.Cells["DvgAmount"].Value));


                if (MainClass.con.State == ConnectionState.Closed) { MainClass.con.Open(); }
                cmd2.ExecuteNonQuery();
                if (MainClass.con.State == ConnectionState.Open) { MainClass.con.Close(); }


                MessageBox.Show("Operation has been successfully done", "Notification");
                MainID = 0;
                DetailID = 0;
                guna2DataGridView1.Rows.Clear();
                lblTable.Text = "";
                lblWaiter.Text = "";
                lblTable.Visible = false;
                lblWaiter.Visible = false;
                lbltotal.Text = "00";
                lblDriverName.Text = "";
            }
        }

        private void btnTakeaway_Click(object sender, EventArgs e)
        {
            btnNew.Checked = false;
            lblTable.Text = "";
            lblWaiter.Text = "";
            lblTable.Visible = false;
            lblWaiter.Visible = false;
            OrderType = "Take Away";


            FormAddCustomer frm = new FormAddCustomer();
            frm.MainID = MainID;
            frm.OrderType = OrderType;
            MainClass.BlurBackground(frm);

            if (frm.txtName.Text != "")// as take away did not a driver
            {
                DriverID = frm.DriverID;
                lblDriverName.Text = "Customer Name: " + frm.txtName.Text + " Phone: " + frm.txtPhone.Text;
                lblDriverName.Visible = true;
                CustomerName = frm.txtName.Text;
                CustomerPhone = frm.txtPhone.Text;

                btnTakeaway.Checked = true;
                //btnKOT.Enabled = false;
            }

        }

        private void guna2DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == guna2DataGridView1.Columns["dvgDelete"].Index && e.RowIndex >= 0)
                if (e.ColumnIndex == guna2DataGridView1.Columns["dvgDelete"].Index && e.RowIndex >= 0)
                {
                    using (CustomMessageBox passwordBox = new CustomMessageBox())
                    {
                        if (passwordBox.ShowDialog() == DialogResult.OK)
                        {
                            string enteredPassword = passwordBox.Password;

                            // Validate the entered password
                            if (enteredPassword != "Cook ro cook")
                            {
                                MessageBox.Show("Password is worg");
                                return;
                            }
                            else
                            {
                                guna2DataGridView1.Rows.RemoveAt(e.RowIndex);

                                GetTotal();
                            }

                        }
                    }
                    // Remove the row



                }

        }

        private void btnKitchen_Click(object sender, EventArgs e)
        {
            FormKitchenView kit = new FormKitchenView();
            kit.ShowDialog();
        }
        private void btnBill_Click(object sender, EventArgs e)
        {
            FormPaidBill pb = new FormPaidBill();
            pb.ShowDialog();
        }
    }
}
