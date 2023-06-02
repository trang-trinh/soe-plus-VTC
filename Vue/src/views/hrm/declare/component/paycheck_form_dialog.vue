<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { encr, checkURL } from "../../../../util/function.js";
import moment from "moment";
import exportDoc from "./paycheck_doc.vue"

const cryoptojs = inject("cryptojs");

const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios");

const basedomainURL = baseURL;
const config = {
  headers: {
    Authorization: `Bearer ${store.getters.token}`,
  },
};
const toast = useToast();
const props = defineProps({
  paycheck_form: Object,
  closeDialogDialog: Function,
  displayPaycheck: Boolean,
  
});
const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);

const options = ref({
  IsNext: true,
  sort: "created_date",
  SearchText: "",
  PageNo: 0,
  PageSize: 20,
  loading: true,
  totalRecords: null,

  pagenoP: 0,
  pagesizeP: 20,
  searchP: "",
  sortP: "created_date",
}); 
const datalists = ref([]);
const loadData = () => {
   debugger
  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_paycheck_form_dia",
            par: [
             
              { par: "declare_id", va: props.paycheck_form.declare_id },
              { par: "user_id", va: store.getters.user.user_id },
              { par: "profile_id", va:props.paycheck_form.profile_id },
              { par: "month", va:  props.paycheck_form.paycheck_form_date },
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
      data.forEach((element, i) => {
        element.STT = options.value.PageNo * options.value.PageSize + i + 1;
        element.paycheck_form_id =props.paycheck_form.paycheck_form_id;
      });
       
      datalists.value =renderPaycheck(data.filter(x=>x.paycheck_type==1),data.filter(x=>x.paycheck_type==2));
      options.value.loading = false;
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");

      options.value.loading = false;
    });
};
const renderPaycheck=(dataPa,dataChild)=>{
var arr =[];
  dataPa.forEach(element => {
    arr.push(element);
    
    dataChild.forEach(item => {
      if(item.parent_id==element.paycheck_id){
        arr.push(item);
      }
    });
    
  });
  return arr;
}
//Thêm bản ghi

