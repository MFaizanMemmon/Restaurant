using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
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
           
            string qry = "select CategoryID  'id', CategoryName  'name' from Category ";

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
            string qry = "";

            if (id == 0)
            {
                qry = "Insert into Product values (@Name , @Price , @Cat, @Img)";
            }
            else
            {
                qry = "update Product set ProductName = @Name, ProductPrice = @Price, CategoryID = @cat, ProductImage = @img  where ProductID = @id";
               // guna2MessageDialog1.Show("Update Succesfull");
            }
            // for Image 
            Image temp = new Bitmap(txtimage.Image);
            MemoryStream ms = new MemoryStream();
            temp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ImageByteArray = ms.ToArray();

            Hashtable ht = new Hashtable();
            ht.Add("@id", id);
            ht.Add("@Name", txtName.Text);
            ht.Add("@Price", txtPrice.Text);
            ht.Add("@Cat", Convert.ToInt32(cbCateory.SelectedValue));
            ht.Add("@Img", ImageByteArray);


            if (MainClass.SQL(qry, ht) > 0)
            {
                MessageBox.Show("Operation has been successfully done", "Notification");
                id = 0;
                cid = 0;
                txtName.Text = "";
                txtPrice.Text = "";
                cbCateory.SelectedValue = 0;
                cbCateory.SelectedValue = -1;
                txtimage.Image = Restaurant.Properties.Resources.product_removebg_preview__1_;
                this.Close();
            }
        }

        private void ForUpdateLoadData()
        {
            string qry = @"select * from Product where ProductiD = '" + id + "' ";
            SqlCommand cmd = new SqlCommand(qry, MainClass.con);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);

            if (dt.Rows.Count > 0)
            {
                txtName.Text = dt.Rows[0]["ProductName"].ToString();
                txtPrice.Text = dt.Rows[0]["ProductPrice"].ToString();

                byte[] ImageArray = (byte[])(dt.Rows[0]["ProductImage"]);
                byte[] imageByteArray = ImageArray;
                txtimage.Image = Image.FromStream(new MemoryStream(imageByteArray));

            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}