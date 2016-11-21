var html = "";
var keys = [];
// #TODO: generate decorators from gui ...
var decorators = {"num2hex":["offset","vaddr","paddr","plt","from"]}

// thx stackoverflow, WebBrowser for .net is O_o=*x!< compatibility 
// with modern webbrowsers s*x... need a better .net control
function inList(psString, laList) 
{
    var i = laList.length;
    while (i--) {
        if (laList[i] === psString) return true;
    }
    return false;
}
function text2html(varname, value)
{
	return "<span class="+varname+" name="+varname+">0x"+value.toString(16)+"</span>";
}
function decorate(varname, value)
{
	for (var key in decorators)
	{
		if( inList(varname, decorators[key]) == true )
		{
			value = value.toString(16); // this should be a callback to decorator
			value = text2html(varname, value);
		}
	}
	return value;
}
function readDict(dict)
{
	var html="";
	html+= "<table class=tbl_dict border=0>\r\n";
	for (var key in dict) {
		value = dict[key];
		if(typeof(value) == "object") 
			value=readDict(value);
		value = decorate( key, value);
		html+="<tr valign=top><td class=op_stack nowrap>"+key;
		html+="</td><td class=group>:</td><td width=100%>"+value+"</td></tr>\r\n";
	}
	html+="</table>";
	return html;
}
if ( r2output != null )
{
	for(var k in r2output[0]) keys.push(k);
	html  = "<table class=tbl border=0>\r\n";
	html += "<tr>\r\n";
	html += "<th>"+keys.join("</th><th>")+"</th>";
	html += "</tr><tr>\r\n";
	for(i=0;i<r2output.length,row=r2output[i];i++)
	{
		html+="<tr>\r\n";
		for(j=0;j<keys.length,key=keys[j];j++)
		{
			value = decorate( key, row[key] );
			html+="<td nowrap>"+value+"</td>";
		}
		html+="</tr>\r\n";
	}
	html+="</table>";

	html+=readDict(r2output);
	document.write(html);

	//document.write(keys);
	//document.write(JSON.stringify(r2output));
}