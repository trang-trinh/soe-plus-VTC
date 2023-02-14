<script setup>
import { ref, inject, onMounted } from "vue";
import { useToast } from "vue-toastification";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import moment from "moment";
import detailsDevice from "../../../components/device/detailsDevice.vue";
import { encr, checkURL } from "../../../util/function.js";
//Khai báo
const router = inject("router");
const cryoptojs = inject("cryptojs");
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const basedomainURL = baseURL;
const selectedCard = ref([]);
const checkDelList = ref(false);
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const toast = useToast();
const options = ref({
  IsNext: true,
  sort: "card_id DESC",
  sortDM: null,
  search: "",
  pageno: 0,
  pagesize: 50,
  pagenoDM: 0,
  pagesizeDM: 10,
  loading: true,
  totalRecords: null,
  totalRecordsDM: null,
  start_date: null,
  end_date: null,
  next: true,
  filterOrg: [],
  id: "card_id",
  totalRecordsExport:50,
  pagenoExport:1
});

const datalists = ref();
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
    .post(baseURL + "/api/SQL/Filter_device_report_expiration", data, config)
    .then((response) => {
      let dt = JSON.parse(response.data.data);

      let data = dt[0];
      if (data.length > 0) {
        data.forEach((element, i) => {
          element.is_order =
            (options.value.pageno) * options.value.pagesize + i + 1;
        });

        datalists.value = data;
      } else {
        datalists.value = [];
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
      console.log(error);
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const loadData = () => {
  if (isDynamicSQL.value) {
    loadDataSQL();
    return false;
  }
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "device_report_expiration",
        par: [
          { par: "pageno", va: options.value.pageno },
          { par: "pagesize", va: options.value.pagesize },
          { par: "user_id", va: store.state.user.user_id }
        ],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      let totalRecords = JSON.parse(response.data.data)[1];
      options.value.totalRecords = totalRecords[0].dmc;
      if (data.length > 0) {
        data.forEach((element, i) => {
          element.is_order =
            (options.value.pageno) * options.value.pagesize + i + 1;
        });

        datalists.value = data;
      } else {
        datalists.value = [];
      }

      options.value.loading = false;
    })
    .catch((error) => {
      console.log(error);
     
    });
};
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
const displayDetails = ref(false);
const openDetails = (data) => {
  axios
    .post(
      baseURL + "/api/device_card/getData",
        {
          str: encr(
            JSON.stringify({
        proc: "device_card_get",
        par: [{ par: "card_id", va: data.card_id }],
      }),
            SecretKey,
            cryoptojs
          ).toString(),
        },config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        devicecard.value = data[0];
      } else {
        devicecard.value = null;
      }

      displayDetails.value = true;
      options.value.loading = false;
    })
    .catch((error) => {
      console.log(error);
    });
};
const closeDetails = () => {
  displayDetails.value = false;
  devicecard.value = {};
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
        },config
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
      console.log("err", error);
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
        },config
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
      console.log("err", error);
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
        },config
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
      console.log("err", error);
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
          { par: "organization_id", va: store.getters.user.organization_id },
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
        },config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      data.forEach((element, i) => {
        listDropdownUser.value.push({
          name: element.full_name,
          code: element.user_id,
          avatar: element.avatar,
          department_name: element.department_name,
          role_name: element.role_name,position_name:element.position_name,
        });
      });
      listUsers.value = data;

      listDropdownUserCheck.value = listDropdownUser.value;
    })
    .catch((error) => {
      console.log(error);

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

//Tìm kiếm
const searchReceive = () => {
  options.value.loading = true;
  loadDataSQL();
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

  return { arrChils: arrChils, arrtreeChils: arrtreeChils };
};
  
const selectedColumns = ref();
const columns = ref([
  { field: "issue_place", header: "Nơi gửi" },
  { field: "doc_group", header: "Nhóm văn bản" },
  { field: "ldt", header: "Lãnh đạo" },
]);
const isCheckTable = ref(true);
const onToggle = (val) => {
  selectedColumns.value = columns.value.filter((col) => val.includes(col));

  if (selectedColumns.value.length == 0) {
    isCheckTable.value = true;
  } else {
    isCheckTable.value = false;
  }
};
const listFilterDM = ref({
  ca_groups_list: [],
  ca_issue_place_list: [],
  ca_fields_list: [],
  ca_dispatch_book_list: [],
});
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

const displaySidebarDR = ref(false);
const liUserRecever = ref([]);
const showDetailsRecever = (value) => {
  liUserRecever.value = value;
  displaySidebarDR.value = true;
};
const headerExport=ref('Cấu hình xuất Excel');
const showExport = ref(false);
// IN

const print = () => {

  var htmltable = "";
  htmltable = renderhtml("formprint", htmltable);
  var printframe = window.frames["printframe"];
  printframe.document.write(htmltable);
  setTimeout(function () {
    printframe.print();
    printframe.document.close();
  }, 0);
};

function renderhtml(id, htmltable) {
  htmltable = "";
  //Style
  htmltable += `<style>
    #formprint {
      background: #fff !important;
    }
    #formprint * {
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
    td{
      word-break: break-word;
    }
    tfoot {
      display: table-footer-group !important;
    }
    .uppercase,
    .uppercase * {
      text-transform: uppercase !important;
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
  </style>
  
  
  `;
  htmltable+=`<div id="formprint">
      <table>
        <thead>
          <tr>
            <td class="text-center" colspan="6">
              <div style="padding: 1rem 0">
                <div class="uppercase title2"><b>BÁO CÁO TRANG THIẾT BỊ HẾT NIÊN HẠN</b></div>
             
              </div>
            </td>
          </tr>
        </thead>
      </table>
      <table>
        <thead class="boder">
          <tr>
            <th style="width: 30px">TT</th>
            <th style="width: 150px">Số hiệu</th>
            <th style="width: 100px">Mã barcode</th>
            <th style="min-width: 150px">Tên thiết bị</th>
            <th style="width: 100px">Ngày mua</th>
            <th style="width: 150px">Nguyên giá</th>
            <th style="min-width: 150px">Tình trạng</th>
            <th style="width: 100px">Trạng thái</th>
            <th style="width: 110px">Người dùng</th>
        
          </tr>
        </thead>
        <tbody class="boder">`;
  for (let index = 0; index < datalistsExport.value.length; index++) {
    const value = datalistsExport.value[index];
    htmltable+=`
          <tr >
            <td align="center">
              <div>` + (index + 1)+ `</div>
            </td>
            <td  style="width: 150px">
              <div >
                ` +value.device_number + `
              </div>
            </td>
            <td  style="width: 100px">
              <div >
                ` +value.barcode_id + `
              </div>
            </td>
            <td  style="min-width: 150px">
              <div >
                ` +value.device_name + `
              </div>
            </td>
            <td  style="width: 150px">
              <div >
                ` + moment(new Date(value.purchase_date)).format("DD/MM/YYYY") + `
              
              </div>
            </td>
            <td align="center"  style="width: 100px">
              <div>
                
                ` +value.price.toLocaleString()+ `VND </div>
            </td>
            <td  style="min-width: 150px">
              <div >
                ` +value.assets_condition + `
              </div>
            </td>
            <td  style="width: 100px">
              <div >
                ` +value.device_status_name + `
              </div>
            </td>
            <td  style="width: 110px">
              <div >
                ` +value.device_user_name + `
              </div>
            </td>
          </tr>`
  }
  htmltable+=`
        </tbody>
      
      </table>
    </div>`
  // var html = document.getElementById(id);
  // if (html) {
  //   htmltable += html.innerHTML;
  // }
  return htmltable;
}
const datalistsExport=ref();
var checkTypeExpport=false;

//Xuất excel
const menuButs = ref();
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
      console.log(error);
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
     if(options.value.totalRecords>50){

  
       headerExport.value='Cấu hình in báo cáo';
      options.value.totalRecordsExport=50;
      checkTypeExpport = false;
       showExport.value = true;
     }
     else{
      exportExcelR();
     }
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
        excelname: "BÁO CÁO THIẾT BỊ HẾT NIÊN HẠN",
        proc: "device_report_expiration_export",
        par: [
        { par: "user_id", va: store.state.user.user_id },
        { par: "search", va: options.value.search },
        { par: "status", va: options.value.status },
        { par: "device_user_id", va: options.value.device_user_id },
        { par: "device_type_id", va: options.value.device_type_id },
        { par: "warehouse_id", va: options.value.warehouse_id },
        { par: "start_date", va: options.value.start_date },
          { par: "end_date", va: options.value.end_date },
          { par: "sort", va: options.value.sort },
          { par: "pageno", va: options.value.pageno },
          { par: "pagesize", va: options.value.totalRecordsExport},
         
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
  device_number: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
  barcode_id: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
  
  device_name: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.DATE_IS }],
  },
  
});
const filterSQL = ref();
const onPage = (event) => {
  if (event.rows != options.value.pagesize) {
    options.value.pagesize = event.rows;
  }
  if (event.page == 0) {
    //Trang đầu
    options.value.id = null;
    options.value.IsNext = true;
  } else if (event.page > options.value.pageno +1) {
    //Trang cuối
    options.value.id = -1;
    options.value.IsNext = false;
  } else if (event.page > (options.value.pageno
  )) {
    //Trang sau

    options.value.id =
      datalists.value[datalists.value.length - 1].card_id;
    options.value.IsNext = true;
  } else if (event.page < (options.value.pageno)) {
    //Trang trước
    options.value.id = datalists.value[0].card_id;
    options.value.IsNext = false;
  }
  options.value.pagesize = event.rows;
  options.value.pageno = event.page;
  loadDataSQL();
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
    filterCardWarehouse.value!= null
    )
  )
    checkFilter.value = false;
};

