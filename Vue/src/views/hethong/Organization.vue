<script setup>
import { ref, inject, onMounted, watch } from "vue";
import organizationList from "../../components/system/organization/OrganizationList.vue"
import organizationTree from "../../components/system/organization/OrganizationTree.vue"
const emitter = inject("emitter");
const isViewList = ref(false);
const isViewTree = ref(true);
emitter.on("emitData", (obj) => {
  switch (obj.type) {
    case "change_type":
      isViewList.value =  obj.data["isViewList"];
      isViewTree.value =  obj.data["isViewTree"];
      break;
  }
});
onMounted(() => {

});
</script>
<template>
    <organizationList v-if="isViewList"></organizationList>
    <organizationTree v-if="isViewTree"></organizationTree>
</template>