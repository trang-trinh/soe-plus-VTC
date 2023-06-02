<script setup>
import { ref, inject, onMounted } from "vue";
import { useToast } from "vue-toastification";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import detailsDevice from "../../components/device/detailsDevice.vue";
import { encr } from "../../util/function.js";
import moment from "moment";
const cryoptojs = inject("cryptojs");
import detailsHandover from "../../components/device/detailsHandover.vue";
import {   checkURL } from "../../util/function.js";
//Khai báo
const router = inject("router");
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const basedomainURL = baseURL;
const selectedHandOver = ref();
const taskDateFilter = ref();
const menu_ID = ref();
//Lọc theo ngày

const todayClick = () => {
  taskDateFilter.value = [];
  taskDateFilter.value.push(new Date());
};
const delDayClick = () => {
  taskDateFilter.value = [];
  options.value.start_date = null;
  options.value.end_date = null;
  filterSQL.value = [];
  isDynamicSQL.value = true;
  loadData(true);
};

 
const onDayClick = () => {
  if (taskDateFilter.value == null) taskDateFilter.value = [];
  else {
    options.value.start_date = taskDateFilter.value[0];
    options.value.end_date = taskDateFilter.value[1];
    if (!options.value.end_date)
      options.value.end_date = options.value.start_date;
    filterSQL.value = [];
    if (
      options.value.start_date &&
      options.value.start_date != options.value.end_date
    ) {
      let sDate = new Date(options.value.start_date);
      sDate.setDate(sDate.getDate() - 1);
      options.value.start_date = sDate;
      let filterS = {
        filterconstraints: [
          { value: options.value.start_date, matchMode: "dateAfter" },
        ],
        filteroperator: "and",
        key: "handover_created_date",
      };
      filterSQL.value.push(filterS);
    }
    if (
      options.value.end_date &&
      options.value.start_date != options.value.end_date
    ) {
      let eDate = new Date(options.value.end_date);
      eDate.setDate(eDate.getDate() + 1);
      options.value.end_date = eDate;
      let filterS = {
        filterconstraints: [
          { value: options.value.end_date, matchMode: "dateBefore" },
        ],
        filteroperator: "and",
        key: "handover_created_date",
      };
      filterSQL.value.push(filterS);
    }
    if (
      options.value.start_date &&
      options.value.start_date == options.value.end_date
    ) {
      let filterS1 = {
        filterconstraints: [
          { value: options.value.start_date, matchMode: "dateIs" },
        ],
        filteroperator: "and",
        key: "handover_created_date",
      };
      filterSQL.value.push(filterS1);
    }
  }
  isDynamicSQL.value = true;
  loadData(true);
};
const filters = ref({
  global: { value: null, matchMode: FilterMatchMode.CONTAINS },
  handover_number: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
  handover_created_date: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.DATE_IS }],
  },
  user_verifier_name: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
  user_receiver_name: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
  status: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.EQUALS }],
  },
  handover_created_date: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.DATE_BEFORE }],
  },
  handover_created_date: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.DATE_AFTER }],
  },
});
//Phân trang dữ liệu
const onPage = (event) => {
  options.value.pagesize = event.rows;
  options.value.pageno = event.page;
  loadData();
};
const filterSQL = ref([]);
const isDynamicSQL = ref(false);
const loadDataSQL = () => {
 
  let data = {
    id: menu_ID.value,
    next: options.value.IsNext,
    sqlO: options.value.sort,
    Search: options.value.search,
    PageNo: options.value.pageno,
    PageSize: options.value.pagesize,
    fieldSQLS: filterSQL.value,
  };

  options.value.loading = true;
  axios
    .post(baseURL + "/api/SQL/Filter_accept_device_handover", data, config)
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
        controller: "Card.vue",
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
//Sort
const onSort = (event) => {
  if (event.sortField == null) {
    isDynamicSQL.value = false;
    loadData();
  } else {
    options.value.sort =
      event.sortField + (event.sortOrder == 1 ? " ASC" : " DESC");
    if (event.sortField != " handover_id") {
      options.value.sort +=
        ", handover_id " + (event.sortOrder == 1 ? " ASC" : " DESC");
    }

    isDynamicSQL.value = true;
    loadData();
  }
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
const toast = useToast();
const isFirst = ref(true);
const datalists = ref();
 
const sttCard = ref(1);
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
//ADD log
const addLog = (log) => {
  axios.post(baseURL + "/api/Proc/AddLog", log, config);
};
const options = ref({
  IsNext: true,
  sort: " handover_id DESC",
  sortDM: "card_id DESC",
  search: "",
  pageno: 0,
  pagesize: 20,
  pagenoDM: 0,
  pagesizeDM: 10,
  loading: true,
  totalRecords: null,
  totalRecordsDM: null,
  start_date: null,
  end_date: null,
  next: true,
});
const danhMuc = ref();
const displayDetails = ref(false);
const listAssetsH = ref();
const openDetails = (handover_id) => {
  axios
    .post(
      baseURL + "/api/device_card/getData",
        {
          str: encr(
            JSON.stringify({
        proc: "device_handover_card_list",
        par: [{ par: "handover_id", va: handover_id }],
      }),
            SecretKey,
            cryoptojs
          ).toString(),
        },config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);

      listAssetsH.value = data[0];
      displayDetails.value = true;
    })
    .catch((error) => {});
};
// Upload, remove file

