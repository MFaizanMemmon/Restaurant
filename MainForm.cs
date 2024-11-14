using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Restaurant.Model;
using Restaurant.View;

namespace Restaurant
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        //for accessing mainform
        static MainForm _obj;
        public static MainForm Insstance
        {
            get {  if (_obj == null) { _obj = new MainForm(); } return _obj; }
        }


        private void CenterPanel_Paint(object sender, PaintEventArgs e)
        {

        }

        public void AddControl(Form f)
        {
            CenterPanel.Controls.Clear();
            f.Dock = DockStyle.Fill;
            f.TopLevel = false;
            CenterPanel.Controls.Add(f);
            f.Show();
        }

        private void btnHome_Click(object sender, EventArgs e)
        {
            AddControl(new FormHome());
        }

        private void btnCategory_Click(object sender, EventArgs e)
        {
            string btnName = btnCategory.Text;
            AddControl(new FormCategoryView(btnName)); 
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            lbluser.Text = MainClass.USER;
            //AccessPage(MainClass.ROLEID);
            _obj = this;
            AddControl(new FormHome());

        }
        //private void AccessPage(int roleID)
        //{
        //    // Assuming you have a connection string defined elsewhere
        //    using (SqlConnection con = new SqlConnection(MainClass.con_string))
        //    {
        //        string query = "SELECT Access FROM TblRoleAuther WHERE RoleID = @RoleID";

        //        using (SqlCommand cmd = new SqlCommand(query, con))
        //        {
        //            cmd.Parameters.AddWithValue("@RoleID", roleID);

        //            con.Open();

        //            using (SqlDataReader reader = cmd.ExecuteReader())
        //            {
        //                // Clear any existing buttons
                        
        //                while (reader.Read())
        //                {
        //                    string accessText = reader["Access"].ToString();
        //                    switch (accessText)
        //                    {
        //                        case "Home":
        //                            btnHome.Visible = true;
        //                            break;
        //                        case  "Category":
        //                            btnCategory.Visible = true;
        //                            break;
        //                        case "Vendor":
        //                            btnVender.Visible = true;
        //                            break;
        //                        case "Product":
        //                            btnProduct.Visible = true;
        //                            break;
        //                        case "Table":
        //                            btnTable.Visible = true;
        //                            break;
        //                        case "POS":
        //                            btnPOS.Visible = true;
        //                            break;
        //                        case "POS Return":
        //                            btnPOSReturn.Visible = true;
        //                            break;
        //                        case "Report":
        //                            btnReports.Visible = true;
        //                            break;
        //                        case "Expense":
        //                            btnExpense.Visible = true;
        //                            break;
        //                        case "Bill":
        //                            btnBill.Visible = true;
        //                            break;
        //                        default:
        //                            btnSeting.Visible = true;
        //                            break;
        //                    }



        //                }
        //                btnSeting.Visible = true;
        //            }
        //        }
        //    }
        //}

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void CenterPanel_Paint_1(object sender, PaintEventArgs e)
        {

        }

        private void btnTable_Click(object sender, EventArgs e)
        {
            string btnName = btnTable.Text;
            AddControl(new FormTableView(btnName));
        }

        private void btnStaff_Click(object sender, EventArgs e)
        {
            string btnName = btnStaff.Text;
            AddControl(new FormStaffView(btnName));
        }

        private void btnProduct_Click(object sender, EventArgs e)
        {
            string btnName = btnProduct.Text;
            AddControl(new FormProductView(btnName));
        }

        private void btnPOS_Click(object sender, EventArgs e)
        {
            FormPOS pos = new FormPOS();
            pos.Show();
        }

        private void btnKitchen_Click(object sender, EventArgs e)
        {
            
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            AddControl(new FormReports());
        }

        private void btnVender_Click(object sender, EventArgs e)
        {
            string btnName = btnVender.Text;
            AddControl(new FormVenderView(btnName));
        }

        private void btnSeting_Click(object sender, EventArgs e)
        {
            string btnName = btnSeting.Text;
            AddControl(new FormSetting(btnName));
        }

        private void guna2Button1_Click_1(object sender, EventArgs e)
        {
            FormPosReturn pos = new FormPosReturn();
            pos.ShowDialog();
        }

        private void btnExpense_Click(object sender, EventArgs e)
        {
            string btnName = btnExpense.Text;
            AddControl(new ExpenseView(btnName));
        }

        private void btnBill_Click(object sender, EventArgs e)
        {
            string btnName = btnBill.Text;
            AddControl(new FormBillView(btnName));
        }

        private void guna2Button1_Click_2(object sender, EventArgs e)
        {
            string btnName = btnCash.Text;
            AddControl(new FormCashinView(btnName));
        }
    }
}
