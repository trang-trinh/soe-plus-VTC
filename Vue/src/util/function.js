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
export const change_unsigned = (str) => {
    var result = str;
    result = result.toLowerCase();
    result = result.replace(/à|á|ạ|ả|ã|â|ầ|ấ|ậ|ẩ|ẫ|ă|ằ|ắ|ặ|ẳ|ẵ/g, "a");
    result = result.replace(/è|é|ẹ|ẻ|ẽ|ê|ề|ế|ệ|ể|ễ/g, "e");
    result = result.replace(/ì|í|ị|ỉ|ĩ/g, "i");
    result = result.replace(/ò|ó|ọ|ỏ|õ|ô|ồ|ố|ộ|ổ|ỗ|ơ|ờ|ớ|ợ|ở|ỡ/g, "o");
    result = result.replace(/ù|ú|ụ|ủ|ũ|ư|ừ|ứ|ự|ử|ữ/g, "u");
    result = result.replace(/ỳ|ý|ỵ|ỷ|ỹ/g, "y");
    result = result.replace(/đ/g, "d");
    return result;
}
export const checkURL = (url, listModule) => {


    var checkrw = listModule.find(x => x.is_link == url);

    if (
        checkrw != null
    ) {
        return true;
    } else
        return false;
}
export const removeAccents = (str) => {
    return str.normalize('NFD')
              .replace(/[\u0300-\u036f]/g, '')
              .replace(/đ/g, 'd').replace(/Đ/g, 'D');
  }