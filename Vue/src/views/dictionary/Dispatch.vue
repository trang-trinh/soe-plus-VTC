<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import moment from "moment";
import { encr } from "../../util/function.js";

const cryoptojs = inject("cryptojs");
import { filter } from "jszip";
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const isDynamicSQL = ref(false);
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const select = ref();
const filters = ref({
  global: { value: null, matchMode: FilterMatchMode.CONTAINS },
  dispatch_book_name: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
});
const rules = {
  dispatch_book_name: {
    required,
    $errors: [
      {
        $property: "dispatch_book_name",
        $validator: "required",
        $message: "Tên khối cơ quan không được để trống!",
      },
    ],
  },
  current_num: {
    required,
    $errors: [
      {
        $property: "current_num",
        $validator: "required",
        $message: "Số hiện tại không được để trống!",
      },
    ],
  },
  nav_type: {
    required,
    $errors: [
      {
        $property: "nav_type",
        $validator: "required",
        $message: "Loại văn bản không được để trống!",
      },
    ],
  },
};
const dispatch = ref({
  dispatch_book_name: "",
  year: 1970,
  current_num: 1,
  tracking_place: "",
  nav_type: 0,
  status: true,
  is_order: 1,
});

const selectedDispatchs = ref();
const submitted = ref(false);
const v$ = useVuelidate(rules, dispatch);
const issaveDispatch = ref(false);
const datalists = ref();
const toast = useToast();
const basedomainURL = baseURL;
const checkDelList = ref(false);
const typeOptions = ref([
  { name: "Chọn loại văn bản...", code: null },
  { name: "Văn bản đến", code: 1 },
  { name: "Văn bản đi", code: 2 },
  { name: "Văn bản nội bộ", code: 3 },
]);
const options = ref({
  IsNext: true,
  sort: "is_order asc",
  SearchText: "",
  PageNo: 0,
  PageSize: 20,
  loading: true,
  totalRecords: null,
  sortsql: 0,
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
            proc: "ca_dispatch_book_count",
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
const minDate = ref();
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
              proc: "ca_dispatch_book_list",
              par: [
                { par: "pageno", va: options.value.PageNo },
                { par: "pagesize", va: options.value.PageSize },
                { par: "user_id", va: store.state.user.user_id },
                { par: "nav_type", va: null },
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

          element.open_date = element.open_date
            ? moment(new Date(element.open_date)).format("DD/MM/YYYY")
            : null;
          element.end_date = element.end_date
            ? moment(new Date(element.end_date)).format("DD/MM/YYYY")
            : null;
        });

        datalists.value = data;
        options.value.loading = false;
      })
      .catch((error) => {
        toast.error("Tải dữ liệu không thành công!");
        options.value.loading = false;
        addLog({
          title: "Lỗi Console loadData",
          controller: "DispatchView.vue",
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
  loadDonvi1();
  loadDonvi();
};
const loadDonvi1 = () => {
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
};
const treedonvis = ref();
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
      dataTemp.value = data;

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
const dataTemp = ref([]);
const donvis = ref();
const findID = (kid) => {
  dataTemp.value[0]
    .filter((x) => x.organization_id == kid)
    .forEach((x) => {
      if (x.parent_id != null) {
        findID(x.parent_id);
      } else {
        dispatch.value.organization_id = x.organization_id;
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
//Hiển thị dialog
const today = new Date();
const headerDialog = ref();
const displayBasic = ref(false);
let y = new Date().getFullYear();
const openBasic = (str) => {
  select.value = {};
  submitted.value = false;
  dispatch.value = {
    dispatch_book_name: "",
    is_order: sttDispatch.value,
    status: true,
    open_date: new Date(),
    end_date: null,
    year: y,
  };

  dispatch.value.organization_id = store.state.user.is_super
    ? 1
    : store.state.user.organization_id;
  issaveDispatch.value = false;
  headerDialog.value = str;
  displayBasic.value = true;
};
const closeDialog = () => {
  dispatch.value = {
    dispatch_book_name: "",
    is_order: 1,
    status: true,
  };
  displayBasic.value = false;
  loadData(true);
};

//Thêm bản ghi
const saveDispatch = (isFormValid) => {
  if (select.value) {
    let id = parseInt(Object.keys(select.value)[0]);
    dispatch.value.department_id = id;
    if (store.state.user.is_super) {
      findID(id);
    }
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
  if (typeof dispatch.value.year == "string") {
    let y = dispatch.value.year.split("/");
    //dispatch.value.year = new Date(arr[1] + "/" + arr[0] + "/" + arr[2]);
  }
  if (issaveDispatch.value) {
    if (typeof dispatch.value.open_date == "string") {
      let arr = dispatch.value.open_date.split("/");
      dispatch.value.open_date = new Date(arr[1] + "/" + arr[0] + "/" + arr[2]);
    }
    if (typeof dispatch.value.end_date == "string") {
      let arr1 = dispatch.value.end_date.split("/");
      dispatch.value.end_date = new Date(
        arr1[1] + "/" + arr1[0] + "/" + arr1[2],
      );
    }
  }
  axios({
    method: issaveDispatch.value ? "put" : "post",
    url:
      baseURL +
      `/api/ca_dispatch_books/${
        issaveDispatch.value ? "Update_dispatch_books" : "Add_dispatch_book"
      }`,
    data: dispatch.value,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success(
          response.config.method == "post"
            ? "Thêm mới khối cơ quan thành công!"
            : "Cập nhật khối cơ quan thành công!",
        );

        loadData(true);
        closeDialog();
      } else {
        let ms = response.data.ms;
        swal.fire({
          title: "Thông báo!",
          text:
            ms.includes("dispatch_book_name") == true
              ? "Tên khối cơ quan không quá 250 ký tự!"
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

const sttDispatch = ref();
//Thêm bản ghi con
const isChirlden = ref(false);

//Sửa bản ghi
const editDispatch = (dataPlace) => {
  select.value = {};
  submitted.value = false;
  dispatch.value = dataPlace;
  isChirlden.value = false;
  if (dataPlace.parent_id != null) {
    isChirlden.value = true;
  }
  if (typeof dataPlace.open_date == "string") {
    let arr1 = dataPlace.open_date.split("/");
    minDate.value = new Date(arr1[1] + "/" + arr1[0] + "/" + arr1[2]);
  }
  dispatch.value.organization_id = store.state.user.is_super
    ? 1
    : store.state.user.organization_id;
  if (dispatch.value.department_id) {
    select.value[dataPlace.department_id || "-1"] = true;
  }
  headerDialog.value = "Sửa khối cơ quan";
  issaveDispatch.value = true;
  displayBasic.value = true;
};
//Xóa bản ghi
const delDispatch = (Dispatch) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá khối cơ quan này không!",
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
          .delete(baseURL + "/api/ca_dispatch_books/Delete_dispatch_books", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: Dispatch != null ? [Dispatch.dispatch_book_id] : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá khối cơ quan thành công!");
              if (
                ((options.value.totalRecords - 1) % 2 == 0 &&
                  options.value.PageNo > 0) ||
                (options.value.totalRecords < options.value.PageSize &&
                  options.value.PageNo != 0)
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
        excelname: "DANH SÁCH SỔ CÔNG VĂN",
        proc: "ca_dispatch_book_listexport",
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
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const filterPhanloai = ref();
const filterSQL = ref([]);
const isFirst = ref(true);
const loadDataSQL = () => {
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
    .post(baseURL + "/api/SQL/Filter_Dispatch", data, config)
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
  options.value.PageNo = 0;
  isDynamicSQL.value = true;
  loadDataSQL();
};
//Checkbox
const onCheckBox = (value) => {
  let data = {
    IntID: value.dispatch_book_id,
    TextID: value.dispatch_book_id + "",
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
        baseURL + "/api/ca_dispatch_books/Update_StatusDispatch_Books",
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
      text: "Bạn không có quyền chỉnh sửa!",
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
          listId.push(item.dispatch_book_id);
        });
        axios
          .delete(baseURL + "/api/ca_dispatch_books/Delete_dispatch_books", {
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
//Filter
const trangThai = ref([
  { name: "Hiển thị", code: 1 },
  { name: "Không hiển thị", code: 0 },
]);

const filterTrangthai = ref();
const first = ref();
const reFilterEmail = (event) => {
  options.value.PageNo = 0;
  first.value = 0;
  options.value = {
    PageNo: 0,
    PageSize: 20,
  };
  filterSQL.value = [];
  filterTrangthai.value = null;
  filterPhanloai.value = null;
  checkFilter.value = false;
  options.value.SearchText = "";
  isDynamicSQL.value = false;
  loadData(true);
  selectedDispatchs.value = [];
  styleObj.value = "";
};
const checkFilter = ref(false);
const filterEmails = (event) => {
  styleObj.value = style.value;
  loadDataSQL();
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
const item = "/Portals/Mau Excel/Mẫu Excel Khối cơ quan.xlsx";
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
      .post(baseURL + "/api/ImportExcel/ImportDispatch", formData, config)
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
const styleObj = ref();
const style = ref({
  "background-color": "#2196F3 !important",
  color: "#fff !important",
  " border": "1px solid #5ca7e3 !important",
});
const ChangeSharedStatus = (value) => {
  let data = {
    IntID: value.dispatch_book_id,
    TextID: value.dispatch_book_id + "",
    IntTrangthai: 1,
    BitTrangthai: value.is_shared,
    check: true,
  };
  if (
    store.state.user.is_super == true ||
    store.state.user.user_id == value.created_by ||
    store.state.user.role_id == "admin"
  ) {
    axios
      .put(
        baseURL + "/api/ca_dispatch_books/Update_StatusDispatch_Books",
        data,
        config,
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Cập nhật trạng thái chia sẻ thành công!");
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
      text: "Bạn không có quyền chỉnh sửa!",
      icon: "error",
      confirmButtonText: "OK",
    });
    loadData(true);
  }
};

onMounted(() => {
  loadData(true);
  loadDonvi();
  loadDonvi1();
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
      dataKey="dispatch_book_id"
      :rowHover="true"
      v-model:filters="filters"
      :showGridlines="true"
      paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
      :rowsPerPageOptions="[20, 30, 50, 100, 200]"
      responsiveLayout="scroll"
    >
      <template #header>
        <h3 class="module-title mt-0 ml-1 mb-2">
          <i class="pi pi-file-pdf"></i>
          Danh sách khối cơ quan ({{ options.totalRecords }})
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
            /><OverlayPanel
              ref="op"
              appendTo="body"
              class="overlay-panel-setup p-0 m-0"
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
              @click="openBasic('Thêm khối cơ quan')"
              label="Thêm mới"
              icon="pi pi-plus"
              class="mr-2"
            />
            <Button
              @click="reFilterEmail"
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
        headerStyle="text-align:center;max-width:75px;height:50px"
        bodyStyle="text-align:center;max-width:75px;max-height:60px"
        class="align-items-center justify-content-center text-center"
        v-if="store.state.user.is_admin == true"
      >
      </Column>
      <Column
        field="STT"
        header="STT"
        :sortable="true"
        headerStyle="text-align:center;max-width:75px;height:50px"
        bodyStyle="text-align:center;max-width:75px;max-height:60px"
        class="align-items-center justify-content-center text-center"
      >
      </Column>

      <Column
        field="dispatch_book_name"
        header="Tên sổ"
        :sortable="true"
        headerStyle="height:50px"
        bodyStyle="max-height:60px"
      >
      </Column>
      <Column
        field="year"
        header="Năm"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:100px;height:50px"
        bodyStyle="text-align:center;max-width:100px;;max-height:60px"
      >
      </Column>
      <Column
        field="current_num"
        header="Số hiện tại"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:150px;height:50px"
        bodyStyle="text-align:center;max-width:150px;;max-height:60px"
      >
      </Column>
      <Column
        field="tracking_place"
        header="Nơi theo dõi"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:150px;height:50px"
        bodyStyle="text-align:center;max-width:150px;;max-height:60px"
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
        field="is_shared"
        header="Chia sẻ"
        headerStyle="text-align:center;max-width:120px;height:50px"
        bodyStyle="text-align:center;max-width:120px;;max-height:60px"
        class="align-items-center justify-content-center text-center"
      >
        <template #body="data">
          <Checkbox
            :binary="data.data.is_shared"
            v-model="data.data.is_shared"
            @click="ChangeSharedStatus(data.data)"
          />
        </template>
      </Column>
      <Column
        header="Chức năng"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:150px;height:50px"
        bodyStyle="text-align:center;max-width:150px;;max-height:60px"
      >
        <template #body="Dispatch">
          <!-- //is_super: adm tổng -->
          <!-- <div
            v-if="
              (store.state.user.IsSupper == true &&
                store.state.user.is_admin == 1) ||
              store.state.user.organization_id ==
                Dispatch.data.organization_id ||
              store.state.user.user_id == Dispatch.data.created_by
            "
          > -->
          <div
            v-if="
              store.state.user.is_super == true ||
              store.state.user.user_id == Dispatch.data.created_by ||
              store.state.user.role_id == 'admin'
            "
          >
            <Button
              @click="editDispatch(Dispatch.data)"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-pencil"
              v-tooltip="'Sửa'"
            ></Button>
            <Button
              @click="delDispatch(Dispatch.data, true)"
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
    :closable="false"
    :header="headerDialog"
    v-model:visible="displayBasic"
    :style="{ width: '40vw' }"
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
                Tên sổ <span class="redsao">(*)</span>
              </div>
              <InputText
                v-model="dispatch.dispatch_book_name"
                spellcheck="false"
                class="col-10 p-0 m-0 ip36 px-2"
              />
            </div>
            <div class="col-12 flex p-0">
              <div class="col-2"></div>
              <small
                v-if="
                  (v$.dispatch_book_name.$invalid && submitted) ||
                  v$.dispatch_book_name.$pending.$response
                "
                class="col-10 p-error p-0"
              >
                <span class="col-12 p-0">{{
                  v$.dispatch_book_name.required.$message
                    .replace("Value", "Tên khối cơ quan")
                    .replace("is required", "không được để trống")
                }}</span>
              </small>
            </div>
            <div class="flex">
              <div class="col-6 p-0 flex">
                <div class="col-12 p-0 my-3 flex">
                  <label class="line-height-4 col-4 p-0">Năm</label>
                  <InputNumber
                    :useGrouping="false"
                    v-model="dispatch.year"
                    spellcheck="false"
                    class="col-8 p-0 ip36"
                  />
                </div>
              </div>
              <div class="col-6 p-0 flex">
                <div class="col-12 p-0 my-3 flex">
                  <label class="line-height-4 col-4"
                    >Số hiện tại <span class="redsao">(*)</span></label
                  >
                  <InputNumber
                    :useGrouping="false"
                    v-model="dispatch.current_num"
                    spellcheck="false"
                    class="col-8 p-0 ip36"
                  />
                </div>
              </div>
            </div>
            <div class="col-12 flex p-0">
              <div class="col-8"></div>
              <small
                v-if="
                  (v$.current_num.$invalid && submitted) ||
                  v$.current_num.$pending.$response
                "
                class="col-4 p-error p-0"
              >
                <span class="col-12 p-0">{{
                  v$.current_num.required.$message
                    .replace("Value", "Số hiện tại")
                    .replace("is required", "không được để trống")
                }}</span>
              </small>
            </div>
            <div class="col-12 p-0 flex">
              <div class="col-6 p-0 my-3 flex">
                <label class="line-height-4 col-4 p-0">Ngày mở</label>
                <Calendar
                  class="col-8 p-0"
                  :showIcon="true"
                  autocomplete="on"
                  v-model="dispatch.open_date"
                />
              </div>
              <div class="col-6 p-0 my-3 flex">
                <label class="line-height-4 col-4">Ngày đóng</label>
                <Calendar
                  class="col-8 p-0"
                  :showIcon="true"
                  autocomplete="on"
                  v-model="dispatch.end_date"
                  :min-date="minDate ? minDate : false"
                />
              </div>
            </div>

            <div class="col-12 p-0 my-3 flex">
              <div class="pb-2 col-2 p-0 line-height-4">Nơi theo dõi</div>
              <InputText
                class="col-10 ip36 px-2"
                v-model="dispatch.tracking_place"
              />
            </div>

            <div class="col-12 p-0 my-3 flex">
              <div class="pb-2 col-2 p-0 line-height-4">Phòng ban</div>
              <!-- <Dropdown
                v-model="dispatch.department_id"
                :options="departmentOptions"
                optionLabel="name"
                optionValue="code"
                placeholder="Chọn phòng ban"
                :filter="true"
                :editable="true"
                class="col-3"
              />
              <br /> -->

              <TreeSelect
                v-model="select"
                :options="donvis"
                optionLabel="data.organization_name"
                optionValue="data.organization_id"
                placeholder="Chọn đơn vị"
                class="col-10 md:col-10"
                :scrollable="true"
              />
            </div>

            <div class="col-12 p-0 my-3 flex">
              <div class="pb-2 col-2 p-0 line-height-4">
                Loại văn bản<span class="redsao">(*)</span>
              </div>
              <Dropdown
                class="col-10 my-class"
                v-model="dispatch.nav_type"
                :options="typeOptions"
                optionLabel="name"
                placeholder="Chọn loại văn bản"
                optionValue="code"
                :filter="true"
              />
            </div>
            <div class="col-12 flex p-0">
              <div class="col-2"></div>
              <small
                v-if="
                  (v$.nav_type.$invalid && submitted) ||
                  v$.nav_type.$pending.$response
                "
                class="col-10 p-error p-0"
              >
                <span class="col-12 p-0">{{
                  v$.nav_type.required.$message
                    .replace("Value", "Loại văn bản")
                    .replace("is required", "không được để trống")
                }}</span>
              </small>
            </div>
            <div class="col-12 p-0 my-3 flex">
              <div class="col-6 flex p-0">
                <div class="pb-2 col-4 p-0 line-height-4">STT</div>
                <InputNumber
                  v-model="dispatch.is_order"
                  class="col-8 p-0 ip36"
                />
              </div>
              <div class="col-6 flex p-0">
                <div class="pb-2 col-4 p-0 line-height-4 px-2">Trạng thái</div>
                <InputSwitch
                  v-model="dispatch.status"
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
</style>
<style lang="scss" scoped></style>
