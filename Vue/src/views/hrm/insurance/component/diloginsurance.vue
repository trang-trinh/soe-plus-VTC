<script setup>
import { onMounted, inject, ref } from "vue";
import { useToast } from "vue-toastification";
import { encr, checkURL } from "../../../../util/function.js";
import { required, maxLength } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import moment from "moment";
const cryoptojs = inject("cryptojs");
const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios");
const toast = useToast();
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const basedomainURL = baseURL;
// const rules = {
//   insurance_id: {
//     required,
//     maxLength: maxLength(50),
//     $errors: [
//       {
//         $property: "insurance_id",
//         $validator: "required",
//         $message: "Số sổ bảo hiểm không được để trống!",
//       },
//     ],
//   },
//   insurance_code: {
//     required,
//     maxLength: maxLength(50),
//     $errors: [
//       {
//         $property: "insurance_code",
//         $validator: "required",
//         $message: "Số thẻ bảo hiểm không được để trống!",
//       },
//     ],
//   },
// };
//Get arguments
const props = defineProps({
  key: Number,
  headerDialog: String,
  displayDialog: Boolean,
  closeDialog: Function,
  isAdd: Boolean,
  isView: Boolean,
  model: Object,
  addRow: Function,
  deleteRow: Function,
  insurance_pays: Array,
  insurance_resolves: Array,
  statuss: Array,
  dictionarys: Array,
  hinhthucs: Array,
  initData: Function,
  datefilter: Date,
});
//const v$ = useVuelidate(rules, props.model);

