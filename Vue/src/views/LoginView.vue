<script setup>
import { inject, onMounted, ref } from "vue";
import { required, email } from "@vuelidate/validators";
import { encr, decr } from "../util/function";
import { socketMethod } from "../util/methodSocket";
import { useCookies } from "vue3-cookies";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
const { cookies } = useCookies();

const socket = inject("socket");
const axios = inject("axios"); // inject axios
const user = ref({ user_id: "", is_psword: "" });
const errors = ref({ user_id: "", is_psword: "" });
const toast = useToast();
const store = inject("store");
const swal = inject("$swal");
const router = inject("router");
const dialogForgetPass = ref(false);
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
        store.commit("setlistOrgTree", []);
        
        // if (localStorage.getItem("ck_cgi") != null) {
        //   localStorage.removeItem("ck_cgi");
        // }
        // if (localStorage.getItem("ck_tabchat") != null) {
        //   localStorage.removeItem("ck_tabchat");
        // }
        if (cookies.get("ck_cgi") != null) {
          cookies.remove("ck_cgi");
        }
        if (cookies.get("ck_tabchat") != null) {
          cookies.remove("ck_tabchat");
        }
        router.push({ name: "profile" });
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
                      .then((res) => { });
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
// forget pass
const rules = {
  email: {
    email,
  },
};
const mailInfo = ref();
const topmail = ref();
const bottommail = ref();
const isSendEmail = ref(false);
const submitted = ref(false);
const v$ = useVuelidate(rules, mailInfo);
const showDialogForgetPss = () => {
  submitted.value = false;
  mailInfo.value = {
    to: "",
    email: "",
    display_name: "SOE+ - Smart Office Enterprise +",
    subject: "Quên mật khẩu",
    body: "",
    top: "",
    bottom: "",
    isBodyHtml: true,
  };
  getConfigMail();
  isSendEmail.value = false
  dialogForgetPass.value = true;
}
const configMail = ref();
const getConfigMail = () => {
  axios
    .get(baseURL + "/api/Login/GetConfigMail", {
      headers: { Authorization: `Bearer ${store.getters.token}` },
    })
    .then((response) => {
      if (response.data.err != "1") {
        configMail.value = JSON.parse(response.data.data);
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

const sendMail = (isFormValid) => {
  submitted.value = true;
  if (!isFormValid) {
    return;
  }
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  let time_email = decr(configMail ? configMail.value.timemail : '5', SecretKey, cryoptojs);
  topmail.value = "<div style='font-size:15px'><div style='margin-top:16px;margin-bottom:20px'>Xin chào,</div>"
  topmail.value += " <div>Chúng tôi đã nhận được yêu cầu đặt lại mật khẩu của bạn.</div>";
  topmail.value += "   <div> Vui lòng <a href='"
  //topmail.value +=  "https://project.soe.vn//forgetpass";
  bottommail.value = "'>Click vào đây để đặt lại mật khẩu mới</a></div>";
  bottommail.value += "<div>(Bạn có " + (time_email || 5) + " phút để nhập lại mật khẩu mới kể từ khi nhận thư này)</div></div>";
  mailInfo.value.top += topmail.value.toString();
  mailInfo.value.bottom = bottommail.value.toString();
  let formData = new FormData();
  formData.append(
    "pwMail",
    configMail.value.kpmail != null
      ? decr(configMail.value.kpmail, SecretKey, cryoptojs).toString()
      : null,
  );
  formData.append("mailinfo", JSON.stringify(mailInfo.value));
  axios({
    method: "post",
    url: baseURL + `/api/Login/${"sendEMail"}`,
    data: formData,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err == "-1") {
        swal.fire({
          title: "Thông báo",
          text: response.data.ms,
          icon: "warning",
          confirmButtonText: "OK",
        });
      } else {
        isSendEmail.value = true;
        swal.close();
      };
    })
    .catch((error) => {
      swal.close();
      swal.fire({
        title: "Error!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};
onMounted(() => {  
  if (cookies.get("ck_cgi") != null) {
    cookies.remove("ck_cgi");
  }
  if (cookies.get("ck_tabchat") != null) {
    cookies.remove("ck_tabchat");
  }
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
      <section class="bg-img overflow-hidden h-full w-full" :style="'background-image: url('+basedomainURL+'/Portals/Image/logovtc.jpg)'">  
        <div class="login-org">
          <img v-if="store.getters.user.logo" class="org-logo py-1 ml-2" :src="
            basedomainURL + store.getters.user.logo || '/Portals/Image/noimg.jpg'
          " />
          <h3 class="org-name flex-grow-1 mb-0 font-bold">
            {{ store.getters.user.organization_name }}
          </h3>
          <div class="mr-2" style="font-size:12px; color:#383737">
            <span class="mr-4">Phiên bản chạy thử nghiệm</span>
            <span><i class="pi pi-whatsapp mr-2"></i>Hỗ trợ: 090 142 6788</span>
          </div>
        </div> 
        <div class="container px-4 py-5 px-md-5 text-center text-lg-start" style="margin-top: 14vh;">
          <div class="row gx-lg-5 align-items-center mb-5">
            <div class="title-left col-lg-6 mb-5 mb-lg-0" style="z-index: 1">
              <h1 class="my-5 display-5 fw-bold ls-tight animate__animated animate__fadeInLeftBig animate__delay-0.5s" style="color: hsl(218, 81%, 95%); font-size: 3.8rem;margin-bottom:20px !important;">
                Hệ thống <br />
                <span style="color: #F6C445">Quản lý Nhân sự</span>
              </h1>
              <p class="mb-4 opacity-70 animate__animated animate__fadeInUpBig animate__delay-1s" style="color: #fff;font-size: 1.5rem;">
                Hệ thống cơ sở dữ liệu nhân sự tập trung, đồng nhất, giúp nhà quản lý dễ dàng xử lý các quy trình và nghiệp vụ.
                <br />Giải pháp thực hiện hiện chuyển đổi số toàn diện về quản lý nguồn nhân lực trong doanh nghiệp.
              </p>
            </div>
            <div class="col-lg-6 mb-5 mb-lg-0 position-relative">
              <div class="card bg-glass box-login" style="max-width: 450px;margin: auto;">
                <div class="card-body px-4 py-5 px-md-5 login-form">
                  <form name="frlogin" @submit.prevent="login">
                    <div class="login-ava">
                      <Avatar :image="
                        basedomainURL +
                        (store.getters.user.avatar != null
                          ? store.getters.user.avatar
                          : '/Portals/Image/nouser1.png')
                      " shape="circle" size="xlarge" />
                    </div>
                    <div class="mb-3 mt-6 text-center">
                      <h1>Đăng nhập</h1>
                    </div>
                    <div class="field text-lg">
                      <label for="user_id">Tên đăng nhập</label>
                      <InputText id="user_id" type="text" v-model="user.user_id" @input="checkForm"
                        v-bind:class="errors.user_id != '' ? 'invalid' : ''" autocomplete="off" required />
                      <InlineMessage severity="error" v-if="errors.user_id != ''">{{
                        errors.user_id
                      }}</InlineMessage>
                    </div>
                    <div class="field text-lg">
                      <label for="is_psword">Mật khẩu</label>
                      <Password id="is_psword" v-model="user.is_psword" @input="checkForm" v-bind:class="
                        errors.is_psword != 'w-full' ? 'w-full invalid' : ''
                      " autocomplete="off" required toggleMask :feedback="false" v-on:keyup.enter="login">
                      </Password>
                      <InlineMessage severity="error" v-if="errors.is_psword != ''">{{
                        errors.is_psword
                      }}</InlineMessage>
                    </div>
                    <div class="mt-3 font-bold text-lg" style="text-align: end;"><label class="forget-pass"
                        @click="showDialogForgetPss()">Quên mật khẩu?</label></div>

                    <Button label="Đăng nhập" @click="login()" />
                  </form>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>
    </div>
  <Dialog header="Quên mật khẩu?" v-model:visible="dialogForgetPass" :style="{ width: '38vw', zIndex: 100 }"
    :maximizable="true" :autoZIndex="false">
    <form @submit.prevent="sendMail(!v$.$invalid)">
      <div class="grid formgrid m-2" v-if="!isSendEmail">
        <div class="field col-12 md:col-12">
          <label>Vui lòng nhập tên tài khoản đăng nhập và địa chỉ email để tìm kiếm tài khoản của bạn.</label>
        </div>
        <div class="field col-12">
          <label class="col-3 text-left">Tên đăng nhập</label>
          <InputText spellcheck="false" class="col-9 ip36" v-model="mailInfo.to" />
        </div>
        <div class="field col-12">
          <label class="col-3 text-left">Email</label>
          <InputText spellcheck="false" class="col-9 ip36" v-model="mailInfo.email" />
        </div>
        <small v-if="v$.email.email.$invalid && submitted && mailInfo.email != null" class="p-error field col-12 mb-3">
          <label class="col-3 text-left"></label>
          <span class="col-9">
            Email không hợp lệ</span>
        </small>
      </div>
      <div class="grid formgrid m-2" v-else>
        <div class="waiting-email format-center col-12">
          <img :src="basedomainURL + '/Portals/file/waiting-mail.jpg'" height="250" />
        </div>
        <div class="format-center col-12 font-bold text-lg">
          Vui lòng kiểm tra trong hộp thư email của bạn để đổi mật khẩu!
        </div>
      </div>
    </form>
    <template #footer v-if="!isSendEmail">
      <Button label="Huỷ" icon="pi pi-times" @click="dialogForgetPass = false"
        class="p-button-raised p-button-secondary" />
      <Button label="Gửi thông tin" icon="pi pi-check" @click="sendMail(!v$.$invalid)" />
    </template>
  </Dialog>
</template>
<style lang="scss" scoped>
::v-deep(.p-password) {
  .p-inputtext {
    padding: 0.9rem;
  }
}
</style>
<style scoped>
@import url(../assets/style_login.css);
 .bg-img {
  background-size: cover;
  font-size:16px;
  }

  .bg-glass {
    background-color: hsla(0, 0%, 100%, 0.9) !important;
    backdrop-filter: saturate(200%) blur(25px);
  }
  .title-left {
    font-family: Arial;
  }
.forget-pass {
  color: #551a8b;
  cursor: pointer;
  text-decoration: underline;
}

.forget-pass:active {
  color: -webkit-activelink;
}

.invalid {
  color: red;
  margin: 5px 0;
  font-size: 13px;
}

input.invalid {
  border: 1px solid red;
}

.login-container {
  width: 100vw;
  height: 100vh;
}

.login-bg {
  width: 100vw;
  height: 100vh;
  background-color: #70c5c3;
}

.login-bg>img {
  object-fit: cover;
  width: 100%;
  height: 100%;
}

/* .box-login {
  background-color: rgba(255, 255, 255, 0.9);
  flex: 1;
  display: block;
  padding: 50px;
  position: absolute;
  right: 0;
  height: 100%;
  min-width: 480px;
} */

/* .login-form {
  align-items: center;
  vertical-align: middle;
  margin: auto;
  margin-top: calc(50vh - 242px);
  max-width: 480px;

} */

.login-form>button {
  width: 100%;
  margin-top: 20px;
}

.login-form h1 {
  font-size: 20pt;
  margin-bottom: 20px;
}

.box-login .field * {
  display: block;
}

.box-login .field label {
  margin: 1rem 0;
  font-weight: bold;
}

.p-button {
  padding: 0.9rem;
}

.p-avatar {
  border: 1px solid #c9c9c9;
}

.box-login .p-inputtext {
  display: block;
  margin-bottom: 0.5rem;
  width: 100%;
  padding: 0.9rem;
}

.box-login .p-inputtext:last-child {
  margin-bottom: 0;
}

.login-form button {
  width: 100%;
  margin-top: 2rem;
}

.login-org {
  width:100%;
  background-color: #f2f2f2;
  position: absolute;
  top: 0;
  left: 0;
  display: flex;
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
  color: #0153a2;
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
