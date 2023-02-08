<script setup>
import { ref, inject, onMounted } from "vue";
import { useToast } from "vue-toastification";
import { FilterMatchMode, FilterOperator } from "primevue/api";

import moment from "moment";
import { encr } from "../../util/function.js";

import deviceProcedure from "../../components/device/deviceProcedure.vue";
import detailsInventory from "../../components/device/detailsInventory.vue";
import detailsHandover from "../../components/device/detailsHandover.vue";
import detailsRecall from "../../components/device/detailsRecall.vue";
import {   checkURL } from "../../util/function.js";
//Khai báo
const router = inject("router");
const cryoptojs = inject("cryptojs");
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const basedomainURL = baseURL;
const selectedHandOver = ref();

//Nơi nhận dữ liệu

// emitter.on("emitData", (obj) => {
//   switch (obj.type) {
//     case "sendAccept":
//       loadData();
//       displayDeviceRepair.value = false;
//       break;
//     case "hideAccept":
//       displayDeviceRepair.value = false;
//       break;
//   }
// });

const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);

const listTCard = ref([
  { name: "Sửa chữa", code: 1 },
  { name: "Bảo trì - Bảo dưỡng", code: 2 },
]);
const filterSQL = ref([]);
const isDynamicSQL = ref(false);
const loadDataSQL = (index) => {
  let data = {
    id: " device_process_id",
    next: options.value.IsNext,
    sqlO: options.value.sort,
    Search: options.value.search,
    PageNo: options.value.pageno,
    PageSize: options.value.pagesize,
    fieldSQLS: filterSQL.value,
  };
  if(activeIndexTab.value!=0){
 let filterS1 = {
      filterconstraints: [{ value: activeIndexTab.value, matchMode: "equals" }],
      filteroperator: "and",
      key: "device_process_type",
    };

    filterSQL.value.push(filterS1);}
  options.value.loading = true;
  axios
    .post(baseURL + "/api/SQL/Filter_device_follows_all", data, config)
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
      if (dt.length > 1) {
        if (index != null) {
          if (index == 0) options.value.totalRecords = dt[1][0].totalRecords;
          if (index == 1)
            options.value.totalRecordsHandover = dt[2][0].totalRecordsHandover;
          if (index == 2)
            options.value.totalRecordsRepair = dt[3][0].totalRecordsRepair;
          if (index == 3)
            options.value.totalRecordsRecall = dt[4][0].totalRecordsRecall;
          if (index == 4)
            options.value.totalRecordsInventory =
              dt[5][0].totalRecordsInventory;
        }
        else{
           options.value.totalRecords = dt[1][0].totalRecords;
        
            options.value.totalRecordsHandover = dt[2][0].totalRecordsHandover;
          
            options.value.totalRecordsRepair = dt[3][0].totalRecordsRepair;
        
            options.value.totalRecordsRecall = dt[4][0].totalRecordsRecall;
        
            options.value.totalRecordsInventory =
              dt[5][0].totalRecordsInventory;
        }
      }
    })
    .catch((error) => {
      options.value.loading = false;
      
    });
};

const toast = useToast();
const isFirst = ref(true);
const datalists = ref();

const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};

const options = ref({
  IsNext: true,
  sort: " device_process_id DESC",

  search: "",
  pageno: 0,
  pagesize: 20,

  loading: true,
  totalRecords: null,
  totalRecordsDM: null,
  start_date: null,
  end_date: null,
  next: true,
});
const device_handover = ref();
const displayDetailsHandover = ref(false);
const danhMuc = ref();
const openDetailsHandover = (data) => {
  device_handover.value = {
    handover_id: data.device_note_id,
  };
  displayDetailsHandover.value = true;
};
const checkImg = (src) => {
  let allowedExtensions = /(\.jpg|\.jpeg|\.png|\.gif)$/i;
  return allowedExtensions.exec(src);
};
const closeDetailsHandover = () => {
  displayDetailsHandover.value = false;
  device_handover.value = null;
};
const closeDetailsRepair = () => {
  displayDetailsRepair.value = false;
  device_repair.value = null;
};

const loadData = () => {
  if (isDynamicSQL.value) {
    loadDataSQL(activeIndexTab.value);
    return false;
  }

  options.value.loading = false;
  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "device_follows_list",
            par: [
              { par: "pageno", va: options.value.pageno },
              { par: "pagesize", va: options.value.pagesize },
              { par: "user_id", va: store.state.user.user_id },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
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

      if (dt.length > 1) {
        options.value.totalRecords = dt[1][0].totalRecords;
        options.value.totalRecordsHandover = dt[2][0].totalRecordsHandover;
        options.value.totalRecordsRepair = dt[3][0].totalRecordsRepair;
        options.value.totalRecordsRecall = dt[4][0].totalRecordsRecall;
        options.value.totalRecordsInventory = dt[5][0].totalRecordsInventory;
      }
    })
    .catch((error) => {
    
      toast.error("Tải dữ liệu không thành công!");
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

 
const first = ref(0);
const liItemsDetails = ref([
  {
    label: "Xem phiếu",
    icon: "pi pi-book",
    command: () => {
      openDetails(deviceDataDetails.value);
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
const listAssetsH = ref([]);
const listFilesS = ref([]);
 
const device_inventory_id = ref();
const displayDetailsInventory = ref(false);
const displayDetailsRepair = ref(false);
const device_repair = ref();
const openDetails = (data) => {
  if (data.device_process_type == 1) {
    axios
      .post(
        baseURL + "/api/device_card/getData",
        {
          str: encr(
            JSON.stringify({
              proc: "device_repair_get",
              par: [{ par: "device_repair_id", va: data.device_note_id }],
            }),
            SecretKey,
            cryoptojs
          ).toString(),
        },
        config
      )
      .then((response) => {
        let data = JSON.parse(response.data.data)[0];
        let data1 = JSON.parse(response.data.data)[1];
        let data2 = JSON.parse(response.data.data)[2];
        device_repair.value = data[0];
        device_repair.value.repair_created_date = new Date(
          device_repair.value.repair_created_date
        );
        displayDetailsRepair.value = true;
        listAssetsH.value = data1;
        listFilesS.value = data2;
      })
      .catch((error) => {
      
      });
  }
  if (data.device_process_type == 2) {
    device_inventory_id.value = data.device_note_id;
    displayDetailsInventory.value = true;
  }
  if (data.device_process_type == 3) {
    device_recall_id.value = data.device_note_id;

    displayDetailsRecall.value = true;
  }
};
const closeDetailsInventory = () => {
  displayDetailsInventory.value = false;
};
const closeDetailsRecall = () => {
  displayDetailsRecall.value = false;
};

const device_recall_id = ref();
const displayDetailsRecall = ref(false);
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
                va: deviceDataDetails.value.device_note_id,
              },
              { par: "type", va: deviceDataDetails.value.device_process_type },
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

            eles.is_shows = true;
            element.listprocess.push(eles);
          });
        }

        if (
          element.aprroved_user != null &&
          element.is_approved_by_department == false
        ) {
          element.aprroved_user = JSON.parse(element.aprroved_user);

          element.aprroved_user.forEach((eles) => {
            eles.is_shows = false;
            element.listprocess.push(eles);
          });
        }

        if (element.listprocess.length < 1000) {
          lengthFor.value = element.listprocess.length;
        } else {
          lengthFor.value = 1000;
        }

        element.listprocess = compareProcess(element.listprocess);
        element.aprroved_user = null;
        element.device_process = null;
      });

      processListViews.value.push(data);
      processListViews.value.push(data1);
      loadLogs(deviceDataDetails.value.device_note_id,deviceDataDetails.value.device_process_type );
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
const lengthFor = ref(0);
function compareProcess(data) {
 // if (lengthFor.value > 0) {
    // for (let index = 0; index < lengthFor.value - 1; index++) {
    //   const element = data[index];
    //   for (let Jindex = 1; Jindex < lengthFor.value; Jindex++) {
    //     const Jelement = data[Jindex];
    //     if (data[index].user_stt > data[Jindex].user_stt) {
    //       data[index] = Jelement;
    //       data[Jindex] = element;
    //     }
    //   }
    // }

    // for (let index = 0; index < lengthFor.value; index++) {
    //   const element = data[index];
    //   if (element.is_approved != null) {
    //     for (let i = 0; i < index; i++) {
    //       data[i].is_approved = "2";
    //     }
    //   }
    // }
    
 // }
    data.forEach((element,index) => {
     if (element.is_approved != null) {
      for (let i = 0; i < index; i++) {
        data[i].is_approved = "2";
      
      }
    }
  });
  return data;
}
const menuDetailsR = ref();
const deviceDataDetails = ref();
const onShowDetails = (value) => {
  deviceDataDetails.value = value;
  menuDetailsR.value.toggle(event);
};
const filterTrangthai = ref();
const activeIndexTab = ref(0);
 
