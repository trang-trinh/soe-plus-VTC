<script setup>
import { defineProps, onMounted, ref, inject } from "vue";
import { useToast } from "vue-toastification";
import moment from "moment";

const axios = inject("axios"); // inject axios
const store = inject("store");
const swal = inject("$swal");
const toast = useToast();
const emitter = inject("emitter");
const router = inject("router");
const basedomainURL = fileURL;
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const props = defineProps({
  headerDialog: String,
  array: Array,
  displayDialog: Boolean,
  closeDialog: Function,
  selectModel: Function
});
const selectedIssuePlace = ref();
onMounted(() => {
//   if (props.displayDialog) {
//     LoadLinkTaskOrigin();
//   }
  return {

  };
});
</script>
<template>
   <Dialog :modal="true" :header="props.headerDialog" v-model:visible="props.displayDialog" style="z-index: 1000"
    :style="{ width: '50vw' }">
    <div>
          <form>
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12 flex">
            <form>
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 m-0 p-0">
            <div style="justify-content: space-between;" v-for="item in props.array" :key="item.issue_place_id" class="code-item mb-3 flex">
             <div class="flex">
              <RadioButton :inputId="item.issue_place_id" name="item" :value="item" v-model="selectedIssuePlace" class="mr-5" />
                <label :for="item.issue_place_id"><h3 class="m-0">{{ item.display_name }}</h3></label>
             </div>
            </div>
          </div>
        </div>
      </div>
    </form>
        </div>
      </div>
    </form>
        </div>
        <template #footer>
      <Button label="Hủy" icon="pi pi-times" @click="props.closeDialog" class="p-button-text" />
      <Button label="Chọn" icon="pi pi-check" @click="props.selectModel(selectedIssuePlace); props.closeDialog()" />
    </template>
  </Dialog>
</template>
<style>

</style>
