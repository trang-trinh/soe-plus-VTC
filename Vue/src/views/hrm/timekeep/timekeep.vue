<script setup>
import { onMounted, inject, ref, watch } from "vue";
import { encr } from "../../../util/function";
import { useToast } from "vue-toastification";
import moment from "moment";

const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios");
const cryoptojs = inject("cryptojs");
const base_url = baseURL;
const config = {
  headers: {
    Authorization: `Bearer ${store.getters.token}`,
  },
};
const toast = useToast();

//Declare
const isFunction = ref(false);
const newDate = new Date();
const options = ref({
  loading: true,
  user_id: store.getters.user.user_id,
  search: "",
  pageNo: 1,
  pageSize: 25,
  total: 0,
  sort: "created_date desc",
  orderBy: "desc",
  view: 2,
  tempyear: newDate,
  year: newDate.getFullYear(),
  month: newDate.getMonth() + 1,
  week: 0,
  day: newDate.getDate(),
  start_date: null,
  end_date: null,
  filter_organization_id: null,
});
const isFirst = ref(true);
const checkedAll = ref(false);
const selectedNodes = ref([]);
const datas = ref([]);

//Declare dictionary
const dictionarys = ref([]);
const days = ref([]);
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

//filter
const search = () => {
  options.value.pageNo = 1;
  initData(true);
};
const changeView = (view) => {
  if (view) {
    options.value.view = view;
    options.value.view_copy = view;
    switch (view) {
      case 0:
        options.value["start_date"] = new Date(
          options.value["year"],
          options.value["month"] - 1,
          1
        );
        options.value["end_date"] = new Date(
          options.value["year"],
          options.value["month"],
          0
        );
        initDay(options.value["start_date"], options.value["end_date"]);
        break;
      case 1:
        if (options.value["week"] === 0) {
          var idx = weeks.value.findIndex((x) => x["is_current_week"] === true);
          if (idx !== -1) {
            options.value["week"] = weeks.value[idx]["week_no"];
            options.value["start_date"] = weeks.value[idx]["week_start_date"];
            options.value["end_date"] = weeks.value[idx]["week_end_date"];
          } else {
            options.value["week"] = 0;
            options.value["start_date"] = weeks.value[0]["week_start_date"];
            options.value["end_date"] = weeks.value[0]["week_end_date"];
          }
        } else {
          var current = weeks.value.find(
            (x) => x["week_no"] === options.value["week"]
          );
          options.value["start_date"] = current["week_start_date"];
          options.value["end_date"] = current["week_end_date"];
        }

        initDay(options.value["start_date"], options.value["end_date"]);
        break;
      case 2:
        options.value["start_date"] = new Date(
          options.value["year"],
          options.value["month"] - 1,
          1
        );
        options.value["end_date"] = new Date(
          options.value["year"],
          options.value["month"],
          0
        );
        initDay(options.value["start_date"], options.value["end_date"]);
        break;
      default:
        break;
    }
    initData(true);
  } else {
    options.value.view = options.value.view_copy;
  }
};
const goWeek = (week) => {
  if (week && options.value["week_copy"] !== week) {
    options.value["week"] = week;
    options.value["week_copy"] = week;
    changeView(options.value.view);
  }
};
const goMonth = (month) => {
  if (month && options.value["month_copy"] !== month) {
    options.value["month"] = month;
    options.value["month_copy"] = month;
    changeView(options.value.view);
  }
};
const goYear = (date) => {
  if (date && options.value["year"] !== date.getFullYear()) {
    options.value["year"] = date.getFullYear();
    initDictionary();
  }
};

