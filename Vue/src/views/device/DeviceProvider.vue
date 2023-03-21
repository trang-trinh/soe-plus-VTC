<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
 
import { useVuelidate } from "@vuelidate/core";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import { encr, checkURL } from "../../util/function.js";
//Khai báo
const router = inject("router");
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const item = "/Portals/Mau Excel/Mẫu Excel Chức vụ.xlsx";
const emitter = inject("emitter");
const isDynamicSQL = ref(false);

const cryoptojs = inject("cryptojs");
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const first = ref(0);

const rules = {
  provider_name: {
    required,
    $errors: [
      {
        $property: "provider_name",
        $validator: "required",
        $message: "Tên nhà cung cấp không được để trống!",
      },
    ],
  },
  address: {
    required,
    $errors: [
      {
        $property: "address",
        $validator: "required",
        $message: "Địa chỉ không được để trống!",
      },
    ],
  },
  tel: {
    required,
    $errors: [
      {
        $property: "tel",
        $validator: "required",
        $message: "Số điện thoại không được để trống!",
      },
    ],
  },
  full_name: {
    required,
    $errors: [
      {
        $property: "full_name",
        $validator: "required",
        $message: "Họ và tên không được để trống!",
      },
    ],
  },
  phone_number: {
    required,
    $errors: [
      {
        $property: "phone_number",
        $validator: "required",
        $message: "Số điện thoại không được để trống!",
      },
    ],
  },
};
const rulesUs = {
  full_name: {
    required,
    $errors: [
      {
        $property: "full_name",
        $validator: "required",
        $message: "Họ và tên không được để trống!",
      },
    ],
  },
  phone_number: {
    required,
    $errors: [
      {
        $property: "phone_number",
        $validator: "required",
        $message: "Số điện thoại không được để trống!",
      },
    ],
  },
  contact_address: {
    required,
    $errors: [
      {
        $property: "contact_address",
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
const listDropdownUser = ref([]);
const device_provider = ref({
  provider_name: "",
  status: true,
  is_order: 1,
  type: true,
});
const selectedWarehouses = ref();
const submitted = ref(false);
const v$ = useVuelidate(rules, device_provider);
const vUs$ = useVuelidate(rulesUs, device_provider);
const issaveWarehouse = ref(false);
const datalists = ref();
const toast = useToast();
const basedomainURL = fileURL;
const checkDelList = ref(false);
const options = ref({
  IsNext: true,
  sort: "provider_id DESC",
  SearchText: null,
  PageNo: 0,
  PageSize: 20,
  loading: true,
  totalRecords: null,
});
const listVocative = ref([
  {
    name: "Ông",
    code: "Ông",
  },
  { name: "Bà", code: "Bà" },
]);
//Thêm log
const addLog = (log) => {
  axios.post(baseURL + "/api/Proc/AddLog", log, config);
};
const listTypeP = ref([
  { name: "Tổ chức", code: true },
  { name: "Cá nhân", code: false },
]);

const checkNumber = (valie, prop) => {
  
  let regex = /[0-9]/;
  let checkKey = regex.test(valie.key);
  if (!checkKey) {
    device_provider.value[prop] = device_provider.value[prop].replaceAll(
      valie.key,
      ""
    );
  }
};
//Lấy số bản ghi
const loadCount = () => {
  axios
    .post(
      baseURL + "/api/device_card/getData",
        {
          str: encr(
            JSON.stringify({
        proc: "device_provider_count",
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
const liU = ref([]);
const showManage = (value) => {
  liU.value = [];
  value.forEach((element) => {
    listUsers.value
      .filter((x) => x.user_id == element)
      .forEach((ele) => {
        liU.value.push({ data: ele, active: false });
        return;
      });
  });
  showSidebarUser.value = true;
};
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
          proc: "device_provider_list",
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
      
        options.value.loading = false;

     
      });
  }
  if (store.state.user.is_super == 1) {
    loadDonvi();
  }
};
const loadDonvi = () => {
  axios
    .post(
      baseURL + "/api/device_card/getData",
        {
          str: encr(
            JSON.stringify({
        proc: "sys_org_list",
      }),
            SecretKey,
            cryoptojs
          ).toString(),
        },config
    )
    .then((response) => {
      treedonvis.value = [];
      let data = JSON.parse(response.data.data)[0];
      let sys = { name: "Hệ thống", code: 0 };
      treedonvis.value.push(sys);

      if (data.length > 0) {
        data.forEach((x) => {
          x = { name: x.organization_name, code: x.organization_id };
          treedonvis.value.push(x);
        });
      } else {
        treedonvis.value = [];
      }
    })
    .catch((error) => {
      if (error && error.status === 401) {
        swal.fire({
          title: "Thông báo",
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    });
};
const treedonvis = ref();
//Hiển thị dialog
const headerDialog = ref();
const displayBasic = ref(false);
const onChangeType = () => {
  device_provider.value.phone_number = null;
};
const openBasic = (str) => {
  submitted.value = false;
  device_provider.value = {
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
  device_provider.value = {
    provider_name: "",
    is_order: 1,
    status: true,
  };
  displayBasic.value = false;
  loadData(true);
};

//Thêm bản ghi
const saveWarehouse = (isFormValid, isFormValid1) => {
  submitted.value = true;
  if (!isFormValid && device_provider.value.type) {
    return;
  }
  if (!isFormValid1 && !device_provider.value.type) {
    return;
  }
  if (device_provider.value.address)
    if (device_provider.value.address.length > 250) {
      swal.fire({
        title: "Thông báo",
        text: "Địa chỉ không được lớn hơn 250 kí tự!",
        icon: "error",
        confirmButtonText: "OK",
      });
      return;
    }

  if (device_provider.value.contact_address)
    if (device_provider.value.contact_address.length > 250) {
      swal.fire({
        title: "Thông báo",
        text: "Địa chỉ không được lớn hơn 250 kí tự!",
        icon: "error",
        confirmButtonText: "OK",
      });
      return;
    }
  if (device_provider.value.full_name)
    if (device_provider.value.full_name.length > 50) {
      swal.fire({
        title: "Thông báo",
        text: "Họ và tên không được lớn hơn 50 kí tự!",
        icon: "error",
        confirmButtonText: "OK",
      });
      return;
    }

  if (device_provider.value.phone_number)
    if (device_provider.value.phone_number.length > 50) {
      swal.fire({
        title: "Thông báo",
        text: "Số điện thoại không được lớn hơn 50 kí tự!",
        icon: "error",
        confirmButtonText: "OK",
      });
      return;
    }
  if (device_provider.value.tel)
    if (device_provider.value.tel.length > 50) {
      swal.fire({
        title: "Thông báo",
        text: "Số điện thoại không được lớn hơn 50 kí tự!",
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
  if (device_provider.value.phone_number)
    device_provider.value.phone_number =
      device_provider.value.phone_number.toString();
  if (device_provider.value.tel)
    device_provider.value.tel = device_provider.value.tel.toString();
  if (!issaveWarehouse.value) {
    axios
      .post(
        baseURL + "/api/device_provider/add_device_provider",
        device_provider.value,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Thêm nhà cung cấp thành công!");
          loadData(true);
          closeDialog();
          Refresh();
        } else {
          let ms = response.data.ms;
          swal.fire({
            title: "Thông báo!",
            text:
              ms.includes("provider_name") == true
                ? "Tên nhà cung cấp không quá 500 ký tự!"
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
        baseURL + "/api/device_provider/update_device_provider",
        device_provider.value,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa nhà cung cấp thành công!");
          loadData(true);
          closeDialog();
        } else {
          let ms = response.data.ms;
          swal.fire({
            title: "Thông báo!",
            text:
              ms.includes("provider_name") == true
                ? "Tên nhà cung cấp không quá 500 ký tự!"
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
  submitted.value = false;
  device_provider.value = dataPlace;
  device_provider.value.organization_id =
    store.state.user.is_super == true ? 0 : store.state.user.organization_id;
  isChirlden.value = false;
  if (dataPlace.parent_id != null) {
    isChirlden.value = true;
  }
  headerDialog.value = "Sửa nhà cung cấp";
  issaveWarehouse.value = true;
  displayBasic.value = true;
};
//Xóa bản ghi
const delWarehouse = (Warehouse) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá nhà cung cấp này không!",
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
          .delete(baseURL + "/api/device_provider/delete_device_provider", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: Warehouse != null ? [Warehouse.provider_id] : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err == "0") {
              swal.close();
              toast.success("Xoá nhà cung cấp thành công!");
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
        proc: "device_provider_listexport",
        par: [
          { par: "search", va: options.value.SearchText },
          { par: "status", va: filterTrangthai.value },
          { par: "user_id", va: store.state.user.user_id },
          { par: "s_org", va: filterPhanloai.value },
          { par: "filter_Org", va: options.value.filter_Org },
        ],}
     ,config
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
        key: key == "provider_name" ? "provider_name" : key,
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
    id: "provider_id",
    sqlS: filterTrangthai.value != null ? filterTrangthai.value : null,
    sqlO: options.value.sort,
    Search: options.value.SearchText,
    PageNo: options.value.PageNo,
    PageSize: options.value.PageSize,
    next: true,
    sqlF: null,
    fieldSQLS: filterSQL.value,
  };
  options.value.loading = true;
  axios
    .post(baseURL + "/api/SQL/Filter_device_provider", data, config)
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
    IntID: value.provider_id,
    TextID: value.provider_id + "",
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
        baseURL + "/api/device_provider/update_s_device_provider",
        data,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa nhà cung cấp thành công!");
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
          listId.push(item.provider_id);
        });
        axios
          .delete(baseURL + "/api/device_provider/delete_device_provider", {
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
  filterSQL.value = [];
  styleObj.value = "";
  datalists.value = [];
  checkFilter.value = false;
  filterTrangthai.value = null;
  filterType.value = null;
  filters.value = {
    provider_name: {
      operator: FilterOperator.AND,
      constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
    },
    address: {
      operator: FilterOperator.AND,
      constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
    },
  };
  loadDataSQL();
};
const op = ref();
const filterTrangthai = ref();
const filterType = ref();
const checkFilter = ref(false);
const toggle = (event) => {
  op.value.toggle(event);
};
const trangThai = ref([
  { name: "Hiển thị", code: 1 },
  { name: "Không hiển thị", code: 0 },
]);
const phanLoai = ref([
  { name: "Hệ thống", code: 0 },
  { name: "Đơn vị", code: 1 },
]);
const reFilter = () => {
  filterTrangthai.value = null;
  filterType.value = null;
  isDynamicSQL.value = false;
  checkFilter.value = false;
  loadData(true);
};

const filter = () => {
  checkFilter.value = true;
  filterSQL.value = [];
  if (filterTrangthai.value != null) {
    let filterS = {
      filterconstraints: [
        { value: filterTrangthai.value, matchMode: "equals" },
      ],
      filteroperator: "and",
      key: "status",
    };
    filterSQL.value.push(filterS);
  }
  if (filterType.value != null) {
    let filterS = {
      filterconstraints: [{ value: filterType.value, matchMode: "equals" }],
      filteroperator: "and",
      key: "type",
    };
    filterSQL.value.push(filterS);
  }
  isDynamicSQL.value = true;
  loadData(true);
};
const filters = ref({
  provider_name: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
  address: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
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
const loadUser = () => {
  listUsers.value = [];
  listDropdownUser.value = [];
  axios
    .post(
      baseURL + "/api/device_card/getData",
        {
          str: encr(
            JSON.stringify({
        proc: "sys_users_list_dd",
        par: [
          { par: "search", va: null },
          { par: "user_id", va: store.getters.user.user_id },
          { par: "role_id", va: null },
          { par: "organization_id", va: store.getters.user.organization_id },
          { par: "department_id", va: null },
          { par: "position_id", va: null },
          { par: "pageno", va: 1 },
          { par: "pagesize", va: 10000 },
          { par: "isadmin", va: null },
          { par: "status", va: null },
          { par: "start_date", va: null },
          { par: "end_date", va: null },
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
        listDropdownUser.value.push({
          name: element.full_name,
          code: element.user_id,
          avatar: element.avatar,
        });

        listUsers.value.push({ data: element, active: false });
      });
      listUsers.value = data;

      listDropdownUserCheck.value = listDropdownUser.value;
    
    })
    .catch((error) => {
     

      options.value.loading = false;
 
    });
};
onMounted(() => {  if (!checkURL(window.location.pathname, store.getters.listModule)) {
     //router.back();
  }
  loadUser();
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
      :rows="options.PageSize"
      paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
      :rowsPerPageOptions="[20, 30, 50, 100, 200]"
      :scrollable="true"
      scrollHeight="flex"
      :loading="options.loading"
      removableSort
      dataKey="provider_id"
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
          <i class="pi pi-shopping-bag"></i> Danh sách nhà cung cấp ({{
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
                (filterTrangthai != null || filterType != null) && checkFilter
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
                  <div class="col-4">Phân loại:</div>
                  <div class="col-8">
                    <Dropdown
                      class="col-12 p-0 m-0"
                      v-model="filterType"
                      :options="listTypeP"
                      optionLabel="name"
                      optionValue="code"
                      placeholder="Phân loại"
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
        field="provider_name"
        header="Tên nhà cung cấp"
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
        field="address"
        header="Địa chỉ"
        class="align-items-center justify-content-center text-center"
        headerStyle="height:50px place-content: center;place-items: center;height: 50px; max-width:300px"
        bodyStyle=" max-width:300px"
      ></Column>
      <Column
        field="full_name"
        header="Người liên hệ"
        class="align-items-center justify-content-center text-center"
        headerStyle="height:50px place-content: center;place-items: center;height: 50px; max-width:250px"
        bodyStyle=" max-width:250px"
      >
      </Column>
      <Column
        field="phone_number"
        header="Số điện thoại"
        class="align-items-center justify-content-center text-center"
        headerStyle="height:50px place-content: center;place-items: center;height: 50px; max-width:200px"
        bodyStyle=" max-width:200px"
      ></Column>
      <Column
        field="status"
        header="Trạng thái"
        headerStyle="text-align:center;jusst;max-width:120px;height:50px"
        bodyStyle="text-align:center;max-width:120px"
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
             store.getters.user.is_admin || store.state.user.is_super == true ||
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
    v-model:visible="displayBasic" :modal="true"
    :style="{ width: device_provider.type ? '55vw' : '45vw' }"
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12 flex format-center">
          <div class="col-6 p-0">
            <SelectButton
              v-model="device_provider.type"
              :options="listTypeP"
              optionLabel="name"
              optionValue="code"
              @click="onChangeType()"
            />
          </div>
        </div>
        <div v-if="device_provider.type" class="col-12 md:col-12 p-0">
          <div class="field col-12 md:col-12 text-lg text-primary font-bold">
            Thông tin tổ chức
          </div>

          <div class="field col-12 md:col-12 flex">
            <div class="col-12 p-0 flex align-items-center">
              <label class="col-2 text-left p-0"
                >Tên nhà cung cấp <span class="redsao">(*)</span></label
              >
              <InputText
                v-model="device_provider.provider_name"
                spellcheck="false"
                class="col-10 ip36 px-2"
                :class="{ 'p-invalid': v$.provider_name.$invalid && submitted }"
              />
            </div>
          </div>
          <div
            v-if="
              (v$.provider_name.$invalid && submitted) ||
              v$.provider_name.$pending.$response
            "
            style="display: flex"
            class="field col-12 md:col-12"
          >
            <div class="col-2 text-left"></div>
            <small class="col-10 p-error p-0">
              <span class="col-12 p-0">{{
                v$.provider_name.required.$message
                  .replace("Value", "Tên nhà cung cấp")
                  .replace("is required", "không được để trống")
              }}</span>
            </small>
          </div>
          <div class="field col-12 md:col-12 flex">
            <div class="col-6 p-0 flex align-items-center">
              <label class="col-4 text-left p-0"
                >Địa chỉ <span class="redsao">(*)</span></label
              >
              <InputText
                v-model="device_provider.address"
                spellcheck="false"
                class="col-8 ip36 px-2"
                :class="{ 'p-invalid': v$.address.$invalid && submitted }"
              />
            </div>
            <div class="col-6 p-0 flex align-items-center pl-2">
              <label class="col-4 text-left p-0"
                >Số điện thoại <span class="redsao">(*)</span></label
              >
              <InputText
                v-model="device_provider.tel"
                spellcheck="false"
                class="col-8 ip36"
                :class="{ 'p-invalid': v$.tel.$invalid && submitted }"
                @keyup="checkNumber($event, 'tel')"
              />
            </div>
          </div>
          <div class="col-12 flex">
            <div
              v-if="
                (v$.address.$invalid && submitted) ||
                v$.address.$pending.$response
              "
              style="display: flex"
              class="field col-6 md:col-6"
            >
              <div class="col-4 text-left"></div>
              <small class="col-8 p-error p-0">
                <span class="col-12 p-0">{{
                  v$.address.required.$message
                    .replace("Value", "Địa chỉ tổ chức")
                    .replace("is required", "không được để trống")
                }}</span>
              </small>
            </div>
            <div class="field col-6 md:col-6" v-else></div>
            <div
              v-if="(v$.tel.$invalid && submitted) || v$.tel.$pending.$response"
              style="display: flex"
              class="field col-6 md:col-6"
            >
              <div class="col-4 text-left"></div>
              <small class="col-8 p-error p-0">
                <span class="col-12 p-0">{{
                  v$.tel.required.$message
                    .replace("Value", "Điện thoại tổ chức")
                    .replace("is required", "không được để trống")
                }}</span>
              </small>
            </div>
          </div>
          <div class="field col-12 md:col-12 flex">
            <div class="col-6 p-0 flex align-items-center">
              <label class="col-4 text-left p-0">Email</label>
              <InputText
                v-model="device_provider.email"
                spellcheck="false"
                class="col-8 ip36 px-2"
              />
            </div>
            <div class="col-6 p-0 flex align-items-center pl-2">
              <label class="col-4 text-left p-0">Fax</label>
              <InputText
                v-model="device_provider.fax"
                spellcheck="false"
                class="col-8 ip36 px-2"
              />
            </div>
          </div>
          <div class="field col-12 md:col-12 flex">
            <div class="col-6 p-0 flex align-items-center">
              <label class="col-4 text-left p-0">Website </label>
              <InputText
                v-model="device_provider.website"
                spellcheck="false"
                class="col-8 ip36 px-2"
              />
            </div>
            <div class="col-6 p-0 flex align-items-center pl-2">
              <label class="col-4 text-left p-0">Mã số thuế</label>
              <InputText
                v-model="device_provider.tax_code"
                spellcheck="false"
                class="col-8 ip36 px-2"
              />
            </div>
          </div>
          <div class="field col-12 md:col-12 flex">
            <div class="col-6 p-0 flex align-items-center">
              <label class="col-4 text-left p-0">Số tài khoản </label>
              <InputText
                v-model="device_provider.bank_account_number"
                spellcheck="false"
                class="col-8 ip36 px-2"
              />
            </div>
            <div class="col-6 p-0 flex align-items-center pl-2">
              <label class="col-4 text-left p-0">Ngân hàng</label>
              <InputText
                v-model="device_provider.bank_name"
                spellcheck="false"
                class="col-8 ip36 px-2"
              />
            </div>
          </div>
          <div class="field col-12 md:col-12 text-lg text-primary font-bold">
            Thông tin người liên hệ
          </div>

          <div class="field col-12 md:col-12 flex">
            <div class="col-6 p-0 flex align-items-center">
              <label class="col-4 text-left p-0">Xưng hô</label>

              <Dropdown
                v-model="device_provider.vocative"
                :options="listVocative"
                optionLabel="name"
                optionValue="code"
                placeholder="-------- Chọn xưng hô --------"
                spellcheck="false"
                class="col-8 ip36 p-0 sel-placeholder"
              >
              </Dropdown>
            </div>
            <div class="col-6 p-0 pl-2">
              <div class="col-12 p-0 flex align-items-center">
                <label class="col-4 text-left p-0"
                  >Họ và tên <span class="redsao">(*)</span></label
                >
                <InputText
                  v-model="device_provider.full_name"
                  spellcheck="false"
                  class="col-8 ip36 px-2"
                  :class="{
                    'p-invalid': v$.full_name.$invalid && submitted,
                  }"
                />
              </div>
            </div>
          </div>
          <div
            v-if="
              (v$.full_name.$invalid && submitted) ||
              v$.full_name.$pending.$response
            "
            style="display: flex"
            class="col-12 p-0 field md:col-12"
          >
            <div class="col-6 p-0"></div>
            <div class="col-6 p-0 flex">
              <div class="col-4 text-left"></div>
              <small class="col-8 p-error p-0">
                <span class="col-12 p-0">{{
                  v$.provider_name.required.$message
                    .replace("Value", "Họ tên người liên hệ")
                    .replace("is required", "không được để trống")
                }}</span>
              </small>
            </div>
          </div>
          <div class="field col-12 md:col-12 flex">
            <div class="col-6 p-0 flex align-items-center">
              <label class="col-4 text-left p-0">Chức danh</label>
              <InputText
                v-model="device_provider.position"
                spellcheck="false"
                class="col-8 ip36 px-2"
              />
            </div>
            <div class="col-6 p-0 flex align-items-center pl-2">
              <label class="col-4 text-left p-0">Địa chỉ</label>
              <InputText
                v-model="device_provider.contact_address"
                spellcheck="false"
                class="col-8 ip36 px-2"
              />
            </div>
          </div>
          <div class="field col-12 md:col-12 flex">
            <div class="col-6 p-0 flex align-items-center">
              <label class="col-4 text-left p-0"
                >Số điện thoại <span class="redsao">(*)</span></label
              >
              <InputText
                v-model="device_provider.phone_number"
                spellcheck="false"
                class="col-8 ip36"
                :useGrouping="false"
                :class="{
                  'p-invalid': v$.phone_number.$invalid && submitted,
                }"
                @keyup="checkNumber($event, 'phone_number')"
              />
            </div>
            <div class="col-6 p-0 flex align-items-center pl-2">
              <label class="col-4 text-left p-0">Email</label>
              <InputText
                v-model="device_provider.contact_email"
                spellcheck="false"
                class="col-8 ip36 px-2"
              />
            </div>
          </div>
          <div
            v-if="
              (v$.phone_number.$invalid && submitted) ||
              v$.phone_number.$pending.$response
            "
            style="display: flex"
            class="col-12 p-0 field md:col-12"
          >
            <div class="col-6 flex">
              <div class="col-4 text-left"></div>
              <small class="col-8 p-error p-0">
                <span class="col-12 p-0">{{
                  v$.phone_number.required.$message
                    .replace("Value", "Số điện thoại liên hệ")
                    .replace("is required", "không được để trống")
                }}</span>
              </small>
            </div>
          </div>
        </div>
        <div v-else class="col-12 md:col-12 p-0">
          <div class="field col-12 md:col-12 text-lg text-primary font-bold">
            Thông tin cá nhân
          </div>
          <div class="field col-12 md:col-12 flex">
            <div class="col-6 p-0 flex align-items-center">
              <label class="col-4 text-left p-0">Xưng hô</label>
              <Dropdown
                v-model="device_provider.vocative"
                :options="listVocative"
                optionLabel="name"
                optionValue="code"
                placeholder="-------- Chọn xưng hô --------"
                spellcheck="false"
                class="col-8 ip36 p-0 sel-placeholder"
              >
              </Dropdown>
            </div>
            <div class="col-6 p-0 pl-2">
              <div class="col-12 p-0 flex align-items-center">
                <label class="col-4 text-left p-0"
                  >Họ và tên <span class="redsao">(*)</span></label
                >
                <InputText
                  v-model="device_provider.full_name"
                  spellcheck="false"
                  class="col-8 ip36 px-2"
                  :class="{
                    'p-invalid': vUs$.full_name.$invalid && submitted,
                  }"
                />
              </div>
            </div>
          </div>
          <div
            v-if="
              (vUs$.full_name.$invalid && submitted) ||
              vUs$.full_name.$pending.$response
            "
            style="display: flex"
            class="col-12 p-0 field md:col-12"
          >
            <div class="col-6 p-0"></div>
            <div class="col-6 p-0 flex">
              <div class="col-4 text-left"></div>
              <small class="col-8 p-error p-0">
                <span class="col-12 p-0">{{
                  vUs$.full_name.required.$message
                    .replace("Value", "Họ và tên ")
                    .replace("is required", "không được để trống")
                }}</span>
              </small>
            </div>
          </div>
          <div class="field col-12 md:col-12 flex">
            <div class="col-6 p-0 flex align-items-center">
              <label class="col-4 text-left p-0"
                >Địa chỉ <span class="redsao">(*)</span></label
              >
              <InputText
                v-model="device_provider.contact_address"
                spellcheck="false"
                class="col-8 ip36 px-2"
                :class="{
                  'p-invalid': vUs$.contact_address.$invalid && submitted,
                }"
              />
            </div>
            <div class="col-6 p-0 flex align-items-center pl-2">
              <label class="col-4 text-left p-0"
                >Số điện thoại <span class="redsao">(*)</span></label
              >
              <InputText
                v-model="device_provider.phone_number"
                spellcheck="false"
                class="col-8 ip36"
                :useGrouping="false"
                :class="{
                  'p-invalid': vUs$.phone_number.$invalid && submitted,
                }"
                @keyup="checkNumber($event, 'phone_number')"
              />
            </div>
          </div>
          <div
            v-if="
              (vUs$.full_name.$invalid && submitted) ||
              vUs$.full_name.$pending.$response ||
              (vUs$.phone_number.$invalid && submitted) ||
              vUs$.phone_number.$pending.$response
            "
            class="col-12 p-0 field flex md:col-12"
          >
            <div class="col-6 p-0 flex">
              <div class="col-4 text-left"></div>
              <small
                class="col-8 p-error p-0"
                v-if="
                  (vUs$.contact_address.$invalid && submitted) ||
                  vUs$.contact_address.$pending.$response
                "
              >
                <span class="col-12 p-0">{{
                  vUs$.contact_address.required.$message
                    .replace("Value", "Địa chỉ ")
                    .replace("is required", "không được để trống")
                }}</span>
              </small>
            </div>
            <div class="col-6 flex">
              <div class="col-4 text-left"></div>
              <small
                class="col-8 p-error p-0"
                v-if="
                  (vUs$.phone_number.$invalid && submitted) ||
                  vUs$.phone_number.$pending.$response
                "
              >
                <span class="col-12 p-0">{{
                  vUs$.phone_number.required.$message
                    .replace("Value", "Số điện thoại liên hệ")
                    .replace("is required", "không được để trống")
                }}</span>
              </small>
            </div>
          </div>
          <div class="field col-12 md:col-12 flex">
            <div class="col-6 p-0 flex align-items-center">
              <label class="col-4 text-left p-0">Email</label>
              <InputText
                v-model="device_provider.contact_email"
                spellcheck="false"
                class="col-8 ip36 px-2"
              />
            </div>
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-6 p-0 flex align-items-center">
            <label class="col-4 text-left p-0">STT</label>
            <InputNumber
              v-model="device_provider.is_order"
              spellcheck="false"
              class="col-8 ip36 p-0"
            />
          </div>
          <div class="col-6 p-0 flex align-items-center pl-2">
            <label
              style="vertical-align: text-bottom"
              class="col-4 text-left p-0"
              >Trạng thái
            </label>
            <InputSwitch
              class="w-4rem lck-checked"
              v-model="device_provider.status"
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
        @click="saveWarehouse(!v$.$invalid, !vUs$.$invalid)"
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
</style>