const saveDataDialog = (  ) => {
  let formData = new FormData();
  
  formData.append("hrm_paycheck_form_details", JSON.stringify(datalists.value));
  formData.append("hrm_paycheck_form", JSON.stringify(props.paycheck_form));
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
 
    axios
      .post(baseURL + "/api/hrm_paycheck_form_details/add_hrm_paycheck_form_details", formData, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Cập nhật phiếu lương thành công!");
   
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

function renderhtmlWord(id, htmltable) {
  htmltable = "";
  //Style
  htmltable += `<style>
    #formprint, #formword  {
      background: #fff !important;
    }
    #formprint *, #formword * {
      font-family: "Times New Roman", Times, serif !important;
      font-size: 16pt;
    }
    .title1,
    .title1 * {
      font-size: 24pt !important;
    }
    .title2,
    .title2 * {
      font-size: 20pt !important;
    }
    .title3,
    .title3 * {
      font-size: 16pt !important;
    }
    .boder tr th,
    .boder tr td {
      border: 1px solid #999999 !important;
      padding: 0.5rem;
    }
    table {
      min-width: 100% !important;
      page-break-inside: auto !important;
      border-collapse: collapse !important;
      table-layout: fixed !important;
    }
    thead {
      display: table-header-group !important;
    }
    tbody {
      display: table-header-group !important;
    }
    tr {
      -webkit-column-break-inside: avoid !important;
      page-break-inside: avoid !important;
    }
    tfoot {
      display: table-footer-group !important;
    }
     
    .text-center {
      text-align: center !important;
    }
    .text-left {
      text-align: left !important;
    }
    .format-center {
    display: flex;
    justify-content: center;
    align-items: center;
    text-align: center;
}
    .text-right {
      text-align: right !important;
    }
    .html p{
      margin: 0 !important;
      padding: 0 !important;
    }
  </style>`;
  var html = document.getElementById(id);
  if (html) {
    htmltable += html.innerHTML;
  }
  return htmltable;
}

 
const exportWord = (method) => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  var htmltable = "";
  htmltable = renderhtmlWord("formword", htmltable);
  axios
    .post(
      baseURL + "/api/device_handover/ExportDoc",
      {
        lib: "word",
        name: "BIEN_BAN_BAN_GIAO",
        html: htmltable,
        opition: {
          orientation: "Portrait",
          pageSize: "A4",
          left: 37.79,
          top: 68.03,
          right: 37.79,
          bottom: 68.03,
        },
      },
      config
    )
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        swal.close();

        toast.success("Kết xuất dữ liệu thành công!");
        if (response.data.path != null) {
          window.open(baseURL + response.data.path);
        }
      } else {
        swal.fire({
          title: "Thông báo!",
          text: response.data.ms,
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    })
    .catch((error) => {
      if (error.status === 401) {
        swal.fire({
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};



const print = () => {
  var htmltable = "";
 
  htmltable = renderhtmlWord("formword", htmltable);
  var printframe = window.frames["printframe"];
  printframe.document.write(htmltable);
  setTimeout(function () {
    printframe.print();
    printframe.document.close();
  }, 0);
};
const displayPaycheck=ref(false);
onMounted(() => {
  loadData(true);
  displayPaycheck.value=props.displayPaycheck;
  return {};
});
</script>
<template>
  <Dialog
    :header="'Chi tiết phiếu lương'"
    v-model:visible="displayPaycheck"
    :style="{ width: '50vw' }"
    :closable="true"
    :modal="true"
    @hide="props.closeDialogDialog"
    ><form>
      <div class="grid formgrid m-2">
        <div class="col-12 p-0 field font-bold text-md">
          Nhập thông tin chi trả tiền lương tháng
          {{
            moment(new Date(props.paycheck_form.paycheck_form_date)).format(
              " MM/YYYY"
            )
          }}
          cho nhân viên {{ props.paycheck_form.profile_user_name }}:
        </div>

        <table border="1" width="100%" cellpadding="0" >
          <tbody>
            <tr>
              <th class="w-2 py-2">STT</th>
              <th class="w-8  py-2">Tên đầu mục</th>
              <th class="w-2  py-2">Đơn vị tính</th>
            </tr>
            <tr
              v-for="(item, index) in datalists"
              :key="index"
              :class="item.paycheck_type == 1 ? 'font-bold' : ''"
            >
              <td>
             <div class="format-center">
              {{ item.type_order }}
             </div>
              </td>
              <td>
                <div class="pl-2">         {{ item.paycheck_name }}</div>
              </td>
              <td>
                <InputNumber class="w-full  design-paycheck"   v-model="item.paycheck_input" />
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      <iframe name="printframe" id="printframe" style="display: none"></iframe>
  <exportDoc :datas="datalists" :paycheck="props.paycheck_form"  />
    </form>
    <template #footer>
      <Toolbar class="custoolbar">
        <template #end>
          <div class="flex">
            <Button
        label="Hủy"
        icon="pi pi-times"
        @click="props.closeDialogDialog"
        class="p-button-outlined"
      />

      <Button
        label="Xuất Word"
        icon="pi pi-check"
       @click="exportWord()"
        class="p-button-outlined"
      />
      <Button
        label=" In phiếu"
        icon="pi pi-check"  class="p-button-outlined"
        @click="print()"
        autofocus
      />

      <Button
        label="Lưu"
        icon="pi pi-check"
        @click="saveDataDialog()"
        autofocus
      />
          </div>
        </template>
      </Toolbar>
      
    </template>
  </Dialog>

</template>
<style scoped>
#formprint {
  background: #fff !important;
}

#formprint * {
  font-family: "Times New Roman", Times, serif !important;
  font-size: 13pt;
}

.hover:hover {
  cursor: pointer;
  color: #2196f3 !important;
}

.title1,
.title1 * {
  font-size: 17pt !important;
}

.title2,
.title2 * {
  font-size: 16pt !important;
}

.title3,
.title3 * {
  font-size: 15pt !important;
}

.boder tr th,
.boder tr td {
  border: 1px solid #999999 !important;
  padding: 0.5rem;
}

table {
  min-width: 100% !important;
  page-break-inside: auto !important;
  border-collapse: collapse !important;
  table-layout: fixed !important;
}

thead {
  display: table-header-group !important;
}

tbody {
  display: table-header-group !important;
}

tr {
  -webkit-column-break-inside: avoid !important;
  page-break-inside: avoid !important;
}

tfoot {
  display: table-footer-group !important;
}

.uppercase,
.uppercase * {
  text-transform: uppercase !important;
}

.text-center {
  text-align: center !important;
}

.text-left {
  text-align: left !important;
}

.text-right {
  text-align: right !important;
}

.html p {
  margin: 0 !important;
  padding: 0 !important;
}
</style>
<style lang="scss" scoped>
::v-deep(.html) {
  p {
    margin: 0 !important;
    padding: 0 !important;
  }
}
</style>
