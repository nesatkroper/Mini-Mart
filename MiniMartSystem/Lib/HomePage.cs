using MiniMartSystem.DotNunServer;
using MiniMartSystem.Lib.Product;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiniMartSystem.Lib
{
    public partial class HomePage : Form
    {
        public HomePage()
        {
            InitializeComponent();
        }

        NunCMD cmd = new NunCMD();
        int num = 10;

        void ButtonStyle(object sender)
        {
            foreach (Control control in pnSidebar.Controls)
            {
                control.BackColor = Color.FromArgb(107, 138, 122);
                control.Font = new Font("Poppins", 12f, FontStyle.Regular);
            }
            Control click = (Control)sender;
            click.BackColor = Color.FromArgb(37, 67, 54);
            click.Font = new Font("Poppins", 12f, FontStyle.Bold);
        }

        private void HomePage_Load(object sender, EventArgs e)
        {

        }

        private void HomePage_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void btnDash_Click(object sender, EventArgs e)
        {
            ButtonStyle(btnDash);
        }

        private void btnRep_Click(object sender, EventArgs e)
        {

            ButtonStyle(btnRep);

        }

        private void btnCat_Click(object sender, EventArgs e)
        {
            ButtonStyle(btnCat);
        }

        private void btnEmp_Click(object sender, EventArgs e)
        {
            ButtonStyle(btnEmp);
        }

        private void btnCus_Click(object sender, EventArgs e)
        {
            ButtonStyle(btnCus);
        }

        private void btnQuo_Click(object sender, EventArgs e)
        {
            ButtonStyle(btnQuo);
        }

        private void btnExp_Click(object sender, EventArgs e)
        {
            ButtonStyle(btnExp);
        }

        private void btnPro_Click(object sender, EventArgs e)
        {
            ButtonStyle(btnPro);

            ProductPage page = new ProductPage();
            page.TopLevel = false;
            page.Dock = DockStyle.Fill;
            pnMain.Controls.Add(page);
            page.Show();
            page.BringToFront();
        }

        private void btnPos_Click(object sender, EventArgs e)
        {
            ButtonStyle(btnPos);
        }

        private void btnSet_Click(object sender, EventArgs e)
        {
            ButtonStyle(btnSet);
        }

        private void tmTime_Tick(object sender, EventArgs e)
        {
            lbDate.Text = DateTime.Now.ToString("D");
            lbTime.Text = DateTime.Now.ToString("T");
        }
    }
}
