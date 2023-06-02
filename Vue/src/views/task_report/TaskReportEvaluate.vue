<script setup>
import { ref, inject, onMounted, watch, onBeforeUnmount } from "vue";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
import { integer, required } from "@vuelidate/validators";

import moment from "moment";
import { concat } from "lodash";
import { encr } from "../../util/function.js";
import router from "@/router";
import treeuser from "../../components/user/treeuser.vue";
const cryoptojs = inject("cryptojs");
const emitter = inject("emitter");
const basedomainURL = fileURL;

const toast = useToast();
const swal = inject("$swal");
const store = inject("store");
const axios = inject("axios"); // inject axios

const config = {
    headers: { Authorization: `Bearer ${store.getters.token}` },
};
const addLog = (log) => {
    axios.post(baseURL + "/api/Proc/AddLog", log, config);
};

const group = ref();
const selectedKeys = ref();
const expandedKeys = ref([]);
const itemSortButs = ref([
    {
        label: "Ngày tạo mới đến cũ",
        sort: "created_date",
        ob: "DESC",
        active: true,
        command: (event) => {
            ChangeSortTask("created_date", "DESC");
        },
    },
    {
        label: "Ngày tạo cũ đến mới",
        sort: "created_date",
        ob: "ASC",
        active: false,
        command: (event) => {
            ChangeSortTask("created_date", "ASC");
        },
    },
    {
        label: "Ngày bắt đầu mới đến cũ",
        sort: "start_date",
        ob: "DESC",
        active: false,
        command: (event) => {
            ChangeSortTask("modified_date", "DESC");
        },
    },
    {
        label: "Ngày bắt đầu cũ đến mới",
        sort: "start_date",
        ob: "ASC",
        active: false,
        command: (event) => {
            ChangeSortTask("modified_date", "ASC");
        },
    },
    {
        label: "Ngày kết thúc mới đến cũ",
        sort: "end_date",
        ob: "DESC",
        active: false,
        command: (event) => {
            ChangeSortTask("modified_date", "DESC");
        },
    },
    {
        label: "Ngày kết thúc cũ đến mới",
        sort: "end_date",
        ob: "ASC",
        active: false,
        command: (event) => {
            ChangeSortTask("modified_date", "ASC");
        },
    },
    {
        label: "Người giao việc",
        sort: "modified_date",
        ob: "ASC",
        active: false,
        command: (event) => {
            ChangeSortTask("modified_date", "ASC");
        },
    },
]);

const itemExportButs = ref([
    {
        label: "File Word",
        type: 1,
        command: (event) => {
            ChangeExportTask();
        },
    },
    {
        label: "File XML",
        type: 2,
        command: (event) => {
            ChangeExportTask();
        },
    },
]);

const ChangeExportTask = (item) => {
    if (item.type == 1) {
        openExportData();
    } else {
        openExportXML();
    }
    menuExportButs.value.toggle();
};

const ChangeSortTask = (sort, ob) => {
    options.value.sort = sort;
    options.value.ob = ob;
    itemSortButs.value.forEach((i) => {
        if (i.sort == sort && i.ob == ob) {
            i.active = true;
        } else {
            i.active = false;
        }
    });
    menuSortButs.value.toggle();
    loadData(true);
};

const refresh = () => {
    options.value = {
        IsNext: true,
        sort: "created_date",
        ob: "DESC",
        pageNo: 1,
        pageSize: 20,
        search: "",
        IsType: null,
        SearchTextUser: "",
        filter_type: 0,
        Filteruser_id: null,
        organization_type: null,
        user_id: store.getters.user_id,
        loctitle: "Lọc",
        sdate: null,
        edate: null,
        filterDuan: null,
        filterStatus: null,
        filterUser: null,
        type_group: 0,
        loc_title: "Lọc dữ liệu",
        active_filter: false,
    };
    group.value = null;
    loadData(true);
};

