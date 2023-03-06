<script setup>
import { ref, inject, onMounted, onBeforeUnmount, watch } from "vue";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
import { required } from "@vuelidate/validators";
import { encr } from "../../util/function.js";
import moment from "moment";
import TaskOrigin from "../../views/task_origin/TaskOrigin.vue";

const cryoptojs = inject("cryptojs");
const first = ref(0);
const toast = useToast();
const swal = inject("$swal");
const store = inject("store");
const axios = inject("axios"); // inject axios
const emitter = inject("emitter");
const basedomainURL = fileURL;

const countTasks = ref(0);
const countMembers = ref(0);
const countComments = ref(0);
const countAllFile = ref(0);

const opition = ref({
    type_chart : 1,
    PageNo: 0,
    PageSize: 20,
    sort: "created_date",
    ob: "DESC",
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

const headerAddDiscuss = ref();
const displayDiscuss = ref(false);
const listDiscussProjects = ref();
const issaveDiscuss = ref(false);
const listDropdownMembers = ref([]);
const submitted = ref(false);

const rules_discuss = {
  discuss_project_content: {
    required,
    $errors: [
      {
        $property: "discuss_project_content",
        $validator: "required",
        $message: "Nội dung thảo luận không được để trống!",
      },
    ],
  },
};
const discussProject = ref({
  discuss_project_id: null,
  project_id: props.id,
  discuss_project_content: null,
  is_public: true,
  start_date: new Date(),
  end_date: null,
  is_order: null,
  members: [],
})
const v3$ = useVuelidate(rules_discuss, discussProject);

const listStatusProjectMain = ref([
    {value: 0, text: "Chưa bắt đầu" },
    {value: 1, text: "Đang thực hiện" },
    {value: 2, text: "Đã hoàn thành" },
    {value: 3, text: "Tạm dừng" },
    {value: 4, text: "Đóng" },
]);

const props = defineProps({
  isShow: Boolean,
  id: String,
  turn: Intl,
});

const ProjectMainDetail = ref({});
const ProjectMainMembers = ref();
const ProjectMainParticipants = ref();
const Thanhviens = ref([]);
const ThanhvienShows = ref([]);
const PhongBanThamGia = ref();
const chartDataPie = ref();
const chartData = ref();
const listChartDate = ref();
const listProjectMainChild = ref([]);
const listProjectMainLogs = ref([]);
const checkListProjectMainLogs = ref([]);
const selectedProjectMains = ref();
const listProjectMainFile = ref([]);
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

const listDropdownStatusProject = ref([
  {
    value: 0,
    text: "Chưa bắt đầu",
    bg_color: "#bbbbbb",
    text_color: "#FFFFFF",
  },
  {
    value: 1,
    text: "Đang thực hiện",
    bg_color: "#2196f3",
    text_color: "#FFFFFF",
  },
  {
    value: 2,
    text: "Đã hoàn thành",
    bg_color: "#04D215",
    text_color: "#FFFFFF",
  },
  { value: 3, text: "Tạm dừng", bg_color: "#d87777", text_color: "#FFFFFF" },
  { value: 4, text: "Đóng", bg_color: "red", text_color: "#FFFFFF" },
]);

const optionsChartPie = {
  responsive: true,
  plugins: {
    title: {
      display: true,
      position: "bottom",
      text: "Thống kê công việc thuộc dự án",
    },
    legend: {
      position: "bottom",
    },
  },
};

const optionsChart = {
    responsive: true,
  plugins: {
    title: {
      display: true,
      position: "bottom",
      text: opition.value.title_type_chart,
    },
    legend: {
      position: "bottom",
    },
  },
};
const addLog = (log) => {
  axios.post(baseURL + "/api/Proc/AddLog", log, config);
};

const groupBy = (list, props) => {
  return list.reduce((a, b) => {
    (a[b[props]] = a[b[props]] || []).push(b);
    return a;
  }, {});
};

const ChangeTypeChart = (type) => {
    opition.value.type_chart = type;
    let chart2 = {
        labels: [""],
        datasets: [],
      };
    if(opition.value.type_chart == 1){
            var list = listChartDate.value.filter(x=>x.is_type == 1 || x.is_type == 2 || x.is_type == 3);
        }else if(opition.value.type_chart == 2){
            var list = listChartDate.value.filter(x=>x.is_type == 1 || x.is_type == 2);
        }else if(opition.value.type_chart == 3){
            var list = listChartDate.value.filter(x=>x.is_type == 0);
        }
        var listCV = groupBy(list, "user_id");
            var arrNewChart = [];
            for (let k in listCV) {
                var CVGroup = [];
                listCV[k].forEach(function (r) {
                    CVGroup.push(r);
                });
                arrNewChart.push({
                    user_id: k,
                    user_name: listCV[k].filter((x) => x.user_id == k)[0].last_name,
                    // status_bg_color: listDropdownStatus.value.filter((x) => x.value == k)[0].bg_color,
                    CVGroup: CVGroup,
                    });
            }
            arrNewChart.forEach(function(t,i){
                chart2.datasets.push({
                label: t.user_name,
                backgroundColor: bgColor.value[i % 7],
                hoverBackgroundColor: bgColor.value[i % 7],
                data: [t.CVGroup.length],
                });
            })
            chartData.value = chart2;
}

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
            proc: "project_main_get_detail",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "project_id", va: props.id },
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
      ProjectMainDetail.value = data[0][0];
      ProjectMainDetail.value.status_name = listStatusProjectMain.value.filter(x=>x.value == data[0][0].status)[0].text;
      if(data[5].length > 0){
        Thanhviens.value = new Set(data[5]);
        listDropdownMembers.value = data[5].map((x) => ({
          name: x.full_name,
          code: x.user_id,
          avatar: x.avatar,
          ten: x.last_name,
        }));
        if(data[5].length > 5) {
          ThanhvienShows.value = data[5].slice(0, 5);
        } else {
          ThanhvienShows.value = [...data[5]];
        }
      }
      ProjectMainMembers.value = data[5].filter(x=>x.is_type == 0);
      ProjectMainParticipants.value = data[5].filter(x=>x.is_type == 1);
      PhongBanThamGia.value = data[2];
      PhongBanThamGia.value.forEach((pb) => {
        pb.tenToChuc = data[1].filter(x=>x.department_id == pb.department_id)[0].tenToChuc ? data[1].filter(x=>x.department_id == pb.department_id)[0].tenToChuc : '';
      })
        //biểu đồ tròn trạng thái công việc
        let chart1 = {
        labels: [],
        datasets: [
          {
            data: [],
            backgroundColor: [],
            hoverBackgroundColor: [],
          },
        ],
      };

      let chart2 = {
        labels: [""],
        datasets: [],
      };
      
      if(data[3].length > 0) {
        countTasks.value = data[3].length;
        let listCV = groupBy(data[3], "status");
        var arrNew = [];
        for (let k in listCV) {
            var CVGroup = [];
            listCV[k].forEach(function (r) {
                CVGroup.push(r);
            });
            arrNew.push({
                status: k,
                status_name: listDropdownStatus.value.filter((x) => x.value == k)[0].text,
                status_bg_color: listDropdownStatus.value.filter((x) => x.value == k)[0].bg_color,
                CVGroup: CVGroup,
                });
        }
        arrNew.forEach(function(t){
            chart1.labels.push(t.status_name);
            chart1.datasets[0].data.push(t.CVGroup.length);
            chart1.datasets[0].backgroundColor.push(t.status_bg_color);
            chart1.datasets[0].hoverBackgroundColor.push(t.status_bg_color);
        })
        chartDataPie.value = chart1;
      }
      listChartDate.value = data[4];
        if(opition.value.type_chart == 1){
            var list = data[4].filter(x=>x.is_type == 1 || x.is_type == 2 || x.is_type == 3);
            opition.value.title_type_chart = "Công việc tham gia";
        }else if(opition.value.type_chart == 2){
            var list = data[4].filter(x=>x.is_type == 1 || x.is_type == 2);
            opition.value.title_type_chart = "Công việc thực hiện";
        }else if(opition.value.type_chart == 3){
            var list = data[4].filter(x=>x.is_type == 0);
            opition.value.title_type_chart = "Công việc quản lý";
        }
        var listCV = groupBy(list, "user_id");
            var arrNewChart = [];
            for (let k in listCV) {
                var CVGroup = [];
                listCV[k].forEach(function (r) {
                    CVGroup.push(r);
                });
                arrNewChart.push({
                    user_id: k,
                    user_name: listCV[k].filter((x) => x.user_id == k)[0].last_name,
                    CVGroup: CVGroup,
                    });
            }
            arrNewChart.forEach(function(t,i){
                chart2.datasets.push({
                label: t.user_name,
                backgroundColor: bgColor.value[i % 7],
                hoverBackgroundColor: bgColor.value[i % 7],
                data: [t.CVGroup.length],
                });
            })
            chartData.value = chart2;
            // listProjectMainChild.value = data[5];
      if (rf) {
        opition.value.loading = false;
        swal.close();
      }
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      opition.value.loading = false;
      addLog({
        title: "Lỗi Console loadData",
        controller: "LogsView.vue",
        log_content: error.message,
        loai: 2,
      });
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
}

