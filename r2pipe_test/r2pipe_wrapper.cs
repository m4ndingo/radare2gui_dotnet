using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using Newtonsoft.Json;
using r2pipe;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace r2pipe_test
{
    public class R2PIPE_WRAPPER
    {
        private IR2Pipe r2          = null;
        public String fileName      = "";
        public RConfig rconfig      = null;
        r2html r2html               = null;
        private bool mouseMoved     = false;
        private string lastAddress  = null;
        TabControl tabcontrol       = null;
        public Dictionary<string, object> controls;
        public R2PIPE_WRAPPER(RConfig rconfig, Form1 frm)
        {
            this.controls = new Dictionary<string, object>();
            this.rconfig  = rconfig;
            this.tabcontrol = ((Form1)frm).tabcontrol;
            new Hotkeys();
        }
        public string run(String cmds, String controlName=null, Boolean append = false, List<string> cols = null)
        {
            string res = "";
            dynamic json_obj = null;
            if (controls.ContainsKey("output"))
            {
                string control_type = "unknown";
                if(controlName!=null && controls.ContainsKey(controlName))
                    control_type = controls[controlName].GetType().ToString();
                setText("output", string.Format("r2.RunCommand(\"{1}\"): target='{0}' type='{2}' cols='{3}'\n", controlName, cmds, control_type, cols != null ? string.Join(", ", cols) : ""), true);
            }
            if (r2 == null)
            {
                Show(string.Format("{0}\nR2PIPE_WRAPPER: run(): {1}: IR2Pipe is null", cmds, controlName), "Wops!");
                return null;
            }
            if (controlName!=null && !controls.ContainsKey(controlName))
            {
                add_tab(controlName, cmds);
            }
            if (controlName!=null && !controls.ContainsKey(controlName))
            {
                Show(string.Format("{0}\ncontrols: control '{1}' not found...", cmds, controlName), "Wops!");
                return null;
            }
            res = r2.RunCommand(cmds).Replace("\r", "");
            if(res.StartsWith("[") || res.StartsWith("{"))
            try
            {
                json_obj = JsonConvert.DeserializeObject(res);
            }
            catch (Exception){}
            if(controlName!=null) setText(controlName, res, append, json_obj, cols);
            return res;
        }
        delegate void SetTextCallback(string controlName, string someText, bool append = false, dynamic json_obj = null, List<string> cols = null);
        private void setText(string controlName, string someText, bool append = false, dynamic json_obj = null, List<string> cols = null)
        {
            object c = controls[controlName];
            if (c.GetType() == typeof(RichTextBox))
            {
                RichTextBox rtbox = (RichTextBox)c;
                if (rtbox.InvokeRequired)
                {
                    SetTextCallback d = new SetTextCallback(setText);
                    rtbox.Invoke(d, new object[] { controlName, someText, append, json_obj, cols });
                }
                else
                {
                    if (!append) rtbox.Text = "";
                    rtbox.Text += someText;
                }
            }
            else if (c.GetType() == typeof(ListView))
            {
                if (json_obj != null)
                {
                    ListView lstview = (ListView)c;
                    lstview.Invoke(new BeginListviewUpdate(listviewUpdate), new object[] { lstview, true, cols });
                    for (int i = 0; i < json_obj.Count; i++)
                    {
                        string col0 = json_obj[i][cols[0]];
                        ListViewItem row_item = new ListViewItem(col0);
                        for (int j = 1; j < cols.Count; j++)
                        {
                            string cname = cols[j];
                            if (json_obj[i][cname] != null)
                            {
                                string value = json_obj[i][cname].ToString();
                                row_item.SubItems.Add(value);
                            }
                        }
                        lstview.Invoke(new AddToListviewCallback(listviewAdd), new object[] { lstview, row_item });
                    }
                    lstview.Invoke(new BeginListviewUpdate(listviewUpdate), new object[] { lstview, false, null });
                }
                else
                {
                    Console.WriteLine(string.Format("setText: controlName='{0}' type='{1}' no json results received?", controlName, c.GetType()));
                }
            }
            else if (c.GetType() == typeof(WebBrowser))
            {
                string url = BuildWebPage((WebBrowser)c, controlName, someText);
                ((WebBrowser)c).DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.webBrowser_DocumentCompleted);
                ((WebBrowser)c).Navigate(url);
            }
            else
            {
                Show(string.Format("setText: controlName='{0}' Unknown control:{1}", controlName, c.GetType()), "unknown control type");
            }
        }
        private string Prompt(string text, string caption)
        {
            askForm frm = new askForm();
            string answer = frm.Prompt(text, caption, frm);
            return answer;
        }
        private string BuildWebPage(WebBrowser wBrowser, string controlName, string someText)
        {
            string tmpName = string.Format("{0}{1}.html", rconfig.tempPath, controlName);
            using (StreamWriter sw = new StreamWriter(tmpName))
            {
                sw.WriteLine(r2html.convert(someText));
            }
            return tmpName;                
        }
        private void webBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            ((WebBrowser)sender).Document.Body.MouseUp += new HtmlElementEventHandler(webBrowser_MouseUp);
            ((WebBrowser)sender).Document.Body.MouseDown += new HtmlElementEventHandler(webBrowser_MouseDown);
            ((WebBrowser)sender).Document.Body.MouseMove += new HtmlElementEventHandler(webBrowser_MouseMove);
        }
        void webBrowser_MouseDown(Object sender, HtmlElementEventArgs e)
        {
            mouseMoved = false;
        }
        void webBrowser_MouseUp(Object sender, HtmlElementEventArgs e)
        {
            HtmlElement browser = (HtmlElement)sender;
            switch (e.MouseButtonsPressed)
            {
                case MouseButtons.Left:
                    HtmlElement element = browser.Document.GetElementFromPoint(e.ClientMousePosition);
                    string text = null;
                    string tagname = element.TagName;
                    if (element.OuterText != null)
                    {
                        text = element.OuterText.Replace(" ", "");
                        if (mouseMoved == false && tagname.Equals("SPAN"))
                            gotoAddress(text);
                    }
                    break;
            }
            browser.Focus();
        }
        void webBrowser_MouseMove(Object sender, HtmlElementEventArgs e)
        {
            mouseMoved = true;
        }
        public void gotoAddress(string address)
        {
            if (address!=null && address.Length>0 && address != lastAddress)
            {
                run("pd 100 @" + address, "dissasembly");
                run("px 2000 @" + address, "hexview");
                lastAddress = address;
            }            
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
                    int col_width = lstview.Width / cols.Count;
                    lstview.Columns.Clear();
                    foreach (string cname in cols)
                    {
                        lstview.Columns.Add(cname);
                        lstview.Columns[i++].Width = col_width;
                    }
                    lstview.Columns[cols.Count - 1].Width = lstview.Width * 3 / 4; ;
                }
            }
            else lstview.EndUpdate();

        }
        public void listviewAdd(ListView lstview, ListViewItem item)
        {
            lstview.Items.Add(item);
        }
        public void add_control(string name, object control)
        {
            this.controls.Add(name, control);
            if (control.GetType() == typeof(WebBrowser))
            {
                ((WebBrowser)control).PreviewKeyDown -= new PreviewKeyDownEventHandler(webBrowser_PreviewKeyDown);
                ((WebBrowser)control).PreviewKeyDown += new PreviewKeyDownEventHandler(webBrowser_PreviewKeyDown);
                ((WebBrowser)control).WebBrowserShortcutsEnabled = true;
                ((WebBrowser)control).Refresh();
            }
        }
        private void webBrowser_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.G) //71 g keyvalue
            {
                string address = Prompt("Address:", "goto address");
                gotoAddress(address);
            }
        }
        public void open(String fileName)
        {
            this.r2 = new R2Pipe(fileName, rconfig.r2path);
            this.fileName = fileName;
            this.r2html = new r2html(this);
        }
        public void add_menucmd(string menuName, string text, string cmds, MenuStrip menu)
        {
            foreach (ToolStripMenuItem item in menu.Items)
            {                
                if (item.Text.Equals(menuName))
                {
                    ToolStripItem newitem=item.DropDownItems.Add(string.Format("{0} ( {1} )",text,cmds));
                    newitem.Tag = cmds;
                    newitem.Click += new EventHandler(MenuItemClickHandler);
                }
            }
        }
        private void MenuItemClickHandler(object sender, EventArgs e)
        {
            System.Windows.Forms.ToolStripItem item = ((System.Windows.Forms.ToolStripItem)(sender));
            string cmds = item.Tag.ToString();
            run(cmds, item.Text);
        }
        public void add_tab(string tabname, string cmds)
        {
            var page = new TabPage(tabname);
            var browser = new WebBrowser();
            browser.Dock = DockStyle.Fill;
            page.Controls.Add(browser);
            tabcontrol.TabPages.Add(page);
            browser.Navigate("about:"+cmds);
            page.Select();
            add_control(tabname, browser);
            tabcontrol.SelectedTab = page;
        }
        public DialogResult Show(string text, string caption)
        {
            if (controls.ContainsKey("output"))
                setText("output", string.Format("{0} {1}", caption, text), true);
            return MessageBox.Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        public void exit()
        {
            try
            {
                this.r2.RunCommand("q");
            }
            catch (Exception) { };
        }
    }
}