using CrystalDecisions.CrystalReports.Engine;
using Restaurant.Reportss;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Restaurant
{
    public partial class FormCashLedger : Form
    {
        public FormCashLedger()
        {
            InitializeComponent();
        }

        private void btnReport_Click(object sender, EventArgs e)
        {
            string query = @"
                WITH CashLedger AS
                (
                    -- Expenses
                    SELECT 
                        CAST(ExpDate AS DATETIME) AS LedgerDateTime,
                        Notes AS Description,
                        0 AS Debit,
                        Amount AS Credit,
                        NULL AS Balance,
                        CAST(ExpID AS VARCHAR) AS ID
                    FROM tblExpence

                    UNION ALL

                    -- Sales
                    SELECT 
                        CAST(Date AS DATETIME) + CAST(Time AS DATETIME) AS LedgerDateTime,
                        'Sale' AS Description,
                        Total AS Debit,
                        0 AS Credit,
                        NULL AS Balance,
                        CAST(MainID AS VARCHAR) AS ID
                    FROM tblMain

                    UNION ALL

                    -- Sales Returns
                    SELECT 
                        CAST(Date AS DATETIME) + CAST(Time AS DATETIME) AS LedgerDateTime,
                        'Sale Return' AS Description,
                        Total AS Debit,
                        0 AS Credit,
                        NULL AS Balance,
                        CAST(MainID AS VARCHAR) AS ID
                    FROM tblMainReturn

                    UNION ALL

                    -- Cash Inflows
                    SELECT 
                        CAST(DateTime AS DATETIME) AS LedgerDateTime,
                        Notes AS Description,
                        Amount AS Debit,
                        0 AS Credit,
                        NULL AS Balance,
                        CAST(CashID AS VARCHAR) AS ID
                    FROM tblCashIn
                )
                -- Select with Running Balance
                SELECT 
                    LedgerDateTime as 'Date',
                    Description,
                    Debit as 'DR',
                    Credit as 'CR',
                    ISNULL(SUM(Debit - Credit) OVER (ORDER BY LedgerDateTime ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW), 0) AS Balance,
                    ID
                FROM CashLedger
             
                ";


            // Initialize SqlConnection and SqlCommand
            using (SqlConnection con = new SqlConnection(MainClass.con.ConnectionString))
            {
                SqlCommand cmd = new SqlCommand(query, con);
                //cmd.Parameters.AddWithValue("@StartDateParam", dateTimePicker1.Value.Date);
                //cmd.Parameters.AddWithValue("@EndDateParam", dateTimePicker2.Value.Date);

                // Open the connection
                con.Open();

                // Create DataSet and fill it
                DataSet dataSet = new DataSet();
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dataSet, "CashLedger");

                // Close the connection
                con.Close();

                // Load the report and set the data source
                ReportDocument reportDocument = new ReportDocument();


                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string reportFolder = Path.GetFullPath(Path.Combine(baseDirectory, @"..\..\"));
                string reportPath = Path.Combine(reportFolder, @"Reportss\crptCashLedger.rpt");


                // Dynamic path to the report file
              
                reportDocument.Load(reportPath);

                reportDocument.SetDataSource(dataSet.Tables["CashLedger"]);

                // Show the report in the form
                FormPrint frm = new FormPrint();
                frm.crystalReportViewer1.ReportSource = reportDocument;
                frm.crystalReportViewer1.Refresh();
                frm.Show();
            }
        }

        private void FormCashLedger_Load(object sender, EventArgs e)
        {

        }
    }
}
