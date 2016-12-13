using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace r2pipe_test
{
    public partial class convertFrm : Form
    {
        const string welcome_message    = "welcome to r4w gui powered by radare2";
        public string seek_address      = "0";
        public string blocksize         = "128";
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
            btnConvert.Enabled = true;
            switch (lstOperations.SelectedItems[0].Text)
            {
                case "To Hexdump":
                    command = "px " + this.blocksize + " @ " + this.seek_address;
                    break;
                case "To Dissasembly":
                    command = "pd " + this.blocksize + " @ " + this.seek_address;
                    break;
                case "To MD5 hash":
                    command = "ph md5 " + this.blocksize + " @ " + this.seek_address;
                    break;
                case "ASM to Hexpairs":
                    if (txtInput.Text.Equals(welcome_message))
                        txtInput.Text = "nop";
                    command = "pa " + txtInput.Text;
                    break;
                case "Hexpairs to ASM":
                    if (txtInput.Text.Equals(welcome_message) || !Regex.IsMatch(txtInput.Text, @"^[0-9a-f\s]+$"))
                        txtInput.Text = "90";
                    command = "pad " + txtInput.Text;
                    break;
                default:
                    break;
            }
            txtCommands.Text = txtCommands.Text.TrimEnd('\r').TrimEnd('\n');
            if (txtCommands.Text.Length > 0) 
                txtCommands.Text += Environment.NewLine;
            txtCommands.Text += command + Environment.NewLine;
        }
        private void btnConvert_Click(object sender, EventArgs e)
        {
            string input = txtInput.Text;
            string write_cmd = cmdWriteInput.Text;
            if( write_cmd.Length>0 )
                r2pw.run(write_cmd);
            txtOutput.Text = "";
            foreach (string cmd in txtCommands.Text.Split('\n'))
            {
                if (cmd.Length > 0)
                {
                    input = r2pw.run(cmd).TrimEnd('\r').TrimEnd('\n');
                    if (input.Length > 0)
                    {
                        txtOutput.Text += input + Environment.NewLine;
                    }
                }
            }
            if ( txtOutput.TextLength > 0 )
            {
                txtOutput.Focus();
                txtOutput.SelectionStart = txtOutput.TextLength - 1;
                txtOutput.SelectionLength = 0;
            }
        }
        private void init_r2_commands()
        {
            if (txtInput.TextLength == 0)
            {
                cmdWriteInput.Clear();
                return;
            }
            cmdWriteInput.Text = "w0 " + this.blocksize + " @ " + this.seek_address + "; ";
            cmdWriteInput.Text += "w " + txtInput.Text + " @ " + this.seek_address + Environment.NewLine;
        }
        private void convertFrm_Load(object sender, EventArgs e)
        {
            Color foreColor = Color.FromName(r2pw.rconfig.load<string>("gui.output.fg", "black"));
            Color backColor = r2pw.theme_background();
            Color foreColor_lbl = Color.FromName(r2pw.rconfig.load<string>("gui.output.fg.sym", "black"));
            this.ForeColor = foreColor;
            this.BackColor = backColor;
            lstOperations.BackColor = backColor;
            lstOperations.ForeColor = foreColor;
            txtCommands.BackColor = backColor;
            txtCommands.ForeColor = foreColor;
            txtInput.BackColor = backColor;
            txtInput.ForeColor = foreColor;
            cmdWriteInput.BackColor = backColor;
            cmdWriteInput.ForeColor = foreColor;
            txtSeekAddress.BackColor = backColor;
            txtSeekAddress.ForeColor = foreColor;
            txtBlockSize.BackColor = backColor;
            txtBlockSize.ForeColor = foreColor;
            txtOutput.BackColor = backColor;
            txtOutput.ForeColor = foreColor;
            btnConvert.BackColor = foreColor;
            btnConvert.ForeColor = backColor;
            lblInput.ForeColor = foreColor_lbl;
            lblSeek.ForeColor = foreColor_lbl;
            lblBlock.ForeColor = foreColor_lbl;
            lblOutput.ForeColor = foreColor_lbl;
            lblCommands.ForeColor = foreColor_lbl;
            lstOperations.Items.Add("To Hexdump");
            lstOperations.Items.Add("To Dissasembly");
            lstOperations.Items.Add("To MD5 hash");
            lstOperations.Items.Add("ASM to Hexpairs");
            lstOperations.Items.Add("Hexpairs to ASM");
            init_r2_commands();
        }
        private void txtInput_TextChanged(object sender, EventArgs e)
        {
            init_r2_commands();
        }
        private void txtSeekAddress_TextChanged(object sender, EventArgs e)
        {
            if ( txtSeekAddress.TextLength == 0 ) return;
            this.seek_address = txtSeekAddress.Text;
        }
        private void txtBlockSize_TextChanged(object sender, EventArgs e)
        {
            if ( txtBlockSize.TextLength == 0 ) return;
            this.blocksize = txtBlockSize.Text;
        }
        private void txtCommands_KeyDown(object sender, KeyEventArgs e)
        {
            btnConvert.Enabled = true;
        }
    }
}
