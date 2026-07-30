using System;
using System.Collections;
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
    public partial class FormCheckOut : SampleAdd
    {
        public FormCheckOut()
        {
            InitializeComponent();
        }

        public double amt;
        public int MainID = 0;
        public bool CheckoutCompleted { get; private set; }
        private void txtPayRecieved_TextChanged(object sender, EventArgs e)
        {
            double amt = 0;
            double reciept = 0;
            double change = 0;

            double.TryParse(txtBillAmount.Text, out amt);
            double.TryParse(txtPayRecieved.Text, out reciept);

            change = Math.Abs(amt - reciept); // Convert Positive or Negative To Always Positive

            txtChange.Text = change.ToString();
        }

        public override void btnSave_Click(object sender, EventArgs e)
        {
            double amt = 0;
            double reciept = 0;

            double.TryParse(txtBillAmount.Text, out amt);
            double.TryParse(txtPayRecieved.Text, out reciept);

            // Validate that received amount is not less than the bill amount
            if (reciept < amt)
            {
                MessageBox.Show("Received amount cannot be less than the bill amount.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // Exit the method to prevent saving invalid data
            }

            if (MainID <= 0)
            {
                MessageBox.Show("No valid bill was selected for checkout.", "Checkout Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string qry = @"UPDATE TblMain
                    SET Total = ?, Recieved = ?, [Change] = ?, PaidDateTime = ?, [Status] = ?
                    WHERE MainID = ?";

            using (var connection = new OleDbConnection(MainClass.con_string))
            using (OleDbCommand cmd = new OleDbCommand(qry, connection))
            {
                cmd.Parameters.Add("@total", OleDbType.Currency).Value = Convert.ToDecimal(txtBillAmount.Text);
                cmd.Parameters.Add("@rec", OleDbType.Currency).Value = Convert.ToDecimal(txtPayRecieved.Text);
                cmd.Parameters.Add("@change", OleDbType.Currency).Value = Convert.ToDecimal(txtChange.Text);
                cmd.Parameters.Add("@paidAt", OleDbType.Date).Value = DateTime.Now;
                cmd.Parameters.Add("@status", OleDbType.VarWChar, 50).Value = "Paid";
                cmd.Parameters.Add("@id", OleDbType.Integer).Value = MainID;

                try
                {
                    connection.Open();
                    int result = cmd.ExecuteNonQuery();

                    if (result > 0)
                    {
                        CheckoutCompleted = true;
                        guna2MessageDialog1.Buttons = Guna.UI2.WinForms.MessageDialogButtons.OK;
                        MessageBox.Show("Operation has been successfully done", "Notification");
                        Close();
                    }
                    else
                    {
                        MessageBox.Show("No records were updated.", "Error");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error occurred during the update: " + ex.Message, "Error");
                }
            }
           
        }



        private void FormCheckOut_Load(object sender, EventArgs e)
        {
            txtBillAmount.Text = amt.ToString("0.00");
            txtPayRecieved.Focus();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