const options = ref({
    IsNext: true,
    sort: "created_date",
    ob: "DESC",
    pageNo: 1,
    pageSize: 20,
    search: "",
    IsType: null,
    SearchTextUser: "",
    filter_type: 0,
    Filteruser_id: null,
    organization_type: null,
    user_id: store.getters.user_id,
    loctitle: "Lọc",
    sdate: null,
    edate: null,
    filterDuan: null,
    filterStatus: null,
    filterUser: null,
    type_group: 0,
    loc_title: "Lọc dữ liệu",
    active_filter: false,
});
const listDropdownProject = ref([]);
const listDropdownVaitro = ref([
    { value: 3, text: "Theo dõi" },
    { value: 1, text: "Đang làm" },
    { value: 0, text: "Quản lý" },
]);
const listTaskReportEvaluates = ref([]);
const RenderData = (data) => {
    // listTask.value = [];
    // opition.value.totalRecords = null;
    let arrChils = [];
    data
        .filter(
            (x) =>
                x.parent_id == null ||
                (x.parent_id != null &&
                    data.filter((y) => y.task_id == x.parent_id).length == 0),
        )
        .forEach((m, i) => {
            m.STT2 = opition.value.PageNo * opition.value.PageSize + i + 1;
            let om = { key: m.task_id, data: m };
            const rechildren = (mm, task_id) => {
                let dts = data.filter((x) => x.parent_id == task_id);
                if (dts.length > 0) {
                    if (!mm.children) mm.children = [];
                    dts.forEach((em, j) => {
                        em.STT2 = mm.data.STT2 + "." + (j + 1);
                        let om1 = { key: em.task_id, data: em };
                        om1.data.is_order = j + 1;
                        rechildren(om1, em.task_id);
                        mm.children.push(om1);
                    });
                }
            };
            rechildren(om, m.task_id);

            arrChils.push(om);
        });
    return arrChils;
};
const renderTree = (data, id, name, title) => {
    let arrChils = [];
    let arrtreeChils = [];
    data
        .filter((x) => x.parent_id == null)
        .forEach((m, i) => {
            m.IsOrder = i + 1;
            m.label_order = m.IsOrder.toString();
            if (options.value.pageNo > 0) {
                m.STT = (options.value.pageNo - 1) * options.value.pageSize + i + 1;
            } else {
                m.STT = i + 1;
            }
            let om = { key: m[id], data: m };
            const rechildren = (mm, pid) => {
                let dts = data.filter((x) => x.parent_id == pid);
                if (dts.length > 0) {
                    if (!mm.children) mm.children = [];
                    dts.forEach((em, index) => {
                        em.label_order = mm.data.label_order + "." + em.is_order;
                        em.STT = mm.data.STT + "." + (index + 1);
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
                let dts = data.filter((x) => x.parent_id == pid);
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
    arrtreeChils.unshift({
        key: -1,
        data: -1,
        label: "-----Chọn " + title + "----",
    });
    return { arrChils: arrChils, arrtreeChils: arrtreeChils };
};
const groupBy = (list, props) => {
    return list.reduce((a, b) => {
        (a[b[props]] = a[b[props]] || []).push(b);
        return a;
    }, {});
};
const listDropdownStatus = ref([
    {
        value: 0,
        text: "Chưa bắt đầu",
        bg_color: "#bbbbbb",
        text_color: "#FFFFFF",
    },
    { value: 1, text: "Đang làm", bg_color: "#2196f3", text_color: "#FFFFFF" },
    { value: 2, text: "Tạm ngừng", bg_color: "#d87777", text_color: "#FFFFFF" },
    { value: 3, text: "Đã đóng", bg_color: "#d87777", text_color: "#FFFFFF" },
    { value: 4, text: "HT đúng hạn", bg_color: "#04D215", text_color: "#FFFFFF" },
    {
        value: 5,
        text: "Chờ đánh giá",
        bg_color: "#33c9dc",
        text_color: "#FFFFFF",
    },
    { value: 6, text: "Bị trả lại", bg_color: "#ffa500", text_color: "#FFFFFF" },
    { value: 7, text: "HT sau hạn", bg_color: "#ff8b4e", text_color: "#FFFFFF" },
    { value: 8, text: "Đã đánh giá", bg_color: "#51b7ae", text_color: "#FFFFFF" },
    { value: -1, text: "Bị xóa", bg_color: "red", text_color: "#FFFFFF" },
]);
var is_permission = null,
    report_module_id;
var is_reportmodule = 0;
const getReportModuleId = (rf) => {
    axios
        .post(
            baseURL + "/api/Modules/GetDataProc",
            {
                str: encr(
                    JSON.stringify({
                        proc: "report_modules_get_id1",
                        par: [
                            { par: "@is_link", va: router.currentRoute.value.fullPath },
                            { par: "@user_id", va: store.getters.user.user_id },
                        ],
                    }),
                    SecretKey,
                    cryoptojs,
                ).toString(),
            },
            config,
        )
        .then((response) => {
            let data = JSON.parse(response.data.data)[0];
            if (data.length > 0) {
                is_permission = data[0].is_permission;
                report_module_id = data[0].report_module_id;
            }
            loadDataContinue(rf);
        })
        .catch((error) => {
            if (error && error.status === 401) {
                errorMessage();
            }
        });
};
const loadDataContinue = (rf) => {
    if (rf) {
        options.value.loading = true;
        swal.fire({
            width: 110,
            didOpen: () => {
                swal.showLoading();
            },
        });
    }
    axios
        .post(
            baseURL + "/api/TaskProc/getTaskData",
            {
                str: encr(
                    JSON.stringify({
                        proc: "task_report_evaluate_list",
                        par: [
                            { par: "user_id", va: store.getters.user.user_id },
                            { par: "pageNo", va: options.value.pageNo },
                            { par: "pageSize", va: options.value.pageSize },
                            { par: "search", va: options.value.search },
                            { par: "IsType", va: options.value.IsType },
                            { par: "loc", va: options.value.filter_type },
                            { par: "sdate", va: options.value.sdate },
                            { par: "edate", va: options.value.edate },
                            { par: "filterUser", va: options.value.filterUser },
                            { par: "is_reportmodule", va: is_reportmodule },
                            { par: "report_module_id", va: report_module_id },
                            { par: "type_report", va: is_permission },
                            { par: "type_group", va: options.value.type_group },
                            { par: "filterDuan", va: options.value.filterDuan },
                        ],
                    }),
                    SecretKey,
                    cryoptojs,
                ).toString(),
            },
            config,
        )
        .then((response) => {
            let data = JSON.parse(response.data.data);
            if (data[0].length > 0) {
                data[0].forEach((d, i) => {
                    let arr = d.tasks ? JSON.parse(d.tasks) : [];
                    let newarr = [];
                    if (arr.length > 0) {
                        for (var i = 0; i < arr.length; i++) {
                            if (newarr.filter(x => x.task_id == arr[i].task_id).length == 0) {
                                newarr.push(arr[i]);
                            }
                        }
                    }
                    d.tasks = newarr;
                    d.STT = (options.value.pageNo - 1) * options.value.pageSize + (i + 1);
                    d.point_task = 0;
                    d.isShow = false;
                    if (d.tasks.length > 0) {
                        d.tasks.forEach((t, i) => {
                            t.STT = d.STT + "." + (i + 1);
                            d.point_task += parseInt(t.point_task);
                        })
                    }
                });
                var arrNew = [];
                if (options.value.type_group == 1) {
                    var listReports = groupBy(data[0], "department_id");
                    for (let k in listReports) {
                        var Group = [];
                        listReports[k].forEach(function (r) {
                            Group.push(r);
                        });
                        arrNew.push({
                            isShow: true,
                            group_id: k,
                            group_name: k != 'null' ? listReports[k][0].department_name : "Không thuộc phòng ban",
                            Group: Group,
                        });
                    }
                } else if (options.value.type_group == 2) {
                    var listReports = groupBy(data[0], "organization_id");
                    for (let k in listReports) {
                        var Group = [];
                        listReports[k].forEach(function (r) {
                            Group.push(r);
                        });
                        arrNew.push({
                            isShow: true,
                            group_id: k,
                            group_name: k != 'null' ? listReports[k][0].organization_name : "Không thuộc đơn vị",
                            Group: Group,
                        });
                    }
                }
                if (options.value.type_group == 0) {
                    listTaskReportEvaluates.value = data[0];
                } else {
                    arrNew.forEach((z, i) => {
                        z.Group.forEach((x, i) => {
                            x.STT = (options.value.pageNo - 1) * options.value.pageSize + (i + 1);
                            if (x.tasks.length > 0) {
                                x.tasks.forEach((t, j) => {
                                    t.STT = x.STT + "." + (j + 1);
                                    t.point_task += parseInt(j.point_task);
                                })
                            }
                        });
                    });
                    listTaskReportEvaluates.value = arrNew;
                }
            }
            if (rf) {
                options.value.loading = false;
                swal.close();
            }
        })
        .catch((error) => {
            toast.error("Tải dữ liệu không thành công!");
            options.value.loading = false;
            addLog({
                title: "Lỗi Console loadData",
                controller: "LogsView.vue",
                log_content: error.message,
                loai: 2,
            });
            if (error && error.status === 401) {
                swal.fire({
                    text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                    confirmButtonText: "OK",
                });
                store.commit("gologout");
            }
        });
};
const loadData = (rf) => {
    if (router.currentRoute.value.fullPath.indexOf("/reportmodule") !== -1) {
        is_reportmodule = 1;
        getReportModuleId(rf);
    } else loadDataContinue(rf);
};
const bgColor = ref([
    "#F8E69A",
    "#AFDFCF",
    "#F4B2A3",
    "#9A97EC",
    "#CAE2B0",
    "#8BCFFB",
    "#CCADD7",
]);
const menuSortButs = ref();
const menuExportButs = ref();
const toggleSort = (event) => {
    menuSortButs.value.toggle(event);
};
const toggleExport = (event) => {
    menuExportButs.value.toggle(event);
};
const Del_ChangeFilter = () => {
    menuFilterButs.value.toggle();
    options.value = {
        IsNext: true,
        sort: "created_date",
        ob: "DESC",
        pageNo: 1,
        pageSize: 20,
        search: "",
        IsType: null,
        SearchTextUser: "",
        filter_type: 0,
        Filteruser_id: null,
        organization_type: null,
        user_id: store.getters.user_id,
        loc_title: "Lọc dữ liệu",
        sdate: null,
        edate: null,
        filterDuan: null,
        filterStatus: null,
        filterUser: null,
        type_group: 0,
        active_filter: false,
    };
    loadData(true);
};

const listDropdownUser = ref();

const listUser = () => {
    axios
        .post(
            baseURL + "/api/TaskProc/getTaskData",
            {
                str: encr(
                    JSON.stringify({
                        proc: "sys_users_list_task_origin",
                        par: [
                            { par: "search", va: options.value.SearchTextUser },
                            { par: "user_id", va: store.getters.user.user_id },
                            { par: "role_id", va: null },
                            {
                                par: "organization_id",
                                va: store.getters.user.organization_id,
                            },
                            { par: "department_id", va: null },
                            { par: "position_id", va: null },

                            { par: "isadmin", va: null },
                            { par: "status", va: null },
                            { par: "start_date", va: null },
                            { par: "end_date", va: null },
                        ],
                    }),
                    SecretKey,
                    cryoptojs,
                ).toString(),
            },
            config,
        )
        .then((response) => {
            let data = JSON.parse(response.data.data)[0];
            listDropdownUser.value = data.map((x, i) => ({
                stt: i,
                name: x.full_name,
                code: x.user_id,
                avatar: x.avatar,
                ten: x.last_name,
            }));
        })
        .catch((error) => {
            console.log(error);
            toast.error("Tải dữ liệu không thành công!");
            options.value.loading = false;

            if (error && error.status === 401) {
                swal.fire({
                    text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                    confirmButtonText: "OK",
                });
                store.commit("gologout");
            }
        });
};

const listTudien = () => {
    axios
        .post(
            baseURL + "/api/TaskProc/getTaskData",
            {
                str: encr(
                    JSON.stringify({
                        proc: "task_origin_get_list_init",
                        par: [
                            {
                                par: "user_id",
                                va: store.getters.user.user_id,
                            },
                        ],
                    }),
                    SecretKey,
                    cryoptojs,
                ).toString(),
            },
            config,
        )
        .then((response) => {
            let data = JSON.parse(response.data.data);
            listDropdownProject.value = data[0];
        })
        .catch((error) => {
            console.log(error);
            toast.error("Tải dữ liệu không thành công!");
            options.value.loading = false;

            if (error && error.status === 401) {
                swal.fire({
                    text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                    confirmButtonText: "OK",
                });
                store.commit("gologout");
            }
        });
};

onMounted(() => {
    if (store.getters.user.is_super == true) {
        group.value = "organization_id";
    } else {
        group.value = "department_id";
    }
    listTudien();
    listUser();
    loadData(true);
    return {};
});
function bindRow(r, td, rp, type) {
    let trs = "";
    if (type == 1) {
        if (!r[td.value]) {
            if (td.value == 'count_tasks') {
                let str = r.tasks.length;
                trs += `<td rowspan=${rp} style='text-align:center;' class="text">${str} </td>`
            } else {
                trs += `<td rowspan=${rp}></td>`
            }
        } else {
            if (r[td.value].toString().includes("Portals"))
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
            // else if (td.typedata == 2 || td.typedata == 3) {
            //     trs += `<td rowspan=${rp} class='date' style='text-align:center'>`;
            //     if (r[td.key]) {
            //         try {
            //             trs += r[td.key].toLocaleString("vi-VN", {
            //                 year: 'numeric',
            //                 month: '2-digit', day: '2-digit'
            //             })
            //         } catch (e) { }
            //     }
            //     trs += "</td>"
            // }

            else {
                let str = r[td.value];
                if (td.value == "full_name") {
                    trs += `<td rowspan=${rp} class="text">${str} </td>`
                } else {
                    trs += `<td rowspan=${rp} style='text-align:center;' class="text">${str} </td>`
                }
            }
        }
    } else if (type == 2) {
        if (td.value == "full_name") {
            let str = r['task_name'];
            let start_date = r['start_date'];
            let end_date = r['end_date'];
            trs += `<td colspan='2' class="text" style='padding-left:10px;'>`
            trs += `<div style="display: flex; flex-direction: column;">`
            trs += `<span>${str}</span>`
            if (start_date || end_date) {
                trs += `<div style="font-size: 12px; padding-left: 5px"><span v-if="t.start_date || t.end_date" style="color: #98a9bc">( ${start_date} - ${end_date} )</span></div>`
            }
            trs += `</div>`
            trs += `</td>`
        } else if (td.value == "point_task") {
            let str = r[td.value].replace("NaN",'');
            trs += `<td rowspan=${rp} style='text-align:center;' class="text">${str} </td>`
        }
    }

    return trs;
}

const hTieude = ref("Báo cáo đánh giá công việc");
const getHTMLTable = (f) => {
    let titlebaocao = `
                <table style="border:none;">
                    <tr style="border:none"><td style="border:none;text-align:center;font-weight: bold; font-size: 20px;" colspan=${listColumExportNews.value.length + 1}>${hTieude.value}</td></tr>
                </table>
            `;
    let divid = "table-task-report";
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
    listColumExportNews.value.forEach((c, i) => {
        trs += `<th class="text" style='width:${widths[i + 1]}px;text-align:${c.typedata == 2 || c.typedata == 3 ? "center" : "left"}'>${c.text}</th>`
    })
    trs += "</tr>"
    trs += "</thead>"
    trs += "<tbody>"
    if (options.value.type_group == 1 || options.value.type_group == 2) {
        listTaskReportEvaluates.value.forEach((k, i) => {
            trs += "<tr>"
            let rosp = listColumExportNews.value.length + 1;
            trs += `<td colspan=${rosp} style='background-color: #f6ddcc; font-size: 14px; font-weight: bold;'>${k.group_name}</td>`
            trs += "</tr>"
            if (k.Group.length > 0) {
                trs += "</tr>"
                k.Group.forEach((r, j) => {
                    if (r.tasks.length > 0) {
                        trs += "<tr>"
                        trs += `<td style='text-align:center;'>${j + 1}</td>`
                        listColumExportNews.value.forEach(td => {
                            trs += bindRow(r, td, 1, 1);
                        });
                        trs += "</tr>"
                        r.tasks.forEach((t, m) => {
                            trs += "<tr>"
                            trs += `<td style='text-align:center;'>${(j + 1) + '.' + (m + 1)}</td>`
                            listColumExportNews.value.forEach(td => {
                                trs += bindRow(t, td, 1, 2);
                            });
                            trs += "</tr>"
                        })
                    } else {
                        trs += "<tr>"
                        trs += `<td style='text-align:center;'>${j + 1}</td>`
                        listColumExportNews.value.forEach(td => {
                            trs += bindRow(r, td, 1, 1);
                        });
                        trs += "</tr>"
                    }
                })
            }
        })
    }
    else {
        listTaskReportEvaluates.value.forEach((r, i) => {
            if (r.tasks.length > 0) {
                trs += "<tr>"
                trs += `<td style='text-align:center;'>${i + 1}</td>`
                listColumExportNews.value.forEach(td => {
                    trs += bindRow(r, td, 1, 1);
                });
                trs += "</tr>"
                r.tasks.forEach((t, j) => {
                    trs += "<tr>"
                    trs += `<td style='text-align:center;'>${(i + 1) + '.' + (j + 1)}</td>`
                    listColumExportNews.value.forEach(td => {
                        trs += bindRow(t, td, 1, 2);
                    });
                    trs += "</tr>"
                })
            } else {
                trs += "<tr>"
                trs += `<td style='text-align:center;'>${i + 1}</td>`
                listColumExportNews.value.forEach(td => {
                    trs += bindRow(r, td, 1, 1);
                });
                trs += "</tr>"
            }
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
const downloadFileExport = (
    name_func,
    file_name_download,
    file_name,
    file_type
) => {
    let nameF = (file_name || "file_download") + file_type;
    let nameDownload = (file_name_download || "file_download") + file_type;
    const a = document.createElement("a");
    a.href = "https://apidev.soe.vn/" + "/api/SRC/" + name_func + "?name=" + nameF;
    a.download = nameDownload;
    a.target = "_blank";
    a.click();
    a.remove();
};
const downloadFile = async () => {
    // var wnd = window.open("about:blank", "", "_blank");
    // wnd.document.write(getHTMLTable(true));
    // return false;
    let filename = 'test';
    swal.fire({
        width: 110,
        didOpen: () => {
            swal.showLoading();
        },
    });
    try {
        let dataHtml = { html: getHTMLTable(true), filename: filename || "doc" };
        const axResponse = await axios
            .post(
                "https://apidev.soe.vn/" + "api/SRC/ConvertFileXLS",
                dataHtml,
                {
                    headers: { "Content-Type": "application/json" }
                }
            );

        if (axResponse.status == 200) {
            downloadFileExport(
                "GetDownloadXLS",
                dataHtml.filename,
                axResponse.data.fileName,
                ".xlsx"
            );
        }
    } catch (e) {
        console.log(e);
    }
    swal.close();
}

const DialogExportData = ref(false);
const headerDialogExportData = ref(false);
const listColumExportNews = ref([]);
const listColumExportOrigins = ref([]);
const listColumExports = ref([
    { value: "full_name", text: "Tên nhân sự", STT: 1, typedata: 2 },
    { value: "count_tasks", text: "Số công việc thực hiện", STT: 2, typedata: 2 },
    { value: "point_task", text: "Điểm đanh giá", STT: 3, typedata: 2 },
]);
const openExportData = () => {
    listColumExportNews.value = [];
    listColumExportOrigins.value = [...listColumExports.value];
    DialogExportData.value = true;
    headerDialogExportData.value = "Kết xuất ra file excel";
};

const openExportXML = () => {
    swal.fire({
        width: 110,
        didOpen: () => {
            swal.showLoading();
        },
    });
    axios
        .post(
            baseURL + "/api/task_origin/TaskPersonal_export_xml",
            listTaskReportEvaluates.value,
            config,
        )
        .then((response) => {
            if (response.data.err != "1") {
                swal.close();
                toast.success("Export file XML thành công!");
            }
        })
        .catch((error) => {
            swal.close();
            swal.fire({
                title: "Thông báo!",
                text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
                icon: "error",
                confirmButtonText: "OK",
            });
        });
};

const chooseColumExport = (col) => {
    listColumExportNews.value.push(col);
    var a = listColumExportOrigins.value.indexOf(col);
    listColumExportOrigins.value
        .splice(a, 1)
        .sort((a, b) => (a.STT > b.STT ? 1 : -1));
};

const removeColumExport = (col) => {
    listColumExportOrigins.value.push(col);
    listColumExportOrigins.value.sort((a, b) => (a.STT > b.STT ? 1 : -1));
    var a = listColumExportNews.value.indexOf(col);
    listColumExportNews.value.splice(a, 1);
};

const chooseAllColum = (isChon) => {
    if (isChon == true) {
        listColumExportNews.value = [];
        listColumExportNews.value = [...listColumExports.value];
        listColumExportOrigins.value = [];
    } else {
        listColumExportNews.value = [];
        listColumExportOrigins.value = [...listColumExports.value];
    }
};

const closeDialogExport = () => {
    DialogExportData.value = false;
};

const onPage = (event) => {
    if (event.rows != options.value.pageSize) {
        options.value.pageSize = event.rows;
    }
    if (event.page == 0) {
        //Trang đầu
        options.value.id = null;
        options.value.IsNext = true;
    } else if (event.page > options.value.pageNo + 1) {
        //Trang cuối
        options.value.id = -1;
        options.value.IsNext = false;
    } else if (event.page > options.value.pageNo) {
        //Trang sau

        options.value.id =
            listTaskReportEvaluates.value[
                listTaskReportEvaluates.value.length - 1
            ].task_id;
        options.value.IsNext = true;
    } else if (event.page < options.value.pageNo) {
        //Trang trước
        options.value.id = listTaskReportEvaluates.value[0].task_id;
        options.value.IsNext = false;
    }
    options.value.pageNo = event.page;
    loadData(true);
};
const menuGroupByButs = ref();
const menuFilterButs = ref();
const itemGroupBys = ref([
    {
        label: "Không group",
        value: 0,
        active: true,
        command: () => {
            ChangeGroupBy(0);
        },
    },
    {
        label: "Phòng ban",
        value: 1,
        active: false,
        command: () => {
            ChangeGroupBy(1);
        },
    },
    {
        label: "Đơn vị",
        value: 2,
        active: false,
        command: () => {
            ChangeGroupBy(2);
        },
    },
]);
const itemFilterButs = ref([
    {
        label: "",
        icon: "",
        active: false,
        istype: 5,
        hasChildren: true,
        groups: [
            {
                label: "Chọn cây nhân sự",
                icon: "pi pi-calendar",
                active: false,
                is_children: 1,
            },
            {
                label: "Dự án",
                icon: "pi pi-calendar",
                active: false,
                is_children: 2,
            },
        ],
    },
    {
        label: "Theo thời gian",
        icon: "pi pi-calendar-times",
        active: false,
        istype: 2,
        hasChildren: true,
        groups: [
            {
                label: "Ngày bắt đầu",
                icon: "pi pi-calendar",
                children_id: true,
                is_change: 1,
            },
            {
                label: "Ngày kết thúc",
                icon: "pi pi-calendar",
                children_id: true,
                is_change: 2,
            },
        ],
    },
]);
const toggleGroupBy = (event) => {
    if (store.getters.user.is_super == true) {
        itemGroupBys.value = ([
            {
                label: "Không group",
                value: 0,
                active: true,
                command: () => {
                    ChangeGroupBy(0);
                },
            },
            {
                label: "Phòng ban",
                value: 1,
                active: false,
                command: () => {
                    ChangeGroupBy(1);
                },
            },
            {
                label: "Đơn vị",
                value: 2,
                active: false,
                command: () => {
                    ChangeGroupBy(2);
                },
            },
        ]);
    } else {
        itemGroupBys.value = ([
            {
                label: "Không group",
                value: 0,
                active: true,
                command: () => {
                    ChangeGroupBy(0);
                },
            },
            {
                label: "Phòng ban",
                value: 1,
                active: false,
                command: () => {
                    ChangeGroupBy(1);
                },
            },
        ]);
    }
    menuGroupByButs.value.toggle(event);
};
const toggleFilter = (event) => {
    menuFilterButs.value.toggle(event);
}
const ChangeGroupBy = (model) => {
    menuGroupByButs.value.toggle();
    options.value.type_group = model.value;
    loadData(true);
}
const openShowTask = (model) => {
    model.isShow = !model.isShow;
}
// dialog user
const displayDialogUser = ref(false);
const headerDialogUser = ref();
const is_one = ref(false);
const selectedUser = ref([]);
const openTreeUser = (model) => {
    selectedUser.value = [];
    headerDialogUser.value = "Chọn nhân sự";
    displayDialogUser.value = true;
}
const closeDialog = () => {
    displayDialogUser.value = false;
}
const filterusers = ref([]);
const choiceTreeUser = () => {
    let arrfilter = [];
    if (selectedUser.value.length > 0) {
        selectedUser.value.forEach((t) => {
            arrfilter.push(listTaskReportEvaluates.value.filter(x => x.user_id == t.user_id));
            filterusers.value.push(t.user_id);
        });
    }
    listTaskReportEvaluates.value = [...arrfilter[0]];
    displayDialogUser.value = false;
};
//end
const filterTime1 = ref();
const filterTime2 = ref();
const ChangeFilterAdvanced = (type) => {
    options.value.loc_title = "Theo dự án";
    options.value.active_filter = true;
    menuFilterButs.value.toggle();
    loadData(true);
};
const ChangeFilter = () => {
    if (options.value.sdate) {
        if (options.value.edate) {
            options.value.loc_title =
                "Từ " +
                moment(options.value.sdate).format("DD/MM/YYYY") +
                " - " +
                moment(options.value.edate).format("DD/MM/YYYY");
        } else {
            options.value.loc_title =
                "Từ ngày " + moment(options.value.sdate).format("DD/MM/YYYY");
        }
    } else {
        if (options.value.edate) {
            options.value.loc_title =
                "Đến ngày " + moment(options.value.edate).format("DD/MM/YYYY");
        }
    }
    filterTime1.value = options.value.sdate
        ? moment(new Date(options.value.sdate)).format("DD/MM/YYYY")
        : null;
    filterTime2.value = options.value.edate
        ? moment(new Date(options.value.edate)).format("DD/MM/YYYY")
        : null;
    options.value.active_filter = true;
    menuFilterButs.value.toggle();
    loadData(true);
};
const ChangeTimeFilter = (type, value) => {
    if (type == 1) {
        filterTime1.value = moment(new Date(value)).format("DD/MM/YYYY HH:mm");
        options.value.sdate = value;
    } else {
        filterTime2.value = moment(new Date(value)).format("DD/MM/YYYY HH:mm");
        options.value.edate = value;
    }
};
</script>
<template>
    <div class="surface-100 p-2">

        <!-- {{ listTaskReportEvaluates }} -->
        <Toolbar class="outline-none surface-0 border-none">
            <template #start>
                <div class="flex" style="width: 100%;">
                    <span class="p-input-icon-left" style="width: 100%;">
                        <i class="pi pi-search" />
                        <InputText type="text" spellcheck="false" v-model="options.search" style="width: 100%;"
                            placeholder="Tìm kiếm" v-on:keyup.enter="loadDataContinue(true)" />
                    </span>
                </div>
            </template>
            <template #end>
                <ul id="toolbar_right" style="padding: 0px; margin: 0px; display: flex">
                    <li @click="toggleGroupBy($event)" :class="{ active: options.group_by }" aria-haspopup="true"
                        aria-controls="overlay_Export">
                        <a> Nhóm dữ liệu
                            <i class="pi pi-angle-down"></i></a>
                    </li>
                    <li @click="toggleFilter($event)" :class="{ active: options.active_filter }" aria-haspopup="true"
                        aria-controls="overlay_Export">
                        <a> {{ options.loc_title }}
                            <i class="pi pi-angle-down"></i></a>
                    </li>
                </ul>
                <OverlayPanel ref="menuFilterButs" id="task_filter" style="z-index: 10">
                    <div style="
                min-height: calc(100vh - 250px);
                max-height: calc(100vh - 250px);
                width: 100%;
                overflow-x: scroll;
              ">
                        <ul v-for="(item, index) in itemFilterButs" :key="index" style="padding: 0px; margin: 0px">
                            <li v-if="item.istype == 5" :class="{
                                children: item.hasChildren,
                                parent: !item.hasChildren,
                            }" class="p-menuitem">
                                <ul style="padding: 0px; display: flex; flex-direction: row">
                                    <li style="
                        list-style: none;
                        /* padding: 10px; */
                        /* display: flex; */
                        flex: 1;
                        align-items: center;
                      " v-for="(item1, index) in item.groups" :key="index">
                                        <div v-if="item1.is_children == 1">
                                            <span class="hover-tree-user" @click="openTreeUser(item)"
                                                :class="{ active: item.active }"
                                                style="display: flex; height: 30px;padding: 10px;align-items: center;background-color: #0d89ec;border-radius: 5px;color: #fff;width:fit-content;">
                                                {{ item1.label }}
                                            </span>
                                        </div>
                                        <div v-if="item1.is_children != 1 && item1.is_children != 3"
                                            style="display: flex; align-items: center">
                                            <a style="flex: 1">{{ item1.label }}
                                            </a>
                                            <span style="margin-left: 10px; flex: auto">
                                                <Dropdown @change="ChangeFilterAdvanced(item1)"
                                                    v-if="item1.is_children == 2" :filter="true"
                                                    v-model="options.filterDuan" panelClass="d-design-dropdown"
                                                    selectionLimit="1" :options="listDropdownProject"
                                                    optionLabel="project_name" optionValue="project_id" spellcheck="false"
                                                    class="col-9 ip36 p-0" placeholder="Chọn">
                                                    <template #option="slotProps">
                                                        <div class="country-item flex">
                                                            <div class="pt-1">
                                                                {{ slotProps.option.project_name }}
                                                            </div>
                                                        </div>
                                                    </template>
                                                </Dropdown>
                                            </span>
                                        </div>
                                    </li>
                                </ul>
                            </li>
                            <li v-if="item.istype == 2" :class="{
                                children: item.hasChildren,
                                parent: !item.hasChildren,
                            }" class="p-menuitem">
                                <a :class="{ active: item.active }"><i style="padding-right: 5px" :class="item.icon"></i>{{
                                    item.label }}</a>
                                <ul style="padding: 0px; display: flex">
                                    <li style="
                        list-style: none;
                        padding: 10px;
                        font-weight: bold;
                        display: flex;
                        flex-direction: column;
                      " v-for="(item1, index) in item.groups" :key="index">
                                        <div style="padding-bottom: 10px">
                                            <span>{{ item1.label }}</span>
                                            <span style="
                            color: #2196f3;
                            font-weight: bold;
                            margin-left: 5px;
                            font-size: 14px;
                          " v-if="item1.is_change == 1">{{ filterTime1 }}
                                                <i @click="removeTime(item1.is_change)" v-if="filterTime1"
                                                    style="color: black" class="pi pi-times-circle"></i></span>
                                            <span style="
                            color: #2196f3;
                            font-weight: bold;
                            margin-left: 5px;
                            font-size: 14px;
                          " v-if="item1.is_change == 2">{{ filterTime2 }}
                                                <i @click="removeTime(item1.is_change)" v-if="filterTime2"
                                                    style="color: black" class="pi pi-times-circle"></i></span>
                                        </div>
                                        <Calendar v-if="item1.is_change == 1" @date-select="
                                            ChangeTimeFilter(item1.is_change, options.sdate)
                                            " v-model="options.sdate" :showTime="true" id="filterTime1" :inline="true"
                                            :manualInput="true" />
                                        <Calendar v-if="item1.is_change == 2" @date-select="
                                            ChangeTimeFilter(item1.is_change, options.edate)
                                            " v-model="options.edate" :showTime="true" id="filterTime2"
                                            :inline="true" />
                                    </li>
                                </ul>
                            </li>
                        </ul>
                    </div>
                    <div style="float: right; padding: 10px">
                        <Button @click="ChangeFilter()" label="Thực hiện" />``
                        <Button @click="Del_ChangeFilter" id="btn_huy" style="
                  background-color: #f2f4f6;
                  border: 1px solid #f2f4f6;
                  color: #333;
                  margin-left: 10px;
                " label="Hủy lọc" />
                    </div>
                </OverlayPanel>
                <Menu id="request_group_by" :model="itemGroupBys" ref="menuGroupByButs" :popup="true">
                    <template #item="{ item }">
                        <a @click="ChangeGroupBy(item)" :class="item.value == options.type_group ? 'active' : ''">{{
                            item.label }}</a>
                    </template>
                </Menu>
                <Button @click="openExportData()" class="p-button-outlined p-button-secondary mr-2" icon="pi pi-file"
                    label="Export" />
                <Button @click="refresh()" class="p-button-outlined p-button-secondary mr-2" icon="pi pi-refresh"
                    label="Tải lại" />
            </template>
        </Toolbar>
        <div class="flex"
            style="width: 100%;height: 50px;justify-content: center;align-items: center;font-weight: bold;font-size: 14px;background-color: #fff;">
            BÁO CÁO ĐÁNH GIÁ CÔNG VIỆC
        </div>
        <div class="d-lang-table" id="table-task-report">
            <div class="p-datatable p-component p-datatable-hoverable-rows p-datatable-scrollable p-datatable-scrollable-vertical p-datatable-flex-scrollable p-datatable-responsive-scroll p-datatable-gridlines"
                data-scrollselectors=".p-datatable-wrapper" style="
              overflow: hidden auto;
              min-height: calc(100vh - 13rem);
              max-height: calc(100vh - 13rem);
            ">
                <div class="p-datatable-wrapper">
                    <table role="table" class="p-datatable-table" v-if="listTaskReportEvaluates.length > 0">
                        <thead class="p-datatable-thead" role="rowgroup" style="z-index: 100;">
                            <tr role="row">
                                <th class="align-items-center justify-content-center max-w-5rem" role="cell">
                                    <div class="p-column-header-content">
                                        <span class="p-column-title">STT</span>
                                    </div>
                                </th>
                                <th role="cell">
                                    <div class="p-column-header-content">
                                        <span class="p-column-title">Tên nhân sự</span>
                                    </div>
                                </th>
                                <th class="align-items-center justify-content-center max-w-20rem" role="cell">
                                    <div class="p-column-header-content">
                                        <span class="p-column-title">Số công việc thực hiện</span>
                                    </div>
                                </th>
                                <th class="align-items-center justify-content-center max-w-10rem" role="cell">
                                    <div class="p-column-header-content">
                                        <span class="p-column-title">Điểm đánh giá</span>
                                    </div>
                                </th>
                            </tr>
                        </thead>
                        <tbody class="p-datatable-tbody" role="rowgroup" v-for="(item, index) in listTaskReportEvaluates"
                            :key="index">
                            <div v-if="options.type_group == 0">
                                <table role="table" class="p-datatable-table-2">
                                    <tbody class="p-datatable-tbody" role="rowgroup">
                                        <tr class="" role="row" style="top: 42px">
                                            <td class="align-items-center justify-content-center text-center max-w-5rem">
                                                <div class="flex align-items-center gap-2">
                                                    <span>{{ item.STT }}</span>
                                                </div>
                                            </td>
                                            <td>
                                                <div class="flex align-items-center gap-2">
                                                    <span
                                                        style="width: 2rem;height:2rem;display: flex;align-items: center;justify-content: center;">
                                                        <i @click="openShowTask(item)" v-if="item.tasks.length > 0"
                                                            style="font-weight: bold; font-size: 16px;padding: 5px;"
                                                            class="icon-show-task" :class="item.isShow == false ? 'pi pi-angle-right' : 'pi pi-angle-down'
                                                                "></i>
                                                    </span>
                                                    <Avatar v-tooltip.bottom="{
                                                        value:
                                                            item.full_name +
                                                            '<br/>' +
                                                            (item.tenChucVu || '') +
                                                            '<br/>' +
                                                            (item.tenToChuc || ''),
                                                        escape: true,
                                                    }" v-bind:label="item.avatar
    ? ''
    : (item.last_name ?? '').substring(0, 1)
    " v-bind:image="basedomainURL + item.avatar" style="
              background-color: #2196f3;
              color: #ffffff;
              width: 2.5rem;
              height: 2.5rem;
              font-size: 15px !important;
            " :style="{
                background: bgColor[0] + '!important',
            }" class="cursor-pointer" size="xlarge" shape="circle" />
                                                    <span>{{ item.full_name }}</span>
                                                </div>
                                            </td>
                                            <td class="align-items-center justify-content-center text-center max-w-20rem">
                                                <div class="flex align-items-center gap-2">
                                                    <span>{{ item.tasks.length }}</span>
                                                </div>
                                            </td>
                                            <td class="align-items-center justify-content-center text-center max-w-10rem">
                                                <div class="flex align-items-center gap-2">
                                                    <span>{{ item.point_task }}</span>
                                                </div>
                                            </td>
                                        </tr>
                                        <tr class="" role="row" style="top: 42px"
                                            v-if="item.isShow == true && item.tasks.length > 0" v-for="t in item.tasks">
                                            <td class="align-items-center justify-content-center text-center max-w-5rem">
                                                <div class="flex align-items-center gap-2">
                                                    <span>{{ t.STT }}</span>
                                                </div>
                                            </td>
                                            <td style="padding-left: 40px !important;" colspan="2">
                                                <div class="flex align-items-center gap-2">
                                                    <div style="display: flex; flex-direction: column; padding: 5px">
                                                        <div style="line-height: 20px; display: flex">
                                                            <span v-tooltip="'Ưu tiên'" v-if="t.is_prioritize"
                                                                style="margin-right: 5px"><i style="color: orange"
                                                                    class="pi pi-star-fill"></i></span>
                                                            <span style="
                      font-weight: bold;
                      font-size: 14px;
                      overflow: hidden;
                      text-overflow: ellipsis;
                      width: 100%;
                      display: -webkit-box;
                      -webkit-line-clamp: 2;
                      -webkit-box-orient: vertical;
                    ">{{ t.task_name }}</span>
                                                        </div>
                                                        <div style="font-size: 12px; margin-top: 5px">
                                                            <span v-if="t.start_date || t.end_date"
                                                                style="color: #98a9bc">{{
                                                                    t.start_date }} - {{ t.end_date }}</span>
                                                        </div>
                                                        <div v-if="t.project_name" style="
                    min-height: 25px;
                    display: flex;
                    align-items: center;
                    margin-top: 10px;
                  ">
                                                            <i class="pi pi-tag"></i>
                                                            <span class="duan" style="
                      font-size: 13px;
                      font-weight: 400;
                      margin-left: 5px;
                      color: #0078d4;
                    ">{{ t.project_name }}</span>
                                                        </div>
                                                    </div>
                                                </div>
                                            </td>
                                            <!-- <td class="align-items-center justify-content-center text-center max-w-20rem">

                                            </td> -->
                                            <td class="align-items-center justify-content-center text-center max-w-10rem">
                                                <span>{{ t.point_task }}</span>
                                            </td>
                                        </tr>
                                    </tbody>
                                </table>
                            </div>
                            <div v-if="options.type_group == 1 || options.type_group == 2">
                                <table role="table" class="p-datatable-table-2">
                                    <tbody class="p-datatable-tbody" role="rowgroup">
                                        <tr class="" role="row" style="top: 42px">
                                            <td colspan="4" class="p-2"
                                                style="background-color: #f6ddcc; font-size: 14px; font-weight: bold;">
                                                <div class="flex align-items-center gap-2">
                                                    <span>{{ item.group_name }}</span>
                                                </div>
                                            </td>
                                        </tr>
                                    </tbody>
                                    <tbody class="p-datatable-tbody" role="rowgroup" v-for="(l, teamIndex) in item.Group"
                                        :key="teamIndex">
                                        <tr class="" role="row" style="top: 42px">
                                            <td class="align-items-center justify-content-center text-center max-w-5rem">
                                                <div class="flex align-items-center gap-2">
                                                    <span>{{ l.STT }}</span>
                                                </div>
                                            </td>
                                            <td>
                                                <div class="flex align-items-center gap-2">
                                                    <span
                                                        style="width: 2rem;height:2rem;display: flex;align-items: center;justify-content: center;">
                                                        <i @click="openShowTask(l)" v-if="l.tasks.length > 0"
                                                            style="font-weight: bold; font-size: 16px;padding: 5px;"
                                                            class="icon-show-task" :class="l.isShow == false ? 'pi pi-angle-right' : 'pi pi-angle-down'
                                                                "></i>
                                                    </span>
                                                    <Avatar v-tooltip.bottom="{
                                                        value:
                                                            l.full_name +
                                                            '<br/>' +
                                                            (l.tenChucVu || '') +
                                                            '<br/>' +
                                                            (l.tenToChuc || ''),
                                                        escape: true,
                                                    }" v-bind:label="l.avatar
    ? ''
    : (l.last_name ?? '').substring(0, 1)
    " v-bind:image="basedomainURL + l.avatar" style="
              background-color: #2196f3;
              color: #ffffff;
              width: 2.5rem;
              height: 2.5rem;
              font-size: 15px !important;
            " :style="{
                background: bgColor[0] + '!important',
            }" class="cursor-pointer" size="xlarge" shape="circle" />
                                                    <span>{{ l.full_name }}</span>
                                                </div>
                                            </td>
                                            <td class="align-items-center justify-content-center text-center max-w-20rem">
                                                <div class="flex align-items-center gap-2">
                                                    <span>{{ l.tasks.length }}</span>
                                                </div>
                                            </td>
                                            <td class="align-items-center justify-content-center text-center max-w-10rem">
                                                <div class="flex align-items-center gap-2">
                                                    <span>{{ l.point_task }}</span>
                                                </div>
                                            </td>
                                        </tr>
                                        <tr class="" role="row" style="top: 42px"
                                            v-if="l.isShow == true && l.tasks.length > 0" v-for="t in l.tasks">
                                            <td class="align-items-center justify-content-center text-center max-w-5rem">
                                                <div class="flex align-items-center gap-2">
                                                    <span>{{ t.STT }}</span>
                                                </div>
                                            </td>
                                            <td style="padding-left: 40px !important;" colspan="2">
                                                <div class="flex align-items-center gap-2">
                                                    <div style="display: flex; flex-direction: column; padding: 5px">
                                                        <div style="line-height: 20px; display: flex">
                                                            <span v-tooltip="'Ưu tiên'" v-if="t.is_prioritize"
                                                                style="margin-right: 5px"><i style="color: orange"
                                                                    class="pi pi-star-fill"></i></span>
                                                            <span style="
                      font-weight: bold;
                      font-size: 14px;
                      overflow: hidden;
                      text-overflow: ellipsis;
                      width: 100%;
                      display: -webkit-box;
                      -webkit-line-clamp: 2;
                      -webkit-box-orient: vertical;
                    ">{{ t.task_name }}</span>
                                                        </div>
                                                        <div style="font-size: 12px; margin-top: 5px">
                                                            <span v-if="t.start_date || t.end_date"
                                                                style="color: #98a9bc">{{
                                                                    t.start_date }} - {{ t.end_date }}</span>
                                                        </div>
                                                        <div v-if="t.project_name" style="
                    min-height: 25px;
                    display: flex;
                    align-items: center;
                    margin-top: 10px;
                  ">
                                                            <i class="pi pi-tag"></i>
                                                            <span class="duan" style="
                      font-size: 13px;
                      font-weight: 400;
                      margin-left: 5px;
                      color: #0078d4;
                    ">{{ t.project_name }}</span>
                                                        </div>
                                                    </div>
                                                </div>
                                            </td>
                                            <td class="align-items-center justify-content-center text-center max-w-10rem">
                                                <span>{{ t.point_task }}</span>
                                            </td>
                                        </tr>
                                    </tbody>
                                </table>
                            </div>
                        </tbody>
                    </table>
                    <table v-else role="table" class="p-datatable-table">
                        <thead class="p-datatable-thead" role="rowgroup" style="z-index: 100;">
                            <tr role="row">
                                <th class="align-items-center justify-content-center max-w-3rem" role="cell">
                                    <div class="p-column-header-content">
                                        <span class="p-column-title">STT</span>
                                    </div>
                                </th>
                                <th role="cell">
                                    <div class="p-column-header-content">
                                        <span class="p-column-title">Tên nhân sự</span>
                                    </div>
                                </th>
                                <th class="align-items-center justify-content-center max-w-20rem" role="cell">
                                    <div class="p-column-header-content">
                                        <span class="p-column-title">Số công việc thực hiện</span>
                                    </div>
                                </th>
                                <th class="align-items-center justify-content-center max-w-10rem" role="cell">
                                    <div class="p-column-header-content">
                                        <span class="p-column-title">Điểm đánh giá</span>
                                    </div>
                                </th>
                            </tr>
                        </thead>

                        <tbody class="p-datatable-tbody" role="rowgroup">
                            <tr class="p-datatable-emptymessage" role="row">
                                <td colspan="4">
                                    <div data-v-e7fddb26=""
                                        class="w-full align-items-center justify-content-center p-4 text-center">
                                        <img data-v-e7fddb26="" src="/src/assets/background/nodata.png" height="144" />
                                        <h3 data-v-e7fddb26="" class="m-1">
                                            Không có dữ liệu
                                        </h3>
                                    </div>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>
                <Paginator :rows="options.pageSize" :totalRecords="listTaskReportEvaluates.length" template="FirstPageLink PrevPageLink PageLinks NextPageLink
            LastPageLink RowsPerPageDropdown" :rowsPerPageOptions="[20, 100, 200, 300, 500]" @page="onPage" />
            </div>
        </div>
        <Dialog :header="headerDialogExportData" v-model:visible="DialogExportData" :closable="true" :maximizable="true"
            :style="{ width: '700px' }">
            <form>
                <div class="grid formgrid m-2">
                    <div class="field col-12 md:col-12">
                        <label>Chọn cột thông tin cần xuất</label>
                    </div>
                    <div class="field col-5 md:col-5 list-colum-exports" style="
              border: 1px solid #aaa;
              min-height: calc(100vh - 360px) !important;
              max-height: calc(100vh - 360px) !important;
              overflow: hidden;
            ">
                        <ul>
                            <li @click="chooseColumExport(l)" v-for="l in listColumExportOrigins" :key="l.STT"
                                style="display: flex">
                                <p style="flex: 1; margin: 0px">{{ l.text }}</p>
                                <span style="display: none"><i v-tooltip="'Select'" class="pi pi-reply"></i></span>
                            </li>
                        </ul>
                    </div>
                    <div class="field col-2 md:col-2">
                        <ul class="chucnang">
                            <li>
                                <i @click="chooseAllColum(true)" v-tooltip="'Chọn tất cả'"
                                    class="pi pi-angle-double-right"></i>
                            </li>
                            <li>
                                <i @click="chooseAllColum(false)" v-tooltip="'Bỏ chọn tất cả'"
                                    class="pi pi-angle-double-left"></i>
                            </li>
                        </ul>
                    </div>
                    <div class="field col-5 md:col-5 list-colum-exports" style="
              border: 1px solid #aaa;
              min-height: calc(100vh - 360px) !important;
              max-height: calc(100vh - 360px) !important;
              overflow: hidden;
            ">
                        <ul>
                            <li v-for="l in listColumExportNews" style="display: flex">
                                <p style="flex: 1; margin: 0px">{{ l.text }}</p>
                                <span @click="removeColumExport(l)" style="display: none"><i class="pi pi-times"></i></span>
                            </li>
                        </ul>
                    </div>
                </div>
            </form>
            <template #footer>
                <Button label="Hủy" icon="pi pi-times" @click="closeDialogExport" class="p-button-text" />

                <Button label="Xuất File" icon="pi pi-file" @click="downloadFile()" />
            </template>
        </Dialog>
    </div>
    <treeuser v-if="displayDialogUser === true" :headerDialog="headerDialogUser" :displayDialog="displayDialogUser"
        :one="is_one" :selected="selectedUser" :closeDialog="closeDialog" :choiceUser="choiceTreeUser" />
</template>
<style lang="scss" scope>
#request_group_by {
    min-width: fit-content !important;
}

#request_group_by .p-menuitem {
    padding: 5px 10px;
}

#request_group_by .p-menuitem:hover {
    cursor: pointer;
    background-color: #e9ecef;
}

#request_group_by .p-menuitem .active {
    color: #2196f3 !important;
}

#table-report-personal .fixcol {
    color: #000;
    font-weight: 600;
    position: sticky;
    /* background: #f5f5f5; */
    background-color: #f8f9fa;
    outline: 1px solid #e9e9e9;
    border: none;
    vertical-align: middle;
}

#table-report-personal th .fixcol {
    z-index: 5;
    border: 1px solid #e9e9e9 !important;
}

