<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { encr } from "../../../../util/function.js";
import moment from "moment";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import DetailedWork from "../../../../components/task_origin/DetailedWork.vue";
import TaskChart from "./Chart/TaskChart.vue";
import FilterTask from "./filterTask.vue";
const cryoptojs = inject("cryptojs");
const emitter = inject("emitter");

const filters1 = ref({
  global: { value: null, matchMode: FilterMatchMode.CONTAINS },
  full_name: { value: null, matchMode: FilterMatchMode.CONTAINS },
  user_id: { value: [], matchMode: FilterMatchMode.IN },
});
const filters2 = ref({
  global: { value: null, matchMode: FilterMatchMode.CONTAINS },
});
//khai báo
const axios = inject("axios");
const store = inject("store");
const toast = useToast();
const swal = inject("$swal");
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
const bgColor = ref(["#F4B2A3"]);
let user = store.state.user;
const datalists = ref([]);
const options = ref({
  loading: true,
});
const listStatus = ref([
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
const listUser = ref([]);
const Chartdata = ref();
const loadData = (rf) => {
  if (rf) {
    options.value.loading = true;
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
  }
  axios
    .post(
      // eslint-disable-next-line no-undef
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "task_dashboard_by_member",
            par: [
              { par: "user_id", va: user.user_id },
              { par: "project_id", va: options.value.project_id },
              { par: "group_id", va: options.value.group_id },
              { par: "fromDate", va: options.value.start_date },
              { par: "toDate", va: options.value.end_date },
              { par: "search", va: options.value.searchText },
              { par: "filterDateType", va: options.value.filterDateType },
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
      let user = JSON.parse(response.data.data)[1];
      user.forEach((x) => {
        let kk = {};
        kk.user_key = x.user_key;
        kk.user_id = x.user_id;
        kk.full_name = x.full_name;
        kk.avatar = x.avatar;
        kk.position_name = x.position_name;
        kk.department_name = x.organization_name;
        listUser.value.push(kk);
      });
      data.forEach((x) => {
        x.task_info = JSON.parse(x.task_info);
        x.progress =
          x.total > 0 ? Math.floor((x.finished / x.total) * 100) : 100;
        if (x.task_info != null)
          x.task_info.forEach((t) => {
            let sttus = listStatus.value.filter((a) => a.value == t.status);
            t.status_display = {
              text: sttus[0].text,
              bg_color: sttus[0].bg_color,
              text_color: sttus[0].text_color,
            };
            t.creator = t.creator[0];
            t.creator_tooltip =
              "Người giao việc <br/>" +
              t.creator.full_name +
              "<br/>" +
              t.creator.position_name +
              "<br/>" +
              (t.creator.department_name != null
                ? t.creator.department_name
                : t.creator.organiztion_name);
            t.project_name =
              t.p_id == -1 ? "Công việc không thuộc dự án" : t.project_name;
          });
      });
      Chartdata.value = data;
      datalists.value = [];
      if (options.value.display_type != 1) {
        let data2 = JSON.parse(JSON.stringify(data));
        data = data2.filter((x) => x.task_info != null && x.task_info != []);
        let listprojectname = [];
        let listproject = [];
        data2.forEach((z) => {
          if (
            z.task_info != null &&
            z.task_info != [] &&
            z.task_info.length > 0
          ) {
            z.task_info.forEach((y) => {
              if (listprojectname.includes(y.project_name) == false) {
                listprojectname.push(y.project_name);
                let kk = {
                  project_id: y.p_id,
                  project_name: y.project_name,
                };
                if (kk.project_id != -1) listproject.push(kk);
              }
            });
          }
        });
        listproject.forEach((x) => {
          x.data = [];
          data2.forEach((z) => {
            if (z.task_info != null && z.task_info != []) {
              z.task_info.forEach((k) => {
                if (k.p_id == x.project_id) {
                  let kk = JSON.parse(JSON.stringify(k));

                  kk.user_key = z.user_key;
                  kk.user_id = z.user_id;
                  kk.full_name = z.full_name;
                  kk.avatar = z.avatar;
                  kk.position_name = z.position_name;
                  kk.department_name = z.department_name;
                  kk.organization_name = z.organization_name;
                  kk.woker_tooltip =
                    "Người thực hiện <br/>" +
                    kk.full_name +
                    "<br/>" +
                    kk.position_name +
                    "<br/>" +
                    (kk.department_name != null
                      ? kk.department_name
                      : kk.organiztion_name);
                  x.data.push(kk);
                }
              });
            }
          });
        });
        datalists.value = listproject;
      } else datalists.value = data;
      options.value.loading = false;
      swal.close();
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!" + error);
      addLog({
        title: "Lỗi Console loadData",
        controller: "TaskReview(Dashboard).vue",
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

const expandedRows = ref([]);
const expandedRowGroups = ref([]);
const count1 = (data, name) => {
  let total = 0;
  if (data.task_info != null)
    for (let v of data.task_info)
      if (v.project_name == name) {
        total += 1;
      }
  return total;
};
const showDetail = ref(false);
const selectedTaskID = ref();
const onNodeSelect = (id) => {
  showDetail.value = false;
  showDetail.value = true;
  selectedTaskID.value = id.task_id;
};
emitter.on("SideBar", (obj) => {
  showDetail.value = obj;
  loadData(true);
});
watch(showDetail, () => {
  if (showDetail.value == false) {
    loadData(true);
  }
});
const PositionSideBar = ref("right");
emitter.on("psb", (obj) => {
  PositionSideBar.value = obj;
});

const OpenDialog = ref(false);
const menuButs = ref();
const itemButs = ref([
  {
    label: "Biểu đồ so sánh",
    icon: "pi pi-file-excel",
    command: (event) => {
      openChartVue("ExportExcel");
    },
  },
]);
const model = ref({
  total: {
    data: {
      labels: [],
      datasets: [{ data: [], backgroundColor: [], hoverBackgroundColor: [] }],
    },
    options: {
      responsive: true,
      plugins: {
        title: {
          display: true,
          position: "bottom",
          text: "Tất cả",
        },
        legend: {
          position: "bottom",
        },
      },
    },
  },
  doing: {
    data: {
      labels: [],
      datasets: [{ data: [], backgroundColor: [], hoverBackgroundColor: [] }],
    },
    options: {
      responsive: true,
      plugins: {
        title: {
          display: true,
          position: "bottom",
          text: "Đang làm",
        },
        legend: {
          position: "bottom",
        },
      },
    },
  },
  finished: {
    data: {
      labels: [],
      datasets: [{ data: [], backgroundColor: [], hoverBackgroundColor: [] }],
    },
    options: {
      responsive: true,
      plugins: {
        title: {
          display: true,
          position: "bottom",
          text: "Hoàn thành",
        },
        legend: {
          position: "bottom",
        },
      },
    },
  },
  expired: {
    data: {
      labels: [],
      datasets: [{ data: [], backgroundColor: [], hoverBackgroundColor: [] }],
    },
    options: {
      responsive: true,
      plugins: {
        title: {
          display: true,
          position: "bottom",
          text: "Quá hạn",
        },
        legend: {
          position: "bottom",
        },
      },
    },
  },
});

function getRandomColor() {
  var letters = "0123456789ABCDEF";
  var color = "#";
  for (var i = 0; i < 6; i++) {
    color += letters[Math.floor(Math.random() * 16)];
  }
  return color;
}
const toggleExport = (event) => {
  menuButs.value.toggle(event);
};
const openChartVue = () => {
  OpenDialog.value = true;
  model.value = {
    total: {
      data: {
        labels: [],
        datasets: [{ data: [], backgroundColor: [], hoverBackgroundColor: [] }],
      },
      options: {
        responsive: true,
        plugins: {
          title: {
            display: true,
            position: "bottom",
            text: "Tất cả",
          },
          legend: {
            position: "bottom",
          },
        },
      },
    },
    doing: {
      data: {
        labels: [],
        datasets: [{ data: [], backgroundColor: [], hoverBackgroundColor: [] }],
      },
      options: {
        responsive: true,
        plugins: {
          title: {
            display: true,
            position: "bottom",
            text: "Đang làm",
          },
          legend: {
            position: "bottom",
          },
        },
      },
    },
    finished: {
      data: {
        labels: [],
        datasets: [{ data: [], backgroundColor: [], hoverBackgroundColor: [] }],
      },
      options: {
        responsive: true,
        plugins: {
          title: {
            display: true,
            position: "bottom",
            text: "Hoàn thành",
          },
          legend: {
            position: "bottom",
          },
        },
      },
    },
    expired: {
      data: {
        labels: [],
        datasets: [{ data: [], backgroundColor: [], hoverBackgroundColor: [] }],
      },
      options: {
        responsive: true,
        plugins: {
          title: {
            display: true,
            position: "bottom",
            text: "Quá hạn",
          },
          legend: {
            position: "bottom",
          },
        },
      },
    },
  };
  let data = JSON.parse(JSON.stringify(Chartdata.value));
  data.forEach((x) => {
    //total
    model.value.total.data.labels.push(x.full_name);
    model.value.total.data.datasets[0].data.push(x.total);
    model.value.total.data.datasets[0].backgroundColor.push(getRandomColor());
    model.value.total.data.datasets[0].hoverBackgroundColor.push(
      getRandomColor(),
    );
    //doing
    model.value.doing.data.labels.push(x.full_name);
    model.value.doing.data.datasets[0].data.push(x.doing);
    model.value.doing.data.datasets[0].backgroundColor.push(getRandomColor());
    model.value.doing.data.datasets[0].hoverBackgroundColor.push(
      getRandomColor(),
    );
    //finished
    model.value.finished.data.labels.push(x.full_name);
    model.value.finished.data.datasets[0].data.push(x.finished);
    model.value.finished.data.datasets[0].backgroundColor.push(
      getRandomColor(),
    );
    model.value.finished.data.datasets[0].hoverBackgroundColor.push(
      getRandomColor(),
    );
    //exp
    model.value.expired.data.labels.push(x.full_name);
    model.value.expired.data.datasets[0].data.push(x.expired);
    model.value.expired.data.datasets[0].backgroundColor.push(getRandomColor());
    model.value.expired.data.datasets[0].hoverBackgroundColor.push(
      getRandomColor(),
    );
  });
};
const listButton = ref([
  { label: "Tất cả", value: 0, is_active: true },
  { label: "Đang làm", value: 1, is_active: false },
  { label: "Hoàn thành", value: 2, is_active: false },
  { label: "Quá hạn", value: 3, is_active: false },
]);
const initFilter = () => {
  filters1.value = {
    global: { value: null, matchMode: FilterMatchMode.CONTAINS },
    full_name: { value: null, matchMode: FilterMatchMode.CONTAINS },
    user_id: { value: [], matchMode: FilterMatchMode.IN },
  };
  filters2.value = {
    global: { value: null, matchMode: FilterMatchMode.CONTAINS },
  };
};
initFilter();
const refresh = () => {
  removeFilter();
  initFilter();
  styleObj.value = "";
  options.value.PageSize = 20;
  options.value.PageNo = 0;
  options.value.loading = true;
  options.value.totalRecords = 0;
  options.value.filterDateType = null;
  options.value.project_id = null;
  options.value.group_id = null;
  options.value.start_date = null;
  options.value.end_date = null;
  options.value.searchText = null;
  options.value.loading = true;
  options.value.SearchText = "";

  loadData(true);
};
const size = ref(75);
const props = defineProps({
  typeView: Intl,
});
watch(props, () => {
  if (props.typeView != null) {
    options.value.display_type = props.typeView;
    loadData(true);
  } else {
    options.value.display_type = 1;
    loadData(true);
  }
});
const op = ref();
const toggle = (event) => {
  op.value.toggle(event);
};
const styleObj = ref();
const style = ref({
  "background-color": "#2196F3 !important",
  color: "#fff !important",
  " border": "1px solid #5ca7e3 !important",
});

const filterChange = () => {
  styleObj.value = style.value;
};
const removeFilter = () => {
  styleObj.value = {};
};
const load = () => {
  loadData(true);
};
const SearchFieldChange = (event) => {};
onMounted(() => {
  if (props.typeView != null) {
    options.value.display_type = props.typeView;
  } else options.value.display_type = 1;
  loadData(true);
});
</script>

<template>
  <div class="main-layout main-h true flex-grow-1 pb-3">
    <Toolbar class="p-4 bg-white w-full custoolbar">
      <template #start>
        <div class="flex col-12">
          <div class="col py-0 px-1">
            <span class="p-input-icon-left">
              <i class="pi pi-search" />
              <InputText
                v-if="options.display_type == 1"
                v-model="filters1['global'].value"
                type="text"
                spellcheck="false"
                placeholder="Tìm kiếm"
                @input="SearchFieldChange($event)"
              />
              <InputText
                v-else
                v-model="filters2['global'].value"
                type="text"
                spellcheck="false"
                placeholder="Tìm kiếm"
                @input="SearchFieldChange($event)"
              />
            </span>
          </div>

          <div v-if="options.display_type == 1">
            <MultiSelect
              :filter="true"
              v-model="filters1['user_id'].value"
              :options="listUser"
              optionValue="user_id"
              optionLabel="full_name"
              placeholder="Chọn thành viên"
              display="chip"
              :filterFields="[
                'user_id',
                'full_name',
                'department_name',
                'position_name',
              ]"
              style="width: 40vh"
            >
              <template #option="slotProps">
                <div
                  class="country-item flex"
                  style="align-items: center; margin-left: 10px"
                >
                  <Avatar
                    @error="
                      $event.target.src =
                        basedomainURL + '/Portals/Image/nouser1.png'
                    "
                    v-bind:label="
                      slotProps.option.avatar
                        ? ''
                        : (slotProps.option.name ?? '').substring(0, 1)
                    "
                    v-bind:image="basedomainURL + slotProps.option.avatar"
                    style="
                      background-color: #2196f3;
                      color: #ffffff;
                      width: 32px;
                      height: 32px;
                      font-size: 15px !important;
                      margin-left: -10px;
                    "
                    :style="{
                      background: bgColor[slotProps.index % 7] + '!important',
                    }"
                    class="cursor-pointer"
                    size="xlarge"
                    shape="circle"
                  />
                  <div
                    class="pt-1"
                    style="padding-left: 10px"
                  >
                    <b>{{ slotProps.option.full_name }}</b>
                    <br />
                    {{ slotProps.option.position_name }}
                    <br />
                    {{ slotProps.option.department_name }}
                  </div>
                </div>
              </template>
            </MultiSelect>
          </div>
        </div>
      </template>

      <template #end>
        <Button
          class="mx-2 p-button-outlined p-button-secondary"
          label=""
          icon="pi pi-filter"
          v-tooltip="'Lọc'"
          type="button"
          @click="toggle"
          aria:haspopup="true"
          aria-controls="overlay_panel"
          :style="[styleObj]"
        ></Button>
        <OverlayPanel
          ref="op"
          appendTo="body"
          class="p-0 m-0"
          :showCloseIcon="false"
          id="overlay_panel"
          style="width: 45vw; z-index: 1000"
        >
          <FilterTask
            class="w-full"
            :func="load"
            :data="options"
            :refs="refresh"
            :filterChange="filterChange"
          >
          </FilterTask>
        </OverlayPanel>
        <Button
          @click="refresh()"
          class="mr-2 p-button-outlined p-button-secondary"
          icon="pi pi-refresh"
          v-tooltip="'Tải lại'"
        />
        <Button
          label="Tiện ích"
          icon="pi pi-file-excel"
          class="mr-2 p-button-outlined p-button-secondary"
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
    <DataTable
      v-if="options.display_type == 1 && options.loading == false"
      :value="datalists"
      responsiveLayout="scroll"
      :scrollable="true"
      scrollHeight="flex"
      :loading="options.loading"
      dataKey="user_key"
      :rowHover="true"
      :showGridlines="true"
      v-model:filters="filters1"
      v-model:expandedRows="expandedRows"
      :globalFilterFields="['full_name', 'user_id']"
    >
      <Column
        :expander="true"
        class="max-w-3rem"
      />
      <Column
        header="Họ tên"
        field="full_name"
        headerClass="justify-content-center"
        filterField="full_name"
        :showFilterMatchModes="false"
        :filterMenuStyle="{ width: '14rem' }"
      >
        <template #body="data">
          <div class="col-12 flex p-0">
            <div class="col p-0 flex justify-content-center align-items-center">
              <Avatar
                @error="
                  $event.target.src =
                    basedomainURL + '/Portals/Image/nouser1.png'
                "
                v-bind:label="
                  data.data.avatar
                    ? ''
                    : data.data.full_name.split(' ').at(-1).substring(0, 1)
                "
                v-bind:image="basedomainURL + data.data.avatar"
                style="color: #ffffff; cursor: pointer"
                :style="{
                  background: bgColor[0],
                  border: '1px solid' + bgColor[0],
                }"
                class=""
                size="large"
                shape="circle"
              />
            </div>
            <div class="col-11 p-0">
              <div class="font-bold text-blue-400 text-lg">
                {{ data.data.full_name }}
              </div>
              <div class="font-normal">
                {{ data.data.position_name }}
              </div>
              <div class="font-normal">
                {{ data.data.department_name ?? data.data.organization_name }}
              </div>
            </div>
          </div>
        </template>
      </Column>

      <Column
        header="Tiến độ"
        field=""
        class="align-items-center justify-content-center max-w-8rem"
      >
        <template #body="data">
          <div class="align-items-center justify-content-center text-center">
            <Knob
              class="w-full"
              v-model="data.data.progress"
              :readonly="true"
              valueTemplate="{value}%"
              :valueColor="
                data.data.progress < 33
                  ? '#FF0000'
                  : data.data.progress < 66
                  ? '#2196f3'
                  : '#6dd230'
              "
              :textColor="
                data.data.progress < 33
                  ? '#FF0000'
                  : data.data.progress < 66
                  ? '#2196f3'
                  : '#6dd230'
              "
              :size="size"
            />
          </div>
        </template>
      </Column>
      <Column
        header="Thông tin chung"
        field="full_name"
        class="align-items-center justify-content-center max-w-30rem"
      >
        <template #body="data">
          <Button
            class="font-bold p-button-text border-1 border-round-xl border-gray-200 w-auto justify-content-center"
            style="background-color: #bbbbbb; color: white"
          >
            <span>
              <span class="font-bold"> {{ data.data.total }}</span>
              <br />
              <span class="font-bold">Tất cả</span>
            </span>
          </Button>
          <Button
            class="font-bold p-button-text border-1 border-round-xl border-gray-200 w-auto justify-content-center"
            style="background-color: #2196f3; color: white"
          >
            <span>
              <span class="font-bold"> {{ data.data.doing }}</span>
              <br />
              <span class="font-bold">Đang làm</span>
            </span>
          </Button>
          <Button
            class="font-bold p-button-text border-1 border-round-xl border-gray-200 w-auto justify-content-center"
            style="background-color: #6fbf73; color: white"
          >
            <span>
              <span class="font-bold"> {{ data.data.finished }}</span>
              <br />
              <span class="font-bold">Hoàn thành</span>
            </span>
          </Button>
          <Button
            class="font-bold p-button-text border-1 border-round-xl border-gray-200 w-auto justify-content-center"
            style="background-color: #f00000; color: white"
          >
            <span>
              <span class="font-bold"> {{ data.data.expired }}</span>
              <br />
              <span class="font-bold">Quá hạn</span>
            </span>
          </Button>
        </template>
      </Column>
      <template #expansion="slotProps">
        <div
          v-if="slotProps.data.task_info != null"
          class="w-full"
        >
          <DataTable
            :value="slotProps.data.task_info"
            responsiveLayout="scroll"
            rowGroupMode="subheader"
            groupRowsBy="p_id"
            :scrollable="true"
            scrollHeight="60vh"
            :expandableRowGroups="true"
            v-model:expandedRowGroups="expandedRowGroups"
          >
            <template #groupheader="slotProps2">
              <span class="image-text">
                {{ slotProps2.data.project_name }}
                ({{ count1(slotProps.data, slotProps2.data.project_name) }})
              </span>
            </template>
            <Column
              field="task_name"
              header="Tên công việc"
              headerClass=" align-items-center justify-content-center"
              bodyClass="align-items-center"
            >
              <template #body="data">
                <div
                  style="display: flex; flex-direction: column; padding: 5px"
                  @click="onNodeSelect(data.data)"
                  class="task-hover w-full"
                >
                  <div style="line-height: ; display: flex">
                    <span
                      v-tooltip="'Ưu tiên'"
                      v-if="data.data.is_prioritize"
                      style="margin-right: 5px"
                      ><i
                        style="color: orange"
                        class="pi pi-star-fill"
                      ></i>
                    </span>
                    <span
                      style="
                        font-weight: bold;
                        font-size: 14px;
                        overflow: hidden;
                        text-overflow: ellipsis;
                        width: 100%;
                        display: -webkit-box;
                        -webkit-line-clamp: 2;
                        -webkit-box-orient: vertical;
                      "
                      >{{ data.data.task_name }}</span
                    >
                  </div>
                  <div
                    style="
                      font-size: 12px;
                      margin-top: 5px;
                      display: flex;
                      align-items: center;
                    "
                  >
                    <span
                      v-if="data.data.start_date || data.data.end_date"
                      style="color: #98a9bc"
                    >
                      {{
                        data.data.start_date
                          ? moment(new Date(data.data.start_date)).format(
                              "DD/MM/YYYY",
                            )
                          : null
                      }}
                      -
                      {{
                        data.data.end_date
                          ? moment(new Date(data.data.end_date)).format(
                              "DD/MM/YYYY",
                            )
                          : null
                      }}
                    </span>
                  </div>
                  <div
                    v-if="data.data.p_id != -1"
                    style="
                      min-height: 25px;
                      display: flex;
                      align-items: center;
                      margin-top: 10px;
                    "
                  >
                    <i class="pi pi-tag"></i>
                    <span
                      class="duan"
                      style="
                        font-size: 13px;
                        font-weight: 400;
                        margin-left: 5px;
                        color: #0078d4;
                      "
                      >{{ data.data.project_name }}</span
                    >
                  </div>
                </div>
              </template>
            </Column>
            <Column
              field="progress"
              header="Tiến độ"
              class="align-items-center justify-content-center text-center max-w-8rem"
            >
              <template #body="data">
                <Knob
                  class="w-full"
                  v-model="data.data.progress"
                  :readonly="true"
                  valueTemplate="{value}%"
                  :valueColor="
                    data.data.progress < 33
                      ? '#FF0000'
                      : data.data.progress < 66
                      ? '#2196f3'
                      : '#6dd230'
                  "
                  :textColor="
                    data.data.progress < 33
                      ? '#FF0000'
                      : data.data.progress < 66
                      ? '#2196f3'
                      : '#6dd230'
                  "
                  :size="size"
                />
              </template>
            </Column>
            <Column
              header="Người giao việc"
              class="align-items-center justify-content-center text-center max-w-10rem"
            >
              <template #body="data">
                <Avatar
                  @error="
                    $event.target.src =
                      basedomainURL + '/Portals/Image/nouser1.png'
                  "
                  v-tooltip.bottom="{
                    value: data.data.creator_tooltip,
                    escape: true,
                  }"
                  v-bind:label="
                    data.data.creator.avatar
                      ? ''
                      : data.data.creator.full_name
                          .split(' ')
                          .at(-1)
                          .substring(0, 1)
                  "
                  v-bind:image="basedomainURL + data.data.creator.avatar"
                  style="color: #ffffff; cursor: pointer"
                  :style="{
                    background: bgColor[1 % 7],
                    border: '1px solid' + bgColor[1 % 7],
                  }"
                  class="p-0 myclass"
                  size="large"
                  shape="circle"
                />
              </template>
            </Column>
            <Column
              header="Hạn xử lý"
              field=""
              class="align-items-center justify-content-center text-center max-w-10rem"
            >
              <template #body="data">
                <div
                  v-if="data.data.is_deadline == true"
                  style="
                    background-color: #fff2d7;
                    padding: 10px 10px;
                    border-radius: 5px;
                  "
                  class="w-full font-bold text-blue-500"
                >
                  {{ moment(data.data.end_date).format("DD/MM/YYYY") }}
                </div>
              </template>
            </Column>
            <Column
              header="Thời gian xử lý"
              field=""
              class="align-items-center justify-content-center text-center max-w-10rem"
            >
              <template #body="data">
                <div
                  v-if="data.data.is_deadline == true"
                  style="
                    background-color: #fff8ee;
                    padding: 10px 10px;
                    border-radius: 5px;
                  "
                  class="w-full"
                >
                  <span
                    v-if="data.data.exp_time > 0"
                    style="color: #f00000; font-size: 13px; font-weight: bold"
                    >Quá hạn {{ data.data.exp_time }} ngày</span
                  >
                  <span
                    v-else-if="data.data.exp_time < 0"
                    style="color: #04d214; font-size: 13px; font-weight: bold"
                    >Còn {{ Math.abs(data.data.exp_time) }} ngày</span
                  >
                  <span
                    v-else
                    style="color: #2196f3; font-size: 13px; font-weight: bold"
                    >Đến hạn hoàn thành</span
                  >
                </div>
              </template>
            </Column>
            <Column
              header="Trạng thái"
              field=""
              class="align-items-center justify-content-center text-center max-w-10rem"
            >
              <template #body="data">
                <Chip
                  class="px-3 py-1"
                  :style="{
                    background: data.data.status_display.bg_color,
                    color: data.data.status_display.text_color,
                  }"
                  v-bind:label="data.data.status_display.text"
                />
              </template>
            </Column>
          </DataTable>
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
      </template>
      <template #empty>
        <div
          class="align-items-center justify-content-center p-4 text-center m-auto"
          style="display: flex; flex-direction: column"
        >
          <img
            src="../../../../assets/background/nodata.png"
            height="144"
          />
          <h3 class="m-1">Không có dữ liệu</h3>
        </div>
      </template>
    </DataTable>
    <DataTable
      v-if="options.loading == false && options.display_type == 2"
      :value="datalists"
      responsiveLayout="scroll"
      :scrollable="true"
      scrollHeight="flex"
      :loading="options.loading"
      dataKey="user_key"
      :rowHover="true"
      :showGridlines="true"
      v-model:filters="filters2"
      v-model:expandedRows="expandedRows"
      :globalFilterFields="['project_name']"
    >
      <Column
        :expander="true"
        class="max-w-3rem"
      />
      <Column
        header="Tên dự án"
        field="project_name"
        headerClass="justify-content-center"
      >
        <template #body="data">
          {{ data.data.project_name }} ({{ data.data.data.length }})
        </template>
      </Column>

      <template #expansion="slotProps">
        <div
          v-if="slotProps.data.data != null"
          class="w-full"
        >
          <DataTable :value="slotProps.data.data">
            <Column
              field="task_name"
              header="Tên công việc"
              headerClass=" align-items-center justify-content-center"
              bodyClass="align-items-center"
            >
              <template #body="data">
                <div
                  style="display: flex; flex-direction: column; padding: 5px"
                  @click="onNodeSelect(data.data)"
                  class="task-hover w-full"
                >
                  <div style="line-height: ; display: flex">
                    <span
                      v-tooltip="'Ưu tiên'"
                      v-if="data.data.is_prioritize"
                      style="margin-right: 5px"
                      ><i
                        style="color: orange"
                        class="pi pi-star-fill"
                      ></i>
                    </span>
                    <span
                      style="
                        font-weight: bold;
                        font-size: 14px;
                        overflow: hidden;
                        text-overflow: ellipsis;
                        width: 100%;
                        display: -webkit-box;
                        -webkit-line-clamp: 2;
                        -webkit-box-orient: vertical;
                      "
                      >{{ data.data.task_name }}</span
                    >
                  </div>
                  <div
                    style="
                      font-size: 12px;
                      margin-top: 5px;
                      display: flex;
                      align-items: center;
                    "
                  >
                    <span
                      v-if="data.data.start_date || data.data.end_date"
                      style="color: #98a9bc"
                    >
                      {{
                        data.data.start_date
                          ? moment(new Date(data.data.start_date)).format(
                              "DD/MM/YYYY",
                            )
                          : null
                      }}
                      -
                      {{
                        data.data.end_date
                          ? moment(new Date(data.data.end_date)).format(
                              "DD/MM/YYYY",
                            )
                          : null
                      }}
                    </span>
                  </div>
                  <div
                    v-if="data.data.p_id != -1"
                    style="
                      min-height: 25px;
                      display: flex;
                      align-items: center;
                      margin-top: 10px;
                    "
                  >
                    <i class="pi pi-tag"></i>
                    <span
                      class="duan"
                      style="
                        font-size: 13px;
                        font-weight: 400;
                        margin-left: 5px;
                        color: #0078d4;
                      "
                      >{{ data.data.project_name }}</span
                    >
                  </div>
                </div>
              </template>
            </Column>
            <Column
              field="progress"
              header="Tiến độ"
              class="align-items-center justify-content-center text-center max-w-8rem"
            >
              <template #body="data">
                <Knob
                  class="w-full"
                  v-model="data.data.progress"
                  :readonly="true"
                  valueTemplate="{value}%"
                  :valueColor="
                    data.data.progress < 33
                      ? '#FF0000'
                      : data.data.progress < 66
                      ? '#2196f3'
                      : '#6dd230'
                  "
                  :textColor="
                    data.data.progress < 33
                      ? '#FF0000'
                      : data.data.progress < 66
                      ? '#2196f3'
                      : '#6dd230'
                  "
                  :size="size"
                />
              </template>
            </Column>
            <Column
              header="Người thực hiện"
              class="align-items-center justify-content-center text-center max-w-10rem"
            >
              <template #body="data">
                <Avatar
                  @error="
                    $event.target.src =
                      basedomainURL + '/Portals/Image/nouser1.png'
                  "
                  v-tooltip.bottom="{
                    value: data.data.woker_tooltip,
                    escape: true,
                  }"
                  v-bind:label="
                    data.data.avatar
                      ? ''
                      : data.data.full_name.split(' ').at(-1).substring(0, 1)
                  "
                  v-bind:image="basedomainURL + data.data.avatar"
                  style="color: #ffffff; cursor: pointer"
                  :style="{
                    background: bgColor[1 % 7],
                    border: '1px solid' + bgColor[1 % 7],
                  }"
                  class="p-0 myclass"
                  size="large"
                  shape="circle"
                />
              </template>
            </Column>
            <Column
              header="Hạn xử lý"
              field=""
              class="align-items-center justify-content-center text-center max-w-10rem"
            >
              <template #body="data">
                <div
                  v-if="data.data.is_deadline == true"
                  style="
                    background-color: #fff2d7;
                    padding: 10px 10px;
                    border-radius: 5px;
                  "
                  class="w-full font-bold text-blue-500"
                >
                  {{ moment(data.data.end_date).format("DD/MM/YYYY HH:mm") }}
                </div>
              </template>
            </Column>
            <Column
              header="Thời gian xử lý"
              field=""
              class="align-items-center justify-content-center text-center max-w-10rem"
            >
              <template #body="data">
                <div
                  v-if="data.data.is_deadline == true"
                  style="
                    background-color: #fff8ee;
                    padding: 10px 10px;
                    border-radius: 5px;
                  "
                  class="w-full"
                >
                  <span
                    v-if="data.data.exp_time > 0"
                    style="color: #f00000; font-size: 13px; font-weight: bold"
                    >Quá hạn {{ data.data.exp_time }} ngày</span
                  >
                  <span
                    v-else-if="data.data.exp_time < 0"
                    style="color: #04d214; font-size: 13px; font-weight: bold"
                    >Còn {{ Math.abs(data.data.exp_time) }} ngày</span
                  >
                  <span
                    v-else
                    style="color: #2196f3; font-size: 13px; font-weight: bold"
                    >Đến hạn hoàn thành</span
                  >
                </div>
              </template>
            </Column>
            <Column
              header="Trạng thái"
              field=""
              class="align-items-center justify-content-center text-center max-w-10rem"
            >
              <template #body="data">
                <Chip
                  class="px-3 py-1"
                  :style="{
                    background: data.data.status_display.bg_color,
                    color: data.data.status_display.text_color,
                  }"
                  v-bind:label="data.data.status_display.text"
                />
              </template>
            </Column>
          </DataTable>
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
      </template>
      <template #empty>
        <div
          class="align-items-center justify-content-center p-4 text-center m-auto"
          style="display: flex; flex-direction: column"
        >
          <img
            src="../../../../assets/background/nodata.png"
            height="144"
          />
          <h3 class="m-1">Không có dữ liệu</h3>
        </div>
      </template>
    </DataTable>
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
    </DetailedWork
  ></Sidebar>
  <Dialog
    :visible="OpenDialog"
    :header="'Biểu đồ so sánh xử lý công việc'"
    :style="{ width: '100vw' }"
    :closable="false"
    maximizable="true"
    contentClass="main-layout true "
  >
    <TaskChart
      :data="model"
      :buttons="listButton"
    >
    </TaskChart>
    <template #footer>
      <div class="col-12">
        <Button
          label="Đóng"
          class=" "
          @click="OpenDialog = false"
        ></Button>
      </div>
    </template>
  </Dialog>
</template>

<style lang="scss" scoped>
.main-h {
  height: calc(100vh - 7rem);
}

.task-hover:hover {
  background-color: #f5f5f5;
  color: #2196f3;
}
</style>
