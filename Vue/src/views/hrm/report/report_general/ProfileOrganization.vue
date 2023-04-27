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
                        proc: "hrm_report_profile_organization_list1",
                        par: [
                            { par: "search", va: options.value.SearchText },
                            { par: "user_id", va: store.getters.user.user_id },
                            { par: "department_id", va: options.value.department_id},
                            { par: "gender", va: options.value.gender},
                            { par: "title_id", va: options.value.title_id},
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
                    const startDate = moment(item.recruitment_date || new Date());
                    const endDate = moment(new Date());
                    item.duration = moment.duration(endDate.diff(startDate));
                    item.diffyear = item.duration.years();
                    item.diffmonth = item.duration.months();
                });
                data[0] = groupBy(data[0], 'department_id');
                    let arr = [];
                    for (let pb in data[0]) {
                        // let data_ns_by_id = groupBy(data[0][pb], 'profile_code');
                        // let arr_ns = [];
                        // for (let ns in data_ns_by_id) {
                        //     arr_ns.push({ group_ns: ns, name_group_ns: data_ns_by_id[ns][0].profile_user_name, list_con: data_ns_by_id[ns] });
                        // };
                        // data_ns_by_id = arr_ns;
                        arr.push({ group_pb: pb, name_group_pb: data[0][pb][0].organization_name, list_ns: data[0][pb] });
                    }
                datalists.value = arr;
                options.totalRecords = arr.length;
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
const exportExcel = () => {
  
  let name = "BC.HS002";
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
  tab_text = tab_text+ "<table><td colspan='18' class='head2'>"+(department_name.value.toUpperCase() || store.getters.user.organization_name)+"</td></table>";
  // tab_text = tab_text+ "<table><td colspan='18' class='cstd' style='text-align: left; vertical-align: left;'>CÔNG TY/PHÒNG/TRUNG TÂM "+(store.getters.user.organization_name||".......")+"</td></table>";
  tab_text =
      tab_text +'<table><td colspan="18" class="cstd" > BÁO CÁO TỔNG HỢP NHÂN SỰ</td >';
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
    <div style="overflow: scroll;max-height: calc(100vh - 140px);">
        <table cellspacing=0 id="table-bc" class="table table-condensed table-hover tbpad" style="table-layout: fixed;width: 100%;">
        <thead style="position: sticky; z-index: 6; top:0">
            <tr>
                <th class="text-center" width="50" rowspan="2" >STT</th>
                <th class="text-center" width="100" rowspan="2" >Mã nhân viên</th>
                <th class="text-center " width="150" rowspan="2">Họ và tên</th>
                <th class="text-center " width="100" rowspan="2">Ngày sinh</th>
                <th class="text-center " width="60" rowspan="2">Tuổi</th>
                <th class="text-center " width="100" rowspan="2">Giới tính</th>
                <th class="text-center " width="150" rowspan="2">Chức danh</th>
                <th class="text-center " width="150" rowspan="2">Chức vụ</th>
                <th class="text-center " width="100" rowspan="2">Loại LĐ</th>
                <th class="text-center " width="250" rowspan="2">Quê quán</th>
                <th class="text-center " width="100" rowspan="2">Tình trạng hôn nhân</th>
                <th class="text-center " width="80" rowspan="2">Dân tộc</th>
                <th class="text-center " width="80" rowspan="2">Tôn giáo</th>
                <th class="text-center " width="80" rowspan="2">Đảng viên</th>
                <th class="text-center " width="80" rowspan="2">Đoàn viên</th>
                <th class="text-center " width="300" colspan="2">Thuộc diện chính sách</th>
                <th class="text-center " width="300" colspan="2">Nơi đăng ký HKTT</th>
                <th class="text-center " width="300" colspan="2">Nơi đăng ở hiện nay</th>
                <th class="text-center " width="150" rowspan="2">Liên hệ khẩn cấp</th>
                <th class="text-center " width="150" rowspan="2">Thành phần gia đình</th>
                <th class="text-center " width="300" colspan="3">Theo dõi CMND</th>
                <th class="text-center " width="100" rowspan="2">Ngày vào đơn vị</th>
                <th class="text-center " width="120" rowspan="2">Thâm niên công tác</th>
                <th class="text-center " width="120" rowspan="2">Thâm niên phép tính năm</th>
                <th class="text-center " width="500" colspan="5">Trình độ chuyên môn chính</th>
                <th class="text-center " width="120" rowspan="2">Số di động</th>
                <th class="text-center " width="120" rowspan="2">Email</th>
                <th class="text-center " width="80" rowspan="2">Chiều cao</th>
                <th class="text-center " width="80" rowspan="2">Cân nặng</th>
            </tr>
            <tr>
                <th class="text-center " width="100">Đã tham gia quân đội</th>
                <th class="text-center " width="100">Con gia đình chính sách</th>
                <th class="text-center " width="100">Số nhà, đường phố</th>
                <th class="text-center " width="100">Xã/Phường, Quận/Huyện, Tỉnh/TP</th>
                <th class="text-center " width="100">Số nhà, đường phố</th>
                <th class="text-center " width="100">Xã/Phường, Quận/Huyện, Tỉnh/TP</th>
                <th class="text-center " width="100">Số CMND</th>
                <th class="text-center " width="100">Ngày cấp CMND</th>
                <th class="text-center " width="100">Nơi cấp CMND</th>
                <th class="text-center " width="100">Trình độ văn hóa</th>
                <th class="text-center " width="100">Bằng cấp</th>
                <th class="text-center " width="100">Chuyên ngành</th>
                <th class="text-center " width="100">Trường tốt nghiệp</th>
                <th class="text-center " width="100">Hình thức đào tạo</th>
            </tr>
        </thead>
        <tbody v-for="(bc, index1) in datalists" :key="index1">
            <tr>
                <td colspan="38" class="bg-group"><b>{{bc.name_group_pb}}</b></td>
            </tr>
            <tr v-for="(dg, index2) in bc.list_ns" :key="index2">
                <td class="text-center bg-stt">{{dg.stt}}</td>
                <td  align="left" class="bg-aliceblue" >
                    {{dg.profile_code}}
                </td>
                <td align="left" class="bg-aliceblue" >
                    {{dg.profile_user_name}}
                </td>
                <td align="center">
                  <span v-if="dg.birthday"> {{ moment(new Date(dg.birthday)).format("DD/MM/YYYY ") }}</span>
                </td>
                <td align="center">
                    {{dg.age}}
                </td>
                <td align="center">
                    {{dg.gender}}
                </td>
                <td align="left">{{ dg.title_name }}</td>
                <td align="left">
                    {{dg.position_name}}
                </td>
                <td align="left">
                    {{dg.personel_groups_name}}
                </td>
                <td align="left">
                    {{dg.birthplace_origin_name}}
                </td>
                <td align="center">
                    {{dg.marital_status==0?'Độc thân':dg.marital_status==1 ?'Kết hôn':dg.marital_status==2?'Ly hôn':''}}
                </td>
                <td align="center">
                    {{dg.ethnic_name}}
                </td>
                <td align="center">
                    {{dg.religion_name}}
                </td>
                <td align="center">
                    {{dg.is_partisan?'X':''}}
                </td>
                <td align="center">
                    {{dg.bevy_date?'X':''}}
                </td>
                <td align="center">
                    {{dg.is_join_military?'X':''}}
                </td>
                <td align="left">{{dg.military_policy_family}}</td>
                <td align="left">{{dg.place_register_permanent_first}}</td>
                <td align="left">{{dg.place_register_permanent_name}}</td>
                <td align="left">{{dg.place_permanent}}</td>
                <td align="left">{{dg.place_residence_name || dg.name}}</td>
                <td align="left">
                  <span v-if="dg.relationship_name">{{ dg.relationship_name}}: </span>
                  <span v-if="dg.involved_name">{{ dg.involved_name}} - </span>
                  <span v-if="dg.involved_phone">{{ dg.involved_phone}} </span>
                </td>
                <td align="left">{{dg.family_member}}</td>
                <td align="left">{{dg.identity_papers_code}}</td>
                <td align="left">
                  <span v-if="dg.identity_date_issue"> {{ moment(new Date(dg.identity_date_issue)).format("DD/MM/YYYY ") }}</span>
                </td>
                <td align="left">{{dg.name}}</td>
                <td align="left">
                  <span v-if="dg.start_date"> {{ moment(new Date(dg.start_date)).format("DD/MM/YYYY ") }}</span>
                </td>
                <td align="center">
                  <span v-if="dg.diffyear > 0">
                    {{ dg.diffyear }} năm
                  </span>
                  <span v-if="dg.diffmonth > 0">
                    {{ dg.diffmonth }} tháng
                  </span>
                </td>
                <td align="center">{{dg.countAllRecruitment}}</td>
                <td align="left">{{dg.cultural_level_name}}</td>
                <td align="left">{{dg.certificate_name}}</td>
                <td align="left">{{dg.specialization_name}}</td>
                <td align="left" width="150">{{dg.university_name}}</td>
                <td align="left">{{dg.form_traning_name}}</td>
                <td align="center">{{dg.phone}}</td>
                <td align="center">{{dg.email}}</td>
                <td align="center">{{dg.height}}</td>
                <td align="center">{{dg.weight}}</td>
            </tr> 
      
          </tbody>
    </table>
    </div>
    </div>
</template>
<style scoped>
  .bg-group{
    background-color: rgb(222, 230, 240) !important;
  }
  .bg-stt{
    background-color: #e6e6e6;
  }
    .table {
        margin-bottom: 0px !important;
    }

    
    /* td span {
        text-overflow: ellipsis;
        overflow: hidden;
        display: -webkit-box;
        -webkit-line-clamp: 2;
        -webkit-box-orient: vertical;
    } */

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
    }

    table th {
        background-color: #e6e6e6 !important;
    }

    table th, table td {
        border: 0.3px solid rgba(0,0,0,.3) !important;
    }

</style>
<style scoped>
#table-bc{
  max-height: calc(100vh - 110px);
  overflow-y: auto;
  overflow-x: scroll;

}
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
</style>
