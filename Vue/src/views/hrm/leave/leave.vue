<script setup>
import { onMounted, inject, ref, watch } from "vue";
import { encr } from "../../../util/function";
import { useToast } from "vue-toastification";
import moment from "moment";
import { groupBy } from "lodash";
import dialogleaveprofile from "./component/dialogleaveprofile.vue";

const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios");
const config = {
  headers: {
    Authorization: `Bearer ${store.getters.token}`,
  },
};
const toast = useToast();
const cryoptojs = inject("cryptojs");
const basedomainURL = baseURL;
const basefileURL = fileURL;

//Decalre
const isFunction = ref(false);
const newDate = new Date();
const options = ref({
  loading: true,
  user_id: store.getters.user.user_id,
  search: "",
  pageNo: 1,
  pageSize: 25,
  total: 0,
  sort: "created_date desc",
  orderBy: "desc",
  view: 2,
  tempyear: newDate,
  year: newDate.getFullYear(),
  month: newDate.getMonth() + 1,
  week: 0,
  day: newDate.getDate(),
  start_date: null,
  end_date: null,
  filter_organization_id: store.getters.user.organization_id,
  departments: [],
});
const isFirst = ref(true);
const selectedNodes = ref({});
const datas = ref([]);
const profile = ref({});

//Declare dictionary
const dictionarys = ref([]);
const department = ref([]);
const days = ref([]);
const months = ref([
  { month: 1, name: "Tháng 1" },
  { month: 2, name: "Tháng 2" },
  { month: 3, name: "Tháng 3" },
  { month: 4, name: "Tháng 4" },
  { month: 5, name: "Tháng 5" },
  { month: 6, name: "Tháng 6" },
  { month: 7, name: "Tháng 7" },
  { month: 8, name: "Tháng 8" },
  { month: 9, name: "Tháng 9" },
  { month: 10, name: "Tháng 10" },
  { month: 11, name: "Tháng 11" },
  { month: 12, name: "Tháng 12" },
]);
const weeks = ref([]);
const years = ref([]);

//filter
const search = () => {
  options.value.pageNo = 1;
  initData(true);
};
const opfilter = ref();
const toggleFilter = (event) => {
  opfilter.value.toggle(event);
};
const filter = (event) => {
  opfilter.value.toggle(event);
  search();
};
const resetFilter = () => {
  options.value.filter_organization_id = null;
  options.value.departments = [];
};
const changeOrganization = () => {
  bindDepartment();
};
const removeFilter = (idx, arr) => {
  arr.splice(idx, 1);
};
const bindDepartment = () => {
  department.value = JSON.parse(JSON.stringify(dictionarys.value[1]));
  var temp2 = [];
  addToArray(
    temp2,
    department.value,
    options.value.filter_organization_id,
    0,
    "is_order"
  );
  department.value = temp2;
};
const goYear = (date) => {
  if (date && options.value["year"] !== date.getFullYear()) {
    options.value["year"] = date.getFullYear();
    initData();
  }
};

//Function
const addToArray = (temp, array, id, lv, od) => {
  var filter = array.filter((x) => x.parent_id === id);
  filter = filter.sort((a, b) => {
    return b[od] - a[od];
  });
  if (filter.length > 0) {
    var sp = "";
    for (var i = 0; i < lv; i++) {
      sp += "---";
    }
    lv++;
    filter.forEach((item) => {
      item.lv = lv;
      item.close = true;
      if (!item.ids) {
        item.ids = "";
        item.ids += "," + item.organization_id;
      }
      if (!item.newname) item.newname = sp + item.organization_name;
      temp.push(item);
      addToArray(temp, array, item.organization_id, lv);
    });
  }
};

const componentKey = ref({});
const forceRerender = (type) => {
  if (!componentKey.value[type]) {
    componentKey.value[type] = 0;
  }
  componentKey.value[type] += 1;
};

