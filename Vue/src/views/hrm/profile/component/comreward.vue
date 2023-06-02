<script setup>
import { onMounted, inject, ref } from "vue";
import { useToast } from "vue-toastification";
import { encr } from "../../../../util/function";
import moment from "moment";

const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios");
const cryoptojs = inject("cryptojs");
const toast = useToast();
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const basedomainURL = baseURL;

//Get arguments
const props = defineProps({
  profile_id: String,
  view: Number,
});

//Declare
const options = ref({});
const dictionarys = ref([]);
const rewards = ref([]);
const disciplines = ref([]);

//Dictionary
const typestatus = ref([
  { value: 0, title: "Chưa hiệu lực", bg_color: "#bbbbbb", text_color: "#fff" },
  { value: 1, title: "Đang làm việc", bg_color: "#5fc57b", text_color: "#fff" },
  { value: 2, title: "Đã làm việc", bg_color: "red", text_color: "#fff" },
  { value: 3, title: "Đã làm việc", bg_color: "red", text_color: "#fff" },
]);

//Function
const goFile = (file) => {
  window.open(basedomainURL + file.file_path, "_blank");
};

//init
const initDictionary2 = () => {
  dictionarys.value = [];
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_contract_dictionary",
            par: [{ par: "user_id", va: store.getters.user.user_id }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      if (response != null && response.data != null) {
        var data = response.data.data;
        if (data != null) {
          let tbs = JSON.parse(data);
          dictionarys.value = tbs;
        }
      }
    })
    .then(() => {
      initView2(true);
    });
};
const initView16 = (rf) => {
  if (rf) {
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
  }
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_profile_reward",
            par: [{ par: "profile_id", va: props.profile_id }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      var data = response.data.data;
      if (data != null) {
        var tbs = JSON.parse(data);
        if (tbs[0] != null && tbs[0].length > 0) {
          tbs[0].forEach((item) => {
            if (item.effective_date != null) {
              item.effective_date = moment(
                new Date(item.effective_date)
              ).format("DD/MM/YYYY");
            }
            if (item.files != null) {
              item.files = JSON.parse(item.files);
            }
          });
          rewards.value = tbs[0].filter(
            (x) => x.reward_type === 1 || x.reward_type === 2
          );
          disciplines.value = tbs[0].filter((x) => x.reward_type === 3);
        } else {
          rewards.value = [];
          disciplines.value = [];
        }
      }
      swal.close();
    })
    .catch((error) => {
      swal.close();
      if (error && error.status === 401) {
        swal.fire({
          title: "Thông báo!",
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
        return;
      } else {
        swal.fire({
          title: "Thông báo!",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
        return;
      }
    });
};
onMounted(() => {
  if (props.view === 16) {
    initView16(true);
  }
});
</script>
<template>
  <div
    class="d-lang-table"
    :style="{
      height: 'calc(100vh - 182px) !important',
      overflowY: 'auto',
    }"
  >
    <div
      v-if="
        (rewards && rewards.length > 0) ||
        (disciplines && disciplines.length > 0)
      "
      class="w-full"
    >
      <div class="col-12 md:col-12">
        <div class="row">
          <div class="col-6 md:col-6">
            <h3 class="m-0"><b>Khen thưởng</b></h3>
          </div>
          <div class="col-6 md:col-6">
            <h3 class="m-0"><b>Kỷ luật</b></h3>
          </div>
          <div class="col-6 md:col-6">
            <Timeline
              :value="rewards"
              align="alternate"
              class="customized-timeline"
            >
              <template #marker="slotProps">
                <span
                  class="flex w-2rem h-2rem align-items-center justify-content-center text-white border-circle z-1 shadow-1"
                  :style="{
                    backgroundColor: '#0078d4',
                  }"
                >
                  <i class="pi pi-bookmark"></i>
                </span>
              </template>
              <template #content="slotProps">
                <Card class="mb-5" :style="{ backgroundColor: '#D6EAF8', boxShadow: 'none' }">
                  <template #subtitle>
                    <div class="w-full text-left">
                      {{ slotProps.item.effective_date }}
                    </div>
                  </template>
                  <template #content>
                    <div class="w-full text-left">
                      <div class="mb-2">
                        Số:
                        <b>{{ slotProps.item.reward_number }}</b>
                      </div>
                      <div class="mb-2">
                        Cấp khen thưởng:
                        <b>{{ slotProps.item.reward_level_name }}</b>
                      </div>
                      <div class="mb-2">
                        Danh hiệu: <b>{{ slotProps.item.reward_title_name }}</b>
                      </div>
                      <div>
                        Đính kèm:
                        <div
                          v-for="(file, index) in slotProps.item.files"
                          class="mt-2"
                        >
                          <a @click="goFile(file)" class="hover"
                            ><i class="pi pi-paperclip"></i>
                            {{ file.file_name }}</a
                          >
                        </div>
                      </div>
                    </div>
                  </template>
                </Card>
              </template>
            </Timeline>
          </div>
          <div class="col-6 md:col-6">
            <Timeline
              :value="disciplines"
              align="alternate"
              class="customized-timeline"
            >
              <template #marker="slotProps">
                <span
                  class="flex w-2rem h-2rem align-items-center justify-content-center text-white border-circle z-1 shadow-1"
                  :style="{
                    backgroundColor: '#F39C12',
                  }"
                >
                  <i class="pi pi-bolt"></i>
                </span>
              </template>
              <template #content="slotProps">
                <Card class="mb-5" :style="{ backgroundColor: '#FDEBD0', boxShadow: 'none' }">
                  <template #subtitle>
                    <div class="w-full text-left">
                      {{ slotProps.item.effective_date }}
                    </div>
                  </template>
                  <template #content>
                    <div class="w-full text-left">
                      <div class="mb-2">
                        Số:
                        <b>{{ slotProps.item.reward_number }}</b>
                      </div>
                      <div class="mb-2">
                        Cấp khen thưởng:
                        <b>{{ slotProps.item.reward_level_name }}</b>
                      </div>
                      <div class="mb-2">
                        Danh hiệu: <b>{{ slotProps.item.reward_title_name }}</b>
                      </div>
                      <div>
                        Đính kèm:
                        <div
                          v-for="(file, index) in slotProps.item.files"
                          class="mt-2"
                        >
                          <a @click="goFile(file)" class="hover"
                            ><i class="pi pi-paperclip"></i>
                            {{ file.file_name }}</a
                          >
                        </div>
                      </div>
                    </div>
                  </template>
                </Card>
              </template>
            </Timeline>
          </div>
        </div>
      </div>
    </div>
    <div v-else class="w-full h-full format-center">
      <div class="description">Hiện chưa có dữ liệu</div>
    </div>
  </div>
</template>
<style scoped>
.row {
  width: 100%;
  display: -webkit-box;
  display: -ms-flexbox;
  display: flex;
  -ms-flex-wrap: wrap;
  flex-wrap: wrap;
}

.form-group {
  display: grid;
  margin-bottom: 1rem;
  flex: 1;
}

.form-group > label {
  margin-bottom: 0.5rem;
}

.ip36 {
  width: 100%;
}
.p-timeline-event-opposite {
  display: none;
}
.hover {
  cursor: pointer;
  color: #0078d4;
}
</style>
<style lang="scss" scoped>
::v-deep(.p-timeline) {
  .p-timeline-event .p-timeline-event-opposite {
    display: none !important;
  }
  .p-timeline-event:nth-child(even) {
    flex-direction: row;
  }
}
</style>
