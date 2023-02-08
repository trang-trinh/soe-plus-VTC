<script setup>
import { defineProps, ref, inject, onMounted } from "vue";
import TrangthaiCongviec from "./TrangthaiCongviec.vue";
const props = defineProps({
  Congviec: Object,
});
const iCongviec = ref(props.Congviec);
const swal = inject("$swal");
const store = inject("store");
const axios = inject("axios"); // inject axios
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const basedomainURL = fileURL;
const thuchiens = ref([]);
const quanlys = ref([]);
const theodois = ref([]);
const initCongviec = () => {
  if (iCongviec.value.Users) {
    thuchiens.value = iCongviec.value.Users.filter((x) => x.TypeUser == 1);
    quanlys.value = iCongviec.value.Users.filter((x) => x.TypeUser == 2);
    theodois.value = iCongviec.value.Users.filter((x) => x.TypeUser == 0);
  }
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "Plan_Congviec_Get",
        par: [
          { par: "user_id", va: store.getters.user.user_id },
          { par: "Congviec_ID", va: iCongviec.value.Congviec_ID },
        ],
      },
      config
    )
    .then((response) => {
      swal.close();
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        let obj = data[0][0];
        if (obj.Tukhoa) obj.Tukhoa = obj.Tukhoa.split(",");
        if (data[1].length > 0) {
          thuchiens.value = data[1].filter((x) => x.TypeUser == 1);
          quanlys.value = data[1].filter((x) => x.TypeUser == 2);
          theodois.value = data[1].filter((x) => x.TypeUser == 0);
        }
        iCongviec.value = obj;
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
onMounted(() => {
  initCongviec();
  return {};
});
</script>
<template>
  <div class="flex h-full">
    <div class="flex-grow-1 info-duan">
      <TabView v-model:activeIndex="activeIndex" class="flex flex-column h-full">
        <TabPanel>
          <template #header> Check list </template>
        </TabPanel>
        <TabPanel>
          <template #header> Lịch họp </template>
        </TabPanel>
        <TabPanel>
          <template #header> Bình luận </template>
        </TabPanel>
        <TabPanel>
          <template #header> Tài liệu </template>
        </TabPanel>
        <TabPanel>
          <template #header> Time Line </template>
        </TabPanel>
        <TabPanel>
          <template #header> Thành viên </template>
        </TabPanel>
        <TabPanel>
          <template #header> Thông báo </template>
        </TabPanel>
      </TabView>
    </div>
    <div class="info-congviec h-full shadow-1">
      <div class="flex info-row">
        <h4 class="mt-1 mb-1" v-if="thuchiens.length > 0">
          <i class="pi pi-user-edit mr-2"></i>Thực hiện
          <Badge v-bind:value="thuchiens.length" class="mr-2"></Badge>
        </h4>
        <div style="flex-grow: 1"></div>
        <h4 class="mt-1 mb-1" v-if="quanlys.length > 0">
          <i class="pi pi-user mr-2"></i>Quản lý
          <Badge v-bind:value="quanlys.length" severity="success" class="mr-2"></Badge>
        </h4>
      </div>
      <div class="flex info-row">
        <AvatarGroup v-if="thuchiens.length > 0">
          <Avatar
            v-for="item in thuchiens"
            v-tooltip.top="item.full_name"
            :key="item.PlanUser_ID"
            v-bind:label="item.Avartar ? '' : item.full_name.substring(0, 1)"
            v-bind:image="basedomainURL + item.Avartar"
            style="background-color: #2196f3; color: #ffffff; vertical-align: middle"
            class="mr-2"
            size="large"
            shape="circle"
          />
        </AvatarGroup>
        <div style="flex-grow: 1"></div>
        <AvatarGroup>
          <Avatar
            v-for="item in quanlys"
            v-tooltip.top="item.full_name"
            :key="item.PlanUser_ID"
            v-bind:label="item.Avartar ? '' : item.full_name.substring(0, 1)"
            v-bind:image="basedomainURL + item.Avartar"
            style="background-color: #2196f3; color: #ffffff; vertical-align: middle"
            class="mr-2"
            size="large"
            shape="circle"
          />
        </AvatarGroup>
      </div>
      <div v-if="theodois.length > 0" class="mt-2">
        <h4 class="mt-1 mb-1">
          <i class="pi pi-users mr-2"></i>Theo dõi
          <Badge v-bind:value="theodois.length" severity="warning" class="mr-2"></Badge>
        </h4>
        <AvatarGroup class="mt-2">
          <Avatar
            v-for="item in theodois"
            v-tooltip.top="item.full_name"
            :key="item.PlanUser_ID"
            v-bind:label="item.Avartar ? '' : item.full_name.substring(0, 1)"
            v-bind:image="basedomainURL + item.Avartar"
            style="background-color: #2196f3; color: #ffffff; vertical-align: middle"
            class="mr-2"
            size="large"
            shape="circle"
          />
        </AvatarGroup>
      </div>
      <h4>Chi tiết</h4>
      <div class="flex info-row">
        <label><i class="pi pi-user-edit mr-2"></i> Người tạo</label>
        <div class="info-value ml-2 text-right">
          {{ Congviec.full_nameTao }}
        </div>
      </div>
      <div class="flex info-row">
        <label><i class="pi pi-calendar mr-2"></i>Ngày tạo</label>
        <div class="info-value ml-2 text-right">
          {{ Congviec.NgaytaoS }}
        </div>
      </div>
      <div class="flex info-row">
        <label><i class="pi pi-calendar-minus mr-2"></i>Ngày bắt đầu</label>
        <label class="text-right"
          ><i class="pi pi-calendar-plus mr-2"></i>Ngày kết thúc</label
        >
      </div>
      <div class="flex info-row">
        <div class="info-value ml-2 text-left">
          {{ Congviec.NgayBDS }}
        </div>
        <div style="flex-grow: 1"></div>
        <div class="info-value ml-2 text-right">
          {{ Congviec.NgayKTS }}
        </div>
      </div>

      <div class="flex info-row">
        <label><i v-bind:class="'mr-2 ' + Congviec.iconUutien"></i>Ưu tiên</label>
        <label class="text-right"
          ><i v-bind:class="'mr-2 ' + Congviec.iconMucdo"></i>Mức độ</label
        >
      </div>
      <div class="flex info-row">
        <div class="ml-2 text-left">
          <Chip
            v-bind:label="Congviec.TenUutien"
            :style="{ background: Congviec.MaunenUutien, color: Congviec.MauchuUutien }"
          />
        </div>
        <div style="flex-grow: 1"></div>
        <div class="ml-2 text-right">
          <Chip
            v-bind:label="Congviec.TenMucdo"
            :style="{ background: Congviec.MaunenMucdo, color: Congviec.MauchuMucdo }"
          />
        </div>
      </div>
      <div class="flex info-row" v-if="iCongviec.Tukhoa">
        <label><i class="pi pi-tags mr-2"></i> Từ khoá</label>
        <div class="ml-2 text-right">
          <Chip
            class="ml-1 mb-1"
            v-bind:key="item"
            v-for="item in iCongviec.Tukhoa"
            v-bind:label="item"
          />
        </div>
      </div>
    </div>
  </div>
</template>
<style scoped>
.info-congviec {
  width: 400px;
  border-left: 1px solid #eee;
  padding: 1rem;
}
.info-row {
  margin-bottom: 0.875rem;
}
.info-row > label {
  color: #888;
  flex-grow: 1;
}
.info-value {
  font-weight: 500;
  background-color: #eee;
  padding: 5px 10px;
  border-radius: 30px;
  font-size: 0.875rem;
}
</style>
