<script setup>
//Import
import { inject, onMounted, ref } from "vue";
import moment from "moment";
import ModuleMenu from "../../components/base/ModuleMenu.vue";
import Notify from "../../components/base/Notify.vue";
import { encr } from "../../util/function.js";
const cryoptojs = inject("cryptojs");
const emitter = inject("emitter");

const swal = inject("$swal");
//Khai báo biến
const socket = inject("socket");
const router = inject("router");
const store = inject("store");
const storetheme = inject("storetheme");
const showSidebarNoti = ref(false);
const count_noti = ref(0);

const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
// nhận dữ liệu
emitter.on("emitData", (obj) => {
  switch (obj.type) {
    case "closeNoti":
      setTimeout(() => {
        loadNoti();
      }, 100);
      showSidebarNoti.value = obj.data.showSidebarNoti;
      break;
    default:
      break;
  }
});
//load noti

const setTheme = (name) => {
  storetheme.value = name;
  localStorage.setItem("storetheme", name);
  visibleSidebarRight.value = false;
};
const basedomainURL = fileURL;
let fnames = store.getters.user.full_name
  ? store.getters.user.full_name.split(" ")
  : ["A"];
let FName = fnames[fnames.length - 1].substring(0, 1);
const menuTaikhoan = ref();
const visibleSidebarRight = ref(false);
const axios = inject("axios"); // inject axios
const itemheaders = ref([
  { label: "Cài đặt", icon: "pi pi-fw pi-cog", to: "/options" },
  {
    label: "Đăng xuất",
    icon: "pi pi-fw pi-power-off",
    command: (event) => {
      logout();
    },
  },
]);
//
const logout = () => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn đăng xuất khỏi tài khoản này không!",
      icon: "warning",
      showCancelButton: true,
      confirmButtonColor: "#3085d6",
      cancelButtonColor: "#d33",
      confirmButtonText: "Có",
      cancelButtonText: "Không",
    })
    .then(async (result) => {
      if (result.isConfirmed) {
        axios.get(baseURL + "/api/Login/Logout", {
          headers: { Authorization: `Bearer ${store.getters.token}` },
        });
        store.commit("gologout");
        if (router) router.push({ path: "/login" });
      }
    });
};