const loadProjectMainChild = () => {
    axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "project_main_get_list_child",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "project_id", va: props.id },
              { par: "pageno", va: opition.value.PageNo },
              { par: "pagesize", va: opition.value.PageSize },
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
      listProjectMainChild.value = data[0];
      if(listProjectMainChild.value.length > 0){
        listProjectMainChild.value.forEach((element, i) => {
          element.status_name = listDropdownStatusProject.value.filter(
            (x) => x.value == element.status,
          )[0].text;
          element.status_bg_color = listDropdownStatusProject.value.filter(
            (x) => x.value == element.status,
          )[0].bg_color;
          element.status_text_color = listDropdownStatusProject.value.filter(
            (x) => x.value == element.status,
          )[0].text_color;
          element.STT = opition.value.PageNo * opition.value.PageSize + i + 1;
        });
      }
      opition.value.totalRecordProjectMainChilds = data[1][0].total;
    })
    .catch((error) => {
        debugger
      toast.error("Tải dữ liệu không thành công!");
      opition.value.loading = false;
      addLog({
        title: "Lỗi Console loadData",
        controller: "LogsView.vue",
        log_content: error.message,
        loai: 2,
      });
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
}

const onPage = (event) => {
  if (event.rows != opition.value.PageSize) {
    opition.value.PageSize = event.rows;
  }

  if (event.page == 0) {
    //Trang đầu
    opition.value.id = null;
    opition.value.IsNext = true;
  } else if (event.page > opition.value.PageNo + 1) {
    //Trang cuối
    opition.value.id = -1;
    opition.value.IsNext = false;
  } else if (event.page > opition.value.PageNo) {
    //Trang sau

    opition.value.id =
    listProjectMainChild.value[listProjectMainChild.value.length - 1].project_id;
    opition.value.IsNext = true;
  } else if (event.page < opition.value.PageNo) {
    //Trang trước
    opition.value.id = listProjectMainChild.value[0].project_id;
    opition.value.IsNext = false;
  }
  opition.value.PageNo = event.page;
  loadProjectMainChild();
};

const loadTaskLogProjetc = () => {
    axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "project_main_get_list_log",
            par: [
                { par: "project_id", va: props.id },
                { par: "ob", va: opition.value.ob },
            ],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
        let data = JSON.parse(response.data.data)[0];
        var weekday = new Array(7);
        weekday[0] = "Chủ nhật";
        weekday[1] = "Thứ 2";
        weekday[2] = "Thứ 3";
        weekday[3] = "Thứ 4";
        weekday[4] = "Thứ 5";
        weekday[5] = "Thứ 6";
        weekday[6] = "Thứ 7";
        data.forEach(function (r) {
            r.created_date = new Date(r.created_date);
            r.DaysName = weekday[r.created_date.getDay()];
            r.Time = r.DaysName + " (" + r.Ngay + ")";
        });
        groupByLog(data, "Time");
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      opition.value.loading = false;
      addLog({
        title: "Lỗi Console loadData",
        controller: "LogsView.vue",
        log_content: error.message,
        loai: 2,
      });
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
}

const groupByLog = (array, key) => {
  const result = {};
  array.forEach((item) => {
    if (!result[item[key]]) {
      result[item[key]] = [];
    }
    result[item[key]].push(item);
  });
  checkListProjectMainLogs.value = array;
  listProjectMainLogs.value = result;
};

const ChangeSort = () => {
  if (opition.value.ob == "DESC") {
    opition.value.ob = "ASC";
  } else {
    opition.value.ob = "DESC";
  }
  loadTaskLogProjetc();
};

const loadFileTaiLieu = () => {
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "project_main_get_list_file",
            par: [
                { par: "project_id", va: props.id },
                { par: "ob", va: opition.value.ob },
            ],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
        let data = JSON.parse(response.data.data)[0];
        listProjectMainFile.value = data;
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      opition.value.loading = false;
      addLog({
        title: "Lỗi Console loadData",
        controller: "LogsView.vue",
        log_content: error.message,
        loai: 2,
      });
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
}

const ChangeTab = (event) => {
    switch(event){
        case 0:
            //Thông tin chung
            loadData(false);
            break;
        case 1:
            // dự án con
            loadProjectMainChild();
            break;
        case 2:
            //giai đoạn
            loadProjectMainChild();
            break;
        case 3:
            //Công việc
            loadProjectMainChild();
            break;
        case 4:
            //Tàil iệu
            loadFileTaiLieu();
            break;
        case 5:
            //Thảo luận
            loadFileTaiLieu();
            break;
        case 6:
            //Lịch
            loadProjectMainChild();
            break;
        case 7:
            //Báo cáo
            loadProjectMainChild();
            break;
        case 8:
            //hoạt động
            loadTaskLogProjetc();
            break;
    }
}

const addDiscuss = (str) => {
  submitted.value = false;
  discussProject.value = {
    discuss_project_id: null,
    project_id: props.id,
    discuss_project_content: null,
    is_public: true,
    start_date: new Date(),
    end_date: null,
    is_order: null,
    members: [],
  };
  issaveDiscuss.value = false;
  headerAddDiscuss.value = str;
  displayDiscuss.value = true;
}

const saveDiscussProjectMain = (isFormValid) => {
  debugger
  submitted.value = true;
  if (!isFormValid) {
    return;
  }
  let formData = new FormData();
  formData.append("discussProject", JSON.stringify(discussProject.value));
  formData.append("discussMember", JSON.stringify(discussProject.value.members));
  axios
      .post(
        baseURL +
        "/api/ProjectMain/Add_DiscussProjectMain",
        formData,
        config,
      )
      .then((response) => {
        debugger
        if (response.data.err != "1") {
          swal.close();
          toast.success("Cập nhật thảo luận dự án thành công!");
          closeDialogProjectMain();
        } else {
          swal.fire({
            title: "Error!",
            text: response.data.ms,
            icon: "error",
            confirmButtonText: "OK",
          });
        }
      })
      .catch(() => {
        swal.close();
        swal.fire({
          title: "Error!",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
      });
}

onMounted(() => {
    ChangeTab(0);
//   loadData(true);
  return {};
});
</script>
<template>
    <div class="overflow-hidden h-full w-full col-md-12 p-0 m-0 flex" style="height: 100%;">
        <div class="col-12 p-0 m-0" style="height: 100%;">
            <div class="row w-full px-0 mx-0" style="height: 100%;">
                <div class="col-12 p-0 m-0" style="height: 100%;">
                    <div style="position: absolute;top: 2px; right: 20px; z-index: 2;">
                        <AvatarGroup>
                            <div
                            v-for="(value, index) in ThanhvienShows"
                            :key="index"
                            >
                            <div>
                                <Avatar
                                v-tooltip.bottom="{
                                    value:
                                    value.fullName +
                                    '<br/>' +
                                    (value.tenChucVu || '') +
                                    '<br/>' +
                                    (value.tenToChuc || ''),
                                    escape: true,
                                }"
                                v-bind:label="
                                    value.avatar ? '' : (value.last_name ?? '').substring(0, 1)
                                "
                                v-bind:image="basedomainURL + value.avatar" style="
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
                            v-if="
                                Thanhviens.length - ThanhvienShows.length >
                                0
                            "
                            :label="
                                '+' +
                                (Thanhviens.length -
                                ThanhvienShows.length) +
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
                    <TabView ref="tabview1" @tab-change="ChangeTab($event.index)"  style="height: 100%;">
                        <TabPanel header="Thông tin chung">
                            <div class="tab-project-content h-full w-full col-md-12 p-0 m-0 flex">
                                <div class="col-6 p-0 m-0 tab-project-content-left">
                                    <div class="row">
                                        <div class="col-12" style="border-bottom: 1px solid #aaa; font-weight: 600;padding: 10px;"><i class="pi pi-chart-pie"></i> Thống kê chung</div>
                                        <div class="col-12">
                                            <div class="row flex">
                                                <div class="col-3 format-center pt-1">
                                                    <Button class="p-button-success py-0 w-full"
                                                    style="
                                                        font-weight: 500;
                                                        background-color: #59d05d !important;
                                                        font-size: 1rem !important;
                                                        box-shadow: 0px 1px 15px 1px rgb(69 65 78 / 8%);
                                                        height: 30px;
                                                        display: flex;
                                                        justify-content: center;
                                                    "
                                                    @click="GotoView('Checklist')"
                                                    >
                                                    <i class="pi pi-list px-0"></i>
                                                    <span class="pl-2">{{ countTasks }}</span>
                                                    <span class="pl-2"> Công việc </span>
                                                    </Button>
                                                </div>
                                                <div class="col-3 format-center pt-1">
                                                    <Button
                                                    class="p-button-success py-0 w-full"
                                                    style="
                                                        font-weight: 500;
                                                        background-color: #fbad4c !important;
                                                        font-size: 1rem !important;
                                                        box-shadow: 0px 1px 15px 1px rgb(69 65 78 / 8%);
                                                        height: 30px;
                                                        display: flex;
                                                        justify-content: center;
                                                    "
                                                    @click="GotoView('members')"
                                                    >
                                                    <i class="pi pi-user px-0"></i>
                                                    <span class="pl-2">{{ countMembers }}</span>
                                                    <span class="pl-2"> Tham Gia </span>
                                                    </Button>
                                                </div>
                                                <div class="col-3 format-center pt-1">
                                                    <Button
                                                    class="p-button-success py-0 w-full"
                                                    style="
                                                        font-weight: 500;
                                                        background-color: #ff646d !important;
                                                        font-size: 1rem !important;
                                                        box-shadow: 0px 1px 15px 1px rgb(69 65 78 / 8%);
                                                        height: 30px;
                                                        display: flex;
                                                        justify-content: center;
                                                    "
                                                    @click="GotoView('comments')"
                                                    >
                                                    <i class="pi pi-comments px-0"></i>
                                                    <span class="pl-2">{{ countComments }}</span>
                                                    <span class="pl-2"> Bình luận </span>
                                                    </Button>
                                                </div>
                                                <div class="col-3 format-center pt-1">
                                                    <Button
                                                    class="p-button-success py-0 w-full"
                                                    style="
                                                        font-weight: 500;
                                                        background-color: #1d62f0 !important;
                                                        font-size: 1rem !important;
                                                        box-shadow: 0px 1px 15px 1px rgb(69 65 78 / 8%);
                                                        height: 30px;
                                                        display: flex;
                                                        justify-content: center;
                                                    "
                                                    @click="GotoView('file')"
                                                    >
                                                    <i class="pi pi-file px-0"></i>
                                                    <span class="pl-2">{{ countAllFile }}</span>
                                                    <span class="pl-2"> Tài liệu </span>
                                                    </Button>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="col-12 scroll-outer" style="overflow: hidden auto;max-height: calc(100vh - 155px); min-height: calc(100vh - 155px);">
                                            <div class="row scroll-inner">
                                                <div class="col-12">
                                                    <div class="row flex">
                                                        <div class="col-3"></div>
                                                        <div class="col-6">
                                                            <Chart
                                                            type="doughnut"
                                                            :height="300"
                                                            :width="300"
                                                            :data="chartDataPie"
                                                            :options="optionsChartPie"
                                                            />
                                                        </div>
                                                        <div class="col-3"></div>
                                                    </div>
                                                </div>
                                                <div class="col-12">
                                                    <div class="row flex">
                                                        <ul class="style-li" style="display: flex; list-style: none;padding: 0px;">
                                                            <li @click="ChangeTypeChart(1)" style="border-radius: 5px 0px 0px 5px;" :class="{ active: opition.type_chart == 1 }">Công việc tham gia</li>
                                                            <li @click="ChangeTypeChart(2)" :class="{ active: opition.type_chart == 2 }">Công việc thực hiện</li>
                                                            <li @click="ChangeTypeChart(3)" style="border-radius: 0px 5px 5px 0px;" :class="{ active: opition.type_chart == 3 }">Công việc quản lý</li>
                                                        </ul>
                                                    </div>
                                                </div>
                                                <div class="col-12">
                                                    <Chart type="bar" :data="chartData" :options="optionsChart"/>
                                                </div>
                                                    </div>
                                              </div>
                                            </div>
                                        </div>
                                <div class="col-6 p-0 m-0 tab-project-content-right">
                                    <div class="row" style="padding: 15px; font-size: 13px;">
                                        <div class="col-12">Tên dự án: <span style="font-weight: 600;">{{ ProjectMainDetail.project_name }}</span></div>
                                        <div class="col-12">Tạo bởi: <span style="font-weight: 600;color:#2196F3;">{{ ProjectMainDetail.full_name }} - </span><span>{{ moment(new Date(ProjectMainDetail.created_date)).format("HH:mm DD/MM") }}</span></div>
                                        <div class="col-12" style="display: flex; flex-direction: column;">
                                            <span>Thành viên quản trị dự án</span>
                                            <div style="margin: 10px 0px 0px 10px;">
                                                <AvatarGroup>
                                                    <div
                                                        v-for="(value, index) in ProjectMainMembers"
                                                        :key="index"
                                                        >
                                                        <div>
                                                            <Avatar
                                                            v-tooltip.bottom="{
                                                                value:
                                                                value.full_name +
                                                                '<br/>' +
                                                                (value.tenChucVu || '') +
                                                                '<br/>' +
                                                                (value.tenToChuc || ''),
                                                                escape: true,
                                                            }"
                                                            v-bind:label="
                                                                value.avatar ? '' : (value.last_name ?? '').substring(0, 1)
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
                                                </AvatarGroup>
                                            </div>
                                        </div>
                                        <div class="col-12" style="display: flex; flex-direction: column;">
                                            <span>Thành viên tham gia dự án</span>
                                            <div style="margin: 10px 0px 0px 10px;">
                                                <AvatarGroup>
                                                    <div
                                                        v-for="(value, index) in ProjectMainParticipants"
                                                        :key="index"
                                                        >
                                                        <div>
                                                            <Avatar
                                                            v-tooltip.bottom="{
                                                                value:
                                                                value.full_name +
                                                                '<br/>' +
                                                                (value.tenChucVu || '') +
                                                                '<br/>' +
                                                                (value.tenToChuc || ''),
                                                                escape: true,
                                                            }"
                                                            v-bind:label="
                                                                value.avatar ? '' : (value.last_name ?? '').substring(0, 1)
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
                                                </AvatarGroup>
                                            </div>
                                        </div>
                                        <div class="col-12">Nhóm dự án: <span style="font-weight: 600;">{{ ProjectMainDetail.group_name }}</span></div>
                                        <div class="col-12">
                                            <div class="row flex">
                                                <div class="col-6 flex" style="flex-direction: column;padding: 0px;font-weight: 600;line-height: 25px;">
                                                    <span>Ngày bắt đầu</span>
                                                    <span v-if="ProjectMainDetail.start_date">{{ moment(new Date(ProjectMainDetail.start_date)).format("DD/MM/YYYY") }}</span>
                                                </div>
                                                <div class="col-6 flex" style="flex-direction: column;padding: 0px;font-weight: 600;line-height: 25px;">
                                                    <span>Ngày kết thúc</span>
                                                    <span v-if="ProjectMainDetail.end_date">{{ moment(new Date(ProjectMainDetail.end_date)).format("DD/MM/YYYY") }}</span>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="col-12">
                                            <div class="row flex">
                                                <div class="col-12 flex" style="flex-direction: column;padding: 0px;line-height: 25px;">
                                                    <span>Trạng thái dự án</span>
                                                    <span style="font-weight: 600;">{{ ProjectMainDetail.status_name }}</span>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="col-12">
                                            <div class="row flex">
                                                <div class="col-12 flex" style="flex-direction: column;padding: 0px;line-height: 25px;">
                                                    <span>Phòng ban tham gia</span>
                                                    <span v-for="pb in PhongBanThamGia" style="font-weight: 600;">- {{ pb.organization_name }}  <span v-if="pb.tenToChuc" style="font-weight: 500;">({{ pb.tenToChuc }})</span></span>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </TabPanel>
                        <TabPanel header="Dự án con">
                            <div class="h-full w-full col-md-12 p-0 m-0">
                                <DataTable
                                id="projectmain-child"
                                v-model:first="first"
                                :rowHover="true"
                                :value="listProjectMainChild"
                                :paginator="true"
                                :rows="opition.PageSize"
                                paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
                                :rowsPerPageOptions="[1,20, 30, 50, 100, 200]"
                                :scrollable="true"
                                scrollHeight="flex"
                                :totalRecords="opition.totalRecordProjectMainChilds"
                                :row-hover="true"
                                dataKey="project_id"
                                v-model:selection="selectedProjectMains"
                                @page="onPage($event)"
                                @sort="onSort($event)"
                                @filter="onFilter($event)"
                                :lazy="true"
                                selectionMode="single"
                                >
                                <Column field="STT" header="STT"
                                    class="align-items-center justify-content-center text-center font-bold"
                                    headerStyle="text-align:center;max-width:4rem" bodyStyle="text-align:center;max-width:4rem">
                                </Column>
                                <Column field="Logo" header="Logo" class="align-items-center justify-content-center text-center"
                                    headerStyle="text-align:center;max-width:80px" bodyStyle="text-align:center;max-width:80px">
                                    <template #body="md">
                                    <Avatar v-if="md.data.logo" :image="basedomainURL + md.data.logo" class="mr-2" size="large" />
                                    </template>
                                </Column>
                                <Column field="project_name" header="Tên dự án" headerStyle="max-width:auto;">
                                    <template #body="md">
                                    <div style="display: flex; align-items: center">
                                        <span style="margin-left: 5px">{{
                                        md.data.project_name
                                        }}</span>
                                    </div>
                                    </template>
                                </Column>
                                <Column field="project_code" header="Mã dự án" class="align-items-center justify-content-center text-center"
                                    headerStyle="max-width:100px;text-align:center;" bodyStyle="max-width:100px;text-align:center;">
                                </Column>
                                <Column field="group_name" header="Nhóm dự án" class="align-items-center justify-content-center text-center"
                                    headerStyle="max-width:300px;text-align:center;" bodyStyle="max-width:300px;text-align:center;">
                                </Column>
                                <Column field="status" header="Trạng thái" class="align-items-center justify-content-center text-center"
                                    headerStyle="text-align:center;max-width:120px" bodyStyle="text-align:center;max-width:120px">
                                    <template #body="md">
                                    <Chip :style="{
                                        background: md.data.status_bg_color,
                                        color: md.data.status_text_color,
                                    }" v-bind:label="md.data.status_name" />
                                    </template>
                                </Column>
                                <template #empty>
                                    <div
                                    class="align-items-center justify-content-center p-4 text-center m-auto" style="
                                        min-height: calc(100vh - 215px);
                                        max-height: calc(100vh - 215px);
                                        display: flex;
                                        flex-direction: column;
                                    "
                                    v-if="listProjectMainChild != null"
                                    >
                                    <img
                                        src="../../assets/background/nodata.png"
                                        height="144"
                                    />
                                    <h3 class="m-1">Không có dữ liệu</h3>
                                    </div>
                                </template>
                                </DataTable>
                            </div>
                        </TabPanel>
                        <TabPanel header="Công việc">
                          <TaskOrigin
                              :isShow="showDetail"
                              :id="props.id"
                              :turn="0"
                            >
                            </TaskOrigin>
                        </TabPanel>
                        <TabPanel header="Tài liệu">
                          <div class="h-full w-full col-md-12 p-0 m-0">
                                <div
                                    style="border-bottom: 1px solid #ccc; margin-right: 10px; height: 40px; padding-left: 10px;"
                                >
                                    <ul style="display: flex; padding: 0px; float: left">
                                    <li
                                        style="list-style: none; margin-right: 20px;font-weight: 600;"
                                    >
                                        <a style="display: flex; font-size: 15px; align-items: center;"
                                        >
                                        Danh sách file tài liệu dự án</a
                                        >
                                    </li>
                                    </ul>
                                </div>
                                <div
                                    class="col-12 p-0 m-0" style="
                                    max-height: calc(100vh - 110px);
                                    min-height: calc(100vh - 110px);
                                    overflow-y: auto;
                                    "
                                >
                                    <div>
                                        <ul class="project-active-list" style="margin: 0px; padding: 0px">
                                            <li
                                            style="list-style: none; padding: 5px 10px"
                                            v-for="(value, index) in listProjectMainFile"
                                            >
                                            <div style="display: flex; align-items: center;">
                                                <div style="display: flex; flex: 1">
                                                  <!-- <Avatar
                                                    :key="index"
                                                    v-bind:label="
                                                    value.file_type
                                                        ? ''
                                                        : (value.last_name ?? '').substring(0, 1)
                                                    "
                                                    v-bind:image="basedomainURL + value.avatar"
                                                    style="
                                                    background-color: #2196f3;
                                                    color: #ffffff;
                                                    width: 48px;
                                                    height: 48px;
                                                    font-size: 15px !important;
                                                    "
                                                    :style="{
                                                    background: bgColor[index%7] + '!important',
                                                    }"
                                                    class="cursor-pointer"
                                                    size="xlarge"
                                                    shape="circle"
                                                /> -->
                                                  <div
                                                      style="
                                                      font-size: 13px;
                                                      display: flex;
                                                      flex-direction: column;
                                                      padding: 8px 20px;
                                                      "
                                                  >
                                                    <span style="flex: 1; font-weight: 500;">{{ value.file_name }}</span>
                                                  </div>
                                                </div>
                                                <div style="font-size: 11px; color: #aaa">
                                                <span style="font-weight: 500">{{
                                                    moment(new Date(value.created_date)).format(
                                                    "HH:mm DD/MM/YYYY",
                                                    )
                                                }}</span>
                                                </div>
                                            </div>
                                            </li>
                                            <div 
                                              class="align-items-center justify-content-center p-4 text-center m-auto" style="
                                                  min-height: calc(100vh - 215px);
                                                  max-height: calc(100vh - 215px);
                                                  display: flex;
                                                  flex-direction: column;
                                              "
                                              v-if="listProjectMainFile.length == 0"
                                              >
                                              <img
                                                  src="../../assets/background/nodata.png"
                                                  height="144"
                                              />
                                              <h3 class="m-1">Không có dữ liệu</h3>
                                              </div>
                                        </ul>
                                    </div>
                                </div>
                            </div>
                        </TabPanel>
                        <TabPanel header="Thảo luận">
                          <div class="tab-project-content h-full w-full col-md-12 p-0 m-0 flex">
                                <div class="col-6 p-0 m-0 tab-project-content-left">
                                    <div class="row">
                                        <div class="col-12" style="border-bottom: 1px solid #aaa; font-weight: 600;padding: 10px;">
                                          <Button label="Tạo thảo luận" icon="pi pi-plus" class="mr-2" @click="addDiscuss('Thêm mới thảo luận')" />
                                        </div>
                                        <div class="col-12">
                                          <DataTable
                                          id="projectmain-thaoluan"
                                          v-model:first="first"
                                          :rowHover="true"
                                          :value="listProjectMainChild"
                                          :paginator="true"
                                          :rows="opition.PageSize"
                                          paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
                                          :rowsPerPageOptions="[1,20, 30, 50, 100, 200]"
                                          :scrollable="true"
                                          scrollHeight="flex"
                                          :totalRecords="opition.totalRecordProjectMainChilds"
                                          :row-hover="true"
                                          dataKey="project_id"
                                          v-model:selection="selectedProjectMains"
                                          @page="onPage($event)"
                                          @sort="onSort($event)"
                                          @filter="onFilter($event)"
                                          :lazy="true"
                                          selectionMode="single"
                                          >
                                          <Column field="STT" header="STT"
                                              class="align-items-center justify-content-center text-center font-bold"
                                              headerStyle="text-align:center;max-width:4rem" bodyStyle="text-align:center;max-width:4rem">
                                          </Column>
                                          <Column field="" header="Người tạo" class="align-items-center justify-content-center text-center"
                                              headerStyle="text-align:center;max-width:100px" bodyStyle="text-align:center;max-width:100px">
                                              <template #body="md">
                                              <Avatar v-if="md.data.logo" :image="basedomainURL + md.data.logo" class="mr-2" size="large" />
                                              </template>
                                          </Column>
                                          <Column field="" header="Nội dung thảo luận" headerStyle="max-width:auto;">
                                              <template #body="md">
                                              <div style="display: flex; align-items: center">
                                                  <span style="margin-left: 5px">{{
                                                  md.data.project_name
                                                  }}</span>
                                              </div>
                                              </template>
                                          </Column>
                                          <Column field="status" header="" class="align-items-center justify-content-center text-center"
                                              headerStyle="text-align:center;max-width:120px" bodyStyle="text-align:center;max-width:120px">
                                              <template #body="md">
                                              <Chip :style="{
                                                  background: md.data.status_bg_color,
                                                  color: md.data.status_text_color,
                                              }" v-bind:label="md.data.status_name" />
                                              </template>
                                          </Column>
                                          <template #empty>
                                              <div
                                              class="align-items-center justify-content-center p-4 text-center m-auto" style="
                                                  min-height: calc(100vh - 215px);
                                                  max-height: calc(100vh - 215px);
                                                  display: flex;
                                                  flex-direction: column;
                                              "
                                              v-if="listProjectMainChild != null"
                                              >
                                              <img
                                                  src="../../assets/background/nodata.png"
                                                  height="144"
                                              />
                                              <h3 class="m-1">Không có dữ liệu</h3>
                                              </div>
                                          </template>
                                          </DataTable>
                                        </div>
                                    </div>
                                </div>
                                <div class="col-6 p-0 m-0 tab-project-content-right">
                                    <div class="row" style="padding: 15px; font-size: 13px;">
                                        <div class="col-12">Tên dự án: <span style="font-weight: 600;">{{ ProjectMainDetail.project_name }}</span></div>
                                        <div class="col-12">Tạo bởi: <span style="font-weight: 600;color:#2196F3;">{{ ProjectMainDetail.full_name }} - </span><span>{{ moment(new Date(ProjectMainDetail.created_date)).format("HH:mm DD/MM") }}</span></div>
                                        <div class="col-12" style="display: flex; flex-direction: column;">
                                            <span>Thành viên quản trị dự án</span>
                                            <div style="margin: 10px 0px 0px 10px;">
                                                <AvatarGroup>
                                                    <div
                                                        v-for="(value, index) in ProjectMainMembers"
                                                        :key="index"
                                                        >
                                                        <div>
                                                            <Avatar
                                                            v-tooltip.bottom="{
                                                                value:
                                                                value.full_name +
                                                                '<br/>' +
                                                                (value.tenChucVu || '') +
                                                                '<br/>' +
                                                                (value.tenToChuc || ''),
                                                                escape: true,
                                                            }"
                                                            v-bind:label="
                                                                value.avatar ? '' : (value.last_name ?? '').substring(0, 1)
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
                                                </AvatarGroup>
                                            </div>
                                        </div>
                                        <div class="col-12" style="display: flex; flex-direction: column;">
                                            <span>Thành viên tham gia dự án</span>
                                            <div style="margin: 10px 0px 0px 10px;">
                                                <AvatarGroup>
                                                    <div
                                                        v-for="(value, index) in ProjectMainParticipants"
                                                        :key="index"
                                                        >
                                                        <div>
                                                            <Avatar
                                                            v-tooltip.bottom="{
                                                                value:
                                                                value.full_name +
                                                                '<br/>' +
                                                                (value.tenChucVu || '') +
                                                                '<br/>' +
                                                                (value.tenToChuc || ''),
                                                                escape: true,
                                                            }"
                                                            v-bind:label="
                                                                value.avatar ? '' : (value.last_name ?? '').substring(0, 1)
                                                            "
                                                            v-bind:image="basedomainURL + value.avatar" style="
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
                                                </AvatarGroup>
                                            </div>
                                        </div>
                                        <div class="col-12">Nhóm dự án: <span style="font-weight: 600;">{{ ProjectMainDetail.group_name }}</span></div>
                                        <div class="col-12">
                                            <div class="row flex">
                                                <div class="col-6 flex" style="flex-direction: column;padding: 0px;font-weight: 600;line-height: 25px;">
                                                    <span>Ngày bắt đầu</span>
                                                    <span v-if="ProjectMainDetail.start_date">{{ moment(new Date(ProjectMainDetail.start_date)).format("DD/MM/YYYY") }}</span>
                                                </div>
                                                <div class="col-6 flex" style="flex-direction: column;padding: 0px;font-weight: 600;line-height: 25px;">
                                                    <span>Ngày kết thúc</span>
                                                    <span v-if="ProjectMainDetail.end_date">{{ moment(new Date(ProjectMainDetail.end_date)).format("DD/MM/YYYY") }}</span>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="col-12">
                                            <div class="row flex">
                                                <div class="col-12 flex" style="flex-direction: column;padding: 0px;line-height: 25px;">
                                                    <span>Trạng thái dự án</span>
                                                    <span style="font-weight: 600;">{{ ProjectMainDetail.status_name }}</span>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="col-12">
                                            <div class="row flex">
                                                <div class="col-12 flex" style="flex-direction: column;padding: 0px;line-height: 25px;">
                                                    <span>Phòng ban tham gia</span>
                                                    <span v-for="pb in PhongBanThamGia" style="font-weight: 600;">- {{ pb.organization_name }}  <span v-if="pb.tenToChuc" style="font-weight: 500;">({{ pb.tenToChuc }})</span></span>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </TabPanel>
                        <TabPanel header="Lịch">
                            <p>Lịch</p>
                        </TabPanel>
                        <TabPanel header="Báo cáo">
                            <p>Báo cáo</p>
                        </TabPanel>
                        <TabPanel header="Hoạt động">
                            <div class="h-full w-full col-md-12 p-0 m-0">
                                <div
                                    style="border-bottom: 1px solid #ccc; margin-right: 10px; height: 40px; padding-left: 10px;"
                                >
                                    <ul style="display: flex; padding: 0px; float: left">
                                    <li
                                        @click="addLinkTaskOrigin(datalists)"
                                        style="list-style: none; margin-right: 20px; color: #0d89ec"
                                    >
                                        <a style="display: flex; font-size: 15px; align-items: center;"
                                        ><i
                                            style="margin-right: 5px"
                                            class="p-custom pi pi-calendar-times"
                                        ></i>
                                        Hoạt động gần nhất</a
                                        >
                                    </li>
                                    </ul>
                                    <ul
                                    id="project-active-sort"
                                    style="display: flex; padding: 0px; float: right"
                                    >
                                    <li
                                        style="list-style: none; margin-right: 20px"
                                        @click="ChangeSort()"
                                        :class="{ active: opition.sort }"
                                        aria-haspopup="true"
                                        aria-controls="overlay_Export"
                                    >
                                        <a style="display: flex; font-size: 15px; align-items: center"
                                        ><i class="pi pi-sort"></i> Sắp xếp
                                        <i class="pi pi-angle-down"></i
                                        ></a>
                                    </li>
                                    </ul>
                                </div>
                                <div
                                    class="col-12 p-0 m-0" style="
                                    max-height: calc(100vh - 110px);
                                    min-height: calc(100vh - 110px);
                                    overflow-y: auto;
                                    "
                                >
                                    <div v-for="(group, groupName) in listProjectMainLogs">
                                        <span
                                            style="
                                            color: #0d89ec;
                                            font-size: 12px;
                                            border-bottom: 1px solid #ccc;
                                            display: block;
                                            padding: 10px;
                                            "
                                            >{{ groupName }}</span
                                        >
                                        <ul class="project-active-list" style="margin: 0px; padding: 0px">
                                            <li
                                            style="list-style: none; padding: 5px 10px"
                                            v-for="(value, index) in group"
                                            >
                                            <div style="display: flex">
                                                <div style="display: flex; flex: 1">
                                                <Avatar
                                                    :key="index"
                                                    v-tooltip.bottom="{
                                                    value:
                                                        value.full_name +
                                                        '<br/>' +
                                                        (value.tenChucVu || '') +
                                                        '<br/>' +
                                                        (value.tenToChuc || ''),
                                                    escape: true,
                                                    }"
                                                    v-bind:label="
                                                    value.avatar
                                                        ? ''
                                                        : (value.last_name ?? '').substring(0, 1)
                                                    "
                                                    v-bind:image="basedomainURL + value.avatar"
                                                    style="
                                                    background-color: #2196f3;
                                                    color: #ffffff;
                                                    width: 48px;
                                                    height: 48px;
                                                    font-size: 15px !important;
                                                    "
                                                    :style="{
                                                    background: bgColor[index%7] + '!important',
                                                    }"
                                                    class="cursor-pointer"
                                                    size="xlarge"
                                                    shape="circle"
                                                />
                                                <div
                                                    style="
                                                    font-size: 13px;
                                                    display: flex;
                                                    flex-direction: column;
                                                    padding: 8px 20px;
                                                    "
                                                >
                                                    <span style="flex: 1; font-weight: 600;">{{ value.full_name }}</span>
                                                    <span
                                                    style="font-weight: 500"
                                                    v-html="value.description"
                                                    ></span>
                                                </div>
                                                </div>
                                                <div style="font-size: 11px; color: #aaa">
                                                <span style="font-weight: 500">{{
                                                    moment(new Date(value.created_date)).format(
                                                    "HH:mm DD/MM/YYYY",
                                                    )
                                                }}</span>
                                                </div>
                                            </div>
                                            </li>
                                        </ul>
                                    </div>
                                    <div 
                                              class="align-items-center justify-content-center p-4 text-center m-auto" 
                                              style="
                                                  min-height: calc(100vh - 215px);
                                                  max-height: calc(100vh - 215px);
                                                  display: flex;
                                                  flex-direction: column;
                                              "
                                              v-if="checkListProjectMainLogs.length == 0"
                                              >
                                              <img
                                                  src="../../assets/background/nodata.png"
                                                  height="144"
                                              />
                                              <h3 class="m-1">Không có dữ liệu</h3>
                                     </div>
                                </div>
                            </div>
                        </TabPanel>
                    </TabView>
                </div>
            </div>
        </div>
    </div>

    <Dialog :header="headerAddDiscuss" v-model:visible="displayDiscuss" :style="{ width: '40vw' }"
    :closable="true" :maximizable="true">
                            <form>
                              <div class="grid formgrid m-2">
                                <div class="field col-12 md:col-12">
                                  <label class="col-3 text-left p-0">Nội dung thảo luận<span class="redsao"> (*) </span></label>
                                  <InputText v-model="discussProject.discuss_project_content" spellcheck="false" class="col-9 ip36 px-2" 
                                  :class="{ 'p-invalid': v3$.discuss_project_content.$invalid && submitted }"/>
                                </div>
                                <div style="display: flex" class="field col-12 md:col-12">
                                  <div class="col-3 text-left"></div>
                                  <small v-if="
                                    (v3$.discuss_project_content.$invalid && submitted) ||
                                    v3$.discuss_project_content.$pending.$response
                                  " class="col-9 p-error p-0">
                                    <span class="col-12 p-0">{{
                                      v3$.discuss_project_content.required.$message
                                        .replace("Value", "Nội dung thảo luận")
                                        .replace("is required", "không được để trống")
                                    }}</span>
                                  </small>
                                </div>
                                <div class="field col-12 md:col-12" style="display: flex; align-items: center">
                                  <label class="col-3 text-left p-0">Ngày bắt đầu</label>
                                  <div class="col-9" style="display: flex; padding: 0px; align-items: center">
                                    <Calendar :manualInput="true" :showIcon="true" class="col-5 ip36 title-lable"
                                      style="margin-top: 5px; padding: 0px" id="time1" autocomplete="on" v-model="discussProject.start_date" />
                                    <div class="col-7" style="display: flex; padding: 0px; align-items: center">
                                      <label class="col-5 text-center">Ngày kết thúc</label>
                                      <Calendar :manualInput="true" :showIcon="true" class="col-7 ip36 title-lable"
                                        style="margin-top: 5px; padding: 0px" id="time2" placeholder="dd/MM/yy" autocomplete="on"
                                        v-model="discussProject.end_date" @date-select="CheckDate($event)" />
                                    </div>
                                  </div>
                                </div>
                                <div class="field col-12 md:col-12">
                                  <label class="col-3 text-left p-0">STT</label>
                                  <InputNumber v-model="discussProject.is_order" style="padding: 0px !important" class="col-9 ip36 px-2" />
                                </div>
                                <div class="field col-12 md:col-12 flex">
                                  <label class="col-3 text-left p-0">Public thảo luận</label>
                                  <div class="col-9" style="position: relative;">
                                    <InputSwitch
                                      class="col-12"
                                      style="position: absolute; top: 0px; left: 0px"
                                      v-model="discussProject.is_public"
                                    />
                                  </div>
                                </div>
                                <div class="field col-12 md:col-12" v-if="discussProject.is_public == false">
                                  <label class="col-3 text-left p-0"
                                    >Người tham gia
                                    <span
                                      @click="OpenDialogTreeUser(false, 1)"
                                      class="choose-user"
                                      ><i class="pi pi-user-plus"></i></span
                                    ></label
                                  >
                                  <MultiSelect
                                    :filter="true"
                                    v-model="discussProject.members"
                                    :options="listDropdownMembers"
                                    optionValue="code"
                                    optionLabel="name"
                                    class="col-9 ip36 p-0"
                                    placeholder="Người tham gia"
                                    display="chip"
                                  >
                                    <template #option="slotProps">
                                      <div
                                        class="country-item flex"
                                        style="align-items: center; margin-left: 10px"
                                      >
                                        <Avatar
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
                                          {{ slotProps.option.name }}
                                        </div>
                                      </div>
                                    </template>
                                  </MultiSelect>
                                </div>
                              </div>
                            </form>
                            <template #footer>
                              <Button label="Hủy" icon="pi pi-times" @click="closeDialogProjectMain" class="p-button-text" />
                              <Button label="Lưu" icon="pi pi-check" @click="saveDiscussProjectMain(!v3$.discuss_project_content.$invalid)"/>
                            </template>
                          </Dialog> 
