<script setup>
import { ref, defineProps, onMounted } from "vue";
const props = defineProps({
  Trangthai: Number,
});
const Trangthai = props.Trangthai;
const tdTrangthais = [
  { value: 0, text: "Đang lập kế hoạch" },
  { value: 1, text: "Đang thực hiện" },
  { value: 2, text: "Đã hoàn thành" },
  { value: 3, text: "Chờ xác nhận" },
  { value: 4, text: "Hoàn thành sau hạn" },
  { value: 5, text: "Tạm dừng" },
  { value: 6, text: "Đóng" },
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
  background-color: #eec137;
  color: #fff;
  font-size: 0.875rem;
}
.ttda-4 {
  background-color: #35c4dc;
  color: #fff;
  font-size: 0.875rem;
}
.ttda-5 {
  background-color: #d97706;
  color: #fff;
  font-size: 0.875rem;
}
.ttda-6 {
  background-color: #dc2626;
  color: #fff;
  font-size: 0.875rem;
}
</style>
