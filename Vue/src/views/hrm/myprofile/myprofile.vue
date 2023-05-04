<script setup>
import { onMounted, ref } from "vue";

//init
const initData = (ref) => {};
onMounted(() => {
  initData(true);
});
</script>
<template>
  <div class="surface-100 p-2" style="overflow: hidden">
    <Toolbar class="outline-none surface-0 border-none pb-0">
      <template #start>
        <div>
          <h3 class="module-title m-0">
            <i class="pi pi-calendar"></i>
          </h3>
        </div>
      </template>
      <template #end>
        <Button
          @click="refresh()"
          class="p-button-outlined p-button-secondary"
          icon="pi pi-refresh"
          v-tooltip.top="'Tải lại'"
        />
      </template>
    </Toolbar>
    <div class="tabview">
      <div class="tableview-nav-content">
        <ul class="tableview-nav">
          <li
            v-for="(tab, key) in tabs"
            :key="key"
            @click="activeTab(tab)"
            class="tableview-header"
            :class="{ highlight: options.tab === tab.id }"
          >
            <a>
              <i :class="tab.icon"></i>
              <span>{{ tab.title }} ({{ tab.total }})</span>
            </a>
          </li>
        </ul>
      </div>
    </div>
    <div class="col-12 md:col-12 p-0">
      <div class="row">
        <div class="col-8 md:col-8">test</div>
        <div class="col-4 md:col-4">test</div>
      </div>
    </div>
  </div>
</template>
<style scoped>
@import url(../contract/component/stylehrm.css);
</style>