#table-report-personal td .fixcol {
    z-index: 4;
}

#table-report-personal .left-0 {
    left: 0px;
}

#table-report-personal .left-5 {
    left: 5rem;
}

#toolbar_right li {
    list-style: none;
    display: flex;
    justify-content: center;
    align-items: center;
    height: 30px;
    border: 1px solid;
    border-radius: 4px;
    margin: 0px 5px 0px 0px;
}

#toolbar_right li a {
    padding: 0px 10px;
}

#toolbar_right li:hover {
    cursor: pointer;
    background-color: #2196f3 !important;
    border: 1px solid #5ca7e3 !important;
    color: #fff;
}

#task_report_filter {
    width: 450px;
}

#task_report_filter ul li {
    list-style: none;
}

.p-dropdown-panel {
    width: 412px;
}

#task-report .p-datatable-flex-scrollable {
    height: 88% !important;
}

#task_sort,
#task_export {
    min-width: fit-content !important;
}

#task_sort .p-menuitem,
#task_export .p-menuitem {
    padding: 5px 10px;
}

#task_sort .p-menuitem:hover,
#task_export .p-menuitem:hover {
    cursor: pointer;
    background-color: #e9ecef;
}

#task_sort .p-menuitem .active,
#task_export .p-menuitem .active {
    color: #2196f3 !important;
}

