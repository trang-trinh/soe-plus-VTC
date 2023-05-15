<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { required, maxLength, minLength, email } from "@vuelidate/validators";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
import { encr } from "../../../../util/function.js";
import moment from "moment";
import ChartDataLabels from "chartjs-plugin-datalabels";


const cryoptojs = inject("cryptojs");
const emitter = inject("emitter");

//color
const bgColor = ref([
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
  "#B0DE09",
]);
const colors = ref([
  "#CB4335",
  "#FF6600",
  "#FF9E01",
  "#FCD202",
  "#B0DE09",
  "#04D215",
  "#0D8ECF",
  "#0D52D1",
  "#2A0CD0",
  "#8A0CCF",
  "#CD0D74",
  "#754DEB",
  "#DDDDDD",
  "#999999",
  "#999999",
  "#999999",
  "#999999",
  "#999999",
  "#999999",
  "#999999",
  "#999999",
  "#999999",
  "#999999",
  "#999999",
  "#999999",
  "#999999",
  "#999999",
]);

const plugins = [ChartDataLabels];

//Khai báo biến
const types = ref([
  { type: 1, icon: "pi pi-chart-bar", title: "Biểu đồ cột" },
  { type: 2, icon: "pi pi-align-left", title: "Biểu đồ thanh ngang" },
  { type: 3, icon: "pi pi-chart-pie", title: "Biểu đồ tròn" },
  // { type: 4, icon: "pi pi-table", title: "Biểu đồ bảng" },
]);
const store = inject("store");
const datalists = ref();
const router = inject("router");
const options = ref({
  view:1,
  loading: true,
  department_id: store.getters.user.organization_id,
});
const genders = ref(
  [
    { text: "Nam", value: 1 },
    { text: "Nữ", value: 2 }
  ]
)
const isFirst = ref(true);
const toast = useToast();
const swal = inject("$swal");
const axios = inject("axios"); // inject axios
const basedomainURL = fileURL;
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const chartDatapie = ref();
const academics = ref({
  labels: [],
  datasets: [
    {
      data: [],
      backgroundColor: [],
      hoverBackgroundColor: [],
    },
  ],
});
const data_lines = ref([
  { labels: [], datasets: [{ data: [], backgroundColor: [], hoverBackgroundColor: [], },], text:"Biểu đồ nhân sự theo giới tính" },
  { labels: [], datasets: [{ data: [], backgroundColor: [], hoverBackgroundColor: [], },], text:"Biểu đồ nhân sự theo loại nhân sự" },
  { labels: [], datasets: [{ data: [], backgroundColor: [], hoverBackgroundColor: [], },], text:"Biểu đồ nhân sự theo chức vụ"},
  { labels: [], datasets: [{ data: [], backgroundColor: [], hoverBackgroundColor: [], },], text:"Biểu đồ nhân sự tham gia Đảng"},
]);
const loadData = () => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_report_chart",
            par: [
              { par: "department_id", va: options.value.department_id },
              { par: "status", va: options.value.gender },
              { par: "type", va: options.value.title_id },
            ],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        // data[0].forEach((item) => {
        //   if (item.total) {
        //     item.total_name = item.total.toLocaleString("vi-vN", {
        //       style: "decimal",
        //       minimumFractionDigits: 0,
        //       maximumFractionDigits: 20,
        //     });
        //   }
        // })
        renderTotalName(data[0]);
        renderTotalName(data[1]);
        renderTotalName(data[2]);
        renderTotalName(data[3]);
        renderAcademic(data_lines.value[0], data[0], 0);
        renderAcademic(data_lines.value[1], data[1], 1);
        renderAcademic(data_lines.value[2], data[2], 2);
        renderAcademic(data_lines.value[3], data[3], 3);
      }
      else renderAcademic(data_lines.value, []);
      swal.close();
      options.value.loading = false;
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;
    });
};
const renderAcademic = (chart, data, type) => {
  var temp = data;
  chart.datasets = [];
  var labels = [];
  var arr = [];
  if (temp != null && temp.length > 0) {
    if (type == 0)
      labels = temp.map((item) => item["gender_name"] + " ")
    else if (type == 1)
      labels = temp.map((item) => item["personel_groups_name"] + " ")
    else if (type == 2)
      labels = temp.map((item) => item["position_name"] + " ")
    else 
      labels = temp.map((item) => item["partisan"] + " ")

    arr = temp.map((item) => item["total"]);
  }
  setTimeout(() => {
    //lightOptions.value.plugins.legend.display = true;
    chart.datasets.push({
      label: "",
      data: [],
      backgroundColor: [],
      hoverBackgroundColor: [],
    });
    chart.labels = labels;
    chart.datasets[0].data = arr;
    chart.datasets[0].backgroundColor = colors.value;
    chart.datasets[0].hoverBackgroundColor = colors.value;
  }, 100);
};
const basicOptions = ref({
  plugins: {
    legend: {
      display: false,
      labels: {
        color: "#495057",
      },
    },
    datalabels: {
      formatter: (val) =>
        val.toLocaleString("vi-vN", {
          style: "decimal",
          minimumFractionDigits: 0,
          maximumFractionDigits: 20,
        }),
      anchor: "center",
      align: "end",
      color: "black",
      labels: {
        title: {
          font: {
            //weight: "bold",
            //size: 48,
          },
        },
        value: {
          color: "black",
          font: {
            //weight: "bold",
            //size: 48,
          },
        },
      },
    },
  },
  scales: {
    x: {
      ticks: {
        color: "#495057",
      },
      grid: {
        color: "#ebedef",
      },
    },
    y: {
      ticks: {
        color: "#495057",
      },
      grid: {
        color: "#ebedef",
      },
    },
  },
});
const lightOptions = ref({
  plugins: {
    legend: {
      display: true,
      labels: {
        color: "#495057",
      },
    },
    datalabels: {
      formatter: (val) =>
        val.toLocaleString("vi-vN", {
          style: "decimal",
          minimumFractionDigits: 0,
          maximumFractionDigits: 20,
        }) + " %",
      anchor: "center",
      align: "center",
      color: "white",
      labels: {
        title: {
          font: {
            //weight: "bold",
            //size: 48,
          },
        },
        value: {
          color: "white",
          font: {
            //weight: "bold",
            //size: 48,
          },
        },
      },
    },
  },
});
const horizontalOptions = ref({
  indexAxis: "y",
  plugins: {
    legend: {
      display: false,
      labels: {
        color: "#495057",
      },
    },
    datalabels: {
      formatter: (val) => val,
      anchor: "end",
      align: "end",
      color: "black",
      labels: {
        title: {
          font: {
            //weight: "bold",
            //size: 48,
          },
        },
        value: {
          color: "black",
          font: {
            //weight: "bold",
            //size: 48,
          },
        },
      },
    },
  },
  scales: {
    x: {
      ticks: {
        color: "#495057",
      },
      grid: {
        color: "#ebedef",
      },
    },
    y: {
      ticks: {
        color: "#495057",
      },
      grid: {
        color: "#ebedef",
      },
    },
  },
});
const goBack = () => {
  history.back();
};
//Khai báo function
const renderTotalName = (data) => {
  data.forEach((item) => {
    if (item.total) {
      item.total_name = item.total.toLocaleString("vi-vN", {
        style: "decimal",
        minimumFractionDigits: 0,
        maximumFractionDigits: 20,
      });
    }
  })
}
onMounted(() => {
  //init
  loadData();
  // initTudien();
});
</script>

