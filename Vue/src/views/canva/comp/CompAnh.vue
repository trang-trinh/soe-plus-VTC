<script setup>
import { ref, inject, defineProps, onMounted } from "vue";
import { store } from "../../../store/store";
import { useToast } from "vue-toastification";
const emitter = inject("emitter");
const props = defineProps({
  id: String,
});
const toast = useToast();
const swal = inject("$swal");
const axios = inject("axios"); // inject axios
const basedomainURL = fileURL;
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const search = ref("");
const onSearch = () => {};
const typeAnh = ref(0);
const listImages = ref([]);
const optionButtons = ref([
  { name: "Hệ thống", value: 0 },
  { name: "Của tôi", value: 1 },
  { name: "Tải lên", value: 2 },
]);
const opition = ref({
  search: "",
  PageNo: 0,
  PageSize: 20,
  loading: false,
  isEnd: false,
});
const loadAnh = (rf) => {
  if (opition.value.loading || opition.value.isEnd) {
    return false;
  }
  if (rf) {
    opition.value.loading = true;
    opition.value.PageNo += 1;
  }
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "Image_List",
        par: [
          { par: "user_id", va: store.getters.user.user_id },
          { par: "PageNo", va: opition.value.PageNo },
          { par: "PageSize", va: opition.value.PageSize },
          { par: "Search", va: opition.value.search },
        ],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      data.forEach((f) => {
        let idx = f.Duongdan.lastIndexOf(".");
        f.DuongdanThumb =
          basedomainURL +
          f.Duongdan.substring(0, idx) +
          "_thumb" +
          f.Duongdan.substring(idx);
        f.filetype = f.Duongdan.substring(idx + 1);
        f.Duongdan = basedomainURL + f.Duongdan;
      });
      if (rf) {
        opition.value.loading = false;
      }
      if (data.length > 0) listImages.value = listImages.value.concat(data);
      else opition.value.isEnd = true;
    })
    .catch((error) => {
      console.log(error);
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
      }
    });
};
const goImage=(img)=>{
  emitter.emit("clickMenuItem",{type:"img",data:img});
}
onMounted(() => {
  listImages.value = [];
  opition.value.PageNo = 1;
  return {};
});
</script>
<template>
  <div class="main-layout flex flex-column h-full flex-grow-1 p-0">
    <span class="p-input-icon-left w-full">
      <i class="pi pi-search" />
      <InputText
        type="text"
        spellcheck="false"
        v-model="search"
        placeholder="Tìm kiếm"
        v-on:keyup.enter="onSearch"
      />
    </span>
    <Button icon="pi pi-upload" label="Tải ảnh lên" class="p-button-info w-full mt-2" />
    <SelectButton
      class="mt-2"
      v-model="typeAnh"
      optionLabel="name"
      optionValue="value"
      :lazy="true"
      :options="optionButtons"
      @lazy-load="loadAnh(true)"
    />
    <div class="divCanvaImage">
      <!-- <img v-for="item in listImages" v-bind:src="item.DuongdanThumb" /> -->
      <VirtualScroller
        :items="listImages"
        :itemSize="50"
        :loading="opition.loading"
        :lazy="true"
        @lazy-load="loadAnh(true)"
      >
        <template v-slot:item="{ item, options }">
          <img @click="goImage(item)" loading="lazy" v-bind:src="item.DuongdanThumb" />
        </template>
      </VirtualScroller>
    </div>
  </div>
</template>
<style scoped>
.main-layout {
  background-color: #fff;
}
.divCanvaImage {
  height: 100%;
  flex: 1;
  overflow-y: auto;
}
.divCanvaImage img {
  width: 115px;
  margin-top: 5px;
  cursor: pointer;
}
</style>
