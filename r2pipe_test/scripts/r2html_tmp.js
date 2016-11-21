function dump_json(jsondata)
{
    document.write("<span class=r2code>");
    document.write(JSON.stringify(jsondata, null, "   "));
    document.write("</span>");
}

if (r2output != null && typeof(r2output) == "object") dump_json(r2output);