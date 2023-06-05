<script setup>
import { onMounted, ref } from "vue";

//Get arguments
const props = defineProps({
  headerDialog: String,
  displayDialog: Boolean,
  closeDialog: Function,
  options: Array,
  bindData: Function,
});

const display = ref(props.displayDialog);

//declare
const options = ref({});
const outputs = ref([]);

//function

//init
onMounted(() => {
  if (props.displayDialog) {
  }
});
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
    <form @submit.prevent="" name="submitform">
      <div class="grid formgrid m-2">
        <div class="col-5 md:col-5">
          <DataTable :value="props.options">
            
          </DataTable>
        </div>
        <div class="col-2 md:col-2"></div>
        <div class="col-5 md:col-5">
          <DataTable
            :value="outputs"
            :reorderableColumns="true"
            responsiveLayout="scroll"
          >
          </DataTable>
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="props.closeDialog()"
        class="p-button-text"
      />
      <Button
        label="Xuất file"
        icon="pi pi-file-excel"
        @click="props.bindData(outputs)"
      />
    </template>
  </Dialog>
</template>
<style></style>
