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
  field_name: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
});
const rules = {
  field_name: {
    required,
    $errors: [
      {
        $property: "field_name",
        $validator: "required",
        $message: "Tên lĩnh vực không được để trống!",
      },
    ],
  },
};
const field = ref({
  field_name: "",
  year: 1970,
  current_num: 1,
  tracking_place: "",
  nav_type: 0,
  status: true,
  is_order: 1,
});
const selectedFields = ref();
const submitted = ref(false);
const v$ = useVuelidate(rules, field);
const issaveField = ref(false);
const datalists = ref();
const toast = useToast();
const basedomainURL = baseURL;
const checkDelList = ref(false);
const options = ref({
  IsNext: true,
  sort: "is_order desc",
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
          JSON.stringify({
            proc: "ca_fields_count",
            par: [{ par: "user_id", va: store.state.user.user_id }],
          }),
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
  loadDonvi();
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
              proc: "ca_fields_list",
              par: [
                { par: "pageno", va: options.value.PageNo },
                { par: "pagesize", va: options.value.PageSize },
                { par: "user_id", va: store.state.user.user_id },
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
          controller: "FieldView.vue",
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
  }
};
const checkSort = ref(false);
//Phân trang dữ liệu
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

    options.value.id = datalists.value[datalists.value.length - 1].field_id;
    options.value.IsNext = true;
    if (checkSort.value) {
      options.value.id = null;
    }
  } else if (event.page < options.value.PageNo) {
    //Trang trước
    options.value.id = datalists.value[0].field_id;
    options.value.IsNext = false;
  }
  options.value.PageNo = event.page;
  loadData(true);
};
//Hiển thị dialog
const headerDialog = ref();
const displayBasic = ref(false);
const openBasic = (str) => {
  submitted.value = false;
  field.value = {
    field_name: "",
    is_order: sttField.value,
    status: true,
  };

  field.value.organization_id =
    store.state.user.is_super == true
      ? store.state.user.organization_parent_id != null
        ? store.state.user.organization_parent_id
        : store.state.user.organization_id
      : store.state.user.organization_id;
  field.value.is_system = store.state.user.is_super ? true : false;

  issaveField.value = false;
  headerDialog.value = str;
  displayBasic.value = true;
};
const closeDialog = () => {
  field.value = {
    field_name: "",
    is_order: 1,
    status: true,
  };
  displayBasic.value = false;
  loadData(true);
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
  axios({
    method: issaveField.value ? "put" : "post",
    url:
      baseURL +
      `/api/ca_fields/${
        issaveField.value ? "Update_ca_fields" : "Add_ca_fields"
      }`,
    data: field.value,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success(
          response.config.method == "put"
            ? "Cập nhật lĩnh vực thành công!"
            : "Thêm mới lĩnh vực thành công!",
        );
        closeDialog();
        loadData(true);
      } else {
        let ms = response.data.ms;
        swal.fire({
          title: "Thông báo!",
          text:
            ms.includes("field_name") == true
              ? "Tên lĩnh vực không quá 250 ký tự!"
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
};

const sttField = ref();
//Thêm bản ghi con
const isChirlden = ref(false);

//Sửa bản ghi
const editField = (dataPlace) => {
  submitted.value = false;
  field.value = dataPlace;
  isChirlden.value = false;
  if (dataPlace.parent_id != null) {
    isChirlden.value = true;
  }
  headerDialog.value = "Sửa lĩnh vực";
  issaveField.value = true;
  displayBasic.value = true;
};
//Xóa bản ghi
const delField = (Field) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá lĩnh vực này không!",
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
          .delete(baseURL + "/api/ca_fields/Delete_ca_fields", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: Field != null ? [Field.field_id] : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá lĩnh vực thành công!");
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
      ImportExcel("ImportExcel");
    },
  },
]);
const toggleExport = (event) => {
  menuButs.value.toggle(event);
};
const exportData = (method) => {
  if (store.state.user.is_super == true) {
    if (
      filterPhanloai.value == undefined ||
      Object.keys(filterPhanloai.value)[0] == undefined
    ) {
      options.value.filter_Org = 1;
    } else if (Object.keys(filterPhanloai.value)[0] == 0) {
      options.value.filter_Org = 3; //list hệ thống
    } else options.value.filter_Org = 2; // list đơn vị
  } else {
    if (filterPhanloai.value == undefined) {
      options.value.filter_Org = 1;
    } else if (filterPhanloai.value == 0) {
      options.value.filter_Org = 3; //list hệ thống
    } else options.value.filter_Org = 2; // list đơn vị
  }
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
        excelname: "DANH SÁCH LĨNH VỰC",
        proc: "ca_fields_listexport",
        par: [
          { par: "search", va: options.value.SearchText },
          { par: "status", va: filterTrangthai.value },
          { par: "user_id", va: store.state.user.user_id },
          {
            par: "s_org",
            va:
              filterPhanloai.value != null
                ? Object.keys(filterPhanloai.value)[0]
                : null,
          },
          { par: "filter_Org", va: options.value.filter_Org },
        ],
      },
      config,
    )
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        swal.close();

        toast.success("Kết xuất Data thành công!");
        //window.open(baseURL + response.data.path);
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
  options.value.sort =
    event.sortField + (event.sortOrder == 1 ? " ASC" : " DESC");
  if (event.sortField == "STT") {
    options.value.sort = "is_order" + (event.sortOrder == 1 ? " ASC" : " DESC");
  }
  options.value.PageNo = 0;
  checkSort.value = true;
  isDynamicSQL.value = true;
  loadDataSQL();
};
const filterSQL = ref([]);
const isFirst = ref(true);
const loadDataSQL = () => {
  isDynamicSQL.value = true;
  datalists.value = [];
  if (filterTrangthai.value == undefined) {
    filterTrangthai.value = null;
  }
  if (filterPhanloai.value == undefined) {
    filterPhanloai.value = null;
  }
  let fpl;
  if (filterPhanloai.value != undefined && store.state.user.is_super) {
    fpl = parseInt(Object.keys(filterPhanloai.value)[0]);
  } else {
    fpl =
      filterPhanloai.value != undefined && filterPhanloai.value != null
        ? store.state.user.is_super
          ? filterPhanloai.value
          : filterPhanloai.value == 0
          ? 0
          : store.state.user.organization_id
        : null;
  }
  let data = {
    sqlS: filterTrangthai.value != null ? filterTrangthai.value : null,
    sqlO: options.value.sort,
    Search: options.value.SearchText,
    PageNo: options.value.PageNo,
    PageSize: options.value.PageSize,
    sqlF: fpl,
  };
  options.value.loading = true;
  axios
    .post(baseURL + "/api/SQL/Filter_Field", data, config)
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
              if (options.value.sort == "is_order DESC") {
                {
                  element.STT =
                    options.value.totalRecords -
                    options.value.PageNo * options.value.PageSize -
                    i;
                }
              }
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
      toast.error("Tải dữ liệu không thành công!" + error.message);
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
const searchFields = (event) => {
  options.value.loading = true;
  isDynamicSQL.value = true;
  loadData(true);
};
const onFilter = (event) => {
  filterSQL.value = [];

  for (const [key, value] of Object.entries(event.filters)) {
    if (key != "global") {
      let obj = {
        key: key != "field_name" ? "field_name" : key,
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
  options.value.id = null;
  isDynamicSQL.value = true;
  loadDataSQL();
};
//Checkbox
const onCheckBox = (value) => {
  let data = {
    IntID: value.field_id,
    TextID: value.field_id + "",
    IntTrangthai: 1,
    BitTrangthai: value.status,
  };
  if (
    store.state.user.is_super == true ||
    store.state.user.user_id == value.created_by ||
    (store.state.user.is_admin &&
      value.is_system != true &&
      store.state.user.organization_id == value.organization_id)
  ) {
    axios
      .put(baseURL + "/api/ca_fields/Update_StatusFields", data, config)
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
      text:
        "Bạn không có quyền chỉnh sửa! Chỉ có " +
        (value.is_system
          ? "Quản trị viên hệ thống"
          : "Quản trị viên đơn vị hoặc Quản trị viên hệ thống") +
        " mới có quyền chỉnh sửa mục này",
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
          listId.push(item.field_id);
        });
        axios
          .delete(baseURL + "/api/ca_fields/Delete_ca_fields", {
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
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                icon: "error",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
};
const first = ref();
const searchField = () => {
  first.value = 0;
  options.value.loading = true;
  options.value.PageNo = 0;
  options.value = {
    PageNo: 0,
    PageSize: 20,
  };
  selectedFields.value = [];
  options.value.SearchText = "";
  filterPhanloai.value = "";
  filterTrangthai.value = "";
  styleObj.value = "";
  options.value.loading = true;
  isDynamicSQL.value = false;
  loadData(true);
};

//Filter
const showFilter = ref(false);

const trangThai = ref([
  { name: "Hiển thị", code: 1 },
  { name: "Không hiển thị", code: 0 },
]);
const phanLoai = ref([
  { name: "Hệ thống", code: 0 },
  { name: "Đơn vị", code: 1 },
]);
const filterPhanloai = ref();
const filterTrangthai = ref();

const reFilterEmail = () => {
  filterPhanloai.value = null;
  filterTrangthai.value = null;
  filterFileds();
  showFilter.value = false;
  styleObj.value = "";
};
const filterFileds = () => {
  styleObj.value = style.value;
  loadDataSQL();
  showFilter.value = false;
};
watch(selectedFields, () => {
  if (selectedFields.value.length > 0) {
    checkDelList.value = true;
  } else {
    checkDelList.value = false;
  }
});
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
const loadDonvi = () => {
  axios
    .post(
      baseURL + "/api/DictionaryProc/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_organization_list_dictionary",
            par: [{ par: "user_id", va: store.state.user.user_id }],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      treedonvis.value = [];
      let data = JSON.parse(response.data.data)[0];
      let sys = { key: 0, label: "Hệ thống", data: { organization_id: 0 } };

      treedonvis.value.push(sys);

      if (data.length > 0) {
        if (data.length > 0) {
          data.forEach((x) => {
            x = { key: x.organization_id, data: x, label: x.organization_name };
            treedonvis.value.push(x);
          });
        } else {
          treedonvis.value = [];
        }
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
const Imp = ref(false);
const ImportExcel = () => {
  Imp.value = true;
  files = [];
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
      .post(baseURL + "/api/ImportExcel/Import_Field", formData, config)
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
const treedonvis = ref();
const item = "/Portals/Mau Excel/Mẫu Excel Lĩnh vực.xlsx";

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
      :paginator="true"
      :rows="options.PageSize"
      paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
      :rowsPerPageOptions="[20, 30, 50, 100, 200]"
      :scrollable="true"
      scrollHeight="flex"
      :loading="options.loading"
      v-model:selection="selectedFields"
      :lazy="true"
      @page="onPage($event)"
      @filter="onFilter($event)"
      @sort="onSort($event)"
      :totalRecords="options.totalRecords"
      dataKey="field_id"
      :rowHover="true"
      v-model:filters="filters"
      filterDisplay="menu"
      :showGridlines="true"
      filterMode="lenient"
      responsiveLayout="scroll"
    >
      <template #header>
        <h3 class="module-title mt-0 ml-1 mb-2">
          <i class="pi pi-briefcase"></i> Danh sách lĩnh vực ({{
            options.totalRecords
          }})
        </h3>

        <Toolbar class="w-full custoolbar">
          <template #start>
            <span class="p-input-icon-left">
              <i class="pi pi-search" />
              <InputText
                v-model="options.SearchText"
                @keyup.enter="searchFields"
                type="text"
                spellcheck="false"
                placeholder="Tìm kiếm"
            /></span>

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
              :style="'width:400px'"
            >
              <div class="grid formgrid m-0">
                <div class="flex field col-12 p-0">
                  <div
                    :class="'col-4 text-left pt-2 p-0'"
                    style="text-align: left"
                  >
                    Phân loại
                  </div>

                  <div :class="'col-8'">
                    <TreeSelect
                      v-model="filterPhanloai"
                      :options="treedonvis"
                      optionLabel="data.organization_name"
                      optionValue="data.organization_id"
                      placeholder="Chọn đơn vị"
                      class="col-12 p-0 m-0 md:col-12"
                      v-if="store.state.user.is_super == 1"
                      panelClass="d-design-dropdown"
                    />
                    <Dropdown
                      class="col-12 p-0 m-0"
                      v-model="filterPhanloai"
                      :options="phanLoai"
                      optionLabel="name"
                      optionValue="code"
                      placeholder="Phân loại"
                      v-else
                    />
                  </div>
                </div>

                <div class="flex field col-12 p-0">
                  <div
                    :class="'col-4 text-left pt-2 p-0'"
                    style="text-align: center,justify-content:center"
                  >
                    Trạng thái
                  </div>
                  <div :class="'col-8'">
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
                        @click="reFilterEmail"
                        class="p-button-outlined"
                        label="Xóa"
                      ></Button>
                    </template>
                    <template #end>
                      <Button
                        @click="filterFileds"
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
              @click="openBasic('Thêm lĩnh vực')"
              label="Thêm mới"
              icon="pi pi-plus"
              class="mr-2"
            />
            <Button
              @click="searchField"
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
          </template>
        </Toolbar>
      </template>

      <Column
        selectionMode="multiple"
        class="align-items-center justify-content-center text-center max-w-3rem"
        v-if="store.state.user.is_super == true"
      ></Column>
      <Column
        field="STT"
        header="STT"
        :sortable="true"
        class="align-items-center justify-content-center text-center max-w-5rem"
      >
      </Column>

      <Column
        field="field_name"
        header="Tên lĩnh vực"
        :sortable="true"
      >
      </Column>

      <Column
        field="status"
        header="Hiển thị"
        class="align-items-center justify-content-center text-center max-w-8rem"
      >
        <template #body="data">
          <Checkbox
            :binary="data.data.status"
            v-model="data.data.status"
            @click="onCheckBox(data.data)"
          />
        </template>
      </Column>
      <Column
        field="is_system"
        header="Hệ thống"
        class="align-items-center justify-content-center text-center max-w-8rem"
      >
        <template #body="data">
          <div v-if="data.data.is_system == true">
            <i
              class="pi pi-check text-blue-400"
              style="font-size: 1.5rem"
            ></i>
          </div>
          <div v-else></div>
        </template>
      </Column>
      <Column
        field="organization_name"
        header="Đơn vị"
        class="align-items-center justify-content-center text-center max-w-20rem"
      >
      </Column>
      <Column
        header="Chức năng"
        class="align-items-center justify-content-center text-center max-w-9rem"
      >
        <template #body="Field">
          <div
            v-if="
              store.state.user.is_super == true ||
              store.state.user.user_id == Field.data.created_by ||
              (store.state.user.is_admin &&
                Field.data.is_system != true &&
                Field.data.organization_id == store.state.user.organization_id)
            "
          >
            <Button
              @click="editField(Field.data)"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-pencil"
              v-tooltip="'Sửa'"
            ></Button>
            <Button
              @click="delField(Field.data, true)"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-trash"
              v-tooltip="'Xóa'"
            >
            </Button>
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
            <div class="col-12 p-0 flex my-2">
              <div class="col-3 text-left p-0 pb-2 line-height-4">
                Tên lĩnh vực <span class="redsao">(*)</span>
              </div>
              <InputText
                v-model="field.field_name"
                spellcheck="false"
                class="col-9 p-0 m-0 ip36 px-2"
              />
            </div>
            <div class="col-12 flex p-0">
              <div class="col-3"></div>
              <small
                v-if="
                  (v$.field_name.$invalid && submitted) ||
                  v$.field_name.$pending.$response
                "
                class="col-9 p-error p-0"
              >
                <span class="col-12 p-0">{{
                  v$.field_name.required.$message
                    .replace("Value", "Tên lĩnh vực")
                    .replace("is required", "không được để trống")
                }}</span>
              </small>
            </div>

            <div class="col-12 p-0 my-3 flex">
              <div class="col-6 flex p-0">
                <div class="pb-2 col-6 p-0 line-height-4">STT</div>
                <InputNumber
                  v-model="field.is_order"
                  class="col-6 p-0 ip36"
                />
              </div>
              <div class="col-3 flex p-0">
                <div class="pb-2 col-6 p-0 line-height-4 px-2">Trạng thái</div>
                <InputSwitch
                  v-model="field.status"
                  class="p-0 ip36"
                />
              </div>
              <div
                class="col-3 flex p-0"
                v-if="store.state.user.is_super"
              >
                <div class="pb-2 col-6 p-0 line-height-4 px-2">Hệ thống</div>
                <InputSwitch
                  v-model="field.is_system"
                  class="p-0 ip36"
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
