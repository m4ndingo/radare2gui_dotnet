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
            string bg_color = "#fff";
            string background = string.Format(
                @"{0}/../../media/sf2_original_low_bright.jpg", r2pw.rconfig.guiPath);
            background = background.Replace(@"\", "/");
            background = ""; //use config
            //r2pipe css temporary hardcoded
            WriteTmpFile("r2pipe.css",
                "body{background:"+bg_color+" url('file:///"
                    + background +
                "');color:#000080;font-weight:bold;background-repeat: repeat;"+
                "background-attachment: fixed;}\r\n" +
                ".r2code{font-family:Fixedsys;font-size:12px;cursor:arrow;font-weight:bold;}\r\n" +
                ".comment{color:green;}\r\n"+
                ".address, .shorted_address{color:black;}\r\n" +
                ".address:hover{text-decoration:underline;}\r\n" +
                ".shorted_address:hover{text-decoration:underline;background:#ee0}\r\n" +
                ".number{color:green;}\r\n" +
                ".hexb{color:blue;}\r\n"
                );
            WriteTmpFile("r2pipe_high.css",
                "body{background:#000 url('file:///"
                    + background +
                "');color:#ff0;font-weight:bold;background-repeat: repeat;" +
                "background-attachment: fixed;}\r\n" +
                ".r2code_s{font-family:Fixedsys;color:black;position:absolute;left:2px;top:2px;font-weight:bold;}\r\n" +
                ".r2code{font-family:Fixedsys;position:absolute;left:0;top:0;padding:0;cursor:arrow;}\r\n" +
                ".comment{color:#0f0;}\r\n" +
                ".address, .shorted_address{color:#fff;}\r\n" +
                ".address:hover{text-decoration:underline;}\r\n" +
                ".number{color:#0f0;}\r\n" +
                ".hexb{color:#aaf;}\r\n"
                );
        }
        public string convert(string console_text)
        {
            string html = "";
            string console_text_cut = "";
            string console_text_cut_copy = "";
            string css_filename = "r2pipe.css";
            int maxlen_line = 100;
            foreach (string line in console_text.Split('\n'))
            {
                console_text_cut += line.Substring(0, line.Length < maxlen_line ? line.Length : maxlen_line) + "\n";
            }
            console_text_cut_copy = console_text_cut;

            console_text_cut = (new Regex(@"(;.+)")).Replace(console_text_cut, "<span class=comment>$1</span>");
            console_text_cut = (new Regex(@"(- offset -.+)")).Replace(console_text_cut, "<span class=comment>$1</span>");
            console_text_cut = (new Regex(@"(0x[0-9a-f]{2})([\s\]])", RegexOptions.IgnoreCase)).Replace(console_text_cut, "<span class=number>$1</span>$2");
            console_text_cut = (new Regex(@"([-\+]\s)([0-9]{1,})", RegexOptions.IgnoreCase)).Replace(console_text_cut, "$1<span class=number>$2</span>");
            console_text_cut = (new Regex(@"(0x[0-9a-f]{2,}\s+)([0-9a-f]{2,})", RegexOptions.IgnoreCase)).Replace(console_text_cut, "$1<span class=hexb>$2</span>");
            console_text_cut = (new Regex(@"([\[\s])(0x[0-9a-f]{4,})([\]\s])", RegexOptions.IgnoreCase)).Replace(console_text_cut, "$1<span class=address>$2</span>$3");
            console_text_cut = (new Regex(@"\[(sym.imp.KERNEL32.dll_(GetStartupInfoA))\]", RegexOptions.IgnoreCase)).Replace(console_text_cut, "[<span class=shorted_address title='$1'>$2</span>]");

            //webpage temporary hardcoded
            html =  "<!DOCTYPE html>\n";
            html += "<meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge,chrome=1\">\n";
            html += "<html>\r\n";
            html += "<link href='" + css_filename + "' rel='stylesheet'>\r\n";
            html += "<body>\r\n";
            //html += "<div class=r2code_s><pre>"         + console_text_cut_copy + "</pre></div>";
            html += "<div class=r2code id=r2code><pre>" + console_text_cut      + "</pre></div>";
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
