<script setup>
import { onMounted, ref, inject } from "vue";
import { encr } from "../../util/function.js";
const cryoptojs = inject("cryptojs");
const axios = inject("axios"); // inject axios
const store = inject("store");
const swal = inject("$swal");
const basedomainURL = fileURL;
const menu = ref([
 /*  {
    header: "",
    hiddenOnCollapse: true,
  }, */
  {
    href: "/",
    title: "Dashboard",
    icon: "pi pi-home",
  },
]);
const config = {
    headers: { Authorization: `Bearer ${store.getters.token}` },
};
const modules = ref([]);
const initModulesMenu = () => {
    axios
        .post(
      baseURL + "/api/Notify/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_modules_listmodulestop",
                par: [
                    { par: "user_id", va: store.getters.user.user_id }
                ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
        .then((response) => {
            let data = JSON.parse(response.data.data);
            if (data.length > 0) {
                 
                modules.value = data[0];
            }
        })
        .catch((error) => {
            if (error.status === 401) {
                swal.fire({
                    text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
                    confirmButtonText: "OK",
                });
            }
        });
};

onMounted(() => {
    //init
    initModulesMenu();
    return {};
});
</script>
<template>
    <div class="grid">
        <div class="col-4" v-for="item in modules" :key="item.module_id">
            <a class="link-module" :href="item.is_link || '#'" :target="item.is_target || '_self'">
                <div class="box-module flex align-items-center flex-column">
                    <img class="mb-2" height="48" :src="basedomainURL + item.image" />
                    <div><span>{{ item.module_name }}</span></div>
                </div>
            </a>
        </div>
    </div>
</template>
<style scoped>
.box-module:hover {
    border: 1px solid #eee;
    background-color: #f0f8ff;
}

.box-module {
    padding: 1rem;
    border: 1px solid transparent;
}

.link-module {
    color: #000;
    text-decoration: none;
    font-weight: bold;
}
</style>
