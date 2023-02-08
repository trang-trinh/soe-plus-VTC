<script setup>
import { ref, defineProps, inject, onMounted } from "vue";
import { encr } from "../../util/function";
const cryoptojs = inject("cryptojs");
const axios = inject("axios"); // inject axios
const store = inject("store");
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const props = defineProps({
  
});
const emitter = inject("emitter");
const defCountDoc = ref([
  {ord: 4, text: 'Đã liên kết công việc', code: 'reltask', color: '#51b7ae'},
  {ord: 5, text: 'Nhận chưa đọc', code: 'notseen', color: '#4167b2'},
  {ord: 6, text: 'Chờ xử lý', code: 'handle', color: '#2196f3'},
  {ord: 7, text: 'Quá hạn', code: 'ood', color: '#f54335'}
]);
const loadCountDoc = () => {
  axios
    .post(
      baseURL + "/api/DocProc/CallProc",
      {str: 
        encr(JSON.stringify(
        {
        proc: "doc_master_receive_count",
        par: [
          { par: "user_key", va: store.getters.user.user_key }
        ],
      }
      ),
        SecretKey, cryoptojs)
        .toString()
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        defCountDoc.value.forEach(function (r) {
            r.value = data[0][r.code];
        });
      } 
    })
    .catch((error) => {
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
      }
    });
};
const onChangeCountDoc = (value) => {
    emitter.emit("emitData", { type: "changeAdditionalCount", data: value });
}
onMounted(() => {
  emitter.on("emitData", (obj) => {
    switch (obj.type) {
      case "loadCountDocFilter":
        if (obj.data) {
          if (obj.data.length > 0) {
            let data = obj.data;
            defCountDoc.value.forEach(function (r) {
            r.value = data[0][r.code];
        });
          }
        }
        break;
      default: break;
    }
});
    loadCountDoc();
  return {
   
  };
});
</script>
<template>
    <span @click="onChangeCountDoc(btn.ord)" v-tooltip="btn.text" :style="{ backgroundColor: btn.color}" v-for="btn in defCountDoc" :key="btn.code" class="btn-count">{{btn.value}}</span>
</template>
<style lang="scss" scoped>
.p-button {
    margin-right: 0.5rem;
    width: 35px;
    height: 32px;
    border: none;
}
.btn-count{
    user-select: none;
    width: 28px;
    height: 28px;
    border-radius: 50%;
    text-align: center;
    padding-top: 6px;
    margin-right: 0.5rem;
    color: #fff;
    cursor: pointer;
}
</style>
