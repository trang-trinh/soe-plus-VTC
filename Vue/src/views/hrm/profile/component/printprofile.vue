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
});
</script>
<template>
  <div id="formword">
    <table style="width: 100%">
      <thead>
        <tr>
          <td
            class="text-left"
            colspan="4"
            style="width: 40%; vertical-align: bottom"
          >
            <div>Bộ, Tỉnh: .............................................</div>
            <div>Đơn vị trực thuộc: ..............................</div>
            <div>Đơn vị cơ sở: ......................................</div>
          </td>
          <td
            class="text-right"
            colspan="2"
            style="min-width: 40%; vertical-align: top"
          >
            <div>Mẫu 2C/TCTW-98</div>
          </td>
        </tr>
        <tr>
          <td class="text-center" colspan="6">
            <div style="padding-top: 1rem">
              <div class="title2">
                <b>SƠ YẾU LÝ LỊCH</b>
              </div>
            </div>
          </td>
        </tr>
        <tr>
          <td class="text-right" colspan="6">
            <div style="padding-bottom: 1rem">
              <div>............................................</div>
              <div>Số hiệu cán bộ, công chức</div>
            </div>
          </td>
        </tr>
      </thead>
    </table>
    <br />
    <table style="width: 100%">
      <tbody class="tbody">
        <tr>
          <td rowspan="4">Ảnh</td>
          <td colspan="3">1) Họ và tên khai sinh: </td>
          <td colspan="2">Nam, Nữ: </td>
        </tr>
        <tr>
          <td colspan="6">2) Các tên gọi khác: </td>
        </tr>
        <tr>
          <td colspan="3">3) Cấp uỷ hiện tại: </td>
          <td colspan="3">Phụ cấp chức vụ: </td>
        </tr>
        <tr>
          <td colspan="6">4) Sinh ngày ,tháng ,năm 5) Nơi sinh: </td>
        </tr>
        <tr>
          <td colspan="6">6) Quê quán (Xã, phường): ,(huyện, quận): ,(tỉnh, TP):</td>
        </tr>
        <tr>
          <td colspan="6">7) Nơi ở hiện nay (Xã, huyện, tỉnh hoặc số nhà, đường phố, TP): ,Điện thoại: </td>
        </tr>
        <tr>
          <td colspan="4">8) Dân tộc (Kinh, Tày, Êđê…): </td>
          <td colspan="2">9) Tôn giáo: </td>
        </tr>
        <tr>
          <td colspan="6">10) Thành phần gia đình xuất thân (Ghi là công nhân, nông dân, cán bộ, công chức, trí thức, quân nhân, dân nghèo thành thị, tiểu thương, tiểu chủ, tư sản ...): </td>
        </tr>
        <tr>
          <td colspan="6">11) Nghề nghiệp bản thân trước khi được tuyển dụng (Ghi nghề được đào tạo hoặc công nhân (thợ gì), làm ruộng, buôn bán, học sinh ...): </td>
        </tr>
        <tr>
          <td colspan="3">12) Ngày được tuyển dụng: </td>
          <td colspan="3"> ;Vào cơ quan nào: ;Ở đâu: </td>
        </tr>
        <tr>
          <td colspan="3">13) Ngày vào cơ quan hiện đang công tác: </td>
          <td colspan="3"> ;Ngày tham gia cách mạng: </td>
        </tr>
        <tr>
          <td colspan="3">14) Ngày vào Đảng Cộng sản Việt Nam: </td>
          <td colspan="3"> ;Ngày chính thức: </td>
        </tr>
        <tr>
          <td colspan="6">15) Ngày tham gia các tổ chức chính trị xã hội ( Ngày vào Đoàn TNCSHCM, Công đoàn, Hội ...): </td>
        </tr>
        <tr>
          <td colspan="6">16) Ngày nhập ngũ: ;Ngày xuất ngũ: ;Quân hàm: </td>
        </tr>
        <tr>
          <td colspan="6">17) Trình độ học vấn (Giáo dục phổ thông lớp mấy): ;Học hàm, học vị cao nhất (GS, PGS, TS, Thạc sĩ, Cử nhân, Kỹ sư,…năm nào, chuyên ngành): </td>
        </tr>
        <tr>
          <td colspan="6">- Lý luận chính trị (Cử nhân, Cao cấp, Trung cấp, Sơ cấp): ;Quản lý nhà nước (Chuyên viên cao cấp, Chuyên viên chính ...): ;</td>
        </tr>
      </tbody>
    </table>
    <br />
    <table style="width: 100%">
      <tbody class="tbody">
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
</template>
<style scoped>
#formprint,
#formword {
  background: #fff !important;
}
#formprint *,
#formword * {
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
.thead tr th, .tbody tr td {
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
