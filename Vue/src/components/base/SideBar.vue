<script setup>
//Import
import { inject, onMounted, ref } from "vue";
import { encr } from "../../util/function.js";
const cryoptojs = inject("cryptojs");
//Khai báo biến
const basedomainURL = fileURL;
const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios"); // inject axios
const emitter = inject("emitter");
var data_menus = [];
const menu = ref([
  /*  {
     header: "",
     hiddenOnCollapse: true,
   }, */
  {
    href: "/",
    title: "Trang chủ",
    icon: "pi pi-home",
    command: () => {
      console.log("12");
    },
  },
]);
emitter.on("emitData", (obj) => {
  switch (obj.type) {
    case "moduleFromUrl":
      loadModulesMenu(obj.data);
      break;
    default:
      break;
  }
});
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const router = inject("router");
const appconfig = ref({ version: "1.0" });
const initModule = () => {
  // data memu left o trang chu

  axios
    .get(
      baseURL +
        "/api/Cache/ListModuleUserCache?cache=" +
        store.getters.user.user_id,
      {
        headers: { Authorization: `Bearer ${store.getters.token}` },
      }
    )
    .then((response) => {
      let dt = JSON.parse(response.data.data);
      data_menus = dt[0];
    
           
    //  if(store.getters.listModule.length==0){
         
    //    store.commit("setlistModule",data_menus);
  
    //  }

      menu.value = [
        {
          href: "/",
          title: "Trang chủ",
          icon: "pi pi-home",
        },
      ];
      if (data_menus.length > 0) {
        data_menus
          .filter(
            (x) => x.parent_id == null //&& (x.IsVitri == null || x.IsVitri.includes("Menu"))
          )
          .forEach((md) => {
            let obj = {
              title: md.module_name,
              icon: md.icon,
              href: md.is_link,
            };
            let childs = data_menus.filter(
              (x) => x.parent_id == md.module_id //&&(x.IsVitri == null || x.IsVitri.includes("Menu"))
            );
            if (childs.length > 0) {
              obj.child = [];
              childs.forEach((md1) => {
                let obj1 = {
                  title: md1.module_name,
                  icon: md1.icon,
                  href: md1.is_link,
                };
                childs = data_menus.filter((x) => x.parent_id == md1.module_id);
                if (childs.length > 0) {
                  obj1.child = [];
                  childs.forEach((md2) => {
                    let obj2 = {
                      title: md2.module_name,
                      icon: md2.icon,
                      href: md2.is_link,
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
      if (dt.length > 1) {
        let u = store.getters.user;
        u.organization_id = dt[1][0].organization_id;
        u.role_id = dt[1][0].role_id;
        u.organization_name = dt[1][0].organization_name;
        u.product_name = dt[1][0].product_name;
        u.logo = dt[1][0].logo;
        u.full_name = dt[1][0].full_name;
        u.user_key = dt[1][0].user_key;
        u.background_image = dt[1][0].background_image;
        u.is_super = dt[1][0].is_super;
        store.commit("setuser", u);
        if (u.organization_name)
          document.getElementsByTagName("title")[0].innerText =
            u.organization_name;
      }
         
      // let lang = dt[3].find((x) => x.is_main);
      // store.commit("setlangs", dt[3]);
      // store.commit("setlang", lang);
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
const loadModulesMenu = (module) => {
  //data menu chi tiet cho tung module top
  axios
    .post(
      baseURL + "/api/Notify/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_modules_listbymodule_id",
            par: [{ par: "module_id", va: module.module_id }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let dt = JSON.parse(response.data.data);
      let data = dt[0];
      if (data.length > 0) {
        menu.value = [
          {
            href: "/",
            title: "Trang chủ",
            icon: "pi pi-home",
          },
        ];

        //list con cap 2
        data
          .filter(
            (x) =>
              x.lv == 2 &&
              data_menus.map((x) => x.module_id).includes(x.module_id) //&& (x.IsVitri == null || x.IsVitri.includes("Menu"))
          )
          .forEach((md) => {
            let obj = {
              title: md.module_name,
              icon: md.icon,
              href: md.is_link,
            };
            //list con cap 3
            let childs = data.filter(
              (x) =>
                x.parent_id == md.module_id &&
                data_menus.map((x) => x.module_id).includes(x.module_id) //&&(x.IsVitri == null || x.IsVitri.includes("Menu"))
            );
            if (childs.length > 0) {
              obj.child = [];
              childs.forEach((md1) => {
                let obj1 = {
                  title: md1.module_name,
                  icon: md1.icon,
                  href: md1.is_link,
                };
                childs = data.filter((x) => x.parent_id == md1.module_id);
                if (childs.length > 0) {
                  obj1.child = [];
                  childs.forEach((md2) => {
                    let obj2 = {
                      title: md2.module_name,
                      icon: md2.icon,
                      href: md2.is_link,
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
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const reloadDashboardMenu = (event, item) => {
  if (item.title !== "Trang chủ") return false;
  initModule();
};
//Vue App
onMounted(() => {
  if (store.getters.islogin) {
    initModule();
  }
  return {};
});
</script>
<template>
  <sidebar-menu
    :menu="menu"
    @item-click="reloadDashboardMenu"
    class="vsm_white-theme"
  >
    <template v-slot:footer></template>
    <template v-slot:toggle-icon>
      <img class="vsm--logo" :src="basedomainURL + store.getters.user.logo" />
      <!-- <img class="vsm--logo" src="../../assets/logo_nobg.png" />
      <h5 class="ml-2 hversion">SmartOffice</h5> -->
    </template>
  </sidebar-menu>
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
