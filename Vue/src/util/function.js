export const encr = (str, strkey, CryptoJS) => {
    var key = CryptoJS.enc.Utf8.parse(strkey);
    var iv = CryptoJS.enc.Utf8.parse(strkey);
    return CryptoJS.AES.encrypt(CryptoJS.enc.Utf8.parse(str), key, {
        keySize: 128 / 8,
        iv: iv,
        mode: CryptoJS.mode.CBC,
        padding: CryptoJS.pad.Pkcs7
    });
}
export const decr = (str, strkey, CryptoJS) => {
    var key = CryptoJS.enc.Utf8.parse(strkey);
    var iv = CryptoJS.enc.Utf8.parse(strkey);
    var de = CryptoJS.AES.decrypt(str, key, {
        keySize: 128 / 8,
        iv: iv,
        mode: CryptoJS.mode.CBC,
        padding: CryptoJS.pad.Pkcs7
    });
    return de.toString(CryptoJS.enc.Utf8);
}
export const deepSearch = (data, value, key = 'id', sub = 'children', tempObj = {}) => {
    if (value && data) {
        data.find((node) => {
            if (node[key] == value) {
                tempObj.found = node;
                return node;
            }
            return deepSearch(node[sub], value, key, sub, tempObj);
        });
        if (tempObj.found) {
            return tempObj.found;
        }
    }
    return null;
};
export const getParent = (root, id, key, pnode) => {
    id = id.toString();
    var i, pnode, node;
    for (var i = 0; i < root.length; i++) {
        pnode = root[i];
        node = root[i];
        if (node[key].toString() === id || node.children && (node = getParent(node.children, id, key, pnode))) {
            return pnode;
        }
    }
    return null;
};

export const getParentNode = (nodeName, nodeValue, rootNode) => {
    const queue = [rootNode]
    while (queue.length) {
        const node = queue.shift()
        if (node[nodeName] === nodeValue) {
            return node
        } else if (node instanceof Object) {
            const children = Object.values(node)
            if (children.length) {
                queue.push(...children)
            }
        }
    }
    return null
};
export const renderTree = (data, id, name, title) => {
    let arrChils = [];
    let arrtreeChils = [];
    data
        .filter((x) => x.Capcha_ID == null)
        .forEach((m, i) => {
            m.IsOrder = i + 1;
            let om = { key: m[id], data: m };
            const rechildren = (mm, pid) => {
                let dts = data.filter((x) => x.Capcha_ID == pid);
                if (dts.length > 0) {
                    if (!mm.children) mm.children = [];
                    dts.forEach((em) => {
                        let om1 = { key: em[id], data: em };
                        rechildren(om1, em[id]);
                        mm.children.push(om1);
                    });
                }
            };
            rechildren(om, m[id]);
            arrChils.push(om);
            //
            om = { key: m[id], data: m[id], label: m[name] };
            const retreechildren = (mm, pid) => {
                let dts = data.filter((x) => x.Capcha_ID == pid);
                if (dts.length > 0) {
                    if (!mm.children) mm.children = [];
                    dts.forEach((em) => {
                        let om1 = { key: em[id], data: em[id], label: em[name] };
                        retreechildren(om1, em[id]);
                        mm.children.push(om1);
                    });
                }
            };
            retreechildren(om, m[id]);
            arrtreeChils.push(om);
        });
    arrtreeChils.unshift({ key: -1, data: -1, label: "-----Chọn " + title + "----" });
    return { arrChils: arrChils, arrtreeChils: arrtreeChils };
};

