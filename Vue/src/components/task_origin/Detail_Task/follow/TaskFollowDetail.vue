<!-- eslint-disable vue/no-mutating-props -->
<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { required } from "@vuelidate/validators";
import { useToast } from "vue-toastification";
import { encr } from "../../../../util/function.js";
import moment from "moment";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import useVuelidate from "@vuelidate/core";
const cryoptojs = inject("cryptojs");
const emitter = inject("emitter");
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
  "#F4B2A3",
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);
let user = store.state.user;
const datalists = ref([]);
const options = ref({
  loading: true,
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
const props = defineProps({
  data: Object,
  isOpen: Boolean,
  closeDialogDetail: Function,
});
onMounted(() => {});
</script>
<template>
  <Dialog
    v-model:visible="props.isOpen"
    :style="'width:80vw;'"
    :showCloseIcon="true"
    :header="'Chi tiết quy trình'"
    @update:visible="closeDialogDetail"
    maximizable
    modal
  >
    <div
      class="col-12"
      style="overflow: auto"
    >
      <div class="col-12">
        <h3 class="align-items-center justify-content-center text-center">
          Thông tin chi tiết quy trình:
          <br />
          <h2
            class="py-0"
            style="color: #2196f3"
          >
            Bước: {{ props.data.is_step }} - {{ props.data.follow_name }}
          </h2>
        </h3>
        <Accordion :activeIndex="0">
          <AccordionTab header="Mô tả quy trình">
            <div
              class="max-h-15rem"
              style="overflow-y: auto"
            >
              <span
                v-html="props.data.description"
                v-if="props.data.description != null"
              ></span>
              <b v-else>Không có mô tả cho quy trình này!</b>
            </div>
          </AccordionTab>
        </Accordion>
      </div>
      <DataTable
        :value="props.data.childTask"
        showGridlines
        scrollable
        scrollHeight="flex"
        :reorderableColumns="true"
        @rowReorder="onRowReorder"
      >
        <!-- <template #header>
          <Toolbar class="w-full custoolbar">
            <template #end> </template>
          </Toolbar>
        </template> -->
        <Column
          field="task_name"
          header="Tên công việc"
          headerClass=" align-items-center justify-content-center"
          bodyClass="align-items-center"
        >
          <template #body="data">
            <div
              style="display: flex; flex-direction: column; padding: 5px"
              class="task-hover w-full"
            >
              <div style="line-height: ; display: flex">
                <span
                  v-tooltip="'Ưu tiên'"
                  v-if="data.data.is_prioritize"
                  style="margin-right: 5px"
                  ><i
                    style="color: orange"
                    class="pi pi-star-fill"
                  ></i>
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
                  >{{ data.data.task_name }}</span
                >
              </div>
              <div
                style="
                  font-size: 11px;
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
                      ? moment(new Date(data.data.end_date)).format(
                          "DD/MM/YYYY",
                        )
                      : null
                  }}
                </span>
              </div>
              <div
                v-if="data.data.p_id != null"
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
          field="progress"
          header="Tiến độ"
          class="align-items-center justify-content-center text-center max-w-6rem"
        >
          <template #body="data">
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
          </template>
        </Column>
        <Column
          header="Thành viên tham gia"
          class="align-items-center justify-content-center text-center max-w-6rem"
        >
          <template #body="data">
            <AvatarGroup
              class="align-items-center justify-content-center text-center"
            >
              <div
                v-for="(user, index) in data.data.users"
                :key="index"
              >
                <Avatar
                  @error="
                    $event.target.src =
                      basedomainURL + '/Portals/Image/nouser1.png'
                  "
                  v-if="user.is_type == 0 && user.STTGV == 0"
                  v-tooltip.right="{
                    value: user.tooltip,
                    escape: true,
                  }"
                  v-bind:label="
                    user.avt
                      ? ''
                      : user.full_name.split(' ').at(-1).substring(0, 1)
                  "
                  v-bind:image="basedomainURL + user.avt"
                  style="color: #ffffff; cursor: pointer"
                  :style="{
                    background: bgColor[index % 7],
                    border: '1px solid' + bgColor[index % 10],
                  }"
                  class="myTextAvatar"
                  size="large"
                  shape="circle"
                />
                <Avatar
                  @error="
                    $event.target.src =
                      basedomainURL + '/Portals/Image/nouser1.png'
                  "
                  v-if="user.is_type == 1 && user.STTTH == 0"
                  v-tooltip.right="{
                    value: user.tooltip,
                    escape: true,
                  }"
                  v-bind:label="
                    user.avt
                      ? ''
                      : user.full_name.split(' ').at(-1).substring(0, 1)
                  "
                  v-bind:image="basedomainURL + user.avt"
                  style="color: #ffffff; cursor: pointer"
                  :style="{
                    background: bgColor[index % 7],
                    border: '1px solid' + bgColor[index % 10],
                  }"
                  class="myTextAvatar"
                  size="large"
                  shape="circle"
                />
                <Avatar
                  @error="
                    $event.target.src =
                      basedomainURL + '/Portals/Image/nouser1.png'
                  "
                  v-if="user.is_type == 2 && user.STTDTH == 0"
                  v-tooltip.right="{
                    value: user.tooltip,
                    escape: true,
                  }"
                  v-bind:label="
                    user.avt
                      ? ''
                      : user.full_name.split(' ').at(-1).substring(0, 1)
                  "
                  v-bind:image="basedomainURL + user.avt"
                  style="color: #ffffff; cursor: pointer"
                  :style="{
                    background: bgColor[index % 7],
                    border: '1px solid' + bgColor[index % 10],
                  }"
                  class="myTextAvatar"
                  size="large"
                  shape="circle"
                />
                <Avatar
                  @error="
                    $event.target.src =
                      basedomainURL + '/Portals/Image/nouser1.png'
                  "
                  v-if="user.is_type == 3 && user.STTTD == 0"
                  v-tooltip.right="{
                    value: user.tooltip,
                    escape: true,
                  }"
                  v-bind:label="
                    user.avt
                      ? ''
                      : user.full_name.split(' ').at(-1).substring(0, 1)
                  "
                  v-bind:image="basedomainURL + user.avt"
                  style="color: #ffffff; cursor: pointer"
                  :style="{
                    background: bgColor[index % 7],
                    border: '1px solid' + bgColor[index % 10],
                  }"
                  class="myTextAvatar"
                  size="large"
                  shape="circle"
                />
              </div>
              <Avatar
                @error="
                  $event.target.src =
                    basedomainURL + '/Portals/Image/nouser1.png'
                "
                v-if="data.data.users.length > 4"
                v-tooltip.right="{
                  value:
                    'và ' +
                    (data.data.users.length - 4) +
                    ' người khác tham gia',
                }"
                :label="'+' + (data.data.users.length - 4)"
                style="color: #ffffff; cursor: pointer; font-size: 1rem"
                :style="{
                  background: bgColor[index % 7],
                  border: '1px solid' + bgColor[index % 10],
                }"
                class="myTextAvatar"
                size="large"
                shape="circle"
              ></Avatar>
            </AvatarGroup>
          </template>
        </Column>
        <Column
          header="Hạn xử lý"
          field=""
          class="align-items-center justify-content-center text-center max-w-5rem"
        >
          <template #body="data">
            <div
              v-if="data.data.is_deadline == true"
              style="
                background-color: #fff2d7;
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
          field=""
          class="align-items-center justify-content-center text-center max-w-4rem"
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
          field=""
          class="align-items-center justify-content-center text-center max-w-4rem"
        >
          <template #body="data">
            <Chip
              class="px-3 py-1"
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
            class="row col-12 align-items-center justify-content-center p-4 text-center m-auto"
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
  </Dialog>
</template>

<style lang="scss" scoped>
::v-deep(.myTextAvatar) {
  .p-avatar-text {
    font-size: 2rem;
  }
}
</style>
