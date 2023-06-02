<script setup>
import { ref, inject, onMounted } from "vue";
import { encr } from "../../../util/function";
import detailedwork from "../../../components/task_origin/DetailedWork.vue";
import DocStatus from "../../../components/doc/DocStatus.vue";
import dialogcutrice from "./dialogcutrice.vue";
import vi from "date-fns/locale/vi";
import moment from "moment";
import { useToast } from "vue-toastification";
import { useCookies } from "vue3-cookies";
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
const { cookies } = useCookies();
const PositionSideBar = ref("right");
emitter.on("psb", (obj) => {
  PositionSideBar.value = obj;
});
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
  view: 2,
  week_start_date: null,
  week_end_date: null,
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

//Declare
const holiday = ref({});
const duty_sunday = ref({});
const holidays = ref([]);
const databookings = ref([]);
const datanews = ref([]);
const datanotifys = ref([]);
const datavideos = ref([]);
const datatasks = ref([]);
const datadocs = ref([]);
const datalideshows = ref([]);
const datatodaybirthdays = ref([]);
const databirthdays = ref([]);
const dataphonebooks = ref([]);
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
const datacalendars = ref([]);
const datacalendardutys = ref([]);
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
      break;
    default:
      break;
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
  interval,
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

//Function
const counts = ref({
  countcalendar: 0,
  countdoc: 0,
  counttask: 0,
  countbooking: 0,
  countmessage: 0,
  countlaw: 0,
});
const countcvs = ref({
  labels: [" Được giao ", " Quản lý ", " Theo dõi "],
  datasets: [
    {
      data: [],
      backgroundColor: ["#ff8b4e", "#d87777", "#f17ac7"],
      hoverBackgroundColor: ["#ff8b4e", "#d87777", "#f17ac7"],
    },
  ],
});
const countvbs = ref({
  labels: [" Đến ", " Đi ", " Nội bộ "],
  datasets: [
    {
      data: [],
      backgroundColor: ["#FF6384", "#36A2EB", "#FFCE56"],
      hoverBackgroundColor: ["#FF6384", "#36A2EB", "#FFCE56"],
    },
  ],
});
const lightOptions = ref({
  plugins: {
    legend: {
      display: false,
      labels: {
        color: "#495057",
      },
    },
  },
});
const goRouter = (name, params) => {
  if (name != null) {
    router.push({ name: name, params: params || {} });
  }
};
const removeVietnameseTones = (str) => {
  str = str.replace(/à|á|ạ|ả|ã|â|ầ|ấ|ậ|ẩ|ẫ|ă|ằ|ắ|ặ|ẳ|ẵ/g, "a");
  str = str.replace(/è|é|ẹ|ẻ|ẽ|ê|ề|ế|ệ|ể|ễ/g, "e");
  str = str.replace(/ì|í|ị|ỉ|ĩ/g, "i");
  str = str.replace(/ò|ó|ọ|ỏ|õ|ô|ồ|ố|ộ|ổ|ỗ|ơ|ờ|ớ|ợ|ở|ỡ/g, "o");
  str = str.replace(/ù|ú|ụ|ủ|ũ|ư|ừ|ứ|ự|ử|ữ/g, "u");
  str = str.replace(/ỳ|ý|ỵ|ỷ|ỹ/g, "y");
  str = str.replace(/đ/g, "d");
  str = str.replace(/À|Á|Ạ|Ả|Ã|Â|Ầ|Ấ|Ậ|Ẩ|Ẫ|Ă|Ằ|Ắ|Ặ|Ẳ|Ẵ/g, "A");
  str = str.replace(/È|É|Ẹ|Ẻ|Ẽ|Ê|Ề|Ế|Ệ|Ể|Ễ/g, "E");
  str = str.replace(/Ì|Í|Ị|Ỉ|Ĩ/g, "I");
  str = str.replace(/Ò|Ó|Ọ|Ỏ|Õ|Ô|Ồ|Ố|Ộ|Ổ|Ỗ|Ơ|Ờ|Ớ|Ợ|Ở|Ỡ/g, "O");
  str = str.replace(/Ù|Ú|Ụ|Ủ|Ũ|Ư|Ừ|Ứ|Ự|Ử|Ữ/g, "U");
  str = str.replace(/Ỳ|Ý|Ỵ|Ỷ|Ỹ/g, "Y");
  str = str.replace(/Đ/g, "D");
  // Some system encode vietnamese combining accent as individual utf-8 characters
  // Một vài bộ encode coi các dấu mũ, dấu chữ như một kí tự riêng biệt nên thêm hai dòng này
  str = str.replace(/\u0300|\u0301|\u0303|\u0309|\u0323/g, ""); // ̀ ́ ̃ ̉ ̣  huyền, sắc, ngã, hỏi, nặng
  str = str.replace(/\u02C6|\u0306|\u031B/g, ""); // ˆ ̆ ̛  Â, Ê, Ă, Ơ, Ư
  // Remove extra spaces
  // Bỏ các khoảng trắng liền nhau
  str = str.replace(/ + /g, " ");
  str = str.trim();
  // Remove punctuations
  // Bỏ dấu câu, kí tự đặc biệt
  str = str.replace(
    /!|@|%|\^|\*|\(|\)|\+|\=|\<|\>|\?|\/|,|\.|\:|\;|\'|\"|\&|\#|\[|\]|~|\$|_|`|-|{|}|\||\\/g,
    " ",
  );
  return str;
};
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

//Function task
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
  { value: 5, text: "Đợi review", bg_color: "#33c9dc", text_color: "#FFFFFF" },
  { value: 6, text: "Bị trả lại", bg_color: "#ffa500", text_color: "#FFFFFF" },
  { value: 7, text: "HT sau hạn", bg_color: "#ff8b4e", text_color: "#FFFFFF" },
  { value: 8, text: "Đã review", bg_color: "#51b7ae", text_color: "#FFFFFF" },
  { value: -1, text: "Bị xóa", bg_color: "red", text_color: "#FFFFFF" },
]);
const showDetail = ref(false);
const selectedTaskID = ref();
const onRowSelect = (id) => {
  showDetail.value = false;
  showDetail.value = true;
  selectedTaskID.value = id;
};
const closeDetail = () => {
  showDetail.value = false;
  selectedTaskID.value = null;
  initData();
};
//Cắt cơm
const componentKey = ref(0);
const forceRerender = () => {
  componentKey.value += 1;
};
const headerDialogCutRice = ref();
const displayDialogCutRice = ref(false);
const openDialogCutRice = (str) => {
  forceRerender();
  headerDialogCutRice.value = str;
  displayDialogCutRice.value = true;
};
const closeDialogCutRice = () => {
  displayDialogCutRice.value = false;
};

