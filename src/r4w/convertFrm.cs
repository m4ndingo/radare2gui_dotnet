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
        const string welcome_message    = "Welcome to r4w gui powered by radare2";
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
                case "To Entropy value":
                    command = "ph entropy " + this.blocksize + " @ " + this.seek_address;
                    break;
                case "To C array":
                    command = "pc " + this.blocksize + " @ " + this.seek_address;
                    break;
                case "To Base64":
                    command = "p6e " + this.blocksize + " @ " + this.seek_address;
                    break;
                case "From Base64":
                    command = "p6d " + this.blocksize + " @ " + this.seek_address;
                    break;
                case "ASM to Hexpairs":
                    if (txtInput.Text.Equals(welcome_message) || !Regex.IsMatch(txtInput.Text, @"^[0-9a-f\s]+$"))
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
            Color foreColor_lbl = Color.FromName(r2pw.rconfig.load<string>("gui.output.fg", "black"));
            Color backcolor_lst = Color.FromName(r2pw.rconfig.load<string>("gui.output.bg.imp", "white")); 
            this.ForeColor = foreColor;
            this.BackColor = backColor;
            lstOperations.BackColor = backColor;
            lstOperations.ForeColor = foreColor;
            txtCommands.BackColor = backcolor_lst;
            txtCommands.ForeColor = foreColor;
            txtInput.BackColor = backColor;
            txtInput.ForeColor = foreColor;
            cmdWriteInput.BackColor = backcolor_lst;
            cmdWriteInput.ForeColor = foreColor;
            txtSeekAddress.BackColor = backcolor_lst;
            txtSeekAddress.ForeColor = foreColor;
            txtBlockSize.BackColor = backcolor_lst;
            txtBlockSize.ForeColor = foreColor;
            txtOutput.BackColor = backColor;
            txtOutput.ForeColor = foreColor;
            //btnConvert.BackColor = foreColor;
            //btnConvert.ForeColor = backColor;
            lstOperations.BackColor = backcolor_lst;
            lblInput.ForeColor = foreColor_lbl;
            lblSeek.ForeColor = foreColor_lbl;
            lblBlock.ForeColor = foreColor_lbl;
            lblOutput.ForeColor = foreColor_lbl;
            lblCommands.ForeColor = foreColor_lbl;
            lstOperations.Items.Add("To Hexdump");
            lstOperations.Items.Add("To Dissasembly");
            lstOperations.Items.Add("To Base64");
            lstOperations.Items.Add("From Base64");
            lstOperations.Items.Add("To C array");
            lstOperations.Items.Add("To MD5 hash");
            lstOperations.Items.Add("To Entropy value");            
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
        private void btnClear_Click(object sender, EventArgs e)
        {
            txtCommands.Clear();
            txtOutput.Clear();
            if (MessageBox.Show("Clear input?", "Clear", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                txtInput.Clear();
                txtInput.Focus();
            }else
            {
                txtCommands.Focus();
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }
        private void txtSeekAddress_TextChanged_1(object sender, EventArgs e)
        {
            this.seek_address = txtSeekAddress.Text;
        }
        private void txtBlockSize_TextChanged_1(object sender, EventArgs e)
        {
            this.blocksize = txtBlockSize.Text;
        }
    }
}
