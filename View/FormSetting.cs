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

namespace Restaurant.View
{
    public partial class FormSetting : Form
    {
        public FormSetting(string btnText)
        {
            InitializeComponent();
            label2.Text = btnText;
        }

        private void btnStaff_Click(object sender, EventArgs e)
        {

        }
         private void btnSave_Click(object sender, EventArgs e)
        {

            // Define CheckBox and Button pairs
            var checkBoxButtonPairs = new[]
            {
        new { CheckBox = guna2CheckBox1, Button = btnHome },
        new { CheckBox = guna2CheckBox2, Button = btnCategory },
        new { CheckBox = guna2CheckBox3, Button = btnVender },
        new { CheckBox = guna2CheckBox4, Button = btnProduct },
        new { CheckBox = guna2CheckBox5, Button = btnTable },
        new { CheckBox = guna2CheckBox6, Button = btnStaff },
        new { CheckBox = guna2CheckBox7, Button = btnPOS },
        new { CheckBox = guna2CheckBox8, Button = btnKitchen }
    };

            string qry = "INSERT INTO TblRoleAuther (RoleID, Access) VALUES (@RoleID, @Access)";

            using (SqlConnection connection = MainClass.con)
            {
                connection.Open(); // Open the connection

                foreach (var pair in checkBoxButtonPairs)
                {
                    if (pair.CheckBox.Checked)
                    {
                        using (SqlCommand cmd = new SqlCommand(qry, connection))
                        {
                            // Determine RoleID based on button
                            int roleId = GetRoleID(pair.Button);

                            cmd.Parameters.AddWithValue("@RoleID", cbRole.SelectedValue); ;
                            cmd.Parameters.AddWithValue("@Access", pair.Button.Text);

                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }

            // Optionally show a message
            MessageBox.Show("Save successful");
            // Uncheck all checkboxes
            foreach (var pair in checkBoxButtonPairs)
            {
                pair.CheckBox.Checked = false;
                
            }
            cbRole.SelectedIndex = -1;
        }

        private int GetRoleID(Control button)
        {
            switch (button.Name)
            {
                case "btnHome": return 1;
                case "btnCategory": return 2;
                case "btnVender": return 3;
                case "btnProduct": return 4;
                case "btnTable": return 5;
                case "btnStaff": return 6;
                case "btnPOS": return 7;
                case "btnKitchen": return 8;
                default: return 0; // Default value if no match is found
            }
        }

        private void LoadColumnNames()
               {
            string qry = @"SELECT RoleID, RoleName FROM TblRole";

            SqlDataAdapter adapter = new SqlDataAdapter(qry, MainClass.con);
            DataTable dataTable = new DataTable();

            MainClass.con.Open();
            adapter.Fill(dataTable);
            MainClass.con.Close();

            cbRole.DataSource = dataTable;
            cbRole.DisplayMember = "RoleName";
            cbRole.ValueMember = "RoleID";
        }

        private void cbRole_MouseClick(object sender, MouseEventArgs e)
        {
            LoadColumnNames();
        }
    }
}