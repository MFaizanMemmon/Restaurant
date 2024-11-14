﻿using Restaurant.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Restaurant.View
{
    public partial class FormBillView : SampleView
    {
        public FormBillView(string btnText)
        {
            InitializeComponent();
            label2.Text = btnText;
        }

        private void btnAdd_Click_1(object sender, EventArgs e)
        {
            FormBill pos = new FormBill();
            pos.Show();
        }
    }
}
