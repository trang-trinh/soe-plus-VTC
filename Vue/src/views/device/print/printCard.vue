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
  
});
</script>
<template>
  <Dialog :visible="true" :closable="false" :style="{ width: '50vw' }" style="display: none">
    <div id="formwordlist"><div  >
      <table style="width: 100%;">
        <thead>
          <tr>
            <td
              class="text-center"
              colspan="2"
              style="width: 100%; vertical-align: bottom"
            >
             
              <div  >
                <b>THÔNG TIN THẺ THIẾT BỊ</b>
                
              </div>
            </td>
            
          </tr>
          
        </thead>
 s
      </table>
         <table style="width: 100%;">
        <thead>
          <tr>
            <td
              class="text-center"
              colspan="2"
              style="width: 100%; vertical-align: bottom"
            >
             
              <div  >
                <b>{{props.datas.image}}</b>
                
              </div>
            </td>
            
          </tr>
          
        </thead>
      </table>

 
 
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
