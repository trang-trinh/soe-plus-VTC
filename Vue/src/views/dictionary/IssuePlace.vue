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
  issue_place_name: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
});
const rules = {
  issue_place_name: {
    required,
    $errors: [
      {
        $property: "issue_place_name",
        $validator: "required",
        $message: "Tên nơi ban hành không được để trống!",
      },
    ],
  },
};
const issuePlace = ref({
  issue_place_name: "",
  current_num: 1,
  tracking_place: "",
  nav_type: 0,
  status: true,
  is_order: 1,
  parent_id: null,
  level: null,
  static_code: "",
  dynamic_code: "",
  search_code: "",
  display_code: "",
});
const selectedFields = ref();
const submitted = ref(false);
const v$ = useVuelidate(rules, issuePlace);
const issaveField = ref(false);
const datalists = ref([]);
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
            proc: "ca_issue_place_count",
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
  if (store.state.user.is_super) {
    loadDonvi();
  }
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
              proc: "ca_issue_place_list",
              par: [
                { par: "pageno", va: options.value.PageNo },
                { par: "pagesize", va: options.value.PageSize },
                { par: "user", va: store.state.user.user_id },
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
        let obj = renderTree(data, "issue_place_id", "", "");
        datalists.value = obj.arrChils;

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
const renderTree = (data, id, name, title) => {
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
  arrtreeChils.unshift({
    key: -1,
    data: -1,
    label: "-----Chọn " + title + "----",
  });
  return { arrChils: arrChils, arrtreeChils: arrtreeChils };
};
//Phân trang dữ liệu
const onPage = (event) => {
  if (event.rows != options.value.PageSize) {
    options.value.PageSize = event.rows;
  }
};
//Hiển thị dialog
const headerDialog = ref();
const displayBasic = ref(false);
const openBasic = (str) => {
  isLengthName.value = false;
  isLengthDisplay_Code.value = false;
  isLengthDynamic_Code.value = false;
  isLengthSearch_Code.value = false;
  isLengthStatic_Code.value = false;
  submitted.value = false;
  issuePlace.value = {
    issue_place_name: "",
    is_order: 1,
    status: true,
    level: 1,
    parent_id: null,
    static_code: "",
    dynamic_code: "",
    search_code: "",
    display_code: "",
  };
  if (store.state.user.is_super == true) {
    issuePlace.value.organization_id = 0;
  } else {
    issuePlace.value.organization_id = store.state.user.organization_id;
  }
  console.log(datalists.value);
  issuePlace.value.is_order =
    datalists.value.length > 0 ? datalists.value[0].data.is_order + 1 : 1;
  issaveField.value = false;
  headerDialog.value = str;
  displayBasic.value = true;
};
const closeDialog = () => {
  issuePlace.value = {
    issue_place_name: "",
    is_order: 1,
    status: true,
  };
  issuePlace.value.organization_id = null;
  displayBasic.value = false;
  loadData(true);
};

//Thêm bản ghi
const saveField = (isFormValid) => {
  submitted.value = true;
  if (
    !isFormValid ||
    isLengthName.value == true ||
    isLengthDisplay_Code.value == true ||
    isLengthDynamic_Code.value == true ||
    isLengthSearch_Code.value == true ||
    isLengthStatic_Code.value == true
  ) {
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
      `/api/ca_issue_places/${
        issaveField.value ? "Update_issue_places" : "Add_issue_place"
      }`,
    data: issuePlace.value,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success(
          response.data.method == "put"
            ? "Cập nhật nơi ban hành thành công!"
            : "Thêm nơi ban hành thành công!",
        );
        loadData(true);
        closeDialog();
      } else {
        let ms = response.data.ms;
        swal.fire({
          title: "Thông báo!",
          text: ms,
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
//Sửa bản ghi
const editField = (dataPlace) => {
  submitted.value = false;
  issuePlace.value = dataPlace;
  headerDialog.value = "Sửa nơi ban hành";
  issaveField.value = true;
  displayBasic.value = true;
  if (store.state.user.is_super == true) {
    issuePlace.value.organization_id = 0;
  } else {
    issuePlace.value.organization_id = store.state.user.organization_id;
  }
};
//Xóa bản ghi
const delField = (Field) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá nơi ban hành này không!",
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
          .delete(baseURL + "/api/ca_issue_places/Delete_issue_places", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: Field != null ? [Field.issue_place_id] : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              if (
                (options.value.totalRecords - 1) % options.value.PageSize ==
                  0 &&
                options.value.totalRecords - 1 != 0
              ) {
                options.value.PageNo = options.value.PageNo - 1;
              }
              toast.success("Xoá nơi ban hành thành công!");
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
        excelname: "DANH SÁCH NƠI BAN HÀNH",
        proc: "ca_issue_place_listexport",
        par: [
          { par: "search", va: options.value.SearchText },
          { par: "status", va: filterTrangthai.value },
          { par: "user_id", va: store.state.user.user_id },
          { par: "s_org", va: filterPhanloai.value },
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
  isDynamicSQL.value = true;
  loadDataSQL();
};

const isFirst = ref(true);
const loadDataSQL = () => {
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
    .post(baseURL + "/api/SQL/Filter_IssuePlace", data, config)
    .then((response) => {
      let dt = JSON.parse(response.data.data);

      let data = dt[0];
      console.log(dt);

      let obj = renderTree(data, "issue_place_id", "", "");

      datalists.value = [];
      datalists.value = obj.arrChils;

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
const searchFields = (event) => {
  options.value.loading = true;
  isDynamicSQL.value = true;
  loadData(true);
};

//Checkbox
const onCheckBox = (value) => {
  let data = {
    IntID: value.issue_place_id,
    TextID: value.issue_place_id + "",
    IntTrangthai: 1,
    BitTrangthai: value.status,
  };
  if (
    store.state.user.is_super == true ||
    store.state.user.user_id == value.created_by ||
    store.state.user.role_id == "admin"
  ) {
    axios
      .put(
        baseURL + "/api/ca_issue_places/Update_StatusIssue_Place",
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
      text: "Bạn không có quyền chỉnh sửa! Chỉ có Quản trị viên đơn vị hoặc Quản trị viên hệ thống mới có quyền chỉnh sửa mục này",
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
          listId.push(item.issue_place_id);
        });
        axios
          .delete(baseURL + "/api/ca_issue_places/Delete_issue_places", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: listId != null ? listId : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá danh sách thành công!");
              checkDelList.value = false;

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
  selectedFields.value = [];
  first.value = 0;
  options.value.PageNo = 0;
  options.value.SearchText = "";
  filterPhanloai.value = "";
  filterTrangthai.value = "";
  options.value = {
    PageNo: 0,
    PageSize: 20,
  };
  styleObj.value = "";
  options.value.loading = true;
  isDynamicSQL.value = false;
  loadData(true);
};

//Filter
const showFilter = ref(false);

const filterPhanloai = ref();
const filterTrangthai = ref();

const reFilterEmail = () => {
  filterPhanloai.value = null;
  filterTrangthai.value = null;
  filterFileds();
  styleObj.value = "";
  showFilter.value = false;
};
const filterFileds = () => {
  styleObj.value = style.value;
  isDynamicSQL.value = true;
  loadData(true);
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
          JSON.stringify({ proc: "sys_org_list" }),
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
const trangThai = ref([
  { name: "Hiển thị", code: 1 },
  { name: "Không hiển thị", code: 0 },
]);
const phanLoai = ref([
  { name: "Hệ thống", code: 0 },
  { name: "Đơn vị", code: 1 },
]);
const treedonvis = ref();
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
      .post(baseURL + "/api/ImportExcel/Import_IssuePlace", formData, config)
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
const item = "/Portals/Mau Excel/Mẫu Excel Nơi ban hành.xlsx";
const isLengthName = ref(false);
const isLengthStatic_Code = ref(false);
const isLengthDynamic_Code = ref(false);
const isLengthSearch_Code = ref(false);
const isLengthDisplay_Code = ref(false);
const checkName = () => {
  const textbox = document.getElementById("name");
  if (textbox.value.length > 250) {
    isLengthName.value = true;
  } else {
    isLengthName.value = false;
  }
};
const checkSttCode = () => {
  const textbox = document.getElementById("stt");
  if (textbox.value.length > 50) {
    isLengthStatic_Code.value = true;
  } else {
    isLengthStatic_Code.value = false;
  }
};
const checkDynCode = () => {
  const textbox = document.getElementById("dyn");
  if (textbox.value.length > 50) {
    isLengthDynamic_Code.value = true;
  } else {
    isLengthDynamic_Code.value = false;
  }
};
const checkSearchCode = () => {
  const textbox = document.getElementById("search_code");
  if (textbox.value.length > 50) {
    isLengthSearch_Code.value = true;
  } else {
    isLengthSearch_Code.value = false;
  }
};
const checkDisplayCode = () => {
  const textbox = document.getElementById("display");
  if (textbox.value.length > 50) {
    isLengthDisplay_Code.value = true;
  } else {
    isLengthDisplay_Code.value = false;
  }
};
const openChild = (data) => {
  isLengthName.value = false;
  isLengthDisplay_Code.value = false;
  isLengthDynamic_Code.value = false;
  isLengthSearch_Code.value = false;
  isLengthStatic_Code.value = false;
  submitted.value = false;
  issuePlace.value = {
    parent_name: "",
    issue_place_name: "",
    status: true,
    static_code: "",
    dynamic_code: "",
    search_code: "",
    display_code: "",
  };
  issuePlace.value.parent_name = data.data.issue_place_name;
  issuePlace.value.level = data.data.level + 1;
  issuePlace.value.parent_id = data.data.issue_place_id;
  issuePlace.value.is_order =
    data.data.maxIsOrder != null
      ? data.data.maxIsOrder + 1
      : data.children != null
      ? data.children[0].data.is_order + 1
      : 1;
  if (store.state.user.is_super == true) {
    issuePlace.value.organization_id = 0;
  } else {
    issuePlace.value.organization_id = store.state.user.organization_id;
  }
  issaveField.value = false;
  headerDialog.value = "Thêm nơi ban hành con";
  displayBasic.value = true;
};
const expandedKeys = ref();
const selectedKeys = ref();
onMounted(() => {
  loadData(true);
  return {};
});
</script>
<template>
  <div class="main-layout true flex-grow-1 p-2">
    <TreeTable
      ref="dt"
      :rowHovers="true"
      :showGridlines="true"
      responsiveLayout="scroll"
      @page="onPage($event)"
      v-model:first="first"
      :expandedKeys="expandedKeys"
      :value="datalists"
      :paginator="true"
      :rows="options.PageSize"
      paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
      :rowsPerPageOptions="[20, 30, 50, 100, 200]"
      :scrollable="true"
      scrollHeight="flex"
      :totalRecords="options.totalRecords"
      dataKey="issue_place_id"
      v-model:selectionKeys="selectedKeys"
    >
      <template #header>
        <h3 class="module-title mt-0 ml-1 mb-2">
          <i class="pi pi-briefcase"></i>
          Danh sách nơi ban hành ({{ options.totalRecords }})
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
              />
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
                class="m-0"
                :showCloseIcon="false"
                id="overlay_panel"
                :style="
                  store.state.user.is_super == 1 ? 'width:40vw' : 'width:300px'
                "
              >
                <div class="grid formgrid m-0">
                  <div class="flex field col-12">
                    <div
                      :class="
                        store.state.user.is_super == 1
                          ? 'col-2 text-left pt-2 '
                          : 'col-4 text-left pt-2 '
                      "
                      style="text-align: left"
                    >
                      Phân loại
                    </div>

                    <div
                      :class="
                        store.state.user.is_super == 1 ? 'col-10' : 'col-8'
                      "
                    >
                      <TreeSelect
                        v-model="filterPhanloai"
                        :options="treedonvis"
                        optionLabel="data.organization_name"
                        optionValue="data.organization_id"
                        placeholder="Chọn đơn vị"
                        class="col-12md:col-12"
                        v-if="store.state.user.is_super == 1"
                      />
                      <Dropdown
                        class="col-12 m-0"
                        v-model="filterPhanloai"
                        :options="phanLoai"
                        optionLabel="name"
                        optionValue="code"
                        placeholder="Phân loại"
                        v-else
                      />
                    </div>
                  </div>

                  <div class="flex field col-12">
                    <div
                      :class="
                        store.state.user.is_super == 1
                          ? 'col-2 text-left pt-2 '
                          : 'col-4 text-left pt-2 '
                      "
                      style="text-align: center,justify-content:center"
                    >
                      Trạng thái
                    </div>
                    <div
                      :class="
                        store.state.user.is_super == 1 ? 'col-10' : 'col-8'
                      "
                    >
                      <Dropdown
                        class="col-12 m-0"
                        v-model="filterTrangthai"
                        :options="trangThai"
                        optionLabel="name"
                        optionValue="code"
                        placeholder="Trạng thái"
                      />
                    </div>
                  </div>
                  <div class="flex col-12">
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
            </span>
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
              @click="openBasic('Thêm nơi ban hành')"
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
        headerClass="align-items-center justify-content-center text-center max-w-2rem"
        bodyClass="align-items-center justify-content-center text-center "
        bodyStyle="max-width:calc(2rem + 2px)"
        v-if="store.state.user.is_super == true"
      ></Column>
      <Column
        field="is_order"
        header="STT"
        :sortable="true"
        headerClass="align-items-center justify-content-center text-center max-w-4rem"
        bodyClass="align-items-center justify-content-center text-center max-w-4rem"
      >
      </Column>

      <Column
        field="issue_place_name"
        header="Nơi ban hành"
        :sortable="true"
        headerClass="align-items-center justify-content-center text-center"
        bodyClass="  word-break"
        :expander="true"
      >
      </Column>

      <Column
        field="status"
        header="Hiển thị"
        headerClass="align-items-center justify-content-center text-center max-w-6rem"
        bodyClass="align-items-center justify-content-center text-center max-w-6rem"
      >
        <template #body="data">
          <Checkbox
            :binary="data.node.data.status"
            v-model="data.node.data.status"
            @click="onCheckBox(data.node.data)"
          />
        </template>
      </Column>
      <Column
        field="organization_id"
        header="Hệ thống"
        headerClass="align-items-center justify-content-center text-center max-w-8rem"
        bodyClass="align-items-center justify-content-center text-center max-w-8rem"
      >
        <template #body="data">
          <div v-if="data.node.data.organization_id == 0">
            <i
              class="pi pi-check text-blue-400"
              style="font-size: 1.5rem"
            ></i>
          </div>
          <div v-else></div>
        </template>
      </Column>
      <Column
        header="Mã tra cứu"
        field="search_code"
        headerClass="align-items-center justify-content-center text-center max-w-12rem"
        bodyClass=" max-w-12rem  justify-content-center word-break"
      >
      </Column>
      <Column
        header="Mã hiển thị"
        field="display_code"
        headerClass="align-items-center justify-content-center text-center max-w-12rem"
        bodyClass=" max-w-12rem  justify-content-center word-break"
      >
      </Column>
      <Column
        header="Chức năng"
        headerClass="align-items-center justify-content-center text-center max-w-10rem"
        bodyClass="align-items-center justify-content-center text-center max-w-10rem"
      >
        <template #body="data">
          <div
            v-if="
              store.state.user.is_super == true ||
              store.state.user.user_id == data.node.data.created_by ||
              (store.state.user.role_id == 'admin' &&
                store.state.user.organization_id ==
                  data.node.data.organization_id)
            "
          >
            <Button
              @click="openChild(data.node)"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-plus-circle"
              v-tooltip="'Thêm nơi ban hành con'"
            ></Button>
            <Button
              @click="editField(data.node.data)"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-pencil"
              v-tooltip="'Sửa'"
            ></Button>
            <Button
              @click="delField(data.node.data, true)"
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
    </TreeTable>
  </div>

  <Dialog
    :header="headerDialog"
    v-model:visible="displayBasic"
    :style="{ width: '40vw' }"
    :closable="false"
  >
    <form>
      <div class="col-12">
        <div
          class="col-12 flex"
          v-if="issuePlace.parent_name != null"
        >
          <div class="col-3 text-left">Cấp cha</div>
          <InputText
            v-model="issuePlace.parent_name"
            spellcheck="false"
            class="col-9 ip36"
            disabled
          />
        </div>
        <div class="col-12 flex">
          <div class="col-3 text-left">
            Nơi ban hành <span class="redsao">(*)</span>
          </div>
          <InputText
            id="name"
            v-model="issuePlace.issue_place_name"
            spellcheck="false"
            class="col-9 ip36"
            @input="checkName()"
            @paste="checkName()"
          />
        </div>
        <div class="col-12 p-0 flex">
          <div class="col-3"></div>
          <small
            v-if="
              (v$.issue_place_name.$invalid && submitted) ||
              v$.issue_place_name.$pending.$response
            "
            class="col-9 p-0 p-error"
          >
            <span class="col-12">{{
              v$.issue_place_name.required.$message
                .replace("Value", "Tên nơi ban hành")
                .replace("is required", "không được để trống")
            }}</span>
          </small>
        </div>
        <div class="col-12 p-0 flex">
          <div class="col-3"></div>
          <small
            v-if="isLengthName == true"
            class="col-9 p-0 p-error"
          >
            <span class="col-12">Tên nơi ban hành không quá 250 ký tự.</span>
          </small>
        </div>
        <div class="col-12 flex">
          <div class="col-3 text-left">Mã đơn vị gốc</div>
          <InputText
            id="stt"
            v-model="issuePlace.static_code"
            spellcheck="false"
            class="col-9 ip36"
            @input="checkSttCode()"
            @paste="checkSttCode()"
          />
        </div>
        <div class="col-12 p-0 flex">
          <div class="col-3"></div>
          <small
            v-if="isLengthStatic_Code == true"
            class="col-9 p-0 p-error"
          >
            <span class="col-12">Mã đơn vị gốc không quá 50 ký tự.</span>
          </small>
        </div>
        <div class="col-12 flex">
          <div class="col-3 text-left">Mã đơn vị</div>
          <InputText
            id="dyn"
            v-model="issuePlace.dynamic_code"
            spellcheck="false"
            class="col-9 ip36"
            @input="checkDynCode()"
            @paste="checkDynCode()"
          />
        </div>
        <div class="col-12 p-0 flex">
          <div class="col-3"></div>
          <small
            v-if="isLengthDynamic_Code == true"
            class="col-9 p-0 p-error"
          >
            <span class="col-12">Mã đơn vị không quá 50 ký tự.</span>
          </small>
        </div>
        <div class="col-12 flex">
          <div class="col-3 text-left">Mã tra cứu</div>
          <InputText
            id="search_code"
            v-model="issuePlace.search_code"
            spellcheck="false"
            class="col-9 ip36"
            @input="checkSearchCode()"
            @paste="checkSearchCode()"
          />
        </div>
        <div class="col-12 p-0 flex">
          <div class="col-3"></div>
          <small
            v-if="isLengthSearch_Code == true"
            class="col-9 p-0 p-error"
          >
            <span class="col-12">Mã tìm kiếm không quá 50 ký tự.</span>
          </small>
        </div>
        <div class="col-12 flex">
          <div class="col-3 text-left">Mã hiển thị</div>
          <InputText
            id="display"
            v-model="issuePlace.display_code"
            spellcheck="false"
            class="col-9 ip36"
            @input="checkDisplayCode()"
            @paste="checkDisplayCode()"
          />
        </div>
        <div class="col-12 p-0 flex">
          <div class="col-3"></div>
          <small
            v-if="isLengthDisplay_Code == true"
            class="col-9 p-0 p-error"
          >
            <span class="col-12">Mã hiển thị không quá 50 ký tự.</span>
          </small>
        </div>
        <div class="col-12 flex align-items-center">
          <div class="col-3 text-left">STT</div>
          <InputNumber
            v-model="issuePlace.is_order"
            class="col-3 ip-36 px-0"
          />
          <div class="col-1"></div>
          <div class="col-3 text-center">Trạng thái</div>
          <div class="col-2">
            <InputSwitch v-model="issuePlace.status" />
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
<style>
.p-treeselect-panel {
  max-width: 30vw !important;
}
.p-treeselect-panel .p-treeselect-items-wrapper .p-tree {
  max-height: 17vh !important;
}
.p-dropdown-item {
  white-space: normal !important;
}
.word-break {
  word-break: break-all;
}
</style>
