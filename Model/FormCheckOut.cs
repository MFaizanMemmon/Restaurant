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
    public partial class FormCheckOut : SampleAdd
    {
        public FormCheckOut()
        {
            InitializeComponent();
        }

        public double amt;
        public int MainID = 0;
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

            // Proceed with updating the database if validation passes
            string qry = @"UPDATE TblMain 
                    SET Total = @total, Recieved = @rec, Change = @chan, PaidDateTime = @dateTime, Status = 'Paid' 
                    WHERE MainID = @id";

            using (SqlCommand cmd = new SqlCommand(qry, MainClass.con))
            {
                // Define parameters and their values
                cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.Int)).Value = MainID;
                cmd.Parameters.Add(new SqlParameter("@total", SqlDbType.Decimal)).Value = Convert.ToDecimal(txtBillAmount.Text);
                cmd.Parameters.Add(new SqlParameter("@rec", SqlDbType.Decimal)).Value = Convert.ToDecimal(txtPayRecieved.Text);
                cmd.Parameters.Add(new SqlParameter("@chan", SqlDbType.Decimal)).Value = Convert.ToDecimal(txtChange.Text);
                cmd.Parameters.Add(new SqlParameter("@dateTime", SqlDbType.DateTime)).Value = DateTime.Now; // Includes both date and time

                try
                {
                    if (MainClass.con.State == ConnectionState.Closed)
                        MainClass.con.Open();

                    int result = cmd.ExecuteNonQuery();

                    if (result > 0)
                    {
                        guna2MessageDialog1.Buttons = Guna.UI2.WinForms.MessageDialogButtons.OK;
                        MessageBox.Show("Operation has been successfully done", "Notification");
                        this.Close();
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
                finally
                {
                    if (MainClass.con.State == ConnectionState.Open)
                        MainClass.con.Close();
                        
                }
            }
           
        }



        private void FormCheckOut_Load(object sender, EventArgs e)
        {
            txtBillAmount.Text = amt.ToString();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
