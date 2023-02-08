<script setup>
import { inject, onMounted, ref } from "vue";
import { encr, decr } from "../util/function";
import { socketMethod } from "../util/methodSocket";
import { useCookies } from "vue3-cookies";
const { cookies } = useCookies();

const socket = inject("socket");
const axios = inject("axios"); // inject axios
const user = ref({ user_id: "", is_psword: "" });
const errors = ref({ user_id: "", is_psword: "" });
const store = inject("store");
const swal = inject("$swal");
const router = inject("router");
const cryoptojs = inject("cryptojs");
const basedomainURL = fileURL;
const checkForm = () => {
  if (!user.value.user_id) {
    errors.value.user_id = "Tên đăng nhập không được để trống!";
  } else {
    errors.value.user_id = "";
  }
  if (!user.value.is_psword) {
    errors.value.is_psword = "Mật khẩu không được để trống!";
  } else if (user.value.is_psword.length < 8) {
    errors.value.is_psword = "Mật khẩu không được ít hơn 8 ký tự!";
  } else {
    errors.value.is_psword = "";
  }
};
const login = () => {
  checkForm();
  let form = document.getElementsByName("frlogin")[0];
  let check = form.checkValidity();
  if (!check) {
    return false;
  }
  // Light theme
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios
    .post(baseURL + "/api/Login/Login", {
      str: encr(JSON.stringify(user.value), SecretKey, cryoptojs).toString(),
    })
    .then((response) => {
      if (response.data.err != "1") {
        //localStorage.setItem("tk", response.data.data);
        //localStorage.setItem("u", response.data.u);
        cookies.set("tk", response.data.data);
        cookies.set("u", response.data.u);
        store.commit(
          "setuser",
          JSON.parse(decr(response.data.u, SecretKey, cryoptojs))
        );
        store.commit(
          "settoken",
          decr(response.data.data, SecretKey, cryoptojs)
        );
        store.commit("setislogin", true);
        // if (localStorage.getItem("chatGroupID") != null) {
        //   localStorage.removeItem("chatGroupID");
        // }
        // if (localStorage.getItem("viewTabChatID") != null) {
        //   localStorage.removeItem("viewTabChatID");
        // }
        if (cookies.get("chatGroupID") != null) {
          cookies.remove("chatGroupID");
        }
        if (cookies.get("viewTabChatID") != null) {
          cookies.remove("viewTabChatID");
        }
        router.push({ path: "/" });
        swal.close();

        //socket
        store.getters.user.islogin = store.getters.islogin;
        socket.auth = store.getters.user;
        socket.connect();
        if ("serviceWorker" in navigator) {
          navigator.serviceWorker
            .register("/src/util/serviceWorker.js")
            .then((registration) => {
              if ("pushManager" in registration) {
                registration.pushManager
                  .subscribe({
                    userVisibleOnly: true,
                    applicationServerKey:
                      "BClGBpl2qTJtZVVSmksuLtbWUTN-JdA8SSafi1jKxqFc0aV4ZhZ50ecYzI5DK1nob6jTZHur25nX9A1Q1HuyXb4",
                  })
                  .then((subscrition) => {
                    socketMethod
                      .post("subscription", {
                        id: store.getters.user.user_id,
                        subscrition: subscrition,
                      })
                      .then((res) => {});
                  });
              } else {
                //console.log("The service worker doesn't support push");
              }
            });
        } else {
          //console.log("The browser doesn't support service workers");
        }
        //socket
      } else {
        swal.fire({
          title: "Error!",
          text: response.data.ms,
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    })
    .catch((error) => {
      console.log("lỗi", error);
      swal.fire({
        title: "Error!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};
onMounted(() => {
  return {
    checkForm,
    login,
    user,
    errors,
  };
});
</script>
<template>
  <div class="login-container">
    <div class="login-bg">
      <img
        :src="
          basedomainURL + store.getters.user.background_image ||
          '../assets/background/bg.png'
        "
      />
    </div>
    <div class="login-org">
      <img
        class="org-logo"
        :src="
          basedomainURL + store.getters.user.logo || '/Portals/Image/noimg.jpg'
        "
      />
      <h3 class="org-name">
        {{ store.getters.user.organization_name }}
      </h3>
    </div>
    <div class="login-product-container">
      <div class="login-product">
        <h3
          class="
            product-name
            animate__animated animate__fadeInDown animate__delay-1s
          "
        >
          {{ store.getters.user.product_name }}
        </h3>
      </div>
    </div>
    <div class="box-login">
      <div class="login-form">
        <form name="frlogin" @submit.prevent="login">
          <div class="login-ava">
            <Avatar
              :image="
                basedomainURL +
                (store.getters.user.avatar != null
                  ? store.getters.user.avatar
                  : '/Portals/Image/nouser1.png')
              "
              shape="circle"
              size="xlarge"
            />
          </div>
          <div class="mb-3 mt-6">
            <h1>Đăng nhập</h1>
          </div>
          <div class="field">
            <label for="user_id">Tên đăng nhập</label>
            <InputText
              id="user_id"
              type="text"
              v-model="user.user_id"
              @input="checkForm"
              v-bind:class="errors.user_id != '' ? 'invalid' : ''"
              autocomplete="off"
              required
            />
            <InlineMessage severity="error" v-if="errors.user_id != ''">{{
              errors.user_id
            }}</InlineMessage>
          </div>
          <div class="field">
            <label for="is_psword">Mật khẩu</label>
            <Password
              id="is_psword"
              v-model="user.is_psword"
              @input="checkForm"
              v-bind:class="
                errors.is_psword != 'w-full' ? 'w-full invalid' : ''
              "
              autocomplete="off"
              required
              toggleMask
              :feedback="false"
              v-on:keyup.enter="login"
            >
            </Password>
            <InlineMessage severity="error" v-if="errors.is_psword != ''">{{
              errors.is_psword
            }}</InlineMessage>
          </div>
          <Button label="Đăng nhập" @click="login()" />
        </form>
      </div>
    </div>
  </div>
</template>
<style lang="scss" scoped>
::v-deep(.p-password) {
  .p-inputtext {
    padding: 0.9rem;
  }
}
</style>
<style scoped>
.invalid {
  color: red;
  margin: 5px 0;
  font-size: 13px;
}

input.invalid {
  border: 1px solid red;
}

.login-container {
  display: flex;
  width: 100vw;
  height: 100vh;
}

.login-bg {
  width: 100vw;
  height: 100vh;
  background-color: #70c5c3;
}

.login-bg > img {
  object-fit: cover;
  width: 100%;
  height: 100%;
}

.box-login {
  background-color: rgba(255, 255, 255, 0.9);
  flex: 1;
  display: block;
  padding: 50px;
  position: absolute;
  right: 0;
  height: 100%;
  min-width: 480px;
}

.login-form {
  align-items: center;
  vertical-align: middle;
  max-width: 480px;
  margin: auto;
  margin-top: calc(50vh - 242px);
}

.login-form > button {
  width: 100%;
  margin-top: 20px;
}

.login-form h1 {
  font-size: 20pt;
  margin-bottom: 20px;
}
.field * {
  display: block;
}
.field label {
  margin: 1rem 0;
  font-weight: bold;
}
.p-button {
  padding: 0.9rem;
}
.p-avatar {
  border: 1px solid #c9c9c9;
}
.p-inputtext {
  display: block;
  margin-bottom: 0.5rem;
  width: 100%;
  padding: 0.9rem;
}
.p-inputtext:last-child {
  margin-bottom: 0;
}
.login-form button {
  width: 100%;
  margin-top: 2rem;
}
.login-org {
  min-width: calc(100vw - 480px);
  background-color: #ddddddcf;
  position: absolute;
  top: 0;
  left: 0;
  display: flex;
  justify-content: center;
  align-items: center;
}

.login-org .org-logo {
  max-width: 18rem;
  height: 4.7rem;
  margin-right: 1rem;
  object-fit: contain;
}

.login-org .org-name {
  text-transform: uppercase;
  font-size: 1.8rem;
  color: #cf1127;
}

.login-product-container {
  min-width: calc(100vw - 480px);
  position: absolute;
  height: 100vh;
}

.login-product {
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  width: max-content;
  background-color: #ddddddcf;
  position: absolute;
}

.login-product .product-name {
  text-transform: uppercase;
  font-size: 2.4rem;
  padding: 1.2rem;
  color: #005a9e;
}
.login-ava {
  display: flex;
  justify-content: center;
}
</style>
