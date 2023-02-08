<script setup>
import { defineProps, ref, onMounted } from "vue";
import CongviecBox from "../congviec/CongviecBox.vue";
const props = defineProps({
  Muctieu: Object,
  addTask: Function,
  goMuctieu: Function,
  editMuctieu: Function,
  delMuctieu: Function,
  editCongviec: Function,
  delCongviec: Function,
  changeTrangthaiCV: Function,
});
const basedomainURL = fileURL;
const menuButMores = ref();
const itemButMores = [
  {
    label: "Sửa mục tiêu",
    icon: "pi pi-pencil",
    command: (event) => {
      props.editMuctieu(props.Muctieu);
    },
  },
  {
    label: "Xoá mục tiêu",
    icon: "pi pi-trash",
    command: (event) => {
      props.delMuctieu(props.Muctieu);
    },
  },
];
const toggleMores = (event) => {
  menuButMores.value.toggle(event);
};
onMounted(() => {
  return {};
});
</script>
<template>
  <div class="box-muctieu">
    <div v-bind:class="'flex mt-2 p-3 ttda-' + Muctieu.Trangthai">
      <div>
        <Avatar
          v-tooltip.right="'Người tạo: ' + Muctieu.full_nameTao"
          v-bind:label="Muctieu.AvartarTao ? '' : Muctieu.full_nameTao.substring(0, 1)"
          v-bind:image="basedomainURL + Muctieu.AvartarTao"
          style="background-color: #2196f3; color: #ffffff; vertical-align: middle"
          class="mt-1"
          size="small"
          shape="circle"
        />
      </div>
      <div class="flex-grow-1 ml-1 mr-1 mb-2">
        <Button
          v-tooltip.top="Muctieu.Muctieu_Ten"
          class="shadow-none p-button-text p-button-plain block"
          @click="goMuctieu(Muctieu)"
        >
          <h3 class="m-0 textoneline">{{ Muctieu.Muctieu_Ten }}</h3>
        </Button>
      </div>
      <Button
        icon="pi pi-ellipsis-h"
        class="shadow-none p-button-rounded p-button-text"
        @click="toggleMores($event)"
        aria-haspopup="true"
        aria-controls="overlay_More"
      />
      <Menu id="overlay_More" ref="menuButMores" :model="itemButMores" :popup="true" />
    </div>
    <Button
      @click="addTask(Muctieu)"
      label="Thêm công việc"
      icon="pi pi-plus-circle"
      class="shadow-none mt-2 p-button-sm w-full block text-center"
    />
    <div class="pfullgrid">
      <VirtualScroller id="Congviec_ID" :items="Muctieu.Congviecs" :itemSize="5">
        <template v-slot:item="{ item }">
          <CongviecBox
            :Congviec="item"
            :editCongviec="editCongviec"
            :delCongviec="delCongviec"
            :changeTrangthaiCV="changeTrangthaiCV"
          ></CongviecBox>
        </template>
      </VirtualScroller>
    </div>
  </div>
</template>
<style scoped>
.pfullgrid {
  height: calc(100vh - 360px);
  overflow-y: auto;
}
.box-muctieu {
  background-color: #eee;
  width: 360px;
  margin-right: 5px;
  margin-left: 5px;
}
.ttda-0 {
  background-color: #dee2e6;
  font-size: 0.875rem;
}
.p-button.p-button-text {
  color: #fff;
}
.ttda-0 .p-button.p-button-text {
  color: inherit;
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
