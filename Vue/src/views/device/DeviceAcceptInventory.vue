<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { FilterMatchMode, FilterOperator } from "primevue/api";
 
import moment from "moment";

import treeuser from "../../components/user/treeuser.vue";
 
import configAprroved from "../../components/device/configAprroved.vue";
import deviceProcedure from "../../components/device/deviceProcedure.vue";
 
import detailsInventory from "../../components/device/detailsInventory.vue";
import { encr, checkURL } from "../../util/function.js";
//Khai báo
const router = inject("router");
const cryoptojs = inject("cryptojs");
//Khai báo device_card_get
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const basedomainURL = baseURL;
const selectedHandOver = ref();
const emitter = inject("emitter");
const selectedCard = ref([]);
const checkDelList = ref(false);
const displayAssets = ref(false);
 
watch(selectedHandOver, () => {
  if (selectedHandOver.value.length > 0) {
    checkDelList.value = true;
  } else {
    checkDelList.value = false;
  }
});

//Nơi nhận dữ liệu

emitter.on("emitData", (obj) => {
  switch (obj.type) {
    case "sendAccept":
      loadData();
      displayDeviceRepair.value = false;
      break;
    case "hideAccept":
      displayDeviceRepair.value = false;
      break;
  }
});
 
const checkMultile = ref(false);
const taskDateFilter = ref();
 
const selectedUser = ref([]);
const menu_ID = ref();
 
const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);
const checkShowAssets = ref(false);
 
const device_inventory_id=ref();
const openDetailsHandover = (data) => {
 device_inventory_id.value=data.inventory_slip_id;
    displayDetailsHandover.value = true;
};
const displayDialogUser = ref(false);

const headerDialogUser = ref("Chọn người tham gia");
 
const listUserA = ref([]);
const closeDialogUser = () => {
  displayDialogUser.value = false;
};
 
const choiceUser = () => {
  if (checkMultile.value == true) {
    //   datalistsD.value.forEach((m, i) => {
    //   let om = { key: m.key, data: m };
    //   if (m.key == selectedTreeU.organization_id) {
    //     m.data.userM = selectedUser.value[0].user_id;
    //     return;
    //   } else {
    //     let check = false;
    //     const rechildren = (mm, pid) => {
    //       if (mm.key == selectedTreeU.organization_id) {
    //         mm.data.data.userM = selectedUser.value[0].user_id;
    //         check = true;
    //         return;
    //       } else {
    //         if (mm.data.children) {
    //           let dts = mm.data.children;
    //           if (dts.length > 0) {
    //             dts.forEach((em) => {
    //               let om1 = { key: em.key, data: em };
    //               if (check) return;
    //               rechildren(om1, em.key);
    //             });
    //           };
    //         }
    //       }
    //     };
    //     if (check) return;
    //     rechildren(om, m.key);
    //   }
    // });
  } else {
    selectedUser.value.forEach((element, i) => {
      element.is_order = i + 1;
    });
    listUserA.value = selectedUser.value;
  }

  closeDialogUser();
};
//Lọc theo ngày
  
