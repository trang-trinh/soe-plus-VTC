<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { encr, checkURL } from "../../../../util/function.js";
import moment from "moment";

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
  closeDialogDialog:Function,
  displayPaycheck:Boolean
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
const loadCount = () => {
  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_paycheck_count",
            par: [
              { par: "search", va: options.value.SearchText },
              { par: "declare_id", va: props.paycheck_form.declare_id },
              { par: "user_id", va: store.state.user.user_id },
              { par: "status", va: null },
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
      let data1 = JSON.parse(response.data.data)[1];

      if (data.length > 0) {
        options.value.totalRecordsP = data[0].totalRecords;
      }
      if (data1.length > 0) {
        options.value.totalRecordsPage = data1[0].totalRecordsPage;
      }
    })
    .catch(() => {});
};
const datalists = ref([]);
const loadData = (rf) => {
  if (rf) {
    loadCount();
  }
  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_paycheck_list",
            par: [
              { par: "search", va: options.value.SearchText },
              { par: "declare_id", va: props.paycheck_form.declare_id },
              { par: "pageno", va: options.value.pagenoP },
              { par: "pagesize", va: options.value.pagesizeP },
              { par: "user_id", va: store.getters.user.user_id },
              { par: "status", va: null },
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
      });
      debugger;
      datalists.value = data;
      options.value.loading = false;
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");

      options.value.loading = false;
    });
};

//Thêm bản ghi

onMounted(() => {
  loadData(true);

  return {};
});
</script>
<template>
   <Dialog
    :header="'Chi tiết phiếu lương'"
    v-model:visible="props.displayPaycheck"
    :style="{ width: '50vw' }"
    :closable="true"
    :modal="true"
  ><form>
    <div class="grid formgrid m-2">
        <div class="col-12 p-0 field font-bold text-md">
        Nhập thông tin chi trả tiền lương tháng     {{
                    moment(new Date(props.paycheck_form.paycheck_form_date)).format(
                      " MM/YYYY"
                    )
                  }} cho nhân viên {{ props.paycheck_form.profile_user_name }}:
        </div>
      <div class="w-full" v-for="(item, index) in datalists" :key="index">
        <div class="col-12 p-0 flex" 
        :class="item.paycheck_type==1?'font-bold' :''"
        
        
        >
          <div class="col-2 border-1 border-solid border-black format-center">
            {{ item.type_order }}
          </div>

          <div class="col-7 border-1 border-solid border-black align-items-center flex">
            {{ item.paycheck_name }}
          </div>
          <div class="col-3 p-0 border-1 border-solid border-black align-items-center">
            <InputNumber
              class="w-full"
              :placeholder="  item.paycheck_unit "
              v-model="item.paycheck_input"
              :suffix=" ' ' + item.paycheck_unit"
            />
          </div>
        </div>
      </div>


    </div>
   
  </form> 
  <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="props.closeDialogDialog"
        class="p-button-outlined"
      />

      <Button
        label="Lưu"
        icon="pi pi-check"
        @click="saveDataDialog( )"
        autofocus
      />
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
