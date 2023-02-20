<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import { encr, checkURL } from "../../util/function.js";
//Khai báo
const router = inject("router");
const cryoptojs = inject("cryptojs");
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const item = "/Portals/Mau Excel/Mẫu Excel Chức vụ.xlsx";
const emitter = inject("emitter");
const isDynamicSQL = ref(false);
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const first = ref(0);

const rules = {
  current_number: {
    required,
    $errors: [
      {
        $property: "current_number",
        $validator: "required",
        $message: "Số hiệu hiện tại không được để trống!",
      },
    ],
  },

  text_symbols: {
    required,
    $errors: [
      {
        $property: "text_symbols",
        $validator: "required",
        $message: "Ký hiệu văn bản không được để trống!",
      },
    ],
  },
  code_number: {
    required,
    $errors: [
      {
        $property: "code_number ",
        $validator: "required",
        $message: "Loại phiếu không được để trống!",
      },
    ],
  },
};
//Nơi nhận EMIT từ component

emitter.on("emitData", (obj) => {
  switch (obj.type) {
    case "hideSidebarGU":
      showSidebarUser.value = false;
      break;
  }
});
const listMSH = ref([
  { name: "TS_PhieuTaiSan", code: "Thẻ thiết bị" },
  { name: "TS_PhieuBanGiao", code: "Phiếu cấp phát" },
  { name: "TS_PhieuSuaChua", code: "Phiếu sửa chữa" },
  { name: "TS_PhieuKiemKe", code: "Phiếu kiểm kê" },
  { name: "TS_PhieuThuHoi", code: "Phiếu thu hồi" },
]);

// { name: "TS_PhieuTongHopSuaChua",code:"Phiếu tổng hợp sửa chữa"},
//
//   { name: "TS_PhieuThanhLy",code:"Phiếu thanh lý"},
//   { name: "TS_PhieuThatThoat",code:"Phiếu thất thoát"},

