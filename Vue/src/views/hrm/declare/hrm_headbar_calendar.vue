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
    if (isDynamicSQL.value) {
      loadDataSQL();
      return false;
    }
    if (rf) {
      if (options.value.PageNo == 0) {
        loadCount();
      }
    }
    state.value.items = [];
    axios
      .post(
        baseURL + "/api/hrm_ca_SQL/getData",
        {
          str: encr(
            JSON.stringify({
              proc: "hrm_holiday_dates_list",
              par: [
                { par: "pageno", va: options.value.PageNo },
                { par: "pagesize", va: options.value.PageSize },
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
              id: element.holiday_dates_id,
              startDate: new Date(element.start_date),
              endDate: element.end_date
                ? new Date(element.end_date)
                : new Date(element.start_date),
              title:
                "<span>" +
                element.reason +
                "</span> <br/><span class='text-sm'>" +
                element.holiday_type_name +
                "</span>",
              tooltip: element.reason,
              style: element.text_color + element.background_color,
            });
          }
        });
        datalists.value = data;

        options.value.loading = false;
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
        if (isFirst.value) isFirst.value = false;

 
        data.forEach((element, i) => {
          if (element.work_schedule_days) {
            element.work_schedule_days.split(",").forEach((item,idx) => {
              if (element.text_color)
                element.text_color = "color:" + element.text_color;
              else element.text_color = "";
              if (element.background_color)
                element.background_color =
                  "; background-color:" + element.background_color;
              else element.background_color = "";
              state.value.items.push({
                id: element.profile_id+ element.work_schedule_id,
                startDate: new Date(item.work_schedule_days),
                endDate:new Date(item.work_schedule_days),
                title:
                  "<span>" +
                  element.declare_shift_name +
                  "</span>" ,
                tooltip: element.declare_shift_name,
                style: element.text_color + element.background_color,
              });
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
            text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
            confirmButtonText: "OK",
          });
          store.commit("gologout");
        }
      });
  }
};
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
});

//Hiển thị dialog
const headerDialog = ref();
const displayBasic = ref(false);

//Thêm bản ghi

