using System.Windows.Forms;

namespace r2pipe_test
{
    public partial class askForm : Form
    {
        public string answer = null;
        public askForm()
        {
            InitializeComponent();
        }
        public string Prompt(string text, string caption, string defval=null,askForm o=null)
        {
            askLabel.Text = text;
            Text = caption;
            if (defval != null) txtAnswer.Text = defval;
            o.ShowDialog();
            return answer;
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
    }
}
