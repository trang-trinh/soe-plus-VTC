<script setup>
import { ref, inject, onMounted } from "vue";
import { useToast } from "vue-toastification";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import moment from "moment";
import detailsDevice from "../../components/device/detailsDevice.vue";
import configAprroved from "../../components/device/configAprroved.vue";
import detailsRecall from "../../components/device/detailsRecall.vue";
 
import deviceProcedure from "../../components/device/deviceProcedure.vue";
 
 
import { encr, checkURL } from "../../util/function.js";
//Khai báo
const router = inject("router");
const cryoptojs = inject("cryptojs");
//Khai báo device_card_get
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const basedomainURL = baseURL;
const emitter = inject("emitter");
 
 
//Nơi nhận dữ liệu

emitter.on("emitData", (obj) => {
  switch (obj.type) {
    case "sendAccept_1":
      loadData();
      displayDeviceRepair.value = false;
      displayDetailsHandover.value = false;
      displayDetailsInventory.value = false;
      displayDetailsRecall.value = false;

      break;
    case "hideAccept":
      displayDeviceRepair.value = false;
      break;
    default:
      break;
  }
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
        key: "repair_created_date",
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
        key: "repair_created_date",
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
        key: "repair_created_date",
      };
      filterSQL.value.push(filterS1);
    }
  }
  isDynamicSQL.value = true;
  loadData(true);
};

