<script setup>
import { ref, inject, onMounted    } from "vue";
  import { useToast } from "vue-toastification";
 
import treeuser from "../../components/user/treeuser.vue";
import { encr, checkURL } from "../../util/function.js";
//Khai báo
const router = inject("router");
const cryoptojs = inject("cryptojs");
 
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const basedomainURL = baseURL;
 const toast = useToast();
 
const expandedKeys = ref([]);
 
const selectedUser = ref([]);
 
 

const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);
const checkMultile = ref(false);
 
let selectedTreeU = null;
const showTreeUser = (value) => {
  checkMultile.value = true;
  selectedTreeU = value;
  displayDialogUser.value = true;
};

 
const listUserA = ref([]);
  
 
const isFirst = ref(true);
 
 
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
 
 
const options = ref({
  IsNext: true,
  sort: " cp.approved_group_id DESC",
  sortDM: "card_id DESC",
  search: "",
  pageno: 0,
  pagesize: 20,
  pagenoDM: 0,
  pagesizeDM: 10,
  loading: true,
  totalRecords: null,
  totalRecordsDM: null,
  start_date: null,
  end_date: null,
  next: true,
}); 
 
const danhMuc = ref();
//METHOD
 const liData = ref([]);
const closeDialogUser = () => {
  displayDialogUser.value = false;
};
const choiceUser = () => {
  if (checkMultile.value == true)
    datalistsD.value.forEach((m, i) => {
      let om = { key: m.key, data: m };
      if (m.key == selectedTreeU.organization_id) {
        m.data.user_receiver = selectedUser.value[0].user_id;
        return;
      } else {
        let check = false;
        const rechildren = (mm, pid) => {
          if (mm.key == selectedTreeU.organization_id) {
            mm.data.data.user_receiver = selectedUser.value[0].user_id;
            check = true;
            return;
          } else {
            if (mm.data.children) {
              let dts = mm.data.children;

              if (dts.length > 0) {
                dts.forEach((em) => {
                  let om1 = { key: em.key, data: em };
                  if (check) return;
                  rechildren(om1, em.key);
                });
              }
            }
          }
        };
        if (check) return;
        rechildren(om, m.key);
      }
    });
  else {
 
    selectedUser.value.forEach(element => {
      listUserA.value.push(element);
 
    });
  
  }

  closeDialogUser();
};
 
const renderTreeDV1 = (data, id, name, title, org_id) => {
  let arrtreeChils = [];
  let arrChils = [];

  data
    .filter((x) => x.parent_id == null)
    .forEach((m, i) => {
      if (!m.user_receiver) m.user_receiver = null;
      let om = { key: m[id], data: m };

      const rechildren = (mm, pid) => {
        let dts = data.filter((x) => x.parent_id == pid);
        if (dts.length > 0) {
          if (!mm.children) mm.children = [];
          dts.forEach((em) => {
            if (!em.user_receiver) em.user_receiver = null;
            let om1 = { key: em[id], data: em };
            rechildren(om1, em[id]);
            mm.children.push(om1);
          });
        }
      };
      rechildren(om, m[id]);
      arrChils.push(om);
    });
  if (org_id == "") {
    data.forEach((m, i) => {
      let om = { key: m[id], data: m[id], label: m[name] };

      const retreechildren = (mm, pid) => {
        let dts = data.filter((x) => x.parent_id == pid);
        if (dts.length > 0) {
          if (!mm.children) mm.children = [];
          dts.forEach((em) => {
            let om1 = { key: em[id], data: em[id], label: em[name] };
            retreechildren(om1, em[id]);
            mm.children.push(om1);
          });
        }
      };
      retreechildren(om, m[id]);
      arrtreeChils.push(om);
    });
  } else {
    let rew = Number(org_id);
    data
      .filter((x) => x.parent_id == rew)
      .forEach((m, i) => {
        let om = { key: m[id], data: m[id], label: m[name] };

        const retreechildren = (mm, pid) => {
          let dts = data.filter((x) => x.parent_id == pid);
          if (dts.length > 0) {
            if (!mm.children) mm.children = [];
            dts.forEach((em) => {
              let om1 = { key: em[id], data: em[id], label: em[name] };
              retreechildren(om1, em[id]);
              mm.children.push(om1);
            });
          }
        };
        retreechildren(om, m[id]);
        arrtreeChils.push(om);
      });
  }
  return { arrChils: arrChils, arrtreeChils: arrtreeChils };
};
 
