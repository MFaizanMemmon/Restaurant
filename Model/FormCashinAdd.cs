using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
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
            if (!decimal.TryParse(txtAmount.Text, out decimal amount))
            {
                MessageBox.Show("Please enter a valid amount.");
                txtAmount.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(cbMode.Text))
            {
                MessageBox.Show("Please select a cash mode.");
                cbMode.Focus();
                return;
            }

            string qry = id == 0
                ? "INSERT INTO TblCashIn ([DateTime], CashMode, Amount, Notes, createBy, ModifyBy) VALUES (?, ?, ?, ?, ?, ?)"
                : "UPDATE TblCashIn SET [DateTime] = ?, CashMode = ?, Amount = ?, Notes = ?, createBy = ?, ModifyBy = ? WHERE CashID = ?";

            using (OleDbConnection conn = new OleDbConnection(MainClass.con_string))
            using (OleDbCommand cmd = new OleDbCommand(qry, conn))
            {
                cmd.Parameters.Add("@DateTime", OleDbType.Date).Value = CashDateTime.Value;
                cmd.Parameters.Add("@CashMode", OleDbType.VarWChar, 100).Value = cbMode.Text;
                cmd.Parameters.Add("@Amount", OleDbType.Currency).Value = amount;
                cmd.Parameters.Add("@Notes", OleDbType.LongVarWChar).Value = txtNotes.Text;
                cmd.Parameters.Add("@createBy", OleDbType.VarWChar, 100).Value = MainClass.USER ?? "";
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
            if (id == 0) return;
            string qry = @"SELECT [DateTime], CashMode, Amount, Notes
                   FROM TblCashIn 
                   WHERE CashID = ?";

            using (var connection = new OleDbConnection(MainClass.con_string))
            using (OleDbCommand cmd = new OleDbCommand(qry, connection))
            {
                // Add parameter to the query
                cmd.Parameters.Add("@CashID", OleDbType.Integer).Value = id;

                OleDbDataAdapter da = new OleDbDataAdapter(cmd);
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