</template>
<style scoped>
#project-active-sort li:hover {
  cursor: pointer;
  color: #0d89ec;
}
.project-active-list li {
  border-bottom: 1px solid #ccc;
}
.project-active-list li:hover {
  cursor: pointer;
  background-color: #e5f3ff !important;
}
.tab-project-content{
    height: calc(100vh - 50px) !important;
    background-color: #f3f3f3;
}
.tab-project-content-left{
    background-color: #fff;
    margin: 5px 5px 0px 0px !important;
    height: 100%;
}
.tab-project-content-right{
    background-color: #fff;
    margin: 5px 0px 5px 5px !important;
    height: 100%;
}
.pl-2{
    padding-left: 0.5rem !important;
}
.style-li li{
    padding: 5px 10px;
    background-color: #e5e5e5;
    color: black;
}
.style-li li:hover{
    cursor: pointer;
    background-color: #33c9dc;
    color: #fff;
}
.style-li .active{
    background-color: #33c9dc !important;
    color: #fff !important;
}
.scroll-outer{
    visibility: hidden;
}
.scroll-inner{
    visibility: visible;
}
.scroll-inner, .scroll-outer:hover, .scroll-outer:focus{
    visibility: visible;
}
::-webkit-scrollbar{
    width: 17px !important;
}

#projectmain-child {
  height: 99% !important;
  padding: 10px;
}
#projectmain-thaoluan{
  max-height: calc(100vh - 110px);
  min-height: calc(100vh - 110px);
}

</style>
<style>
.p-tabview-panels .p-tabview-panel{
    height: 100%;
}
.p-sidebar  .p-sidebar-content{
    height: 100%;
    background-color: #f3f3f3;
}
.p-tabview .p-tabview-panels{
    height: 96%;
    padding: 0px !important;
    /* background-color: #f3f3f3; */
}
.main-layout{
  height: 100%;
}
</style>