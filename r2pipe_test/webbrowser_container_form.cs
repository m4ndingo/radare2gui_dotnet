using System.Windows.Forms;

namespace r2pipe_test
{
    public partial class webbrowser_container_form : Form
    {
        R2PIPE_WRAPPER r2pw = null;
        GuiControl control = null;
        //string frmName = null;
        public webbrowser_container_form(R2PIPE_WRAPPER r2pw, GuiControl control)
        {
            InitializeComponent();
            this.r2pw = r2pw;
            this.control = control;
            //this.frmName = control.tabTitle;
        }
        private void webbrowser_container_form_FormClosed(object sender, FormClosedEventArgs e)
        {
            r2pw.rconfig.save(
                string.Format("gui.layout.{0}", control.sName),
                string.Format("{0};{1};{2};{3}", Top, Left, Width, Height)
                );
            r2pw.unregister_control(control.name);
        }

        private void webbrowser_container_form_Load(object sender, System.EventArgs e)
        {
            string layout = r2pw.rconfig.load<string>(string.Format("gui.layout.{0}", control.sName));
            if (layout != null)
            {
                string[] fields = layout.Split(';');
                Top     = int.Parse(fields[0]);
                Left    = int.Parse(fields[1]);
                Width   = int.Parse(fields[2]);
                Height  = int.Parse(fields[3]);
            }
        }
    }
}
