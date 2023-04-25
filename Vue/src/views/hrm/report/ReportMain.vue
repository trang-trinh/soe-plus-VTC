<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { required, maxLength, minLength, email } from "@vuelidate/validators";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
import arrIcons from "../../../assets/json/icons.json";
import { encr } from "../../../util/function.js";
import moment from "moment";

const cryoptojs = inject("cryptojs");
const emitter = inject("emitter");

//color
const bgColor = ref([
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
  "#B0DE09",
]);

//Valid Form
const expandedKeys = ref({})
const id_active = ref();
const department_name = ref();
const datalists = ref();
const store = inject("store");
const selectedNodes = ref([]);
const filters = ref({});
const router = inject("router");
const opition = ref({
  search: "",
  PageNo: 1,
  PageSize: 1000,
  organization_type: null,
  user_id: store.getters.user.user_id,
});

const options = ref({
  IsNext: true,
  sort: "created_date",
  SearchText: null,
  PageNo: 0,
  PageSize: 20,
  loading: true,
  totalRecords: null,
  loadingP: true,
  pagenoP: 0,
  pagesizeP: 20,
  searchP: "",
  sortP: "created_date",
});
const isFirst = ref(true);
const toast = useToast();
const swal = inject("$swal");
const axios = inject("axios"); // inject axios
const basedomainURL = fileURL;
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const menuButs = ref();
//Khai báo function
const toggleExport = (event) => {
  menuButs.value.toggle(event);
};
const onNodeSelect = (node) => {
  selectedNodes.value.push(node.data.organization_id);
};
const onNodeUnselect = (node) => {
  selectedNodes.value.splice(
    selectedNodes.value.indexOf(node.data.organization_id),
    1,
  );
};
// on event

const loadData = () => {
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_report_main_list",
          par: [
            { par: "user_id", va: store.getters.user.user_id },
          ],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      data.filter(x => x.is_level==0).forEach((item1, index1) => {
        item1.label_module= romanize(index1+1) + '. '+ item1.module_name;
        data.filter(x => x.is_level == 1 && x.parent_id == item1.module_id).forEach((item2, index2)=>{
          item2.label_module= (index2+1) + '. '+ item2.module_name;
        });
      });

      datalists.value = data;
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");

      options.value.loading = false;
    });
};
const goDetailReport= (item)=>{
  router.push({ name: item.is_link});
}

const exportExcel = () => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  let proc = "hrm_contact_list_user_export";
  axios
    .post(
      baseURL + "/api/Excel/ExportExcel" ,
      {
        excelname: "DANH BA_"+department_name.value.toUpperCase(),
        proc: proc,
        par: [
        { par: "organization_id", va: id_active.value },
          { par: "user_id", va: store.getters.user.user_id },
          { par: "search", va: options.value.SearchText },
          { par: "pageno", va: options.value.pagenoP },
          { par: "pagesize", va: options.value.pagesizeP },
        ],
      },
      config
    )
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        swal.close();
        toast.success("Kết xuất Data thành công!");
        //window.open(baseURL + response.data.path);
        if (response.data.path != null) {
          let pathReplace = response.data.path
            .replace(/\\+/g, "/")
            .replace(/\/+/g, "/")
            .replace(/^\//g, "");
          var listPath = pathReplace.split("/");
          var pathFile = "";
          listPath.forEach((item) => {
            if (item.trim() != "") {
              pathFile += "/" + item;
            }
          });
          window.open(baseURL + pathFile);
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
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
      }
    });
};
function romanize(num) {
  var lookup = {M:1000,CM:900,D:500,CD:400,C:100,XC:90,L:50,XL:40,X:10,IX:9,V:5,IV:4,I:1},roman = '',i;
  for ( i in lookup ) {
    while ( num >= lookup[i] ) {
      roman += i;
      num -= lookup[i];
    }
  }
  return roman;
}
onMounted(() => {
  //init
  loadData();
});
</script>

<template>
  <div class="main-layout true flex-grow-1 p-2 pb-0 pr-0">
    <div class="overflow-auto" style="max-height: calc(100vh - 40px);">
      <table class="w-full" id="table-bc">
      <thead>
      <tr class="">
        <th align="center">
          Tên báo cáo
        </th>
        <th align="center" width="150">
          Mã báo cáo
        </th>
        <th align="center" width="400" >
          Ghi chú
        </th>
      </tr>
    </thead>
    <tbody>
        <tr v-for="(item, index) in datalists" :key="index">
          <td  class="text-left"
            >
            <span :class="item.is_level==0 ? 'row-parent':'row-child'" @click="item.is_level==0?'':goDetailReport(item)">
              {{ item.label_module }}
            </span>
          </td>
          <td  class="text-center" >
              {{ item.report_code }}
          </td>
          <td class="text-left">
              {{ item.report_note }}
          </td>
        </tr>
    </tbody>
    </table>
    </div>
  </div>

</template>

<style scoped>

  th, td{
    background: #fff;
    padding: 1rem;
  }
  .row-parent{
    font-weight: bold;
    margin-left:0.5em ;
  }
  .row-child{
    cursor: pointer;
    margin-left:1.5em ;
  }
  .row-child:hover {
  color: #0078d4;
}
</style>
<style lang="scss" scoped>
// ::v-deep(.p-treetable-tbody) {
//   tr {
//     cursor: pointer;
//   }
//   tr > td {
//   border:none;
// }
//}

</style>
