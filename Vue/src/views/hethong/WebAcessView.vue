<script setup>
import { ref, inject, onMounted } from "vue";
import { useToast } from "vue-toastification";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import moment from "moment";
import { encr } from "../../util/function.js";
import router from "@/router";
const cryoptojs = inject("cryptojs");
const store = inject("store");

const tdfrom_device = ref(["PC", "Android", "IOS", "Laptop"]);
const treedonvis = ref([]);
const datalists = ref();
const tdUsers = ref([]);
const displayAddData = ref(false);
const isFirst = ref(true);
const filterSQL = ref([]);
const isDynamicSQL = ref(true); //phân trang bình thường hay phân trang tối ưu cho dữ liệu lớn
const toast = useToast();
const swal = inject("$swal");
const axios = inject("axios"); // inject axios
const opition = ref({
  IsNext: true,
  sort: "web_acess_id DESC",
  PageNo: 0,
  PageSize: 20,
  Filteruser_id: null,
  user_id: store.getters.user_id,
  department_id: store.getters.user.organization_id,
});
const basedomainURL = fileURL;
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
//Khai báo biến
var is_permission = 1, report_module_id;
var is_reportmodule = false;
const selectCapcha = ref();
selectCapcha.value = {};
selectCapcha.value[store.getters.user.organization_id] = true;
const filters = ref({
  user_id: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
  is_time: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.DATE_IS }],
  },
  from_ip: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
  from_device: {
    operator: FilterOperator.OR,
    constraints: [{ value: null, matchMode: FilterMatchMode.EQUALS }],
  },
  full_name: { value: null, matchMode: FilterMatchMode.IN },
});
const first = ref(0);
//lọc
const taskDateFilter = ref();
const filterButs = ref();
const checkFilter=ref(false);
const todayClick = () => {
  taskDateFilter.value = [];
  taskDateFilter.value.push(new Date());
};
const delDayClick = () => {
  taskDateFilter.value = [];
  opition.value.start_date = null;
  opition.value.end_date = null;
  loadData(true);
};
const onDayClick = () => {
  if (taskDateFilter.value == null) taskDateFilter.value = [];
  else {
    opition.value.start_date = taskDateFilter.value[0];
    opition.value.end_date = taskDateFilter.value[1];
    loadData(true);
  }
};
//Khai báo function
const toggleFilter = (event) => {
 
  filterButs.value.toggle(event);
};
const hideFilter = () => {
  if(!(options.value.is_hot!=null||options.value.status!=null||
  options.value.news_type!=null))
  checkFilter.value=false;
 
};

