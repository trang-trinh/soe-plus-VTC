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
const task = ref({});
const tasks = ref([]);

//Dictionary
const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);
const typestatus = ref([
  { value: 0, title: "Chưa hiệu lực", bg_color: "#bbbbbb", text_color: "#fff" },
  { value: 1, title: "Đang làm việc", bg_color: "#5fc57b", text_color: "#fff" },
  { value: 2, title: "Đã làm việc", bg_color: "red", text_color: "#fff" },
  { value: 3, title: "Đã làm việc", bg_color: "red", text_color: "#fff" },
]);

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
const initView2 = (rf) => {
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
            proc: "hrm_profile_task_get",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "profile_id", va: props.profile_id },
            ],
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
          task.value = tbs[0][0];
          var idx = typestatus.value.findIndex(
            (x) => x["value"] === task.value["status"]
          );
          if (idx != -1) {
            task.value["status_name"] = typestatus.value[idx]["title"];
            task.value["bg_color"] = typestatus.value[idx]["bg_color"];
            task.value["text_color"] = typestatus.value[idx]["text_color"];
          } else {
            task.value["status_name"] = "Chưa xác định";
            task.value["bg_color"] = "#bbbbbb";
            task.value["text_color"] = "#fff";
          }
          if (task.value["start_date"] != null) {
            task.value["start_date"] = moment(
              new Date(task.value["start_date"])
            ).format("DD/MM/YYYY");
          }
          if (task.value["end_date"] != null) {
            task.value["end_date"] = moment(
              new Date(task.value["end_date"])
            ).format("DD/MM/YYYY");
          }
          if (task.value["sign_date"] != null) {
            task.value["sign_date"] = moment(
              new Date(task.value["sign_date"])
            ).format("DD/MM/YYYY");
          }

          tbs[1].forEach((item) => {
            if (item.managers != null) {
              item.managers = JSON.parse(item.managers);
            }
            var idx = typestatus.value.findIndex(
              (x) => x["value"] === item["status"]
            );
            if (idx != -1) {
              item["status_name"] = typestatus.value[idx]["title"];
              item["bg_color"] = typestatus.value[idx]["bg_color"];
              item["text_color"] = typestatus.value[idx]["text_color"];
            } else {
              item["status_name"] = "Chưa xác định";
              item["bg_color"] = "#bbbbbb";
              item["text_color"] = "#fff";
            }
            if (item["start_date"] != null) {
              item["start_date"] = moment(new Date(item["start_date"])).format(
                "DD/MM/YYYY"
              );
            }
            if (item["end_date"] != null) {
              item["end_date"] = moment(new Date(item["end_date"])).format(
                "DD/MM/YYYY"
              );
            }
            if (item["sign_date"] != null) {
              item["sign_date"] = moment(new Date(item["sign_date"])).format(
                "DD/MM/YYYY"
              );
            }
          });
          tasks.value = tbs[1];
        } else {
          task.value = {};
          tasks.value = [];
        }
      }
      swal.close();
    })
    .catch((error) => {
      swal.close();
      if (error && error.status === 401) {
        swal.fire({
          title: "Thông báo!",
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
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
  if (props.view === 2) {
    initView2(true);
  }
});
</script>
<template>
  <div
    class="d-lang-table my-3"
    :style="{
      height: 'calc(100vh - 182px) !important',
      overflowY: 'auto',
    }"
  >
    <div v-if="tasks && tasks.length > 0" class="w-full">
      <Timeline :value="tasks" align="alternate" class="customized-timeline">
        <template #marker="slotProps">
          <span
            class="flex w-2rem h-2rem align-items-center justify-content-center text-white border-circle z-1 shadow-1"
            :style="{
              backgroundColor: slotProps.item.bg_color,
            }"
          >
            <i class="pi pi-briefcase"></i>
          </span>
        </template>
        <template #content="slotProps">
          <Card>
            <template #title>
              <div class="w-full text-left">
                <Button
                  :label="slotProps.item.status_name"
                  class="p-button-outlined"
                  :style="{
                    borderColor: slotProps.item.bg_color,
                    // backgroundColor: slotProps.data.bg_color,
                    color: slotProps.item.bg_color,
                    borderRadius: '15px',
                    padding: '0.3rem 0.75rem !important',
                  }"
                />
              </div>
            </template>
            <template #subtitle>
              <div class="w-full text-left">
                {{ slotProps.item.sign_date }}
              </div>
            </template>
            <template #content>
              <div class="w-full text-left">
                <div class="mb-2">
                  Chức danh: <b>{{ slotProps.item.work_position_name }}</b>
                </div>
                <div class="mb-2">
                  Chức vụ: <b>{{ slotProps.item.position_name }}</b>
                </div>
                <div class="mb-2">
                  Phòng ban: <b>{{ slotProps.item.department_name }}</b>
                </div>
                <div class="mb-2">
                  Công việc chuyên môn:
                  <b>{{ slotProps.item.professional_work_name }}</b>
                </div>
                <div class="mb-2">
                  Loại hợp đồng: <b>{{ slotProps.item.type_contract_name }}</b>
                </div>
                <div class="mb-2">
                  Loại nhân sự: <b>{{ slotProps.item.personel_groups_name }}</b>
                </div>
                <div
                  class="flex format-center justify-content-left"
                  :style="{ justifyContent: 'left' }"
                >
                  <span class="mr-2">Người quản lý: </span>
                  <AvatarGroup
                    v-if="
                      slotProps.item.managers &&
                      slotProps.item.managers.length > 0
                    "
                  >
                    <Avatar
                      v-for="(item, index) in slotProps.item.managers.slice(
                        0,
                        3
                      )"
                      v-bind:label="
                        item.avatar
                          ? ''
                          : item.profile_user_name.substring(0, 1)
                      "
                      v-bind:image="
                        item.avatar
                          ? basedomainURL + item.avatar
                          : basedomainURL + '/Portals/Image/noimg.jpg'
                      "
                      v-tooltip.top="item.profile_user_name"
                      :key="item.user_id"
                      style="border: 2px solid orange; color: white"
                      @click="onTaskUserFilter(item)"
                      @error="basedomainURL + '/Portals/Image/noimg.jpg'"
                      size="large"
                      shape="circle"
                      class="cursor-pointer"
                      :style="{
                        backgroundColor: bgColor[index % 7],
                        width: '2.5rem',
                        height: '2.5rem',
                      }"
                    />
                    <Avatar
                      v-if="
                        slotProps.item.managers &&
                        slotProps.item.managers.length > 3
                      "
                      v-bind:label="
                        '+' + (slotProps.item.managers - 3).toString()
                      "
                      shape="circle"
                      size="large"
                      :style="{
                        backgroundColor: '#2196f3',
                        color: '#ffffff',
                        width: '2.5rem',
                        height: '2.5rem',
                      }"
                      class="cursor-pointer"
                    />
                  </AvatarGroup>
                </div>
              </div>
            </template>
          </Card>
        </template>
      </Timeline>
    </div>
    <div v-else class="w-full h-full format-center">
      <div class="description">Hiện chưa có dữ liệu</div>
    </div>
  </div>
</template>
<style scoped></style>
