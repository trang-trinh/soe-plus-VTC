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
  id: "card_id", department_use_name_fake: null,
  totalRecordsExport:50,
  pagenoExport:1,
 filterMonth:new Date()
});
const datalists = ref();
const isDynamicSQL = ref(false);
const loadDataSQL = () => {
  let data = {
    id: options.value.id,
    next: options.value.IsNext,
    sqlO: options.value.sort,
    Search: options.value.search,
    PageNo: options.value.pageno,
    PageSize: options.value.pagesize,
    fieldSQLS: filterSQL.value,
    Date:(   options.value.filterMonth.getMonth() +1)+"/"+  options.value.filterMonth.getDate() +"/"+   options.value.filterMonth.getFullYear() ,
  };
  options.value.loading = true;
  axios
    .post(baseURL + "/api/SQL/Filter_device_report_insurance", data, config)
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
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
        proc: "device_report_insurance",
        par: [
           { par: "filterMonth", va: options.value.filterMonth },
          { par: "pageno", va: options.value.pageno },
          { par: "pagesize", va: options.value.pagesize },
          { par: "user_id", va: store.state.user.user_id }
        ],
      }),
            SecretKey,
            cryoptojs
          ).toString(),
        },config
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
     
      options.value.loading = false;
   
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
    
      options.value.loading = false;
  
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
 
