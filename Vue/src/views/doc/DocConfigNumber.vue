<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import moment from "moment";
import { encr } from "../../util/function";
//Khai báo
const cryoptojs = inject("cryptojs");
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const basedomainURL = baseURL;
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const toast = useToast();
const options = ref({
  IsNext: true,
  sort: "doc_master_id DESC",
  sortDM: null,
  search: "",
  pageno: 0,
  pagesize: 50,
  loading: true,
  totalRecords: null,
  start_date: null,
  end_date: null,
  next: true,
  id: null,
});
const organization = ref();
const loadOrg = () => {
  axios
    .post(
      baseURL + "/api/DocProc/CallProc",
      {
        str:
          encr(JSON.stringify(
            {
              proc: "doc_organization_get",
              par: [
                { par: "user_id", va: store.getters.user.user_id },
              ],
            }
          ),
            SecretKey, cryoptojs)
            .toString()
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        organization.value = data[0];
      }

      options.value.loading = false;
    })
    .catch((error) => {
      console.log(error);
      toast.error("Tải dữ liệu không thành công!");
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
const doc_number = ref({
  doc_number_receiver: false,
  doc_number_send: true,
  doc_number_internal: false,
  auto_gen_send: true,
  num_by_group_send: true,
  auto_gen_receiver: true,
  num_by_group_receiver: true,
  auto_gen_internal: true,
  num_by_group_internal: true,
});
const loadAddDocCodes = () => {
  axios
    .post(baseURL + "/api/doc_codes/add_doc_codes", null, config)
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
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
};
const listCodesReceiver = ref();

const listCodesSend = ref();

const listCodesInternal = ref();
const loadData = () => {
  axios
    .post(
      baseURL + "/api/DocProc/CallProc",
      {str: 
        encr(JSON.stringify(
          {
        proc: "doc_codes_list",
        par: [{ par: "user_id", va: store.getters.user.user_id }],
      }
      ),
        SecretKey, cryoptojs)
        .toString()
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      let data1 = JSON.parse(response.data.data)[1];
      let data2 = JSON.parse(response.data.data)[2];
      if (data.length > 0) {
        listCodesReceiver.value = data;
        listCodesSend.value = data1;
        listCodesInternal.value = data2;
      } else {
        listCodesReceiver.value = [];
        listCodesSend.value = [];
        listCodesInternal.value = [];
      }
      options.value.loading = false;
    })
    .catch((error) => {
      console.log(error);

      options.value.loading = false;
    });
};
const onChangeTypeDoc = (value) => {
  if (value == 1) {
    if (doc_number.value.doc_number_receiver) {
      doc_number.value.doc_number_send = false;
      doc_number.value.doc_number_internal = false;
    } else {
      doc_number.value.doc_number_send = true;
    }
  }
  if (value == 2) {
    if (doc_number.value.doc_number_send) {
      doc_number.value.doc_number_receiver = false;
      doc_number.value.doc_number_internal = false;
    } else {
      doc_number.value.doc_number_send = true;
      loadData();
    }
  }
  if (value == 3) {
    if (doc_number.value.doc_number_internal) {
      doc_number.value.doc_number_send = false;
      doc_number.value.doc_number_receiver = false;
    } else {
      doc_number.value.doc_number_send = true;
    }
  }
};
function array_is_unique(array, size) {
  //flag =  1 =>  tồn tại phần tử trùng nhau
  //flag =  0 =>  không tồn tại phần tử trùng nhau
  let flag = 0;
  for (let i = 0; i < size - 1; ++i) {
    for (let j = i + 1; j < size; ++j) {
      if (array[i] == array[j]) {
        /*Tìm thấy 1 phần tử trùng là đủ và dừng vòng lặp*/
        flag = 1;
        break;
      }
    }
  }

  return flag;
}
const saveDocConfig = () => {
  
  let arrP = [];
  let sttR = [];
  let sttS = [];
  let sttI = [];
  listCodesReceiver.value.forEach((element) => {
    element.auto_gen = doc_number.value.auto_gen_receiver;
    element.num_by_group = doc_number.value.num_by_group_receiver;
    sttR.push(element.is_order);
    arrP.push(element);
  });
  listCodesSend.value.forEach((element) => {
    element.auto_gen = doc_number.value.auto_gen_send;
    element.num_by_group = doc_number.value.num_by_group_send;
    sttS.push(element.is_order);
    arrP.push(element);
  });
  listCodesInternal.value.forEach((element) => {
    element.auto_gen = doc_number.value.auto_gen_internal;
    element.num_by_group = doc_number.value.num_by_group_internal;
    sttI.push(element.is_order);
    arrP.push(element);
  });
  for (let index = 0; index < sttI.length; index++) {
    if (sttI[index] == null) {
      swal.fire({
        title: "Thông báo",
        text: "Số thứ tự không được để trống!",
        icon: "error",
        confirmButtonText: "OK",
      });
      return;
    }
  }
  for (let index = 0; index < sttR.length; index++) {
    if (sttR[index] == null) {
      swal.fire({
        title: "Thông báo",
        text: "Số thứ tự không được để trống!",
        icon: "error",
        confirmButtonText: "OK",
      });
      return;
    }
  }
  for (let index = 0; index < sttS.length; index++) {
    if (sttS[index] == null) {
      swal.fire({
        title: "Thông báo",
        text: "Số thứ tự không được để trống!",
        icon: "error",
        confirmButtonText: "OK",
      });
      return;
    }
  }
  if (array_is_unique(sttR, sttR.length) == 1) {
    swal.fire({
      title: "Thông báo",
      text: "Số thứ tự không được trùng nhau!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  if (array_is_unique(sttS, sttS.length) == 1) {
    swal.fire({
      title: "Thông báo",
      text: "Số thứ tự không được trùng nhau!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  if (array_is_unique(sttI, sttI.length) == 1) {
    swal.fire({
      title: "Thông báo",
      text: "Số thứ tự không được trùng nhau!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  console.log("alas",arrP);
  axios
    .put(baseURL + "/api/doc_codes/update_doc_codes", arrP, config)
    .then((response) => {
      
      if (response.data.err != "1") {
      
        swal.close();
        toast.success("Cập nhật số hiệu thành công!");
      }
      else{
        console.log(response.data);
      }
    })
    .catch((error) => {
      
      console.log(error);
      swal.close();
      swal.fire({
        title: "Thông báo",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};
onMounted(() => {
  loadOrg();
  loadData();
  loadAddDocCodes();
});
</script>
<template>
  <div class="d-container ">
    <div class="d-lang-table surface-0">
      <div class=" w-full  p-4 style-vb-1  text-center text-3xl">
          BẢNG THIẾT LẬP SỐ HIỆU VĂN BẢN
        </div>
        <div class="w-full p-0 style-vb-2 text-center text-xl" v-if="organization">
          {{ organization.organization_name }}
        </div>
        <div class="w-full style-vb-3 p-4 text-center format-center">
          <div class="col-2 flex justify-content-end align-items-center">
            <div class="pr-2">Sổ văn bản đến:</div>
            <InputSwitch
              @change="onChangeTypeDoc(1)"
              class="w-4rem lck-checked"
              v-model="doc_number.doc_number_receiver"
            />
          </div>
          <div class="col-2 flex align-items-center format-center">
            <div class="pr-2">Văn bản đi:</div>
            <InputSwitch
              @change="onChangeTypeDoc(2)"
              class="w-4rem lck-checked"
              v-model="doc_number.doc_number_send"
            />
          </div>
          <div class="col-2 flex align-items-center">
            <div class="pr-2">Văn bản nội bộ:</div>
            <InputSwitch
              @change="onChangeTypeDoc(3)"
              class="w-4rem lck-checked"
              v-model="doc_number.doc_number_internal"
            />
          </div>
        </div>
      <div class="grid">
      
      
       
        <div class="col-12 p-0 format-center">
          <div class="col-6 p-0" v-show="doc_number.doc_number_receiver">
            <DataTable
              class="w-full"
              :rowHover="true"
              responsiveLayout="scroll"
              :lazy="true"
              :scrollable="true"
              scrollHeight="flex"
              filterMode="strict"
              :value="listCodesReceiver"
            >
              <Column
                field="is_order"
                header="STT"
                class="align-items-center justify-content-center text-center"
                headerStyle="text-align:center;max-width:80px"
                bodyStyle="text-align:center;max-width:80px"
              >
                <template #body="data">
                  <div class="w-full">
                    <InputNumber
                      inputStyle="text-align:center"
                      class="w-full"
                      v-model="data.data.is_order"
                    />
                  </div>
                </template>
              </Column>
              <Column
                field="info_col"
                header="Cột thông tin"
                class="align-items-center justify-content-center text-center"
                headerStyle="text-align:center"
                bodyStyle="text-align:center"
              >
                <template #body="data">
                  <div class="w-full h-full">
                    <Button
                      class="
                        w-full
                        h-full
                        text-center
                        surface-200
                        border-1 border-400
                        text-900
                      "
                      :label="data.data.info_col"
                    ></Button>
                  </div> </template
              ></Column>
              <Column
                field="is_used"
                header="Sử dụng"
                class="
                  align-items-center
                  justify-content-center
                  text-center
                  font-bold
                "
                headerStyle="text-align:center;max-width:100px"
                bodyStyle="text-align:center;max-width:100px"
              >
                <template #body="data">
                  <InputSwitch
                    class="w-4rem lck-checked"
                    v-model="data.data.is_used"
                  ></InputSwitch> </template
              ></Column>
              <Column
                field="separator"
                header="Ký tự ngăn cách"
                class="
                  align-items-center
                  justify-content-center
                  text-center
                  font-bold
                "
                headerStyle="text-align:center;max-width:200px"
                bodyStyle="text-align:center;max-width:200px"
              >
                <template #body="data">
                  <div class="w-full">
                    <InputText
                      class=""
                      style="max-width: 120px; text-align: center"
                      v-model="data.data.separator"
                    />
                  </div>
                </template>
              </Column>
            </DataTable>
          </div>
          <div class="col-6 p-0" v-show="doc_number.doc_number_internal">
            <DataTable
              class="w-full"
              :rowHover="true"
              responsiveLayout="scroll"
              :lazy="true"
              :scrollable="true"
              scrollHeight="flex"
              filterMode="strict"
              :value="listCodesInternal"
            >
              <Column
                field="is_order"
                header="STT"
                class="align-items-center justify-content-center text-center"
                headerStyle="text-align:center;max-width:80px"
                bodyStyle="text-align:center;max-width:80px"
              >
                <template #body="data">
                  <div class="w-full">
                    <InputNumber
                      inputStyle="text-align:center"
                      class="w-full"
                      v-model="data.data.is_order"
                    />
                  </div>
                </template>
              </Column>
              <Column
                field="info_col"
                header="Cột thông tin"
                class="align-items-center justify-content-center text-center"
                headerStyle="text-align:center"
                bodyStyle="text-align:center"
              >
                <template #body="data">
                  <div class="w-full h-full">
                    <Button
                      class="
                        w-full
                        h-full
                        text-center
                        surface-200
                        border-1 border-400
                        text-900
                      "
                      :label="data.data.info_col"
                    ></Button>
                  </div> </template
              ></Column>
              <Column
                field="is_used"
                header="Sử dụng"
                class="
                  align-items-center
                  justify-content-center
                  text-center
                  font-bold
                "
                headerStyle="text-align:center;max-width:100px"
                bodyStyle="text-align:center;max-width:100px"
              >
                <template #body="data">
                  <InputSwitch
                    class="w-4rem lck-checked"
                    v-model="data.data.is_used"
                  ></InputSwitch> </template
              ></Column>
              <Column
                field="separator"
                header="Ký tự ngăn cách"
                class="
                  align-items-center
                  justify-content-center
                  text-center
                  font-bold
                "
                headerStyle="text-align:center;max-width:200px"
                bodyStyle="text-align:center;max-width:200px"
              >
                <template #body="data">
                  <div class="w-full">
                    <InputText
                      class=""
                      style="max-width: 120px; text-align: center"
                      v-model="data.data.separator"
                    />
                  </div>
                </template>
              </Column>
            </DataTable>
          </div>
          <div class="col-6 p-0" v-show="doc_number.doc_number_send">
            <DataTable
              class="w-full"
              :rowHover="true"
              responsiveLayout="scroll"
              :lazy="true"
              :scrollable="true"
              scrollHeight="flex"
              filterMode="strict"
              :value="listCodesSend"
            >
              <Column
                field="is_order"
                header="STT"
                class="align-items-center justify-content-center text-center"
                headerStyle="text-align:center;max-width:80px"
                bodyStyle="text-align:center;max-width:80px"
              >
                <template #body="data">
                  <div class="w-full">
                    <InputNumber
                      inputStyle="text-align:center"
                      class="w-full"
                      v-model="data.data.is_order"
                    />
                  </div>
                </template>
              </Column>
              <Column
                field="info_col"
                header="Cột thông tin"
                class="align-items-center justify-content-center text-center"
                headerStyle="text-align:center"
                bodyStyle="text-align:center"
              >
                <template #body="data">
                  <div class="w-full h-full">
                    <Button
                      class="
                        w-full
                        h-full
                        text-center
                        surface-200
                        border-1 border-400
                        text-900
                      "
                      :label="data.data.info_col"
                    ></Button>
                  </div> </template
              ></Column>
              <Column
                field="is_used"
                header="Sử dụng"
                class="
                  align-items-center
                  justify-content-center
                  text-center
                  font-bold
                "
                headerStyle="text-align:center;max-width:100px"
                bodyStyle="text-align:center;max-width:100px"
              >
                <template #body="data">
                  <InputSwitch
                    class="w-4rem lck-checked"
                    v-model="data.data.is_used"
                  ></InputSwitch> </template
              ></Column>
              <Column
                field="separator"
                header="Ký tự ngăn cách"
                class="
                  align-items-center
                  justify-content-center
                  text-center
                  font-bold
                "
                headerStyle="text-align:center;max-width:200px"
                bodyStyle="text-align:center;max-width:200px"
              >
                <template #body="data">
                  <div class="w-full">
                    <InputText
                      class=""
                      style="max-width: 120px; text-align: center"
                      v-model="data.data.separator"
                    />
                  </div>
                </template>
              </Column>
            </DataTable>
          </div>
        </div>
        <div
          v-show="doc_number.doc_number_receiver"
          class="col-12 p-4 pb-2 text-center format-center"
        >
          <div class="text-center format-center">
            <div class="pr-2">Tự động tạo ra ký hiệu:</div>
            <InputSwitch
              class="w-4rem lck-checked"
              v-model="doc_number.auto_gen_receiver"
            />
          </div>
        </div>
        <div
          v-show="doc_number.doc_number_internal"
          class="col-12 p-4 pb-2 text-center format-center"
        >
          <div class="text-center format-center">
            <div class="pr-2">Tự động tạo ra ký hiệu:</div>
            <InputSwitch
              class="w-4rem lck-checked"
              v-model="doc_number.auto_gen_internal"
            />
          </div>
          <div class="text-center format-center">
            <div class="pr-2 pl-4">Số văn bản được tạo theo nhóm văn bản:</div>
            <InputSwitch
              class="w-4rem lck-checked"
              v-model="doc_number.num_by_group_internal"
            />
          </div>
        </div>
        <div
          v-show="doc_number.doc_number_send"
          class="col-12 p-4 pb-2 text-center format-center"
        >
          <div class="text-center format-center">
            <div class="pr-2">Tự động tạo ra ký hiệu:</div>
            <InputSwitch
              class="w-4rem lck-checked"
              v-model="doc_number.auto_gen_send"
            />
          </div>
          <div class="text-center format-center">
            <div class="pr-2 pl-4">Số văn bản được tạo theo nhóm văn bản:</div>
            <InputSwitch
              class="w-4rem lck-checked"
              v-model="doc_number.num_by_group_send"
            />
          </div>
        </div>
        <div class="col-12 style-vb-3 py-4 text-center format-center">
          <Button @click="saveDocConfig()">Cập nhật</Button>
        </div>
        <div class="col-12 p-0 format-center style-vb-5">
          <div class="col-8 p-0 text-left align-items-center">
            <div class="w-full">
              <font-awesome-icon
                style="color: #ecec15"
                icon="fa-solid fa-circle-info"
              />
              Bảng thiết lập bộ mã ký hiệu văn bản đi (phát hành), mục đích hệ
              thống tự động sinh ra bộ mã theo tiêu chí đơn vị sử dụng thiết
              lập.
            </div>
            <div class="w-full">
              Ví dụ: Đơn vị sử dụng cả 05 cột thông tin trên, theo thứ tự mặc
              định (từ 1 đến 5):
            </div>

            <div class="w-full px-6">
              <div>1. Số văn bản: 1 (tự động tăng)</div>
              <div>2. Năm: 2020</div>
              <div>3. Loại văn bản: QĐ (Quyết định)</div>
              <div>4. Mã phòng ban: TCHC (Tổ chức hành chính)</div>
              <div>5: Mã công ty: ECP</div>
              <div>
                Hệ thống tự động sinh ra bộ mã ký hiệu như sau:
                1/2020/QĐ/TCHC-ECP
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
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

}
@media only screen and (max-height: 678px) {
  .style-vb-5{
    display: none;
  }
}

</style>