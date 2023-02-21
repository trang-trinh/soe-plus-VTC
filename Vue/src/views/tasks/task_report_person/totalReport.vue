<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { encr } from "../../../util/function.js";
import moment from "moment";
import DetailReportVue from "./component/DetailReport.vue";
import Processing from "./component/ProcessingReport.vue";
import framPrint from "./component/framePrint.vue";
const toast = useToast();
const cryoptojs = inject("cryptojs");

const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");

const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const addLog = (log) => {
  axios.post(baseURL + "/api/Proc/AddLog", log, config);
};
const basedomainURL = fileURL;
const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
  "#FF88D3",
]);
const headerDialog = ref();
const DialogVisible = ref(false);
let user = store.state.user;
const personReport = ref({
  group_id: null,
  config_id: null,
  report_name: "",
  list_task_id: [],
  messages: null,
  self_point: null,
  reviewed_point: null,
  is_approving: null,
  status: null,
});
const rules = {
  report_name: { required },
  list_task_id: { required },
  messages: { required },
  self_point: { required },
  reviewed_point: { required },
};
const v$ = useVuelidate(rules, personReport);

const submitted = ref(false);

const today = new Date();
const optionsLinkTask = ref({
  IsNext: true,
  sort: "modified_date",
  ob: "DESC",
  PageNo: 0,
  PageSize: 20,
  search: "",
  Filteruser_id: null,
  user_id: store.getters.user_id,
  IsType: 0,
  SearchTextUser: "",
  filter_type: 0,
  month: today.getMonth() + 1,
  year: today.getFullYear(),
  loctitle: "Lọc",
  type_view: 1,
  DateTime: today,
});

