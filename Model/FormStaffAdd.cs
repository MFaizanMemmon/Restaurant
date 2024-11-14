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
    public partial class FormStaffAdd : SampleAdd
    {
        public FormStaffAdd()
        {
            InitializeComponent();
        }
        public int id = 0;
        public override void btnSave_Click(object sender, EventArgs e)
        {
            string qry = "";

            if (id == 0)
            {
                qry = "Insert into Staff values (@Name , @Phone, @Role, @StaffRole )";
            }
            else
            {
                qry = "update Staff set StaffName = @Name, StaffPhone = @Phone ,RoleID = @Role ,StaffRole = @StaffRole  where StaffID = @id";
                //guna2MessageDialog1.Show("Update Succesfull");
            }

            Hashtable ht = new Hashtable();
            ht.Add("@id", id);
            ht.Add("@Name", txtName.Text);
            ht.Add("@Phone", txtPhone.Text);
            ht.Add("@Role", cbRole.SelectedValue);
            ht.Add("@StaffRole", cbRole.Text);

            if (MainClass.SQL(qry, ht) > 0)
            {
                MessageBox.Show("Operation has been successfully done", "Notification");
                id = 0;
                txtName.Text = "";
                txtPhone.Text = "";
                txtName.Focus();
                txtPhone.Focus();
                cbRole.SelectedIndex = -1;
                this.Close();

            }
        }

        private void LoadColumnNames()
        {
            string qry = @"SELECT DISTINCT RoleID, RoleName FROM TblRole";

            using (SqlDataAdapter adapter = new SqlDataAdapter(qry, MainClass.con))
            {
                DataTable dataTable = new DataTable();
                try
                {
                    MainClass.con.Open();
                    adapter.Fill(dataTable);
                }
                catch (Exception ex)
                {
                    // Handle exceptions (e.g., logging or displaying a message)
                    MessageBox.Show("Error: " + ex.Message);
                }
                finally
                {
                    MainClass.con.Close();
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
            LoadColumnNames();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