const displaySidebarDR = ref(false);
const liUserRecever = ref([]);
 
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
      font-size: 10pt;
    }
    .title1,
    .title1 * {
      font-size: 16pt !important;
    }
    .title2,
    .title2 * {
      font-size: 14pt !important;
    }
    .title3,
    .title3 * {
      font-size: 12pt !important;
    }
    .boder tr th,
    .boder tr td {
      border: 1px solid #999999 !important;
      padding: 0.25rem;
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
            <td style="width:33.33%">
          
 
          <div style="width:100%; align-item:center; font-weight:600">Tổng số: `+  datalistsExport.value.length+` </div>
          
     
              </td>
            <td    style="width:33.33%;padding: 0 0 0.5rem 0 ;text-align:center; " >
            
               <div class="title2" style="width:100%;font-weight:600;height:100%;    padding-top:0">BÁO CÁO TRANG THIẾT BỊ ĐẾN THỜI KỲ BẢO HÀNH, BẢO TRÌ</div> 
             
          <div></div>
            </td>
            <td style="width:33.33%">
              <div  style="width:100%; text-align:right; align-item:center; font-weight:600"> Ngày in: `+moment(new Date()).format("DD/MM/YYYY")+` </div>
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
           
            <th style="min-width: 150px">Tình trạng</th>
            <th style="width: 100px">Trạng thái</th>
            <th style="width: 110px">Người dùng</th>
        
          </tr>
        </thead>
        <tbody class="boder">`;
  for (let index = 0; index < datalistsExport.value.length; index++) {
    const value = datalistsExport.value[index];
    
if(value.is_receiver_department)
value.device_user_name=value.department_use_name;   
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
    Date:  options.value.filterMonth,
  };
  options.value.loading = true;
  axios
    .post(baseURL + "/api/SQL/Filter_device_report_insurance", data, config)
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
        excelname: "BÁO CÁO THIẾT BỊ ĐẾN THỜI KỲ BẢO HÀNH, BẢO TRÌ",
        proc: "device_report_insurance_export",
        par: [
          { par: "user_id", va: store.state.user.user_id },
          { par: "search", va: options.value.search },
          {
            par: "status",
            va: options.value.status ? options.value.status.toString() : null,
          },
          {
            par: "device_use",
            va: options.value.device_user
              ? options.value.device_user.toString()
              : null,
          },
          {
            par: "manufacture",
            va: options.value.manufacture
              ? options.value.manufacture.toString()
              : null,
          },
          {
            par: "device_groups_id",
            va: options.value.device_groups_id
              ? options.value.device_groups_id.toString()
              : null,
          },
          {
            par: "device_unit_id",
            va: options.value.device_unit_id
              ? options.value.device_unit_id.toString()
              : null,
          },
          {
            par: "device_id",
            va: options.value.device_id
              ? options.value.device_id.toString()
              : null,
          },
          {
            par: "department_use_name",
            va: options.value.department_use_name_fake,
          },
          {
            par: "device_type_id",
            va: options.value.device_type_id
              ? options.value.device_type_id.toString()
              : null,
          },
          {
            par: "provider_id",
            va: options.value.provider_id
              ? options.value.provider_id.toString()
              : null,
          },
          {
            par: "warehouse_id",
            va: options.value.warehouse_id
              ? options.value.warehouse_id.toString()
              : null,
          },
          { par: "filterMonth", va: options.value.filterMonth },
          { par: "start_date", va: options.value.start_date },
          { par: "end_date", va: options.value.end_date },
          { par: "sort", va: options.value.sort },
          { par: "pageno", va: options.value.pageno },
          { par: "pagesize", va: options.value.totalRecordsExport },
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
      options.value.status != null ||	
      options.value.device_user != null ||	
      options.value.device_id != null ||	
      options.value.provider_id != null ||	
      options.value.department_use_name != null || 	
      options.value.device_unit_id!= null ||	
      options.value.device_type_id!= null ||	
      options.value.device_groups_id != null ||	
      options.value.manufacture != null ||	
      options.value.warehouse_id != null
    )
  )
    checkFilter.value = false;
};
 
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
    
      options.value.loading = false;
 
    });
};
const showFilter = ref(false);
const reFilterCard = () => {
  checkFilter.value = false;
  options.value.device_user = null;	
  options.value.device_id = null ;	
      options.value.provider_id  = null ;	
  options.value.device_type_id= null;	
  options.value.department_use_name=null;	
  options.value.warehouse_id=null;	
  options.value.status = null;	
  options.value.device_unit_id= null ;	
  options.value.device_groups_id= null;	
  options.value.manufacture= null;	
  taskDateFilter.value = null;	
  options.value.is_hot = null;	
  options.value.news_type = null;	
  options.value.status = null;	
  filterCard(false);	 
};
const listDepartmentF = ref([]);
const filterCard = (check) => {	
  if (check) checkFilter.value = true;	
  filterSQL.value = [];	
    	
  if (options.value.department_use_name ) {	
    if (Object.keys(options.value.department_use_name).length > 0) {
      options.value.department_use_name_fake = "";
      let filterS1 = {
        filterconstraints: [],
        filteroperator: "or",
        key: "tm.department_use_name",
      };
      var srcs = "";

      for (const key in options.value.department_use_name) {
        var obi = "";
        if (listDepartmentF.value.find((x) => x.code == key))
          obi = listDepartmentF.value.find((x) => x.code == key).name;

        options.value.department_use_name_fake += srcs + key;
        srcs = ",";
        var addr = { value: obi, matchMode: "equals" };
        filterS1.filterconstraints.push(addr);
      }
      filterSQL.value.push(filterS1);
    }
}	
  if (options.value.status ) {	
  	
    let filterS1 = {	
      filterconstraints: [],	
      filteroperator: "or",	
      key: "tm.status",	
    };	
    if (options.value.status.length > 0) {	
      options.value.status.forEach((element) => {	
        var addr = { value: element, matchMode: "equals" };	
        filterS1.filterconstraints.push(addr);	
      });	
      filterSQL.value.push(filterS1);	
    }	
  }	
  if (options.value.device_user ) {	
    let filterS1 = {	
      filterconstraints: [],	
      filteroperator: "or",	
      key: "tm.device_user_id",	
    };	
    if (options.value.device_user.length > 0) {	
      options.value.device_user.forEach((element) => {	
        var addr = { value: element, matchMode: "equals" };	
        filterS1.filterconstraints.push(addr);	
      });	
      filterSQL.value.push(filterS1);	
    }	
  }	
  if (options.value.provider_id ) {	
    let filterS1 = {	
      filterconstraints: [],	
      filteroperator: "or",	
      key: "tm.warranty_company",	
    };	
    if (options.value.provider_id.length > 0) {	
      options.value.provider_id.forEach((element) => {	
        var addr = { value: element, matchMode: "equals" };	
        filterS1.filterconstraints.push(addr);	
      });	
      filterSQL.value.push(filterS1);	
    }	
  }	
  if (options.value.device_id ) {	
    let filterS1 = {	
      filterconstraints: [],	
      filteroperator: "or",	
      key: "tm.device_id",	
    };	
    if (options.value.device_id.length > 0) {	
      options.value.device_id.forEach((element) => {	
        var addr = { value: element, matchMode: "equals" };	
        filterS1.filterconstraints.push(addr);	
      });	
      filterSQL.value.push(filterS1);	
    }	
  }	
  if (options.value.device_type_id ) {	
    let filterS1 = {	
      filterconstraints: [],	
      filteroperator: "or",	
      key: "tm.device_type_id",	
    };	
    if (options.value.device_type_id.length > 0) {	
      options.value.device_type_id.forEach((element) => {	
        var addr = { value: element, matchMode: "equals" };	
        filterS1.filterconstraints.push(addr);	
      });	
      filterSQL.value.push(filterS1);	
    }	
  }	
   	
  if (options.value.device_unit_id ) {	
    let filterS1 = {	
      filterconstraints: [],	
      filteroperator: "or",	
      key: "tm.device_unit_id",	
    };	
    if (options.value.device_unit_id.length > 0) {	
      options.value.device_unit_id.forEach((element) => {	
        var addr = { value: element, matchMode: "equals" };	
        filterS1.filterconstraints.push(addr);	
      });	
      filterSQL.value.push(filterS1);	
    }	
  }	
  if (options.value.device_groups_id ) {	
    let filterS1 = {	
      filterconstraints: [],	
      filteroperator: "or",	
      key: "tm.device_groups_id",	
    };	
    if (options.value.device_groups_id.length > 0) {	
      options.value.device_groups_id.forEach((element) => {	
        var addr = { value: element, matchMode: "equals" };	
        filterS1.filterconstraints.push(addr);	
      });	
      filterSQL.value.push(filterS1);	
    }	
  }	
  if (options.value.manufacture ) {	
    let filterS1 = {	
      filterconstraints: [],	
      filteroperator: "or",	
      key: "tm.producer",	
    };	
    if (options.value.manufacture.length > 0) {	
      options.value.manufacture.forEach((element) => {	
        var addr = { value: element, matchMode: "equals" };	
        filterS1.filterconstraints.push(addr);	
      });	
      filterSQL.value.push(filterS1);	
    }	
  }	
    	
  if (options.value.warehouse_id ) {	
    let filterS1 = {	
      filterconstraints: [],	
      filteroperator: "or",	
      key: "tm.warehouse_id",	
    };	
    if (options.value.warehouse_id.length > 0) {	
      options.value.warehouse_id.forEach((element) => {	
        var addr = { value: element, matchMode: "equals" };	
        filterS1.filterconstraints.push(addr);	
      });	
      filterSQL.value.push(filterS1);	
    }	
  }	
   	
     	
  isDynamicSQL.value = true;	
  loadData(true);	
  filterButs.value.hide()	
}; 
const listDevice=ref([]);
//Tìm kiếm
 
const first = ref(0);
const refreshData = () => {
  options.value.search = "";
  options.value.status = null;
  options.value.device_type_id = null;	
  options.value.department_use_name = null;	
  options.value.device_id  = null;	
  options.value.provider_id  = null;	
  options.value.device_user = null;	
  options.value.manufacture = null;	
  options.value.device_groups_id = null;	
  	
  options.value.device_unit_id = null;	
  	
  options.value.status = null;	
  options.value.warehouse_id = null;
  options.value.start_date = null;
  options.value.end_date = null;
  taskDateFilter.value = [];
  checkFilter.value = false;
 
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



const listDepartment = ref();
const loadOrganization = (value) => {
  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_department_list",
            par: [
              { par: "user_id", va: store.state.user.user_id },
              { par: "department_id", va: value },
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
        var arrr = [...data];
        arrr.forEach((element) => {
          listDepartmentF.value.push({
            name: element.organization_name,
            code: element.organization_id,
          });
        });
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

 
    });
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
const listManufact=ref([]);
const listDeviceGroups=ref([]);
 
 
const listProvider = ref([]);	
const listUnitList = ref([]);	
const initTudien = () => {
  listUnitList.value = [];	
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
        listUnitList.value.push({	
          name: element.device_unit_name,	
          code: element.device_unit_id,	
          	
        });	
      });	
    })	
    .catch((error) => {	
      options.value.loading = false;	
 	
    });	
  listProvider.value = [];	
  axios	
    .post(	
      baseURL + "/api/device_card/getData",	
      {	
        str: encr(	
          JSON.stringify({	
            proc: "device_provider_list",	
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
        listProvider.value.push({	
          name: element.provider_name,	
          code: element.provider_id,	
          address:element.address,	
          full_name:element.full_name,	
          phone_number:element.phone_number	
        });	
      });	
    })	
    .catch((error) => {	
      options.value.loading = false;	
 	
    });	
  listDevice.value = [];	
  axios	
    .post(	
      baseURL + "/api/device_card/getData",	
      {	
        str: encr(	
          JSON.stringify({	
            proc: "device_main_list",	
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
        listDevice.value.push({	
          name: element.device_name,	
          code: element.device_id,	
        });	
      });	
    })	
    .catch((error) => {	
      options.value.loading = false;	
 	
    });	
  axios	
    .post(	
      baseURL + "/api/device_card/getData",	
      {	
        str: encr(	
          JSON.stringify({	
            proc: "device_groups_list",	
            par: [	
              { par: "search", va: null },	
              { par: "pageno", va: 0 },	
              { par: "pagesize", va: 100000 },	
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
        listDeviceGroups.value.push({	
          name: element.groups_name,	
          code: element.device_groups_id,	
        });	
      });	
    })	
    .catch((error) => {	
      options.value.loading = false;	
    });	
  axios	
    .post(	
      baseURL + "/api/device_card/getData",	
      {	
        str: encr(	
          JSON.stringify({	
            proc: "device_manufacturer_list",	
            par: [	
              { par: "pageno", va: 0 },	
              { par: "pagesize", va: 100000 },	
              { par: "user_id", va: store.getters.user.user_id },	
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
        listManufact.value.push({	
          name: element.device_manufacturer_name,	
          code: element.device_manufacturer_id,	
        });	
      });	
    })	
    .catch((error) => {	
      options.value.loading = false;	
    });
  axios
  .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
          proc: "device_groups_list",
          par: [
            { par: "search", va: null },
            { par: "pageno", va: 0 },
            { par: "pagesize", va: 100000 },
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
        listDeviceGroups.value.push({
          name: element.groups_name,
          code: element.device_groups_id,
        });
      });
    })
    .catch((error) => {
      options.value.loading = false;
 
    });
   

 
};
onMounted(() => {
     if (!checkURL(window.location.pathname, store.getters.listModule)) {
     //router.back();
  }
  loadStatusDevice();
  initTudien();
  loadUser();
  loadWareHouse(); loadOrganization(store.getters.user.organization_id);
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
       thiết bị đến kì bảo hành ({{
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
                    (options.status != null ||	
                    options.device_user != null ||	
                    options.device_type_id != null ||	
                    options.department_use_name != null ||	
                   	
                    options.device_id != null ||	
                    options.provider_id != null ||	
                      options.device_groups_id != null ||	
                      options.device_unit_id != null ||	
                      	
                      options.manufacture!= null ||	
                      options.warehouse_id     != null) &&	
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
                  style="width: 700px"
                  :breakpoints="{ '960px': '20vw' }"
                >
                  <div class="grid formgrid m-0">
                    <div class="col-12 p-0 flex">
                      <div class="p-0 col-6 md:col-6">
                        <div class="col-12 md:col-12">
                          <div class="col-12 p-0 pb-2 align-items-center flex">
                            Trạng thái:
                          </div>

                          <MultiSelect
                            :style="
                              options.status != null
                                ? 'border:2px solid #2196f3'
                                : ''
                            "
                            :options="listSCard"
                            :filter="true"
                            :showClear="true"
                            :editable="false"
                            v-model="options.status"
                            optionLabel="name"
                            optionValue="code"
                            placeholder="Chọn trạng thái"
                            class="col-12 p-0"
                            display="chip"
                            panelClass="d-design-dropdown"
                          >
                          </MultiSelect>
                        </div>
                        <div class="col-12 md:col-12">
                          <div class="col-12 p-0 py-2 align-items-center flex">
                            Người sử dụng:
                          </div>

                          <MultiSelect
                            :style="
                              options.device_user != null
                                ? 'border:2px solid #2196f3'
                                : ''
                            "
                            :options="listDropdownUser"
                            :filter="true"
                            :showClear="true"
                            :editable="false"
                            v-model="options.device_user"
                            optionLabel="name"
                            optionValue="code"
                            placeholder="Chọn người sử dụng"
                            class="col-12 p-0"   display="chip"
                            panelClass="d-design-dropdown"
                          >
                            <template #option="slotProps">
                              <div class="country-item flex align-items-center">
                                <div class="grid w-full p-0">
                                  <div
                                    class="field p-0 py-1 col-12 flex m-0 cursor-pointer align-items-center"
                                  >
                                    <div
                                      class="col-1 mx-2 p-0 align-items-center"
                                    >
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
                                          basedomainURL +
                                          slotProps.option.avatar
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
                                    <div
                                      class="col-11 p-0 pl-3 align-items-center"
                                    >
                                      <div class="pt-2">
                                        <div class="font-bold">
                                          {{ slotProps.option.name }}
                                        </div>
                                        <div
                                          class="flex w-full text-sm font-italic text-500"
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
                        <div class="col-12 md:col-12">
                          <div class="col-12 py-2 p-0 align-items-center flex">
                            Hãng thiết bị:
                          </div>

                          <MultiSelect
                            :style="
                              options.manufacture != null
                                ? 'border:2px solid #2196f3'
                                : ''
                            "
                            :options="listManufact"
                            :filter="true"
                            :showClear="true"
                            :editable="false"
                            v-model="options.manufacture"
                            optionLabel="name"
                            optionValue="name"
                            placeholder="Chọn hãng thiết bị"   display="chip"
                            class="col-12 p-0"
                            panelClass="d-design-dropdown"
                          >
                          </MultiSelect>
                        </div>
                        <div class="col-12 md:col-12">
                          <div class="col-12 p-0 py-2 align-items-center flex">
                            Nhóm thiết bị:
                          </div>

                          <MultiSelect
                            :style="
                              options.device_groups_id != null
                                ? 'border:2px solid #2196f3'
                                : ''
                            "
                            :options="listDeviceGroups"
                            :filter="true"
                            :showClear="true"
                            :editable="false"   display="chip"
                            v-model="options.device_groups_id"
                            optionLabel="name"
                            optionValue="code"
                            placeholder="Chọn nhóm thiết bị"
                            class="col-12 p-0"
                            panelClass="d-design-dropdown"
                          >
                          </MultiSelect>
                        </div>
                        <div class="col-12 md:col-12">
                          <div class="col-12 p-0 py-2 align-items-center flex">
                            Đơn vị tính:
                          </div>

                          <MultiSelect
                            :style="
                              options.device_unit_id != null
                                ? 'border:2px solid #2196f3'
                                : ''
                            "
                            :options="listUnitList"   display="chip"
                            :filter="true"
                            :showClear="true"
                            :editable="false"
                            v-model="options.device_unit_id"
                            optionLabel="name"
                            optionValue="code"
                            placeholder="Chọn đơn vị tính"
                            class="col-12 p-0"
                            panelClass="d-design-dropdown"
                          >
                          </MultiSelect>
                        </div>
                      </div>
                      <div class="p-0 col-6 md:col-6">
                        <div class="col-12 md:col-12">
                          <div class="col-12 p-0 pb-2 align-items-center flex">
                            Thiết bị:
                          </div>

                          <MultiSelect
                            :style="
                              options.device_id != null
                                ? 'border:2px solid #2196f3'
                                : ''
                            "
                            :options="listDevice"
                            :filter="true"   display="chip"
                            :showClear="true"
                            :editable="false"
                            v-model="options.device_id"
                            optionLabel="name"
                            optionValue="code"
                            placeholder="Chọn loại thiết bị"
                            class="col-12 p-0"
                            panelClass="d-design-dropdown"
                          >
                          </MultiSelect>
                        </div>
                        <div class="col-12 md:col-12">
                          <div class="col-12 p-0 py-2 align-items-center flex">
                            Phòng ban sử dụng:
                          </div>
                          <TreeSelect
                          
                          :style="
                              options.department_use_name != null
                                ? 'border:2px solid #2196f3'
                                : ''
                            " class="w-full" v-model="options.department_use_name"
                          :options="listDepartment" display="chip" selectionMode="checkbox"
                          placeholder="Chọn phòng sử dụng" :clear="true"></TreeSelect>
                          <!-- <MultiSelect
                            :style="
                              options.warehouse_id != null
                                ? 'border:2px solid #2196f3'
                                : ''
                            "
                            :options="listWarehouse"   display="chip"
                            :filter="true"
                            :showClear="true"
                            :editable="false"
                            v-model="options.warehouse_id"
                            optionLabel="name"
                            optionValue="code"
                            placeholder="Chọn kho thiết bị"
                            class="col-12 p-0"
                            panelClass="d-design-dropdown"
                          >
                          </MultiSelect> -->
                        </div>
                        <div class="col-12 md:col-12">
                          <div class="col-12 p-0 py-2 align-items-center flex">
                            Loại thiết bị:
                          </div>

                          <MultiSelect
                            :style="
                              options.device_type_id != null
                                ? 'border:2px solid #2196f3'
                                : ''
                            "
                            :options="listType"
                            :filter="true"   display="chip"
                            :showClear="true"
                            :editable="false"
                            v-model="options.device_type_id"
                            optionLabel="name"
                            optionValue="code"
                            placeholder="Chọn loại thiết bị"
                            class="col-12 p-0"
                            panelClass="d-design-dropdown"
                          >
                          </MultiSelect>
                        </div>
                        <div class="col-12 md:col-12">
                          <div class="col-12 p-0 py-2 align-items-center flex">
                            Nhà cung cấp:
                          </div>

                          <MultiSelect
                            :style="
                              options.provider_id != null
                                ? 'border:2px solid #2196f3'
                                : ''
                            "
                            :options="listProvider"
                            :filter="true"
                            :showClear="true"
                            :editable="false"
                            v-model="options.provider_id"
                            optionLabel="name"   display="chip"
                            optionValue="name"
                            placeholder="Chọn nhà cung cấp"
                            class="col-12 p-0"
                            panelClass="d-design-dropdown"
                          >
                          </MultiSelect>
                        </div>
                        <div class="col-12 md:col-12">
                          <div class="col-12 p-0 py-2 align-items-center flex">
                            Kho:
                          </div>

                          <MultiSelect
                            :style="
                              options.warehouse_id != null
                                ? 'border:2px solid #2196f3'
                                : ''
                            "
                            :options="listWarehouse"   display="chip"
                            :filter="true"
                            :showClear="true"
                            :editable="false"
                            v-model="options.warehouse_id"
                            optionLabel="name"
                            optionValue="code"
                            placeholder="Chọn kho thiết bị"
                            class="col-12 p-0"
                            panelClass="d-design-dropdown"
                          >
                          </MultiSelect>
                        </div>
                      </div>
                    </div>
                    <div class="col-12 field p-0 m-0">
                      <Toolbar class="toolbar-filter px-2">
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
             
                <Calendar v-tooltip.top="'Lọc theo tháng bảo hành'" 
                 @date-select ="loadData(true)" 
                    class="mx-2"    placeholder="Tháng bảo hành" 
                      :showIcon="true" inputId="monthpicker"
                       v-model="options.filterMonth" view="month" dateFormat="mm/yy"  />
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
          field="insurance_cycle"
          header="Chu kì bảo hành"
        >
             <template #body="data">
            <div>
                 {{       data.data.insurance_cycle?data.data.insurance_cycle + ' Tháng':''  }}
            </div>
          </template>
        </Column>
        <Column
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;max-width:150px;height:50px"
          bodyStyle="text-align:center;max-width:150px;overflow: hidden;"
          field="price"
          header="Thời gian bảo hành"
        >
        <template #body="data">
            <div>
                 {{       data.data.insurance_month?data.data.insurance_month + ' Tháng':''  }}
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
                class="textonelinec w-full surface-200 justify-content-center p-button-status-d"
              />
              <Chip
                v-else-if="data.data.status == 'CXN'"
                :label="data.data.device_status_name"
                v-tooltip.top="data.data.device_status_name"
                class="textonelinec w-full bg-pink-300 justify-content-center p-button-status-d"
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
                class="textonelinec w-full bg-yellow-300 justify-content-center p-button-status-d"
              />
              <Chip
                v-else-if="data.data.is_recall == true"
                label="Đã thu hồi"
                v-tooltip.top="data.data.device_status_name"
                class="textonelinec w-full bg-bluegray-300 justify-content-center p-button-status-d"
              />
              <Chip
                v-else-if="data.data.status == 'TK'"
                :label="data.data.device_status_name"
                v-tooltip.top="data.data.device_status_name"
                class="textonelinec w-full bg-green-300 justify-content-center p-button-status-d"
              />
              <Chip
                v-else-if="data.data.status == 'DSC'"
                :label="data.data.device_status_name"
                v-tooltip.top="data.data.device_status_name"
                class="textonelinec w-full bg-orange-300 justify-content-center p-button-status-d"
              />
              <Chip
                v-else-if="data.data.status == 'DSD'"
                :label="data.data.device_status_name"
                v-tooltip.top="data.data.device_status_name"
                class="textonelinec w-full bg-blue-300 justify-content-center p-button-status-d"
              />
              <Chip
                v-else-if="data.data.status == 'HKS'"
                :label="data.data.device_status_name"
                v-tooltip.top="data.data.device_status_name"
                class="textonelinec w-full bg-purple-300 justify-content-center p-button-status-d"
              />
              <Chip
                v-else-if="data.data.status == 'TPTH'"
                :label="data.data.device_status_name"
                v-tooltip.top="data.data.device_status_name"
                class="textonelinec w-full bg-purple-300 justify-content-center p-button-status-d"
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
          ><template #body="data">
            <div>
              <span v-if="data.data.is_receiver_department">
                {{ data.data.department_use_name }}
              </span>
              <span v-else>{{ data.data.device_user_name }}</span>
            </div>
          </template>
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
}.toolbar-filter {
  border: unset;
  outline: unset;
  background-color: #fff;
  padding-bottom: 0px;
}

</style>