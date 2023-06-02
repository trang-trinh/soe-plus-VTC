<script setup>
import { ref, inject, onMounted, onBeforeUnmount, watch } from "vue";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
import { required } from "@vuelidate/validators";
import { encr } from "../../util/function.js";
import moment from "moment";
import DetailedWork from "../../components/task_origin/DetailedWork.vue";
import { VuemojiPicker } from "vuemoji-picker";
const closeDetail = () => {
  showDetail.value = false;
  selectedTaskID.value = null;
  loadData(true);
};
const cryoptojs = inject("cryptojs");
const first = ref(0);
const toast = useToast();
const swal = inject("$swal");
const store = inject("store");
const axios = inject("axios"); // inject axios
const emitter = inject("emitter");
const basedomainURL = fileURL;
const user = store.state.user;
const PositionSideBarCalender = ref("right");
const width1 = ref(window.screen.availWidth);

const props = defineProps({
  id: String,
});

const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
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

const opition = ref({
  IsNext: true,
  sort: "modified_date",
  ob: "DESC",
  PageNo: 0,
  PageSize: 20,
  search: "",
  Filteruser_id: null,
  user_id: store.getters.user_id,
  IsType: -1,
  filter_type: 0,
  sdate: null,
  edate: null,
  loctitle: "Lọc",
  type_view: 2,
  type_group_view: null,
  filter_date: null,
  filter_duan: null,
  filter_taskgroup: null,
});

const listProject = ref([]);
const GrandsDate = ref([]);
const Grands = ref();
const WeekDay = ref([
  { value: "Monday", text: "T2", bg: "" },
  { value: "Tuesday", text: "T3", bg: "" },
  { value: "Wednesday", text: "T4", bg: "" },
  { value: "Thursday", text: "T5", bg: "" },
  { value: "Friday", text: "T6", bg: "" },
  { value: "Saturday", text: "T7", bg: "aliceblue" },
  { value: "Sunday", text: "CN", bg: "antiquewhite" },
]);

const ChangeFilterDate = (time) => {
  var startDate = new Date(time.getFullYear(), time.getMonth(), 1);
  var endDate = new Date(time.getFullYear(), time.getMonth() + 1, 0);
  getDates(startDate, endDate);
};

const getDates = (startDate, endDate) => {
  var dateArray = [];
  var currentDate = moment(startDate);
  var stopDate = moment(endDate);
  while (currentDate <= stopDate && currentDate) {
    var d = moment.utc(currentDate).toDate();
    var date = new Date();
    var currentYear = date.getFullYear();
    var currentMonth = date.getMonth() + 1;

    dateArray.push({
      DayN: moment(currentDate).format("DD"),
      DW: d.getDay(),
      Day: parseInt(moment(currentDate).format("DD")),
      DayName: WeekDay.value.filter(
        (x) => x.value == d.toLocaleString("en-us", { weekday: "long" }),
      )[0]["text"],

      bg: WeekDay.value.filter(
        (x) => x.value == d.toLocaleString("en-us", { weekday: "long" }),
      )[0].bg,
      color:
        parseInt(moment(currentDate).format("DD/MM/YYYY")) ==
        parseInt(moment(new Date()).format("DD/MM/YYYY"))
          ? "#ff0000"
          : "",
      totalDayCurrent: getDaysInMonth(currentYear, currentMonth),
      currentDate: currentDate,
      Month: d.getMonth(),
      Year: d.getFullYear(),
    });
    currentDate = moment(currentDate).add(1, "days");
  }
  listProject.value.forEach(function (d) {
    var dates = [];
    var bd = new Date(d.start_date);
    bd.setHours(0, 0, 0, 0);

    dateArray.forEach(function (t, i) {
      var to = { DW: t.DW, Day: t.Day, totalDay: 0 };
      if (
        new Date(t.currentDate) >= bd &&
        new Date(t.currentDate) <=
          new Date(d.finish_date != null ? d.finish_date : new Date())
      ) {
        to.IsCheck = true;
        to.Name = d.task_name;
        to.progress = d.progress;
        to.totalDay = to.totalDay + 1;
        if (i > 0 && dates[i - 1].IsCheck) {
          to.IsHide = true;
        } else {
          to.IsHide = false;
        }
      } else {
        to.IsHide = false;
      }
      to.color = t.color;
      to.bg = t.bg;
      dates.push(to);
    });
    d.totalDay = dates.filter((x) => x.IsCheck == true).length;
    d.dateArray = dates.filter((x) => x.IsHide == false);
  });

  GrandsDate.value = dateArray;

  var years = [];
  for (
    var i = new Date(startDate).getFullYear();
    i <= new Date(endDate).getFullYear();
    i++
  ) {
    for (var j = 0; j < 12; j++) {
      var Month = {
        Month: j + 1,
        Year: i,
        Dates: [],
        Time: new Date(startDate),
      };
      Month.Dates = dateArray.filter((x) => x.Month === j && x.Year === i);
      if (Month.Dates.length > 0) years.push(Month);
    }
  }
  Grands.value = years;
};

