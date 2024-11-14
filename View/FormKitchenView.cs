using CrystalDecisions.CrystalReports.Engine;
using Restaurant.Reportss;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Restaurant.View
{
    public partial class FormKitchenView : SampleAdd
    {
        public int invoiceId { get; set; }
        public FormKitchenView()
        {
            InitializeComponent();


        }

        private void FormKitchenView_Load(object sender, EventArgs e)
        {
            string connectionString = MainClass.con.ConnectionString;
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
            p.ProductPrice * d.Qty AS 'TotalPrice',
            m.Total,
            m.Recieved AS 'Received',
            m.Change AS 'Changed',
            d.ordercount
        FROM
            TblMain m
            INNER JOIN tblOrderLog d ON m.MainID = d.MainID
            INNER JOIN Product p ON p.ProductID = d.itemid
        WHERE
            m.MainID = @InvoiceId
        ORDER BY
            d.ordercount DESC";

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@InvoiceId", invoiceId);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    guna2DataGridView1.DataSource = dataTable;

                    // Apply row coloring
                    ApplyRowColoring();
                }
            }
        }
        private List<Color> colors = new List<Color>
{
    Color.LightYellow,
    Color.LightGreen,
    Color.LightBlue,
    Color.LightCoral,
    Color.LightPink,
    Color.LightSalmon,
    // Add more colors as needed
};

        private void ApplyRowColoring()
        {
            // Create a dictionary to track which rows should be colored
            var itemColors = new Dictionary<string, Color>();
            var itemRows = new Dictionary<string, List<DataGridViewRow>>();
            int colorIndex = 0;

            // Iterate over rows to group by item value
            foreach (DataGridViewRow row in guna2DataGridView1.Rows)
            {
                if (!row.IsNewRow)
                {
                    string itemName = row.Cells["ProductName"].Value.ToString();

                    if (!itemColors.ContainsKey(itemName))
                    {
                        // Assign a color to the item
                        if (colorIndex >= colors.Count)
                        {
                            colorIndex = 0; // Reset to use the first color again if there are more products than colors
                        }
                        itemColors[itemName] = colors[colorIndex++];
                    }

                    if (!itemRows.ContainsKey(itemName))
                    {
                        itemRows[itemName] = new List<DataGridViewRow>();
                    }

                    itemRows[itemName].Add(row);
                }
            }

            // Iterate over the grouped rows and apply color
            foreach (var itemGroup in itemRows)
            {
                Color rowColor = itemColors[itemGroup.Key];
                foreach (var row in itemGroup.Value)
                {
                    row.DefaultCellStyle.BackColor = rowColor;
                }
            }
        }


        //private void InitializeTableLayoutPanel()
        //{
        //    tableLayoutPanel1.ColumnCount = 3; // 3 columns
        //    tableLayoutPanel1.AutoScroll = true;
        //    tableLayoutPanel1.Dock = DockStyle.Fill;

        //    // Configure row styles for dynamic height
        //    for (int i = 0; i < 3; i++)
        //    {
        //        tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        //    }
        //}

        //private void LoadOrders()
        //{
        //    tableLayoutPanel1.Controls.Clear();
        //    tableLayoutPanel1.RowCount = 0;

        //    DataTable orders = FetchPendingOrders();

        //    int cardCount = 0;
        //    int rowCount = 0;

        //    foreach (DataRow order in orders.Rows)
        //    {
        //        if (cardCount % 3 == 0)
        //        {
        //            tableLayoutPanel1.RowCount++;
        //        }

        //        int mainID = Convert.ToInt32(order["MainID"]);
        //        FlowLayoutPanel orderPanel = CreateOrderPanel();

        //        // Create and add panels in the correct order
        //        var headerPanel = CreateHeaderPanel(order);
        //        var detailPanel = CreateProductPanel(FetchOrderProducts(mainID));
        //        var buttonPanel = CreateButtonPanel(mainID);

        //        // Add the panels to the orderPanel
        //        orderPanel.Controls.Add(headerPanel);
        //        orderPanel.Controls.Add(CreateSpacer()); // Spacer after header
        //        orderPanel.Controls.Add(detailPanel);
        //        orderPanel.Controls.Add(CreateSpacer()); // Spacer before button
        //        orderPanel.Controls.Add(buttonPanel);

        //        tableLayoutPanel1.Controls.Add(orderPanel, cardCount % 3, rowCount);

        //        cardCount++;
        //        if (cardCount % 3 == 0)
        //        {
        //            rowCount++;
        //        }
        //    }

        //    tableLayoutPanel1.PerformLayout();
        //}

        private DataTable FetchPendingOrders()
        {
            string query = @"
                SELECT * 
                FROM TblMain 
                WHERE Status = 'Complete' 
                ORDER BY 1 DESC";

            using (SqlDataAdapter adapter = new SqlDataAdapter(query, MainClass.con))
            {
                DataTable table = new DataTable();
                adapter.Fill(table);
                return table;
            }
        }

        private DataTable FetchOrderProducts(int mainID)
        {
            string query = @"
                SELECT * 
                FROM TblMain m 
                INNER JOIN TblDetail d ON m.MainID = d.MainID 
                INNER JOIN Product p ON p.ProductID = d.ProID 
                WHERE m.MainID = @MainID";

            using (SqlDataAdapter adapter = new SqlDataAdapter(query, MainClass.con))
            {
                adapter.SelectCommand.Parameters.AddWithValue("@MainID", mainID);
                DataTable table = new DataTable();
                adapter.Fill(table);
                return table;
            }
        }

        private FlowLayoutPanel CreateOrderPanel()
        {
            return new FlowLayoutPanel
            {
                AutoSize = true,
                Width = 250,
                FlowDirection = FlowDirection.TopDown,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(10),
                Padding = new Padding(5),
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
        }

        private FlowLayoutPanel CreateHeaderPanel(DataRow order)
        {
            var headerPanel = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.TopDown,
                BackColor = Color.FromArgb(50, 55, 89),
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(5)
            };

            AddHeaderLabel(headerPanel, "Order No: ", order["MainID"].ToString());
            AddHeaderLabel(headerPanel, "Tables: ", order["TableName"].ToString());
            AddHeaderLabel(headerPanel, "Waiter Name: ", order["WaiterName"].ToString());
            AddHeaderLabel(headerPanel, "Order Time: ", order["Time"].ToString());
            AddHeaderLabel(headerPanel, "Order Type: ", order["OrderType"].ToString());

            return headerPanel;
        }

        private void AddHeaderLabel(FlowLayoutPanel panel, string labelText, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                panel.Controls.Add(new Label
                {
                    ForeColor = Color.White,
                    Font = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold),
                    Text = $"{labelText}{value}",
                    AutoSize = true,
                    Margin = new Padding(5)
                });
            }
        }

        private FlowLayoutPanel CreateProductPanel(DataTable products)
        {
            var productPanel = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.TopDown,
                Margin = new Padding(0, 10, 0, 0),
                Padding = new Padding(5),
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            productPanel.Controls.Add(new Label
            {
                Text = "Products",
                Font = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold),
                ForeColor = Color.Black,
                AutoSize = true,
                Margin = new Padding(5, 0, 5, 0)
            });

            foreach (DataRow product in products.Rows)
            {
                productPanel.Controls.Add(new Label
                {
                    Text = $"{product["ProductName"]} - Qty {product["Qty"]}",
                    ForeColor = Color.Black,
                    Margin = new Padding(10, 5, 3, 0),
                    AutoSize = true
                });
            }

            return productPanel;
        }

        private FlowLayoutPanel CreateButtonPanel(int mainID)
        {
            var buttonPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                Margin = new Padding(10, 5, 3, 10)
            };

            var btnPrint = new Guna.UI2.WinForms.Guna2Button
            {
                AutoRoundedCorners = true,
                Size = new Size(100, 35),
                FillColor = Color.FromArgb(85, 126, 241),
                Text = "Print",
                Tag = mainID
            };
            btnPrint.Click += BtnPrint_Click;

            buttonPanel.Controls.Add(btnPrint);
            return buttonPanel;
        }

        private FlowLayoutPanel CreateSpacer()
        {
            return new FlowLayoutPanel
            {
                Height = 10, // Adjust this value as needed
                Width = 0,
                Margin = new Padding(0, 5, 0, 5) // Spacing between sections
            };
        }

        private void BtnPrint_Click(object sender, EventArgs e)
        {
            if (sender is Guna.UI2.WinForms.Guna2Button button)
            {
                int mainID = Convert.ToInt32(button.Tag);
                var reportDocument = new ReportDocument();
                string reportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\Reportss\crptOrderReports.rpt");

                using (SqlConnection con = new SqlConnection(MainClass.con.ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("usp_GetBill", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@InvoiceId", mainID);

                        var billDataSet = new DSBill();
                        var adapter = new SqlDataAdapter(cmd);
                        adapter.Fill(billDataSet, "BillDT");

                        reportDocument.Load(reportPath);
                        reportDocument.SetDataSource(billDataSet.Tables["BillDT"]);

                        FormPrint frm = new FormPrint();
                        frm.crystalReportViewer1.ReportSource = reportDocument;
                        frm.crystalReportViewer1.Refresh();
                        frm.Show();
                    }
                }
            }
        }

        private void btnexit_Click(object sender, EventArgs e)
        {
            Close();
        }

        // yha se new code hoga

        private DataTable GetOrderDetails(int mainId)
        {
            string query = @"
        SELECT 
            m.mainid, 
            m.Date, 
            m.TableName, 
            m.WaiterName, 
            m.Total, 
            m.Status, 
            o.itemid, 
            p.ProductName, 
            o.qty AS TotalQuantity,
            o.ordercount
        FROM 
            tblMain m
        INNER JOIN 
            tblOrderLog o ON m.mainid = o.mainid
        INNER JOIN
            Product p ON o.itemid = p.ProductID
        WHERE 
            m.Status = 'Complete' 
            AND m.MainID = @MainID 
            AND o.isdeleted = 0";

            DataTable dt = new DataTable();

            using (SqlCommand cmd = new SqlCommand(query, MainClass.con))
            {
                cmd.Parameters.AddWithValue("@MainID", mainId);

                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    try
                    {
                        if (MainClass.con.State == ConnectionState.Closed)
                            MainClass.con.Open();

                        adapter.Fill(dt);
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

            return dt;
        }

        private void CreateCards(DataTable dt)
        {
            // Clear existing controls if needed
            flowLayoutPanel1.Controls.Clear();

            // Group data by mainid
            var groupedData = dt.AsEnumerable()
                                .GroupBy(row => row.Field<int>("mainid"));

            foreach (var group in groupedData)
            {
                // Create a header card for each mainid
                Panel mainCardPanel = new Panel
                {
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.LightGray,
                    Size = new Size(400, 300),
                    Margin = new Padding(10)
                };

                // Safely retrieve and cast values from the group
                var firstRow = group.First();
                string dateStr = Convert.ToDateTime(firstRow["Date"]).ToString("yyyy-MM-dd");
                string tableName = Convert.ToString(firstRow["TableName"]);
                string waiterName = Convert.ToString(firstRow["WaiterName"]);
                decimal total = Convert.ToDecimal(firstRow["Total"]);

                Label mainCardHeader = new Label
                {
                    Text = $"Order ID: {group.Key}\nDate: {dateStr}\n" +
                           $"Table: {tableName}\n" +
                           $"Waiter: {waiterName}\n" +
                           $"Total: {total:C}",
                    Font = new Font("Arial", 12, FontStyle.Bold),
                    Dock = DockStyle.Top,
                    BackColor = Color.DarkGray,
                    Padding = new Padding(10),
                    Height = 80
                };

                mainCardPanel.Controls.Add(mainCardHeader);

                // Add product details under each mainid
                var itemGroups = group.GroupBy(row => row.Field<int>("itemid"));

                foreach (var itemGroup in itemGroups)
                {
                    Panel itemCardPanel = new Panel
                    {
                        BorderStyle = BorderStyle.FixedSingle,
                        BackColor = Color.White,
                        Size = new Size(380, 120),
                        Margin = new Padding(5)
                    };

                    var itemRow = itemGroup.First();
                    string productName = Convert.ToString(itemRow["ProductName"]);
                    int totalQuantity = itemGroup.Sum(row => Convert.ToInt32(row["TotalQuantity"]));
                    int totalOrders = itemGroup.Sum(row => Convert.ToInt32(row["ordercount"]));

                    Label itemCardHeader = new Label
                    {
                        Text = $"Product: {productName}",
                        Font = new Font("Arial", 10, FontStyle.Bold),
                        Dock = DockStyle.Top,
                        BackColor = Color.LightGray,
                        Padding = new Padding(10),
                        Height = 30
                    };

                    Label itemCardDetails = new Label
                    {
                        Text = $"Quantity: {totalQuantity}\n" +
                               $"Order Count: {totalOrders}",
                        Font = new Font("Arial", 9),
                        Dock = DockStyle.Fill,
                        Padding = new Padding(10)
                    };

                    itemCardPanel.Controls.Add(itemCardDetails);
                    itemCardPanel.Controls.Add(itemCardHeader);

                    mainCardPanel.Controls.Add(itemCardPanel);
                }

                flowLayoutPanel1.Controls.Add(mainCardPanel);
            }
        }




    }
}
