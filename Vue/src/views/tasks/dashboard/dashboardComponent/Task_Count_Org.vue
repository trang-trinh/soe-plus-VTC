<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { encr } from "../../../../util/function.js";
import moment from "moment";
import DetailedWork from "../../../../components/task_origin/DetailedWork.vue";
import TaskChart from "./Chart/TaskChart.vue";
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
  let vl = data.filter((x) => x.parent_id == null);

  if (vl.length > 0) {
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
    data.forEach((m, i) => {
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
            proc: "task_dashboar_count_by_org",
            par: [
              { par: "user_id", va: user.user_id },
              { par: "type", va: props.type },
              { par: "search", va: options.value.SearchText },
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
      data.forEach((x) => {
        x.progress =
          x.total > 0 ? Math.floor((x.finished / x.total) * 100) : 100;
      });
      Chartdata.value = data;
      let obj = renderTree(data, "organization_id", "", "");
      datalists.value = obj.arrChils;
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
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const expandedKeys = ref();
const refresh = () => {
  first.value = 0;
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
onMounted(() => {
  loadData();
});
</script>

<template>
  <div class="div-main">
    <TreeTable
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
</template>

<style lang="scss" scoped>
.div-main {
  height: calc(100vh - 7.5rem);
}
</style>