const loadCount = () => {
  axios
    .post(
      baseURL + "/api/device_card/getData",
        {
          str: encr(
            JSON.stringify({
        proc: "device_accept_handover_count",
        par: [
          { par: "user_id", va: store.getters.user.user_id },
          { par: "status", va: null },
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
        options.value.totalRecords = data[0].totalRecords;
        sttCard.value = data[0].totalRecords + 1;
      } else options.value.totalRecords = 0;
    })
    .catch(( ) => {
     
    });
};
//Sửa bản ghi

const cancelAccept = (value) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Xác nhận trả lại thiết bị này không!",
      icon: "warning",
      showCancelButton: true,
      confirmButtonColor: "#3085d6",
      cancelButtonColor: "#d33",
      confirmButtonText: "Có",
      cancelButtonText: "Không",
    })
    .then((result) => {
      if (result.isConfirmed) {
        swal.fire({
          width: 110,
          didOpen: () => {
            swal.showLoading();
          },
        });
        let data = {
          IntID: value.handover_id,
          TextID: value.handover_id + "",
          IntTrangthai: 3,
          BitTrangthai: true,
        };

        axios
          .put(
            baseURL + "/api/device_handover/update_cancel_device_handover",
            data,
            config
          )
          .then((response) => {
            if (response.data.err != "1") {
              swal.close();
              toast.success("Trả lại thiết bị thành công!");
              loadData();
            } else {
              swal.fire({
                title: "Thông báo",
                text: response.data.ms,
                icon: "error",
                confirmButtonText: "OK",
              });
            }
          })
          .catch((error) => {
            swal.close();
            swal.fire({
              title: "Thông báo",
              text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
              icon: "error",
              confirmButtonText: "OK",
            });
          });
      }
    });
};
const sendAccept = (value) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Xác nhận nhận thiết bị này không!",
      icon: "warning",
      showCancelButton: true,
      confirmButtonColor: "#3085d6",
      cancelButtonColor: "#d33",
      confirmButtonText: "Có",
      cancelButtonText: "Không",
    })
    .then((result) => {
      if (result.isConfirmed) {
        swal.fire({
          width: 110,
          didOpen: () => {
            swal.showLoading();
          },
        });
        let data = {
          IntID: value.handover_id,
          TextID: value.handover_id + "",
          IntTrangthai: 2,
          BitTrangthai: true,
        };

        axios
          .put(
            baseURL + "/api/device_handover/update_s_device_handover",
            data,
            config
          )
          .then((response) => {
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xác nhận đã nhận thiết bị thành công!");
              loadData();
            } else {
              swal.fire({
                title: "Thông báo",
                text: response.data.ms,
                icon: "error",
                confirmButtonText: "OK",
              });
            }
          })
          .catch((error) => {
            swal.close();
            swal.fire({
              title: "Thông báo",
              text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
              icon: "error",
              confirmButtonText: "OK",
            });
          });
      }
    });
};
const loadData = (rf) => {
  if (isDynamicSQL.value) {
    loadDataSQL();
    return false;
  }

  if (rf) {
    options.value.loading = true;
    loadCount();
  }
  axios
    .post(
      baseURL + "/api/device_card/getData",
        {
          str: encr(
            JSON.stringify({
        proc: "device_accept_handover_list",
        par: [
          { par: "pageno", va: options.value.pageno },
          { par: "pagesize", va: options.value.pagesize },
          { par: "user_id", va: store.state.user.user_id },
          { par: "status", va: null },
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
          element.is_order =
            options.value.pageno * options.value.pagesize + i + 1;
        });

        datalists.value = data;
      } else {
        datalists.value = [];
      }
      if (isFirst.value) isFirst.value = false;
      options.value.loading = false;
    })
    .catch(( ) => {
       
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;
    
      
    });
};
 
const checkFilter = ref(false);
const filterTrangthai = ref();
const filterCardUser = ref();
const filterCardUserSend = ref();
const closeDetailsHandover = () => {
  displayDetailsHandover.value = false;
  device_handover.value = null;
};
const device_handover = ref();
 
const displayDetailsHandover = ref(false);
const openDetailsHandover = (data) => {
  device_handover.value=data;
  displayDetailsHandover.value = true; 
};
//Tìm kiếm
const searchCard = () => {
  filterSQL.value = [];
  let filterS = {
    filterconstraints: [
      { value: store.getters.user.user_id, matchMode: "equals" },
    ],
    filteroperator: "or",
    key: "user_verifier_id",
  };
  filterSQL.value.push(filterS);
  let filterS1 = {
    filterconstraints: [
      { value: store.getters.user.user_id, matchMode: "equals" },
    ],
    filteroperator: "or",
    key: "user_receiver_id",
  };
  filterSQL.value.push(filterS1);
  
  loadDataSQL();
};
const first = ref(0);
const refreshData = () => {
  options.value.search = "";
  options.value.status = null;
  filterCardUser.value = null;
  filterCardUserSend.value = null;
  filterTrangthai.value = null;
  options.value.start_date = null;
  options.value.end_date = null;
  taskDateFilter.value = [];
  checkFilter.value = false;
  first.value = 0;
  options.value.pageno = 0;
  selectedHandOver.value = [];

  filters.value = {
    global: { value: null, matchMode: FilterMatchMode.CONTAINS },
    handover_number: {
      operator: FilterOperator.AND,
      constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
    },
    handover_created_date: {
      operator: FilterOperator.AND,
      constraints: [{ value: null, matchMode: FilterMatchMode.DATE_IS }],
    },
    user_verifier_name: {
      operator: FilterOperator.AND,
      constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
    },
    user_receiver_name: {
      operator: FilterOperator.AND,
      constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
    },
    status: {
      operator: FilterOperator.AND,
      constraints: [{ value: null, matchMode: FilterMatchMode.EQUALS }],
    },
    
  };
  searchCard();
};

//Xuất excel
onMounted(() => {
    if (!checkURL(window.location.pathname, store.getters.listModule)) {
     //router.back();
  }
  loadData(true);
  return {
    isFirst,
    options,
    danhMuc,
  };
});
</script>
        
        <template>
  <div class="d-container">
    <div class="d-lang-table">
      <DataTable
        class="w-full p-datatable-sm e-sm"
        @page="onPage($event)"
        @filter="onFilter($event)"
        @sort="onSort($event)"
        v-model:filters="filters"
        removableSort
        filterDisplay="menu"
        filterMode="lenient"
        dataKey="handover_id"
        responsiveLayout="scroll"
        :scrollable="true"
        scrollHeight="flex"
        :showGridlines="true"
        :rows="options.pagesize"
        :lazy="true"
        :value="datalists"
        :loading="options.loading"
        :paginator="true"
        :totalRecords="options.totalRecords"
        :row-hover="true"
        v-model:first="first"
        v-model:selection="selectedHandOver"
        :pageLinkSize="options.pagesize"
        paginatorTemplate="FirstPageLink PrevPageLink PageLinks  NextPageLink LastPageLink    RowsPerPageDropdown"
        :rowsPerPageOptions="[20, 30, 50, 100, 200]"
      >
        <template #header>
          <div>
            <h3 class="module-title my-2 ml-1">
              <i class="pi pi-sort-alt"></i> Xác nhận cấp phát ({{
                options.totalRecords ? options.totalRecords : 0
              }})
            </h3>
          </div>
          <Toolbar class="d-toolbar p-0 py-3 surface-50">
            <template #start>
              <span class="p-input-icon-left">
                <i class="pi pi-search" />
                <InputText
                  v-model="options.search"
                  @keyup.enter="searchCard()"
                  type="text"
                  spellcheck="false"
                  placeholder="Tìm kiếm"
                />
                <!-- :class="checkFilter?'':'p-button-secondary'" -->
              </span>
              <Calendar
                placeholder="Lọc theo ngày lập"
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

              <!-- <TreeSelect
                  style="margin-left: 24px; min-width: 200px"
                  @change="selectTree()"
                  v-model="menu_IDNode"
                  :options="danhMuc"
                  placeholder="Tất cả tin tức"
                ></TreeSelect> -->
            </template>

            <template #end>
              <Button
                class="mr-2 p-button-outlined p-button-secondary"
                icon="pi pi-refresh"
                @click="refreshData"
              />
            </template>
          </Toolbar>
        </template>

        <Column
          :sortable="true"
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;max-width:70px;height:50px"
          bodyStyle="text-align:center;max-width:70px"
          field="is_order"
          header="STT"
        >
        </Column>
        <Column
          headerStyle="text-align:center;max-width:180px;height:50px"
          bodyStyle="text-align:center;max-width:180px"
          field="handover_number"
          class="align-items-center justify-content-center text-center"
          header="Số phiếu"
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
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;height:50px"
          bodyStyle="text-align:center"
          field="user_receiver_name"
          header="Người sử dụng"
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
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;height:50px"
          bodyStyle="text-align:center"
          field="user_verifier_name"
          header="Người xác nhận"
          :sortable="true"
          ><template #filter="{ filterModel }">
            <InputText
              type="text"
              v-model="filterModel.value"
              class="p-column-filter"
              placeholder="Từ khoá"
            />
          </template>
        </Column>
        <Column
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;height:50px;max-width:100px"
          bodyStyle="text-align:center;max-width:100px"
          field="assets"
          header="Tài sản"
          ><template #body="data">
            <div>
              <Button
                @click="openDetails(data.data.handover_id)"
                :label="data.data.assets"
                class="p-button-rounded"
              />
            </div>
          </template>
        </Column>
        <Column
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;max-width:120px;height:50px"
          bodyStyle="text-align:center;max-width:120px"
          field="handover_created_date"
          header="Ngày lập"
        >
          <template #body="data">
            <div>
              {{
                moment(new Date(data.data.handover_created_date)).format(
                  "DD/MM/YYYY"
                )
              }}
            </div>
          </template>
        </Column>

        <Column
          headerStyle="text-align:center;height:50px"
          bodyStyle="text-align:center"
          field="handover_type"
          header="Kiểu bàn giao"
          class="align-items-center justify-content-center text-center"
        >
          <template #body="data">
            <div>
              {{
                data.data.handover_type == 0
                  ? "Bàn giao 2 bên"
                  : "Bàn giao 3 bên"
              }}
            </div>
          </template>
        </Column>

        <Column
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;height:50px;max-width:170px"
          bodyStyle="text-align:center;max-width:170px"
          field="status"
          header="Trạng thái"
        >
          <template #body="data">
            <div class="w-full">
              <Chip
                v-if="data.data.status == 0"
                label="Đã tạo"
                class="
                  w-full
                  surface-200
                  justify-content-center
                  p-button-status-d
                "
              />
              <Chip
                v-else-if="
                  data.data.status == 1 &&
                  data.data.is_receiver_accept &&
                  data.data.handover_type == 1
                "
                label="Chờ người xác nhận duyệt"
                class="
                  w-full
                  bg-green-300
                  justify-content-center
                  p-button-status-d
                "
              />
              <Chip
                v-else-if="
                  data.data.status == 1 &&
                  data.data.is_verifier_accept &&
                  data.data.handover_type == 1
                "
                label="Chờ người nhận duyệt"
                class="
                  w-full
                  bg-green-300
                  justify-content-center
                  p-button-status-d
                "
              />
              <Chip
                v-else-if="data.data.status == 1"
                label="Chờ xác nhận"
                class="
                  w-full
                  bg-pink-300
                  justify-content-center
                  p-button-status-d
                "
              />

              <Chip
                v-else-if="data.data.status == 2"
                label="Đã nhận"
                class="
                  w-full
                  bg-blue-300
                  justify-content-center
                  p-button-status-d
                "
              />
              <Chip
                v-else-if="data.data.status == 3"
                label="Trả lại"
                style="background-color: red; color: white"
                class="w-full justify-content-center p-button-status-d"
              />
            </div>
          </template>
        </Column>

        <Column
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;max-width:150px;height:50px"
          bodyStyle="text-align:center;max-width:150px"
          header="Chức năng"
        >
          <template #body="data">
            <div>
              <Button
                v-tooltip.top="'Chi tiết phiếu'"
                class="
                  p-button-rounded p-button-secondary p-button-outlined
                  mx-1
                "
                @click="openDetailsHandover(data.data)"
                type="button"
                icon="pi pi-info-circle"
              ></Button>

              <Button
                v-if="
                  data.data.status == 1 &&
                  (store.getters.user.is_admin ||   (data.data.user_receiver_id == store.getters.user.user_id &&
                    !data.data.is_receiver_accept) ||
                    (data.data.user_verifier_id == store.getters.user.user_id &&
                      !data.data.is_verifier_accept) ||
                    data.data.handover_type == 0)
                "
                v-tooltip.top="'Xác nhận thiết bị'"
                class="
                  p-button-rounded p-button-secondary p-button-outlined
                  mx-1
                "
                @click="sendAccept(data.data)"
                type="button"
                icon="pi pi-check"
              ></Button>
              <Button
                v-if="
                  data.data.status == 1 &&
                  ((data.data.user_receiver_id == store.getters.user.user_id &&
                    !data.data.is_receiver_accept) ||
                    (data.data.user_verifier_id == store.getters.user.user_id &&
                      !data.data.is_verifier_accept) ||
                    data.data.handover_type == 0)
                "
                v-tooltip.top="'Trả lại'"
                class="
                  p-button-rounded p-button-secondary p-button-outlined
                  mx-1
                "
                @click="cancelAccept(data.data)"
                type="button"
                icon="pi pi-times"
              ></Button>
            </div>
          </template>
        </Column>
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
  <Sidebar
    class="p-sidebar-lg"
    :showCloseIcon="false"
    v-model:visible="displayDetails"
    position="right"
  >
    <div class="w-full format-center">
      <h1>Danh sách thiết bị kèm theo</h1>
    </div>
    <div
      class="w-full p-0 pt-2"
      v-for="(item, index) in listAssetsH"
      :key="index"
    >
      <div
        style="border-radius: 10px"
        class="
          product-item
          border-3 border-solid border-round-3xl border-blue-200
          surface-50
          p-2
        "
      >
        <div class="image-container pr-2">
          <img
            :src="
              item.image
                ? basedomainURL + item.image
                : basedomainURL + '/Portals/Image/noimg.jpg'
            "
            style="object-fit: cover; width: 125px; height: 75px"
          />
        </div>
        <div class="product-list-detail">
          <h5 class="my-2 text-justify">
            {{ item.device_name }}
          </h5>

          <div class="flex pb-2">
            <div class="w-full">
              <i class="pi pi-tag product-category-icon"></i>
              <span class="product-category">{{ item.device_number }}</span>
            </div>
            <div class="w-full">
              <i class="pi pi-qrcode product-category-icon"></i>
              <span class="product-category">{{ item.barcode_id }}</span>
            </div>
          </div>
          <div class="flex">
            <div class="w-full">
              <i class="pi pi-home product-category-icon"></i>
              <span class="product-category">
                {{ item.warehouse_name }}
              </span>
            </div>
            <div class="w-full">
              <i class="pi pi-shopping-cart product-category-icon"></i>
              <span class="product-category">
                {{ moment(new Date(item.purchase_date)).format("DD/MM/YYYY") }}
              </span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </Sidebar>
  <Dialog
    header="Phiếu cấp phát"
    v-model:visible="displayDetailsHandover"
    :maximizable="true"
    :style="{ width: '70vw' }"
  >
    <detailsHandover     :handover="device_handover" :check="0"   />
    <template #footer>
      <Button
        @click="closeDetailsHandover"
        label="Đóng"
        icon="pi pi-times"
        autofocus
      />
    </template>
  </Dialog>

 
</template>
<style scoped>
.product-item {
  display: flex;
  align-items: center;
  padding: 0.2rem;
  width: 100%;
}
.product-list-detail {
  flex: 1 1 0;
}

.product-list-action {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
}

.product-category-icon {
  vertical-align: middle;
  margin-right: 0.5rem;
  font-size: 0.875rem;
}

.product-category {
  vertical-align: middle;
  line-height: 1;
  font-size: 0.875rem;
}

@media screen and (max-width: 576px) {
  .product-item {
    flex-wrap: wrap;
  }
  .image-container {
    width: 100%;
    text-align: center;
  }

  img {
    margin: 0 0 1rem 0;
    width: 100px;
  }
}
</style>
        <style scoped>
.ck-editor__editable {
  max-height: 500px !important;
}
.d-container {
  background-color: #f5f5f5;
}

.d-lang-header {
  background-color: #ffff;
  padding: 12px 8px 0px 8px;
  margin: 8px 8px 0px 8px;
  height: 33px;
}
.d-lang-header h3,
i {
  font-weight: 600;
}
.d-module-title {
  margin: 0;
}
.d-lang-table {
  margin: 0px 8px 0px 8px;
  height: calc(100vh - 50px);
}

.d-toolbar {
  border: unset;
  outline: unset;
  background-color: #fff;
  margin: 0px 8px 0px 8px;
}

.d-btn-function {
  border-radius: 50%;
  margin-left: 6px;
}
.inputanh {
  border: 1px solid #ccc;
  width: 100%;
  height: 200px;
  cursor: pointer;
  padding: 1px;
  object-fit: contain;
  background-color: #eeeeee;
}
.ipnone {
  display: none;
}

.d-avatar-device_handover {
  position: relative;
  width: 100%;
  height: 350px;
}
.d-avatar-device_handover img {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  object-fit: contain;
}
.multi-width {
  max-width: 500px !important;
}
.toolbar-filter {
  border: unset;
  outline: unset;
  background-color: #fff;
  padding-bottom: 0px;
}
.sel-placeholder::placeholder {
  text-align: center;
  position: absolute;
  top: 0;
}
</style>
              
    <style lang="scss" scoped>
::v-deep(.p-calendar) {
  .p-button.p-button-icon-only {
    width: 3.5rem !important;
  }
}
</style>
    