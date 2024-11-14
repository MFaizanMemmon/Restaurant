using System;
using System.Collections;
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
    public partial class FormVenderAdd : SampleAdd
    {
        public FormVenderAdd()
        {
            InitializeComponent();
        }

        public int id = 0;
        public override void btnSave_Click(object sender, EventArgs e)
        {
            string qry = "";

            if (id == 0)
            {
                qry = "Insert into TblVender (Name,PhoneNO,Address,OpeningBalance,Description) values (@Name,@PhoneNO,@Address,@OpeningBalance,@Description)";
            }
            else
            {
                qry = "update TblVender set Name = @Name , PhoneNO = @PhoneNO, Address = @Address, OpeningBalance = @OpeningBalance, Description = @Description   where VenderID = @id";
              
            }

            Hashtable ht = new Hashtable();
            ht.Add("@id", id);
            ht.Add("@Name", txtName.Text);
            ht.Add("@PhoneNO", txtPhone.Text);
            ht.Add("@Address", txtAddress.Text);
            ht.Add("@OpeningBalance", txtOPeningBalance.Text);
            ht.Add("@Description", txtDescription.Text);

            if (MainClass.SQL(qry, ht) > 0)
            {
                MessageBox.Show("Operation hass been Successfully");
                id = 0;
                txtName.Text = "";
                txtName.Focus();
                this.Close();
            }
        }
    }
}
