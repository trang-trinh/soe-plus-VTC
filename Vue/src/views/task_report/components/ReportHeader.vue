<script setup>
import { ref, inject, onMounted, watch, onBeforeUnmount } from "vue";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
import { required } from "@vuelidate/validators";
import moment from "moment";
import { concat } from "lodash";
import { encr } from "../../../util/function.js";
import router from "@/router";
const cryoptojs = inject("cryptojs");
const emitter = inject("emitter");
const basedomainURL = fileURL;
const toast = useToast();
const swal = inject("$swal");
const store = inject("store");
const axios = inject("axios"); // inject axios
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const addLog = (log) => {
  axios.post(baseURL + "/api/Proc/AddLog", log, config);
};
const options = ref({
  search: "",
});
const props = defineProps({
  headersName: String,
  loadData: Function,
  refresh: Function,
});
const vla = ref();
const Change = (e) => {
  vla.value = e;
};
const listButton = ref([
  {
    label: "Tải lại",
    icon: "pi pi-refresh",
    value: 0,
    active: false,
  },
  {
    label: "Lọc",
    icon: "pi pi-filter",
    value: 1,
    active: false,
  },
  {
    label: "Sắp xếp",
    icon: "pi pi-sort",
    value: 2,
    active: false,
  },
  {
    label: "Tiện ích",
    icon: "pi pi-file",
    value: 3,
    active: false,
  },
]);
const Filter = ref();
const Sort = ref();
const utilities = ref();
const Switch = (e, va) => {
  if (va == 0) props.refresh();
  if (va == 1) Filter.value.toggle(e);
  if (va == 2) Sort.value.toggle(e);
  if (va == 3) utilities.value.toggle(e);
  else return;
};
onMounted(() => {});
</script>
<template>
  <div class="flex justify-content-center align-items-center">
    {{ vla }}
    <Toolbar class="w-full custoolbar">
      <template #start>
        <div class="flex justify-content-center align-items-center">
          <span class="p-input-icon-left">
            <i class="pi pi-search" />
            <InputText
              style="min-width: 300px; margin-right: 10px"
              type="text"
              spellcheck="false"
              v-model="options.search"
              placeholder="Tìm kiếm"
              @keyup.enter="props.loadData()"
            />
          </span>
          <h3 class="w-full">{{ props.headersName }}</h3>
        </div>
      </template>
      <template #end>
        <Button
          v-for="(item, index) in listButton"
          :key="index"
          :label="item.label"
          :icon="item.icon"
          outlined
          severity="secondary"
          class="flex align-items-center mx-1"
          @click="Switch($event, item.value)"
        >
        </Button>
      </template>
    </Toolbar>
  </div>
  <OverlayPanel ref="Filter"> </OverlayPanel>
  <OverlayPanel ref="Sort"> </OverlayPanel>
  <OverlayPanel ref="utilities"> </OverlayPanel>
</template>

<style lang="scss" scoped></style>
