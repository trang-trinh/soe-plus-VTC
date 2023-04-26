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
    description: null
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
    axios
        .post(
            baseURL + "/api/hrm/callProc",
            {
                str: encr(
                    JSON.stringify({
                        proc: "hrm_report_workers_list1",
                        par: [
                            { par: "search", va: options.value.SearchText },
                            { par: "user_id", va: store.getters.user.user_id },
                            { par: "department_id", va: options.value.department_id},
                            { par: "gender", va: options.value.gender},
                            { par: "academic_level_id", va: options.value.academic_level_id},
                            { par: "specialization_id", va: options.value.specialization_id},
                            { par: "professional_work_id", va: options.value.professional_work_id},
                            { par: "title_id", va: options.value.title_id},
                            { par: "description", va: options.value.description},
                            { par: "pageno", va: options.value.PageNo},
                            { par: "pagesize", va: options.value.PageSize},
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
                data[0].forEach((item, index) => {
                    item.stt = index + 1;
                });
                datalists.value = data[0];
                options.totalRecords = data[0].length;
            }
            else datalists.value = [];
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
const exportExcel = () => {
  let text_string = "";
  
  let name = "BC.HS003";
  let id = "tablequizz";
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
    '<style>.p-datatable-thead th {background:#7bb0d7 !important;height: 30px !important;} .cstd{font-family: Times New Roman;border:none!important; font-size: 12pt; font-weight: 700; text-align: center; vertical-align: center;color:#000000}.head2{font-family: Times New Roman;border:none!important; font-size: 11pt; text-align: left; vertical-align: left;}</style>'    
  tab_text = tab_text+ "<table><td colspan='18' class='head2'>TỔNG CÔNG TY.../CÔNG TY.........</td></table>";
  tab_text = tab_text+ "<table><td colspan='18' class='cstd' style='text-align: left; vertical-align: left;'>CÔNG TY/PHÒNG/TRUNG TÂM.........</td></table>";
  tab_text =
      tab_text +'<table><td colspan="18" class="cstd" > BÁO CÁO DANH SÁCH NHÂN SỰ</td >';
  tab_text = tab_text + "</table>";
  //var exportTable = $('#' + id).clone();
  //exportTable.find('input').each(function (index, elem) { $(elem).remove(); });\
  tab_text =
    tab_text +
    "<style>th,table,tr{font-family: Times New Roman; font-size: 11pt; vertical-align: middle;}</style><table border='1'>";
  var exportTable = document
    .getElementsByClassName("p-datatable-table")[0]
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
onMounted(() => {
    //init
    loadData();
    initTudien();
});
</script>

<template>
    <div class="main-layout true flex-grow-1 p-2 pb-0 pr-0">
        <div style="background-color: #fff; padding: 1rem;">
        <Toolbar class="w-full custoolbar">
          <template #start>
            <span class="p-input-icon-left">
              <i class="pi pi-search" />
              <InputText
                v-model="options.SearchText"
                v-on:keyup.enter="loadDataDetail(id_active,department_name)"
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
                <div class="col-4">Trình độ chuyên môn</div>
                <Dropdown
                  class="ip36 col-8"
                  v-model="options.academic_level_id"
                  :options="tudiens[1]"
                  optionLabel="academic_level_name"
                  optionValue="academic_level_id"
                  placeholder="Chọn trình độ chuyên môn"
                  :showClear="true"
                />              
              </div>
              <div class="field col-12 md:col-12 flex align-items-center">
                <div class="col-4">Chuyên ngành học</div>
                <Dropdown
                  class="ip36 col-8"
                  v-model="options.specialization_id"
                  :options="tudiens[5]"
                  optionLabel="specialization_name"
                  optionValue="specialization_id"
                  placeholder="Chọn chuyên ngành"
                  :showClear="true"
                />              
              </div>
              <div class="field col-12 md:col-12 flex align-items-center">
                <div class="col-4">Công việc chuyên môn</div>
                <Dropdown
                  class="ip36 col-8"
                  v-model="options.professional_work_id"
                  :options="tudiens[2]"
                  optionLabel="professional_work_name"
                  optionValue="professional_work_id"
                  placeholder="Chọn công việc chuyên môn"
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
              <div class="field col-12 md:col-12 flex align-items-center">
                <div class="col-4">Mô tả công việc</div>
                <InputText
                  spellcheck="false"
                  class="col-8 ip36"
                  v-model="options.description"
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
        <div>
            <DataTable
            id="table-bc"
            class="p-datatable-sm e-sm"
            :value="datalists"
            dataKey="contract_id"
            :showGridlines="true"
            :rowHover="true"
            :loading="options.loading"
            currentPageReportTemplate=""
            responsiveLayout="scroll"
            rows="100"
            :scrollable="true"
            scrollHeight="flex"
            rowGroupMode="subheader"
            groupRowsBy="personel_groups_id"
            :paginator="true"
            :totalRecords="options.totalRecords"
            @page="onPage($event)"
            @filter="onFilter($event)"
            @sort="onSort($event)"
            v-model:first="first"
            scrollDirection="both"
            >
            <!-- <DataTable class="table-ca-request" :value="datalists" :paginator="false" :scrollable="true"
            scrollDirection="both" scrollHeight="flex" :lazy="true" dataKey="request_formd_id" :rowHover="true"
            > -->
            <template #groupheader="slotProps">
               <span class="font-bold">{{ slotProps.data.personel_groups_name }}</span> 
            </template>
                <Column field="stt" header="STT" headerStyle="text-align:center;width:50px;height:50px"
                    bodyStyle="text-align:center;width:50px;"
                    class="align-items-center justify-content-center text-center">
                </Column>
                <Column field="profile_code" header="Mã nhân viên" 
                    headerStyle="text-align:center;width:80px;height:50px;justify-content:center"
                    bodyStyle="width:80px;text-align:left"
                    >
                </Column>
                <Column field="profile_user_name" header="Họ và tên"
                    headerStyle="text-align:center;height:50px;justify-content:center;width:150px"
                    bodyStyle="text-align:left;word-break:break-word;justify-content:start;width:150px">
                </Column>
                <Column field="organization_name" header="Đơn vị/phòng ban"
                    headerStyle="text-align:center;width:220px;height:50px;justify-content:center"
                    bodyStyle="width:220px;text-align:left"
                    >
                </Column>
                <Column field="gender" header="Giới tính" headerStyle="text-align:center;width:80px;height:50px;justify-content:center"
                    bodyStyle="width:80px;text-align:left"
                   >
                </Column>
                <Column field="birthday" header="Ngày sinh" headerStyle="text-align:center;width:100px;height:50px"
                    bodyStyle="text-align:center;width:100px;"
                    class="align-items-center justify-content-center text-center">
                    <template #body="{ data }">
                        <span v-if="data.birthday"> {{ moment(new Date(data.birthday)).format("DD/MM/YYYY ") }}</span>
                    </template>
                </Column>
                <Column field="age" header="Tuổi" headerStyle="text-align:center;width:60px;height:50px;"
                    bodyStyle="text-align:center;width:60px;"
                    class="align-items-center justify-content-center text-center">
                </Column>
                <Column field="ethnic_name" header="Dân tộc" headerStyle="text-align:center;width:80px;height:50px;justify-content:center"
                    bodyStyle="width:80px;text-align:left"
                  >
                </Column>
                <Column field="cultural_level_name" header="Trình độ văn hóa" headerStyle="text-align:center;width:150px;height:50px;justify-content:center"
                    bodyStyle="width:150px;text-align:left"
                   >
                </Column>
                <Column field="academic_level_name" header="Trình độ chuyên môn" headerStyle="text-align:center;width:150px;height:50px;justify-content:center"
                    bodyStyle="width:150px;text-align:left"
                  >
                </Column>
                <Column field="specialization_name" header="Chuyên ngành" headerStyle="text-align:center;width:150px;height:50px;justify-content:center"
                    bodyStyle="width:150px;text-align:left"
                   >
                </Column>
                <Column field="professional_work_name" header="Công việc chuyên môn" headerStyle="text-align:center;width:150px;height:50px;justify-content:center"
                    bodyStyle="width:150px;text-align:left"
                   >
                </Column>
                <Column field="description" header="Mô tả chi tiết công việc" headerStyle="text-align:center;width:150px;height:50px;justify-content:center"
                    bodyStyle="width:150px;text-align:left"
                   >
                </Column>
                <Column field="start_date" header="Ngày vào đơn vị" headerStyle="text-align:center;width:100px;height:50px"
                    bodyStyle="text-align:center;width:100px;"
                    class="align-items-center justify-content-center text-center">
                    <template #body="{ data }">
                        <span v-if="data.birthday"> {{ moment(new Date(data.start_date)).format("DD/MM/YYYY ") }}</span>
                    </template>
                </Column>
                <Column field="is_partisan" header="Đảng viên" headerStyle="text-align:center;width:100px;height:50px;"
                    bodyStyle="text-align:center;width:100px;"
                    class="align-items-center justify-content-center text-center">
                    <template #body="{ data }">
                        <span v-if="data.is_partisan">X</span>
                    </template>
                </Column>
                <Column field="position_name" header="Chức vụ" headerStyle="text-align:center;width:100px;height:50px;justify-content:center"
                    bodyStyle="width:100px;text-align:left"
                   >
                </Column>
                <Column field="title_name" header="Chức danh"
                    headerStyle="text-align:center;width:100px;height:50px;justify-content:center"
                    bodyStyle="width:100px;text-align:left"
                    >
                </Column>
                <Column field="place_permanent" header="Nơi ở hiện nay"
                    headerStyle="text-align:center;width:200px;height:50px;justify-content:center"
                    bodyStyle="width:200px;text-align:left"
                    >
                </Column>
                <Column field="note" header="Ghi chú"
                    headerStyle="text-align:center;width:100px;height:50px;justify-content:center"
                    bodyStyle="width:100px;text-align:left"
                    >
                </Column>
                <template #empty>
                    <div class="
                    align-items-center
                    justify-content-center
                    p-4
                    text-center
                    m-auto
                    " >
                        <img src="../../../../assets/background/nodata.png" height="144" />
                        <h3 class="m-1">Không có dữ liệu</h3>
                    </div>
                </template>
            </DataTable>
        </div>
    </div>
</template>

<style scoped>
#table-bc{
  max-height: calc(100vh - 110px);
  overflow-y: auto;
  overflow-x: scroll;

}
th,
td {
    background: #fff;
    padding: 1rem;
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
