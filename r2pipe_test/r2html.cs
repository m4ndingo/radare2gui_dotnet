using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace r2pipe_test
{
    public class r2html
    {
        R2PIPE_WRAPPER r2pw = null;
        public r2html(R2PIPE_WRAPPER r2pw)
        {
            this.r2pw = r2pw;
            WriteTmpFile("r2pipe.css",
                "body{background:#fff;color:#000080;font-weight:bold;}\r\n" +
                ".r2code{font-family:Fixedsys;}\r\n" +
                ".comment{color:green;}\r\n"+
                ".address{color:black;}\r\n"+
                ".number{color:green;}\r\n"+
                ".hexb{color:blue;}\r\n"
                );
        }
        public string convert(string sometext)
        {
            string html = "";
            sometext = (new Regex(@"(;.+)")).Replace(sometext, "<span class=comment>$1</span>");
            sometext = (new Regex(@"(- offset -.+)")).Replace(sometext, "<span class=comment>$1</span>");
            sometext = (new Regex(@"(0x[0-9a-f]{2})([\s\]])", RegexOptions.IgnoreCase)).Replace(sometext, "<span class=number>$1</span>$2");
            sometext = (new Regex(@"([-\+]\s)([0-9]{1,})", RegexOptions.IgnoreCase)).Replace(sometext, "$1<span class=number>$2</span>");
            sometext = (new Regex(@"(0x[0-9a-f]{2,}\s+)([0-9a-f]{2,})", RegexOptions.IgnoreCase)).Replace(sometext, "$1<span class=hexb>$2</span>");
            sometext = (new Regex(@"([\[\s]0x[0-9a-f\s]{4,}[\]\s])", RegexOptions.IgnoreCase)).Replace(sometext, "<span class=address>$1</span>");

            html = "<html>\r\n";
            html +="<link href='r2pipe.css' rel='stylesheet'>\r\n";
            html += "<body>\r\n";
            html += "<div class=r2code><pre>"+sometext+"</pre></div>";
            return html;
        }
        private void WriteTmpFile(string fileName, string content)
        {
            string tmpName = string.Format("{0}{1}", r2pw.rconfig.tempPath, fileName);
            using (StreamWriter sw = new StreamWriter(tmpName))
            {
                sw.WriteLine(content);
            }
        }
    }
}
