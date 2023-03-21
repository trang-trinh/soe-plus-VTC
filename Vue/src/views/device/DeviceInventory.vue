<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import moment from "moment";


import treeuser from "../../components/user/treeuser.vue";

import configAprroved from "../../components/device/configAprroved.vue";
import deviceProcedure from "../../components/device/deviceProcedure.vue";
import detailsInventory from "../../components/device/detailsInventory.vue";
import printListInventory from "./print/printListInventory.vue";
import { encr, checkURL } from "../../util/function.js";
//Khai báo
const router = inject("router");
const cryoptojs = inject("cryptojs");
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const basedomainURL = baseURL;

const selectedHandOver = ref();
const emitter = inject("emitter");
const selectedCard = ref([]);
const checkDelList = ref(false);
const displayAssets = ref(false);
const isFirstCard = ref(false);
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
const rules = {
  inventory_number: {
    required,
    $errors: [
      {
        $property: "inventory_number",
        $validator: "required",
        $message: "Số phiếu không được để trống!",
      },
    ],
  },
};
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
const showListAssets = () => {
  filterSQLDM.value = [];
  checkShowAssets.value = true;
  loadDeviceType();
  loadDataSQLDM();
  displayAssets.value = true;
};

const displayDialogUser = ref(false);

const headerDialogUser = ref("Chọn người tham gia");

const showTreeUser = () => {
  checkMultile.value = false;

  selectedUser.value = [];
  displayDialogUser.value = true;
};
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

//Xóa tin tức

const delCard = (Card) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá kiểm kê này không!",
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
            baseURL + "/api/device_inventory_slip/delete_device_inventory_slip",
            {
              headers: { Authorization: `Bearer ${store.getters.token}` },
              data: Card != null ? [Card.inventory_slip_id] : 1,
            }
          )
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá phiếu kiểm kê thành công!");
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
    .post(baseURL + "/api/SQL/Filter_device_inventory_slip", data, config)
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