const headerDialogLeaveProfile = ref();
const displayDialogLeaveProfile = ref(false);
const openDialogLeaveProfile = (item, str) => {
  profile.value = item;
  headerDialogLeaveProfile.value = str;
  displayDialogLeaveProfile.value = true;
  forceRerender(0);
};
const closeDialogLeaveProfile = () => {
  displayDialogLeaveProfile.value = false;
  forceRerender(0);
};

//function export
//Xuất excel
const menuButs = ref();
const itemButs = ref([
  {
    label: "Xuất Excel",
    icon: "pi pi-file-excel",
    command: (event) => {
      exportExcel();
    },
  },
  {
    label: "Nhập Excel",
    icon: "pi pi-file-excel",
    command: (event) => {
      importExcel(event);
    },
  },
]);
const toggleExport = (event) => {
  menuButs.value.toggle(event);
};
const exportExcel = () => {
  excel("table-leave", "bangphepnam" + options.value.year);
};
const excel = (id, name) => {
  var html = '<html xmlns:x="urn:schemas-microsoft-com:office:excel">';
  html += "<head><xml><x:ExcelWorkbook><x:ExcelWorksheets><x:ExcelWorksheet>";
  html += "<x:Name>Sheet1</x:Name>";
  html +=
    "<x:WorksheetOptions><x:Panes></x:Panes></x:WorksheetOptions></x:ExcelWorksheet>";
  html += "</x:ExcelWorksheets></x:ExcelWorkbook></xml></head><body>";
  html +=
    "<style>.cstd{font-family: Times New Roman; font-size: 21px; font-weight: 700; text-align: center; vertical-align: center;}</style><table><td colspan='" +
    (days.value.length + 3) +
    "' class='cstd'>BẢNG PHÉP NĂM '" +
    options.value.year +
    "'</td>";
  html += "</table>";
  html +=
    "<style>.cstd{font-family: Times New Roman; font-size: 21px; font-weight: 700; text-align: left; vertical-align: center;}</style>";
  html +=
    "<style>th,table,tr{font-family: Times New Roman; font-size: 18px; vertical-align: middle; text-align: left;}</style><table border='1'>";

  var htmltable = document.getElementById(id);
  html += htmltable.innerHTML;
  html += "</table></body></html>";
  var data_type = "data:application/vnd.ms-excel";
  var ua = window.navigator.userAgent;
  var msie = ua.indexOf("MSIE ");
  var fileName = name + ".xls";
  if (msie > 0 || !!navigator.userAgent.match(/Trident.*rv\:11\./)) {
    if (window.navigator.msSaveBlob) {
      var blob = new Blob([html], {
        type: "application/csv;charset=utf-8;",
      });
      navigator.msSaveBlob(blob, fileName);
    }
  } else {
    var blob2 = new Blob([html], {
      type: "application/csv;charset=utf-8;",
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

// Import excel
const linkformimport = "/Portals/Mau Excel/Mẫu Excel Phép năm.xlsx";
let files = [];
const displayImport = ref(false);
const importExcel = (type) => {
  displayImport.value = true;
};
const removeFile = (event) => {
  files = [];
};
const selectFile = (event) => {
  event.files.forEach((element) => {
    files.push(element);
  });
};
const upload = () => {
  displayImport.value = false;
  let formData = new FormData();
  for (var i = 0; i < files.length; i++) {
    let file = files[i];
    formData.append("files", file);
  }
  axios
    .post(baseURL + "/api/hrm_leave/import_excel", formData, config)
    .then((response) => {
      toast.success("Nhập dữ liệu thành công");
      initData(true);
    })
    .catch((error) => {
      swal.close();
      swal.fire({
        title: "Thông báo!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
      return;
    });
};
const downloadFile = (url) => {
  const a = document.createElement("a");
  a.href =
    basedomainURL +
    "/Viewer/DownloadFile?url=" +
    encodeURIComponent(url) +
    "&title=" +
    encodeURIComponent("Mẫu Excel Phép năm.xlsx");
  a.download = "Mẫu Excel Phép năm.xlsx";
  //a.target = "_blank";
  a.click();
  a.remove();
};

//init
const initDictionary = () => {
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_leave_dictionary",
            par: [{ par: "user_id", va: store.getters.user.user_id }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      if (response != null && response.data != null) {
        var data = response.data.data;
        if (data != null) {
          let tbs = JSON.parse(data);
          if (tbs[0] != null && tbs[0].length > 0) {
            var temp1 = [];
            addToArray(temp1, tbs[0], null, 0, "is_order");
            tbs[0] = temp1;
          }
          tbs[0].unshift({ organization_id: null, newname: "Tất cả" });
          if (tbs[1] != null && tbs[1].length > 0) {
            department.value = JSON.parse(JSON.stringify(tbs[1]));
            var temp2 = [];
            addToArray(
              temp2,
              department.value,
              options.value.filter_organization_id,
              0,
              "is_order"
            );
            department.value = temp2;
          }
          dictionarys.value = tbs;
        }
      }
    })
    .catch((error) => {
      swal.fire({
        title: "Thông báo!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
      return;
    });
};
const initData = (ref) => {
  selectedNodes.value = [];
  datas.value = [];
  if (ref) {
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
  }
  var departments = "";
  if (options.value.departments && options.value.departments.length > 0) {
    departments = options.value.departments
      .map((x) => x.organization_id)
      .join(",");
  }
  if (options.value.filter_organization_id == null) {
    departments = "";
    options.value.departments = [];
  }
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_leave_list",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "year", va: options.value.year },
              { par: "search", va: options.value.search },
              {
                par: "filter_organization_id",
                va: options.value.filter_organization_id,
              },
              {
                par: "departments",
                va: departments,
              },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      if (response != null && response.data != null) {
        let data = JSON.parse(response.data.data);
        if (data != null) {
          if (data[0] != null && data[0].length > 0) {
            var group = groupBy(data[0], "department_id");
            for (var k in group) {
              let obj = {
                department_id: k,
                department_name: group[k][0].department_name,
                users: group[k],
              };
              datas.value.push(obj);
            }
            var count = 1;
            datas.value.forEach((item) => {
              if (item.users != null && item.users.length > 0) {
                item.users.forEach((us) => {
                  us.stt = count++;
                });
              }
            });
          } else {
            datas.value = [];
          }
        }
      }
      if (isFirst.value) isFirst.value = false;
      swal.close();
      if (options.value.loading) options.value.loading = false;
    })
    .catch((error) => {
      swal.close();
      if (options.value.loading) options.value.loading = false;
      if (error && error.status === 401) {
        swal.fire({
          title: "Thông báo!",
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
        return;
      } else {
        swal.fire({
          title: "Thông báo!",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
        return;
      }
    });
};
const refresh = () => {
  selectedNodes.value = [];
  initData(true);
};
onMounted(() => {
  initDictionary();
  initData(true);
});
</script>
<template>
  <div class="surface-100 p-2" style="overflow: hidden">
    <Toolbar class="outline-none surface-0 border-none pb-0">
      <template #start>
        <div>
          <h3 class="module-title m-0">
            <i class="pi pi-calendar"></i> Bảng phép năm {{ options.year }}
          </h3>
        </div>
      </template>
      <template #end>
        <!-- <Button
          @click="openUpdateDialog('Chọn loại chấm công')"
          label="Chọn loại chấm công"
          icon="pi pi-check"
          class="mr-2"
        /> -->
        <Button
          @click="toggleExport"
          label="Tiện ích"
          icon="pi pi-file-excel"
          class="mr-2 p-button-outlined p-button-secondary"
          aria-haspopup="true"
          aria-controls="overlay_Export"
        />
        <Menu
          :model="itemButs"
          :popup="true"
          id="overlay_Export"
          ref="menuButs"
        />
        <Button
          @click="refresh()"
          class="p-button-outlined p-button-secondary"
          icon="pi pi-refresh"
          v-tooltip.top="'Tải lại'"
        />
      </template>
    </Toolbar>
    <Toolbar class="outline-none surface-0 border-none">
      <template #start>
        <span class="p-input-icon-left mr-2">
          <i class="pi pi-search" />
          <InputText
            @keypress.enter="search()"
            v-model="options.search"
            type="text"
            spellcheck="false"
            placeholder="Tìm kiếm"
            class="ip36 input-search"
          />
        </span>
        <Button
          @click="toggleFilter($event)"
          type="button"
          class="p-button-outlined p-button-secondary ip36"
          aria:haspopup="true"
          aria-controls="overlay_panel"
        >
          <div>
            <span class="mr-2"><i class="pi pi-filter"></i></span>
            <span class="mr-2">Lọc dữ liệu</span>
            <span><i class="pi pi-chevron-down"></i></span>
          </div>
        </Button>
        <OverlayPanel
          :showCloseIcon="false"
          ref="opfilter"
          appendTo="body"
          class="p-0 m-0"
          id="overlay_panel"
          :style="{ width: '400px' }"
        >
          <div class="grid formgrid m-0">
            <div
              class="col-12 md:col-12 p-0"
              :style="{
                minHeight: 'unset',
                maxHeight: 'calc(100vh - 300px)',
                overflow: 'auto',
              }"
            >
              <div class="row">
                <div class="col-12 md:col-12">
                  <div class="form-group">
                    <label>Đơn vị</label>
                    <Dropdown
                      :options="dictionarys[0]"
                      :filter="true"
                      :showClear="true"
                      :editable="false"
                      v-model="options.filter_organization_id"
                      @change="changeOrganization()"
                      optionLabel="newname"
                      optionValue="organization_id"
                      placeholder="Chọn đơn vị"
                      class="ip36"
                      :style="{
                        whiteSpace: 'nowrap',
                        overflow: 'hidden',
                        textOverflow: 'ellipsis',
                      }"
                    />
                  </div>
                </div>
                <div class="col-12 md:col-12">
                  <div class="form-group">
                    <label>Phòng ban</label>
                    <MultiSelect
                      :disabled="options.filter_organization_id == null"
                      :options="department"
                      :filter="true"
                      :showClear="true"
                      :editable="false"
                      v-model="options.departments"
                      optionLabel="newname"
                      placeholder="Chọn phòng ban"
                      class="w-full limit-width"
                      panelClass="d-design-dropdown"
                      :style="{ width: '200px', minHeight: '36px' }"
                    >
                      <template #value="slotProps">
                        <ul
                          class="p-ulchip"
                          v-if="slotProps.value && slotProps.value.length > 0"
                        >
                          <li
                            class="p-lichip"
                            v-for="(value, index) in slotProps.value"
                            :key="index"
                          >
                            <Chip class="mr-2 mb-2 px-3 py-2">
                              <div class="flex">
                                <div>
                                  <span>{{ value.newname }}</span>
                                </div>
                                <span
                                  tabindex="0"
                                  class="p-chip-remove-icon pi pi-times-circle format-flex-center"
                                  @click="
                                    removeFilter(index, options.departments);
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
            </div>
            <div class="col-12 md:col-12 p-0">
              <Toolbar
                class="border-none surface-0 outline-none px-0 pb-0 w-full"
              >
                <template #start>
                  <Button
                    @click="resetFilter()"
                    class="p-button-outlined"
                    label="Bỏ chọn"
                  ></Button>
                </template>
                <template #end>
                  <Button @click="filter($event)" label="Lọc"></Button>
                </template>
              </Toolbar>
            </div>
          </div>
        </OverlayPanel>
      </template>
      <template #end>
        <div class="form-group m-0 mr-2">
          <Calendar
            v-model="options.tempyear"
            @date-select="goYear(options.tempyear)"
            :showIcon="true"
            :manualInput="false"
            inputId="yearpicker"
            :showOnFocus="false"
            view="year"
            dateFormat="'Năm' yy"
            placeholder="Chọn năm"
            class="ip36"
          />
        </div>
      </template>
    </Toolbar>
    <div class="box-table gridline-custom scrollable-both-custom">
      <table id="table-leave" class="table-custom">
        <thead class="thead-custom">
          <tr>
            <th
              class="sticky text-center"
              :style="{ left: '0', top: '0', width: '50px' }"
            >
              STT
            </th>
            <th
              class="sticky text-center"
              :style="{ left: '50px', top: '0', width: '200px' }"
            >
              HỌ VÀ TÊN
            </th>
            <th
              class="text-center"
              :style="{ width: '70px' }"
              v-for="(item, month_key) in months"
            >
              {{ item.name }}
            </th>
            <th
              class="sticky text-center"
              :style="{ top: '0', width: '100px' }"
            >
              Phép tồn
            </th>
            <th
              class="sticky text-center"
              :style="{ top: '0', width: '100px' }"
            >
              Phép thưởng
            </th>
            <th
              class="sticky text-center"
              :style="{ top: '0', width: '100px' }"
            >
              Thâm niên
            </th>
            <th
              class="sticky text-center"
              :style="{ top: '0', width: '100px', backgroundColor: '#F2FBE6' }"
            >
              TỔNG SỐ
            </th>
            <th
              class="sticky text-center"
              :style="{ top: '0', width: '100px', backgroundColor: '#EEFAF5' }"
            >
              ĐÃ NGHỈ
            </th>
            <th
              class="sticky text-center"
              :style="{ top: '0', width: '100px', backgroundColor: '#FDF2F0' }"
            >
              CÒN LẠI
            </th>
          </tr>
        </thead>
        <tbody
          class="tbody-custom"
          v-for="(group, group_key) in datas"
          :key="group_key"
        >
          <tr>
            <td
              colspan="10"
              class="sticky"
              :style="{
                left: 0,
                background: '#DEE6F0',
              }"
            >
              <b>{{ group.department_name }}</b>
            </td>
            <td
              :colspan="months.length"
              :style="{
                background: '#DEE6F0',
              }"
            ></td>
          </tr>
          <tr
            v-for="(user, user_key) in group.users"
            :key="user_key"
            @click="openDialogLeaveProfile(user)"
            class="hover"
          >
            <td
              class="sticky"
              :style="{
                left: '0',
                width: '50px',
                background: '#f8f9fa',
                textAlign: 'center',
              }"
            >
              {{ user.stt }}
            </td>
            <td
              class="sticky text-left"
              :style="{
                left: '50px',
                width: '200px',
                backgroundColor: '#fff',
              }"
            >
              <b>{{ user.profile_user_name }}</b>
            </td>
            <td
              class="text-center"
              :style="{
                width: '90px',
                backgroundColor: '#fff',
              }"
              v-for="(item, month_key) in months"
            >
              <span> {{ user["month" + item.month] }}</span>
            </td>
            <td
              class="text-center"
              :style="{
                width: '150px',
                backgroundColor: '#fff',
              }"
            >
              <span> {{ user.leaveInventory }}</span>
            </td>
            <td
              class="text-center"
              :style="{
                width: '150px',
                backgroundColor: '#fff',
              }"
            >
              <span> {{ user.leaveBonus }}</span>
            </td>
            <td
              class="text-center"
              :style="{
                width: '150px',
                backgroundColor: '#fff',
              }"
            >
              <span> {{ user.leaveSeniority }}</span>
            </td>
            <td
              class="text-center"
              :style="{
                width: '150px',
                backgroundColor: '#fff',
                backgroundColor: '#F2FBE6',
              }"
            >
              <b> </b>
            </td>
            <td
              class="text-center"
              :style="{
                width: '150px',
                backgroundColor: '#fff',
                backgroundColor: '#EEFAF5',
              }"
            >
              <b> {{ user.leaveAll }}</b>
            </td>
            <td
              class="text-center"
              :style="{
                width: '150px',
                backgroundColor: '#fff',
                backgroundColor: '#FDF2F0',
              }"
            >
              <b> {{ user.leaveRemain }}</b>
            </td>
          </tr>
        </tbody>
      </table>
      <div
        v-if="!options.loading && datas.length == 0"
        class="align-items-center justify-content-center p-4 text-center m-auto"
        style="
          display: flex;
          width: 100%;
          height: calc(100vh - 230px);
          background-color: #fff;
        "
      >
        <div>
          <img src="../../../assets/background/nodata.png" height="144" />
          <h3 class="m-1">Không có dữ liệu</h3>
        </div>
      </div>
    </div>
    <!-- <div class="d-lang-table">
      <DataTable
        @row-select="() => {}"
        :value="datas"
        :scrollable="true"
        :lazy="true"
        :rowHover="true"
        :showGridlines="false"
        :globalFilterFields="['profile_name']"
        v-model:selection="selectedNodes"
        selectionMode="single"
        dataKey="profile_id"
        scrollHeight="flex"
        filterDisplay="menu"
        filterMode="lenient"
        paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
        responsiveLayout="scroll"
      >
        <Column
          header="STT"
          headerStyle="text-align:center;max-width:50px;height:50px"
          bodyStyle="text-align:center;max-width:50px;"
          class="align-items-center justify-content-center text-center"
        >
          <template #body="slotProps">
            {{ slotProps.index + 1 }}
          </template>
        </Column>
        <Column
          field="profile_name"
          header="Họ và tên"
          headerStyle="text-align:center;max-width:200px;height:50px"
          bodyStyle="text-align:center;max-width:200px;"
          class="align-items-center justify-content-center text-center"
        >
        </Column>
        <Column
          field="profile_name"
          header="Họ và tên"
          headerStyle="text-align:center;max-width:100px;height:50px"
          bodyStyle="text-align:center;max-width:100px;"
          class="align-items-center justify-content-center text-center"
        >
        </Column>
        <template #empty>
          <div
            class="align-items-center justify-content-center p-4 text-center m-auto"
            style="
              display: flex;
              width: 100%;
              height: calc(100vh - 180px);
              background-color: #fff;
            "
          >
            <div v-if="!options.loading && options.total == 0">
              <img src="../../../assets/background/nodata.png" height="144" />
              <h3 class="m-1">Không có dữ liệu</h3>
            </div>
          </div>
        </template>
      </DataTable>
    </div> -->
  </div>

  <!--dialog-->
  <dialogleaveprofile
    :key="componentKey['0']"
    :headerDialog="headerDialogLeaveProfile"
    :displayDialog="displayDialogLeaveProfile"
    :closeDialog="closeDialogLeaveProfile"
    :profile="profile"
    :year="options.year"
    :initData="initData"
  />

  <Dialog
    header="Tải lên file Excel"
    v-model:visible="displayImport"
    :style="{ width: '40vw' }"
    :closable="true"
    :modal="true"
  >
    <h3>
      <label>
        <a @click="downloadFile(linkformimport)">Nhấn vào đây</a> để tải xuống
        tệp mẫu.
      </label>
    </h3>
    <form>
      <FileUpload
        accept=".xls,.xlsx"
        @remove="removeFile"
        @select="selectFile"
        :multiple="false"
        :show-upload-button="false"
        choose-label="Chọn tệp"
        cancel-label="Hủy"
      >
        <template #empty>
          <p>Kéo và thả tệp vào đây để tải lên.</p>
        </template>
      </FileUpload>
    </form>
    <template #footer>
      <Button label="Lưu" icon="pi pi-check" @click="upload" />
    </template>
  </Dialog>
