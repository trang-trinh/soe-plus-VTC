<!-- eslint-disable vue/no-mutating-props -->
<script setup>
import { ref, inject, onMounted } from "vue";
import moment from "moment";
import DetailedWork from "../../DetailedWork.vue";

// eslint-disable-next-line no-undef
const optionsView = ref([
  { icon: "pi pi-table", value: 1, tooltip: "Bảng" },
  { icon: "pi pi-th-large", value: 2, tooltip: "Lưới" },
]);
const expandedRows = ref([]);
const props = defineProps({
  data: Object,
  isOpen: Boolean,
  closeDialogDetail: Function,
  rowReorder: Function,
  memberType: Intl,
});
const closeDetail = () => {
  showDetail.value = false;
  selectedTaskID.value = null;
};
const type_display = ref();
const PositionSideBar = ref("right");
const emitter = inject("emitter");
const showDetail = ref(false);
const selectedTaskID = ref();
const onNodeSelect = (id) => {
  showDetail.value = false;
  showDetail.value = true;
  selectedTaskID.value = id;
};

onMounted(() => {
  type_display.value = 1;
});
</script>
<template>
  <Dialog
    v-model:visible="props.isOpen"
    :style="'width:90vw;height:100vh'"
    :showCloseIcon="true"
    :header="'Chi tiết quy trình'"
    @update:visible="props.closeDialogDetail"
    maximizable
    modal
    keepInViewPort
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
            {{ props.data.follow_name }}
          </h2>
        </h3>
        <Accordion :activeIndex="0">
          <AccordionTab>
            <template #header>
              <div class="flex justify-content-between w-full">
                <div class="flex left-0 align-items-center">
                  Thông tin chung
                </div>
                <div class="relative right-0">
                  <SelectButton
                    v-model="type_display"
                    :options="optionsView"
                    optionValue="value"
                    aria-labelledby="basic"
                  >
                    <template #option="slotProps">
                      <i
                        :class="slotProps.option.icon"
                        v-tooltip="slotProps.option.tooltip"
                      ></i>
                    </template>
                  </SelectButton>
                </div>
              </div>
            </template>
            <div
              class="max-h-15rem"
              style="overflow-y: auto"
            >
              <div
                class="col-12 flex"
                v-if="props.data.start_date || props.data.end_date"
              >
                <div
                  class="col-6"
                  v-if="props.data.start_date"
                >
                  Ngày bắt đầu(dự kiến):
                  <span class="pl-2 text-xl text-blue-600">{{
                    moment(new Date(props.data.start_date)).format(
                      "HH:mm DD/MM/YYYY",
                    )
                  }}</span>
                </div>
                <div
                  class="col-6"
                  v-if="props.data.end_date"
                >
                  Ngày kết thúc(dự kiến):
                  <span class="pl-2 text-xl text-blue-600">{{
                    moment(new Date(props.data.end_date)).format(
                      "HH:mm DD/MM/YYYY",
                    )
                  }}</span>
                </div>
              </div>
              <div
                class="col-12 flex"
                v-if="props.data.start_real_date || props.data.end_real_date"
              >
                <div
                  class="col-6"
                  v-if="props.data.start_real_date"
                >
                  Ngày bắt đầu:
                  <span class="pl-2 text-xl text-blue-600">{{
                    moment(new Date(props.data.start_real_date)).format(
                      "HH:mm DD/MM/YYYY",
                    )
                  }}</span>
                </div>
                <div
                  class="col-6"
                  v-if="props.data.end_real_date"
                >
                  Ngày kết thúc:
                  <span class="pl-2 text-xl text-blue-600">{{
                    moment(new Date(props.data.end_real_date)).format(
                      "HH:mm DD/MM/YYYY",
                    )
                  }}</span>
                </div>
              </div>
              <div class="col-12 flex">
                <div class="col-4">
                  Trọng số: <span class="pl-2">{{ props.data.weight }}</span>
                </div>
                <div class="col-4">
                  Trạng thái:
                  <span
                    class=""
                    :style="{
                      background: props.data.status_display.bg_color,
                      color: props.data.status_display.text_color,
                      padding: '2px 8px',
                      border: '1px solid' + props.data.status_display.bg_color,
                      borderRadius: '5px',
                    }"
                  >
                    {{ props.data.status_display.label }}
                  </span>
                </div>
                <div class="col-4">
                  Trình tự thực hiện:
                  <span
                    class=""
                    :style="{
                      background: props.data.type_display.bg_color,
                      color: props.data.type_display.text_color,
                      padding: '2px 8px',
                      border: '1px solid' + props.data.type_display.bg_color,
                      borderRadius: '5px',
                    }"
                  >
                    {{ props.data.type_display.label }}
                  </span>
                </div>
              </div>
              <div class="col-12 flex">
                <div class="col-1">Mô tả quy trình:</div>
                <div
                  class="col-11"
                  v-html="props.data.description"
                  v-if="props.data.description != null"
                ></div>
                <b
                  class="col-11"
                  v-else
                  >Không có mô tả cho quy trình này!</b
                >
              </div>
            </div>
          </AccordionTab>
        </Accordion>
      </div>

      <DataTable
        v-if="type_display == 1"
        :value="props.data.task_follow_step"
        v-model:expandedRows="expandedRows"
      >
        <Column
          expander
          class="w-1rem"
        />
        <Column
          header="Thứ tự"
          field="is_step"
          class="justify-content-center align-items-center text-center w-5rem"
        >
        </Column>
        <Column
          header="Tên bước"
          field="step_name"
          header-class="justify-content-center align-items-center text-center"
        >
          <template #body="data">
            <div>
              <span class="font-bold text-xl">
                {{ data.data.step_name }}
              </span>
              <br />
              <span v-if="data.data.start_date">
                {{
                  moment(new Date(data.data.start_date)).format("DD/MM/YYYY")
                }}-
              </span>

              <span v-if="data.data.is_deadline == true">
                {{ moment(new Date(data.data.end_date)).format("DD/MM/YYYY") }}
              </span>
            </div></template
          ></Column
        >
        <Column
          header="Công việc"
          field="step_name"
          class="justify-content-center align-items-center text-center w-7rem"
        >
          <template #body="data">
            <div>
              {{ data.data.countTaskFinished }} /
              {{ data.data.countTask }}
            </div>
            <div v-if="data.data.TaskProgress > 0">
              <ProgressBar :value="data.data.TaskProgress" />
            </div>
            <div
              class="pt-2"
              v-else
            >
              0%
            </div>
          </template>
        </Column>
        <Column
          header="Trình tự thực hiện"
          field="task_id_follow"
          class="justify-content-center align-items-center text-center w-12rem"
        >
          <template #body="data">
            <span
              :style="{
                background: data.data.type_display.bg_color,
                color: data.data.type_display.text_color,
                padding: '5px 10px',
                border: '1px solid' + data.data.type_display.bg_color,
                borderRadius: '5px',
              }"
            >
              {{ data.data.type_display.label }}
            </span>
          </template></Column
        >
        <Column
          header="Trạng thái"
          field="task_id_follow"
          class="justify-content-center align-items-center text-center w-14rem"
        >
          <template #body="data">
            <span
              :style="{
                background: data.data.status_display.bg_color,
                color: data.data.status_display.text_color,
                padding: '5px 10px',
                border: '1px solid' + data.data.status_display.bg_color,
                borderRadius: '5px',
              }"
            >
              {{ data.data.status_display.label }}
            </span>
          </template></Column
        >
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
        <template #expansion="slotProps">
          <div class="w-full">
            <Toolbar class="w-full custoolbar">
              <template #start>
                <span>
                  Thông tin bước:
                  <span class="px-1 font-bold line-height-3">{{
                    slotProps.data.step_name
                  }}</span>
                  ádsd
                </span>
              </template>
            </Toolbar>
            <Accordion :activeIndex="0">
              <AccordionTab header="Thông tin chung">
                <div
                  class="max-h-15rem"
                  style="overflow-y: auto"
                >
                  <div
                    class="col-12 flex"
                    v-if="slotProps.data.start_date || slotProps.data.end_date"
                  >
                    <div
                      class="col-6"
                      v-if="slotProps.data.start_date"
                    >
                      Ngày bắt đầu(dự kiến):
                      <span class="pl-2 text-xl text-blue-600">{{
                        moment(new Date(slotProps.data.start_date)).format(
                          "HH:mm DD/MM/YYYY",
                        )
                      }}</span>
                    </div>
                    <div
                      class="col-6"
                      v-if="slotProps.data.end_date"
                    >
                      Ngày kết thúc(dự kiến):
                      <span class="pl-2 text-xl text-blue-600">{{
                        moment(new Date(slotProps.data.end_date)).format(
                          "HH:mm DD/MM/YYYY",
                        )
                      }}</span>
                    </div>
                  </div>
                  <div
                    class="col-12 flex"
                    v-if="
                      slotProps.data.start_real_date ||
                      slotProps.data.end_real_date
                    "
                  >
                    <div
                      class="col-6"
                      v-if="slotProps.data.start_real_date"
                    >
                      Ngày bắt đầu:
                      <span class="pl-2 text-xl text-blue-600">{{
                        moment(new Date(slotProps.data.start_real_date)).format(
                          "HH:mm DD/MM/YYYY",
                        )
                      }}</span>
                    </div>
                    <div
                      class="col-6"
                      v-if="slotProps.data.end_real_date"
                    >
                      Ngày kết thúc:
                      <span class="pl-2 text-xl text-blue-600">{{
                        moment(new Date(slotProps.data.end_real_date)).format(
                          "HH:mm DD/MM/YYYY",
                        )
                      }}</span>
                    </div>
                  </div>
                  <div class="col-12 flex">
                    <div class="col-6">
                      Trạng thái:
                      <span
                        class=""
                        :style="{
                          background: slotProps.data.status_display.bg_color,
                          color: slotProps.data.status_display.text_color,
                          padding: '2px 8px',
                          border:
                            '1px solid' +
                            slotProps.data.status_display.bg_color,
                          borderRadius: '5px',
                        }"
                      >
                        {{ slotProps.data.status_display.label }}
                      </span>
                    </div>
                    <div class="col-6">
                      Trình tự thực hiện:
                      <span
                        class=""
                        :style="{
                          background: slotProps.data.type_display.bg_color,
                          color: slotProps.data.type_display.text_color,
                          padding: '2px 8px',
                          border:
                            '1px solid' + slotProps.data.type_display.bg_color,
                          borderRadius: '5px',
                        }"
                      >
                        {{ slotProps.data.type_display.label }}
                      </span>
                    </div>
                  </div>
                  <div class="col-12 flex">
                    <div class="col-1">Mô tả:</div>

                    <div
                      class="col-11"
                      v-html="slotProps.data.description"
                      v-if="
                        slotProps.data.description != null &&
                        slotProps.data.description != ''
                      "
                    ></div>
                    <b
                      class="col-11"
                      v-else
                      >Không có mô tả cho quy trình này!</b
                    >
                  </div>
                </div>
              </AccordionTab>
            </Accordion>

            <DataTable
              :value="slotProps.data.task_info"
              rowHover
              @row-click="onNodeSelect($event.data.task_id)"
            >
              <Column
                header="STT"
                field="step"
                class="justify-content-center align-items-center text-center w-5rem"
              ></Column>
              <Column
                header="Tên công việc"
                field="task_id_follow"
                header-class="justify-content-center align-items-center text-center"
              >
                <template #body="data">
                  <div>
                    <span class="font-bold text-xl">
                      {{ data.data.task_name }}
                    </span>
                    <br />
                    <span>
                      {{
                        moment(new Date(data.data.start_date)).format(
                          "DD/MM/YYYY",
                        )
                      }}
                    </span>
                    -
                    <span v-if="data.data.is_deadline == true">
                      {{
                        moment(new Date(data.data.end_date)).format(
                          "DD/MM/YYYY",
                        )
                      }}
                    </span>
                  </div>
                </template>
              </Column>
              <Column
                header="Tiến độ"
                field="task_id_follow"
                class="justify-content-center align-items-center text-center w-8rem"
              >
                <template #body="data">
                  <div v-if="data.data.progress > 0">
                    <ProgressBar :value="data.data.progress" />
                  </div>
                  <div
                    class="pt-2"
                    v-else
                  >
                    0%
                  </div>
                </template></Column
              >
              <Column
                header="Trạng thái"
                field="task_id_follow"
                class="justify-content-center align-items-center text-center w-14rem"
              >
                <template #body="data">
                  <span
                    :style="{
                      background: data.data.status_display.bg_color,
                      color: data.data.status_display.text_color,
                      padding: '5px 10px',
                      border: '1px solid' + data.data.status_display.bg_color,
                      borderRadius: '5px',
                    }"
                  >
                    {{ data.data.status_display.text }}
                  </span>
                </template></Column
              >

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
        </template>
      </DataTable>

      <div
        v-if="type_display == 2"
        class="w-full flex"
        style="overflow-y: auto"
      >
        <div
          class="flex align-items-center justify-content-center"
          v-for="(item, index) in props.data.task_follow_step"
          :key="index"
        >
          <Card class="align-items-start m-3 w-45rem h-45rem card-custom">
            <template #header>
              <div
                class="w-full align-items-center justify-content-center flex"
              >
                <span
                  class="flex w-3rem h-3rem align-items-center justify-content-center text-center border-circle z-1 shadow-1 text-2xl font-bold text-blue-500"
                >
                  {{ item.is_step }}
                </span>
              </div>
            </template>
            <template #content>
              <div class="align-items-center justify-content-center flex my-2">
                <span class="font-bold text-xl">
                  Bước:
                  <span class="text-blue-500"> {{ item.step_name }}</span>
                </span>
              </div>
              <div class="align-items-center justify-content-center flex my-2">
                <Chip
                  class="mx-2"
                  :label="item.status_display.label"
                  :style="{
                    color: item.status_display.text_color,
                    background: item.status_display.bg_color,
                  }"
                />
                <Chip
                  class="mx-2"
                  :label="item.type_display.label"
                  :style="{
                    color: item.type_display.text_color,
                    background: item.type_display.bg_color,
                  }"
                />
              </div>
              <div class="col-12 flex">
                <span class="col-2">Mô tả bước:</span>
                <div
                  class="col-10 max-h-10rem overflow-y-auto"
                  v-if="item.description"
                  v-html="item.description"
                ></div>
                <b
                  class="col-10"
                  v-else
                  >Không có mô tả cho bước này!</b
                >
              </div>
              <div class="col-12 flex">
                <div
                  class="col-6"
                  v-if="item.start_date"
                >
                  Ngày bắt đầu(dự kiến):
                  <span class="pl-2 text-blue-600">
                    {{
                      moment(new Date(item.start_date)).format(
                        "HH:mm DD/MM/YYYY",
                      )
                    }}</span
                  >
                </div>
                <div
                  class="col-6"
                  v-if="item.end_date"
                >
                  Ngày kết thúc(dự kiến):
                  <span class="pl-2 text-blue-600">
                    {{
                      moment(new Date(item.end_date)).format("HH:mm DD/MM/YYYY")
                    }}
                  </span>
                </div>
              </div>
              <div class="col-12 flex">
                <div
                  class="col-6"
                  v-if="item.start_real_date"
                >
                  Ngày bắt đầu:
                  <span class="pl-2 text-blue-600">
                    {{
                      moment(new Date(item.start_real_date)).format(
                        "HH:mm DD/MM/YYYY",
                      )
                    }}</span
                  >
                </div>
                <div
                  class="col-6"
                  v-if="item.end_real_date"
                >
                  Ngày kết thúc:
                  <span class="pl-2 text-blue-600">
                    {{
                      moment(new Date(item.end_real_date)).format(
                        "HH:mm DD/MM/YYYY",
                      )
                    }}
                  </span>
                </div>
              </div>
              <div
                v-if="item.task_id_follow != null"
                class="h-30rem overflow-y-auto grid justify-content-center"
              >
                <div
                  class="m-2"
                  v-for="(item2, index) in item.task_info"
                  :key="index"
                >
                  <Card
                    class="bg-bluegray-50 w-30rem"
                    @click="onNodeSelect(item2.task_id)"
                  >
                    <template #header>
                      <div
                        class="w-full align-items-center justify-content-center flex"
                      >
                        <span
                          class="bg-white flex w-2rem h-2rem align-items-center justify-content-center text-center border-circle z-1 shadow-1 text-2xl font-bold text-blue-500"
                        >
                          {{ item2.step }}
                        </span>
                      </div></template
                    >
                    <template #title>
                      <span class="font-bold text-xl">
                        Công việc:
                        <span class="text-blue-700">
                          {{ item2.task_name }}</span
                        >
                      </span>
                    </template>
                    <template #subtitle>
                      <div
                        class="flex justify-content-center align-items-center"
                      >
                        <span
                          v-if="item2.start_date || item2.end_date"
                          style="color: #98a9bc"
                        >
                          <i
                            style="margin-right: 5px"
                            class="pi pi-calendar"
                          >
                          </i>
                          {{
                            item2.start_date
                              ? moment(new Date(item2.start_date)).format(
                                  "DD/MM/YYYY",
                                )
                              : null
                          }}
                          -
                          {{
                            item2.end_date
                              ? moment(new Date(item2.end_date)).format(
                                  "DD/MM/YYYY",
                                )
                              : null
                          }}
                        </span>
                      </div>
                    </template>
                    <template #content>
                      <div
                        class="w-50 flex justify-content-center align-items-center"
                      >
                        <span
                          class=""
                          :style="{
                            background: item2.status_display.bg_color,
                            color: item2.status_display.text_color,
                            padding: '2px 8px',
                            border: '1px solid' + item2.status_display.bg_color,
                            borderRadius: '5px',
                          }"
                        >
                          {{ item2.status_display.text }}
                        </span>
                      </div>
                    </template>
                    <template #footer>
                      <div
                        v-if="item2.progress != 0"
                        style="width: 100%"
                      >
                        <ProgressBar :value="item2.progress ?? 0" /></div
                    ></template>
                  </Card>
                  <icon
                    v-tooltip="'Tuần tự'"
                    class="py-2 pi pi-arrow-down font-bold text-2xl flex justify-content-center"
                    v-if="item.type == 1 && index < item.task_info.length - 1"
                  ></icon>
                  <icon
                    class="py-2 pi pi-sort-alt font-bold text-2xl flex justify-content-center"
                    v-tooltip="'Song song'"
                    v-if="item.type == 2 && index < item.task_info.length - 1"
                  ></icon>
                </div>
              </div>
            </template>
          </Card>

          <icon
            v-tooltip="'Tuần tự'"
            class="pi pi-arrow-right font-bold text-2xl"
            v-if="
              props.data.type == 1 &&
              index < props.data.task_follow_step.length - 1
            "
          ></icon>
          <icon
            class="pi pi-sort-alt font-bold text-2xl"
            style="transform: rotate(90deg)"
            v-tooltip="'Song song'"
            v-if="
              props.data.type == 2 &&
              index < props.data.task_follow_step.length - 1
            "
          ></icon>
        </div>
      </div>
    </div>
  </Dialog>
  <DetailedWork
    v-if="showDetail === true"
    :id="selectedTaskID"
    :turn="0"
    :closeDetail="closeDetail"
  >
  </DetailedWork>
</template>

<style lang="scss" scoped>
::v-deep(.myTextAvatar) {
  .p-avatar-text {
    font-size: 2rem;
  }
}
.w-45rem {
  width: 45rem;
}
.h-45rem {
  height: 57rem;
}
::v-deep(.card-custom) {
  &.p-card {
    .p-card-content {
      text-align: unset !important;
    }
  }
}
</style>
