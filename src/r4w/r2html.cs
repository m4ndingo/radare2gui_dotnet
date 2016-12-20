using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Windows.Forms;

namespace r2pipe_test
{
    public class r2html
    {
        R2PIPE_WRAPPER r2pw = null;
        List<string> addresses;
        public r2html(R2PIPE_WRAPPER r2pw)
        {
            this.r2pw = r2pw;
            this.addresses = new List<string>();
        }
        private string htmlize(string console_text, ref MatchCollection mc)
        {
            int maxlen_line = int.Parse(r2pw.rconfig.load<int>("gui.max_line_length", 150));
            string html = "";
            string console_text_cut = "";
            string console_text_cut_copy = "";
            if (console_text == null) return null;
            console_text_cut = console_text;
            console_text_cut = console_text_cut.Replace("<", "&lt");
            Regex address_regex = new Regex((@"\b(0x[0-9a-f]{3,})\b"), RegexOptions.IgnoreCase);
            mc = address_regex.Matches(console_text_cut);
            console_text_cut_copy = console_text_cut;            
            console_text_cut = (new Regex(@"(- offset -.+|int3\b|;[^\n]+)")).Replace(console_text_cut,
                "<span class=comment>$1</span>");       
            console_text_cut = (new Regex(@"\b((fcn|str)\.([\:\w]+))", RegexOptions.IgnoreCase)).Replace(console_text_cut,
                "<a name=$3></a><span class=address id=_ title='$0'>$3 <span class=agfpad>$2</span></span>");
            console_text_cut = (new Regex(@"((sub|sym)\.(imp\.)?([^\.]+\.dll_)?([\w\.]+))\b", RegexOptions.IgnoreCase)).Replace(console_text_cut,
                "<a name=$5></a><span class=address id='_' title='$1'>$5<span class=agfpad>$2.$3$4</span></span>");
            console_text_cut = (new Regex(@"(0x[0-9a-f]{1,2})([\s\]])", RegexOptions.IgnoreCase)).Replace(console_text_cut,
                "<span class=number>$1</span>$2");
            //console_text_cut = (new Regex(@"(0x[0-9a-f]{2,}\s+)([0-9a-f]{2,})\b", RegexOptions.IgnoreCase)).Replace(console_text_cut,
            //    "$1<span class=hexb>$2</span>");
            console_text_cut = (new Regex(@"(0x[0-9a-f]{2,})\b", RegexOptions.IgnoreCase)).Replace(console_text_cut,
                "<span class=hexb>$1</span>");
            console_text_cut = (new Regex(@"([\[\s])(0x[0-9a-f]{3,})([\]\s])\b", RegexOptions.IgnoreCase)).Replace(console_text_cut,
                "$1<span class=address id=_>$2</span>$3");
            console_text_cut = (new Regex(@"([-\+]\s)([0-9]{1,})\b", RegexOptions.IgnoreCase)).Replace(console_text_cut,
                "$1<span class=number>$2</span>");
            console_text_cut = (address_regex.Replace(console_text_cut,
                "<a name=$1></a><span class=address></span><span class=address title='$1' id=_>$1</span>"));
            console_text_cut = (new Regex(@"(push|pop\b|cli\b|int\b)", RegexOptions.IgnoreCase)).Replace(console_text_cut,
                "<span class=op_stack>$1</span>");
            console_text_cut = (new Regex(@"([rl]?jmp|je|jne|jbe?|ret|brcs)", RegexOptions.IgnoreCase)).Replace(console_text_cut,
                "<span class=op_ip>$1</span>");
            console_text_cut = (new Regex(@"\b([rl]?call)\b")).Replace(console_text_cut,
                "<span class=op_call>$1</span>");
            console_text_cut = (new Regex(@"(\bmov[wsxd]*\b|lea\b|clc|xchg|setne|qword|dword|byte|std\b|ldd)")).Replace(console_text_cut,
                "<span class=op_mov>$1</span>");
            console_text_cut = (new Regex(@"(add|subi?|inc|dec|i?div|[if]?mul|sbb|sbci?|adc)(\s)", RegexOptions.IgnoreCase)).Replace(console_text_cut,
                "<span class=op_add>$1</span>$2");
            console_text_cut = (new Regex(@"(nop)", RegexOptions.IgnoreCase)).Replace(console_text_cut,
                "<span class=op_nop>$1</span>");
            console_text_cut = (new Regex(@"(invalid)\b", RegexOptions.IgnoreCase)).Replace(console_text_cut,
                "<span class=op_err>$1</span>");
            console_text_cut = (new Regex(@"([\,\-\+\[\]\(\)])", RegexOptions.IgnoreCase)).Replace(console_text_cut,
                "<span class=group>$1</span>");
            console_text_cut = (new Regex(@"([re]ip:|pc\d?:)")).Replace(console_text_cut,
                "<span class=esil_rip>$1</span>");

            html = "<div class=r2code id=r2code>" + console_text_cut + "</div>";
            return html;
        }
        public string encodeutf8(string text)
        {
            byte[] bytes = {};
            if ( text!=null )
                bytes = Encoding.Default.GetBytes(text);
            return Encoding.UTF8.GetString(bytes);
        }
        public string convert(string cmds, string console_text, dynamic json_obj, ref MatchCollection mc) // calls htmlize or htmljsonize
        {
            string html_body = "", html_header = "";
            string css_filename = "";            
            string themeName = "classic";
            string js_filename = r2pw.rconfig.load<string>(
                "gui.scripts.js_def", "r2html.js");
            string jquery_base = r2pw.rconfig.load<string>(
                 "gui.datapath");
            if (r2pw.rconfig.dataPath == null)
            {
                r2pw.rconfig.dataPath = 
                    r2pw.find_dataPath(
                        string.Format(
                            @"{0}\media",
                            System.IO.Path.GetDirectoryName(Application.ExecutablePath)
                         )                    
                    );
                jquery_base = r2pw.rconfig.load<string>(
                 "gui.datapath");
            }
            if (jquery_base != null)
                jquery_base = "file:///" + jquery_base + @"\..\scripts\";
            else
                jquery_base = "https://code.jquery.com/";
            themeName = r2pw.rconfig.load<string>("gui.theme_name", themeName);
            js_filename  = r2pw.rconfig.load<string>("gui.datapath") + @"\..\scripts\" + js_filename;
            css_filename = 
                string.Format(
                    @"{0}\{1}",
                    r2pw.rconfig.load<string>("gui.datapath"),
                    r2pw.rconfig.load<string>("gui.hexview.css", "r2pipe.css")
                    );
            html_header = "<!DOCTYPE html>\n";
            //html_header += "<meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge,chrome=1\">\n";
            html_header += "<meta charset=\"UTF-8\">\n";
            html_header += "<html>\r\n";
            html_header += "<head>\r\n";
            html_header += "<link href='" + css_filename + "' rel='stylesheet'>\r\n";
            //html_header += "<link rel = \"stylesheet\" href = \"https://code.jquery.com/ui/1.12.1/themes/base/jquery-ui.css\" >\r\n";
            html_header += "<script src=\"" + jquery_base + "jquery-1.11.3.js\"></script>\r\n";
            html_header += "<script>var r2output = null; addresses = null; pd_previews = null; </script>\r\n";
            html_header += "<script>var address_hexlength = '" + r2pw.address_hexlength + "'; </script>\r\n";
            html_header += "</head>\r\n";
            html_header += "<body>\r\n";
            html_header += "<div id=my_dialog class=msg></div>\r\n";            
            // dump html or json js
            if (json_obj == null)
            {
                mc = null;
                html_body = htmlize(console_text, ref mc);
                html_body = "<div id=#r2code>" + html_body + "</div>\r\n";
            }else
            {
                html_body = htmljsonize(cmds, json_obj);
            }
            // add list with matched addresses and pd previews
            if (mc != null)
            {
                addresses.Clear();
                List<string> pd_previews    = new List<string>();
                Cursor.Current = Cursors.WaitCursor;
                // add funcions (fcn) to previews
                Regex address_regex = new Regex((@"\b((fcn|str)\.([\:\w]+))"), RegexOptions.IgnoreCase); // same regex that used up
                mc = address_regex.Matches(console_text);
                foreach (Match m in mc)
                {
                    string address = m.Groups[0].Value; // get the address
                    if (!address.StartsWith("str."))    // fix the fnc.0000000 address
                        address = "0x" + address;
                    if (!addresses.Contains(address))
                    {
                        addresses.Add(address); 
                    }
                }
                addresses = new HashSet<string>(addresses).ToList();
                foreach (string address in addresses)
                { 
                    if( cmds!=null && (cmds.StartsWith("pd") || cmds.StartsWith("agf")) && r2pw.fileName.StartsWith("-")==false )
                    {
                        string preview = ""; // get some previevs
                        string print_cmd = "psz";
                        if(!address.StartsWith("str."))
                            print_cmd = "pd 32";

                        preview = r2pw.run(print_cmd + " @ " + address);
                        if (preview != null)
                        {
                            preview = preview.TrimEnd('\n');
                            //preview = encodeutf8(preview);
                            preview = htmlize(preview, ref mc);
                            preview = preview.Replace("'", "\\'");
                            preview = preview.Replace("\r", "\\r");
                            preview = preview.Replace("\n", "\\n");
                            pd_previews.Add("'" + address + "':'" + preview + "'");
                        }
                    }
                }
                // add the rest of address
                foreach (Match m in mc)
                {
                    string address = m.Groups[2].Value;
                    addresses.Add(m.Groups[2].Value);
                }
                Cursor.Current = Cursors.Default;
                html_body += "<script>\r\n";
                html_body += "addresses   = ['" + string.Join("', '", addresses) + "'];\r\n";
                html_body += "pd_previews = {" + string.Join(", ", pd_previews) + "};\r\n";
                html_body += "</script>\r\n";
            }
            html_body += "<script src='" + js_filename + "'></script>\r\n";
            if (!File.Exists(js_filename))
                html_body += string.Format("<div class=msg></div>", js_filename);
            html_body += "</body>\r\n";
            html_body += "</html>\r\n";
            return html_header + html_body;
        }
        private string htmljsonize(string cmds, dynamic json_obj)
        {
            string js_string = JsonConvert.SerializeObject(json_obj);
            string html_body = "";
            //js_string = r2pw.escape_json(js_string);
            js_string = js_string.Replace(@"'", @"\'");
            js_string = js_string.Replace(@"\", @"\\");
            html_body += "<script>\r\n";
            html_body += "var r2cmds   = '" + cmds + "';\r\n";
            html_body += "var r2output = jQuery.parseJSON('" + js_string + "')\r\n";
            html_body += "</script>\r\n";
            return html_body;
        }
    }
}
