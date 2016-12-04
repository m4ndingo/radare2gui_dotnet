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
    var sAddress = value;
	if(sAddress[0]=="-" || sAddress<0)
		sAddress=sAddress.replace(/^-/ ,"-0x")
	else
		sAddress= "0x" + sAddress;
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
			value = parseInt(value).toString(16); // this should be a callback to decorator
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

$(document).click(function (e)
{
	if (timeoutId) {
		window.clearTimeout(timeoutId);
		window.clearTimeout(popup_timeout_id);
		timeoutId=null;
		popup_timeout_id=null;
	}
	if( !$( "#my_dialog" ).is(":visible")) return;
    clear_selected_addresses();
	$( "#my_dialog" ).hide()
});
var last_control="";
$( "#my_dialog" ).hover(
  function(e) {
	last_control="my_dialog";
    //console.log(e.currentTarget.id);
  }
);
$( "#r2code" ).hover(
  function(e) {
	last_control="r2code";
    //console.log(e.currentTarget.id);
  }
);
$( "span" ).hover(
  function(e) {
	cname=e.currentTarget.className;
	if( cname!="address" && cname!="comment")
		last_control="out "+e.currentTarget;
	else
		last_control=cname;
    //console.log(last_control);
  }
);
$("#my_dialog").click(function (e)
{
	var myDialog = $('#my_dialog');
	myDialog.hide()
});
var saved_address = "";
var seladdr = 0;
var popup_timeout_id = null;
function add_select_event(id,cname_orig,cname_selected)
{
    $(id).hover(function (e) { 
		var text = null;       
        if ($(this).hasClass("address")) {
			if(address!=saved_address)
			{
				if (timeoutId) {
					window.clearTimeout(timeoutId);
					window.clearTimeout(popup_timeout_id);
					timeoutId=null;
					popup_timeout_id=null;
				}
		        var address = $(id).text();				
				var myDialog = $('#my_dialog');
				var dialog_visible = myDialog.is(":visible");
				if(last_control!="div")
					clear_selected_addresses();
				if(dialog_visible){
					return;
				}
				real_address=$(id).prop("real_address");
				if(real_address && real_address.length>0)
				{
					$(id).text(real_address);
				}
				myDialog.hide()
				timeoutId = window.setTimeout(function () {                    
					saved_address=address;
    				if ( address!=saved_address ){
						return;
					}
					text = address;
					if (text) {
						text = pd_previews[address];							
						if (text!=null && text.length>0) {
							text = text.replace(/\n/g, "<br>")
							text = text.replace(/\s\s/g, "&nbsp;&nbsp;")
							myDialog.css({ top: e.pageY + 10, left: e.pageX > 100 ? 100 : e.PageX, position: 'absolute' });
							myDialog.html(text);                    
							make_selectable();						
							$(id).removeClass("address").addClass("address_selected");
						}else{
							$(id).text($(id).prop('title')); // set real address							
							$(id).removeClass("address").addClass("address_selected");
						}
					popup_timeout_id = window.setTimeout(function () {
						if(last_control!="address" && last_control!="comment") return;
						var address = $(id).text();
						if(address==saved_address && text!=null)
							myDialog.show();
						}, 1000);
					}
				},2000);
				/*
				window.setTimeout(function () {
					if(last_control!="address" && last_control!="comment") return;
					var address = $(id).text();
					if(address==saved_address && text!=null)
						myDialog.show();
					}, 3000);
				*/
				return;
			}
        }
        else {
            if(address!=saved_address)
				$(id).removeClass("address_selected").addClass("address");
        }
	}); // hover
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