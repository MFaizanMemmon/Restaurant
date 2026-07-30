using CrystalDecisions.CrystalReports.Engine;
using System;
using System.Drawing.Printing;

namespace Restaurant
{
    internal static class PrintService
    {
        public static bool PrintOrPreview(
            ReportDocument report,
            PrinterSettings printerSettings,
            PageSettings pageSettings,
            string previewTitle)
        {
            using (FormPrint previewForm = new FormPrint())
            {
                previewForm.Text = string.IsNullOrWhiteSpace(previewTitle)
                    ? "Print Preview"
                    : previewTitle;
                previewForm.crystalReportViewer1.ReportSource = report;
                previewForm.crystalReportViewer1.RefreshReport();
                previewForm.ShowDialog();
                previewForm.crystalReportViewer1.ReportSource = null;
            }

            return false;
        }
    }
}