const filters = ref({
  global: { value: null, matchMode: FilterMatchMode.CONTAINS },
  inventory_number: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
  inventory_created_date: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.DATE_IS }],
  },
  proposer: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },

  status: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.EQUALS }],
  } 
});
//Phân trang dữ liệu
const onPage = (event) => {
  options.value.pagesize = event.rows;
  options.value.pageno = event.page;
  loadData();
};
const filterSQL = ref([]);
const isDynamicSQL = ref(false);
const loadDataSQL = () => {
  let data = {
    id: menu_ID.value,
    next: options.value.IsNext,
    sqlO: options.value.sort,
    Search: options.value.search,
    PageNo: options.value.pageno,
    PageSize: options.value.pagesize,
    fieldSQLS: filterSQL.value,
  };

  options.value.loading = true;
  axios
    .post(baseURL + "/api/SQL/Filter_accept_inventory_slip", data, config)
    .then((response) => {
      let dt = JSON.parse(response.data.data);

      let data = dt[0];

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
  inventory_number: "",
  image: "",

  barcode_id: "",
  barcode_type: 0,
  barcode_image: "",
  status: 0,
  not_used: false,
  used: true,
});
const filterSQLDM = ref([]);
const loadDataSQLDM = () => {
  datalistsDM.value = [];
  let arw = null;
  if (device_inventory.value.department_id_fake) {
    Object.keys(device_inventory.value.department_id_fake).forEach((key) => {
      arw = key;
      return;
    });
  }
  if (
    device_inventory.value.warehouse_id != null ||
    device_inventory.value.department_id_fake != null
  ) {
    if (device_inventory.value.warehouse_id != null) {
      filterSQLDM.value = [];
      let filterS = {
        filterconstraints: [
          {
            value: device_inventory.value.warehouse_id,
            matchMode: "equals",
          },
        ],
        filteroperator: "or",
        key: "warehouse_id",
      };
      filterSQLDM.value.push(filterS);
    }
    if (device_inventory.value.department_id_fake != null) {
      let filterS = {
        filterconstraints: [{ value: arw, matchMode: "equals" }],
        filteroperator: "and",
        key: "manage_department_id",
      };
      filterSQLDM.value.push(filterS);
    }
  } else {
    let filterS1 = {
      filterconstraints: [
        { value: store.getters.user.user_id, matchMode: "equals" },
      ],
      filteroperator: "and",
      key: "device_user_id",
    };
    filterSQLDM.value.push(filterS1);
  }
  if (options.value.device_type_id != null) {
    let filterS = {
      filterconstraints: [
        { value: options.value.device_type_id, matchMode: "equals" },
      ],
      filteroperator: "and",
      key: "device_type_id",
    };
    filterSQLDM.value.push(filterS);
  }

  let filterS = {
    filterconstraints: [{ value: "DSC", matchMode: "notEquals" }],
    filteroperator: "and",
    key: "status",
  };
  filterSQLDM.value.push(filterS);
  let data = {
    sqlS: "True",
    sqlO: options.value.sortDM,
    Search: options.value.SearchTextDM,
    PageNo: options.value.pagenoDM,
    PageSize: options.value.pagesizeDM,
    sqlF: null,
    fieldSQLS: filterSQLDM.value,
    next: true,
    id: options.value.id,
  };
  options.value.loading = true;
  axios
    .post(baseURL + "/api/SQL/Filter_device_card", data, config)
    .then((response) => {
      let dt = JSON.parse(response.data.data);
      let data = dt[0];
      if (data.length > 0) {
        data.forEach((element, i) => {
          element.STT =
            options.value.pagenoDM * options.value.pagesizeDM + i + 1;
        });
        datalistsDM.value = [data, []];
        if (selectedCard.value.length > 0) {
          let arr = data;
          selectedCard.value.forEach((element) => {
            arr = arr.filter((x) => x.card_id != element.card_id);
          });
          datalistsDM.value = [arr, selectedCard.value];
        }
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
const refreshData = () => {
  options.value.search = "";
  options.value.status = null;
  filterCardUserSend.value = null;
  filterTrangthai.value = null;
  options.value.start_date = null;
  options.value.end_date = null;
  taskDateFilter.value = [];
  checkFilter.value = false;
  first.value = 0;
  options.value.pageno = 0;
  filterSQL.value = [];
  selectedHandOver.value = [];
  filters.value = {
    global: { value: null, matchMode: FilterMatchMode.CONTAINS },
    repair_number: {
      operator: FilterOperator.AND,
      constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
    },
    repair_created_date: {
      operator: FilterOperator.AND,
      constraints: [{ value: null, matchMode: FilterMatchMode.DATE_IS }],
    },
    proposer: {
      operator: FilterOperator.AND,
      constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
    },

    status: {
      operator: FilterOperator.AND,
      constraints: [{ value: null, matchMode: FilterMatchMode.EQUALS }],
    },
  };
  loadDataSQL();
};
const onPageDM = (event) => {
  if (event.rows != options.value.pagesizeDM) {
    options.value.pagesizeDM = event.rows;
  }
  if (event.page == 0) {
    //Trang đầu
    options.value.id = null;
    options.value.next = true;
  } else if (event.page > options.value.pagenoDM + 1) {
    //Trang cuối
    options.value.id = -1;
    options.value.next = false;
  } else if (event.page > options.value.pagenoDM) {
    //Trang sau

    options.value.id =
      datalistsDM.value[0][datalistsDM.value[0].length - 1].card_id;
    options.value.next = true;
  } else if (event.page < options.value.pagenoDM) {
    //Trang trước
    options.value.id = datalistsDM.value[0][0].card_id;
    options.value.next = false;
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
    if (event.sortField != " inventory_slip_id") {
      options.value.sort +=
        ", inventory_slip_id " + (event.sortOrder == 1 ? " ASC" : " DESC");
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
 
const files = ref([]);

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
  sort: " inventory_slip_id DESC",
  sortDM: "card_id DESC",
  search: "",
  pageno: 0,
  pagesize: 20,
  pagenoDM: 0,
  pagesizeDM: 10,
  loading: true,
  totalRecords: null,
  totalRecordsDM: null,
  start_date: null,
  end_date: null,
  next: true,
});
const device_inventory = ref({
  is_order: 1,
  proposal_code: "",
  device_id: null,
  inventory_number: "",
  image: "",
  barcode_id: "",
  barcode_type: 0,
  barcode_image: "",
  status: 0,
  department_id_fake: {},
});
 
const danhMuc = ref();
//METHOD

 
 
 
const hideSelectDevice = () => {
  selectedDeviceMain.value = {
    is_order: 1,
    proposal_code: "",
    device_id: null,
    inventory_number: "",
    image: "",

    barcode_id: "",
    barcode_type: 0,
    barcode_image: "",
    status: 0,
    not_used: false,
    used: true,
  };
  selectedCard.value = [];
  options.value.device_card_type = null;
  options.value.SearchTextDM = null;
  displayAssets.value = false;
};

const onSelectDevice = () => {
  selectedCard.value = [];
  if (datalistsDM.value[1].length > 0) {
    datalistsDM.value[1].forEach((element, i) => {
      element.serial = element.barcode_id;
      if (element.inventory_slip_id == null)
        element.condition = element.assets_condition;
      element.note = element.device_des;
      element.is_order = i + 1;
      element.amount_before = 1;

      element.amount_after = 1;
      selectedCard.value.push(element);
    });
  } else {
    swal.fire({
      title: "Thông báo!",
      text: "Vui lòng chọn thiết bị!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }

  displayAssets.value = false;
};
//Tìm kiếm
const searchDeviceMain = () => {
  filterSQLDM.value = [];
  loadDataSQLDM();
};
const filterDeviceMain = () => {
  loadDataSQLDM();
};
const listDepartment = ref();
const initTudien = () => {
  axios
    .post(
      baseURL + "/api/device_card/getData",
        {
          str: encr(
            JSON.stringify({
        proc: "sys_device_department_child",
        par: [{ par: "user_id", va: store.getters.user.user_id }],
      }),
            SecretKey,
            cryoptojs
          ).toString(),
        },config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];

      if (data.length > 0) {
        let obj = renderTreeDV1(
          data,
          "organization_id",
          "organization_name",
          "đơn vị",
          store.getters.user.organization_id
        );
        listDepartment.value = obj.arrtreeChils;
      }  
    })
    .catch((error) => {
    
    });
};
 const filterButs=ref();
 const checkFilter=ref(false);
const toggleFilter = (event) => {
  filterButs.value.toggle(event);
};

const hideFilter = () => {
  if (
    !(
      options.value.is_hot != null ||
  filterTrangthai.value != null ||
   filterCardUserSend.value != null
    )
  )
    checkFilter.value = false;
};
const listSCard = ref([
 
  { name: "Chờ đánh giá", code: 1 },
  { name: "Đã đánh giá", code: 2 },
  { name: "Chờ duyệt", code: 3 },
  { name: "Hoàn thành", code: 4 },
  { name: "Trả lại", code: 5 },
]);

const filterTrangthai = ref();
const filterCardUserSend = ref();

const showFilter = ref(false);
const reFilterCard = () => {
  checkFilter.value = false;
 
  filterCardUserSend.value = null;
  filterTrangthai.value = null;
  taskDateFilter.value = [];
  options.value.is_hot = null;
  options.value.news_type = null;
  options.value.status = null;
  filterCard(false);
  showFilter.value = false;
   filterButs.value.hide();
};
const filterCard = (check) => {
  if (check) checkFilter.value = true;

  showFilter.value = false;

  filterSQL.value = [];

  if (filterCardUserSend.value != null) {
    let filterS = {
      filterconstraints: [],
      filteroperator: "or",
      key: "inventory_user_id",
    };
    filterCardUserSend.value.forEach((element) => {
      filterS.filterconstraints.push({ value: element, matchMode: "equals" });
    });
    filterSQL.value.push(filterS);
  }
  if (filterTrangthai.value != null) {
      let filterS1 = {
      filterconstraints: [],
      filteroperator: "or",
      key: "status",
    };
    filterTrangthai.value.forEach((element) => {
      filterS1.filterconstraints.push({ value: element, matchMode: "equals" });
    });
    
    filterSQL.value.push(filterS1);
  }
  isDynamicSQL.value = true;
  loadData(true);
  filterButs.value.hide();
};
//Tìm kiếm
const searchCard = () => {
  loadDataSQL();
};
const displayProcedure = ref(false);
const closeDialogProcedure = () => {
  displayProcedure.value = false;
};
const processListViews = ref([]);
const onShowProcedure = () => {
  processListViews.value = [];
  axios
    .post(
      baseURL + "/api/device_card/getData",
        {
          str: encr(
            JSON.stringify({
        proc: "device_process_list_views",
        par: [
          {
            par: "device_node_id",
            va: deviceDataDetails.value.inventory_slip_id,
          },
          { par: "type", va: 2 },
        ],
      }),
            SecretKey,
            cryoptojs
          ).toString(),
        },config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      let data1 = JSON.parse(response.data.data)[1];

      data1.forEach((element) => {
        if (!element.listprocess) element.listprocess = [];
        if (element.device_process != null) {
          element.device_process = JSON.parse(element.device_process);
          element.device_process.forEach((eles) => {
            if (eles.is_approved === "1") eles.is_approved = "2";
            if (eles.is_returned === "1") eles.is_approved = "1";
            if (eles.is_returned === "1" && eles.is_type == "1")
              eles.is_approved = "3";

            if (eles.files != "") {
              eles.files = JSON.parse(eles.files);
            }

            element.listprocess.push(eles);
          });
        }
        // if (element.aprroved_user != null) {
        //   element.aprroved_user = JSON.parse(element.aprroved_user);
        //   element.aprroved_user.forEach((eles) => {
        //     element.listprocess.push(eles);
        //   });
        // }
        // element.listprocess = compareProcess(element.listprocess);

        element.aprroved_user = null;
        element.device_process = null;
      });
      processListViews.value.push(data);
      processListViews.value.push(data1);
loadLogs(deviceDataDetails.value.inventory_slip_id,2);
      displayProcedure.value = true;
      // loadDTliView(data[0].approved_group_id)
    })
    .catch((error) => {
     
    });
}; 

const listLogs = ref([]);
const loadLogs=(device_note_id, type)=>{
   listLogs.value=[];
   axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "device_process_log_list",
            par: [
              {
                par: "device_note_id",
                va:device_note_id,
              },
              { par: "device_process_type", va: type },
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
     
  listLogs.value=data;
       
    })
    .catch((error) => {});
}
const liItemsDetails = ref([
  {
    label: "Xem phiếu",
    icon: "pi pi-book",
    command: () => {
      openDetailsHandover(deviceDataDetails.value);
    },
  },
  {
    label: "Xem quy trình",
    icon: "pi pi-sitemap",
    command: () => {
      onShowProcedure();
    },
  },
]); 
const displayDeviceRepair = ref(false);
 
const closeDetailsHandover = () => {
  displayDetailsHandover.value = false;
  device_inventory.value = null;
};
const displayDetailsHandover = ref(false);
 
 

const loadCount = () => {
  axios
    .post(
      baseURL + "/api/device_card/getData",
        {
          str: encr(
            JSON.stringify({
        proc: "device_accept_inventory_count",
        par: [{ par: "user_id", va: store.getters.user.user_id }],
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
        sttCard.value = data[0].totalRecords + 1;
      } else options.value.totalRecords = 0;
    })
    .catch((error) => {});
};
const saveHandover = () => {
  if (selectedCard.value) {
    var arrayAdda = [...selectedCard.value];
    arrayAdda = arrayAdda.filter(
      (x) => x.user_id == store.getters.user.user_id
    );
    if (arrayAdda.length > 0) {
      arrayAdda.forEach((element) => {
        element.representative = null;
      });
    }
    let formData = new FormData();
    formData.append("inventorypersonnel", JSON.stringify(arrayAdda));
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });

    axios
      .put(
        baseURL + "/api/device_inventory_slip/update_inventory_details",
        formData,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Đánh giá đánh giá thành công!");
          loadData(true);
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
const listFilesS = ref([]);
 
const deviceAprrovedId = ref();
const checkDefault = ref(false);
const deviceRepairS = ref();
const deviceDataDetails = ref();

const todayClick = () => {
  taskDateFilter.value = [];
  taskDateFilter.value.push(new Date());
};
const delDayClick = () => {
  taskDateFilter.value = [];
  options.value.start_date = null;
  options.value.end_date = null;
  filterSQL.value = [];
  isDynamicSQL.value = true;
  loadData(true);
};
const onDayClick = () => {
  if (taskDateFilter.value == null) taskDateFilter.value = [];
  else {
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
        key: "inventory_created_date",
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
        key: "inventory_created_date",
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
        key: "inventory_created_date",
      };
      filterSQL.value.push(filterS1);
    }
  }
  isDynamicSQL.value = true;
  loadData(true);
};
//Sửa bản ghi
const menuDetailsR = ref();
const onShowDetails = (value) => {
  deviceDataDetails.value = value;
  menuDetailsR.value.toggle(event);
}; 
const editCard = (data) => {
  submitted.value = false;
  files.value = [];
  listUserA.value = [];
  axios
    .post(
      baseURL + "/api/device_card/getData",
        {
          str: encr(
            JSON.stringify({
        proc: "device_accept_inventory_get",
        par: [{ par: "inventory_slip_id", va: data.inventory_slip_id }],
      }),
            SecretKey,
            cryoptojs
          ).toString(),
        },config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      let data1 = JSON.parse(response.data.data)[1];
      let data2 = JSON.parse(response.data.data)[2];
      let data3 = JSON.parse(response.data.data)[3];
      device_inventory.value = data[0];

      device_inventory.value.inventory_date = new Date(
        device_inventory.value.inventory_date
      );
      if (device_inventory.value.inventory_created_date) {
        device_inventory.value.inventory_created_date = new Date(
          device_inventory.value.inventory_created_date
        );
      }

      data1.forEach((element, i) => {
        element.is_order = i + 1;
        if (element.representative) {
          element.representative = JSON.parse(element.representative)[0];
        }
      });
    
      selectedCard.value = data1;
      listFilesS.value = data2;
      data3.forEach((element, i) => {
        element.is_order = i + 1;
      });
      listUserA.value = data3;
      checkShowAssets.value = false;
      headerDialog.value = "Đánh giá thiết bị kiểm kê";
      isSaveCard.value = true;
      displayBasic.value = true;
    })
    .catch((error) => {
     
    });
};
const checkImg = (src) => {
  let allowedExtensions = /(\.jpg|\.jpeg|\.png|\.gif)$/i;
  return allowedExtensions.exec(src);
};
//Hiển thị dialog

const headerDialog = ref();
const displayBasic = ref(false);
  
const closeDialogDC = () => {
  displayBasic.value = false;
};
const closeDialog = () => {
  // isFirstCard.value = false;
  // loadData(true);
  displayBasic.value = false;
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
  options.value.loading = false;
  axios
    .post(
      baseURL + "/api/device_card/getData",
        {
          str: encr(
            JSON.stringify({
        proc: "device_accept_inventory_list",
        par: [
          { par: "pageno", va: options.value.pageno },
          { par: "pagesize", va: options.value.pagesize },
          { par: "user_id", va: store.state.user.user_id },
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

 
 
//Khai báo function 
 
 
 
const first = ref(0); 
//Xuất excel 
const renderTreeDV1 = (data, id, name, title, org_id) => {
  let arrtreeChils = [];
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
  return { arrtreeChils: arrtreeChils };
};
const listType = ref();   
const listDropdownUser = ref();
 
const loadUser = () => {
 
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
          { par: "organization_id", va: store.getters.user.organization_id },
          { par: "department_id", va: null },
          { par: "position_id", va: null },
          { par: "pageno", va: 1 },
          { par: "pagesize", va: 100000 },
          { par: "isadmin", va: null },
          { par: "status", va: null },
          { par: "start_date", va: null },
          { par: "end_date", va: null },
        ],
      }),
            SecretKey,
            cryoptojs
          ).toString(),
        },config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      data.forEach((element, i) => {
        listDropdownUser.value.push({
          name: element.full_name,
          code: element.user_id,
          avatar: element.avatar,position_name:element.position_name,
          department_name: element.department_name,
          role_name: element.role_name,
        });
      });
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
onMounted(() => {  if (!checkURL(window.location.pathname, store.getters.listModule)) {
     //router.back();
  }
  // loadDeviceUnit();
  // loadWareHouse();
  loadUser();
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
  <div class="d-inventory_slip_idntainer">
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
        dataKey="inventory_slip_id"
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
        v-model:selection="selectedHandOver"
        :pageLinkSize="options.pagesize"
        paginatorTemplate="FirstPageLink PrevPageLink PageLinks  NextPageLink LastPageLink    RowsPerPageDropdown"
        :rowsPerPageOptions="[20, 30, 50, 100, 200]"
      >
        <template #header>
          <div>
            <h3 class="module-title my-2 ml-1">
              <i class="pi pi-file-pdf"></i> Đánh giá phiếu kiểm kê ({{
                options.totalRecords ? options.totalRecords : 0
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
                    (filterTrangthai != null || filterCardUserSend != null) &&
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
                      <MultiSelect
                        v-model="filterTrangthai"
                        :options="listSCard"
                        optionLabel="name"
                        optionValue="code"
                        placeholder="Chọn trạng thái"
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
                        Người lập:
                      </div>
                      <MultiSelect
                        v-model="filterCardUserSend"
                        panelClass="d-design-dropdown"
                        :options="listDropdownUser"
                        :filter="true"
                        optionLabel="name"
                        optionValue="code"
                        style="width: calc(100% - 10rem)"
                        class="w-full"
                        placeholder="Chọn người lập"
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
                                <div class="col-1"></div>
                                <div class="col-10 p-0 pl-2 align-items-center">
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
                      </MultiSelect>
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
                placeholder="Lọc theo ngày lập"
                id="range"
                v-model="taskDateFilter"
                :showIcon="true"
                selectionMode="range"
                class="mx-2"
                :manualInput="false"
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

              <!-- <TreeSelect
                  style="margin-left: 24px; min-width: 200px"
                  @change="selectTree()"
                  v-model="menu_IDNode"
                  :options="danhMuc"
                  placeholder="Tất cả tin tức"
                ></TreeSelect> -->
            </template>

            <template #end>
            

              <Button
                class="mr-2 p-button-outlined p-button-secondary"
                icon="pi pi-refresh"
                @click="refreshData"
              />
              <!-- 
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
          bodyStyle="text-align:center;max-width:70px"
          field="is_order"
          header="STT"
        >
        </Column>
        <Column
          headerStyle="text-align:center;max-width:180px;height:50px"
          bodyStyle="text-align:center;max-width:180px"
          field="inventory_number"
          class="align-items-center justify-content-center text-center"
          header="Số phiếu"
          :sortable="true"
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
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;height:50px;max-width:200px"
          bodyStyle="text-align:center;max-width:200px"
          field="full_name"
          header="Người lập"
      
        >
        
          <template #body="data">
            <div>
              <div class="flex w-full align-items-center pr-2">
                <Avatar
                  v-bind:label="
                    data.data.avatar
                      ? ''
                      : data.data.full_name
                      ? data.data.full_name.substring(
                          data.data.full_name.lastIndexOf(' ') + 1,
                          data.data.full_name.lastIndexOf(' ') + 2
                        )
                      : 'a'
                  "
                  :image="basedomainURL + data.data.avatar"
                  size="small"
                  :style="
                    data.data.avatar
                      ? 'background-color: #2196f3'
                      : 'background:' + data.data.full_name.length
                      ? bgColor[data.data.full_name.length % 7]
                      : bgColor[1]
                  "
                  shape="circle"
                  @error="
                    $event.target.src =
                      basedomainURL + '/Portals/Image/nouser1.png'
                  "
                />
                <div class="px-2">{{ data.data.full_name }}</div>
              </div>
            </div>
          </template>
        </Column>
      
        <Column
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;max-width:120px;height:50px"
          bodyStyle="text-align:center;max-width:120px"
          field="inventory_created_date"
          header="Ngày lập"
        >
          <template #body="data">
            <div>
              {{
                moment(new Date(data.data.inventory_created_date)).format(
                  "DD/MM/YYYY"
                )
              }}
            </div>
          </template>
        </Column>

        <Column
          headerStyle="text-align:center;height:50px"
          bodyStyle="text-align:center"
          field="deviceFrom"
          header="Kho/Phòng ban kiểm kê"
        >
        </Column>
       <!-- <Column
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;height:50px;max-width:150px"
          bodyStyle="text-align:center;max-width:150px"
          field="inventory_type"
          header="Kiểu phiếu"
        >
          <template #body="data">
            <div>
              {{
                data.data.inventory_type == 1
                  ? "Sửa chữa"
                  : "Bảo trì - Bảo dưỡng"
              }}
            </div>
          </template>
        </Column> -->

        <Column
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;max-width:150px;height:50px"
          bodyStyle="text-align:center;max-width:150px"
          field="status"
          header="Trạng thái"
        >
          <template #body="data">
            <div class="w-full">
              <Chip
                v-if="data.data.status == 1"
                label="Chờ đánh giá"
                class="
                  w-full
                  bg-yellow-300
                  justify-content-center
                  p-button-status-d
                "
              />
           
              <Chip
                v-else
                label="Đã đánh giá"
                class="
                  w-full
            bg-blue-300
                  justify-content-center
                  p-button-status-d
                "
              />
            </div>
          </template>
        </Column>

        <Column
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;height:50px;max-width:180px"
          bodyStyle="text-align:center;max-width:180px"
          header="Chức năng"
        >
          <template #body="data">
            <div>
                <Button
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              @click="onShowDetails(data.data)"
              type="button"
              icon="pi pi-info-circle"
              aria-haspopup="true"
              aria-controls="overlay_menu_details"
              v-tooltip.left="'Chi tiết'"
              v-if="data.data.status!=1"
            ></Button>

              <Button
              v-else
                @click="editCard(data.data)"
                class="
                  p-button-rounded p-button-secondary p-button-outlined
                  mx-1
                "
                type="button"
                icon="pi pi-arrow-circle-up"
              ></Button>
              
            <Menu
              id="overlay_menu_details"
              ref="menuDetailsR"
              :model="liItemsDetails"
              :popup="true"
            />
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
  </div>  
  <Dialog
    @hide="closeDialog"
    :header="headerDialog"
    v-model:visible="displayBasic"
    :maximizable="true"
    :style="{ width: '70vw' }"
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="col-12 field p-0 text-lg font-bold">Thông tin phiếu</div>
        <div class="col-12 field flex p-0">
          <div class="col-6 flex p-0 align-items-center">
            <div class="w-10rem">Số phiếu:</div>
            <div style="width: calc(100% - 10rem)">
              <InputText
                :disabled="true"
                v-model="device_inventory.inventory_number"
                class="w-full class-disabled"
              />
            </div>
          </div>
          <div class="col-6 flex p-0 text-center align-items-center">
            <div class="col-4 p-0 pl-5 text-left">Người lập:</div>
            <div class="col-8 p-0 flex text-left font-bold">
              {{ device_inventory.created_name }}
            </div>
          </div>
        </div>

        <div class="col-12 flex p-0">
          <div class="col-6 p-0 text-left align-items-center">
            <div class="col-12 field p-0 flex text-left align-items-center">
              <div class="w-10rem">Ngày lập:</div>
              <div style="width: calc(100% - 10rem)">
                <Calendar
                  :disabled="true"
                  placeholder="dd/mm/yyyy"
                  class="w-full class-disabled"
                  id="basic_use_date"
                  v-model="device_inventory.inventory_date"
                  autocomplete="on"
                  :showIcon="true"
                />
              </div>
            </div>
          </div>
          <div class="col-6 p-0 text-left align-items-center">
            <div class="col-12 field p-0 flex text-left align-items-center">
              <div class="col-4 p-0 pl-5 text-left">Ngày kiểm kê:</div>
              <Calendar
                placeholder="dd/mm/yyyy"
                class="w-full class-disabled"
                id="basic_use_date"
                v-model="device_inventory.inventory_created_date"
                autocomplete="on"
                :disabled="true"
                :showIcon="true"
              />
            </div>
          </div>
        </div>
        <div class="col-12 flex p-0">
          <div class="col-6 p-0 text-left align-items-center">
            <div class="col-12 field p-0 flex text-left align-items-center"               v-if="device_inventory.department_id">
              <div class="w-10rem">Phòng ban:</div>
              <div style="width: calc(100% - 10rem)">
                <InputText
                  v-if="device_inventory.department_id"
                  :disabled="true"
                  v-model="device_inventory.device_fromname"
                  class="w-full class-disabled"
                />
                <InputText
                  v-else
                  :disabled="true"
                  v-model="device_inventory.department_name"
                  class="w-full class-disabled"
                  placeholder="Tài sản thuộc kho"
                />
              </div>
            </div>
          </div>
          <div class="col-6 p-0 text-left align-items-center">
            <div class="col-12 field p-0 flex text-left align-items-center"               v-if="device_inventory.warehouse_id">
              <div class="col-4 p-0 pl-5 text-left">Kho:</div>
 
              <InputText
                v-if="device_inventory.warehouse_id"
                :disabled="true"
                v-model="device_inventory.device_fromname"
                class="w-full class-disabled"
              />
              <InputText
                v-else
                :disabled="true"
                v-model="device_inventory.department_name"
                class="w-full class-disabled"
                placeholder="Tài sản thuộc phòng ban"
              />
            </div>
          </div>
        </div>
        <div class="col-12 field p-0 text-lg font-bold">
          <Toolbar class="p-0 surface-0 border-none">
            <template #start>
              <div class="text-lg font-bold">Danh sách người tham gia</div>
            </template>
            <template #end> </template>
          </Toolbar>
        </div>

        <div class="col-12 p-0 field">
          <div class="w-full p-0">
            <DataTable
              class="w-full p-datatable-sm e-sm"
              filterDisplay="menu"
              filterMode="lenient"
              dataKey="card_id"
              responsiveLayout="scroll"
              :scrollable="true"
              scrollHeight="flex"
              :showGridlines="true"
              :lazy="true"
              :value="listUserA"
              :paginator="false"
              :totalRecords="listUserA.length"
              :row-hover="true"
            >
              <Column
                class="align-items-center justify-content-center text-center"
                headerStyle="text-align:center;max-width:70px;height:50px"
                bodyStyle="text-align:center;max-width:70px;overflow: hidden;"
                field="is_order"
                header="STT"
                headerClass="format-center"
              >
              </Column>
              <Column
                headerStyle="text-align:left;height:50px"
                bodyStyle="text-align:left;overflow: hidden;"
                headerClass="format-center"
                field="full_name"
                header="Họ và tên"
              >
                <template #body="data">
                  <div>
                    <div class="flex w-full align-items-center pr-2">
                      <Avatar
                        v-bind:label="
                          data.data.avatar
                            ? ''
                            : data.data.full_name.substring(
                                data.data.full_name.lastIndexOf(' ') + 1,
                                data.data.full_name.lastIndexOf(' ') + 2
                              )
                        "
                        :image="basedomainURL + data.data.avatar"
                        size="small"
                        :style="
                          data.data.avatar
                            ? 'background-color: #2196f3'
                            : 'background:' +
                              bgColor[data.data.full_name.length % 7]
                        "
                        shape="circle"
                        @error="
                          $event.target.src =
                            basedomainURL + '/Portals/Image/nouser1.png'
                        "
                      />
                      <div class="px-2">{{ data.data.full_name }}</div>
                    </div>
                  </div>
                </template>
              </Column>

              <Column
                headerStyle="text-align:left;max-width:300px;height:50px"
                bodyStyle="text-align:left;max-width:300px; overflow: hidden;"
                field="organization_name"
                header="Phòng ban"
                headerClass="format-center"
              >
              </Column>

              <Column
                class="align-items-center justify-content-center text-center"
                headerStyle="text-align:center;max-width:220px;height:50px"
                bodyStyle="text-align:center;max-width:220px;overflow: hidden;"
                field="position_name"
                header="Chức vụ"
              >
              </Column>

              <template #empty>
                <div
                  class="align-items-center justify-c p-0 text-center m-auto"
                  v-if="!isFirst"
                >
                  <img src="../../assets/background/nodata.png" height="144" />
                  <h3 class="m-1">Không có dữ liệu</h3>
                </div>
              </template>
            </DataTable>
          </div>
        </div>
        <div class="col-12 pb-2 p-0 text-lg font-bold">
          <Toolbar class="p-0 surface-0 border-none">
            <template #start>
              <div class="text-lg font-bold">Danh sách thiết bị</div>
            </template>
            <template #end> </template>
          </Toolbar>
        </div>

        <div class="col-12 p-0 field">
          <div class="w-full" v-if="selectedCard.length > 0">
            <DataTable
              class="w-full p-datatable-sm e-sm p-tbl-cs"
              filterDisplay="menu"
              filterMode="lenient"
              dataKey="card_id"
              responsiveLayout="scroll"
              scrollHeight="flex"
              :showGridlines="true"
              :lazy="true"
              :value="selectedCard"
              :paginator="false"
              :totalRecords="selectedCard.length"
              sortMode="single"
              rowGroupMode="rowspan"
              groupRowsBy="representative.device_number"
            >
              <Column
                class="align-items-center justify-content-center text-center"
                headerStyle="text-align:center;width:50px;height:50px"
                bodyStyle="text-align:center;width:50px;overflow: hidden;"
                header="STT"
              
              >
                <template #body="data">
                  <div>
                    {{ data.index + 1 }}
                  </div>
                </template>
              </Column>

              <Column
                headerStyle="text-align:left;width:120px;height:50px"
                bodyStyle="text-align:left;width:120px;overflow: hidden; "
                field="representative.device_number"
                header="Số hiệu"
              >
                <template #body="slotProps">
                  <div class="image-text format-center">
                    {{
                      slotProps.data.representative
                        ? slotProps.data.representative.device_number
                        : ""
                    }}
                  </div>
                </template>
              </Column>
              <Column
                headerStyle="text-align:left;height:50px;max-width:200px"
                bodyStyle="text-align:left;overflow: hidden;max-width:200px"
                field="representative.device_number"
             
                header="Tên thiết bị"
              >
                <template #body="slotProps">
                  <span class="image-text">{{
                    slotProps.data.representative
                      ? slotProps.data.representative.device_name
                      : ""
                  }}</span>
                </template>
              </Column>
              <Column
                headerStyle="text-align:left;width:120px;height:50px"
                bodyStyle="text-align:left;width:120px;overflow: hidden;"
                field="representative.device_number"
                header="Sử dụng"
              >
                <template #body="slotProps">
                  <span class="image-text">{{
                    slotProps.data.representative
                      ? slotProps.data.representative.full_name
                      : ""
                  }}</span>
                </template>
              </Column>
              <Column
                headerStyle="text-align:left;width:50px;height:50px"
                bodyStyle="text-align:left;width:50px;overflow: hidden;"
                field="representative.device_number"
                header="SL trước"
              >
                <template #body="slotProps">
                  <div class="image-text format-center">
               
                    <div style="border-radius:50%" class="bg-blue-500 w-2rem h-2rem format-center text-0">
                      {{ slotProps.data.representative
                          ? slotProps.data.representative.amount_before
                          : ''}}
                    </div>
                  </div>
                </template>
              </Column>
              <Column
                headerStyle="text-align:left;width:50px;height:50px"
                bodyStyle="text-align:left;width:50px;overflow: hidden;"
                field="representative.device_number"
                header="SL sau"
              >
                <template #body="slotProps">
                  <div class="image-text format-center">
                       <div style="border-radius:50%" class="bg-blue-500 w-2rem h-2rem format-center text-0">
                      {{ slotProps.data.representative
                          ? slotProps.data.representative.amount_after
                          : ''}}
                    </div>
               
                  </div>
                </template>
              </Column>
              <Column
                headerStyle="text-align:left;width:150px;height:50px"
                bodyStyle="text-align:left;width:150px;overflow: hidden;"
                field="representative.device_number"
                header="Tình trạng"
              >
                <template #body="slotProps">
                  <span class="image-text">{{
                    slotProps.data.representative
                      ? slotProps.data.representative.condition
                      : ""
                  }}</span>
                </template>
              </Column>
              <Column
                headerStyle="text-align:left;height:50px; min-width:150px"
                bodyStyle="text-align:left; overflow: hidden; min-width:150px"
                field="full_name"
                header="Người đánh giá"
      
              >
                <template #body="data">
                  <div class="flex w-full align-items-center pr-2">
                    <Avatar
                      v-bind:label="
                        data.data.avatar
                          ? ''
                          : data.data.full_name.substring(
                              data.data.full_name.lastIndexOf(' ') + 1,
                              data.data.full_name.lastIndexOf(' ') + 2
                            )
                      "
                      :image="basedomainURL + data.data.avatar"
                      size="small"
                      :style="
                        data.data.avatar
                          ? 'background-color: #2196f3'
                          : 'background:' +
                            bgColor[data.data.full_name.length % 7]
                      "
                      shape="circle"
                      @error="
                        $event.target.src =
                          basedomainURL + '/Portals/Image/nouser1.png'
                      "
                    />
                    <div class="px-2">{{ data.data.full_name }}</div>
                  </div>
                </template>
              </Column>
              <Column
                headerStyle="text-align:left;width:60px;height:50px"
                bodyStyle="text-align:left;width:60px;overflow: hidden;"
                field="representative.amount"
                header="Số lượng"
              >
                <template #body="slotProps">
                  <div
                    class="w-full bg-blue-300"
                    v-if="
                      store.getters.user.user_id == slotProps.data.user_id &&
                      slotProps.data.representative
                    "
                  >
                    <InputNumber
                      :max="slotProps.data.representative.amount_before"
                      :min="0"
                      class="w-full p-0 bg-blue-100"
                      v-model="slotProps.data.amount"
                    />
                  </div>
                  <div v-else>
                    {{ slotProps.data.amount }}
                  </div>
                </template>
              </Column>
              <Column
                headerStyle="text-align:left;height:50px; min-width:100px"
                bodyStyle="text-align:left;overflow: hidden; min-width:100px;"
                field="representative.reviews"
                header="Đánh giá"
              >
                <template #body="slotProps">
                  <div
                    class="w-full h-full p-0"
                    v-if="store.getters.user.user_id == slotProps.data.user_id"
                  >
                    <Textarea
                      autoResize
                      v-model="slotProps.data.reviews"
                      class="w-full h-full bg-blue-100"
                    ></Textarea>
                  </div>
                  <div class="w-full" v-else>
                    {{ slotProps.data.reviews }}
                  </div>
                </template>
              </Column>

              <template #empty>
                <div
                  class="align-items-center justify-c p-0 text-center m-auto"
                  v-if="!isFirst"
                >
                  <img src="../../assets/background/nodata.png" height="144" />
                  <h3 class="m-1">Không có dữ liệu</h3>
                </div>
              </template>
            </DataTable>
          </div>
        </div>

        <div
          class="col-12 flex p-0 field mt-2 pt-2"
          v-if="listFilesS.length > 0"
        >
          <div class="w-10rem p-0 font-bold">File đính kèm</div>
        </div>
        <div class="col-12 p-0 flex">
          <div class="w-10rem p-0"></div>
          <div
            style="width: calc(100% - 10rem)"
            class="p-0 flex field"
            v-for="(item, index) in listFilesS"
            :key="index"
          >
            <div
              class="
                p-0
                border-3 border-solid border-round-3xl border-blue-200
                surface-50
              "
              style="width: 100%; border-radius: 10px"
            >
              <Toolbar class="w-full py-3">
                <template #start>
                  <div class="flex">
                    <Image
                      v-if="checkImg(item.file_path)"
                      :src="basedomainURL + item.file_path"
                      :alt="item.file_name"
                      width="70"
                      height="50"
                      style="
                        object-fit: contain;
                        border: 1px solid #ccc;
                        width: 70px;
                        height: 50px;
                      "
                      preview
                      @error="
                        $event.target.src =
                          basedomainURL + '/Portals/Image/noimg.jpg'
                      "
                      class="pr-2"
                    />
                    <div v-else>
                      <a
                        :href="basedomainURL + item.file_path"
                        download
                        class="w-full no-underline"
                      >
                        <img
                          :src="
                            basedomainURL +
                            '/Portals/Image/file/' +
                            item.file_path.substring(
                              item.file_path.lastIndexOf('.') + 1
                            ) +
                            '.png'
                          "
                          style="width: 70px; height: 50px; object-fit: contain"
                          :alt="item.file_name"
                        />
                      </a>
                    </div>
                    <span class="ml-2" style="line-height: 50px">
                      {{ item.file_name }}</span
                    >
                  </div>
                </template>
                <template #end>
                 
                </template>
              </Toolbar>
            </div>
          </div>
        </div>
      </div>
    </form>
    <template #footer>
      <div class="pt-3">
        <Button
          label="Hủy"
          icon="pi pi-times"
          @click="closeDialogDC()"
    class="p-button-outlined"
        />

        <Button
          @click="saveHandover()"
          label="Đánh giá"
          icon="pi pi-check"
          autofocus
        />
      </div>
    </template>
  </Dialog>

  <Dialog
    header="Chọn thiết bị từ danh sách"
    v-model:visible="displayAssets"
    :maximizable="true"
    :style="{ width: '55vw' }"
  >
    <div>
      <div class="true flex-grow-1 p-2" id="scrollTop">
        <div class="grid p-0">
          <div class="col-12 field format-center">
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
                v-model="options.device_type_id"
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
          </div>
        </div>

        <PickList
          class="picklist-custom"
          v-model="datalistsDM"
          listStyle="height:1000px !important"
          dataKey="card_id"
        >
          <template #sourceheader
            ><div class="format-center">Danh sách thiết bị</div>
          </template>
          <template #targetheader>
            <div class="format-center">Danh sách đã chọn</div>
          </template>
          <template #item="slotProps">
            <div class="product-item">
              <div class="image-container">
                <img
                  :src="
                    slotProps.item.image
                      ? basedomainURL + slotProps.item.image
                      : basedomainURL + '/Portals/Image/noimg.jpg'
                  "
                  style="
                    object-fit: cover;
                    width: 75px;
                    height: 50px;
                    box-shadow: 0 3px 6px rgba(0, 0, 0, 0.16),
                      0 3px 6px rgba(0, 0, 0, 0.23);
                    margin-right: 1rem;
                  "
                />
              </div>
              <div class="product-list-detail">
                <h5 class="my-2 text-justify">
                  {{ slotProps.item.device_name }}
                </h5>

                <div class="flex">
                  <div class="w-full" v-tooltip.top="'Số hiệu'">
                    <i class="pi pi-tag product-category-icon"></i>
                    <span class="product-category">{{
                      slotProps.item.device_number
                    }}</span>
                  </div>

                  <div class="w-full" v-tooltip.top="'Mã barcode'">
                    <i class="pi pi-qrcode product-category-icon"></i>
                    <span class="product-category">{{
                      slotProps.item.barcode_id
                    }}</span>
                  </div>
                </div>
                <div class="flex">
                  <div class="w-full" v-tooltip.top="'Nhà kho'">
                    <i class="pi pi-home product-category-icon"></i>
                    {{ slotProps.item.warehouse_name }}
                  </div>
                  <div class="w-full" v-tooltip.top="'Ngày mua'">
                    <i class="pi pi-shopping-cart product-category-icon"></i>
                    <span class="product-category">
                      {{
                        moment(new Date(slotProps.item.purchase_date)).format(
                          "DD/MM/YYYY"
                        )
                      }}
                    </span>
                  </div>
                </div>
              </div>
            </div>
          </template>
        </PickList>

        <Paginator
          v-if="options.totalRecordsDM > 10"
          @page="onPageDM($event)"
          class="m-0 p-0 pt-5"
          :rows="10"
          :totalRecords="options.totalRecordsDM"
        ></Paginator>
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
    header="Phiếu kiểm kê"
    v-model:visible="displayDetailsHandover"
    :maximizable="true"
    :style="{ width: '70vw' }"
  >
      <form v-if="displayDetailsHandover"> 
    <detailsInventory  :device_inventory_id="device_inventory_id"/>
    </form>
    <template #footer>
      <Button
        @click="closeDetailsHandover"
        label="Đóng"
        icon="pi pi-times"
        autofocus
      />
    </template>
  </Dialog>

 
  <Dialog
    header="Trình duyệt"
    v-model:visible="displayDeviceRepair"
    :maximizable="true"
    :style="{ width: '35vw' }"
  >
    <div v-if="displayDeviceRepair">
      <configAprroved
        :display="displayDeviceRepair"
        :type="'TS_PhieuSuaChua'"
        :isdefault="checkDefault"
        :approved_group_id="deviceAprrovedId"
        :device_process_code="deviceRepairS.inventory_number"
        :device_note_id="deviceRepairS.inventory_slip_id"
        :checkApp="1"
        :device_process_id="null"
        :device_process_type="2"      :listAssetsH="null"
      ></configAprroved>
    </div>
  </Dialog>
  <Dialog
    header="Quy trình duyệt"
    v-model:visible="displayProcedure"
    :dismissableMask="true"
    :modal="true"
    :maximizable="true"
    :style="{ width: '35vw' }"
  >
    <div v-if="displayProcedure">
      <deviceProcedure :dataProcess="processListViews" :devicelogs="listLogs"></deviceProcedure>
    </div>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="closeDialogProcedure()"
        class="p-button-outlined "
      />
    </template>
  </Dialog>
  <treeuser
    v-if="displayDialogUser === true"
    :headerDialog="headerDialogUser"
    :displayDialog="displayDialogUser"
    :closeDialog="closeDialogUser"
    :one="checkMultile"
    :selected="selectedUser"
    :choiceUser="choiceUser"
  />
</template>


<style scoped>
.product-item {
  display: flex;
  align-items: center;
  padding: 0.2rem;
  width: 100%;
}
.product-list-detail {
  flex: 1 1 0;
}

.product-list-action {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
}

.product-category-icon {
  vertical-align: middle;
  margin-right: 0.5rem;
  font-size: 0.875rem;
}

.product-category {
  vertical-align: middle;
  line-height: 1;
  font-size: 0.875rem;
}

@media screen and (max-width: 576px) {
  .product-item {
    flex-wrap: wrap;
  }
  .image-container {
    width: 100%;
    text-align: center;
  }

  img {
    margin: 0 0 1rem 0;
    width: 100px;
  }
}
</style>
        <style scoped>
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

.d-avatar-device_inventory {
  position: relative;
  width: 100%;
  height: 350px;
}
.d-avatar-device_inventory img {
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
  text-align: center !important;
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
    