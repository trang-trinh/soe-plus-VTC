<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { required, maxLength, minLength, email } from "@vuelidate/validators";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
import { encr } from "../../../../util/function.js";
import { useRouter, useRoute } from "vue-router";
import moment from "moment";

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
const expandedKeys = ref({})
const id_active = ref();
const department_name = ref();
department_name.value= store.getters.user.organization_name;
const datalists = ref();
const filters = ref({});
const options = ref({
    loading: true,
    departments: null,
    genders: null,
    personel_groups: null, 
    titles: null,
    positions: null,
    is_link : null,
  });
const genders = ref(
  [
    {text: "Nam",value:1},
    {text: "Nữ",value:2}
  ]
)
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
const positions = ref();
const titles = ref();
const personel_groups = ref();
// on event
var department_id
  ,gender
  ,personel_group_id
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
  gender = getListStringfromArr(options.value.genders, 'value');
  personel_group_id = getListStringfromArr(options.value.professional_works, 'personel_group_id');
  title_id = getListStringfromArr(options.value.titles, 'title_id');
  position_id = getListStringfromArr(options.value.positions, 'position_id');
  axios
      .post(
          baseURL + "/api/hrm/callProc",
          {
              str: encr(
                  JSON.stringify({
                      proc: "hrm_report_member_quit1",
                      par: [
                          { par: "user_id", va: store.getters.user.user_id },
                          { par: "department_id", va: department_id},
                          { par: "gender", va: gender},
                          { par: "title_id", va: title_id},
                          { par: "position_id", va: position_id},
                          { par: "personel_group_id", va: personel_group_id},
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
                data[0].forEach((item, index) => {
                    item.is_active = false;
                    const startDate = moment(item.recruitment_date || new Date());
                    const endDate = moment(new Date());
                    item.duration = moment.duration(endDate.diff(startDate));
                    item.diffyear = item.duration.years();
                    item.diffmonth = item.duration.months();
                    item.cultural_level_name = item.cultural_level_name? item.cultural_level_name.replace('/','/ '):null;
                });
                data[0] = groupBy(data[0], 'department_id');
                    let arr = [];
                    var count = 1;
                    for (let pb in data[0]) {
                        // let data_ns_by_id = groupBy(data[0][pb], 'profile_code');
                        // let arr_ns = [];
                        // for (let ns in data_ns_by_id) {
                        //     arr_ns.push({ group_ns: ns, name_group_ns: data_ns_by_id[ns][0].profile_user_name, list_con: data_ns_by_id[ns] });
                        // };
                        // data_ns_by_id = arr_ns;
                        data[0][pb].forEach((item)=>{
                          item.stt = count;
                          count++
                        })
                        arr.push({ group_pb: pb, name_group_pb: data[0][pb][0].department_name, list_ns: data[0][pb] });
                }
                datalists.value = arr;
                options.totalRecords = arr.length;
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
            personel_groups.value= data[4];
            swal.close();
            options.value.loading = false;
        })
        .catch((error) => {
            swal.close();
            toast.error("Tải dữ liệu không thành công!");
            options.value.loading = false;
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
  
  let name = "BC.HS005";
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
  tab_text = tab_text+ "<table><td colspan='31' class='head2'>"+(department_name.value.toUpperCase() || store.getters.user.organization_name)+"</td></table>";
  // tab_text = tab_text+ "<table><td colspan='18' class='cstd' style='text-align: left; vertical-align: left;'>CÔNG TY/PHÒNG/TRUNG TÂM "+(store.getters.user.organization_name||".......")+"</td></table>";
  tab_text =
      tab_text +'<table><td colspan="31" class="cstd" > DANH SÁCH THỐNG KÊ BÁO CÁO NHÂN SỰ NGHỈ VIỆC</td >';
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
const valueClick = ref();
const activeRow = (row, value)=>{
  valueClick.value = value;
  datalists.value.forEach((item)=>{
    item.list_ns.forEach((item2)=>{
      item2.is_active = false;
    })
  })
  row.is_active= true;
}
const goBack = () => {
  history.back();
};
const removeFilter = (idx, array) => {
  array.splice(idx, 1);
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
onMounted(() => {
 //init
 if (router.fullPath != null)
      options.value.is_link = router.fullPath;
    loadData();
   // initTudien();
});
</script>

<template>
    <div class="main-layout true flex-grow-1 p-2 pb-0 pr-0">
      <div style="background-color: #fff; padding: 1rem;padding-left: 0;">
        <div class="bg-white format-center py-1 font-bold text-xl">
          BÁO CÁO NHÂN SỰ ĐÃ NGHỈ VIỆC<span v-if="options.totalRecords != null">&nbsp({{ options.totalRecords }})</span>
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
                    <div class="col-6 md:col-6">
                      <div class="form-group">
                        <label>Giới tính</label>
                        <MultiSelect
                          :options="genders"
                          :filter="true"
                          :showClear="true"
                          :editable="false"
                          v-model="options.genders"
                          optionLabel="text"
                          placeholder="Chọn giới tính"
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
                                      <span>{{ value.text }}</span>
                                    </div>
                                    <span
                                      tabindex="0"
                                      class="p-chip-remove-icon pi pi-times-circle format-flex-center"
                                      @click="
                                        removeFilter(index, options.genders);
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
                        <label>Loại nhân sự</label>
                        <MultiSelect
                          :options="personel_groups"
                          :filter="true"
                          :showClear="true"
                          :editable="false"
                          v-model="options.personel_groups"
                          optionLabel="personel_groups_name"
                          placeholder="Chọn loại nhân sự"
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
                                      <span>{{ value.personel_groups_name }}</span>
                                    </div>
                                    <span
                                      tabindex="0"
                                      class="p-chip-remove-icon pi pi-times-circle format-flex-center"
                                      @click="
                                        removeFilter(index, options.personel_groups);
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
                <th class="text-center"  colspan="28">THÔNG TIN NHÂN SỰ</th>
                <th class="text-center"  colspan="3">THÔNG TIN, QUYẾT ĐỊNH, THỜI HẠN HỢP ĐỒNG LAO ĐỘNG</th>
              </tr>
              <tr>
                  <th class="text-center sticky left-sticky1 left-1" width="50">STT</th>
                  <th class="text-center sticky left-sticky1 left-2" width="150">Họ và tên</th>
                  <th class="text-center" width="100">Năm sinh</th>
                  <th class="text-center" width="100">Giới tính</th>
                  <th class="text-center" width="150">Số chứng thực</th>
                  <th class="text-center" width="150">Ngày chứng thực</th>
                  <th class="text-center" width="150">Nơi cấp chứng thực</th>
                  <th class="text-center" width="100">Loại nhân sự</th>
                  <th class="text-center" width="150">Chức vụ</th>
                  <th class="text-center" width="150">Chức danh</th>
                  <th class="text-center" width="150">Chức vụ kiêm nghiệm</th>
                  <th class="text-center" width="150">Công việc mô tả</th>
                  <th class="text-center" width="120">Di động</th>
                  <th class="text-center" width="120">Email</th>
                  <th class="text-center" width="200">Nơi ở hiện tại</th>
                  <th class="text-center" width="200">Hợp đồng lao động</th>
                  <th class="text-center" width="100">Số hợp đồng</th>
                  <th class="text-center" width="120">Ngày hết hạn hợp đồng</th>
                  <th class="text-center" width="120">Số ngày làm việc</th>
                  <th class="text-center" width="100">Trình độ học vấn</th>
                  <th class="text-center" width="100">Chuyên ngành</th>
                  <th class="text-center" width="150">Nơi đào tạo</th>
                  <th class="text-center" width="100">Mã số thuế</th>
                  <th class="text-center" width="100">Số sổ bảo hiểm</th>
                  <th class="text-center" width="150">Tháng đóng bảo hiểm</th>
                  <th class="text-center" width="150">Mức đóng bảo hiểm</th>
                  <th class="text-center" width="150">Nơi đóng BHXH</th>
                  <th class="text-center" width="150">Ký nhận</th>
                  <th class="text-center" width="120">Ngày vào công ty</th>
                  <th class="text-center" width="120">Ngày nghỉ việc</th>
                  <th class="text-center" width="150">Lý do nghỉ</th>
              </tr>
          </thead>
          <tbody v-for="(bc, index1) in datalists" :key="index1">
              <tr>
                  <td colspan="38" class="bg-group left-sticky1 left-1"><b>{{bc.name_group_pb}}</b></td>
              </tr>
              <tr v-for="(dg, index2) in bc.list_ns" :key="index2" class="item-hover" @click="activeRow(dg)">
                  <td class="text-center bg-stt left-sticky1 left-1" :class="dg.is_active?'active-item':'bg-stt'">{{dg.stt}}</td>
                  <td align="left" class="left-sticky1 left-2" @click="activeRow(dg)">
                    {{dg.profile_user_name}}
                  </td>
                  <td align="center" @click="activeRow(dg)">      
                      {{dg.birthday}}
                  </td>
                  <td align="center" >{{dg.gender}}</td>
                  <td align="center" >{{dg.identity_papers_code}}</td>
                  <td align="center" >
                    <span v-if="dg.identity_date_issue"> {{ moment(new Date(dg.identity_date_issue)).format("DD/MM/YYYY ") }}</span>
                  </td>
                  <td align="center" >{{dg.identity_name_issue}}</td>
                  <td align="center" >{{dg.personel_groups_name}}</td>
                  <td align="center" >{{dg.position_name}}</td>
                  <td align="center" >{{dg.title_name}}</td>
                  <td align="center" ></td>
                  <td align="center" ></td>
                  <td align="center" >{{dg.phone}}</td>
                  <td align="center" >{{dg.email}}</td>
                  <td align="center" >
                      {{ dg.place_permanent }} {{ dg.place_residence_name || dg.place_name }}
                  </td>
                  <td align="center" >{{dg.type_contract_name}}</td>
                  <td align="center" >{{dg.contract_code}}</td>
                  <td align="center" >
                    <span v-if="dg.contract_end_date"> {{ moment(new Date(dg.contract_end_date)).format("DD/MM/YYYY ") }}</span>
                  </td>
                  <td align="center" >
                    <span v-if="dg.diffyear > 0">
                      {{ dg.diffyear }} năm
                    </span>
                    <span v-if="dg.diffmonth > 0">
                      {{ dg.diffmonth }} tháng
                    </span>
                  </td>
                  <td align="center" >{{dg.cultural_level_name}}</td>
                  <td align="center" >{{dg.specialization_name}}</td>
                  <td align="center" >{{dg.university_name}}</td>
                  <td align="center" >{{dg.tax_code}}</td>
                  <td align="center" >{{dg.insurance_code}}</td>
                  <td align="center" ></td>
                  <td align="center" ></td>
                  <td align="center" ></td>
                  <td align="center" ></td>
                  <td align="center" >
                    <span v-if="dg.recruitment_date"> {{ moment(new Date(dg.recruitment_date)).format("DD/MM/YYYY ") }}</span>
                  </td>
                  <td align="center" >
                    <span v-if="dg.start_date_quit"> {{ moment(new Date(dg.start_date_quit)).format("DD/MM/YYYY ") }}</span>
                  </td>
                  <td align="center" ></td>        
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
    td.bg-group>b {
    position: sticky;
    left: 10px;
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
