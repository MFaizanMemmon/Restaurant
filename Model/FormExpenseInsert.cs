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

namespace Restaurant.Model
{
    public partial class FormExpenseInsert : Form
    {
        public FormExpenseInsert()
        {
            InitializeComponent();
        }

        public int id = 0;
        private void btnSave_Click(object sender, EventArgs e)
        {
            string qry = "";

            if (id == 0)
            {
                qry = "INSERT INTO TblExpenseHead (ExpenseHead) VALUES (@ExpenseHead)";
            }
            else
            {
                qry = "UPDATE TblExpenseHead SET ExpenseHead = @ExpenseHead WHERE ExpenseID = @id";
            }

            using (SqlConnection conn = new SqlConnection(MainClass.con_string))
            using (SqlCommand cmd = new SqlCommand(qry, conn))
            {
                // Add parameters
                cmd.Parameters.AddWithValue("@ExpenseHead", txtExpenseHead.Text);

                if (id != 0)
                {
                    cmd.Parameters.AddWithValue("@id", id);
                }

                try
                {
                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery(); // Execute the query

                    if (rowsAffected > 0)
                    {
                        if (id == 0)
                        {
                            MessageBox.Show("Operation has been successfully done", "Notification");
                        }
                        else
                        {
                            MessageBox.Show("Operation has been successfully done", "Notification");
                        }

                        // Reset fields
                        id = 0;
                        txtExpenseHead.Text = "";
                        this.Close(); // Close the form or perform any other action
                    }
                    else
                    {
                        MessageBox.Show("Operation Failed: No rows affected.");
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception message and show an error message to the user
                    Console.WriteLine("An error occurred: " + ex.Message);
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
                finally
                {
                    // Ensure the connection is closed if it's still open
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        conn.Close();
                    }
                }
            }
        }

            public void GetData()
        {
            SqlConnection conn = new SqlConnection(MainClass.con_string);
            string qry = "SELECT * FROM TblExpenseHead WHERE ExpenseHead LIKE '%" + txtSearch.Text + "%'";

            SqlCommand cmd = new SqlCommand(qry, conn); // Initialize SqlCommand
            conn.Open();

            cmd.Parameters.AddWithValue("@ExpenseHead", "%" + txtSearch.Text + "%");

            SqlDataReader dr = cmd.ExecuteReader(); // Execute query

            datagridview1.Rows.Clear(); // Clear previous data

            while (dr.Read())
            {
                datagridview1.Rows.Add(
                    dr["Expenseid"].ToString(), // Column 1
                    dr["ExpenseHead"].ToString() // Column 2
                );
            }

            dr.Close(); // Close SqlDataReader
            conn.Close();
        }

        private void datagridview1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (datagridview1.CurrentCell.OwningColumn.Name == "dvgEdit")
            {
                FormCategoryAdd frm = new FormCategoryAdd();
                frm.id = Convert.ToInt16(datagridview1.CurrentRow.Cells["dvgid"].Value);
                frm.txtName.Text = Convert.ToString(datagridview1.CurrentRow.Cells["dvgAdd"].Value);
                MainClass.BlurBackground(frm);
                GetData();

            }
            if (datagridview1.CurrentCell.OwningColumn.Name == "dvgDelete")
            {
                // Need to Cinform Delete

                guna2MessageDialog1.Icon = Guna.UI2.WinForms.MessageDialogIcon.Question;
                guna2MessageDialog1.Buttons = Guna.UI2.WinForms.MessageDialogButtons.YesNo;

                if (guna2MessageDialog1.Show("You are sure you want to delete ") == DialogResult.Yes)
                {
                    int id = Convert.ToInt16(datagridview1.CurrentRow.Cells["dvgid"].Value);
                    string query = "Delete from TblExpenseHead where ExpenseID = '" + id + "' ";
                    Hashtable ht = new Hashtable();
                    MainClass.SQL(query, ht);

                    guna2MessageDialog1.Icon = Guna.UI2.WinForms.MessageDialogIcon.Information;
                    guna2MessageDialog1.Buttons = Guna.UI2.WinForms.MessageDialogButtons.OK;

                    MessageBox.Show("Detete is Succesfull");
                    GetData();

                }
            }
        }

        private void FormExpenseInsert_Load(object sender, EventArgs e)
        {
            GetData();
        }
    }
}
