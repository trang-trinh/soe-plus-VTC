<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import moment from "moment";

import detailsDevice from "../../components/device/detailsDevice.vue";
import printCardVue from "./print/printCard.vue";
import { encr, checkURL } from "../../util/function.js";
//Khai báo
const router = inject("router");
const cryoptojs = inject("cryptojs");
//Khai báo
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const basedomainURL = baseURL;
const selectedCard = ref();
const checkDelList = ref(false);
const displayAssets = ref(false);
const isFirstCard = ref(false);
watch(selectedCard, () => {
  if (selectedCard.value.length > 0) {
    checkDelList.value = true;
  } else {
    checkDelList.value = false;
  }
});

const rules = {
  depreciation_month: {
    required,
    $errors: [
      {
        $property: "depreciation_month",
        $validator: "required",
        $message: "Tháng khấu hao không được để trống!",
      },
    ],
  },
  device_number: {
    required,
    $errors: [
      {
        $property: "device_number",
        $validator: "required",
        $message: "Số hiệu không được để trống!",
      },
    ],
  },
  barcode_id: {
    required,
    $errors: [
      {
        $property: "barcode_id",
        $validator: "required",
        $message: "Mã barcode không được để trống!",
      },
    ],
  },
  assets_condition: {
    required,
    $errors: [
      {
        $property: "assets_condition",
        $validator: "required",
        $message: "Kho thiết bị không được để trống!",
      },
    ],
  },
  warehouse_id: {
    required,
    $errors: [
      {
        $property: "warehouse_id",
        $validator: "required",
        $message: "Kho thiết bị không được để trống!",
      },
    ],
  },
  device_unit: {
    required,
    $errors: [
      {
        $property: "device_unit",
        $validator: "required",
        $message: "Đơn vị tính không được để trống!",
      },
    ],
  },
  price: {
    required,
    $errors: [
      {
        $property: "price",
        $validator: "required",
        $message: "Giá trị không được để trống!",
      },
    ],
  },
  device_name: {
    required,
    $errors: [
      {
        $property: "device_name",
        $validator: "required",
        $message: "Tên thiết bị không được để trống!",
      },
    ],
  },
  purchase_date: {
    required,
    $errors: [
      {
        $property: "purchase_date",
        $validator: "required",
        $message: "Ngày mua không được để trống!",
      },
    ],
  },
  corporation: {
    required,
    $errors: [
      {
        $property: "corporation",
        $validator: "required",
        $message: "Pháp nhân không được để trống!",
      },
    ],
  },
};
const rulesDM = {
  price: {
    required,
    $errors: [
      {
        $property: "price",
        $validator: "required",
        $message: "Giá trị không được để trống!",
      },
    ],
  },
};
const taskDateFilter = ref();

const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);
//Lọc theo ngày

const changeMonthPur = (event) => {
  filterSQL.value = [];
  options.value.start_date = new Date(event.month + "/1/" + event.year);
  if (event.month < 12)
    options.value.end_date = new Date(event.month + 1 + "/1/" + event.year);
  else options.value.end_date = new Date("1/1/" + (event.year + 1));
  let filterS = {
    filterconstraints: [
      { value: options.value.start_date, matchMode: "dateAfter" },
    ],
    filteroperator: "and",
    key: "purchase_date",
  };
  filterSQL.value.push(filterS);

  let filterS1 = {
    filterconstraints: [
      { value: options.value.end_date, matchMode: "dateBefore" },
    ],
    filteroperator: "and",
    key: "purchase_date",
  };
  filterSQL.value.push(filterS1);
};

const print = () => {
  var htmltable = "";
  htmltable = renderhtmlWord("formwordlist", htmltable);
   
  var printframe = window.frames["printframe"];
  printframe.document.write(htmltable);
  setTimeout(function () {
    printframe.print();
    printframe.document.close();
  }, 0);
};

function renderhtmlWord(id, htmltable) {
  htmltable = "";
  //Style
  htmltable += `<style>
    #formprint, #formword  {
      background: #fff !important;
    }
    #formprint *, #formword * {
      font-family: "Times New Roman", Times, serif !important;
      font-size: 13pt;
    }
    .title1,
    .title1 * {
      font-size: 17pt !important;
    }
    .title2,
    .title2 * {
      font-size: 16pt !important;
    }
    .title3,
    .title3 * {
      font-size: 15pt !important;
    }
    .boder tr th,
    .boder tr td {
      border: 1px solid #999999 !important;
      padding: 0.5rem;
    }
    table {
      min-width: 100% !important;
      page-break-inside: auto !important;
      border-collapse: collapse !important;
      table-layout: fixed !important;
    }
    thead {
      display: table-header-group !important;
    }
    tbody {
      display: table-header-group !important;
    }
    tr {
      -webkit-column-break-inside: avoid !important;
      page-break-inside: avoid !important;
    }
    tfoot {
      display: table-footer-group !important;
    }
    
    .text-center {
      text-align: center !important;
    }
    .text-left {
      text-align: left !important;
    }
    .text-right {
      text-align: right !important;
    }
    .html p{
      margin: 0 !important;
      padding: 0 !important;
    }
  </style>`;
  var html = document.getElementById(id);
  if (html) {
    htmltable += html.innerHTML;
  }
  return htmltable;
}
const changeYearPur = (event) => {
  filterSQL.value = [];
  options.value.start_date = new Date("1/1/" + event.year);

  options.value.end_date = new Date("1/1/" + (event.year + 1));
  let filterS = {
    filterconstraints: [
      { value: options.value.start_date, matchMode: "dateAfter" },
    ],
    filteroperator: "and",
    key: "purchase_date",
  };
  filterSQL.value.push(filterS);

  let filterS1 = {
    filterconstraints: [
      { value: options.value.end_date, matchMode: "dateBefore" },
    ],
    filteroperator: "and",
    key: "purchase_date",
  };
  filterSQL.value.push(filterS1);
};
const todayClick = () => {
  taskDateFilter.value = [];
  taskDateFilter.value.push(new Date());
};
const delDayClick = () => {
  taskDateFilter.value = null;
  options.value.start_date = null;
  options.value.end_date = null;
  filterSQL.value = [];
  isDynamicSQL.value = true;
  loadData(true);
};
const onDayClick = () => {
  if (taskDateFilter.value != null) {
    options.value.start_date = taskDateFilter.value[0];
    options.value.end_date = taskDateFilter.value[1];
    if (!options.value.end_date)
      options.value.end_date = options.value.start_date;
    filterSQL.value = [];
    if (
      options.value.start_date &&
      options.value.start_date != options.value.end_date
    ) {
      let sDate = new Date(options.value.start_date);
      sDate.setDate(sDate.getDate() - 1);
      options.value.start_date = sDate;
      let filterS = {
        filterconstraints: [
          { value: options.value.start_date, matchMode: "dateAfter" },
        ],
        filteroperator: "and",
        key: "purchase_date",
      };
      filterSQL.value.push(filterS);
    }
    if (
      options.value.end_date &&
      options.value.start_date != options.value.end_date
    ) {
      let eDate = new Date(options.value.end_date);
      eDate.setDate(eDate.getDate() + 1);
      options.value.end_date = eDate;
      let filterS = {
        filterconstraints: [
          { value: options.value.end_date, matchMode: "dateBefore" },
        ],
        filteroperator: "and",
        key: "purchase_date",
      };
      filterSQL.value.push(filterS);
    }
    if (
      options.value.start_date &&
      options.value.start_date == options.value.end_date
    ) {
      let filterS1 = {
        filterconstraints: [
          { value: options.value.start_date, matchMode: "dateIs" },
        ],
        filteroperator: "and",
        key: "purchase_date",
      };
      filterSQL.value.push(filterS1);
    }
  }
  isDynamicSQL.value = true;
  loadData(true);
};
const filtersDM = ref({
  device_name: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
});

//Xóa tin tức