<template>
  <div class="main-layout true flex-grow-1 p-2 pb-0 pr-0">
    <div style="background-color: #fff; padding: 1rem;padding-left: 0;">
      <div style="height: 36px; display: flex; align-items: center;">
        <Button
            label="Quay lại"
            icon="pi pi-arrow-left"
            class="p-button-outlined mr-2 p-button-secondary"
            @click="goBack()"
            />
          <SelectButton
            v-model="options.view"
            :options="types"
            optionValue="type"
            optionLabel="type"
            dataKey="type"
            aria-labelledby="custom"
          >
            <template #option="slotProps">
              <i
                v-if="slotProps.option.icon != ''"
                :class="slotProps.option.icon"
                class="mr-2"
              ></i
              ><span>{{ slotProps.option.title }}</span>
            </template>
          </SelectButton>
        </div>
      <div v-if="options.view == 1" class="w-full h-full flex" style="flex-flow:wrap">
        <div class="col-6 md:col-6" v-for="(item, index) in data_lines" :key="index">
          <div class="card m-1">
            <div class="card-header" :style="{ cursor: 'pointer', padding: '4px 4px 4px 1rem' }">
              <div style="text-align: center;" :style="{ fontSize: '15px', fontWeight: 'bold'}">{{ item.text }}</div>
            </div>
            <Chart id="chart32" type="bar" :data="item" :options="basicOptions" :plugins="plugins" class="w-full"
              :style="{
                width: '80% !important',
                display: 'flex',
                alignItems: 'center',
              }" />
          </div>
        </div>
      </div>
      <div v-if="options.view == 2" class="w-full h-full flex" style="flex-flow:wrap">
        <div class="col-6 md:col-6" v-for="(item, index) in data_lines" :key="index">
          <div class="card m-1">
            <div class="card-header" :style="{ cursor: 'pointer', padding: '4px 4px 4px 1rem' }">
              <div style="text-align: center;" :style="{ fontSize: '15px', fontWeight: 'bold'}">{{ item.text }}</div>
            </div>
            <Chart id="chart32" type="bar" :data="item" :options="horizontalOptions" :plugins="plugins" class="w-full"
              :style="{
                width: '80% !important',
                display: 'flex',
                alignItems: 'center',
              }" />
          </div>
        </div>
      </div>
      <div v-if="options.view == 3" class="w-full h-full flex" style="flex-flow:wrap">
        <div class="col-6 md:col-6" v-for="(item, index) in data_lines" :key="index">
          <div class="card m-1">
            <div class="card-header" :style="{ cursor: 'pointer', padding: '4px 4px 4px 1rem' }">
              <div style="text-align: center;" :style="{ fontSize: '15px', fontWeight: 'bold'}">{{ item.text }}</div>
            </div>
            <div class="format-center w-full">
              <Chart id="chart32" type="doughnut" :data="item" :options="lightOptions" :plugins="plugins"
              :style="{
                width: '40% !important',
                display: 'flex',
                alignItems: 'center',
              }" />
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
<style lang="scss" scoped></style>
