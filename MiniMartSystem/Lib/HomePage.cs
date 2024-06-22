using MiniMartSystem.Lib.Product;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
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

        void ButtonStyle(object sender, EventArgs e)
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
            ButtonStyle(btnDash, null);
        }

        private void btnRep_Click(object sender, EventArgs e)
        {
            ButtonStyle(btnRep, null);
        }

        private void btnCat_Click(object sender, EventArgs e)
        {
            ButtonStyle(btnCat, null);
        }

        private void btnEmp_Click(object sender, EventArgs e)
        {
            ButtonStyle(btnEmp, null);
        }

        private void btnCus_Click(object sender, EventArgs e)
        {
            ButtonStyle(btnCus, null);
        }

        private void btnQuo_Click(object sender, EventArgs e)
        {
            ButtonStyle(btnQuo, null);
        }

        private void btnExp_Click(object sender, EventArgs e)
        {
            ButtonStyle(btnExp, null);
        }

        private void btnPro_Click(object sender, EventArgs e)
        {
            ButtonStyle(btnPro, null);

            ProductPage page = new ProductPage();
            page.TopLevel = false;
            page.Dock = DockStyle.Fill;
            pnMain.Controls.Add(page);
            page.Show();
            page.BringToFront();
        }

        private void btnPos_Click(object sender, EventArgs e)
        {
            ButtonStyle(btnPos, null);
        }

        private void btnSet_Click(object sender, EventArgs e)
        {
            ButtonStyle(btnSet, null);
        }
    }
}
