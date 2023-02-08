<script setup>
import { ref, inject, onMounted } from "vue";
import { useToast } from "vue-toastification";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import moment from "moment";
import { encr, checkURL } from "../../util/function.js";
//Khai báo
const router = inject("router");
const cryoptojs = inject("cryptojs");
//Khai báo biến
const filters = ref({
  user_id: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
  StartDate: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.DATE_IS }],
  },
  IP: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
  full_name: { value: null, matchMode: FilterMatchMode.IN },
  title: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
  milliseconds: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.EQUALS }],
  },
});
const first = ref(0);
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
const options = ref({
  IsNext: true,
  sort: "log_id DESC",
  PageNo: 1,
  PageSize: 20,
  Filteruser_id: null,
  user_id: store.getters.user.user_id,
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
const checkFilter = ref(false);
const todayClick = () => {
  taskDateFilter.value = [];
  taskDateFilter.value.push(new Date());
};
const delDayClick = () => {
  taskDateFilter.value = [];
  options.value.StartDate = null;
  options.value.EndDate = null;
  loadData(true);
};
const onDayClick = () => {
  if (taskDateFilter.value == null) taskDateFilter.value = [];
  else {
    options.value.StartDate = taskDateFilter.value[0];
    options.value.EndDate = taskDateFilter.value[1];
    options.value.PageNo = 0;
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
      baseURL + "/api/device_card/getData",
        {
          str: encr(
            JSON.stringify({
        proc: "SQL_Log_Get",
        par: [
          { par: "user_id", va: options.value.user_id },
          { par: "id", va: md.id },
        ],
      }),
            SecretKey,
            cryoptojs
          ).toString(),
        },config
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
  options.value.search = "";
  taskDateFilter.value = [];
  options.value = {
    IsNext: true,
    sort: "id DESC",
    PageNo: 0,
    PageSize: 20,
    Filteruser_id: null,
    user_id: store.getters.user.user_id,
  };
  first.value = 0;
  loadData(true);
};
const onSearch = () => {
  isDynamicSQL.value = false;
  options.value.PageNo = 0;
  first.value = 0;
  options.value.id = null;
  options.value.IsNext = true;
  options.value.sort = "id DESC";
  loadData(true);
};
const addLog = (log) => {
  axios.post(baseURL + "/api/Proc/AddLog", log, config);
};

const loadCount = () => {
  axios
    .post(
      baseURL + "/api/device_card/getData",
        {
          str: encr(
            JSON.stringify({
        proc: "device_log_count",
        par: [
          { par: "user_id", va: options.value.user_id },
          { par: "filteruser_id", va: options.value.Filteruser_id },

          { par: "search", va: options.value.search },
          { par: "filter_type", va: options.value.FilterType },
          { par: "startdate", va: options.value.StartDate },
          { par: "enddate", va: options.value.EndDate },
        ],
      }),
            SecretKey,
            cryoptojs
          ).toString(),
        },config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
    
      if (data.length > 0) {
        options.value.totalRecords = data[0][0].totalrecords;
        tdUsers.value = data[1];
      }
    })
    .catch((error) => {
      addLog({
        title: "Lỗi Console loadCount",
        controller: "SQLView.vue",
        log_content: error.message,
        loai: 2,
      });
    });
};
const onPage = (event) => {
  if (event.page == 0) {
    //Trang đầu
    options.value.id = null;
    options.value.IsNext = true;
  } else if (event.page > options.value.PageNo + 1) {
    //Trang cuối
    options.value.id = -1;
    options.value.IsNext = false;
  } else if (event.page > options.value.PageNo) {
    //Trang sau
    options.value.id = datalists.value[datalists.value.length - 1].id;
    options.value.IsNext = true;
  } else if (event.page < options.value.PageNo) {
    //Trang trước
    options.value.id = datalists.value[0].id;
    options.value.IsNext = false;
  }
  options.value.PageNo = event.page+1;
  loadData(true);
};
const onSort = (event) => {
  options.value.sort =
    event.sortField + (event.sortOrder == 1 ? " ASC" : " DESC");
  if (event.sortField != "id") {
    options.value.sort += ",id " + (event.sortOrder == 1 ? " ASC" : " DESC");
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
  options.value.PageNo = 0;
  first.value = 0;
  options.value.id = null;
  isDynamicSQL.value = true;
  loadDataSQL();
};
const loadDataSQL = () => {
  let data = {
    id: options.value.id,
    next: options.value.IsNext,
    sqlO: options.value.sort,
    Search: options.value.search,
    PageNo: options.value.PageNo,
    PageSize: options.value.PageSize,
    fieldSQLS: filterSQL.value,
  };
  options.value.loading = true;
  axios
    .post(baseURL + "/api/SQL/FilterSQLsql_log", data, config)
    .then((response) => {
      let dt = JSON.parse(response.data.data);
      let data = dt[0];
      if (data.length > 0) {
        data.forEach((element, i) => {
          element.STT =
            (options.value.PageNo - 1) * options.value.PageSize + i + 1;
          element.Ngay = moment(new Date(element.StartDate)).format(
            "DD/MM/YYYY HH:mm:ss"
          );
        });
        datalists.value = data;
      } else {
        datalists.value = [];
      }
      if (isFirst.value) isFirst.value = false;
      options.value.loading = false;
      //Show Count nếu có
      if (dt.length == 2) {
        options.value.totalRecords = dt[1][0].totalRecords;
      }
    })
    .catch((error) => {
      options.value.loading = false;
      toast.error("Tải dữ liệu không thành công!");
      addLog({
        title: "Lỗi Console loadData",
        controller: "SQLView.vue",
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
  options.value.loading = true;
  loadCount();
  axios
    .post(
      baseURL + "/api/device_card/getData",
        {
          str: encr(
            JSON.stringify({
        proc: "device_log_list",
        par: [
          { par: "user_id", va: options.value.user_id },
          { par: "filteruser_id", va: options.value.Filteruser_id },
          { par: "pageno", va: options.value.PageNo },
          { par: "pagesize", va: options.value.PageSize },
          { par: "search", va: options.value.search },
          { par: "filter_type", va: options.value.FilterType },
          { par: "startdate", va: options.value.StartDate },
          { par: "enddate", va: options.value.EndDate },
        ],
      }),
            SecretKey,
            cryoptojs
          ).toString(),
        },config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        data.forEach((element, i) => {
          element.STT =
            (options.value.PageNo - 1) * options.value.PageSize + i + 1;
          element.Ngay = moment(new Date(element.created_date)).format(
            "DD/MM/YYYY HH:mm:ss"
          );
        });
        datalists.value = data;
      } else {
        datalists.value = [];
      }
      if (isFirst.value) isFirst.value = false;
      if (rf) {
        options.value.loading = false;
      }
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;
      addLog({
        title: "Lỗi Console loadData",
        controller: "SQLView.vue",
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
        excelname: "SQLLOGS",
        proc: "SQL_Log_ListExport",
        par: [
          { par: "user_id", va: options.value.user_id },
          { par: "Filteruser_id", va: options.value.Filteruser_id },
          { par: "Search", va: options.value.search },
          { par: "IP", va: options.value.IP },
          { par: "StartDate", va: options.value.StartDate },
          { par: "EndDate", va: options.value.EndDate },
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
  return data.log_type == 0
    ? "green"
    : data.log_type == 1
    ? "success"
    : data.log_type == 2
    ? "orange"
    : "error";
};
const rowClassError = (data) => {
  return data.error ? "error" : "success";
};

onMounted(() => {  if (!checkURL(window.location.pathname, store.getters.listModule)) {
     //router.back();
  }
  //init
  // initTudien();
  loadData(true);
  return {
    tdUsers,
    displayAddData,
    isFirst,
    options,
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
    class="main-layout flex flex-column flex-grow-1 p-2"
    v-if="store.getters.islogin"
  >
    <DataTable
      class="w-full p-datatable-sm e-sm"
      :lazy="true"
      @page="onPage($event)"
      @filter="onFilter($event)"
      @sort="onSort($event)"
      :value="datalists"
      :loading="options.loading"
      :paginator="options.totalRecords > options.PageSize"
      :rows="options.PageSize"
      :totalRecords="options.totalRecords"
      dataKey="log_id"
      :rowHover="true"
      :filters="filters"
      :showGridlines="true"
      :pageLinkSize="1"
      filterDisplay="menu"
      filterMode="lenient"
      paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
      :rowsPerPageOptions="[20, 30, 50, 100, 200]"
      :currentPageReportTemplate="
        isDynamicSQL ? '{currentPage}' : '{currentPage}/{totalPages}'
      "
      responsiveLayout="scroll"
      :scrollable="true"
      scrollHeight="flex"
      :rowClass="rowClass"
      v-model:first="first"
    >
      <template #header>
        <h3 class="module-title module-title-hidden mt-0 ml-1 mb-2">
          <i class="pi pi-list"></i> Theo dõi Log Tài sản
          <span v-if="options.totalRecords > 0"
            >({{ options.totalRecords.toLocaleString() }})</span
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
                  v-model="options.search"
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
      </template>

      <Column
        field="log_id"
        header="ID"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:120px"
        bodyStyle="text-align:center;max-width:120px"
      ></Column>
      <Column field="title" header="Tiêu đề" class="align-items-center">
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
        field="created_by"
        header="Tài khoản"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:200px"
        bodyStyle="text-align:center;max-width:200px"
      >
        <template #body="{ data }">
          {{ data.created_by }}
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
        header="Tên người tạo"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:300px"
        bodyStyle="text-align:center;max-width:300px"
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
        field="created_date"
        dataType="date"
        header="Ngày thực hiện"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:200px"
        bodyStyle="text-align:center;max-width:200px"
      >
        <template #body="{ data }">
          {{ moment(new Date(data.created_date)).format("DD/MM/YYYY HH:mm") }}
        </template>
        <template #filter="{ filterModel }">
          <Calendar v-model="filterModel.value" />
        </template>
      </Column>

      <Column
        field="created_ip"
        header="IP người tạo"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:150px"
        bodyStyle="text-align:center;max-width:150px"
      >
        <template #body="{ data }">
          {{ data.created_ip }}
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
      <!-- <Column
            headerStyle="max-width: 60px"
            headerClass="text-center"
            bodyClass="text-center"
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
          </Column> -->
      <template #empty>
        <div
          class="
            align-items-center
            justify-content-center
            p-4
            text-center
            m-auto
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
    :style="{ width: '70vw', zIndex: 1000 }"
    :maximizable="true"
    :autoZIndex="false"
    :modal="true"
  >
    <div class="grid">
      <div class="col-4" v-if="modelview.data">
        <Panel header="Tham số truyền vào">
          <div v-if="modelview.data.length == 1">
            <h3>{{ modelview.data[0].key }}</h3>
            <h5 style="word-break: break-word">
              {{ modelview.data[0].value }}
            </h5>
          </div>
          <DataTable
            class="p-datatable-sm"
            showGridlines
            v-if="modelview.data.length > 1"
            :value="modelview.data"
            :rowClass="rowClassError"
            responsiveLayout="scroll"
          >
            <Column
              field="key"
              header="Tham số"
              headerStyle="background-color:aliceblue;font-weight:bold"
              :style="{ minWidth: '100px' }"
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
        <Panel header="Chi tiết SQL">
          <InlineMessage
            style="width: 100%; justify-content: start"
            severity="success"
            >{{ modelview.title }}</InlineMessage
          >
          <div
            style="
              background: rgba(0, 0, 0, 0.8);
              color: #fff;
              padding: 10px;
              overflow: auto;
            "
          >
            <code
              id="diverrrorContent"
              class="m-3"
              v-html="modelview.contents"
            ></code>
          </div>
        </Panel>
      </div>
    </div>

    <template #footer>
      <Button label="Đóng lại" icon="pi pi-times" @click="closedisplayDetail" />
    </template>
  </Dialog>
</template>
    
    <style lang="scss" scoped>
.boxtable {
  height: calc(100% - 60px);
}
::v-deep(.p-datatable) {
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
    