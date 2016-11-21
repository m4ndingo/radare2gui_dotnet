using System;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;
using System.Text;

namespace r2pipe_test
{    
    public partial class Form1 : Form
    {        
        R2PIPE_WRAPPER r2pw = null;
        private RConfig rconfig = null;
        private string fileName = null;
        private bool updating_gui = false;
        public TabControl tabcontrol = null;
        public string themeName = null;
        public string currentShell = null;
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
            //add controls
            r2pw.add_control("output",              txtOutput);
            r2pw.add_control("dissasembly",         webBrowser1);
            r2pw.add_control("strings_listview",    lstStrings);
            r2pw.add_control("functions_listview",  listView1);
            r2pw.add_control("imports_listview",    lstImports);
            r2pw.add_control("sections_listview",   lstSections);
            r2pw.add_control("hexview",             webBrowser2);
            r2pw.add_control("r2help", webBrowser3);
            //add and assign "decorators"
            r2pw.add_decorator("num2hex", num2hex, new List<string>(){"offset","vaddr","paddr","plt"});
            r2pw.add_decorator("dec_b64", dec_b64, new List<string>(){"string"});
            //add menu options and function callbacks
            r2pw.add_menucmd("&View", "Functions", "aaa;aflj", mainMenu);
            r2pw.add_menucmd("&View", "File info", "iIj", mainMenu);
            r2pw.add_menucmd("&View", "File version", "iV", mainMenu);
            r2pw.add_menucmd("&View", "Strings", "izj", mainMenu);
            r2pw.add_menucmd("&View", "Strings", "izj", mainMenu);
            r2pw.add_menucmd("&View", "Libraries", "ilj", mainMenu);
            r2pw.add_menucmd("&View", "Symbols", "isj", mainMenu);
            r2pw.add_menucmd("&View", "Relocs", "irj", mainMenu);
            r2pw.add_menucmd("&View", "Entropy", "p=", mainMenu);
            r2pw.add_menucmd("&View", "Entry Point", "pdfj @ entry0", mainMenu);
            r2pw.add_menucmd("&View", "List all RBin plugins loaded", "iL", mainMenu);
            r2pw.add_menucmd("r2", "Strings", "i?", mainMenu);
            r2pw.add_menucmd("r2", "Print help", "p?", mainMenu);
            r2pw.add_menucmd("r2", "Version", "?V", mainMenu);
            //add menu function callbacks
            r2pw.add_menufcn("&Gui", "Update gui", "*", UpdateGUI, mainMenu);
            r2pw.add_menufcn("&Gui", "Enum registry vars", "*", dumpGuiVars, mainMenu);
            r2pw.add_menufcn("Recent", "", rconfig.lastFileName, LoadFile, mainMenu);
            //add shell options
            r2pw.add_shellopt("radare2", guiPrompt_callback);
            r2pw.add_shellopt("javascript", guiPrompt_callback);
            //new auto-generated tabs
            r2pw.add_control_tab("version ( ?V )", "#todo");
            r2pw.add_control_tab("xrefs ( axtj )", "#todo");
            //load some example file
            //LoadFile(@"c:\windows\SysWOW64\notepad.exe");
            LoadFile("-");
        }
        public void UpdateGUI(string args=null)
        {
            Color backColor;
            Color foreColor;
            updating_gui    = true;
            tabcontrol      = tabControl1;
            currentShell    = rconfig.load<string>("gui.current_shell", "radare2");
            themeName       = rconfig.load<string>("gui.theme_name", "default");
            backColor       = Color.FromName(rconfig.load<string>("gui.output.bg", "blue"));
            foreColor       = Color.FromName(rconfig.load<string>("gui.output.fg", "white"));
            Left            = int.Parse(rconfig.load<int>("gui.left",Left));
            Top             = int.Parse(rconfig.load<int>("gui.top",Top));
            Width           = int.Parse(rconfig.load<int>("gui.width",Width));
            Height          = int.Parse(rconfig.load<int>("gui.height",Height));
            BackColor = backColor;
            mainMenu.BackColor = backColor;
            mainMenu.ForeColor = foreColor;
            tabControl1.BackColor = backColor;
            tabControl1.ForeColor = foreColor;
            statusStrip1.BackColor = backColor;
            statusStrip1.ForeColor = foreColor;
            cmbCmdline.BackColor = backColor;
            cmbCmdline.ForeColor = foreColor;
            txtOutput.BackColor = backColor;
            txtOutput.ForeColor = foreColor;
            listView1.BackColor = backColor;
            listView1.ForeColor = foreColor;
            lstStrings.BackColor = backColor;
            lstStrings.ForeColor = foreColor;
            lstImports.BackColor = BackColor;
            lstImports.ForeColor = foreColor;
            lstSections.BackColor = backColor;
            lstSections.ForeColor = foreColor;
            splitContainer1.Panel1.BackColor = backColor;
            splitContainer1.SplitterDistance = int.Parse(rconfig.load<int>("gui.splitter_1.dist", splitContainer1.SplitterDistance));
            splitContainer2.SplitterDistance = int.Parse(rconfig.load<int>("gui.splitter_2.dist", splitContainer2.SplitterDistance));
            slabelTheme.Text = themeName + " theme";
            button1.Text = currentShell;
            Refresh();
            updating_gui = false;
        }
        private void DoLoadFile()
        {
            if (!File.Exists(fileName) && !fileName.Equals("-"))
            {
                r2pw.Show(string.Format("Wops!\n{0}\nfile not found...", fileName), "LoadFile");
                return;
            }
            r2pw.open(fileName);
            r2pw.setText("version ( ?V )", "?V", r2pw.r2.RunCommand("?V"));
            r2pw.run_script("openfile_post.txt");
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
            tabControl1.SelectedTab = tabControl1.TabPages[0];
        }
        private void save_gui_config()
        {
            if (updating_gui) return;
            rconfig.save("gui.left", Left);
            rconfig.save("gui.top", Top);
            rconfig.save("gui.width", Width);
            rconfig.save("gui.height", Height);
            rconfig.save("gui.splitter_1.dist", splitContainer1.SplitterDistance);
            rconfig.save("gui.splitter_2.dist", splitContainer2.SplitterDistance);
            rconfig.save("gui.output.bg", txtOutput.BackColor.Name);
            rconfig.save("gui.output.fg", txtOutput.ForeColor.Name);
            if( !r2pw.fileName.Equals("-") )
                rconfig.save("gui.lastfile", r2pw.fileName);
            UpdateGUI();
        }
        public void show_message(string text)
        {
            slabel1.Text = text;
        }
        public void script_executed_cb()
        {
            show_message(string.Format("Binary file '{0}' loaded.", fileName));
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
        private string get_selectedAddress(object sender)
        {
            string msg = null;
            if (sender.GetType() == typeof(ListView))
            {
                ListView listview = ((ListView)sender);
                if( listview.Items.Count > 0 )
                    msg = listview.SelectedItems[0].Text;
            }else{
                r2pw.Show(string.Format("Form1: get_selectedAddress(): can't read address from '{0}'", sender.GetType().ToString()), "error");
                return null;
            }
            return msg;
        }
        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            string address = get_selectedAddress(sender);
            if (address != null)
            {
                r2pw.run("pdf @ " + address, "dissasembly");
                r2pw.run("px 2000 @ " + address, "hexview");
                r2pw.run("axtj @ " + address, "xrefs ( axtj )");
            }
        }
        private void menuXrefs_Click(object sender, EventArgs e)
        {
            int i;
            string address = get_selectedAddress(listView1);
            if (address != null)
            {
                TabPage page = null;
                r2pw.run("axtj @ " + address, "xrefs ( axtj )");
                for(i=0;i<tabcontrol.TabPages.Count;i++)
                {
                    page = tabcontrol.TabPages[i];
                    if (page.Text == "xrefs ( axtj )")
                        tabcontrol.SelectedTab = page;
                }
            }
        }
        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            save_gui_config();
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            r2pw.exit();
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
            r2pw.setText("output", "", text, true);
        }
        private string guiPrompt_callback()
        {
            //MessageBox.Show(currentShell);
            return null;
        }
        private string num2hex() // decorator
        {
            return string.Format("0x{0:x}", int.Parse(r2pw.decorator_param));
        }
        private string dec_b64() // decorator
        {
            byte[] data = Convert.FromBase64String(r2pw.decorator_param);
            return Encoding.UTF8.GetString(data);
        }
        private void changeTheme(string themeName)
        {
            if (r2pw != null)
            {
                r2pw.set_theme(themeName);
                r2pw.sendToWebBrowser("dissasembly", null, null, null);
                r2pw.sendToWebBrowser("hexview", null, null, null);
                r2pw.sendToWebBrowser("r2help", null, null, null);
                UpdateGUI();
            }
        } // themes
        private void classicToolStripMenuItem_Click(object sender, EventArgs e)
        {
            changeTheme("classic");
        }
        private void blueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            changeTheme("azure");
        }
        private void darkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            changeTheme("pink");
        }
        private void controlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            changeTheme("control");
        }
        private void darkToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            changeTheme("terminal256");
        }
        private void sandedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            changeTheme("lemon");
        }
        private void terminal256ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            changeTheme("terminal256");
        }
        private void contorlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            changeTheme("control");
        }
        private void classicToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            changeTheme("classic");
        }
        private void azuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            changeTheme("azure");
        }
        private void pinkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            changeTheme("pink");
        }
        private void sandedToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            changeTheme("lemon");
        }
        private void button1_Click(object sender, EventArgs e)
        {
            r2pw.next_shell();
        }
        private void cmbCmdline_SelectedValueChanged(object sender, EventArgs e)
        {

        }
        private void cmbCmdline_Enter(object sender, EventArgs e)
        {

        }
        private void cmbCmdline_KeyUp(object sender, KeyEventArgs e)
        {
            if (cmbCmdline.Focused && cmbCmdline.Text.Length != 0 && e.KeyValue == 38)
            {
                string text = cmbCmdline.Text;
                cmbCmdline.Text = ""; // trick to refresh combo control with text selected
                cmbCmdline.Text = text;
                cmbCmdline.SelectionStart = cmbCmdline.Text.Length ;
            }
        }
        private void ctxTabsItemClose_Click(object sender, EventArgs e)
        {
            tabControl1.TabPages.Remove(tabControl1.SelectedTab);
        }
        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(txtOutput.SelectedText.Replace("\n","\r\n"));
        }
        private void webBrowser1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            bool controlPressed = e.Modifiers == Keys.Control;
            switch (e.KeyValue)
            {
                case 69: // e key
                    r2pw.gotoAddress("entry0");
                    break;
                case 27: // esc key
                    webBrowser1.GoBack();
                    r2pw.lastAddress = null;
                    break;
            }
        }
        private void filterResultsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            r2pw.Show(tabControl1.SelectedTab.Text, "filter");
        }
    }
}