</template>
<style scoped>
.box-table {
  height: calc(100vh - 160px) !important;
  background-color: #fff;
  overflow: auto;
}
.thead-custom > tr > th + th {
  border-left-width: 0;
}
.tbody-custom > tr + tr > td,
.tbody-custom > tr:first-child > td {
  border-top-width: 0;
}
.tbody-custom > tr > td + td {
  border-left-width: 0;
}
.table-custom {
  border-collapse: collapse;
  width: 100%;
  table-layout: fixed;
}
.thead-custom {
  position: sticky;
  top: 0;
  z-index: 2;
}
.thead-custom > tr > th {
  padding: 1rem 0.5rem;
  border: 1px solid #e9ecef;
  border-width: 0 0 1px 0;
  font-weight: 600;
  color: #495057;
  background: #f8f9fa;
  transition: box-shadow 0.2s;
}
.thead-custom > tr > th {
  border-width: 1px;
}
.tbody-custom > tr > td {
  border: 1px solid #e9ecef;
  border-width: 0 0 1px 0;
  padding: 1rem;
}
.tbody-custom > tr > td {
  border-width: 1px;
}

.scrollable-both-custom .thead-custom > tr > th,
.scrollable-both-custom .tbody-custom > tr > td,
.scrollable-both-custom .tfoot-custom > tr > td,
.p-datatable-scrollable-horizontal
  .p-datatable-thead
  > tr
  > th
  .p-datatable-scrollable-horizontal
  .p-datatable-tbody
  > tr
  > td,
