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
    public partial class FormStaffView : SampleView
    {
        public FormStaffView(string btnText)
        {
            InitializeComponent();
            label2.Text = btnText;
        }
        public void GetData()
        {
            string qry = "SELECT s.StaffID,s.StaffName,s.StaffPhone,s.StaffRole FROM Staff s where s.StaffName LIKE @StaffName order by 1 desc";

            using (SqlCommand cmd = new SqlCommand(qry, MainClass.con)) // Initialize SqlCommand
            {
                cmd.Parameters.AddWithValue("@StaffName", "%" + txtSearch.Text + "%");

                // Open connection
                MainClass.con.Open();

                using (SqlDataReader dr = cmd.ExecuteReader()) // Execute query
                {
                    guna2DataGridView1.Rows.Clear(); // Clear previous data

                    while (dr.Read())
                    {
                        guna2DataGridView1.Rows.Add(
                            dr["StaffID"].ToString(),    // Column 1
                            dr["StaffName"].ToString(),  // Column 2
                            dr["StaffPhone"].ToString(), // Column 3
                            dr["StaffRole"].ToString()  // Column 4
                        );
                    }
                }

                MainClass.con.Close(); // Close SqlConnection
            }
            if (guna2DataGridView1.Columns.Count == 0)
            {
                guna2DataGridView1.Columns.Add("StaffID", "Staff ID");
                guna2DataGridView1.Columns.Add("StaffName", "Staff Name");
                guna2DataGridView1.Columns.Add("StaffPhone", "Staff Phone");
                guna2DataGridView1.Columns.Add("StaffRole", "Staff Role");
            }
        }

        private void FormStaffView_Load(object sender, EventArgs e)
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
            MainClass.BlurBackground(new FormStaffAdd());

            //FormTableAdd frm = new FormTableAdd();
            //frm.ShowDialog();
            GetData();
        }
        private void guna2DataGridView1_CellClick_1(object sender, DataGridViewCellEventArgs e)
        {

            if (guna2DataGridView1.CurrentCell.OwningColumn.Name == "dvgEdit")
            {
                FormStaffAdd frm = new FormStaffAdd();
                frm.id = Convert.ToInt16(guna2DataGridView1.CurrentRow.Cells["dvgid"].Value);
                frm.txtName.Text = Convert.ToString(guna2DataGridView1.CurrentRow.Cells["dvgName"].Value);
                frm.txtPhone.Text = Convert.ToString(guna2DataGridView1.CurrentRow.Cells["dvgPhone"].Value);
                frm.cbRole.Text = Convert.ToString(guna2DataGridView1.CurrentRow.Cells["dvgRole"].Value);

                frm.ShowDialog();
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
                    string query = "Delete from Staff where StaffID = '" + id + "' ";
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
