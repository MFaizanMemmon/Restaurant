using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Restaurant.Model
{
    public partial class FormAddCustomer : Form
    {
        public FormAddCustomer()
        {
            InitializeComponent();
        }

        public string OrderType = "";
        public int DriverID = 0;
        public string CustomerName = "";
        public int MainID = 0;
        private void FormAddCustomer_Load(object sender, EventArgs e)
        {
            if (OrderType == "Take Away")
            {
                lblDriver.Visible = false;
                cbDriver.Visible = false;
            }

            string qry = "select StaffID 'id', StaffName 'name' from Staff where StaffRole = 'Driver' ";
            MainClass.CBFill(qry, cbDriver);

            if (MainID >0)
            {
                cbDriver.SelectedValue = DriverID;
            }

        }

        private void cbDriver_SelectedIndexChanged(object sender, EventArgs e)
        {
            DriverID = Convert.ToInt32(cbDriver.SelectedValue);
        }
    }
}
