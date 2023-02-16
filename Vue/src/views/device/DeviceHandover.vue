<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import moment from "moment";
import treeuser from "../../components/user/treeuser.vue";
import detailsHandover from "../../components/device/detailsHandover.vue";
import detailsDevice from "../../components/device/detailsDevice.vue";
import { el } from "date-fns/locale";
import { encr, checkURL } from "../../util/function.js";
//Khai báo
const router = inject("router");

const cryoptojs = inject("cryptojs");
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
import printDocHandover from "./print/printDocHandover.vue";
import printListHandover from "./print/printListHandover.vue";
const basedomainURL = baseURL;
const selectedHandOver = ref();
const selectedCard = ref([]);
const checkDelList = ref(false);
const displayAssets = ref(false);
const isFirstCard = ref(false);
const selectedCardRP = ref();
watch(selectedHandOver, () => {
  if (selectedHandOver.value.length > 0) {
    checkDelList.value = true;
  } else {
    checkDelList.value = false;
  }
});
const rules = {
  handover_number: {
    required,
    $errors: [
      {
        $property: "handover_number",
        $validator: "required",
        $message: "Số phiếu không được để trống!",
      },
    ],
  },
};

const displayDialogUser = ref(false);

let checkTypeUse = null;
const showTreeUser = (value) => {
  checkMultile.value = true;
  selectedUser.value = [...listUserA.value];
  checkTypeUse = value;
  displayDialogUser.value = true;
};
const headerDialogUser = ref("Chọn người duyệt");
const closeDialogUser = () => {
  displayDialogUser.value = false;
};
const datalistsD = ref();
const checkMultile = ref(true);
const listUserA = ref([]);

const choiceUser = () => {
  if (checkMultile.value == true)
    selectedUser.value.forEach((element) => {
      if (checkTypeUse == 1) {
        device_handover.value.user_receiver_id = element.user_id;
        onChangeUser1(element.user_id);
      } else {
        device_handover.value.user_verifier_id = element.user_id;
        onChangeUser2(element.user_id);
      }
    });
  closeDialogUser();
};
const taskDateFilter = ref();
const listTypeP = ref([
  { name: "Bàn giao thiết bị", code: 1 },
  { name: "Bàn giao thiết bị cá nhân", code: 0 },
]);

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
const showListAssets = () => {
  filterSQLDM.value = [];
  checkShowAssets.value = true;

  loadDeviceType();
  loadDataSQLDM();
  displayAssets.value = true;
};
const displaySwitchAssets = ref(false);
const datalistsDRD = ref();
const selectedRepairHandover = ref();
const selectedRepairDevice = ref([]);
const onSelectRepairDevice = (event, check) => {
  dataselectRP.value = [];
  if (check == 1) {
    selectedRepairDevice.value = event.data;
  }
  if (check == 4) {
    selectedRepairDevice.value = event.data;
    selectedCard.value = [];
  }
  if (check == 3) {
    selectedCard.value = selectedCard.value.filter(
      (x) => x.repair_details_id != event.data.repair_details_id
    );
  }
  if (selectedRepairDevice.value)
    selectedRepairDevice.value.forEach((element) => {
      dataselectRP.value.push({
        pre: element,
        next: selectedCard.value.find(
          (el) => el.repair_details_id == element.repair_details_id
        ),
      });
    });
};
const onSelectRepairSW = (data, check) => {
  if (device_handover.value.device_repair_id != data.device_repair_id) {
    selectedCard.value = [];
  }

  device_handover.value.device_repair_id = data.device_repair_id;
  device_handover.value.user_receiver_id = data.repair_user_id;
  onChangeUser1(device_handover.value.user_receiver_id);
  datalistsDRD.value = [];

  dataselectRP.value = [];
  selectedRepairDevice.value = [];
  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "device_handover_repair_get",
            par: [{ par: "repair_id", va: data.device_repair_id }],
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
        element.STT = i + 1;
      });

      datalistsDRD.value = data;
      if (isSaveCard.value == false) {
        datalistsDRD.value = datalistsDRD.value.filter(
          (x) => x.is_replace == null
        );
        datalistsDRD.value.forEach((element, i) => {
          element.STT = i + 1;
        });
      }
      if (
        device_handover.value.device_repair_id != null &&
        isSaveCard.value == true &&
        check
      ) {
        datalistsDRD.value = [];
        data.forEach((element) => {
          selectedCard.value.forEach((item) => {
            if (element.repair_details_id == item.repair_details_id) {
              datalistsDRD.value.push(element);
              selectedRepairDevice.value.push(element);
              dataselectRP.value.push({
                pre: element,
                next: selectedCard.value.find(
                  (el) => el.repair_details_id == element.repair_details_id
                ),
              });
            } else if (element.is_replace == null) {
              datalistsDRD.value.push(element);
            }
          });
        });
      } else {
        selectedCard.value = [];
        datalistsDRD.value = datalistsDRD.value.filter(
          (x) => x.is_replace == null
        );
        datalistsDRD.value.forEach((element, i) => {
          element.STT = i + 1;
        });
      }
    })
    .catch((error) => {
      options.value.loading = false;
    });
};
const listDropdownRepair = ref();
const loadRepairHandover = (id) => {
  listDropdownRepair.value = [];

  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "device_repair_handover_list",
            par: [
              { par: "pageno", va: options.value.pagenoSW },
              { par: "pagesize", va: options.value.pagesizeSW },
              { par: "user_id", va: store.getters.user.user_id },
              { par: "status", va: 2 },
              { par: "search", va: options.value.searchSW },
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
      data.forEach((element, i) => {
        element.STT = i + 1;
        if (id != null) {
          if (element.countDevice > 0 || id == element.device_repair_id)
            listDropdownRepair.value.push({
              name: element.repair_number,
              code: element,
            });

          if (element.device_repair_id == id) {
            selectedRepairHandover.value = element;
            if (selectedRepairHandover.value)
              onSelectRepairSW(selectedRepairHandover.value, true);
          }
        } else {
          if (element.countDevice > 0) {
            listDropdownRepair.value.push({
              name: element.repair_number,
              code: element,
            });
          }
        }
      });
      datalistsSW.value = data;

      options.value.totalRecordsSW = data1[0].totalRecord;
    })
    .catch((error) => {
      options.value.loading = false;
    });
};
const datalistsSW = ref([]);

const closeSwitchRepairReal = () => {
  selectedCard.value = [];
  displaySwitchAssets.value = false;
};
const closeSwitchRepair = () => {
  displaySwitchAssets.value = false;
};
const displayReplaceAssets = ref(false);
const dataselectRP = ref([]);
const keySave = ref();
const showReplaceAssets = (value) => {
  keySave.value = value.repair_details_id;
  options.value.loadingRP = true;
  filterSQLRP.value = [];
  if (selectedCard.value.length > 0) {
    selectedCard.value.forEach((element) => {
      let filterS = {
        filterconstraints: [{ value: element.card_id, matchMode: "notEquals" }],
        filteroperator: "and",
        key: "card_id",
      };
      filterSQLRP.value.push(filterS);
    });
  }
  loadDataSQLRP(value);
  displayReplaceAssets.value = true;
};
const closeReplaceRepair = () => {
  if (!selectedCardRP.value.repair_details_id) {
    selectedCardRP.value.repair_details_id = keySave.value;
  }
  selectedCard.value.push(selectedCardRP.value);
  dataselectRP.value.forEach((element) => {
    if (element.pre.repair_details_id == keySave.value) {
      element.next = selectedCardRP.value;
    }
  });
  displayReplaceAssets.value = false;
};
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
        key: "handover_created_date",
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
        key: "handover_created_date",
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
        key: "handover_created_date",
      };
      filterSQL.value.push(filterS1);
    }
  }
  isDynamicSQL.value = true;
  loadData(true);
};

//Xóa tin tức

const delCard = (Card) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá cấp phát này không!",
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
          .delete(baseURL + "/api/device_handover/delete_device_handover", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: Card != null ? [Card.handover_id] : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá cấp phát thành công!");
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
  handover_number: {
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
  handover_created_date: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.DATE_BEFORE }],
  },
  handover_created_date: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.DATE_AFTER }],
  },
});
//Phân trang dữ liệu
const onPage = (event) => {
  options.value.pagesize = event.rows;
  options.value.pageno = event.page;
  loadData();
};
const onPageRP = (event) => {
  options.value.pagesizeRP = event.rows;
  options.value.pagenoRP = event.page;
  loadDataSQLRP();
};

