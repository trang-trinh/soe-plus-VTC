<script setup>
import { ref, inject, onMounted } from "vue";
import moment from "moment";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { de } from "date-fns/locale";
import dialogmodelduty from "./dialogmodelduty.vue";
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
  headerDialog: String,
  displayDialog: Boolean,
  closeDialog: Function,
  eye: Int8Array,
  currentweek: Object,
  weeks: Array,
  databoardrooms: Array,
  datatrucbans: Array,
  datachihuys: Array,
  initData: Function,
});

//Declare
const datas = ref([]);
const temps = ref([]);
const currentweek = ref(props.currentweek);
const options = ref({
  loading: false,
  eye: props.eye,
  week: currentweek.value["week_no"],
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
const timelots = ref([
  { value: 0, title: "Sáng" },
  { value: 1, title: "Chiều" },
  { value: 2, title: "Cả ngày" },
]);
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
function CreateGuid() {
  function _p8(s) {
    var p = (Math.random().toString(16) + "000000000").substr(2, 8);
    return s ? "-" + p.substr(0, 4) + "-" + p.substr(4, 4) : p;
  }
  return _p8() + _p8(true) + _p8(true) + _p8();
}

//Filter
const goWeek = (week) => {
  options.value.week = week;
  currentweek.value = props.weeks.find((x) => x["week_no"] === week);
  initWeek(true);
};

//Choose file
const files = ref([]);
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

//Function
const model = ref({});
const isAdd = ref(false);
const submitted = ref(false);
const headerDialogWeek = ref();
const displayDialogWeek = ref(false);
const openAddDialogWeek = (str, item) => {
  files.value = [];
  isAdd.value = true;
  submitted.value = false;
  model.value = {
    is_timelot: 0,
    date_timelot: null,
    status: 0,
    trucban: null,
    chihuy: null,
    boardroom_id: null,
    files: [],
  };
  headerDialogWeek.value = str;
  displayDialogWeek.value = true;
};
const addModelTemp = (item) => {
  var md = {
    calendar_duty_id: CreateGuid(),
    date_timelot: new Date(
      item["day"].getFullYear(),
      item["day"].getMonth(),
      item["day"].getDate(),
      7,
      0,
      0
    ),
    day: new Date(
      item["day"].getFullYear(),
      item["day"].getMonth(),
      item["day"].getDate(),
      7,
      0,
      0
    ),
    day_name: getDayDate(item["day"]),
    day_string: moment(item["day"]).format("DD/MM/YYYY"),
    is_holiday: new Date(item["day"]).getDay() == 0,
    is_timelot: 0,
    status: 0,
    trucban: null,
    chihuy: null,
    boardroom_id: null,
    files: [],
  };
  datas.value.push(md);
  initWeek();
};
const copyItem = (item) => {
  if (datas.value && datas.value.length > 0) {
    var start_date = new Date(currentweek.value["week_start_date"]);
    var end_date = new Date(currentweek.value["week_end_date"]);
    temps.value = datas.value.filter(
      (item) =>
        item["date_timelot"] >= start_date &&
        item["date_timelot"] <= end_date.setDate(end_date.getDate() + 1)
    );
  }
  let dateinweeks = bindDateBetweenFirstAndLast(
    new Date(currentweek.value["week_start_date"]),
    new Date(currentweek.value["week_end_date"])
  );
  dateinweeks
    .filter(
      (a) =>
        temps.value.findIndex(
          (b) => b["day_string"] === moment(a).format("DD/MM/YYYY")
        ) === -1
    )
    .forEach((day, i) => {
      var it = Object.assign({}, item);
      (it.calendar_duty_id = CreateGuid()), (it.date_timelot = day);
      it.day = day;
      it.day_name = getDayDate(day);
      (it.day_string = moment(day).format("DD/MM/YYYY")),
        (it.is_holiday = day.getDay() == 0);
      datas.value.push(it);
    });
  initWeek();
};
const closeDialogWeek = () => {
  model.value = {
    status: 0,
    is_type: 0,
    is_iterations: 0,
    distance_iterations: 0,
    numeric_iterations: 0,
    numeric_attendees: 0,
    chutris: [],
    thamgias: [],
    departments: [],
    files: [],
  };
  displayDialogWeek.value = false;
};
const saveTemp = (md) => {
  if (md["trucban"] != null) {
    if (md["trucbans"] == null) {
      md["trucbans"] = [];
    }
    md["trucbans"].push(md["trucban"]);
  }
  if (md["chihuy"] != null) {
    if (md["chihuys"] == null) {
      md["chihuys"] = [];
    }
    md["chihuys"].push(md["chihuy"]);
  }
  if (md["files"] != null && md["files"].length > 0) {
    md["files"].forEach((file) => {
      file["file_name"] = file["name"];
      file["file_type"] = file["type"].split("/").at(-1);
      file["file_size"] = file["size"];
    });
  }
  if (isAdd.value === true) {
    md["calendar_duty_id"] = CreateGuid();
    var date_timelot = new Date(md["date_timelot"]);
    md["day"] = new Date(
      date_timelot.getFullYear(),
      date_timelot.getMonth(),
      date_timelot.getDate(),
      7,
      0,
      0
    );
    md["day_name"] = getDayDate(md["day"]);
    md["day_string"] = moment(md["day"]).format("DD/MM/YYYY");
    md["is_holiday"] = new Date(md["day"]).getDay() == 0;
    var checkroom = props.databoardrooms.findIndex(
      (x) => x["boardroom_id"] === md["boardroom_id"]
    );
    if (checkroom === -1) {
      md["place_name"] = md["boardroom_id"];
      md["boardroom_name"] = md["place_name"];
    }
    datas.value.push(md);
  } else {
    var idx = datas.value.findIndex(
      (x) => x["calendar_duty_id"] === md["calendar_duty_id"]
    );
    if (idx !== -1) {
      var date_timelot = new Date(md["date_timelot"]);
      md["day"] = new Date(
        date_timelot.getFullYear(),
        date_timelot.getMonth(),
        date_timelot.getDate(),
        7,
        0,
        0
      );
      md["day_name"] = getDayDate(md["day"]);
      md["day_string"] = moment(md["day"]).format("DD/MM/YYYY");
      md["is_holiday"] = new Date(md["day"]).getDay() == 0;
      var checkroom = props.databoardrooms.findIndex(
        (x) => x["boardroom_id"] === md["boardroom_id"]
      );
      if (checkroom === -1) {
        md["place_name"] = md["boardroom_id"];
        md["boardroom_name"] = md["place_name"];
      }
      datas.value[idx] = md;
    }
  }
  closeDialogWeek();
  initWeek();
};
const changePosition = (md) => {
  if (md["trucban"] != null) {
    md["trucbans"] = [md["trucban"]];
  }
  if (md["chihuy"] != null) {
    md["chihuys"] = [md["chihuy"]];
  }
};
const openEditDialogWeek = (md) => {
  files.value = [];
  submitted.value = false;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  isAdd.value = false;
  if (md["boardroom_id"] == null && md["place_name"] != null) {
    md["boardroom_id"] = md["place_name"];
  }
  model.value = md;
  swal.close();
  headerDialogWeek.value = "Cập nhật lịch trực ban";
  displayDialogWeek.value = true;
};
const deleteItem = (md) => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  var idx = datas.value.findIndex(
    (x) => x["calendar_duty_id"] === md["calendar_duty_id"]
  );
  if (idx !== -1) {
    datas.value.splice(idx, 1);
  }
  initWeek();
  swal.close();
};
const saveModelMultiple = () => {
  if (datas.value == null || datas.value.length === 0) {
    swal.fire({
      title: "Thông báo!",
      text: "Vui lòng thêm mới lịch trực ban!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  var invalid = false;
  if (datas.value != null && datas.value.length > 0) {
    datas.value.forEach((item, i) => {
      var checkroom = props.databoardrooms.findIndex(
        (x) => x["boardroom_id"] === item["boardroom_id"]
      );
      if (checkroom === -1) {
        item["place_name"] = item["boardroom_id"];
        item["boardroom_name"] = item["place_name"];
        item["boardroom_id"] = null;
      }
      if (item["trucban"] == null) {
        invalid = true;
        return;
      }
    });
  }
  if (invalid) {
    swal.fire({
      title: "Thông báo!",
      text: "Vui lòng nhập đầy đủ thông tin trường bôi đỏ!",
      icon: "error",
      confirmButtonText: "OK",
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
  let formData = new FormData();
  var multiple = JSON.parse(JSON.stringify(datas.value));
  var members = [];
  var departments = [];
  if (multiple != null && multiple.length > 0) {
    multiple.forEach((item) => {
      //Date
      if (item["date_timelot"] != null) {
        item["date_timelot"] = moment(item["date_timelot"]).format(
          "YYYY-MM-DDTHH:mm:ssZZ"
        );
      }
      if (item["create_date"] != null) {
        item["create_date"] = moment(item["create_date"]).format(
          "YYYY-MM-DDTHH:mm:ssZZ"
        );
      }
      //Chủ trì
      if (item["trucbans"] != null && item["trucbans"].length > 0) {
        item["trucbans"].forEach((mb) => {
          var obj = {
            calendar_duty_id: item["calendar_duty_id"],
            user_id: mb["user_id"],
            is_type: 0,
          };
          members.push(obj);
        });
      }
      //Thành phần tham gia
      if (item["chihuys"] != null && item["chihuys"].length > 0) {
        item["chihuys"].forEach((mb) => {
          var obj = {
            calendar_duty_id: item["calendar_duty_id"],
            user_id: mb["user_id"],
            is_type: 1,
          };
          members.push(obj);
        });
      }
      //Files
      if (item["files"] != null && item["files"].length > 0) {
        for (var i = 0; i < item["files"].length; i++) {
          let file = item["files"][i];
          formData.append("files" + item["calendar_duty_id"], file);
        }
      }
    });
  }
  formData.append("multiple", JSON.stringify(multiple));
  formData.append("members", JSON.stringify(members));
  axios
    .put(
      baseURL + "/api/calendar_duty/update_calendar_duty_multiple",
      formData,
      config
    )
    .then((response) => {
      if (response.data.err === "1") {
        swal.fire({
          title: "Thông báo!",
          text: response.data.ms,
          icon: "error",
          confirmButtonText: "OK",
        });
      }
      swal.close();
      toast.success("Thêm lịch trực ban thành công!");
      props.closeDialog();
      props.initData(true);
    })
    .catch((error) => {
      swal.close();
      swal.fire({
        title: "Thông báo!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
      return;
    });
  if (submitted.value) submitted.value = true;
};

//Carousel
const page = ref();
const numVisible = ref(9);
const numScroll = ref(8);
var width = window.screen.width;
if (width > 1400) {
  numVisible.value = 9;
  numScroll.value = 8;
} else if (width > 1300) {
  numVisible.value = 8;
  numScroll.value = 7;
} else if (width > 1200) {
  numVisible.value = 7;
  numScroll.value = 6;
} else if (width > 1100) {
  numVisible.value = 6;
  numScroll.value = 6;
} else if (width > 1000) {
  numVisible.value = 5;
  numScroll.value = 5;
} else if (width > 900) {
  numVisible.value = 4;
  numScroll.value = 4;
} else if (width > 800) {
  numVisible.value = 3;
  numScroll.value = 3;
} else if (width > 700) {
  numVisible.value = 3;
  numScroll.value = 3;
} else if (width > 600) {
  numVisible.value = 2;
  numScroll.value = 2;
} else if (width > 500) {
  numVisible.value = 2;
  numScroll.value = 2;
} else if (width > 400) {
  numVisible.value = 1;
  numScroll.value = 1;
} else {
  numScroll.value = 1;
}
page.value = Math.floor(numScroll.value * (currentweek.value.week_no / 52));

//init week
const initWeek = (rf) => {
  if (rf) {
    options.value.loading = true;
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
  }
  temps.value = [];
  setTimeout(() => {
    if (datas.value && datas.value.length > 0) {
      var start_date = new Date(currentweek.value["week_start_date"]);
      var end_date = new Date(currentweek.value["week_end_date"]);
      temps.value = datas.value.filter(
        (item) =>
          item["date_timelot"] >= start_date &&
          item["date_timelot"] <= end_date.setDate(end_date.getDate() + 1)
      );
    }
    let dateinweeks = bindDateBetweenFirstAndLast(
      new Date(currentweek.value["week_start_date"]),
      new Date(currentweek.value["week_end_date"])
    );
    dateinweeks
      .filter(
        (a) =>
          temps.value.findIndex(
            (b) => b["day_string"] === moment(a).format("DD/MM/YYYY")
          ) === -1
      )
      .forEach((day, i) => {
        temps.value.push({
          day: day,
          day_name: getDayDate(day),
          day_string: moment(day).format("DD/MM/YYYY"),
          is_holiday: day.getDay() == 0,
        });
      });
    temps.value = temps.value.sort(function (a, b) {
      return new Date(a["day"]) - new Date(b["day"]);
    });
    swal.close();
    if (options.value.loading) options.value.loading = false;
  }, 100);
};
onMounted(() => {
  initWeek(true);
  window.addEventListener("resize", (event) => {
    var width = window.screen.width;
    if (width > 1400) {
      numVisible.value = 9;
      numScroll.value = 8;
    } else if (width > 1300) {
      numVisible.value = 8;
      numScroll.value = 7;
    } else if (width > 1200) {
      numVisible.value = 7;
      numScroll.value = 6;
    } else if (width > 1100) {
      numVisible.value = 6;
      numScroll.value = 6;
    } else if (width > 1000) {
      numVisible.value = 5;
      numScroll.value = 5;
    } else if (width > 900) {
      numVisible.value = 4;
      numScroll.value = 4;
    } else if (width > 800) {
      numVisible.value = 3;
      numScroll.value = 3;
    } else if (width > 700) {
      numVisible.value = 3;
      numScroll.value = 3;
    } else if (width > 600) {
      numVisible.value = 2;
      numScroll.value = 2;
    } else if (width > 500) {
      numVisible.value = 2;
      numScroll.value = 2;
    } else if (width > 400) {
      numVisible.value = 1;
      numScroll.value = 1;
    } else {
      numScroll.value = 1;
    }
    page.value = Math.floor(numScroll.value * (currentweek.value.week_no / 52));
  });
  return {};
});
</script>
<template>
  <Dialog
    :header="props.headerDialog"
    v-model:visible="props.displayDialog"
    :style="{ width: '80vw' }"
    :maximizable="true"
    :closable="false"
    style="z-index: 1000"
  >
    <form>
      <div class="row m-2">
        <div class="col-12 md:col-12">
          <Carousel
            :value="weeks"
            :page="page"
            :numVisible="numVisible"
            :numScroll="numScroll"
            class="custom-carousel"
            :circular="false"
            :responsiveOptions="[
              {
                breakpoint: '1400px',
                numVisible: 9,
                numScroll: 8,
              },
              {
                breakpoint: '1300px',
                numVisible: 8,
                numScroll: 7,
              },
              {
                breakpoint: '1200px',
                numVisible: 7,
                numScroll: 6,
              },
              {
                breakpoint: '1100px',
                numVisible: 6,
                numScroll: 5,
              },
              {
                breakpoint: '1000px',
                numVisible: 5,
                numScroll: 4,
              },
              {
                breakpoint: '900px',
                numVisible: 4,
                numScroll: 3,
              },
              {
                breakpoint: '800px',
                numVisible: 3,
                numScroll: 2,
              },
              {
                breakpoint: '700px',
                numVisible: 3,
                numScroll: 1,
              },
              {
                breakpoint: '600px',
                numVisible: 2,
                numScroll: 1,
              },
              {
                breakpoint: '500px',
                numVisible: 2,
                numScroll: 1,
              },
              {
                breakpoint: '400px',
                numVisible: 1,
                numScroll: 1,
              },
            ]"
          >
            <template #header>
              <h4 class="mt-0">Chọn tuần đặt lịch</h4>
            </template>
            <template #item="slotProps">
              <div
                @click="goWeek(slotProps.data.week_no)"
                class="format-grid-center item-week"
                :class="{
                  pass: slotProps.data.pass,
                  current: slotProps.data.week_no === options.week,
                }"
              >
                <b class="mb-2">Tuần {{ slotProps.data.week_no }}</b>
                <div>
                  {{
                    moment(new Date(slotProps.data.week_start_date)).format(
                      "DD/MM/YYYY"
                    )
                  }}
                </div>
                <div>
                  {{
                    moment(new Date(slotProps.data.week_end_date)).format(
                      "DD/MM/YYYY"
                    )
                  }}
                </div>
              </div>
            </template>
          </Carousel>
        </div>
        <div class="col-12 md:col-12">
          <div class="form-group">
            <label>Chi tiết đặt lịch trực ban tuần</label>
            <div class="lang-table">
              <div v-if="options.eye === 0">
                <DataTable
                  @sort="onSort($event)"
                  :value="temps"
                  :totalRecords="options.total"
                  :lazy="true"
                  :rowHover="true"
                  :showGridlines="true"
                  :scrollable="false"
                  v-model:selection="selectedNodes"
                  dataKey="calendar_duty_id"
                  scrollHeight="flex"
                  filterDisplay="menu"
                  filterMode="lenient"
                  rowGroupMode="rowspan"
                  groupRowsBy="day_string"
                >
                  <Column
                    field="day_string"
                    header=""
                    headerStyle="text-align:center;width:50px;height:50px;"
                    bodyStyle="text-align:center;width:50px;max-height:60px"
                    class="align-items-center justify-content-center text-center"
                  >
                    <template #body="slotProps">
                      <div class="format-center">
                        <Button
                          @click="
                            //openAddDialogWeek('Thêm mới lịch trực ban',slotProps.data)
                            addModelTemp(slotProps.data)
                          "
                          class="p-button-rounded p-button-secondary p-button-outlined mx-1"
                          type="button"
                          icon="pi pi-plus-circle"
                          v-tooltip.top="'Thêm lịch trực ban'"
                        ></Button>
                      </div>
                    </template>
                  </Column>
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
                    field="contents"
                    header="Nội dung"
                    headerStyle="height:50px;max-width:auto;min-width:150px;"
                    bodyStyle="max-height:60px;"
                  >
                    <template #body="slotProps">
                      <Textarea
                        v-if="slotProps.data.calendar_duty_id != null"
                        v-model="slotProps.data.contents"
                        :autoResize="true"
                        rows="2"
                        style="width: 100%"
                      />
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
                      <Dropdown
                        v-if="slotProps.data.calendar_duty_id != null"
                        :options="timelots"
                        :filter="false"
                        :showClear="false"
                        :editable="false"
                        v-model="slotProps.data.is_timelot"
                        optionLabel="title"
                        optionValue="value"
                        placeholder="Chọn ca trực"
                        class="ip36"
                        @change="changeTimelot(slotProps.data.is_timelot)"
                      >
                        <template #option="slotProps">
                          <div class="country-item flex align-items-center">
                            <div class="pt-1 pl-2">
                              {{ slotProps.option.title }}
                            </div>
                          </div>
                        </template>
                      </Dropdown>
                    </template>
                  </Column> -->
                  <Column
                    field="trucbans"
                    headerStyle="text-align:center;width:150px;height:50px"
                    bodyStyle="text-align:center;width:150px;"
                    class="align-items-center justify-content-center text-center"
                  >
                    <template #header>
                      <div><b>Trực ban </b><span class="redsao">(*)</span></div>
                    </template>
                    <template #body="slotProps">
                      <Dropdown
                        v-if="slotProps.data.calendar_duty_id"
                        :options="props.datatrucbans"
                        :filter="true"
                        :showClear="true"
                        :editable="false"
                        optionLabel="full_name"
                        placeholder="Chọn người trực ban"
                        v-model="slotProps.data.trucban"
                        class="ip36"
                        style="height: auto; min-height: 36px"
                        :change="changePosition(slotProps.data)"
                      >
                        <template #value="slotProps">
                          <div class="mt-2" v-if="slotProps.value">
                            <Chip
                              :image="slotProps.value.avatar"
                              :label="slotProps.value.full_name"
                              class="mr-2 mb-2 pl-0"
                            >
                              <div class="flex">
                                <div class="format-flex-center">
                                  <Avatar
                                    v-bind:label="
                                      slotProps.value.avatar
                                        ? ''
                                        : (
                                            slotProps.value.last_name ?? ''
                                          ).substring(0, 1)
                                    "
                                    v-bind:image="
                                      slotProps.value.avatar
                                        ? basedomainURL + slotProps.value.avatar
                                        : basedomainURL +
                                          '/Portals/Image/noimg.jpg'
                                    "
                                    style="
                                      background-color: #2196f3;
                                      color: #ffffff;
                                      width: 2rem;
                                      height: 2rem;
                                    "
                                    :style="{
                                      background:
                                        bgColor[slotProps.value.is_order % 7],
                                    }"
                                    class="mr-2 text-avatar"
                                    size="xlarge"
                                    shape="circle"
                                  />
                                </div>
                                <div class="format-flex-center">
                                  <span>{{ slotProps.value.full_name }}</span>
                                </div>
                              </div>
                            </Chip>
                          </div>
                          <span v-else> {{ slotProps.placeholder }} </span>
                        </template>
                        <template #option="slotProps">
                          <div v-if="slotProps.option" class="flex">
                            <div class="format-center">
                              <Avatar
                                v-bind:label="
                                  slotProps.option.avatar
                                    ? ''
                                    : slotProps.option.last_name.substring(0, 1)
                                "
                                v-bind:image="
                                  slotProps.option.avatar
                                    ? basedomainURL + slotProps.option.avatar
                                    : basedomainURL + '/Portals/Image/noimg.jpg'
                                "
                                style="
                                  background-color: #2196f3;
                                  color: #ffffff;
                                  width: 3rem;
                                  height: 3rem;
                                  font-size: 1.4rem !important;
                                "
                                :style="{
                                  background:
                                    bgColor[slotProps.option.is_order % 7],
                                }"
                                class="text-avatar"
                                size="xlarge"
                                shape="circle"
                              />
                            </div>
                            <div class="ml-3">
                              <div class="mb-1">
                                {{ slotProps.option.full_name }}
                              </div>
                              <div class="description">
                                <div>{{ slotProps.option.position_name }}</div>
                                <div>
                                  {{ slotProps.option.department_name }}
                                </div>
                              </div>
                            </div>
                          </div>
                          <span v-else> Chưa có dữ liệu tuần </span>
                        </template>
                      </Dropdown>
                    </template>
                  </Column>
                  <Column
                    field="chihuys"
                    header="Chỉ huy"
                    headerStyle="text-align:center;width:150px;height:50px"
                    bodyStyle="text-align:center;width:150px;"
                    class="align-items-center justify-content-center text-center"
                  >
                    <template #body="slotProps">
                      <Dropdown
                        v-if="slotProps.data.calendar_duty_id != null"
                        :options="props.datachihuys"
                        :filter="true"
                        :showClear="true"
                        :editable="false"
                        optionLabel="full_name"
                        placeholder="Chọn người chỉ huy"
                        v-model="slotProps.data.chihuy"
                        class="ip36"
                        style="height: auto; min-height: 36px"
                      >
                        <template #value="slotProps">
                          <div class="mt-2" v-if="slotProps.value">
                            <Chip
                              :image="slotProps.value.avatar"
                              :label="slotProps.value.full_name"
                              class="mr-2 mb-2 pl-0"
                            >
                              <div class="flex">
                                <div class="format-flex-center">
                                  <Avatar
                                    v-bind:label="
                                      slotProps.value.avatar
                                        ? ''
                                        : (
                                            slotProps.value.last_name ?? ''
                                          ).substring(0, 1)
                                    "
                                    v-bind:image="
                                      slotProps.value.avatar
                                        ? basedomainURL + slotProps.value.avatar
                                        : basedomainURL +
                                          '/Portals/Image/noimg.jpg'
                                    "
                                    style="
                                      background-color: #2196f3;
                                      color: #ffffff;
                                      width: 2rem;
                                      height: 2rem;
                                    "
                                    :style="{
                                      background:
                                        bgColor[slotProps.value.is_order % 7],
                                    }"
                                    class="mr-2 text-avatar"
                                    size="xlarge"
                                    shape="circle"
                                  />
                                </div>
                                <div class="format-flex-center">
                                  <span>{{ slotProps.value.full_name }}</span>
                                </div>
                              </div>
                            </Chip>
                          </div>
                          <span v-else> {{ slotProps.placeholder }} </span>
                        </template>
                        <template #option="slotProps">
                          <div v-if="slotProps.option" class="flex">
                            <div class="format-center">
                              <Avatar
                                v-bind:label="
                                  slotProps.option.avatar
                                    ? ''
                                    : slotProps.option.last_name.substring(0, 1)
                                "
                                v-bind:image="
                                  slotProps.option.avatar
                                    ? basedomainURL + slotProps.option.avatar
                                    : basedomainURL + '/Portals/Image/noimg.jpg'
                                "
                                style="
                                  background-color: #2196f3;
                                  color: #ffffff;
                                  width: 3rem;
                                  height: 3rem;
                                  font-size: 1.4rem !important;
                                "
                                :style="{
                                  background:
                                    bgColor[slotProps.option.is_order % 7],
                                }"
                                class="text-avatar"
                                size="xlarge"
                                shape="circle"
                              />
                            </div>
                            <div class="ml-3">
                              <div class="mb-1">
                                {{ slotProps.option.full_name }}
                              </div>
                              <div class="description">
                                <div>{{ slotProps.option.position_name }}</div>
                                <div>
                                  {{ slotProps.option.department_name }}
                                </div>
                              </div>
                            </div>
                          </div>
                          <span v-else> Chưa có dữ liệu tuần </span>
                        </template>
                      </Dropdown>
                    </template>
                  </Column>
                  <Column
                    field="boardroom_name"
                    header="Địa điểm"
                    headerStyle="text-align:center;width:150px;height:50px"
                    bodyStyle="text-align:center;width:150px;"
                    class="align-items-center justify-content-center text-center"
                  >
                    <template #body="slotProps">
                      <Dropdown
                        v-if="slotProps.data.calendar_duty_id"
                        :options="props.databoardrooms"
                        :filter="true"
                        :showClear="true"
                        :editable="true"
                        v-model="slotProps.data.boardroom_id"
                        optionLabel="boardroom_name"
                        optionValue="boardroom_id"
                        placeholder="Chọn địa điểm"
                        class="ip36"
                      >
                        <template #option="slotProps">
                          <div class="country-item flex align-items-center">
                            <div class="pt-1 pl-2">
                              {{ slotProps.option.boardroom_name }}
                            </div>
                          </div>
                        </template>
                      </Dropdown>
                    </template>
                  </Column>
                  <Column
                    header="Chức năng"
                    class="align-items-center justify-content-center text-center"
                    headerStyle="text-align:center;width:100px;height:50px"
                    bodyStyle="text-align:center;max-width:100px;max-height:60px"
                  >
                    <template #body="slotProps">
                      <div>
                        <Button
                          v-if="slotProps.data.calendar_duty_id != null"
                          @click="openEditDialogWeek(slotProps.data)"
                          class="p-button-rounded p-button-secondary p-button-outlined mx-1 mb-2"
                          type="button"
                          icon="pi pi-pencil"
                          v-tooltip.top="'Sửa'"
                        ></Button>
                        <Button
                          v-if="slotProps.data.calendar_duty_id != null"
                          @click="deleteItem(slotProps.data)"
                          class="p-button-rounded p-button-secondary p-button-outlined mx-1 mb-2"
                          type="button"
                          v-tooltip.top="'Xóa'"
                          icon="pi pi-trash"
                        ></Button>
                      </div>
                    </template>
                  </Column>
                  <template #empty>
                    <div
                      class="align-items-center justify-content-center p-4 text-center m-auto"
                      v-if="!isFirst || options.total == 0"
                      style="display: flex; height: calc(100vh - 225px)"
                    ></div>
                  </template>
                </DataTable>
              </div>
              <div v-if="options.eye === 1">
                <DataTable
                  @sort="onSort($event)"
                  :value="temps"
                  :lazy="true"
                  :rowHover="true"
                  :showGridlines="true"
                  :scrollable="false"
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
                    header=""
                    headerStyle="text-align:center;width:50px;height:50px;"
                    bodyStyle="text-align:center;width:50px;max-height:60px"
                    class="align-items-center justify-content-center text-center"
                  >
                    <template #body="slotProps">
                      <div class="format-center">
                        <Button
                          @click="
                            //openAddDialogWeek('Thêm mới lịch trực ban',slotProps.data)
                            addModelTemp(slotProps.data)
                          "
                          class="p-button-rounded p-button-secondary p-button-outlined mx-1"
                          type="button"
                          icon="pi pi-plus-circle"
                          v-tooltip.top="'Thêm lịch trực ban'"
                        ></Button>
                      </div>
                    </template>
                  </Column>
                  <Column
                    field="trucbans"
                    headerStyle="text-align:center;width:180px;height:50px"
                    bodyStyle="text-align:center;width:180px;max-height:60px"
                    class="align-items-center justify-content-center text-center"
                  >
                    <template #header>
                      <div>
                        <b>Họ và tên </b><span class="redsao">(*)</span>
                      </div>
                    </template>
                    <template #body="slotProps">
                      <Dropdown
                        v-if="slotProps.data.calendar_duty_id"
                        :options="props.datatrucbans"
                        :filter="true"
                        :showClear="true"
                        :editable="false"
                        :class="{
                          'p-invalid': !slotProps.data.trucban && submitted,
                        }"
                        optionLabel="full_name"
                        placeholder="Chọn người trực ban"
                        v-model="slotProps.data.trucban"
                        class="ip36"
                        style="height: auto; min-height: 36px"
                        :change="changePosition(slotProps.data)"
                      >
                        <template #value="slotProps">
                          <div class="mt-2" v-if="slotProps.value">
                            <Chip
                              :image="slotProps.value.avatar"
                              :label="slotProps.value.full_name"
                              class="mr-2 mb-2 pl-0"
                            >
                              <div class="flex">
                                <div class="format-flex-center">
                                  <Avatar
                                    v-bind:label="
                                      slotProps.value.avatar
                                        ? ''
                                        : (
                                            slotProps.value.last_name ?? ''
                                          ).substring(0, 1)
                                    "
                                    v-bind:image="
                                      slotProps.value.avatar
                                        ? basedomainURL + slotProps.value.avatar
                                        : basedomainURL +
                                          '/Portals/Image/noimg.jpg'
                                    "
                                    style="
                                      background-color: #2196f3;
                                      color: #ffffff;
                                      width: 2rem;
                                      height: 2rem;
                                    "
                                    :style="{
                                      background:
                                        bgColor[slotProps.value.is_order % 7],
                                    }"
                                    class="mr-2 text-avatar"
                                    size="xlarge"
                                    shape="circle"
                                  />
                                </div>
                                <div class="format-flex-center">
                                  <span>{{ slotProps.value.full_name }}</span>
                                </div>
                              </div>
                            </Chip>
                          </div>
                          <span v-else> {{ slotProps.placeholder }} </span>
                        </template>
                        <template #option="slotProps">
                          <div v-if="slotProps.option" class="flex">
                            <div class="format-center">
                              <Avatar
                                v-bind:label="
                                  slotProps.option.avatar
                                    ? ''
                                    : slotProps.option.last_name.substring(0, 1)
                                "
                                v-bind:image="
                                  slotProps.option.avatar
                                    ? basedomainURL + slotProps.option.avatar
                                    : basedomainURL + '/Portals/Image/noimg.jpg'
                                "
                                style="
                                  background-color: #2196f3;
                                  color: #ffffff;
                                  width: 3rem;
                                  height: 3rem;
                                  font-size: 1.4rem !important;
                                "
                                :style="{
                                  background:
                                    bgColor[slotProps.option.is_order % 7],
                                }"
                                class="text-avatar"
                                size="xlarge"
                                shape="circle"
                              />
                            </div>
                            <div class="ml-3">
                              <div class="mb-1">
                                {{ slotProps.option.full_name }}
                              </div>
                              <div class="description">
                                <div>{{ slotProps.option.position_name }}</div>
                                <div>
                                  {{ slotProps.option.department_name }}
                                </div>
                              </div>
                            </div>
                          </div>
                          <span v-else> Chưa có dữ liệu tuần </span>
                        </template>
                      </Dropdown>
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
                          slotProps.data.trucbans &&
                          slotProps.data.trucbans.length > 0
                        "
                      >
                        {{ slotProps.data.trucbans[0].rank }}
                        <span
                          v-if="
                            slotProps.data.trucbans[0].rank &&
                            slotProps.data.trucbans[0].position_name
                          "
                          >,
                        </span>
                        {{ slotProps.data.trucbans[0].position_name }}
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
                      <Dropdown
                        v-if="slotProps.data.calendar_duty_id != null"
                        :options="timelots"
                        :filter="false"
                        :showClear="false"
                        :editable="false"
                        v-model="slotProps.data.is_timelot"
                        optionLabel="title"
                        optionValue="value"
                        placeholder="Chọn ca trực"
                        class="ip36"
                        @change="changeTimelot(slotProps.data.is_timelot)"
                      >
                        <template #option="slotProps">
                          <div class="country-item flex align-items-center">
                            <div class="pt-1 pl-2">
                              {{ slotProps.option.title }}
                            </div>
                          </div>
                        </template>
                      </Dropdown>
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
                    bodyStyle="text-align:center;width:15180px0px;max-height:60px"
                    class="align-items-center justify-content-center text-center"
                  >
                    <template #body="slotProps">
                      <Dropdown
                        v-if="slotProps.data.calendar_duty_id != null"
                        :options="props.datachihuys"
                        :filter="true"
                        :showClear="true"
                        :editable="false"
                        optionLabel="full_name"
                        placeholder="Chọn người chỉ huy"
                        v-model="slotProps.data.chihuy"
                        class="ip36"
                        style="height: auto; min-height: 36px"
                      >
                        <template #value="slotProps">
                          <div class="mt-2" v-if="slotProps.value">
                            <Chip
                              :image="slotProps.value.avatar"
                              :label="slotProps.value.full_name"
                              class="mr-2 mb-2 pl-0"
                            >
                              <div class="flex">
                                <div class="format-flex-center">
                                  <Avatar
                                    v-bind:label="
                                      slotProps.value.avatar
                                        ? ''
                                        : (
                                            slotProps.value.last_name ?? ''
                                          ).substring(0, 1)
                                    "
                                    v-bind:image="
                                      slotProps.value.avatar
                                        ? basedomainURL + slotProps.value.avatar
                                        : basedomainURL +
                                          '/Portals/Image/noimg.jpg'
                                    "
                                    style="
                                      background-color: #2196f3;
                                      color: #ffffff;
                                      width: 2rem;
                                      height: 2rem;
                                    "
                                    :style="{
                                      background:
                                        bgColor[slotProps.value.is_order % 7],
                                    }"
                                    class="mr-2 text-avatar"
                                    size="xlarge"
                                    shape="circle"
                                  />
                                </div>
                                <div class="format-flex-center">
                                  <span>{{ slotProps.value.full_name }}</span>
                                </div>
                              </div>
                            </Chip>
                          </div>
                          <span v-else> {{ slotProps.placeholder }} </span>
                        </template>
                        <template #option="slotProps">
                          <div v-if="slotProps.option" class="flex">
                            <div class="format-center">
                              <Avatar
                                v-bind:label="
                                  slotProps.option.avatar
                                    ? ''
                                    : slotProps.option.last_name.substring(0, 1)
                                "
                                v-bind:image="
                                  slotProps.option.avatar
                                    ? basedomainURL + slotProps.option.avatar
                                    : basedomainURL + '/Portals/Image/noimg.jpg'
                                "
                                style="
                                  background-color: #2196f3;
                                  color: #ffffff;
                                  width: 3rem;
                                  height: 3rem;
                                  font-size: 1.4rem !important;
                                "
                                :style="{
                                  background:
                                    bgColor[slotProps.option.is_order % 7],
                                }"
                                class="text-avatar"
                                size="xlarge"
                                shape="circle"
                              />
                            </div>
                            <div class="ml-3">
                              <div class="mb-1">
                                {{ slotProps.option.full_name }}
                              </div>
                              <div class="description">
                                <div>{{ slotProps.option.position_name }}</div>
                                <div>
                                  {{ slotProps.option.department_name }}
                                </div>
                              </div>
                            </div>
                          </div>
                          <span v-else> Chưa có dữ liệu tuần </span>
                        </template>
                      </Dropdown>
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
                          v-if="slotProps.data.calendar_duty_id != null"
                          @click="copyItem(slotProps.data)"
                          class="p-button-rounded p-button-secondary p-button-outlined mx-1 mb-2"
                          type="button"
                          icon="pi pi-copy"
                          v-tooltip.top="
                            'Nhân bản lịch tất các ngày còn lại trong tuần'
                          "
                        ></Button>
                        <Button
                          v-if="slotProps.data.calendar_duty_id != null"
                          @click="openEditDialogWeek(slotProps.data)"
                          class="p-button-rounded p-button-secondary p-button-outlined mx-1 mb-2"
                          type="button"
                          icon="pi pi-pencil"
                          v-tooltip.top="'Sửa'"
                        ></Button>
                        <Button
                          v-if="slotProps.data.calendar_duty_id != null"
                          @click="deleteItem(slotProps.data)"
                          class="p-button-rounded p-button-secondary p-button-outlined mx-1 mb-2"
                          type="button"
                          v-tooltip.top="'Xóa'"
                          icon="pi pi-trash"
                        ></Button>
                      </div>
                    </template>
                  </Column>
                  <template #empty>
                    <div
                      class="align-items-center justify-content-center p-4 text-center m-auto"
                      v-if="
                        !options.loading && (!isFirst || options.total == 0)
                      "
                    >
                      <div style="height: calc(100vh)"></div>
                    </div>
                  </template>
                </DataTable>
              </div>
            </div>
          </div>
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="props.closeDialog()"
        class="p-button-text"
      />
      <Button label="Lưu" icon="pi pi-check" @click="saveModelMultiple()" />
    </template>
  </Dialog>

  <!--Model-->
  <dialogmodelduty
    :temp="true"
    :headerDialog="headerDialogWeek"
    :displayDialog="displayDialogWeek"
    :closeDialog="closeDialogWeek"
    :isAdd="isAdd"
    :submitted="submitted"
    :model="model"
    :files="files"
    :selectFile="selectFile"
    :removeFile="removeFile"
    :initData="saveTemp"
    :databoardrooms="props.databoardrooms"
    :datatrucbans="props.datatrucbans"
    :datachihuys="props.datachihuys"
  />
</template>
<style scoped>
@import url(./stylecalendar.css);
</style>
<style lang="scss" scoped>
::v-deep(.lang-table) {
  .p-datatable-thead .justify-content-center .p-column-header-content {
    justify-content: center !important;
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
