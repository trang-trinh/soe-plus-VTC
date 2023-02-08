<script setup>
import { ref, defineProps, onMounted } from "vue";
import CompAnh from "./CompAnh.vue";
import CompText from "./CompText.vue";
const props = defineProps({
  id: String,
});
const tabIndex = ref(3);
const goTab = (idx) => {
  if (tabIndex.value == idx) {
    tabIndex.value = null;
  } else tabIndex.value = idx;
};
onMounted(() => {
  return {};
});
</script>
<template>
  <main class="tabs h-full" :style="{maxWidth:tabIndex==null?'100px':'410px'}">
    <div class="tab-headers">
      <div :class="'tab' + (tabIndex == 0)" @click="goTab(0)">Mẫu</div>
      <div :class="'tab' + (tabIndex == 1)" @click="goTab(1)">Slide</div>
      <div :class="'tab' + (tabIndex == 2)" @click="goTab(2)">Văn bản</div>
      <div :class="'tab' + (tabIndex == 3)" @click="goTab(3)">Ảnh</div>
      <div :class="'tab' + (tabIndex == 4)" @click="goTab(4)">Thành phần</div>
      <div :class="'tab' + (tabIndex == 5)" @click="goTab(5)">Video</div>
      <div :class="'tab' + (tabIndex == 6)" @click="goTab(6)">Âm thanh</div>
      <div :class="'tab' + (tabIndex == 7)" @click="goTab(7)">Nền</div>
      <div :class="'tab' + (tabIndex == 8)" @click="goTab(8)">Câu hỏi</div>
      <div :class="'tab' + (tabIndex == 9)" @click="goTab(9)">Bình luận</div>
    </div>
    <div class="tab-contents" v-show="tabIndex != null">
      <div v-show="tabIndex == 2">
        <CompText></CompText>
      </div>
      <div v-show="tabIndex == 3">
        <CompAnh></CompAnh>
      </div>
    </div>
  </main>
</template>
<style scoped>
.tabs {
  display: flex;
  flex-direction: column;
  flex: 1;
}

.tabs * {
  box-sizing: border-box;
}

@media (min-width: 768px) {
  .tabs {
    flex-direction: row;
  }

  .tabs > * {
    min-height: 12rem;
  }
}

/* Inner and Outer Layout */

.tab-headers {
  min-width: 100px;
  background-color: #4285f4;
}

.tab-headers div {
  padding: 25px 10px;
  text-align: center;
  cursor: pointer;
  transition: 0.3s;
  overflow: hidden;
  color: white;
  font-size: 0.9rem;
}

.tab-contents {
  min-width: 300px;
  border-right: 1px solid #eee;
  background-color: #fff;
}

.tab-contents div {
  padding: 1rem;
  height: 100%;

  color: white;
  font-size: 1rem;
}

/* Add some decoration */

.tab-headers div:hover,
div.tabtrue {
  background-color: #fff;
  color: #000;
}
</style>