.p-datatable-scrollable-horizontal .p-datatable-tfoot > tr > td {
  -webkit-box-flex: 0;
  -ms-flex: 0 0 auto;
  flex: 0 0 auto;
}
.sticky {
  display: sticky;
}
th.sticky {
  z-index: 2;
}
td.sticky {
  z-index: 1;
}
.btn-hover-true:hover {
  cursor: pointer;
  background: aliceblue;
  color: #000;
}
.btn-selected {
  background: aliceblue !important;
}
.icon-selected {
  transition: 0.5s ease;
  opacity: 1;
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  -ms-transform: translate(-50%, -50%);
  text-align: center;
}
.icon-selected > i {
  color: blue;
  font-size: 16px;
}
.style-0 {
  background-color: #52be80;
  color: #fff;
}
.style-1 {
  background-color: #eb984e;
  color: #fff;
}
.style-2 {
  background-color: #af7ac5;
  color: #fff;
}
.style-3 {
  background-color: #ec7063;
  color: #fff;
}

th.isHoliday {
  background-color: #f1948a !important;
  color: #fff !important;
}
.isHoliday {
  background-color: #f1948a;
  color: #fff;
}

.form-group {
  display: grid;
  margin-bottom: 1rem;
  flex: 1;
}

.form-group > label {
  margin-bottom: 0.5rem;
}

