<script setup>
import { ref, onMounted, inject } from "vue";
import { encr } from "../../../../util/function.js";
import { useToast } from "vue-toastification";
import moment from "moment";
const emitter = inject("emitter");
const axios = inject("axios");
const store = inject("store");
const toast = useToast();
const swal = inject("$swal");
const cryoptojs = inject("cryptojs");
const router = inject("router");
const user = store.state.user;
const addLog = (log) => {
  // eslint-disable-next-line no-undef
  axios.post(baseURL + "/api/Proc/AddLog", log, config);
};
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const options = ref({});
const listDropdownGroup = ref([]);
const listDropdownProject = ref([]);
const loadData = () => {
  axios
    .post(
      // eslint-disable-next-line no-undef
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "task_dashboard_count",
            par: [
              { par: "user_id", va: user.user_id },
              { par: "page", va: 0 },
            ],
          }),
          // eslint-disable-next-line no-undef
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let listDropdown = JSON.parse(response.data.data)[4];
      let temp1 = JSON.parse(listDropdown[0].list_project);
      let temp2 = JSON.parse(listDropdown[0].list_group);
      temp1.forEach((element) => {
        listDropdownProject.value.push({
          label: element.project_name,
          value: element.project_id,
        });
      });
      temp2.forEach((x) => {
        listDropdownGroup.value.push({
          label: x.group_name,
          value: x.group_id,
        });
      });
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!" + error);
      addLog({
        title: "Lỗi Console loadData",
        controller: "MyTaskInfo.vue",
        logcontent: error.message,
        loai: 2,
      });
      if (error && error.status === 401) {
        swal.fire({
          title: "Thông báo",
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const OnWeek = () => {
  options.value.start_week_date = moment().startOf("isoWeek").toDate();
  options.value.end_week_date = moment().endOf("isoWeek").toDate();
  options.value.end_week_date = moment().endOf("isoWeek").toDate();
};
const props = defineProps({
  func: Function,
  data: Object,
});
onMounted(() => {
  loadData();
  options.value = props.data;
});
</script>
<template>
  <div class="">
    <div
      class="col-12"
      style="height: 60vh; overflow: auto"
    >
      {{ options }}
      <div class="col-12">
        <div class="col-12 flex p-cus1">
          <div class="col flex align-items-center">
            <div class="col-6">
              <i class="pi pi-calendar pr-2"></i> Ngày nhận
            </div>
            <Calendar
              v-model="options.start_temp_date"
              :showIcon="true"
              :showTime="true"
              class="col-6"
              :manualInput="true"
            >
            </Calendar>
          </div>
          <div class="col-1"></div>
          <div class="col flex align-items-center">
            <div class="flex col-5 align-items-center">Dự án</div>
            <div class="flex col-7 align-items-center">
              <Dropdown
                :filter="true"
                v-model="options.project_id"
                :options="listDropdownProject"
                optionLabel="label"
                placeholder="Chọn dự án"
                panelClass="d-design-dropdown"
                class="col-12"
                optionValue="value"
                :showClear="true"
              >
              </Dropdown>
            </div>
          </div>
        </div>
        <div class="col-12 flex p-cus1">
          <div class="col flex align-items-center">
            <div class="col-6">
              <i class="pi pi-calendar pr-2"></i> Ngày kết thúc
            </div>
            <Calendar
              v-model="options.start_from_date"
              :showIcon="true"
              :showTime="true"
              class="col-6"
              :manualInput="true"
            >
            </Calendar>
          </div>
          <div class="col-1"></div>
          <div class="col flex align-items-center">
            <div class="flex col-5 align-items-center">Nhóm công việc</div>
            <div class="flex col-7 align-items-center">
              <Dropdown
                :filter="true"
                v-model="options.group_id"
                :options="listDropdownGroup"
                optionLabel="label"
                placeholder="Chọn nhóm công việc"
                panelClass="d-design-dropdown"
                optionValue="value"
                :showClear="true"
                class="col-12"
              >
              </Dropdown>
            </div>
          </div>
        </div>
      </div>

      <div class="col-12">
        <Button
          class="p-button-text p-button-secondary"
          label="Trong tuần"
          icon="pi pi-calendar"
          @click="OnWeek()"
        ></Button>
      </div>
      <div class="col-12">
        <Button
          class="p-button-text p-button-secondary"
          label="Trong tháng"
          icon="pi pi-calendar"
        ></Button>
      </div>
      <div class="col-12">
        <Button
          class="p-button-text p-button-secondary"
          label="Trong trong năm"
          icon="pi pi-calendar"
        ></Button>
      </div>

      <div class="col-12 p-cus1 pt-2">
        <label
          class="p-button-text p-button-secondary"
          disabled="true"
        >
          <i class="pi pi-calendar-times pr-2" />
          Theo khoảng thời gian
        </label>
      </div>
      <div class="col-12 flex p-cus1 font-bold pt-4">
        <div class="col-6 py-0 flex align-items-center">
          Ngày bắt đầu
          <div
            class="flex align-items-center"
            v-if="
              options.start_from_date != null && options.start_from_date != ''
            "
          >
            <p class="px-2 font-bold text-blue-500">
              {{ moment(options.start_from_date).format("DD/MM/YYYY") }}
            </p>
            <Button
              icon="pi pi-times"
              class="p-button-rounded p-button-text p-button-danger"
              @click="options.start_from_date = null"
            >
            </Button>
          </div>
        </div>
        <div class="px-2"></div>
        <div class="col-6 py-0 flex align-items-center">
          Ngày kết thúc
          <div
            class="flex align-items-center"
            v-if="options.end_to_date != null && options.end_to_date != ''"
          >
            <p class="px-2 font-bold text-blue-500">
              {{ moment(options.end_to_date).format("DD/MM/YYYY") }}
            </p>
            <Button
              icon="pi pi-times"
              class="p-button-rounded p-button-text p-button-danger"
              @click="options.end_to_date = null"
            >
            </Button>
          </div>
        </div>
      </div>
      <div class="col-12 flex py-0">
        <Calendar
          v-model="options.start_from_date"
          :showTime="true"
          :inline="true"
          class="col-6 py-0 p-cus1"
        ></Calendar>
        <Calendar
          v-model="options.end_to_date"
          :showTime="true"
          :inline="true"
          class="col-6 py-0 p-cus1"
        ></Calendar>
      </div>
    </div>
    <div class="col-12 flex align-items-center justify-content-end py-4">
      <Button
        icon="pi pi-check"
        class="mx-2 p-button-raised"
        label="Lọc"
        @click="props.func"
      ></Button>
      <Button
        icon="pi pi-times"
        class="mx-2 p-button-text p-button-raised"
        label="Hủy"
        @click="refresh()"
      ></Button>
    </div>
  </div>
</template>

<style lang="scss" scoped>
.col-12,
.col-11,
.col-10,
.col-9,
.col-8,
.col-7,
.col-6,
.col-5,
.col-6,
.col-3,
.col-2,
.col-1,
.col {
  padding: 2px 0;
  margin: 0;
}
.p-cus1 {
  padding: 0.05rem 1rem;
}
</style>
