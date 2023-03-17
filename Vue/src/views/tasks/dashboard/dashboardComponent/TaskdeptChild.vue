<script setup>
import { ref, inject, onMounted, watch, h } from "vue";
import { useToast } from "vue-toastification";
import { encr } from "../../../../util/function.js";
import moment from "moment";
import TaskdeptChild2 from "./TaskdeptChild.vue";
import DetailedWork from "../../../../components/task_origin/DetailedWork.vue";
import TaskChart from "./Chart/TaskChart.vue";
import { FilterMatchMode, FilterOperator } from "primevue/api";
const emitter = inject("emitter");

const props = defineProps({
  data: Array,
  option: Object,
});
const expandedRows = ref([]);
const expandedRowGroups = ref([]);
onMounted(() => {});
const showDetail = ref(false);
const selectedTaskID = ref();
const onNodeSelect = (id) => {
  showDetail.value = false;
  showDetail.value = true;
  selectedTaskID.value = id.task_id;
};
emitter.on("SideBar", (obj) => {
  showDetail.value = obj;
  props.func();
});
watch(showDetail, () => {
  if (showDetail.value == false) {
    props.func();
  }
});
const PositionSideBar = ref("right");
emitter.on("psb", (obj) => {
  PositionSideBar.value = obj;
});
</script>

<template>
  <div class="w-full h-40rem">
    <DataTable
      v-if="props.data.children.length > 0"
      :value="props.data.children"
      responsiveLayout="scroll"
      :scrollable="true"
      scrollHeight="flex"
      :loading="props.option.loading"
      dataKey="organization_id"
      :rowHover="true"
      :showGridlines="true"
      v-model:expandedRows="expandedRows"
    >
      <template #header>
        <h3>
          Phòng ban trực thuộc :<span class="pl-2">{{
            props.data.organization_name
          }}</span>
        </h3>
      </template>
      <Column
        :expander="true"
        class="max-w-3rem"
      />
      <Column
        header="Tên phòng ban"
        field="organization_name"
        headerClass="justify-content-center"
      ></Column>
      <Column
        header="Tiến độ"
        field=""
        class="align-items-center justify-content-center max-w-8rem"
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
        header="Thông tin chung"
        field=""
        class="align-items-center justify-content-center max-w-30rem"
      >
        <template #body="data">
          <Button
            class="font-bold p-button-text border-1 border-round-xl border-gray-200 w-auto justify-content-center"
            style="background-color: #bbbbbb; color: white"
          >
            <span>
              <span class="font-bold"> {{ data.data.total }}</span>
              <br />
              <span class="font-bold">Tất cả</span>
            </span>
          </Button>
          <Button
            class="font-bold p-button-text border-1 border-round-xl border-gray-200 w-auto justify-content-center"
            style="background-color: #2196f3; color: white"
          >
            <span>
              <span class="font-bold"> {{ data.data.doing }}</span>
              <br />
              <span class="font-bold">Đang làm</span>
            </span>
          </Button>
          <Button
            class="font-bold p-button-text border-1 border-round-xl border-gray-200 w-auto justify-content-center"
            style="background-color: #6fbf73; color: white"
          >
            <span>
              <span class="font-bold"> {{ data.data.finished }}</span>
              <br />
              <span class="font-bold">Hoàn thành</span>
            </span>
          </Button>
          <Button
            class="font-bold p-button-text border-1 border-round-xl border-gray-200 w-auto justify-content-center"
            style="background-color: #f00000; color: white"
          >
            <span>
              <span class="font-bold"> {{ data.data.expired }}</span>
              <br />
              <span class="font-bold">Quá hạn</span>
            </span>
          </Button>
        </template>
      </Column>
      <template #expansion="slotProps">
        <span class="w-full">
          <div class="col-12">
            <DataTable
              :value="slotProps.data.task_info"
              responsiveLayout="scroll"
              rowGroupMode="subheader"
              groupRowsBy="p_id"
              :scrollable="true"
              scrollHeight="60vh"
              :expandableRowGroups="true"
              v-model:expandedRowGroups="expandedRowGroups"
              ><template #header>
                <h3>
                  Công việc của :<span class="pl-2">{{
                    slotProps.data.organization_name
                  }}</span>
                </h3>
              </template>
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
                </div>
              </template>
              <template #groupheader="slotProps2">
                <span class="image-text">
                  {{
                    slotProps2.data.project_name ??
                    "Công việc không thuộc dự án"
                  }}
                  ({{ count1(slotProps.data, slotProps2.data.project_name) }})
                </span>
              </template>
              <Column
                field="task_name"
                header="Tên công việc"
                headerClass=" align-items-center justify-content-center"
                bodyClass="align-items-center"
              >
                <template #body="data">
                  <div
                    style="display: flex; flex-direction: column; padding: 5px"
                    @click="onNodeSelect(data.data)"
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
                            ? moment(new Date(data.data.end_date)).format(
                                "DD/MM/YYYY",
                              )
                            : null
                        }}
                      </span>
                    </div>
                    <div
                      v-if="data.data.p_id != -1"
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
                class="align-items-center justify-content-center text-center max-w-8rem"
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
                    :size="size"
                  />
                </template>
              </Column>
              <Column
                header="Người giao việc"
                class="align-items-center justify-content-center text-center max-w-10rem"
              >
                <template #body="data">
                  <Avatar
                    @error="
                      $event.target.src =
                        basedomainURL + '/Portals/Image/nouser1.png'
                    "
                    v-tooltip.bottom="{
                      value: data.data.creator_tooltip,
                      escape: true,
                    }"
                    v-bind:label="
                      data.data.creator.avatar
                        ? ''
                        : data.data.creator.full_name
                            .split(' ')
                            .at(-1)
                            .substring(0, 1)
                    "
                    v-bind:image="basedomainURL + data.data.creator.avatar"
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
                header="Hạn xử lý"
                field=""
                class="align-items-center justify-content-center text-center max-w-10rem"
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
                    {{ moment(data.data.end_date).format("DD/MM/YYYY") }}
                  </div>
                </template>
              </Column>
              <Column
                header="Thời gian xử lý"
                field=""
                class="align-items-center justify-content-center text-center max-w-10rem"
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
                class="align-items-center justify-content-center text-center max-w-10rem"
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
            </DataTable>
          </div>
          <div
            class="col-12"
            v-if="slotProps.data.children != null"
          >
            <TaskdeptChild2
              :data="slotProps.data"
              :option="props.option"
            ></TaskdeptChild2></div
        ></span>
      </template>
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
            ? ' 65vw'
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
.task-hover:hover {
  background-color: #f5f5f5;
  color: #2196f3;
}
</style>
