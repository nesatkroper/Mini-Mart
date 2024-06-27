using MiniMartSystem.DotNunServer;
using System;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace MiniMartSystem.Lib.Product
{
    public partial class AddProduct : Form
    {
        public AddProduct()
        {
            InitializeComponent();
        }

        string dirName = "Product Picture";
        string picture = "";

        private void AddProduct_Load(object sender, EventArgs e)
        {

        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            NunCMD cmd = new NunCMD(dbConnect.conStr);
            cmd.AddNomalRecord("tbl_product", "pro_name", txtName,"cat_id", txtCode,"pro_qty",cbCategory);

            DialogResult = DialogResult.OK;
            //this.Close();
        }

        private void pcbItem_Click(object sender, EventArgs e)
        {
            NunCMD cmd = new NunCMD(dbConnect.conStr);

            string a = cmd.BrowseImage_Location();
            pcbItem.ImageLocation = a;
            picture = cmd.CopyImage_ToDestination(dirName, a);
        }
    }
}
