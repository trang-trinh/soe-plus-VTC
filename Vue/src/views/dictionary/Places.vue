<script setup>
import { ref, inject, onMounted } from "vue";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
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
const item = "/Portals/Mau Excel/Mẫu Excel địa danh.xlsx";
const rules = {
  name: {
    required,
    $errors: [
      {
        $property: "name",
        $validator: "required",
        $message: "Tên địa danh không được để trống!",
      },
    ],
  },
};
const place = ref({
  name: "",
  status: true,
  is_level: 1,
  is_order: 1,
});
const submitted = ref(false);
const v$ = useVuelidate(rules, place);
const isSavePlace = ref(false);
const datalists = ref();
const toast = useToast();
const basedomainURL = baseURL;

const selectedKeys = ref([]);
const options = ref({
  sort: "v.is_order asc",
  SearchText: "",
  PageNo: 0,
  PageSize: 20,
  FilterUsers_ID: null,
  loading: true,
  totalRecords: null,
});
const onNodeSelect = (node) => {
  if (expandedKeys.value[node.data.place_id] == true) {
    expandedKeys.value[node.data.place_id] = false;
  } else {
    expandedKeys.value[node.data.place_id] = true;
  }
};
const selectedNodes = ref();
const onNodeUnselect = (node) => {
  selectedNodes.value.splice(selectedNodes.value.indexOf(node.data.Menu_ID), 1);
};
const expandedKeys = ref({});
//Thêm log
const addLog = (log) => {
  axios.post(baseURL + "/api/Proc/AddLog", log, config);
};
const addUserLog = (log) => {
  axios.post(baseURL + "/api/Proc/AddUserLog", log, config);
};
//Lấy số bản ghi
const loadCount = () => {
  axios
    .post(
      baseURL + "/api/DictionaryProc/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "ca_places_count",
            par: [
              { par: "parent_id", va: null },
              { par: "search", va: options.value.SearchText },
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
const RenderData = (response) => {
  options.value.allRecord = null;
  let list1 = [];
  let list2 = [];
  let list3 = [];
  let d1 = JSON.parse(response.data.data)[0];
  d1.forEach((element, i) => {
    let c = {
      key: element.place_id,
      data: {
        place_id: element.place_id,
        parent_id: element.parent_id,
        name: element.name,
        status: element.status,
        is_order: element.is_order,
        is_level: element.is_level,
        STT: null,
        created_by: element.created_by,
      },
      children: null,
    };
    if (options.value.PageNo > 0) {
      c.data.STT = options.value.PageNo * options.value.PageSize + i + 1;
    } else {
      c.data.STT = i + 1;
    }
    if (d1[i].children) {
      list2 = JSON.parse(d1[i].children);
      if (list2 != null) {
        list2.forEach((element, i) => {
          //đổi dạng stt=> true/false
          if (element.data.status == 1) {
            element.data.status = true;
          } else {
            element.data.status = false;
          }
          //đổi is_order
          element.data.STT = c.data.STT + "." + (i + 1);
          let temp = list2[i].data.STT;
          if (list2[i].children != null) {
            list3 = list2[i].children;
            list3.forEach((element, i) => {
              element.data.STT = temp + "." + (i + 1);
              if (element.data.status == 1) {
                element.data.status = true;
              } else {
                element.data.status = false;
              }
            });
            list2[i].children = list3;
          }
        });
      }
      c.children = list2;
    }
    list1.push(c);
  });
  datalists.value = list1;
  if (JSON.parse(response.data.data)[1]) {
    let data2 = JSON.parse(response.data.data)[1];
    options.value.allRecord = data2[0].allRecord;
  } else {
    options.value.allRecord = datalists.value.length;
  }
};
//Lấy dữ liệu ngôn ngữ
const loadData = (rf) => {
  if (rf) {
    if (isDynamicSQL.value) {
      loadDataSQL();
      return;
    }
    loadCount();
    axios
      .post(
        baseURL + "/api/DictionaryProc/getData",
        {
          str: encr(
            JSON.stringify({
              proc: "ca_places_list",
              par: [
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
        RenderData(response);
        options.value.loading = false;
      })
      .catch((error) => {
        toast.error("Tải dữ liệu không thành công!");
        options.value.loading = false;
        addLog({
          title: "Lỗi Console loadData",
          controller: "PlacesView.vue",
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
const headerDialog = ref();
const displayBasic = ref(false);
const displayStt = ref();
const openBasic = (str) => {
  submitted.value = false;
  place.value = {
    name: "",
    is_order: true,
    status: true,
    is_level: 1,
  };
  if (options.value.totalRecords > 0) {
    place.value.is_order = options.value.totalRecords + 1;
  } else {
    place.value.is_order = 1;
  }
  displayStt.value = place.value.is_order;
  isSavePlace.value = false;
  isChirlden.value = false;
  headerDialog.value = str;
  displayBasic.value = true;
};
const closeDialog = () => {
  place.value = {
    name: "",
    is_order: 1,
    status: true,
  };
  displayBasic.value = false;
  loadData(true);
};

const handleFileUpload = (event) => {
  files = event.target.files;
  var output = document.getElementById("logoLang");
  output.src = URL.createObjectURL(event.target.files[0]);
  output.onload = function () {
    URL.revokeObjectURL(output.src); // free memory
  };
};
//Thêm bản ghi
const savePlace = (isFormValid) => {
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
  if (!isSavePlace.value) {
    axios
      .post(baseURL + "/api/ca_places/Add_ca_places", place.value, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Thêm địa danh thành công!");
          loadData(true);
          closeDialog();
        } else {
          let ms = response.data.ms;
          swal.fire({
            title: "Thông báo!",
            text:
              ms.includes("name") == true
                ? "Tên địa danh không quá 500 ký tự!"
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
  } else {
    axios
      .put(baseURL + "/api/ca_places/Update_ca_places", place.value, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa địa danh thành công!");
          loadData(true);
          closeDialog();
        } else {
          let ms = response.data.ms;
          swal.fire({
            title: "Thông báo!",
            text:
              ms.includes("name") == true
                ? "Tên địa danh không quá 500 ký tự!"
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

const sttChirl = ref();
const nameParent = ref();
const idParent = ref();
//Thêm bản ghi con
const AddChirl = (value) => {
  (async () => {
    await axios
      .post(
        baseURL + "/api/DictionaryProc/getData",
        {
          str: encr(
            JSON.stringify({
              proc: "ca_places_count",
              par: [
                { par: "parent_id", va: value.place_id },
                { par: "search", va: options.value.SearchText },
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
        sttChirl.value = data[0].totalRecords + 1;
      })
      .catch((error) => {
        toast.error("Tải dữ liệu không thành công!");
        options.value.loading = false;
        addLog({
          title: "Lỗi Menu Count",
          controller: "PlaceView.vue",
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
    place.value = {
      name: "",
      status: true,
      is_level: parseInt(value.is_level) + 1,
      parent_id: value.place_id,
      is_order: sttChirl.value,
    };

    displayStt.value = value.STT + "." + place.value.is_order;
    nameParent.value = value.name;
    isChirlden.value = true;
    submitted.value = false;
    headerDialog.value = "Thêm địa danh con";
    isSavePlace.value = false;
    idParent.value = value.place_id;
    displayBasic.value = true;
  })();
};
const isChirlden = ref(false);

//Sửa bản ghi
const editPlace = (dataPlace) => {
  submitted.value = false;
  place.value = dataPlace;

  isChirlden.value = false;
  if (dataPlace.parent_id != null) {
    isChirlden.value = true;
    axios
      .post(
        baseURL + "/api/DictionaryProc/getData",
        {
          str: encr(
            JSON.stringify({
              proc: "ca_places_get",
              par: [{ par: "place_id", va: dataPlace.parent_id }],
            }),
            SecretKey,
            cryoptojs,
          ).toString(),
        },
        config,
      )
      .then((response) => {
        let data = JSON.parse(response.data.data)[0];
        nameParent.value = data[0].name;
      })
      .catch((error) => {
        toast.error("Tải dữ liệu không thành công!");
        options.value.loading = false;
        addLog({
          title: "Lỗi Menu Count",
          controller: "PlaceView.vue",
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
  headerDialog.value = "Sửa địa danh";
  isSavePlace.value = true;
  displayBasic.value = true;
};
//Xóa bản ghi
const delPlace = (Place) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá địa danh này không!",
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
          .delete(baseURL + "/api/ca_places/Delete_ca_places", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: Place != null ? [Place.place_id] : 1,
          })
          .then((response) => {
            swal.close();

            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá địa danh thành công!");
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
      ImportExcel(event);
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
  if (filterHienthi.value == undefined) {
    filterHienthi.value = null;
  }
  if (filterPhanLoai.value == undefined) {
    filterPhanLoai.value = null;
  }
  filterHienthi.value =
    filterHienthi.value != null
      ? filterHienthi.value == 1
        ? true
        : false
      : null;
  axios
    .post(
      baseURL + "/api/Excel/ExportExcel",
      {
        excelname: "DANH SÁCH ĐỊA DANH_",
        proc: "ca_place_list_export_all",
        par: [
          { par: "s", va: options.value.SearchText },
          { par: "lv", va: filterPhanLoai.value },
        ],
      },
      config,
    )
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        swal.close();
        toast.success("Kết xuất Data thành công!");
        // let domst = response.data.path;
        // window.open(baseURL + domst);
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
//Sort;
const onSort = (event) => {
  if (event.sortField == "STT") {
    options.value.sort = "v.is_order";
  } else {
    options.value.sort = "v.name";
  }
  options.value.sort += event.sortOrder == 1 ? " asc" : " desc";
  options.value.PageNo = 0;
  isDynamicSQL.value = true;
  loadData(true);
};
const isFirst = ref(true);
const filterPhanLoai = ref();
const loadDataSQL = () => {
  datalists.value = [];
  if (filterHienthi.value == undefined) {
    filterHienthi.value = null;
  }
  if (filterPhanLoai.value == undefined || filterPhanLoai.value == "") {
    filterPhanLoai.value = null;
  }
  filterHienthi.value =
    filterHienthi.value != null
      ? filterHienthi.value == 1
        ? true
        : false
      : null;
  axios
    .post(
      baseURL + "/api/DictionaryProc/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "Find_place",
            par: [
              { par: "s", va: options.value.SearchText },
              { par: "lv", va: filterPhanLoai.value },
              { par: "pageno", va: options.value.PageNo },
              { par: "pagesize", va: options.value.PageSize },
              { par: "col", va: options.value.sort },
            ],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      RenderData(response);
      options.value.loading = false;
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;
      addLog({
        title: "Lỗi Console loadData",
        controller: "PlacesView.vue",
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
const searchPlaces = () => {
  {
    options.value.PageNo = 0;
    isDynamicSQL.value = true;
    loadData(true);
  }
};

//Checkbox
const onCheckBox = (value) => {
  expandedKeys.value[value.place_id] = false;
  let data = {
    IntID: value.place_id,
    TextID: value.place_id + "",
    IntTrangthai: 1,
    BitTrangthai: value.status == true ? 1 : 0,
  };
  if (
    store.state.user.is_super == true ||
    store.state.user.user_id == value.created_by ||
    store.state.user.role_id == "admin"
  ) {
    axios
      .put(baseURL + "/api/ca_places/Update_StatusPlace", data, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa địa danh thành công!");
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

const Imp = ref(false);
const ImportExcel = () => {
  if (store.state.user.is_super) {
    Imp.value = true;
    files = [];
  } else {
    swal.close();
    swal.fire({
      title: "Thông báo",
      text: "Chỉ có quản trị viên hệ thống mới được sử dụng chức năng này!",
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
      .post(baseURL + "/api/ImportExcel/Import_Place", formData, config)
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
const first = ref();
const reload = () => {
  isDynamicSQL.value = false;
  options.value.SearchText = "";
  filterPhanLoai.value = "";
  styleObj.value = "";
  options.value = {
    PageNo: 0,
    PageSize: 20,
  };
  first.value = 0;
  selectedKeys.value = [];
  loadData(true);
};
const op = ref();
const filterHienthi = ref();
const toggle = (event) => {
  op.value.toggle(event);
};
const phanLoai = ref([
  { name: "Hiển thị", code: 1 },
  { name: "Không hiển thị", code: 0 },
]);
const level = ref([
  { name: "Tỉnh Thành phố", code: 1 },
  { name: "Quận Huyện", code: 2 },
  { name: "Xã Phường", code: 3 },
]);
const styleObj = ref();
const reFilter = (event) => {
  filterPhanLoai.value = null;
  filterHienthi.value = null;
  loadData(true);
  styleObj.value = null;
};
const filter = () => {
  if (
    (options.value.SearchText == "" || options.value.SearchText == null) &&
    (filterHienthi.value == null || filterHienthi.value == undefined) &&
    (filterPhanLoai.value == null || filterPhanLoai.value == undefined)
  ) {
    isDynamicSQL.value = false;
  } else {
    isDynamicSQL.value = true;
  }
  loadData(true);
  styleObj.value = style.value;
};
const style = ref({
  "background-color": "#2196F3 !important",
  color: "#fff !important",
  " border": "1px solid #5ca7e3 !important",
});

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
    handleFileUpload,
    savePlace,
    isFirst,
    searchPlaces,
    onCheckBox,
  };
});
</script>
<template>
  <div class="main-layout true flex-grow-1 p-2">
    <TreeTable
      sortMode="single"
      ref="dt"
      @nodeSelect="onNodeSelect"
      @nodeUnselect="onNodeUnselect"
      @page="onPage($event)"
      @sort="onSort($event)"
      :value="datalists"
      :paginator="true"
      :rows="options.PageSize"
      paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
      :rowsPerPageOptions="[20, 30, 50, 100, 200]"
      :scrollable="true"
      scrollHeight="flex"
      v-model:selectionKeys="selectedKeys"
      :loading="options.loading"
      :expandedKeys="expandedKeys"
      :rowHover="true"
      :showGridlines="true"
      responsiveLayout="scroll"
      :lazy="true"
      :totalRecords="options.allRecord"
      selectionMode="multiple"
    >
      <template #header>
        <h3 class="module-title mt-0 ml-1 mb-2">
          <i class="pi pi-globe"></i> Danh sách địa danh ({{
            options.allRecord
          }})
        </h3>
        <Toolbar class="w-full custoolbar">
          <template #start>
            <span class="p-input-icon-left">
              <i class="pi pi-search" />
              <InputText
                v-model="options.SearchText"
                @keypress.enter="searchPlaces"
                type="text"
                spellcheck="false"
                placeholder=" Tìm kiếm địa danh"
              />
            </span>
            <Button
              type="button"
              class="ml-2 p-button-outlined p-button-secondary"
              icon="pi pi-filter"
              @click="toggle"
              aria:haspopup="true"
              :style="[styleObj]"
              aria-controls="overlay_panel"
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
                    class="col-5 text-left pt-2 p-0"
                    style="text-align: left"
                  >
                    Cấp hành chính
                  </div>
                  <div class="col-7">
                    <Dropdown
                      class="col-12 p-0 m-0"
                      v-model="filterPhanLoai"
                      :options="level"
                      optionLabel="name"
                      optionValue="code"
                      placeholder="Cấp hành chính"
                    />
                  </div>
                </div>
                <!-- <div class="flex field col-12 p-0">
                  <div
                    class="col-4 text-left pt-2 p-0"
                    style="text-align: left"
                  >
                    Trạng thái
                  </div>
                  <div class="col-8">
                    <Dropdown
                      class="col-12 p-0 m-0"
                      v-model="filterHienthi"
                      :options="phanLoai"
                      optionLabel="name"
                      optionValue="code"
                      placeholder="Trạng thái"
                    />
                  </div>
                </div> -->
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
              @click="openBasic('Thêm mới')"
              label="Thêm mới"
              icon="pi pi-plus"
              class="mr-2"
              v-if="
                store.state.user.is_super &&
                store.state.user.organization_id == null
              "
            />
            <Button
              @click="reload"
              class="mr-2 p-button-outlined p-button-secondary"
              v-tooltip="'Tải lại'"
              icon="pi pi-refresh"
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
        field="STT"
        header="STT"
        :sortable="true"
        headerStyle="text-align:center;max-width:75px;height:50px"
        bodyStyle="text-align:center;max-width:75px;;max-height:60px"
        class="align-items-center justify-content-center text-center"
      >
        <template #body="menu">
          <div
            v-if="menu.node.data.parent_id == null"
            @click="onNodeSelect(menu.node.data)"
            style="font-weight: 1000"
          >
            {{ menu.node.data.STT }}
          </div>
          <div
            v-else
            style="font-weight: 600"
          >
            {{ menu.node.data.STT }}
          </div>
        </template>
      </Column>

      <Column
        field="name"
        header="Tên địa danh"
        :expander="true"
        :sortable="true"
        headerStyle="height:50px;max-width:auto;"
        bodyStyle="max-height:60px"
      >
      </Column>

      <Column
        field="is_level"
        header="Cấp hành chính"
        headerStyle="text-align:center;max-width:150px;height:50px"
        bodyStyle="text-align:center;max-width:150px;max-height:60px"
        class="align-items-center justify-content-center text-center"
      >
        <template #body="data">
          <div v-if="data.node.data.is_level == 1">Tỉnh/Thành phố</div>
          <div v-else-if="data.node.data.is_level == 2">Quận/Huyện</div>
          <div v-else>Xã/Phường</div>
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
            :binary="data.node.data.status"
            v-model="data.node.data.status"
            @click="onCheckBox(data.node.data)"
          /> </template
      ></Column>

      <Column
        header="Chức năng"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:200px;height:50px"
        bodyStyle="text-align:center;max-width:200px;;max-height:60px"
        v-if="
          store.state.user.is_super == true &&
          store.state.user.organization_id == null
        "
      >
        <template #body="Place">
          <div v-if="store.state.user.is_super == true">
            <Button
              v-if="Place.node.data.is_level < '3'"
              @click="AddChirl(Place.node.data)"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-plus-circle"
              v-tooltip="'Thêm địa danh con'"
            ></Button>
          </div>
          <div>
            <Button
              @click="editPlace(Place.node.data)"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-pencil"
              v-tooltip="'Sửa'"
            ></Button>
            <Button
              @click="delPlace(Place.node.data, true)"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              v-tooltip="'Xóa'"
              icon="pi pi-trash"
            ></Button>
          </div>
        </template>
      </Column>
      <template #empty>
        <div
          class="align-items-center justify-content-center p-4 text-center m-auto"
          v-if="!isFirst || options.allRecord == 0"
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
    :header="headerDialog"
    v-model:visible="displayBasic"
    :style="{ width: '40vw' }"
    :closable="false"
  >
    <form>
      <div class="grid formgrid m-2">
        <div
          v-if="isChirlden"
          class="field col-12 md:col-12"
        >
          <label class="col-3 text-left p-0"
            >Cấp cha<span class="redsao"></span
          ></label>
          <InputText
            v-model="nameParent"
            :disabled="true"
            class="col-8 ip36 px-2"
          />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-3 text-left p-0"
            >Tên địa danh <span class="redsao">(*)</span></label
          >
          <InputText
            v-model="place.name"
            spellcheck="false"
            class="col-8 ip36 px-2"
            :class="{ 'p-invalid': v$.name.$invalid && submitted }"
          />
        </div>
        <div
          style="display: flex"
          class="field col-12 md:col-12"
        >
          <div class="col-4 text-left"></div>
          <small
            v-if="(v$.name.$invalid && submitted) || v$.name.$pending.$response"
            class="col-8 p-error"
          >
            <span class="col-12 p-0">{{
              v$.name.required.$message
                .replace("Value", "Tên địa danh")
                .replace("is required", "không được để trống")
            }}</span>
          </small>
        </div>
        <div
          style="display: flex"
          class="col-12 field md:col-12"
        >
          <div class="field col-6 md:col-6 p-0">
            <label class="col-6 text-left p-0">STT </label>
            <InputNumber
              v-model="displayStt"
              class="col-6 ip36"
              v-if="openBasic == true"
            />
            <InputNumber
              v-model="place.is_order"
              class="col-6 ip36"
              v-else
            />
          </div>
          <div class="field col-6 md:col-6 p-0">
            <label
              style="vertical-align: text-bottom"
              class="col-6 text-center p-0"
              >Trạng thái
            </label>
            <InputSwitch v-model="place.status" />
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
        @click="savePlace(!v$.$invalid)"
      />
    </template>
  </Dialog>
</template>

<style scoped></style>
