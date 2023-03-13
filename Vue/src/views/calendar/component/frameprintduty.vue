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
  datas: Array,
  mission: Object,
  week_start_date: Date,
  week_end_date: Date,
});
const newDate = new Date();
</script>
<template>
  <Dialog :visible="true" :closable="false" style="display: block">
    <div id="formprint">
      <table>
        <thead>
          <tr>
            <td
              class="text-center"
              colspan="2"
              style="width: 40%; vertical-align: bottom"
            >
              <div>BỘ QUỐC PHÒNG</div>
              <div>
                <b style="text-decoration: underline">BẢO HIỂM XÃ HỘI</b>
              </div>
            </td>
            <td
              class="text-center"
              colspan="4"
              style="min-width: 40%; vertical-align: bottom"
            >
              <div>
                <b>CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM</b>
              </div>
              <div>
                <b style="text-decoration: underline"
                  >Độc lập - Tự do - Hạnh phúc</b
                >
              </div>
            </td>
          </tr>
          <tr>
            <td class="text-center" colspan="2">
              <div style="padding: 1rem 0">Số:_____/LT-BHXH</div>
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
                <div class="title2"><b>LỊCH TRỰC BAN CƠ QUAN</b></div>
                <div class="">
                  <b>
                    <i>
                      (Từ ngày
                      {{ moment(props.week_start_date).format("DD/MM") }} đến
                      ngày
                      {{ moment(props.week_end_date).format("DD/MM/YYYY") }})
                    </i>
                  </b>
                </div>
              </div>
            </td>
          </tr>
        </thead>
      </table>
      <table style="border: 3px double #999999 !important">
        <thead class="boder">
          <tr>
            <th style="width: 30px">TT</th>
            <th style="width: 180px">Họ và tên</th>
            <th style="min-width: 100px">Cấp bậc, chức vụ</th>
            <!-- <th style="width: 80px">Ca trực</th> -->
            <th style="width: 110px">Thời gian</th>
            <th style="width: 100px">Thứ</th>
            <th style="width: 180px">Trực chỉ huy</th>
          </tr>
        </thead>
        <tbody class="boder">
          <tr v-for="(value, index) in props.datas" :key="index">
            <td align="center">
              <div>{{ index + 1 }}</div>
            </td>
            <td>
              <div v-if="value.trucbans && value.trucbans.length > 0">
                {{ value.trucbans[0].full_name }}
              </div>
            </td>
            <td>
              <div v-if="value.trucbans && value.trucbans.length > 0">
                {{ value.trucbans[0].position_name }}
              </div>
            </td>
            <!-- <td align="center">
              <div v-if="value.calendar_duty_id != null">
                {{
                  value.is_timelot === 0
                    ? "Sáng"
                    : value.is_timelot === 1
                    ? "Chiều"
                    : "Cả ngày"
                }}
              </div>
            </td> -->
            <td align="center">
              <div>{{ value.day_string }}</div>
            </td>
            <td align="left">
              <div>{{ value.day_name }}</div>
            </td>
            <td align="center">
              <div v-if="value.chihuys && value.chihuys.length > 0">
                {{ value.chihuys[0].full_name }}
              </div>
            </td>
          </tr>
        </tbody>
      </table>
      <table>
        <tbody>
          <tr>
            <td colspan="6">
              <div style="padding: 0.5rem 0; text-decoration: underline">
                <b><i>Trực ban có nhiệm vụ:</i></b>
              </div>
              <div class="html" v-html="props.mission.mission"></div>
            </td>
          </tr>
          <tr>
            <td colspan="4" style="vertical-align: top">
              <div style="padding: 0.5rem 0; text-decoration: underline">
                <b><i>Nơi nhận:</i></b>
              </div>
              <div class="html" v-html="props.mission.address"></div>
            </td>
            <td align="center" colspan="2" style="vertical-align: top">
              <div style="padding-top: 1rem; min-height: 150px">
                <div><b>KT.GIÁM ĐỐC</b></div>
                <div><b>PHÓ GIÁM ĐỐC</b></div>
                <div style="height: 170px; position: relative">
                  <div
                    v-if="props.mission.path_signature"
                    style="
                      position: absolute;
                      width: 150px;
                      height: 150px;
                      top: 50%;
                      left: 50%;
                      transform: translate(-80%, -50%);
                    "
                  >
                    <img
                      :src="basedomainURL + props.mission.path_signature"
                      style="width: 100%; height: 100%; object-fit: contain"
                    />
                  </div>
                  <div
                    v-if="props.mission.path_stamp"
                    style="
                      position: absolute;
                      width: 170px;
                      height: 170px;
                      top: 50%;
                      left: 50%;
                      transform: translate(-20%, -50%);
                    "
                  >
                    <img
                      :src="basedomainURL + props.mission.path_stamp"
                      style="width: 100%; height: 100%; object-fit: contain"
                    />
                  </div>
                  <div
                    v-if="props.mission.sign_id"
                    :style="{ marginTop: '100px' }"
                  >
                    <b>{{ props.mission.full_name }}</b>
                  </div>
                </div>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </Dialog>
</template>
<style scoped>
#formprint {
  background: #fff !important;
}
#formprint * {
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
  padding: 0.5rem;
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
