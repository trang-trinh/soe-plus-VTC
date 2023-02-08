<script setup>
import { ref, inject, onMounted, watch } from "vue";
import videoview from "../../components/videos/videoview.vue"

//Khai báo
const keywords = ref(
  window.location.href.substring(
    window.location.href.lastIndexOf("keywords-")+ 9
  ).replaceAll('-', ' ')
);
const click = ()=>{
    display.value = true
}
const display = ref(true);
// setTimeout(()=>{
//   display.value = true;
// }, 1000)
onMounted(() => {
});
</script>
<template>
<!-- <button @click="click" style="witdh:100px ; height:100px">click me</button> -->
<div v-if="display">
  <videoview :keywords="keywords" :display="display"></videoview>
</div>
</template>


