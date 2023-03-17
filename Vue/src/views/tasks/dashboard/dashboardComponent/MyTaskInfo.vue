<script setup>
import { ref, inject, onMounted } from "vue";
import { useToast } from "vue-toastification";
import { encr } from "../../../../util/function.js";
import moment from "moment";
import DetailedWork from "../../../../components/task_origin/DetailedWork.vue";
const cryoptojs = inject("cryptojs");
//khai báo
const emitter = inject("emitter");
const axios = inject("axios");
const store = inject("store");
const toast = useToast();
const swal = inject("$swal");
const router = inject("router");

// eslint-disable-next-line no-undef
const basedomainURL = baseURL;
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};

const width1 = window.screen.width;
const addLog = (log) => {
  // eslint-disable-next-line no-undef
  axios.post(baseURL + "/api/Proc/AddLog", log, config);
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

const user = store.state.user;
const listCountTaskButton = ref([
  {
    label: "Tất cả",
    icon: "",
    code: "-1",
    count: 0,
    color: "#3192D3",
  },
  { label: "Được giao", icon: "", code: "0", count: 0, color: "#FF8B4E" },
  { label: "Quản lý", icon: "", code: "1", count: 0, color: "#D87777" },
  { label: "Theo dõi", icon: "", code: "2", count: 0, color: "#F17AC7" },
  { label: "Tôi tạo", icon: "", code: "3", count: 0, color: "#33C9DC" },
  { label: "Hoàn thành", icon: "", code: "4", count: 0, color: "#04D214" },
]);
const listCountIDoButton = ref([
  {
    label: "Chưa làm",
    icon: "",
    code: "",
    count: 0,
    color: "#FFFFFF",
    bgColor: "#BBBBBB",
  },
  {
    label: "Đang làm",
    icon: "",
    code: "",
    count: 0,
    color: "#FFFFFF",
    bgColor: "#2196F3",
  },
  {
    label: "Quá hạn",
    icon: "",
    code: "",
    count: 0,
    color: "#FFFFFF",
    bgColor: "#f00000",
  },
  {
    label: "Chờ đánh giá",
    icon: "",
    code: "",
    count: 0,
    color: "#FFFFFF",
    bgColor: "#33c9C3",
  },
  {
    label: "Hoàn thành",
    icon: "",
    code: "",
    count: 0,
    color: "#FFFFFF",
    bgColor: "#6FBF73",
  },
]);
const listTask = ref([]);
const chartData1 = ref();
const chartData2 = ref();
const listActive = ref([]);
const simpleDateName = (date) => {
  let numOfDate = new Date(date).getDay() + 1;
  if (numOfDate == 1) {
    return "Thứ 2" + " (" + moment(new Date(date)).format("DD/MM/YYYY") + ")";
  }
  if (numOfDate == 7) {
    return (
      "Chủ nhật" + " (" + moment(new Date(date)).format("DD/MM/YYYY") + ")"
    );
  } else {
    return (
      "Thứ " +
      numOfDate +
      " (" +
      moment(new Date(date)).format("DD/MM/YYYY") +
      ")"
    );
  }
};
const PositionSideBar = ref("right");
const page = ref();
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
const LoadActive = () => {
  axios
    .post(
      // eslint-disable-next-line no-undef
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "task_logs_dashboard",
            par: [
              { par: "user_id", va: user.user_id },
              { par: "page", va: page.value },
            ],
          }),
          // eslint-disable-next-line no-undef
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let listact = JSON.parse(response.data.data)[0];

      let listDate = [];
      listact.forEach((x) => {
        x.creator = JSON.parse(x.creator);
        let zss = listDropdownStatus.value.filter((z) => z.value == x.status);
        x.status_display = {};
        x.status_display = zss[0];
        x.creator.tooltip =
          x.creator.full_name +
          "<br/>" +
          x.creator.position_name +
          "<br/>" +
          (x.creator.department_name || x.creator.organization_name);
        let d = moment(new Date(x.created_date)).format("MM/DD/YYYY");
        if (listDate.includes(d) == false) {
          listDate.push(d);
        }
      });
      let listDate2 = [];
      listDate.forEach((x) => {
        let d = {
          date: moment(new Date(x)).format("DD/MM/YYYY"),
          date_display: simpleDateName(x),
        };
        listDate2.push(d);
      });
      listDate2.forEach((z) => {
        z.data = [];
        listact.forEach((x) => {
          if (moment(new Date(x.created_date)).format("DD/MM/YYYY") == z.date) {
            z.data.push(x);
          }
        });
      });
      listDate2.forEach((z) => {
        listActive.value.push(z);
      });
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!" + error);
      addLog({
        title: "Lỗi Console loadData",
        controller: "MyTaskInfo.vue",
        logcontent: error.message,
        loai: 2,
      });
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
const LoadCountTask = () => {
  axios
    .post(
      // eslint-disable-next-line no-undef
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "task_dashboard_count",
            par: [
              { par: "user_id", va: user.user_id },
              { par: "page", va: page.value },
            ],
          }),
          // eslint-disable-next-line no-undef
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      let listdata = JSON.parse(response.data.data)[1];
      let countToButton = JSON.parse(response.data.data)[3];
      let listDropdown = JSON.parse(response.data.data)[4];
      emitter.emit("listDropdown", { data: listDropdown });
      emitter.emit("count", { data: countToButton });
      let gotData = data[0];
      listCountTaskButton.value[0].count = gotData.TatCa;
      listCountTaskButton.value[1].count = gotData.DuocGiao;
      listCountTaskButton.value[2].count = gotData.QuanLy;
      listCountTaskButton.value[3].count = gotData.TheoDoi;
      listCountTaskButton.value[4].count = gotData.ToiTao;
      listCountTaskButton.value[5].count = gotData.HoanThanh;
      listCountIDoButton.value[0].count = gotData.ChuaLam;
      listCountIDoButton.value[1].count = gotData.DangLam;
      listCountIDoButton.value[2].count = gotData.QuaHan;
      listCountIDoButton.value[3].count = gotData.ChoDanhGia;
      listCountIDoButton.value[4].count = gotData.HoanThanh;
      listdata.forEach((element) => {
        element.members = JSON.parse(element.members);
        element.members.forEach((x, i) => {
          x.stt = i;
          x.tooltip =
            x.type_name +
            "<br/>" +
            x.full_name +
            "<br/>" +
            x.position_name +
            "<br/>" +
            (x.department_name || x.organization_name);
        });
      });
      listTask.value = listdata;
      let chart1 = {
        labels: [],
        datasets: [
          {
            data: [
              gotData.TatCa,
              gotData.DuocGiao,
              gotData.QuanLy,
              gotData.TheoDoi,
              gotData.ToiTao,
              gotData.HoanThanh,
            ],
            backgroundColor: [],
            hoverBackgroundColor: [],
          },
        ],
      };
      let chart2 = {
        labels: [],
        datasets: [
          {
            data: [
              gotData.ChuaLam,
              gotData.DangLam,
              gotData.QuaHan,
              gotData.ChoDanhGia,
              gotData.HoanThanh,
            ],
            backgroundColor: [],
            hoverBackgroundColor: [],
          },
        ],
      };
      listCountTaskButton.value.forEach((x) => {
        chart1.labels.push(x.label);
        chart1.datasets[0].backgroundColor.push(x.color);
        chart1.datasets[0].hoverBackgroundColor.push(x.color);
      });
      listCountIDoButton.value.forEach((x) => {
        chart2.labels.push(x.label);
        chart2.datasets[0].backgroundColor.push(x.bgColor);
        chart2.datasets[0].hoverBackgroundColor.push(x.bgColor);
      });

      chartData1.value = chart1;
      chartData2.value = chart2;
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!" + error);
      addLog({
        title: "Lỗi Console loadData",
        controller: "MyTaskInfo.vue",
        logcontent: error.message,
        loai: 2,
      });
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
const optionsChart1 = {
  responsive: true,
  plugins: {
    title: {
      display: true,
      position: "bottom",
      text: "Thống kê chung",
    },
    legend: {
      position: "bottom",
    },
  },
};
const optionsChart2 = {
  responsive: true,
  plugins: {
    title: {
      display: true,
      position: "bottom",
      text: "Thống kê cá nhân thực hiện",
    },
    legend: {
      position: "bottom",
    },
  },
};
const ViewTask = (id) => {
  showDetail.value = false;
  showDetail.value = true;
  selectedTaskID.value = id;
};

const showDetail = ref(false);
const selectedTaskID = ref();

emitter.on("SideBar", (obj) => {
  showDetail.value = obj;
  LoadCountTask(0);
});
emitter.on("psb", (obj) => {
  PositionSideBar.value = obj;
});
const loadMore = () => {
  page.value += 1;
  LoadActive();
};
const gotoTaskMain = (value) => {
  router.push({ name: "taskmainFilter", params: { type: value } });
};
onMounted(() => {
  page.value = 0;
  LoadCountTask(0);
  LoadActive();
});
</script>
<template>
  <div class="main-layout true">
    <div class="col-12 flex p-0 m-0">
      <div class="col-5">
        <div class="col-12 bg-white">
          <div class="col-12 font-bold">
            <i
              class="pi pi-box font-bold text-pink-300"
              id="flip-animation"
            />
            Bảng theo dõi chung công việc của tôi
          </div>
          <div class="inline-block format-left">
            <div
              class="p-1 inline-block"
              v-for="(item, index) in listCountTaskButton"
              :key="index"
            >
              <Button
                :icon="item.icon"
                class="font-bold format-center p-button-text border-1 border-round-xl border-gray-200"
                style="min-width: 6.2rem"
                @click="gotoTaskMain(item.code)"
              >
                <span>
                  <span
                    class="font-bold"
                    :style="'color:' + item.color"
                  >
                    {{ item.count }}</span
                  >
                  <br />
                  <span style="color: #72777a">{{ item.label }}</span></span
                >
              </Button>
            </div>
          </div>
          <hr />
          <div class="col-12 font-bold">
            <i
              class="pi pi-box font-bold text-pink-300"
              id="flip-animation"
            />
            Bảng theo dõi công việc cá nhân tôi thực hiện
          </div>
          <div class="inline-block format-left">
            <div
              class="p-1 inline-block"
              v-for="(item, index) in listCountIDoButton"
              :key="index"
            >
              <Button
                :icon="item.icon"
                class="font-bold format-center p-button-text border-1 border-round-xl border-gray-200"
                style="min-width: 6.2rem"
                :style="'background-color:' + item.bgColor"
              >
                <span>
                  <span
                    class="font-bold"
                    :style="'color:' + item.color"
                  >
                    {{ item.count }}</span
                  >
                  <br />
                  <span :style="'color:' + item.color">{{
                    item.label
                  }}</span></span
                >
              </Button>
            </div>
          </div>
        </div>
        <div class="p-1"></div>
        <div
          class="col-12 bg-white"
          style="height: calc(100vh - 330px)"
        >
          <div class="col-12 font-bold">Hoạt động gần đây</div>
          <div
            v-if="listActive.length > 0"
            style="height: calc(100vh - 375px); overflow: auto"
          >
            <div
              class="col-12 border-gray-500 p-0"
              v-for="(item, index) in listActive"
              :key="index"
            >
              <div
                class="col-12 font-bold text-blue-500 border-gray-500"
                style="padding: 10px; background-color: aliceblue"
              >
                {{ item.date_display }}
              </div>
              <div
                class="col-12 flex format-left border-bottom-1 border-gray-100 task-hover"
                v-for="(item, index) in item.data"
                :key="index"
              >
                <div class="co-2">
                  <Avatar
                    @error="
                      $event.target.src =
                        basedomainURL + '/Portals/Image/nouser1.png'
                    "
                    v-tooltip.right="{
                      value: item.creator.tooltip,
                      escape: true,
                    }"
                    v-bind:label="
                      item.creator.avt
                        ? ''
                        : item.creator.full_name
                            .split(' ')
                            .at(-1)
                            .substring(0, 1)
                    "
                    v-bind:image="basedomainURL + item.creator.avt"
                    style="color: #ffffff; cursor: pointer"
                    :style="{
                      background: bgColor[index % 7],
                      border: '1px solid' + bgColor[index % 7],
                    }"
                    class="p-0 myclass"
                    size="large"
                    shape="circle"
                  />
                </div>
                <div
                  class="col-7"
                  style="
                    display: flex;
                    align-items: center;
                    align-content: center;
                    flex-wrap: wrap;
                  "
                >
                  <span
                    class="col-12 py-0"
                    v-html="
                      item.description.length > 100
                        ? item.description.substring(0, 100)
                        : item.description
                    "
                  ></span>
                  <div class="col-12 text-gray-500 text-sm py-1">
                    {{ moment(item.created_date).format("HH:mm DD/MM/YYYY") }}
                  </div>
                </div>

                <div class="col-4 format-right">
                  <Button
                    :style="{
                      'background-color': item.status_display.bg_color,
                      color: item.status_display.text_color,
                    }"
                    class=""
                    :label="
                      item.task_name.length > 18
                        ? item.task_name.substring(0, 25) + '...'
                        : item.task_name
                    "
                    @click="ViewTask(item.task_id)"
                  ></Button>
                </div>
              </div>
            </div>
            <div class="col-12 flex align-items-center justify-content-center">
              <Button
                label="Xem thêm..."
                class="p-button-text"
                @click="loadMore()"
              >
              </Button>
            </div>
          </div>
          <div
            v-else
            class="align-items-center justify-content-center p-4 text-center m-auto"
            style="display: flex; flex-direction: column"
          >
            <img
              src="../../../../assets/background/nodata.png"
              height="144"
            />
            <h3 class="m-1">Không có dữ liệu</h3>
          </div>
        </div>
      </div>

      <div class="col-7">
        <div class="col-12 bg-white">
          <div class="col-12 font-bold">Công việc gần nhất đang thực hiện</div>

          <div
            style="height: 33vh; overflow: auto"
            v-if="listTask.length > 0"
          >
            <div
              class="col-12 flex task-hover"
              v-for="(item, index) in listTask"
              :key="index"
              @click="ViewTask(item.task_id)"
            >
              <div class="col-5 p-0">
                <div class="col-12 font-bold hyphens">
                  <span
                    v-tooltip="'Ưu tiên'"
                    v-if="item.is_prioritize"
                    style="margin-right: 5px"
                  >
                    <i
                      style="color: orange"
                      class="pi pi-star-fill"
                    >
                    </i>
                  </span>
                  {{
                    item.task_name.length > 100
                      ? item.task_name.substring(0, 100) + "..."
                      : item.task_name
                  }}
                </div>
                <div class="col-12 py-0 px-5">
                  <span>
                    {{ moment(new Date(item.start_date)).format("DD/MM/YYYY") }}
                    {{
                      item.is_deadline == true
                        ? " - " +
                          moment(new Date(item.end_date)).format("DD/MM/YYYY")
                        : ""
                    }}
                  </span>
                </div>
                <div
                  v-if="item.project_name"
                  class="col-12 pt-0 text-blue-500 hyphens"
                >
                  <i class="pi pi-tag mx-4" />{{ item.project_name ?? "" }}
                </div>
              </div>
              <div class="col-7 p-0 flex">
                <div class="col-2 format-center h-full">
                  <AvatarGroup>
                    <div
                      class="flex p-0 m-0"
                      v-for="(m, index) in item.members"
                      :key="m"
                    >
                      <Avatar
                        @error="
                          $event.target.src =
                            basedomainURL + '/Portals/Image/nouser1.png'
                        "
                        v-if="m.stt < 2"
                        v-tooltip.right="{
                          value: m.tooltip,
                          escape: true,
                        }"
                        v-bind:label="
                          m.avt
                            ? ''
                            : m.full_name.split(' ').at(-1).substring(0, 1)
                        "
                        v-bind:image="basedomainURL + m.avt"
                        style="color: #ffffff; cursor: pointer"
                        :style="{
                          background: bgColor[index % 7],
                          border: '1px solid' + bgColor[index % 7],
                        }"
                        class="flex myclass"
                        size="normal"
                        shape="circle"
                      />
                    </div>
                    <Avatar
                      @error="
                        $event.target.src =
                          basedomainURL + '/Portals/Image/nouser1.png'
                      "
                      v-if="item.members.length > 2"
                      v-tooltip.right="{
                        value:
                          'và ' +
                          (item.members.length - 2) +
                          ' người khác tham gia',
                      }"
                      :label="'+' + (item.members.length - 2)"
                      style="color: #ffffff; cursor: pointer"
                      :style="{
                        background: bgColor[item.members.length % 7],
                        border: '1px solid' + bgColor[item.members.length % 7],
                      }"
                      class="flex p-0 m-0"
                      size="normal"
                      shape="circle"
                    />
                  </AvatarGroup>
                </div>
                <div class="col-2 format-center flex">
                  <ProgressBar
                    v-if="item.progress != null"
                    :value="item.progress"
                    :showValue="true"
                    class="col-12 p-0 m-0"
                  />
                  <span v-else>0%</span>
                </div>
                <div class="col-3 format-center">
                  <div
                    v-if="item.end_date != null"
                    style="
                      background-color: #fff8ee;
                      padding: 10px 10px;
                      border-radius: 5px;
                    "
                  >
                    <span
                      style="color: #ffab2b; font-size: 13px; font-weight: bold"
                      >{{
                        moment(new Date(item.end_date)).format("DD/MM/YYYY")
                      }}</span
                    >
                  </div>
                </div>
                <div class="col-5 format-center">
                  <div
                    v-if="item.is_deadline == true"
                    style="
                      background-color: #fff8ee;
                      padding: 10px 10px;
                      border-radius: 5px;
                    "
                    class="w-full"
                  >
                    <span
                      v-if="item.exp_time > 0"
                      style="color: #f00000; font-size: 13px; font-weight: bold"
                      >Quá hạn {{ item.exp_time }} ngày</span
                    >
                    <span
                      v-else-if="item.exp_time < 0"
                      style="color: #04d214; font-size: 13px; font-weight: bold"
                      >Còn {{ Math.abs(item.exp_time) }} ngày</span
                    >
                    <span
                      v-else
                      style="color: #2196f3; font-size: 13px; font-weight: bold"
                      >Đến hạn hoàn thành</span
                    >
                  </div>
                </div>
              </div>
            </div>
          </div>
          <div
            v-else
            class="align-items-center justify-content-center p-4 text-center m-auto"
            style="display: flex; flex-direction: column"
          >
            <img
              src="../../../../assets/background/nodata.png"
              height="144"
            />
            <h3 class="m-1">Không có dữ liệu</h3>
          </div>
        </div>

        <div class="p-1"></div>
        <div
          class="col-12 flex bg-white"
          style="min-height: calc(67vh - 15rem)"
        >
          <div class="col-6">
            <Chart
              type="pie"
              :data="chartData1"
              :options="optionsChart1"
            />
          </div>
          <div class="col-6">
            <Chart
              type="pie"
              :data="chartData2"
              :options="optionsChart2"
            />
          </div>
        </div>
      </div>
    </div>
  </div>
  <Sidebar
    v-model:visible="showDetail"
    :position="PositionSideBar"
    :style="{
      width:
        PositionSideBar == 'right'
          ? width1 > 1800
            ? ' 65vw'
            : '75vw'
          : '100vw',
      'min-height': '100vh !important',
    }"
    :showCloseIcon="false"
  >
    <DetailedWork
      :isShow="showDetail"
      :id="selectedTaskID"
      :turn="0"
    >
    </DetailedWork>
  </Sidebar>