const changeDeviceList = (event) => {
  event.items.forEach((element) => {
    selectedCard.value.push(element);
  });
};
const changeDeviceAllList = (event) => {
  event.items.forEach((element) => {
    selectedCard.value = selectedCard.value.filter(
      (x) => x.card_id != element.card_id
    );
  });
};
const filterSQLDM = ref([]);
const loadDataSQLDM = () => {
  datalistsDM.value = [];

  if (
    device_inventory.value.warehouse_id != null ||
    device_inventory.value.department_id_fake != null
  ) {
    if (device_inventory.value.warehouse_id != null) {
      filterSQLDM.value = [];

      let filterS = {
        filterconstraints: [],
        filteroperator: "or",
        key: "warehouse_id",
      };
      let filterS1 = {
        filterconstraints: [],
        filteroperator: "or",
        key: "warehouse_id_old",
      };
      device_inventory.value.warehouse_id.forEach((element) => {
        filterS.filterconstraints.push({
          value: element,
          matchMode: "equals",
        });
        filterS1.filterconstraints.push({
          value: element,
          matchMode: "equals",
        });
      });
      filterSQLDM.value.push(filterS1);
      filterSQLDM.value.push(filterS);
    }
    if (device_inventory.value.department_id_fake != null) {
      filterSQLDM.value = [];
      let filterS = {
        filterconstraints: [],
        filteroperator: "or",
        key: "manage_department_id",
      };

      for (const key in device_inventory.value.department_id_fake) {
        if (Number(key))
          filterS.filterconstraints.push({
            value: key,
            matchMode: "equals",
          });
      }

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

  // let filterS = {
  //   filterconstraints: [{ value: "DSC", matchMode: "notEquals" }],
  //   filteroperator: "and",
  //   key: "status",
  // };
  // filterSQLDM.value.push(filterS);
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
  // options.value.loading = true;
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
  filterTrangthai.value = null;
  filterCardUserSend.value = null;
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
            ? "Không được xóa phiếu kiểm kê có trạng thái: Chờ đánh giá"
            : element.status == 2
            ? "Không được xóa phiếu kiểm kê có trạng thái:  Đã đánh giá"
            : "Không được xóa phiếu kiểm kê có trạng thái: Trả lại",
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
        text: "Bạn có muốn xóa danh sách kiểm kê này không!",
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
            listId.push(item.inventory_slip_id);
          });
          axios
            .delete(
              baseURL +
                "/api/device_inventory_slip/delete_device_inventory_slip",
              {
                headers: { Authorization: `Bearer ${store.getters.token}` },
                data: listId != null ? listId : 1,
              }
            )
            .then((response) => {
              swal.close();
              if (response.data.err != "1") {
                swal.close();
                toast.success("Xoá kiểm kê thành công!");
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
const listSignature = ref([]);
const listSig = () => {
  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "device_process_get_signature",
            par: [
              {
                par: "device_note_id",
                va: device_inventory.value.inventory_slip_id,
              },
              {
                par: "device_process_code",
                va: device_inventory.value.inventory_number,
              },
              { par: "device_process_type", va: 2 },
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

      listSignature.value = data;
      var htmltable = "";
      htmltable = renderhtml("formprint", htmltable);
      var printframe = window.frames["printframe"];
      printframe.document.write(htmltable);
      setTimeout(function () {
        printframe.print();
        printframe.document.close();
      }, 0);
    });
};
const print = () => {
  listUserA.value = [];

  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "device_accept_inventory_get",
            par: [{ par: "inventory_slip_id", va: device_inventory_id.value }],
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
      let data3 = JSON.parse(response.data.data)[3];

      device_inventory.value = data[0];

      data1.forEach((element, i) => {
        element.is_order = i + 1;
        if (element.representative) {
          element.representative = JSON.parse(element.representative)[0];
        }
      });

      data1.forEach((element) => {
        if (element.reviews == null) element.reviews = "";
        if (element.amount == null) element.amount = "";
      });
      selectedCard.value = data1;
      listFilesS.value = data2;
      data3.forEach((element, i) => {
        element.is_order = i + 1;
      });
      listUserA.value = data3;
      listSig();
    })
   
};

function renderhtml(id, htmltable) {
  var htmltable = "";
  htmltable = "<font face='Times New Roman'>";
   

  htmltable +=
    "   <table  border='0' width='1024' cellpadding='0'> <thead> <tr> ";
  htmltable +=
    "<td   colspan='2' style='width: 40%; vertical-align: bottom ;text-align:center' >";
  htmltable +=
    "    <div   >BỘ QUỐC PHÒNG</div>";
  htmltable +=
    "    <div  > <b>BẢO HIỂM XÃ HỘI</b> <div   style='text-align:center;border-top: 1.5px solid #000; margin: 5px 125px;margin-bottom: 0px '></div></div></td>";
  htmltable +=
    "   <td  colspan='4' style='min-width: 40%; vertical-align: bottom;text-align:center' >";
  htmltable +=
    "  <div > <b>CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM</b> </div>";
  htmltable += "    <div><b>Độc lập - Tự do - Hạnh phúc</b></div>";
  htmltable +=
    "     <div   style='text-align:center;border-top: 1.5px solid #000; margin: 5px 200px;margin-bottom: 0px '  ></div>  </td>  </tr>";
  htmltable +=
    "<tr> <td   colspan='2'>  <div style='text-align:center;padding: 1rem 0'>Số: " +
    device_inventory.value.inventory_number +
    "</div>  </td> <td class='text-center' colspan='4'>";
  htmltable +=
    " <div style='text-align:center;padding:   0'> <i>Hà Nội, ngày " +
    new Date(device_inventory.value.created_date).getDate() +
    ", tháng " +
    (new Date(device_inventory.value.created_date).getMonth() + 1) +
    ", năm " +
    new Date(device_inventory.value.created_date).getFullYear() +
    "</i>    </div>   </td>  </tr> </thead>  </table>";

  htmltable += "<table border='0' width='1024' cellpadding='10'><tbody>";
  htmltable +=
    "<tr><td align='center' style='font-size:25px;padding-top:30px;padding-bottom:25px;font-weight:bold;'>PHIẾU KIỂM KÊ</td></tr>";
  htmltable +=
    "<tr><td align='left' style='font-size:16px;'><a style='font-weight:bold;float:left;width:120px;'>Người lập phiếu: </a><a style='width:calc(100% - 120px);'>" +
    device_inventory.value.created_name +
    "</a></td></tr>";
  htmltable +=
    "<tr><td align='left' style='font-size:16px;'><a style='font-weight:bold;float:left;width:120px;'>Loại kiểm kê: </a><a style='width:calc(100% - 120px);'>" +
    (device_inventory.value.is_type != null
      ? "Kiểm kê kho"
      : "Kiểm kê phòng ban") +
    "</a></td></tr>";

  htmltable +=
    "<tr><td align='left' style='font-size:16px;'><a style='font-weight:bold;float:left;width:120px;'>Phòng ban: </a><a style='width:calc(100% - 120px);'>" +
    (device_inventory.value.deviceFrom != null
      ? device_inventory.value.deviceFrom
      : "") +
    "</a></td></tr>";

  htmltable +=
    "<tr><td align='left' style='font-size:16px;'><a style='font-weight:bold;float:left;width:120px;'>Ngày kiểm kê: </a><a style='width:calc(100% - 120px);'>" +
    moment(device_inventory.value.inventory_date).format("DD/MM/YYYY") +
    "</a></td></tr>";
  htmltable +=
    "<tr><td align='left' style='font-size:16px;'><a style='font-weight:bold;'>Danh sách người tham gia kiểm kê: </a></th></tr></tbody></table>";

  htmltable +=
    "<table width='1024' cellpadding='8' style='padding:0px 15px 15px;border-spacing:0;'><thead>";
  htmltable +=
    "<tr><th width='50' align='center' style='font-size:16px;border:1px solid #000;'>STT</th>";
  htmltable +=
    "<th align='left' style='font-size:16px;border:1px solid #000;border-left:none;'>Họ tên</th>";
  htmltable +=
    "<th width='300' align='left' style='font-size:16px;border:1px solid #000;border-left:none;'>Phòng ban</th>";
  htmltable +=
    "<th width='300' align='left' style='font-size:16px;border:1px solid #000;border-left:none;'>Chức vụ</th></tr></thead>";
  htmltable += "<tbody>";
  var stt = 0;
  listUserA.value.forEach(function (u) {
    stt += 1;
    htmltable +=
      "<tr><td width='50' align='center' style='font-size:16px;border:1px solid #000;border-top:none;'>" +
      stt +
      "</td>";
    htmltable +=
      "<td align='left' style='font-size:16px;border:1px solid #000;border-left:none;border-top:none;'>" +
      u.full_name +
      "</td>";
    htmltable +=
      "<td width='300' align='left' style='font-size:16px;border:1px solid #000;border-left:none;border-top:none;'>" +
      u.organization_name +
      "</td>";
    htmltable +=
      "<td width='300' align='left' style='font-size:16px;border:1px solid #000;border-left:none;border-top:none;'>" +
      u.position_name +
      "</td></tr>";
  });
  htmltable += "</tbody></table>";
  htmltable +=
    "<table width='1024' cellpadding='0' style='padding:0px 15px 15px;'><tbody>";
  htmltable +=
    "<tr><td style='font-size:16px;font-weight:bold;'>Danh sách tài sản kiểm kê: </td></tr></tbody></table>";
  htmltable +=
    "<table width='1024' cellpadding='8' style='padding:0px 15px 15px;border-spacing:0;'><thead>";
  htmltable +=
    "<tr><th width='30' align='center' style='font-size:16px;border:1px solid #000;'>STT</th>";
  htmltable +=
    "<th width='100' align='left' style='font-size:16px;border:1px solid #000;border-left:none;'>Số hiệu</th>";
  htmltable +=
    "<th width='120' align='left' style='font-size:16px;border:1px solid #000;min-width:120px;border-left:none;'>Tên tài sản</th>";
  htmltable +=
    "<th width='150' align='left' style='font-size:16px;border:1px solid #000;border-left:none;'>Người sử dụng</th>";
  htmltable +=
    "<th width='70' align='center' style='font-size:16px;border:1px solid #000;border-left:none;'>SL trước</th>";
  htmltable +=
    "<th width='70' align='center' style='font-size:16px;border:1px solid #000;border-left:none;'>SL sau</th>";
  htmltable +=
    "<th width='100' align='center' style='font-size:16px;border:1px solid #000;border-left:none;'>Tình trạng</th>";
  htmltable +=
    "<th width='150' align='center' style='font-size:16px;border:1px solid #000;border-left:none;'>Người đánh giá</th>";
  htmltable +=
    "<th width='50' align='center' style='font-size:16px;border:1px solid #000;border-left:none;'>SL</th>";
  htmltable +=
    "<th width='100' align='center' style='font-size:16px;border:1px solid #000;border-left:none;'>Đánh giá</th>";

  htmltable += "</tr></thead>";
  htmltable += "<tbody>";
  var sttTS = 0;
  selectedCard.value.forEach(function (ts) {
    sttTS++;
    htmltable +=
      "<tr><td width='50' align='center' style='font-size:16px;border:1px solid #000;border-top:none;'>" +
      sttTS +
      "</td>";
    htmltable +=
      "<td width='100' align='left' style='font-size:16px;border:1px solid #000;border-left:none;border-top:none;'>" +
      ts.representative.device_number +
      "</td>";
    htmltable +=
      "<td width='120' align='left' style='font-size:16px;border:1px solid #000;min-width:120px;border-left:none;border-top:none;'>" +
      ts.representative.device_name +
      "</td>";
    htmltable +=
      "<td width='150' align='left' style='font-size:16px;border:1px solid #000;border-left:none;border-top:none;'>" +
      ts.representative.full_name +
      "</td>";
    htmltable +=
      "<td width='70' align='center' style='font-size:16px;border:1px solid #000;border-left:none;border-top:none;'>" +
      ts.representative.amount_before +
      "</td>";
    htmltable +=
      "<td width='70' align='center' style='font-size:16px;border:1px solid #000;border-left:none;border-top:none;'>" +
      ts.representative.amount_after +
      "</td>";
    htmltable +=
      "<td width='100' align='left' style='font-size:16px;border:1px solid #000;border-left:none;border-top:none;'>" +
      ts.representative.condition +
      "</td>";
    htmltable +=
      "<td width='150' align='left' style='font-size:16px;border:1px solid #000;border-left:none;border-top:none;'>" +
      ts.full_name +
      "</td>";
    htmltable +=
      "<td width='50' align='center' style='font-size:16px;border:1px solid #000;border-left:none;border-top:none;'>" +
      ts.amount +
      "</td>";
    htmltable +=
      "<td width='100' align='left' style='font-size:16px;border:1px solid #000;border-left:none;border-top:none;'>" +
      ts.reviews +
      "</td>";
    htmltable += "</tr>";
  });
  htmltable += "</tbody></table>";
  htmltable +=
    "<table border='0' width='1024' cellpadding='0' style='padding:0px 15px;'><tbody>";
  htmltable +=
    "<tr><td colspan='2' align='left' style='font-size:16px;font-weight:bold;'>Xác nhận quy trình xử lý:</td></tr>";

  var isStaRow = true;
  listSignature.value.forEach(function (log) {
    htmltable += isStaRow ? "<tr>" : "";
    htmltable +=
      "<td style='text-align:center;padding-top:10px;padding-bottom:25px;'>";
    // htmltable +=
    //   "<p style='font-size:16px;'>" + log.approved_group_name + "</p>";
    htmltable +=
      "<p style='font-size:16px;font-weight:bold;text-transform:uppercase;" +
      (log.signature != "" ? "" : "padding-bottom:75px;") +
      "'>" +
      log.approved_group_name +
      "</p>";
    if (log.signature != "") {
      htmltable +=
        "<img src='" +
        basedomainURL +
        log.signature +
        "' style='height:60px;width:auto;' />";
    }
    htmltable += "<p style='font-size:16px;'>" + log.full_name + "</p></td>";
    htmltable += isStaRow ? "" : "</tr>";
    isStaRow = !isStaRow;
  });

  htmltable += "</tbody></table></font>";
  return htmltable;
}
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
const v$ = useVuelidate(rules, device_inventory);
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
  filterSQLDM.value = [];
  loadDataSQLDM();
};
const listDepartment = ref([]);
const listDepartmentD = ref([]);
const initTudien = () => {
  listDepartmentD.value = [];
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
        data.forEach((element) => {
          listDepartmentD.value = [
            {
              name: element.organization_name,
              code: element.organization_id,
            },
          ];
          // if (element.organization_id == store.getters.user.department_id) {
          //   listDepartmentD.value = [
          //     {
          //       name: element.organization_name,
          //       code: element.organization_id,
          //     },
          //   ];
          // }
        });

        var arr = [...data];
        let obj = renderTreeDV1(
          arr,
          "organization_id",
          "organization_name",
          "đơn vị",
          store.getters.user.organization_id
        );
        listDepartment.value = obj.arrtreeChils;
      }
    })
    
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
        name: "DANH_SACH_KIEM_KE",
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
        excelname: "DANH SÁCH PHIẾU KIỂM KÊ",
        proc: "device_inventory_list_export",
        par: [
          { par: "user_id", va: store.state.user.user_id },
          { par: "search", va: options.value.search },
          { par: "status", va: filterTrangthai.value },

          { par: "created_by", va: filterCardUserSend.value },
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

const displayDetails = ref(false);
const listAssetsH = ref();
const liItemsAccept = ref([
  {
    label: "Chuyển đánh giá",
    icon: "pi pi-angle-double-right",
    command: () => {
      sendAcceptInventory(deviceRepairS.value);
    },
  },
  {
    label: "Trình duyệt",
    icon: "pi pi-directions",
    command: () => {
      onShowConfigGroup(1);
    },
  },
  {
    label: "Chọn nhóm duyệt",
    icon: "pi pi-desktop",
    command: () => {
      onShowConfigGroup(2);
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
        if (element.aprroved_user != null) {
          element.aprroved_user = JSON.parse(element.aprroved_user);
          element.aprroved_user.forEach((eles) => {
            eles.is_shows = false;
            element.listprocess.push(eles);
          });
        }

        element.listprocess = compareProcess(element.listprocess);

        element.aprroved_user = null;
        element.device_process = null;
      });
      processListViews.value.push(data);
      processListViews.value.push(data1);
      loadLogs(deviceDataDetails.value.inventory_slip_id, 2);
      displayProcedure.value = true;
      // loadDTliView(data[0].approved_group_id)
    })
    
};
const listLogs = ref([]);
const loadLogs = (device_note_id, type) => {
  listLogs.value = [];
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
                va: device_note_id,
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

      listLogs.value = data;
    })
    .catch(() => {});
};
function compareProcess(data) {
  // for (let index = 0; index < data.length - 1; index++) {
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
  //       if (data[i].approved_type != 1) {
  //         data[i].is_approved = "2";
  //       }
  //     }
  //   }
  // }
  data.forEach((element, index) => {
    if (element.is_approved != null) {
      for (let i = 0; i < index; i++) {
        data[i].is_approved = "2";
      }
    }
  });
  return data;
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
const onShowConfigGroup = (type) => {
  if (type == 1) {
    axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
          proc: "device_approved_group_get",
          par: [
            { par: "approved_group_id", va: null },
            { par: "module", va: "TS_PhieuKiemKe" },
            { par: "default", va: true },
            { par: "user_id", va: store.getters.user.user_id },
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
          deviceAprrovedId.value = data[0].approved_group_id;
          checkDefault.value = true;
          displayDeviceRepair.value = true;
        }
      })

    
  } else {
    checkDefault.value = false;
    displayDeviceRepair.value = true;
  }
};
const device_inventory_id = ref();
const openDetailsHandover = (data) => {
  device_inventory_id.value = data.inventory_slip_id;
  displayDetailsHandover.value = true;
};

