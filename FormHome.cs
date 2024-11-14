using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Restaurant
{
    public partial class FormHome : Form
    {
        public FormHome()
        {
            InitializeComponent();
        }

        private void FormHome_Load(object sender, EventArgs e)
        {
            UserLoad();
            StaffLoad();
            CategoryLoad();
            ProductLoad();
            LoadCashTotal();
        }

        public void UserLoad()
        {
            int rowCount = 0;

            string query = "SELECT COUNT(*) FROM Users";
            SqlCommand cmd = new SqlCommand(query, MainClass.con);

            MainClass.con.Open();
            rowCount = (int)cmd.ExecuteScalar();
            MainClass.con.Close();

            // Displaying rowCount in the label
            lblUsers.Text = rowCount.ToString();

            
        }

        public void StaffLoad()
        {
            int rowCount = 0;

            string query = "SELECT COUNT(*) FROM Staff";
            SqlCommand cmd = new SqlCommand(query, MainClass.con);

            MainClass.con.Open();
            rowCount = (int)cmd.ExecuteScalar();
            MainClass.con.Close();

            // Displaying rowCount in the label
            lblStaffs.Text =  rowCount.ToString();

            
        }

        public void CategoryLoad()
        {
            int rowCount = 0;

            string query = "SELECT COUNT(*) FROM Category";
            SqlCommand cmd = new SqlCommand(query, MainClass.con);

            MainClass.con.Open();
            rowCount = (int)cmd.ExecuteScalar();
            MainClass.con.Close();

            // Displaying rowCount in the label
            llblCategorys.Text = rowCount.ToString();

            
        }

        // Controller Action
        public void ProductLoad()
        {
            int rowCount = 0;

            string query = "SELECT COUNT(*) FROM Product";
            SqlCommand cmd = new SqlCommand(query, MainClass.con);

            MainClass.con.Open();
            rowCount = (int)cmd.ExecuteScalar();
            MainClass.con.Close();

            // Displaying rowCount in the label
            llblProducts.Text = rowCount.ToString();
        }

        private void LoadCashTotal()
        {
            string qry = "SELECT SUM(TotalAmount) AS Amount FROM (SELECT SUM(Total) AS TotalAmount FROM tblMain where Status = 'Paid'  UNION ALL SELECT -SUM(Amount) AS TotalAmount FROM TblExpence UNION ALL SELECT SUM(Total) AS TotalAmount FROM tblMainReturn UNION ALL SELECT SUM(Amount) AS TotalAmount FROM tblCashIN) AS CombinedTotals;";

            // Initialize SQL command
            using (SqlCommand cmd = new SqlCommand(qry, MainClass.con))
            {
                try
                {
                    // Open connection
                    MainClass.con.Open();

                    // Execute the query and get the result
                    object result = cmd.ExecuteScalar();

                    // Check for null result and convert to string
                    if (result != null)
                    {
                        // Set the result to lblCash
                        lblCash.Text = result.ToString();
                    }
                    else
                    {
                        // Set lblCash to zero if no result
                        lblCash.Text = "0";
                    }
                }
                catch (Exception ex)
                {
                    // Handle potential exceptions
                    MessageBox.Show("Error: " + ex.Message);
                }
                finally
                {
                    // Ensure the connection is closed
                    MainClass.con.Close();
                }
            }
        }


    }
}
