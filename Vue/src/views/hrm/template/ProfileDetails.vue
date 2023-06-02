<script>
import { ref, inject, onMounted, computed } from "vue";
import { useToast } from "vue-toastification";
import { encr, decr } from "../../../util/function.js";
import { FilterMatchMode } from "primevue/api";
import DocComponent from "./components/DocComponent.vue";
import { useRoute } from "vue-router";
export default {
  components: {
    DocComponent,
  },
  setup() {
    const store = inject("store");
    const route = useRoute();
    const cryoptojs = inject("cryptojs");
    const axios = inject("axios");
    const toast = useToast();
    const swal = inject("$swal");
    const showLoadding = ref(false);
    const report = ref();
    const pars = ref([]);
    const initBaocao = async (id) => {
      let strSQL = {
        query: false,
        proc: "report_get_key",
        par: [
          {
            par: "report_key",
            va: id,
          },
        ],
      };
      swal.fire({
        width: 110,
        didOpen: () => {
          swal.showLoading();
        },
      });
      const axResponse = await axios.post(
        baseURL + "/api/HRM_SQL/getData",
        {
          str: encr(JSON.stringify(strSQL), SecretKey, cryoptojs).toString(),
        },
        {
          headers: { Authorization: `Bearer ${store.getters.token}` },
        }
      );
debugger
      if (axResponse.status == 200) {
        if (axResponse.data.error) {
          toast.error("Không mở được báo cáo");
        } else {
          report.value = JSON.parse(axResponse.data.data)[0][0];
        }
      }
      swal.close();
    };
    const callbackFun = (dt, ty, pr) => {};
    onMounted(() => {
   
      try {
        let qr = decodeURIComponent(route.params.id.replaceAll("==", "%"));
        let obj = {};
         
        try {
          obj = JSON.parse(decr(qr, SecretKey, cryoptojs));
        } catch (e) {
          obj = { id: qr };
        }
        debugger
        // //3 là mẫu Mẫu 2c-BNV/2008
        // //4 mẫu 2c Mau 2C TCTW-98
        // let o = { id: 3, par: { "profile_id": "auto" } }
        // let url=encodeURIComponent(encr(
        //     JSON.stringify(o),
        //     SecretKey,
        //     cryoptojs
        // ).toString());
        // url="https://doconline.soe.vn/report/"+url.replaceAll('%','==');
        // document.getElementById("IframeDoc").src=url;
        // //<iframe id="IframeDoc" frameborder='0' style="width:100%;height:1005"/>
        // window.open(url);
        if (obj.id) {
           
          initBaocao(obj.id);
          pars.value = obj.par || {};
        }
      } catch (e) {
        toast.error("Không thể mở báo cáo này.");
      }
    });
    return {
      showLoadding,
      report,
      initBaocao,
      callbackFun,
      pars,
    };
  },
};
</script>
<template>
  <div class="bg-white h-full">
    <DocComponent
      :isedit="true"
      v-if="report"
      :pars="pars"
      :report="report"
      :callbackFun="callbackFun"
    ></DocComponent>
  </div>
</template>
<style lang="scss" scoped></style>
