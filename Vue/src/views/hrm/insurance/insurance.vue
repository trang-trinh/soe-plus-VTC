<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import diloginsurance from "../insurance/component/diloginsurance.vue";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import { encr, checkURL } from "../../../util/function.js";
import moment from "moment";

const router = inject("router");
const cryoptojs = inject("cryptojs");
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const isDynamicSQL = ref(false);
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const filters = ref({
  global: { value: null, matchMode: FilterMatchMode.CONTAINS },
  insurance_id: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
});
//khai bao bien
const statuss = ref([
  { value: 1, text: "Trả" },
  { value: 2, text: "Sửa" },
  { value: 3, text: "Chốt" },
  { value: 4, text: "Xin cấp" },
  { value: 5, text: "Gộp" },
  { value: 6, text: "Người lao động giữ sổ" },
]);
const hinhthucs = ref([
  { value: 1, text: "Báo tăng" },
  { value: 2, text: "Báo giảm" },
]);
const insurance_pays = ref();
const insurance_resolves = ref();
const dictionarys = ref();
//Function
const componentKey = ref(0);
const forceRerender = () => {
  componentKey.value += 1;
};
//Lấy số bản ghi
const loadCount = () => {
  axios
    .post(
      baseURL + "/api/insurance/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_insurance_count_1",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "date", va: options.value.date },
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
        sttStamp.value = data[0].totalRecords + 1;
      }
    })
    .catch((error) => {});
};
const loadTudien = () => {
  axios
    .post(
      baseURL + "api/insurance/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_insurance_dictionary",
            par: [{ par: "user_id", va: store.state.user.user_id }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      dictionarys.value = data;
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
//Lấy dữ liệu bank
const initData = (rf) => {
  if (rf) {
    if (isDynamicSQL.value) {
      loadDataSQL();
      return false;
    }
    if (rf) {
      if (options.value.PageNo == 0) {
        loadCount();
      }
    }
    axios
      .post(
        baseURL + "/api/insurance/GetDataProc",
        {
          str: encr(
            JSON.stringify({
              proc: "hrm_insurance_list_1",
              par: [
                { par: "pageno", va: options.value.PageNo },
                { par: "pagesize", va: options.value.PageSize },
                { par: "user_id", va: store.getters.user.user_id },
                { par: "status", va: null },
                { par: "search", va: options.value.searchStamp },
                { par: "date", va: options.value.date },
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
        if (isFirst.value) isFirst.value = false;
        data.forEach((element, i) => {
          element.STT = options.value.PageNo * options.value.PageSize + i + 1;
        });
        datalists.value = data;

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
  }
};
//Phân trang dữ liệu
const onPage = (event) => {
  if (event.rows != options.value.PageSize) {
    options.value.PageSize = event.rows;
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

    options.value.id = datalists.value[datalists.value.length - 1].bank_id;
    options.value.IsNext = true;
  } else if (event.page < options.value.PageNo) {
    //Trang trước
    options.value.id = datalists.value[0].bank_id;
    options.value.IsNext = false;
  }
  options.value.PageNo = event.page;
  initData(true);
};

const insurance = ref({
  status: null,
  organization_payment: null,
  insurance_province_id: null,
  hospital_name: null,
  organization_id: store.getters.user.organization_id,
  is_order: 1,
  profile_id: 1,
});
const selectedStamps = ref();
const submitted = ref(false);
const isAdd = ref(false);
const isView = ref(false);
const datalists = ref();
const toast = useToast();
const basedomainURL = baseURL;
const checkDelList = ref(false);

const options = ref({
  IsNext: true,
  sort: "created_date",
  SearchText: "",
  PageNo: 0,
  PageSize: 20,
  loading: true,
  totalRecords: null,
  date : null
});

//Hiển thị dialog
const headerDialog = ref();
const displayBasic = ref(false);
const openBasic = (str) => {
  forceRerender();
  submitted.value = false;
  insurance.value = {
    status: null,
    organization_payment: null,
    insurance_province_id: null,
    hospital_name: null,
    organization_id: store.getters.user.organization_id,
    profile_id: 1,
  };
  insurance_pays.value = [
    {
      start_date: null,
      payment_form: null,
      reason: null,
      end_date: null,
      organization_payment: null,
      total_payment: null,
      company_payment: null,
      member_payment: null,
    },
  ];
  insurance_resolves.value = [
    {
      type_mode: null,
      payment_form: null,
      type_mode: null,
      completed_date: null,
      received_money_date: null,
      money: null,
    },
  ];
  checkIsmain.value = false;
  isAdd.value = true;
  headerDialog.value = str;
  displayBasic.value = true;
};

const closeDialog = () => {
  displayBasic.value = false;
  initData(true);
  forceRerender();
};

//Thêm bản ghi

const sttStamp = ref(1);
const saveData = (isFormValid) => {
  submitted.value = true;
  if (!isFormValid) {
    return;
  }
  insurance_pays.value.forEach((item) => {
    item.is_duplicate = false;
  });
  if (insurance_pays.value.length >= 2) {
    let count_duplicate = 0;
    for (let i = 0; i < insurance_pays.value.length - 1; i++) {
      for (let j = i + 1; j < insurance_pays.value.length; j++) {
        if (
          !isEmpty(insurance_pays.value[i].start_date) &&
          !isEmpty(insurance_pays.value[j].start_date) &&
          isMonth(
            insurance_pays.value[i].start_date,
            insurance_pays.value[j].start_date
          )
        ) {
          insurance_pays.value[j].is_duplicate = true;
          insurance_pays.value[i].is_duplicate = true;
          count_duplicate++;
        }
      }
      if (count_duplicate > 0) {
        swal.fire({
          title: "Thông báo!",
          text: "Vui lòng nhập tháng đóng đóng bảo hiểm không được trùng nhau!",
          icon: "error",
          confirmButtonText: "OK",
        });
        return;
      }
    }
  }
  insurance_pays.value.forEach((item) => {
    if (!isEmpty(item.is_duplicate)) item.is_duplicate = null;
  });
  let formData = new FormData();

  formData.append("insurance", JSON.stringify(insurance.value));
  formData.append(
    "insurance_pay",
    JSON.stringify(
      insurance_pays.value.filter(
        (item) => !Object.values(item).every((o) => isEmpty(o))
      )
    )
  );
  formData.append(
    "insurance_resolve",
    JSON.stringify(
      insurance_resolves.value.filter(
        (item) => !Object.values(item).every((o) => isEmpty(o))
      )
    )
  );
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: isAdd.value == false ? "put" : "post",
    url:
      baseURL +
      `/api/insurance/${
        isAdd.value == false ? "update_insurance" : "add_insurance"
      }`,
    data: formData,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  }).then((response) => {
    if (response.data.err === "0") {
      swal.close();
      toast.success("Cập nhật thành công!");
      displayBasic.value = false;
      initData(true);
    } else {
      swal.fire({
        title: "Thông báo!",
        text: "Đã có mã sổ này trong hệ thống rồi!",
        icon: "error",
        confirmButtonText: "OK",
      });
    }
  });
};
const checkIsmain = ref(true);
//Sửa bản ghi
const editTem = (dataTem) => {
  submitted.value = false;
  axios
    .post(
      baseURL + "/api/Profile/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_insurance_get_1",
            par: [
              { par: "insurance_id", va: dataTem.insurance_id },
              { par: "date", va: options.value.date },
          ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      swal.close();
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        insurance.value = data[0][0];
        //get child
        if (data[1].length > 0) {
          insurance_pays.value = data[1];
          insurance_pays.value.forEach((item) => {
            if (item.start_date != null) {
              item.start_date = new Date(item.start_date);
            }
          });
        } else insurance_pays.value = [];

        if (data[2].length > 0) {
          insurance_resolves.value = data[2];
          insurance_resolves.value.forEach((item) => {
            if (item.received_file_date != null) {
              item.received_file_date = new Date(item.received_file_date);
            }
            if (item.completed_date != null) {
              item.completed_date = new Date(item.completed_date);
            }
            if (item.received_money_date != null) {
              item.received_money_date = new Date(item.received_money_date);
            }
          });
        } else insurance_resolves.value = [];
      }
      headerDialog.value = "Sửa thông tin";
      isAdd.value = false;
      displayBasic.value = true;
    })
    .catch((error) => {
      if (error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
      }
    });
};
//Xóa bản ghi
const delTem = (Tem) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá bản ghi này không!",
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
          .delete(baseURL + "/api/insurance/del_insurance", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: [Tem.insurance_id],
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá thẻ bảo hiểm thành công!");
              initData(true);
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
//Xuất excel

//Sort
const onSort = (event) => {
  options.value.PageNo = 0;

  if (event.sortField == null) {
    isDynamicSQL.value = false;
    initData(true);
  } else {
    options.value.sort =
      event.sortField + (event.sortOrder == 1 ? " ASC" : " DESC");
    if (event.sortField == "STT") {
      options.value.sort =
        "is_order" + (event.sortOrder == 1 ? " ASC" : " DESC");
    }
    isDynamicSQL.value = true;
    loadDataSQL();
  }
};
const checkFilter = ref(false);
const filterSQL = ref([]);
const isFirst = ref(true);
const loadDataSQL = () => {
  datalists.value = [];

  let data = {
    id: "bank_id",
    sqlS: filterTrangthai.value != null ? filterTrangthai.value : null,
    sqlO: options.value.sort,
    Search: options.value.SearchText,
    PageNo: options.value.PageNo,
    PageSize: options.value.PageSize,
    next: true,
    sqlF: null,
    fieldSQLS: filterSQL.value,
  };
  options.value.loading = true;
  axios
    .post(baseURL + "/api/hrm_ca_SQL/Filter_hrm_ca_bank", data, config)
    .then((response) => {
      let dt = JSON.parse(response.data.data);
      let data = dt[0];
      if (data.length > 0) {
        data.forEach((element, i) => {
          element.STT = options.value.PageNo * options.value.PageSize + i + 1;
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
          title: "Thông báo",
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
//Tìm kiếm
const searchStamp = (event) => {
  if (event.code == "Enter") {
    if (options.value.SearchText == "") {
      isDynamicSQL.value = false;
      options.value.loading = true;
      initData(true);
    } else {
      isDynamicSQL.value = true;
      options.value.loading = true;
      initData(true);
    }
  }
};
const refreshStamp = () => {
  options.value.searchStamp = null;
  options.value.date = null;
  monthPickerFilter.value = null;
  filterTrangthai.value = null;
  options.value.loading = true;
  selectedStamps.value = [];
  isDynamicSQL.value = false;
  filterSQL.value = [];
  initData(true);
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
  options.value.PageNo = 0;
  options.value.id = null;
  isDynamicSQL.value = true;
  loadDataSQL();
};
//Xóa nhiều
const deleteList = () => {
  let listId = new Array(selectedStamps.value.length);
  let checkD = false;
  selectedStamps.value.forEach((item) => {
    if (item.is_default) {
      toast.error("Không được xóa thẻ bảo hiểm mặc định!");
      checkD = true;
      return;
    }
  });
  if (!checkD) {
    swal
      .fire({
        title: "Thông báo",
        text: "Bạn có muốn xoá thẻ bảo hiểm này không!",
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

          selectedStamps.value.forEach((item) => {
            listId.push(item.insurance_id);
          });
          axios
            .delete(baseURL + "/api/insurance/del_insurance", {
              headers: { Authorization: `Bearer ${store.getters.token}` },
              data: listId != null ? listId : 1,
            })
            .then((response) => {
              swal.close();
              if (response.data.err != "1") {
                swal.close();
                toast.success("Xoá thẻ bảo hiểm thành công!");
                checkDelList.value = false;

                initData(true);
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

//Filter
const trangThai = ref([
  { name: "Hiển thị", code: 1 },
  { name: "Không hiển thị", code: 0 },
]);

const filterTrangthai = ref();

watch(selectedStamps, () => {
  if (selectedStamps.value.length > 0) {
    checkDelList.value = true;
  } else {
    checkDelList.value = false;
  }
});
const op = ref();
const toggle = (event) => {
  op.value.toggle(event);
};
const addRow = (type) => {
  //relative
  if (type == 1) {
    let obj = {
      start_date: null,
      payment_form: null,
      reason: null,
      end_date: null,
      organization_payment: null,
      total_payment: null,
      company_payment: null,
      member_payment: null,
    };
    insurance_pays.value.push(obj);
  }
  if (type == 2) {
    let obj = {
      type_mode: null,
      payment_form: null,
      type_mode: null,
      completed_date: null,
      received_money_date: null,
      money: null,
    };
    insurance_resolves.value.push(obj);
  }
};
const deleteRow = (idx, type) => {
  if (type == 1) {
    insurance_pays.value.splice(idx, 1);
  }
  if (type == 2) {
    insurance_resolves.value.splice(idx, 1);
  }
};
const goProfile = (item) => {
  router.push({
    name: "profileinfo",
    params: { id: generateUUID()},
    query: { id: item.profile_id },
  });
};
//filter date
const monthPickerFilter = ref();
const onFilterMonth = ()=>{
  options.value.date = new Date(monthPickerFilter.value.month +1 +"/01" +"/" +monthPickerFilter.value.year);
  initData(true);
}
const onCleanFilterMonth = () => {
  if (monthPickerFilter.value) monthPickerFilter.value = null;
  options.value.date  = null;
  initData(true);
};
//check empy object
function isEmpty(val) {
  return val === undefined || val == null || val.length <= 0 ? true : false;
}
function generateUUID() {
  // Public Domain/MIT
  var d = new Date().getTime(); //Timestamp
  var d2 =
    (typeof performance !== "undefined" &&
      performance.now &&
      performance.now() * 1000) ||
    0; //Time in microseconds since page-load or 0 if unsupported
  return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, function (c) {
    var r = Math.random() * 16; //random number between 0 and 16
    if (d > 0) {
      //Use timestamp until depleted
      r = (d + r) % 16 | 0;
      d = Math.floor(d / 16);
    } else {
      //Use microseconds since page-load if supported
      r = (d2 + r) % 16 | 0;
      d2 = Math.floor(d2 / 16);
    }
    return (c === "x" ? r : (r & 0x3) | 0x8).toString(16);
  });
}
function formatNumber(a, b, c, d) {
  var e = isNaN((b = Math.abs(b))) ? 2 : b;
  b = void 0 == c ? "," : c;
  d = void 0 == d ? "," : d;
  c = 0 > a ? "-" : "";
  var g = parseInt((a = Math.abs(+a || 0).toFixed(e))) + "",
    n = 3 < (n = g.length) ? n % 3 : 0;
  return (
    c +
    (n ? g.substr(0, n) + d : "") +
    g.substr(n).replace(/(\d{3})(?=\d)/g, "$1" + d) +
    (e
      ? b +
        Math.abs(a - g)
          .toFixed(e)
          .slice(2)
      : "")
  );
}
onMounted(() => {
  initData(true);
  loadTudien();
  return {
    datalists,
    options,
    onPage,
    initData,
    loadCount,
    openBasic,
    closeDialog,
    basedomainURL,

    saveData,
    isFirst,
    searchStamp,
    selectedStamps,
    deleteList,
  };
});
</script>
    <template>
  <div class="main-layout true flex-grow-1 p-2 pb-0 pr-0">
    <DataTable
      @page="onPage($event)"
      @sort="onSort($event)"
      @filter="onFilter($event)"
      v-model:filters="filters"
      filterDisplay="menu"
      filterMode="lenient"
      :filters="filters"
      :scrollable="true"
      scrollHeight="flex"
      :showGridlines="true"
      columnResizeMode="fit"
      :lazy="true"
      :totalRecords="options.totalRecords"
      :loading="options.loading"
      :reorderableColumns="true"
      :value="datalists"
      removableSort
      v-model:rows="options.PageSize"
      paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
      :rowsPerPageOptions="[20, 30, 50, 100, 200]"
      :paginator="true"
      dataKey="insurance_id"
      responsiveLayout="scroll"
      v-model:selection="selectedStamps"
      :row-hover="true"
    >
      <template #header>
        <h3 class="module-title mt-0 ml-1 mb-2">
          <i class="pi pi-building"></i> Danh sách thẻ bảo hiểm ({{
            options.totalRecords
          }})
        </h3>
        <Toolbar class="w-full custoolbar">
          <template #start>
            <span class="p-input-icon-left">
              <i class="pi pi-search" />
              <InputText
                v-model="options.SearchText"
                @keyup="initData()"
                type="text"
                spellcheck="false"
                placeholder="Tìm kiếm"
              />
              <!-- <Button
                type="button"
                class="ml-2"
                icon="pi pi-filter"
                @click="toggle"
                aria:haspopup="true"
                aria-controls="overlay_panel"
                v-tooltip="'Bộ lọc'"
                :class="
                  filterTrangthai != null && checkFilter
                    ? ''
                    : 'p-button-secondary p-button-outlined'
                "
              />
              <OverlayPanel
                ref="op"
                appendTo="body"
                class=""
                :showCloseIcon="false"
                id="overlay_panel"
                style="width: 300px"
              >
                <div class="grid formgrid m-0">
                  <div class="flex field col-12 p-0">
                    <div
                      class="col-4 text-left pt-2 p-0"
                      style="text-align: left"
                    >
                      Trạng thái
                    </div>
                    <div class="col-8">
                      <Dropdown
                        class="col-12"
                        v-model="filterTrangthai"
                        :options="trangThai"
                        optionLabel="name"
                        optionValue="code"
                        placeholder="Trạng thái"
                      />
                    </div>
                  </div>
                  <div class="flex col-12 p-0">
                    <Toolbar
                      class="border-none surface-0 outline-none pb-0 w-full"
                    >
                      <template #start>
                        <Button
                          @click="reFilterEmail"
                          class="p-button-outlined"
                          label="Xóa"
                        ></Button>
                      </template>
                      <template #end>
                        <Button @click="filterFileds" label="Lọc"></Button>
                      </template>
                    </Toolbar>
                  </div>
                </div>
              </OverlayPanel> -->
            </span>
          </template>

          <template #end>
            <Datepicker
              @closed="onFilterMonth(false)"
              class="mr-2"
              locale="vi"
              selectText="Thực hiện"
              cancelText="Hủy"
              placeholder=" Lọc theo tháng"
              v-model="monthPickerFilter"
              monthPicker
              ><template #clear-icon>
                <Button
                  @click="onCleanFilterMonth"
                  icon="pi pi-times"
                  class="p-button-rounded p-button-text"
                />
              </template>
              <template #input-icon>
                <Button icon="pi pi-calendar" class="p-button-text" />
              </template>
            </Datepicker>
            <Button
              v-if="checkDelList"
              @click="deleteList()"
              label="Xóa"
              icon="pi pi-trash"
              class="mr-2 p-button-danger"
            />
            <!-- <Button
               @click="openBasic('Cập nhật thẻ bảo hiểm')"
              label="Thêm mới"
              icon="pi pi-plus"
              class="mr-2"
            />  -->
            <Button
              @click="refreshStamp"
              class="mr-2 p-button-outlined p-button-secondary"
              icon="pi pi-refresh"
              v-tooltip="'Tải lại'"
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
        </Toolbar></template
      >

      <Column
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:70px;height:50px"
        bodyStyle="text-align:center;max-width:70px"
        selectionMode="multiple"
      >
      </Column>


      <!-- <Column
        field="profile_id"
        header="Mã nhân sự"
        headerStyle="text-align:center;max-width:100px;height:50px"
        bodyStyle="text-align:center;max-width:100px"
        class="align-items-center justify-content-center text-center"
      >
      </Column> -->

      <Column
        field="profile_user_name"
        header="Họ và tên"
        headerStyle="text-align:center;height:50px"
        bodyStyle="text-align:center"
        class="align-items-center justify-content-center text-center"
      >
      <template #body="slotProps">
            <b @click="goProfile(slotProps.data)" class="hover cursor-pointer">{{
              slotProps.data.profile_user_name
            }}</b>
          </template>
      </Column>
      <Column
        field="organization_name"
        header="Phòng ban"
        headerStyle="text-align:center;max-width:100px;height:50px"
        bodyStyle="text-align:center;max-width:100px;;max-height:60px"
        class="align-items-center justify-content-center text-center"
      >
      </Column>
      <Column
        field="organization_name"
        header="Chức vụ"
        headerStyle="text-align:center;max-width:150px;height:50px"
        bodyStyle="text-align:center;max-width:150px;;max-height:60px"
        class="align-items-center justify-content-center text-center"
      >
      </Column>
      <Column
        field="recruitment_date"
        header="Ngày vào"
        headerStyle="text-align:center;max-width:150px;height:50px"
        bodyStyle="text-align:center;max-width:150px;;max-height:60px"
        class="align-items-center justify-content-center text-center"
      >
      <template #body="{ data }">
          <span v-if="data.recruitment_date"> {{ moment(new Date(data.recruitment_date)).format("DD/MM/YYYY ") }}</span>
        </template>
      </Column>
      <Column
        field="insurance_id"
        header="Số sổ"
        headerStyle="text-align:center;max-width:100px;height:50px"
        bodyStyle="text-align:center;max-width:100px;;max-height:60px"
        class="align-items-center justify-content-center text-center"
      >
      </Column>
      <Column
        field="insurance_code"
        header="Số thẻ"
        headerStyle="text-align:center;max-width:100px;height:50px"
        bodyStyle="text-align:center;max-width:100px;;max-height:60px"
        class="align-items-center justify-content-center text-center"
      >
      </Column>
      <Column
        field="batdaudong"
        header="Bắt đầu đóng"
        headerStyle="text-align:center;max-width:150px;height:50px"
        bodyStyle="text-align:center;max-width:150px;;max-height:60px"
        class="align-items-center justify-content-center text-center"
      >
        <template #body="{ data }">
          <span v-if="data.batdaudong">{{ moment(new Date(data.batdaudong)).format("MM/YYYY ") }}</span>
        </template>
      </Column>
      <Column
        field="mucdong"
        header="Mức đóng"
        headerStyle="text-align:center;max-width:150px;height:50px;justify-content:center"
        bodyStyle="text-align:center;max-width:150px;max-height:60px;justify-content:end"
      >
        <template #body="{ data }">
          {{ formatNumber(data.mucdong, 0, ".", ".") }}
        </template>
      </Column>
      <Column
        field="congtydong"
        header="Công ty đóng"
        headerStyle="text-align:center;max-width:150px;height:50px;justify-content:center"
        bodyStyle="text-align:center;max-width:150px;;max-height:60px; justify-content:end"
      >
        <template #body="{ data }">
          {{ formatNumber(data.congtydong, 0, ".", ".") }}
        </template>
      </Column>
      <Column
        field="nhanviendong"
        header="Người lao động đóng"
        headerStyle="text-align:center;max-width:150px;height:50px;justify-content:center"
        bodyStyle="text-align:right;max-width:150px;max-height:60px;justify-content:end"
      >
        <template #body="{ data }">
          {{ formatNumber(data.nhanviendong, 0, ".", ".") }}
        </template>
      </Column>
      <Column
        header="Chức năng"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:150px;height:50px"
        bodyStyle="text-align:center;max-width:150px"
      >
        <template #body="Tem">
          <div>
            <Button
              @click="editTem(Tem.data)"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-pencil"
              v-tooltip.top="'Sửa'"
            ></Button>
            <Button
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-trash"
              @click="delTem(Tem.data)"
              v-tooltip.top="'Xóa'"
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
          <img src="../../../assets/background/nodata.png" height="144" />
          <h3 class="m-1">Không có dữ liệu</h3>
        </div>
      </template>
    </DataTable>
  </div>

  <diloginsurance
    :key="componentKey"
    :headerDialog="headerDialog"
    :displayDialog="displayBasic"
    :closeDialog="closeDialog"
    :isAdd="isAdd"
    :isView="isView"
    :model="insurance"
    :addRow="addRow"
    :deleteRow="deleteRow"
    :insurance_pays="insurance_pays"
    :insurance_resolves="insurance_resolves"
    :statuss="statuss"
    :hinhthucs="hinhthucs"
    :dictionarys="dictionarys"
    :initData="initData"
    :datefilter="options.date"
  />
</template>
    
    <style scoped>
    .hover:hover {
  color: #0078d4;
}
.ip33 {
  height: 33px !important;
}
.scroll-outer {
  visibility: hidden;
  margin: 0 1rem;
}
.scroll-inner,
.scroll-outer:hover,
.scroll-outer:focus {
  visibility: visible;
}
</style>