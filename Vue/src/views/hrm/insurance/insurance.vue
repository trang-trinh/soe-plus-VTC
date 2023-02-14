<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import { encr, checkURL } from "../../../util/function.js";
import moment from "moment";

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
const rules = {
  insurance: {
    required,
    $errors: [
      {
        $property: "bank_name",
        $validator: "required",
        $message: "Tên thẻ bảo hiểm không được để trống!",
      },
    ],
  },
};
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
  { value: 1, text: "Bao tăng" },
  { value: 2, text: "Báo giảm" },
]);
const insurance_pays = ref();
const insurance_resolves = ref();
const dictionarys = ref();

//Lấy số bản ghi
const loadCount = () => {
  axios
    .post(
      baseURL + "/api/Profile/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_insurance_count",
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
        sttStamp.value = data[0].totalRecords + 1;
      }
    })
    .catch((error) => {});
};
const loadTudien = () => {
  axios
    .post(
      baseURL + "api/Profile/GetDataProc",
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
      addLog({
        title: "Lỗi Console loadData",
        controller: "SQLView.vue",
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
//Lấy dữ liệu bank
const loadData = (rf) => {
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
        baseURL + "/api/Profile/GetDataProc",
        {
          str: encr(
            JSON.stringify({
              proc: "hrm_insurance_list",
              par: [
                { par: "pageno", va: options.value.PageNo },
                { par: "pagesize", va: options.value.PageSize },
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
  loadData(true);
};

const bank = ref({
  bank_name: "",
  emote_file: "",
  status: true,
  is_default: false,
  is_order: 1,
});
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
const v$ = useVuelidate(rules, insurance);
const isAdd = ref(false);
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
});

//Hiển thị dialog
const headerDialog = ref();
const displayBasic = ref(false);
const openBasic = (str) => {
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
  bank.value = {
    bank_name: "",
    emote_file: "",
    status: true,
    is_default: false,
    is_order: 1,
  };

  displayBasic.value = false;
  loadData(true);
};

//Thêm bản ghi

const sttStamp = ref(1);
const saveData = (isFormValid) => {
  //   submitted.value = true;
  //   if (!isFormValid) {
  //     return;
  //   }

  let formData = new FormData();

  formData.append("insurance", JSON.stringify(insurance.value));
  debugger;
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
      loadData(true);
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
            proc: "hrm_insurance_get",
            par: [{ par: "insurance_id", va: dataTem.insurance_id }],
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
//Xuất excel

//Sort
const onSort = (event) => {
  options.value.PageNo = 0;

  if (event.sortField == null) {
    isDynamicSQL.value = false;
    loadData(true);
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
      loadData(true);
    } else {
      isDynamicSQL.value = true;
      options.value.loading = true;
      loadData(true);
    }
  }
};
const refreshStamp = () => {
  options.value.SearchText = null;
  filterTrangthai.value = null;
  options.value.loading = true;
  selectedStamps.value = [];
  isDynamicSQL.value = false;
  filterSQL.value = [];
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
  options.value.PageNo = 0;
  options.value.id = null;
  isDynamicSQL.value = true;
  loadDataSQL();
};
//Checkbox
const onCheckBox = (value, check, checkIsmain) => {
  if (check) {
    let data = {
      IntID: value.bank_id,
      TextID: value.bank_id + "",
      IntTrangthai: 1,
      BitTrangthai: value.status,
    };
    axios
      .put(baseURL + "/api/hrm_ca_bank/update_s_hrm_ca_bank", data, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa trạng thái thẻ bảo hiểm thành công!");
          loadData(true);
          closeDialog();
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
    let data1 = {
      IntID: value.bank_id,
      TextID: value.bank_id + "",
      BitMain: value.is_default,
    };
    axios
      .put(baseURL + "/api/hrm_ca_bank/Update_DefaultStamp", data1, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa trạng thái thẻ bảo hiểm thành công!");
          loadData(true);
          closeDialog();
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
            listId.push(item.bank_id);
          });
          axios
            .delete(baseURL + "/api/hrm_ca_bank/delete_hrm_ca_bank", {
              headers: { Authorization: `Bearer ${store.getters.token}` },
              data: listId != null ? listId : 1,
            })
            .then((response) => {
              swal.close();
              if (response.data.err != "1") {
                swal.close();
                toast.success("Xoá thẻ bảo hiểm thành công!");
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

//Filter
const trangThai = ref([
  { name: "Hiển thị", code: 1 },
  { name: "Không hiển thị", code: 0 },
]);

const filterTrangthai = ref();

const reFilterEmail = () => {
  filterTrangthai.value = null;
  isDynamicSQL.value = false;
  checkFilter.value = false;
  filterSQL.value = [];
  options.value.SearchText = null;
  op.value.hide();
  loadData(true);
};
const filterFileds = () => {
  filterSQL.value = [];
  checkFilter.value = true;
  let filterS = {
    filterconstraints: [{ value: filterTrangthai.value, matchMode: "equals" }],
    filteroperator: "and",
    key: "status",
  };
  filterSQL.value.push(filterS);
  loadDataSQL();
};
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
const addRow_Item = (type) => {
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
const delRow_Item = (item, type) => {
  if (type == 1) {
    insurance_pays.value.splice(insurance_pays.value.lastIndexOf(item), 1);
  }
  if (type == 2) {
    insurance_resolves.value.splice(
      insurance_resolves.value.lastIndexOf(item),
      1
    );
  }
};
//check empy object
function isEmpty(val) {
  return val === undefined || val == null || val.length <= 0 ? true : false;
}
onMounted(() => {
  loadData(true);
  loadTudien();
  return {
    datalists,
    options,
    onPage,
    loadData,
    loadCount,
    openBasic,
    closeDialog,
    basedomainURL,

    saveData,
    isFirst,
    searchStamp,
    onCheckBox,
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
      dataKey="bank_id"
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
                @keyup="searchStamp"
                type="text"
                spellcheck="false"
                placeholder="Tìm kiếm"
              />
              <Button
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
              </OverlayPanel>
            </span>
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
              @click="openBasic('Thêm thẻ bảo hiểm')"
              label="Thêm mới"
              icon="pi pi-plus"
              class="mr-2"
            />
            <Button
              @click="refreshStamp"
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
        </Toolbar></template
      >

      <Column
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:70px;height:50px"
        bodyStyle="text-align:center;max-width:70px"
        selectionMode="multiple"
      >
      </Column>

      <Column
        field="STT"
        header="STT"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:50px;height:50px"
        bodyStyle="text-align:center;max-width:50px"
        :sortable="true"
      ></Column>

      <Column
        field="profile_id"
        header="Mã nhân sự"
        headerStyle="text-align:center;max-width:100px;height:50px"
        bodyStyle="text-align:center;max-width:100px"
        class="align-items-center justify-content-center text-center"
      >
      </Column>

      <Column
        field="profile_user_name"
        header="Họ và tên"
        headerStyle="text-align:left;height:50px"
        bodyStyle="text-align:left"
      >
      </Column>
      <Column
        field="organization_name"
        header="Phòng ban"
        headerStyle="text-align:center;max-width:200px;height:50px"
        bodyStyle="text-align:center;max-width:200px;;max-height:60px"
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
      </Column>
      <Column
        field="insurance_id"
        header="Số sổ"
        headerStyle="text-align:center;max-width:150px;height:50px"
        bodyStyle="text-align:center;max-width:150px;;max-height:60px"
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
          {{ moment(new Date(data.batdaudong)).format("MM/YYYY ") }}
        </template>
      </Column>
            <Column
        field="mucdong"
        header="Mức đóng"
        headerStyle="text-align:center;max-width:150px;height:50px"
        bodyStyle="text-align:center;max-width:150px;;max-height:60px"
        class="align-items-center justify-content-center text-center"
      >
      </Column>
            <Column
        field="congtydong"
        header="Công ty đóng"
        headerStyle="text-align:center;max-width:150px;height:50px"
        bodyStyle="text-align:center;max-width:150px;;max-height:60px"
        class="align-items-center justify-content-center text-center"
      >
      </Column>
            <Column
        field="nhanviendong"
        header="Người lao động đóng"
        headerStyle="text-align:center;max-width:150px;height:50px"
        bodyStyle="text-align:center;max-width:150px;;max-height:60px"
        class="align-items-center justify-content-center text-center"
      >
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

  <Dialog
    :header="headerDialog"
    v-model:visible="displayBasic"
    :style="{ width: '55vw' }"
    :closable="true"
    :modal="true"
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left p-0"
            >Số sổ bảo hiểm <span class="redsao">(*)</span></label
          >
          <InputText
            v-model="insurance.insurance_id"
            spellcheck="false"
            class="col-10 ip33"
          />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left p-0">Trạng thái</label>
          <Dropdown
            class="col-10 ip33"
            v-model="insurance.status"
            :options="statuss"
            optionLabel="text"
            optionValue="value"
            placeholder="Trạng thái"
            :showClear="true"
          />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left p-0">Pháp nhân đóng</label>
          <Dropdown
            class="col-10 ip33"
            v-model="insurance.organization_payment"
            :options="dictionarys[0]"
            optionLabel="organization_name"
            optionValue="organization_name"
            :editable="true"
          />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left p-0">Số thẻ BHYT</label>
          <InputText
            v-model="insurance.insurance_code"
            spellcheck="false"
            class="col-10 ip33"
          />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left p-0">Mã tỉnh cấp</label>
          <Dropdown
            class="col-10 ip33"
            v-model="insurance.insurance_province_id"
            :options="dictionarys[1]"
            optionLabel="insurance_province_name"
            optionValue="insurance_province_id"
            placeholder="Mã tỉnh"
            :showClear="true"
          />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left p-0">Nơi đăng ký</label>
          <Dropdown
            class="col-10 ip33"
            v-model="insurance.hospital_name"
            :options="dictionarys[2]"
            optionLabel="hospital_name"
            optionValue="hospital_name"
            :editable="true"
          />
        </div>
        <h4 class="my-2">Lịch sử đóng bảo hiểm</h4>
        <div style="text-align: end" class="field col-12 md:col-12">
          <!-- <Button
            label="Thêm"
            icon="pi pi-plus"
            style="padding: 0.3rem 0.6rem !important"
            class="p-button-outlined p-button-secondary"
            @click="addRow_Item(1)"
          /> -->
          <a
            @click="addRow_Item(1)"
            class="cursor-pointer"
            v-tooltip.top="'Thêm'"
          >
            <i class="pi pi-plus-circle" style="font-size: 18px"></i>
          </a>
        </div>
        <div style="overflow-x: scroll" class="scroll-outer">
          <table
            class="table table-condensed table-hover tbpad table-child"
            style="table-layout: fixed"
          >
            <thead>
              <tr>
                <th
                  class="text-center row-bc sticky"
                  style="width: 100px; left: 0px !important"
                ></th>
                <th class="text-center row-bc" style="width: 150px">
                  Từ tháng
                </th>
                <th class="text-center row-bc" style="width: 150px">
                  Hình thức
                </th>
                <th class="text-center row-bc" style="width: 150px">Lý do</th>
                <th class="text-center row-bc" style="width: 200px">
                  Pháp nhân đóng
                </th>
                <th class="text-center row-bc" style="width: 150px">
                  Mức đóng
                </th>
                <th class="text-center row-bc" style="width: 150px">
                  Công ty đóng
                </th>
                <th class="text-center row-bc" style="width: 150px">
                  NLĐ đóng
                </th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(item, index) in insurance_pays" :key="index">
                <td
                  class="sticky"
                  align="center"
                  style="
                    color: black;
                    width: 100px;
                    left: 0px !important;
                    z-index: 100;
                  "
                >
                  <i
                    class="pi pi-times text-xl cursor-pointer"
                    style="color: red"
                    v-tooltip.top="'Xóa'"
                    @click="delRow_Item(item, 1)"
                  ></i>
                </td>
                <td align="center">
                  <Calendar
                    v-model="item.start_date"
                    view="month"
                    dateFormat="mm/yy"
                    class="ip33"
                    style="width: 150px"
                    placeholder="Bắt đầu"
                  />
                </td>
                <td align="center">
                  <Dropdown
                    class="ip33"
                    v-model="item.payment_form"
                    :options="hinhthucs"
                    optionLabel="text"
                    optionValue="text"
                    style="width: 150px"
                    :showClear="true"
                  />
                </td>
                <td align="center">
                  <InputText
                    spellcheck="false"
                    class="ip33"
                    style="width: 150px"
                    v-model="item.reason"
                  />
                </td>
                <td align="center">
                  <Dropdown
                    class="ip33"
                    v-model="item.organization_payment"
                    :options="dictionarys[0]"
                    optionLabel="organization_name"
                    optionValue="organization_name"
                    :editable="true"
                    style="width: 200px"
                  />
                </td>
                <td align="center">
                  <InputNumber
                    class="ip33 p-0"
                    v-model="item.total_payment"
                    style="witdh: 150px"
                  />
                </td>
                <td align="center">
                  <InputNumber
                    class="ip33 p-0"
                    v-model="item.company_payment"
                    style="witdh: 150px"
                  />
                </td>
                <td align="center">
                  <InputNumber
                    class="ip33 p-0"
                    v-model="item.member_payment"
                    style="witdh: 150px"
                  />
                </td>
              </tr>
            </tbody>
          </table>
        </div>
        <h4 class="my-2">Lịch sử giải quyết chế độ</h4>
        <div style="text-align: end" class="field col-12 md:col-12">
          <!-- <Button
            label="Thêm"
            icon="pi pi-plus"
            style="padding: 0.3rem 0.6rem !important"
            class="p-button-outlined p-button-secondary"
            @click="addRow_Item(2)"
          /> -->
          <a
            @click="addRow_Item(2)"
            class="cursor-pointer"
            v-tooltip.top="'Thêm'"
          >
            <i class="pi pi-plus-circle" style="font-size: 18px"></i>
          </a>
        </div>
        <div style="overflow-x: auto" class="scroll-outer">
          <table
            class="table table-condensed table-hover tbpad table-child"
            style="table-layout: fixed"
          >
            <thead>
              <tr>
                <th
                  class="text-center row-bc sticky"
                  style="width: 100px; left: 0px !important"
                ></th>
                <th class="text-center row-bc" style="width: 200px">
                  Loại chế độ
                </th>
                <th class="text-center row-bc" style="width: 150px">
                  Ngày nhận hồ sơ
                </th>
                <th class="text-center row-bc" style="width: 150px">
                  Ngày hoàn thiện thủ tục
                </th>
                <th class="text-center row-bc" style="width: 150px">
                  Ngày nhận tiền BH trả
                </th>
                <th class="text-center row-bc" style="width: 150px">Số tiền</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(item, index) in insurance_resolves" :key="index">
                <td
                  class="sticky"
                  align="center"
                  style="
                    color: black;
                    width: 100px;
                    left: 0px !important;
                    z-index: 100;
                  "
                >
                  <i
                    class="pi pi-times text-xl cursor-pointer"
                    style="color: red"
                    v-tooltip.top="'Xóa'"
                    @click="delRow_Item(item, 2)"
                  ></i>
                </td>
                <td align="center">
                  <Dropdown
                    class="ip33"
                    v-model="item.type_mode"
                    :options="dictionarys[3]"
                    optionLabel="insurance_type_mode_name"
                    optionValue="insurance_type_mode_name"
                    :editable="true"
                    style="width: 200px"
                  />
                </td>
                <td align="center">
                  <Calendar
                    style="width: 150px"
                    class="ip33"
                    id="icon"
                    v-model="item.received_file_date"
                    :showIcon="true"
                  />
                </td>
                <td align="center">
                  <Calendar
                    style="width: 150px"
                    class="ip33"
                    id="icon"
                    v-model="item.completed_date"
                    :showIcon="true"
                  />
                </td>
                <td align="center">
                  <Calendar
                    style="width: 150px"
                    class="ip33"
                    id="icon"
                    v-model="item.received_money_date"
                    :showIcon="true"
                  />
                </td>
                <td align="center">
                  <InputNumber
                    class="ip33 p-0"
                    v-model="item.money"
                    style="witdh: 150px"
                  />
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="closeDialog"
        class="p-button-outlined"
      />

      <Button
        label="Lưu"
        icon="pi pi-check"
        @click="saveData(!v$.$invalid)"
        autofocus
      />
    </template>
  </Dialog>
</template>
    
    <style scoped>
.scroll-outer {
  margin: 0 1rem;
}
.ip33 {
  height: 33px !important;
}
</style>
