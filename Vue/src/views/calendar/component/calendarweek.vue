<script setup>
import { ref, inject, onMounted } from "vue";
import { encr } from "../../../util/function";
import moment from "moment";
import { socketMethod } from "../../../util/methodSocket";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import dialogmodelweek from "../component/dialogmodelweek.vue";
import dialogmodelmultipleweek from "./dialogmodelmultipleweek.vue";
import dialogsend from "../component/dialogsend.vue";
import dialogapprove from "../component/dialogapprove.vue";
import dialogenact from "../component/dialogenact.vue";
import dialogchart from "../component/dialogchart.vue";
import dialogcoincide from "../component/dialogcoincide.vue";
import frameprintweek from "../component/frameprintweek.vue";
import frameprintweek2 from "../component/frameprintweek2.vue";
import framewordweek from "../component/framewordweek.vue";
import framewordweek2 from "../component/framewordweek2.vue";
import calendardutysunday from "../component/calendardutysunday.vue";
import calendarleader from "../component/calendarleader.vue";

const cryoptojs = inject("cryptojs");
const router = inject("router");
const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios");
const emitter = inject("emitter");
const isDynamicSQL = ref(false);
const basedomainURL = baseURL;
const config = {
  headers: {
    Authorization: `Bearer ${store.getters.token}`,
  },
};
const rules = {
  contents: {
    required,
    $errors: [
      {
        $property: "contents",
        $validator: "required",
        $message: "Nội dung cuộc họp không được để trống!",
      },
    ],
  },
  start_date: {
    required,
    $errors: [
      {
        $property: "start_date",
        $validator: "required",
        $message: "Ngày bắt đầu không được để trống!",
      },
    ],
  },
  end_date: {
    required,
    $errors: [
      {
        $property: "end_date",
        $validator: "required",
        $message: "Ngày kết thúc không được để trống!",
      },
    ],
  },
  boardroom_id: {
    required,
    $errors: [
      {
        $property: "boardroom_id",
        $validator: "required",
        $message: "Phòng họp không được để trống!",
      },
    ],
  },
};
const toast = useToast();

//Get arguments
const props = defineProps({
  group: Number,
  state: Number, //1 Đặt lịch, 2 Chờ duyệt, 3 Theo dõi
});

//Declare
const isFirst = ref(true);
const datas = ref([]);
const datadays = ref([]);
const datachutris = ref([]);
const holiday = ref({});
const selectedKeys = ref([]);
const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);
const options = ref({
  loc: 0,
  title:
    props.group === 0
      ? "lịch họp tuần"
      : props.group === 1
      ? "lịch công tác"
      : "",
  is_group: props.group,
  week_start_date: null,
  week_end_date: null,
  view: 1,
  loading: false,
  user_id: store.getters.user.user_id,
  today: new Date(),
  week: 0,
  month: new Date().getMonth() + 1,
  year: new Date().getFullYear(),
  is_type: props.state, //0 Đặt lịch, 1: chờ duyệt, 2 theo dõi
  search: "",
  total: 0,
  sort: "created_date desc",
  orderBy: "desc",
});
const groups = ref([
  { view: 0, icon: "", title: "Tất cả các lịch" },
  { view: 1, icon: "", title: "Xem theo tuần" },
  { view: 2, icon: "", title: "Xem theo tháng" },
]);
const typestatus = ref([
  { value: 0, title: "Dự thảo", bg_color: "#bbbbbb", text_color: "#fff" },
  { value: 1, title: "Chờ duyệt", bg_color: "#2196f3", text_color: "#fff" },
  { value: 2, title: "Ban hành", bg_color: "#04D215", text_color: "#fff" },
  { value: 3, title: "Trả lại", bg_color: "#ff8b4e", text_color: "#fff" },
  { value: 4, title: "Hủy", bg_color: "red", text_color: "#fff" },
]);
const typeprocedures = ref([
  { value: 0, title: "Duyệt tuần tự" },
  { value: 1, title: "Duyệt một trong nhiều" },
  //{ value: 2, title: "Duyệt ngẫu nhiên" },
]);
const locs = ref([
  { value: 0, title: "Tất cả các lịch" },
  { value: 1, title: "Lịch có sử dụng xe" },
  { value: 2, title: "Lịch không sử dụng xe" },
]);

const componentKey = ref({});
const forceRerender = (type) => {
  if (!componentKey.value) {
    componentKey.value = { type: 0 };
  }
  if (!componentKey.value[type]) {
    componentKey.value[type] = 0;
  }
  componentKey.value[type] += 1;
};

//
const opConfig = ref();
const itemConfigs = ref([
  {
    label: "Trực chủ nhật",
    icon: "pi pi-cog",
    command: (event) => {
      configDutySunday();
    },
  },
  {
    label: "Lãnh đạo",
    icon: "pi pi-cog",
    command: (event) => {
      configLeader();
    },
  },
]);
const toggleConfig = (event) => {
  opConfig.value.toggle(event);
};

