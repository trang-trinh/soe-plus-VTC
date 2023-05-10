const draggable = (ele,doc) => {
    // The current position of mouse
    let x = 0;
    let y = 0;

    const mouseDownHandler = function (e) {
        const r = e.target.getBoundingClientRect();
        if (r.right < e.x + 20 && r.bottom < e.y + 20)
            return;
        // Get the current mouse position
        x = e.clientX;
        y = e.clientY;
        // Attach the listeners to `document`
        doc.addEventListener('mousemove', mouseMoveHandler);
        doc.addEventListener('mouseup', ()=>{
            mouseUpHandler(doc);
        });
    };

    const mouseMoveHandler = function (e) {
        // How far the mouse has been moved
        const dx = e.clientX - x;
        const dy = e.clientY - y;

        // Set the position of element
        ele.style.top = `${ele.offsetTop + dy}px`;
        ele.style.left = `${ele.offsetLeft + dx}px`;

        // Reassign the position of mouse
        x = e.clientX;
        y = e.clientY;
    };

    const mouseUpHandler = function (doc) {
        // Remove the handlers of `mousemove` and `mouseup`
        doc.removeEventListener('mousemove', mouseMoveHandler);
        doc.removeEventListener('mouseup', mouseUpHandler);
    };
    ele.addEventListener('mousedown', mouseDownHandler);
}
const wrap = (el, wrapper) => {
    el.parentNode.insertBefore(wrapper, el);
    wrapper.appendChild(el);
}
const draggableEle = (img,doc) => {
    doc=doc||document;
    let divdraggable = doc.createElement("div");
    divdraggable.setAttribute("style", img.getAttribute("style"));
    divdraggable.style.width = img.getAttribute("width") + "px";
    divdraggable.style.height = img.getAttribute("height") + "px";
    divdraggable.style.removeProperty("margin-left");
    divdraggable.style.removeProperty("margin-top");
    divdraggable.style.removeProperty("margin-right");
    divdraggable.style.removeProperty("margin-bottom");
    divdraggable.classList.add("resizable");
    divdraggable.classList.add("draggable");
    img.setAttribute("draggable", false);
    img.setAttribute("width", "100%");
    img.setAttribute("height", "100%");
    img.removeAttribute("style");
    wrap(img, divdraggable);
    draggable(divdraggable,doc);
}
const parseHTML = (html) => {
    if (!html) return null;
    var doc = new DOMParser().parseFromString(html, "text/html");
    doc = doc.querySelector('span') || doc;
    return (doc.innerHTML || "").replaceAll("&nbsp;", "").trim();
}
const parseHTMLText = (html, line) => {
    var doc = new DOMParser().parseFromString(html, "text/html");
    doc = doc.querySelector("span") || doc;
    if (!doc.innerHTML) return "";
    html = doc.innerHTML.replaceAll("&nbsp;", "").trim();
    if (line == null) line = 20;
    if (html.length < line) return html;
    return html.substring(0, line) + "...";
}
//Excel
function toColumnName(num) {
    let nn=num;
    for (var ret = '', a = 1, b = 26; (num -= a) >= 0; a = b, b *= 26) {
      ret = String.fromCharCode(parseInt((num % b) / a) + 65) + ret;
    }
    //console.log(nn+"="+ret);
    return ret;
  }
const colName = (n) => {
    return toColumnName(n);
    var ordA = 'a'.charCodeAt(0);
    var ordZ = 'z'.charCodeAt(0);
    var len = ordZ - ordA + 1;

    var s = "";
    while (n >= 0) {
        s = String.fromCharCode(n % len + ordA) + s;
        n = Math.floor(n / len) - 1;
    }
    return s.toLocaleUpperCase();
}
const colNumber = (text) => {
    text = text.replace(/[0-9]/g, '').toUpperCase()
    var cl_no = 0
    var len = text.length;
    for (var i = 0; i < len; i++) {
        cl_no += (Math.pow(26, (len - i - 1)) * (text.charCodeAt(i) - 64));
    }
    return cl_no;
}
const getSpreadSheetCellNumber = (row, column) => {
    let result = '';
    // Get spreadsheet column letter
    let n = column;
    while (n >= 0) {
        result = String.fromCharCode(n % 26 + 65) + result;
        n = Math.floor(n / 26) - 1;
    }

    // Get spreadsheet row number
    result += `${row + 1}`;

    return result;
};
export { draggable, draggableEle, wrap, parseHTML, parseHTMLText,colNumber, colName, getSpreadSheetCellNumber }