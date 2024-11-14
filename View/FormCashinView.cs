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
    public partial class FormCashinView : SampleView
    {
        public FormCashinView(string btnText)
        {
            InitializeComponent();
            label2.Text = btnText;
        }

        private void btnAdd_Click_1(object sender, EventArgs e)
        {
            MainClass.BlurBackground(new FormCashinAdd());
            GetData();
        }
        public void GetData()
        {
            // Define the query to select all data from TblExpence
            string qry = "SELECT CashID, CONVERT(varchar, DateTime , 106) as 'DateTime', CashMode, Amount, Notes FROM TblCashIn WHERE CashMode LIKE @CashMode";

            // Initialize SqlConnection and SqlCommand

            using (SqlCommand cmd = new SqlCommand(qry, MainClass.con))
            {
                // Add parameter with LIKE pattern
                cmd.Parameters.AddWithValue("@CashMode", "%" + txtSearch.Text + "%");

                try
                {
                    // Open connection
                    MainClass.con.Open();

                    // Execute the query
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        // Clear previous data
                        guna2DataGridView1.Rows.Clear();

                        // Read and add data to DataGridView
                        while (dr.Read())
                        {
                            guna2DataGridView1.Rows.Add(
                                dr["CashID"].ToString(),       // Column 1
                                dr["DateTime"].ToString(),     // Column 3
                                dr["CashMode"].ToString(),     // Column 4
                                dr["Amount"].ToString(),      // Column 6
                                dr["Notes"].ToString()
                            );
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle exceptions
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
                finally
                {
                    // Ensure connection is closed if an exception occurs
                    if (MainClass.con.State == System.Data.ConnectionState.Open)
                    {
                        MainClass.con.Close();
                    }
                }
            }
        }

        public override void txtSearch_TextChanged(object sender, EventArgs e)
        {
            GetData();
        }

        private void FormCashinView_Load(object sender, EventArgs e)
        {
            GetData();
        }

        private void guna2DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (guna2DataGridView1.CurrentCell.OwningColumn.Name == "dvgEdit")
            {
                FormCashinAdd frm = new FormCashinAdd();
                frm.id = Convert.ToInt16(guna2DataGridView1.CurrentRow.Cells["dvgid"].Value);
                //fr.Text = Convert.ToString(guna2DataGridView1.CurrentRow.Cells["dvgName"].Value);
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
                    string query = "Delete from TblCashIn where CashID = '" + id + "' ";
                    Hashtable ht = new Hashtable();
                    MainClass.SQL(query, ht);

                    guna2MessageDialog1.Icon = Guna.UI2.WinForms.MessageDialogIcon.Information;
                    guna2MessageDialog1.Buttons = Guna.UI2.WinForms.MessageDialogButtons.OK;

                    MessageBox.Show("Detete is Succesfull");
                    GetData();
                }

            }
        }
    }
}