function getDaysInMonth(year, month) {
  return new Date(year, month, 0).getDate();
}

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
  {
    value: 5,
    text: "Chờ đánh giá",
    bg_color: "#33c9dc",
    text_color: "#FFFFFF",
  },
  { value: 6, text: "Bị trả lại", bg_color: "#ffa500", text_color: "#FFFFFF" },
  { value: 7, text: "HT sau hạn", bg_color: "#ff8b4e", text_color: "#FFFFFF" },
  { value: 8, text: "Đã đánh giá", bg_color: "#51b7ae", text_color: "#FFFFFF" },
  { value: -1, text: "Bị xóa", bg_color: "red", text_color: "#FFFFFF" },
]);

const addLog = (log) => {
  axios.post(baseURL + "/api/Proc/AddLog", log, config);
};

const interval = ref(null);
const startProgress = (datalists) => {
  interval.value = setInterval(() => {
    let newValue = Math.floor(Math.random() * 10) + 1;
    if (newValue < datalists) {
      newValue = datalists;
    }
    datalists = newValue;
  }, 5000);
};
const endProgress = () => {
  clearInterval(interval.value);
  interval.value = null;
};

const loadData = (rf) => {
  if (rf) {
    opition.value.loading = true;
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
  }
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "task_origin_list_new",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "pageno", va: opition.value.PageNo },
              { par: "pagesize", va: opition.value.PageSize },
              { par: "search", va: opition.value.search },
              { par: "sort", va: opition.value.sort },
              { par: "ob", va: opition.value.ob },
              { par: "IsType", va: opition.value.IsType },
              { par: "loc", va: opition.value.filter_type },
              { par: "sdate", va: opition.value.sdate },
              { par: "edate", va: opition.value.edate },
              { par: "filter_date", va: opition.value.filter_date },
              { par: "filter_duan", va: opition.value.filter_duan },
              { par: "filter_taskgroup", va: opition.value.filter_taskgroup },
              { par: "project_id", va: props.id ? props.id : null },
            ],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let data1 = JSON.parse(response.data.data)[0];
      if (data1.length > 0) {
        data1.forEach((element, i) => {
          element.progress = element.progress == null ? 0 : element.progress;
          element.update_date = element.modified_date
            ? element.modified_date
            : element.created_date;
          element.status_name =
            element.status != null
              ? listDropdownStatus.value.filter(
                  (x) => x.value == element.status,
                )[0]["text"]
              : "";
          element.status_name =
            element.count_extend > 0 ? "Xin gia hạn" : element.status_name;
          element.status_bg_color =
            element.status != null
              ? listDropdownStatus.value.filter(
                  (x) => x.value == element.status,
                )[0].bg_color
              : "";
          element.status_bg_color =
            element.count_extend > 0 ? "#F18636" : element.status_bg_color;
          element.status_text_color =
            element.status != null
              ? listDropdownStatus.value.filter(
                  (x) => x.value == element.status,
                )[0].text_color
              : "";
          //thời gian xử lý
          if (element.end_date != null) {
            if (element.thoigianquahan <= 0) {
              if (element.thoigianxuly > 0 && element.thoigianxuly < 60) {
                element.title_time = element.thoigianxuly + " phút";
              } else if (
                element.thoigianxuly >= 60 &&
                element.thoigianxuly < 1440
              ) {
                element.title_time =
                  Math.floor(element.thoigianxuly / 60) + " giờ";
              } else {
                element.title_time =
                  Math.floor(element.thoigianxuly / 1440) + " ngày";
              }
              element.totalDay = element.thoigianxuly;
              element.time_bg = element.status_bg_color;
              element.time_color = "color: #fff;";
            } else {
              if (element.thoigianquahan > 0) {
                if (element.thoigianquahan > 0 && element.thoigianquahan < 60) {
                  element.title_time =
                    "Quá hạn " + element.thoigianquahan + " phút";
                } else if (
                  element.thoigianquahan >= 60 &&
                  element.thoigianquahan < 1440
                ) {
                  element.title_time =
                    "Quá hạn " +
                    Math.floor(element.thoigianquahan / 60) +
                    " giờ";
                } else {
                  element.title_time =
                    "Quá hạn " +
                    Math.floor(element.thoigianquahan / 1440) +
                    " ngày";
                }
                // element.title_time = "Quá hạn " + element.thoigianquahan + " ngày";
                element.totalDay = element.thoigianquahan;
                element.time_bg = "red";
                element.time_color = "color: #fff;";
              }
            }
          } else if (element.thoigianxuly) {
            if (element.thoigianxuly > 0 && element.thoigianxuly < 60) {
              element.title_time = element.thoigianxuly + " phút";
            } else if (
              element.thoigianxuly >= 60 &&
              element.thoigianxuly < 1440
            ) {
              element.title_time =
                Math.floor(element.thoigianxuly / 60) + " giờ";
            } else {
              element.title_time =
                Math.floor(element.thoigianxuly / 1440) + " ngày";
            }
            // element.title_time = element.thoigianxuly + " ngày";
            element.totalDay = element.thoigianxuly;
            element.time_bg = element.status_bg_color;
            element.time_color = "color: #fff;";
          }

          element.Thanhviens = element.Thanhviens
            ? JSON.parse(element.Thanhviens)
            : [];
          element.ThanhvienShows = [];
          if (element.Thanhviens.length > 3) {
            element.ThanhvienShows = element.Thanhviens.slice(0, 3);
          } else {
            element.ThanhvienShows = [...element.Thanhviens];
          }
          element.files = element.files ? JSON.parse(element.files) : [];
          element.files = element.files ? element.files : [];
          element.STT = opition.value.PageNo * opition.value.PageSize + i + 1;
          startProgress(element.progress);
        });
      }

      listProject.value = data1;
      let date1 = new Date(
        opition.value.sdate ? opition.value.sdate : new Date(),
      );
      let date2 = new Date(
        opition.value.edate ? opition.value.edate : new Date(),
      );
      // var firstDay = new Date(date.getFullYear(), date.getMonth(), 1);
      // var lastDay = new Date(date.getFullYear(), date.getMonth() + 1, 0);
      var firstDay = new Date(date1.getFullYear(), date1.getMonth(), 1);
      var lastDay = new Date(date2.getFullYear(), date2.getMonth() + 1, 0);
      getDates(firstDay, lastDay);

      if (rf) {
        opition.value.loading = false;
        swal.close();
      }
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!" + error.message + "1");
      console.log(error);
      opition.value.loading = false;
      addLog({
        title: "Lỗi Console loadData",
        controller: "LogsView.vue",
        log_content: error.message,
        loai: 2,
      });
      if (error && error.status === 401) {
        swal.fire({
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};

const componentKey = ref(0);
const forceRerender = () => {
  componentKey.value += 1;
};

const showDetail = ref(false);
const selectedTaskID = ref();
const onRowSelect = (id) => {
  forceRerender();
  showDetail.value = false;
  showDetail.value = true;
  selectedTaskID.value = id.task_id;
};
const selectedKeys = ref();
const onNodeSelect = (id) => {
  forceRerender();
  showDetail.value = false;
  showDetail.value = true;
  selectedTaskID.value = id.data.task_id;
};
emitter.on("SideBar", (obj) => {
  showDetail.value = obj;
  showDetail.value = false;
  selectedTaskID.value = null;
});
const onRowUnselect = (id) => {};

onMounted(() => {
  loadData(true);
  return {};
});
</script>
<template>
  <div
    id="project-calendar"
    style="
      max-height: calc(100vh - 60px);
      min-height: calc(100vh - 60px);
      display: -webkit-box;
      overflow-x: auto;
      padding-top: 5px;
      overflow-y: hidden;
    "
    class="grid formgrid m-2"
  >
    <div
      class="field col-12 md:col-12"
      style="
        display: flex;
        padding: 0px;
        max-height: calc(100vh - 60px);
        min-height: calc(100vh - 60px);
        flex-direction: column;
      "
    >
      <div
        class="col-12 scrollbox_delayed"
        style="height: 100%; padding: 0px; overflow: auto"
      >
        <table
          class="table table-border"
          style="
            width: max-content;
            table-layout: fixed;
            min-width: 100%;
            border-collapse: collapse;
            overflow-x: scroll;
          "
        >
          <thead
            style="
              background-color: #f8f9fa;
              position: sticky;
              top: 0px;
              z-index: 5;
            "
          >
            <tr>
              <th
                class="fixcol left-0 p-3"
                rowspan="3"
                style="width: 200px; border: 1px solid #e9e9e9"
              >
                Công việc
              </th>
              <th
                class="fixcol left-200 p-3"
                rowspan="3"
                style="width: 150px; border: 1px solid #e9e9e9"
              >
                Thực hiện
              </th>
              <!-- <th class="fixcol left-350 p-3" rowspan="3" style="width: 100px; border: 1px solid #e9e9e9">
                                Bắt đầu
                            </th>
                            <th class="fixcol left-450 p-3" rowspan="3" style="width: 100px; border: 1px solid #e9e9e9">
                                Kết thúc
                            </th> -->
              <th
                v-for="m in Grands"
                class="p-3"
                align="center"
                :width="m.Dates.length * 40"
                :colspan="m.Dates.length"
                style="text-align: center; min-width: 100px; color: #2196f3"
              >
                <!-- Tháng {{ m.Month }}/{{ m.Year }}  -->
                <Calendar
                  @date-select="ChangeFilterDate(m.Time)"
                  inputId="icon"
                  v-model="m.Time"
                  :showIcon="true"
                  view="month"
                  dateFormat="Tháng mm/yy"
                />
              </th>
              <!-- <th class="p-3" style="border: 1px solid #e9e9e9;" :colspan="GrandsDate.length">Tháng 2</th> -->
            </tr>
            <tr>
              <th
                class="no-fixcol p-3"
                width="40"
                style="border: 1px solid #e9e9e9"
                :style="
                  (g.bg == ''
                    ? 'background-color: #fff;'
                    : 'background-color:' + g.bg + ';',
                  'color:' + g.color)
                "
                v-for="g in GrandsDate"
              >
                {{ g.DayName }}
              </th>
            </tr>
            <tr>
              <th
                class="no-fixcol p-3"
                width="40"
                style="border: 1px solid #e9e9e9"
                :style="
                  (g.bg == ''
                    ? 'background-color: #fff;'
                    : 'background-color:' + g.bg + ';',
                  'color:' + g.color)
                "
                v-for="g in GrandsDate"
              >
                {{ g.DayN }}
              </th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="l in listProject"
              @click="onRowSelect(l)"
            >
              <td
                class="fixcol left-0 p-3"
                style="border: 1px solid #e9e9e9; background-color: #f8f9fa"
              >
                <div>
                  <label @click="onRowSelect(l)">{{ l.task_name }}</label>
                  <div
                    style="
                      font-size: 12px;
                      margin-top: 5px;
                      display: flex;
                      align-items: center;
                    "
                  >
                    <span
                      v-if="l.start_date || l.end_date"
                      style="color: #98a9bc"
                      >{{
                        l.start_date
                          ? moment(new Date(l.start_date)).format("DD/MM/YYYY")
                          : null
                      }}
                      {{
                        l.end_date
                          ? "- " +
                            moment(new Date(l.end_date)).format("DD/MM/YYYY")
                          : null
                      }}</span
                    >
                  </div>
                </div>
              </td>
              <td
                class="fixcol left-200 p-3"
                style="border: 1px solid #e9e9e9; background-color: #f8f9fa"
              >
                <div style="display: flex; justify-content: center">
                  <AvatarGroup>
                    <div
                      v-for="(value, index) in l.ThanhvienShows"
                      :key="index"
                    >
                      <div>
                        <Avatar
                          v-tooltip.bottom="{
                            value:
                              value.type_name +
                              ': ' +
                              value.fullName +
                              '<br/>' +
                              (value.tenChucVu || '') +
                              '<br/>' +
                              (value.tenToChuc || ''),
                            escape: true,
                          }"
                          v-bind:label="
                            value.avatar
                              ? ''
                              : (value.ten ?? '').substring(0, 1)
                          "
                          v-bind:image="basedomainURL + value.avatar"
                          style="
                            background-color: #2196f3;
                            color: #ffffff;
                            width: 32px;
                            height: 32px;
                            font-size: 15px !important;
                            margin-left: -10px;
                          "
                          :style="{
                            background: bgColor[index % 7] + '!important',
                          }"
                          class="cursor-pointer"
                          size="xlarge"
                          shape="circle"
                        />
                      </div>
                    </div>
                    <Avatar
                      v-if="l.Thanhviens.length - l.ThanhvienShows.length > 0"
                      :label="
                        '+' +
                        (l.Thanhviens.length - l.ThanhvienShows.length) +
                        ''
                      "
                      class="cursor-pointer"
                      shape="circle"
                      style="
                        background-color: #e9e9e9 !important;
                        color: #98a9bc;
                        font-size: 14px !important;
                        width: 32px;
                        margin-left: -10px;
                        height: 32px;
                      "
                    />
                  </AvatarGroup>
                </div>
              </td>
              <!-- <td class="fixcol left-350 p-3"
                                style="border: 1px solid #e9e9e9; background-color: #f8f9fa; text-align: center;">
                                {{
                                    l.start_date
                                    ? moment(new Date(l.start_date)).format("DD/MM/YYYY HH:mm")
                                    : ""
                                }}
                            </td>
                            <td class="fixcol left-450 p-3"
                                style="border: 1px solid #e9e9e9; background-color: #f8f9fa; text-align: center;">
                                {{
                                    l.end_date
                                    ? moment(new Date(l.end_date)).format("DD/MM/YYYY HH:mm")
                                    : ""
                                }}
                            </td> -->
              <td
                class="no-fixcol-hover"
                style="background-color: #fff; border: 1px solid #e9e9e9"
                width="40"
                :colspan="g.IsCheck ? l.totalDay : 1"
                :style="
                  (g.Name
                    ? 'background-color: #fff;'
                    : 'background-color:' + g.bg + ';',
                  'color:' + g.color)
                "
                v-for="g in l.dateArray"
              >
                <div
                  v-if="g.IsCheck"
                  class="divbg"
                  :style="
                    'background-color:' +
                    l.status_bg_color +
                    '!important;color:' +
                    l.status_text_color
                  "
                >
                  {{ g.progress }} %
                </div>
              </td>
            </tr>
            <tr v-if="listProject.length == 0">
              <td
                :colspan="GrandsDate.length + 4"
                style="text-align: center"
              >
                <div
                  class="align-items-center justify-content-center p-4 text-center m-auto"
                  style="
                    min-height: calc(100vh - 215px);
                    max-height: calc(100vh - 215px);
                    display: flex;
                    flex-direction: column;
                  "
                  v-if="listProject != null || opition.totalRecords == 0"
                >
                  <img
                    src="../../assets/background/nodata.png"
                    height="144"
                  />
                  <h3 class="m-1">Không có dữ liệu</h3>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
  <DetailedWork
    v-if="showDetail === true"
    :id="selectedTaskID"
    :turn="0"
    :closeDetail="closeDetail"
  >
  </DetailedWork>
