<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import DataTable from "primevue/datatable";
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
  doc_group_name: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
  doc_group_code: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
});
const rules = {
  doc_group_name: {
    required,
    $errors: [
      {
        $property: "doc_group_name",
        $validator: "required",
        $message: "Tên nhóm văn bản không được để trống!",
      },
    ],
  },
  doc_group_code: {
    required,
    $errors: [
      {
        $property: "doc_group_code",
        $validator: "required",
        $message: "Mã nhóm văn bản không được để trống!",
      },
    ],
  },
};
const group = ref({
  doc_group_name: "",
  doc_group_code: "",
  year: 1970,
  current_num: 1,
  status: true,
  is_order: 1,
});
const selectedGroups = ref();
const submitted = ref(false);
const v$ = useVuelidate(rules, group);
const issaveGroup = ref(false);
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
            proc: "ca_groups_count",
            par: [
              {
                par: "user_id",
                va: store.state.user.user_id,
              },
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
      if (data.length > 0) {
        options.value.totalRecords = data[0].totalRecords;
        sttGroup.value = options.value.totalRecords + 1;
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
              proc: "ca_groups_list",
              par: [
                { par: "pageno", va: options.value.PageNo },
                { par: "pagesize", va: options.value.PageSize },
                { par: "search", va: options.value.SearchText },
                { par: "status", va: options.value.status },
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
          controller: "GroupView.vue",
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
  if (store.state.user.is_super == 1) {
    loadDonvi();
  }
};
const loadDonvi = () => {
  axios
    .post(
      baseURL + "/api/DictionaryProc/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_org_list",
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
const treedonvis = ref();

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
    options.value.IsNext = true;
  } else if (event.page < options.value.PageNo) {
    //Trang trước
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
  group.value = {
    doc_group_name: "",
    is_order: sttGroup.value,
    status: true,
  };
  if (store.state.user.is_super) {
    group.value.organization_id = 0;
  } else {
    group.value.organization_id = store.state.user.organization_id;
  }
  issaveGroup.value = false;
  headerDialog.value = str;
  displayBasic.value = true;
  group_id.value = null;
  loadOrganization();
};
const closeDialog = () => {
  group.value = {
    doc_group_name: "",
    is_order: 1,
    status: true,
  };
  displayBasic.value = false;
  loadData(true);
};

//Thêm bản ghi
const gr_dpt = ref();
const saveGroup = (isFormValid) => {
  let formData = new FormData();
  gr_dpt.value = [];
  datalistsD.value.forEach((element) => {
    let jett = {
      doc_group_id: element.data.doc_gr_id,
      department_id:
        element.data.parent_id == null
          ? element.data.parent_id
          : element.data.organization_id,
      current_num: element.data.current_num,
      organization_id:
        store.state.user.is_super == 1
          ? element.data.organization_id
          : store.state.user.organization_id,
    };
    const rechildren = (mm) => {
      if (mm == null) {
        return;
      } else {
        mm.forEach((element) => {
          let jett = {
            doc_group_id: element.data.doc_gr_id,
            department_id:
              element.data.parent_id == null
                ? element.data.parent_id
                : element.data.organization_id,
            current_num: element.data.current_num,
          };
          gr_dpt.value.push(jett);
          rechildren(element.children);
        });
      }
    };
    rechildren(element.children);
    gr_dpt.value.push(jett);
  });

  formData.append("group", JSON.stringify(group.value));
  formData.append("current", JSON.stringify(gr_dpt.value));
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
    method: issaveGroup.value ? "put" : "post",
    url:
      baseURL +
      `/api/ca_groups/${issaveGroup.value ? "Update_groups" : "Add_groups"}`,
    data: formData,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success(
          response.config.method == "post"
            ? "Thêm mới nhóm văn bản thành công!"
            : "Cập nhật nhóm văn bản thành công!",
        );
        loadData(true);
        closeDialog();
      } else {
        let ms = response.data.ms;
        swal.fire({
          title: "Thông báo!",
          html:
            ms.includes("doc_group_code") == true &&
            ms.includes("doc_group_name") == true
              ? "Tên nhóm văn bản không quá 50 ký tự! <br> Ký hiệu không quá 50 ký tự!"
              : ms.includes("doc_group_name")
              ? "Tên nhóm không quá 50 ký tự!"
              : ms.includes("doc_group_code")
              ? "Ký hiệu không quá 50 ký tự!"
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

const sttGroup = ref();
const displayAssets2 = ref(false);
//Sửa bản ghi
const editGroup = (dataGroup) => {
  submitted.value = false;
  group.value = dataGroup;
  headerDialog.value = "Sửa nhóm văn bản";
  issaveGroup.value = true;
  displayBasic.value = true;
  if (store.state.user.is_super) {
    group.value.organization_id = 0;
  } else {
    group.value.organization_id = store.state.user.organization_id;
  }
  group_id.value = dataGroup.doc_group_id;
  loadOrganization();
};
const configGroup = (dataGroup) => {
  submitted.value = false;
  group.value = dataGroup;
  issaveGroup.value = true;
  displayAssets2.value = true;
  group_id.value = dataGroup.doc_group_id;
  loadOrganization();
};
//Xóa bản ghi
const delGroup = (Groups) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá nhóm văn bản này không!",
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
          .delete(baseURL + "/api/ca_groups/Delete_groups", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: Groups != null ? [Groups.doc_group_id] : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá nhóm văn bản thành công!");
              if (
                (options.value.totalRecords - 1) % options.value.PageSize ==
                  0 &&
                options.value.totalRecords - 1 != 0
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
        excelname: "DANH SÁCH NHÓM VĂN BẢN",
        proc: "ca_groups_listexport",
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

  isDynamicSQL.value = event.sortField == null ? false : true;
  loadData(true);
};
const filterSQL = ref([]);
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
    .post(baseURL + "/api/SQL/Filter_CaGroup", data, config)
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
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
//Tìm kiếm
const searchgroups = (event) => {
  options.value.loading = true;
  isDynamicSQL.value = true;
  loadData(true);
};
const onFilter = (event) => {
  filterSQL.value = [];

  for (const [key, value] of Object.entries(event.filters)) {
    if (key != "global") {
      let obj = {
        key: key != "doc_group_name" ? "doc_group_name" : key,
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
const onCheckBox = (value) => {
  let data = {
    IntID: value.doc_group_id,
    TextID: value.doc_group_id + "",
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
      .put(baseURL + "/api/ca_groups/Update_StatusGroups", data, config)
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
  let listId = [];

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
        selectedGroups.value.forEach((item) => {
          listId.push(item.doc_group_id);
        });
        axios
          .delete(baseURL + "/api/ca_groups/Delete_groups", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: listId != null ? listId : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá danh sách thành công!");
              checkDelList.value = false;
              let l = listId.length;

              if (
                (options.value.totalRecords - l) % options.value.PageSize ==
                  0 &&
                options.value.totalRecords - l != 0
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
const searchgroup = () => {
  first.value = 0;
  options.value.PageNo = 0;
  options.value.PageSize = 20;
  options.value.SearchText = "";
  filterPhanloai.value = "";
  filterTrangthai.value = "";
  styleObj.value = "";
  options.value.loading = true;
  isDynamicSQL.value = false;
  selectedGroups.value = [];
  loadData(true);
};

//Filter
const showFilter = ref(false);

const filterPhanloai = ref();
const filterTrangthai = ref();

const reFilterGroup = () => {
  filterPhanloai.value = null;
  filterTrangthai.value = null;
  filterGroups();
  showFilter.value = false;
  styleObj.value = "";
};
const filterGroups = () => {
  styleObj.value = style.value;
  isDynamicSQL.value = true;
  loadData(true);
  showFilter.value = false;
};
watch(selectedGroups, () => {
  if (selectedGroups.value.length > 0) {
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
const trangThai = ref([
  { name: "Hiển thị", code: 1 },
  { name: "Không hiển thị", code: 0 },
]);
const phanLoai = ref([
  { name: "Hệ thống", code: 0 },
  { name: "Đơn vị", code: 1 },
]);
const first = ref(0);

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
      .post(baseURL + "/api/ImportExcel/Import_CaGroup", formData, config)
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
const item = "/Portals/Mau Excel/Mẫu Excel Nhóm văn bản.xlsx";
const displayAssets = ref(false);
const showListAssets = () => {
  displayAssets.value = true;
  loadOrganization();
};
const hideSelectDevice = () => {
  displayAssets.value = false;
};
const onSelectDevice = () => {
  displayAssets.value = false;
};
const group_id = ref();

const datalistsD = ref();
const renderTreeDV1 = (data, id, name, title, org_id) => {
  let arrtreeChils = [];
  let arrChils = [];

  data
    .filter((x) => x.parent_id == null)
    .forEach((m, i) => {
      if (!m.userM) m.userM = null;
      let om = { key: m[id], data: m };

      const rechildren = (mm, pid) => {
        let dts = data.filter((x) => x.parent_id == pid);
        if (dts.length > 0) {
          if (!mm.children) mm.children = [];
          dts.forEach((em) => {
            if (!em.userM) em.userM = null;
            let om1 = { key: em[id], data: em };
            rechildren(om1, em[id]);
            mm.children.push(om1);
          });
        }
      };
      rechildren(om, m[id]);
      arrChils.push(om);
    });
  if (org_id == "") {
    data.forEach((m, i) => {
      let om = { key: m[id], data: m[id], label: m[name] };

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
  } else {
    let rew = Number(org_id);
    data
      .filter((x) => x.parent_id == rew)
      .forEach((m, i) => {
        let om = { key: m[id], data: m[id], label: m[name] };

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
  }
  return { arrChils: arrChils, arrtreeChils: arrtreeChils };
};
const loadOrganization = () => {
  axios
    .post(
      baseURL + "/api/DictionaryProc/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_organization_list_doc_ca_grroup_dept",
            par: [
              { par: "user_id", va: store.state.user.user_id },
              { par: "group_id", va: group_id.value },
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
      if (data.length > 0) {
        let obj = renderTreeDV1(
          data,
          "organization_id",
          "organization_name",
          "đơn vị",
          store.state.user.organization_id,
        );
        datalistsD.value = obj.arrChils;

        // datalistsD.value.forEach((element) => {
        //   expandNode(element);
        // });
      }
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;
    });
};
const expandedKeys = ref();
onMounted(() => {
  loadData(true);
  loadOrganization();
  return { datalistsD, expandedKeys };
});
</script>
<template>
  <div class="main-layout true flex-grow-1 p-2">
    <DataTable
      v-model:first="first"
      :value="datalists"
      :paginator="true"
      :rows="options.PageSize"
      :scrollable="true"
      scrollHeight="flex"
      :loading="options.loading"
      v-model:selection="selectedGroups"
      :lazy="true"
      @page="onPage($event)"
      @filter="onFilter($event)"
      @sort="onSort($event)"
      :totalRecords="options.totalRecords"
      dataKey="doc_group_id"
      :rowHover="true"
      v-model:filters="filters"
      filterDisplay="menu"
      :showGridlines="true"
      filterMode="lenient"
      paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
      :rowsPerPageOptions="[20, 30, 50, 100, 200]"
      responsiveLayout="scroll"
      :globalFiltergroups="['doc_group_name']"
      :removableSort="true"
    >
      <template #header>
        <h3 class="module-title mt-0 ml-1 mb-2">
          <i class="pi pi-book"></i> Danh sách nhóm văn bản ({{
            options.totalRecords
          }})
        </h3>
        <Toolbar class="w-full custoolbar">
          <template #start>
            <span class="p-input-icon-left">
              <i class="pi pi-search" />
              <InputText
                v-model="options.SearchText"
                @keyup.enter="searchgroups"
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
              v-tooltip="'Bộ lọc'"
              :style="[styleObj]"
            />
            <OverlayPanel
              ref="op"
              appendTo="body"
              class="p-0 m-0"
              :showCloseIcon="false"
              id="overlay_panel"
              :style="
                store.state.user.is_super == 1 ? 'width:40vw' : 'width:300px'
              "
            >
              <div class="grid formgrid m-0">
                <div class="flex field col-12 p-0">
                  <div
                    :class="
                      store.state.user.is_super == 1
                        ? 'col-2 text-left pt-2 p-0'
                        : 'col-4 text-left pt-2 p-0'
                    "
                    style="text-align: left"
                  >
                    Phân loại
                  </div>

                  <div
                    :class="store.state.user.is_super == 1 ? 'col-10' : 'col-8'"
                  >
                    <TreeSelect
                      v-model="filterPhanloai"
                      :options="treedonvis"
                      optionLabel="data.organization_name"
                      optionValue="data.organization_id"
                      placeholder="Chọn đơn vị"
                      class="col-12 p-0 m-0 md:col-12"
                      v-if="store.state.user.is_super == 1"
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
                    :class="
                      store.state.user.is_super == 1
                        ? 'col-2 text-left pt-2 p-0'
                        : 'col-4 text-left pt-2 p-0'
                    "
                    style="text-align: center,justify-content:center"
                  >
                    Trạng thái
                  </div>
                  <div
                    :class="store.state.user.is_super == 1 ? 'col-10' : 'col-8'"
                  >
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
                        @click="reFilterGroup"
                        class="p-button-outlined"
                        label="Xóa"
                      ></Button>
                    </template>
                    <template #end>
                      <Button
                        @click="filterGroups"
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
              @click="openBasic('Thêm nhóm văn bản')"
              label="Thêm mới"
              icon="pi pi-plus"
              class="mr-2"
            />
            <Button
              @click="searchgroup"
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
        headerStyle="text-align:center;max-width:75px;height:50px"
        bodyStyle="text-align:center;max-width:75px;;max-height:60px"
        class="align-items-center justify-content-center text-center"
      >
      </Column>

      <Column
        field="doc_group_name"
        header="Nhóm văn bản"
        :sortable="true"
        headerStyle="height:50px"
        bodyStyle="max-height:60px"
      >
      </Column>
      <Column
        field="doc_group_code"
        header="Ký hiệu"
        headerStyle="text-align:center;max-width:120px;height:50px"
        bodyStyle="text-align:center;max-width:120px;;max-height:60px"
        class="align-items-center justify-content-center text-center"
      >
      </Column>
      <Column
        field="year"
        header="Năm"
        headerStyle="text-align:center;max-width:120px;height:50px"
        bodyStyle="text-align:center;max-width:120px;;max-height:60px"
        class="align-items-center justify-content-center text-center"
      >
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
            @click="onCheckBox(data.data)"
          />
        </template>
      </Column>
      <Column
        field="organization_id"
        header="Hệ thống"
        headerStyle="text-align:center;max-width:125px;height:50px"
        bodyStyle="text-align:center;max-width:125px;;max-height:60px"
        class="align-items-center justify-content-center text-center"
      >
        <template #body="data">
          <div v-if="data.data.organization_id == 0">
            <i
              class="pi pi-check text-blue-400"
              style="font-size: 1.5rem"
            ></i>
          </div>
          <div v-else></div>
        </template>
      </Column>
      <Column
        header="Chức năng"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:200px;height:50px"
        bodyStyle="text-align:center;max-width:200px;;max-height:60px"
      >
        <template #body="data">
          <div>
            <Button
              @click="configGroup(data.data)"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-cog"
              v-tooltip="'Thiết lập số hiện tại'"
            />
            <Button
              @click="editGroup(data.data)"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-pencil"
              v-tooltip="'Sửa'"
              v-if="
                store.state.user.is_super == true ||
                store.state.user.user_id == data.data.created_by ||
                (store.state.user.role_id == 'admin' &&
                  store.state.user.organization_id == data.data.organization_id)
              "
            >
            </Button>
            <Button
              @click="delGroup(data.data, true)"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-trash"
              v-tooltip="'Xóa'"
              v-if="
                store.state.user.is_super == true ||
                store.state.user.user_id == data.data.created_by ||
                (store.state.user.role_id == 'admin' &&
                  store.state.user.organization_id == data.data.organization_id)
              "
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
    :style="{ width: '50vw' }"
    :closable="false"
  >
    <form>
      <div class="grid formgrid m-2">
        <div
          style="display: flex"
          class="group col-12 md:col-12"
        >
          <div class="col-12 p-0">
            <div class="col-12 p-0 flex my-2">
              <div class="col-2 text-left p-0 pb-2 line-height-4">
                Tên nhóm <span class="redsao">(*)</span>
              </div>
              <InputText
                v-model="group.doc_group_name"
                spellcheck="false"
                class="col-10 p-0 m-0 ip36 px-2"
              />
            </div>
            <div class="col-12 flex p-0">
              <div class="col-2"></div>
              <small
                v-if="
                  (v$.doc_group_name.$invalid && submitted) ||
                  v$.doc_group_name.$pending.$response
                "
                class="col-10 p-error p-0"
              >
                <span class="col-12 p-0">{{
                  v$.doc_group_name.required.$message
                    .replace("Value", "Tên nhóm văn bản")
                    .replace("is required", "không được để trống")
                }}</span>
              </small>
            </div>
            <div class="col-12 p-0 flex my-2">
              <div class="col-2 text-left p-0 pb-2 line-height-4">
                Ký hiệu <span class="redsao">(*)</span>
              </div>
              <InputText
                v-model="group.doc_group_code"
                spellcheck="false"
                class="col-10 p-0 m-0 ip36 px-2"
              />
            </div>
            <div class="col-12 flex p-0">
              <div class="col-2"></div>
              <small
                v-if="
                  (v$.doc_group_code.$invalid && submitted) ||
                  v$.doc_group_code.$pending.$response
                "
                class="col-10 p-error p-0"
              >
                <span class="col-12 p-0">{{
                  v$.doc_group_code.required.$message
                    .replace("Value", "Ký hiệu")
                    .replace("is required", "không được để trống")
                }}</span>
              </small>
            </div>
            <div class="col-12 p-0 flex">
              <label class="line-height-4 col-2 p-0">Năm</label>
              <InputNumber
                :useGrouping="false"
                v-model="group.year"
                spellcheck="false"
                class="col-10 p-0 ip36"
              />
            </div>
            <div class="col-12 p-0 my-3 flex">
              <div class="col-6 flex p-0 format-left">
                <div class="col-4 p-0 pl-0 line-height-4 format-left">
                  Giấy mời
                </div>
                <InputSwitch
                  :trueValue="0"
                  :falseValue="null"
                  v-model="group.type_group"
                  class="col-6 p-0 ip36"
                />
              </div>

              <div class="col-6 flex p-0 format-left">
                <div class="col-4 p-0 pl-0 line-height-4 px-2 format-left">
                  GQ BHXH, BHYT
                </div>
                <InputSwitch
                  :trueValue="1"
                  :falseValue="null"
                  v-model="group.type_group"
                  class="col-6 p-0 ip36"
                />
              </div>
            </div>
            <div class="col-12 p-0 my-3 flex">
              <div class="col-6 flex p-0 format-left">
                <div class="col-4 p-0 line-height-4">STT</div>
                <InputNumber
                  v-model="group.is_order"
                  class="col-8 p-0 ip36"
                />
              </div>
              <div class="col-6 flex p-0 format-left">
                <div class="col-4 p-0 line-height-4 px-2 format-left">
                  Trạng thái
                </div>
                <InputSwitch
                  v-model="group.status"
                  class="col-6 p-0 ip36"
                />
              </div>
            </div>
            <div class="col-12 p-0 my-3 flex">
              <Button
                icon="pi pi-plus"
                label="Thiết lập số văn bản hiện tại theo phòng ban"
                @click="showListAssets()"
              />
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
        @click="saveGroup(!v$.$invalid)"
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

  <Dialog
    header="Cập nhật số hiện tại nhóm văn bản theo phòng ban"
    v-model:visible="displayAssets"
    :maximizable="true"
    :style="{ width: '50vw' }"
  >
    <div>
      <div
        class="true flex-grow-1 p-2"
        id="scrollTop"
      >
        <div class="grid p-0">
          <div class="col-12 field format-center">
            <TreeTable
              :expandedKeys="expandedKeys"
              :value="datalistsD"
            >
              <Column
                class="w-7"
                field="organization_name"
                :expander="true"
              >
                <template #header>
                  <div class="w-full format-center">
                    <i class="pi pi-building pr-2"></i> Phòng ban
                  </div>
                </template>
              </Column>
              <Column class="w-5">
                <template #header>
                  <div class="w-full format-center">
                    <i class="pi pi-ticket pr-2"></i>Số hiện tại
                  </div>
                </template>
                <template #body="data">
                  <div class="w-full flex">
                    <InputNumber
                      v-model="data.node.data.current_num"
                      spellcheck="false"
                      class="col-10 p-0"
                      mode="decimal"
                      showButtons
                      :min="0"
                      :useGrouping="false"
                      autocomplete="off"
                    />
                  </div>
                </template>
              </Column>
            </TreeTable>
          </div>
        </div>
      </div>

      <div
        class="p-0"
        id="scrollDM"
      >
        <Toolbar class="p-2 surface-0 border-none">
          <template #end>
            <Button
              @click="hideSelectDevice()"
              label="Hủy"
              icon="pi pi-times"
              class="mr-2 p-button-outlined"
            />
            <Button
              @click="onSelectDevice()"
              label="Chọn"
              icon="pi pi-check"
              autofocus
            />
          </template>
        </Toolbar>
      </div>
    </div>
  </Dialog>
  <Dialog
    header="Cập nhật số hiện tại nhóm văn bản theo phòng ban"
    v-model:visible="displayAssets2"
    :maximizable="true"
    :style="{ width: '50vw' }"
  >
    <div>
      <div
        class="true flex-grow-1 p-2"
        id="scrollTop"
      >
        <div class="grid p-0">
          <div class="col-12 field format-center">
            <TreeTable
              :value="datalistsD"
              :scrollable="true"
              :rowHover="true"
              :expandedKeys="expandedKeys"
              :lazy="true"
              dataKey="organization_id"
              filterMode="strict"
              scrollHeight="flex"
              filterDisplay="menu"
              class="d-lang-table"
            >
              <Column
                class="w-7"
                field="organization_name"
                :expander="true"
              >
                <template #header>
                  <div class="w-full format-center">
                    <i class="pi pi-building pr-2"></i> Phòng ban
                  </div>
                </template>
              </Column>
              <Column class="w-5">
                <template #header>
                  <div class="w-full format-center">
                    <i class="pi pi-ticket pr-2"></i>Số hiện tại
                  </div>
                </template>
                <template #body="data">
                  <div class="w-full flex">
                    <InputNumber
                      v-model="data.node.data.current_num"
                      spellcheck="false"
                      class="col-10 p-0"
                      mode="decimal"
                      showButtons
                      :min="0"
                      :useGrouping="false"
                      autocomplete="off"
                    />
                  </div>
                </template>
              </Column>
            </TreeTable>
          </div>
        </div>
      </div>

      <div
        class="p-0"
        id="scrollDM"
      >
        <Toolbar class="p-2 surface-0 border-none">
          <template #end>
            <Button
              @click="displayAssets2 = false"
              label="Hủy"
              icon="pi pi-times"
              class="mr-2 p-button-outlined"
            />
            <Button
              @click="saveGroup(true), (displayAssets2 = false)"
              label="Chọn"
              icon="pi pi-check"
              autofocus
            />
          </template>
        </Toolbar>
      </div>
    </div>
  </Dialog>
</template>

<style scoped>
.format-left {
  display: flex;
  justify-content: flex-start;
  align-items: center;
  vertical-align: middle;
  text-align: left;
}
</style>
<style>
.p-treeselect-panel {
  max-width: 30vw !important;
}
.p-treeselect-items-wrapper .p-tree {
  max-height: 17vh !important;
}
.p-dropdown-item {
  white-space: normal !important;
}
</style>
