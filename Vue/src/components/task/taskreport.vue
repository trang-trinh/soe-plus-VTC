<script setup>
//Khai báo InJect và Import (import)
import { ref, inject, onMounted, watch } from "vue";
import groupuser from "./groupuser.vue";
import { useToast } from "vue-toastification";
import moment from "moment";
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const emitter = inject("emitter");
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const typeChart = ref(1);
const userFilter = ref(store.getters.user.user_id);
const basedomainURL = baseURL;
//Nơi nhận dữ liệu
const props = defineProps({
  isShowDialog: Boolean,
  dataReport: Object,
  dateReport: Object,
  userReport: Object,
  listDropdownUser: Object,
});
const userReportLocal = ref();
const dateReportLocal = ref();
const listDropdownUser = ref([]);
const listUsers = ref();
//Nơi nhận EMIT từ component
emitter.on("emitData", (obj) => {
  switch (obj.type) {
    case "listDropdownUser":
      listDropdownUser.value = obj.data;
      break;
    case "listUsers":
      listUsers.value = obj.data;
      break;
    // case "listDropdownUserCheck":
    //   listDropdownUserCheck.value = obj.data;
    //   break;
  }
});
const taskDateFilter = ref();
const listUserSelected = ref([]);
const options = ref({
  IsNext: true,
  sort: "task_id",
  searchText: "",
  PageNo: 0,
  PageSize: 10,
  loading: false,
  totalRecords: null,
  finishedRecord: null,
  waitedRecord: null,
  tempClose: null,
  unFinishRecord: null,
  statusTask: null,
  outOfDate: null,
  SearchTextUser: "",
  Start_date: null,
  End_date: null,
  Date_Filter: 0,
  typeCheckList: 0,
});
const isShowDialog = ref(false);

const filterMonth = ref();
const onShowReport = () => {
  loadDataReport();
};
const toggleFilterMonth = (event) => {
  filterMonth.value.toggle(event);
};
const todayClick = () => {
  taskDateFilter.value = [];
  taskDateFilter.value.push(new Date());
};

const onCleanFilterMonth = () => {
  if (monthPickerFilter.value) monthPickerFilter.value = null;
  if (weekPickerFilter.value) weekPickerFilter.value = null;
  options.value.Start_Date = null;
  options.value.End_Date = null;
  loadDataReport();
};
const monthPickerFilter = ref();

const weekPickerFilter = ref();
const delDayClick = () => {
  taskDateFilter.value = [];
  options.value.Start_Date = null;
  options.value.End_Date = null;
  loadDataReport();
};
const onDayClick = () => {
  console.log("soa", taskDateFilter.value);
  if (monthPickerFilter.value) monthPickerFilter.value = null;
  if (weekPickerFilter.value) weekPickerFilter.value = null;
  if (taskDateFilter.value == null) taskDateFilter.value = [];
  else {
    options.value.Start_Date = taskDateFilter.value[0];
    options.value.End_Date = taskDateFilter.value[1];
    if (options.value.End_Date == null) {
      options.value.End_Date = options.value.Start_Date;
    }
    loadDataReport();
  }
};

var groupByDefault = function (xs, key) {
  if (xs != null)
    if (xs.length > 0)
      return xs.reduce(function (rv, x) {
        (rv[x[key]] = rv[x[key]] || []).push(x);
        return rv;
      }, {});
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
]);