//Function
Date.prototype.addDays = function (days) {
  var date = new Date(this.valueOf());
  date.setDate(date.getDate() + days);
  return date;
};
const getDayDate = (d) => {
  var date = new Date(d);
  var current_day = date.getDay();
  var day_name = "";
  var day_name_short = "";
  if (current_day != null) {
    switch (current_day) {
      case 0:
        day_name = "Chủ Nhật";
        day_name_short = "CN";
        break;
      case 1:
        day_name = "Thứ Hai";
        day_name_short = "T2";
        break;
      case 2:
        day_name = "Thứ Ba";
        day_name_short = "T3";
        break;
      case 3:
        day_name = "Thứ Tư";
        day_name_short = "T4";
        break;
      case 4:
        day_name = "Thứ Năm";
        day_name_short = "T5";
        break;
      case 5:
        day_name = "Thứ Sáu";
        day_name_short = "T6";
        break;
      case 6:
        day_name = "Thứ Bảy";
        day_name_short = "T7";
        break;
      default:
        break;
    }
  }
  return { day_name, day_name_short };
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
const componentKey = ref(0);
const forceRerender = () => {
  componentKey.value += 1;
};
const addToArray = (temp, array, id, lv, od) => {
  var filter = array.filter((x) => x.parent_id === id);
  filter = filter.sort((a, b) => {
    return b[od] - a[od];
  });
  if (filter.length > 0) {
    var sp = "";
    for (var i = 0; i < lv; i++) {
      sp += "---";
    }
    lv++;
    filter.forEach((item) => {
      item.lv = lv;
      item.close = true;
      if (!item.ids) {
        item.ids = "";
        item.ids += "," + item.organization_id;
      }
      if (!item.newname) item.newname = sp + item.organization_name;
      temp.push(item);
      addToArray(temp, array, item.organization_id, lv);
    });
  }
};

//function selection
const selectNode = (user, day) => {
  if (isFunction.value) {
    day["selected"] = !(day["selected"] || false);
    var workday_string = moment(new Date(day["day"])).format("YYYY/MM/DD");
    if (day["selected"]) {
      selectedNodes.value.push({
        user_id: user["user_id"],
        workday: workday_string,
      });
    } else {
      var idx = selectedNodes.value.findIndex(
        (x) =>
          x["user_id"] === user["user_id"] && x["workday"] === workday_string
      );
      if (idx != -1) {
        selectedNodes.value.splice(idx, 1);
      }
    }
  }
};
const removeSelectedNodes = () => {
  checkedAll.value = false;
  selectedNodes.value = [];
  if (datas.value != null && datas.value.length > 0) {
    datas.value
      .filter((a) => a["list_days"].filter((b) => b["selected"]).length > 0)
      .forEach((user) => {
        if (user["list_days"] != null && user["list_days"].length > 0) {
          user["checked"] = false;
          user["list_days"].forEach((day) => {
            day["selected"] = false;
          });
        }
      });
  }
};

//add model
const isAdd = ref(false);
const submitted = ref(false);
const model = ref({});
const headerDialog = ref();
const displayDialog = ref(false);
const openUpdateDialog = (str) => {
  forceRerender();
  isAdd.value = true;
  model.value = {};
  headerDialog.value = str;
  displayDialog.value = true;
};
const closeDialog = () => {
  displayDialog.value = false;
};
const saveTimekeep = () => {
  submitted.value = true;
  if (!model.value.symbol && Object.entries(model.value.symbol).length > 0) {
    swal.fire({
      title: "Thông báo!",
      text: "Vui lòng điền đầy đủ thông tin trường bôi đỏ!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  if (model.value) {
  }
  var obj = JSON.parse(JSON.stringify(model.value));
  if (obj.symbol) {
    obj.symbol_id = obj.symbol.symbol_id;
  }
  let formData = new FormData();
  formData.append("model", JSON.stringify(obj));
  formData.append("selected", JSON.stringify(selectedNodes.value));
  axios
    .put(baseURL + "/api/hrm_timekeep/update_timekeep", formData, config)
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
      swal.close();
      toast.success("Cập nhật thành công!");
      closeDialog();
      initData(true);
    })
    .catch((error) => {
      swal.close();
      if (error && error.status === 401) {
        swal.fire({
          title: "Thông báo!",
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
        return;
      } else {
        swal.fire({
          title: "Thông báo!",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
        return;
      }
    });
  if (submitted.value) submitted.value = true;
};
const deleteTimekeep = (item) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá mục chấm công đã chọn không!",
      icon: "warning",
      showCancelButton: true,
      confirmButtonColor: "#3085d6",
      cancelButtonColor: "#d33",
      confirmButtonText: "Có",
      cancelButtonText: "Không",
    })
    .then((result) => {
      if (result.isConfirmed) {
        submitted.value = true;
        swal.fire({
          width: 110,
          didOpen: () => {
            swal.showLoading();
          },
        });
        let formData = new FormData();
        formData.append("selected", JSON.stringify(selectedNodes.value));
        axios
          .put(baseURL + "/api/hrm_timekeep/delete_timekeep", formData, config)
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
            swal.close();
            toast.success("Xóa thành công!");
            initDictionary();
          })
          .catch((error) => {
            swal.close();
            if (error && error.status === 401) {
              swal.fire({
                title: "Thông báo!",
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                icon: "error",
                confirmButtonText: "OK",
              });
              store.commit("gologout");
              return;
            } else {
              swal.fire({
                title: "Thông báo!",
                text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
                icon: "error",
                confirmButtonText: "OK",
              });
              return;
            }
          });
        if (submitted.value) submitted.value = true;
      }
    });
};