//Show Modal
const showModalDetail = () => {
  displayAddData.value = true;
};
const closedisplayDetail = () => {
  displayAddData.value = false;
};
const onRefersh = () => {
  selectCapcha.value = {};
  selectCapcha.value[store.getters.user.user_id || -1] = true;
  opition.value.search = "";
  taskDateFilter.value = [];
  opition.value = {
    IsNext: true,
    sort: "web_acess_id DESC",
    PageNo: 0,
    IsLess: true,
    PageSize: 20,
    Filteruser_id: null,
    user_id: store.getters.user.user_id,
    start_date:null,
    end_date:null,
  };
    first.value=0;
  //isDynamicSQL.value = false;
  filterSQL.value = [];
  loadData(true);
};
const onSearch = () => {
  //isDynamicSQL.value = false;
  opition.value.PageNo = 0;
  first.value = 0;
  opition.value.web_acess_id = null;
  opition.value.IsNext = true;
  opition.value.sort = "web_acess_id DESC";
  loadData(true);
};
const addLog = (log) => {
  axios.post(baseURL + "/api/Proc/AddLog", log, config);
};
const initTudien = () => {
  axios
    .post(
        baseURL + "/api/Users/GetDataProc",
        {
          str: encr(JSON.stringify({
            proc: "sys_webacess_dictionary",
        par: [{ par: "user_id", va: store.getters.user.user_id }],
          }), SecretKey, cryoptojs
          ).toString()
        },
        config
      )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        let obj = renderTreeDV(
          data[0],
          "organization_id",
          "organization_name",
          "phòng ban"
        );
        treedonvis.value = obj.arrtreeChils;
      } else {
        donvis.value = [];
      }
    })
    .catch((error) => {});
};
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
const filterUser = () => {
  opition.value.PageNo = 0;
  checkFilter.value = true;
    let keys = Object.keys(selectCapcha.value);
  opition.value.department_id = parseInt(keys[0]);
  loadData(true);
};
const reFilterUser = () => {
    opition.value.PageNo = 0;
  selectCapcha.value = {};
  selectCapcha.value[store.getters.user.organization_id || -1] = true;
  checkFilter.value = false;
  first.value=0;
  loadData(true);
};
const loadCount = () => {
  axios
    .post(
        baseURL + "/api/Users/GetDataProc",
        {
          str: encr(JSON.stringify({
            proc: "sys_webacess_count1",
        par: [
          { par: "search", va: opition.value.search },
          { par: "users_id", va: store.getters.user.user_id  },
          { par: "filterusers_id", va: opition.value.Filteruser_id },
          { par: "department_id", va: opition.value.department_id },
          { par: "pageno", va: opition.value.PageNo },
          { par: "pagesize", va: opition.value.PageSize },
          { par: "fromdivice", va: opition.value.from_device },
          { par: "ip", va: opition.value.IP },
          { par: "startdate", va: opition.value.start_date },
          { par: "enddate", va: opition.value.end_date },
          { par: "is_reportmodule", va: is_reportmodule },
          { par: "report_module_id", va: report_module_id },
          { par: "is_permission", va: is_permission },
        ],
          }), SecretKey, cryoptojs
          ).toString()
        },
        config
      )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        opition.value.totalRecords = data[0][0].totalrecords;
        tdUsers.value= data[1]
      }
    })
    .catch((error) => {
      addLog({
        title: "Lỗi Console loadCount",
        controller: "WebAcessView.vue",
        logcontent: error.message,
        loai: 2,
      });
    });
};
const onPage = (event) => {
  if (event.rows != opition.value.PageSize) {
    opition.value.PageSize = event.rows;
  }
  if (event.page == 0) {
    //Trang đầu
    opition.value.web_acess_id = null;
    opition.value.IsNext = true;
  } else if (event.page == event.pageCount - 1) {
    //Trang cuối
    opition.value.web_acess_id = -1;
    opition.value.IsNext = false;
  } else if (event.page > opition.value.PageNo) {
    //Trang sau
    opition.value.web_acess_id =
      datalists.value[datalists.value.length - 1].web_acess_id;
    opition.value.IsNext = true;
  } else if (event.page < opition.value.PageNo) {
    //Trang trước
    opition.value.web_acess_id = datalists.value[0].web_acess_id;
    opition.value.IsNext = false;
  }
  opition.value.PageNo = event.page;
  loadData(true);
};
const onSort = (event) => {
  opition.value.sort =
    event.sortField + (event.sortOrder == 1 ? " ASC" : " DESC");
  if (event.sortField != "web_acess_id") {
    opition.value.sort +=
      ",web_acess_id " + (event.sortOrder == 1 ? " ASC" : " DESC");
  }
  isDynamicSQL.value = true;
  loadDataSQL();
};
const onFilter = (event) => {
  filterSQL.value = [];
  for (const [key, value] of Object.entries(event.filters)) {
    if (key != "global") {
      let obj = {
        key: key == "full_name" ? "user_id" : key,
        filteroperator: value.operator,
        filterconstraints: value.constraints,
      };
      if (value.value && value.value.length > 0) {
        obj.filteroperator = value.matchMode;
        obj.filterconstraints = [];
        value.value.forEach(function (vl) {
          obj.filterconstraints.push({ value: vl[obj.key] });
        });
      } else if (value.matchMode) {
        obj.filteroperator = "and";
        obj.filterconstraints = [value];
      }
      if (
        obj.filterconstraints &&
        obj.filterconstraints.filter((x) => x.value != null).length > 0
      )
        filterSQL.value.push(obj);
    }
  }
  opition.value.PageNo = 0;
  first.value = 0;
  opition.value.web_acess_id = null;
  isDynamicSQL.value = true;
  loadDataSQL();
};
const loadDataSQL = () => {
  let data = {
    id: opition.value.web_acess_id,
    next: opition.value.IsNext,
    sqlO: opition.value.sort,
    Search: opition.value.search,
    PageNo: opition.value.PageNo,
    PageSize: opition.value.PageSize,
    fieldSQLS: filterSQL.value,
  };
  opition.value.loading = true;
  axios
    .post(baseURL + "/api/SQL/FilterSQLsys_web_acess", data, config)
    .then((response) => {
      let dt = JSON.parse(response.data.data);
      let data = dt[0];
      if (data.length > 0) {
        data.forEach((element, i) => {
          element.STT = opition.value.PageNo * opition.value.PageSize + i + 1;
          element.Ngay = moment(new Date(element.is_time)).format(
            "DD/MM/YYYY HH:mm:ss"
          );
        });
        datalists.value = data;
      } else {
        datalists.value = [];
      }
      if (isFirst.value) isFirst.value = false;
      opition.value.loading = false;
      //Show Count nếu có
      if (dt.length == 2) {
        opition.value.totalRecords = dt[1][0].totalRecords;
      }
    })
    .catch((error) => {
      opition.value.loading = false;
      toast.error("Tải dữ liệu không thành công!");
      addLog({
        title: "Lỗi Console loadData",
        controller: "WebAcessView.vue",
        logcontent: error.message,
        loai: 2,
      });
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const getReportModuleId = ()=>{
  axios
    .post(
      baseURL + "/api/Modules/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "report_modules_get_id1",
            par: [
              { par: "@is_link", va: router.currentRoute.value.fullPath },
              { par: "@user_id", va: store.getters.user.user_id },
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
      if (data.length > 0) {
        is_permission = data[0].is_permission;
        report_module_id = data[0].report_module_id;
      }
    })
    .catch((error) => {
      if (error && error.status === 401) {
        errorMessage();
      }
    });
}
const loadData = (rf) => {
  // if (isDynamicSQL.value) {
  //   loadDataSQL();
  //   return false;
  // }
  if(router.currentRoute.value.fullPath.indexOf('/reportmodule') !== -1){
   is_reportmodule = true;
   getReportModuleId()
  }
  opition.value.loading = true;
  if (rf) {
    if (opition.value.PageNo == 0) {
      loadCount();
    }
  }
  let proc = "sys_webacess_listseek1";
  let datas = [
    { par: "web_acess_id", va: opition.value.web_acess_id },
    { par: "isnext", va: opition.value.IsNext },
    { par: "search", va: opition.value.search },
    { par: "user_id", va: store.getters.user.user_id  },
    { par: "filteruser_id", va: opition.value.Filteruser_id },
    { par: "department_id", va: opition.value.department_id },
    { par: "pageno", va: opition.value.PageNo },
    { par: "pagesize", va: opition.value.PageSize },
    { par: "fromdivice", va: opition.value.from_device },
    { par: "ip", va: opition.value.IP },
    { par: "start_date", va: opition.value.start_date },
    { par: "end_date", va: opition.value.end_date },
    { par: "is_reportmodule", va: is_reportmodule },
    { par: "report_module_id", va: report_module_id },
    { par: "type_report", va: is_permission },
  ];
  axios
    .post(
        baseURL + "/api/Users/GetDataProc",
        {
          str: encr(JSON.stringify({
            proc: proc,
        par: datas,
          }), SecretKey, cryoptojs
          ).toString()
        },
        config
      )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        data.forEach((element, i) => {
          element.STT = opition.value.PageNo * opition.value.PageSize + i + 1;
          element.Ngay = moment(new Date(element.is_time)).format(
            "DD/MM/YYYY HH:mm:ss"
          );
        });
        datalists.value = data;
      } else {
        datalists.value = [];
      }
      if (isFirst.value) isFirst.value = false;
      opition.value.loading = false;
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      opition.value.loading = false;
      addLog({
        title: "Lỗi Console loadData",
        controller: "WebAcessView.vue",
        logcontent: error.message,
        loai: 2,
      });
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};

const exportData = (method) => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios
    .post(
      baseURL + "/api/Excel/ExportExcel",
      {
        excelname: "LỊCH SỬ TRUY CẬP",
        proc: "Sys_WebAcess_ListExport",
        par: [
          { par: "Search", va: opition.value.search },
          { par: "user_id", va: opition.value.user_id },
          { par: "Filteruser_id", va: opition.value.Filteruser_id },
          { par: "from_device", va: opition.value.from_device },
          { par: "IP", va: opition.value.IP },
          { par: "StartDate", va: opition.value.StartDate },
          { par: "EndDate", va: opition.value.EndDate },
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
            let pathReplace = response.data.path.replace(/\\+/g, '/').replace(/\/+/g, '/').replace(/^\//g, '');
            var listPath = pathReplace.split('/');
            var pathFile = "";
            listPath.forEach(item => {
              if (item.trim() != "")
              {
                  pathFile += "/" + item;
              }
            });
            window.open(baseURL + pathFile);
          }
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
      if (error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};

onMounted(() => {
  //init
  initTudien();
  loadData(true);
  return {};
});
</script>
<template>
  <div
    class="main-layout true flex flex-column flex-grow-1 p-2"
    v-if="store.getters.islogin"
  >
    <DataTable
      class="w-full p-datatable-sm e-sm cursor-pointer"
      :lazy="true"
      @page="onPage($event)"
      @filter="onFilter($event)"
      @sort="onSort($event)"
      :value="datalists"
      :loading="opition.loading"
      :paginator="true"
      :rows="opition.PageSize"
      :totalRecords="opition.totalRecords"
      :pageLinkSize="1"
      dataKey="web_acess_id"
      :rowHover="true"
      v-model:filters="filters"
      filterDisplay="menu"
      :showGridlines="true"
      filterMode="lenient"
      paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
      :rowsPerPageOptions="[ 20,30, 50, 100, 200]"
      :currentPageReportTemplate="
        isDynamicSQL ? '{currentPage}' : '{currentPage}/{totalPages}'
      "
      responsiveLayout="scroll"
      :scrollable="true"
      scrollHeight="flex"
      v-model:first="first"
    >
      <template #header>
        <h3 class="module-title mt-0 ml-1 mb-2">
          <i class="pi pi-database"></i> Lịch sử truy cập
          <span v-if="opition.totalRecords > 0"
            >({{ opition.totalRecords.toLocaleString() }})</span
          >
        </h3>
        <div class="flex justify-content-center align-items-center">
          <Toolbar class="w-full custoolbar">
            <template #start>
              <span class="p-input-icon-left">
                <i class="pi pi-search" />
                <InputText
                  type="text"
                  spellcheck="false"
                  v-model="opition.search"
                  placeholder="Tìm kiếm"
                  v-on:keyup.enter="onSearch"
                />
              </span>
              <Calendar
                placeholder="Lọc theo ngày"
                id="range"
                v-model="taskDateFilter"
                :showIcon="true"
                selectionMode="range"
                class="mx-2"
                :manualInput="false"
              >
                <template #footer>
                  <div class="w-full flex">
                    <div class="w-4 format-center">
                      <span
                        @click="todayClick"
                        class="cursor-pointer text-primary"
                        >Hôm nay</span
                      >
                    </div>
                    <div class="w-4 format-center">
                      <Button @click="onDayClick" label="Thực hiện"></Button>
                    </div>
                    <div class="w-4 format-center">
                      <span
                        @click="delDayClick"
                        class="cursor-pointer text-primary"
                        >Xóa</span
                      >
                    </div>
                  </div>
                </template>
              </Calendar>
                          <Button
              :class="
                checkFilter
                  ? 'ml-2'
                  : 'ml-2 p-button-secondary p-button-outlined'
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
              style="width: 350px"
              :breakpoints="{ '960px': '20vw' }"
            >
              <div class="grid formgrid m-2">
                <div class="field col-12 md:col-12 flex align-items-center">
                  <div class="col-4 p-0">Phòng ban:</div>
                  <TreeSelect
                    class="col-8 p-0"
                    v-model="selectCapcha"
                    :options="treedonvis"
                    :showClear="true"
                    :max-height="200"
                    placeholder="Chọn phòng ban"
                    optionLabel="organization_name"
                    optionValue="organization_id"
                  >
                  </TreeSelect>
                </div>
                <div class="col-12 field p-0">
                  <Toolbar class="toolbar-filter">
                    <template #start>
                      <Button
                        @click="reFilterUser"
                        class="p-button-outlined"
                        label="Xóa"
                      ></Button>
                    </template>
                    <template #end>
                      <Button @click="filterUser" label="Lọc"></Button>
                    </template>
                  </Toolbar>
                </div>
              </div>
            </OverlayPanel>
            </template>

            <template #end>
              <Button
                class="mr-2 p-button-outlined p-button-secondary"
                icon="pi pi-refresh"
                @click="onRefersh"
              />
              <Button
                label="Export"
                icon="pi pi-file-excel"
                class="mr-2 p-button-outlined p-button-secondary"
                @click="exportData('ExportExcel')"
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
      </template>
      <Column
        field="web_acess_id"
        header="Mã truy cập"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:150px"
        bodyStyle="text-align:center;max-width:150px"
      ></Column>
      <Column
        :sortable="true"
        field="user_id"
        header="Tài khoản"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:150px"
        bodyStyle="text-align:center;max-width:150px"
      >
        <template #body="{ data }">
          {{ data.user_id }}
        </template>
        <template #filter="{ filterModel }">
          <InputText
            type="text"
            v-model="filterModel.value"
            class="p-column-filter"
            placeholder="Từ khoá"
          />
        </template>
      </Column>
      <Column
        :sortable="true"
        field="full_name"
        filterField="full_name"
        header="Tên người dùng"
        :showFilterMatchModes="false"
      >
        <template #body="{ data }">
          <span class="image-text">{{ data.full_name }}</span>
        </template>
        <template #filter="{ filterModel }">
          <MultiSelect
            v-model="filterModel.value"
            :options="tdUsers"
            optionLabel="full_name"
            placeholder="Chọn user"
            class="p-column-filter"
          >
            <template #option="slotProps">
              <div class="p-multiselect-representative-option">
                <Avatar
                  v-bind:label="
                    slotProps.option.avatar
                      ? ''
                      : slotProps.option.full_name.substring(0, 1)
                  "
                  v-bind:image="basedomainURL + slotProps.option.avatar"
                  style="
                    background-color: #2196f3;
                    color: #ffffff;
                    vertical-align: middle;
                  "
                  class="mr-2"
                  size="small"
                  shape="circle"
                />
                <span class="image-text">{{ slotProps.option.full_name }}</span>
              </div>
            </template>
          </MultiSelect>
        </template>
      </Column>
      <Column
        :sortable="true"
        field="is_time"
        dataType="date"
        header="Ngày truy cập"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:200px"
        bodyStyle="text-align:center;max-width:200px"
      >
        <template #body="{ data }">
          {{ data.Ngay }}
        </template>
        <!-- <template #filter="{ filterModel }">
          <Calendar v-model="filterModel.value" />
        </template> -->
      </Column>
      <Column
        :sortable="true"
        field="from_ip"
        header="IP"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:100px"
        bodyStyle="text-align:center;max-width:100px"
      >
        <template #body="{ data }">
          {{ data.from_ip }}
        </template>
        <!-- <template #filter="{ filterModel }">
          <InputText
            type="text"
            v-model="filterModel.value"
            class="p-column-filter"
            placeholder="Từ khoá"
          />
        </template> -->
      </Column>
      <Column
        :sortable="true"
        field="from_device"
        header="Thiết bị"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:140px"
        bodyStyle="text-align:center;max-width:140px"
      >
        <template #body="{ data }">
          <span>{{ data.from_device }}</span>
        </template>
        <template #filter="{ filterModel }">
          <Dropdown
            v-model="filterModel.value"
            :options="tdfrom_device"
            placeholder="Chọn thiết bị"
            class="p-column-filter"
            :showClear="true"
          >
            <template #value="slotProps">
              <span
                :class="'customer-badge status-' + slotProps.value"
                v-if="slotProps.value"
                >{{ slotProps.value }}</span
              >
              <span v-else>{{ slotProps.placeholder }}</span>
            </template>
            <template #option="slotProps">
              <span>{{ slotProps.option }}</span>
            </template>
          </Dropdown>
        </template>
      </Column>
      <template #empty>
        <div
          class="
            align-items-center
            justify-content-center
            p-4
            text-center
            w-full
          "
          v-if="!isFirst"
        >
          <img src="../../assets/background/nodata.png" height="144" />
          <h3 class="m-1">Không có dữ liệu</h3>
        </div>
      </template>
    </DataTable>
  </div>
  <Dialog
    header="Chi tiết"
    v-model:visible="displayAddData"
    :style="{ width: '480px', zIndex: 1000 }"
    :maximizable="true"
    :autoZIndex="false"
    :modal="true"
  >
    <template #footer>
      <Button label="Đóng lại" icon="pi pi-times" @click="closedisplayDetail" />
    </template>
  </Dialog>
</template>
<style>
.toolbar-filter {
  border: unset;
  outline: unset;
  background-color: #fff;
  padding-bottom: 0px;
}
</style>
<style lang="scss" scoped>
::v-deep(.p-treetable-tbody) {
  tr {
    cursor: pointer;
  }
}
::v-deep(.p-calendar) {
    .p-inputtext {
        width:170px !important;
    }

}
</style>