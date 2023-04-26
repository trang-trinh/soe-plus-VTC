<script setup>
import { ref, inject, onMounted } from "vue";
import { useToast } from "vue-toastification";
import { required, maxLength, integer } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { encr, change_unsigned } from "../../../../util/function.js";
//import moment from "moment";
//import treeuser from "../../../../components/user/treeuser.vue";
const cryoptojs = inject("cryptojs");
const toast = useToast();
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const emitter = inject("emitter");
const basedomainURL = baseURL;
const baseUrlCheck = baseURL;
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const props = defineProps({
  key: Number,
  dataForm: Object,
  id: String,
  headerDialog: String,
  displayDialog: Boolean,
  listSettingForms: Object,
  closeDialogSetting: Function,
  listSettingForms: Object,
});
const cpnSettingForm = ref(0);
const forceRerenderFormSetting = () => {
  cpnSettingForm.value += 1;
};
function generateUUID() {
  // Public Domain/MIT
  var d = new Date().getTime(); //Timestamp
  var d2 =
    (typeof performance !== "undefined" &&
      performance.now &&
      performance.now() * 1000) ||
    0; //Time in microseconds since page-load or 0 if unsupported
  return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, function (c) {
    var r = Math.random() * 16; //random number between 0 and 16
    if (d > 0) {
      //Use timestamp until depleted
      r = (d + r) % 16 | 0;
      d = Math.floor(d / 16);
    } else {
      //Use microseconds since page-load if supported
      r = (d2 + r) % 16 | 0;
      d2 = Math.floor(d2 / 16);
    }
    return (c === "x" ? r : (r & 0x3) | 0x8).toString(16);
  });
}
const listSettingForms = ref(props.listSettingForms);
const listDropdownClass = ref([
  { value: "col-1", text: "col-1" },
  { value: "col-2", text: "col-2" },
  { value: "col-3", text: "col-3" },
  { value: "col-4", text: "col-4" },
  { value: "col-5", text: "col-5" },
  { value: "col-6", text: "col-6" },
  { value: "col-7", text: "col-7" },
  { value: "col-8", text: "col-8" },
  { value: "col-9", text: "col-9" },
  { value: "col-10", text: "col-10" },
  { value: "col-11", text: "col-11" },
  { value: "col-12", text: "col-12" },
])
const listDropdownAlign = ref([
  { value: "left", text: "Trái" },
  { value: "right", text: "Phải" },
  { value: "center", text: "Giữa" },
])
const listDropdownPermission = ref([
  { value: "0", text: "Tất cả" },
  { value: "1", text: "Người tạo nhập" },
  { value: "2", text: "Người duyệt nhập" },
  { value: "3", text: "Người xử lý nhập" },
  { value: "4", text: "Người xử lý và người tạo" },
  { value: "5", text: "Người xử lý vào người duyệt" },
])
const listTypeColumn = ref([
  { value: 'varchar', text: 'varchar', is_length: true },
  { value: 'nvarchar', text: 'nvarchar', is_length: true },
  { value: 'int', text: 'int', is_length: true },
  { value: 'float', text: 'float', is_length: true },
  { value: 'bit', text: 'bit', is_length: false },
  { value: 'textarea', text: 'textarea', is_length: true },
  { value: 'checkbox', text: 'checkbox', is_length: false },
  { value: 'radio', text: 'radio', is_length: false },
  { value: 'date', text: 'date', is_length: false },
  { value: 'datetime', text: 'datetime', is_length: false },
  { value: 'time', text: 'time', is_length: false },
  { value: 'file', text: 'file', is_length: false },
  { value: 'select', text: 'select', is_length: false },
  { value: 'email', text: 'email', is_length: true },
])
const selectedType = ref();
const listDataType = ref([
  {
    label: 'Chung',
    code: 0,
    items: [
      { label: 'Trường bình thường', value: 0 },
      { label: 'Trường tổng hợp (tính tổng)', value: 1 },
      { label: 'Trường ẩn tên', value: 2 },
      { label: 'Table', value: 3 },
      { label: 'Column', value: 4 },
      { label: 'Row', value: 5 },
    ]
  },
  {
    label: 'HR-Nghỉ phép',
    code: 1,
    items: [
      { label: 'Ngày', value: 6 },
      { label: 'Giờ', value: 7 },
      { label: 'Nghỉ phép?', value: 8 },
    ]
  },
])
const listSettingFormOf = ref([]);
const addSettingFormTeams = () => {
  let arr_form_d = { request_formd_id: generateUUID(), request_form_id: props.id, ten_truong: null, kieu_truong: null, is_length: null, is_order: listSettingForms.value.length + 1, is_required: 0, is_label: false, is_parent_id: null, is_type: 0, selectedType: { label: 'Trường bình thường', value: 0 }, is_class: null, tudien_id: null, text_key: null, value_key: null, is_width: null, text_align: null, is_permission: null, lv: 1, is_edit: true };
  listSettingForms.value.push(arr_form_d);

  listSettingForms.value.forEach((e, i) => {
    if (listSettingForms.value.length == 1) {
      e.isDisableDown = true;
      e.isDisableUp = true;
    } else if (i == 0 && listSettingForms.value.length > 1) {
      e.isDisableDown = false;
      e.isDisableUp = true;
    } else if ((i + 1 == listSettingForms.value.length) && listSettingForms.value.length > 1) {
      e.isDisableDown = true;
      e.isDisableUp = false;
    } else if (i > 0 && i < listSettingForms.value.length - 1) {
      e.isDisableDown = false;
      e.isDisableUp = false;
    }
  })
}
const MoveDownQT = (arr, m, idx_cr, type) => {
  if (idx_cr !== -1 && idx_cr < arr.length - 1) {
    let el = arr[idx_cr];
    arr[idx_cr] = arr[idx_cr + 1];
    arr[idx_cr + 1] = el;
  }
  if (type == 0) {
    listSettingForms.value.forEach((e, i) => {
      if (arr.length == 1) {
        e.isDisableDown = true;
        e.isDisableUp = true;
      } else if (i == 0 && listSettingForms.value.length > 1) {
        e.isDisableDown = false;
        e.isDisableUp = true;
      } else if ((i + 1 == listSettingForms.value.length) && listSettingForms.value.length > 1) {
        e.isDisableDown = true;
        e.isDisableUp = false;
      } else if (i > 0 && i < listSettingForms.value.length - 1) {
        e.isDisableDown = false;
        e.isDisableUp = false;
      }
    })
  } else {

  }
};
const MoveUpQT = (arr, m, idx_cr, type) => {
  if (idx_cr > 0) {
    let el = arr[idx_cr];
    arr[idx_cr] = arr[idx_cr - 1];
    arr[idx_cr - 1] = el;
  }
  if (type == 0) {
    listSettingForms.value.forEach((e, i) => {
      if (arr.length == 1) {
        e.isDisableDown = true;
        e.isDisableUp = true;
      } else if (i == 0 && listSettingForms.value.length > 1) {
        e.isDisableDown = false;
        e.isDisableUp = true;
      } else if ((i + 1 == listSettingForms.value.length) && listSettingForms.value.length > 1) {
        e.isDisableDown = true;
        e.isDisableUp = false;
      } else if (i > 0 && i < listSettingForms.value.length - 1) {
        e.isDisableDown = false;
        e.isDisableUp = false;
      }
    })
  }
};
const addChild = (model, idx) => {
  let arr1 = [...listSettingForms.value];
  listSettingFormOf.value = arr1;
  let arr_form_d = { request_formd_id: generateUUID(), request_form_id: props.id, ten_truong: null, kieu_truong: null, is_length: null, is_order: listSettingForms.value.length + 1, is_required: 0, is_label: false, is_parent_id: model.request_formd_id, is_type: 0, selectedType: { label: 'Trường bình thường', value: 0 }, is_class: null, tudien_id: null, text_key: null, value_key: null, is_width: null, text_align: null, is_permission: null, lv: model.lv + 1, is_edit: true, isDisableDown: true, isDisableUp: false };
  // listSettingForms.value.push(arr_form_d);

  let idx_cr = listSettingForms.value.findIndex(x => x.request_formd_id == model.request_formd_id);
  let idx_countChild = listSettingForms.value.filter(x => x.is_parent_id == model.request_formd_id).length;
  listSettingForms.value.splice(idx_cr + idx_countChild + 1, 0, arr_form_d);

  listSettingForms.value.forEach((e, i) => {
    if (arr.length == 1) {
      e.isDisableDown = true;
      e.isDisableUp = true;
    } else if (i == 0 && listSettingForms.value.length > 1) {
      e.isDisableDown = false;
      e.isDisableUp = true;
    } else if ((i + 1 == listSettingForms.value.length) && listSettingForms.value.length > 1) {
      e.isDisableDown = true;
      e.isDisableUp = false;
    } else if (i > 0 && i < listSettingForms.value.length - 1) {
      e.isDisableDown = false;
      e.isDisableUp = false;
    }
  })
}
const saveDataFormD = () => {
  if (listSettingForms.value.length == 0) {
    return;
  } else {
    listSettingForms.value.forEach((e) => {
      e.is_type = e.selectedType.value;
    })
  }
  let formData = new FormData();
  formData.append("request_ca_formd", JSON.stringify(listSettingForms.value));
  axios
    .post(
      baseURL +
      "/api/request_ca_form/add_request_ca_formd",
      formData,
      config,
    )
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật team sử dụng cho đề xuất thành công!");
        props.closeDialogSetting();
      }
    })
    .catch((res) => {
      swal.close();
      swal.fire({
        title: "Error!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
}
const ChangeData = (model, idx) => {
  let arr1 = [...listSettingForms.value];
  listSettingFormOf.value = arr1;
  let idx_cr = listSettingForms.value.findIndex(x => x.request_formd_id == model.request_formd_id);
  let idx_new = listSettingForms.value.findIndex(x => x.request_formd_id == model.is_parent_id);
  listSettingForms.value.splice(idx_cr, 1);
  if (idx_new == 0) {
    let idx_countChild = listSettingForms.value.filter(x => x.is_parent_id == model.is_parent_id).length;
    listSettingForms.value.splice(idx_new + idx_countChild, 0, model);
  } else {
    listSettingForms.value.splice(idx_new, 0, model);
  }
  listSettingForms.value.forEach((e, i) => {
    if (arr.length == 1) {
      e.isDisableDown = true;
      e.isDisableUp = true;
    } else if (i == 0 && listSettingForms.value.length > 1) {
      e.isDisableDown = false;
      e.isDisableUp = true;
    } else if ((i + 1 == listSettingForms.value.length) && listSettingForms.value.length > 1) {
      e.isDisableDown = true;
      e.isDisableUp = false;
    } else if (i > 0 && i < listSettingForms.value.length - 1) {
      e.isDisableDown = false;
      e.isDisableUp = false;
    }
  })
}
const ChangeRequired = (model) => {
  model.is_required = !model.is_required;
}
const ChangeDropdown = (data, event) => {
  data.selectedType = event.value;
}
const editSettingFormD = (model) => {
  model.is_edit = !model.is_edit;
}
onMounted(() => {
  //loadDataTeamUse(true);
  return {};
});
</script>
<template>
  <Dialog :header="props.headerDialog" v-model:visible="props.displayDialog" @update:visible="props.closeDialogSetting()"
    :style="{ width: '90vw' }" :closable="true" position="top" :modal="true">
    <form @submit.prevent="">
      <div class="grid formgrid m-0">
        <div class="field col-12 md:col-12 algn-items-center flex p-0">
          <div class="col-6 text-left flex p-0" style="align-items:center;">
            <span class="p-input-icon-left">
              <i class="pi pi-search" />
              <InputText type="text" spellcheck="false" placeholder="Tìm kiếm" style="min-width:10rem;" />
            </span>
          </div>
          <div class="col-6 text-left flex p-0" style="align-items:center;justify-content: end;">
            <span class="p-input-icon-left">
              <i class="pi pi-search" />
              <Button @click="addSettingFormTeams('Thêm')" label="Thêm" icon="pi pi-plus" class="mr-2" />
            </span>
          </div>
        </div>
        <div class="field col-12 md:col-12 algn-items-center p-0">
          <DataTable class="table-ca-request" :value="listSettingForms" :paginator="false" :scrollable="true"
            scrollDirection="both" scrollHeight="flex" :lazy="true" dataKey="request_formd_id" :rowHover="true"
            v-model:selection="selectedTeamDatas">
            <Column field="ten_truong" header="Trường"
              headerStyle="text-align:center;width:30rem;height:50px;border-left:none;border-right:none;"
              bodyStyle="text-align:center;height:50px;;width:30rem;border-left:none;border-right:none;position:relative"
              class="align-items-center justify-content-center text-center">>
              <template #body="data">
                <div :style="'padding-left:' + (data.data.lv == 1 ? '0' : data.data.lv * 10) + 'px'" style="width: 100%;">
                  <InputText type="text" style="" class="col-12 ip36" spellcheck="false" v-model="data.data.ten_truong"
                    :disabled="!data.data.is_edit" placeholder="Nhập tên trường..." />
                </div>
              </template>
            </Column>
            <Column field="is_parent_id" header="Thuộc"
              headerStyle="text-align:center;width:20rem;height:50px;border-left:none;border-right:none;"
              bodyStyle="text-align:center;height:50px;;width:20rem;border-left:none;border-right:none;position:relative"
              class="align-items-center justify-content-center text-center">
              <template #body="data">
                <Dropdown :options="listSettingFormOf" :disabled="!data.data.is_edit"
                  @Change="ChangeData(data.data, data.index)" style="" :filter="true" :showClear="true" :editable="false"
                  v-model="data.data.is_parent_id" optionLabel="ten_truong" optionValue="request_formd_id" placeholder=""
                  class="col-12 ip36">
                </Dropdown>
              </template>
            </Column>
            <Column field="" header="Chức năng"
              headerStyle="text-align:center;width:15rem;height:50px;border-left:none;border-right:none;"
              bodyStyle="text-align:center;height:50px;;width:15rem;border-left:none;border-right:none;position:relative"
              class="align-items-center justify-content-center text-center">
              <template #body="data">
                <span style="width: 100%;display: flex;align-items: center;justify-content: space-between;"
                  class="col-12 ip36">
                  <span>
                    <i style="font-size: 15px;" @click="addChild(data.data, data.index)"
                      class="pi pi-plus-circle hover-cursor" v-tooltip.top="'Thêm cấp con'"></i>
                  </span>
                  <span v-if="data.data.is_edit">
                    <i style="font-size: 15px;" @click="editSettingFormD(data.data)" class="pi pi-lock hover-cursor"
                      v-tooltip.top="'Khóa'"></i>
                  </span>
                  <span v-if="!data.data.is_edit">
                    <i style="font-size: 15px;" @click="editSettingFormD(data.data)" class="pi pi-pencil hover-cursor"
                      v-tooltip.top="'Sửa'"></i>
                  </span>
                  <span>
                    <i style="font-size: 15px;" class="pi pi-trash hover-cursor"
                      @click="Remove_FormSign(listSettingForms, data.data, data.index)" v-tooltip.top="'Xóa'"></i>
                  </span>
                  <span>
                    <i style="font-size: 15px;" :class="(data.data.isDisableDown ? 'colorDisable' : '')"
                      class="pi pi-angle-down hover-cursor"
                      @click="MoveDownQT(listSettingForms, data.data, data.index, 0)" v-tooltip.top="'Xuống'"
                      disabled></i>
                  </span>
                  <span>
                    <i style="font-size: 15px;" :class="(data.data.isDisableUp ? 'colorDisable' : '')"
                      class="pi pi-angle-up hover-cursor" @click="MoveUpQT(listSettingForms, data.data, data.index, 0)"
                      v-tooltip.top="'Lên'"></i>
                  </span>
                </span>
              </template>
            </Column>
            <Column field="is_label" header="Nhãn"
              headerStyle="text-align:center;width:5rem;height:50px;border-left:none;border-right:none;"
              bodyStyle="text-align:center;height:50px;;width:5rem;border-left:none;border-right:none;position:relative"
              class="align-items-center justify-content-center text-center">
              <template #body="data">
                <InputSwitch style="" v-model="data.data.is_label" />
              </template>
            </Column>
            <Column field="is_required" header="Required"
              headerStyle="text-align:center;width:10rem;height:50px;border-left:none;border-right:none;"
              bodyStyle="text-align:center;height:50px;;width:10rem;border-left:none;border-right:none;position:relative"
              class="align-items-center justify-content-center text-center">
              <template #body="data">
                <span v-if="!data.data.is_label" class="required-hover" @click="ChangeRequired(data.data)"
                  :style="(data.data.is_required ? 'color: red;' : '')" style="padding: 20px;">{{ data.data.is_required ?
                    "* REQUIRED" : "NOT REQUIRED" }}</span>
              </template>
            </Column>
            <Column field="kieu_truong" header="Kiểu"
              headerStyle="text-align:center;width:10rem;height:50px;border-left:none;border-right:none;"
              bodyStyle="text-align:center;height:50px;;width:10rem;border-left:none;border-right:none;position:relative"
              class="align-items-center justify-content-center text-center">
              <template #body="data">
                <Dropdown :options="listTypeColumn" :disabled="!data.data.is_edit" style="" :filter="true"
                  :showClear="true" :editable="false" v-model="data.data.kieu_truong" optionLabel="text"
                  optionValue="value" placeholder="" class="col-12 ip36">
                </Dropdown>
              </template>
            </Column>
            <Column field="is_length" header="Length"
              headerStyle="text-align:center;width:10rem;height:50px;border-left:none;border-right:none;"
              bodyStyle="text-align:center;height:50px;;width:10rem;border-left:none;border-right:none;position:relative"
              class="align-items-center justify-content-center text-center">
              <template #body="data">
                <InputText type="text" style="" :disabled="!data.data.is_edit" class="col-12 ip36" spellcheck="false"
                  v-model="data.data.is_length" placeholder="" />
              </template>
            </Column>
            <Column field="is_width" header="Width"
              headerStyle="text-align:center;width:10rem;height:50px;border-left:none;border-right:none;"
              bodyStyle="text-align:center;height:50px;;width:10rem;border-left:none;border-right:none;position:relative"
              class="align-items-center justify-content-center text-center">
              <template #body="data">
                <InputNumber v-if="!data.data.is_label" :disabled="!data.data.is_edit" style="" class="col-12 ip36"
                  spellcheck="false" v-model="data.data.is_width" placeholder="" />
              </template>
            </Column>
            <Column field="text_align" header="Align"
              headerStyle="text-align:center;width:10rem;height:50px;border-left:none;border-right:none;"
              bodyStyle="text-align:center;height:50px;;width:10rem;border-left:none;border-right:none;position:relative"
              class="align-items-center justify-content-center text-center">
              <template #body="data">
                <Dropdown v-if="!data.data.is_label" :disabled="!data.data.is_edit" :options="listDropdownAlign" style=""
                  :filter="true" :showClear="true" :editable="false" v-model="data.data.text_align" optionLabel="text"
                  optionValue="value" placeholder="" class="col-12 ip36">
                </Dropdown>
              </template>
            </Column>
            <Column field="is_class" header="Class"
              headerStyle="text-align:center;width:10rem;height:50px;border-left:none;border-right:none;"
              bodyStyle="text-align:center;height:50px;;width:10rem;border-left:none;border-right:none;position:relative"
              class="align-items-center justify-content-center text-center">
              <template #body="data">
                <Dropdown v-if="!data.data.is_label" :disabled="!data.data.is_edit" :options="listDropdownClass" style=""
                  :filter="true" :showClear="true" :editable="false" v-model="data.data.is_class" optionLabel="text"
                  optionValue="value" placeholder="" class="col-12 ip36">
                </Dropdown>
              </template>
            </Column>
            <Column field="is_type" header="Type"
              headerStyle="text-align:center;width:15rem;height:50px;border-left:none;border-right:none;"
              bodyStyle="text-align:center;height:50px;;width:15rem;border-left:none;border-right:none;position:relative"
              class="align-items-center justify-content-center text-center">
              <template #body="data">
                <Dropdown v-model="data.data.selectedType" :disabled="!data.data.is_edit"
                  @change="ChangeDropdown(data.data, $event)" :options="listDataType" optionLabel="label"
                  optionGroupLabel="label" optionGroupChildren="items" placeholder="" class="col-12 ip36">
                  <template #optiongroup="slotProps">
                    <div class="flex align-items-center">
                      <div>{{ slotProps.option.label }}</div>
                    </div>
                  </template>
                </Dropdown>
              </template>
            </Column>
            <Column field="is_permission" header="Quyền"
              headerStyle="text-align:center;width:10rem;height:50px;border-left:none;border-right:none;"
              bodyStyle="text-align:center;height:50px;;width:10rem;border-left:none;border-right:none;position:relative"
              class="align-items-center justify-content-center text-center">
              <template #body="data">
                <Dropdown v-if="!data.data.is_label" :disabled="!data.data.is_edit" :options="listDropdownPermission"
                  style="" :filter="true" :showClear="true" :editable="false" v-model="data.data.is_permission"
                  optionLabel="text" optionValue="value" placeholder="" class="col-12 ip36">
                </Dropdown>
              </template>
            </Column>
            <template #empty>
              <div class="align-items-center justify-content-center p-4 text-center m-auto" style="
                                  display: flex;
                                  flex-direction: column;
                                " v-if="listSettingForms.length == 0">
                <img src="../../../../assets/background/nodata.png" height="144" />
                <h3 class="m-1">Không có dữ liệu</h3>
              </div>
            </template>
          </DataTable>
        </div>
      </div>
    </form>
    <template #footer>
      <Button label="Hủy" icon="pi pi-times" @click="props.closeDialogSetting()" class="p-button-outlined" />
      <Button label="Lưu lại" icon="pi pi-check" @click="saveDataFormD()" autofocus />
    </template>
  </Dialog>
</template>
<style lang="scss" scoped>
.required-hover:hover {
  cursor: pointer;
  color: #2196f3 !important;
}

.hover-cursor:hover {
  cursor: pointer;
  color: #0d89ec !important;
}

.colorDisable {
  color: #DDDDDD !important;
}

::v-deep(.table-ca-request) {
  .p-datatable-emptymessage {
    justify-content: center;
  }
}
</style>