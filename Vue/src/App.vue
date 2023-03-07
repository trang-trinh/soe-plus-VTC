<script setup>
//Import
// import devtools from "../node_modules/devtools-detect/index.js";
import { RouterView } from "vue-router";
import HeadBar from "./components/base/HeadBar.vue";
import SideBar from "./components/base/SideBar.vue";
import LoginView from "./views/LoginView.vue";
import { watch, inject, onMounted, ref } from "vue";
import { useRouter, useRoute } from "vue-router";
import { decr, encr } from "./util/function";
import { useToast } from "vue-toastification";
import { useCookies } from "vue3-cookies";
const { cookies } = useCookies();

//Khai báo biến
const devtools = inject("devtools");
const socket = inject("socket");
const toast = useToast();
const cryoptojs = inject("cryptojs");
const store = inject("store");
const router = useRoute();
const emitter = inject("emitter");
const axios = inject("axios"); // inject axios
const swal = inject("$swal");
const isDeveloper = ref(isDev);

store.commit("setisframe", (window === window.parent) ? false : true);

//init Data
// if (localStorage.getItem("u") != null) {
//   let u = decr(localStorage.getItem("u"), SecretKey, cryoptojs);
//   if (u != null) {

//     store.commit("setuser", JSON.parse(u));
//   }
// }

// if (localStorage.getItem("tk") != null) {
//   store.commit(
//     "settoken",
//     decr(localStorage.getItem("tk"), SecretKey, cryoptojs)
//   );
// }
if (cookies.get("u") != null) {
  let u = decr(cookies.get("u"), SecretKey, cryoptojs);
  if (u != null) {
    store.commit("setuser", JSON.parse(u));
  }
}

if (cookies.get("tk") != null) {
  store.commit("settoken", decr(cookies.get("tk"), SecretKey, cryoptojs));
}
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const passModuleToSidebar = () => {
  var link = router.fullPath;
  if (!link) return false;
  var module_name = link.split("/")[1];
  var response = axios
    .post(
      baseURL + "/api/Notify/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_modules_getmodulefromlink",
            par: [{ par: "link", va: module_name }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      {
        headers: { Authorization: `Bearer ${store.getters.token}` },
      }
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data[0].length > 0) {
        cookies.set("max_length_file", data[0][0].max_length_file);
        emitter.emit("emitData", { type: "moduleFromUrl", data: data[0][0] });
      }
    });
};
//Vue App
onMounted(() => {
  watch(router, () => {
    if (router.fullPath && router.fullPath.length > 1 && store.getters.islogin)
      passModuleToSidebar();
  });
 
  //socket
  store.getters.user.islogin = store.getters.islogin;
  socket.auth = store.getters.user;
  socket.connect();
  socket.on("getusers", (data) => {
    data.forEach((user) => {
      user.connected = true;
      user.self = user.socket_id === socket.id;
      user.last_name = (user.full_name || "").split(" ").slice(-1).join(" ");
    });
    data = data.sort((a, b) => {
      if (a.self) return -1;
      if (b.self) return 1;
      if (a.last_name < b.last_name) return -1;
      return a.last_name > b.last_name ? 1 : 0;
    });
    store.commit("setuserConnected", data);
    //console.log(store.getters.userConnected);
  });
  socket.on("userconnected", (user) => {
    user.connected = true;
    user.self = user.socket_id === socket.id;
    user.last_name = (user.full_name || "").split(" ").slice(-1).join(" ");
    var userconnected = store.getters.userConnected;
    userconnected.push(user);
    userconnected = userconnected.sort((a, b) => {
      if (a.self) return -1;
      if (b.self) return 1;
      if (a.last_name < b.last_name) return -1;
      return a.last_name > b.last_name ? 1 : 0;
    });
    store.commit("setuserConnected", userconnected);
    //console.log("a user connect", store.getters.userConnected);
  });
  socket.on("userdisconnected", (id) => {
    var userconnected = store.getters.userConnected;
    userconnected.forEach((user) => {
      if (user.socket_id === id) {
        user.connected = false;
      }
    });
    store.commit("setuserConnected", userconnected);
    //console.log("a user disconnect", store.getters.userConnected);
  });
  socket.on("connect", () => {
    var userconnected = store.getters.userConnected;
    userconnected.forEach((user) => {
      if (user.self) {
        user.connected = true;
      }
    });
    store.commit("setuserConnected", userconnected);
    //console.log("a user connect", store.getters.userConnected);
  });
  socket.on("disconnect", () => {
    var userconnected = store.getters.userConnected;
    userconnected.forEach((user) => {
      if (user.self) {
        user.connected = false;
      }
    });
    store.commit("setuserConnected", userconnected);
    //console.log("a user disconnect", store.getters.userConnected);
  });
  //socket
 
});
// console.log("Is DevTools open:", devtools.isOpen);

// console.log("DevTools orientation:", devtools.orientation);
const isDevtool = ref(devtools.isOpen || false);
window.addEventListener("devtoolschange", (event) => {
  isDevtool.value = event.detail.isOpen;
  if (isDevtool.value && !isDeveloper) {
    window.location.reload();
  }
  // console.log("Is DevTools open:", event.detail.isOpen);
  // console.log("DevTools orientation:", event.detail.orientation);
});
 
 
</script>

<template>
 
  <div
    v-if="isDevtool && !isDeveloper"
    class="w-full h-full flex justify-content-center"
    style="margin: 0 5%"
  >
    <div class="construct-title format-center h-full">
      <div class="text-left">
        <div class="font-bold" style="font-size: 8em; color: #141b51">
          Stop!
        </div>
        <div class="font-bold" style="font-size: 4em; color: #141b51">
          developer tools
        </div>
        <div class="mt-5" style="font-size: 1.5em; color: #b4b4b9">
          Tính nắng chỉ dành cho nhà phát triển vui lòng quay lại!
        </div>
      </div>
    </div>
    <div class="construct-image format-center h-full">
      <img
        src="./assets/background/construct.svg"
        alt="construct.svg"
        style="width: 80%; height: 80%; object-fit: contain"
      />
    </div>
  </div>
  <div v-else class="flex flex-column flex-grow-1 h-full">
    <HeadBar v-if="store.getters.islogin && !store.getters.isframe" />
    <div class="body-layout flex flex-grow-1 w-full h-full">
      <SideBar class="shadow-1 h-full" v-if="store.getters.islogin && !store.getters.isframe" />
      <RouterView v-if="store.getters.islogin" />
      <LoginView v-if="!store.getters.islogin"></LoginView>
    </div>
  </div>
</template>
