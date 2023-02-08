<script setup>
import { defineProps, onMounted } from "vue";
const props = defineProps({
  Trangthai: Number,
});
const Trangthai = props.Trangthai;
const tdTrangthais = [
  { value: 0, text: "Đang lập kế hoạch" },
  { value: 1, text: "Đang thực hiện" },
  { value: 2, text: "Đã hoàn thành" },
  { value: 3, text: "Tạm dừng" },
  { value: 4, text: "Đóng" },
];
const TenTrangthai = tdTrangthais.find((x) => x.value == Trangthai).text;
onMounted(() => {
  return {
    Trangthai,
    TenTrangthai,
  };
});
</script>
<template>
  <Chip v-bind:label="TenTrangthai" v-bind:class="'ttda-' + Trangthai" />
</template>
<style scoped>
.ttda-0 {
  background-color: #dee2e6;
  font-size: 0.875rem;
}
.ttda-1 {
  background-color: #3b82f6;
  color: #fff;
  font-size: 0.875rem;
}
.ttda-2 {
  background-color: #16a34a;
  color: #fff;
  font-size: 0.875rem;
}
.ttda-3 {
  background-color: #d97706;
  color: #fff;
  font-size: 0.875rem;
}
.ttda-4 {
  background-color: #dc2626;
  color: #fff;
  font-size: 0.875rem;
}
</style>
