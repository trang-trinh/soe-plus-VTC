<script setup>
import ReportHeader from "./components/ReportHeader.vue";
import { ref, inject, onMounted, watch, onBeforeUnmount } from "vue";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
import { required } from "@vuelidate/validators";
import moment from "moment";
import { concat } from "lodash";
import { encr } from "../../util/function.js";
import router from "@/router";
const cryoptojs = inject("cryptojs");
const emitter = inject("emitter");
const basedomainURL = fileURL;
const toast = useToast();
const swal = inject("$swal");
const store = inject("store");
const axios = inject("axios"); // inject axios
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const addLog = (log) => {
  axios.post(baseURL + "/api/Proc/AddLog", log, config);
};
const datalists = ref([]);
const loadData = () => {
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "task_report_project",
            par: [{ par: "user_id", va: store.getters.user.user_id }],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      data.forEach((x) => {
        x.manage_members =
          x.manage_members != null ? JSON.parse(x.manage_members) : [];
        x.work_members = x.work_members ? JSON.parse(x.work_members) : [];
      });
      datalists.value = data;
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      addLog({
        title: "Lỗi Console loadData",
        controller: "ProjectReport.vue",
        log_content: error.message,
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
const formatData = (e, i) => {
  if (i == 2 || i == 3)
    return e ? moment(new Date(e)).format("HH:mm DD/MM/YYYY") : "";
  if (i == 4 || i == 5)
    if (e != null) {
      let string = "";
      e.forEach((z) => {
        string += z.full_name + " (" + z.user_id + ")" + "</br>";
      });
      return string;
    } else return "";
};
const col = ref([
  {
    header: "Tên dự án",
    field: "project_name",
    class: "min-w-30rem",
    headerClass: "align-items-center justify-content-center text-center",
    bodyClass: "",
    isTemplate: false,
    template: "",
  },
  {
    header: "Mô tả",
    field: "description",
    class: "min-w-30rem",
    headerClass: "align-items-center justify-content-center text-center",
    bodyClass: "",
    isTemplate: false,
    template: "",
  },
  {
    header: "Ngày bắt đầu",
    field: "start_date",
    class: "align-items-center justify-content-center text-center min-w-10rem",
    headerClass: "",
    bodyClass: "",
    isTemplate: true,
    template: (e, header) => {
      return formatData(e, header);
    },
  },
  {
    header: "Ngày kết thúc",
    field: "end_date",
    class: "align-items-center justify-content-center text-center min-w-10rem",
    headerClass: "",
    bodyClass: "",
    isTemplate: true,
    template: (e, header) => {
      return formatData(e, header);
    },
  },
  {
    header: "Thành viên quản lý",
    field: "manage_members",
    class: "min-w-16rem",
    headerClass: "align-items-center justify-content-center text-center",
    bodyClass: "",
    isTemplate: true,
    template: (e, header) => {
      return formatData(e, header);
    },
  },
  {
    header: "Thành viên tham gia",
    field: "work_members",
    class: " min-w-16rem",
    headerClass: "align-items-center justify-content-center text-center",
    bodyClass: "",
    isTemplate: true,
    template: (e, header) => {
      return formatData(e, header);
    },
  },
  {
    header: "Nhóm dự án",
    field: "project_group_name",
    class: " min-w-15rem",
    headerClass: "align-items-center justify-content-center text-center",
    bodyClass: "",
    isTemplate: false,
    template: (e, header) => {
      return formatData(e, header);
    },
  },
  {
    header: "Tổng số công việc",
    field: "countTask",
    class: "align-items-center justify-content-center text-center min-w-11rem",
    headerClass: "",
    bodyClass: "",
    isTemplate: false,
    template: (e, header) => {
      return formatData(e, header);
    },
  },
  {
    header: "Trạng thái",
    field: "status",
    class: "align-items-center justify-content-center text-center min-w-8rem",
    headerClass: "",
    bodyClass: "",
    isTemplate: false,
    template: (e, header) => {
      return formatData(e, header);
    },
  },
]);
onMounted(() => {
  loadData();
});
</script>
<template>
  <div class="main-layout true flex-grow-1 p-2">
    <DataTable
      :value="datalists"
      scrollable
      scrollHeight="flex"
      showGridlines
    >
      <template #header>
        <ReportHeader
          class="h-4rem"
          :headersName="'BÁO CÁO THỐNG KÊ TỔNG HỢP DỰ ÁN'"
        ></ReportHeader>
      </template>
      <Column
        v-for="(item, index) in col"
        :key="index"
        :header="item.header"
        :field="item.field"
        :headerClass="item.headerClass"
        :bodyClass="item.bodyClass"
        :class="[item.class]"
      >
        <template
          #body="data"
          v-if="item.isTemplate == true"
        >
          <div v-html="item.template(data.data[item.field], index)"></div>
        </template>
      </Column>
    </DataTable>
  </div>
</template>
<style lang="scss" scoped></style>