//function export
const exportExcel = () => {
  excel(
    "table-timekeep",
    "bangchamcong_" +
      moment(options.value["start_date"]).format("DDMMYYYY") +
      "_" +
      moment(options.value["end_date"]).format("DDMMYYYY")
  );
};
const excel = (id, name) => {
  var html = '<html xmlns:x="urn:schemas-microsoft-com:office:excel">';
  html += "<head><xml><x:ExcelWorkbook><x:ExcelWorksheets><x:ExcelWorksheet>";
  html += "<x:Name>Sheet1</x:Name>";
  html +=
    "<x:WorksheetOptions><x:Panes></x:Panes></x:WorksheetOptions></x:ExcelWorksheet>";
  html += "</x:ExcelWorksheets></x:ExcelWorkbook></xml></head><body>";
  html +=
    "<style>.cstd{font-family: Times New Roman; font-size: 21px; font-weight: 700; text-align: center; vertical-align: center;}</style><table><td colspan='" +
    (days.value.length + 3) +
    "' class='cstd'>BẢNG CHECKIN CHẤM CÔNG THÁNG '" +
    options.value.month +
    "'</td>";
  html += "</table>";
  html +=
    "<style>.cstd{font-family: Times New Roman; font-size: 16px; font-weight: 700; text-align: center; vertical-align: center;}</style><table><td colspan='" +
    (days.value.length + 3) +
    "' class='cstd'>Từ " +
    moment(options.value["start_date"]).format("DD/MM/YYYY") +
    " đến " +
    moment(options.value["end_date"]).format("DD/MM/YYYY") +
    "</td>";
  html += "</table>";
  html +=
    "<style>.cstd{font-family: Times New Roman; font-size: 21px; font-weight: 700; text-align: left; vertical-align: center;}</style>";
  html +=
    "<style>th,table,tr{font-family: Times New Roman; font-size: 18px; vertical-align: middle; text-align: left;}</style><table border='1'>";

  var htmltable = document.getElementById(id);
  html += htmltable.innerHTML;
  html += "</table></body></html>";
  var data_type = "data:application/vnd.ms-excel";
  var ua = window.navigator.userAgent;
  var msie = ua.indexOf("MSIE ");
  var fileName = name + ".xls";
  if (msie > 0 || !!navigator.userAgent.match(/Trident.*rv\:11\./)) {
    if (window.navigator.msSaveBlob) {
      var blob = new Blob([html], {
        type: "application/csv;charset=utf-8;",
      });
      navigator.msSaveBlob(blob, fileName);
    }
  } else {
    var blob2 = new Blob([html], {
      type: "application/csv;charset=utf-8;",
    });
    var filename = fileName;
    var elem = window.document.createElement("a");
    elem.href = window.URL.createObjectURL(blob2);
    elem.download = filename;
    document.body.appendChild(elem);
    elem.click();
    document.body.removeChild(elem);
  }
};

