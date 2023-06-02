<script setup>
import { ref, inject, onMounted, watch, onBeforeUnmount } from "vue";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
import { required } from "@vuelidate/validators";

import moment from "moment";
import { concat } from "lodash";
import { encr } from "../../util/function.js";
import router from "@/router";
import ModalListUserDepartmentVue from "../department_configuration/ModalListUserDepartment.vue";
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
  opition.value.sort = sort;
  opition.value.ob = ob;
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

const onRefresh = () => {
  opition.value = {
    IsNext: true,
    sort: "created_date",
    ob: "DESC",
    PageNo: 1,
    PageSize: 20,
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
  };
  loadData(true);
};

const opition = ref({
  IsNext: true,
  sort: "created_date",
  ob: "DESC",
  PageNo: 1,
  PageSize: 20,
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
});
const listDropdownProject = ref([]);
const listDropdownVaitro = ref([
  { value: 3, text: "Theo dõi" },
  { value: 1, text: "Đang làm" },
  { value: 0, text: "Quản lý" },
]);
const listTaskReportPersonal = ref();
const renderTree = (data, id, name, title) => {
  let arrChils = [];
  let arrtreeChils = [];
  data
    .filter((x) => x.parent_id == null)
    .forEach((m, i) => {
      m.IsOrder = i + 1;
      m.label_order = m.IsOrder.toString();
      if (opition.value.PageNo > 0) {
        m.STT = (opition.value.PageNo - 1) * opition.value.PageSize + i + 1;
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
    opition.value.loading = true;
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
            proc: "task_report_personal_list_new",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "pageno", va: opition.value.PageNo },
              { par: "pagesize", va: opition.value.PageSize },
              { par: "search", va: opition.value.search },
              { par: "sort", va: opition.value.sort },
              { par: "ob", va: opition.value.ob },
              { par: "IsType", va: opition.value.IsType },
              { par: "loc", va: opition.value.filter_type },
              { par: "sdate", va: opition.value.sdate },
              { par: "edate", va: opition.value.edate },
              { par: "filterDuan", va: opition.value.filterDuan },
              { par: "filterStatus", va: opition.value.filterStatus },
              { par: "filterUser", va: opition.value.filterUser },
              { par: "is_reportmodule", va: is_reportmodule },
              { par: "report_module_id", va: report_module_id },
              { par: "type_report", va: is_permission },
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
          d.status_name =
            d.status != null
              ? listDropdownStatus.value.filter((x) => x.value == d.status)[0]
                  .text
              : "";
          d.STT = i + 1;
        });
        listTaskReportPersonal.value = data[0];
      } else {
        listTaskReportPersonal.value = [];
      }
      opition.value.totalRecords = data[1].totalRecords;
      if (rf) {
        opition.value.loading = false;
        swal.close();
      }
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      opition.value.loading = false;
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
const menuFilterButs = ref();
const menuSortButs = ref();
const menuExportButs = ref();
const toggleFilter = (event) => {
  menuFilterButs.value.toggle(event);
};
const toggleSort = (event) => {
  menuSortButs.value.toggle(event);
};
const toggleExport = (event) => {
  menuExportButs.value.toggle(event);
};
const Del_ChangeFilter = () => {
  menuFilterButs.value.toggle();
  opition.value = {
    IsNext: true,
    sort: "created_date",
    ob: "DESC",
    PageNo: 1,
    PageSize: 20,
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
              { par: "search", va: opition.value.SearchTextUser },
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
      opition.value.loading = false;

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
      opition.value.loading = false;

      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};

const ChangeFilter = () => {
  loadData(true);
  menuFilterButs.value.toggle();
};

onMounted(() => {
  listTudien();
  listUser();
  loadData(true);
  return {};
});

const ExportData = () => {
  var header =
    "<html xmlns:o='urn:schemas-microsoft-com:office:office' " +
    "xmlns:w='urn:schemas-microsoft-com:office:word' " +
    "xmlns='http://www.w3.org/TR/REC-html40'>" +
    "<head><meta charset='utf-8'><title>Export HTML to Word Document with JavaScript</title></head><body>";
  var footer = "</body></html>";
  // var sourceHTML = header+document.querySelector("#table-report-department .p-datatable-wrapper").innerHTML+footer;
  let data = [...listTaskReportPersonal.value];
  data = groupBy(data, "Tennhom");
  var arrNew = [];
  for (let k in data) {
    var Group = [];
    data[k].forEach(function (r) {
      Group.push(r);
    });
    arrNew.push({
      Tennhom: k,
      Group: Group,
    });
  }
  var htmltable = "";
  htmltable +=
    "<table border='0' width='100%' cellpadding='10' style='border-collapse:collapse;' colspan='" +
    listColumExportNews.value.length +
    "'>";
  htmltable += "<thead>";
  htmltable +=
    "<tr><th colspan='15' style='font-size:16px;text-align:center;'>BÁO CÁO THỐNG KÊ TỔNG HỢP CÔNG VIỆC CÁ NHÂN</th></tr></thead></table>";

  htmltable +=
    "<table border='1' width='100%' cellpadding='10' style='border-collapse:collapse;' colspan='" +
    listColumExportNews.value.length +
    "'>";
  htmltable += "<thead>";
  htmltable += "<tr>";
  htmltable +=
    "<th style='width:30px;font-size:14px;text-align:center;'>STT</th>";
  if (
    listColumExportNews.value.filter((x) => x.value == "task_name").length > 0
  ) {
    htmltable +=
      "<th style='width: 150px;font-size:14px;text-align: center;'> Tên công việc</th>";
  }
  if (
    listColumExportNews.value.filter((x) => x.value == "project_name").length >
    0
  ) {
    htmltable +=
      "<th style='width: 150px;font-size:14px;text-align: center;'> Thuộc dự án</th>";
  }
  if (
    listColumExportNews.value.filter((x) => x.value == "Nguoigiaoviecs")
      .length > 0
  ) {
    htmltable +=
      "<th style='width: 150px;font-size:14px;text-align: center;'> Người giao việc</th>";
  }
  if (
    listColumExportNews.value.filter((x) => x.value == "Nguoithuchiens")
      .length > 0
  ) {
    htmltable +=
      "<th style='width: 150px;font-size:14px;text-align: center;'> Người thực hiện</th>";
  }
  if (
    listColumExportNews.value.filter((x) => x.value == "description").length > 0
  ) {
    htmltable +=
      "<th style='width: 150px;font-size:14px;text-align: center;'> Mô tả</th>";
  }
  if (listColumExportNews.value.filter((x) => x.value == "target").length > 0) {
    htmltable +=
      "<th style='width: 150px;font-size:14px;text-align: center;'> Mục tiêu</th>";
  }
  if (
    listColumExportNews.value.filter((x) => x.value == "progress1").length > 0
  ) {
    htmltable +=
      "<th style='width: 150px;font-size:14px;text-align: center;'> Kết quả đạt được</th>";
  }
  if (
    listColumExportNews.value.filter((x) => x.value == "start_date").length > 0
  ) {
    htmltable +=
      "<th style='width: 150px;font-size:14px;text-align: center;'> Ngày bắt đầu</th>";
  }
  if (
    listColumExportNews.value.filter((x) => x.value == "end_date").length > 0
  ) {
    htmltable +=
      "<th style='width: 150px;font-size:14px;text-align: center;'> Ngày kết thúc</th>";
  }
  if (
    listColumExportNews.value.filter((x) => x.value == "SoNgaygiahan").length >
    0
  ) {
    htmltable +=
      "<th style='width: 150px;font-size:14px;text-align: center;'> Số ngày gia hạn</th>";
  }
  if (
    listColumExportNews.value.filter((x) => x.value == "progress2").length > 0
  ) {
    htmltable +=
      "<th style='width: 150px;font-size:14px;text-align: center;'> Tiến độ</th>";
  }
  if (
    listColumExportNews.value.filter((x) => x.value == "status_name").length > 0
  ) {
    htmltable +=
      "<th style='width: 150px;font-size:14px;text-align: center;'> Trạng thái</th>";
  }
  if (
    listColumExportNews.value.filter((x) => x.value == "Nguoitheodois").length >
    0
  ) {
    htmltable +=
      "<th style='width: 150px;font-size:14px;text-align: center;'> Người theo dõi</th>";
  }
  htmltable += "</tr>";
  htmltable += "</thead>";
  htmltable += "<tbody>";
  if (arrNew.length > 0) {
    arrNew.forEach(function (t, index) {
      htmltable +=
        "<tr><th colspan='" +
        listColumExportNews.value.length +
        "' style='text-align: left;'>" +
        (t.Tennhom != "null" ? t.Tennhom : "") +
        "</th></tr>";
      t.Group.forEach(function (d, index) {
        htmltable += "<tr>";
        htmltable +=
          "<td style='width:30px;font-size:14px;text-align:center;'>" +
          d.STT +
          "</td>";
        if (
          listColumExportNews.value.filter((x) => x.value == "task_name")
            .length > 0
        ) {
          htmltable +=
            "<td style='width: 150px;font-size:14px;'>" +
            (d.task_name != null ? d.task_name : "") +
            "</td>";
        }
        if (
          listColumExportNews.value.filter((x) => x.value == "project_name")
            .length > 0
        ) {
          htmltable +=
            "<td style='width: 150px;font-size:14px;text-align:center;'>" +
            (d.project_name != null ? d.project_name : "") +
            "</td>";
        }
        if (
          listColumExportNews.value.filter((x) => x.value == "Nguoigiaoviecs")
            .length > 0
        ) {
          htmltable +=
            "<td style='width: 150px;font-size:14px;'>" +
            (d.Nguoigiaoviecs != null ? d.Nguoigiaoviecs : "") +
            "</td>";
        }
        if (
          listColumExportNews.value.filter((x) => x.value == "Nguoithuchiens")
            .length > 0
        ) {
          htmltable +=
            "<td style='width: 150px;font-size:14px;'>" +
            (d.Nguoithuchiens != null ? d.Nguoithuchiens : "") +
            "</td>";
        }
        if (
          listColumExportNews.value.filter((x) => x.value == "description")
            .length > 0
        ) {
          htmltable +=
            "<td style='width: 150px;font-size:14px;'>" +
            (d.description != null ? d.description : "") +
            "</td>";
        }
        if (
          listColumExportNews.value.filter((x) => x.value == "target").length >
          0
        ) {
          htmltable +=
            "<td style='width: 150px;font-size:14px;'>" +
            (d.target != null ? d.target : "") +
            "</td>";
        }
        if (
          listColumExportNews.value.filter((x) => x.value == "progress1")
            .length > 0
        ) {
          htmltable +=
            "<td style='width: 150px;font-size:14px;text-align:center;'>" +
            (d.progress != null ? d.progress : "") +
            "</td>";
        }
        if (
          listColumExportNews.value.filter((x) => x.value == "start_date")
            .length > 0
        ) {
          htmltable +=
            "<td style='width: 150px;font-size:14px;text-align:center;'>" +
            (d.start_date != null
              ? moment(new Date(d.start_date)).format("DD/MM/YYYY")
              : "") +
            "</td>";
        }
        if (
          listColumExportNews.value.filter((x) => x.value == "end_date")
            .length > 0
        ) {
          htmltable +=
            "<td style='width: 150px;font-size:14px;text-align:center;'>" +
            (d.end_date
              ? moment(new Date(d.end_date)).format("DD/MM/YYYY")
              : "") +
            "</td>";
        }
        if (
          listColumExportNews.value.filter((x) => x.value == "SoNgaygiahan")
            .length > 0
        ) {
          htmltable +=
            "<td style='width: 150px;font-size:14px;'>" +
            (d.SoNgaygiahan != null ? d.SoNgaygiahan : "") +
            "</td>";
        }
        if (
          listColumExportNews.value.filter((x) => x.value == "progress2")
            .length > 0
        ) {
          htmltable +=
            "<td style='width: 150px;font-size:14px;text-align:center;'>" +
            (d.progress != null ? d.progress : "") +
            "</td>";
        }
        if (
          listColumExportNews.value.filter((x) => x.value == "status_name")
            .length > 0
        ) {
          htmltable +=
            "<td style='width: 150px;font-size:14px;text-align:center;'>" +
            (d.status_name != null ? d.status_name : "") +
            "</td>";
        }
        if (
          listColumExportNews.value.filter((x) => x.value == "Nguoitheodois")
            .length > 0
        ) {
          htmltable +=
            "<td style='width: 150px;font-size:14px;'>" +
            (d.Nguoitheodois != null ? d.Nguoitheodois : "") +
            "</td>";
        }
        htmltable += "</tr>";
      });
    });
  }
  htmltable += "</tbody>";

  htmltable += "</table>";
  var source =
    "data:application/vnd.ms-word;charset=utf-8," +
    encodeURIComponent(header + htmltable + footer);
  var fileDownload = document.createElement("a");
  document.body.appendChild(fileDownload);
  fileDownload.href = source;
  fileDownload.download = "BC_DSCV_CaNhan.doc";
  fileDownload.click();
  document.body.removeChild(fileDownload);
};

const DialogExportData = ref(false);
const headerDialogExportData = ref(false);
const listColumExportNews = ref([]);
const listColumExportOrigins = ref([]);
const listColumExports = ref([
  { value: "task_name", text: "Tên công việc", STT: 1 },
  { value: "project_name", text: "Thuộc dự án", STT: 2 },
  { value: "Nguoigiaoviecs", text: "Người giao việc", STT: 3 },
  { value: "Nguoithuchiens", text: "Người thực hiện", STT: 4 },
  { value: "description", text: "Mô tả", STT: 5 },
  { value: "target", text: "Mục tiêu", STT: 6 },
  { value: "progress1", text: "Kết quả đạt được", STT: 7 },
  { value: "start_date", text: "Ngày bắt đầu", STT: 8 },
  { value: "end_date", text: "Ngày kết thúc", STT: 9 },
  { value: "SoNgaygiahan", text: "Số ngày gia hạn", STT: 10 },
  { value: "progress2", text: "Tiến độ", STT: 11 },
  { value: "status_name", text: "Trạng thái", STT: 12 },
  { value: "Nguoitheodois", text: "Người theo dõi", STT: 13 },
]);
const openExportData = () => {
  listColumExportNews.value = [];
  listColumExportOrigins.value = [...listColumExports.value];
  DialogExportData.value = true;
  headerDialogExportData.value = "Kết xuất ra file Word";
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
      listTaskReportPersonal.value,
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
  if (event.rows != opition.value.PageSize) {
    opition.value.PageSize = event.rows;
  }
  if (event.page == 0) {
    //Trang đầu
    opition.value.id = null;
    opition.value.IsNext = true;
  } else if (event.page > opition.value.PageNo + 1) {
    //Trang cuối
    opition.value.id = -1;
    opition.value.IsNext = false;
  } else if (event.page > opition.value.PageNo) {
    //Trang sau

    opition.value.id =
      listTaskReportPersonal.value[
        listTaskReportPersonal.value.length - 1
      ].task_id;
    opition.value.IsNext = true;
  } else if (event.page < opition.value.PageNo) {
    //Trang trước
    opition.value.id = listTaskReportPersonal.value[0].task_id;
    opition.value.IsNext = false;
  }
  opition.value.PageNo = event.page;
  loadData(true);
};
</script>
<template>
  <div
    v-if="store.getters.islogin"
    class="main-layout true flex-grow-1 p-2"
    id="task-report"
  >
    <div class="flex justify-content-center align-items-center">
      <Toolbar class="w-full custoolbar">
        <template #start>
          <div class="flex justify-content-center align-items-center">
            <span class="p-input-icon-left">
              <i class="pi pi-search" />
              <InputText
                style="min-width: 300px; margin-right: 10px"
                type="text"
                spellcheck="false"
                v-model="opition.search"
                placeholder="Tìm kiếm"
                @keyup.enter="loadData(true)"
              />
            </span>
            <h3>BÁO CÁO THỐNG KÊ TỔNG HỢP CÔNG VIỆC CÁ NHÂN</h3>
          </div>
        </template>
        <template #end>
          <ul
            id="toolbar_right"
            style="padding: 0px; margin: 0px; display: flex"
          >
            <li @click="onRefresh">
              <a><i class="pi pi-refresh"></i> Tải lại</a>
            </li>
            <li
              @click="toggleFilter"
              aria-haspopup="true"
              :class="{ active: opition.filter_type != 0 }"
              aria-controls="overlay_Export"
            >
              <a
                ><i
                  style="margin-right: 5px"
                  class="pi pi-filter"
                ></i
                >{{ opition.loctitle
                }}<i
                  style="margin-left: 5px"
                  class="pi pi-angle-down"
                ></i
              ></a>
            </li>
            <li
              @click="toggleSort"
              :class="{ active: opition.sort }"
              aria-haspopup="true"
              aria-controls="overlay_Export"
            >
              <a
                ><i class="pi pi-sort"></i> Sắp xếp
                <i class="pi pi-angle-down"></i
              ></a>
            </li>
            <li
              @click="toggleExport"
              aria-haspopup="true"
              aria-controls="overlay_Export"
            >
              <a
                ><i class="pi pi-file"></i> Tiện ích
                <i class="pi pi-angle-down"></i
              ></a>
            </li>
          </ul>
          <OverlayPanel
            ref="menuFilterButs"
            id="task_report_filter"
          >
            <ul style="padding: 0px; margin: 0px">
              <li
                class="p-menuitem"
                v-if="store.state.user.is_admin == true"
              >
                <div
                  class="field col-12 md:col-12"
                  style="display: flex; flex-direction: column"
                >
                  <label>Người dùng</label>
                  <Dropdown
                    :filter="true"
                    @change="changeDonvi($event)"
                    v-model="opition.filterUser"
                    panelClass="d-design-dropdown"
                    placeholder="Chọn người dùng"
                    selectionLimit="1"
                    :options="listDropdownUser"
                    optionLabel="name"
                    optionValue="code"
                    spellcheck="false"
                    class="col-12 ip36 p-0"
                  >
                    <template #option="slotProps">
                      <div style="display: flex; align-items: center">
                        <Avatar
                          v-bind:label="
                            slotProps.option.avatar
                              ? ''
                              : (slotProps.option.ten ?? '').substring(0, 1)
                          "
                          v-bind:image="basedomainURL + slotProps.option.avatar"
                          style="
                            background-color: #2196f3;
                            color: #ffffff;
                            width: 2.5rem;
                            height: 2.5rem;
                            font-size: 15px !important;
                          "
                          :style="{
                            background:
                              bgColor[slotProps.option.stt % 7] + '!important',
                          }"
                          class="cursor-pointer"
                          size="xlarge"
                          shape="circle"
                        />
                        <div
                          class="pt-1"
                          style="margin-left: 10px"
                        >
                          {{ slotProps.option.name }}
                        </div>
                      </div>
                    </template>
                  </Dropdown>
                </div>
              </li>
              <li class="p-menuitem">
                <div
                  class="field col-12 md:col-12"
                  style="display: flex; flex-direction: column"
                >
                  <label>Dự án</label>
                  <Dropdown
                    :filter="true"
                    v-model="opition.filterDuan"
                    panelClass="d-design-dropdown"
                    placeholder="Chọn dự án"
                    selectionLimit="1"
                    :options="listDropdownProject"
                    optionLabel="project_name"
                    optionValue="project_id"
                    spellcheck="false"
                    class="col-12 ip36 p-0"
                  >
                    <template #option="slotProps">
                      <div class="pt-1">
                        {{ slotProps.option.project_name }}
                      </div>
                    </template>
                  </Dropdown>
                </div>
              </li>
              <li class="p-menuitem">
                <div
                  class="field col-12 md:col-12"
                  style="display: flex; flex-direction: column; margin: 0px"
                >
                  <label>Từ ngày - đến ngày</label>
                  <div class="grid formgrid">
                    <div class="field col-6 md:col-6">
                      <Calendar
                        inputId="icon"
                        class="col-12 ip36 px-2"
                        placeholder="dd/MM/yy"
                        v-model="opition.sdate"
                        :showIcon="true"
                      />
                    </div>
                    <div class="field col-6 md:col-6">
                      <Calendar
                        inputId="icon"
                        class="col-12 ip36 px-2"
                        placeholder="dd/MM/yy"
                        v-model="opition.edate"
                        :showIcon="true"
                      />
                    </div>
                  </div>
                </div>
              </li>
              <li class="p-menuitem">
                <div
                  class="field col-12 md:col-12"
                  style="display: flex; flex-direction: column"
                >
                  <label>Trạng thái công việc</label>
                  <Dropdown
                    :filter="true"
                    panelClass="d-design-dropdown"
                    v-model="opition.filterStatus"
                    placeholder="Chọn trạng thái"
                    selectionLimit="1"
                    :options="listDropdownStatus"
                    optionLabel="text"
                    optionValue="value"
                    spellcheck="false"
                    class="col-12 ip36 p-0"
                  >
                    <template #option="slotProps">
                      <div class="pt-1">{{ slotProps.option.text }}</div>
                    </template>
                  </Dropdown>
                </div>
              </li>
              <li class="p-menuitem">
                <div
                  class="field col-12 md:col-12"
                  style="display: flex; flex-direction: column"
                >
                  <label>Vai trò</label>
                  <Dropdown
                    :filter="true"
                    panelClass="d-design-dropdown"
                    v-model="opition.IsType"
                    placeholder="Chọn vai trò"
                    selectionLimit="1"
                    :options="listDropdownVaitro"
                    optionLabel="text"
                    optionValue="value"
                    spellcheck="false"
                    class="col-12 ip36 p-0"
                  >
                    <template #option="slotProps">
                      <div class="pt-1">{{ slotProps.option.text }}</div>
                    </template>
                  </Dropdown>
                </div>
              </li>
            </ul>
            <div style="float: right; padding: 10px">
              <Button
                @click="ChangeFilter()"
                label="Thực hiện"
              />
              <Button
                @click="Del_ChangeFilter"
                id="btn_huy"
                style="
                  background-color: #f2f4f6;
                  border: 1px solid #f2f4f6;
                  color: #333;
                  margin-left: 10px;
                "
                label="Hủy lọc"
              />
            </div>
          </OverlayPanel>
          <Menu
            id="task_sort"
            :model="itemSortButs"
            ref="menuSortButs"
            :popup="true"
          >
            <template #item="{ item }">
              <a
                @click="ChangeSortTask(item.sort, item.ob)"
                :class="{ active: item.active }"
                >{{ item.label }}</a
              >
            </template>
          </Menu>
          <Menu
            id="task_export"
            :model="itemExportButs"
            ref="menuExportButs"
            :popup="true"
          >
            <template #item="{ item }">
              <a @click="ChangeExportTask(item)">{{ item.label }}</a>
            </template>
          </Menu>
        </template>
      </Toolbar>
    </div>

    <DataTable
      :showGridlines="true"
      :paginator="true"
      :rows="opition.PageSize"
      @page="onPage($event)"
      :totalRecords="opition.totalRecords"
      paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
      dataKey="task_id"
      :rowsPerPageOptions="[20, 30, 50, 100, 200]"
      id="table-report-personal"
      :value="listTaskReportPersonal"
      rowGroupMode="subheader"
      groupRowsBy="Tennhom"
      sortMode="single"
      :sortOrder="1"
      scrollable
      style="max-height: calc(100vh - 145px); min-height: calc(100vh - 145px)"
      scrollHeight="calc(100vh - 170px)"
    >
      <Column
        field="STT"
        class="align-items-center justify-content-center text-center fixcol left-0"
        header="STT"
        headerStyle="min-height:3.125rem;min-width: 5rem;"
        bodyStyle="min-width: 5rem;"
      ></Column>
      <Column
        field="task_name" class="fixcol left-5"
        header="Tên công việc"
        headerStyle="min-height:3.125rem;min-width: 25rem;"
        bodyStyle="min-width: 25rem;"
      ></Column>
      <Column
        field="project_name"
        class="align-items-center justify-content-center text-center"
        header="Thuộc dự án"
        headerStyle="min-height:3.125rem;min-width: 10rem;"
        bodyStyle="min-width: 10rem;"
      >
      </Column>
      <Column
        header="Người giao việc"
        class="align-items-center justify-content-center text-center"
        headerStyle="min-height:3.125rem;min-width: 10rem;"
        bodyStyle="min-width: 10rem;"
      >
        <template #body="data">
          <span
            style="
              overflow: hidden;
              text-overflow: ellipsis;
              width: 100%;
              display: -webkit-box;
              -webkit-line-clamp: 2;
              -webkit-box-orient: vertical;
            "
            >{{ data.data.Nguoigiaoviecs }}</span
          >
        </template>
      </Column>
      <Column
        header="Người thực hiện"
        headerStyle="min-height:3.125rem;min-width: 40rem;"
        bodyStyle="min-width: 40rem;"
      >
        <template #body="data">
          <span
            style="
              overflow: hidden;
              text-overflow: ellipsis;
              width: 100%;
              display: -webkit-box;
              -webkit-line-clamp: 2;
              -webkit-box-orient: vertical;
            "
            >{{ data.data.Nguoithuchiens }}</span
          >
        </template>
      </Column>
      <Column
        field="description"
        header="Mô tả"
        class="align-items-center justify-content-center text-center"
        headerStyle="min-height:3.125rem;min-width: 10rem;"
        bodyStyle="min-width: 10rem;"
      >
      </Column>
      <Column
        field="target"
        header="Mục tiêu"
        class="align-items-center justify-content-center text-center"
        headerStyle="min-height:3.125rem;min-width: 10rem;"
        bodyStyle="min-width: 10rem;"
      >
      </Column>
      <Column
        field="progress"
        header="Kết quả đạt được"
        class="align-items-center justify-content-center text-center"
        headerStyle="min-height:3.125rem;min-width: 10rem;"
        bodyStyle="min-width: 10rem;"
      >
      </Column>
      <Column
        header="Ngày bắt đầu"
        class="align-items-center justify-content-center text-center"
        headerStyle="min-height:3.125rem;min-width: 10rem;"
        bodyStyle="min-width: 10rem;"
      >
        <template #body="data">
          <span v-if="data.data.start_date">{{
            moment(new Date(data.data.start_date)).format("DD/MM/YYYY")
          }}</span>
        </template>
      </Column>
      <Column
        header="Ngày kết thúc"
        class="align-items-center justify-content-center text-center"
        headerStyle="min-height:3.125rem;min-width: 10rem;"
        bodyStyle="min-width: 10rem;"
      >
        <template #body="data">
          <span v-if="data.data.end_date">{{
            moment(new Date(data.data.end_date)).format("DD/MM/YYYY")
          }}</span>
        </template>
      </Column>
      <Column
        field="SoNgaygiahan"
        header="Số ngày gia hạn"
        class="align-items-center justify-content-center text-center"
        headerStyle="min-height:3.125rem;min-width: 10rem;"
        bodyStyle="min-width: 10rem;"
      >
      </Column>
      <Column
        header="Tiến độ"
        class="align-items-center justify-content-center text-center"
        headerStyle="min-height:3.125rem;min-width: 10rem;"
        bodyStyle="min-width: 10rem;"
      >
        <template #body="data">
          <span>{{ data.data.progress }}</span>
        </template>
      </Column>
      <Column
        header="Trạng thái"
        class="align-items-center justify-content-center text-center"
        headerStyle="min-height:3.125rem;min-width: 10rem;"
        bodyStyle="min-width: 10rem;"
      >
        <template #body="data">
          <span>{{ data.data.status_name }}</span>
        </template>
      </Column>
      <Column
        header="Người theo dõi"
        headerStyle="min-height:3.125rem;min-width: 40rem;"
        bodyStyle="min-width: 40rem;"
      >
        <template #body="data">
          <span
            style="
              overflow: hidden;
              text-overflow: ellipsis;
              width: 100%;
              display: -webkit-box;
              -webkit-line-clamp: 2;
              -webkit-box-orient: vertical;
            "
            >{{ data.data.Nguoitheodois }}</span
          >
        </template>
      </Column>
      <template #groupheader="slotProps">
        <span class="image-text">{{ slotProps.data.Tennhom }}</span>
      </template>
      <template #empty>
        <div
          class="align-items-center justify-content-center p-4 text-center m-auto"
          style="
            min-height: calc(100vh - 190px);
            max-height: calc(100vh - 190px);
            display: flex;
            flex-direction: column;
          "
          v-if="listTaskReportPersonal != null"
        >
          <img
            src="../../assets/background/nodata.png"
            height="144"
          />
          <h3 class="m-1">Không có dữ liệu</h3>
        </div>
      </template>
    </DataTable>

    <Dialog
      :header="headerDialogExportData"
      v-model:visible="DialogExportData"
      :closable="true"
      :maximizable="true"
      :style="{ width: '700px' }"
    >
      <form>
        <div class="grid formgrid m-2">
          <div class="field col-12 md:col-12">
            <label>Chọn cột thông tin cần xuất</label>
          </div>
          <div
            class="field col-5 md:col-5 list-colum-exports"
            style="
              border: 1px solid #aaa;
              min-height: calc(100vh - 360px) !important;
              max-height: calc(100vh - 360px) !important;
              overflow: hidden;
            "
          >
            <ul>
              <li
                @click="chooseColumExport(l)"
                v-for="l in listColumExportOrigins"
                :key="l.STT"
                style="display: flex"
              >
                <p style="flex: 1; margin: 0px">{{ l.text }}</p>
                <span style="display: none"
                  ><i
                    v-tooltip="'Select'"
                    class="pi pi-reply"
                  ></i
                ></span>
              </li>
            </ul>
          </div>
          <div class="field col-2 md:col-2">
            <ul class="chucnang">
              <li>
                <i
                  @click="chooseAllColum(true)"
                  v-tooltip="'Chọn tất cả'"
                  class="pi pi-angle-double-right"
                ></i>
              </li>
              <li>
                <i
                  @click="chooseAllColum(false)"
                  v-tooltip="'Bỏ chọn tất cả'"
                  class="pi pi-angle-double-left"
                ></i>
              </li>
            </ul>
          </div>
          <div
            class="field col-5 md:col-5 list-colum-exports"
            style="
              border: 1px solid #aaa;
              min-height: calc(100vh - 360px) !important;
              max-height: calc(100vh - 360px) !important;
              overflow: hidden;
            "
          >
            <ul>
              <li
                v-for="l in listColumExportNews"
                style="display: flex"
              >
                <p style="flex: 1; margin: 0px">{{ l.text }}</p>
                <span
                  @click="removeColumExport(l)"
                  style="display: none"
                  ><i class="pi pi-times"></i
                ></span>
              </li>
            </ul>
          </div>
        </div>
      </form>
      <template #footer>
        <Button
          label="Hủy"
          icon="pi pi-times"
          @click="closeDialogExport"
          class="p-button-text"
        />

        <Button
          label="Xuất Word"
          icon="pi pi-file"
          @click="ExportData()"
        />
      </template>
    </Dialog>
  </div>
</template>
<style lang="scss" scope>
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
#table-report-personal th .fixcol{
  z-index: 5;
  border: 1px solid #e9e9e9 !important;
}
#table-report-personal td .fixcol{
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
</style>
