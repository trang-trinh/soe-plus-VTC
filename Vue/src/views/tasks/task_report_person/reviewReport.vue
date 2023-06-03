<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { encr } from "../../../util/function.js";
import moment from "moment";
import DetailReportVue from "./component/DetailReport.vue";
import Processing from "./component/ProcessingReport.vue";
const toast = useToast();
const cryoptojs = inject("cryptojs");
const emitter = inject("emitter");
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");

const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const addLog = (log) => {
  axios.post(baseURL + "/api/Proc/AddLog", log, config);
};
const basedomainURL = fileURL;
const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
  "#FF88D3",
]);
let user = store.state.user;
const options = ref({
  PageNo: 0,
  PageSize: 20,
  totalRecords: 0,
  loading: true,
  month: null,
  year: null,
  DateTime: null,
});
const first = ref(0);
const datalists = ref();
const noData = ref(true);
const status = ref([
  { code: 0, label: "Khởi tạo", bgColor: "#2196F3", text: "#FFFFFF" },
  { code: 1, label: "Đang chờ duyệt", bgColor: "#FF6E31", text: "#FFFFFF" },
  { code: 2, label: "Đã duyệt", bgColor: "#6DD230", text: "#FFFFFF" },
  { code: 3, label: "Trả lại", bgColor: "#FF0000", text: "#FFFFFF" },
]);
const isType = ref([
  { code: -1, label: "Đang chờ duyệt", bgColor: "#FF6E31", text: "#FFFFFF" },
  { code: 0, label: "Đã duyệt", bgColor: "#6DD230", text: "#FFFFFF" },
  { code: 1, label: "Trả lại", bgColor: "#FF0000", text: "#FFFFFF" },
]);
const LoadLinkTaskOrigin = (user, list_task_id) => {
  optionsLinkTask.value.loading = true;
  if (optionsLinkTask.value.PageNo) {
    first.value = 0;
  }
  if (SidebarVisible.value == true) {
    optionsLinkTask.value.PageSize = 999999999;
  }

  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "task_origin_list_review_person",
            par: [
              { par: "user", va: user },
              { par: "string", va: list_task_id },
            ],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let data1 = JSON.parse(response.data.data)[0];
      if (data1.length > 0) {
        data1.forEach((element, i) => {
          element.progress = element.progress == null ? 0 : element.progress;
          element.update_date = element.modified_date
            ? element.modified_date
            : element.created_date;
          element.status_name =
            element.status != null
              ? listDropdownStatus.value.filter(
                  (x) => x.value == element.status,
                )[0].text
              : "";
          element.status_bg_color =
            element.status != null
              ? listDropdownStatus.value.filter(
                  (x) => x.value == element.status,
                )[0].bg_color
              : "";
          element.status_text_color =
            element.status != null
              ? listDropdownStatus.value.filter(
                  (x) => x.value == element.status,
                )[0].text_color
              : "";
          //thời gian xử lý
          if (element.end_date != null) {
            if (element.thoigianquahan < 0) {
              if (element.thoigianxuly > 0) {
                element.title_time = element.thoigianxuly + " ngày";
                element.time_bg = element.status_bg_color;
                element.time_color = "color: #fff;";
              }
            } else {
              if (element.thoigianquahan > 0) {
                element.title_time =
                  "Quá hạn " + element.thoigianquahan + " ngày";
                element.time_bg = "red";
                element.time_color = "color: #fff;";
              }
            }
          } else if (element.thoigianxuly) {
            element.title_time = element.thoigianxuly + " ngày";
            element.time_bg = element.status_bg_color;
            element.time_color = "color: #fff;";
          }
          element.Thanhviens = element.Thanhviens
            ? JSON.parse(element.Thanhviens)
            : [];
          element.ThanhvienShows = [];
          if (element.Thanhviens.length > 3) {
            element.ThanhvienShows = element.Thanhviens.slice(0, 3);
          } else {
            element.ThanhvienShows = [...element.Thanhviens];
          }
          element.files = element.files ? JSON.parse(element.files) : [];
          element.files = element.files ? element.files : [];
          element.STT =
            optionsLinkTask.value.PageNo * optionsLinkTask.value.PageSize +
            i +
            1;
        });
      }
      listTaskLink.value = data1;
      optionsLinkTask.value.totalRecords = data1[1][0].total;
    })
    .catch((error) => {
      //   toast.error("Tải dữ liệu không thành công6!");
      optionsLinkTask.value.loading = false;
      if (error && error.status === 401) {
        swal.fire({
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const loadData = () => {
  noData.value = true;
  options.value.loading = true;
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "task_person_report_processing_list_user",
            par: [
              { par: "user_id", va: user.user_id },
              { par: "pageno", va: options.value.PageNo },
              { par: "pagesize", va: options.value.PageSize },
              { par: "search", va: options.value.SearchText },
              { par: "month", va: options.value.month },
              { par: "year", va: options.value.year },
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
      let count = JSON.parse(response.data.data)[1];
      options.value.totalRecords = count[0].totalRecords;
      data.forEach((x, i) => {
        let temp = "";
        temp = x.list_task_id;
        x.list_task_id = [];
        x.list_task_id = temp.split(",");
        x.status_display = status.value.filter((a) => a.code == x.status)[0];
        x.is_type_display = isType.value.filter((a) => a.code == x.is_type)[0];
        x.stt = options.value.PageNo * options.value.PageSize + (i + 1);
        x.user_info = JSON.parse(x.user_info);
      });
      datalists.value = data;
      if (count > 0) {
        noData.value = false;
      }
      options.value.loading = false;
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;
      addLog({
        title: "Lỗi Console loadData",
        controller: "reviewReport.vue",
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

const refresh = () => {
  first.value = 0;
  options.value = {
    PageNo: 0,
    PageSize: 20,
    totalRecords: 0,
    SearchText: null,
    DateTime: null,
  };
  styleObj.value = "";
  loadData();
};
const checkDelList = ref(false);
const selectedReport = ref();
watch(selectedReport, () => {
  if (selectedReport.value.length > 0) {
    checkDelList.value = true;
  } else {
    checkDelList.value = false;
  }
});
const optionsLinkTask = ref({
  IsNext: true,
  sort: "modified_date",
  ob: "DESC",
  PageNo: 0,
  PageSize: 20,
  search: "",
  Filteruser_id: null,
  user_id: store.getters.user_id,
  IsType: 0,
  SearchTextUser: "",
  filter_type: 0,
  sdate: null,
  edate: null,
  loctitle: "Lọc",
  type_view: 1,
});
const listTaskLink = ref();
const SidebarVisible = ref(false);
const pDataa = ref();
const showInfo = (data) => {
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "task_origin_list_review_person",
            par: [
              { par: "user_id", va: "" },
              { par: "string", va: data.list_task_id1 },
            ],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let data1 = JSON.parse(response.data.data)[0];
      if (data1.length > 0) {
        data1.forEach((element, i) => {
          element.progress = element.progress == null ? 0 : element.progress;
          element.update_date = element.modified_date
            ? element.modified_date
            : element.created_date;
          element.status_name =
            element.status != null
              ? listDropdownStatus.value.filter(
                  (x) => x.value == element.status,
                )[0].text
              : "";
          element.status_bg_color =
            element.status != null
              ? listDropdownStatus.value.filter(
                  (x) => x.value == element.status,
                )[0].bg_color
              : "";
          element.status_text_color =
            element.status != null
              ? listDropdownStatus.value.filter(
                  (x) => x.value == element.status,
                )[0].text_color
              : "";
          //thời gian xử lý
          if (element.end_date != null) {
            if (element.thoigianquahan < 0) {
              if (element.thoigianxuly > 0) {
                element.title_time = element.thoigianxuly + " ngày";
                element.time_bg = element.status_bg_color;
                element.time_color = "color: #fff;";
              }
            } else {
              if (element.thoigianquahan > 0) {
                element.title_time =
                  "Quá hạn " + element.thoigianquahan + " ngày";
                element.time_bg = "red";
                element.time_color = "color: #fff;";
              }
            }
          } else if (element.thoigianxuly) {
            element.title_time = element.thoigianxuly + " ngày";
            element.time_bg = element.status_bg_color;
            element.time_color = "color: #fff;";
          }
          element.Thanhviens = element.Thanhviens
            ? JSON.parse(element.Thanhviens)
            : [];
          element.ThanhvienShows = [];
          if (element.Thanhviens.length > 3) {
            element.ThanhvienShows = element.Thanhviens.slice(0, 3);
          } else {
            element.ThanhvienShows = [...element.Thanhviens];
          }
          element.files = element.files ? JSON.parse(element.files) : [];
          element.files = element.files ? element.files : [];
          element.STT =
            optionsLinkTask.value.PageNo * optionsLinkTask.value.PageSize +
            i +
            1;
        });
      }
      pDataa.value = null;
      let datasend = Object.assign({}, data);
      datasend.task_info = [];
      data1.forEach((x) => {
        datasend.task_info.push(x);
      });
      pDataa.value = datasend;

      SidebarVisible.value = true;
    })
    .catch((error) => {
      //   toast.error("Tải dữ liệu không thành công6!");
      optionsLinkTask.value.loading = false;
      if (error && error.status === 401) {
        swal.fire({
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
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
    text: "Đợi đánh giá",
    bg_color: "#33c9dc",
    text_color: "#FFFFFF",
  },
  { value: 6, text: "Bị trả lại", bg_color: "#ffa500", text_color: "#FFFFFF" },
  { value: 7, text: "HT sau hạn", bg_color: "#ff8b4e", text_color: "#FFFFFF" },
  { value: 8, text: "Đã đánh giá", bg_color: "#51b7ae", text_color: "#FFFFFF" },
  { value: -1, text: "Bị xóa", bg_color: "red", text_color: "#FFFFFF" },
]);

const menu = ref();
const items = ref([
  {
    label: "Phê duyệt",
    code: 0,
    command: () => {
      reviewReport(0);
    },
  },
  {
    label: "Trả lại",
    code: 1,
    command: () => {
      reviewReport(1);
    },
  },
]);
const temporaryData = ref();
const toggle = (event, data) => {
  temporaryData.value = [];
  menu.value.toggle(event);
  if (data == null) {
    temporaryData.value = selectedReport.value;
  } else {
    temporaryData.value.push(data);
  }
};
const headerDialog = ref();
const MainDialogVisible = ref(false);
const reviewReport = (typeValue) => {
  headerDialog.value = typeValue == 0 ? "Phê duyệt báo cáo" : "Trả lại báo cáo";
  MainDialogVisible.value = true;
  submitted.value = false;
  review.value = {
    process_id: "",
    messages: null,
    point: 0,
    type: typeValue,
  };

  temporaryData.value.forEach((element) => {
    review.value.process_id +=
      (review.value.process_id != "" ? "," : "") + element.process_id;
    review.value.point = typeValue == 0 ? element.self_point : 0;
  });
};
const review = ref({
  process_id: null,
  messages: "",
  point: null,
  type: null,
});
const rules = {
  messages: { required },
};
const v$ = useVuelidate(rules, review);
const submitted = ref(false);
const saveData = (isFormValid) => {
  submitted.value = true;
  if (!isFormValid) {
    return;
  } else {
    swal
      .fire({
        title: "Thông báo",
        html: "Xác nhận hoàn thành đánh giá ?",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "OK",
        cancelButtonText: "Không",
      })
      .then((result) => {
        if (result.isConfirmed) {
          swal.fire({
            width: 110,
            didOpen: () => {
              swal.showLoading();
            },
          });
          let formData = new FormData();
          let data = Object.assign({}, review.value);
          data.messages = data.messages.replace(/\n/g, "<br/>");
          formData.append("id", JSON.stringify(data.process_id));
          formData.append("message", JSON.stringify(data.messages));
          formData.append("point", JSON.stringify(data.point));
          formData.append("type", JSON.stringify(data.type));
          axios({
            method: "post",
            url: baseURL + `/api/review_Person_Report/${"Processing"}`,
            data: formData,
            headers: {
              Authorization: `Bearer ${store.getters.token}`,
            },
          })
            .then((response) => {
              if (response.data.err != "1") {
                swal.close();
                toast.success("Đánh giá báo cáo thành công!");
                loadData();
                MainDialogVisible.value = false;
                swal.close();
              } else {
                swal.close();
                let ms = response.data.ms;
                swal.fire({
                  title: "Thông báo!",
                  html: ms,
                  icon: "error",
                  confirmButtonText: "OK",
                });
              }
            })
            .catch((error) => {
              swal.close();
              swal.fire({
                title: "Thông báo",
                text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
                icon: "error",
                confirmButtonText: "OK",
              });
            });
        }
      });
  }
};
const ProcessingVisible = ref(false);
const processing_id = ref();
const ShowProcess = (data) => {
  processing_id.value = data.report_id;
  ProcessingVisible.value = true;
};
const MonthChange2 = (event) => {
  options.value.month = event.getMonth() + 1;
  options.value.year = event.getFullYear();
};
const style = ref({
  "background-color": "#2196F3 !important",
  color: "#fff !important",
  " border": "1px solid #5ca7e3 !important",
});
const styleObj = ref();
const op2 = ref();
const toggle2 = (event) => {
  op2.value.toggle(event);
};
const reNewFilter = () => {
  options.value.year = null;
  options.value.month = null;
  options.value.DateTime = null;
  styleObj.value = null;
  loadData();
};
const onPage = (event) => {
  if (event.rows != options.value.PageSize) {
    options.value.PageSizeze = event.rows;
  }
  options.value.PageNo = event.page;
  loadData();
};
onMounted(() => {
  loadData();
  LoadLinkTaskOrigin(user.user_id, "");
  return;
});
</script>
<template>
  <div class="main-layout true flex-grow-1 p-2">
    <DataTable
      v-model:first="first"
      :value="datalists"
      :paginator="true"
      :rows="options.PageSize"
      :scrollable="true"
      scrollHeight="flex"
      :loading="options.loading"
      v-model:selection="selectedReport"
      :lazy="true"
      @page="onPage($event)"
      @filter="onFilter($event)"
      @sort="onSort($event)"
      :totalRecords="options.totalRecords"
      dataKey="report_id"
      :rowHover="true"
      :showGridlines="true"
      paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
      :rowsPerPageOptions="[20, 30, 50, 100, 200]"
      responsiveLayout="scroll"
      @row-dblclick="showInfo($event.data)"
    >
      <!-- {{ pDataa }} -->
      <template #header>
        <h3 class="module-title mt-0 ml-1 mb-2">
          <i class="pi pi-sliders-v"></i> Danh sách báo cáo công việc ({{
            options.totalRecords
          }})
        </h3>
        <Toolbar class="w-full custoolbar"
          ><template #start>
            <span class="p-input-icon-left">
              <i class="pi pi-search" />
              <InputText
                v-model="options.SearchText"
                @keyup.enter="loadData()"
                type="text"
                spellcheck="false"
                placeholder="Tìm kiếm"
              />
            </span>
            <Button
              type="button"
              class="ml-2 p-button-outlined p-button-secondary"
              icon="pi pi-filter"
              @click="toggle2($event)"
              aria:haspopup="true"
              aria-controls="overlay_panel2"
              v-tooltip="'Bộ lọc'"
              :style="[styleObj]" />
            <OverlayPanel
              ref="op2"
              appendTo="body"
              class="p-0 m-0"
              :showCloseIcon="false"
              id="overlay_panel2"
            >
              <div class="grid formgrid m-0">
                <Calendar
                  inputId="basic"
                  v-model="options.DateTime"
                  autocomplete="off"
                  view="month"
                  dateFormat="MM/yy"
                  :showIcon="true"
                  :manualInput="false"
                  class="w-full"
                  @date-select="MonthChange2($event)"
                  placeholder="Chọn tháng năm"
                />

                <Toolbar class="border-none surface-0 outline-none pb-0 w-full">
                  <template #start>
                    <Button
                      @click="reNewFilter()"
                      class="p-button-outlined"
                      label="Xóa"
                    >
                    </Button>
                  </template>
                  <template #end>
                    <Button
                      @click="loadData(), (styleObj = style)"
                      label="Lọc"
                    ></Button>
                  </template>
                </Toolbar>
              </div> </OverlayPanel
          ></template>
          <template #end>
            <Button
              v-if="checkDelList"
              label="Xử lý"
              icon="pi pi-send"
              class="mr-2 p-button-raised"
              @click="toggle($event, null)"
              aria-haspopup="true"
              aria-controls="overlay_panel"
            />

            <Button
              @click="refresh()"
              class="mr-2 p-button-outlined p-button-secondary"
              icon="pi pi-refresh"
              v-tooltip="'Tải lại'"
            />
          </template>
        </Toolbar>
      </template>

      <Column
        selectionMode="multiple"
        headerStyle="text-align:center;max-width:60px;height:50px"
        bodyStyle="text-align:center;max-width:60px "
        class="align-items-center justify-content-center text-center"
      ></Column>
      <Column
        field="stt"
        header="STT"
        headerStyle="text-align:center;max-width:75px;height:50px"
        bodyStyle="text-align:center;max-width:75px "
        class="align-items-center justify-content-center text-center"
      ></Column>
      <Column
        field="report_name"
        header="Tên báo cáo"
        headerStyle="text-align:center;height:50px"
        bodyStyle=""
        headerClass="align-items-center justify-content-center"
      >
        <template #body="data">
          <div
            class="name-hover"
            @click="showInfo(data.data)"
          >
            <span>{{ data.data.report_name }}</span>
          </div>
        </template>
      </Column>
      <Column
        field="self_point"
        header="Điểm tự đánh giá"
        headerStyle="text-align:center;max-width:10rem;height:50px"
        bodyStyle="text-align:center;max-width:10rem"
        class="align-items-center justify-content-center text-center"
      >
      </Column>
      <Column
        field="user_point"
        header="Điểm đánh giá"
        headerStyle="text-align:center;max-width:10rem;height:50px"
        bodyStyle="text-align:center;max-width:10rem"
        class="align-items-center justify-content-center text-center"
      >
      </Column>
      <Column
        field="is_type"
        header="Trạng thái"
        headerStyle="text-align:center;max-width:10rem;height:50px"
        bodyStyle="text-align:center;max-width:10rem"
        class="align-items-center justify-content-center text-center"
      >
        <template #body="data">
          <Chip
            :style="{
              background: data.data.is_type_display.bgColor,
              color: data.data.is_type_display.text,
            }"
            v-bind:label="data.data.is_type_display.label"
          />
        </template>
      </Column>
      <Column
        field="is_type"
        header="Người tạo"
        headerStyle="text-align:center;max-width:10rem;height:50px"
        bodyStyle="text-align:center;max-width:10rem"
        class="align-items-center justify-content-center text-center"
      >
        <template #body="data">
          <Avatar
            @error="
              $event.target.src = basedomainURL + '/Portals/Image/nouser1.png'
            "
            v-tooltip.bottom="{
              value:
                'Người tạo báo cáo: <br/>' +
                data.data.user_info.full_name +
                '<br/>' +
                (data.data.user_info.position_name || '') +
                '<br/>' +
                (data.data.user_info.department_name ||
                  data.data.user_info.organization_name),
              escape: true,
            }"
            v-bind:label="
              data.data.user_info.avatar
                ? ''
                : data.data.user_info.full_name
                    .split(' ')
                    .at(-1)
                    .substring(0, 1)
            "
            v-bind:image="basedomainURL + data.data.user_info.avatar"
            style="color: #ffffff; cursor: pointer"
            :style="{
              background: 'pink',
              border: '2px solid #fffa8d',
            }"
            class="flex p-0 m-0"
            size=""
            shape="circle"
          />
        </template>
      </Column>
      <Column
        field=""
        header="Chức năng"
        headerStyle="text-align:center;height:50px;max-width:15rem"
        bodyStyle="max-width:15rem"
        class="align-items-center justify-content-center text-center"
      >
        <template #body="data">
          <div class="format-center">
            <Button
              @click="showInfo(data.data)"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-info-circle"
              v-tooltip="'Thông tin báo cáo'"
            ></Button>
            <Button
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-send"
              v-tooltip="'Xử lý'"
              v-if="data.data.is_type == -1"
              @click="toggle($event, data.data)"
              aria-haspopup="true"
              aria-controls="overlay_panel"
            >
            </Button>
            <Button
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-chart-bar"
              v-tooltip="'Quy trình xử lý'"
              @click="ShowProcess(data.data)"
            ></Button>
          </div>
        </template>
      </Column>
      <template #empty>
        <div
          class="align-items-center justify-content-center p-4 text-center m-auto"
          v-if="noData == true"
        >
          <img
            src="../../../assets/background/nodata.png"
            height="144"
          />
          <h3 class="m-1">Không có dữ liệu</h3>
        </div>
      </template>
    </DataTable>
  </div>
  <Dialog
    v-model:visible="SidebarVisible"
    :style="'width: 70vw;'"
    :showCloseIcon="true"
    header="Thông tin chi tiết"
    maximizable="true"
  >
    <DetailReportVue
      :data="pDataa"
      :is_rv="true"
    ></DetailReportVue>
    <template #footer>
      <div class="mt-2">
        <Button
          icon="pi pi-times"
          label="Đóng"
          @click="SidebarVisible = false"
        />
      </div>
    </template>
  </Dialog>
  <Menu
    :model="items"
    ref="menu"
    :popup="true"
    id="overlay_panel"
    style=""
  >
    <template #item="{ item }">
      <Button
        :icon="item.icon"
        class="p-button-text p-button-secondary w-full format-center"
        :label="item.label"
        @click="item.command"
      >
      </Button>
    </template>
  </Menu>
  <Dialog
    v-model:visible="MainDialogVisible"
    :style="'width: 50vw;'"
    :showCloseIcon="true"
    :header="headerDialog"
    maximizable="true"
  >
    <form>
      <Button
        v-if="temporaryData.length == 1"
        @click="showInfo(temporaryData[0])"
        label="Thông tin báo cáo"
        class="col-12 w-full h-3rem p-button-outlined"
        type="button"
        icon="pi pi-info-circle"
        v-tooltip="'Thông tin báo cáo'"
      ></Button>
      <div class="col-12">
        <div class="col-12 format-left">
          <div class="col-4">
            Nội dung phê duyệt <span class="redsao">(*)</span>
          </div>
          <Textarea
            id="report_name"
            v-model="review.messages"
            spellcheck="false"
            class="col-8"
            rows="2"
            :class="{
              'p-invalid': v$.messages.$invalid && submitted,
            }"
            autocomplete="off"
          />
        </div>
        <div
          style="display: flex"
          class="col-12 py-0"
          v-if="
            (v$.messages.$invalid && submitted) ||
            v$.messages.$pending.$response
          "
        >
          <div class="col-4 p-0 text-left"></div>
          <small class="col-8 p-0 p-error">
            <span class="col-12">{{
              v$.messages.required.$message
                .replace("Value", "Nội dung")
                .replace("is required", "không được để trống")
            }}</span>
          </small>
        </div>
        <div class="col-12 flex">
          <div class="col-4">Điểm đánh giá (0-100)</div>
          <InputNumber
            v-model="review.point"
            spellcheck="false"
            class="col-8 p-0"
            mode="decimal"
            showButtons
            :min="0"
            :max="100"
            :useGrouping="false"
            autocomplete="off"
          />
        </div>
      </div>
    </form>
    <template #footer>
      <div class="mt-2">
        <Button
          icon="pi pi-times"
          label="Đóng"
          @click="MainDialogVisible = false"
        />
        <Button
          icon="pi pi-check"
          label="Xác nhận"
          @click="saveData(!v$.$invalid)"
        />
      </div>
    </template>
  </Dialog>
  <Dialog
    v-model:visible="ProcessingVisible"
    :style="'width: 80vw;'"
    :modal="true"
    :showCloseIcon="true"
    header="Quá trình duyệt báo cáo"
  >
    <Processing :id="processing_id" />
    <template #footer>
      <Button
        icon="pi pi-times"
        class=""
        label="Đóng"
        @click="ProcessingVisible = false"
      />
    </template>
  </Dialog>
</template>
<style lang="scss" scoped>
.format-center {
  display: flex;
  justify-content: center;
  align-items: center;
  vertical-align: middle;
  text-align: center;
}

.format-right {
  display: flex;
  justify-content: flex-end;
  align-items: center;
  vertical-align: middle;
  text-align: center;
}

.format-left {
  display: flex;
  justify-content: flex-start;
  align-items: center;
  vertical-align: middle;
  text-align: left;
}
.btn-c-hover:hover {
  color: #0025f8 !important;
  background-color: white !important;
}
.name-hover:hover {
  color: #2196f3;
  cursor: pointer;
}
.format-left2 {
  justify-content: flex-start;
  align-items: center;
  vertical-align: middle;
  text-align: start;
}
.description {
  color: #aaa;
}
</style>