const displayDeviceRepair = ref(false);

const closeDetailsHandover = () => {
  displayDetailsHandover.value = false;
  device_inventory.value = null;
};
const displayDetailsHandover = ref(false);
let fileSize = [];
const onUploadFile = (event) => {
  fileSize = [];
  files.value = [];
  event.files.forEach((element) => {
    files.value.push(element);
    fileSize.push(element.size);
  });
};
const removeFile = (event) => {
  files.value = files.value.filter((a) => a != event.file);
};
const deleteFileH = (value) => {
  listFilesS.value = listFilesS.value.filter(
    (x) => x.inventory_files_id != value.inventory_files_id
  );
};
const deleteFileD = (value) => {
  selectedCard.value = selectedCard.value.filter(
    (x) => x.card_id != value.card_id
  );
};

const deleteFileU = (value) => {
  listUserA.value = listUserA.value.filter((x) => x.user_id != value.user_id);
  listUserA.value.forEach((element, i) => {
    element.is_order = i + 1;
  });
};
const loadCount = () => {
  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "device_inventory_count",
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
  if (!device_inventory.value.inventory_created_date) {
    return;
  }

  if (!selectedCard.value.length > 0) {
    swal.fire({
      title: "Thông báo!",
      text: "Vui lòng chọn thiết bị kiểm kê trước khi tạo phiếu!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  if (!listUserA.value.length > 0) {
    swal.fire({
      title: "Thông báo!",
      text: "Vui lòng chọn người kiểm tra trước khi tạo phiếu!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  let formData = new FormData();
  for (var i = 0; i < files.value.length; i++) {
    let file = files.value[i];
    formData.append("image", file);
  }
  device_inventory.value.deviceFrom = "";

  if (device_inventory.value.department_id_fake) {
    var detach = "";
    device_inventory.value.department_id = "";
    var arrCheck = [];

    selectedCard.value.forEach((element) => {
      if (!arrCheck.includes(element.manage_department_id)) {
        device_inventory.value.deviceFrom +=
          detach + element.manage_department_name;
        device_inventory.value.department_id +=
          detach + element.manage_department_id;
        detach = ", ";
        arrCheck.push(element.manage_department_id);
      }
    });
  }
  if (device_inventory.value.warehouse_id) {
    var detach1 = "";
    var arrCheck1 = [];
    selectedCard.value.forEach((element) => {
      if (!arrCheck1.includes(element.warehouse_id)) {
        if (element.warehouse_name != null)
          device_inventory.value.deviceFrom += detach1 + element.warehouse_name;
        else
          device_inventory.value.deviceFrom +=
            detach1 + element.manage_department_name;
        detach1 = ", ";
        device_inventory.value.is_type = 1;
        arrCheck1.push(element.warehouse_id);
      }
    });
    device_inventory.value.warehouse_id =
      device_inventory.value.warehouse_id.toString();
  }

  formData.append("inventory", JSON.stringify(device_inventory.value));
  formData.append("inventorydetails", JSON.stringify(selectedCard.value));
  formData.append("inventorypersonnel", JSON.stringify(listUserA.value));
  formData.append("inventoryfiles", JSON.stringify(listFilesS.value));
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
        baseURL + "/api/device_inventory_slip/add_device_inventory_slip",
        formData,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Thêm phiếu kiểm kê thành công!");

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
        baseURL + "/api/device_inventory_slip/update_device_inventory_slip",
        formData,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa phiếu kiểm kê thành công!");

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
const menSendAccept = ref();
const deviceAprrovedId = ref();
const checkDefault = ref(false);
const deviceRepairS = ref();
const deviceDataDetails = ref();
//Sửa bản ghi
const menuDetailsR = ref();
const onShowDetails = (value) => {
  deviceDataDetails.value = value;
  menuDetailsR.value.toggle(event);
};
const sendAccept = (value) => {
  deviceRepairS.value = value;
  menSendAccept.value.toggle(event);
};
const sendAcceptInventory = (valoe) => {
  let data = {
    IntID: valoe.inventory_slip_id,
    TextID: valoe.inventory_slip_id + "",
    IntTrangthai: 1,
    BitTrangthai: null,
  };

  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn chuyển đánh giá không!",
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
          .put(
            baseURL +
              "/api/device_inventory_slip/update_s_device_inventory_slip",
            data,
            config
          )
          .then((response) => {
            if (response.data.err != "1") {
              swal.close();
              toast.success("Chuyển đánh giá thành công!");

              displayBasic.value = false;
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
            swal.fire({
              title: "Error!",
              text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
              icon: "error",
              confirmButtonText: "OK",
            });
          });
      }
    });
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

// const menuAccept = ref();
// const dataAccept = ref();
// const toggleAccept = (event, value) => {
//   menuAccept.value.toggle(event);
//   dataAccept.value = value;
// };

// const itemsAccept = ref([
//   {
//     label: "Chuye",
//     icon: "pi pi-pencil",
//     command: () => {
//       editCard(dataCog.value);
//     },
//   },
//   {
//     label: "Xóa",
//     icon: "pi pi-trash",
//     command: () => {
//       delCard(dataCog.value);
//     },
//   },
// ]);
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
            proc: "device_inventory_get",
            par: [{ par: "inventory_slip_id", va: data.inventory_slip_id }],
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
      let data3 = JSON.parse(response.data.data)[3];
      device_inventory.value = data[0];
      if (device_inventory.value.warehouse_id) {
        var src = [];
        device_inventory.value.warehouse_id =
          device_inventory.value.warehouse_id.split(",");
        device_inventory.value.warehouse_id.forEach((element) => {
          src.push(Number(element));
        });
        device_inventory.value.warehouse_id = src;
      }
      device_inventory.value.department_id_fake = {};

      if (device_inventory.value.department_id) {
        var src = [];

        device_inventory.value.department_id.split(",").forEach((element) => {
          src.push(Number(element));
        });

        src.forEach((element) => {
          device_inventory.value.department_id_fake[element] = {
            checked: true,
            partialChecked: false,
          };
        });
      } else {
        device_inventory.value.department_id_fake = null;
      }
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
      });
      selectedCard.value = data1;

      listFilesS.value = data2;
      data3.forEach((element, i) => {
        element.is_order = i + 1;
      });
      listUserA.value = data3;
      checkShowAssets.value = false;
      headerDialog.value = "Sửa phiếu kiểm kê";
      isSaveCard.value = true;
      displayBasic.value = true;
    })
    
};
const checkImg = (src) => {
  let allowedExtensions = /(\.jpg|\.jpeg|\.png|\.gif)$/i;
  return allowedExtensions.exec(src);
};
//Hiển thị dialog

