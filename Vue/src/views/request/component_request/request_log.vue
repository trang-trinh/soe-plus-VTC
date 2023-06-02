<script setup>
import { ref, inject, onMounted } from "vue";
import { useToast } from "vue-toastification";
import { encr } from "../../../util/function";
import { VuemojiPicker } from "vuemoji-picker";
import moment from "moment";
import { da } from "date-fns/locale";
const toast = useToast();
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const cryoptojs = inject("cryptojs");
const emitter = inject("emitter");
const basedomainURL = baseURL;
const baseUrlCheck = baseURL;
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);
const props = defineProps({
  id: String,
});
const listLogs = ref([]);
const groupBy = (array, key) => {
  const result = {};
  array.forEach((item) => {
    if (!result[item[key]]) {
      result[item[key]] = [];
    }
    result[item[key]].push(item);
  });
  return result;
};
const initData = () => {
  axios
    .post(
      baseURL + "/api/request/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "request_log_list",
            par: [
              {
                par: "id",
                va: props.id,
              },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      if (response.data != null && response.data.data != null) {
        var data = JSON.parse(response.data.data)[0];
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
          r.Ngay = r.created_date.getDate() + "/" + r.created_date.getMonth();
          r.Time = r.DaysName + " (" + r.Ngay + ")";
        });
        listLogs.value = groupBy(data, "Time");
      }
    })
    .catch((error) => {
      if (error && error.status === 401) {
        swal.fire({
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
onMounted(() => {
  initData();
});
</script>
<template>
  <div>
    <div class="col-12 p-0 h-cus overflow-scroll" v-if="listLogs != null">
      <div
        class="col-12"
        v-for="(group_data, group, index) in listLogs"
        :key="index"
      >
        <span
          style="
            color: #0d89ec;
            font-size: 15px;
            border-bottom: 1px solid #ccc;
            display: block;
            padding: 10px;
          "
        >
          {{ group }}
        </span>
        <div
          class="flex"
          v-for="(value, index2) in group_data"
          :key="index2"
          style="border-bottom: 1px solid #ccc"
        >
          <div class="col-2 flex align-items-center justify-content-center">
            <Avatar
              v-tooltip.bottom="{
                value:
                  value.full_name +
                  '<br/>' +
                  (value.position_name != null
                    ? value.position_name + '<br/>'
                    : '') +
                  (value.department_id != null
                    ? value.department_id + '<br/>'
                    : '') +
                  (value.organization_name || ''),
                escape: true,
              }"
              v-bind:label="
                value.avatar ? '' : (value.full_name ?? '').substring(0, 1)
              "
              v-bind:image="basedomainURL + value.avatar"
              style="background-color: #2196f3; color: #ffffff"
              :style="{
                background: bgColor[index2] + '!important',
              }"
              class="cursor-pointer myavt"
              shape="circle"
              size="large"
            />
          </div>

          <div
            class="col-10"
            style="
              font-size: 13px;
              display: flex;
              flex-direction: column;
              padding: 8px 20px;
            "
          >
            <div class="col-12 font-bold p-0" style="font-size: 15px">
              {{ value.full_name }}
            </div>
            <div class="col-12 px-0" v-html="value.title"></div>
            <span
              style="font-weight: 500"
              class="flex align-items-center text-center"
              >{{
                moment(new Date(value.created_date)).format("HH:mm DD/MM/YYYY")
              }}</span
            >
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
<style lang="scss" scoped>
.h-cus {
  max-height: 84vh !important;
}
::v-deep(.p-avatar) {
  &.myavt {
    .p-avatar-text {
      font-size: 175%;
    }
  }
}
</style>
