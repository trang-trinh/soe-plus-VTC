<script setup>
import { onMounted, inject, ref } from "vue";
import { useToast } from "vue-toastification";
import { encr } from "../../../../util/function";
import moment from "moment";

const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios");
const cryoptojs = inject("cryptojs");
const toast = useToast();
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const basedomainURL = baseURL;

//Get arguments
const props = defineProps({
  key: Number,
  profile_id: String,
});

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
});
const isFirst = ref(true);
const checkedAll = ref(false);
const selectedNodes = ref([]);
const datas = ref([]);

//Declare dictionary
const dictionarys = ref([]);
const department = ref([]);
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

//Filter
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
  options.value.year = date.getFullYear();
  initDictionary();
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
            var temp1 = [];
            addToArray(temp1, tbs[1], null, 0, "is_order");
            tbs[1] = temp1;
          }
          tbs[1].unshift({ organization_id: null, newname: "Tất cả" });
          if (tbs[3] != null && tbs[3].length > 0) {
            department.value = JSON.parse(JSON.stringify(tbs[3]));
            var temp2 = [];
            addToArray(
              temp2,
              department.value,
              options.value.filter_organization_id,
              0,
              "is_order"
            );
            department.value = temp2;
          }
          dictionarys.value = tbs;
          changeView(options.value.view);
        }
      }
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
      is_pass: new Date() > day,
    });
  });
  days.value = days.value.sort(function (a, b) {
    return new Date(a["day"]) - new Date(b["day"]);
  });
};
const refresh = () => {
  initData(true);
};
const initData = (rf) => {};
onMounted(() => {
  initDictionary();
});
</script>
<template>
  <Toolbar>
    <template #start>
      <div class="form-group m-0 mr-2" v-if="options.view !== 0">
        <Calendar
          v-model="options.tempyear"
          @date-select="goYear(options.tempyear)"
          :showIcon="true"
          inputId="yearpicker"
          view="year"
          dateFormat="yy"
          placeholder="Chọn năm"
          class="ip36"
          style="min-width: 170px"
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
              class="country-item country-item-value flex"
              v-if="slotProps.value"
            >
              <i class="pi pi-calendar mr-2 format-flex-center"></i>
              <div>
                Tháng {{ slotProps.value }} 
                <!-- ({{
                  moment(new Date(options["week_start_date"])).format("DD/MM")
                }}
                -
                {{
                  moment(new Date(options["week_end_date"])).format("DD/MM")
                }}) -->
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
    </template>
    <template #end>
      <Button
        label="30"
        class="mr-2"
        style="background-color: #0078d4; color: #fff; border-radius: 10px"
        v-tooltip.top="'Ngày đi làm'"
      />
      <Button
        label="30"
        style="background-color: #0078d4; color: #fff; border-radius: 10px"
        v-tooltip.top="'Tống số ngày'"
      />
    </template>
  </Toolbar>
  <div
    class="d-lang-table p-2"
    :style="{
      height: 'calc(100vh - 220px)',
      overflowY: 'auto',
    }"
  >
    <div class="list-item">
      <div
        v-for="(day, day_index) in days"
        class="item-day"
        :class="{ isPass: day.is_pass, isToday: day.is_today }"
      >
        <div class="flex justify-content-between">
          <div>{{ day.day_name }}</div>
          <div>{{ day.day_string_date }}</div>
        </div>
        <div class="format-center h-full">
          <span :style="{ fontSize: '16px' }">P</span>
        </div>
      </div>
    </div>
  </div>
</template>
<style scoped>
@import url(./stylehrm.css);
.list-item {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(110px, 1fr));
  gap: 0.5rem;
}
.item-day {
  border: 1px solid rgba(0, 0, 0, 0.1);
  border-radius: 3px;
  height: 110px;
  padding: 0.5rem;
}
.item-day:hover {
  cursor: pointer;
  background-color: aliceblue;
  color: #495057;
}

.isPass {
  background-color: #eee;
  color: #495057;
}
.isToday {
  background-color: #6fd234;
  color: #fff;
}
</style>
