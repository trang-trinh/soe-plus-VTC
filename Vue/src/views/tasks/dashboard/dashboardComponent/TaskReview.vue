<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { encr } from "../../../../util/function.js";
import moment from "moment";
import DetailedWork from "../../../../components/task_origin/DetailedWork.vue";
import FilterTask from "./filterTask.vue";
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
});
let user = store.state.user;
const listTask = ref([]);
const loadData = () => {
  axios
    .post(
      // eslint-disable-next-line no-undef
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "task_reportprogress_list_to_dashboard",
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

        x.STT = options.value.PageNo * options.value.PageSize + i + 1;
        x.progress = x.progress ?? 0;
      });
      listTask.value = data;
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
const first = ref(0);
const checkDelList = ref(false);
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
watch(selectedTasks, () => {
  if (selectedTasks.value.length > 0) {
    checkDelList.value = true;
  } else checkDelList.value = false;
});
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
const DialogVisible = ref(false);
const DialogFileVisible = ref(false);
const listSelected = ref([]);
const openDialog = () => {
  DialogVisible.value = true;
  listSelected.value = [];
  listSelected.value = JSON.parse(JSON.stringify(selectedTasks.value));
  listSelected.value.forEach((x) => {
    let vl = x.request_progress;
    x.progressReview = vl;
    x.is_agree = true;
    x.contentsReview = "";
    x.pointRating = 5;
    x.file = [];
    x.file_display = [];
  });
};
const PositionSideBar = ref("right");
emitter.on("psb", (obj) => {
  PositionSideBar.value = obj;
});
const TempData = ref();
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
const Review = ref({
  project_id: null,
  task_id: null,
  report_id: null,
  contents: "",
  point: null,
  progress: null,
  is_type: null,
});
const SaveData = () => {
  listSelected.value.forEach((x) => {
    if (x.contents == null || x.contents == "") {
      swal.fire({
        title: "Thông báo!",
        html: "Nội dung đánh giá không được để trống",
        icon: "warning",
        confirmButtonText: "OK",
      });
      return;
    }

    Review.value = {
      project_id: x.project_id,
      task_id: x.task_id,
      report_id: x.report_id,
      contents: x.contentsReview,
      point: x.pointRating,
      progress: x.progressReview,
      is_type: x.is_agree == true ? 0 : 1,
    };
    Review.value.contents =
      Review.value.contents != null
        ? Review.value.contents.replace(/\n/g, "<br/>")
        : "";
    let formData = new FormData();
    if (x.file != null)
      for (var i = 0; i < x.file.length; i++) {
        let file = x.file[i];
        formData.append("url_file", file);
      }
    formData.append("comment", JSON.stringify(Review.value));
    axios({
      method: "post",
      url: baseURL + `/api/Review/${"addReview"}`,
      data: formData,
      headers: {
        Authorization: `Bearer ${store.getters.token}`,
      },
    })
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Thêm mới báo cáo công việc thành công!");
          Review.value = {
            project_id: null,
            task_id: null,
            report_id: null,
            contents: "",
            point: null,
            progress: null,
            is_type: null,
          };
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
  });
};

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
const size = ref(75);
const filterChange = () => {
  styleObj.value = style.value;
};
const removeFilter = () => {
  styleObj.value = {};
};
onMounted(() => {
  loadData();
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
      dataKey="report_id"
      :rowHover="true"
      :showGridlines="true"
      paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
      :rowsPerPageOptions="[20, 30, 50, 100, 200]"
    >
      <template #header>
        <Toolbar class="w-full custoolbar">
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
              v-tooltip="'Gửi đánh giá'"
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
              style="width: 45vw; z-index: 1000"
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
        header="Tên công việc"
        field="task_name"
        headerClass="align-items-center justify-content-center text-center vertical-align-middle"
        bodyClass="align-items-center justify-content-center vertical-align-middle"
      >
        <template #body="data">
          <div
            style="
              display: flex;
              flex-direction: column;
              padding: 5px;
              justify-content: center;
            "
            @click="onNodeSelect(data.data)"
            class="task-hover w-full h-full vertical-align-middle"
          >
            <div style="line-height: 20px; display: flex">
              <span
                v-tooltip="'Ưu tiên'"
                v-if="data.data.is_prioritize"
                style="margin-right: 5px"
              >
                <i
                  style="color: orange"
                  class="pi pi-star-fill"
                >
                </i>
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
                    ? moment(new Date(data.data.end_date)).format("DD/MM/YYYY")
                    : null
                }}
              </span>
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
              >
                {{ data.data.project_name }}
              </span>
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
        header="Người báo cáo"
        field="creator"
        class="align-items-center justify-content-center text-center max-w-10rem"
      >
        <template #body="data">
          <Avatar
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
        header="Ngày báo cáo"
        field="created_date"
        class="align-items-center justify-content-center text-center max-w-9rem"
      >
        <template #body="data">
          <span
            class="px-3 py-2 border-round-lg font-bold text-white"
            style="background: #33c9dc"
          >
            {{ moment(data.data.created_date).format("DD/MM/YYYY") }}
          </span>
        </template>
      </Column>
      <Column
        header="Thông tin báo cáo"
        field=""
        headerClass="align-items-center justify-content-center"
        class="max-w-30rem"
      >
        <template #body="data">
          <div class="p-0">
            <div class="col-12">
              <div class="col-12 font-bold">Nội dung báo cáo</div>
              <div class="col-12">
                <span class="">
                  -
                  <span
                    class=""
                    v-html="data.data.contents"
                  >
                  </span>
                </span>
              </div>
            </div>
            <div
              class="col-12"
              v-if="data.data.difficult"
            >
              <div class="col-12 font-bold">Khó khăn</div>
              <div class="col-12">
                <span class="">
                  -
                  <span
                    class=""
                    v-html="data.data.difficult"
                  >
                  </span>
                </span>
              </div>
            </div>
            <div
              class="col-12"
              v-if="data.data.request"
            >
              <div class="col-12 font-bold">Nội dung báo cáo</div>
              <div class="col-12">
                <span class="">
                  -
                  <span
                    class=""
                    v-html="data.data.request"
                  >
                  </span>
                </span>
              </div>
            </div>
          </div>
        </template>
      </Column>
      <Column
        header="Tiến độ thực hiện"
        field=""
        class="align-items-center justify-content-center text-center max-w-10rem"
      >
        <template #body="data">
          <div class="w-full">
            <Knob
              class="w-full"
              v-model="data.data.request_progress"
              :readonly="true"
              valueTemplate="{value}%"
              :valueColor="
                data.data.request_progress < 33
                  ? '#FF0000'
                  : data.data.request_progress < 66
                  ? '#2196f3'
                  : '#6dd230'
              "
              :textColor="
                data.data.request_progress < 33
                  ? '#FF0000'
                  : data.data.request_progress < 66
                  ? '#2196f3'
                  : '#6dd230'
              "
              :size="size"
            />
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
  </div>
  <Dialog
    :visible="DialogVisible"
    :header="'Đánh giá công việc'"
    :breakpoints="{ '1366px': '90vw', '960px': '90vw', '640px': '95vw' }"
    :style="{ width: '85vw' }"
    :closable="false"
  >
    <DataTable
      :value="listSelected"
      responsiveLayout="scroll"
      :scrollable="true"
      scrollHeight="75vh"
      dataKey="report_id"
      :rowHover="true"
      :showGridlines="true"
    >
      <Column
        header="STT"
        field="STT"
        class="align-items-center justify-content-center text-center max-w-4rem"
      ></Column>

      <Column
        header="Tên công việc"
        field="task_name"
        headerClass="align-items-center justify-content-center text-center vertical-align-middle"
        bodyClass="align-items-center justify-content-center vertical-align-middle"
      >
        <template #body="data">
          <div
            style="
              display: flex;
              flex-direction: column;
              padding: 5px;
              justify-content: center;
            "
            @click="onNodeSelect(data.data)"
            class="task-hover w-full h-full vertical-align-middle"
          >
            <div style="line-height: 20px; display: flex">
              <span
                v-tooltip="'Ưu tiên'"
                v-if="data.data.is_prioritize"
                style="margin-right: 5px"
              >
                <i
                  style="color: orange"
                  class="pi pi-star-fill"
                >
                </i>
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
                    ? moment(new Date(data.data.end_date)).format("DD/MM/YYYY")
                    : null
                }}
              </span>
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
              >
                {{ data.data.project_name }}
              </span>
            </div>
          </div>
        </template>
      </Column>
      <Column
        header="Người báo cáo"
        field="creator"
        class="align-items-center justify-content-center text-center max-w-10rem"
      >
        <template #body="data">
          <Avatar
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
        header="Tiến độ báo cáo"
        field=""
        class="align-items-center justify-content-center text-center max-w-10rem"
      >
        <template #body="data">
          <div class="w-full">
            <Knob
              class="w-full"
              v-model="data.data.request_progress"
              :readonly="true"
              valueTemplate="{value}%"
              :valueColor="
                data.data.request_progress < 33
                  ? '#FF0000'
                  : data.data.request_progress < 66
                  ? '#2196f3'
                  : '#6dd230'
              "
              :textColor="
                data.data.request_progress < 33
                  ? '#FF0000'
                  : data.data.request_progress < 66
                  ? '#2196f3'
                  : '#6dd230'
              "
              :size="size"
            />
          </div>
        </template>
      </Column>
      <Column
        header="Tiến độ đánh giá"
        field="progress"
        class="align-items-center justify-content-center text-center max-w-10rem"
      >
        <template #body="data">
          <InputNumber
            v-model="data.data.progressReview"
            class="w-full"
            inputId="minmax-buttons"
            mode="decimal"
            showButtons
            :min="0"
            :max="100"
            suffix=" %"
            v-tooltip.top="{
              value: 'Tiến độ công việc <br/> (0<= x <=100)',
              escape: true,
            }"
          ></InputNumber>
        </template>
      </Column>

      <Column
        header="Nội dung đánh giá"
        field=""
        headerClass="align-items-center justify-content-center p-2 max-w-30rem"
        bodyClass=" p-2 max-w-30rem"
      >
        <template #body="data">
          <div class="w-full">
            <Textarea
              class="w-full max-h-20rem"
              v-model="data.data.contentsReview"
            ></Textarea>
            <Rating
              v-model="data.data.pointRating"
              :stars="5"
              style="text-align: left"
              class="w-full"
            />
          </div>
        </template>
      </Column>
      <Column
        header="Đồng ý"
        field=""
        class="align-items-center justify-content-center p-2 max-w-6rem"
      >
        <template #body="data">
          <InputSwitch v-model="data.data.is_agree"></InputSwitch>
        </template>
      </Column>
      <Column
        header="Tệp đính kèm"
        field=""
        class="align-items-center justify-content-center p-2 max-w-8rem"
      >
        <template #body="data">
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
        label="Gửi đánh giá"
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
</style>
