<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { encr } from "../../../../util/function.js";
import moment from "moment";
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
const props = defineProps({
  data: Object,
  is_rv: Boolean,
});
onMounted(() => {
  return {};
});
</script>
<template>
  <div class="col-12">
    <div
      class="col-12 format-center font-bold text-2xl border-bottom-1 h-6rem flex"
    >
      <span><i class="pi pi-check-square text-xl mr-2" /></span>
      <span class="format-left font-bold">{{ props.data.report_name }}</span>
    </div>
    <div>
      <div class="col-12">
        <div class="col-12 flex">
          <div class="col-8 format-left">
            <span class="text-black-alpha-90 font-bold">
              <i class="pi pi-user-edit mr-2 font-bold" />Người tạo:
            </span>
            <span class="pl-2 font-bold text-blue-500"
              >{{ props.data.user_info.full_name }} -
              {{
                moment(new Date(props.data.created_date)).format(
                  "HH:mm DD/MM/YYYY",
                )
              }}</span
            >
          </div>
          <div class="col-4">
            <Chip
              v-if="props.is_rv == true"
              :style="{
                background: props.data.is_type_display.bgColor,
                color: props.data.is_type_display.text,
              }"
              v-bind:label="props.data.is_type_display.label"
              class="format-center"
            />
            <Chip
              v-else
              :style="{
                background: props.data.status_display.bgColor,
                color: props.data.status_display.text,
              }"
              v-bind:label="props.data.status_display.label"
              class="format-center"
            />
          </div>
        </div>
        <div class="col-12">
          <div class="col-12 p-0 font-bold">
            <i class="pi pi-info-circle mr-2 font-bold" />Nội dung báo cáo
          </div>
          <div
            class="col-12 mx-4"
            v-html="props.data.messages"
          ></div>
        </div>
        <div class="col-12 flex">
          <div class="col-6 p-0">
            <span class="font-bold"
              ><i class="pi pi-user-plus mr-2 font-bold" /> Điểm tự đánh giá:
            </span>
            <span class="ml-1 text-blue-500 font-bold">{{
              props.data.self_point
            }}</span>
          </div>
          <div
            class="col-6 p-0"
            v-if="props.data.reviewed_point != null"
          >
            <span class="font-bold"
              ><i class="pi pi-users mr-2 font-bold" /> Điểm được đánh giá:
            </span>
            <span class="ml-1 text-blue-500 font-bold">{{
              props.data.reviewed_point
            }}</span>
          </div>
        </div>
        <div class="col-12 flex">
          <div class="col-12 p-0 font-bold">
            <i class="pi pi-check-circle mr-2 font-bold" /> Công việc
          </div>
        </div>
        <div
          class="col-12 mx-5"
          v-for="(data, index) in props.data.task_info"
          :key="data"
        >
          <div class="col-12 p-0 flex">
            <div class="col-1 p-0 flex">
              <div class="col-2 font-bold p-0 format-center">
                {{ index + 1 }}
              </div>
              <div class="col-9 p-0 format-center">
                <Avatar
                  v-tooltip.bottom="{
                    value:
                      'Người tạo công việc: <br/>' +
                      data.full_name +
                      '<br/>' +
                      (data.tenChucVu || '') +
                      '<br/>' +
                      (data.tenToChuc || ''),
                    escape: true,
                  }"
                  v-bind:label="
                    data.avatar ? '' : (data.last_name ?? '').substring(0, 1)
                  "
                  v-bind:image="basedomainURL + data.avatar"
                  :style="{
                    background: bgColor[0] + '!important',
                    'border-color': '#ffffff',
                  }"
                  class="cursor-pointer"
                  size="small"
                  shape="circle"
                />
              </div>
            </div>

            <div class="col-4 p-0">
              <div style="display: flex; flex-direction: column; padding: 5px">
                <div style="min-height: 25px">
                  <span style="font-weight: bold; font-size: 14px">{{
                    data.task_name
                  }}</span>
                </div>
                <div style="font-size: 12px">
                  <span
                    v-if="data.start_date || data.end_date"
                    style="color: #98a9bc"
                    >{{
                      data.start_date
                        ? moment(new Date(data.start_date)).format("DD/MM/YYYY")
                        : null
                    }}
                    -
                    {{
                      data.end_date
                        ? moment(new Date(data.end_date)).format("DD/MM/YYYY")
                        : null
                    }}</span
                  >
                </div>
                <div
                  v-if="data.project_name"
                  style="min-height: 25px; display: flex; align-items: center"
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
                    >{{ data.project_name }}</span
                  >
                </div>
              </div>
            </div>
            <div class="col-7 flex p-0">
              <div class="col-12 flex">
                <div class="col-4 flex">
                  <div class="col-7 p-0 format-center">
                    <AvatarGroup>
                      <div
                        v-for="(value, index) in data.ThanhvienShows"
                        :key="index"
                      >
                        <div>
                          <Avatar
                            v-tooltip.bottom="{
                              value:
                                value.type_name +
                                ': ' +
                                value.fullName +
                                '<br/>' +
                                (value.tenChucVu || '') +
                                '<br/>' +
                                (value.tenToChuc || ''),
                              escape: true,
                            }"
                            v-bind:label="
                              value.avatar
                                ? ''
                                : (value.ten ?? '').substring(0, 1)
                            "
                            v-bind:image="basedomainURL + value.avatar"
                            style="
                              background-color: #2196f3;
                              color: #ffffff;
                              width: 32px;
                              height: 32px;
                              font-size: 15px !important;
                              margin-left: -10px;
                            "
                            :style="{
                              background: bgColor[index % 7] + '!important',
                            }"
                            class="cursor-pointer"
                            size="xlarge"
                            shape="circle"
                          />
                        </div>
                      </div>
                      <Avatar
                        v-if="
                          data.Thanhviens.length - data.ThanhvienShows.length >
                          0
                        "
                        :label="
                          '+' +
                          (data.Thanhviens.length -
                            data.ThanhvienShows.length) +
                          ''
                        "
                        v-tooltip.bottom="{
                          value:
                            'và ' +
                            (data.Thanhviens.length -
                              data.ThanhvienShows.length) +
                            ' người khác tham gia',
                        }"
                        class="cursor-pointer"
                        shape="circle"
                        style="
                          background-color: #e9e9e9 !important;
                          color: #98a9bc;
                          font-size: 14px !important;
                          width: 32px;
                          margin-left: -10px;
                          height: 32px;
                        "
                      />
                    </AvatarGroup>
                  </div>
                  <div class="col-5 m-1 format-center">
                    <span v-if="data.progress == 0">{{ data.progress }} %</span>
                    <div
                      v-if="data.progress != 0"
                      style="width: 100%"
                    >
                      <ProgressBar :value="data.progress" />
                    </div>
                  </div>
                </div>
                <div class="col-3 format-center">
                  <div v-if="data.title_time">
                    <span
                      style="
                        font-size: 10px;
                        font-weight: bold;
                        padding: 5px;
                        border-radius: 5px;
                      "
                      :style="{
                        background: data.time_bg,
                        color: data.status_text_color,
                      }"
                      >{{ data.title_time }}</span
                    >
                  </div>
                </div>
                <div class="col-2 format-center">
                  <div
                    v-if="data.end_date != null"
                    style="
                      background-color: #ff5a;
                      padding: 10px 10px;
                      border-radius: 5px;
                    "
                  >
                    <span
                      style="color: #0025f8; font-size: 13px; font-weight: bold"
                      >{{
                        moment(new Date(data.end_date)).format("DD/MM/YYYY")
                      }}</span
                    >
                  </div>
                </div>
                <div class="col-3 format-center">
                  <Chip
                    :style="{
                      background: data.status_bg_color,
                      color: data.status_text_color,
                    }"
                    class="format-center"
                    v-bind:label="data.status_name"
                  />
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
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
</style>
