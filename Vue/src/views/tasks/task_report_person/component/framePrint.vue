<script setup>
const props = defineProps({
  datas: Array,
});
</script>
<template>
  <!-- style="display: none" -->
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
              <div class="uppercase">Bộ quốc phòng</div>
              <div class="uppercase">
                <b>Bảo hiểm xã hội</b>
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
              <div class="uppercase">
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
              <div style="padding: 1rem 0"></div>
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
                <div class="uppercase title2">
                  <b>Tổng hợp đánh giá công việc</b>
                </div>
              </div>
            </td>
          </tr>
        </thead>
      </table>
      <table>
        <thead class="boder">
          <tr>
            <th style="width: 30px">STT</th>
            <th style="width: 200px">Họ và tên</th>
            <th style="width: 100px">Phòng ban</th>
            <th style="width: 110px">Tên báo cáo</th>
            <th style="width: 150px">Điểm tự đánh giá</th>
            <th style="width: 130px">Điểm đánh giá</th>
            <th style="width: 130px">Ghi chú</th>
          </tr>
        </thead>
        <tbody class="boder">
          <tr v-for="(item, index) in props.datas" :key="index">
            <td style="width: 30px" align="center">{{ index + 1 }}</td>
            <td style="width: 200px">
              {{ item.user_info.full_name }}
            </td>
            <td style="width: 150px" align="center">
              {{ item.user_info.position_name }}
              <br />
              {{ item.user_info.department_name }}
            </td>
            <td style="width: 200px">{{ item.report_name }}</td>
            <td style="width: 130px" align="center">{{ item.self_point }}</td>
            <td style="width: 130px" align="center">
              {{ item.reviewed_point }}
            </td>
            <td style="width: 130px"></td>
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
