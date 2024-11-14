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
    public partial class FormBill : SampleAdd
    {
        public FormBill()
        {
            InitializeComponent();
        }

        private void LoadExpenseHeads()
        {
            string qry = "SELECT DISTINCT ProductName, ProductPrice FROM Product"; // Include ProductPrice

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
                        MessageBox.Show("No data found in TblProduct.");
                        return;
                    }

                    // Bind the DataTable to the ComboBox
                    cbProduct.DataSource = dataTable;
                    cbProduct.DisplayMember = "ProductName";
                    cbProduct.ValueMember = "ProductPrice"; // Set ValueMember to ProductPrice
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }
        }

        private void LoadVender()
        {
            string qry = "SELECT DISTINCT Name FROM tblvender"; // Include ProductPrice

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
                        MessageBox.Show("No data found in TblProduct.");
                        return;
                    }

                    // Bind the DataTable to the ComboBox
                    cbvender.DataSource = dataTable;
                    cbvender.DisplayMember = "Name";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }
        }


        private void cbProduct_MouseClick(object sender, MouseEventArgs e)
        {
            LoadExpenseHeads();
        }

        private void cbProduct_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbProduct.SelectedItem != null)
            {
                // Get the selected row
                DataRowView selectedRow = cbProduct.SelectedItem as DataRowView;

                if (selectedRow != null)
                {
                    // Display the ProductPrice in the TextBox
                    txtPrice.Text = selectedRow["ProductPrice"].ToString();
                }
            }
        }

        private void txtQTY_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(txtPrice.Text, out int price) && int.TryParse(txtQTY.Text, out int qty))
            {
                // Calculate the total
                int total = price * qty;
                txtTotal.Text = total.ToString();
            }
            else
            {
                // Clear the txtTotal if the input is invalid
                txtTotal.Text = string.Empty;
            }
        }

        // Example code to add columns (if not added via Designer)
        private void InitializeDataGridView()
        {
            guna2DataGridView2.Columns.Add("ProductName", "Product Name");
            guna2DataGridView2.Columns.Add("ProductPrice", "Price");
            guna2DataGridView2.Columns.Add("Quantity", "Quantity");
            guna2DataGridView2.Columns.Add("Total", "Total");
        }

        private void CalculateSubTotal()
        {
            decimal subTotal = 0;

            // Loop through each row in the DataGridView to sum the 'Total' column
            foreach (DataGridViewRow row in guna2DataGridView2.Rows)
            {
                // Ensure the row is not the new row placeholder
                if (row.Cells["dvgTotal"].Value != null && decimal.TryParse(row.Cells["dvgTotal"].Value.ToString(), out decimal total))
                {
                    subTotal += total;
                }
            }

            // Display the subtotal in the txtSubTotal TextBox
            txtSubTotal.Text = subTotal.ToString();
        }

        private void btnAdd_Click_1(object sender, EventArgs e)
        {
            // Ensure the input is valid before proceeding
            if (cbProduct.SelectedItem != null &&
                int.TryParse(txtPrice.Text, out int price) &&
                int.TryParse(txtQTY.Text, out int qty) &&
                int.TryParse(txtTotal.Text, out int total))
            {
                // Get the product name from ComboBox
                string productName = cbProduct.Text;  // or cbProduct.SelectedItem.ToString();

                // Add a new row to the DataGridView
                guna2DataGridView2.Rows.Add(productName, price, qty, total);

                // Recalculate Subtotal after adding the row
                CalculateSubTotal();
            }
            else
            {
                MessageBox.Show("Please ensure all inputs are valid before adding to the grid.");
            }

            txtQTY.Clear();
            txtTotal.Clear();
            cbProduct.SelectedIndex = -1;
        }

        private void txtDescount_TextChanged(object sender, EventArgs e)
        {
            // Ensure both the SubTotal and Discount fields have valid numeric values
            if (txtDescount.Text.Length > 0 && txtSubTotal.Text.Length > 0)
            {
                try
                {
                    // Convert SubTotal and Discount to integers
                    int subTotal = Convert.ToInt32(txtSubTotal.Text);
                    int discount = Convert.ToInt32(txtDescount.Text);

                    // Calculate the NetAmount
                    int netAmount = subTotal - discount;

                    // Display the NetAmount in the txtNetAmount TextBox
                    txtNetAmount.Text = netAmount.ToString();
                }
                catch (FormatException)
                {
                    MessageBox.Show("Please enter valid numeric values for SubTotal and Discount.");
                }
            }
        }

        private void txtPaidAmount_TextChanged(object sender, EventArgs e)
        {
            // Ensure both the PaidAmount and NetAmount fields have valid numeric values
            if (txtPaidAmount.Text.Length > 0 && txtNetAmount.Text.Length > 0)
            {
                try
                {
                    // Convert NetAmount and PaidAmount to integers
                    int netAmount = Convert.ToInt32(txtNetAmount.Text);
                    int paidAmount = Convert.ToInt32(txtPaidAmount.Text);

                    // Calculate the Balance
                    int balance = netAmount - paidAmount;

                    // Display the Balance in the txtBalance TextBox
                    txtBalance.Text = balance.ToString();
                }
                catch (FormatException)
                {
                    MessageBox.Show("Please enter valid numeric values for NetAmount and PaidAmount.");
                }
            }
        }

        private void guna2DataGridView2_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Ensure the user is clicking on a valid row and not on the header
            if (e.RowIndex >= 0)
            {
                // Show confirmation dialog
                DialogResult result = MessageBox.Show("Do you want to delete this row?", "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                // If the user clicked Yes, remove the row
                if (result == DialogResult.Yes)
                {
                    // Remove the row at the clicked index
                    guna2DataGridView2.Rows.RemoveAt(e.RowIndex);
                }
            }
        }
        private void cbvender_MouseClick(object sender, MouseEventArgs e)
        {
            LoadVender();
        }
    }
}