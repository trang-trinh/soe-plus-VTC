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
let status = ref([
  {
    code: null,
    label: "Chờ người trước duyệt",
    bgColor: "#2196F3",
    text: "#FFFFFF",
    icon: "pi pi-loading",
  },
  {
    code: -1,
    label: "Đang duyệt",
    bgColor: "#FF6E31",
    text: "#FFFFFF",
    icon: "pi pi-question",
  },
  {
    code: 0,
    label: "Đã duyệt",
    bgColor: "#6DD230",
    text: "#FFFFFF",
    icon: "pi pi-check",
  },
  {
    code: 1,
    label: "Trả lại",
    bgColor: "#FF0000",
    text: "#FFFFFF",
    icon: "pi pi-times",
  },
  {
    code: 2,
    label: "Đã được duyệt hoặc bị trả lại",
    bgColor: "",
    text: "#000000",
    icon: "pi pi-ban",
  },
]);
const datalists = ref();
const props = defineProps({
  id: Intl,
});

const loadData = () => {
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "task_person_report_processing_quyTrinh",
            par: [{ pa: "report_id", va: props.id }],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      let datal = [];
      data.forEach((x, i) => {
        x.user_info = JSON.parse(x.user_info);
        let z = status.value.filter((a) => a.code == x.is_type);
        x.status_display = z[0];
        let o = {
          review_turn: i + 1,
          name: x.group_name ?? x.user_info.full_name,
          data: [],
        };
        datal.push(o);
      });
      datal.forEach((dl) => {
        let x = data.filter((gg) => gg.review_turn == dl.review_turn);
        if (x.length > 0) {
          dl.data = x;
        }
      });
      datalists.value = datal.filter((zz) => zz.data.length > 0);
    })
    .catch((error) => {
      addLog({
        title: "Lỗi Console loadData",
        controller: "SignerView.vue",
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
const is_collapsed = ref(true);
onMounted(() => {
  loadData();
});
</script>
<template>
  <div class="w-full h-full">
    <div
      class="p-2"
      v-for="x in datalists"
      :key="x"
    >
      <Fieldset
        :legend="
          'Lượt: ' + x.review_turn + ' - ' + x.name ?? x.user_info.full_name
        "
        :toggleable="true"
        :collapsed="is_collapsed"
      >
        <div class="col-12">
          <Timeline
            :value="x.data"
            align="alternate"
          >
            <template #marker="slotProps">
              <Avatar
                v-tooltip.bottom="{
                  value:
                    slotProps.item.user_info.full_name +
                    '<br/>' +
                    (slotProps.item.user_info.position_name || '') +
                    '<br/>' +
                    (slotProps.item.user_info.department_name ||
                      slotProps.item.user_info.organization_name),
                  escape: true,
                }"
                v-bind:label="
                  slotProps.item.user_info.avatar
                    ? ''
                    : slotProps.item.user_info.full_name
                        .split(' ')
                        .at(-1)
                        .substring(0, 1)
                "
                v-bind:image="basedomainURL + slotProps.item.user_info.avatar"
                style="color: #ffffff; cursor: pointer"
                :style="{
                  background: bgColor[slotProps.index % 7],
                  border: '2px solid ' + bgColor[slotProps.index % 7],
                }"
                class="flex py-0 m-0"
                size="large"
                shape="circle"
              />
            </template>
            <template #content="slotProps">
              <Card class="mt-3">
                <template #title>
                  <div class="col-12 format-left text-xl">
                    {{ slotProps.item.user_info.full_name }}
                  </div>
                </template>
                <template #subtitle>
                  <div class="col-12 ml-3">
                    {{ slotProps.item.user_info.position_name ?? "" }}
                  </div>
                  <div class="col-12 ml-3">
                    {{
                      slotProps.item.user_info.organization_name ??
                      slotProps.item.user_info.department_name
                    }}
                  </div>
                </template>
                <template #content>
                  <div
                    class="col-12 p-2"
                    v-if="slotProps.item.modified_date != null"
                  >
                    <Chip
                      :label="
                        moment(new Date(slotProps.item.modified_date)).format(
                          'HH:mm DD/MM/YYYY',
                        )
                      "
                      :style="{
                        background: slotProps.item.status_display.bgColor,
                        color: slotProps.item.status_display.text,
                      }"
                      class="w-full format-center"
                    />
                  </div>
                  <div
                    class="col-12 flex"
                    v-if="slotProps.item.status_display"
                  >
                    <div class="col-4">Trạng thái:</div>
                    <div class="col-8 py-0">
                      <Chip
                        :label="slotProps.item.status_display.label"
                        :icon="slotProps.item.status_display.icon"
                        :style="{
                          background: slotProps.item.status_display.bgColor,
                          color: slotProps.item.status_display.text,
                        }"
                      />
                    </div>
                  </div>
                  <div class="col-12 flex">
                    <div class="col-4">Nội dung:</div>
                    <span
                      class="col-8 py-0 no-text-align"
                      v-if="
                        slotProps.item.user_messages != null &&
                        slotProps.item.user_messages.length < 40
                      "
                    >
                      {{ slotProps.item.user_messages.slice(0, 40) }}
                    </span>
                    <Inplace
                      v-else-if="
                        slotProps.item.user_messages != null &&
                        slotProps.item.user_messages.length > 40
                      "
                      class="col-8 py-0"
                      :closable="true"
                    >
                      <template #display>
                        <span>
                          {{ slotProps.item.user_messages.slice(0, 40) }}.....
                        </span>
                      </template>
                      <template #content>
                        <div
                          style="white-space: none"
                          v-html="slotProps.item.user_messages"
                        ></div>
                      </template>
                    </Inplace>
                  </div>
                  <div
                    class="col-12 flex"
                    v-if="slotProps.item.user_messages != 0"
                  >
                    <div class="col-4">Điểm :</div>
                    <div class="col-8">
                      {{ slotProps.item.user_point }}
                    </div>
                  </div>
                </template>
              </Card>
            </template>
          </Timeline>
        </div>
      </Fieldset>
    </div>
  </div>
</template>
<style scoped></style>
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
.no-text-align {
  display: flex;
  justify-content: flex-start;
  align-items: center;
  vertical-align: middle;
}
</style>
