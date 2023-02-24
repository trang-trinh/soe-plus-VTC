<script setup>
import { ref, inject, onMounted } from "vue";
import { useToast } from "vue-toastification";
import { encr } from "../../../../../util/function.js";
import moment from "moment";
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
const width1 = window.screen.width;
const addLog = (log) => {
  // eslint-disable-next-line no-undef
  axios.post(baseURL + "/api/Proc/AddLog", log, config);
};
const props = defineProps({
  data: Array,
  buttons: Array,
});
const listBtn = ref([]);
const ChangeViews = (num) => {
  model.value = {
    data: {},
    options: {},
  };
  let data = JSON.parse(JSON.stringify(props.data));
  let tempData = Object.values(data);
  model.value.data = tempData[num].data;
  model.value.options = tempData[num].options;
  listBtn.value.forEach((x, i) => {
    if (x.value == num) {
      x.is_active = true;
    } else {
      x.is_active = false;
    }
  });
};
const model = ref({
  data: {},
  options: {},
});
const listChart = ref([
  { name: "Tròn", value: 0 },
  { name: "Cột", value: 1 },
  { name: "Cột ngang", value: 2 },
]);
function getRandomColor() {
  var letters = "0123456789ABCDEF";
  var color = "#";
  for (var i = 0; i < 6; i++) {
    color += letters[Math.floor(Math.random() * 16)];
  }
  return color;
}
const drdModel = ref();
const chartMode = ref();
const drdModelChange = (e) => {
  if (e.value == 1 || e.value == 2) {
    chartMode.value = "bar";
    let data = JSON.parse(JSON.stringify(props.data));
    let tempData = Object.values(data);
    model.value.data = {
      labels: [],
      datasets: [],
    };
    listBtn.value.forEach((x, i) => {
      model.value.data.labels = tempData[0].data.labels;
      model.value.data.datasets.push({
        label: x.label,
        backgroundColor: getRandomColor(),
        hoverBackgroundColor: getRandomColor(),
        data: tempData[i].data.datasets[0].data,
      });
    });
  } else {
    let data = JSON.parse(JSON.stringify(props.data));
    let tempData = Object.values(data);
    chartMode.value = "pie";
    model.value.data = tempData[0].data;
    model.value.options = tempData[0].options;
  }
  if (e.value == 2) {
    model.value.options = {
      indexAxis: "y",
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
    };
  }
};
onMounted(() => {
  let data = JSON.parse(JSON.stringify(props.buttons));
  data.forEach((x) => {
    listBtn.value.push(x);
  });
  drdModel.value = 0;
  chartMode.value = "pie";
  ChangeViews(0);
});
</script>
<template>
  <div class="main-layout true w-full h-full">
    <div class="col-12 flex justify-content-center align-items-center p-0">
      <div class="flex col-4 justify-content-center"></div>
      <div class="flex col-6 justify-content-center">
        <div v-if="chartMode != 'bar'">
          <Button
            v-for="(item, index) in listBtn"
            :key="index"
            :label="item.label"
            class="mx-1 py-2"
            :class="item.is_active == true ? 'active' : ''"
            @click="ChangeViews(item.value)"
          >
          </Button>
        </div>
      </div>

      <div class="col-4 flex">
        <div class="col-3 align-self-center">Loại biểu đồ</div>
        <Dropdown
          v-model="drdModel"
          :options="listChart"
          optionLabel="name"
          optionValue="value"
          placeholder="Loại biểu đồ"
          @change="drdModelChange($event)"
          class="col-5 p-0"
        />
      </div>
    </div>
    <div class="col-12 flex justify-content-center align-items-center p-0">
      <div :class="chartMode == 'pie' ? 'col-6' : 'col-12'">
        <Chart
          :type="chartMode"
          :data="model.data"
          :options="model.options"
        />
      </div>
    </div>
  </div>
</template>
<style lang="scss" scoped>
.active {
  background-color: #f18636;
}
</style>
