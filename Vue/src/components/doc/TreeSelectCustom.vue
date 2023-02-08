<script setup>
import { ref, defineProps, onMounted } from "vue";
const props = defineProps({
  model: Object,
  propertyName: String,
  options: Array,
  placeholder: String,
  removeTree: Function,
  disabled: Boolean
});
const stopprop = (event) => {
  event.stopPropagation();
}
onMounted(() => {
  return {
  };
});
</script>
<template>
  <TreeSelect :disabled="disabled" v-model="model[propertyName]" :options="options"
                :max-height="200" :placeholder="placeholder">
                <template #value="{value}">
                    <div class="flex" style="justify-content: space-between">
                      <div>{{value[0] ? value[0].label : placeholder}}</div>
                      <Button v-if="value[0]" @click="stopprop($event); removeTree(model, propertyName)" icon="pi pi-times" class="p-button-link p-0 rm-dropdown" />
                    </div>
                </template>
              </TreeSelect>
</template>
<style lang="scss" scoped>
::v-deep(.p-treeselect-label){
  padding-right: 0;
}
.rm-dropdown{
    color: #6c757d !important;
}
</style>
