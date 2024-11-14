using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using System.Security.Cryptography;
using System.Drawing;

namespace Restaurant
{
    internal class MainClass
    {
        public static readonly string con_string = @"Data Source=DESKTOP-V6DB7O3\SQLEXPRESS;Initial Catalog=RM;Integrated Security=True";
        public static SqlConnection con = new SqlConnection(con_string);

        public static bool invaliduser(string user, string pass)
        {
            bool iavalid = false;

            string query = "select * from Users where UName = '" + user + "' and UPass = '" + pass + "' ";
            SqlCommand cmd = new SqlCommand(query, con);
            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(dt);

            if (dt.Rows.Count > 0)
            {
                iavalid = true;
                USER = dt.Rows[0]["UserName"].ToString();
                ROLEID = Convert.ToInt32(dt.Rows[0]["RoleId"]);
            }

            return iavalid;
        }

        //Create Propert For UserName

        public static string user;
        public static int role;

        public static string USER
        {
            get { return user; }
            private set { user = value; }
        }

        public static int ROLEID
        {
            get { return role; }
            private set { role = value; }
        }

        // Method For Curd Operation

        public static int SQL(string qry, Hashtable ht)
        {
            int res = 0;
            try
            {
                SqlCommand cmd = new SqlCommand(qry, con);
                cmd.CommandType = CommandType.Text;

                foreach (DictionaryEntry item in ht)
                {
                    cmd.Parameters.AddWithValue(item.Key.ToString(), item.Value);
                }
                if (con.State == ConnectionState.Closed) { con.Open(); }
                res = cmd.ExecuteNonQuery();
                if (con.State == ConnectionState.Open) { con.Close(); }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                con.Close();
            }
            return res;
        }

        // Loading Data in DataBase

        public static void LoadData(string qry, DataGridView gv, ListBox lb)
        {
            // Serial no in Gridview

            //gv.CellFormatting += new DataGridViewCellFormattingEventHandler(gv_CellFormatting);

            //try
            //{
            //    SqlCommand cmd = new SqlCommand(qry, con);
            //    cmd.CommandType = CommandType.Text;

            //    SqlDataAdapter da = new SqlDataAdapter(cmd);
            //    DataTable dt = new DataTable();
            //    da.Fill(dt);

            //    for (int i = 0; i < lb.Items.Count; i++)
            //    {
            //        string Colno1 = ((DataGridViewColumn)lb.Items[i]).Name;
            //        gv.Columns[Colno1].DataPropertyName = dt.Columns[i].ToString();
            //    }
            //    gv.DataSource = dt;
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.ToString());
            //    con.Close();
            //}


            try
            {
                SqlCommand cmd = new SqlCommand(qry, con);
                cmd.CommandType = CommandType.Text;

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                // Ensure ListBox and DataGridView are not null
                if (lb != null && gv != null)
                {
                    // Check if the ListBox items can be cast to DataGridViewColumn and if gv has the corresponding columns
                    for (int i = 0; i < lb.Items.Count && i < dt.Columns.Count; i++)
                    {
                        if (lb.Items[i] is DataGridViewColumn column)
                        {
                            string Colno1 = column.Name;
                            if (gv.Columns.Contains(Colno1))
                            {
                                gv.Columns[Colno1].DataPropertyName = dt.Columns[i].ColumnName;
                            }
                            else
                            {
                                // Handle the case where the column name does not exist in the DataGridView
                                MessageBox.Show($"Column {Colno1} does not exist in the DataGridView.");
                            }
                        }
                    }
                    gv.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

       



        //private static void gv_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        //{
        //    Guna.UI2.WinForms.Guna2DataGridView gv = (Guna.UI2.WinForms.Guna2DataGridView)sender;
        //    int count = 0;

        //    foreach (DataGridViewRow row in gv.Rows)
        //    {
        //        count++;
        //        row.Cells[0].Value = count;
        //    }
        //}


        public static void BlurBackground(Form model)
        {
            Form background = new Form();
            using (model)
            {
                background.StartPosition = FormStartPosition.Manual;
                background.FormBorderStyle = FormBorderStyle.None;
                background.Opacity = 0.5d;
                background.BackColor = Color.Black;
                background.Size = MainForm.Insstance.Size;
                background.Location = MainForm.Insstance.Location;
                background.ShowInTaskbar = false;
                background.Show();
                model.Owner = background;
                model.ShowDialog(background);
                background.Dispose();
            }
        }

        //cb fill

        public static void CBFill(string qry, ComboBox cb)
        {
            SqlCommand cmd = new SqlCommand(qry, con);
            cmd.CommandType = CommandType.Text;
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);

            cb.DisplayMember = "name";
            cb.ValueMember = "id";
            cb.DataSource = dt;
            cb.SelectedIndex = -1;
        }

    }
}
