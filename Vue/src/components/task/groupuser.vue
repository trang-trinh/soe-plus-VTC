<script setup>
//Khai báo InJect và Import (import)
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import sidebarGrU from "../task/sidebaruser.vue";
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const emitter = inject("emitter");
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};

//Khai báo biến (variable)
const basedomainURL = baseURL;
const toast = useToast();
const options = ref({
  IsNext: true,
  sort: "task_id",
  searchText: "",
  PageNo: 0,
  PageSize: 10,
  loading: true,
  totalRecords: null,
  finishedRecord: null,
  waitedRecord: null,
  tempClose: null,
  unFinishRecord: null,
  statusTask: null,
  outOfDate: null,
  SearchTextUser: "",
  Start_date: null,
  End_date: null,
});
const listUsers = ref([]);
const listUserShow = ref([]);
const UsersCount = ref();
const listDropdownUser = ref([]);
const listDropdownUserCheck = ref([]);
const showSidebarUser = ref(false);

// Khai báo hàm (function)

emitter.on("emitData", (obj) => {
  switch (obj.type) {
    case "hideSidebarGU":
      showSidebarUser.value = false;
      break;

    default:
      break;
  }
});
///DANH SÁCH NGƯỜI DÙNG

const onTaskUserFilter = (value) => {
  emitter.emit("emitData", { type: "onTaskUserFilter", data: value });
  if (value.active) {
    listUserShow.value.forEach((element) => {
      if (element.data == value.data) element.active = false;
    });
    listUsers.value.forEach((element) => {
      if (element.data == value.data) element.active = false;
    });

    emitter.emit("emitData", {
      type: "userFilter",
      data: store.getters.user.user_id,
    });
    return;
  } else {
    options.value.loading = true;

    listUserShow.value.forEach((element) => {
      if (element.data == value.data) element.active = true;
      else element.active = false;
    });
    listUsers.value.forEach((element) => {
      if (element.data == value.data) element.active = true;
      else element.active = false;
    });
    emitter.emit("emitData", { type: "userFilter", data: value.data.user_id });
  }
};

const filterTaskUser = (value) => {
  showSidebarUser.value = false;
  emitter.emit("emitData", { type: "onTaskUserFilter", data: value });
};
const sidebarUser = () => {
  showSidebarUser.value = true;
  console.log("so1");
};
const reloadUser = () => {
  listUser();
};
const layout = ref("list");
const liUsers = ref();
const listUser = () => {
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "sys_users_list_dd",
        par: [
          { par: "search", va: options.value.SearchTextUser },
          { par: "user_id", va: store.getters.user.user_id },
          { par: "role_id", va: null },
          { par: "organization_id", va: store.getters.user.organization_id },
          { par: "department_id", va: null },
          { par: "position_id", va: null },
          { par: "pageno", va: 1 },
          { par: "pagesize", va: 10000 },
          { par: "isadmin", va: null },
          { par: "status", va: null },
          { par: "start_date", va: null },
          { par: "end_date", va: null },
        ],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      data.forEach((element, i) => {
        if (i < 5) listUserShow.value.push({ data: element, active: false });
        listDropdownUser.value.push({
          name: element.full_name,
          code: element.user_id,
          avatar: element.avatar,
        });

        listUsers.value.push({ data: element, active: false });
      });
      liUsers.value = data;

      listDropdownUserCheck.value = listDropdownUser.value;
      emitter.emit("emitData", { type: "liUsers", data: liUsers.value });
      emitter.emit("emitData", {
        type: "listDropdownUser",
        data: listDropdownUser.value,
      });
      emitter.emit("emitData", { type: "listUsers", data: listUsers.value });
      emitter.emit("emitData", {
        type: "listDropdownUserCheck",
        data: listDropdownUserCheck.value,
      });
    })
    .catch((error) => {
      console.log(error);

      options.value.loading = false;

      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "sys_users_count",
        par: [
          { par: "search", va: options.value.searchText },
          { par: "user_id", va: store.getters.user.user_id },
          { par: "role_id", va: null },
          { par: "organization_id", va: null },
          { par: "department_id", va: null },
          { par: "position_id", va: null },
          { par: "filter_department", va: -1 },
          { par: "pageno", va: 0 },
          { par: "pagesize", va: 10 },
          { par: "is_admin", va: null },
          { par: "status", va: null },
          { par: "start_date", va: null },
          { par: "end_date", va: null },
        ],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];

      UsersCount.value = data[0].totalrecords - 5;
      emitter.emit("emitData", { type: "UsersCount", data: UsersCount.value });
    })
    .catch((error) => {
      console.log(error);

      options.value.loading = false;

      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
onMounted(() => {
  listUser();
  return {};
});
</script>
<template>
  <div>
    <AvatarGroup>
      <Avatar
        v-bind:label="
          item.data.avatar ? '' : item.data.full_name.substring(0, 1)
        "
        @click="onTaskUserFilter(item)"
        v-for="item in listUserShow"
        :key="item.data.user_key"
        v-bind:image="
          item.data.avatar
            ? basedomainURL + item.data.avatar
            : basedomainURL + '/Portals/Image/noimg.jpg'
        "
        :style="
          item.active == false
            ? 'border: 3px solid white'
            : 'border: 3px solid red'
        "
        @error="basedomainURL + '/Portals/Image/noimg.jpg'"
        size="large"
        shape="circle"
        class="cursor-pointer bg-blue-200"
      />

      <Avatar
        @click="sidebarUser"
        v-if="UsersCount > 0"
        :label="'+' + UsersCount"
        shape="circle"
        size="large"
        style="background-color: #9c27b0; color: #ffffff"
        class="cursor-pointer"
      />
    </AvatarGroup>
  </div>
  <div v-if="showSidebarUser == true && listUsers.length > 0">
    <sidebarGrU
      :title="'Danh sách nhân viên'"
      :listUsers="listUsers"
      :showSidebarUser="showSidebarUser"
    />
  </div>
</template>
