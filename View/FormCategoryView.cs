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
using Guna.UI2.WinForms;
using Restaurant.Model;

namespace Restaurant.View
{
    public partial class FormCategoryView : SampleView
    {
        public FormCategoryView(string btnText)
        {
            InitializeComponent();
            label2.Text = btnText;
        }
        public void GetData()
        {

            string qry = "SELECT * FROM Category WHERE CategoryName LIKE '%" + txtSearch.Text + "%' order by 1 desc";

            SqlCommand cmd = new SqlCommand(qry, MainClass.con); // Initialize SqlCommand

            cmd.Parameters.AddWithValue("@CategoryName", "%" + txtSearch.Text + "%");

            // Open connection
            MainClass.con.Open();

            SqlDataReader dr = cmd.ExecuteReader(); // Execute query

            guna2DataGridView1.Rows.Clear(); // Clear previous data

            while (dr.Read())
            {
                guna2DataGridView1.Rows.Add(
                    dr["CategoryID"].ToString(), // Column 1
                    dr["CategoryName"].ToString() // Column 2
                );
            }

            dr.Close(); // Close SqlDataReader
            MainClass.con.Close(); // Close SqlConnection
        }


        private void FormCategoryView_Load(object sender, EventArgs e)
        {
            GetData();
        }
        public override void txtSearch_TextChanged(object sender, EventArgs e)
        {
            GetData();
        }

        public virtual void btnAdd_Click_1(object sender, EventArgs e)
        {
            //adding blur effect
            MainClass.BlurBackground(new FormCategoryAdd());

            //FormCategoryAdd frm = new FormCategoryAdd();
            //frm.ShowDialog();
            GetData();
        }

        private void guna2DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (guna2DataGridView1.CurrentCell.OwningColumn.Name == "dvgEdit")
            {
                FormCategoryAdd frm = new FormCategoryAdd();
                frm.id = Convert.ToInt16(guna2DataGridView1.CurrentRow.Cells["dvgid"].Value);
                frm.txtName.Text = Convert.ToString(guna2DataGridView1.CurrentRow.Cells["dvgName"].Value);
                MainClass.BlurBackground(frm);
                GetData();
            }
            if (guna2DataGridView1.CurrentCell.OwningColumn.Name == "dvgDelete")
            {
                // Need to Cinform Delete

                DialogResult result = MessageBox.Show(
                        "Are you sure you want to delete this record ?",  // Message text
                        "Confirmation",              // Title text
                        MessageBoxButtons.YesNo,    // Buttons to display
                        MessageBoxIcon.Question);   // Icon to display

                if (result == DialogResult.Yes)
                {
                    int id = Convert.ToInt16(guna2DataGridView1.CurrentRow.Cells["dvgid"].Value);
                    string query = "Delete from Category where CategoryID = '" + id + "' ";
                    Hashtable ht = new Hashtable();
                    MainClass.SQL(query, ht);


                    MessageBox.Show("Detete is Succesfull");
                    GetData();
                }

            }
        }
    }
}
