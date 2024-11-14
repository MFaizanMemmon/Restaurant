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
    public partial class FormTableView : SampleView
    {
        public FormTableView(string btnText)
        {
            InitializeComponent();
            label2.Text = btnText;
        }

        public void GetData()
        {
            string qry = "SELECT * FROM Tables WHERE Tname LIKE '%" + txtSearch.Text + "%' order by 1 desc";

            SqlCommand cmd = new SqlCommand(qry, MainClass.con); // Initialize SqlCommand

            cmd.Parameters.AddWithValue("@TName", "%" + txtSearch.Text + "%");

            // Open connection
            MainClass.con.Open();

            SqlDataReader dr = cmd.ExecuteReader(); // Execute query

            guna2DataGridView1.Rows.Clear(); // Clear previous data

            while (dr.Read())
            {
                guna2DataGridView1.Rows.Add(
                    dr["Tid"].ToString(), // Column 1
                    dr["TName"].ToString() // Column 2
                );
            }

            dr.Close(); // Close SqlDataReader
            MainClass.con.Close(); // Close SqlConnection
        }


        private void FormTable_Load(object sender, EventArgs e)
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
            MainClass.BlurBackground(new FormTableAdd());

            //FormTableAdd frm = new FormTableAdd();
            //frm.ShowDialog();
            GetData();
        }

        private void guna2DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (guna2DataGridView1.CurrentCell.OwningColumn.Name == "dvgEdit")
            {
                FormTableAdd frm = new FormTableAdd();
                frm.id = Convert.ToInt16(guna2DataGridView1.CurrentRow.Cells["dvgid"].Value);
                frm.txtName.Text = Convert.ToString(guna2DataGridView1.CurrentRow.Cells["dvgName"].Value);
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
                    string query = "Delete from Tables where Tid = '" + id + "' ";
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
