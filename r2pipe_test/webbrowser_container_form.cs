using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace r2pipe_test
{
    public partial class webbrowser_container_form : Form
    {
        R2PIPE_WRAPPER r2pw = null;
        string frmName = null;
        public webbrowser_container_form(R2PIPE_WRAPPER r2pw, string frmName)
        {
            InitializeComponent();
            this.r2pw = r2pw;
            this.frmName = frmName;
        }
        private void webbrowser_container_form_FormClosed(object sender, FormClosedEventArgs e)
        {
            r2pw.unregister_control(frmName);
        }
    }
}
