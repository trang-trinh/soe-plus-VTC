<script setup>
//Import
// import devtools from "../node_modules/devtools-detect/index.js";
import { RouterView } from "vue-router";
import HeadBar from "./components/base/HeadBar.vue";
import SideBar from "./components/base/SideBar.vue";
import LoginView from "./views/LoginView.vue";
import ForgetView from "./views/login/ForgetPass.vue";
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
const route = inject("router");
const emitter = inject("emitter");
const axios = inject("axios"); // inject axios
const swal = inject("$swal");
const isDeveloper = ref(isDev);

store.commit("setisframe", window === window.parent ? false : true);

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
// axios.interceptors.response.use(
//   (response) => {
//     if (response.data.dataKey) {
//       let dataKey = decr(response.data.dataKey, SecretKey, cryoptojs);
//       let arr_list = dataKey.split("26$#");
//       if (arr_list.length > 1) {
//         let check_endcode = arr_list[0].toString() == "1111" ? true : false,
//           key = arr_list[1];
//         if (check_endcode)
//           response.data.data = decr(response.data.data, key, cryoptojs);
//       }
//     }
//     return response;
//   },
//   (error) => {
//     return error;
//   },
// );
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
            proc: "sys_modules_getmodulefromlink_1",
            par: [
              { par: "link", va: module_name },
              { par: "user_id", va: store.getters.user.user_id },
            ],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      {
        headers: { Authorization: `Bearer ${store.getters.token}` },
      },
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      let root_path = router.fullPath;
      let arr_params = Object.values(router.params);
      //check router from notify (contain params id, type,....)
      if (arr_params.length > 0) {
        arr_params.forEach((item) => {
          let idx = root_path.lastIndexOf("/" + item);
          if (idx != -1) {
            root_path = root_path.slice(0, idx);
          }
        });
      }
      let path_system = [
        "/options",
        "/hrm/contact",
        "/hrm/hrm_headbar_calendar",
        "/tasks/taskmaintype",
        "/tasks/aa",
      ];
      if (
        data[1].filter((x) => x.is_link == root_path).length == 0 
        && !path_system.includes(root_path)
        && (data[1].filter((x) => x.is_link == '/hrm/template/smart_report').length == 0 || !root_path.includes('/hrm/template/smart_report'))
        && (data[1].filter((x) => x.is_link == '/hrm/profile').length == 0 || !root_path.includes('/hrm/profile/report'))
        && (data[1].filter((x) => x.is_link == '/hrm/payroll/hrm_payroll').length == 0 || !root_path.includes('/hrm/payroll/hrm_payroll/details'))

     
        )
        route.push({ path: "/" });
      else if (data[0].length > 0) {
        cookies.set("max_length_file", data[0][0].max_length_file);
        emitter.emit("emitData", { type: "moduleFromUrl", data: data[0][0] });
      }
    });
};
const getPortalFile = () => {
  axios
    .post(
      baseURL + "/api/Cache/getPortalFile",
      {},
      {
        headers: { Authorization: `Bearer ${store.getters.token}` },
      },
    )
    .then((response) => {
      if (response.data.err == "0") {
        cookies.set("porFi", response.data.portF);
      }
    });
};
//Vue App
onMounted(() => {
  if ("Notification" in window) {
    // yêu cầu quyền thông báo
    Notification.requestPermission().then(function (result) {});
  }
  getPortalFile();
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

const is_forgetpass = ref(false);
const data_props = ref({});
const currentLink = ref(window.location.href);
if (currentLink.value.includes("/forgetpss/")) {
  is_forgetpass.value = true;
  let str = window.location.href
    .substring(window.location.href.lastIndexOf("/forgetpss/") + 11)
    .replaceAll("tun", "+");
  data_props.value = JSON.parse(
    decr(str, SecretKey, cryoptojs).replaceAll('"', "").replaceAll("'", '"'),
  );
}
</script>

<template>
  <div
    v-if="isDevtool && !isDeveloper"
    class="w-full h-full flex justify-content-center"
    style="margin: 0 5%"
  >
    <div class="construct-title format-center h-full">
      <div class="text-left">
        <div
          class="font-bold"
          style="font-size: 8em; color: #141b51"
        >
          Stop!
        </div>
        <div
          class="font-bold"
          style="font-size: 4em; color: #141b51"
        >
          developer tools
        </div>
        <div
          class="mt-5"
          style="font-size: 1.5em; color: #b4b4b9"
        >
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
  <div
    v-else
    class="flex flex-column flex-grow-1 h-full"
  >
    <HeadBar
      v-if="store.getters.islogin && !store.getters.isframe && !is_forgetpass"
    />
    <div class="body-layout flex flex-grow-1 w-full h-full">
      <SideBar
        class="shadow-1 h-full"
        v-if="store.getters.islogin && !store.getters.isframe && !is_forgetpass"
      />
      <RouterView v-if="store.getters.islogin && !is_forgetpass" />
      <LoginView v-if="!store.getters.islogin && !is_forgetpass"></LoginView>
      <ForgetView
        v-if="is_forgetpass"
        :uid="data_props.uid"
        :code="data_props.code"
      ></ForgetView>
    </div>
  </div>
</template>
