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

namespace Restaurant.Reportss
{
    public partial class FormSaleByCategory : Form
    {
        public FormSaleByCategory()
        {
            InitializeComponent();
        }

        private void btnReport_Click(object sender, EventArgs e)
        {
            string qry = @"select 
    m.MainID, 
    m.Date, 
    m.Time, 
    p.ProductName, 
    d.qty as 'Qty', 
    p.ProductPrice as 'Price', 
    (d.qty * p.ProductPrice) as 'Amount'
   
from 
    TblMain m
inner join 
    tblOrderLog d on m.MainID = d.MainID
inner join 
    Product p on p.ProductID = d.itemid
inner join 
    Category c on c.CategoryID = p.CategoryID
where 
    d.Isdeleted = 0


";

            // Create a SqlCommand object
            SqlCommand cmd = new SqlCommand(qry, MainClass.con);

            MainClass.con.Open();

            // Create a new DataSet (assuming you have a dataset defined in your project)
            DSDeleteBill ds = new DSDeleteBill();

            // Use SqlDataAdapter to fill the dataset
            SqlDataAdapter adpt = new SqlDataAdapter(cmd);
            adpt.Fill(ds, "DtDeleteOrSaleReport");

            MainClass.con.Close();

            // Create an instance of your Crystal Report
            rptSaleByCategory cr = new rptSaleByCategory();

            // Set the dataset as the data source for the report
            cr.SetDataSource(ds.Tables["DtDeleteOrSaleReport"]);

            // Load the report in the CrystalReportViewer
            FormPrint frm = new FormPrint();
            frm.crystalReportViewer1.ReportSource = cr;
            frm.crystalReportViewer1.Refresh();
            frm.Show();


            
          
        }
    }
}
