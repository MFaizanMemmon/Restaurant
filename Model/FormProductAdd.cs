using System;
using System.Collections;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Restaurant.Model
{
    public partial class FormProductAdd : SampleAdd
    {
        private int _CategoryId { get; set; }
        public FormProductAdd(int CategoryId)
        {
            InitializeComponent();
            _CategoryId = CategoryId;
            txtPrice.Focus();
        }

        public int id = 0;
        public int cid = 0;
        private void FormProductAdd_Load(object sender, EventArgs e)
        {
           
            string qry = "select CategoryID AS id, CategoryName AS [name] from Category ";

            MainClass.CBFill(qry, cbCateory);

            //cbCateory.SelectedValue = cid;
            cbCateory.SelectedValue = _CategoryId;

            if (id > 0)
            {
                ForUpdateLoadData();
            }
        }

        string filepath;
        byte[] ImageByteArray;
        internal object txtDescription;

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "images(.jpg, .png)|* .png; * .jpg";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                filepath = ofd.FileName;
                txtimage.Image = new Bitmap(filepath);
            }

        }

        public override void btnSave_Click(object sender, EventArgs e)
        {
            decimal price;
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Please enter the product name.");
                txtName.Focus();
                return;
            }
            if (!decimal.TryParse(txtPrice.Text, out price))
            {
                MessageBox.Show("Please enter a valid product price.");
                txtPrice.Focus();
                return;
            }
            if (cbCateory.SelectedValue == null ||
                !int.TryParse(cbCateory.SelectedValue.ToString(), out int categoryId))
            {
                MessageBox.Show("Please select a category.");
                cbCateory.Focus();
                return;
            }

            using (var stream = new MemoryStream())
            using (var image = new Bitmap(txtimage.Image))
            {
                image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                ImageByteArray = stream.ToArray();
            }

            string qry = id == 0
                ? "INSERT INTO Product (ProductName, ProductPrice, CategoryID, ProductImage) VALUES (?, ?, ?, ?)"
                : "UPDATE Product SET ProductName = ?, ProductPrice = ?, CategoryID = ?, ProductImage = ? WHERE ProductID = ?";

            int affected;
            using (var connection = new OleDbConnection(MainClass.con_string))
            using (var command = new OleDbCommand(qry, connection))
            {
                command.Parameters.Add("@Name", OleDbType.VarWChar, 150).Value = txtName.Text.Trim();
                command.Parameters.Add("@Price", OleDbType.Currency).Value = price;
                command.Parameters.Add("@CategoryID", OleDbType.Integer).Value = categoryId;
                command.Parameters.Add("@Image", OleDbType.LongVarBinary).Value = ImageByteArray;
                if (id != 0)
                    command.Parameters.Add("@ProductID", OleDbType.Integer).Value = id;

                connection.Open();
                affected = command.ExecuteNonQuery();
            }

            if (affected > 0)
            {
                MessageBox.Show("Operation has been successfully done", "Notification");
                id = 0;
                cid = 0;
                txtName.Clear();
                txtPrice.Clear();
                cbCateory.SelectedIndex = -1;
                txtimage.Image = Restaurant.Properties.Resources.product_removebg_preview__1_;
                Close();
            }
        }

        private void ForUpdateLoadData()
        {
            DataTable dt = new DataTable();
            using (var connection = new OleDbConnection(MainClass.con_string))
            using (var cmd = new OleDbCommand("SELECT * FROM Product WHERE ProductID = ?", connection))
            using (var da = new OleDbDataAdapter(cmd))
            {
                cmd.Parameters.Add("@ProductID", OleDbType.Integer).Value = id;
                da.Fill(dt);
            }

            if (dt.Rows.Count > 0)
            {
                txtName.Text = dt.Rows[0]["ProductName"].ToString();
                txtPrice.Text = dt.Rows[0]["ProductPrice"].ToString();
                cbCateory.SelectedValue = Convert.ToInt32(dt.Rows[0]["CategoryID"]);

                if (dt.Rows[0]["ProductImage"] != DBNull.Value)
                {
                    byte[] imageByteArray = (byte[])dt.Rows[0]["ProductImage"];
                    using (var stream = new MemoryStream(imageByteArray))
                    using (var source = Image.FromStream(stream))
                    {
                        txtimage.Image = new Bitmap(source);
                    }
                }

            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