//   { name: "TS_PhieuTaiSan",code:"Phiếu thiết bị"},
//   { name: "TS_PhieuDieuChuyen",code:"Phiếu điều chuyển"},
const device_config_number = ref({
  current_number: "",
  status: true,
  is_order: 1,
});
const selectedWarehouses = ref();
const submitted = ref(false);
const v$ = useVuelidate(rules, device_config_number);
const issaveWarehouse = ref(false);
const datalists = ref();
const toast = useToast();
const basedomainURL = fileURL;
const checkDelList = ref(false);
const options = ref({
  IsNext: true,
  id: "config_number_id",
  sort: "config_number_id DESC",
  SearchText: null,
  PageNo: 0,
  PageSize: 20,
  loading: true,
  totalRecords: null,
});
//Thêm log
const addLog = (log) => {
  axios.post(baseURL + "/api/Proc/AddLog", log, config);
};
//Lấy số bản ghi
const loadCount = () => {
  axios
    .post(
      baseURL + "/api/device_card/getData",
        {
          str: encr(
            JSON.stringify({
        proc: "device_config_number_count",
        par: [
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
        options.value.totalRecords = data[0].totalRecords;
        sttWarehouse.value = options.value.totalRecords + 1;
      }
    })
    .catch((error) => {
       
    });
};
const showSidebarUser = ref(false);
 
//Lấy dữ liệu ngôn ngữ
const loadData = (rf) => {
  datalists.value = [];
  if (rf) {
    if (isDynamicSQL.value) {
      loadDataSQL();
      return;
    }
    if (rf) {
      loadCount();
    }
    axios
      .post(
        baseURL + "/api/Proc/CallProc",
        {
          proc: "device_config_number_list",
          par: [
            { par: "pageno", va: options.value.PageNo },
            { par: "pagesize", va: options.value.PageSize },
            { par: "user_id", va: store.state.user.user_id },
            { par: "status", va: null },
          ],
        },
        config
      )
      .then((response) => {
        let data = JSON.parse(response.data.data)[0];
        data.forEach((element, i) => {
          element.STT = options.value.PageNo * options.value.PageSize + i + 1;
        });
        if (isFirst.value) isFirst.value = false;
        datalists.value = data;
        options.value.loading = false;
      })
      .catch((error) => {
        toast.error("Tải dữ liệu không thành công!");
    
        options.value.loading = false;

        if (error && error.status === 401) {
          swal.fire({
            title: "Thông báo",
            text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
            icon: "error",
            confirmButtonText: "OK",
          });
          store.commit("gologout");
        }
      });
  }
 
};
 
//Hiển thị dialog
const headerDialog = ref();
const displayBasic = ref(false);
const openBasic = (str) => {
  submitted.value = false;
  device_config_number.value = {
    current_number: null,
    price: null,
    is_order: sttWarehouse.value,
    status: true,
    organization_id: store.getters.user.organization_id,
    type: true,
  };
  issaveWarehouse.value = false;
  headerDialog.value = str;
  displayBasic.value = true;
};
const closeDialog = () => {
  device_config_number.value = {
    current_number: "",
    is_order: 1,
    status: true,
  };
  displayBasic.value = false;
  loadData(true);
};

//Thêm bản ghi
const saveWarehouse = (isFormValid) => {
  submitted.value = true;
  if (!isFormValid) {
    return;
  }

  if (!device_config_number.value.year_fake) {
    return;
  }

  if (!device_config_number.value.code_number) {
    return;
  }

  if (device_config_number.value.text_symbols.length > 250) {
    swal.fire({
      title: "Thông báo",
      text: "Ký hiệu văn bản không được lớn hơn 250 ký tự.",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }

  if (device_config_number.value.year_fake)
    device_config_number.value.year =
      device_config_number.value.year_fake.getFullYear();
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  if (!issaveWarehouse.value) {
    axios
      .post(
        baseURL + "/api/device_config_number/add_device_config_number",
        device_config_number.value,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Thêm số hiệu thành công!");

          closeDialog();
        }
      })
      .catch((error) => {
        swal.close();
        swal.fire({
          title: "Thông báo",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!" + error.message,
          icon: "error",
          confirmButtonText: "OK",
        });
      });
  } else {
    axios
      .put(
        baseURL + "/api/device_config_number/update_device_config_number",
        device_config_number.value,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa số hiệu thành công!");

          closeDialog();
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
};

const sttWarehouse = ref();
//Thêm bản ghi con
const isChirlden = ref(false);

//Sửa bản ghi
const editWarehouse = (dataPlace) => {
  submitted.value = false;
  device_config_number.value = dataPlace;
  dataPlace.year_fake = new Date("1/1/" + dataPlace.year);
  isChirlden.value = false;

  headerDialog.value = "Sửa số hiệu";
  issaveWarehouse.value = true;
  displayBasic.value = true;
};
//Xóa bản ghi
const delWarehouse = (Warehouse) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá số hiệu này không!",
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

        axios
          .delete(
            baseURL + "/api/device_config_number/delete_device_config_number",
            {
              headers: { Authorization: `Bearer ${store.getters.token}` },
              data: Warehouse != null ? [Warehouse.config_number_id] : 1,
            }
          )
          .then((response) => {
            swal.close();
            if (response.data.err == "0") {
              swal.close();
              toast.success("Xoá số hiệu thành công!");
              if (
                (options.value.totalRecords - Warehouse.length) % 2 == 0 &&
                options.value.PageNo > 0
              ) {
                options.value.PageNo = options.value.PageNo - 1;
              }
              loadData(true);
            } else {
              swal.fire({
                title: "",
                text: response.data.ms,
                icon: "error",
                confirmButtonText: "OK",
              });
            }
          })
          .catch((error) => {
            swal.close();
            if (error.status === 401) {
              swal.fire({
                title: "Thông báo",
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                icon: "error",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
};
 
//Sort
const onSort = (event) => {
  options.value.PageNo = 0;
  first.value = 0;
  if (event.sortField == null) {
    isDynamicSQL.value = false;
    loadData();
  } else {
    options.value.sort =
      event.sortField + (event.sortOrder == 1 ? " ASC" : " DESC");
    if (event.sortField == "STT") {
      options.value.sort =
        "is_order" + (event.sortOrder == 1 ? " ASC" : " DESC");
    }
    isDynamicSQL.value = true;
    loadDataSQL();
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
  options.value.PageNo = 0;
  first.value = 0;
  options.value.id = null;
  loadDataSQL();
};
const filterSQL = ref([]);
const isFirst = ref(true);
const loadDataSQL = () => {
  datalists.value = [];
  let data = {
    id: "config_number_id",
    sqlS: filterTrangthai.value != null ? filterTrangthai.value : null,
    sqlO: options.value.sort,
    Search: options.value.SearchText,
    PageNo: options.value.PageNo,
    next: true,
    PageSize: options.value.PageSize,
    sqlF: null,
    fieldSQLS: filterSQL.value,
  };
  options.value.loading = true;
  axios
    .post(baseURL + "/api/SQL/Filter_device_config_number", data, config)
    .then((response) => {
      let dt = JSON.parse(response.data.data);
      let data = dt[0];
      if (data.length > 0) {
        data.forEach((element, i) => {
          element.STT = options.value.PageNo * options.value.PageSize + i + 1;
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
    
      if (error && error.status === 401) {
        swal.fire({
          title: "Thông báo",
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
//Tìm kiếm
const searchWarehouses = () => {
  isDynamicSQL.value = true;
  loadData(true);
};

//Checkbox
const onCheckBox = (value) => {
  options.value.loading = true;
  let data = {
    IntID: value.config_number_id,
    TextID: value.config_number_id + "",
    IntTrangthai: 1,
    BitTrangthai: value.status,
  };
  if (
    store.state.user.is_super == true ||
    store.state.user.user_id == value.created_by ||
    (store.state.user.role_id == "admin" &&
      store.state.user.organization_id == value.organization_id)
  ) {
    axios
      .put(
        baseURL + "/api/device_config_number/update_s_device_config_number",
        data,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa số hiệu thành công!");
          loadData(true);
          closeDialog();
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
  } else {
    swal.fire({
      title: "Thông báo!",
      text: "Bạn không có quyền chỉnh sửa! Chỉ có Quản trị viên đơn vị hoặc Quản trị viên hệ thống mới có quyền chỉnh sửa mục này",
      icon: "error",
      confirmButtonText: "OK",
    });
    loadData(true);
  }
};
//Xóa nhiều
const deleteList = () => {
  let listId = new Array(selectedWarehouses.value.length);
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá danh sách này không!",
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
        selectedWarehouses.value.forEach((item) => {
          listId.push(item.config_number_id);
        });
        axios
          .delete(
            baseURL + "/api/device_config_number/delete_device_config_number",
            {
              headers: { Authorization: `Bearer ${store.getters.token}` },
              data: listId != null ? listId : 1,
            }
          )
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá danh sách thành công!");
              checkDelList.value = false;
              if (
                (options.value.totalRecords - listId.length) % 2 == 0 &&
                options.value.PageNo > 0
              ) {
                options.value.PageNo = options.value.PageNo - 1;
              }
              loadData(true);
              Refresh();
            } else {
              swal.fire({
                title: "",
                text: response.data.ms,
                icon: "error",
                confirmButtonText: "OK",
              });
            }
          })
          .catch((error) => {
            swal.close();
            if (error.status === 401) {
              swal.fire({
                title: "Thông báo",
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                icon: "error",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
};
watch(selectedWarehouses, () => {
  if (selectedWarehouses.value.length > 0) {
    checkDelList.value = true;
  } else {
    checkDelList.value = false;
  }
});
const onPage = (event) => {
  if (event.rows != options.value.PageSize) {
    options.value.PageSize = event.rows;
  }
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

    options.value.id = datalists.value[datalists.value.length - 1].place_id;
    options.value.IsNext = true;
  } else if (event.page < options.value.PageNo) {
    //Trang trước
    options.value.id = datalists.value[0].place_id;
    options.value.IsNext = false;
  }
  options.value.PageNo = event.page;
  loadData(true);
};
const Refresh = () => {
  filterTrangthai.value = "";
  selectedWarehouses.value = [];
  first.value = 0;
  options.value = {
    IsNext: true,
    sort: "created_date DESC",
    SearchText: "",
    PageNo: 0,
    PageSize: 20,
    loading: true,
    totalRecords: null,
  };
  checkFilter.value = false;
  styleObj.value = "";
  datalists.value = [];
  filterSQL.value = [];
  filters.value = {
    current_number: {
      operator: FilterOperator.AND,
      constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
    },
    text_symbols: {
      operator: FilterOperator.AND,
      constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
    },
  };
  isDynamicSQL.value = false;
  loadData(true);
  loadCount();
};
const op = ref();
const filterTrangthai = ref();
const checkFilter = ref(false);

const toggle = (event) => {
  op.value.toggle(event);
};
const trangThai = ref([
  { name: "Hiển thị", code: 1 },
  { name: "Không hiển thị", code: 0 },
]);
 
const reFilter = () => {
  isDynamicSQL.value = false;
  checkFilter.value = false;
  filterTrangthai.value = null;
  loadData(true);
};
const filter = (valueST) => {
  filterSQL.value = [];
  checkFilter.value = true;
  let filterS = {
    filterconstraints: [{ value: valueST, matchMode: "equals" }],
    filteroperator: "and",
    key: "status",
  };
  filterSQL.value.push(filterS);
  isDynamicSQL.value = true;
  loadData(true);
};
const filters = ref({
  current_number: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
  text_symbols: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
});
const Imp = ref(false); 
let files = [];
const removeFile = (event) => {
  files = [];
};
const selectFile = (event) => {
  event.files.forEach((element) => {
    files.push(element);
  });
};
const Upload = () => {
  Imp.value = false;
  let formData = new FormData();
  for (var i = 0; i < files.length; i++) {
    let file = files[i];
    formData.append("url_file", file);
  }
  axios
    .post(baseURL + "/api/ImportExcel/Import_Warehouse", formData, config)
    .then((response) => {
      toast.success("Nhập dữ liệu thành công");
      isDynamicSQL.value = false;
      loadData(true);
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
};
const styleObj = ref();
 
 
onMounted(() => {  if (!checkURL(window.location.pathname, store.getters.listModule)) {
     //router.back();
  }
  loadData(true);
  loadCount();
  return {
    datalists,
    options,
    loadData,
    loadCount,
    openBasic,
    closeDialog,
    basedomainURL,
    saveWarehouse,
    isFirst,
    searchWarehouses,
    onCheckBox,
    selectedWarehouses,
    deleteList,
  };
});
</script>
            <template>
  <div class="main-layout true flex-grow-1 p-0">
    <DataTable
      v-model:first="first"
      :show-gridlines="true"
      :value="datalists"
      :paginator="true"
      :rows="options.PageSize"
      paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
      :rowsPerPageOptions="[20, 30, 50, 100, 200]"
      :scrollable="true"
      scrollHeight="flex"
      :loading="options.loading"
      dataKey="config_number_id"
      v-model:selection="selectedWarehouses"
      @page="onPage($event)"
      @sort="onSort($event)"
      @filter="onFilter($event)"
      :lazy="true"
      v-model:filters="filters"
      filterDisplay="menu"
      filterMode="lenient"
      :totalRecords="options.totalRecords"
      :row-hover="true"
    >
      <template #header>
        <h3 class="module-title mt-0 ml-1 mb-2">
          <i class="pi pi-cog"></i> Danh sách số hiệu ({{
            options.totalRecords
          }})
        </h3>

        <Toolbar class="w-full custoolbar">
          <template #start>
            <span class="p-input-icon-left">
              <i class="pi pi-search" />
              <InputText
                v-model="options.SearchText"
                @keyup.enter="searchWarehouses"
                type="text"
                spellcheck="false"
                placeholder="Tìm kiếm"
              />
            </span>
            <Button
              :class="
                filterTrangthai != null && checkFilter
                  ? ''
                  : 'p-button-secondary p-button-outlined'
              "
              type="button"
              class="ml-2"
              icon="pi pi-filter"
              @click="toggle"
              aria:haspopup="true"
              aria-controls="overlay_panel"
              v-tooltip.top="'Bộ lọc'"
              :style="[styleObj]"
            />
            <OverlayPanel
              ref="op"
              appendTo="body"
              class="p-0 m-0"
              :showCloseIcon="false"
              id="overlay_panel"
              :style="
                store.state.user.is_super == 1 ? 'width:25rem' : 'width:300px'
              "
            >
              <div class="grid formgrid m-0">
                <div class="flex field col-12 p-0 align-items-center">
                  <div class="col-4">Mặc định</div>
                  <div class="col-8">
                    <Dropdown
                      class="col-12 p-0 m-0"
                      v-model="filterTrangthai"
                      :options="trangThai"
                      optionLabel="name"
                      optionValue="code"
                      placeholder="Mặc định"
                    />
                  </div>
                </div>
                <div class="flex col-12 p-0">
                  <Toolbar
                    class="border-none surface-0 outline-none pb-0 w-full"
                  >
                    <template #start>
                      <Button
                        @click="reFilter()"
                        class="p-button-outlined"
                        label="Xóa"
                      ></Button>
                    </template>
                    <template #end>
                      <Button
                        @click="filter(filterTrangthai)"
                        label="Lọc"
                      ></Button>
                    </template>
                  </Toolbar>
                </div>
              </div>
            </OverlayPanel>
          </template>

          <template #end>
            <Button
              v-if="checkDelList"
              @click="deleteList()"
              label="Xóa"
              icon="pi pi-trash"
              class="mr-2 p-button-danger"
            />
            <Button
              @click="openBasic('Thêm mới')"
              label="Thêm mới"
              icon="pi pi-plus"
              class="mr-2"
            />
            <Button
              @click="Refresh"
              class="mr-2 p-button-outlined p-button-secondary"
              icon="pi pi-refresh"
              v-tooltip.top="'Tải lại'"
            />

            <!-- <Button
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
            /> -->
          </template>
        </Toolbar></template
      >
      <Column
        selectionMode="multiple"
        headerStyle="text-align:center;max-width:75px;height:50px"
        bodyStyle="text-align:center;max-width:75px"
        class="align-items-center justify-content-center text-center"
      ></Column>
      <Column
        field="STT"
        header="STT"
        :sortable="true"
        headerStyle="text-align:center;max-width:75px;height:50px"
        bodyStyle="text-align:center;max-width:75px"
        class="align-items-center justify-content-center text-center"
      >
      </Column>

      <Column
        field="current_number"
        header="Số hiện tại"
        :sortable="true"
        headerStyle="text-align:center;jusst;max-width:170px;height:50px"
        bodyStyle="text-align:center;max-width:170px;"
        class="align-items-center justify-content-center text-center"
      >
        <template #filter="{ filterModel }">
          <InputText
            type="text"
            v-model="filterModel.value"
            class="p-column-filter"
            placeholder="Từ khoá"
          /> </template
      ></Column>

      <Column
        field="year"
        header="Năm"
        headerStyle="text-align:center;jusst;max-width:100px;height:50px"
        bodyStyle="text-align:center;max-width:100px;"
        class="align-items-center justify-content-center text-center"
      >
      </Column>
      <Column
        field="text_symbols"
        header="Ký hiệu"
        :sortable="true"
        headerStyle="text-align:left;height:50px"
        bodyStyle="text-align:left"
      >
        <template #filter="{ filterModel }">
          <InputText
            type="text"
            v-model="filterModel.value"
            class="p-column-filter"
            placeholder="Từ khoá"
          /> </template
      ></Column>
      <Column
        field="agency_issued"
        header="Cơ quan ban hành"
        headerStyle="text-align:center;jusst;max-width:200px;height:50px"
        bodyStyle="text-align:center;max-width:200px;"
        class="align-items-center justify-content-center text-center"
      >
      </Column>
      <Column
        field="code_number"
        header="Loại phiếu"
        headerStyle="text-align:center;jusst;max-width:150px;height:50px"
        bodyStyle="text-align:center;max-width:150px;"
        class="align-items-center justify-content-center text-center"
        ><template #body="data">
          <div>
            <div v-if="data.data.code_number == 'TS_PhieuTaiSan'">
              Thẻ thiết bị
            </div>
            <div v-if="data.data.code_number == 'TS_PhieuBanGiao'">
              Phiếu cấp phát
            </div>
            <div v-if="data.data.code_number == 'TS_PhieuSuaChua'">
              Phiếu sửa chữa
            </div>
            <div v-if="data.data.code_number == 'TS_PhieuKiemKe'">
              Phiếu kiểm kê
            </div>
            <div v-if="data.data.code_number == 'TS_PhieuThuHoi'">
              Phiếu thu hồi
            </div>
          </div>
        </template>
      </Column>
      <Column
        field="status"
        header="Mặc định"
        headerStyle="text-align:center;jusst;max-width:120px;height:50px"
        bodyStyle="text-align:center;max-width:120px;"
        class="align-items-center justify-content-center text-center"
      >
        <template #body="data">
          <Checkbox
            :binary="data.data.status"
            v-model="data.data.status"
            @click="onCheckBox(data.data)"
             :disabled="data.data.status"
          />
        </template>
      </Column>

      <Column
        header="Chức năng"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:150px;height:50px"
        bodyStyle="text-align:center;max-width:150px"
      >
        <template #body="data">
          <div
            v-if="
              store.state.user.is_super == true ||
              store.state.user.user_id == data.data.created_by ||
              (store.state.user.role_id == 'admin' &&
                store.state.user.organization_id == data.data.organization_id)
            "
          >
            <Button
              @click="editWarehouse(data.data)"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-pencil"
              v-tooltip.top="'Sửa'"
            ></Button>
            <Button
              @click="delWarehouse(data.data, true)"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-trash"
              v-tooltip.top="'Xóa'"
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
  <Dialog
    :maximizable="true"
    :header="headerDialog"
    v-model:visible="displayBasic"
    :style="{ width: '30vw' }"
    :modal="true"
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="col-12 md:col-12 p-0">
          <div class="field col-12 md:col-12 flex align-items-center">
            <label class="col-4 text-left p-0"
              >Số hiện tại <span class="redsao">(*)</span></label
            >
            <InputNumber
              v-model="device_config_number.current_number"
              spellcheck="false"
              class="col-8 ip36 px-0"
              :class="{ 'p-invalid': v$.current_number.$invalid && submitted }"
              :min="1"
            />
          </div>
          <div
            v-if="
              (v$.current_number.$invalid && submitted) ||
              v$.current_number.$pending.$response
            "
            style="display: flex"
            class="field col-12 md:col-12"
          >
            <div class="col-4 text-left"></div>
            <small class="col-8 p-error p-0">
              <span class="col-12 p-0">{{
                v$.current_number.required.$message
                  .replace("Value", "Tên số hiệu")
                  .replace("is required", "không được để trống")
              }}</span>
            </small>
          </div>
        </div>
        <div class="col-12 md:col-12 p-0">
          <div class="field col-12 md:col-12 flex align-items-center">
            <label class="col-4 text-left p-0"
              >Năm <span class="redsao">(*)</span></label
            >
            <Calendar
              class="w-full"
              inputId="yearpicker"
              v-model="device_config_number.year_fake"
              view="year"
              dateFormat="yy"
              :class="{
                'p-invalid': !device_config_number.year_fake && submitted,
              }"
            />
          </div>
          <div
            v-if="!device_config_number.year_fake && submitted"
            style="display: flex"
            class="field col-12 md:col-12"
          >
            <div class="col-4 text-left"></div>
            <small class="col-8 p-error p-0">
              <span class="col-12 p-0"> Năm không được để trống</span>
            </small>
          </div>
        </div>
        <div class="col-12 md:col-12 p-0">
          <div class="field col-12 md:col-12 flex align-items-center">
            <label class="col-4 text-left p-0"
              >Ký hiệu văn bản <span class="redsao">(*)</span></label
            >
            <InputText
              v-model="device_config_number.text_symbols"
              spellcheck="false"
              class="col-8 ip36 px-2"
              :class="{ 'p-invalid': v$.text_symbols.$invalid && submitted }"
            />
          </div>
          <div
            v-if="
              (v$.text_symbols.$invalid && submitted) ||
              v$.text_symbols.$pending.$response
            "
            style="display: flex"
            class="field col-12 md:col-12"
          >
            <div class="col-4 text-left"></div>
            <small class="col-8 p-error p-0">
              <span class="col-12 p-0">{{
                v$.text_symbols.required.$message
                  .replace("Value", "Ký hiệu văn bản")
                  .replace("is required", "không được để trống")
              }}</span>
            </small>
          </div>
        </div>
        <div class="field col-12 md:col-12 flex align-items-center">
          <label class="col-4 text-left p-0">Cơ quan ban hành </label>
          <InputText
            v-model="device_config_number.agency_issued"
            spellcheck="false"
            class="col-8 ip36 px-2"
          />
        </div>
        <div class="col-12 md:col-12 p-0">
          <div class="field col-12 md:col-12 flex align-items-center">
            <label class="col-4 text-left p-0"
              >Loại  phiếu <span class="redsao">(*)</span></label
            >
            <Dropdown
              v-model="device_config_number.code_number"
              :options="listMSH"
              :filter="true"
              optionLabel="code"
              optionValue="name"
              class="col-8 p-0"
              panelClass="d-design-dropdown"
              :class="{
                'p-invalid': !device_config_number.code_number && submitted,
              }"
              placeholder="----- Loại phiếu -----"
              :showClear="true"
            />
          </div>
          <div
            v-if="!device_config_number.code_number && submitted"
            style="display: flex"
            class="field col-12 md:col-12"
          >
            <div class="col-4 text-left"></div>
            <small class="col-8 p-error p-0">
              <span class="col-12 p-0"> Loại phiếu không được để trống</span>
            </small>
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <label class="col-4 text-left p-0">STT</label>
          <InputNumber
            v-model="device_config_number.is_order"
            spellcheck="false"
            class="col-8 ip36 p-0"
          />
        </div>
        <div class="field col-12 md:col-12 flex">
          <label style="vertical-align: text-bottom" class="col-4 text-left p-0"
            >Mặc định
          </label>
          <InputSwitch
            class="w-4rem lck-checked"
            v-model="device_config_number.status"
          />
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="closeDialog"
     class="p-button-outlined"
      />

      <Button
        label="Lưu"
        icon="pi pi-check"
        @click="saveWarehouse(!v$.$invalid)"
      />
    </template>
  </Dialog>
  <Dialog
    header="Tải lên file Excel"
    v-model:visible="Imp"
    :style="{ width: '40vw' }"
    :closable="true"   :modal="true"
  >
    <h3>
      <label>
        <a :href="basedomainURL + item" download>Nhấn vào đây</a> để tải xuống
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
    <template #footer
      ><Button label="Lưu" icon="pi pi-check" @click="Upload"
    /></template>
    <!-- Chức năng đang chỉnh sửa vui lòng liên hệ quản trị viên phần mềm -->
  </Dialog>
</template>
            
            <style scoped>
.sel-placeholder::placeholder {
  text-align: center;
  position: absolute;
  top: 0;
  
}
</style>