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
  paycheck: Object,
});
</script>
<template>
  <Dialog
    :visible="true"
    :closable="false"
    :style="{ width: '50vw' }"
    style="display: none"
     
  >
    <div id="formword">
      <div v-if="props.datas">
        <div>
        <table style="width: 100%; margin-bottom: 24px">
          <thead>
            <tr>
              <td style="width: 40%; vertical-align: bottom ; padding-bottom: 12px ; font-size: 13pt;">
                <div>
                  Kính gửi Ông/Bà:
                  <span style="font-weight: 600 ;  ">
                    {{ props.paycheck.profile_user_name }}</span
                  >
                </div>
              </td>
            </tr>
            <tr>
              <td style="vertical-align: bottom; padding-bottom: 12px ; font-size: 13pt; ">
             <div>   Ban Tổ chức {{store.getters.user.organization_name}} trân trọng
                gửi tới Ông/Bà thông tin chi trả tiền lương tháng
                {{
                  moment(new Date(props.paycheck.paycheck_form_date)).format(
                    " MM/YYYY"
                  )
                }}
                như sau:</div>
              </td>
            </tr>
          </thead>
        </table>
    </div>
    <div>
        <table border="1" width="100%" cellpadding="0" style="padding-top: 24px;">
          <tbody>
         
            <tr  
              v-for="(item, index) in props.datas"
              :key="index"
              :style="item.paycheck_type == 1 ? 'font-weight:600' : ''"
            >
              <td  style="width: 10%; padding: 6px 0px; font-size: 13pt;">
                <div class="format-center">
                  {{ item.type_order }}
                </div>
              </td>
              <td style="width: 65% ;padding: 6px 0px; padding-left: 6px; font-size: 13pt;">
                <div class=" ">{{ item.paycheck_name }}</div>
              </td>
              <td style="text-align: right ; width: 25%;padding: 6px 0px;padding-right: 8px; font-size: 13pt;">
           <span v-if="item.paycheck_input" > {{ item.paycheck_input.toLocaleString() }} {{ item.paycheck_unit }} </span>
              </td>
            </tr>
          </tbody>
        </table></div>
      </div>
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
  font-size: 24pt !important;
}
.title2,
.title2 * {
  font-size: 20pt !important;
}
.title3,
.title3 * {
  font-size: 16pt !important;
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