const sttStamp = ref(1);
const saveData = () => {
  submitted.value = true;
  if (
    holiday_dates.value.start_date == null ||
    holiday_dates.value.holiday_type_id == null
  ) {
    return;
  }
  let formData = new FormData();

  formData.append("hrm_holiday_dates", JSON.stringify(holiday_dates.value));
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  if (!isSaveTem.value) {
    axios
      .post(
        baseURL + "/api/hrm_holiday_dates/add_hrm_holiday_dates",
        formData,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Thêm ngày nghỉ lễ thành công!");
          loadData(true);
        } else {
          swal.fire({
            title: "Error!",
            text: response.data.ms,
            icon: "error",
            confirmButtonText: "OK",
          });
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
  } else {
    axios
      .put(
        baseURL + "/api/hrm_holiday_dates/update_hrm_holiday_dates",
        formData,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa ngày nghỉ lễ thành công!");
        } else {
          swal.fire({
            title: "Error!",
            text: response.data.ms,
            icon: "error",
            confirmButtonText: "OK",
          });
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
  }
};
const checkIsmain = ref(true);
//Sửa bản ghi
const editTem = (dataTem) => {
  submitted.value = false;
  holiday_dates.value = dataTem;
  if (holiday_dates.value.start_date)
    holiday_dates.value.start_date = new Date(holiday_dates.value.start_date);
  if (holiday_dates.value.end_date)
    holiday_dates.value.end_date = new Date(holiday_dates.value.end_date);
  headerDialog.value = "Sửa ngày nghỉ lễ";
  isSaveTem.value = true;
  displayBasic.value = true;
};
//Xóa bản ghi
const delTem = (Tem) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá bản ghi này không!",
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

        axios
          .delete(baseURL + "/api/hrm_holiday_dates/delete_hrm_holiday_dates", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: Tem != null ? [Tem.holiday_dates_id] : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá ngày nghỉ lễ thành công!");
              loadData(true);
            } else {
              swal.fire({
                title: "Error!",
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
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
};
//Xuất excel

//Sort
const onSort = (event) => {
  options.value.PageNo = 0;

  if (event.sortField == null) {
    isDynamicSQL.value = false;
    loadData(true);
  } else {
    options.value.sort =
      event.sortField + (event.sortOrder == 1 ? " ASC" : " DESC");
    if (event.sortField == "STT") {
      options.value.sort =
        "is_order" + (event.sortOrder == 1 ? " ASC" : " DESC");
    }
    isDynamicSQL.value = true;
    loadDataSQL();
  }
};
const optionsSelB = ref([
  { icon: "pi pi-bars", value: 1 },
  { icon: "pi pi-table", value: 2 },
]);
const listOpCalendar = ref([
  { label: "Tuần", value: "week" },
  { label: "Tháng", value: "month" },
  { label: "Năm", value: "year" },
]);

const optionsHoliday = ref(2);

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
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
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
      isDynamicSQL.value = false;
      options.value.loading = true;
      loadData(true);
    } else {
      isDynamicSQL.value = true;
      options.value.loading = true;
      loadData(true);
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
const trangThai = ref([
  { name: "Hiển thị", code: 1 },
  { name: "Không hiển thị", code: 0 },
]);

const filterTrangthai = ref();

const reFilterEmail = () => {
  filterTrangthai.value = null;
  isDynamicSQL.value = false;
  checkFilter.value = false;
  filterSQL.value = [];
  options.value.SearchText = null;
  op.value.hide();
  loadData(true);
};
const filterFileds = () => {
  filterSQL.value = [];
  checkFilter.value = true;
  let filterS = {
    filterconstraints: [{ value: filterTrangthai.value, matchMode: "equals" }],
    filteroperator: "and",
    key: "status",
  };
  filterSQL.value.push(filterS);
  loadDataSQL();
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
const listHolidayType = ref([]);
const initTudien = () => {
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
  useTodayIcons: false,
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

const onClickDay = (d) => {
  submitted.value = false;
  holiday_dates.value = {
    status: true,
    is_order: sttStamp.value,
    organization_id: store.getters.user.organization_id,
    is_system: store.getters.user.is_super ? true : false,
    start_date: d,
    end_date: d,
  };
  checkIsmain.value = false;
  isSaveTem.value = false;
  headerDialog.value = "Thêm ngày nghỉ lễ";
  displayBasic.value = true;
};

const onClickItem = (e) => {
  submitted.value = false;
  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_holiday_dates_get",
            par: [{ par: "holiday_dates_id", va: e.id }],
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
      headerDialog.value = "Sửa ngày nghỉ lễ";
      isSaveTem.value = true;
      displayBasic.value = true;
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;
    });
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

    saveData,
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
        <i class="pi pi-credit-card"></i> Danh sách ngày nghỉ lễ ({{
          options.totalRecords
        }})
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
              v-tooltip="'Bộ lọc'"
              :class="
                filterTrangthai != null && checkFilter
                  ? ''
                  : 'p-button-secondary p-button-outlined'
              "
            />
            <OverlayPanel
              ref="op"
              appendTo="body"
              class="p-0 m-0"
              :showCloseIcon="false"
              id="overlay_panel"
              style="width: 300px"
            >
              <div class="grid formgrid m-0">
                <div class="flex field col-12 p-0">
                  <div
                    class="col-4 text-left pt-2 p-0"
                    style="text-align: left"
                  >
                    Trạng thái
                  </div>
                  <div class="col-8">
                    <Dropdown
                      class="col-12 p-0 m-0"
                      v-model="filterTrangthai"
                      :options="trangThai"
                      optionLabel="name"
                      optionValue="code"
                      placeholder="Trạng thái"
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
                      <Button @click="filterFileds" label="Lọc"></Button>
                    </template>
                  </Toolbar>
                </div>
              </div>
            </OverlayPanel>
          </span>
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
          >
          </SelectButton>
          <SelectButton
            v-model="optionsHoliday"
            :options="optionsSelB"
            optionValue="value"
            dataKey="value"
            class="mx-2"
            aria-labelledby="basic"
          >
            <template #option="slotProps">
              <i :class="slotProps.option.icon"></i>
            </template>
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
    <div class="h-full px-3">
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
        :current-period-label="state.useTodayIcons ? 'icons' : ''"
        :displayWeekNumbers="state.displayWeekNumbers"
        :enable-date-selection="true"
        :selection-start="state.selectionStart"
        :selection-end="state.selectionEnd"
        :starting-day-of-week="state.startingDayOfWeek"
        :locale="'vi-VN'"
        class="theme-default"
      >
        <template #header="{ headerProps }">
          <calendar-view-header
            :header-props="headerProps"
            @input="setShowDate"
          />
        </template>
      </calendar-view>
    </div>
  </div>
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

  background-color: white;
}
.calendar-parent .cv-wrapper {
  min-height: unset !important;
}
</style>
    