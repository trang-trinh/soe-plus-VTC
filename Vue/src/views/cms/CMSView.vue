<script setup>
//Import
import { RouterView } from "vue-router";
import { inject, onMounted, ref } from "vue";
import { useToast } from "vue-toastification";

//Khai báo biến
const toast = useToast();
const store = inject("store");
const axios = inject("axios"); // inject axios
const menu = ref([]);
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
//Vue App
const initModule = () => {
  axios
    .get(
      baseURL +
        "/api/Cache/ListModuleUserCache?cache=" +
        store.getters.user.user_id,
      {
        headers: { Authorization: `Bearer ${store.getters.token}` },
      },
    )
    .then((response) => {
      let dt = JSON.parse(response.data.data);
      let data = dt[0];
      if (data.length > 0) {
        let pathname = window.location.pathname;
        let mdCMS = data.find((x) => xis_link == pathname);
        data
          .filter((x) => x.Capcha_ID == mdCMS.module_id)
          .forEach((md) => {
            let obj = {
              title: md.module_name,
              icon: md.icon,
              href: mdis_link,
            };
            let childs = data.filter((x) => x.Capcha_ID == md.module_id);
            if (childs.length > 0) {
              obj.child = [];
              childs.forEach((md1) => {
                let obj1 = {
                  title: md1.module_name,
                  icon: md1.icon,
                  href: md1is_link,
                };
                childs = data.filter((x) => x.Capcha_ID == md1.module_id);
                if (childs.length > 0) {
                  obj1.child = [];
                  childs.forEach((md2) => {
                    let obj2 = {
                      title: md2.module_name,
                      icon: md2.icon,
                      href: md2is_link,
                    };
                    obj1.child.push(obj2);
                  });
                }
                obj.child.push(obj1);
              });
            }
            menu.value.push(obj);
          });
      }
    })
    .catch((error) => {
      console.log(error);
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
  if (store.getters.islogin) {
    initModule();
  }
  return {};
});
</script>
<template>
  <div
    v-if="store.getters.islogin"
    class="app-system flex flex-row flex-grow-1 h-full"
    style="width: auto"
  >
    <div class="flex flex-column flex-grow-1 h-full">
      <RouterView />
    </div>
  </div>
</template>
<style scoped>
.AppSideBar {
  background-color: #fff;
}
.vsm_white-theme {
  font-weight: 700;
}
.vsm_collapsed .hversion {
  display: none;
}
</style>