const delCard = (Card) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá thẻ thiết bị này không!",
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
          .delete(baseURL + "/api/device_card/delete_device_card", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: Card != null ? [Card.card_id] : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá thẻ thiết bị thành công!");
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
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
};
const filters = ref({
  global: { value: null, matchMode: FilterMatchMode.CONTAINS },
  device_name: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
  purchase_date: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.DATE_IS }],
  },
  device_user_id: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.EQUALS }],
  },
  device_number: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
  barcode_id: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
  status: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.EQUALS }],
  },
  purchase_date: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.DATE_BEFORE }],
  },
  purchase_date: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.DATE_AFTER }],
  },
});
//Phân trang dữ liệu
const onPage = (event) => {
  options.value.pagesize = event.rows;
  options.value.pageno = event.page;
  loadDataSQL();
};
const filterSQL = ref([]);
const isDynamicSQL = ref(false);
const loadDataSQL = () => {
  let data = {
    id: "card_id",
    next: options.value.IsNext,
    sqlO: options.value.sort,
    Search: options.value.search,
    PageNo: options.value.pageno,
    PageSize: options.value.pagesize,
    fieldSQLS: filterSQL.value,
  };

  options.value.loading = true;
  axios
    .post(baseURL + "/api/SQL/Filter_device_card", data, config)
    .then((response) => {
      let dt = JSON.parse(response.data.data);

      let data = dt[0];

      if (data.length > 0) {
        data.forEach((element, i) => {
          element.is_order =
            options.value.pageno * options.value.pagesize + i + 1;
          if (!element.created_date_show) element.created_date_show = null;
          element.created_date_show = moment(
            new Date(element.created_date)
          ).format("DD/MM/YYYY");
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

      addLog({
        title: "Lỗi Console loadData",
        controller: "Card.vue",
        logcontent: error.message,
        loai: 2,
      });
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const selectedDeviceMain = ref({
  is_order: 1,
  proposal_code: "",
  device_id: null,
  device_name: "",
  image: "",
  device_number: false,
  barcode_id: "",
  barcode_type: 0,
  barcode_image: "",
  status: 0,
  not_used: false,
  used: true,
});
const onChangeUser = (value) => {
  selectedDeviceMain.value.device_department_name =
    listDropdownUser.value.filter((x) => x.code == value)[0].department_name;
};

const onChangeCountry = () => {
  devicecard.value.manufacture_country = listPCard.value.filter(
    (x) => x.code == devicecard.value.producer
  )[0]
    ? listPCard.value.filter((x) => x.code == devicecard.value.producer)[0]
        .countryside
    : "";
};
const onSortDM = (event) => {
  options.value.sortDM =
    event.sortField + (event.sortOrder == 1 ? " ASC" : " DESC");
  if (event.sortField == "STT") {
    options.value.sortDM =
      "is_order" + (event.sortOrder == 1 ? " ASC" : " DESC");
  }

  loadDataSQLDM();
};
const onSelectedDM = (event) => {
  document.getElementById("scrollDM").scrollIntoView();
};
const onFilterDM = (event) => {
  filterSQLDM.value = [];
  for (const [key, value] of Object.entries(event.filters)) {
    if (key != "global") {
      let obj = {
        key: key == "device_name" ? "device_name" : key,
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
        filterSQLDM.value.push(obj);
    }
  }
  options.value.pagenoDM = 0;
  firstDM.value = 0;
  options.value.id = null;
  loadDataSQLDM();
};
const firstDM = ref(0);
const filterSQLDM = ref([]);
const loadDataSQLDM = () => {
  datalistsDM.value = [];
  let data = {
    sqlS: true,
    sqlO: options.value.sortDM,
    Search: options.value.SearchTextDM,
    PageNo: options.value.pagenoDM,
    PageSize: options.value.pagesizeDM,
    sqlF: null,
    next: true,
    fieldSQLS: filterSQLDM.value,
  };
  options.value.loading = true;
  axios
    .post(baseURL + "/api/SQL/Filter_device_main", data, config)
    .then((response) => {
      let dt = JSON.parse(response.data.data);
      let data = dt[0];
      if (data.length > 0) {
     
        data.forEach((element, i) => {
          element.STT =
            options.value.pagenoDM * options.value.pagesizeDM + i + 1;
        });

        datalistsDM.value = data;
      } else {
        datalistsDM.value = [];
      }
      if (isFirst.value) isFirst.value = false;
      options.value.loading = false;

      //Show Count nếu có
      if (dt.length == 2) {
        options.value.totalRecordsDM = dt[1][0].totalRecords;
      }
    })
    .catch((error) => {
      options.value.loading = false;

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
const onPageDM = (event) => {
  if (event.rows != options.value.pagesizeDM) {
    options.value.pagesizeDM = event.rows;
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

    options.value.id = datalistsDM.value[datalistsDM.value.length - 1].place_id;
    options.value.IsNext = true;
  } else if (event.page < options.value.PageNo) {
    //Trang trước
    options.value.id = datalistsDM.value[0].device_id;
    options.value.IsNext = false;
  }
  options.value.pagenoDM = event.page;
  loadDataSQLDM();
};
//Sort
const onSort = (event) => {
  if (event.sortField == null) {
    isDynamicSQL.value = false;
    loadData();
  } else {
    options.value.sort =
      event.sortField + (event.sortOrder == 1 ? " ASC" : " DESC");
    if (event.sortField != "card_id") {
      options.value.sort +=
        ",card_id " + (event.sortOrder == 1 ? " ASC" : " DESC");
    }

    isDynamicSQL.value = true;
    loadData();
  }
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

  options.value.pageno = 0;
  options.value.id = null;
  isDynamicSQL.value = true;
  loadData(true);
};
//DropDown

const onDropDown = (value) => {
  let data = {
    IntID: value.card_id,
    TextID: value.card_id + "",
    IntTrangthai: value.status,
    BitTrangthai: false,
  };
  axios
    .put(baseURL + "/api/device_card/update_status", data, config)
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Sửa trạng thái thành công!");
        loadData(false);
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
};
//Xóa nhiều
const deleteList = () => {
  let listId = new Array(selectedCard.value.length);
  let check = false;
  selectedCard.value.forEach((element) => {
    if (element.status != "TPBG") {
      swal.fire({
        title: "Error!",
        text:
          "Không được xóa thẻ thiết bị có trạng thái: " +
          element.device_status_name,
        icon: "error",
        confirmButtonText: "OK",
      });
      check = true;
    }
  });
  if (check) {
    return;
  } else {
    swal
      .fire({
        title: "Thông báo",
        text: "Bạn có muốn xóa danh sách thẻ thiết bị này không!",
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
          selectedCard.value.forEach((item) => {
            listId.push(item.card_id);
          });
          axios
            .delete(baseURL + "/api/device_card/delete_device_card", {
              headers: { Authorization: `Bearer ${store.getters.token}` },
              data: listId != null ? listId : 1,
            })
            .then((response) => {
              swal.close();
              if (response.data.err != "1") {
                swal.close();
                toast.success("Xoá thẻ thiết bị thành công!");
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
                  text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                  icon: "error",
                  confirmButtonText: "OK",
                });
              }
            });
        }
      });
  }
};
const delAvatar = () => {
  devicecard.value.image = null;
  files.value = [];
  document.getElementById("logoLang").src =
    baseURL + "/Portals/Image/noimg.jpg";
};
//Lấy file ảnh
const chonanh = (id) => {
  document.getElementById(id).click();
};
const files = ref([]);
const handleFileUpload = (event) => {
  files.value = event.target.files;
  var output = document.getElementById("logoLang");
  output.src = URL.createObjectURL(event.target.files[0]);
  output.onload = function () {
    URL.revokeObjectURL(output.src); // free memory
  };
};
const toast = useToast();
const isFirst = ref(true);
const datalists = ref();
const isSaveCard = ref(false);
const sttCard = ref(1);
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
//ADD log
const addLog = (log) => {
  axios.post(baseURL + "/api/Proc/AddLog", log, config);
};
const submitted = ref(false);
const options = ref({
  IsNext: true,
  sort: "card_id DESC",
  sortDM: "device_id DESC",
  search: "",
  pageno: 0,
  pagesize:2 ,
  pagenoDM: 0,
  pagesizeDM: 20,
  loading: true,
  totalRecords: null,
  start_date: null,
  end_date: null,
});
const devicecard = ref({
  is_order: 1,
  proposal_code: "",
  device_id: null,
  device_name: "",
  image: "",
  device_number: false,
  barcode_id: "",
  barcode_type: 0,
  barcode_image: "",
  status: 0,
});
const v$ = useVuelidate(rules, devicecard);
const vDM$ = useVuelidate(rulesDM, selectedDeviceMain);
const danhMuc = ref();
const treedonvis = ref();
const submittedDM = ref(false);
//METHOD

const hideSelectDevice = () => {
  selectedDeviceMain.value = {
    is_order: 1,
    proposal_code: "",
    device_id: null,
    device_name: "",
    image: "",
    device_number: false,
    barcode_id: "",
    barcode_type: 0,
    barcode_image: "",
    status: 0,
    not_used: false,
    used: true,
  };
  options.value.device_card_type = null;
  options.value.SearchTextDM = null;
  displayAssets.value = false;
};

const checkNumber = (valie, prop) => {
  let regex = /[0-9]/;
  let checkKey = regex.test(valie.key);
  if (!checkKey) {
    devicecard.value[prop] = devicecard.value[prop].replaceAll(valie.key, "");
  }
};
const onSelectDevice = (isFormValid) => {
  device_handover.value = null;
  submittedDM.value = true;
  if (selectedDeviceMain.value.device_name == "") {
    swal.fire({
      title: "Thông báo!",
      text: "Vui lòng chọn thiết bị!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }

  if (!isFormValid) {
    return;
  }
  if (selectedDeviceMain.value.device_department_id)
    Object.keys(selectedDeviceMain.value.device_department_id).forEach(
      (key) => {
        devicecard.value.manage_department_id = Number(key);
        return;
      }
    );
  if (
    !selectedDeviceMain.value.warehouse_id ||
    (selectedUser.value.used &&
      (!selectedDeviceMain.value.device_user_id ||
        (!selectedDeviceMain.value.device_department_id &&
          selectedUser.value.used)))
  ) {
    return;
  }
  if (selectedUser.value.used == false) {
    selectedDeviceMain.value.device_user_id = null;
    devicecard.value.manage_department_id = null;
  }
  devicecard.value.device_id = selectedDeviceMain.value.device_id;
  devicecard.value.device_name = selectedDeviceMain.value.device_name;
  devicecard.value.device_unit = Number(
    selectedDeviceMain.value.device_unit_id
  );
  devicecard.value.price = selectedDeviceMain.value.price;
  devicecard.value.insurance_cycle = selectedDeviceMain.value.insurance_cycle;
  if (selectedDeviceMain.value.device_user_id)
    devicecard.value.device_user_id = selectedDeviceMain.value.device_user_id;

  devicecard.value.insurance_month = selectedDeviceMain.value.insurance_month;
  devicecard.value.device_type_id = selectedDeviceMain.value.device_type_id;

  devicecard.value.depreciation_month =
    selectedDeviceMain.value.depreciation_month;
  devicecard.value.device_note = selectedDeviceMain.value.device_note;
  devicecard.value.warehouse_id = selectedDeviceMain.value.warehouse_id;
  devicecard.value.device_des = selectedDeviceMain.value.device_des;
  devicecard.value.user_department_name =
    selectedDeviceMain.value.device_department_name;
  displayAssets.value = false;
  addDeviceHanover();
  selectedDeviceMain.value = null;
};
//Tìm kiếm
const searchDeviceMain = () => {
  loadDataSQLDM();
};
const filterDeviceMain = () => {
  filterSQLDM.value = [];
  if (options.value.device_card_type != null) {
    let filterS = {
      filterconstraints: [
        { value: options.value.device_card_type, matchMode: "equals" },
      ],
      filteroperator: "and",
      key: "device_type_id",
    };
    filterSQLDM.value.push(filterS);
  }
  loadDataSQLDM();
};
const initTudien = () => {
  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_device_department",
            par: [{ par: "user_id", va: store.getters.user.user_id }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data[1].length > 0) {
        let obj = renderTreeDV(
          data[1],
          "organization_id",
          "organization_name",
          "đơn vị"
        );

        treedonvis.value = obj.arrtreeChils;
      }  
     
    })
    .catch((error) => {});
};
const displayDetails = ref(false);
const openDetails = (data) => {
  if (data.corporation_name)
    data.corporation_name = treedonvis.value.filter(
      (x) => x.data == data.corporation
    )[0].label;
  if (data.created_name)
    data.created_name = listUsers.value.filter(
      (x) => x.user_id == data.created_by
    )[0].full_name;

  if (data.device_unit)
    data.device_unit_name = listUnit.value.filter(
      (x) => x.code == Number(data.device_unit)
    )[0].name;
  if (data.device_type_id)
    data.device_type_name = listType.value.filter(
      (x) => x.code == data.device_type_id
    )[0].name;
  if (data.warehouse_id)
    data.warehouse_name = listWarehouse.value.filter(
      (x) => x.code == data.warehouse_id
    )[0].name;
  devicecard.value = data;
  displayDetails.value = true;
};
const closeDetails = () => {
  displayDetails.value = false;
  devicecard.value = {};
};
const loadCount = () => {
  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "device_card_count",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "device_id", va: options.value.device_id },
              { par: "proposal_code", va: options.value.proposal_code },
              { par: "search", va: options.value.search },
              { par: "status", va: options.value.status },
              { par: "manufacture_year", va: options.value.manufacture_year },
              { par: "device_number", va: options.value.device_number },
              { par: "barcode_type", va: options.value.barcode_type },
              { par: "device_unit", va: options.value.device_unit },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        options.value.totalRecords = data[0].totalRecords;
        sttCard.value = data[0].totalRecords + 1;
      } else options.value.totalRecords = 0;
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
const saveCard = (isFormValid) => {
  submitted.value = true;
  if (!isFormValid) {
    return;
  }
  if (devicecard.value.device_number.length > 50) {
    swal.fire({
      title: "Thông báo!",
      text: "Số hiệu không được dài quá 50 kí tự!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  Object.keys(devicecard.value.corporation).forEach((key) => {
    devicecard.value.corporation = Number(key);
    return;
  });

  if (devicecard.value.manufacture_year_fake)
    devicecard.value.manufacture_year =
      devicecard.value.manufacture_year_fake.getFullYear();
  if (devicecard.value.import_year_fake)
    devicecard.value.import_year =
      devicecard.value.import_year_fake.getFullYear();

  if (typeof devicecard.value.purchase_date == "string") {
    var startDay = devicecard.value.purchase_date.split("/");
    devicecard.value.purchase_date = new Date(
      startDay[2] + "/" + startDay[1] + "/" + startDay[0]
    );
  }

  let formData = new FormData();
  for (var i = 0; i < files.value.length; i++) {
    let file = files.value[i];
    formData.append("image", file);
  }
  formData.append("card", JSON.stringify(devicecard.value));
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });

  if (!isSaveCard.value) {
    if (devicecard.value.status == "CXN") {
       device_handover.value.device_department_id=devicecard.value.manage_department_id
      formData.append("handover", JSON.stringify(device_handover.value));
    }
    axios
      .post(baseURL + "/api/device_card/add_device_card", formData, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Thêm thẻ thiết bị thành công!");
          checkCV.value = true;
          displayBasic.value = false;
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
      .put(baseURL + "/api/device_card/update_device_card", formData, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa thẻ thiết bị thành công!");
          checkCV.value = true;
          displayBasic.value = false;
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
const addDeviceHanover = () => {
  if (devicecard.value.device_user_id) {
    device_handover.value = {
      status: 1,
      is_order: 1,
      receipt_type: 0,
      handover_created_date: new Date(),
      handover_type: 0,
      user_deliver_id: store.getters.user.user_id,
      user_deliver_name: store.getters.user.full_name,
      user_deliver_department_id: store.getters.user.organization_id,
      user_deliver_position: store.getters.user.role_name,
      print_note: null,
      is_receiver_accept: false,
      is_individual: true,
      organization_id: store.getters.user.organization_id,
     
    };
    axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
          proc: "device_config_number_get",
          par: [
            { par: "user_id", va: store.getters.user.user_id },
            { par: "current_number", va: null },
            { par: "year", va: null },
            { par: "text_symbols", va: null },
            { par: "agency_issued", va: null },
            { par: "code_number", va: "TS_PhieuBanGiao" },
            { par: "status", va: true },
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
          loadDeviceNumberHandover(data[0]);
        }
      })
      .catch((error) => {});
  }
};
//Sửa bản ghi
const selectCapcha = ref();
const editCard = (data) => {
  submitted.value = false;
  files.value = [];
  if (data.manufacture_year)
    data.manufacture_year_fake = new Date("1/1/" + data.manufacture_year);
  if (data.import_year)
    data.import_year_fake = new Date("1/1/" + data.import_year);

  data.purchase_date = new Date(data.purchase_date);
  if (data.use_date != null) data.use_date = new Date(data.use_date);
  selectCapcha.value = {};
  selectCapcha.value[data.corporation] = true;
  data.device_unit = Number(data.device_unit);
  devicecard.value = data;
  devicecard.value.corporation = selectCapcha.value;
  if (devicecard.value.barcode_type == 1) {
    devicecard.value.qrcode = true;
    devicecard.value.barcode = false;
  } else {
    devicecard.value.qrcode = false;
    devicecard.value.barcode = true;
  }
  headerDialog.value = "Sửa thẻ thiết bị";
  isSaveCard.value = true;
  barcode_img_old.value = devicecard.value.barcode_image;
  displayBasic.value = true;
};
//Hiển thị dialog
const headerDialog = ref();
const displayBasic = ref(false);
const danhMucAdd = ref();
const selectedUser = ref({
  used: false,
  not_used: true,
});
const onChangeUsed = () => {
  if (selectedUser.value.used == false) {
    devicecard.value.status = "TK";
    selectedUser.value.not_used = true;
    return;
  }
  if (selectedUser.value.used == true) {
    devicecard.value.status = "CXN";
    selectedUser.value.not_used = false;
    return;
  }
  if (selectedUser.value.not_used == false) {
    devicecard.value.status = "CXN";
    selectedUser.value.used = true;
    return;
  }
  if (selectedUser.value.not_used == true) {
    devicecard.value.status = "TK";
    selectedUser.value.used = false;
    return;
  }
};
const onChangeUsed1 = () => {
  if (selectedUser.value.not_used == false) {
    devicecard.value.status = "CXN";
    selectedUser.value.used = true;
    return;
  }
  if (selectedUser.value.not_used == true) {
    devicecard.value.status = "TK";
    selectedUser.value.used = false;
    devicecard.value.device_user_id = null;
    return;
  }
};
const onChangeBarcode = (value) => {
  if (value == 1) {
    devicecard.value.barcode = devicecard.value.qrcode;
    devicecard.value.qrcode = !devicecard.value.barcode;
    devicecard.value.barcode_type = 0;
    if (
      devicecard.value.barcode_id != null &&
      devicecard.value.barcode_id != ""
    )
      onConvertBarCode();
  } else {
    devicecard.value.qrcode = devicecard.value.barcode;
    devicecard.value.barcode = !devicecard.value.qrcode;
    devicecard.value.barcode_type = 1;
    if (
      devicecard.value.barcode_id != null &&
      devicecard.value.barcode_id != ""
    )
      onConvertBarCode();
  }
};
const onConvertBarCode = () => {
  if (devicecard.value.barcode_id && devicecard.value.barcode_id != "") {
    let formData = new FormData();
    formData.append(
      "barcode",
      JSON.stringify({
        barcodestr: devicecard.value.barcode_id,
        barcode_old: devicecard.value.barcode_image,
        barcode_type: devicecard.value.barcode ? 0 : 1,
      })
    );

    axios
      .post(baseURL + "/api/device_card/GenBarcode", formData, config)
      .then((response) => {
        if (response.data.data)
          if (response.data.err != "1") {
            swal.close();
            let data = JSON.parse(response.data.data);

            devicecard.value.barcode_image = data;
            return data;
          } else {
            swal.fire({
              title: "Error!",
              text: response.data.data,
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
const listSCard = ref([]);
const listPCard = ref([]);
const openBasic = (str) => {
  // loadRelate();
  devicecard.value = {
    barcode: true,
    qrcode: false,
    purchase_date: new Date(),
    barcode_type: 0,
    status: "TK",
    is_order: sttCard.value,
    corporation: {},
    import_year_fake: new Date(),
    manufacture_year_fake: new Date(),
  };
  selectedUser.value = {
    used: false,
    not_used: true,
  };
  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "device_config_number_get",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "current_number", va: null },
              { par: "year", va: null },
              { par: "text_symbols", va: null },
              { par: "agency_issued", va: null },
              { par: "code_number", va: "TS_PhieuTaiSan" },
              { par: "status", va: true },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];

      if (data.length > 0) {
        loadDeviceNumber(data[0]);
        devicecard.value.corporation[store.getters.user.organization_id] = true;
        checkCV.value = false;
        files.value = [];
        submitted.value = false;
        headerDialog.value = str;
        isSaveCard.value = false;
        displayBasic.value = true;
      }
      else{
        
        swal.fire({
          title: "Error!",
          text: "Vui lòng cấu hình số hiệu cho thẻ thiết bị!",
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    })
    .catch((error) => {});
};
const device_handover = ref({
  is_order: 1,
  proposal_code: "",
  device_id: null,
  handover_number: "",
  image: "",
  barcode_id: "",
  barcode_type: 0,
  barcode_image: "",
  status: 0,
  receipt_type: 1,
});
const loadDeviceNumberHandover = (dataVL) => {
  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "device_number_cf_nb",
            par: [
              { par: "type", va: 2 },
              { par: "user_id", va: store.getters.user.user_id },
              { par: "config_number_id", va: dataVL.config_number_id },
              { par: "current_number", va: dataVL.current_number },
              { par: "year", va: dataVL.year },
              { par: "text_symbols", va: dataVL.text_symbols },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];

      if (data.length > 0) {
        device_handover.value.handover_number =
          data[0].current_number +
          "/" +
          data[0].year +
          "/" +
          data[0].text_symbols;
      }
    })
    .catch((error) => {});
};
const checkCV = ref(false);
const closeDialogDC = () => {
  displayBasic.value = false;
};
const barcode_img_old = ref();
const closeDialog = () => {
  if (!checkCV.value) {
    devicecard.value.barcode_id = null;
    if (
      isSaveCard.value &&
      devicecard.value.barcode_image == barcode_img_old.value
    )
      devicecard.value.barcode_image = null;
    onConvertBarCode();
  }
  isFirstCard.value = false;
  loadData(true);
  displayBasic.value = false;
};
const loadCountDeviceMain = () => {
  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "device_main_count",
            par: [
              { par: "user_id", va: store.state.user.user_id },
              { par: "status", va: true },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];

      if (data.length > 0) {
        options.value.totalRecordsDM = data[0].totalRecords;
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
const loadDataDeviceMain = (rf) => {
  if (rf) {
    loadCountDeviceMain();
  }
  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "device_main_list",
            par: [
              { par: "pageno", va: options.value.pagenoDM },
              { par: "pagesize", va: options.value.pagesizeDM },
              { par: "user_id", va: store.state.user.user_id },
              { par: "status", va: true },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      data.forEach((element, i) => {
        element.STT = options.value.pagenoDM * options.value.pagesizeDM + i + 1;
      });
      if (isFirst.value) isFirst.value = false;
      datalistsDM.value = data;
      options.value.loading = false;

      displayAssets.value = true;

      setTimeout(() => {
        document.getElementById("idScroll-2022").scrollIntoView();
      }, 200);
    })
    .catch((error) => {
      options.value.loading = false;

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
const loadData = (rf) => {
  if (isDynamicSQL.value) {
    loadDataSQL();
    return false;
  }

  if (rf) {
    options.value.loading = true;
    loadCount();
  }
  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "device_card_list",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "device_id", va: options.value.device_id },
              { par: "proposal_code", va: options.value.proposal_code },
              { par: "search", va: options.value.search },
              { par: "status", va: options.value.status },
              { par: "manufacture_year", va: options.value.manufacture_year },
              { par: "device_number", va: options.value.device_number },
              { par: "barcode_type", va: options.value.barcode_type },
              { par: "device_unit", va: options.value.device_unit },
              { par: "pageno", va: options.value.pageno },
              { par: "pagesize", va: options.value.pagesize },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        data.forEach((element, i) => {
          element.is_order =
            options.value.pageno * options.value.pagesize + i + 1;
        });

        datalists.value = data;
      } else {
        datalists.value = [];
      }
      if (isFirst.value) isFirst.value = false;
      options.value.loading = false;
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;
    });
};

const filterButs = ref();
const checkFilter = ref(false);
//Khai báo function
const toggleFilter = (event) => {
  filterButs.value.toggle(event);
};
const hideFilter = () => {
  if (
    !(
      filterTrangthai.value != null ||
      filterCardUser.value != null ||
      filterCardType.value != null ||
      filterCardWarehouse.value != null
    )
  )
    checkFilter.value = false;
};
const filterIsHot = ref();
const filterTrangthai = ref();
const filterCardUser = ref();
const filterCardType = ref();
const filterCardWarehouse = ref();

const reFilterCard = () => {
  checkFilter.value = false;
  filterIsHot.value = null;
  filterCardUser.value = null;
  filterCardType.value = null;
  filterCardWarehouse.value = null;
  filterTrangthai.value = null;
  taskDateFilter.value = null;
  options.value.is_hot = null;
  options.value.news_type = null;
  options.value.status = null;
  filterCard(false);
};
const filterCard = (check) => {
  if (check) checkFilter.value = true;

  filterSQL.value = [];
  if (filterCardUser.value != null) {
    let filterS = {
      filterconstraints: [{ value: filterCardUser.value, matchMode: "equals" }],
      filteroperator: "and",
      key: "device_user_id",
    };
    filterSQL.value.push(filterS);
  }
  if (filterCardType.value != null) {
    let filterS = {
      filterconstraints: [{ value: filterCardType.value, matchMode: "equals" }],
      filteroperator: "and",
      key: "device_type_id",
    };
    filterSQL.value.push(filterS);
  }
  if (filterCardWarehouse.value != null) {
    let filterS = {
      filterconstraints: [
        { value: filterCardWarehouse.value, matchMode: "equals" },
      ],
      filteroperator: "and",
      key: "warehouse_id",
    };
    filterSQL.value.push(filterS);
  }
  if (filterTrangthai.value != null) {
    let filterS = {
      filterconstraints: [
        { value: filterTrangthai.value, matchMode: "equals" },
      ],
      filteroperator: "and",
      key: "status",
    };
    filterSQL.value.push(filterS);
  }
  isDynamicSQL.value = true;
  loadData(true);
};
//Tìm kiếm
const searchCard = () => {
  loadDataSQL();
};
const first = ref(0);
const refreshData = () => {
  options.value.search = "";
  options.value.status = null;
  filterCardType.value = null;
  filterCardUser.value = null;
  filterTrangthai.value = null;
  filterCardWarehouse.value = null;
  options.value.start_date = null;
  options.value.end_date = null;
  taskDateFilter.value = null;
  checkFilter.value = false;
  filterSQL.value = [];
  first.value = 0;
  options.value.pageno = 0;
  filterSQL.value = [];
  selectedCard.value = [];

  filters.value = {
    global: { value: null, matchMode: FilterMatchMode.CONTAINS },
    device_name: {
      operator: FilterOperator.AND,
      constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
    },
    purchase_date: {
      operator: FilterOperator.AND,
      constraints: [{ value: null, matchMode: FilterMatchMode.DATE_IS }],
    },
    device_user_id: {
      operator: FilterOperator.AND,
      constraints: [{ value: null, matchMode: FilterMatchMode.EQUALS }],
    },
    device_number: {
      operator: FilterOperator.AND,
      constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
    },
    barcode_id: {
      operator: FilterOperator.AND,
      constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
    },
    status: {
      operator: FilterOperator.AND,
      constraints: [{ value: null, matchMode: FilterMatchMode.EQUALS }],
    },
    purchase_date: {
      operator: FilterOperator.AND,
      constraints: [{ value: null, matchMode: FilterMatchMode.DATE_BEFORE }],
    },
    purchase_date: {
      operator: FilterOperator.AND,
      constraints: [{ value: null, matchMode: FilterMatchMode.DATE_AFTER }],
    },
  };
  loadData(true);
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
        excelname: "SỔ CÔNG VĂN",
        proc: "ca_dispatch_book_listexport",
        par: [
          { par: "search", va: options.value.SearchText },
          { par: "status", va: filterTrangthai.value },
          { par: "user_id", va: store.state.user.user_id },
        ],
      },
      config
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
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};

const renderTreeDV = (data, id, name, title) => {
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
const renderTreeDV1 = (data, id, name, title, org_id) => {
  let arrtreeChils = [];
  if (org_id == "" || org_id == null) {
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
  return { arrtreeChils: arrtreeChils };
};
const listType = ref();
const loadDeviceType = () => {
  listType.value = [];
  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "device_type_list",
            par: [
              { par: "pageno", va: 0 },
              { par: "pagesize", va: 1000000 },
              { par: "user_id", va: store.state.user.user_id },
              { par: "status", va: true },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      data.forEach((element, i) => {
        listType.value.push({
          name: element.device_type_name,
          code: element.device_type_id,
        });
      });
    })
    .catch((error) => {
      options.value.loading = false;

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
const listUnit = ref();
const loadDeviceUnit = () => {
  listUnit.value = [];
  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "device_unit_list",
            par: [
              { par: "pageno", va: 0 },
              { par: "pagesize", va: 1000000 },
              { par: "user_id", va: store.state.user.user_id },

              { par: "status", va: true },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      data.forEach((element, i) => {
        listUnit.value.push({
          name: element.device_unit_name,
          code: element.device_unit_id,
        });
      });
    })
    .catch((error) => {
      options.value.loading = false;

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
const listWarehouse = ref();
const loadWareHouse = () => {
  listWarehouse.value = [];
  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "device_warehouse_list",
            par: [
              { par: "pageno", va: 0 },
              { par: "pagesize", va: 10000000 },
              { par: "user_id", va: store.state.user.user_id },
              { par: "status", va: true },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      data.forEach((element, i) => {
        listWarehouse.value.push({
          name: element.warehouse_name,
          code: element.warehouse_id,
        });
      });
    })
    .catch((error) => {
      options.value.loading = false;
    });
};

const listDropdownUserCheck = ref();
const listDropdownUser = ref();
const listUsers = ref([]);
const loadUser = () => {
  listUsers.value = [];
  listDropdownUser.value = [];
  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_users_list_dd",
            par: [
              { par: "search", va: null },
              { par: "user_id", va: store.getters.user.user_id },
              { par: "role_id", va: null },
              {
                par: "organization_id",
                va: store.getters.user.organization_id,
              },
              { par: "department_id", va: null },
              { par: "position_id", va: null },
              { par: "pageno", va: 1 },
              { par: "pagesize", va: 10000 },
              { par: "isadmin", va: null },
              { par: "status", va: null },
              { par: "start_date", va: null },
              { par: "end_date", va: null },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      data.forEach((element, i) => {
        listDropdownUser.value.push({
          name: element.full_name,
          code: element.user_id,
          avatar: element.avatar,
          department_name: element.department_name,
          role_name: element.role_name,
          position_name: element.position_name,
        });
      });
      listUsers.value = data;

      listDropdownUserCheck.value = listDropdownUser.value;
    })
    .catch((error) => {
      options.value.loading = false;

      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};

const datalistsDM = ref();
const showListAssets = () => {
  let arw = "";
  Object.keys(devicecard.value.corporation).forEach((key) => {
    arw = key;
    return;
  });
  loadOrganization(arw);

  selectedDeviceMain.value = {
    is_order: 1,
    proposal_code: "",
    device_id: null,
    device_name: "",
    image: "",
    device_number: false,
    barcode_id: "",
    barcode_type: 0,
    barcode_image: "",
    status: false,
    used: true,
  };
  selectedUser.value = {
    not_used: devicecard.value.status == "TK" ? true : false,
    used: devicecard.value.status == "TK" ? false : true,
  };
  submittedDM.value = false;
  loadDataDeviceMain(true);
};
const listDepartment = ref([]);
const loadOrganization = (value) => {
  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_organization_list_d",
            par: [{ par: "user_id", va: store.getters.user.user_id }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        let obj = renderTreeDV1(
          data,
          "organization_id",
          "organization_name",
          "đơn vị",
          value
        );

        listDepartment.value = obj.arrtreeChils;
      } else listDepartment.value = [];
    })
    .catch((error) => {
      options.value.loading = false;

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
const loadStatusDevice = () => {
  listSCard.value = [];
  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "device_status_list",
            par: [
              { par: "pageno", va: 0 },
              { par: "pagesize", va: 1000000 },
              { par: "user_id", va: store.state.user.user_id },
              { par: "status", va: true },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      data.forEach((element, i) => {
        listSCard.value.push({
          name: element.device_status_name,
          code: element.device_status_code,
        });
      });
    })
    .catch((error) => {
      options.value.loading = false;

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
const loadProducerDevice = () => {
  listPCard.value = [];

  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "device_manufacturer_list",
            par: [
              { par: "pageno", va: 0 },
              { par: "pagesize", va: 1000000 },
              { par: "user_id", va: store.state.user.user_id },
              { par: "status", va: true },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];

      data.forEach((element, i) => {
        listPCard.value.push({
          name: element.device_manufacturer_name,
          code: element.device_manufacturer_code,
          countryside: element.countryside,
          logo: element.logo,
        });
      });
    })
    .catch((error) => {
      options.value.loading = false;

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

const loadDeviceNumber = (dataVL) => {
  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "device_number_cf_nb",
            par: [
              { par: "type", va: 1 },
              { par: "user_id", va: store.getters.user.user_id },
              { par: "config_number_id", va: dataVL.config_number_id },
              { par: "current_number", va: dataVL.current_number },
              { par: "year", va: dataVL.year },
              { par: "text_symbols", va: dataVL.text_symbols },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];

      if (data.length > 0) {
        devicecard.value.device_number =
          data[0].current_number +
          "/" +
          data[0].year +
          "/" +
          data[0].text_symbols;
      }
    })
    .catch((error) => {});
};
onMounted(() => {
  if (!checkURL(window.location.pathname, store.getters.listModule)) {
    //router.back();
  }
  loadProducerDevice();
  loadStatusDevice();
  loadOrganization(null);
  loadUser();
  loadWareHouse();
  loadDeviceType();
  loadDeviceUnit();
  initTudien();
  loadData(true);
  return {
    isFirst,
    options,
    danhMuc,
  };
});
</script>
    
    <template>
  <div class="d-container">
    <div class="d-lang-table">
      <DataTable
        class="w-full p-datatable-sm e-sm"
        @page="onPage($event)"
        @filter="onFilter($event)"
        @sort="onSort($event)"
        v-model:filters="filters"
        removableSort
        filterDisplay="menu"
        filterMode="lenient"
        dataKey="card_id"
        responsiveLayout="scroll"
        :scrollable="true"
        scrollHeight="flex"
        :showGridlines="true"
        :rows="options.pagesize"
        :lazy="true"
        :value="datalists"
        :loading="options.loading"
        :paginator="true"
        :totalRecords="options.totalRecords"
        :row-hover="true"
        v-model:first="first"
        v-model:selection="selectedCard"
        :pageLinkSize="options.pagesize"
        paginatorTemplate="FirstPageLink PrevPageLink PageLinks  NextPageLink LastPageLink    RowsPerPageDropdown"
        :rowsPerPageOptions="[20, 30, 50, 100, 200]"
      >
        <template #header>
          <div>
            <h3 class="module-title my-2 ml-1">
              <i class="pi pi-tags"></i> Danh sách thẻ thiết bị ({{
                options.totalRecords
              }})
            </h3>
          </div>
          <Toolbar class="d-toolbar p-0 py-3 surface-50">
            <template #start>
              <span class="p-input-icon-left">
                <i class="pi pi-search" />
                <InputText
                  v-model="options.search"
                  @keyup.enter="searchCard()"
                  type="text"
                  spellcheck="false"
                  placeholder="Tìm kiếm"
                />
                <!-- :class="checkFilter?'':'p-button-secondary'" -->
                <Button
                  :class="
                    (filterTrangthai != null ||
                      filterCardUser != null ||
                      filterCardType != null ||
                      filterCardWarehouse != null) &&
                    checkFilter
                      ? ''
                      : 'p-button-secondary p-button-outlined'
                  "
                  class="ml-2"
                  icon="pi pi-filter"
                  @click="toggleFilter"
                  aria-haspopup="true"
                  aria-controls="overlay_panelS"
                />
                <OverlayPanel
                  @hide="hideFilter"
                  ref="filterButs"
                  appendTo="body"
                  :showCloseIcon="false"
                  id="overlay_panelS"
                  style="width: 400px"
                  :breakpoints="{ '960px': '20vw' }"
                >
                  <div class="grid formgrid m-2">
                    <div class="field col-12 md:col-12 flex">
                      <div class="col-4 p-0 align-items-center flex">
                        Trạng thái:
                      </div>
                      <Dropdown
                        v-model="filterTrangthai"
                        :options="listSCard"
                        optionLabel="name"
                        optionValue="code"
                        placeholder="Trạng thái"
                        panelClass="d-design-dropdown"
                        class="col-8 p-0"
                        :style="
                          filterTrangthai != null
                            ? 'border:2px solid #2196f3'
                            : ''
                        "
                      />
                    </div>
                    <div class="field col-12 md:col-12 flex">
                      <div class="col-4 p-0 align-items-center flex">
                        Người sử dụng:
                      </div>
                      <Dropdown
                        v-model="filterCardUser"
                        panelClass="d-design-dropdown"
                        :options="listDropdownUser"
                        :filter="true"
                        optionLabel="name"
                        optionValue="code"
                        style="width: calc(100% - 10rem)"
                        class="w-full"
                        placeholder="Người sử dụng"
                      >
                        <template #option="slotProps">
                          <div class="country-item flex align-items-center">
                            <div class="grid w-full p-0">
                              <div
                                class="
                                  field
                                  p-0
                                  py-1
                                  col-12
                                  flex
                                  m-0
                                  cursor-pointer
                                  align-items-center
                                "
                              >
                                <div class="col-1 mx-2 p-0 align-items-center">
                                  <Avatar
                                    v-bind:label="
                                      slotProps.option.avatar
                                        ? ''
                                        : slotProps.option.name.substring(
                                            slotProps.option.name.lastIndexOf(
                                              ' '
                                            ) + 1,
                                            slotProps.option.name.lastIndexOf(
                                              ' '
                                            ) + 2
                                          )
                                    "
                                    :image="
                                      basedomainURL + slotProps.option.avatar
                                    "
                                    size="small"
                                    :style="
                                      slotProps.option.avatar
                                        ? 'background-color: #2196f3'
                                        : 'background:' +
                                          bgColor[
                                            slotProps.option.name.length % 7
                                          ]
                                    "
                                    shape="circle"
                                    @error="
                                      $event.target.src =
                                        basedomainURL +
                                        '/Portals/Image/nouser1.png'
                                    "
                                  />
                                </div>
                                <div class="col-11 p-0 pl-2 align-items-center">
                                  <div class="pt-2">
                                    <div class="font-bold">
                                      {{ slotProps.option.name }}
                                    </div>
                                    <div
                                      class="
                                        flex
                                        w-full
                                        text-sm
                                        font-italic
                                        text-500
                                      "
                                    >
                                      <div>
                                        {{ slotProps.option.position_name }}
                                      </div>
                                    </div>
                                    <!-- <div
                              class="flex w-full text-sm font-italic text-500"
                            >
                              {{ slotProps.option.department_name }}
                            </div> -->
                                  </div>
                                </div>
                              </div>
                            </div>
                          </div>
                        </template>
                      </Dropdown>
                    </div>
                    <div class="field col-12 md:col-12 flex">
                      <div class="col-4 p-0 align-items-center flex">
                        Loại thiết bị:
                      </div>
                      <Dropdown
                        v-model="filterCardType"
                        :options="listType"
                        :filter="true"
                        optionLabel="name"
                        optionValue="code"
                        panelClass="d-design-dropdown"
                        class="col-8 p-0"
                        placeholder="Loại thiết bị"
                      >
                      </Dropdown>
                    </div>
                    <div class="field col-12 md:col-12 flex">
                      <div class="col-4 p-0 align-items-center flex">Kho:</div>
                      <Dropdown
                        v-model="filterCardWarehouse"
                        :options="listWarehouse"
                        :filter="true"
                        optionLabel="name"
                        optionValue="code"
                        panelClass="d-design-dropdown"
                        placeholder="Kho"
                        class="w-full"
                      >
                      </Dropdown>
                    </div>
                    <div class="col-12 field p-0">
                      <Toolbar class="toolbar-filter">
                        <template #start>
                          <Button
                            @click="reFilterCard"
                            class="p-button-outlined"
                            label="Xóa"
                          ></Button>
                        </template>
                        <template #end>
                          <Button
                            @click="filterCard(true)"
                            label="Lọc"
                          ></Button>
                        </template>
                      </Toolbar>
                    </div>
                  </div>
                </OverlayPanel>
              </span>
              <Calendar
                placeholder="Lọc theo ngày mua"
                id="range"
                v-model="taskDateFilter"
                :showIcon="true"
                selectionMode="range"
                class="mx-2"
                :manualInput="false"
                @month-change="changeMonthPur($event)"
                @year-change="changeYearPur($event)"
              >
                <template #footer>
                  <div class="w-full flex">
                    <div class="w-4 format-center">
                      <span
                        @click="todayClick"
                        class="cursor-pointer text-primary"
                        >Hôm nay</span
                      >
                    </div>
                    <div class="w-4 format-center">
                      <Button @click="onDayClick" label="Thực hiện"></Button>
                    </div>
                    <div class="w-4 format-center">
                      <span
                        @click="delDayClick"
                        class="cursor-pointer text-primary"
                        >Xóa</span
                      >
                    </div>
                  </div>
                </template>
              </Calendar>
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
                @click="openBasic('Thêm mới')"
                label="Thêm mới"
                icon="pi pi-plus"
                class="mr-2"
              />

              <Button
                class="mr-2 p-button-outlined p-button-secondary"
                icon="pi pi-refresh"
                @click="refreshData"
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
          </Toolbar>
        </template>
        <Column
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;max-width:50px;height:50px"
          bodyStyle="text-align:center;max-width:50px"
          selectionMode="multiple"
        >
        </Column>
        <Column
          :sortable="true"
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;max-width:70px;height:50px"
          bodyStyle="text-align:center;max-width:70px;overflow: hidden;"
          field="is_order"
          header="STT"
        >
          <template #body="data">
            <div>
              {{ data.data.is_order }}
            </div>
          </template>
        </Column>
        <Column
          headerStyle="text-align:left;max-width:150px;height:50px"
          bodyStyle="text-align:left;max-width:150px;overflow: hidden;"
          field="device_number"
          header="Số hiệu"
          :sortable="true"
        >
          <template #body="data">
            <div>
              {{ data.data.device_number }}
            </div>
          </template>
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
          headerStyle="text-align:left;max-width:150px;height:50px"
          bodyStyle="text-align:left;max-width:150px;overflow: hidden;"
          field="barcode_id"
          header="Mã barcode"
          :sortable="true"
        >
          <template #body="data">
            <div>
              {{ data.data.barcode_id }}
            </div>
          </template>
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
          headerStyle="text-align:left;height:50px"
          bodyStyle="text-align:left; overflow: hidden;"
          field="device_name"
          header="Tên thiết bị"
          :sortable="true"
        >
          <template #body="data">
            <div>
              {{ data.data.device_name }}
            </div>
          </template>
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
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;max-width:105px;height:52px"
          bodyStyle="text-align:center;max-width:105px"
          field="image"
          header="Ảnh đại diện"
        >
          <template #body="data">
            <Image
              v-if="data.data.image"
              image-style="object-fit: cover; border: unset; outline: unset"
              width="100"
              height="50"
              alt=" "
              v-bind:src="
                data.data.image
                  ? basedomainURL + data.data.image
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
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;max-width:120px;height:50px"
          bodyStyle="text-align:center;max-width:120px;overflow: hidden;"
          field="purchase_date"
          header="Ngày mua"
        >
          <template #body="data">
            <div>
              {{
                moment(new Date(data.data.purchase_date)).format("DD/MM/YYYY")
              }}
            </div>
          </template>
        </Column>

        <Column
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;max-width:150px;height:50px"
          bodyStyle="text-align:center;max-width:150px;overflow: hidden;"
          field="status"
          header="Trạng thái"
        >
          <template #body="data">
            <div class="w-full">
              <Chip
                v-if="data.data.status == 'TPBG'"
                :label="data.data.device_status_name"
                v-tooltip.top="data.data.device_status_name"
                class="
                  textonelinec
                  w-full
                  surface-200
                  justify-content-center
                  p-button-status-d
                "
              />
              <Chip
                v-else-if="data.data.status == 'CXN'"
                :label="data.data.device_status_name"
                v-tooltip.top="data.data.device_status_name"
                class="
                  textonelinec
                  w-full
                  bg-pink-300
                  justify-content-center
                  p-button-status-d
                "
              />
              <Chip
                v-else-if="data.data.status == 'DTL'"
                :label="data.data.device_status_name"
                v-tooltip.top="data.data.device_status_name"
                style="background-color: red; color: white"
                class="
                  w-full
                  justify-content-center
                  p-button-status-d
                  textonelinec
                "
              />
              <Chip
                v-else-if="data.data.status == 'CNK'"
                :label="data.data.device_status_name"
                v-tooltip.top="data.data.device_status_name"
                class="
                  textonelinec
                  w-full
                  bg-yellow-300
                  justify-content-center
                  p-button-status-d
                "
              />
              <Chip
                v-else-if="data.data.is_recall == true"
                label="Đã thu hồi"
                v-tooltip.top="data.data.device_status_name"
                class="
                  textonelinec
                  w-full
                  bg-bluegray-300
                  justify-content-center
                  p-button-status-d
                "
              />
              <Chip
                v-else-if="data.data.status == 'TK'"
                :label="data.data.device_status_name"
                v-tooltip.top="data.data.device_status_name"
                class="
                  textonelinec
                  w-full
                  bg-green-300
                  justify-content-center
                  p-button-status-d
                "
              />
              <Chip
                v-else-if="data.data.status == 'DSC'"
                :label="data.data.device_status_name"
                v-tooltip.top="data.data.device_status_name"
                class="
                  textonelinec
                  w-full
                  bg-orange-300
                  justify-content-center
                  p-button-status-d
                "
              />
              <Chip
                v-else-if="data.data.status == 'DSD'"
                :label="data.data.device_status_name"
                v-tooltip.top="data.data.device_status_name"
                class="
                  textonelinec
                  w-full
                  bg-blue-300
                  justify-content-center
                  p-button-status-d
                "
              />
              <Chip
                v-else-if="data.data.status == 'HKS'"
                :label="data.data.device_status_name"
                v-tooltip.top="data.data.device_status_name"
                class="
                  textonelinec
                  w-full
                  bg-purple-300
                  justify-content-center
                  p-button-status-d
                "
              />
                 <Chip
                v-else-if="data.data.status == 'TPTH'"
                :label="data.data.device_status_name"
                v-tooltip.top="data.data.device_status_name"
                class="
                  textonelinec
                  w-full
                  bg-purple-300
                  justify-content-center
                  p-button-status-d
                "
              />
            </div>
          </template>
        </Column>
        <Column
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;max-width:180px;height:50px"
          bodyStyle="text-align:center;max-width:180px;overflow: hidden;"
          field="device_user_name"
          header="Sử dụng"
        ><template #body="data">
            <div
            >
            <span v-if="data.data.is_receiver_department">
  {{ data.data.department_use_name }}

            </span>
            <span v-else>{{data.data.device_user_name}}</span>
            
            </div>
          </template>
        </Column>
        <!-- <Column
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;max-width:180px;height:50px"
          bodyStyle="text-align:center;max-width:180px"
          field="manage_department_name"
          header="Bộ phận"
        >
          <template #body="data">
            <div
              :class="
                data.data.status == 0
                  ? 'text-yellow-500'
                  : data.data.status == 1
                  ? 'text-blue-500'
                  : data.data.status == 2
                  ? 'text-indigo-400'
                  : data.data.status == 3
                  ? 'text-cyan-900'
                  : data.data.status == 4
                  ? 'text-400'
                  : data.data.status == 5
                  ? 'text-red-500'
                  : data.data.status == 6
                  ? 'text-orange-500'
                  : data.data.status == 7
                  ? 'text-bluegray-500'
                  : ''
              "
            >
              {{ data.data.manage_department_name }}
            </div>
          </template>
        </Column> -->
        <Column
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;max-width:150px;height:50px"
          bodyStyle="text-align:center;max-width:150px"
          header="Chức năng"
        >
          <template #body="data">
            <Button
              v-tooltip.top="'Chi tiết'"
              @click="openDetails(data.data)"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-info-circle"
            ></Button>
            <Button
              v-if="
                (store.state.user.is_super == true ||
                  store.state.user.user_id == data.data.created_by ||
                  (store.state.user.role_id == 'admin' &&
                    store.state.user.organization_id ==
                      data.data.organization_id)) &&
                data.data.status == 'TK'
              "
              v-tooltip.top="'Sửa thẻ thiết bị'"
              @click="editCard(data.data)"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-pencil"
            ></Button>
            <Button
              v-if="
                (store.state.user.is_super == true ||
                  store.state.user.user_id == data.data.created_by ||
                  (store.state.user.role_id == 'admin' &&
                    store.state.user.organization_id ==
                      data.data.organization_id)) &&
                (data.data.status == 'TK' || data.data.status == 'HKS')
              "
              v-tooltip.top="'Xóa thẻ thiết bị'"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-trash"
              @click="delCard(data.data)"
            ></Button>
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
    <printCardVue
    :datas="devicecard"  
  />  <iframe name="printframe" id="printframe" style="display: none"></iframe>
  </div>


  <Dialog
    header="Chi tiết thẻ thiết bị"
    v-model:visible="displayDetails"
    :maximizable="true"
    :style="{ width: '80vw' }"
    :modal="true"
  >
    <div v-if="displayDetails && devicecard" style="min-height: 80vh">
      <detailsDevice :device="devicecard" />
    </div>

    <template #footer>
      <Button @click="closeDetails" label="Đóng" icon="pi pi-times" autofocus />
       <!-- <Button @click="print()" label="In phiếu" icon="pi pi-print" /> -->
    </template>
  </Dialog>
  <Dialog
    @hide="closeDialog"
    :header="headerDialog"
    :modal="true"
    v-model:visible="displayBasic"
    :maximizable="true"
    :style="{ width: '55vw' }"
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="col-12 field p-0 text-lg font-bold">Thông tin thẻ</div>
        <div class="col-12 flex p-0">
          <div class="col-6 p-0">
            <div class="col-12 field flex p-0 text-left align-items-center">
              <div class="w-10rem">
                Số hiệu<span class="redsao pl-1"> (*)</span>
              </div>
              <div style="width: calc(100% - 10rem)">
                <InputText
                  v-model="devicecard.device_number"
                  class="w-full"
                  :class="{
                    'p-invalid': v$.device_number.$invalid && submitted,
                  }"
                />
              </div>
            </div>
            <div
              v-if="
                (v$.device_number.$invalid && submitted) ||
                v$.device_number.$pending.$response
              "
              class="col-12 field p-0 flex"
            >
              <div class="w-10rem"></div>
              <small style="width: calc(100% - 10rem)">
                <span style="color: red" class="w-full">{{
                  v$.device_number.required.$message
                    .replace("Value", "Số hiệu")
                    .replace("is required", "không được để trống!")
                }}</span>
              </small>
            </div>
          </div>
          <div class="col-6 flex p-0 text-center align-items-center">
            <div class="col-4 p-0 pl-5 text-left">Loại mã</div>
            <div class="col-4 p-0 flex text-left align-items-center">
              <div class="col-6 p-0">Barcode</div>
              <div class="col-6 p-0">
                <InputSwitch
                  @change="onChangeBarcode(1)"
                  class="w-4rem lck-checked"
                  v-model="devicecard.barcode"
                />
              </div>
            </div>
            <div class="col-4 p-0 flex text-center align-items-center">
              <div class="col-6 p-0">QR Code</div>
              <div class="col-6 p-0">
                <InputSwitch
                  @change="onChangeBarcode(2)"
                  class="w-4rem lck-checked"
                  v-model="devicecard.qrcode"
                />
              </div>
            </div>
          </div>
        </div>
        <div class="col-12 flex p-0">
          <div class="col-6 p-0 text-left align-items-center">
            <div class="col-12 field p-0 flex text-left align-items-center">
              <div class="w-10rem">
                Mã Barcode<span class="redsao pl-1"> (*)</span>
              </div>
              <div style="width: calc(100% - 10rem)">
                <div class="col-12 p-0">
                  <div class="p-inputgroup">
                    <InputText
                      placeholder="Nhập và click nút bên cạnh để tạo mã!"
                      v-model="devicecard.barcode_id"
                      :class="{
                        'p-invalid': v$.barcode_id.$invalid && submitted,
                      }"
                    />

                    <Button @click="onConvertBarCode(true)">
                      <font-awesome-icon icon="fa-solid fa-barcode" />
                    </Button>
                  </div>
                </div>
              </div>
            </div>
            <div
              v-if="
                (v$.barcode_id.$invalid && submitted) ||
                v$.barcode_id.$pending.$response
              "
              class="col-12 field p-0 flex"
            >
              <div class="w-10rem"></div>
              <small style="width: calc(100% - 10rem)">
                <span style="color: red" class="w-full">{{
                  v$.barcode_id.required.$message
                    .replace("Value", "Mã Barcode")
                    .replace("is required", "không được để trống!")
                }}</span>
              </small>
            </div>
            <div class="col-12 field p-0 flex text-left align-items-center">
              <div class="w-10rem">
                Trạng thái<span class="redsao pl-1"> (*)</span>
              </div>
              <div style="width: calc(100% - 10rem)">
                <Dropdown
                  v-model="devicecard.status"
                  :options="listSCard"
                  optionLabel="name"
                  optionValue="code"
                  placeholder="Chọn trạng thái"
                  class="w-full"
                  :class="{
                    'p-invalid': !devicecard.status && submitted,
                  }"
                  :disabled="selectedUser.used"
                />
              </div>
            </div>
            <div
              v-if="!devicecard.status && submitted"
              class="col-12 field p-0 flex"
            >
              <div class="w-10rem"></div>
              <small style="width: calc(100% - 10rem)">
                <span style="color: red" class="w-full"
                  >Trạng thái không được để trống</span
                >
              </small>
            </div>
            <div class="col-12 field p-0 flex text-left align-items-center">
              <div class="w-10rem">Ngày mua</div>
              <div style="width: calc(100% - 10rem)">
                <Calendar
                  class="w-full"
                  id="basic_purchase_date"
                  v-model="devicecard.purchase_date"
                  autocomplete="on"
                  :showIcon="true"
                />
              </div>
            </div>

            <div class="col-12 field p-0 flex text-left align-items-center">
              <div class="w-10rem">Nhà sản xuất</div>
              <div style="width: calc(100% - 10rem)">
                <Dropdown
                  v-model="devicecard.producer"
                  :options="listPCard"
                  :filter="true"
                  optionLabel="name"
                  optionValue="code"
                  placeholder="Chọn nhà sản xuất"
                  class="w-full"
                  @change="onChangeCountry()"
                >
                  <template #option="slotProps">
                    <div class="country-item flex">
                      <Avatar
                        v-bind:label="
                          slotProps.option.logo
                            ? ''
                            : slotProps.option.name.substring(
                                slotProps.option.name.lastIndexOf(' ') + 1,
                                slotProps.option.name.lastIndexOf(' ') + 2
                              )
                        "
                        :style="
                          slotProps.option.logo
                            ? 'background-color: #2196f3'
                            : 'background:' +
                              bgColor[slotProps.option.name.length % 7]
                        "
                        :image="
                          slotProps.option.logo
                            ? basedomainURL + slotProps.option.logo
                            : basedomainURL + '/Portals/Image/nouser1.png'
                        "
                        class="mr-2 w-2rem h-2rem"
                        size="large"
                        shape="circle"
                      />
                      <div class="pt-1">{{ slotProps.option.name }}</div>
                    </div>
                  </template>
                </Dropdown>
              </div>
            </div>

            <div class="col-12 field p-0 flex text-left align-items-center">
              <div class="w-10rem">Nước sản xuất</div>
              <div style="width: calc(100% - 10rem)">
                <InputText
                  class="w-full"
                  v-model="devicecard.manufacture_country"
                />
              </div>
            </div>

            <div class="col-12 field p-0 flex text-left align-items-center">
              <div class="w-10rem">Năm sản xuất</div>
              <div style="width: calc(100% - 10rem)">
                <Calendar
                  class="w-full"
                  inputId="yearpicker"
                  v-model="devicecard.manufacture_year_fake"
                  view="year"
                  dateFormat="yy"
                />
              </div>
            </div>
          </div>
          <div class="col-6 p-0">
            <div
              class="col-12 p-0 text-center align-items-center format-center"
            >
              <div class="col-4"></div>
              <div class="col-8 p-0">
                <img
                  :src="
                    devicecard.barcode_image
                      ? basedomainURL + devicecard.barcode_image
                      : basedomainURL + '/Portals/Image/noimg.jpg'
                  "
                  v-if="devicecard.barcode_image"
                  style="object-fit: cover"
                  :style="
                    devicecard.barcode
                      ? 'height:50px;width:150px'
                      : 'height:150px; width:150px'
                  "
                />
              </div>
            </div>
            <div class="col-12 p-0 field pt-3 flex">
              <div class="col-4 p-0 pl-5 text-left">Chọn ảnh</div>
              <div class="col-8 p-0 relative">
                <img
                  v-tooltip.top="'Ảnh đại diện'"
                  @click="chonanh('AnhLang')"
                  class="inputanh p-0"
                  id="logoLang"
                  :style="devicecard.image ? 'object-fit:cover' : ''"
                  v-bind:src="
                    devicecard.image
                      ? basedomainURL + devicecard.image
                      : basedomainURL + '/Portals/Image/noimg.jpg'
                  "
                />
                <Button
                  v-if="files.length > 0 || devicecard.image"
                  icon="pi pi-times"
                  @click="delAvatar"
                  class="p-button-rounded absolute top-0 right-0"
                />

                <input
                  class="ipnone"
                  id="AnhLang"
                  type="file"
                  accept=".png,.jpg,.jpeg,.gif,.raw"
                  @change="handleFileUpload"
                />
              </div>
            </div>
          </div>
        </div>
        <div class="col-12 field flex p-0 align-items-center">
          <div class="col-6 p-0 flex align-items-center">
            <div class="w-10rem">Đợt nhập</div>
            <div style="width: calc(100% - 10rem)">
              <InputNumber
                class="w-full"
                :min="1"
                v-model="devicecard.import_batch"
              />
            </div>
          </div>
          <div class="col-6 p-0 flex align-items-center">
            <div class="w-10rem pl-5">Năm nhập</div>
            <div style="width: calc(100% - 10rem)">
              <Calendar
                class="w-full"
                inputId="yearpicker"
                v-model="devicecard.import_year_fake"
                view="year"
                dateFormat="yy"
              />
            </div>
          </div>
        </div>
        <div class="col-12 field flex p-0">
          <div class="col-6 p-0 flex align-items-center">
            <div class="w-10rem">Số CV,QĐ</div>
            <div style="width: calc(100% - 10rem)">
              <InputText class="w-full" v-model="devicecard.dispatch_number" />
            </div>
          </div>
          <div class="col-6 p-0 flex align-items-center">
            <div class="w-10rem pl-5">
              Pháp nhân<span class="redsao pl-1"> (*) </span>
            </div>
            <div style="width: calc(100% - 10rem)">
              <TreeSelect
                class="w-full"
                v-model="devicecard.corporation"
                :options="treedonvis"
                :showClear="true"
                :max-height="200"
                optionLabel="label"
                optionValue="data"
                panelClass="d-design-dropdown"
                :class="{ 'p-invalid': !devicecard.corporation && submitted }"
              >
              </TreeSelect>
            </div>
          </div>
        </div>
        <div
          v-if="!devicecard.corporation && submitted"
          class="col-12 field p-0 flex"
        >
          <div class="w-10rem"></div>
          <small style="width: calc(100% - 10rem)">
            <span style="color: red" class="w-full"
              >Pháp nhân không được để trống!</span
            >
          </small>
        </div>
        <div class="col-12 field flex p-0 align-items-center">
          <div class="w-10rem">Khu vực</div>
          <div style="width: calc(100% - 10rem)">
            <InputText v-model="devicecard.device_area" class="w-full" />
          </div>
        </div>
        <div class="col-12 field p-0 text-lg font-bold">
          <Toolbar class="p-0 surface-0 border-none">
            <template #start>
              <div class="text-lg font-bold">Thông tin thiết bị</div>
            </template>
            <template #end>
              <div class="flex" v-if="!isSaveCard">
                <Button
                  label="Chọn thiết bị"
                  icon="pi pi-list"
                  class="mr-2 p-button-outlined"
                  @click="showListAssets"
                />
                <!-- <Button
                  label="Chọn từ phiếu"
                  icon="pi pi-book"
                  class="p-button-outlined"
                /> -->
              </div>
            </template>
          </Toolbar>
        </div>
        <div class="col-12 field flex p-0 align-items-center">
          <div class="w-10rem">
            Tên thiết bị<span class="redsao pl-1"> (*)</span>
          </div>
          <div style="width: calc(100% - 10rem)">
            <Textarea
              :autoResize="true"
              rows="2"
              cols="30"
              v-model="devicecard.device_name"
              class="w-full surface-200"
              disabled
              :class="
                !devicecard.device_name && v$.device_name.$invalid && submitted
                  ? 'surface-200 p-invalid'
                  : ''
              "
              :style="
                devicecard.device_name
                  ? 'background-color:white !important'
                  : ''
              "
            />
          </div>
        </div>
        <div
          v-if="
            (v$.device_name.$invalid && submitted) ||
            v$.device_name.$pending.$response
          "
          class="col-12 field p-0 flex"
        >
          <div class="w-10rem"></div>
          <small style="width: calc(100% - 10rem)">
            <span style="color: red" class="w-full">{{
              v$.device_name.required.$message
                .replace("Value", "")
                .replace("is required", "Vui lòng chọn thiết bị!")
            }}</span>
          </small>
        </div>
        <div class="col-12 flex p-0">
          <div class="col-6 p-0 text-left align-items-center">
            <div class="col-12 field p-0 flex text-left align-items-center">
              <div class="w-10rem">
                Giá trị<span class="redsao pl-1"> (*)</span>
              </div>
              <InputNumber
                style="width: calc(100% - 10rem)"
                v-model="devicecard.price"
                mode="currency"
                currency="VND"
                :class="{ 'p-invalid': v$.price.$invalid && submitted }"
              />
            </div>
            <div
              v-if="
                (v$.price.$invalid && submitted) || v$.price.$pending.$response
              "
              class="col-12 field p-0 flex"
            >
              <div class="w-10rem"></div>
              <small style="width: calc(100% - 10rem)">
                <span style="color: red" class="w-full">{{
                  v$.price.required.$message
                    .replace("Value", "Giá trị")
                    .replace("is required", "không được để trống!")
                }}</span>
              </small>
            </div>
            <div class="col-12 field p-0 flex text-left align-items-center">
              <div class="w-10rem">Chu kỳ bảo hành</div>
              <InputNumber
                style="width: calc(100% - 10rem)"
                v-model="devicecard.insurance_cycle"
                suffix=" Tháng"
              />
            </div>
            <div class="col-12 field p-0 flex text-left align-items-center">
              <div class="w-10rem">
                Tháng khấu hao <span class="redsao pl-1"> (*)</span>
              </div>
              <InputNumber
                suffix=" Tháng"
                style="width: calc(100% - 10rem)"
                v-model="devicecard.depreciation_month"
                :class="{
                  'p-invalid': v$.depreciation_month.$invalid && submitted,
                }"
              />
            </div>
            <div
              v-if="
                (v$.depreciation_month.$invalid && submitted) ||
                v$.depreciation_month.$pending.$response
              "
              class="col-12 field p-0 flex"
            >
              <div class="w-10rem"></div>
              <small style="width: calc(100% - 10rem)">
                <span style="color: red" class="w-full">{{
                  v$.depreciation_month.required.$message
                    .replace("Value", "Tháng khấu hao")
                    .replace("is required", "không được để trống!")
                }}</span>
              </small>
            </div>
            <div class="col-12 field p-0 flex text-left align-items-center">
              <div class="w-10rem">Bộ phận quản lý</div>
              <Dropdown
                v-model="devicecard.manage_department_id"
                :options="listDepartment"
                optionLabel="label"
                optionValue="data"
                disabled
                class="surface-300"
                style="width: calc(100% - 10rem)"
              />
            </div>
          </div>
          <div class="col-6 p-0 text-left align-items-center">
            <div class="col-12 field p-0 flex text-left align-items-center">
              <div class="w-10rem pl-2">
                Đơn vị tính<span class="redsao pl-1"> (*)</span>
              </div>
              <Dropdown
                v-model="devicecard.device_unit"
                :options="listUnit"
                disabled
                optionLabel="name"
                optionValue="code"
                placeholder="--------- Đơn vị tính ---------"
                spellcheck="false"
                class="sel-placeholder"
                style="width: calc(100% - 10rem)"
                :class="{ 'p-invalid': !devicecard.device_unit && submitted }"
              />
            </div>
            <div
              v-if="!devicecard.device_unit && submitted"
              class="col-12 field p-0 flex"
            >
              <div class="w-10rem"></div>
              <small style="width: calc(100% - 10rem)">
                <span style="color: red" class="w-full"
                  >Đơn vị tính không được để trống!</span
                >
              </small>
            </div>
            <div class="col-12 field p-0 flex text-left align-items-center">
              <div class="w-10rem pl-2">Thời gian bảo hành</div>
              <InputNumber
                style="width: calc(100% - 10rem)"
                suffix=" Tháng"
                v-model="devicecard.insurance_month"
              />
            </div>
            <div class="col-12 field p-0 flex text-left align-items-center">
              <div class="w-10rem pl-2">Người sử dụng</div>
              <Dropdown
                v-model="devicecard.device_user_id"
                :options="listDropdownUser"
                optionLabel="name"
                optionValue="code"
                disabled
                style="width: calc(100% - 10rem)"
                ><template #option="slotProps">
                  <div class="country-item flex align-items-center">
                    <div class="grid w-full p-0">
                      <div
                        class="
                          field
                          p-0
                          py-1
                          col-12
                          flex
                          m-0
                          cursor-pointer
                          align-items-center
                        "
                      >
                        <div class="col-1 mx-2 p-0 align-items-center">
                          <Avatar
                            v-bind:label="
                              slotProps.option.avatar
                                ? ''
                                : slotProps.option.name.substring(
                                    slotProps.option.name.lastIndexOf(' ') + 1,
                                    slotProps.option.name.lastIndexOf(' ') + 2
                                  )
                            "
                            :image="basedomainURL + slotProps.option.avatar"
                            size="small"
                            :style="
                              slotProps.option.avatar
                                ? 'background-color: #2196f3'
                                : 'background:' +
                                  bgColor[slotProps.option.name.length % 7]
                            "
                            shape="circle"
                            @error="
                              $event.target.src =
                                basedomainURL + '/Portals/Image/nouser1.png'
                            "
                          />
                        </div>
                        <div class="col-11 p-0 pl-2 align-items-center">
                          <div class="pt-2">
                            <div class="font-bold">
                              {{ slotProps.option.name }}
                            </div>
                            <div
                              class="flex w-full text-sm font-italic text-500"
                            >
                              <div>{{ slotProps.option.position_name }}</div>
                            </div>
                            <!-- <div
                              class="flex w-full text-sm font-italic text-500"
                            >
                              {{ slotProps.option.department_name }}
                            </div> -->
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>
                </template>
              </Dropdown>
            </div>
          </div>
        </div>
        <div class="col-12 field flex p-0 align-items-center">
          <div class="w-10rem">Mô tả</div>
          <div style="width: calc(100% - 10rem)">
            <Textarea
              :autoResize="true"
              rows="2"
              cols="30"
              v-model="devicecard.device_des"
              class="w-full"
            />
          </div>
        </div>
        <div class="col-12 field flex p-0 align-items-center">
          <div class="w-10rem">Ghi chú</div>
          <div style="width: calc(100% - 10rem)">
            <Textarea
              :autoResize="true"
              rows="2"
              cols="30"
              v-model="devicecard.device_note"
              class="w-full"
            />
          </div>
        </div>
        <div class="col-12 field flex p-0 align-items-center">
          <div class="w-10rem">Kho<span class="redsao pl-1"> (*)</span></div>
          <Dropdown
            v-model="devicecard.warehouse_id"
            :options="listWarehouse"
            optionLabel="name"
            optionValue="code"
            panelClass="d-design-dropdown"
            :filter="true"
            :class="{ 'p-invalid': !devicecard.warehouse_id && submitted }"
            style="width: calc(100% - 10rem)"
          />
        </div>
        <div
          v-if="!devicecard.warehouse_id && submitted"
          class="col-12 field p-0 flex"
        >
          <div class="w-10rem"></div>
          <small style="width: calc(100% - 10rem)">
            <span style="color: red" class="w-full"
              >Kho không được để trống!</span
            >
          </small>
        </div>
        <div class="col-12 field flex p-0 align-items-center">
          <div class="w-10rem">
            Tình trạng<span class="redsao pl-1"> (*)</span>
          </div>
          <div style="width: calc(100% - 10rem)">
            <Textarea
              :autoResize="true"
              rows="2"
              cols="30"
              v-model="devicecard.assets_condition"
              class="w-full"
              :class="{
                'p-invalid': v$.assets_condition.$invalid && submitted,
              }"
            />
          </div>
        </div>
        <div
          v-if="
            (v$.assets_condition.$invalid && submitted) ||
            v$.assets_condition.$pending.$response
          "
          class="col-12 field p-0 flex"
        >
          <div class="w-10rem"></div>
          <small style="width: calc(100% - 10rem)">
            <span style="color: red" class="w-full">{{
              v$.assets_condition.required.$message
                .replace("Value", "Tình trạng")
                .replace("is required", "không được để trống!")
            }}</span>
          </small>
        </div>
        <div class="col-12 field p-0 text-lg font-bold">
          Thông tin công ty bảo hành
        </div>
        <div class="col-12 field flex p-0 align-items-center">
          <div class="w-10rem">Tên công ty</div>
          <div style="width: calc(100% - 10rem)">
            <InputText
              :autoResize="true"
              v-model="devicecard.warranty_company"
              class="w-full"
            />
          </div>
        </div>
        <div class="col-12 field flex p-0 align-items-center">
          <div class="col-6 field flex p-0 align-items-center">
            <div class="w-10rem">Điện thoại</div>
            <div style="width: calc(100% - 10rem)">
              <InputText
                :autoResize="true"
                v-model="devicecard.warranty_phone"
                class="w-full"
                @keyup="checkNumber($event, 'warranty_phone')"
              />
            </div>
          </div>
          <div class="col-6 field flex p-0 align-items-center">
            <div class="w-10rem pl-2">Người liên hệ</div>
            <div style="width: calc(100% - 10rem)">
              <InputText
                :autoResize="true"
                v-model="devicecard.warranty_contact"
                class="w-full"
              />
            </div>
          </div>
        </div>
        <div class="col-12 flex p-0 align-items-center">
          <div class="w-10rem">Địa chỉ</div>
          <div style="width: calc(100% - 10rem)">
            <Textarea
              :autoResize="true"
              rows="2"
              cols="30"
              v-model="devicecard.warranty_company_address"
              class="w-full"
            />
          </div>
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="closeDialogDC()"
      class="p-button-outlined"
      />

      <Button
        @click="saveCard(!v$.$invalid)"
        label="Lưu"
        icon="pi pi-check"
        autofocus
      />
    </template>
  </Dialog>

  <Dialog
    header="Chọn thiết bị từ danh sách"
    v-model:visible="displayAssets"
    :maximizable="true"
    :modal="true"
    :style="{ width: '55vw' }"
  >
    <div>
      <div class="true p-2" id="idScroll-2022">
        <div class="w-full d-device-table">
          <DataTable
            v-model:first="firstDM"
            :show-gridlines="true"
            :value="datalistsDM"
            :paginator="true"
            :rows="options.pagesizeDM"
            paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink"
            :rowsPerPageOptions="[20, 30, 50, 100, 200]"
            :scrollable="true"
            scrollHeight="flex"
            :loading="options.loading"
            dataKey="device_id"
            v-model:selection="selectedDeviceMain"
            selectionMode="single"
            @page="onPageDM($event)"
            @sort="onSortDM($event)"
            @filter="onFilterDM($event)"
            @row-click="onSelectedDM($event)"
            :lazy="true"
            v-model:filters="filtersDM"
            filterDisplay="menu"
            filterMode="lenient"
            :totalRecords="options.totalRecordsDM"
            :row-hover="true"
          >
            <template #header>
              <Toolbar class="w-full custoolbar">
                <template #start>
                  <div>
                    <span class="p-input-icon-left">
                      <i class="pi pi-search" />
                      <InputText
                        v-model="options.SearchTextDM"
                        @keyup.enter="searchDeviceMain"
                        type="text"
                        spellcheck="false"
                        placeholder="Tìm kiếm"
                      />
                    </span>
                  </div>
                  <div>
                    <Dropdown
                      v-model="options.device_card_type"
                      :options="listType"
                      :filter="true"
                      optionLabel="name"
                      optionValue="code"
                      @change="filterDeviceMain()"
                      class="ml-2 w-15rem"
                      panelClass="d-design-dropdown"
                      placeholder="Loại thiết bị"
                      :showClear="true"
                    />
                  </div>
                </template>

                <template #end> </template> </Toolbar
            ></template>

            <Column
              selectionMode="single"
              headerStyle="text-align:center;max-width:75px;height:50px"
              bodyStyle="text-align:center;max-width:75px"
              class="align-items-center justify-content-center text-center"
            ></Column>
            <Column
              field="STT"
              header="STT"
              headerStyle="text-align:center;max-width:75px;height:50px"
              bodyStyle="text-align:center;max-width:75px;"
              class="align-items-center justify-content-center text-center"
            >
            </Column>

            <Column
              field="device_name"
              :sortable="true"
              header="Tên thiết bị"
              headerStyle="height:50px place-content: center;place-items: center;height: 50px"
            >
              <template #filter="{ filterModel }">
                <InputText
                  type="text"
                  v-model="filterModel.value"
                  class="p-column-filter"
                  placeholder="Từ khoá"
                /> </template
            ></Column>
            <Column
              field="device_type_name"
              header="Loại thiết bị"
              class="align-items-center justify-content-center text-center"
              headerStyle="height:50px place-content: center;place-items: center;height: 50px; max-width:150px"
              bodyStyle=" max-width:150px"
            ></Column>
            <Column
              field="device_unit_name"
              header="Đơn vị tính"
              class="align-items-center justify-content-center text-center"
              headerStyle="height:50px place-content: center;place-items: center;height: 50px; max-width:150px"
              bodyStyle="max-width:150px"
            >
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
          <div class="w-full">
            <div class="grid p-3">
              <div class="col-12 field p-0 text-lg font-bold">
                <Toolbar class="p-0 surface-0 border-none">
                  <template #start>
                    <div class="text-xl font-bold">Thông tin thiết bị</div>
                  </template>
                </Toolbar>
              </div>
              <div class="col-12 field flex p-0 align-items-center">
                <div class="col-6 p-0 flex align-items-center">
                  <div class="pr-2 text-lg font-bold">
                    Tài sản chưa có người sử dụng :
                  </div>

                  <InputSwitch
                    @change="onChangeUsed1()"
                    class="w-4rem lck-checked"
                    v-model="selectedUser.not_used"
                  />
                </div>
                <div class="col-6 p-0 flex align-items-center">
                  <div class="pr-2 text-lg font-bold">
                    Tài sản có người sử dụng :
                  </div>

                  <InputSwitch
                    @change="onChangeUsed()"
                    class="w-4rem lck-checked"
                    v-model="selectedUser.used"
                  />
                </div>
              </div>
              <div class="col-12 field flex p-0 align-items-center">
                <div class="col-6 flex p-0 align-items-center">
                  <div class="w-10rem">Tên thiết bị</div>
                  <div style="width: calc(100% - 10rem)">
                    <InputText
                      v-model="selectedDeviceMain.device_name"
                      class="w-full surface-200"
                      disabled
                      :style="
                        selectedDeviceMain.device_name
                          ? 'background-color:white !important'
                          : ''
                      "
                    />
                  </div>
                </div>
                <div class="col-6 p-0 p-0 align-items-center flex">
                  <div class="w-10rem pl-2">Đơn vị tính</div>
                  <Dropdown
                    v-model="selectedDeviceMain.device_unit_id"
                    :options="listUnit"
                    optionLabel="name"
                    optionValue="code"
                    placeholder="--------- Đơn vị tính ---------"
                    spellcheck="false"
                    class="sel-placeholder"
                    style="width: calc(100% - 10rem)"
                    disabled
                  />
                </div>
              </div>

              <div class="col-12 flex p-0">
                <div class="col-6 p-0 text-left align-items-center">
                  <div
                    class="col-12 field p-0 flex text-left align-items-center"
                  >
                    <div class="w-10rem">
                      Giá trị <span class="redsao"> (*)</span>
                    </div>
                    <InputNumber
                      mode="currency"
                      currency="VND"
                      style="width: calc(100% - 10rem)"
                      v-model="selectedDeviceMain.price"
                      :class="{
                        'p-invalid': vDM$.price.$invalid && submittedDM,
                      }"
                    />
                  </div>
                  <div
                    v-if="
                      (vDM$.price.$invalid && submittedDM) ||
                      vDM$.price.$pending.$response
                    "
                    class="col-12 field p-0 flex"
                  >
                    <div class="w-10rem"></div>
                    <small style="width: calc(100% - 10rem)">
                      <span style="color: red" class="w-full">{{
                        vDM$.price.required.$message
                          .replace("Value", "Giá trị")
                          .replace("is required", "không được để trống!")
                      }}</span>
                    </small>
                  </div>
                  <div
                    class="col-12 field p-0 flex text-left align-items-center"
                  >
                    <div class="w-10rem">Chu kỳ bảo hành</div>
                    <InputNumber
                      style="width: calc(100% - 10rem)"
                      v-model="selectedDeviceMain.insurance_cycle"
                      suffix=" Tháng"
                    />
                  </div>
                  <div
                    v-if="selectedUser.used"
                    class="col-12 field p-0 flex text-left align-items-center"
                  >
                    <div class="w-10rem">
                      Người sử dụng <span class="redsao"> (*)</span>
                    </div>
                    <Dropdown
                      v-model="selectedDeviceMain.device_user_id"
                      :options="listDropdownUser"
                      :filter="true"
                      optionLabel="name"
                      optionValue="code"
                      style="width: calc(100% - 10rem)"
                      :class="{
                        'p-invalid':
                          !selectedDeviceMain.device_user_id && submittedDM,
                      }"
                      @change="onChangeUser(selectedDeviceMain.device_user_id)"
                    >
                      <template #option="slotProps">
                        <div class="country-item flex align-items-center">
                          <div class="grid w-full p-0">
                            <div
                              class="
                                field
                                p-0
                                py-1
                                col-12
                                flex
                                m-0
                                cursor-pointer
                                align-items-center
                              "
                            >
                              <div class="col-1 mx-2 p-0 align-items-center">
                                <Avatar
                                  v-bind:label="
                                    slotProps.option.avatar
                                      ? ''
                                      : slotProps.option.name.substring(
                                          slotProps.option.name.lastIndexOf(
                                            ' '
                                          ) + 1,
                                          slotProps.option.name.lastIndexOf(
                                            ' '
                                          ) + 2
                                        )
                                  "
                                  :image="
                                    basedomainURL + slotProps.option.avatar
                                  "
                                  size="small"
                                  :style="
                                    slotProps.option.avatar
                                      ? 'background-color: #2196f3'
                                      : 'background:' +
                                        bgColor[
                                          slotProps.option.name.length % 7
                                        ]
                                  "
                                  shape="circle"
                                  @error="
                                    $event.target.src =
                                      basedomainURL +
                                      '/Portals/Image/nouser1.png'
                                  "
                                />
                              </div>
                              <div class="col-11 p-0 pl-2 align-items-center">
                                <div class="pt-2">
                                  <div class="font-bold">
                                    {{ slotProps.option.name }}
                                  </div>
                                  <div
                                    class="
                                      flex
                                      w-full
                                      text-sm
                                      font-italic
                                      text-500
                                    "
                                  >
                                    <div>
                                      {{ slotProps.option.position_name }}
                                    </div>
                                  </div>
                                  <!-- <div
                              class="flex w-full text-sm font-italic text-500"
                            >
                              {{ slotProps.option.department_name }}
                            </div> -->
                                </div>
                              </div>
                            </div>
                          </div>
                        </div>
                      </template>
                    </Dropdown>
                  </div>
                  <div
                    v-if="
                      !selectedDeviceMain.device_user_id &&
                      submittedDM &&
                      selectedUser.used
                    "
                    class="col-12 field p-0 flex"
                  >
                    <div class="w-10rem"></div>
                    <small style="width: calc(100% - 10rem)">
                      <span style="color: red" class="w-full"
                        >Người sử dụng không được để trống !</span
                      >
                    </small>
                  </div>
                  <div class="col-12 field flex p-0 align-items-center">
                    <div class="w-10rem">
                      Kho <span class="redsao"> (*)</span>
                    </div>
                    <Dropdown
                      v-model="selectedDeviceMain.warehouse_id"
                      :options="listWarehouse"
                      optionLabel="name"
                      optionValue="code"
                      :filter="true"
                      class="sel-placeholder"
                      panelClass="d-design-dropdown"
                      placeholder="----Chọn kho----"
                      style="width: calc(100% - 10rem)"
                      :class="{
                        'p-invalid':
                          !selectedDeviceMain.warehouse_id && submittedDM,
                      }"
                    />
                  </div>
                  <div
                    v-if="!selectedDeviceMain.warehouse_id && submittedDM"
                    class="col-12 field p-0 flex"
                  >
                    <div class="w-10rem"></div>
                    <small style="width: calc(100% - 10rem)">
                      <span style="color: red" class="w-full"
                        >Kho không được để trống !</span
                      >
                    </small>
                  </div>
                </div>
                <div class="col-6 p-0 text-left align-items-center">
                  <div
                    class="col-12 field p-0 flex text-left align-items-center"
                  >
                    <div class="w-10rem pl-2">Thời gian bảo hành</div>
                    <InputNumber
                      style="width: calc(100% - 10rem)"
                      v-model="selectedDeviceMain.insurance_month"
                      suffix=" Tháng"
                    />
                  </div>
                  <div
                    class="col-12 field p-0 flex text-left align-items-center"
                  >
                    <div class="w-10rem pl-2">Tháng khấu hao</div>
                    <InputNumber
                      style="width: calc(100% - 10rem)"
                      v-model="selectedDeviceMain.depreciation_month"
                      suffix=" Tháng"
                    />
                  </div>
                  <div
                    v-if="selectedUser.used"
                    class="col-12 field p-0 flex text-left align-items-center"
                  >
                    <div class="w-10rem pl-2">Phòng ban</div>
                    <InputText
                      disabled
                      v-model="selectedDeviceMain.device_department_name"
                      style="width: calc(100% - 10rem)"
                    />
                  </div>

                  <div class="col-12 p-0">
                    <div
                      v-if="selectedUser.used"
                      class="col-12 field p-0 flex text-left align-items-center"
                    >
                      <div class="w-10rem pl-2">
                        Bộ phận quản lý <span class="redsao"> (*)</span>
                      </div>

                      <TreeSelect
                        v-model="selectedDeviceMain.device_department_id"
                        :options="listDepartment"
                        :showClear="true"
                        :max-height="200"
                        style="width: calc(100% - 10rem)"
                        placeholder="---------- Đơn vị ----------"
                        optionLabel="data.organization_name"
                        optionValue="data.department_id"
                        panelClass="d-design-dropdown"
                        class="sel-placeholder"
                        :class="{
                          'p-invalid':
                            !selectedDeviceMain.device_department_id &&
                            submittedDM,
                        }"
                      >
                      </TreeSelect>
                    </div>
                    <div
                      v-if="
                        !selectedDeviceMain.device_department_id &&
                        submittedDM &&
                        selectedUser.used
                      "
                      class="col-12 field p-0 flex"
                    >
                      <div class="w-10rem"></div>
                      <small style="width: calc(100% - 10rem)">
                        <span style="color: red" class="w-full"
                          >Bộ phận quản lý không được để trống !</span
                        >
                      </small>
                    </div>
                  </div>
                </div>
              </div>
              <div class="col-12 field flex p-0 align-items-center">
                <div class="w-10rem">Mô tả</div>
                <div style="width: calc(100% - 10rem)">
                  <Textarea
                    :autoResize="true"
                    rows="2"
                    cols="30"
                    v-model="selectedDeviceMain.device_des"
                    class="w-full"
                  />
                </div>
              </div>
              <div class="col-12 field flex p-0 align-items-center">
                <div class="w-10rem">Ghi chú</div>
                <div style="width: calc(100% - 10rem)">
                  <Textarea
                    :autoResize="true"
                    rows="2"
                    cols="30"
                    v-model="selectedDeviceMain.device_note"
                    class="w-full"
                  />
                </div>
              </div>
            </div>
            <div class="p-0" id="scrollDM">
              <Toolbar class="p-2 surface-0 border-none">
                <template #end>
                  <Button
                    @click="hideSelectDevice()"
                    label="Hủy"
                    icon="pi pi-times"
                    class="mr-2 p-button-outlined"
                  />
                  <Button
                    @click="onSelectDevice(!vDM$.$invalid)"
                    label="Chọn"
                    icon="pi pi-check"
                    autofocus
                  />
                </template>
              </Toolbar>
            </div>
          </div>
        </div>
      </div>
    </div>
  </Dialog>
</template>
    <style scoped>
.d-device-table {
  height: 80vh;
}
.ck-editor__editable {
  max-height: 500px !important;
}
.d-container {
  background-color: #f5f5f5;
}

.d-lang-header {
  background-color: #ffff;
  padding: 12px 8px 0px 8px;
  margin: 8px 8px 0px 8px;
  height: 33px;
}
.d-lang-header h3,
i {
  font-weight: 600;
}
.d-module-title {
  margin: 0;
}
.d-lang-table {
  margin: 0px 8px 0px 8px;
  height: calc(100vh - 50px);
}

.d-toolbar {
  border: unset;
  outline: unset;
  background-color: #fff;
  margin: 0px 8px 0px 8px;
}

.d-btn-function {
  border-radius: 50%;
  margin-left: 6px;
}
.inputanh {
  border: 1px solid #ccc;
  width: 100%;
  height: 200px;
  cursor: pointer;
  padding: 1px;
  object-fit: contain;
  background-color: #eeeeee;
}
.ipnone {
  display: none;
}

.d-avatar-devicecard {
  position: relative;
  width: 100%;
  height: 350px;
}
.d-avatar-devicecard img {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  object-fit: contain;
}
.multi-width {
  max-width: 500px !important;
}
.toolbar-filter {
  border: unset;
  outline: unset;
  background-color: #fff;
  padding-bottom: 0px;
}
.sel-placeholder::placeholder {
  text-align: center;
  position: absolute;
  top: 0;
}
</style>
          
<style lang="scss" scoped>
::v-deep(.p-calendar) {
  .p-button.p-button-icon-only {
    width: 3.5rem !important;
  }
}
</style>
