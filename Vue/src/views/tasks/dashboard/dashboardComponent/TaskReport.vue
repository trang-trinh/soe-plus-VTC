<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { encr, decr } from "../../../../util/function.js";
import FilterTask from "./filterTask.vue";
import moment from "moment";
import DetailedWork from "../../../../components/task_origin/DetailedWork.vue";
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
  searchText: null,
  filterDateType: null,
  project_id: null,
  group_id: null,
  start_date: null,
  end_date: null,
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
const user = store.state.user;
const listTask = ref([]);
const first = ref(0);
const PositionSideBar = ref("right");
emitter.on("psb", (obj) => {
  PositionSideBar.value = obj;
});
const loadData = () => {
  options.value.loading = true;
  axios
    .post(
      // eslint-disable-next-line no-undef
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "task_origin_list_to_dashboard_report",
            par: [
              { par: "user_id", va: user.user_id },
              { par: "pn", va: options.value.PageNo },
              { par: "ps", va: options.value.PageSize },
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
      let count = JSON.parse(response.data.data)[1];
      data.forEach((x, i) => {
        x.creator = JSON.parse(x.creator);
        x.creator.tooltip =
          x.creator.full_name +
          "<br/>" +
          x.creator.position_name +
          "<br/>" +
          (x.creator.department_name || x.creator.organization_name);
        x.task_master = JSON.parse(x.task_master);
        x.task_master.tooltip =
          x.creator.full_name +
          "<br/>" +
          x.creator.position_name +
          "<br/>" +
          (x.creator.department_name || x.creator.organization_name);
        x.STT = options.value.PageNo * options.value.PageSize + i + 1;
        x.progress = x.progress ?? 0;
        let sttus = listStatus.value.filter((a) => a.value == x.status);
        x.status_display = {
          text: sttus[0].text,
          bg_color: sttus[0].bg_color,
          text_color: sttus[0].text_color,
        };
      });
      listTask.value = data;
      options.value.totalRecords = count[0].totalRecords;
      options.value.loading = false;
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
const onPage = (event) => {
  if (event.rows != options.value.PageSize) {
    options.value.PageSize = event.rows;
  }
  options.value.PageNo = event.page;
  loadData();
};
const selectedTasks = ref([]);
const refresh = () => {
  removeFilter();
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
  loadData();
};
const checkDelList = ref(false);
watch(selectedTasks, () => {
  if (selectedTasks.value.length > 0) {
    checkDelList.value = true;
  } else {
    checkDelList.value = false;
  }
});
const DialogVisible = ref(false);
const DialogFileVisible = ref(false);
const DialogMoreVisible = ref(false);
const listSelected = ref([]);
const openDialog = () => {
  DialogVisible.value = true;
  listSelected.value = [];
  listSelected.value = JSON.parse(JSON.stringify(selectedTasks.value));
  listSelected.value.forEach((x) => {
    x.difficult = "";
    x.request = "";
    x.reportProgress = 0;
    x.contents = "";
    x.is_send_email = false;
    x.file = [];
    x.file_display = [];
  });
};
const TempData = ref();
const openMore = (data) => {
  DialogMoreVisible.value = true;
  TempData.value = data;
};
const openFile = (data) => {
  DialogFileVisible.value = true;
  TempData.value = data;
};
const formatSize = (bytes) => {
  if (bytes === 0) {
    return "0 B";
  }

  let k = 1024,
    dm = 3,
    sizes = ["B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"],
    i = Math.floor(Math.log(bytes) / Math.log(k));
  return parseFloat((bytes / Math.pow(k, i)).toFixed(dm)) + " " + sizes[i];
};
let filecoments = [];
const Upload = () => {
  filecoments = [];
  filecoments = event.target.files;
  filecoments.forEach((x) => {
    if (x.size > 104857600) {
      swal.fire({
        title: "Thông báo",
        text: "Tệp tải lên không quá 100MB!",
        icon: "error",
        confirmButtonText: "OK",
      });
      return;
    }
  });
  if (
    filecoments.length > 12 ||
    TempData.value.file.length == 12 ||
    TempData.value.file.length + filecoments.length > 12
  ) {
    swal.fire({
      title: "Thông báo",
      text: "Bạn chỉ có thể chọn tối đa 12 tệp!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  let fileIndex = [];
  if (filecoments && TempData.value.file.length < 12) {
    if (TempData.value.file.length == 0) {
      filecoments.forEach((f, i) => {
        TempData.value.file.push(f);
      });
    } else {
      filecoments.forEach((x, i) => {
        let fi = TempData.value.file.filter((z) => x.name == z.name);
        if (fi.length > 0) {
          fileIndex.push(i);
        } else {
          TempData.value.file.push(x);
        }
      });
    }

    filecoments.forEach((filecomentIndex, index) => {
      let check =
        fileIndex.length > 0 ? fileIndex.filter((k) => index == k) : 0;
      if (check.length > 0) {
        return;
      } else {
        const element = filecomentIndex;
        let size = formatSize(element.size);
        var allowedExtensions = /(\.jpg|\.jpeg|\.png|\.gif)$/i;
        //Kiểm tra định dạng
        if (allowedExtensions.exec(element.name)) {
          TempData.value.file_display.push({
            data: element,
            src: URL.createObjectURL(element),
            size: size,
            checkimg: true,
          });
          URL.revokeObjectURL(element);
        } else {
          TempData.value.file_display.push({
            data: element.data,
            src: element.name,
            size: size,
            checkimg: false,
          });
        }
      }
    });
  }
  filecoments = [];
};
const delImgComment = (value, index) => {
  if (value.checkimg == true) {
    TempData.value.file = TempData.value.file.filter((x) => x != value.data);
  } else
    TempData.value.file = TempData.value.file.filter(
      (x) => x.name != value.src,
    );
  TempData.value.file_display = [];
  TempData.value.file.forEach((x) => {
    const element = x;
    let size = formatSize(element.size);
    var allowedExtensions = /(\.jpg|\.jpeg|\.png|\.gif)$/i;
    //Kiểm tra định dạng
    if (allowedExtensions.exec(element.name)) {
      TempData.value.file_display.push({
        data: element,
        src: URL.createObjectURL(element),
        size: size,
        checkimg: true,
      });
      URL.revokeObjectURL(element);
    } else {
      TempData.value.file_display.push({
        data: element.data,
        src: element.name,
        size: size,
        checkimg: false,
      });
    }
  });
};

const Report = ref({
  project_id: null,
  task_id: null,
  review_Id: null,
  request_progress: null,
  progress: null,
  contents: null,
  difficult: null,
  request: null,
  status: null,
  is_send_email: false,
});
const mailInfo = ref({
  to: "",
  subject: "",
  body: "",
  isBodyHtml: false,
});
const sendData = (x) => {
  let file = x.file;
  Report.value = {
    project_id: null,
    task_id: x.task_id,
    review_Id: null,
    request_progress: x.reportProgress,
    progress: x.progress,
    contents: x.contents,
    difficult: x.difficult,
    request: x.request,
    status: 0,
    is_send_email: false,
  };

  Report.value.contents =
    Report.value.contents != null
      ? Report.value.contents.replace(/\n/g, "<br/>")
      : "";
  Report.value.difficult =
    Report.value.difficult != null
      ? Report.value.difficult.replace(/\n/g, "<br/>")
      : "";
  Report.value.request =
    Report.value.request != null
      ? Report.value.request.replace(/\n/g, "<br/>")
      : "";
  Report.value.status = 0;
  let formData = new FormData();
  if (file != null)
    for (var i = 0; i < file.length; i++) {
      let filezz = file[i];
      formData.append("url_file", filezz);
    }
  formData.append("comment", JSON.stringify(Report.value));
  axios({
    method: "post",
    url: baseURL + `/api/ReportProgress/${"addReportProgress"}`,
    data: formData,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        toast.success("Thêm mới báo cáo công việc thành công!");
        loadData();
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
};
const configMail = ref();
const getConfigMail = () => {
  axios
    .get(baseURL + "/api/SendEmail/GetConfigMail", {
      headers: { Authorization: `Bearer ${store.getters.token}` },
    })
    .then((response) => {
      if (response.data.err != "1") {
        configMail.value = JSON.parse(response.data.data);
      }
    })
    .catch((error) => {
      if (error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
      }
    });
};
const bodymail = ref();
const sendMail = (x) => {
  mailInfo.value = {
    to: x.uid,
    display_name: "SOE+ - Smart Office Enterprise +",
    subject: "Thông báo công việc trên SOE+",
    body: "",
    isBodyHtml: true,
  };
  bodymail.value =
    "<div style=''><span style='font-weight:700;color:blue;'>" +
    store.state.user.full_name +
    "</span> đã báo cáo công việc: '<span style='font-weight:700;color:blue;'>" +
    x.task_name +
    "</span>'. <a href=" +
    window.location.origin +
    "> Nhấn vào đây để chuyển sang SOE</a></div>";
  mailInfo.value.body = bodymail.value.toString();

  let formData = new FormData();
  formData.append(
    "pwMail",
    configMail.value.kpmail != null
      ? decr(configMail.value.kpmail, SecretKey, cryoptojs).toString()
      : null,
  );
  formData.append("mailinfo", JSON.stringify(mailInfo.value));
  axios({
    method: "post",
    url: baseURL + `/api/SendEmail/${"sendEMail"}`,
    data: formData,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err == "1") {
        swal.fire({
          title: "Lỗi",
          text: response.data.ms,
          icon: "warning",
          confirmButtonText: "OK",
        });
      } else toast.success("Đã gửi email tới người đánh giá");
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
};
const SaveData = () => {
  listSelected.value.forEach((x) => {
    if (x.contents == "" || x.contents == null) {
      swal.fire({
        title: "Thông báo",
        text: "Nội dung báo cáo không được để trống",
        icon: "warning",
        confirmButtonText: "OK",
      });
      return;
    }
  });
  listSelected.value.forEach((x) => {
    sendData(x);
    if (x.is_send_email == true) {
      sendMail(x);
    }
    DialogVisible.value = false;
  });
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
  loadData();
});
watch(showDetail, () => {
  if (showDetail.value == false) {
    loadData();
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
const size = ref(75);
onMounted(() => {
  loadData();
  getConfigMail();
  bodymail.value = "";
});
</script>
<template>
  <div class="main-layout true flex-grow-1 p-2 main-div">
    <DataTable
      :value="listTask"
      v-model:first="first"
      :paginator="true"
      :rows="options.PageSize"
      responsiveLayout="scroll"
      :scrollable="true"
      scrollHeight="flex"
      :loading="options.loading"
      v-model:selection="selectedTasks"
      :lazy="true"
      @page="onPage($event)"
      :totalRecords="options.totalRecords"
      dataKey="task_id"
      :rowHover="true"
      :showGridlines="true"
      paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
      :rowsPerPageOptions="[20, 30, 50, 100, 200]"
    >
      <template #header>
        <Toolbar class="w-full custoolbar p-0">
          <template #start>
            <span class="p-input-icon-left">
              <i class="pi pi-search" />
              <InputText
                v-model="options.searchText"
                type="text"
                spellcheck="false"
                placeholder="Tìm kiếm"
                @keyup.enter="loadData()"
              />
            </span>
          </template>
          <template #end>
            <Button
              class=""
              label=""
              icon="pi pi-send"
              v-tooltip="'Gửi báo cáo'"
              v-if="checkDelList == true"
              @click="openDialog()"
            ></Button>
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
              class="p-button-outlined p-button-secondary"
              label=""
              icon="pi pi-refresh"
              @click="refresh()"
              v-tooltip="'Tải lại'"
            ></Button>
          </template>
        </Toolbar>
      </template>
      <Column
        selectionMode="multiple"
        headerStyle="max-width: 75px"
        bodyStyle="max-width: 75px"
        class="align-items-center justify-content-center text-center"
      ></Column>
      <Column
        header="STT"
        field="STT"
        class="align-items-center justify-content-center text-center max-w-4rem"
      ></Column>
      <Column
        header="Người giao"
        field="creator"
        class="align-items-center justify-content-center text-center max-w-8rem"
      >
        <template #body="data">
          <Avatar
            @error="
              $event.target.src = basedomainURL + '/Portals/Image/nouser1.png'
            "
            v-tooltip.right="{
              value: data.data.creator.tooltip,
              escape: true,
            }"
            v-bind:label="
              data.data.creator.avt
                ? ''
                : data.data.creator.full_name.split(' ').at(-1).substring(0, 1)
            "
            v-bind:image="basedomainURL + data.data.creator.avt"
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
        header="Tên công việc"
        field="task_name"
        headerClass="align-items-center justify-content-center text-center"
      >
        <template #body="data">
          <div
            style="display: flex; flex-direction: column; padding: 5px"
            @click="onNodeSelect(data.data)"
            class="task-hover w-full"
          >
            <div style="line-height: 20px; display: flex">
              <span
                v-tooltip="'Ưu tiên'"
                v-if="data.data.is_prioritize"
                style="margin-right: 5px"
                ><i
                  style="color: orange"
                  class="pi pi-star-fill"
                ></i
              ></span>
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
                >{{
                  data.data.start_date
                    ? moment(new Date(data.data.start_date)).format(
                        "DD/MM/YYYY",
                      )
                    : null
                }}
                -
                {{
                  data.data.end_date
                    ? moment(new Date(data.data.end_date)).format("DD/MM/YYYY")
                    : null
                }}</span
              >
            </div>
            <div
              v-if="data.data.project_name"
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
        header="Tiến độ"
        field="progress"
        class="align-items-center justify-content-center text-center max-w-8rem"
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
        header="Hạn xử lý"
        field="end_date"
        class="align-items-center justify-content-center text-center max-w-12rem"
      >
        <template #body="data">
          <div
            v-if="data.data.is_deadline == true"
            style="
              background-color: #fff8ee;
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
        field="progress"
        class="align-items-center justify-content-center text-center max-w-12rem"
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
        field="status"
        class="align-items-center justify-content-center text-center max-w-12rem"
      >
        <template #body="data">
          <Chip
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
        </div></template
      >
    </DataTable>
  </div>
  <Dialog
    :visible="DialogVisible"
    :header="'Gửi báo cáo công việc'"
    :breakpoints="{ '1366px': '90vw', '960px': '90vw', '640px': '95vw' }"
    :style="{ width: '85vw' }"
    :closable="false"
  >
    <DataTable
      :value="listSelected"
      responsiveLayout="scroll"
      :scrollable="true"
      scrollHeight="75vh"
      dataKey="task_id"
      :rowHover="true"
      :showGridlines="true"
    >
      <Column
        header="STT"
        field="STT"
        class="align-items-center justify-content-center text-center max-w-4rem"
      ></Column>
      <Column
        header="Người đánh giá"
        field="task_master"
        class="align-items-center justify-content-center text-center max-w-10rem"
      >
        <template #body="data">
          <Avatar
            @error="
              $event.target.src = basedomainURL + '/Portals/Image/nouser1.png'
            "
            v-tooltip.right="{
              value: data.data.task_master.tooltip,
              escape: true,
            }"
            v-bind:label="
              data.data.task_master.avt
                ? ''
                : data.data.task_master.full_name
                    .split(' ')
                    .at(-1)
                    .substring(0, 1)
            "
            v-bind:image="basedomainURL + data.data.task_master.avt"
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
        header="Tên công việc"
        field="task_name"
        headerClass="align-items-center justify-content-center text-center"
      >
        <template #body="data">
          <div style="display: flex; flex-direction: column; padding: 5px">
            <div style="line-height: 20px; display: flex">
              <span
                v-tooltip="'Ưu tiên'"
                v-if="data.data.is_prioritize"
                style="margin-right: 5px"
                ><i
                  style="color: orange"
                  class="pi pi-star-fill"
                ></i
              ></span>
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
              >
                {{ data.data.task_name }}
              </span>
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
                >{{
                  data.data.start_date
                    ? moment(new Date(data.data.start_date)).format(
                        "DD/MM/YYYY",
                      )
                    : null
                }}
                -
                {{
                  data.data.end_date
                    ? moment(new Date(data.data.end_date)).format("DD/MM/YYYY")
                    : null
                }}</span
              >
            </div>
            <div
              v-if="data.data.project_name"
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
        header="Tiến độ"
        field="progress"
        class="align-items-center justify-content-center text-center max-w-8rem"
      >
        <template #body="data">
          <span v-if="data.data.progress > 0">
            <ProgressBar
              :value="data.data.progress"
              :show-value="true"
          /></span>
          <span v-else>0%</span>
        </template>
      </Column>
      <Column
        header="Tiến độ BC"
        field="end_date"
        class="align-items-center justify-content-center text-center max-w-8rem"
      >
        <template #body="data">
          <InputNumber
            v-model="data.data.reportProgress"
            class="w-full"
            inputId="minmax-buttons"
            mode="decimal"
            showButtons
            :min="0"
            :max="100"
            suffix=" %"
            v-tooltip.top="{
              value: 'Tiến độ công việc <br/> (0<= x <=100)',
            }"
          >
          </InputNumber>
        </template>
      </Column>
      <Column
        header="Nội dung báo cáo"
        field="progress"
        class="align-items-center justify-content-center text-center max-w-25rem"
      >
        <template #body="data">
          <Textarea
            class="w-full"
            placeholder="Cập nhật kết quả công việc"
            v-model="data.data.contents"
          >
          </Textarea>
        </template>
      </Column>
      <Column
        header=" Email"
        field="progress"
        class="align-items-center justify-content-center text-center max-w-4rem"
      >
        <template #body="data">
          <InputSwitch v-model="data.data.is_send_email"> </InputSwitch>
        </template>
      </Column>
      <Column
        header=""
        field="progress"
        class="align-items-center justify-content-center text-center max-w-8rem"
      >
        <template #body="data">
          <Button
            v-tooltip="'Thêm khó khăn/đề xuất'"
            icon="pi pi-plus-circle"
            class="m-1"
            @click="openMore(data.data)"
          ></Button>
          <div
            class="p-0 m-0"
            style="position: relative"
          >
            <Button
              v-tooltip="'Đính kèm tệp'"
              icon="pi pi-file"
              class="m-1"
              @click="openFile(data.data)"
            ></Button>
            <span
              v-if="data.data.file.length > 0 && selectedTasks != null"
              style="
                position: absolute;
                top: 0px;
                right: 0px;
                height: 20px;
                width: 20px;
                background-color: red;
                color: #fff;
                border-radius: 50%;
                text-align: center;
                padding-top: 2px;
                font-size: 11px;
                font-weight: bold;
              "
              >{{ data.data.file.length }}</span
            >
          </div>
        </template>
      </Column>
      <template #empty>
        <div
          class="block w-full h-full format-center"
          v-if="options.totalRecords == 0"
        >
          <img
            src="../../../../assets/background/nodata.png"
            height="144"
          />
          <h3 class="m-1">Không có dữ liệu</h3>
        </div>
      </template>
    </DataTable>
    <template #footer>
      <Button
        @click="DialogVisible = false"
        class="p-button-raised p-button-text"
        icon="pi pi-times"
        label="Hủy"
      ></Button>
      <Button
        @click="SaveData()"
        class=""
        icon="pi pi-check"
        label="Gửi báo cáo"
      ></Button>
    </template>
  </Dialog>
  <Dialog
    header="Tải lên tệp đính kèm"
    v-model:visible="DialogFileVisible"
    :closable="true"
    :modal="true"
    :style="{ width: '60vw' }"
  >
    <form>
      <div class="col-12">
        <label
          for="FileUpload"
          class="p-button justify-content-center p-button-raised col-4 col-offset-4 font-bold text-xl"
        >
          <i class="pi pi-upload font-bold mr-4 text-2xl" />
          <span>Thêm tệp đính kèm</span>
        </label>
      </div>

      <input
        id="FileUpload"
        type="file"
        multiple
        @input="Upload"
        hidden
      />
      <div class="col-12">
        <div
          class="left-0 w-full bg-white"
          v-if="TempData.file_display.length > 0"
        >
          <div class="col-12 h-full bottom-0 p-0 m-0">
            <div
              class="col-12 p-0 m-0 font-bold pl-2 bg-white"
              style="
                font-weight: bold;
                font-size: 16px;
                margin: 10px 0;
                border-top: 1px solid #f5f5f5;
                padding-top: 15px;
                color: #2196f3;
              "
              v-if="TempData.file_display.length > 0"
            >
              Tệp đính kèm
            </div>
            <div
              class="col-12 flex format-center bg-white"
              v-if="TempData.file_display.length > 0"
              style="
                max-width: 70vw;
                height: auto;
                display: flex;
                flex-wrap: wrap;
              "
            >
              <div
                v-for="(item, index) in TempData.file_display"
                :key="index"
                class="col-2 relative format-center file-hover border-1 border-gray-300 border-round-xl my-1 px-1 h-8rem"
              >
                <Button
                  @click="delImgComment(item, index)"
                  icon="pi pi-times-circle"
                  class="absolute p-button-danger p-button-text p-button-rounded top-0 right-0 pr-0 mr-0 p-button-hover"
                  v-tooltip="{ value: 'Xóa tệp' }"
                ></Button>

                <div
                  class=""
                  v-if="item.checkimg == true"
                >
                  <img
                    :src="item.src"
                    :alt="' '"
                    style="
                      max-width: 80px;
                      max-height: 50px;
                      object-fit: contain;
                      margin-top: 5px;
                    "
                    class="pt-1"
                  />
                  <div
                    class="p-1"
                    style="
                      width: 95px;
                      font-size: 13px;
                      overflow: hidden;
                      text-overflow: ellipsis;
                      display: block;
                      font-weight: 500;
                      white-space: nowrap;
                    "
                  >
                    {{ item.data.name }}
                    <br />
                    {{ item.size }}
                  </div>
                </div>
                <div
                  class=""
                  v-else
                >
                  <img
                    :src="
                      basedomainURL +
                      '/Portals/Image/file/' +
                      item.src.substring(item.src.lastIndexOf('.') + 1) +
                      '.png'
                    "
                    style="
                      max-width: 80px;
                      max-height: 50px;
                      object-fit: contain;
                      margin-top: 5px;
                    "
                    :alt="' '"
                    class="pt-1"
                  />
                  <div
                    class="p-1"
                    style="
                      width: 95px;
                      font-size: 13px;
                      overflow: hidden;
                      text-overflow: ellipsis;
                      display: block;
                      font-weight: 500;
                      white-space: nowrap;
                    "
                  >
                    {{ item.src }}
                    <br />
                    {{ item.size }}
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        class="p-button-raised p-button-text"
        @click="
          (DialogFileVisible = false),
            (TempData.file = []),
            (TempData.file_display = [])
        "
      />
      <Button
        label="Lưu"
        icon="pi pi-check"
        @click="DialogFileVisible = false"
      />
    </template>
    <!-- Chức năng đang chỉnh sửa vui lòng liên hệ quản trị viên phần mềm -->
  </Dialog>
  <Dialog
    header="Khó khăn/đề xuất"
    v-model:visible="DialogMoreVisible"
    :closable="true"
    :modal="true"
    :style="{ width: '60vw' }"
  >
    <form>
      <div class="col-12 flex">
        <div class="col-3">Khó khăn</div>
        <Textarea
          class="col-9"
          v-model="TempData.difficult"
        ></Textarea>
      </div>
      <div class="col-12 flex">
        <div class="col-3">Đề xuất giải quyết</div>
        <Textarea
          class="col-9"
          v-model="TempData.request"
        ></Textarea>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        class="p-button-raised p-button-text"
        @click="
          ((DialogMoreVisible = false), (TempData.difficult = '')),
            (TempData.request = '')
        "
      />
      <Button
        label="Lưu"
        icon="pi pi-check"
        @click="DialogMoreVisible = false"
      />
    </template>
  </Dialog>
  <Sidebar
    v-model:visible="showDetail"
    :position="PositionSideBar"
    :style="{
      width:
        PositionSideBar == 'right'
          ? width1 > 1800
            ? ' 55vw'
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
</template>

<style lang="scss" scoped>
.main-div {
  height: calc(100vh - 7rem);
}
.task-hover:hover {
  background-color: #f5f5f5;
  color: #2196f3;
}
.active {
  background-color: #2196f3 !important;
  color: #ffffff !important;
}
</style>