const loadFollowsFilter = (event) => {
  activeIndexTab.value=event.index;
  if (event.index == 0) {
    filterSQL.value = [];
    loadDataSQL(0);
  } else {
    let num = 0;
    if (event.index == 1) {
      num = -2;
    }
    if (event.index == 2) {
      num = 1;
    }
    if (event.index == 3) {
      num = 3;
    }
    if (event.index == 4) {
      num = 2;
    }
    filterSQL.value = [];
   activeIndexTab.value=num;
    loadDataSQL(event.index);
  }
};
//Xuất excel
const headerExport=ref('Cấu hình xuất Excel');
const showExport = ref(false);
const menuButs = ref();
const datalistsExport=ref();
var checkTypeExpport=false;

const itemButs = ref([
  {
    label: "Xuất Excel",
    icon: "pi pi-file-excel",
    command: (event) => {
      if (options.value.totalRecords < 10000) {
        options.value.totalRecordsExport=options.value.totalRecords;
        exportData("ExportExcel");
      } else {
        headerExport.value='Cấu hình xuất Excel';
     options.value.totalRecordsExport=50;
     checkTypeExpport=true;
        showExport.value = true;
      }
    },
  },
  {
    label: "In báo cáo",
    icon: "pi pi-print",
    command: (event) => {
      exportExcelR();
    //   headerExport.value='Cấu hình in báo cáo';
    //  options.value.totalRecordsExport=50;
   checkTypeExpport=false;
    //     showExport.value = true;
  
    },
  },
]);
const exportExcelR=()=>{
  showExport.value = false;


  if(checkTypeExpport)
  {  if(options.value.totalRecordsExport>10000)
  {  swal.fire({
                title: "Thông báo",
                text: "Nhập số bản ghi nhỏ hơn 10000.",
                icon: "error",
                confirmButtonText: "OK",
              });
return;
  }
  exportData("ExportExcel");}
  else
  {
    options.value.loading=true;


    let data = {
    id: options.value.id,
    next: options.value.IsNext,
    sqlO: options.value.sort,
    Search: options.value.search,
    PageNo: options.value.pageno,
    PageSize: options.value.pagesize,
    fieldSQLS: filterSQL.value,
  };

  options.value.loading = true;
  axios
    .post(baseURL + "/api/SQL/Filter_device_report_expiration", data, config)
    .then((response) => {
      let dt = JSON.parse(response.data.data);

      let data = dt[0];
      if (data.length > 0) {
        data.forEach(data => {
          if(data.device_user_name ==null){
            data.device_user_name='';
          }
        });
        datalistsExport.value = data;
        
         
           print();

      options.value.loading = false;
      } else {
        datalistsExport.value = [];
      }
      options.value.loading = false;
      //Show Count nếu có
      if (dt.length == 2) {
        options.value.totalRecords = dt[1][0].totalRecords;
      }
    })
    .catch((error) => {
      options.value.loading = false;
      toast.error("Tải dữ liệu không thành công!");
     
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });

 
  }
}
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
        excelname: "DANH SÁCH THIẾT BỊ",
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
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};

const filters = ref({
  global: { value: null, matchMode: FilterMatchMode.CONTAINS },
  device_process_code: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
  created_date: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.DATE_IS }],
  },
  created_name: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.EQUALS }],
  },
});

