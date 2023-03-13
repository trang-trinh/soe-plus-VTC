<script setup>
import { ref, inject, onMounted } from "vue";
import { encr } from "../../../util/function";
import moment from "moment";
import { socketMethod } from "../../../util/methodSocket";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { de } from "date-fns/locale";
import dialogmodelduty from "../component/dialogmodelduty.vue";
import dialogdutysend from "../component/dialogdutysend.vue";
import dialogdutyapprove from "../component/dialogdutyapprove.vue";
import dialogdutyenact from "../component/dialogdutyenact.vue";
import dialogchart from "../component/dialogchart.vue";
import dialogmodelmultipleduty from "../component/dialogmodelmultipleduty.vue";
import dialogxml from "../component/dialogxml.vue";
import frameprintduty from "../component/frameprintduty.vue";
import framewordduty from "../component/framewordduty.vue";

const cryoptojs = inject("cryptojs");
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
  state: Number, //1 Đặt lịch, 2 Chờ duyệt, 3 Theo dõi
});

//Declare
const isFirst = ref(true);
const datas = ref([]);
const datatrucbans = ref([]);
const datachihuys = ref([]);
const boardrooms = ref([]);
const mission = ref({});
const roleFunctions = ref({});
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
  is_group: 2,
  eye: 1,
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
  { value: 2, title: "Đã duyệt", bg_color: "#04D215", text_color: "#fff" },
  { value: 3, title: "Trả lại", bg_color: "#ff8b4e", text_color: "#fff" },
  { value: 4, title: "Hủy", bg_color: "red", text_color: "#fff" },
]);
const typeprocedures = ref([
  { value: 0, title: "Duyệt tuần tự" },
  { value: 1, title: "Duyệt một trong nhiều" },
  //{ value: 2, title: "Duyệt ngẫu nhiên" },
]);

