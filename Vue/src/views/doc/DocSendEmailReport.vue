<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import moment from "moment";
import _ from "lodash";
import { encr } from "../../util/function";
//Khai báo
const cryoptojs = inject("cryptojs");
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const basedomainURL = baseURL;
const selectedHandOver = ref();
const selectedCard = ref([]);
const checkDelList = ref(false);
const displayAssets = ref(false);
const isFirstCard = ref(false);
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const toast = useToast();
const options = ref({
  IsNext: true,
  sort: "email_sent_id DESC",
  sortDM: null,
  search: "",
  pageno: 0,
  pagesize: 50,
  pagenoDM: 0,
  pagesizeDM: 10,
  loading: true,
  totalRecords: null,
  totalRecordsDM: null,
  start_date: null,
  end_date: null,
  next: true,
  filterOrg: [],
  id: null,
});

const datalists = ref();
const isDynamicSQL = ref(false);
const loadDataSQL = () => {
  let data = {
    id: options.value.id,
    next: options.value.IsNext,
    sqlO: options.value.sort,
    Search: options.value.search,
    PageNo: options.value.pageno,
    PageSize: options.value.pagesize,
    fieldSQLS: filterSQL.value,
  };

  options.value.loading = true;
  axios
    .post(baseURL + "/api/SQL/Filter_doc_report_list_send_email", data, config)
    .then((response) => {
      let dt = JSON.parse(response.data.data);

      let data = dt[0];
      if (data.length > 0) {
        data.forEach((element, i) => {
          element.is_order =
            options.value.pageno * options.value.pagesize + i + 1;
        });

        datalists.value = data;
      } else {
        datalists.value = [];
      }
      options.value.loading = false;
      //Show Count nếu có
      if (dt.length == 2) {
        options.value.totalRecords = dt[1][0].totalRecords;
      }
    })
    .catch((error) => {
      options.value.loading = false;
      toast.error("Tải dữ liệu không thành công!");
      
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const loadData = () => {
  if (isDynamicSQL.value) {
    loadDataSQL();
    return false;
  }
  axios
    .post(
      baseURL + "/api/DocProc/CallProc",
      {str: 
        encr(JSON.stringify(
          {
        proc: "doc_report_list_send_email",
        par: [
        { par: "pageno", va: options.value.pageno },
          { par: "pagesize", va: options.value.pagesize },
          { par: "user_id", va: store.state.user.user_id },
          { par: "user_id_send", va: filterCardUserSend.value },
          { par: "start_dateD", va: options.value.start_dateD },
          { par: "end_dateD", va: options.value.end_dateD },
          { par: "search", va: options.value.search },
        ],
      }
      ),
        SecretKey, cryoptojs)
        .toString()
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      let totalRecords = JSON.parse(response.data.data)[1];
      options.value.totalRecords = totalRecords[0].dmc;
      if (data.length > 0) {
        data.forEach((element, i) => {
          element.is_order =
            options.value.pageno * options.value.pagesize + i + 1;
        });

        datalists.value = data;
      } else {
        datalists.value = [];
      }

      options.value.loading = false;
    })
    .catch((error) => {
      
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;

      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};

//Tìm kiếm
const searchReceive = () => {
  loadData();
};
const refreshData = () => {
  filterCardUserSend.value= null;
  options.value.start_dateD = null;
  options.value.end_dateD = null;
  options.value.search = null;
  loadData();
};
const filterCardUserSend=ref();
const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]); 
const filterButs = ref();
const toggleFilter = (event) => {
  filterButs.value.toggle(event);
};
const hideFilter = () => {
  if (
    !(
      options.value.start_dateD != null ||
      options.value.end_dateD != null ||
      filterCardUserSend.value != null
    )
  )
    checkFilter.value = false;
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
const showURLSCV=(value)=>{
  window.open(value,'_blank');

}
const listDepartment = ref();
const checkFilter = ref(false);
const onRefilterDM = () => {
  filterCardUserSend.value=null;
  options.value.end_dateD = null;

  options.value.start_dateD = null;
  filterButs.value.hide();
  loadData();
};
const onFilterDM = () => {  filterButs.value.hide()
  options.value.loading = true;
  checkFilter.value = true;
  loadData();
  filterButs.value.hide();
};

//Xuất excel
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
const toggleExport = (event) => {
  menuButs.value.toggle(event);
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
        excelname: "BÁO CÁO GỬI EMAIL",
        proc: "doc_report_list_send_email_export",
        par: [
          { par: "pageno", va: options.value.pageno },
          { par: "pagesize", va: options.value.pagesize },
          { par: "user_id", va: store.state.user.user_id }, 
          { par: "user_id_send", va: filterCardUserSend.value },
          { par: "start_dateD", va: options.value.start_dateD },
          { par: "end_dateD", va: options.value.end_dateD },
          { par: "search", va: options.value.search },
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
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};

const filters = ref({
  global: { value: null, matchMode: FilterMatchMode.CONTAINS },
  email_name: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
  doc_code: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
  sent_date: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.DATE_IS }],
  },
});
const filterSQL = ref();
const onPage = (event) => {
  if (event.rows != options.value.pagesize) {
    options.value.pagesize = event.rows;
  }
  if (event.page == 0) {
    //Trang đầu
    options.value.id = null;
    options.value.IsNext = true;
  } else if (event.page > options.value.pageno + 1) {
    //Trang cuối
    options.value.id = -12;
    options.value.IsNext = false;
  } else if (event.page > options.value.pageno) {
    //Trang sau

    options.value.id =
      datalists.value[datalists.value.length - 1].email_sent_id;
    options.value.IsNext = true;
  } else if (event.page < options.value.pageno) {
    //Trang trước
    options.value.id = datalists.value[0].email_sent_id;
    options.value.IsNext = false;
  }
  options.value.pagesize = event.rows;
  options.value.pageno = event.page;
  loadDataSQL();
};
const onFilter = (event) => {
  filterSQL.value = [];
  for (const [key, value] of Object.entries(event.filters)) {
    if (key != "global") {
      let obj = {
        key: key,
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
  options.value.pageno = 0;
  options.value.id = null;
  isDynamicSQL.value = true;
  loadData(true);
};

//Sort
const onSort = (event) => {
  if (event.sortField == null) {
    isDynamicSQL.value = false;
    loadData();
  } else {
    options.value.sort =
      event.sortField + (event.sortOrder == 1 ? " ASC" : " DESC");
    if (event.sortField != "email_sent_id") {
      options.value.sort +=
        ",email_sent_id " + (event.sortOrder == 1 ? " ASC" : " DESC");
    }
    isDynamicSQL.value = true;
    loadData();
  }
};

const listDropdownUser = ref();
const loadUser = () => {
  listDropdownUser.value = [];
  axios
    .post(
      baseURL + "/api/DocProc/CallProc",
      {str: 
        encr(JSON.stringify(
          {
        proc: "sys_users_list_dd",
        par: [
          { par: "search", va: null },
          { par: "user_id", va: store.getters.user.user_id },
          { par: "role_id", va: null },
          { par: "organization_id", va: store.getters.user.organization_id },
          { par: "department_id", va: null },{ par: "position_id", va:null },
          { par: "pageno", va: 1 },
          { par: "pagesize", va: 10000 },
          { par: "isadmin", va: null },
          { par: "status", va: null },
          { par: "start_date", va: null },
          { par: "end_date", va: null },
        ],
      }
      ),
        SecretKey, cryoptojs)
        .toString()
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];

      data.forEach((element, i) => {
        listDropdownUser.value.push({
          name: element.full_name,
          code: element.user_id,
          avatar: element.avatar,
        });
      });
    })
    .catch((error) => {
      
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;

      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};

onMounted(() => {
  // loadOrganization();
  loadUser();
  loadData();
});
</script>
<template>
  <div class="d-container">
    <div class="d-lang-table">
      <DataTable
        class="w-full p-datatable-sm e-sm"
        :lazy="true"
        @page="onPage($event)"
        @filter="onFilter($event)"
        @sort="onSort($event)"
        :value="datalists"
        :loading="options.loading"
        :paginator="options.totalRecords > options.pagesize"
        :rows="options.pagesize"
        :totalRecords="options.totalRecords"
        dataKey="email_sent_id"
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
  
      >
        <template #header>
          <div>
    <h3 class="module-title my-2 ml-1">
      <font-awesome-icon icon="fa-solid fa-file-import" /> Báo cáo gửi Email ({{
        options.totalRecords ? options.totalRecords : 0
      }})
    </h3>
  </div>
          <Toolbar class="custoolbar p-0 py-3 surface-50">
            <template #start>
              <span class="p-input-icon-left">
                <i class="pi pi-search" />
                <InputText
                  v-model="options.search"
                  @keyup.enter="searchReceive()"
                  type="text"
                  spellcheck="false"
                  placeholder="Tìm kiếm"
                />
                <!-- :class="checkFilter?'':'p-button-secondary'" -->
                <Button
                  :class="
                    checkFilter ? '' : 'p-button-secondary p-button-outlined'
                  "
                  class="ml-2"
                  icon="pi pi-filter"
                  @click="toggleFilter"
                  aria-haspopup="true"
                  aria-controls="overlay_panelS"
                />
                <OverlayPanel
                  @hide="hideFilter"
                  ref="filterButs"
                  appendTo="body"
                  :showCloseIcon="false"
                  id="overlay_panelS"
                  style="width: 500px"
                  :breakpoints="{ '960px': '20vw' }"
                >
                  <div class="grid formgrid m-2">
                    <!-- <div class="field col-12 md:col-12 flex">
                      <div class="col-3 p-0 align-items-center flex">
                        Công ty:
                      </div>
                      <TreeSelect
                        v-model="options.filterOrg"
                        :options="listDepartment"
                        :showClear="true"
                        :max-height="200"
                        optionLabel="label"
                        optionValue="data"
                        panelClass="d-design-dropdown"
                        class="d-tree-input col-9 p-0"
                        :style="
                          options.filterOrg != null &&
                          options.filterOrg[
                            store.getters.user.organization_id
                          ] == false
                            ? 'border:2px solid #2196f3'
                            : ''
                        "
                      >
                      </TreeSelect>
                    </div> -->
                    <div class="field col-12 md:col-12 flex">
                      <div class="col-3 p-0 align-items-center flex">
                        Ngày gửi:
                      </div>
                      <div class="col-4 p-0 align-items-center flex">
                        <Calendar
                          class="w-full"
                          v-model="options.start_dateD"
                          placeholder="dd/MM/yy"
                        />
                      </div>
                      <div
                        class="col-1 p-0 align-center align-items-center flex"
                      >
                        <span class="w-full text-center font-bold">-</span>
                      </div>
                      <div class="col-4 p-0 align-items-center flex">
                        <Calendar
                          class="w-full"
                          v-model="options.end_dateD"
                          placeholder="dd/MM/yy"
                        />
                      </div>
                    </div>
                    <div class="field col-12 md:col-12 flex">
                      <div class="col-3 p-0 align-items-center flex">
                        Người gửi:
                      </div>
                      <div class="col-9 p-0 align-items-center flex">
                        <Dropdown
                        v-model="filterCardUserSend"
                        panelClass="d-design-dropdown"
                        :options="listDropdownUser"
                        :filter="true"
                        optionLabel="name"
                        optionValue="code"
              
                        class="w-full"
                        placeholder="Người gửi"
                      >
                        <template #option="slotProps">
                          <div class="country-item flex align-items-center">
                            <Avatar
                              v-bind:label="
                                slotProps.option.avatar
                                  ? ''
                                  : slotProps.option.name.substring(
                                      slotProps.option.name.lastIndexOf(' ') +
                                        1,
                                      slotProps.option.name.lastIndexOf(' ') + 2
                                    )
                              "
                              :image="basedomainURL + slotProps.option.avatar"
                              class="w-3rem h-3rem"
                              size="large"
                              :style="
                                slotProps.option.avatar
                                  ? 'background-color: #2196f3'
                                  : 'background:' +
                                    bgColor[slotProps.option.name.length % 7]
                              "
                              shape="circle"
                              @error="
                                $event.target.src =
                                  basedomainURL + '/Portals/Image/nouser1.png'
                              "
                            />
                            <div class="pt-1 pl-2">
                              {{ slotProps.option.name }}
                            </div>
                          </div>
                        </template>
                      </Dropdown>
                      </div>
                      
                    </div>
                 
                    <div class="col-12 field p-0">
                      <Toolbar class="toolbar-filter custoolbar">
                        <template #start>
                          <Button
                            @click="onRefilterDM()"
                            class="p-button-outlined"
                            label="Xóa"
                          ></Button>
                        </template>
                        <template #end>
                          <Button @click="onFilterDM()" label="Lọc"></Button>
                        </template>
                      </Toolbar>
                    </div>
                  </div>
                </OverlayPanel>
              </span>
            </template>

            <template #end>
            
              <Button
                class="mr-2 p-button-outlined p-button-secondary"
                icon="pi pi-refresh"
                @click="refreshData"
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
        </template>

        <Column
          field="is_order"
          header="STT"
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;max-width:70px"
          bodyStyle="text-align:center;max-width:70px; word-break:break-all"
        ></Column>

        <Column
          field="email_name"
          header="Tên email"
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;max-width:200px"
          bodyStyle="text-align:center;max-width:200px; word-break:break-word"
          :sortable="true"
          >
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
          field="sent_date"
          header="Ngày gửi"
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;max-width:170px"
          bodyStyle="text-align:center;max-width:170px; word-break:break-word"
          :sortable="true"
          filterField="sent_date" dataType="date"
        >
          <template #filter="{ filterModel }">
            <Calendar
              v-model="filterModel.value"
              class="p-column-filter"
              placeholder="dd/MM/yy"
              dateFormat="mm/dd/yy"
            />
          </template>
          <template #body="data">
            <div>
              {{
                moment(new Date(data.data.sent_date)).format("DD/MM/YYYY")
              }}
            </div>
          </template>
        </Column>
        <Column
          field="created_name"
          header="Người gửi"
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;max-width:150px"
          bodyStyle="text-align:center;max-width:150px; word-break:break-word"
        ></Column>
        <!-- <Column
          field="dispatch_book_num"
          header="Số vào sổ"
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;max-width:150px"
          bodyStyle="text-align:center;max-width:150px"
          :sortable="true"
        >
          <template #filter="{ filterModel }">
            <InputText
              type="text"
              v-model="filterModel.value"
              class="p-column-filter"
              placeholder="Từ khoá"
            />
          </template>
        </Column> -->
    
        <Column
          field="doc_code"
          header="Số ký hiệu"
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;max-width:160px"
          bodyStyle="text-align:center;max-width:160px; word-break:break-all"
          :sortable="true"
        >
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
          field="title"
          header="Tiêu đề"
       
          headerStyle="max-width:200px"
          bodyStyle="max-width:200px; word-break:break-word"
       
        >
        </Column>
        <Column
          field="content"
          header="Nội dung"
          class=" text-justify"
          headerStyle="text-align:left"
          bodyStyle="text-align:left; word-break:break-word"
        >
        </Column>
       
        <!-- <Column
          header="Ghi chú"
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;max-width:70px"
          bodyStyle="text-align:center;max-width:70px"
        >
          <template #body="data">
            <div>
           
              <Button @click="showURLSCV(basedomainURL + data.data.file_path)" icon="pi pi-paperclip" v-tooltip.top="'Chi tiết công văn'" class="p-button-rounded p-button-outlined" />
        
            </div>
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
  </div>
</template>
  <style scoped>
.d-lang-table {
  margin: 8px 8px 0px 8px;
  height: calc(100vh - 50px);
}
</style>
  