<script setup>
import { inject, onMounted, ref } from "vue";
import { useToast } from "vue-toastification";
import Steps from "primevue/steps";
import moment from "moment";
import VueApexCharts from "vue3-apexcharts";
import { forEach } from "jszip";
const axios = inject("axios"); // inject axios
const config = ref({
  working_days : [],
  list_price: [],
  price: null,
});
const store = inject("store");
const swal = inject("$swal");
const toast = useToast();

// import { GChart } from "vue-google-charts";
var series = ref([
  {
    name: "Giá tiền",
    data: [],
  },
]);
var chartOptions = ref({
  chart: {
    type: "line",
    height: 350,
  },
  markers: {
    size: 0,
  },
  toolbar: { show: false },
  stroke: {
    curve: "stepline",
  },
  dataLabels: {
    enabled: false,
  },
  title: {
    text: "Giá tiền (VNĐ)",
    align: "left",
  },
  grid: {
    row: {
      colors: ["#f3f3f3", "transparent"], // takes an array which will be repeated on columns
      opacity: 0.5,
    },
  },
  markers: {
    hover: {
      sizeOffset: 4,
    },
  },
  xaxis: {
    categories: [],
  },
});
const working_days = ref([]);
const initConfig = () => {
  axios
    .get(baseURL + "/api/BookingMeal/GetConfig", {
      headers: { Authorization: `Bearer ${store.getters.token}` },
    })
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        if(response.data.data) config.value = response.data.data;
        if (config.value && config.value.working_days) {
          // for (let i = 0; i < config.value.working_days.length; i++) {
          //   config.value.working_days[i] = new Date(
          //     config.value.working_days[i]
          //   );
          // }
         // config.value.working_days = config.value.working_days.slice(0, 2);
          config.value.working_days.forEach((item, index) => {
            let dt = new Date(item);
            let date = new Date(dt.getFullYear(), dt.getMonth(), dt.getDate());
            config.value.working_days[index] = date;
          });
        }
        if (config.value.list_price) {
          var prices = config.value.list_price.map((x) => x.price);
          var days = config.value.list_price.map((x) => x.day_string);
          series.value[0].data = prices;
          chartOptions.value.xaxis.categories = days;
        }
      } else {
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
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
      }
    });
};

const saveConfig = () => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  debugger
  let dt = new Date();
  var string_date = moment(dt).format("DD/MM/yyyy");
  var obj_price = {
    price: config.value.price,
    day: dt,
    day_string: string_date,
  };
  if (config.value.list_price.length > 0) {
    config.value.list_price.map((x) => x.day_string).includes(string_date);
    var index = config.value.list_price
      .map((x) => x.day_string)
      .indexOf(string_date);
    if (index !== -1) {
      config.value.list_price[index] = obj_price;
    } else {
      config.value.list_price.push(obj_price);
    }
  }
  else if(!isEmpty(config.value.price)){
          config.value.list_price.push(obj_price);
  }
  // var arr = [];
  // for (let y = 2023; y < 2030; y++) {
  //   for (let m = 0; m < 12; m++) {
  //     if (m == 0 || m == 2 || m == 4 || m == 6 || m == 7 || m == 9 || m == 11) {
  //       for (let d = 1; d < 32; d++) {
  //         let date1 = new Date(y, m, d);
  //         if (date1.getDay() == 0 || date1.getDay() == 6) arr.push(date1);
  //       }
  //     } else if (m == 1) {
  //       if (y == 2024) {
  //         for (let d = 1; d < 30; d++) {
  //           let date2 = new Date(y, m, d);
  //           if (date2.getDay() == 0 || date2.getDay() == 6) arr.push(date2);
  //         }
  //       } else {
  //         for (let d = 1; d < 29; d++) {
  //           let date2 = new Date(y, m, d);
  //           if (date2.getDay() == 0 || date2.getDay() == 6) arr.push(date2);
  //         }
  //       }
  //     } else {
  //       for (let d = 1; d < 31; d++) {
  //         let date3 = new Date(y, m, d);
  //         if (date3.getDay() == 0 || date3.getDay() == 6) arr.push(date3);
  //       }
  //     }
  //   }
  // }
  //config.value.working_days = arr;
  axios
    .post(baseURL + "/api/BookingMeal/SetConfig", config.value, {
      headers: { Authorization: `Bearer ${store.getters.token}` },
    })
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        initConfig();
        toast.success("Cập nhật thiết lập thành công!");
      } else {
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
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
      }
    });
};
//check empy object
function isEmpty(val) {
  return val === undefined || val == null || val.length <= 0 ? true : false;
}
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
  return {
    saveConfig,
  };
});
</script>
<template>
  <div
    class="main-layout p-4"
    v-if="store.getters.islogin"
    style="max-height: calc(100vh - 50px); overflow: scroll"
  >
    <Card class="px-4 py-3">
      <template #header>
        <h3><i class="pi pi-cog"></i> Thiết lập</h3>
      </template>
      <template #content>
        <form @submit.prevent="saveConfig">
          <div class="grid formgrid m-2">
            <div class="field col-12 md:col-12">
              <label class="col-4 text-left" style="vertical-align: text-bottom"
                >Chọn ngày nghỉ
              </label>
              <Calendar
                class="col-8"
                inputId="disableddays"
                selectionMode="multiple"
                :manualInput="false"
                :showIcon="true"
                autocomplete="on"
                v-model="config.working_days"
              >
              </Calendar>
            </div>
            <div class="field col-12 md:col-12">
              <label class="col-4 text-left" style="vertical-align: middle"
                >Giá tiền (1 suất/ VNĐ)</label
              >
              <InputNumber class="col-8 ip36 p-0" v-model="config.price" />
            </div>
          </div>
        </form>
      </template>
      <template #footer>
        <div class="text-center">
          <Button icon="pi pi-save" label="Cập nhật" @click="saveConfig" />
        </div>
      </template>
    </Card>
    <Card class="px-4 py-2">
      <template #header>
        <h3><i class="pi pi-chart-line"></i> Lịch sử thay đổi giá tiền</h3>
      </template>
      <template #content>
        <div>
          <div
            id="chart"
            v-if="series.length > 0 && chartOptions.xaxis.categories.length > 0"
          >
            <VueApexCharts
              type="line"
              height="300"
              :options="chartOptions"
              :series="series"
            ></VueApexCharts>
          </div>
        </div>
      </template>
    </Card>
  </div>
</template>
<style scoped>
.p-calendar-w-btn {
  padding: 0px !important;
}
</style>
