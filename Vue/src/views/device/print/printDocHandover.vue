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
  datas: Object,
  listAssets: Array,
});
</script>
<template>
  <Dialog :visible="true" :closable="false" :style="{ width: '50vw' }" style="display: none">
    <div id="formword"><div  v-if="props.datas" >
      <table style="width: 100%;">
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
              <div style="padding: 1rem 0">Số:{{props.datas.handover_number}}</div>
            </td>
            <td class="text-center" colspan="4">
              <div style="padding: 1rem 0">
                <i>Hà Nội, ngày {{ new Date(props.datas.handover_created_date).getDate() > 9
      ? new Date(props.datas.handover_created_date).getDate()
      : "0" + new Date(props.datas.handover_created_date).getDate()}}, tháng {{ new Date(props.datas.handover_created_date).getMonth() + 1 > 9
      ? new Date(props.datas.handover_created_date).getMonth() + 1
      : "0" +
        (new Date(props.datas.handover_created_date).getMonth() + 1)}}, năm {{  new Date(props.datas.handover_created_date).getFullYear() }}</i>
              </div>
            </td>
          </tr>
          <tr>
            <td class="text-center" colspan="6">
              <div style="padding: 1rem 0">
                <div class="uppercase title2"><b>Biên bản bàn giao</b></div>
                <div class="">
                     
                  <!-- <i>
                    (Từ ngày
                    {{ moment(props.week_start_date).format("DD/MM/YYYY") }} đến ngày
                    {{ moment(props.week_end_date).format("DD/MM/YYYY") }})
                  </i> -->
                </div>
              </div>
            </td>
          </tr>
        </thead>
      </table>
       



 <table border='0' width='1024' cellpadding='10'><tbody><tr><td><p style='font-size:16px;'>Hôm nay, ngày 
    {{new Date(props.datas.handover_created_date).getDate() > 9
      ? new Date(props.datas.handover_created_date).getDate()
      : "0" + new Date(props.datas.handover_created_date).getDate()}} tháng  
 {{ new Date(props.datas.handover_created_date).getMonth() + 1 > 9
      ? new Date(props.datas.handover_created_date).getMonth() + 1
      : "0" +
        (new Date(props.datas.handover_created_date).getMonth() + 1)}} năm  
        {{new Date(props.datas.handover_created_date).getFullYear()}} tại {{ props.datas.user_receiver_department_name}}, chúng tôi gồm:</p></td></tr></tbody></table>
 <table border='0' width='1024' cellpadding='10'><tbody><tr><td><p style='font-size:16px;text-transform:uppercase;font-weight:bold;'> 
  A. Bên giao: 
    {{props.datas.user_deliver_department_name}}  
    </p></td></tr></tbody></table>
 <table border='0' width='1024' cellpadding='10'><tbody><tr><td width='50%'><p style='font-size:16px;'>Đồng chí: 
    {{props.datas.user_deliver_name}}  </p></td>
 <td width='50%'><p style='font-size:16px;'>Chức vụ: 
   {{ props.datas.user_deliver_position}}</p></td></tr>
  </tbody></table>
 <table border='0' width='1024' cellpadding='10'><tbody><tr><td><p style='font-size:16px;text-transform:uppercase;font-weight:bold;'> 
    B. Bên nhận:  
   {{ props.datas.user_receiver_department_name}}</p></td></tr></tbody></table> 
   <table border='0' width='1024' cellpadding='10'><tbody><tr><td width='50%'><p style='font-size:16px;'>Đồng chí: 
    {{ props.datas.user_receiver_name}}</p></td> 
 <td width='50%'><p style='font-size:16px;'>Chức vụ: 
   {{props.datas.user_receiver_position}}</p></td></tr> 
   </tbody></table> 
 <table v-if="props.datas.handover_type == 1" border='0' width='1024' cellpadding='10'><tbody><tr><td><p style='font-size:16px;text-transform:uppercase;font-weight:bold;'> 
      C. Bên xác nhận: 
     {{ props.datas.user_verifier_department_name}}  
 </p></td></tr></tbody></table> 
 <table v-if="props.datas.handover_type == 1"  border='0' width='1024' cellpadding='10'><tbody><tr><td width='50%'><p style='font-size:16px;'>Đồng chí: 
       {{props.datas.user_verifier_name}}</p></td> 
 <td width='50%'><p style='font-size:16px;'>Chức vụ:  
       {{props.datas.user_verifier_position}}</p></td></tr> 
 </tbody></table> 
 <table border='0' width='100%' cellpadding='10'><tbody><tr><td><p style='font-size:16px;'>Bên giao tiến hành bàn giao các tài sản như sau:</p></td></tr></tbody></table> 
 <table border='0' width='100%' cellpadding='10' style='border-spacing:0;padding-left:15px;padding-right:15px;'><thead><tr> 
 <th align='center' width='10%' style='border:1px solid #000 !important;font-size:16px !important;'>STT</th> 
 <th align='center' width='15%' style='border:1px solid #000 !important;font-size:16px !important;border-left:none !important;'>Số hiệu</th> 
  <th align='center' width='15%' style='border:1px solid #000 !important;font-size:16px !important;border-left:none !important;'>Mã barcode</th> 
 <th align='center' style='border:1px solid #000 !important;font-size:16px !important;border-left:none !important;'>Tên thiết bị</th> 
   <th align='center' width='10%' style='border:1px solid #000 !important;font-size:16px !important;border-left:none !important;'>Đơn vị tính</th> 
 <th align='center' width='10%' style='border:1px solid #000 !important;font-size:16px !important;border-left:none !important;'>Giá trị</th> 
 <th align='center' width='20%' style='border:1px solid #000 !important;font-size:16px !important;border-left:none !important;'>Tình trạng</th> 
 </tr></thead> 
 <tbody>

