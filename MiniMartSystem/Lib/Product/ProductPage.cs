using MiniMartSystem.DotNunServer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiniMartSystem.Lib.Product
{
    public partial class ProductPage : Form
    {
        public ProductPage()
        {
            InitializeComponent();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {


            AddProduct add = new AddProduct();
            add.Show();
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {

        }

        private void ProductPage_Load(object sender, EventArgs e)
        {
            NunCMD cmd = new NunCMD(dbConnect.conStr);
            //dgvPro.DataSource = cmd.SelectOneRowRecord("tbl_product", "pro_name", "pro_photo", "pro_id", "6");
            //dgvPro.DataSource = cmd.SelectAllNomalRecord("tbl_product", "pro_name", "pro_photo");

        }
    }
}
