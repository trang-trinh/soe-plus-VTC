<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import { encr, checkURL } from "../../../util/function.js";
import moment from "moment";
import dialogTraining from "./component/dialog_training.vue";
//Khai báo

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
  training_emps_name: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
  training_emps_code: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
});
const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);

const listStatus = ref([
  { name: "Lên kế hoạch", code: 1 },
  { name: "Đang thực hiện", code: 2 },
  { name: "Đã hoàn thành", code: 3 },
  { name: "Tạm dừng", code: 4 },
  { name: "Đã hủy", code: 5 },
]);
const listFormTraining = ref([
  { name: "Bắt buộc", code: 1 },
  { name: "Đăng ký", code: 2 },
  { name: "Cả hai", code: 3 },
]);
//Lấy số bản ghi
const loadCount = () => {
  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_training_emps_count",
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
      let data1 = JSON.parse(response.data.data)[1];
      let data2 = JSON.parse(response.data.data)[2];
      let data3 = JSON.parse(response.data.data)[3];
      let data4 = JSON.parse(response.data.data)[4];
      let data5 = JSON.parse(response.data.data)[5];
      if (data.length > 0) {
        options.value.totalRecords = data[0].totalRecords;
        options.value.totalRecords1 = data1[0].totalRecords1;
        options.value.totalRecords2 = data2[0].totalRecords2;
        options.value.totalRecords3 = data3[0].totalRecords3;
        options.value.totalRecords4 = data4[0].totalRecord4;
        options.value.totalRecords5 = data5[0].totalRecords5;

        sttStamp.value = data[0].totalRecords + 1;
      }
    })
    .catch((error) => {});
};

