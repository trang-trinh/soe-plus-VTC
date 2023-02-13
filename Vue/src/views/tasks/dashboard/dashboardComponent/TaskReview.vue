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
        // x.contents =
        //   "Vivamus suscipit tortor eget felis porttitor volutpat. Vivamus suscipit tortor eget felis porttitor volutpat. Vestibulum ac diam sit amet quam vehicula elementum sed sit amet dui. Vestibulum ac diam sit amet quam vehicula elementum sed sit amet dui. Quisque velit nisi, pretium ut lacinia in, elementum id enim. Donec sollicitudin molestie malesuada. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; Donec velit neque, auctor sit amet aliquam vel, ullamcorper sit amet ligula. Nulla quis lorem ut libero malesuada feugiat. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; Donec velit neque, auctor sit amet aliquam vel, ullamcorper sit amet ligula. Sed porttitor lectus nibh. Mauris blandit aliquet elit, eget tincidunt nibh pulvinar a. Vivamus suscipit tortor eget felis porttitor volutpat. Praesent sapien massa, convallis a pellentesque nec, egestas non nisi. Vivamus magna justo, lacinia eget consectetur sed, convallis at tellus. Nulla porttitor accumsan tincidunt. Curabitur non nulla sit amet nisl tempus convallis quis ac lectus. Vestibulum ac diam sit amet quam vehicula elementum sed sit amet dui. Curabitur aliquet quam id dui posuere blandit. Donec sollicitudin molestie malesuada. Mauris blandit aliquet elit, eget tincidunt nibh pulvinar a.<br/><br/><br/><br/>Proin eget tortor risus. Proin eget tortor risus. Sed porttitor lectus nibh. Vivamus suscipit tortor eget felis porttitor volutpat. Nulla quis lorem ut libero malesuada feugiat. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; Donec velit neque, auctor sit amet aliquam vel, ullamcorper sit amet ligula. Nulla quis lorem ut libero malesuada feugiat. Donec rutrum congue leo eget malesuada. Curabitur non nulla sit amet nisl tempus convallis quis ac lectus. Donec rutrum congue leo eget malesuada. Nulla porttitor accumsan tincidunt. Praesent sapien massa, convallis a pellentesque nec, egestas non nisi. Vivamus suscipit tortor eget felis porttitor volutpat. Pellentesque in ipsum id orci porta dapibus. Vivamus magna justo, lacinia eget consectetur sed, convallis at tellus. Sed porttitor lectus nibh. Proin eget tortor risus. Nulla porttitor accumsan tincidunt. Pellentesque in ipsum id orci porta dapibus. Vestibulum ac diam sit amet quam vehicula elementum sed sit amet dui.<br/><br/><br/><br/>Mauris blandit aliquet elit, eget tincidunt nibh pulvinar a. Vivamus suscipit tortor eget felis porttitor volutpat. Nulla quis lorem ut libero malesuada feugiat. Proin eget tortor risus. Vivamus suscipit tortor eget felis porttitor volutpat. Nulla quis lorem ut libero malesuada feugiat. Donec sollicitudin molestie malesuada. Praesent sapien massa, convallis a pellentesque nec, egestas non nisi. Vivamus suscipit tortor eget felis porttitor volutpat. Curabitur arcu erat, accumsan id imperdiet et, porttitor at sem. Nulla porttitor accumsan tincidunt. Cras ultricies ligula sed magna dictum porta. Praesent sapien massa, convallis a pellentesque nec, egestas non nisi. Curabitur aliquet quam id dui posuere blandit. Curabitur non nulla sit amet nisl tempus convallis quis ac lectus. Quisque velit nisi, pretium ut lacinia in, elementum id enim. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Proin eget tortor risus. Sed porttitor lectus nibh. Curabitur aliquet quam id dui posuere blandit.<br/><br/><br/><br/>Sed porttitor lectus nibh. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus suscipit tortor eget felis porttitor volutpat. Proin eget tortor risus. Vivamus magna justo, lacinia eget consectetur sed, convallis at tellus. Vivamus suscipit tortor eget felis porttitor volutpat. Nulla porttitor accumsan tincidunt. Curabitur non nulla sit amet nisl tempus convallis quis ac lectus. Proin eget tortor risus. Donec rutrum congue leo eget malesuada. Curabitur non nulla sit amet nisl tempus convallis quis ac lectus. Vivamus suscipit tortor eget felis porttitor volutpat. Vestibulum ac diam sit amet quam vehicula elementum sed sit amet dui. Nulla quis lorem ut libero malesuada feugiat. Vestibulum ac diam sit amet quam vehicula elementum sed sit amet dui. Curabitur aliquet quam id dui posuere blandit. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; Donec velit neque, auctor sit amet aliquam vel, ullamcorper sit amet ligula. Curabitur arcu erat, accumsan id imperdiet et, porttitor at sem. Curabitur aliquet quam id dui posuere blandit. Curabitur non nulla sit amet nisl tempus convallis quis ac lectus.<br/><br/><br/><br/>Curabitur arcu erat, accumsan id imperdiet et, porttitor at sem. Nulla porttitor accumsan tincidunt. Mauris blandit aliquet elit, eget tincidunt nibh pulvinar a. Nulla quis lorem ut libero malesuada feugiat. Curabitur aliquet quam id dui posuere blandit. Vivamus suscipit tortor eget felis porttitor volutpat. Pellentesque in ipsum id orci porta dapibus. Nulla quis lorem ut libero malesuada feugiat. Quisque velit nisi, pretium ut lacinia in, elementum id enim. Donec sollicitudin molestie malesuada. Donec rutrum congue leo eget malesuada. Donec rutrum congue leo eget malesuada. Nulla quis lorem ut libero malesuada feugiat. Sed porttitor lectus nibh. Curabitur non nulla sit amet nisl tempus convallis quis ac lectus. Pellentesque in ipsum id orci porta dapibus. Quisque velit nisi, pretium ut lacinia in, elementum id enim. Praesent sapien massa, convallis a pellentesque nec, egestas non nisi. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; Donec velit neque, auctor sit amet aliquam vel, ullamcorper sit amet ligula. Quisque velit nisi, pretium ut lacinia in, elementum id enim.";
        // x.request = x.contents;
        // x.difficult = x.contents;
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
  first.value = 0;
  options.value.loading = true;
  loadData();
};
watch(selectedTasks, () => {
  if (selectedTasks.value.length > 0) {
    checkDelList.value = true;
  } else checkDelList.value = false;
});
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
            <h3>Đánh giá công việc</h3>
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
          <span v-if="data.data.progress > 0">
            <ProgressBar
              :value="data.data.progress"
              :show-value="true"
          /></span>
          <span v-else>0%</span>
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
        header="Nội dung báo cáo"
        field=""
        class="align-items-center justify-content-center p-2 max-w-30rem"
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
            <div class="col-12">
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
            {{ data.data }}
          </div>
        </template>
      </Column>
      <Column
        header="Tiến độ thực hiện"
        field=""
        class="align-items-center justify-content-center text-center max-w-10rem"
      >
        <template #body="data">
          <div></div>
        </template>
      </Column>
    </DataTable>
  </div>
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
