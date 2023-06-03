<script setup>
import { ref, inject, onMounted, computed } from "vue";
import { useToast } from "vue-toastification";
import { encr, change_unsigned } from "../../../../util/function.js";

const cryoptojs = inject("cryptojs");
const store = inject("store");
const axios = inject("axios");
const swal = inject("$swal");
const toast = useToast();
const config = {
    headers: { Authorization: `Bearer ${store.getters.token}` },
};
const viewDB = 'View_SearchEngine';
const baseUrlCheck = baseURL;
const selectedCols = ref();
const expandedKeys = ref({});
const selectedKey = ref(null);
const datas = ref([]);
const cols = ref([]);
const colgroups = ref([]);
const ipsearch = ref();
const itemButExports = [
    {
        label: 'In',
        icon: 'pi pi-print',
        command: () => {
            PrintDiv();
        }
    },
    {
        label: 'Xuất file',
        icon: 'pi pi-download',
        command: () => {
            downloadFile();
        }
    }
];
const drTypes = ref([
    { text: "Lớn hơn", value: ">", types: ",1,2,3," },
    { text: "Lớn hơn hoặc bằng", value: ">=", types: ",1,2,3," },
    { text: "Bằng", value: "=" },
    { text: "Nhỏ hơn", value: "<", types: ",1,2,3," },
    { text: "Nhỏ hơn hoặc bằng", value: "<=", types: ",1,2,3," },
    { text: "Khác", value: "<>", types: ",0,1,2,3,4,7,", placeholder: "Tìm trong chuỗi khác với 'giá trị' nhập vào" },
    { text: "Gồm", value: "Contain", types: ",0,5,7,", placeholder: "Tìm trong chuỗi có chứa 'giá trị' nhập vào" },
    { text: "Bắt đầu bằng", value: "StartWith", types: ",0,7,", placeholder: "Tìm trong chuỗi bắt đầu bằng 'giá trị' nhập vào" },
    { text: "Kết thúc bằng", value: "EndWith", types: ",0,7,", placeholder: "Tìm trong chuỗi kết thúc bằng 'giá trị' nhập vào" },
    { text: "Trong khoảng", value: "FromTo", types: ",1,2,3,", placeholder: "Giá trị đầu và giá trị cuối cách nhau bởi dấu - , ví dụ 10-15" },
    { text: "Có", value: " =1 ", types: ",4,", hide: true },
    { text: "Không", value: " =0 ", types: ",4,", hide: true },
    { text: "Có giá trị", value: " IS NOT NULL ", hide: true },
    { text: "Không Có giá trị", value: " IS NULL ", hide: true }
]);
const drTypeDate = [
    { text: "Mặc định", value: "convert(datetime,$date,103)" },
    { text: "Ngày", value: "DAY(convert(datetime,$date,103))" },
    { text: "Tháng", value: "MONTH(convert(datetime,$date,103))" },
    { text: "Năm", value: "YEAR(convert(datetime,$date,103))" },
    { text: "Giờ", value: "FORMAT(convert(datetime,$date,103),'HH')" },
    { text: "Phút", value: "FORMAT(convert(datetime,$date,103),'mm')" },
    { text: "Ngày tháng", value: "FORMAT(convert(datetime,$date,103),'dd/MM')" },
    { text: "tháng năm", value: "FORMAT(convert(datetime,$date,103),'MM/yyyy')" },
];

