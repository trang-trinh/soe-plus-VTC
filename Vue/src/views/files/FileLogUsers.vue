<script setup>
import { ref, inject, onMounted } from "vue";
import { useToast } from "vue-toastification";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import moment from "moment";
import { encr } from "../../util/function.js";
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
  sort: "file_log_id DESC",
  PageNo: 0,
  PageSize: 30,
  Filteruser_id: null,
  user_id: store.getters.user_id,
  department_id: store.getters.user.organization_id,
  is_folder: null,
  sort: '',
  ob:''
});
const basedomainURL = fileURL;
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
//Khai báo biến
const active_sort = ref(false);
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
const checkFilter = ref(false);
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
  if (
    !(
      options.value.is_hot != null ||
      options.value.status != null ||
      options.value.news_type != null
    )
  )
    checkFilter.value = false;
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
  active_sort.value = false;
  opition.value = {
    IsNext: true,
    sort: "file_log_id DESC",
    PageNo: 0,
    IsLess: true,
    PageSize: 30,
    Filteruser_id: null,
    user_id: store.getters.user.user_id,
    start_date: null,
    end_date: null,
      department_id: store.getters.user.organization_id,
      sort:'',
      ob:'',
  };
  first.value = 0;
  //isDynamicSQL.value = false;
  filterSQL.value = [];
  loadData(true);
};
const onSearch = () => {
  //isDynamicSQL.value = false;
  opition.value.PageNo = 0;
  first.value = 0;
  opition.value.file_log_id = null;
  opition.value.IsNext = true;
  opition.value.sort = "file_log_id DESC";
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
        str: encr(
          JSON.stringify({
            proc: "sys_webacess_dictionary",
            par: [{ par: "user_id", va: store.getters.user.user_id }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
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
  first.value = 0;
  loadData(true);
};
const loadCount = () => {
  axios
    .post(
      baseURL + "/api/Users/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "file_log_user_count",
            par: [
              { par: "search", va: opition.value.search },
              { par: "user_id", va: store.getters.user.user_id },
              { par: "filterusers_id", va: opition.value.Filteruser_id },
              { par: "department_id", va: opition.value.department_id },
              { par: "pageno", va: opition.value.PageNo },
              { par: "pagesize", va: opition.value.PageSize },
              { par: "ip", va: opition.value.IP },
              { par: "startdate", va: opition.value.start_date },
              { par: "enddate", va: opition.value.end_date },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        opition.value.totalRecords = data[0][0].totalrecords;
        tdUsers.value = data[1];
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
    opition.value.file_log_id = null;
    opition.value.IsNext = true;
  } else if (event.page == event.pageCount - 1) {
    //Trang cuối
    opition.value.file_log_id = -1;
    opition.value.IsNext = false;
  } else if (event.page > opition.value.PageNo) {
    //Trang sau
    opition.value.file_log_id =
      datalists.value[datalists.value.length - 1].file_log_id;
    opition.value.IsNext = true;
  } else if (event.page < opition.value.PageNo) {
    //Trang trước
    opition.value.file_log_id = datalists.value[0].file_log_id;
    opition.value.IsNext = false;
  }
  opition.value.PageNo = event.page;
  loadData(true);
};
const onSort = (event) => {
  opition.value.sort =
    event.sortField + (event.sortOrder == 1 ? " ASC" : " DESC");
  if (event.sortField != "file_log_id") {
    opition.value.sort +=
      ",file_log_id " + (event.sortOrder == 1 ? " ASC" : " DESC");
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
  opition.value.file_log_id = null;
  isDynamicSQL.value = true;
  loadDataSQL();
};
const loadDataSQL = () => {
  let data = {
    id: opition.value.file_log_id,
    next: opition.value.IsNext,
    sqlO: opition.value.sort,
    Search: opition.value.search,
    PageNo: opition.value.PageNo,
    PageSize: opition.value.PageSize,
    fieldSQLS: filterSQL.value,
  };
  opition.value.loading = true;
  axios
    .post(baseURL + "/api/SQL/FilterSQL_file_log", data, config)
    .then((response) => {
      let dt = JSON.parse(response.data.data);
      let data = dt[0];
      if (data.length > 0) {
        data.forEach((element, i) => {
          element.STT = opition.value.PageNo * opition.value.PageSize + i + 1;
          element.Ngay = moment(new Date(element.is_time)).format(
            "DD/MM/YYYY HH:mm"
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
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const loadData = (rf) => {
  debugger;
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
  let proc = "file_log_user_list_new";
  let datas = [
    { par: "search", va: opition.value.search },
    { par: "user_id", va: store.getters.user.user_id },
    { par: "filteruser_id", va: opition.value.Filteruser_id },
    { par: "department_id", va: opition.value.department_id },
    { par: "pageno", va: opition.value.PageNo },
    { par: "pagesize", va: opition.value.PageSize },
    { par: "ip", va: opition.value.IP },
    { par: "start_date", va: opition.value.start_date },
    { par: "end_date", va: opition.value.end_date },
    { par: "sort", va: opition.value.sort },
    { par: "ob", va: opition.value.ob },
  ];
  axios
    .post(
      baseURL + "/api/FileMain/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: proc,
            par: datas,
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
        data.forEach((element, i) => {
          element.STT = opition.value.PageNo * opition.value.PageSize + i + 1;
          element.Ngay = moment(new Date(element.is_time)).format(
            "DD/MM/YYYY HH:mm"
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
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
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
        window.open(baseURL + response.data.path);
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
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const itemSortButs = ref([
  {
    label: "Dung lượng tăng dần",
    type_sort: "capacity_asc",
    sort: "capacity",
        ob: "asc",
    active: false,
    command: (event) => {
      ChangeSortFile(sort, ob);
    },
  },
  {
    label: "Kích thước giảm dần",
    sort: "capacity",
        ob: "desc",
    active: false,
    command: (event) => {
      ChangeSortFile(sort, ob);
    },
  },
  {
    label: "Tên từ A-Z",
    active: false,
    sort: "full_name",
        ob: "asc",
    command: (event) => {
      ChangeSortFile(sort, ob);
    },
  },
  {
    label: "Tên từ Z-A",
    type_sort: "full_name_desc",
    active: false,
    sort: "full_name",
        ob: "desc",
    command: (event) => {
      ChangeSortFile(sort, ob);
    },
  },
]);

const menuSortButs = ref();
const toggleSort = (event) => {
  menuSortButs.value.toggle(event);
};
const ChangeSortFile = (sort, ob) => {
  active_sort.value = true;
  itemSortButs.value.forEach((i) => {
    if (i.sort == sort && i.ob == ob) {
      i.active = true;
    } else {
      i.active = false;
    }
  });
  menuSortButs.value.toggle();
   opition.value.sort = sort;
   opition.value.ob = ob;
   loadData()
};
const formatBytes = (bytes, decimals = 2) => {
  if (bytes === 0) return "0 Bytes";

  const k = 1024;
  const dm = decimals < 0 ? 0 : decimals;
  const sizes = ["Bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"];

  const i = Math.floor(Math.log(bytes) / Math.log(k));

  return parseFloat((bytes / Math.pow(k, i)).toFixed(dm)) + " " + sizes[i];
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
      dataKey="file_log_id"
      :rowHover="true"
      v-model:filters="filters"
      filterDisplay="menu"
      :showGridlines="true"
      filterMode="lenient"
      paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
      :rowsPerPageOptions="[20, 30, 50, 100, 200]"
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
          <i class="pi pi-database"></i> Thống kê toài khoản sử dụng dữ liệu
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
              <Button
                v-if="store.getters.user.is_admin"
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
              <div
                  @click="toggleSort"
                  aria-haspopup="true"
                  aria-controls="overlay_Export"
                  class="btn_sort ml-3 px-1 cursor-pointer"
                  :class="active_sort?'active-sort':''"
                >
                  <a
                    ><i class="pi pi-sort"></i> Sắp xếp
                    <i class="pi pi-angle-down"></i
                  ></a>:
                </div>
              <Button
                class="mr-2 p-button-outlined p-button-secondary"
                icon="pi pi-refresh"
                @click="onRefersh"
              />
              <Menu
                  class="menu_sort"
                  :model="itemSortButs"
                  ref="menuSortButs"
                  :popup="true"
                >
                  <template #item="{ item }">
                    <a
                      @click="ChangeSortFile(item.sort, item.ob)"
                      :class="{ active: item.active }"
                      >{{ item.label }}</a
                    >
                  </template>
                </Menu>
              <!-- <Button
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
              /> -->
            </template>
          </Toolbar>
        </div>
      </template>
      <Column
        field="STT"
        header="STT"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:80px"
        bodyStyle="text-align:center;max-width:80px"
      ></Column>
      <Column
        :sortable="false"
        field="full_name"
        filterField="full_name"
        header="Tài khoản"
        :showFilterMatchModes="false"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:150px"
        bodyStyle="text-align:center;max-width:150px"
      >
        <template #body="{ data }">
          <span class="image-text">{{ data.full_name }}</span>
        </template>
        <!-- <template #filter="{ filterModel }">
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
        </template> -->
      </Column>
      <Column field="organization_name" header="Phòng ban"></Column>
      <Column
        field="position_name"
        header="Chức vụ"
        headerStyle="text-align:center;max-width:150px"
        bodyStyle="text-align:center;max-width:150px"
      ></Column>
      <Column
        field="capacity"
        header="Tổng dung lượng"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:150px"
        bodyStyle="text-align:center;max-width:150px"
      >
        <template #body="{ data }">
          {{ formatBytes(data.capacity || 0) }}
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
        field="count_file"
        header="Số file tải lên"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:150px"
        bodyStyle="text-align:center;max-width:150px"
      >
        <template #body="{ data }">
          {{ data.count_file }}
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
  <ContextMenu :model="menuModel" ref="cm" />
</template>
<style>
.toolbar-filter {
  border: unset;
  outline: unset;
  background-color: #fff;
  padding-bottom: 0px;
}
.yellow {
  background-color: #ffc107 !important;
  color: #000 !important;
}
.blue {
  background-color: #17a2b8 !important;
  color: #000 !important;
}
.btn_sort{
  list-style: none;
    display: flex;
    justify-content: center;
    align-items: center;
    height: 30px;
    border: 1px solid;
    border-radius: 4px;
    margin: 0px 5px 0px 0px;
}
.menu_sort {
  min-width: fit-content !important;
}
.menu_sort .p-menuitem {
  padding: 5px 10px !important;
}

.menu_sort .p-menuitem:hover {
  cursor: pointer;
  background-color: #e9ecef;
}
.menu_sort .p-menuitem .active {
  color: #2196f3 !important;
}
.menu_sort .p-menuitem a {
  text-decoration: none;
}
.active-sort{
background-color: #2196f3!important;
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
    width: 170px !important;
  }
}
</style>
