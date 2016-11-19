using System;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;

namespace r2pipe_test
{    
    public partial class Form1 : Form
    {        
        R2PIPE_WRAPPER r2pw = null;
        private RConfig rconfig = null;
        private string fileName = null;
        private bool updating_gui = false;
        public TabControl tabcontrol = null;
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            rconfig = new RConfig();
            UpdateGUI();
            CheckR2path();
            r2pw = new R2PIPE_WRAPPER(rconfig, this);
            //assign controls
            r2pw.add_control("output",              txtOutput);
            r2pw.add_control("dissasembly",         webBrowser1);
            r2pw.add_control("strings_listview",    lstStrings);
            r2pw.add_control("functions_listview",  listView1);
            r2pw.add_control("imports_listview",    lstImports);
            r2pw.add_control("sections_listview",   lstSections);
            r2pw.add_control("hexview",             webBrowser2);
            r2pw.add_control("r2help",              webBrowser3);
            //assign menu optrions
            r2pw.add_menucmd("View", "Functions", "afl", mainMenu);
            r2pw.add_menucmd("View", "File info", "iI", mainMenu);
            r2pw.add_menufcn("Gui", "Update gui", "*", UpdateGUI, mainMenu);
            r2pw.add_menufcn("Gui", "Enum registry vars", "*", dumpGuiVars, mainMenu);
            //load some example file
            LoadFile(@"c:\windows\SysWOW64\notepad.exe");            
        }
        private void UpdateGUI(string args=null)
        {
            updating_gui    = true;
            tabcontrol = tabControl1;
            Left            = int.Parse(rconfig.load<int>("gui.left"));
            Top             = int.Parse(rconfig.load<int>("gui.top"));
            Width           = int.Parse(rconfig.load<int>("gui.width"));
            Height          = int.Parse(rconfig.load<int>("gui.height"));
            txtOutput.BackColor = Color.FromName(rconfig.load<string>("gui.output.bg", "blue"));
            txtOutput.ForeColor = Color.FromName(rconfig.load<string>("gui.output.fg", "white"));
            listView1.BackColor = txtOutput.BackColor;
            listView1.ForeColor = txtOutput.ForeColor;
            splitContainer1.Panel1.BackColor = txtOutput.BackColor;
            splitContainer1.SplitterDistance = int.Parse(rconfig.load<int>("gui.splitter_1.dist"));
            splitContainer2.SplitterDistance = int.Parse(rconfig.load<int>("gui.splitter_2.dist"));
            Refresh();
            updating_gui = false;
        }
        private void DoLoadFile()
        {
            if (!File.Exists(fileName))
            {
                r2pw.Show(string.Format("Wops!\n{0}\nfile not found...", fileName), "LoadFile");
                return;
            }
            r2pw.open(fileName);
            r2pw.run("aaa;aflj", "functions_listview", false, new List<string> { "name", "offset" });
            r2pw.run("pd 100", "dissasembly");
            r2pw.run("izj", "strings_listview",false,new List<string> { "vaddr", "section", "type", "string" });
            r2pw.run("iij", "imports_listview", false, new List<string> { "name", "plt" });
            r2pw.run("iSj", "sections_listview", false, new List<string> { "name", "size", "flags", "paddr", "vaddr" });
            r2pw.run("px 2000", "hexview");
            r2pw.run("?", "r2help");
        }
        private void CheckR2path()
        {
            string r2path = rconfig.r2path;
            if (r2path == null || !File.Exists(rconfig.r2path))
            {
                if(((object)r2pw)!=null) r2pw.Show("Form1: CheckR2path(): Path for 'radare2.exe' not found...", "radare2.exe not found");
                openFileDialog1.FileName = "radare2.exe";
                openFileDialog1.Title = "Please, locate your radare2.exe binary";
                openFileDialog1.ShowDialog();
                rconfig.save("r2path", openFileDialog1.FileName);
            }
        }
        private void LoadFile(String fileName)
        {
            Text = String.Format("r2pipe gui .net v1.0alpha - {0}", fileName);
            if (!File.Exists(rconfig.r2path))
            {
                CheckR2path();
                return;
            }
            this.fileName = fileName;
            Thread newThread = new Thread(new ThreadStart(this.DoLoadFile));
            newThread.Start();
            cmbCmdline.Focus();
        }
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = r2pw.fileName;
            openFileDialog1.ShowDialog();
            if(openFileDialog1.FileName.Length>0)
                LoadFile(openFileDialog1.FileName);
        }
        private void txtOutput_TextChanged(object sender, EventArgs e)
        {
            if (txtOutput.TextLength > 0)
            {
                txtOutput.SelectionStart = txtOutput.TextLength;
                txtOutput.SelectionLength = 0;
                txtOutput.Focus();
                cmbCmdline.Focus();
            }
        }
        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            txtOutput.Clear();
        }
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(r2pw!=null) r2pw.exit();
            Environment.Exit(0);
        }
        private void cmbCmdline_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                cmbCmdline.Items.Remove("");
                cmbCmdline.Items.Add(cmbCmdline.Text);
                r2pw.run(cmbCmdline.Text, "output", true); // append
                cmbCmdline.Items.Add("");
                cmbCmdline.Text = "";
            }
        }
        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            string msg=((ListView)sender).SelectedItems[0].Text;
            string res=r2pw.run("? " + msg);
            string address=res.Split(' ')[1];
            r2pw.run("pd @" + address, "dissasembly");
            r2pw.run("px 2000 @" + address, "hexview");
            //((WebBrowser)r2pw.controls["dissasembly"]).Focus();
        }
        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            save_gui_config();
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            r2pw.exit();
        }
        private void save_gui_config()
        {
            if (updating_gui) return;
            rconfig.save("gui.left", Left);
            rconfig.save("gui.top", Top);
            rconfig.save("gui.width", Width);
            rconfig.save("gui.height", Height);
            rconfig.save("gui.splitter_1.dist",splitContainer1.SplitterDistance);
            rconfig.save("gui.splitter_2.dist",splitContainer2.SplitterDistance);
            rconfig.save("gui.output.bg", txtOutput.BackColor.Name);
            rconfig.save("gui.output.fg", txtOutput.ForeColor.Name);
            UpdateGUI();
        }
        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            save_gui_config();
        }
        private void splitContainer2_SplitterMoved(object sender, SplitterEventArgs e)
        {
            save_gui_config();
        }
        private void dumpGuiVars(string args)
        {
            List<string>keys=rconfig.reg_enumkeys();
            if (args == "*") args = r2pw.Prompt("filter results", "Enum registry vars", args);
            output(string.Format("filter: {0}\n", args));
            foreach (string varname in keys)
            {
                string value = rconfig.load<string>(varname);
                output(string.Format("{0} = {1}\n",varname,value));
            }
        }
        private void dumpGuiVar(string varname)
        {
            string value = rconfig.load<string>(varname);
            output(string.Format("dumpGuiVars(): varname='{0}' value='{1}'", varname, value));
        }
        private void output(string text)
        {
            r2pw.setText("output", text, true);
        }
        private void changeTheme(string themeName)
        {
            if (r2pw != null)
            {
                r2pw.set_theme(themeName);
                r2pw.sendToWebBrowser("dissasembly", null);
                UpdateGUI();
            }
        }
        private void classicToolStripMenuItem_Click(object sender, EventArgs e)
        {
            changeTheme("classic");
        }
        private void blueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            changeTheme("blue");
        }
        private void darkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            changeTheme("pink");
        }
        private void controlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            changeTheme("control");
        }
    }
}
