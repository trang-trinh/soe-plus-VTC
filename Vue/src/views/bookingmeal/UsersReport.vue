<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import ClassicEditor from "@ckeditor/ckeditor5-build-classic";
import moment from "moment";
import { encr } from "../../util/function.js";
const cryoptojs = inject("cryptojs");
//Khai báo
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const basedomainURL = baseURL;
const weekPickerFilter = ref();
const datalists = ref([]);
const listUserSelected = ref([]);
const listDropdownUser = ref([]);
var price = 1;
var working_days = [];
const total_Ration = ref(0);
const checkFilterDate = ref(false);
const total_Price = ref();
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const toast = useToast();
const options = ref({
  Start_Date: null,
  End_Date: null,
  loading: true,
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
//Lọc
const dataTime = ref([]);
//loc date
const filterMonth = ref();
const taskDateFilter = ref();
const todayClick = () => {
  // taskDateFilter.value = [];
  // taskDateFilter.value.push(new Date());
  options.value.Start_Date = new Date();
  options.value.End_Date = null;
  loadData();
};
const toggleFilterMonth = (event) => {
  filterMonth.value.toggle(event);
};
const onFilterMonth = (check) => {
  checkFilterDate.value = true;
  taskDateFilter.value = [];
  if (check) {
    if (weekPickerFilter.value) {
      options.value.Start_Date = weekPickerFilter.value[0];
      options.value.End_Date = weekPickerFilter.value[1];
    }
  } else {
    if (monthPickerFilter.value) {
      weekPickerFilter.value = [];
      let day = new Date(
        monthPickerFilter.value.year,
        monthPickerFilter.value.month + 1,
        0
      ).getDate();
      options.value.Start_Date = new Date(
        monthPickerFilter.value.month +
          1 +
          "/01" +
          "/" +
          monthPickerFilter.value.year
      );
      options.value.End_Date = new Date(
        monthPickerFilter.value.month +
          1 +
          "/" +
          day +
          "/" +
          monthPickerFilter.value.year
      );
    }
  }
  loadData();
  filterMonth.value.hide();
};
const monthPickerFilter = ref();

const delDayClick = () => {
  taskDateFilter.value = [];
  options.value.Start_Date = null;
  options.value.End_Date = null;
  loadData(true);
};
const onDayClick = () => {
  debugger;
  if (monthPickerFilter.value) monthPickerFilter.value = null;
  if (weekPickerFilter.value) weekPickerFilter.value = null;
  if (taskDateFilter.value == null) taskDateFilter.value = [];
  else {
    options.value.Start_Date = taskDateFilter.value[0];
    options.value.End_Date = taskDateFilter.value[1];
    loadData();
  }
};
const onCleanFilterMonth = () => {
  if (weekPickerFilter.value) weekPickerFilter.value = null;
  if (monthPickerFilter.value) monthPickerFilter.value = null;
    checkFilterDate.value = false;
  loadData(true);
};
const loadData = (f) => {
  total_Ration.value = 0;
  var day_off = [];
  if (f) {
    const today = new Date();
    const firstDay = getFirstDayOfWeek(today);
    const lastDay = new Date(firstDay);
    lastDay.setDate(lastDay.getDate() + 6);
    options.value.Start_Date = firstDay;
    options.value.End_Date = lastDay;
  }
  working_days.forEach((item) => {
    let dt = new Date(item);
    if (dt >= options.value.Start_Date && dt <= options.value.End_Date) {
      let dateString = moment(dt).format("MM/DD/yyyy");
      day_off.push(dateString);
    }
  });
  axios
    .post(
        baseURL + "/api/BookingMeal/GetDataProc",
        {
          str: encr(JSON.stringify({
            proc: "booking_report_week",
        par: [
          { par: "user_id", va: options.value.Start_Date },
          { par: "search", va: options.value.End_Date },
          {
            par: "list_user",
            va:
              listUserSelected.value.length > 0
                ? listUserSelected.value.toString()
                : null,
          },
          { par: "user_id", va: store.getters.user.user_id },
        ],
          }), SecretKey, cryoptojs
          ).toString()
        },
        config
      )
    .then((response) => {
      debugger;
      let data = JSON.parse(response.data.data)[0];
      let data1 = JSON.parse(response.data.data)[1];
      if (data1.length > 0) {
        dataTime.value = data1[0].days.split(",");
      }
      if (data.length > 0) {
        data.forEach((item, index) => {
          if (item.listdate != null) item.listdate = item.listdate.split(",");
          else item.listdate = [];
          item.is_order = index;
          item.totalRation = item.listdate.length;
        });
        datalists.value = [...data];
        var totalR = 0;
        var count = 0;
        data.forEach((item, index) => {
          totalR += item.totalRation;
          if (
            index == data.length - 1 ||
            item.department_id !== data[index + 1].department_id
          ) {
            let obj = {
              isTotal: true,
              full_name: "Tổng " + (item.department_name || ""),
              totalRation: totalR,
              role_name: "",
              department_name: "",
              listdate: [],
            };
            total_Ration.value += obj.totalRation;
            datalists.value.splice(index + count + 1, 0, obj);
            datalists.value.join();
            count++;
            totalR = 0;
          }
        });
      } else {
        datalists.value = [];
      }
      options.value.loading = false;
    })
    .catch((error) => {
      console.log(error);
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
const initConfig = () => {
  axios
    .get(baseURL + "/api/BookingMeal/GetConfig", {
      headers: { Authorization: `Bearer ${store.getters.token}` },
    })
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        if (response.data.data) {
          price = response.data.data.price;
          working_days = response.data.data.working_days;
          loadData(true);
        }
      } else {
        loadData(true);
        swal.fire({
          title: "Error!",
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
      }
    });
};
const loadFilterUsers = () => {
  loadData();
};
const loadUser = () => {
  axios
    .post(
        baseURL + "/api/BookingMeal/GetDataProc",
        {
          str: encr(JSON.stringify({
            proc: "booking_meal_user_list",
        par: [{ par: "user_id", va: store.getters.user.user_id }],
          }), SecretKey, cryoptojs
          ).toString()
        },
        config
      )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        listDropdownUser.value = data[0];
      }
    })
    .catch((error) => {
      if (error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
      }
    });
};
//refresh
const onRefresh = () => {
  checkFilterDate.value = false;
  if (monthPickerFilter.value) monthPickerFilter.value = null;
  if (weekPickerFilter.value) weekPickerFilter.value = null;
  taskDateFilter.value = [];
  listUserSelected.value = [];
  loadData(true);
};
const exportExcel = () => {
  debugger;
  let text_string = '';
  if(options.value.End_Date){
    text_string = 'TỪ NGÀY '+ moment(new Date(options.value.Start_Date)).format("DD/MM/YYYY").toString()+' - '+
           moment(new Date(options.value.End_Date)).format("DD/MM/YYYY").toString() 
  }
  else{
        text_string = ' NGÀY '+ moment(new Date(options.value.Start_Date)).format("DD/MM/YYYY").toString()
  }
  let name = "BC_suat_an_";
  let id = "tablequizz";
  var htmltable1 = "";
  // htmltable1 = renderExcel_Ketqua();
  var tab_text = '<html xmlns:x="urn:schemas-microsoft-com:office:excel">';
  tab_text =
    tab_text +
    "<head><xml><x:ExcelWorkbook><x:ExcelWorksheets><x:ExcelWorksheet>";
  tab_text = tab_text + "<x:Name>Test Sheet</x:Name>";
  tab_text =
    tab_text +
    "<x:WorksheetOptions><x:Panes></x:Panes></x:WorksheetOptions></x:ExcelWorksheet>";
  tab_text =
    tab_text + "</x:ExcelWorksheets></x:ExcelWorkbook></xml></head><body>";
  tab_text =
    tab_text +
    "<style>.item-date{min-width:100px !important}.bc-content th,td,table,tr{padding:5px;font-size:13pt}table{margin:20px auto;border-collapse: collapse;}</style>";
  tab_text =
    tab_text +
    '<style>.cstd{font-family: Times New Roman;border:none!important; font-size: 17px; font-weight: 700; text-align: center; vertical-align: center;color:#1769aa}</style><table><td colspan="' +
    (dataTime.value.length+6) +'" class="cstd" > BÁO CÁO CẮT CƠM '+text_string+'</td > ';
  tab_text = tab_text + "</table>";

  //var exportTable = $('#' + id).clone();
  //exportTable.find('input').each(function (index, elem) { $(elem).remove(); });\
tab_text = tab_text + "<style>th,table,tr{font-family: Times New Roman; font-size: 12px; vertical-align: middle; text-align: center;}</style><table border='1'>";
  var exportTable = document
    .getElementById("table-bc")
    .cloneNode(true).innerHTML;
  tab_text = tab_text + exportTable;
  tab_text = tab_text + htmltable1;
   tab_text = tab_text + '</table>';
  tab_text = tab_text + '<meta charset="utf-8"/></ta></body></html>';
  var data_type =
    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
  var ua = window.navigator.userAgent;
  var msie = ua.indexOf("MSIE ");

  var fileName = name + "_" + parseInt(Math.random() * 1000) + ".xls";
  if (msie > 0 || !!navigator.userAgent.match(/Trident.*rv\:11\./)) {
    if (window.navigator.msSaveBlob) {
      var blob = new Blob([tab_text], {
        type: data_type, //"application/csv;charset=utf-8;"
      });
      navigator.msSaveBlob(blob, fileName);
    }
  } else {
    var blob2 = new Blob([tab_text], {
      type: data_type, //"application/csv;charset=utf-8;"
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
// def
function getFirstDayOfWeek(d) {
  const date = new Date(d);
  const day = date.getDay();

  const diff = date.getDate() - day + (day === 0 ? -6 : 1);

  return new Date(date.setDate(diff));
}
//format number
function formatNumber(a, b, c, d) {
  var e = isNaN((b = Math.abs(b))) ? 2 : b;
  b = void 0 == c ? "," : c;
  d = void 0 == d ? "," : d;
  c = 0 > a ? "-" : "";
  var g = parseInt((a = Math.abs(+a || 0).toFixed(e))) + "",
    n = 3 < (n = g.length) ? n % 3 : 0;
  return (
    c +
    (n ? g.substr(0, n) + d : "") +
    g.substr(n).replace(/(\d{3})(?=\d)/g, "$1" + d) +
    (e
      ? b +
        Math.abs(a - g)
          .toFixed(e)
          .slice(2)
      : "")
  );
}
onMounted(() => {
  initConfig();
  loadUser();
  return {};
});
</script>

<template>
  <div class="relative overflow-hidden" style="overflow-y: hidden !important">
    <div class="w-full format-center">
      <div>
        <h2 class="pt-0 mt-3 text-blue-700" v-if="options.End_Date">
          Báo cáo cắt cơm từ ngày
          {{ moment(new Date(options.Start_Date)).format("DD/MM/YYYY") }} -
          {{ moment(new Date(options.End_Date)).format("DD/MM/YYYY") }}
        </h2>
        <h2 class="pt-0 mt-3 text-blue-700" v-else>
          Báo cáo suất ăn ngày
          {{ moment(new Date(options.Start_Date)).format("DD/MM/YYYY") }}
        </h2>
        <div class="text-md">
          {{ store.getters.user.full_name }} -
          {{ moment(new Date()).format("HH:mm DD/MM/YYYY") }}
          <span class="font-italic">(<b>X</b> : ngày cắt cơm)</span>
        </div>
      </div>
    </div>
    <div>
      <Toolbar class="custoolbar mx-2">
        <template #start>
          <div class="p-inputgroup w-20rem">
            <span class="p-inputgroup-addon">
              <i class="pi pi-user"></i>
            </span>
            <span class="p-float-label">
              <MultiSelect
                id="multiselect"
                :filter="true"
                v-model="listUserSelected"
                :options="listDropdownUser"
                optionLabel="full_name"
                optionValue="user_id"
                class="col-4 ip36 p-0"
                @change="loadFilterUsers()"
              >
                <template #option="slotProps">
                  <div class="country-item flex">
                    <Avatar
                      v-bind:label="
                        slotProps.option.avatar
                          ? ''
                          : slotProps.option.full_name
                              .split(' ')
                              .at(-1)
                              .substring(0, 1)
                      "
                      :image="basedomainURL + slotProps.option.avatar"
                      style="color: #ffffff"
                      :style="{
                        background:
                          bgColor[slotProps.option.full_name.length % 7],
                      }"
                      class="mr-2 w-2rem h-2rem"
                      size="large"
                      shape="circle"
                    />
                    <div class="pt-1">{{ slotProps.option.full_name }}</div>
                  </div>
                </template>
              </MultiSelect>

              <label for="multiselect">Danh sách nhân viên</label>
            </span>
          </div>
        </template>
        <template #end>
          <Button
            class="mr-2 p-button-sm p-button-outlined p-button-secondary"
            @click="onRefresh()"
            icon="pi pi-refresh"
          />
          <Calendar
            placeholder="Lọc theo ngày"
            id="range"
            v-model="taskDateFilter"
            :showIcon="true"
            selectionMode="range"
            :manualInput="false"
          >
            <template #footer>
              <div class="w-full flex">
                <div class="w-4 format-center">
                  <span @click="todayClick" class="cursor-pointer text-primary"
                    >Hôm nay</span
                  >
                </div>
                <div class="w-4 format-center">
                  <Button @click="onDayClick" label="Thực hiện"></Button>
                </div>
                <div class="w-4 format-center">
                  <span @click="delDayClick" class="cursor-pointer text-primary"
                    >Xóa</span
                  >
                </div>
              </div>
            </template>
          </Calendar>

          <Button
            type="button"
            :class="
              checkFilterDate
                ? 'mx-2'
                : 'mx-2 p-button-secondary p-button-outlined'
            "
            icon="pi pi-filter"
            @click="toggleFilterMonth($event)"
            aria-haspopup="true"
            aria-controls="overlay_panelMonth"
          />
                    <Button
            label="Xuất báo cáo"
            icon="pi pi-file-excel"
            class="mr-2 p-button-outlined p-button-secondary"
            @click="exportExcel"
          />
          <!-- <Datepicker
            @closed="onFilterMonth()"
            selectText="Thực hiện"
            cancelText="Hủy"
            class="w-full"
            locale="vi"
            placeholder=" Lọc theo tuần"
            v-model="weekPickerFilter"
            weekPicker
          >
            <template #clear-icon>
              <Button
                @click="onCleanFilterMonth"
                icon="pi pi-times"
                class="p-button-rounded p-button-text"
              />
            </template>
            <template #input-icon>
              <Button class="mr-2 p-button-text" icon="pi pi-calendar" />
            </template>
          </Datepicker> -->
        </template>
      </Toolbar>
    </div>
    <div
      style="width: 100%; height: calc(100vh - 160px)"
      class="overflow-scroll relative api-test"
    >
      <table class="w-full mt-2" style="overflow-y: scroll" id="table-bc">
        <thead>
          <tr
            style="background-color: #f8f9fa; z-index: 10 !important"
            class="top-0 sticky"
          >
            <th
              rowspan="2"
              style="
                padding: 0.5rem;
                height: 50px;
                background-color: #f8f9fa;
                min-width: 50px;
                max-width: 50px;
              "
              class="m-checkbox-table top-0 text-lg sticky left-0"
            >
              STT
            </th>
            <th
              rowspan="2"
              style="
                padding: 0.5rem;
                height: 50px;
                background-color: #f8f9fa;
                min-width: 150px;
                max-width: 200px;
              "
              class="m-checkbox-table top-0 text-lg sticky left-50"
            >
              Họ tên nhân sự
            </th>
            <th
              rowspan="2"
              style="
                padding: 0.5rem;
                height: 50px;
                background-color: #f8f9fa;
                min-width: 120px;
                max-width: 120px;
              "
              class="m-checkbox-table top-0 text-lg sticky left-200"
            >
              Chức vụ
            </th>
            <th
              rowspan="2"
              style="
                padding: 0.5rem;
                height: 50px;
                background-color: #f8f9fa;
                min-width: 200px;
                max-width: 250px;
              "
              class="m-checkbox-table top-0 text-lg sticky left-320"
            >
              Phòng ban
            </th>
            <th
              rowspan="2"
              style="
                padding: 0.5rem;
                height: 50px;
                background-color: #f8f9fa;
                min-width: 80px;
                max-width: 80px;
              "
              class="m-checkbox-table top-0 text-lg sticky left-520"
            >
              Tổng suất
            </th>
            <th :colspan="dataTime.length" class="text-center text-lg py-2">
              <span v-if="options.End_Date">
                Từ
                {{ moment(new Date(options.Start_Date)).format("DD/MM/YYYY") }}
                -
                {{ moment(new Date(options.End_Date)).format("DD/MM/YYYY") }}
              </span>
              <span v-else>
                Ngày
                {{ moment(new Date(options.Start_Date)).format("DD/MM/YYYY") }}
              </span>
            </th>
          </tr>
          <tr class="sticky" style="z-index:1 !important; top: 33px !important;background-color: #f8f9fa;">
            <th
              class="text-lg item-date"
              style="padding: 0.5rem"
              v-for="(item, index) in dataTime"
              :key="index"
            >
              <div class="">
                {{ moment(new Date(item)).format("DD/MM/YYYY") }}
              </div>
            </th>
          </tr>
        </thead>
        <tbody>
          <tr
            style="vertical-align: top"
            v-for="(item, index) in datalists"
            :key="index"
          >
            <td
              v-if="!item.isTotal || item.is_order"
              class="sticky left-0 p-2 text-lg align-content-center text-center"
              style="background-color: #f8f9fa"
            >
              {{ item.is_order + 1 }}
            </td>
            <td
              v-if="!item.isTotal"
              class="sticky p-2 text-lg left-50"
              style="background-color: #f8f9fa"
            >
              {{ item.full_name }}
            </td>
            <td
              v-if="!item.isTotal"
              class="sticky p-2 text-lg left-200"
              style="background-color: #f8f9fa"
            >
              {{ item.role_name }}
            </td>
            <td
              v-if="!item.isTotal"
              class="sticky left-320 p-2 text-lg"
              style="background-color: #f8f9fa"
            >
              {{ item.department_name }}
            </td>
            <td
              class="sticky left-0 p-2 text-lg"
              style="background-color: #dbeaf7"
              v-if="item.isTotal"
            ></td>
            <td
              class="text-lg font-medium sticky left-50"
              style="
                background-color: #dbeaf7;
                text-align: center;
                vertical-align: middle;
              "
              v-if="item.isTotal"
              colspan="3"
            >
              {{ item.full_name }}
            </td>
            <td
              class="sticky left-520 p-2 text-lg text-center"
              :style="
                item.isTotal
                  ? 'background-color:#dbeaf7'
                  : 'background-color: #f8f9fa'
              "
            >
              {{ item.totalRation }}
            </td>
            <td
              v-for="(date, index1) in dataTime"
              :key="index1"
              class="item-date"
              :style="item.isTotal ? 'background-color:#dbeaf7' : ''"
            >
              <div
                v-if="item.listdate.includes(date)"
                style="text-align: center"
              >
                <span>X</span>
              </div>
            </td>
          </tr>
          <tr>
            <td class="sticky left-0 p-2 text-lg text-center"></td>
            <td
              class="sticky left-50 p-2 text-lg text-center font-bold"
              colspan="3"
            >
              Tổng cộng
            </td>
            <td class="sticky left-520 p-2 text-lg text-center font-bold">
              {{ total_Ration }}
            </td>
            <td class="item-date" :colspan="dataTime.length"></td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
  <OverlayPanel
    ref="filterMonth"
    appendTo="body"
    class="p-0 m-0"
    :showCloseIcon="false"
    id="overlay_panelMonth"
    style="width: 300px"
  >
    <div class="grid formgrid m-0">
      <div class="flex field col-12 p-0">
        <Datepicker
          @closed="onFilterMonth(true)"
          selectText="Thực hiện"
          cancelText="Hủy"
          class="w-full"
          locale="vi"
          placeholder=" Lọc theo tuần"
          v-model="weekPickerFilter"
          weekPicker
        >
          <template #clear-icon>
            <Button
              @click="onCleanFilterMonth"
              icon="pi pi-times"
              class="p-button-rounded p-button-text"
            />
          </template>
          <template #input-icon>
            <Button class="mr-2 p-button-text" icon="pi pi-calendar" />
          </template>
        </Datepicker>
      </div>

      <div class="flex field col-12 p-0">
        <Datepicker
          @closed="onFilterMonth(false)"
          class="w-full"
          locale="vi"
          selectText="Thực hiện"
          cancelText="Hủy"
          placeholder=" Lọc theo tháng"
          v-model="monthPickerFilter"
          monthPicker
          ><template #clear-icon>
            <Button
              @click="onCleanFilterMonth"
              icon="pi pi-times"
              class="p-button-rounded p-button-text"
            />
          </template>
          <template #input-icon>
            <Button icon="pi pi-calendar" class="p-button-text" />
          </template>
        </Datepicker>
      </div>
    </div>
  </OverlayPanel>
</template>
  <style scoped>
.left-50 {
  left: 50px !important;
}
.left-200 {
  left: 200px !important;
}
.left-320 {
  left: 320px !important;
}
.left-520 {
  left: 520px !important;
}
.left-600 {
  left: 600px !important;
}
.item-date {
  vertical-align: middle;
}
.color-red {
  color: red;
}
.bg-color-red {
  background-color: red;
}
table,
th,
td {
  border: 1px solid #e9ecef;
  border-collapse: separate;
  border-spacing: 0;
}

.m-checkbox-table {
  background-color: #fff;
  position: sticky;
  left: 0;
  top: 0;
  padding: 0px;
  width: 200px;
  text-align: center;
  border-right: none !important;
}
.api-test::-webkit-scrollbar {
  height: 25px !important;
}
</style>
    <style lang="scss" scoped>
// ::v-deep(.p-sidebar) {
//   .p-sidebar-content {
//     padding: 0px !important;
//     overflow-y: hidden;
//   }
// }
</style>