const toggle = (event, type) => {
  //storetheme.value="bootstrap4-dark-blue";
  switch (type) {
    case 1:
      menuTaikhoan.value.toggle(event);
      break;
    case 2:
      moduleMenu.value.toggle(event);
      break;
    default:
      break;
  }
};
//module
const itemmodules = ref([
  { label: "Văn bản", icon: "pi pi-fw pi-file", to: "/options" },
  {
    label: "Đăng xuất",
    icon: "pi pi-fw pi-power-off",
    command: (event) => {
      logout();
    },
  },
]);
const moduleMenu = ref();
//lang
const menuLang = ref();
const itemlangs = ref([]);
const toggleLang = (event) => {
  if (itemlangs.value.length == 0) {
    itemlangs.value = store.getters.langs.map((x) => ({
      label: x.Name,
      icon: basedomainURL + x.icon,
      item: x,
    }));
  }
  menuLang.value.toggle(event);
};
const goLang = (item) => {
  store.commit("setlang", item.item);
  //emitter.emit("lang");
  menuLang.value.hide();
};
// noti
const loadNoti = () => {
  axios
    .post(
      baseURL + "/api/Notify/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_noti_count",
            par: [{ par: "user_id", va: store.getters.user.user_id }],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        count_noti.value = data[0].c;
      }
    });
};
const clickNoti = () => {
  showSidebarNoti.value = true;
  axios({
    method: "put",
    url: baseURL + "/api/Notify/Update_Noti",
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  });
};
//Vue App
onMounted(() => {
  loadNoti();
  socket.on("sendNotify", (data) => {
    loadNoti();
    let audioNotify = new Audio(
      basedomainURL + "/Portals/FileChatSystem/pristine-sound.mp3",
    );
    if (audioNotify != null) {
      audioNotify.play();
    }
    let bell = document.getElementById("bell");
    if (bell != null) {
      bell.classList.add("bell");
      setTimeout(() => {
        bell.classList.remove("bell");
      }, 8000);
    }
  });

  return {};
});
</script>
<template>
  <div class="flex flex-row shadow-3 header-bar">
    <div class="flex flex-row flex-grow-1 headerbar align-items-center">
      <div
        class="ml-3 mr-2"
        v-if="store.getters.user.logo"
      >
        <img
          :src="basedomainURL + store.getters.user.logo"
          height="40"
        />
      </div>
      <h2 class="title-org">
        {{
          store.getters.user.product_name ||
          store.getters.user.organization_name ||
          "SMART OFFICE"
        }}
      </h2>
    </div>
    <div class="flex flex-none align-items-center justify-content-center">
      <Button
        class="shadow-none p-button-text p-ripple"
        v-ripple
        v-if="store.getters.langs.length > 0"
        @click="toggleLang"
        aria-haspopup="true"
        aria-controls="overlay_menulang"
      >
        <img
          v-tooltip="store.getters.lang.Name"
          :src="basedomainURL + store.getters.lang.icon"
          height="20"
        />
      </Button>
      <Menu
        id="overlay_menulang"
        ref="menuLang"
        :model="itemlangs"
        :popup="true"
      >
        <template #item="{ item }">
          <Button
            @click="goLang(item)"
            class="p-button-text p-button-plain w-full"
          >
            <img
              :src="item.icon"
              height="16"
              class="mr-1"
            />
            {{ item.label }}
          </Button>
        </template>
      </Menu>
      <Button
        type="button"
        class="module-button p-button p-component p-button-icon-only p-button-rounded p-button-plain p-button-text"
        @click="toggle($event, 2)"
        aria-haspopup="true"
        aria-controls="module_menu"
      >
        <i class="pi pi-table p-button-icon"></i>
      </Button>
      <OverlayPanel
        id="module_menu"
        ref="moduleMenu"
        style="width: 450px"
        :breakpoints="{ '960px': '75vw' }"
      >
        <ModuleMenu></ModuleMenu>
      </OverlayPanel>
      <!-- <Menu id="module_menu" ref="menuTaikhoan" :model="itemheaders" :popup="true" /> -->
      <Button
        type="button"
        class="noti-button p-button p-component p-button-icon-only p-button-rounded p-button-secondary p-button-text"
        aria-haspopup="true"
        aria-controls="overlay_menu"
        style="padding: 0.5rem"
        @click="clickNoti()"
      >
        <span
          id="bell"
          v-ripple
          :class="count_noti && count_noti > 0 ? '' : 'hide-bell'"
          class="pi pi-bell p-button-icon"
          style="font-size: 1.5rem"
          v-badge="count_noti"
        ></span>
      </Button>
      <Button
        type="button"
        class="shadow-none p-button-text p-1 p-ripple"
        @click="toggle($event, 1)"
        aria-haspopup="true"
        aria-controls="overlay_menu"
      >
        <Avatar
          v-tooltip.left="store.getters.user.full_name"
          v-ripple
          v-bind:image="
            basedomainURL + store.getters.user.avatar + '?public=true'
          "
          v-bind:label="store.getters.user.avatar ? '' : FName"
          style="background-color: #2196f3; color: #ffffff"
          shape="circle"
        />
      </Button>
      <Menu
        id="overlay_menu"
        ref="menuTaikhoan"
        :model="itemheaders"
        :popup="true"
      />
    </div>
    <div v-if="showSidebarNoti">
      <Notify :showSidebarNoti="showSidebarNoti"></Notify>
    </div>
  </div>
  <Sidebar
    v-model:visible="visibleSidebarRight"
    :baseZIndex="1000"
    style="width: 360px"
    position="right"
    :showCloseIcon="false"
  >
    <h2 class="align-items-center justify-content-center">
      Cấu hình Theme
      <Button
        icon="pi pi-palette"
        @click="setTheme('saga-blue')"
        class="p-button-rounded"
      />
    </h2>

    <div
      v-for="(value, name) in skin"
      :key="name"
    >
      <h5>{{ name }}</h5>
      <div class="grid col-12">
        <div
          class="col-3 align-items-center justify-content-center text-center"
          v-for="item in value"
          :key="item.name"
          @click="setTheme(item.name)"
        >
          <Avatar
            class="divskin"
            v-bind:image="item.icon"
            size="xlarge"
          />
          <h5>{{ item.title }}</h5>
        </div>
      </div>
    </div>
  </Sidebar>
