using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Restaurant.Model
{
    public partial class FormExpenseAdd : SampleAdd
    {
        public FormExpenseAdd()
        {
            InitializeComponent();
        }

        private void btnExpense_Click(object sender, EventArgs e)
        {
            MainClass.BlurBackground(new FormExpenseInsert());
        }

        private void cbType_MouseClick(object sender, MouseEventArgs e)
        {
            LoadExpenseHeads();
        }

        private void LoadExpenseHeads()
        {
            string qry = "SELECT DISTINCT ExpenseHead FROM TblExpenseHead"; // Ensure DISTINCT if needed

            using (SqlConnection connection = new SqlConnection(MainClass.con_string))
            using (SqlDataAdapter adapter = new SqlDataAdapter(qry, connection))
            {
                DataTable dataTable = new DataTable();

                try
                {
                    // Fill DataTable
                    adapter.Fill(dataTable);

                    // Check if dataTable has rows
                    if (dataTable.Rows.Count == 0)
                    {
                        MessageBox.Show("No data found in TblExpenseHead.");
                        return;
                    }

                    // Bind the DataTable to the ComboBox
                    cbType.DataSource = dataTable;
                    cbType.DisplayMember = "ExpenseHead";
                    cbType.ValueMember = "ExpenseHead"; // Optional, set if needed
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }
        }

        public int id = 0;
        public override void btnSave_Click(object sender, EventArgs e)
        {
            if (txtNotes.Text == string.Empty && txtAmount.Text == string.Empty)
            {
                MessageBox.Show("Please Enter proper data");
                return;
            }

            string qry = "";

            if (id == 0)
            {
                qry = "INSERT INTO TblExpence (ExpDate, ExpHead, paymenttype, Amount, Notes, createdBy, ModifyBy) " +
                      "VALUES (@ExpDate, @ExpHead, @paymenttype, @Amount, @Notes, @createdBy, @ModifyBy)";
            }
            else
            {
                qry = "UPDATE TblExpence SET ExpDate = @ExpDate, ExpHead = @ExpHead, paymenttype = @paymenttype, " +
                      "Amount = @Amount, Notes = @Notes, createdBy = @createdBy, ModifyBy = @ModifyBy WHERE ExpID = @id";
            }

            using (SqlConnection conn = new SqlConnection(MainClass.con_string))
            using (SqlCommand cmd = new SqlCommand(qry, conn))
            {
                // Convert text input to appropriate types
                DateTime expDate;
                if (!DateTime.TryParse(ExpDateTime.Value.ToShortDateString(), out expDate))
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
                cmd.Parameters.AddWithValue("@ExpDate", DateTime.Now);
                cmd.Parameters.AddWithValue("@ExpHead", cbType.Text);
                cmd.Parameters.AddWithValue("@paymenttype", cbMode.Text);
                cmd.Parameters.AddWithValue("@Amount", amount);
                cmd.Parameters.AddWithValue("@Notes", txtNotes.Text);
                cmd.Parameters.AddWithValue("@createdBy", DateTime.Now); // Replace with actual user or appropriate value
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
                        ExpDateTime.Text = "";
                        cbType.SelectedIndex = -1; // Reset to no selection
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

        private void FormExpenseAdd_Load(object sender, EventArgs e)
        {
            ForUpdateLoadData();
        }

        private void ForUpdateLoadData()
        {
            // Replace with parameterized query to prevent SQL injection
            string qry = @"SELECT ExpDate, ExpHead, PaymentType, Amount, Notes 
                   FROM TblExpence 
                   WHERE ExpID = @ExpID";

            using (SqlCommand cmd = new SqlCommand(qry, MainClass.con))
            {
                // Add parameter to the query
                cmd.Parameters.AddWithValue("@ExpID", id);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    // Load data into text boxes and other controls
                    txtAmount.Text = dt.Rows[0]["Amount"].ToString();
                    txtNotes.Text = dt.Rows[0]["Notes"].ToString();

                    // Load additional fields
                    ExpDateTime.Value = Convert.ToDateTime(dt.Rows[0]["ExpDate"]);
                    cbType.SelectedItem = dt.Rows[0]["ExpHead"].ToString(); // Assuming it's a string, adjust as needed
                    cbMode.SelectedItem = dt.Rows[0]["PaymentType"].ToString(); // Assuming it's a string, adjust as needed
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
