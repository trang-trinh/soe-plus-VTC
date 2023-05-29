<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useRouter, useRoute } from "vue-router";
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
const router = useRoute();
const store = inject("store");
var data_org = [];
const positions = ref();
const titles = ref();
const department_name = ref();
department_name.value= store.getters.user.organization_name;
const datalists = ref();
const filters = ref({});
const options = ref({
    totalRecords: null,
    departments: null,
    position_id: null,
    title_id: null,
  });
const genders = ref(
  [
    {text: "Nam",value:1},
    {text: "Nữ",value:2}
  ]
)
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
var department_id
  ,title_id
  ,position_id;
const loadData = () => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  if (options.value.departments != null && Object.keys(options.value.departments).length > 0) {
    var dep_ids = [];
    for (var key in options.value.departments) {
      if (options.value.departments[key]) {
        dep_ids.push(key);
      }
    }
    department_id = dep_ids.join(",");
  }
  title_id = getListStringfromArr(options.value.titles, 'title_id');
  position_id = getListStringfromArr(options.value.positions, 'position_id');
    axios
        .post(
            baseURL + "/api/hrm/callProc",
            {
                str: encr(
                    JSON.stringify({
                        proc: "hrm_report_contract_statistical_list1",
                        par: [
                            { par: "user_id", va: store.getters.user.user_id },
                            { par: "department_id", va: department_id},
                            { par: "title_id", va: title_id},
                            { par: "position_id", va: position_id},
                            { par: "is_link", va: options.value.is_link},
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
            options.value.totalRecords =data[0].length;
            if (data[0].length > 0) {
               var baocaoct = [];
                data[0].forEach((item, index) => {
                    item.is_active = false;
                    if (item.Congtac !== null) {
                      item.Congtac = JSON.parse(item.Congtac.replaceAll('\n', ' '));                            
                    } if (item.Congtac == null) {
                        item.Congtac = [];
                        item.Congtac.push(baocaoct);
                    }  
                });
                datalists.value = data[0];
            }
            else datalists.value = [];
            if (data[1].length > 0) {
              data_org = data[1];
              let obj = renderTreeDV(
                data[1],
                "organization_id",
                "organization_name",
                "phòng ban"
              );
              treedonvis.value = obj.arrtreeChils;
            }
          titles.value= data[2];
          positions.value= data[3];
          swal.close();
        })
        .catch((error) => {
          swal.close();
            toast.error("Tải dữ liệu không thành công!");
        });
};
//filter
const opfilter = ref();
const isfilter = ref(false)
const toggleFilter = (event) => {
  opfilter.value.toggle(event);
};
const filter = (event) => {
  opfilter.value.toggle(event);
  isfilter.value = true;
  loadData();
};
const resetFilter = (f) => {
  options.value.positions = [];
  options.value.titles = [];
  options.value.academic_levels = [];
  options.value.specializations = [];
  options.value.is_partisans = [];
  options.value.genders = [];
  options.value.description = null;
  options.value.departments = null;
  department_id.value= null;
  if(f) loadData(true);
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
function getListStringfromArr(arr, type){
  let id = null
  if (arr != null && arr.length > 0) {
    id = arr.map((x) => x[type]).join(",");
  } else id = null;
  return id;
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
    if (router.fullPath != null)
      options.value.is_link = router.fullPath;
    loadData();
});
</script>

<template>
    <div class="main-layout true flex-grow-1 p-2 pb-0 pr-0">
      <div style="background-color: #fff; padding: 1rem;padding-left: 0;">
        <div class="bg-white format-center py-1 font-bold text-xl">
          BÁO CÁO tHỐNG KÊ HỢP ĐỒNG<span v-if="options.totalRecords != null">&nbsp({{ options.totalRecords }})</span>
        </div>
        <Toolbar class="w-full custoolbar">
          <template #start>
            <Button
              @click="toggleFilter($event)"
              type="button"
              class="ml-2 p-button-outlined p-button-secondary"
              aria:haspopup="true"
              aria-controls="overlay_panel"
            >
              <div>
                <span class="mr-2"><i class="pi pi-filter"></i></span>
                <span class="mr-2">Chọn điều kiện lập báo cáo</span>
                <span><i class="pi pi-chevron-down"></i></span>
              </div>
            </Button>
            <OverlayPanel :showCloseIcon="false" ref="opfilter" appendTo="body" class="p-0 m-0 panel-filter" id="overlay_panel" style="width: 600px; z-index:1000">
              <div class="grid formgrid m-0">
                <div class="col-12 md:col-12 p-0" :style="{
                  minHeight: 'unset',
                  maxheight: 'calc(100vh - 300px)',
                  overflow: 'auto',
                }">
                  <div class="row">
                    <div class="col-12 md:col-12">
                      <div class="form-group">
                        <label>Chọn phòng ban/ Đơn vị</label>
                        <TreeSelect class="col-12 ip36 mt-2 p-0 text-left" style="max-width: calc(600px - 3rem);"  :options="treedonvis"
                          v-model="options.departments"  selectionMode="multiple" :metaKeySelection="false"
                          :showClear="true" :max-height="200" display="chip" placeholder="Chọn phòng ban/ Đơn vị">
                        </TreeSelect>
                      </div>
                    </div>  
                    <div class="col-6 md:col-6">
                      <div class="form-group">
                        <label>Chức danh</label>
                        <MultiSelect
                          :options="titles"
                          :filter="true"
                          :showClear="true"
                          :editable="false"
                          v-model="options.titles"
                          optionLabel="title_name"
                          placeholder="Chọn chức danh"
                          class="w-full limit-width"
                          style="min-height: 36px"
                          panelClass="d-design-dropdown"
                        >
                          <template #value="slotProps">
                            <ul
                              class="p-ulchip"
                              v-if="
                                slotProps.value && slotProps.value.length > 0
                              "
                            >
                              <li
                                class="p-lichip"
                                v-for="(value, index) in slotProps.value"
                                :key="index"
                              >
                                <Chip class="mr-2 mb-2 px-3 py-2">
                                  <div class="flex">
                                    <div>
                                      <span>{{ value.title_name }}</span>
                                    </div>
                                    <span
                                      tabindex="0"
                                      class="p-chip-remove-icon pi pi-times-circle format-flex-center"
                                      @click="
                                        removeFilter(index, options.titles);
                                        $event.stopPropagation();
                                      "
                                      v-tooltip.top="'Xóa'"
                                    ></span>
                                  </div>
                                </Chip>
                              </li>
                            </ul>
                            <span v-else>
                              {{ slotProps.placeholder }}
                            </span>
                          </template>
                        </MultiSelect>
                      </div>
                    </div>                        
                    <div class="col-6 md:col-6">
                      <div class="form-group">
                        <label>Chức vụ</label>
                        <MultiSelect
                          :options="positions"
                          :filter="true"
                          :showClear="true"
                          :editable="false"
                          v-model="options.positions"
                          optionLabel="position_name"
                          placeholder="Chọn chức vụ"
                          class="w-full limit-width"
                          style="min-height: 36px"
                          panelClass="d-design-dropdown"
                        >
                          <template #value="slotProps">
                            <ul
                              class="p-ulchip"
                              v-if="
                                slotProps.value && slotProps.value.length > 0
                              "
                            >
                              <li
                                class="p-lichip"
                                v-for="(value, index) in slotProps.value"
                                :key="index"
                              >
                                <Chip class="mr-2 mb-2 px-3 py-2">
                                  <div class="flex">
                                    <div>
                                      <span>{{ value.position_name }}</span>
                                    </div>
                                    <span
                                      tabindex="0"
                                      class="p-chip-remove-icon pi pi-times-circle format-flex-center"
                                      @click="
                                        removeFilter(index, options.positions);
                                        $event.stopPropagation();
                                      "
                                      v-tooltip.top="'Xóa'"
                                    ></span>
                                  </div>
                                </Chip>
                              </li>
                            </ul>
                            <span v-else>
                              {{ slotProps.placeholder }}
                            </span>
                          </template>
                        </MultiSelect>
                      </div>
                    </div>
                      
                  </div>
                  <div class="col-12 md:col-12 p-0">
                    <Toolbar class="border-none surface-0 outline-none px-0 pb-0 w-full">
                      <template #start>
                        <Button @click="resetFilter()" class="p-button-outlined" label="Bỏ chọn"></Button>
                      </template>
                      <template #end>
                        <Button @click="filter($event)" label="Lọc"></Button>
                      </template>
                    </Toolbar>
                  </div>
                </div>
              </div>
            </OverlayPanel>
        </template>

          <template #end>
            <Button
            label="Quay lại"
            icon="pi pi-arrow-left"
            class="p-button-outlined mr-2 p-button-secondary"
            @click="goBack()"
            />
            <Button
              @click="resetFilter(true)"
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
                  <th class="text-center" rowspan="2" width="150">Chức vụ</th>
                  <th class="text-center" rowspan="2" width="150">Chức danh</th>
                  <th class="text-center" rowspan="2" width="100">Ngày vào công ty</th>
                  <th class="text-center" rowspan="2" width="150">Ghi chú</th>
                  <th class="text-center" colspan="12">Quá trình ký hợp đồng</th>
              </tr>
              <tr>
                  <th class="text-center" width="80">Lần ký</th>
                  <th class="text-center" width="150">Số hợp đồng</th>
                  <th class="text-center" width="120">Ngày ký</th>
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
@import url(../style_report.css);
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
::v-deep(.form-group) {
  .p-multiselect .p-multiselect-label, .p-treeselect .p-multiselect-label {
    height: 100%;
    display: flex;
    align-items: center;
    padding:0px 0.5rem !important
  }
}
::v-deep(.p-treeselect) {
   .p-multiselect-label {
    height: 100%;
    display: flex;
    align-items: center;
  }
  .p-treeselect-label,.p-treeselect-token{
    height: 100%;
  }
}
</style>
