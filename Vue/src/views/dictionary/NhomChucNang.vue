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
const router = inject("router");
const isDynamicSQL = ref(false);
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const filters = ref({
  global: { value: null, matchMode: FilterMatchMode.CONTAINS },
  role_group_name: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
  email_name: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
  full_name: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
});
const rules = {
  role_group_name: {
    required,
    $errors: [
      {
        $property: "role_group_name",
        $validator: "required",
        $message: "Tên nhóm chức năng không được để trống!",
      },
    ],
  },
};

const rulesEmail = {
  user_id: {
    required,
  },
};
const EmailGroup = ref({
  role_group_name: "",
  is_order: null,
  organization_id: null,
  status: null,
  is_type: null,
});
const Email = ref({
  user_id: "",
  is_order: null,
  organization_id: null,
});
const selectedEmailGroups = ref();
const selectedEmail = ref();
const submitted = ref(false);
const v$ = useVuelidate(rules, EmailGroup);
const validateEmail = useVuelidate(rulesEmail, Email);
const issaveEmailGroup = ref(false);
const issaveEmail = ref(false);
const datalists = ref();
const toast = useToast();
const basedomainURL = fileURL;
const checkDelList = ref(false);
const checkDelListEmail = ref(false);
const options = ref({
  IsNext: true,
  sort: "is_order desc",
  SearchText: null,
  PageNo: 0,
  PageSize: 20,
  loading: true,
  totalRecords: null,
  Status: null,
});
const optionsEmail = ref({
  IsNext: true,
  sort: "is_order desc",
  SearchText: "",
  PageNo: 0,
  PageSize: 20,
  loading: true,
  totalRecords: 0,
  Status: null,
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
            proc: "doc_ca_role_groups_count",
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
        sttEmailGroup.value = options.value.totalRecords + 1;
      }
    })
    .catch((error) => {
      addLog({
        title: "Lỗi Console loadCount",
        controller: "Email.vue",
        logcontent: error.message,
        loai: 2,
      });
    });
};
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
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    });
};
const treedonvis = ref();
//Lấy dữ liệu ngôn ngữ
const loadData = (rf) => {
  if (store.state.user.is_super == true) {
    loadDonvi();
  }
  if (rf) {
    if (isDynamicSQL.value) {
      // loadDataSQL();
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
              proc: "doc_ca_role_groups_list",
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

        addLog({
          title: "Lỗi Console loadData",
          controller: "Email.vue",
          logcontent: error.message,
          loai: 2,
        });
        if (error && error.status === 401) {
          swal.fire({
            title: "Thông báo!",
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
  if (!isDynamicSQL.value) {
    loadData(true);
  } else {
    // loadDataSQL();
  }
};
const isNextEmailPage = ref(false);
const onPageEmail = (event) => {
  if (event.rows != options.value.PageSize) {
    optionsEmail.value.PageSize = event.rows;
  }

  isNextEmailPage.value = true;
  if (event.page == 0) {
    //Trang đầu
    optionsEmail.value.id = null;
    optionsEmail.value.IsNext = true;
  } else if (event.page > optionsEmail.value.PageNo + 1) {
    //Trang cuối
    optionsEmail.value.id = -1;
    optionsEmail.value.IsNext = false;
  } else if (event.page > optionsEmail.value.PageNo) {
    //Trang sau
    optionsEmail.value.id =
      datalists.value[datalists.value.length - 1].place_id;
    optionsEmail.value.IsNext = true;
  } else if (event.page < optionsEmail.value.PageNo) {
    //Trang trước
    optionsEmail.value.id = datalists.value[0].place_id;
    optionsEmail.value.IsNext = false;
  }
  optionsEmail.value.PageNo = event.page;
  showEmails(options.value.groupid);
};
//Hiển thị dialog
const headerDialog = ref();
const headerAddEmail = ref();
const displayBasic = ref(false);
const displayEmail = ref(false);
const addEmail = (str) => {
  submitted.value = false;
  checkUser.value = [];
  issaveEmail.value = false;
  headerAddEmail.value = str;
  displayEmail.value = true;
};
const openBasic = (str) => {
  submitted.value = false;
  EmailGroup.value = {
    role_group_name: "",
    is_order: sttEmailGroup.value,
    organization_id: null,
    status: true,
  };
  EmailGroup.value.is_type = 0;
  EmailGroup.value.organization_id = store.state.user.organization_id;
  issaveEmailGroup.value = false;
  headerDialog.value = str;
  displayBasic.value = true;
};
const closeDialog = () => {
  EmailGroup.value = {
    role_group_name: "",
    is_order: 1,
    status: true,
  };
  displayBasic.value = false;
  loadData(true);
};
const closeDialogEmail = () => {
  Email.value = {};
  searchUser.value = "";
  displayEmail.value = false;
  showEmails(options.value.groupid);
};
//Thêm bản ghi

const saveEmailGroup = (isFormValid) => {
  submitted.value = true;

  if (EmailGroup.value.is_order == null)
    EmailGroup.value.is_order == sttEmailGroup.value;
  if (EmailGroup.value.status == null) EmailGroup.value.status == false;

  if (!isFormValid) {
    return;
  }
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  if (!issaveEmailGroup.value) {
    axios
      .post(
        baseURL + "/api/role_groups/Add_role_groups",
        EmailGroup.value,
        config,
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Thêm nhóm chức năng thành công!");
          closeDialog();
          loadData(true);
        } else {
          let ms = response.data.ms;
          swal.fire({
            title: "Thông báo!",
            text:
              ms.includes("role_group_name") == true
                ? "Tên nhóm chức năng không quá 500 ký tự!"
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
          title: "Thông báo!",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
      });
  } else {
    axios
      .put(
        baseURL + "/api/role_groups/Update_role_groups",
        EmailGroup.value,
        config,
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa nhóm chức năng thành công!");
          loadData(true);
          closeDialog();
        } else {
          let ms = response.data.ms;
          swal.fire({
            title: "Thông báo!",
            text:
              ms.includes("role_group_name") == true
                ? "Tên nhóm chức năng không quá 500 ký tự!"
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
          title: "Thông báo!",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
      });
  }
};

const saveEmail = (isFormValid) => {
  submitted.value = true;

  if (isFormValid.length == 0 || isFormValid == null || isFormValid == []) {
    return;
  }
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  isFormValid.forEach((x, i) => {
    let user = {
      role_group_id: emailGroupID.value,
      user_id: x,
      is_order: i,
      organization_id: store.state.user.organization_id,
    };
    axios
      .post(
        baseURL + "/api/role_group_users/Add_role_groups_user",
        user,
        config,
      )
      .then((response) => {})
      .catch((error) => {
        swal.close();
        swal.fire({
          title: "Thông báo!",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!" + error,
          icon: "error",
          confirmButtonText: "OK",
        });
      });
  });
  swal.close();
  toast.success("Thêm người dùng thành công!");
  refreshEmail(emailGroupID.value);
  closeDialogEmail();
};
const sttEmailGroup = ref();
const sttEmail = ref();
//Sửa bản ghi
const editEmailGroup = (dataEmailGroup) => {
  submitted.value = false;
  EmailGroup.value = dataEmailGroup;
  headerDialog.value = "Sửa nhóm chức năng";
  issaveEmailGroup.value = true;
  displayBasic.value = true;
  {
    EmailGroup.value.organization_id = store.state.user.organization_id;
  }
};
const editEmail = (dataEmail) => {
  submitted.value = false;
  Email.value = dataEmail;
  headerAddEmail.value = "Sửa Email";
  issaveEmail.value = true;
  displayEmail.value = true;
  {
    Email.value.organization_id = store.state.user.organization_id;
  }
};
//Xóa bản ghi
const delEmailGroup = (EmailGroup) => {
  if (EmailGroup != null) {
    deleteEmailOnDeleteGroup(EmailGroup.role_group_id);
  }
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá nhóm chức năng này không!",
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
          .delete(baseURL + "/api/role_groups/Delete_Role_groups", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: EmailGroup != null ? [EmailGroup.role_group_id] : -1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá nhóm chức năng thành công!");
              if (
                (options.value.totalRecords - EmailGroup.length) % 2 == 0 &&
                options.value.PageNo > 0
              ) {
                options.value.PageNo = options.value.PageNo - 1;
              }
              loadData(true);
            } else {
              swal.fire({
                title: "Thông báo!",
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
                title: "Thông báo!",
                text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
                icon: "error",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
};

const delEmail = (Email) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá người dùng này không!",
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
          .delete(baseURL + "/api/role_group_users/Delete_Users", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: Email != null ? [Email.role_group_user_id] : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá người dùng thành công!");
              if (
                (optionsEmail.value.totalRecords - Email.length) % 2 == 0 &&
                optionsEmail.value.PageNo > 0
              ) {
                optionsEmail.value.PageNo = optionsEmail.value.PageNo - 1;
              }
              showEmails(emailGroupID.value);
            } else {
              swal.fire({
                title: "Thông báo!",
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
                title: "Thông báo!",
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
      if (isShowEmail.value) {
        exportDataEmail("ExportExcel");
      } else exportData("ExportExcel");
    },
  },
  {
    label: "Nhập Excel",
    icon: "pi pi-file-excel",
    command: (event) => {
      ImportExcel("ExportExcel");
    },
  },
]);

const toggleExport = (event) => {
  menuButs.value.toggle(event);
};
const excel_name = ref();
const exportDataEmail = (method) => {
  excel_name.value = "DANH SÁCH EMAIL" + " - " + EmailGrName.value;
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
        excelname: excel_name.value,
        proc: "ca_email_listexport",
        par: [
          { par: "search", va: optionsEmail.value.SearchText },
          { par: "status", va: filterTrangthai.value },
          { par: "em_gr_id", va: emailGroupID.value },
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
          window.open(baseURL + pathFile);
        }
      } else {
        swal.fire({
          title: "Thông báo!",
          text: response.data.ms,
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    })
    .catch((error) => {
      if (error.status === 401) {
        swal.fire({
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const exportData = (method) => {
  if (filterPhanloai_EG.value == undefined || filterPhanloai_EG.value == null) {
    options.value.filter_Org = 1;
  } else if (filterPhanloai_EG.value == 0) {
    options.value.filter_Org = 3;
  } else options.value.filter_Org = 2;
  filterTrangthai_EG.value =
    filterPhanloai_EG.value != null
      ? filterTrangthai_EG.value == 1
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
        excelname: "DANH SÁCH nhóm chức năng",
        proc: "doc_ca_email_groups_listexport",
        par: [
          { par: "search", va: options.value.SearchText },
          { par: "status", va: filterTrangthai_EG.value },
          { par: "user_id", va: store.state.user.user_id },
          { par: "s_org", va: filterPhanloai_EG.value },
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
          title: "Thông báo!",
          text: response.data.ms,
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    })
    .catch((error) => {
      if (error.status === 401) {
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
  options.value.sort =
    event.sortField + (event.sortOrder == 1 ? " ASC" : " DESC");
  if (event.sortField == "STT") {
    options.value.sort = "is_order" + (event.sortOrder == 1 ? " ASC" : " DESC");
  }
  options.value.PageNo = 0;
  isDynamicSQL.value = true;
  // loadDataSQL();
};

const onSortEmail = (event) => {
  optionsEmail.value.sort =
    event.sortField + (event.sortOrder == 1 ? " ASC" : " DESC");
  if (event.sortField == "STT") {
    optionsEmail.value.sort =
      "is_order" + (event.sortOrder == 1 ? " ASC" : " DESC");
  }
  optionsEmail.value.PageNo = 0;
  // loadEmailSQL();
};

const isFirst = ref(true);
const loadDataSQL = () => {
  let fpl;
  if (
    filterPhanloai_EG.value != undefined &&
    store.state.user.is_super &&
    filterPhanloai_EG.value != null
  ) {
    fpl = parseInt(Object.keys(filterPhanloai_EG.value)[0]);
  } else {
    fpl =
      filterPhanloai_EG.value != undefined && filterPhanloai_EG.value != null
        ? store.state.user.is_super
          ? filterPhanloai_EG.value
          : filterPhanloai_EG.value == 0
          ? 0
          : store.state.user.organization_id
        : null;
  }
  datalists.value = [];
  let data = {
    sqlS: filterTrangthai_EG.value != null ? filterTrangthai_EG.value : null,
    sqlF: fpl,
    sqlO: options.value.sort,
    Search: options.value.SearchText,
    PageNo: options.value.PageNo,
    PageSize: options.value.PageSize,
  };

  options.value.loading = true;
  axios
    .post(baseURL + "/api/SQL/Filter_EmailGroup", data, config)
    .then((response) => {
      let dt = JSON.parse(response.data.data);
      let data = dt[0];
      if (data.length > 0) {
        data.forEach((element, i) => {
          element.STT = options.value.PageNo * options.value.PageSize + i + 1;
          if (options.value.sort == "is_order DESC") {
            {
              element.STT =
                options.value.totalRecords -
                options.value.PageNo * options.value.PageSize -
                i;
            }
          }
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
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};

const loadEmailSQL = () => {
  let data = {
    id: emailGroupID.value,
    sqlS: filterTrangthai.value != null ? filterTrangthai.value : null,

    sqlO: optionsEmail.value.sort,
    Search: optionsEmail.value.SearchText,
    PageNo: optionsEmail.value.PageNo,
    PageSize: optionsEmail.value.PageSize,
  };

  optionsEmail.value.loading = true;
  axios
    .post(baseURL + "/api/SQL/Filter_Email", data, config)
    .then((response) => {
      let dt = JSON.parse(response.data.data);
      let data = dt[0];
      if (data.length > 0) {
        data.forEach((element, i) => {
          element.STT =
            optionsEmail.value.PageNo * optionsEmail.value.PageSize + i + 1;
          if (options.value.sort == "is_order DESC") {
            {
              element.STT =
                optionsEmail.value.totalRecords -
                optionsEmail.value.PageNo * optionsEmail.value.PageSize -
                i;
            }
          }
        });
        emailList.value = data;
      } else {
        emailList.value = [];
      }
      optionsEmail.value.loading = false;
      //Show Count nếu có
      if (dt.length == 2) {
        optionsEmail.value.totalRecords = dt[1][0].totalRecords;
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
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
//Tìm kiếm
const searchEmailGroups = () => {
  isDynamicSQL.value = true;
  loadData(true);
};
const searchEmail = (event) => {
  // loadEmailSQL();
};

//Checkbox
const onCheckBox = (value) => {
  let data = {
    IntID: value.role_group_id,
    TextID: value.role_group_id + "",
    IntTrangthai: 1,
    BitTrangthai: value.status,
  };
  if (
    store.state.user.is_super == true ||
    store.state.user.user_id == value.created_by ||
    store.state.user.role_id == "admin"
  ) {
    axios
      .put(baseURL + "/api/role_groups/Update_StatusGroup_Role", data, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Cập nhật trạng thái nhóm chức năng thành công!");
          loadData(true);
          closeDialog();
        } else {
          swal.fire({
            title: "Thông báo!",
            text: response.data.ms,
            icon: "error",
            confirmButtonText: "OK",
          });
        }
      })
      .catch((error) => {
        swal.close();
        swal.fire({
          title: "Thông báo!",
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

const onCheckBoxEmail = (value) => {
  let data = {
    IntID: value.email_id,
    TextID: value.email_id + "",
    IntTrangthai: 1,
    BitTrangthai: value.status,
  };
  if (
    store.state.user.is_super == true ||
    store.state.user.user_id == value.created_by ||
    store.state.user.role_id == "admin"
  ) {
    axios
      .put(baseURL + "/api/ca_emails/Update_StatusEmail", data, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa email thành công!");
          refreshEmail(emailGroupID.value);
          closeDialogEmail();
        } else {
          swal.fire({
            title: "Thông báo!",
            text: response.data.ms,
            icon: "error",
            confirmButtonText: "OK",
          });
        }
      })
      .catch((error) => {
        swal.close();
        swal.fire({
          title: "Thông báo!",
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
    showEmails(emailGroupID.value);
  }
};
//Xóa nhiều
const deleteEmailList = (event) => {
  let listId = new Array(selectedEmail.value.length);
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
        selectedEmail.value.forEach((item) => {
          listId.push(item.role_group_user_id);
        });
        axios
          .delete(baseURL + "/api/role_group_users/Delete_Users", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: listId != null ? listId : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá danh sách thành công!");
              checkDelListEmail.value = false;
              if (
                (optionsEmail.value.totalRecords - listId.length) % 2 == 0 &&
                optionsEmail.value.PageNo > 0
              ) {
                optionsEmail.value.PageNo = optionsEmail.value.PageNo - 1;
              }
              loadData(true);
              showEmails(emailGroupID.value);
              refreshEmail(emailGroupID.value);
            } else {
              swal.fire({
                title: "Thông báo!",
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
                title: "Thông báo!",
                text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
                icon: "error",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
};

const deleteList = () => {
  let listId = new Array(selectedEmailGroups.value.length);
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
        selectedEmailGroups.value.forEach((item) => {
          listId.push(item.role_group_id);
        });
        listId.forEach((element) => {
          deleteEmailOnDeleteGroup(element);
        });
        axios
          .delete(baseURL + "/api/role_groups/Delete_Role_groups", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: listId != null ? listId : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá danh sách nhóm chức năng thành công!");
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
                title: "Thông báo!",
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
                title: "Thông báo!",
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
const showFilter = ref(false);
const toggleFilter = (event) => {
  op.value.toggle(event);
};

const isType = ref([
  { name: "Nhóm nhận phân phát", code: 0 },
  { name: "Nhóm xem để biết", code: 1 },
  { name: "Nhóm chung", code: 2 },
]);
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
const filterPhanloai_EG = ref();
const filterTrangthai_EG = ref();
const reFilterNews = () => {
  filterPhanloai_EG.value = null;
  filterTrangthai_EG.value = null;
  filterGroupEmails();
  showFilter.value = false;
  styleObj.value = "";
};
const reFilterEmail = () => {
  filterPhanloai.value = null;
  filterTrangthai.value = null;
  // loadEmailSQL();
  showFilter.value = false;
  styleObj.value = "";
};
const filterGroupEmails = () => {
  //loadDataSQL();
  styleObj.value = style.value;
};
const styleObj1 = ref();
const filterEmails = () => {
  styleObj1.value = style.value;
  //loadEmailSQL();
  showFilter.value = false;
};
const isShowEmail = ref(false);
const emailList = ref();
const emailGroupID = ref();
const refreshEmail = (value) => {
  selectedEmail.value = [];
  filterPhanloai.value = "";
  optionsEmail.value = {
    PageNo: 0,
    PageSize: 20,
  };
  filterTrangthai.value = "";
  styleObj1.value = "";
  optionsEmail.value.SearchText = "";
  firstEmail.value = 0;
  loadEMail();
};
const showEmails = (value) => {
  emailList.value = [];
  EmailGrName.value = !value.role_group_name
    ? EmailGrName.value
    : value.role_group_name;
  options.value.loading = true;
  options.value.groupid = !value.role_group_id ? value : value.role_group_id;
  loadEMail();
  isShowEmail.value = true;
  //  document.getElementById(value).style.backgroundColor = "red";
  emailGroupID.value = !value.role_group_id ? value : value.role_group_id;
  if (isShowEmail.value == true) options.value.loading = false;
  else options.value.loading = true;
  LoadUser();
};
const loadEMail = () => {
  optionsEmail.value.loading = true;
  axios
    .post(
      baseURL + "/api/DictionaryProc/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "role_group_user_count",
            par: [{ par: "id", va: options.value.groupid }],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      optionsEmail.value.totalRecords = data[0].totalRecords;
      sttEmail.value = data[0].totalRecords + 1;
    });
  axios
    .post(
      baseURL + "/api/DictionaryProc/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "role_group_user_list",
            par: [
              { par: "group_id", va: options.value.groupid },
              { par: "pageno", va: optionsEmail.value.PageNo },
              { par: "pagesize", va: optionsEmail.value.PageSize },
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
        data.forEach((element, i) => {
          element.STT =
            optionsEmail.value.PageNo * options.value.PageSize + i + 1;
        });
      }
      emailList.value = data;
      isShowEmail.value = true;
      optionsEmail.value.loading = false;
      options.value.loading = false;
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      optionsEmail.value.loading = false;
      addLog({
        title: "Lỗi Console loadData",
        controller: "Email.vue",
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
watch(selectedEmailGroups, () => {
  if (selectedEmailGroups.value.length > 0) {
    checkDelList.value = true;
  } else {
    checkDelList.value = false;
  }
});
watch(selectedEmail, () => {
  if (selectedEmail.value.length > 0) {
    checkDelListEmail.value = true;
  } else {
    checkDelListEmail.value = false;
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
const EmailGrName = ref();
const hideall = () => {
  displayEmail.value = false;
  isShowEmail.value = false;
  loadData(true);
};

const prevPage = () => {
  isShowEmail.value = false;
  selectedEmail.value = [];
};
const first = ref(0);
const firstEmail = ref(0);
const refresh = () => {
  options.value = {
    PageNo: 0,
    PageSize: 20,
  };
  options.value.loading = true;
  filterPhanloai.value = "";
  filterTrangthai.value = "";
  isDynamicSQL.value = false;
  selectedEmailGroups.value = [];
  loadData(true);
  styleObj.value = "";
  options.value.SearchText = "";
  first.value = 0;
};
const deleteEmailOnDeleteGroup = (id) => {
  axios
    .post(
      baseURL + "/api/DictionaryProc/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "doc_ca_role_group",
            par: [{ par: "id", va: id }],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {})
    .catch((error) => {});
};
const width = ref(window.screen.width);
const height = ref(window.screen.height);
//upload section
// -------------------Email----------------
const item = "/Portals/Mau Excel/Mẫu Excel nhóm chức năng.xlsx";
const item2 = "/Portals/Mau Excel/Mẫu Excel Email.xlsx";
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
    if (isShowEmail.value == true) {
      formData.append("id", JSON.stringify(options.value.groupid));
    }
    axios
      .post(
        !isShowEmail.value
          ? baseURL + "/api/ImportExcel/Import_EmailGroup"
          : baseURL + "/api/ImportExcel/Import_Email",
        formData,
        config,
      )
      .then((response) => {
        if (response.data.err != "1") {
          toast.success("Nhập dữ liệu thành công");
          isDynamicSQL.value = false;
          if (!isShowEmail.value) loadData(true);
          else {
            loadEMail();
            loadData(true);
          }
        } else {
          swal.fire({
            title: "Thông báo",
            html: "Vui lòng kiểm tra lại:<br>" + response.data.ms,
            icon: "error",
            confirmButtonText: "OK",
          });
        }
      })
      .catch((error) => {
        swal.close();
        swal.fire({
          title: "Thông báo",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!" + error,
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
const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);
const ListUser = ref();
const searchUser = ref();
const checkUser = ref();

const LoadUser = () => {
  ListUser.value = [];
  axios
    .post(
      baseURL + "/api/DictionaryProc/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "role_group_getUsertoList",
            par: [
              { par: "user_id", va: store.state.user.user_id },
              { par: "search", va: searchUser.value },
              { par: "gr_id", va: emailGroupID.value },
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
      ListUser.value = data;
      options.value.loading = false;
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;

      addLog({
        title: "Lỗi Console loadData",
        controller: "Email.vue",
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
onMounted(() => {
  loadData(true);
  return {};
});
</script>
<template>
  <div class="main-layout true flex-grow-1 p-2">
    <DataTable
      v-model:first="first"
      :lazy="true"
      @page="onPage($event)"
      @filter="onFilter($event)"
      @sort="onSort($event)"
      :value="datalists"
      :loading="options.loading"
      :paginator="true"
      :rows="options.PageSize"
      :totalRecords="options.totalRecords"
      dataKey=""
      :rowHover="true"
      v-model:filters="filters"
      filterDisplay="menu"
      :showGridlines="true"
      filterMode="lenient"
      paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
      :rowsPerPageOptions="[20, 30, 50, 100, 200]"
      responsiveLayout="scroll"
      :scrollable="true"
      scrollHeight="flex"
      v-model:selection="selectedEmailGroups"
      :class="'w-full'"
    >
      <template #header>
        <h3 class="module-title mt-0 ml-1 mb-2">
          <i class="pi pi-at"> </i> Danh sách nhóm chức năng ({{
            options.totalRecords
          }})
        </h3>

        <Toolbar class="w-full custoolbar">
          <!-- <template #start>
            <span class="p-input-icon-left">
              <i class="pi pi-search" />
              <InputText
                v-model="options.SearchText"
                @keyup.enter="searchEmailGroups"
                type="text"
                spellcheck="false"
                placeholder="Tìm kiếm nhóm chức năng"
            /></span> -->

          <!-- <Button
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
                store.state.user.is_super == 1 ? 'width:40vw' : 'width:400px'
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
                    :class=" 'col-8'"
                  >
                    <TreeSelect
                      v-model="filterPhanloai_EG"
                      :options="treedonvis"
                      optionLabel="data.organization_name"
                      optionValue="data.organization_id"
                      placeholder="Chọn đơn vị"
                      class="col-12 p-0 m-0 md:col-12"
                      v-if="store.state.user.is_super == 1"
                    />
                    <Dropdown
                      class="col-12 p-0 m-0"
                      v-model="filterPhanloai_EG"
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
                    :class=" 'col-8'"
                  >
                    <Dropdown
                      class="col-12 p-0 m-0"
                      v-model="filterTrangthai_EG"
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
                        @click="reFilterNews"
                        class="p-button-outlined"
                        label="Xóa"
                      ></Button>
                    </template>
                    <template #end>
                      <Button @click="filterGroupEmails" label="Lọc"></Button>
                    </template>
                  </Toolbar>
                </div>
              </div>
            </OverlayPanel>
          </template> -->

          <template #end>
            <Button
              v-if="checkDelList"
              @click="deleteList()"
              label="Xóa"
              icon="pi pi-trash"
              class="mr-2 p-button-danger"
            />
            <Button
              @click="openBasic('Thêm mới nhóm chức năng')"
              label="Thêm mới"
              icon="pi pi-plus"
              class="mr-2"
            />
            <Button
              @click="refresh()"
              class="mr-2 p-button-outlined p-button-secondary"
              icon="pi pi-refresh"
              v-tooltip="'Tải lại'"
            />

            <!-- <Button
              label="Tiện ích"
              icon="pi pi-file-excel"
              class="mr-2 p-button-outlined p-button-secondary"
              @click="toggleExport"
              aria-haspopup="true"
              aria-controls="overlay_Export"
            /> -->
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
        headerStyle="text-align:center;max-width:5rem;height:3.125rem"
        bodyStyle="text-align:center;max-width:5rem; "
        class="align-items-center justify-content-center text-center"
        v-if="store.state.user.is_super == true || store.state.user.is_admin"
      >
      </Column>
      <Column
        field="STT"
        header="STT"
        headerStyle="text-align:center;max-width:8rem;height:3.125rem"
        bodyStyle="text-align:center;max-width:8rem; "
        class="align-items-center justify-content-center text-center"
      >
      </Column>
      <Column
        field="role_group_name"
        header="Tên nhóm chức năng"
        headerStyle="height:3.125rem"
        headerClass=" align-items-center justify-content-center text-center"
        class=""
      >
      </Column>
      <Column
        field="is_type"
        header="Loại nhóm"
        headerStyle="text-align:center;max-width:20rem;height:3.125rem"
        bodyStyle="text-align:center;max-width:20rem; "
        class="align-items-center justify-content-center text-center"
        ><template #body="data">
          <span v-if="data.data.is_type == 0">Nhóm nhận phân phát</span>
          <span v-if="data.data.is_type == 1">Nhóm xem để biết</span>
          <span v-if="data.data.is_type == 2">Nhóm chung</span>
        </template>
      </Column>
      <Column
        field="email_count"
        header="Thành viên"
        headerStyle="text-align:center;max-width:150px;height:3.125rem"
        bodyStyle="text-align:center;max-width:150px; "
        class="align-items-center justify-content-center text-center"
      >
        <template #body="data">
          <div>
            <Button
              :id="data.data.role_group_id"
              class="p-button-rounded p-button-secondary"
              @click="showEmails(data.data)"
            >
              {{ data.data.email_count }}</Button
            >
          </div>
        </template>
      </Column>
      <Column
        field="status"
        header="Hiển thị"
        headerStyle="text-align:center;max-width:10rem;height:3.125rem"
        bodyStyle="text-align:center;max-width:10rem; "
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
      <!-- <Column
        v-if="!isShowEmail"
        field="organization_id"
        header="Hệ thống"
        headerStyle="text-align:center;max-width:7.5rem;height:3.125rem"
        bodyStyle="text-align:center;max-width:7.5rem; "
        class="align-items-center justify-content-center text-center"
      >
        <template #body="data">
          <div v-if="data.data.organization_id == 0">
            <i class="pi pi-check text-blue-400" style="font-size: 1.5rem"></i>
          </div>
          <div v-else></div>
        </template>
      </Column> -->
      <Column
        header="Chức năng"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:15rem;height:3.125rem;min-width:9.375rem;"
        bodyStyle="text-align:center;max-width:15rem ;min-width:9.375rem"
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
              @click="editEmailGroup(data.data)"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-pencil"
              v-tooltip="'Sửa'"
            ></Button>
            <Button
              @click="delEmailGroup(data.data, true)"
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
  <Sidebar
    v-model:visible="isShowEmail"
    position="right"
    class="main-layout p-sidebar-lg py-0 overflow-hidden true flex-grow-1 p-2"
    :breakpoints="{ '1366px': '100vw', '960px': '100vw', '640px': '100vw' }"
    :style="'width: 45vw; min-height: 100vh !important'"
    :showCloseIcon="false"
    @hide="hideall()"
  >
    <div
      :style="
        width < 1900
          ? 'height: 100vh;max-height: 95vh !important;over-follow: hidden !important;margin-top: 1rem;'
          : 'height: 100vh;max-height: 97vh !important;over-follow: hidden !important;margin-top: 1rem;'
      "
    >
      <DataTable
        :lazy="true"
        @page="onPageEmail($event)"
        @filter="onFilterEmail($event)"
        @sort="onSortEmail($event)"
        :value="emailList"
        :loading="optionsEmail.loading"
        :paginator="true"
        :rows="optionsEmail.PageSize"
        :totalRecords="optionsEmail.totalRecords"
        dataKey="user_id"
        :rowHover="true"
        v-model:filters="filters"
        filterDisplay="menu"
        :showGridlines="true"
        filterMode="lenient"
        paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
        :rowsPerPageOptions="[20, 30, 50, 100, 200]"
        responsiveLayout="scroll"
        :scrollable="true"
        scrollHeight="flex"
        :globalFilterFields="['email_name', 'full_name']"
        v-model:selection="selectedEmail"
        class="w-full"
        :style="'min-height: 92vh !important'"
        v-model:first="firstEmail"
      >
        <template #header>
          <h3 class="module-title mt-0 ml-1 mb-2">
            <i class="pi pi-at"></i>
            Danh sách người dùng ({{ optionsEmail.totalRecords }}) - Nhóm chức
            năng:
            {{ EmailGrName }}
          </h3>

          <Toolbar class="w-full custoolbar">
            <!-- <template #start>
              <span class="p-input-icon-left">
                <i class="pi pi-search" /> -->
            <!-- <InputText
                  v-model="optionsEmail.SearchText"
                  @keyup.enter="searchEmail"
                  type="text"
                  spellcheck="false"
                  placeholder="Tìm kiếm Email"
                /> -->
            <!-- <Button
                  class="ml-2 p-button-outlined p-button-secondary"
                  icon="pi pi-filter"
                  @click="toggleFilter"
                  aria-haspopup="true"
                  aria-controls="overlay_filter"
                  v-tooltip="'Bộ lọc'"
                  :style="[styleObj1]"
                /> -->
            <!-- <OverlayPanel
                  ref="op"
                  appendTo="body"
                  class="p-0 m-0"
                  :showCloseIcon="false"
                  id="overlay_panel"
                  :style="'width:400px'"
                >
                  <div class="grid formgrid m-0"> -->
            <!-- <div class="flex field col-12 p-0">
                      <div
                        :class="'col-4 text-left pt-2 p-0'"
                        style="text-align: left"
                      >
                        Phân loại
                      </div>

                      <div :class="'col-8'">
                        <Dropdown
                          class="col-12 p-0 m-0"
                          v-model="filterPhanloai"
                          :options="phanLoai"
                          optionLabel="name"
                          optionValue="code"
                          placeholder="Phân loại"
                        />
                      </div>
                    </div> -->
            <!-- <div class="flex field col-12 p-0">
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
                          <Button @click="filterEmails" label="Lọc"></Button>
                        </template>
                      </Toolbar>
                    </div>
                  </div>
                </OverlayPanel>
              </span>
            </template> -->

            <template #end>
              <Button
                label="Quay lại"
                icon="pi pi-arrow-left"
                class="mr-2 p-button-outlined"
                @click="prevPage"
              />
              <Button
                v-if="checkDelListEmail"
                @click="deleteEmailList($event)"
                label="Xóa"
                icon="pi pi-trash"
                class="mr-2 p-button-danger"
              />
              <Button
                @click="addEmail('Thêm người dùng')"
                label="Thêm mới"
                icon="pi pi-plus"
                class="mr-2"
                v-tooltip="'Thêm mới'"
              />
              <Button
                @click="refreshEmail()"
                class="mr-2 p-button-outlined p-button-secondary"
                icon="pi pi-refresh"
                v-tooltip="'Tải lại'"
              />

              <!-- <Button
                label="Tiện ích"
                icon="pi pi-file-excel"
                class="mr-2 p-button-outlined p-button-secondary"
                @click="toggleExport"
                aria-haspopup="true"
                aria-controls="overlay_Export"
              /> -->
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
          headerStyle="text-align:center;max-width:5rem;height:50px"
          bodyStyle="text-align:center;max-width:5rem;"
          class="align-items-center justify-content-center text-center"
          v-if="
            store.state.user.is_super == true ||
            store.state.user.is_admin == true
          "
        >
        </Column>
        <Column
          field="full_name"
          header="Thông tin"
          headerStyle="justify-content:center;align-items:center;text-align:center;height:50px"
          bodyStyle=""
          class=""
        >
          <template #body="data">
            <div class="flex row col-12 p-0 m-0">
              <div class="flex col-3 p-0 m-0 format-center">
                <Avatar
                  @error="
                    $event.target.src =
                      basedomainURL + '/Portals/Image/nouser1.png'
                  "
                  v-bind:label="
                    data.data.avt
                      ? ''
                      : data.data.full_name.split(' ').at(-1).substring(0, 1)
                  "
                  v-bind:image="basedomainURL + data.data.avt"
                  style="color: #ffffff; cursor: pointer"
                  :style="{
                    background: bgColor[data.data.is_order % 7],
                    border: '2px solid' + bgColor[data.data.is_order % 7],
                  }"
                  class="myTextAvatar"
                  size="xlarge"
                  shape="circle"
                />
              </div>
              <div class="flex col-9 p-0 m-0">
                <div class="">
                  <span class="font-bold">{{ data.data.full_name }}</span>
                  <br />
                  <span class="py-1">{{ data.data.positions }}</span>
                  <br v-if="data.data.organization_name != null" />
                  <span>
                    {{
                      data.data.organization_name
                        ? data.data.organization_name
                        : ""
                    }}
                  </span>
                  <br v-if="data.data.department_name != null" />
                  <span>
                    {{
                      data.data.department_name ? data.data.department_name : ""
                    }}
                  </span>
                  <br v-if="data.data.department_name != null" />
                  <span>
                    {{ data.data.phone ? data.data.phone : "" }}
                  </span>
                </div>
              </div>
            </div>
          </template>
        </Column>

        <!-- <Column
          field="organization_id"
          header="Hệ thống"
          headerStyle="text-align:center;max-width:5rem;height:50px"
          bodyStyle="text-align:center;max-width:5rem;"
          class="align-items-center justify-content-center text-center"
        >
          <template #body="data">
            <div v-if="data.data.organization_id == 0">
              <i
                class="pi text-blue-400 pi-check"
                style="font-size: 1.5rem"
              ></i>
            </div>
            <div v-else></div>
          </template>
        </Column> -->
        <Column
          header="Chức năng"
          headerStyle="text-align:center;max-width:7em;height:50px"
          bodyStyle="text-align:center;max-width:7rem;"
          class="align-items-center justify-content-center text-center"
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
                @click="delEmail(data.data, true)"
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
  </Sidebar>
  <Dialog
    :header="headerDialog"
    v-model:visible="displayBasic"
    :style="{ width: '40vw' }"
    :closable="false"
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12">
          <label class="col-3 text-left p-0"
            >Nhóm chức năng<span class="redsao"> (*) </span></label
          >
          <InputText
            v-model="EmailGroup.role_group_name"
            spellcheck="false"
            class="col-8 ip36 p-0 px-2"
            :class="{ 'p-invalid': v$.role_group_name.$invalid && submitted }"
          />
        </div>
        <div
          style="display: flex"
          class="field col-12 md:col-12 p-0 mb-2"
        >
          <div class="col-3 text-left"></div>
          <small
            v-if="
              (v$.role_group_name.$invalid && submitted) ||
              v$.role_group_name.$pending.$response
            "
            class="col-9 p-error p-0"
          >
            <span class="col-12 p-0">{{
              v$.role_group_name.required.$message
                .replace("Value", "Tên nhóm chức năng")
                .replace("is required", "không được để trống")
            }}</span>
          </small>
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-3 text-left p-0">Loại nhóm </label>
          <Dropdown
            class="col-8 p-0 m-0"
            v-model="EmailGroup.is_type"
            :options="isType"
            optionLabel="name"
            optionValue="code"
            placeholder="Loại nhóm"
          />
        </div>

        <div
          style="display: flex"
          class="col-12 field md:col-12"
        >
          <div class="field col-6 md:col-6 p-0">
            <label class="col-6 text-left p-0">STT </label>
            <InputNumber
              v-model="EmailGroup.is_order"
              class="col-6 ip36 p-0"
            />
          </div>
          <div class="field col-6 md:col-6 p-0">
            <label
              style="vertical-align: text-bottom"
              class="col-6 text-center p-0"
              >Trạng thái
            </label>
            <InputSwitch
              v-model="EmailGroup.status"
              class="col-6"
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
        class="p-button-text"
      />

      <Button
        label="Lưu"
        icon="pi pi-check"
        @click="saveEmailGroup(!v$.$invalid)"
      />
    </template>
  </Dialog>
  <Dialog
    :header="headerAddEmail"
    v-model:visible="displayEmail"
    :style="{ width: '35vw' }"
    :closable="false"
    style="z-index: 10000"
  >
    <form>
      <div class="row">
        <div class="col-12">
          <InputText
            v-model="searchUser"
            @keyup="LoadUser"
            type="text"
            spellcheck="false"
            placeholder="Nhập tên người dùng..."
            class="col-12"
          />
        </div>
      </div>

      <div class="row p-0 m-0">
        <div class="col-12 p-0 m-0">
          <ScrollPanel style="width: 100%; height: 50vh">
            <div
              v-for="(p, index) in ListUser"
              :key="p"
              class="user-hover-z"
            >
              <div
                class="row p-0 m-0 flex py-2"
                :class="
                  checkUser != null && checkUser.includes(p.user_id)
                    ? 'user-hover'
                    : ''
                "
              >
                <div class="flex col-2 format-center">
                  <Checkbox
                    :value="p.user_id"
                    v-model="checkUser"
                  />
                </div>
                <div class="flex col-3 p-0 m-0 format-center">
                  <Avatar
                    @error="
                      $event.target.src =
                        basedomainURL + '/Portals/Image/nouser1.png'
                    "
                    v-bind:label="
                      p.avatar
                        ? ''
                        : p.full_name.split(' ').at(-1).substring(0, 1)
                    "
                    v-bind:image="basedomainURL + p.avatar"
                    style="color: #ffffff; cursor: pointer"
                    :style="{
                      background: bgColor[index % 7],
                      border: '2px solid' + bgColor[index % 7],
                    }"
                    class=""
                    size="large"
                    shape="circle"
                  />
                </div>
                <div class="col-7">
                  <div class="font-bold text-xl mt-1">{{ p.full_name }}</div>

                  <div class="mt-1">{{ p.tenChucVu }}</div>

                  <div class="mt-1">
                    {{ p.organization_name ?? p.department_name }}
                  </div>
                </div>
              </div>
            </div>
          </ScrollPanel>
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="closeDialogEmail"
        class="p-button-text"
      />

      <Button
        label="Lưu"
        icon="pi pi-check"
        @click="saveEmail(checkUser)"
        v-if="checkUser.length != 0"
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
      <label v-if="!isShowEmail">
        <a
          :href="basedomainURL + item"
          download
          >Nhấn vào đây</a
        >
        để tải xuống tệp mẫu.
      </label>
      <label v-else>
        <a
          :href="basedomainURL + item2"
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

<style lang="scss" scoped>
.user-hover {
  background-color: var(--blue-100);
}
.email {
  background-color: #2196f3 !important;
  color: #fff !important;
  border: 1px solid #5ca7e3 !important;
}
.p-paginator .p-paginator-pages .p-paginator-page.p-highlight {
  background: #2196f3 !important;
  border-color: #fde3e3;
  color: #574949;
}

.p-avatar.p-avatar-xl {
  font-size: 2rem !important;
}
.user-hover-z:hover {
  background-color: var(--blue-100);
}
</style>
