<script setup>
import { ref, inject, onMounted, watch } from "vue";
import userList from "../../components/system/user/UserList.vue"
import userTree from "../../components/system/user/UserTree.vue"
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
    <userList v-if="isViewList"></userList>
    <userTree v-if="isViewTree"></userTree>
</template>