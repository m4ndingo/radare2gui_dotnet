using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace r2pipe_test
{
    public partial class convertFrm : Form
    {
        private R2PIPE_WRAPPER r2pw = null;
        public convertFrm(R2PIPE_WRAPPER r2pw)
        {
            this.r2pw = r2pw;
            InitializeComponent();
        }

        private void lstOperations_DoubleClick(object sender, EventArgs e)
        {
            string command = "";
            if (lstOperations.SelectedItems.Count == 0) return;
            switch (lstOperations.SelectedItems[0].Text)
            {
                case "To Hexdump":
                    command = "px";
                    break;
                default:
                    break;
            }
            txtCommands.Text += command + Environment.NewLine;
        }

        private void txtInput_TextChanged(object sender, EventArgs e)
        {
        }

        private void btnConvert_Click(object sender, EventArgs e)
        {
            r2pw.run("w " + txtInput.Text + " @ 0");
            foreach (string cmd in txtCommands.Text.Split('\n'))
            {
                if (cmd.Length > 0)
                {
                    string res = r2pw.run(cmd).Replace("\r","").Replace("\n","");
                    txtOutput.Text = res;
                    r2pw.run("w " + res + " @ 0");
                }
            }
        }
        private void txtCommands_TextChanged(object sender, EventArgs e)
        {
            btnConvert.Enabled = txtCommands.Text.Length > 0;
        }
    }
}
