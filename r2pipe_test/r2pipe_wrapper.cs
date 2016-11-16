using System;
using System.Windows.Forms;
using System.Collections.Generic;
using Newtonsoft.Json;
using r2pipe;

public class R2PIPE_WRAPPER
{
    private IR2Pipe r2 = null;
    public String fileName = "";
    private RichTextBox outputTxt = null;
    //Dictionary<string, RichTextBox> controls;
    Dictionary<string, object> controls;
    public R2PIPE_WRAPPER(RichTextBox control)
    {
        this.controls = new Dictionary<string, object>();
        outputTxt = control;
    }
    public string run(String cmds,String controlName,Boolean append=false,List<string> cols=null)
    {
        string res = "";
        dynamic json_obj = null;
        if (!controls.ContainsKey(controlName))
        {
            MessageBox.Show(string.Format("Wops!\n{0}\ncontrol not found...", controlName), "run", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return null;
        }
        if (controls.ContainsKey("output"))
            setText("output", string.Format("r2.RunCommand(\"{1}\"): target='{0}' type='{2}' cols='{3}'\n", controlName, cmds, controls[controlName].GetType(), cols!=null?string.Join(", ",cols):""), true);
        res=r2.RunCommand(cmds).Replace("\r", "");
        try
        {
            json_obj = JsonConvert.DeserializeObject(res);
        }
        catch(Exception e)
        {
            Console.WriteLine(e.ToString());
        }
        setText(controlName, res, append, json_obj, cols);
        return res;
    }
    delegate void SetTextCallback(string controlName, string someText, bool append = false, dynamic json_obj = null, List<string> cols=null);
    private void setText(string controlName, string someText, bool append=false, dynamic json_obj = null, List<string> cols=null)
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
        else
        { 
            Console.WriteLine(string.Format("setText: controlName='{0}' Unknown control:{1}", controlName, c.GetType()));
        }
    }
    public delegate void BeginListviewUpdate(ListView lstview, bool update, List<string> cols);
    public delegate void AddToListviewCallback(ListView lstview, ListViewItem item);
    public void listviewUpdate(ListView lstview,bool update = true, List<string> cols = null)
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
                foreach(string cname in cols)
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
        this.r2 = new R2Pipe(fileName, @"C:\Users\alberto.moro\Documents\radare2win\radare2.exe");
        this.fileName = fileName;
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
