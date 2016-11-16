using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Collections.Specialized;

namespace r2pipe_test
{    
    public partial class Form1 : Form
    {        
        R2PIPE_WRAPPER r2pw = null;
        private RConfig rconfig = null;
        private string fileName = null;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            rconfig = new RConfig();
            CheckR2path();

            r2pw    = new R2PIPE_WRAPPER(rconfig);
            r2pw.add_control("dissasembly", txtDissasembly);
            r2pw.add_control("strings", txtStrings);
            r2pw.add_control("output", txtOutput);
            r2pw.add_control("functions_listview", listView1);            
            LoadFile(@"c:\windows\SysWOW64\notepad.exe");            
        }
        private void CheckR2path()
        {
            string r2path = rconfig.r2path; 
            if (r2path == null)
            {
                MessageBox.Show("Path for 'radare2.exe' not found...","radare2.exe not found",MessageBoxButtons.OK,MessageBoxIcon.Information);
                openFileDialog1.FileName = "radare2.exe";
                openFileDialog1.Title = "Please, locate your radare2.exe binary";
                openFileDialog1.ShowDialog();
                rconfig.save("r2path", openFileDialog1.FileName);                
            }
        }
        private void DoLoadFile()
        {
            if (!File.Exists(fileName))
            {
                MessageBox.Show(string.Format("Wops!\n{0}\nfile not found...", fileName), "LoadFile", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            //txtOutput.Text = string.Format("Loading '{0}'\n", fileName);
            r2pw.open(fileName);
            r2pw.run("pd 100", "dissasembly");
            r2pw.run("izq", "strings");
            r2pw.run("aaa;aflj", "functions_listview",false,new List<string> { "name", "offset" });
        }
        private void LoadFile(String fileName)
        {
            /*Task.Run(() =>
            {
                DoLoadFile(fileName);
            });*/
            this.fileName = fileName;
            Thread newThread = new Thread(new ThreadStart(this.DoLoadFile));
            newThread.Start();
            Text = String.Format("r2pipe gui .net v1.0 - {0}", fileName);
            cmdline.Focus();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = r2pw.fileName;
            openFileDialog1.ShowDialog();
            if(openFileDialog1.FileName.Length>0)
                LoadFile(openFileDialog1.FileName);
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            r2pw.exit();
        }

        private void cmdline_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                r2pw.run(cmdline.Text, "output", true); // append
                cmdline.Text = "";
            }            
        }

        private void txtOutput_TextChanged(object sender, EventArgs e)
        {
            if (txtOutput.TextLength > 0)
            {
                txtOutput.SelectionStart = txtOutput.TextLength - 1;
                txtOutput.SelectionLength = 0;
                txtOutput.Focus();
                cmdline.Focus();
            }
        }
    }
}
