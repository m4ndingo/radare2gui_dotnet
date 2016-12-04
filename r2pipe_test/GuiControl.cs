using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace r2pipe_test
{
    public class GuiControl
    {
        public object control   = null;
        public string name      = null;
        public string sName     = null;
        public string cmds      = null;
        public string tabTitle  = null;
        public bool synchronize = true;
        public List<string> column_titles = null;
        public GuiControl(object control, string name, string sName, string cmds, string tabTitle, List<string> column_titles = null)
        {
            this.control    = control;
            this.name       = name;
            this.sName      = sName;
            this.cmds       = cmds;
            this.tabTitle   = tabTitle;
            this.column_titles = column_titles;
        }
        public void set_columnTitles(List<string> column_titles)
        {
            this.column_titles = column_titles;
        }        
        public override string ToString()
        {
            return string.Format(
                "control  : {0}\n" +
                "name     : {1}\n" +
                "cmds     : {2}\n" +
                "tabTitle : {3}\n" +
                "column_titles : {4}",
                control.ToString(), // access problems
                name, cmds, tabTitle,
                column_titles.ToString());
        }
    }
    public class GuiControls
    {
        private R2PIPE_WRAPPER r2pw       = null;
        public  List<GuiControl> controls = null;
        public GuiControls(R2PIPE_WRAPPER r2pw)
        {
            this.r2pw = r2pw;
            controls = new List<GuiControl>();
        }
        public GuiControl add_control(string name, object control, string tabTitle = null, string cmds = null)
        {
            GuiControl gui_control = null;
            try
            {
                string sName = name;
                if( sName.Contains("_") )
                {
                    int pos = sName.IndexOf("_");
                    sName = sName.Substring(0, pos);
                }
                gui_control = new GuiControl(control, name, sName, cmds, tabTitle);
                controls.Add(gui_control);
            }
            catch (Exception e)
            {
                r2pw.Show(e.ToString(), "GuiControls: add_control()");
            }
            return gui_control;
        }
        public void dump(string dummy=null)
        {
            foreach (GuiControl c in controls)
            {
                r2pw.output(string.Format("{0,12}|{1,12}|{2,20}|{3}",
                    c.tabTitle, c.cmds, c.name, c.control.ToString()));
            }
        }
        public GuiControl findControlBy_name(string name)
        {
            foreach (GuiControl c in controls)
            {
                if (name.Equals(c.name))
                    return c;
            }
            return null;
        }
        public GuiControl findControlBy_cmds(string cmds)
        {
            foreach (GuiControl c in controls)
            {
                if (cmds.Equals(c.cmds))
                    return c;
            }
            return null;
        }
        public GuiControl findControlBy_tabTitle(string tabTitle)
        {
            if (tabTitle == null) return null;
            foreach (GuiControl c in controls)
            {
                if (tabTitle.Equals(c.tabTitle)) 
                    return c;
            }
            return null;
        }
        public void remove_control_byName(string controlName)
        {
            GuiControl gc = findControlBy_name(controlName);
            controls.Remove(gc);
        }
        public GuiControl get_active_control()
        {
            string tabTitle = r2pw.guicontrol.selected_tab("title");
            GuiControl gui_control = findControlBy_tabTitle(tabTitle);
            return gui_control;
        }
    }
}
