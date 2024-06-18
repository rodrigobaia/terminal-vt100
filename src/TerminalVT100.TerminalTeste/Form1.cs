using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TerminalVT100.TerminalTeste
{
    public partial class Form1 : Form
    {
        private TedVT100Server TedVT100Service;

        public Form1()
        {
            InitializeComponent();
        }

        private void BtnConectar_Click(object sender, EventArgs e)
        {
            var port = Convert.ToInt32(TxtPorta.Text);
            TedVT100Service = new TedVT100Server();
        }
    }
}
