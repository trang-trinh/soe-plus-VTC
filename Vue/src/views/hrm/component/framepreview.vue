<script setup>
import { ref, inject, onMounted, onBeforeMount, nextTick } from "vue";
import { encr } from "../../../util/function";
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

//Get arguments
const props = defineProps({
  headerDialog: String,
  displayDialog: Boolean,
  closeDialog: Function,
  type: Number,
  model: Object,
});

const display = ref(props.displayDialog);
//init
const initData = () => {
  switch (props.type) {
    case 1:
      // Hồ sơ
      if (props.model && props.model.report_key) {
        let o = {
          id: props.model.report_key,
          par: { profile_id: props.model.profile_id, isedit: props.model.isEdit || false },
        };
        let url = encodeURIComponent(
          encr(JSON.stringify(o), SecretKey, cryoptojs).toString()
        );
        url =
          "https://doconline.soe.vn/report/" +
          url.replaceAll("%", "==") +
          "?v=" +
          new Date().getTime().toString();
        document.getElementById("IframeDoc2").src = url;
      } else {
        swal.fire({
          title: "Thông báo!",
          text: "Chưa thiết lập mẫu in cho hồ sơ!",
          icon: "error",
          confirmButtonText: "OK",
        });
        return;
      }
      break;
    case 2:
      // Hợp đồng
      if (props.model && props.model.report_key) {
        let o = {
          id: props.model.report_key,
          par: { contract_id: props.model.contract_id, isedit: props.model.isEdit || false },
        };
        let url = encodeURIComponent(
          encr(JSON.stringify(o), SecretKey, cryoptojs).toString()
        );
        url =
          "https://doconline.soe.vn/report/" +
          url.replaceAll("%", "==") +
          "?v=" +
          new Date().getTime().toString();
        document.getElementById("IframeDoc2").src = url;
      } else {
        swal.fire({
          title: "Thông báo!",
          text: "Chưa thiết lập mẫu in cho loại hợp đồng!",
          icon: "error",
          confirmButtonText: "OK",
        });
        return;
      }
      break;
    case 3:
      // Quyết định
      if (props.model && props.model.report_key) {
        let o = {
          id: props.model.report_key,
          par: { decision_id: props.model.decision_id, isedit: props.model.isEdit || false },
        };
        let url = encodeURIComponent(
          encr(JSON.stringify(o), SecretKey, cryoptojs).toString()
        );
        url =
          "https://doconline.soe.vn/decided/" +
          url.replaceAll("%", "==") +
          "?v=" +
          new Date().getTime().toString();
        "?v=" + new Date().getTime().toString();
        document.getElementById("IframeDoc2").src = url;
      } else {
        swal.fire({
          title: "Thông báo!",
          text: "Chưa thiết lập mẫu in cho loại quyết định!",
          icon: "error",
          confirmButtonText: "OK",
        });
        return;
      }
      break;
  }
};
onMounted(() => {
  if (props.displayDialog) {
    nextTick(() => {
      initData();
    });
  }
});
</script>
<template>
  <!-- <Button @click="showDialog = true" label="Show Dialog" /> -->
  <Dialog
    v-model:visible="display"
    appendTo="body"
    :closable="false"
    :style="{
      zIndex: '9001',
    }"
    class="p-dialog-maximized p-dialog-content-padding-0"
  >
    <template #header>
      <Toolbar class="outline-none surface-0 border-none p-0 w-full">
        <template #start>
          <h2 class="m-0">{{ props.headerDialog }}</h2>
        </template>
        <template #end>
          <Button
            @click="props.closeDialog()"
            label="Đóng"
            icon="pi pi-times"
            class="p-button-text"
          />
        </template>
      </Toolbar>
    </template>
    <div class="w-full h-full">
      <iframe id="IframeDoc2" frameborder="0" class="w-full h-full" />
    </div>
  </Dialog>
</template>
<style scoped>
.p-dialog-header-icons{
  display: none !important;
}
</style>