const filterTrangthai = ref();
const filterCardUser = ref();
const filterCardType = ref();
const filterCardWarehouse = ref();
const listSCard = ref([]);

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
        },config
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
      console.log("err", error);
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
 
const reFilterCard = () => {
  checkFilter.value = false;
 
  filterCardUser.value = null;
  filterCardType.value = null;
  filterCardWarehouse.value = null;
  filterTrangthai.value = null;
  taskDateFilter.value = [];
  
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
   filterButs.value.hide();
};
//Tìm kiếm
const searchCard = () => {
  loadData(true);
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
  taskDateFilter.value = [];
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
onMounted(() => {
     if (!checkURL(window.location.pathname, store.getters.listModule)) {
     //router.back();
  }
  loadStatusDevice();
  
  loadUser();
  loadWareHouse();
  loadDeviceType();
  loadDeviceUnit();
  loadData();
});
</script>
<template>
  <div class="d-container">
    <div class="d-lang-table">
      <DataTable
        class="w-full p-datatable-sm e-sm p-table-custom-d"
        :lazy="true"
        @page="onPage($event)"
        @filter="onFilter($event)"
        @sort="onSort($event)"
        :value="datalists"
        :loading="options.loading"
        :paginator="options.totalRecords > options.pagesize"
        :rows="options.pagesize"
        :totalRecords="options.totalRecords"
        dataKey="card_id"
        :rowHover="true"
        :filters="filters"
        :showGridlines="true"
        :pageLinkSize="1"
        filterDisplay="menu"
        filterMode="lenient"
        paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
        :rowsPerPageOptions="[20, 30, 50, 100, 200]"
        :currentPageReportTemplate="
          isDynamicSQL ? '{currentPage}' : '{currentPage}/{totalPages}'
        "
        responsiveLayout="scroll"
        :scrollable="true"
        scrollHeight="flex"
     
      >
      <template #header>
        <div>
            <h3 class="module-title my-2 ml-1">
               <i class="pi pi-book"></i> Báo cáo
       thiết bị hết niên hạn ({{
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
                  @keyup.enter="searchReceive()"
                  type="text"
                  spellcheck="false"
                  placeholder="Tìm kiếm"
                />
                <!-- :class="checkFilter?'':'p-button-secondary'" -->
                <Button
                  :class="
                    ( filterTrangthai!= null || 
      filterCardUser!= null ||
       filterCardType!= null ||
        filterCardWarehouse!= null ) &&
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
                                    <div>{{ slotProps.option.position_name }}</div>
                                  </div>
                                  
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
                         class="col-8 p-0"
                      >
                      </Dropdown>
                    </div>
                    <div class="col-12 field p-0">
                      <Toolbar class="toolbar-filter d-toolbar">
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
                class="mr-2 p-button-outlined p-button-secondary"
                icon="pi pi-refresh"
                @click="refreshData"
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
          headerStyle="text-align:center;max-width:120px;height:50px"
          bodyStyle="text-align:center;max-width:120px;overflow: hidden;"
          field="depreciation_month"
          header="Tháng khấu hao"
        >
         
        </Column>
        <Column
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;max-width:150px;height:50px"
          bodyStyle="text-align:center;max-width:150px;overflow: hidden;"
          field="price"
          header="Nguyên giá"
        >
        <template #body="data">
            <div>
                 {{       data.data.price?data.data.price.toLocaleString():'0'  }} VND
            </div>
          </template>
        </Column>
        <!-- <Column
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;max-width:150px;height:50px"
          bodyStyle="text-align:center;max-width:150px;overflow: hidden;"
          field="curent_price"
          header="Giá trị hiện tại"
        >
        <template #body="data">
            <div>
                 {{     data.data.curent_price?data.data.curent_price.toLocaleString():'0' }} VND
            </div>
          </template>
        </Column> -->
        <Column
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;max-width:150px;height:50px"
          bodyStyle="text-align:center;max-width:150px;overflow: hidden;"
          field="assets_condition"
          header="Tình trạng"
        >
          
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
                class="w-full justify-content-center p-button-status-d textonelinec"
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
          header="Người sử dụng gần nhất"
        >
        </Column>
        <Column
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;max-width:70px;height:50px"
          bodyStyle="text-align:center;max-width:70px"
          header="Ghi chú"
        >
          <template #body="data">
            <Button
              v-tooltip.top="'Chi tiết'"
              @click="openDetails(data.data)"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-info-circle"
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
            <img src="../../../assets/background/nodata.png" height="144" />
            <h3 class="m-1">Không có dữ liệu</h3>
          </div>
        </template>
      </DataTable>
    </div>
    <iframe name="printframe" id="printframe" style="display: none"></iframe>
  </div>

  <Sidebar
    :showCloseIcon="false"
    position="right"
    v-model:visible="displaySidebarDR"
  >
    <div class="w-full format-center">
      <h3>Danh sách nơi nhận</h3>
    </div>
    <div>
      <div v-for="(item, index) in liUserRecever.split(',')" :key="index">
        <div class="py-2 align-items-center justify-content-center">
          <i class="pi pi-angle-double-right pr-1"></i>{{ item }}
        </div>
      </div>
    </div>
  </Sidebar>
   <Dialog
    :style="{ width: '20vw' }"
    :header="headerExport"
    v-model:visible="showExport"
    :modal="true"
  >
    <div class="grid">
      <div class="col-12 field flex">
        <div class="col-6 p-0">Số bản ghi:</div>
        <div class="col-6 p-0">
          <InputNumber class="w-full" v-model="options.totalRecordsExport" />
        </div>
      </div>
      <div class="col-12 field flex">
        <div class="col-6 p-0">Trang bắt đầu:</div>
        <div class="col-6 p-0">
          <InputNumber class="w-full" :min="1" :max="Math.ceil(options.totalRecords/options.totalRecordsExport)" v-model="options.pagenoExport" />
        </div>
      </div>
      <div class="col-12 p-0">
        <Toolbar class="surface-0 p-0 border-0">
          <template #end>
            <div>
              <Button label="Xuất" @click="exportExcelR"></Button>
            </div>
          </template>
        </Toolbar>
      </div>
    </div>
  </Dialog>

  <Dialog
    header="Chi tiết thẻ thiết bị"
    v-model:visible="displayDetails"
    :maximizable="true"
    :style="{ width: '80vw' }"
  >
    <div v-if="displayDetails && devicecard">
      <detailsDevice :device="devicecard" />
    </div>

    <template #footer>
      <Button @click="closeDetails" label="Đóng" icon="pi pi-times" autofocus />
    </template>
  </Dialog>
</template>
  <style scoped>
.d-lang-table {
  margin: 8px 8px 0px 8px;
  height: calc(100vh - 50px);
}
.limit-line {
  text-overflow: ellipsis;
  overflow: hidden;
  column-gap: initial;
  -webkit-line-clamp: 4;
  display: -webkit-box;
  -webkit-box-orient: vertical;
}
.limit-line-1 {
  text-overflow: ellipsis !important;
  overflow: hidden !important;
  column-gap: initial !important;
  -webkit-line-clamp: 1 !important;
  display: -webkit-box !important;
  -webkit-box-orient: vertical !important;
}
.d-toolbar {
  border: unset;
  outline: unset;
  background-color: #fff;
  margin: 0px 8px 0px 8px;
}

</style>
  