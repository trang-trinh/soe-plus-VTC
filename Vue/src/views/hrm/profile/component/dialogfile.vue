<script setup>
import moment from "moment";
import { onMounted, inject, ref } from "vue";
import { useToast } from "vue-toastification";
import { encr } from "../../../../util/function";
const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios");
const cryoptojs = inject("cryptojs");
const toast = useToast();
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const basedomainURL = baseURL;

//Get arguments
const props = defineProps({
  key: Number,
  headerDialog: String,
  displayDialog: Boolean,
  closeDialog: Function,
  file: Object,
});
const display = ref(props.displayDialog);
</script>
<template>
  <Dialog
    :header="props.headerDialog"
    v-model:visible="display"
    :style="{ width: '60vw' }"
    :maximizable="true"
    :closable="true"
    style="z-index: 9000"
  >
    <form
      @submit.prevent=""
      name="submitform"
      class="h-full"
      :style="{ overflow: 'hidden' }"
    >
      <div
        class="grid formgrid m-2 h-full"
        :style="{ minHeight: 'calc(100vh - 300px)' }"
      >
        <div class="col-12 md:col-12">
          <div class="form-group m-0 h-full">
            <iframe
              v-if="props.file.file_path"
              :src="
                basedomainURL +
                '/Viewer?title=' +
                props.file.file_name +
                '&url=' +
                props.file.file_path
              "
              class="w-full h-full"
            ></iframe>
          </div>
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Đóng"
        icon="pi pi-times"
        @click="props.closeDialog()"
        class="p-button-text"
      />
    </template>
  </Dialog>
</template>
<style scoped>
@import url(../../profile/component/stylehrm.css);
</style>
