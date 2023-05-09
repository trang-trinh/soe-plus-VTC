<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { required, maxLength, minLength, email } from "@vuelidate/validators";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
import { encr } from "../../../../util/function.js";
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

//Khai báo biến
const store = inject("store");
var data_org = [];
const expandedKeys = ref({})
const id_active = ref();
const department_name = ref();
department_name.value= store.getters.user.organization_name;
const datalists = ref();
const personel_groups = ref();
const selectedNodes = ref([]);
const filters = ref({});
const router = inject("router");
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
    department_id: store.getters.user.organization_id,
    gender: null,
    academic_level_id: null, 
    specialization_id: null,
    professional_work_id: null,
    title_id: null,
  });
const genders = ref(
  [
    {text: "Nam",value:1},
    {text: "Nữ",value:2}
  ]
)
const tudiens= ref();
const isFirst = ref(true);
const toast = useToast();
const swal = inject("$swal");
const axios = inject("axios"); // inject axios
const basedomainURL = fileURL;
const config = {
    headers: { Authorization: `Bearer ${store.getters.token}` },
};
const menuButs = ref();
const first = ref(0);
const selectCapcha = ref();
selectCapcha.value = {};

// on event

const loadData = () => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
    axios
        .post(
            baseURL + "/api/hrm/callProc",
            {
                str: encr(
                    JSON.stringify({
                        proc: "hrm_report_contract_statistical_list",
                        par: [
                            { par: "search", va: options.value.SearchText },
                            { par: "user_id", va: store.getters.user.user_id },
                            { par: "department_id", va: options.value.department_id},
                            { par: "gender", va: options.value.gender},
                            { par: "position_id", va: options.value.position_id},
                        ],
                    }),
                    SecretKey,
                    cryoptojs,
                ).toString(),
            },
            config,
        )
        .then((response) => {
            let data = JSON.parse(response.data.data);
            if (data[0].length > 0) {
               var baocaoct = [];
                data[0].forEach((item, index) => {
                    item.is_active = false;
                    if (item.Congtac !== null) {
                        item.Congtac = JSON.parse(item.Congtac);                               
                    } if (item.Congtac == null) {
                        item.Congtac = [];
                        item.Congtac.push(baocaoct);
                    }  
                });
                datalists.value = data[0];
            }
            else datalists.value = [];
            swal.close();
            options.value.loading = false;
        })
        .catch((error) => {
            toast.error("Tải dữ liệu không thành công!");
            options.value.loading = false;
        });
};
//filter
const filterButs = ref();
const checkFilter = ref(false);
const toggleFilter = (event) => {
  filterButs.value.toggle(event);
};
const filterReport = ()=>{
  let keys = Object.keys(selectCapcha.value);
  if (keys.length> 0) {
    options.value.department_id = parseInt(keys[0]);
    department_name.value= data_org.filter(x=>x.organization_id == options.value.department_id )[0].organization_name || '';
  }
  checkFilter.value = true;
  loadData(true);
}
const refilterReport = () => {
  checkFilter.value = false;
  selectCapcha.value[store.getters.user.organization_id] = true;
  options.value.department_id = store.getters.user.organization_id;
  //loadData(true);
};
//Khai báo function
const toggleExport = (event) => {
  menuButs.value.toggle(event);
};
const itemButs = ref([
  {
    label: "Export dữ liệu ra Excel",
    icon: "pi pi-file-excel",
    command: (event) => {
      exportExcel();
    },
}]);
const treedonvis = ref();
const initTudien = () => {
  axios
    .post(
        baseURL + "/api/hrm/callProc",
        {
          str: encr(JSON.stringify({
            proc: "hrm_report_dictionary",
            par: [{ par: "user_id", va: store.getters.user.user_id }],
              }), SecretKey, cryoptojs
              ).toString()
            },
        config
      )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data[0].length > 0) {
        data_org = data[0];
        let obj = renderTreeDV(
          data[0],
          "organization_id",
          "organization_name",
          "phòng ban"
        );
        treedonvis.value = obj.arrtreeChils;
      }
      tudiens.value= data;
    })
    .catch((error) => {});
};
const isHuman =() =>{

}
const exportExcel = () => {
  
  let name = "BC.HĐ001";
  var htmltable1 = "";
  // htmltable1 = renderExcel_Ketqua();
  var tab_text = '<html xmlns:x="urn:schemas-microsoft-com:office:excel">';
  tab_text =
    tab_text +
    "<head><xml><x:ExcelWorkbook><x:ExcelWorksheets><x:ExcelWorksheet>";
  tab_text = tab_text + "<x:Name>Test Sheet</x:Name>";
  tab_text =
    tab_text +
    "<x:WorksheetOptions><x:Panes></x:Panes></x:WorksheetOptions></x:ExcelWorksheet>";
  tab_text =
    tab_text + "</x:ExcelWorksheets></x:ExcelWorkbook></xml></head><body>";
  tab_text =
    tab_text +
    "<style>.font-bold {font-weight:bold} th,td,table,tr{padding:5px;font-size:9pt;} .text-right{text-align:right} .text-left{text-align:left}table{margin:20px auto;border-collapse: collapse;}</style>";
  tab_text =
    tab_text +
    '<style>.p-datatable-thead th {background:#7bb0d7 !important;height: 30px !important;} .cstd{font-family: Times New Roman;border:none!important; font-size: 12pt; font-weight: 700; text-align: center; vertical-align: center;color:#000000}.head2{font-family: Times New Roman;border:none!important; font-size: 12pt;font-weight:bold; text-align: left; vertical-align: left;}</style>'    
  tab_text = tab_text+ "<table><td colspan='19' class='head2'>"+(department_name.value.toUpperCase() || store.getters.user.organization_name)+"</td></table>";
  // tab_text = tab_text+ "<table><td colspan='18' class='cstd' style='text-align: left; vertical-align: left;'>CÔNG TY/PHÒNG/TRUNG TÂM "+(store.getters.user.organization_name||".......")+"</td></table>";
  tab_text =
      tab_text +'<table><td colspan="19" class="cstd" >THEO DÕI KÝ HỢP ĐỒNG</td >';
  tab_text = tab_text + "</table>";
  //var exportTable = $('#' + id).clone();
  //exportTable.find('input').each(function (index, elem) { $(elem).remove(); });\
  tab_text =
    tab_text +
    "<style>th,table,tr{font-family: Times New Roman; font-size: 11pt; vertical-align: middle;}</style><table border='1'>";
  var exportTable = document
    .getElementById("table-bc")
    .cloneNode(true).innerHTML;
  tab_text = tab_text + exportTable;
  tab_text = tab_text + htmltable1;
  tab_text = tab_text + "</table>";
  tab_text = tab_text + '<meta charset="utf-8"/></ta></body></html>';
  var data_type =
    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
  var ua = window.navigator.userAgent;
  var msie = ua.indexOf("MSIE ");

  var fileName = name + " " + parseInt(Math.random() * 100) + ".xls";
  if (msie > 0 || !!navigator.userAgent.match(/Trident.*rv\:11\./)) {
    if (window.navigator.msSaveBlob) {
      var blob = new Blob([tab_text], {
        type: data_type, //"application/csv;charset=utf-8;"
      });
      navigator.msSaveBlob(blob, fileName);
    }
  } else {
    var blob2 = new Blob([tab_text], {
      type: data_type, //"application/csv;charset=utf-8;"
    });
    var filename = fileName;
    var elem = window.document.createElement("a");
    elem.href = window.URL.createObjectURL(blob2);
    elem.download = filename;
    document.body.appendChild(elem);
    elem.click();
    document.body.removeChild(elem);
  }
};
const activeRow = (row)=>{
  datalists.value.forEach((item)=>{
      item.is_active = false;
  })
  row.is_active= true;
}
const goBack = () => {
  history.back();
};
//Khai báo function
const renderTreeDV = (data, id, name, title) => {
  let arrChils = [];
  let arrtreeChils = [];
  data
    .filter((x) => x.parent_id == null)
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
function groupBy(list, props) {
    return list.reduce((a, b) => {
        (a[b[props]] = a[b[props]] || []).push(b);
        return a;
    }, {});
}
function formatNumber(a, b, c, d) {
  var e = isNaN((b = Math.abs(b))) ? 2 : b;
  b = void 0 == c ? "," : c;
  d = void 0 == d ? "," : d;
  c = 0 > a ? "-" : "";
  var g = parseInt((a = Math.abs(+a || 0).toFixed(e))) + "",
    n = 3 < (n = g.length) ? n % 3 : 0;
  return (
    c +
    (n ? g.substr(0, n) + d : "") +
    g.substr(n).replace(/(\d{3})(?=\d)/g, "$1" + d) +
    (e
      ? b +
        Math.abs(a - g)
          .toFixed(e)
          .slice(2)
      : "")
  );
}
onMounted(() => {
    //init
    loadData();
    initTudien();
});
</script>

<template>
    <div class="main-layout true flex-grow-1 p-2 pb-0 pr-0">
        <div style="background-color: #fff; padding: 1rem; padding-left: 0;">
          <h3 class="module-title module-title-hidden mt-0 ml-3 mb-2">
            <i class="pi pi-chart-bar"></i> Báo cáo thống kê hợp đồng
          </h3>
        <Toolbar class="w-full custoolbar">
          <template #start>
            <Button
            label="Quay lại"
            icon="pi pi-arrow-left"
            class="p-button-outlined mr-2 p-button-secondary"
            @click="goBack()"
            />
            <span class="p-input-icon-left">
              <i class="pi pi-search" />
              <InputText
                v-model="options.SearchText"
                v-on:keyup.enter="loadData()"
                type="text"
                spellcheck="false"
                placeholder="Tìm kiếm"
              />            
            </span>
            <Button
            :class="
              checkFilter ? 'ml-2' : 'ml-2 p-button-secondary p-button-outlined'
            "
            icon="pi pi-filter"
            @click="toggleFilter"
            aria-haspopup="true"
            aria-controls="overlay_panelS"
          />
          <OverlayPanel
            ref="filterButs"
            appendTo="body"
            :showCloseIcon="false"
            id="overlay_panelS"
            style="width: 400px"
            :breakpoints="{ '960px': '20vw' }"
          >
            <div class="grid formgrid m-2">
              <div class="field col-12 md:col-12 flex align-items-center">
                <div class="col-4 p-0">Phòng ban:</div>
                <TreeSelect
                  class="col-8 p-0 ip36"
                  v-model="selectCapcha"
                  :options="treedonvis"
                  :showClear="true"
                  :max-height="200"
                  placeholder="Chọn đơn vị/phòng ban"
                  optionLabel="organization_name"
                  optionValue="organization_id"
                >
                </TreeSelect>
              </div>
              <div class="field col-12 md:col-12 flex align-items-center">
                <div class="col-4">Giới tính</div>
                <Dropdown
                  class="ip36 col-8"
                  v-model="options.gender"
                  :options="genders"
                  optionLabel="text"
                  optionValue="value"
                  placeholder="Chọn giới tính"
                  :showClear="true"
                />
               
              </div>
              <div class="field col-12 md:col-12 flex align-items-center">
                <div class="col-4">Chức danh</div>
                <Dropdown
                  class="ip36 col-8"
                  v-model="options.title_id"
                  :options="tudiens[3]"
                  optionLabel="title_name"
                  optionValue="title_id"
                  placeholder="Chọn chức danh"
                  :showClear="true"
                />              
              </div>
              <div class="col-12 field p-0">
                <Toolbar class="toolbar-filter">
                  <template #start>
                    <Button
                      @click="refilterReport"
                      class="p-button-outlined"
                      label="Xóa"
                    ></Button>
                  </template>
                  <template #end>
                    <Button @click="filterReport" label="Lọc"></Button>
                  </template>
                </Toolbar>
              </div>
            </div>
          </OverlayPanel>
        </template>

          <template #end>
            <Button
              @click="onRefresh"
              class="mr-2 p-button-outlined p-button-secondary"
              icon="pi pi-refresh"
              v-tooltip="'Tải lại'"
            />

            <Button
              label="Tiện ích"
              icon="pi pi-file-excel"
              class="mr-2 p-button-outlined p-button-secondary"
              @click="toggleExport"
              aria-haspopup="true"
              aria-controls="overlay_Export"
            />
            <Menu
              id="overlay_Export"
              ref="menuButs"
              :model="itemButs"
              :popup="true"
            /> 
          </template>
        </Toolbar>
    </div>
    <div style="overflow: scroll;max-height: calc(100vh - 147px);min-height: calc(100vh - 147px);background-color: #fff;">
        <table cellspacing=0 id="table-bc" class="table table-condensed table-hover tbpad" style="width: max-content;">
        <thead style="position: sticky; z-index: 6; top:0">
            <tr>
                <th class="text-center sticky left-sticky1 left-1" rowspan="2" width="50">STT</th>
                <th class="text-center sticky left-sticky1 left-2" rowspan="2" width="100">Mã nhân sự</th>
                <th class="text-center sticky left-sticky1 left-3" rowspan="2" width="150">Họ và tên</th>
                <th class="text-center" rowspan="2" width="100">Ngày sinh</th>
                <th class="text-center" rowspan="2" width="100">Chức vụ</th>
                <th class="text-center" rowspan="2" width="100">Chức danh</th>
                <th class="text-center" rowspan="2" width="100">Ngày vào công ty</th>
                <th class="text-center" rowspan="2" width="100">Ghi chú</th>
                <th class="text-center" colspan="12">Quá trình ký hợp đồng</th>
            </tr>
            <tr>
                <th class="text-center" width="80">Lần ký</th>
                <th class="text-center" width="120">Số hợp đồng</th>
                <th class="text-center" width="100">Ngày ký</th>
                <th class="text-center" width="180">Loại hợp đồng</th>
                <th class="text-center" width="100">Thời hạn</th>
                <th class="text-center" width="100">Ngày hết hạn</th>
                <th class="text-center" width="140">Người ký</th>
                <th class="text-center" width="100">Mức lương</th>
                <th class="text-center" width="150">Ngạch lương</th>
                <th class="text-center" width="100">Bậc lương</th>
                <th class="text-center" width="100">Hệ số lương</th>
                <th class="text-center" width="100">Phụ cấp</th>
            </tr>
        </thead>
        <tbody v-for="(bc, index1) in datalists" :key="index1">
            <tr v-for="(qt, index3) in bc.Congtac" :key="index3" class="item-hover" @click="activeRow(bc)"> 
                <td v-if="index3 ==0" :rowspan="bc.Congtac.length" class="text-center bg-stt left-sticky1 left-1" :class="bc.is_active?'active-item':'bg-stt'">{{index1+1}}</td>
                <td v-if="index3 ==0"  :rowspan="bc.Congtac.length" align="left" class="left-sticky1 left-2">
                    {{bc.profile_code}}
                </td>
                <td v-if="index3 ==0"  :rowspan="bc.Congtac.length" align="left" class="left-sticky1 left-3">
                    {{bc.profile_user_name}}
                </td>
                <td v-if="index3 ==0"  :rowspan="bc.Congtac.length" align="center">
                  <span v-if="bc.birthday"> {{ moment(new Date(bc.birthday)).format("DD/MM/YYYY ") }}</span>
                </td>
                <td v-if="index3 ==0"  :rowspan="bc.Congtac.length" align="left" >
                    {{bc.position_name}}
                </td>
                <td v-if="index3 ==0"  :rowspan="bc.Congtac.length" align="left">
                    {{bc.title_name}}
                </td>
                <td v-if="index3 ==0"  :rowspan="bc.Congtac.length" align="center">
                  <span v-if="bc.recruitment_date"> {{ moment(new Date(bc.recruitment_date)).format("DD/MM/YYYY ") }}</span>
                </td>
                <td v-if="index3 ==0"  :rowspan="bc.Congtac.length" align="left" class="center"></td>
                <td align="center" v-if="qt.contract_code">Lần {{index3+1}}</td>
                <td align="left">{{qt.contract_code}}</td>
                <td align="center">{{qt.sign_date}}</td>
                <td align="left">{{qt.type_contract_name}}</td>
                <td align="center"></td>
                <td align="center">
                  <span v-if="qt.end_date"> {{ moment(new Date(qt.end_date)).format("DD/MM/YYYY ") }}</span>
                </td>
                <td align="left">{{qt.user_sign}}</td>
                <td align="right">
                  <span v-if="qt.wage>0">{{ formatNumber(qt.wage, 0, ".", ".") }}</span>
                </td>
                <td align="left">
                  {{qt.wage_name}}
                </td>
                <td align="center">
                  <span v-if="qt.wage_level>0">{{qt.wage_level}}</span>
                </td>
                <td align="center">
                  <span v-if="qt.coef_salary_name>0">{{qt.coef_salary_name}}</span>
                </td>
                <td align="center"></td>
            </tr>      
          </tbody>
    </table>
    
    </div>
    </div>
</template>
<style scoped>
  .item-hover:hover{
    background-color: #f0f8ff!important;
  }
  .bg-group{
    background-color: rgb(222, 230, 240) !important;
  }
  .bg-stt{
    background-color: #e6e6e6;
  }
  .active-item{
    background-color: #ffd95f;
  }
  .active-border{
    border:2px solid #008eff !important
  }
    .table {
        margin-bottom: 0px !important;
    }


    .left-sticky1 {
        position: sticky;
        z-index: 5;
        vertical-align: middle !important;
    }

    .left-1 {
        left:-1px;
    }
    .left-2 {
        left: 49px;
    }
    .left-3 {
        left: 149px;
    }
    .left-4 {
        left: 299px;
    }
    .btn.btn-secondary:hover {
        background-color: #e6f0f8 !important;
        color: #2f90d1 !important;
    }

    table{
      border: 0.3px solid rgba(0,0,0,.3) !important;
    }

    tr td {
        word-break: break-word;
        vertical-align: middle !important;
        cursor: pointer;
    }

    table th {
        background-color: #e6e6e6 !important;
    }

    table th, table td {
        border: 0.3px solid rgba(0,0,0,.3) !important;
    }

</style>
<style scoped>
/* #table-bc{
  max-height: calc(100vh - 110px);
  overflow-y: auto;
  overflow-x: scroll;

} */
th,
td {
    background: #fff;
    padding: 0.6rem;
}

.row-parent {
    font-weight: bold;
    margin-left: 0.5em;
}

.row-child {
    cursor: pointer;
    margin-left: 1.5em;
}

.row-child:hover {
    color: #0078d4;
}
.toolbar-filter {
  border: unset;
  outline: unset;
  background-color: #fff;
  padding-bottom: 0px;
}
td.bg-group>b {
    position: sticky;
    left: 10px;
}
</style>
<style lang="scss" scoped>
::v-deep(.p-rowgroup-header) {
  td {
    flex:1 1 0 !important;
  }
}
::v-deep(.p-datatable-emptymessage) {
  td {
    flex:1 1 0 !important;
  }
}
</style>