const isEdit = ref(false);
const editData = (data) => {
  length.value = false;
  personReport.value = null;
  isEdit.value = true;
  headerDialog.value = "Sửa báo cáo công việc";
  personReport.value = data;
  personReport.value.report_id = data.report_id;
  let task_id = [];
  task_id = personReport.value.list_task_id1;
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "task_origin_list_review_person",
            par: [
              { par: "user_id", va: "" },
              { par: "string", va: data.list_task_id },
            ],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let data1 = JSON.parse(response.data.data)[0];
      if (data1.length > 0) {
        data1.forEach((element, i) => {
          element.progress = element.progress == null ? 0 : element.progress;
          element.update_date = element.modified_date
            ? element.modified_date
            : element.created_date;
          element.status_name =
            element.status != null
              ? listDropdownStatus.value.filter(
                  (x) => x.value == element.status,
                )[0].text
              : "";
          element.status_bg_color =
            element.status != null
              ? listDropdownStatus.value.filter(
                  (x) => x.value == element.status,
                )[0].bg_color
              : "";
          element.status_text_color =
            element.status != null
              ? listDropdownStatus.value.filter(
                  (x) => x.value == element.status,
                )[0].text_color
              : "";
          //thời gian xử lý
          if (element.end_date != null) {
            if (element.thoigianquahan < 0) {
              if (element.thoigianxuly > 0) {
                element.title_time = element.thoigianxuly + " ngày";
                element.time_bg = element.status_bg_color;
                element.time_color = "color: #fff;";
              }
            } else {
              if (element.thoigianquahan > 0) {
                element.title_time =
                  "Quá hạn " + element.thoigianquahan + " ngày";
                element.time_bg = "red";
                element.time_color = "color: #fff;";
              }
            }
          } else if (element.thoigianxuly) {
            element.title_time = element.thoigianxuly + " ngày";
            element.time_bg = element.status_bg_color;
            element.time_color = "color: #fff;";
          }
          element.Thanhviens = element.Thanhviens
            ? JSON.parse(element.Thanhviens)
            : [];
          element.ThanhvienShows = [];
          if (element.Thanhviens.length > 3) {
            element.ThanhvienShows = element.Thanhviens.slice(0, 3);
          } else {
            element.ThanhvienShows = [...element.Thanhviens];
          }
        });
      }

      personReport.value.list_task_id = null;
      personReport.value.list_task_id = [];
      task_id.forEach((x) => {
        let task = data1.filter((z) => z.task_id == x);
        if (task != null) {
          personReport.value.list_task_id =
            personReport.value.list_task_id.concat(task);
        }
      });
      personReport.value.messages = data.messages.replace(/<br\s*\/?>/gi, "\n");
      DialogVisible.value = true;
    })
    .catch((error) => {
      //   toast.error("Tải dữ liệu không thành công6!");
      optionsLinkTask.value.loading = false;
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const selectedReport = ref([]);
const checkDelList = ref();
watch(selectedReport, () => {
  if (selectedReport.value.length > 0) {
    checkDelList.value = true;
  } else {
    checkDelList.value = false;
  }
});
const saveData = (isFormValid, type) => {
  submitted.value = true;
  if (!isFormValid || length.value == true) {
    return;
  }
  let data = Object.assign({}, personReport.value);
  data.list_task_id = "";
  personReport.value.list_task_id.forEach((x) => {
    data.list_task_id += (data.list_task_id != "" ? "," : "") + x.task_id;
  });
  data.messages = data.messages.replace(/\n/g, "<br/>");
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: isEdit.value ? "put" : "post",
    url:
      baseURL +
      `/api/create_Person_Report/${
        isEdit.value
          ? "Update_report"
          : type == 1
          ? "add_Report_BH"
          : "add_Report"
      }`,
    data: data,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        if (response.data.ms != "" && response.data.ms != null && type == 1) {
          swal.fire({
            title: "Thông báo",
            html: response.data.ms,
            icon: "error",
            confirmButtonText: "OK",
          });
        } else {
          toast.success(
            response.config.method == "put"
              ? "Cập nhật báo cáo công việc cá nhân thành công!"
              : "Thêm báo cáo công việc cá nhân thành công",
          );
        }
        isEdit.value = false;
        loadData();
        DialogVisible.value = false;
      } else {
        let ms = response.data.ms;
        swal.fire({
          title: "Thông báo!",
          html: ms,
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    })
    .catch((error) => {
      swal.close();
      swal.fire({
        title: "Thông báo",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};

//load EveryThings U need
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
    text: "Đợi đánh giá",
    bg_color: "#33c9dc",
    text_color: "#FFFFFF",
  },
  { value: 6, text: "Bị trả lại", bg_color: "#ffa500", text_color: "#FFFFFF" },
  { value: 7, text: "HT sau hạn", bg_color: "#ff8b4e", text_color: "#FFFFFF" },
  { value: 8, text: "Đã đánh giá", bg_color: "#51b7ae", text_color: "#FFFFFF" },
  { value: -1, text: "Bị xóa", bg_color: "red", text_color: "#FFFFFF" },
]);
const listTaskLink = ref();
const onPage = (event) => {
  if (event.rows != options.value.PageSize) {
    options.value.PageSize = event.rows;
  }
  options.value.PageNo = event.page;
  loadData();
};
const first = ref();
const LoadLinkTaskOrigin = () => {
  optionsLinkTask.value.loading = true;

  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "task_origin_list_to_person_reports",
            par: [
              { par: "user_id", va: user.user_id },
              { par: "month", va: optionsLinkTask.value.month },
              { par: "year", va: optionsLinkTask.value.year },
            ],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let data1 = JSON.parse(response.data.data)[0];
      if (data1.length > 0) {
        data1.forEach((element, i) => {
          element.progress = element.progress == null ? 0 : element.progress;
          element.update_date = element.modified_date
            ? element.modified_date
            : element.created_date;
          element.status_name =
            element.status != null
              ? listDropdownStatus.value.filter(
                  (x) => x.value == element.status,
                )[0].text
              : "";
          element.status_bg_color =
            element.status != null
              ? listDropdownStatus.value.filter(
                  (x) => x.value == element.status,
                )[0].bg_color
              : "";
          element.status_text_color =
            element.status != null
              ? listDropdownStatus.value.filter(
                  (x) => x.value == element.status,
                )[0].text_color
              : "";
          //thời gian xử lý
          if (element.end_date != null) {
            if (element.thoigianquahan < 0) {
              if (element.thoigianxuly > 0) {
                element.title_time = element.thoigianxuly + " ngày";
                element.time_bg = element.status_bg_color;
                element.time_color = "color: #fff;";
              }
            } else {
              if (element.thoigianquahan > 0) {
                element.title_time =
                  "Quá hạn " + element.thoigianquahan + " ngày";
                element.time_bg = "red";
                element.time_color = "color: #fff;";
              }
            }
          } else if (element.thoigianxuly) {
            element.title_time = element.thoigianxuly + " ngày";
            element.time_bg = element.status_bg_color;
            element.time_color = "color: #fff;";
          }
          element.Thanhviens = element.Thanhviens
            ? JSON.parse(element.Thanhviens)
            : [];
          element.ThanhvienShows = [];
          if (element.Thanhviens.length > 3) {
            element.ThanhvienShows = element.Thanhviens.slice(0, 3);
          } else {
            element.ThanhvienShows = [...element.Thanhviens];
          }
          element.files = element.files ? JSON.parse(element.files) : [];
          element.files = element.files ? element.files : [];
          element.STT =
            optionsLinkTask.value.PageNo * optionsLinkTask.value.PageSize +
            i +
            1;
        });
      }
      listTaskLink.value = data1;
      if (ChangeMonth.value == true) {
        personReport.value.list_task_id = [];
        personReport.value.list_task_id = [...listTaskLink.value];
        ChangeMonth.value = false;
      }
      optionsLinkTask.value.month = today.getMonth();
      optionsLinkTask.value.year = today.getFullYear();
      optionsLinkTask.value.loading = false;
      swal.close();
    })
    .catch((error) => {
      //   toast.error("Tải dữ liệu không thành công6!");
      optionsLinkTask.value.loading = false;
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const widthScreen = ref(window.screen.width);

const options = ref({
  PageNo: 0,
  PageSize: 20,
  totalRecords: 0,
  loading: true,
  month: null,
  year: null,
  DateTime: null,
});
const datalists = ref();
const noData = ref(true);
const status = ref([
  { code: 0, label: "Khởi tạo", bgColor: "#2196F3", text: "#FFFFFF" },
  { code: 1, label: "Đang trình duyệt", bgColor: "#FF6E31", text: "#FFFFFF" },
  { code: 2, label: "Đã duyệt", bgColor: "#6DD230", text: "#FFFFFF" },
  { code: 3, label: "Trả lại", bgColor: "#FF0000", text: "#FFFFFF" },
]);
const loadData = () => {
  options.value.loading = true;

  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "task_person_report_list_total",
            par: [
              { par: "user_id", va: user.user_id },
              { par: "pageno", va: options.value.PageNo },
              {
                par: "pagesize",
                va: options.value.PageSize,
              },
              { par: "search", va: options.value.SearchText },
              { par: "month", va: options.value.month },
              { par: "year", va: options.value.year },
              { par: "search_user", va: options.value.selectedUser },
              { par: "search_user", va: options.value.selectedOrg },
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
      let count = JSON.parse(response.data.data)[1];
      options.value.totalRecords = count[0].totalRecords;
      data.forEach((x, i) => {
        let temp = "";
        temp = x.list_task_id;
        x.list_task_id1 = [];
        x.list_task_id1 = temp.split(",");
        x.status_display = status.value.filter((a) => a.code == x.status)[0];
        x.stt = options.value.PageNo * options.value.PageSize + (i + 1);
        x.user_info = JSON.parse(x.user_info);
      });

      datalists.value = data;
      if (data.length > 0) {
        noData.value = false;
      }
      options.value.loading = false;
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;
      addLog({
        title: "Lỗi Console loadData",
        controller: "SignerView.vue",
        logcontent: error.message,
        loai: 2,
      });
      if (error && error.status === 401) {
        swal.fire({
          title: "Thông báo",
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const SidebarVisible = ref(false);
const pDataa = ref();
const showInfo = (data) => {
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "task_origin_list_review_person",
            par: [
              { par: "user", va: "" },
              { par: "string", va: data.list_task_id },
            ],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let data1 = JSON.parse(response.data.data)[0];
      if (data1.length > 0) {
        data1.forEach((element, i) => {
          element.progress = element.progress == null ? 0 : element.progress;
          element.update_date = element.modified_date
            ? element.modified_date
            : element.created_date;
          element.status_name =
            element.status != null
              ? listDropdownStatus.value.filter(
                  (x) => x.value == element.status,
                )[0].text
              : "";
          element.status_bg_color =
            element.status != null
              ? listDropdownStatus.value.filter(
                  (x) => x.value == element.status,
                )[0].bg_color
              : "";
          element.status_text_color =
            element.status != null
              ? listDropdownStatus.value.filter(
                  (x) => x.value == element.status,
                )[0].text_color
              : "";
          //thời gian xử lý
          if (element.end_date != null) {
            if (element.thoigianquahan < 0) {
              if (element.thoigianxuly > 0) {
                element.title_time = element.thoigianxuly + " ngày";
                element.time_bg = element.status_bg_color;
                element.time_color = "color: #fff;";
              }
            } else {
              if (element.thoigianquahan > 0) {
                element.title_time =
                  "Quá hạn " + element.thoigianquahan + " ngày";
                element.time_bg = "red";
                element.time_color = "color: #fff;";
              }
            }
          } else if (element.thoigianxuly) {
            element.title_time = element.thoigianxuly + " ngày";
            element.time_bg = element.status_bg_color;
            element.time_color = "color: #fff;";
          }
          element.Thanhviens = element.Thanhviens
            ? JSON.parse(element.Thanhviens)
            : [];
          element.ThanhvienShows = [];
          if (element.Thanhviens.length > 3) {
            element.ThanhvienShows = element.Thanhviens.slice(0, 3);
          } else {
            element.ThanhvienShows = [...element.Thanhviens];
          }
        });
      }
      pDataa.value = [];
      let datasend = Object.assign({}, data);
      datasend.task_info = [];
      datasend.list_task_id1.forEach((x) => {
        let task = data1.filter((z) => z.task_id == x);
        if (task != null) {
          datasend.task_info.push(task[0]);
        }
      });
      pDataa.value = datasend;
      SidebarVisible.value = true;
    })
    .catch((error) => {
      //   toast.error("Tải dữ liệu không thành công6!");
      optionsLinkTask.value.loading = false;
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};

const refresh = () => {
  first.value = 0;
  options.value = {
    PageNo: 0,
    PageSize: 20,
    totalRecords: 0,
    SearchText: null,
    DateTime: null,
    selectUser: null,
    selectedUser: null,
    selectOrg: null,
    selectedOrg: null,
  };
  drdModel.value = null;
  loadData();
};
const length = ref(false);
const checkLenght = () => {
  length.value = false;
  const textbox = document.getElementById("report_name");
  if (textbox.value.length > 250) {
    length.value = true;
  }
  return length.value;
};

const ProcessingVisible = ref(false);
const processing_id = ref();
const ShowProcess = (data) => {
  processing_id.value = data.report_id;
  ProcessingVisible.value = true;
};
const BHBQP = ref(true);
const ChangeMonth = ref(false);
const MonthChange2 = (event) => {
  options.value.month = event.getMonth() + 1;
  options.value.year = event.getFullYear();
};
const style = ref({
  "background-color": "#2196F3 !important",
  color: "#fff !important",
  " border": "1px solid #5ca7e3 !important",
});
const styleObj = ref();
const op = ref();
const toggle = (event) => {
  drdModel.value = 1;
  op.value.toggle(event);
  listDepartment();
};
const reNewFilter = () => {
  options.value.PageNo = 0;
  options.value.year = null;
  options.value.month = null;
  options.value.DateTime = null;
  options.value.selectUser = null;
  options.value.selectedUser = null;
  options.value.selectOrg = null;
  options.value.selectedOrg = null;
  drdModel.value = 1;
  styleObj.value = null;
  first.value = 0;
  loadData();
};
const opt = [
  { va: 0, label: "Cá nhân" },
  { va: 1, label: "Phòng ban" },
];
const drdModel = ref();
const listDropdownUser = ref();
const listDropdownDepartment = ref();
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
      listDropdownUser.value = data.map((x) => ({
        name: x.full_name,
        code: x.user_id,
        avatar: x.avatar,
        position_name: x.position_name,
        department_name: x.department_name,
      }));
    })
    .catch((error) => {
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
const renderTree = (data) => {
  let arrChils = [];
  let arrtreeChils = [];
  data
    .filter((x) => x.parent_id != null)
    .forEach((m, i) => {
      m.IsOrder = i + 1;
      let om = { key: m.organization_id, label: m.organization_name };
      const rechildren = (mm, pid) => {
        let dts = data.filter((x) => x.parent_id == pid);
        if (dts.length > 0) {
          if (!mm.children) mm.children = [];
          dts.forEach((em) => {
            let om1 = { key: em.organization_id, label: em.organization_name };
            rechildren(om1, em.organization_id);
            mm.children.push(om1);
          });
        }
      };
      rechildren(om, m.organization_id);
      arrChils.push(om);
      arrtreeChils.push(om);
    });

  return { arrChils: arrChils, arrtreeChils: arrtreeChils };
};

const listDepartment = () => {
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_organization_list_department",
            par: [
              { par: "user_id", va: user.user_id },
              { par: "org_type", va: null },
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
      let Data = renderTree(data);
      listDropdownDepartment.value = Data.arrtreeChils;
    })
    .catch((error) => {
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
const drdModelChange = (e) => {
  options.value.selectedUser = null;
  options.value.selectUser = null;
  options.value.selectOrg = null;
  options.value.selectedOrg = null;
  if (e.value == 0) listUser();
  else {
    listDepartment();
  }
};
const listExp = ref([]);
const ExportToWord = () => {
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "task_person_report_list_total",
            par: [
              { par: "user_id", va: user.user_id },
              { par: "pageno", va: 0 },
              {
                par: "pagesize",
                va: 9999999,
              },
              { par: "search", va: options.value.SearchText },
              { par: "month", va: options.value.month },
              { par: "year", va: options.value.year },
              { par: "search_user", va: options.value.selectedUser },
              { par: "org_id", va: options.value.selectedOrg },
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
      let count = JSON.parse(response.data.data)[1];
      options.value.totalRecords = count[0].totalRecords;
      data.forEach((x, i) => {
        let temp = "";
        temp = x.list_task_id;
        x.list_task_id1 = [];
        x.list_task_id1 = temp.split(",");
        x.status_display = status.value.filter((a) => a.code == x.status)[0];
        x.stt = options.value.PageNo * options.value.PageSize + (i + 1);
        x.user_info = JSON.parse(x.user_info);
      });

      listExp.value = [];
      listExp.value = data;
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;
      addLog({
        title: "Lỗi Console loadData",
        controller: "SignerView.vue",
        logcontent: error.message,
        loai: 2,
      });
      if (error && error.status === 401) {
        swal.fire({
          title: "Thông báo",
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
  options.value.loading = true;
  setTimeout(function () {
    print();
    options.value.loading = false;
  }, 500);
};
const print = () => {
  var htmltable = "";
  htmltable = renderhtml("formprint", htmltable);
  var printframe = window.frames["printframe"];
  printframe.document.write(htmltable);
  setTimeout(function () {
    document.title = "Đánh giá công việc";
    printframe.print();
    printframe.document.close();
  }, 0);
};
function renderhtml(id, htmltable) {
  htmltable = "";
  //Style
  htmltable += `<style type="text/css" media="print">

  @page { size: landscape; }
</style>
  <style>
    #formprint {
      background: #fff !important;
    }
    #formprint * {
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

const ChangeUser = (e) => {
  options.value.selectedUser = e.value.code;
};
const ChangeDept = (e) => {
  options.value.selectedOrg = null;
  let temp = null;
  temp = Object.keys(e);
  temp.forEach((x) => {
    options.value.selectedOrg += "," + x;
  });
  options.value.selectedOrg = options.value.selectedOrg.substring(5);
};
onMounted(() => {
  loadData();
  LoadLinkTaskOrigin();

  if (user.organization_id == 9) {
    BHBQP.value = true;
  } else {
    BHBQP.value = false;
  }
  BHBQP.value = true;

  return { selectedReport };
});
</script>
<template>
  <div class="main-layout true flex-grow-1 p-2">
    <DataTable
      v-model:first="first"
      :value="datalists"
      :paginator="true"
      :rows="options.PageSize"
      :scrollable="true"
      scrollHeight="flex"
      :loading="options.loading"
      v-model:selection="selectedReport"
      :lazy="true"
      @page="onPage($event)"
      @filter="onFilter($event)"
      @sort="onSort($event)"
      :totalRecords="options.totalRecords"
      dataKey="report_id"
      :rowHover="true"
      :showGridlines="true"
      paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
      :rowsPerPageOptions="[20, 30, 50, 100, 200]"
      responsiveLayout="scroll"
      @row-dblclick="showInfo($event.data)"
      rowGroupMode="subheader"
      groupRowsBy="user_info.department_name"
    >
      <template #header>
        <h3 class="module-title mt-0 ml-1 mb-2">
          <i class="pi pi-sliders-v"></i> Danh sách báo cáo công việc ({{
            options.totalRecords
          }})
        </h3>
        <Toolbar class="w-full custoolbar">
          <template #start>
            <span class="p-input-icon-left">
              <i class="pi pi-search" />
              <InputText
                v-model="options.SearchText"
                @keyup.enter="loadData()"
                type="text"
                spellcheck="false"
                placeholder="Tìm kiếm"
              />
            </span>
            <Button
              type="button"
              class="ml-2 p-button-outlined p-button-secondary"
              icon="pi pi-filter"
              @click="toggle"
              aria:haspopup="true"
              aria-controls="overlay_panel"
              v-tooltip.bottom="'Bộ lọc'"
              :style="[styleObj]"
            />
            <OverlayPanel
              ref="op"
              appendTo="body"
              class="w-30rem p-0 m-0"
              :showCloseIcon="false"
              id="overlay_panel"
              style="z-index: 999"
            >
              <div class="col-12 w-max-20rem">
                <div class="col-12 flex format-left">
                  <div class="col-3">Lọc theo:</div>
                  <Dropdown
                    :modelValue="1"
                    v-model="drdModel"
                    :options="opt"
                    optionLabel="label"
                    optionValue="va"
                    placeholder="Loại"
                    class="col-9 py-0"
                    @change="drdModelChange($event)"
                  />
                </div>

                <div
                  v-if="drdModel == 0"
                  class="col-12 flex format-left"
                >
                  <div class="col-3">Người dùng:</div>
                  <Dropdown
                    :filter="true"
                    v-model="options.selectUser"
                    :options="listDropdownUser"
                    optionLabel="name"
                    class="col-9"
                    placeholder="Chọn người báo cáo"
                    display="chip"
                    panelClass="d-design-dropdown"
                    @change="ChangeUser($event)"
                  >
                    <template #value="slotProps">
                      <div
                        class="flex"
                        v-if="slotProps.value"
                      >
                        <div
                          class="flex"
                          style="margin-left: 10px"
                        >
                          <Avatar
                            v-bind:label="
                              slotProps.value.avatar
                                ? ''
                                : (slotProps.value.name ?? '').substring(0, 1)
                            "
                            v-bind:image="
                              basedomainURL + slotProps.value.avatar
                            "
                            style="
                              background-color: #2196f3;
                              color: #ffffff;
                              width: 32px;
                              height: 32px;
                              font-size: 15px !important;
                              margin-left: -10px;
                            "
                            :style="{
                              background: bgColor[10000 % 7] + '!important',
                            }"
                            class="cursor-pointer"
                            size="xlarge"
                            shape="circle"
                          />
                          <div
                            class="pt-1"
                            style="padding-left: 10px"
                          >
                            {{ slotProps.value.name }}
                          </div>
                        </div>
                      </div>
                      <span v-else>
                        {{ slotProps.placeholder }}
                      </span>
                    </template>
                    <template #option="slotProps">
                      <div
                        class="country-item flex"
                        style="align-items: center; margin-left: 10px"
                      >
                        <Avatar
                          v-bind:label="
                            slotProps.option.avatar
                              ? ''
                              : (slotProps.option.name ?? '').substring(0, 1)
                          "
                          v-bind:image="basedomainURL + slotProps.option.avatar"
                          style="
                            background-color: #2196f3;
                            color: #ffffff;
                            width: 32px;
                            height: 32px;
                            font-size: 15px !important;
                            margin-left: -10px;
                          "
                          :style="{
                            background:
                              bgColor[slotProps.index % 7] + '!important',
                          }"
                          class="cursor-pointer"
                          size="xlarge"
                          shape="circle"
                        />
                        <div>
                          <div
                            class="pt-1"
                            style="padding-left: 10px"
                          >
                            {{ slotProps.option.name }}
                          </div>
                          <div
                            class="pt-1"
                            style="padding-left: 10px"
                          >
                            {{ slotProps.option.position_name }}
                          </div>
                          <div
                            class="pt-1"
                            style="padding-left: 10px"
                          >
                            {{ slotProps.option.department_name }}
                          </div>
                        </div>
                      </div>
                    </template>
                  </Dropdown>
                </div>

                <div
                  v-if="drdModel == 1"
                  class="col-12 flex format-left"
                >
                  <div class="col-3">Phòng:</div>

                  <TreeSelect
                    v-model="options.selectOrg"
                    :options="listDropdownDepartment"
                    display="chip"
                    selectionMode="multiple"
                    :metaKeySelection="false"
                    placeholder="Chọn phòng ban"
                    panelClass="d-design-dropdown"
                    class="col-9"
                    @change="ChangeDept($event)"
                  ></TreeSelect>
                </div>

                <div class="col-12 flex format-left">
                  <div class="col-3">Tháng:</div>
                  <Calendar
                    inputId="basic"
                    v-model="options.DateTime"
                    autocomplete="off"
                    view="month"
                    dateFormat="MM/yy"
                    :showIcon="true"
                    :manualInput="false"
                    @date-select="MonthChange2($event)"
                    placeholder="Chọn tháng năm"
                    class="col-9 p-0"
                  />
                </div>

                <Toolbar class="border-none surface-0 outline-none pb-0 w-full">
                  <template #start>
                    <Button
                      @click="reNewFilter()"
                      class="p-button-outlined"
                      label="Xóa"
                    >
                    </Button>
                  </template>
                  <template #end>
                    <Button
                      @click="loadData(), (styleObj = style)"
                      label="Lọc"
                    ></Button>
                  </template>
                </Toolbar>
              </div>
            </OverlayPanel>
          </template>

          <template #end>
            <Button
              @click="refresh()"
              class="mr-2 p-button-outlined p-button-secondary"
              icon="pi pi-refresh"
              v-tooltip.bottom="'Tải lại'"
            />
            <Button
              @click="ExportToWord()"
              class="mr-2"
              label="In báo cáo"
              icon="pi pi-print"
            />
          </template>
        </Toolbar>
      </template>
      <template #groupheader="slotProps">
        <span class="image-text">{{
          slotProps.data.user_info.department_name
        }}</span>
      </template>
      <Column
        field="stt"
        header="STT"
        headerStyle="text-align:center;max-width:75px;height:50px"
        bodyStyle="text-align:center;max-width:75px "
        class="align-items-center justify-content-center text-center"
      ></Column>
      <Column
        field="is_type"
        header="Người tạo"
        headerStyle="text-align:center;max-width:10rem;height:50px"
        bodyStyle="text-align:center;max-width:10rem"
        class="align-items-center justify-content-center text-center"
      >
        <template #body="data">
          <Avatar
            v-tooltip.bottom="{
              value:
                'Người tạo báo cáo: <br/>' +
                data.data.user_info.full_name +
                '<br/>' +
                (data.data.user_info.position_name || '') +
                '<br/>' +
                (data.data.user_info.department_name ||
                  data.data.user_info.organization_name),
              escape: true,
            }"
            v-bind:label="
              data.data.user_info.avatar
                ? ''
                : data.data.user_info.full_name
                    .split(' ')
                    .at(-1)
                    .substring(0, 1)
            "
            v-bind:image="basedomainURL + data.data.user_info.avatar"
            style="color: #ffffff; cursor: pointer"
            :style="{
              background: 'pink',
              border: '2px solid #fffa8d',
            }"
            class="flex p-0 m-0"
            size=""
            shape="circle"
          />
        </template>
      </Column>
      <Column
        field="report_name"
        header="Tên báo cáo"
        headerStyle="text-align:center;height:50px"
        bodyStyle=""
        headerClass="align-items-center justify-content-center"
      >
        <template #body="data">
          <div
            class="name-hover"
            @click="showInfo(data.data)"
          >
            <span>{{ data.data.report_name }}</span>
          </div>
        </template>
      </Column>
      <Column
        field="self_point"
        header="Điểm tự đánh giá"
        headerStyle="text-align:center;max-width:10rem;height:50px"
        bodyStyle="text-align:center;max-width:10rem"
        class="align-items-center justify-content-center text-center"
      >
      </Column>
      <Column
        field="reviewed_point"
        header="Điểm được đánh giá"
        headerStyle="text-align:center;max-width:12rem;height:50px"
        bodyStyle="text-align:center;max-width:12rem"
        class="align-items-center justify-content-center text-center"
      >
      </Column>
      <Column
        field="status"
        header="Trạng thái"
        headerStyle="text-align:center;max-width:10rem;height:50px"
        bodyStyle="text-align:center;max-width:10rem"
        class="align-items-center justify-content-center text-center"
      >
        <template #body="data">
          <Chip
            :style="{
              background: data.data.status_display.bgColor,
              color: data.data.status_display.text,
            }"
            v-bind:label="data.data.status_display.label"
          />
        </template>
      </Column>
      <Column
        field=""
        header="Chức năng"
        headerStyle="text-align:center;height:50px;max-width:20rem"
        bodyStyle="max-width:20rem"
        class="align-items-center justify-content-center text-center"
      >
        <template #body="data">
          <div>
            <Button
              @click="showInfo(data.data)"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-info-circle"
              v-tooltip.bottom="'Thông tin báo cáo'"
            ></Button>

            <Button
              @click="editData(data.data)"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-pencil"
              v-tooltip.bottom="'Sửa'"
            ></Button>
            <Button
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-chart-bar"
              v-tooltip.bottom="'Quy trình xử lý'"
              @click="ShowProcess(data.data)"
            ></Button>
          </div>
        </template>
      </Column>
      <template #empty>
        <div
          class="block w-full h-full format-center"
          v-if="noData == true"
        >
          <img
            src="../../../assets/background/nodata.png"
            height="144"
          />
          <h3 class="m-1">Không có dữ liệu</h3>
        </div>
      </template>
    </DataTable>

    <iframe
      name="printframe"
      id="printframe"
      style="display: none"
    ></iframe>
  </div>
  <Dialog
    :header="headerDialog"
    v-model:visible="DialogVisible"
    :style="{ width: widthScreen < 1800 ? '85vw' : '70vw' }"
    :closable="true"
    :maximizable="true"
  >
    <div class="col-12">
      <div class="col-12 format-left">
        <div class="col-2">Tên báo cáo <span class="redsao">(*)</span></div>
        <Textarea
          id="report_name"
          v-model="personReport.report_name"
          spellcheck="false"
          class="col-10"
          rows="2"
          :class="{
            'p-invalid':
              (v$.report_name.$invalid && submitted) || length == true,
          }"
          autocomplete="off"
          disabled
          @input="checkLenght()"
        />
      </div>
      <div
        style="display: flex"
        class="col-12 py-0"
        v-if="length == true"
      >
        <div class="col-2 p-0 text-left"></div>
        <small class="col-10 p-0 p-error">
          <span class="col-12">Tên báo cáo không quá 250 ký tự!</span>
        </small>
      </div>
      <div
        style="display: flex"
        class="col-12 py-0"
        v-if="
          (v$.report_name.$invalid && submitted) ||
          v$.report_name.$pending.$response
        "
      >
        <div class="col-2 p-0 text-left"></div>
        <small class="col-10 p-0 p-error">
          <span class="col-12">{{
            v$.report_name.required.$message
              .replace("Value", "Tên báo cáo")
              .replace("is required", "không được để trống")
          }}</span>
        </small>
      </div>
      <div
        class="col-12 flex"
        :class="{
          'p-invalid': v$.list_task_id.$invalid && submitted,
        }"
      >
        <div class="col-2 format-left">
          Công việc thực hiện: <span class="redsao"> (*)</span>
        </div>

        <div
          class="col-4 p-0 format-left text-red_600"
          v-if="
            (v$.list_task_id.$invalid && submitted) ||
            v$.list_task_id.$pending.$response
          "
        >
          <small class="col-10 p-0 p-error">
            <span class="col-12">{{
              v$.list_task_id.required.$message.replace(
                "Value is required",
                "Bạn chưa chọn công việc!",
              )
            }}</span>
          </small>
        </div>
      </div>
      <OrderList
        v-model="personReport.list_task_id"
        listStyle="height:auto"
        dataKey="task_id"
        class="col-12"
      >
        <template #header> Danh sách công việc</template>
        <template #item="data">
          <div class="col-12 p-0 flex">
            <div class="col-1 p-0 flex">
              <div class="col-2 font-bold p-0 format-center">
                {{ data.index + 1 }}
              </div>
              <div class="col-9 p-0 format-center">
                <Avatar
                  v-tooltip.bottom="{
                    value:
                      'Người tạo công việc: <br/>' +
                      data.item.full_name +
                      '<br/>' +
                      (data.item.position_name || '') +
                      '<br/>' +
                      (data.item.tenToChuc || ''),
                    escape: true,
                  }"
                  v-bind:label="
                    data.item.avatar
                      ? ''
                      : (data.item.last_name ?? '').substring(0, 1)
                  "
                  v-bind:image="basedomainURL + data.item.avatar"
                  :style="{
                    background: bgColor[0] + '!important',
                    'border-color': '#ffffff',
                  }"
                  class="cursor-pointer"
                  size="small"
                  shape="circle"
                />
              </div>
            </div>

            <div class="col-4 p-0">
              <div style="display: flex; flex-direction: column; padding: 5px">
                <div style="min-height: 25px">
                  <span style="font-weight: bold; font-size: 14px">{{
                    data.item.task_name
                  }}</span>
                </div>
                <div style="font-size: 12px">
                  <span
                    v-if="data.item.start_date || data.item.end_date"
                    style="color: #98a9bc"
                    >{{
                      data.item.start_date
                        ? moment(new Date(data.item.start_date)).format(
                            "DD/MM/YYYY",
                          )
                        : null
                    }}
                    -
                    {{
                      data.item.end_date
                        ? moment(new Date(data.item.end_date)).format(
                            "DD/MM/YYYY",
                          )
                        : null
                    }}</span
                  >
                </div>
                <div
                  v-if="data.item.project_name"
                  style="min-height: 25px; display: flex; align-items: center"
                >
                  <i class="pi pi-tag"></i>
                  <span
                    class="duan"
                    style="
                      font-size: 13px;
                      font-weight: 400;
                      margin-left: 5px;
                      color: #0078d4;
                    "
                    >{{ data.item.project_name }}</span
                  >
                </div>
              </div>
            </div>
            <div class="col-7 flex p-0">
              <div class="col-11 flex">
                <div class="col-4 flex">
                  <div class="col-8 p-0 format-center">
                    <AvatarGroup>
                      <div
                        v-for="(value, index) in data.item.ThanhvienShows"
                        :key="index"
                      >
                        <div>
                          <Avatar
                            v-tooltip.bottom="{
                              value:
                                value.type_name +
                                ': ' +
                                value.full_name +
                                '<br/>' +
                                (value.position_name || '') +
                                '<br/>' +
                                (value.tenToChuc || ''),
                              escape: true,
                            }"
                            v-bind:label="
                              value.avatar
                                ? ''
                                : (value.ten ?? '').substring(0, 1)
                            "
                            v-bind:image="basedomainURL + value.avatar"
                            style="
                              background-color: #2196f3;
                              color: #ffffff;
                              width: 32px;
                              height: 32px;
                              font-size: 15px !important;
                              margin-left: -10px;
                            "
                            :style="{
                              background: bgColor[index % 7] + '!important',
                            }"
                            class="cursor-pointer"
                            size="xlarge"
                            shape="circle"
                          />
                        </div>
                      </div>
                      <Avatar
                        v-if="
                          data.item.Thanhviens.length -
                            data.item.ThanhvienShows.length >
                          0
                        "
                        :label="
                          '+' +
                          (data.item.Thanhviens.length -
                            data.item.ThanhvienShows.length) +
                          ''
                        "
                        v-tooltip.bottom="{
                          value:
                            'và ' +
                            (data.item.Thanhviens.length -
                              data.item.ThanhvienShows.length) +
                            ' người khác tham gia',
                        }"
                        class="cursor-pointer"
                        shape="circle"
                        style="
                          background-color: #e9e9e9 !important;
                          color: #98a9bc;
                          font-size: 14px !important;
                          width: 32px;
                          margin-left: -10px;
                          height: 32px;
                        "
                      />
                    </AvatarGroup>
                  </div>
                  <div class="col-4 p-1 format-center">
                    <span v-if="data.item.progress == 0"
                      >{{ data.item.progress }} %</span
                    >
                    <div
                      v-if="data.item.progress != 0"
                      style="width: 100%"
                    >
                      <ProgressBar :value="data.item.progress" />
                    </div>
                  </div>
                </div>
                <div class="col-3 format-center">
                  <div v-if="data.item.title_time">
                    <span
                      style="
                        font-size: 10px;
                        font-weight: bold;
                        padding: 5px;
                        border-radius: 5px;
                      "
                      :style="{
                        background: data.item.time_bg,
                        color: data.item.status_text_color,
                      }"
                      >{{ data.item.title_time }}</span
                    >
                  </div>
                </div>
                <div class="col-2 format-center">
                  <div
                    v-if="data.item.end_date != null"
                    style="
                      background-color: #fff8ee;
                      padding: 10px 20px;
                      border-radius: 5px;
                    "
                  >
                    <span
                      style="color: #ffab2b; font-size: 13px; font-weight: bold"
                      >{{
                        moment(new Date(data.item.end_date)).format(
                          "DD/MM/YYYY",
                        )
                      }}</span
                    >
                  </div>
                </div>
                <div class="col-3 p-2 format-center">
                  <Chip
                    :style="{
                      background: data.item.status_bg_color,
                      color: data.item.status_text_color,
                    }"
                    class="format-center"
                    v-bind:label="data.item.status_name"
                  />
                </div>
                <div class="col-1 p-0 format-center">
                  <!-- <a class="btn-c-hover">
                    <Button
                      icon="pi pi-trash"
                      class="p-button-text"
                      @click="removeTask(data.item, personReport.list_task_id)"
                      v-tooltip.bottom="'xóa'"
                    ></Button>
                  </a> -->
                </div>
              </div>
            </div>
          </div>
        </template>
      </OrderList>
      <div class="col-12 flex">
        <div class="col-2 format-left">
          Nội dung báo cáo<span class="redsao">(*)</span>
        </div>
        <Textarea
          v-model="personReport.messages"
          spellcheck="false"
          class="col-10"
          rows="10"
          autocomplete="off"
          disabled
          :class="{
            'p-invalid': v$.messages.$invalid && submitted,
          }"
        />
      </div>
      <div class="col-12 flex">
        <div class="col-2"></div>
        <div
          class="col-10 p-0 format-left text-red_600"
          v-if="
            (v$.messages.$invalid && submitted) ||
            v$.messages.$pending.$response
          "
        >
          <small class="col-10 p-0 p-error">
            <span class="col-12">{{
              v$.messages.required.$message.replace(
                "Value is required",
                "Nội dung báo cáo không được để trống!",
              )
            }}</span>
          </small>
        </div>
      </div>

      <div class="col-12 flex">
        <div class="col-2">
          Điểm đánh giá (0-100)<span class="redsao">(*)</span>
        </div>
        <InputNumber
          v-model="personReport.reviewed_point"
          spellcheck="false"
          class="col-10 p-0"
          mode="decimal"
          showButtons
          :min="0"
          :max="100"
          :useGrouping="false"
          autocomplete="off"
          :class="{
            'p-invalid': v$.reviewed_point.$invalid && submitted,
          }"
        />
      </div>
      <div class="col-12 flex">
        <div class="col-2"></div>
        <div
          class="col-10 p-0 format-left text-red_600"
          v-if="
            (v$.reviewed_point.$invalid && submitted) ||
            v$.reviewed_point.$pending.$response
          "
        >
          <small class="col-10 p-0 p-error">
            <span class="col-12">{{
              v$.self_point.required.$message.replace(
                "Value is required",
                "Điểm đánh giá không được để trống!",
              )
            }}</span>
          </small>
        </div>
      </div>
    </div>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="(DialogVisible = false), (isEdit = false), loadData()"
        class="p-button-text"
      />
      <Button
        label="Lưu"
        icon="pi pi-check"
        @click="saveData(!v$.$invalid, 0)"
      />
    </template>
  </Dialog>

  <Dialog
    v-model:visible="SidebarVisible"
    :style="'width: 70vw;'"
    :showCloseIcon="true"
    header="Thông tin chi tiết"
    maximizable="true"
  >
    <DetailReportVue :data="pDataa"></DetailReportVue>
    <template #footer>
      <div class="mt-2">
        <Button
          icon="pi pi-times"
          label="Đóng"
          @click="SidebarVisible = false"
        />
      </div>
    </template>
  </Dialog>
  <Dialog
    v-model:visible="ProcessingVisible"
    :style="'width: 80vw;'"
    :showCloseIcon="true"
    :modal="true"
    header="Quá trình duyệt báo cáo"
  >
    <Processing :id="processing_id" />
    <template #footer>
      <Button
        icon="pi pi-times"
        class=""
        label="Đóng"
        @click="ProcessingVisible = false"
      />
    </template>
  </Dialog>
  <framPrint :datas="listExp" />
</template>
<style lang="scss" scoped>
.format-center {
  display: flex;
  justify-content: center;
  align-items: center;
  vertical-align: middle;
  text-align: center;
}

.format-right {
  display: flex;
  justify-content: flex-end;
  align-items: center;
  vertical-align: middle;
  text-align: center;
}

.format-left {
  display: flex;
  justify-content: flex-start;
  align-items: center;
  vertical-align: middle;
  text-align: left;
}
.btn-c-hover:hover {
  color: #0025f8 !important;
  background-color: white !important;
}
.name-hover:hover {
  color: #2196f3;
  cursor: pointer;
}
.format-left2 {
  justify-content: flex-start;
  align-items: center;
  vertical-align: middle;
  text-align: start;
}
.description {
  color: #aaa;
}
</style>
