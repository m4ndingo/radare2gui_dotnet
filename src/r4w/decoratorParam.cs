using System.Collections.Generic;
using System.Windows.Forms;

namespace r2pipe_test
{
    public class decoratorParam
    {
        public string controlName;
        public string columnName;
        public string value;
        public string decoratorName;
        public object json_row;
        public List<string> cols;
        public ListViewItem listviewItem;
        R2PIPE_WRAPPER r2wp;
        public decoratorParam(string controlName, string columName, string value, string decoratorName, object json_row, List<string> cols, ListViewItem listviewItem, R2PIPE_WRAPPER r2wp)
        {
            this.controlName = controlName;
            this.columnName = columName;
            this.value = value;
            this.decoratorName = decoratorName;
            this.json_row = json_row;
            this.cols = cols;
            this.listviewItem = listviewItem;
            this.r2wp= r2wp;
        }
    }
}
