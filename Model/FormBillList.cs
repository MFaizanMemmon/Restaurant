using CrystalDecisions.CrystalReports.Engine;
using Restaurant.Reportss;
using Restaurant.View;
using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Windows.Forms;

namespace Restaurant.Model
{
    public partial class FormBillList : SampleAdd
    {
        public FormBillList()
        {
            InitializeComponent();
        }
        public int MainID = 0;

        private void FormBillList_Load(object sender, EventArgs e)
        {
            LoadData();



        }

        private void LoadData()
        {
            string qry = @"select MainID, TableName, WaiterName,CustName, OrderType, Status, Total from TblMain Where Status not in  ('Pending','Paid') order by Time desc ";
            ListBox lb = new ListBox();
            lb.Items.Add(dvgid);
            lb.Items.Add(dvgTable);
            lb.Items.Add(dvgWaiter);
            lb.Items.Add(dgvCusName);
            lb.Items.Add(dvgType);
            lb.Items.Add(dvgStatus);
            lb.Items.Add(dvgTotal);
            lb.Items.Add(dvgEdit);


            MainClass.LoadData(qry, guna2DataGridView1, lb);
            CalculateTotal();

        }
        private void CalculateTotal()
        {
            double total = 0.0;
            // Assuming the column name is "Amount"
            string columnName = "dvgTotal";

            // Ensure that the column exists
            if (guna2DataGridView1.Columns.Contains(columnName))
            {
                // Loop through each row in the DataGridView
                foreach (DataGridViewRow row in guna2DataGridView1.Rows)
                {
                    // Make sure the row is not a new row
                    if (!row.IsNewRow)
                    {
                        // Get the value from the column "Amount"
                        object value = row.Cells[columnName].Value;

                        // Check if the value is not null and can be converted to double
                        if (value != null && double.TryParse(value.ToString(), out double cellValue))
                        {
                            // Add the value to the total
                            total += cellValue;
                        }
                    }
                }

                // Output the total to a label or other control
                lblTotal.Text = "Total: " + total.ToString("C");
            }
            else
            {
                MessageBox.Show("Column not found: " + columnName);
            }
        }






