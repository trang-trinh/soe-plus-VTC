<script setup>
import { ref, defineProps, onMounted } from "vue";
const props = defineProps({
  TypeDoc: Number,
});
const TypeDoc = props.TypeDoc;
const tdTypesDoc = [
  { value: 1, text: "Đến", icon: "pi pi-arrow-left" },
  { value: 2, text: "Đi", icon: "pi pi-arrow-right" },
  { value: 3, text: "Nội bộ", icon: "pi pi-undo" }
];
const TypeDocName = tdTypesDoc.find((x) => x.value == TypeDoc).text;
const TypeDocIcon = tdTypesDoc.find((x) => x.value == TypeDoc).icon;
onMounted(() => {
  return {
    TypeDoc,
    TypeDocName,
    TypeDocIcon
  };
});
</script>
<template>
  <i class="m-1" :class="TypeDocIcon" v-tooltip.top="TypeDocName" ></i>
</template>
<style scoped>

</style>
