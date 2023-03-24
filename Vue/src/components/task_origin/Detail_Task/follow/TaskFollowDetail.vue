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
onMounted(() => {});
</script>
<template>
  <Dialog
    v-model:visible="props.isOpen"
    :style="'width:88vw;'"
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
                  Ngày bắt đầu(dự kiến):
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
                  Ngày bắt đầu(dự kiến):
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
                  Ngày bắt đầu(dự kiến):
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
                <div class="col-1">Mô tả quy trình</div>
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
        ></Column>
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
                  }}</span></span
                >
              </template>
            </Toolbar>
            <Accordion
              :activeIndex="0"
              class="w-full"
            >
              <AccordionTab header="Mô tả bước">
                <span
                  class="max-h-15rem"
                  style="overflow-y: auto"
                >
                  {{ slotProps.data.description }}
                  <span
                    v-html="slotProps.data.description"
                    v-if="
                      slotProps.data.description != null &&
                      slotProps.data.description != ''
                    "
                  ></span>
                  <b v-else>Không có mô tả cho bước này!</b>
                </span>
              </AccordionTab>
            </Accordion>

            <DataTable
              :value="slotProps.data.task_info"
              @rowReorder="props.rowReorder"
            >
              <Column
                v-if="props.memberType == 0"
                rowReorder
                class="max-w-1rem justify-content-center align-items-center text-center"
              />
              <Column
                header="STT"
                field="step"
                class="justify-content-center align-items-center text-center max-w-5rem"
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
                class="justify-content-center align-items-center text-center max-w-8rem"
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
                class="justify-content-center align-items-center text-center max-w-8rem"
              >
                <template #body="data">
                  <!-- <span
                    :style="{
                      background: data.data.status_display.bg_color,
                      color: data.data.status_display.text_color,
                      padding: '5px 10px',
                      border: '1px solid' + data.data.status_display.bg_color,
                      borderRadius: '5px',
                    }"
                  >
                    {{ data.data.status_display.text }}
                  </span> -->
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
    </div>

    {{ props.data.task_follow_step }}
  </Dialog>
</template>

<style lang="scss" scoped>
::v-deep(.myTextAvatar) {
  .p-avatar-text {
    font-size: 2rem;
  }
}
</style>
