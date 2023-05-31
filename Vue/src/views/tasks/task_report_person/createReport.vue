<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { encr } from "../../../util/function.js";
import moment from "moment";
import DetailReportVue from "./component/DetailReport.vue";
import Processing from "./component/ProcessingReport.vue";
const toast = useToast();
const cryoptojs = inject("cryptojs");
const emitter = inject("emitter");
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
};
const v$ = useVuelidate(rules, personReport);

const submitted = ref(false);
const addNew = () => {
  length.value = false;
  personReport.value = {
    report_name: "",
    list_task_id: [],
    messages: null,
    self_point: null,
    reviewed_point: null,
    status: 0,
  };
  if (BHBQP.value == true) {
    // LoadLinkTaskOrigin();
    personReport.value.list_task_id = [...listTaskLink.value];
  }
  headerDialog.value = "Thêm mới báo cáo công việc";
  submitted.value = false;
  DialogVisible.value = true;
};
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

const headerAddLinkTask = ref();
const displayLinkTask = ref(false);
const selectUser = () => {
  headerAddLinkTask.value = "Chọn công việc";
  displayLinkTask.value = true;
};
const isEdit = ref(false);
const editData = (data) => {
  length.value = false;
  loadTaskInfo("", data.list_task_id);
  personReport.value = null;
  isEdit.value = true;
  headerDialog.value = "Sửa báo cáo công việc";
  personReport.value = data;
  personReport.value.report_id = data.report_id;
  let task_id = [];
  task_id = personReport.value.list_task_id1;
  personReport.value.list_task_id = null;
  personReport.value.list_task_id = [];
  task_id.forEach((x) => {
    let task = task_info_list.value.filter((z) => z.task_id == x);
    if (task != null) {
      personReport.value.list_task_id.push(task[0]);
    }
  });
  personReport.value.messages = data.messages.replace(/<br\s*\/?>/gi, "\n");
  DialogVisible.value = true;
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
              : "Thêm báo cáo công việc cá nhân thành công"
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
  if (event.rows != optionsLinkTask.value.PageSize) {
    optionsLinkTask.value.PageSize = event.rows;
  }
  optionsLinkTask.value.PageNo = event.page;
  LoadLinkTaskOrigin();
};
const first = ref();
const LoadLinkTaskOrigin = () => {
  optionsLinkTask.value.loading = true;
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
          cryoptojs
        ).toString(),
      },
      config
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
                  (x) => x.value == element.status
                )[0].text
              : "";
          element.status_bg_color =
            element.status != null
              ? listDropdownStatus.value.filter(
                  (x) => x.value == element.status
                )[0].bg_color
              : "";
          element.status_text_color =
            element.status != null
              ? listDropdownStatus.value.filter(
                  (x) => x.value == element.status
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
const selectedTasks = ref([]);
const saveAddLinkTask = () => {
  personReport.value.list_task_id = [];
  if (selectedTasks.value.length > 0) {
    selectedTasks.value.forEach((x) => {
      personReport.value.list_task_id.push(x);
    });
  }
  selectedTasks.value = [];
  displayLinkTask.value = false;
};
const widthScreen = ref(window.screen.width);
const removeTask = (item, us) => {
  var idx = us.findIndex((x) => x["task_id"] === item["task_id"]);
  if (idx != -1) {
    us.splice(idx, 1);
  }
};
const options = ref({
  PageNo: 0,
  PageSize: 20,
  totalRecords: 0,
  loading: true,
  SearchText: null,
  month: null,
  year: null,
  DateTime: null,
});
const datalists = ref();
const noData = ref(true);
const status = ref([
  { code: 0, label: "Khởi tạo", bgColor: "#2196F3", text: "#FFFFFF" },
  { code: 1, label: "Đang chờ duyệt", bgColor: "#FF6E31", text: "#FFFFFF" },
  { code: 2, label: "Đã duyệt", bgColor: "#6DD230", text: "#FFFFFF" },
  { code: 3, label: "Trả lại", bgColor: "#FF0000", text: "#FFFFFF" },
]);
const loadData = () => {
  options.value.loading = true;
  noData.value = true;
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "task_person_report_list",
            par: [
              { par: "user_id", va: user.user_id },
              { par: "pageno", va: options.value.PageNo },
              { par: "pagesize", va: options.value.PageSize },
              { par: "search", va: options.value.SearchText },
              { par: "month", va: options.value.month },
              { par: "year", va: options.value.year },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
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
const task_info_list = ref();
const loadTaskInfo = (user, list_task_id) => {
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "task_origin_list_review_person",
            par: [
              { par: "user_id", va: user },
              { par: "string", va: list_task_id },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
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
                  (x) => x.value == element.status
                )[0].text
              : "";
          element.status_bg_color =
            element.status != null
              ? listDropdownStatus.value.filter(
                  (x) => x.value == element.status
                )[0].bg_color
              : "";
          element.status_text_color =
            element.status != null
              ? listDropdownStatus.value.filter(
                  (x) => x.value == element.status
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
      task_info_list.value = data1;
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
const showInfo = (data) => {
  loadTaskInfo("", data.list_task_id);
  SidebarVisible.value = true;

  pDataa.value = [];
  let datasend = Object.assign({}, data);

  datasend.task_info = [];
  datasend.list_task_id1.forEach((x) => {
    let task = task_info_list.value.filter((z) => z.task_id == x);
    if (task != null) {
      datasend.task_info.push(task[0]);
    }
  });
  pDataa.value = datasend;
};

const deleteList = (type, data) => {
  //type 0 : nhiều ; 1: 1;
  let f = [];
  if (type == 0) {
    f = selectedReport.value.filter((x) => x.status != 0);
  }
  if (f.length > 0 || (type == 1 && data.is_default == true)) {
    swal.fire({
      title: "Thông báo!",
      text: "Không thể xóa báo cáo đã được trình duyệt!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  let listId = new Array(type == 0 ? selectedReport.value.length : 1);
  swal
    .fire({
      title: "Thông báo",
      text:
        type == 0
          ? "Bạn có muốn xoá danh sách này không!"
          : "Bạn có muốn xóa báo cáo này không ?",
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
        if (type == 0) {
          selectedReport.value.forEach((item) => {
            listId.push(item.report_id);
          });
        } else {
          listId.push(data.report_id);
        }
        axios
          .delete(baseURL + "/api/create_Person_Report/Delete_group", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: listId != null ? listId : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá báo cáo thành công!");
              checkDelList.value = false;
              if (
                (options.value.totalRecords - listId.length) % 2 == 0 &&
                options.value.PageNo > 0
              ) {
                options.value.PageNo = options.value.PageNo - 1;
              }
              loadData(true);
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
            swal.close();
            if (error.status === 401) {
              swal.fire({
                title: "Thông báo!",
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                icon: "error",
                confirmButtonText: "OK",
              });
            }
          });
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
  };
  styleObj.value = "";

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
const sendto = ref();
const sendtoDropdown = ref([
  { code: 0, label: "Nhóm duyệt" },
  { code: 1, label: "Cá nhân" },
]);
const NextStepDialog = ref(false);
const SendProcess = (type, data) => {
  NextStepDialog.value = true;
  sendto.value = null;
  modal.value = { group_id: null, user_id: null, list_report_id: [] };
  if (type == 1) {
    selectedReport.value.forEach((x) => {
      modal.value.list_report_id.push(x.report_id);
    });
  } else {
    modal.value.list_report_id.push(data.report_id);
  }
};
const listDropdownGroup = ref();
const listDropdownUser = ref();
const receiver = ref();
const group_role = ref([
  {
    code: 0,
    label: "Duyệt tuần tự",
    tip: "Duyệt theo thứ tự người duyệt",
  },
  {
    code: 1,
    label: "Duyệt một trong nhiều",
    tip: "Nhiều người duyệt. Chỉ cần một người xác nhận.",
  },
  {
    code: 2,
    label: "Duyệt ngẫu nhiên",
    tip: "Tất cả người duyệt cần xác nhận.",
  },
]);
const loadGroupOrUser = (type) => {
  modal.value.user_id = null;
  modal.value.group_id = null;
  if (type == 1) {
    loadDefaultUser();
  }
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify(
            sendto.value == 1
              ? {
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
                }
              : {
                  proc: "task_browse_group_list_select",
                  par: [{ par: "user_id", va: store.getters.user.user_id }],
                }
          ),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (sendto.value == 1) {
        listDropdownUser.value = data.map((x) => ({
          name: x.full_name,
          code: x.user_id,
          avatar: x.avatar,
          position_name: x.position_name,
          department_name: x.department_name,
        }));
        modal.value.user_id = receiver.value;
      } else {
        data.forEach((x) => {
          let adsasd = x.user_info;
          x.user_info = [];
          if (adsasd != null) {
            x.user_info = JSON.parse(adsasd);
          }
          var f = group_role.value.filter((z) => z.code == x.group_role);
          x.group_role_label = f[0].label;
        });
        listDropdownGroup.value = data;
        var def = listDropdownGroup.value.filter((x) => x.is_default == true);
        modal.value.group_id = def[0];
        receiver.value = def[0];
      }
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

const loadDefaultUser = () => {
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "Task_Person_Config_get",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "is_Bbhqp", va: 1 },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];

      if (data.length > 0) {
        data.forEach((element) => {
          element.receiver_info = JSON.parse(element.receiver_info);
        });
        receiver.value = {
          name: data[0].receiver_info.full_name,
          code: data[0].receiver_info.user_id,
          avatar: data[0].receiver_info.avatar,
          position_name: data[0].receiver_info.position_name,
          department_name: data[0].receiver_info.department_name,
        };
      } else {
        swal.fire({
          title: "Thông báo",
          html: "Chưa có người duyệt!<br/> Vui lòng thiết lập phòng ban và thử lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const modal = ref({
  group_id: null,
  user_id: null,
  list_report_id: [],
});
const changeUserReview = (e) => {
  receiver.value = e;
};
const sendReportToUser = () => {
  swal
    .fire({
      title: "Thông báo",
      html:
        sendto.value == 0
          ? "Bạn có chắc trình duyệt tới nhóm <b>" +
            modal.value.group_id.group_name +
            "</b> không ?"
          : "Bạn có chắc trình duyệt tới <b> " +
            modal.value.user_id.name +
            "</b> không?",
      icon: "warning",
      showCancelButton: true,
      confirmButtonColor: "#3085d6",
      cancelButtonColor: "#d33",
      confirmButtonText: "Có",
      cancelButtonText: "Không",
    })
    .then((result) => {
      if (result.isConfirmed) {
        sending();
      }
    });
};
const sending = () => {
  if (
    modal.value.list_report_id == null ||
    modal.value.list_report_id.length < 1 ||
    modal.value.list_report_id == [] ||
    (modal.value.user_id == null && modal.value.group == null)
  ) {
    swal.close();

    isEdit.value = false;
    loadData();
    NextStepDialog.value = false;
    swal.close();
    modal.value.list_report_id = [];
    selectedReport.value = [];
    return;
  } else {
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
    let formData = new FormData();
    let data = Object.assign({}, modal.value);
    if (data.group_id != null) {
      data.user_id = null;
      data.group_id = data.group_id.group_id;
    } else if (data.user_id) {
      data.user_id = data.user_id.code;
    }
    let temp = data.list_report_id;
    data.list_report_id = "";
    temp.forEach((x) => {
      data.list_report_id +=
        (data.list_report_id != null && data.list_report_id != "" ? "," : "") +
        x;
    });
    formData.append("group", JSON.stringify(data.group_id));
    formData.append("user", JSON.stringify(data.user_id));
    formData.append("report", JSON.stringify(data.list_report_id));
    axios({
      method: "post",
      url: baseURL + `/api/send_Process_Report/${"addRound"}`,
      data: formData,
      headers: {
        Authorization: `Bearer ${store.getters.token}`,
      },
    })
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Báo cáo đã được chuyển đi!");
          isEdit.value = false;
          loadData();
          NextStepDialog.value = false;
          swal.close();
          modal.value.list_report_id = [];
        } else {
          swal.close();
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
  }
};

const ProcessingVisible = ref(false);
const processing_id = ref();
const ShowProcess = (data) => {
  processing_id.value = data.report_id;
  ProcessingVisible.value = true;
};
const BHBQP = ref(true);
const ChangeMonth = ref(false);
const MonthChange = (event) => {
  optionsLinkTask.value.month = event.getMonth() + 1;
  optionsLinkTask.value.year = event.getFullYear();
  ChangeMonth.value = true;
  LoadLinkTaskOrigin();
};
const MonthChange2 = (event) => {
  options.value.month = event.getMonth() + 1;
  options.value.year = event.getFullYear();
};
const SendBHBQP = (type, data) => {
  sendto.value = 1;
  loadDefaultUser();
  modal.value = {
    group_id: null,
    user_id: null,
    list_report_id: [],
  };
  let userzz = Object.assign({}, receiver.value);
  modal.value.user_id = userzz;

  if (data == null) {
    selectedReport.value.forEach((x) => {
      if (x.status != 1 && x.status != 2)
        modal.value.list_report_id.push(x.report_id);
    });
  } else {
    modal.value.list_report_id.push(data.report_id);
  }
  selectedReport.value = [];

  sending();
};
const style = ref({
  "background-color": "#2196F3 !important",
  color: "#fff !important",
  " border": "1px solid #5ca7e3 !important",
});
const styleObj = ref();
const op = ref();
const toggle = (event) => {
  op.value.toggle(event);
};
const reNewFilter = () => {
  options.value.year = null;
  options.value.month = null;
  options.value.DateTime = null;
  styleObj.value = null;
  loadData();
};
onMounted(() => {
  loadData();
  LoadLinkTaskOrigin();
  loadDefaultUser();
  loadTaskInfo(user.user_id, "");
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
              v-tooltip="'Bộ lọc'"
              :style="[styleObj]"
            />
            <OverlayPanel
              ref="op"
              appendTo="body"
              class="p-0 m-0"
              :showCloseIcon="false"
              id="overlay_panel"
            >
              <div class="grid formgrid m-0">
                <Calendar
                  inputId="basic"
                  v-model="options.DateTime"
                  autocomplete="off"
                  view="month"
                  dateFormat="MM/yy"
                  :showIcon="true"
                  :manualInput="false"
                  class="w-full"
                  @date-select="MonthChange2($event)"
                  placeholder="Chọn tháng năm"
                />

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
              v-if="checkDelList == true && BHBQP != true"
              label="Chuyển xử lý"
              icon="pi pi-send"
              class="mr-2 p-button-raised"
              @click="SendProcess(1, null)"
            />
            <Button
              v-if="checkDelList && BHBQP == true"
              label="Chuyển xử lý"
              icon="pi pi-send"
              class="mr-2 p-button-raised"
              @click="SendBHBQP(1, null)"
            />
            <Button
              v-if="checkDelList"
              @click="deleteList(0, null)"
              label="Xóa"
              icon="pi pi-trash"
              class="mr-2 p-button-danger"
            />
            <Button
              @click="addNew()"
              label="Thêm mới"
              icon="pi pi-plus"
              class="mr-2"
            />
            <Button
              @click="refresh()"
              class="mr-2 p-button-outlined p-button-secondary"
              icon="pi pi-refresh"
              v-tooltip.bottom="'Tải lại'"
            />
          </template>
        </Toolbar>
      </template>

      <Column
        selectionMode="multiple"
        headerStyle="text-align:center;max-width:60px;height:50px"
        bodyStyle="text-align:center;max-width:60px "
        class="align-items-center justify-content-center text-center"
      ></Column>
      <Column
        field="stt"
        header="STT"
        headerStyle="text-align:center;max-width:75px;height:50px"
        bodyStyle="text-align:center;max-width:75px "
        class="align-items-center justify-content-center text-center"
      ></Column>
      <Column
        field="report_name"
        header="Tên báo cáo"
        headerStyle="text-align:center;height:50px"
        bodyStyle=""
        headerClass="align-items-center justify-content-center"
      >
        <template #body="data">
          <div class="name-hover" @click="showInfo(data.data)">
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
          <Button
            @click="showInfo(data.data)"
            class="p-button-rounded p-button-secondary p-button-outlined mx-1"
            type="button"
            icon="pi pi-info-circle"
            v-tooltip.bottom="'Thông tin báo cáo'"
          ></Button>
          <div v-if="data.data.status != 1 && data.data.status != 2">
            <Button
              v-if="BHBQP == true"
              @click="SendBHBQP(0, data.data)"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-send"
              v-tooltip.bottom="'chuyển xử lý'"
            ></Button>
            <Button
              v-else
              @click="SendProcess(0, data.data)"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-send"
              v-tooltip.bottom="'chuyển xử lý'"
            ></Button>

            <Button
              @click="editData(data.data)"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-pencil"
              v-tooltip.bottom="'Sửa'"
            ></Button>
            <Button
              @click="deleteList(1, data.data)"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-trash"
              v-tooltip.bottom="'Xóa'"
            ></Button>
          </div>
          <div v-if="BHBQP == true && data.data.status == 1">
            <Button
              @click="editData(data.data)"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-pencil"
              v-tooltip.bottom="'Sửa'"
            ></Button>
          </div>
          <div
            v-if="
              data.data.status == 1 ||
              data.data.status == 2 ||
              data.data.status == 3
            "
          >
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
        <div class="block w-full h-full format-center" v-if="noData == true">
          <img src="../../../assets/background/nodata.png" height="144" />
          <h3 class="m-1">Không có dữ liệu</h3>
        </div>
      </template>
    </DataTable>
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
          @input="checkLenght()"
        />
      </div>
      <div style="display: flex" class="col-12 py-0" v-if="length == true">
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
          <Button
            v-if="BHBQP != true"
            icon="pi pi-check-square text-xl"
            class="p-button-text ml-2 p-button-info ip36"
            v-tooltip.top="'Chọn công việc'"
            @click="selectUser()"
          ></Button>
        </div>

        <div class="col-6 format-left">
          <Calendar
            inputId="basic"
            v-model="optionsLinkTask.DateTime"
            autocomplete="off"
            view="month"
            dateFormat="MM/yy"
            :showIcon="true"
            :manualInput="false"
            class="w-full"
            @date-select="MonthChange($event)"
          />
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
                "Bạn chưa chọn công việc!"
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
                  @error="
                    $event.target.src =
                      basedomainURL + '/Portals/Image/nouser1.png'
                  "
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
                            "DD/MM/YYYY"
                          )
                        : null
                    }}
                    -
                    {{
                      data.item.end_date
                        ? moment(new Date(data.item.end_date)).format(
                            "DD/MM/YYYY"
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
                            @error="
                              $event.target.src =
                                basedomainURL + '/Portals/Image/nouser1.png'
                            "
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
                        @error="
                          $event.target.src =
                            basedomainURL + '/Portals/Image/nouser1.png'
                        "
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
                    <div v-if="data.item.progress != 0" style="width: 100%">
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
                          "DD/MM/YYYY"
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
                  <a class="btn-c-hover">
                    <Button
                      icon="pi pi-trash"
                      class="p-button-text"
                      @click="removeTask(data.item, personReport.list_task_id)"
                      v-tooltip.bottom="'xóa'"
                    ></Button>
                  </a>
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
                "Nội dung báo cáo không được để trống!"
              )
            }}</span>
          </small>
        </div>
      </div>

      <div class="col-12 flex">
        <div class="col-2">
          Điểm tự đánh giá (0-100)<span class="redsao">(*)</span>
        </div>
        <InputNumber
          v-model="personReport.self_point"
          spellcheck="false"
          class="col-10 p-0"
          mode="decimal"
          showButtons
          :min="0"
          :max="100"
          :useGrouping="false"
          autocomplete="off"
          :class="{
            'p-invalid': v$.self_point.$invalid && submitted,
          }"
        />
      </div>
      <div class="col-12 flex">
        <div class="col-2"></div>
        <div
          class="col-10 p-0 format-left text-red_600"
          v-if="
            (v$.self_point.$invalid && submitted) ||
            v$.self_point.$pending.$response
          "
        >
          <small class="col-10 p-0 p-error">
            <span class="col-12">{{
              v$.self_point.required.$message.replace(
                "Value is required",
                "Điểm tự đánh giá không được để trống!"
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
      <Button
        label="Lưu và gửi"
        icon="pi pi-check"
        @click="saveData(!v$.$invalid, 1)"
      />
    </template>
  </Dialog>
  <Dialog
    :header="headerAddLinkTask"
    v-model:visible="displayLinkTask"
    style="overflow-y: hidden !important"
    :style="{ width: '80vw', 'max-height': '90vh' }"
    :closable="true"
  >
    <form>
      <div v-if="store.getters.islogin" style="height: 100%">
        <DataTable
          v-model:first="first"
          :rowHover="true"
          :value="listTaskLink"
          :scrollable="true"
          :totalRecords="optionsLinkTask.totalRecords"
          :row-hover="true"
          dataKey="task_id"
          :paginator="true"
          :rows="optionsLinkTask.PageSize"
          paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
          :rowsPerPageOptions="[20, 30, 50, 100, 200]"
          v-model:selection="selectedTasks"
          @page="onPage($event)"
          :lazy="true"
          :showGridlines="true"
          scrollHeight="calc(100vh - 27rem)"
        >
          <template #header>
            <div class="flex justify-content-center align-items-center">
              <Toolbar class="w-full custoolbar">
                <template #start>
                  <span class="p-input-icon-left">
                    <i class="pi pi-search" />
                    <InputText
                      style="min-width: 300px"
                      type="text"
                      spellcheck="false"
                      v-model="optionsLinkTask.search"
                      placeholder="Tìm kiếm"
                      @keyup.enter="LoadLinkTaskOrigin(datalists)"
                    />
                  </span>
                </template>
              </Toolbar>
            </div>
          </template>
          <Column
            selectionMode="multiple"
            headerStyle="text-align:center;max-width:4rem;height:3.125rem"
            bodyStyle="text-align:center;max-width:4rem; "
            class="align-items-center justify-content-center text-center"
          >
          </Column>

          <Column
            headerStyle="text-align:center;max-width:50px;min-height:3.125rem"
            bodyStyle="text-align:center;max-width:50px; "
            class="align-items-center justify-content-center text-center"
          >
            <template #body="value">
              <Avatar
                @error="
                  $event.target.src =
                    basedomainURL + '/Portals/Image/nouser1.png'
                "
                v-tooltip.bottom="{
                  value:
                    'Người tạo: <br/>' +
                    value.data.full_name +
                    '<br/>' +
                    (value.data.position_name || '') +
                    '<br/>' +
                    (value.data.tenToChuc || ''),
                  escape: true,
                }"
                v-bind:label="
                  value.data.avatar
                    ? ''
                    : (value.data.last_name ?? '').substring(0, 1)
                "
                v-bind:image="basedomainURL + value.data.avatar"
                style="
                  background-color: #2196f3;
                  color: #ffffff;
                  width: 2.5rem;
                  height: 2.5rem;
                  font-size: 15px !important;
                "
                :style="{
                  background: bgColor[0] + '!important',
                }"
                class="cursor-pointer"
                size="xlarge"
                shape="circle"
              />
            </template>
          </Column>
          <Column
            header="Tên công việc"
            headerStyle="min-height:3.125rem"
            bodyStyle=" "
          >
            <template #body="data">
              <div style="display: flex; flex-direction: column; padding: 5px">
                <div style="min-height: 25px">
                  <span style="font-weight: bold; font-size: 14px">{{
                    data.data.task_name
                  }}</span>
                </div>
                <div style="font-size: 12px">
                  <span
                    v-if="data.data.start_date || data.data.end_date"
                    style="color: #98a9bc"
                    >{{
                      data.data.start_date
                        ? moment(new Date(data.data.start_date)).format(
                            "DD/MM/YYYY"
                          )
                        : null
                    }}
                    -
                    {{
                      data.data.end_date
                        ? moment(new Date(data.data.end_date)).format(
                            "DD/MM/YYYY"
                          )
                        : null
                    }}</span
                  >
                </div>
                <div
                  v-if="data.data.project_name"
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
                    >{{ data.data.project_name }}</span
                  >
                </div>
              </div>
            </template>
          </Column>
          <Column
            field=""
            header="Thành viên"
            class="align-items-center justify-content-center text-center"
            headerStyle="text-align:center;max-width:150px;min-height:3.125rem"
            bodyStyle="text-align:center;max-width:150px;"
          >
            <template #body="data">
              <AvatarGroup>
                <div
                  v-for="(value, index) in data.data.ThanhvienShows"
                  :key="index"
                >
                  <div>
                    <Avatar
                      @error="
                        $event.target.src =
                          basedomainURL + '/Portals/Image/nouser1.png'
                      "
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
                        value.avatar ? '' : (value.ten ?? '').substring(0, 1)
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
                  @error="
                    $event.target.src =
                      basedomainURL + '/Portals/Image/nouser1.png'
                  "
                  v-if="
                    data.data.Thanhviens.length -
                      data.data.ThanhvienShows.length >
                    0
                  "
                  :label="
                    '+' +
                    (data.data.Thanhviens.length -
                      data.data.ThanhvienShows.length) +
                    ''
                  "
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
            </template>
          </Column>
          <Column
            class="align-items-center justify-content-center text-center"
            header="Tiến độ"
            headerStyle="text-align:center;max-width:100px;min-height:3.125rem"
            bodyStyle="text-align:center;max-width:100px;"
          >
            <template #body="data">
              <span v-if="data.data.progress == 0"
                >{{ data.data.progress }} %</span
              >
              <div v-if="data.data.progress != 0" style="width: 100%">
                <ProgressBar :value="data.data.progress" />
              </div>
            </template>
          </Column>
          <Column
            class="align-items-center justify-content-center text-center"
            header="Thời gian xử lý"
            headerStyle="text-align:center;max-width:150px;min-height:3.125rem"
            bodyStyle="text-align:center;max-width:150px;"
          >
            <template #body="data">
              <div v-if="data.data.title_time">
                <span
                  style="
                    font-size: 10px;
                    font-weight: bold;
                    padding: 5px;
                    border-radius: 5px;
                  "
                  :style="{
                    'background-color': data.data.time_bg,
                    color: data.data.status_text_color,
                  }"
                  >{{ data.data.title_time }}</span
                >
              </div>
            </template>
          </Column>
          <Column
            class="align-items-center justify-content-center text-center"
            header="Ngày cập nhật"
            headerStyle="text-align:center;max-width:150px;min-height:3.125rem"
            bodyStyle="text-align:center;max-width:150px;"
          >
            <template #body="data">
              <div
                style="
                  background-color: #fff8ee;
                  padding: 10px 20px;
                  border-radius: 5px;
                "
              >
                <span
                  style="color: #ffab2b; font-size: 13px; font-weight: bold"
                  >{{
                    moment(new Date(data.data.update_date)).format("DD/MM/YYYY")
                  }}</span
                >
              </div>
            </template>
          </Column>
          <Column
            class="align-items-center justify-content-center text-center"
            header="Trạng thái"
            headerStyle="text-align:center;max-width:120px;min-height:3.125rem"
            bodyStyle="text-align:center;max-width:120px;"
          >
            <template #body="data">
              <Chip
                :style="{
                  background: data.data.status_bg_color,
                  color: data.data.status_text_color,
                }"
                v-bind:label="data.data.status_name"
              />
            </template>
          </Column>
          <template #empty>
            <div
              class="align-items-center justify-content-center p-4 text-center m-auto min-h-full w-full"
              style="display: flex; flex-direction: column"
              v-if="!noData"
            >
              <img src="../../../assets/background/nodata.png" height="144" />
              <h3 class="m-1">Không có dữ liệu</h3>
            </div>
          </template>
        </DataTable>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="displayLinkTask = false"
        class="p-button-text"
      />
      <Button label="Lưu" icon="pi pi-check" @click="saveAddLinkTask()" />
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
    v-model:visible="NextStepDialog"
    :style="'width: 40vw;'"
    :showCloseIcon="true"
    header="Chuyển xử lý"
    maximizable="true"
  >
    <form>
      <div class="col-12 flex">
        <div class="col-2 format-left">Chuyển tới:</div>
        <Dropdown
          v-model="sendto"
          :options="sendtoDropdown"
          optionLabel="label"
          optionValue="code"
          class="col-10 p-0"
          placeholder="Nơi nhận"
          @change="loadGroupOrUser($event.value)"
        />
      </div>
      <div class="col-12 flex" v-if="sendto != null">
        <div class="col-2 format-left">
          <span v-if="sendto == 1">Người duyệt</span>
          <span v-if="sendto == 0">Nhóm duyệt</span>
        </div>
        <Dropdown
          v-if="sendto == 1"
          :modelValue="receiver"
          :filter="true"
          v-model="modal.user_id"
          :options="listDropdownUser"
          optionLabel="name"
          class="col-10 ip36 p-0"
          placeholder="Người đánh giá"
          display="chip"
          style="height: 3.125rem"
          @change="changeUserReview($event.value)"
        >
          <template #value="slotProps">
            <div class="flex" v-if="slotProps.value">
              <div class="flex" style="margin-left: 10px">
                <Avatar
                  @error="
                    $event.target.src =
                      basedomainURL + '/Portals/Image/nouser1.png'
                  "
                  v-bind:label="
                    slotProps.value.avatar
                      ? ''
                      : (slotProps.value.name ?? '').substring(0, 1)
                  "
                  v-bind:image="basedomainURL + slotProps.value.avatar"
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
                <div class="pt-1" style="padding-left: 10px">
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
                @error="
                  $event.target.src =
                    basedomainURL + '/Portals/Image/nouser1.png'
                "
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
                  background: bgColor[slotProps.index % 7] + '!important',
                }"
                class="cursor-pointer"
                size="xlarge"
                shape="circle"
              />
              <div>
                <div
                  class="pt-1 text-black-alpha-90"
                  style="padding-left: 10px"
                >
                  {{ slotProps.option.name }}
                </div>
                <div class="pt-1 description" style="padding-left: 10px">
                  {{ slotProps.option.position_name }}
                </div>
                <div class="pt-1 description" style="padding-left: 10px">
                  {{ slotProps.option.department_name }}
                </div>
              </div>
            </div>
          </template>
        </Dropdown>
        <Dropdown
          v-if="sendto == 0"
          :filter="true"
          v-model="modal.group_id"
          :options="listDropdownGroup"
          panelClass="d-design-dropdown"
          class="col-10 p-design-dropdown"
          placeholder="Nhóm duyệt"
          @change="changeUserReview($event.value)"
        >
          <template #value="slotProps">
            <div class="col-12" v-if="slotProps.value">
              <div class="col-12">{{ slotProps.value.group_name }}</div>
              <div class="col-12 description py-0 elipsis">
                {{ slotProps.value.group_role_label }}
              </div>
              <div class="col-12 ml-3 description">
                <AvatarGroup>
                  <div
                    v-for="(value, index) in slotProps.value.user_info"
                    :key="index"
                  >
                    <div>
                      <Avatar
                        @error="
                          $event.target.src =
                            basedomainURL + '/Portals/Image/nouser1.png'
                        "
                        v-if="index <= 2"
                        v-tooltip.bottom="{
                          value:
                            value.full_name +
                            '<br/>' +
                            (value.position_name || '') +
                            '<br/>' +
                            (value.department_name || value.organization_name),
                          escape: true,
                        }"
                        v-bind:label="
                          value.avatar
                            ? ''
                            : (value.full_name ?? '').substring(0, 1)
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
                          background: bgColor[index % 10] + '!important',
                        }"
                        class="cursor-pointer"
                        size="xlarge"
                        shape="circle"
                      />
                    </div>
                  </div>
                  <Avatar
                    @error="
                      $event.target.src =
                        basedomainURL + '/Portals/Image/nouser1.png'
                    "
                    v-if="slotProps.value.user_info.length - 3 > 0"
                    :label="'+' + (slotProps.value.user_info.length - 3) + ''"
                    v-tooltip.bottom="{
                      value:
                        'và ' +
                        (slotProps.value.user_info.length - 3) +
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
            </div>

            <span v-else>
              {{ slotProps.placeholder }}
            </span>
          </template>
          <template #option="slotProps">
            <div class="col-12 p-dropdown-car-option">
              <div class="col-12">{{ slotProps.option.group_name }}</div>
              <div class="col-12 description py-0 elipsis">
                {{ slotProps.option.group_role_label }}
              </div>
              <div class="col-12 ml-3 description">
                <AvatarGroup>
                  <div
                    v-for="(value, index) in slotProps.option.user_info"
                    :key="index"
                  >
                    <div>
                      <Avatar
                        @error="
                          $event.target.src =
                            basedomainURL + '/Portals/Image/nouser1.png'
                        "
                        v-if="index <= 2"
                        v-tooltip.bottom="{
                          value:
                            value.full_name +
                            '<br/>' +
                            (value.position_name || '') +
                            '<br/>' +
                            (value.department_name || value.organization_name),
                          escape: true,
                        }"
                        v-bind:label="
                          value.avatar
                            ? ''
                            : (value.full_name ?? '').substring(0, 1)
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
                    @error="
                      $event.target.src =
                        basedomainURL + '/Portals/Image/nouser1.png'
                    "
                    v-if="slotProps.option.user_info.length - 3 > 0"
                    :label="'+' + (slotProps.option.user_info.length - 3) + ''"
                    v-tooltip.bottom="{
                      value:
                        'và ' +
                        (slotProps.option.user_info.length - 3) +
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
            </div>
          </template>
        </Dropdown>
      </div>
    </form>
    <template #footer>
      <Button
        icon="pi pi-times"
        class="p-button-text p-button-raised"
        label="Đóng"
        @click="NextStepDialog = false"
      />
      <Button
        v-if="sendto != null"
        icon="pi pi-check"
        label="Chuyển đi"
        @click="sendReportToUser()"
      />
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
