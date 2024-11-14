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
using System.Web;
using System.Windows.Forms;

namespace Restaurant.Model
{
    public partial class FormPosReturn : Form
    {
        public FormPosReturn()
        {
            InitializeComponent();
        }

        public int MainID = 0;
        public string OrderType = "";
        public int DriverID = 0;
        public string CustomerName = "";
        public string CustomerPhone = "";

        private void FormPosReturn_Load(object sender, EventArgs e)
        {
            AddCategory();
            guna2DataGridView1.BorderStyle = BorderStyle.FixedSingle;
            LoadProducts();
        }

        //Getting product From DataBase
        private void LoadProducts()
        {
            string qry = "SELECT * FROM Product INNER JOIN Category ON Product.CategoryID = Category.CategoryID";
            SqlCommand cmd = new SqlCommand(qry, MainClass.con);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);

            foreach (DataRow item in dt.Rows)
            {
                byte[] imagearray = (byte[])item["ProductImage"];
                byte[] imageBytearray = imagearray;

                AddItems("0", item["ProductID"].ToString(), item["ProductName"].ToString(), item["CategoryName"].ToString(),
                item["ProductPrice"].ToString(), Image.FromStream(new MemoryStream(imageBytearray)));
            }
        }

        private void AddItems(string id, string ProID, string name, string cat, string price, Image Pimage)
        {
            var w = new UsProduct()
            {
                PName = name,
                pPrice = price,
                PCategory = cat,
                PIameg = Pimage,
                id = Convert.ToInt32(ProID)
            };

            ProductPanel.Controls.Add(w);

            w.onSelect += (ss, ee) =>
            {
                var wdg = (UsProduct)ss;

                foreach (DataGridViewRow item in guna2DataGridView1.Rows)
                {
                    // this will check it product already there then a one to quantity and update price
                    if (Convert.ToInt32(item.Cells["dvgProID"].Value) == wdg.id)
                    {
                        item.Cells["dvgQty"].Value = (int.Parse(item.Cells["dvgQty"].Value.ToString()) + 1).ToString();
                        item.Cells["dvgAmount"].Value = int.Parse(item.Cells["dvgQty"].Value.ToString()) *
                                                        double.Parse(item.Cells["dvgPrice"].Value.ToString());
                        return;

                    }
                }
                // This line and New Product
                guna2DataGridView1.Rows.Add(new object[] { 0, wdg.id, wdg.PName, 1, wdg.pPrice, wdg.pPrice });
                GetTotal();
            };
        }

        private void GetTotal()
        { 
            int count = 0;

            foreach (DataGridViewRow row in guna2DataGridView1.Rows)
            {
                count++;
                row.Cells[0].Value = count;
            }


            // LbLToTal Amount Show POS Screen
            double tot = 0;
            lbltotal.Text = "";
            foreach (DataGridViewRow item in guna2DataGridView1.Rows)
            {
                tot += double.Parse(item.Cells["dvgAmount"].Value.ToString());
            }
            lbltotal.Text = tot.ToString("N2");
        }

        private void btnexit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void AddCategory()
        {
            string qry = "select * from Category";
            SqlCommand cmd = new SqlCommand(qry, MainClass.con);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);

            CategoryPanel.Controls.Clear();

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    Guna.UI2.WinForms.Guna2Button b = new Guna.UI2.WinForms.Guna2Button();
                    b.FillColor = Color.FromArgb(50, 55, 89);
                    b.Size = new Size(134, 45);
                    b.ButtonMode = Guna.UI2.WinForms.Enums.ButtonMode.RadioButton;
                    b.Text = row["CategoryName"].ToString();

                    // event for click
                    b.Click += new EventHandler(b_Click);

