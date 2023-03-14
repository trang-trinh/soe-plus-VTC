<script setup>
import { ref, inject, onMounted } from "vue";
import { useToast } from "vue-toastification";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import moment from "moment";
import { encr } from "../../util/function.js";
const cryoptojs = inject("cryptojs");
//Khai báo biến
const filters = ref({
  global: { value: null, matchMode: FilterMatchMode.CONTAINS },
  user_id: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
  logdate: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.DATE_IS }],
  },
  IP: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
  loai: { value: null, matchMode: FilterMatchMode.EQUALS },
  full_name: { value: null, matchMode: FilterMatchMode.IN },
  title: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
});
const first = ref(0);
const tdLoais = ref([
  { id: 0, name: "Lỗi" },
  { id: 1, name: "Log" },
  { id: 2, name: "Lỗi Console" },
]);
const modelview = ref({});
const datalists = ref();
const tdUsers = ref([]);
const displayAddData = ref(false);
const isFirst = ref(true);
const filterSQL = ref([]);
const isDynamicSQL = ref(true); //phân trang bình thường hay phân trang tối ưu cho dữ liệu lớn
const toast = useToast();
const swal = inject("$swal");
const store = inject("store");
const axios = inject("axios"); // inject axios
const opition = ref({
  IsNext: true,
  sort: "id DESC",
  PageNo: 0,
  PageSize:20,
  Filteruser_id: null,
  user_id: store.getters.user_id,
});
const basedomainURL = fileURL;
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const menuButs = ref();
const itemButs = ref([
  {
    label: "Xuất Excel",
    icon: "pi pi-file-excel",
    command: (event) => {
      exportData("ExportExcel");
    },
  },
]);
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
  opition.value.StartDate = null;
  opition.value.EndDate = null;
  loadData(true);
};
const onDayClick = () => {
  if (taskDateFilter.value == null) taskDateFilter.value = [];
  else {
    opition.value.StartDate = taskDateFilter.value[0];
    opition.value.EndDate = taskDateFilter.value[1];
    opition.value.PageNo=0;
    loadData(true);
  }
};
//Khai báo function
const toggleExport = (event) => {
  menuButs.value.toggle(event);
};
//Show Modal
const showModalDetail = (md) => {
  if (md.log_content != null) {
    showDetailLog(md);
    return false;
  }
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios
    .post(
        baseURL + "/api/Modules/GetDataProc",
        {
          str: encr(JSON.stringify({
            proc: "sys_logs_get",
        par: [
          { par: "user_id", va: opition.value.user_id },
          { par: "id", va: md.id },
        ],
          }), SecretKey, cryoptojs
          ).toString()
        },
        config
      )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        md = data[0][0];
        let mdx = datalists.value.find((x) => x.id == md.id);
        mdx.log_content = md.log_content;
        showDetailLog(md);
      }
      swal.close();
    })
    .catch((error) => {
      swal.close();
    });
};
const showDetailLog = (md) => {
  let obj = {};
  let erros = JSON.parse(md.log_content);
  obj.contents = erros.contents;
  if (erros.data) {
    let arres = [];
    if (typeof erros.data == "string") {
      let objstr;
      try {
        objstr = JSON.parse(erros.data);
      } catch (error) {}
      if (objstr) {
        for (const [key, value] of Object.entries(objstr)) {
          if (key != "par") {
            arres.push({
              key: key,
              value: value,
              error: obj.contents && obj.contents.includes(`'${key}'`),
            });
          } else {
            value.forEach(function (d) {
              arres.push({
                key: d.par,
                value: d.va,
                error: obj.contents && obj.contents.includes(`'${d.par}'`),
              });
            });
          }
        }
      } else {
        arres.push({ key: "String", value: erros.data });
      }
    } else if (!(erros.data instanceof Object)) {
      JSON.parse(erros.data).forEach((element, i) => {
        arres.push({ key: i, value: JSON.stringify(element) });
      });
    } else {
      for (const [key, value] of Object.entries(erros.data)) {
        if (key != "par") {
          arres.push({
            key: key,
            value: value,
            error: obj.contents && obj.contents.includes(`'${key}'`),
          });
        } else {
          value.forEach(function (d) {
            arres.push({
              key: d.par,
              value: d.va,
              error: obj.contents && obj.contents.includes(`'${d.par}'`),
            });
          });
        }
      }
    }
    obj.data = arres;
  }
  obj.title = md.title;
  modelview.value = obj;
  displayAddData.value = true;
};
const closedisplayDetail = () => {
  displayAddData.value = false;
};
//Thêm sửa xoá
const onRefersh = () => {
  opition.value.search = "";
  taskDateFilter.value = [];
  opition.value = {
    IsNext: true,
    sort: "id DESC",
    PageNo: 0,
    PageSize: 20,
    Filteruser_id: null,
    user_id: store.getters.user.user_id,
    StartDate: null,
    EndDate: null,
  };
  first.value=0;
  isDynamicSQL.value = false;
  filterSQL.value = [];
  loadData(true);
};
const onSearch = () => {
  isDynamicSQL.value = false;
  opition.value.PageNo = 0;
  opition.value.id = null;
  opition.value.IsNext = true;
  opition.value.sort = "id DESC";
  first.value=0;
  loadData(true);
};
const addLog = (log) => {
  axios.post(baseURL + "/api/Proc/AddLog", log, config);
};
const initTudien = () => {
  axios
    .post(
        baseURL + "/api/Modules/GetDataProc",
        {
          str: encr(JSON.stringify({
            proc: "sys_logs_user",
        par: [
          { par: "user_id", va: store.getters.user.user_id },
        ],
          }), SecretKey, cryoptojs
          ).toString()
        },
        config
      )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        tdUsers.value = data[0];
      }
    })
    .catch((error) => {});
};
const loadCount = () => {
  axios
    .post(
        baseURL + "/api/Modules/GetDataProc",
        {
          str: encr(JSON.stringify({
            proc: "sys_logs_count",
        par: [
          { par: "user_id", va: store.getters.user.user_id },
          { par: "filteruser_id", va: opition.value.Filteruser_id },
          { par: "pageno", va: opition.value.PageNo },
          { par: "pagesize", va: opition.value.PageSize },
          { par: "search", va: opition.value.search },
          { par: "ip", va: opition.value.IP },
          { par: "loai", va: opition.value.Loai },
          { par: "module", va: opition.value.module },
          { par: "start_date", va: opition.value.StartDate },
          { par: "end_date", va: opition.value.EndDate },
        ],
          }), SecretKey, cryoptojs
          ).toString()
        },
        config
      )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      debugger
      if (data.length > 0) {
        opition.value.totalRecords = data[0][0].totalrecords;
        tdUsers.value = data[1];
      }
    })
    .catch((error) => {
      addLog({
        title: "Lỗi Console loadCount",
        controller: "LogsView.vue",
        log_content: error.message,
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
    opition.value.id = null;
    opition.value.IsNext = true;
  } else if (event.page > opition.value.PageNo + 1) {
    //Trang cuối
    opition.value.id = -1;
    opition.value.IsNext = false;
  } else if (event.page > opition.value.PageNo) {
    //Trang sau
    opition.value.id = datalists.value[datalists.value.length - 1].id;
    opition.value.IsNext = true;
  } else if (event.page < opition.value.PageNo) {
    //Trang trước
    opition.value.id = datalists.value[0].id;
    opition.value.IsNext = false;
  }
  opition.value.PageNo = event.page;
  loadData(true);
};
const onSort = (event) => {
  opition.value.sort = event.sortField + (event.sortOrder == 1 ? " ASC" : " DESC");
  if (event.sortField != "id") {
    opition.value.sort += ",id " + (event.sortOrder == 1 ? " ASC" : " DESC");
  }
  isDynamicSQL.value = true;
  loadDataSQL();
};
const onFilter = (event) => {
  filterSQL.value = [];
  debugger
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
    first.value=0;
  opition.value.id = null;
  isDynamicSQL.value = true;
  loadDataSQL();
};
const loadDataSQL = () => {
  let data = {
    id: opition.value.id,
    next: opition.value.IsNext,
    sqlO: opition.value.sort,
    Search: opition.value.search,
    PageNo: opition.value.PageNo,
    PageSize: opition.value.PageSize,
    fieldSQLS: filterSQL.value,
  };
  opition.value.loading = true;
  axios
    .post(baseURL + "/api/SQL/FilterSQLsys_logs", data, config)
    .then((response) => {
      let dt = JSON.parse(response.data.data);
      let data = dt[0];
      if (data.length > 0) {
        data.forEach((element, i) => {
          element.STT = (opition.value.PageNo - 1) * opition.value.PageSize + i + 1;
          element.Ngay = moment(new Date(element.log_date)).format("DD/MM/YYYY HH:mm:ss");
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
        controller: "LogsView.vue",
        log_content: error.message,
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
const loadData = (rf) => {
  // if (isDynamicSQL.value) {
  //   loadDataSQL();
  //   return false;
  // }
  opition.value.loading = true;
  if (rf) {
    if (opition.value.PageNo == 0) {
      loadCount();
    }
  }
  let proc = "sys_logs_listseek";
  let datas = [
    { par: "id", va: opition.value.id },
    { par: "isnext", va: opition.value.IsNext },
    { par: "user_id", va: store.getters.user.user_id },
    { par: "filteruser_id", va: opition.value.Filteruser_id },
    { par: "pageno", va: opition.value.PageNo },
    { par: "pagesize", va: opition.value.PageSize },
    { par: "search", va: opition.value.search },
    { par: "ip", va: opition.value.IP },
    { par: "loai", va: opition.value.Loai },
    { par: "module", va: opition.value.module },
    { par: "startdate", va: opition.value.StartDate },
    { par: "enddate", va: opition.value.EndDate },
  ];
  axios
    .post(
        baseURL + "/api/Modules/GetDataProc",
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
          element.STT = (opition.value.PageNo - 1) * opition.value.PageSize + i + 1;
          element.Ngay = moment(new Date(element.log_date)).format("DD/MM/YYYY HH:mm:ss");
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
        controller: "LogsView.vue",
        log_content: error.message,
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
        excelname: "LOGS",
        proc: "Sys_Logs_ListExport",
        par: [
          { par: "user_id", va: opition.value.user_id },
          { par: "Filteruser_id", va: opition.value.Filteruser_id },
          { par: "Search", va: opition.value.search },
          { par: "IP", va: opition.value.IP },
          { par: "Loai", va: opition.value.Loai },
          { par: "module", va: opition.value.module },
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
            //window.open(baseURL + response.data.path);
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
      }
    });
};
const rowClass = (data) => {
  return data.loai == 1 ? "success" : "error";
};
const rowClassError = (data) => {
  return data.error ? "error" : "success";
};

onMounted(() => {
  //init
 // initTudien();
  loadData(true);
  return {
    displayAddData,
    isFirst,
    opition,
    showModalDetail,
    closedisplayDetail,
    onSearch,
    basedomainURL,
    filters,
    onRefersh,
    itemButs,
    menuButs,
    toggleExport,
    onPage,
    modelview,
  };
});
</script>
<template>
  <div
    :class="
      'main-layout ' + (opition.totalRecords > 10) + ' flex flex-column flex-grow-1 p-2'
    "
    v-if="store.getters.islogin"
  >
    <DataTable
      class="w-full p-datatable-sm e-sm"
      :lazy="true"
      @page="onPage($event)"
      @filter="onFilter($event)"
      @sort="onSort($event)"
      :value="datalists"
      :loading="opition.loading"
      :paginator="opition.totalRecords > opition.PageSize"
      :rows="opition.PageSize"
      :totalRecords="opition.totalRecords"
      dataKey="id"
      :filters="filters"
      :rowHover="true"
      filterDisplay="menu"
      :showGridlines="true"
      filterMode="lenient"
      paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
      :rowsPerPageOptions="[ 20,30, 50, 100,200]"
      :currentPageReportTemplate="
        isDynamicSQL ? '{currentPage}' : '{currentPage}/{totalPages}'
      "
      :pageLinkSize="1"
      responsiveLayout="scroll"
      :scrollable="true"
      scrollHeight="flex"
      :rowClass="rowClass"
      v-model:first="first"

    >
      <template #header>
        <h3 class="module-title mt-0 ml-1 mb-2">
          <i class="pi pi-list"></i> Theo dõi Log
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
                  placeholder="Tìm kiếm"
                  v-model="filters['global'].value"
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
            </template>

            <template #end>
              <Button
                class="mr-2 p-button-outlined p-button-secondary"
                icon="pi pi-refresh"
                @click="onRefersh"
              />
              <!-- <Button
                label="Export"
                icon="pi pi-file-excel"
                class="mr-2 p-button-outlined p-button-secondary"
                @click="toggleExport"
                aria-haspopup="true"
                aria-controls="overlay_Export"
              />
              <Menu id="overlay_Export" ref="menuButs" :model="itemButs" :popup="true" /> -->
            </template>
          </Toolbar>
        </div>
      </template>
      <Column
        field="id"
        header="ID"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:120px"
        bodyStyle="text-align:center;max-width:120px"
      ></Column>
      <Column field="title" header="Tiêu đề" class="align-items-center"  bodyStyle="word-break:break-word">
        <template #body="md">
          <InlineMessage
            style="justify-content: center"
            v-bind:severity="md.data.loai == 1 ? 'success' : 'error'"
            >{{ md.data.title }}</InlineMessage
          >
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
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:200px"
        bodyStyle="text-align:center;max-width:200px"
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
            style="max-width:200px"
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
        field="logdate"
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
        field="IP"
        header="IP"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:120px"
        bodyStyle="text-align:center;max-width:120px"
      >
        <template #body="{ data }">
          {{ data.modified_ip }}
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
        field="loai"
        headerStyle="max-width: 60px"
        headerClass="text-center"
        bodyClass="text-center"
        :showFilterMatchModes="false"
        bodyStyle="text-align:center;max-width:60px"
      >
        <template #header> </template>
        <template #body="md">
          <Button
            v-if="md.data.loai != 1"
            type="button"
            icon="pi pi-info-circle"
            class="p-button-sm p-button-secondary"
            @click="showModalDetail(md.data)"
          ></Button>
        </template>
        <!-- <template #filter="{ filterModel }">
          <Dropdown
            v-model="filterModel.value"
            :options="tdLoais"
            optionLabel="name"
            optionValue="id"
            placeholder="Chọn loại"
            class="p-column-filter"
            :showClear="true"
          >
          </Dropdown>
        </template> -->
      </Column>
      <template #empty>
        <div
          class="align-items-center justify-content-center p-4 text-center m-auto"
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
    :style="{ width: '70vw', zIndex: 1000 }"
    :maximizable="true"
    :autoZIndex="false"
    :modal="true"
  >
    <div class="grid">
      <div class="col-4 h-full" v-if="modelview.data">
        <Panel header="Tham số truyền vào">
          <div v-if="modelview.data.length == 1">
            <h3>{{ modelview.data[0].key }}</h3>
            <h5 style="word-break: break-word">{{ modelview.data[0].value }}</h5>
          </div>
          <DataTable
            class="p-datatable-sm"
            showGridlines
            v-if="modelview.data.length > 1"
            :value="modelview.data"
            :rowClass="rowClassError"
            responsiveLayout="scroll"
            :scrollable="true"
          >
            <Column
              field="key"
              header="Tham số"
              headerStyle="background-color:aliceblue;font-weight:bold;max-width:120px"
              bodyStyle="max-width:120px"
            ></Column>
            <Column
              field="value"
              header="Giá trị"
              headerStyle="background-color:aliceblue;font-weight:bold"
              bodyStyle="word-break:break-word"
            ></Column>
          </DataTable>
        </Panel>
      </div>
      <div :class="'col-' + (modelview.data ? 8 : 12)">
        <Panel header="Chi tiết lỗi">
          <InlineMessage style="width: 100%; justify-content: start">{{
            modelview.title
          }}</InlineMessage>
          <div
            style="
              background: rgba(0, 0, 0, 0.8);
              color: #fff;
              padding: 10px;
              overflow: auto;
            "
          >
            <code id="diverrrorContent" class="m-3" v-html="modelview.contents"></code>
          </div>
        </Panel>
      </div>
    </div>

    <template #footer>
      <Button label="Đóng lại" icon="pi pi-times" @click="closedisplayDetail" />
    </template>
  </Dialog>
</template>
<style scoped>
.boxtable {
  height: calc(100% - 60px);
}
</style>
<style lang="scss" scoped>
::v-deep(.p-datatable) {
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

