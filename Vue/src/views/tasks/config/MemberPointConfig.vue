//Thiết lập đánh giá thành viên của công việc.
<script setup>
import { ref, inject, onMounted } from "vue";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
import { required } from "@vuelidate/validators";
import { encr } from "../../../util/function.js";
import moment from "moment";
const cryoptojs = inject("cryptojs");
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const toast = useToast();
const router = inject("router");
const emitter = inject("emitter");
const config = { headers: { Authorization: `Bearer ${store.getters.token}` } };
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
const addLog = (log) => {
  axios.post(baseURL + "/api/Proc/AddLog", log, config);
};
const configMember = ref({
  executors: 0,
  co_executors: 0,
  supervisor: 0,
  organization_id: "",
});
const user = store.getters.user;
const isHaveConfig = ref(false);
const loadData = () => {
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "task_review_member_config_get",
            par: [{ par: "user_id", va: user.user_id }],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      configMember.value =
        data.length > 0
          ? data[0]
          : {
              executors: 0,
              co_executors: 0,
              supervisor: 0,
              organization_id: user.organization_id,
            };
      isHaveConfig.value = data.length > 0 ? true : false;
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công2!");
      addLog({
        title: "Lỗi Console loadData",
        controller: "MemberPointConfig.vue",
        logcontent: error.message,
        loai: 2,
      });
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const saveData = () => {
  let formData = new FormData();
  formData.append("config", JSON.stringify(configMember.value));
  axios({
    method: isHaveConfig.value == true ? "put" : "post",
    url:
      baseURL +
      `/api/task_review_member_config/${
        isHaveConfig.value == true ? "Update_config" : "Add_config"
      }`,
    data: formData,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        toast.success("Sửa thiết lập thành công!");
      } else {
        let ms = response.data.ms;
        swal.fire({
          title: "Thông báo!",
          html:
            ms.includes("group_name") == true
              ? "Tên nhóm công việc không quá 250 ký tự!"
              : ms,
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    })
    .catch((error) => {
      swal.close();
      swal.fire({
        title: "Thông báo",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!" + error,
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};
onMounted(() => {
  loadData();
  return {};
});
</script>
<template>
  <div class="main-layout true flex-grow-1 p-2">
    <div class="col-12 bg-white">
      <div
        class="col-6 col-offset-3 flex align-items-center text-3xl text-primary format-center"
      >
        <div class="font-bold">Thiết lập đánh giá thành viên công việc</div>
      </div>

      <div class="col-6 col-offset-3 flex align-items-center">
        <div class="col-4">Người thực hiện</div>
        <InputNumber
          class="col-8"
          suffix=" %"
          mode="decimal"
          :minFractionDigits="2"
          :useGrouping="false"
          v-model="configMember.executors"
          v-tooltip="'Mức hoàn thành công việc'"
        >
        </InputNumber>
      </div>

      <div class="col-6 col-offset-3 flex align-items-center">
        <div class="col-4">Người đồng thực hiện</div>
        <InputNumber
          class="col-8"
          suffix=" %"
          mode="decimal"
          :minFractionDigits="2"
          :useGrouping="false"
          v-model="configMember.co_executors"
          v-tooltip="'Mức hoàn thành công việc'"
        >
        </InputNumber>
      </div>
      <div class="col-6 col-offset-3 flex align-items-center">
        <div class="col-4">Người theo dõi</div>
        <InputNumber
          class="col-8"
          suffix=" %"
          mode="decimal"
          :minFractionDigits="2"
          :useGrouping="false"
          v-model="configMember.supervisor"
          v-tooltip="'Mức hoàn thành công việc'"
        >
        </InputNumber>
      </div>
      <div
        class="col-6 col-offset-3 flex align-items-center justify-content-center"
      >
        <Button
          class="mx-1"
          icon="pi pi-check"
          label="Cập nhật"
          @click="saveData()"
        ></Button>
        <Button
          class="mx-1 p-button-danger"
          icon="pi pi-times"
          label="Hủy"
          @click="loadData"
        ></Button>
      </div>
    </div>
  </div>
</template>

<style lang="scss" scoped></style>
