<script setup>
import { ref, defineProps, inject, onMounted, nextTick, onUnmounted } from "vue";
import { useToast } from "vue-toastification";
// import Markdown from 'vue3-markdown-it';
import moment from "moment";
const store = inject("store");
const toast = useToast();
const swal = inject("$swal");
const axios = inject("axios"); // inject axios
const props = defineProps({
  proptintuc: Object,
  showTitle: Boolean,
});
const tintuc = ref();
const basedomainURL = fileURL;
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const getNews = () => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      { proc: "CMS_News_View", par: [{ par: "News_ID", va: tintuc.value.News_ID }] },
      config
    )
    .then(async (response) => {
      swal.close();
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        let obj = data[0][0];
        obj.CreateDateString = moment(new Date(obj.CreateDate)).format(
          "DD/MM/YYYY HH:mm:ss"
        );
        if (obj.Keywords) obj.Keywords = obj.Keywords.split(",");
        obj.Details = obj.Details.replaceAll("<img ", '<img loading="lazy" ');
        tintuc.value = obj;
        await nextTick();
        renderImage();
      }
    })
    .catch((error) => {
      if (error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
      }
    });
};
const showGalleria = ref(false);
const compImages = ref([]);
const activeIndex = ref(0);
function renderImage() {
  var imgs = [];
  document
    .getElementById("new-details")
    .querySelectorAll("img")
    .forEach((img, i) => {
      let oimg = { src: img.getAttribute("src"), alt: img.getAttribute("alt") };
      let a = img.parentElement;
      if (a) {
        if (oimg.alt == "") {
          let figcaption = a.nextSibling;
          if (figcaption && figcaption.tagName == "FIGCAPTION")
            oimg.alt = figcaption.textContent;
        }
        a.removeAttribute("href");
        img.addEventListener("click", function (e) {
          if (!showGalleria.value) {
            showGalleria.value = true;
            activeIndex.value = i;
          }
        });
      }
      imgs.push(oimg);
    });
  compImages.value = imgs;
}
const emitter = inject("emitter");
onMounted(() => {
  if (props.proptintuc) {
    tintuc.value = props.proptintuc;
    getNews();
  }
  emitter.on("read-new", (obj) => {
    tintuc.value = obj;
    getNews();
  });
  return {};
});
onUnmounted(() => {
  emitter.off("read-new");
});
</script>
<template>
  <div class="view-new" v-if="tintuc">
    <h2 v-if="showTitle" class="new-title">{{ tintuc.News_Name }}</h2>
    <p>
      <span class="new-tao">{{ tintuc.full_nameTao }}</span>
      <span class="new-ngay">{{ tintuc.CreateDateString }}</span>
    </p>
    <div class="new-conttent">{{ tintuc.Contents }}</div>
    <div id="new-details" class="new-details" v-html="tintuc.Details"></div>
    <!-- <Markdown :source="tintuc.Details" /> -->
  </div>
  <Galleria
    v-model:visible="showGalleria"
    :value="compImages"
    :numVisible="9"
    :activeIndex="activeIndex"
    containerStyle="max-width: 60vw;"
    :circular="true"
    :fullScreen="true"
    :showItemNavigators="true"
    :showThumbnails="true"
  >
    <template #item="{ item }">
      <img :src="item.src" :alt="item.alt" style="width: 100%; display: block" />
    </template>
    <template #thumbnail="{ item }">
      <div class="grid grid-nogutter justify-content-center">
        <img
          :src="item.src"
          :alt="item.alt"
          style="display: block; height: 60px; width: 100px; object-fit: cover"
        />
      </div>
    </template>
    <template #caption="{ item }">
      <h4>{{ item.alt }}</h4>
    </template>
  </Galleria>
</template>
<style scoped>
.new-details {
  min-height: 450px;
}
.view-new {
  padding: 0 20px;
}
.new-title {
  font-size: 15pt;
  font-weight: bold;
}
.new-tao {
  font-weight: 500;
  margin-right: 5px;
}
.new-ngay {
  color: #888;
}
</style>
