using Restaurant.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using System.Data.SqlClient;

namespace Restaurant.View
{
    public partial class FormVenderView : SampleView
    {
        public FormVenderView(string btnName)
        {
            InitializeComponent();
            label2.Text = btnName;
        }

        private void btnAdd_Click_1(object sender, EventArgs e)
        {
            MainClass.BlurBackground(new FormVenderAdd());
            GetData();

        }


        public void GetData()
        {
            string qry = "SELECT * FROM TblVender WHERE Name LIKE @Name order by 1 desc";

            using (SqlCommand cmd = new SqlCommand(qry, MainClass.con)) // Initialize SqlCommand
            {
                cmd.Parameters.AddWithValue("@Name", "%" + txtSearch.Text + "%");

                // Open connection
                MainClass.con.Open();

                using (SqlDataReader dr = cmd.ExecuteReader()) // Execute query
                {
                    guna2DataGridView1.Rows.Clear(); // Clear previous data

                    while (dr.Read())
                    {
                        guna2DataGridView1.Rows.Add(
                            dr["VenderID"].ToString(),
                            dr["Name"].ToString(),
                            dr["PhoneNo"].ToString(),
                            dr["Address"].ToString(),
                            dr["OpeningBalance"].ToString(),
                            dr["Description"].ToString()
                        );
                    }
                }

                MainClass.con.Close(); // Close SqlConnection
            }

            // Set column headers
            guna2DataGridView1.Columns[0].HeaderText = "Vender ID";
            guna2DataGridView1.Columns[1].HeaderText = "Name";
            guna2DataGridView1.Columns[2].HeaderText = "Phone No";
            guna2DataGridView1.Columns[3].HeaderText = "Address";
            guna2DataGridView1.Columns[4].HeaderText = "Balance"; // Change header text for OpeningBalance
            guna2DataGridView1.Columns[5].HeaderText = "Desc";

            // Set column widths
            guna2DataGridView1.Columns[0].Width = 100; // VenderID
            guna2DataGridView1.Columns[1].Width = 200; // Name
            guna2DataGridView1.Columns[2].Width = 150; // PhoneNo
            guna2DataGridView1.Columns[3].Width = 250; // Address
            guna2DataGridView1.Columns[4].Width = 150; // Balance (formerly OpeningBalance)
            guna2DataGridView1.Columns[5].Width = 300; // Description
        }

        private void guna2DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (guna2DataGridView1.CurrentCell.OwningColumn.Name == "dvgEdit")
            {
                FormVenderAdd frm = new FormVenderAdd();
                frm.id = Convert.ToInt16(guna2DataGridView1.CurrentRow.Cells["dvgid"].Value);
                frm.txtName.Text = Convert.ToString(guna2DataGridView1.CurrentRow.Cells["dvgName"].Value);
                frm.txtPhone.Text = Convert.ToString(guna2DataGridView1.CurrentRow.Cells["dvgPhoneNO"].Value);
                frm.txtAddress.Text = Convert.ToString(guna2DataGridView1.CurrentRow.Cells["dvgAdress"].Value);
                frm.txtOPeningBalance.Text = Convert.ToString(guna2DataGridView1.CurrentRow.Cells["dvgOpeningBalance"].Value);
                frm.txtDescription.Text = Convert.ToString(guna2DataGridView1.CurrentRow.Cells["dvgDescription"].Value);

                MainClass.BlurBackground(frm);
                GetData();
            }
            if (guna2DataGridView1.CurrentCell.OwningColumn.Name == "dvgDelete")
            {
                DialogResult result = MessageBox.Show(
                    "Do you want to continue?",  // Message text
                    "Confirmation",              // Title text
                    MessageBoxButtons.YesNo,    // Buttons to display
                    MessageBoxIcon.Question);   // Icon to display

                if (result== DialogResult.Yes)
                {
                    int id = Convert.ToInt16(guna2DataGridView1.CurrentRow.Cells["dvgid"].Value);
                    string query = "Delete from TblVender where VenderID = '" + id + "' ";
                    Hashtable ht = new Hashtable();
                    MainClass.SQL(query, ht);

                    guna2MessageDialog1.Icon = Guna.UI2.WinForms.MessageDialogIcon.Information;
                    guna2MessageDialog1.Buttons = Guna.UI2.WinForms.MessageDialogButtons.OK;

                    MessageBox.Show("Detete is Succesfull");
                    GetData();
                }

            }
        }

        private void FormVenderView_Load(object sender, EventArgs e)
        {
            GetData();
        }

        public override void txtSearch_TextChanged(object sender, EventArgs e)
        {
            GetData();
        }
    }
}
