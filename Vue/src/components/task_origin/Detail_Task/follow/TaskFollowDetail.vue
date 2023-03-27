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

const expandedRows = ref([]);
const props = defineProps({
  data: Object,
  isOpen: Boolean,
  closeDialogDetail: Function,
  rowReorder: Function,
  memberType: Intl,
});
const type_display = ref("1");
onMounted(() => {});
</script>
<template>
  <Dialog
    v-model:visible="props.isOpen"
    :style="'width:90vw;'"
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
            {{ props.data.follow_name }}
          </h2>
        </h3>
        <Accordion :activeIndex="0">
          <AccordionTab header="Thông tin chung">
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

            <DataTable :value="slotProps.data.task_info">
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
          class="flex"
          v-for="(item, index) in props.data.task_follow_step"
          :key="index"
        >
          <Card class="flex m-3 w-45rem h-45rem">
            <template #content>
              <div class="align-items-center justify-content-center flex">
                <span
                  class="flex w-3rem h-3rem align-items-center justify-content-center text-center border-circle z-1 shadow-1 text-2xl font-bold text-blue-500"
                >
                  {{ item.is_step }}
                </span>
              </div>
              <div class="align-items-center justify-content-center flex my-2">
                {{ item.step_name }}
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
                  class="col-10"
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
                <div class="col-6">
                  <span>
                    Ngày bắt đầu(dự kiến):
                    <span v-if="item.start_date">
                      {{
                        moment(new Date(item.start_date)).format("DD/MM/YYYY")
                      }}-
                    </span>

                    <span v-if="item.is_deadline == true">
                      {{ moment(new Date(item.end_date)).format("DD/MM/YYYY") }}
                    </span></span
                  >
                </div>
              </div>
              <div class="col-12">{{ item }}</div>
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
    <!-- 
    {{ props.data.task_follow_step }} -->
  </Dialog>
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
  height: 45rem;
}
</style>