.form-group .p-multiselect .p-multiselect-label,
.form-group .p-dropdown .p-dropdown-label,
.form-group .p-treeselect .p-treeselect-label {
  height: 100%;
  display: flex;
  align-items: center;
}
.p-ulchip {
  margin: 0;
  margin-top: 0.5rem;
  padding: 0;
  list-style: none;
}

.p-lichip {
  float: left;
  white-space: normal;
}
.hover {
  cursor: pointer;
}
.hover:hover td {
  background-color: aliceblue !important;
}
</style>
<style lang="scss" scoped>
::v-deep(.d-lang-table) {
  .p-datatable-thead .justify-content-center .p-column-header-content {
    justify-content: center !important;
  }

  .p-datatable-table {
    position: absolute;
  }

  .p-datatable-thead {
    position: sticky;
    top: 0;
    z-index: 1;
  }
}
::v-deep(.form-group) {
  .p-multiselect .p-multiselect-label,
  .p-dropdown .p-dropdown-label {
    height: 100%;
    display: flex;
    align-items: center;
  }
  .p-chip img {
    margin: 0;
  }
  .p-avatar-text {
    font-size: 1rem;
  }
}
::v-deep(.table) {
  border-collapse: collapse;
  width: 100%;
  table-layout: fixed;
  tr th {
    background-color: #f8f9fa;
  }
}
::v-deep(.border) {
  tr th,
  tr td {
    border: solid 1px rgba(0, 0, 0, 0.2);
    padding: 0.5rem;
  }
}
</style>