//Declare dictionary
const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);
const display = ref(props.displayDialog);
//function
const model = ref();
const openAddRow = ()=>{
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
const changeProfile = (profile_id) => {
  axios
    .post(
      baseURL + "/api/hrm/callProc",
        {
          str: encr(
          JSON.stringify({
            proc: "hrm_insurance_get_user",
            par: [
              { par: "profile_id", va: profile_id },
          ],
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
        props.model.gender = data[0].gender;
        props.model.departmant_name = data[0].departmant_name;
        props.model.birthday = data[0].birthday? new Date(data[0].birthday) : null;
        props.model.identity_date_issue = data[0].identity_date_issue? new Date(data[0].identity_date_issue) : null;
        props.model.identity_papers_code = data[0].identity_papers_code;
        props.model.birthplace_origin =  data[0].birthplace_origin_name ?  data[0].birthplace_origin_name: data[0].birthplace_origin_last;
        props.model.identity_place_name = data[0].identity_place_name;
        props.model.place_register_permanent = data[0].place_register_permanent_last ? data[0].place_register_permanent_last:((data[0].place_register_permanent_first||'') + ' '+ (data[0].place_register_permanent_name||''));

      }
    })
    .catch((error) => {
      if (error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
      }
    });
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
  props.insurance_pays.push(model.value);
  model.value = null;
}
const submitted = ref(false);
const saveData = () => {
  submitted.value = true;
  props.model.profile_id = props.model.profile.profile_id;
  if (isEmpty(props.model.profile_id )) {
    swal.fire({
      title: "Thông báo!",
      text: "Nhân sự không được để trống!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  if (isEmpty(props.model.insurance_id)) {
    swal.fire({
      title: "Thông báo!",
      text: "Số sổ bảo hiểm không được để trống!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  if (isEmpty(props.model.insurance_code)) {
    swal.fire({
      title: "Thông báo!",
      text: "Số thẻ BHYT không được để trống!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  props.insurance_pays.forEach((item) => {
    item.is_duplicate = false;
  });
  if (props.insurance_pays.length >= 2) {
    let count_duplicate = 0;
    for (let i = 0; i < props.insurance_pays.length - 1; i++) {
      for (let j = i + 1; j < props.insurance_pays.length; j++) {
        if (
          !isEmpty(props.insurance_pays[i].start_date) &&
          !isEmpty(props.insurance_pays[i].end_date) &&
          !isEmpty(props.insurance_pays[j].start_date) &&
          !isEmpty(props.insurance_pays[j].end_date) &&
          isMonth(
            props.insurance_pays[i],
            props.insurance_pays[j]
          )
        ) {
          props.insurance_pays[j].is_duplicate = true;
          props.insurance_pays[i].is_duplicate = true;
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
  props.insurance_pays.forEach((item) => {
    if (!isEmpty(item.is_duplicate)) item.is_duplicate = null;
  });
  let formData = new FormData();

  formData.append("insurance", JSON.stringify(props.model));
  formData.append(
    "insurance_pay",
    JSON.stringify(
      props.insurance_pays.filter(
        (item) => !Object.values(item).every((o) => isEmpty(o))
      )
    )
  );
  formData.append(
    "insurance_resolve",
    JSON.stringify(
      props.insurance_resolves.filter(
        (item) => !Object.values(item).every((o) => isEmpty(o))
      )
    )
  );
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: props.isAdd == false ? "put" : "post",
    url:
      baseURL +
      `/api/insurance/${
        props.isAdd  == false ? "update_insurance" : "add_insurance"
      }`,
    data: formData,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  }).then((response) => {
    if (response.data.err === "0") {
      swal.close();
      toast.success("Cập nhật thành công!");
      props.closeDialog();
      props.initData(true);
    } else {
      swal.fire({
        title: "Thông báo!",
        text: "Đã có mã sổ này trong hệ thống rồi!",
        icon: "error",
        confirmButtonText: "OK",
      });
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
//init
onMounted(() => {

});
</script>
<template>
  <Dialog
    :header="props.headerDialog"
    v-model:visible="props.displayDialog"
    :style="{ width: '65vw', zIndex:100 }"
    :closable="true"
    :modal="true"
    :maximizable="true"
    :autoZIndex="true"
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
            <label>Nhân sự <span class="redsao">(*)</span></label>
            <!-- <InputText
              spellcheck="false"
              class="ip36"
              v-model="props.model.profile_user_name"
              maxLength="50"
            /> -->
            <Dropdown
              :options="dictionarys[6]"
              @change="changeProfile(props.model.profile.profile_id)"
              :filter="true"
              :showClear="true"
              :editable="false"
              optionLabel="profile_user_name"
              placeholder="Chọn nhân sự"
              v-model="props.model.profile"
              class="ip36"
              style="height: auto; min-height: 36px"
              :class="{
                'p-invalid': !props.model.profile && submitted,
              }"
            >
              <template #value="slotProps">
                <div class="mt-2" v-if="slotProps.value">
                  <Chip
                    :image="slotProps.value.avatar"
                    :label="slotProps.value.profile_user_name"
                    class="mr-2 mb-2 pl-0"
                  >
                    <div class="flex">
                      <div class="format-flex-center">
                        <Avatar
                          v-bind:label="
                            slotProps.value.avatar
                              ? ''
                              : (
                                  slotProps.value.profile_user_name ?? ''
                                ).substring(0, 1)
                          "
                          v-bind:image="
                            slotProps.value.avatar
                              ? basedomainURL + slotProps.value.avatar
                              : basedomainURL + '/Portals/Image/noimg.jpg'
                          "
                          :style="{
                            background:
                              bgColor[slotProps.value.is_order % 7],
                            color: '#ffffff',
                            width: '2rem',
                            height: '2rem',
                          }"
                          class="mr-2 text-avatar"
                          size="xlarge"
                          shape="circle"
                        />
                      </div>
                      <div class="format-flex-center">
                        <span>{{ slotProps.value.profile_user_name }}</span>
                      </div>
                    </div>
                  </Chip>
                </div>
                <span v-else> {{ slotProps.placeholder }} </span>
              </template>
              <template #option="slotProps">
                <div v-if="slotProps.option" class="flex">
                  <div class="format-center">
                    <Avatar
                      v-bind:label="
                        slotProps.option.avatar
                          ? ''
                          : slotProps.option.profile_user_name.substring(
                              0,
                              1
                            )
                      "
                      v-bind:image="
                        slotProps.option.avatar
                          ? basedomainURL + slotProps.option.avatar
                          : basedomainURL + '/Portals/Image/noimg.jpg'
                      "
                      style="
                        background-color: #2196f3;
                        color: #ffffff;
                        width: 3rem;
                        height: 3rem;
                        font-size: 1.4rem !important;
                      "
                      :style="{
                        background: bgColor[slotProps.option.is_order % 7],
                      }"
                      class="text-avatar"
                      size="xlarge"
                      shape="circle"
                    />
                  </div>
                  <div class="format-center text-left ml-3">
                    <div>
                      <div class="mb-1">
                        {{ slotProps.option.profile_user_name }}
                      </div>
                      <div class="description">
                        <div>
                          <span>{{ slotProps.option.profile_code }}</span
                          ><span v-if="slotProps.option.department_name">
                            | {{ slotProps.option.department_name }}</span
                          >
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
                <span v-else> Chưa có dữ liệu </span>
              </template>
            </Dropdown>
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Giới tính</label>
            <InputText
              spellcheck="false"
              class="ip36"
              v-model="props.model.gender"
              maxLength="50"
              disabled="true"
            />
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Đơn vị/ Phòng ban</label>
            <InputText
              spellcheck="false"
              class="ip36"
              v-model="props.model.departmant_name"
              maxLength="50"
              disabled="true"
            />
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Ngày sinh</label>
            <Calendar
              class="ip36"
              id="icon"
              v-model="props.model.birthday"
              :showIcon="true"
              disabled 
            />
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Số CMT/ Số CCCD</label>
            <InputText
              spellcheck="false"
              class="ip36"
              v-model="props.model.identity_papers_code"
              maxLength="50"
              disabled="true"
            />
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Nguyên quán</label>
            <InputText
              spellcheck="false"
              class="ip36"
              v-model="props.model.birthplace_origin"
              maxLength="50"
              disabled="true"
            />
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Ngày cấp</label>
            <Calendar
              class="ip36"
              id="icon"
              v-model="props.model.identity_date_issue"
              :showIcon="true"
              disabled 
            />
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Nơi cấp</label>
            <InputText
              spellcheck="false"
              class="ip36"
              v-model="props.model.identity_place_name"
              maxLength="50"
              disabled="true"
            />
          </div>
        </div>
        <div class="col-12 md:col-12">
          <div class="form-group">
            <label>Hộ khẩu thường trú</label>
            <InputText
              spellcheck="false"
              class="ip36"
              v-model="props.model.place_register_permanent"
              maxLength="50"
              disabled="true"
            />
          </div>
        </div>
        <div class="col-12 md:col-12" autofocus>
          <div class="form-group">
            <h3 class="m-0">2. Thông tin hồ sơ</h3>
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Số sổ bảo hiểm <span class="redsao">(*)</span></label>
            <InputText
              spellcheck="false"
              class="ip36"
              v-model="props.model.insurance_id"
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
              v-model="props.model.insurance_code"
              maxLength="50"
            />
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Trạng thái</label>
            <Dropdown
              class="ip36"
              v-model="props.model.status"
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
              v-model="props.model.organization_payment"
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
              v-model="props.model.insurance_province_id"
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
            <label>Nơi đăng ký khám chữa bệnh</label>
            <Dropdown
              autofocus
              class="ip36"
              v-model="props.model.hospital_name"
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
                <h3 class="m-0">3. Quá trình đóng</h3>
              </div>
              <div>
                <a
                  @click="
                    openAddRow();
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
                mode="decimal"
                locale="vi-VN"
                :minFractionDigits="0"
                :maxFractionDigits="2"                
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
                mode="decimal"
                locale="vi-VN"
                :minFractionDigits="0"
                :maxFractionDigits="2"
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
                mode="decimal"
                locale="vi-VN"
                :minFractionDigits="0"
                :maxFractionDigits="2"
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
              <label>Công ty đóng (số tiền)</label>
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
              <label>Nhân sự đóng (số tiền)</label>
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
            :value="props.insurance_pays"
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
                  mode="decimal"
                  locale="vi-VN"
                  :minFractionDigits="0"
                  :maxFractionDigits="2"
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
              field="coef_salary"
              header="Hệ số lương"
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
                  mode="decimal"
                  locale="vi-VN"
                  :minFractionDigits="0"
                  :maxFractionDigits="2"
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
                  mode="decimal"
                locale="vi-VN"
                :minFractionDigits="0"
                :maxFractionDigits="2"
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
        <div class="col-12 md:col-12 pt-3">
          <div class="form-group">
            <div class="flex justify-content-between">
              <div>
                <h3 class="m-0">4. Lịch sử giải quyết chế độ</h3>
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
            :value="props.insurance_resolves"
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
              headerStyle="text-align:center;width:180px;height:50px"
              bodyStyle="text-align:center;width:180px;"
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
              headerStyle="text-align:center;width:170px;height:50px"
              bodyStyle="text-align:center;width:170px;"
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
              headerStyle="text-align:center;width:200px;height:50px"
              bodyStyle="text-align:center;width:200px;"
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
              headerStyle="text-align:center;width:200px;height:50px"
              bodyStyle="text-align:center;width:200px;"
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
                <InputNumber
                  spellcheck="false"
                  mode="decimal"
                  class="ip36 text-right input-money"
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
        class="p-button-outlined"
      />
      <Button 
        label="Lưu"
        icon="pi pi-check"
        @click="saveData()"
        autofocus
      />
      <!-- <Button 
        v-if="props.datefilter == null"
        label="Lưu"
        icon="pi pi-check"
        @click="saveData()"
        autofocus
      /> -->
    </template>
  </Dialog>
</template>
    
    <style scoped>
    @import url(../../profile/component/stylehrm.css);

.ip33 {
  height: 33px !important;
}
.scroll-outer {
  visibility: hidden;
  margin: 0 1rem;
}
.scroll-inner,
.scroll-outer:hover,
.scroll-outer:focus {
  visibility: visible;
}
</style>
<style lang="scss" scoped>
::v-deep(.input-money) {
  .p-inputnumber-input {
    text-align:right !important;
  }
}
::v-deep(.p-datatable) {
  .p-datatable-emptymessage {
    height:50px;
  }
  .p-datatable-emptymessage td{
    display:none !important;
  }
}
</style>