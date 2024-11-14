using Guna.UI2.WinForms;
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
    public partial class SampleView : Form
    {
        public SampleView()
        {
            InitializeComponent();
            guna2MessageDialog1.Caption = "Notification";
            guna2MessageDialog1.Text = "Update Successful";
            guna2MessageDialog1.Icon = MessageDialogIcon.Information; // You can set other icons like Error, Warning, etc.
            guna2MessageDialog1.Buttons = MessageDialogButtons.OK;
            guna2MessageDialog1.Style = MessageDialogStyle.Light; // You can set the style (Light/Dark)
        }

        public virtual void btnAdd_Click(object sender, EventArgs e)
        {

        }

        public virtual void txtSearch_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
