using System;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Collections;
using Newtonsoft.Json;

namespace r2pipe_test
{
    public partial class Form1 : Form
    {
        R2PIPE_WRAPPER r2pw = null;
        private RConfig rconfig = null;
        private string fileName = null;
        public string fileType = null;
        public string arch = null;
        private bool updating_gui = false;
        public TabControl tabcontrol = null;
        public Benchmarks benchmarks = null;
        public string themeName = null;
        public string currentShell = null;
        List<string> locked_tabs = null;        // tab names used in Form1 design
        private bool skip_next_keydown = false;
        private bool esil_initilized = false;
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            rconfig = new RConfig();
            benchmarks = new Benchmarks();
            locked_tabs = new List<string>() { "Dissasembly", "Hex view", "Strings", "Imports", "Sections", "Maps" };
            CheckR2path();            
            tabcontrol = tabControl1;
            r2pw = new R2PIPE_WRAPPER(rconfig, this);        // init here
            tabControl1.SelectedTab = tabControl1.TabPages[1]; // def tab when start gui
            UpdateGUI();
            //add controls
            r2pw.add_control("output", txtOutput);
            r2pw.add_control("dissasembly", webBrowser1, "Dissasembly", "pd 256");
            r2pw.add_control("strings_listview", lstStrings, "Strings", "izzj");
            r2pw.add_control("functions_listview", listView1, "Functions", "aflj");
            r2pw.add_control("imports_listview", lstImports, "Imports", "iij");
            r2pw.add_control("sections_listview", lstSections, "Sections", "iSj");
            r2pw.add_control("processes_listView", lstProcesses, "Processes", "dpj");
           // r2pw.add_control("maps_listView", lstMaps, "Maps", "dmj");
            r2pw.add_control("hexview", webBrowser2, "Hex view", "pxa 4000");
            //add and assign "decorators"
            r2pw.add_decorator("num2hex", num2hex, new List<string>(){
                "offset", "vaddr", "paddr", "plt", "addr", "addr_end", "eip"});
            r2pw.add_decorator("dec_b64", dec_b64, new List<string>() { "string" });
            r2pw.add_decorator("short_addr_name", short_addr_name, new List<string>() { "name" });
            //add menu options and function callbacks
            r2pw.add_menucmd("&View", "Processes", "dpj", mainMenu);
            r2pw.add_menucmd("&View", "Disassembly", "pd 256", mainMenu);
            r2pw.add_menucmd("&View", "Hexadecimal", "pxa 4000", mainMenu);
            r2pw.add_menucmd("&View", "Functions", "aflj", mainMenu);
            r2pw.add_menucmd("&View", "File info", "iIj", mainMenu);
            r2pw.add_menucmd("&View", "File version", "iV", mainMenu);
            r2pw.add_menucmd("&View", "Sections", "S=", mainMenu);
            r2pw.add_menucmd("&View", "Strings", "izzj", mainMenu);
            r2pw.add_menucmd("&View", "Libraries", "ilj", mainMenu);
            r2pw.add_menucmd("&View", "Imports", "iij", mainMenu);
            r2pw.add_menucmd("&View", "Exports", "iEj", mainMenu);
            r2pw.add_menucmd("&View", "Symbols", "isj", mainMenu);
            r2pw.add_menucmd("&View", "Relocs", "irj", mainMenu);
            r2pw.add_menucmd("&View", "Maps", "dmj", mainMenu);
            r2pw.add_menucmd("&View", "Entropy", "p=", mainMenu);
            r2pw.add_menucmd("&View", "Entry Point", "pdfj @ entry0", mainMenu);
            r2pw.add_menucmd("&View", "Ascii Art Bar", "p-", mainMenu);
            r2pw.add_menufcn("&View", "ESIL registers", "aerj", popup_cb, mainMenu);
            r2pw.add_menucmd("&View", "List all RBin plugins loaded", "iL", mainMenu);
            r2pw.add_menucmd("&View", "Configuration", "ej", mainMenu);
            r2pw.add_menucmd("&View", "Debug registers", "drj", mainMenu);
            r2pw.add_menufcn("&View", "Call graph", "agf", newtab_cb, mainMenu);
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
            r2pw.add_menufcn("ESIL", "initialize ESIL VM state", "aei", ESILcmds, mainMenu);
            r2pw.add_menufcn("ESIL", "step", "aes", ESILcmds, mainMenu);
            r2pw.add_menufcn("ESIL", "registers", "aer", ESILcmds, mainMenu);
            r2pw.add_menufcn("Settings", "switch utf8 encoding", "e!scr.utf8;e scr.utf8", runCmds, mainMenu);
            r2pw.add_menufcn("Settings", "switch asm bytes", "e!asm.bytes", runCmds, mainMenu);
            r2pw.add_menufcn("Miscelanea", "Dump controls", "*", r2pw.gui_controls.dump, mainMenu);
            r2pw.add_menufcn("Miscelanea", "Enum registry vars", "*", dumpGuiVars, mainMenu);
            r2pw.add_menufcn("Miscelanea", "Purge r2pipe_gui_dotnet registry", "*", purgeR2pipeGuiRegistry, mainMenu);
            r2pw.add_menufcn("Recent", "", rconfig.lastFileName, LoadFile, mainMenu);
            //add shell options
            r2pw.add_shellopt("radare2", guiPrompt_callback);
            r2pw.add_shellopt("javascript", guiPrompt_callback);
            //load some example file
            //LoadFile(@"c:\windows\SysWOW64\notepad.exe");
            LoadFile("-"); // -- = no file
        }
        public void UpdateGUI(string args = null)
        {
            Color backColor;
            Color foreColor;
            if (r2pw == null) return;
            updating_gui = true;
            currentShell = rconfig.load<string>("gui.current_shell", "radare2");
            themeName = rconfig.load<string>("gui.theme_name", "classic");
            foreColor = Color.FromName(rconfig.load<string>("gui.output.fg", "black"));
            backColor = r2pw.theme_background();
            Left = int.Parse(rconfig.load<int>("gui.left", Left));
            Top = int.Parse(rconfig.load<int>("gui.top", Top));
            Width = int.Parse(rconfig.load<int>("gui.width", Width));
            Height = int.Parse(rconfig.load<int>("gui.height", Height));
            try
            {
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
                //lstMaps.BackColor = backColor;
                //lstMaps.ForeColor = foreColor;
                tsDebug.BackColor = backColor;
                tsDebug.ForeColor = foreColor;
                splitContainer1.Panel1.BackColor = backColor;
            }
            catch (Exception) { } // may fail better catch
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
            bool force_initial_analysis = false;
            if (r2pw == null) return;
            r2pw.open(fileName);
            if (fileName.StartsWith("-"))
                force_initial_analysis = true;
            if (force_initial_analysis == true ||
                MessageBox.Show("File loaded. Analyze now?", "File loaded",
                MessageBoxButtons.OKCancel, MessageBoxIcon.Question) ==
                    System.Windows.Forms.DialogResult.OK)
            {
                if (!fileName.Equals("-"))
                {
                    fileType = r2pw.run("e file.type");
                    fileType = null;
                    if (fileType != null)
                        fileType = fileType.Replace("\n", "");
                    arch = r2pw.run("iI~arch[1]");
                    if (arch == null || (arch!=null && arch.Length==0)) arch = "binary";
                    arch = arch.Replace("\n", "");
                    arch = Prompt("new arch", "select arch", arch);
                    if (arch != null)
                    {
                        arch = arch.Replace("\n", "");
                        r2pw.run("e asm.arch = " + arch);
                    }
                }
                r2pw.run_script("openfile_post.txt");
            }
        }
        private void CheckBinaryPath(string fileName, string varName, string defaultPath=null)
        {
            string binPath = rconfig.load<string>(varName);
            string fullPath =
            fullPath = rconfig.load<string>(varName);
            if (fullPath != null && File.Exists(fullPath))
                return;
            fullPath =
                System.IO.Path.GetDirectoryName(Application.ExecutablePath) +
                @"\" + defaultPath + @"\" + fileName;
            if (File.Exists(fullPath))
            {
                rconfig.save(varName, fullPath);
                return;
            }
            if (((object)r2pw) != null)
            {
                r2pw.Show("Form1: CheckBinaryPath(): Path for '" + fileName + "' not found...", fileName + " not found");
            }
            rconfig.save(varName, FindFile(fileName, "Please, locate your "+fileName+" binary"));
        }
        public void CheckDotpath()
        {
            CheckBinaryPath("dot.exe","dotPath");
        }
        private void CheckR2path()
        {
            CheckBinaryPath("radare2.exe", "r2path", "radare2-w64-1.1.0-git");
        }
        private void LoadFile(String fileName)
        {
            string version = "1.0";
            string realFilename = fileName;
            Text = String.Format("R4w alpha {0} radare2 gui - {1}", version, fileName);
            if (!File.Exists(rconfig.r2path))
            {
                CheckR2path();
                return;
            }
            realFilename = realFilename.Replace("-d ", "");
            if (realFilename != null && !File.Exists(realFilename) && !realFilename.StartsWith("-"))
            {
                r2pw.Show(string.Format("Wops!\n{0}\nfile not found...", fileName), "LoadFile");
                return;
            }
            clearControls();
            this.fileName = fileName;

            Refresh();
            Thread newThread = new Thread(new ThreadStart(this.DoLoadFile));
            newThread.Start();
            cmbCmdline.Focus();
            esil_initilized = false;
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
            catch (Exception) { 
                //r2pw.Show(e.ToString(), "show_message"); 
            } // manage this, script_executed_cb fails on this when prompt
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
        private void update_archs()
        {
            if (r2pw == null) return;
            string architectures = r2pw.run("e asm.arch=?~[2]");
            foreach (string arch in architectures.Split('\n'))
            {
                if (arch.Length > 0)
                    r2pw.add_menufcn("Architecture", "", arch, changeArch, mainMenu);
            }
        }
        private void update_cpus()
        {
            if (r2pw == null) return;
            string cpus = r2pw.run("iL~[1]");
            foreach (string cpu in cpus.Split('\n'))
            {
                if (cpu.Length > 0)
                    r2pw.add_menufcn("Cpu", "", cpu, changeCpu, mainMenu);
            }
        }
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (r2pw == null) return;
            string FileName = Prompt("Locate some file", "Open dialog", r2pw.fileName);
            if( FileName!=null )
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
                string cmds = cmbCmdline.Text;
                cmds = cmds.Replace("\r", "");
                cmbCmdline.Items.Remove("");
                cmbCmdline.Items.Add(cmds);
                r2pw.run(cmds, "output", true, null, null, true); // append
                cmbCmdline.Items.Add("");
                cmbCmdline.Text = "";
                cmbCmdline.Focus();
            }
        }
        private string get_selectedAddress(object sender)
        {
            string msg = null;
            if (sender.GetType() == typeof(ListView))
            {
                ListView listview = ((ListView)sender);
                if (listview.Items.Count > 0 && listview.SelectedItems.Count > 0)
                    msg = listview.SelectedItems[0].SubItems[1].Text; // todo: replace 0 with column index address
            }
            else
            {
                r2pw.Show(string.Format("Form1: get_selectedAddress(): can't read address from '{0}'", sender.GetType().ToString()), "error");
                return null;
            }
            return msg;
        }
        public void refresh_main_controls(string address = null)
        {
            int current_tab_index = tabcontrol.SelectedIndex;
            r2pw.refresh_control("hexview");
            r2pw.refresh_control("dissasembly");
            r2pw.refresh_control("Call graph");
            refresh_popups();
            tabcontrol.SelectedIndex = current_tab_index;
        }
        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            string address = get_selectedAddress(sender);
            if (address != null)
            {
                r2pw.gotoAddress(address);
                refresh_main_controls(address);
            }
        }
        private void menuXrefs_Click(object sender, EventArgs e)
        {
            string addr = null;
            ListViewItem.ListViewSubItem item = null;
            if (listView1.SelectedItems.Count == 0) return;
            item = listView1.SelectedItems[0].SubItems[1]; // find address
            addr = Prompt("Address:", "Xrefs", item.Text);
            if (addr != null && addr.Length > 0)
                runCmds("axtj @ " + addr);            
        }
        private void popup_tab(GuiControl c)
        {
            if (c == null)
            {
                r2pw.Show("can't popup tab, control is null", "popup_tab");
                return;
            }
            r2pw.run(c.cmds, c.name, false, null, null, false, false, c);
        }
        public void popup_cmds(string title, string cmds, bool popup=true)
        {
            int i;
            TabPage selected = tabcontrol.SelectedTab;
            tabcontrol.SuspendLayout();
            string address = get_selectedAddress(listView1);
            this.SuspendLayout();
            TabPage page = null;
            r2pw.run("s " + address);
            GuiControl gc = r2pw.gui_controls.findControlBy_cmds(cmds);
            if( gc==null )
                r2pw.add_control_tab(title, cmds);            
            r2pw.run(cmds, title, false, null, null, false, false, gc);
            if (popup == true)
            {
                for (i = 0; i < tabcontrol.TabPages.Count; i++)
                {
                    page = tabcontrol.TabPages[i];
                    if (page.Text == title)
                    {
                        tabcontrol.SelectedTab = page;
                        webbrowser_container_form webFrm = popup_tab();
                        close_selected_tab();
                        webFrm.Focus();
                    }
                }
            }
            tabcontrol.SelectedTab = selected; // don't lost focus on selected tab
            this.ResumeLayout();
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
            if (args == "*") args = Prompt("filter results", "Enum registry vars", args);
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
        private void runCmds(string cmds)
        {
            //GuiControl gc = null;
            //gc = r2pw.gui_controls.findControlBy_cmds(cmds);
            r2pw.run(cmds, cmds, true);
        }
        private void ESILcmds(string cmds)
        {
            r2pw.run(cmds, "output", true);
            if (cmds.StartsWith("ae"))
                refresh_tab();
        }
        private void newtab_cb(string cmds)
        {
            GuiControl gc = find_control_by_cmds(cmds);
            if (gc == null)
            {
                gc = r2pw.gui_controls.add_control(cmds, null, cmds, cmds, "", "", tabcontrol.SelectedIndex);
            }
            popup_tab(gc);
        }
        private void popup_cb(string cmds)
        {
            string popupName = "";
            GuiControl gc = find_control_by_cmds(cmds);
            if (gc == null)
                popupName = cmds;
            else
                popupName = gc.name;
            popup_cmds(popupName, cmds);
        }
        private void changeArch(String arch)
        {
            string nbits = r2pw.run("e asm.bits");
            r2pw.run(string.Format("e asm.arch = {0}; aaa", arch));
            try
            {
                rconfig.save("gui.hexdigits", int.Parse(nbits) / 8);
            }
            catch (Exception e) { r2pw.Show(e.ToString(), "changeArch"); }
        }
        private void changeCpu(String cpu)
        {
            string nbits = null;
            r2pw.run(string.Format("e asm.cpu = {0}; aaa", cpu));
            nbits = r2pw.run("e asm.bits");
            try
            {
                rconfig.save("gui.hexdigits", int.Parse(nbits) / 8);
            }
            catch (Exception e) { r2pw.Show(e.ToString(), "changeCpu"); }
        }
        private string num2hex() // decorator
        {
            int hexdigits = int.Parse(rconfig.load<int>("gui.hexdigits", 8));
            string format = "0x{0:x" + (hexdigits / 2).ToString() + "}";
            string value = get_decoratorvalue_string();
            return string.Format(format, Int64.Parse(value));
        }
        private string short_addr_name()
        {
            string lstname_short_address = null;
            decoratorParam dp = (decoratorParam)r2pw.decorator_param;
            Newtonsoft.Json.Linq.JObject row = (Newtonsoft.Json.Linq.JObject)dp.json_row;
            ListViewItem listview_item = dp.listviewItem;
            lstname_short_address = dp.value;
            string type = "";
            if(row["type"]!=null) type=row["type"].ToString();
            if (type.Equals("fcn"))
            {
                if (lstname_short_address.StartsWith("fcn."))
                {
                    listview_item.ForeColor = r2pw.get_color_address("fg", type, listview_item.ForeColor);
                    listview_item.BackColor = r2pw.get_color_address("bg", type, listview_item.BackColor);
                    lstname_short_address = ""; // that info is in address
                }
                else if (lstname_short_address.StartsWith("sym.imp."))
                {
                    listview_item.ForeColor = r2pw.get_color_address("fg", "imp", listview_item.ForeColor);
                    listview_item.BackColor = r2pw.get_color_address("bg", "imp", listview_item.BackColor);
                    lstname_short_address = lstname_short_address.Substring(8); // remove sys.imp.
                    listview_item.SubItems[0].Text = "imp"; // todo: find column type (not use 0)
                }
                else if (lstname_short_address.StartsWith("sub."))
                {
                    listview_item.ForeColor = r2pw.get_color_address("fg", "sub", listview_item.ForeColor);
                    listview_item.BackColor = r2pw.get_color_address("bg", "sub", listview_item.BackColor);
                    lstname_short_address = lstname_short_address.Substring(4); // remove sys.imp.
                    listview_item.SubItems[0].Text = "fcn"; // todo: find column type (not use 0)
                }
                else if (lstname_short_address.StartsWith("sym."))
                {
                    listview_item.ForeColor = r2pw.get_color_address("fg", "sym", listview_item.ForeColor);
                    listview_item.BackColor = r2pw.get_color_address("bg", "sym", listview_item.BackColor);
                    lstname_short_address = lstname_short_address.Substring(4); // remove sys.imp.
                    listview_item.SubItems[0].Text = "sym"; // todo: find column type (not use 0)
                }
                else
                {
                    listview_item.ForeColor = r2pw.get_color_address("fg", lstname_short_address, Color.FromName("yellow"));
                    listview_item.BackColor = r2pw.get_color_address("bg", lstname_short_address, Color.FromName("blue"));
                }
            }
            if (type.Equals("loc") && lstname_short_address.StartsWith("loc."))
            {
                listview_item.ForeColor = r2pw.get_color_address("fg", type, listview_item.ForeColor);
                listview_item.BackColor = r2pw.get_color_address("bg", type, listview_item.BackColor);
                lstname_short_address = ""; // that info is in address
            }
            if (type.Equals("sym"))
            {
                if (lstname_short_address.StartsWith("sym.imp."))
                {
                    listview_item.ForeColor = r2pw.get_color_address("fg", "imp", listview_item.ForeColor);
                    listview_item.BackColor = r2pw.get_color_address("bg", "imp", listview_item.BackColor);
                    lstname_short_address = lstname_short_address.Substring(8); // remove sys.imp.
                    listview_item.SubItems[0].Text = "imp";
                }
                else if (lstname_short_address.StartsWith("sym."))
                {
                    lstname_short_address = lstname_short_address.Substring(4); // remove sym.
                }
            }

            return lstname_short_address;
        }
        private string dec_b64() // decorator
        {
            string value = null;
            byte[] data = null;

            value = get_decoratorvalue_string();    
            data=Convert.FromBase64String(value);
            return Encoding.UTF8.GetString(data);
        }
        private string get_decoratorvalue_string()
        {
            string value = null;
            if (r2pw.decorator_param.GetType() == typeof(decoratorParam))
            {
                decoratorParam dp = (decoratorParam)r2pw.decorator_param;
                value = dp.value;
            }
            if (r2pw.decorator_param.GetType() == typeof(string))
                value = (string)r2pw.decorator_param;
            return value;
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
                refresh_functions_listview();
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
            if (r2pw != null)
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
            close_selected_tab();
        }
        private void close_selected_tab()
        {
            string controlName = tabControl1.SelectedTab.Text;
            if (locked_tabs.Contains(controlName)) return;
            tabControl1.TabPages.Remove(tabControl1.SelectedTab);
            r2pw.controls.Remove(controlName); //check this
            r2pw.gui_controls.remove_control_byName(controlName);
        }
        private void hide_selected_tab()
        {
            tabControl1.SelectedTab.Hide();
        }
        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try // may fail
            {
                Clipboard.SetText(txtOutput.SelectedText.Replace("\n", "\r\n"));
            }
            catch (Exception) { }
        }
        private void webBrowser1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            bool controlPressed = e.Modifiers == Keys.Control;
            switch (e.KeyValue)
            {
                case 69: // e key
                    if( controlPressed ) 
                        r2pw.gotoAddress("entry0");
                    break;
                case 27: // esc key
                    if (skip_next_keydown == true)
                    {
                        skip_next_keydown = false;
                        return;
                    }
                    webBrowser1.GoBack();
                    skip_next_keydown = true;
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
            if (r2pw != null)
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
            string tabTag = selected_tab("tag");
            // send to output control as string
            maximize("output");
            if (tabTag != null) cmds = tabTag;
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
        public webbrowser_container_form popup_tab()
        {
            WebBrowser webbrowser = new WebBrowser();
            string new_controlName = null;
            string tabTitle = selected_tab("title");
            String timeStamp = r2pw.get_timestamp();
            new_controlName = genControlName(tabTitle); // generate a short name for the control            
            new_controlName += "_" + timeStamp; // add some "mark" (timestamp)
            GuiControl gui_control_tab = r2pw.gui_controls.findControlBy_tabTitle(tabTitle);
            GuiControl gui_control = r2pw.add_control(
                new_controlName, webbrowser, "popup" + "_" + timeStamp, gui_control_tab.cmds);
            webbrowser_container_form webFrm = new webbrowser_container_form(r2pw, gui_control);

            webbrowser.Dock = DockStyle.Fill;
            webFrm.Controls.Add(webbrowser);
            webFrm.Width = Width - splitContainer1.SplitterDistance;
            webFrm.Height = Height;
            refresh_control(gui_control);
            string frmTitle = tabTitle;
            if (!tabTitle.Contains("(") && !tabTitle.Equals(gui_control.cmds))
                frmTitle = string.Format("{0} ( {1} )", tabTitle, gui_control_tab.cmds);
            webFrm.Text = frmTitle;
            webFrm.Show();
            return webFrm;
        }
        private string genControlName(string longName)
        {
            string controlName = longName;
            if (controlName.Contains("(")) // trim till "("
            {
                int pos = controlName.IndexOf("(");
                controlName = controlName.Substring(0, pos - 1);
            }
            controlName = controlName.Replace(" ", ""); //remove spaces
            return controlName;
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
            initialize_esil();
        }
        private void initialize_esil()
        {
            string pc = ""; // new eip/pc for ESIL emulation
            esil_initilized = true;
            ESILcmds("aei");
            pc = r2pw.run("? $$~[1]").Replace("\n", "");
            pc = Prompt("Init ESIL", "Starting address ( pc )", pc);
            if (pc != null)
            {
                r2pw.run("s " + pc);
                ESILcmds("aeip");
                ESILcmds("aeim");
                //ESILcmds("aer eip = " + pc);
                //ESILcmds("aer rip = " + pc);
                //ESILcmds("aep = " + pc); // don't work?
            }
            popup_cb("aerj");
            refresh_popups();
        }
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (esil_initilized == false)
                initialize_esil();
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
        public string selected_tab(string attr)
        {
            string text = null;
            try // may fail
            {
                if (attr.Equals("title")) text = tabControl1.SelectedTab.Text;
                if (attr.Equals("tag") && tabControl1.SelectedTab.Tag != null)
                    text = tabControl1.SelectedTab.Tag.ToString();
                if (attr.Equals("controlName"))
                    text = r2pw.findControlBy_tabTitle(attr);
            }
            catch (Exception) { } // better manage
            return text;
        }
        private void maximizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            maximize("output");
        }
        private void maximize(string controlName = "")
        {
            splitContainer1.SplitterDistance = 0;
            if (controlName.Equals("output"))
                splitContainer2.SplitterDistance = 0;
            else
                splitContainer2.SplitterDistance = Height - 146;
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
                    //if (splitContainer1.SplitterDistance == 118)
                    //    todo("elf", "invalid distance");
                    if (splitContainer1.SplitterDistance < 118)
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
                r2pw.Show(e.ToString(), "autoresize_output()");
            }
        }
        private void tabControl1_Click(object sender, EventArgs e)
        {
            //autoresize_output();
            cmbCmdline.Focus();
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
        private GuiControl find_control_by_cmds(string cmds)
        {
            return r2pw.gui_controls.findControlBy_cmds(cmds);
        }
        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            refresh_tab();
        }
        public void refresh_control(GuiControl control, bool timeout = false)
        {
            if (control != null && control.cmds != null)
            {
                List<string> column_titles = control.column_titles;
                if (column_titles == null)
                {
                    if ( control.control!=null && control.control.GetType() == typeof(ListView))
                    {
                        r2pw.save_active_cols(control.name, (ListView)control.control);
                        column_titles = control.column_titles;
                    }
                }
                if (timeout == false)
                    r2pw.run(control.cmds, control.name, false, column_titles,null,false,true); // no timeout
                else
                    r2pw.run_task(control.cmds, control.name, false, column_titles); // with timeout
            }
        }
        public void refresh_tab()
        {
            string tabTitle = selected_tab("title");
            GuiControl gui_control = find_control_by_title(tabTitle);
            if (gui_control != null)
            {
                //r2pw.Show("refresh_tab(): gui_control found " + gui_control.ToString() + " for title '" + tabTitle + "'","refresh_tab()");
                refresh_control(gui_control);
            }
            refresh_popups();
        }
        public string Prompt(string text, string caption, string defval = "")
        {
            askForm frm = new askForm();
            string answer = frm.Prompt(text, caption, defval, frm, this);
            if (answer != null) answer = answer.Replace("\n", "");
            return answer;
        }
        public string FindFile(string FileName, string Title)
        {
            return Prompt(FileName + " location?", Title, FileName);
        }
        private void runToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string binary_path = null;
            if (r2pw == null) return;
            maximize("output");
            binary_path = Prompt("Program to attach:", "debug", @"-d c:\windows\syswow64\notepad.exe");
            if (binary_path != null)
                LoadFile(binary_path);
        }
        private void zoomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            maximize();
        }
        private void openfileposttxtToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (r2pw == null) return;
            r2pw.run_script("openfile_post.txt");
        }
        private void pathsToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            if (r2pw == null) return;
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
            if (r2pw == null) return;
            r2pw.run("Pn", "Notes");
        }
        private void refreshToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            refresh_functions_listview();
        }
        private void refresh_functions_listview()
        { 
            GuiControl gui_control = find_control_by_name("functions_listview");
            refresh_control(gui_control); // need timeout here, but command *j fails sometimes :_/
        }
        private void reloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            update_archs(); // todo: remove option
        }
        private void yesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            r2pw.long_command_output = false;
        }
        private void noToolStripMenuItem_Click(object sender, EventArgs e)
        {
            r2pw.long_command_output = true;
        }
        private void yesToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            r2pw.autorefresh_activetab = true;
        }
        private void noToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            r2pw.autorefresh_activetab = false;
        }
        private void reloadToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            update_cpus(); // todo: remove option
        }
        private void refreshAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            refresh_popups();
        }
        public void refresh_popups()
        {
            try
            {
                foreach (GuiControl c in r2pw.gui_controls.controls)
                {
                    if (c.tabTitle != null && c.tabTitle.StartsWith("popup_") && c.synchronize == true)
                        refresh_control(c);
                }
            }
            catch (Exception) { }; // may fail, better catch
        }
        private void tabControl1_DoubleClick(object sender, EventArgs e)
        {
            maximize();
        }
        private void shellRadare2exeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            exec_process(rconfig.load<string>("r2path"), fileName);
        }
        public void exec_process(string binaryPath, string args = "")
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = binaryPath;
            startInfo.Arguments = "\""+args+"\"";
            Process.Start(startInfo);
        }
        private void copyNameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try // may fail
            {
                Clipboard.SetText(listView1.SelectedItems[0].Text);
            }
            catch (Exception) { }
        }
        public void selectFunction(string address){
            ListViewItem item   = null;
            string fcnName      = address; // 1st column of listview
            item                = listView1.FindItemWithText(fcnName);
            if (item!=null)
            {
                listView1.SelectedIndices.Clear();
                item.Selected = true;
                item.EnsureVisible();
                //Console.WriteLine(item);
            }
        }
        private string get_currentlistview_selected_address()
        {
            string address = null;
            GuiControl gui_control = r2pw.gui_controls.get_active_control();
            if (gui_control.control.GetType() == typeof(ListView))
            {
                ListView lstView = (ListView)gui_control.control;
                if (lstView.Items.Count > 0)
                {
                    int addr_index = -1, i = 0;
                    foreach (ColumnHeader c in lstView.Columns)
                    {
                        if (c.Text.Equals("vaddr") || c.Text.Equals("addr") || c.Text.Equals("plt"))
                            addr_index = i;
                        i++;
                    }
                    if (addr_index != -1) address = lstView.SelectedItems[0].SubItems[addr_index].Text;
                }
            }
            return address;
        }
        private void addressToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string selected_address = get_currentlistview_selected_address();
            if (selected_address != null)
                Clipboard.SetText(selected_address);
        }
        private void copyAllFieldsMenuItem_Click(object sender, EventArgs e)
        {
            GuiControl gui_control = r2pw.gui_controls.get_active_control();
            if (gui_control.control.GetType() == typeof(ListView))
            {
                ListView lstView = (ListView)gui_control.control;
                if (lstView.Items.Count > 0)
                {
                    string items_as_csv = "";
                    foreach (ListViewItem row in lstView.SelectedItems)
                    {
                        if(items_as_csv.Length>0) items_as_csv+="\n";
                        for (int i = 0; i < row.SubItems.Count; i++)
                        {
                            items_as_csv += row.SubItems[i].Text + ";";
                        }
                    }
                    try
                    {
                        Clipboard.SetText(items_as_csv);
                    }
                    catch (Exception) { } // may fail
                }                    
            }
        }
        private void lstStrings_DoubleClick(object sender, EventArgs e)
        {
            string selected_address = get_currentlistview_selected_address();
            if (selected_address != null)
                r2pw.gotoAddress(selected_address);
        }
        private void lstImports_DoubleClick(object sender, EventArgs e)
        {
            string selected_address = get_currentlistview_selected_address();
            if (selected_address != null)
                r2pw.gotoAddress(selected_address);
        }
        private void lstSections_DoubleClick(object sender, EventArgs e)
        {
            string selected_address = get_currentlistview_selected_address();
            if (selected_address != null)
                r2pw.gotoAddress(selected_address);
        }
        private void renameAfnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if ( listView1.Items.Count == 0 ) return;
            if ( listView1.SelectedItems.Count == 0) return;
            string current_address = listView1.SelectedItems[0].Text;
            string new_name = Prompt("New name:", "Rename", current_address);
            r2pw.run(string.Format("afn {0} {1}", new_name, current_address));
            refresh_functions_listview();
        }
        private void toolStripButton1_Click_1(object sender, EventArgs e)
        {
            string pc = null;
            if ( esil_initilized == false )
                initialize_esil();
            pc = r2pw.run("? $$~[1]").Replace("\n", "");
            if (pc.Equals("0x0")) pc = "";
            pc = Prompt("Seek until ( aesu )", "Continue until address");
            if (pc != null)
            {
                ESILcmds("aesu " + pc);
            }
        }
        private int sortColumn = -1;
        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            // Determine whether the column is the same as the last column clicked.
            if (e.Column != sortColumn)
            {
                // Set the sort column to the new column.
                sortColumn = e.Column;
                // Set the sort order to ascending by default.
                listView1.Sorting = SortOrder.Ascending;
            }
            else
            {
                // Determine what the last sort order was and change it.
                if (listView1.Sorting == SortOrder.Ascending)
                    listView1.Sorting = SortOrder.Descending;
                else
                    listView1.Sorting = SortOrder.Ascending;
            }

            // Call the sort method to manually sort.
            listView1.Sort();
            // Set the ListViewItemSorter property to a new ListViewItemComparer
            // object.
            this.listView1.ListViewItemSorter = new ListViewItemComparer(e.Column,
                                                              listView1.Sorting);
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            int usage = (int)benchmarks.getCurrentCpuUsage();
            if (usage > 70)
                lblCpu.Text = "CPU "+usage.ToString()+"%";
            else
                lblCpu.Text = "";
        }
        private void listView1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (listView1.FocusedItem.Bounds.Contains(e.Location) == true)
                {
                    dynamic json_obj = null;
                    ListViewItem.ListViewSubItem item = null;
                    if( listView1.SelectedItems.Count==0 ) return;
                    item = listView1.SelectedItems[0].SubItems[listView1.SelectedItems[0].SubItems.Count-1];
                    json_obj = JsonConvert.DeserializeObject(item.Text);
                    r2pw.clean_contextmenucmd("Data refs", ctxFunctions);
                    Cursor.Current = Cursors.WaitCursor;
                    for (int i = 0; i < json_obj.Count; i++)
                    {
                        string address = "0x" + json_obj[i].ToString("x");
                        string t_address = "["+address + "] " + r2pw.run_silent("axt @ " + address);
                        r2pw.add_contextmenucmd("Data refs", t_address, address, ctxFunctions);
                    }
                    Cursor.Current = Cursors.Default;
                    // show context menu now
                    ctxFunctions.Show(Cursor.Position);
                }
            }
        }
        private void xrefsToFunctionAxgToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListViewItem.ListViewSubItem item = null;
            if (listView1.SelectedItems.Count == 0) return;
            item = listView1.SelectedItems[0].SubItems[1]; // find address
            runCmds("axg @ "+item.Text);
        }
        private void functionInformationAfiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string addr = null;
            ListViewItem.ListViewSubItem item = null;
            if (listView1.SelectedItems.Count == 0) return;
            item = listView1.SelectedItems[0].SubItems[1]; // find address
            addr = Prompt("Address:", "Information", item.Text);
            if( addr!=null && addr.Length>0 )
                runCmds("af @ " + addr + "; afij @ " + addr);
        }
        private void xrefsAxtjToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string address = get_selectedAddress(listView1);
            string title = null, cmds = null;
            address = Prompt("Address:", "Xrefs ( axtj @ )", address);
            if (address == null) return;
            address = address.Replace("\r", "").Replace("\n", "");
            title = "xrefs @ " + address;
            cmds = "axtj @ " + address;
            //popup_cmds("xrefs", "axtj");
            r2pw.add_control_tab(title, cmds);
            r2pw.run(cmds, title);
        }
    }
    public class ListViewItemComparer : IComparer
    {
        private int col;
        private SortOrder order;
        public ListViewItemComparer()
        {
            col = 0;
            order = SortOrder.Ascending;
        }
        public ListViewItemComparer(int column, SortOrder order)
        {
            col = column;
            this.order = order;
        }
        public int Compare(object x, object y)
        {
            int returnVal = -1;
            returnVal = String.Compare(((ListViewItem)x).SubItems[col].Text,
                            ((ListViewItem)y).SubItems[col].Text);
            // Determine whether the sort order is descending.
            if (order == SortOrder.Descending)
                // Invert the value returned by String.Compare.
                returnVal *= -1;
            return returnVal;
        }
    }
}