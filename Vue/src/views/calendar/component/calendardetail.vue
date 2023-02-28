<script setup>
import { ref, inject, onMounted } from "vue";
import { encr } from "../../../util/function";
import { socketMethod } from "../../../util/methodSocket";
import { useRoute } from "vue-router";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { VuemojiPicker } from "vuemoji-picker";
import { de } from "date-fns/locale";
import moment from "moment";
import vi from "date-fns/locale/vi";
import dialogmodelweek from "./dialogmodelweek.vue";
import dialogsend from "./dialogsend.vue";
import dialogapprove from "./dialogapprove.vue";
import dialogenact from "./dialogenact.vue";
import dialogchart from "./dialogchart.vue";

const cryoptojs = inject("cryptojs");
const socket = inject("socket");
const route = useRoute();
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
const toast = useToast();

//Get arguments
const props = defineProps({
  group: Number, //0 Lịch họp, 1: lịch công tác
});

//Declare
function urlify(string) {
  var urlRegex = string.match(
    /((((ftp|https?):\/\/)|(w{3}\.))[\-\w@:%_\+.~#?,&\/\/=]+)/g
  );
  if (urlRegex) {
    urlRegex.forEach(function (url) {
      string = string.replace(
        url,
        '<a target="_blank" href="' + url + '">' + url + "</a>"
      );
    });
  }
  return string.replace("(", "<br/>(");
}
const trustAsHtml = (html) => {
  if (!html) {
    return "";
  }
  return urlify(html).replace(/\n/g, "<br/>");
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
const groups = ref([
  // { view: 0, icon: "", title: "Tất cả các lịch"},
  { view: 1, icon: "", title: "Xem theo tuần" },
  { view: 2, icon: "", title: "Xem theo tháng" },
]);
const typestatus = ref([
  { value: 0, title: "dự thảo", bg_color: "#bbbbbb", text_color: "#fff" },
  { value: 1, title: "chờ duyệt", bg_color: "#2196f3", text_color: "#fff" },
  { value: 2, title: "ban hành", bg_color: "#04D215", text_color: "#fff" },
  { value: 3, title: "trả lại", bg_color: "#ff8b4e", text_color: "#fff" },
  { value: 4, title: "hủy", bg_color: "red", text_color: "#fff" },
]);
const typeprocedures = ref([
  { value: 0, title: "Duyệt tuần tự" },
  { value: 1, title: "Duyệt một trong nhiều" },
  //{ value: 2, title: "Duyệt ngẫu nhiên" },
]);
const options = ref({
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
  search: "",
  total: 0,
  sort: "created_date desc",
  orderBy: "desc",
});

//Model
const model = ref({
  status: 0,
  is_type: 0,
  is_iterations: 0,
  numeric_iterations: 0,
  numeric_attendees: 0,
  chutris: [],
  thamgias: [],
  departments: [],
  files: [],
});

//Function declare
const show_send = ref(false);
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
const currentweek = ref({});
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

//Function add
const isAdd = ref(false);
const submitted = ref(false);
const headerDialog = ref();
const displayDialog = ref(false);
const boardrooms = ref([]);
const departments = ref([]);
const cars = ref([]);
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
    end_date: new Date(
      start_date.getFullYear(),
      start_date.getMonth(),
      start_date.getDate(),
      7,
      0,
      0
    ),
    status: 0,
    is_type: 0,
    is_iterations: 0,
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
                  title: "Error!",
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
                controller: "boardroom.vue",
                logcontent: error.message,
                loai: 2,
              });
              if (error.status === 401) {
                swal.fire({
                  title: "Error!",
                  text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                  icon: "error",
                  confirmButtonText: "OK",
                });
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
};
const selectFile = (event) => {
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
      var data = response.data.data;
      if (data != null) {
        var tbs = JSON.parse(data);
        if (tbs[0] != null && tbs[0].length > 0) {
          model.value = tbs[0][0];
        } else {
          model.value = {};
        }
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
        } else {
          model.value.files = [];
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
        controller: "boardroom.vue",
        logcontent: error.message,
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
                title: "Error!",
                text: response.data.ms,
                icon: "error",
                confirmButtonText: "OK",
              });
              return;
            }
            swal.close();
            toast.success("Xoá " + options.value.title + " thành công!");
            if (options.value.loading) options.value.loading = false;
            router.back();
          })
          .catch((error) => {
            swal.close();
            if (options.value.loading) options.value.loading = false;
            addLog({
              title: "Lỗi Console delItem",
              controller: "boardroom.vue",
              logcontent: error.message,
              loai: 2,
            });
            if (error.status === 401) {
              swal.fire({
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
};
const deleteFile = (item) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá tệp đính kèm này không!",
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
        var ids = [item["file_id"]];
        axios
          .delete(baseURL + "/api/calendar_week/delete_file", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: ids,
          })
          .then((response) => {
            if (response.data.err === "1") {
              swal.fire({
                title: "Error!",
                text: response.data.ms,
                icon: "error",
                confirmButtonText: "OK",
              });
            }
            if (ids.length > 0) {
              ids.forEach((element, i) => {
                var idx = model.value.files.findIndex(
                  (x) => x.file_id == element
                );
                if (idx != -1) {
                  model.value.files.splice(idx, 1);
                }
              });
            }
            swal.close();
            toast.success("Xoá tệp đính kèm thành công!");
          })
          .catch((error) => {
            swal.close();
            if (options.value.loading) options.value.loading = false;
            addLog({
              title: "Lỗi Console delItem",
              controller: "boardroom.vue",
              logcontent: error.message,
              loai: 2,
            });
            if (error.status === 401) {
              swal.fire({
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                confirmButtonText: "OK",
              });
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
        var ids = [item["calendar_id"]];
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
              controller: "boardroom.vue",
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
  files.value = [];
  submitted.value = false;
  modelsend.value = {
    is_type_send: type,
    content: "",
    read_date: moment(new Date()).format("YYYY-MM-DDTHH:mm:ssZZ"),
  };
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
      var data = response.data.data;
      if (data != null) {
        var tbs = JSON.parse(data);
        if (tbs[0] != null && tbs[0].length > 0) {
          chartprocedures.value = tbs[0];
        } else {
          chartprocedures.value = [];
        }
        if (tbs[1] != null && tbs[1].length > 0) {
          chartsigns.value = tbs[1];
        } else {
          chartsigns.value = [];
        }
        if (tbs[2] != null && tbs[2].length > 0) {
          chartsignusers.value = tbs[2];
        } else {
          chartsignusers.value = [];
        }
        if (tbs[3] != null && tbs[3].length > 0) {
          chartlogs.value = tbs[3];
        } else {
          chartlogs.value = [];
        }
        if (chartsignusers.value != null && chartsignusers.value.length > 0) {
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
        if (chartsigns.value != null && chartsigns.value.length > 0) {
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
        if (chartprocedures.value != null && chartprocedures.value.length > 0) {
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
        if (chartlogs.value != null && chartlogs.value.length > 0) {
          chartlogs.value.forEach((l) => {
            l["created_date"] = moment(new Date(l["created_date"])).format(
              "HH:mm DD/MM/YYYY"
            );
          });
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
        controller: "boardroom.vue",
        logcontent: error.message,
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
const closeDialogLog = () => {
  headerDialogLog.value = false;
  displayDialogLog.value = false;
};

//Function detail
const goBack = () => {
  router.back();
};
const goFile = (file) => {
  window.open(basedomainURL + file.file_path, "_blank");
};

const scrollMember = () => {
  let elmnt = document.getElementById("div-member");
  if (elmnt != null) {
    elmnt.scrollIntoView({ behavior: "smooth" });
  }
};

const scrollComment = () => {
  let elmnt = document.getElementById("div-comment");
  if (elmnt != null) {
    elmnt.scrollIntoView({ behavior: "smooth" });
  }
};

const scrollFile = () => {
  let elmnt = document.getElementById("div-file");
  if (elmnt != null) {
    elmnt.scrollIntoView({ behavior: "smooth" });
  }
};

//Send message
const formatByte = (bytes, precision) => {
  if (isNaN(parseFloat(bytes)) || !isFinite(bytes)) return "-";
  if (typeof precision === "undefined") precision = 1;
  let units = ["bytes", "KB", "MB", "GB", "TB", "PB"];
  if (typeof bytes === "string" || bytes instanceof String) {
    bytes = parseFloat(bytes);
  }
  let number = Math.floor(Math.log(bytes) / Math.log(1024));
  return (
    (bytes / Math.pow(1024, Math.floor(number))).toFixed(precision) +
    " " +
    units[number]
  );
};
const modelcomment = ref({ contents: "" });
const panelEmoij = ref();
const showEmoji = (event) => {
  panelEmoij.value.toggle(event);
};
const handleEmojiClick = (event) => {
  if (modelcomment.value.contents) {
    modelcomment.value.contents = modelcomment.value.contents + event.unicode;
  } else {
    modelcomment.value.contents = event.unicode;
  }
};
const fileAttach = ref([]);
const chooseFile = (id) => {
  document.getElementById(id).click();
};
const putFileUpload = (event, type) => {
  var ms = false;
  var listFiles = [];
  if (!type) {
    listFiles = event.target.files;
  } else {
    listFiles = event;
  }
  listFiles.forEach((fi, idx) => {
    if (fi.size > 100 * 1024 * 1024) {
      ms = true;
    } else {
      fi.file_name = fi.name;
      fi.file_size = fi.size;
      fi.file_type = fi.name.substring(fi.name.lastIndexOf(".") + 1);
      fileAttach.value.push(fi);
    }
  });
  if (ms) {
    swal.fire({
      icon: "warning",
      type: "warning",
      title: "Thông báo",
      text: "Bạn chỉ được upload file có dung lượng tối đa 100MB!",
    });
  }
};
const removeFileMS = (files, file, index) => {
  if (file["file_id"] != null) {
    swal
      .fire({
        title: "Thông báo",
        text: "Bạn có muốn xoá tài liệu kèm này không!",
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
          var ids = [file["file_id"]];
          axios
            .delete(baseURL + "/api/calendar_week/delete_file", {
              headers: { Authorization: `Bearer ${store.getters.token}` },
              data: ids,
            })
            .then((response) => {
              if (response.data.err === "1") {
                swal.fire({
                  title: "Error!",
                  text: response.data.ms,
                  icon: "error",
                  confirmButtonText: "OK",
                });
              }
              files.splice(index, 1);
              swal.close();
              toast.success("Xoá tài liệu kèm thành công!");
            })
            .catch((error) => {
              swal.close();
              if (options.value.loading) options.value.loading = false;
              addLog({
                title: "Lỗi Console delItem",
                controller: "boardroom.vue",
                logcontent: error.message,
                loai: 2,
              });
              if (error.status === 401) {
                swal.fire({
                  title: "Error!",
                  text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                  icon: "error",
                  confirmButtonText: "OK",
                });
              }
            });
        }
      });
  } else {
    files.splice(index, 1);
  }
};

//Function Message
const functionMS = ref();
const mesFocus = ref();
const toogleFunctionMS = (event, message) => {
  functionMS.value.toggle(event);
  mesFocus.value = message;
};
const isEdit = ref(false);
const editMS = (message) => {
  detail.value.comments
    .filter((x) => x.isEdit)
    .forEach(function (r) {
      r.isEdit = false;
    });
  message.isEdit = true;
  isEdit.value = true;
  fileAttach.value = [];
  message["contents_edit"] = message["contents"];
  var messagedit = { ...message };
  if (messagedit.parentcomment != null) {
    delete messagedit.parentcomment;
  }
  modelcomment.value = JSON.parse(JSON.stringify(messagedit));
  functionMS.value.toggle();
};
const isReply = ref(false);

const replyMS = (message) => {
  isReply.value = true;
  functionMS.value.toggle();
};
const clearMS = () => {
  isEdit.value = false;
  isReply.value = false;
  fileAttach.value = [];
  modelcomment.value = { contents: "" };
};
const sendMS = (event) => {
  if (!modelcomment.value.contents) {
    return;
  }
  if (submitted.value) {
    swal.fire({
      type: "error",
      icon: "error",
      title: "",
      text: "Đang gửi tin, vui chờ trong giây lát!",
    });
    return;
  }
  submitted.value = true;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  var obj = { ...modelcomment.value };
  var formData = new FormData();
  formData.append("isEdit", isEdit.value);
  formData.append("isReply", isReply.value);
  formData.append("calendar_id", route.params.id);
  var reply_comment_id = null;
  if (mesFocus.value && mesFocus.value.comment_id != null) {
    reply_comment_id = mesFocus.value.comment_id;
  }
  formData.append("reply_comment_id", reply_comment_id);
  formData.append("model", JSON.stringify(obj));
  var hasfile = fileAttach.value && fileAttach.value.length > 0;
  if (!hasfile) {
    swal.showLoading();
  } else {
    fileAttach.value.forEach((file, i) => {
      formData.append("files", file);
    });

    // $("#modalfileProgress").modal("show");
    // var fileProgress = document.getElementById("fileProgress");
    // var desProgress = document.getElementById("podes");
    // fileProgress.setAttribute("value", 0);
    // fileProgress.setAttribute("max", 0);
    // fileProgress.style.display = "block";
  }
  axios
    .put(baseURL + "/api/calendar_week/send_comment", formData, config)
    .then((response) => {
      if (response.data.err === "1") {
        swal.fire({
          title: "Error!",
          text: response.data.ms,
          icon: "error",
          confirmButtonText: "OK",
        });
        return;
      }
      //Socket sendMessage
      if (detail.value.members && detail.value.members.length > 0) {
        var uids = detail.value.members
          .filter((x) => x["user_id"] !== store.getters.user.user_id)
          .map((x) => x["user_id"]);
        var message = {
          event: "sendMessage",
          socket_id: socket.id,
          token_id: store.getters.user.token_id,
          user_id: store.getters.user.user_id,
          contents: obj.contents,
          date: new Date(),
          uids: uids,
        };
        socket.emit("sendData", message);
      }
      panelEmoij.value.toggle(event);
      isEdit.value = false;
      isReply.value = false;
      fileAttach.value = [];
      initComment();
      modelcomment.value = { contents: "" };
      swal.close();
      toast.success("Gửi thành công!");
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
      swal.fire({
        title: "Error!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
  if (submitted.value) submitted.value = false;
};
const deleteMS = (message) => {
  functionMS.value.toggle();
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá bình luận này không!",
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
        var ids = [message["comment_id"]];
        axios
          .delete(baseURL + "/api/calendar_week/delete_calendar_comment", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: ids,
          })
          .then((response) => {
            if (response.data.err === "1") {
              swal.fire({
                title: "Error!",
                text: response.data.ms,
                icon: "error",
                confirmButtonText: "OK",
              });
              return;
            }
            initComment();
            swal.close();
            toast.success("Xoá bình luận thành công!");
            if (options.value.loading) options.value.loading = false;
          })
          .catch((error) => {
            swal.close();
            if (options.value.loading) options.value.loading = false;
            addLog({
              title: "Lỗi Console delItem",
              controller: "boardroom.vue",
              logcontent: error.message,
              loai: 2,
            });
            if (error.status === 401) {
              swal.fire({
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
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
const changeContent = (ev) => {
  if (ev.keyCode == 13 && !ev.shiftKey) {
    ev.preventDefault();
  }
};

//init
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
        }
      }
    })
    .then(() => {
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
const detail = ref({});
const initData = (rf) => {
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
              { par: "calendar_id", va: route.params.id },
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
        if (tbs[0] != null && tbs[0].length > 0) {
          detail.value = tbs[0][0];
        } else {
          detail.value = {};
        }
        if (detail.value["contents"]) {
          detail.value["contents"] = detail.value["contents"].replaceAll(
            "\n",
            "<br/>"
          );
        }
        if (detail.value["invitee"] != null) {
          detail.value["invitee"] = detail.value["invitee"].replaceAll(
            "\n",
            "<br/>"
          );
        }
        if (detail.value["equip"] != null) {
          detail.value["equip"] = detail.value["equip"].replaceAll(
            "\n",
            "<br/>"
          );
        }
        if (detail.value["note"] != null) {
          detail.value["note"] = detail.value["note"].replaceAll("\n", "<br/>");
        }
        var idx = typestatus.value.findIndex(
          (x) => x["value"] === detail.value["status"]
        );
        if (idx != -1) {
          detail.value["status_name"] = typestatus.value[idx]["title"];
          detail.value["bg_color"] = typestatus.value[idx]["bg_color"];
          detail.value["text_color"] = typestatus.value[idx]["text_color"];
        } else {
          detail.value["status_name"] = "Chưa xác định";
          detail.value["bg_color"] = "#bbbbbb";
          detail.value["text_color"] = "#fff";
        }
        if (
          detail.value["boardroom_id"] == null &&
          detail.value["place_name"] != null
        ) {
          detail.value["boardroom_id"] = detail.value["place_name"];
        }
        if (detail.value.start_date != null && detail.value.start_date != "") {
          if (props.group === 0) {
            detail.value.start_date = moment(
              new Date(detail.value.start_date)
            ).format("HH:mm DD/MM/YYYY");
          } else if (props.group === 1) {
            detail.value.start_date = moment(
              new Date(detail.value.start_date)
            ).format("DD/MM/YYYY");
          }
        }
        if (detail.value.end_date != null && detail.value.end_date != "") {
          if (props.group === 0) {
            detail.value.end_date = moment(
              new Date(detail.value.end_date)
            ).format("HH:mm DD/MM/YYYY");
          } else if (props.group === 1) {
            detail.value.end_date = moment(
              new Date(detail.value.end_date)
            ).format("DD/MM/YYYY");
          }
        }
        if (
          detail.value.created_date != null &&
          detail.value.created_date != ""
        ) {
          detail.value.created_date = moment(
            new Date(detail.value.created_date)
          ).format("HH:mm DD/MM/YYYY");
        }
        if (tbs[1] != null && tbs[1].length > 0) {
          var members = tbs[1];
          detail.value.members = tbs[1];
          detail.value.chutris = members.filter((x) => x["is_type"] === 0);
          detail.value.thamgias = members.filter((x) => x["is_type"] === 1);
        } else {
          detail.value.chutris = [];
          detail.value.thamgias = [];
        }
        if (tbs[2] != null && tbs[2].length > 0) {
          detail.value.departments = tbs[2].map((x) => x["department_id"]);
        } else {
          detail.value.departments = [];
        }
        if (tbs[3] != null && tbs[3].length > 0) {
          detail.value.files = tbs[3];
        } else {
          detail.value.files = [];
        }
        if (tbs[4] && tbs[4].length > 0) {
          tbs[4].forEach((r) => {
            // if (r["contents"] != null) {
            //   r["contents"] = r["contents"].replaceAll("\n", "<br/>");
            // }
            if (r["files"] != null) {
              r["files"] = JSON.parse(r["files"]);
            }
            if (r["created_date"] != null) {
              r["created_date"] = new Date(r["created_date"]);
            }
            r.countReply = [];
            r.countReply = tbs[4].filter((x) => x.parent_id === r.comment_id);
            if (r.parent_id != null) {
              r.comments = tbs[4].filter((x) => x.parent_id === r.comment_id);
              r.parentcomment = tbs[4].find(
                (x) => x.comment_id === r.parent_id
              );
            }
          });
        }
        detail.value.comments = tbs[4];
      }
      swal.close();
      if (options.value.loading) options.value.loading = false;
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      if (options.value.loading) options.value.loading = false;
      addLog({
        title: "Lỗi Console editItem",
        controller: "boardroom.vue",
        logcontent: error.message,
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
const initComment = () => {
  axios
    .post(
      baseURL + "/api/calendar/get_datas",
      {
        str: encr(
          JSON.stringify({
            proc: "calendar_get_comment",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "calendar_id", va: route.params.id },
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
        let tbs = JSON.parse(data);
        if (tbs[0] && tbs[0].length > 0) {
          tbs[0].forEach((r) => {
            if (r["contents"] != null) {
              r["contents"] = r["contents"].replaceAll("\n", "<br/>");
            }
            if (r["files"] != null) {
              r["files"] = JSON.parse(r["files"]);
            }
            if (r["created_date"] != null) {
              r["created_date"] = new Date(r["created_date"]);
            }
            r.countReply = [];
            r.countReply = tbs[0].filter((x) => x.parent_id === r.comment_id);
            if (r.parent_id != null) {
              r.comments = tbs[0].filter((x) => x.parent_id === r.comment_id);
              r.parentcomment = tbs[0].find(
                (x) => x.comment_id === r.parent_id
              );
            }
          });
          detail.value.comments = tbs[0];
        } else {
          detail.value.comments = [];
        }
      }
    })
    .catch((error) => {
      swal.fire({
        title: "Error!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};

onMounted(() => {
  if (route.params.id != null) {
    selectedNodes.value = [{ calendar_id: route.params.id }];
    initDictionary();
  } else {
    router.back();
    return;
  }

  socket.on("sendMessage", (data) => {
    var users = store.getters.userConnected;
    if (users != null && users.length > 0) {
      for (let i = 0; i < users.length; i++) {
        const user = users[i];
        if (user.connected && user.user_id === data.user_id) {
          initComment();
        }
      }
    }
  });
});
</script>
<template>
  <div class="surface-100 p-3 calendar" style="height: calc(100vh - 50px)">
    <div class="grid formgrid box-detail m-0">
      <div class="col-9 md:col-9 p-0 h-full">
        <div class="m-3 relative">
          <div style="height: calc(100vh - 88px)">
            <ul class="list-style-none p-0 mb-3">
              <li class="mr-2 f-left">
                <Button
                  @click="goBack()"
                  type="button"
                  label="Quay lại"
                  icon="pi pi-arrow-left"
                  class="p-button"
                  style="
                    background-color: #5bc0de !important;
                    border: 1px solid #5bc0de !important;
                  "
                />
              </li>
              <li class="mr-2 f-left">
                <Button
                  @click="scrollMember()"
                  type="button"
                  label="Người tham gia"
                  icon="pi pi-users"
                  class="p-button"
                  :badge="detail.numeric_attendees"
                  badgeClass=""
                  style="
                    background-color: #fbad4c !important;
                    border: 1px solid #fbad4c !important;
                  "
                />
              </li>
              <li v-if="detail.comments" class="mr-2 f-left">
                <Button
                  @click="scrollComment()"
                  type="button"
                  label="Thảo luận"
                  icon="pi pi-comments"
                  class="p-button"
                  :badge="detail.comments.length"
                  badgeClass=""
                  style="
                    background-color: #ff646d !important;
                    border: 1px solid #ff646d !important;
                  "
                />
              </li>
              <li v-if="detail.files" class="mr-2 f-left">
                <Button
                  @click="scrollFile()"
                  type="button"
                  label="Tài liệu"
                  icon="pi pi-paperclip"
                  class="p-button"
                  :badge="detail.files.length"
                  badgeClass=""
                />
              </li>
              <li class="mr-2" v-if="detail.is_type === 1">
                <Button
                  @click="goMeeting(detail)"
                  type="button"
                  label="Meeting"
                  icon="pi pi-video"
                  class="p-button"
                  style="
                    background-color: #59d05d !important;
                    border: 1px solid #59d05d !important;
                  "
                />
              </li>
            </ul>
            <div
              id="div-scroll"
              class="scroll-outer"
              style="width: 100%; height: calc(100vh - 238px); overflow-y: auto"
            >
              <div class="scroll-inner">
                <div class="grid formgrid m-3">
                  <div class="col-12 md:col-12 p-0">
                    <div class="flex">
                      <div class="mr-3">
                        <i class="pi pi-info-circle"></i>
                      </div>
                      <div class="form-group">
                        <label><b> Thông tin chi tiết:</b></label>
                        <div>
                          Tạo bởi:
                          <b style="color: #2196f3">{{ detail.full_name }}</b> -
                          <i>{{ detail.created_date }}</i>
                        </div>
                      </div>
                    </div>
                  </div>
                  <div class="col-12 md:col-12 p-0">
                    <div class="flex">
                      <div class="mr-3">
                        <i class="pi pi-check-square"></i>
                      </div>
                      <div class="form-group">
                        <label><b>Nội dung:</b></label>
                        <div v-html="detail.contents"></div>
                      </div>
                    </div>
                  </div>
                  <div class="col-6 md:col-6 p-0">
                    <div class="flex">
                      <div class="mr-3">
                        <i class="pi pi-calendar"></i>
                      </div>
                      <div class="form-group">
                        <label><b>Ngày bắt đầu:</b></label>
                        <div style="color: #2196f3">
                          {{ detail.start_date }}
                        </div>
                      </div>
                    </div>
                  </div>
                  <div class="col-6 md:col-6 p-0">
                    <div class="flex">
                      <div class="mr-3">
                        <i class="pi pi-calendar"></i>
                      </div>
                      <div class="form-group">
                        <label><b>Ngày kết thúc:</b></label>
                        <div style="color: #2196f3">
                          {{ detail.end_date }}
                        </div>
                      </div>
                    </div>
                  </div>
                  <div id="div-member" class="col-12 md:col-12 p-0">
                    <div class="flex">
                      <div class="mr-3">
                        <i class="pi pi-user"></i>
                      </div>
                      <div class="form-group">
                        <label><b> Người chủ trì:</b></label>
                        <div>
                          <ul
                            class="p-ulchip"
                            v-if="detail.chutris && detail.chutris.length > 0"
                          >
                            <li
                              class="p-lichip"
                              v-for="(value, index) in detail.chutris"
                              :key="index"
                            >
                              <Chip
                                :image="value.avatar"
                                :label="value.full_name"
                                class="mr-2 mb-2 pl-0"
                              >
                                <div class="flex">
                                  <div class="format-flex-center">
                                    <Avatar
                                      v-bind:label="
                                        value.avatar
                                          ? ''
                                          : (value.last_name ?? '').substring(
                                              0,
                                              1
                                            )
                                      "
                                      v-bind:image="
                                        basedomainURL + value.avatar
                                      "
                                      style="
                                        background-color: #2196f3;
                                        color: #ffffff;
                                        width: 2rem;
                                        height: 2rem;
                                      "
                                      :style="{
                                        background: bgColor[index % 7],
                                      }"
                                      class="mr-2 text-avatar"
                                      size="xlarge"
                                      shape="circle"
                                    />
                                  </div>
                                  <div class="format-flex-center">
                                    <span>{{ value.full_name }}</span>
                                  </div>
                                </div>
                              </Chip>
                            </li>
                          </ul>
                        </div>
                      </div>
                    </div>
                  </div>
                  <div class="col-12 md:col-12 p-0">
                    <div class="flex">
                      <div class="mr-3">
                        <i class="pi pi-users"></i>
                      </div>
                      <div class="form-group">
                        <label><b>Người tham gia:</b></label>
                        <div>
                          <ul
                            class="p-ulchip"
                            v-if="detail.thamgias && detail.thamgias.length > 0"
                          >
                            <li
                              class="p-lichip"
                              v-for="(value, index) in detail.thamgias"
                              :key="index"
                            >
                              <Chip
                                :image="value.avatar"
                                :label="value.full_name"
                                class="mr-2 mb-2 pl-0"
                              >
                                <div class="flex">
                                  <div class="format-flex-center">
                                    <Avatar
                                      v-bind:label="
                                        value.avatar
                                          ? ''
                                          : (value.last_name ?? '').substring(
                                              0,
                                              1
                                            )
                                      "
                                      v-bind:image="
                                        basedomainURL + value.avatar
                                      "
                                      style="
                                        background-color: #2196f3;
                                        color: #ffffff;
                                        width: 2rem;
                                        height: 2rem;
                                      "
                                      :style="{
                                        background: bgColor[index % 7],
                                      }"
                                      class="mr-2 text-avatar"
                                      size="xlarge"
                                      shape="circle"
                                    />
                                  </div>
                                  <div class="format-flex-center">
                                    <span>{{ value.full_name }}</span>
                                  </div>
                                </div>
                              </Chip>
                            </li>
                          </ul>
                        </div>
                      </div>
                    </div>
                  </div>
                  <div class="col-12 md:col-12 p-0">
                    <div class="flex">
                      <div class="mr-3">
                        <i class="pi pi-home"></i>
                      </div>
                      <div class="form-group">
                        <label
                          ><b
                            >{{
                              options.is_group === 0
                                ? "Địa điểm họp"
                                : options.is_group === 1
                                ? "Địa điểm công tác"
                                : ""
                            }}
                            :</b
                          ></label
                        >
                        <div>
                          {{ detail.boardroom_name }}
                        </div>
                      </div>
                    </div>
                  </div>
                  <div
                    v-if="options.is_group === 1 && detail.car_name != null"
                    class="col-12 md:col-12 p-0"
                  >
                    <div class="flex">
                      <div class="mr-3">
                        <i class="pi pi-car"></i>
                      </div>
                      <div class="form-group">
                        <label><b>Xe sử dụng:</b></label>
                        <div>
                          {{ detail.car_name }}
                        </div>
                      </div>
                    </div>
                  </div>
                  <div
                    v-if="options.is_group === 0"
                    class="col-12 md:col-12 p-0"
                  >
                    <div class="flex">
                      <div class="mr-3">
                        <i class="pi pi-video"></i>
                      </div>
                      <div class="form-group">
                        <label><b>Hình thức họp:</b></label>
                        <div>
                          {{
                            detail.is_type === 0
                              ? "Họp bình thường"
                              : detail.is_type === 1
                              ? "Họp trực tuyến"
                              : ""
                          }}
                        </div>
                      </div>
                    </div>
                  </div>
                  <div v-if="detail.invitee" class="col-12 md:col-12 p-0">
                    <div class="flex">
                      <div class="mr-3">
                        <i class="pi pi-users"></i>
                      </div>
                      <div class="form-group">
                        <label><b>Người được mời:</b></label>
                        <div v-html="detail.invitee"></div>
                      </div>
                    </div>
                  </div>
                  <div
                    v-if="detail.departments && detail.departments.length > 0"
                    class="col-12 md:col-12 p-0"
                  >
                    <div class="flex">
                      <div class="mr-3">
                        <i class="pi pi-building"></i>
                      </div>
                      <div class="form-group">
                        <label><b>Phòng ban được mời:</b></label>
                        <div>
                          <span
                            v-for="(item, index) in detail.departments"
                            :key="index"
                          >
                            <span
                              v-if="
                                index > 0 && index < detail.departments.length
                              "
                              >,
                            </span>
                            {{
                              departments.find(
                                (x) => x["department_id"] === item
                              ).department_name
                            }}
                          </span>
                        </div>
                      </div>
                    </div>
                  </div>
                  <div v-if="detail.equip" class="col-12 md:col-12 p-0">
                    <div class="flex">
                      <div class="mr-3">
                        <i class="pi pi-file"></i>
                      </div>
                      <div class="form-group">
                        <label><b>Chuẩn bị:</b></label>
                        <div v-html="detail.equip"></div>
                      </div>
                    </div>
                  </div>
                  <div v-if="detail.note" class="col-12 md:col-12 p-0">
                    <div class="flex">
                      <div class="mr-3">
                        <i class="pi pi-file"></i>
                      </div>
                      <div class="form-group">
                        <label><b>Ghi chú:</b></label>
                        <div v-html="detail.note"></div>
                      </div>
                    </div>
                  </div>
                  <div
                    id="div-file"
                    v-if="detail.files && detail.files.length > 0"
                    class="col-12 md:col-12 p-0"
                  >
                    <div class="flex">
                      <div class="mr-3">
                        <i class="pi pi-paperclip"></i>
                      </div>
                      <div class="form-group">
                        <label><b>Tài liệu đính kèm:</b></label>
                        <DataView
                          :lazy="true"
                          :value="detail.files"
                          :rowHover="true"
                          :scrollable="true"
                          class="
                            w-full
                            h-full
                            ptable
                            p-datatable-sm
                            flex flex-column
                          "
                          layout="list"
                          responsiveLayout="scroll"
                        >
                          <template #list="slotProps">
                            <div class="w-full">
                              <Toolbar class="w-full">
                                <template #start>
                                  <div
                                    @click="goFile(slotProps.data)"
                                    class="flex align-items-center"
                                  >
                                    <img
                                      class="mr-2"
                                      :src="
                                        basedomainURL +
                                        '/Portals/Image/file/' +
                                        slotProps.data.file_type +
                                        '.png'
                                      "
                                      style="object-fit: contain"
                                      width="24"
                                      height="24"
                                    />
                                    <span style="line-height: 1.5">
                                      {{ slotProps.data.file_name }}</span
                                    >
                                  </div>
                                </template>
                              </Toolbar>
                            </div>
                          </template>
                        </DataView>
                      </div>
                    </div>
                  </div>
                  <div
                    id="div-comment"
                    class="col-12 md:col-12 p-0"
                    v-if="detail.comments && detail.comments.length > 0"
                  >
                    <div class="flex">
                      <div class="mr-3">
                        <i class="pi pi-comments"></i>
                      </div>
                      <div class="form-group">
                        <label><b>Thảo luận:</b></label>
                        <div class="box-comment">
                          <div class="list-comment">
                            <div
                              class="item-comment"
                              v-for="(item, index) in detail.comments"
                              :key="index"
                              :class="
                                item.isMe
                                  ? 'justify-content-end'
                                  : 'justify-content-start'
                              "
                            >
                              <div class="item-comment-image" v-if="!item.isMe">
                                <Avatar
                                  v-bind:label="
                                    item.avatar
                                      ? ''
                                      : (item.last_name ?? '').substring(0, 1)
                                  "
                                  v-bind:image="basedomainURL + item.avatar"
                                  style="
                                    background-color: #2196f3;
                                    color: #ffffff;
                                    width: 3rem;
                                    height: 3rem;
                                  "
                                  :style="{
                                    background: bgColor[index % 7],
                                  }"
                                  class="mr-2 text-avatar"
                                  size="xlarge"
                                  shape="circle"
                                />
                              </div>
                              <div
                                class="item-comment-content"
                                :class="{ isMe: item.isMe }"
                              >
                                <div
                                  v-if="item.parent_id && item.parentcomment"
                                >
                                  <div class="flex">
                                    <div class="mr-2">
                                      <font-awesome-icon
                                        icon="fa-solid fa-quote-right"
                                        style="font-size: 1rem"
                                      />
                                    </div>
                                    <div style="flex: 1">
                                      <div class="item-comment">
                                        <div class="item-comment-content">
                                          <div class="d-grid">
                                            <div
                                              v-html="
                                                item.parentcomment.contents
                                              "
                                            ></div>
                                            <div class="list-message-file">
                                              <ul class="list-style-none">
                                                <li
                                                  class="item-comment-file"
                                                  v-for="f in item.parentcomment
                                                    .files"
                                                  :key="f.file_id"
                                                  @click="goFile(f)"
                                                >
                                                  <div
                                                    class="item-file-content"
                                                  >
                                                    <img
                                                      width="32"
                                                      v-bind:src="
                                                        basedomainURL +
                                                        '/Portals/Image/file/' +
                                                        f.file_type +
                                                        '.png'
                                                      "
                                                    />
                                                    <b>{{ f.file_name }}</b>
                                                    <span>{{
                                                      formatByte(f.file_size)
                                                    }}</span>
                                                  </div>
                                                </li>
                                              </ul>
                                            </div>
                                            <div class="description mt-2">
                                              <span>{{
                                                item.parentcomment.full_name
                                              }}</span>
                                              -
                                              <timeago
                                                :datetime="
                                                  item.parentcomment
                                                    .created_date
                                                "
                                                :locale="vi"
                                              />
                                            </div>
                                          </div>
                                        </div>
                                      </div>
                                    </div>
                                  </div>
                                  <div>
                                    <hr
                                      style="
                                        border: solid 1px rgba(0, 0, 0, 0.1);
                                      "
                                    />
                                  </div>
                                </div>
                                <div class="flex">
                                  <div style="flex: 1">
                                    <b style="font-size: 14px">{{
                                      item.full_name
                                    }}</b>
                                    <div class="description mb-2">
                                      {{ item.position_name }}
                                    </div>
                                    <div
                                      v-html="trustAsHtml(item.contents)"
                                    ></div>
                                    <div class="list-message-file">
                                      <ul class="list-style-none">
                                        <li
                                          class="item-comment-file"
                                          v-for="f in item.files"
                                          :key="f.file_id"
                                          @click="goFile(f)"
                                        >
                                          <div class="item-file-content">
                                            <img
                                              width="32"
                                              v-bind:src="
                                                basedomainURL +
                                                '/Portals/Image/file/' +
                                                f.file_type +
                                                '.png'
                                              "
                                            />
                                            <b>{{ f.file_name }}</b>
                                            <span>{{
                                              formatByte(f.file_size)
                                            }}</span>
                                          </div>
                                        </li>
                                      </ul>
                                    </div>
                                    <div class="description mt-2">
                                      <timeago
                                        :datetime="item.created_date"
                                        :locale="vi"
                                      />
                                    </div>
                                  </div>
                                  <div>
                                    <Button
                                      type="button"
                                      icon="pi pi-ellipsis-h"
                                      class="icon-comment-function"
                                      @click="toogleFunctionMS($event, item)"
                                      aria-haspopup="true"
                                      aria-controls="overlay_panelFunctionMS"
                                    />
                                  </div>
                                </div>
                              </div>
                              <div class="item-comment-image" v-if="item.isMe">
                                <Avatar
                                  v-bind:label="
                                    item.avatar
                                      ? ''
                                      : (item.last_name ?? '').substring(0, 1)
                                  "
                                  v-bind:image="basedomainURL + item.avatar"
                                  style="
                                    background-color: #2196f3;
                                    color: #ffffff;
                                    width: 3rem;
                                    height: 3rem;
                                  "
                                  :style="{
                                    background: bgColor[index % 7],
                                  }"
                                  class="ml-2 text-avatar"
                                  size="xlarge"
                                  shape="circle"
                                />
                              </div>
                            </div>
                          </div>
                        </div>
                      </div>
                    </div>
                    <OverlayPanel
                      ref="functionMS"
                      appendTo="body"
                      class="p-0 m-0 panelFuncionMS"
                      :showCloseIcon="false"
                      id="overlay_panelFunctionMS"
                      style="width: fit-content"
                    >
                      <div>
                        <ul class="list-style-none">
                          <li
                            class="py-2 px-3 item-function-info"
                            v-if="mesFocus.isMe"
                          >
                            <a @click="editMS(mesFocus)" class="d-b td-n">
                              <font-awesome-icon icon="fa-solid fa-pencil" />
                              <span class="ml-1"> Chỉnh sửa</span>
                            </a>
                          </li>
                          <li class="py-2 px-3 item-function-info">
                            <a @click="replyMS(mesFocus)" class="d-b td-n">
                              <font-awesome-icon
                                icon="fa-solid fa-quote-right"
                              />
                              <span class="ml-1"> Trả lời</span></a
                            >
                          </li>
                          <li
                            class="py-2 px-3 item-function-warning"
                            v-if="mesFocus.isMe"
                          >
                            <a
                              @click="deleteMS(mesFocus)"
                              class="d-b td-n"
                              style="color: red"
                            >
                              <i class="pi pi-trash"></i>
                              <span class="ml-1"> Xóa</span>
                            </a>
                          </li>
                        </ul>
                      </div>
                    </OverlayPanel>
                  </div>
                </div>
              </div>
            </div>
          </div>
          <div class="absolute w-full" style="bottom: 0">
            <div
              class="mes-file"
              v-if="
                (fileAttach && fileAttach.length > 0) ||
                (isEdit &&
                  mesFocus &&
                  mesFocus.files &&
                  mesFocus.files.length > 0)
              "
            >
              <h3 class="py-2 m-0" style="color: #2196f3">
                Danh sách file đã chọn
              </h3>
              <div>
                <ul class="list-style-none pb-2">
                  <li
                    class="item-file"
                    v-for="(f, idx) in fileAttach"
                    :key="idx"
                  >
                    <div class="item-file-content">
                      <img
                        width="32"
                        v-bind:src="
                          basedomainURL +
                          '/Portals/Image/file/' +
                          f.file_type +
                          '.png'
                        "
                      />
                      <b>{{ f.file_name }}</b>
                      <span>{{ formatByte(f.file_size) }}</span>
                      <div
                        style="
                          width: 40px;
                          position: absolute;
                          top: -10px;
                          right: -20px;
                        "
                      >
                        <a
                          class="trash"
                          @click="removeFileMS(fileAttach, f, idx)"
                          ><i
                            style="font-size: 20px"
                            class="pi pi-times-circle"
                          ></i
                        ></a>
                      </div>
                    </div>
                  </li>
                  <func
                    v-if="
                      mesFocus && mesFocus.files && mesFocus.files.length > 0
                    "
                  >
                    <li
                      class="item-file"
                      v-for="(f, idx) in mesFocus.files"
                      :key="idx"
                    >
                      <div class="item-file-content">
                        <img
                          width="32"
                          v-bind:src="
                            basedomainURL +
                            '/Portals/Image/file/' +
                            f.file_type +
                            '.png'
                          "
                        />
                        <b>{{ f.file_name }}</b>
                        <span>{{ formatByte(f.file_size) }}</span>
                        <div
                          style="
                            width: 40px;
                            position: absolute;
                            top: -10px;
                            right: -20px;
                          "
                        >
                          <a
                            class="trash"
                            @click="removeFileMS(mesFocus.files, f, idx)"
                            ><i
                              style="font-size: 20px"
                              class="pi pi-times-circle"
                            ></i
                          ></a>
                        </div>
                      </div>
                    </li>
                  </func>
                </ul>
              </div>
            </div>
            <div class="mes-reply" v-if="isReply">
              <div class="flex">
                <div class="mr-2">
                  <font-awesome-icon
                    icon="fa-solid fa-quote-right"
                    style="font-size: 1rem"
                  />
                </div>
                <div style="flex: 1">
                  <div class="item-comment">
                    <div class="item-comment-content">
                      <div class="d-grid">
                        <div v-html="mesFocus.contents"></div>
                        <div class="list-message-file">
                          <ul class="list-style-none">
                            <li
                              class="item-comment-file"
                              v-for="f in mesFocus.files"
                              :key="f.file_id"
                              @click="goFile(f)"
                            >
                              <div class="item-file-content">
                                <img
                                  width="32"
                                  v-bind:src="
                                    basedomainURL +
                                    '/Portals/Image/file/' +
                                    f.file_type +
                                    '.png'
                                  "
                                />
                                <b>{{ f.file_name }}</b>
                                <span>{{ formatByte(f.file_size) }}</span>
                              </div>
                            </li>
                          </ul>
                        </div>
                        <div class="description mt-2">
                          <span>{{ mesFocus.full_name }}</span> -
                          <timeago
                            :datetime="mesFocus.created_date"
                            :locale="vi"
                          />
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
                <div>
                  <a class="hover" @click="clearMS(0)">
                    <i
                      class="pi pi-times-circle"
                      style="color: red !important"
                    ></i>
                  </a>
                </div>
              </div>
            </div>
            <div class="mes-box mb-3">
              <Textarea
                id="centents"
                autoResize="false"
                v-model="modelcomment.contents"
                v-on:keypress="changeContent($event)"
                v-on:keydown.enter.exact.prevent="sendMS($event)"
                v-on:keydown.enter.shift.exact.prevent="
                  modelcomment.contents += '\n'
                "
                class="mes-content scroll-width-thin"
                placeholder="Nhập nội dung bình luận..."
              >
              </Textarea>
              <div class="mes-function">
                <ul class="list-item-function">
                  <li
                    v-if="isEdit || isReply"
                    class="item-function"
                    @click="clearMS()"
                    style="color: red !important"
                  >
                    <i class="pi pi-times-circle"></i>
                  </li>
                  <li class="item-function" @click="showEmoji($event)">
                    <font-awesome-icon
                      icon="fa-solid fa-face-smile"
                      style="color: orange"
                    />
                    <OverlayPanel
                      class="p-0 m-0 cuttom-emoij"
                      ref="panelEmoij"
                      appendTo="body"
                      :showCloseIcon="false"
                      id="overlay_panelEmoij"
                    >
                      <VuemojiPicker
                        :isDark="false"
                        @emojiClick="handleEmojiClick"
                        :pickerStyle="{
                          borderColor: '#fff',
                        }"
                      />
                    </OverlayPanel>
                  </li>
                  <li class="item-function" @click="chooseFile('imageUpChat')">
                    <i class="pi pi-image"></i>
                    <input
                      class="hidden"
                      id="imageUpChat"
                      type="file"
                      multiple="true"
                      accept="image/*"
                      @change="putFileUpload"
                    />
                  </li>
                  <li class="item-function" @click="chooseFile('fileUpChat')">
                    <i class="pi pi-paperclip"></i>
                    <input
                      class="hidden"
                      id="fileUpChat"
                      type="file"
                      multiple="true"
                      accept=".xlsx,.xls,image/*,.doc, .docx,.ppt, .pptx,.txt,.pdf"
                      @change="putFileUpload"
                    />
                  </li>
                  <li class="item-function" @click="sendMS($event)">
                    <i class="pi pi-send"></i>
                  </li>
                </ul>
              </div>
            </div>
          </div>
        </div>
      </div>
      <div
        class="col-3 md:col-3 p-0 h-full"
        style="border-left: solid 1px rgba(0, 0, 0, 0.1)"
      >
        <div class="m-3">
          <div
            class="scroll-outer"
            style="width: 100%; height: calc(100vh - 87px); overflow-y: auto"
          >
            <div class="scroll-inner">
              <ul class="list-style-none d-grid p-0">
                <li v-if="detail.status_name">
                  <Button
                    type="button"
                    :label="'Trạng thái ' + detail.status_name"
                    icon="pi pi-info-circle"
                    class="p-button"
                    :style="{
                      backgroundColor: detail.bg_color,
                      border: detail.bg_color,
                      color: detail.text_color,
                    }"
                    style="width: 100%"
                  />
                </li>
                <li class="mt-3" v-if="detail.is_approve">
                  <Button
                    @click="show_send = !(show_send || false)"
                    type="button"
                    label="Chuyển xử lý"
                    icon="pi pi-send"
                    class="p-button p-button-hover-info"
                    style="
                      background-color: #f5f5f5;
                      border: 1px solid #f5f5f5;
                      color: #495057;
                      width: 100%;
                    "
                  >
                    <div
                      style="
                        display: flex;
                        flex: 1;
                        justify-content: space-between;
                      "
                    >
                      <span>
                        <i class="pi pi pi-send"></i>
                      </span>
                      <span class="ml-2">Chuyển xử lý</span>
                      <span>
                        <i class="pi pi-angle-down"></i>
                      </span>
                    </div>
                  </Button>
                  <ul
                    v-if="
                      (detail.status === 0 ||
                        detail.status === 3 ||
                        detail.status === 4) &&
                      show_send
                    "
                    class="list-style-none d-grid p-0"
                  >
                    <li v-for="(item, index) in itemSends" :key="index">
                      <div v-if="item.id !== 3" class="mt-3">
                        <Button
                          @click="openAddDialogSend(item.label, item.id)"
                          type="button"
                          :label="item.label"
                          :icon="item.icon"
                          class="p-button p-button-hover-info"
                          style="
                            background-color: #f5f5f5;
                            border: 1px solid #f5f5f5;
                            color: #495057;
                            width: 100%;
                          "
                        />
                      </div>
                      <div v-if="item.id === 3 && detail.is_enact" class="mt-3">
                        <Button
                          @click="openAddDialogEnact(item.label, item.id)"
                          type="button"
                          :label="item.label"
                          :icon="item.icon"
                          class="p-button p-button-hover-info"
                          style="
                            background-color: #f5f5f5;
                            border: 1px solid #f5f5f5;
                            color: #495057;
                            width: 100%;
                          "
                        />
                      </div>
                    </li>
                  </ul>
                  <ul
                    v-if="detail.status === 1 && show_send"
                    class="list-style-none d-grid p-0"
                  >
                    <li v-for="(item, index) in itemApproves" :key="index">
                      <div v-if="item.id !== 2" class="mt-3">
                        <Button
                          @click="openAddDialogApprove(item.label, item.id)"
                          type="button"
                          :label="item.label"
                          :icon="item.icon"
                          class="p-button p-button-hover-info"
                          style="
                            background-color: #f5f5f5;
                            border: 1px solid #f5f5f5;
                            color: #495057;
                            width: 100%;
                          "
                        />
                      </div>
                      <div v-if="item.id === 2 && detail.is_enact" class="mt-3">
                        <Button
                          @click="openAddDialogEnact(item.label, item.id)"
                          type="button"
                          :label="item.label"
                          :icon="item.icon"
                          class="p-button p-button-hover-info"
                          style="
                            background-color: #f5f5f5;
                            border: 1px solid #f5f5f5;
                            color: #495057;
                            width: 100%;
                          "
                        />
                      </div>
                    </li>
                  </ul>
                </li>
                <li class="mt-3">
                  <Button
                    @click="logItem(detail)"
                    type="button"
                    label="Quy trình xử lý"
                    icon="pi pi-chart-bar"
                    class="p-button p-button-hover-info"
                    style="
                      background-color: #f5f5f5;
                      border: 1px solid #f5f5f5;
                      color: #495057;
                      width: 100%;
                    "
                  />
                </li>
                <li class="mt-3" v-if="detail.is_edit">
                  <Button
                    @click="editItem(detail)"
                    type="button"
                    label="Chỉnh sửa"
                    icon="pi pi-pencil"
                    class="p-button p-button-hover-info"
                    style="
                      background-color: #f5f5f5;
                      border: 1px solid #f5f5f5;
                      color: #495057;
                      width: 100%;
                    "
                  />
                </li>
                <li class="mt-3" v-if="detail.is_cancel">
                  <Button
                    @click="cancelItem(detail)"
                    type="button"
                    label="Hủy"
                    icon="pi pi-times-circle"
                    class="p-button p-button-hover-info"
                    style="
                      background-color: #f5f5f5;
                      border: 1px solid #f5f5f5;
                      color: #495057;
                      width: 100%;
                    "
                  />
                </li>
                <li class="mt-3" v-if="detail.is_delete">
                  <Button
                    @click="deleteItem(detail)"
                    type="button"
                    label="Xóa"
                    icon="pi pi-trash"
                    class="p-button p-button-hover-warning"
                    style="
                      background-color: #f5f5f5;
                      border: 1px solid #f5f5f5;
                      color: red;
                      width: 100%;
                    "
                  />
                </li>
              </ul>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>

  <!--Model-->
  <dialogmodelweek
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

  <!--Send-->
  <dialogsend
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

  <!--logs-->
  <dialogchart
    :headerDialog="headerDialogLog"
    :displayDialog="displayDialogLog"
    :closeDialog="closeDialogLog"
    :is_type_calendar="is_type_calendar"
    :chartprocedures="chartprocedures"
    :chartsigns="chartsigns"
    :chartsignusers="chartsignusers"
    :chartlogs="chartlogs"
  />
</template>
<style scoped>
@import url(../component/stylecalendar.css);
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
::v-deep(.box-detail) {
  .p-button {
    padding: 0.75rem 1rem;
  }
  .p-button * {
    font-size: 14px;
  }
  .p-button .p-badge {
    display: inline-block;
    min-width: 1.3rem !important;
    height: 1.3rem !important;
    line-height: 1.3rem !important;
    border-radius: 0.25rem !important;
    padding: 0 0.25rem !important;
    font-size: 12px !important;
  }
}
::v-deep(.icon-comment-function) {
  padding: 0;
}
</style>
