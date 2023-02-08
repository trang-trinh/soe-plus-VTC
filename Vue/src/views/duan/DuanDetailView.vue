<script setup>
import { ref, inject, onMounted } from "vue";
import moment from "moment";
import { useRoute } from "vue-router";
import MuctieuView from "../../components/muctieu/MuctieuView.vue";
import CongviecView from "../../components/congviec/CongviecView.vue";
import TrangthaiDuanView from "../../components/duan/TrangthaiDuan.vue";
const route = useRoute(); //init Model
const swal = inject("$swal");
const store = inject("store");
const axios = inject("axios"); // inject axios
const basedomainURL = fileURL;
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const Duan_ID = route.params.duanid;
const duan = ref({
  Duan_ID: Duan_ID,
  users: [],
  muctieus: [],
  congviecs: [],
  count: {},
});
const activeIndex = ref(1);
//Function
const emitter = inject("emitter");
emitter.on("duan", (obj) => {
  switch (obj.type) {
    case "refershCount":
      for (const key in obj.data) {
        duan.value.count[key] = obj.data[key];
      }
      break;
  }
});
const initDuan = () => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "Plan_Duan_Info",
        par: [
          { par: "user_id", va: store.getters.user.user_id },
          { par: "Duan_ID", va: duan.value.Duan_ID },
        ],
      },
      config
    )
    .then((response) => {
      swal.close();
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        let obj = data[0][0];
        if (obj.Ngaytao)
          obj.NgaytaoS = moment(new Date(obj.Ngaytao)).format("DD/MM/YYYY HH:mm:ss");
        if (obj.NgayBD)
          obj.NgayBDS = moment(new Date(obj.NgayBD)).format("DD/MM/YYYY HH:mm:ss");
        if (obj.NgayKT)
          obj.NgayKTS = moment(new Date(obj.NgayKT)).format("DD/MM/YYYY HH:mm:ss");
        if (obj.NgayKT)
          obj.NgayKTD =
            moment(new Date(obj.NgayBD)).format("DD/MM") +
            "-" +
            moment(new Date(obj.NgayKT)).format("DD/MM/YYYY");
        if (obj.NgayTH)
          obj.NgayTHS = moment(new Date(obj.NgayTH)).format("DD/MM/YYYY HH:mm:ss");
        if (obj.NgayHT)
          obj.NgayHTS = moment(new Date(obj.NgayHT)).format("DD/MM/YYYY HH:mm:ss");
        duan.value.model = obj;
        if (data.length > 1) {
          duan.value.users = data[1];
        }
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
  //init
  initDuan();
  return {};
});
</script>
<template>
  <div
    class="main-layout flex flex-column flex-grow-1 p-2"
    style="background-color: #fff"
    v-if="store.getters.islogin && duan.model"
  >
    <div class="header-bar flex">
      <Avatar
        v-bind:label="duan.model.Logo ? '' : duan.model.Duan_Ten.substring(0, 1)"
        v-bind:image="basedomainURL + duan.model.Logo"
        style="color: #000"
        class="mr-2"
        size="small"
        shape="circle"
      />
      <div class="flex flex-grow-1 mr-2">
        <h3 class="m-0 mt-1">{{ duan.model.Duan_Ten }}</h3>
        <div
          v-if="duan.model.NgayKTS"
          class="text-right spandate mt-1 ml-2"
          v-tooltip.top="'Ngày kết thúc (' + duan.model.NgayKTS + ')'"
        >
          {{ duan.model.NgayKTD }}
        </div>
      </div>
      <TrangthaiDuanView :Trangthai="duan.model.Trangthai"></TrangthaiDuanView>
      <AvatarGroup class="ml-2" v-if="duan.users">
        <Avatar
          v-for="item in duan.users.slice(0, 3)"
          :key="item.PlanUser_ID"
          v-bind:label="item.Avartar ? '' : item.full_name.substring(0, 1)"
          v-bind:image="basedomainURL + item.Avartar"
          style="background-color: #2196f3; color: #ffffff; vertical-align: middle"
          class="mr-2"
          size="small"
          shape="circle"
        />
        <Avatar
          v-if="duan.users && duan.users.length > 3"
          v-bind:label="'+' + (duan.users.length - 3).toString()"
          shape="circle"
          size="small"
          style="background-color: #2196f3; color: #ffffff"
        />
      </AvatarGroup>
    </div>
    <div class="flex-grow-1 info-duan">
      <TabView v-model:activeIndex="activeIndex" class="flex flex-column h-full">
        <TabPanel>
          <template #header> Thông tin chung </template>
        </TabPanel>
        <TabPanel>
          <template #header>
            {{ "Mục tiêu" + (duan.count.Muctieu ? ` (${duan.count.Muctieu})` : "") }}
          </template>
          <MuctieuView :Duan_ID="Duan_ID"></MuctieuView>
        </TabPanel>
        <TabPanel>
          <template #header>
            {{ "Công việc" + (duan.count.Congviec ? ` (${duan.count.Congviec})` : "") }}
          </template>
          <CongviecView :Duan_ID="Duan_ID"></CongviecView>
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
  </div>
</template>
<style scoped>
.spduan-name {
  font-weight: 500;
  color: #6c757d;
  font-size: 0.87rem;
}
.spduan-ngay {
  color: #6c757d;
  font-size: 0.87rem;
}
.spandate {
  background-color: #4285f4;
  padding: 5px 10px;
  width: max-content;
  border-radius: 8px;
  margin: 10px 0;
  height: fit-content;
  margin-bottom: 0;
  margin-right: 5px;
  font-weight: 500;
  color: #fff;
  font-size: 0.75rem;
}
</style>