const filters = ref({
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
  repair_created_date: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.DATE_BEFORE }],
  },
  repair_created_date: {
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
const filterSQL = ref([]);
const isDynamicSQL = ref(false);
const loadDataSQL = () => {
  let data = {
    id: null,
    next: options.value.IsNext,
    sqlO: options.value.sort,
    Search: options.value.search,
    PageNo: options.value.pageno,
    PageSize: options.value.pagesize,
    fieldSQLS: filterSQL.value,
  };

  options.value.loading = true;
  axios
    .post(baseURL + "/api/SQL/Filter_device_process", data, config)
    .then((response) => {
      let dt = JSON.parse(response.data.data);
      let data = dt[0];
      if (data.length > 0) {
        data.forEach((element, i) => {
          element.is_order =
            options.value.pageno * options.value.pagesize + i + 1;
          element.classify_fake = element.classify.split(",");
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

      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
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

const toast = useToast();
const isFirst = ref(true);
const datalists = ref();
const isSaveCard = ref(false);
const sttCard = ref(1);
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const options = ref({
  IsNext: true,
  sort: " device_process_id DESC",
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
const device_process = ref({
  is_order: 1,
  proposal_code: "",
  device_id: null,
  repair_number: "",
  image: "",
  barcode_id: "",
  barcode_type: 0,
  barcode_image: "",
  status: 0,
  department_id_fake: {},
});
const danhMuc = ref();
//METHOD

const displayDetails = ref(false);
const listAssetsH = ref();
const openDetails = (device_process_id) => {
  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "device_process_card_list",
            par: [{ par: "device_process_id", va: device_process_id }],
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

const liItemsAccept = ref([]);

const onShowConfigGroup = (type, module) => {
  if (type == 1) {
     console.log("");
  } else {
    checkDefault.value = false;
    displayDeviceRepair.value = true;
  }
};
const checkReturnCreated = ref(false);
const onCloseReturnCreated = () => {
  if (device_process.value.is_last) {
    checkReturnCreated.value = true;
    isShowReturn.value = true;
  } else {
    onReturnCreated(device_process.value);
  }
};
const onCloseReturnRepair = () => {
  if (device_process.value.is_last) {
    checkReturnCreated.value = false;
    isShowReturn.value = true;
  } else {
    onReturnProposer(device_process.value);
  }
};
const openDetailsHandover = (data) => {
  if (data.device_process_type == 1) {
    axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
          proc: "device_process_get",
          par: [{ par: "device_process_id", va: data.device_process_id }],
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
        device_process.value = data[0];
        device_process.value.classify_fake =
          device_process.value.classify.split(",");

        displayDetailsHandover.value = true;
        listAssetsH.value = data1;
        listAssetsH.value.forEach((element) => {
          element.repair_condition_fake = element.repair_condition;
        });
        listFilesS.value = data2;
        listUserFollows.value = data3;
      })
      .catch((error) => {});
  }
  if (data.device_process_type == 2) {
    listUserA.value = [];
    files.value = [];
    axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
          proc: "device_process_inventory_get",
          par: [{ par: "device_process_id", va: data.device_process_id }],
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
        let data4 = JSON.parse(response.data.data)[4];
        let data5 = JSON.parse(response.data.data)[5];

        device_inventory.value = data[0];

        if (device_inventory.value) {
          if (device_inventory.value.inventory_date) {
            device_inventory.value.inventory_date = new Date(
              device_inventory.value.inventory_date
            );
          }
          if (device_inventory.value.inventory_created_date) {
            device_inventory.value.inventory_created_date = new Date(
              device_inventory.value.inventory_created_date
            );
          }
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

        device_process.value = data4[0];

        device_process.value.classify_fake =
          device_process.value.classify.split(",");
        // listFilesS.value = data5;
        listUserFollows.value = data5;

        isSaveCard.value = true;
        displayDetailsInventory.value = true;
      })
      .catch((error) => {});
  }
  if (data.device_process_type == 3) {
    device_recall_id.value = data.device_note_id;
    device_process.value = data;
    displayDetailsRecall.value = true;
  }
};
const device_recall_id = ref();
const device_inventory = ref();
const selectedCard = ref();
const listUserA = ref();
const displayDetailsRecall = ref(false);
const displayDetailsInventory = ref(false);
const dataPropsD = ref();
const displayDeviceRepair = ref(false);
const displayDetailsDevice = ref(false);
const showDetailsDevice = (value) => {
  dataPropsD.value = value;
  displayDetailsDevice.value = true;
};
const closeDetailsInventory = () => {
  displayDetailsInventory.value = false;
  device_process.value = null;
};
const closeDetailsHandover = () => {
  displayDetailsHandover.value = false;
  device_process.value = null;
};
const displayDetailsHandover = ref(false);
const loadCount = () => {
  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "device_process_count",
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
      if (data.length > 0) {
        options.value.totalRecords = data[0].totalRecords;
        sttCard.value = data[0].totalRecords + 1;
      } else options.value.totalRecords = 0;
    })
    .catch((error) => {});
};
const listFilesS = ref([]);
const listUserFollows = ref([]);
const menSendAccept = ref();

const deviceAprrovedId = ref();
const checkDefault = ref(false);
const isShowReturn = ref(false);
const listTypeRepair = ref([
  { name: "Hoàn thành", code: 1 },
  { name: "Chuyển kho sửa chữa", code: 2 },
  { name: "Hỏng không sửa chữa được", code: 3 },
]);

const sendAccept = (value) => {
  device_process.value = value;
  liItemsAccept.value = [];
  
  if (value.classify_fake.includes("17") && value.is_last) {
    liItemsAccept.value.push({
      label: "Hoàn thành",
      icon: "pi pi-check",
      command: () => {
        if (device_process.value.device_process_type == 1) {
          onAcceptCompleted(device_process.value);

          toast.success("Hoàn thành sửa chữa!");
        }
        if (device_process.value.device_process_type == 2) {
          onAcceptCompleted(device_process.value);

          toast.success("Hoàn thành kiểm kê!");
        }
        if (device_process.value.device_process_type == 3) {
          onAcceptCompleted(device_process.value);

          toast.success("Hoàn thành thu hồi!");
        }
      },
    });
  }

  if (value.is_return_created == true) {
    liItemsAccept.value.push({
      label: "Trả lại người duyệt trước",
      icon: "pi pi-undo",
      command: () => {
        onReturnCreated(device_process.value);
      },
    });
  }
  if (value.is_suggest_repair == true) {
    liItemsAccept.value.push({
      label: "Trả người đề nghị sửa",
      icon: "pi pi-reply",
      command: () => {
        if (device_process.value.device_process_type == 1) {
          // isShowReturn.value = true;
          onReturnProposer(device_process.value);
        }
      },
    });
  }

  if (
    value.is_last ||
    value.is_approved_by_department ||
    value.approved_type == 1
  ) {
    liItemsAccept.value.push({
      label: "Chọn nhóm duyệt",
      icon: "pi pi-desktop",
      command: () => {
        if (device_process.value.device_process_type == 1) {
          onShowConfigGroup(2, "TS_PhieuSuaChua");
        }
        if (device_process.value.device_process_type == 2) {
          onShowConfigGroup(2, "TS_PhieuKiemKe");
        }
        if (device_process.value.device_process_type == 3) {
          onShowConfigGroup(2, "TS_PhieuThuHoi");
        }
      },
    });
  } else {
    liItemsAccept.value.push({
      label: "Xác nhận duyệt",
      icon: "pi pi-check",
      command: () => {
        swal
          .fire({
            title: "Thông báo",
            text: "Bạn có muốn xác nhận duyệt phiếu này không!",
            icon: "warning",
            showCancelButton: true,
            confirmButtonColor: "#3085d6",
            cancelButtonColor: "#d33",
            cancelButtonText: "Không",
            confirmButtonText: "Có",
          })
          .then((result) => {
            if (result.isConfirmed) {
              swal.fire({
                width: 110,
                didOpen: () => {
                  swal.showLoading();
                },
              });

              onAcceptApprovedRepair(device_process.value);

              swal.close();
            }
          });
      },
    });
  }

  menSendAccept.value.toggle(event);
};

const onReturnProposer = (value) => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  let formData = new FormData();

  for (var i = 0; i < files.value.length; i++) {
    let file = files.value[i];
    formData.append("image", file);
  }
  formData.append("process", JSON.stringify(value));
  formData.append("filesize", JSON.stringify(fileSize));
  axios
    .put(baseURL + "/api/device_process/return_device_repair", formData, config)
    .then((response) => {
      if (response.data.err != "1") {
        displayDetailsHandover.value = false;
        toast.success("Trả lại thành công!");
        loadData(true);
        swal.close();
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
const onReturnCreated = (value) => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  let formData = new FormData();
  for (var i = 0; i < files.value.length; i++) {
    let file = files.value[i];
    formData.append("image", file);
  }
  formData.append("process", JSON.stringify(value));
  formData.append("filesize", JSON.stringify(fileSize));
  axios
    .put(
      baseURL + "/api/device_process/return_device_process",
      formData,
      config
    )
    .then((response) => {
      if (response.data.err != "1") {
        displayDetailsHandover.value = false;
        displayDetailsRecall.value = false;
        displayDetailsInventory.value = false;

        toast.success("Trả lại thành công!");
        loadData(true);
        swal.close();
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

let fileSize = [];
const onUploadFile = (event) => {
  files.value = [];
  fileSize = [];
  event.files.forEach((element) => {
    files.value.push(element);
    fileSize.push(element.size);
  });
};
const removeFile = (event) => {
  files.value = files.value.filter((a) => a != event.file);
};
const onReturnD = (value) => {
  if (checkReturnCreated.value) {
    onReturnCreated(value);
  } else {
    onReturnProposer(value);
  }
  isShowReturn.value = false;
};
const deleteFileD = (value) => {
  selectedCard.value = selectedCard.value.filter(
    (x) => x.card_id != value.card_id
  );
};
const files = ref([]);
const isshowCompleted = ref(false);
const onShowCompleted = () => {
  isshowCompleted.value = true;
};

const onAcceptCompleted = (value) => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  if (listAssetsH.value) {
    listAssetsH.value.forEach((element) => {
      element.repair_condition = element.repair_condition_fake;
    });
  }
  let formData = new FormData();
  for (var i = 0; i < files.value.length; i++) {
    let file = files.value[i];
    formData.append("image", file);
  }
  value.pre_process_id = null;
  formData.append("process", JSON.stringify(value));
  formData.append("filesize", JSON.stringify(fileSize));
  formData.append(
    "details",
    value.device_process_type == 1 ? JSON.stringify(listAssetsH.value) : null
  );
  axios
    .put(
      baseURL + "/api/device_process/finish_device_process",
      formData,
      config
    )
    .then((response) => {
      if (response.data.err != "1") {
        displayDetailsHandover.value = false;
        displayDetailsInventory.value = false;
        displayDetailsRecall.value = false;
        isshowCompleted.value = false;

        loadData(true);

        swal.close();
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

const deviceDataDetails = ref();
//Sửa bản ghi
const menuDetailsR = ref();
const onShowDetails = (value) => {
  deviceDataDetails.value = value;
  menuDetailsR.value.toggle(event);
};
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
const displayProcedure = ref(false);
const closeDialogProcedure = () => {
  displayProcedure.value = false;
};

const processListViews = ref([]);

const onShowProcedure = () => {
  processListViews.value = [];
  listLogs.value = [];
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

        if (element.listprocess.length > 0) {
          if (element.listprocess.length > 1000) {
            lengthFor.value = element.listprocess.length;
          } else {
            lengthFor.value = 1000;
          }
        } else {
          lengthFor.value = 0;
        }
        element.listprocess = compareProcess(element.listprocess);
        element.aprroved_user = null;
        element.device_process = null;
      });

      processListViews.value.push(data);
      processListViews.value.push(data1);
      loadLogs(deviceDataDetails.value.device_note_id,deviceDataDetails.value.device_process_type);
      displayProcedure.value = true;
      // loadDTliView(data[0].approved_group_id)
    })
    .catch((error) => {});
};
const listLogs = ref([]);
const loadLogs=(device_note_id, type)=>{
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
  //  if( lengthFor.value>0){
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
  data.forEach((element, index) => {
    if (element.is_approved != null) {
      for (let i = 0; i < index; i++) {
        data[i].is_approved = "2";
      }
    }
  });
  return data;

  //   for (let index = 0; index < data.length - 1; index++) {
  //   const element = data[index];
  //   for (let Jindex = 1; Jindex < data.length; Jindex++) {
  //     const Jelement = data[Jindex];
  //     if (data[index].user_stt > data[Jindex].user_stt) {
  //       data[index] = Jelement;
  //       data[Jindex] = element;
  //     }
  //   }
  // }
  // for (let index = 0; index < data.length; index++) {
  //   const element = data[index];
  //   if (element.is_approved != null) {
  //     for (let i = 0; i < index; i++) {

  //       data[i].is_approved = "2";

  //     }
  //   }
  // }
  // return data;
}
const onAcceptApprovedRepair = (value) => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  if (listAssetsH.value) {
    listAssetsH.value.forEach((element) => {
      element.repair_condition = element.repair_condition_fake;
    });
  }
  let formData = new FormData();
  for (var i = 0; i < files.value.length; i++) {
    let file = files.value[i];
    formData.append("image", file);
  }
  formData.append("process", JSON.stringify(value));
  formData.append("filesize", JSON.stringify(fileSize));
  formData.append("userfollows", JSON.stringify([]));
  formData.append(
    "details",
    value.device_process_type == 1 ? JSON.stringify(listAssetsH.value) : null
  );
  axios
    .put(
      baseURL + "/api/device_process/accept_device_process",
      formData,
      config
    )
    .then((response) => {
      if (response.data.err != "1") {
        displayDetailsHandover.value = false;
        displayDetailsInventory.value = false;
        displayDetailsRecall.value = false;
        loadData();

        swal.close();
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

//Hiển thị dialog

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
            proc: "device_process_list",
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

          element.classify_fake = element.classify.split(",");
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

const first = ref(0);

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
        listDropdownUser.value.push({
          name: element.full_name,
          code: element.user_id,
          avatar: element.avatar,
          department_name: element.department_name,
          role_name: element.role_name,
          position_name: element.position_name,
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

onMounted(() => {
  if (!checkURL(window.location.pathname, store.getters.listModule)) {
     //router.back();
  }
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
  <div class="d-device_process_idntainer">
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
        dataKey="device_process_id"
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
        :pageLinkSize="options.pagesize"
        paginatorTemplate="FirstPageLink PrevPageLink PageLinks  NextPageLink LastPageLink    RowsPerPageDropdown"
        :rowsPerPageOptions="[20, 30, 50, 100, 200]"
      >
        <template #header>
          <div>
            <h3 class="module-title my-2 ml-1">
              <i class="pi pi-book"></i> Danh sách phiếu chờ duyệt ({{
                options.totalRecords ? options.totalRecords : 0
              }})
            </h3>
          </div>
        </template>

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
        >
          <!-- <template #body="data">
            <div>
              {{data.data}}
            </div>
          </template> -->
        </Column>

        <Column
          headerStyle="text-align:center;height:50px;max-width:250px"
          bodyStyle="text-align:center;max-width:250px"
          field="proposer"
          header="Người gửi"
          headerClass="format-center"
          bodyClass="pl-3"
        >
          <template #body="data">
            <div class="format-center w-full">
              <div
                v-tooltip.bottom="{
                  value:
                    data.data.department_name  ,
                  class: 'custom-error-tl1',
                }"
                class="flex surface-100 align-items-center pr-2 format-center"
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
                      : 'background:' + bgColor[data.data.full_name.length % 7]
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
          field="repair_created_date"
          header="Ngày gửi"
        >
          <template #body="data">
            <div>
              {{ moment(new Date(data.data.date_send)).format("DD/MM/YYYY") }}
            </div>
          </template>
        </Column>

        <Column
          headerStyle="text-align:center;height:50px"
          bodyStyle="text-align:center"
          field="deviceFrom"
          header="Nội dung trình"
          ><template #body="data">
            <div>
              {{ data.data.content_send }}
            </div>
          </template>
        </Column>

        <Column
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;max-width:170px;height:50px"
          bodyStyle="text-align:center;max-width:170px"
          field="device_process_type"
          header="Loại phiếu"
        >
          <template #body="data">
            <div class="w-full">
              <Chip
                v-if="data.data.device_process_type == 1"
                label="Phiếu sửa chữa"
                class="w-full bg-cyan-300 justify-content-center"
              />
              <Chip
                v-else-if="data.data.device_process_type == 2"
                label="Phiếu kiểm kê"
                class="w-full bg-yellow-300 justify-content-center"
              />
              <Chip
                v-else-if="data.data.device_process_type == 3"
                label="Phiếu thu hồi"
                class="w-full bg-pink-300 justify-content-center"
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
            <Button
              v-tooltip.top="'Chi tiết phiếu'"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              @click="onShowDetails(data.data)"
              type="button"
              icon="pi pi-info-circle"
              aria-haspopup="true"
              aria-controls="overlay_menu_details"
            ></Button>

            <Menu
              id="overlay_menu_details"
              ref="menuDetailsR"
              :model="liItemsDetails"
              :popup="true"
            />
            <!-- <Button
              v-tooltip.left="'Xem phiếu'"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              @click="openDetailsHandover(data.data)"
              type="button"
              icon="pi pi-eye"
            ></Button> -->

            <div>
              <Button
                type="button"
                icon="pi pi-angle-double-right"
                class="
                  p-button-rounded p-button-secondary p-button-outlined
                  mx-1
                "
                v-tooltip.left="'Xác nhận'"
                @click="sendAccept(data.data)"
                aria-haspopup="true"
                aria-controls="overlay_menu"
              />
              <Menu
                id="overlay_menu"
                ref="menSendAccept"
                :model="liItemsAccept"
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
  <Sidebar
    class="p-sidebar-lg"
    :showCloseIcon="false"
    v-model:visible="displayDetails"
    position="right"
  >
    <div class="w-full format-center">
      <h3>Danh sách thiết bị sửa chữa kèm theo</h3>
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
              <i class="pi pi-shopping-cart product-category-icon"></i>
              <span class="product-category">
                {{ moment(new Date(item.purchase_date)).format("DD/MM/YYYY") }}
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
  </Sidebar>

  <Dialog
    header=" "
    class="appdock-1"
    v-model:visible="displayDetailsHandover"
    :maximizable="true"
    :style="{ width: '60vw' }"
    :modal="true"
  >
    <form>
      <TabView>
        <TabPanel class="mx-2">
          <template #header>
            <div class="flex align-items-center">
              <div class="pr-1 align-items-center format-center">
                <i class="pi pi-book"></i>
              </div>
              <div>
                <span>Thông tin phiếu</span>
              </div>
            </div>
          </template>
          <div class="grid m-0 px-3 formgrid flex surface-0">
            <div class="col-12 p-0 field flex">
              <div class="col-12 p-0">
                <div
                  class="
                    format-center
                    col-12
                    field
                    mb-5
                    font-bold
                    text-xl
                    py-0
                    px-3
                  "
                >
                  Thông tin chi tiết
                </div>

                <div class="col-12 field flex">
                  <div class="col-6 p-0 flex">
                    <div class="w-10rem p-0 font-bold">Số phiếu:</div>
                    <div class="p-0" style="width: calc(100% - 10rem)">
                      {{ device_process.device_process_code }}
                    </div>
                  </div>
                  <div class="col-6 p-0 flex align-items-center">
                    <div class="w-10rem p-0 font-bold">Loại phiếu:</div>
                    <div class="p-0" style="width: calc(100% - 10rem)">
                      {{
                        device_process.device_process_type == 1
                          ? "Sửa chữa"
                          : device_process.device_process_type == 2
                          ? "Kiểm kê"
                          : "Khác"
                      }}
                    </div>
                  </div>
                </div>
                <div class="col-12 field flex">
                  <div class="col-6 p-0 flex align-items-center">
                    <div class="w-10rem font-bold">Người đề xuất:</div>
                    <div style="max-width: calc(100% - 10rem)" class="p-0">
                      <div>
                        <div
                          class="flex w-full surface-0 align-items-center pr-2"
                          style="border-radius: 16px"
                        >
                          <Avatar
                            v-bind:label="
                              device_process.offer_user_avatar
                                ? ''
                                : device_process.offer_user_name.substring(
                                    device_process.offer_user_name.lastIndexOf(
                                      ' '
                                    ) + 1,
                                    device_process.offer_user_name.lastIndexOf(
                                      ' '
                                    ) + 2
                                  )
                            "
                            :image="
                              basedomainURL + device_process.offer_user_avatar
                            "
                            size="small"
                            :style="
                              device_process.offer_user_avatar
                                ? 'background-color: #2196f3'
                                : 'background:' +
                                  bgColor[
                                    device_process.offer_user_name.length % 7
                                  ]
                            "
                            shape="circle"
                            @error="
                              $event.target.src =
                                basedomainURL + '/Portals/Image/nouser1.png'
                            "
                          />
                          <div class="px-2">
                            {{ device_process.offer_user_name }}
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>

                  <div class="col-6 p-0 flex">
                    <div class="w-10rem font-bold">Ngày đề xuất:</div>
                    <div class="p-0" style="width: calc(100% - 10rem)">
                      {{
                        moment(
                          new Date(device_process.offer_user_created_date)
                        ).format("DD/MM/YYYY")
                      }}
                    </div>
                  </div>
                </div>

                <!-- <div class="field col-12 flex">
                  <div class="w-10rem font-bold">Nội dung:</div>
                  <div class="p-0" style="width: calc(100% - 10rem)">
                    {{ device_process.content }}
                  </div>
                </div> -->
                <div
                  v-if="listUserFollows.length > 0"
                  class="field col-12 flex align-items-center"
                >
                  <div class="w-10rem font-bold">Người theo dõi:</div>
                  <div class="p-0 flex" style="max-width: calc(100% - 10rem)">
                    <div
                      v-for="(item, index) in listUserFollows"
                      :key="index"
                      class="flex"
                    >
                      <div>
                        <div
                          v-tooltip.bottom="{
                            value:
                              item.department_name  ,
                            class: 'custom-error-tl1',
                          }"
                          class="flex w-full surface-0 align-items-center pr-2"
                          style="border-radius: 16px"
                        >
                          <Avatar
                            v-bind:label="
                              item.avatar
                                ? ''
                                : item.full_name.substring(
                                    item.full_name.lastIndexOf(' ') + 1,
                                    item.full_name.lastIndexOf(' ') + 2
                                  )
                            "
                            :image="basedomainURL + item.avatar"
                            size="small"
                            :style="
                              item.avatar
                                ? 'background-color: #2196f3'
                                : 'background:' +
                                  bgColor[item.full_name.length % 7]
                            "
                            shape="circle"
                            @error="
                              $event.target.src =
                                basedomainURL + '/Portals/Image/nouser1.png'
                            "
                          />
                          <div class="px-2">{{ item.full_name }}</div>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <div
              class="
                format-center
                col-12
                field
                mb-5
                font-bold
                text-xl
                py-0
                px-3
              "
            >
              Thông tin người gửi
            </div>
            <div class="col-12 field flex">
              <div class="col-6 p-0 flex align-items-center">
                <div class="w-10rem font-bold">Người gửi:</div>
                <div style="max-width: calc(100% - 10rem)" class="p-0">
                  <div>
                    <div
                      v-tooltip.bottom="{
                        value:
                          device_process.department_name ,
                        class: 'custom-error-tl1',
                      }"
                      class="flex w-full surface-0 align-items-center pr-2"
                      style="border-radius: 16px"
                    >
                      <Avatar
                        v-bind:label="
                          device_process.avatar
                            ? ''
                            : device_process.full_name.substring(
                                device_process.full_name.lastIndexOf(' ') + 1,
                                device_process.full_name.lastIndexOf(' ') + 2
                              )
                        "
                        :image="basedomainURL + device_process.avatar"
                        size="small"
                        :style="
                          device_process.avatar
                            ? 'background-color: #2196f3'
                            : 'background:' +
                              bgColor[device_process.full_name.length % 7]
                        "
                        shape="circle"
                        @error="
                          $event.target.src =
                            basedomainURL + '/Portals/Image/nouser1.png'
                        "
                      />
                      <div class="px-2">{{ device_process.full_name }}</div>
                    </div>
                  </div>
                </div>
              </div>

              <div class="col-6 p-0 flex">
                <div class="w-10rem font-bold">Ngày gửi:</div>
                <div class="p-0" style="width: calc(100% - 10rem)">
                  {{
                    moment(new Date(device_process.date_send)).format(
                      "DD/MM/YYYY"
                    )
                  }}
                </div>
              </div>
            </div>
            <!-- <div class="col-12 field flex">
              <div class="w-10rem font-bold">Nội dung:</div>
              <div style="max-width: calc(100% - 10rem)" class="p-0">
                {{ device_process.content }}
              </div>
            </div> -->

            <div class="col-12 field p-0">
              <div class="format-center col-12 field font-bold text-xl px-3">
                Danh sách thiết bị sửa chữa kèm theo
              </div>
              <div v-for="(item, index) in listAssetsH" :key="index">
                <div
                  class="mb-2"
                  style="border: 1px solid #33ccff; border-radius: 2px"
                ></div>
                <div
                  class="flex cursor-pointer"
                  @click="showDetailsDevice(item)"
                >
                  <div class="image-container pr-2">
                    <img
                      :src="
                        item.image
                          ? basedomainURL + item.image
                          : basedomainURL + '/Portals/Image/noimg.jpg'
                      "
                      class="pr-2"
                      style="object-fit: cover; width: 10rem; height: 75px"
                    />
                  </div>
                  <div class="product-list-detail">
                    <h5 class="my-2 text-justify" style="font-size: 16px">
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
                        <i
                          class="pi pi-shopping-cart product-category-icon"
                        ></i>
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
                </div>
                <div class="flex w-full py-2 align-items-center">
                  <div class="font-bold w-10rem">Tình trạng:</div>

                  <div style="width: calc(100% - 10rem)" class="">
                    <Textarea
                      class="w-full"
                      v-model="item.condition"
                      :autoResize="true"
                      rows="1"
                      cols="30"
                      spellcheck="false"
                      :disabled="
                        device_process.classify_fake.includes('17')   ||
                        (item.repair_condition == 3 &&
                          item.modified_by != store.getters.user.user_id)
                      "
                    />
                  </div>
                </div>
                <div class="flex w-full pt-2 align-items-center">
                  <div class="font-bold w-10rem">Phương hướng sửa:</div>
                  <div style="width: calc(100% - 10rem)" class="">
                    <Textarea
                      class="w-full"
                      v-model="item.repair_plan"
                      :autoResize="true"
                      rows="1"
                      cols="30"
                      spellcheck="false"
                      :disabled="
                        ( 
                          device_process.classify_fake.includes('11')) ||
                        (item.repair_condition == 3 &&
                          item.modified_by != store.getters.user.user_id)
                      "
                    />
                  </div>
                </div>
                <div class="flex w-full pt-2">
                  <div class="flex w-6 p-0 align-items-center">
                    <div class="font-bold w-10rem">Tình trạng sửa:</div>

                    <div style="width: calc(100% - 10rem)" class="">
                      <Dropdown
                        v-model="item.repair_condition_fake"
                        :options="listTypeRepair"
                        :filter="true"
                        optionLabel="name"
                        optionValue="code"
                        class="w-full"
                        panelClass="d-design-dropdown"
                        placeholder="Tình trạng sửa chữa"
                        :disabled="
                          device_process.classify_fake.includes('11') ==
                            false ||
                          (item.repair_condition == 3 &&
                            item.modified_by != store.getters.user.user_id)
                        "
                      />
                    </div>
                  </div>
                  <div class="flex w-6 p-0 align-items-center">
                    <div class="font-bold w-10rem format-center">
                      Giá sửa chữa:
                    </div>
                    <div style="width: calc(100% - 10rem)" class="">
                      <InputNumber
                        class="w-full p-0"
                        v-model="item.repair_price"
                        suffix=" VND"
                        :disabled="
                          device_process.classify_fake.includes('11') ==
                            false ||
                          (item.repair_condition == 3 &&
                            item.modified_by != store.getters.user.user_id)
                        "
                      >
                      </InputNumber>
                    </div>
                  </div>
                </div>

                <div class="flex w-full pt-2 align-items-center">
                  <div class="font-bold w-10rem">Ghi chú</div>
                  <div style="width: calc(100% - 10rem)" class="">
                    <Textarea
                      class="w-full"
                      v-model="item.repair_note"
                      :disabled="
                        item.repair_condition == 3 &&
                        item.modified_by != store.getters.user.user_id
                      "
                      :autoResize="true"
                      rows="1"
                      cols="30"
                      spellcheck="false"
                    />
                  </div>
                </div>
              </div>
            </div>
            <div class="col-12 p-0 field" v-if="listFilesS.length > 0">
              <div class="format-center font-bold text-xl p-2">
                Files đính kèm
              </div>
              <div class="w-full p-0">
                <div
                  class="p-0 mb-2 field w-full"
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
                          <div
                            v-if="checkImg(item.file_path)"
                            class="align-items-center flex"
                          >
                            <Image
                              :src="basedomainURL + item.file_path"
                              :alt="item.process_files_name"
                              imageStyle="
                                object-fit: cover;
                                border: 1px solid #ccc;
                                width: 8rem;
                                height: 75px;
                              "
                              preview
                              @error="
                                $event.target.src =
                                  basedomainURL + '/Portals/Image/noimg.jpg'
                              "
                              class="pr-2"
                            />
                            <div class="ml-2">
                              {{ item.process_files_name }}
                            </div>
                          </div>
                          <div v-else>
                            <a
                              :href="basedomainURL + item.file_path"
                              download
                              class="w-full no-underline"
                            >
                              <div class="align-items-center flex">
                                <img
                                  :src="
                                    basedomainURL +
                                    '/Portals/Image/file/' +
                                    item.file_path.substring(
                                      item.file_path.lastIndexOf('.') + 1
                                    ) +
                                    '.png'
                                  "
                                  style="
                                    width: 8rem;
                                    height: 75px;
                                    object-fit: contain;
                                  "
                                  :alt="item.process_files_name"
                                />
                                <div class="ml-2" style="line-height: 50px">
                                  {{ item.process_files_name }}
                                </div>
                              </div>
                            </a>
                          </div>
                        </div>
                      </template>
                      <template #end> </template>
                    </Toolbar>
                  </div>
                </div>
              </div>
            </div>
            <div
              class="col-12 p-0"
              v-if="
                !device_process.is_last && device_process.approved_type != 1
              "
            >
              <div class="format-center col-12 field font-bold text-xl px-3">
                Thông tin phiếu gửi
              </div>
              <div class="field col-12 p-0 flex">
                <div class="flex w-full p-0 pt-2 align-items-center">
                  <div class="font-bold w-10rem">Nội dung:</div>
                  <div style="width: calc(100% - 10rem)" class="">
                    <Textarea
                      class="w-full"
                      v-model="device_process.content"
                      :autoResize="true"
                      rows="2"
                      cols="30"
                      spellcheck="false"
                    />
                  </div>
                </div>
              </div>
            </div>
            <div
              class="col-12 flex p-0 field mt-2 pt-2"
              v-if="
                !device_process.is_last && device_process.approved_type != 1
              "
            >
              <div class="w-10rem p-0 font-bold">File đính kèm:</div>
              <div class="p-0" style="width: calc(100% - 10rem)">
                <FileUpload
                  chooseLabel="Chọn File"
                  :showUploadButton="false"
                  :showCancelButton="false"
                  :multiple="true"
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
          </div>
        </TabPanel>
        <!-- <TabPanel>
          <template #header
            ><div class="flex align-items-center">
              <div class="pr-1 align-items-center format-center">
                <i class="pi pi-chart-bar"></i>
              </div>
              <div>
                <span>Quy trình xử lý</span>
              </div>
            </div>
          </template>
          Content II
        </TabPanel>
        <TabPanel>
          <template #header
            ><div class="flex align-items-center">
              <div class="pr-1 align-items-center format-center">
                <i class="pi pi-history"></i>
              </div>
              <div>
                <span>Lịch sử </span>
              </div>
            </div>
          </template>
          Content III
        </TabPanel> -->
      </TabView>
    </form>
    <template #footer>
      <Toolbar class="surface-0 pb-0 border-0">
        <template #end>
          <div class="flex">
            <Button
              @click="closeDetailsHandover"
              label="Đóng"
              icon="pi pi-times"
              autofocus
            />

            <div class="flex" v-if="device_process.is_return_created == true">
              <Button
                @click="onCloseReturnCreated"
                label="Trả lại người duyệt trước"
                icon="pi pi-undo"
                autofocus
                class="p-button-warning"
              />
            </div>

            <div class="flex" v-if="device_process.is_suggest_repair == true">
              <Button
                @click="onCloseReturnRepair"
                label="Trả người đề nghị sửa"
                icon="pi pi-reply"
                autofocus
                class="p-button-danger"
              />
            </div>
            <div
              class="flex"
              v-if="device_process.approved_type == 1 || device_process.is_last"
            >
              <!-- <Button
                @click="onShowConfigGroup(1)"
                label="Trình duyệt"
                icon="pi pi-directions"
                autofocus
                class="p-button-success opacity-80"
              /> -->
              <Button
                @click="onShowConfigGroup(2)"
                label="Chọn nhóm duyệt"
                icon="pi pi-desktop"
                autofocus
                class="p-button-success opacity-80"
              />
              <div
                class="flex"
                v-if="
                  (device_process.classify_fake.includes('17')  ) &&
                  device_process.is_last
                "
              >
                <Button
                  @click="onShowCompleted()"
                  label="Hoàn thành"
                  icon="pi pi-check"
                  autofocus
                  class="p-button-success"
                />
              </div>
            </div>
            <div v-else>
              <Button
                @click="onAcceptApprovedRepair(device_process)"
                label="Xác nhận duyệt"
                icon="pi pi-check"
                class="p-button-success"
                autofocus
              />
            </div>
          </div>
        </template>
      </Toolbar>
    </template>
  </Dialog>

  <Dialog
    header=" "
    class="appdock-1"
    v-model:visible="displayDetailsInventory"
    :maximizable="true"
    :style="{ width: '60vw' }"
    :dismissableMask="true"
    :modal="true"
  >
    <form>
      <TabView>
        <TabPanel class="mx-2">
          <template #header>
            <div class="flex align-items-center">
              <div class="pr-1 align-items-center format-center">
                <i class="pi pi-book"></i>
              </div>
              <div>
                <span>Thông tin phiếu</span>
              </div>
            </div>
          </template>
          <div class="grid formgrid m-2 surface-0">
            <div class="col-12 p-0 mb-5 field flex">
              <div class="col-12 p-0">
                <div
                  class="
                    format-center
                    col-12
                    field
                    mb-5
                    font-bold
                    text-xl
                    py-0
                    px-3
                  "
                >
                  Thông tin chi tiết
                </div>

                <div class="col-12 field flex">
                  <div class="col-6 p-0 flex">
                    <div class="w-10rem p-0 font-bold">Số phiếu:</div>
                    <div class="p-0" style="width: calc(100% - 10rem)">
                      {{ device_process.device_process_code }}
                    </div>
                  </div>
                  <div class="col-6 p-0 pl-5 flex align-items-center">
                    <div class="w-10rem p-0 font-bold">Loại phiếu:</div>
                    <div class="p-0" style="width: calc(100% - 10rem)">
                      {{
                        device_process.device_process_type == 1
                          ? "Sửa chữa"
                          : device_process.device_process_type == 2
                          ? "Kiểm kê"
                          : "Khác"
                      }}
                    </div>
                  </div>
                </div>
                <div class="col-12 field flex">
                  <div class="col-6 p-0 flex align-items-center">
                    <div class="w-10rem font-bold">Người gửi:</div>
                    <div style="max-width: calc(100% - 10rem)" class="p-0">
                      <div>
                        <div
                          v-tooltip.bottom="{
                            value:
                              device_process.department_name  ,
                            class: 'custom-error-tl1',
                          }"
                          class="flex w-full surface-0 align-items-center pr-2"
                          style="border-radius: 16px"
                        >
                          <Avatar
                            v-bind:label="
                              device_process.avatar
                                ? ''
                                : device_process.full_name.substring(
                                    device_process.full_name.lastIndexOf(' ') +
                                      1,
                                    device_process.full_name.lastIndexOf(' ') +
                                      2
                                  )
                            "
                            :image="basedomainURL + device_process.avatar"
                            size="small"
                            :style="
                              device_process.avatar
                                ? 'background-color: #2196f3'
                                : 'background:' +
                                  bgColor[device_process.full_name.length % 7]
                            "
                            shape="circle"
                            @error="
                              $event.target.src =
                                basedomainURL + '/Portals/Image/nouser1.png'
                            "
                          />
                          <div class="px-2">{{ device_process.full_name }}</div>
                        </div>
                      </div>
                    </div>
                  </div>

                  <div class="col-6 p-0 pl-5 flex">
                    <div class="w-10rem font-bold">Ngày gửi:</div>
                    <div class="p-0" style="width: calc(100% - 10rem)">
                      {{
                        moment(new Date(device_process.date_send)).format(
                          "DD/MM/YYYY"
                        )
                      }}
                    </div>
                  </div>
                </div>

                <!-- <div class="field col-12 flex">
                  <div class="w-10rem font-bold">Nội dung:</div>
                  <div class="p-0" style="width: calc(100% - 10rem)">
                    {{ device_process.content }}
                  </div>
                </div> -->
                <div
                  v-if="listUserFollows.length > 0"
                  class="field col-12 flex align-items-center"
                >
                  <div class="w-10rem font-bold">Người theo dõi:</div>
                  <div class="p-0 flex" style="max-width: calc(100% - 10rem)">
                    <div
                      v-for="(item, index) in listUserFollows"
                      :key="index"
                      class="flex"
                    >
                      <div>
                        <div
                          v-tooltip.bottom="{
                            value:
                              item.department_name  ,
                            class: 'custom-error-tl1',
                          }"
                          class="flex w-full surface-0 align-items-center pr-2"
                          style="border-radius: 16px"
                        >
                          <Avatar
                            v-if="item.full_name"
                            v-bind:label="
                              item.avatar
                                ? ''
                                : item.full_name.substring(
                                    item.full_name.lastIndexOf(' ') + 1,
                                    item.full_name.lastIndexOf(' ') + 2
                                  )
                            "
                            :image="basedomainURL + item.avatar"
                            size="small"
                            :style="
                              item.avatar
                                ? 'background-color: #2196f3'
                                : 'background:' +
                                  bgColor[item.full_name.length % 7]
                            "
                            shape="circle"
                            @error="
                              $event.target.src =
                                basedomainURL + '/Portals/Image/nouser1.png'
                            "
                          />
                          <div class="px-2">{{ item.full_name }}</div>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
            <div class="col-12 field p-0 text-lg font-bold format-center">
              Thông tin phiếu kiểm kê
            </div>
            <div class="col-12 field flex p-0">
              <div class="col-6 flex text-center align-items-center">
                <div class="w-10rem p-0 font-bold text-left">Người lập:</div>
                <div class="p-0 flex text-left" style="calc(100% -10rem)">
                  <div
                    v-if="device_inventory"
                    class="flex surface-0 align-items-center pr-2"
                    style="border-radius: 16px"
                  >
                    <Avatar
                      v-bind:label="
                        device_inventory.avatar
                          ? ''
                          : device_inventory.created_name.substring(
                              device_inventory.created_name.lastIndexOf(' ') +
                                1,
                              device_inventory.created_name.lastIndexOf(' ') + 2
                            )
                      "
                      :image="basedomainURL + device_inventory.avatar"
                      size="small"
                      :style="
                        device_inventory.avatar
                          ? 'background-color: #2196f3'
                          : 'background:' +
                            bgColor[device_inventory.created_name.length % 7]
                      "
                      shape="circle"
                      @error="
                        $event.target.src =
                          basedomainURL + '/Portals/Image/nouser1.png'
                      "
                    />
                    <div class="px-2">{{ device_inventory.created_name }}</div>
                  </div>
                </div>
              </div>
              <div
                class="col-6 p-0 pl-5 text-left align-items-center"
                v-if="device_inventory"
              >
                <div class="col-12 field p-0 flex text-left align-items-center">
                  <div class="w-10rem font-bold">Ngày lập:</div>
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
            </div>

            <div class="col-12 flex p-0 field mb-7">
              <div
                class="col-6 flex text-left align-items-center"
                v-if="device_inventory"
              >
                <div class="w-10rem p-0 text-left font-bold">Ngày kiểm kê:</div>

                <div style="calc(100% - 10rem)">
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
              <div
                class="col-6 p-0 flex pl-5 text-left align-items-center"
                v-if="device_inventory"
              >
                <div class="w-10rem font-bold">Phòng ban/Kho:</div>
                <div style="width: calc(100% - 10rem)">
                  <InputText
                    :disabled="true"
                    v-model="device_inventory.device_fromname"
                    class="w-full class-disabled"
                  />
                </div>
              </div>
            </div>

            <div class="col-12 field p-0 text-lg font-bold format-center">
              <div class="text-lg font-bold">Danh sách người tham gia</div>
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
                    class="
                      align-items-center
                      justify-content-center
                      text-center
                    "
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
                    class="
                      align-items-center
                      justify-content-center
                      text-center
                    "
                    headerStyle="text-align:center;max-width:220px;height:50px"
                    bodyStyle="text-align:center;max-width:220px;overflow: hidden;"
                    field="position_name"
                    header="Chức vụ"
                  >
                  </Column>

                  <template #empty>
                    <div
                      class="
                        align-items-center
                        justify-c
                        p-0
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
            </div>
            <div class="col-12 pb-2 p-0 text-lg font-bold format-center">
              <div class="text-lg font-bold">Danh sách thiết bị</div>
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
                    class="
                      align-items-center
                      justify-content-center
                      text-center
                    "
                    headerStyle="text-align:center;width:50px;"
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
                    headerStyle="text-align:left;width:120px"
                    bodyStyle="text-align:left;width:120px;overflow: hidden; "
                    field="representative.device_number"
                    header="Số hiệu"
                  >
                    <template #body="slotProps">
                      <div
                        class="image-text format-center"
                        v-if="slotProps.data.representative"
                      >
                        {{ slotProps.data.representative.device_number }}
                      </div>
                    </template>
                  </Column>
                  <Column
                    headerStyle="text-align:left;width:150px"
                    bodyStyle="text-align:left;overflow: hidden;width:150px"
                    field="representative.device_number"
                    header="Tên thiết bị"
                  >
                    <template #body="slotProps">
                      <span
                        class="image-text"
                        v-if="slotProps.data.representative"
                        >{{ slotProps.data.representative.device_name }}</span
                      >
                    </template>
                  </Column>
                  <Column
                    headerStyle="text-align:left;width:120px"
                    bodyStyle="text-align:left;width:120px;overflow: hidden;"
                    field="representative.device_number"
                    header="Sử dụng"
                  >
                    <template #body="slotProps">
                      <div class="w-full">
                        <span
                          class="image-text"
                          v-if="slotProps.data.representative"
                          >{{ slotProps.data.representative.full_name }}</span
                        >
                      </div>
                    </template>
                  </Column>
                  <Column
                    headerStyle="text-align:left;width:70px;"
                    bodyStyle="text-align:left;width:70px;overflow: hidden;"
                    field="representative.device_number"
                    header="SL trước"
                  >
                    <template #body="slotProps">
                      <div
                        class="image-text format-center"
                        v-if="slotProps.data.representative"
                      >
                        <Button
                          class="p-button-rounded"
                          :label="slotProps.data.representative.amount_before"
                        ></Button>
                      </div>
                    </template>
                  </Column>
                  <Column
                    headerStyle="text-align:left;width:70px;"
                    bodyStyle="text-align:left;width:70px;overflow: hidden;"
                    field="representative.device_number"
                    header="SL sau"
                  >
                    <template #body="slotProps">
                      <div
                        class="image-text format-center"
                        v-if="slotProps.data.representative"
                      >
                        <Button
                          class="p-button-rounded"
                          :label="slotProps.data.representative.amount_after"
                        ></Button>
                      </div>
                    </template>
                  </Column>
                  <Column
                    headerStyle="text-align:left;width:150px;"
                    bodyStyle="text-align:left;width:150px;overflow: hidden;"
                    field="representative.device_number"
                    header="Tình trạng"
                  >
                    <template #body="slotProps">
                      <span
                        class="image-text"
                        v-if="slotProps.data.representative"
                        >{{ slotProps.data.representative.condition }}</span
                      >
                    </template>
                  </Column>
                  <Column
                    headerStyle="text-align:left; min-width:120px"
                    bodyStyle="text-align:left; overflow: hidden; min-width:120px"
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
                          style="min-width: 25px"
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
                    headerStyle="text-align:left;width:70px;height:50px"
                    bodyStyle="text-align:left;width:70px;overflow: hidden;"
                    field="representative.amount"
                    header="Số lượng"
                  >
                    <template #body="slotProps">
                      <div
                        class="w-full format-center"
                        v-if="slotProps.data.is_approved == false"
                      >
                        <Button
                          v-tooltip.top="'Chưa đánh giá'"
                          icon="pi pi-times"
                          class="p-button-rounded p-button-danger"
                        />
                      </div>
                      <div v-else class="format-center">
                        <Button
                          class="p-button-rounded"
                          :label="
                            slotProps.data.amount ? slotProps.data.amount : '0'
                          "
                        ></Button>
                      </div>
                    </template>
                  </Column>
                  <Column
                    headerStyle="text-align:left;height:50px; min-width:120px"
                    bodyStyle="text-align:left;overflow: hidden; min-width:120px"
                    field="representative.reviews"
                    header="Đánh giá"
                  >
                    <template #body="slotProps">
                      <div
                        class="w-full format-center"
                        v-if="slotProps.data.is_approved == false"
                      >
                        <Button
                          v-tooltip.top="'Chưa đánh giá'"
                          icon="pi pi-times"
                          class="p-button-rounded p-button-danger"
                        />
                      </div>
                      <div class="w-full" v-else>
                        {{ slotProps.data.reviews }}
                      </div>
                    </template>
                  </Column>

                  <template #empty>
                    <div
                      class="
                        align-items-center
                        justify-c
                        p-0
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
            </div>

            <div
              class="col-12 flex p-0 field mt-2 pt-2"
              v-if="listFilesS.length > 0"
            >
              <div class="w-10rem p-0 font-bold">File đính kèm</div>
            </div>
            <div class="col-12 p-0">
              <div
                style="width: 100%"
                class="p-0 px-8 field"
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
                            class="w-full no-underline cursor-pointer"
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
                              style="
                                width: 70px;
                                height: 50px;
                                object-fit: contain;
                              "
                              :alt="item.file_name"
                            />
                          </a>
                        </div>
                        <a
                          :href="basedomainURL + item.file_path"
                          download
                          class="w-full no-underline cursor-pointer text-900"
                        >
                          <span
                            class="ml-2 cursor-pointer"
                            style="line-height: 50px"
                          >
                            {{ item.file_name }}</span
                          >
                        </a>
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

            <div class="col-12 p-0" v-if="!device_process.is_last">
              <div class="format-center col-12 field font-bold text-xl px-3">
                Thông tin phiếu gửi
              </div>
              <div class="field col-12 p-0 flex">
                <div class="flex w-full p-0 pt-2 align-items-center">
                  <div class="font-bold w-10rem">Nội dung:</div>
                  <div style="width: calc(100% - 10rem)" class="">
                    <Textarea
                      class="w-full"
                      v-model="device_process.content"
                      :autoResize="true"
                      rows="2"
                      cols="30"
                      spellcheck="false"
                    />
                  </div>
                </div>
              </div>
            </div>
            <div
              class="col-12 flex p-0 field mt-2 pt-2"
              v-if="!device_process.is_last"
            >
              <div class="w-10rem p-0 font-bold">File đính kèm:</div>
              <div class="p-0" style="width: calc(100% - 10rem)">
                <FileUpload
                  chooseLabel="Chọn File"
                  :showUploadButton="false"
                  :showCancelButton="false"
                  :multiple="true"
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
          </div>
        </TabPanel>
      </TabView>
    </form>
    <template #footer>
      <Toolbar class="surface-0 pb-0 border-0">
        <template #end>
          <div class="flex">
            <Button
              @click="closeDetailsInventory"
              label="Đóng"
              icon="pi pi-times"
              autofocus
            />
            <div class="flex" v-if="device_process.is_return_created == true">
              <Button
                @click="onCloseReturnCreated"
                label="Trả lại người duyệt trước"
                icon="pi pi-undo"
                autofocus
                class="p-button-warning"
              />
            </div>

            <div class="flex" v-if="device_process.is_suggest_repair == true">
              <Button
                @click="onCloseReturnRepair"
                label="Trả người đề nghị sửa"
                icon="pi pi-reply"
                autofocus
                class="p-button-danger"
              />
            </div>
            <div
              class="flex"
              v-if="device_process.approved_type == 1 || device_process.is_last"
            >
              <!-- <Button
                @click="onShowConfigGroup(1)"
                label="Trình duyệt"
                icon="pi pi-directions"
                autofocus
                class="p-button-success opacity-80"
              /> -->
              <Button
                @click="onShowConfigGroup(2)"
                label="Chọn nhóm duyệt"
                icon="pi pi-desktop"
                autofocus
                class="p-button-success opacity-80"
              />
              <div
                class="flex"
                v-if="
                  (device_process.classify_fake.includes('17')  ) &&
                  device_process.is_last
                "
              >
                <Button
                  @click="onShowCompleted()"
                  label="Hoàn thành"
                  icon="pi pi-check"
                  autofocus
                  class="p-button-success"
                />
              </div>
            </div>
            <div v-else>
              <Button
                @click="onAcceptApprovedRepair(device_process)"
                label="Xác nhận duyệt"
                icon="pi pi-check"
                class="p-button-success"
                autofocus
              />
            </div>
          </div>
        </template>
      </Toolbar>
    </template>
  </Dialog>
  <Dialog
    header="Chi tiết phiếu thu hồi"
    v-model:visible="displayDetailsRecall"
    :maximizable="true"
    :style="{ width: '60vw' }"
    :dismissableMask="true"
    :modal="true"
  >
    <form>
      <div v-if="displayDetailsRecall">
        <detailsRecall :device_recall_id="device_recall_id" />
      </div>
    </form>
    <template #footer>
      <Toolbar class="surface-0 pb-0 border-0">
        <template #end>
          <div class="flex">
            <Button
              @click="closeDetailsHandover"
              label="Đóng"
              icon="pi pi-times"
              autofocus
            />

            <div class="flex" v-if="device_process.is_return_created == true">
              <Button
                @click="onCloseReturnCreated"
                label="Trả lại người duyệt trước"
                icon="pi pi-undo"
                autofocus
                class="p-button-warning"
              />
            </div>

            <div class="flex" v-if="device_process.is_suggest_repair == true">
              <Button
                @click="onCloseReturnRepair"
                label="Trả người đề nghị sửa"
                icon="pi pi-reply"
                autofocus
                class="p-button-danger"
              />
            </div>
            <div
              class="flex"
              v-if="device_process.approved_type == 1 || device_process.is_last"
            >
              <!-- <Button
                @click="onShowConfigGroup(1)"
                label="Trình duyệt"
                icon="pi pi-directions"
                autofocus
                class="p-button-success opacity-80"
              /> -->
              <Button
                @click="onShowConfigGroup(2)"
                label="Chọn nhóm duyệt"
                icon="pi pi-desktop"
                autofocus
                class="p-button-success opacity-80"
              />
              <div
                class="flex"
                v-if="
                  (device_process.classify_fake.includes('17')  ) &&
                  device_process.is_last
                "
              >
                <Button
                  @click="onShowCompleted()"
                  label="Hoàn thành"
                  icon="pi pi-check"
                  autofocus
                  class="p-button-success"
                />
              </div>
            </div>
            <div v-else>
              <Button
                @click="onAcceptApprovedRepair(device_process)"
                label="Xác nhận duyệt"
                icon="pi pi-check"
                class="p-button-success"
                autofocus
              />
            </div>
          </div>
        </template>
      </Toolbar>
    </template>
  </Dialog>
  <Dialog
    header="Chi tiết thiết bị"
    v-model:visible="displayDetailsDevice"
    :maximizable="true"
    :style="{ width: '80vw' }"   :modal="true"
  >
    <div v-if="displayDetailsDevice && dataPropsD">
      <detailsDevice :device="dataPropsD" style="min-height: 80vh" />
    </div>
  </Dialog>
  <Dialog
    header="Trình duyệt"
    v-model:visible="displayDeviceRepair"
    :maximizable="true"   :modal="true"
    :style="{ width: '35vw' }"
  >
    <div v-if="displayDeviceRepair">
      <configAprroved
        :display="displayDeviceRepair"
        :type="
          device_process.device_process_type == 1
            ? 'TS_PhieuSuaChua'
            : device_process.device_process_type == 2
            ? 'TS_PhieuKiemKe'
            : 'TS_PhieuThuHoi'
        "
        :isdefault="checkDefault"
        :approved_group_id="deviceAprrovedId"
        :device_process_code="device_process.device_process_code"
        :device_note_id="device_process.device_note_id"
        :device_process_id="device_process.device_process_id"
        :checkApp="2"
        :device_process_type="device_process.device_process_type"
        :listAssetsH="listAssetsH"
      ></configAprroved>
    </div>
  </Dialog>

  <Dialog
    header="Nội dung trả về"
    v-model:visible="isShowReturn"
    :maximizable="true"
    :style="{ width: '35vw' }"   :modal="true"
  >
    <div class="grid p-0 pr-2 mx-2">
      <div class="col-12 p-0 py-2 field flex align-items-center">
        <div class="col-2 p-0 font-bold">Nội dung</div>
        <div class="col-10 p-0">
          <Textarea
            class="w-full"
            v-model="device_process.content"
            rows="3"
            cols="30"
          />
        </div>
      </div>
      <div class="col-12 field p-0 flex align-items-center">
        <div class="col-2 p-0 font-bold">File đính kèm</div>
        <div class="col-10 p-0">
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
    </div>
    <template #footer>
      <Toolbar class="surface-0 pb-0 border-0">
        <template #end>
          <div class="flex">
            <Button
              @click="onReturnD(device_process)"
              label="Gửi"
              icon="pi pi-check"
              autofocus
            />
          </div>
        </template>
      </Toolbar>
    </template>
  </Dialog>
  <Dialog
    header="Hoàn thành"
    v-model:visible="isshowCompleted"
    :maximizable="true"
    :style="{ width: '35vw' }"   :modal="true"
  >
    <div class="grid p-0 pr-2 mx-2">
      <div class="col-12 p-0 py-2 field flex align-items-center">
        <div class="col-2 p-0 font-bold">Nội dung</div>
        <div class="col-10 p-0">
          <Textarea
            class="w-full"
            v-model="device_process.content"
            rows="3"
            cols="30"
          />
        </div>
      </div>
      <div class="col-12 field p-0 flex align-items-center">
        <div class="col-2 p-0 font-bold">File đính kèm</div>
        <div class="col-10 p-0">
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
    </div>
    <template #footer>
      <Toolbar class="surface-0 pb-0 border-0">
        <template #end>
          <div class="flex">
            <Button
              @click="onAcceptCompleted(device_process)"
              label="Gửi"
              icon="pi pi-check"
              autofocus
            />
          </div>
        </template>
      </Toolbar>
    </template>
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
      <deviceProcedure
        :dataProcess="processListViews"
        :devicelogs="listLogs"
      ></deviceProcedure>
    </div>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="closeDialogProcedure()"
        class="p-button-outlined"
      />
    </template>
  </Dialog>
</template>


<style scoped>
.custom-error-tl1 {
  min-width: 500px;
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

.d-avatar-device_process {
  position: relative;
  width: 100%;
  height: 350px;
}
.d-avatar-device_process img {
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