const toggleExport = (event) => {
  menuButs.value.toggle(event);
};
const exportData = (method) => {
  let arrP = [];
  let arrMain = [];
  let topCell = [{ date: "Nhân viên" }];
  dataTime.value.forEach((el) => {
    topCell.push({ date: el });
  });
  userReportLocal.value.forEach((item) => {
    let set1 = { full_name: item.full_name };
    dataTime.value.forEach((date) => {
      let dataDate = "";

      if (datalists.value[date][item.full_name]) {
        datalists.value[date][item.full_name].forEach((data) => {
          dataDate += "\n-" + data.des;
        });
        if (!set1[date]) set1[date] = dataDate;
      }
    });
    arrMain.push(set1);
  });
  if (arrMain.length > 0) {
    arrP.push(JSON.stringify(arrMain));
    arrP.push(JSON.stringify(topCell));
    arrP.push("CHECK LIST");
  }

  axios
    .post(baseURL + "/api/Excel/ExportExcelFromFE", arrP, config)
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        window.open(baseURL + response.data.path);
      } else {
        swal.fire({
          title: "Error!",
          text: response.data.ms,
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    })
    .catch(() => {
      swal.close();
      swal.fire({
        title: "Error!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};
const datalists = ref([]);
const dataTime = ref();
const dataReportLocal = ref();
const arcData = ref({ data: null });

const onFilterMonth = (check) => {
  taskDateFilter.value = [];
  if (check) {
    if (weekPickerFilter.value) {
      options.value.Start_Date = weekPickerFilter.value[0];
      options.value.End_Date = weekPickerFilter.value[1];
    } else {
      options.value.Start_Date = null;
      options.value.End_Date = null;
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
    } else {
      options.value.Start_Date = null;
      options.value.End_Date = null;
    }
  }
  loadDataReport();
  filterMonth.value.hide();
};
const showDetails = (data) => {
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "task_bugcomment_get",
        par: [{ par: "bugcomment_id", va: data.bugcomment_id }],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data[0].url_file) {
        data[0].url_file = data[0].url_file.split(",");
        let arrFile = [];
        let allowedExtensions = /(\.jpg|\.jpeg|\.png|\.gif)$/i; //các tập tin cho phép
        data[0].url_file.forEach((element) => {
          if (element != "")
            if (allowedExtensions.exec(element)) {
              // Kiểm tra định dạng
              arrFile.push({
                data: element.substring(20),
                src: baseURL + element,
                checkimg: true,
                allsrc: element,
              });
              URL.revokeObjectURL(element);
            } else {
              arrFile.push({
                data: element.substring(20),
                src: baseURL + element,
                checkimg: false,
                allsrc: element,
              });
            }

          data[0].url_file = arrFile;
        });
      }

      emitter.emit("emitData", {
        type: "detailsBugcomment",
        data: {
          data: data[0],
        },
      });
    })
    .catch((error) => {
      console.log(error);
    });
};
const loadDataReport = () => {
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "task_checklist_report",
        par: [
          { par: "sdate", va: options.value.Start_Date },
          { par: "edate", va: options.value.End_Date },
          {
            par: "users",
            va:
              listUserSelected.value.length > 0
                ? listUserSelected.value.toString()
                : null,
          },
          { par: "user_id", va: store.getters.user.user_id },
        ],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      let data1 = JSON.parse(response.data.data)[1];
      let data2 = JSON.parse(response.data.data)[2];

      userReportLocal.value = data1;
      dateReportLocal.value = data[0];
      data2.forEach((element) => {
        if (element.start_date != null)
          element.start_date = moment(new Date(element.start_date)).format(
            "MM/DD/YYYY"
          );
        if (element.actual_date != null)
          element.actual_date = moment(new Date(element.actual_date)).format(
            "MM/DD/YYYY"
          );
        if (element.created_date != null)
          element.created_date = moment(new Date(element.created_date)).format(
            "MM/DD/YYYY"
          );
          if ((element.finished_date_save == null))
            element.finished_date_save = element.finished_date;
        if (element.finished_date != null)
         
        element.finished_date = moment(new Date(element.finished_date)).format(
          "MM/DD/YYYY"
        );
        if (element.end_date != null)
          element.end_date = moment(new Date(element.end_date)).format(
            "MM/DD/YYYY"
          );
        if (element.switch_test_date != null)
          element.switch_test_date = moment(
            new Date(element.switch_test_date)
          ).format("MM/DD/YYYY");
      });

      dataReportLocal.value = data2;

      var arrData = [];
      datalists.value = [];
      dataTime.value = dateReportLocal.value.days.split(",");
      dateReportLocal.value.days.split(",").forEach((element) => {
        let completed = [];
        dataReportLocal.value.forEach((x) => {
          if (
            new Date(x.start_date) <= new Date(element) &&
            new Date(element) <= new Date()
          ) {
            if (!arrData[element]) arrData[element] = [];
            arrData[element].push(x);
          }
          if (x.finished_date == element) completed.push(x.finished_date);
        });
        completed.forEach((element) => {
          dataReportLocal.value = dataReportLocal.value.filter(
            (x) => x.finished_date != element
          );
        });
        datalists.value[element] = groupByDefault(
          arrData[element],
          "full_name"
        );
      });
    })
    .catch((error) => {
      console.log(error);
    });
};
const switchColor = ref("B");
const optionsSwitch = ref(["B", "C"]);
const clickFilter = ref();
const onFilter = (value) => {
  clickFilter.value = value;
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "task_checklist_report",
        par: [
          { par: "sdate", va: options.value.Start_Date },
          { par: "edate", va: options.value.End_Date },
          {
            par: "users",
            va:
              listUserSelected.value.length > 0
                ? listUserSelected.value.toString()
                : null,
          },
          { par: "user_id", va: store.getters.user.user_id },
        ],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      let data1 = JSON.parse(response.data.data)[1];
      let data2 = JSON.parse(response.data.data)[2];

      userReportLocal.value = data1;
      dateReportLocal.value = data[0];
      data2.forEach((element) => {
        if (element.start_date != null)
          element.start_date = moment(new Date(element.start_date)).format(
            "MM/DD/YYYY"
          );
        if (element.actual_date != null)
          element.actual_date = moment(new Date(element.actual_date)).format(
            "MM/DD/YYYY"
          );
        if (element.created_date != null)
          element.created_date = moment(new Date(element.created_date)).format(
            "MM/DD/YYYY"
          );if ((element.finished_date_save == null))
            element.finished_date_save = element.finished_date;
        if (element.finished_date != null)
        
          element.finished_date = moment(
            new Date(element.finished_date)
          ).format("MM/DD/YYYY");
        if (element.end_date != null)
          element.end_date = moment(new Date(element.end_date)).format(
            "MM/DD/YYYY"
          );
        if (element.switch_test_date != null)
          element.switch_test_date = moment(
            new Date(element.switch_test_date)
          ).format("MM/DD/YYYY");
      });

      dataReportLocal.value = data2;

      var arrData = [];
      datalists.value = [];
      dataTime.value.forEach((element) => {
        let completed = [];
        if (value == 8) {
          dataReportLocal.value = dataReportLocal.value.filter(
            (x) => x.overtime == true
          );
        }
        if (value == 7) {
          dataReportLocal.value = dataReportLocal.value.filter(
            (x) =>
              x.finished_date == null &&
              x.end_date != null &&
              new Date(x.end_date) < new Date()
          );
        } else if (value == 6) {
          dataReportLocal.value = dataReportLocal.value.filter(
            (x) => x.status == 6
          );
        } else if (value == 4) {
          dataReportLocal.value = dataReportLocal.value.filter(
            (x) => x.status == 4
          );
        }
        if (value == 3) {
          dataReportLocal.value = dataReportLocal.value.filter(
            (x) => x.finished_date != null
          );
        }
        if (value == 2) {
          dataReportLocal.value = dataReportLocal.value.filter(
            (x) => x.switch_test_date != null && x.finished_date == null
          );
        }
        if (value == 1) {
          dataReportLocal.value = dataReportLocal.value.filter(
            (x) =>
              x.actual_date != null &&
              x.switch_test_date == null &&
              x.finished_date == null
          );
        }
        if (value == 0) {
          dataReportLocal.value = dataReportLocal.value.filter(
            (x) =>
              x.actual_date == null &&
              x.switch_test_date == null &&
              x.finished_date == null
          );
        }
        dataReportLocal.value.forEach((x) => {
          if (
            new Date(x.start_date) <= new Date(element) &&
            new Date(element) <= new Date()
          ) {
            if (!arrData[element]) arrData[element] = [];
            arrData[element].push(x);
          }
          if (x.finished_date == element) completed.push(x.finished_date);
        });
        completed.forEach((element) => {
          dataReportLocal.value = dataReportLocal.value.filter(
            (x) => x.finished_date != element
          );
        });
        datalists.value[element] = groupByDefault(
          arrData[element],
          "full_name"
        );
      });
    })
    .catch((error) => {
      console.log(error);
    });
};
const onRefresh = () => {
  clickFilter.value = null;
  if (monthPickerFilter.value) monthPickerFilter.value = null;
  if (weekPickerFilter.value) weekPickerFilter.value = null;
  taskDateFilter.value = [];
  options.value.Start_Date = null;
  options.value.End_Date = null;
  listUserSelected.value = [];
  loadDataReport();
};
onMounted(() => {
  isShowDialog.value = props.isShowDialog;
  dataReportLocal.value = props.dataReport;
  userReportLocal.value = props.userReport;
  dateReportLocal.value = props.dateReport;
  listDropdownUser.value = props.listDropdownUser;
  var arrData = [];
  datalists.value = [];
  dataTime.value = props.dateReport.days.split(",");
  props.dateReport.days.split(",").forEach((element) => {
    let completed = [];
    dataReportLocal.value.forEach((x) => {
      if (new Date(x.start_date) <= new Date(element)) {
        if (!arrData[element]) arrData[element] = [];
        arrData[element].push(x);
      }
      if (x.finished_date == element) completed.push(x.finished_date);
    });
    completed.forEach((element) => {
      dataReportLocal.value = dataReportLocal.value.filter(
        (x) => x.finished_date != element
      );
    });

    datalists.value[element] = groupByDefault(arrData[element], "full_name");
  });
});
</script>
<template>
  <div
    v-if="isShowDialog"
    class="relative overflow-hidden"
    style="overflow-y: hidden !important"
  >
    <div class="w-full format-center">
      <div>
        <h2 class="pt-0 mt-0 text-blue-700">
          Thông tin Check List từ ngày
          {{ moment(new Date(dataTime[0])).format("DD/MM/YYYY") }} -
          {{
            moment(new Date(dataTime[dataTime.length - 1])).format("DD/MM/YYYY")
          }}
        </h2>
        <div class="font-italic text-md">
          {{ store.getters.user.full_name }} -
          {{ moment(new Date()).format("HH:mm DD/MM/YYYY") }}
        </div>
        <div class="text-lg">
          <span
            class="text-900 cursor-pointer"
            :style="
              clickFilter == 0 ? 'background-color:cyan;border-radius:5px' : ''
            "
            @click="onFilter(0)"
            >Kế hoạch </span
          >|<span
            @click="onFilter(1)"
            :style="
              clickFilter == 1 ? 'background-color:cyan;border-radius:5px' : ''
            "
            class="text-blue-600 cursor-pointer"
          >
            Đang xử lý </span
          >|
          <span
            class="text-purple-600 cursor-pointer"
            :style="
              clickFilter == 2 ? 'background-color:cyan;border-radius:5px' : ''
            "
            @click="onFilter(2)"
          >
            Đang Test </span
          >|
          <span
            class="text-green-600 line-through cursor-pointer"
            :style="
              clickFilter == 3 ? 'background-color:cyan;border-radius:5px' : ''
            "
            @click="onFilter(3)"
          >
            Đã xử lý</span
          >
          |
          <span
            class="text-yellow-600 cursor-pointer"
            :style="
              clickFilter == 4 ? 'background-color:cyan;border-radius:5px' : ''
            "
            @click="onFilter(4)"
          >
            Test chưa OK</span
          >
          |

          <span
            class="text-pink-500 cursor-pointer"
            :style="
              clickFilter == 6 ? 'background-color:cyan;border-radius:5px' : ''
            "
            @click="onFilter(6)"
          >
            Làm lại</span
          >
          |
          <span
            style="color: red"
            class="cursor-pointer"
            :style="
              clickFilter == 7 ? 'background-color:cyan;border-radius:5px' : ''
            "
            @click="onFilter(7)"
            >Quá hạn</span
          >
          |
          <span
            class="text-cyan-600 cursor-pointer"
            :style="
              clickFilter == 8 ? 'background-color:cyan;border-radius:5px' : ''
            "
            @click="onFilter(8)"
            >Ngoài giờ</span
          >
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
                optionValue="code"
                optionLabel="name"
                class="col-4 ip36 p-0"
                @change="onShowReport()"
                :showClear="true"
              >
                <template #option="slotProps">
                  <div class="country-item flex">
                    <Avatar
                      :image="
                        slotProps.option.avatar
                          ? basedomainURL + slotProps.option.avatar
                          : basedomainURL + '/Portals/Image/noimg.jpg'
                      "
                      class="mr-2 w-2rem h-2rem"
                      size="large"
                      shape="circle"
                    />
                    <div class="pt-1">{{ slotProps.option.name }}</div>
                  </div>
                </template>
              </MultiSelect>

              <label for="multiselect">Danh sách nhân viên</label>
            </span>
          </div>
        </template>
        <template #end>
          <SelectButton
            class="mr-2"
            v-model="switchColor"
            :options="optionsSwitch"
            aria-labelledby="single"
          />
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
            class="mx-2 p-button-outlined p-button-secondary"
            icon="pi pi-filter"
            @click="toggleFilterMonth($event)"
            aria-haspopup="true"
            aria-controls="overlay_panelMonth"
          />
          <Button
            label="Tiện ích"
            icon="pi pi-file-excel"
            class="p-button-outlined p-button-secondary"
            @click="toggleExport"
            aria-haspopup="true"
            aria-controls="overlay_Export"
          />
          <Menu
            id="overlay_Export"
            ref="menuButs"
            :model="itemButs"
            :popup="true"
          />
        </template>
      </Toolbar>
    </div>
    <div
      style="width: 100vw; height: calc(100vh - 190px)"
      class="overflow-scroll relative api-test"
    >
      <table class="w-full mt-2" style="overflow-y: scroll">
        <tr
          style="background-color: #f8f9fa; z-index: 100000 !important"
          class="sticky top-0"
        >
          <th
            style="padding: 0.5rem; height: 50px; background-color: #f8f9fa"
            class="m-checkbox-table top-0 text-lg"
          >
            Nhân viên
          </th>
          <th
            class="text-lg"
            style="padding: 0.5rem; height: 50px"
            v-for="(item, index) in dataTime"
            :key="index"
          >
            <div class="">
              {{ moment(new Date(item)).format("DD/MM/YYYY") }}
            </div>
          </th>
        </tr>
        <tr
          style="vertical-align: top"
          v-for="(item, index) in userReportLocal"
          :key="index"
        >
          <td
            class="
              sticky
              left-0
              align-content-center
              text-center text-900
              font-bold
              p-2
              text-lg
            "
            style="background-color: #f8f9fa"
          >
            {{ item.full_name }}
          </td>
          <td v-for="(date, index1) in dataTime" :key="index1">
            <div v-if="datalists[date] != null">
              <div
                v-for="(data, index2) in datalists[date][item.full_name]"
                :key="index2"
              >
                <div
                  :style="data ? 'min-width: 300px' : 'min-width: 100px'"
                  @click="showDetails(data)"
                  class="p-2 cursor-pointer text-lg flex"
                  :class="
                    switchColor == 'B'
                      ? new Date(data.end_date) < new Date(date) &&
                        data.end_date != null
                        ? 'color-red'
                        : data.status == 4
                        ? 'text-yellow-600 '
                        : data.status == 6
                        ? 'text-pink-600 '
                        : data.finished_date == date
                        ? 'text-green-600 line-through'
                        : data.switch_test_date == date ||
                          (data.switch_test_date != null &&
                            new Date(date) > new Date(data.switch_test_date))
                        ? 'text-purple-600'
                        : new Date(data.actual_date) <= new Date(date)
                        ? 'text-blue-600'
                        : 'text-900'
                      : new Date(data.end_date) < new Date(date) &&
                        data.end_date != null
                      ? 'bg-color-red text-0'
                      : data.status == 4
                      ? 'bg-yellow-600 text-0 '
                      : data.status == 6
                      ? 'bg-pink-600 text-0  '
                      : data.finished_date == date
                      ? 'bg-green-600 text-0  line-through'
                      : data.switch_test_date == date ||
                        (data.switch_test_date != null &&
                          new Date(date) > new Date(data.switch_test_date))
                      ? ' bg-purple-600 text-0 '
                      : new Date(data.actual_date) <= new Date(date)
                      ? ' bg-blue-600 text-0 '
                      : ' text-0 surface-500'
                  "
                >
                  <div
                    v-tooltip.top="
                      'Hoàn thành: ' +
                      moment(new Date(data.finished_date_save)).format(
                        'HH:mm DD/MM/YYYY'
                      )
                    "
                  >
                
                    <div class="p-0 pr-2" v-if="data.status == 3">
                      
                       <font-awesome-icon
                        :class="
                          switchColor == 'B' ? 'text-green-500' : 'text-0'
                        "
                        class="text-sm"
                        icon="fa-solid fa-flag"
                      />
                    </div>
                  </div>

                  <div class="flex" v-tooltip.top="'Bug: ' + data.bug_name">
                    <span class="pr-2" v-if="data.status != 3">-</span>
                    <div
                      v-if="data.creator == 0"
                      v-badge.infor
                      class="relative"
                    >
                      <span> {{ data.des }}</span>
                    </div>
                    <div v-else>
                      <span>{{ data.des }}</span>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </td>
        </tr>
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
  min-width: 200px !important;
  max-width: 400px !important;
  text-align: center;
  border-right: none !important;
}
.api-test::-webkit-scrollbar {
  height: 25px !important;
}
</style>


<style lang="scss" scoped>
::v-deep(.p-sidebar) {
  .p-sidebar-content {
    padding: 0px !important;
    overflow-y: hidden;
  }
}
</style>