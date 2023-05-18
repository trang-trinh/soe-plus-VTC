<script setup>
import moment from "moment";
import { onMounted, inject, ref } from "vue";
import { useToast } from "vue-toastification";
import { encr } from "../../../../util/function";
const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios");
const cryoptojs = inject("cryptojs");
const toast = useToast();
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const basedomainURL = baseURL;

//Get arguments
const props = defineProps({
  headerDialog: String,
  displayDialog: Boolean,
  closeDialog: Function,
  isAdd: Boolean,
  isView: Boolean,
  profile: Object,
  initData: Function,
});
const isAdd = ref(props.isAdd);
const display = ref(props.displayDialog);
const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);
const statuss = ref([
  { value: 1, text: "Trả" },
  { value: 2, text: "Sửa" },
  { value: 3, text: "Chốt" },
  { value: 4, text: "Xin cấp" },
  { value: 5, text: "Gộp" },
  { value: 6, text: "Người lao động giữ sổ" },
]);
const hinhthucs = ref([
  { value: 1, text: "Bao tăng" },
  { value: 2, text: "Báo giảm" },
]);
const dictionarys = ref([]);
const insurance = ref({});
const insurance_pays = ref([]);
const insurance_resolves = ref([]);

//function
const opstatus = ref();
const toggleStatus = (event) => {
  opstatus.value.toggle(event);
};
const submitted = ref(false);
const saveModel = () => {
  submitted.value = true;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });

  if (!insurance.value.insurance_id || !insurance.value.insurance_code) {
    swal.fire({
      title: "Thông báo!",
      text: "Vui lòng nhập đầy đủ thông tin trường bôi đỏ!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  if (insurance_pays.value.length >= 2) {
    let count_duplicate = 0;
    for (let i = 0; i < insurance_pays.value.length - 1; i++) {
      for (let j = i + 1; j < insurance_pays.value.length; j++) {
        if (
          !isEmpty(insurance_pays.value[i].start_date) &&
          !isEmpty(insurance_pays.value[i].end_date) &&
          !isEmpty(insurance_pays.value[j].start_date) &&
          !isEmpty(insurance_pays.value[j].end_date) &&
          isMonth(
            insurance_pays.value[i],
            insurance_pays.value[j]
          )
        ) {
          count_duplicate++;
        }
      }
      if (count_duplicate > 0) {
        swal.fire({
          title: "Thông báo!",
          text: "Vui lòng nhập tháng đóng bảo hiểm không được trùng nhau!",
          icon: "error",
          confirmButtonText: "OK",
        });
        return;
      }
    }
  }
  var obj = JSON.parse(JSON.stringify(insurance.value));
  var pays = JSON.parse(JSON.stringify(insurance_pays.value));
  var resolves = JSON.parse(JSON.stringify(insurance_resolves.value));

  let formData = new FormData();
  formData.append("isAdd", isAdd.value);
  formData.append("insurance", JSON.stringify(obj));
  formData.append("insurance_pay", JSON.stringify(pays));
  formData.append("insurance_resolve", JSON.stringify(resolves));
  axios
    .put(
      baseURL + "/api/hrm_profile/update_profile_insurance",
      formData,
      config
    )
    .then((response) => {
      if (response.data.err === "1") {
        swal.fire({
          title: "Thông báo!",
          text: response.data.ms,
          icon: "error",
          confirmButtonText: "OK",
        });
        return;
      }
      swal.close();
      toast.success("Cập nhật thành công!");
      props.closeDialog();
    })
    .catch((error) => {
      swal.close();
      if (error && error.status === 401) {
        swal.fire({
          title: "Thông báo!",
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
        return;
      } else {
        swal.fire({
          title: "Thông báo!",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
        return;
      }
    });
  if (submitted.value) submitted.value = true;
};
const model = ref();
const addRow = (type) => {
  //relative
  if (type == 1) {
    model.value ={
    start_date: new Date(), 
    end_date: null,
    organization_name: null, 
    title_name: null,
    coef_salary: null,
    coef_allowance: null,
    payment_form: null,
    total_payment: null,
    company_payment: null,
    member_payment: null,
  }
  }
  if (type == 2) {
    let obj = {
      type_mode: null,
      payment_form: null,
      type_mode: null,
      completed_date: null,
      received_money_date: null,
      money: null,
    };
    insurance_resolves.value.push(obj);
  }
};
const saveRowData = ()=>{
  if(model.value.start_date == null){
    swal.fire({
      title: "Thông báo!",
      text: "Vui lòng chọn tháng bắt đầu!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  insurance_pays.value.push(model.value);
  model.value = null;
}
const deleteRow = (idx, type) => {
  if (type == 1) {
    insurance_pays.value.splice(idx, 1);
  }
  if (type == 2) {
    insurance_resolves.value.splice(idx, 1);
  }
};
//init
const initDictionary = () => {
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_insurance_dictionary",
            par: [{ par: "user_id", va: store.getters.user.user_id }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      if (response != null && response.data != null) {
        var data = response.data.data;
        if (data != null) {
          let tbs = JSON.parse(data);
          dictionarys.value = tbs;
        }
      }
    });
};
const initData = (rf) => {
  if (rf) {
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
  }
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_profile_insurance_edit",
            par: [{ par: "profile_id", va: props.profile.profile_id }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      var data = response.data.data;
      if (data != null) {
        var tbs = JSON.parse(data);
        if (tbs[0] != null && tbs[0].length > 0) {
          insurance.value = tbs[0][0];
        } else {
          insurance.value = {};
        }
        if(insurance.value.insurance_id == null){
          isAdd.value = true;
        }
        insurance.value.profile_id = props.profile.profile_id;
        //get child
        debugger
        if (tbs[1] != null && tbs[1].length > 0) {
          insurance_pays.value = tbs[1];
          insurance_pays.value.forEach((item) => {
            if (item.start_date != null) {
              item.start_date = new Date(item.start_date);
            }
            if (item.end_date != null) {
              item.end_date = new Date(item.end_date);
            }
          });
        } else {
          insurance_pays.value = [];
        }

        if (tbs[2] != null && tbs[2].length > 0) {
          insurance_resolves.value = tbs[2];
          insurance_resolves.value.forEach((item) => {
            if (item.received_file_date != null) {
              item.received_file_date = new Date(item.received_file_date);
            }
            if (item.completed_date != null) {
              item.completed_date = new Date(item.completed_date);
            }
            if (item.received_money_date != null) {
              item.received_money_date = new Date(item.received_money_date);
            }
          });
        } else {
          insurance_resolves.value = [];
        }
      }
      swal.close();
    })
    .catch((error) => {
      swal.close();
      if (error && error.status === 401) {
        swal.fire({
          title: "Thông báo!",
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
        return;
      } else {
        swal.fire({
          title: "Thông báo!",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
        return;
      }
    });
};
//check month  date
function isMonth(data1, data2) {
  let start1 = new Date(data1.start_date);
  let end1 = new Date(data1.end_date);
  let start2 = new Date(data2.start_date);
  let end2 = new Date(data2.end_date);
  return (start1 < end2 && end2< end1)
  || (end1 > start2 && end1 < end2)
  || (start1> start2 && end1< end2)
  || (start1< start2 && end1> end2)
    ? true
    : false;
}
function isEmpty(val) {
  return val === undefined || val == null || val.length <= 0 ? true : false;
} 
onMounted(() => {
  if (props.displayDialog) {
    initDictionary();
    initData(true);
  }
});
</script>
<template>
  <Dialog
    :header="props.headerDialog"
    v-model:visible="display"
    :style="{ width: '60vw' }"
    :maximizable="true"
    :closable="true"
    style="z-index: 9000"
  >
    <form @submit.prevent="" name="submitform">
      <div class="grid formgrid m-2">
        <div class="col-12 md:col-12">
          <div class="form-group">
            <h3 class="m-0">1. Thông tin chung</h3>
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Số sổ bảo hiểm <span class="redsao">(*)</span></label>
            <InputText
              spellcheck="false"
              class="ip36"
              v-model="insurance.insurance_id"
              maxLength="50"
            />
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Số thẻ BHYT <span class="redsao">(*)</span></label>
            <InputText
              spellcheck="false"
              class="ip36"
              v-model="insurance.insurance_code"
              maxLength="50"
            />
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Trạng thái</label>
            <Dropdown
              class="ip36"
              v-model="insurance.status"
              :options="statuss"
              optionLabel="text"
              optionValue="value"
              placeholder="Chọn trạng thái"
              :showClear="true"
            />
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Pháp nhân đóng</label>
            <Dropdown
              class="ip36"
              v-model="insurance.organization_payment"
              :options="dictionarys[0]"
              optionLabel="organization_name"
              optionValue="organization_name"
              :editable="true"
              placeholder="Chọn pháp nhân"
            />
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Mã tỉnh cấp</label>
            <Dropdown
              class="ip36"
              v-model="insurance.insurance_province_id"
              :options="dictionarys[1]"
              optionLabel="insurance_province_name"
              optionValue="insurance_province_id"
              placeholder="Mã tỉnh"
              :showClear="true"
            />
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Nơi đăng ký</label>
            <Dropdown
              class="ip36"
              v-model="insurance.hospital_name"
              :options="dictionarys[2]"
              optionLabel="hospital_name"
              optionValue="hospital_name"
              :editable="true"
              placeholder="Chọn nơi đăng ký"
            />
          </div>
        </div>
        <div class="col-12 md:col-12">
          <div class="form-group">
            <div class="flex justify-content-between">
              <div>
                <h3 class="m-0">2. Quá trình đóng bảo hiểm</h3>
              </div>
              <div>
                <a
                  @click="
                    addRow(1);
                    $event.stopPropagation();
                  "
                  class="hover"
                  v-tooltip.top="'Thêm mới'"
                >
                  <i
                    class="pi pi-plus-circle"
                    data-v-62364173=""
                    style="font-size: 18px"
                  ></i>
                </a>
              </div>
            </div>
          </div>
        </div>
        <div v-if="model != null" class="grid formgrid m-0">
          <div class="col-12 md:col-12">
            <div class="form-group">
              <label>Thời gian đóng bảo hiểm:</label>
            </div>
          </div>
          <div class="col-3 md:col-3">
            <div class="form-group">
              <label>Từ tháng, năm <span class="redsao">(*)</span></label>
              <Calendar
                :showIcon="false"
                view="month"
                dateFormat="mm/yy"
                class="ip36"
                placeholder="mm/yyyy"
                v-model="model.start_date"
              />
            </div>
          </div>
          <div class="col-3 md:col-3">
            <div class="form-group">
              <label>Đến tháng, năm</label>
              <Calendar
                :showIcon="false"
                view="month"
                dateFormat="mm/yy"
                class="ip36"
                placeholder="mm/yyyy"
                v-model="model.end_date"
              />
            </div>
          </div>
          <div class="col-6 md:col-6">
            <div class="form-group">
              <label>Bậc lương</label>
              <InputNumber
               v-model="model.salary"
                inputId="minmax"
                :min="0"
                class="ip36"
              />
            </div>
          </div>
          <div class="col-6 md:col-6">
            <div class="form-group">
              <label>Đơn vị đóng</label>
              <Dropdown
                class="ip36"
                v-model="model.organization_name"
                :options="dictionarys[4]"
                optionLabel="organization_name"
                optionValue="organization_name"
                :editable="true"
                placeholder="Chọn đơn vị"
              />
            </div>
          </div>
          <div class="col-6 md:col-6">
            <div class="form-group">
              <label>Hệ số lương</label>
              <InputNumber
                spellcheck="false"
                class="ip36"
                :min="0"
                v-model="model.coef_salary"
                maxLength="50"
              />
            </div>
          </div>
          <div class="col-6 md:col-6">
            <div class="form-group">
              <label>Chức danh</label>
              <Dropdown
                class="ip36"
                v-model="model.title_name"
                :options="dictionarys[5]"
                optionLabel="title_name"
                optionValue="title_name"
                :editable="true"
                placeholder="Chọn chức danh"
              />
            </div>
          </div>
          <div class="col-6 md:col-6">
            <div class="form-group">
              <label>Hệ số phụ cấp</label>
              <InputNumber
               v-model="model.coef_allowance"
                inputId="minmax"
                :min="0"
                class="ip36"
              />
            </div>
          </div>
          <div class="col-6 md:col-6">
            <div class="form-group">
              <label>Hình thức đóng</label>
              <Dropdown
                :options="hinhthucs"
                v-model="model.payment_form"
                optionLabel="text"
                optionValue="text"
                placeholder="Chọn hình thức"
                class="ip36"
              />
            </div>
          </div>
          <div class="col-6 md:col-6">
            <div class="form-group">
              <label>Mức đóng bảo hiểm</label>
              <InputNumber
                  spellcheck="false"
                  mode="decimal"
                  class="ip36 text-right input-money"
                  v-model="model.total_payment"
                  maxLength="250"
                />
            </div>
          </div>
          <div class="col-6 md:col-6">
            <div class="form-group">
              <label>Công ty đóng</label>
              <InputNumber
                  spellcheck="false"
                  mode="decimal"
                  class="ip36 text-right input-money"
                  v-model="model.company_payment"
                  maxLength="250"
                />
            </div>
          </div>
          <div class="col-6 md:col-6">
            <div class="form-group">
              <label>Nhân sự đóng</label>
              <InputNumber
                  spellcheck="false"
                  mode="decimal"
                  class="ip36 text-right input-money"
                  v-model="model.member_payment"
                  maxLength="250"
                />
            </div>
          </div>
          <div class="col-12 md:col-12 justify-content-end flex pb-3">
            <Button 
              label="Cập nhật"
              icon="pi pi-check"
              @click="saveRowData()"              
            />
          </div>
        </div>
        <div class="col-12 md:col-12">
          <DataTable
            :value="insurance_pays"
            :scrollable="true"
            :lazy="true"
            :rowHover="true"
            :showGridlines="true"
            scrollDirection="both"
          >
            <Column
              header=""
              headerStyle="text-align:center;width:50px"
              bodyStyle="text-align:center;width:50px"
              class="align-items-center justify-content-center text-center"
            >
              <template #body="slotProps">
                <a
                  @click="deleteRow(slotProps.index, 1)"
                  class="hover"
                  v-tooltip.top="'Xóa'"
                >
                  <i class="pi pi-times-circle" style="font-size: 18px"></i>
                </a>
              </template>
            </Column>
            <Column
              field="start_date"
              header="Từ tháng"
              headerStyle="text-align:center;width:120px;height:50px"
              bodyStyle="text-align:center;width:120px;"
              class="align-items-center justify-content-center text-center"
            >
              <template #body="slotProps">
                <Calendar
                  v-model="slotProps.data.start_date"
                  :showIcon="false"
                  view="month"
                  dateFormat="mm/yy"
                  class="ip36"
                  placeholder="mm/yyyy"
                />
              </template>
            </Column>
            <Column
              field="end_date"
              header="Đến tháng"
              headerStyle="text-align:center;width:120px;height:50px"
              bodyStyle="text-align:center;width:120px;"
              class="align-items-center justify-content-center text-center"
            >
              <template #body="slotProps">
                <Calendar
                  v-model="slotProps.data.end_date"
                  :showIcon="false"
                  view="month"
                  dateFormat="mm/yy"
                  class="ip36"
                  placeholder="mm/yyyy"
                />
              </template>
            </Column>
            <Column
              field="salary"
              header="Bậc lương"
              headerStyle="text-align:center;width:120px;height:50px"
              bodyStyle="text-align:center;width:120px;"
              class="align-items-center justify-content-center text-center"
            >
              <template #body="slotProps">
                <InputNumber
                  v-model="slotProps.data.salary"
                  inputId="minmax"
                  :min="0"
                  class="ip36"
                  />                
              </template>
            </Column>
            <Column
              field="organization_name"
              header="Đơn vị"
              headerStyle="text-align:center;width:150px;height:50px"
              bodyStyle="text-align:center;width:150px;"
              class="align-items-center justify-content-center text-center"
            >
              <template #body="slotProps">
                <div class="form-group m-0">
                  <Dropdown
                    :options="dictionarys[4]"
                    v-model="slotProps.data.organization_name"
                    optionLabel="organization_name"
                    optionValue="organization_name"
                    placeholder="Chọn đơn vị đóng"
                    :editable="true"
                    class="ip36"
                    :style="{
                      whiteSpace: 'nowrap',
                      overflow: 'hidden',
                      textOverflow: 'ellipsis',
                    }"
                  />
                </div>
              </template>
            </Column>
            <Column
              field="salary"
              header="Bậc lương"
              headerStyle="text-align:center;width:120px;height:50px"
              bodyStyle="text-align:center;width:120px;"
              class="align-items-center justify-content-center text-center"
            >
              <template #body="slotProps">
                <InputNumber
                  v-model="slotProps.data.coef_salary"
                  inputId="minmax"
                  :min="0"
                  class="ip36"
                  />                
              </template>
            </Column>
            <Column
              field="title_name"
              header="Chức danh"
              headerStyle="text-align:center;width:150px;height:50px"
              bodyStyle="text-align:center;width:150px;"
              class="align-items-center justify-content-center text-center"
            >
              <template #body="slotProps">
                <div class="form-group m-0">
                  <Dropdown
                    :options="dictionarys[5]"
                    v-model="slotProps.data.title_name"
                    optionLabel="title_name"
                    optionValue="title_name"
                    placeholder="Chọn chức danh"
                    :editable="true"
                    class="ip36"
                    :style="{
                      whiteSpace: 'nowrap',
                      overflow: 'hidden',
                      textOverflow: 'ellipsis',
                    }"
                  />
                </div>
              </template>
            </Column>
            <Column
              field="coef_allowance"
              header="Hệ số phụ cấp"
              headerStyle="text-align:center;width:120px;height:50px"
              bodyStyle="text-align:center;width:120px;"
              class="align-items-center justify-content-center text-center"
            >
              <template #body="slotProps">
                <InputNumber
                  v-model="slotProps.data.coef_allowance"
                  inputId="minmax"
                  :min="0"
                  class="ip36"
                  />                
              </template>
            </Column>
            <Column
              field="payment_form"
              header="Hình thức đóng"
              headerStyle="text-align:center;width:150px;height:50px"
              bodyStyle="text-align:center;width:150px;"
              class="align-items-center justify-content-center text-center"
            >
              <template #body="slotProps">
                <div class="form-group m-0">
                  <Dropdown
                    :options="hinhthucs"
                    v-model="slotProps.data.payment_form"
                    optionLabel="text"
                    optionValue="text"
                    placeholder="Chọn hình thức"
                    class="ip36"
                    :style="{
                      whiteSpace: 'nowrap',
                      overflow: 'hidden',
                      textOverflow: 'ellipsis',
                    }"
                  />
                </div>
              </template>
            </Column>
            <Column
              field="total_payment"
              header="Mức đóng"
              headerStyle="text-align:center;width:150px;height:50px"
              bodyStyle="text-align:center;width:150px;"
              class="align-items-center justify-content-center text-center"
            >
              <template #body="slotProps">
                <InputNumber
                  spellcheck="false"
                  mode="decimal"
                  class="ip36 text-right input-money"
                  v-model="slotProps.data.total_payment"
                  maxLength="250"
                />
              </template>
            </Column>
            <Column
              field="payment_form"
              header="Công ty đóng"
              headerStyle="text-align:center;width:150px;height:50px"
              bodyStyle="text-align:center;width:150px;"
              class="align-items-center justify-content-center text-center"
            >
              <template #body="slotProps">
                <div class="form-group m-0">
                  <InputNumber
                  mode="decimal"
                  spellcheck="false"
                  class="ip36 text-right input-money"
                  v-model="slotProps.data.organization_payment"
                  maxLength="250"
                />
                </div>
              </template>
            </Column>
            <Column
              field="reason"
              header="Nhân sự đóng"
              headerStyle="text-align:center;width:150px;height:50px"
              bodyStyle="text-align:center;width:150px;"
              class="align-items-center justify-content-center text-center"
            >
              <template #body="slotProps">
                <InputNumber
                  mode="decimal"
                  spellcheck="false"
                  class="ip36 text-right input-money"
                  v-model="slotProps.data.member_payment"
                  maxLength="250"
                />
              </template>
            </Column>
          </DataTable>
        </div>
        <!-- <div class="col-12 md:col-12">
          <DataTable
            :value="insurance_pays"
            :scrollable="true"
            :lazy="true"
            :rowHover="true"
            :showGridlines="true"
            scrollDirection="both"
          >
            <Column
              header=""
              headerStyle="text-align:center;width:50px"
              bodyStyle="text-align:center;width:50px"
              class="align-items-center justify-content-center text-center"
            >
              <template #body="slotProps">
                <a
                  @click="deleteRow(slotProps.index, 1)"
                  class="hover"
                  v-tooltip.top="'Xóa'"
                >
                  <i class="pi pi-times-circle" style="font-size: 18px"></i>
                </a>
              </template>
            </Column>
            <Column
              field="start_date"
              header="Từ tháng, năm"
              headerStyle="text-align:center;width:120px;height:50px"
              bodyStyle="text-align:center;width:120px;"
              class="align-items-center justify-content-center text-center"
            >
              <template #body="slotProps">
                <Calendar
                  v-model="slotProps.data.start_date"
                  :showIcon="false"
                  view="month"
                  dateFormat="mm/yy"
                  class="ip36"
                  placeholder="mm/yyyy"
                />
              </template>
            </Column>
            <Column
              field="payment_form"
              header="Hình thức"
              headerStyle="text-align:center;width:150px;height:50px"
              bodyStyle="text-align:center;width:150px;"
              class="align-items-center justify-content-center text-center"
            >
              <template #body="slotProps">
                <div class="form-group m-0">
                  <Dropdown
                    :options="hinhthucs"
                    v-model="slotProps.data.payment_form"
                    optionLabel="text"
                    optionValue="text"
                    placeholder="Chọn hình thức"
                    class="ip36"
                  />
                </div>
              </template>
            </Column>
            <Column
              field="reason"
              header="Lý do"
              headerStyle="text-align:center;width:150px;height:50px"
              bodyStyle="text-align:center;width:150px;"
              class="align-items-center justify-content-center text-center"
            >
              <template #body="slotProps">
                <InputText
                  spellcheck="false"
                  class="ip36"
                  v-model="slotProps.data.reason"
                  maxLength="250"
                />
              </template>
            </Column>
            <Column
              field="payment_form"
              header="Công ty đóng"
              headerStyle="text-align:center;width:150px;height:50px"
              bodyStyle="text-align:center;width:150px;"
              class="align-items-center justify-content-center text-center"
            >
              <template #body="slotProps">
                <div class="form-group m-0">
                  <Dropdown
                    :options="dictionarys[0]"
                    v-model="slotProps.data.organization_payment"
                    optionLabel="organization_name"
                    optionValue="organization_name"
                    placeholder="Chọn pháp nhân"
                    class="ip36"
                    :style="{
                      whiteSpace: 'nowrap',
                      overflow: 'hidden',
                      textOverflow: 'ellipsis',
                    }"
                  />
                </div>
              </template>
            </Column>
            <Column
              field="total_payment"
              header="Mức đóng"
              headerStyle="text-align:center;width:150px;height:50px"
              bodyStyle="text-align:center;width:150px;"
              class="align-items-center justify-content-center text-center"
            >
              <template #body="slotProps">
                <InputText
                  spellcheck="false"
                  class="ip36"
                  v-model="slotProps.data.total_payment"
                  maxLength="250"
                />
              </template>
            </Column>
            <Column
              field="reason"
              header="Công ty đóng"
              headerStyle="text-align:center;width:150px;height:50px"
              bodyStyle="text-align:center;width:150px;"
              class="align-items-center justify-content-center text-center"
            >
              <template #body="slotProps">
                <InputText
                  spellcheck="false"
                  class="ip36"
                  v-model="slotProps.data.company_payment"
                  maxLength="250"
                />
              </template>
            </Column>
            <Column
              field="reason"
              header="NLĐ đóng"
              headerStyle="text-align:center;width:150px;height:50px"
              bodyStyle="text-align:center;width:150px;"
              class="align-items-center justify-content-center text-center"
            >
              <template #body="slotProps">
                <InputText
                  spellcheck="false"
                  class="ip36"
                  v-model="slotProps.data.member_payment"
                  maxLength="250"
                />
              </template>
            </Column>
          </DataTable>
        </div> -->
        <div class="col-12 md:col-12">
          <div class="form-group">
            <div class="flex justify-content-between">
              <div>
                <h3 class="m-0">3. Lịch sử giải quyết chế độ</h3>
              </div>
              <div>
                <a
                  @click="
                    addRow(2);
                    $event.stopPropagation();
                  "
                  class="hover"
                  v-tooltip.top="'Thêm mới'"
                >
                  <i
                    class="pi pi-plus-circle"
                    data-v-62364173=""
                    style="font-size: 18px"
                  ></i>
                </a>
              </div>
            </div>
          </div>
        </div>
        <div class="col-12 md:col-12">
          <DataTable
            :value="insurance_resolves"
            :scrollable="true"
            :lazy="true"
            :rowHover="true"
            :showGridlines="true"
            scrollDirection="both"
          >
            <Column
              header=""
              headerStyle="text-align:center;width:50px"
              bodyStyle="text-align:center;width:50px"
              class="align-items-center justify-content-center text-center"
            >
              <template #body="slotProps">
                <a
                  @click="deleteRow(slotProps.index, 2)"
                  class="hover"
                  v-tooltip.top="'Xóa'"
                >
                  <i class="pi pi-times-circle" style="font-size: 18px"></i>
                </a>
              </template>
            </Column>
            <Column
              field="payment_form"
              header="Loại chế độ"
              headerStyle="text-align:center;width:150px;height:50px"
              bodyStyle="text-align:center;width:150px;"
              class="align-items-center justify-content-center text-center"
            >
              <template #body="slotProps">
                <div class="form-group m-0">
                  <Dropdown
                    :options="dictionarys[3]"
                    v-model="slotProps.data.type_mode"
                    optionLabel="insurance_type_mode_name"
                    optionValue="insurance_type_mode_name"
                    placeholder="Chọn loại chế độ"
                    class="ip36"
                  />
                </div>
              </template>
            </Column>
            <Column
              field="received_file_date"
              header="Ngày nhận hồ sơ"
              headerStyle="text-align:center;width:150px;height:50px"
              bodyStyle="text-align:center;width:150px;"
              class="align-items-center justify-content-center text-center"
            >
              <template #body="slotProps">
                <Calendar
                  v-model="slotProps.data.received_file_date"
                  :showIcon="false"
                  class="ip36"
                  placeholder="dd/mm/yyyy"
                />
              </template>
            </Column>
            <Column
              field="completed_date"
              header="Ngày hoàn thiện thủ tục"
              headerStyle="text-align:center;width:160px;height:50px"
              bodyStyle="text-align:center;width:160px;"
              class="align-items-center justify-content-center text-center"
            >
              <template #body="slotProps">
                <Calendar
                  v-model="slotProps.data.completed_date"
                  :showIcon="false"
                  class="ip36"
                  placeholder="dd/mm/yyyy"
                />
              </template>
            </Column>
            <Column
              field="received_money_date"
              header="Ngày nhận tiền BH trả"
              headerStyle="text-align:center;width:150px;height:50px"
              bodyStyle="text-align:center;width:150px;"
              class="align-items-center justify-content-center text-center"
            >
              <template #body="slotProps">
                <Calendar
                  v-model="slotProps.data.received_money_date"
                  :showIcon="false"
                  class="ip36"
                  placeholder="dd/mm/yyyy"
                />
              </template>
            </Column>
            <Column
              field="total_payment"
              header="Số tiền"
              headerStyle="text-align:center;width:150px;height:50px"
              bodyStyle="text-align:center;width:150px;"
              class="align-items-center justify-content-center text-center"
            >
              <template #body="slotProps">
                <InputText
                  spellcheck="false"
                  class="ip36"
                  v-model="slotProps.data.total_payment"
                  maxLength="250"
                />
              </template>
            </Column>
          </DataTable>
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="props.closeDialog()"
        class="p-button-text"
      />
      <Button label="Lưu" icon="pi pi-check" @click="saveModel()" />
    </template>
  </Dialog>
</template>
<style scoped>
@import url(../../profile/component/stylehrm.css);
.p-overlaypanel {
  z-index: 99999;
}
</style>
<style lang="scss" scoped>
::v-deep(.p-datatable) {
  table {
    border-collapse: collapse;
    min-width: 100%;
    table-layout: fixed;
  }

  .p-datatable-thead .justify-content-center .p-column-header-content {
    justify-content: center !important;
  }

  .p-datatable-thead .justify-content-left .p-column-header-content {
    justify-content: left !important;
  }

  .p-datatable-thead .justify-content-right .p-column-header-content {
    justify-content: right !important;
  }
}
::v-deep(.disable-header) {
  table thead {
    display: none;
  }
}
::v-deep(.form-group) {
  .p-multiselect .p-multiselect-label,
  .p-dropdown .p-dropdown-label {
    height: 100%;
    display: flex;
    align-items: center;
  }
}
::v-deep(.padding-0) {
  .p-accordion-content {
    padding: 0 !important;
  }
}
::v-deep(.empty-full) {
  .p-datatable-emptymessage td {
    width: 100% !important;
  }
}
::v-deep(.border-none) {
  .p-accordion-header a {
    border: none !important;
  }
  .p-accordion-content {
    border: none !important;
  }
  .p-datatable-table tr th,
  .p-datatable-table tr td {
    border: none !important;
  }
}
::v-deep(.selectbutton-custom) {
  .p-button.p-highlight {
    // color: #ffffff;
    // background: #64748b;
    // border: 1px solid #64748b;
    color: #000;
    background: #d3e3f8;
    border: 1px solid #d3e3f8;
  }
}
::v-deep(.border-radius) {
  img {
    border-radius: 5px;
  }
}
::v-deep(.header-padding-y-0) {
  .p-accordion-header-link {
    padding-top: 0;
    padding-bottom: 0;
  }
}
</style>