</template>
<style scoped>
.icon-modules {
  width: 14px;
  height: 14px;
}
.item-right-filter {
  display: flex;
  justify-content: end;
  align-items: center;
}
.headerbar {
  background-color: #fff;
  height: 50px;
}
.divskin:hover {
  cursor: pointer;
  background-color: #eee;
}
</style>
<style scoped>
/* Bell */
.bell {
  display: block;
  color: #9e9e9e;
  -webkit-animation: ring 4s 0.7s ease-in-out infinite;
  -webkit-transform-origin: 50% 4px;
  -moz-animation: ring 4s 0.7s ease-in-out infinite;
  -moz-transform-origin: 50% 4px;
  animation: ring 4s 0.7s ease-in-out infinite;
  transform-origin: 50% 4px;
}
@-webkit-keyframes ring {
  0% {
    -webkit-transform: rotateZ(0);
  }
  1% {
    -webkit-transform: rotateZ(30deg);
  }
  3% {
    -webkit-transform: rotateZ(-28deg);
  }
  5% {
    -webkit-transform: rotateZ(34deg);
  }
  7% {
    -webkit-transform: rotateZ(-32deg);
  }
  9% {
    -webkit-transform: rotateZ(30deg);
  }
  11% {
    -webkit-transform: rotateZ(-28deg);
  }
  13% {
    -webkit-transform: rotateZ(26deg);
  }
  15% {
    -webkit-transform: rotateZ(-24deg);
  }
  17% {
    -webkit-transform: rotateZ(22deg);
  }
  19% {
    -webkit-transform: rotateZ(-20deg);
  }
  21% {
    -webkit-transform: rotateZ(18deg);
  }
  23% {
    -webkit-transform: rotateZ(-16deg);
  }
  25% {
    -webkit-transform: rotateZ(14deg);
  }
  27% {
    -webkit-transform: rotateZ(-12deg);
  }
  29% {
    -webkit-transform: rotateZ(10deg);
  }
  31% {
    -webkit-transform: rotateZ(-8deg);
  }
  33% {
    -webkit-transform: rotateZ(6deg);
  }
  35% {
    -webkit-transform: rotateZ(-4deg);
  }
  37% {
    -webkit-transform: rotateZ(2deg);
  }
  39% {
    -webkit-transform: rotateZ(-1deg);
  }
  41% {
    -webkit-transform: rotateZ(1deg);
  }
  43% {
    -webkit-transform: rotateZ(0);
  }
  100% {
    -webkit-transform: rotateZ(0);
  }
}
@-moz-keyframes ring {
  0% {
    -moz-transform: rotate(0);
  }
  1% {
    -moz-transform: rotate(30deg);
  }
  3% {
    -moz-transform: rotate(-28deg);
  }
  5% {
    -moz-transform: rotate(34deg);
  }
  7% {
    -moz-transform: rotate(-32deg);
  }
  9% {
    -moz-transform: rotate(30deg);
  }
  11% {
    -moz-transform: rotate(-28deg);
  }
  13% {
    -moz-transform: rotate(26deg);
  }
  15% {
    -moz-transform: rotate(-24deg);
  }
  17% {
    -moz-transform: rotate(22deg);
  }
  19% {
    -moz-transform: rotate(-20deg);
  }
  21% {
    -moz-transform: rotate(18deg);
  }
  23% {
    -moz-transform: rotate(-16deg);
  }
  25% {
    -moz-transform: rotate(14deg);
  }
  27% {
    -moz-transform: rotate(-12deg);
  }
  29% {
    -moz-transform: rotate(10deg);
  }
  31% {
    -moz-transform: rotate(-8deg);
  }
  33% {
    -moz-transform: rotate(6deg);
  }
  35% {
    -moz-transform: rotate(-4deg);
  }
  37% {
    -moz-transform: rotate(2deg);
  }
  39% {
    -moz-transform: rotate(-1deg);
  }
  41% {
    -moz-transform: rotate(1deg);
  }
  43% {
    -moz-transform: rotate(0);
  }
  100% {
    -moz-transform: rotate(0);
  }
}
@keyframes ring {
  0% {
    transform: rotate(0);
  }
  1% {
    transform: rotate(30deg);
  }
  3% {
    transform: rotate(-28deg);
  }
  5% {
    transform: rotate(34deg);
  }
  7% {
    transform: rotate(-32deg);
  }
  9% {
    transform: rotate(30deg);
  }
  11% {
    transform: rotate(-28deg);
  }
  13% {
    transform: rotate(26deg);
  }
  15% {
    transform: rotate(-24deg);
  }
  17% {
    transform: rotate(22deg);
  }
  19% {
    transform: rotate(-20deg);
  }
  21% {
    transform: rotate(18deg);
  }
  23% {
    transform: rotate(-16deg);
  }
  25% {
    transform: rotate(14deg);
  }
  27% {
    transform: rotate(-12deg);
  }
  29% {
    transform: rotate(10deg);
  }
  31% {
    transform: rotate(-8deg);
  }
  33% {
    transform: rotate(6deg);
  }
  35% {
    transform: rotate(-4deg);
  }
  37% {
    transform: rotate(2deg);
  }
  39% {
    transform: rotate(-1deg);
  }
  41% {
    transform: rotate(1deg);
  }
  43% {
    transform: rotate(0);
  }
  100% {
    transform: rotate(0);
  }
}
</style>
<style lang="scss" scoped>
::v-deep(.p-button-icon) {
  .p-badge {
    background-color: #ff0000 !important;
  }
}
::v-deep(.p-sidebar .p-sidebar-lg) {
  .p-sidebar-content {
    overflow-y: hidden !important;
  }
}

::v-deep(.pi-bell.hide-bell) {
  .p-badge {
    display: none;
  }
}
</style>
