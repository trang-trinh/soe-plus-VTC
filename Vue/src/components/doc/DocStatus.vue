<script setup>
import { ref, onMounted } from "vue";
const props = defineProps({
  DocObj: Object,
});
const DocObj = props.DocObj
const color = ref({
    backgroundColor: DocObj.background_color,
    color: DocObj.text_color
})
onMounted(() => {
  return {
   DocObj,
   color
  };
});
</script>
<template>
  <Chip v-bind:label="DocObj.status_name" :style="color" />
</template>
<style lang="scss" scoped>
.p-chip{
    border-radius: 5px;
}
</style>
