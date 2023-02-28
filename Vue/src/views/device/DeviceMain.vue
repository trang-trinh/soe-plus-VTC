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
  device_name: {
    required,
    $errors: [
      {
        $property: "device_name",
        $validator: "required",
        $message: "Tên thiết bị không được để trống!",
      },
    ],
  },
  device_code: {
    required,
    $errors: [
      {
        $property: "device_code",
        $validator: "required",
        $message: "Địa chỉ không được để trống!",
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
 
const device_main = ref({
  device_name: "",
  status: true,
  is_order: 1,
  type: true,
});
const selectedWarehouses = ref();
const submitted = ref(false);
const v$ = useVuelidate(rules, device_main);
const issaveWarehouse = ref(false);
const datalists = ref();
const toast = useToast();
const basedomainURL = fileURL;
const checkDelList = ref(false);
const options = ref({
  IsNext: true,
  sort: "device_id ASC",
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
        proc: "device_main_count",
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
      addLog({
        title: "Lỗi Console loadCount",
        controller: "SQLView.vue",
        logcontent: error.message,
        loai: 2,
      });
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
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
          proc: "device_main_list",
          par: [
            { par: "pageno", va: options.value.PageNo },
            { par: "pagesize", va: options.value.PageSize },
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
  device_main.value = {
    device_code: null,
    device_name: null,
    device_unit_id: null,
    device_type_id: null,
    price: null,
    depreciation_month: null,
    is_order: sttWarehouse.value,
    status: true,
    organization_id: store.getters.user.organization_id,
    type: true,
  };
  selectCapcha.value=null;
  issaveWarehouse.value = false;
  headerDialog.value = str;
  displayBasic.value = true;
};
const closeDialog = () => {
  device_main.value = {
    device_name: "",
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
  if (
    !device_main.value.device_unit_id ||
    !device_main.value.device_type_id ||
    !selectCapcha.value
  ) {
    return;
  }
  if (device_main.value.device_code.length > 50) {
    swal.fire({
      title: "Thông báo!",
      text: "Mã thiết bị không quá 50 ký tự!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });

  let arw = null;
  if (selectCapcha.value)
    Object.keys(selectCapcha.value).forEach((key) => {
      arw = key;
      return;
    });
  device_main.value.device_groups_id = arw;
  if (!issaveWarehouse.value) {
    axios
      .post(
        baseURL + "/api/device_main/add_device_main",
        device_main.value,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Thêm thiết bị thành công!");
          loadData(true);
          closeDialog();
          Refresh();
        } else {
          let ms = response.data.ms;
          swal.fire({
            title: "Thông báo!",
            text:
              ms.includes("device_name") == true
                ? "Tên thiết bị không quá 500 ký tự!"
                : ms,
            icon: "error",
            confirmButtonText: "OK",
          });
          loadData(true);
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
        baseURL + "/api/device_main/update_device_main",
        device_main.value,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa thiết bị thành công!");
          loadData(true);
          closeDialog();
        } else {
          let ms = response.data.ms;
          swal.fire({
            title: "Thông báo!",
            text:
              ms.includes("device_name") == true
                ? "Tên thiết bị không quá 500 ký tự!"
                : ms,
            icon: "error",
            confirmButtonText: "OK",
          });
          loadData(true);
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
  selectCapcha.value = {};
  submitted.value = false;
  device_main.value = dataPlace;
  device_main.value.organization_id = store.state.user.organization_id;

  selectCapcha.value[device_main.value.device_groups_id] = true;
  isChirlden.value = false;
  if (dataPlace.parent_id != null) {
    isChirlden.value = true;
  }
  headerDialog.value = "Sửa thiết bị";
  issaveWarehouse.value = true;
  displayBasic.value = true;
};
//Xóa bản ghi
const delWarehouse = (Warehouse) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá thiết bị này không!",
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
          .delete(baseURL + "/api/device_main/delete_device_main", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: Warehouse != null ? [Warehouse.device_id] : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err == "0") {
              swal.close();
              toast.success("Xoá thiết bị thành công!");
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
  {
    label: "Nhập Excel",
    icon: "pi pi-file-excel",
    command: (event) => {
      ImportExcel(event);
    },
  },
]);
const toggleExport = (event) => {
  menuButs.value.toggle(event);
};
const exportData = (method) => {
  if (filterPhanloai.value == undefined) {
    options.value.filter_Org = 1;
  } else if (filterPhanloai.value == 0) {
    options.value.filter_Org = 3; //list hệ thống
  } else options.value.filter_Org = 2; // list đơn vị
  filterTrangthai.value =
    filterTrangthai.value != null
      ? filterTrangthai.value == 1
        ? true
        : false
      : null;
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
        excelname: "DANH SÁCH CHỨC VỤ",
        proc: "device_main_listexport",
        par: [
          { par: "search", va: options.value.SearchText },
          { par: "status", va: filterTrangthai.value },
          { par: "user_id", va: store.state.user.user_id },
          { par: "s_org", va: filterPhanloai.value },
          { par: "filter_Org", va: options.value.filter_Org },
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
          title: "Thông báo",
          text: response.data.ms,
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    })
    .catch((error) => {
      if (error.status === 401) {
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
//Sort
const onSort = (event) => {
  first.value = 0;
  options.value.PageNo = 0;

  if (event.sortField == null) {
    isDynamicSQL.value = false;
    loadData(true);
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
        key: key == "device_name" ? "device_name" : key,
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
    id: "device_id",
    sqlS: filterTrangthai.value != null ? filterTrangthai.value : null,
    sqlO: options.value.sort,
    Search: options.value.SearchText,
    PageNo: options.value.PageNo,
    PageSize: options.value.PageSize,
    sqlF: null,
    next: true,
    fieldSQLS: filterSQL.value,
  };
  options.value.loading = true;
  axios
    .post(baseURL + "/api/SQL/Filter_device_main", data, config)
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

      addLog({
        title: "Lỗi Console loadData",
        controller: "SQLView.vue",
        logcontent: error.message,
        loai: 2,
      });
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
    IntID: value.device_id,
    TextID: value.device_id + "",
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
      .put(baseURL + "/api/device_main/update_s_device_main", data, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa thiết bị thành công!");
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
          listId.push(item.device_id);
        });
        axios
          .delete(baseURL + "/api/device_main/delete_device_main", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: listId != null ? listId : 1,
          })
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
  filterTrangthai.value = null;
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
  filters.value = {
    device_name: {
      operator: FilterOperator.AND,
      constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
    },
    address: {
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
  { name: "Hiển thị", code: "1" },
  { name: "Không hiển thị", code: "0" },
]);
const phanLoai = ref([
  { name: "Hệ thống", code: 0 },
  { name: "Đơn vị", code: 1 },
]);
const reFilter = () => {
  filterSQL.value = [];
  checkFilter.value = false;
  filterTrangthai.value = null;
  filterTypeDevice.value = null;
  isDynamicSQL.value = true;
  loadData(true);
};
const filter = () => {
  checkFilter.value = true;
  filterSQL.value = [];
 
  if (filterTrangthai.value) {
    let filterS = {
      filterconstraints: [
        { value: filterTrangthai.value, matchMode: "equals" },
      ],
      filteroperator: "and",
      key: "status",
    };
    filterSQL.value.push(filterS);
  }
  if (filterTypeDevice.value) {
    let filterS = {
      filterconstraints: [
        { value: filterTypeDevice.value, matchMode: "equals" },
      ],
      filteroperator: "and",
      key: "device_type_id",
    };
    filterSQL.value.push(filterS);
  }
  
  // isDynamicSQL.value = true;
  loadDataSQL();
};
const filters = ref({
  device_code: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
  device_name: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
  address: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
  status: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.EQUALS }],
  },
});
const Imp = ref(false);
const ImportExcel = () => {
  Imp.value = true;
};
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
const style = ref({
  "background-color": "#2196F3 !important",
  color: "#fff !important",
  " border": "1px solid #5ca7e3 !important",
});
const listDropdownUserCheck = ref();
const listUsers = ref([]);
const listType = ref();
const loadDeviceType = () => {
  listType.value = [];
  axios
    .post(
      baseURL + "/api/device_card/getData",
        {
          str: encr(
            JSON.stringify({
        proc: "device_type_list",
        par: [
          { par: "pageno", va: 0 },
          { par: "pagesize", va: 100000 },
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
      data.forEach((element, i) => {
        listType.value.push({
          name: element.device_type_name,
          code: element.device_type_id,
        });
      });
    })
    .catch((error) => {
      console.log("err", error);
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
};
const filterTypeDevice = ref();
const listUnit = ref();
const loadDeviceUnit = () => {
  listUnit.value = [];
  axios
    .post(
      baseURL + "/api/device_card/getData",
        {
          str: encr(
            JSON.stringify({
        proc: "device_unit_list",
        par: [
          { par: "pageno", va: 0 },
          { par: "pagesize", va: 100000 },
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
      data.forEach((element, i) => {
        listUnit.value.push({
          name: element.device_unit_name,
          code: element.device_unit_id,
        });
      });
    })
    .catch((error) => {
      console.log("err", error);
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
};
const renderTree = (data, id, name, title) => {
  let arrChils = [];
  let arrtreeChils = [];
  data
    .filter((x) => x.parent_id == null)
    .forEach((m, i) => {
      m.STT = i + 1;
      let om = { key: m[id], data: m };
      const rechildren = (mm, pid) => {
        let dts = data.filter((x) => x.parent_id == pid);
        if (dts.length > 0) {
          if (!mm.children) mm.children = [];
          dts.forEach((em, j) => {
            em.STT = j + 1;
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
const treeDeviceGroupsModules = ref();

const selectCapcha = ref();
const loadDeviceGroups = () => {
  axios
    .post(
      baseURL + "/api/device_card/getData",
        {
          str: encr(
            JSON.stringify({
        proc: "device_groups_list",
        par: [
          { par: "search", va: null },
          { par: "pageno", va: 0 },
          { par: "pagesize", va: 100000 },
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
      let obj = renderTree(data, "device_groups_id", "groups_name", "cấp cha");
      treeDeviceGroupsModules.value = obj.arrtreeChils;
    })
    .catch((error) => {
      console.log("err", error);
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
};

onMounted(() => {  if (!checkURL(window.location.pathname, store.getters.listModule)) {
     //router.back();
  }
  loadDeviceGroups();
  loadDeviceType();
  loadDeviceUnit();
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
  <div class="main-layout true flex-grow-1 p-2 pb-0 pr-0">
    <DataTable
      v-model:first="first"
      :show-gridlines="true"
      :value="datalists"
      :paginator="true"
      removableSort
      :rows="options.PageSize"
      paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
      :rowsPerPageOptions="[20, 30, 50, 100, 200]"
      :scrollable="true"
      scrollHeight="flex"
      :loading="options.loading"
      dataKey="device_id"
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
          <i class="pi pi-shopping-bag"></i> Danh sách thiết bị ({{
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
                (filterTrangthai != null || filterTypeDevice != null) &&
                checkFilter
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
                  <div class="col-4">Trạng thái</div>
                  <div class="col-8">
                    <Dropdown
                      class="col-12 p-0 m-0"
                      v-model="filterTrangthai"
                      :options="trangThai"
                      optionLabel="name"
                      optionValue="code"
                      placeholder="Trạng thái"
                    />
                  </div>
                </div>
                <div class="flex field col-12 p-0 align-items-center">
                  <div class="col-4">Loại thiết bị</div>
                  <div class="col-8">
                    <Dropdown
                      v-model="filterTypeDevice"
                      :options="listType"
                      optionLabel="name"
                      optionValue="code"
                      placeholder="----- Loại thiết bị -----"
                      spellcheck="false"
                      class="w-full"
                      panelClass="d-design-dropdown"
                    >
                      <template #option="slotProps">
                        <div class="p-dropdown-car-option text-2line">
                          {{ slotProps.option.name }}
                        </div>
                      </template>
                    </Dropdown>
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
                      <Button @click="filter()" label="Lọc"></Button>
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
            <!-- 
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
        field="device_code"
        header="Mã thiết bị"
        :sortable="true"
        headerStyle="height:50px place-content: center;place-items: center;height: 50px; max-width:200px"
        bodyStyle=" max-width:200px"
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
        field="device_name"
        header="Tên thiết bị"
        :sortable="true"
        headerStyle="height:50px place-content: center;place-items: center;height: 50px"
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
        field="device_type_name"
        header="Loại thiết bị"
        class="align-items-center justify-content-center text-center"
        headerStyle="height:50px place-content: center;place-items: center;height: 50px; max-width:150px"
        bodyStyle="max-width:150px"
      ></Column>
      <Column
        field="device_unit_name"
        header="Đơn vị tính"
        class="align-items-center justify-content-center text-center"
        headerStyle="height:50px place-content: center;place-items: center;height: 50px; max-width:150px"
        bodyStyle=" max-width:150px"
      >
      </Column>
      <Column
        field="price"
        header="Đơn giá"
        class="align-items-center justify-content-center text-center"
        headerStyle="height:50px place-content: center;place-items: center;height: 50px; max-width:150px"
        bodyStyle="max-width:150px"
      ></Column>
      <Column
        field="status"
        header="Trạng thái"
        headerStyle="text-align:center;jusst;max-width:100px;height:50px"
        bodyStyle="text-align:center;max-width:100px"
        class="align-items-center justify-content-center text-center"
      >
        <template #body="data">
          <Checkbox
            :disabled="
              !(
                store.state.user.is_super == true ||
                store.state.user.user_id == data.data.created_by ||
                (store.state.user.role_id == 'admin' &&
                  store.state.user.organization_id == data.data.organization_id)
              )
            "
            :binary="data.data.status"
            v-model="data.data.status"
            @click="onCheckBox(data.data)"
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
    :style="{ width: '40vw' }" :modal="true"
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12 flex">
          <div class="col-12 p-0 flex align-items-center">
            <label class="col-3 text-left p-0"
              >Mã thiết bị <span class="redsao">(*)</span></label
            >
            <InputText
              v-model="device_main.device_code"
              spellcheck="false"
              class="col-9 ip36 px-2"
              :class="{ 'p-invalid': v$.device_code.$invalid && submitted }"
            />
          </div>
        </div>
        <div
          v-if="
            (v$.device_code.$invalid && submitted) ||
            v$.device_code.$pending.$response
          "
          style="display: flex"
          class="field col-12 md:col-12"
        >
          <div class="col-3 text-left"></div>
          <small class="col-9 p-error p-0">
            <span class="col-12 p-0">{{
              v$.device_code.required.$message
                .replace("Value", "Mã thiết bị")
                .replace("is required", "không được để trống")
            }}</span>
          </small>
        </div>

        <div class="col-12 field flex align-items-center">
          <label class="col-3 text-left p-0"
            >Tên thiết bị <span class="redsao">(*)</span></label
          >

          <Textarea
            :autoResize="true"
            rows="2"
            cols="30"
            v-model="device_main.device_name"
            class="w-full h-full px-2"
            :class="{ 'p-invalid': v$.device_name.$invalid && submitted }"
          />

          <!-- <InputText
                v-model="device_main.device_name"
                spellcheck="false"
                class="col-9 ip36 px-2"
                :class="{ 'p-invalid': v$.device_name.$invalid && submitted }"
              /> -->
        </div>
        <div
          v-if="
            (v$.device_name.$invalid && submitted) ||
            v$.device_name.$pending.$response
          "
          style="display: flex"
          class="field col-12 md:col-12"
        >
          <div class="col-3 text-left"></div>
          <small class="col-9 p-error p-0">
            <span class="col-12 p-0">{{
              v$.device_name.required.$message
                .replace("Value", "Tên thiết bị")
                .replace("is required", "không được để trống")
            }}</span>
          </small>
        </div>

        <div class="field col-12 md:col-12 flex">
          <div class="col-12 p-0 flex align-items-center">
            <label class="col-3 text-left p-0"
              >Loại thiết bị <span class="redsao">(*)</span></label
            >
 <Dropdown


   v-model="device_main.device_type_id"
              :options="listType"
              optionLabel="name"
              optionValue="code"
              placeholder="--------------- Loại thiết bị --------------"
              spellcheck="false"


              
              :filter="true"
              panelClass="d-design-dropdown"
             
             class="col-9 ip36 p-0 sel-placeholder"
              ref="isRefAprroved"
             :class="{ 'p-invalid': !device_main.device_type_id && submitted }"
                    
            
            >
            
              <template #option="slotProps">
                <div class="p-dropdown-car-option format-center">
                  <div>
                    <div class="font-bold p-2 px-0">
                      {{ slotProps.option.name }}
                    </div>

                  
                  </div>
                </div>
              </template>
            </Dropdown>

            <!-- <Dropdown
              v-model="device_main.device_type_id"
              :options="listType"
              optionLabel="name"
              optionValue="code"
              placeholder="--------------- Loại thiết bị --------------"
              spellcheck="false"
              class="col-9 ip36 p-0 sel-placeholder"
              panelClass="d-design-dropdown"
              :class="{ 'p-invalid': !device_main.device_type_id && submitted }"
            >
              <template #option="slotProps">
                <div class="p-dropdown-car-option text-2line">
                  {{ slotProps.option.name }}
                </div>
              </template>
            </Dropdown> -->
          </div>
        </div>
        <div
          v-if="!device_main.device_type_id && submitted"
          style="display: flex"
          class="field col-12 md:col-12"
        >
          <div class="col-3 text-left"></div>
          <small class="col-9 p-error p-0">
            <span class="col-12 p-0">Loại thiết bị không được để trống</span>
          </small>
        </div>
        <div class="flex field col-12 align-items-center">
          <div class="col-3 p-0">
            Nhóm thiết bị <span class="redsao">(*)</span>
          </div>

          <TreeSelect
            class="col-9 d-tree-input"
            v-model="selectCapcha"
            :options="treeDeviceGroupsModules"
            :showClear="true"
            placeholder="------ Chọn nhóm thiết bị ------"
            optionLabel="data.groups_name"
            optionValue="data.groups_id"
            panelClass="d-design-dropdown"
            :class="{ 'p-invalid': !selectCapcha && submitted }"
          ></TreeSelect>
        </div>
        <div
          v-if="!selectCapcha && submitted"
          style="display: flex"
          class="field col-12 md:col-12"
        >
          <div class="col-3 text-left"></div>
          <small class="col-9 p-error p-0">
            <span class="col-12 p-0">Nhóm thiết bị không được để trống!</span>
          </small>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-6 p-0 flex align-items-center">
            <label class="col-6 text-left p-0">
              Đơn vị tính <span class="redsao">(*)</span></label
            >
            <Dropdown
              v-model="device_main.device_unit_id"
              :options="listUnit"
              optionLabel="name"
              optionValue="code"
              placeholder="--- Đơn vị tính ---"
              spellcheck="false"
              class="col-6 ip36 p-0 sel-placeholder"
              :class="{ 'p-invalid': !device_main.device_unit_id && submitted }"
            >
            </Dropdown>
          </div>
          <div class="col-6 p-0 flex align-items-center pl-2">
            <label class="col-4 text-left p-0">Đơn giá</label>
            <InputNumber
              v-model="device_main.price"
              spellcheck="false"
              class="col-8 ip36 p-0"
              mode="currency"
              currency="VND"
            />
          </div>
        </div>
        <div
          v-if="!device_main.device_unit_id && submitted"
          style="display: flex"
          class="field col-12 md:col-12"
        >
          <div class="col-3 text-left"></div>
          <small class="col-9 p-error p-0">
            <span class="col-12 p-0">Đơn vị tính không được để trống</span>
          </small>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-6 p-0 flex align-items-center">
            <label class="col-6 text-left p-0"> Tháng khấu hao</label>
            <InputNumber
              v-model="device_main.depreciation_month"
              spellcheck="false"
              class="col-6 ip36 p-0"
              suffix=" Tháng"
            />
          </div>
          <div class="col-6 p-0 flex align-items-center pl-2">
            <label class="col-4 text-left p-0">STT</label>
            <InputNumber
              v-model="device_main.is_order"
              spellcheck="false"
              class="col-8 ip36 p-0"
            />
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-6 p-0 flex align-items-center">
            <label
              style="vertical-align: text-bottom"
              class="col-6 text-left p-0"
              >Trạng thái
            </label>
            <InputSwitch
              class="w-4rem lck-checked"
              v-model="device_main.status"
            />
          </div>
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
    :closable="true" :modal="true"
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
.text-2line {
  text-overflow: ellipsis;
  overflow: hidden;
  column-gap: initial;
  -webkit-line-clamp: 1;
  display: -webkit-box;
  -webkit-box-orient: vertical;
}
</style>

<style lang="scss" scoped>
// ::v-deep(.p-dropdown-panel .p-component) {
//   .p-panel-header {
//     padding: 16px 8px;
//     background-color: #fff !important;
//   }
//   .p-panel-content {
//     padding: 0;
//   }
// }
// ::v-deep(.p-grid) {
//   div {
//     border-bottom: unset !important;
//     border-width: 1px 0px 0px 0px !important;
//   }
// }
</style>