const onPage = (event) => {
  options.value.pagesize = event.rows;
  options.value.pageno = event.page;
  loadDataSQL(activeIndexTab.value);
};
//Sort
const onSort = (event) => {
  if (event.sortField == null) {
    isDynamicSQL.value = false;
    loadData();
  } else {
    options.value.sort =
      event.sortField + (event.sortOrder == 1 ? " ASC" : " DESC");
    if (event.sortField != " device_process_id") {
      options.value.sort +=
        ", device_process_id " + (event.sortOrder == 1 ? " ASC" : " DESC");
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
const Refresh = () => {
  options.value.loading=true;
  options.value.search = null;
  options.value.pageno = 0;
  options.value.pagesize = 20;
  activeIndexTab.value=null;
  filters.value = {
    global: { value: null, matchMode: FilterMatchMode.CONTAINS },
    device_process_code: {
      operator: FilterOperator.AND,
      constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
    },
    created_date: {
      operator: FilterOperator.AND,
      constraints: [{ value: null, matchMode: FilterMatchMode.DATE_IS }],
    },
    created_name: {
      operator: FilterOperator.AND,
      constraints: [{ value: null, matchMode: FilterMatchMode.EQUALS }],
    },
  };
  isDynamicSQL.value = false;

      loadDataSQL(activeIndexTab.value);
};

onMounted(() => {  if (!checkURL(window.location.pathname, store.getters.listModule)) {
     //router.back();
  }
  loadData();
  return {
    isFirst,
    options,
    danhMuc,
  };
});
</script>
        
  <template>
  <div class="main-layout true flex-grow-1 p-2 pb-0 pr-0 pt-0 surface-0 mt-2">
    <div class="pt-2 pl-2">
      <div class="p-0 py-0 surface-100">
        <h3 class="module-title m-0 p-2 px-0 surface-0">
          <i class="pi pi-eye"></i> Theo dõi danh sách thiết bị
        </h3>

        <Toolbar class="w-full custoolbar surface-0 px-0">
          <template #start>
            <span class="p-input-icon-left">
              <i class="pi pi-search" />
              <InputText
                v-model="options.search"
                @keyup.enter="loadDataSQL(activeIndexTab)"
                type="text"
                spellcheck="false"
                placeholder="Tìm kiếm"
              />
            </span>
            <!-- <Button
              :class="
                (filterTrangthai != null || filterTypeDevice != null) &&
                checkFilter
                  ? ''
                  : 'p-button-secondary p-button-outlined'
              "
              type="button"
              class="ml-2"
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
                store.state.user.is_super == 1 ? 'width:25rem' : 'width:300px'
              "
            >
              <div class="grid formgrid m-0">
                <div class="flex field col-12 p-0 align-items-center">
                  <div class="col-4">Trạng thái</div>
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
                <div class="flex field col-12 p-0 align-items-center">
                  <div class="col-4">Loại thiết bị</div>
                  <div class="col-8">
                    <Dropdown
                      v-model="filterTypeDevice"
                      :options="listType"
                      optionLabel="name"
                      optionValue="code"
                      placeholder="----- Loại thiết bị -----"
                      spellcheck="false"
                      class="w-full"
                      panelClass="d-design-dropdown"
                    >
                      <template #option="slotProps">
                        <div class="p-dropdown-car-option text-2line">
                          {{ slotProps.option.name }}
                        </div>
                      </template>
                    </Dropdown>
                  </div>
                </div>
                <div class="flex col-12 p-0">
                  <Toolbar
                    class="border-none surface-0 outline-none pb-0 w-full"
                  >
                    <template #start>
                      <Button
                        @click="reFilter()"
                        class="p-button-outlined"
                        label="Xóa"
                      ></Button>
                    </template>
                    <template #end>
                      <Button @click="filter()" label="Lọc"></Button>
                    </template>
                  </Toolbar>
                </div>
              </div>
            </OverlayPanel> -->
          </template>

          <template #end>
            <Button
              @click="Refresh"
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
        </Toolbar>

        <TabView
          class="tabview-custom-d"
       
          @tab-click="loadFollowsFilter($event)"
        >
          <TabPanel class="p-0">
            <template #header>
              <i class="pi pi-bars pr-2"></i>
              <span> Tất cả ({{ options.totalRecords }})</span>
            </template>
            <div class="d-lang-table mx-0">
              <DataTable
                class="w-full p-datatable-sm e-sm"
                @page="onPage($event)"
                @filter="onFilter($event)"
                @sort="onSort($event)"
                v-model:filters="filters"
                removableSort
                filterDisplay="menu"
                filterMode="lenient"
                dataKey="device_process_code"
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
                <Column
                   
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
                  field="device_process_code"
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
                  headerStyle="text-align:center;height:50px;max-width:180px;"
                  headerClass="text-center format-center"
                  bodyStyle="text-align:center;max-width:180px"
                  field="created_name"
                  header="Người gửi"
                >
                  <template #body="data">
                    <div class="w-full">
                      <div
                        class="flex surface-100 align-items-center pr-2"
                        style="border-radius: 16px"
                        v-if="data.data.created_name"
                      > 
                        <Avatar
                          v-bind:label="
                            data.data.created_avatar
                              ? ''
                              : data.data.created_name.substring(
                                  data.data.created_name.lastIndexOf(' ') + 1,
                                  data.data.created_name.lastIndexOf(' ') + 2
                                )
                          "
                          :image="basedomainURL + data.data.created_avatar"
                          size="small"
                          :style="
                            data.data.created_avatar
                              ? 'background-color: #2196f3'
                              : 'background:' +
                                bgColor[data.data.created_name.length % 7]
                          "
                          shape="circle"
                          @error="
                            $event.target.src =
                              basedomainURL + '/Portals/Image/nouser1.png'
                          "
                        />
                        <div class="px-2">{{ data.data.created_name }}</div>
                      </div>
                    </div>
                  </template>
                </Column>

                <Column
                  class="align-items-center justify-content-center text-center"
                  headerStyle="text-align:center;max-width:150px;height:50px"
                  bodyStyle="text-align:center;max-width:150px"
                  field="created_date"
                  filterField="created_date"
                  dataType="date"
                  header="Ngày lập"
                  :sortable="true"
                >
                  <template #filter="{ filterModel }">
                    <Calendar
                      v-model="filterModel.value"
                      class="p-column-filter"
                      placeholder="dd/MM/yy"
                      dateFormat="mm/dd/yy"
                    />
                  </template>
                  <template #body="data">
                    <div>
                      {{
                        moment(new Date(data.data.created_date)).format(
                          "DD/MM/YYYY"
                        )
                      }}
                    </div>
                  </template>
                </Column>

                <Column
                  headerStyle="text-align:center;height:50px"
                  bodyStyle="text-align:center"
                  field="content"
                  header="Nội dung trình"
                >
                  <template #body="data">
                    <div>
                      {{ data.data.content }}
                    </div>
                  </template>
                </Column>

                <Column
                  class="align-items-center justify-content-center text-center"
                  headerStyle="text-align:center;max-width:150px;height:50px"
                  bodyStyle="text-align:center;max-width:150px"
                  field="status"
                  header="Loại phiếu"
                >
                  <template #body="data">
                    <div class="w-full">
                      <div v-if="data.data.device_process_type == -2">
                        Phiếu cấp phát
                      </div>
                      <div v-if="data.data.device_process_type == 1">
                        Phiếu sửa chữa
                      </div>
                      <div v-if="data.data.device_process_type == 2">
                        Phiếu kiểm kê
                      </div>
                      <div v-if="data.data.device_process_type == 3">
                        Phiếu thu hồi
                      </div>
                    </div>
                  </template>
                </Column>

                <Column
                  class="align-items-center justify-content-center text-center"
                  headerStyle="text-align:center;height:50px;max-width:70px"
                  bodyStyle="text-align:center;max-width:70px"
                  header=""
                >
                  <template #body="data">
                    <!-- <SpeedDial :model="itemsBtnFunc"  :radius="80" direction="left" type="semi-circle " /> -->
                    <div v-if="data.data.device_process_type != -2">
                      <Button
                        v-tooltip.left="'Chi tiết phiếu'"
                        class="
                          p-button-rounded p-button-secondary p-button-outlined
                          mx-1
                        "
                        @click="onShowDetails(data.data)"
                        type="button"
                        icon="pi pi-info-circle"
                        aria-haspopup="true"
                        aria-controls="overlay_menu_details"
                      ></Button>
                    </div>
                    <div v-else>
                      <Button
                        v-tooltip.left="'Chi tiết phiếu'"
                        class="
                          p-button-rounded p-button-secondary p-button-outlined
                          mx-1
                        "
                        @click="openDetailsHandover(data.data)"
                        type="button"
                        icon="pi pi-info-circle"
                        aria-haspopup="true"
                        aria-controls="overlay_menu_details"
                      ></Button>
                    </div>
                    <Menu
                      id="overlay_menu_details"
                      ref="menuDetailsR"
                      :model="liItemsDetails"
                      :popup="true"
                    />
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
                    <img
                      src="../../assets/background/nodata.png"
                      height="144"
                    />
                    <h3 class="m-1">Không có dữ liệu</h3>
                  </div>
                </template>
              </DataTable>
            </div>
          </TabPanel>
          <TabPanel>
            <template #header>
              <font-awesome-icon
                icon="fa-solid fa-location-crosshairs"
                class="pr-2"
              />
              <span> Cấp phát ({{ options.totalRecordsHandover }})</span>
            </template>
            <div class="d-lang-table mx-0">
              <DataTable
                class="w-full p-datatable-sm e-sm"
                @page="onPage($event)"
                @filter="onFilter($event)"
                @sort="onSort($event)"
                v-model:filters="filters"
                removableSort
                filterDisplay="menu"
                filterMode="lenient"
                dataKey="device_process_code"
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
                  field="device_process_code"
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
                  headerStyle="text-align:center;height:50px;max-width:180px;"
                  headerClass="text-center format-center"
                  bodyStyle="text-align:center;max-width:180px"
                  field="created_name"
                  header="Người gửi"
                >
                  <template #body="data">
                    <div class="w-full">
                      <div
                        class="flex surface-100 align-items-center pr-2"
                        style="border-radius: 16px" v-if="data.data.created_name"
                      >
                        <Avatar
                          v-bind:label="
                            data.data.created_avatar
                              ? ''
                              : data.data.created_name.substring(
                                  data.data.created_name.lastIndexOf(' ') + 1,
                                  data.data.created_name.lastIndexOf(' ') + 2
                                )
                          "
                          :image="basedomainURL + data.data.created_avatar"
                          size="small"
                          :style="
                            data.data.created_avatar
                              ? 'background-color: #2196f3'
                              : 'background:' +
                                bgColor[data.data.created_name.length % 7]
                          "
                          shape="circle"
                          @error="
                            $event.target.src =
                              basedomainURL + '/Portals/Image/nouser1.png'
                          "
                        />
                        <div class="px-2">{{ data.data.created_name }}</div>
                      </div>
                    </div>
                  </template>
                </Column>

                <Column
                  class="align-items-center justify-content-center text-center"
                  headerStyle="text-align:center;max-width:150px;height:50px"
                  bodyStyle="text-align:center;max-width:150px"
                  field="created_date"
                  filterField="created_date"
                  dataType="date"
                  header="Ngày lập"
                  :sortable="true"
                >
                  <template #filter="{ filterModel }">
                    <Calendar
                      v-model="filterModel.value"
                      class="p-column-filter"
                      placeholder="dd/MM/yy"
                      dateFormat="mm/dd/yy"
                    />
                  </template>
                  <template #body="data">
                    <div>
                      {{
                        moment(new Date(data.data.created_date)).format(
                          "DD/MM/YYYY"
                        )
                      }}
                    </div>
                  </template>
                </Column>

                <Column
                  headerStyle="text-align:center;height:50px"
                  bodyStyle="text-align:center"
                  field="content"
                  header="Nội dung trình"
                >
                  <template #body="data">
                    <div>
                      {{ data.data.content }}
                    </div>
                  </template>
                </Column>

                <Column
                  class="align-items-center justify-content-center text-center"
                  headerStyle="text-align:center;max-width:150px;height:50px"
                  bodyStyle="text-align:center;max-width:150px"
                  field="status"
                  header="Loại phiếu"
                >
                  <template #body="data">
                    <div class="w-full">
                      <div v-if="data.data.device_process_type == -2">
                        Phiếu cấp phát
                      </div>
                      <div v-if="data.data.device_process_type == 1">
                        Phiếu sửa chữa
                      </div>
                      <div v-if="data.data.device_process_type == 2">
                        Phiếu kiểm kê
                      </div>
                      <div v-if="data.data.device_process_type == 3">
                        Phiếu thu hồi
                      </div>
                    </div>
                  </template>
                </Column>

                <Column
                  class="align-items-center justify-content-center text-center"
                  headerStyle="text-align:center;height:50px;max-width:70px"
                  bodyStyle="text-align:center;max-width:70px"
                  header=""
                >
                  <template #body="data">
                    <!-- <SpeedDial :model="itemsBtnFunc"  :radius="80" direction="left" type="semi-circle " /> -->
                    <div v-if="data.data.device_process_type != -2">
                      <Button
                        v-tooltip.left="'Chi tiết phiếu'"
                        class="
                          p-button-rounded p-button-secondary p-button-outlined
                          mx-1
                        "
                        @click="onShowDetails(data.data)"
                        type="button"
                        icon="pi pi-info-circle"
                        aria-haspopup="true"
                        aria-controls="overlay_menu_details"
                      ></Button>
                    </div>
                    <div v-else>
                      <Button
                        v-tooltip.left="'Chi tiết phiếu'"
                        class="
                          p-button-rounded p-button-secondary p-button-outlined
                          mx-1
                        "
                        @click="openDetailsHandover(data.data)"
                        type="button"
                        icon="pi pi-info-circle"
                        aria-haspopup="true"
                        aria-controls="overlay_menu_details"
                      ></Button>
                    </div>
                    <Menu
                      id="overlay_menu_details"
                      ref="menuDetailsR"
                      :model="liItemsDetails"
                      :popup="true"
                    />
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
                    <img
                      src="../../assets/background/nodata.png"
                      height="144"
                    />
                    <h3 class="m-1">Không có dữ liệu</h3>
                  </div>
                </template>
              </DataTable>
            </div>
          </TabPanel>
          <TabPanel>
            <template #header>
              <font-awesome-icon
                icon="fa-solid fa-screwdriver-wrench"
                class="pr-2"
              />
              <span> Sửa chữa ({{ options.totalRecordsRepair }})</span>
            </template>
            <div class="d-lang-table mx-0">
              <DataTable
                class="w-full p-datatable-sm e-sm"
                @page="onPage($event)"
                @filter="onFilter($event)"
                @sort="onSort($event)"
                v-model:filters="filters"
                removableSort
                filterDisplay="menu"
                filterMode="lenient"
                dataKey="device_process_code"
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
                  field="device_process_code"
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
                  headerStyle="text-align:center;height:50px;max-width:180px;"
                  headerClass="text-center format-center"
                  bodyStyle="text-align:center;max-width:180px"
                  field="created_name"
                  header="Người gửi"
                >
                  <template #body="data">
                    <div class="w-full">
                      <div
                        class="flex surface-100 align-items-center pr-2"
                        style="border-radius: 16px" v-if="data.data.created_name"
                      >
                        <Avatar
                          v-bind:label="
                            data.data.created_avatar
                              ? ''
                              : data.data.created_name.substring(
                                  data.data.created_name.lastIndexOf(' ') + 1,
                                  data.data.created_name.lastIndexOf(' ') + 2
                                )
                          "
                          :image="basedomainURL + data.data.created_avatar"
                          size="small"
                          :style="
                            data.data.created_avatar
                              ? 'background-color: #2196f3'
                              : 'background:' +
                                bgColor[data.data.created_name.length % 7]
                          "
                          shape="circle"
                          @error="
                            $event.target.src =
                              basedomainURL + '/Portals/Image/nouser1.png'
                          "
                        />
                        <div class="px-2">{{ data.data.created_name }}</div>
                      </div>
                    </div>
                  </template>
                </Column>

                <Column
                  class="align-items-center justify-content-center text-center"
                  headerStyle="text-align:center;max-width:150px;height:50px"
                  bodyStyle="text-align:center;max-width:150px"
                  field="created_date"
                  filterField="created_date"
                  dataType="date"
                  header="Ngày lập"
                  :sortable="true"
                >
                  <template #filter="{ filterModel }">
                    <Calendar
                      v-model="filterModel.value"
                      class="p-column-filter"
                      placeholder="dd/MM/yy"
                      dateFormat="mm/dd/yy"
                    />
                  </template>
                  <template #body="data">
                    <div>
                      {{
                        moment(new Date(data.data.created_date)).format(
                          "DD/MM/YYYY"
                        )
                      }}
                    </div>
                  </template>
                </Column>

                <Column
                  headerStyle="text-align:center;height:50px"
                  bodyStyle="text-align:center"
                  field="content"
                  header="Nội dung trình"
                >
                  <template #body="data">
                    <div>
                      {{ data.data.content }}
                    </div>
                  </template>
                </Column>

                <Column
                  class="align-items-center justify-content-center text-center"
                  headerStyle="text-align:center;max-width:150px;height:50px"
                  bodyStyle="text-align:center;max-width:150px"
                  field="status"
                  header="Loại phiếu"
                >
                  <template #body="data">
                    <div class="w-full">
                      <div v-if="data.data.device_process_type == -2">
                        Phiếu cấp phát
                      </div>
                      <div v-if="data.data.device_process_type == 1">
                        Phiếu sửa chữa
                      </div>
                      <div v-if="data.data.device_process_type == 2">
                        Phiếu kiểm kê
                      </div>
                      <div v-if="data.data.device_process_type == 3">
                        Phiếu thu hồi
                      </div>
                    </div>
                  </template>
                </Column>

                <Column
                  class="align-items-center justify-content-center text-center"
                  headerStyle="text-align:center;height:50px;max-width:70px"
                  bodyStyle="text-align:center;max-width:70px"
                  header=""
                >
                  <template #body="data">
                    <!-- <SpeedDial :model="itemsBtnFunc"  :radius="80" direction="left" type="semi-circle " /> -->
                    <div v-if="data.data.device_process_type != -2">
                      <Button
                        v-tooltip.left="'Chi tiết phiếu'"
                        class="
                          p-button-rounded p-button-secondary p-button-outlined
                          mx-1
                        "
                        @click="onShowDetails(data.data)"
                        type="button"
                        icon="pi pi-info-circle"
                        aria-haspopup="true"
                        aria-controls="overlay_menu_details"
                      ></Button>
                    </div>
                    <div v-else>
                      <Button
                        v-tooltip.left="'Chi tiết phiếu'"
                        class="
                          p-button-rounded p-button-secondary p-button-outlined
                          mx-1
                        "
                        @click="openDetailsHandover(data.data)"
                        type="button"
                        icon="pi pi-info-circle"
                        aria-haspopup="true"
                        aria-controls="overlay_menu_details"
                      ></Button>
                    </div>
                    <Menu
                      id="overlay_menu_details"
                      ref="menuDetailsR"
                      :model="liItemsDetails"
                      :popup="true"
                    />
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
                    <img
                      src="../../assets/background/nodata.png"
                      height="144"
                    />
                    <h3 class="m-1">Không có dữ liệu</h3>
                  </div>
                </template>
              </DataTable>
            </div>
          </TabPanel>
          <TabPanel>
            <template #header>
              <font-awesome-icon icon="fa-solid fa-rotate-left" class="pr-2" />
              <span> Thu hồi ({{ options.totalRecordsRecall }})</span>
            </template>
            <div class="d-lang-table mx-0">
              <DataTable
                class="w-full p-datatable-sm e-sm"
                @page="onPage($event)"
                @filter="onFilter($event)"
                @sort="onSort($event)"
                v-model:filters="filters"
                removableSort
                filterDisplay="menu"
                filterMode="lenient"
                dataKey="device_process_code"
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
                  field="device_process_code"
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
                  headerStyle="text-align:center;height:50px;max-width:180px;"
                  headerClass="text-center format-center"
                  bodyStyle="text-align:center;max-width:180px"
                  field="created_name"
                  header="Người gửi"
                >
                  <template #body="data">
                    <div class="w-full">
                      <div
                        class="flex surface-100 align-items-center pr-2"
                        style="border-radius: 16px"  v-if="data.data.created_name"
                      >
                        <Avatar
                          v-bind:label="
                            data.data.created_avatar
                              ? ''
                              : data.data.created_name.substring(
                                  data.data.created_name.lastIndexOf(' ') + 1,
                                  data.data.created_name.lastIndexOf(' ') + 2
                                )
                          "
                          :image="basedomainURL + data.data.created_avatar"
                          size="small"
                          :style="
                            data.data.created_avatar
                              ? 'background-color: #2196f3'
                              : 'background:' +
                                bgColor[data.data.created_name.length % 7]
                          "
                          shape="circle"
                          @error="
                            $event.target.src =
                              basedomainURL + '/Portals/Image/nouser1.png'
                          "
                        />
                        <div class="px-2">{{ data.data.created_name }}</div>
                      </div>
                    </div>
                  </template>
                </Column>

                <Column
                  class="align-items-center justify-content-center text-center"
                  headerStyle="text-align:center;max-width:150px;height:50px"
                  bodyStyle="text-align:center;max-width:150px"
                  field="created_date"
                  filterField="created_date"
                  dataType="date"
                  header="Ngày lập"
                  :sortable="true"
                >
                  <template #filter="{ filterModel }">
                    <Calendar
                      v-model="filterModel.value"
                      class="p-column-filter"
                      placeholder="dd/MM/yy"
                      dateFormat="mm/dd/yy"
                    />
                  </template>
                  <template #body="data">
                    <div>
                      {{
                        moment(new Date(data.data.created_date)).format(
                          "DD/MM/YYYY"
                        )
                      }}
                    </div>
                  </template>
                </Column>

                <Column
                  headerStyle="text-align:center;height:50px"
                  bodyStyle="text-align:center"
                  field="content"
                  header="Nội dung trình"
                >
                  <template #body="data">
                    <div>
                      {{ data.data.content }}
                    </div>
                  </template>
                </Column>

                <Column
                  class="align-items-center justify-content-center text-center"
                  headerStyle="text-align:center;max-width:150px;height:50px"
                  bodyStyle="text-align:center;max-width:150px"
                  field="status"
                  header="Loại phiếu"
                >
                  <template #body="data">
                    <div class="w-full">
                      <div v-if="data.data.device_process_type == -2">
                        Phiếu cấp phát
                      </div>
                      <div v-if="data.data.device_process_type == 1">
                        Phiếu sửa chữa
                      </div>
                      <div v-if="data.data.device_process_type == 2">
                        Phiếu kiểm kê
                      </div>
                      <div v-if="data.data.device_process_type == 3">
                        Phiếu thu hồi
                      </div>
                    </div>
                  </template>
                </Column>

                <Column
                  class="align-items-center justify-content-center text-center"
                  headerStyle="text-align:center;height:50px;max-width:70px"
                  bodyStyle="text-align:center;max-width:70px"
                  header=""
                >
                  <template #body="data">
                    <!-- <SpeedDial :model="itemsBtnFunc"  :radius="80" direction="left" type="semi-circle " /> -->
                    <div v-if="data.data.device_process_type != -2">
                      <Button
                        v-tooltip.left="'Chi tiết phiếu'"
                        class="
                          p-button-rounded p-button-secondary p-button-outlined
                          mx-1
                        "
                        @click="onShowDetails(data.data)"
                        type="button"
                        icon="pi pi-info-circle"
                        aria-haspopup="true"
                        aria-controls="overlay_menu_details"
                      ></Button>
                    </div>
                    <div v-else>
                      <Button
                        v-tooltip.left="'Chi tiết phiếu'"
                        class="
                          p-button-rounded p-button-secondary p-button-outlined
                          mx-1
                        "
                        @click="openDetailsHandover(data.data)"
                        type="button"
                        icon="pi pi-info-circle"
                        aria-haspopup="true"
                        aria-controls="overlay_menu_details"
                      ></Button>
                    </div>
                    <Menu
                      id="overlay_menu_details"
                      ref="menuDetailsR"
                      :model="liItemsDetails"
                      :popup="true"
                    />
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
                    <img
                      src="../../assets/background/nodata.png"
                      height="144"
                    />
                    <h3 class="m-1">Không có dữ liệu</h3>
                  </div>
                </template>
              </DataTable>
            </div>
          </TabPanel>
          <!-- <TabPanel>
            <template #header>
              <font-awesome-icon
                icon="fa-solid fa-money-bill-1-wave"
                class="pr-2"
              />
              <span> Thanh lý</span>
            </template>
            <p>Chưa có dữ liệu.</p>
          </TabPanel>
          <TabPanel>
            <template #header>
              <font-awesome-icon icon="fa-solid fa-square-minus" class="pr-2" />
              <span> Thất thoát</span>
            </template>
            <p>Chưa có dữ liệu.</p>
          </TabPanel> -->
          <TabPanel>
            <template #header>
              <font-awesome-icon
                icon="fa-solid fa-boxes-stacked"
                class="pr-2"
              />
              <span> Kiểm kê ({{ options.totalRecordsInventory }})</span>
            </template>
            <div class="d-lang-table mx-0">
              <DataTable
                class="w-full p-datatable-sm e-sm"
                @page="onPage($event)"
                @filter="onFilter($event)"
                @sort="onSort($event)"
                v-model:filters="filters"
                removableSort
                filterDisplay="menu"
                filterMode="lenient"
                dataKey="device_process_code"
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
                  field="device_process_code"
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
                  headerStyle="text-align:center;height:50px;max-width:180px;"
                  headerClass="text-center format-center"
                  bodyStyle="text-align:center;max-width:180px"
                  field="created_name"
                  header="Người gửi"
                >
                  <template #body="data">
                    <div class="w-full">
                      <div
                        class="flex surface-100 align-items-center pr-2"
                        style="border-radius: 16px"  v-if="data.data.created_name"
                      >
                        <Avatar
                          v-bind:label="
                            data.data.created_avatar
                              ? ''
                              : data.data.created_name.substring(
                                  data.data.created_name.lastIndexOf(' ') + 1,
                                  data.data.created_name.lastIndexOf(' ') + 2
                                )
                          "
                          :image="basedomainURL + data.data.created_avatar"
                          size="small"
                          :style="
                            data.data.created_avatar
                              ? 'background-color: #2196f3'
                              : 'background:' +
                                bgColor[data.data.created_name.length % 7]
                          "
                          shape="circle"
                          @error="
                            $event.target.src =
                              basedomainURL + '/Portals/Image/nouser1.png'
                          "
                        />
                        <div class="px-2">{{ data.data.created_name }}</div>
                      </div>
                    </div>
                  </template>
                </Column>

                <Column
                  class="align-items-center justify-content-center text-center"
                  headerStyle="text-align:center;max-width:150px;height:50px"
                  bodyStyle="text-align:center;max-width:150px"
                  field="created_date"
                  filterField="created_date"
                  dataType="date"
                  header="Ngày lập"
                  :sortable="true"
                >
                  <template #filter="{ filterModel }">
                    <Calendar
                      v-model="filterModel.value"
                      class="p-column-filter"
                      placeholder="dd/MM/yy"
                      dateFormat="mm/dd/yy"
                    />
                  </template>
                  <template #body="data">
                    <div>
                      {{
                        moment(new Date(data.data.created_date)).format(
                          "DD/MM/YYYY"
                        )
                      }}
                    </div>
                  </template>
                </Column>

                <Column
                  headerStyle="text-align:center;height:50px"
                  bodyStyle="text-align:center"
                  field="content"
                  header="Nội dung trình"
                >
                  <template #body="data">
                    <div>
                      {{ data.data.content }}
                    </div>
                  </template>
                </Column>

                <Column
                  class="align-items-center justify-content-center text-center"
                  headerStyle="text-align:center;max-width:150px;height:50px"
                  bodyStyle="text-align:center;max-width:150px"
                  field="status"
                  header="Loại phiếu"
                >
                  <template #body="data">
                    <div class="w-full">
                      <div v-if="data.data.device_process_type == -2">
                        Phiếu cấp phát
                      </div>
                      <div v-if="data.data.device_process_type == 1">
                        Phiếu sửa chữa
                      </div>
                      <div v-if="data.data.device_process_type == 2">
                        Phiếu kiểm kê
                      </div>
                      <div v-if="data.data.device_process_type == 3">
                        Phiếu thu hồi
                      </div>
                    </div>
                  </template>
                </Column>

                <Column
                  class="align-items-center justify-content-center text-center"
                  headerStyle="text-align:center;height:50px;max-width:70px"
                  bodyStyle="text-align:center;max-width:70px"
                  header=""
                >
                  <template #body="data">
                    <!-- <SpeedDial :model="itemsBtnFunc"  :radius="80" direction="left" type="semi-circle " /> -->
                    <div v-if="data.data.device_process_type != -2">
                      <Button
                        v-tooltip.left="'Chi tiết phiếu'"
                        class="
                          p-button-rounded p-button-secondary p-button-outlined
                          mx-1
                        "
                        @click="onShowDetails(data.data)"
                        type="button"
                        icon="pi pi-info-circle"
                        aria-haspopup="true"
                        aria-controls="overlay_menu_details"
                      ></Button>
                    </div>
                    <div v-else>
                      <Button
                        v-tooltip.left="'Chi tiết phiếu'"
                        class="
                          p-button-rounded p-button-secondary p-button-outlined
                          mx-1
                        "
                        @click="openDetailsHandover(data.data)"
                        type="button"
                        icon="pi pi-info-circle"
                        aria-haspopup="true"
                        aria-controls="overlay_menu_details"
                      ></Button>
                    </div>
                    <Menu
                      id="overlay_menu_details"
                      ref="menuDetailsR"
                      :model="liItemsDetails"
                      :popup="true"
                    />
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
                    <img
                      src="../../assets/background/nodata.png"
                      height="144"
                    />
                    <h3 class="m-1">Không có dữ liệu</h3>
                  </div>
                </template>
              </DataTable>
            </div>
          </TabPanel>
          <!-- <TabPanel>
            <template #header>
              <font-awesome-icon icon="fa-solid fa-cart-flatbed" class="pr-2" />
              <span> Điều chuyển</span>
            </template>
            <p>Chưa có dữ liệu.</p>
          </TabPanel> -->
        </TabView>
      </div>
    </div>
  </div>
  
  <Dialog  :modal="true"  :style="{ width: '20vw' }" :header="headerExport" v-model:visible="showExport" >
	<div class="grid">
    <div class="col-12  field flex">
      <div class="col-6 p-0">
        Số bản ghi: 
      </div>
      <div class="col-6 p-0">
        <InputNumber class="w-full" v-model="options.totalRecordsExport" />
      </div>
    </div>
    <div class="col-12 field flex">
      <div class="col-6 p-0">
       Trang bắt đầu: 
      </div>
      <div class="col-6 p-0">
        <InputNumber class="w-full" v-model="options.pageno" />
      </div>
    </div>
    <div class="col-12 p-0">
      <Toolbar class="surface-0 p-0 border-0 ">
        <template #end>
          <div>
      <Button label="Xuất" @click="exportExcelR"></Button></div>
        </template>
      </Toolbar>
    </div>
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
        label="Đóng"
        icon="pi pi-times"
        @click="closeDialogProcedure()"
        class="p-button-outlined"
      />
    </template>
  </Dialog>

  <Dialog
    header="Phiếu cấp phát"
    v-model:visible="displayDetailsHandover"
    :maximizable="true"
    :style="{ width: '70vw' }"
    :modal="true"
  >
    <detailsHandover :handover="device_handover" :check="0" />
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
    header="Chi tiết phiếu thu hồi"
    v-model:visible="displayDetailsRecall"
    :modal="true"
    :maximizable="true"
    :style="{ width: '60vw' }"
  >
    <form>
      <div v-if="displayDetailsRecall">
        <detailsRecall :device_recall_id="device_recall_id" />
      </div>
      <form></form>
    </form>
    <template #footer>
      <Button
        @click="closeDetailsRecall"
        label="Đóng"
        icon="pi pi-times"
        autofocus
      />
    </template>
  </Dialog>

  <Dialog
    header="Chi tiết phiếu sửa chữa"
    v-model:visible="displayDetailsRepair"
    :maximizable="true"
    :style="{ width: '70vw' }" :modal="true" 
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="col-12 field p-0 text-lg font-bold">Thông tin phiếu</div>
        <div class="col-12 field flex p-0">
          <div class="col-6 flex p-0 align-items-center font-bold">
            <div class="w-10rem">Số phiếu</div>
            <div style="width: calc(100% - 10rem)">
              <InputText
                v-model="device_repair.repair_number"
                class="w-full class-disabled"
                :disabled="true"
              />
            </div>
          </div>
          <div class="col-6 flex p-0 text-center align-items-center">
            <div class="col-4 p-0 pl-5 text-left font-bold">Ngày lập</div>
            <div class="col-8 p-0 flex text-left">
              <Calendar
                placeholder="dd/mm/yyyy"
                class="w-full class-disabled"
                id="basic_use_date"
                v-model="device_repair.repair_created_date"
                :disabled="true"
                :showIcon="true"
              />
            </div>
          </div>
        </div>

        <div class="col-12 flex p-0">
          <div class="col-6 p-0 text-left align-items-center">
            <div class="col-12 field p-0 flex text-left align-items-center">
              <div class="w-10rem font-bold">Kiểu phiếu</div>
              <div style="width: calc(100% - 10rem)">
                <Dropdown
                  v-model="device_repair.repair_type"
                  :options="listTCard"
                  optionLabel="name"
                  optionValue="code"
                  class="w-full class-disabled"
                  :disabled="true"
                />
              </div>
            </div>
          </div>
          <div class="col-6 p-0 text-left align-items-center">
            <div class="col-12 field p-0 flex text-left align-items-center">
              <div class="col-4 p-0 pl-5 text-left font-bold">Nơi lập:</div>
              {{ device_repair.repair_created_place }}
            </div>
          </div>
        </div>
        <div class="field py-2 px-0 col-12 flex">
          <div class="col-6 p-0 flex">
            <div class="col-4 p-0 font-bold text-lg">Người đề xuất</div>
          </div>
        </div>
        <div class="field p-0 col-12 flex">
          <div class="col-6 p-0 flex align-items-center">
            <div class="w-10rem font-bold">Người đề xuất:</div>
            <div style="width: calc(100% - 10rem)">
              <div class="country-item flex align-items-center">
                <Avatar
                  v-bind:label="
                    device_repair.avartar
                      ? ''
                      : device_repair.proposer.substring(
                          device_repair.proposer.lastIndexOf(' ') + 1,
                          device_repair.proposer.lastIndexOf(' ') + 2
                        )
                  "
                  :image="basedomainURL + device_repair.avartar"
                  class="w-2rem h-2rem"
                  size="large"
                  :style="
                    device_repair.avartar
                      ? 'background-color: #2196f3'
                      : 'background:' +
                        bgColor[device_repair.proposer.length % 7]
                  "
                  shape="circle"
                  @error="
                    $event.target.src =
                      basedomainURL + '/Portals/Image/nouser1.png'
                  "
                />
                <div class="pt-1 pl-2">
                  {{ device_repair.proposer }}
                </div>
              </div>
            </div>
          </div>
          <div class="col-6 p-0 flex align-items-center">
            <div class="col-4 p-0 pl-5 font-bold">Phòng ban:</div>
            <div class="col-8 p-0">
              {{ device_repair.department_name }}
            </div>
          </div>
        </div>
        <div class="col-12 field flex">
          <div class="col-6 p-0 flex"></div>
          <div class="col-6 p-0 flex">
            <div class="col-4 p-0 pl-5 font-bold">Chức vụ:</div>
            <div class="col-8 p-0">
              {{ device_repair.position_name }}
            </div>
          </div>
        </div>

        <div class="col-12 field p-0 text-lg font-bold">
          <Toolbar class="p-0 surface-0 border-none">
            <template #start>
              <div class="text-lg font-bold">Thông tin thiết bị kèm theo</div>
            </template>
            <template #end> </template>
          </Toolbar>
        </div>

        <div class="col-12 p-0 px-8">
          <!-- <div class="w-10rem px-2"></div> -->
          <div
            style="border-radius: 5px"
            class="
              w-full
              field
              p-2
              border-none
              image-container
              border-2 border-solid border-300
            "
            v-for="(item, index) in listAssetsH"
            :key="index"
          >
            <div class="product-item surface-50 p-0 field">
              <div class="w-10rem pr-2 relative">
                <Image
                  :src="
                    item.image
                      ? basedomainURL + item.image
                      : basedomainURL + '/Portals/Image/noimg.jpg'
                  "
                  width="120"
                  height="75"
                  style="object-fit: contain; width: 100%"
                  class="p-2 cursor-pointer"
                  preview
                  :alt="item.image"
                  @error="
                    $event.target.src =
                      basedomainURL + '/Portals/Image/noimg.jpg'
                  "
                />
              </div>
              <div class="product-list-detail">
                <h5 class="my-2 text-justify">
                  {{ item.device_name }}
                </h5>

                <div class="flex pb-2">
                  <div class="w-full">
                    <i
                      class="pi pi-tag product-category-icon"
                      v-tooltip.top="'Số hiệu'"
                    ></i>
                    <span class="product-category">{{
                      item.device_number
                    }}</span>
                  </div>
                  <div class="w-full">
                    <i
                      class="pi pi-qrcode product-category-icon"
                      v-tooltip.top="'Mã Barcode'"
                    ></i>
                    <span class="product-category">{{ item.barcode_id }}</span>
                  </div>
                </div>
                <div class="flex">
                  <div class="w-full">
                    <i
                      class="pi pi-shopping-cart product-category-icon"
                      v-tooltip.top="'Ngày mua'"
                    ></i>
                    <span class="product-category">
                      {{
                        moment(new Date(item.purchase_date)).format(
                          "DD/MM/YYYY"
                        )
                      }}
                    </span>
                  </div>

                  <div class="w-full flex">
                    <div v-tooltip.top="'Giá trị hiện tại'">
                      <font-awesome-icon icon="fa-solid fa-money-bill-1-wave" />
                    </div>
                    <span class="product-category pl-2">
                      {{
                        item.current_price
                          ? item.current_price.toLocaleString()
                          : "0"
                      }}
                      VND
                    </span>
                  </div>
                </div>
              </div>

              <div class="pr-2">
                <!-- <Button
                  icon="pi pi-times"
                  class="p-button-rounded p-button-danger"
                  @click="deleteFileD(item)"
                /> -->
              </div>
            </div>
            <div class="w-full field flex align-items-center">
              <div class="w-10rem p-0 font-bold">Tình trạng:</div>
              <div class="p-0" style="width: calc(100% - 10rem)">
                {{ item.condition }}
              </div>
            </div>
            <div class="flex w-full field pt-2 align-items-center">
              <div class="font-bold w-10rem">Phương hướng sửa:</div>
              <div style="width: calc(100% - 10rem)" class="">
                {{ item.repair_plan }}
              </div>
            </div>
            <div class="flex w-full pt-2">
              <div class="flex field w-6 p-0 align-items-center">
                <div class="font-bold w-10rem">Tình trạng sửa:</div>

                <div style="width: calc(100% - 10rem)" class="">
                  {{
                    item.repair_condition == 2
                      ? "Trong kho chờ sửa chữa"
                      : item.repair_condition == 3
                      ? "Hỏng không sửa được"
                      : item.repair_condition == 1
                      ? "Hoàn thành sửa chữa"
                      : ""
                  }}
                </div>
              </div>
              <div class="flex w-6 p-0 align-items-center">
                <div class="font-bold w-10rem format-center">Giá sửa chữa:</div>
                <div style="width: calc(100% - 10rem)" class="">
                  {{
                    item.repair_price
                      ? item.repair_price.toLocaleString() + " VND"
                      : ""
                  }}
                </div>
              </div>
            </div>

            <div class="flex w-full pt-2 align-items-center">
              <div class="font-bold w-10rem">Ghi chú:</div>
              <div style="width: calc(100% - 10rem)" class="">
                {{ item.repair_note }}
              </div>
            </div>
          </div>
        </div>
        <div
          class="col-12 field p-0 text-lg font-bold"
          v-if="listFilesS.length > 0"
        >
          <Toolbar class="p-0 surface-0 border-none">
            <template #start>
              <div class="text-lg font-bold">Files đính kèm</div>
            </template>
            <template #end> </template>
          </Toolbar>
        </div>

        <div class="col-12 p-0 px-8">
          <div
            class="p-0 field w-full"
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
                    <div>
                      <a
                        :href="basedomainURL + item.file_path"
                        download
                        class="w-full no-underline text-900 align-items-center"
                      >
                        <span class="ml-2 text-900" style="line-height: 50px">
                          {{ item.file_name }}</span
                        >
                      </a>
                    </div>
                  </div>
                </template>
                <template #end>
                  <!-- <Button
                    icon="pi pi-times"
                    class="p-button-rounded p-button-danger"
                    @click="deleteFileH(item)"
                  /> -->
                </template>
              </Toolbar>
            </div>
          </div>
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        @click="closeDetailsRepair"
        label="Đóng"
        icon="pi pi-times"
        autofocus
      />
    </template>
  </Dialog>

  <Dialog
    header="Phiếu kiểm kê"
    v-model:visible="displayDetailsInventory"
    :maximizable="true"
    :style="{ width: '70vw' }"
    :modal="true"
  >
    <form v-if="displayDetailsInventory">
      <detailsInventory :device_inventory_id="device_inventory_id" />
    </form>
    <template #footer>
      <Button
        @click="closeDetailsInventory"
        label="Đóng"
        icon="pi pi-times"
        autofocus
      />
    </template>
  </Dialog>
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
  height: calc(100vh - 185px);
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
    