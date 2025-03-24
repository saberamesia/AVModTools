using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AVModTools
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            pathBox.Text = System.IO.Directory.GetCurrentDirectory();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (true)
            {
                if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                {
                    pathBox.Text = folderBrowserDialog1.SelectedPath;
                }
            }
        }

        private void folderBrowserDialog1_HelpRequest(object sender, EventArgs e)
        {

        }

        private async void button1_Click_1(object sender, EventArgs e)
        {
            patchButton.Text = "Patching...";
            await Task.Run(() => ModTools.InstallModLoader(pathBox.Text));
            patchButton.Text = "Patched!";
        }
    }
}
