using System;
using System.Windows.Forms;

namespace r2pipe_test
{
    public partial class askForm : Form
    {
        public string answer = null;
        private Form owner = null;
        private int width_orig = 123;
        private int top_orig = 123;
        public askForm()
        {
            InitializeComponent();
        }
        public string Prompt(string text, string caption, string defval=null,askForm o=null, Form owner=null)
        {
            this.owner      = owner;
            width_orig      = Width;
            top_orig        = Height*2;
            askLabel.Text   = text;
            Text            = caption;
            txtHeader.Text  = caption;
            if (defval != null) txtAnswer.Text = defval;
            resize_controls();
            o.ShowDialog();
            if( answer!=null )
                answer = answer.Replace("\n", "").Replace("\r", "");
            return answer;
        }
        private void recenter_form()
        {           
            int new_top = (top_orig * 16) / 32;
            if (owner != null)
                new_top += owner.Top;
            CenterToParent();
            this.Top = new_top;
        }
        private void resize_controls()
        {
            if (txtAnswer.TextLength > 20)
                this.Width = width_orig * 2;
            recenter_form();
        }
        private void txtAnswer_TextChanged(object sender, System.EventArgs e)
        {
            answer = txtAnswer.Text;
        }
        private void txtAnswer_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape ||
                e.KeyCode == Keys.Enter) this.Close();
        }
        private void btnOpenfile_Click(object sender, System.EventArgs e)
        {
            openFileDialog1.Title = txtHeader.Text;
            openFileDialog1.FileName = txtAnswer.Text;
            try
            {
                openFileDialog1.ShowDialog();
                if (openFileDialog1.FileName.Length > 0)
                    txtAnswer.Text = openFileDialog1.FileName;
            }
            catch (Exception) { } // may fail
            resize_controls();
         }
        private void askForm_VisibleChanged(object sender, System.EventArgs e)
        {
            recenter_form();
        }
        private void btnCancel_Click(object sender, System.EventArgs e)
        {
            answer = null;
        }
    }
}
