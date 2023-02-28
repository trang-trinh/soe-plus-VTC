<script setup>
import { ref, inject, onMounted, nextTick } from "vue";
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
const loadData = (is_clickfirst) => {
  let nav_type = doc_number.value.doc_number_receiver ? 1 : (doc_number.value.doc_number_send ? 2 : 3);
  axios
    .post(
      baseURL + "/api/DocProc/CallProc",
      {str: 
        encr(JSON.stringify(
          {
        proc: "doc_codes_list",
        par: [{ par: "organization_id", va: store.getters.user.organization_id },
        { par: "nav_type", va: nav_type }
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
     doc_codes.value = data;
     debugger
     if(is_clickfirst){
      if(doc_codes.value.length > 0){
        doc_code.value = doc_codes.value[0];
        getDocCodeByID({value: {code_master_id: doc_codes.value[0].code_master_id}});
     }
     }
      options.value.loading = false;
    })
    .catch((error) => {
      console.log(error);

      options.value.loading = false;
    });
};
const getDocCodeByID = (ev) => {
  is_new.value = false;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios
    .post(
      baseURL + "/api/DocProc/CallProc",
      {str: 
        encr(JSON.stringify(
          {
        proc: "doc_codes_get_byID",
        par: [{ par: "code_master_id", va: ev.value.code_master_id }],
      }
      ),
        SecretKey, cryoptojs)
        .toString()
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      swal.close();
      selected_doc_code.value = data[0][0];
      filter_doc_code.value = selected_doc_code.value.code_name;
      selected_doc_code.value.details = data[1];
      selected_doc_code.value.groups = data[2];
      selected_doc_code.value.dispatch_books = data[3];
      options.value.loading = false;
    })
    .catch((error) => {
      swal.close();
      console.log(error);

      options.value.loading = false;
    });
}
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
  selected_doc_code.value = null;
  doc_code.value = null;
  loadData();
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
  if(!selected_doc_code.value.is_default && selected_doc_code.value.groups.length === 0 && selected_doc_code.value.dispatch_books.length === 0){
    swal.fire({
                    type: 'error',
                    icon: 'error',
                    title: 'Thông báo!',
                    text: 'Không có loại cấu hình nào được chọn !'
                });
                return false;
  }
  if(!filter_doc_code.value){
    swal.fire({
                    type: 'error',
                    icon: 'error',
                    title: 'Thông báo!',
                    text: 'Tên cấu hình số ký hiệu trống !'
                });
                return false;
  }
  let unique = selected_doc_code.value.details.filter((v, i, a) => a.findIndex(t => (t.is_order === v.is_order)) === i);
  if (unique.length < selected_doc_code.value.details.length) {
              swal.fire({
                    type: 'error',
                    icon: 'error',
                    title: 'Thông báo!',
                    text: 'Số thứ tự không được trùng nhau !'
                });
                return false;
            }
  if(doc_number.value.doc_number_receiver) selected_doc_code.value.nav_type === 1
  else if(doc_number.value.doc_number_send) selected_doc_code.value.nav_type === 2
  else if(doc_number.value.doc_number_internal) selected_doc_code.value.nav_type === 3
  selected_doc_code.value.code_name = filter_doc_code.value;
  let formData = new FormData();
  formData.append("doc_code", JSON.stringify(selected_doc_code.value));
  if(!selected_doc_code.value.is_default && selected_doc_code.value.groups.length > 0){
    let grs = selected_doc_code.value.groups.map(x=>x.doc_group_id);
    formData.append("groups", JSON.stringify(grs));
  }
  if(!selected_doc_code.value.is_default && selected_doc_code.value.dispatch_books.length > 0)
  {
    let dpbs = selected_doc_code.value.dispatch_books.map(x=>x.dispatch_book_id);
    formData.append("dispatch_books", JSON.stringify(dpbs));
  }
  formData.append("details", JSON.stringify(selected_doc_code.value.details));
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: 'post',
    url: baseURL +
    "/api/doc_codes/update_doc_codes",
    data: formData,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      
      if (response.data.err != "1") {
        swal.close();
        loadData(true);
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
const delDocConfig = () => {
  let id = selected_doc_code.value.code_master_id;
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá cấu hình này không!",
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
          .delete(baseURL + "/api/doc_codes/delete_doc_codes", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: [id],
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá cấu hình thành công!");
              selected_doc_code.value = null;
              loadData();
            } else {
              console.log(response.data.ms);
              swal.fire({
                title: "Thông báo",
                text: "Xảy ra lỗi khi xóa cấu hình",
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
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                icon: "error",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
}
//Thêm log
const addLog = (log) => {
  axios.post(baseURL + "/api/Proc/AddLog", log, config);
};
// Load Category
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
      category.value.issue_places = data[0].filter(x=>!x.is_slider);
      category.value.issue_places_checkbox = data[0].filter(x=>x.is_slider);

      category.value.groups = data[1];
      category.value.org_groups = data[1];
      
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
      category.value.reservation_numbers = data[13];
      options.value.loading = false;
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
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
// Init doc code
const doc_codes = ref([]);
const code_details_init = [
  {idKey: 'Sovanban', is_order: 1, info_col: 'Số văn bản', separator: '/'},
  {idKey: 'Nam', is_order: 2, info_col: 'Năm', separator: '/'},
  {idKey: 'Nhomvanban', is_order: 3, info_col: 'Nhóm văn bản', separator: '/'},
  {idKey: 'Maphongban', is_order: 4, info_col: 'Mã phòng ban', separator: '/'},
  {idKey: 'Macongty', is_order: 5, info_col: 'Mã công ty', separator: '/'},
  {idKey: 'Makhoi', is_order: 6, info_col: 'Mã khối cơ quan', separator: ''},
]
const selected_doc_code = ref();
const filter_doc_code = ref();
const doc_code = ref({
    organization_id: store.getters.user.organization_id,
    nav_type: 2,
    is_default: true,
    code_name: 'Cấu hình mới',
    groups: [],
    dispatch_books: [],
    details: code_details_init
});
const is_new = ref(false);
const createNewDocCode = () => {
  is_new.value = true;
  doc_code.value = {
    organization_id: store.getters.user.organization_id,
    nav_type: 2,
    is_default: true,
    code_name: 'Cấu hình mới',
    groups: [],
    dispatch_books: [],
    details: code_details_init
  }
  // doc_codes.value.unshift(doc_code.value);
  filter_doc_code.value = doc_code.value.code_name;
  selected_doc_code.value = doc_code.value;
  nextTick(() => {
    let focus_ip = document.querySelector("#focus-input > input");
  if(focus_ip){
    focus_ip.focus();
  }
  })
}
onMounted(() => {
  loadCategorys();
  loadOrg();
  loadData();
  // loadAddDocCodes();
});
</script>
<template>
  <div class="d-container ">
    <div class="d-lang-table surface-0 mr-0" style="overflow-y: auto">
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
        <div class="col-12 md:col-12 format-center">
          <div class="col-6 md:col-6 flex" style="justify-content: space-between; column-gap: 1rem">
            <div class="flex" style="column-gap: 1rem;">
              <div id="focus-input" class="flex align-items-center">
                <div class="pr-2">Cấu hình:</div>
                <InputText autoFocus v-if="is_new" v-model="filter_doc_code"/>
                <Dropdown v-if="!is_new" @change="getDocCodeByID" :selectOnFocus="true" v-model="doc_code" :options="doc_codes" :filter="true" optionLabel="code_name" />
              </div>
              <div v-if="selected_doc_code" class="flex align-items-center">
                <div class="pr-2">Mặc định:</div>
                <InputSwitch
                  
                  class="w-4rem lck-checked"
                  v-model="selected_doc_code.is_default"
                />
              </div>
            </div>
            <div class="flex">
              <Button v-if="selected_doc_code && selected_doc_code.code_master_id" @click="delDocConfig" label="Xoá" icon="pi pi-trash" class="mr-2 p-button-danger" />
              <Button icon="pi pi-plus-circle" label="Thêm mới" @click="createNewDocCode" />
            </div>
          </div>
        </div>
        <div v-if="selected_doc_code && !selected_doc_code.is_default" class="col-12 md:col-12 format-center pb-5">
          <div class="col-6 md:col-6" style="row-gap: 2rem">
          <div class="flex align-items-center pb-5">
            <div class="pr-2 col-3 p-0 text-left">Theo nhóm văn bản:</div>
            <MultiSelect class="wrap-multi col-9 p-0" v-model="selected_doc_code.groups" :options="category.groups" optionLabel="doc_group_name"
              placeholder="Nhóm văn bản" :filter="true" display="chip" />
          </div>
          <div class="flex align-items-center">
            <div class="pr-2 col-3 p-0 text-left">Theo khối cơ quan:</div>
            <MultiSelect class="wrap-multi col-9 p-0" v-model="selected_doc_code.dispatch_books" :options="category.dispatch_books" optionLabel="dispatch_book_name"
              placeholder="Khối cơ quan" :filter="true" display="chip" />
          </div>
          </div>
        </div>
      <div v-if="selected_doc_code" class="grid">
        <div class="col-12 p-0 format-center">
          <div class="col-6 p-0">
            <DataTable
              class="w-full"
              :rowHover="true"
              responsiveLayout="scroll"
              :lazy="true"
              :scrollable="true"
              scrollHeight="flex"
              filterMode="strict"
              :value="selected_doc_code.details"
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
          class="col-12 p-4 pb-2 text-center format-center"
        >
        <div class="text-center format-center">
            <div class="pr-2">Tự động tạo ra ký hiệu:</div>
            <InputSwitch
              class="w-4rem lck-checked"
              v-model="selected_doc_code.auto_gen"
            />
          </div>
          <div class="text-center format-center">
            <div class="pr-2 pl-4">Số văn bản được tạo theo nhóm văn bản:</div>
            <InputSwitch
              class="w-4rem lck-checked"
              v-model="selected_doc_code.num_by_group"
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
              Ví dụ: Đơn vị sử dụng cả 06 cột thông tin trên, theo thứ tự mặc
              định (từ 1 đến 6):
            </div>

            <div class="w-full px-6">
              <div>1. Số văn bản: 1 (tự động tăng)</div>
              <div>2. Năm: 2020</div>
              <div>3. Loại văn bản: QĐ (Quyết định)</div>
              <div>4. Mã phòng ban: TCHC (Tổ chức hành chính)</div>
              <div>5: Mã công ty: ECP</div>
              <div>6: Khối cơ quan: Đảng uỷ</div>
              <div>
                Hệ thống tự động sinh ra bộ mã ký hiệu như sau:
                1/2020/QĐ/TCHC-ECP/ĐƯ
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