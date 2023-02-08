<script setup>
import { ref, inject, onMounted } from "vue";
import { useToast } from "vue-toastification";
import CompMenu from "./comp/CompMenu.vue";
import CompCavan from "./comp/CompCavan.vue";
const toast = useToast();
const swal = inject("$swal");
</script>
<template>
  <div class="main-layout flex h-full flex-grow-1 p-2">
    <CompMenu></CompMenu>
    <CompCavan></CompCavan>
  </div>
</template>

<style scoped>
.main-layout {
  background-color: #eee;
}
</style>
