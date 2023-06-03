<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import { encr, checkURL } from "../../../util/function.js";
import {
  CalendarView,
  CalendarViewHeader,
  CalendarMath,
} from "vue-simple-calendar";
import "../../../../node_modules/vue-simple-calendar/dist/style.css";
// The next two lines are optional themes
import "../../../../node_modules/vue-simple-calendar/dist/css/default.css";
import "../../../../node_modules/vue-simple-calendar/dist/css/holidays-us.css";
import moment from "moment";
import { Tooltip } from "chart.js";
//Khai báo

const cryoptojs = inject("cryptojs");
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const isDynamicSQL = ref(false);
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const filters = ref({});

//Lấy số bản ghi
const loadCount = () => {
  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_holiday_dates_count",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "status", va: null },
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
        options.value.totalRecords = data[0].totalRecords;
        sttStamp.value = data[0].totalRecords + 1;
      }
    })
    .catch((error) => {});
};
//Lấy dữ liệu holiday_dates
const loadData = (rf) => {
  if (rf) {
    loadCountCal(state.value.showDate);
  }
  state.value.items = [];
  (async () => {
  await axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_holiday_dates_list_all",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "status", va: true },
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
      if (isFirst.value) isFirst.value = false;

      data.forEach((element, i) => {
        element.STT = options.value.PageNo * options.value.PageSize + i + 1;

        if (element.start_date) {
          if (element.text_color)
            element.text_color = "color:" + element.text_color;
          else element.text_color = "";
          if (element.background_color)
            element.background_color =
              "; background-color:" + element.background_color;
          else element.background_color = "";
          state.value.items.push({
            id: "nothingshow",
            id_hol: element.holiday_dates_id,
            holiday_type_id: element.holiday_type_id,
            startDate: new Date(element.start_date),
            endDate: element.end_date
              ? new Date(element.end_date)
              : new Date(element.start_date),
            title:
              " <span class='text-sm'> <i class='" +
              element.icon +
              " px-1' ></i>" +
              " </span>   <span>" +
              element.reason +
              "</span>",
            tooltip: element.holiday_type_name,
            style: element.text_color + element.background_color,
          });
        }
      });
 

      options.value.loading = false;
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;

      if (error && error.status === 401) {
        swal.fire({
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
    await axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_work_schedule_user_list",
            par: [{ par: "user_id", va: store.getters.user.user_id }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];

      data.forEach((element, i) => {
        var txt_color = "";
        if (element.background_color) txt_color = "color:" + element.text_color;
        var bg_color = "";
        if (element.background_color)
          bg_color = "; background-color:" + element.background_color;

        element.text_color = txt_color;
        element.background_color = bg_color;
        if (!element.is_full_time) {
          if (element.work_schedule_days) {
            element.work_schedule_days.split(",").forEach((item, idx) => {
              state.value.items.push({
                id: element.work_schedule_id,
                startDate: new Date(item),
                endDate: new Date(item),
                title: element.declare_shift_name,
                tooltip: " ",
                declare_shift_id: element.declare_shift_id,
                style: element.text_color + element.background_color,
              });
            });
          }
          if (element.work_schedule_months) {
            element.work_schedule_months.split(",").forEach((item, idx) => {
              var date = new Date(item);
              var numDays = new Date(
                date.getFullYear(),
                date.getMonth() + 1,
                0
              ).getDate();

              for (let index = 1; index <= numDays; index++) {
                state.value.items.push({
                  id: element.work_schedule_id,
                  startDate: new Date(
                    date.getFullYear(),
                    date.getMonth(),
                    index
                  ),
                  endDate: new Date(date.getFullYear(), date.getMonth(), index),
                  title: element.declare_shift_name, declare_shift_id: element.declare_shift_id,
                  tooltip: " ",
                  style: element.text_color + element.background_color,
                });
              }
            });
          }
        } else {
          for (let indexs = 0; indexs < 12; indexs++) {
            var date = new Date();
            var numDays = new Date(date.getFullYear(), indexs, 0).getDate();
            var newDate = new Date(date.getFullYear(), indexs, 0);
            for (let index = 1; index <= numDays; index++) {
              state.value.items.push({
                id: element.work_schedule_id,
                startDate: new Date(
                  newDate.getFullYear(),
                  newDate.getMonth(),
                  index
                ),
                endDate: new Date(
                  newDate.getFullYear(),
                  newDate.getMonth(),
                  index
                ),
                title: element.declare_shift_name, declare_shift_id: element.declare_shift_id,
                tooltip: " ",
                style: element.text_color + element.background_color,
              });
            }
          }
        }
      });
      options.value.loading = false;
      itemSave=[... state.value.items ]
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;

      if (error && error.status === 401) {
        swal.fire({
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
  })()

}
//Phân trang dữ liệu
const onPage = (event) => {
  if (event.rows != options.value.PageSize) {
    options.value.PageSize = event.rows;
  }
  if (event.page == 0) {
    //Trang đầu
    options.value.id = null;
    options.value.IsNext = true;
  } else if (event.page > options.value.PageNo + 1) {
    //Trang cuối
    options.value.id = -1;
    options.value.IsNext = false;
  } else if (event.page > options.value.PageNo) {
    //Trang sau

    options.value.id =
      datalists.value[datalists.value.length - 1].holiday_dates_id;
    options.value.IsNext = true;
  } else if (event.page < options.value.PageNo) {
    //Trang trước
    options.value.id = datalists.value[0].holiday_dates_id;
    options.value.IsNext = false;
  }
  options.value.PageNo = event.page;
  loadData(true);
};

const holiday_dates = ref({
  emote_file: "",
  status: true,
  is_order: 1,
});

const selectedStamps = ref();
const submitted = ref(false);

const isSaveTem = ref(false);
const datalists = ref();
const toast = useToast();
const basedomainURL = baseURL;
const checkDelList = ref(false);

const options = ref({
  IsNext: true,
  sort: "created_date",
  SearchText: "",
  PageNo: 0,
  PageSize: 20,
  loading: true,
  totalRecords: null,
  displayPeriodCount: 1,
  declareShiftCount: 0,
  holidayDatesCount: 0,
});
const setShowDate = (d) => {
  state.value.showDate = d;
  state.value.items = [];

  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_holiday_dates_list_all",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "status", va: null },
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
      if (isFirst.value) isFirst.value = false;

      data.forEach((element, i) => {
        element.STT = options.value.PageNo * options.value.PageSize + i + 1;

        if (element.start_date) {
          if (element.text_color)
            element.text_color = "color:" + element.text_color;
          else element.text_color = "";
          if (element.background_color)
            element.background_color =
              "; background-color:" + element.background_color;
          else element.background_color = "";
          state.value.items.push({
            id: "nothingshow",
            id_hol: element.holiday_dates_id,
            holiday_type_id: element.holiday_type_id,
            startDate: new Date(element.start_date),
            endDate: element.end_date
              ? new Date(element.end_date)
              : new Date(element.start_date),
            title:
              " <span class='text-sm'> <i class='" +
              element.icon +
              " px-1' ></i>" +
              " </span>   <span>" +
              element.reason +
              "</span>",
            tooltip:  element.holiday_type_name,
            style: element.text_color + element.background_color,
          });
        }
      });
      options.value.loading = false;
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;

      if (error && error.status === 401) {
        swal.fire({
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_work_schedule_user_list",
            par: [{ par: "user_id", va: store.getters.user.user_id }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];

      data.forEach((element, i) => {
        var txt_color = "";
        if (element.background_color) txt_color = "color:" + element.text_color;
        var bg_color = "";
        if (element.background_color)
          bg_color = "; background-color:" + element.background_color;

        element.text_color = txt_color;
        element.background_color = bg_color;
        if (!element.is_full_time) {
          if (element.work_schedule_days) {
            element.work_schedule_days.split(",").forEach((item, idx) => {
              state.value.items.push({
                id: element.work_schedule_id,
                startDate: new Date(item),
                endDate: new Date(item),
                title: element.declare_shift_name,
                tooltip: " ",
                declare_shift_id: element.declare_shift_id,
                style: element.text_color + element.background_color,
              });
            });
          }
          if (element.work_schedule_months) {
            element.work_schedule_months.split(",").forEach((item, idx) => {
              var date = new Date(item);
              var numDays = new Date(
                date.getFullYear(),
                date.getMonth() + 1,
                0
              ).getDate();

              for (let index = 1; index <= numDays; index++) {
                state.value.items.push({
                  id: element.work_schedule_id,
                  startDate: new Date(
                    date.getFullYear(),
                    date.getMonth(),
                    index
                  ),
                  endDate: new Date(date.getFullYear(), date.getMonth(), index),
                  title: element.declare_shift_name,
                  tooltip: " ", declare_shift_id: element.declare_shift_id,
                  style: element.text_color + element.background_color,
                });
              }
            });
          }
        } else {
          for (let indexs = 0; indexs < 12; indexs++) {
            var date = d;
            var numDays = new Date(date.getFullYear(), indexs, 0).getDate();
            var newDate = new Date(date.getFullYear(), indexs, 0);
            for (let index = 1; index <= numDays; index++) {
              state.value.items.push({
                id: new Date(
                  newDate.getFullYear(),
                  newDate.getMonth(),
                  index
                ).toString(),
                startDate: new Date(
                  newDate.getFullYear(),
                  newDate.getMonth(),
                  index
                ),
                endDate: new Date(
                  newDate.getFullYear(),
                  newDate.getMonth(),
                  index
                ),
                title: element.declare_shift_name,  declare_shift_id: element.declare_shift_id,
                tooltip: " ",
                style: element.text_color + element.background_color,
              });
            }
          }
        }
      });
      options.value.loading = false;
      itemSave=[... state.value.items ]
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;

      if (error && error.status === 401) {
        swal.fire({
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
  loadCountCal(state.value.showDate);
};
function getWeekNumber(date) {
  const oneJan = new Date(date.getFullYear(), 0, 1);
  const millisecsInDay = 86400000;
  return Math.ceil(((date - oneJan) / millisecsInDay + oneJan.getDay()) / 7);
}
//Hiển thị dialog
const loadCountCal = (dateCount) => {
  options.value.declareShiftCount = 0;
  options.value.holidayDatesCount = 0;
  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_holiday_dates_list_all",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "status", va: null },
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
      data.forEach((element, i) => {
        if (element.start_date) {
          if (state.value.displayPeriodUom == "week") {
            if (
              getWeekNumber(new Date(element.start_date)) ==
                getWeekNumber(dateCount) - 1 &&
              new Date(element.start_date).getFullYear() ==
                dateCount.getFullYear()
            ) {
              let date = new Date(element.start_date); // Tạo đối tượng Date
              let dayOfWeek = date.getDay(); // Lấy thứ của ngày đó (trả về số từ 0 - 6, 0 là chủ nhật, 1 là thứ 2, ... , 6 là thứ 7)
              let daysLeftInWeek = 7 - dayOfWeek; // Số ngày còn lại để đến ngày cuối cùng của tuần (6 nếu tuần bắt đầu vào chủ nhật)
              let lastDayOfWeek = new Date(
                date.getTime() + daysLeftInWeek * 24 * 60 * 60 * 1000
              ); // Tạo đối tượng Date mới với ngày cuối cùng của tuần
              if (!element.end_date) element.end_date = element.start_date;
              let timeDiff = 0;
              if (
                new Date(element.end_date).getTime() > lastDayOfWeek.getTime()
              )
                timeDiff =
                  new Date(element.end_date).getTime() -
                  lastDayOfWeek.getTime();

              let daysDiff = Math.round(timeDiff / (1000 * 60 * 60 * 24));
              options.value.holidayDatesCount += daysDiff;
            }
            if (
              getWeekNumber(new Date(element.start_date)) ==
                getWeekNumber(dateCount) &&
              new Date(element.start_date).getFullYear() ==
                dateCount.getFullYear()
            ) {
              let date = new Date(element.start_date); // Tạo đối tượng Date

              let dayOfWeek = date.getDay(); // Lấy thứ của ngày đó (trả về số từ 0 - 6, 0 là chủ nhật, 1 là thứ 2, ... , 6 là thứ 7)

              let daysLeftInWeek = 7 - dayOfWeek; // Số ngày còn lại để đến ngày cuối cùng của tuần (6 nếu tuần bắt đầu vào chủ nhật)

              let lastDayOfWeek = new Date(
                date.getTime() + daysLeftInWeek * 24 * 60 * 60 * 1000
              ); // Tạo đối tượng Date mới với ngày cuối cùng của tuần
              if (!element.end_date) element.end_date = element.start_date;
              let timeDiff = 0;
              if (
                new Date(element.end_date).getTime() > lastDayOfWeek.getTime()
              )
                timeDiff =
                  lastDayOfWeek.getTime() -
                  new Date(element.start_date).getTime();
              else
                timeDiff =
                  new Date(element.end_date).getTime() -
                  new Date(element.start_date).getTime();
              let daysDiff = Math.round(timeDiff / (1000 * 60 * 60 * 24)) + 1;
              options.value.holidayDatesCount += daysDiff;
            }
          }
          if (state.value.displayPeriodUom == "month") {
            if (
              new Date(element.start_date).getMonth() == dateCount.getMonth() &&
              new Date(element.start_date).getFullYear() ==
                dateCount.getFullYear()
            ) {
              let date = new Date(element.start_date); // Tạo đối tượng Date
              let month = date.getMonth(); // Lấy tháng của ngày đó (trả về số từ 0 - 11, 0 là tháng 1, 1 là tháng 2, ... , 11 là tháng 12)
              let year = date.getFullYear(); // Lấy năm của ngày đó
              let firstDayOfNextMonth = new Date(year, month + 1, 1); // Tạo đối tượng Date mới với ngày đầu tiên của tháng tiếp theo
              let lastDayOfMonth = new Date(
                firstDayOfNextMonth.getTime() - 24 * 60 * 60 * 1000
              ); // Trừ đi 1 ngày từ ngày đầu tiên của tháng tiếp theo để tính ra ngày cuối cùng của tháng hiện tại

              if (!element.end_date) element.end_date = element.start_date;
              let timeDiff = 0;
              if (
                new Date(element.end_date).getTime() > lastDayOfMonth.getTime()
              )
                timeDiff =
                  lastDayOfMonth.getTime() -
                  new Date(element.start_date).getTime();
              else
                timeDiff =
                  new Date(element.end_date).getTime() -
                  new Date(element.start_date).getTime();
              let daysDiff = Math.round(timeDiff / (1000 * 60 * 60 * 24)) + 1;
              options.value.holidayDatesCount += daysDiff;
            }
          }
          if (state.value.displayPeriodUom == "year") {
            if (
              new Date(element.start_date).getFullYear() ==
              dateCount.getFullYear()
            ) {
              let date = new Date(element.start_date); // Tạo đối tượng Date

              const year = date.getFullYear(); // Lấy năm của ngày đó

              const firstDayOfNextYear = new Date(year + 1, 0, 1); // Tạo đối tượng Date mới với ngày đầu tiên của năm tiếp theo

              const lastDayOfYear = new Date(
                firstDayOfNextYear.getTime() - 24 * 60 * 60 * 1000
              ); // Trừ đi 1 ngày từ ngày đầu tiên của năm tiếp theo để tính ra ngày cuối cùng của năm hiện tại

              if (!element.end_date) element.end_date = element.start_date;
              let timeDiff = 0;
              if (
                new Date(element.end_date).getTime() > lastDayOfYear.getTime()
              )
                timeDiff =
                  lastDayOfYear.getTime() -
                  new Date(element.start_date).getTime();
              else
                timeDiff =
                  new Date(element.end_date).getTime() -
                  new Date(element.start_date).getTime();
              let daysDiff = Math.round(timeDiff / (1000 * 60 * 60 * 24)) + 1;
              options.value.holidayDatesCount += daysDiff;
            }
          }
        }
      });
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;

      if (error && error.status === 401) {
        swal.fire({
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_work_schedule_user_list",
            par: [{ par: "user_id", va: store.getters.user.user_id }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];

      data.forEach((element, i) => {
        if (!element.is_full_time) {
          if (element.work_schedule_days) {
            element.work_schedule_days.split(",").forEach((item, idx) => {
              if (state.value.displayPeriodUom == "week") {
                if (
                  getWeekNumber(new Date(item)) == getWeekNumber(dateCount) &&
                  new Date(item).getFullYear() == dateCount.getFullYear()
                ) {
                  options.value.declareShiftCount++;
                }
              }
              if (state.value.displayPeriodUom == "month") {
                if (
                  new Date(item).getMonth() == dateCount.getMonth() &&
                  new Date(item).getFullYear() == dateCount.getFullYear()
                ) {
                  options.value.declareShiftCount++;
                }
              }
              if (state.value.displayPeriodUom == "year") {
                if (new Date(item).getFullYear() == dateCount.getFullYear()) {
                  options.value.declareShiftCount++;
                }
              }
            });
          }
          if (element.work_schedule_months) {
            element.work_schedule_months.split(",").forEach((item, idx) => {
              var date = new Date(item);
              var numDays = new Date(
                date.getFullYear(),
                date.getMonth() + 1,
                0
              ).getDate();

              for (let index = 1; index <= numDays; index++) {
                if (state.value.displayPeriodUom == "week") {
                  if (
                    getWeekNumber(
                      new Date(date.getFullYear(), date.getMonth(), index)
                    ) == getWeekNumber(dateCount) &&
                    new Date(item).getFullYear() == dateCount.getFullYear()
                  ) {
                    options.value.declareShiftCount++;
                  }
                }

                if (state.value.displayPeriodUom == "month") {
                  if (
                    new Date(item).getMonth() == dateCount.getMonth() &&
                    new Date(item).getFullYear() == dateCount.getFullYear()
                  ) {
                    options.value.declareShiftCount++;
                  }
                }
                if (state.value.displayPeriodUom == "year") {
                  if (new Date(item).getFullYear() == dateCount.getFullYear()) {
                    options.value.declareShiftCount++;
                  }
                }
              }
            });
          }
        } else {
          for (let indexs = 0; indexs < 12; indexs++) {
            var date = dateCount;
            var numDays = new Date(date.getFullYear(), indexs, 0).getDate();
            var newDate = new Date(date.getFullYear(), indexs, 0);
            for (let index = 1; index <= numDays; index++) {
              var dateCheck = new Date(newDate.getFullYear(), indexs, index);
              if (state.value.displayPeriodUom == "week") {
                if (
                  getWeekNumber(dateCheck) == getWeekNumber(dateCount) &&
                  new Date(
                    new Date(newDate.getFullYear(), newDate.getMonth(), index)
                  ).getFullYear() == dateCount.getFullYear()
                ) {
                  options.value.declareShiftCount++;
                }
              }
              if (state.value.displayPeriodUom == "month") {
                if (
                  new Date(
                    new Date(newDate.getFullYear(), newDate.getMonth(), index)
                  ).getMonth() == dateCount.getMonth() &&
                  new Date(
                    new Date(newDate.getFullYear(), newDate.getMonth(), index)
                  ).getFullYear() == dateCount.getFullYear()
                ) {
                  options.value.declareShiftCount++;
                }
              }
              if (state.value.displayPeriodUom == "year") {
                if (
                  new Date(
                    new Date(newDate.getFullYear(), newDate.getMonth(), index)
                  ).getFullYear() == dateCount.getFullYear()
                ) {
                  options.value.declareShiftCount++;
                }
              }
            }
          }
        }
      });
      options.value.loading = false;
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;

      if (error && error.status === 401) {
        swal.fire({
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
//Thêm bản ghi

const sttStamp = ref(1);

//Sort

const listOpCalendar = ref([
  { label: "Tuần", value: "week" },
  { label: "Tháng", value: "month" },
  { label: "Năm", value: "year" },
]);
const onChangeOpCal = () => {
  loadCountCal(state.value.showDate);
};
const checkFilter = ref(false);
const filterSQL = ref([]);
const isFirst = ref(true);
const loadDataSQL = () => {
  datalists.value = [];

  let data = {
    id: "holiday_dates_id",
    sqlS: filterTrangthai.value != null ? filterTrangthai.value : null,
    sqlO: options.value.sort,
    Search: options.value.SearchText,
    PageNo: options.value.PageNo,
    PageSize: options.value.PageSize,
    next: true,
    sqlF: null,
    fieldSQLS: filterSQL.value,
  };
  options.value.loading = true;
  axios
    .post(baseURL + "/api/HRM_SQL/Filter_hrm_holiday_dates", data, config)
    .then((response) => {
      let dt = JSON.parse(response.data.data);
      let data = dt[0];
      if (data.length > 0) {
        data.forEach((element, i) => {
          element.STT = options.value.PageNo * options.value.PageSize + i + 1;
        });

        datalists.value = data;
      } else {
        datalists.value = [];
      }
      if (isFirst.value) isFirst.value = false;
      options.value.loading = false;
      //Show Count nếu có
      if (dt.length == 2) {
        options.value.totalRecords = dt[1][0].totalRecords;
      }
    })
    .catch((error) => {
      options.value.loading = false;
      toast.error("Tải dữ liệu không thành công!");

      if (error && error.status === 401) {
        swal.fire({
          title: "Thông báo",
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
//Tìm kiếm
const searchStamp = (event) => {
  if (event.code == "Enter") {
    if (options.value.SearchText == "") {
      loadData(true);
    } else {
      state.value.items = state.value.items.filter((x) =>
        x.title.includes(options.value.SearchText)
      );
    }
  }
};
const refreshStamp = () => {
  options.value.SearchText = null;
  filterTrangthai.value = null;
  options.value.loading = true;
  selectedStamps.value = [];
  isDynamicSQL.value = false;
  filterSQL.value = [];
  loadData(true);
};
const onFilter = (event) => {
  filterSQL.value = [];

  for (const [key, value] of Object.entries(event.filters)) {
    if (key != "global") {
      let obj = {
        key: key,
        filteroperator: value.operator,
        filterconstraints: value.constraints,
      };

      if (value.value && value.value.length > 0) {
        obj.filteroperator = value.matchMode;
        obj.filterconstraints = [];
        value.value.forEach(function (vl) {
          obj.filterconstraints.push({ value: vl[obj.key] });
        });
      } else if (value.matchMode) {
        obj.filteroperator = "and";
        obj.filterconstraints = [value];
      }
      if (
        obj.filterconstraints &&
        obj.filterconstraints.filter((x) => x.value != null).length > 0
      )
        filterSQL.value.push(obj);
    }
  }
  options.value.PageNo = 0;
  options.value.id = null;
  isDynamicSQL.value = true;
  loadDataSQL();
};

//Filter
const listWeeks = ref([
  { name: "Thứ Hai", code: 1 },
  { name: "Thứ Ba", code: 2 },
  { name: "Thứ Tư", code: 3 },
  { name: "Thứ Năm", code: 4 },
  { name: "Thứ Sáu", code: 5 },
  { name: "Thứ Bảy", code: 6 },
  { name: "Chủ nhật", code: 0 },
]);

const filterTrangthai = ref();
const reFilterEmail1 = () => {
  options.value.filterShift=[];
   
  state.value.items=[...itemSave];
  loadCountCal(state.value.showDate);
  op1.value.hide();
};
const reFilterEmail2 = () => {
  options.value.filterHolidayType=[];
  state.value.items=[...itemSave];
  loadCountCal(state.value.showDate);
  op2.value.hide();
};
const reFilterEmail = () => {
  options.value.displayPeriodCount = 1;
  state.value.displayPeriodCount = 1;

  options.value.startingDayOfWeek = 1;
  state.value.startingDayOfWeek = 1;

  options.value.displayWeekNumbers = false;
  state.value.displayWeekNumbers = false;
  options.value.SearchText = null;
  op.value.hide();
};
const filterFileds = () => {
  state.value.displayPeriodCount = options.value.displayPeriodCount;
  state.value.startingDayOfWeek = options.value.startingDayOfWeek;
  state.value.displayWeekNumbers = options.value.displayWeekNumbers;
};
var itemSave=[];
const filterFileds1 = () => {
 
  var newARR = [];
  var ARR = [];
var arrS=[...itemSave]
 
  options.value.filterShift.forEach((element) => {
     
    ARR = arrS.filter((x) => x.declare_shift_id == element);
    newARR = newARR.concat(ARR);
  });

  // newARR = newARR.concat( arrS.filter((x) => x.id_hol!=null));
  options.value.holidayDatesCount=0;
 options.value.declareShiftCount=newARR.length;
  state.value.items = newARR;
  op1.value.hide();
};

const filterFileds2 = () => {
 
 var newARR = [];
 var ARR = [];
var arrS=[...itemSave]

 options.value.filterHolidayType.forEach((element) => {
   ARR = arrS.filter((x) => x.holiday_type_id == element);
   newARR = newARR.concat(ARR);
 });
 // newARR = newARR.concat( arrS.filter((x) => x.id_hol!=null));
 state.value.items = newARR;
 options.value.holidayDatesCount=newARR.length;
 options.value.declareShiftCount=0;
 op1.value.hide();
};

watch(selectedStamps, () => {
  if (selectedStamps.value.length > 0) {
    checkDelList.value = true;
  } else {
    checkDelList.value = false;
  }
});

const op = ref();
const toggle = (event) => {
  op.value.toggle(event);
};
const op1 = ref();
const toggle1 = (event) => {
  op1.value.toggle(event);
};
const op2 = ref();
const toggle2 = (event) => {
  op2.value.toggle(event);
};
const listHolidayType = ref([]);
const listDeclareShift = ref([]);
const initTudien = () => {
  listDeclareShift.value = [];
  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_declare_shift_list",
            par: [
              { par: "pageno", va: 0 },
              { par: "pagesize", va: 1000000 },
              { par: "user_id", va: store.getters.user.user_id },
              { par: "status", va: true },
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
      if (isFirst.value) isFirst.value = false;

      data.forEach((element, i) => {
        listDeclareShift.value.push({
          name: element.declare_shift_name,
          code: element.declare_shift_id,
          text_color: element.text_color,
          background_color: element.background_color,
        });
      });

      options.value.loading = false;
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;
    });
  listHolidayType.value = [];
  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_ca_holiday_type_list",
            par: [
              { par: "pageno", va: 0 },
              { par: "pagesize", va: 1000000 },
              { par: "user_id", va: store.getters.user.user_id },
              { par: "status", va: true },
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
      if (isFirst.value) isFirst.value = false;
      data.forEach((element, i) => {
        listHolidayType.value.push({
          name: element.holiday_type_name,
          code: element.holiday_type_id,
          text_color: element.text_color,
          background_color: element.background_color,
        });
      });

      options.value.loading = false;
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;
    });
};

const thisMonth = (d) => {
  const t = new Date();
  return new Date(t.getFullYear(), t.getMonth(), d);
};

const state = ref({
  /* Show the current month, and give it some fake items to show */
  showDate: thisMonth(1),
  message: "",
  startingDayOfWeek: 1,
  disablePast: false,
  disableFuture: false,
  displayPeriodUom: "month",
  displayPeriodCount: 1,
  displayWeekNumbers: false,
  showTimes: false,
  selectionStart: undefined,
  selectionEnd: undefined,
  newItemTitle: "",
  newItemStartDate: "",
  newItemEndDate: "",
  useDefaultTheme: true,
  useHolidayTheme: true,
  useTodayIcons: true,
  items: [
    /*{
			id: "e0",
			startDate: "2018-01-05",
		},*/
    {
      id: "e1",
      startDate: thisMonth(15, 18, 30),
    },
  ],
});

const work_schedule = ref({});

const displayBasicCal = ref(false);
const displayBasicHol = ref(false);
const closeDialogHol = () => {
  displayBasicHol.value = false;
};
const closeDialogCal = () => {
  displayBasicCal.value = false;
};
const onClickItem = (e) => {
  submitted.value = false;
  if (e.id != "nothingshow") {
    axios
      .post(
        baseURL + "/api/hrm_ca_SQL/getData",
        {
          str: encr(
            JSON.stringify({
              proc: "hrm_work_schedule_get",
              par: [{ par: "work_schedule_id", va: e.id }],
            }),
            SecretKey,
            cryoptojs
          ).toString(),
        },
        config
      )
      .then((response) => {
        let data = JSON.parse(response.data.data)[0];
        work_schedule.value = data[0];
        if (work_schedule.value.profile_id)
          work_schedule.value.profile_id_fake =
            work_schedule.value.profile_id.split(",");
        if (work_schedule.value.work_schedule_months) {
          work_schedule.value.work_schedule_monthsfake = [];
          work_schedule.value.work_schedule_months
            .split(",")
            .forEach((element) => {
              work_schedule.value.work_schedule_monthsfake.push(
                new Date(element)
              );
            });
        }
        if (work_schedule.value.work_schedule_days) {
          work_schedule.value.work_schedule_daysfake = [];
          work_schedule.value.work_schedule_days
            .split(",")
            .forEach((element) => {
              work_schedule.value.work_schedule_daysfake.push(
                new Date(element)
              );
            });
        }
        if (work_schedule.value.start_time)
          work_schedule.value.start_time = new Date(
            work_schedule.value.start_time
          );
        if (work_schedule.value.end_time)
          work_schedule.value.end_time = new Date(work_schedule.value.end_time);
        displayBasicCal.value = true;
      })
      .catch((error) => {
        toast.error("Tải dữ liệu không thành công!");
        options.value.loading = false;
      });
  } else {
    axios
      .post(
        baseURL + "/api/hrm_ca_SQL/getData",
        {
          str: encr(
            JSON.stringify({
              proc: "hrm_holiday_dates_get",
              par: [{ par: "holiday_dates_id", va: e.originalItem.id_hol }],
            }),
            SecretKey,
            cryoptojs
          ).toString(),
        },
        config
      )
      .then((response) => {
        let data = JSON.parse(response.data.data)[0];
        holiday_dates.value = data[0];

        if (holiday_dates.value.start_date)
          holiday_dates.value.start_date = new Date(
            holiday_dates.value.start_date
          );
        if (holiday_dates.value.end_date)
          holiday_dates.value.end_date = new Date(holiday_dates.value.end_date);
        displayBasicHol.value = true;
      })
      .catch((error) => {
        toast.error("Tải dữ liệu không thành công!");
        options.value.loading = false;
      });
  }
};

onMounted(() => {
  initTudien();
  loadData(true);
  return {
    datalists,
    options,
    onPage,
    loadData,
    loadCount,

    basedomainURL,

    isFirst,
    searchStamp,

    selectedStamps,
  };
});
</script>
    <template>
  <div class="calendar-parent">
    <div class="surface-0 p-2">
      <h3 class="module-title mt-0 ml-1 mb-2">
        <i class="pi pi-calendar"></i> Thông tin lịch làm việc
      </h3>
      <Toolbar class="w-full custoolbar">
        <template #start>
          <span class="p-input-icon-left">
            <i class="pi pi-search" />
            <InputText
              v-model="options.SearchText"
              @keyup="searchStamp"
              type="text"
              spellcheck="false"
              placeholder="Tìm kiếm"
            />
            <Button
              type="button"
              class="ml-2"
              icon="pi pi-filter"
              @click="toggle"
              aria:haspopup="true"
              aria-controls="overlay_panel"
              v-tooltip="'Cấu hình'"
              :class="checkFilter ? '' : 'p-button-secondary p-button-outlined'"
            />
            <OverlayPanel
              ref="op"
              appendTo="body"
              class="p-0 m-0"
              :showCloseIcon="false"
              id="overlay_panel"
              style="width: 350px"
            >
              <div class="grid formgrid m-0">
                <div class="flex field col-12 p-0">
                  <div
                    class="col-6 text-left pt-2 p-0"
                    style="text-align: left"
                  >
                    <span v-if="state.displayPeriodUom == 'month'">
                      Số tháng hiển thị</span
                    >
                    <span v-if="state.displayPeriodUom == 'year'">
                      Số năm hiển thị</span
                    >
                    <span v-if="state.displayPeriodUom == 'week'">
                      Số tuần hiển thị</span
                    >
                  </div>
                  <div class="col-6">
                    <InputNumber
                      class="col-12 p-0 m-0"
                      v-model="options.displayPeriodCount"
                      :min="1"
                    />
                  </div>
                </div>
                <div class="flex field col-12 p-0">
                  <div
                    class="col-6 text-left pt-2 p-0"
                    style="text-align: left"
                  >
                    Ngày bắt đầu
                  </div>
                  <div class="col-6">
                    <Dropdown
                      class="col-12 p-0 m-0"
                      v-model="options.startingDayOfWeek"
                      :options="listWeeks"
                      optionLabel="name"
                      optionValue="code"
                      placeholder="Chọn ngày"
                    />
                  </div>
                </div>
                <div class="flex col-12 p-0">
                  <div
                    class="col-6 text-left pt-2 p-0"
                    style="text-align: left"
                  >
                    Hiển thị số tuần
                  </div>
                  <div class="col-6">
                    <InputSwitch
                      v-model="options.displayWeekNumbers"
                      class="w-4rem lck-checked"
                    />
                  </div>
                </div>
                <div class="flex col-12 p-0">
                  <Toolbar
                    class="border-none surface-0 outline-none pb-0 w-full"
                  >
                    <template #start>
                      <Button
                        @click="reFilterEmail"
                        class="p-button-outlined"
                        label="Xóa"
                      ></Button>
                    </template>
                    <template #end>
                      <Button @click="filterFileds" label="Cấu hình"></Button>
                    </template>
                  </Toolbar>
                </div>
              </div>
            </OverlayPanel>
          </span>
          <Button
            type="button"
            label="Ca làm việc"
            icon="pi pi-briefcase"
            :badge="options.declareShiftCount.toString()"
            badgeClass="p-badge-danger"
            class="p-button-outlined p-button-secondary mx-2"
            aria:haspopup="true"
            aria-controls="overlay_panel1"
            @click="toggle1"
          />
          <Button
            type="button"
            label="Ngày nghỉ lễ"
            icon="pi pi-sort-alt-slash"
            :badge="options.holidayDatesCount.toString()"
            badgeClass="p-badge-danger"
            class="p-button-outlined p-button-secondary mx-2"
            aria:haspopup="true"
            aria-controls="overlay_panel2"
            @click="toggle2"
          />
          <OverlayPanel
            ref="op1"
            appendTo="body"
            class="p-0 m-0"
            :showCloseIcon="false"
            id="overlay_panel1"
            style="width: 350px"
          >
            <div class="grid formgrid m-0">
              <div class="col-12 p-0">
                <Listbox
                  v-model="options.filterShift"
                  :options="listDeclareShift"
                  optionLabel="name"
                  optionValue="code"
                  class="w-full"
                  :multiple="true"
                  :filter="true"
                  listStyle="max-height:300px"
                >
                  <template #option="slotProps">
                    <div class="flex align-items-center">
                      <Chip   class="w-full format-center"
                        :label="slotProps.option.name"
                        :style="{
                          backgroundColor: slotProps.option.background_color,
                          color: slotProps.option.text_color,
                        }"
                      />
                    </div>
                  </template>
                </Listbox>
              </div>
              <Toolbar class="border-none surface-0 outline-none pb-0 w-full">
                <template #start>
                  <Button
                    @click="reFilterEmail1"
                    class="p-button-outlined"
                    label="Xóa"
                  ></Button>
                </template>
                <template #end>
                  <Button @click="filterFileds1" label="Lọc"></Button>
                </template>
              </Toolbar>
            </div>
          </OverlayPanel>
          <OverlayPanel
            ref="op2"
            appendTo="body"
            class="p-0 m-0"
            :showCloseIcon="false"
            id="overlay_panel2"
            style="width: 350px"
          >
            <div class="grid formgrid m-0">
              <div class="col-12 p-0">
                <Listbox
                  v-model="options.filterHolidayType"
                  :options="listHolidayType"
                  optionLabel="name"
                  optionValue="code"
                  class="w-full"
                  :multiple="true"
                  :filter="true"
                  listStyle="max-height:300px"
                >
                  <template #option="slotProps">
                    <div class="flex align-items-center">
                      <Chip
                      class="w-full format-center"
                        :label="slotProps.option.name"
                        :style="{
                          backgroundColor: slotProps.option.background_color,
                          color: slotProps.option.text_color,
                        }"
                      />
                    </div>
                  </template>
                </Listbox>
              </div>
              <div class="  col-12 p-0">
                <Toolbar class="border-none surface-0 outline-none pb-0 w-full">
                  <template #start>
                    <Button
                      @click="reFilterEmail2"
                      class="p-button-outlined"
                      label="Xóa"
                    ></Button>
                  </template>
                  <template #end>
                    <Button @click="filterFileds2" label="Lọc"></Button>
                  </template>
                </Toolbar>
              </div>
            </div>
          </OverlayPanel>
        </template>

        <template #end>
          <SelectButton
            v-model="state.displayPeriodUom"
            :options="listOpCalendar"
            optionValue="value"
            optionLabel="label"
            dataKey="value"
            class="mx-2"
            aria-labelledby="basic"
            @change="onChangeOpCal"
          >
          </SelectButton>

          <Button
            @click="refreshStamp"
            class="mr-2 p-button-outlined p-button-secondary"
            icon="pi pi-refresh"
            v-tooltip="'Tải lại'"
          />
        </template>
      </Toolbar>
    </div>
    <div class="px-3" style="height: 90%">
      <calendar-view
        :items="state.items"
        :show-date="state.showDate"
        :time-format-options="{ hour: 'numeric', minute: '2-digit' }"
        :enable-drag-drop="true"
        :disable-past="state.disablePast"
        :disable-future="state.disableFuture"
        :show-times="state.showTimes"
        :display-period-uom="state.displayPeriodUom"
        :display-period-count="state.displayPeriodCount"
        :current-period-label="'Hiện tại'"
        :displayWeekNumbers="state.displayWeekNumbers"
        :enable-date-selection="true"
        :selection-start="state.selectionStart"
        :selection-end="state.selectionEnd"
        :starting-day-of-week="state.startingDayOfWeek"
        :weekday-name-format="'long'"
        :locale="'vi-VN'"
        @click-item="onClickItem"
        class="theme-default"
      >
        <template #header="{ headerProps }">
          <calendar-view-header
            :header-props="headerProps"
            @input="setShowDate"
          />
        </template>
        <template #dayContent="{ day }">
          <div
            v-if="state.displayPeriodUom == 'year'"
            style="font-weight: bold !important; font-size: 1.2rem !important"
            class="p-1"
          >
            {{ moment(day).format("DD/MM") }}
          </div>
          <div
            v-if="
              state.displayPeriodUom == 'month' ||
              state.displayPeriodUom == 'week'
            "
            style="font-weight: bold !important; font-size: 1.2rem !important"
            class="p-1"
          >
            {{ moment(day).format("DD") }}
          </div>
        </template>
      </calendar-view>
    </div>
  </div>
  <Dialog
    header="Chi tiết ca làm việc"
    v-model:visible="displayBasicCal"
    :style="{ width: '35vw' }"
    :closable="true"
    :modal="true"
  >
    <form>
      <div class="grid formgrid px-2">
        <div class="field col-12 flex align-items-center md:col-12">
          <label class="col-3 text-left p-0">Ca làm việc </label>
          <InputText
            v-model="work_schedule.declare_shift_name"
            class="w-full"
            :style="{
              color: work_schedule.text_color,
              backgroundColor: work_schedule.background_color,
            }"
            disabled
            style="opacity: 1 !important"
          />
        </div>
        <div class="field flex align-items-center col-12 md:col-12">
          <label class="col-3 text-left p-0">Tên địa điểm</label>

          <InputText
            v-model="work_schedule.config_work_location_name"
            class="w-full"
            disabled
            style="opacity: 1 !important"
          />
        </div>
        <div class="field flex align-items-center col-12 md:col-12">
          <label class="col-3 text-left p-0">Địa điểm làm việc </label>
          <Textarea
            :autoResize="true"
            rows="1"
            cols="30"
            v-model="work_schedule.config_work_location_address"
            class="w-full"
            disabled
            style="opacity: 1 !important; background-color: #eee"
          />
        </div>
        <div class="field flex align-items-center col-12 md:col-12">
          <label class="col-3 text-left p-0">Thời gian làm việc </label>
          <div class="col-4 text-left p-0">
            <Calendar
              inputId="time12"
              hourFormat="24"
              class="w-full d-design-calendar-1"
              autocomplete="on"
              icon="pi pi-clock"
              :showIcon="true"
              :timeOnly="true"
              v-model="work_schedule.start_time"
              disabled
            />
          </div>
          <div class="col-1 text-left p-0"></div>
          <div class="col-4 text-left p-0">
            <Calendar
              inputId="time12"
              hourFormat="24"
              class="w-full d-design-calendar-1"
              autocomplete="on"
              icon="pi pi-clock"
              :showIcon="true"
              :timeOnly="true"
              v-model="work_schedule.end_time"
              disabled
            />
          </div>
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="closeDialogCal"
        class="p-button-outlined"
      />
    </template>
  </Dialog>
  <Dialog
    header="Chi tiết ngày nghỉ lễ"
    v-model:visible="displayBasicHol"
    :style="{ width: '35vw' }"
    :closable="true"
    :modal="true"
  >
    <form>
      <div class="grid formgrid px-2">
        <div class="field col-12 flex align-items-center md:col-12">
          <label class="col-3 text-left p-0">Loại nghỉ lễ</label>
          <InputText
            v-model="holiday_dates.holiday_type_name"
            class="w-full"
            :style="{
              color: holiday_dates.text_color,
              backgroundColor: holiday_dates.background_color,
            }"
            disabled
            style="opacity: 1 !important"
          />
        </div>
        <div class="field flex align-items-center col-12 md:col-12">
          <label class="col-3 text-left p-0">Lý do</label>

          <Textarea
            :autoResize="true"
            rows="1"
            cols="30"
            v-model="holiday_dates.reason"
            class="w-full"
            disabled
            style="opacity: 1 !important; background-color: #eee"
          />
        </div>

        <div class="field flex align-items-center col-12 md:col-12">
          <label class="col-3 text-left p-0">Thời gian </label>
          <div class="col-4 text-left p-0">
            <Calendar
              class="w-full d-design-calendar-1"
              autocomplete="on"
              icon="pi pi-clock"
              :showIcon="true"
              v-model="holiday_dates.start_date"
              disabled
            />
          </div>
          <div class="col-1 text-left p-0"></div>
          <div class="col-4 text-left p-0">
            <Calendar
              class="w-full d-design-calendar-1"
              autocomplete="on"
              icon="pi pi-clock"
              :showIcon="true"
              v-model="holiday_dates.end_date"
              disabled
            />
          </div>
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="closeDialogHol"
        class="p-button-outlined"
      />
    </template>
  </Dialog>
</template>
    
    <style scoped>
.inputanh {
  border: 1px solid #ccc;
  width: 8rem;
  height: 8rem;
  cursor: pointer;
  padding: 0.063rem;
  display: block;
  margin-left: auto;
  margin-right: auto;
}
.ipnone {
  display: none;
}
.inputanh img {
  object-fit: cover;
  width: 100%;
  height: 100%;
}
.d-lang-table {
  max-height: calc(100vh - 135px);
}
.calendar-parent {
  display: flex;
  flex-direction: column;
  flex-grow: 1;
  overflow-x: hidden;
  overflow-y: hidden;
  max-height: 95%;
}
.calendar-parent .cv-wrapper {
  min-height: unset !important;
}
</style>
    