//init
const initDictionary = () => {
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_timekeep_dictionary",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
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
      if (response != null && response.data != null) {
        var data = response.data.data;
        if (data != null) {
          let tbs = JSON.parse(data);
          if (tbs[0] != null && tbs[0].length > 0) {
            weeks.value = tbs[0];
          } else {
            weeks.value = [];
          }
          if (tbs[1] != null && tbs[1].length > 0) {
            var temp = [];
            addToArray(temp, tbs[1], null, 0, "is_order");
            tbs[1] = temp;
          }
          tbs[1].unshift({ organization_id: null, newname: "Tất cả" });
          dictionarys.value = tbs;
          changeView(options.value.view);
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
const initDay = (start_date, end_date) => {
  days.value = [];
  var datebetweens = bindDateBetweenFirstAndLast(
    new Date(start_date),
    new Date(end_date)
  );
  datebetweens.forEach((day, i) => {
    days.value.push({
      day: day,
      day_name: getDayDate(day).day_name,
      day_name_short: getDayDate(day).day_name_short,
      day_string: moment(day).format("DD/MM/YYYY"),
      day_string_short: moment(day).format("DD/MM"),
      day_string_date: moment(day).format("DD"),
      is_today:
        moment(day).format("DD/MM/YYYY") ==
        moment(new Date()).format("DD/MM/YYYY"),
      is_holiday: day.getDay() === 0 || day.getDay() === 6,
    });
  });
  days.value = days.value.sort(function (a, b) {
    return new Date(a["day"]) - new Date(b["day"]);
  });
};
const initData = (ref) => {
  selectedNodes.value = [];
  datas.value = [];
  if (ref) {
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
  }
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_timekeep_list",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "start_date", va: options.value.start_date },
              { par: "end_date", va: options.value.end_date },
              { par: "search", va: options.value.search },
              {
                par: "filter_organization_id",
                va: options.value.filter_organization_id,
              },
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
        let data = JSON.parse(response.data.data);
        if (data != null) {
          if (data[0] != null && data[0].length > 0) {
            data[0].forEach((user, i) => {
              user["STT"] = i + 1;
              if (user["days"] != null) {
                user["days"] = JSON.parse(user["days"]);
                user["days"].forEach((day) => {
                  day["workday"] = new Date(day["workday"]);
                  day["workday_string"] = moment(day["workday"]).format(
                    "DD/MM/YYYY"
                  );
                });
              } else {
                user["days"] = [];
              }
              user["list_days"] = JSON.parse(JSON.stringify(days.value));
              user["list_days"].forEach((d) => {
                d["status_name"] = "";
                if (new Date(d["day"]).getTime() < new Date().getTime()) {
                  d["status_name"] = "";
                }
                var filterDays = user["days"].filter(
                  (x) => x["workday_string"] === d["day_string"]
                );
                filterDays.forEach((day) => {
                  var idx = dictionarys.value[2].findIndex(
                    (s) => s["symbol_id"] === parseInt(day["symbol_id"])
                  );
                  if (idx !== -1) {
                    d["status_name"] = dictionarys.value[2][idx]["symbol_code"];
                  }
                });
              });
            });
            datas.value = data[0];
          } else {
            datas.value = [];
          }
        }
      }
      if (isFirst.value) isFirst.value = false;
      swal.close();
      if (options.value.loading) options.value.loading = false;
    })
    .catch((error) => {
      swal.close();
      if (options.value.loading) options.value.loading = false;
      if (error && error.status === 401) {
        swal.fire({
          title: "Thông báo!",
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
        return;
      } else {
        swal.fire({
          title: "Thông báo!",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
        return;
      }
    });
};
const refresh = () => {
  selectedNodes.value = [];
  initData(true);
};
onMounted(() => {
  if (
    store.getters.user.is_super ||
    store.getters.user.is_admin ||
    store.getters.user.role_code === "admin"
  ) {
    isFunction.value = true;
  }
  isFunction.value = true;
  initDictionary();
});
</script>
<template>
  <div class="surface-100 p-3" style="overflow: hidden">
    <Toolbar class="outline-none surface-0 border-none pb-0">
      <template #start>
        <div>
          <h3 class="module-title m-0">
            <i class="pi pi-check"></i> Bảng chấm công nhân sự (từ ngày
            <span v-if="options.start_date">{{
              moment(options.start_date).format("DD/MM/YYYY")
            }}</span>
            đến
            <span v-if="options.end_date">{{
              moment(options.end_date).format("DD/MM/YYYY")
            }}</span
            >)
          </h3>
        </div>
      </template>
      <template #end>
        <Button
          v-if="selectedNodes.length > 0"
          @click="openUpdateDialog('Chọn loại chấm công')"
          label="Chọn loại chấm công"
          icon="pi pi-check"
          class="mr-2"
        />
        <Button
          v-if="selectedNodes.length > 0"
          :label="'Bỏ chọn (' + selectedNodes.length + ')'"
          icon="pi pi-times"
          class="mr-2 p-button-outlined p-button-danger"
          @click="removeSelectedNodes()"
        />
        <Button
          v-if="selectedNodes.length > 0"
          label="Xóa chấm công"
          icon="pi pi-trash"
          class="mr-2 p-button-outlined p-button-danger"
          @click="deleteTimekeep()"
        />
        <Button
          @click="exportExcel()"
          class="mr-2 p-button-outlined p-button-secondary"
          icon="pi pi-file-excel"
          label="Xuất Excel"
        />
        <Button
          @click="refresh()"
          class="p-button-outlined p-button-secondary"
          icon="pi pi-refresh"
          label="Tải lại"
        />
      </template>
    </Toolbar>
    <Toolbar class="outline-none surface-0 border-none">
      <template #start>
        <span class="p-input-icon-left mr-2">
          <i class="pi pi-search" />
          <InputText
            @keypress.enter="search()"
            v-model="options.search"
            type="text"
            spellcheck="false"
            placeholder="Tìm kiếm"
            class="ip36 input-search"
          />
        </span>
        <div class="form-group m-0">
          <Dropdown
            :options="dictionarys[1]"
            :filter="true"
            :showClear="true"
            :editable="false"
            v-model="options.filter_organization_id"
            @change="search()"
            optionLabel="newname"
            optionValue="organization_id"
            placeholder="Chọn đơn vị, phòng ban"
            :style="{ minWidth: '200px', minHeight: '36px' }"
          />
        </div>
      </template>
      <template #end>
        <div class="form-group m-0 mr-2" v-if="options.view !== 0">
          <Calendar
            v-model="options.tempyear"
            @date-select="goYear(options.tempyear)"
            :showIcon="false"
            :manualInput="false"
            inputId="yearpicker"
            view="year"
            dateFormat="'Năm' yy"
            placeholder="Chọn năm"
            class="ip36"
          />
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
                class="country-item country-item-value format-center"
                v-if="slotProps.value"
              >
                <i class="pi pi-calendar mr-2"></i>
                <div style="color: #495057">
                  Tháng {{ slotProps.value }} ({{
                    moment(new Date(options["start_date"])).format("DD/MM")
                  }}
                  -
                  {{ moment(new Date(options["end_date"])).format("DD/MM") }})
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
                class="country-item country-item-value format-center"
                v-if="slotProps.value"
              >
                <i class="pi pi-calendar mr-2"></i>
                <div>
                  Tuần {{ slotProps.value }} ({{
                    moment(new Date(options["start_date"])).format("DD/MM")
                  }}
                  -
                  {{ moment(new Date(options["end_date"])).format("DD/MM") }})
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
        <div class="form-group m-0 mr-2" v-if="options.view === 0">
          <Calendar
            inputId="basic"
            v-model="options.start_date"
            autocomplete="off"
            class="ip36"
            :showIcon="true"
            :manualInput="false"
            @date-select="changeStartDate()"
            placeholder="Từ ngày"
          />
        </div>
        <div class="form-group m-0" v-if="options.view === 0">
          <Calendar
            inputId="basic"
            v-model="options.end_date"
            autocomplete="off"
            class="ip36"
            :showIcon="true"
            :manualInput="false"
            @date-select="changeEndDate()"
            placeholder="Đến ngày"
          />
        </div>
      </template>
    </Toolbar>
    <div class="box-table gridline-custom scrollable-both-custom">
      <table id="table-timekeep" class="table-custom">
        <thead class="thead-custom">
          <tr>
            <th
              rowspan="3"
              class="sticky text-center"
              :style="{ left: '0', top: '0', width: '50px' }"
            >
              STT
            </th>
            <th
              rowspan="3"
              class="sticky text-center"
              :style="{ left: '50px', top: '0', width: '200px' }"
            >
              HỌ VÀ TÊN
            </th>
            <th
              :colspan="days.length"
              class="text-center"
              :style="{ width: days.length * 50 + 'px' }"
            >
              NGÀY CÔNG TRONG THÁNG
            </th>
            <th rowspan="3" class="text-center" :style="{ width: '100px' }">
              NGHỈ PHÉP
            </th>
          </tr>
          <tr>
            <th
              class="text-center"
              v-for="(day, index) in days"
              :key="index"
              :style="{ width: '50px' }"
              :class="{ isHoliday: day.is_holiday }"
            >
              {{ day.day_name_short }}
            </th>
          </tr>
          <tr>
            <th
              class="text-center"
              v-for="(day, index) in days"
              :key="index"
              :style="{ width: '50px' }"
              :class="{ isHoliday: day.is_holiday }"
            >
              {{ day.day_string_date }}
            </th>
          </tr>
        </thead>
        <tbody class="tbody-custom">
          <tr v-for="(user, user_key) in datas" :key="user_key">
            <td
              class="sticky"
              :style="{
                left: '0',
                width: '50px',
                background: '#f8f9fa',
                textAlign: 'center',
              }"
            >
              {{ user.STT }}
            </td>
            <td
              class="sticky text-left"
              :style="{
                left: '50px',
                width: '200px',
                background: 'antiquewhite',
              }"
            >
              {{ user.full_name }}
            </td>
            <td
              v-for="(day, day_key) in user.list_days"
              :key="day_key"
              @click="selectNode(user, day)"
              :class="{
                isHoliday: day.is_holiday,
                'btn-hover-true': isFunction,
                'btn-selected': day.selected,
              }"
              class="relative"
              :style="{ textAlign: 'center', backgroundColor: day.is_holiday ? '#f1948a' : '' }"
            >
              <div v-if="!day.selected">{{ day.status_name }}</div>
              <div v-if="day.selected" class="icon-selected">
                <i class="pi pi-check-circle"></i>
              </div>
            </td>
            <td :style="{ textAlign: 'center' }">{{ user.total_p }}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>

  <!--Dialog-->
  <Dialog
    :header="headerDialog"
    v-model:visible="displayDialog"
    :style="{ width: '30vw' }"
    :maximizable="true"
    :closable="false"
    style="z-index: 9000"
  >
    <form @submit.prevent="" name="submitform">
      <div class="grid formgrid m-2">
        <div class="col-12 md:col-12">
          <div class="form-group">
            <label>Loại chấm công <span class="redsao">(*)</span></label>
            <Dropdown
              :options="dictionarys[2]"
              :filter="true"
              :showClear="false"
              :editable="false"
              v-model="model.symbol"
              optionLabel="symbol_name"
              placeholder="Chọn loại chấm công"
              class="ip36"
              :class="{
                'p-invalid': !model.symbol && submitted,
              }"
            >
              <template #value="slotProps">
                <div
                  class="country-item country-item-value"
                  v-if="slotProps.value"
                >
                  <span
                    >{{ slotProps.value.symbol_name }} ({{
                      slotProps.value.symbol_code
                    }})</span
                  >
                </div>
                <span v-else>
                  {{ slotProps.placeholder }}
                </span>
              </template>
              <template #option="slotProps">
                <div class="country-item">
                  <span class="mr-2"
                    >{{ slotProps.option.symbol_name }} ({{
                      slotProps.option.symbol_code
                    }})</span
                  >
                </div>
              </template>
            </Dropdown>
            <div v-if="!model.symbol && submitted">
              <small class="p-error">
                <span class="col-12 p-0"
                  >Loại chấm công không được để trống</span
                >
              </small>
            </div>
          </div>
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="closeDialog()"
        class="p-button-text"
      />
      <Button label="Lưu" icon="pi pi-check" @click="saveTimekeep()" />
    </template>
  </Dialog>
