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

namespace Restaurant.Model
{
    public partial class FormCashinAdd : SampleAdd
    {
        public FormCashinAdd()
        {
            InitializeComponent();
        }

        public int id = 0;
        public override void btnSave_Click(object sender, EventArgs e)
        {
            if (txtAmount.Text == string.Empty && txtNotes.Text == string.Empty)
            {
                MessageBox.Show("Please enter proper data");
                return;
            }

            string qry = "";

            if (id == 0)
            {
                qry = "INSERT INTO TblCashIn (DateTime, CashMode, Amount, Notes, createBy, ModifyBy) " +
                      "VALUES (@DateTime, @CashMode, @Amount, @Notes, @createBy, @ModifyBy)";
            }
            else
            {
                qry = "UPDATE TblCashIn SET DateTime = @DateTime, CashMode = @CashMode ,Amount = @Amount, Notes = @Notes, createBy = @createBy, ModifyBy = @ModifyBy WHERE CashID = @id";
            }

            using (SqlConnection conn = new SqlConnection(MainClass.con_string))
            using (SqlCommand cmd = new SqlCommand(qry, conn))
            {
                // Convert text input to appropriate types
                DateTime expDate;
                if (!DateTime.TryParse(CashDateTime.Value.ToShortDateString(), out expDate))
                {
                    MessageBox.Show("Invalid date format.");
                    return;
                }

                decimal amount;
                if (!decimal.TryParse(txtAmount.Text, out amount))
                {
                    MessageBox.Show("Invalid amount format.");
                    return;
                }

                // Add parameters
                cmd.Parameters.AddWithValue("@DateTime",DateTime.Now);
                cmd.Parameters.AddWithValue("@CashMode", cbMode.Text);
                cmd.Parameters.AddWithValue("@Amount", amount);
                cmd.Parameters.AddWithValue("@Notes", txtNotes.Text);
                cmd.Parameters.AddWithValue("@createBy", DateTime.Now); // Replace with actual user or appropriate value
                cmd.Parameters.AddWithValue("@ModifyBy", DateTime.Now); // Replace with actual user or appropriate value

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
                        CashDateTime.Text = "";
                        cbMode.SelectedIndex = -1; // Reset to no selection
                        txtAmount.Text = "";
                        txtNotes.Text = "";
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

        private void FormCashinAdd_Load(object sender, EventArgs e)
        {
            ForUpdateLoadData();
        }

        private void ForUpdateLoadData()
        {
            // Replace with parameterized query to prevent SQL injection
            string qry = @"SELECT DateTime, CashMode, Amount, Notes 
                   FROM TblCashIn 
                   WHERE CashID = @CashID";

            using (SqlCommand cmd = new SqlCommand(qry, MainClass.con))
            {
                // Add parameter to the query
                cmd.Parameters.AddWithValue("@CashID", id);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    // Load data into text boxes and other controls
                    txtAmount.Text = dt.Rows[0]["Amount"].ToString();
                    txtNotes.Text = dt.Rows[0]["Notes"].ToString();

                    // Load additional fields
                    CashDateTime.Value = Convert.ToDateTime(dt.Rows[0]["DateTime"]);
                    cbMode.SelectedItem = dt.Rows[0]["CashMode"].ToString(); // Assuming it's a string, adjust as needed
                    txtAmount.Text = dt.Rows[0]["Amount"].ToString();
                    txtNotes.Text = dt.Rows[0]["Notes"].ToString();
                }
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
