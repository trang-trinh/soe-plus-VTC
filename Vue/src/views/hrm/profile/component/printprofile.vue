<script setup>
import { ref, inject, onMounted } from "vue";
import { encr } from "../../../../util/function";
import { useToast } from "vue-toastification";
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
const cryoptojs = inject("cryptojs");

const props = defineProps({
  profile: Object,
  view: Number,
});

//Function

//Init
onMounted(() => {
  if (props.view === 13 || props.view === 14) {
    let view = props.view;
    //3 là mẫu Mẫu 2c-BNV/2008
    //4 mẫu 2c Mau 2C TCTW-98
    let o = {
      id: view == 13 ? 3 : view == 14 ? 4 : view == 15 ? 5 : 3,
      par: { profile_id: props.profile.profile_id },
    };
    let url = encodeURIComponent(
      encr(JSON.stringify(o), SecretKey, cryoptojs).toString()
    );
    url =
      "https://doconline.soe.vn/report/" +
      url.replaceAll("%", "==") +
      "?v=" +
      (new Date().getTime().toString());
    document.getElementById("IframeDoc1").src = url;
  }
});
</script>
<template>
  <iframe
    id="IframeDoc1"
    frameborder="0"
    class="w-full"
    :style="{ height: 'calc(100vh - 165px)' }"
  />
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
.thead tr th,
.tbody tr td {
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
<style lang="scss" scoped></style>
