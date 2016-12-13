using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using Newtonsoft.Json;
using r2pipe;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Drawing;

namespace r2pipe_test
{
    public class R2PIPE_WRAPPER
    {
        // class vars
        public          IR2Pipe  r2                  =   null  ;
        public           String  current_shell       =     ""  ;
        public           String  fileName            =     ""  ;
        public            Form1  guicontrol          =   null  ;
        public          RConfig  rconfig             =   null  ;
        private      TabControl  tabcontrol          =   null  ;
        private    themeManager  theme_manager       =   null  ;
        public           r2html  r2html              =   null  ;
        public           object  decorator_param     =   null  ;
        public           string  lastAddress         =   null  ;
        public      GuiControls  gui_controls        =   null  ;
        public             bool  long_command_output =  false  ;
        public           UInt64  seek_address        =      0  ;
        public             bool  autorefresh_activetab = true  ;
        private            bool  ignore_mouse_events =  false  ;
        private            bool  debugMode           =  false  ;
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
            this.gui_controls       = new GuiControls(this);
            this.controls           = new Dictionary<string, object>();
            this.decorators_cb      = new Dictionary<string, Func<string>>();
            this.decorators_names   = new Dictionary<string, List<string>>();
            this.shellopts_cb       = new Dictionary<string, Func<string>>();
            this.cached_results     = new Dictionary<string, string>();
            this.current_shell = 
                rconfig.load<string>("gui.current_shell", "radare");
            string r2dir = System.IO.Path.GetDirectoryName(rconfig.r2path);
            //chdir to radare2 directory
            Directory.SetCurrentDirectory(r2dir);
            //new Hotkeys();
        }
        // some problems found at dynamic tab append also timeouts     
        public string run_task(String cmds, String controlName=null, Boolean append = false, List<string> cols = null, string filter = null)
        {
            if (r2 == null) return null; // may happend if gui closed when sending commands (r2.exit)
            var task = Task.Run(() => run(cmds,controlName, append, cols));
            if (task.Wait(TimeSpan.FromSeconds(int.Parse(rconfig.load<int>("r2.cmd_timeout",30)))))
                return task.Result;
            else
                Show(string.Format("run: {0} Timed out\n",cmds),"run");
            return null;
        }
        public string run(String cmds, String controlName = null, Boolean append = false, List<string> cols = null, string filter = null, bool refresh_tab = false, bool silent=false, GuiControl gc=null)
        {
            string res = "";
            dynamic json_obj = null;
            int cpu_usage = (int) guicontrol.benchmarks.getCurrentCpuUsage();
            //Console.WriteLine("[cmds] "+cmds);
            while (cpu_usage == 100) // wait for some free cpu resources
            {
                System.Threading.Thread.Sleep(50);
                cpu_usage = (int)guicontrol.benchmarks.getCurrentCpuUsage();
            }
            if (controls.ContainsKey("output"))
            {
                string control_type = "unknown";
                string output_msg = "";
                if(controlName!=null)
                {
                    if( gc==null )
                        gc = gui_controls.findControlBy_name(controlName);
                    if( gc!=null && gc.control!=null)
                        control_type = gc.control.GetType().ToString();
                }
                if (long_command_output == true)
                    output_msg = string.Format(
                        "r2.RunCommand(\"{1}{2}\"): target='{0}' type='{3}' cols='{4}'\n",
                        controlName,
                        cmds,
                        filter != null ? "~" + filter : "",
                        current_shell, control_type,
                        cols != null ? string.Join(", ", cols) : "");
                else
                {
                    if (r2 == null) return null;
                    string seekaddr = r2.RunCommand("? $$~[1]");
                    if(seekaddr!=null) 
                        seekaddr=seekaddr.Replace("\r", "").Replace("\n", "");
                    output_msg = string.Format("[{0}]> {1,-25} # {2}\n",
                        seekaddr, cmds, controlName);
                }
                if (controlName != null && silent == false)
                    setText("output", "", output_msg, true); // send command to output
            }
            if (r2 == null)
            {
                if( cmds!=null ) cmds="";
                //Show(string.Format("{0}\nR2PIPE_WRAPPER: run(): {1}: IR2Pipe is null", cmds, controlName), "Wops!");
                return null;
            }
            if (controlName!=null)
            {
                //todo: find tab index with cmds and select it if exists
                //if (!controls.ContainsKey(controlName))
                GuiControl gc_tab = gui_controls.findControlBy_name(controlName, 1);
                if( gc_tab!=null )
                    tabcontrol.SelectedIndex = gc_tab.tab_index;
                //else
                //{
                //    add_control_tab(controlName, cmds);
                //}
            }
            if (controlName!=null && gui_controls.findControlBy_name(controlName)==null) //!controls.ContainsKey(controlName))
            {
                add_control_tab(controlName, cmds);
                //Show(string.Format("{0}\ncontrols: control '{1}' not found...", cmds, controlName), "Wops!");
                //return null;
            }
            if( controlName!=null ) Cursor.Current = Cursors.WaitCursor;
            update_statusbar(cmds); // may fail
            if (cmds != null)
            {
                string cmds_new = cmds;
                if (filter != null) // apply filter (~) to cmds
                    cmds_new = string.Format("{0}~{1}", cmds, filter);
                switch (current_shell)
                {
                    case "radare":
                    case "radare2":
                        string pre_cmd = "", pos_cmd = "";
                        if (gc != null && gc.pre_cmd != null) pre_cmd = gc.pre_cmd + ";";
                        if (gc != null && gc.pos_cmd != null) pos_cmd = ";" + gc.pos_cmd;
                        if (r2 == null) return null;
                        res = r2.RunCommand(pre_cmd+cmds_new+pos_cmd);
                        break;
                    case "javascript":
                        res = invokeJavascript(cmds, filter);
                        break;
                    default:
                        Show(string.Format("R2PIPE_WRAPPER: run(): current_shell='{0}'",
                                            current_shell), "unknown shell");
                        break;
                }
                if (res != null)
                {
                    res = r2html.encodeutf8(res);
                    res = res.Replace("\r", "");
                }
                /*
                if (gc != null)
                {
                    if (gc.cmds.StartsWith("ag"))
                    {
                        guicontrol.CheckDotpath();
                        string args = null;
                        string tmpFileName = WriteTempFile(res, "temp_"+gc.sName+".dot");
                        string dotPath = rconfig.load<string>("dotPath");
                        args = string.Format("-T png \"{0}\" -o \"{1}\".png", tmpFileName, tmpFileName);
                        guicontrol.exec_process(dotPath, args);
                        res = string.Format("<img src='file:///{0}.png'>", tmpFileName);
                    }
                }*/
            }
            if(res != null && (res.StartsWith("[") || res.StartsWith("{")))
            try
            {

                    string resultString = escape_json(res);
                json_obj = JsonConvert.DeserializeObject(resultString);
            }
            catch (Exception e)
            {
                if (cmds.EndsWith("j")) setText("output",cmds,e.Message,true);
            }
            if (controlName != null)
            {
                // send results and "others" to control (ex: listview)
                setText(controlName, cmds, res, append, json_obj, cols, gc);
                if (cached_results.ContainsKey(controlName)) cached_results.Remove(controlName);
                cached_results.Add(controlName, res);
            }
            // refresh required tabs after cmds
            if (refresh_tab && autorefresh_activetab)
            {
                string tabTitle = guicontrol.selected_tab("title");
                GuiControl gui_control = gui_controls.findControlBy_tabTitle(tabTitle);
                if ( gui_control != null )
                {
                    bool need_refresh = false;
                    bool need_tabs_refresh = false;
                    // selected control cmds like commandline -> refresh tab
                    if( cmds.Length>2 )
                        need_refresh = cmds.Substring(0, 2) == gui_control.cmds.Substring(0, 2);
                    if (cmds.StartsWith("s") && cmds.Length>1) need_refresh = true;
                    if (cmds.Contains("ae") && cmds.Length>2) need_tabs_refresh = true; //todo: fix req, can be numbers at start
                    if ( need_refresh )
                        guicontrol.refresh_main_controls();
                    if (need_tabs_refresh)
                    {
                        guicontrol.refresh_tab();
                        guicontrol.refresh_popups();
                    }
                }
            }
            if (controlName != null) Cursor.Current = Cursors.Default;
            return res;
        }
        public string run_silent(string cmds)
        {
            return run(cmds, null, false, null, null, false, true).Replace("\n","").Replace("\r","");
        }
        public void setText(string controlName, string cmds, string someText, bool append = false, dynamic json_obj = null, List<string> cols = null, GuiControl gc_orig = null)
        {
            GuiControl gc = gc_orig;
            if( gc==null )
                gc=gui_controls.findControlBy_name(controlName);
            //object c = null;
            if ( gc == null)
            {
                // some problems trying to use: add_control_* here ...
                Console.WriteLine(string.Format("setText: gui_control {0} not found, please 'add_control'", controlName), "error");
                return;
            }
            //c = controls[controlName];
            if (r2 == null) return; // may happend if the gui is closed while using it (silent escape)
            if (gc.control == null)
            {
                Console.WriteLine(string.Format("setText: gc.control {0} is null, please set some control before 'setText'", controlName), "error");
                return;
            }
            if (gc.control.GetType() == typeof(RichTextBox))
            {
                RichTextBox rtbox = (RichTextBox) gc.control;
                if (rtbox.InvokeRequired)
                {
                    SetTextCallback d = new SetTextCallback(setText);
                    try
                    {
                        rtbox.Invoke(d, new object[] { controlName, cmds, someText, append, json_obj, cols, gc });
                    }
                    catch (Exception e) // may fail on closing gui
                    {
                        Show(e.ToString(), "setText callback invoke");
                    }
                }
                else
                {
                    if (!append) rtbox.Text = "";
                    rtbox.Text += someText;
                }
            }
            else if (gc.control.GetType() == typeof(ListView))
            {
                ListView lstview = (ListView) gc.control;
                if (cols == null || cols.Count == 0)
                    cols = save_active_cols(controlName, lstview);
                lstview.Invoke(new BeginListviewUpdate(listviewUpdate),
                    new object[] { lstview, true, controlName, cols });
                if (json_obj != null)
                {
                    try // sometimes fails
                    {
                        if (cols.Count > 0)
                        {
                            for (int i = 0; i < json_obj.Count; i++)
                            {
                                string col0 = json_obj[i][cols[0]];
                                col0 = decorate(controlName, cols[0], col0, json_obj[i], cols);
                                ListViewItem row_item = new ListViewItem(col0);
                                //row_item.ForeColor = Color.FromName("lime");
                                for (int j = 1; j < cols.Count; j++)
                                {
                                    string cname = cols[j];
                                    if (json_obj[i][cname] != null)
                                    {
                                        string value = json_obj[i][cname].ToString();
                                        value = decorate(controlName, cname, value, json_obj[i], cols, row_item);
                                        ListViewItem.ListViewSubItem subitem = row_item.SubItems.Add(value);
                                    }
                                }
                                lstview.Invoke(new AddToListviewCallback(listviewAdd), new object[] { lstview, row_item });
                            }
                        }
                    }
                    catch (Exception e) { MessageBox.Show(e.ToString()); }
                }
                else
                {
                    Console.WriteLine(string.Format("setText: controlName='{0}' type='{1}' no json results received?", 
                        controlName, gc.control.GetType()));
                }           
                try
                {
                    lstview.Invoke(new BeginListviewUpdate(listviewUpdate), new object[] { lstview, false, controlName, null });
                }
                catch (Exception e) // may fail when closing gui
                {
                    Show(e.ToString(), "listViewUpdate");
                }
            }
            else if (gc.control.GetType() == typeof(WebBrowser))
            {
                sendToWebBrowser(controlName, cmds, someText, json_obj);
            }
            else
            {
                Show(string.Format("setText: controlName='{0}' Unknown control:{1}", controlName, gc.control.GetType()), "unknown control type");
            }
        }
        delegate void SetTextCallback(string controlName, string cmds, string someText, bool append = false, dynamic json_obj = null, List<string> cols = null, GuiControl gc_orig=null );
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
            if(controls.ContainsKey(controlName))
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
        public string decorate(string controlName, string columName, string value, object json_row=null, List<string> cols=null, ListViewItem listviewItem=null)
        {
            string decorator = null;
            foreach (string key in decorators_names.Keys)
            {
                if (decorators_names[key].Contains(columName)) decorator = key;
            }
            if ( decorator == null ) return value;
            Func<string> decorator_cb = findDecorator_callback(decorator);
            decorator_param = value;
            if (json_row != null)
            {
                decorator_param = new decoratorParam(controlName, columName, value, decorator, json_row, cols, listviewItem, this);
            }
            return decorator_cb();
        }
        public string WriteTempFile(string contents, string filename="tmpname")
        {
            string tmpName = rconfig.tempPath + filename;
            using (StreamWriter sw = new StreamWriter(tmpName))
            {
                sw.WriteLine(contents);
            }
            return tmpName;
        }
        public void sendToWebBrowser(string controlName, string cmds, string someText, dynamic json_obj)
        {
            if (!controls.ContainsKey(controlName))
            {
                MessageBox.Show("Controls don't contain key " + controlName, 
                    "sendToWebBrowser", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
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
        private string BuildWebPage(WebBrowser wBrowser, string controlName, string cmds, string someText, dynamic json_obj)
        {
            string tmpName = null;
            MatchCollection mc_addresses = null;
            // webpage(s) will be saved to temp path 
            // other support files will be referenced
            tmpName = string.Format("{0}_{1}_{2}.html", controlName, cmds, get_timestamp());
            tmpName = (new Regex(@"([\\\/>\~\n])")).Replace(tmpName, "");
            tmpName = tmpName.Replace("?",  "[question]");
            tmpName = tmpName.Replace(":",  "[tp]");
            tmpName = tmpName.Replace("\"", "[q]");
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
                ((WebBrowser)sender).Document.Body.MouseDown += new HtmlElementEventHandler(webBrowser_MouseDown);
            }
            catch (Exception) { }
        }
        void webBrowser_MouseDown(Object sender, HtmlElementEventArgs e)
        {
            HtmlElement browser = (HtmlElement)sender;
            switch (e.MouseButtonsPressed)
            {
                case MouseButtons.Left:
                    if (ignore_mouse_events) return;
                    HtmlElement element = browser.Document.GetElementFromPoint(e.ClientMousePosition);
                    if ( element!=null && element.OuterText != null )
                    {
                        string tagname = element.TagName;
                        if (tagname.Equals("SPAN"))
                        {
                            string text = element.OuterText.Replace(" ", "");
                            string innertext = element.InnerText.Replace(" ", ""); ;
                            bool selected = element.OuterHtml.Contains("_selected");
                            if (selected == true)
                            {
                                ignore_mouse_events = true;
                                gotoAddress(text);
                                ignore_mouse_events = false;
                            }
                        }
                    }
                    break;
            }
        }
        public void gotoAddress(string address)
        {
            if (address!=null && address.Length>0 && address != lastAddress)
            {
                run("s " + address, "output", true);
                //update controls
                guicontrol.refresh_control(gui_controls.findControlBy_name("dissasembly"));
                guicontrol.refresh_control(gui_controls.findControlBy_name("hexview"));
                guicontrol.refresh_control(gui_controls.findControlBy_name("Call graph"));
                guicontrol.refresh_popups();
                guicontrol.selectFunction(address);
            }
            lastAddress = address;
            //tabcontrol.SelectedIndex = 0;
        }
        public delegate void BeginListviewUpdate(ListView lstview, bool update, string controlName, List<string> cols);
        public delegate void AddToListviewCallback(ListView lstview, ListViewItem item);
        public List<string> save_active_cols(string controlName, ListView lstview)
        {
            List<string> cols = new List<string>();
            foreach (ColumnHeader item in lstview.Columns)
            {
                cols.Add(item.Text);
            }
            //output("#todo: save cols of " + controlName+"\n"+cols.ToString());
            GuiControl control = gui_controls.findControlBy_name(controlName);
            control.set_columnTitles(cols);
            //output(control.ToString());
            return cols;
        }
        public void listviewUpdate(ListView lstview, bool update = true, string controlName = null, List<string> cols = null)
        {
            if (update)
            {
                lstview.BeginUpdate();
                if (cols == null)
                {
                    cols = save_active_cols(controlName, lstview);
                }
                if (cols != null && cols.Count > 0)
                {
                    int i = 0;
                    int col_width = (lstview.Width - 20) / cols.Count;
                    lstview.Clear();
                    lstview.Columns.Clear();
                    foreach (string cname in cols) // add colums to listview
                    {
                        lstview.Columns.Add(cname);
                        lstview.Columns[i].Tag = cname;
                        lstview.Columns[i].Width = -2;// col_width;
                        // todo: get textalign from decorators or guicontrols
                        if (!cname.Equals("name") && !cname.Equals("string") && !cname.Equals("datarefs")) 
                            lstview.Columns[i].TextAlign = HorizontalAlignment.Right;
                        i++;
                    }
                }
                
            }
            else
            {
                int i; // resize columns to "fit" contents
                for (i = 0; i < lstview.Columns.Count; i++)
                {
                    lstview.AutoResizeColumn(i, ColumnHeaderAutoResizeStyle.ColumnContent);
                    //if (lstview.Columns[i].Width < 100) lstview.Columns[i].Width = 100;
                    //if (lstview.Columns[i].Width > (guicontrol.Width * 20) / 32)
                    //    lstview.Columns[i].Width = (guicontrol.Width * 20) / 32;
                }
                lstview.EndUpdate();
            }

        }
        public void listviewAdd(ListView lstview, ListViewItem item)
        {
            item.ToolTipText = item.Text;
            item.ImageIndex = 1;
            lstview.Items.Add(item);
        }
        public void unregister_control(string controlName)
        {
            //controls.Remove(controlName);
            gui_controls.remove_control_byName(controlName);
        }
        public GuiControl add_control(string name, object control, string tabTitle = null, string cmds = null)
        {
            string pre_cmd = "", pos_cmd = "";
            if (name == null) return null;
            if (!controls.ContainsKey(name))
                controls.Add(name, control);
            if (cmds != null && cmds.Equals("agf"))
            {
                pre_cmd = "e asm.section=false";
                pos_cmd = "e asm.section=true";
            }
            GuiControl gui_control = gui_controls.add_control
                (name, control, tabTitle, cmds,
                pre_cmd, pos_cmd, tabcontrol.SelectedIndex);
            if (control.GetType() == typeof(WebBrowser))
            {
                ((WebBrowser)control).PreviewKeyDown -= new PreviewKeyDownEventHandler(webBrowser_PreviewKeyDown);
                ((WebBrowser)control).PreviewKeyDown += new PreviewKeyDownEventHandler(webBrowser_PreviewKeyDown);
                ((WebBrowser)control).WebBrowserShortcutsEnabled = true;
                ((WebBrowser)control).Refresh();
            }
            return gui_control;
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
                    menuText = string.Format("{0} ( {1} )", text, args);
                else
                    menuText = args;
                newitem = item.DropDownItems.Add(menuText);
                newitem.Tag = callback_args;
                newitem.Click += new EventHandler(MenuItemClick_CallbackHandler);
                string cname = text; // text.Replace(" ", "").ToLower();
                gui_controls.add_control(cname, null, menuText, args, "", "", tabcontrol.SelectedIndex);
            }
        }
        public void add_control_tab(string tabname, string cmds)
        {
            if ( tabcontrol == null)
                return;
            var page = new TabPage(tabname);
            WebBrowser browser = null;
            try
            {
                browser = new WebBrowser();
            }
            catch (Exception e)
            {
                //Show(e.ToString(), "add_control_tab: browser");
                return;
            }
            page.Tag = cmds; // tabname.ToLower();
            page.ImageIndex = 1;
            if (browser != null)
            {
                browser.Dock = DockStyle.Fill;
                //browser.Navigate("about:" + cmds);
                page.Controls.Add(browser);
            }
            try
            {
                tabcontrol.TabPages.Add(page);
            }
            catch (Exception e)
            {
                Show(e.ToString(), "add_control_tab: page");
                return;
            }
            if (browser != null)
            {
                page.Select();
                tabcontrol.SelectedTab = page;
                add_control(tabname, browser, tabname, cmds);
            }
            guicontrol.autoresize_output();
        }
        private void webBrowser_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            
            if (e.KeyCode == Keys.G && e.Control) //71 g keyvalue
            {
                string address = guicontrol.Prompt("Address:", "goto address");
                if (address != null)
                {
                    address = address.Replace("\r", "");
                    gotoAddress(address);
                }
            }
        }
        public void add_contextmenucmd(string menuName, string text, string cmds, ContextMenuStrip menu, string decorator = null)
        {
            ToolStripMenuItem item = find_contextmenucmd(menuName, menu);
            ToolStripItem newitem = null;
            if (item == null)
            {
                Show(string.Format("Menu '{0}' not found...", menuName), "add_meucmd");
                return;
            }
            newitem  = item.DropDownItems.Add(string.Format("{0}", text));
            newitem.Tag = cmds;
            newitem.Click += new EventHandler(ListView1ItemClickHandler);
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
            newitem = item.DropDownItems.Add(string.Format("{0} ( {1} )", text, cmds));
            newitem.Tag = cmds;
            newitem.Click += new EventHandler(MenuItemClickHandler);
        }
        public void open(String fileName)
        {
            string commandline = fileName;
            string args = "";
            string r2file = fileName;
            string r2path = rconfig.r2path;
            if (this.r2 != null) this.r2.RunCommand("q");
            this.r2 = null; // remove the object
            debugMode = false;
            if (fileName.StartsWith("-d"))
            {
                fileName = fileName.Substring(3);
                args = "-d";
                debugMode = true;
            }
            if (fileName.StartsWith("dbg://"))
            {
                fileName = fileName.Substring(6);
                args = "dbg://";
                debugMode = true;
            }
            string quotedFileName = fileName;
            if (!fileName.StartsWith("-"))
            {
                rconfig.save("gui.lastfile", fileName);
                if(fileName.Contains(" "))
                    quotedFileName = "\"" + fileName + "\"";
            }
            commandline = args + quotedFileName;
            this.r2 = new R2Pipe(commandline, r2path);
            this.fileName = fileName;
            this.r2html = new r2html(this);
        }
        private void MenuItemClick_CallbackHandler(object sender, EventArgs e)
        {
            System.Windows.Forms.ToolStripItem item = ((System.Windows.Forms.ToolStripItem)(sender));
            object [] args = (object []) item.Tag;
            ((Action<string>)args[0])((String)args[1]);            
        }
        private void ListView1ItemClickHandler(object sender, EventArgs e)
        {
            System.Windows.Forms.ToolStripItem item = ((System.Windows.Forms.ToolStripItem)(sender));
            string address = (string)item.Tag;
            gotoAddress(address);
            tabcontrol.SelectedIndex = 0; // select disasm
            //string name = item.Text;
            //GuiControl gc = gui_controls.findControlBy_cmds(cmds);
            //if (gc != null) name = gc.name;
            //run(cmds, name, false, null, null, false, false, gc);            
        }
        private void MenuItemClickHandler(object sender, EventArgs e)
        {
            System.Windows.Forms.ToolStripItem item = ((System.Windows.Forms.ToolStripItem)(sender));
            string name = item.Text;
            string cmds = item.Tag.ToString();
            GuiControl gc = gui_controls.findControlBy_cmds(cmds);
            if (gc != null) name = gc.name;
            run(cmds, name, false, null, null, false, false, gc);
        }
        public DialogResult Show(string text, string caption)
        {
            if (controls.ContainsKey("output"))
                setText("output", "", string.Format("{0} {1}", caption, text), true);
            return MessageBox.Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void update_statusbar(string cmds) // called from run
        {
            try // may fail on timeouts
            {
                seek_address = UInt64.Parse(r2.RunCommand("? $$~[1]")); // get seek address in decimal ?v
                string message = string.Format("{0} {1} {2} [0x{3}] > {4}",
                        guicontrol.fileType, guicontrol.arch, Path.GetFileName(fileName), seek_address, cmds);
                message = message.TrimStart(' ');
                this.guicontrol.show_message(message);

            }
            catch (Exception){} // better manage
        }
        public void refresh_control(string controlName)
        {
            GuiControl gc = gui_controls.findControlBy_name(controlName);
            if (gc == null) return;
            run(gc.cmds, controlName, false, null, null, false, true, gc);
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
        public void show_processes(string filter=null)
        {
            run("dpj", "processes_listView", false, new List<string> { "path", "status", "pid" }, filter);
        }
        public string find_dataPath(string def="")
        {
            string datapath = rconfig.load<string>("gui.datapath", def);
            if (datapath == null)
                datapath = def;
            if (!Directory.Exists(datapath))
            {
                datapath = guicontrol.Prompt("gui media path?", "Please, locate your data path...", def);
            }
            rconfig.save("gui.datapath", datapath);
            rconfig.save("gui.hexview.css", "r2pipe.css");
            rconfig.save("gui.theme_name", guicontrol.themeName);
            return datapath;
        }
        public string findControlBy_tabTitle(string title)
        {
            return gui_controls.findControlBy_tabTitle(title).ToString();
        }
        private Func<string> findDecorator_callback(string decoratorName)
        {
            return decorators_cb[decoratorName];
        }
        public ToolStripMenuItem clean_contextmenucmd(string menuName, ContextMenuStrip menu)
        {
            foreach (object obj in menu.Items)
            {
                ToolStripMenuItem item = null;
                if (obj.GetType() == typeof(ToolStripSeparator)) continue;
                item = (ToolStripMenuItem)obj;
                if (item.Text.Equals(menuName) && item.HasDropDownItems)
                {
                    item.DropDownItems.Clear();
                    return null;
                }
            }
            return null;
        }
        public ToolStripMenuItem enable_contextmenucmd(string menuName, ContextMenuStrip menu, bool enabled = false)
        {
            foreach (object obj in menu.Items)
            {
                ToolStripMenuItem item = null;
                if (obj.GetType() == typeof(ToolStripSeparator)) continue;
                item = (ToolStripMenuItem)obj;
                if (item.Text.Equals(menuName))
                {
                    item.Enabled = enabled;
                    return null;
                }
            }
            return null;
        }
        public ToolStripMenuItem find_contextmenucmd(string menuName, ContextMenuStrip menu)
        {
            foreach (object obj in menu.Items)
            {
                ToolStripMenuItem item = null;
                if (obj.GetType() == typeof(ToolStripSeparator)) continue;
                item = (ToolStripMenuItem)obj;
                if (item.Text.Equals(menuName))
                {
                    return item;
                }
                if (item.HasDropDownItems)
                {
                    foreach (object subitem in item.DropDownItems)
                    {
                        if (subitem.GetType() == typeof(ToolStripMenuItem))
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
                        if (subitem.GetType() == typeof(ToolStripMenuItem))
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
        public Color get_color_address(string colorLocation, string addr_type, Color def)
        {            
            return theme_manager.get_color_address(colorLocation, addr_type, def);
        }
        public Color theme_background()
        {
            Color backColor = theme_manager.get_current_background();
            return backColor;
        }
        public string get_themeName()
        {
            return theme_manager.themeName;
        }
        public string escape_json(string r2_json)
        {
            return Regex.Replace(r2_json,
                @"(?<!\\)  # lookbehind: Check that previous character isn't a \
                \\         # match a \
                (?!\\)     # lookahead: Check that the following character isn't a \",
                @"\\", RegexOptions.IgnorePatternWhitespace);
        }
        public string get_timestamp()
        {
            return DateTime.Now.Millisecond.ToString();
        }
        public void output(string text)
        {
            setText("output", "", text + "\n", true);
        }
        public void run_script(string scriptFileName)
        {
            // 1. read input from scriptFilename
            // 2. parse fields: <controlName[,bAppend,['col1','col2',...]> <r2 commands>            
            if (debugMode)
            {
                run("dcu entry0", "output", true);
            }
            run("Ps default"); // defaul project
            run("aa");
            run("pxa 4000",         "hexview");
            run("aaa",              "output", true);
            run("e scr.rows        = 100", "output", true);
            run("e scr.columns     = 80", "output", true);
            run("e scr.interactive = false", "output", true);
            run("e scr.utf8        = true", "output", true);
            run("e asm.emu         = false", "output", true);
            run("e asm.emustr      = true", "output", true);
            run("e asm.section     = true", "output", true);
            run("e asm.section.col = 14", "output", true);            
            run("e asm.tabs        = 6", "output", true);
            run("e asm.cmtcol      = 50", "output", true);
            run("e asm.flgoff      = true", "output", true);
            run("e asm.linesright  = true", "output", true);
            run("e asm.lineswidth  = 4", "output", true);
            run("e asm.marks       = false", "output", true);
            run("e asm.bytes       = false", "output", true);
            run("e anal.autoname   = false", "output", true);
            run("e io.cache        = true", "output", true); // needed for esil writes
            run("e cfg.editor      = c:\\windows\\notepad.exe", "output", true);
            if (!fileName.Equals("-"))
            {
                run("aflj", "functions_listview", false, new List<string> { "type", "offset", "name", "size", "cc", "nargs", "nlocals", "datarefs" });
                run("pd 256", "dissasembly"); // pd or pdf?
                run("izzj", "strings_listview", false, new List<string> { "section", "string", "vaddr", "type" });
                run("iij", "imports_listview", false, new List<string> { "ordinal", "name", "plt", "type" });
                run("Sj", "sections_listview", false, new List<string> { "name", "size", "vsize", "flags", "paddr", "vaddr" });
                run("dpj", "processes_listView", false, new List<string> { "path", "status", "pid" });
                popup_cmds_async("Call graph", "agf", false);
                if (debugMode) //fileName.StartsWith("-d "))
                {
                    popup_cmds_async("Maps", "dmj", true);
                    popup_cmds_async("regs", "drj", true);
                }
                guicontrol.refresh_popups();
            }
        }
        public void popup_cmds_async(string title, string cmds, bool popup = true)
        {
            try
            {
                guicontrol.Invoke(new popup_cmds_invoke(popup_cmds_send), new object[] { title, cmds, popup });
            }
            catch (Exception) { }// may fail on close gui
        }
        public delegate void popup_cmds_invoke(string title, string cmds, bool popup = true);
        public void popup_cmds_send(string title, string cmds, bool popup = true)
        {
            guicontrol.popup_cmds(title, cmds, popup);
        }
        public void exit()
        {
            try
            {
                rconfig.save("gui.current_shell", "radare2");
                Cursor.Current = Cursors.WaitCursor;                
                this.r2.RunCommand("q"); // may fail if "radare2.exe" process "not found"
                Cursor.Current = Cursors.Default;                
            }
            catch (Exception) { };
            this.r2 = null;
        }
    }
}