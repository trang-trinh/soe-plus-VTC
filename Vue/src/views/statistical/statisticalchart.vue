<script setup>
import { ref, inject, onMounted } from "vue";
import { encr } from "../../util/function";
import { useRoute } from "vue-router";
import moment from "moment";
import ChartDataLabels from "chartjs-plugin-datalabels";

const router = inject("router");
const route = useRoute();
const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios");
const cryoptojs = inject("cryptojs");
const config = {
  headers: {
    Authorization: `Bearer ${store.getters.token}`,
  },
};
const plugins = [ChartDataLabels];

//Declare
const options = ref({
  start_date: new Date(),
  end_date: new Date(),
  sort: 0,
  loading: true,
  statistical_id: null,
  loai: null,
  view: 2,
  type: 1,
  today: new Date(),
  week: 0,
  month: new Date().getMonth() + 1,
  tempyear: new Date(),
  year: new Date().getFullYear(),
});
const types = ref([
  { type: 1, icon: "pi pi-chart-bar", title: "Biểu đồ cột đứng" },
  { type: 2, icon: "pi pi-align-left", title: "Biểu đồ cột ngang" },
  { type: 3, icon: "pi pi-chart-pie", title: "Biểu đồ tròn" },
  { type: 4, icon: "pi pi-table", title: "Biểu đồ bảng" },
]);
const views = ref([
  { view: 0, icon: "", title: "Xem theo ngày" },
  { view: 1, icon: "", title: "Xem theo tuần" },
  { view: 2, icon: "", title: "Xem theo tháng" },
  { view: 3, icon: "", title: "Xem theo năm" },
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
const orders = ref([
  { id: 0, icon: "pi pi-times-circle", title: "không sắp xếp" },
  { id: 1, icon: "pi pi-sort-numeric-down", title: "Số thứ tự bé tới lớn" },
  {
    id: 2,
    icon: "pi pi-sort-numeric-down-alt",
    title: "Số thứ tự lớn tới bé",
  },
  {
    id: 3,
    icon: "pi pi-sort-numeric-down-alt",
    title: "Số lượng từ bé tới lớn",
  },
  {
    id: 4,
    icon: "pi pi-sort-numeric-down-alt",
    title: "Số lượng lớn tới bé",
  },
]);
const currentweek = ref({});
const weeks = ref([]);
const months = ref([
  { month: 1 },
  { month: 2 },
  { month: 3 },
  { month: 4 },
  { month: 5 },
  { month: 6 },
  { month: 7 },
  { month: 8 },
  { month: 9 },
  { month: 10 },
  { month: 11 },
  { month: 12 },
]);
const years = ref([]);
const modelstatistical = ref({});
const temps = ref([]);
const datas = ref({
  labels: [],
  datasets: [
    {
      data: [],
      backgroundColor: [],
      hoverBackgroundColor: [],
    },
  ],
});
const tables = ref([]);
const lightOptions = ref({
  plugins: {
    legend: {
      display: true,
      labels: {
        color: "#495057",
      },
    },
    datalabels: {
      formatter: (val) => val,
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
const basicOptions = ref({
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

//function
const goYear = (year) => {
  options.value.year = year.getFullYear();
  if (options.value.view !== 3) {
    initDictionary();
  } else {
    changeView(options.value.view);
  }
};
const goMonth = (month) => {
  options.value.month = month;
  changeView(options.value.view);
};
const goWeek = (week) => {
  options.value.week = week;
  changeView(options.value.view);
};
const changeType = (type) => {
  options.value.type = type;
};
const changeView = (view) => {
  options.value.view = view;
  if (options.value.view != null) {
    switch (options.value.view) {
      case 1:
        if (weeks.value != null && weeks.value.length > 0) {
          currentweek.value = weeks.value.find(
            (x) => x["week_no"] === options.value["week"]
          );
          options.value["week_start_date"] = new Date(
            currentweek.value["week_start_date"]
          );
          options.value["week_end_date"] = new Date(
            currentweek.value["week_end_date"]
          );
        }
        initData(true);
        break;
      case 2:
        options.value["week_start_date"] = new Date(
          options.value.year,
          options.value.month - 1,
          1
        );
        options.value["week_end_date"] = new Date(
          options.value.year,
          options.value.month,
          0
        );
        initData(true);
        break;
      case 3:
        options.value["week_start_date"] = new Date(
          options.value["year"],
          0,
          1
        );
        options.value["week_end_date"] = new Date(
          options.value["year"],
          11,
          31
        );
        initData(true);
        break;
      default:
        break;
    }
  }
};
const goBack = () => {
  router.back();
};
const orderby = (value) => {
  var data = [...temps.value];
  tables.value = [...temps.value];
  switch (value) {
    case 1:
      tables.value = tables.value.sort(function (a, b) {
        return new Date(a["stt"]) - new Date(b["stt"]);
      });
      data = data.sort(function (a, b) {
        return new Date(a["stt"]) - new Date(b["stt"]);
      });
      break;
    case 2:
      data = data.sort(function (a, b) {
        return new Date(b["stt"]) - new Date(a["stt"]);
      });
      tables.value = tables.value.sort(function (a, b) {
        return new Date(b["stt"]) - new Date(a["stt"]);
      });
      break;
    case 3:
      data = data.sort(function (a, b) {
        return new Date(a["soluong"]) - new Date(b["soluong"]);
      });
      tables.value = tables.value.sort(function (a, b) {
        return new Date(a["soluong"]) - new Date(b["soluong"]);
      });
      break;
    case 4:
      data = data.sort(function (a, b) {
        return new Date(b["soluong"]) - new Date(a["soluong"]);
      });
      tables.value = tables.value.sort(function (a, b) {
        return new Date(b["soluong"]) - new Date(a["soluong"]);
      });
      break;
    default:
      break;
  }
  setTimeout(() => {
    renderData(data);
  }, 100);
};
const changeStartDate = () => {
  if (options.value.week_start_date > options.value.week_end_date) {
    options.value.week_end_date = options.value.week_start_date;
    swal.fire({
      title: "Thông báo!",
      text: "Ngày bắt đầu không được nhỏ hơn ngày kết thúc!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  initData(true);
};
const changeEndDate = () => {
  if (options.value.week_start_date > options.value.week_end_date) {
    options.value.week_end_date = options.value.week_start_date;
    swal.fire({
      title: "Thông báo!",
      text: "Ngày bắt đầu không được nhỏ hơn ngày kết thúc!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  initData(true);
};

//init
const renderData = (data) => {
  var temp = data;
  datas.value.datasets = [];
  if (
    options.value.loai === 1 ||
    options.value.loai === 4 ||
    options.value.loai === 7
  ) {
    //var temp = [{ soluong: 100 }];
    var labels = [];
    var arr = [];
    if (temp != null && temp.length > 0) {
      labels = [" Số lượng "];
      arr = temp.map((item) => item["soluong"]);
    }
    setTimeout(() => {
      datas.value.labels = labels;
      datas.value.datasets.push({
        label: "",
        data: [],
        backgroundColor: [],
        hoverBackgroundColor: [],
      });
      datas.value.datasets[0].data = arr;
      datas.value.datasets[0].backgroundColor = ["#0D8ECF"];
      datas.value.datasets[0].hoverBackgroundColor = ["#0D8ECF"];
    }, 100);
  } else if (
    options.value.loai === 2 ||
    options.value.loai === 3 ||
    options.value.loai === 5 ||
    options.value.loai === 6 ||
    options.value.loai === 8 ||
    options.value.loai === 9 ||
    options.value.loai === 10
  ) {
    // var temp = [
    //   { stt: 0, Ma: "01", Ten: "Quân khu A", soluong: 100 },
    //   { stt: 1, Ma: "02", Ten: "Quân khu B", soluong: 200 },
    //   { stt: 2, Ma: "03", Ten: "Quân khu C", soluong: 50 },
    //   { stt: 3, Ma: "04", Ten: "Quân khu D", soluong: 300 },
    //   { stt: 5, Ma: "05", Ten: "Quân khu E", soluong: 250 },
    //   { stt: 6, Ma: "06", Ten: "Quân khu F", soluong: 120 },
    //   { stt: 7, Ma: "07", Ten: "Quân khu G", soluong: 150 },
    //   { stt: 8, Ma: "08", Ten: "Quân khu H", soluong: 220 },
    //   { stt: 9, Ma: "09", Ten: "Quân khu I", soluong: 240 },
    // ];
    var labels = [];
    var arr = [];
    if (temp != null && temp.length > 0) {
      labels = temp.map((item) => " (" + item["Ma"] + ") " + item["Ten"] + " ");
      arr = temp.map((item) => item["soluong"]);
    }
    setTimeout(() => {
      lightOptions.value.plugins.legend.display = true;
      basicOptions.value.plugins.legend.display = false;
      horizontalOptions.value.plugins.legend.display = false;
      datas.value.datasets.push({
        label: "",
        data: [],
        backgroundColor: [],
        hoverBackgroundColor: [],
      });
      datas.value.labels = labels;
      datas.value.datasets[0].data = arr;
      datas.value.datasets[0].backgroundColor = colors.value;
      datas.value.datasets[0].hoverBackgroundColor = colors.value;
    }, 100);
  } else if (options.value.loai === 11 || options.value.loai === 12) {
    // var temp = [
    //   {
    //     stt: 0,
    //     Ma: "01",
    //     Ten: "Hưu trí",
    //     soluong_TN: 100,
    //     soluong_GQ: 200,
    //     soluong_CHUA: 50,
    //   },
    //   {
    //     stt: 1,
    //     Ma: "02",
    //     Ten: "Hưu trí",
    //     soluong_TN: 200,
    //     soluong_GQ: 50,
    //     soluong_CHUA: 220,
    //   },
    //   {
    //     stt: 2,
    //     Ma: "03",
    //     Ten: "Hưu trí",
    //     soluong_TN: 50,
    //     soluong_GQ: 300,
    //     soluong_CHUA: 100,
    //   },
    //   {
    //     stt: 3,
    //     Ma: "04",
    //     Ten: "Hưu trí",
    //     soluong_TN: 300,
    //     soluong_GQ: 240,
    //     soluong_CHUA: 300,
    //   },
    //   {
    //     stt: 4,
    //     Ma: "05",
    //     Ten: "Hưu trí",
    //     soluong_TN: 250,
    //     soluong_GQ: 100,
    //     soluong_CHUA: 100,
    //   },
    //   {
    //     stt: 5,
    //     Ma: "06",
    //     Ten: "Hưu trí",
    //     soluong_TN: 120,
    //     soluong_GQ: 100,
    //     soluong_CHUA: 240,
    //   },
    //   {
    //     stt: 6,
    //     Ma: "07",
    //     Ten: "Hưu trí",
    //     soluong_TN: 150,
    //     soluong_GQ: 250,
    //     soluong_CHUA: 150,
    //   },
    //   {
    //     stt: 7,
    //     Ma: "08",
    //     Ten: "Hưu trí",
    //     soluong_TN: 220,
    //     soluong_GQ: 220,
    //     soluong_CHUA: 100,
    //   },
    //   {
    //     stt: 8,
    //     Ma: "09",
    //     Ten: "Hưu trí",
    //     soluong_TN: 240,
    //     soluong_GQ: 100,
    //     soluong_CHUA: 220,
    //   },
    //   {
    //     stt: 9,
    //     Ma: "10",
    //     Ten: "Hưu trí",
    //     soluong_TN: 120,
    //     soluong_GQ: 120,
    //     soluong_CHUA: 250,
    //   },
    // ];
    var labels = [];
    var arr1 = [];
    var arr2 = [];
    var arr3 = [];
    if (temp != null && temp.length > 0) {
      labels = temp.map((item) => " (" + item["Ma"] + ") " + item["Ten"] + " ");
      arr1 = temp.map((item) => item["soluong_TN"]);
      arr2 = temp.map((item) => item["soluong_GQ"]);
      arr3 = temp.map((item) => item["soluong_CHUA"]);
    }
    setTimeout(() => {
      lightOptions.value.plugins.legend.display = true;
      basicOptions.value.plugins.legend.display = true;
      horizontalOptions.value.plugins.legend.display = true;
      datas.value.labels = labels;
      datas.value.datasets.push({
        label: "",
        data: [],
        backgroundColor: [],
        hoverBackgroundColor: [],
      });
      datas.value.datasets[0].label = "Tiếp nhận";
      datas.value.datasets[0].data = arr1;
      datas.value.datasets[0].backgroundColor = ["#FFA726"];
      datas.value.datasets[0].hoverBackgroundColor = colors.value;
      datas.value.datasets.push({
        label: "",
        data: [],
        backgroundColor: [],
        hoverBackgroundColor: [],
      });
      datas.value.datasets[1].label = "Giải quyết";
      datas.value.datasets[1].data = arr2;
      datas.value.datasets[1].backgroundColor = ["#42A5F5"];
      datas.value.datasets[1].hoverBackgroundColor = colors.value;
      datas.value.datasets.push({
        label: "",
        data: [],
        backgroundColor: [],
        hoverBackgroundColor: [],
      });
      datas.value.datasets[2].label = "Chưa giải quyết";
      datas.value.datasets[2].data = arr3;
      datas.value.datasets[2].backgroundColor = ["#78909C"];
      datas.value.datasets[2].hoverBackgroundColor = colors.value;
    }, 100);
  } else if (options.value.loai === 13 || options.value.loai === 14) {
    // var temp = [
    //   {
    //     stt: 0,
    //     Ma: "100",
    //     Ten: "Thân nhân",
    //     soluong_1: 100,
    //     Ten2: "Thân nhân",
    //     soluong_2: 100,
    //   },
    //   {
    //     stt: 1,
    //     Ma: "100",
    //     Ten: "Thân nhân",
    //     soluong_1: 100,
    //     Ten2: "Thân nhân",
    //     soluong_2: 100,
    //   },
    // ];
    var labels = [];
    var arr1 = [];
    var arr2 = [];
    if (temp != null && temp.length > 0) {
      labels = temp.map(
        (item) =>
          " (" + item["Ma"] + ") " + item["Ten"] + ", " + item["Ten2"] + " "
      );
      arr1 = temp.map((item) => item["soluong_1"]);
      arr2 = temp.map((item) => item["soluong_2"]);
    }
    setTimeout(() => {
      lightOptions.value.plugins.legend.display = true;
      basicOptions.value.plugins.legend.display = true;
      horizontalOptions.value.plugins.legend.display = true;
      datas.value.datasets.push({
        label: "",
        data: [],
        backgroundColor: [],
        hoverBackgroundColor: [],
      });
      datas.value.labels = labels;
      datas.value.datasets[0].label = "Quân nhân";
      datas.value.datasets[0].data = arr1;
      datas.value.datasets[0].backgroundColor = ["#FFA726"];
      datas.value.datasets[0].hoverBackgroundColor = colors.value;
      datas.value.datasets.push({
        label: "",
        data: [],
        backgroundColor: [],
        hoverBackgroundColor: [],
      });
      datas.value.datasets[1].label = "Khác quân nhân";
      datas.value.datasets[1].data = arr2;
      datas.value.datasets[1].backgroundColor = ["#42A5F5"];
      datas.value.datasets[1].hoverBackgroundColor = colors.value;
    }, 100);
  } else if (options.value.loai === 15) {
    // var temp = [
    //   {
    //     stt: 0,
    //     Ma: "01",
    //     Ten: "Bệnh viện A",
    //     soluong_1: 100,
    //     soluong_2: 100,
    //   },
    //   {
    //     stt: 1,
    //     Ma: "02",
    //     Ten: "Bệnh viện B",
    //     soluong_1: 100,
    //     soluong_2: 100,
    //   },
    // ];
    var labels = [];
    var arr1 = [];
    var arr2 = [];
    if (temp != null && temp.length > 0) {
      labels = temp.map((item) => " (" + item["Ma"] + ") " + item["Ten"] + " ");
      arr1 = temp.map((item) => item["soluong_1"]);
      arr2 = temp.map((item) => item["soluong_2"]);
    }
    setTimeout(() => {
      lightOptions.value.plugins.legend.display = true;
      basicOptions.value.plugins.legend.display = true;
      horizontalOptions.value.plugins.legend.display = true;
      datas.value.labels = labels;
      datas.value.datasets.push({
        label: "",
        data: [],
        backgroundColor: [],
        hoverBackgroundColor: [],
      });
      datas.value.datasets[0].label = "Quân nhân";
      datas.value.datasets[0].data = arr1;
      datas.value.datasets[0].backgroundColor = ["#FFA726"];
      datas.value.datasets[0].hoverBackgroundColor = colors.value;
      datas.value.datasets.push({
        label: "",
        data: [],
        backgroundColor: [],
        hoverBackgroundColor: [],
      });
      datas.value.datasets[1].label = "Khác quân nhân";
      datas.value.datasets[1].data = arr2;
      datas.value.datasets[1].backgroundColor = ["#42A5F5"];
      datas.value.datasets[1].hoverBackgroundColor = colors.value;
    }, 100);
  }
};
const initDictionary = (id) => {
  axios
    .post(
      baseURL + "/api/statistical/get_datas",
      {
        str: encr(
          JSON.stringify({
            proc: "statistical_get_dictionary",
            par: [
              { par: "year", va: options.value.year },
              { par: "statistical_id", va: id },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      if (response != null && response.data != null) {
        var data = response.data.data;
        if (data != null) {
          let tbs = JSON.parse(data);
          if (tbs[0] != null && tbs[0].length > 0) {
            weeks.value = tbs[0];
          } else {
            weeks.value = [];
          }
          if (tbs[1] != null && tbs[1].length > 0) {
            modelstatistical.value = tbs[1][0];
            options.value.loai = modelstatistical.value.is_type;

            if (
              options.value.loai === 1 ||
              options.value.loai === 4 ||
              options.value.loai === 7
            ) {
              orders.value = orders.value.filter(
                (x) =>
                  x["id"] !== 1 &&
                  x["id"] !== 2 &&
                  x["id"] !== 3 &&
                  x["id"] !== 4
              );
            } else if (
              options.value.loai === 11 ||
              options.value.loai === 12 ||
              options.value.loai === 13 ||
              options.value.loai === 14 ||
              options.value.loai === 15
            ) {
              orders.value = orders.value.filter(
                (x) => x["id"] !== 3 && x["id"] !== 4
              );
            }
          }
          var exist =
            weeks.value.findIndex((x) => x["is_current_week"] === true) != -1;
          if (exist) {
            currentweek.value = weeks.value.find(
              (x) => x["is_current_week"] === true
            );
          } else {
            currentweek.value = weeks.value.find(
              (x) => x["week_no"] === options.value.week || 0
            );
          }
          options.value["week"] = currentweek.value["week_no"];
          switch (options.value.view) {
            case 1:
              options.value["week_start_date"] = new Date(
                currentweek.value["week_start_date"]
              );
              options.value["week_end_date"] = new Date(
                currentweek.value["week_end_date"]
              );
              break;
            case 2:
              options.value["week_start_date"] = new Date(
                options.value.year,
                options.value.month - 1,
                1
              );
              options.value["week_end_date"] = new Date(
                options.value.year,
                options.value.month,
                0
              );
              break;
            case 3:
              options.value["week_start_date"] = new Date(
                options.value["year"],
                0,
                1
              );
              options.value["week_end_date"] = new Date(
                options.value["year"],
                11,
                31
              );
              break;
            default:
              break;
          }
          months.value.forEach((m, i) => {
            m["is_current_month"] = false;
            if (m["month"] === options.value.month) {
              m["is_current_month"] = true;
            }
          });
        }
      }
    })
    .then(() => {
      initData(true);
    })
    .catch((error) => {
      swal.fire({
        title: "Thông báo!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
      return;
    });
};
const initData = (rf) => {
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
      baseURL + "/api/statistical/statistical_get",
      {
        str: encr(
          JSON.stringify({
            statistical_id: options.value.statistical_id,
            tu: options.value["week_start_date"],
            den: options.value["week_end_date"],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      if (response != null && response.data != null) {
        var data = response.data.data;
        if (data != null) {
          let tbs = JSON.parse(data);
          if (tbs[0] != null && tbs[0].length > 0) {
            tbs[0].forEach((x) => {
              if(x["soluong"] != null){
                x["soluong_name"] = x["soluong"].toLocaleString();
              }
              if(x["soluong_1"] != null){
                x["soluong_1_name"] = x["soluong_1"].toLocaleString();
              }
              if(x["soluong_2"] != null){
                x["soluong_2_name"] = x["soluong_2"].toLocaleString();
              }
              if(x["soluong_TN"] != null){
                x["soluong_TN_name"] = x["soluong_TN"].toLocaleString();
              }
              if(x["soluong_GQ"] != null){
                x["soluong_GQ_name"] = x["soluong_GQ"].toLocaleString();
              }
              if(x["soluong_CHUA"] != null){
                x["soluong_CHUA_name"] = x["soluong_CHUA"].toLocaleString();
              }
            });
            temps.value = [...tbs[0]];
            tables.value = [...tbs[0]];
            renderData(tbs[0]);
          }
        }
      }
      swal.close();
      if (options.value.loading) options.value.loading = false;
    })
    .catch((error) => {
      swal.close();
      if (options.value.loading) options.value.loading = false;
      swal.fire({
        title: "Thông báo!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
      return;
    });
};
onMounted(() => {
  if (route.params.id != null) {
    options.value.statistical_id = route.params.id;
    initDictionary(route.params.id);
  } else {
    router.back();
    return;
  }
});
</script>
<template>
  <div class="surface-100 p-3">
    <Toolbar class="outline-none surface-0 border-none">
      <template #start>
        <div style="width: calc(100vw - 765px)">
          <ul class="flex p-0 m-0" style="list-style: none">
            <li class="mr-3 f-left">
              <Button
                @click="goBack()"
                type="button"
                label="Quay lại"
                icon="pi pi-arrow-left"
                class="p-button"
                style="
                  background-color: #5bc0de !important;
                  border: 1px solid #5bc0de !important;
                "
              />
            </li>
            <li class="mr-2 f-left format-center" style="text-align: left">
              <b style="font-size: 13pt">{{
                modelstatistical.statistical_name
              }}</b>
            </li>
          </ul>
        </div>
      </template>
      <template #end>
        <div style="height: 36px; display: flex; align-items: center">
          <SelectButton
            v-model="options.type"
            :options="types"
            @change="changeType(options.type)"
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
      </template>
    </Toolbar>
    <Toolbar class="outline-none surface-0 border-none pt-0">
      <template #start>
        <div style="height: 36px; display: flex; align-items: center">
          <SelectButton
            v-model="options.view"
            :options="views"
            @change="changeView(options.view)"
            optionValue="view"
            optionLabel="view"
            dataKey="view"
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
      </template>
      <template #end>
        <div class="form-group m-0 mr-2">
          <Dropdown
            v-model="options.sort"
            :options="orders"
            :filter="false"
            :showClear="false"
            optionLabel="name"
            placeholder="Sắp xếp"
            class="ip36"
            @change="orderby(options.sort.id)"
          >
            <template #value="slotProps">
              <div v-if="slotProps.value">
                <i :class="slotProps.value.icon" class="mr-2"></i
                ><span>{{ slotProps.value.title }}</span>
              </div>
              <span v-else>
                {{ slotProps.placeholder }}
              </span>
            </template>
            <template #option="slotProps">
              <div>
                <i :class="slotProps.option.icon" class="mr-2"></i
                ><span>{{ slotProps.option.title }}</span>
              </div>
            </template>
          </Dropdown>
        </div>
        <div class="form-group m-0 mr-2" v-if="options.view !== 0">
          <Calendar
            v-model="options.tempyear"
            @date-select="goYear(options.tempyear)"
            :showIcon="true"
            inputId="yearpicker"
            view="year"
            dateFormat="yy"
            placeholder="Chọn năm"
            class="ip36"
            style="min-width: 170px"
          />
        </div>
        <div class="form-group m-0" v-if="options.view === 2">
          <Dropdown
            :options="months"
            :filter="true"
            :showClear="false"
            v-model="options.month"
            @change="goMonth(options.month)"
            optionLabel="month"
            optionValue="month"
            placeholder="Chọn tháng"
            class="ip36"
            style="min-width: 170px"
          >
            <template #value="slotProps">
              <div
                class="country-item country-item-value flex"
                v-if="slotProps.value"
              >
                <i class="pi pi-calendar mr-2 format-flex-center"></i>
                <div style="color: #495057">
                  Tháng {{ slotProps.value }} ({{
                    moment(new Date(options["week_start_date"])).format("DD/MM")
                  }}
                  -
                  {{
                    moment(new Date(options["week_end_date"])).format("DD/MM")
                  }})
                </div>
              </div>
              <span v-else>
                {{ slotProps.placeholder }}
              </span>
            </template>
            <template #option="slotProps">
              <div
                class="country-item country-item-value py-2"
                v-if="slotProps.option"
              >
                <div>Tháng {{ slotProps.option.month }}</div>
              </div>
              <span v-else> Chưa có dữ liệu tháng </span>
            </template>
          </Dropdown>
        </div>
        <div class="form-group m-0" v-if="options.view === 1">
          <Dropdown
            :options="weeks"
            :filter="true"
            :showClear="false"
            v-model="options.week"
            @change="goWeek(options.week)"
            optionLabel="week_no"
            optionValue="week_no"
            placeholder="Chọn tuần"
            class="ip36"
            style="min-width: 170px"
          >
            <template #value="slotProps">
              <div
                class="country-item country-item-value flex"
                v-if="slotProps.value"
              >
                <i class="pi pi-calendar mr-2 format-flex-center"></i>
                <div>
                  Tuần {{ slotProps.value }} ({{
                    moment(new Date(options["week_start_date"])).format("DD/MM")
                  }}
                  -
                  {{
                    moment(new Date(options["week_end_date"])).format("DD/MM")
                  }})
                </div>
              </div>
              <span v-else>
                {{ slotProps.placeholder }}
              </span>
            </template>
            <template #option="slotProps">
              <div
                class="country-item country-item-value py-2"
                v-if="slotProps.option"
              >
                <div>
                  Tuần {{ slotProps.option.week_no }} ({{
                    moment(new Date(slotProps.option.week_start_date)).format(
                      "DD/MM"
                    )
                  }}
                  -
                  {{
                    moment(new Date(slotProps.option.week_end_date)).format(
                      "DD/MM"
                    )
                  }})
                </div>
              </div>
              <span v-else> Chưa có dữ liệu tuần </span>
            </template>
          </Dropdown>
        </div>
        <div class="form-group m-0 mr-2" v-if="options.view === 0">
          <Calendar
            inputId="basic"
            v-model="options.week_start_date"
            autocomplete="off"
            class="ip36"
            :showIcon="true"
            :manualInput="false"
            @date-select="changeStartDate()"
            placeholder="Từ ngày"
          />
        </div>
        <div class="form-group m-0" v-if="options.view === 0">
          <Calendar
            inputId="basic"
            v-model="options.week_end_date"
            autocomplete="off"
            class="ip36"
            :showIcon="true"
            :manualInput="false"
            @date-select="changeEndDate()"
            placeholder="Đến ngày"
          />
        </div>
      </template>
    </Toolbar>
    <div
      class="d-lang-table"
      style="
        height: calc(100vh - 185px);
        background-color: #fff;
        border-top: solid 1px rgba(0, 0, 0, 0.1);
      "
    >
      <div class="card w-full h-full">
        <div class="card-body w-full h-full">
          <div v-show="options.type === 1" class="w-full h-full format-center">
            <Chart
              id="chart1"
              type="bar"
              :data="datas"
              :options="basicOptions"
              :plugins="plugins"
              style="
                width: calc(100vh - 0px) !important;
                height: 40% !important;
                vertical-align: middle;
                align-items: center;
                display: flex;
              "
            />
          </div>
          <div v-show="options.type === 2" class="w-full h-full format-center">
            <Chart
              id="chart32"
              type="bar"
              :data="datas"
              :options="horizontalOptions"
              :plugins="plugins"
              style="
                width: calc(100vh - 0px) !important;
                height: 40% !important;
                vertical-align: middle;
                align-items: center;
                display: flex;
              "
            />
          </div>
          <div v-show="options.type === 3" class="w-full h-full format-center">
            <Chart
              id="chart3"
              type="doughnut"
              :data="datas"
              :options="lightOptions"
              :plugins="plugins"
              style="
                width: calc(100vh - 300px) !important;
                height: 40% !important;
                vertical-align: middle;
                align-items: center;
                display: flex;
              "
            />
          </div>
          <div v-show="options.type === 4" class="w-full h-full">
            <DataTable
              :value="tables"
              :scrollable="true"
              :lazy="true"
              :rowHover="true"
              :showGridlines="true"
              dataKey="Ma"
              scrollHeight="flex"
              filterDisplay="menu"
              filterMode="lenient"
              paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
              responsiveLayout="scroll"
            >
              <Column
                v-if="
                  options.loai !== 1 && options.loai !== 4 && options.loai !== 7
                "
                field="stt"
                header="STT"
                headerStyle="text-align:center;max-width:75px;height:50px"
                bodyStyle="text-align:center;max-width:75px;"
                class="align-items-center justify-content-center text-center"
              >
              </Column>
              <Column
                v-if="
                  options.loai !== 1 && options.loai !== 4 && options.loai !== 7
                "
                field="Ma"
                header="Mã"
                headerStyle="text-align:center;max-width:150px;height:50px"
                bodyStyle="text-align:center;max-width:150px;"
                class="align-items-center justify-content-center text-center"
              >
              </Column>
              <Column
                v-if="
                  options.loai !== 1 && options.loai !== 4 && options.loai !== 7
                "
                field="Ten"
                header="Tên"
                headerStyle="height:50px;max-width:auto;"
              >
                <template #body="slotProps">
                  <b>{{ slotProps.data.Ten }}</b>
                </template>
              </Column>
              <Column
                v-if="
                  options.loai === 1 ||
                  options.loai === 2 ||
                  options.loai === 3 ||
                  options.loai === 5 ||
                  options.loai === 6 ||
                  options.loai === 8 ||
                  options.loai === 9 ||
                  options.loai === 10
                "
                field="soluong_name"
                header="Số lượng"
                headerStyle="text-align:center;max-width:150px;height:50px"
                bodyStyle="text-align:center;max-width:150px;"
                class="align-items-center justify-content-center text-center"
              >
              </Column>
              <Column
                v-if="options.loai === 11 || options.loai === 12"
                field="soluong_TN_name"
                header="Tiếp nhận"
                headerStyle="text-align:center;max-width:150px;height:50px"
                bodyStyle="text-align:center;max-width:150px;"
                class="align-items-center justify-content-center text-center"
              >
              </Column>
              <Column
                v-if="options.loai === 11 || options.loai === 12"
                field="soluong_GQ_name"
                header="Đã giải quyết"
                headerStyle="text-align:center;max-width:150px;height:50px"
                bodyStyle="text-align:center;max-width:150px;"
                class="align-items-center justify-content-center text-center"
              >
              </Column>
              <Column
                v-if="options.loai === 11 || options.loai === 12"
                field="soluong_CHUA_name"
                header="Chưa giải quyết"
                headerStyle="text-align:center;max-width:150px;height:50px"
                bodyStyle="text-align:center;max-width:150px;"
                class="align-items-center justify-content-center text-center"
              >
              </Column>
              <Column
                v-if="
                  options.loai === 13 ||
                  options.loai === 14 ||
                  options.loai === 15
                "
                field="soluong_1_name"
                header="Quân nhân"
                headerStyle="text-align:center;max-width:150px;height:50px"
                bodyStyle="text-align:center;max-width:150px;"
                class="align-items-center justify-content-center text-center"
              >
              </Column>
              <Column
                v-if="options.loai === 13 || options.loai === 14"
                field="Ten2"
                header="Tên"
                headerStyle="height:50px;max-width:auto;"
              >
                <template #body="slotProps">
                  <b>{{ slotProps.data.Ten2 }}</b>
                </template>
              </Column>
              <Column
                v-if="
                  options.loai === 13 ||
                  options.loai === 14 ||
                  options.loai === 15
                "
                field="soluong_2_name"
                header="Khác quân nhân"
                headerStyle="text-align:center;max-width:150px;height:50px"
                bodyStyle="text-align:center;max-width:150px;"
                class="align-items-center justify-content-center text-center"
              >
              </Column>
            </DataTable>
          </div>
          <div
            v-show="
              !options.loading && options.type !== 4 && datas.datasets != null
            "
            class="w-full h-full format-center"
          >
            <span class="description">Hiện chưa có dữ liệu</span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
<style scoped>
.description {
  color: #aaa;
  font-size: 12px;
}
</style>
<style lang="scss" scoped>
::v-deep(.d-lang-table) {
  .p-datatable-thead .justify-content-center .p-column-header-content {
    justify-content: center !important;
  }

  .p-datatable-table {
    position: absolute;
  }

  .p-datatable-thead {
    position: sticky;
    top: 0;
    z-index: 1;
  }
}
::v-deep(.form-group) {
  .p-multiselect .p-multiselect-label,
  .p-dropdown .p-dropdown-label {
    height: 100%;
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