const listDropdownUserGive = ref();
const listDropdownUserCheck = ref();
const listDropdownUser = ref();
const listUsers = ref([]);
const loadingUser = ref(false);
const onFilterUserDropdown = (value) => {
  loadingUser.value = true;

  if (value.organization_id == 1)
    listDropdownUserGive.value = listDropdownUser.value;
  else
    listDropdownUserGive.value = listDropdownUser.value.filter(
      (x) => x.department_id == value.organization_id,
    );
  loadingUser.value = false;
};
const loadUser = () => {
  listUsers.value = [];
  listDropdownUser.value = [];
  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_users_list_dd",
            par: [
              { par: "search", va: null },
              { par: "user_id", va: store.getters.user.user_id },
              { par: "role_id", va: null },
              {
                par: "organization_id",
                va: store.getters.user.organization_id,
              },
              { par: "department_id", va: null },
              { par: "position_id", va: null },
              { par: "pageno", va: 1 },
              { par: "pagesize", va: 10000 },
              { par: "isadmin", va: null },
              { par: "status", va: null },
              { par: "start_date", va: null },
              { par: "end_date", va: null },
            ],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];

      data.forEach((element, i) => {
        listDropdownUser.value.push({
          name: element.full_name,
          code: element.user_id,
          avatar: element.avatar,
          department_name: element.department_name,
          department_id: element.department_id, position_name:element.position_name,
          role_name: element.role_name,
          organization_id: element.organization_id,
        });
        listUsers.value.push({ data: element, active: false });
      });
      listUsers.value = data;
      listDropdownUserGive.value = listDropdownUser.value;
      listDropdownUserCheck.value = listDropdownUser.value.filter(
        (x) => x.code != store.getters.user.user_id,
      );
      loadOrganization(store.getters.user.organization_id);
    })
    .catch((error) => {
    
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
 
const datalistsD = ref();
const loadOrganization = (value) => {
  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_organization_list_d",
            par: [
 
              { par: "user_id", va: store.getters.user.user_id },
            ],
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
        let obj = renderTreeDV1(
          data,
          "organization_id",
          "organization_name",
          "đơn vị",
          value,
        );
        datalistsD.value = obj.arrChils;
        datalistsDSave.value = obj.arrChils;
 
        expandListD(datalistsD.value);
      }
    })
    .catch((error) => {
     
      options.value.loading = false;
    });
};
const datalistsDSave = ref();
const expandListD = (data) => {
  for (let node of data) {
    expandedKeys.value[node.key] = true;
    expandNode(node);
  }
};
const expandNode = (node) => {
  if (node.children && node.children.length) {
    expandedKeys.value[node.key] = true;
    for (let child of node.children) {
      expandNode(child);
    }
  }
}; 
const displayDialogUser = ref(false);

const reOrganization = (data) => {
   if (data.data != null) {
    if (data.data.user_receiver)
      liData.value.push({
        user_receiver: data.data.user_receiver,
        organization_id: data.data.organization_id,
      });
      else{
          liData.value.push({
        user_receiver: null,
        organization_id: data.data.organization_id,
      });
      }
    if (data.children)
      data.children.forEach((em) => {
        reOrganization(em);
      });
 
 
}};
 const saveDeConfig=()=>{
liData.value=[];
for (const key in datalistsD.value) {
   
  if (Object.hasOwnProperty.call(datalistsD.value, key)) {
    const element = datalistsD.value[key];
    reOrganization(element);
  }
}
   axios
     .put(
        baseURL + "/api/device_handover/update_config_user_handover",
         liData.value,
        config
      )
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Cập nhật người nhận phòng ban thành công!");
               loadOrganization(store.getters.user.organization_id)
            } else {
              swal.fire({
                title: "Error!",
                text: "Có lỗi xảy ra vui lòng thử lại sau",
                icon: "error",
                confirmButtonText: "OK",
              });
            }
          })
          .catch((error) => {
            swal.close();
        
          });
 
 }

