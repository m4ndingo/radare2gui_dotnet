using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace r2pipe_test
{
    public class GuiControl
    {
        public object control  = null;
        public string name = null;
        public string cmds = null;
        public string tabTitle = null;
        public List<string> column_titles = null;
        public GuiControl(object control, string name, string cmds, string tabTitle, List<string> column_titles = null)
        {
            this.control    = control;
            this.name       = name;
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
                control.ToString(),
                name, cmds, tabTitle,
                column_titles);
        }
    }
    public class GuiControls
    {
        private R2PIPE_WRAPPER r2pw       = null;
        private List<GuiControl> controls = null;
        public GuiControls(R2PIPE_WRAPPER r2pw)
        {
            this.r2pw = r2pw;
            controls = new List<GuiControl>();
        }
        public void add_control(string name, object control, string tabTitle = null, string cmds = null)
        {
            GuiControl gui_control;
            try
            {
                gui_control = new GuiControl(control, name, cmds, tabTitle);
                controls.Add(gui_control);
            }
            catch (Exception e)
            {
                r2pw.Show(e.ToString(), "GuiControls: add_control()");
            }
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
            r2pw.output(string.Format("findControlByName: name='{0}'", name));
            foreach (GuiControl c in controls)
            {
                if (name.Equals(c.name))
                    return c;
            }
            return null;
        }
        public GuiControl findControlBy_tabTitle(string tabTitle)
        {
            r2pw.output(string.Format("findControlByTitle: tabTitle='{0}'", tabTitle));
            foreach (GuiControl c in controls)
            {
                if (tabTitle.Equals(c.tabTitle)) 
                    return c;
            }
            return null;
        }
    }
}