//Model
const model = ref({
  status: 0,
  trucban: null,
  trucban: null,
  chihuy: null,
  boardroom_id: null,
  files: [],
});

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
const files = ref([]);
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
    is_timelot: 2,
    date_timelot: null,
    status: 0,
    trucban: null,
    chihuy: null,
    boardroom_id: null,
    files: [],
  };
  headerDialog.value = str;
  displayDialog.value = true;
};
const closeDialog = () => {
  model.value = {
    status: 0,
    trucbans: [],
    chihuys: [],
    files: [],
  };
  displayDialog.value = false;
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
            proc: "calendar_duty_get",
            par: [
              { par: "user_id", va: store.state.user.user_id },
              { par: "calendar_duty_id", va: item.calendar_duty_id },
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
          if (model.value.date_timelot != null) {
            model.value.date_timelot = new Date(model.value.date_timelot);
            model.value.date_timelot_time = model.value.date_timelot;
          }
          if (tbs[1] != null && tbs[1].length > 0) {
            var members = tbs[1];
            model.value.trucban =
              members.find((x) => x["is_type"] === 0) || null;
            model.value.chihuy =
              members.find((x) => x["is_type"] === 1) || null;
          }
          if (tbs[2] != null && tbs[2].length > 0) {
            model.value.files = tbs[2];
          } else {
            model.value.files = [];
          }
        }
      }
      swal.close();
      if (options.value.loading) options.value.loading = false;

      headerDialog.value = "Cập nhật lịch trực ban";
      displayDialog.value = true;
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      if (options.value.loading) options.value.loading = false;
      addLog({
        title: "Lỗi Console editItem",
        controller: "calendarduty.vue",
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
const deleteItem = (item) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá lịch trực ban này không!",
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
          ids = [item["calendar_duty_id"]];
        } else {
          if (selectedNodes.value.length > 0) {
            selectedNodes.value.forEach((row, i) => {
              ids.push(row["calendar_duty_id"]);
            });
          }
        }
        axios
          .delete(baseURL + "/api/calendar_duty/delete_calendar_duty", {
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
            //       (x) => x.user_id == element.calendar_duty_id
            //     );
            //     if (idx != -1) {
            //       datas.value.splice(idx, 1);
            //     }
            //   });
            // }
            swal.close();
            toast.success("Xoá lịch trực ban thành công!");
            if (options.value.loading) options.value.loading = false;
          })
          .catch((error) => {
            swal.close();
            if (options.value.loading) options.value.loading = false;
            addLog({
              title: "Lỗi Console delItem",
              controller: "calendarduty.vue",
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
            proc: "calendar_duty_get",
            par: [
              { par: "user_id", va: store.state.user.user_id },
              { par: "calendar_duty_id", va: item.calendar_duty_id },
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
          if (model.value.date_timelot != null) {
            model.value.date_timelot = new Date(model.value.date_timelot);
            model.value.date_timelot_time = model.value.date_timelot;
          }
          if (tbs[1] != null && tbs[1].length > 0) {
            var members = tbs[1];
            members.forEach((x, i) => {
              if (x["duty_person_id"] != null) {
                delete x["duty_person_id"];
              }
            });
            model.value.trucban =
              members.find((x) => x["is_type"] === 0) || null;
            model.value.chihuy =
              members.find((x) => x["is_type"] === 1) || null;
          }
          if (tbs[2] != null && tbs[2].length > 0) {
            model.value.files = tbs[2];
          } else {
            model.value.files = [];
          }
        }
      }
      swal.close();
      if (options.value.loading) options.value.loading = false;

      headerDialog.value = "Cập nhật lịch trực ban";
      displayDialog.value = true;
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      if (options.value.loading) options.value.loading = false;
      addLog({
        title: "Lỗi Console copyItem",
        controller: "calendarduty.vue",
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
const cancelItem = (item) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn hủy lịch trực ban này không!",
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
          ids = [item["calendar_duty_id"]];
        } else {
          if (selectedNodes.value.length > 0) {
            selectedNodes.value.forEach((row, i) => {
              ids.push(row["calendar_duty_id"]);
            });
          }
        }
        axios
          .delete(baseURL + "/api/calendar_duty/cancel_calendar", {
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
            toast.success("Hủy lịch trực ban thành công!");
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
                        url: "/calendar/detail/".concat(
                          item["calendar_duty_id"]
                        ),
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
              controller: "calendarduty.vue",
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
// Function
const changeCheckedAll = (checkedAll) => {
  if (datas.value != null && datas.value.length > 0) {
    datas.value
      .filter((x) => x["calendar_duty_id"] != null)
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
const goFile = (file) => {
  window.open(basedomainURL + file.file_path, "_blank");
};

//! Funtion send old
// const menuSends = ref();
// const itemSends = ref([
//   {
//     id: 0,
//     label: "Trình duyệt",
//     icon: "pi pi-send",
//   },
// ]);
// const toggleSend = (event) => {
//   menuSends.value.toggle(event);
// };
// const send = (str, type) => {
//   submitted.value = false;
//   var dutys = selectedNodes.value.map((x) => x["calendar_duty_id"]);
//   if (dutys.length === 0) {
//     swal.fire({
//       title: "Thông báo!",
//       text: "Vui lòng chọn lịch trực ban!",
//       icon: "error",
//       confirmButtonText: "OK",
//     });
//     return;
//   }
//   swal.fire({
//     width: 110,
//     didOpen: () => {
//       swal.showLoading();
//     },
//   });
//   let formData = new FormData();
//   formData.append("dutys", JSON.stringify(dutys));
//   axios
//     .put(baseURL + "/api/calendar_duty/send_duty", formData, config)
//     .then((response) => {
//       if (response.data.err === "1") {
//         swal.fire({
//           title: "Thông báo!",
//           text: response.data.ms,
//           icon: "error",
//           confirmButtonText: "OK",
//         });
//         return;
//       }
//       selectedNodes.value = [];
//       initData(true);
//       swal.close();
//       toast.success("Gửi thành công!");
//     })
//     .catch((error) => {
//       swal.close();
//       swal.fire({
//         title: "Thông báo!",
//         text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
//         icon: "error",
//         confirmButtonText: "OK",
//       });
//       return;
//     });
//   if (submitted.value) submitted.value = false;
// };

//!Function approve old
// const menuApproves = ref();
// const itemApproves = ref([
//   {
//     id: 2,
//     label: "Phê duyệt",
//     icon: "pi pi-send",
//   },
//   {
//     id: 3,
//     label: "Hủy",
//     icon: "pi pi-undo",
//   },
// ]);
// const toggleApproves = (event) => {
//   menuApproves.value.toggle(event);
// };
// const approve = (status) => {
//   submitted.value = false;
//   var dutys = selectedNodes.value.map((x) => x["calendar_duty_id"]);
//   if (dutys.length === 0) {
//     swal.fire({
//       title: "Thông báo!",
//       text: "Vui lòng chọn lịch trực ban!",
//       icon: "error",
//       confirmButtonText: "OK",
//     });
//     return;
//   }
//   swal.fire({
//     width: 110,
//     didOpen: () => {
//       swal.showLoading();
//     },
//   });
//   let formData = new FormData();
//   formData.append("status", status);
//   formData.append("dutys", JSON.stringify(dutys));
//   axios
//     .put(baseURL + "/api/calendar_duty/approve_duty", formData, config)
//     .then((response) => {
//       if (response.data.err === "1") {
//         swal.fire({
//           title: "Thông báo!",
//           text: response.data.ms,
//           icon: "error",
//           confirmButtonText: "OK",
//         });
//         return;
//       }
//       selectedNodes.value = [];
//       initData(true);
//       swal.close();
//       toast.success("Gửi thành công!");
//     })
//     .catch((error) => {
//       swal.close();
//       swal.fire({
//         title: "Thông báo!",
//         text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
//         icon: "error",
//         confirmButtonText: "OK",
//       });
//     });
//   if (submitted.value) submitted.value = false;
// };

//Function Eye
const menuEye = ref();
const itemEyes = ref([
  {
    id: 0,
    label: "Mặc định",
    icon: "",
  },
  {
    id: 1,
    label: "Mẫu trực ban cơ quan",
    icon: "",
  },
]);
const toggleEye = (event) => {
  menuEye.value.toggle(event);
};
const goEye = (eye) => {
  options.value.eye = eye;
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
    label: "Xuất Word",
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
  {
    label: "Xuất XML",
    icon: "pi pi-file-excel",
    command: (event) => {
      openDialogXML(event);
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
        excelname: "LỊCH TRỰC BAN CƠ QUAN",
        proc: "calendar_duty_listexport",
        par: [
          { par: "user_id", va: store.getters.user.user_id },
          { par: "search", va: options.value.search },
          { par: "week_start_date", va: options.value["week_start_date"] },
          { par: "week_end_date", va: options.value["week_end_date"] },
          { par: "is_type", va: options.value.is_type },
        ],
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
const exportWord = (method) => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  var htmltable = "";
  htmltable = renderhtml("formword", htmltable);
  axios
    .post(
      baseURL + "/api/calendar_duty/ExportDoc",
      {
        lib: "word",
        name:
          "lichtrucban_" +
          moment(options.week_start_date).format("DDMMYYYY") +
          "_" +
          moment(options.week_end_date).format("DDMMYYYY") +
          ".doc",
        html: htmltable,
        opition: {
          orientation: "Portrait",
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

//Function add week
const componentKey = ref(0);
const forceRerender = () => {
  componentKey.value += 1;
};
const headerDialogMultiple = ref();
const displayDialogMultiple = ref(false);
const openAddDialogMultiple = (str) => {
  forceRerender();
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
            proc: "calendar_duty_log_get2",
            par: [
              { par: "user_id", va: store.state.user.user_id },
              { par: "calendar_duty_id", va: item.calendar_duty_id },
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
          if (
            chartprocedures.value != null &&
            chartprocedures.value.length > 0
          ) {
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
        controller: "calendarduty.vue",
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

//Coincide
const coincide = ref([]);
const headerDialogLogCoincide = ref();
const displayDialogLogCoincide = ref(false);
const openDialogCoincide = (str, id) => {
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
            proc: "calendar_duty_coincide_get",
            par: [
              { par: "user_id", va: store.state.user.user_id },
              { par: "calendar_duty_id", va: id },
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
            coincide.value = tbs[0];
          } else {
            coincide.value = [];
          }
        }
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
        controller: "calendarduty.vue",
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

//Print
const print = () => {
  var htmltable = "";
  htmltable = renderhtml("formprint", htmltable);
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
      padding: 0.2rem !important;
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

// XML
const headerDialogXML = ref();
const displayDialogXML = ref(false);
const openDialogXML = () => {
  forceRerender();
  headerDialogXML.value = "Xuất file XML";
  displayDialogXML.value = true;
};
const closeDialogXML = () => {
  displayDialogXML.value = false;
};

//Init
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
            proc: "calendar_get_dictionary_duty2",
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
          if (months.value != null && months.value.length > 0) {
            months.value.forEach((m, i) => {
              m["is_current_month"] = false;
              if (m["month"] === options.value.month) {
                m["is_current_month"] = true;
              }
            });
          }
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
            var members = tbs[3];
            datatrucbans.value = members.filter((x) => x["is_type"] === 0);
            datachihuys.value = members.filter((x) => x["is_type"] === 1);
          } else {
            datatrucbans.value = [];
            datachihuys.value = [];
          }
          if (tbs[4] != null && tbs[4].length > 0) {
            mission.value = tbs[4][0];
          } else {
            mission.value = {};
          }
          if (tbs[5] != null && tbs[5].length > 0) {
            procedureforms.value = tbs[5];
          } else {
            procedureforms.value = [];
          }
          if (tbs[6] != null && tbs[6].length > 0) {
            signforms.value = tbs[6];
          } else {
            signforms.value = [];
          }
          if (tbs[7] != null && tbs[7].length > 0) {
            users.value = tbs[7];
          } else {
            users.value = [];
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
      initData(true);
    })
    .catch((error) => {
      swal.fire({
        title: "Thông báo!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
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
  axios
    .post(
      baseURL + "/api/calendar/get_datas",
      {
        str: encr(
          JSON.stringify({
            proc: "calendar_duty_list3",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "search", va: options.value.search },
              { par: "week_start_date", va: options.value["week_start_date"] },
              { par: "week_end_date", va: options.value["week_end_date"] },
              { par: "is_type", va: options.value.is_type },
              { par: "filterDate", va: filterDate.value },
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
          //convert data
          if (tbs[0] != null && tbs[0].length > 0) {
            tbs[0].forEach((item, i) => {
              if (item["members"] != null) {
                item["members"] = JSON.parse(item["members"]);

                item["trucbans"] = item["members"].filter(
                  (x) => x["is_type"] === "0"
                );
                item["chihuys"] = item["members"].filter(
                  (x) => x["is_type"] === "1"
                );
              }
              if (item["files"] != null) {
                item["files"] = JSON.parse(item["files"]);
              } else {
                item["files"];
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
          }
          if (tbs[0] != null && tbs[0].length > 0) {
            datas.value = tbs[0];
          } else {
            datas.value = [];
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
            placeholder=" Tìm kiếm lịch trực"
          />
        </span>
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
        <Button
          @click="toggleSend"
          icon="pi pi-check-circle"
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
          icon="pi pi-check-circle"
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
          @click="openAddDialog('Thêm mới lịch trực ban')"
          label="Thêm mới"
          icon="pi pi-plus"
          class="mr-2"
          v-if="props.state === 0"
        />
        <Button
          @click="openAddDialogMultiple('Thêm mới lịch trực ban')"
          label="Thêm mới theo tuần"
          icon="pi pi-plus"
          class="mr-2"
          v-if="props.state === 0"
        />
        <Button
          @click="toggleEye"
          icon="pi pi-eye"
          label="Kiểu hiển thị"
          class="mr-2"
          aria:haspopup="true"
          aria-controls="overlay_approves"
        />
        <Menu
          :model="itemEyes"
          :popup="true"
          id="overlay_approves"
          ref="menuEye"
        >
          <template #item="{ item }">
            <a
              :class="{ eyeactive: item.id === options.eye }"
              class="p-menuitem-link"
              role="menuitem"
              tabindex="0"
              @click="goEye(item.id)"
            >
              <span class="p-menuitem-icon" :class="item.icon"></span>
              <span class="p-menuitem-text">{{ item.label }}</span>
            </a>
          </template>
        </Menu>
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
      <div v-if="options.eye === 0">
        <DataTable
          @sort="onSort($event)"
          :value="datas"
          :totalRecords="options.total"
          :lazy="true"
          :rowHover="true"
          :showGridlines="true"
          v-model:selection="selectedNodes"
          dataKey="calendar_duty_id"
          scrollHeight="flex"
          filterDisplay="menu"
          filterMode="lenient"
          responsiveLayout="scroll"
          rowGroupMode="rowspan"
          groupRowsBy="day_string"
        >
          <Column
            field="day_string"
            header="Thứ/ngày"
            headerStyle="text-align:center;width:150px;height:50px;"
            bodyStyle="text-align:center;width:150px;"
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
              <div class="mx-2">
                <Checkbox
                  v-if="slotProps.data.calendar_duty_id != null"
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
                  <a
                    v-if="
                      slotProps.data.is_coincide && slotProps.data.status !== 2
                    "
                  >
                    <i
                      class="pi pi-exclamation-triangle"
                      v-tooltip.top="'Trùng lịch trực'"
                      style="color: red"
                    ></i>
                  </a>
                </div>
                <div>
                  <b>{{ slotProps.data.contents }}</b>
                </div>
              </div>
            </template>
          </Column>
          <!-- <Column
            field="is_timelot"
            header="Ca trực"
            headerStyle="text-align:center;width:80px;height:50px"
            bodyStyle="text-align:center;width:80px;"
            class="align-items-center justify-content-center text-center"
          >
            <template #body="slotProps">
              <div v-if="slotProps.data.calendar_duty_id != null">
                {{
                  slotProps.data.is_timelot === 0
                    ? "Sáng"
                    : slotProps.data.is_timelot === 1
                    ? "Chiều"
                    : "Cả ngày"
                }}
              </div>
            </template>
          </Column> -->
          <Column
            field="trucbans"
            header="Trực ban"
            headerStyle="text-align:center;width:100px;height:50px"
            bodyStyle="text-align:center;width:100px;"
            class="align-items-center justify-content-center text-center"
          >
            <template #body="slotProps">
              <div class="flex justify-content-center">
                <AvatarGroup
                  v-if="
                    slotProps.data.trucbans &&
                    slotProps.data.trucbans.length > 0
                  "
                >
                  <Avatar
                    v-for="(item, index) in slotProps.data.trucbans.slice(0, 3)"
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
                      slotProps.data.trucbans &&
                      slotProps.data.trucbans.length > 3
                    "
                    v-bind:label="
                      '+' + (slotProps.data.trucbans.length - 3).toString()
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
            field="chihuys"
            header="Chỉ huy"
            headerStyle="text-align:center;width:100px;height:50px"
            bodyStyle="text-align:center;width:100px;"
            class="align-items-center justify-content-center text-center"
          >
            <template #body="slotProps">
              <div class="flex justify-content-center">
                <AvatarGroup
                  v-if="
                    slotProps.data.chihuys && slotProps.data.chihuys.length > 0
                  "
                >
                  <Avatar
                    v-for="(item, index) in slotProps.data.chihuys.slice(0, 3)"
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
                      slotProps.data.chihuys &&
                      slotProps.data.chihuys.length > 3
                    "
                    v-bind:label="
                      '+' + (slotProps.data.chihuys.length - 3).toString()
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
            field="boardroom_name"
            header="Địa điểm"
            headerStyle="text-align:center;width:180px;height:50px"
            bodyStyle="text-align:center;width:180px;"
            class="align-items-center justify-content-center text-center"
          >
          </Column>
          <Column
            field="note"
            header="Ghi chú"
            headerStyle="text-align:center;width:150px;height:50px"
            bodyStyle="text-align:center;width:150px;"
            class="align-items-center justify-content-center text-center"
          >
            <template #body="slotProps">
              <div class="mb-2">{{ slotProps.data.note }}</div>
              <div
                v-if="
                  slotProps.data.files != null &&
                  slotProps.data.files.length > 0
                "
              >
                <i
                  class="pi pi-paperclip"
                  style="cursor: pointer"
                  @click="goFile(slotProps.data.files[0])"
                ></i>
              </div>
            </template>
          </Column>
          <Column
            field="status_name"
            header="Trạng thái"
            headerStyle="text-align:center;width:120px;height:50px"
            bodyStyle="text-align:center;width:120px;"
            class="align-items-center justify-content-center text-center"
          >
            <template #body="slotProps">
              <div
                class="format-flex-center"
                v-if="slotProps.data.calendar_duty_id != null"
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
            headerStyle="text-align:center;width:150px;height:50px"
            bodyStyle="text-align:center;max-width:150px;"
            class="align-items-center justify-content-center text-center"
          >
            <template #body="slotProps">
              <div>
                <Button
                  v-if="slotProps.data.is_copy"
                  @click="copyItem(slotProps.data)"
                  class="
                    p-button-rounded p-button-secondary p-button-outlined
                    mx-1
                    mb-2
                  "
                  type="button"
                  icon="pi pi-copy"
                  v-tooltip.top="'Nhân bản'"
                ></Button>
                <Button
                  v-if="slotProps.data.calendar_duty_id != null"
                  @click="logItem(slotProps.data)"
                  class="
                    p-button-rounded p-button-secondary p-button-outlined
                    mx-1
                    mb-2
                  "
                  type="button"
                  icon="pi pi-chart-bar"
                  v-tooltip.top="'Quy trình xử lý'"
                ></Button>
                <Button
                  v-if="slotProps.data.is_edit"
                  @click="editItem(slotProps.data)"
                  class="
                    p-button-rounded p-button-secondary p-button-outlined
                    mx-1
                    mb-2
                  "
                  type="button"
                  icon="pi pi-pencil"
                  v-tooltip.top="'Sửa'"
                ></Button>
                <Button
                  v-if="slotProps.data.is_cancel"
                  @click="cancelItem(slotProps.data)"
                  class="
                    p-button-rounded p-button-secondary p-button-outlined
                    mx-1
                    mb-2
                  "
                  type="button"
                  icon="pi pi-times"
                  v-tooltip.top="'Hủy lịch'"
                ></Button>
                <Button
                  v-if="slotProps.data.is_delete"
                  @click="deleteItem(slotProps.data)"
                  class="
                    p-button-rounded p-button-secondary p-button-outlined
                    mx-1
                    mb-2
                  "
                  type="button"
                  v-tooltip.top="'Xóa'"
                  icon="pi pi-trash"
                ></Button>
              </div>
            </template>
          </Column>
          <template #empty>
            <div
              class="
                align-items-center
                justify-content-center
                p-4
                text-center
                m-auto
              "
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
      <div v-if="options.eye === 1">
        <DataTable
          @sort="onSort($event)"
          :value="datas"
          :totalRecords="options.total"
          :lazy="true"
          :rowHover="true"
          :showGridlines="true"
          v-model:selection="selectedNodes"
          dataKey="calendar_duty_id"
          scrollHeight="flex"
          filterDisplay="menu"
          filterMode="lenient"
          responsiveLayout="scroll"
          rowGroupMode="rowspan"
          groupRowsBy="day_string"
        >
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
              <div class="mx-2">
                <Checkbox
                  v-if="slotProps.data.calendar_duty_id != null"
                  v-model="slotProps.data.is_check"
                  :binary="true"
                  @change="changeChecked(slotProps.data)"
                />
              </div>
            </template>
          </Column>
          <Column
            field="trucbans"
            header="Họ và tên"
            headerStyle="text-align:center;width:180px;height:50px"
            bodyStyle="text-align:center;width:180px;max-height:60px"
            class="align-items-center justify-content-center text-center"
          >
            <template #body="slotProps">
              <div>
                <a
                  v-if="
                    slotProps.data.is_coincide && slotProps.data.status !== 2
                  "
                >
                  <i
                    class="pi pi-exclamation-triangle"
                    v-tooltip.top="'Trùng lịch trực'"
                    style="color: red"
                  ></i>
                </a>
              </div>
              <div
                v-if="
                  slotProps.data.trucbans && slotProps.data.trucbans.length > 0
                "
              >
                {{ slotProps.data.trucbans[0].full_name }}
              </div>
            </template>
          </Column>
          <Column
            field="trucbans"
            header="Cấp bậc, chức vụ"
            headerStyle="height:50px;max-width:auto;min-width:150px;"
            bodyStyle="max-height:60px;"
            class="align-items-center justify-content-center text-center"
          >
            <template #body="slotProps">
              <div
                v-if="
                  slotProps.data.trucbans && slotProps.data.trucbans.length > 0
                "
              >
                {{ slotProps.data.trucbans[0].position_name }}
              </div>
            </template>
          </Column>
          <!-- <Column
            field="is_timelot"
            header="Ca trực"
            headerStyle="text-align:center;width:80px;height:50px"
            bodyStyle="text-align:center;width:80px;"
            class="align-items-center justify-content-center text-center"
          >
            <template #body="slotProps">
              <div v-if="slotProps.data.calendar_duty_id != null">
                {{
                  slotProps.data.is_timelot === 0
                    ? "Sáng"
                    : slotProps.data.is_timelot === 1
                    ? "Chiều"
                    : "Cả ngày"
                }}
              </div>
            </template>
          </Column> -->
          <Column
            field="day_string"
            header="Thời gian"
            headerStyle="text-align:center;width:100px;height:50px;"
            bodyStyle="text-align:center;width:100px;max-height:60px"
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
                <span>{{ slotProps.data.day_string }}</span>
              </div>
            </template>
          </Column>
          <Column
            field="day_string"
            header="Thứ"
            headerStyle="text-align:center;width:100px;height:50px;"
            bodyStyle="text-align:center;width:100px;max-height:60px"
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
                <span>{{ slotProps.data.day_name }}</span>
              </div>
            </template>
          </Column>
          <Column
            field="chihuys"
            header="Trực chỉ huy"
            headerStyle="text-align:center;width:180px;height:50px"
            bodyStyle="text-align:center;width:180px;max-height:60px"
            class="align-items-center justify-content-center text-center"
          >
            <template #body="slotProps">
              <div
                v-if="
                  slotProps.data.chihuys && slotProps.data.chihuys.length > 0
                "
              >
                {{ slotProps.data.chihuys[0].full_name }}
              </div>
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
                v-if="slotProps.data.calendar_duty_id != null"
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
                  class="
                    p-button-rounded p-button-secondary p-button-outlined
                    mx-1
                    mb-2
                  "
                  type="button"
                  icon="pi pi-copy"
                  v-tooltip.top="'Nhân bản'"
                ></Button>
                <Button
                  v-if="slotProps.data.calendar_duty_id != null"
                  @click="logItem(slotProps.data)"
                  class="
                    p-button-rounded p-button-secondary p-button-outlined
                    mx-1
                    mb-2
                  "
                  type="button"
                  icon="pi pi-chart-bar"
                  v-tooltip.top="'Quy trình xử lý'"
                ></Button>
                <Button
                  v-if="slotProps.data.is_edit"
                  @click="editItem(slotProps.data)"
                  class="
                    p-button-rounded p-button-secondary p-button-outlined
                    mx-1
                    mb-2
                  "
                  type="button"
                  icon="pi pi-pencil"
                  v-tooltip.top="'Sửa'"
                ></Button>
                <Button
                  v-if="slotProps.data.is_cancel"
                  @click="cancelItem(slotProps.data)"
                  class="
                    p-button-rounded p-button-secondary p-button-outlined
                    mx-1
                    mb-2
                  "
                  type="button"
                  icon="pi pi-times"
                  v-tooltip.top="'Hủy lịch'"
                ></Button>
                <Button
                  v-if="slotProps.data.is_delete"
                  @click="deleteItem(slotProps.data)"
                  class="
                    p-button-rounded p-button-secondary p-button-outlined
                    mx-1
                    mb-2
                  "
                  type="button"
                  v-tooltip.top="'Xóa'"
                  icon="pi pi-trash"
                ></Button>
              </div>
            </template>
          </Column>
          <template #empty>
            <div
              class="
                align-items-center
                justify-content-center
                p-4
                text-center
                m-auto
              "
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
    </div>

    <iframe name="printframe" id="printframe" style="display: none"></iframe>
  </div>

  <!--Model-->
  <dialogmodelduty
    :temp="false"
    :headerDialog="headerDialog"
    :displayDialog="displayDialog"
    :closeDialog="closeDialog"
    :isAdd="isAdd"
    :submitted="submitted"
    :model="model"
    :files="files"
    :selectFile="selectFile"
    :removeFile="removeFile"
    :initData="initData"
    :databoardrooms="boardrooms"
    :datatrucbans="datatrucbans"
    :datachihuys="datachihuys"
  />

  <!--Multiple model-->
  <dialogmodelmultipleduty
    :key="componentKey"
    :headerDialog="headerDialogMultiple"
    :displayDialog="displayDialogMultiple"
    :closeDialog="closeDialogMultiple"
    :eye="options.eye"
    :currentweek="currentweek"
    :weeks="weeks"
    :databoardrooms="boardrooms"
    :datatrucbans="datatrucbans"
    :datachihuys="datachihuys"
    :initData="initData"
  />

  <!--Send-->
  <dialogdutysend
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
  <dialogdutyapprove
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
  <dialogdutyenact
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

  <!--XML-->
  <dialogxml
    :key="componentKey"
    :headerDialog="headerDialogXML"
    :displayDialog="displayDialogXML"
    :closeDialog="closeDialogXML"
  />

  <frameprintduty
    :datas="datas"
    :mission="mission"
    :week_start_date="options.week_start_date"
    :week_end_date="options.week_end_date"
  />

  <framewordduty
    :datas="datas"
    :mission="mission"
    :week_start_date="options.week_start_date"
    :week_end_date="options.week_end_date"
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