onMounted(() => {
    if (!checkURL(window.location.pathname, store.getters.listModule)) {
     //router.back();
  }
  loadUser();
  
 
  return {
    isFirst,
    options,
    danhMuc,
  };
});
</script>
<template>
 <div class="d-container   " style="margin: 0 15%;">
    <div class="d-lang-table surface-0">
      <div class=" w-full  p-4 style-vb-1  text-center text-3xl">
          BẢNG THIẾT LẬP NGƯỜI NHẬN PHÒNG BAN
        </div>
        <div class="w-full p-0 style-vb-2 text-center text-xl"  >
    BẢO HIỂM XÃ HỘI BỘ QUỐC PHÒNG
        </div>
       
      <div class="grid mt-4 ">
        <div class="col-12 p-0  check-scroll"   >
            <TreeTable :expandedKeys="expandedKeys" :value="datalistsD">
              <Column class="w-7" field="organization_name" :expander="true">
                <template #header>
                  <div class="w-full format-center">
                    <i class="pi pi-building pr-2"></i> Phòng ban
                  </div>
                </template>
              </Column>
              <Column class="w-5">
                <template #header>
                  <div class="w-full format-center">
                    <i class="pi pi-user pr-2"></i> Người nhận
                  </div>
                </template>
                <template #body="data">
                  <div class="w-full flex">
                    <Button
                      @click="showTreeUser(data.node.data)"
                      v-tooltip.top="'Chọn người duyệt'"
                      icon="pi pi-user-plus"
                      class="p-button-rounded w-3rem mx-3"
                    ></Button>

                    <Dropdown
                      v-model="data.node.data.user_receiver"
                      :options="listDropdownUserGive"
                      :filter="true"
                      optionLabel="name"
                      optionValue="code"
                      class="w-full p-design-dropdown"
                      placeholder="Chọn người duyệt "
                      :showClear="true"
                      :virtualScrollerOptions="{
                        lazy: true,
                        itemSize: 1,
                        showLoader: true,
                        loading: loadingUser,
                        delay: 250,
                      }"
                      @click="onFilterUserDropdown(data.node.data)"
                    >
                      <template #value="slotProps">
                        <div
                          class="country-item country-item-value flex align-items-center"
                          v-if="slotProps.value"
                        >
                          <Avatar
                            v-bind:label="
                              listDropdownUser.filter(
                                (x) => x.code == slotProps.value,
                              )[0].avatar
                                ? ''
                                : listDropdownUser
                                    .filter((x) => x.code == slotProps.value)[0]
                                    .name.substring(
                                      listDropdownUser
                                        .filter(
                                          (x) => x.code == slotProps.value,
                                        )[0]
                                        .name.lastIndexOf(' ') + 1,
                                      listDropdownUser
                                        .filter(
                                          (x) => x.code == slotProps.value,
                                        )[0]
                                        .name.lastIndexOf(' ') + 2,
                                    )
                            "
                            :image="
                              basedomainURL +
                              listDropdownUser.filter(
                                (x) => x.code == slotProps.value,
                              )[0].avatar
                            "
                            class="w-2rem h-2rem mr-2"
                            size="large"
                            :style="
                              listDropdownUser.filter(
                                (x) => x.code == slotProps.value,
                              )[0].avatar
                                ? 'background-color: #2196f3'
                                : 'background:' +
                                  bgColor[
                                    listDropdownUser.filter(
                                      (x) => x.code == slotProps.value,
                                    )[0].name.length % 7
                                  ]
                            "
                            shape="circle"
                            @error="
                              $event.target.src =
                                basedomainURL + '/Portals/Image/nouser1.png'
                            "
                          />
                          <div>
                            {{
                              listDropdownUser.filter(
                                (x) => x.code == slotProps.value,
                              )[0].name
                            }}
                          </div>
                        </div>
                        <span v-else>
                          {{ slotProps.placeholder }}
                        </span>
                      </template>
                      <template #option="slotProps">
                        <div class="country-item flex align-items-center">
                          <Avatar
                            v-bind:label="
                              slotProps.option.avatar
                                ? ''
                                : slotProps.option.name.substring(
                                    slotProps.option.name.lastIndexOf(' ') + 1,
                                    slotProps.option.name.lastIndexOf(' ') + 2,
                                  )
                            "
                            :image="basedomainURL + slotProps.option.avatar"
                            class="w-3rem h-3rem"
                            size="large"
                            :style="
                              slotProps.option.avatar
                                ? 'background-color: #2196f3'
                                : 'background:' +
                                  bgColor[slotProps.option.name.length % 7]
                            "
                            shape="circle"
                            @error="
                              $event.target.src =
                                basedomainURL + '/Portals/Image/nouser1.png'
                            "
                          />
                          <div class="pt-1 pl-2">
                            {{ slotProps.option.name }}
                          </div>
                        </div>
                      </template>
                    </Dropdown>
                  </div>
                </template>
              </Column>
            </TreeTable>
        



        </div>
      
      
         
        <div class="col-12 style-vb-3 py-4 text-center format-center">
          <Button @click="saveDeConfig()">Cập nhật</Button>
        </div>
        <div class="col-12 p-0 format-center style-vb-5">
          <div class="col-8 p-0 text-left align-items-center">
            <div class="w-full">
              <font-awesome-icon
                style="color: #ecec15"
                icon="fa-solid fa-circle-info"
              />
              Bảng thiết lập người nhận phòng ban, mục đích là cấu hình người nhận mặc định của một phòng ban khi thiết bị được cấp phát cho một phòng ban.
            </div>
            

            
          </div>
        </div>
      </div>
    </div>
    
  <treeuser
    v-if="displayDialogUser === true"
    :headerDialog="headerDialogUser"
    :displayDialog="displayDialogUser"
    :closeDialog="closeDialogUser"
    :one="checkMultile"
    :selected="selectedUser"
    :choiceUser="choiceUser"
  />
  </div>
     
</template>
<style scoped>
.d-lang-table {
  margin: 16px;
  height: calc(100vh - 50px);
}
@media only screen and (max-height: 768px) {

  .style-vb-1{
    font-size: large !important;
    padding: 8px !important;;
  }
  .style-vb-2{
    font-size: small !important;
  }
  .style-vb-3{
    padding: 8px !important;
  }
 .check-scroll{
    max-height:32rem ;
     overflow:scroll
  }
}
@media only screen and (max-height: 678px) {
  .style-vb-5{
    display: none;
  }
  .check-scroll{
    max-height:32rem ;
     overflow:scroll
  }
}

</style>