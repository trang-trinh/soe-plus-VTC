<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import { encr } from "../../util/function.js";
import moment from "moment";
const cryoptojs = inject("cryptojs");
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const isDynamicSQL = ref(false);
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const filters = ref({
  global: { value: null, matchMode: FilterMatchMode.CONTAINS },
  status_name: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
});
const rules = {
  status_name: {
    required,
    $errors: [
      {
        $property: "status_name",
        $validator: "required",
        $message: "Tên trạng thái không được để trống!",
      },
    ],
  },
  status_id: {
    required,
    $errors: [
      {
        $property: "status_id",
        $validator: "required",
        $message: "Mã trạng thái không được để trống!",
      },
    ],
  },
};
const caStatus = ref({
  status_name: "",
  status: true,
  is_order: 1,
  background_color: "",
  text_color: "",
  is_handle: false,
});
const selectedFields = ref();
const submitted = ref(false);
const v$ = useVuelidate(rules, caStatus);
const issaveField = ref(false);
const datalists = ref();
const toast = useToast();
const basedomainURL = baseURL;
const checkDelList = ref(false);
const options = ref({
  IsNext: true,
  sort: "status_id",
  SearchText: "",
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
      baseURL + "/api/DictionaryProc/getData",
      {
        str: encr(
          JSON.stringify({ proc: "ca_status_count" }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        options.value.totalRecords = data[0].totalRecords;
        sttField.value = options.value.totalRecords + 1;
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
const loadData = (rf) => {
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
        baseURL + "/api/DictionaryProc/getData",
        {
          str: encr(
            JSON.stringify({
              proc: "ca_status_list",
              par: [
                { par: "pageno", va: options.value.PageNo },
                { par: "pagesize", va: options.value.PageSize },
                { par: "search", va: options.value.SearchText },
                { par: "status", va: options.value.Status },
              ],
            }),
            SecretKey,
            cryoptojs,
          ).toString(),
        },
        config,
      )
      .then((response) => {
        let data = JSON.parse(response.data.data)[0];
        if (isFirst.value) isFirst.value = false;
        data.forEach((element, i) => {
          element.STT = options.value.PageNo * options.value.PageSize + i + 1;
        });
        datalists.value = data;
        options.value.loading = false;
      })
      .catch((error) => {
        toast.error("Tải dữ liệu không thành công!");
        options.value.loading = false;
        addLog({
          title: "Lỗi Console loadData",
          controller: "StatusView.vue",
          logcontent: error.message,
          loai: 2,
        });
        if (error && error.status === 401) {
          swal.fire({
            title: "Thông báo",
            text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
            icon: "error",
            confirmButtonText: "OK",
          });
          store.commit("gologout");
        }
      });
  }
};
//Phân trang dữ liệu
const onPage = (event) => {
  if (event.rows != options.value.PageSize) {
    options.value.PageSize = event.rows;
  }
  options.value.PageNo = event.page;
  loadData(true);
};
//Hiển thị dialog
const headerDialog = ref();
const displayBasic = ref(false);
const isADD = ref(false);
const openBasic = (str) => {
  if (str.toString() == "Thêm trạng thái") {
    isADD.value = true;
  }
  isShowMain.value = false;
  submitted.value = false;
  caStatus.value = {
    status_name: "",
    is_order: sttField.value,
    status: true,
  };
  issaveField.value = false;
  headerDialog.value = str;
  displayBasic.value = true;
};
const closeDialog = () => {
  caStatus.value = {
    status_id: "",
    status_name: "",
    is_order: 1,
    status: true,
  };
  displayBasic.value = false;
  loadData(true);
  isADD.value = false;
};

//Thêm bản ghi
const saveField = (isFormValid) => {
  submitted.value = true;
  if (!isFormValid) {
    return;
  }
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  if (caStatus.value.is_handle == null) caStatus.value.is_handle = 0;
  axios({
    method: issaveField.value ? "put" : "post",
    url:
      baseURL +
      `/api/ca_status/${issaveField.value ? "Update_status" : "Add_status"}`,
    data: caStatus.value,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success(
          response.data.method == "put"
            ? "Cập nhật trạng thái thành công!"
            : "Thêm mới trạng thái thành công!",
        );
        loadData(true);
        closeDialog();
      } else {
        let ms = response.data.ms;
        swal.fire({
          title: "Thông báo!",
          text:
            ms.includes("status_id") == true &&
            ms.includes("status_name") == true
              ? "Mã trạng thái không quá 50 ký tự!" +
                " Tên trạng thái không quá 50 ký tự!"
              : ms.includes("status_name")
              ? "Tên trạng thái không quá 50 ký tự!"
              : "Mã trạng thái không quá 50 ký tự!",
          icon: "error",
          confirmButtonText: "OK",
        });
        loadData(true);
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
};

const sttField = ref();
//Sửa bản ghi
const editField = (dataPlace) => {
  submitted.value = false;
  caStatus.value = dataPlace;
  headerDialog.value = "Sửa trạng thái";
  issaveField.value = true;
  displayBasic.value = true;
};
//Xóa bản ghi
const delField = (Field) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá trạng thái này không!",
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
          .delete(baseURL + "/api/ca_status/Delete_status", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: Field != null ? [Field.status_id] : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá trạng thái thành công!");
              if (
                (options.value.totalRecords - Field.length) % 2 == 0 &&
                options.value.PageNo > 0
              ) {
                options.value.PageNo = options.value.PageNo - 1;
              }
              loadData(true);
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
            if (error.status === 401) {
              swal.fire({
                title: "Thông báo",
                text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
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
      ImportExcel("ImportExcel");
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
  filterPhanloai.value =
    filterPhanloai.value != null && filterPhanloai.value != ""
      ? filterPhanloai.value
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
        excelname: "DANH SÁCH TRẠNG THÁI",
        proc: "ca_status_listexport",
        par: [
          { par: "s", va: options.value.SearchText },
          { par: "status", va: filterTrangthai.value },
          { par: "handle", va: filterPhanloai.value },
        ],
      },
      config,
    )
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        swal.close();

        toast.success("Kết xuất Data thành công!");
        if (response.data.path != null) {
          let pathReplace = response.data.path
            .replace(/\\+/g, "/")
            .replace(/\/+/g, "/")
            .replace(/^\//g, "");
          var listPath = pathReplace.split("/");
          var pathFile = "";
          listPath.forEach((item) => {
            if (item.trim() != "") {
              pathFile += "/" + item;
            }
          });
          //window.open(baseURL + response.data.path);
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
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
//Sort
const onSort = (event) => {
  options.value.sort =
    event.sortField + (event.sortOrder == 1 ? " ASC" : " DESC");
  if (event.sortField == "STT") {
    options.value.sort = "is_order" + (event.sortOrder == 1 ? " ASC" : " DESC");
  }
  options.value.PageNo = 0;
  isDynamicSQL.value = true;
  loadDataSQL();
};
const filterSQL = ref([]);
const isFirst = ref(true);
const loadDataSQL = () => {
  let data = {
    sqlS: filterTrangthai.value != null ? filterTrangthai.value : null,
    sqlO: options.value.sort,
    Search: options.value.SearchText,
    PageNo: options.value.PageNo,
    PageSize: options.value.PageSize,
    sqlF:
      filterPhanloai.value != undefined && filterPhanloai.value != null
        ? store.state.user.is_super
          ? filterPhanloai.value
          : filterPhanloai.value == 0
          ? 0
          : store.state.user.organization_id
        : null,
  };
  options.value.loading = true;
  axios
    .post(baseURL + "/api/SQL/Filter_Status", data, config)
    .then((response) => {
      let dt = JSON.parse(response.data.data);
      let data = dt[0];
      if (data.length > 0) {
        data.forEach((element, i) => {
          element.STT = options.value.PageNo * options.value.PageSize + i + 1;
        });
        if (options.value.sort == "is_order DESC") {
          {
            data.forEach((element, i) => {
              element.STT =
                options.value.totalRecords -
                options.value.PageNo * options.value.PageSize -
                i;
            });
          }
        }
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
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
//Tìm kiếm
const searchFields = (event) => {
  if (event.code == "Enter") {
    options.value.loading = true;
    loadData(true);
  }
};
const first = ref();
const refreshReceivePlace = (event) => {
  first.value = 0;
  styleObj.value = "";
  options.value = {
    PageNo: 0,
    PageSize: 20,
  };
  selectedFields.value = [];
  options.value.SearchText = "";
  options.value.loading = true;
  loadData(true);
};
const onFilter = (event) => {
  filterSQL.value = [];

  for (const [key, value] of Object.entries(event.filters)) {
    if (key != "global") {
      let obj = {
        key: key != "status_name" ? "status_name" : key,
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
  options.value.PageNo = 1;
  options.value.id = null;
  isDynamicSQL.value = true;
  loadDataSQL();
};
//Checkbox
const onCheckBox = (value, check) => {
  let val = null;
  if (check) {
    val = value.status;
  } else {
    val = value.is_handle;
  }
  let data = {
    IntID: value.status_id,
    TextID: value.status_id + "",
    IntTrangthai: 1,
    BitTrangthai: val,
  };
  if (store.state.user.is_super == true) {
    axios
      .put(
        baseURL +
          `/api/ca_status/${
            check ? "Update_Status_Status" : "Update_Handle_Status"
          }`,
        data,
        config,
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Cập nhật trạng thái thành công!");
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
      text: "Bạn không có quyền chỉnh sửa! Chỉ có Quản trị viên hệ thống mới có quyền chỉnh sửa mục này",
      icon: "error",
      confirmButtonText: "OK",
    });
    loadData(true);
  }
};
//Xóa nhiều
const deleteList = () => {
  let listId = new Array(selectedFields.value.length);
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
        selectedFields.value.forEach((item) => {
          listId.push(item.status_id);
        });
        axios
          .delete(baseURL + "/api/ca_status/Delete_status", {
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
            if (error.status === 401) {
              swal.fire({
                title: "Thông báo",
                text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
                icon: "error",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
};

//Filter
watch(selectedFields, () => {
  if (selectedFields.value.length > 0) {
    checkDelList.value = true;
  } else {
    checkDelList.value = false;
  }
});
const isShowMain = ref(false);
const opColor = ref();
let pcolor = "";
const toggleColor = (event, pc) => {
  opColor.value.toggle(event);
  pcolor = pc;
};
const changeColor = (color) => {
  caStatus.value[pcolor] = color.hex;
  if (!color.hex.includes("#")) opColor.value.hide();
};
const filterPhanloai = ref();
const filterTrangthai = ref();
const op = ref();
const toggle = (event) => {
  op.value.toggle(event);
};
const styleObj = ref();
const style = ref({
  "background-color": "#2196F3 !important",
  color: "#fff !important",
  " border": "1px solid #5ca7e3 !important",
});
const trangThai = ref([
  { name: "Hiển thị", code: 1 },
  { name: "Không hiển thị", code: 0 },
]);
const phanLoai = ref([
  { name: "Xử lý", code: 1 },
  { name: "Không xử lý", code: 0 },
]);
const filter = () => {
  styleObj.value = style.value;
  isDynamicSQL.value = true;
  loadData(true);
};
const reFilter = () => {
  styleObj.value = "";
  filterPhanloai.value = "";
  filterTrangthai.value = "";
  loadData(true);
};
const Imp = ref(false);
const ImportExcel = () => {
  if (store.state.user.is_super) {
    Imp.value = true;
  } else {
    swal.fire({
      title: "Thông báo",
      html: "Bạn không có quyền thêm! Chỉ có quản trị viên hệ thống mới thêm được mục này!",
      icon: "error",
      confirmButtonText: "OK",
    });
  }
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
  let checkFile;
  Imp.value = false;
  let formData = new FormData();
  if (files.length == 0) {
    checkFile = "Chưa có tệp tải lên!";
  }
  for (var i = 0; i < files.length; i++) {
    let file = files[i];
    formData.append("url_file", file);

    if (
      file.name.includes(".xls") == true ||
      file.name.includes(".xlsx") == true ||
      file.name.includes(".xlsm") == true ||
      file.name.includes(".csv")
    ) {
      checkFile = null;
    } else {
      checkFile = "File không đúng định dạng! Vui lòng kiểm tra lại!";
    }
  }
  if (checkFile == null) {
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
    axios
      .post(baseURL + "/api/ImportExcel/Import_Status", formData, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Nhập dữ liệu thành công");
          isDynamicSQL.value = false;
          loadData(true);
        } else {
          swal.close();
          swal.fire({
            title: "Thông báo",
            html: "Vui lòng kiểm tra lại: <br>" + response.data.ms,
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
    swal.close();
    swal.fire({
      title: "Thông báo",
      text: checkFile,
      icon: "error",
      confirmButtonText: "OK",
    });
  }
};
const item = "/Portals/Mau Excel/Mẫu Excel Trạng thái.xlsx";

onMounted(() => {
  loadData(true);
  return {
    datalists,
    options,
    onPage,
    loadData,
    loadCount,
    openBasic,
    closeDialog,
    basedomainURL,
    saveField,
    isFirst,
    searchFields,
    onCheckBox,
    selectedFields,
    deleteList,
  };
});
</script>
<template>
  <div class="main-layout true flex-grow-1 p-2">
    <DataTable
      v-model:first="first"
      :value="datalists"
      :rows="options.PageSize"
      :scrollable="true"
      scrollHeight="flex"
      :loading="options.loading"
      v-model:selection="selectedFields"
      :lazy="true"
      @page="onPage($event)"
      @filter="onFilter($event)"
      @sort="onSort($event)"
      :totalRecords="options.totalRecords"
      dataKey="status_id"
      :rowHover="true"
      v-model:filters="filters"
      filterDisplay="menu"
      :showGridlines="true"
      filterMode="lenient"
      :paginator="true"
      paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
      :rowsPerPageOptions="[20, 30, 50, 100, 200]"
      responsiveLayout="scroll"
    >
      <template #header>
        <h3 class="module-title mt-0 ml-1 mb-2">
          <i class="pi pi-palette"></i> Danh sách trạng thái ({{
            options.totalRecords
          }})
        </h3>
        <Toolbar class="w-full custoolbar">
          <template #start>
            <span class="p-input-icon-left">
              <i class="pi pi-search" />
              <InputText
                v-model="options.SearchText"
                @keyup="searchFields"
                type="text"
                spellcheck="false"
                placeholder="Tìm kiếm"
              />
            </span>
            <Button
              type="button"
              class="ml-2 p-button-outlined p-button-secondary"
              icon="pi pi-filter"
              @click="toggle"
              aria:haspopup="true"
              aria-controls="overlay_panel"
              :style="[styleObj]"
              v-tooltip="'Bộ lọc'"
            />
            <OverlayPanel
              ref="op"
              appendTo="body"
              class="p-0 m-0"
              :showCloseIcon="false"
              id="overlay_panel"
              style="width: 300px"
            >
              <div class="grid formgrid m-0">
                <div class="flex field col-12 p-0">
                  <div
                    class="col-4 text-left pt-2 p-0"
                    style="text-align: left"
                  >
                    Phân loại
                  </div>
                  <div class="col-8">
                    <Dropdown
                      class="col-12 p-0 m-0"
                      v-model="filterPhanloai"
                      :options="phanLoai"
                      optionLabel="name"
                      optionValue="code"
                      placeholder="Phân loại"
                    />
                  </div>
                </div>

                <div class="flex field col-12 p-0">
                  <div
                    class="col-4 text-left pt-2 p-0"
                    style="text-align: left"
                  >
                    Trạng thái
                  </div>
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
                <div class="flex col-12 p-0">
                  <Toolbar
                    class="border-none surface-0 outline-none pb-0 w-full"
                  >
                    <template #start>
                      <Button
                        @click="reFilter"
                        class="p-button-outlined"
                        label="Xóa"
                      ></Button>
                    </template>
                    <template #end>
                      <Button
                        @click="filter"
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
              @click="openBasic('Thêm trạng thái')"
              label="Thêm mới"
              icon="pi pi-plus"
              class="mr-2"
              v-if="store.state.user.is_super"
            />
            <Button
              @click="refreshReceivePlace"
              class="mr-2 p-button-outlined p-button-secondary"
              icon="pi pi-refresh"
              v-tooltip="'Tải lại'"
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
          </template> </Toolbar
      ></template>

      <Column
        selectionMode="multiple"
        headerStyle="text-align:center;max-width:75px;height:50px"
        bodyStyle="text-align:center;max-width:75px;max-height:60px"
        class="align-items-center justify-content-center text-center"
        v-if="store.state.user.is_super == true"
      ></Column>
      <Column
        field="STT"
        header="STT"
        :sortable="true"
        headerStyle="text-align:center;max-width:100px;height:50px"
        bodyStyle="text-align:center;max-width:100px;;max-height:60px"
        class="align-items-center justify-content-center text-center"
      >
      </Column>
      <Column
        field="status_id"
        header="Mã trạng thái"
        :sortable="true"
        headerStyle="text-align:center;max-width:150px;height:50px"
        bodyStyle="text-align:center;max-width:150px;;max-height:60px"
        class="align-items-center justify-content-center text-center"
      >
      </Column>
      <Column
        field="status_name"
        header="Tên trạng thái"
        :sortable="true"
        headerStyle="height:50px"
        bodyStyle="max-height:60px"
        ><template #body="md">
          <Chip
            :style="{
              background: md.data.background_color,
              color: md.data.text_color,
            }"
            v-bind:label="md.data.status_name"
          />
        </template>
      </Column>

      <Column
        field="status"
        header="Hiển thị"
        headerStyle="text-align:center;max-width:120px;height:50px"
        bodyStyle="text-align:center;max-width:120px;;max-height:60px"
        class="align-items-center justify-content-center text-center"
      >
        <template #body="data">
          <Checkbox
            :binary="data.data.status"
            v-model="data.data.status"
            @click="onCheckBox(data.data, true)"
          />
        </template>
      </Column>
      <Column
        field="is_handle"
        header="Xử lý"
        headerStyle="text-align:center;max-width:125px;height:50px"
        bodyStyle="text-align:center;max-width:125px;;max-height:60px"
        class="align-items-center justify-content-center text-center"
      >
        <template #body="data">
          <Checkbox
            :binary="data.data.is_handle"
            v-model="data.data.is_handle"
            @click="onCheckBox(data.data, false)"
          />
        </template>
      </Column>
      <Column
        header="Chức năng"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:150px;height:50px"
        bodyStyle="text-align:center;max-width:150px;;max-height:60px"
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
              @click="editField(data.data)"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-pencil"
              v-tooltip="'Sửa'"
            ></Button>
            <Button
              @click="delField(data.data, true)"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-trash"
              v-tooltip="'Xóa'"
            ></Button>
          </div>
        </template>
      </Column>
      <template #empty>
        <div
          class="align-items-center justify-content-center p-4 text-center m-auto"
          v-if="!isFirst"
        >
          <img
            src="../../assets/background/nodata.png"
            height="144"
          />
          <h3 class="m-1">Không có dữ liệu</h3>
        </div>
      </template>
    </DataTable>
  </div>

  <Dialog
    :header="headerDialog"
    v-model:visible="displayBasic"
    :style="{ width: '40vw' }"
    :closable="false"
  >
    <form>
      <div class="grid formgrid m-2">
        <div
          style="display: flex"
          class="field col-12 md:col-12"
        >
          <div class="col-12 p-0">
            <div class="col-12 p-0 line-height-6">
              <div class="col-12 p-0 flex my-2">
                <div class="col-3 text-left p-0 pb-2 line-height-4">
                  Mã trạng thái <span class="redsao">(*)</span>
                </div>
                <InputText
                  v-model="caStatus.status_id"
                  spellcheck="false"
                  class="col-8 p-0 m-0 ip36 px-2"
                  :disabled="isADD ? false : true"
                />
              </div>
              <div class="col-12 flex p-0">
                <div class="col-3"></div>
                <small
                  v-if="
                    (v$.status_id.$invalid && submitted) ||
                    v$.status_id.$pending.$response
                  "
                  class="col-9 p-error p-0 line-height-4"
                >
                  <span class="col-12">{{
                    v$.status_id.required.$message
                      .replace("Value", "Mã trạng thái")
                      .replace("is required", "không được để trống")
                  }}</span>
                </small>
              </div>
            </div>
            <div class="col-12 p-0 line-height-6">
              <div class="col-12 p-0 flex my-2">
                <div class="col-3 text-left p-0 pb-2 line-height-4">
                  Tên trạng thái <span class="redsao">(*)</span>
                </div>
                <InputText
                  v-model="caStatus.status_name"
                  spellcheck="false"
                  class="col-8 p-0 m-0 ip36 px-2"
                />
              </div>
              <div class="col-12 flex p-0">
                <div class="col-3"></div>
                <small
                  v-if="
                    (v$.status_name.$invalid && submitted) ||
                    v$.status_name.$pending.$response
                  "
                  class="col-9 p-error p-0 line-height-4"
                >
                  <span class="col-12">{{
                    v$.status_name.required.$message
                      .replace("Value", "Tên trạng thái")
                      .replace("is required", "không được để trống")
                  }}</span>
                </small>
              </div>
            </div>

            <div class="field col-12 md:col-12 p-0 flex">
              <div class="col-6 flex p-0">
                <div class="pb-2 col-6 p-0 line-height-4 field">Màu chữ</div>
                <Button
                  class="p-button-rounded p-button-outlined p-button-secondary col-6 field"
                  :style="{
                    backgroundColor: caStatus.text_color,
                    color: caStatus.text_color ? 'transparent' : '#333',
                    border: '1px solid #ccc',
                  }"
                  type="button"
                  icon="pi pi-palette"
                  @click="toggleColor($event, 'text_color')"
                />
              </div>

              <OverlayPanel ref="opColor">
                <ColorPicker
                  theme="dark"
                  @changeColor="changeColor"
                  :sucker-hide="true"
                />
              </OverlayPanel>
              <div class="col-6 flex p-0">
                <div
                  class="pb-2 col-6 p-0 line-height-4 field text-center px-2"
                >
                  Màu nền
                </div>
                <Button
                  class="p-button-rounded p-button-outlined p-button-secondary col-6"
                  :style="{
                    backgroundColor: caStatus.background_color,
                    color: caStatus.background_color ? 'transparent' : '#333',
                    border: '1px solid #ccc',
                  }"
                  type="button"
                  icon="pi pi-palette"
                  @click="toggleColor($event, 'background_color')"
                />
              </div>
            </div>

            <div class="col-12 p-0 my-3 flex">
              <div class="col-6 flex p-0">
                <div class="pb-2 col-6 p-0 line-height-4">Xử lý</div>
                <InputSwitch
                  v-model="caStatus.is_handle"
                  class="col-6 p-0 ip36"
                />
              </div>
              <div class="col-6 flex p-0">
                <div class="pb-2 col-6 p-0 line-height-4 px-2 text-center">
                  Trạng thái
                </div>
                <InputSwitch
                  v-model="caStatus.status"
                  class="col-6 p-0 ip36"
                />
              </div>
            </div>
            <div class="col-12 p-0 my-3 flex">
              <div class="col-6 flex p-0">
                <div class="pb-2 col-6 p-0 line-height-4">STT</div>
                <InputNumber
                  v-model="caStatus.is_order"
                  class="col-6 p-0 ip36"
                />
              </div>
            </div>
          </div>
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="closeDialog"
        class="p-button-text"
      />

      <Button
        label="Lưu"
        icon="pi pi-check"
        @click="saveField(!v$.$invalid)"
      />
    </template>
  </Dialog>
  <Dialog
    header="Tải lên file Excel"
    v-model:visible="Imp"
    :style="{ width: '40vw' }"
    :closable="true"
  >
    <h3>
      <label>
        <a
          :href="basedomainURL + item"
          download
          >Nhấn vào đây</a
        >
        để tải xuống tệp mẫu.
      </label>
    </h3>
    <form>
      <FileUpload
        accept=".xls,.xlsx"
        @remove="removeFile"
        @select="selectFile"
        :show-upload-button="false"
        choose-label="Chọn tệp"
        cancel-label="Hủy"
        :fileLimit="1"
        :invalidFileTypeMessage="'Chỉ chấp nhận file dạng .xsl,.xlsx,.slsm,.csv'"
      >
        <template #empty>
          <p>Kéo và thả tệp vào đây để tải lên.</p>
        </template>
      </FileUpload>
    </form>
    <template #footer>
      <Button
        label="Lưu"
        icon="pi pi-check"
        @click="Upload"
      />
    </template>
    <!-- Chức năng đang chỉnh sửa vui lòng liên hệ quản trị viên phần mềm -->
  </Dialog>
</template>

<style scoped></style>
<style></style>
