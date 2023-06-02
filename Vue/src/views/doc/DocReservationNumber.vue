<script setup>
import { ref, defineProps, onMounted, inject } from "vue";
import { useToast } from "vue-toastification";
import TreeSelectCustom from "../../components/doc/TreeSelectCustom.vue";
import moment from "moment";
import { encr } from "../../util/function";
const swal = inject("$swal");
const cryoptojs = inject("cryptojs");
const toast = useToast();
const basedomainURL = fileURL;
const axios = inject("axios"); // inject axios
const store = inject("store");
const config = {
    headers: { Authorization: `Bearer ${store.getters.token}` },
};
const props = defineProps({
 
});
const options = ref({
    organization_name: store.getters.user.organization_name,
    organization_id: store.getters.user.organization_id
})
// Load Category
const treeorgs = ref({});
const raw_organizations = ref([]);
const loadOrgs = () => {
  
  axios
    .post(
      baseURL + "/api/DocProc/CallProc",
      {str: 
        encr(JSON.stringify(
          {
        proc: "doc_organization_list",
        par: [{ par: "user_id", va: store.getters.user.user_key }],
      }
      ),
        SecretKey, cryoptojs)
        .toString()
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data[0].length > 0) {
        raw_organizations.value = data[0];
        let obj = renderTreeDV(
          data[0],
          "organization_id",
          "organization_name",
          "đơn vị"
        );
        treeorgs.value = obj.arrtreeChils;
        is_loaded_org.value = true;
        // treeorgs.value = data[0];
      }
    })
    .catch((error) => { });
};
const renderTreeDV = (data, id, name, title) => {
  let arrChils = [];
  let arrtreeChils = [];
  data
    .filter((x) => !data.find(y => y.organization_id === x.parent_id))
    .forEach((m, i) => {
      m.IsOrder = i + 1;
      let om = { key: m[id], data: m };
      const rechildren = (mm, pid) => {
        let dts = data.filter((x) => x.parent_id == pid);
        if (dts.length > 0) {
          if (!mm.children) mm.children = [];
          dts.forEach((em) => {
            let om1 = { key: em[id], data: em };
            rechildren(om1, em[id]);
            mm.children.push(om1);
          });
        }
      };
      rechildren(om, m[id]);
      arrChils.push(om);
      //
      om = { key: m[id], data: m[id], label: m[name] };
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
  return { arrChils: arrChils, arrtreeChils: arrtreeChils };
};
const isFirst = ref(true);
const is_loaded_department = ref(false);
const is_loaded_org = ref(false);
const category = ref({});
const loadCategorys = () => {
  axios
    .post(
      baseURL
      //baseUrl
      + "/api/DocProc/CallProc",
      {str: 
        encr(JSON.stringify(
          {
        proc: "doc_master_category_list",
        par: [
          { par: "organization_id", va: store.getters.user.organization_id },
          { par: "user_id", va: store.getters.user.user_id }
        ],
      }
      ),
        SecretKey, cryoptojs)
        .toString()
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (isFirst.value) isFirst.value = false;
      category.value.issue_places = data[0];

      category.value.groups = data[1];
      category.value.org_groups = data[1];

      category.value.org_departments = data[2];
      let obj = renderTreeDV(
        category.value.org_departments,
        "organization_id",
        "organization_name",
        "phòng ban"
      );
      category.value.departments = obj.arrtreeChils;
      category.value.urgency = data[3];
      category.value.security = data[4];
      category.value.fields = data[5];
      category.value.send_ways = data[6];
      category.value.receive_places = data[7];

      category.value.dispatch_books = data[8];
      category.value.org_dispatch_books = data[8];

      category.value.positions = data[9];
      category.value.signers = data[10];
      category.value.email = data[11];
      category.value.email_groups = data[12];
      options.value.loading = false;
      is_loaded_department.value = true;
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;
      addLog({
        title: "Lỗi Console loadData",
        controller: "SQLView.vue",
        logcontent: error.message,
        loai: 2,
      });
      if (error && error.status === 401) {
        swal.fire({
          title: "Thông báo",
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
// Defined variable
const reservation_item = ref({
  is_type_receive: false,
  is_type_send: true,
  is_type_internal: false,
  is_manual: false,
  reservation_code: null,
  is_private: false,
  nav_type: 2,
  organization_id: options.value.organization_id
})
const config_number = ref({
  number: {value: null, separator: '/'},
  year: {value: null, separator: '/'},
  doc_group_id: {value: '', separator: '/'},
  department_id: {value: '', separator: '/'},
  organization_id: {value: '', separator: ''}
})
const refreshConfigNumber = () => {
  config_number.value = {
    number: {value: null, separator: '/'},
  year: {value: null, separator: '/'},
  doc_group_id: {value: '', separator: '/'},
  department_id: {value: '', separator: '/'},
  organization_id: {value: '', separator: ''}
  };
}
const refreshReservationItem = () =>{
  reservation_item.value = {
    is_type_receive: false,
  is_type_send: true,
  is_type_internal: false,
  is_manual: false,
  reservation_code: null,
  is_private: false,
  nav_type: 2,
  organization_id: options.value.organization_id
  }
}
const onChangeTypeDoc = (type) => {
  switch(type){
    case 1:
      if(!reservation_item.value.is_type_receive){
        reservation_item.value.is_type_send = true;
      }
      else{
        reservation_item.value.nav_type = 1;
        reservation_item.value.is_type_send = false;
        reservation_item.value.is_type_internal = false;
      }
      break;
  case 2:
      if(!reservation_item.value.is_type_send){
        reservation_item.value.is_type_send = true;
      }
      else{
        reservation_item.value.nav_type = 2;
        reservation_item.value.is_type_receive = false;
        reservation_item.value.is_type_internal = false;
      }
      break;
  case 3:
      if(!reservation_item.value.is_type_internal){
        reservation_item.value.is_type_send = true;
      }
      else{
        reservation_item.value.nav_type = 3;
        reservation_item.value.is_type_receive = false;
        reservation_item.value.is_type_send = false;
      }
      break;
  }
  validateDocCode();
}
const generateDocCode = () => {
  reservation_item.value.is_manual = false;
  reservation_item.value.reservation_code = '';
  for(var key in config_number.value){
    let true_value = '';
    switch(key){
      case 'number':
        if(config_number.value[key].value !== null && config_number.value[key].value < 10){
          true_value = config_number.value[key].value.toString().slice(-1);
          true_value = '0' + config_number.value[key].value;
        }
        else{
          true_value = config_number.value[key].value;
        }
      break;
      case 'doc_group_id':
        true_value = category.value.groups.find(x=>x.doc_group_id == config_number.value[key].value).doc_group_code;
      break;
      case 'department_id':
        let de_id = Object.keys(config_number.value[key].value)[0];
        let de = category.value.org_departments.find(x=>x.organization_id == de_id);
        if(de)
          true_value = de.department_doc_code;
      break;
      case 'organization_id':
        let org_id = Object.keys(config_number.value[key].value)[0];
        let org = raw_organizations.value.find(x=>x.organization_id == org_id);
        if(org){
            true_value = org.short_name;
        }
      break;
      default:
        true_value = config_number.value[key].value ?? '';
      break;
    }
    reservation_item.value.reservation_code +=
       true_value + config_number.value[key].separator;
       validateDocCode();
  }
}
var is_can_check = true;
const validateDocCode = () => {
  var tout = setTimeout(() => {
    axios
    .post(baseURL + "/api/DocMain/Validate_Reservation_Code", reservation_item.value, config)
    .then((response) => {
      if (response.data.err != "1") {
        if(response.data.is_same){
          reservation_item.value.is_same = true;
        }
        else{
          reservation_item.value.is_same = false;
        }
        is_can_check = true;
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
      swal.fire({
        title: "Error!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
  }, 100);
  if(!is_can_check){
    clearTimeout(tout);
  }
}
// save
const saveReservationNumber = () => {
  
  if(reservation_item.value.is_same){
    swal.fire({
        title: "Error!",
        text: "Số đã tồn tại! Vui lòng chọn số khác",
        icon: "error",
        confirmButtonText: "OK",
      });
      return false;
  }
  if(!reservation_item.value.reservation_code){
    swal.fire({
        title: "Error!",
        text: "Chưa nhập số!",
        icon: "error",
        confirmButtonText: "OK",
      });
      return false
  }
  reservation_item.value.num = config_number.value.number.value;
  if(reservation_item.value.is_manual){
    reservation_item.value.num = reservation_item.value.reservation_code.replace(/\D+/g, ' ').trim().split(' ').map(e => parseInt(e))[0];
  }
  reservation_item.value.year = config_number.value.year.value;
  reservation_item.value.organization_config_id =  Object.keys(config_number.value.organization_id.value)[0];
  reservation_item.value.doc_group_id = config_number.value.doc_group_id.value;
  reservation_item.value.department_id = Object.keys(config_number.value.department_id.value)[0];
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios
  .post(
        baseURL + "/api/DocMain/Save_Reservation_Code",
        reservation_item.value,
        config
      )
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        toast.success("Giữ số văn bản thành công!");
        refreshConfigNumber();
        refreshReservationItem();
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
      swal.fire({
        title: "Error!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
}
// View
const displayViewNumber = ref(false);
const headerViewNumber = ref();
const tab_view_number = ref([
  {nav_type: 1, type_name: "Văn bản đến", data: []},
  {nav_type: 2, type_name: "Văn bản đi", data: []},
  {nav_type: 3, type_name: "Văn bản nội bộ", data: []},
])
const openModalViewReservationCode = () => {
  displayViewNumber.value = true;
  headerViewNumber.value = "Danh sách số văn bản đã giữ";
  loadReservationCode();
}
const loadReservationCode = () => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios
    .post(
      baseURL
      //baseUrl
      + "/api/DocProc/CallProc",
      {str: 
        encr(JSON.stringify(
          {
        proc: "doc_reservation_number_list",
        par: [
          { par: "organization_id", va: store.getters.user.organization_id },
          { par: "user_id", va: store.getters.user.user_id }
        ],
      }
      ),
        SecretKey, cryoptojs)
        .toString()
      },
      config
    )
    .then((response) => {
      swal.close();
      let data = JSON.parse(response.data.data);
    
      if (data[0]) {
        tab_view_number.value[0].data = data[0].filter(x=>x.nav_type === 1);
        tab_view_number.value[1].data = data[0].filter(x=>x.nav_type === 2);
        tab_view_number.value[2].data = data[0].filter(x=>x.nav_type === 3);
      }
    })
    .catch((error) => {
      swal.close();
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;
      addLog({
        title: "Lỗi Console loadData",
        controller: "SQLView.vue",
        logcontent: error.message,
        loai: 2,
      });
      if (error && error.status === 401) {
        swal.close();
        swal.fire({
          title: "Thông báo",
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const deleteReservationCode = (data) => {
  let del_data_id = data.filter(x=>x.checked).map(x=>x.reservation_number_id);
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn bỏ số đã giữ này không!",
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
          .delete(baseURL + "/api/DocMain/Delete_Reservation_Code", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: del_data_id,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Bỏ số đã chọn thành công!");
              loadReservationCode();
            } else {
              console.log(response.data.ms);
              swal.fire({
                title: "Thông báo",
                text: "Xảy ra lỗi khi bỏ số",
                icon: "error",
                confirmButtonText: "OK",
              });
            }
          })
          .catch((error) => {
            swal.close();
            if (error.status === 401) {
              swal.fire({
                title: "Thông báo",
                text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
                icon: "error",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
}
// remove tree select
const removeTreeSelect = (var_name, value) => {
  var_name[value] = null;
}
onMounted(() => {
    loadCategorys();
    loadOrgs();
  return {
  };
});
</script>
<template>
<div class="r-container">
    <div class=" w-full  p-4 style-vb-1  text-center text-3xl">
          BẢNG THIẾT LẬP GIỮ SỐ VĂN BẢN
        </div>
        <div class="w-full p-0 style-vb-2 text-center text-xl">
          {{ options.organization_name }}
        </div>
    <div class="w-full style-vb-3 p-4 text-center format-center">
        <div class="col-2 flex justify-content-end align-items-center">
            <div class="pr-2">Sổ văn bản đến:</div>
            <InputSwitch @change="onChangeTypeDoc(1)" v-model="reservation_item.is_type_receive" class="w-4rem lck-checked" />
        </div>
        <div class="col-2 flex align-items-center format-center">
            <div class="pr-2">Văn bản đi:</div>
            <InputSwitch @change="onChangeTypeDoc(2)" v-model="reservation_item.is_type_send" class="w-4rem lck-checked" />
        </div>
        <div class="col-2 flex align-items-center">
            <div class="pr-2">Văn bản nội bộ:</div>
            <InputSwitch @change="onChangeTypeDoc(3)" v-model="reservation_item.is_type_internal" class="w-4rem lck-checked" />
        </div>
    </div>
    <div class="c-container">
        <div class="form-box">
            <form>
                <div class="grid formgrid m-2">
                  <div class="field col-12 md:col-12 flex">
                    <h3 class="mt-0"><b>Cấu hình tự động</b></h3>
                  </div>
                    <div class="field col-12 md:col-12 flex">
                        <div class="col-12 md:col-12 m-0 p-0 flex">
                            <label class="col-4 text-left flex" style="align-items:center;">
                                Số văn bản
                            </label>
                            <div class="p-inputgroup ol-8 p-0 ip36">
                                <InputNumber v-model="config_number.number.value" :useGrouping="false" />
                                <span class="p-inputgroup-addon" style="width: 60px">
                                    <InputText v-model="config_number.number.separator" class="text-center" />
                                </span>
                            </div>
                        </div>
                    </div>
                    <div class="field col-12 md:col-12 flex">
                        <div class="col-12 md:col-12 m-0 p-0 flex">
                            <label class="col-4 text-left flex" style="align-items:center;">
                                Năm
                            </label>
                            <div class="p-inputgroup ol-8 p-0 ip36">
                                <InputNumber v-model="config_number.year.value" :useGrouping="false" />
                                <span class="p-inputgroup-addon" style="width: 60px">
                                    <InputText v-model="config_number.year.separator" class="text-center" />
                                </span>
                            </div>
                        </div>
                    </div>
                    <div class="field col-12 md:col-12 flex">
                        <div class="col-12 md:col-12 m-0 p-0 flex">
                            <label class="col-4 text-left flex" style="align-items:center;">
                                Nhóm văn bản
                            </label>
                            <div class="p-inputgroup ol-8 p-0 ip36">
                                <Dropdown :showClear="true" v-model="config_number.doc_group_id.value" spellcheck="false" :options="category.groups" optionLabel="doc_group_name"
                                    optionValue="doc_group_id" :editable="false" :filter="true" placeholder="Chọn nhóm" />
                                <span class="p-inputgroup-addon" style="width: 60px">
                                    <InputText v-model="config_number.doc_group_id.separator" class="text-center" />
                                </span>
                            </div>
                        </div>
                    </div>
                    <div class="field col-12 md:col-12 flex">
                        <div class="col-12 md:col-12 m-0 p-0 flex">
                            <label class="col-4 text-left flex" style="align-items:center;">
                                Mã phòng ban
                            </label>
                            <div class="p-inputgroup ol-8 p-0">
                              <TreeSelectCustom class="department-tree" :removeTree="removeTreeSelect" :model="config_number.department_id" property-name="value" :options="category.departments"
                                placeholder="Chọn phòng ban">
                              </TreeSelectCustom>
                                <span class="p-inputgroup-addon" style="width: 60px">
                                    <InputText v-model="config_number.department_id.separator" class="text-center" />
                                </span>
                            </div>
                        </div>
                    </div>
                    <div class="field col-12 md:col-12 flex">
                        <div class="col-12 md:col-12 m-0 p-0 flex">
                            <label class="col-4 text-left flex" style="align-items:center;">
                                Mã đơn vị
                            </label>
                            <div class="p-inputgroup ol-8 p-0 ip36">
                                <TreeSelect  v-model="config_number.organization_id.value" v-if="is_loaded_org" :options="treeorgs" :showClear="true" :max-height="200"
                                    placeholder="Chọn đơn vị">
                                </TreeSelect>
                                <span class="p-inputgroup-addon" style="width: 60px">
                                    <InputText  v-model="config_number.organization_id.separator" class="text-center" />
                                </span>
                            </div>
                        </div>
                    </div>
                    <div class="field col-12 md:col-12 flex">
                    <div class="col-12 md:col-12 m-0 p-0">
                      <Button label="Làm mới" icon="pi pi-refresh" @click="refreshConfigNumber()" class="p-button-outlined mr-2" />
                      <Button label="Tạo" @click="generateDocCode()"/>
                    </div>
                </div>
                </div>
            </form>
        </div>
        <div class="form-config ml-4">
            <div class="grid formgrid m-2">
              <div class="field col-12 md:col-12 flex">
                    <div class="col-12 md:col-12 m-0 p-0 flex">
                        <div class="col-4 flex align-items-center">
                          <Button @click="openModalViewReservationCode" label="Danh sách số đã giữ"
                        />
                        </div>
                    </div>
                </div>
              
                <div class="field col-12 md:col-12 flex mt-5">
                    <div class="col-12 md:col-12 m-0 p-0 flex">
                        <div class="col-4 flex align-items-center">
                            <div class="pr-3">Nhập thủ công</div>
                            <InputSwitch v-model="reservation_item.is_manual" class="w-4rem lck-checked" />
                        </div>
                    </div>
                </div>
                <div class="field col-12 md:col-12 flex mt-5">
                        <div class="col-12 md:col-12 m-0 p-0 flex">
                            <label class="col-2 text-left flex" style="align-items:center;">
                                Số khởi tạo
                            </label>
                            <div class="col-8 p-0 ip-36">
                                <InputText @keyup="validateDocCode" style="min-width: 16rem" :disabled="!reservation_item.is_manual" v-model="reservation_item.reservation_code" :useGrouping="false" />
                            </div>
                        </div>
                </div>
                <div v-if="reservation_item.is_same" class="field col-12 md:col-12 flex">
                    <b style="color: red">Số đã tồn tại!</b>
                </div>
                <div class="field col-12 md:col-12 flex mt-5">
                    <div class="col-12 md:col-12 m-0 p-0 flex">
                        <div class="col-4 flex align-items-center">
                            <div class="pr-3">Chỉ mình tôi</div>
                            <InputSwitch v-model="reservation_item.is_private" class="w-4rem lck-checked" />
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <div class="col-12 style-vb-3 py-4 text-center format-center">
          <Button icon="pi pi-check" label="Lưu" @click="saveReservationNumber()"/>
    </div>
    </div>
    <Dialog :modal="true" :header="headerViewNumber" v-model:visible="displayViewNumber" :autoZIndex="true"
    :style="{ width: '70vw' }">
    <TabView >
      <TabPanel v-for="tab in tab_view_number" :key="tab.nav_type" :header="tab.type_name">
        <Button @click="deleteReservationCode(tab.data)" v-if="tab.data.find(x=>x.checked)" label="Xoá" icon="pi pi-trash" class="abs-btn mr-2 p-button-danger" />
        <div>
          <form>
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 m-0 p-0">
            <div style="justify-content: space-between;" v-for="item in tab.data" :key="tab.reservation_number_id" class="code-item  flex">
             <div class="flex">
                <Checkbox :binary="true" class="mr-3" v-model="item.checked" />
                <h3 class="m-0">{{ item.reservation_code }}</h3>
             </div>
             <Chip class="mr-3" v-if="item.is_private" v-bind:label="'Chỉ mình tôi'" style="background-color:#2196F3;color: #fff; border-radius: 5px; font-size: 11px;" />
            </div>
          </div>
        </div>
      </div>
    </form>
        </div>
      </TabPanel>
    </TabView>
  </Dialog>
</template>
<style lang="scss" scoped>
.abs-btn{
  position: absolute;
    right: 1.5rem;
    top: 5.4rem;
}
.code-item{
  padding-bottom: 10px;
  margin-bottom: 10px;
    border-bottom: 1px solid rgb(242,242,242);
}
.r-container{
    margin: auto;
    overflow-y: auto;
    max-height: calc(100vh - 4rem);
}
    .c-container{
        display: flex;
        align-items: center;
        justify-content: center;
    }
    .form-box{
        margin: 0.5rem;
    border: 1px solid #dfe7ef;
    padding: 2rem;
    border-radius: 10px;
    min-height: max-content;
    width: 40rem;
    }
    .form-config{
        width: 40rem;
    }
    .p-inputgroup-addon{
        padding: 2px;
    }
    ::v-deep(.p-treeselect .p-treeselect-label){
        text-overflow: ellipsis;
        max-width: 15rem;
    }
    ::v-deep(.department-tree.p-treeselect .p-treeselect-label){
      margin-top: 0.3rem;
      padding-right: 0;
    }
</style>