//Lấy dữ liệu training_emps
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
        baseURL + "/api/hrm_ca_SQL/getData",
        {
          str: encr(
            JSON.stringify({
              proc: "hrm_training_emps_list",
              par: [
                { par: "pageno", va: options.value.PageNo },
                { par: "pagesize", va: options.value.PageSize },
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
        if (isFirst.value) isFirst.value = false;
        data.forEach((element, i) => {
          element.STT = options.value.PageNo * options.value.PageSize + i + 1;
          if (element.li_user_verify) {
            element.li_user_verify = JSON.parse(element.li_user_verify);
          }
        });
        datalists.value = data;

        options.value.loading = false;
      })
      .catch((error) => {
        toast.error("Tải dữ liệu không thành công!");
        options.value.loading = false;
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

    options.value.id =
      datalists.value[datalists.value.length - 1].training_emps_id;
    options.value.IsNext = true;
  } else if (event.page < options.value.PageNo) {
    //Trang trước
    options.value.id = datalists.value[0].training_emps_id;
    options.value.IsNext = false;
  }
  options.value.PageNo = event.page;
  loadData(true);
};

const training_emps = ref({
  training_emps_name: "",
  emote_file: "",
  status: true,
  is_default: false,
  is_order: 1,
});

const selectedStamps = ref();

const isSaveTem = ref(true);
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
  totalRecords: 0,
  tab: -1,
  totalRecords1: 0,
  totalRecords2: 0,
  totalRecords3: 0,
  totalRecords4: 0,
  totalRecords5: 0,
});

//Hiển thị dialog
const headerDialog = ref();
const displayBasic = ref(false);
const openBasic = (str) => {
  training_emps.value = {
    training_emps_code: null,
    training_emps_name: null,
    form_training: 1,
    status: 1,
    training_place: null,
    is_order: sttStamp.value,
    organization_id: store.getters.user.organization_id,
    user_follows_fake: [],
    user_verify_fake: [],
    organization_training_fake: {},
  };

  if (store.getters.user.organization_id)
    training_emps.value.organization_training_fake[
      store.getters.user.organization_id
    ] = true;

  isSaveTem.value = true;
  headerDialog.value = str;

  displayBasic.value = true;
};

const closeDialog = () => {
  training_emps.value = {
    training_emps_name: "",
    emote_file: "",
    status: true,
    is_default: false,
    is_order: 1,
  };

  displayBasic.value = false;
  loadData(true);
};
const sttStamp = ref(1);

//Sửa bản ghi
const editTem = (dataTem) => {
  training_emps.value = dataTem;
  headerDialog.value = "Sửa đào tạo";
  isSaveTem.value = false;
  displayBasic.value = true;
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
          .delete(baseURL + "/api/hrm_training_emps/delete_hrm_training_emps", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: Tem != null ? [Tem.training_emps_id] : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá thông tin đào tạo thành công!");
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
const checkLoadCount = ref(true);
const loadDataSQL = () => {
  datalists.value = [];

  let data = {
    id: "training_emps_id",
    sqlS: null,
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
    .post(baseURL + "/api/HRM_SQL/Filter_hrm_training_emps", data, config)
    .then((response) => {
      let dt = JSON.parse(response.data.data);
      let data = dt[0];
      if (data.length > 0) {
        data.forEach((element, i) => {
          element.STT = options.value.PageNo * options.value.PageSize + i + 1;
          if (element.li_user_verify) {
            element.li_user_verify = JSON.parse(element.li_user_verify);
          }
        });

        datalists.value = data;
      } else {
        datalists.value = [];
      }
      if (isFirst.value) isFirst.value = false;
      options.value.loading = false;
      //Show Count nếu có
      if (dt.length >= 2 &&checkLoadCount.value==true) {
        options.value.totalRecords = dt[1][0].totalRecords;
        options.value.totalRecords1 = dt[2][0].totalRecords1;
        options.value.totalRecords2 = dt[3][0].totalRecords2;
        options.value.totalRecords3 = dt[4][0].totalRecords3;
        options.value.totalRecords4 = dt[5][0].totalRecords4;
        options.value.totalRecords5 = dt[6][0].totalRecords5;
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

const setStatus =(value)=>{
   opstatus.value.hide();
    let data = {
    IntID: value.training_emps_id,
    TextID: value.training_emps_id + "",
    IntTrangthai:  value.status,
    BitTrangthai: false,
  };
   axios
      .put(
        baseURL + "/api/hrm_training_emps/update_s_hrm_training_emps",
        data,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Cập nhật trạng thái thành công!");
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
const opstatus = ref();
const toggleStatus = (item, event) => {
  training_emps.value = item;
  opstatus.value.toggle(event);
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
  options.value.status_filter = null;
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
const tabs = ref([
  { id: 0, title: "Tất cả", icon: "", total: options.value.totalRecords },
  { id: 1, title: "Lên kế hoạch", icon: "", total: 0 },
  { id: 2, title: "Đang thực hiện", icon: "", total: 0 },
  { id: 3, title: "Đã hoàn thành", icon: "", total: 0 },
  { id: 4, title: "Tạm dừng", icon: "", total: 0 },
  { id: 5, title: "Đã hủy", icon: "", total: 0 },
]);
//Checkbox
const onCheckBox = (value, check) => {
  if (check) {
    let data = {
      IntID: value.training_emps_id,
      TextID: value.training_emps_id + "",
      IntTrangthai: 1,
      BitTrangthai: value.status,
    };
    axios
      .put(
        baseURL + "/api/hrm_training_emps/update_s_hrm_training_emps",
        data,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa trạng thái đào tạo thành công!");
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
      IntID: value.training_emps_id,
      TextID: value.training_emps_id + "",
      BitMain: value.is_default,
    };
    axios
      .put(
        baseURL + "/api/hrm_training_emps/Update_DefaultStamp",
        data1,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa trạng thái đào tạo thành công!");
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
const activeTab = (tab) => {
   
  options.value.tab = tab.id;
  reFilter();
  if (tab.id) {
    checkLoadCount.value=false;
    let filterS1 = {
      filterconstraints: [{ value: tab.id , matchMode: "equals" }],
      filteroperator: "and",
      key: "status",
    };

    filterSQL.value.push(filterS1);
  }

  loadDataSQL();
};
const menuButMores = ref();
const itemButMores = ref([
  {
    label: "Hiệu chỉnh nội dung",
    icon: "pi pi-pencil",
    command: (event) => {
      editTem(training_emps.value, "Chỉnh sửa hợp đồng");
    },
  },
  {
    label: "Xoá",
    icon: "pi pi-trash",
    command: (event) => {
      delTem(training_emps.value);
    },
  },
]);
const toggleMores = (event, item) => {
  training_emps.value = item;
  menuButMores.value.toggle(event);
  //selectedNodes.value = item;
};
//Xóa nhiều
const deleteList = () => {
  let listId = new Array(selectedStamps.value.length);
  let checkD = false;

  if (!checkD) {
    swal
      .fire({
        title: "Thông báo",
        text: "Bạn có muốn xoá thông tin đào tạo này không!",
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
            listId.push(item.training_emps_id);
          });
          axios
            .delete(
              baseURL + "/api/hrm_training_emps/delete_hrm_training_emps",
              {
                headers: { Authorization: `Bearer ${store.getters.token}` },
                data: listId != null ? listId : 1,
              }
            )
            .then((response) => {
              swal.close();
              if (response.data.err != "1") {
                swal.close();
                toast.success("Xoá thông tin đào tạo thành công!");
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
const reFilter=()=>{
 options.value.type_formtraining = null;

  options.value.status_filter = null;
  checkLoadCount.value=true;
  isDynamicSQL.value = false;
  checkFilter.value = false;
  filterSQL.value = [];
  options.value.SearchText = null;
}
const reFilterEmail = () => {
 reFilter();
  op.value.hide();
  loadData(true);
};
const filterFileds = () => {
  filterSQL.value = [];
  checkFilter.value = true;
  if (options.value.status_filter) {
    let filterS1 = {
      filterconstraints: [],
      filteroperator: "or",
      key: "status",
    };
    if (options.value.status_filter.length > 0) {
      options.value.status_filter.forEach((element) => {
        var addr = { value: element, matchMode: "equals" };
        filterS1.filterconstraints.push(addr);
      });

      filterSQL.value.push(filterS1);
    }
  }
  if (options.value.type_formtraining) {
    let filterS2 = {
      filterconstraints: [],
      filteroperator: "or",
      key: "form_training",
    };
    if (options.value.type_formtraining.length > 0) {
      options.value.type_formtraining.forEach((element) => {
        var addr = { value: element, matchMode: "equals" };
        filterS2.filterconstraints.push(addr);
      });

      filterSQL.value.push(filterS2);
    }
  }
  loadDataSQL();
  op.value.hide();
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

onMounted(() => {
  if (!checkURL(window.location.pathname, store.getters.listModule)) {
    //router.back();
  }
  loadData(true);

  return {
    datalists,
    options,
    onPage,
    loadData,
    loadCount,
    openBasic,
    closeDialog,
    basedomainURL,

    isFirst,
    searchStamp,
    onCheckBox,
    selectedStamps,
    deleteList,
  };
});
</script>
    <template>
  <div class="p-3 surface-100">
    <div class="main-layout true flex-grow-1 pb-0 pr-0 surface-0">
      <div class="p-3 pb-0">
        <h3 class="module-title mt-0 ml-1 mb-2">
          <i class="pi pi-book"></i> Danh sách đào tạo
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
                @click="toggle"
                type="button"
                class="ml-2 p-button-outlined p-button-secondary"
                aria:haspopup="true"
                aria-controls="overlay_panel"
                :class="
                  options.status_filter == null &&
                  options.form_training == null &&
                  !checkFilter
                    ? ''
                    : 'p-button-secondary p-button-outlined'
                "
              >
                <div>
                  <span class="mr-2"><i class="pi pi-filter"></i></span>
                  <span class="mr-2">Lọc dữ liệu</span>
                  <span><i class="pi pi-chevron-down"></i></span>
                </div>
              </Button>

              <OverlayPanel
                ref="op"
                appendTo="body"
                class="p-0 m-0"
                :showCloseIcon="false"
                id="overlay_panel"
                style="width: 700px"
              >
                <div class="grid formgrid m-0">
                  <div
                    class="col-12 md:col-12 p-0"
                    style="
                      min-height: unset;
                      max-height: calc(100vh - 300px);
                      overflow: auto;
                    "
                  >
                    <div class="flex">
                      <div class="col-6 md:col-6">
                        <div class="row">
                          <div class="col-12 md:col-12 p-0">
                            <div class="form-group">
                              <label>Nhóm đào tạo</label>
                              <MultiSelect
                                :options="organizations"
                                :filter="true"
                                :showClear="true"
                                :editable="false"
                                v-model="options.organizations"
                                optionLabel="organization_name"
                                placeholder="Chọn nhóm đào tạo"
                                class="w-full limit-width"
                                style="min-height: 36px"
                                panelClass="d-design-dropdown"
                              >
                                <template #value="slotProps">
                                  <ul
                                    class="p-ulchip"
                                    v-if="
                                      slotProps.value &&
                                      slotProps.value.length > 0
                                    "
                                  >
                                    <li
                                      class="p-lichip"
                                      v-for="(value, index) in slotProps.value"
                                      :key="index"
                                    >
                                      <Chip class="mr-2 mb-2 px-3 py-2">
                                        <div class="flex">
                                          <div>
                                            <span>{{
                                              value.organization_name
                                            }}</span>
                                          </div>
                                          <span
                                            tabindex="0"
                                            class="
                                              p-chip-remove-icon
                                              pi pi-times-circle
                                              format-flex-center
                                            "
                                            @click="
                                              removeFilter(
                                                index,
                                                options.organizations
                                              );
                                              $event.stopPropagation();
                                            "
                                            v-tooltip.top="'Xóa'"
                                          ></span>
                                        </div>
                                      </Chip>
                                    </li>
                                  </ul>
                                  <span v-else>
                                    {{ slotProps.placeholder }}
                                  </span>
                                </template>
                              </MultiSelect>
                            </div>
                          </div>
                          <div class="col-12 md:col-12 p-0">
                            <div class="form-group">
                              <label>Người phụ trách</label>
                              <MultiSelect
                                :options="departments"
                                :filter="true"
                                :showClear="true"
                                :editable="false"
                                v-model="options.departments"
                                optionLabel="department_name"
                                placeholder="Chọn người phụ trách"
                                class="w-full limit-width"
                                style="min-height: 36px"
                                panelClass="d-design-dropdown"
                              >
                                <template #value="slotProps">
                                  <ul
                                    class="p-ulchip"
                                    v-if="
                                      slotProps.value &&
                                      slotProps.value.length > 0
                                    "
                                  >
                                    <li
                                      class="p-lichip"
                                      v-for="(value, index) in slotProps.value"
                                      :key="index"
                                    >
                                      <Chip class="mr-2 mb-2 px-3 py-2">
                                        <div class="flex">
                                          <div>
                                            <span>{{
                                              value.department_name
                                            }}</span>
                                          </div>
                                          <span
                                            tabindex="0"
                                            class="
                                              p-chip-remove-icon
                                              pi pi-times-circle
                                              format-flex-center
                                            "
                                            @click="
                                              removeFilter(
                                                index,
                                                options.departments
                                              );
                                              $event.stopPropagation();
                                            "
                                            v-tooltip.top="'Xóa'"
                                          ></span>
                                        </div>
                                      </Chip>
                                    </li>
                                  </ul>
                                  <span v-else>
                                    {{ slotProps.placeholder }}
                                  </span>
                                </template>
                              </MultiSelect>
                            </div>
                          </div>
                          <div class="col-12 md:col-12 p-0">
                            <div class="form-group">
                              <label>Hình thức đào tạo</label>
                              <MultiSelect
                                :options="listFormTraining"
                                :filter="false"
                                :showClear="true"
                                :editable="false"
                                v-model="options.type_formtraining"
                                optionLabel="name"
                                optionValue="code"
                                display="chip"
                                placeholder="Chọn hình thức đào tạo"
                                class="w-full limit-width"
                                style="min-height: 36px"
                                panelClass="d-design-dropdown"
                              >
                              </MultiSelect>
                            </div>
                          </div>
                        </div>
                      </div>
                      <div class="col-6 md:col-6">
                        <div class="row">
                          <div class="col-12 md:col-12">
                            <div class="form-group m-0">
                              <label>Thời gian đào tạo</label>
                            </div>
                          </div>
                          <div class="col-12 p-0 flex">
                            <div class="col-6 md:col-6">
                              <div class="form-group">
                                <Calendar
                                  :showIcon="true"
                                  class="ip36"
                                  autocomplete="on"
                                  inputId="time24"
                                  v-model="options.sign_start_date"
                                  @date-select="changeSignDate()"
                                  @input="changeSignDate()"
                                  placeholder="Từ ngày"
                                />
                              </div>
                            </div>
                            <div class="col-6 md:col-6">
                              <div class="form-group">
                                <Calendar
                                  :showIcon="true"
                                  class="ip36"
                                  autocomplete="on"
                                  inputId="time24"
                                  v-model="options.sign_end_date"
                                  @date-select="changeSignDate()"
                                  @input="changeSignDate()"
                                  placeholder="Đến ngày"
                                />
                              </div>
                            </div>
                          </div>
                          <div class="col-12 md:col-12">
                            <div class="form-group">
                              <label>Giảng viên</label>
                              <MultiSelect
                                :options="users"
                                v-model="options.users"
                                :filter="true"
                                :showClear="true"
                                :editable="false"
                                optionLabel="full_name"
                                placeholder="Chọn giảng viên"
                                class="w-full limit-width"
                                style="min-height: 36px"
                                panelClass="d-design-dropdown"
                              >
                                <template #value="slotProps">
                                  <ul
                                    class="p-ulchip"
                                    v-if="
                                      slotProps.value &&
                                      slotProps.value.length > 0
                                    "
                                  >
                                    <li
                                      class="p-lichip"
                                      v-for="(value, index) in slotProps.value"
                                      :key="index"
                                    >
                                      <Chip
                                        :image="value.avatar"
                                        :label="value.full_name"
                                        class="mr-2 mb-2 px-3 py-2"
                                      >
                                        <div class="flex">
                                          <div class="format-flex-center">
                                            <Avatar
                                              v-bind:label="
                                                value.avatar
                                                  ? ''
                                                  : (
                                                      value.last_name ?? ''
                                                    ).substring(0, 1)
                                              "
                                              v-bind:image="
                                                value.avatar
                                                  ? basedomainURL + value.avatar
                                                  : basedomainURL +
                                                    '/Portals/Image/noimg.jpg'
                                              "
                                              style="
                                                background-color: #2196f3;
                                                color: #ffffff;
                                                width: 2rem;
                                                height: 2rem;
                                              "
                                              :style="{
                                                background:
                                                  bgColor[value.is_order % 7],
                                              }"
                                              class="mr-2 text-avatar"
                                              size="xlarge"
                                              shape="circle"
                                            />
                                          </div>
                                          <div
                                            class="format-flex-center text-left"
                                          >
                                            <span>{{ value.full_name }}</span>
                                          </div>
                                          <span
                                            tabindex="0"
                                            class="
                                              p-chip-remove-icon
                                              pi pi-times-circle
                                              format-flex-center
                                            "
                                            @click="
                                              removeFilter(
                                                index,
                                                options.users
                                              );
                                              $event.stopPropagation();
                                            "
                                            v-tooltip.top="'Xóa'"
                                          ></span>
                                        </div>
                                      </Chip>
                                    </li>
                                  </ul>
                                  <span v-else>
                                    {{ slotProps.placeholder }}
                                  </span>
                                </template>
                                <template #option="slotProps">
                                  <div v-if="slotProps.option" class="flex">
                                    <div class="format-center">
                                      <Avatar
                                        v-bind:label="
                                          slotProps.option.avatar
                                            ? ''
                                            : slotProps.option.last_name.substring(
                                                0,
                                                1
                                              )
                                        "
                                        v-bind:image="
                                          slotProps.option.avatar
                                            ? basedomainURL +
                                              slotProps.option.avatar
                                            : basedomainURL +
                                              '/Portals/Image/noimg.jpg'
                                        "
                                        style="
                                          background-color: #2196f3;
                                          color: #ffffff;
                                          width: 3rem;
                                          height: 3rem;
                                          font-size: 1.4rem !important;
                                        "
                                        :style="{
                                          background:
                                            bgColor[
                                              slotProps.option.is_order % 7
                                            ],
                                        }"
                                        class="text-avatar"
                                        size="xlarge"
                                        shape="circle"
                                      />
                                    </div>
                                    <div class="ml-3">
                                      <div class="mb-1">
                                        {{ slotProps.option.full_name }}
                                      </div>
                                      <div class="description">
                                        <div>
                                          {{ slotProps.option.position_name }}
                                        </div>
                                        <div>
                                          {{ slotProps.option.department_name }}
                                        </div>
                                      </div>
                                    </div>
                                  </div>
                                  <span v-else> Chưa có dữ liệu </span>
                                </template>
                              </MultiSelect>
                            </div>
                          </div>

                          <div class="col-12 md:col-12">
                            <div class="form-group">
                              <label>Trạng thái</label>
                              <MultiSelect
                                :options="listStatus"
                                v-model="options.status_filter"
                                :filter="true"
                                :showClear="true"
                                :editable="false"
                                display="chip"
                                optionLabel="name"
                                optionValue="code"
                                placeholder="Chọn trạng thái"
                                class="w-full limit-width"
                                style="min-height: 36px"
                                panelClass="d-design-dropdown"
                              >
                              </MultiSelect>
                            </div>
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>
                  <div class="col-12 md:col-12 p-0">
                    <Toolbar
                      class="
                        border-none
                        surface-0
                        outline-none
                        px-0
                        pb-0
                        w-full
                      "
                    >
                      <template #start>
                        <Button
                          @click="reFilterEmail()"
                          class="p-button-outlined"
                          label="Bỏ chọn"
                        ></Button>
                      </template>
                      <template #end>
                        <Button @click="filterFileds()" label="Lọc"></Button>
                      </template>
                    </Toolbar>
                  </div>
                </div>
              </OverlayPanel>
            </span>
          </template>

          <template #end>
            <Button
              @click="openBasic('Thêm mới đào tạo')"
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
            <Button
              v-if="checkDelList"
              @click="deleteList()"
              label="Xóa"
              icon="pi pi-trash"
              class="mr-2 p-button-danger"
            />
            <Button
              @click="toggleExport"
              label="Tiện ích"
              icon="pi pi-file-excel"
              class="p-button-outlined p-button-secondary"
              aria-haspopup="true"
              aria-controls="overlay_Export"
            >
              <div>
                <span class="mr-2">Tiện ích</span>
                <span><i class="pi pi-chevron-down"></i></span>
              </div>
            </Button>
          </template>
        </Toolbar>
      </div>
      <div class="tabview">
        <div class="tableview-nav-content">
          <ul class="tableview-nav">
            <li
              v-for="(tab, key) in tabs"
              :key="key"
              @click="activeTab(tab)"
              class="tableview-header"
              :class="{ highlight: options.tab === tab.id }"
            >
              <a>
                <i :class="tab.icon"></i>
                <span
                  >{{ tab.title }} ({{
                    tab.id == 1
                      ? options.totalRecords1
                      : tab.id == 2
                      ? options.totalRecords2
                      : tab.id == 3
                      ? options.totalRecords3
                      : tab.id == 4
                      ? options.totalRecords4
                      : tab.id == 5
                      ? options.totalRecords5
                      : options.totalRecords
                  }})</span
                >
              </a>
            </li>
          </ul>
        </div>
      </div>
      <div class="d-container">
        <div class="d-lang-table">
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
            dataKey="training_emps_id"
            responsiveLayout="scroll"
            v-model:selection="selectedStamps"
            :row-hover="true"
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
              headerStyle="text-align:center;max-width:70px;height:50px"
              bodyStyle="text-align:center;max-width:70px"
            ></Column>
            <Column
              field="training_emps_code"
              header="Mã số"
              headerStyle="text-align:center;max-width:150px;height:50px"
              bodyStyle="text-align:center;max-width:150px"
              class="align-items-center justify-content-center text-center"
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
              field="training_emps_name"
              header="Tên khoá đào tạo"
              :sortable="true"
              headerStyle="text-align:left;height:50px"
              bodyStyle="text-align:left"
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
              field="form_training"
              header="Hình thức"
              headerStyle="text-align:center;max-width:100px;height:50px"
              bodyStyle="text-align:center;max-width:100px"
              class="align-items-center justify-content-center text-center"
            >
              <template #body="data">
                <div>
                  {{
                    data.data.form_training == 1
                      ? "Bắt buộc"
                      : data.data.form_training == 2
                      ? "Đăng ký"
                      : "Cả hai"
                  }}
                </div>
              </template>
            </Column>
            <Column
              field="start_date"
              header="Từ ngày"
              headerStyle="text-align:center;max-width:100px;height:50px"
              bodyStyle="text-align:center;max-width:100px"
              class="align-items-center justify-content-center text-center"
            >
              <template #body="data">
                <div v-if="data.data.start_date">
                  {{
                    moment(new Date(data.data.start_date)).format("DD/MM/YYYY")
                  }}
                </div>
              </template>
            </Column>

            <Column
              field="end_date"
              header="Đến ngày"
              headerStyle="text-align:center;max-width:100px;height:50px"
              bodyStyle="text-align:center;max-width:100px"
              class="align-items-center justify-content-center text-center"
            >
              <template #body="data">
                <div v-if="data.data.end_date">
                  {{
                    moment(new Date(data.data.end_date)).format("DD/MM/YYYY")
                  }}
                </div>
              </template>
            </Column>
            <Column
              field="li_user_verify"
              header="Giảng viên"
              headerStyle="text-align:center;max-width:150px;height:50px"
              bodyStyle="text-align:center;max-width:150px"
              class="align-items-center justify-content-center text-center"
            >
              <template #body="data">
                <div>
                  <AvatarGroup>
                    <Avatar
                      v-for="(item, index) in data.data.li_user_verify.slice(
                        0,
                        4
                      )"
                      :key="index"
                      :style="
                        item.avatar
                          ? 'background-color: #2196f3'
                          : 'background:' + bgColor[item.full_name.length % 7]
                      "
                      :image="basedomainURL + item.avatar"
                      class="w-3rem h-3rem"
                      shape="circle"
                    />
                    <Avatar
                      v-if="data.data.li_user_verify.length > 4"
                      :label="(data.data.li_user_verify.length - 4).toString()"
                      shape="circle"
                      class="w-3rem h-3rem"
                      style="
                        background-color: #9c27b0;
                        color: #ffffff;
                        font-size: 12pt !important;
                      "
                    />
                  </AvatarGroup>
                </div>
              </template>
            </Column>
            <Column
              field="count_emps"
              header="Học viên"
              headerStyle="text-align:center;max-width:100px;height:50px"
              bodyStyle="text-align:center;max-width:100px"
              class="align-items-center justify-content-center text-center"
            >
              <template #body="data">
                <div>
                  {{ data.data.count_emps ? data.data.count_emps : "0" }}
                </div>
              </template>
            </Column>
            <Column
              field="created_date"
              header="Ngày tạo"
              headerStyle="text-align:center;max-width:150px;height:50px"
              bodyStyle="text-align:center;max-width:150px"
              class="align-items-center justify-content-center text-center"
            >
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
              field="status"
              header="Trạng thái"
              headerStyle="text-align:center;max-width:11rem;height:50px"
              bodyStyle="text-align:center;max-width:11rem"
              class="align-items-center justify-content-center text-center"
            >
              <template #body="slotProps">
                <div
                  class="m-2"
                  @click="
                    toggleStatus(slotProps.data, $event);
                    $event.stopPropagation();
                  "
                  aria:haspopup="true"
                  aria-controls="overlay_panel_status"
                >
                  <Button
                    :label="
                      slotProps.data.status == 1
                        ? 'Lên kế hoạch'
                        : slotProps.data.status == 2
                        ? 'Đang thực hiện'
                        : slotProps.data.status == 3
                        ? 'Đã hoàn thành'
                        : slotProps.data.status == 4
                        ? 'Tạm dừng'
                        : 'Đã hủy'
                    "
                    :class="
                      slotProps.data.status == 1
                        ? 'bg-blue-500'
                        : slotProps.data.status == 2
                        ? 'bg-yellow-500'
                        : slotProps.data.status == 3
                        ? 'bg-green-500'
                        : slotProps.data.status == 4
                        ? 'bg-orange-500'
                        : 'bg-pink-500'
                    "
                    icon="pi pi-chevron-down"
                    iconPos="right"
                    class="px-2 w-10rem"
                  />
                </div>
                <OverlayPanel
                  :showCloseIcon="false"
                  ref="opstatus"
                  appendTo="body"
                  class="p-0 m-0"
                  id="overlay_panel_status"
                  style="width: 200px"
                >
                  <div class="form-group">
                   <div class="col-12 p-0 field">
                    Chọn trạng thái
                   </div> <div class="col-12 p-0  ">
                    <Dropdown
                  :options="listStatus"
                  :filter="false"
                  :showClear="false"
                  :editable="false"
                  v-model="training_emps.status"
                  optionLabel="name"
                  optionValue="code"
                  placeholder="Chọn trạng thái"
                  class="w-full"
                  @change="setStatus(training_emps)"
                >
                  
                </Dropdown>
                </div>
                  </div>
                 
                </OverlayPanel>
              </template>
            </Column>
            <Column
              header=""
              headerStyle="text-align:center;max-width:50px"
              bodyStyle="text-align:center;max-width:50px"
              class="align-items-center justify-content-center text-center"
            >
              <template #body="slotProps">
                <Button
                  icon="pi pi-ellipsis-h"
                  class="p-button-rounded p-button-text ml-2"
                  @click="toggleMores($event, slotProps.data)"
                  aria-haspopup="true"
                  aria-controls="overlay_More"
                  v-tooltip.top="'Tác vụ'"
                />
              </template>
            </Column>
            <!-- <Column
              header="Chức năng"
              class="align-items-center justify-content-center text-center"
              headerStyle="text-align:center;max-width:150px;height:50px"
              bodyStyle="text-align:center;max-width:150px"
            >
              <template #body="Tem">
                <div
                  v-if="
                    store.state.user.is_super == true ||
                    store.state.user.user_id == Tem.data.created_by ||
                    (store.state.user.role_id == 'admin' &&
                      store.state.user.organization_id ==
                        Tem.data.organization_id)
                  "
                >
                  <Button
                    @click="editTem(Tem.data)"
                    class="
                      p-button-rounded p-button-secondary p-button-outlined
                      mx-1
                    "
                    type="button"
                    icon="pi pi-pencil"
                    v-tooltip.top="'Sửa'"
                  ></Button>
                  <Button
                    class="
                      p-button-rounded p-button-secondary p-button-outlined
                      mx-1
                    "
                    type="button"
                    icon="pi pi-trash"
                    @click="delTem(Tem.data)"
                    v-tooltip.top="'Xóa'"
                  ></Button>
                </div>
              </template>
            </Column> -->
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
      </div>
    </div>
    <div v-if="displayBasic">
      <dialogTraining
        :headerDialog="headerDialog"
        :displayBasic="displayBasic"
        :training_emps="training_emps"
        :checkadd="isSaveTem"
        :view="false"
        :closeDialog="closeDialog"
      />
    </div>
  </div>

  <Menu
    id="overlay_More"
    ref="menuButMores"
    :model="itemButMores"
    :popup="true"
  />
</template>
  
    <style scoped>
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
  margin: 0px;
  height: calc(100vh - 200px);
}

.tableview-nav {
  background: #ffffff;
  border: 1px solid #dee2e6;
  border-width: 0 0 2px 0;
  display: flex;
  flex: 1 1 auto;
  list-style-type: none;
  margin: 0;
  padding: 0;
}
.tableview-header {
  display: inline-block;
}
.tableview-nav li {
  border: solid #dee2e6;
  border-width: 0 0 2px 0;
  padding: 1.25rem;
  font-weight: 700;
  margin: 0 0 -2px 0;
  transition: background-color 0.2s, border-color 0.2s, box-shadow 0.2s;
}
.tableview-nav li:hover {
  cursor: pointer;
}
.tableview-nav li.highlight {
  background: #ffffff;
  border-color: #3b82f6;
  color: #3b82f6;
}
.tableview-nav li:not(.highlight):hover {
  background: #ffffff;
  border-color: #adb5bd;
  color: #6c757d;
}
.tableview-nav li a:focus {
  outline: 0 none;
  outline-offset: 0;
  box-shadow: inset 0 0 0 0.2rem #bfdbfe;
}
.btn-hidden {
  filter: opacity(40%) !important;
  cursor: auto !important;
}
.hover:hover {
  cursor: pointer;
  color: #2196f3 !important;
}
.inputanh {
  border: 1px solid #ccc;
  width: 8rem;
  height: 8rem;
  cursor: pointer;
  padding: 0.063rem;
  display: block;
  margin-left: auto;
  margin-right: auto;
}
.ipnone {
  display: none;
}
.inputanh img {
  object-fit: cover;
  width: 100%;
  height: 100%;
}
</style>
    