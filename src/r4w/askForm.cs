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
            top_orig        = Height*3;
            askLabel.Text   = text;
            Text            = caption;
            txtHeader.Text  = caption;
            if (defval != null) txtAnswer.Text = defval;
            o.ShowDialog();
            answer = txtAnswer.Text;
            if( answer!=null )
                answer = answer.Replace("\n", "").Replace("\r", "");
            return answer;
        }
        private void recenter_form()
        {           
            //CenterToParent();
            CenterToScreen();
        }
        private void resize_controls()
        {
            if (txtAnswer.TextLength > 20)
                this.Width = ( width_orig * 30 ) / 20;
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
        private void askForm_VisibleChanged(object sender, System.EventArgs e)
        {
            recenter_form();
        }
        private void btnCancel_Click(object sender, System.EventArgs e)
        {
            answer = null;
        }

        private void pasteClipboard()
        {
            txtAnswer.Text = Clipboard.GetText();
            txtAnswer.Focus();
            if (txtAnswer.TextLength > 0)
            {
                txtAnswer.SelectionStart = txtAnswer.TextLength;
                txtAnswer.SelectionLength = 0;
            }
        }

        private void askForm_Load(object sender, EventArgs e)
        {
            recenter_form();
            System.Windows.Forms.ToolTip ToolTip1 = new System.Windows.Forms.ToolTip();
            ToolTip1.SetToolTip(btnPaste, "Paste clipboard text");
            ToolTip1.SetToolTip(btnOpenfile, "Browse for file ...");            
        }
        private void btnPaste_Click_1(object sender, EventArgs e)
        {
            pasteClipboard();
        }
        private void btnOpenfile_Click_1(object sender, EventArgs e)
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
    }
}
