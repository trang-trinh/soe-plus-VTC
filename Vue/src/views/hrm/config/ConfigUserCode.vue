<script setup>
import { ref, inject, onMounted } from "vue";
import { useToast } from "vue-toastification";

import treeuser from "../../../components/user/treeuser.vue";
import { encr, checkURL } from "../../../util/function.js";
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
    .put(
      baseURL + "/api/hrm_config_usercode/update_data",
      null,
      config
    )
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        swal.close();

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
};

const datalistsD = ref();
const datalistsBD = ref();
const loadOrganization = (value) => {
  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_config_usercode_list",
            par: [

              { par: "user_id", va: store.getters.user.user_id },
              { par: "status", va: true },
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

        configUserCodeMain.value = data.filter(x => x.organization_type == 3);
        data = data.filter(x => x.organization_type == 0 && x.organization_id != 162);
        datalistsBD.value=[...data];
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
const configUserCodeMain = ref([]);
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

    liData.value.push(data.data);

    if (data.children)
      data.children.forEach((em) => {
        reOrganization(em);
      });


  }
};
const saveDeConfig = () => {
  liData.value = [];
  // for (const key in datalistsD.value) {

  //   if (Object.hasOwnProperty.call(datalistsD.value, key)) {
  //     const element = datalistsD.value[key];
  //     reOrganization(element);
  //   }
  // } 
  datalistsBD.value.forEach(element => {
    liData.value.push(element);
  });
  
  let formData = new FormData();
  if(configUserCodeMain.value.length>0){
    liData.value.push(configUserCodeMain.value[0])
  }
  formData.append("hrm_config_usercode", JSON.stringify(liData.value));
  axios
    .put(
      baseURL + "/api/hrm_config_usercode/update_config_usercode",
      formData,
      config
    )
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật mã nhân sự thành công!");
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
  
   
  loadUser();
  return {
    isFirst,
    options,
    danhMuc,
  };
});
</script>
<template>
  <div class="d-container  p-0 "  >
    <div class=" p-0 surface-0  check-scroll">
      <div class="grid mt-0 p-0 m-0 ">


        <div class="col-12 field   p-0" v-if="configUserCodeMain.length>0">
          <DataTable :value="configUserCodeMain">
            <Column   label="">
              <template #body="data">
                <div class="w-full format-center font-bold ">
                  {{ data.data.organization_name }}
                </div>
              </template>
            </Column>
            <Column headerStyle="text-align:center; width:200px" bodyStyle="text-align:center; width:200px" field="organization_name">
              <template #header>
                <div class="w-full format-center">
                  Ký hiệu
                </div>
              </template>
              <template #body="data">
                <div class="w-full format-center">
                  <InputText class="w-full  duy-inpput" v-model="data.data.symbol" type="text" spellcheck="false" />
                </div>
              </template>
            </Column>
            <Column headerStyle="text-align:center; width:100px" bodyStyle="text-align:center; width:100px" field="organization_name">
              <template #header>
                <div class="w-full format-center">
                  Độ dài
                </div>
              </template>
              <template #body="data">
                <div class="w-full  ">
                  <InputNumber  class="w-full  duy-inpput" v-model="data.data.length" :min="1" spellcheck="false" />
                </div>
              </template>
            </Column>
            <Column headerStyle="text-align:center; width:100px" bodyStyle="text-align:center; width:100px" field="organization_name">
              <template #header>
                <div class="w-full  ">
                  Khởi tạo
                </div>
              </template>
              <template #body="data">
                <div class="w-full ">
                  <InputNumber class="w-full duy-inpput " :min="0" v-model="data.data.initialization" spellcheck="false" />
                </div>
              </template>
            </Column>
            <Column headerStyle="text-align:center; width:100px" bodyStyle="text-align:center; width:100px" field="organization_name">
              <template #header>
                <div class="w-full  ">
                  Tự động
                </div>
              </template>
              <template #body="data">
                <div class="w-full  ">
                  <InputSwitch class="w-4rem lck-checked" v-model="data.data.automatic" />
                </div>
              </template>
            </Column>

          </DataTable>


        </div>
        <div class="col-12 p-0   ">

          <DataTable  :value="datalistsBD" class="p-treetable-sm" :rowHover="true"
            :lazy="true" scrollHeight="flex">
            <Column field="organization_name"  
            headerStyle="text-align:center " 
            >
              <template #header>
                <div class="w-full ">
                  <i class="pi pi-building pr-2"></i>Tên công ty
                </div>
              </template>
            </Column>
            <Column headerStyle="text-align:center; width:200px" bodyStyle=" ; width:200px"
              field="organization_name">
              <template #header>
                <div class="w-full format-center">
                  Ký hiệu
                </div>
              </template>
              <template #body="data">
                <div class="w-full  ">

                  <InputText class="w-full duy-inpput "  v-model="data.data.symbol" spellcheck="false" />
                </div>
              </template>
            </Column>
            <Column headerStyle="text-align:center; width:100px" bodyStyle="text-align:center; width:100px"
              class="align-items-center justify-content-center text-center" field="organization_name">
              <template #header>
                <div class="w-full format-center">
                  Độ dài
                </div>
              </template>
              <template #body="data">
                <div class="w-full  ">

                  <InputNumber :min="1" class="w-full duy-inpput " v-model="data.data.length" type="text" spellcheck="false" />
                </div>
              </template>
            </Column>
            <Column headerStyle="  text-align:center; width:100px" bodyStyle="text-align:center; width:100px"
              class="align-items-center justify-content-center text-center" field="organization_name"
              
              
              
              
              
              >
              <template #header>
                <div class="w-full  ">
                  Khởi tạo
                </div>
              </template>
              <template #body="data">
                <div class="w-full ">

                  <InputNumber :min="0"    style=" max-width: 100px;  text-align:center !important" class="w-full duy-inpput " v-model="data.data.initialization" spellcheck="false" />
                </div>
              </template>
            </Column>
            <Column headerStyle="text-align:center;width:100px" bodyStyle="text-align:center;width:100px"
              class="align-items-center justify-content-center text-center" field="organization_name">
              <template #header>
                <div class="w-full format-center">
                  Tự động
                </div>
              </template>
              <template #body="data">
                <div class="w-full format-center">

                  <InputSwitch class="w-4rem lck-checked" v-model="data.data.automatic" spellcheck="false" />
                </div>
              </template>
            </Column>

          </DataTable>



        </div>



        <div class="col-12 style-vb-3 py-4 text-center format-center">
          <Button @click="saveDeConfig()">Cập nhật</Button>
        </div>
        <div class="col-12 p-0 format-center style-vb-5">
          <div class="col-8 p-0 text-left align-items-center">
            <!-- <div class="w-full">
                        <font-awesome-icon
                          style="color: #ecec15"
                          icon="fa-solid fa-circle-info"
                        />
                        Bảng thiết lập người nhận phòng ban, mục đích là cấu hình người nhận mặc định của một phòng ban khi thiết bị được cấp phát cho một phòng ban.
                      </div> -->



          </div>
        </div>
      </div>
    </div>

    <treeuser v-if="displayDialogUser === true" :headerDialog="headerDialogUser" :displayDialog="displayDialogUser"
      :closeDialog="closeDialogUser" :one="checkMultile" :selected="selectedUser" :choiceUser="choiceUser" />
  </div>
</template>
<style scoped>
 

 

@media only screen and (max-height: 768px) {

  .style-vb-1 {
    font-size: large !important;
    padding: 8px !important;
    ;
  }

  .style-vb-2 {
    font-size: small !important;
  }

  .style-vb-3 {
    padding: 8px !important;
  }

 
}

@media only screen and (max-height: 678px) {
  .style-vb-5 {
    display: none;
  }

 
}
</style>