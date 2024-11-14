using Guna.UI2.WinForms;
using Restaurant.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Restaurant.View
{
    public partial class FormProductView : SampleView
    {
        public FormProductView(string btnName)
        {
            InitializeComponent();
            label2.Text = btnName;
        }

        public void GetData()
        {
            string qry = "SELECT p.ProductId, p.ProductName, c.CategoryName, p.ProductPrice,p.CategoryID " +
                         "FROM Product p " +
                         "INNER JOIN Category c ON c.CategoryID = p.CategoryID " +
                         "WHERE p.ProductName LIKE @ProductName order by 1 desc";

            using (SqlCommand cmd = new SqlCommand(qry, MainClass.con)) // Initialize SqlCommand
            {
                cmd.Parameters.AddWithValue("@ProductName", "%" + txtSearch.Text + "%");

                // Open connection
                MainClass.con.Open();

                using (SqlDataReader dr = cmd.ExecuteReader()) // Execute query
                {
                    guna2DataGridView1.Rows.Clear(); // Clear previous data

                    while (dr.Read())
                    {
                        guna2DataGridView1.Rows.Add(
                            dr["ProductId"].ToString(),    // Column 1
                            dr["ProductName"].ToString(),  // Column 2
                            dr["CategoryName"].ToString(), // Column 3
                            dr["ProductPrice"].ToString(),  // Column 4
                            dr["CategoryID"].ToString()  // Column 4
                        );
                    }
                }

                MainClass.con.Close(); // Close SqlConnection
            }

            // Ensure that the columns are added if not present
            if (guna2DataGridView1.Columns.Count == 0)
            {
                guna2DataGridView1.Columns.Add("ProductId", "Product ID");
                guna2DataGridView1.Columns.Add("ProductName", "Product Name");
                guna2DataGridView1.Columns.Add("CategoryName", "Category Name");
                guna2DataGridView1.Columns.Add("ProductPrice", "Product Price");
            }

            //// Set column headers
            //guna2DataGridView1.Columns["ProductId"].HeaderText = "Product ID";
            //guna2DataGridView1.Columns["ProductName"].HeaderText = "Product Name";
            //guna2DataGridView1.Columns["CategoryName"].HeaderText = "Category Name";
            //guna2DataGridView1.Columns["ProductPrice"].HeaderText = "Product Price";

            //// Set column widths
            //guna2DataGridView1.Columns["ProductId"].Width = 100;
            //guna2DataGridView1.Columns["ProductName"].Width = 200;
            //guna2DataGridView1.Columns["CategoryName"].Width = 150;
            //guna2DataGridView1.Columns["ProductPrice"].Width = 100;
        }


        private void FormProductView_Load(object sender, EventArgs e)
        {
            GetData();
        }
        public override void txtSearch_TextChanged(object sender, EventArgs e)
        {
            GetData();
        }

        private void btnAdd_Click_1(object sender, EventArgs e)
        {
            //adding blur effect
            MainClass.BlurBackground(new FormProductAdd(0));

            //FormCategoryAdd frm = new FormCategoryAdd();
            //frm.ShowDialog();
            GetData();
        }

        private void guna2DataGridView1_CellClick_1(object sender, DataGridViewCellEventArgs e)
        {

            if (guna2DataGridView1.CurrentCell.OwningColumn.Name == "dvgEdit")
            {
                FormProductAdd frm = new FormProductAdd(Convert.ToInt16(guna2DataGridView1.CurrentRow.Cells["dvgCategory"].Value));
                frm.id = Convert.ToInt16(guna2DataGridView1.CurrentRow.Cells["dvgid"].Value);
                

                MainClass.BlurBackground(frm);
                
                GetData();
            }
            if (guna2DataGridView1.CurrentCell.OwningColumn.Name == "dvgDelete")
            {
                // Need to Cinform Delete

                guna2MessageDialog1.Icon = Guna.UI2.WinForms.MessageDialogIcon.Question;
                guna2MessageDialog1.Buttons = Guna.UI2.WinForms.MessageDialogButtons.YesNo;

                if (guna2MessageDialog1.Show("You are sure you want to delete ") == DialogResult.Yes)
                {
                    int id = Convert.ToInt16(guna2DataGridView1.CurrentRow.Cells["dvgid"].Value);
                    string query = "Delete from Product where ProductID = '" + id + "' ";
                    Hashtable ht = new Hashtable();
                    MainClass.SQL(query, ht);

                    guna2MessageDialog1.Icon = Guna.UI2.WinForms.MessageDialogIcon.Information;
                    guna2MessageDialog1.Buttons = Guna.UI2.WinForms.MessageDialogButtons.OK;

                    guna2MessageDialog1.Show("Detete is Succesfull");
                    GetData();
                }

            }
        }
    }

}
