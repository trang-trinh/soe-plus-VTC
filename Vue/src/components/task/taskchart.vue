<script setup>
//Khai báo InJect và Import (import)
import { ref, inject, onMounted, watch } from "vue";
import groupuser from "./groupuser.vue";
import { useToast } from "vue-toastification";

const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const emitter = inject("emitter");
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const typeChart = ref(1);
const userFilter = ref(store.getters.user.user_id);

//Nơi nhận dữ liệu
const props = defineProps({
  isCheckTask: Boolean,
  dataChart: Object,
  typeChart: Intl,
  categoryid: Intl,
  projectid: Intl,
  typeDateFilter: Intl,
});
const listUsers = ref();
//Nơi nhận EMIT từ component
emitter.on("emitData", (obj) => {
  switch (obj.type) {
    case "listDropdownUser":
      listDropdownUser.value = obj.data;
      
      break;
    case "listUsers":
      listUsers.value = obj.data;
      break;
    case "listDropdownUserCheck":
      listDropdownUserCheck.value = obj.data;
      break;
      default:
      break;
  }
});
const displayDateChart = ref(false);
const headerDateChart = ref("");
const typeDateFilter = ref("modified_date");

// Khai báo hàm (function)
watch(props, () => {
  if (props.isCheckTask == true) {
    listColMain.value = [];
 typeChart.value = 1;
    dataCol.value = [];
    if (props.typeDateFilter == 0) typeDateFilter.value = "modified_date";
    else typeDateFilter.value = "created_date";
    userChart.value = props.typeChart;
    dataGroup.value = [];
    isCheckTask.value = true;
    listUserSelected.value = [store.getters.user.user_id];
    if (userChart.value == 1) {
      typeChartMain.value=0;
      typeDateFilter.value = "user_id";
      onShowUserChart();
    } else if (userChart.value == 2) {
        typeChartMain.value=1;
      datalists.value.totalRecords = JSON.parse(props.dataChart.totalRecords);
      datalists.value.finishedRecord = JSON.parse(
        props.dataChart.finishedRecord
      );
      datalists.value.tempClose = JSON.parse(props.dataChart.tempClose);
      datalists.value.unFinishRecord = JSON.parse(
        props.dataChart.unFinishRecord
      );
      datalists.value.waitedRecord = JSON.parse(props.dataChart.waitedRecord);
      datalists.value.err_work = JSON.parse(props.dataChart.err_work);
  
       multiAxisDataDefaul.value.datasets[0].data = [datalists.value.totalRecords
          ? datalists.value.totalRecords.length
          : 0,
        datalists.value.finishedRecord
          ? datalists.value.finishedRecord.length
          : 0,
        datalists.value.unFinishRecord
          ? datalists.value.unFinishRecord.length
          : 0,
        datalists.value.tempClose ? datalists.value.tempClose.length : 0,
        datalists.value.waitedRecord ? datalists.value.waitedRecord.length : 0,
        datalists.value.err_work ? datalists.value.err_work.length : 0,
      ];
            basicDataLineDefauld.value.datasets[0].data = [  datalists.value.totalRecords
          ? datalists.value.totalRecords.length
          : 0, datalists.value.finishedRecord
          ? datalists.value.finishedRecord.length
          : 0,
        datalists.value.unFinishRecord
          ? datalists.value.unFinishRecord.length
          : 0,
        datalists.value.tempClose ? datalists.value.tempClose.length : 0,
        datalists.value.waitedRecord ? datalists.value.waitedRecord.length : 0,
        datalists.value.err_work ? datalists.value.err_work.length : 0,];
        
      chartDatapie.value.datasets[0].data = [
        datalists.value.finishedRecord
          ? datalists.value.finishedRecord.length
          : 0,
        datalists.value.unFinishRecord
          ? datalists.value.unFinishRecord.length
          : 0,
        datalists.value.tempClose ? datalists.value.tempClose.length : 0,
        datalists.value.waitedRecord ? datalists.value.waitedRecord.length : 0,
        datalists.value.err_work ? datalists.value.err_work.length : 0,
      ];
  
    } else {
        typeChartMain.value=0;
      datalists.value.totalRecords = JSON.parse(props.dataChart.totalRecords);
      datalists.value.finishedRecord = JSON.parse(
        props.dataChart.finishedRecord
      );
      datalists.value.tempClose = JSON.parse(props.dataChart.tempClose);
      datalists.value.unFinishRecord = JSON.parse(
        props.dataChart.unFinishRecord
      );
      datalists.value.waitedRecord = JSON.parse(props.dataChart.waitedRecord);
      datalists.value.err_work = JSON.parse(props.dataChart.err_work);

      dataGroup.value.push(
        groupBy(JSON.parse(props.dataChart.totalRecords), typeDateFilter.value)
      );
      dataGroup.value.push(
        groupBy(
          JSON.parse(props.dataChart.finishedRecord),
          typeDateFilter.value
        )
      );
  console.log("áooaoo",datalists.value);
      dataGroup.value.push(
        groupBy(JSON.parse(props.dataChart.tempClose), typeDateFilter.value)
      );
      dataGroup.value.push(
        groupBy(
          JSON.parse(props.dataChart.unFinishRecord),
          typeDateFilter.value
        )
      );
      dataGroup.value.push(
        groupBy(JSON.parse(props.dataChart.waitedRecord), typeDateFilter.value)
      );
      dataGroup.value.push(
        groupBy(JSON.parse(props.dataChart.err_work), typeDateFilter.value)
      );
      // let max1=0;

      //   for (let index = 0; index <   dataGroup.value.length; index++) {
      //     const element =   dataGroup.value[index];

      //   // if(max1<element.length)
      //       console.log(">W",element);
      //   }
      let arrD = [];

      dataGroup.value.forEach((element) => {
        for (var i in element) {
          arrD.push(i);
        }
      });
      arrD = Array.from(new Set(arrD)).sort(function (a, b) {
        let ys = a.split("-");
        let yr = b.split("-");
        a = new Date(ys[1] + "-" + ys[0] + "-" + ys[2]);
        b = new Date(yr[1] + "-" + yr[0] + "-" + yr[2]);
        return a === b ? 0 : a < b ? -1 : 1;
      });
      dataChartShow.value = [];
      let chartBar1 = [];
      let chartBar2 = [];
      let chartBar3 = [];
      let chartBar4 = [];
      let chartBar5 = [];
      let chartBar6 = [];
      listColMain.value = [];

      arrD.forEach((element, i) => {
        listColMain.value.push(element);
        chartBar1.push(
          dataGroup.value[0] != null
            ? dataGroup.value[0][element]
              ? dataGroup.value[0][element].length
              : 0
            : 0
        );
        chartBar2.push(
          dataGroup.value[1] != null
            ? dataGroup.value[1][element]
              ? dataGroup.value[1][element].length
              : 0
            : 0
        );
        chartBar3.push(
          dataGroup.value[2] != null
            ? dataGroup.value[2][element]
              ? dataGroup.value[2][element].length
              : 0
            : 0
        );
        chartBar4.push(
          dataGroup.value[3] != null
            ? dataGroup.value[3][element]
              ? dataGroup.value[3][element].length
              : 0
            : 0
        );
        chartBar5.push(
          dataGroup.value[4] != null
            ? dataGroup.value[4][element]
              ? dataGroup.value[4][element].length
              : 0
            : 0
        );
        chartBar6.push(
          dataGroup.value[5] != null
            ? dataGroup.value[5][element]
              ? dataGroup.value[5][element].length
              : 0
            : 0
        );

        dataChartShow.value.push({
          date: element,
          totalRecords: dataGroup.value[0] ? dataGroup.value[0][element] : null,
          finishedRecord: dataGroup.value[1]
            ? dataGroup.value[1][element]
            : null,
          tempClose: dataGroup.value[2] ? dataGroup.value[2][element] : null,
          unFinishRecord: dataGroup.value[3]
            ? dataGroup.value[3][element]
            : null,
          waitedRecord: dataGroup.value[4] ? dataGroup.value[4][element] : null,
          err_work: dataGroup.value[5] ? dataGroup.value[5][element] : null,
        });
      });

      multiAxisDataMain.value.labels = listColMain.value;
      multiAxisDataMain.value.datasets[0].data = chartBar1;
      multiAxisDataMain.value.datasets[1].data = chartBar2;
      multiAxisDataMain.value.datasets[2].data = chartBar3;
      multiAxisDataMain.value.datasets[3].data = chartBar4;
      multiAxisDataMain.value.datasets[4].data = chartBar5;
      multiAxisDataMain.value.datasets[5].data = chartBar6;
      basicDataLine.value.labels = listColMain.value;
      basicDataLine.value.datasets[0].data = chartBar1;
      basicDataLine.value.datasets[1].data = chartBar2;
      basicDataLine.value.datasets[2].data = chartBar3;
      basicDataLine.value.datasets[3].data = chartBar4;
      basicDataLine.value.datasets[4].data = chartBar5;
      basicDataLine.value.datasets[5].data = chartBar6;
  
    }
  }
});