function formatDate2String(date) {
    var d = new Date(date),
        month = '' + (d.getMonth() + 1),
        day = '' + d.getDate(),
        year = d.getFullYear();

    if (month.length < 2)
        month = '0' + month;
    if (day.length < 2)
        day = '0' + day;

    return [day, month, year].join('/');
}
export const convertDateDBToString = (obj) => {
    for (var key in obj) {
        let isnum = /^\d+$/.test(obj[key]);
        if (!isnum && moment(obj[key], moment.ISO_8601, true).isValid()) {
            obj[key] = formatDate2String(obj[key]);
        }
    }
}
import moment from "moment";
export const formatDate = (value, type) => {
    if (value) {
        switch (type) {
            case 'date':
                return moment(String(value)).format('DD/MM/yyyy');
                break;
            case 'datetime':
                return moment(String(value)).format('DD/MM/yyyy HH:mm');
                break;
            default:
                return moment(String(value)).format('DD/MM/yyyy HH:mm');
                break;
        }
    }
    return false;
}
export const change_unsigned = (str, c) => {
    var result = str;
    result = result.toLowerCase();
    result = result.replace(/à|á|ạ|ả|ã|â|ầ|ấ|ậ|ẩ|ẫ|ă|ằ|ắ|ặ|ẳ|ẵ/g, "a");
    result = result.replace(/è|é|ẹ|ẻ|ẽ|ê|ề|ế|ệ|ể|ễ/g, "e");
    result = result.replace(/ì|í|ị|ỉ|ĩ/g, "i");
    result = result.replace(/ò|ó|ọ|ỏ|õ|ô|ồ|ố|ộ|ổ|ỗ|ơ|ờ|ớ|ợ|ở|ỡ/g, "o");
    result = result.replace(/ù|ú|ụ|ủ|ũ|ư|ừ|ứ|ự|ử|ữ/g, "u");
    result = result.replace(/ỳ|ý|ỵ|ỷ|ỹ/g, "y");
    result = result.replace(/đ/g, "d");
    if (c) {
        result = result.replaceAll(" ", c);
    }
    return result;
}
export const removeAccents = (str) => {
    var AccentsMap = [
        "aàảãáạăằẳẵắặâầẩẫấậ",
        "AÀẢÃÁẠĂẰẲẴẮẶÂẦẨẪẤẬ",
        "dđ", "DĐ",
        "eèẻẽéẹêềểễếệ",
        "EÈẺẼÉẸÊỀỂỄẾỆ",
        "iìỉĩíị",
        "IÌỈĨÍỊ",
        "oòỏõóọôồổỗốộơờởỡớợ",
        "OÒỎÕÓỌÔỒỔỖỐỘƠỜỞỠỚỢ",
        "uùủũúụưừửữứự",
        "UÙỦŨÚỤƯỪỬỮỨỰ",
        "yỳỷỹýỵ",
        "YỲỶỸÝỴ"
    ];
    for (var i = 0; i < AccentsMap.length; i++) {
        var re = new RegExp('[' + AccentsMap[i].substr(1) + ']', 'g');
        var char = AccentsMap[i][0];
        str = str.replace(re, char);
    }
    return str.toLocaleLowerCase().replaceAll(" ", "");
}
export const isObject = (val) => {
    return (typeof val === 'object');
}
export const capitalizeFirstLetter = (string, f) => {
    try {
        if (f == true) {
            const words = string.split(" ");
            for (let i = 0; i < words.length; i++) {
                words[i] = words[i][0].toUpperCase() + words[i].substr(1);
            }
            return words.join(" ");
        }
        return string.charAt(0).toUpperCase() + string.slice(1);
    } catch (e) {
        return string;
    }
}
export const download = (filename, text) => {
    var element = document.createElement('a');
    element.setAttribute('href', 'data:text/plain;charset=utf-8,' + encodeURIComponent(text));
    element.setAttribute('download', filename);
    element.style.display = 'none';
    document.body.appendChild(element);
    element.click();
    document.body.removeChild(element);
}
export const strimHTML = (str) => {
    return str.replace(/<\/?("[^"]*"|'[^']*'|[^>])*(>|$)/g, "");
}
export const getTagSelection = (dwindow) => {
    const sel = dwindow.getSelection();
    const range = sel.getRangeAt(0);
    let istable = range.startContainer.parentElement.closest("td").children.length == 1;
    let tagn = istable ? "tr" : "p";
    return tagn;
}
export const uid = function () {
    return Date.now().toString(36) + Math.random().toString(36).substr(2);
}
export const getKeyByValue = (object, value) => {
    let vl = Object.keys(object).find(key => object[key] === value);
    return vl;
}
export const decodeHTMLEntities = (text) => {
    var entities = {
        'amp': '&',
        'apos': '\'',
        '#x27': '\'',
        '#x2F': '/',
        '#39': '\'',
        '#47': '/',
        'lt': '<',
        'gt': '>',
        'nbsp': ' ',
        'quot': '"'
    }
    return text.replace(/&([^;]+);/gm, function (match, entity) {
        return entities[entity] || match
    })
}
export const getRandomInt = (min, max, st) => {
    return (Math.floor(Math.random() * (max - min + 1)) + min) * (st || 1);
}
export const getRandomArray = (arr) => {
    return arr[Math.floor(Math.random() * arr.length)];
}
export const getCookie = (name) => {
    function escape(s) { return s.replace(/([.*+?\^$(){}|\[\]\/\\])/g, '\\$1'); }
    var match = document.cookie.match(RegExp('(?:^|;\\s*)' + escape(name) + '=([^;]*)'));
    return match ? match[1] : null;
}
var monthNames = [
    "January", "February", "March",
    "April", "May", "June", "July",
    "August", "September", "October",
    "November", "December"
];

var Days = [
    "Sunday", "Monday", "Tuesday", "Wednesday",
    "Thursday", "Friday", "Saturday"
];
var pad = function (n, width, z) {
    z = z || '0';
    n = n + '';
    return n.length >= width ? n : new Array(width - n.length + 1).join(z) + n;
}
export const utilformatDate = (dt, format) => {
    format = format.replace('ss', pad(dt.getSeconds(), 2));
    format = format.replace('s', dt.getSeconds());
    format = format.replace('dd', pad(dt.getDate(), 2));
    format = format.replace('d', dt.getDate());
    format = format.replace('mm', pad(dt.getMinutes(), 2));
    format = format.replace('m', dt.getMinutes());
    format = format.replace('MMMM', monthNames[dt.getMonth()]);
    format = format.replace('MMM', monthNames[dt.getMonth()].substring(0, 3));
    format = format.replace('MM', pad(dt.getMonth() + 1, 2));
    format = format.replace(/M(?![ao])/, dt.getMonth() + 1);
    format = format.replace('DD', Days[dt.getDay()]);
    format = format.replace(/D(?!e)/, Days[dt.getDay()].substring(0, 3));
    format = format.replace('yyyy', dt.getFullYear());
    format = format.replace('YYYY', dt.getFullYear());
    format = format.replace('yy', (dt.getFullYear() + "").substring(2));
    format = format.replace('YY', (dt.getFullYear() + "").substring(2));
    format = format.replace('HH', pad(dt.getHours(), 2));
    format = format.replace('H', dt.getHours());
    return format;
}
export const formatSize = (bytes) => {
    if (bytes === 0) return "0 B";
    const k = 1024;
    const sizes = ["B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + " " + sizes[i];
};
export const checkURL = (url, listModule) => {
    var checkrw = listModule.find(x => x.is_link == url);
    if (
        checkrw != null
    ) {
        return true;
    } else
        return false;
}
 
  
export const autoFillDate = (model,prop_name) => {
    var ip_val = document.getElementById(prop_name).value;
    var today = new Date();
        var dd = today.getDate();
        var mm = today.getMonth() + 1;
        var yyyy = today.getFullYear();
        if(dd < 10) dd = '0' + dd;
        if(mm < 10) mm = '0' + mm;
    if(ip_val.length === 2 && ip_val > 0){
        model[prop_name] = ip_val + '/' + mm + '/' + yyyy;
        ip_val = "";
        if(!moment(model[prop_name], "DD/MM/YYYY", true).isValid()){
          model[prop_name] = dd + '/' + mm + '/' + yyyy;
        }
    }
    else if(ip_val.length === 4){
      let d = ip_val.substring(0, 2);
      let m = ip_val.substring(2, 4)
        model[prop_name] = d + '/' + m + '/' + yyyy;
        ip_val = "";
        if(!moment(model[prop_name], "DD/MM/YYYY", true).isValid()){
          model[prop_name] = dd + '/' + mm + '/' + yyyy;
        }
    }
    else if(ip_val.length === 6){
      let d = ip_val.substring(0, 2);
      let m = ip_val.substring(2, 4)
      let y = ip_val.substring(4, 6)
        model[prop_name] =  d + '/' + m + '/' + '20' + y;
        ip_val = "";
        if(!moment(model[prop_name], "DD/MM/YYYY", true).isValid()){
          model[prop_name] = dd + '/' + mm + '/' + yyyy;
        }
    }
}