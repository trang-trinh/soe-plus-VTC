<script setup>
import { ref, inject, onMounted } from "vue";
import moment from "moment";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { de } from "date-fns/locale";
const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios");
const emitter = inject("emitter");
const isDynamicSQL = ref(false);
const basedomainURL = baseURL;
const config = {
  headers: {
    Authorization: `Bearer ${store.getters.token}`,
  },
};
const toast = useToast();
const props = defineProps({
  datadays: Array,
  datachutris: Array,
  holiday: Object,
  duty_sunday: Array,
  group: Number,
  week_start_date: Date,
  week_end_date: Date,
});
const newDate = new Date();

//function
function urlify(string) {
  var urlRegex = string.match(
    /((((ftp|https?):\/\/)|(w{3}\.))[\-\w@:%_\+.~#?,&\/\/=]+)/g
  );
  if (urlRegex) {
    urlRegex.forEach(function (url) {
      string = string.replace(
        url,
        '<a target="_blank" href="' + url + '">' + url + "</a>"
      );
    });
  }
  return string.replace("(", "<br/>(");
}
const trustAsHtml = (html) => {
  if (!html) {
    return "";
  }
  return urlify(html).replace(/\n/g, "<br/>");
};
</script>
<template>
  <Dialog
    :visible="true"
    :closable="false"
    :style="{ width: '50vw' }"
    style="display: none"
  >
    <div id="formword_2">
      <table style="width: 100%">
        <thead>
          <tr>
            <td
              class="text-center"
              colspan="2"
              style="width: 80px; vertical-align: bottom"
            >
              <div>BỘ QUỐC PHÒNG</div>
              <div>
                <b style="text-decoration: underline">BẢO HIỂM XÃ HỘI</b>
                <!-- <div
                  class="text-center"
                  style="border-top: 1.5px solid #000; margin: 0px 100px"
                ></div> -->
              </div>
            </td>
            <td
              class="text-center"
              colspan="4"
              style="min-width: 200px; vertical-align: bottom"
            >
              <div>
                <b>CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM</b>
              </div>
              <div>
                <b style="text-decoration: underline"
                  >Độc lập - Tự do - Hạnh phúc</b
                >
              </div>
              <!-- <div
                class="text-center"
                style="border-top: 1.5px solid #000; margin: 0px 100px"
              ></div> -->
            </td>
          </tr>
          <tr>
            <td class="text-center" colspan="2">
              <div style="padding: 1rem 0">Số:_____/CTr-BHXH</div>
            </td>
            <td class="text-center" colspan="4">
              <div style="padding: 1rem 0">
                <i
                  >Hà Nội, ngày {{ newDate.getDate() }}, tháng
                  {{ newDate.getMonth() + 1 }}, năm
                  {{ newDate.getFullYear() }}</i
                >
              </div>
            </td>
          </tr>
          <tr>
            <td class="text-center" colspan="6">
              <div style="padding: 1rem 0">
                <div class="title2">
                  <b
                    >CHƯƠNG TRÌNH LÀM VIỆC CỦA BAN GIÁM ĐỐC BẢO HIỂM XÃ HỘI BỘ
                    QUỐC PHÒNG</b
                  >
                </div>
                <div class="">
                  <b
                    ><i>
                      (Từ ngày
                      {{ moment(props.week_start_date).format("DD/MM/YYYY") }}
                      đến ngày
                      {{ moment(props.week_end_date).format("DD/MM/YYYY") }})
                    </i></b
                  >
                </div>
                <div>
                  <b>
                    Trực Chỉ huy: Đồng chí ______________________________ - Phó
                    Giám đốc
                  </b>
                </div>
              </div>
            </td>
          </tr>
        </thead>
      </table>
      <br />
      <table style="width: 100%">
        <thead class="boder">
          <tr>
            <th style="width: 80px">Thứ / Ngày</th>
            <th
              v-for="(value, index) in props.datachutris"
              :key="index"
              style="width: 250px"
            >
              Đ/c {{ value.full_name }}
            </th>
          </tr>
        </thead>
        <tbody class="boder">
          <tr v-for="(day, dayindex) in props.datadays" :key="dayindex">
            <td align="center">
              <div>{{ day.day_name }} <br />{{ day.day_string_short }}</div>
            </td>
            <td
              v-for="(chutri, chutriindex) in props.datachutris"
              :key="chutriindex"
            >
              <template
                v-for="(content, contentindex) in day.list_contents"
                :key="contentindex"
              >
                <div
                  v-if="content.user_id === chutri.user_id"
                  v-html="content.contents"
                ></div>
              </template>
            </td>
          </tr>
        </tbody>
      </table>
      <br />
      <table style="width: 100%">
        <tbody>
          <tr>
            <td colspan="6">
              <div style="padding: 0.5rem 0">
                * Trực Chủ nhật ({{ props.holiday.day_string }}): Đồng chí
                {{ props.duty_sunday.rank }} {{ props.duty_sunday.full_name }} -
                {{ props.duty_sunday.position_name }}
                {{ props.duty_sunday.department_name }}
              </div>
            </td>
          </tr>
          <tr>
            <td colspan="6" style="vertical-align: top">
              <div style="padding: 0.5rem 0; text-decoration: underline">
                <b><i>Nơi nhận:</i></b>
              </div>
              <div>
                <div>- Ban Giám đốc;</div>
                <div>
                  - Các Phòng, Ban, TLCT, TLCP, TLTC nội bộ, Lái xe/C79;
                </div>
                <div>- Lưu: KH-TH. T15.</div>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </Dialog>
</template>
<style scoped>
#formword_2 {
  background: #fff !important;
}
#formword_2 * {
  font-family: "Times New Roman", Times, serif !important;
  font-size: 13pt;
}
.title1,
.title1 * {
  font-size: 17pt !important;
}
.title2,
.title2 * {
  font-size: 16pt !important;
}
.title3,
.title3 * {
  font-size: 15pt !important;
}
.boder tr th,
.boder tr td {
  border: 1px solid #999999 !important;
  padding: 0.2rem !important;
}
table {
  min-width: 100% !important;
  page-break-inside: auto !important;
  border-collapse: collapse !important;
  table-layout: fixed !important;
}
thead {
  display: table-header-group !important;
}
tbody {
  display: table-header-group !important;
}
tr {
  -webkit-column-break-inside: avoid !important;
  page-break-inside: avoid !important;
}
tfoot {
  display: table-footer-group !important;
}
.uppercase,
.uppercase * {
  text-transform: uppercase !important;
}
.text-center {
  text-align: center !important;
}
.text-left {
  text-align: left !important;
}
.text-right {
  text-align: right !important;
}
.html p {
  margin: 0 !important;
  padding: 0 !important;
}
</style>
<style lang="scss" scoped>
::v-deep(.html) {
  p {
    margin: 0 !important;
    padding: 0 !important;
  }
}
</style>