const filterMonth = ref();
const toggleFilterMonth = (event) => {
  filterMonth.value.toggle(event);
};
const justifyOptions = ref([
  { icon: "pi pi-calendar-minus", value: 0 },
  { icon: "pi pi-chart-bar", value: 1 },
  { icon: "pi pi-chart-line", value: 2 },

]);
const justifyOptionsChild = ref([
  { icon: "pi pi-chart-bar", value: 1 },
  { icon: "pi pi-chart-line", value: 2 },
  { icon: "pi pi-chart-pie", value: 3 },
]);
const filterDateChecklist = ref([
  { name: "Ngày cập nhật", code: 0 },

  { name: "Ngày tạo", code: 1 },
  { name: "Ngày thực hiện", code: 2 },
]);
const typeChartMain = ref(0);
const listColMain = ref([]);
const dataColMain = ref([]);
const listCol = ref([]);
const dataCol = ref([]);
const heightChart = ref(150);
const indexAxisChart = ref("x");

const multiAxisMainOptions = ref({
  indexAxis: "x",
  plugins: {
    legend: {
      labels: {
        color: "#495057",
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
const multiAxisOptions = ref({
  indexAxis: indexAxisChart.value,
  plugins: {
    legend: {
      labels: {
        color: "#495057",
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

const multiAxisDataChild = ref({
  labels: listCol.value,
  datasets: [
    {
      label: "Số Check List",
      backgroundColor: ["#607D8B"],
      data: dataCol.value,
    },
  ],
});


const multiAxisDataDefaul = ref({
  labels: [
    "Tất cả",
    "Đã hoàn thành",
    "Đang đợi Test",
    "Đang làm",
    "Test chưa OK",
    "Lỗi",
  ],
  datasets: [
    {
      label: "Tất cả",
      backgroundColor: [
        "#607D8B",
        "#689F38",
        "#9C27B0",
        "#2196F3",
        "#FBC02D",
        "#D32F2F",
      ],
      data: dataCol.value,
    },
  ],
});
const multiAxisDataMain = ref({
  labels: listColMain.value,
  datasets: [
    {
      label: "Tất cả",
      backgroundColor: ["#607D8B"],
      data: dataCol.value,
    },
    {
      label: "Đã hoàn thành",
      backgroundColor: ["#689F38"],
      data: dataCol.value,
    },

    {
      label: "Test chưa OK",
      backgroundColor: ["#FBC02D"],
      data: dataCol.value,
    },
    {
      label: "Đang làm",
      backgroundColor: ["#2196F3"],
      data: dataCol.value,
    },
    {
      label: "Đang đợi Test",
      backgroundColor: ["#9C27B0"],
      data: dataCol.value,
    },
    {
      label: "Lỗi",
      backgroundColor: ["#D32F2F"],
      data: dataCol.value,
    },
  ],
});

const multiAxisDateChart = ref({
  labels: [
    "Tất cả",
    "Đã hoàn thành",
    "Đang đợi Test",
    "Đang làm",
    "Test chưa OK",
    "Lỗi",
  ],
  datasets: [
    {
      label: "Tất cả",
      backgroundColor: [
        "#607D8B",
        "#689F38",
        "#9C27B0",
        "#2196F3",
        "#FBC02D",
        "#D32F2F",
      ],
      data: dataCol.value,
    },
  ],
});
const onChartNum = () => {
  typeChart.value = 1;
};
const listDropdownUser = ref([]);
const listDropdownUserCheck = ref();
const changeTypeChart = (value, index) => {
  let arr1 = [];
  let arr2 = [];
   let arr3 = [];
  let arw = groupBy(value, typeDateFilter.value);
  for (const key in arw) {
    if (Object.hasOwnProperty.call(arw, key)) {
      arr1.push(arw[key].length);
      arr2.push(arw[key][0].full_name);
      arr3.push(key);
    }
  }
  arr3 = arr3.sort(function (a, b) {
    let ys = a.split("-");
    let yr = b.split("-");
    a = new Date(ys[1] + "-" + ys[0] + "-" + ys[2]);
    b = new Date(yr[1] + "-" + yr[0] + "-" + yr[2]);
    return a === b ? 0 : a > b ? -1 : 1;
  });
  arr2 = arr2.sort(function (a, b) {
    return a === b ? 0 : a > b ? -1 : 1;
  });

  dataCol.value = arr1;
  if(userChart.value==1)
  listCol.value = arr2;
  else
    listCol.value = arr3;
   
  multiAxisDataChild.value.labels = listCol.value;
  multiAxisDataChild.value.datasets[0].data = dataCol.value;
  if (index == 1)
    multiAxisDataChild.value.datasets[0].backgroundColor = "#607D8B";
  if (index == 2)
    multiAxisDataChild.value.datasets[0].backgroundColor = "#9C27B0";
  if (index == 3)
    multiAxisDataChild.value.datasets[0].backgroundColor = "#689F38";

  if (index == 4)
    multiAxisDataChild.value.datasets[0].backgroundColor = "#2196F3";
  if (index == 5)
    multiAxisDataChild.value.datasets[0].backgroundColor = "#FBC02D";
  if (index == 6)
    multiAxisDataChild.value.datasets[0].backgroundColor = "#D32F2F";
  typeChart.value = 2;
 
};

const basicDataLineDefauld = ref({
  labels:[ "Tất cả","Đã hoàn thành","Test chưa OK","Đang làm","Đang đợi Test","Lỗi"],
  datasets: [
    {
      label: "Số Check List",
      data: dataCol.value,
      borderColor: "#42A5F5",
      fill: true,
      tension: 0.4,
    }
  ],
});
const basicDataLine = ref({
  labels: listCol.value,
  datasets: [
    {
      label: "Tất cả",
      data: dataCol.value,

      borderColor: "#607D8B",
      fill: false,
      tension: 0.4,
    },
    {
      label: "Đã hoàn thành",

      data: dataCol.value,
      borderColor: "#689F38",
      fill: false,
      tension: 0.4,
    },

    {
      label: "Test chưa OK",

      data: dataCol.value,
      borderColor: "#FBC02D",
      fill: false,
      tension: 0.4,
    },
    {
      label: "Đang làm",

      data: dataCol.value,
      borderColor: "#2196F3",
      fill: false,
      tension: 0.4,
    },
    {
      label: "Đang đợi Test",

      data: dataCol.value,
      borderColor: "#9C27B0",
      fill: false,
      tension: 0.4,
    },
    {
      label: "Lỗi",

      data: dataCol.value,
      borderColor: "#D32F2F",
      fill: false,
      tension: 0.4,
    },
  ],
});
const basicOptionsLine = ref({
  plugins: {
    legend: {
      labels: {
        color: "#495057",
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
const chartDatapie = ref({
  labels: ["Hoàn thành", "Đang làm", "Test chưa OK", "Đợi Test", "Lỗi"],
  datasets: [
    {
      data: dataCol.value,
      backgroundColor: ["#689F38", "#2196F3", "#FBC02D", "#9C27B0", "#D32F2F"],
      hoverBackgroundColor: [
        "#81C784",
        "#FFB74D",
        "#64B5F6",
        "#81C784",
        "#FFB74D",
      ],
    },
  ],
});
const lightOptions = ref({
  plugins: {
    legend: {
      labels: {
        color: "#495057",
      },
    },
  },
});
const dataChartShow = ref([]);
const dataGroup = ref([]);
const onShowDateChart = (data, check) => {
  multiAxisDateChart.value.datasets[0].data = [
    data.totalRecords ? data.totalRecords.length : 0,
    data.finishedRecord ? data.finishedRecord.length : 0,
    data.waitedRecord ? data.waitedRecord.length : 0,
    data.unFinishRecord ? data.unFinishRecord.length : 0,
    data.tempClose ? data.tempClose.length : 0,
    data.err_work ? data.err_work.length : 0,
  ];
  if (!check) headerDateChart.value = "Biểu đồ ngày " + data.date;
  else headerDateChart.value = "Biểu đồ của " + data.date[0].full_name;
  displayDateChart.value = true;
};
const closeDialog = () => {
  displayDateChart.value = false;
};
const onCleanFilterMonth = () => {
  if (monthPickerFilter.value) monthPickerFilter.value = null;
  if (weekPickerFilter.value) weekPickerFilter.value = null;
  options.value.Start_Date = null;
  options.value.End_Date = null;

  emitter.emit("emitData", {
    type: "onTaskFilter",
    data: {
      userFilter: userFilter.value,
      statusTask: options.value.statusTask,
      Start_Date: options.value.Start_Date,
      End_Date: options.value.End_Date,
      typeCheckList: options.value.typeCheckList,
      Date_Filter: options.value.Date_Filter,
    },
  });
};
var groupBy = function (xs, key) {
  if (xs != null)
    if (xs.length > 0)
      return xs.reduce(function (rv, x) {
        (rv[x[key]] = rv[x[key]] || []).push(x);
        return rv;
      }, {});
};
// var groupBy = function(xs, key) {
//   if(xs!=null)
//   return xs.reduce(function(rv, x) {
//     (rv[x[key]] = rv[x[key]] || []).push(x);
//     return rv;
//   }, {});
// };
//Khai báo biến (variable)
const basedomainURL = baseURL;
const taskDateFilter = ref();
const datalists = ref([]);
const toast = useToast();
const options = ref({
  IsNext: true,
  sort: "task_id",
  searchText: "",
  PageNo: 0,
  PageSize: 10,
  loading: false,
  totalRecords: null,
  finishedRecord: null,
  waitedRecord: null,
  tempClose: null,
  unFinishRecord: null,
  statusTask: null,
  outOfDate: null,
  SearchTextUser: "",
  Start_date: null,
  End_date: null,
  Date_Filter: 0,
  typeCheckList: 0,
});
const typeSidebar = ref("width:70% !important");

const isCheckTask = ref(false);
const sidebarTooltip = ref("Toàn màn hình");
// Khai báo hàm (function)

const monthPickerFilter = ref();

const weekPickerFilter = ref();
const todayClick = () => {
  taskDateFilter.value = [];
  taskDateFilter.value.push(new Date());
};
const delDayClick = () => {
  taskDateFilter.value = [];
  options.value.Start_Date = null;
  options.value.End_Date = null;
  emitter.emit("emitData", {
    type: "onTaskFilter",
    data: {
      userFilter: userFilter.value,
      statusTask: options.value.statusTask,
      Start_Date: options.value.Start_Date,
      End_Date: options.value.End_Date,
      typeCheckList: options.value.typeCheckList,
      Date_Filter: options.value.Date_Filter,
    },
  });
};
const onDayClick = () => {
  if (monthPickerFilter.value) monthPickerFilter.value = null;
  if (weekPickerFilter.value) weekPickerFilter.value = null;
  if (taskDateFilter.value == null) taskDateFilter.value = [];
  else {
    options.value.Start_Date = taskDateFilter.value[0];
    options.value.End_Date = taskDateFilter.value[1];
    emitter.emit("emitData", {
      type: "onTaskFilter",
      data: {
        userFilter: userFilter.value,
        statusTask: options.value.statusTask,
        Start_Date: options.value.Start_Date,
        End_Date: options.value.End_Date,
        typeCheckList: options.value.typeCheckList,
        Date_Filter: options.value.Date_Filter,
      },
    });
  }
};
const totalRecords = ref(10);
const onFilterMonth = (check) => {
  if (check) {
    if (weekPickerFilter.value) {
      options.value.Start_Date = weekPickerFilter.value[0];
      options.value.End_Date = weekPickerFilter.value[1];
    } else {
      options.value.Start_Date = null;
      options.value.End_Date = null;
    }
  } else {
    if (monthPickerFilter.value) {
      weekPickerFilter.value = [];
      let day = new Date(
        monthPickerFilter.value.year,
        monthPickerFilter.value.month + 1,
        0
      ).getDate();
      options.value.Start_Date = new Date(
        monthPickerFilter.value.month +
          1 +
          "/01" +
          "/" +
          monthPickerFilter.value.year
      );
      options.value.End_Date = new Date(
        monthPickerFilter.value.month +
          1 +
          "/" +
          day +
          "/" +
          monthPickerFilter.value.year
      );
    } else {
      options.value.Start_Date = null;
      options.value.End_Date = null;
    }
  }
  options.value.PageNo = 0;
  emitter.emit("emitData", {
    type: "onTaskFilter",
    data: {
      userFilter: userFilter.value,
      statusTask: options.value.statusTask,
      Start_Date: options.value.Start_Date,
      End_Date: options.value.End_Date,
      typeCheckList: options.value.typeCheckList,
      Date_Filter: options.value.Date_Filter,
    },
  });
};
const onTaskFilter = () => {
  emitter.emit("emitData", {
    type: "onTaskFilter",
    data: {
      userFilter: userFilter.value,
      statusTask: options.value.statusTask,
      Start_Date: options.value.Start_Date,
      End_Date: options.value.End_Date,
      typeCheckList: options.value.typeCheckList,
      Date_Filter: options.value.Date_Filter,
    },
  });
};
const hideSidebar = () => {
  emitter.emit("emitData", { type: "hideChartSidebar", data: null });
};
const onHideSidebar = () => {
  isCheckTask.value = false;
};
const onFullSidebar = () => {
  if (typeSidebar.value == "width:70% !important") {
    sidebarTooltip.value = "Thu nhỏ";
    typeSidebar.value = "width:100% !important";
  } else {
    sidebarTooltip.value = "Toàn màn hình";
    typeSidebar.value = "width:70% !important";
  }
  // if(typeSidebar.value=="right")
  // {
  // typeSidebar.value="full";
  //   console.log(
  //   "!@3",typeSidebar.value
  // );
  // }
  // if(typeSidebar.value=="full"){
  // typeSidebar.value="right";
  // }
};

/// Biểu đồ Cá nhân

const userChart = ref(0);

const listUserSelected = ref([store.getters.user.user_id]);

const onShowUserChart = () => {
  dataGroup.value = [];
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "task_user_chart",
        par: [
          { par: "user_id", va: userFilter.value },
          { par: "list_users", va: listUserSelected.value.toString() },
          { par: "search", va: options.value.searchText },
          {
            par: "category_id",
            va: props.categoryid ? props.categoryid : null,
          },
          { par: "typefilter", va: options.value.typeCheckList },
          { par: "datefilter", va: options.value.Date_Filter },
          { par: "project_id", va: props.projectid ? props.projectid : null },
          { par: "status", va: options.value.statusTask },
          { par: "start_date", va: options.value.Start_Date },
          { par: "end_date", va: options.value.End_Date },
        ],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0][0];

      datalists.value.totalRecords = JSON.parse(data.totalRecords);
      datalists.value.finishedRecord = JSON.parse(data.finishedRecord);
      datalists.value.tempClose = JSON.parse(data.tempClose);
      datalists.value.unFinishRecord = JSON.parse(data.unFinishRecord);
      datalists.value.waitedRecord = JSON.parse(data.waitedRecord);
      datalists.value.err_work = JSON.parse(data.err_work);

      dataGroup.value.push(
        groupBy(JSON.parse(data.totalRecords), typeDateFilter.value)
      );

      dataGroup.value.push(
        groupBy(JSON.parse(data.finishedRecord), typeDateFilter.value)
      );
      dataGroup.value.push(
        groupBy(JSON.parse(data.tempClose), typeDateFilter.value)
      );
      dataGroup.value.push(
        groupBy(JSON.parse(data.unFinishRecord), typeDateFilter.value)
      );
      dataGroup.value.push(
        groupBy(JSON.parse(data.waitedRecord), typeDateFilter.value)
      );
      dataGroup.value.push(
        groupBy(JSON.parse(data.err_work), typeDateFilter.value)
      );

      let arrD = [];

      dataGroup.value.forEach((element) => {
        for (var i in element) {
          arrD.push(i);
        }
      });
      arrD = Array.from(new Set(arrD)).sort(function (a, b) {
        let ys = a.split("-");
        let yr = b.split("-");
        a = new Date(ys[1] + "-" + ys[0] + "-" + ys[2]);
        b = new Date(yr[1] + "-" + yr[0] + "-" + yr[2]);
        return a === b ? 0 : a < b ? -1 : 1;
      });

      dataChartShow.value = [];
      let chartBar1 = [];
      let chartBar2 = [];
      let chartBar3 = [];
      let chartBar4 = [];
      let chartBar5 = [];
      let chartBar6 = [];
      listColMain.value = [];

      arrD.forEach((element, i) => {
        listColMain.value.push(element);
        chartBar1.push(
          dataGroup.value[0] != null
            ? dataGroup.value[0][element]
              ? dataGroup.value[0][element].length
              : 0
            : 0
        );
        chartBar2.push(
          dataGroup.value[1] != null
            ? dataGroup.value[1][element]
              ? dataGroup.value[1][element].length
              : 0
            : 0
        );
        chartBar3.push(
          dataGroup.value[2] != null
            ? dataGroup.value[2][element]
              ? dataGroup.value[2][element].length
              : 0
            : 0
        );
        chartBar4.push(
          dataGroup.value[3] != null
            ? dataGroup.value[3][element]
              ? dataGroup.value[3][element].length
              : 0
            : 0
        );
        chartBar5.push(
          dataGroup.value[4] != null
            ? dataGroup.value[4][element]
              ? dataGroup.value[4][element].length
              : 0
            : 0
        );
        chartBar6.push(
          dataGroup.value[5] != null
            ? dataGroup.value[5][element]
              ? dataGroup.value[5][element].length
              : 0
            : 0
        );
        dataChartShow.value.push({
          date: userChart.value == 1 ? dataGroup.value[0][element] : element,
          totalRecords: dataGroup.value[0] ? dataGroup.value[0][element] : null,
          finishedRecord: dataGroup.value[1]
            ? dataGroup.value[1][element]
            : null,
          tempClose: dataGroup.value[2] ? dataGroup.value[2][element] : null,
          unFinishRecord: dataGroup.value[3]
            ? dataGroup.value[3][element]
            : null,
          waitedRecord: dataGroup.value[4] ? dataGroup.value[4][element] : null,
          err_work: dataGroup.value[5] ? dataGroup.value[5][element] : null,
        });
      });
      let arrw = [];
      listColMain.value.forEach((element) => {
        listUsers.value
          .filter((x) => x.data.user_id == element)
          .forEach((x) => {
            arrw.push(x.data.full_name);
          });
      });
      listColMain.value = arrw;

      multiAxisDataMain.value.labels = listColMain.value;
      multiAxisDataMain.value.datasets[0].data = chartBar1;
      multiAxisDataMain.value.datasets[1].data = chartBar2;
      multiAxisDataMain.value.datasets[2].data = chartBar3;
      multiAxisDataMain.value.datasets[3].data = chartBar4;
      multiAxisDataMain.value.datasets[4].data = chartBar5;
      multiAxisDataMain.value.datasets[5].data = chartBar6;
      basicDataLine.value.labels = listColMain.value;
      basicDataLine.value.datasets[0].data = chartBar1;
      basicDataLine.value.datasets[1].data = chartBar2;
      basicDataLine.value.datasets[2].data = chartBar3;
      basicDataLine.value.datasets[3].data = chartBar4;
      basicDataLine.value.datasets[4].data = chartBar5;
      basicDataLine.value.datasets[5].data = chartBar6;

      chartDatapie.value.datasets[0].data = [
        datalists.value.finishedRecord
          ? datalists.value.finishedRecord.length
          : 0,
        datalists.value.unFinishRecord
          ? datalists.value.unFinishRecord.length
          : 0,
        datalists.value.tempClose ? datalists.value.tempClose.length : 0,
        datalists.value.waitedRecord ? datalists.value.waitedRecord.length : 0,
        datalists.value.err_work ? datalists.value.err_work.length : 0,
      ];
    })
    .catch((error) => {
      console.log(error);
    });
};

onMounted(() => {
  return {};
});
</script>
<template>
  <Sidebar
    v-model:visible="isCheckTask"
    :style="typeSidebar"
    :showCloseIcon="false"
    position="right"
    v-on:hide="hideSidebar()"
  >
    <div class="overflow-y-hidden">
      <div>
        <div>
          <Toolbar class="custoolbar mt-2">
            <template #start>
              <Button
                @click="onFullSidebar"
                v-tooltip.bottom="sidebarTooltip"
                icon="pi pi-th-large"
                class="p-button-rounded"
              />
              <h2 class="px-2">Biểu đồ</h2>
            </template>
            <template #end>
              <Button
                @click="onHideSidebar"
                icon="pi pi-times"
                class="p-button-rounded"
              />
            </template>
          </Toolbar>
        </div>
        <div>
          <Toolbar class="custoolbar mx-2">
            <template #start> </template>
            <template #end>
              <SelectButton
                v-model="typeChartMain"
                :options="justifyOptionsChild"
                optionValue="value"
                dataKey="value"
                v-if="userChart == 2"
              >
                <template #option="slotProps">
                  <i :class="slotProps.option.icon"></i>
                </template>
              </SelectButton>
              <SelectButton
                v-model="typeChartMain"
                :options="justifyOptions"
                optionValue="value"
                dataKey="value"
                v-else
              >
                <template #option="slotProps">
                  <i :class="slotProps.option.icon"></i>
                </template>
              </SelectButton>
            </template>
          </Toolbar>
        </div>
        <div v-if="userChart == 0 || userChart == 2">
          <Toolbar class="custoolbar mx-2">
            <template #start>
              <groupuser />
            </template>
            <template #end>
              <Dropdown
                v-model="options.Date_Filter"
                :options="filterDateChecklist"
                optionLabel="name"
                optionValue="code"
                class="mx-2 w-12rem"
                spellcheck="true"
                @change="onTaskFilter"
              />
              <Calendar
                placeholder="Lọc theo ngày"
                id="range"
                v-model="taskDateFilter"
                :showIcon="true"
                selectionMode="range"
                :manualInput="false"
              >
                <template #footer>
                  <div class="w-full flex">
                    <div class="w-4 format-center">
                      <span
                        @click="todayClick"
                        class="cursor-pointer text-primary"
                        >Hôm nay</span
                      >
                    </div>
                    <div class="w-4 format-center">
                      <Button @click="onDayClick" label="Thực hiện"></Button>
                    </div>
                    <div class="w-4 format-center">
                      <span
                        @click="delDayClick"
                        class="cursor-pointer text-primary"
                        >Xóa</span
                      >
                    </div>
                  </div>
                </template>
              </Calendar>

              <Button
                type="button"
                class="ml-2 p-button-outlined p-button-secondary"
                icon="pi pi-filter"
                @click="toggleFilterMonth($event)"
                aria-haspopup="true"
                aria-controls="overlay_panelMonth"
              />
            </template>
          </Toolbar>
        </div>
      </div>
      <div v-if="userChart == 0">
        <div v-if="typeChartMain == 0">
          <div v-if="typeChart == 1" class="d-lang-table">
            <DataTable
              :value="dataChartShow"
              dataKey="id"
              :rowHover="true"
              responsiveLayout="scroll"
              :scrollable="true"
              scrollHeight="flex"
              :loading="options.loading"
              :lazy="true"
            >
              <template #empty>
                <div
                  class="
                    align-items-center
                    justify-content-center
                    p-4
                    text-center
                    m-auto
                  "
                >
                  <img src="../../assets/background/nodata.png" height="144" />
                  <h3 class="m-1">Không có dữ liệu</h3>
                </div></template
              >

              <Column
                headerStyle="justify-content: center;text-align:center;height:50px; font-weight:700"
                bodyStyle="text-align:center;max-height:60px"
                class="
                  align-items-center
                  justify-content-center
                  text-center
                  w-2
                "
                field="date"
                header="Ngày"
              >
                <template #body="slotProps">
                  <div class="text-lg">{{ slotProps.data.date }}</div>
                </template></Column
              >

              <Column
                headerStyle="justify-content: center;text-align:center;height:50px; font-weight:700;width:150px"
                bodyStyle="text-align:center;max-height:60px ;width:150px"
                field="totalRecords"
                header="Tất cả"
              >
                <template #body="slotProps"
                  ><div class="w-10 pt-2">
                    <KProgress
                      color="#607D8B"
                      v-if="slotProps.data.totalRecords"
                      :show-text="false"
                      class="w-full"
                      :percent="
                      100
                      "
                      :line-height="15"
                    />
                  </div>
                  <div class="w-2" style="line-height: 15px">
                    {{
                      slotProps.data.totalRecords
                        ? slotProps.data.totalRecords.length
                        : ""
                    }}
                  </div>
                </template>
              </Column>

              <Column
                headerStyle="justify-content: center;text-align:center;height:50px; font-weight:700; width:120px"
                bodyStyle="text-align:center;max-height:60px; width:120px"
                field="finishedRecord"
                header="Đã xử lý"
              >
                <template #body="slotProps"
                  ><div class="w-10 pt-2">
                    <KProgress
                      color="#689F38"
                      v-if="slotProps.data.finishedRecord"
                      :show-text="false"
                      class="w-full"
                      :percent="
                        parseInt(
                          (
                            (slotProps.data.finishedRecord.length /
                           slotProps.data.totalRecords.length) *
                            100
                          ).toFixed()
                        )
                      "
                      :line-height="15"
                    />
                  </div>
                  <div class="w-2" style="line-height: 15px">
                    {{
                      slotProps.data.finishedRecord
                        ? slotProps.data.finishedRecord.length
                        : ""
                    }}
                  </div>
                </template> </Column
              ><Column
                headerStyle="justify-content: center;text-align:center;height:50px; font-weight:700; width:120px"
                bodyStyle="text-align:center;max-height:60px; width:120px"
                field="waitedRecord"
                header="Đang đợi Test"
              >
                <template #body="slotProps"
                  ><div class="w-10 pt-2">
                    <KProgress
                      color="#9C27B0"
                      v-if="slotProps.data.waitedRecord"
                      :show-text="false"
                      class="w-full"
                      :percent="
                        parseInt(
                          Math.floor(
                            (slotProps.data.waitedRecord.length / slotProps.data.totalRecords.length) *
                              100
                          )
                        )
                      "
                      :line-height="15"
                    />
                  </div>
                  <div class="w-2" style="line-height: 15px">
                    {{
                      slotProps.data.waitedRecord
                        ? slotProps.data.waitedRecord.length
                        : ""
                    }}
                  </div>
                </template>
              </Column>
              <Column
                headerStyle="justify-content: center;text-align:center;height:50px; font-weight:700; width:120px"
                bodyStyle="text-align:center;max-height:60px; width:120px"
                field="unFinishRecord"
                header="Đang xử lý"
              >
                <template #body="slotProps"
                  ><div class="w-10 pt-2">
                    <KProgress
                      color="#2196F3"
                      v-if="slotProps.data.unFinishRecord"
                      :show-text="false"
                      class="w-full"
                      :percent="
                        parseInt(
                          Math.floor(
                            (slotProps.data.unFinishRecord.length /
                           slotProps.data.totalRecords.length) *
                              100
                          )
                        )
                      "
                      :line-height="15"
                    />
                  </div>
                  <div class="w-2" style="line-height: 15px">
                    {{
                      slotProps.data.unFinishRecord
                        ? slotProps.data.unFinishRecord.length
                        : ""
                    }}
                  </div>
                </template>
              </Column>
              <Column
                headerStyle="justify-content: center;text-align:center;height:50px; font-weight:700; width:120px"
                bodyStyle="text-align:center;max-height:60px; width:120px"
                field="tempClose"
                header="Test chưa OK"
              >
                <template #body="slotProps">
                  <div class="w-10 pt-2">
                    <KProgress
                      color="#FBC02D"
                      v-if="slotProps.data.tempClose"
                      :show-text="false"
                      class="w-full"
                      :percent="
                        parseInt(
                          (
                            (slotProps.data.tempClose.length / slotProps.data.totalRecords.length) *
                            100
                          ).toFixed()
                        )
                      "
                      :line-height="15"
                    />
                  </div>
                  <div class="w-2" style="line-height: 15px">
                    {{
                      slotProps.data.tempClose
                        ? slotProps.data.tempClose.length
                        : ""
                    }}
                  </div>
                </template>
              </Column>
              <Column
                headerStyle="justify-content: center;text-align:center;height:50px; font-weight:700; width:120px"
                bodyStyle="text-align:center;max-height:60px; width:120px"
                field="err_work"
                header="Lỗi"
              >
                <template #body="slotProps">
                  <div class="w-10 pt-2">
                    <KProgress
                      color="#D32F2F"
                      v-if="slotProps.data.err_work"
                      :show-text="false"
                      class="w-full"
                      :percent="
                        parseInt(
                          (
                            (slotProps.data.err_work.length / slotProps.data.totalRecords.length) *
                            100
                          ).toFixed()
                        )
                      "
                      :line-height="15"
                    />
                  </div>
                  <div class="w-2" style="line-height: 15px">
                    {{
                      slotProps.data.err_work
                        ? slotProps.data.err_work.length
                        : ""
                    }}
                  </div>
                </template></Column
              >
              <Column
                headerStyle="justify-content: center;text-align:center;height:50px; width:100px"
                bodyStyle="justify-content: center;text-align:center; width:100px"
                field="err_work"
                header="Chi tiết"
              >
                <template #body="slotProps">
                  <!-- <div class="w-10 pt-2">
                    pi-info-circle
                   </div> -->
                  <div class="format-center">
                    <Button
                      @click="onShowDateChart(slotProps.data, false)"
                      icon="pi pi-info-circle"
                      class="p-button-rounded p-button-info"
                    />
                  </div> </template
              ></Column>
            </DataTable>
          </div>
        </div>
        <div v-else-if="typeChartMain == 1">
          <div v-if="typeChart == 1" class="d-lang-table relative">
            <div>
              <Chart
                type="bar"
                :height="100"
                :data="multiAxisDataMain"
                :options="multiAxisMainOptions"
              />
            </div>
          </div>
        </div>
        <div v-else-if="typeChartMain == 2">
          <div v-if="typeChart == 1" class="d-lang-table">
            <div>
              <Chart
                :height="100"
                type="line"
                :data="basicDataLine"
                :options="basicOptionsLine"
              />
            </div>
          </div>
        </div>
        <div v-else-if="typeChartMain == 3">
          <!-- <div v-if="typeChart == 1" class="d-lang-table">
            <div class="format-center">
              <Chart
                type="pie"
                :style="
                  typeSidebar == 'width:70% !important'
                    ? 'width: 50% !important'
                    : 'width: 30% !important'
                "
                :data="chartDatapie"
                :options="lightOptions"
              />
            </div>
          </div> -->
        </div>
        <div v-if="typeChart == 2" class="d-lang-table">
          <div>
            
            <Chart
              type="bar"
              :height="100"
              :data="multiAxisDataChild"
              :options="multiAxisOptions"
            />
          </div>
        </div>
        <div class="w-full flex mt-2 pr-4">
          <div class="w-2 text-left font-bold text-lg format-center">
            Tổng cộng:
          </div>
          <div class="px-2 w-2 text-center">
            <Button
              class="
                w-full
                font-bold
                justify-content-center
                p-button-outlined p-button-secondary
              "
              @click="onChartNum"
              >{{
                datalists.totalRecords ? datalists.totalRecords.length : 0
              }}</Button
            >
          </div>

          <div class="px-2 w-2 text-center">
            <Button
              @click="onChartNum"
              class="
                w-full
                font-bold
                justify-content-center
                p-button-outlined p-button-success
              "
              >{{
                datalists.finishedRecord ? datalists.finishedRecord.length : 0
              }}</Button
            >
          </div>
          <div class="px-2 w-2 text-center">
            <Button
              @click="onChartNum"
              class="
                w-full
                font-bold
                justify-content-center
                p-button-outlined p-button-help
              "
              >{{
                datalists.waitedRecord ? datalists.waitedRecord.length : 0
              }}</Button
            >
          </div>
          <div class="px-2 w-2 text-center">
            <Button
              @click="onChartNum"
              class="w-full justify-content-center p-button-outlined font-bold"
              >{{
                datalists.unFinishRecord ? datalists.unFinishRecord.length : 0
              }}</Button
            >
          </div>
          <div class="px-2 w-2 text-center">
            <Button
              @click="onChartNum"
              class="
                w-full
                font-bold
                justify-content-center
                p-button-outlined p-button-warning
              "
              >{{
                datalists.tempClose ? datalists.tempClose.length : 0
              }}</Button
            >
          </div>
          <div class="px-2 w-2 text-center">
            <Button
              @click="onChartNum"
              class="
                w-full
                font-bold
                justify-content-center
                p-button-outlined p-button-danger
              "
              >{{ datalists.err_work ? datalists.err_work.length : 0 }}</Button
            >
          </div>
          <div class="px-2 w-2 text-center"></div>
        </div>
        <div class="w-full flex mt-2 pr-4">
          <div class="w-2 text-left font-bold text-lg format-center">
            Biểu đồ:
          </div>
          <div class="px-2 w-2 text-center">
            <Button
              @click="changeTypeChart(datalists.totalRecords, 1)"
              icon="pi 
pi-chart-bar"
              class="w-full justify-content-center p-button-secondary"
            ></Button>
          </div>

          <div class="px-2 w-2 text-center">
            <Button
              @click="changeTypeChart(datalists.finishedRecord, 3)"
              icon="pi 
pi-chart-bar"
              class="w-full justify-content-center p-button-success"
            ></Button>
          </div>
          <div class="px-2 w-2 text-center">
            <Button
              @click="changeTypeChart(datalists.waitedRecord, 2)"
              icon="pi 
pi-chart-bar"
              class="w-full justify-content-center p-button-help"
            ></Button>
          </div>
          <div class="px-2 w-2 text-center">
            <Button
              @click="changeTypeChart(datalists.unFinishRecord, 4)"
              icon="pi 
pi-chart-bar"
              class="w-full justify-content-center"
            ></Button>
          </div>
          <div class="px-2 w-2 text-center">
            <Button
              @click="changeTypeChart(datalists.tempClose, 5)"
              icon="pi 
pi-chart-bar"
              class="w-full justify-content-center p-button-warning"
            ></Button>
          </div>
          <div class="px-2 w-2 text-center">
            <Button
              @click="changeTypeChart(datalists.err_work, 6)"
              icon="
pi pi-chart-bar"
              class="w-full justify-content-center p-button-danger"
            ></Button>
          </div>
          <div class="px-2 w-2 text-center"></div>
        </div>
      </div>
    </div>
    <div v-if="userChart == 1">
      <div>
        <Toolbar class="custoolbar mx-2">
          <template #start>
            <div class="p-inputgroup w-20rem">
              <span class="p-inputgroup-addon">
                <i class="pi pi-user"></i>
              </span>
              <span class="p-float-label">
                <MultiSelect
                  id="multiselect"
                  :filter="true"
                  v-model="listUserSelected"
                  :options="listDropdownUser"
                  optionValue="code"
                  optionLabel="name"
                  class="col-4 ip36 p-0"
                  @change="onShowUserChart()"
                >
                  <template #option="slotProps">
                    <div class="country-item flex">
                      <Avatar
                        :image="
                          slotProps.option.avatar
                            ? basedomainURL + slotProps.option.avatar
                            : basedomainURL + '/Portals/Image/noimg.jpg'
                        "
                        class="mr-2 w-2rem h-2rem"
                        size="large"
                        shape="circle"
                      />
                      <div class="pt-1">{{ slotProps.option.name }}</div>
                    </div>
                  </template>
                </MultiSelect>

                <label for="multiselect">Danh sách nhân viên</label>
              </span>
            </div>
          </template>
          <template #end>
            <Dropdown
              v-model="options.Date_Filter"
              :options="filterDateChecklist"
              optionLabel="name"
              optionValue="code"
              class="mx-2 w-12rem"
              spellcheck="true"
            />
            <Calendar
              placeholder="Lọc theo ngày"
              id="range"
              v-model="taskDateFilter"
              :showIcon="true"
              selectionMode="range"
              :manualInput="false"
            >
              <template #footer>
                <div class="w-full flex">
                  <div class="w-4 format-center">
                    <span
                      @click="todayClick"
                      class="cursor-pointer text-primary"
                      >Hôm nay</span
                    >
                  </div>
                  <div class="w-4 format-center">
                    <Button @click="onDayClick" label="Thực hiện"></Button>
                  </div>
                  <div class="w-4 format-center">
                    <span
                      @click="delDayClick"
                      class="cursor-pointer text-primary"
                      >Xóa</span
                    >
                  </div>
                </div>
              </template>
            </Calendar>

            <Button
              type="button"
              class="ml-2 p-button-outlined p-button-secondary"
              icon="pi pi-filter"
              @click="toggleFilterMonth($event)"
              aria-haspopup="true"
              aria-controls="overlay_panelMonth"
            />
          </template>
        </Toolbar>
      </div>

      <div v-if="typeChartMain == 0">
        <div v-if="typeChart == 1" class="d-lang-table">
          <DataTable
            :value="dataChartShow"
            dataKey="id"
            :rowHover="true"
            responsiveLayout="scroll"
            :scrollable="true"
            scrollHeight="flex"
            :loading="options.loading"
            :lazy="true"
          >
            <template #empty>
              <div
                class="
                  align-items-center
                  justify-content-center
                  p-4
                  text-center
                  m-auto
                "
              >
                <img src="../../assets/background/nodata.png" height="144" />
                <h3 class="m-1">Không có dữ liệu</h3>
              </div></template
            >

            <Column
              headerStyle="justify-content: center;text-align:center;height:50px; font-weight:700"
              bodyStyle="text-align:center;max-height:60px"
              class="align-items-center justify-content-center text-center w-2"
              field="date"
              header="Nhân viên"
            >
              <template #body="slotProps">
                <div>
                  <Avatar
                    v-tooltip.top="slotProps.data.date[0].full_name"
                    :image="
                      slotProps.data.date[0].avatar
                        ? basedomainURL + slotProps.data.date[0].avatar
                        : basedomainURL + '/Portals/Image/noimg.jpg'
                    "
                    :style="'border: 3px solid white'"
                    @error="basedomainURL + '/Portals/Image/noimg.jpg'"
                    size="large"
                    shape="circle"
                    class="cursor-pointer"
                  />
                </div> </template
            ></Column>

            <Column
              headerStyle="justify-content: center;text-align:center;height:50px; font-weight:700;width:150px"
              bodyStyle="text-align:center;max-height:60px ;width:150px"
              field="totalRecords"
              header="Tất cả"
            >
              <template #body="slotProps"
                ><div class="w-10 pt-2">
                  <KProgress
                    color="#607D8B"
                    v-if="slotProps.data.totalRecords"
                    :show-text="false"
                    class="w-full"
                    :percent="
                      parseInt(
                        (
                          (slotProps.data.totalRecords.length /
                            slotProps.data.totalRecords.length) *
                          100
                        ).toFixed()
                      )
                    "
                    :line-height="15"
                  />
                </div>
                <div class="w-2" style="line-height: 15px">
                    {{
                      slotProps.data.totalRecords
                        ? slotProps.data.totalRecords.length
                        : ""
                    }}
                  </div>
              </template>
            </Column>

            <Column
              headerStyle="justify-content: center;text-align:center;height:50px; font-weight:700; width:120px"
              bodyStyle="text-align:center;max-height:60px; width:120px"
              field="finishedRecord"
              header="Đã xử lý"
            >
              <template #body="slotProps"
                ><div class="w-10 pt-2">
                  <KProgress
                    color="#689F38"
                    v-if="slotProps.data.finishedRecord"
                    :show-text="false"
                    class="w-full"
                    :percent="
                      parseInt(
                        (
                          (slotProps.data.finishedRecord.length /
                            slotProps.data.totalRecords.length) *
                          100
                        ).toFixed()
                      )
                    "
                    :line-height="15"
                  />
                </div>
                <div class="w-2" style="line-height: 15px">
                    {{
                      slotProps.data.finishedRecord
                        ? slotProps.data.finishedRecord.length
                        : ""
                    }}
                  </div>
              </template> </Column
            ><Column
              headerStyle="justify-content: center;text-align:center;height:50px; font-weight:700; width:120px"
              bodyStyle="text-align:center;max-height:60px; width:120px"
              field="waitedRecord"
              header="Đang đợi Test"
            >
              <template #body="slotProps"
                ><div class="w-10 pt-2">
                  <KProgress
                    color="#9C27B0"
                    v-if="slotProps.data.waitedRecord"
                    :show-text="false"
                    class="w-full"
                    :percent="
                      parseInt(
                        Math.floor(
                          (slotProps.data.waitedRecord.length /
                            slotProps.data.totalRecords.length) *
                            100
                        )
                      )
                    "
                    :line-height="15"
                  />
                </div>
                <div class="w-2" style="line-height: 15px">
                    {{
                      slotProps.data.waitedRecord
                        ? slotProps.data.waitedRecord.length
                        : ""
                    }}
                  </div>
              </template>
            </Column>
            <Column
              headerStyle="justify-content: center;text-align:center;height:50px; font-weight:700; width:120px"
              bodyStyle="text-align:center;max-height:60px; width:120px"
              field="unFinishRecord"
              header="Đang xử lý"
            >
              <template #body="slotProps"
                ><div class="w-10 pt-2">
                  <KProgress
                    color="#2196F3"
                    v-if="slotProps.data.unFinishRecord"
                    :show-text="false"
                    class="w-full"
                    :percent="
                      parseInt(
                        Math.floor(
                          (slotProps.data.unFinishRecord.length /
                            slotProps.data.totalRecords.length) *
                            100
                        )
                      )
                    "
                    :line-height="15"
                  />
                </div>
                <div class="w-2" style="line-height: 15px">
                    {{
                      slotProps.data.unFinishRecord
                        ? slotProps.data.unFinishRecord.length
                        : ""
                    }}
                  </div>
              </template>
            </Column>
            <Column
              headerStyle="justify-content: center;text-align:center;height:50px; font-weight:700; width:120px"
              bodyStyle="text-align:center;max-height:60px; width:120px"
              field="tempClose"
              header="Test chưa OK"
            >
              <template #body="slotProps">
                <div class="w-10 pt-2">
                  <KProgress
                    color="#FBC02D"
                    v-if="slotProps.data.tempClose"
                    :show-text="false"
                    class="w-full"
                    :percent="
                      parseInt(
                        (
                          (slotProps.data.tempClose.length /
                            slotProps.data.totalRecords.length) *
                          100
                        ).toFixed()
                      )
                    "
                    :line-height="15"
                  />
                </div>
                <div class="w-2" style="line-height: 15px">
                    {{
                      slotProps.data.tempClose
                        ? slotProps.data.tempClose.length
                        : ""
                    }}
                  </div>
              </template>
            </Column>
            <Column
              headerStyle="justify-content: center;text-align:center;height:50px; font-weight:700; width:120px"
              bodyStyle="text-align:center;max-height:60px; width:120px"
              field="err_work"
              header="Lỗi"
            >
              <template #body="slotProps">
                <div class="w-10 pt-2">
                  <KProgress
                    color="#D32F2F"
                    v-if="slotProps.data.err_work"
                    :show-text="false"
                    class="w-full"
                    :percent="
                      parseInt(
                        (
                          (slotProps.data.err_work.length /
                            slotProps.data.totalRecords.length) *
                          100
                        ).toFixed()
                      )
                    "
                    :line-height="15"
                  />
                </div>
                <div class="w-2" style="line-height: 15px">
                    {{
                      slotProps.data.err_work
                        ? slotProps.data.err_work.length
                        : ""
                    }}
                  </div>
              </template></Column
            >
            <Column
              headerStyle="justify-content: center;text-align:center;height:50px; width:100px"
              bodyStyle="justify-content: center;text-align:center; width:100px"
              field="err_work"
              header="Chi tiết"
            >
              <template #body="slotProps">
                <!-- <div class="w-10 pt-2">
                    pi-info-circle
                   </div> -->
                <div class="format-center">
                  <Button
                    @click="onShowDateChart(slotProps.data, true)"
                    icon="pi pi-info-circle"
                    class="p-button-rounded p-button-info"
                  />
                </div> </template
            ></Column>
          </DataTable>
        </div>
      </div>
      <div v-else-if="typeChartMain == 1">
        <div v-if="typeChart == 1" class="d-lang-table relative">
          <div>
            <Chart
              type="bar"
              height="100"
              :data="multiAxisDataMain"
              :options="multiAxisMainOptions"
            />
          </div>
        </div>
      </div>
      <div v-else-if="typeChartMain == 2">
        <div v-if="typeChart == 1" class="d-lang-table">
          <div>
            <Chart
              height="100"
              type="line"
              :data="basicDataLine"
              :options="basicOptionsLine"
            />
          </div>
        </div>
      </div>
      <div v-else-if="typeChartMain == 3">
        <div v-if="typeChart == 1" class="d-lang-table">
          <div class="format-center">
            <Chart
              type="pie"
              :style="
                typeSidebar == 'width:70% !important'
                  ? 'width: 50% !important'
                  : 'width: 30% !important'
              "
              :data="chartDatapie"
              :options="lightOptions"
            />
          </div>
        </div>
      </div>
      <div v-if="typeChart == 2" class="d-lang-table">
        <div>
          <Chart
            type="bar"
            height="100"
            :data="multiAxisDataChild"
            :options="multiAxisOptions"
          />
        </div>
      </div>
      <div class="w-full flex mt-2 pr-4">
        <div class="w-2 text-left font-bold text-lg format-center">
          Tổng cộng:
        </div>
        <div class="px-2 w-2 text-center">
          <Button
            class="
              w-full
              font-bold
              justify-content-center
              p-button-outlined p-button-secondary
            "
            @click="onChartNum"
            >{{
              datalists.totalRecords ? datalists.totalRecords.length : 0
            }}</Button
          >
        </div>

        <div class="px-2 w-2 text-center">
          <Button
            @click="onChartNum"
            class="
              w-full
              font-bold
              justify-content-center
              p-button-outlined p-button-success
            "
            >{{
              datalists.finishedRecord ? datalists.finishedRecord.length : 0
            }}</Button
          >
        </div>
        <div class="px-2 w-2 text-center">
          <Button
            @click="onChartNum"
            class="
              w-full
              font-bold
              justify-content-center
              p-button-outlined p-button-help
            "
            >{{
              datalists.waitedRecord ? datalists.waitedRecord.length : 0
            }}</Button
          >
        </div>
        <div class="px-2 w-2 text-center">
          <Button
            @click="onChartNum"
            class="w-full justify-content-center p-button-outlined font-bold"
            >{{
              datalists.unFinishRecord ? datalists.unFinishRecord.length : 0
            }}</Button
          >
        </div>
        <div class="px-2 w-2 text-center">
          <Button
            @click="onChartNum"
            class="
              w-full
              font-bold
              justify-content-center
              p-button-outlined p-button-warning
            "
            >{{ datalists.tempClose ? datalists.tempClose.length : 0 }}</Button
          >
        </div>
        <div class="px-2 w-2 text-center">
          <Button
            @click="onChartNum"
            class="
              w-full
              font-bold
              justify-content-center
              p-button-outlined p-button-danger
            "
            >{{ datalists.err_work ? datalists.err_work.length : 0 }}</Button
          >
        </div>
      </div>
      <div class="w-full flex mt-2 pr-4">
        <div class="w-2 text-left font-bold text-lg format-center">
          Biểu đồ:
        </div>
        <div class="px-2 w-2 text-center">
          <Button
            @click="changeTypeChart(datalists.totalRecords, 1)"
            icon="pi 
pi-chart-bar"
            class="w-full justify-content-center p-button-secondary"
          ></Button>
        </div>

        <div class="px-2 w-2 text-center">
          <Button
            @click="changeTypeChart(datalists.finishedRecord, 3)"
            icon="pi 
pi-chart-bar"
            class="w-full justify-content-center p-button-success"
          ></Button>
        </div>
        <div class="px-2 w-2 text-center">
          <Button
            @click="changeTypeChart(datalists.waitedRecord, 2)"
            icon="pi 
pi-chart-bar"
            class="w-full justify-content-center p-button-help"
          ></Button>
        </div>
        <div class="px-2 w-2 text-center">
          <Button
            @click="changeTypeChart(datalists.unFinishRecord, 4)"
            icon="pi 
pi-chart-bar"
            class="w-full justify-content-center"
          ></Button>
        </div>
        <div class="px-2 w-2 text-center">
          <Button
            @click="changeTypeChart(datalists.tempClose, 5)"
            icon="pi 
pi-chart-bar"
            class="w-full justify-content-center p-button-warning"
          ></Button>
        </div>
        <div class="px-2 w-2 text-center">
          <Button
            @click="changeTypeChart(datalists.err_work, 6)"
            icon="
pi pi-chart-bar"
            class="w-full justify-content-center p-button-danger"
          ></Button>
        </div>
      </div>
    </div>
    <div v-if="userChart == 2">
      <div v-if="typeChartMain == 1">
        <div v-if="typeChart == 1" class="d-lang-table relative">
          <div>
            <Chart
              type="bar"
              height="100"
              :data="multiAxisDataDefaul"
              :options="multiAxisMainOptions"
            />
          </div>
        </div>
      </div>
      <div v-else-if="typeChartMain == 2">
        <div v-if="typeChart == 1" class="d-lang-table">
          <div>
            <Chart
              height="100"
              type="line"
              :data="basicDataLineDefauld"
              :options="basicOptionsLine"
            />
          </div>
        </div>
      </div>
      <div v-else-if="typeChartMain == 3">
        <div v-if="typeChart == 1" class="d-lang-table">
          <div class="format-center">
            <Chart
              type="pie"
              :style="
                typeSidebar == 'width:70% !important'
                  ? 'width: 50% !important'
                  : 'width: 10% !important'
              "
              :data="chartDatapie"
              :options="lightOptions"
            />
          </div>
        </div>
      </div>
      <div v-if="typeChart == 2" class="d-lang-table">
        <div>
          <Chart
            type="bar"
            height="100"
            :data="multiAxisDataChild"
            :options="multiAxisOptions"
          />
        </div>
      </div>
      <div class="w-full flex mt-2 pr-4">
        <div class="w-2 text-left font-bold text-lg format-center">
          Tổng cộng:
        </div>
        <div class="px-2 w-2 text-center">
          <Button
            class="
              w-full
              font-bold
              justify-content-center
              p-button-outlined p-button-secondary
            "
            @click="onChartNum"
            >{{
              datalists.totalRecords ? datalists.totalRecords.length : 0
            }}</Button
          >
        </div>

        <div class="px-2 w-2 text-center">
          <Button
            @click="onChartNum"
            class="
              w-full
              font-bold
              justify-content-center
              p-button-outlined p-button-success
            "
            >{{
              datalists.finishedRecord ? datalists.finishedRecord.length : 0
            }}</Button
          >
        </div>
        <div class="px-2 w-2 text-center">
          <Button
            @click="onChartNum"
            class="
              w-full
              font-bold
              justify-content-center
              p-button-outlined p-button-help
            "
            >{{
              datalists.waitedRecord ? datalists.waitedRecord.length : 0
            }}</Button
          >
        </div>
        <div class="px-2 w-2 text-center">
          <Button
            @click="onChartNum"
            class="w-full justify-content-center p-button-outlined font-bold"
            >{{
              datalists.unFinishRecord ? datalists.unFinishRecord.length : 0
            }}</Button
          >
        </div>
        <div class="px-2 w-2 text-center">
          <Button
            @click="onChartNum"
            class="
              w-full
              font-bold
              justify-content-center
              p-button-outlined p-button-warning
            "
            >{{ datalists.tempClose ? datalists.tempClose.length : 0 }}</Button
          >
        </div>
        <div class="px-2 w-2 text-center">
          <Button
            @click="onChartNum"
            class="
              w-full
              font-bold
              justify-content-center
              p-button-outlined p-button-danger
            "
            >{{ datalists.err_work ? datalists.err_work.length : 0 }}</Button
          >
        </div>
      </div>
      <div class="w-full flex mt-2 pr-4">
        <div class="w-2 text-left font-bold text-lg format-center">
          Biểu đồ:
        </div>
        <div class="px-2 w-2 text-center">
          <Button
            @click="changeTypeChart(datalists.totalRecords, 1)"
            icon="pi 
pi-chart-bar"
            class="w-full justify-content-center p-button-secondary"
          ></Button>
        </div>

        <div class="px-2 w-2 text-center">
          <Button
            @click="changeTypeChart(datalists.finishedRecord, 3)"
            icon="pi 
pi-chart-bar"
            class="w-full justify-content-center p-button-success"
          ></Button>
        </div>
        <div class="px-2 w-2 text-center">
          <Button
            @click="changeTypeChart(datalists.waitedRecord, 2)"
            icon="pi 
pi-chart-bar"
            class="w-full justify-content-center p-button-help"
          ></Button>
        </div>
        <div class="px-2 w-2 text-center">
          <Button
            @click="changeTypeChart(datalists.unFinishRecord, 4)"
            icon="pi 
pi-chart-bar"
            class="w-full justify-content-center"
          ></Button>
        </div>
        <div class="px-2 w-2 text-center">
          <Button
            @click="changeTypeChart(datalists.tempClose, 5)"
            icon="pi 
pi-chart-bar"
            class="w-full justify-content-center p-button-warning"
          ></Button>
        </div>
        <div class="px-2 w-2 text-center">
          <Button
            @click="changeTypeChart(datalists.err_work, 6)"
            icon="
pi pi-chart-bar"
            class="w-full justify-content-center p-button-danger"
          ></Button>
        </div>
      </div>
    </div>
  </Sidebar>
  <OverlayPanel
    ref="filterMonth"
    appendTo="body"
    class="p-0 m-0"
    :showCloseIcon="false"
    id="overlay_panelMonth"
    style="width: 300px"
  >
    <div class="grid formgrid m-0">
      <div class="flex field col-12 p-0">
        <Datepicker
          @closed="onFilterMonth(true)"
          selectText="Thực hiện"
          cancelText="Hủy"
          class="w-full"
          locale="vi"
          placeholder=" Lọc theo tuần"
          v-model="weekPickerFilter"
          weekPicker
        >
          <template #clear-icon>
            <Button
              @click="onCleanFilterMonth"
              icon="pi pi-times"
              class="p-button-rounded p-button-text"
            />
          </template>
          <template #input-icon>
            <Button class="mr-2 p-button-text" icon="pi pi-calendar" />
          </template>
        </Datepicker>
      </div>

      <div class="flex field col-12 p-0">
        <Datepicker
          @closed="onFilterMonth(false)"
          class="w-full"
          locale="vi"
          selectText="Thực hiện"
          cancelText="Hủy"
          placeholder=" Lọc theo tháng"
          v-model="monthPickerFilter"
          monthPicker
          ><template #clear-icon>
            <Button
              @click="onCleanFilterMonth"
              icon="pi pi-times"
              class="p-button-rounded p-button-text"
            />
          </template>
          <template #input-icon>
            <Button icon="pi pi-calendar" class="p-button-text" />
          </template>
        </Datepicker>
      </div>
    </div>
  </OverlayPanel>
  <Dialog
    :header="headerDateChart"
    v-model:visible="displayDateChart"
    :style="{ width: '50vw' }"
    :closable="true"
  >
    <form>
      <div>
        <Chart
          type="bar"
          :height="100"
          :data="multiAxisDateChart"
          :options="multiAxisMainOptions"
        />
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="closeDialog"
        class="p-button-text"
      />
    </template>
  </Dialog>
</template>
<style scoped>
.format-center {
  display: flex;
  justify-content: center;
  align-items: center;
  vertical-align: middle;
  text-align: center;
}
.d-lang-table {
  height: calc(100vh - 300px);
}
.p-column-header-content {
  justify-content: center !important;
}
</style>
<style lang="scss" scoped>
::v-deep(.p-datatable-table) {
  .p-datatable-thead .p-column-header-content {
    justify-content: center !important;
  }
}
</style>