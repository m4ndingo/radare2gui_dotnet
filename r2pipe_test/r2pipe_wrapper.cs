using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using Newtonsoft.Json;
using r2pipe;


namespace r2pipe_test
{
    public class R2PIPE_WRAPPER
    {
        private IR2Pipe r2 = null;
        public String fileName = "";
        public RConfig rconfig = null;
        Dictionary<string, object> controls;
        r2html r2html = null;
        private bool mouseMoved   = false;
        private bool skipNextKey = true;
        private string lastAddress = null;
        public R2PIPE_WRAPPER(RConfig rconfig)
        {
            this.controls = new Dictionary<string, object>();
            this.rconfig  = rconfig;
        }
        public string run(String cmds, String controlName, Boolean append = false, List<string> cols = null)
        {
            string res = "";
            dynamic json_obj = null;
            if (r2==null)
            {
                MessageBox.Show(string.Format("Wops!\n{0}\nIR2Pipe is null", controlName), "run", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }
            if (!controls.ContainsKey(controlName))
            {
                MessageBox.Show(string.Format("Wops!\n{0}\ncontrol not found...", controlName), "run", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }
            if (controls.ContainsKey("output"))
                setText("output", string.Format("r2.RunCommand(\"{1}\"): target='{0}' type='{2}' cols='{3}'\n", controlName, cmds, controls[controlName].GetType(), cols != null ? string.Join(", ", cols) : ""), true);
            res = r2.RunCommand(cmds).Replace("\r", "");
            try
            {
                json_obj = JsonConvert.DeserializeObject(res);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            setText(controlName, res, append, json_obj, cols);
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
                string url=BuildWebPage((WebBrowser)c, controlName, someText);
                ((WebBrowser)c).DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.webBrowser_DocumentCompleted);
                ((WebBrowser)c).Navigate(url);
                ((WebBrowser)c).PreviewKeyDown -= new PreviewKeyDownEventHandler(webBrowser_PreviewKeyDown);
                ((WebBrowser)c).PreviewKeyDown += new PreviewKeyDownEventHandler(webBrowser_PreviewKeyDown);
            }
            else
            {
                MessageBox.Show(string.Format("setText: controlName='{0}' Unknown control:{1}", controlName, c.GetType()));
            }
        }
        private void webBrowser_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (skipNextKey)
            {
                skipNextKey = false;
                return;
            }
            if (e.KeyValue == 71) //g key
            {
                askForm frm = new askForm();
                frm.ShowDialog();
                skipNextKey = true;
            }
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
                    text = element.OuterText.Replace(" ", "");

                    if (mouseMoved == false && tagname.Equals("SPAN"))
                        gotoAddress(text);
                    break;
            }
        }
        void webBrowser_MouseMove(Object sender, HtmlElementEventArgs e)
        {
            mouseMoved = true;
        }
        public void gotoAddress(string address)
        {
            if (address != lastAddress)
            {
                run("pd 100 @" + address, "dissasembly");
                run("px 2000 @" + address, "hexview");
            }
            lastAddress = address;
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
                        lstview.Columns[i].Width = col_width;
                    }
                }
            }
            else lstview.EndUpdate();

        }
        public void listviewAdd(ListView lstview, ListViewItem item)
        {
            lstview.BeginUpdate();
            lstview.Items.Add(item);
            lstview.EndUpdate();
        }
        public void add_control(string name, object control)
        {
            this.controls.Add(name, control);

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
            run(cmds, "output", true);
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