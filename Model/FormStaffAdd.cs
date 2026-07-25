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
    public partial class FormStaffAdd : SampleAdd
    {
        public FormStaffAdd()
        {
            InitializeComponent();
        }
        public int id = 0;
        public override void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Please enter the staff name.");
                txtName.Focus();
                return;
            }
            if (cbRole.SelectedValue == null ||
                !int.TryParse(cbRole.SelectedValue.ToString(), out int roleId))
            {
                MessageBox.Show("Please select a staff role.");
                cbRole.Focus();
                return;
            }

            string qry = id == 0
                ? "INSERT INTO Staff (StaffName, StaffPhone, RoleID, StaffRole) VALUES (?, ?, ?, ?)"
                : "UPDATE Staff SET StaffName = ?, StaffPhone = ?, RoleID = ?, StaffRole = ? WHERE StaffID = ?";

            int affected;
            using (var connection = new OleDbConnection(MainClass.con_string))
            using (var command = new OleDbCommand(qry, connection))
            {
                command.Parameters.Add("@Name", OleDbType.VarWChar, 150).Value = txtName.Text.Trim();
                command.Parameters.Add("@Phone", OleDbType.VarWChar, 50).Value = txtPhone.Text.Trim();
                command.Parameters.Add("@RoleID", OleDbType.Integer).Value = roleId;
                command.Parameters.Add("@StaffRole", OleDbType.VarWChar, 100).Value = cbRole.Text;
                if (id != 0)
                    command.Parameters.Add("@StaffID", OleDbType.Integer).Value = id;

                connection.Open();
                affected = command.ExecuteNonQuery();
            }

            if (affected > 0)
            {
                MessageBox.Show("Operation has been successfully done", "Notification");
                id = 0;
                txtName.Clear();
                txtPhone.Clear();
                cbRole.SelectedIndex = -1;
                Close();
            }
        }

        private void LoadColumnNames()
        {
            string qry = @"SELECT DISTINCT RoleID, RoleName FROM TblRole";

            using (var connection = new OleDbConnection(MainClass.con_string))
            using (OleDbDataAdapter adapter = new OleDbDataAdapter(qry, connection))
            {
                DataTable dataTable = new DataTable();
                try
                {
                    adapter.Fill(dataTable);
                }
                catch (Exception ex)
                {
                    // Handle exceptions (e.g., logging or displaying a message)
                    MessageBox.Show("Error: " + ex.Message);
                }
                if (dataTable.Rows.Count > 0)
                {
                    cbRole.DataSource = dataTable;
                    cbRole.DisplayMember = "RoleName";
                    cbRole.ValueMember = "RoleID";
                }
                else
                {
                    // Handle case where no data is returned
                    cbRole.DataSource = null;
                    cbRole.Items.Clear();
                }
            }
        }


        private void FormStaffAdd_Load(object sender, EventArgs e)
        {
            string selectedRole = cbRole.Text;
            LoadColumnNames();
            if (id > 0 && !string.IsNullOrWhiteSpace(selectedRole))
                cbRole.SelectedIndex = cbRole.FindStringExact(selectedRole);
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