</template>

<style lang="scss" scoped>
.format-center {
  justify-content: center;
  align-items: center;
  vertical-align: middle;
  text-align: center;
}

.format-right {
  justify-content: flex-end;
  align-items: flex-end;
  vertical-align: middle;
  text-align: right;
}

.format-left {
  justify-content: flex-start;
  align-items: center;
  vertical-align: middle;
  text-align: left;
}

#flip-animation {
  animation-name: spinner;
  animation-timing-function: linear;
  animation-iteration-count: infinite;
  animation-duration: 2s;
  transform-style: preserve-3d;
}
@-webkit-keyframes spinner {
  from {
    -webkit-transform: rotateY(0deg);
  }

  to {
    -webkit-transform: rotateY(-360deg);
  }
}

@keyframes spinner {
  from {
    -moz-transform: rotateY(0deg);
    -ms-transform: rotateY(0deg);
    transform: rotateY(0deg);
  }

  to {
    -moz-transform: rotateY(-360deg);
    -ms-transform: rotateY(-360deg);
    transform: rotateY(-360deg);
  }
}
.hyphens {
  hyphens: auto;
}
::v-deep(.p-avatar) {
  &.myclass {
    .p-avatar-text {
      font-size: 2rem !important;
    }
  }
}
.task-hover:hover {
  background-color: #e5f3ff;
}
</style>