</template>
<style scoped>
.box-table {
  height: calc(100vh - 180px) !important;
  background-color: #fff;
  overflow: auto;
}
.thead-custom > tr > th + th {
  border-left-width: 0;
}
.tbody-custom > tr + tr > td,
.tbody-custom > tr:first-child > td {
  border-top-width: 0;
}
.tbody-custom > tr > td + td {
  border-left-width: 0;
}
.table-custom {
  border-collapse: collapse;
  width: 100%;
  table-layout: fixed;
}
.thead-custom {
  position: sticky;
  top: 0;
  z-index: 2;
}
.thead-custom > tr > th {
  padding: 1rem;
  border: 1px solid #e9ecef;
  border-width: 0 0 1px 0;
  font-weight: 600;
  color: #495057;
  background: #f8f9fa;
  transition: box-shadow 0.2s;
}
.thead-custom > tr > th {
  border-width: 1px;
}
.tbody-custom > tr > td {
  border: 1px solid #e9ecef;
  border-width: 0 0 1px 0;
  padding: 1rem;
}
.tbody-custom > tr > td {
  border-width: 1px;
}

.scrollable-both-custom .thead-custom > tr > th,
.scrollable-both-custom .tbody-custom > tr > td,
.scrollable-both-custom .tfoot-custom > tr > td,
.p-datatable-scrollable-horizontal
  .p-datatable-thead
  > tr
  > th
  .p-datatable-scrollable-horizontal
  .p-datatable-tbody
  > tr
  > td,
