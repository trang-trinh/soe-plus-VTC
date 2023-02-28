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
  group: Number,
  week_start_date: Date,
  week_end_date: Date,
});
</script>
<template>
  <Dialog :visible="true" :closable="false" style="display: none">
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
                <b>BẢO HIỂM XÃ HỘI</b>
                <div
                  class="text-center"
                  style="border-top: 1.5px solid #000; margin: 0px 100px"
                ></div>
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
              <div><b>Độc lập - Tự do - Hạnh phúc</b></div>
              <div
                class="text-center"
                style="border-top: 1.5px solid #000; margin: 0px 100px"
              ></div>
            </td>
          </tr>
          <tr>
            <td class="text-center" colspan="2">
              <div style="padding: 1rem 0">Số:_____</div>
            </td>
            <td class="text-center" colspan="4">
              <div style="padding: 1rem 0">
                <i>Hà Nội, ngày_____, tháng_____, năm_____</i>
              </div>
            </td>
          </tr>
          <tr>
            <td class="text-center" colspan="6">
              <div style="padding: 1rem 0">
                <div class="title2">
                  <b>{{
                    props.group === 0 ? "LỊCH HỌP TUẦN" : "LỊCH CÔNG TÁC"
                  }}</b>
                </div>
                <div class="">
                  <i>
                    (Từ ngày
                    {{ moment(props.week_start_date).format("DD/MM") }} đến ngày
                    {{ moment(props.week_end_date).format("DD/MM") }})
                  </i>
                </div>
              </div>
            </td>
          </tr>
        </thead>
      </table>
      <table>
        <thead class="boder">
          <tr>
            <th style="width: 30px">TT</th>
            <th style="width: 100px">Thứ</th>
            <th style="min-width: 100px">Nội dung</th>
            <th style="width: 100px">Chủ trì</th>
            <th style="width: 100px">Tham gia</th>
            <th style="width: 100px">Địa điểm</th>
          </tr>
        </thead>
        <tbody class="boder">
          <tr v-for="(value, index) in props.datas" :key="index">
            <td align="center">
              <div>{{ index + 1 }}</div>
            </td>
            <td align="center">
              <div>{{ value.day_name }} <br />{{ value.day_string }}</div>
            </td>
            <td>
                <div v-html="value.contents"></div>
                <div v-if="props.group === 0">
                <div v-if="value.day_space < 1">
                  (<span>{{
                    moment(value.start_date).format("HH:mm")
                  }}</span>
                  <span
                    v-if="
                      value.start_date != null &&
                      value.end_date != null
                    "
                  >
                    -
                  </span>
                  <span>{{
                    moment(value.end_date).format("HH:mm")
                  }}</span
                  >)
                </div>
                <div v-if="value.day_space > 0">
                  <span>{{
                    moment(value.start_date).format("DD/MM/YYYY")
                  }}</span>
                  <span
                    v-if="
                      value.start_date != null &&
                      value.end_date != null
                    "
                  >
                    - </span
                  ><span>{{
                    moment(value.end_date).format("DD/MM/YYYY")
                  }}</span>
                </div>
              </div>
            </td>
            <td>
              <div v-if="value.chutris && value.chutris.length > 0">
                {{ value.chutris[0].full_name }}
              </div>
            </td>
            <td>
              <div>
                <span v-for="(item, index) in value.thamgias" :key="index">
                  <span v-if="index > 0 && index < value.thamgias.length"
                    >,</span
                  >
                  {{ item.full_name }}
                </span>
              </div>
              <div class="mt-2" v-if="value.invitee">
                Người được mời: <span v-html="value.invitee"></span>
              </div>
              <div class="mt-2" v-if="value.departments">
                <div>
                  Phòng ban được mời:
                  <span v-for="(item, index) in value.departments" :key="index">
                    <span v-if="index > 0 && index < value.departments.length"
                      >,</span
                    >
                    {{ item.department_name }}
                  </span>
                </div>
              </div>
            </td>
            <td>
                {{value.boardroom_name}}
            </td>
          </tr>
        </tbody>
      </table>
      <table>
        <tbody>
          <tr>
            <td colspan="4" style="vertical-align: top; width: 40%"></td>
            <td align="center" colspan="2" style="vertical-align: top">
              <div style="padding-top: 3rem; min-height: 150px">
                <div><b>KT.GIÁM ĐỐC</b></div>
                <div><b>PHÓ GIÁM ĐỐC</b></div>
                <div style="height: 170px; position: relative"></div>
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
