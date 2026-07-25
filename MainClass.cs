using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using System.Security.Cryptography;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;

namespace Restaurant
{
    internal class MainClass
    {
        public static readonly string DatabasePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RM.accdb");
        public static readonly string con_string =
            @"Provider=Microsoft.ACE.OLEDB.16.0;Data Source=" + DatabasePath +
            @";Persist Security Info=False;";
        public static OleDbConnection con = new OleDbConnection(con_string);

        public static bool invaliduser(string user, string pass)
        {
            bool iavalid = false;

            string query = "select * from Users where UName = ? and UPass = ?";
            OleDbCommand cmd = new OleDbCommand(query, con);
            cmd.Parameters.AddWithValue("@user", user);
            cmd.Parameters.AddWithValue("@pass", pass);
            DataTable dt = new DataTable();
            OleDbDataAdapter da = new OleDbDataAdapter(cmd);
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
                OleDbCommand cmd = CreateCommand(qry, ht);
                cmd.CommandType = CommandType.Text;
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

        public static int GetLastIdentity()
        {
            using (OleDbCommand cmd = new OleDbCommand("SELECT @@IDENTITY", con))
            {
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        // ACE/OLE DB uses positional parameters. This preserves the order in
        // which @parameters occur in the SQL instead of Hashtable iteration order.
        private static OleDbCommand CreateCommand(string query, Hashtable values)
        {
            OleDbCommand cmd = new OleDbCommand(query, con);
            MatchCollection names = Regex.Matches(query, @"@[A-Za-z_][A-Za-z0-9_]*");
            HashSet<string> added = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (Match match in names)
            {
                string name = match.Value;
                if (added.Contains(name)) continue;
                object value = values[name] ?? values[name.TrimStart('@')];
                cmd.Parameters.AddWithValue(name, value ?? DBNull.Value);
                added.Add(name);
            }
            return cmd;
        }

        // Loading Data in DataBase

        public static void LoadData(string qry, DataGridView gv, ListBox lb)
        {
            // Serial no in Gridview

            //gv.CellFormatting += new DataGridViewCellFormattingEventHandler(gv_CellFormatting);

            //try
            //{
            //    OleDbCommand cmd = new OleDbCommand(qry, con);
            //    cmd.CommandType = CommandType.Text;

            //    OleDbDataAdapter da = new OleDbDataAdapter(cmd);
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
                OleDbCommand cmd = new OleDbCommand(qry, con);
                cmd.CommandType = CommandType.Text;

                OleDbDataAdapter da = new OleDbDataAdapter(cmd);
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
            OleDbCommand cmd = new OleDbCommand(qry, con);
            cmd.CommandType = CommandType.Text;
            OleDbDataAdapter da = new OleDbDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);

            cb.DisplayMember = "name";
            cb.ValueMember = "id";
            cb.DataSource = dt;
            cb.SelectedIndex = -1;
        }

    }
}
