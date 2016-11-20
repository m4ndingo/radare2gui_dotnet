using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace r2pipe_test
{
    public class r2html
    {
        R2PIPE_WRAPPER r2pw = null;
        public r2html(R2PIPE_WRAPPER r2pw)
        {
            this.r2pw = r2pw;
        }
        public string encodeutf8(string text)
        {
            byte[] bytes = Encoding.Default.GetBytes(text);
            return Encoding.UTF8.GetString(bytes);
        }
        public string convert(string cmds, string console_text, dynamic json_obj)
        {
            string html_body = "", html_header = "";
            string css_filename = "";
            string js_filename = "r2html.js";
            string themeName = "classic";

            if (r2pw.rconfig.dataPath == null)
            {
                string datapath = r2pw.Prompt("gui media path?", "Please, locate your data path...");
                r2pw.rconfig.save("gui.datapath", datapath);
                r2pw.rconfig.save("gui.theme_name", themeName);
                r2pw.rconfig.save("gui.hexview.css", "r2pipe.css");
            }
            themeName    = r2pw.rconfig.load<string>("gui.theme_name", themeName);
            js_filename  = r2pw.rconfig.load<string>("gui.datapath") + @"\..\scripts\" + js_filename;
            css_filename = 
                string.Format(
                    @"{0}\{1}",
                    r2pw.rconfig.load<string>("gui.datapath"),
                    r2pw.rconfig.load<string>("gui.hexview.css", "r2pipe.css")
                    );
            html_header = "<!DOCTYPE html>\n";
            html_header += "<meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge,chrome=1\">\n";
            html_header += "<meta charset=\"UTF-8\">\n";
            html_header += "<html>\r\n";
            html_header += "<link href='" + css_filename + "' rel='stylesheet'>\r\n";
            html_header += "<body>\r\n";
            html_header += "<script>var r2output = null;</script>\r\n";
            if (json_obj == null)
            {
                html_body = htmlize(console_text);
            }else
            {
                html_body = htmljsonize(cmds, json_obj);
            }
            html_body += "<script src='" + js_filename + "'></script>\r\n";
            return html_header + html_body;
        }
        private string htmljsonize(string cmds, dynamic json_obj)
        {
            string js_string = JsonConvert.SerializeObject(json_obj);
            string html_body = "";
            js_string = js_string.Replace(@"'", @"\'");
            html_body += "<script src=\"https://code.jquery.com/jquery-1.11.3.js\"></script>\r\n";
            html_body += "<script>\r\n";
            html_body += "var r2cmds   = '" + cmds + "';\r\n";
            html_body += "var r2output = jQuery.parseJSON('" + js_string + "')\r\n";
            html_body += "</script>\r\n";
            return html_body;
        }
        private string htmlize(string console_text)
        {
            int maxlen_line = 100;
            string html = "";
            string console_text_cut = "";
            string console_text_cut_copy = "";
            if (console_text == null) return null;
            foreach (string line in console_text.Split('\n'))
            {
                console_text_cut += line.Substring(0, line.Length < maxlen_line ? line.Length : maxlen_line) + "\n";
            }
            console_text_cut = encodeutf8(console_text_cut);
            console_text_cut_copy = console_text_cut;
            console_text_cut = (new Regex(@"(;.+)")).Replace(console_text_cut, 
                "<span class=comment>$1</span>");
            console_text_cut = (new Regex(@"(- offset -.+)")).Replace(console_text_cut, 
                "<span class=comment>$1</span>");
            console_text_cut = (new Regex(@"(0x[0-9a-f]{2})([\s\]])", RegexOptions.IgnoreCase)).Replace(console_text_cut, 
                "<span class=number>$1</span>$2");
            console_text_cut = (new Regex(@"(0x[0-9a-f]{2,}\s+)([0-9a-f]{2,})", RegexOptions.IgnoreCase)).Replace(console_text_cut, 
                "$1<span class=hexb>$2</span>");
            console_text_cut = (new Regex(@"([\[\s])(0x[0-9a-f]{4,})([\]\s])", RegexOptions.IgnoreCase)).Replace(console_text_cut, 
                "$1<span class=address>$2</span>$3");
            console_text_cut = (new Regex(@"([-\+]\s)([0-9]{1,})", RegexOptions.IgnoreCase)).Replace(console_text_cut,
                "$1<span class=number>$2</span>");
            console_text_cut = (new Regex(@"\[(sym.imp.KERNEL32.dll_(GetStartupInfoA))\]", RegexOptions.IgnoreCase)).Replace(console_text_cut, 
                "<span class=group>[</span><span class=shorted_address title='$1'>$2</span><span class=group>]</span>");
            console_text_cut = (new Regex(@"(push|pop|cli)", RegexOptions.IgnoreCase)).Replace(console_text_cut,
                "<span class=op_stack>$1</span>");
            console_text_cut = (new Regex(@"(l?call|l?jmp|je|jne|jbe?|ret)", RegexOptions.IgnoreCase)).Replace(console_text_cut,
                "<span class=op_ip>$1</span>");
            console_text_cut = (new Regex(@"(mov|lea|clc|xchg|setne|qword|dword|byte)", RegexOptions.IgnoreCase)).Replace(console_text_cut,
                "<span class=op_mov>$1</span>");
            console_text_cut = (new Regex(@"(add|sub|inc|dec|idiv|imul|sbb)(\s)", RegexOptions.IgnoreCase)).Replace(console_text_cut,
                "<span class=op_add>$1</span>$2");
            console_text_cut = (new Regex(@"(nop)", RegexOptions.IgnoreCase)).Replace(console_text_cut,
                "<span class=op_nop>$1</span>");
            console_text_cut = (new Regex(@"(invalid)", RegexOptions.IgnoreCase)).Replace(console_text_cut,
                "<span class=op_err>$1</span>");
            console_text_cut = (new Regex(@"([\,\-\+\[\]\(\)])", RegexOptions.IgnoreCase)).Replace(console_text_cut,
                "<span class=group>$1</span>");
            //webpage temporary hardcoded

            //html += "<div class=r2code_s><pre>"         + console_text_cut_copy + "</pre></div>";
            html = "<div class=r2code id=r2code>" + console_text_cut      + "</div>";
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