                    CategoryPanel.Controls.Add(b);
                }
            }
        }

        private void b_Click(object sender, EventArgs e)
        {
            Guna.UI2.WinForms.Guna2Button b = (Guna.UI2.WinForms.Guna2Button)sender;
            if (b.Text == "All Categoryies")
            {
                txtSearch.Text = "1";
                txtSearch.Text = "";
                return;
            }
            foreach (var item in ProductPanel.Controls)
            {
                var pro = (UsProduct)item;
                pro.Visible = pro.PCategory.ToLower().Contains(b.Text.Trim().ToLower());
            }
        }

        private void btnReturn_Click(object sender, EventArgs e)
        {

            string qry1 = ""; // Main Table
            string qry2 = ""; // Detail Table

            int DetailID = 0;

            if (MainID == 0) // Insert
            {
                qry1 = @"INSERT INTO TblMainReturn (Date, Time, TableName, WaiterName, Status, OrderType, Total, Recieved, Change, DriverID, CustName, CustPhone,InvoiceID,Reson)
                 VALUES (@Date, @Time, @TableName, @WaiterName, @Status, @OrderType, @Total, @Recieved, @Change, @DriverID, @CustName, @CustPhone,@InvoiceID,@Reson);
                 SELECT SCOPE_IDENTITY();";
            }
            else // Update
            {
                qry1 = @"UPDATE TblMainReturn
                 SET Status = @Status, Total = @Total, Recieved = @Recieved, Change = @Change, InvoiceID = @InvoiceID, Reson = @Reson 
                 WHERE MainID = @ID;";
            }

            SqlCommand cmd = new SqlCommand(qry1, MainClass.con);

            if (MainID != 0)
            {
                cmd.Parameters.AddWithValue("@ID", MainID);
            }
            cmd.Parameters.AddWithValue("@Date", DateTime.Now.Date);
            cmd.Parameters.AddWithValue("@Time", DateTime.Now.ToShortTimeString());
            cmd.Parameters.AddWithValue("@TableName", lblTable.Text);
            cmd.Parameters.AddWithValue("@WaiterName", lblWaiter.Text);
            cmd.Parameters.AddWithValue("@Status", "Pending");
            cmd.Parameters.AddWithValue("@OrderType", OrderType); // Make sure OrderType is provided and not null
            cmd.Parameters.AddWithValue("@Total", Convert.ToDouble(lbltotal.Text));
            cmd.Parameters.AddWithValue("@Recieved", Convert.ToDouble(0));
            cmd.Parameters.AddWithValue("@Change", Convert.ToDouble(0));
            cmd.Parameters.AddWithValue("@DriverID", DriverID);
            cmd.Parameters.AddWithValue("@CustName", CustomerName);
            cmd.Parameters.AddWithValue("@CustPhone", CustomerPhone);
            cmd.Parameters.AddWithValue("@InvoiceID", txtInvoiceID.Text);
            cmd.Parameters.AddWithValue("@Reson", txtReson.Text);

            if (MainClass.con.State == ConnectionState.Closed) { MainClass.con.Open(); }
            if (MainID == 0) { MainID = Convert.ToInt32(cmd.ExecuteScalar()); } else { cmd.ExecuteNonQuery(); }
            if (MainClass.con.State == ConnectionState.Open) { MainClass.con.Close(); }

            foreach (DataGridViewRow row in guna2DataGridView1.Rows)
            {
                DetailID = Convert.ToInt32(row.Cells["dvgid"].Value);

                if (DetailID != 0)
                {
                    qry2 = @"INSERT INTO TblDetailReturn (MainID, ProID, Qty, Price, Amount) 
                          VALUES (@MainID, @ProID, @Qty, @Price, @Amount)";
                }
                else
                {
                    qry2 = @"UPDATE TblDetailReturn 
                          SET ProID = @ProID, Qty = @Qty, Price = @Price, Amount = @Amount
                          WHERE DetailID = @ID";
                }

                SqlCommand cmd2 = new SqlCommand(qry2, MainClass.con);
                if (DetailID != 0)
                {
                    cmd2.Parameters.AddWithValue("@ID", DetailID);
                }
                cmd2.Parameters.AddWithValue("@MainID", MainID);
                cmd2.Parameters.AddWithValue("@ProID", Convert.ToInt32(row.Cells["DvgProID"].Value));
                cmd2.Parameters.AddWithValue("@Price", Convert.ToDouble(row.Cells["DvgPrice"].Value));

                int qty;
                if (int.TryParse(row.Cells["DvgQty"].Value?.ToString(), out qty))
                {
                    cmd2.Parameters.AddWithValue("@Qty", qty);
                }
                else
                {
                    qty = 0; // or any default value you prefer
                    cmd2.Parameters.AddWithValue("@Qty", qty);
                }

                cmd2.Parameters.AddWithValue("@Amount", Convert.ToDouble(row.Cells["DvgAmount"].Value));


                if (MainClass.con.State == ConnectionState.Closed) { MainClass.con.Open(); }
                cmd2.ExecuteNonQuery();
                if (MainClass.con.State == ConnectionState.Open) { MainClass.con.Close(); }


                MessageBox.Show("Operation has been successfully done", "Notification");
                MainID = 0;
                DetailID = 0;
                guna2DataGridView1.Rows.Clear();
                lblTable.Text = "";
                lblWaiter.Text = "";
                lblTable.Visible = false;
                lblWaiter.Visible = false;
                lbltotal.Text = "00";
                lblDriverName.Text = "";
            }
        }
    }
}
