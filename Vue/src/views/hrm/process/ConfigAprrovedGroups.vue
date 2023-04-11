<script setup>
import { ref, inject, onMounted } from "vue";
import { useToast } from "vue-toastification";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";

import treeuser from "../../../components/user/treeuser.vue";
import { encr, checkURL } from "../../../util/function.js";
//Khai báo
const router = inject("router");
const cryoptojs = inject("cryptojs");

const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const basedomainURL = baseURL;
const selectedHandOver = ref();
const displayAssets = ref(false);
const headerDialogUser = ref("Chọn người duyệt");
const expandedKeys = ref([]);
const isFirstCard = ref(false);
const selectedUser = ref([]);

const rules = {
  approved_group_name: {
    required,
    $errors: [
      {
        $property: "approved_group_name",
        $validator: "required",
        $message: "Tên nhóm duyệt không được để trống!",
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
const checkMultile = ref(false);

const showListAssets = () => {
  liUserCF.value = [];
  displayAssets.value = true;
};
const showListUser = () => {
  checkMultile.value = false;
  selectedUser.value = [...listUserA.value];
  displayDialogUser.value = true;
};
let selectedTreeU = null;
const showTreeUser = (value) => {
  checkMultile.value = false;
  selectedTreeU = value;
  displayDialogUser.value = true;
};
const onChangeSWT = () => {
  if (sys_approved_groups.value.is_department == true) {
    listUserA.value = [];
  } else {
    selectedUser.value = [];
  }
};

const removeListUser = (value) => {
  listUserA.value = listUserA.value.filter((x) => x.user_id != value.user_id);
};
const listUserA = ref([]);

const delCard = (Card) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá nhóm duyệt này không!",
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
            baseURL + "/api/sys_approved_groups/delete_sys_approved_groups",
            {
              headers: { Authorization: `Bearer ${store.getters.token}` },
              data: Card != null ? [Card.approved_groups_id] : 1,
            }
          )
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá nhóm duyệt thành công!");
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
  approved_group_name: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
  handover_created_date: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.DATE_IS }],
  },
  user_deliver_name: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
  user_receiver_name: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
  status: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.EQUALS }],
  },
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
    id: "approved_groups_id",
    next: options.value.IsNext,
    sqlO: options.value.sort,
    Search: options.value.search,
    PageNo: options.value.pageno,
    PageSize: options.value.pagesize,
    fieldSQLS: filterSQL.value,
  };

  options.value.loading = true;
  axios
    .post(baseURL + "/api/SQL/Filter_sys_approved_groups", data, config)
    .then((response) => {
      let dt = JSON.parse(response.data.data);

      let data = dt[0];

      if (data.length > 0) {
        data.forEach((element, i) => {
          element.is_order =
            options.value.pageno * options.value.pagesize + i + 1;

          if (!element.module) {
            element.module_fake = element.module.split(",");
          }

          if (!element.config_type_name) element.config_type_name = "";
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

const checkTypeHO = ref(false);

//Sort
const onSort = (event) => {
  first.value = 0;
  options.value.pageno = 0;

  if (event.sortField == null) {
    isDynamicSQL.value = false;
    loadData();
  } else {
    options.value.sort =
      event.sortField + (event.sortOrder == 1 ? " ASC" : " DESC");
    if (event.sortField != " cp.approved_groups_id") {
      options.value.sort +=
        ", cp.approved_groups_id " + (event.sortOrder == 1 ? " ASC" : " DESC");
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
  sort: " cp.approved_groups_id DESC",
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
const sys_approved_groups = ref({
  is_order: 1,
  proposal_code: "",
  device_id: null,
  approved_group_name: "",
  image: "",
  barcode_id: "",
  barcode_type: 0,
  barcode_image: "",
  status: 0,
  is_department: false,
});
const v$ = useVuelidate(rules, sys_approved_groups);
const danhMuc = ref();
//METHOD

const hideSelectDevice = () => {
  loadOrganization(store.getters.user.user_id);
  displayAssets.value = false;
};

const liUserCF = ref([]);
const rechildren = (data) => {
  if (data.data != null) {
    if (data.data.userM)
      data.data.userM.forEach((element) => {
        liUserCF.value.push({
          user_id: element,
          department_id: data.data.organization_id,
        });
      });

    if (data.children)
      data.children.forEach((em) => {
        rechildren(em);
      });
  }
};
const closeDialogUser = () => {
  displayDialogUser.value = false;
};

const choiceUser = () => {
  if (sys_approved_groups.value.is_department == true) {
    datalistsD.value.forEach((m, i) => {
      let om = { key: m.key, data: m };

      if (m.key == selectedTreeU.organization_id) {
        if (m.data.userM == null) m.data.userM = [];
        selectedUser.value.forEach((element) => {
          m.data.userM.push(element.user_id);
        });
        return;
      } else {
        let check = false;
        const rechildrenD = (mm) => {
          if (mm.key == selectedTreeU.organization_id) {
            if (mm.data.data.userM == null) mm.data.data.userM = [];

            selectedUser.value.forEach((element) => {
              mm.data.data.userM.push(element.user_id);
            });
            check = true;
            return;
          } else {
            if (mm.data.children) {
              let dts = mm.data.children;

              if (dts.length > 0) {
                dts.forEach((em) => {
                  let om1 = { key: em.key, data: em };
                  if (check) return;
                  rechildrenD(om1, em.key);
                });
              }
            }
          }
        };
        if (check) return;
        rechildrenD(om, m.key);
      }
    });
  } else {
    selectedUser.value.forEach((element) => {
      listUserA.value.push(element);
    });
  }

  closeDialogUser();
};
const onSelectDevice = () => {
  datalistsD.value.forEach((dataT) => {
    rechildren(dataT);
  });
  displayAssets.value = false;
};
const listUserApproved = ref();
const typeUserApp = ref(false);
const idApprovedSave = ref();
const openDetails = (data) => {
  if (
    swithViewGroups.value &&
    idApprovedSave.value == data.approved_groups_id
  ) {
    typeUserApp.value = false;
    selectedHandOver.value = null;
    swithViewGroups.value = false;
  } else {
    idApprovedSave.value = data.approved_groups_id;
    typeUserApp.value = data.is_department;
    selectedHandOver.value = data;
    options.value.loadingU = true;
    swithViewGroups.value = true;
    axios
      .post(
        baseURL + "/api/device_card/getData",
        {
          str: encr(
            JSON.stringify({
              proc: "sys_approved_groups_list_user",
              par: [
                { par: "approved_groups_id", va: data.approved_groups_id },
                { par: "search", va: null },
              ],
            }),
            SecretKey,
            cryoptojs
          ).toString(),
        },
        config
      )
      .then((response) => {
        let data = JSON.parse(response.data.data);
        listUserApproved.value = data[0];

        options.value.loadingU = false;
      })
      .catch((error) => {});
  }
};
const loadCount = () => {
  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_approved_groups_count",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "status", va: options.value.status },
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
    .catch(() => {});
};
const saveHandover = (isFormValid) => {
  submitted.value = true;

  if (!isFormValid) {
    return;
  }

  if (!sys_approved_groups.value.module_fake) {
    return;
  } else {
    let str = "";
    let kol = "";
    for (const key in sys_approved_groups.value.module_fake) {
      str += kol + key;
      kol = ",";
    }
    sys_approved_groups.value.module = str;
  }
  if (sys_approved_groups.value.approved_group_name.length > 250) {
    swal.fire({
      title: "Thông báo!",
      text: "Tên nhóm duyệt không được dài quá 250 kí tự!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  let arrP = null;
  if (sys_approved_groups.value.is_department == true) {
    arrP = liUserCF.value;
  } else {
    arrP = listUserA.value;
  }
  let formData = new FormData();
  formData.append("approved", JSON.stringify(sys_approved_groups.value));
  formData.append("approvedusers", JSON.stringify(arrP));
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  
  if (!isSaveCard.value) {
    axios
      .post(
        baseURL + "/api/sys_approved_groups/add_sys_approved_groups",
        formData,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Thêm nhóm duyệt thành công!");

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
      .put(
        baseURL + "/api/sys_approved_groups/update_sys_approved_groups",
        formData,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa nhóm duyệt thành công!");

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
const dpmId = ref({});
const editCard = (data) => {
  submitted.value = false;
  dpmId.value = {};
   
  sys_approved_groups.value = data;
  if (!sys_approved_groups.value.is_department) {
    if( data.signusers)
    listUserA.value = data.signusers;
    else
    listUserA.value=[];
  } else {
    if( data.signusers)
    liUserCF.value = data.signusers;
    else
    liUserCF.value=[];
    
    datalistsD.value.forEach((element) => {
      if( data.signusers)
      rechildrenSPK(element,  data.signusers);
    });
  }
  headerDialog.value = "Sửa nhóm duyệt";
  isSaveCard.value = true;
  displayBasic.value = true;
};
const rechildrenSPK = (item, data) => {
  let arm = data.filter((x) => x.department_id == item.key);
   
  if (arm.length > 0) {
    let aru=[];
    arm.forEach(element => {
      aru.push(element.user_id);
    });
    item.data.userM =aru;
  }
  if (item.children) {
    item.children.forEach((em) => {
      rechildrenSPK(em, data);
    });
  }
};
//Hiển thị dialog

const headerDialog = ref();
const displayBasic = ref(false);

const listModules = ref([]);
const onChangeModule = (event) => {
  for (const key in event) {
    if (key == -1) {
      const qrechildren = (mm) => {
        sys_approved_groups.value.module_fake[mm.key] = {
          checked: true,
          partialChecked: false,
        };
        if (mm.children) {
          mm.children.forEach((ol) => {
            qrechildren(ol);
          });
        }
      };
      listModules.value.forEach((element) => {
        qrechildren(element);
      });
    }
  }
};
const listTCard = ref([
  { name: "Duyệt một nhiều", code: 1 },
  { name: "Duyệt tuần tự", code: 2 },
  { name: "Duyệt ngẫu nhiên", code: 3 },
]);

const openBasic = (str) => {
  checkTypeHO.value = false;
  listUserA.value = [];
  datalistsD.value = datalistsDSave.value;
  sys_approved_groups.value = {
    is_local: true,
    module_fake: null,
    approved_type: 1,
    is_department: false,
    is_return_pre: false,
    status: true,
    is_order: sttCard.value ? sttCard.value : 1,
    is_return_created: false,
    is_skip: false,
  };

  submitted.value = false;
  headerDialog.value = str;
  isSaveCard.value = false;

  displayBasic.value = true;
};

const closeDialogDC = () => {
  displayBasic.value = false;
};
const closeDialog = () => {
  isFirstCard.value = false;
  loadData(true);
  displayBasic.value = false;
};
const loadData = (rf) => {
  if (isDynamicSQL.value) {
    loadDataSQL();
    return false;
  };
 
  axios
    .post(
      baseURL + "/api/HRM_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_config_approved_list_module",
            par: [
              { par: "pageno", va: options.value.pageno },
              { par: "pagesize", va: options.value.pagesize },
              { par: "user_id", va: store.getters.user.user_id },
    
              { par: "module_key", va: "M13" },
              { par: "status", va: null },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      if (response != null && response.data != null) {
        var data = response.data.data;
        if (data != null) {
          let tbs = JSON.parse(data);
          if (tbs[0] != null && tbs[0].length > 0) {
            datalists.value = tbs[0];
            if (datalists.value.length > 0) {
              datalists.value.forEach((element, i) => {
                element["STT"] = i + 1;

                if (element["signusers"] != null) {
                  element["signusers"] = JSON.parse(element["signusers"]);
                }
                
                if (element["signusers"] != null) {
                element["signusers"].forEach(ilem => {
                  if(ilem.is_order=="")
                  ilem.is_order=null;
                  else
                  ilem.is_order=Number(ilem.is_order);
                  if(ilem.approved_users_id=="")
                  ilem.approved_users_id=null;
                  else
                  ilem.approved_users_id=Number(ilem.approved_users_id);
                  if(ilem.department_id=="")
                  ilem.department_id=null;
                  else
                  ilem.department_id=Number(ilem.department_id);
                  if(ilem.avatar=="")
                  ilem.avatar=null;
             
                  
             
                });

              }

                if (!element.module_fake) {
                  element.module_fake = {};
                  element.module.split(",").forEach((item) => {
                    element.module_fake[item] = {
                      checked: true,
                      partialChecked: false,
                    };
                  });
                }
              });
            }
          } else {
            datalists.value = [];
          }
          if (tbs.length == 2) {
            options.value.totalRecords = tbs[1][0].total;
          }
        }
      }
      swal.close();
      if (isFirst.value) isFirst.value = false;
      if (options.value.loading) options.value.loading = false;
    })
    .catch((error) => {
      if (options.value.loading) options.value.loading = false;
      swal.close();
      swal.fire({
        title: "Thông báo!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
      console.log(error);
      return;
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
      filterSCard.value != null ||
      filterMCard.value != null ||
      filterTCard.value != null
    )
  )
    checkFilter.value = false;
};
const filterMCard = ref();
const filterTCard = ref();
const filterSCard = ref();

const showFilter = ref(false);
const reFilterCard = () => {
  checkFilter.value = false;
  filterSCard.value = null;
  filterMCard.value = null;
  filterTCard.value = null;
  taskDateFilter.value = [];
  options.value.is_hot = null;
  options.value.news_type = null;
  options.value.status = null;
  filterCard(false);
  showFilter.value = false;
};
const filterCard = (check) => {
  if (check) checkFilter.value = true;

  showFilter.value = false;

  filterSQL.value = [];
  if (filterSCard.value != null) {
    let filterS = {
      filterconstraints: [{ value: filterSCard.value, matchMode: "equals" }],
      filteroperator: "and",
      key: "module",
    };
    filterSQL.value.push(filterS);
  }
  if (filterTCard.value != null) {
    let filterS = {
      filterconstraints: [{ value: filterTCard.value, matchMode: "equals" }],
      filteroperator: "and",
      key: "approved_type",
    };
    filterSQL.value.push(filterS);
  }
  if (filterMCard.value != null) {
    let filterS = {
      filterconstraints: [{ value: filterMCard.value, matchMode: "equals" }],
      filteroperator: "and",
      key: "module",
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
  filterSCard.value = null;
  filterMCard.value = null;
  filterTCard.value = null;
  options.value.start_date = null;
  options.value.end_date = null;
  taskDateFilter.value = [];
  checkFilter.value = false;
  first.value = 0;
  options.value.pageno = 0;
  filterSQL.value = [];
  filters.value = {
    global: { value: null, matchMode: FilterMatchMode.CONTAINS },
    approved_group_name: {
      operator: FilterOperator.AND,
      constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
    },
    handover_created_date: {
      operator: FilterOperator.AND,
      constraints: [{ value: null, matchMode: FilterMatchMode.DATE_IS }],
    },
    user_deliver_name: {
      operator: FilterOperator.AND,
      constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
    },
    user_receiver_name: {
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

const renderTreeDV1 = (data, id, name, title, org_id) => {
  let arrtreeChils = [];
  let arrChils = [];

  data
    .filter((x) => x.parent_id == null)
    .forEach((m, i) => {
      if (!m.userM) m.userM = null;
      let om = { key: m[id], data: m };

      const Drechildren = (mm, pid) => {
        let dts = data.filter((x) => x.parent_id == pid);
        if (dts.length > 0) {
          if (!mm.children) mm.children = [];
          dts.forEach((em) => {
            if (!em.userM) em.userM = null;
            let om1 = { key: em[id], data: em };
            Drechildren(om1, em[id]);
            mm.children.push(om1);
          });
        }
      };
      Drechildren(om, m[id]);
      arrChils.push(om);
    });
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
  return { arrChils: arrChils, arrtreeChils: arrtreeChils };
};

const listDropdownUserGive = ref();
const listDropdownUserCheck = ref();
const listDropdownUser = ref();
const listUsers = ref([]);
const loadingUser = ref(false);
const onFilterUserDropdown = (value) => {
  loadingUser.value = true;

  if (value.organization_id == 1)
    listDropdownUserGive.value = listDropdownUser.value;
  else
    listDropdownUserGive.value = listDropdownUser.value.filter(
      (x) => x.department_id == value.organization_id
    );
  loadingUser.value = false;
};
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
          department_id: element.department_id,
          position_name: element.position_name,
          role_name: element.role_name,
          organization_id: element.organization_id,
        });
        listUsers.value.push({ data: element, active: false });
      });
      listUsers.value = data;
      listDropdownUserGive.value = listDropdownUser.value;
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

const listUser = () => {
  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_approved_groups_list_user",
            par: [
              {
                par: "approved_groups_id",
                va: selectedHandOver.value.approved_groups_id,
              },
              { par: "search", va: options.value.SearchTextUser },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      listUserApproved.value = data[0];

      options.value.loadingU = false;
    })
    .catch((error) => {});
};
const datalistsD = ref();
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
        datalistsD.value = obj.arrChils;
        datalistsDSave.value = obj.arrChils;

        expandListD(datalistsD.value);
      }
    })
    .catch((error) => {
      options.value.loading = false;
    });
};
const datalistsDSave = ref();
const expandListD = (data) => {
  for (let node of data) {
    expandedKeys.value[node.key] = true;
    expandNode(node);
  }
};
const expandNode = (node) => {
  if (node.children && node.children.length) {
    expandedKeys.value[node.key] = true;
    for (let child of node.children) {
      expandNode(child);
    }
  }
};
const swithViewGroups = ref(false);
const displayDialogUser = ref(false);

//Checkbox

const onCheckBoxD = (value) => {
  if (!value.is_default) {
    options.value.loading = true;
    let data = {
      IntID: value.approved_groups_id,
      TextID: value.module_fake.toString() + "",
      IntTrangthai: null,
      BitTrangthai: value.status,
    };
    if (
      store.state.user.is_super == true ||
      store.state.user.user_id == value.created_by ||
      (store.state.user.role_id == "admin" &&
        store.state.user.organization_id == value.organization_id)
    ) {
      axios
        .put(
          baseURL + "/api/sys_approved_groups/update_d_approved_group",
          data,
          config
        )
        .then((response) => {
          if (response.data.err != "1") {
            swal.close();
            toast.success("Sửa nhóm duyệt thành công!");
            loadData(false);
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
  } else {
    return;
  }
};
const onCheckBox = (value) => {
  options.value.loading = true;
  let data = {
    IntID: value.approved_groups_id,
    TextID: value.approved_groups_id + "",
    IntTrangthai: 1,
    BitTrangthai: value.status,
  };
  if (
    store.state.user.is_super == true ||
    store.state.user.user_id == value.created_by ||
    (store.state.user.role_id == "admin" &&
      store.state.user.organization_id == value.organization_id)
  ) {
    axios
      .put(
        baseURL + "/api/sys_approved_groups/update_s_approved_group",
        data,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa nhóm duyệt thành công!");
          loadData(false);
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
const renderTree = (data, id, name, title) => {
  let arrChils = [];
  let arrtreeChils = [];

  data
    .filter(
      (x) =>
        x.parent_id == null &&
        x.module_key=="M13"
    )
    .forEach((m, i) => {
      m.IsOrder = i + 1;
      let om = { key: m[id], data: m };
      const rrechildren = (mm, pid) => {
        let dts = data.filter((x) => x.parent_id == pid);
        if (dts.length > 0) {
          if (!mm.children) mm.children = [];
          dts.forEach((em) => {
            let om1 = { key: em[id], data: em };
            rrechildren(om1, em[id]);
            mm.children.push(om1);
          });
        }
      };
      rrechildren(om, m[id]);
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
var arrr = [];
const initTudien = () => {
  arrr = [];
  axios
    .post(
      baseURL + "/api/Notify/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_modules_listmodulestop",
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
      if (data.length > 0) {
        arrr = data[0];
      }
    })
    .catch((error) => {
      if (error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
      }
    });

  axios
    .post(
      baseURL + "/api/Modules/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_modules_listbymodule_id",
            par: [{ par: "module_id", va:235}],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
debugger
      if (data.length > 0) {
        let obj = renderTree(data, "module_id", "module_name", "module");
        listModules.value = obj.arrtreeChils;

      }
    })
    .catch((error) => {
      console.log(error);
    });
};
onMounted(() => {
  if (!checkURL(window.location.pathname, store.getters.listModule)) {
    //router.back();
  }
  initTudien();
  loadUser();
  loadOrganization(store.getters.user.organization_id);
  loadData(true);
  return {
    isFirst,
    options,
    danhMuc,
  };
});
</script>

<template>
  <div class="d-container flex">
    <div :style="swithViewGroups == false ? 'width:100%' : 'width:65%'">
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
          dataKey="approved_groups_id"
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
          paginatorTemplate="FirstPageLink PrevPageLink PageLinks  NextPageLink LastPageLink RowsPerPageDropdown"
          :rowsPerPageOptions="[20, 30, 50, 100, 200]"
          selectionMode="single"
        >
          <template #header>
            <div>
              <h3 class="module-title my-2 ml-1">
                <i class="pi pi-th-large"></i> Cấu hình nhóm duyệt ({{
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
                      (filterSCard != null ||
                        filterTCard != null ||
                        filterMCard != null) &&
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
                          Loại quy trình:
                        </div>
                        <Dropdown
                          v-model="filterSCard"
                          :options="listModules"
                          optionLabel="name"
                          optionValue="code"
                          placeholder="Chọn Loại quy trình"
                          panelClass="d-design-dropdown"
                          class="col-8 p-0"
                          :style="
                            filterSCard != null
                              ? 'border:2px solid #2196f3'
                              : ''
                          "
                        />
                      </div>
                      <div class="field col-12 md:col-12 flex">
                        <div class="col-4 p-0 align-items-center flex">
                          Loại duyệt:
                        </div>
                        <Dropdown
                          v-model="filterTCard"
                          panelClass="d-design-dropdown"
                          :options="listTCard"
                          :filter="true"
                          optionLabel="name"
                          optionValue="code"
                          style="width: calc(100% - 10rem)"
                          class="w-full"
                          placeholder="Người bàn giao"
                          :style="
                            filterTCard != null
                              ? 'border:2px solid #2196f3'
                              : ''
                          "
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
 
              </template>

              <template #end>
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
              </template>
            </Toolbar>
          </template>

          <Column
            class="align-items-center justify-content-center text-center"
            headerStyle="text-align:center;max-width:70px;height:50px"
            bodyStyle="text-align:center;max-width:70px; "
            field="STT"
            header="STT"
          >
          </Column>
          <Column
            headerStyle="text-align:center;height:50px;  "
            bodyStyle="text-align:left; "
            field="approved_group_name"
            headerClass="align-items-center justify-content-center text-center"
            header="Nhóm duyệt"
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
            <template #body="slotProps">
              <div>
                <div class="mb-2">
                  {{ slotProps.data.approved_group_name }}
                </div>
                <div v-if="slotProps.data.signusers">
                  <AvatarGroup
                    v-if="
                      slotProps.data.signusers &&
                      slotProps.data.signusers.length > 0
                    "
                  >
                    <Avatar
                      v-for="(item, index) in slotProps.data.signusers.slice(
                        0,
                        3
                      )"
                      v-bind:label="
                        item.avatar ? '' : item.last_name.substring(0, 1)
                      "
                      v-bind:image="
                        item.avatar
                          ? basedomainURL + item.avatar
                          : basedomainURL + '/Portals/Image/noimg.jpg'
                      "
                      v-tooltip.bottom="{
                        value:
                          item.full_name +
                          '<br/>' +
                          item.position_name +
                          '<br/>' +
                          item.department_name,
                        escape: true,
                      }"
                      :key="item.user_id"
                      style="
                        border: 2px solid white;
                        color: white;
                        width: 2.5rem;
                        height: 2.5rem;
                      "
                      @error="basedomainURL + '/Portals/Image/noimg.jpg'"
                      size="large"
                      shape="circle"
                      class="cursor-pointer"
                      :style="{ backgroundColor: bgColor[index % 7] }"
                    />
                    <Avatar
                      v-if="
                        slotProps.data.signusers &&
                        slotProps.data.signusers.length > 3
                      "
                      v-bind:label="
                        '+' + (slotProps.data.signusers.length - 3).toString()
                      "
                      shape="circle"
                      size="large"
                      style="background-color: #2196f3; color: #ffffff"
                      class="cursor-pointer"
                    />
                  </AvatarGroup>
                </div>
              </div>
            </template>
          </Column>

          <Column
            class="align-items-center justify-content-center text-center"
            headerStyle="text-align:center;height:50px ;max-width:400px;"
            bodyStyle="text-align:center;max-width:400px;"
            field="module"
            header="Modules"
          >
            <template #body="data">
              <div class="w-full">
                <TreeSelect
                  panelClass="d-design-dropdown  d-tree-input d-tree-border"
                  class="w-full p-0 sel-placeholder d-tree-input d-tree-border"
                  v-model="data.data.module_fake"
                  :options="listModules"
                  selectionMode="checkbox"
                  optionLabel="data.module_name"
                  optionValue="data.module_id"
                  display="chip"
                ></TreeSelect>
              </div>
            </template>
          </Column>
          <Column
            class="align-items-center justify-content-center text-center"
            headerStyle="text-align:center;height:50px;max-width:200px;"
            bodyStyle="text-align:center;max-width:200px; ;"
            header="Loại duyệt"
            headerClass="textoneline"
          >
            <template #body="data">
              <div>
                {{
                  data.data.approved_type == 1
                    ? "Duyệt một nhiều"
                    : data.data.approved_type == 2
                    ? "Duyệt tuần tự"
                    : "Duyệt ngẫu nhiên"
                }}
              </div>
            </template>
          </Column>

          <Column
            class="align-items-center justify-content-center text-center"
            headerStyle="text-align:center;max-width:200px;height:50px"
            bodyStyle="text-align:center;max-width:200px; "
            header="Duyệt theo phòng ban"
            headerClass="textoneline"
          >
            <template #body="data">
              <Checkbox
                :disabled="true"
                :binary="data.data.is_department"
                v-model="data.data.is_department"
              />
            </template>
          </Column>
          <Column
            class="align-items-center justify-content-center text-center"
            headerStyle="text-align:center;max-width:150px;height:50px"
            bodyStyle="text-align:center;max-width:150px; "
            field="status"
            headerClass="textoneline"
            header="Trạng thái"
          >
            <template #body="data">
              <Checkbox
                :disabled="
                  !(
                    store.state.user.is_super == true ||
                    store.state.user.user_id == data.data.created_by ||
                    (store.state.user.role_id == 'admin' &&
                      store.state.user.organization_id ==
                        data.data.organization_id)
                  ) || ( data.data.is_local==false ||  data.data.is_local==null)
                "
                :binary="data.data.status"
                v-model="data.data.status"
                @click="onCheckBox(data.data)"
              />
             
            </template>
          </Column>

          <Column
            class="align-items-center justify-content-center text-center"
            headerStyle="text-align:center;max-width:150px;height:50px"
            bodyStyle="text-align:center;max-width:150px"
            header="Chức năng"
            headerClass="textoneline"
          >
            <template #body="data">
              <div
                v-if="
                data.data.is_local ==true && ( store.state.user.is_super == true ||
                  store.state.user.user_id == data.data.created_by ||
                  (store.state.user.role_id == 'admin' &&
                    store.state.user.organization_id ==
                      data.data.organization_id))  
                "
              >
                <Button
                  v-tooltip.top="'Sửa'"
                  @click="editCard(data.data)"
                  class="p-button-rounded p-button-secondary p-button-outlined mx-1"
                  type="button"
                  icon="pi pi-pencil"
                ></Button>
                <Button
                  v-tooltip.top="'Xóa'"
                  class="p-button-rounded p-button-secondary p-button-outlined mx-1"
                  type="button"
                  icon="pi pi-trash"
                  @click="delCard(data.data)"
                ></Button>
              </div>
            </template>
          </Column>
          <template #empty>
            <div
              class="align-items-center justify-content-center p-4 text-center m-auto"
              v-if="!isFirst"
            >
              <img src="../../../assets/background/nodata.png" height="144" />
              <h3 class="m-1">Không có dữ liệu</h3>
            </div>
          </template>
        </DataTable>
      </div>
    </div>
    <div v-if="swithViewGroups" style="width: 35%">
      <DataView
        v-if="!typeUserApp"
        class="w-full h-full e-sm flex flex-column"
        responsiveLayout="scroll"
        :scrollable="true"
        layout="list"
        :lazy="true"
        :value="listUserApproved"
        :loading="options.loadingU"
      >
        <template #header>
          <div class="pt-2 pb-3 text-lg">Danh sách người duyệt</div>

          <Toolbar class="custoolbar p-0">
            <template #start>
              <span class="p-input-icon-left">
                <i class="pi pi-search" />
                <InputText
                  type="text"
                  class="p-inputtext-sm"
                  spellcheck="false"
                  placeholder="Tìm kiếm"
                  v-model="options.SearchTextUser"
                  @keyup.enter="listUser(options.SearchTextUser)"
                />
              </span>
            </template>
            <template #end>
              <div></div>
            </template>
          </Toolbar>
        </template>
        <template #list="slotProps">
          <div class="grid w-full p-2">
            <div
              class="field col-12 flex m-0 cursor-pointer align-items-center"
            >
              <div class="col-1 p-0 align-items-center">
                <Avatar
                  v-bind:label="
                    slotProps.data.avatar
                      ? ''
                      : slotProps.data.full_name.substring(
                          slotProps.data.full_name.lastIndexOf(' ') + 1,
                          slotProps.data.full_name.lastIndexOf(' ') + 2
                        )
                  "
                  :image="basedomainURL + slotProps.data.avatar"
                  class="w-3rem"
                  size="large"
                  :style="
                    slotProps.data.avatar
                      ? 'background-color: #2196f3'
                      : 'background:' +
                        bgColor[slotProps.data.full_name.length % 7]
                  "
                  shape="circle"
                  @error="
                    $event.target.src =
                      basedomainURL + '/Portals/Image/nouser1.png'
                  "
                />
              </div>
              <div class="col-11 p-0 pl-3 align-items-center">
                <div class="pt-2">
                  <div class="font-bold">
                    {{ slotProps.data.full_name }}
                  </div>
                  <div class="flex w-full text-sm font-italic text-500">
                    <div>{{ slotProps.data.user_id }}</div>
                    <div v-if="slotProps.data.phone" class="">
                      <span class="px-2">|</span>{{ slotProps.data.phone }}
                    </div>
                    <div v-if="slotProps.data.email" class="">
                      <span class="px-2">|</span>{{ slotProps.data.email }}
                    </div>
                  </div>
                  <div class="flex w-full text-sm font-italic text-500">
                    {{ slotProps.data.department_name }}
                  </div>
                </div>
              </div>
            </div>
          </div>
        </template>
      </DataView>
      <DataView
        v-else
        class="w-full h-full e-sm flex flex-column"
        responsiveLayout="scroll"
        :scrollable="true"
        layout="list"
        :lazy="true"
        :value="listUserApproved"
        :loading="options.loadingU"
      >
        <template #header>
          <div class="pt-2 pb-3 text-lg">
            Danh sách người duyệt theo phòng ban
          </div>

          <Toolbar class="custoolbar p-0">
            <template #start>
              <span class="p-input-icon-left">
                <i class="pi pi-search" />
                <InputText
                  type="text"
                  class="p-inputtext-sm"
                  spellcheck="false"
                  placeholder="Tìm kiếm"
                  v-model="options.SearchTextUser"
                  @keyup.enter="listUser(options.SearchTextUser)"
                />
              </span>
            </template>
            <template #end>
              <div></div>
            </template>
          </Toolbar>
        </template>
        <template #list="slotProps">
          <div class="grid w-full p-2">
            <div class="col-12 p-2 m-0">
              <div class="flex w-full font-bold align-items-center text-lg">
                <i class="pi pi-angle-double-right pr-2"></i>
                {{ slotProps.data.department_name }}
              </div>
            </div>
            <div
              class="field col-12 p-0 pl-5 flex m-0 cursor-pointer align-items-center"
            >
              <div class="col-1 p-0 align-items-center">
                <Avatar
                  v-bind:label="
                    slotProps.data.avatar
                      ? ''
                      : slotProps.data.full_name.substring(
                          slotProps.data.full_name.lastIndexOf(' ') + 1,
                          slotProps.data.full_name.lastIndexOf(' ') + 2
                        )
                  "
                  :image="basedomainURL + slotProps.data.avatar"
                  size="large"
                  :style="
                    slotProps.data.avatar
                      ? 'background-color: #2196f3'
                      : 'background:' +
                        bgColor[slotProps.data.full_name.length % 7]
                  "
                  shape="circle"
                  @error="
                    $event.target.src =
                      basedomainURL + '/Portals/Image/nouser1.png'
                  "
                />
              </div>
              <div class="col-11 p-0 pl-2 align-items-center ml-2">
                <div class="pt-2">
                  <div class="font-bold">
                    {{ slotProps.data.full_name }}
                  </div>
                  <div class="flex w-full text-sm font-italic text-500">
                    <div>{{ slotProps.data.user_id }}</div>
                    <div v-if="slotProps.data.phone" class="">
                      <span class="px-2">|</span>{{ slotProps.data.phone }}
                    </div>
                    <div v-if="slotProps.data.email" class="">
                      <span class="px-2">|</span>{{ slotProps.data.email }}
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </template>
      </DataView>
    </div>
  </div>

  <Dialog
    @hide="closeDialog"
    :header="headerDialog"
    v-model:visible="displayBasic"
    :maximizable="true"
    :style="{ width: '35vw' }"
    :modal="true"
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="col-12 field flex p-0 pb-2 align-items-center">
          <div class="col-3 p-0">
            Tên nhóm duyệt<span class="redsao pl-1"> (*)</span>
          </div>
          <div class="col-9 p-0">
            <InputText
              v-model="sys_approved_groups.approved_group_name"
              class="w-full"
              :class="{
                'p-invalid': v$.approved_group_name.$invalid && submitted,
              }"
            />
          </div>
        </div>
        <div
          v-if="
            (v$.approved_group_name.$invalid && submitted) ||
            v$.approved_group_name.$pending.$response
          "
          class="col-12 field p-0 flex"
        >
          <div class="col-3 p-0"></div>
          <small class="col-9 p-0">
            <span style="color: red" class="w-full">{{
              v$.approved_group_name.required.$message
                .replace("Value", "Tên nhóm duyệt")
                .replace("is required", "không được để trống!")
            }}</span>
          </small>
        </div>

        <div
          class="field p-0 col-12 pb-2 md:col-12 flex"
          v-if="!sys_approved_groups.is_department"
        >
          <div class="col-3 p-0 align-items-center flex">Loại duyệt</div>
          <Dropdown
            v-model="sys_approved_groups.approved_type"
            :options="listTCard"
            optionLabel="name"
            optionValue="code"
            placeholder="--- Chọn loại duyệt ---"
            panelClass="d-design-dropdown"
            class="col-9 p-0 sel-placeholder"
          />
        </div>

        <div class="field p-0 col-12 pb-2 md:col-12 flex">
          <div class="col-3 p-0 align-items-center flex">
            Module <span class="redsao pl-1"> (*)</span>
          </div>
          <TreeSelect
            panelClass="d-design-dropdown  d-tree-input"
            class="col-9 p-0 sel-placeholder d-tree-input"
            placeholder="--- Chọn Module ---"
            v-model="sys_approved_groups.module_fake"
            :options="listModules"
            @change="onChangeModule($event)"
            :showClear="true"
            selectionMode="checkbox"
            optionLabel="data.module_name"
            optionValue="data.module_id"
            display="chip"
            :class="{
              'p-invalid': !sys_approved_groups.module_fake && submitted,
            }"
          ></TreeSelect>
        </div>
        <div
          v-if="!sys_approved_groups.module_fake && submitted"
          class="col-12 field p-0 flex"
        >
          <div class="col-3 p-0"></div>
          <small calss="col-9 p-0">
            <span style="color: red" class="w-full"
              >Module không được để trống!</span
            >
          </small>
        </div>

        <div class="col-12 p-0 field flex align-items-center">
          <!-- <div class="col-6 p-0 flex align-items-center">
            <div class="col-6 text-left p-0">Thứ tự duyệt</div>
            <div class="col-6 p-0">
              <InputNumber
                v-model="sys_approved_groups.is_order"
                class="w-full"
              />
            </div>
          </div> -->
          <div class="col-6 p-0 flex align-items-center">
            <div class="col-6 text-left p-0">Trả người duyệt trước</div>
            <div class="col-6 p-0">
              <InputSwitch
                v-model="sys_approved_groups.is_return_pre"
                class="w-4rem lck-checked"
              />
            </div>
          </div>
          <div class="col-6 p-0 flex align-items-center">
            <div class="col-6 text-left p-0 pl-3">Trả lại người tạo</div>
            <div class="col-6 p-0">
              <InputSwitch
                v-model="sys_approved_groups.is_return_created"
                class="ml-3 w-4rem lck-checked"
              />
            </div>
          </div>
        </div>
        <div class="col-12 p-0 field flex align-items-center">
          <div class="col-6 p-0 flex align-items-center">
            <div class="col-6 text-left p-0">Duyệt phòng ban</div>
            <div class="col-6 p-0">
              <InputSwitch
                @change="onChangeSWT"
                v-model="sys_approved_groups.is_department"
                class="ml-0 w-4rem lck-checked"
              />
            </div>
          </div>
          <div class="col-6 p-0 flex align-items-center">
            <div class="col-6 text-left p-0 pl-3">Trạng thái</div>
            <div class="col-6 p-0">
              <InputSwitch
                v-model="sys_approved_groups.status"
                class="ml-3 w-4rem lck-checked"
              />
            </div>
          </div>
        </div>
        <div
          v-if="sys_approved_groups.is_department"
          class="field p-0 pb-2 col-12 md:col-12 flex"
        >
          <div
            class="col-6 p-0 flex align-items-center cursor-pointer text-blue-500"
            @click="showListAssets"
          >
            <i class="pi pi-plus-circle pr-2"></i> Cấu hình người duyệt theo
            phòng ban
          </div>
        </div>
        <div class="field p-0 pb-2 col-12 md:col-12 flex" v-else>
          <div
            class="col-6 p-0 flex align-items-center cursor-pointer text-blue-500"
            @click="showListUser"
          >
            <i class="pi pi-plus-circle pr-2"></i> Cấu hình người duyệt
          </div>
        </div>
        <div
          v-if="!sys_approved_groups.is_department"
          class="field p-0 pb-2 col-12 md:col-12"
        >
          <!-- style="display: grid; grid-template-columns: repeat(2, 1fr)" -->
          <OrderList
            v-model="listUserA"
            listStyle="height:auto"
            class="w-full"
            dataKey="id"
          >
            <template #header> Danh sách người duyệt </template>
            <template #item="slotProps">
              <Toolbar class="surface-0 m-0 p-0 border-0 w-full">
                <template #start>
                  <div class="flex align-items-center">
                    <div class="format-flex-center">
                      <b class="p-3">{{ slotProps.index + 1 }}
                      
                   </b>
                    </div>
                    <div class="flex">
                      <Avatar
                        v-bind:label="
                          slotProps.item.avatar
                            ? ''
                            : slotProps.item.full_name.substring(
                                slotProps.item.full_name.lastIndexOf(' ') + 1,
                                slotProps.item.full_name.lastIndexOf(' ') + 2
                              )
                        "
                        :image="basedomainURL + slotProps.item.avatar"
                        class="w-2rem h-2rem"
                        size="large"
                        :style="
                          slotProps.item.avatar
                            ? 'background-color: #2196f3'
                            : 'background:' +
                              bgColor[slotProps.item.full_name.length % 7]
                        "
                        shape="circle"
                        @error="
                          $event.target.src =
                            basedomainURL + '/Portals/Image/nouser1.png'
                        "
                      />
                      <div class="pt-1 pl-2">
                        {{ slotProps.item.full_name }}
                      </div>
                    </div>
                  </div>
                </template>
                <template #end>
                  <div>
                    <Button
                      icon="pi pi-trash"
                      class="p-button-rounded p-button-secondary p-button-text ml-1"
                      @click="removeListUser(slotProps.item)"
                    ></Button>
                  </div>
                </template>
              </Toolbar>
            </template>
          </OrderList>
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
        @click="saveHandover(!v$.$invalid)"
        label="Lưu"
        icon="pi pi-check"
        autofocus
      />
    </template>
  </Dialog>
  <Dialog
    header="Cập nhật nhóm duyệt phòng ban"
    v-model:visible="displayAssets"
    :maximizable="true"
    :style="{ width: '50vw' }"
    :modal="true"
  >
    <div>
      <div class="true flex-grow-1 p-2" id="scrollTop">
        <div class="grid p-0">
          <div class="col-12 field format-center">
            <TreeTable :expandedKeys="expandedKeys" :value="datalistsD">
              <!-- <Column 
              bodyClass="w-3rem"
              headerStyle="text-align:center;width:50px"
              bodyStyle="text-align:center;width:50px" :expander="true"></Column> -->
              <Column class="w-7" field="organization_name" :expander="true">
                <template #header>
                  <div class="w-full format-center">
                    <i class="pi pi-building pr-2"></i> Phòng ban
                  </div>
                </template>
              </Column>
              <Column class="w-5">
                <template #header>
                  <div class="w-full format-center">
                    <i class="pi pi-user pr-2"></i> Người duyệt
                  </div>
                </template>
                <template #body="data">
                  <div class="w-full flex align-items-center">
                    <div class="w-3rem p-0">
                      <Button
                        @click="showTreeUser(data.node.data)"
                        v-tooltip.top="'Chọn người duyệt'"
                        icon="pi pi-user-plus"
                        class="p-button-rounded"
                        style="width: 2.5rem; height: 2.5rem"
                      ></Button>
                    </div>
                    <div style="width: calc(100% - 3rem)" class="pl-1">
                      <MultiSelect
                        :options="listDropdownUserGive"
                        :filter="true"
                        :showClear="true"
                        :editable="false"
                        display="chip"
                        v-model="data.node.data.userM"
                        optionLabel="name"
                        optionValue="code"
                        placeholder="Chọn người duyệt "
                        panelClass="d-design-dropdown  d-tree-input"
                        class="p-0 w-full p-design-dropdown ip36"
                      >
                        <template #option="slotProps">
                          <div class="country-item flex align-items-center">
                            <div class="grid w-full p-0">
                              <div
                                class="field p-0 py-1 col-12 flex m-0 cursor-pointer align-items-center"
                              >
                                <div class="col-1 mx-2 p-0 align-items-center">
                                  <Avatar
                                    style="color: #fff"
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
                                <div class="col-11 p-0 ml-3 align-items-center">
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
                                  </div>
                                </div>
                              </div>
                            </div>
                          </div>
                        </template>
                      </MultiSelect>
                    </div>
                  </div>
                </template>
              </Column>
            </TreeTable>
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

.d-avatar-sys_approved_groups {
  position: relative;
  width: 100%;
  height: 350px;
}
.d-avatar-sys_approved_groups img {
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