<tr v-for="(ts, index) in props.listAssets" :key="index">
  <td width='10%' align='center' style='border:1px solid #000 !important;font-size:16px !important;border-top:none !important;'> 
      {{index +1}} </td>
<td width='15%' align='center' style='border:1px solid #000 !important;font-size:16px !important;border-left:none !important;border-top:none !important;'>
    {{ ts.device_number}}
</td>
<td width='15%' align='center' style='border:1px solid #000 !important;font-size:16px !important;border-left:none !important;border-top:none !important;'> 
  {{ts.barcode_id}}  </td>
<td align='left' style='border:1px solid #000 !important;font-size:16px !important;border-left:none !important;border-top:none !important;'> 
      {{ts.device_name}}  </td>
<td width='10%' align='center' style='border:1px solid #000 !important;font-size:16px !important;border-left:none !important;border-top:none !important;'> 
     {{ ts.device_unit}} </td>
<td width='10%' align='center' style='border:1px solid #000 !important;font-size:16px !important;border-left:none !important;border-top:none !important;padding:10px 5px;'> 
        {{ts.price != null ? ts.price.toLocaleString() : " 0 "}}
       VND  </td>
<td width='20%' align='left' style='border:1px solid #000 !important;font-size:16px !important;border-left:none !important;border-top:none !important;'> 
     {{ ts.assets_condition}}  </td></tr>
 </tbody></table> 
  <!-- htmltable +=
    "<table border='0' width='1024' cellpadding='0' style='padding:10px 15px 0px;'><tbody>";
  htmltable +=
    "<tr><td style='font-size:16px;font-weight:bold;text-align:right;'><a style='padding-right:20px'>Tổng giá trị: </a>" +
    sum.toLocaleString() +
    " VND </td></tr></tbody></table>"; -->

    <table v-if="  props.datas.print_note == null ||
    props.datas.print_note ==''" border='0' width='100%' cellpadding='10'><tbody><tr><td colspan='7'><p style='font-size:16px;text-align:justify;'> 
 Biên bản được lập thành 04 bản, mỗi bên giữ 02 bản có giá trị như nhau.</p></td></tr></tbody></table>
  <table v-else border='0' width='100%' cellpadding='10'><tbody><tr><td colspan='7'><p style='font-size:16px;text-align:justify;'> 
      {{props.datas.print_note }}</p></td></tr></tbody></table>
 <table border='0' width='100%' cellpadding='10'><tbody><tr> 
 <td :width="(props.datas.handover_type == 1 ? 33 : 50)" style='text-align:center'>  
 <p style='font-weight:bold;font-size:16px;text-align:center;text-transform:uppercase;'>Bên nhận</p> 
 <p style='font-size:16px;text-align:center; padding-top:24px'> 
    {{props.datas.user_receiver_name}}  </p>
    </td> 
 <td :width="(props.datas.handover_type == 1 ? 33 : 50)" style='text-align:center;'> 
    <p style='font-weight:bold;font-size:16px;text-align:center;text-transform:uppercase;'>Bên giao</p> 
 <p style='font-size:16px;text-align:center; padding-top:24px'> 
    {{props.datas.user_deliver_name}}  
    </p>
    </td> 
 <td v-if="props.datas.handover_type == 1" width='34%'><p style='font-size:16px;text-align:center'>  
    <p style='font-weight:bold;font-size:16px;text-align:center;text-transform:uppercase;'>Bên xác nhận</p> 
    <p style='font-size:16px;text-align:center; padding-top:24px'> 
      {{(props.datas.user_verifier_name != null
        ? props.datas.user_verifier_name
        : "") }} </p>
        </p>
        
        </td> 
  
  </tr></tbody></table> 
























      <!-- <table style="width: 100%;">
        <thead class="boder">
          <tr>
            <th style="width: 50px">TT</th>
            <th style="width: 180px">Họ và tên</th>
            <th style="width: 150px">Cấp bậc, chức vụ</th>
           
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
         
            <td align="center">
              <div>{{ value.day_string }}</div>
            </td>
            <td align="center">
              <div>{{ value.day_name }}</div>
            </td>
            <td>
              <div v-if="value.chihuys && value.chihuys.length > 0">
                {{ value.chihuys[0].full_name }}
              </div>
            </td>
          </tr>
        </tbody>
      </table> -->
      
      <!-- <table style="width: 100%">
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
                <div style="text-transform: uppercase"><b>KT.Giám Đốc</b></div>
                <div style="text-transform: uppercase"><b>Phó GIám Đốc</b></div>
                <div style="height: 170px; position: relative"></div>
              </div>
            </td>
          </tr>
        </tbody>
      </table> -->
    </div> </div>
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
