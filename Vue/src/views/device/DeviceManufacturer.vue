<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import { encr, checkURL } from "../../util/function.js";
//Khai báo
const router = inject("router");
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
  device_manufacturer_name: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
  device_manufacturer_code: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
});
const rules = {
  device_manufacturer_name: {
    required,
    $errors: [
      {
        $property: "device_manufacturer_name",
        $validator: "required",
        $message: "Tên hãng sản xuất không được để trống!",
      },
    ],
  },
  device_manufacturer_code: {
    required,
    $errors: [
      {
        $property: "device_manufacturer_code",
        $validator: "required",
        $message: "Mã hãng sản xuất không được để trống!",
      },
    ],
  },
};
const opColor = ref();
let pcolor = "";
//Thêm log

const addLog = (log) => {
  axios.post(baseURL + "/api/Proc/AddLog", log, config);
};
//Lấy số bản ghi
const loadCount = () => {
  axios
    .post(
      baseURL + "/api/device_card/getData",
        {
          str: encr(
            JSON.stringify({
        proc: "device_manufacturer_count",
        par: [
          { par: "user_id", va: store.getters.user.user_id },
          { par: "status", va: options.value.Status },
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
        sttStamp.value = data[0].totalRecords + 1;
      }
    })
    .catch((error) => {
      addLog({
        title: "Lỗi Console loadCount",
        controller: "TemView.vue",
        log_content: error.message,
      });
    });
};
//Lấy dữ liệu manufacturer
const loadData = (rf) => {
  if (rf) {
    if (isDynamicSQL.value) {
      loadDataSQL();
      return false;
    }
    if (rf) {
      if (options.value.PageNo == 0) {
        loadCount();
      }
    }
    axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
          proc: "device_manufacturer_list",
          par: [
            { par: "pageno", va: options.value.PageNo },
            { par: "pagesize", va: options.value.PageSize },
            { par: "user_id", va: store.getters.user.user_id },
            { par: "status", va: options.value.Status },
          ],
        }),
            SecretKey,
            cryoptojs
          ).toString(),
        },config
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
          controller: "TemView.vue",
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

    options.value.id =
      datalists.value[datalists.value.length - 1].device_manufacturer_id;
    options.value.IsNext = true;
  } else if (event.page < options.value.PageNo) {
    //Trang trước
    options.value.id = datalists.value[0].device_manufacturer_id;
    options.value.IsNext = false;
  }
  options.value.PageNo = event.page;
  loadData(true);
};

const manufacturer = ref({
  device_manufacturer_name: "",
  emote_file: "",
  status: true,
  is_default: false,
  is_order: 1,
});

const selectedStamps = ref();
const submitted = ref(false);
const v$ = useVuelidate(rules, manufacturer);
const isSaveTem = ref(false);
const datalists = ref();
const toast = useToast();
const basedomainURL = baseURL;
const checkDelList = ref(false);

const options = ref({
  IsNext: true,
  sort: "created_date",
  SearchText: "",
  PageNo: 0,
  PageSize: 20,
  loading: true,
  totalRecords: null,
});

