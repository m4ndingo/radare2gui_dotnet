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
        public string fileType = null;
        private bool updating_gui = false;
        public TabControl tabcontrol = null;
        public string themeName = null;
        public string currentShell = null;
        List<string> locked_tabs = null;    // tab names used in Form1 design
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            rconfig = new RConfig();
            locked_tabs = new List<string>() { "Dissasembly", "Hex view", "Strings", "Imports", "Sections", "Maps" };
            UpdateGUI();
            CheckR2path();
            r2pw = new R2PIPE_WRAPPER(rconfig, this);        // init here
            //add controls
            r2pw.add_control("output", txtOutput);
            r2pw.add_control("dissasembly", webBrowser1,         "Dissasembly", "pdf"   );
            r2pw.add_control("strings_listview", lstStrings,     "Strings",     "izj"   );
            r2pw.add_control("functions_listview", listView1,    "Functions",   "aflj"  );
            r2pw.add_control("imports_listview", lstImports,     "Imports",     "iij"   );
            r2pw.add_control("sections_listview", lstSections,   "Sections",    "iSj"   );
            r2pw.add_control("processes_listView", lstProcesses, "Processes",   "dpj"   );
            r2pw.add_control("maps_listView", lstMaps,           "Maps",        "dmj"   );
            r2pw.add_control("hexview", webBrowser2,             "Hex view",    "pxa 2000");
            //add and assign "decorators"
            r2pw.add_decorator("num2hex", num2hex, new List<string>(){
                "offset", "vaddr", "paddr", "plt", "addr", "addr_end", "eip"});
            r2pw.add_decorator("dec_b64", dec_b64, new List<string>() { "string" });
            //add menu options and function callbacks
            r2pw.add_menucmd("&View", "Processes", "dpj", mainMenu);
            r2pw.add_menucmd("&View", "Disassembly", "pdf", mainMenu);
            r2pw.add_menucmd("&View", "Hexadecimal", "px", mainMenu, "num2hex");
            r2pw.add_menucmd("&View", "Functions", "aaa;aflj", mainMenu);
            r2pw.add_menucmd("&View", "File info", "iIj", mainMenu);
            r2pw.add_menucmd("&View", "File version", "iV", mainMenu);
            r2pw.add_menucmd("&View", "Sections", "S=", mainMenu);
            r2pw.add_menucmd("&View", "Strings", "izj", mainMenu);
            r2pw.add_menucmd("&View", "Libraries", "ilj", mainMenu);
            r2pw.add_menucmd("&View", "Symbols", "isj", mainMenu);
            r2pw.add_menucmd("&View", "Relocs", "irj", mainMenu);
            r2pw.add_menucmd("&View", "Entropy", "p=", mainMenu);
            r2pw.add_menucmd("&View", "Entry Point", "pdfj @ entry0", mainMenu);
            r2pw.add_menucmd("&View", "ESIL registers", "aerj", mainMenu);
            r2pw.add_menucmd("&View", "List all RBin plugins loaded", "iL", mainMenu);
            r2pw.add_menucmd("r2", "Main", "?", mainMenu);
            r2pw.add_menucmd("r2", "Expresions", "???", mainMenu);
            r2pw.add_menucmd("r2", "Write", "w?", mainMenu);
            r2pw.add_menucmd("r2", "Dbg cmds", "d?", mainMenu);
            r2pw.add_menucmd("r2", "Processes", "dp?", mainMenu);
            r2pw.add_menucmd("r2", "Strings", "i?", mainMenu);
            r2pw.add_menucmd("r2", "Search", "/?", mainMenu);
            r2pw.add_menucmd("r2", "Metadata", "C?", mainMenu);
            r2pw.add_menucmd("r2", "ESIL", "ae?", mainMenu);
            r2pw.add_menucmd("r2", "Print help", "p?", mainMenu);
            r2pw.add_menucmd("r2", "Version", "?V", mainMenu);
            //add menu function callbacks
            r2pw.add_menufcn("Miscelanea", "Dump controls", "*", r2pw.gui_controls.dump, mainMenu);
            r2pw.add_menufcn("Miscelanea", "Enum registry vars", "*", dumpGuiVars, mainMenu);
            r2pw.add_menufcn("Miscelanea", "Purge r2pipe_gui_dotnet registry", "*", purgeR2pipeGuiRegistry, mainMenu);
            r2pw.add_menufcn("Recent", "", rconfig.lastFileName, LoadFile, mainMenu);
            r2pw.add_menufcn("Architecture", "", "avr", changeArch, mainMenu);
            r2pw.add_menufcn("Architecture", "", "x86", changeArch, mainMenu);
            r2pw.add_menufcn("ESIL", "initialize ESIL VM state", "aei", ESILcmds, mainMenu);
            r2pw.add_menufcn("ESIL", "step", "aes", ESILcmds, mainMenu);
            r2pw.add_menufcn("ESIL", "registers", "aer", ESILcmds, mainMenu);
            //add shell options
            r2pw.add_shellopt("radare2", guiPrompt_callback);
            r2pw.add_shellopt("javascript", guiPrompt_callback);
            //load some example file
            //LoadFile(@"c:\windows\SysWOW64\notepad.exe");
            LoadFile("-");
        }
        public void UpdateGUI(string args = null)
        {
            Color backColor;
            Color foreColor;
            updating_gui = true;
            tabcontrol = tabControl1;
            currentShell = rconfig.load<string>("gui.current_shell", "radare2");
            themeName = rconfig.load<string>("gui.theme_name", "default");
            backColor = Color.FromName(rconfig.load<string>("gui.output.bg", "blue"));
            foreColor = Color.FromName(rconfig.load<string>("gui.output.fg", "white"));
            Left = int.Parse(rconfig.load<int>("gui.left", Left));
            Top = int.Parse(rconfig.load<int>("gui.top", Top));
            Width = int.Parse(rconfig.load<int>("gui.width", Width));
            Height = int.Parse(rconfig.load<int>("gui.height", Height));
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
            lstProcesses.BackColor = backColor;
            lstProcesses.ForeColor = foreColor;
            tsDebug.BackColor = backColor;
            tsDebug.ForeColor = foreColor;
            splitContainer1.Panel1.BackColor = backColor;
            splitContainer1.SplitterDistance = int.Parse(rconfig.load<int>("gui.splitter_1.dist", splitContainer1.SplitterDistance));
            splitContainer2.SplitterDistance = int.Parse(rconfig.load<int>("gui.splitter_2.dist", splitContainer2.SplitterDistance));
            WindowState = rconfig.load<string>("gui.window.state", "normal").Equals("normal") ?
                FormWindowState.Normal : FormWindowState.Maximized;
            slabelTheme.Text = themeName + " theme";
            button1.Text = currentShell;
            Refresh();
            updating_gui = false;
        }
        private void DoLoadFile()
        {
            r2pw.open(fileName);
            // r2pw.setText("version ( ?V )", "?V", r2pw.r2.RunCommand("?V"));
            r2pw.run_script("openfile_post.txt");
            if (!fileName.Equals("-"))
            {
                fileType = r2pw.run("e file.type");
                if( fileType!=null )
                    fileType = fileType.Replace("\n", "");
                //output(string.Format("{0} file loaded", fileType));
                if (fileType.Length > 0) // used only in statusbar
                    fileType = " " + fileType;
            }
        }
        private void CheckR2path()
        {
            string r2path = rconfig.r2path;
            if (r2path == null || !File.Exists(rconfig.r2path))
            {
                if (((object)r2pw) != null) r2pw.Show("Form1: CheckR2path(): Path for 'radare2.exe' not found...", "radare2.exe not found");
                rconfig.save("r2path", r2pw.FindFile("radare2.exe","Please, locate your radare2.exe binary"));
            }
        }
        private void LoadFile(String fileName)
        {
            string version = System.Reflection.Assembly.GetExecutingAssembly()
                                           .GetName()
                                           .Version
                                           .ToString();
            Text = String.Format("R2pipe-Gui .net ( alpha {0} ) - {1}", version, fileName);
            if (!File.Exists(rconfig.r2path))
            {
                CheckR2path();
                return;
            }
            if (!File.Exists(fileName) && !fileName.Equals("-"))
            {
                r2pw.Show(string.Format("Wops!\n{0}\nfile not found...", fileName), "LoadFile");
                return;
            }
            if (r2pw.r2 != null && !fileName.Equals("-")) // set arch if filename != '-'
            {
                string new_arch = null;
                new_arch = r2pw.run("e asm.arch", "output", true); //no wait
                if (new_arch != null)
                {
                    new_arch = new_arch.Replace("\n", "");
                    new_arch = r2pw.Prompt("Arch:", "Select arch", new_arch, this);
                    changeArch(new_arch);
                    //r2pw.run("e asm.arch = " + new_arch, "output", true); // no wait
                }
            }
            clearControls();
            this.fileName = fileName;

            Refresh();
            Thread newThread = new Thread(new ThreadStart(this.DoLoadFile));
            newThread.Start();
            cmbCmdline.Focus();
            tabControl1.SelectedTab = tabControl1.TabPages[1]; // def tan when start gui
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
            if (!r2pw.fileName.Equals("-"))
                rconfig.save("gui.lastfile", r2pw.fileName);
            rconfig.save("gui.window.state", WindowState == FormWindowState.Normal ?
                "normal" : "maximized");
            UpdateGUI();
        }
        public void show_message(string text)
        {
            try
            {
                slabel1.Text = text;
            }
            catch (Exception e) { r2pw.Show(e.ToString(), "show_message"); } // manage this, script_executed_cb fails on this when prompt
            try
            {
                cmbCmdline.Focus();
            }
            catch (Exception) { };
        }
        public void script_executed_cb()
        {
            show_message(string.Format("Binary file '{0}' loaded.", fileName));
        }
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string FileName = r2pw.Prompt("Locate some file", "Open dialog", r2pw.fileName, this);
            LoadFile(FileName);
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
            bye_gui();
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
                if (listview.Items.Count > 0)
                    msg = listview.SelectedItems[0].Text;
            }
            else
            {
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
                r2pw.run("pxa 2000 @ " + address, "hexview");
                r2pw.run("axtj @ " + address, "xrefs ( axtj )");
                r2pw.run("pdf @ " + address, "dissasembly");
            }
            if (!locked_tabs.Contains(tabControl1.SelectedTab.Text))
                tabControl1.SelectedIndex = 0;
        }
        private void menuXrefs_Click(object sender, EventArgs e)
        {
            int i;
            string address = get_selectedAddress(listView1);
            if (address != null)
            {
                TabPage page = null;
                r2pw.add_control_tab("xrefs ( axtj )", "#todo");
                r2pw.run("axtj @ " + address, "xrefs ( axtj )");
                for (i = 0; i < tabcontrol.TabPages.Count; i++)
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
            List<string> keys = rconfig.reg_enumkeys();
            if (args == "*") args = r2pw.Prompt("filter results", "Enum registry vars", args);
            output(string.Format("filter: {0}", args));
            foreach (string varname in keys)
            {
                string value = rconfig.load<string>(varname);
                output(string.Format("{0} = {1}", varname, value));
            }
        }
        private void dumpGuiVar(string varname)
        {
            string value = rconfig.load<string>(varname);
            output(string.Format("dumpGuiVars(): varname='{0}' value='{1}'", varname, value));
        }
        private void output(string text)
        {
            r2pw.output(text);
        }
        private string guiPrompt_callback()
        {
            //MessageBox.Show(currentShell);
            return null;
        }
        private void ESILcmds(string cmds)
        {
            r2pw.run(cmds, "output", true);
            if (cmds.StartsWith("ae"))
            {
                refresh_tab();
                //r2pw.run("pd", "dissasembly");
            }
        }
        private void changeArch(String arch)
        {
            string nbits = r2pw.run("e asm.bits");
            r2pw.run(string.Format("e asm.arch = {0}; aaa", arch));
            try
            {
                rconfig.save("gui.hexdigits", int.Parse(nbits) / 8 );
            }
            catch (Exception e) { r2pw.Show(e.ToString(), "changeArch"); }
        }
        private string num2hex() // decorator
        {
            int hexdigits = int.Parse(rconfig.load<int>("gui.hexdigits", 8));
            string format = "0x{0:x" + (hexdigits / 2).ToString() + "}";
            return string.Format(format, Int64.Parse(r2pw.decorator_param));
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
                // gui controls to refresh
                r2pw.sendToWebBrowser("dissasembly", null, null, null);
                r2pw.sendToWebBrowser("hexview", null, null, null);
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
        private void cmbCmdline_KeyUp(object sender, KeyEventArgs e)
        {
            if (cmbCmdline.Focused && cmbCmdline.Text.Length != 0 && e.KeyValue == 38)
            {
                string text = cmbCmdline.Text;
                cmbCmdline.Text = ""; // trick to refresh combo control with text selected
                cmbCmdline.Text = text;
                cmbCmdline.SelectionStart = cmbCmdline.Text.Length;
            }
        }
        private void ctxTabsItemClose_Click(object sender, EventArgs e)
        {
            string controlName = tabControl1.SelectedTab.Text;
            if (locked_tabs.Contains(controlName)) return;
            tabControl1.TabPages.Remove(tabControl1.SelectedTab);
            r2pw.controls.Remove(controlName);
        }
        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(txtOutput.SelectedText.Replace("\n", "\r\n"));
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
        private void closeFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            r2pw.run("o-*;o -");
            clearControls();
            LoadFile("-");
        }
        private void clearControls()
        {
            listView1.Clear();
            lstStrings.Clear();
            lstImports.Clear();
            lstSections.Clear();
            if (r2pw.r2 != null)
            {
                r2pw.run("pd", "dissasembly");  // no wait
                r2pw.run("px 2000", "hexview"); // no wait
            }
        }
        private string todo(string caption = "#todo", string tip = "no code yet", object sender = null)
        {
            if (sender == null) sender = this;
            return r2pw.Show(tip, caption + " " + sender.GetType().ToString()).ToString();
        }
        private void windowsControlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            todo("Windows control", "check best control for render " + tabControl1.SelectedTab.Text + " data");
        }
        private void textToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string cmds = null;
            string tabTitle = selected_tab("title");
            string tabTag   = selected_tab("tag");
            // send to output control as string
            maximize("output");
            if ( tabTag != null ) cmds = tabTag;
            if (tabTitle == "Dissasembly") cmds = "pd 20";
            if (tabTitle == "Hex view") cmds = "pxa";
            if (tabTitle == "Strings") cmds = "iz";
            if (tabTitle == "Sections") cmds = "iS";
            if (tabTitle == "Imports") cmds = "ii";
            if (tabTitle == "Processes") cmds = "dp";
            if (tabTitle == "Maps") cmds = "dm";
            if (cmds != null)
            {
                if (cmds.EndsWith("j")) // remove "j" from tabcommand if found
                    cmds = cmds.Substring(0, cmds.Length - 1);
                r2pw.run(cmds, "output", true);
            }
            else
                output("no cmds found for: " + tabTitle + " tag: " + tabTag);
        }
        private void jsonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            todo("Json Formated Output", "ListView required for " + tabControl1.SelectedTab.Text);
        }
        private void floatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            popup_tab();
        }
        public void popup_tab()
        {
            webbrowser_container_form webFrm = new webbrowser_container_form(r2pw, "webcont");
            WebBrowser webbrowser = new WebBrowser();

            webbrowser.Dock = DockStyle.Fill;
            webFrm.Controls.Add(webbrowser);
            string tabTitle = selected_tab("title");
            webFrm.Text  = tabTitle;
            webFrm.Width = Width - splitContainer1.SplitterDistance;
            webFrm.Height = Height;
            String timeStamp = DateTime.Now.Millisecond.ToString();
            GuiControl gui_control_tab = r2pw.gui_controls.findControlBy_tabTitle(tabTitle);
            GuiControl gui_control = r2pw.add_control(
                gui_control_tab.name + "_" + timeStamp, webbrowser, "popup", gui_control_tab.cmds);
            refresh_control(gui_control);
            webFrm.Show();
        }
        private void HTMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            todo("HTML Formated Output", "WeBrowser required for " + tabControl1.SelectedTab.Text);
        }
        private void enableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            txtOutput.WordWrap = true;
        }
        private void disableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            txtOutput.WordWrap = false;

        }
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            string pc = ""; // new eip/pc for ESIL emulation
            ESILcmds("aei");
            pc = r2pw.run("aer~eip[1]");
            pc = "entry0";
            pc = r2pw.Prompt("Start address", "New eip", pc).Replace("\n","");
            if (pc != null)
            {
                ESILcmds("aer eip = " + pc);
                 r2pw.run("s " + pc);
                //ESILcmds("aer pc = " + pc);
            }
        }
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            ESILcmds("aes");
        }
        private void classicToolStripMenuItem1_Click_1(object sender, EventArgs e)
        {
            changeTheme("classic");
        }
        private void lemonToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            changeTheme("lemon");
        }
        private void azuToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            changeTheme("azure");
        }
        private void contorlToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            changeTheme("control");
        }
        private void pinkToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            changeTheme("pink");
        }
        private void terminal256ToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            changeTheme("terminal256");
        }
        private string selected_tab(string attr)
        {
            string text = null;
            if (attr.Equals("title")) text = tabControl1.SelectedTab.Text;
            if (attr.Equals("tag") && tabControl1.SelectedTab.Tag!=null) 
                text = tabControl1.SelectedTab.Tag.ToString();
            if (attr.Equals("controlName"))
                text = r2pw.findControlBy_tabTitle(attr);
            return text;
        }
        private void maximizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            maximize("output");
        }
        private void maximize(string controlName="")
        {
            splitContainer1.SplitterDistance = 0;
            if (controlName.Equals("output"))
                splitContainer2.SplitterDistance = 0;
            else
                splitContainer2.SplitterDistance = Height;
            txtOutput.Refresh();
            cmbCmdline.Focus();
        }
        private void wipe_config()
        {
            rconfig.reg_wipeconf();
            output("configuration wipped...");
        }
        public void purgeR2pipeGuiRegistry(string args = null)
        {
            DialogResult res;
            res = MessageBox.Show(
                "wipe configuration (reg) and terminate the gui?",
                "wipe gui reg",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (res == DialogResult.Yes) bye_gui(true);
        }
        private void bye_gui(bool wipeconf = false) // gracefull terminate r2 and the app
        {
            if (r2pw != null) r2pw.exit();
            if (wipeconf) wipe_config();
            Environment.Exit(0);
        }
        public void autoresize_output() // useful after output maximized
        {
            try // may fail on async runs
            {
                // left "functions" splitter
                if (tabControl1.SelectedTab.Text.Equals("Dissasembly"))
                {
                    if (splitContainer1.SplitterDistance == 118)
                        todo("elf", "invalid distance");
                    else if (splitContainer1.SplitterDistance < 118)
                    {
                        splitContainer1.SplitterDistance = 150;
                        if (listView1.Items.Count > 0)
                            listView1.AutoResizeColumn(0, ColumnHeaderAutoResizeStyle.ColumnContent);
                    }
                }
                // up-down splitter
                if (splitContainer2.SplitterDistance < 25)
                    todo("elf", "invalid distance");
                else if (splitContainer2.SplitterDistance >= 25)
                    splitContainer2.SplitterDistance = Height / 2;
            }
            catch (Exception e)
            {
                r2pw.Show(e.ToString(),"autoresize_output()");
            }
        }
        private void tabControl1_Click(object sender, EventArgs e)
        {
            autoresize_output();
        }
        private void maximizeOutputToolStripMenuItem_Click(object sender, EventArgs e)
        {
            autoresize_output();
        }
        private void tabControl2_Click(object sender, EventArgs e)
        {
            maximize("output");
        }
        private void tabControl1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {

        }
        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            r2pw.show_processes(txtSearch.Text);
            txtSearch.Focus();
        }
        private void tabControl1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) hide_search();
        }
        private void hide_search()
        {
            txtSearch.Visible = false;
        }
        private void show_search(string text)
        {
            txtSearch.Visible = true;
            txtSearch.Text += text;
            txtSearch.Focus();
            txtSearch.SelectionStart = txtSearch.TextLength;
        }
        private void lstProcesses_KeyPress(object sender, KeyPressEventArgs e)
        {
            show_search(e.KeyChar.ToString());
        }
        private void lstProcesses_Click(object sender, EventArgs e)
        {
            hide_search();
        }
        private GuiControl find_control_by_title(string title)
        {
            return r2pw.gui_controls.findControlBy_tabTitle(title);
        }
        private GuiControl find_control_by_name(string name)
        {
            return r2pw.gui_controls.findControlBy_name(name);
        }
        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            refresh_tab();
        }
        private void refresh_control(GuiControl control)
        {
            if (control!= null && control.cmds != null)
            {
                List<string> column_titles = control.column_titles;
                if (column_titles == null)
                {
                    if (control.control.GetType() == typeof(ListView))
                    {
                        r2pw.save_active_cols(control.name, (ListView)control.control);
                        column_titles = control.column_titles;
                    }
                }
                //output( "refresh_control: " + control.name + "@" + control.tabTitle + 
                //        " cmds:" + control.cmds);
                r2pw.run(control.cmds, control.name, false, column_titles); // no wait
            }
            else
            {
                if( control!=null )
                    output("refresh_control: '" + control.name + "' no cmds found ( complete add_cmd() args )");
            }
        }
        private void refresh_tab()
        {
            string tabTitle = selected_tab("title");
            GuiControl gui_control =  find_control_by_title(tabTitle);
            if (gui_control != null)
            {
                //r2pw.Show("refresh_tab(): gui_control found " + gui_control.ToString() + " for title '" + tabTitle + "'","refresh_tab()");
                refresh_control(gui_control);
            }
        }
        private void runToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string ooss_command = null;
            maximize("output");
            ooss_command = r2pw.Prompt("ooss command:", "run ( ! )", "!!notepad");
            if (ooss_command != null)
                r2pw.run(ooss_command, "output", true);
        }
        private void zoomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            maximize();
        }
        private void openfileposttxtToolStripMenuItem_Click(object sender, EventArgs e)
        {
            r2pw.run_script("openfile_post.txt");
        }
        private void pathsToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            r2pw.find_dataPath(rconfig.load<string>("gui.datapath", "."));
            changeTheme(themeName);
        }
        private void editPnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateGUI();
            r2pw.run("Pn -");
        }
        private void showPnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            r2pw.run("Pn","Notes");
        }
        private void refreshToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            GuiControl gui_control = find_control_by_name("functions_listview");
            refresh_control(gui_control);
        }
    }
}