.p-datatable table {
    min-height: calc(100vh - 165px);
}

.list-colum-exports ul {
    padding: 0px;
}

.list-colum-exports ul li {
    list-style: none;
    padding: 15px;
    margin: 10px 0px;
    background-color: #f3f2f1;
}

.list-colum-exports ul li:hover {
    cursor: pointer;
    background-color: aliceblue !important;
    color: #2196f3 !important;
}

.list-colum-exports ul li:hover span {
    display: block !important;
}

.scroll-inner,
.scroll-outer:hover,
.scroll-outer:focus {
    visibility: visible;
    overflow: auto;
}

.list-colum-exports:hover {
    overflow: auto !important;
}

.chucnang {
    padding: 0;
}

.chucnang li {
    list-style: none;
    padding: 10px;
    text-align: center;
}

.chucnang li i {
    font-size: 20px;
}

.chucnang li i:hover {
    cursor: pointer;
    color: #2196f3 !important;
}

#task_filter .hover-tree-user:hover {
    cursor: pointer;
}

#table-task-report .p-treetable.p-treetable-hoverable-rows .p-treetable-tbody>tr:not(.p-highlight):hover,
#table-task-report .p-datatable.p-datatable-hoverable-rows .p-datatable-tbody>tr:not(.p-highlight) .icon-show-task:hover {
    cursor: pointer;
    background-color: #f2f2f2;
    border-radius: 50%;
}

.p-datatable-table-2 {
    min-height: fit-content !important;
}

#toolbar_right .active {
    background-color: #2196f3 !important;
    border: 1px solid #5ca7e3 !important;
    color: #fff;
}
</style>
