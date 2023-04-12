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
    selectedUser.value.forEach((element) => {
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
      (x) => x.department_id == value.organization_id
    );
  loadingUser.value = false;
};
const ins_checked = ref(false);
const datalistsD = ref();
const datalists = ref([]);
const arrDisabled=ref([]);
const loadOrganization = () => {
  arrDisabled.value=[];
  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_config_insurance_rate_list",
            par: [{ par: "user_id", va: store.getters.user.user_id }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        data.forEach((element) => {
          ins_checked.value=element.is_auto;
          element.from_month = new Date(element.from_month);
          arrDisabled.value.push(   element.from_month);
        });
        datalists.value = [...data];
      }
    })
    .catch((error) => {
      options.value.loading = false;
    });
};
 
 
const displayDialogUser = ref(false);

 
const saveDeConfig = () => {
 
 
  let formData = new FormData();
 
  formData.append("hrm_config_insurance_rate", JSON.stringify(datalists.value));
  formData.append("ins_checked", JSON.stringify(ins_checked.value));
  axios
    .put(
      baseURL + "/api/hrm_config_insurance_rate/update_hrm_config_insurance_rate",
      formData,
      config
    )
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật tỷ lệ bảo hiểm thành công!");
        loadOrganization();
      } else {
        loadOrganization();
        swal.fire({
          title: "Thông báo",
          text: response.data.ms,
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    })
    .catch((error) => {
      swal.close();
    });
};
const add_Item = () => {
  let obj = {
    is_order: datalists.value.length + 1,
    from_month: null,
    insurance_rate_name: null,
    social_ins_percent: null,
    social_ins_company: null,
    social_ins_employee: null,
    accident_ins_percent: null,
    accident_ins_company: null,
    accident_ins_employee: null,
    health_ins_percent: null,
    health_ins_company: null,
    health_ins_employee: null,
    unemployment_ins_percent: null,
    unemployment_ins_company: null,
    unemployment_ins_employee: null,
    is_auto: false,
  };
  datalists.value.push(obj);
};
const onchangeInsurance = (data, check) => {
  var ard = datalists.value.find(
    (x) => x.insurance_rate_id == data.insurance_rate_id
  );

  if (ard != null) {
    if (check == 1)
      if (ard.social_ins_percent - ard.social_ins_company >= 0)
        ard.social_ins_employee =
          ard.social_ins_percent - ard.social_ins_company;
      else
        ard.social_ins_percent =
          ard.social_ins_company + ard.social_ins_employee;
    if (check == 2)
      if (ard.accident_ins_percent - ard.accident_ins_company >= 0)
        ard.accident_ins_employee =
          ard.accident_ins_percent - ard.accident_ins_company;
      else
        ard.accident_ins_percent =
          ard.accident_ins_employee + ard.accident_ins_company;
    if (check == 3)
      if (ard.health_ins_percent - ard.health_ins_company >= 0)
        ard.health_ins_employee =
          ard.health_ins_percent - ard.health_ins_company;
      else
        ard.health_ins_percent =
          ard.health_ins_employee + ard.health_ins_company;
    if (check == 4)
      if (ard.unemployment_ins_percent - ard.unemployment_ins_company >= 0)
        ard.unemployment_ins_employee =
          ard.unemployment_ins_percent - ard.unemployment_ins_company;
      else
        ard.unemployment_ins_percent =
          ard.unemployment_ins_employee + ard.unemployment_ins_company;
  }
};


