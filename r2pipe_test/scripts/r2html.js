var html = "";
var keys = [];

// #TODO: generate decorators from gui ...

var decorators = {
    "num2hex":
        [   "offset", "vaddr", "paddr", "plt", "from", "addr", "addr_end", "eip", "esp",
            "rax", "rbx", "rcx", "rdx", "rsi", "rdi", "r8", "r9", "r10", "r11", "r12", "r13",
            "r14", "r15", "rsp", "rbp", "rflags", "rip"]
}

// thx stackoverflow, WebBrowser for .net is O_o=*x!< compatibility 
// with modern webbrowsers s*x... need a better .net control
function inList(psString, laList) 
{
    if (!laList) return;
    var i = laList.length;
    while (i--) {
        if (laList[i] === psString) return true;
    }
    return false;
}
function text2html(varname, value)
{
    var sAddress = "0x" + value.toString(16);
    var id = varname + "_" + sAddress;
    if (addresses && !inList(sAddress, addresses))
    {
        addresses = addresses.concat(sAddress);
        add_select_event(varname, "address", "address_selected");
    }
	return "<span class="+varname+" name="+varname+" id="+id+">"+sAddress+"</span>";
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
	html += "<table class=tbl border=0 id=out_dict contenteditable=true>\r\n";
	for (var key in dict) {
		value = dict[key];
		if(typeof(value) == "object") 
			value=readDict(value);
		value = decorate( key, value);
		html+="<tr valign=top><td class=op_stack nowrap>"+key;
		html+="</td><td width=100%>"+value+"</td></tr>\r\n";
	}
	html+="</table>";
	return html;
}
if ( r2output != null )
{
    for (var k in r2output[0]) keys.push(k);
    if (keys.length > 0) {
        html = "<table class=tbl border=0 id=out contenteditable=true width=100%>\r\n";
        html += "<tr>\r\n";
        html += "<th>" + keys.join("</th><th>") + "</th>";
        html += "</tr><tr>\r\n";
        for (i = 0; i < r2output.length, row = r2output[i]; i++) {
            html += "<tr>\r\n";
            for (j = 0; j < keys.length, key = keys[j]; j++) {
                value = decorate(key, row[key]);
                html += "<td nowrap>" + value + "</td>";
            }
            html += "</tr>\r\n";
        }
        html += "</table>";
    } else {
        html += readDict(r2output);
    }	
	document.write(html);
}

var r2code = document.getElementById('r2code');
if (r2code) r2code.contentEditable = true;

var show_msg = true;
var timeoutId = null;

function close_dialog(e)
{
    $('#my_dialog').hide();
    if (timeoutId) window.clearTimeout(timeoutId);
    timeoutId = null;
}

$("#r2code").click(function (e)
{
    var cname_orig = e.target.className;
    if (cname_orig == 'address' || cname_orig == 'address_selected') return;
    close_dialog(e);
});
$("#my_dialog").click(function (e)
{
    var cname_orig = e.target.className;
    if (cname_orig == 'address' || cname_orig == 'address_selected') return;
    close_dialog(e);
});
var seladdr = 0;
function add_select_event(id,cname_orig,cname_selected)
{
    $(id).hover(function (e) {        
        if ($(this).hasClass("address")) {
            $(id).removeClass("address").addClass("address_selected");            
        }
        else {
            $(id).removeClass("address_selected").addClass("address");
        }

        if ($(this).hasClass("address_selected")) {
            if (timeoutId) return;
            timeoutId = window.setTimeout(function () {                    
                var myDialog = $('#my_dialog');
                var address = $(id).text();
                
                clear_selected_addresses();
                text = address;
                if (text) {
                    try {
                        text = pd_previews[address];
                        if (text) {
                            text = text.replace(/\n/g, "<br>")
                            text = text.replace(/\s\s/g, "&nbsp;&nbsp;")
                        }
                    } catch (err) { } // may not exists
                    myDialog.css({ top: e.pageY + 10, left: e.pageX > 100 ? 100 : e.PageX, position: 'absolute' });
                    clear_selected_addresses();
                    myDialog.html(text);                    
                    make_selectable();
                    myDialog.show();
                }
            }, 2000);
        }
    });
}

function clear_selected_addresses() {
    spans = document.getElementsByTagName('span');
    for (var i = 0; i < spans.length; i++) {
		var id = spans[i].getAttribute("id");
		if ( id != null )
		{
			var cname = spans[i].className;
			if( cname == "address_selected" )
				spans[i].className = "address";
		}
	}
}
var idx = 0;
function make_selectable() {
    //clear_selected_addresses();
    spans = document.getElementsByTagName('span');
    if (!spans) return;
    for (var i = 0; i < spans.length; i++) {
        var id = spans[i].getAttribute("id");
        if (id != null && id == "_") {
            var spanClass = spans[i].getAttribute("class");
            spans[i].id = idx;
            add_select_event("#" + idx, "address", "address_selected");
            idx++;
        }
    }
}
make_selectable();
//document.write(keys);
//document.write(JSON.stringify(r2output));