const headerDialog = ref();
const displayBasic = ref(false);
const listSCard = ref([
  { name: "Đã tạo", code: 0 },
  { name: "Chờ đánh giá", code: 1 },
  { name: "Đã xác nhận", code: 2 },
  { name: "Chờ duyệt", code: 3 },
  { name: "Hoàn thành", code: 4 },
  { name: "Trả lại", code: 5 },
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
              { par: "type", va: 4 },
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
        device_inventory.value.inventory_number =
          data[0].current_number +
          "/" +
          data[0].year +
          "/" +
          data[0].text_symbols;
      }
    })
   
};
const openBasic = (str) => {
  selectedCard.value = [];
  listFilesS.value = [];
  listUserA.value = [];
  device_inventory.value = {
    status: 0,
    is_order: sttCard.value ? sttCard.value : 1,
    inventory_created_date: new Date(),
    inventory_date: new Date(),
    inventory_type: 1,
    department_id_fake: {},
    department_id: store.getters.user.department_id,
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
              { par: "code_number", va: "TS_PhieuKiemKe" },
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
        
  if (listDepartment.value.length > 0) {
    device_inventory.value.department_id_fake[
      store.getters.user.department_id
    ] = { checked: true, partialChecked: false };
  } else {
    device_inventory.value.department_id_fake = null;
  }

  files.value = [];
  submitted.value = false;
  headerDialog.value = str;
  isSaveCard.value = false;
  checkShowAssets.value = false;
  displayBasic.value = true;
      } else{
        
        swal.fire({
          title: "Error!",
          text: "Vui lòng cấu hình số hiệu cho phiếu kiểm kê!",
          icon: "error",
          confirmButtonText: "OK",
        });
         
      }
      
    })
   
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
            proc: "device_inventory_list",
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
      filterTrangthai.value != null ||
      filterCardUserSend.value != null
    )
  )
    checkFilter.value = false;
};

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
const first = ref(0);
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
const onChangeWarehouse = () => {
  if (device_inventory.value.warehouse_id != null) {
    device_inventory.value.department_id_fake = null;
    selectedCard.value = [];
  }
};
const onChangeDepartment = () => {
  if (device_inventory.value.department_id_fake != null) {
    selectedCard.value = [];
    device_inventory.value.warehouse_id = null;
  }
};
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
            proc: "device_warehouse_list_stocker",
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
 
    });
};

