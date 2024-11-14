using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Restaurant
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (MainClass.invaliduser(txtLogin.Text, txtPassward.Text) == false)
            {
                guna2MessageDialog1.Show("Invalid UserName Or Passward");
                return;
            }
            else
            {
                this.Hide();
                MainForm frm = new MainForm();
                frm.Show();
            }

        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void txtPassward_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                // Prevent the "ding" sound when pressing Enter
                e.SuppressKeyPress = true;

                // Trigger the button click event
                btnLogin_Click(null, null);
            }
        }

        private void txtLogin_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                // Prevent the "ding" sound when pressing Enter
                e.SuppressKeyPress = true;

                txtPassward.Focus();
            }
        }
    }
}