</template>
<style scoped>
.scrollbox_delayed:hover {
  transition: visibility 0s 0.2s;
  visibility: visible;
}

#project-calendar .table tbody tr:hover td.no-fixcol-hover {
  background-color: #e5f3ff !important;
}

#project-calendar .table thead tr .fixcol {
  z-index: 5;
  color: #000;
  font-weight: 600;
  position: sticky;
  /* background: #f5f5f5; */
  background-color: #f8f9fa;
  outline: 1px solid #e9e9e9;
  border: none;
  vertical-align: middle;
}

#project-calendar .table thead tr th {
  outline: 1px solid #e9e9e9;
}

#project-calendar .table tbody tr .fixcol {
  position: sticky;
  z-index: 0;
  color: #000;
  font-weight: 400;
  /* background: #f5f5f5; */
  background-color: #f8f9fa;
  outline: 1px solid #e9e9e9;
  border: none;
  vertical-align: middle;
}

#project-calendar .left-0 {
  left: 0px;
}

#project-calendar .left-200 {
  left: 200px;
}

#project-calendar .left-350 {
  left: 350px;
}

#project-calendar .left-450 {
  left: 450px;
}
</style>
<style scoped>
#project-calendar .table thead tr th .p-calendar-w-btn .p-inputtext {
  border: none !important;
  background-color: #f8f9fa;
  color: #2196f3;
  width: 110px;
}
.p-sidebar .p-sidebar-content {
  padding: 0px !important;
}
</style>