const opMic = ref({
    start: false,
    isshow: false
});
var SpeechRecognition = SpeechRecognition || webkitSpeechRecognition;
var SpeechGrammarList = SpeechGrammarList || webkitSpeechGrammarList;
var grammar = '#JSGF V1.0;'
var recognition = new SpeechRecognition();
var speechRecognitionList = new SpeechGrammarList();
speechRecognitionList.addFromString(grammar, 1);
recognition.grammars = speechRecognitionList;
recognition.lang = 'vi-VN';
recognition.interimResults = true;
recognition.continuous = true;
let isok = false;
let resultIndex = 0;
recognition.onresult = async function (evt) {
    let reArr = [];
    for (let i = resultIndex; i < evt.results.length; i++) {
        const result = evt.results[i]
        reArr.push(result);
        if (result.isFinal) CheckForCommand(result)
    }
    const t = reArr
        .map(result => result[0])
        .map(result => result.transcript)
        .join('')
    let str = t.replace(/( xong rồi| song rồi| ok| xoá| tìm| xóa)/igm, "");
    if (ipsearch.value.trim() != str.trim()) {
        ipsearch.value = str.trim();
    }
    str = t.toLocaleLowerCase().trim();
    console.log(str);
    if (str.endsWith("xong rồi") || str.endsWith("song rồi")) {
        stopMic(false);
    } else if (str.endsWith("ok") || str.endsWith("tìm")) {
        if (!isok) {
            isok = true;
            await goSearch();
            isok = false;
        }
    } else if (str.endsWith("xoá") || str.endsWith("xóa")) {
        ipsearch.value = "";
        resultIndex = evt.results.length - 1;
    }
};
recognition.onspeechend = function () {
    recognition.stop();
    opMic.value.start = false;
};
recognition.onend = () => {
    opMic.value.start = false;
}
recognition.onerror = function (event) {
    opMic.value.error = true;
}
const CheckForCommand = (result) => {
    const t = result[0].transcript.toLowerCase();
    if (t.endsWith('ok')) {
        //goSearch();
    }
}
const stopMic = (f) => {
    //ipsearch.value = "";
    opMic.value.isshow = f;
    opMic.value.start = false;
    opMic.value.error = false;
    recognition.stop();
}
const goMic = () => {
    isok = false;
    resultIndex = 0;
    ipsearch.value = "";
    opMic.value.isshow = true;
    opMic.value.start = false;
    opMic.value.error = false;
}
const startMic = () => {
    if (opMic.value.start) {
        toast.success("Bắt đầu tìm kiếm bằng giọng nói");
        recognition.start();
    }
    else {
        toast.info("Kết thúc tìm kiếm bằng giọng nói");
        recognition.stop();
        opMic.value.isshow = false;
    }
}
let odby = [];
const checkValidkey = (k) => {
    return true;
}
const validText = (txt) => {
    return txt;
}
const renderAutoInput = (txt) => {
    odby = [];
    if (!txt) txt = "";
    let groups = [];
    let tday = new Date();
    txt = txt.replace(/==/igm, "=");
    txt = txt.replace(/(([^\s]|(và\s*)|(hoặc\s))(có)|(^có))\s*\"*([^\"]+)\"/gmi, '$2$7=IS NOT NULL');
    txt = txt.replace(/(([^\s]|(và\s*)|(hoặc\s))(không có)|(^không có))\s*\"*([^\"]+)\"/gmi, '$2$7=IS NULL');
    txt = txt.replace(/\s+(có|ở)\s+\"?([^\"]+)\"/igm, "=%$2%");
    txt = txt.replace(/\s+có\s+([^\s]+)/igm, "=%$1%");
    txt = txt.replace(/\s+bắt đầu bằng\s+\"?([^\"]+)\"/igm, "=$1%");
    txt = txt.replace(/\s+bắt đầu bằng\s+([^\s]+)/igm, "=$1%");
    txt = txt.replace(/\s+kết thúc bằng\s+\"?([^\"]+)\"/igm, "=%$1");
    txt = txt.replace(/\s+kết thúc bằng\s+([^\s]+)/igm, "=%$1");
    txt = txt.replace(/(lớn hơn)/igm, ">");
    txt = txt.replace(/(lớn hơn hoặc bằng)/igm, ">=");
    txt = txt.replace(/(nhỏ hơn hoặc bằng)/igm, "<=");
    txt = txt.replace(/(nhỏ hơn)/igm, "<=");
    txt = txt.replace(/( khác )/igm, "<>");
    txt = txt.replace(" bằng ", "=");
    txt = txt.replace(/(tên)\s*(nhân sự)?\s*(là)\s*([^\s]+)/igm, (a) => {
        let arr = a.split("là");
        return `tên nhân sự=${arr[1].split(",").map(x => '%' + x).join(",")}`;
    })
    txt = txt.replace(/(tên đệm)\s*(là)\s*([^\s]+)/igm, (a) => {
        let arr = a.split("là");
        return `tên nhân sự=${arr[1].split(",").map(x => '% ' + x + ' %').join(",")}`;
    })
    txt = txt.replace(/(họ)\s*(là)\s*([^\s]+)/igm, (a) => {
        let arr = a.split("là");
        return `tên nhân sự=${arr[1].split(",").map(x => x.trim() + ' %').join(",")}`;
    })
    //txt = txt.replace(/(học)\s*\"([^\"]+)\"\s+(từ năm)\s+(\d+)/igm, "Đào tạo=%\"%$2%$4%\"%");
    txt = txt.replace(/(học trường)\s*\"([^\"]+)\"/igm, "Đào tạo=%$2%");
    txt = txt.replace(/(chuyên ngành)\s*\"([^\"]+)\"/igm, "Đào tạo=%chuyên ngành:%$2%");
    txt = txt.replace(/(Trình độ)\s*\"([^\"]+)\"/igm, "Đào tạo=%trình độ:%$2%");
    txt = txt.replace(/(phòng ban)\s*\"([^\"]+)\"/igm, "Phòng ban=%$2%");
    txt = txt.replace(/(phòng ban)\s+([^\s]+)/igm, 'Phòng ban=%$2%');
    txt = txt.replace(/(tuổi)\s*\s*(\d+)/igm, 'tuổi=$2');
    txt = txt.replace(/(từ|=)\s*(\d+)\s*(đến|-)\s*(\d+)/igm, '# $2 A-N-D $4');
    txt = txt.replace(/(từ)\s*(\d+)/igm, ">=$2");
    txt = txt.replace(/(trên)\s*(\d+)/igm, ">$2");
    txt = txt.replace(/(đến)\s*(\d+)/igm, "<=$2");
    txt = txt.replace(/(dưới)\s*(\d+)/igm, "<$2");
    txt = txt.replace(/sinh năm/igm, "năm sinh");
    txt = txt.replace(/(sinh năm|năm sinh)\s*(\d{4})/igm, "năm sinh =$2");
    txt = txt.replace(/(giới tính)\s*(là)?\s*([^=\s]+)/igm, "giới tính =$3");
    txt = txt.replace(/(sinh nhật\s*(hôm nay)?)/igm, `ngày sinh =${tday.getDate() < 10 ? '0' : ''}${tday.getDate()}/${(tday.getMonth() + 1) < 10 ? '0' : ''}${tday.getMonth() + 1}%`)
    txt = txt.replace(/(sinh\s*(nhật)?\s*tháng)\s*(\d{1,2})\/?(\d{4})?/igm, function (match, p1) {
        let p2 = match.replace(p1, "").trim();
        if (p2.includes("/")) {
            return `ngày sinh =%/${p2}`;
        }
        p2 = (p2.length == 1 ? '0' : '') + p2;
        p2 = p2.replace("00", "0");
        return `ngày sinh =%/${p2}/%`;
    })
    txt = txt.replace(/(sinh\s*ngày)\s*(\d{1,2})\/?(\d{0,2})?\/?(\d{0,4})/igm, function (match, p1) {
        let p2 = match.replace(p1, "").trim();
        let arrp2 = p2.split("/");
        if (arrp2.length == 3) {
            p2 = p2.split("/").map(x => x.trim().length == 1 ? "0" + x : x).join("/");
            if (p2[2].length < 4) {
                p2 = p2 + "%";
            }
            return `ngày sinh =${p2}`;
        }
        return `ngày sinh =${arrp2.map(x => x.trim().length == 1 ? "0" + x : x).join("/")}/%`;
    });
    txt = txt.replace(/(sinh\s*năm)\s*(\d{1,4})/igm, `ngày sinh =%/$2`);
    txt = txt.replaceAll("và", "&").replaceAll("hoặc", "@");
    let strjoin = /\(.+\)\s*@\s*\(.+\)/igm.test(txt) ? 'OR' : 'AND';
    if (!txt.startsWith("(")) {
        txt += `(${txt})`;
    }
    let match = txt.match(/\([^)]+\)/igm);
    if (match) {
        match.forEach(x => {
            x = x.replace(/([^\(&@#]+)(=|>=|<=|<>|<|>|#)/igm, (s, r) => {
                let i = r.indexOf(">");
                let stt = ">";
                if (i == -1) {
                    i = r.indexOf("<");
                    stt = "<";
                }
                if (i == -1) {
                    i = r.length;
                    stt = "";
                }
                let k = r.substring(0, i).trim().toLowerCase();
                let ken = change_unsigned(k).toLowerCase().trim();
                let objk = cols.value.find(x => x.title.toLowerCase() == k || x.titleen.toLowerCase() == k || x.titleen.toLowerCase() == ken);
                if (objk) {
                    k = objk.key;
                    if (odby.findIndex(o => o == '[' + k + ']') == -1) {
                        odby.push('[' + k + ']');
                    }
                    if (selectedCols.value.findIndex(a => a.key == k) == -1) {
                        selectedCols.value.push(objk);
                    }
                }
                s = s.replace(r, ` [${k}]${stt} `);
                return s;
            });
            groups.push(x);
        })
    }
    let rs = groups.join(` ${strjoin} `);
    rs = rs.replace(/(>=|<=|=|<>|#)([^(=|>=|<=|<>|<|>|&|@|#)]+)/igm, " $1 N'$2' ");
    rs = rs.replaceAll(' & ', ' AND ');
    rs = rs.replaceAll(' @ ', ' OR ');
    rs = rs.replaceAll("= N'%", " LIKE N'%");
    rs = rs.replace(/(\[[^\]]+\])[^\]]+%[^\']+'/igm, (s, r) => {
        if (!s.includes("LIKE N")) return s;
        s = s.replaceAll(" '", "");
        let arr = s.split("LIKE N");
        let str = "(";
        arr[1].replaceAll("'", "").split(",").forEach((x, i) => {
            if (i > 0) {
                str += " OR ";
            }
            str += ` ${arr[0]} LIKE N'${x}' `;
        });
        str += ")"
        return str;
    })
    rs = rs.replace(/=( N'[^']+\s*%\s*')/igm, "LIKE $1");
    rs = rs.replace(/%\s*'/igm, "%'");
    rs = rs.replaceAll("#", "BETWEEN");
    rs = rs.replace(/A-N-D/igm, "AND");
    rs = rs.replace(/= N'IS NOT NULL\s*'/igm, "IS NOT NULL");
    rs = rs.replace(/= N'IS NULL\s*'/igm, "IS NULL");
    rs = rs.replace(/N'\s*(\d+)\s*AND\s*(\d+)\s*'/igm, '$1 AND $2');
    rs = rs.replace(/\s{2}/igm, " ");
    rs = rs.replace(/\(\s+/igm, "(");
    //For json
    if (/(học)\s*\"([^\"]+)\"\s+(từ)?\s?năm\s+(\d+)/igm.test(rs)) {
        let col = cols.value.find(x => x.key == "Hồ sơ nhân sự|Đào tạo|5");
        if (selectedCols.value.findIndex(a => a.key == col.key) == -1) {
            selectedCols.value.push(col);
        }
        rs = rs.replace(/(học)\s*\"([^\"]+)\"\s+(từ)?\s?năm\s+(\d+)/igm, `
        ve.profile_id in(select distinct profile_id from View_SearchEngine cross apply OPENJSON([Hồ sơ nhân sự|Đào tạo|5]) where JSON_VALUE(value,'$.json') like N'%$2%$4%')
    `);
    }
    // rs = rs.replace(/(học trường)\s*\"([^\"]+)\"/igm, `
    //     profile_id in(select distinct profile_id from View_SearchEngine cross apply OPENJSON([Hồ sơ nhân sự|Đào tạo|5]) where JSON_VALUE(value,'$.json') like N'%$2%')
    // `);
    //convert(datetime,$date,103)
    rs = rs.replace(/(\[[^\[^\]]*(Ngày)[^\[^\]]*\])\s*(>|<|(BETWEEN))\s*([^\s)']*)/gmi, "convert(datetime,$1,103) $3 '$5'");
    rs = rs.replace(/''/igm, "'");
    return rs;
}
let cacheSQL = '';
const goSearch = async (swhere) => {
    swal.fire({
        width: 110,
        didOpen: () => {
            swal.showLoading();
        },
    });
    if (showListComplete.value == true) showListComplete.value = false;
    //let strSelect = ` Select `;
    let strFrom = ` from ${viewDB} `;
    let strWhere = '';
    strWhere = swhere != null ? swhere : renderAutoInput(ipsearch.value);
    let selectColumn = selectedCols.value.map(x => '[' + x.key + ']').join(",");
    let strOrderby = ` order by ${odby.length > 0 ? odby.join(",") : '[' + cols.value[0].key + ']'} `;
    if (strWhere != "") {
        strWhere = " Where " + strWhere;
        //strWhere = " AND " + strWhere;
    }
    let strJoin = '';
    strOrderby = ` order by ve.profile_id`;
    let arrdbview = Array.from(new Set(selectedCols.value.filter(x => x.dbview).map(x => x.dbview)));
    arrdbview.forEach(x => {
        strJoin += ` left join ${x} on ${x}.profile_id=ve.profile_id `
        strOrderby += `,${x}.is_order`;
    })
    let arr = selectedCols.value.filter(x => !x.dbview).map(x => x.key);
    groupRowsBy.value = [arr[0], arr[1]];
    let uid = store.getters.user.user_id;
    strFrom = `
        select profile_id into #tbv from ViewProfilePermission where user_id='${uid}'
        Select ${selectColumn} from ${viewDB} ve inner join hrm_profile p on ve.profile_id=p.profile_id
        inner join #tbv on #tbv.profile_id=p.profile_id 
        left JOIN hrm_profile_assignment ct on p.profile_id = ct.profile_id and ct.is_active=1
        left join sys_organization o on o.organization_id=p.organization_id
        left join sys_organization op on op.organization_id=ct.organization_id
        ${strJoin}
    `
    let sql = `${strFrom} ${strWhere} ${strOrderby} drop2table #tbv`;
    //let sql = `${strSelect} ${selectColumn} ${strFrom} ${strWhere} ${strOrderby}`;
    sql = sql.replace(/\s{2}/igm, " ");
    sql = sql.replace(/\(\s+/igm, "(");
    if (cacheSQL != sql || opMic.value.isshow == false) {
        cacheSQL = sql;
        await initData(false, sql);
    }
    swal.close();
}
const execSQL = async (strSQL) => {
    const axResponse = await axios
        .post(
            baseUrlCheck + "api/SRC/PostProc",
            {
                str: encr(JSON.stringify(strSQL), SecretKey, cryoptojs).toString(),
            },
            config
        );
    return axResponse;
}
const initData = async (f, sql) => {
    startnumber.value = 0;
    datas.value = [];
    let strSQL = {
        "query": true,
        "proc": `Select Top 1  * from ${viewDB}Column`,

    };
    if (cols.value.length > 0) {
        strSQL.proc += ` order by [${cols.value[0].key}]`;
    }
    if (sql) {
        strSQL.proc = sql.replace(/\s{3,}/igm, " ");
        console.log(strSQL.proc);
    }
    swal.fire({
        width: 110,
        didOpen: () => {
            swal.showLoading();
        },
    });
    try {
        const axResponse = await execSQL(strSQL);
        if (axResponse.status == 200) {
            if (axResponse.data.error) {
            } else {
                let dts = JSON.parse(axResponse.data.data);
                if (dts[0].length > 0) {
                    if (!f) {
                        selectedCols.value.filter(x => x.typedata == 2).forEach(k => {
                            dts[0].forEach(r => {
                                if (r[k.key]) {
                                    try {
                                        var newData = r[k.key].toString().replace(/(\d+[/])(\d+[/])/, '$2$1');
                                        r[k.key] = new Date(newData);
                                    } catch (e) {
                                        console.log(e);
                                    }
                                }
                            });
                        })
                        datas.value = dts[0];
                    }
                    else {
                        let objkey = dts[0][0];
                        let objGroup = {};
                        let keys = Object.keys(objkey);
                        let grs = [];
                        keys.filter(x => x.includes("View_")).forEach(k => {
                            objGroup[objkey[k]] = k;
                        });
                        keys.filter(x => !x.includes("View_")).forEach(k => {
                            expandedKeys.value[k.split("|")[0]] = true;
                            let o = {
                                key: k,
                                group: objkey[k].split("|")[0],
                                title: objkey[k].split("|")[1],
                                label: objkey[k].split("|")[1],
                                typedata: objkey[k].split("|")[2],
                                AND: true,
                                type: "=",
                                show: objkey[k].split("|")[3] == 1,
                                frozen: objkey[k].split("|")[4] == 1,
                            };
                            o.dbview = objGroup[o.group];
                            if (o.typedata == 2 || o.typedat == 3) {
                                o.typedate = "convert(datetime,$date,103)";
                            }
                            if (o.title)
                                o.titleen = change_unsigned(o.title);
                            cols.value.push(o)
                            let obj = grs.find(x => x.label == objkey[k].split("|")[0]);
                            if (obj) {
                                obj.col += 1;
                                obj.items.push(o);
                                obj.children.push(o);
                            } else {
                                grs.push({ key: objkey[k].split("|")[0], label: objkey[k].split("|")[0], col: 1, items: [o], children: [o] });
                            }
                        });
                        colgroups.value = grs;
                        if (!selectedCols.value)
                            selectedCols.value = cols.value.filter(x => x.show);
                        swal.close();
                    }
                }
            }
        }
    } catch (e) {
        console.log(e);
    }
    swal.close();
}
const renderdrTypes = (dt) => {
    return drTypes.value.filter(x => !x.types || x.types.includes("," + dt.typedata + ","));
}
const op = ref();
const opHelp = ref();
const openHelp = (event) => {
    opHelp.value.toggle(event);
}
const openFilter = (event) => {
    op.value.toggle(event);
}
const submitFilter = () => {
    op.value.hide();
    ipsearch.value = "";
    let strWhere = '';
    let notHasValueFilter = false;
    groupBlock.value.forEach(g => {
        if (strWhere != "") {
            strWhere += ` ${AND ? " AND " : " OR "}`;
        }
        strWhere += "(";
        g.datas.forEach((gx, kg) => {
            if (selectedCols.value.findIndex(a => a.key == gx.key) == -1) {
                selectedCols.value.push(gx);
            }
            if (strWhere != "" && kg > 0) {
                strWhere += ` ${g.AND ? " AND " : " OR "}`;
            }
            strWhere += "(";
            gx.childs.forEach((x, ix) => {
                if (strWhere != "" && ix > 0) {
                    strWhere += ` ${gx.AND ? " AND " : " OR "} (`;
                }
                if ((arrConditionRequied.value.includes(x.type) || (arrTypeRequied.value.includes(x.typedata) && x.type == "=")) 
                    && (x.value == null || x.value.trim() == "")) {
                    notHasValueFilter = true;
                }
                switch (x.type) {
                    case "FromTo":
                        strWhere += "(";
                        x.value.split(",").forEach((vl, i) => {
                            strWhere += ` ${i != 0 ? "OR" : ""} ${x.typedate ? x.typedate.replace("$date", "[" + x.key + "]") : "[" + x.key + "]"} BETWEEN ${vl.replace("-", " AND ")}`;
                        });
                        strWhere += ")";
                        ipsearch.value += `${ipsearch.value == "" ? "" : (g.AND ? ' và ' : ' hoặc ')}"${x.title}" trong khoảng ${x.value}`
                        break;
                    case "Contain":
                        strWhere += "(";
                        x.value.split(",").forEach((vl, i) => {
                            strWhere += ` ${i != 0 ? "OR" : ""} [${x.key}] like N'%${vl}%'`;
                        });
                        strWhere += ")";
                        ipsearch.value += `${ipsearch.value == "" ? "" : (g.AND ? ' và ' : ' hoặc ')}"${x.title}" có chữ "${x.value || ''}"`
                        break;
                    case "StartWith":
                        strWhere += "(";
                        x.value.split(",").forEach((vl, i) => {
                            strWhere += ` ${i != 0 ? "OR" : ""} [${x.key}] like N'${vl}%'`;
                        });
                        strWhere += ")";
                        ipsearch.value += `${ipsearch.value == "" ? "" : (g.AND ? ' và ' : ' hoặc ')}"${x.title}" bắt đầu bằng chữ "${x.value || ''}"`
                        break;
                    case "EndWith":
                        strWhere += "(";
                        x.value.split(",").forEach((vl, i) => {
                            strWhere += ` ${i != 0 ? "OR" : ""} [${x.key}] like N'%${vl}'`;
                        });
                        strWhere += ")";
                        ipsearch.value += `${ipsearch.value == "" ? "" : (g.AND ? ' và ' : ' hoặc ')}"${x.title}" kết thúc bằng chữ "${x.value || ''}"`
                        break;
                    default:
                        strWhere += "(";
                        (x.value || "").split(",").forEach((vl, i) => {
                            strWhere += ` ${i != 0 ? "OR" : ""} ${x.typedate ? x.typedate.replace("$date", "[" + x.key + "]") : "[" + x.key + "]"} ${x.type} ${vl ? `N'${vl}'` : ""}`;
                        });
                        strWhere += ")";
                        ipsearch.value += `${ipsearch.value == "" ? "" : (g.AND ? ' và ' : ' hoặc ')}`;
                        if (x.type.trim() == "IS NULL") {
                            ipsearch.value += ` không có "${x.title}"`;
                        } else if (x.type.trim() == "IS NOT NULL") {
                            ipsearch.value += ` có "${x.title}"`;
                        } else {
                            ipsearch.value += ` "${x.title}" ${x.type} "${x.value || ''}"`;
                        }
                        break;
                }
                if (strWhere != "" && ix > 0) {
                    strWhere += ")";
                }
            });
            strWhere += ")";
        });
        strWhere += ")";
    });
    if (notHasValueFilter) {
        swal.fire({
            title: "Thông báo!",
            text: "Nhập giá trị của điều kiện tìm kiếm có dấu *",
            icon: "error",
            confirmButtonText: "OK",
        });
        return;
    }
    goSearch(strWhere);
}
const delFilter = (idx, rows) => {
    rows.splice(idx, 1);
}
const addFilter = (no) => {
    no.childs.push({ ...no });
}
const refresh = () => {
    ipsearch.value = "";
    goSearch();
}
function PrintDiv() {
    dgPrint.value = false;
    let title = "Báo cáo tổng hợp nhân sự";
    var contents = getHTMLTable(false);
    var frame1 = document.createElement('iframe');
    frame1.name = "frame1";
    frame1.style.position = "absolute";
    frame1.style.top = "-1000000px";
    document.body.appendChild(frame1);
    var frameDoc = frame1.contentWindow ? frame1.contentWindow : frame1.contentDocument.document ? frame1.contentDocument.document : frame1.contentDocument;
    frameDoc.document.open();
    frameDoc.document.write(`<html><head><title>${title}</title>`);
    frameDoc.document.write('</head><body>');
    frameDoc.document.write(contents);
    frameDoc.document.write('</body></html>');
    //frameDoc.document.close();
    setTimeout(function () {
        window.frames["frame1"].focus();
        window.frames["frame1"].print();
        document.body.removeChild(frame1);
    }, 500);
    return false;
}
function isDate(str) {
    if (!str) return false;
    var t = str.toString().match(/^(\d{2})\/(\d{2})\/(\d{4})$/);
    if (t === null)
        return false;
    var d = +t[1], m = +t[2], y = +t[3];

    // Below should be a more acurate algorithm
    if (m >= 1 && m <= 12 && d >= 1 && d <= 31) {
        return true;
    }

    return false;
}
var groupBy = function (xs, key) {
    return xs.reduce(function (rv, x) {
        (rv[x[key]] = rv[x[key]] || []).push(x);
        return rv;
    }, {});
};
function bindRow(r, td, rp) {
    let trs = "";
    if (!r[td.key]) {
        trs += `<td rowspan=${rp}></td>`
    } else {
        if (r[td.key].toString().includes("Portals"))
            trs += `<td rowspan=${rp}><img width=48 src="${imgurl + r[td.key]}"></td>`
        else if (td.typedata == 5) {
            let objr = JSON.parse(r[td.key]);
            trs += `<td rowspan=${rp}>`
            objr.forEach(rs => {
                trs += `<p>${rs.json}</p>`;
            });
            trs += "</td>"
        } else if (td.typedata == 4) {
            trs += `<td style="text-align:center" rowspan=${rp}>${r[td.key] ? 'X' : ''} </td>`
        }
        else if (td.typedata == 2 || td.typedata == 3) {
            trs += `<td rowspan=${rp} class='date' style='text-align:center'>`;
            if (r[td.key]) {
                try {
                    trs += r[td.key].toLocaleString("vi-VN", {
                        year: 'numeric',
                        month: '2-digit', day: '2-digit'
                    })
                } catch (e) { }
            }
            trs += "</td>"
        }
        else {
            let str = r[td.key];
            trs += `<td rowspan=${rp} class="text">${str} </td>`
        }
    }
    return trs;
}
const getHTMLTable = (f) => {
    let titlebaocao = `
        <table style="border:none;">
            <tr style="border:none"><td style="border:none;text-align:center" colspan=${selectedCols.value.length + 1}>${hTieude.value}</td></tr>
        </table>
    `;
    let divid = "dtData";
    let colgroup = "<colgroup>"
    let widths = [];
    document.getElementById(divid).querySelectorAll("table>thead>tr:last-child>th").forEach(th => {
        colgroup += `<col style="width:${th.offsetWidth}px;padding:5pt">`;
        widths.push(th.offsetWidth);
    })
    colgroup += "</colgroup>"
    let trs = "";
    trs += "<thead>"
    trs += "<tr>"
    trs += `<th style='text-align:center;width:${widths[0]}px'>STT</th>`
    selectedCols.value.forEach((c, i) => {
        trs += `<th class="text" style='width:${widths[i + 1]}px;text-align:${c.typedata == 2 || c.typedata == 3 ? "center" : "left"}'>${c.title}</th>`
    })
    trs += "</tr>"
    trs += "</thead>"
    trs += "<tbody>"
    if (selectedCols.value.findIndex(x => x.dbview) != -1
    ) {
        let grs = (groupBy(datas.value, selectedCols.value[0].key));
        Object.keys(grs).forEach((k, i) => {
            trs += "<tr>"
            let r = grs[k][0];
            let isrspan = grs[k].length > 1;
            let rosp = grs[k].length;
            trs += `<td rowspan=${rosp} style='text-align:center;'>${i + 1}</td>`
            selectedCols.value.forEach(td => {
                trs += bindRow(r, td, td.dbview ? 1 : rosp);
            });
            trs += "</tr>"
            if (isrspan) {
                trs += "</tr>"
                grs[k].forEach((r, j) => {
                    if (j > 0) {
                        trs += "<tr>"
                        selectedCols.value.filter(x => x.dbview
                        ).forEach(td => {
                            trs += bindRow(r, td, 1, j);
                        });
                        trs += "</tr>"
                    }
                })
            }
        })
    }
    else {
        datas.value.forEach((r, i) => {
            trs += "<tr>"
            trs += `<td style='text-align:center;'>${i + 1}</td>`
            selectedCols.value.forEach(td => {
                trs += bindRow(r, td, 1);
            });
            trs += "</tr>"
        })
    }
    trs += "</tbody>"
    let style = `
    body{font-family: "Times New Roman";font-size:14pt}
    table { 
        border-spacing: 0;
        border-collapse: collapse;
        width:100%;
    }
    th,td{border:1px solid #999;padding:5pt;white-space: nowrap;}
    thead th {background-color: #e6e6e6!important;min-height:40px;text-align: left;}
    .num {mso-number-format:General;}
    .text{mso-number-format:"\@";}
    .date{mso-number-format:"Short Date";text-align:center}
    @page{
            margin:1.27cm;
            mso-paper-source:0;
            size: 'landscape';
        }
        table {page-break-inside: auto;}
        tr {page-break-inside: avoid;page-break-after: auto;}
        thead {display: table-header-group;}
        tfoot {
            display: table-footer-group;
        }
    `;
    let html = "<style>" + style + "</style>" + titlebaocao + "<table>" + colgroup + trs + "</table>";
    return html;
}
const downloadFile = async () => {
    // var wnd = window.open("about:blank", "", "_blank");
    // wnd.document.write(getHTMLTable(true));
    // return false;
    let filename = hTieude.value.replace(/<\/?[^>]*>|<[^>]+>/ig, '');//fileExport.value;
    dgPrint.value = false;
    let dataHtml = { html: getHTMLTable(true), filename: filename || "file_xlsx" };
    swal.fire({
        width: 110,
        didOpen: () => {
            swal.showLoading();
        },
    });
    try {
        const axResponse = await axios
            .post(
                baseUrlCheck + "api/SRC/ConvertFileXLSX",
                dataHtml,
                config
            );

        if (axResponse.status == 200) {
            //window.open(apiURL + "api/Files/downloadFileXLS" + "?name=" + filename + '.xlsx');
            downloadFileExport("GetDownloadXLS", dataHtml.filename, axResponse.data.fileName, ".xlsx");
        }
    } catch (e) {
        console.log(e);
    }
    swal.close();
}
const downloadFileExport = (name_func, file_name_download, file_name, file_type) => {
    let nameF = (file_name || "file_download") + file_type;
    let nameDownload = (file_name_download || "file_download") + file_type;
    const a = document.createElement("a");
    a.href = baseUrlCheck + "/api/SRC/" + name_func + "?name=" + nameF;
    a.download = nameDownload;
    a.target = "_blank";
    a.click();
    a.remove();
};
const onPage = (event) => {
    startnumber.value = event.page * event.rows;
};
const startnumber = ref(0);
const onNodeSelect = (node) => {
    if (groupBlock.value[blockindex.value].datas.findIndex(x => x.key == node.key) == -1) {
        let obj = {
            key: node.key,
            title: node.title,
            AND: node.AND,
            type: node.type,
            typedata: node.typedata,
        };
        if (obj.typedata == 2 || obj.typedata == 3) {
            obj.typedate = node.typedate
        }
        if (node.children) {
            node.children.filter(!x.childs).forEach(x => {
                x.childs = [obj]
            });
            groupBlock.value[blockindex.value].datas = groupBlock.value[blockindex.value].datas.concat(node.children);
        } else {
            node.childs = [obj]
            groupBlock.value[blockindex.value].datas.push(node);
        }
        expandedRows.value.push(node);
    }
};

const onNodeUnselect = (node) => {
    let idx = groupBlock.value[blockindex.value].datas.findIndex(x => x.key == node.key);
    if (idx != -1) {
        groupBlock.value[blockindex.value].datas.splice(idx, 1);
    } else if (node.children) {
        groupBlock.value[blockindex.value].datas = groupBlock.value[blockindex.value].datas.filter(x => node.children.findIndex(a => a.key == x.key) == -1);
    }
};
const dgSort = ref(false);
const openSort = () => {
    dgSort.value = true;
}
const delCols = (idx) => {
    selectedCols.value.splice(idx, 1);
}
const resizeTable = ref(false);
const expandedRows = ref([]);
const blockindex = ref(0);
const groupBlock = ref([
    {
        stt: 1,
        AND: true,
        datas: []
    }
]);
const addBlock = () => {
    groupBlock.value.push({
        stt: groupBlock.value.length + 1,
        AND: true,
        datas: []
    });
}
const delBlock = (i) => {
    groupBlock.value.splice(i, 1);
}
const AND = ref(true);
const selectedIP = ref("");
const imgurl = baseURL;
const showListComplete = ref(false);
const selectListComplete = () => {
    console.log(selectedIP.value);
    let txt = ipsearch.value || "";
    let idx = txt.lastIndexOf(" ");
    if (idx != -1) {
        txt = txt.substring(0, idx) + " " + selectedIP.value.title;
    } else {
        txt = selectedIP.value.title;
    }
    ipsearch.value = txt;
    document.getElementById("ipsearch").focus();
    showListComplete.value = true;
}
const onBlur = () => {
    if (!listfocus.value) {
        setTimeout(() => {
            showListComplete.value = false;
        }, 100);
    }
}
const drHelps =
{
    label: "Mẫu tìm kiếm",
    items: [
        { title: "Tên là Cường,Đức" },
        { title: "Họ là Nguyễn" },
        { title: "Tên đệm là Đức,Hồng" },
        { title: "Tuổi 20" },
        { title: "Tuổi = 20" },
        { title: "Tuổi >=20" },
        { title: "Tuổi từ 20" },
        { title: "Tuổi 20-25" },
        { title: "Tuổi từ 20 đến 25" },
        { title: "Tuổi <45" },
        { title: "Tuổi đến 45" },
        { title: "Giới tính nam" },
        { title: "Giới tính nữ" },
        { title: "Giới tính khác" },
        { title: "Sinh nhật hôm nay" },
        { title: "Sinh tháng 12" },
        { title: "Sinh năm 1988" },
        { title: 'Quê quán ở "Hà Nội"' },
        { title: 'Học trường "Bách khoa"' },
        { title: 'Học "Bách khoa" năm 2004' },
        { title: 'Trình độ "tiến sĩ"' },
    ]
};
drHelps.items.forEach((x) => {
    x.titleen = change_unsigned(x.title);
})
const compComplete = computed(() => {
    let txt = ipsearch.value;
    if (!txt) return [drHelps].concat(colgroups.value);
    txt = txt.substring(txt.lastIndexOf(" ")).trim();
    let drcols = cols.value.filter(x => x.title.toLowerCase().includes(txt) || x.titleen.toLowerCase().includes(txt));
    let drcolshelps = drHelps.items.filter(x => x.title.toLowerCase().includes(txt) || x.titleen.toLowerCase().includes(txt));
    let arr = [];
    if (drcolshelps.length > 0) arr.push({ label: "Mẫu tìm kiếm", items: drcolshelps });
    if (drcols.length > 0) arr.push({ label: "Kết quả tìm kiếm", items: drcols });
    if (arr.length == 0) return [];
    return arr;
});
const listfocus = ref(false);
const goDown = () => {
    document.getElementById("listComp").focus();
    showListComplete.value = true;
    listfocus.value = true;
}
const groupRowsBy = ref();
const selectedData = ref();
const dgPrint = ref(false);
const fileExport = ref();
const hTieude = ref("<h2 style='text-align:center'>BÁO CÁO TỔNG HỢP NHÂN SỰ</h2>");
const opendgPrint = () => {
    dgPrint.value = true;
    hTieude.value = `<h2 style='text-align:center'>BÁO CÁO TỔNG HỢP NHÂN SỰ (${datas.value.length})</h2>`;
}
const changeGroup = (gr) => {
    if (gr.checked) {
        selectedCols.value = selectedCols.value.concat(gr.items)
    } else {
        selectedCols.value = selectedCols.value.filter(x => gr.items.findIndex(a => a.key == x.key) == -1);
    }
}
const showSidebaBaocao = ref(false);
let dtBacaos = ref([]);
let mBaocao = ref({});
const openListBaocao = () => {
    showSidebaBaocao.value = true;
}
const delBaocao = (idx) => {
    showSidebaBaocao.value = false;
    dtBacaos.value.splice(idx, 1);
    localStorage.setItem("report", JSON.stringify(dtBacaos.value));
    toast.success("Xoá báo cáo thành công!");
}
const saveBaocao = () => {
    showSidebaBaocao.value = false;
    dgPrint.value = false;
    let idx = dtBacaos.value.findIndex(x => x.id == mBaocao.value.id);
    if (mBaocao.value.id==-1 ||idx == -1) {
        mBaocao.value.id = uuidv4()||(new Date()).getTime();
        mBaocao.value.date = new Date().toLocaleString("vi-VN");
        mBaocao.value.hTieude = hTieude.value;
        mBaocao.value.name = hTieude.value.replace(/<\/?[^>]*>|<[^>]+>/ig, '');
        mBaocao.value.fileExport = mBaocao.value.name;
        mBaocao.value.cacheSQL = cacheSQL;
        mBaocao.value.search = ipsearch.value;
        mBaocao.value.selectedCols = JSON.stringify(selectedCols.value);
        dtBacaos.value.push(mBaocao.value);
    }
    localStorage.setItem("report", JSON.stringify(dtBacaos.value));
    if(idx==-1){
        mBaocao.value.id = -1;
    }
    toast.success("Lưu báo cáo thành công!");
}
const goBaocao = (rp) => {
    cacheSQL = "";
    mBaocao.value = rp;
    hTieude.value = rp.value;
    hTieude.fileExport = rp.fileExport;
    ipsearch.value = rp.ipsearch;
    selectedCols.value = JSON.parse(rp.selectedCols);
    initData(false, rp.cacheSQL);
    showSidebaBaocao.value = false;
}
const goBack = () => {
  history.back();
};
const rowClassTc = () => {
  return "row-tbl-tc";
};
const rowClassDk = () => {
  return "row-tbl-dk";
};
const arrTypeRequied = ref(['1', '2', '3', '4', '5', '6']);
const arrConditionRequied = ref([">", ">=", "<", "<=", "<>", "FromTo"]);
onMounted(() => {
    initData(true);
    if (localStorage.getItem("report")) {
        dtBacaos.value = JSON.parse(localStorage.getItem("report"));
    }
    return {
        delBaocao,
        dtBacaos,
        mBaocao,
        goBaocao,
        showSidebaBaocao,
        openListBaocao,
        saveBaocao,
        groupRowsBy,
        selectedData,
        fileExport,
        dgPrint,
        hTieude,
        opendgPrint,
        listfocus,
        renderdrTypes,
        AND,
        blockindex,
        groupBlock,
        addBlock,
        delBlock,
        expandedRows,
        resizeTable,
        opHelp,
        openHelp,
        onNodeSelect,
        onNodeUnselect,
        datas,
        cols,
        colgroups,
        ipsearch,
        goMic,
        opMic,
        startMic,
        stopMic,
        goSearch,
        openFilter,
        imgurl,
        selectedCols,
        op,
        drTypes,
        submitFilter,
        delFilter,
        addFilter,
        refresh,
        itemButExports,
        downloadFile,
        PrintDiv,
        onPage,
        startnumber,
        selectedKey,
        expandedKeys,
        openSort,
        dgSort,
        delCols,
        drTypeDate,
        compComplete,
        showListComplete,
        selectListComplete,
        onBlur,
        selectedIP,
        goDown,
        changeGroup,
    };
});

</script>
<template>
    <div class="flex flex-column h-full p-2">
        <div class="py-3">
            <div class="bg-white format-center py-1 font-bold text-xl">
                BÁO CÁO TỔNG HỢP NHÂN SỰ <span class="pl-2" v-if="datas.length">({{ datas.length }})</span>
            </div>
            <Toolbar class="w-full custoolbar">
                <template #start>
                    <div class="flex flex-wrap align-items-center justify-content-between gap-2">
                        <div class="flex-1">
                            <span class="p-input-icon-right w-full">
                                <i class="pi pi-search"></i>
                                <InputText spellcheck="false" id="ipsearch" @keydown.tab="goDown()" @keydown.enter="goSearch()"
                                    @input="showListComplete = true" @focus="showListComplete = true" @blur="onBlur"
                                    v-model="ipsearch" placeholder="Nhập nội dung tìm kiếm" />
                                <div v-if="showListComplete && compComplete.length > 0"
                                    style="position: absolute;z-index: 999;margin-top: 10px;">
                                    <Listbox @focus="listfocus = true" @blur="listfocus = false" id="listComp"
                                        @change="selectListComplete" v-model="selectedIP" :options="compComplete"
                                        optionLabel="title" optionGroupLabel="label" optionGroupChildren="items"
                                        listStyle="max-height:360px;">
                                    </Listbox>
                                </div>
                            </span>
                        </div>
                        <div>
                            <Button v-tooltip.top="'Tìm kiếm thông minh'" @click="openFilter" icon="pi pi-filter" class="mr-2 p-button-outlined p-button-secondary" />
                            <Button v-tooltip.top="'Thực hiện'" @click="goSearch()" icon="pi pi-send" class="mr-2 p-button-outlined p-button-secondary" />
                            <!-- <Button
                                @click="goMic()"
                                v-tooltip.top="'Tìm kiếm bằng giọng nói'"
                                class="mr-2 p-button-outlined p-button-secondary search-microphone"
                                style="padding: 0.65rem 0.75rem 0.6rem;"        
                            >
                                <font-awesome-icon icon="fa-solid fa-microphone" 
                                    style="font-size:1rem; display: block; color: #607d8b"
                                />
                            </Button> -->
                            <Button v-tooltip.top="'Hướng dẫn'" @click="openHelp" icon="pi pi-info-circle" class="mr-2 p-button-outlined p-button-secondary" />                        
                        </div>
                        
                    </div>
                </template>
                <template #end>
                    <div>
                        <Button label="Quay lại" icon="pi pi-arrow-left" 
                            class="p-button-outlined mr-2 p-button-secondary" @click="goBack()" />
                        <Button v-tooltip.top="'Tải lại'" @click="refresh" icon="pi pi-refresh"
                            class="mr-2 p-button-outlined p-button-secondary" />
                        <MultiSelect @change="cacheSQL = ''" scrollHeight="500px" :selectedItemsLabel="'{0} cột hiển thị'"
                            :maxSelectedLabels="3" filter v-model="selectedCols" :options="colgroups" optionLabel="title"
                            optionGroupLabel="label" optionGroupChildren="items" placeholder="Chọn cột hiển thị"
                            class="mr-2"
                        >
                            <template #optiongroup="slotProps">
                                <div class="flex align-items-center">
                                    <Checkbox :inputId="slotProps.option.key" @change="changeGroup(slotProps.option)"
                                        v-model="slotProps.option.checked" :binary="true" />
                                    <label :for="slotProps.option.key"><b class="ml-2">{{ slotProps.option.label
                                    }}</b></label>
                                </div>
                            </template>
                        </MultiSelect>
                        <Button v-tooltip.top="'Sắp xếp cột hiển thị'" @click="openSort" icon="pi pi-sort"
                            class="mr-2 p-button-outlined p-button-secondary" />
                        <!-- <ToggleButton v-tooltip.top="!resizeTable ? 'Chỉnh độ rộng cột' : 'Cột mặc định'" 
                            v-model="resizeTable"
                            onIcon="pi pi-arrows-h" offIcon="pi pi-arrows-alt" class="mr-2" 
                        /> -->
                        <Button v-tooltip.top="'Kết xuất báo cáo'" @click="opendgPrint" icon="pi pi-download" class="p-button-outlined" />
                    </div>
                </template>
            </Toolbar>
        </div>
        <DataTable v-if="selectedCols" 
            rowGroupMode="rowspan" 
            :groupRowsBy="groupRowsBy" 
            v-model:selection="selectedData"
            selectionMode="single" 
            :first="startnumber" 
            :reorderableColumns="true" 
            resizableColumns 
            columnResizeMode="fit"
            :paginator="datas.length > 20" 
            :rows="20" 
            id="dtData" 
            scrollable 
            :scrollHeight="datas.length > 0 ? 'calc(100vh - 210px)' : 'calc(100vh - 170px)'"
            :value="datas" 
            showGridlines 
            class="p-datatable-sm tbl-report-dynamic" 
            @page="onPage($event)"
            :rowsPerPageOptions="[20, 50, 100, 200]"
        >            
            <template #empty>
                <EmptyComponent />
            </template>
            <ColumnGroup type="header" v-if="!resizeTable">
                <Row v-if="colgroups.length > 1">
                    <Column style="background-color: #e6e6e6;text-align:center" class="text-center" :header="g.label"
                        v-for="g in colgroups.filter(x => selectedCols.filter(a => a.group == x.key).length > 0)"
                        :colspan="selectedCols.filter(x => x.group == g.key).length + 1" />
                </Row>
                <Row>
                    <Column class="text-center" :frozen="!resizeTable" style="background-color: #e6e6e6;width:50px;"
                        header="STT" :rowspan="2" />
                    <Column :frozen="c.frozen && !resizeTable" style="background-color: #e6e6e6;" :header="c.title"
                        v-for="c in selectedCols" :sortable="!resizeTable" :field="c.key" />
                </Row>
            </ColumnGroup>
            <Column class="text-center" :frozen="!resizeTable" header="STT"
                style="background-color: #e6e6e6;width:50px;text-align:center">
                <template #body="dt">
                    <b>{{ startnumber + dt.index + 1 }}</b>
                </template>
            </Column>
            <Column :class="c.typedata == 4 ? 'text-center' : ''" :header="c.title" :frozen="c.frozen && !resizeTable"
                v-for="c in selectedCols" :sortable="!resizeTable" :field="c.key">
                <template #body="dt">
                    <img :src="imgurl + dt.data[c.key]"
                        v-if="dt.data[c.key] && (dt.data[c.key].toString()).includes('Portals')" width="48" />
                    <div class="mt-1" v-else-if="c.typedata == 5 && dt.data[c.key]" v-for="r in JSON.parse(dt.data[c.key])">
                        <span v-for="k in Object.keys(r)" v-html="r[k]"></span>
                    </div>
                    <span class="text-center" v-else-if="c.typedata == 4">{{ dt.data[c.key] ? 'X' : '' }}</span>
                    <div v-else-if="c.typedata == 2 || c.typedata == 3">{{ dt.data[c.key] ?
                        dt.data[c.key].toLocaleString("vi-VN",
                            {
                                year: 'numeric',
                                month: '2-digit', day: '2-digit'
                            }) : "" }}</div>
                    <div :style="c.typedata == 7 ? 'white-space:normal;width:250px' : ''" v-else v-html="dt.data[c.key]">
                    </div>
                </template>
            </Column>
        </DataTable>
    </div>
    <OverlayPanel :dismissable="true" ref="op" appendTo="body"
        style="width: 70vw;background-color: #f5f5f5;">
        <div class="flex">
            <div>
                <h3 class="mb-3">Chọn tiêu chí</h3>
                <Tree 
                    :value="colgroups"
                    v-model:selectionKeys="selectedKey"
                    v-model:expandedKeys="expandedKeys" 
                    @nodeSelect="onNodeSelect"
                    @nodeUnselect="onNodeUnselect"
                    selectionMode="checkbox" 
                    :filter="true"
                    filterPlaceholder="Tìm tiêu chí"
                    filterBy="title,titleen"
                    class="mr-2"
                    :rowHover="true"
                    responsiveLayout="scroll"
                    :scrollable="true"
                    scrollHeight="flex"
                    :metaKeySelection="false"
                    style="height: calc(100vh - 315px);overflow-y: auto;min-width:22rem;max-width: 25rem;"
                >
                    <template #default="slotProps">
                        <b v-if="slotProps.node.children">{{ slotProps.node.label }}</b>
                        <span v-else>{{ slotProps.node.label }}</span>
                    </template>
                </Tree>
            </div>
            <div class="flex-1">
                <div class="flex mb-0 w-full align-items-center">
                    <i class="pi pi-cog"></i>
                    <h3 class="flex-1 ml-1 mb-3">Cấu hình tìm kiếm</h3>
                    <div class="flex align-items-center">
                        <Checkbox :binary="true" v-model="AND" />
                        <label class="ml-2"> Kết hợp tất cả nhóm tiêu chí </label>
                    </div>
                    <div class="flex-1"></div>
                    <Button
                        class="p-button-sm ml-1" v-tooltip="'Thêm nhóm'" @click="addBlock()" icon="pi pi-plus" 
                    />
                </div>
                <Accordion
                    class="accor-group-criterias" 
                    :activeIndex="blockindex"
                    style="max-height: calc(100vh - 300px); overflow-y: auto"
                >
                    <AccordionTab v-for="gr in groupBlock">
                        <template #header="dt">
                            <div class="flex w-full align-items-center">
                                <b>Nhóm tiêu chí {{ gr.stt }}</b>
                                <div class="flex align-items-center ml-4">
                                    <Checkbox :binary="true" v-model="gr.AND" />
                                    <label class="ml-2 font-normal"> Kết hợp các tiêu chí </label>
                                </div>
                                <div class="flex-1"></div>
                                <Button v-tooltip.top="'Xoá nhóm'" 
                                     @click="delBlock(dt.index)" 
                                     icon="pi pi-trash"
                                    class="p-button-sm p-button-text p-button-outlined p-button-danger" />
                            </div>
                        </template>
                        <DataTable v-model:expandedRows="expandedRows" 
                            scrollable 
                            scrollHeight="calc(100vh - 220px)"
                            :value="gr.datas" 
                            showGridlines 
                            class="p-datatable-sm w-full tbl-filter-criterias"
                            :rowClass="rowClassTc"
                        >
                            <template #empty>
                                <div
                                    class="align-items-center justify-content-center p-4 text-center m-auto"
                                    :style="{
                                        display: 'flex',
                                        width: '100%',
                                        height: '100px',
                                        backgroundColor: '#fff',
                                    }"
                                ></div>
                            </template>
                            <Column expander 
                                headerStyle="max-width: 4rem"
                                bodyStyle="max-width: 4rem" 
                            />
                            <Column header="Tiêu chí"
                                headerStyle="flex:1;"
                                bodyStyle="flex:1;"
                            >
                                <template #body="dt">
                                    <b>{{ dt.data.title }}</b>
                                </template>
                            </Column>
                            <Column
                                header="Tất cả điều kiện"
                                class="justify-content-center"
                                headerStyle="max-width:10rem"
                                bodyStyle="max-width:10rem"
                            >
                                <template #body="dt">
                                <Checkbox v-model="dt.data.AND" :binary="true" />
                                </template>
                            </Column>
                            <template #expansion="slotProps">
                                <div class="w-full p-0">
                                    <DataTable
                                        class="w-full"
                                        :value="slotProps.data.childs"
                                        :rowClass="rowClassDk"
                                    >
                                        <Column
                                            header="Kiểu giá trị"
                                            headerStyle="max-width:9rem"
                                            bodyStyle="max-width:9rem"
                                            v-if="
                                                slotProps.data.typedata == 2 ||
                                                slotProps.data.typedata == 3
                                            "
                                        >
                                            <template #body="dt">
                                                <Dropdown
                                                filter
                                                v-model="dt.data.typedate"
                                                :options="drTypeDate"
                                                optionLabel="text"
                                                optionValue="value"
                                                class="w-full"
                                                style="border: none; box-shadow: none"
                                                />
                                            </template>
                                        </Column>
                                        <Column
                                            header="Điều kiện"
                                            headerStyle="max-width:13rem"
                                            bodyStyle="max-width:13rem"
                                        >
                                            <template #body="dt">
                                                <Dropdown
                                                    filter
                                                    v-model="dt.data.type"
                                                    :options="renderdrTypes(dt.data)"
                                                    optionLabel="text"
                                                    optionValue="value"
                                                    class="w-full"
                                                    style="border: none; box-shadow: none"
                                                    >
                                                    <template #value="slotProps">
                                                        <div class="" v-if="slotProps.value">
                                                            <div>{{ drTypes.find((x) => x.value == slotProps.value).text }}
                                                                <span class="redsao" v-if="arrConditionRequied.includes(dt.data.type) || (arrTypeRequied.includes(dt.data.typedata) && dt.data.type == '=')">*</span>
                                                            </div>
                                                        </div>
                                                    </template>
                                                </Dropdown>
                                            </template>
                                        </Column>
                                        <Column
                                            header="Giá trị"
                                            headerStyle="flex:1;"
                                            bodyStyle="flex:1;"
                                        >
                                            <template #body="dt">
                                                <Textarea
                                                    rows="1"
                                                    spellcheck="false"
                                                    v-if="!drTypes.find(x => x.value == dt.data.type).hide
                                                    "
                                                    :placeholder="(dt.data.typedata == 2 || dt.data.typedata == 3) ? 'dd/MM/yyyy' : drTypes.find((x) => x.value == dt.data.type).placeholder
                                                    "
                                                    v-model="dt.data.value"
                                                    autoResize                                 
                                                    class="w-full"
                                                    style="border: none; box-shadow: none"
                                                />
                                            </template>
                                        </Column>
                                        <Column
                                            header=""
                                            class="justify-content-center"
                                            headerStyle="max-width: 5rem;"
                                            bodyStyle="max-width: 5rem;min-height:42px;padding-top: 0.25rem !important;padding-bottom: 0.25rem !important;"
                                        >
                                            <template #body="dt">
                                                <Button
                                                class="p-button-text p-button-rounded p-button-outlined p-button-danger"
                                                v-tooltip.top="'Xoá điều kiện'"
                                                @click="
                                                    delFilter(
                                                    dt.index,
                                                    slotProps.data.childs,
                                                    1
                                                    )
                                                "
                                                icon="pi pi-trash"
                                                style="display: none"
                                                />
                                            </template>
                                        </Column>
                                    </DataTable>
                                </div>
                            </template>
                            <Column
                                header=""
                                class="justify-content-center"
                                headerStyle="max-width:100px;"
                                bodyStyle="max-width:100px;min-height:50px;"
                            >
                                <template #body="dt">
                                <Button
                                    class="p-button-text p-button-rounded p-button-outlined mr-1"
                                    v-tooltip.top="'Thêm điều kiện'"
                                    @click="addFilter(dt.data)"
                                    icon="pi pi-plus"
                                    style="display: none"
                                />
                                <Button
                                    class="p-button-text p-button-rounded p-button-outlined p-button-danger"
                                    v-tooltip.top="'Xoá tiêu chí'"
                                    @click="delFilter(dt.index, gr.datas, 0)"
                                    icon="pi pi-trash"
                                    style="display: none"
                                />
                                </template>
                            </Column>
                        </DataTable>
                    </AccordionTab>
                </Accordion>
            </div>
        </div>
        <div class="text-center mt-2">
            <Button
                class="p-button-danger mr-2 w-7rem"
                @click="openFilter()"
                label="Huỷ"
            />
            <Button
                class="w-7rem"
                v-if="groupBlock.length > 0"
                @click="submitFilter()"
                label="Thực hiện"
            />
        </div>
    </OverlayPanel>
    <OverlayPanel ref="opHelp">
        <b>- Ví dụ tìm hồ sơ có tên là "Cường" hoặc "Đức" thì nhập vào như sau:</b>
        <p class="text-blue-500"><i>Tên là Cường,Đức</i></p>
        <b>- Ví dụ tìm hồ sơ nhân sự có tuổi từ 50 trở lên thì nhập theo các cách sau:</b>
        <div class="flex">
            <p class="text-blue-500"><i>Tuổi: >=50</i></p>
            <p class="ml-2 mr-2">Hoặc</p>
            <p class="text-blue-500"><i>Năm sinh: &lt;=1973</i></p>
        </div>
    </OverlayPanel>
    <Dialog v-model:visible="dgSort" modal header="Sắp xếp thứ tự hiển thị cột trong bảng" :style="{ width: '480px' }">
        <OrderList v-model="selectedCols" listStyle="height:auto;" dataKey="key">
            <template #header> Cột dữ liệu </template>
            <template #item="slotProps">
                <div class="flex align-items-center">
                    <div class="flex-1">
                        {{ slotProps.item.title }}
                    </div>
                    <Checkbox v-tooltip="'Giữ cột này khi cuộn'" v-model="slotProps.item.frozen" :binary="true"
                        class="ml-1" />
                    <Button class="p-button-text p-button-danger p-button-outlined ml-2" @click="delCols(slotProps.index)" icon="pi pi-trash" />
                </div>
            </template>
        </OrderList>

    </Dialog>
    <Dialog @hide="stopMic(false)" v-model:visible="opMic.isshow" modal header="Tìm kiếm bằng giọng nói"
        :style="{ width: '480px', backgroundColor: '#eee' }">
        <div class="p-2" style="background-color: #eee;">
            <iframe frameborder="none" style="width: 100%;height: 100%;"
                :src="opMic.start ? 'https://embed.lottiefiles.com/animation/91427' : 'https://embed.lottiefiles.com/animation/10627'"></iframe>
            <Card class="mt-2 mb-2" v-if="opMic.start">
                <template #content>
                    <div v-if="opMic.error" style="font-size: 20pt;font-weight: bold;color:red">Không thu được giọng nói của
                        bạn, vui lòng thử lại</div>
                    <div v-else style="font-size: 20pt;font-weight: bold;">{{ ipsearch || "Hãy nói vào mic nhé" }}</div>
                </template>
            </Card>
        </div>
        <div class="text-center mt-2">
            <ToggleButton @click="startMic" v-model="opMic.start" onLabel="Đã xong" offLabel="Bắt đầu nói"
                offIcon="pi pi-play" onIcon="pi pi-stop" />
        </div>
    </Dialog>
    <Dialog v-model:visible="dgPrint" modal header="Kết xuất báo cáo" :style="{ width: '720px' }">
        <h3 class="mt-0">Tiêu đề báo cáo</h3>
        <Editor spellcheck="false" v-model="hTieude" editorStyle="height: 150px" />
        <!-- <h3>Tên file báo cáo</h3>
        <InputText v-model="fileExport" spellcheck="false" class="w-full" /> -->
        <div class="text-center mt-4">
            <Button class="p-button-danger mr-2" icon="pi pi-print" @click="PrintDiv()" label="In báo cáo" />
            <Button icon="pi pi-download" v-if="groupBlock.length > 0" @click="downloadFile()" label="Tải báo cáo" />
            <Button class="ml-2" icon="pi pi-save" @click="saveBaocao()" label="Lưu báo cáo" />
        </div>
    </Dialog>
    <Sidebar v-model:visible="showSidebaBaocao" style="width: 50vw;">
        <div class="p-2">
            <div class="flex">
                <h2 class="flex-1 m-0">Báo cáo đã lưu ({{ dtBacaos.length }}) </h2>
                <Button icon="pi pi-refresh" severity="secondary" outlined class="ml-2" @click="mBaocao.value.id = null" />
            </div>
            <DataTable showGridlines class="p-datatable-sm mt-2" :value="dtBacaos">
                <Column header="STT" style="width:50px" class="text-center">
                    <template #body="dt">
                        {{ dt.index + 1 }}
                    </template>
                </Column>
                <Column field="hTieude" header="Tiêu đề">
                    <template #body="dt">
                        <Button class="text-left" @click="goBaocao(dt.data)" :label="dt.data.name" link>
                        </Button>
                    </template>
                </Column>
                <Column field="date" header="Ngày tạo" style="width: 160px;" class="text-center"></Column>
                <Column>
                    <template #body="dt">
                        <Button size="small" @click="delBaocao(dt.index)" icon="pi pi-trash" severity="danger" text
                            outlined />
                    </template>
                </Column>
            </DataTable>
        </div>
    </Sidebar>
</template>
<style lang="scss" scoped>
:deep {
    span.p-column-title {
        white-space: nowrap;
    }

    thead th {
        background-color: #e6e6e6 !important;
    }

    .p-listbox-item {
        cursor: pointer;
        position: relative;
        overflow: hidden;
        display: inline-block;
        background-color: aliceblue;
        margin: 5px !important;
        border-radius: 50px !important;
    }

    #dtData td,
    #dtData th {
        border: .5px solid #ccc
    }

    .p-tree .p-tree-container .p-treenode .p-treenode-content {
        padding: 0;
    }

    .p-listbox-item.p-highlight {
        color: #fff !important;
        background: #3b82f6 !important;
    }

    .p-datatable-table tr:hover {
        background-color: aliceblue;
    }
}
:deep(.tbl-report-dynamic) {
    .p-datatable-tbody .p-datatable-emptymessage {
        min-height: calc(100vh - 205px);
        max-height: calc(100vh - 205px);
    }
    .p-datatable-tbody tr.p-datatable-emptymessage:hover {
        background-color: transparent !important;
    }
    .p-datatable-thead {
        z-index: 0;
    }
}
:deep(.custoolbar) {
    .p-toolbar-group-left {
        flex: 1;
    }
}
:deep(.accor-group-criterias) {
  .p-accordion-tab .p-accordion-header .p-accordion-header-link {
    padding: 0.75rem;
  }
  .p-accordion-tab .p-toggleable-content .p-accordion-content {
    padding: 0.75rem;
  }
}

:deep(.tbl-filter-criterias) {
  .p-datatable-row-expansion td {
    padding: 0 !important;
  }
  ::-webkit-input-placeholder {
    color: #b5b5b5;
  }
}
</style>
<style scoped>
    .search-microphone:hover svg {
        color: #ffffff !important;
    }
    .row-tbl-tc:hover > td button {
        display: block !important;
    }
    .row-tbl-dk:hover > td button {
        display: block !important;
    }
</style>
