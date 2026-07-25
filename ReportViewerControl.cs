using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Restaurant
{
    // Lightweight report host used when the separately-installed SAP Crystal
    // WinForms viewer assembly is unavailable. ReportDocument remains fully
    // supported by CrystalDecisions.CrystalReports.Engine.
    public sealed class ReportViewerControl : Panel
    {
        private object reportSource;

        public ReportViewerControl()
        {
            BackColor = Color.White;
            AutoScroll = true;
        }

        [Browsable(false)]
        public int ActiveViewIndex { get; set; }

        [DefaultValue(false)]
        public bool DisplayStatusBar { get; set; }

        [Browsable(false)]
        public object ReportSource
        {
            get { return reportSource; }
            set
            {
                reportSource = value;
                Invalidate();
            }
        }
    }
}
