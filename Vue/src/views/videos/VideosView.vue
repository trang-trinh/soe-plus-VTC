<script setup>
import { ref, inject, onMounted, watch } from "vue";
import videoview from "../../components/videos/videoview.vue"

//Khai báo
const keywords = ref();
const display = ref(true);
// setTimeout(()=>{
//   display.value = true;
// }, 1000)
onMounted(() => {
});
</script>
<template>
<div v-if="display">
  <videoview :keywords="keywords" :display="display"></videoview>
</div>
</template>


