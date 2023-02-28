<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import moment from "moment";
import { encr } from "../../util/function";
import router from "@/router";
//Khai báo
const cryoptojs = inject("cryptojs");
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const basedomainURL = baseURL;
const selectedHandOver = ref();
const selectedCard = ref([]);
const checkDelList = ref(false);
const displayAssets = ref(false);
const isFirstCard = ref(false);
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const toast = useToast();
const options = ref({
  IsNext: true,
  sort: "doc_master_id DESC",
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
  id: null,
  totalRecordsExport: 50,
  pagenoExport: 1,
});

const datalists = ref();
const isDynamicSQL = ref(false);
const loadDataSQL = () => {
  let dataTeo = {
    id: "doc_master_id",
    next: options.value.IsNext,
    sqlO: options.value.sort,
    Search: options.value.search,
    PageNo: options.value.pageno,
    PageSize: options.value.pagesize,
    fieldSQLS: filterSQL.value,
  };

  options.value.loading = true;
  axios
    .post(baseURL + "/api/SQL/Filter_doc_report_list_receive", dataTeo, config)
    .then((response) => {
      let dt = JSON.parse(response.data.data);

      let data = dt[0];
      if (data.length > 0) {
        data.forEach((element, i) => {
          element.is_order = dataTeo.PageNo * options.value.pagesize + i + 1;
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
  var strG = "";
  var strk = "";

  if (options.value.department_id_process) {
    for (const key in options.value.department_id_process) {
      strG += strk + key;
      strk = ",";
    }
  }
  if(strG!=null)
  options.value.department_id_process_fake= strG;

  axios
    .post(
      baseURL + "/api/DocProc/CallProc",
      {
        str: encr(
          JSON.stringify({
            proc: "doc_report_list_receive",
            par: [
              { par: "pageno", va: options.value.pageno },
              { par: "pagesize", va: options.value.pagesize },
              { par: "user_id", va: store.state.user.user_id },
              { par: "user_recever", va: options.value.user_recever },
              { par: "dispatch_book_id", va: options.value.dispatch_book_id },
              { par: "doc_group_id", va: options.value.doc_group_id },
              { par: "field_id", va: options.value.field_id },
              { par: "department_id_process", va: options.value.department_id_process_fake },
              { par: "department_id", va: options.value.department_id },
              { par: "start_dateI", va: options.value.start_dateI },
              { par: "end_dateI", va: options.value.end_dateI },
              { par: "start_dateD", va: options.value.start_dateD },
              { par: "end_dateD", va: options.value.end_dateD },
              { par: "search", va: options.value.search },
              { par: "sort", va: options.value.sort }
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
      let totalRecords = JSON.parse(response.data.data)[1];
      options.value.totalRecords = totalRecords[0].dmc;
      if (data.length > 0) {
        data.forEach((element, i) => {
          element.is_order =
            options.value.pageno * options.value.pagesize + i + 1;
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

//Tìm kiếm
const searchReceive = () => {
  options.value.loading = true;
  if (options.value.search != null && options.value.search != "") loadDataSQL();
  else {
    loadData();
  }
};
const refreshData = () => {

  options.value.fields_id = null;
  options.value.department_id = null;
  options.value.department_id_process = null;

  options.value.ca_fields_list = null;
  options.value.ca_dispatch_book_list = null;
  options.value.end_dateI = null;
  options.value.ca_user_recever_list = null;
  options.value.ca_groups_list = null;
  options.value.dispatch_book_id = null;
  options.value.doc_group_id = null;
  options.value.start_dateI = null;
  options.value.end_dateI = null;
  options.value.start_dateD = null;
  options.value.end_dateD = null;
  options.value.search = null;
  options.value.loading = true;
  checkFilter.value = false; first.value=0;
  filters.value = {
    global: { value: null, matchMode: FilterMatchMode.CONTAINS },
    dispatch_book_num: {
      operator: FilterOperator.AND,
      constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
    },
    doc_code: {
      operator: FilterOperator.AND,
      constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
    },

    receive_date: {
      operator: FilterOperator.AND,
      constraints: [{ value: null, matchMode: FilterMatchMode.DATE_IS }],
    },
    doc_date: {
      operator: FilterOperator.AND,
      constraints: [{ value: null, matchMode: FilterMatchMode.DATE_IS }],
    },
  };
  filterSQL.value = [];
  loadData();
};
const filterButs = ref();
const toggleFilter = (event) => {
  filterButs.value.toggle(event);
};
const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);
const hideFilter = () => {
  if (
    options.value.doc_group_id == null &&

    options.value.fields_id == null &&
    options.value.dispatch_book_id == null &&
    options.value.start_dateI == null &&
    options.value.ca_groups_list == null &&
    options.value.department_id == null && options.value.department_id_process == null &&
    options.value.ca_fields_list == null &&
    options.value.ca_dispatch_book_list == null &&
    options.value.end_dateI == null &&
    options.value.ca_user_recever_list == null &&
    options.value.end_dateD == null &&
    options.value.start_dateD == null
  ) {
    checkFilter.value = false;
  }
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
const showURLSCV = (value) => {
  window.open(value, "_blank");
};
const listDepartment = ref();
const loadOrganization = () => {
  options.value.filterOrg[store.getters.user.organization_id] = true;
  axios
    .post(
      baseURL + "/api/DocProc/CallProc",
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

        listDepartment.value = obj.arrtreeChils;
      } else {
      }
    })
    .catch((error) => {
      console.log(error);
    });
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
  department_list: [],
  ca_fields_list: [],
  ca_dispatch_book_list: [],
  ca_user_recever_list: [],
});
const loadFilterDM = () => {
  listFilterDM.value.ca_user_recever_list = [];

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
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      data.forEach((element, i) => {
        listFilterDM.value.ca_user_recever_list.push({
          name: element.full_name,
          code: element.user_id,
          avatar: element.avatar,
          department_name: element.department_name,
          role_name: element.role_name, position_name: element.position_name
        });
      });
    })
    .catch((error) => {
      console.log(error);

      options.value.loading = false;
    });

  listFilterDM.value.ca_groups_list = [];
  axios
    .post(
      baseURL + "/api/DocProc/CallProc",
      {
        str: encr(
          JSON.stringify({
            proc: "ca_groups_list",
            par: [
              { par: "pageno", va: 0 },
              { par: "pagesize", va: 1000000 },
              { par: "search", va: null },
              { par: "status", va: true },
              { par: "user_id", va: store.getters.user.user_id },
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

      data.forEach((element) => {
        listFilterDM.value.ca_groups_list.push({
          name: element.doc_group_name,
          code: element.doc_group_id,
        });
      });
    })
    .catch((error) => {
      console.log(error);
    });
   
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
          store.getters.user.organization_id
        );
        listFilterDM.value.department_list = obj.arrtreeChils;
      }
    })
    .catch((error) => {
      console.log("err", error);
      options.value.loading = false;
    });
  axios
    .post(
      baseURL + "/api/DocProc/CallProc",
      {
        str: encr(
          JSON.stringify({
            proc: "ca_fields_list",
            par: [
              { par: "pageno", va: 0 },
              { par: "pagesize", va: 1000000 },
              { par: "user_id", va: store.getters.user.user_id },
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
      data.forEach((element) => {
        listFilterDM.value.ca_fields_list.push({
          name: element.field_name,
          code: element.field_id,
        });
      });
    })
    .catch((error) => {
      console.log(error);
    });
  axios
    .post(
      baseURL + "/api/DocProc/CallProc",
      {
        str: encr(
          JSON.stringify({
            proc: "ca_dispatch_book_list",
            par: [
              { par: "pageno", va: 0 },
              { par: "pagesize", va: 1000000 },
              { par: "user_id", va: store.getters.user.user_id },
              { par: "nav_type", va: 1 },
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
      data.forEach((element) => {
        listFilterDM.value.ca_dispatch_book_list.push({
          name: element.dispatch_book_name,
          code: element.dispatch_book_id,
        });
      });
    })
    .catch((error) => {
      console.log(error);
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
const checkFilter = ref(false);
const onRefilterDM = () => {
  options.value.doc_group_id = null;
  options.value.department_id = null;
  options.value.department_id_process = null;
  options.value.fields_id = null;
  options.value.dispatch_book_id = null;
  options.value.start_dateI = null;
  options.value.ca_groups_list = null;
  first.value=0;
  options.value.ca_fields_list = null;
  options.value.ca_dispatch_book_list = null;
  options.value.end_dateI = null;
  options.value.ca_user_recever_list = null;
  options.value.end_dateD = null;
  options.value.start_dateD = null;
 
  filterButs.value.hide();
  filterSQL.value = [];
  options.value.pageno=0;
  isDynamicSQL.value = false;
  checkFilter.value = false;
  options.value.loading = true;

  loadData();
};
const FilterStr=ref("");
const onFilterDM = () => {
  filterButs.value.hide();
  options.value.loading = true;
  checkFilter.value = true;
  filterSQL.value = [];
  options.value.pageno=0;
  let filterS = null;

  var strG = "";
  var strk = "";
  if (options.value.ca_groups_list)
    options.value.ca_groups_list.forEach((element) => {
      strG += strk + element.code;
      strk = ",";
    });

  if (strG != "") {
    options.value.doc_group_id = strG;
    filterS = {
      filterconstraints: [],
      filteroperator: options.value.doc_group_id,
      key: "doc_group_id",
    };
    filterSQL.value.push(filterS);
  }

  strG = "";
  strk = "";

  if (options.value.ca_dispatch_book_list)
    options.value.ca_dispatch_book_list.forEach((element) => {
      strG += strk + element.code;
      strk = ",";
    });

  if (strG != "") {
    options.value.dispatch_book_id = strG;
    filterS = {
      filterconstraints: [],
      filteroperator: options.value.dispatch_book_id,
      key: "dispatch_book_id",
    };
    filterSQL.value.push(filterS);
  }
  strG = "";
  strk = "";
  if (options.value.ca_fields_list)
    options.value.ca_fields_list.forEach((element) => {
      strG += strk + element.code;
      strk = ",";
    });

  if (strG != "") {
    options.value.fields_id = strG;
    filterS = {
      filterconstraints: [],
      filteroperator: options.value.fields_id,
      key: "field_id",
    };
    filterSQL.value.push(filterS);
  }
  strG = "";
  strk = "";

  if (options.value.department_id) {
    for (const key in options.value.department_id) {
      strG += strk + key;
      strk = ",";
    }
  }


  if (strG != "") {

    filterS = {
      filterconstraints: [],
      filteroperator: strG,
      key: "department_id",
    };
    filterSQL.value.push(filterS);
  }
  else {
    options.value.department_id = null;
  }



  strG = "";
  strk = "";
  FilterStr.value="";
  if (options.value.department_id_process) {
    for (const key in options.value.department_id_process) {
      strG += strk + key;
       
       var tsc=listFilterDM.value.department_list.find(x=>x.key==key).label;
      FilterStr.value+= strk +tsc;
      strk = ",";
    }
  }
  if (strG != "") {

    filterS = {
      filterconstraints: [],
      filteroperator: strG,
      key: "department_id_process",
    };
    filterSQL.value.push(filterS);
  }
  else {
    options.value.department_id_process = null;
  }






  strG = "";
  strk = "";
  if (options.value.ca_user_recever_list)
    options.value.ca_user_recever_list.forEach((element) => {
      strG += strk + element.code;
      strk = ",";
    });

  if (strG != "") {
    options.value.user_recever = strG;
    filterS = {
      filterconstraints: [],
      filteroperator: options.value.user_recever,
      key: "user_recever",
    };
    filterSQL.value.push(filterS);
  }


  if (options.value.start_dateI && options.value.end_dateI) {
    filterS = {
      filterconstraints: [{ value: options.value.start_dateI, matchMode: "dateAfter" }, { value: options.value.start_dateI, matchMode: "dateIs" }],
      filteroperator: "or",
      key: "receive_date",
    };
    filterSQL.value.push(filterS);

    filterS = {
      filterconstraints: [{ value: options.value.end_dateI, matchMode: "dateBefore" }, { value: options.value.end_dateI, matchMode: "dateIs" }],
      filteroperator: "or",
      key: "receive_date",
    };
    filterSQL.value.push(filterS);
  }
  else {
    if (options.value.start_dateI) {

      filterS = {
        filterconstraints: [{ value: options.value.start_dateI, matchMode: "dateIs" }],
        filteroperator: "or",
        key: "receive_date",
      };
      filterSQL.value.push(filterS);
    }
    if (options.value.end_dateI) {

      filterS = {
        filterconstraints: [  { value: options.value.end_dateI, matchMode: "dateIs" }],
        filteroperator: "or",
        key: "receive_date",
      };
      filterSQL.value.push(filterS);

    }
  }

  if (options.value.start_dateD && options.value.end_dateD) {
    filterS = {
      filterconstraints: [{ value: options.value.start_dateD, matchMode: "dateAfter" }, { value: options.value.start_dateD, matchMode: "dateIs" }],
      filteroperator: "or",
      key: "doc_date",
    };
    filterSQL.value.push(filterS);

    filterS = {
      filterconstraints: [{ value: options.value.end_dateD, matchMode: "dateBefore" }, { value: options.value.end_dateD, matchMode: "dateIs" }],
      filteroperator: "or",
      key: "doc_date",
    };
    filterSQL.value.push(filterS);
  }
  else {
    if (options.value.start_dateD) {

      filterS = {
        filterconstraints: [{ value: options.value.start_dateD, matchMode: "dateIs" }],
        filteroperator: "or",
        key: "doc_date",
      };
      filterSQL.value.push(filterS);


    }
    if (options.value.end_dateD) {

      filterS = {
        filterconstraints: [{ value: options.value.end_dateD, matchMode: "dateIs" }],
        filteroperator: "or",
        key: "doc_date",
      };
      filterSQL.value.push(filterS);

    }
  }
  first.value=0;
  if (filterSQL.value.length > 0) loadDataSQL();
  else loadData(true);
};
const RemoveMul = (index, value) => {
  if (index == 1)
    options.value.ca_groups_list = options.value.ca_groups_list.filter(
      (x) => x.code != value
    );

  if (index == 3)
    options.value.ca_fields_list = options.value.ca_fields_list.filter(
      (x) => x.code != value
    );
  if (index == 4)
    options.value.ca_dispatch_book_list =
      options.value.ca_dispatch_book_list.filter((x) => x.code != value);
  if (index == 5)
    options.value.ca_user_recever_list =
      options.value.ca_user_recever_list.filter((x) => x.code != value);
};
const displaySidebarDR = ref(false);
const liUserRecever = ref([]);
const showDetailsRecever = (value) => {
  liUserRecever.value = value;
  displaySidebarDR.value = true;
};
const headerExport = ref("Cấu hình xuất Excel");
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
      font-family: Arial, Helvetica, sans-serif;
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
    .boder tr td {  font-size: 12pt !important;}

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
      word-break: break-all;
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
htmltable += `<div id="formprint" style="width:100%">
      <table>
        <thead>
          <tr>
            <td style="width:33.33%">
          
              <div style="width:100%; align-item:center; font-weight:600;word-break: break-word;">`+  FilterStr.value+` </div>
          <div style="width:100%; align-item:center; font-weight:600">Tổng số: `+  datalistsExport.value.length+` </div>
          
     
              </td>
            <td    style="width:33.33%;padding: 0 0 0.5rem 0 ;text-align:center; " >
            
               <div class="title1" style="width:100%;font-weight:600;height:100%;    padding-top:0">VĂN BẢN ĐẾN</div> 
             
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
       
            <th style="width: 60px ;  padding: 0px 3px">Số đến phòng</th>
            <th style="width: 80px ;  padding: 0px 3px">Số,ký hiệu</th>
            <th style="width: 80px ;word-break: break-word;  padding: 0px 2px"> <b>Ngày thu </b>
              <hr style="margin:3px 25px 0px 25px ; font-weight:600"/>
       
             <b> Ban hành </b>
              </th>
              <th style=" min-width: 80px ; word-break: break-word;  padding: 0px 3px">CQ ban hành</th>
       
            <th style="min-width: 150px ;word-break: break-word;  padding: 0px 3px">Trích yếu</th>
         
         
            <th style="width: 20px ;  padding: 0px 3px">Số bản</th>
            <th style="width: 20px ;  padding: 0px 3px">Số tờ</th>
            <th style="width: 40px ;  padding: 0px 3px">Độ mật</th>
            <th style="width: 20px ;  ; padding: 0px 3px"  >Bản đ/tử</th>
            <th style=" min-width: 70px ;word-break: break-word;  padding: 0px 3px">Ng/nhận</th>
            <th style="width: 40px ;  padding: 0px 3px">Ký nhận</th>
            <th style="width: 40px ;  padding: 0px 3px">Ký trả</th>
            <th style="width: 50px ;  padding: 0px 3px">Ghi chú</th>
          </tr>
        </thead>
        <tbody class="boder">`;
  for (let index = 0; index < datalistsExport.value.length; index++) {
    const value = datalistsExport.value[index];

    var doc_date = "";
    var receive_date = "";
    var num_of_pages="";
    var num_of_copies="";
    var is_not_send_paper="";
    var security="";
    var dispatch_book_code="";
    var doc_code="";
    if(value.dispatch_book_code)
    dispatch_book_code=value.dispatch_book_code;
    if(value.doc_code)
    doc_code=value.doc_code;
    if(value.num_of_pages)
    num_of_pages=value.num_of_pages;
    if(value.num_of_copies)
    num_of_copies=value.num_of_copies;
    if(value.security)
    security=value.security; 
 
    if(value.is_not_send_paper==1)
    is_not_send_paper="1";
     debugger
    if (value.doc_date)
      doc_date = moment(new Date(value.doc_date)).format("DD/MM/YYYY");
    if (value.receive_date)
    receive_date=  moment(new Date(value.receive_date)).format("DD/MM/YYYY")
    htmltable +=
      `
          <tr >
          
            <td  >
              <div style="text-align: center">
                ` +
    dispatch_book_code +
      `
              </div>
            </td>
            <td align="center"   >

              <div >
               <div style="text-align:center;padding:0px"> <div style="font-weight:600">` + doc_code +'</div>    '+dispatch_book_code
      +
      ` </div>
              
              </div>
              
            </td>
            <td   >
              <div >
               <div style="text-align:center;padding:0px"> <div style="font-weight:600">` + receive_date +'</div>  '+doc_date
      +
      ` </div>
              
              </div>
            </td>
            <td  style=" word-break: break-word; text-align:center">
            <div >
              ` +value.issue_place + `
       
            </div>
          </td>
            
            <td  style=" word-break: break-word">
              <div >
                ` +
      value.compendium +
      `
               
              </div>
            </td>
            <td  style=" word-break: break-word">
              <div style="text-align: center">
                ` +
       num_of_pages +
      `
              </div>
            </td>
            <td  style=" word-break: break-word">
              <div style="text-align: center">
                ` +
      num_of_copies +
      `
              </div>
            </td>
            <td  style=" word-break: break-word">
              <div style="text-align: center">
                ` + 
       security +
      `
              </div>
            </td>
            <td  style=" word-break: break-word">
              <div style="text-align: center">
                ` +
                is_not_send_paper +
      `
              </div>
            </td>
            <td  style=" word-break: break-word">
              <div>
                ` +
      value.user_receive +
      `
         
              </div>
            </td>
          
            <td  style=" word-break: break-word">
              <div>
                
              </div>
            </td>
            <td  style=" word-break: break-word">
              <div>
                
              </div>
            </td>
            <td  style=" word-break: break-word">
              <div>
                
              </div>
            </td>
          </tr>`;
  }
  htmltable += `
        </tbody>
      
      </table>
    </div>`;
  // var html = document.getElementById(id);
  // if (html) {
  //   htmltable += html.innerHTML;
  // }
  return htmltable;
}
const datalistsExport = ref();
var checkTypeExpport = false;
const first=ref(0);
//Xuất excel
const menuButs = ref();
const exportExcelR = () => {
  showExport.value = false;
  if (options.value.totalRecordsExport > 10000) {
    swal.fire({
      title: "Thông báo",
      text: "Nhập số bản ghi nhỏ hơn 10000.",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }

  if (checkTypeExpport) exportData("ExportExcel");
  else {
     
    options.value.loading = true;
    axios
      .post(
        baseURL + "/api/DocProc/CallProc",
        {
          str: encr(
            JSON.stringify({
              proc: "doc_report_list_receive",
              par: [
                { par: "pageno", va: options.value.pagenoExport - 1 },
                { par: "pagesize", va: options.value.totalRecordsExport },
                { par: "user_id", va: store.state.user.user_id },
                { par: "user_recever", va: options.value.user_recever },
                { par: "dispatch_book_id", va: options.value.dispatch_book_id },
                { par: "doc_group_id", va: options.value.doc_group_id },
                { par: "field_id", va: options.value.field_id },
                { par: "department_id_process", va: options.value.department_id_process_fake },
                { par: "department_id", va: options.value.department_id },
                { par: "start_dateI", va: options.value.start_dateI },
                { par: "end_dateI", va: options.value.end_dateI },
                { par: "start_dateD", va: options.value.start_dateD },
                { par: "end_dateD", va: options.value.end_dateD },
                { par: "search", va: options.value.search },
                { par: "sort", va: options.value.sort },
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
        data.forEach((element) => {
          if (!element.compendium) element.compendium = "";
          if (!element.receive_place) element.receive_place = "";
          if (!element.ldt) element.ldt = "";
          if (!element.signer) element.signer = "";
          if (!element.user_receive) element.user_receive = "";
          if (!element.dispatch_book_num) element.dispatch_book_num = "";
          if (!element.doc_code) element.doc_code = "";
        });
         
        if (data.length > 0) {
          datalistsExport.value = data;

          print();

          options.value.loading = false;
        } else {
          datalistsExport.value = [];
        }
      })
      .catch((error) => {
        console.log(error);
      });
  }
};

const itemButs = ref([
  {
    label: "Xuất Excel",
    icon: "pi pi-file-excel",
    command: (event) => {
      if (options.value.totalRecords < 10000) {
        options.value.totalRecordsExport = options.value.totalRecords;
        exportData("ExportExcel");
      } else {
        headerExport.value = "Cấu hình xuất Excel";
        options.value.totalRecordsExport = 50;
        checkTypeExpport = true;
        showExport.value = true;
      }
    },
  },
  {
    label: "In báo cáo",
    icon: "pi pi-print",
    command: (event) => {
      headerExport.value = "Cấu hình in báo cáo";
      options.value.totalRecordsExport = 50;
      checkTypeExpport = false;
      showExport.value = true;
    },
  },
]);
const toggleExport = (event) => {
  
  var strG = "";
  var strk = "";

  if (options.value.department_id_process) {
    for (const key in options.value.department_id_process) {
      strG += strk + key;
      strk = ",";
    }
  }
  if(strG!=null)
  options.value.department_id_process_fake= strG;
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
        excelname: "BÁO CÁO KHỐI CƠ QUAN ĐẾN",
        proc: "doc_report_list_receive_export",
        par: [
          { par: "pageno", va: options.value.pagenoExport - 1 },
          { par: "pagesize", va: options.value.totalRecordsExport },
          { par: "user_id", va: store.state.user.user_id },
          { par: "user_recever", va: options.value.user_recever },
          { par: "dispatch_book_id", va: options.value.dispatch_book_id },
          { par: "doc_group_id", va: options.value.doc_group_id },
          { par: "field_id", va: options.value.field_id },
          { par: "department_id_process", va: options.value.department_id_process_fake },
          { par: "department_id", va: options.value.department_id },
          { par: "start_dateI", va: options.value.start_dateI },
          { par: "end_dateI", va: options.value.end_dateI },
          { par: "start_dateD", va: options.value.start_dateD },
          { par: "end_dateD", va: options.value.end_dateD },
          { par: "search", va: options.value.search },
          { par: "sort", va: options.value.sort },
        ],
      },
      config
    )
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        swal.close();

        toast.success("Kết xuất Data thành công!");
        window.open(baseURL + response.data.path);
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
  dispatch_book_num: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
  doc_code: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },

  receive_date: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.DATE_IS }],
  },
  doc_date: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.DATE_IS }],
  },
});
const filterSQL = ref([]);
const onPage = (event) => {
  if (event.rows != options.value.pagesize) {
    options.value.pagesize = event.rows;
  }
  if (event.page == 0) {
    //Trang đầu
    options.value.id = null;
    options.value.IsNext = true;
  } else if (event.page > options.value.pageno + 1) {
    //Trang cuối
    options.value.id = -1;
    options.value.IsNext = false;
  } else if (event.page > options.value.pageno) {
    //Trang sau

    options.value.id =
      datalists.value[datalists.value.length - 1].doc_master_id;
    options.value.IsNext = true;
  } else if (event.page < options.value.pageno) {
    //Trang trước
    options.value.id = datalists.value[0].doc_master_id;
    options.value.IsNext = false;
  }
  options.value.pagesize = event.rows;
  options.value.pageno = event.page;

  loadDataSQL();
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
    if (event.sortField != "doc_master_id") {
      options.value.sort +=
        ",doc_master_id " + (event.sortOrder == 1 ? " ASC" : " DESC");
    }
    isDynamicSQL.value = true;
    loadData();
  }
};
onMounted(() => {
  loadFilterDM();
  // loadOrganization();
  loadData();
});
</script>
<template>
  <div class="d-container">
    <div class="d-lang-table">
      <DataTable class="w-full p-datatable-sm e-sm p-table-custom-d" :lazy="true" @page="onPage($event)"
        @filter="onFilter($event)" @sort="onSort($event)" :value="datalists" :loading="options.loading"
        :paginator="options.totalRecords > options.pagesize" :rows="options.pagesize" :totalRecords="options.totalRecords"
        dataKey="doc_master_id" :rowHover="true" :filters="filters" :showGridlines="true" filterDisplay="menu" 
        filterMode="lenient" responsiveLayout="scroll" :scrollable="true" scrollHeight="flex"  v-model:first="first"
        paginatorTemplate="FirstPageLink PrevPageLink PageLinks  NextPageLink LastPageLink    RowsPerPageDropdown"
        :rowsPerPageOptions="[20, 30, 50, 100, 200]">
        <template #header>
          <div>
            <h3 class="module-title my-2 ml-1">
              <font-awesome-icon icon="fa-solid fa-file-arrow-down" /> Báo cáo
              khối cơ quan đến ({{
                options.totalRecords ? options.totalRecords : 0
              }})
            </h3>
          </div>
          <Toolbar class="custoolbar p-0 py-3 surface-50">
            <template #start>
              <span class="p-input-icon-left">
                <i class="pi pi-search" />
                <InputText v-model="options.search" @keyup.enter="searchReceive()" type="text" spellcheck="false"
                  placeholder="Tìm kiếm" />

                <Button :class="
                  (options.doc_group_id != null ||
                    options.department_id != null ||
                    options.department_id_process != null ||

                    options.fields_id != null ||
                    options.dispatch_book_id != null ||
                    options.start_dateI != null ||
                    options.ca_groups_list != null ||

                    options.ca_fields_list != null ||
                    options.ca_dispatch_book_list != null ||
                    options.end_dateI != null ||
                    options.ca_user_recever_list != null ||
                    options.end_dateD != null ||
                    options.start_dateD != null) &&
                    checkFilter
                    ? ''
                    : 'p-button-secondary p-button-outlined'
                " class="ml-2" icon="pi pi-filter" @click="toggleFilter" aria-haspopup="true"
                  aria-controls="overlay_panelS" />
                <OverlayPanel @hide="hideFilter" ref="filterButs" appendTo="body" :showCloseIcon="false"
                  id="overlay_panelS" style="width: 500px" :breakpoints="{ '960px': '20vw' }">
                  <div class="grid formgrid m-2">

                    <div class="field col-12 md:col-12 flex">
                      <div class="col-3 p-0 align-items-center flex">
                        Ngày thu:
                      </div>
                      <div class="col-4 p-0 align-items-center flex">
                        <Calendar class="w-full" v-model="options.start_dateI" placeholder="dd/MM/yy" />
                      </div>
                      <div class="col-1 p-0 align-center align-items-center flex">
                        <span class="w-full text-center font-bold">-</span>
                      </div>
                      <div class="col-4 p-0 align-items-center flex">
                        <Calendar class="w-full" v-model="options.end_dateI"
                          :minDate="options.start_dateI ? new Date(options.start_dateI) : null" placeholder="dd/MM/yy" />
                      </div>
                    </div>
                    <div class="field col-12 md:col-12 flex">
                      <div class="col-3 p-0 align-items-center flex">
                        Ngày ban hành:
                      </div>
                      <div class="col-4 p-0 align-items-center flex">
                        <Calendar class="w-full" v-model="options.start_dateD" placeholder="dd/MM/yy" />
                      </div>
                      <div class="col-1 p-0 align-center align-items-center flex">
                        <span class="w-full text-center font-bold">-</span>
                      </div>
                      <div class="col-4 p-0 align-items-center flex">
                        <Calendar class="w-full" v-model="options.end_dateD"
                          :minDate="options.start_dateD ? new Date(options.start_dateD) : null" placeholder="dd/MM/yy" />
                      </div>
                    </div>

                    <div class="field col-12 md:col-12 flex">
                      <div class="col-3 p-0 align-items-center flex">
                        Nhóm văn bản:
                      </div>
                      <div class="col-9 p-0 align-items-center flex">
                        <MultiSelect v-model="options.ca_groups_list" :options="listFilterDM.ca_groups_list"
                          optionLabel="name" placeholder="Chọn nhóm văn bản" :filter="true"
                          class="multiselect-custom w-full">
                          <template #value="slotProps">
                            <div class="flex">
                              <div class="country-item country-item-value mr-2" v-for="option of slotProps.value"
                                :key="option.code">
                                <Chip :label="option.name" removable @remove="RemoveMul(1, option.code)" />
                              </div>
                            </div>
                            <template v-if="
                              !slotProps.value || slotProps.value.length === 0
                            ">
                              Chọn nhóm văn bản
                            </template>
                          </template>
                          <template #option="slotProps">
                            <div class="country-item">
                              <div>{{ slotProps.option.name }}</div>
                            </div>
                          </template>
                        </MultiSelect>
                      </div>
                    </div>
                    <div class="field col-12 md:col-12 flex">
                      <div class="col-3 p-0 align-items-center flex">
                        Phòng ban soạn thảo:
                      </div>
                      <div class="col-9 p-0 align-items-center flex">
                        <TreeSelect class="w-full" v-model="options.department_id" :options="listFilterDM.department_list"
                          display="chip" selectionMode="checkbox" placeholder="Chọn phòng ban soạn thảo" :clear="true">
                        </TreeSelect>
                      </div>
                    </div>
                    <div class="field col-12 md:col-12 flex">
                      <div class="col-3 p-0 align-items-center flex">
                        Phòng ban xử lý:
                      </div>
                      <div class="col-9 p-0 align-items-center flex">
                        <TreeSelect class="w-full" v-model="options.department_id_process"
                          :options="listFilterDM.department_list" display="chip" selectionMode="checkbox"
                          placeholder="Chọn phòng ban xử lý" :clear="true"></TreeSelect>
                      </div>
                    </div>
                    <!-- <div class="field col-12 md:col-12 flex">
                        <div class="col-3 p-0 align-items-center flex">
                          Phòng ban:
                        </div>
                        <div class="col-9 p-0 align-items-center flex">
                          <MultiSelect
                            v-model="options.ca_issue_place_list"
                            :options="listFilterDM.ca_issue_place_list"
                            optionLabel="name"
                            placeholder="Chọn nhóm văn bản"
                            :filter="true"
                            class="multiselect-custom w-full"
                          >
                            <template #value="slotProps">
                              <div class="flex">
                                <div
                                  class="country-item country-item-value mr-2"
                                  v-for="option of slotProps.value"
                                  :key="option.code"
                                >
                                  <Chip
                                    :label="option.name"
                                    removable
                                    @remove="RemoveMul(2, option.code)"
                                  />
                                </div>
                              </div>
                              <template
                                v-if="
                                  !slotProps.value || slotProps.value.length === 0
                                "
                              >
                                Chọn nơi ban hành
                              </template>
                            </template>
                            <template #option="slotProps">
                              <div class="country-item">
                                <div>{{ slotProps.option.name }}</div>
                              </div>
                            </template>
                          </MultiSelect>
                        </div>
                      </div> -->
                    <div class="field col-12 md:col-12 flex">
                      <div class="col-3 p-0 align-items-center flex">
                        Lĩnh vực:
                      </div>

                      <div class="col-9 p-0 align-items-center flex">
                        <MultiSelect v-model="options.ca_fields_list" :options="listFilterDM.ca_fields_list"
                          optionLabel="name" placeholder="Chọn nhóm văn bản" :filter="true"
                          class="multiselect-custom w-full">
                          <template #value="slotProps">
                            <div class="flex">
                              <div class="country-item country-item-value mr-2" v-for="option of slotProps.value"
                                :key="option.code">
                                <Chip :label="option.name" removable @remove="RemoveMul(3, option.code)" />
                              </div>
                            </div>
                            <template v-if="
                              !slotProps.value || slotProps.value.length === 0
                            ">
                              Chọn lĩnh vực
                            </template>
                          </template>
                          <template #option="slotProps">
                            <div class="country-item">
                              <div>{{ slotProps.option.name }}</div>
                            </div>
                          </template>
                        </MultiSelect>
                      </div>
                    </div>
                    <div class="field col-12 md:col-12 flex">
                      <div class="col-3 p-0 align-items-center flex">
                        Khối cơ quan:
                      </div>

                      <div class="col-9 p-0 align-items-center flex">
                        <MultiSelect v-model="options.ca_dispatch_book_list" :options="listFilterDM.ca_dispatch_book_list"
                          optionLabel="name" placeholder="Chọn nhóm văn bản" :filter="true"
                          class="multiselect-custom w-full">
                          <template #value="slotProps">
                            <div class="flex">
                              <div class="country-item country-item-value mr-2" v-for="option of slotProps.value"
                                :key="option.code">
                                <Chip :label="option.name" removable @remove="RemoveMul(4, option.code)" />
                              </div>
                            </div>
                            <template v-if="
                              !slotProps.value || slotProps.value.length === 0
                            ">
                              Chọn khối cơ quan
                            </template>
                          </template>
                          <template #option="slotProps">
                            <div class="country-item">
                              <div>{{ slotProps.option.name }}</div>
                            </div>
                          </template>
                        </MultiSelect>
                      </div>
                    </div>
                    <div class="field col-12 md:col-12 flex">
                      <div class="col-3 p-0 align-items-center flex">
                        Cán bộ xử lý:
                      </div>

                      <div class="col-9 p-0 align-items-center flex">
                        <MultiSelect v-model="options.ca_user_recever_list" :options="listFilterDM.ca_user_recever_list"
                          optionLabel="name" placeholder="Chọn cán bộ xử lý" :filter="true"
                          class="multiselect-custom w-full">
                          <template #value="slotProps">
                            <div class="flex">
                              <div class="country-item country-item-value mr-2" v-for="option of slotProps.value"
                                :key="option.code">
                                <Chip :label="option.name" removable @remove="RemoveMul(5, option.code)" />
                              </div>
                            </div>
                            <template v-if="
                              !slotProps.value || slotProps.value.length === 0
                            ">
                              Chọn cán bộ xử lý
                            </template>
                          </template>
                          <template #option="slotProps">
                            <div class="country-item flex align-items-center">
                              <div class="grid w-full p-0">
                                <div class="
                                      field
                                      p-0
                                      py-1
                                      col-12
                                      flex
                                      m-0
                                      cursor-pointer
                                      align-items-center
                                    ">
                                  <div class="col-1 mx-2 p-0 align-items-center">
                                    <Avatar v-bind:label="
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
                                    " :image="
  basedomainURL + slotProps.option.avatar
" size="small" :style="
  slotProps.option.avatar
    ? 'background-color: #2196f3'
    : 'background:' +
    bgColor[
    slotProps.option.name.length % 7
    ]
" shape="circle" @error="
  $event.target.src =
  basedomainURL +
  '/Portals/Image/nouser1.png'
" />
                                  </div>
                                  <div class="col-11 p-0 ml-3 align-items-center">
                                    <div class="pt-2">
                                      <div class="font-bold">
                                        {{ slotProps.option.name }}
                                      </div>
                                      <div class="
                                            flex
                                            w-full
                                            text-sm
                                            font-italic
                                            text-500
                                          ">
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
                    </div>
                    <div class="col-12 field p-0">
                      <Toolbar class="toolbar-filter custoolbar">
                        <template #start>
                          <Button @click="onRefilterDM()" class="p-button-outlined" label="Xóa"></Button>
                        </template>
                        <template #end>
                          <Button @click="onFilterDM()" label="Lọc"></Button>
                        </template>
                      </Toolbar>
                    </div>
                  </div>
                </OverlayPanel>
              </span>
            </template>

            <template #end>
              <MultiSelect :modelValue="selectedColumns" :options="columns" optionLabel="header" class="mx-2"
                placeholder="Hiển thị thêm" @update:modelValue="onToggle" />
              <Button class="mr-2 p-button-outlined p-button-secondary" icon="pi pi-refresh" @click="refreshData" />

              <Button label="Tiện ích" icon="pi pi-file-excel" class="mr-2 p-button-outlined p-button-secondary"
                @click="toggleExport" aria-haspopup="true" aria-controls="overlay_Export" />
              <Menu id="overlay_Export" ref="menuButs" :model="itemButs" :popup="true" />
            </template>
          </Toolbar>
        </template>

        <Column field="is_order" header="STT" class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;max-width:50px"
          bodyStyle="text-align:center;max-width:50px; word-break:break-all"></Column>
        <Column field="dispatch_book_code" header="Số vào sổ" class="
              align-items-center
              justify-content-center
              text-center
              limit-line
            " headerStyle="text-align:center;max-width:130px" bodyStyle="text-align:center;max-width:130px;word-break:break-all
            " headerClass="limit-line-1" :sortable="true">
          <template #filter="{ filterModel }">
            <InputText type="text" v-model="filterModel.value" class="p-column-filter" placeholder="Từ khoá" />
          </template>
        </Column>
        <Column header="Ngày thu" class="
              align-items-center
              justify-content-center
              text-center
              limit-line
            " headerStyle="text-align:center;max-width:130px"
          bodyStyle="text-align:center;max-width:130px; word-break:break-word" :sortable="true" filterField="receive_date"
          dataType="date">
          <template #filter="{ filterModel }">
            <Calendar v-model="filterModel.value" class="p-column-filter" placeholder="dd/MM/yy" dateFormat="mm/dd/yy" />
          </template>
          <template #body="data">
            <div>
              {{
                data.data.receive_date ? moment(new Date(data.data.receive_date)).format("DD/MM/YYYY") : ''
              }}
            </div>
          </template>
        </Column>
        <Column field="doc_code" header="Số ký hiệu" class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;max-width:130px" bodyStyle="max-width:130px;word-break:break-all"
          :sortable="true">
          <template #filter="{ filterModel }">
            <InputText type="text" v-model="filterModel.value" class="p-column-filter" placeholder="Từ khoá" />
          </template>
        </Column>
        <Column field="doc_date" header="Ban hành" class="
              align-items-center
              justify-content-center
              text-center
              limit-line
            " headerStyle="text-align:center;max-width:120px"
          bodyStyle="text-align:center;max-width:120px; word-break:break-word" headerClass="limit-line-1" :sortable="true"
          filterField="doc_date" dataType="date">
          <template #filter="{ filterModel }">
            <Calendar v-model="filterModel.value" class="p-column-filter" placeholder="dd/MM/yy" dateFormat="mm/dd/yy" />
          </template>
          <template #body="data">
            <div>
              {{ data.data.doc_date ? moment(new Date(data.data.doc_date)).format("DD/MM/YYYY") : '' }}
            </div>
          </template>
        </Column>

        <Column class="
              align-items-center
              justify-content-center
              text-center
              limit-line
            " headerStyle="text-align:center;max-width:120px"
          bodyStyle="text-align:center;max-width:120px; word-break:break-word" v-for="(col, index) of selectedColumns"
          :field="col.field" :header="col.header" :key="index">
        </Column>
        <Column field="compendium" header="Trích yếu" class="text-justify limit-line" headerStyle="text-align:left"
          bodyStyle="text-align:left; word-break:break-word" headerClass="format-center">
          <template #body="data"> {{ data.data.compendium }} </template>
        </Column>
        <Column field="user_receive" header="Nơi nhận" class="text-left text-justify" headerStyle="text-align:center"
          bodyStyle="text-align:center; word-break:break-word" headerClass="format-center">
          <template #body="data">
            <div class="limit-line cursor-pointer" @click="showDetailsRecever(data.data.user_receive)">
              {{ data.data.user_receive }}
            </div>
          </template>
        </Column>
        <Column field="signer" header="Người ký" class="
              align-items-center
              justify-content-center
              text-center
              limit-line
            " headerStyle="text-align:center;max-width:120px"
          bodyStyle="text-align:center;max-width:120px; word-break:break-word">
        </Column>
        <Column header="Ghi chú" class="
              align-items-center
              justify-content-center
              text-center
              limit-line
            " headerStyle="text-align:center;max-width:70px" bodyStyle="text-align:center;max-width:70px">
          <template #body="data">
            <div v-if="data.data.file_path">
              <Button @click="showURLSCV(basedomainURL + data.data.file_path)" icon="pi pi-paperclip"
                v-tooltip.top="'File đính kèm'" class="p-button-rounded p-button-outlined" />
            </div>
          </template>
        </Column>
        <template #empty>
          <div class="
                align-items-center
                justify-content-center
                p-4
                text-center
                m-auto
              " v-if="!isFirstCard">
            <img src="../../assets/background/nodata.png" height="144" />
            <h3 class="m-1">Không có dữ liệu</h3>
          </div>
        </template>
      </DataTable>
    </div>
    <iframe name="printframe" id="printframe" style="display: none"></iframe>
  </div>

  <Sidebar :showCloseIcon="false" position="right" v-model:visible="displaySidebarDR">
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
  <Dialog :style="{ width: '20vw' }" :header="headerExport" v-model:visible="showExport">
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
          <InputNumber class="w-full" :min="1" v-model="options.pagenoExport" />
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
</style>
  