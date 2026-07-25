using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
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

            using (OleDbConnection connection = new OleDbConnection(MainClass.con_string))
            using (OleDbDataAdapter adapter = new OleDbDataAdapter(qry, connection))
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
            if (!decimal.TryParse(txtAmount.Text, out decimal amount))
            {
                MessageBox.Show("Please enter a valid amount.");
                txtAmount.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(cbType.Text) || string.IsNullOrWhiteSpace(cbMode.Text))
            {
                MessageBox.Show("Please select the expense type and payment mode.");
                return;
            }

            string qry = id == 0
                ? "INSERT INTO TblExpence (ExpDate, ExpHead, PaymentType, Amount, Notes, createdBy, ModifyBy) VALUES (?, ?, ?, ?, ?, ?, ?)"
                : "UPDATE TblExpence SET ExpDate = ?, ExpHead = ?, PaymentType = ?, Amount = ?, Notes = ?, createdBy = ?, ModifyBy = ? WHERE ExpID = ?";

            using (OleDbConnection conn = new OleDbConnection(MainClass.con_string))
            using (OleDbCommand cmd = new OleDbCommand(qry, conn))
            {
                cmd.Parameters.Add("@ExpDate", OleDbType.Date).Value = ExpDateTime.Value;
                cmd.Parameters.Add("@ExpHead", OleDbType.VarWChar, 150).Value = cbType.Text;
                cmd.Parameters.Add("@PaymentType", OleDbType.VarWChar, 100).Value = cbMode.Text;
                cmd.Parameters.Add("@Amount", OleDbType.Currency).Value = amount;
                cmd.Parameters.Add("@Notes", OleDbType.LongVarWChar).Value = txtNotes.Text;
                cmd.Parameters.Add("@createdBy", OleDbType.VarWChar, 100).Value = MainClass.USER ?? "";
                cmd.Parameters.Add("@ModifyBy", OleDbType.VarWChar, 100).Value = MainClass.USER ?? "";

                if (id != 0)
                {
                    cmd.Parameters.Add("@id", OleDbType.Integer).Value = id;
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
            LoadExpenseHeads();
            ForUpdateLoadData();
        }

        private void ForUpdateLoadData()
        {
            // Replace with parameterized query to prevent SQL injection
            if (id == 0) return;
            string qry = @"SELECT ExpDate, ExpHead, PaymentType, Amount, Notes
                   FROM TblExpence 
                   WHERE ExpID = ?";

            using (var connection = new OleDbConnection(MainClass.con_string))
            using (OleDbCommand cmd = new OleDbCommand(qry, connection))
            {
                // Add parameter to the query
                cmd.Parameters.Add("@ExpID", OleDbType.Integer).Value = id;

                OleDbDataAdapter da = new OleDbDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    // Load data into text boxes and other controls
                    txtAmount.Text = dt.Rows[0]["Amount"].ToString();
                    txtNotes.Text = dt.Rows[0]["Notes"].ToString();

                    // Load additional fields
                    ExpDateTime.Value = Convert.ToDateTime(dt.Rows[0]["ExpDate"]);
                    cbType.SelectedValue = dt.Rows[0]["ExpHead"].ToString();
                    cbMode.Text = dt.Rows[0]["PaymentType"].ToString();
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
