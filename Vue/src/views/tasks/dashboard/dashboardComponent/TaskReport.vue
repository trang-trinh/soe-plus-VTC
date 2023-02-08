<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { encr } from "../../../../util/function.js";
import moment from "moment";
import DetailedWork from "../../../../components/task_origin/DetailedWork.vue";
const cryoptojs = inject("cryptojs");
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
const width = window.screen.width;
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
const user = store.state.user;
const listTask = ref([]);
const first = ref(0);
const loadData = () => {
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
  first.value = 0;
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
const openDialog = () => {
  DialogVisible.value = true;
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
      @filter="onFilter($event)"
      @sort="onSort($event)"
      :totalRecords="options.totalRecords"
      dataKey="task_id"
      :rowHover="true"
      :showGridlines="true"
      paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
      :rowsPerPageOptions="[20, 30, 50, 100, 200]"
      @row-dblclick="showInfo($event.data)"
    >
      <template #header>
        <Toolbar class="w-full custoolbar">
          <template #start>
            <h3>Báo cáo công việc cá nhân thực hiện</h3>
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
        header="Người giao"
        field="creator"
        class="align-items-center justify-content-center text-center max-w-8rem"
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
              background: bgColor[index % 7],
              border: '1px solid' + bgColor[index % 7],
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
          <span v-if="data.data.progress > 0">
            <ProgressBar
              :value="data.data.progress"
              :show-value="true"
          /></span>
          <span v-else>0%</span>
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
            {{ moment(data.data.end_date).format("DD/MM/YYYY") }}
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
    </DataTable>
  </div>
  <Dialog
    :visible="DialogVisible"
    :header="'Gửi báo cáo công việc'"
  ></Dialog>
  <DetailedWork
    v-if="showDetail == true && selectedTaskID != null"
    :isShow="showDetail"
    :id="selectedTaskID"
    :turn="0"
  >
  </DetailedWork>
</template>

<style lang="scss" scoped>
.main-div {
  height: calc(100vh - 7rem);
}
</style>
