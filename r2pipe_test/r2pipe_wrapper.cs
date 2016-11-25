using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using Newtonsoft.Json;
using r2pipe;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace r2pipe_test
{
    public class R2PIPE_WRAPPER
    {
        // class vars
        public          IR2Pipe  r2                 =   null  ;
        public           String  current_shell      =     ""  ;
        public           String  fileName           =     ""  ;
        private           Form1  guicontrol         =   null  ;
        public          RConfig  rconfig            =   null  ;
        private      TabControl  tabcontrol         =   null  ;
        private    themeManager  theme_manager      =   null  ;
        public           r2html  r2html             =   null  ;
        public           string  decorator_param    =   null  ;
        public           string  lastAddress        =   null  ;
        private            bool  mouseMoved         =  false  ;
        // gui objects
        public  Dictionary<string, object>       controls          ;
        public  Dictionary<string, Func<string>> decorators_cb     ;
        public  Dictionary<string, List<string>> decorators_names  ;
        public  Dictionary<string, Func<string>> shellopts_cb      ;
        private Dictionary<string, string>       cached_results    ;
        // r2pipe gui commands wrapper
        public R2PIPE_WRAPPER(RConfig rconfig, Form1 frm)
        {
            this.rconfig            = rconfig;
            this.guicontrol         = frm;
            this.tabcontrol         = ((Form1)frm).tabcontrol;
            this.theme_manager      = new themeManager(rconfig);
            this.controls           = new Dictionary<string, object>();
            this.decorators_cb      = new Dictionary<string, Func<string>>();
            this.decorators_names   = new Dictionary<string, List<string>>();
            this.shellopts_cb       = new Dictionary<string, Func<string>>();
            this.cached_results     = new Dictionary<string, string>();
            this.current_shell = 
                rconfig.load<string>("gui.current_shell", "radare");
            //new Hotkeys();
        }
        /* // some problems found at dynamic tab append also timeouts
         * public string run(String cmds, String controlName=null, Boolean append = false, List<string> cols = null)
        {
            if (r2 == null) return null; // may happend if gui closed when sending commands (r2.exit)
            var task = Task.Run(() => run_task(cmds,controlName, append, cols));
            if (task.Wait(TimeSpan.FromSeconds(int.Parse(rconfig.load<int>("r2.cmd_timeout",6)))))
                return task.Result;
            else
                Show(string.Format("run: {0} Timed out",cmds),"run");
            return null;
        }*/
        public string run(String cmds, String controlName=null, Boolean append = false, List<string> cols = null, string filter = null)
        {
            string res = "";
            dynamic json_obj = null;
            if (controls.ContainsKey("output"))
            {
                string control_type = "unknown";
                if(controlName!=null && controls.ContainsKey(controlName))
                    control_type = controls[controlName].GetType().ToString();
                setText("output", "", 
                    string.Format(
                        "r2.RunCommand(\"{1}{2}\"): target='{0}' type='{3}' cols='{4}'\n",
                        controlName, 
                        cmds,
                        filter != null ? "~"+filter : "",
                        current_shell, control_type,
                        cols   != null ? string.Join(", ", cols) : ""),
                    true);
            }
            if (r2 == null && cmds!=null)
            {
                Show(string.Format("{0}\nR2PIPE_WRAPPER: run(): {1}: IR2Pipe is null", cmds, controlName), "Wops!");
                return null;
            }
            if (controlName!=null && !controls.ContainsKey(controlName))
            {
                add_control_tab(controlName, cmds);
            }
            if (controlName!=null && !controls.ContainsKey(controlName))
            {
                Show(string.Format("{0}\ncontrols: control '{1}' not found...", cmds, controlName), "Wops!");
                return null;
            }
            Cursor.Current = Cursors.WaitCursor;
            update_statusbar(cmds);
            if (cmds != null)
            {
                string cmds_new = cmds;
                if (filter != null) // apply filter (~) to cmds
                    cmds_new = string.Format("{0}~{1}", cmds, filter);
                switch (current_shell)
                {
                    case "radare2":
                        res = r2.RunCommand(cmds_new).Replace("\r", "");
                        break;
                    case "javascript":
                        res = invokeJavascript(cmds, filter);
                        break;
                    default:
                        Show(string.Format("R2PIPE_WRAPPER: run(): current_shell='{0}'",
                                            current_shell), "unknown shell");
                        break;
                }
            }
            if(res != null && (res.StartsWith("[") || res.StartsWith("{")))
            try
            {

                string resultString = escape_json(res);
                json_obj = JsonConvert.DeserializeObject(resultString);
            }
            catch (Exception e)
            {
                if (cmds.EndsWith("j")) Show(e.ToString(), "json deserialize error");
            }
            if (controlName != null)
            {
                setText(controlName, cmds, res, append, json_obj, cols);
                if (cached_results.ContainsKey(controlName)) cached_results.Remove(controlName);
                cached_results.Add(controlName, res);
            }
            Cursor.Current = Cursors.Default;
            return res;
        }
        public void setText(string controlName, string cmds, string someText, bool append = false, dynamic json_obj = null, List<string> cols = null)
        {
            object c = null;
            if (!controls.ContainsKey(controlName))
            {
                // some problems trying to use: add_control_* here ...
                Show(string.Format("setText: control {0} not found, please 'add_control'",controlName),"error");
                return;
            }
            c = controls[controlName];
            if (r2 == null) return; // may happend if the gui is closed while using it (silent escape)
            if (c.GetType() == typeof(RichTextBox))
            {
                RichTextBox rtbox = (RichTextBox)c;
                if (rtbox.InvokeRequired)
                {
                    SetTextCallback d = new SetTextCallback(setText);
                    rtbox.Invoke(d, new object[] { controlName, cmds, someText, append, json_obj, cols });
                }
                else
                {
                    if (!append) rtbox.Text = "";
                    
                    rtbox.Text += r2html.encodeutf8(someText);
                }
            }
            else if (c.GetType() == typeof(ListView))
            {
                ListView lstview = (ListView)c;
                lstview.Invoke(new BeginListviewUpdate(listviewUpdate), new object[] { lstview, true, cols });
                if (json_obj != null)
                {
                    try // sometimes fails
                    {
                        for (int i = 0; i < json_obj.Count; i++)
                        {
                            string col0 = json_obj[i][cols[0]];
                            col0 = decorate(controlName, cols[0], col0);
                            ListViewItem row_item = new ListViewItem(col0);
                            for (int j = 1; j < cols.Count; j++)
                            {
                                string cname = cols[j];
                                if (json_obj[i][cname] != null) 
                                {
                                    string value = json_obj[i][cname].ToString();
                                    value = decorate(controlName, cname, value);
                                    ListViewItem.ListViewSubItem subitem = row_item.SubItems.Add(value);
                                }
                            }
                            lstview.Invoke(new AddToListviewCallback(listviewAdd), new object[] { lstview, row_item });
                        }
                    }
                    catch (Exception) { }
                }
                else
                {
                    Console.WriteLine(string.Format("setText: controlName='{0}' type='{1}' no json results received?", controlName, c.GetType()));
                }
                lstview.Invoke(new BeginListviewUpdate(listviewUpdate), new object[] { lstview, false, null });
            }
            else if (c.GetType() == typeof(WebBrowser))
            {
                sendToWebBrowser(controlName, cmds, someText, json_obj);
            }
            else
            {
                Show(string.Format("setText: controlName='{0}' Unknown control:{1}", controlName, c.GetType()), "unknown control type");
            }
        }
        delegate void SetTextCallback(string controlName, string cmds, string someText, bool append = false, dynamic json_obj = null, List<string> cols = null);
        public object get_selected_control()
        {
            string controlName = null;
            object control = null;
            try
            {
                controlName = tabcontrol.SelectedTab.Text;
            }
            catch (Exception) { }
            if (controlName == null) return null;
            if (!controls.ContainsKey(controlName))
            {
                string tag = null;
                try
                {
                    tag = tabcontrol.SelectedTab.Text.ToString().ToLower();
                    tag = tag.Replace(" ", ""); // "Hex view"? space (no spaces on controlName(s))
                }
                catch (Exception) { }
                if (tag == null) return null;
                controlName = tag;
            }
            control = controls[controlName];
            return control;
        }
        public string invokeJavascript(string cmds, string filter = null)
        {
            WebBrowser webBrowser1 = null;
            object control = null;
            control = get_selected_control();
            if (control == null || control.GetType() != typeof(WebBrowser))
            {
                Show(string.Format("invokeJavascript(): incompatible control '{0}'\n",
                    (string)control), "error");
                return null;
            }
            webBrowser1 = (WebBrowser)control;
            if( webBrowser1.Document != null )
            {
                object[] args = { cmds };
                try
                {
                    string res = webBrowser1.Document.InvokeScript("eval", args).ToString();
                    res += "\n";
                    return res;
                }
                catch (Exception)
                {
                    return null; // better manage req
                }
            }
            return null;
        }
        public string decorate(string controlName, string columName, string value)
        {
            string decorator = null;
            foreach (string key in decorators_names.Keys)
            {
                if (decorators_names[key].Contains(columName)) decorator = key;
            }
            if ( decorator == null ) return value;
            Func<string> decorator_cb = findDecorator_callback(decorator);
            decorator_param = value;
            return decorator_cb();
        }
        private Func<string> findDecorator_callback(string decoratorName)
        {
            return decorators_cb[decoratorName];
        }
        public void sendToWebBrowser(string controlName, string cmds, string someText, dynamic json_obj)
        {
            object c = controls[controlName];
            string url;
            if (someText == null && cached_results.ContainsKey(controlName)) 
                someText = cached_results[controlName];
            url = BuildWebPage((WebBrowser)c, controlName, cmds, someText, json_obj);
            ((WebBrowser)c).DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.webBrowser_DocumentCompleted);
            try
            {
                ((WebBrowser)c).Navigate(url);
            }
            catch (Exception) { } // better manage
        }
        public void set_theme(string themeName)
        {
            theme_manager.set_theme(themeName);
            foreach (object o in controls)
            {
                if (o.GetType() == typeof(WebBrowser))
                {
                    ((WebBrowser)o).Refresh();
                }
            }
        }
        public void reload_theme()
        {
            if (theme_manager.themeName != null)
                set_theme(theme_manager.themeName);
        }
        public string Prompt(string text, string caption, string defval = "")
        {
            askForm frm = new askForm();
            string answer = frm.Prompt(text, caption, defval, frm);
            if ( answer != null ) answer = answer.Replace("\n", "");
            return answer;
        }
        private string BuildWebPage(WebBrowser wBrowser, string controlName, string cmds, string someText, dynamic json_obj)
        {
            string tmpName = null;
            MatchCollection mc_addresses = null;
            // webpage(s) will be saved to temp path 
            // other support files will be referenced
            tmpName = string.Format("{0}_{1}.html", controlName, cmds);
            tmpName = (new Regex(@"([\\\/>\~])")).Replace(tmpName, "");
            tmpName = tmpName.Replace("?", "[question]");
            tmpName = rconfig.tempPath + Path.GetFileName(tmpName);
            using (StreamWriter sw = new StreamWriter(tmpName))
            {
                sw.WriteLine(r2html.convert(cmds, someText, json_obj, ref mc_addresses));
            }
            return tmpName;                
        }
        private void webBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            try // because may be already hooked
            {
                ((WebBrowser)sender).Document.Body.MouseUp += new HtmlElementEventHandler(webBrowser_MouseUp);
                ((WebBrowser)sender).Document.Body.MouseDown += new HtmlElementEventHandler(webBrowser_MouseDown);
                ((WebBrowser)sender).Document.Body.MouseMove += new HtmlElementEventHandler(webBrowser_MouseMove);
            }
            catch (Exception) { }
        }
        void webBrowser_MouseDown(Object sender, HtmlElementEventArgs e)
        {
            mouseMoved = false;
        }
        void webBrowser_MouseMove(Object sender, HtmlElementEventArgs e)
        {
            mouseMoved = true;
        }
        void webBrowser_MouseUp(Object sender, HtmlElementEventArgs e)
        {
            HtmlElement browser = (HtmlElement)sender;
            switch (e.MouseButtonsPressed)
            {
                case MouseButtons.Left:
                    HtmlElement element = browser.Document.GetElementFromPoint(e.ClientMousePosition);
                    if ( element!=null && element.OuterText != null )
                    {
                        string text = element.OuterText.Replace(" ", "");
                        string innertext = element.InnerText.Replace(" ", ""); ;
                        string tagname = element.TagName;
                        if (mouseMoved == false && tagname.Equals("SPAN"))
                        {
                            bool selected = element.OuterHtml.Contains("_selected");
                            if( selected && text.StartsWith("0x") == true )
                                gotoAddress(text);
                        }
                    }
                    break;
            }
        }
        public void gotoAddress(string address)
        {
            if (address!=null && address.Length>0 && address != lastAddress)
            {
                string res;
                res=run("pdf @ " + address, "dissasembly");
                if(res.Length == 0)
                    res = run("pd 200 @ " + address, "dissasembly");
                run("px 2000 @ " + address, "hexview");
                lastAddress = address;
            }
            tabcontrol.SelectedIndex = 0;
        }
        public delegate void BeginListviewUpdate(ListView lstview, bool update, List<string> cols);
        public delegate void AddToListviewCallback(ListView lstview, ListViewItem item);
        public void listviewUpdate(ListView lstview, bool update = true, List<string> cols = null)
        {
            if (update)
            {
                lstview.BeginUpdate();
                lstview.Clear();
                if (cols != null)
                {
                    int i = 0;
                    int col_width = ( lstview.Width - 20 ) / cols.Count;
                    lstview.Columns.Clear();
                    foreach (string cname in cols) // add values (rows) to listview
                    {
                        lstview.Columns.Add(cname);
                        lstview.Columns[i].Width = col_width;
                        lstview.Columns[i].TextAlign = HorizontalAlignment.Right;
                        i++;
                    }
                }
            }
            else lstview.EndUpdate();

        }
        public void listviewAdd(ListView lstview, ListViewItem item)
        {
            item.ToolTipText = item.Text;
            item.ImageIndex = 1;
            lstview.Items.Add(item);
        }
        public bool add_control(string name, object control)
        {
            if (controls.ContainsKey(name)) return false;
            controls.Add(name, control);
            if (control.GetType() == typeof(WebBrowser))
            {
                ((WebBrowser)control).PreviewKeyDown -= new PreviewKeyDownEventHandler(webBrowser_PreviewKeyDown);
                ((WebBrowser)control).PreviewKeyDown += new PreviewKeyDownEventHandler(webBrowser_PreviewKeyDown);
                ((WebBrowser)control).WebBrowserShortcutsEnabled = true;
                ((WebBrowser)control).Refresh();
            }
            return true;
        }
        public void add_decorator(string name, Func<string> callback, List<string> fieldNames)
        {
            this.decorators_cb.Add(name, callback);
            this.decorators_names.Add(name, fieldNames);
        }
        public void add_shellopt(string name, Func<string> callback)
        {
            this.shellopts_cb.Add(name, callback);
        }
        public void add_menufcn(string menuName, string text, string args, Action<string> callback, MenuStrip menu)
        {
            ToolStripMenuItem item = find_menucmd(menuName, menu);
            if (item != null)
            {
                ToolStripItem newitem = null;
                object[] callback_args = new object[] { callback, args };
                string menuText = "";
                if (text.Length > 0)
                    menuText = string.Format("{0}: {1}", text, args);
                else
                    menuText = args;
                newitem = item.DropDownItems.Add(menuText);
                newitem.Tag = callback_args;
                newitem.Click += new EventHandler(MenuItemClick_CallbackHandler);
            }
        }
        public void add_control_tab(string tabname, string cmds)
        {
            if (controls.ContainsKey(tabname)) return;
            var page = new TabPage(tabname);
            WebBrowser browser = null;
            try
            {
                browser = new WebBrowser();
            }
            catch (Exception e)
            {
                Show(e.ToString(), "add_control_tab: browser");
            }
            page.Tag = tabname.ToLower();
            page.ImageIndex = 1;
            if (browser != null)
            {
                browser.Dock = DockStyle.Fill;
                browser.Navigate("about:" + cmds);
                page.Controls.Add(browser);
            }
            try
            {
                tabcontrol.TabPages.Add(page);
                page.Select();
                tabcontrol.SelectedTab = page;
            }
            catch (Exception e)
            {
                Show(e.ToString(), "add_control_tab: page");
            }
            if (browser != null)
                add_control(tabname, browser);
            guicontrol.autoresize_output();
        }
        private void webBrowser_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.G) //71 g keyvalue
            {
                string address = Prompt("Address:", "goto address");
                gotoAddress(address);
            }
        }
        public void add_menucmd(string menuName, string text, string cmds, MenuStrip menu, string decorator = null)
        {
            ToolStripMenuItem item = find_menucmd(menuName, menu);
            ToolStripItem newitem = null;
            if (item == null)
            {
                Show(string.Format("Menu '{0}' not found...", menuName), "add_meucmd");
                return;
            }
            newitem  = item.DropDownItems.Add(string.Format("{0} ( {1} )", text, cmds));
            newitem.Tag = cmds;
            newitem.Click += new EventHandler(MenuItemClickHandler);
        }
        public void open(String fileName)
        {
            if (this.r2 == null)
                this.r2 = new R2Pipe(fileName, rconfig.r2path);
            else
                this.r2.RunCommand("o " + fileName);
            this.fileName = fileName;
            this.r2html = new r2html(this);
            if (!fileName.Equals("-"))
            {
                rconfig.save("gui.lastfile", fileName);
            }
        }
        public ToolStripMenuItem find_menucmd(string menuName, MenuStrip menu)
        {
            foreach (ToolStripMenuItem item in menu.Items)
            {                
                if (item.Text.Equals(menuName))
                {
                    return item;
                }
                if (item.HasDropDownItems)
                {
                    foreach (object subitem in item.DropDownItems)
                    {
                        if(subitem.GetType() == typeof(ToolStripMenuItem))
                        {
                            if (((ToolStripMenuItem)subitem).Text.Equals(menuName))
                            {
                                return (ToolStripMenuItem)subitem;
                            }
                        }
                    }
                }
            }
            return null;
        }
        private void MenuItemClick_CallbackHandler(object sender, EventArgs e)
        {
            System.Windows.Forms.ToolStripItem item = ((System.Windows.Forms.ToolStripItem)(sender));
            object [] args = (object []) item.Tag;
            ((Action<string>)args[0])((String)args[1]);            
        }
        private void MenuItemClickHandler(object sender, EventArgs e)
        {
            System.Windows.Forms.ToolStripItem item = ((System.Windows.Forms.ToolStripItem)(sender));
            string cmds = item.Tag.ToString();
            run(cmds, item.Text);
        }
        public DialogResult Show(string text, string caption)
        {
            if (controls.ContainsKey("output"))
                setText("output", "", string.Format("{0} {1}", caption, text), true);
            return MessageBox.Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void update_statusbar(string cmds) // called from run
        {
            string seekpos = r2.RunCommand("s").Replace("\n", "").Replace("\r", "");
            this.guicontrol.show_message(
                string.Format("{0} {1} [{2}] > {3}",
                    guicontrol.fileType, Path.GetFileName(fileName), seekpos, cmds));
        }
        public string readFile(string fileName, bool use_guiPath = true)
        {
            if (use_guiPath)
            {
                fileName = string.Format(@"{0}\{1}", rconfig.dataPath, fileName);
            }
            if (!File.Exists(fileName))
            {
                Show(string.Format("Wops!\nr2html: readFile():\nfileName='{0}'\nnot found in data path...", fileName), "readfile");
                return "file not found...";
            }
            return System.IO.File.ReadAllText(fileName);
        }
        public void next_shell()
        {
            string new_shell = current_shell;
            string first_shell = null;
            bool use_next = true;
            foreach (string shellname in shellopts_cb.Keys)
            {
                if (first_shell == null) first_shell = shellname;
                if ( use_next == false )
                {
                    new_shell = shellname;
                    break;
                }
                if (shellname.Equals(current_shell)) use_next = false;
            }
            if (new_shell.Equals(current_shell)) current_shell = first_shell;
            else current_shell = new_shell;
            rconfig.save("gui.current_shell", current_shell);
            guicontrol.UpdateGUI();
            shellopts_cb[current_shell]();
        }
        public void run_script(string scriptFileName)
        {
            // 1. read input from scriptFilename
            // 2. parse fields: <controlName[,bAppend,['col1','col2',...]> <r2 commands>
            run("e scr.utf8 = true", "output", true);            
            run("aa;aflj", "functions_listview", false, new List<string> { "name", "offset" });
            run("pdf", "dissasembly");
            run("izj", "strings_listview", false, new List<string> { "string", "vaddr", "section", "type" });
            run("iij", "imports_listview", false, new List<string> { "name", "plt" });
            run("iSj", "sections_listview", false, new List<string> { "name", "size", "flags", "paddr", "vaddr" });
            run("dpj", "processes_listView", false, new List<string> { "path", "status", "pid" });
            run("pxa 2000", "hexview");
            run("aaa;aflj", "functions_listview", false, new List<string> { "name", "offset" });
            // run("axtj @ entry0", "xrefs ( axtj )");
            guicontrol.script_executed_cb();
        }
        public void show_processes(string filter=null)
        {
            run("dpj", "processes_listView", false, new List<string> { "path", "status", "pid" }, filter);
        }
        public string find_dataPath(string def="")
        {
            string path = Prompt("gui media path?", "Please, locate your data path...", def);
            rconfig.save("gui.datapath", path);
            rconfig.save("gui.hexview.css", "r2pipe.css");
            rconfig.save("gui.theme_name", guicontrol.themeName);
            return path;
        }
        public string escape_json(string r2_json)
        {
            return Regex.Replace(r2_json,
                @"(?<!\\)  # lookbehind: Check that previous character isn't a \
                \\         # match a \
                (?!\\)     # lookahead: Check that the following character isn't a \",
                @"\\", RegexOptions.IgnorePatternWhitespace);
        }
        public void exit()
        {
            try
            {
                rconfig.save("gui.current_shell", "radare2");
                this.r2.RunCommand("q");
            }
            catch (Exception) { };
            this.r2 = null;
        }
    }
}