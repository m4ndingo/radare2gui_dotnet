using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace r2pipe_test
{
    /*
    public class decoratorParams
    {
        public List<decoratorParam> decoratorParamsList;
        public themeManager tm;
        public decoratorParams(themeManager tm)
        {
            this.decoratorParamsList = new List<decoratorParam>();
            this.tm = tm;
        }
        public void add_decoratorParam(string controlName, string columName, string value, string decoratorName, object json_row, List<string> cols, ListViewItem listviewItem, R2PIPE_WRAPPER rconfig)
        {
            decoratorParam dp = new decoratorParam(controlName, columName, value, decoratorName, json_row, cols, listviewItem);
            decoratorParamsList.Add(dp);
        }
    }*/
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
