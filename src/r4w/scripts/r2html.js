/*
hash = window.location.hash;
hash = "#0x0804842d";
if(hash)
{
	hash = hash.substr(1);
	window.location.hash = hash;
	document.write(hash);
	$("html,body").animate({scrollTop: $('span[title='+hash+']').offset().top},0);
}
*/
//document.write(url);

var html = "";
var keys = [];

// #TODO: generate decorators from gui ...

var decorators = {
    "num2hex":
        [   "offset", "vaddr", "paddr", "plt", "from", "addr", "addr_end", 
			"eip", "esp", "eax", "ebx", "ecx", "edx", "esi", "edi", "ebp", "eflags",
            "rax", "rbx", "rcx", "rdx", "rsi", "rdi", "r0", "r1", "r2", "r3", "r4", "r5",
			"r6", "r7", "r8", "r9", "r10", "r11", "r12", "r13", "r14", "r15", "r16", "r17",
			"sb", "sl", "fp", "ip", "lr", "pc", "rsp", "rbp", "rflags", "rip", 
			"sp", "cpsr"],
	"b64dec":
		[	"string"	]
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
function zeropad(sAddress, length)
{
	while (sAddress.length < length) {
		sAddress = "0" + sAddress;
	}
	return sAddress;
}
function text2html(varname, value)
{
    var sAddress = value;	
	if(sAddress[0]=="-" || sAddress<0)
	{
		sAddress = sAddress.replace(/^-/ ,"");
		sAddress = "-0x" + zeropad(sAddress, address_hexlength);
	}
	else
	{
		sAddress= "0x" + zeropad(sAddress, address_hexlength);
	}
    var id = varname + "_" + sAddress;
    if (addresses && !inList(sAddress, addresses))
    {
        addresses = addresses.concat(sAddress);
        add_select_event(varname, "address", "address_selected");
    }
	return	"<span class='address' name="+varname+
			" id=_ real_address="+sAddress+">"+
			sAddress+"</span>";
}
var Base64={_keyStr:"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=",encode:function(e){var t="";var n,r,i,s,o,u,a;var f=0;e=Base64._utf8_encode(e);while(f<e.length){n=e.charCodeAt(f++);r=e.charCodeAt(f++);i=e.charCodeAt(f++);s=n>>2;o=(n&3)<<4|r>>4;u=(r&15)<<2|i>>6;a=i&63;if(isNaN(r)){u=a=64}else if(isNaN(i)){a=64}t=t+this._keyStr.charAt(s)+this._keyStr.charAt(o)+this._keyStr.charAt(u)+this._keyStr.charAt(a)}return t},decode:function(e){var t="";var n,r,i;var s,o,u,a;var f=0;e=e.replace(/[^A-Za-z0-9+/=]/g,"");while(f<e.length){s=this._keyStr.indexOf(e.charAt(f++));o=this._keyStr.indexOf(e.charAt(f++));u=this._keyStr.indexOf(e.charAt(f++));a=this._keyStr.indexOf(e.charAt(f++));n=s<<2|o>>4;r=(o&15)<<4|u>>2;i=(u&3)<<6|a;t=t+String.fromCharCode(n);if(u!=64){t=t+String.fromCharCode(r)}if(a!=64){t=t+String.fromCharCode(i)}}t=Base64._utf8_decode(t);return t},_utf8_encode:function(e){e=e.replace(/rn/g,"n");var t="";for(var n=0;n<e.length;n++){var r=e.charCodeAt(n);if(r<128){t+=String.fromCharCode(r)}else if(r>127&&r<2048){t+=String.fromCharCode(r>>6|192);t+=String.fromCharCode(r&63|128)}else{t+=String.fromCharCode(r>>12|224);t+=String.fromCharCode(r>>6&63|128);t+=String.fromCharCode(r&63|128)}}return t},_utf8_decode:function(e){var t="";var n=0;var r=c1=c2=0;while(n<e.length){r=e.charCodeAt(n);if(r<128){t+=String.fromCharCode(r);n++}else if(r>191&&r<224){c2=e.charCodeAt(n+1);t+=String.fromCharCode((r&31)<<6|c2&63);n+=2}else{c2=e.charCodeAt(n+1);c3=e.charCodeAt(n+2);t+=String.fromCharCode((r&15)<<12|(c2&63)<<6|c3&63);n+=3}}return t}}

function decorate(varname, value)
{
	for (var key in decorators)
	{
		if( inList(varname, decorators[key]) == true )
		{
			if( key=="num2hex" )
			{
				value = parseInt(value).toString(16); // this should be a callback to decorator
				value = text2html(varname, value);
			}
			if( key=="b64dec" )
			{
				value = Base64.decode(value);
			}
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

/*
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
*/

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

$("#r2code").click(function (e)
{
	var myDialog = $('#my_dialog');
	myDialog.hide()
	//clear_selected_addresses();
});

var saved_address = "";
var seladdr = 0;
var popup_timeout_id = null;

function add_select_event(id,cname_orig,cname_selected)
{
    $(id).click(function (e) { 
		var text = null;       
        if ($(this).hasClass("address")) {
			var address = $(id).text();				
			if(address!=saved_address)
			{
				clear_selected_addresses();
				if(  $(id).prop('title') && $(id).text() != $(id).prop('title') )
				{
					$(id).text($(id).prop('title')); // set real address							
					//$(id).removeClass("address_selected").addClass("address"); // remove selection
					return;
				}
				$(id).removeClass("address").addClass("address_selected");		        
				var myDialog = $('#my_dialog');
				var dialog_visible = myDialog.is(":visible");
				myDialog.hide()
				saved_address=address;
    			if ( address!=saved_address ){
					return;
				}
				text = address;
				if (text && pd_previews) {
					text = pd_previews[address];							
					if (text!=null && text.length>0) {
						x_pos=e.pageX-text.length/2;
						y_pos=e.pageY-26;
						if(x_pos<0)
						{
							x_pos=0;
							y_pos+=38;
						}
						text = text.replace(/\n/g, "<br>")
						text = text.replace(/\s\s/g, "&nbsp;&nbsp;")
						myDialog.css({ top: y_pos, left: x_pos, position: 'absolute' });
						myDialog.html(text);                    
						make_selectable();						
					}else{
						if( $(id).text() != $(id).prop('title') )
						{
							$(id).text($(id).prop('title')); // set real address							
							$(id).removeClass("address_selected").addClass("address"); // remove selection
						}
					}
				popup_timeout_id = window.setTimeout(
					function () {
						//if(last_control!="address" && last_control!="comment") return;
						var address = $(id).text();
						text = pd_previews[address];
						if(address==saved_address && text!=null)
						{
							myDialog.show();
							//clear_selected_addresses();
						}
					},0);
				}
			}
        }
        else {
			var address = $(id).text();				
            if(address!=saved_address)
				$(id).removeClass("address_selected").addClass("address");
        }
	}); // click
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
	saved_address=null; // also clear saved address
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
			//console.log(idx);
            idx++;
        }
    }
}
make_selectable();
//document.write(keys);
//document.write(JSON.stringify(r2output));