//Xuất excel
const menuButs = ref();
const itemButs = ref([
  {
    label: "Xuất Excel",
    icon: "pi pi-file-excel",
    command: (event) => {
      exportData("ExportExcel");
    },
  },
  {
    label: "Xuất word",
    icon: "pi pi-file",
    command: (event) => {
      exportWord(event);
    },
  },
  {
    label: "In",
    icon: "pi pi-file",
    command: (event) => {
      print(event);
    },
  },
]);
const toggleExport = (event) => {
  menuButs.value.toggle(event);
};
const exportData = (method) => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios
    .post(
      baseURL + "/api/Excel/ExportExcel",
      {
        excelname: options.value.title.toUpperCase(),
        proc: "calendar_week_listexport",
        par: [
          { par: "user_id", va: store.getters.user.user_id },
          { par: "search", va: options.value.search },
          { par: "week_start_date", va: options.value["week_start_date"] },
          { par: "week_end_date", va: options.value["week_end_date"] },
          { par: "is_type", va: options.value.is_type },
          { par: "filterDate", va: true },
          { par: "is_group", va: options.value.is_group },
        ],
      },
      config
    )
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        swal.close();

        toast.success("Kết xuất Data thành công!");
        if (response.data["path"] != null) {
          const path = baseURL + response.data["path"];
          window.open(path);
        }
      } else {
        swal.fire({
          title: "Thông báo!",
          text: response.data.ms,
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    })
    .catch((error) => {
      if (error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
//Print
const print = () => {
  forceRerender(9);
  var htmltable = "";
  if (props.group === 0) {
    htmltable = renderhtml("formprint", htmltable);
  } else if (props.group === 1) {
    htmltable = renderhtml("formprint_2", htmltable);
  }
  var printframe = window.frames["printframe"];
  printframe.document.write(htmltable);
  setTimeout(function () {
    printframe.print();
    printframe.document.close();
  }, 0);
};
function renderhtml(id, htmltable) {
  htmltable = "";
  //Style
  htmltable += `<style>
    @page Section1 {
      size: 595.45pt 841.7pt;
      margin: 1in 1.25in 1in 1.25in;
      mso-header-margin: 0.5in;
      mso-footer-margin: 0.5in;
      mso-paper-source: 0;
    }
    div.Section1 {
      page: Section1;
    }
    @page Section2 {
      size: 841.7pt 595.45pt;
      mso-page-orientation: landscape;
      margin: 1.25in 1in 1.25in 1in;
      mso-header-margin: 0.5in;
      mso-footer-margin: 0.5in;
      mso-paper-source: 0;
    }
    div.Section2 {
      page: Section2;
    }
    #formprint, #formword  {
      background: #fff !important;
    }
    #formprint *, #formword * {
      font-family: "Times New Roman", Times, serif !important;
      font-size: 13pt;
    }
    .title1,
    .title1 * {
      font-size: 17pt !important;
    }
    .title2,
    .title2 * {
      font-size: 16pt !important;
    }
    .title3,
    .title3 * {
      font-size: 15pt !important;
    }
    .boder tr th,
    .boder tr td {
      border: 1px solid #999999 !important;
      padding: 0.5rem;
    }
    table {
      min-width: 100% !important;
      page-break-inside: auto !important;
      border-collapse: collapse !important;
      table-layout: fixed !important;
    }
    thead {
      display: table-header-group !important;
    }
    tbody {
      display: table-header-group !important;
    }
    tr {
      -webkit-column-break-inside: avoid !important;
      page-break-inside: avoid !important;
    }
    tfoot {
      display: table-footer-group !important;
    }
    .uppercase,
    .uppercase * {
      text-transform: uppercase !important;
    }
    .text-center {
      text-align: center !important;
    }
    .text-left {
      text-align: left !important;
    }
    .text-right {
      text-align: right !important;
    }
    .html p{
      margin: 0 !important;
      padding: 0 !important;
    }
  </style>`;
  var html = document.getElementById(id);
  if (html) {
    htmltable += html.innerHTML;
  }
  return htmltable;
}
//export word
const exportWord = (method) => {
  forceRerender(10);
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  var htmltable = "";
  var orientation = "";
  if (props.group === 0) {
    htmltable = renderhtml("formword", htmltable);
    orientation = "Portrait";
  } else if (props.group === 1) {
    htmltable = renderhtml("formword_2", htmltable);
    orientation = "Landscape";
  }
  axios
    .post(
      baseURL + "/api/calendar_duty/ExportDoc",
      {
        lib: "word",
        name:
          (options.value.is_group === 0 ? "lichhoptuan_" : "lichcongtac_") +
          moment(options.week_start_date).format("DDMMYYYY") +
          "_" +
          moment(options.week_end_date).format("DDMMYYYY") +
          ".doc",
        html: htmltable,
        opition: {
          orientation: orientation,
          pageSize: "A4",
          left: 37.79,
          top: 68.03,
          right: 37.79,
          bottom: 68.03,
        },
      },
      config
    )
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        swal.close();

        toast.success("Kết xuất dữ liệu thành công!");
        if (response.data.path != null) {
          window.open(baseURL + response.data.path);
        }
      } else {
        swal.fire({
          title: "Thông báo!",
          text: response.data.ms,
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    })
    .catch((error) => {
      if (error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};

//Model
const model = ref({
  is_group: options.value.is_group,
  status: 0,
  is_type: 0,
  is_iterations: 0,
  distance_iterations: 0,
  numeric_iterations: 0,
  numeric_attendees: 0,
  chutris: [],
  thamgias: [],
  departments: [],
  files: [],
});
const v$ = useVuelidate(rules, model);

//Function declare
const is_check_all = ref(false);
const currentweek = ref({});
const weeks = ref([]);
const months = ref([
  { month: 1 },
  { month: 2 },
  { month: 3 },
  { month: 4 },
  { month: 5 },
  { month: 6 },
  { month: 7 },
  { month: 8 },
  { month: 9 },
  { month: 10 },
  { month: 11 },
  { month: 12 },
]);
const years = ref([]);
const procedureforms = ref([]);
const signforms = ref([]);
const users = ref([]);
const selectedNodes = ref([]);

Date.prototype.addDays = function (days) {
  var date = new Date(this.valueOf());
  date.setDate(date.getDate() + days);
  return date;
};
const getDayDate = (d) => {
  var date = new Date(d);
  var current_day = date.getDay();
  var day_name = "";
  if (current_day != null) {
    switch (current_day) {
      case 0:
        day_name = "Chủ Nhật";
        break;
      case 1:
        day_name = "Thứ Hai";
        break;
      case 2:
        day_name = "Thứ Ba";
        break;
      case 3:
        day_name = "Thứ Tư";
        break;
      case 4:
        day_name = "Thứ Năm";
        break;
      case 5:
        day_name = "Thứ Sáu";
        break;
      case 6:
        day_name = "Thứ Bảy";
        break;
      default:
        break;
    }
  }
  return day_name;
};
function isValidDate(d) {
  return d instanceof Date && !isNaN(d);
}
const bindDateBetweenFirstAndLast = (
  start_date,
  end_date,
  add_fn,
  interval
) => {
  var retVal = [];
  if (isValidDate(start_date) && isValidDate(end_date)) {
    add_fn = add_fn || Date.prototype.addDays;
    interval = interval || 1;

    var current = new Date(start_date);

    var checkVR = true;
    if (current >= end_date) {
      checkVR = false;
    }
    while (checkVR) {
      if (current >= end_date) {
        checkVR = false;
      }
      retVal.push(new Date(current));
      current = add_fn.call(current, interval);
    }
  }
  return retVal;
};
const addLog = (log) => {
  axios.post(baseURL + "/api/calendar/add_log", log, config);
};
const groupBy = (list, props) => {
  return list.reduce((a, b) => {
    (a[b[props]] = a[b[props]] || []).push(b);
    return a;
  }, {});
};

//Function filter
const searchData = () => {
  isDynamicSQL.value = false;
  initData(true);
};
const filterDate = ref(true);
const changeView = (view) => {
  options.value.view = view;
  if (options.value.view != null) {
    switch (options.value.view) {
      case 0:
        filterDate.value = false;
        initData(true);
        break;
      case 1:
        filterDate.value = true;
        if (weeks.value != null && weeks.value.length > 0) {
          currentweek.value = weeks.value.find(
            (x) => x["week_no"] === options.value["week"]
          );
          options.value["week_start_date"] = new Date(
            currentweek.value["week_start_date"]
          );
          options.value["week_end_date"] = new Date(
            currentweek.value["week_end_date"]
          );
        }
        initData(true);
        break;
      case 2:
        filterDate.value = true;
        options.value["week_start_date"] = new Date(
          options.value.year,
          options.value.month - 1,
          1
        );
        options.value["week_end_date"] = new Date(
          options.value.year,
          options.value.month,
          0
        );
        initData(true);
        break;
      default:
        break;
    }
  }
  initDutySunday();
};
const goYear = (year) => {
  options.value.year = year;
  initDictionary();
};
const goMonth = (month) => {
  options.value.month = month;
  changeView(options.value.view);
};
const goWeek = (week) => {
  options.value.week = week;
  changeView(options.value.view);
};

//Function add
const isAdd = ref(false);
const submitted = ref(false);
const headerDialog = ref();
const displayDialog = ref(false);
const boardrooms = ref([]);
const departments = ref([]);
const cars = ref([]);
const roleFunctions = ref({});
const files = ref([]);
const types = ref([
  { is_type: 0, name: "Họp bình thường" },
  { is_type: 1, name: "Họp trực tuyến" },
]);
const iterations = ref([
  { is_iterations: 0, name: "Không lặp", short: "ngày" },
  { is_iterations: 1, name: "Lặp theo ngày", short: "ngày" },
  { is_iterations: 2, name: "Lặp theo tuần", short: "tuần" },
  { is_iterations: 3, name: "Lặp theo tháng", short: "tháng" },
  { is_iterations: 4, name: "Lặp theo năm", short: "năm" },
]);
const openAddDialog = (str) => {
  forceRerender(1);
  files.value = [];
  isAdd.value = true;
  submitted.value = false;
  var nextweek =
    weeks.value[weeks.value.findIndex((x) => x["is_current_week"] === true)];
  var exists =
    weeks.value[
      weeks.value.findIndex((x) => x["is_current_week"] === true) + 1
    ];
  if (exists != null) {
    nextweek = exists;
  }
  var start_date = new Date(nextweek["week_start_date"]);
  var end_date = new Date(nextweek["week_end_date"]);
  model.value = {
    start_date: new Date(
      start_date.getFullYear(),
      start_date.getMonth(),
      start_date.getDate(),
      7,
      0,
      0
    ),
    start_date_time: new Date(
      start_date.getFullYear(),
      start_date.getMonth(),
      start_date.getDate(),
      7,
      0,
      0
    ),
    end_date: new Date(
      start_date.getFullYear(),
      start_date.getMonth(),
      start_date.getDate(),
      7,
      0,
      0
    ),
    end_date_time: new Date(
      start_date.getFullYear(),
      start_date.getMonth(),
      start_date.getDate(),
      7,
      0,
      0
    ),
    is_group: options.value.is_group,
    status: 0,
    is_type: 0,
    is_iterations: 0,
    distance_iterations: 0,
    numeric_iterations: 0,
    numeric_attendees: 0,
    chutris: [],
    thamgias: [],
    departments: [],
    files: [],
  };
  headerDialog.value = str;
  displayDialog.value = true;
};
const closeDialog = () => {
  model.value = {
    status: 0,
    is_type: 0,
    is_iterations: 0,
    distance_iterations: 0,
    numeric_iterations: 0,
    numeric_attendees: 0,
    chutris: [],
    thamgias: [],
    departments: [],
    files: [],
  };
  displayDialog.value = false;
};
const removeMember = (user, arr) => {
  if (user["member_id"] != null) {
    swal
      .fire({
        title: "Thông báo",
        text: "Bạn có muốn xoá thành viên này không!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Có",
        cancelButtonText: "Không",
      })
      .then((result) => {
        if (result.isConfirmed) {
          swal.fire({
            width: 110,
            didOpen: () => {
              swal.showLoading();
            },
          });
          var ids = [user["member_id"]];
          axios
            .delete(baseURL + "/api/calendar_week/delete_member", {
              headers: { Authorization: `Bearer ${store.getters.token}` },
              data: ids,
            })
            .then((response) => {
              if (response.data.err === "1") {
                swal.fire({
                  title: "Thông báo!",
                  text: response.data.ms,
                  icon: "error",
                  confirmButtonText: "OK",
                });
                return;
              }
              ids.forEach((value, i) => {
                var idx = arr.findIndex((x) => x["member_id"] === value);
                if (idx != -1) {
                  arr.splice(idx, 1);
                  model.value.numeric_attendees -= 1;
                }
              });

              swal.close();
              toast.success("Xoá thành viên thành công!");
            })
            .catch((error) => {
              swal.close();
              if (options.value.loading) options.value.loading = false;
              addLog({
                title: "Lỗi Console delItem",
                controller: "calendarweek.vue",
                logcontent: error.message,
                loai: 2,
              });
              if (error.status === 401) {
                swal.fire({
                  title: "Thông báo!",
                  text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                  icon: "error",
                  confirmButtonText: "OK",
                });
                return;
              }
            });
        }
      });
  } else {
    var idx = arr.findIndex((x) => x["user_id"] === user["user_id"]);
    if (idx != -1) {
      arr.splice(idx, 1);
      model.value.numeric_attendees -= 1;
    }
  }
};
const onUpload = () => {};
const removeFile = (event) => {
  files.value = [];
  event.files.forEach((element) => {
    files.value.push(element);
  });
};
const selectFile = (event) => {
  files.value = [];
  event.files.forEach((element) => {
    files.value.push(element);
  });
};
const editItem = (item) => {
  files.value = [];
  submitted.value = false;
  options.value.loading = true;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  isAdd.value = false;
  axios
    .post(
      baseURL + "/api/calendar/get_datas",
      {
        str: encr(
          JSON.stringify({
            proc: "calendar_week_get",
            par: [
              { par: "user_id", va: store.state.user.user_id },
              { par: "calendar_id", va: item.calendar_id },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      if (response != null && response.data != null) {
        var data = response.data.data;
        if (data != null) {
          var tbs = JSON.parse(data);
          model.value = tbs[0][0];
          if (
            model.value["boardroom_id"] == null &&
            model.value["place_name"] != null
          ) {
            model.value["boardroom_id"] = model.value["place_name"];
          }
          if (model.value.start_date != null && model.value.start_date != "") {
            model.value.start_date = new Date(model.value.start_date);
            model.value.start_date_time = model.value.start_date;
          }
          if (model.value.end_date != null && model.value.end_date != "") {
            model.value.end_date = new Date(model.value.end_date);
            model.value.end_date_time = model.value.end_date;
          }
          var members = tbs[1];
          model.value.chutris = members.filter((x) => x["is_type"] === 0);
          model.value.thamgias = members.filter((x) => x["is_type"] === 1);
          model.value.departments = tbs[2].map((x) => x["department_id"]);
          model.value.files = tbs[3];
        }
      }

      swal.close();
      if (options.value.loading) options.value.loading = false;

      headerDialog.value = "Cập nhật " + options.value.title;
      displayDialog.value = true;
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      if (options.value.loading) options.value.loading = false;
      addLog({
        title: "Lỗi Console editItem",
        controller: "calendarweek.vue",
        logcontent: error.message,
        loai: 2,
      });
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
        return;
      }
    });
};
const changeCheckedAll = (checkedAll) => {
  if (datas.value != null && datas.value.length > 0) {
    datas.value
      .filter((x) => x["calendar_id"] != null && x["is_private"] !== true)
      .forEach((item, i) => {
        item["is_check"] = checkedAll;
      });
    selectedNodes.value = datas.value.filter((x) => x["is_check"]);
  }
};
const changeChecked = (item) => {
  if (datas.value != null && datas.value.length > 0) {
    selectedNodes.value = datas.value.filter((x) => x["is_check"]);
  }
};
const deleteItem = (item) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá " + options.value.title + " này không!",
      icon: "warning",
      showCancelButton: true,
      confirmButtonColor: "#3085d6",
      cancelButtonColor: "#d33",
      confirmButtonText: "Có",
      cancelButtonText: "Không",
    })
    .then((result) => {
      if (result.isConfirmed) {
        options.value.loading = true;
        swal.fire({
          width: 110,
          didOpen: () => {
            swal.showLoading();
          },
        });
        var ids = [];
        if (item != null) {
          ids = [item["calendar_id"]];
        } else {
          if (selectedNodes.value.length > 0) {
            selectedNodes.value.forEach((row, i) => {
              ids.push(row["calendar_id"]);
            });
          }
        }
        axios
          .delete(baseURL + "/api/calendar_week/delete_calendar_week", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: ids,
          })
          .then((response) => {
            if (response.data.err === "1") {
              swal.fire({
                title: "Thông báo!",
                text: response.data.ms,
                icon: "error",
                confirmButtonText: "OK",
              });
              return;
            }
            initData(true);
            // if (ids.length > 0) {
            //   ids.forEach((element, i) => {
            //     var idx = datas.value.findIndex(
            //       (x) => x.user_id == element.calendar_id
            //     );
            //     if (idx != -1) {
            //       datas.value.splice(idx, 1);
            //     }
            //   });
            // }
            swal.close();
            toast.success("Xoá " + options.value.title + " thành công!");
            if (options.value.loading) options.value.loading = false;
          })
          .catch((error) => {
            swal.close();
            if (options.value.loading) options.value.loading = false;
            addLog({
              title: "Lỗi Console delItem",
              controller: "calendarweek.vue",
              logcontent: error.message,
              loai: 2,
            });
            if (error.status === 401) {
              swal.fire({
                title: "Thông báo!",
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                icon: "error",
                confirmButtonText: "OK",
              });
              return;
            }
          });
      }
    });
};
const cancelItem = (item) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn hủy " + options.value.title + " này không!",
      icon: "warning",
      showCancelButton: true,
      confirmButtonColor: "#3085d6",
      cancelButtonColor: "#d33",
      confirmButtonText: "Có",
      cancelButtonText: "Không",
    })
    .then((result) => {
      if (result.isConfirmed) {
        options.value.loading = true;
        swal.fire({
          width: 110,
          didOpen: () => {
            swal.showLoading();
          },
        });
        var ids = [];
        if (item != null) {
          ids = [item["calendar_id"]];
        } else {
          if (selectedNodes.value.length > 0) {
            selectedNodes.value.forEach((row, i) => {
              ids.push(row["calendar_id"]);
            });
          }
        }
        axios
          .delete(baseURL + "/api/calendar_week/cancel_calendar", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: ids,
          })
          .then((response) => {
            if (response.data.err === "1") {
              swal.fire({
                title: "Thông báo!",
                text: response.data.ms,
                icon: "error",
                confirmButtonText: "OK",
              });
              return;
            }
            initData(true);
            swal.close();
            toast.success("Hủy " + options.value.title + " thành công!");
            if (options.value.loading) options.value.loading = false;
            if (response.data.data != null) {
              var datas = JSON.parse(response.data.data);
              if (datas != null && datas.length > 0) {
                datas.forEach((item) => {
                  socketMethod
                    .post("sendnotification", {
                      uids: item["uids"],
                      options: {
                        title: item["title"],
                        text: item["text"],
                        image:
                          baseURL +
                          (store.getters.user.background_image ||
                            "../assets/background/bg.png"),
                        tag: "project.soe.vn",
                        url: "/calendar/detail/".concat(item["calendar_id"]),
                      },
                    })
                    .then((res) => {});
                });
              }
            }
          })
          .catch((error) => {
            swal.close();
            if (options.value.loading) options.value.loading = false;
            addLog({
              title: "Lỗi Console cancelItem",
              controller: "calendarweek.vue",
              logcontent: error.message,
              loai: 2,
            });
            if (error.status === 401) {
              swal.fire({
                title: "Thông báo!",
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                icon: "error",
                confirmButtonText: "OK",
              });
              return;
            }
          });
      }
    });
};
const copyItem = (item) => {
  files.value = [];
  submitted.value = false;
  options.value.loading = true;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  isAdd.value = true;
  axios
    .post(
      baseURL + "/api/calendar/get_datas",
      {
        str: encr(
          JSON.stringify({
            proc: "calendar_week_get",
            par: [
              { par: "user_id", va: store.state.user.user_id },
              { par: "calendar_id", va: item.calendar_id },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      if (response != null && response.data != null) {
        var data = response.data.data;
        if (data != null) {
          var tbs = JSON.parse(data);
          if (tbs[0] != null && tbs[0].length > 0) {
            model.value = tbs[0][0];
          } else {
            model.value = {};
          }
          model.value.status = 0;
          if (
            model.value["boardroom_id"] == null &&
            model.value["place_name"] != null
          ) {
            model.value["boardroom_id"] = model.value["place_name"];
          }
          if (model.value.start_date != null && model.value.start_date != "") {
            model.value.start_date = new Date(model.value.start_date);
            model.value.start_date_time = model.value.start_date;
          }
          if (model.value.end_date != null && model.value.end_date != "") {
            model.value.end_date = new Date(model.value.end_date);
            model.value.end_date_time = model.value.end_date;
          }
          if (tbs[1] != null && tbs[1].length > 0) {
            var members = tbs[1];
            members.forEach((x, i) => {
              if (x["member_id"] != null) {
                delete x["member_id"];
              }
            });
            model.value.chutris = members.filter((x) => x["is_type"] === 0);
            model.value.thamgias = members.filter((x) => x["is_type"] === 1);
          } else {
            model.value.chutris = [];
            model.value.thamgias = [];
          }
          if (tbs[2] != null && tbs[2].length > 0) {
            model.value.departments = tbs[2].map((x) => x["department_id"]);
          } else {
            model.value.departments = [];
          }
          if (tbs[3] != null && tbs[3].length > 0) {
            model.value.files = tbs[3];
          }
        }
      }
      swal.close();
      if (options.value.loading) options.value.loading = false;

      headerDialog.value = "Cập nhật " + options.value.title;
      displayDialog.value = true;
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      if (options.value.loading) options.value.loading = false;
      addLog({
        title: "Lỗi Console editItem",
        controller: "calendarweek.vue",
        logcontent: error.message,
        loai: 2,
      });
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
        return;
      }
    });
};
const goFile = (file) => {
  window.open(basedomainURL + file.file_path, "_blank");
};
const goMeeting = () => {
  swal.fire({
    type: "error",
    icon: "error",
    title: "",
    text: "Tính năng sẽ có ở phiên bản sắp tới!",
  });
  return;
};

//Function add week
const headerDialogMultiple = ref();
const displayDialogMultiple = ref(false);
const openAddDialogMultiple = (str) => {
  forceRerender(2);
  files.value = [];
  isAdd.value = true;
  submitted.value = false;

  headerDialogMultiple.value = str;
  displayDialogMultiple.value = true;
};
const closeDialogMultiple = () => {
  displayDialogMultiple.value = false;
};

//Funtion send
const modelsend = ref({});
const headerDialogSend = ref();
const displayDialogSend = ref(false);
const menuSends = ref();
const itemSends = ref([
  {
    id: 0,
    label: "Chuyển đến quy trình",
    icon: "pi pi-send",
  },
  {
    id: 1,
    label: "Chuyển đến nhóm",
    icon: "pi pi-send",
  },
  {
    id: 2,
    label: "Chuyển đích danh",
    icon: "pi pi-send",
  },
  {
    id: 3,
    label: "Ban hành",
    icon: "pi pi-check-circle",
  },
]);
const toggleSend = (event) => {
  menuSends.value.toggle(event);
};
const openAddDialogSend = (str, type) => {
  forceRerender(3);
  files.value = [];
  submitted.value = false;
  modelsend.value = {
    is_type_send: type,
    content: "",
    read_date: moment(new Date()).format("YYYY-MM-DDTHH:mm:ssZZ"),
  };
  if (modelsend.value.is_type_send != null) {
    switch (modelsend.value.is_type_send) {
      case 0:
        modelsend.value.procedureforms = [...procedureforms.value];
        break;
      case 1:
        modelsend.value.signforms = [...signforms.value];
        break;
      case 2:
        modelsend.value.users = [...users.value];
        break;
      default:
        break;
    }
  }
  headerDialogSend.value = str;
  displayDialogSend.value = true;
};
const closeDialogSend = () => {
  modelsend.value = {};
  displayDialogSend.value = false;
};

//Function approve
const ruleapproves = {};
const modelapprove = ref({});
const va$ = useVuelidate(ruleapproves, modelapprove);
const headerDialogApprove = ref();
const displayDialogApprove = ref(false);
const menuApproves = ref();
const itemApproves = ref([
  {
    id: 0,
    label: "Phê duyệt",
    icon: "pi pi-send",
  },
  {
    id: 1,
    label: "Trả lại",
    icon: "pi pi-undo",
  },
  {
    id: 2,
    label: "Ban hành",
    icon: "pi pi-check-circle",
  },
]);
const toggleApproves = (event) => {
  menuApproves.value.toggle(event);
};
const openAddDialogApprove = (str, type) => {
  forceRerender(4);
  files.value = [];
  submitted.value = false;
  modelapprove.value = {
    is_type_approve: type,
    content: "",
    read_date: moment(new Date()).format("YYYY-MM-DDTHH:mm:ssZZ"),
  };
  headerDialogApprove.value = str;
  displayDialogApprove.value = true;
};
const closeDialogApprove = () => {
  modelapprove.value = {};
  displayDialogApprove.value = false;
};

//Function enact
const modelenact = ref({});
const headerDialogEnact = ref(false);
const displayDialogEnact = ref(false);
const openAddDialogEnact = (str, type) => {
  forceRerender(5);
  files.value = [];
  submitted.value = false;
  modelenact.value = {
    content: "",
    read_date: moment(new Date()).format("YYYY-MM-DDTHH:mm:ssZZ"),
  };
  headerDialogEnact.value = str;
  displayDialogEnact.value = true;
};
const closeDialogEnact = () => {
  modelenact.value = {};
  displayDialogEnact.value = false;
};

//Logs
const is_type_calendar = ref();
const chartprocedures = ref([]);
const chartsigns = ref([]);
const chartsignusers = ref([]);
const chartlogs = ref([]);
const headerDialogLog = ref();
const displayDialogLog = ref(false);
const logItem = (item) => {
  forceRerender(7);
  is_type_calendar.value = item.is_type_send;
  options.value.loading = true;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  isAdd.value = false;
  axios
    .post(
      baseURL + "/api/calendar/get_datas",
      {
        str: encr(
          JSON.stringify({
            proc: "calendar_log_get",
            par: [
              { par: "user_id", va: store.state.user.user_id },
              { par: "calendar_id", va: item.calendar_id },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      if (response != null && response.data != null) {
        var data = response.data.data;
        if (data != null) {
          var tbs = JSON.parse(data);
          chartprocedures.value = tbs[0];
          chartsigns.value = tbs[1];
          chartsignusers.value = tbs[2];
          chartlogs.value = tbs[3];

          if (chartsignusers.value.length > 0) {
            chartsignusers.value.forEach((u) => {
              if (u["sign_date"] != null) {
                u["sign_date"] = moment(new Date(u["sign_date"])).format(
                  "HH:mm DD/MM/YYYY"
                );
              }
              if (u["files"] != null) {
                u["files"] = JSON.parse(u["files"]);
              }
            });
          }
          if (chartsigns.value.length > 0) {
            chartsigns.value.forEach((s) => {
              var idx = typeprocedures.value.findIndex(
                (x) => x["value"] === s["is_type"]
              );
              if (idx != -1) {
                s["type_name"] = typeprocedures.value[idx].title;
              } else {
                s["type_name"] = "Chưa xác định";
              }
              s["chartsignusers"] = chartsignusers.value.filter(
                (su) => su["sign_id"] === s["sign_id"]
              );
            });
          }
          if (chartprocedures.value.length > 0) {
            chartprocedures.value.forEach((p, i) => {
              var idx = typeprocedures.value.findIndex(
                (x) => x["value"] === p["is_type"]
              );
              if (idx != -1) {
                p["type_name"] = typeprocedures.value[idx].title;
              } else {
                p["type_name"] = "Chưa xác định";
              }
              p["chartsigns"] = chartsigns.value.filter(
                (s) => s["procedure_id"] === p["procedure_id"]
              );
            });
          }
          if (chartlogs.value.length > 0) {
            chartlogs.value.forEach((l) => {
              l["created_date"] = moment(new Date(l["created_date"])).format(
                "HH:mm DD/MM/YYYY"
              );
            });
          }
        }
      }
      swal.close();
      if (options.value.loading) options.value.loading = false;

      headerDialogLog.value = "Quy trình xử lý";
      displayDialogLog.value = true;
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      if (options.value.loading) options.value.loading = false;
      addLog({
        title: "Lỗi Console editItem",
        controller: "calendarweek.vue",
        logcontent: error.message,
        loai: 2,
      });
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
        return;
      }
    });
};
const closeDialogLog = () => {
  headerDialogLog.value = false;
  displayDialogLog.value = false;
};

//
const goCalendar = (calendar) => {
  if (calendar.is_group != null) {
    switch (calendar.is_group) {
      case 0:
        router.push({
          name: "calendardetail",
          params: { id: calendar.calendar_id },
        });
        break;
      case 1:
        router.push({
          name: "calendarplantripdetail",
          params: { id: calendar.calendar_id },
        });
        break;
      default:
        router.push({
          name: "calendardetail",
          params: { id: calendar.calendar_id },
        });
        break;
    }
  }
};
const coincide = ref([]);
const headerDialogLogCoincide = ref();
const displayDialogLogCoincide = ref(false);
const openDialogCoincide = (str, id) => {
  forceRerender(8);
  options.value.loading = true;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios
    .post(
      baseURL + "/api/calendar/get_datas",
      {
        str: encr(
          JSON.stringify({
            proc: "calendar_coincide_get",
            par: [
              { par: "user_id", va: store.state.user.user_id },
              { par: "calendar_id", va: id },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      var data = response.data.data;
      if (data != null) {
        var tbs = JSON.parse(data);
        coincide.value = tbs[0];
      }
      swal.close();
      if (options.value.loading) options.value.loading = false;

      headerDialogLogCoincide.value = str;
      displayDialogLogCoincide.value = true;
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      if (options.value.loading) options.value.loading = false;
      addLog({
        title: "Lỗi Console editItem",
        controller: "calendarweek.vue",
        logcontent: error.message,
        loai: 2,
      });
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
        return;
      }
    });
};
const closeDialogCoincide = () => {
  displayDialogLogCoincide.value = false;
};

//Duty Sunday
const duty_sunday = ref({});
const headerDialogDutySunday = ref();
const displayDialogDutySunday = ref(false);
const configDutySunday = () => {
  forceRerender(6);
  headerDialogDutySunday.value = "Thiết lập lịch trực chủ nhật";
  displayDialogDutySunday.value = true;
};
const closeDialogDutySunday = () => {
  displayDialogDutySunday.value = false;
};

//Config leader
const headerDialogLeader = ref();
const displayDialogLeader = ref(false);
const configLeader = () => {
  forceRerender(11);
  headerDialogLeader.value = "Thiết lập lãnh đạo";
  displayDialogLeader.value = true;
};
const closeDialogLeader = () => {
  displayDialogLeader.value = false;
};

//Init
const tivi = (item) => {
  axios
    .post(
      baseURL + "/api/calendar/Tivi_GetCalendar",
      {
        str: encr(
          JSON.stringify({
            tungay: currentweek.value.week_start_date,
            denngay: currentweek.value.week_end_date,
            token:
              "7WVX5kFb4xTtETG/aAAzETjatmn5lOVPJCI8ew+mwE7GcHpmJhQuvdaI+ggjt4RVV064Q5xNo9nyap01wsHDjz9lLgvBcNSryMteRJImNQc=",
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      if (response != null && response.data != null) {
        var data = response.data.data;
        if (data != null) {
          var tbs = JSON.parse(data);
        }
      }
    })
    .catch((error) => {});
};
const onRefresh = () => {
  is_check_all.value = false;
  selectedNodes.value = [];
  initData(true);
};
const initDictionary = () => {
  axios
    .post(
      baseURL + "/api/calendar/get_datas",
      {
        str: encr(
          JSON.stringify({
            proc: "calendar_get_dictionary2",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "year", va: options.value.year },
              { par: "is_group", va: options.value.is_group },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      if (response != null && response.data != null) {
        var data = response.data.data;
        if (data != null) {
          let tbs = JSON.parse(data);
          if (tbs[0] != null && tbs[0].length > 0) {
            weeks.value = tbs[0];
          } else {
            weeks.value = [];
          }
          var exist =
            weeks.value.findIndex((x) => x["is_current_week"] === true) != -1;
          if (exist) {
            currentweek.value = weeks.value.find(
              (x) => x["is_current_week"] === true
            );
          } else {
            currentweek.value = weeks.value.find(
              (x) => x["week_no"] === options.value.week || 0
            );
          }
          if (currentweek.value != null) {
            options.value["week"] = currentweek.value["week_no"];
            switch (options.value.view) {
              case 1:
                options.value["week_start_date"] = new Date(
                  currentweek.value["week_start_date"]
                );
                options.value["week_end_date"] = new Date(
                  currentweek.value["week_end_date"]
                );
                break;
              case 2:
                options.value["week_start_date"] = new Date(
                  options.value.year,
                  options.value.month - 1,
                  1
                );
                options.value["week_end_date"] = new Date(
                  options.value.year,
                  options.value.month,
                  0
                );
                break;
              case 3:
                options.value["week_start_date"] = new Date(
                  options.value["year"],
                  0,
                  1
                );
                options.value["week_end_date"] = new Date(
                  options.value["year"],
                  11,
                  31
                );
                break;
              default:
                break;
            }
          }
          months.value.forEach((m, i) => {
            m["is_current_month"] = false;
            if (m["month"] === options.value.month) {
              m["is_current_month"] = true;
            }
          });
          // if (!tbs[1] || tbs[1].length === 0) {
          //   tbs[1] = [
          //     { year: options.value.year, IsCurrentYear: true, IsPass: false },
          //   ];
          // }
          // if (tbs[1] != null && tbs[1].length > 0) {
          //   var min_year = new Date().getFullYear(),
          //     max_year = new Date().getFullYear();
          //   var tmp;
          //   if (tbs[1].length > 300) {
          //     tbs[1] = [
          //       {
          //         year: options.value.year,
          //         IsCurrentYear: true,
          //         IsPass: false,
          //       },
          //     ];
          //   }
          //   for (var i = tbs[1].length - 1; i >= 0; i--) {
          //     tmp = tbs[1][i].year;
          //     if (tmp < min_year) {
          //       min_year = tmp;
          //     }
          //     if (tmp > max_year) {
          //       max_year = tmp;
          //     }
          //     if (max_year == null || max_year == Infinity) {
          //       max_year = tmp;
          //     }
          //   }
          //   if (min_year != null) {
          //     if (min_year > 3000) {
          //       min_year = 3000;
          //     }
          //     for (var i = min_year - 5; i <= min_year; i++) {
          //       years.value.push({
          //         year: i,
          //         IsCurrentYear: i === options.value.today.getFullYear(),
          //         IsPass: i < options.value.today.getFullYear(),
          //       });
          //     }
          //     if (min_year < max_year) {
          //       if (max_year > 3000) {
          //         max_year = 3000;
          //       }
          //       for (var i = min_year + 1; i <= max_year; i++) {
          //         years.value.push({
          //           year: i,
          //           IsCurrentYear: i === options.value.today.getFullYear(),
          //           IsPass: i < options.value.today.getFullYear(),
          //         });
          //       }
          //     }
          //   }
          //   if (max_year != null) {
          //     if (max_year > 3000) {
          //       max_year = 3000;
          //     }
          //     for (var i = max_year + 1; i <= max_year + 5; i++) {
          //       years.value.push({
          //         year: i,
          //         IsCurrentYear: i === options.value.today.getFullYear(),
          //         IsPass: i < options.value.today.getFullYear(),
          //       });
          //     }
          //   }
          // }
          var startYear = 1970;
          const endYear = new Date().getFullYear() + 10;
          years.value = [];
          for (var i = startYear; i <= endYear; i++) {
            years.value.push({
              year: i,
              IsCurrentYear: i === options.value.today.getFullYear(),
              IsPass: i < options.value.today.getFullYear(),
            });
            startYear++;
          }
          if (tbs[2] != null && tbs[2].length > 0) {
            boardrooms.value = tbs[2];
          } else {
            boardrooms.value = [];
          }
          if (tbs[3] != null && tbs[3].length > 0) {
            departments.value = tbs[3];
          } else {
            departments.value = [];
          }
          if (tbs[4] != null && tbs[4].length > 0) {
            procedureforms.value = tbs[4];
          } else {
            procedureforms.value = [];
          }
          if (tbs[5] != null && tbs[5].length > 0) {
            signforms.value = tbs[5];
          } else {
            signforms.value = [];
          }
          if (tbs[6] != null && tbs[6].length > 0) {
            users.value = tbs[6];
          } else {
            users.value = [];
          }
          if (tbs[7] != null && tbs[7].length > 0) {
            cars.value = tbs[7];
          } else {
            cars.value = [];
          }
          if (tbs[8] != null && tbs[8].length > 0) {
            roleFunctions.value = tbs[8][0];
          } else {
            roleFunctions.value = {};
          }
        }
      }
    })
    .then(() => {
      initDutySunday();
      initData(true);
    })
    .catch((error) => {
      swal.fire({
        title: "Thông báo!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
      return;
    });
};
const initDutySunday = () => {
  axios
    .post(
      baseURL + "/api/calendar/get_datas",
      {
        str: encr(
          JSON.stringify({
            proc: "calendar_duty_sunday_getweek",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "year", va: options.value.year },
              { par: "week", va: options.value.week },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      if (response != null && response.data != null) {
        var data = response.data.data;
        if (data != null) {
          var tbs = JSON.parse(data);
          if (tbs[0] != null && tbs[0].length > 0) {
            duty_sunday.value = tbs[0][0];
          } else {
            duty_sunday.value = {};
          }
        }
      }
    });
};
const initData = (rf) => {
  if (rf) {
    options.value.loading = true;
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
  }
  if (isDynamicSQL.value) {
    initDataSQL();
    return;
  }
  datas.value = [];
  datadays.value = [];
  datachutris.value = [];
  axios
    .post(
      baseURL + "/api/calendar/get_datas",
      {
        str: encr(
          JSON.stringify({
            proc: "calendar_week_list4",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "search", va: options.value.search },
              { par: "week_start_date", va: options.value["week_start_date"] },
              { par: "week_end_date", va: options.value["week_end_date"] },
              { par: "is_type", va: options.value.is_type },
              { par: "filterDate", va: filterDate.value },
              { par: "is_group", va: options.value.is_group },
              { par: "is_loc", va: options.value.loc },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      if (response != null && response.data != null) {
        var data = response.data.data;
        if (data != null) {
          let tbs = JSON.parse(data);
          //convert data
          if (tbs[0] != null && tbs[0].length > 0) {
            tbs[0].forEach((item, i) => {
              if (item["contents"] != null) {
                item["contents"] = item["contents"].replaceAll("\n", "<br/>");
              }
              if (item["invitee"] != null) {
                item["invitee"] = item["invitee"].replaceAll("\n", "<br/>");
              }
              if (item["equip"] != null) {
                item["equip"] = item["equip"].replaceAll("\n", "<br/>");
              }
              if (item["note"] != null) {
                item["note"] = item["note"].replaceAll("\n", "<br/>");
              }
              if (item["members"] != null) {
                item["members"] = JSON.parse(item["members"]);

                item["chutris"] = item["members"].filter(
                  (x) => x["is_type"] === "0"
                );
                item["thamgias"] = item["members"].filter(
                  (x) => x["is_type"] === "1"
                );
              }
              if (item["departments"] != null) {
                item["departments"] = JSON.parse(item["departments"]);
              }
              item["is_holiday"] = new Date(item["day"]).getDay() == 0;
              var idx = typestatus.value.findIndex(
                (x) => x["value"] === item["status"]
              );
              if (idx != -1) {
                item["status_name"] = typestatus.value[idx]["title"];
                item["bg_color"] = typestatus.value[idx]["bg_color"];
                item["text_color"] = typestatus.value[idx]["text_color"];
              } else {
                item["status_name"] = "Chưa xác định";
                item["bg_color"] = "#bbbbbb";
                item["text_color"] = "#fff";
              }
            });
          }
          if (tbs[1] != null && tbs[1].length > 0) {
            var data1 = JSON.parse(JSON.stringify(tbs[1]));
            let obj = groupBy(data1, "user_id");
            var result = Object.entries(obj);
            result.forEach((item) => {
              let obj = {
                user_id: item[0],
                full_name: item[1][0].full_name,
                is_order: item[1][0].is_order,
              };
              datachutris.value.push(obj);
            });
            datachutris.value = datachutris.value.sort((a, b) => {
              b.is_order - a.is_order;
            });
            tbs[1].forEach((item, i) => {
              if (item["contents"] != null) {
                item["contents"] = item["contents"].replaceAll("\n", "<br/>");
              }
              item["is_holiday"] = new Date(item["day"]).getDay() == 0;
            });
          }
          if (filterDate.value) {
            let dateinweeks = bindDateBetweenFirstAndLast(
              new Date(options.value["week_start_date"]),
              new Date(options.value["week_end_date"])
            );
            dateinweeks
              .filter(
                (a) =>
                  tbs[0].findIndex(
                    (b) => b["day_string"] === moment(a).format("DD/MM/YYYY")
                  ) === -1
              )
              .forEach((day, i) => {
                tbs[0].push({
                  day: day,
                  day_name: getDayDate(day),
                  day_string: moment(day).format("DD/MM/YYYY"),
                  is_holiday: day.getDay() == 0,
                });
              });
            tbs[0] = tbs[0].sort(function (a, b) {
              return new Date(a["day"]) - new Date(b["day"]);
            });
            dateinweeks
              .filter(
                (a) =>
                  tbs[1].findIndex(
                    (b) => b["day_string"] === moment(a).format("DD/MM/YYYY")
                  ) === -1
              )
              .forEach((day, i) => {
                tbs[1].push({
                  day: day,
                  day_name: getDayDate(day),
                  day_string: moment(day).format("DD/MM/YYYY"),
                  is_holiday: day.getDay() == 0,
                });
              });
            tbs[1] = tbs[1].sort(function (a, b) {
              return new Date(a["day"]) - new Date(b["day"]);
            });
          }
          if (tbs[0] != null && tbs[0].length > 0) {
            datas.value = tbs[0];
          } else {
            datas.value = [];
          }
          if (tbs[1] != null && tbs[1].length > 0) {
            var data1 = JSON.parse(JSON.stringify(tbs[1]));
            let obj = groupBy(data1, "day_string");
            var result = Object.entries(obj);
            result.forEach((item) => {
              let obj = {
                day_string: item[0],
                day: item[1][0].day,
                day_name: item[1][0].day_name,
                is_holiday: item[1][0].is_holiday,
                list_contents: item[1],
              };
              datadays.value.push(obj);
            });
            var idx = datadays.value.findIndex((x) => x.is_holiday);
            if (idx != -1) {
              holiday.value = datadays.value[idx];
            }
          } else {
            datadays.value = [];
          }
          options.value.total = datas.value.length;
          if (is_check_all.value === true) {
            changeCheckedAll(true);
          }
        }
      }
      swal.close();
      if (isFirst.value) isFirst.value = false;
      if (options.value.loading) options.value.loading = false;
    })
    .catch((error) => {
      swal.close();
      if (options.value.loading) options.value.loading = false;
      swal.fire({
        title: "Thông báo!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
      return;
    });
};

onMounted(() => {
  if (props.state != null) {
    switch (props.state) {
      case 1:
        is_check_all.value = true;
        groups.value = [
          { view: 0, icon: "", title: "Tất cả các lịch" },
          { view: 1, icon: "", title: "Xem theo tuần" },
          { view: 2, icon: "", title: "Xem theo tháng" },
        ];
        options.value.view = 0;
        filterDate.value = false;
        break;
      default:
        groups.value = [
          { view: 1, icon: "", title: "Xem theo tuần" },
          { view: 2, icon: "", title: "Xem theo tháng" },
        ];
        options.value.view = 1;
        filterDate.value = true;
        break;
    }
  }
  initDictionary();
});
</script>
<template>
  <div class="surface-100 p-3 calendar">
    <Toolbar class="outline-none surface-0 border-none">
      <template #start>
        <span class="p-input-icon-left">
          <i class="pi pi-search" />
          <InputText
            @keypress.enter="searchData()"
            v-model="options.search"
            type="text"
            spellcheck="false"
            :placeholder="'Tìm kiếm ' + options.title"
          />
        </span>
        <Dropdown
          v-if="props.group === 1"
          :options="locs"
          v-model="options.loc"
          optionLabel="title"
          optionValue="value"
          placeholder="Tất cả các lịch"
          class="w-full ml-2"
          @change="initData(true)"
        >
          <template #option="slotProps">
            <div
              v-if="slotProps.option"
              class="country-item flex align-items-center"
            >
              <div class="pt-1 pl-2">
                <div>
                  <span>{{ slotProps.option.title }}</span>
                </div>
              </div>
            </div>
          </template>
        </Dropdown>
        <!-- <Button
          @click="toggle"
          :style="[styleObj]"
          type="button"
          class="ml-2 p-button-outlined p-button-secondary"
          icon="pi pi-filter"
          aria:haspopup="true"
          aria-controls="overlay_panel"
          v-tooltip.top="'Bộ lọc'"
        />
        <OverlayPanel
          :showCloseIcon="false"
          ref="op"
          appendTo="body"
          class="p-0 m-0"
          id="overlay_panel"
          style="width: 300px"
        >
          <div class="grid formgrid m-0">
            <div class="flex field col-12 p-0">
              <div class="col-4 text-left pt-2 p-0" style="text-align: left">
                Trạng thái
              </div>
              <div class="col-8">
                <Dropdown
                  class="col-12 p-0 m-0"
                  v-model="filterHienthi"
                  :options="trangThai"
                  optionLabel="name"
                  optionValue="code"
                  placeholder="Trạng thái"
                />
              </div>
            </div>
            <div class="flex col-12 p-0">
              <Toolbar class="border-none surface-0 outline-none pb-0 w-full">
                <template #start>
                  <Button
                    @click="reFilter"
                    class="p-button-outlined"
                    label="Xóa"
                  ></Button>
                </template>
                <template #end>
                  <Button @click="filters" label="Lọc"></Button>
                </template>
              </Toolbar>
            </div>
          </div>
        </OverlayPanel> -->
      </template>
      <template #end>
        <!-- <Button
          @click="configDutySunday"
          icon="pi pi pi-cog"
          label="Thiết lập"
          class="mr-2 p-button-outlined p-button-secondary"
          aria:haspopup="true"
          aria-controls="overlay_send"
          v-if="props.group === 1 && (store.getters.user.is_admin || store.getters.user.role_code === 'admin')"
        /> -->
        <Button
          v-if="
            props.group === 1 &&
            (store.getters.user.is_admin ||
              store.getters.user.role_code === 'admin')
          "
          @click="toggleConfig"
          type="button"
          class="mr-2 p-button-outlined p-button-secondary"
          icon="pi pi pi-cog"
          label="Thiết lập"
          aria:haspopup="true"
          aria-controls="overlay_config"
        />
        <Menu
          :model="itemConfigs"
          :popup="true"
          id="overlay_config"
          ref="opConfig"
        />
        <Button
          @click="toggleSend"
          icon="pi pi pi-send"
          label="Chuyển xử lý"
          class="mr-2 p-button-outlined p-button-secondary"
          aria:haspopup="true"
          aria-controls="overlay_send"
          v-if="props.state === 0 && selectedNodes.length > 0"
        />
        <Menu
          :model="itemSends"
          :popup="true"
          id="overlay_send"
          ref="menuSends"
        >
          <template #item="{ item }">
            <a
              v-if="item.id !== 3"
              class="p-menuitem-link"
              role="menuitem"
              tabindex="0"
              @click="openAddDialogSend(item.label, item.id)"
            >
              <span class="p-menuitem-icon" :class="item.icon"></span>
              <span class="p-menuitem-text">{{ item.label }}</span>
            </a>
            <a
              v-if="
                item.id === 3 &&
                (store.state.user.is_admin || roleFunctions.calendar_enact)
              "
              class="p-menuitem-link"
              role="menuitem"
              tabindex="0"
              @click="openAddDialogEnact(item.label, item.id)"
            >
              <span class="p-menuitem-icon" :class="item.icon"></span>
              <span class="p-menuitem-text">{{ item.label }}</span>
            </a>
          </template>
        </Menu>
        <Button
          @click="toggleApproves"
          icon="pi pi-send"
          label="Chuyển xử lý"
          class="mr-2 p-button-outlined p-button-secondary"
          aria:haspopup="true"
          aria-controls="overlay_approves"
          v-if="props.state === 1 && selectedNodes.length > 0"
        />
        <Menu
          :model="itemApproves"
          :popup="true"
          id="overlay_approves"
          ref="menuApproves"
        >
          <template #item="{ item }">
            <a
              v-if="item.id !== 2"
              class="p-menuitem-link"
              role="menuitem"
              tabindex="0"
              @click="openAddDialogApprove(item.label, item.id)"
            >
              <span class="p-menuitem-icon" :class="item.icon"></span>
              <span class="p-menuitem-text">{{ item.label }}</span>
            </a>
            <a
              v-if="
                item.id === 2 &&
                (store.state.user.is_admin || roleFunctions.calendar_enact)
              "
              class="p-menuitem-link"
              role="menuitem"
              tabindex="0"
              @click="openAddDialogEnact(item.label, item.id)"
            >
              <span class="p-menuitem-icon" :class="item.icon"></span>
              <span class="p-menuitem-text">{{ item.label }}</span>
            </a>
          </template>
        </Menu>
        <Button
          icon="pi pi-trash"
          label="Xóa"
          class="mr-2 p-button-danger"
          v-if="props.state === 0 && selectedNodes.length > 0"
          @click="deleteItem()"
        />
        <Button
          @click="openAddDialog('Thêm mới ' + options.title)"
          label="Thêm mới"
          icon="pi pi-plus"
          class="mr-2"
          v-if="props.state === 0"
        />
        <Button
          @click="openAddDialogMultiple('Thêm mới ' + options.title)"
          label="Thêm mới theo tuần"
          icon="pi pi-plus"
          class="mr-2"
          v-if="props.state === 0"
        />
        <Button
          @click="toggleExport"
          label="Tiện ích"
          icon="pi pi-file-excel"
          class="mr-2 p-button-outlined p-button-secondary"
          aria-haspopup="true"
          aria-controls="overlay_Export"
        />
        <Menu
          :model="itemButs"
          :popup="true"
          id="overlay_Export"
          ref="menuButs"
        />
        <Button
          @click="onRefresh()"
          class="p-button-outlined p-button-secondary"
          icon="pi pi-refresh"
          label="Tải lại"
        />
      </template>
    </Toolbar>
    <Toolbar class="outline-none surface-0 border-none pt-0">
      <template #start>
        <div style="height: 36px; display: flex; align-items: center">
          <SelectButton
            v-model="options.view"
            :options="groups"
            @change="changeView(options.view)"
            optionValue="view"
            optionLabel="view"
            dataKey="view"
            aria-labelledby="custom"
          >
            <template #option="slotProps">
              <span>{{ slotProps.option.title }}</span>
            </template>
          </SelectButton>
        </div>
      </template>
      <template #end>
        <div class="form-group m-0 mr-2" v-if="options.view !== 0">
          <Dropdown
            :options="years"
            :filter="true"
            :showClear="false"
            v-model="options.year"
            @change="goYear(options.year)"
            optionLabel="year"
            optionValue="year"
            placeholder="Chọn năm"
            class="ip36"
            style="min-width: 170px"
          >
            <template #value="slotProps">
              <div
                class="country-item country-item-value flex"
                v-if="slotProps.value"
              >
                <i class="pi pi-calendar mr-2 format-flex-center"></i>
                <div>Năm {{ slotProps.value }}</div>
              </div>
              <span v-else>
                {{ slotProps.placeholder }}
              </span>
            </template>
            <template #option="slotProps">
              <div
                class="country-item country-item-value py-2"
                v-if="slotProps.option"
              >
                <div>Năm {{ slotProps.option.year }}</div>
              </div>
              <span v-else> Chưa có dữ liệu năm </span>
            </template>
          </Dropdown>
        </div>
        <div class="form-group m-0" v-if="options.view === 2">
          <Dropdown
            :options="months"
            :filter="true"
            :showClear="false"
            v-model="options.month"
            @change="goMonth(options.month)"
            optionLabel="month"
            optionValue="month"
            placeholder="Chọn tháng"
            class="ip36"
          >
            <template #value="slotProps">
              <div
                class="country-item country-item-value flex"
                v-if="slotProps.value"
              >
                <i class="pi pi-calendar mr-2 format-flex-center"></i>
                <div>
                  Tháng {{ slotProps.value }} ({{
                    moment(new Date(options["week_start_date"])).format("DD/MM")
                  }}
                  -
                  {{
                    moment(new Date(options["week_end_date"])).format("DD/MM")
                  }})
                </div>
              </div>
              <span v-else>
                {{ slotProps.placeholder }}
              </span>
            </template>
            <template #option="slotProps">
              <div
                class="country-item country-item-value py-2"
                v-if="slotProps.option"
              >
                <div>Tháng {{ slotProps.option.month }}</div>
              </div>
              <span v-else> Chưa có dữ liệu tháng </span>
            </template>
          </Dropdown>
        </div>
        <div class="form-group m-0" v-if="options.view === 1">
          <Dropdown
            :options="weeks"
            :filter="true"
            :showClear="false"
            v-model="options.week"
            @change="goWeek(options.week)"
            optionLabel="week_no"
            optionValue="week_no"
            placeholder="Chọn tuần"
            class="ip36"
          >
            <template #value="slotProps">
              <div
                class="country-item country-item-value flex"
                v-if="slotProps.value"
              >
                <i class="pi pi-calendar mr-2 format-flex-center"></i>
                <div>
                  Tuần {{ slotProps.value }} ({{
                    moment(new Date(options["week_start_date"])).format("DD/MM")
                  }}
                  -
                  {{
                    moment(new Date(options["week_end_date"])).format("DD/MM")
                  }})
                </div>
              </div>
              <span v-else>
                {{ slotProps.placeholder }}
              </span>
            </template>
            <template #option="slotProps">
              <div
                class="country-item country-item-value py-2"
                v-if="slotProps.option"
              >
                <div>
                  Tuần {{ slotProps.option.week_no }} ({{
                    moment(new Date(slotProps.option.week_start_date)).format(
                      "DD/MM"
                    )
                  }}
                  -
                  {{
                    moment(new Date(slotProps.option.week_end_date)).format(
                      "DD/MM"
                    )
                  }})
                </div>
              </div>
              <span v-else> Chưa có dữ liệu tuần </span>
            </template>
          </Dropdown>
        </div>
      </template>
    </Toolbar>
    <div class="d-lang-table">
      <DataTable
        @sort="onSort($event)"
        :value="datas"
        :totalRecords="options.total"
        :lazy="true"
        :rowHover="true"
        :showGridlines="true"
        v-model:selection="selectedNodes"
        dataKey="calendar_id"
        scrollHeight="flex"
        filterDisplay="menu"
        filterMode="lenient"
        responsiveLayout="scroll"
        rowGroupMode="rowspan"
        groupRowsBy="day_string"
      >
        <Column
          field="day_string"
          header="Ngày tháng"
          headerStyle="text-align:center;width:150px;height:50px;"
          bodyStyle="text-align:center;width:150px;max-height:60px"
          class="align-items-center justify-content-center text-center"
        >
          <template #body="slotProps">
            <div
              class="format-grid-center style-day"
              :class="{
                true: slotProps.data.is_holiday,
                false: !slotProps.data.is_holiday,
              }"
            >
              <b>{{ slotProps.data.day_name }}</b>
              <span>{{ slotProps.data.day_string }}</span>
            </div>
          </template>
        </Column>
        <Column
          headerStyle="text-align:center;width:50px"
          bodyStyle="text-align:center;width:50px"
          class="align-items-center justify-content-center text-center"
          v-if="props.state === 0 || props.state === 1"
        >
          <template #header>
            <div class="mx-2">
              <Checkbox
                v-model="is_check_all"
                :binary="true"
                @change="changeCheckedAll(is_check_all)"
              />
            </div>
          </template>
          <template #body="slotProps">
            <div v-if="!slotProps.data.is_private" class="mx-2">
              <Checkbox
                v-if="slotProps.data.calendar_id != null"
                v-model="slotProps.data.is_check"
                :binary="true"
                @change="changeChecked(slotProps.data)"
              />
            </div>
          </template>
        </Column>
        <Column
          field="contents"
          header="Nội dung"
          headerStyle="height:50px;max-width:auto;min-width:150px;"
          bodyStyle="max-height:60px;"
        >
          <template #body="slotProps">
            <div class="mx-2 style-day">
              <div>
                <a v-if="slotProps.data.is_important" class="mr-2">
                  <i
                    class="pi pi-star-fill"
                    v-tooltip.top="'Lịch quan trọng'"
                    style="color: #f5b041"
                  ></i>
                </a>
                <a
                  v-if="
                    slotProps.data.is_coincide && slotProps.data.status !== 2
                  "
                  @click="
                    openDialogCoincide('Lịch trùng', slotProps.data.calendar_id)
                  "
                  style="cursor: pointer"
                  class="mr-2"
                >
                  <i
                    class="pi pi-exclamation-triangle"
                    v-tooltip.top="'Trùng lịch'"
                    style="color: red"
                  ></i>
                </a>
                <a class="hover" @click="goCalendar(slotProps.data)">
                  <div v-html="slotProps.data.contents"></div>
                </a>
                <a
                  v-if="slotProps.data.is_type === 1"
                  class="hover mx-2"
                  @click="goMeeting()"
                  style="color: #2196f3 !important"
                  v-tooltip.top="'Meeting'"
                >
                  <i class="pi pi-video"></i>
                </a>
              </div>
              <div v-if="props.group === 0">
                <div v-if="slotProps.data.day_space < 1">
                  (<span>{{
                    moment(slotProps.data.start_date).format("HH:mm")
                  }}</span>
                  <span
                    v-if="
                      slotProps.data.start_date != null &&
                      slotProps.data.end_date != null
                    "
                  >
                    -
                  </span>
                  <span>{{
                    moment(slotProps.data.end_date).format("HH:mm")
                  }}</span
                  >)
                </div>
                <div v-if="slotProps.data.day_space > 0">
                  <span>{{
                    moment(slotProps.data.start_date).format("DD/MM/YYYY")
                  }}</span>
                  <span
                    v-if="
                      slotProps.data.start_date != null &&
                      slotProps.data.end_date != null
                    "
                  >
                    - </span
                  ><span>{{
                    moment(slotProps.data.end_date).format("DD/MM/YYYY")
                  }}</span>
                </div>
              </div>
              <div v-if="slotProps.data.is_private">
                <span>(Cá nhân)</span>
              </div>
            </div>
          </template>
        </Column>
        <Column
          field="chutris"
          header="Chủ trì"
          headerStyle="text-align:center;width:100px;height:50px"
          bodyStyle="text-align:center;width:100px;max-height:60px"
          class="align-items-center justify-content-center text-center"
        >
          <template #body="slotProps">
            <div class="flex justify-content-center">
              <AvatarGroup
                v-if="
                  slotProps.data.chutris && slotProps.data.chutris.length > 0
                "
              >
                <Avatar
                  v-for="(item, index) in slotProps.data.chutris.slice(0, 3)"
                  v-bind:label="
                    item.avatar ? '' : item.last_name.substring(0, 1)
                  "
                  v-bind:image="
                    item.avatar
                      ? basedomainURL + item.avatar
                      : basedomainURL + '/Portals/Image/noimg.jpg'
                  "
                  v-tooltip.top="item.full_name"
                  :key="item.user_id"
                  style="border: 2px solid orange; color: white"
                  @click="onTaskUserFilter(item)"
                  @error="basedomainURL + '/Portals/Image/noimg.jpg'"
                  size="large"
                  shape="circle"
                  class="cursor-pointer"
                  :style="{ backgroundColor: bgColor[index % 7] }"
                />
                <Avatar
                  v-if="
                    slotProps.data.chutris && slotProps.data.chutris.length > 3
                  "
                  v-bind:label="
                    '+' + (slotProps.data.chutris.length - 3).toString()
                  "
                  shape="circle"
                  size="large"
                  style="background-color: #2196f3; color: #ffffff"
                  class="cursor-pointer"
                />
              </AvatarGroup>
            </div>
          </template>
        </Column>
        <Column
          field="thamgias"
          header="Thành viên tham gia"
          headerStyle="text-align:center;width:150px;height:50px"
          bodyStyle="text-align:center;width:150px;max-height:60px"
          class="align-items-center justify-content-center text-center"
        >
          <template #body="slotProps">
            <div class="flex justify-content-center">
              <AvatarGroup
                v-if="
                  slotProps.data.thamgias && slotProps.data.thamgias.length > 0
                "
              >
                <Avatar
                  v-for="(item, index) in slotProps.data.thamgias.slice(0, 3)"
                  v-bind:label="
                    item.avatar ? '' : item.last_name.substring(0, 1)
                  "
                  v-bind:image="
                    item.avatar
                      ? basedomainURL + item.avatar
                      : basedomainURL + '/Portals/Image/noimg.jpg'
                  "
                  v-tooltip.top="item.full_name"
                  :key="item.user_id"
                  style="border: 2px solid white; color: white"
                  @click="onTaskUserFilter(item)"
                  @error="basedomainURL + '/Portals/Image/noimg.jpg'"
                  size="large"
                  shape="circle"
                  class="cursor-pointer"
                  :style="{ backgroundColor: bgColor[index % 7] }"
                />
                <Avatar
                  v-if="
                    slotProps.data.thamgias &&
                    slotProps.data.thamgias.length > 3
                  "
                  v-bind:label="
                    '+' + (slotProps.data.thamgias.length - 3).toString()
                  "
                  shape="circle"
                  size="large"
                  style="background-color: #2196f3; color: #ffffff"
                  class="cursor-pointer"
                />
              </AvatarGroup>
            </div>
            <div class="mt-2" v-if="slotProps.data.invitee">
              Người được mời: <span v-html="slotProps.data.invitee"></span>
            </div>
            <div class="mt-2" v-if="slotProps.data.departments">
              <div>
                Phòng ban được mời:
                <span
                  v-for="(item, index) in slotProps.data.departments"
                  :key="index"
                >
                  <span
                    v-if="
                      index > 0 && index < slotProps.data.departments.length
                    "
                    >,
                  </span>
                  {{ item.department_name }}
                </span>
              </div>
            </div>
          </template>
        </Column>
        <Column
          v-if="props.group === 1"
          field="car_name"
          header="Xe sử dụng"
          headerStyle="text-align:center;width:120px;height:50px"
          bodyStyle="text-align:center;width:120px;max-height:60px"
          class="align-items-center justify-content-center text-center"
        >
        </Column>
        <Column
          field="boardroom_name"
          :header="
            props.group === 0
              ? 'Địa điểm họp'
              : props.group === 1
              ? 'Địa điểm công tác'
              : ''
          "
          headerStyle="text-align:center;width:150px;height:50px"
          bodyStyle="text-align:center;width:150px;;max-height:60px"
          class="align-items-center justify-content-center text-center"
        >
        </Column>
        <Column
          v-if="props.group === 0"
          field="equip"
          header="Chuẩn bị"
          headerStyle="text-align:center;width:150px;height:50px"
          bodyStyle="text-align:center;width:150px;max-height:60px"
          class="align-items-center justify-content-center text-center"
        >
          <template #body="slotProps">
            <div v-html="slotProps.data.equip"></div>
          </template>
        </Column>
        <Column
          field="status_name"
          header="Trạng thái"
          headerStyle="text-align:center;width:120px;height:50px"
          bodyStyle="text-align:center;width:120px;max-height:60px"
          class="align-items-center justify-content-center text-center"
        >
          <template #body="slotProps">
            <div
              class="format-flex-center"
              v-if="slotProps.data.calendar_id != null"
            >
              <Tag
                class="px-3 py-1"
                :value="slotProps.data.status_name"
                :style="{
                  backgroundColor: slotProps.data.bg_color,
                  color: slotProps.data.text_color,
                }"
                style="font-size: 11px"
              ></Tag>
            </div>
          </template>
        </Column>
        <Column
          header="Chức năng"
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;width:150px;height:50px"
          bodyStyle="text-align:center;max-width:150px;max-height:60px"
        >
          <template #body="slotProps">
            <div>
              <Button
                v-if="slotProps.data.is_copy"
                @click="copyItem(slotProps.data)"
                class="p-button-rounded p-button-secondary p-button-outlined mx-1 mb-2"
                type="button"
                icon="pi pi-copy"
                v-tooltip.top="'Nhân bản'"
              ></Button>
              <Button
                v-if="slotProps.data.calendar_id != null"
                @click="logItem(slotProps.data)"
                class="p-button-rounded p-button-secondary p-button-outlined mx-1 mb-2"
                type="button"
                icon="pi pi-chart-bar"
                v-tooltip.top="'Quy trình xử lý'"
              ></Button>
              <Button
                v-if="slotProps.data.is_edit"
                @click="editItem(slotProps.data)"
                class="p-button-rounded p-button-secondary p-button-outlined mx-1 mb-2"
                type="button"
                icon="pi pi-pencil"
                v-tooltip.top="'Sửa'"
              ></Button>
              <Button
                v-if="slotProps.data.is_cancel"
                @click="cancelItem(slotProps.data)"
                class="p-button-rounded p-button-secondary p-button-outlined mx-1 mb-2"
                type="button"
                icon="pi pi-times"
                v-tooltip.top="'Hủy lịch'"
              ></Button>
              <Button
                v-if="slotProps.data.is_delete"
                @click="deleteItem(slotProps.data)"
                class="p-button-rounded p-button-secondary p-button-outlined mx-1 mb-2"
                type="button"
                v-tooltip.top="'Xóa'"
                icon="pi pi-trash"
              ></Button>
            </div>
          </template>
        </Column>
        <template #empty>
          <div
            class="align-items-center justify-content-center p-4 text-center m-auto"
            v-if="!options.loading && (!isFirst || options.total == 0)"
            style="display: flex; height: calc(100vh - 245px)"
          >
            <div>
              <img src="../../../assets/background/nodata.png" height="144" />
              <h3 class="m-1">Không có dữ liệu</h3>
            </div>
          </div>
        </template>
      </DataTable>
    </div>
    <iframe name="printframe" id="printframe" style="display: none"></iframe>
  </div>

  <!--Model-->
  <dialogmodelweek
    :key="componentKey['1']"
    :temp="false"
    :headerDialog="headerDialog"
    :displayDialog="displayDialog"
    :closeDialog="closeDialog"
    :isAdd="isAdd"
    :submitted="submitted"
    :loading="options.loading"
    :group="options.is_group"
    :model="model"
    :files="files"
    :selectFile="selectFile"
    :removeFile="removeFile"
    :boardrooms="boardrooms"
    :departments="departments"
    :users="users"
    :cars="cars"
    :initData="initData"
  />

  <!--Multiple model-->
  <dialogmodelmultipleweek
    :key="componentKey['2']"
    :headerDialog="headerDialogMultiple"
    :displayDialog="displayDialogMultiple"
    :closeDialog="closeDialogMultiple"
    :isAdd="isAdd"
    :submitted="submitted"
    :loading="options.loading"
    :group="options.is_group"
    :currentweek="currentweek"
    :weeks="weeks"
    :boardrooms="boardrooms"
    :departments="departments"
    :users="users"
    :cars="cars"
    :initData="initData"
  />

  <!--Send-->
  <dialogsend
    :key="componentKey['3']"
    :headerDialog="headerDialogSend"
    :displayDialog="displayDialogSend"
    :closeDialog="closeDialogSend"
    :modelsend="modelsend"
    :files="files"
    :selectFile="selectFile"
    :removeFile="removeFile"
    :selectedNodes="selectedNodes"
    :initData="initData"
  />

  <!--Approve-->
  <dialogapprove
    :key="componentKey['4']"
    :headerDialog="headerDialogApprove"
    :displayDialog="displayDialogApprove"
    :closeDialog="closeDialogApprove"
    :modelapprove="modelapprove"
    :files="files"
    :selectFile="selectFile"
    :removeFile="removeFile"
    :selectedNodes="selectedNodes"
    :initData="initData"
  />

  <!--enact-->
  <dialogenact
    :key="componentKey['5']"
    :headerDialog="headerDialogEnact"
    :displayDialog="displayDialogEnact"
    :closeDialog="closeDialogEnact"
    :modelenact="modelenact"
    :files="files"
    :selectFile="selectFile"
    :removeFile="removeFile"
    :selectedNodes="selectedNodes"
    :initData="initData"
  />

  <!--duty sunday-->
  <calendardutysunday
    :key="componentKey['6']"
    :headerDialog="headerDialogDutySunday"
    :displayDialog="displayDialogDutySunday"
    :closeDialog="closeDialogDutySunday"
  />

  <!--logs-->
  <dialogchart
    :key="componentKey['7']"
    :headerDialog="headerDialogLog"
    :displayDialog="displayDialogLog"
    :closeDialog="closeDialogLog"
    :is_type_calendar="is_type_calendar"
    :chartprocedures="chartprocedures"
    :chartsigns="chartsigns"
    :chartsignusers="chartsignusers"
    :chartlogs="chartlogs"
  />

  <!--Coincide-->
  <dialogcoincide
    :key="componentKey['8']"
    :headerDialog="headerDialogLogCoincide"
    :displayDialog="displayDialogLogCoincide"
    :closeDialog="closeDialogCoincide"
    :datas="coincide"
    :goCalendar="goCalendar"
  />

  <!--print-->
  <frameprintweek
    :key="componentKey['9']"
    :datas="datas"
    :group="options.is_group"
    :week_start_date="options.week_start_date"
    :week_end_date="options.week_end_date"
  />

  <frameprintweek2
    :key="componentKey['9']"
    :datadays="datadays"
    :holiday="holiday"
    :datachutris="datachutris"
    :duty_sunday="duty_sunday"
    :group="options.is_group"
    :week_start_date="options.week_start_date"
    :week_end_date="options.week_end_date"
  />

  <!--word-->
  <framewordweek
    :key="componentKey['10']"
    :datas="datas"
    :group="options.is_group"
    :week_start_date="options.week_start_date"
    :week_end_date="options.week_end_date"
  />

  <framewordweek2
    :key="componentKey['10']"
    :datadays="datadays"
    :holiday="holiday"
    :datachutris="datachutris"
    :duty_sunday="duty_sunday"
    :group="options.is_group"
    :week_start_date="options.week_start_date"
    :week_end_date="options.week_end_date"
  />

  <!--duty sunday-->
  <calendarleader
    :key="componentKey['11']"
    :headerDialog="headerDialogLeader"
    :displayDialog="displayDialogLeader"
    :closeDialog="closeDialogLeader"
  />
</template>
<style scoped>
@import url(./stylecalendar.css);
</style>
<style lang="scss" scoped>
::v-deep(.d-lang-table) {
  .p-datatable-thead .justify-content-center .p-column-header-content {
    justify-content: center !important;
  }

  .p-datatable-table {
    position: absolute;
  }

  .p-datatable-thead {
    position: sticky;
    top: 0;
    z-index: 1;
  }
}
::v-deep(.form-group) {
  .p-multiselect .p-multiselect-label,
  .p-dropdown .p-dropdown-label {
    height: 100%;
    display: flex;
    align-items: center;
  }
  .p-chip img {
    margin: 0;
  }
  .p-avatar-text {
    font-size: 1rem;
  }
}
::v-deep(.avatar-item) {
  .p-avatar.p-avatar-lg {
    width: 3rem;
    height: 3rem;
  }
}
::v-deep(.is-close) {
  .p-panel-header {
    color: red;
  }
}
</style>
