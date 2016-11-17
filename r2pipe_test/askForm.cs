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
        public string Prompt(string text, string caption, askForm o)
        {
            askLabel.Text = text;
            Text = caption;
            o.ShowDialog();
            return answer;
        }
        private void txtAnswer_TextChanged(object sender, System.EventArgs e)
        {
            answer = txtAnswer.Text;
        }
    }
}