//Init
const initDictionaryCutRice = () => {
  axios
    .get(baseURL + "/api/BookingMeal/GetConfig", {
      headers: { Authorization: `Bearer ${store.getters.token}` },
    })
    .then((response) => {
      if (response != null && response.data != null) {
        var data = response.data.data;
        if (data != null) {
          holidays.value = data.working_days;
        }
      }
    })
    .then(() => {
      initCutRice(true);
    })
    .catch((error) => {
      console.log(error);
    });
};
function CreateGuid() {
  function _p8(s) {
    var p = (Math.random().toString(16) + "000000000").substr(2, 8);
    return s ? "-" + p.substr(0, 4) + "-" + p.substr(4, 4) : p;
  }
  return _p8() + _p8(true) + _p8(true) + _p8();
}
const initCutRice = (rf) => {
  databookings.value = [];
  axios
    .post(
      baseURL + "/api/calendar/get_datas",
      {
        str: encr(
          JSON.stringify({
            proc: "dashboard_check_cutrice",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "search", va: "" },
            ],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      if (response != null && response.data != null) {
        var data = response.data.data;
        if (data != null) {
          let tbn = JSON.parse(data);
          let userbooks = [];
          if (tbn[1] != null && tbn[1].length > 0) {
            userbooks = tbn[1];
          }
          if (tbn[0] != null && tbn[0].length > 0) {
            tbn[0].forEach((item) => {
              if (item["members"] != null) {
                item["members"] = JSON.parse(item["members"]);

                item["chutris"] = item["members"].filter(
                  (x) => x["is_type"] === "0",
                );
                item["thamgias"] = item["members"].filter(
                  (x) => x["is_type"] === "1",
                );
              }
              var start_date_copy = new Date(item["start_date"]);
              var end_date_copy = new Date(item["end_date"]);
              var start_date_new = new Date(
                start_date_copy.getFullYear(),
                start_date_copy.getMonth(),
                start_date_copy.getDate(),
              );
              var end_date_new = new Date(
                end_date_copy.getFullYear(),
                end_date_copy.getMonth(),
                end_date_copy.getDate(),
              );
              if ((start_date_new = end_date_new)) {
                let obj = { ...item };
                obj.day = start_date_copy;
                obj.day_name = getDayDate(start_date_copy);
                obj.day_string = moment(start_date_copy).format("DD/MM/YYYY");
                obj.is_holiday = start_date_copy.getDay() == 0;
                databookings.value.push(obj);
              } else {
                let dateinweeks = bindDateBetweenFirstAndLast(
                  new Date(item["start_date"]),
                  new Date(item["end_date"]),
                );
                dateinweeks.forEach((day, i) => {
                  let obj = { ...item };
                  obj.day = day;
                  obj.day_name = getDayDate(day);
                  obj.day_string = moment(day).format("DD/MM/YYYY");
                  obj.is_holiday = day.getDay() == 0;
                  databookings.value.push(obj);
                });
              }
            });
            databookings.value = databookings.value.sort(function (a, b) {
              return new Date(a["day"]) - new Date(b["day"]);
            });
            if (databookings.value != null && databookings.value.length > 0) {
              databookings.value.forEach((item) => {
                item["calendar_id"] = CreateGuid();
              });
              let skip = databookings.value.filter(
                (a) =>
                  holidays.value.filter(
                    (b) =>
                      moment(b).format("DD/MM/YYYY") ===
                      moment(a["day"]).format("DD/MM/YYYY"),
                  ).length > 0 ||
                  userbooks.filter(
                    (b) =>
                      moment(b["booking_date"]).format("DD/MM/YYYY") ===
                      moment(a["day"]).format("DD/MM/YYYY"),
                  ).length > 0,
              );
              if (skip != null && skip.length > 0) {
                skip.forEach((item) => {
                  var idx = databookings.value.findIndex(
                    (x) => x["calendar_id"] === item["calendar_id"],
                  );
                  if (idx != -1) {
                    databookings.value.splice(idx, 1);
                  }
                });
              }
            }
          }
        }
      }
    })
    .catch((error) => {
      console.log(error);
    });
};
const initCounts = () => {
  axios
    .post(
      baseURL + "/api/calendar/get_datas",
      {
        str: encr(
          JSON.stringify({
            proc: "dashboard_counts",
            par: [{ par: "user_id", va: store.getters.user.user_id }],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      if (response != null && response.data != null) {
        var data = response.data.data;
        if (data != null) {
          let tbn = JSON.parse(data);
          if (tbn[0] != null && tbn[0].length > 0) {
            for (const key in tbn[0][0]) {
              var arw = tbn[0][0];
              if (Object.hasOwnProperty.call(arw, key)) {
                counts.value[key] = arw[key];
              }
            }
          }
        }
      }
    })
    .catch((error) => {
      console.log(error);
    });
};
const initCountCongviec = () => {
  axios
    .post(
      baseURL + "/api/calendar/get_datas",
      {
        str: encr(
          JSON.stringify({
            proc: "task_origin_count",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "search", va: "" },
              { par: "loc", va: 0 },
              { par: "sdate", va: null },
              { par: "edate", va: null },
            ],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      if (response != null && response.data != null) {
        var data = response.data.data;
        if (data != null) {
          let tbn = JSON.parse(data);
          if (tbn[0] != null && tbn[0].length > 0) {
            var arr = [];
            arr.push(tbn[0][0].total_toilam);
            arr.push(tbn[0][0].total_quanly);
            arr.push(tbn[0][0].total_theodoi);
            setTimeout(() => {
              countcvs.value.datasets[0].data = arr;
            }, 1000);
          }
        }
      }
    })
    .catch((error) => {
      console.log(error);
    });
};
const initCongviec = () => {
  axios
    .post(
      baseURL + "/api/calendar/get_datas",
      {
        str: encr(
          JSON.stringify({
            proc: "dashboard_task_list",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "top", va: 20 },
            ],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      if (response != null && response.data != null) {
        var data = response.data.data;
        if (data != null) {
          let tbn = JSON.parse(data);
          if (tbn[0] != null && tbn[0].length > 0) {
            tbn[0].forEach((item) => {
              if (item["start_date"] != null) {
                item["start_date"] = moment(
                  new Date(item["start_date"]),
                ).format("DD/MM/YYYY");
              }
              if (item["end_date"] != null) {
                item["end_date"] = moment(new Date(item["end_date"])).format(
                  "DD/MM/YYYY",
                );
              }
              item.status_name =
                item.status != null
                  ? listDropdownStatus.value.filter(
                      (x) => x.value == item.status,
                    )[0].text
                  : "";
              item.status_bg_color =
                item.status != null
                  ? listDropdownStatus.value.filter(
                      (x) => x.value == item.status,
                    )[0].bg_color
                  : "";
              item.status_text_color =
                item.status != null
                  ? listDropdownStatus.value.filter(
                      (x) => x.value == item.status,
                    )[0].text_color
                  : "";
              //thời gian xử lý
              if (item.end_date != null) {
                if (item.thoigianquahan < 0) {
                  if (item.thoigianxuly > 0) {
                    item.title_time = item.thoigianxuly + " ngày";
                    item.time_bg = item.status_bg_color;
                    item.time_color = "color: #fff;";
                  }
                } else {
                  if (item.thoigianquahan > 0) {
                    item.title_time =
                      "Quá hạn " + item.thoigianquahan + " ngày";
                    item.time_bg = "red";
                    item.time_color = "color: #fff;";
                  }
                }
              } else if (item.thoigianxuly) {
                item.title_time = item.thoigianxuly + " ngày";
                item.time_bg = item.status_bg_color;
                item.time_color = "color: #fff;";
              }
            });
            datatasks.value = tbn[0];
          }
        }
      }
    })
    .catch((error) => {
      console.log(error);
    });
};
const initCountVanban = () => {
  axios
    .post(
      baseURL + "/api/calendar/get_datas",
      {
        str: encr(
          JSON.stringify({
            proc: "doc_master_receive_count",
            par: [{ par: "user_key", va: store.getters.user.user_key }],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      if (response != null && response.data != null) {
        var data = response.data.data;
        if (data != null) {
          let tbn = JSON.parse(data);
          if (tbn[0] != null && tbn[0].length > 0) {
            var arr = [];
            arr.push(tbn[0][0]["receive"]);
            arr.push(tbn[0][0]["send"]);
            arr.push(tbn[0][0]["internal"]);

            setTimeout(() => {
              countvbs.value.datasets[0].data = arr;
            }, 1000);
          }
        }
      }
    })
    .catch((error) => {
      console.log(error);
    });
};
const initVanban = () => {
  axios
    .post(
      baseURL + "/api/calendar/get_datas",
      {
        str: encr(
          JSON.stringify({
            proc: "doc_master_dashboard_list",
            par: [
              {
                par: "organization_id",
                va: store.getters.user.organization_id,
              },
              { par: "user_key", va: store.getters.user.user_key },
              { par: "top", va: 20 },
            ],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      if (response != null && response.data != null) {
        var data = response.data.data;
        if (data != null) {
          let tbn = JSON.parse(data);
          if (tbn[0] != null && tbn[0].length > 0) {
            tbn[0].forEach((item) => {
              if (item["doc_date"] != null) {
                item["doc_date"] = moment(new Date(item["doc_date"])).format(
                  "DD/MM/YYYY",
                );
              }
            });
            datadocs.value = tbn[0];
          }
        }
      }
    })
    .catch((error) => {
      console.log(error);
    });
};
const initNew = () => {
  axios
    .post(
      baseURL + "/api/calendar/get_datas",
      {
        str: encr(
          JSON.stringify({
            proc: "news_main_list",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "category_id", va: null },
              { par: "news_type", va: 0 },
              { par: "key_words", va: null },
              { par: "search", va: null },
              { par: "status", va: 2 },
              { par: "is_hot", va: null },
              { par: "is_notify", va: null },
              { par: "datefilter", va: 0 },
              { par: "pageno", va: 0 },
              { par: "pagesize", va: 12 },
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
      if (response != null && response.data != null) {
        var data = response.data.data;
        if (data != null) {
          let tbn = JSON.parse(data);
          if (tbn[0] != null && tbn[0].length > 0) {
            tbn[0].forEach((item, i) => {
              if (item["approved_date"] != null) {
                item["approved_date"] = moment(
                  new Date(item["approved_date"]),
                ).format("HH:mm DD/MM/YYYY");
              }
            });
            datanews.value = tbn[0];
          }
        }
      }
    })
    .catch((error) => {
      console.log(error);
    });
};
const initNotify = () => {
  axios
    .post(
      baseURL + "/api/calendar/get_datas",
      {
        str: encr(
          JSON.stringify({
            proc: "news_main_list",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "category_id", va: null },
              { par: "news_type", va: 1 },
              { par: "key_words", va: null },
              { par: "search", va: null },
              { par: "status", va: 2 },
              { par: "is_hot", va: null },
              { par: "is_notify", va: null },
              { par: "datefilter", va: 0 },
              { par: "pageno", va: 0 },
              { par: "pagesize", va: 9 },
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
      if (response != null && response.data != null) {
        var data = response.data.data;
        if (data != null) {
          let tbn = JSON.parse(data);
          if (tbn[0] != null && tbn[0].length > 0) {
            tbn[0].forEach((element, i) => {
              let arrI = [];
              if (element.url_file != "" && element.url_file != null) {
                element.url_file.split(",").forEach((item) => {
                  if (item != "" && item != null) arrI.push(item);
                });
                element.url_file = arrI;
              }
              if (element["approved_date"] != null) {
                element["approved_date"] = moment(
                  new Date(element["approved_date"]),
                ).format("HH:mm DD/MM/YYYY");
              }
            });
            datanotifys.value = tbn[0];
          }
        }
      }
    })
    .catch((error) => {
      console.log(error);
    });
};
const initVideo = () => {
  axios
    .post(
      baseURL + "/api/calendar/get_datas",
      {
        str: encr(
          JSON.stringify({
            proc: "dashboard_video_list",
            par: [{ par: "user_id", va: store.getters.user.user_id }],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      if (response != null && response.data != null) {
        var data = response.data.data;
        if (data != null) {
          let tbn = JSON.parse(data);
          if (tbn[0] != null && tbn[0].length > 0) {
            tbn[0].forEach(function (item, i) {
              item["created_date"] = new Date(item["created_date"]);
            });
            datavideos.value = tbn[0];
          }
        }
      }
    })
    .catch((error) => {
      console.log(error);
    });
};
const initLideshow = () => {
  axios
    .post(
      baseURL + "/api/calendar/get_datas",
      {
        str: encr(
          JSON.stringify({
            proc: "shows_main_list",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "key_words", va: null },
              { par: "search", va: null },
              { par: "status", va: 2 },
              { par: "datefilter", va: 3 },
              { par: "pageno", va: 0 },
              { par: "pagesize", va: 3 },
            ],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      if (response != null && response.data != null) {
        var data = response.data.data;
        if (data != null) {
          let tbn = JSON.parse(data);
          if (tbn[0] != null && tbn[0].length > 0) {
            tbn[0].forEach((item, i) => {
              item["created_date"] = moment(
                new Date(item["created_date"]),
              ).format("HH:mm DD/MM/YYYY");
            });
            datalideshows.value = tbn[0];
          }
        }
      }
    })
    .catch((error) => {
      console.log(error);
    });
};
const initBirthday = () => {
  axios
    .post(
      baseURL + "/api/calendar/get_datas",
      {
        str: encr(
          JSON.stringify({
            proc: "dashboard_birthday",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "myDate", va: new Date() },
            ],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      if (response != null && response.data != null) {
        var data = response.data.data;
        if (data != null) {
          let tbn = JSON.parse(data);
          if (tbn[0] != null && tbn[0].length > 0) {
            tbn[0].forEach((item, i) => {
              if (item["birthday"] != null) {
                item["birthday"] = moment(new Date(item["birthday"])).format(
                  "DD/MM/YYYY",
                );
              }
            });
            datatodaybirthdays.value = tbn[0];
          }
          if (tbn[1] != null && tbn[1].length > 0) {
            tbn[1].forEach((item, i) => {
              if (item["birthday"] != null) {
                item["birthday"] = moment(new Date(item["birthday"])).format(
                  "DD/MM/YYYY",
                );
              }
            });
            databirthdays.value = tbn[1];
          }
          if (tbn[3] != null && tbn[3].length > 0) {
            tbn[3].forEach((item, i) => {
              if (item["birthday"] != null) {
                item["birthday"] = moment(new Date(item["birthday"])).format(
                  "DD/MM/YYYY",
                );
              }
            });
            dataphonebooks.value = tbn[3];
          }
        }
      }
    })
    .catch((error) => {
      console.log(error);
    });
};
const initDictionaryCalendarDuty = () => {
  axios
    .post(
      baseURL + "/api/calendar/get_datas",
      {
        str: encr(
          JSON.stringify({
            proc: "calendar_get_dictionary_duty",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "year", va: options.value.year },
            ],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
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
              (x) => x["is_current_week"] === true,
            );
          } else {
            currentweek.value = weeks.value.find(
              (x) => x["week_no"] === options.value.week || 0,
            );
          }
          if (currentweek.value != null) {
            options.value["week"] = currentweek.value["week_no"];
            switch (options.value.view) {
              case 1:
                options.value["week_start_date"] = new Date(
                  currentweek.value["week_start_date"],
                );
                options.value["week_end_date"] = new Date(
                  currentweek.value["week_end_date"],
                );
                break;
              case 2:
                options.value["week_start_date"] = new Date(
                  options.value.year,
                  options.value.month - 1,
                  1,
                );
                options.value["week_end_date"] = new Date(
                  options.value.year,
                  options.value.month,
                  0,
                );
                break;
              case 3:
                options.value["week_start_date"] = new Date(
                  options.value["year"],
                  0,
                  1,
                );
                options.value["week_end_date"] = new Date(
                  options.value["year"],
                  11,
                  31,
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
        }
      }
    })
    .then(() => {
      initDutySunday();
      initCalendar(true);
      //initCalendarDuty(true);
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
const initCalendar = () => {
  datacalendars.value = [];
  datacalendardutys.value = [];
  axios
    .post(
      baseURL + "/api/calendar/get_datas",
      {
        str: encr(
          JSON.stringify({
            proc: "calendar_tivi_get",
            par: [
              { par: "week_start_date", va: options.value["week_start_date"] },
              { par: "week_end_date", va: options.value["week_end_date"] },
            ],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      if (response != null && response.data != null) {
        var data = response.data.data;
        if (data != null) {
          let tbs = JSON.parse(data);
          //convert data
          if (tbs[1] != null && tbs[1].length > 0) {
            tbs[1].forEach((item, i) => {
              if (item["contents"] != null) {
                item["contents"] = item["contents"].replaceAll("\n", "<br/>");
              }
              if (item["members"] != null) {
                item["members"] = JSON.parse(item["members"]);

                item["chutris"] = item["members"].filter(
                  (x) => x["is_type"] === "0",
                );
                item["thamgias"] = item["members"].filter(
                  (x) => x["is_type"] === "1",
                );
              }
              item["is_holiday"] = new Date(item["day"]).getDay() == 0;
            });
          }
          let dateinweeks1 = bindDateBetweenFirstAndLast(
            new Date(currentweek.value["week_start_date"]),
            new Date(currentweek.value["week_end_date"]),
          );
          dateinweeks1
            .filter(
              (a) =>
                tbs[1].findIndex(
                  (b) => b["day_string"] === moment(a).format("DD/MM/YYYY"),
                ) === -1,
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
          if (tbs[1] != null && tbs[1].length > 0) {
            datacalendars.value = tbs[1];
            var idx = datacalendars.value.findIndex((x) => x.is_holiday);
            if (idx != -1) {
              holiday.value = datacalendars.value[idx];
            }
          } else {
            datacalendars.value = [];
          }

          //trực ban
          if (tbs[2] != null && tbs[2].length > 0) {
            tbs[2].forEach((item, i) => {
              if (item["members"] != null) {
                item["members"] = JSON.parse(item["members"]);
                item["trucbans"] = item["members"].filter(
                  (x) => x["is_type"] === "0",
                );
                item["chihuys"] = item["members"].filter(
                  (x) => x["is_type"] === "1",
                );
              }
              if (item["files"] != null) {
                item["files"] = JSON.parse(item["files"]);
              } else {
                item["files"];
              }
              item["is_holiday"] = new Date(item["day"]).getDay() == 0;
            });
          }
          let dateinweeks2 = bindDateBetweenFirstAndLast(
            new Date(options.value["week_start_date"]),
            new Date(options.value["week_end_date"]),
          );
          dateinweeks2
            .filter(
              (a) =>
                tbs[2].findIndex(
                  (b) => b["day_string"] === moment(a).format("DD/MM/YYYY"),
                ) === -1,
            )
            .forEach((day, i) => {
              tbs[2].push({
                day: day,
                day_name: getDayDate(day),
                day_string: moment(day).format("DD/MM/YYYY"),
                is_holiday: day.getDay() == 0,
              });
            });
          tbs[0] = tbs[2].sort(function (a, b) {
            return new Date(a["day"]) - new Date(b["day"]);
          });
          if (tbs[2] != null && tbs[2].length > 0) {
            datacalendardutys.value = tbs[2];
          } else {
            datacalendardutys.value = [];
          }
        }
      }
    })
    .catch((error) => {
      console.log(error);
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
          cryoptojs,
        ).toString(),
      },
      config,
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
const initCalendarDuty = (rf) => {
  datacalendardutys.value = [];
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
              { par: "is_type", va: 3 },
              { par: "filterDate", va: true },
              { par: "is_group", va: 2 },
            ],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
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
                  (x) => x["is_type"] === "0",
                );
                item["chihuys"] = item["members"].filter(
                  (x) => x["is_type"] === "1",
                );
              }
              if (item["files"] != null) {
                item["files"] = JSON.parse(item["files"]);
              } else {
                item["files"];
              }
              item["is_holiday"] = new Date(item["day"]).getDay() == 0;
            });
          }
          let dateinweeks = bindDateBetweenFirstAndLast(
            new Date(options.value["week_start_date"]),
            new Date(options.value["week_end_date"]),
          );
          dateinweeks
            .filter(
              (a) =>
                tbs[0].findIndex(
                  (b) => b["day_string"] === moment(a).format("DD/MM/YYYY"),
                ) === -1,
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
          if (tbs[0] != null && tbs[0].length > 0) {
            datacalendardutys.value = tbs[0];
          } else {
            datacalendardutys.value = [];
          }
        }
      }
    })
    .catch((error) => {
      console.log(error);
    });
};
// region chat
const datalistsChat = ref([]);
const chat = ref({
  chat_group_name: "",
  status: 1,
  organization_id: store.getters.user.organization_id,
});
const ActiveMessage = (user) => {
  router
    .push({
      name: "chat_message/fromdashboard",
      params: { uid: user.user_id, typeid: "dashboard" } || {},
    })
    .then(() => {
      router.go(0);
    });
};
const savingChat = ref(false);
const saveGroupChat = () => {
  if (savingChat.value == true) {
    return;
  }

  var ms = { chat_message_id: null };
  let formData = new FormData();
  let listMember = [];
  formData.append("models", JSON.stringify(chat.value));
  formData.append("members", JSON.stringify(listMember));
  formData.append("messages", JSON.stringify(ms));
  savingChat.value = true;
  axios({
    method: "post",
    url: baseURL + "/api/chat/Add_Chat",
    data: formData,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      savingChat.value = false;
      if (response.data.err != "1") {
        if (response.data.chatGroupID) {
          cookies.set("ck_cgi", response.data.chatGroupID);
          router.push({ name: "chat_message" }).then(() => {
            router.go(0);
          });
        }
      } else {
        swal.fire({
          title: "Thông báo!",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    })
    .catch((error) => {
      savingChat.value = false;
      if (error && error.status === 401) {
        swal.fire({
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
// end region chat

const initData = () => {
  initDictionaryCutRice();
  initCounts();
  initDictionaryCalendarDuty();
  //initCountCongviec();
  initCongviec();
  //initCountVanban();
  initVanban();
  initNew();
  initNotify();
  //initVideo();
  initLideshow();
  initBirthday();
};

onMounted(() => {
  initData();
  return {};
});
</script>
<template>
  <div class="surface-100 dashboard">
    <div class="d-grid formgrid m-1">
      <div class="col-8 md:col-8 p-0">
        <div class="d-grid formgrid">
          <div class="col-12 md:col-12 p-0">
            <div class="card m-1">
              <div
                class="card-body p-0"
                style="height: max-content"
              >
                <div class="d-grid formgrid">
                  <div class="col-4 md:col-4">
                    <div
                      class="card zoom"
                      style="background-color: #7ec7fa; color: #fff"
                      @click="goRouter('calendarenact')"
                    >
                      <div class="card-body">
                        <div class="format-grid-center">
                          <h1
                            class="my-2"
                            style="word-break: break-all"
                          >
                            {{ counts.countcalendar }}
                          </h1>
                          <h4 class="m-0">Lịch họp/trực ban</h4>
                        </div>
                      </div>
                    </div>
                  </div>
                  <div class="col-4 md:col-4">
                    <div
                      class="card zoom"
                      style="background-color: #eb6833; color: #fff"
                      @click="goRouter('docreceive')"
                    >
                      <div class="card-body">
                        <div class="format-grid-center">
                          <h1
                            class="my-2"
                            style="word-break: break-all"
                          >
                            {{ counts.countdoc }}
                          </h1>
                          <h4 class="m-0">Văn bản</h4>
                        </div>
                      </div>
                    </div>
                  </div>
                  <div class="col-4 md:col-4">
                    <div
                      class="card zoom"
                      style="background-color: #bfd7b5; color: #fff"
                      @click="goRouter('taskmain')"
                    >
                      <div class="card-body">
                        <div class="format-grid-center">
                          <h1
                            class="my-2"
                            style="word-break: break-all"
                          >
                            {{ counts.counttask }}
                          </h1>
                          <h4 class="m-0">Công việc chờ xử lý</h4>
                        </div>
                      </div>
                    </div>
                  </div>
                  <div class="col-4 md:col-4">
                    <div
                      class="card zoom"
                      style="background-color: #f7ced3; color: #fff"
                      @click="goRouter('bookingmeal')"
                    >
                      <div class="card-body">
                        <div class="format-grid-center">
                          <h1
                            class="my-2"
                            style="word-break: break-all"
                          >
                            {{ counts.countbooking }}
                          </h1>
                          <h4 class="m-0">Báo cắt cơm</h4>
                        </div>
                      </div>
                    </div>
                  </div>
                  <div class="col-4 md:col-4">
                    <div
                      class="card zoom"
                      style="background-color: #9666b1; color: #fff"
                      @click="goRouter('chat_message')"
                    >
                      <div class="card-body">
                        <div class="format-grid-center">
                          <h1
                            class="my-2"
                            style="word-break: break-all"
                          >
                            {{ counts.countmessage }}
                          </h1>
                          <h4 class="m-0">Tin nhắn chưa đọc</h4>
                        </div>
                      </div>
                    </div>
                  </div>
                  <div class="col-4 md:col-4">
                    <div
                      class="card zoom"
                      style="background-color: #f1a545; color: #fff"
                      @click="goRouter('lawmain')"
                    >
                      <div class="card-body">
                        <div class="format-grid-center">
                          <h1
                            class="my-2"
                            style="word-break: break-all"
                          >
                            {{ counts.countlaw }}
                          </h1>
                          <h4 class="m-0">Văn bản luật</h4>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
          <div class="col-6 md:col-6 p-0">
            <div class="card m-1">
              <div
                class="card-header"
                @click="goRouter('/news/direct')"
                style="cursor: pointer"
              >
                <span>Chỉ đạo điều hành</span>
              </div>
              <div
                class="card-body carousel-hidden-p-link"
                style="height: 460px"
              >
                <Carousel
                  v-show="datanews.length > 0"
                  :value="datanews"
                  :numVisible="4"
                  :numScroll="4"
                  :circular="false"
                  orientation="vertical"
                  verticalViewPortHeight="400px"
                >
                  <template #item="slotProps">
                    <div
                      class="grid-item carousel-item"
                      @click="
                        goRouter('/news/direct/details', {
                          name: '-orient-' + slotProps.data.news_id,
                        })
                      "
                    >
                      <div class="d-grid formgrid px-2">
                        <div class="col-12 md:col-12 p-0 pl-0">
                          <div class="d-grid formgrid">
                            <div class="col-12 md:col-12 p-0 flex pb-2">
                              <div>
                                <img
                                  v-if="slotProps.data.is_hot"
                                  style="
                                    width: 40px;
                                    height: 20px;
                                    margin-right: 12px;
                                  "
                                  :src="basedomainURL + '/Portals/News/new.jpg'"
                                  alt="new"
                                />
                              </div>
                              <div>
                                <span
                                  class="limit-line"
                                  :class="
                                    slotProps.data.is_hot ? 'font-bold' : ''
                                  "
                                  >{{ slotProps.data.title }}</span
                                >
                              </div>
                            </div>
                            <div class="col-12 md:col-12 p-0">
                              <div class="description">
                                <i class="pi pi-clock"></i>
                                <span class="ml-2">{{
                                  slotProps.data.approved_date
                                }}</span>
                              </div>
                            </div>
                          </div>
                        </div>
                        <div class="col-12 md:col-12 p-0 pt-2">
                          <div class="description">
                            <span class="limit-line">{{
                              slotProps.data.des
                            }}</span>
                          </div>
                        </div>
                      </div>
                    </div>
                  </template>
                </Carousel>
                <div
                  v-show="datanews == null || datanews.length == 0"
                  class="w-full h-full format-flex-center"
                >
                  <span class="description">Hiện chưa có dữ liệu</span>
                </div>
              </div>
            </div>
          </div>

          <div class="col-6 md:col-6 p-0">
            <div class="card m-1">
              <div
                class="card-header"
                @click="goRouter('docreceive')"
                style="cursor: pointer"
              >
                <span>Văn bản của tôi</span>
              </div>
              <!-- <div class="card-body" style="height: 190px">
                <div
                  v-show="
                    countvbs.datasets[0].data.filter((x) => x > 0).length > 0
                  "
                  class="w-full h-full format-flex-center"
                >
                  <Chart
                    id="chartvanban"
                    type="doughnut"
                    :data="countvbs"
                    :options="lightOptions"
                    style="
                      width: 45% !important;
                      height: 10% !important;
                      vertical-align: middle;
                      align-items: center;
                      display: flex;
                    "
                  />
                </div>
                <div
                  v-show="
                    countvbs.datasets[0].data.filter((x) => x > 0).length == 0
                  "
                  class="w-full h-full format-flex-center"
                >
                  <span class="description">Hiện chưa có dữ liệu</span>
                </div>
              </div> -->
              <div
                class="card-body carousel-hidden-p-link"
                style="height: 460px"
              >
                <div
                  v-if="datadocs.length > 0"
                  class="scroll-outer"
                  style="overflow: auto; height: 435px"
                >
                  <div class="scroll-inner">
                    <div class="d-grid formgrid">
                      <div
                        class="col-12 md:col-12 p-0 row-item"
                        v-for="(item, index) in datadocs"
                        v-bind:id="item.doc_master_id"
                        @click="goRouter('docreceive')"
                        :key="index"
                      >
                        <div class="flex">
                          <div
                            class="mr-2"
                            style="flex: 1"
                          >
                            <div
                              style="
                                display: flex;
                                flex-direction: column;
                                padding: 5px;
                              "
                            >
                              <div class="flex">
                                <div class="format-center">
                                  <Avatar
                                    class="avatar-image"
                                    :image="
                                      basedomainURL +
                                      (item.avatar != null
                                        ? item.avatar
                                        : '/Portals/Image/nouser1.png')
                                    "
                                    shape="circle"
                                    size="large"
                                  />
                                </div>
                                <div class="format-center ml-2 text-left">
                                  <div>
                                    <div>{{ item.compendium }}</div>
                                    <div class="description mt-1">
                                      Số ký hiệu: <b>{{ item.doc_code }}</b> |
                                      Ngày văn bản: <b>{{ item.doc_date }}</b>
                                    </div>
                                  </div>
                                </div>
                              </div>
                            </div>
                          </div>
                          <div class="format-center">
                            <DocStatus :DocObj="item"></DocStatus>
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
                <div
                  v-show="datadocs == null || datadocs.length == 0"
                  class="w-full h-full format-flex-center"
                >
                  <span class="description">Hiện chưa có dữ liệu</span>
                </div>
              </div>
            </div>
          </div>
          <div class="col-6 md:col-6 p-0">
            <div class="card m-1">
              <div
                class="card-header"
                @click="goRouter('taskmain')"
                style="cursor: pointer"
              >
                <span>Công việc</span>
              </div>
              <div
                class="card-body carousel-hidden-p-link"
                style="height: 460px"
              >
                <div
                  v-if="datatasks.length > 0"
                  class="scroll-outer"
                  style="overflow: auto; height: 435px"
                >
                  <div class="scroll-inner">
                    <div class="d-grid formgrid">
                      <div
                        class="col-12 md:col-12 p-0 row-item"
                        v-for="(item, index) in datatasks"
                        v-bind:id="item.task_id"
                        :key="index"
                        @click="onRowSelect(item.task_id)"
                      >
                        <div class="flex">
                          <div style="flex: 1">
                            <div
                              style="
                                display: flex;
                                flex-direction: column;
                                padding: 5px;
                              "
                            >
                              <div style="line-height: 20px; display: flex">
                                <span
                                  v-tooltip="'Ưu tiên'"
                                  v-if="item.is_prioritize"
                                  style="margin-right: 5px"
                                  ><i
                                    style="color: orange"
                                    class="pi pi-star-fill"
                                  ></i
                                ></span>
                                <span
                                  style="
                                    font-weight: bold;
                                    overflow: hidden;
                                    text-overflow: ellipsis;
                                    width: 100%;
                                    display: -webkit-box;
                                    -webkit-line-clamp: 2;
                                    -webkit-box-orient: vertical;
                                  "
                                  >{{ item.task_name }}</span
                                >
                              </div>
                              <div class="description mt-1">
                                <span v-if="item.start_date || item.end_date"
                                  >{{ item.start_date }}
                                  <span v-if="item.start_date && item.end_date"
                                    >-</span
                                  >
                                  {{ item.end_date }}</span
                                >
                              </div>
                            </div>
                          </div>
                          <div
                            v-if="item.title_time"
                            class="format-center mr-2"
                          >
                            <span
                              style="
                                font-size: 10px;
                                font-weight: bold;
                                padding: 5px;
                                border-radius: 5px;
                              "
                              :style="{
                                background: item.time_bg,
                                color: item.status_text_color,
                              }"
                              >{{ item.title_time }}</span
                            >
                          </div>
                          <div class="format-center">
                            <span
                              style="
                                font-size: 10px;
                                font-weight: bold;
                                padding: 5px;
                                border-radius: 5px;
                              "
                              :style="{
                                background: item.status_bg_color,
                                color: item.status_text_color,
                              }"
                              >{{ item.status_name }}</span
                            >
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
                <div
                  v-show="datatasks == null || datatasks.length == 0"
                  class="w-full h-full format-flex-center"
                >
                  <span class="description">Hiện chưa có dữ liệu</span>
                </div>
              </div>
            </div>
          </div>
          <!-- <div class="col-6 md:col-6 p-0">
            <div class="card m-1">
              <div
                class="card-header"
                @click="goRouter('/news/direct')"
                style="cursor: pointer"
              >
                <span>Thông báo</span>
              </div>
              <div class="card-body" style="height: 400px">
                <div
                  v-if="datanotifys.length > 0"
                  class="scroll-outer"
                  style="overflow: auto; height: 375px"
                >
                  <div class="scroll-inner">
                    <div class="d-grid formgrid">
                      <div
                        class="col-12 md:col-12 p-0 row-item"
                        v-for="(item, index) in datanotifys"
                        v-bind:id="item.news_id"
                        :key="index"
                        @click="
                          goRouter('/news/direct/details', {
                            name: '-orient-' + item.news_id,
                          })
                        "
                      >
                        <div class="mb-2 limit-line">{{ item.title }}</div>
                        <div class="description">
                          <i class="pi pi-clock"></i>
                          <span class="ml-2">{{ item.approved_date }}</span>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
                <div v-else class="w-full h-full format-flex-center">
                  <span class="description">Hiện chưa có dữ liệu</span>
                </div>
              </div>
            </div>
          </div> -->
          <div class="col-6 md:col-6 p-0">
            <div class="card m-1">
              <div
                class="card-header"
                @click="goRouter('/news/shows')"
                style="cursor: pointer"
              >
                <span>Trình diễn</span>
              </div>
              <div
                class="card-body carousel-hidden-p-link"
                style="height: 460px"
              >
                <Carousel
                  v-show="datalideshows.length > 0"
                  :value="datalideshows"
                  :numVisible="1"
                  :numScroll="1"
                  :circular="true"
                  orientation="horizontal"
                  verticalViewPortHeight="460px"
                >
                  <template #item="slotProps">
                    <div
                      class="grid-item"
                      @click="
                        goRouter('/news/shows/details', {
                          name: '-orient-' + slotProps.data.shows_id,
                        })
                      "
                      style="cursor: pointer"
                    >
                      <div class="d-grid formgrid">
                        <div class="col-12 md:col-12 p-0">
                          <div style="height: 335px">
                            <img
                              :src="
                                slotProps.data.image
                                  ? basedomainURL + slotProps.data.image
                                  : basedomainURL + '/Portals/Image/noimg.jpg'
                              "
                              :alt="slotProps.data.title"
                              style="
                                width: 100%;
                                height: 100%;
                                object-fit: cover;
                                border-radius: 3px;
                              "
                            />
                          </div>
                        </div>
                        <div class="col-12 md:col-12 p-0 pt-3">
                          <div class="d-grid formgrid">
                            <div class="col-6 md:col-6 p-0">
                              <div class="description">
                                <i class="pi pi-eye"></i>
                                <span class="ml-2"
                                  >{{ slotProps.data.views_count }} người
                                  xem</span
                                >
                              </div>
                            </div>
                            <div class="col-6 md:col-6 p-0">
                              <div
                                class="description"
                                style="text-align: right"
                              >
                                <i class="pi pi-clock"></i>
                                <span class="ml-2">{{
                                  slotProps.data.created_date
                                }}</span>
                              </div>
                            </div>
                            <div class="col-12 md:col-12 p-0 pt-3">
                              <span
                                class="limit-line"
                                style="text-align: center"
                                >{{ slotProps.data.title }}</span
                              >
                            </div>
                          </div>
                        </div>
                      </div>
                    </div>
                  </template>
                </Carousel>
                <div
                  v-show="datalideshows == null || datalideshows.length == 0"
                  class="w-full h-full format-flex-center"
                >
                  <span class="description">Hiện chưa có dữ liệu</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
      <div class="col-4 md:col-4 p-0">
        <div class="d-grid formgrid">
          <!-- <div class="col-12 md:col-12 p-0">
            <div class="card m-1">
              <div
                class="card-header"
                @click="goRouter('taskmain')"
                style="cursor: pointer"
              >
                <span>Công việc của tôi</span>
              </div>
              <div class="card-body" style="height: 190px">
                <div
                  v-show="
                    countcvs.datasets[0].data.filter((x) => x > 0).length > 0
                  "
                  class="w-full h-full format-flex-center"
                >
                  <Chart
                    id="chartvanban"
                    type="doughnut"
                    :data="countcvs"
                    :options="lightOptions"
                    style="width: 50% !important; height: 100% !important"
                  />
                </div>
                <div
                  v-show="
                    countcvs.datasets[0].data.filter((x) => x > 0).length == 0
                  "
                  class="w-full h-full format-flex-center"
                >
                  <span class="description">Hiện chưa có dữ liệu</span>
                </div>
              </div>
            </div>
          </div> -->
          <div
            v-if="databookings != null && databookings.length > 0"
            class="col-12 md:col-12 p-0"
          >
            <div class="card m-1">
              <div
                class="card-header"
                @click="openDialogCutRice('Đăng ký cắt cơm ngày đi công tác')"
                style="cursor: pointer"
              >
                <div class="flex">
                  <i
                    class="pi pi-bell px-2 class bell flex"
                    style="color: red; align-items: center"
                  ></i>
                  <span
                    >Bạn có ({{ databookings.length }}) lịch đi công tác chưa
                    báo cắt cơm</span
                  >
                </div>
              </div>
            </div>
          </div>
          <div class="col-12 md:col-12 p-0">
            <div class="card m-1">
              <div
                class="card-header"
                @click="goRouter('calendarplantripenact')"
                style="cursor: pointer"
              >
                <span>Lịch làm việc của Ban Giám đốc </span>
              </div>
              <div
                class="card-body d-lang-table p-0"
                style="height: 275px; overflow-y: auto"
              >
                <DataTable
                  @sort="onSort($event)"
                  :value="datacalendars"
                  :lazy="true"
                  :rowHover="true"
                  :showGridlines="true"
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
                    headerStyle="text-align:center;width:100px;height:50px;"
                    bodyStyle="text-align:center;width:100px;"
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
                    field="trucbans"
                    header="Ban giám đốc"
                    headerStyle="text-align:center;width:150px;height:50px"
                    bodyStyle="text-align:center;width:150px;max-height:60px"
                    class="align-items-center justify-content-center text-left"
                  >
                    <template #body="slotProps">
                      <div
                        v-if="
                          slotProps.data.chutris &&
                          slotProps.data.chutris.length > 0
                        "
                        @click="goCalendar(slotProps.data)"
                        class="hover"
                      >
                        {{ slotProps.data.chutris[0].full_name }}
                      </div>
                    </template>
                  </Column>
                  <Column
                    field="contents"
                    header="Nội dung"
                    headerStyle="height:50px;max-width:auto;min-width:120px;"
                    class="align-items-center justify-content-center text-left"
                  >
                    <template #body="slotProps">
                      <div
                        @click="goCalendar(slotProps.data)"
                        v-html="slotProps.data.contents"
                        class="hover"
                      ></div>
                    </template>
                  </Column>
                  <template #empty>
                    <div
                      class="align-items-center justify-content-center p-4 text-center m-auto"
                      v-if="
                        !datacalendardutys && datacalendardutys.length === 0
                      "
                      style="display: flex; height: calc(100vh - 225px)"
                    >
                      <div>
                        <img
                          src="../../../assets/background/nodata.png"
                          height="144"
                        />
                        <h3 class="m-1">Không có dữ liệu</h3>
                      </div>
                    </div>
                  </template>
                </DataTable>
              </div>
            </div>
          </div>
          <div class="col-12 md:col-12 p-0">
            <div class="card m-1">
              <div
                class="card-header"
                @click="goRouter('calendarplantripenact')"
                style="cursor: pointer"
              >
                <span>Trực chủ nhật ({{ holiday.day_string }})</span>
              </div>
              <div
                class="card-body"
                style="height: 65px; overflow-y: auto"
              >
                <div v-if="duty_sunday && duty_sunday.user_id">
                  <div
                    class="format-center"
                    :style="{ justifyContent: 'left' }"
                  >
                    <Avatar
                      v-bind:label="
                        duty_sunday.avatar
                          ? ''
                          : (duty_sunday.last_name ?? '').substring(0, 1)
                      "
                      v-bind:image="
                        duty_sunday.avatar
                          ? basedomainURL + duty_sunday.avatar
                          : basedomainURL + '/Portals/Image/noimg.jpg'
                      "
                      :style="{
                        background: bgColor[1 % 7],
                        color: '#ffffff',
                        width: '3rem',
                        height: '3rem',
                        fontSize: '1rem !important',
                      }"
                      class="mr-2 text-avatar"
                      size="xlarge"
                      shape="circle"
                    />
                    <div class="text-left">
                      <div>
                        Đồng chí {{ duty_sunday.rank }}
                        <b>{{ duty_sunday.full_name }}</b>
                      </div>
                      <div>
                        {{ duty_sunday.position_name }}
                        {{ duty_sunday.department_name }}
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
          <div class="col-12 md:col-12 p-0">
            <div class="card m-1">
              <div
                class="card-header"
                @click="goRouter('calendardutyapproved')"
                style="cursor: pointer"
              >
                <span>Lịch trực ban </span>
                <span>(tháng {{ options.month }}/{{ options.year }})</span>
              </div>
              <div
                class="card-body d-lang-table p-0"
                style="height: 275px; overflow-y: auto"
              >
                <DataTable
                  @sort="onSort($event)"
                  :value="datacalendardutys"
                  :lazy="true"
                  :rowHover="true"
                  :showGridlines="true"
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
                    headerStyle="text-align:center;width:100px;height:50px;"
                    bodyStyle="text-align:center;width:100px;"
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
                  <!-- <Column
                    field="is_timelot"
                    header="Ca trực"
                    headerStyle="text-align:center;width:80px;height:50px"
                    bodyStyle="text-align:center;width:80px;"
                    class="
                      align-items-center
                      justify-content-center
                      text-center
                    "
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
                    field="trucban"
                    header="Họ và tên"
                    headerStyle="height:50px;max-width:auto;min-width:120px;"
                    bodyStyle="max-height:60px;"
                    class="align-items-center justify-content-center text-left"
                  >
                  </Column>
                  <Column
                    field="trucchihuy"
                    header="Trực chỉ huy"
                    headerStyle="text-align:center;width:150px;height:50px"
                    bodyStyle="text-align:center;width:150px;max-height:60px"
                    class="align-items-center justify-content-center text-left"
                  >
                  </Column>

                  <template #empty>
                    <div
                      class="align-items-center justify-content-center p-4 text-center m-auto"
                      v-if="
                        !datacalendardutys && datacalendardutys.length === 0
                      "
                      style="display: flex; height: calc(100vh - 225px)"
                    >
                      <div>
                        <img
                          src="../../../assets/background/nodata.png"
                          height="144"
                        />
                        <h3 class="m-1">Không có dữ liệu</h3>
                      </div>
                    </div>
                  </template>
                </DataTable>
              </div>
            </div>
          </div>
          <div class="col-12 md:col-12 p-0">
            <div
              class="card m-1"
              @click="goRouter('birthday')"
              style="cursor: pointer"
            >
              <div class="card-header">
                <span>Sinh nhật</span>
              </div>
              <div
                class="card-body"
                style="height: 80px"
              >
                <div class="d-grid formgrid">
                  <div class="col-3 md:col-3 p-0">
                    <div class="format-grid-center">
                      <div style="width: 55px">
                        <img
                          :src="basedomainURL + '/Portals/birthday.png'"
                          style="
                            width: 100%;
                            height: 100%;
                            object-fit: contain;
                            border-radius: 3px;
                          "
                        />
                      </div>
                    </div>
                  </div>
                  <div class="col-9 md:col-9 p-0">
                    <div class="d-grid formgrid">
                      <div class="col-12 md:col-12 p-0 pb-2 text-center">
                        <span
                          v-if="
                            datatodaybirthdays && datatodaybirthdays.length > 0
                          "
                          >Sinh nhật hôm nay</span
                        >
                        <span v-else>Sinh nhật sắp tới</span>
                      </div>
                      <div
                        v-if="
                          datatodaybirthdays && datatodaybirthdays.length > 0
                        "
                        class="col-12 md:col-12 p-0"
                      >
                        <div class="flex justify-content-center">
                          <AvatarGroup
                            v-if="
                              datatodaybirthdays &&
                              datatodaybirthdays.length > 0
                            "
                          >
                            <Avatar
                              v-for="(item, index) in datatodaybirthdays.slice(
                                0,
                                3,
                              )"
                              v-bind:label="
                                item.avatar
                                  ? ''
                                  : item.last_name.substring(0, 1)
                              "
                              v-bind:image="
                                item.avatar
                                  ? basedomainURL + item.avatar
                                  : basedomainURL + '/Portals/Image/noimg.jpg'
                              "
                              v-tooltip.top="item.full_name"
                              :key="item.user_id"
                              style="border: 2px solid white; color: white"
                              @error="
                                basedomainURL + '/Portals/Image/noimg.jpg'
                              "
                              size="large"
                              shape="circle"
                              class="cursor-pointer"
                              :style="{ backgroundColor: bgColor[index % 7] }"
                            />
                            <Avatar
                              v-if="
                                datatodaybirthdays &&
                                datatodaybirthdays.length > 3
                              "
                              v-bind:label="
                                '+' + (datatodaybirthdays.length - 3).toString()
                              "
                              shape="circle"
                              size="large"
                              style="background-color: #2196f3; color: #ffffff"
                              class="cursor-pointer"
                            />
                          </AvatarGroup>
                        </div>
                      </div>
                      <div
                        v-else
                        class="col-12 md:col-12 p-0"
                      >
                        <div class="flex justify-content-center">
                          <AvatarGroup
                            v-if="databirthdays && databirthdays.length > 0"
                          >
                            <Avatar
                              v-for="(item, index) in databirthdays.slice(0, 3)"
                              v-bind:label="
                                item.avatar
                                  ? ''
                                  : item.last_name.substring(0, 1)
                              "
                              v-bind:image="
                                item.avatar
                                  ? basedomainURL + item.avatar
                                  : basedomainURL + '/Portals/Image/noimg.jpg'
                              "
                              v-tooltip.top="item.full_name"
                              :key="item.user_id"
                              style="border: 2px solid white; color: white"
                              @error="
                                basedomainURL + '/Portals/Image/noimg.jpg'
                              "
                              size="large"
                              shape="circle"
                              class="cursor-pointer"
                              :style="{ backgroundColor: bgColor[index % 7] }"
                            />
                            <Avatar
                              v-if="databirthdays && databirthdays.length > 3"
                              v-bind:label="
                                '+' + (databirthdays.length - 3).toString()
                              "
                              shape="circle"
                              size="large"
                              style="background-color: #2196f3; color: #ffffff"
                              class="cursor-pointer"
                            />
                          </AvatarGroup>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
          <div class="col-12 md:col-12 p-0">
            <div class="card m-1">
              <div
                class="card-header"
                style="
                  display: flex;
                  justify-content: space-between;
                  cursor: pointer;
                "
                @click="goRouter('birthday')"
              >
                <span>Danh bạ</span>
                <span style="color: gray"
                  >{{ dataphonebooks.length }} người</span
                >
              </div>
              <div
                class="card-body"
                style="height: 270px"
              >
                <div
                  v-if="dataphonebooks.length > 0"
                  class="scroll-outer"
                  style="overflow: auto; height: 250px"
                >
                  <div class="scroll-inner">
                    <div class="d-grid formgrid">
                      <div
                        class="col-12 md:col-12 p-0 pb-2"
                        v-for="(item, index) in dataphonebooks"
                        v-bind:id="item.user_id.replace('.', '')"
                        :key="index"
                      >
                        <div
                          class="d-grid formgrid"
                          style="cursor: pointer"
                          @click="ActiveMessage(item)"
                        >
                          <div class="col-3 md:col-3 p-0 format-flex-center">
                            <div
                              style="display: inline-block; position: relative"
                            >
                              <Avatar
                                v-bind:label="
                                  item.avatar
                                    ? ''
                                    : (item.last_name || '').substring(0, 1)
                                "
                                v-bind:image="
                                  item.avatar
                                    ? basedomainURL + item.avatar
                                    : basedomainURL + '/Portals/Image/noimg.jpg'
                                "
                                style="
                                  background-color: #2196f3;
                                  color: #ffffff;
                                  cursor: pointer;
                                  width: 3.5rem;
                                  height: 3.5rem;
                                  font-size: 1.25rem !important;
                                "
                                :style="{
                                  background: bgColor[index % 7],
                                }"
                                size="xlarge"
                                shape="circle"
                              />
                              <span
                                :class="item.status == 1 ? 'online' : 'offline'"
                              ></span>
                            </div>
                          </div>
                          <div class="col-9 md:col-9 p-0">
                            <div class="mb-2 font-bold">
                              {{ item.full_name }}
                            </div>
                            <div class="description">{{ item.phone }}</div>
                            <div class="description">
                              {{ item.organization_name }}
                            </div>
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
                <div
                  v-else
                  class="w-full h-full format-flex-center"
                >
                  <span class="description">Hiện chưa có dữ liệu</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
  <detailedwork
    v-if="showDetail === true"
    :id="selectedTaskID"
    :turn="0"
    :closeDetail="closeDetail"
  >
  </detailedwork>

  <dialogcutrice
    :key="componentKey"
    :headerDialog="headerDialogCutRice"
    :displayDialog="displayDialogCutRice"
    :closeDialog="closeDialogCutRice"
    :initData="initCutRice"
  />
</template>
<style scoped>
@import url(../../calendar/component/stylecalendar.css);
</style>
<style scoped>
.dashboard {
  overflow: auto;
  max-height: calc(100vh - 50px);
}
.d-grid {
  display: flex;
  flex-wrap: wrap;
}
.d-lang-table {
  height: calc(100vh - 190px);
  overflow-y: auto;
}
.inputanh {
  border: 1px solid #ccc;
  width: 96px;
  height: 96px;
  cursor: pointer;
  padding: 1px;
}
.ipnone {
  display: none;
}
.inputanh img {
  object-fit: cover;
  width: 100%;
  height: 100%;
}
.format-flex-center {
  display: flex;
  align-items: center;
  justify-content: center;
  text-align: center;
}
.format-grid-center {
  display: grid;
  align-items: center;
  justify-content: center;
  text-align: center;
}
.form-group {
  display: grid;
  margin-bottom: 1rem;
}
.form-group > label {
  margin-bottom: 0.5rem;
}
.ip36 {
  width: 100%;
}
.p-ulchip {
  margin: 0;
  margin-top: 0.5rem;
  padding: 0;
  list-style: none;
}
.p-lichip {
  float: left;
}
.p-multiselect-label {
  height: 100%;
  display: flex;
  align-items: center;
}

.dashboard .card {
  border: none;
  border-radius: 0;
  position: relative;
  display: -webkit-box;
  display: -ms-flexbox;
  display: flex;
  -webkit-box-orient: vertical;
  -webkit-box-direction: normal;
  -ms-flex-direction: column;
  flex-direction: column;
  min-width: 0;
  word-wrap: break-word;
  background-color: #fff;
  background-clip: border-box;
}
.dashboard .card-header {
  -webkit-box-flex: 1;
  -ms-flex: 1 1 auto;
  flex: 1 1 auto;
  padding: 1rem;
  overflow: hidden;
  border-bottom: solid 1px rgba(0, 0, 0, 0.1);
  font-size: 15px;
  font-weight: bold;
  color: #005a9e;
}
.dashboard .card-body {
  -webkit-box-flex: 1;
  -ms-flex: 1 1 auto;
  flex: 1 1 auto;
  padding: 1rem;
  overflow: hidden;
}
.zoom {
  cursor: pointer;
  border-radius: 0.25rem;
  box-shadow: 0 2px 4px rgb(0 0 0 / 23%);
  transition: transform 0.3s !important;
}
.zoom:hover {
  transform: scale(0.9) !important;
  box-shadow: 10px 10px 15px rgb(0 0 0 / 23%) !important;
  cursor: pointer !important;
}
.grid-item {
  display: grid;
}
.flex-item {
  display: flex;
}
.limit-line {
  text-overflow: ellipsis;
  overflow: hidden;
  column-gap: initial;
  -webkit-line-clamp: 2;
  display: -webkit-box;
  -webkit-box-orient: vertical;
}
.description {
  color: #aaa;
  font-size: 12px;
}
.carousel-item {
  cursor: pointer;
  padding-bottom: 0.5rem !important;
}
.row-item {
  cursor: pointer;
  padding: 0.5rem !important;
}
.carousel-item:hover,
.row-item:hover {
  background-color: aliceblue;
}
span.online {
  position: absolute;
  display: block;
  width: 14px;
  height: 14px;
  background-color: rgb(98, 203, 0);
  border-radius: 50%;
  right: 0;
  bottom: 0;
  border: 2px solid #fff;
}
.scroll-outer {
  visibility: hidden;
}
.scroll-inner,
.scroll-outer:hover,
.scroll-outer:focus {
  visibility: visible;
}
</style>
<style scoped>
/* Bell */
.bell {
  display: block;
  color: #9e9e9e;
  -webkit-animation: ring 4s 0.7s ease-in-out infinite;
  -webkit-transform-origin: 50% 4px;
  -moz-animation: ring 4s 0.7s ease-in-out infinite;
  -moz-transform-origin: 50% 4px;
  animation: ring 4s 0.7s ease-in-out infinite;
  transform-origin: 50% 4px;
}
@-webkit-keyframes ring {
  0% {
    -webkit-transform: rotateZ(0);
  }
  1% {
    -webkit-transform: rotateZ(30deg);
  }
  3% {
    -webkit-transform: rotateZ(-28deg);
  }
  5% {
    -webkit-transform: rotateZ(34deg);
  }
  7% {
    -webkit-transform: rotateZ(-32deg);
  }
  9% {
    -webkit-transform: rotateZ(30deg);
  }
  11% {
    -webkit-transform: rotateZ(-28deg);
  }
  13% {
    -webkit-transform: rotateZ(26deg);
  }
  15% {
    -webkit-transform: rotateZ(-24deg);
  }
  17% {
    -webkit-transform: rotateZ(22deg);
  }
  19% {
    -webkit-transform: rotateZ(-20deg);
  }
  21% {
    -webkit-transform: rotateZ(18deg);
  }
  23% {
    -webkit-transform: rotateZ(-16deg);
  }
  25% {
    -webkit-transform: rotateZ(14deg);
  }
  27% {
    -webkit-transform: rotateZ(-12deg);
  }
  29% {
    -webkit-transform: rotateZ(10deg);
  }
  31% {
    -webkit-transform: rotateZ(-8deg);
  }
  33% {
    -webkit-transform: rotateZ(6deg);
  }
  35% {
    -webkit-transform: rotateZ(-4deg);
  }
  37% {
    -webkit-transform: rotateZ(2deg);
  }
  39% {
    -webkit-transform: rotateZ(-1deg);
  }
  41% {
    -webkit-transform: rotateZ(1deg);
  }
  43% {
    -webkit-transform: rotateZ(0);
  }
  100% {
    -webkit-transform: rotateZ(0);
  }
}
@-moz-keyframes ring {
  0% {
    -moz-transform: rotate(0);
  }
  1% {
    -moz-transform: rotate(30deg);
  }
  3% {
    -moz-transform: rotate(-28deg);
  }
  5% {
    -moz-transform: rotate(34deg);
  }
  7% {
    -moz-transform: rotate(-32deg);
  }
  9% {
    -moz-transform: rotate(30deg);
  }
  11% {
    -moz-transform: rotate(-28deg);
  }
  13% {
    -moz-transform: rotate(26deg);
  }
  15% {
    -moz-transform: rotate(-24deg);
  }
  17% {
    -moz-transform: rotate(22deg);
  }
  19% {
    -moz-transform: rotate(-20deg);
  }
  21% {
    -moz-transform: rotate(18deg);
  }
  23% {
    -moz-transform: rotate(-16deg);
  }
  25% {
    -moz-transform: rotate(14deg);
  }
  27% {
    -moz-transform: rotate(-12deg);
  }
  29% {
    -moz-transform: rotate(10deg);
  }
  31% {
    -moz-transform: rotate(-8deg);
  }
  33% {
    -moz-transform: rotate(6deg);
  }
  35% {
    -moz-transform: rotate(-4deg);
  }
  37% {
    -moz-transform: rotate(2deg);
  }
  39% {
    -moz-transform: rotate(-1deg);
  }
  41% {
    -moz-transform: rotate(1deg);
  }
  43% {
    -moz-transform: rotate(0);
  }
  100% {
    -moz-transform: rotate(0);
  }
}
@keyframes ring {
  0% {
    transform: rotate(0);
  }
  1% {
    transform: rotate(30deg);
  }
  3% {
    transform: rotate(-28deg);
  }
  5% {
    transform: rotate(34deg);
  }
  7% {
    transform: rotate(-32deg);
  }
  9% {
    transform: rotate(30deg);
  }
  11% {
    transform: rotate(-28deg);
  }
  13% {
    transform: rotate(26deg);
  }
  15% {
    transform: rotate(-24deg);
  }
  17% {
    transform: rotate(22deg);
  }
  19% {
    transform: rotate(-20deg);
  }
  21% {
    transform: rotate(18deg);
  }
  23% {
    transform: rotate(-16deg);
  }
  25% {
    transform: rotate(14deg);
  }
  27% {
    transform: rotate(-12deg);
  }
  29% {
    transform: rotate(10deg);
  }
  31% {
    transform: rotate(-8deg);
  }
  33% {
    transform: rotate(6deg);
  }
  35% {
    transform: rotate(-4deg);
  }
  37% {
    transform: rotate(2deg);
  }
  39% {
    transform: rotate(-1deg);
  }
  41% {
    transform: rotate(1deg);
  }
  43% {
    transform: rotate(0);
  }
  100% {
    transform: rotate(0);
  }
}
</style>
<style lang="scss" scoped>
::v-deep(.carousel-hidden-p-link) {
  .p-carousel-content .p-carousel-prev,
  .p-carousel-content .p-carousel-next {
    display: none;
  }
  .p-carousel-indicators {
    position: absolute;
    bottom: 0;
    left: 50%;
    transform: translate(-50%, 0);
  }
}
::v-deep(.datatable-hidden-header) {
  thead {
    display: none;
  }
}
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
