<script setup>
import { onMounted, inject, ref } from "vue";
import { useToast } from "vue-toastification";
import { required, maxLength } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import moment from "moment";
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
const submitted = ref(false);
const saveData = () => {
  submitted.value = true;
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
          !isEmpty(props.insurance_pays[j].start_date) &&
          isMonth(
            props.insurance_pays[i].start_date,
            props.insurance_pays[j].start_date
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
          text: "Vui lòng nhập tháng đóng đóng bảo hiểm không được trùng nhau!",
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
      debugger
      props.displayDialog = false;
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
function isMonth(date1, date2) {
  let dt1 = new Date(date1);
  let dt2 = new Date(date2);
  return dt1.getMonth() == dt2.getMonth() &&
    dt1.getFullYear() == dt2.getFullYear()
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
    :style="{ width: '55vw' }"
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
            <label>Nơi đăng ký</label>
            <Dropdown
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
                <h3 class="m-0">2. Lịch sử đóng bảo hiểm</h3>
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
                  class="ip36 text-right"
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
                  class="ip36 text-right"
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
                  class="ip36 text-right"
                  v-model="slotProps.data.member_payment"
                  maxLength="250"
                />
              </template>
            </Column>
          </DataTable>
        </div>
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
                  class="ip36 text-right"
                  v-model="slotProps.data.total_payment"
                  maxLength="250"
                />
              </template>
            </Column>
          </DataTable>
        </div>
      </div>
    </form>
    <!-- <form>
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left p-0"
            >Số sổ bảo hiểm <span class="redsao">(*)</span></label
          >
          <InputText
            v-model="props.model.insurance_id"
            spellcheck="false"
            class="col-10 ip33"
          />
        </div>

        <div class="field col-12 md:col-12">
          <label class="col-2 text-left p-0">Trạng thái</label>
          <Dropdown
            class="col-10 ip33"
            v-model="props.model.status"
            :options="statuss"
            optionLabel="text"
            optionValue="value"
            placeholder="Trạng thái"
            :showClear="true"
          />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left p-0">Pháp nhân đóng</label>
          <Dropdown
            class="col-10 ip33"
            v-model="props.model.organization_payment"
            :options="props.dictionarys[0]"
            optionLabel="organization_name"
            optionValue="organization_name"
            :editable="true"
          />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left p-0"
            >Số thẻ BHYT <span class="redsao">(*)</span></label
          >
          <InputText
            v-model="props.model.insurance_code"
            spellcheck="false"
            class="col-10 ip33"
          />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left p-0">Mã tỉnh cấp</label>
          <Dropdown
            class="col-10 ip33"
            v-model="props.model.insurance_province_id"
            :options="props.dictionarys[1]"
            optionLabel="insurance_province_name"
            optionValue="insurance_province_id"
            placeholder="Mã tỉnh"
            :showClear="true"
          />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left p-0">Nơi đăng ký</label>
          <Dropdown
            class="col-10 ip33"
            v-model="props.model.hospital_name"
            :options="props.dictionarys[2]"
            optionLabel="hospital_name"
            optionValue="hospital_name"
            :editable="true"
          />
        </div>
        <div class="field col-12 md:col-12 flex m-0">
          <div class="col-10 text-left p-0">
            <h4>Lịch sử đóng bảo hiểm</h4>
          </div>
          <div class="col-2 p-0 format-center" style="justify-content:end">
            <a @click="addRow(1)" class="cursor-pointer" v-tooltip.top="'Thêm'">
              <i class="pi pi-plus-circle" style="font-size: 18px"></i>
            </a>
          </div>
        </div>
        <div style="overflow-x: scroll" class="scroll-outer">
          <div class="scroll-inner">
            <table
              class="table table-condensed table-hover tbpad table-child"
              style="table-layout: fixed"
            >
              <thead>
                <tr>
                  <th
                    class="text-center row-bc sticky"
                    style="width: 100px; left: 0px !important"
                  ></th>
                  <th class="text-center row-bc" style="width: 150px">
                    Từ tháng
                  </th>
                  <th class="text-center row-bc" style="width: 150px">
                    Hình thức
                  </th>
                  <th class="text-center row-bc" style="width: 150px">Lý do</th>
                  <th class="text-center row-bc" style="width: 200px">
                    Pháp nhân đóng
                  </th>
                  <th class="text-center row-bc" style="width: 150px">
                    Mức đóng
                  </th>
                  <th class="text-center row-bc" style="width: 150px">
                    Công ty đóng
                  </th>
                  <th class="text-center row-bc" style="width: 150px">
                    NLĐ đóng
                  </th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="(item, index) in props.insurance_pays" :key="index">
                  <td
                    class="sticky" align="center"
                    style="color: black;width: 100px;left: 0px !important;z-index: 100;  "
                  >
                    <a
                      @click="props.deleteRow(index, 1)"
                      class="hover"
                      v-tooltip.top="'Xóa'"
                    >
                      <i class="pi pi-times-circle" style="font-size: 18px"></i>
                    </a>
                  </td>
                  <td align="center">
                    <Calendar
                      v-model="item.start_date"
                      view="month"
                      dateFormat="mm/yy"
                      class="ip33"
                      style="width: 150px"
                      placeholder="Bắt đầu"
                      :class="{
                        'p-invalid': insurance_pays[index].is_duplicate,
                      }"
                    />
                  </td>
                  <td align="center">
                    <Dropdown
                      class="ip33"
                      v-model="item.payment_form"
                      :options="props.hinhthucs"
                      optionLabel="text"
                      optionValue="text"
                      style="width: 150px"
                      :showClear="true"
                    />
                  </td>
                  <td align="center">
                    <InputText
                      spellcheck="false"
                      class="ip33"
                      style="width: 150px"
                      v-model="item.reason"
                    />
                  </td>
                  <td align="center">
                    <Dropdown
                      class="ip33"
                      v-model="item.organization_payment"
                      :options="dictionarys[0]"
                      optionLabel="organization_name"
                      optionValue="organization_name"
                      :editable="true"
                      style="width: 200px"
                    />
                  </td>
                  <td align="center">
                    <InputNumber
                      class="ip33 p-0 input-money"
                      v-model="item.total_payment"
                      style="witdh: 150px"
                    />
                  </td>
                  <td align="center">
                    <InputNumber
                      class="ip33 p-0 input-money"
                      v-model="item.company_payment"
                      style="witdh: 150px"
                    />
                  </td>
                  <td align="center">
                    <InputNumber
                      class="ip33 p-0 input-money"
                      v-model="item.member_payment"
                      style="witdh: 150px"
                    />
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
        <div class="field col-12 md:col-12 flex m-0">
          <div class="col-10 text-left p-0">
            <h4>Lịch sử giải quyết chế độ</h4>
          </div>
          <div class="col-2 p-0 format-center" style="justify-content:end">
            <a @click="addRow(2)" class="cursor-pointer" v-tooltip.top="'Thêm'">
              <i class="pi pi-plus-circle" style="font-size: 18px"></i>
            </a>
          </div>
        </div>
        <div style="overflow-x: auto" class="scroll-outer">
          <div class="scroll-inner">
            <table
              class="table table-condensed table-hover tbpad table-child"
              style="table-layout: fixed"
            >
              <thead>
                <tr>
                  <th
                    class="text-center row-bc sticky"
                    style="width: 100px; left: 0px !important"
                  ></th>
                  <th class="text-center row-bc" style="width: 200px">
                    Loại chế độ
                  </th>
                  <th class="text-center row-bc" style="width: 150px">
                    Ngày nhận hồ sơ
                  </th>
                  <th class="text-center row-bc" style="width: 150px">
                    Ngày hoàn thiện thủ tục
                  </th>
                  <th class="text-center row-bc" style="width: 150px">
                    Ngày nhận tiền BH trả
                  </th>
                  <th class="text-center row-bc" style="width: 150px">
                    Số tiền
                  </th>
                </tr>
              </thead>
              <tbody>
                <tr
                  v-for="(item, index) in props.insurance_resolves"
                  :key="index"
                >
                  <td
                    class="sticky"
                    align="center"
                    style="
                      color: black;
                      width: 100px;
                      left: 0px !important;
                      z-index: 100;
                    "
                  >
                    <a
                      @click="props.deleteRow(index, 2)"
                      class="hover"
                      v-tooltip.top="'Xóa'"
                    >
                      <i class="pi pi-times-circle" style="font-size: 18px"></i>
                    </a>
                  </td>
                  <td align="center">
                    <Dropdown
                      class="ip33"
                      v-model="item.type_mode"
                      :options="dictionarys[3]"
                      optionLabel="insurance_type_mode_name"
                      optionValue="insurance_type_mode_name"
                      :editable="true"
                      style="width: 200px"
                    />
                  </td>
                  <td align="center">
                    <Calendar
                      style="width: 150px"
                      class="ip33"
                      id="icon"
                      v-model="item.received_file_date"
                      :showIcon="true"
                    />
                  </td>
                  <td align="center">
                    <Calendar
                      style="width: 150px"
                      class="ip33"
                      id="icon"
                      v-model="item.completed_date"
                      :showIcon="true"
                    />
                  </td>
                  <td align="center">
                    <Calendar
                      style="width: 150px"
                      class="ip33"
                      id="icon"
                      v-model="item.received_money_date"
                      :showIcon="true"
                    />
                  </td>
                  <td align="center">
                    <InputNumber
                      class="ip33 p-0 input-money"
                      v-model="item.money"
                      style="witdh: 150px;"
                    />
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </form> -->
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="closeDialog"
        class="p-button-outlined"
      />

      <Button 
        v-if="datefilter == null"
        label="Lưu"
        icon="pi pi-check"
        @click="saveData()"
        autofocus
      />
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
</style>