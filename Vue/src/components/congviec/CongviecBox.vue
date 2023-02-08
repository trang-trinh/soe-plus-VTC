<script setup>
import { defineProps, ref, onMounted } from "vue";
import TrangthaiCongviec from "./TrangthaiCongviec.vue";
import InfoCongviec from "./InfoCongviec.vue";
const props = defineProps({
  Congviec: Object,
  editCongviec: Function,
  delCongviec: Function,
  changeTrangthaiCV: Function,
});
const basedomainURL = fileURL;
const menuButMores = ref();
const itemButMores = [
  {
    label: "Sửa công việc",
    icon: "pi pi-pencil",
    command: (event) => {
      props.editCongviec(props.Congviec);
    },
  },
  {
    label: "Xoá công việc",
    icon: "pi pi-trash",
    command: (event) => {
      props.delCongviec(props.Congviec);
    },
  },
];
const toggleMores = (event) => {
  menuButMores.value.toggle(event);
};
const visibleInfo = ref(false);
const widthInfo = ref(70);
const toogleScreenCongviec = () => {
  widthInfo.value = widthInfo.value == 70 ? 100 : 70;
};
const goCongviec = () => {
  visibleInfo.value = true;
};
const closeCongviec = () => {
  visibleInfo.value = false;
};
onMounted(() => {
  return {};
});
</script>
<template>
  <Card v-bind:class="'nopadd mt-2 ttcv-' + Congviec.Trangthai">
    <template #content>
      <div class="flex" style="margin: -1rem 0">
        <div>
          <Avatar
            v-tooltip.right="'Người tạo: ' + Congviec.full_nameTao"
            v-bind:label="Congviec.AvartarTao ? '' : Congviec.full_nameTao.substring(0, 1)"
            v-bind:image="basedomainURL + Congviec.AvartarTao"
            style="background-color: #2196f3; color: #ffffff; vertical-align: middle"
            class="mt-1"
            size="small"
            shape="circle"
          />
        </div>
        <div class="flex-grow-1 ml-1 mr-1 mb-2">
          <Button
            class="shadow-none p-button-text p-button-plain block"
            @click="goCongviec()"
          >
            <h3 class="m-0">{{ Congviec.Congviec_Ten }}</h3>
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
      <div class="mt-3 text-center">
        <Button
          class="shadow-none p-button-text p-button-plain"
          @click="changeTrangthaiCV(Congviec)"
        >
          <TrangthaiCongviec :Trangthai="Congviec.Trangthai"></TrangthaiCongviec>
        </Button>
      </div>
      <div class="flex flex-row align-items-center justify-content-center">
        <div
          v-if="Congviec.NgayKTS"
          class="text-right spandate"
          v-tooltip.top="'Ngày kết thúc (' + Congviec.NgayKTS + ')'"
        >
          {{ Congviec.NgayKTD }}
        </div>
        <div class="flex flex-grow-1 justify-content-end">
          <AvatarGroup class="mt-2" v-if="Congviec.Users">
            <Avatar
              v-tooltip.top="item.full_name"
              v-for="item in Congviec.Users.slice(0, 3)"
              :key="item.PlanUser_ID"
              v-bind:label="item.Avartar ? '' : item.full_name.substring(0, 1)"
              v-bind:image="basedomainURL + item.Avartar"
              style="background-color: #2196f3; color: #ffffff; vertical-align: middle"
              class="mr-2"
              size="small"
              shape="circle"
            />
            <Avatar
              v-if="Congviec.Users && Congviec.Users.length > 3"
              v-bind:label="'+' + (Congviec.Users.length - 3).toString()"
              shape="circle"
              size="small"
              style="background-color: #2196f3; color: #ffffff"
            />
          </AvatarGroup>
        </div>
      </div>
    </template>
  </Card>
  <Sidebar
    v-model:visible="visibleInfo"
    :baseZIndex="1000"
    class="slidebar-custom"
    :style="{ width: widthInfo + 'vw' }"
    position="right"
  >
    <div class="info-header flex">
      <h3 class="m-0 flex-grow-1">
        <i class="pi pi-info-circle mr-2"></i>{{ Congviec.Congviec_Ten }}
        <TrangthaiCongviec :Trangthai="Congviec.Trangthai"></TrangthaiCongviec>
      </h3>
      <Button
        @click="toogleScreenCongviec"
        icon="pi pi-window-maximize"
        class="shadow-none p-button-text p-button-text p-button-plain"
      />
      <Button
        @click="closeCongviec"
        icon="pi pi-times"
        class="shadow-none p-button-text p-button-text p-button-plain"
      />
    </div>
    <InfoCongviec :Congviec="Congviec"></InfoCongviec>
  </Sidebar>
</template>
<style scoped>
.info-header {
  padding: 0.75rem;
  background-color: #eee;
}
.p-card {
  border-left: 5px solid;
}
.ttcv-0 {
  border-color: #dee2e6;
}
.ttcv-1 {
  border-color: #3b82f6;
}
.ttcv-2 {
  border-color: #16a34a;
}
.ttcv-3 {
  border-color: #eec137;
}
.ttcv-4 {
  border-color: #35c4dc;
}
.ttcv-5 {
  border-color: #d97706;
}
.ttcv-6 {
  border-color: #dc2626;
}
.spandate {
  background-color: #eee;
  padding: 5px 10px;
  width: max-content;
  border-radius: 5px;
  margin: 10px 0;
  height: fit-content;
  margin-bottom: 0;
  margin-right: 5px;
  font-weight: 500;
  color: #6c757d;
  font-size: 0.85rem;
}
</style>
