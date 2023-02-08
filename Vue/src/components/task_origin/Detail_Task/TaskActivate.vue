<script setup>
import { ref, inject, onMounted } from "vue";
import { useToast } from "vue-toastification";
import moment from "moment";
import { encr } from "../../../util/function.js";
const cryoptojs = inject("cryptojs");
const axios = inject("axios"); // inject axios
const store = inject("store");
const swal = inject("$swal");
const toast = useToast();

const basedomainURL = fileURL;
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const props = defineProps({
  id: Intl,
  psb: String,
});
const opition = ref({
  sort: "created_date",
  ob: "DESC",
});
const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);
const groupBy = (array, key) => {
  const result = {};
  array.forEach((item) => {
    if (!result[item[key]]) {
      result[item[key]] = [];
    }
    result[item[key]].push(item);
  });
  listTaskActives.value = result;
};
const listTaskActives = ref([]);
const loadTaskLog = () => {
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "task_origin_active_get",
            par: [
              { par: "id", va: props.id },
              { par: "ob", va: opition.value.ob },
            ],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      var weekday = new Array(7);
      weekday[0] = "Chủ nhật";
      weekday[1] = "Thứ 2";
      weekday[2] = "Thứ 3";
      weekday[3] = "Thứ 4";
      weekday[4] = "Thứ 5";
      weekday[5] = "Thứ 6";
      weekday[6] = "Thứ 7";
      data.forEach(function (r) {
        r.created_date = new Date(r.created_date);
        r.DaysName = weekday[r.created_date.getDay()];
        r.Time = r.DaysName + " (" + r.Ngay + ")";
      });
      groupBy(data, "Time");
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const ChangeSort = () => {
  if (opition.value.ob == "DESC") {
    opition.value.ob = "ASC";
  } else {
    opition.value.ob = "DESC";
  }
  loadTaskLog();
};
onMounted(() => {
  loadTaskLog(true);
  return {};
});
</script>
<template>
  <div class="w-full">
    <div class="row col-12 p-0 m-0 font-bold text-xl">
      <!-- <i class="pi pi-check-square pr-2"></i> -->
      <div
        style="border-bottom: 1px solid #ccc; margin-right: 10px; height: 40px"
      >
        <ul style="display: flex; padding: 0px; float: left">
          <li
            @click="addLinkTaskOrigin(datalists)"
            style="list-style: none; margin-right: 20px; color: #0d89ec"
          >
            <a style="display: flex; font-size: 15px"
              ><i
                style="margin-right: 5px"
                class="p-custom pi pi-calendar-times"
              ></i>
              Hoạt động gần nhất</a
            >
          </li>
        </ul>
        <ul
          id="task-active-sort"
          style="display: flex; padding: 0px; float: right"
        >
          <li
            style="list-style: none; margin-right: 20px"
            @click="ChangeSort()"
            :class="{ active: opition.sort }"
            aria-haspopup="true"
            aria-controls="overlay_Export"
          >
            <a style="display: flex; font-size: 15px; align-items: center"
              ><i class="pi pi-sort"></i> Sắp xếp
              <i class="pi pi-angle-down"></i
            ></a>
          </li>
        </ul>
      </div>
      <div
        class="col-12 p-0 m-0"
        style="
          max-height: calc(100vh - 110px);
          min-height: calc(100vh - 110px);
          overflow-y: auto;
        "
      >
        <div v-for="(group, groupName) in listTaskActives">
          <span
            style="
              color: #0d89ec;
              font-size: 12px;
              border-bottom: 1px solid #ccc;
              display: block;
              padding: 10px;
            "
            >{{ groupName }}</span
          >
          <ul class="task-active-list" style="margin: 0px; padding: 0px">
            <li
              style="list-style: none; padding: 5px 10px"
              v-for="value in group"
            >
              <div style="display: flex">
                <div style="display: flex; flex: 1">
                  <Avatar
                    :key="index"
                    v-tooltip.bottom="{
                      value:
                        value.full_name +
                        '<br/>' +
                        (value.tenChucVu || '') +
                        '<br/>' +
                        (value.tenToChuc || ''),
                      escape: true,
                    }"
                    v-bind:label="
                      value.avatar
                        ? ''
                        : (value.last_name ?? '').substring(0, 1)
                    "
                    v-bind:image="basedomainURL + value.avatar"
                    style="
                      background-color: #2196f3;
                      color: #ffffff;
                      width: 48px;
                      height: 48px;
                      font-size: 15px !important;
                    "
                    :style="{
                      background: bgColor[0] + '!important',
                    }"
                    class="cursor-pointer"
                    size="xlarge"
                    shape="circle"
                  />
                  <div
                    style="
                      font-size: 13px;
                      display: flex;
                      flex-direction: column;
                      padding: 8px 20px;
                    "
                  >
                    <span style="flex: 1">{{ value.full_name }}</span>
                    <span
                      style="font-weight: 500"
                      v-html="value.description"
                    ></span>
                  </div>
                </div>
                <div style="font-size: 11px; color: #aaa">
                  <span style="font-weight: 500">{{
                    moment(new Date(value.created_date)).format(
                      "HH:mm DD/MM/YYYY",
                    )
                  }}</span>
                </div>
              </div>
            </li>
          </ul>
        </div>
      </div>
    </div>
  </div>
</template>
<style scoped>
#task-active-sort li:hover {
  cursor: pointer;
  color: #0d89ec;
}
.task-active-list li {
  border-bottom: 1px solid #ccc;
}
.task-active-list li:hover {
  cursor: pointer;
  background-color: #e5f3ff !important;
}
</style>