const filtersRP = ref({
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

const onSortRP = (event) => {
  if (event.sortField == null) {
    options.value.sortRP = "card_id DESC";
    loadDataSQLRP();
  } else {
    options.value.sortRP =
      event.sortField + (event.sortOrder == 1 ? " ASC" : " DESC");
    if (event.sortField != "card_id") {
      options.value.sort +=
        ",card_id " + (event.sortOrder == 1 ? " ASC" : " DESC");
    }

    loadDataSQLRP();
  }
};
const onFilterRP = (event) => {
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

  options.value.pagenoRP = 0;
  options.value.id = null;

  loadDataSQLRP();
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
    .post(baseURL + "/api/SQL/Filter_device_handover", data, config)
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
  handover_number: "",
  image: "",

  barcode_id: "",
  barcode_type: 0,
  barcode_image: "",
  status: 0,
  not_used: false,
  used: true,
});
const onChangeUser1 = (value) => {
  device_handover.value.user_receiver_name = listDropdownUser.value.filter(
    (x) => x.code == value
  )[0].name;
  device_handover.value.user_receiver_position = listDropdownUser.value.filter(
    (x) => x.code == value
  )[0].position_name;
  device_handover.value.user_receiver_department_id =
    listDropdownUser.value.filter((x) => x.code == value)[0].department_id;
  device_handover.value.user_receiver_department_name =
    listDropdownUser.value.filter((x) => x.code == value)[0].department_name;

  if (device_handover.value.user_receiver_id)
    listDropdownUserCheck.value = listDropdownUser.value.filter(
      (x) => x.code != device_handover.value.user_receiver_id
    );
};
const onChangeUser2 = (value) => {
  device_handover.value.user_verifier_name = listDropdownUserCheck.value.filter(
    (x) => x.code == value
  )[0].name;
  device_handover.value.user_verifier_position =
    listDropdownUserCheck.value.filter((x) => x.code == value)[0].position_name;
  device_handover.value.user_verifier_department_id =
    listDropdownUserCheck.value.filter((x) => x.code == value)[0].department_id;
  device_handover.value.user_verifier_department_name =
    listDropdownUserCheck.value.filter(
      (x) => x.code == value
    )[0].department_name;
  if (device_handover.value.user_verifier_id)
    listDropdownUserGive.value = listDropdownUser.value.filter(
      (x) => x.code != device_handover.value.user_verifier_id
    );
};
const filterSQLDM = ref([]);
const checkTypeHO = ref(false);
const filterSQLRP = ref([]);
const datalistsRP = ref();
const filterCardWarehouseRP = ref();
const loadDataSQLRP = (dataSQL) => {
  datalistsRP.value = [];

  filterSQLDM.value = [];
  let filterS = {
    filterconstraints: [],
    filteroperator: "or",
    key: "warehouse_id",
  };
  if (listWarehouse.value.length > 0) {
    listWarehouse.value.forEach((element) => {
      filterS.filterconstraints.push({
        value: element.warehouse_id,
        matchMode: "equals",
      });
    });

    filterSQLDM.value.push(filterS);
  }

  let filterSs = {
    filterconstraints: [{ value: "TK", matchMode: "equals" }],
    filteroperator: "and",
    key: "status",
  };
  filterSQLRP.value.push(filterSs);
  let filterS1 = {
    filterconstraints: [{ value: dataSQL.device_type_id, matchMode: "equals" }],
    filteroperator: "and",
    key: "device_type_id",
  };
  filterSQLRP.value.push(filterS1);
  // if (filterCardWarehouseRP.value != null) {
  //   let filterS3 = {
  //     filterconstraints: [
  //       { value: filterCardWarehouseRP.value, matchMode: "equals" },
  //     ],
  //     filteroperator: "and",
  //     key: "warehouse_id",
  //   };
  //   filterSQLRP.value.push(filterS3);
  // }
  let data = {
    sqlS: "True",
    sqlO: options.value.sortRP,
    Search: options.value.SearchTextRP,
    PageNo: options.value.pagenoRP,
    PageSize: options.value.pagesizeRP,
    sqlF: null,
    fieldSQLS: filterSQLRP.value,
    next: true,
    id: "card_id",
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
            options.value.pagenoRP * options.value.pagesizeRP + i + 1;
        });

        datalistsRP.value = data;

        // if (selectedCard.value.length > 0) {
        //   let arr = data;
        //   selectedCard.value.forEach((element) => {
        //     arr = arr.filter((x) => x.card_id != element.card_id);
        //   });
        //   datalistsRP.value = [arr, selectedCard.value];
        // }
      } else {
        datalistsRP.value = [];
      }
      options.value.loading = false;
      options.value.loadingRP = false;
      //Show Count nếu có
      if (dt.length == 2) {
        options.value.totalRecordsRP = dt[1][0].totalRecords;
      }
    })
    .catch((error) => {
      options.value.loadingRP = false;

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
const loadDataSQLDM = () => {
  datalistsDM.value = [];

  if (!checkTypeHO.value) {
    filterSQLDM.value = [];
    let filterS = {
      filterconstraints: [],
      filteroperator: "or",
      key: "warehouse_id",
    };
    if (listWarehouse.value.length > 0) {
      listWarehouse.value.forEach((element) => {
        filterS.filterconstraints.push({
          value: element.warehouse_id,
          matchMode: "equals",
        });
      });

      filterSQLDM.value.push(filterS);
      if (checkShowAssets.value) {
        filterSQLDM.value.push({
          filterconstraints: [
            {
              value: "TK",
              matchMode: "equals",
            },
          ],
          filteroperator: "and",
          key: "status",
        });
      }
    } else {
      datalistsDM.value = [];
      return;
    }
  } else {
    let filterS = {
      filterconstraints: [
        { value: store.getters.user.user_id, matchMode: "equals" },
      ],
      filteroperator: "and",
      key: "device_user_id",
    };
    filterSQLDM.value.push(filterS);
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
  if (options.value.warehouse_id != null) {
    let filterS = {
      filterconstraints: [
        { value: options.value.warehouse_id, matchMode: "equals" },
      ],
      filteroperator: "and",
      key: "warehouse_id",
    };
    filterSQLDM.value.push(filterS);
  }

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
    if (event.sortField != " handover_id") {
      options.value.sort +=
        ", handover_id " + (event.sortOrder == 1 ? " ASC" : " DESC");
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
//Xóa nhiều
const deleteList = () => {
  let listId = new Array(selectedHandOver.value.length);
  let check = false;

  selectedHandOver.value.forEach((element) => {
    if (element.status != 0) {
      swal.fire({
        title: "Error!",
        text:
          element.status == 1
            ? "Không được xóa phiếu cấp phát có trạng thái: Chờ xác nhận"
            : element.status == 2
            ? "Không được xóa phiếu cấp phát có trạng thái:  Đã xác nhận"
            : "Không được xóa phiếu cấp phát có trạng thái: Trả lại",
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
        text: "Bạn có muốn xóa danh sách cấp phát này không!",
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
          selectedHandOver.value.forEach((item) => {
            listId.push(item.handover_id);
          });
          axios
            .delete(baseURL + "/api/device_handover/delete_device_handover", {
              headers: { Authorization: `Bearer ${store.getters.token}` },
              data: listId != null ? listId : 1,
            })
            .then((response) => {
              swal.close();
              if (response.data.err != "1") {
                swal.close();
                toast.success("Xoá cấp phát thành công!");
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
const filesList = ref([]);

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
  sort: " handover_id DESC",
  sortDM: "card_id DESC",
  sortRP: "card_id DESC",
  search: "",
  pageno: 0,
  pagesize: 20,
  pagenoDM: 0,
  pagesizeDM: 10,
  pagenoSW: 0,
  pagesizeSW: 10,
  pagenoRP: 0,
  pagesizeRP: 20,
  loading: true,
  totalRecords: null,
  totalRecordsDM: null,
  start_date: null,
  end_date: null,
  next: true,
});
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
  device_department_id_fake: {},
  user_department: {},
});
const v$ = useVuelidate(rules, device_handover);
const danhMuc = ref();

//METHOD

const hideSelectDevice = () => {
  selectedDeviceMain.value = {
    is_order: 1,
    proposal_code: "",
    device_id: null,
    handover_number: "",
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
    datalistsDM.value[1].forEach((element) => {
      element.serial = element.barcode_id;
      element.condition = element.status;
      element.note = element.device_des;

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
const displayDetails = ref(false);
const listAssetsH = ref([]);
const openDetails = (handover_id) => {
  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "device_handover_card_list",
            par: [{ par: "handover_id", va: handover_id }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);

      listAssetsH.value = data[0];
      displayDetails.value = true;
    })
    .catch((error) => {});
};

const menuCog = ref();
const dataCog = ref();
const toggleCog = (event, value) => {
  menuCog.value.toggle(event);
  dataCog.value = value;
};

const itemsCog = ref([
  {
    label: "Sửa phiếu",
    icon: "pi pi-pencil",
    command: () => {
      editCard(dataCog.value);
    },
  },
  {
    label: "Xóa phiếu",
    icon: "pi pi-trash",
    command: () => {
      delCard(dataCog.value);
    },
  },
]);

const openDetailsHandover = (data) => {
  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "device_handover_get",
            par: [{ par: "handover_id", va: data.handover_id }],
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
      device_handover.value = data[0];
      displayDetailsHandover.value = true;
      listAssetsH.value = data1;
      listFilesS.value = data2;
    })
    .catch((error) => {});
};
const device_repair = ref();
const closeDetailsHandoverRepair = () => {
  displayDetailsHandoverRepair.value = false;
  device_repair.value = null;
};
const closeDetailsHandover = () => {
  displayDetailsHandover.value = false;
  device_handover.value = null;
};
const displayDetailsHandover = ref(false);
const dataPropsD = ref();
const displayDetailsDevice = ref(false);

let fileSize = [];
const onUploadFile = (event) => {
  fileSize = [];
  filesList.value = [];

  var ms = false;

  event.files.forEach((fi) => {
    let formData = new FormData();
    formData.append("fileupload", fi);
    axios({
      method: "post",
      url: baseURL + `/api/chat/ScanFileUpload`,
      data: formData,
      headers: {
        Authorization: `Bearer ${store.getters.token}`,
      },
    })
      .then((response) => {
        if (response.data.err != "1") {
          if (fi.size > 100 * 1024 * 1024) {
            ms = true;
          } else {
            filesList.value.push(fi);
            fileSize.push(fi.size);
          }
        } else {
          filesList.value = filesList.value.filter((x) => x.name != fi.name);
          swal.fire({
            title: "Cảnh báo",
            text: "File bị xóa do tồn tại mối đe dọa với hệ thống!",
            icon: "warning",
            confirmButtonText: "OK",
          });
        }
        if (ms) {
          swal.fire({
            icon: "warning",
            type: "warning",
            title: "Thông báo",
            text: "Bạn chỉ được upload file có dung lượng tối đa 100MB!",
          });
        }
      })
      .catch(() => {
        swal.fire({
          title: "Thông báo",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
      });
  });
};
const removeFile = (event) => {
  filesList.value = filesList.value.filter((a) => a != event.file);
};
const deleteFileH = (value) => {
  listFilesS.value = listFilesS.value.filter(
    (x) => x.handover_files_id != value.handover_files_id
  );
};
const deleteFileD = (value) => {
  selectedCard.value = selectedCard.value.filter(
    (x) => x.card_id != value.card_id
  );
};
const loadCount = () => {
  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "device_handover_count",
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
    .catch((error) => {});
};
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

function renderhtml(id, htmltable) {
  htmltable = "";

  htmltable += `<style>
 
    .formword-d  {
      background: #fff !important;
    }
    .formword-d * {
      font-family: "Times New Roman", Times, serif !important;
      font-size: 16pt;
    }
    .title1,
    .title1 * {
      font-size: 24pt !important;
    }
    .title2,
    .title2 * {
      font-size: 20pt !important;
    }
    .title3,
    .title3 * {
      font-size: 18pt !important;
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
  htmltable +=
    " <div class='formword-d '>  <table  border='0' width='1024' cellpadding='0'> <thead> <tr> ";
  htmltable +=
    "<td   colspan='2' style='width: 40%; vertical-align: bottom ;text-align:center' >";
  htmltable +=
    "    <div>BỘ QUỐC PHÒNG</div>";
  htmltable +=
    "    <div  > <b>BẢO HIỂM XÃ HỘI</b> <div   style='text-align:center;border-top: 1.5px solid #000; margin: 0px 110px'></div></div></td>";
  htmltable +=
    "   <td  colspan='4' style='min-width: 40%; vertical-align: bottom;text-align:center' >";
  htmltable +=
    "  <div  > <b>CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM</b> </div>";
  htmltable += "    <div><b>Độc lập - Tự do - Hạnh phúc</b></div>";
  htmltable +=
    "     <div   style='text-align:center;border-top: 1.5px solid #000; margin: 0px 180px'  ></div>  </td>  </tr>";
  htmltable +=
    "<tr> <td   colspan='2'>  <div style='text-align:center;padding: 1rem 0'>Số: " +
    device_handover.value.handover_number +
    "</div>  </td> <td class='text-center' colspan='4'>";
  htmltable += " <div style='text-align:center;padding:   0'> <i>Hà Nội, ngày ";
  var strD1 =
    new Date(device_handover.value.handover_created_date).getDate() > 9
      ? new Date(device_handover.value.handover_created_date).getDate()
      : "0" + new Date(device_handover.value.handover_created_date).getDate();
  htmltable += strD1;
  htmltable += ", tháng ";
  var strM =
    new Date(device_handover.value.handover_created_date).getMonth() + 1 > 9
      ? new Date(device_handover.value.handover_created_date).getMonth() + 1
      : "0" +
        (new Date(device_handover.value.handover_created_date).getMonth() + 1);
  htmltable += strM;

  htmltable +=
    ", năm " +
    new Date(device_handover.value.handover_created_date).getFullYear() +
    "</i>    </div>   </td>  </tr> </thead>  </table>";
 htmltable +=
    "<table border='0' width='1024' cellpadding='10'><tbody><tr><td><h3 style='text-align:center;padding-top:30px;font-size:20pt;font-weight:bold;'>BIÊN BẢN BÀN GIAO</h3></td></tr></tbody></table>";
 
  htmltable +=
    "<table border='0' width='1024' cellpadding='10'><tbody><tr><td><p style='font-size:16pt;'>Hôm nay, ngày ";
  var strD =
    new Date(device_handover.value.handover_created_date).getDate() > 9
      ? new Date(device_handover.value.handover_created_date).getDate()
      : "0" + new Date(device_handover.value.handover_created_date).getDate();
  htmltable += strD;
  htmltable += " tháng ";
  var strM =
    new Date(device_handover.value.handover_created_date).getMonth() + 1 > 9
      ? new Date(device_handover.value.handover_created_date).getMonth() + 1
      : "0" +
        (new Date(device_handover.value.handover_created_date).getMonth() + 1);
  htmltable += strM;
  var strM1 = device_handover.value.user_receiver_department_name
    ? device_handover.value.user_receiver_department_name
    : " đây ";
  var strM2 = device_handover.value.user_deliver_department_name
    ? device_handover.value.user_deliver_department_name
    : "";
  htmltable +=
    " năm " +
    new Date(device_handover.value.handover_created_date).getFullYear() +
    " tại " +
    strM1 +
    ", chúng tôi gồm:</p></td></tr></tbody></table>";
  htmltable +=
    "<table border='0' width='1024' cellpadding='10'><tbody><tr><td><p style='font-size:16pt; font-weight:bold;'>" +
    "A. BÊN GIAO: " +
    strM2 +
    "</p></td></tr></tbody></table>";
  htmltable +=
    "<table border='0' width='1024' cellpadding='10'><tbody><tr><td width='50%'><p style='font-size:16pt;'>Đồng chí: " +
    device_handover.value.user_deliver_name +
    "</p></td>";
      var strM5 = device_handover.value.user_deliver_position
    ? device_handover.value.user_deliver_position
    : "";
  htmltable +=
    "<td width='50%'><p style='font-size:16pt;'>Chức vụ: " +
    strM5 +
    "</p></td></tr>";
  htmltable += " </tbody></table>";
  var strM3 = device_handover.value.user_receiver_department_name
    ? device_handover.value.user_receiver_department_name
    : "";
  var strM4 = device_handover.value.user_verifier_department_name
    ? device_handover.value.user_verifier_department_name
    : "";
  htmltable +=
    "<table border='0' width='1024' cellpadding='10'><tbody><tr><td><p style='font-size:16pt; font-weight:bold;'>" +
    "B. BÊN NHẬN: " +
    strM3 +
    "</p></td></tr></tbody></table>";
  htmltable +=
    "<table border='0' width='1024' cellpadding='10'><tbody><tr><td width='50%'><p style='font-size:16pt;'>Đồng chí: " +
    device_handover.value.user_receiver_name +
    "</p></td>";
        var strM6 = device_handover.value.user_receiver_position
    ? device_handover.value.user_receiver_position
    : "";
  htmltable +=
    "<td width='50%'><p style='font-size:16pt;'>Chức vụ: " +
    strM6 +
    "</p></td></tr>";
  htmltable += " </tbody></table>";
  if (device_handover.value.handover_type == 1) {
    htmltable +=
      "<table border='0' width='1024' cellpadding='10'><tbody><tr><td><p style='font-size:16pt ;font-weight:bold;'>" +
      "C. BÊN XÁC NHẬN: " +
      strM4 +
      "</p></td></tr></tbody></table>";
    htmltable +=
      "<table border='0' width='1024' cellpadding='10'><tbody><tr><td width='50%'><p style='font-size:16pt;'>Đồng chí: " +
      device_handover.value.user_verifier_name +
      "</p></td>";
         var strM7 = device_handover.value.user_verifier_position
    ? device_handover.value.user_verifier_position
    : "";
    htmltable +=
      "<td width='50%'><p style='font-size:16pt;'>Chức vụ: " +
     strM7 +
      "</p></td></tr>";
    htmltable += " </tbody></table>";
  }
  htmltable +=
    "<table border='0' width='1024' cellpadding='10'><tbody><tr><td><p style='font-size:16pt;'>Bên giao tiến hành bàn giao các tài sản như sau:</p></td></tr></tbody></table>";
  htmltable +=
    "<table border='0' width='1024' cellpadding='10' style='border-spacing:0;padding-left:15px;padding-right:15px;'><thead><tr>";
  htmltable +=
    "<th align='center' width='30' style='border:1px solid #000 !important;font-size:14pt !important;'>STT</th>";
  htmltable +=
    "<th align='center' width='100' style='border:1px solid #000 !important;font-size:14pt !important;border-left:none !important;'>Số hiệu</th>";
  htmltable +=
    "<th align='center' width='100' style='border:1px solid #000 !important;font-size:14pt !important;border-left:none !important;'>Mã barcode</th>";
  htmltable +=
    "<th align='center' style='border:1px solid #000 !important;font-size:14pt !important;border-left:none !important;'>Tên thiết bị</th>";
  //htmltable += "<th align='center' width='100' style='border:1px solid #000 !important;font-size:14pt !important;border-left:none !important;'>Ngày mua</th>";
  htmltable +=
    "<th align='center' width='90' style='border:1px solid #000 !important;font-size:14pt !important;border-left:none !important;'>Đơn vị tính</th>";
  htmltable +=
    "<th align='center' width='90' style='border:1px solid #000 !important;font-size:14pt !important;border-left:none !important;'>Giá trị</th>";
  htmltable +=
    "<th align='center' width='150' style='border:1px solid #000 !important;font-size:14pt !important;border-left:none !important;'>Tình trạng</th>";
  htmltable += "</tr></thead>";
  htmltable += "<tbody>";
  var stt = 0;
  var sum = 0;
  listAssetsH.value.forEach(function (ts) {
    stt += 1;
    sum += ts.price;
    htmltable +=
      "<tr><td width='30' align='center' style='border:1px solid #000 !important;font-size:14pt !important;border-top:none !important;'>" +
      stt +
      "</td>";
    htmltable +=
      "<td width='100' align='center' style='border:1px solid #000 !important;font-size:14pt !important;border-left:none !important;border-top:none !important;'>" +
      ts.device_number +
      "</td>";
    htmltable +=
      "<td width='100' align='center' style='border:1px solid #000 !important;font-size:14pt !important;border-left:none !important;border-top:none !important;'>" +
      (ts.barcode_id != null ? ts.barcode_id : "") +
      "</td>";
    htmltable +=
      "<td align='left' style='border:1px solid #000 !important;font-size:14pt !important;border-left:none !important;border-top:none !important;'>" +
      ts.device_name +
      "</td>";
    //htmltable += "<td width='100' align='center' style='border:1px solid #000 !important;font-size:14pt !important;border-left:none !important;border-top:none !important;'>" + moment(ts.ngaymua).format('DD/MM/YYYY') + "</td>";

    var dv = ts.device_unit ? ts.device_unit : "";
    htmltable +=
      "<td width='90' align='center' style='border:1px solid #000 !important;font-size:14pt !important;border-left:none !important;border-top:none !important;'>" +
      dv +
      "</td>";
    htmltable +=
      "<td width='90' align='center' style='border:1px solid #000 !important;font-size:14pt !important;border-left:none !important;border-top:none !important;padding:10px 5px;'>" +
      (ts.price != null ? ts.price.toLocaleString() : " 0 ") +
      " VND " +
      "</td>";
    htmltable +=
      "<td width='150' align='left' style='border:1px solid #000 !important;font-size:14pt !important;border-left:none !important;border-top:none !important;'>" +
      ts.assets_condition +
      "</td></tr>";
  });
  htmltable += "</tbody></table>";
  htmltable +=
    "<table border='0' width='1024' cellpadding='0' style='padding:10px 15px 0px;'><tbody>";
  htmltable +=
    "<tr><td style='font-size:16pt;font-weight:bold;text-align:right; padding-top:12px'><a style='padding-right:20px'>Tổng giá trị: </a>" +
    sum.toLocaleString() +
    " VND </td></tr></tbody></table>";
  if (
    device_handover.value.print_note == null ||
    device_handover.value.print_note == ""
  ) {
    htmltable +=
      "<table border='0' width='1024' cellpadding='10'><tbody><tr><td colspan='7'><p style='font-size:16pt;text-align:justify;'>" +
      "Biên bản được lập thành 04 bản, mỗi bên giữ 02 bản có giá trị như nhau.</p></td></tr></tbody></table>";
  } else {
    htmltable +=
      "<table border='0' width='1024' cellpadding='10'><tbody><tr><td colspan='7'><p style='font-size:16pt;text-align:justify;'>" +
      device_handover.value.print_note +
      "</p></td></tr></tbody></table>";
  }
  htmltable += "<table border='0' width='1024' cellpadding='10'><tbody><tr>";
  htmltable +=
    "<td width='" +
    (device_handover.value.handover_type == 1 ? 33 : 50) +
    "%' style='text-align:center;'> ";
  htmltable +=
    "<p style='font-weight:bold;font-size:16pt;text-align:center; '>BÊN NHẬN</p>";
  // if (device_handover.value.chuKyNBG != '') {
  //     htmltable += "<img src='" + $rootScope.fileUrl + device_handover.value.chuKyNBG + "' style='height:60px;width:auto;' />";
  // } else {
  //     htmltable += "<p style='padding-top:30px;padding-bottom:30px;'></p>";
  // }
  htmltable +=
    "<p style='font-size:16pt;text-align:center; padding-top:30px'>" +
    device_handover.value.user_receiver_name +
    "</p></td>";
  htmltable +=
    "<td width='" +
    (device_handover.value.handover_type == 1 ? 33 : 50) +
    "%' style='text-align:center;'> ";
  htmltable +=
    "<p style='font-weight:bold;font-size:16pt;text-align:center; '>BÊN GIAO</p>";
  // if (device_handover.value.chuKyNN != '') {
  //     htmltable += "<img src='" + $rootScope.fileUrl + device_handover.value.chuKyNN + "' style='height:60px;width:auto;' />";
  // } else {
  //     htmltable += "<p style='padding-top:30px;padding-bottom:30px;'></p>";
  // }
  htmltable +=
    "<p style='font-size:16pt;text-align:center ; padding-top:30px'>" +
    device_handover.value.user_deliver_name +
    "</p></td>";
  if (device_handover.value.handover_type == 1) {
    htmltable +=
      "<td width='34%'><p style='font-size:16pt;text-align:center'> ";
    htmltable +=
      "<p style='font-weight:bold;font-size:16pt;text-align:center; '>BÊN XÁC NHẬN</p>";

    htmltable +=
      "<p style='font-size:16pt;text-align:center;padding-top:30px'>" + 
      (device_handover.value.user_verifier_name != null
        ? device_handover.value.user_verifier_name
        : "") +
      "</p></td>";
  }
  htmltable += "</tr></tbody></table>   </div>";
  return htmltable;
}
const saveHandover = (isFormValid, isPrint) => {
  submitted.value = true;

  if (!isFormValid) {
    return;
  }
  if (device_handover.value.device_department_id_fake)
    Object.keys(device_handover.value.device_department_id_fake).forEach(
      (key) => {
        device_handover.value.device_department_id = Number(key);
      }
    );
  if (device_handover.value.user_department) {
    let mre = null;
    Object.keys(device_handover.value.user_department).forEach((key) => {
      mre = Number(key);
    });

    var liOr = listOrganization.value.filter((x) => x.organization_id == mre);
    if (liOr) {
      if (liOr[0].user_receiver != null) {
        device_handover.value.receiver_department = mre;
        device_handover.value.is_receiver_department = true;
        device_handover.value.user_receiver_id = liOr[0].user_receiver;

        device_handover.value.user_receiver_name =
          listDropdownUser.value.filter(
            (x) => x.code == device_handover.value.user_receiver_id
          )[0].name;
        device_handover.value.user_receiver_position =
          listDropdownUser.value.filter(
            (x) => x.code == device_handover.value.user_receiver_id
          )[0].position_name;
        device_handover.value.user_receiver_department_id =
          listDropdownUser.value.filter(
            (x) => x.code == device_handover.value.user_receiver_id
          )[0].department_id;
        device_handover.value.user_receiver_department_name =
          listDropdownUser.value.filter(
            (x) => x.code == device_handover.value.user_receiver_id
          )[0].department_name;
      } else {
        swal.fire({
          title: "Thông báo!",
          text: "Phòng ban chưa được cấu hình người nhận!",
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    }
  } else {
    device_handover.value.receiver_department = null;
    device_handover.value.is_receiver_department = false;
  }

  if (
    (!device_handover.value.user_verifier_id &&
      device_handover.value.handover_type == 1) ||
    !device_handover.value.user_receiver_id ||
    !device_handover.value.device_department_id
  ) {
    return;
  }
  if (device_handover.value.handover_number.length > 50) {
    swal.fire({
      title: "Thông báo!",
      text: "Số phiếu không được dài quá 50 kí tự!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  if (!selectedCard.value.length > 0) {
    swal.fire({
      title: "Thông báo!",
      text: "Vui lòng chọn thiết bị trước khi bàn giao!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  if (!device_handover.value.print_note_tf) {
    device_handover.value.print_note = null;
  }

  let formData = new FormData();
  for (var i = 0; i < filesList.value.length; i++) {
    let file = filesList.value[i];
    formData.append("image", file);
  }
  if (filterSTypeH.value == 2) {
    device_handover.value.device_repair_id =
      selectedRepairHandover.value.device_repair_id;

    selectedRepairDevice;
    selectedCard.value.forEach((element) => {
      let check = false;

      selectedRepairDevice.value.forEach((item) => {
        if (item.repair_details_id == element.repair_details_id) check = true;
      });
      if (check == false) {
        selectedCard.value = selectedCard.value.filter(
          (x) => x.repair_details_id != element.repair_details_id
        );
      }
    });
  }
  formData.append("handover", JSON.stringify(device_handover.value));
  formData.append("handoverattach", JSON.stringify(selectedCard.value));
  formData.append("handoverfiles", JSON.stringify(listFilesS.value));
  formData.append("filesize", JSON.stringify(fileSize));
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });

  if (!isSaveCard.value) {
    axios
      .post(
        baseURL + "/api/device_handover/add_device_handover",
        formData,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Thêm cấp phát thành công!");
          if (isPrint) {
            listAssetsH.value = selectedCard.value;
            print();
          }
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
        console.log(error);
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
        baseURL + "/api/device_handover/update_device_handover",
        formData,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa cấp phát thành công!");
          if (isPrint) {
            listAssetsH.value = selectedCard.value;
            print();
          }
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
const displayDetailsHandoverRepair = ref(false);
const openDetailsHandoverRepair = (data) => {
  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "device_repair_get",
            par: [{ par: "device_repair_id", va: data.device_repair_id }],
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
      displayDetailsHandoverRepair.value = true;
      listAssetsH.value = data1;
      listFilesS.value = data2;
    })
    .catch((error) => {});
};
const changeDeviceList = (event) => {
  event.items.forEach((element) => {
    selectedCard.value.push(element);
  });
};
const handoverType1 = ref(true);

const handoverType2 = ref(false);
const onChangeBarcode = (value) => {
  if (value == 1) {
    handoverType1.value = handoverType2.value;
    handoverType2.value = !handoverType1.value;

    if (handoverType1.value) {
      filterSTypeH.value = 1;
    }

    if (handoverType2.value) filterSTypeH.value = 2;
  } else {
    handoverType2.value = handoverType1.value;
    handoverType1.value = !handoverType2.value;
    if (handoverType1.value) filterSTypeH.value = 1;
    if (handoverType2.value) filterSTypeH.value = 2;
  }
  if (filterSTypeH.value == 2) {
    loadRepairHandover(null);
    selectedRepairHandover.value = null;
  } else {
    selectedRepairHandover.value = null;
    datalistsSW.value = [];
  }
};
const userReceiver1 = ref(true);

const userReceiver2 = ref(false);
const typeReceiver = ref(1);
const onChangeUserReceiver = (value) => {
  if (value == 1) {
    userReceiver1.value = userReceiver2.value;
    userReceiver2.value = !userReceiver1.value;

    if (userReceiver1.value) {
      typeReceiver.value = 1;
    }

    device_handover.value.user_department = null;
    if (userReceiver2.value) typeReceiver.value = 2;
  } else {
    userReceiver2.value = userReceiver1.value;
    userReceiver1.value = !userReceiver2.value;
    if (userReceiver1.value) typeReceiver.value = 1;
    if (userReceiver2.value) typeReceiver.value = 2;
    device_handover.value.user_receiver_id = null;
  }
  // if (typeReceiver.value == 2) {

  //   selectedRepairHandover.value = null;
  // } else {
  //   selectedRepairHandover.value = null;

  // }
};
const changeDeviceAllList = (event) => {
  event.items.forEach((element) => {
    selectedCard.value = selectedCard.value.filter(
      (x) => x.card_id != element.card_id
    );
  });
};
const listFilesS = ref([]);
//Sửa bản ghi
const sendAccept = (value) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Xác nhận gửi biên bản bàn giao này không!",
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
        let data = {
          IntID: value.handover_id,
          TextID: value.handover_id + "",
          IntTrangthai: 1,
          BitTrangthai: value.status,
        };

        axios
          .put(
            baseURL + "/api/device_handover/update_s_device_handover",
            data,
            config
          )
          .then((response) => {
            if (response.data.err != "1") {
              swal.close();
              toast.success("Gửi biên bản bàn giao thành công!");
              loadData();
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
      }
    });
};
const editCard = (data) => {
  submitted.value = false;
  filesList.value = [];
  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "device_handover_get",
            par: [{ par: "handover_id", va: data.handover_id }],
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
      device_handover.value = data[0];
      if (device_handover.value.is_receiver_department) {
        device_handover.value.user_department = {};
        device_handover.value.user_department[
          device_handover.value.receiver_department
        ] = true;
        userReceiver2.value = true;
        userReceiver1.value = false;
        typeReceiver.value = 2;
      }
      device_handover.value.device_department_id_fake = {};
      device_handover.value.device_department_id_fake[
        device_handover.value.device_department_id
      ] = true;
      device_handover.value.handover_created_date = new Date(
        device_handover.value.handover_created_date
      );

      selectedCard.value = data1;
      listFilesS.value = data2;
      checkShowAssets.value = false;
      headerDialog.value = "Sửa cấp phát";
      isSaveCard.value = true;

      if (device_handover.value.device_repair_id != null) {
        filterSTypeH.value = 2;
        handoverType2.value = true;
        handoverType1.value = false;
        loadRepairHandover(device_handover.value.device_repair_id);
      } else {
        filterSTypeH.value = 1;
        handoverType1.value = true;
        handoverType2.value = false;
        selectedRepairHandover.value = null;
      }
      displayBasic.value = true;
    })
    .catch((error) => {});
};
const checkImg = (src) => {
  let allowedExtensions = /(\.jpg|\.jpeg|\.png|\.gif)$/i;
  return allowedExtensions.exec(src);
};
//Hiển thị dialog

const headerDialog = ref();
const displayBasic = ref(false);
const selectedUser = ref({
  used: true,
  not_used: false,
});
const listSCard = ref([
  { name: "Đã tạo", code: 0 },
  { name: "Chờ xác nhận", code: 1 },
  { name: "Đã xác nhận", code: 2 },
  { name: "Trả lại", code: 3 },
]);
const listTCard = ref([
  { name: "Bàn giao 3 bên", code: 1 },
  { name: "Bàn giao 2 bên", code: 0 },
]);

const loadDeviceNumber = (dataVL) => {
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
const openBasic = (str) => {
  checkTypeHO.value = false;
  handoverType1.value = true;
  handoverType2.value = false;

  userReceiver1.value = true;
  userReceiver2.value = false;
  selectedCard.value = [];
  filterSTypeH.value = 1;
  typeReceiver.value = 1;
  listFilesS.value = [];
  device_handover.value = {
    status: 0,
    is_order: sttCard.value ? sttCard.value : 1,
    receipt_type: 1,
    handover_created_date: new Date(),
    handover_type: 1,
    user_deliver_id: store.getters.user.user_id,
    user_deliver_name: store.getters.user.full_name,
    user_deliver_department_id: store.getters.user.organization_id,
    user_deliver_department_name: store.getters.user.organization_name,
    user_deliver_position: store.getters.user.position_name,
    print_note_tf: false,
    is_receiver_department: false,
  };
  device_handover.value.print_note =
    "Sau khi tiến hành kiểm tra TS, CCDC cấp phát đã được lắp đặt, bàn giao hai bên nhất trí hiện trạng: máy móc, trang thiết bị, dụng cụ đang hoạt động tốt. Bên nhận có trách nhiệm quản lý sử dụng, bảo quản nếu xảy ra mất mát/hư hỏng không do khách quan sẽ chịu hoàn toàn trách nhiệm. Biên bản này được lập làm 2 bản, mỗi bên giữ 1 bản và có giá trị như nhau.";
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
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];

      if (data.length > 0) {
        loadDeviceNumber(data[0]);
        checkCV.value = false;
        filesList.value = [];
        submitted.value = false;
        headerDialog.value = str;
        isSaveCard.value = false;
        checkShowAssets.value = false;
        displayBasic.value = true;
      } else {
        swal.fire({
          title: "Error!",
          text: "Vui lòng cấu hình số hiệu cho phiếu cấp phát!",
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    })
    .catch((error) => {});
  // loadRelate();
};
const onChangeReceipt = () => {
  selectedCard.value = [];
  if (device_handover.value.receipt_type == 0) {
    checkTypeHO.value = true;
    device_handover.value.user_verifier_id = null;
    device_handover.value.handover_type = 0;
  } else {
    checkTypeHO.value = false;
    device_handover.value.handover_type = 1;
  }
};
const checkCV = ref(false);
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
  }

  if (rf) {
    options.value.loading = true;
    loadCount();
  }
  // if (menu_ID.value == -1) {
  //   menu_ID.value = null;
  // }
  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "device_handover_list",
            par: [
              { par: "pageno", va: options.value.pageno },
              { par: "pagesize", va: options.value.pagesize },
              { par: "user_id", va: store.state.user.user_id },
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

      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
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
      options.value.is_hot != null ||
      options.value.status != null ||
      options.value.news_type != null
    )
  )
    checkFilter.value = false;
};
const filterIsHot = ref();
const filterTrangthai = ref();
const filterCardUser = ref();
const filterCardUserSend = ref();
const listSTypeH = ref([
  {
    name: "Cấp phát mới thiết bị",
    code: 1,
  },
  {
    name: "Thay thế tài sản từ phiếu sửa chữa",
    code: 2,
  },
]);
const filterSTypeH = ref(1);

const reFilterCard = () => {
  checkFilter.value = false;
  filterIsHot.value = null;
  filterCardUser.value = null;
  filterCardUserSend.value = null;
  filterTrangthai.value = null;
  taskDateFilter.value = [];
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
      key: "user_receiver_id",
    };
    filterSQL.value.push(filterS);
  }
  if (filterCardUserSend.value != null) {
    let filterS = {
      filterconstraints: [
        { value: filterCardUserSend.value, matchMode: "equals" },
      ],
      filteroperator: "and",
      key: "user_deliver_id",
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
const firstSW = ref(0);
const firstRP = ref(0);
const refreshData = () => {
  options.value.search = "";
  options.value.status = null;
  filterCardUser.value = null;
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
    handover_number: {
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
    handover_created_date: {
      operator: FilterOperator.AND,
      constraints: [{ value: null, matchMode: FilterMatchMode.DATE_BEFORE }],
    },
    handover_created_date: {
      operator: FilterOperator.AND,
      constraints: [{ value: null, matchMode: FilterMatchMode.DATE_AFTER }],
    },
  };
  loadDataSQL();
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
    label: "Xuất Word",
    icon: "pi pi-file",
    command: (event) => {
      exportWordListH(event);
    },
  },
]);

const exportWordListH = (method) => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  var htmltable = "";
  htmltable = renderhtmlWord("formwordlist", htmltable);
  axios
    .post(
      baseURL + "/api/device_handover/ExportDoc",
      {
        lib: "word",
        name: "DANH_SACH_CAP_PHAT",
        html: htmltable,
        opition: {
          orientation: "Portrait",
          pageSize: "A4",
          left: 37.79,
          top: 68.03,
          right: 37.79,
          bottom: 68.03,
        },
      },
      config
    )
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        swal.close();

        toast.success("Kết xuất dữ liệu thành công!");
        if (response.data.path != null) {
          window.open(baseURL + response.data.path);
        }
      } else {
        swal.fire({
          title: "Thông báo!",
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
const exportWord = (method) => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  var htmltable = "";
  htmltable = renderhtmlWord("formword", htmltable);
  axios
    .post(
      baseURL + "/api/device_handover/ExportDoc",
      {
        lib: "word",
        name: "BIEN_BAN_BAN_GIAO",
        html: htmltable,
        opition: {
          orientation: "Portrait",
          pageSize: "A4",
          left: 37.79,
          top: 68.03,
          right: 37.79,
          bottom: 68.03,
        },
      },
      config
    )
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        swal.close();

        toast.success("Kết xuất dữ liệu thành công!");
        if (response.data.path != null) {
          window.open(baseURL + response.data.path);
        }
      } else {
        swal.fire({
          title: "Thông báo!",
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
        excelname: "DANH SÁCH BIÊN BẢN BÀN GIAO",
        proc: "device_handover_list_export",
        par: [
          { par: "user_id", va: store.state.user.user_id },
          { par: "search", va: options.value.search },
          { par: "status", va: filterTrangthai.value },
          { par: "user_deliver_name", va: filterCardUserSend.value },
          { par: "user_receiver_name", va: filterCardUser.value },
          { par: "start_date", va: options.value.start_date },
          { par: "end_date", va: options.value.end_date },
          { par: "pageno", va: options.value.pageno },
          { par: "pagesize", va: options.value.pagesize },
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
const onChangeHT = () => {
  device_handover.value.user_verifier_id = null;
};
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
              { par: "pagesize", va: 100000 },
              { par: "user_id", va: store.state.user.user_id },
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
              { par: "pagesize", va: 100000 },
              { par: "user_id", va: store.state.user.user_id },

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
const listTWarehouse = ref();
const loadWareHouse = () => {
  listWarehouse.value = [];
  listTWarehouse.value = [];
  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "device_filter_handover_warehouse",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
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
      let data = JSON.parse(response.data.data)[0];
      data.forEach((element) => {
        listTWarehouse.value.push({
          name: element.warehouse_name,
          code: element.warehouse_id,
        });
        listWarehouse.value = data;
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

const listDropdownUserGive = ref();
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
          department_id: element.department_id,
          role_name: element.role_name,
          position_name: element.position_name,
          phone: element.phone,
          organization_id: element.organization_id,
        });
        listUsers.value.push({ data: element, active: false });
      });
      listUsers.value = data;
      listDropdownUserGive.value = listDropdownUser.value;
      listDropdownUserCheck.value = listDropdownUser.value.filter(
        (x) => x.code != store.getters.user.user_id
      );
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
const listOrganization = ref([]);
const datalistsDM = ref();
const listDepartment = ref();
const loadOrganization = () => {
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
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];

      if (data.length > 0) {
        listOrganization.value = data;
        var arr = [...data];
        let obj = renderTreeDV1(
          arr,
          "organization_id",
          "organization_name",
          "đơn vị",
          store.getters.user.organization_id
        );
        datalistsD.value = obj.arrChils;
        listDepartment.value = obj.arrtreeChils;
      } else listDepartment.value = [];
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

onMounted(() => {
  if (!checkURL(window.location.pathname, store.getters.listModule)) {
    //router.back();
  }
  loadDeviceUnit();

  loadOrganization();
  loadWareHouse();
  loadUser();

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
        dataKey="handover_id"
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
              <i class="pi pi-briefcase"></i> Danh sách cấp phát ({{
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
                    (filterCardUser != null ||
                      filterTrangthai != null ||
                      filterCardUserSend != null) &&
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
                        Người bàn giao:
                      </div>
                      <Dropdown
                        v-model="filterCardUserSend"
                        panelClass="d-design-dropdown"
                        :options="listDropdownUser"
                        :filter="true"
                        optionLabel="name"
                        optionValue="code"
                        style="width: calc(100% - 10rem)"
                        class="w-full"
                        placeholder="Người bàn giao"
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
                v-if="checkDelList"
                @click="deleteList()"
                label="Xóa"
                icon="pi pi-trash"
                class="mr-2 p-button-danger"
              />
              <Button
                @click="openBasic('Thêm phiếu cấp phát')"
                label="Thêm mới"
                icon="pi pi-plus"
                class="mr-2"
              />

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
          field="handover_number"
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
          headerStyle="text-align:center;height:50px"
          bodyStyle="text-align:center"
          field="user_deliver_name"
          header="Người bàn giao"
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
          headerStyle="text-align:center;height:50px"
          bodyStyle="text-align:center"
          field="user_receiver_name"
          header="Sử dụng"
        >
          <template #body="data">
            <div>
              <span v-if="data.data.is_receiver_department">
                {{ data.data.receiver_department_name }}</span
              ><span v-else>{{ data.data.user_receiver_name }}</span>
            </div>
          </template>
        </Column>
        <Column
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;height:50px;max-width:100px"
          bodyStyle="text-align:center;max-width:100px"
          field="assets"
          header="Tài sản"
          ><template #body="data">
            <div>
              <Button
                @click="openDetails(data.data.handover_id)"
                :label="data.data.assets.toString()"
                class="p-button-rounded"
              />
            </div>
          </template>
        </Column>
        <Column
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;max-width:120px;height:50px"
          bodyStyle="text-align:center;max-width:120px"
          field="handover_created_date"
          header="Ngày lập"
          :sortable="true"
        >
          <template #body="data">
            <div>
              {{
                moment(new Date(data.data.handover_created_date)).format(
                  "DD/MM/YYYY"
                )
              }}
            </div>
          </template>
        </Column>
        <Column
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;max-width:120px;height:50px"
          bodyStyle="text-align:center;max-width:120px"
          field="receiver_date"
          header="Ngày xác nhận"
        >
          <template #body="data">
            <div v-if="data.data.receiver_date">
              {{
                moment(new Date(data.data.receiver_date)).format("DD/MM/YYYY")
              }}
            </div>
          </template>
        </Column>
        <Column
          headerStyle="text-align:center;max-width:200px;height:50px"
          bodyStyle="text-align:center;max-width:200px"
          field="handover_type"
          header="Kiểu bàn giao"
          class="align-items-center justify-content-center text-center"
        >
          <template #body="data">
            <div>
              {{
                data.data.handover_type == 1
                  ? "Bàn giao 3 bên"
                  : "Bàn giao 2 bên"
              }}
            </div>
          </template>
        </Column>

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
                v-if="data.data.status == 0"
                label="Đã tạo"
                class="w-full surface-200 justify-content-center"
              />
              <Chip
                v-else-if="data.data.status == 1"
                label="Chờ xác nhận"
                class="
                  w-full
                  bg-pink-300
                  justify-content-center
                  p-button-status-d
                "
              />
              <Chip
                v-else-if="data.data.status == 2"
                label="Đã xác nhận"
                class="
                  w-full
                  bg-blue-300
                  justify-content-center
                  p-button-status-d
                "
              />
              <Chip
                v-else-if="data.data.status == 3"
                label="Trả lại"
                style="background-color: red"
                class="w-full justify-content-center p-button-status-d"
              />
            </div>
          </template>
        </Column>

        <Column
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;height:50px"
          bodyStyle="text-align:center"
          header="Chức năng"
        >
          <template #body="data">
            <Button
              v-tooltip.top="'Chi tiết phiếu'"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              @click="openDetailsHandover(data.data)"
              type="button"
              icon="pi pi-info-circle"
            ></Button>
            <div
              v-if="
                (store.state.user.is_super == true ||
                  store.state.user.user_id == data.data.created_by ||
                  (store.state.user.role_id == 'admin' &&
                    store.state.user.organization_id ==
                      data.data.organization_id)) &&
                data.data.status != 2 &&
                data.data.status != 1
              "
            >
              <Button
                type="button"
                class="
                  p-button-rounded p-button-secondary p-button-outlined
                  mx-1
                "
                icon="pi pi-cog"
                @click="toggleCog($event, data.data)"
                aria-haspopup="true"
                aria-controls="overlay_menu_cog"
                v-tooltip.left="'Chức năng'"
              />
              <Menu
                id="overlay_menu_cog"
                ref="menuCog"
                :model="itemsCog"
                :popup="true"
              />
              <!-- <Button
                v-tooltip.top="'Sửa'"
                @click="editCard(data.data)"
                class="
                  p-button-rounded p-button-secondary p-button-outlined
                  mx-1
                "
                type="button"
                icon="pi pi-pencil"
              ></Button>
              <Button
                v-tooltip.top="'Xóa'"
                class="
                  p-button-rounded p-button-secondary p-button-outlined
                  mx-1
                "
                type="button"
                icon="pi pi-trash"
                @click="delCard(data.data)"
              ></Button> -->
              <Button
                v-tooltip.top="'Chuyển xác nhận'"
                class="
                  p-button-rounded p-button-secondary p-button-outlined
                  mx-1
                "
                @click="sendAccept(data.data)"
                type="button"
                icon="pi pi-angle-double-right"
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
    <iframe name="printframe" id="printframe" style="display: none"></iframe>
  </div>
  <Sidebar
    class="p-sidebar-lg"
    :showCloseIcon="false"
    v-model:visible="displayDetails"
    position="right"
  >
    <div class="w-full format-center">
      <h3>Danh sách thiết bị kèm theo</h3>
    </div>
    <div
      class="w-full p-0 pt-2"
      v-for="(item, index) in listAssetsH"
      :key="index"
    >
      <div
        style="border-radius: 10px"
        class="
          product-item
          border-3 border-solid border-round-3xl border-blue-200
          surface-50
          p-2
        "
      >
        <div class="image-container pr-2">
          <img
            :src="
              item.image
                ? basedomainURL + item.image
                : basedomainURL + '/Portals/Image/noimg.jpg'
            "
            style="object-fit: cover; width: 125px; height: 75px"
          />
        </div>
        <div class="product-list-detail">
          <h5 class="my-2 text-justify">
            {{ item.device_name }}
          </h5>

          <div class="flex pb-2">
            <div class="w-full">
              <i class="pi pi-tag product-category-icon"></i>
              <span class="product-category">{{ item.device_number }}</span>
            </div>
            <div class="w-full">
              <i class="pi pi-qrcode product-category-icon"></i>
              <span class="product-category">{{ item.barcode_id }}</span>
            </div>
          </div>
          <div class="flex">
            <div class="w-full">
              <i class="pi pi-home product-category-icon"></i>
              <span class="product-category">
                {{ item.warehouse_name }}
              </span>
            </div>
            <div class="w-full">
              <i class="pi pi-shopping-cart product-category-icon"></i>
              <span class="product-category">
                {{ moment(new Date(item.purchase_date)).format("DD/MM/YYYY") }}
              </span>
            </div>
          </div>
        </div>
        <!-- <div v-if="isSaveCard">
          <Button
            icon="pi pi-times"
            class="p-button-rounded p-button-danger"
            @click="deleteFileD(item)"
          />
        </div> -->
      </div>
    </div>
  </Sidebar>

  <Dialog
    @hide="closeDialog"
    :header="headerDialog"
    v-model:visible="displayBasic"
    :maximizable="true"
    :style="{ width: '55vw' }"
    :modal="true"
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12 flex format-center">
          <div class="col-6 p-0">
            <SelectButton
              v-model="device_handover.receipt_type"
              :options="listTypeP"
              optionLabel="name"
              optionValue="code"
              @change="onChangeReceipt()"
            />
          </div>
        </div>
        <div class="col-12 field p-0 text-lg font-bold">Thông tin phiếu</div>
        <div class="col-12 field flex p-0">
          <div class="col-6 flex p-0 align-items-center">
            <div class="w-10rem">
              Số phiếu<span class="redsao pl-1"> (*)</span>
            </div>
            <div style="width: calc(100% - 10rem)">
              <InputText
                v-model="device_handover.handover_number"
                class="w-full"
                :class="{
                  'p-invalid': v$.handover_number.$invalid && submitted,
                }"
              />
            </div>
          </div>
          <div class="col-6 flex p-0 text-center align-items-center">
            <div class="col-4 p-0 pl-5 text-left">
              Ngày lập<span class="redsao pl-1"> (*)</span>
            </div>
            <div class="col-8 p-0 flex text-left">
              <Calendar
                placeholder="dd/mm/yyyy"
                class="w-full"
                id="basic_use_date"
                v-model="device_handover.handover_created_date"
                autocomplete="on"
                :min-date="
                  device_handover.handover_created_date
                    ? device_handover.handover_created_date
                    : new Date('1/1/1970')
                "
                :showIcon="true"
              />
            </div>
          </div>
        </div>
        <div
          v-if="
            (v$.handover_number.$invalid && submitted) ||
            v$.handover_number.$pending.$response
          "
          class="col-12 field p-0 flex"
        >
          <div class="w-10rem"></div>
          <small style="width: calc(100% - 10rem)">
            <span style="color: red" class="w-full">{{
              v$.handover_number.required.$message
                .replace("Value", "Số phiếu")
                .replace("is required", "không được để trống!")
            }}</span>
          </small>
        </div>
        <div class="col-12 flex p-0">
          <div class="col-6 p-0 text-left align-items-center">
            <div class="col-12 field p-0 flex text-left align-items-center">
              <div class="w-10rem">Kiểu bàn giao</div>
              <div style="width: calc(100% - 10rem)">
                <Dropdown
                  :disabled="device_handover.receipt_type == 0 ? true : false"
                  v-model="device_handover.handover_type"
                  :options="listTCard"
                  optionLabel="name"
                  optionValue="code"
                  class="w-full"
                  @change="onChangeHT"
                />
              </div>
            </div>
          </div>
          <div class="col-6 p-0 text-left align-items-center">
            <div class="col-12 field p-0 flex text-left align-items-center">
              <div class="col-4 p-0 pl-5 text-left">Số thứ tự</div>

              <InputNumber
                class="p-0 col-8"
                v-model="device_handover.is_order"
              />
            </div>
          </div>
        </div>

        <div class="col-12 field p-0 text-lg font-bold">
          <Toolbar class="p-0 surface-0 border-none">
            <template #start>
              <div class="text-lg font-bold">Người bàn giao</div>
            </template>
            <template #end> </template>
          </Toolbar>
        </div>

        <div class="col-12 flex p-0">
          <div class="col-6 p-0 text-left align-items-center">
            <div class="col-12 field p-0 flex text-left align-items-center">
              <div class="w-10rem">
                Người bàn giao<span class="redsao pl-1"> (*)</span>
              </div>
              <InputText
                v-model="device_handover.user_deliver_name"
                disabled
                style="width: calc(100% - 10rem)"
              />
            </div>
          </div>
          <div class="col-6 p-0 text-left align-items-center">
            <div class="col-12 field p-0 flex text-left align-items-center">
              <div class="col-4 p-0 pl-5">Đơn vị</div>
              <InputText
                v-model="device_handover.user_deliver_department_name"
                disabled
                class="w-full"
              />
            </div>
          </div>
        </div>
        <div class="col-12 flex p-0 field">
          <div class="col-6 p-0 text-left align-items-center"></div>
          <div class="col-6 p-0 text-left align-items-center flex">
            <div class="col-4 p-0 pl-5">Chức vụ</div>
            <InputText
              v-model="device_handover.user_deliver_position"
              disabled
              class="w-full"
            />
          </div>
        </div>
        <div class="col-12 field p-0 align-items-center flex">
          <div class="w-10rem p-0 font-bold text-lg">Người nhận</div>
          <div
            style="width: calc(100% - 10rem)"
            class="p-0 flex text-left align-items-center"
          >
            <div class="p-0">Cá nhân:</div>

            <InputSwitch
              @click="onChangeUserReceiver(1)"
              class="w-4rem lck-checked ml-3"
              v-model="userReceiver1"
            />

            <div class="ml-5 mr-3">Phòng ban:</div>

            <InputSwitch
              @click="onChangeUserReceiver(2)"
              class="w-4rem lck-checked"
              v-model="userReceiver2"
            />
          </div>
        </div>

        <div class="col-12 flex p-0" v-if="typeReceiver == 1">
          <div class="col-6 p-0 text-left align-items-center">
            <div class="col-12 field p-0 flex text-left align-items-center">
              <div class="w-10rem align-items-center flex">
                <div>Người nhận<span class="redsao pl-1"> (*)</span></div>
                <Button
                  v-tooltip.top="'Chọn người nhận'"
                  @click="showTreeUser(1)"
                  icon="pi pi-user-plus"
                  class="p-button-text p-button-rounded"
                />
              </div>
              <Dropdown
                v-model="device_handover.user_receiver_id"
                panelClass="d-design-dropdown"
                class="sel-placeholder"
                :options="listDropdownUserGive"
                :filter="true"
                optionLabel="name"
                optionValue="code"
                style="width: calc(100% - 10rem)"
                @change="onChangeUser1(device_handover.user_receiver_id)"
                placeholder="-------- Chọn người nhận --------"
                :class="{
                  'p-invalid': !device_handover.user_receiver_id && submitted,
                }"
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
                        <div class="col-11 p-0 ml-3 align-items-center">
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
            <div
              v-if="!device_handover.user_receiver_id && submitted"
              class="col-12 field p-0 flex"
            >
              <div class="w-10rem"></div>
              <small style="width: calc(100% - 10rem)">
                <span style="color: red" class="w-full"
                  >Người nhận không được để trống!</span
                >
              </small>
            </div>
          </div>
          <div class="col-6 p-0 text-left align-items-center">
            <div class="col-12 field p-0 flex text-left align-items-center">
              <div class="col-4 p-0 pl-5">Đơn vị</div>
              <InputText
                v-model="device_handover.user_receiver_department_name"
                disabled
                class="w-full"
              />
            </div>
          </div>
        </div>
        <div class="col-12 flex p-0" v-if="typeReceiver == 2">
          <div class="col-6 p-0 text-left align-items-center">
            <div class="col-12 field p-0 flex text-left align-items-center">
              <div class="w-10rem">
                Phòng ban nhận<span class="redsao pl-1"> (*)</span>
              </div>
              <TreeSelect
                v-model="device_handover.user_department"
                :options="listDepartment"
                :showClear="true"
                :max-height="200"
                style="width: calc(100% - 10rem)"
                placeholder="---------- Chọn phòng ban nhận ----------"
                optionLabel="data.organization_name"
                optionValue="data.department_id"
                panelClass="d-design-dropdown"
                class="d-tree-input"
                :class="{
                  'p-invalid': !device_handover.user_department && submitted,
                }"
              >
              </TreeSelect>
            </div>
            <div
              v-if="!device_handover.user_department && submitted"
              class="col-12 field p-0 flex"
            >
              <div class="w-10rem"></div>
              <small style="width: calc(100% - 10rem)">
                <span style="color: red" class="w-full"
                  >Phòng ban nhận không được để trống!</span
                >
              </small>
            </div>
          </div>
          <div class="col-6 p-0 text-left align-items-center"></div>
        </div>
        <div class="col-12 flex p-0 field">
          <div class="col-6 p-0 align-items-center flex">
            <div class="w-10rem">
              Bộ phận quản lý<span class="redsao pl-1"> (*)</span>
            </div>

            <TreeSelect
              v-model="device_handover.device_department_id_fake"
              :options="listDepartment"
              :showClear="true"
              :max-height="200"
              style="width: calc(100% - 10rem)"
              placeholder="---------- Đơn vị ----------"
              optionLabel="data.organization_name"
              optionValue="data.department_id"
              panelClass="d-design-dropdown"
              class="d-tree-input"
              :class="{
                'p-invalid':
                  !device_handover.device_department_id_fake && submitted,
              }"
            >
            </TreeSelect>
          </div>
          <div
            class="col-6 p-0 text-left align-items-center flex"
            v-if="typeReceiver == 1"
          >
            <div class="col-4 p-0 pl-5">Chức vụ</div>
            <InputText
              v-model="device_handover.user_receiver_position"
              disabled
              class="w-full"
            />
          </div>
        </div>
        <div
          v-if="!device_handover.device_department_id_fake && submitted"
          class="col-12 field p-0 flex"
        >
          <div class="w-10rem"></div>
          <small style="width: calc(100% - 10rem)">
            <span style="color: red" class="w-full"
              >Bộ phận quản lý không được để trống!</span
            >
          </small>
        </div>
        <div class="col-12 field p-0" v-if="device_handover.handover_type == 1">
          <div class="col-12 field p-0 text-lg font-bold">
            <Toolbar class="p-0 surface-0 border-none">
              <template #start>
                <div class="text-lg font-bold">Người duyệt</div>
              </template>
              <template #end> </template>
            </Toolbar>
          </div>

          <div class="col-12 flex p-0">
            <div class="col-6 p-0 text-left align-items-center">
              <div class="col-12 field p-0 flex text-left align-items-center">
                <div class="w-10rem align-items-center flex">
                  <div>Người duyệt<span class="redsao pl-1"> (*)</span></div>
                  <Button
                    v-tooltip.top="'Chọn người duyệt'"
                    @click="showTreeUser(2)"
                    icon="pi pi-user-plus"
                    class="p-button-text p-button-rounded"
                  />
                </div>
                <Dropdown
                  v-model="device_handover.user_verifier_id"
                  panelClass="d-design-dropdown"
                  class="sel-placeholder"
                  :options="listDropdownUserCheck"
                  :filter="true"
                  optionLabel="name"
                  optionValue="code"
                  :class="{
                    'p-invalid': !device_handover.user_verifier_id && submitted,
                  }"
                  style="width: calc(100% - 10rem)"
                  @change="onChangeUser2(device_handover.user_verifier_id)"
                  placeholder="Chọn người duyệt"
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
                                      slotProps.option.name.lastIndexOf(' ') +
                                        1,
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
                          <div class="col-11 p-0 ml-3 align-items-center">
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
            <div class="col-6 p-0 text-left align-items-center">
              <div class="col-12 field p-0 flex text-left align-items-center">
                <div class="col-4 p-0 pl-5">Đơn vị</div>
                <InputText
                  v-model="device_handover.user_verifier_department_name"
                  disabled
                  class="w-full"
                />
              </div>
            </div>
          </div>
          <div
            v-if="!device_handover.user_verifier_id && submitted"
            class="col-12 field p-0 flex"
          >
            <div class="w-10rem"></div>
            <small style="width: calc(100% - 10rem)">
              <span style="color: red" class="w-full"
                >Người xác nhận không được để trống!</span
              >
            </small>
          </div>
          <div class="col-12 flex p-0 field">
            <div class="col-6 p-0 text-left align-items-center flex"></div>
            <div class="col-6 p-0 text-left align-items-center flex">
              <div class="col-4 p-0 pl-5">Chức vụ</div>
              <InputText
                v-model="device_handover.user_verifier_position"
                disabled
                class="w-full"
              />
            </div>
          </div>
        </div>

        <div
          class="field col-12 p-0 md:col-12 flex align-items-center"
          v-if="device_handover.receipt_type != 0"
        >
          <div class="w-10rem p-0 font-bold text-lg">Loại bàn giao</div>
          <div
            style="width: calc(100% - 10rem)"
            class="p-0 flex text-left align-items-center"
          >
            <div class="p-0">Cấp phát mới:</div>

            <InputSwitch
              @click="onChangeBarcode(1)"
              class="w-4rem lck-checked ml-3"
              v-model="handoverType1"
            />

            <div class="ml-5 mr-3">Theo phiếu sửa chữa:</div>

            <InputSwitch
              @click="onChangeBarcode(2)"
              class="w-4rem lck-checked"
              v-model="handoverType2"
            />
          </div>
        </div>

        <div
          class="col-12 mt-3 field flex p-0"
          v-if="filterSTypeH == 2 && device_handover.receipt_type != 0"
        >
          <div class="col-6 p-0 text-left align-items-center">
            <div class="col-12 field p-0 flex text-left align-items-center">
              <div class="w-10rem">Chọn phiếu sửa chữa</div>
              <div
                style="width: calc(100% - 10rem)"
                class="flex align-items-center"
              >
                <Dropdown
                  v-model="selectedRepairHandover"
                  panelClass="d-design-dropdown"
                  class="sel-placeholder w-full"
                  :options="listDropdownRepair"
                  :filter="true"
                  optionLabel="name"
                  optionValue="code"
                  @change="onSelectRepairSW(selectedRepairHandover, false)"
                  placeholder="------Chọn phiếu------"
                >
                </Dropdown>
                <div
                  class="px-2 cursor-pointer"
                  v-tooltip.top="'Xem phiếu'"
                  @click="openDetailsHandoverRepair(selectedRepairHandover)"
                  v-if="selectedRepairHandover"
                >
                  <i class="pi pi-eye" style="font-size: 1.25rem"></i>
                </div>
              </div>
            </div>
          </div>
          <div class="col-6 p-0 text-left align-items-center"></div>
        </div>
        <div
          class="field col-12 p-0"
          v-if="
            filterSTypeH == 2 &&
            device_handover.receipt_type != 0 &&
            selectedRepairHandover
          "
        >
          <DataTable
            :show-gridlines="true"
            :value="datalistsDRD"
            :rows="options.totalRecordsDRD"
            :scrollable="true"
            scrollHeight="flex"
            :loading="options.loading"
            dataKey="card_id"
            v-model:selection="selectedRepairDevice"
            selectionMode="multiple"
            :lazy="true"
            filterDisplay="menu"
            filterMode="lenient"
            :totalRecords="options.totalRecordsDRD"
            :row-hover="true"
            @row-unselect="onSelectRepairDevice($event, 3)"
            @row-unselect-all="onSelectRepairDevice($event, 4)"
            @row-select="onSelectRepairDevice($event, 0)"
            @row-select-all="onSelectRepairDevice($event, 1)"
          >
            <Column
              class="align-items-center justify-content-center text-center"
              headerStyle="text-align:center;max-width:50px;height:50px"
              bodyStyle="text-align:center;max-width:50px"
              selectionMode="multiple"
            >
            </Column>
            <Column
              class="align-items-center justify-content-center text-center"
              headerStyle="text-align:center;max-width:50px;height:50px"
              bodyStyle="text-align:center;max-width:50px;overflow: hidden;"
              field="STT"
              header="STT"
            >
            </Column>
            <Column
              headerStyle="text-align:left;max-width:150px;height:50px"
              bodyStyle="text-align:left;max-width:150px;overflow: hidden;"
              field="device_number"
              header="Số hiệu"
            >
              <template #body="data">
                <div>
                  {{ data.data.device_number }}
                </div>
              </template>
            </Column>
            <!-- <Column
          headerStyle="text-align:left;max-width:150px;height:50px"
          bodyStyle="text-align:left;max-width:150px;overflow: hidden;"
          field="barcode_id"
          header="Mã barcode"
         
        >
          <template #body="data">
            <div>
              {{ data.data.barcode_id }}
            </div>
          </template>
       
        </Column> -->
            <Column
              headerStyle="text-align:left;height:50px"
              bodyStyle="text-align:left; overflow: hidden;"
              field="device_name"
              header="Tên thiết bị"
            >
              <template #body="data">
                <div>
                  {{ data.data.device_name }}
                </div>
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
                    moment(new Date(data.data.purchase_date)).format(
                      "DD/MM/YYYY"
                    )
                  }}
                </div>
              </template>
            </Column>
            <Column
              class="align-items-center justify-content-center text-center"
              headerStyle="text-align:center; height:50px"
              bodyStyle="text-align:center; overflow: hidden;"
              field="full_name"
              header="Người sử dụng gần nhất"
              ><template #body="data">
                <div class="format-center w-full">
                  <div
                    v-tooltip.bottom="{
                      value:
                        data.data.department_name  ,
                      class: 'custom-error-tl1',
                    }"
                    class="
                      flex
                      surface-100
                      align-items-center
                      pr-2
                      format-center
                    "
                    style="border-radius: 16px"
                  >
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
                      v-if="data.data.full_name"
                    />
                    <div class="px-2">{{ data.data.full_name }}</div>
                  </div>
                </div>
              </template>
            </Column>
            <Column
              class="align-items-center justify-content-center text-center"
              headerStyle="text-align:center; height:50px"
              bodyStyle="text-align:center; overflow: hidden;"
              field="condition"
              header="Tình trạng"
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
        </div>

        <div
          class="col-12 p-0 format-center"
          v-if="
            filterSTypeH == 2 &&
            device_handover.receipt_type != 0 &&
            (selectedCard.length > 0 || selectedRepairDevice.length > 0)
          "
        >
          <h3>Chọn thiết bị thay thế</h3>
        </div>
        <div
          class="col-12 p-0 field"
          v-if="
            filterSTypeH == 2 &&
            device_handover.receipt_type != 0 &&
            (selectedCard.length > 0 || selectedRepairDevice.length > 0)
          "
        >
          <div
            style="border-radius: 5px"
            class="
              w-full
              field
              p-2
              pt-0
              border-none
              image-container
              border-2 border-solid border-300
            "
            v-for="(item, index) in dataselectRP"
            :key="index"
          >
            <div class="col-12 p-2 flex">
              <div class="col-5 p-0 border-2 border-blue-300 p-2">
                <div class="product-item">
                  <div class="image-container">
                    <img
                      :src="
                        item.pre.image
                          ? basedomainURL + item.pre.image
                          : basedomainURL + '/Portals/Image/noimg.jpg'
                      "
                      width="100%"
                      style="object-fit: cover"
                      class="p-0 img-d"
                      :alt="item.pre.image"
                      @error="
                        $event.target.src =
                          basedomainURL + '/Portals/Image/noimg.jpg'
                      "
                    />
                  </div>
                  <div class="product-list-detail">
                    <h4 class="mb-2 mt-0">
                      {{ item.pre.device_name }}
                    </h4>

                    <div class="flex pb-1">
                      <div class="w-full" v-tooltip.top="'Số hiệu'">
                        <i class="pi pi-tag product-category-icon"></i>
                        <span class="product-category">{{
                          item.pre.device_number
                        }}</span>
                      </div>

                      <!-- <div class="w-full" v-tooltip.top="'Mã barcode'">
                    <i class="pi pi-qrcode product-category-icon"></i>
                    <span class="product-category">{{
                      item.pre.barcode_id
                    }}</span>
                  </div> -->
                    </div>
                    <div class="flex">
                      <div class="w-full" v-tooltip.top="'Nguyên giá'">
                        <i class="pi pi-money-bill product-category-icon"></i>
                        {{ item.pre.price.toLocaleString() }} VND
                      </div>
                      <!-- <div class="w-full" v-else v-tooltip.top="'Phòng ban quản lý'">
                    <i class="pi pi-home product-category-icon"></i>
                    {{ item.pre.manage_department_name }}
                  </div> -->
                      <!-- <div class="w-full" v-tooltip.top="'Ngày mua'">
                    <i class="pi pi-shopping-cart product-category-icon"></i>
                    <span class="product-category">
                      {{
                        moment(new Date(item.pre.purchase_date)).format(
                          "DD/MM/YYYY"
                        )
                      }}
                    </span>
                  </div> -->
                    </div>
                  </div>
                </div>
                <!-- <Card class="w-full h-full">
                <template #header>
                  <Image
                    :src="
                      item.pre.image
                        ? basedomainURL + item.pre.image
                        : basedomainURL + '/Portals/Image/noimg.jpg'
                    "
                    width="100%"
                    height="250"
                    imageStyle="object-fit: cover"
                    style="object-fit: cover; width: 100%"
                    class="p-8 cursor-pointer"
                    preview
                    :alt="item.pre.image"
                    @error="
                      $event.target.src =
                        basedomainURL + '/Portals/Image/noimg.jpg'
                    "
                  />
                </template>
                <template #title>
                  <span style="font: size 1.25rem !important">
                    {{ item.pre.device_name }}
                  </span>
                </template>
                <template #subtitle>
                  <div class="flex w-full">
                    <div class="w-full">
                      <i class="pi pi-tag product-category-icon"></i>
                      <span class="product-category">{{
                        item.pre.device_number
                      }}</span>
                    </div>
                    <div class="w-full">
                      <i class="pi pi-qrcode product-category-icon"></i>
                      <span class="product-category">
                        {{ item.pre.barcode_id }}
                      </span>
                    </div>
                  </div>

                  <div class="flex w-full">
                    <div class="w-full">
                      <i class="pi pi-money-bill product-category-icon"></i>
                      <span class="product-category">
                        {{ item.pre.current_price.toLocaleString() }} VND
                      </span>
                    </div>
                    <div class="w-full">
                      <i class="pi pi-shopping-cart product-category-icon"></i>
                      <span class="product-category">
                        {{
                          moment(new Date(item.pre.purchase_date)).format(
                            "DD/MM/YYYY"
                          )
                        }}
                      </span>
                    </div>
                  </div>
                </template>
                <template #content>
                  <p>Tình trạng: {{ item.pre.condition }}</p>
                </template>
              </Card> -->
              </div>
              <div class="col-2 p-0 format-center">
                <i style="font-size: 26px" class="pi pi-sync"></i>
              </div>
              <div
                class="col-5 p-0 border-2 border-blue-300 p-2 cursor-pointer"
                v-if="item.next"
                @click="showReplaceAssets(item.pre)"
              >
                <div class="product-item">
                  <div class="image-container">
                    <img
                      :src="
                        item.next.image
                          ? basedomainURL + item.next.image
                          : basedomainURL + '/Portals/Image/noimg.jpg'
                      "
                      width="100%"
                      style="object-fit: cover"
                      class="p-0 cursor-pointer img-d"
                      preview
                      :alt="item.next.image"
                      @error="
                        $event.target.src =
                          basedomainURL + '/Portals/Image/noimg.jpg'
                      "
                    />
                  </div>
                  <div class="product-list-detail">
                    <h4 class="mb-2 mt-0">
                      {{ item.next.device_name }}
                    </h4>

                    <div class="flex pb-2">
                      <div class="w-full" v-tooltip.top="'Số hiệu'">
                        <i class="pi pi-tag product-category-icon"></i>
                        <span class="product-category">{{
                          item.next.device_number
                        }}</span>
                      </div>

                      <!-- <div class="w-full" v-tooltip.top="'Mã barcode'">
                    <i class="pi pi-qrcode product-category-icon"></i>
                    <span class="product-category">{{
                      item.next.barcode_id
                    }}</span>
                  </div> -->
                    </div>
                    <div class="flex">
                      <div class="w-full" v-tooltip.top="'Nguyên giá'">
                        <i class="pi pi-money-bill product-category-icon"></i>
                        {{ item.next.price.toLocaleString() }} VND
                      </div>
                      <!-- <div class="w-full" v-else v-tooltip.top="'Phòng ban quản lý'">
                    <i class="pi pi-home product-category-icon"></i>
                    {{ item.pre.manage_department_name }}
                  </div> -->
                      <!-- <div class="w-full" v-tooltip.top="'Ngày mua'">
                    <i class="pi pi-shopping-cart product-category-icon"></i>
                    <span class="product-category">
                      {{
                        moment(new Date(item.next.purchase_date)).format(
                          "DD/MM/YYYY"
                        )
                      }}
                    </span>
                  </div> -->
                    </div>
                  </div>
                </div>
              </div>
              <div
                @click="showReplaceAssets(item.pre)"
                class="
                  col-5
                  p-0
                  border-2 border-blue-300
                  p-2
                  format-center
                  font-italic
                  cursor-pointer
                "
                v-else
              >
                Chọn thiết bị thay thế!
              </div>
            </div>

            <!-- <div class="w-full format-center font-bold text-lg py-2">
              <Button
                @click="showReplaceAssets(item.pre)"
                label="Chọn thiết bị thay thế"
                icon="pi pi-plus"
              />
            </div> -->
          </div>
        </div>

        <div class="field col-12 md:col-12 flex format-center pt-2">
          <div
            class="col-6 p-0"
            v-if="filterSTypeH == 1 || device_handover.receipt_type == 0"
          >
            <Button
              @click="showListAssets"
              label="Chọn thiết bị"
              icon="pi pi-plus"
            />
          </div>
          <!-- <div
            class="col-6 p-0"
            v-if="
              selectedRepairHandover &&
              device_handover.receipt_type != 0 &&
              filterSTypeH != 1
            "
          >
            <Button
              @click="showSwitchAssets"
              label="Chọn thiết bị cần thay thế "
              icon="pi pi-plus"
            />
          </div> -->
        </div>

        <div class="col-12 p-0 flex" v-if="filterSTypeH == 1">
          <div class="w-10rem px-2"></div>
          <div style="width: calc(100% - 10rem)">
            <div
              class="w-full p-0 pt-2"
              v-for="(item, index) in selectedCard"
              :key="index"
            >
              <div
                style="border-radius: 10px"
                class="
                  product-item
                  border-3 border-solid border-round-3xl border-blue-200
                  surface-50
                  p-2
                "
              >
                <div class="image-container pr-2">
                  <img
                    :src="
                      item.image
                        ? basedomainURL + item.image
                        : basedomainURL + '/Portals/Image/noimg.jpg'
                    "
                    style="object-fit: cover; width: 125px; height: 75px"
                  />
                </div>
                <div class="product-list-detail">
                  <h5 class="my-2 text-justify">
                    {{ item.device_name }}
                  </h5>

                  <div class="flex pb-2">
                    <div class="w-full">
                      <i class="pi pi-tag product-category-icon"></i>
                      <span class="product-category">{{
                        item.device_number
                      }}</span>
                    </div>
                    <div class="w-full">
                      <i class="pi pi-qrcode product-category-icon"></i>
                      <span class="product-category">{{
                        item.barcode_id
                      }}</span>
                    </div>
                  </div>
                  <div class="flex">
                    <div class="w-full">
                      <i class="pi pi-home product-category-icon"></i>
                      <span class="product-category">
                        {{ item.warehouse_name }}
                      </span>
                    </div>
                    <div class="w-full">
                      <i class="pi pi-shopping-cart product-category-icon"></i>
                      <span class="product-category">
                        {{
                          moment(new Date(item.purchase_date)).format(
                            "DD/MM/YYYY"
                          )
                        }}
                      </span>
                    </div>
                  </div>
                </div>
                <div v-if="isSaveCard">
                  <Button
                    icon="pi pi-times"
                    class="p-button-rounded p-button-danger"
                    @click="deleteFileD(item)"
                  />
                </div>
              </div>
            </div>
          </div>
        </div>

        <div class="col-12 flex p-0 field mt-2 pt-2">
          <div class="w-10rem p-0">File đính kèm</div>
          <div class="p-0" style="width: calc(100% - 10rem)">
            <FileUpload
              chooseLabel="Chọn File"
              :showUploadButton="false"
              :showCancelButton="false"
              :multiple="false"
              :maxFileSize="524288000"
              @select="onUploadFile"
              @remove="removeFile"
              :invalidFileSizeMessage="'{0}: Dung lượng File không được lớn hơn {1}'"
            >
              <template #empty>
                <p class="p-0 m-0 text-500">Kéo thả hoặc chọn File.</p>
              </template>
            </FileUpload>
          </div>
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
                  <Button
                    icon="pi pi-times"
                    class="p-button-rounded p-button-danger"
                    @click="deleteFileH(item)"
                  />
                </template>
              </Toolbar>
            </div>
          </div>
        </div>
        <div class="col-12 flex p-0 field">
          <div class="w-10rem p-0">Ghi chú khi in</div>
          <div class="w-3rem">
            <InputSwitch v-model="device_handover.print_note_tf" />
          </div>
        </div>
        <div class="col-12 flex p-0 field" v-if="device_handover.print_note_tf">
          <div class="w-10rem p-0"></div>
          <Textarea
            style="width: calc(100% - 10rem); line-height: 24px"
            v-model="device_handover.print_note"
            rows="5"
            cols="30"
          />
        </div>
        <!-- <div class="field col-12 md:col-12 flex format-center pt-2">
          <div class="col-6 p-0">
            <SelectButton
              v-model="device_handover.receipt_type"
              :options="listTypePR"
              optionLabel="name"
              optionValue="code"
            />
          </div>
        </div> -->
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
          @click="saveHandover(!v$.$invalid, true)"
          label="Lưu và In"
          icon="pi pi-print"
        />
        <Button
          @click="saveHandover(!v$.$invalid, false)"
          label="Lưu"
          icon="pi pi-check"
          class="p-button-success"
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
    :modal="true"
  >
    <div>
      <div class="true flex-grow-1 p-2" id="scrollTop">
        <div class="grid p-0">
          <div class="col-12 field flex" style="justify-content: center">
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
            <div>
              <Dropdown
                v-model="options.warehouse_id"
                :options="listTWarehouse"
                :filter="true"
                optionLabel="name"
                optionValue="code"
                @change="filterDeviceMain()"
                class="ml-2 w-15rem"
                panelClass="d-design-dropdown"
                placeholder="Chọn kho"
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
          @move-to-target="changeDeviceList($event)"
          @move-all-to-target="changeDeviceList($event)"
          @move-to-source="changeDeviceAllList($event)"
          @move-all-to-source="changeDeviceAllList($event)"
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
      <Button @click="print()" label="In phiếu" icon="pi pi-print" />
      <Button @click="exportWord()" label="Xuất Word" icon="pi pi-file" />
    </template>
  </Dialog>

  <Dialog
    header="Chi tiết thiết bị"
    v-model:visible="displayDetailsDevice"
    :maximizable="true"
    :style="{ width: '80vw' }"
    :modal="true"
  >
    <div v-if="displayDetailsDevice && dataPropsD" style="min-height: 80vh">
      <detailsDevice :device="dataPropsD" />
    </div>
  </Dialog>

  <Dialog
    header="Chọn thiết bị từ phiếu sửa chữa"
    v-model:visible="displaySwitchAssets"
    :maximizable="true"
    :style="{ width: '55vw' }"
    :modal="true"
  >
    <div class=""></div>

    <div
      class="font-bold pt-5 pb-3 text-xl format-center"
      v-if="selectedRepairDevice.length > 0"
    >
      Danh sách thiết bị thay thế
    </div>
    <div>
      <div class="col-12 p-0">
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
          v-for="(item, index) in dataselectRP"
          :key="index"
        >
          <div class="col-12 p-0 field flex">
            <div class="col-5 p-0">
              <Card class="w-full h-full">
                <template #header>
                  <Image
                    :src="
                      item.pre.image
                        ? basedomainURL + item.pre.image
                        : basedomainURL + '/Portals/Image/noimg.jpg'
                    "
                    width="100%"
                    height="250"
                    imageStyle="object-fit: cover"
                    style="object-fit: cover; width: 100%"
                    class="p-8 cursor-pointer"
                    preview
                    :alt="item.pre.image"
                    @error="
                      $event.target.src =
                        basedomainURL + '/Portals/Image/noimg.jpg'
                    "
                  />
                </template>
                <template #title>
                  <span style="font: size 1.25rem !important">
                    {{ item.pre.device_name }}
                  </span>
                </template>
                <template #subtitle>
                  <div class="flex w-full">
                    <div class="w-full">
                      <i class="pi pi-tag product-category-icon"></i>
                      <span class="product-category">{{
                        item.pre.device_number
                      }}</span>
                    </div>
                    <div class="w-full">
                      <i class="pi pi-qrcode product-category-icon"></i>
                      <span class="product-category">
                        {{ item.pre.barcode_id }}
                      </span>
                    </div>
                  </div>

                  <div class="flex w-full">
                    <div class="w-full">
                      <i class="pi pi-money-bill product-category-icon"></i>
                      <span class="product-category">
                        {{ item.pre.current_price.toLocaleString() }} VND
                      </span>
                    </div>
                    <div class="w-full">
                      <i class="pi pi-shopping-cart product-category-icon"></i>
                      <span class="product-category">
                        {{
                          moment(new Date(item.pre.purchase_date)).format(
                            "DD/MM/YYYY"
                          )
                        }}
                      </span>
                    </div>
                  </div>
                </template>
                <template #content>
                  <p>Tình trạng: {{ item.pre.condition }}</p>
                </template>
              </Card>
            </div>
            <div class="col-2 p-0 format-center">
              <i style="font-size: 26px" class="pi pi-fast-forward"></i>
            </div>
            <div class="col-5 p-0">
              <Card class="w-full h-full" v-if="item.next">
                <template #header>
                  <Image
                    :src="
                      item.next.image
                        ? basedomainURL + item.next.image
                        : basedomainURL + '/Portals/Image/noimg.jpg'
                    "
                    width="100%"
                    height="250"
                    imageStyle="object-fit: cover"
                    style="object-fit: cover; width: 100%"
                    class="p-8 cursor-pointer"
                    preview
                    :alt="item.next.image"
                    @error="
                      $event.target.src =
                        basedomainURL + '/Portals/Image/noimg.jpg'
                    "
                  />
                </template>
                <template #title>
                  <span style="font: size 1.25rem">
                    {{ item.next.device_name }}</span
                  >
                </template>
                <template #subtitle>
                  <div class="flex w-full">
                    <div class="w-full">
                      <i class="pi pi-tag product-category-icon"></i>
                      <span class="product-category">{{
                        item.next.device_number
                      }}</span>
                    </div>
                    <div class="w-full">
                      <i class="pi pi-qrcode product-category-icon"></i>
                      <span class="product-category">
                        {{ item.next.barcode_id }}
                      </span>
                    </div>
                  </div>

                  <div class="flex w-full">
                    <div class="w-full">
                      <i class="pi pi-money-bill product-category-icon"></i>
                      <span class="product-category">
                        {{ item.next.current_price.toLocaleString() }} VND
                      </span>
                    </div>
                    <div class="w-full">
                      <i class="pi pi-shopping-cart product-category-icon"></i>
                      <span class="product-category">
                        {{
                          moment(new Date(item.next.purchase_date)).format(
                            "DD/MM/YYYY"
                          )
                        }}
                      </span>
                    </div>
                  </div>
                </template>
                <template #content>
                  <p>Tình trạng: {{ item.next.assets_condition }}</p>
                </template>
              </Card>
            </div>
          </div>

          <div class="w-full format-center font-bold text-lg py-2">
            <Button
              @click="showReplaceAssets(item.pre)"
              label="Chọn thiết bị thay thế"
              icon="pi pi-plus"
            />
          </div>
        </div>
      </div>
    </div>
    <template #footer>
      <Button
        @click="closeSwitchRepairReal"
        label="Đóng"
        icon="pi pi-times"
        autofocus
        class="mt-2 p-button-outlined"
      />
      <Button
        @click="closeSwitchRepair"
        label="Xác nhận thay thế"
        icon="pi pi-check"
        autofocus
        class="mt-2"
      />
    </template>
  </Dialog>
  <Dialog
    header="Chọn thiết bị thay thế"
    v-model:visible="displayReplaceAssets"
    :maximizable="true"
    :style="{ width: '70vw' }"
    :modal="true"
  >
    <div class="flex w-full pt-2 align-items-center">
      <DataTable
        class="w-full p-datatable-sm e-sm"
        @page="onPageRP($event)"
        @filter="onFilterRP($event)"
        @sort="onSortRP($event)"
        v-model:filters="filtersRP"
        removableSort
        filterDisplay="menu"
        filterMode="lenient"
        dataKey="card_id"
        responsiveLayout="scroll"
        :scrollable="true"
        scrollHeight="flex"
        :showGridlines="true"
        :rows="options.pagesizeRP"
        :lazy="true"
        :value="datalistsRP"
        :loading="options.loadingRP"
        :paginator="true"
        :totalRecords="options.totalRecordsRP"
        :row-hover="true"
        v-model:first="firstRP"
        v-model:selection="selectedCardRP"
        :pageLinkSize="options.pagesizeRP"
        selectionMode="single"
        paginatorTemplate="FirstPageLink PrevPageLink PageLinks  NextPageLink LastPageLink     "
      >
        <template #header>
          <Toolbar class="d-toolbar p-0 py-3 surface-50">
            <template #start>
              <div class="flex align-items-center">
                <span class="p-input-icon-left">
                  <i class="pi pi-search" />
                  <InputText
                    v-model="options.search"
                    @keyup.enter="loadDataSQLRP()"
                    type="text"
                    spellcheck="false"
                    placeholder="Tìm kiếm"
                  />
                </span>
              </div>
              <!-- <div class="flex align-items-center">
                <Dropdown
                  v-model="filterCardWarehouseRP"
                  :options="listWarehouse"
                  :filter="true"
                  optionLabel="name"
                  optionValue="code"
                  panelClass="d-design-dropdown"
                  placeholder="Chọn kho thiết bị"
                  class="mx-3"
                  style="min-width: 200px !important"
                  @change="loadDataSQLRP"
                >
                </Dropdown>
              </div> -->
            </template>
          </Toolbar>
        </template>
        <Column
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;max-width:50px;height:50px"
          bodyStyle="text-align:center;max-width:50px"
          selectionMode="single"
        >
        </Column>
        <Column
          :sortable="true"
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;max-width:70px;height:50px"
          bodyStyle="text-align:center;max-width:70px;overflow: hidden;"
          field="STT"
          header="STT"
        >
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
                v-if="data.data.status == 'TK'"
                :label="data.data.device_status_name"
                class="
                  w-full
                  bg-green-300
                  justify-content-center
                  p-button-status-d
                "
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
    <template #footer>
      <Button
        @click="closeReplaceRepair"
        label="Xác nhận chọn"
        icon="pi pi-check"
        autofocus
      />
    </template>
  </Dialog>
  <Dialog
    header="Chi tiết phiếu sửa chữa"
    v-model:visible="displayDetailsHandoverRepair"
    :maximizable="true"
    :style="{ width: '70vw' }"
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
              <div class="col-4 p-0 pl-5 text-left">Nơi lập</div>
              <InputText
                v-model="device_repair.repair_created_place"
                class="w-full class-disabled"
                :disabled="true"
              />
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
        @click="closeDetailsHandoverRepair"
        label="Đóng"
        icon="pi pi-times"
        autofocus
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
  <printDocHandover :datas="device_handover" :listAssets="listAssetsH" />
  <printListHandover :datas="datalists" />
</template>


<style scoped>
.img-d {
  width: 125px;
  height: 75px;
  object-fit: cover;
  box-shadow: 0 3px 6px rgba(0, 0, 0, 0.16), 0 3px 6px rgba(0, 0, 0, 0.23);
  margin-right: 1rem;
}
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

.d-avatar-device_handover {
  position: relative;
  width: 100%;
  height: 350px;
}
.d-avatar-device_handover img {
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
    