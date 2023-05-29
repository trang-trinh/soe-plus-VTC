<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { required, maxLength, minLength, email } from "@vuelidate/validators";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
import { encr } from "../../../../util/function.js";
import moment from "moment";
import { useRouter, useRoute } from "vue-router";

const cryoptojs = inject("cryptojs");
const emitter = inject("emitter");
const router = useRoute();

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
const department_name = ref();
department_name.value= store.getters.user.organization_name;
const datalists = ref();
const treedonvis = ref();
const filters = ref({});
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
    gender: null,
    academic_level_id: null, 
    specialization_id: null,
    professional_work_id: null,
    title_id: null,
    departments: null,
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
var department_id;
const loadData = () => {
  if (options.value.departments != null && Object.keys(options.value.departments).length > 0) {
    var dep_ids = [];
    for (var key in options.value.departments) {
      if (options.value.departments[key]) {
        dep_ids.push(key);
      }
    }
    department_id = dep_ids.join(",");
  }
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
                        proc: "hrm_report_academic_level_member_list1",
                        par: [
                            { par: "user_id", va: store.getters.user.user_id },
                            { par: "department_id", va: department_id},
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
            if (data[0].length > 0) {
              options.value.totalRecords =data[0].length;
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
                // data[0] = groupBy(data[0], 'department_id');
                //     let arr = [];
                //     var count = 1;
                //     for (let pb in data[0]) {
                //         // let data_ns_by_id = groupBy(data[0][pb], 'profile_code');
                //         // let arr_ns = [];
                //         // for (let ns in data_ns_by_id) {
                //         //     arr_ns.push({ group_ns: ns, name_group_ns: data_ns_by_id[ns][0].profile_user_name, list_con: data_ns_by_id[ns] });
                //         // };
                //         // data_ns_by_id = arr_ns;
                //         data[0][pb].forEach((item)=>{
                //           item.stt = count;
                //           count++
                //         })
                //         arr.push({ group_pb: pb, name_group_pb: data[0][pb][0].department_name, list_ns: data[0][pb] });
                //     }
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
          swal.close();
          options.value.loading = false;
        })
        .catch((error) => {
            swal.close();
            toast.error("Tải dữ liệu không thành công!");
            options.value.loading = false;
            swal.close();
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

const exportExcel = () => {
  
  let name = "BC.HS006";
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
  tab_text = tab_text+ "<table><td colspan='14' class='head2'>"+(department_name.value.toUpperCase() || store.getters.user.organization_name)+"</td></table>";
  // tab_text = tab_text+ "<table><td colspan='18' class='cstd' style='text-align: left; vertical-align: left;'>CÔNG TY/PHÒNG/TRUNG TÂM "+(store.getters.user.organization_name||".......")+"</td></table>";
  tab_text =
      tab_text +'<table><td colspan="14" class="cstd" >BÁO CÁO TRÌNH ĐỘ HỌC VẤN</td >';
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
  if (router.fullPath != null)
    options.value.is_link = router.fullPath;
  loadData();
  //initTudien();
});
</script>

<template>
    <div class="main-layout true flex-grow-1 p-2 pb-0 pr-0">
      <div style="background-color: #fff; padding: 1rem;padding-left: 0;">
        <div class="bg-white format-center py-1 font-bold text-xl">
          BÁO CÁO NHÂN SỰ THEO TRÌNH ĐỘ HỌC VẤN &nbsp<span v-if="options.totalRecords != null">({{ options.totalRecords }})</span>
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
              <th class="text-center left-sticky1 left-1" rowspan="2" width="50">STT</th>
                <th class="text-center left-sticky1 left-2" colspan="4">Nhân viên</th>
                <th class="text-center" rowspan="2" width="150">Công việc chuyên môn</th>
                <th class="text-center" rowspan="2" width="150">Mô tả chi tiết công việc</th>
                <th class="text-center" rowspan="2" width="120">Trình độ văn hóa</th>
                <th class="text-center" colspan="6">Quá trình học</th>
              </tr>
              <tr>
                  <th class="text-center left-sticky1 left-2" width="100">Mã nhân viên</th>
                  <th class="text-center left-sticky1 left-3" width="150">Tên nhân viên</th>
                  <th class="text-center left-sticky1 left-4" width="100">Ngày sinh</th>
                  <th class="text-center left-sticky1 left-5" width="200">Đơn vị/ Phòng ban</th>
                  <th class="text-center" width="200">Nơi đào tạo</th>
                  <th class="text-center" width="130">Thời gian đào tạo</th>
                  <th class="text-center" width="100">Trình độ chuyên môn</th>
                  <th class="text-center" width="150">Chuyên ngành học</th>
                  <th class="text-center" width="110">Tốt nghiệp loại</th>
                  <th class="text-center" width="80">Trình độ chính</th>
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
                  <td v-if="index3 ==0"  :rowspan="bc.Congtac.length" align="center" class="left-sticky1 left-4">
                      <span v-if="bc.birthday"> {{ moment(new Date(bc.birthday)).format("DD/MM/YYYY ") }}</span>
                  </td>
                  <td v-if="index3 ==0"  :rowspan="bc.Congtac.length" align="left" class="left-sticky1 left-5">
                      {{bc.department_name}}
                  </td>
                  <td align="left" v-if="index3 ==0"  :rowspan="bc.Congtac.length" >{{}}</td>
                  <td align="left" v-if="index3 ==0"  :rowspan="bc.Congtac.length" >{{}}</td>
                  <td align="left" v-if="index3 ==0"  :rowspan="bc.Congtac.length" >{{bc.personel_groups_name}}</td>
                  <td align="left">{{qt.university_name}}</td>
                  <td align="center">
                      <span v-if="qt.start_date">{{  qt.start_date}}</span>
                      <span v-if="qt.start_date && qt.end_date"> - </span>
                      <span v-if="qt.end_date">{{ qt.end_date}}</span>
                  </td>
                  <td align="center">{{qt.academic_level_name}}</td>
                  <td align="left">{{qt.specialization_name}}</td>
                  <td align="center">{{qt.rating}}</td>
                  <td align="center">
                      <span v-if="qt.is_man_degree=='1'">X</span>
                  </td>
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
    .left-5 {
        left: 399px;
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
::v-deep(.p-treeselect) {
   .p-multiselect-label {
    height: 100%;
    display: flex;
    align-items: center;
    padding:0px 0.5rem !important;
  }
  .p-treeselect-label,.p-treeselect-token{
    height: 100%;
  }
}
</style>