const delRow_Item = (item) => {
 if(!item.insurance_rate_id){


datalists.value=datalists.value.filter(x=>x!=item);

 }
 else{
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá bản ghi này không!",
      icon: "warning",
      showCancelButton: true,
      confirmButtonColor: "#3085d6",
      cancelButtonColor: "#d33",
      confirmButtonText: "Có",
      cancelButtonText: "Không",
    })
    .then((result) => {
      if (result.isConfirmed) {
        swal.fire({
          width: 110,
          didOpen: () => {
            swal.showLoading();
          },
        });

        axios
          .delete(baseURL + "/api/hrm_config_insurance_rate/delete_hrm_config_insurance_rate", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: item != null ? [item.insurance_rate_id] : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá bản ghi thành công!");
        loadOrganization();
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
            swal.close();
            if (error.status === 401) {
              swal.fire({
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
  }
 
};
onMounted(() => {
  loadOrganization();

  return {
    isFirst,
    options,
    danhMuc,
  };
});
</script>
<template>
  <div class="d-container p-0">
    <div class="p-0 surface-0">
      <div class="grid mt-0 p-0 m-0">
        <div class="col-12 field p-0 font-bold">
          <div class="col-12 p-0 format-center text-xl">Tỷ lệ đóng bảo hiểm</div>
        </div>
        <div class="col-12 field format-center p-0 font-bold">
          <Toolbar class="w-full custoolbar">

            <template #end>
              <div>
                <Button @click="add_Item" icon="pi pi-plus" />
              </div>
            </template>
          </Toolbar>
        
        </div>

        <div class="col-12 field p-0">
          <DataTable
            :value="datalists"
            :scrollable="true"
            :lazy="true"
            :rowHover="true"
            :showGridlines="true"
            scrollDirection="both"
            :disabledDates="arrDisabled"
          >
            <!-- <Column
                  field="card_number"
                  header="STT"
                  headerStyle="text-align:center;width:50px;height:50px"
                  bodyStyle="text-align:center;width:50px;"
                  class="align-items-center justify-content-center text-center"
                >
                  <template #body="slotProps">
                    {{ slotProps.data.is_order }}
             
                  </template>
                </Column> -->
            <Column
              field="card_number"
              headerStyle="text-align:center;width:120px;height:50px"
              bodyStyle="text-align:center;width:120px;"
              class="align-items-center justify-content-center text-center"
            >
              <template #header>
                <div>Từ tháng <span class="redsao pl-1"> (*)</span></div>
              </template>
              <template #body="slotProps">
                <Calendar
                  class="w-full"
                  v-model="slotProps.data.from_month"
                  autocomplete="on"
                  dateFormat="mm/yy"
                  :showIcon="true"
                  view="month"
                  placeholder="mm/yyyy"
                  :disabledDates="arrDisabled"
                />
              </template>
            </Column>

            <Column
              field="start_date"
              header="Mức đóng"
              headerStyle="text-align:center;width:170px;height:50px"
              bodyStyle=" width:170px;"
              class="align-items-center justify-content-center"
            >
              <template #body="slotProps">
                <InputNumber
                  v-model="slotProps.data.ins_premium"
                  class="w-full"
                
                />
              </template>
            </Column>

            <Column
              field="admission_place"
              header="Bảo hiểm xã hội"
              headerStyle="text-align:center;width:15rem;height:50px"
              bodyStyle="text-align:center;width:15rem;"
              class="align-items-center justify-content-center text-center"
            >
              <template #body="slotProps">
                <InputNumber
                  spellcheck="false"
                  :useGrouping="false"
                  class="w-4rem"
                  inputClass="format-center"
                  :max="100"
                  suffix=" %"
                  v-model="slotProps.data.social_ins_percent"
                  @update:modelValue="onchangeInsurance(slotProps.data, 1)"
                  v-tooltip.top="'Mức đóng'"
                />
                <InputNumber
                
                v-tooltip.top="'Công ty đóng'"
                  spellcheck="false"
                  :max="100"
                  suffix=" %"
                  class="w-4rem ml-2"
                  inputClass="format-center"
                  :useGrouping="false"
                  v-model="slotProps.data.social_ins_company"
                  @update:modelValue="onchangeInsurance(slotProps.data, 1)"
                />
                <InputNumber
                v-tooltip.top="'Nhân viên đóng'"
                  spellcheck="false"
                  class="w-4rem ml-2"
                  inputClass="format-center d-design-disabled"
                  :max="100"
                  suffix=" %"
                  :useGrouping="false"
                 
                  v-model="slotProps.data.social_ins_employee"
                />
              </template>
            </Column>
            <Column
              field="transfer_place"
              header="Bảo hiểm TNLĐ - BNN"
              headerStyle="text-align:center;width:15rem ;height:50px"
              bodyStyle="text-align:center ;width:15rem;"
              class="align-items-center justify-content-center text-center"
            >
              <template #body="slotProps">
                <InputNumber
                  spellcheck="false"
                  :useGrouping="false"
                  class="w-4rem"
                  :max="100"
                  suffix=" %"
                  placeholder="%"      v-tooltip.top="'Mức đóng'"
                  v-model="slotProps.data.accident_ins_percent"
                />
                <InputNumber
                  spellcheck="false"
                  :max="100"            v-tooltip.top="'Công ty đóng'"
                  suffix=" %"
                  class="w-4rem ml-2 format-center"
                  placeholder="CTY"
                  :useGrouping="false"
                  v-model="slotProps.data.accident_ins_company"
                  @update:modelValue="onchangeInsurance(slotProps.data, 2)"
                />
                <InputNumber   v-tooltip.top="'Nhân viên đóng'"
                  spellcheck="false"
                  class="w-4rem ml-2 "
                  :max="100"    inputClass="format-center d-design-disabled"
                  placeholder="NLĐ"
                  suffix=" %"
                  :useGrouping="false"
                   
                  v-model="slotProps.data.accident_ins_employee"
                  @update:modelValue="onchangeInsurance(slotProps.data, 2)"
                />
              </template>
            </Column>

            <Column
              field="transfer_place"
              header="Bảo hiểm y tế"
              headerStyle="text-align:center;width:15rem ;height:50px"
              bodyStyle="text-align:center ;width:15rem;"
              class="align-items-center justify-content-center text-center"
            >
              <template #body="slotProps">
                <InputNumber
                  spellcheck="false"
                  class="w-4rem"
                  :max="100" 
                  suffix=" %"     v-tooltip.top="'Mức đóng'"
                  inputClass="format-center"
                  v-model="slotProps.data.health_ins_percent"
                  @update:modelValue="onchangeInsurance(slotProps.data, 3)"
                />
                <InputNumber
                  spellcheck="false"
                  class="w-4rem ml-2"
                  :max="100"
                  suffix=" %"            v-tooltip.top="'Công ty đóng'"
                  inputClass="format-center"
                  v-model="slotProps.data.health_ins_company"
                  @update:modelValue="onchangeInsurance(slotProps.data, 3)"
                />
                <InputNumber
                  spellcheck="false"
                  :max="100"
                  class="w-4rem ml-2"
                  suffix=" %"   v-tooltip.top="'Nhân viên đóng'"
                  inputClass="format-center d-design-disabled" 
                   
                  v-model="slotProps.data.health_ins_employee"
                />
              </template>
            </Column>
            <Column
              field="transfer_place"
              header="Bảo hiểm thất nghiệp" 
              headerStyle="text-align:center;width:15rem ;height:50px"
              bodyStyle="text-align:center ;width:15rem;"
              class="align-items-center justify-content-center text-center"
            >
              <template #body="slotProps">
                <InputNumber
                  spellcheck="false"
                  class="w-4rem"     v-tooltip.top="'Mức đóng'"
                  suffix=" %"
                  inputClass="format-center"
                  v-model="slotProps.data.unemployment_ins_percent"
                  @update:modelValue="onchangeInsurance(slotProps.data, 4)"
                />
                <InputNumber
                  spellcheck="false"
                  class="w-4rem ml-2"
                  suffix=" %"
                  inputClass="format-center"            v-tooltip.top="'Công ty đóng'"
                  v-model="slotProps.data.unemployment_ins_company"
                  @update:modelValue="onchangeInsurance(slotProps.data, 4)"
                />
                <InputNumber
                  spellcheck="false"
                  class="w-4rem ml-2"
                  suffix=" %"    inputClass="format-center d-design-disabled"
                  v-tooltip.top="'Nhân viên đóng'"
                   
                  v-model="slotProps.data.unemployment_ins_employee"
                />
              </template>
            </Column>
            <Column
              header=""
              headerStyle="text-align:center;width:50px"
              bodyStyle="text-align:center;width:50px"
              class="align-items-center justify-content-center text-center"
            >
              <template #body="slotProps">
                <a
                  @click="delRow_Item(slotProps.data )"
                  class="hover cursor-pointer"
                >
                  <i class="pi pi-times-circle" style="font-size: 18px"></i>
                </a>
              </template>
            </Column>
          </DataTable>
        </div>
        <div class="col-12 style-vb-3 p-0 py-2 align-items-center flex">
          <Checkbox v-model="ins_checked" :binary="true" />
<div class="pl-1">
          <span> Tự động tính toán lại bảo hiểm từ tháng hiện tại</span></div>
        </div>
        <div class="col-12 style-vb-3 p-0 py-3">
          <InputText spellcheck="false" class="w-4rem  " style="    background-color: white;
    color: black;
    opacity: 1;" disabled placeholder="CTY" />
          <InputText
            spellcheck="false"
            class="w-4rem ml-2"
            disabled
            placeholder="NLĐ"
          />
          <span class="font-italic">
            Các ô màu xám là số % người lao động chi trả</span
          >
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
  </div>
</template>
<style scoped>
.hover:hover {
  cursor: pointer;
  color: #2196f3 !important;
}
.check-scroll {
  max-height: 45rem;
  overflow: scroll;
}

@media only screen and (max-height: 768px) {
  .style-vb-1 {
    font-size: large !important;
    padding: 8px !important;
  }

  .style-vb-2 {
    font-size: small !important;
  }

  .style-vb-3 {
    padding: 8px !important;
  }

  .check-scroll {
    max-height: 30rem;
    overflow: scroll;
  }
}

@media only screen and (max-height: 678px) {
  .style-vb-5 {
    display: none;
  }

  .check-scroll {
    max-height: 20rem;
    overflow: scroll;
  }
}
</style>