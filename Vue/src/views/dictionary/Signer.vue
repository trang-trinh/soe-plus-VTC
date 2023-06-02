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
  signer_name: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
});
const rules = {
  signer_name: {
    required,
    $errors: [
      {
        $property: "signer_name",
        $validator: "required",
        $message: "Tên người ký không được để trống!",
      },
    ],
  },
};
const signer = ref({
  signer_name: "",

  status: true,
  is_order: 1,
});
const selectedDispatchs = ref();
const submitted = ref(false);
const v$ = useVuelidate(rules, signer);
const issaveDispatch = ref(false);
const datalists = ref();
const toast = useToast();
const basedomainURL = baseURL;
const checkDelList = ref(false);
const departmentOptions = ref([
  { name: "Chọn phòng ban...", code: null },
  { name: "Kế toán", code: 0 },
  { name: "Nhân sự", code: 1 },
  { name: "Giám đốc", code: -1 },
]);
const typeOptions = ref([
  { name: "Chọn loại văn bản...", code: null },
  { name: "Văn bản đến", code: 1 },
  { name: "Văn bản đi", code: 2 },
  { name: "Nội bộ", code: 3 },
]);
const options = ref({
  IsNext: true,
  sort: "signer_id",
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
            proc: "ca_signers_count",
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
        sttDispatch.value = options.value.totalRecords + 1;
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
              proc: "ca_signers_list",
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
          controller: "SignerView.vue",
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

    options.value.id = datalists.value[datalists.value.length - 1].signer_id;
    options.value.IsNext = true;
  } else if (event.page < options.value.PageNo) {
    //Trang trước
    options.value.id = datalists.value[0].signer_id;
    options.value.IsNext = false;
  }
  options.value.PageNo = event.page;
  loadData(true);
};
//Hiển thị dialog
const headerDialog = ref();
const displayBasic = ref(false);
const openBasic = (str) => {
  select.value = {};
  submitted.value = false;
  signer.value = {
    signer_name: "",
    is_order: sttDispatch.value,
    status: true,
    organization_id:
      store.state.user.is_super == true
        ? store.state.user.organization_parent_id != null
          ? store.state.user.organization_parent_id
          : store.state.user.organization_id
        : store.state.user.organization_id,
    is_system: store.state.user.is_super == true ? true : false,
  };
  issaveDispatch.value = false;
  headerDialog.value = str;
  displayBasic.value = true;
};
const closeDialog = () => {
  signer.value = {
    signer_name: "",
    is_order: 1,
    status: true,
  };
  displayBasic.value = false;
  loadData(true);
};
const select = ref();
//Thêm bản ghi
const saveDispatch = (isFormValid) => {
  if (select.value) {
    let id = parseInt(Object.keys(select.value)[0]);
    signer.value.department_id = id;
  }
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
    method: issaveDispatch.value ? "put" : "post",
    url:
      baseURL +
      `/api/ca_signers/${
        issaveDispatch.value ? "Update_signers" : "Add_signer"
      }`,
    data: signer.value,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success(
          response.data.method == "put"
            ? "Cập nhật người ký thành công!"
            : "Thêm người ký thành công",
        );
        loadData(true);
        closeDialog();
      } else {
        let ms = response.data.ms;
        swal.fire({
          title: "Thông báo!",
          html:
            ms.includes("signer_name") == true &&
            ms.includes("position") == true
              ? "Tên người ký không quá 250 ký tự! <br> Chức vụ không quá 250 ký tự!"
              : ms.includes("positon")
              ? "Chức vụ không quá 250 ký tự!"
              : "Tên người ký không quá 250 ký tự!",
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

const sttDispatch = ref();
//Thêm bản ghi con
const isChirlden = ref(false);

//Sửa bản ghi
const editDispatch = (dataPlace) => {
  select.value = {};
  submitted.value = false;
  signer.value = dataPlace;
  isChirlden.value = false;
  if (dataPlace.parent_id != null) {
    isChirlden.value = true;
  }
  headerDialog.value = "Sửa người ký";
  issaveDispatch.value = true;
  displayBasic.value = true;

  if (signer.value.department_id) {
    select.value[dataPlace.department_id || "-1"] = true;
  }
};
//Xóa bản ghi
const delDispatch = (Dispatch) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá người ký này không!",
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
          .delete(baseURL + "/api/ca_signers/Delete_signers", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: Dispatch != null ? [Dispatch.signer_id] : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá người ký thành công!");
              if (
                (options.value.totalRecords - Dispatch.length) % 2 == 0 &&
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
        excelname: "DANH SÁCH NGƯỜI KÝ",
        proc: "ca_signers_listexport",
        par: [
          { par: "search", va: options.value.SearchText },
          { par: "status", va: filterTrangthai.value },
          { par: "user_id", va: store.state.user.user_id },
          { par: "s_org", va: filterPhanloai.value },
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
  filterPhanloai.value =
    filterPhanloai.value == undefined ? null : filterPhanloai.value;
  let fpl;
  if (filterPhanloai.value != undefined && store.state.user.is_super) {
    fpl = parseInt(Object.keys(filterPhanloai.value)[0]);
  }
  let data = {
    sqlS: filterTrangthai.value != null ? filterTrangthai.value : null,
    sqlO: options.value.sort,
    Search: options.value.SearchText,
    PageNo: options.value.PageNo,
    PageSize: options.value.PageSize,
    sqlF: store.state.user.is_super ? fpl : store.state.user.organization_id,
  };

  options.value.loading = true;
  axios
    .post(baseURL + "/api/SQL/Filter_Signer", data, config)
    .then((response) => {
      let dt = JSON.parse(response.data.data);
      let data = dt[0];
      if (data.length > 0) {
        data.forEach((element, i) => {
          element.STT = options.value.PageNo * options.value.PageSize + i + 1;
          if (options.value.sort == "is_order DESC") {
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
const searchDispatchs = (event) => {
  options.value.loading = true;
  isDynamicSQL.value = true;
  loadData(true);
};
const onFilter = (event) => {
  filterSQL.value = [];

  for (const [key, value] of Object.entries(event.filters)) {
    if (key != "global") {
      let obj = {
        key: key != "signer_name" ? "signer_name" : key,
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
    IntID: value.signer_id,
    TextID: value.signer_id + "",
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
      .put(baseURL + "/api/ca_signers/Update_StatusSigner", data, config)
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
  let listId = new Array(selectedDispatchs.value.length);
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
        selectedDispatchs.value.forEach((item) => {
          listId.push(item.signer_id);
        });
        axios
          .delete(baseURL + "/api/ca_signers/Delete_signers", {
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
const searchDispatch = () => {
  options.value.PageNo = 0;
  filterTrangthai.value = null;
  filterPhanloai.value = null;
  options.value.SearchText = "";
  isDynamicSQL.value = false;
  styleObj.value = "";
  options.value.loading = true;
  first.value = 0;
  options.value = {
    PageNo: 0,
    PageSize: 20,
  };
  selectedDispatchs.value = [];
  loadData(true);
};
const first = ref(0);
//Filter
const trangThai = ref([
  { name: "Hiển thị", code: 1 },
  { name: "Không hiển thị", code: 0 },
]);
const filterPhanloai = ref();
const filterTrangthai = ref();

const reFilterEmail = () => {
  filterPhanloai.value = null;
  filterTrangthai.value = null;
  loadDataSQL();
  styleObj.value = "";
};
const filterEmails = () => {
  styleObj.value = style.value;
  isDynamicSQL.value = true;
  loadData(true);
};
watch(selectedDispatchs, () => {
  if (selectedDispatchs.value.length > 0) {
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
            proc: "sys_organization_list_all_for_select",
            par: [
              {
                par: "is_super",
                va: store.getters.user.is_super == 1 ? true : false,
              },
              { par: "user_id", va: store.getters.user.user_id },
            ],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      //dataTemp.value = data;

      if (data.length > 0) {
        let obj = renderTree(
          data[0],
          "organization_id",
          "organization_name",
          "phòng ban",
        );

        donvis.value = obj.arrtreeChils;
      } else {
        donvis.value = [];
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
const loadDonvi1 = () => {
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
        data.forEach((x) => {
          x = { key: x.organization_id, data: x, label: x.organization_name };
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
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    });
};

const renderTree = (data, id, name, title) => {
  let arrChils = [];
  let arrtreeChils = [];

  if (store.state.user.is_super == true) {
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
              let om1 = { key: em[id], data: em, label: em[name] };
              retreechildren(om1, em[id]);
              mm.children.push(om1);
            });
          }
        };
        retreechildren(om, m[id]);
        arrtreeChils.push(om);
      });

    return { arrChils: arrChils, arrtreeChils: arrtreeChils };
  } else {
    data.forEach((m, i) => {
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
            let om1 = { key: em[id], data: em, label: em[name] };
            retreechildren(om1, em[id]);
            mm.children.push(om1);
          });
        }
      };
      retreechildren(om, m[id]);
      arrtreeChils.push(om);
    });

    return { arrChils: arrChils, arrtreeChils: arrtreeChils };
  }
};
const donvis = ref();
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
      .post(baseURL + "/api/ImportExcel/Import_Signer", formData, config)
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
const item = "/Portals/Mau Excel/Mẫu Excel Người ký.xlsx";
onMounted(() => {
  loadData(true);
  loadDonvi1();
  loadDonvi();
  return {};
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
      v-model:selection="selectedDispatchs"
      :lazy="true"
      @page="onPage($event)"
      @filter="onFilter($event)"
      @sort="onSort($event)"
      :totalRecords="options.totalRecords"
      dataKey="signer_id"
      :rowHover="true"
      v-model:filters="filters"
      filterDisplay="menu"
      :showGridlines="true"
      filterMode="lenient"
      paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
      :rowsPerPageOptions="[20, 30, 50, 100, 200]"
      responsiveLayout="scroll"
      :globalFilterFields="['signer_name']"
    >
      <template #header>
        <h3 class="module-title mt-0 ml-1 mb-2">
          <i class="pi pi-pound"></i> Danh sách người ký ({{
            options.totalRecords
          }})
        </h3>

        <Toolbar class="w-full custoolbar">
          <template #start>
            <span class="p-input-icon-left">
              <i class="pi pi-search" />
              <InputText
                v-model="options.SearchText"
                @keyup.enter="searchDispatchs"
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
                class="p-0 m-0"
                :showCloseIcon="false"
                id="overlay_panel"
                :style="'width:400px'"
              >
                <div class="grid formgrid m-0">
                  <div
                    class="flex field col-12 p-0"
                    v-if="store.state.user.is_super"
                  >
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
                          @click="filterEmails"
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
              @click="openBasic('Thêm người ký')"
              label="Thêm mới"
              icon="pi pi-plus"
              class="mr-2"
            />
            <Button
              @click="searchDispatch"
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
        class="align-items-center justify-content-center text-center max-w-4rem"
      >
      </Column>

      <Column
        field="signer_name"
        header="Tên người ký"
        :sortable="true"
        headerClass="align-items-center justify-content-center text-center"
      >
      </Column>
      <Column
        field="nav_type"
        header="Loại văn bản"
        class="align-items-center justify-content-center text-center max-w-10rem"
      >
        <template #body="data">
          <div v-if="data.data.nav_type == 1">Văn bản đến</div>
          <div v-else-if="data.data.nav_type == 2">Văn bản đi</div>
          <div v-else>Văn bản nội bộ</div>
        </template>
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
        field="status"
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
        header="Đơn vị"
        field="organization_name"
        class="align-items-center justify-content-center text-center max-w-20rem"
      >
      </Column>
      <Column
        header="Chức năng"
        class="align-items-center justify-content-center text-center max-w-10rem"
      >
        <template #body="data">
          <div
            v-if="
              store.state.user.is_super == true ||
              store.state.user.user_id == data.data.created_by ||
              (store.state.user.is_admin &&
                data.data.is_system != true &&
                store.state.user.organization_id == data.data.organization_id)
            "
          >
            <Button
              @click="editDispatch(data.data)"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-pencil"
              v-tooltip="'Sửa'"
            ></Button>
            <Button
              @click="delDispatch(data.data, true)"
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
    :style="{ width: '50vw' }"
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
              <div class="col-2 text-left p-0 pb-2 line-height-4">
                Tên người ký <span class="redsao">(*)</span>
              </div>
              <InputText
                v-model="signer.signer_name"
                spellcheck="false"
                class="col-10 p-0 m-0 ip36 px-2"
              />
            </div>
            <div class="col-12 flex p-0">
              <div class="col-2"></div>
              <small
                v-if="
                  (v$.signer_name.$invalid && submitted) ||
                  v$.signer_name.$pending.$response
                "
                class="col-10 p-error p-0"
              >
                <span class="col-12 p-0">{{
                  v$.signer_name.required.$message
                    .replace("Value", "Tên người ký")
                    .replace("is required", "không được để trống")
                }}</span>
              </small>
            </div>

            <div class="col-12 p-0 my-3 flex">
              <div class="pb-2 col-2 p-0 line-height-4">Chức vụ</div>
              <InputText
                class="col-10 ip36 px-2"
                v-model="signer.position"
              />
            </div>
            <div class="col-12 p-0 my-3 flex">
              <div class="pb-2 col-2 p-0 line-height-4">Phòng ban</div>

              <TreeSelect
                v-model="select"
                :options="donvis[0].children"
                optionLabel="data.organization_name"
                optionValue="data.organization_id"
                placeholder="Chọn đơn vị"
                class="col-10 md:col-10"
                :scrollable="true"
                :metaKeySelection="false"
              />
            </div>
            <div class="col-12 p-0 my-3 flex">
              <div class="pb-2 col-2 p-0 line-height-4">Loại văn bản</div>
              <Dropdown
                class="col-10"
                v-model="signer.nav_type"
                :options="typeOptions"
                ed
                optionLabel="name"
                placeholder="Chọn loại văn bản"
                optionValue="code"
                :filter="true"
              />
            </div>
            <div class="col-12 p-0 my-3 flex">
              <div class="col-6 flex p-0">
                <div class="pb-2 col-4 p-0 line-height-4">STT</div>
                <InputNumber
                  v-model="signer.is_order"
                  class="col-8 p-0 ip36"
                />
              </div>
              <div class="col-3 flex p-0">
                <div class="col-6 pb-2p-0 line-height-4 px-2">Trạng thái</div>
                <InputSwitch
                  v-model="signer.status"
                  class="p-0 ip36"
                />
              </div>
              <div
                class="col-3 flex p-0"
                v-if="store.state.user.is_super"
              >
                <div class="pb-2 col-6 p-0 line-height-4 px-2">Hệ thống</div>
                <InputSwitch
                  v-model="signer.is_system"
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
        @click="saveDispatch(!v$.$invalid)"
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