.p-datatable-scrollable-horizontal .p-datatable-tfoot > tr > td {
  -webkit-box-flex: 0;
  -ms-flex: 0 0 auto;
  flex: 0 0 auto;
}
.sticky {
  display: sticky;
}
th.sticky {
  z-index: 2;
}
td.sticky {
  z-index: 1;
}
.btn-hover-true:hover {
  cursor: pointer;
  background: aliceblue;
  color: #000;
}
.btn-selected {
  background: aliceblue !important;
}
.icon-selected {
  transition: 0.5s ease;
  opacity: 1;
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  -ms-transform: translate(-50%, -50%);
  text-align: center;
}
.icon-selected > i {
  color: blue;
  font-size: 16px;
}
.style-0 {
  background-color: #52be80;
  color: #fff;
}
.style-1 {
  background-color: #eb984e;
  color: #fff;
}
.style-2 {
  background-color: #af7ac5;
  color: #fff;
}
.style-3 {
  background-color: #ec7063;
  color: #fff;
}

th.isHoliday {
  background-color: #f1948a !important;
  color: #fff !important;
}
.isHoliday {
  background-color: #f1948a;
  color: #fff;
}

.form-group {
  display: grid;
  margin-bottom: 1rem;
  flex: 1;
}

.form-group > label {
  margin-bottom: 0.5rem;
}

.form-group .p-multiselect .p-multiselect-label,
.form-group .p-dropdown .p-dropdown-label,
.form-group .p-treeselect .p-treeselect-label {
  height: 100%;
  display: flex;
  align-items: center;
}
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
</style>
