<script setup>
import LoginView from "./LoginView.vue";
import { inject, onMounted } from "vue";
const store = inject("store");
onMounted(() => {
  return {};
});
</script>
<template>
  <LoginView v-if="!store.getters.islogin" />
  <div
    class="main-layout flex-grow-1"
    v-if="store.getters.islogin"
    style="padding: 400px"
  ></div>
</template>
<style scoped>
.main-layout {
  background-color: #eee;
}
</style>