const datalistsDM = ref();
onMounted(() => {
  if (!checkURL(window.location.pathname, store.getters.listModule)) {
     //router.back();
  }
  loadWareHouse();
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
              <i class="pi pi-book"></i> Danh sách phiếu kiểm kê ({{
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
                v-if="checkDelList"
                @click="deleteList()"
                label="Xóa"
                icon="pi pi-trash"
                class="mr-2 p-button-danger"
              />
              <Button
                @click="openBasic('Thêm phiếu kiểm kê')"
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
          headerStyle="text-align:center;height:50px;max-width:180px"
          bodyStyle="text-align:center;max-width:180px"
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
          headerStyle="text-align:center;max-width:150px;height:50px"
          bodyStyle="text-align:center;max-width:150px"
          field="inventory_created_date"
          header="Ngày lập"
          filterField="inventory_created_date"
          dataType="date"
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
        >    <template #body="data">
            <div>
              {{ data.data.deviceFrom  ? data.data.deviceFrom  : "" }}
            </div>
          </template>
        </Column>
        <Column
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;height:50px;max-width:150px"
          bodyStyle="text-align:center;max-width:150px"
          field="inventory_type"
          header="Loại phiếu"
        >
          <template #body="data">
            <div>
              {{ data.data.is_type == 1 ? "Kiểm kê kho" : "Kiểm kê phòng ban" }}
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
                v-else-if="data.data.status == 2"
                label="Đã đánh giá"
                class="
                  w-full
                  bg-blue-300
                  justify-content-center
                  p-button-status-d
                "
              />
              <Chip
                v-else-if="data.data.status == 3"
                label="Chờ duyệt"
                class="
                  w-full
                  bg-pink-300
                  justify-content-center
                  p-button-status-d
                "
              />
              <Chip
                v-else-if="data.data.status == 4"
                label="Hoàn thành"
                class="
                  w-full
                  bg-green-300
                  justify-content-center
                  p-button-status-d
                "
              />
              <Chip
                v-else-if="data.data.status == 5"
                label="Trả lại"
                style="background-color: red; color: white"
                class="w-full justify-content-center p-button-status-d"
              />
              <Chip
                v-else
                label="Đã tạo"
                class="
                  wfull
                  surface-200
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
            <Button
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              @click="onShowDetails(data.data)"
              type="button"
              icon="pi pi-info-circle"
              aria-haspopup="true"
              aria-controls="overlay_menu_details"
              v-tooltip.left="'Chi tiết'"
            ></Button>

            <Menu
              id="overlay_menu_details"
              ref="menuDetailsR"
              :model="liItemsDetails"
              :popup="true"
            />

            <div
              v-if="
                (store.getters.user.is_admin ||  store.state.user.is_super == true ||
                  store.state.user.user_id == data.data.created_by ||
                  (store.state.user.role_id == 'admin' &&
                    store.state.user.organization_id ==
                      data.data.organization_id)) &&
                data.data.status != 3 &&
                data.data.status != 4
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
              <Button
                type="button"
                v-if="data.data.status == 0"
                icon="pi pi-angle-double-right"
                class="
                  p-button-rounded p-button-secondary p-button-outlined
                  mx-1
                "
                v-tooltip.left="'Chuyển đánh giá'"
                @click="sendAcceptInventory(data.data)"
                aria-haspopup="true"
                aria-controls="overlay_menu_accept"
              />

              <!-- @click="sendAcceptInventory(data.data)" -->
            </div>
            <Button
              type="button"
              icon="pi pi-arrow-circle-up"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              v-if="
                (store.state.user.is_super == true ||
                  store.state.user.user_id == data.data.created_by) &&
                (data.data.status == 2 || data.data.status == 5)
              "
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
      <h3>Danh sách thiết bị kiểm kê kèm theo</h3>
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
    @hide="closeDialog"
    :header="headerDialog"
    v-model:visible="displayBasic"
    :maximizable="true"
    :style="{ width: '70vw' }"
    :modal="true"
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="col-12 field p-0 text-lg font-bold">Thông tin phiếu</div>
        <div class="col-12 field flex p-0">
          <div class="col-6 flex p-0 align-items-center">
            <div class="w-10rem">
              Số phiếu:<span class="redsao pl-1"> (*)</span>
            </div>
            <div style="width: calc(100% - 10rem)">
              <InputText
                v-model="device_inventory.inventory_number"
                class="w-full"
                :class="{
                  'p-invalid': v$.inventory_number.$invalid && submitted,
                }"
              />
            </div>
          </div>
          <div class="col-6 flex p-0 text-center align-items-center">
            <div class="col-4 p-0 pl-5 text-left">Người lập:</div>
            <div class="col-8 p-0 flex text-left font-bold">
              <div
                class="flex surface-0 align-items-center pr-2"
                style="border-radius: 16px"
              >
                <Avatar
                  v-bind:label="
                    store.getters.user.avatar
                      ? ''
                      : store.getters.user.full_name.substring(
                          store.getters.user.full_name.lastIndexOf(' ') + 1,
                          store.getters.user.full_name.lastIndexOf(' ') + 2
                        )
                  "
                  :image="basedomainURL + store.getters.user.avatar"
                  size="small"
                  :style="
                    store.getters.user.avatar
                      ? 'background-color: #2196f3'
                      : 'background:' +
                        bgColor[store.getters.user.full_name.length % 7]
                  "
                  shape="circle"
                  @error="
                    $event.target.src =
                      basedomainURL + '/Portals/Image/nouser1.png'
                  "
                />
                <div class="px-2">{{ store.getters.user.full_name }}</div>
              </div>
            </div>
          </div>
        </div>
        <div
          v-if="
            (v$.inventory_number.$invalid && submitted) ||
            v$.inventory_number.$pending.$response
          "
          class="col-12 field p-0 flex"
        >
          <div class="w-10rem"></div>
          <small style="width: calc(100% - 10rem)">
            <span style="color: red" class="w-full">{{
              v$.inventory_number.required.$message
                .replace("Value", "Số phiếu")
                .replace("is required", "không được để trống!")
            }}</span>
          </small>
        </div>
        <div class="col-12 flex p-0">
          <div class="col-6 p-0 text-left align-items-center">
            <div class="col-12 field p-0 flex text-left align-items-center">
              <div class="w-10rem">Ngày lập:</div>
              <div style="width: calc(100% - 10rem)">
                <Calendar
                  placeholder="dd/mm/yyyy"
                  class="w-full"
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
              <div class="col-4 p-0 pl-5 text-left">
                Ngày kiểm kê:
                <span class="redsao pl-1">(*)</span>
              </div>
              <Calendar
                placeholder="dd/mm/yyyy"
                class="w-full"
                id="basic_use_date"
                v-model="device_inventory.inventory_created_date"
                autocomplete="on"
                :min-date="device_inventory.inventory_date"
                :showIcon="true"
              />
            </div>
            <div
              v-if="!device_inventory.inventory_created_date && submitted"
              class="col-12 field p-0 flex"
            >
              <div class="col-4 p-0"></div>
              <small class="col-8 p-0">
                <span style="color: red" class="w-full">{{
                  v$.inventory_number.required.$message
                    .replace("Value", "Ngày kiểm kê")
                    .replace("is required", "không được để trống!")
                }}</span>
              </small>
            </div>
          </div>
        </div>
        <div class="col-12 flex p-0">
          <div class="col-6 p-0 text-left align-items-center">
            <div class="col-12 field p-0 flex text-left align-items-center">
              <div class="w-10rem">Phòng ban:</div>
              <div style="width: calc(100% - 10rem)">
                <TreeSelect
                  v-model="device_inventory.department_id_fake"
                  :options="listDepartment"
                  :showClear="true"
                  :max-height="200"
                  placeholder="---------- Đơn vị ----------"
                  optionLabel="data.organization_name"
                  optionValue="data.department_id"
                  panelClass="d-design-dropdown"
                  class="sel-placeholder w-full"
                  selectionMode="checkbox"
                  @change="onChangeDepartment"
                >
                </TreeSelect>
                <!-- <Dropdown
                  v-model="device_inventory.department_id"
                  v-else
                  :options="listDepartmentD"
                  optionLabel="name"
                  placeholder="---------- Đơn vị ----------"
                  optionValue="code"
                  class="sel-placeholder w-full"
                  @change="onChangeDepartment1"
                /> -->
              </div>
            </div>
          </div>
          <div class="col-6 p-0 text-left align-items-center">
            <div class="col-12 field p-0 flex text-left align-items-center">
              <div class="col-4 p-0 pl-5 text-left">Kho:</div>

              <MultiSelect
                v-model="device_inventory.warehouse_id"
                :options="listWarehouse"
                optionLabel="name"
                optionValue="code"
                :filter="true"
                placeholder="----Chọn kho----"
                panelClass="d-design-dropdown"
                @change="onChangeWarehouse"
                class="col-8 p-0 d-tree-input"
                :style="
                  device_inventory.warehouse_id != null
                    ? 'border:2px solid #2196f3'
                    : ''
                "
              >
              </MultiSelect>
              <!-- <Dropdown
                v-model="device_inventory.warehouse_id"
                :options="listWarehouse"
                optionLabel="name"
                optionValue="code"
                :filter="true"
                class="sel-placeholder w-full"
                panelClass="d-design-dropdown"
                placeholder="----Chọn kho----"
                @change="onChangeWarehouse"
              /> -->
            </div>
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

        <div class="field col-12 md:col-12 flex format-center pt-2">
          <div class="col-6 p-0 pb-2">
            <Button
              @click="showListAssets"
              label="Chọn thiết bị"
              icon="pi pi-tags"
            />
          </div>
        </div>
        <div class="col-12 p-0 field">
          <!-- {{selectedCard}} -->
          <div class="w-full">
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
              :value="selectedCard"
              :paginator="false"
              :totalRecords="selectedCard.length"
              :row-hover="true"
            >
              <Column
                class="align-items-center justify-content-center text-center"
                headerStyle="text-align:center;max-width:50px;height:50px"
                bodyStyle="text-align:center;max-width:50px;overflow: hidden;"
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
                headerStyle="text-align:left;max-width:100px;height:50px"
                bodyStyle="text-align:left;max-width:100px;overflow: hidden;"
                field="device_number"
                header="Số hiệu"
                headerClass="format-center"
              >
                <template #body="data">
                  <div>
                    {{ data.data.device_number }}
                  </div>
                </template>
              </Column>

              <Column
                headerStyle="text-align:left;height:50px"
                bodyStyle="text-align:left; overflow: hidden;"
                field="device_name"
                header="Tên thiết bị"
                headerClass="format-center"
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
                field="device_user_name"
                header="Người sử dụng"
                ><template #body="data">
                  <div  >
                    {{ data.data.device_user_name }}
                  </div>
                </template>
              </Column>
              <Column
                class="align-items-center justify-content-center text-center"
                headerStyle="text-align:center;max-width:80px;height:50px"
                bodyStyle="text-align:center;max-width:80px;overflow: hidden;"
                field="amount_before"
                header="SL trước"
              >
              </Column>
              <Column
                class="align-items-center justify-content-center text-center"
                headerStyle="text-align:center;max-width:70px;height:50px"
                bodyStyle="text-align:center;max-width:70px;overflow: hidden;"
                header="SL sau"
              >
                <template #body="data">
                  <div>
                    <InputNumber
                      v-model="data.data.amount_after"
                      class="w-full p-0 h-full"
                    ></InputNumber>
                  </div>
                </template>
              </Column>
              <Column
                class="align-items-center justify-content-center text-center"
                headerStyle="text-align:center;height:50px"
                bodyStyle="text-align:center;overflow: hidden;"
                header="Tình trạng"
              >
                <template #body="data">
                  <div class="w-full">
                    <Textarea
                      rows="2"
                      cols="30"
                      v-model="data.data.condition"
                      class="w-full h-full"
                    ></Textarea>
                  </div>
                </template>
              </Column>
              <Column
                class="align-items-center justify-content-center text-center"
                headerStyle="text-align:center;max-width:50px;height:50px"
                bodyStyle="text-align:center;max-width:50px"
                header=" "
              >
                <template #body="data">
                  <Button
                    icon="pi pi-trash"
                    class="p-button-rounded p-button-danger"
                    @click="deleteFileD(data.data)"
                  />
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

        <div class="col-12 field p-0 text-lg font-bold">
          <Toolbar class="p-0 surface-0 border-none">
            <template #start>
              <div class="text-lg font-bold">Danh sách người tham gia</div>
            </template>
            <template #end> </template>
          </Toolbar>
        </div>
        <div class="field col-12 md:col-12 flex format-center pt-2">
          <div class="col-6 p-0 pb-2">
            <Button
              @click="showTreeUser()"
              label="Chọn người tham gia"
              icon="pi pi-users"
            />
          </div>
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
                headerStyle="text-align:left;max-width:220px;height:50px"
                bodyStyle="text-align:left;max-width:220px; overflow: hidden;"
                field="organization_name"
                header="Phòng ban"
                headerClass="format-center"
                class="align-items-center justify-content-center text-center"
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

              <Column
                class="align-items-center justify-content-center text-center"
                headerStyle="text-align:center;max-width:50px;height:50px"
                bodyStyle="text-align:center;max-width:50px"
                header=" "
              >
                <template #body="data">
                  <Button
                    icon="pi pi-trash"
                    class="p-button-rounded p-button-danger"
                    @click="deleteFileU(data.data)"
                  />
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
                <p class="p-0 m-0 text-500">Kéo thả hoặc chọn File đính kèm.</p>
              </template>
            </FileUpload>
          </div>
        </div>
        <div class="col-12 p-0">
          <div
            class="p-0 flex field w-full"
            v-for="(item, index) in listFilesS"
            :key="index"
          >
            <div class="w-10rem p-0"></div>
            <div
              class="
                p-0
                border-3 border-solid border-round-3xl border-blue-200
                surface-50
              "
              style="width: calc(100% - 10rem); border-radius: 10px"
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
          @click="saveHandover(!v$.$invalid)"
          label="Lưu"
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
    :modal="true"
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
                  <div
                    v-if="slotProps.item.warehouse_name"
                    class="w-full"
                    v-tooltip.top="'Nhà kho'"
                  >
                    <i class="pi pi-home product-category-icon"></i>
                    {{ slotProps.item.warehouse_name }}
                  </div>
                  <div class="w-full" v-else>
                    <i class="pi pi-home product-category-icon"></i>
                    {{ slotProps.item.manage_department_name }}
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
    :modal="true"
  >
    <form v-if="displayDetailsHandover">
      <detailsInventory :device_inventory_id="device_inventory_id" />
    </form>
    <template #footer>
      <Button
        @click="closeDetailsHandover"
        label="Đóng"
        icon="pi pi-times"
        autofocus
      /><Button @click="print()" label="In phiếu" icon="pi pi-print" />
    </template>
  </Dialog>

  <Dialog
    header="Trình duyệt"
    v-model:visible="displayDeviceRepair"
    :maximizable="true"
    :style="{ width: '35vw' }"
    :modal="true"
  >
    <div v-if="displayDeviceRepair">
      <configAprroved
        :display="displayDeviceRepair"
        :type="'TS_PhieuKiemKe'"
        :isdefault="checkDefault"
        :approved_group_id="deviceAprrovedId"
        :device_process_code="deviceRepairS.inventory_number"
        :device_note_id="deviceRepairS.inventory_slip_id"
        :checkApp="1"
        :device_process_id="null"
        :device_process_type="2"
        :listAssetsH="null"
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
  <treeuser
    v-if="displayDialogUser === true"
    :headerDialog="headerDialogUser"
    :displayDialog="displayDialogUser"
    :closeDialog="closeDialogUser"
    :one="checkMultile"
    :selected="selectedUser"
    :choiceUser="choiceUser"
  />
  <printListInventory :datas="datalists" />
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
    