        private void guna2DataGridView1_CellClick_1(object sender, DataGridViewCellEventArgs e)
        {

            if (guna2DataGridView1.CurrentCell.OwningColumn.Name == "dvgEdit")
            {
                MainID = Convert.ToInt32(guna2DataGridView1.CurrentRow.Cells["dvgid"].Value);
                this.Close();

                //if (MainClass.USER.ToLower() == "admin")
                //{
                //    this.Close();

                //}
                //else
                //{
                //    bool isPrinted = CheckIfPrinted(MainID);


                //    if (isPrinted)
                //    {
                //        MainID = 0;
                //        MessageBox.Show("You can't Edit bcz Unpaid bill is print...");
                //        return;

                //    }
                //    else
                //    {
                //        this.Close();

                //    }
                //}


            }
            else if (guna2DataGridView1.CurrentCell.OwningColumn.Name == "dgvPrint")
            {
                int mainId = Convert.ToInt32(guna2DataGridView1.CurrentRow.Cells["dvgid"].Value);

                try
                {
                    if (MainClass.USER.ToLower() == "admin")
                    {
                        // Admin can print unlimited times
                        OrderPrint(mainId);

                        MessageBox.Show("Printed successfully.");
                    }
                    else
                    {
                        // Regular user - check if already printed
                        bool isPrinted = CheckIfPrintedOrder(mainId);

                        if (isPrinted)
                        {
                            MessageBox.Show("This record has already been printed and cannot be printed again.");
                            return;
                        }
                        else
                        {
                            // Proceed with printing
                            OrderPrint(mainId);
                            UpdatePrintStatusOrder(mainId); // Update the print status to 'printed'
                            MessageBox.Show("Printed successfully.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }



            }


            else if (guna2DataGridView1.CurrentCell.OwningColumn.Name == "dvgUnPaid")
            {
                int mainID = Convert.ToInt32(guna2DataGridView1.CurrentRow.Cells["dvgid"].Value);

                try
                {
                    if (MainClass.USER.ToLower() == "admin")
                    {
                        // Admin can print unlimited times
                        PrintBill(mainID);
                        MessageBox.Show("Printed successfully.");
                    }
                    else
                    {
                        // Regular user - check if already printed
                        bool isPrinted = CheckIfPrinted(mainID);

                        if (isPrinted)
                        {
                            MessageBox.Show("This record has already been printed and cannot be printed again.");
                        }
                        else
                        {
                            // Proceed with printing
                            PrintBill(mainID);
                            UpdatePrintStatus(mainID); // Update the print status to 'printed'
                            //MessageBox.Show("Printed successfully.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
                LoadData();

            }
            else
            {
                FormKitchenView kv = new FormKitchenView();
                kv.invoiceId = Convert.ToInt32(guna2DataGridView1.CurrentRow.Cells["dvgid"].Value);
                kv.ShowDialog();
            }
        }

        private void PrintBill(int id)
        {
            string query = "usp_GetBill"; // Stored procedure name

            // Initialize SqlConnection and SqlCommand
            using (SqlConnection con = new SqlConnection(MainClass.con.ConnectionString))
            {
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@InvoiceId", id); // Ensure id has a valid value

                // Open the connection
                con.Open();

                // Create DataSet and fill it
                DSBill billDataSet = new DSBill(); // Ensure DSBill is your DataSet defined in the .xsd file
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(billDataSet, "BillDT");

                // Close the connection
                con.Close();

                // Load the Crystal Report and set the data source
                ReportDocument reportDocument = new ReportDocument();

                // Set up dynamic path to the Crystal Report file
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string reportFolder = Path.GetFullPath(Path.Combine(baseDirectory, @"..\..\"));
                string reportPath = Path.Combine(reportFolder, @"Reportss\rptBill.rpt");

                reportDocument.Load(reportPath);

                // Set the DataSource of the report to the DataTable in the DataSet
                reportDocument.SetDataSource(billDataSet.Tables["BillDT"]);

                // Set up the thermal printer settings
                PrinterSettings printerSettings = new PrinterSettings();
                // Optionally specify the printer's name if needed
                // printerSettings.PrinterName = "Your Thermal Printer Name"; 

                PageSettings pageSettings = new PageSettings();
                // Set the paper size to match the thermal printer (example: 80mm width)
                pageSettings.PaperSize = new PaperSize("Thermal", 350, 700); // Adjust based on your printer's specifications

                pageSettings.Margins = new Margins(0, 0, 0, 0);
                // Configure the print options
                reportDocument.PrintOptions.PrinterName = printerSettings.PrinterName; // Specify printer if needed
                reportDocument.PrintOptions.PaperSize = (CrystalDecisions.Shared.PaperSize)pageSettings.PaperSize.RawKind;

                // Print the report directly
                // 'false' here means the report won't be sent to the print preview but will go directly to the printer
                reportDocument.PrintToPrinter(printerSettings, pageSettings, false);
            }
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

        private void UpdatePrintStatus(int id)
        {
            string query = "UPDATE TblMain SET IsPrintUnPaid = 1 WHERE MainID = @ID";

            try
            {
                using (SqlConnection con = new SqlConnection(MainClass.con.ConnectionString))
                {
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@ID", id);

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



        private bool CheckIfPrintedOrder(int id)
        {
            string query = "SELECT IsNull(IsOrderPrint,0) FROM TblMain WHERE MainID = @ID";

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
                        //reportDocument.PrintOptions.PaperSize = (CrystalDecisions.Shared.PaperSize)pageSettings.PaperSize.RawKind;

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
                       // reportDocument.PrintOptions.PaperSize = (CrystalDecisions.Shared.PaperSize)pageSettings.PaperSize.RawKind;

                        // Print the report only if data exists
                        reportDocument.PrintToPrinter(printerSettings, pageSettings, false);
                    }

                }

            }
            //}
        }

    }
}
