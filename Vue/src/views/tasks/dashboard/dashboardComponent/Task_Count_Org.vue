<script setup>
import { ref, inject, onMounted, watch, h } from "vue";
import { useToast } from "vue-toastification";
import { encr } from "../../../../util/function.js";
import moment from "moment";
import DetailedWork from "../../../../components/task_origin/DetailedWork.vue";
import TaskChart from "./Chart/TaskChart.vue";
import TaskdeptChild from "./TaskdeptChild.vue";
import FilterTask from "./filterTask.vue";
import { FilterMatchMode, FilterOperator } from "primevue/api";
const cryoptojs = inject("cryptojs");
const emitter = inject("emitter");
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
const filters1 = ref({});
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
const options = ref({
  PageSize: 20,
  PageNo: 0,
  loading: true,
  totalRecords: 0,
  SearchText: "",
});
const props = defineProps({
  type: Boolean,
});
let user = store.state.user;
const first = ref(0);
const selectedKeys = ref();
const datalists = ref([]);
const renderTree = (data, id, name, title) => {
  let arrChils = [];
  let arrtreeChils = [];

  if (props.type != 1) {
    data
      .filter((x) => x.parent_id == null)
      .forEach((m, i) => {
        let om = { key: m[id], data: m };

        const rechildren = (mm, pid) => {
          let dts = data.filter((x) => x.parent_id == pid);
          if (dts.length > 0) {
            if (!mm.children) mm.children = [];
            dts.forEach((em) => {
              let om1 = { key: em[id], data: em };
              rechildren(om1, em[id]);
              mm.children.push(om1);
            });
          }
        };
        rechildren(om, m[id]);
        arrChils.push(om);
        //
      });
  } else {
    let parent_id = [];
    data.forEach((z) => {
      parent_id.push(z.parent_id);
    });

    data
      .filter((x) => x.parent_id == null)
      .forEach((m, i) => {
        const rechildren = (mm, pid) => {
          let dts = data.filter((x) => x.parent_id == pid);
          if (dts.length > 0) {
            if (!mm.children) mm.children = [];
            dts.forEach((em) => {
              rechildren(em, em[id]);
              mm.children.push(em);
            });
          }
        };
        rechildren(m, m[id]);
        arrChils.push(m);
      });
    arrChils.forEach((x) => {
      let z = x;
      if (z.children.length > 0)
        z.children.forEach((y) => {
          arrtreeChils.push(y);
        });
    });
  }
  return { arrChils: arrChils, arrtreeChils: arrtreeChils };
};
const Chartdata = ref();
const loadData = () => {
  options.value.loading = true;
  axios
    .post(
      // eslint-disable-next-line no-undef
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "task_dashboar_count_by_org_bh",
            par: [
              { par: "user_id", va: user.user_id },
              { par: "type", va: props.type },
              { par: "project_id", va: options.value.project_id },
              { par: "group_id", va: options.value.group_id },
              { par: "fromDate", va: options.value.start_date },
              { par: "toDate", va: options.value.end_date },
              { par: "search", va: options.value.SearchText },
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
      datalists.value = [];
      let data = JSON.parse(response.data.data)[0];
      let count = JSON.parse(response.data.data)[1];
      options.value.total = count[0].total;
      options.value.progress = count[0].progress;
      Chartdata.value = data;
      if (props.type == 1) {
        data.forEach((x) => {
          x.children = [];
          x.progress = x.dept_progress != null ? x.dept_progress : 100;
          x.task_info = JSON.parse(x.task_info);
          if (x.task_info != null)
            x.task_info.forEach((t) => {
              t.progress = t.progress != null ? t.progress : 0;
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
              t.p_id = t.p_id != null ? t.p_id : -1;
              t.project_name =
                t.p_id == -1 ? "Công việc không thuộc dự án" : t.project_name;
            });
        });
        if (
          options.value.SearchText != null &&
          options.value.SearchText != ""
        ) {
          datalists.value = data;
        } else {
          let obj = renderTree(data, "organization_id", "", "");
          datalists.value = obj.arrtreeChils;
        }
      } else {
        data.forEach((x) => {
          x.progress = x.org_progress != null ? x.org_progress : 100;
        });
        let obj = renderTree(data, "organization_id", "", "");
        datalists.value = obj.arrChils;
      }
      options.value.totalRecords = count[0].totalRecords;
      options.value.loading = false;
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
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const expandedKeys = ref();
const refresh = () => {
  removeFilter;
  first.value = 0;
  styleObj.value = "";
  options.value = {
    PageSize: 20,
    PageNo: 0,
    loading: true,
    totalRecords: 0,
    filterDateType: null,
    project_id: null,
    group_id: null,
    start_date: null,
    end_date: null,
    searchText: null,
  };
  options.value.loading = true;
  options.value.SearchText = "";
  loadData();
};

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
    model.value.total.data.labels.push(x.organization_name);
    model.value.total.data.datasets[0].data.push(x.total);
    model.value.total.data.datasets[0].backgroundColor.push(getRandomColor());
    model.value.total.data.datasets[0].hoverBackgroundColor.push(
      getRandomColor(),
    );
    //doing
    model.value.doing.data.labels.push(x.organization_name);
    model.value.doing.data.datasets[0].data.push(x.doing);
    model.value.doing.data.datasets[0].backgroundColor.push(getRandomColor());
    model.value.doing.data.datasets[0].hoverBackgroundColor.push(
      getRandomColor(),
    );
    //finished
    model.value.finished.data.labels.push(x.organization_name);
    model.value.finished.data.datasets[0].data.push(x.finished);
    model.value.finished.data.datasets[0].backgroundColor.push(
      getRandomColor(),
    );
    model.value.finished.data.datasets[0].hoverBackgroundColor.push(
      getRandomColor(),
    );
    //exp
    model.value.expired.data.labels.push(x.organization_name);
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
const size = ref(75);
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
const closeDetail = () => {
  showDetail.value = false;
  selectedTaskID.value = null;
  loadData(true);
};
watch(showDetail, () => {
  if (showDetail.value == false) {
    loadData(true);
  }
});

const op = ref();
const toggle = (event) => {
  op.value.toggle(event);
};
const filterChange = () => {
  styleObj.value = style.value;
};
const removeFilter = () => {
  styleObj.value = {};
};
const styleObj = ref();
const style = ref({
  "background-color": "#2196F3 !important",
  color: "#fff !important",
  " border": "1px solid #5ca7e3 !important",
});
onMounted(() => {
  loadData();
});
</script>

<template>
  <div class="div-main">
    <TreeTable
      v-if="props.type == 0"
      ref="dt"
      :loading="options.loading"
      :rowHovers="true"
      :showGridlines="true"
      responsiveLayout="scroll"
      @page="onPage($event)"
      v-model:first="first"
      :expandedKeys="expandedKeys"
      :value="datalists"
      :paginator="true"
      :rows="options.PageSize"
      paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
      :rowsPerPageOptions="[20, 30, 50, 100, 200]"
      :scrollable="true"
      scrollHeight="flex"
      :totalRecords="options.totalRecords"
      dataKey="organization_id"
      v-model:selectionKeys="selectedKeys"
      :filters="filters1"
      filterMode="lenient"
    >
      <template #header>
        <h3>
          {{
            props.type == 1
              ? "Thống kê số lượng công việc theo phòng ban"
              : "Thống kê số lượng công việc theo đơn vị"
          }}
        </h3>
        <Toolbar class="w-full custoolbar">
          <template #start>
            <span class="p-input-icon-left">
              <i class="pi pi-search" />
              <InputText
                v-model="filters1['global']"
                type="text"
                spellcheck="false"
                placeholder="Tìm kiếm"
              />
            </span>
          </template>

          <template #end>
            <Button
              v-if="checkDelList"
              @click="deleteList()"
              label="Xóa"
              icon="pi pi-trash"
              class="mr-2 p-button-danger"
            />

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
      </template>
      <Column
        :header="props.type == 1 ? 'Tên phòng ban' : 'Tên đơn vị'"
        field="organization_name"
        headerClass="align-items-center justify-content-center text-center "
        :expander="true"
      ></Column>
      <Column
        class="align-items-center justify-content-center text-center max-w-8rem"
        header="Tiến độ"
        field=""
      >
        <template #body="data">
          <div class="align-items-center justify-content-center text-center">
            <Knob
              class="w-full"
              v-model="data.node.data.progress"
              :readonly="true"
              valueTemplate="{value}%"
              :valueColor="
                data.node.data.progress < 33
                  ? '#FF0000'
                  : data.node.data.progress < 66
                  ? '#2196f3'
                  : '#6dd230'
              "
              :textColor="
                data.node.data.progress < 33
                  ? '#FF0000'
                  : data.node.data.progress < 66
                  ? '#2196f3'
                  : '#6dd230'
              "
              :size="size"
            />
          </div>
        </template>
      </Column>
      <Column
        class="align-items-center justify-content-center text-center max-w-8rem"
        header="Tất cả"
        field="total"
      >
        <template #body="data">
          <Button
            class="font-bold p-button-text border-1 border-round-xl border-gray-200 w-full justify-content-center"
            style="background-color: #bbbbbb; color: white"
          >
            <span>
              <span class="font-bold"> {{ data.node.data.total }}</span>
              <br />
              <span class="font-bold">Công việc</span>
            </span>
          </Button>
        </template>
      </Column>
      <Column
        class="align-items-center justify-content-center text-center max-w-8rem"
        header="Đang làm"
        field="doing"
      >
        <template #body="data">
          <Button
            class="font-bold p-button-text border-1 border-round-xl border-gray-200 w-full justify-content-center"
            style="background-color: #2196f3; color: white"
          >
            <span>
              <span class="font-bold"> {{ data.node.data.doing }}</span>
              <br />
              <span class="font-bold">Công việc</span>
            </span>
          </Button>
        </template>
      </Column>
      <Column
        header="Hoàn thành"
        class="align-items-center justify-content-center text-center max-w-8rem"
        field="finished"
      >
        <template #body="data">
          <Button
            class="font-bold p-button-text border-1 border-round-xl border-gray-200 w-full justify-content-center"
            style="background-color: #6fbf73; color: white"
          >
            <span>
              <span class="font-bold"> {{ data.node.data.finished }}</span>
              <br />
              <span class="font-bold">Công việc</span>
            </span>
          </Button>
        </template>
      </Column>

      <Column
        header="Quá hạn"
        field="expired"
        class="align-items-center justify-content-center text-center max-w-8rem"
      >
        <template #body="data">
          <Button
            class="font-bold p-button-text border-1 border-round-xl border-gray-200 w-full justify-content-center"
            style="background-color: #f00000; color: white"
          >
            <span>
              <span class="font-bold"> {{ data.node.data.expired }}</span>
              <br />
              <span class="font-bold">Công việc</span>
            </span>
          </Button>
        </template>
      </Column>
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
    </TreeTable>

    <DataTable
      v-else
      :value="datalists"
      responsiveLayout="scroll"
      :scrollable="true"
      scrollHeight="flex"
      :loading="options.loading"
      dataKey="organization_id"
      :rowHover="true"
      :showGridlines="true"
      v-model:expandedRows="expandedRows"
    >
      <template #header>
        <h3>
          {{
            props.type == 1
              ? "Thống kê số lượng công việc theo phòng ban"
              : "Thống kê số lượng công việc theo đơn vị"
          }}
          <span
            v-if="options.total"
            style="color: #2196f3"
            >( Tổng số công việc: {{ options.total }} - Tiến độ:
            {{ options.progress }}% )</span
          >
        </h3>
        <Toolbar class="w-full custoolbar">
          <template #start>
            <span class="p-input-icon-left">
              <i class="pi pi-search" />
              <InputText
                v-model="options.SearchText"
                type="text"
                spellcheck="false"
                placeholder="Tìm kiếm"
                @keyup.enter="loadData()"
              />
            </span>
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
              style="z-index: 1000"
            >
              <FilterTask
                class="w-full"
                :func="loadData"
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
      </template>
      <Column
        :expander="true"
        class="max-w-3rem"
      />
      <Column
        header="Tên phòng ban"
        field="organization_name"
        headerClass="justify-content-center"
      ></Column>
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
        field=""
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
        <span
          class="w-full"
          v-if="
            (slotProps.data.children != null &&
              slotProps.data.children != []) ||
            (slotProps.data.task_info != null && slotProps.data.task_info != [])
          "
        >
          <div class="w-full">
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
              <template #header>
                <h3>
                  Công việc của phòng ban:
                  <span class="pl-2">{{
                    slotProps.data.organization_name
                  }}</span>
                </h3>
              </template>
              <template #groupheader="slotProps2">
                <span class="image-text">
                  {{
                    slotProps2.data.project_name ??
                    "Công việc không thuộc dự án"
                  }}
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
          <div class="w-full">
            <TaskdeptChild
              :data="slotProps.data"
              :option="options"
            ></TaskdeptChild>
          </div>
        </span>
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
  <Dialog
    :visible="OpenDialog"
    :header="'Biểu đồ so sánh xử lý công việc'"
    :style="{ width: '75vw' }"
    :closable="false"
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
  <DetailedWork
    v-if="showDetail === true"
    :id="selectedTaskID"
    :turn="0"
    :closeDetail="closeDetail"
  >
  </DetailedWork>
</template>

<style lang="scss" scoped>
.div-main {
  height: calc(100vh - 7.5rem);
}
.task-hover:hover {
  background-color: #f5f5f5;
  color: #2196f3;
}
</style>