//Hiển thị dialog
const headerDialog = ref();
const displayBasic = ref(false);
const openBasic = (str) => {
  submitted.value = false;
  manufacturer.value = {
    device_manufacturer_name: "",
    emote_file: "",
    status: true,
    is_default: false,
    is_order: sttStamp.value,
    organization_id: store.getters.user.organization_id,
  };

  checkIsmain.value = false;
  isSaveTem.value = false;
  headerDialog.value = str;
  displayBasic.value = true;
};
const selectedCountry = ref();
const countries = ref([
  { name: "Nga", code: "US" },
  { name: "Canada", code: "US" },
  { name: "Hoa Kỳ", code: "US" },
  { name: "Trung Quốc", code: "US" },
  { name: "Brasil", code: "US" },
  { name: "Úc", code: "US" },
  { name: "Ấn Độ", code: "US" },
  { name: " Argentina", code: "US" },
  { name: "Kazakhstan", code: "US" },
  { name: "Algérie", code: "US" },
  { name: "Cộng hòa Dân chủ Congo", code: "US" },
  { name: "Greenland", code: "US" },
  { name: "Ả Rập Xê Út", code: "US" },
  { name: "México", code: "US" },
  { name: "Indonesia", code: "US" },
  { name: "Sudan", code: "US" },
  { name: "Việt Nam", code: "US" },
  { name: "Nhật Bản", code: "US" },
  { name: "Thụy Điển", code: "US" },
  { name: "Thụy Sĩ", code: "US" },
  { name: "Hàn quốc", code: "US" },
  { name: "Anh Quốc", code: "US" },
  { name: "Lào", code: "US" },
  { name: "Pháp", code: "US" },
  { name: "Thái lan", code: "US" },
]);
const closeDialog = () => {
  manufacturer.value = {
    device_manufacturer_name: "",
    emote_file: "",
    status: true,
    is_default: false,
    is_order: 1,
  };
   files.value = [];
  displayBasic.value = false;
  loadData(true);
};
//Lấy file logo
const chonanh = (id) => {
  document.getElementById(id).click();
};
const handleFileUpload = (event) => {
  files.value = event.target.files;
 
  var output = document.getElementById("logoTem");
  output.src = URL.createObjectURL(event.target.files[0]);
  output.onload = function () {
    URL.revokeObjectURL(output.src); // free memory
  };
};
//Thêm bản ghi
const  files =ref( []);
const sttStamp = ref(1);
const saveData = (isFormValid) => {
  submitted.value = true;
  if (!isFormValid) {
    return;
  }
  if (manufacturer.value.device_manufacturer_code.length > 50) {
    swal.fire({
      title: "Error!",
      text: "Mã hãng sản xuất không được vượt quá 50 ký tự!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  if (manufacturer.value.device_manufacturer_name.length > 250) {
    swal.fire({
      title: "Error!",
      text: "Tên hãng sản xuất không được vượt quá 250 ký tự!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  let formData = new FormData();
  for (var i = 0; i <  files.value.length; i++) {
    let file =  files.value[i];
    formData.append("logo", file);
  }

  if (manufacturer.value.countryside_fake)
    manufacturer.value.countryside = manufacturer.value.countryside_fake;
  formData.append("manufacturer", JSON.stringify(manufacturer.value));
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  if (!isSaveTem.value) {
    axios
      .post(
        baseURL + "/api/device_manufacturer/add_device_manufacturer",
        formData,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Thêm hãng sản xuất thành công!");
          loadData(true);
           files.value = [];
          closeDialog();
        } else {
          swal.fire({
            title: "Error!",
            text: response.data.ms,
            icon: "error",
            confirmButtonText: "OK",
          });
        }
      })
      .catch((error) => {
        swal.close();
        swal.fire({
          title: "Error!",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
      });
  } else {
    axios
      .put(
        baseURL + "/api/device_manufacturer/update_device_manufacturer",
        formData,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa hãng sản xuất thành công!");

           files.value = [];
          closeDialog();
        } else {
          swal.fire({
            title: "Error!",
            text: response.data.ms,
            icon: "error",
            confirmButtonText: "OK",
          });
        }
      })
      .catch((error) => {
        swal.close();
        swal.fire({
          title: "Error!",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
      });
  }
};
const checkIsmain = ref(true);
//Sửa bản ghi
const editTem = (dataTem) => {
  submitted.value = false;
  manufacturer.value = dataTem;
  if (manufacturer.value.countryside)
    manufacturer.value.countryside_fake = manufacturer.value.countryside;
  if (manufacturer.value.is_default) {
    checkIsmain.value = false;
  } else {
    checkIsmain.value = true;
  }
  headerDialog.value = "Sửa hãng sản xuất";
  isSaveTem.value = true;
  displayBasic.value = true;
  if (store.state.user.is_admin) {
    manufacturer.value.organization_id = 0;
  } else {
    manufacturer.value.organization_id = 1;
  }
};
//Xóa bản ghi
const delTem = (Tem) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá bản ghi này không!",
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
          .delete(
            baseURL + "/api/device_manufacturer/delete_device_manufacturer",
            {
              headers: { Authorization: `Bearer ${store.getters.token}` },
              data: Tem != null ? [Tem.device_manufacturer_id] : 1,
            }
          )
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá hãng sản xuất thành công!");
              loadData(true);
            } else {
              swal.fire({
                title: "Error!",
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
                text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
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
        excelname: "DANH SÁCH TEM",
        proc: "ca_emote_listexport",
        par: [{ par: "search", va: options.value.SearchText }],
      },
      config
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
          title: "Error!",
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
const checkFilter = ref(false);
const filterSQL = ref([]);
const isFirst = ref(true);
const loadDataSQL = () => {
  datalists.value = [];

  let data = {
    id: "device_manufacturer_id",
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
    .post(baseURL + "/api/SQL/Filter_device_manufacturer", data, config)
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
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
//Tìm kiếm
const searchStamp = (event) => {
  if (event.code == "Enter") {
    if (options.value.SearchText == "") {
      isDynamicSQL.value = false;
      options.value.loading = true;
      loadData(true);
    } else {
      isDynamicSQL.value = true;
      options.value.loading = true;
      loadData(true);
    }
  }
};
const refreshStamp = () => {
  options.value.SearchText = "";
  filterTrangthai.value = "";
  options.value.loading = true;
  selectedStamps.value = [];
  isDynamicSQL.value = false;
  loadData(true);
};
const onFilter = (event) => {
  filterSQL.value = [];

  for (const [key, value] of Object.entries(event.filters)) {
    if (key != "global") {
      let obj = {
        key: key,
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
const onCheckBox = (value, check, checkIsmain) => {
  if (check) {
    let data = {
      IntID: value.device_manufacturer_id,
      TextID: value.device_manufacturer_id + "",
      IntTrangthai: 1,
      BitTrangthai: value.status,
    };
    axios
      .put(
        baseURL + "/api/device_manufacturer/update_s_device_manufacturer",
        data,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa trạng thái hãng sản xuất thành công!");
          loadData(true);
          closeDialog();
        } else {
          swal.fire({
            title: "Error!",
            text: response.data.ms,
            icon: "error",
            confirmButtonText: "OK",
          });
        }
      })
      .catch((error) => {
        swal.close();
        swal.fire({
          title: "Error!",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
      });
  } else {
    let data1 = {
      IntID: value.device_manufacturer_id,
      TextID: value.device_manufacturer_id + "",
      BitMain: value.is_default,
    };
    axios
      .put(
        baseURL + "/api/device_manufacturer/Update_DefaultStamp",
        data1,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa trạng thái hãng sản xuất thành công!");
          loadData(true);
          closeDialog();
        } else {
          swal.fire({
            title: "Error!",
            text: response.data.ms,
            icon: "error",
            confirmButtonText: "OK",
          });
        }
      })
      .catch((error) => {
        swal.close();
        swal.fire({
          title: "Error!",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
      });
  }
};
//Xóa nhiều
const deleteList = () => {
  let listId = new Array(selectedStamps.value.length);
  let checkD = false;
  selectedStamps.value.forEach((item) => {
    if (item.is_default) {
      toast.error("Không được xóa hãng sản xuất mặc định!");
      checkD = true;
      return;
    }
  });
  if (!checkD) {
    swal
      .fire({
        title: "Thông báo",
        text: "Bạn có muốn xoá hãng sản xuất này không!",
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

          selectedStamps.value.forEach((item) => {
            listId.push(item.device_manufacturer_id);
          });
          axios
            .delete(
              baseURL + "/api/device_manufacturer/delete_device_manufacturer",
              {
                headers: { Authorization: `Bearer ${store.getters.token}` },
                data: listId != null ? listId : 1,
              }
            )
            .then((response) => {
              swal.close();
              if (response.data.err != "1") {
                swal.close();
                toast.success("Xoá hãng sản xuất thành công!");
                checkDelList.value = false;

                loadData(true);
              } else {
                swal.fire({
                  title: "Error!",
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
                  title: "Error!",
                  text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
                  icon: "error",
                  confirmButtonText: "OK",
                });
              }
            });
        }
      });
  }
};

//Filter
const trangThai = ref([
  { name: "Hiển thị", code: 1 },
  { name: "Không hiển thị", code: 0 },
]);
const phanLoai = ref([
  { name: "Hệ thống", code: 0 },
  { name: "Đơn vị", code: 1 },
]);

const filterTrangthai = ref();

const reFilterEmail = () => {
  filterTrangthai.value = null;
  isDynamicSQL.value = false;
  checkFilter.value = false;
  op.value.hide();
  loadData(true);
};
const filterFileds = () => {
  filterSQL.value = [];
  checkFilter.value = true;
  let filterS = {
    filterconstraints: [{ value: filterTrangthai.value, matchMode: "equals" }],
    filteroperator: "and",
    key: "status",
  };
  filterSQL.value.push(filterS);
  loadDataSQL();
};
watch(selectedStamps, () => {
  if (selectedStamps.value.length > 0) {
    checkDelList.value = true;
  } else {
    checkDelList.value = false;
  }
});
const op = ref();
const toggle = (event) => {
  op.value.toggle(event);
};
const deleteemote_file = () => {
   files.value = [];
  manufacturer.value.logo = null;
  var output = document.getElementById("logoTem");
  output.src = basedomainURL + '/Portals/Image/noimg.jpg';
 
};
onMounted(() => {  if (!checkURL(window.location.pathname, store.getters.listModule)) {
     //router.back();
  }
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
    saveData,
    isFirst,
    searchStamp,
    onCheckBox,
    selectedStamps,
    deleteList,
  };
});
</script>
    <template>
  <div class="main-layout true flex-grow-1 p-2 pb-0 pr-0">
    <DataTable
      @page="onPage($event)"
      @sort="onSort($event)"
      @filter="onFilter($event)"
      v-model:filters="filters"
      filterDisplay="menu"
      filterMode="lenient"
      :filters="filters"
      :scrollable="true"
      scrollHeight="flex"
      :showGridlines="true"
      columnResizeMode="fit"
      :lazy="true"
      :totalRecords="options.totalRecords"
      :loading="options.loading"
      :reorderableColumns="true"
      :value="datalists"
      removableSort
      v-model:rows="options.PageSize"
      paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
      :rowsPerPageOptions="[20, 30, 50, 100, 200]"
      :paginator="true"
      dataKey="device_manufacturer_id"
      responsiveLayout="scroll"
      v-model:selection="selectedStamps"
      :row-hover="true"
    >
      <template #header>
        <h3 class="module-title mt-0 ml-1 mb-2">
          <i class="pi pi-money-bill"></i> Danh sách hãng sản xuất ({{
            options.totalRecords
          }})
        </h3>
        <Toolbar class="w-full custoolbar">
          <template #start>
            <span class="p-input-icon-left">
              <i class="pi pi-search" />
              <InputText
                v-model="options.SearchText"
                @keyup="searchStamp"
                type="text"
                spellcheck="false"
                placeholder="Tìm kiếm"
              />
              <Button
                type="button"
                class="ml-2"
                icon="pi pi-filter"
                @click="toggle"
                aria:haspopup="true"
                aria-controls="overlay_panel"
                v-tooltip="'Bộ lọc'"
                :class="
                  filterTrangthai != null && checkFilter
                    ? ''
                    : 'p-button-secondary p-button-outlined'
                "
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
                          @click="reFilterEmail"
                          class="p-button-outlined"
                          label="Xóa"
                        ></Button>
                      </template>
                      <template #end>
                        <Button @click="filterFileds" label="Lọc"></Button>
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
              @click="openBasic('Thêm hãng sản xuất')"
              label="Thêm mới"
              icon="pi pi-plus"
              class="mr-2"
            />
            <Button
              @click="refreshStamp"
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
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:70px;height:50px"
        bodyStyle="text-align:center;max-width:70px"
        selectionMode="multiple"
      >
      </Column>

      <Column
        field="STT"
        header="STT"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:70px;height:50px"
        bodyStyle="text-align:center;max-width:70px"
        :sortable="true"
      ></Column>
      <Column
        field="device_manufacturer_code"
        header="Mã hãng"
        :sortable="true"
        headerStyle="text-align:center;height:50px;max-width:150px"
        bodyStyle="text-align:center;max-width:150px"
        class="align-items-center justify-content-center text-center"
      >
        <template #filter="{ filterModel }">
          <InputText
            type="text"
            v-model="filterModel.value"
            class="p-column-filter"
            placeholder="Từ khoá"
          />
        </template>
      </Column>
      <Column
        field="device_manufacturer_name"
        header="Tên hãng sản xuất"
        :sortable="true"
        headerStyle="text-align:left;height:50px"
        bodyStyle="text-align:left"
      >
        <template #filter="{ filterModel }">
          <InputText
            type="text"
            v-model="filterModel.value"
            class="p-column-filter"
            placeholder="Từ khoá"
          />
        </template>
      </Column>

      <Column
        field="logo"
        header="Hình ảnh"
        headerStyle="text-align:center;max-width:150px;height:50px"
        bodyStyle="text-align:center;max-width:150px"
        class="align-items-center justify-content-center text-center"
      >
        <template #body="logo">
          <Image
            v-if="logo.data.logo"
            image-style="object-fit: cover; border: unset; outline: unset"
            width="100"
            height="50"
            alt=" "
            v-bind:src="
              logo.data.logo
                ? basedomainURL + logo.data.logo
                : basedomainURL + '/Portals/Image/noimg.jpg'
            "
            @error="
              $event.target.src = basedomainURL + '/Portals/Image/noimg.jpg'
            "
            preview
          />
        </template>
      </Column>
      <Column
        field="status"
        header="Trạng thái"
        headerStyle="text-align:center;max-width:150px;height:50px"
        bodyStyle="text-align:center;max-width:150px"
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
            :binary="true"
            v-model="data.data.status"
            @click="onCheckBox(data.data, true, true)"
          /> </template
      ></Column>

      <Column
        header="Chức năng"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:150px;height:50px"
        bodyStyle="text-align:center;max-width:150px"
      >
        <template #body="Tem">
          <div
            v-if="
           store.getters.user.is_admin ||   store.state.user.is_super == true ||
              store.state.user.user_id == Tem.data.created_by ||
              (store.state.user.role_id == 'admin' &&
                store.state.user.organization_id == Tem.data.organization_id)
            "
          >
            <Button
              @click="editTem(Tem.data)"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-pencil"
              v-tooltip.top="'Sửa'"
            ></Button>
            <Button
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-trash"
              @click="delTem(Tem.data)"
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
    :header="headerDialog"
    v-model:visible="displayBasic"
    :style="{ width: '40vw' }"
    :closable="true" :modal="true"
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12">
          <label class="col-3 text-left"
            >Mã hãng <span class="redsao">(*)</span></label
          >
          <InputText
            v-model="manufacturer.device_manufacturer_code"
            spellcheck="false"
            class="col-9 ip36 px-2"
            :class="{
              'p-invalid': v$.device_manufacturer_code.$invalid && submitted,
            }"
          />
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-3 text-left"></div>
          <small
            v-if="
              (v$.device_manufacturer_code.$invalid && submitted) ||
              v$.device_manufacturer_code.$pending.$response
            "
            class="col-9 p-error"
          >
            <span class="col-12 p-0">{{
              v$.device_manufacturer_code.required.$message
                .replace("Value", "Mã hãng sản xuất")
                .replace("is required", "không được để trống")
            }}</span>
          </small>
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-3 text-left"
            >Tên hãng <span class="redsao">(*)</span></label
          >
          <InputText
            v-model="manufacturer.device_manufacturer_name"
            spellcheck="false"
            class="col-9 ip36 px-2"
            :class="{
              'p-invalid': v$.device_manufacturer_name.$invalid && submitted,
            }"
          />
        </div>
        <div style="display: flex" class="field col-12 md:col-12">
          <div class="col-3 text-left"></div>
          <small
            v-if="
              (v$.device_manufacturer_name.$invalid && submitted) ||
              v$.device_manufacturer_name.$pending.$response
            "
            class="col-9 p-error"
          >
            <span class="col-12 p-0">{{
              v$.device_manufacturer_name.required.$message
                .replace("Value", "Tên hãng sản xuất")
                .replace("is required", "không được để trống")
            }}</span>
          </small>
        </div>

        <div class="field col-12 md:col-12 align-items-center flex">
          <div class="col-3 text-left">Địa chỉ</div>
          <Textarea
            class="col-9 p-2"
            v-model="manufacturer.head_office"
            :autoResize="true"
            rows="3"
            cols="30"
          />
        </div>
        <div style="display: flex" class="col-12 field md:col-12">
          <div class="col-9">
            <div class="field col-12 p-0">
              <label class="col-4 m-0 p-0 text-left align-items-center"
                >Số điện thoại</label
              >
              <InputNumber
                inputId="withoutgrouping"
                class="col-8 ip36 px-0"
                v-model="manufacturer.phone_number"
                mode="decimal"
                :useGrouping="false"
              />
            </div>
            <div class="col-12 field p-0 flex align-items-center">
              <label class="col-4 p-0 m-0 text-left">Quốc gia</label>
              <div class="col-8 p-0">
                <Dropdown
                  v-model="manufacturer.countryside_fake"
                  :options="countries"
                  optionLabel="name"
                  optionValue="name"
                  :filter="true"
                  :editable="true"
                  placeholder="Chọn hoặc nhập tên quốc gia"
                  :showClear="true"
                  class="col-12 ip36 px-2"
                >
                </Dropdown>

                <!-- <InputText
                  v-model="manufacturer.countryside_fake"
                  spellcheck="false"
                  class="col-12 ip36 px-2"
                /> -->
              </div>
            </div>
            <div class="field col-12 md:col-12 p-0">
              <label class="col-4 text-left p-0">STT</label>
              <InputNumber
                v-model="manufacturer.is_order"
                class="col-8 ip36 p-0"
              />
            </div>
            <div class="field col-12 md:col-12 p-0">
              <label
                style="vertical-align: text-bottom"
                class="col-4 text-left p-0"
                >Trạng thái
              </label>
              <InputSwitch v-model="manufacturer.status" />
            </div>
          </div>
          <div
            class="col-3 align-items-center justify-content-center text-center"
          >
            <div class="field">
              <label>Logo</label>
              <div class="inputanh relative">
                <img
                  @click="chonanh('AnhTem')"
                  id="logoTem"
                  
                  v-bind:src="
                    manufacturer.logo
                      ? basedomainURL + manufacturer.logo
                      : basedomainURL + '/Portals/Image/noimg.jpg'
                  "
                />
              
                <Button
                  @click="deleteemote_file()"
                  v-if="manufacturer.logo || files.length>0 "
                  style="
                    top: -10%;
                    right: -10%;
                    height: 20px !important;
                    width: 20px !important;
                  "
                  class="absolute p-0 m-0 p-button-rounded"
                  icon="pi pi-times"
                >
                </Button>
              </div>
              <input
                class="ipnone"
                id="AnhTem"
                type="file"
                accept=".png,.jpg,.gif,.jpeg"
                @change="handleFileUpload"
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
      class="p-button-outlined"
      />

      <Button
        label="Lưu"
        icon="pi pi-check"
        @click="saveData(!v$.$invalid)"
        autofocus
      />
    </template>
  </Dialog>
</template>
    
    <style scoped>
.inputanh {
  border: 1px solid #ccc;
  width: 8rem;
  height: 8rem;
  cursor: pointer;
  padding: 0.063rem;
  display: block;
  margin-left: auto;
  margin-right: auto;
}
.ipnone {
  display: none;
}
.inputanh img {
  object-fit: cover;
  width: 100%;
  height: 100%;
}
</style>
    