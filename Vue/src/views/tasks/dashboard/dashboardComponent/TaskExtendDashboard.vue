<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { encr } from "../../../../util/function.js";
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
});
const datalists = ref([]);
let user = store.state.user;
const loadData = () => {
  options.value.loading = true;
  axios
    .post(
      // eslint-disable-next-line no-undef
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "task_extend_dashboard",
            par: [
              { par: "user_id", va: user.user_id },
              { par: "pn", va: options.value.PageNo },
              { par: "ps", va: options.value.PageSize },
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
        x.STT = options.value.PageNo * options.value.PageSize + (i + 1);
        x.creator_tooltip =
          "Người tạo gia hạn <br/>" +
          x.creator_full_name +
          "<br/>" +
          x.creator_positions +
          "<br/>" +
          (x.creator_department_name != null
            ? x.creator_department_name
            : x.creator_organiztion_name);
      });
      datalists.value = data;
      options.value.totalRecords = count[0].totalRecords;
      options.value.loading = false;
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
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
const selectedTasks = ref();
const checkDelList = ref(false);
watch(selectedTasks, () => {
  if (selectedTasks.value == null) {
    checkDelList.value = false;
  } else {
    checkDelList.value = true;
  }
});
const PositionSideBar = ref("right");

const showDetail = ref(false);
const selectedTaskID = ref();
const onNodeSelect = (id) => {
  showDetail.value = false;
  showDetail.value = true;
  selectedTaskID.value = id.task_id;
};
emitter.on("SideBar", (obj) => {
  showDetail.value = false;
  loadData();
});
emitter.on("psb", (obj) => {
  PositionSideBar.value = obj;
});
watch(showDetail, () => {
  if (showDetail.value == false) {
    loadData();
  }
});
const refresh = () => {
  loadData();
};
const DialogVisible = ref(false);

const listSelected = ref([]);

const openDialog = () => {
  DialogVisible.value = true;
  listSelected.value = [];
  listSelected.value = JSON.parse(JSON.stringify(selectedTasks.value));
  listSelected.value.forEach((x) => {
    x.accept_date = moment(x.extend_new_date).format("DD/MM/YYYY");
    x.minDate = x.current_end_date;
    x.is_agree = true;
  });
};
const SaveData = () => {
  listSelected.value.forEach((x) => {
    x.answer = x.answer ? x.answer.replace(/\n/g, "<br/>") : x.answer;
    axios
      .put(baseURL + "/api/TaskExtend/Upgrade_Status_TaskExtend", x, {
        headers: { Authorization: `Bearer ${store.getters.token}` },
      })
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Duyệt gia hạn công việc thành công!");
          DialogVisible.value = false;
          loadData();
        } else {
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
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!" + error,
          icon: "error",
          confirmButtonText: "OK",
        });
      });
  });
};
onMounted(() => {
  loadData();
});
</script>
<template>
  <div class="main-layout true flex-grow-1 p-2 main-div">
    <DataTable
      :value="datalists"
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
            <h3>Gia hạn công việc</h3>
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
            ></Button>
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
              size="75"
            />
          </div>
        </template>
      </Column>
      <Column
        header="Người xin gia hạn"
        field=""
        class="align-items-center justify-content-center text-center max-w-11rem word-break"
      >
        <template #body="data">
          <Avatar
            v-tooltip.right="{
              value: data.data.creator_tooltip,
              escape: true,
            }"
            v-bind:label="
              data.data.creator_avt
                ? ''
                : data.data.creator_full_name.split(' ').at(-1).substring(0, 1)
            "
            v-bind:image="basedomainURL + data.data.creator_avt"
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
        header="Gia hạn đến"
        field=""
        class="align-items-center justify-content-center text-center max-w-11rem word-break"
      >
        <template #body="data">
          <Chip
            :label="
              moment(new Date(data.data.extend_new_date)).format('DD/MM/YYYY')
            "
            icon="pi pi-calendar"
            class="new-date col-12 format-center"
          />
        </template>
      </Column>
      <Column
        header="Lý do"
        field=""
        class="word-break"
      >
        <template #body="data">
          <span v-html="data.data.reason"></span>
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
        header="Người xin gia hạn"
        field=""
        class="align-items-center justify-content-center text-center max-w-11rem word-break"
      >
        <template #body="data">
          <Avatar
            v-tooltip.right="{
              value: data.data.creator_tooltip,
              escape: true,
            }"
            v-bind:label="
              data.data.creator_avt
                ? ''
                : data.data.creator_full_name.split(' ').at(-1).substring(0, 1)
            "
            v-bind:image="basedomainURL + data.data.creator_avt"
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
        header="Gia hạn đến"
        field=""
        class="align-items-center justify-content-center text-center max-w-11rem word-break"
      >
        <template #body="data">
          <Calendar
            v-model="data.data.accept_date"
            selectionMode="single"
            dateFormat="dd/mm/yy"
            showIcon="true"
            :minDate="new Date(data.data.minDate)"
          />
        </template>
      </Column>
      <Column
        header="Trả lời"
        field=""
        class="word-break"
      >
        <template #body="data">
          <Textarea
            v-model="data.data.answer"
            class="w-full h-full"
          ></Textarea>
        </template>
      </Column>
      <Column
        header="Đồng ý"
        field=""
        class="max-w-6rem align-content-center align-items-center justify-content-center"
      >
        <template #body="data">
          <InputSwitch v-model="data.data.is_agree"></InputSwitch>
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
</template>

<style lang="scss" scoped>
.main-div {
  height: calc(100vh - 7rem);
}
.task-hover:hover {
  background-color: #f5f5f5;
  color: #2196f3;
}
.word-break {
  word-wrap: break-word;
}
.new-date {
  background-color: #ff8b4e;
  color: #fff;
}
</style>
