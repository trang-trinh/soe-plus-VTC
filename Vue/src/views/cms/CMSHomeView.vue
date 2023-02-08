<script setup>
//Import
import { RouterView } from "vue-router";
import { inject, onMounted, ref } from "vue";
import { useToast } from "vue-toastification";

//Khai báo biến
const toast = useToast();
const store = inject("store");
const axios = inject("axios"); // inject axios
onMounted(() => {
  return {};
});
</script>
<template>
</template>
