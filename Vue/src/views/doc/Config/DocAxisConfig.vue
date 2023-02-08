<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import moment from "moment";
import { encr } from "../../../util/function";
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
  organization_connect_id: {
    required,
  },
  connect_token_key: {
    required,
  },
};
const doc_org = ref({
  organization_connect_id: null,
  organization_connect_name: null,
  connect_token_key: null,
  status: null,
  organization_id: null,
  is_order: null,
});
const selectedDispatchs = ref();
const submitted = ref(false);
const v$ = useVuelidate(rules, doc_org);
const issaveDispatch = ref(false);
const datalists = ref();
const toast = useToast();
const basedomainURL = baseURL;
const checkDelList = ref(false);

const options = ref({
  IsNext: true,
  sort: "organization_connect_id",
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
      baseURL + "/api/DocProc/CallProc",
      {
        str: encr(
          JSON.stringify({
            proc: "doc_organization_connect_count",
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
        baseURL + "/api/DocProc/CallProc",
        {
          str: encr(
            JSON.stringify({
              proc: "doc_organization_connect_list",
              par: [
                { par: "user_id", va: store.state.user.user_id },
                { par: "pageno", va: options.value.PageNo },
                { par: "pagesize", va: options.value.PageSize },
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
          controller: "DocAxisConfig.vue",
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
const openBasic = (str) => {
  textboxLength.value = 0;
  textboxLength2.value = 0;
  textboxLength3.value = 6;
  select.value = {};
  submitted.value = false;
  doc_org.value = {
    organization_connect_id: null,
    organization_connect_name: null,
    connect_token_key: null,
    status: true,
    organization_id: null,
  };
  doc_org.value.organization_id = store.state.user.organization_id;
  doc_org.value.is_order = datalists.value[0].is_order + 1;
  issaveDispatch.value = false;
  headerDialog.value = str;
  displayBasic.value = true;
};
const closeDialog = () => {
  doc_org.value = {
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
  if (
    textboxLength.value > 50 ||
    textboxLength2.value > 250 ||
    textboxLength3.value > 50 ||
    checkSpace1.value == true ||
    checkSpace3.value == true
  ) {
    return;
  }
  if (select.value) {
    let id = parseInt(Object.keys(select.value)[0]);
    doc_org.value.department_id = id;
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
      `/api/AxisConfig/${
        issaveDispatch.value ? "Update_doc_org_connect" : "add_doc_org_connect"
      }`,
    data: doc_org.value,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success(
          response.data.method == "put"
            ? "Cập nhật cấu hình thành công!"
            : "Thêm cấu hình thành công",
        );
        loadData(true);
        closeDialog();
      } else {
        let ms = response.data.ms;
        swal.fire({
          title: "Thông báo!",
          html: ms,
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
  textboxLength.value = 0;
  textboxLength2.value = 0;
  textboxLength3.value = 0;
  checkSpace1.value = false;
  checkSpace3.value = false;
  select.value = {};
  submitted.value = false;
  doc_org.value = dataPlace;
  isChirlden.value = false;
  if (dataPlace.parent_id != null) {
    isChirlden.value = true;
  }
  headerDialog.value = "Sửa cấu hình";
  issaveDispatch.value = true;
  displayBasic.value = true;
  doc_org.value.organization_id = store.state.user.organization_id;
};
//Xóa bản ghi
const delDispatch = (Dispatch) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá cấu hình này không!",
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
          .delete(baseURL + "/api/AxisConfig/Delete_doc_org_connect", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: Dispatch != null ? [Dispatch.organization_connect_id] : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá cấu hình thành công!");
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
                html: response.data.ms,
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
        window.open(baseURL + response.data.path);
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
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
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
    IntID: value.organization_connect_id,
    TextID: value.organization_connect_id,
    IntTrangthai: 1,
    BitTrangthai: value.status,
  };
  axios
    .put(
      baseURL + "/api/AxisConfig/Update_Status_Doc_org_Connect",
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
          listId.push(item.organization_connect_id);
        });
        axios
          .delete(baseURL + "/api/AxisConfig/Delete_doc_org_connect", {
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
                html: response.data.ms,
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
      baseURL + "/api/DocProc/CallProc",
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
const loadDonvi1 = () => {
  axios
    .post(
      baseURL + "/api/DocProc/CallProc",
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
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    });
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
  return { arrChils: arrChils, arrtreeChils: arrtreeChils };
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
const checkSpace1 = ref();
const checkSpace2 = ref();
const checkSpace3 = ref();
let pattern = /\s/;
const textboxLength = ref();
const focusInput = (e) => {};
const removeVietnameseTones = (str) => {
  str = str.replace(/à|á|ạ|ả|ã|â|ầ|ấ|ậ|ẩ|ẫ|ă|ằ|ắ|ặ|ẳ|ẵ/g, "a");
  str = str.replace(/è|é|ẹ|ẻ|ẽ|ê|ề|ế|ệ|ể|ễ/g, "e");
  str = str.replace(/ì|í|ị|ỉ|ĩ/g, "i");
  str = str.replace(/ò|ó|ọ|ỏ|õ|ô|ồ|ố|ộ|ổ|ỗ|ơ|ờ|ớ|ợ|ở|ỡ/g, "o");
  str = str.replace(/ù|ú|ụ|ủ|ũ|ư|ừ|ứ|ự|ử|ữ/g, "u");
  str = str.replace(/ỳ|ý|ỵ|ỷ|ỹ/g, "y");
  str = str.replace(/đ/g, "d");
  str = str.replace(/À|Á|Ạ|Ả|Ã|Â|Ầ|Ấ|Ậ|Ẩ|Ẫ|Ă|Ằ|Ắ|Ặ|Ẳ|Ẵ/g, "A");
  str = str.replace(/È|É|Ẹ|Ẻ|Ẽ|Ê|Ề|Ế|Ệ|Ể|Ễ/g, "E");
  str = str.replace(/Ì|Í|Ị|Ỉ|Ĩ/g, "I");
  str = str.replace(/Ò|Ó|Ọ|Ỏ|Õ|Ô|Ồ|Ố|Ộ|Ổ|Ỗ|Ơ|Ờ|Ớ|Ợ|Ở|Ỡ/g, "O");
  str = str.replace(/Ù|Ú|Ụ|Ủ|Ũ|Ư|Ừ|Ứ|Ự|Ử|Ữ/g, "U");
  str = str.replace(/Ỳ|Ý|Ỵ|Ỷ|Ỹ/g, "Y");
  str = str.replace(/Đ/g, "D");
  // Some system encode vietnamese combining accent as individual utf-8 characters
  // Một vài bộ encode coi các dấu mũ, dấu chữ như một kí tự riêng biệt nên thêm hai dòng này
  str = str.replace(/\u0300|\u0301|\u0303|\u0309|\u0323/g, ""); // ̀ ́ ̃ ̉ ̣  huyền, sắc, ngã, hỏi, nặng
  str = str.replace(/\u02C6|\u0306|\u031B/g, ""); // ˆ ̆ ̛  Â, Ê, Ă, Ơ, Ư
  // Remove extra spaces
  // Bỏ các khoảng trắng liền nhau
  str = str.replace(/ + /g, " ");
  // loại bỏ tất cả khoảng trắng
  str = str.replace(/\s+/g, "");
  str = str.trim();
  // Remove punctuations
  // Bỏ dấu câu, kí tự đặc biệt
  str = str.replace(
    /!|@|%|\^|\*|\(|\)|\+|\=|\<|\>|\?|\/|,|\.|\:|\;|\'|\"|\&|\#|\[|\]|~|\$|_|`|-|{|}|\||\\/g,
    " ",
  );
  doc_org.value.organization_connect_id = str;
  checkSpace1.value = false;
  textboxLength.value = 0;
  textboxLength.value = doc_org.value.organization_connect_id.length;
  checkSpace1.value = pattern.test(doc_org.value.organization_connect_id);
};
const textboxLength2 = ref();
const focusInput2 = () => {
  textboxLength2.value = 0;
  const textbox = document.getElementById("name");
  textboxLength2.value = textbox.value.length;
  checkSpace2.value = pattern.test(textbox.value);
};
const textboxLength3 = ref();
const focusInput3 = () => {
  textboxLength3.value = 0;
  const textbox = document.getElementById("token");
  textboxLength3.value = textbox.value.length;
  checkSpace3.value = pattern.test(textbox.value);
};
const item = "/Portals/Mau Excel/Mẫu Excel Người ký.xlsx";
const CreateGuid = () => {
  const _p8 = (s) => {
    var p = (Math.random().toString(16) + "000000000").substr(2, 8);
    return s ? "-" + p.substr(0, 4) + "-" + p.substr(4, 4) : p;
  };
  doc_org.value.connect_token_key = _p8() + _p8(true) + _p8(true) + _p8();
};
const isLetter = (e) => {
  console.log(e);
  var ew = e.keyCode;
  // if (ew == 32) return true;
  if (48 <= ew && ew <= 57) return true;
  if (65 <= ew && ew <= 90) return true;
  if (96 <= ew && ew <= 105) return true;
  else return false;
};
onMounted(() => {
  loadData(true);
  loadDonvi1();
  loadDonvi();
  return {
    datalists,
    options,
    onPage,
    loadData,
    loadCount,
    openBasic,
    closeDialog,
    basedomainURL,
    saveDispatch,
    isFirst,
    searchDispatchs,
    onCheckBox,
    selectedDispatchs,
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
      :scrollable="true"
      scrollHeight="flex"
      :loading="options.loading"
      v-model:selection="selectedDispatchs"
      :lazy="true"
      @page="onPage($event)"
      @filter="onFilter($event)"
      @sort="onSort($event)"
      :totalRecords="options.totalRecords"
      dataKey="organization_connect_id"
      :rowHover="true"
      v-model:filters="filters"
      filterDisplay="menu"
      :showGridlines="true"
      filterMode="lenient"
      paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
      :rowsPerPageOptions="[20, 30, 50, 100, 200]"
      responsiveLayout="scroll"
    >
      <template #header>
        <h3 class="module-title mt-0 ml-1 mb-2">
          <i class="pi pi-cog"></i> Danh sách cấu hình ({{
            options.totalRecords
          }})
        </h3>

        <Toolbar class="w-full custoolbar">
          <!-- <template #start>
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
                :style="
                  store.state.user.is_super == 1 ? 'width:40vw' : 'width:300px'
                "
              >
                <div class="grid formgrid m-0">
                  <div
                    class="flex field col-12 p-0"
                    v-if="store.state.user.is_super"
                  >
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
                        class="col-12 p-0 m-0 md:col-12"
                        v-if="store.state.user.is_super == 1"
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
                      :class="
                        store.state.user.is_super == 1 ? 'col-10' : 'col-8'
                      "
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
              v-if="checkDelList"
              @click="deleteList()"
              label="Xóa"
              icon="pi pi-trash"
              class="mr-2 p-button-danger"
            />
            <Button
              @click="openBasic('Thêm cấu hình')"
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
            <!-- 
            <Button
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
        headerStyle="text-align:center;max-width:75px;height:50px"
        bodyStyle="text-align:center;max-width:75px;"
        class="align-items-center justify-content-center text-center"
      ></Column>
      <Column
        field="is_order"
        header="STT"
        :sortable="false"
        headerStyle="text-align:center;max-width:75px;height:50px"
        bodyStyle="text-align:center;max-width:75px;"
        class="align-items-center justify-content-center text-center"
      >
      </Column>
      <Column
        field="organization_connect_id"
        header="Mã cấu hình"
        :sortable="false"
        headerStyle="text-align:center;max-width:25rem;height:50px"
        bodyStyle="text-align:center;max-width:25rem;"
        class="align-items-center justify-content-center text-center"
      >
      </Column>

      <Column
        field="organization_connect_name"
        header="Tên cấu hình"
        :sortable="false"
        headerStyle="height:50px"
        bodyStyle=""
      >
      </Column>
      <Column
        field="connect_token_key"
        header="Mã bảo mật"
        :sortable="false"
        headerStyle="text-align:center;max-width:25rem;height:50px"
        bodyStyle="text-align:center;max-width:25rem;"
        class="align-items-center justify-content-center text-center"
      >
      </Column>
      <Column
        field="status"
        header="Hiển thị"
        headerStyle="text-align:center;max-width:120px;height:50px"
        bodyStyle="text-align:center;max-width:120px;"
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
        header="Chức năng"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:150px;height:50px"
        bodyStyle="text-align:center;max-width:150px;"
      >
        <template #body="data">
          <div>
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
            src="../../../assets/background/nodata.png"
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
                Mã cấu hình
                <i
                  class="pi pi-info-circle"
                  v-tooltip="'Mã cấu hình không được viết dấu!'"
                /><span class="redsao">(*)</span>
              </div>
              <div class="col-10 p-0 m-0 ip36 px-2">
                <InputText
                  :disabled="issaveDispatch == true ? true : false"
                  id="ID"
                  v-model="doc_org.organization_connect_id"
                  spellcheck="false"
                  class="w-full"
                  @input="focusInput($event)"
                  @change="
                    removeVietnameseTones(doc_org.organization_connect_id)
                  "
                  :class="{
                    'p-invalid':
                      (v$.organization_connect_id.$invalid && submitted) ||
                      textboxLength > 50 ||
                      checkSpace1 == true,
                  }"
                  autocomplete="off"
                />
              </div>
            </div>
            <div
              style="display: flex"
              class="col-12 md:col-12 px-0"
              v-if="textboxLength > 50"
            >
              <div class="col-2 text-left"></div>
              <small class="col-10 p-error">
                <span class="col-12 p-0">Mã cấu hình không quá 50 kí tự!</span>
              </small>
            </div>
            <div
              style="display: flex"
              class="col-12 md:col-12 px-0"
              v-if="checkSpace1 == true"
            >
              <div class="col-2 text-left"></div>
              <small class="col-10 p-error">
                <span class="col-12 p-0"
                  >Mã cấu hình không được chứa dấu cách!</span
                >
              </small>
            </div>
            <div class="col-12 flex p-0">
              <div class="col-2"></div>
              <small
                v-if="
                  (v$.organization_connect_id.$invalid && submitted) ||
                  v$.organization_connect_id.$pending.$response
                "
                class="col-10 p-error"
              >
                <span class="col-12 p-0">{{
                  v$.organization_connect_id.required.$message
                    .replace("Value", "Mã cấu hình")
                    .replace("is required", "không được để trống")
                }}</span>
              </small>
            </div>

            <div class="col-12 p-0 my-3 flex format-left">
              <div class="col-2 p-0 line-height-4">Tên cấu hình</div>
              <div class="col-10 ip36 px-2">
                <InputText
                  id="name"
                  v-model="doc_org.organization_connect_name"
                  class="w-full"
                  @input="focusInput2()"
                  :class="{
                    'p-invalid': textboxLength2 > 250,
                  }"
                  autocomplete="off"
                />
              </div>
            </div>
            <div
              style="display: flex"
              class="col-12 md:col-12 px-0"
              v-if="textboxLength2 > 250"
            >
              <div class="col-2 text-left"></div>
              <small class="col-10 p-error">
                <span class="col-12 p-0"
                  >Tên cấu hình không quá 250 kí tự!</span
                >
              </small>
            </div>

            <div class="col-12 p-0 my-3 flex">
              <div class="col-2 flex p-0 format-left">
                <div class="col-12 p-0 line-height-4">
                  Mã bảo mật <span class="redsao">(*)</span>
                </div>
              </div>
              <div class="col-7">
                <InputText
                  id="token"
                  v-model="doc_org.connect_token_key"
                  class="w-full"
                  @input="focusInput3()"
                  :class="{
                    'p-invalid':
                      textboxLength3 > 50 ||
                      textboxLength3 < 6 ||
                      checkSpace3 == true ||
                      (v$.connect_token_key.$invalid && submitted),
                  }"
                  autocomplete="off"
                />
              </div>
              <div class="col-3 flex format-left">
                <Button
                  icon="pi pi-key"
                  class="w-full"
                  label="Tạo mã bảo mật"
                  @click="CreateGuid()"
                ></Button>
              </div>
            </div>
            <div
              style="display: flex"
              class="col-12 md:col-12 px-0 p-0"
              v-if="textboxLength3 < 6"
            >
              <div class="col-2 text-left"></div>
              <small class="col-10 p-error">
                <span class="col-12 p-0">Mã bảo mật ít nhất 6 kí tự!</span>
              </small>
            </div>
            <div
              style="display: flex"
              class="col-12 md:col-12 px-0 p-0"
              v-if="textboxLength3 > 50"
            >
              <div class="col-2 text-left"></div>
              <small class="col-10 p-error">
                <span class="col-12 p-0">Mã bảo mật không quá 50 kí tự!</span>
              </small>
            </div>
            <div
              style="display: flex"
              class="col-12 md:col-12 px-0 p-0"
              v-if="checkSpace3 == true"
            >
              <div class="col-2 text-left"></div>
              <small class="col-10 p-error">
                <span class="col-12 p-0"
                  >Mã bảo mật không được chứa dấu cách!</span
                >
              </small>
            </div>
            <div class="col-12 flex p-0">
              <div class="col-2"></div>
              <small
                v-if="
                  (v$.connect_token_key.$invalid && submitted) ||
                  v$.connect_token_key.$pending.$response
                "
                class="col-10 p-error"
              >
                <span class="col-12 p-0">{{
                  v$.connect_token_key.required.$message
                    .replace("Value", "Mã bảo mật")
                    .replace("is required", "không được để trống")
                }}</span>
              </small>
            </div>
            <div class="flex col-12 px-0">
              <div class="col-2 format-left px-0">Số thứ tự</div>
              <InputNumber
                v-model="doc_org.is_order"
                class="col-7"
                autocomplete="off"
              />
              <div class="col-3 flex p-0">
                <div class="col-6 format-left line-height-4">Hiển thị</div>
                <div class="col-6 format-center">
                  <InputSwitch
                    v-model="doc_org.status"
                    class=""
                  />
                </div>
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
.format-center {
  display: flex;
  justify-content: center;
  align-items: center;
  vertical-align: middle;
  text-align: center;
}

.format-right {
  display: flex;
  justify-content: flex-end;
  align-items: center;
  vertical-align: middle;
  text-align: center;
}

.format-left {
  display: flex;
  justify-content: flex-start;
  align-items: center;
  vertical-align: middle;
}
</style>
