using Restaurant.Reportss;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Restaurant.View
{
    public partial class FormReports : Form
    {
        public FormReports()
        {
            InitializeComponent();
            if (MainClass.USER == "admin")
            {
                guna2Button2.Visible = true;
            }
            else
            {
                guna2Button2.Visible = false;
            }
        }

        private void btnMenu_Click(object sender, EventArgs e)
        {
            string qry = @"select * from Product ";

            OleDbCommand cmd = new OleDbCommand(qry, MainClass.con);
            MainClass.con.Open();
            DataTable dt = new DataTable();
            OleDbDataAdapter da = new OleDbDataAdapter(cmd);
            da.Fill(dt);
            MainClass.con.Close();

            FormPrint frm = new FormPrint();
            rptMenu cr = new rptMenu();

            //cr.SetDatabaseLogon("sa", "123");
            cr.SetDataSource(dt);
            frm.crystalReportViewer1.ReportSource = cr;
            frm.crystalReportViewer1.Refresh();
            frm.Show();
        }

        private void btnStaff_Click(object sender, EventArgs e)
        {
            string qry = @"select * from Staff";
            OleDbCommand cmd = new OleDbCommand(qry, MainClass.con);
            MainClass.con.Open();
            DataTable dt = new DataTable();
            OleDbDataAdapter adpt = new OleDbDataAdapter(cmd);
            adpt.Fill(dt);

            MainClass.con.Close();
            FormPrint frm = new FormPrint();
            rptStaff cr = new rptStaff();

            //cr.SetDatabaseLogon("sa", "123");
            cr.SetDataSource(dt);
            frm.crystalReportViewer1.ReportSource = cr;
            frm.crystalReportViewer1.Refresh();
            frm.Show();
        }

        private void btnCategory_Click(object sender, EventArgs e)
        {
            FormSaleByCategory frm = new FormSaleByCategory();
            frm.ShowDialog();
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            FormCashLedger cl = new FormCashLedger();
            cl.ShowDialog();

        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {

            // Show a confirmation dialog with "Yes" or "No"
            DialogResult result = MessageBox.Show("Are you sure you want to delete data before today?",
                                                  "Confirm Deletion",
                                                  MessageBoxButtons.YesNo,
                                                  MessageBoxIcon.Warning);

            // Check if the user clicked "Yes"
            if (result == DialogResult.Yes)
            {
                try
                {
                    // Your deletion queries
                    string deleteQuery1 = "DELETE FROM tblMain";
                    string deleteQuery2 = "DELETE FROM tblDetail";
                    string deleteQuery3 = "DELETE FROM tblExpence";
                    string deleteQuery4 = "DELETE FROM tblMainReturn";
                    string deleteQuery5 = "DELETE FROM tblDetailReturn";
                    string deleteQuery6 = "DELETE FROM TblCashIn";
                    string deleteQuery7 = "DELETE FROM tblOrderLog";

                    // Get today's date
                    DateTime today = DateTime.Today.AddDays(-2);


                    // Execute the deletion for each table
                    using (OleDbCommand cmd1 = new OleDbCommand(deleteQuery1, MainClass.con))
                    using (OleDbCommand cmd2 = new OleDbCommand(deleteQuery2, MainClass.con))
                    using (OleDbCommand cmd3 = new OleDbCommand(deleteQuery3, MainClass.con))
                    using (OleDbCommand cmd4 = new OleDbCommand(deleteQuery4, MainClass.con))
                    using (OleDbCommand cmd5 = new OleDbCommand(deleteQuery5, MainClass.con))
                    using (OleDbCommand cmd6 = new OleDbCommand(deleteQuery6, MainClass.con))
                    using (OleDbCommand cmd7 = new OleDbCommand(deleteQuery7, MainClass.con))
                    {
                        
                        // Open the connection
                        if (MainClass.con.State == ConnectionState.Closed)
                            MainClass.con.Open();

                        // Execute the queries
                        cmd1.ExecuteNonQuery();
                        cmd2.ExecuteNonQuery();
                        cmd3.ExecuteNonQuery();
                        cmd4.ExecuteNonQuery();
                        cmd5.ExecuteNonQuery();
                        cmd6.ExecuteNonQuery();
                        cmd7.ExecuteNonQuery();


                        // Success message
                        MessageBox.Show("Data before today has been successfully deleted.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    // Handle any exceptions
                    MessageBox.Show("An error occurred while deleting the data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    // Close the connection
                    if (MainClass.con.State == ConnectionState.Open)
                        MainClass.con.Close();
                }
            }
        }

        private void guna2Button3_Click(object sender, EventArgs e)
        {
            // Define the query to fetch data
            string qry = @"select 
    m.MainID, 
    m.Date, 
    m.Time, 
    p.ProductName, 
    d.qty as 'Qty', 
    p.ProductPrice as 'Price', 
    (d.qty * p.ProductPrice) as 'Amount',
    SUM(d.qty * p.ProductPrice) OVER () as 'TotalAmount' -- Grand Total
from 
    TblMain m
inner join 
    tblOrderLog d on m.MainID = d.MainID
inner join 
    Product p on p.ProductID = d.itemid
inner join 
    Category c on c.CategoryID = p.CategoryID
where 
    d.Isdeleted = 1
group by 
    m.MainID, 
    m.Date, 
    m.Time, 
    p.ProductName, 
    d.qty, 
    p.ProductPrice";

            // Create a OleDbCommand object
            OleDbCommand cmd = new OleDbCommand(qry, MainClass.con);

            MainClass.con.Open();

            // Create a new DataSet (assuming you have a dataset defined in your project)
            DSDeleteBill ds = new DSDeleteBill();

            // Use OleDbDataAdapter to fill the dataset
            OleDbDataAdapter adpt = new OleDbDataAdapter(cmd);
            adpt.Fill(ds, "DtDeleteOrSaleReport"); 

            MainClass.con.Close();

            // Create an instance of your Crystal Report
            rptDeleteItem cr = new rptDeleteItem();

            // Set the dataset as the data source for the report
            cr.SetDataSource(ds.Tables["DtDeleteOrSaleReport"]);

            // Load the report in the CrystalReportViewer
            FormPrint frm = new FormPrint();
            frm.crystalReportViewer1.ReportSource = cr;
            frm.crystalReportViewer1.Refresh();
            frm.Show();


        }
    }
}
