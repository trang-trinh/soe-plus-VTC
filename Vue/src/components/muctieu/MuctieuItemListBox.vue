<script setup>
import { defineProps, ref, onMounted } from "vue";
import CongviecListBox from "../congviec/CongviecListBox.vue";
import TrangthaiDuanView from "../duan/TrangthaiDuan.vue";
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
const collapsed = ref(false);
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
const tooglecollapsed = () => {
  collapsed.value = !collapsed.value;
};
onMounted(() => {
  return {};
});
</script>
<template>
  <Fieldset class="w-full mt-2 no-pad" :collapsed="collapsed">
    <template #legend>
      <div v-bind:class="'w-full pb-2 border-round ttda-' + Muctieu.Trangthai">
        <div class="flex align-items-center justify-content-center">
          <div class="flex flex-grow-1 flex-column">
            <Button
              class="shadow-none hover-none transition-none p-button-text p-button-plain block mr-2"
              @click="tooglecollapsed"
            >
              <h3 class="m-0 textoneline text-left" v-ripple>
                {{ Muctieu.Muctieu_Ten }}
              </h3>
            </Button>
            <div class="flex ml-2">
              <Avatar
                v-tooltip.right="'Người tạo: ' + Muctieu.full_nameTao"
                v-bind:label="
                  Muctieu.AvartarTao ? '' : Muctieu.full_nameTao.substring(0, 1)
                "
                v-bind:image="basedomainURL + Muctieu.AvartarTao"
                style="background-color: #2196f3; color: #ffffff; vertical-align: middle"
                class="mr-2 mt-1"
                size="small"
                shape="circle"
              />
              <div>
                <div class="spduan-name">{{ Muctieu.full_nameTao }}</div>
                <div class="spduan-ngay">{{ Muctieu.NgaytaoS }}</div>
              </div>
            </div>
          </div>
          <AvatarGroup class="ml-2 mr-2" v-if="Muctieu.Users">
            <Avatar
              v-for="item in Muctieu.Users.slice(0, 3)"
              :key="item.PlanUser_ID"
              v-bind:label="item.Avartar ? '' : item.full_name.substring(0, 1)"
              v-bind:image="basedomainURL + item.Avartar"
              style="background-color: #2196f3; color: #ffffff; vertical-align: middle"
              class="mr-2"
              size="small"
              shape="circle"
            />
            <Avatar
              v-if="Muctieu.Users && Muctieu.Users.length > 3"
              v-bind:label="'+' + (Muctieu.Users.length - 3).toString()"
              shape="circle"
              size="small"
              style="background-color: #2196f3; color: #ffffff"
            />
          </AvatarGroup>
          <div
            v-if="Muctieu.NgayKTS"
            class="text-right spandate mt-0"
            v-tooltip.top="'Ngày kết thúc (' + Muctieu.NgayKTS + ')'"
          >
            {{ Muctieu.NgayKTD }}
          </div>
          <TrangthaiDuanView
            class="spanWhite"
            :Trangthai="Muctieu.Trangthai"
          ></TrangthaiDuanView>
          <Button
            icon="pi pi-ellipsis-h"
            class="p-button-text p-button-plain ml-2 mr-3"
            @click="toggleMores($event, Muctieu)"
            aria-haspopup="true"
            aria-controls="overlay_More"
          />
          <Menu
            id="overlay_More"
            ref="menuButMores"
            :model="itemButMores"
            :popup="true"
          />
        </div>
      </div>
    </template>
    <VirtualScroller
      :scrollHeight="
        (Muctieu.Congviecs
          ? Muctieu.Congviecs.length * 100 < 300
            ? Muctieu.Congviecs.length * 100
            : 300
          : 0) + 'px'
      "
      id="Congviec_ID"
      :items="Muctieu.Congviecs"
      :itemSize="5"
    >
      <template v-slot:item="{ item }">
        <CongviecListBox
          :Congviec="item"
          :editCongviec="editCongviec"
          :delCongviec="delCongviec"
          :changeTrangthaiCV="changeTrangthaiCV"
        ></CongviecListBox>
      </template>
    </VirtualScroller>
    <Button
      @click="addTask(Muctieu)"
      icon="pi pi-plus-circle"
      label="Thêm công việc cho mục tiêu"
      class="shadow-none p-button-sm"
    />
  </Fieldset>
</template>
<style scoped>
.spanWhite {
  background-color: #fff !important;
  color: #495057 !important;
  font-weight: 500;
  margin-left: 10px;
}
.pfullgrid {
  max-height: calc(100vh - 450px);
}
.box-muctieu {
  background-color: #eee;
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
