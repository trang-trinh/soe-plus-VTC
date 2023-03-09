<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
import { encr } from "../../util/function.js";

const cryoptojs = inject("cryptojs");
const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios");
const emitter = inject("emitter");
const router = inject("router");

const isDynamicSQL = ref(false);
const basedomainURL = fileURL;
const config = {
  headers: {
    Authorization: `Bearer ${store.getters.token}`,
  },
};
const isChangePass = ref(false);
const checkTime = ref(false);
const errors = ref({ password: "", repassword: "" });
const checkForm = () => {
  if (!rePass.value.password) {
    errors.value.password = "Mật khẩu không được để trống!";
  } else if (rePass.value.password.length < 8) {
    errors.value.password = "Mật khẩu không được ít hơn 8 ký tự!";
  } else {
    errors.value.password = "";
  }
  if (rePass.value.repassword.length>0 && !rePass.value.password.includes(rePass.value.repassword )) {
    errors.value.repassword = "Vui lòng nhập mật khẩu giống nhau!";
  } 
  else {
    errors.value.repassword = "";
  }
};
//Get arguments
const props = defineProps({
  uid: String,
  code: Boolean,
});
const rePass = ref({
  user_id: props.uid,
  password: "",
  repassword: "",
})
const getprop = (id, code) => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios
    .post(
      baseURL + "/api/Login/checkForgetPass",
      {
        str: encr(
          JSON.stringify({
            uid: id,
            code: code
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
    )
    .then((response) => {
      swal.close();
      if (response.data.err == '0') {
        checkTime.value = false;
      }
      else {
        checkTime.value = true;
      }
    })
    .catch((error) => { });
}
const savePass = ()=>{
  if(rePass.value.password.length <8 ){
    swal.fire({
      title: "Thông báo!",
      text: "Mật khẩu không được ít hơn 8 ký tự!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  if(rePass.value.password != rePass.value.repassword){
    swal.fire({
      title: "Thông báo!",
      text: "Vui lòng nhập lại mật khẩu giống nhau!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios
    .post(
      baseURL + "/api/Login/changePass",
      {
        str: encr(
          JSON.stringify(
            rePass.value
          ),
          SecretKey,
          cryoptojs
        ).toString(),
      },
    )
    .then((response) => {
      swal.close();
      if (response.data.err == '0') {
        isChangePass.value = true;
        toast.success("Đổi mật khẩu thành công!");
      }
      else {
        swal.fire({
          title: "Thông báo",
          html: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    })
    .catch((error) => { });
}
const goLogin = ()=>{
  router.push({ path: "/login" });
}
onMounted(() => {
  if (props) {
    getprop(props.uid, props.code);
  }
  //initRender();
  return {};
});
</script>

<template>
  <div v-if="!checkTime">
    <div class="h-full format-center" style="background-color: #eee;">
      <Card class="px-4" style="width: 40vw;">
        <template #header>
        <h3>Thay đổi mật khẩu</h3>
      </template>
        <template #content>
          <form @submit.prevent="savePass">
          <div class="grid formgrid m-2">
            <div class="field col-12 md:col-12">
              <label class="col-4 text-left" for="is_psword">Mật khẩu mới</label>
              <Password id="is_psword" class="col-8 p-0" v-model="rePass.password" @input="checkForm" v-bind:class="
              errors.password != '' ? 'invalid' : ''
            " autocomplete="off" required toggleMask :feedback="false" >
            </Password>
            <InlineMessage style="margin-left:50px" severity="error" v-if="errors.password != ''">{{
              errors.password
            }}</InlineMessage>
              <!-- <InputText spellcheck="false" class="col-9 ip34" type="password" /> -->
            </div>
            <div class="field col-12 md:col-12">
              <label class="col-4 text-left">Nhập lại mật khẩu mới</label>
              <InputText @input="checkForm"  spellcheck="false" class="col-8 ip34" type="password"  onpaste="return false" v-model="rePass.repassword"/>
              <InlineMessage style="margin-left:50px" severity="error" v-if="errors.repassword != ''">{{
              errors.repassword
            }}</InlineMessage>
            </div>
            
          </div>
          <div v-if="isChangePass" class="text-sussces">Đã đổi mật khẩu thành công! Vui lòng <a href="/login" >đăng nhập</a> lại</div>
        </form>
        </template>
        <template #footer v-if="!isChangePass">
        <div class="text-center">
          <Button
            class="mr-2"
            icon="pi pi-save"
            label="Đổi mật khẩu"
            @click="savePass"
          />  
        </div>
      </template>
      </Card>
    </div>
  </div>
  <div v-else>
    <div class="format-center">
      <img :src="basedomainURL + '/Portals/file/later.png'" style="height:65vh" />
    </div>
    <div class="text-center">
      <h2>Đã quá thời gian kể từ lúc bạn gửi yêu cầu. Vui lòng thử lại!</h2>
    </div>
  </div>
</template>
<style scoped>
.ip34{
  height:34px !important; 
}
.text-sussces{
  font-weight: 700;
    font-size: 1.125rem;
    /* color: #28a745!important; */
    font-style: italic;
}
.invalid{
  border-color:#f44336;
}
</style>
<style lang="scss" scoped>
::v-deep(.p-password) {
  .p-password-input {
    width: 100%;
    height: 34px;
  }
}
</style>
