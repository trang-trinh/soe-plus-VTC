<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import { encr, checkURL } from "../../../util/function.js";
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
  type_decision_name: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
});
const rules = {
  type_decision_name: {
    required,
    $errors: [
      {
        $property: "type_decision_name",
        $validator: "required",
        $message: "Tên loại quyết định không được để trống!",
      },
    ],
  },
};

//Lấy số bản ghi
const loadCount = () => {
  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_ca_type_decision_count",
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
        sttStamp.value = data[0].totalRecords + 1;
      }
    })
    .catch((error) => {});
};
//Lấy dữ liệu type_decision
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
              proc: "hrm_ca_type_decision_list",
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

    options.value.id =
      datalists.value[datalists.value.length - 1].type_decision_id;
    options.value.IsNext = true;
  } else if (event.page < options.value.PageNo) {
    //Trang trước
    options.value.id = datalists.value[0].type_decision_id;
    options.value.IsNext = false;
  }
  options.value.PageNo = event.page;
  loadData(true);
};

const type_decision = ref({
  type_decision_name: "",
  emote_file: "",
  status: true,
  is_order: 1,
});

const selectedStamps = ref();
const submitted = ref(false);
const v$ = useVuelidate(rules, type_decision);
const isSaveTem = ref(false);
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
const listTypeContract = ref([]);
const openBasic = (str) => {
  submitted.value = false;
  type_decision.value = {
    type_decision_name: "",
    emote_file: "",
    status: true,
    is_order: sttStamp.value,
    organization_id: store.getters.user.organization_id,
    is_system: store.getters.user.is_super ? true : false,
  };
  checkDisabled.value = false;
  listFilesS.value = [];
  checkIsmain.value = false;
  isSaveTem.value = false;
  headerDialog.value = str;
  displayBasic.value = true;
};

const closeDialog = () => {
  type_decision.value = {
    type_decision_name: "",
    emote_file: "",
    status: true,
    is_order: 1,
  };

  displayBasic.value = false;
  loadData(true);
};

//Thêm bản ghi

const sttStamp = ref(1);
const saveData = (isFormValid) => {
  submitted.value = true;
  if (!isFormValid) {
    return;
  }

  if (type_decision.value.type_decision_name.length > 250) {
    swal.fire({
      title: "Error!",
      text: "Tên loại quyết định không được vượt quá 250 ký tự!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  let formData = new FormData();
  for (var i = 0; i < filesList.value.length; i++) {
    let file = filesList.value[i];
    formData.append("image", file);
  }
 
  formData.append("hrm_files", JSON.stringify(listFilesS.value));
  formData.append("hrm_ca_type_decision", JSON.stringify(type_decision.value));
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  if (!isSaveTem.value) {
    axios
      .post(
        baseURL + "/api/hrm_ca_type_decision/add_hrm_ca_type_decision",
        formData,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Thêm loại quyết định thành công!");
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
    axios
      .put(
        baseURL + "/api/hrm_ca_type_decision/update_hrm_ca_type_decision",
        formData,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa loại quyết định thành công!");

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
const checkIsmain = ref(true);
//Sửa bản ghi
const editTem = (dataTem) => {
  submitted.value = false;

  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_type_decision_get",
            par: [
              {
                par: "user_id",
                va: store.getters.user.user_id,
              },
              {
                par: "type_decision_id",
                va: dataTem.type_decision_id,
              },
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
      if (data) {
        type_decision.value = data[0];

        if (
          type_decision.value.is_system == true &&
          (store.getters.user.is_super == false ||
            store.getters.user.is_super == null)
        ) {
          checkDisabled.value = true;
        }
        if (data1) {
          listFilesS.value = data1;
        }
      }

      headerDialog.value = "Sửa loại quyết định";
      isSaveTem.value = true;
      displayBasic.value = true;
    })
    .catch((error) => {});
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
          .delete(
            baseURL + "/api/hrm_ca_type_decision/delete_hrm_ca_type_decision",
            {
              headers: { Authorization: `Bearer ${store.getters.token}` },
              data: Tem != null ? [Tem.type_decision_id] : 1,
            }
          )
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá loại quyết định thành công!");
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
const checkDisabled = ref(false);
//Xuất excel

const deleteFileH = (value) => {
  listFilesS.value = listFilesS.value.filter((x) => x.file_id != value.file_id);
};
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
    id: "type_decision_id",
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
    .post(baseURL + "/api/hrm_ca_SQL/Filter_hrm_ca_type_decision", data, config)
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
const onCheckBox = (value, check) => {
  if (check) {
    let data = {
      IntID: value.type_decision_id,
      TextID: value.type_decision_id + "",
      IntTrangthai: 1,
      BitTrangthai: value.status,
    };
    axios
      .put(
        baseURL + "/api/hrm_ca_type_decision/update_s_hrm_ca_type_decision",
        data,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa trạng thái loại quyết định thành công!");
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

  if (!checkD) {
    swal
      .fire({
        title: "Thông báo",
        text: "Bạn có muốn xoá loại quyết định này không!",
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
            listId.push(item.type_decision_id);
          });
          axios
            .delete(
              baseURL + "/api/hrm_ca_type_decision/delete_hrm_ca_type_decision",
              {
                headers: { Authorization: `Bearer ${store.getters.token}` },
                data: listId != null ? listId : 1,
              }
            )
            .then((response) => {
              swal.close();
              if (response.data.err != "1") {
                swal.close();
                toast.success("Xoá loại quyết định thành công!");
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

const filesList = ref([]);
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
const initTuDien = () => {
  listTypeContract.value = [];
  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "smartreport_list ",
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
      if (isFirst.value) isFirst.value = false;
      var arrGroups = [];
      data.forEach((element) => {
        var strchk = arrGroups.find((x) => x == element.report_group);
        if (strchk == null) {
          arrGroups.push(element.report_group);
        }
      });
      arrGroups.forEach((item) => {
        var ardf = {
          label: item,
          items: [],
        };
        data
          .filter((x) => x.report_group == item)
          .forEach((z) => {
            ardf.items.push({ label: z.report_name, value: z.report_key });
          });
          listTypeContract.value.push(ardf);
      });

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
const listFilesS = ref([]);
onMounted(() => {
  initTuDien();
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
      :filters="filters"
      :scrollable="true"
      filterDisplay="menu"
      filterMode="lenient"
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
      :row-hover="true"
      dataKey="type_decision_id"
      responsiveLayout="scroll"
      v-model:selection="selectedStamps"
    >
      <template #header>
        <h3 class="module-title mt-0 ml-1 mb-2">
          <i class="pi pi-book"></i> Danh sách loại quyết định ({{
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
                class="p-0 m-0"
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
                        class="col-12 p-0 m-0"
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
              @click="openBasic('Thêm loại quyết định')"
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
         selectionMode="multiple"  v-if="store.getters.user.is_super==true"
      >
      </Column>

      <Column
        field="STT"
        header="STT"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:70px;height:50px"
        bodyStyle="text-align:center;max-width:70px"
        :sortable="true"
      ></Column>

      <Column
        field="type_decision_name"
        header="Tên loại quyết định"
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
      <!-- <Column
        field="decision_name"
        header="File mẫu"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:100px;height:50px"
        bodyStyle="text-align:center;max-width:100px"
      >
        <template #body="item">
          <div>
            <div v-if="item.data.file_path">
              <a
                :href="basedomainURL + item.data.file_path"
                download
                class="w-full no-underline cursor-pointer text-900"
              >
                <div class="align-items-center flex">
                  <div>
                    <img
                      :src="
                        basedomainURL +
                        '/Portals/Image/file/' +
                        item.data.file_path.substring(
                          item.data.file_path.lastIndexOf('.') + 1
                        ) +
                        '.png'
                      "
                      style="width: 70px; height: 50px; object-fit: contain"
                      alt=""
                    />
                  </div>
                   
                </div>
              </a>
            </div>
            <div  v-else-if="item.data.file_path_sys">
              <a
                :href="basedomainURL + item.data.file_path_sys"
                download
                class="w-full no-underline cursor-pointer text-900"
              >
                <div class="align-items-center flex">
                  <div>
                    <img
                      :src="
                        basedomainURL +
                        '/Portals/Image/file/' +
                        item.data.file_path_sys.substring(
                          item.data.file_path_sys.lastIndexOf('.') + 1
                        ) +
                        '.png'
                      "
                      style="width: 70px; height: 50px; object-fit: contain"
                      alt=""
                    />
                  </div>
                   
                </div>
              </a>
            </div>
          </div>
        </template>
      </Column> -->
      <Column
        field="status"
        header="Trạng thái"
        headerStyle="text-align:center;max-width:150px;height:50px"
        bodyStyle="text-align:center;max-width:150px"
        class="align-items-center justify-content-center text-center"
      >
        <template #body="data">
          <Checkbox
            :disabled="
              !(
                store.state.user.is_super == true ||
                store.state.user.user_id == data.data.created_by ||
                (store.state.user.role_id == 'admin' &&
                  store.state.user.organization_id == data.data.organization_id)
              )
            "
            :binary="true"
            v-model="data.data.status"
            @click="onCheckBox(data.data, true, true)"
          /> </template
      ></Column>
      <Column
        field="organization_id"
        header="Hệ thống"
        headerStyle="text-align:center;max-width:125px;height:50px"
        bodyStyle="text-align:center;max-width:125px;;max-height:60px"
        class="align-items-center justify-content-center text-center"
      >
        <template #body="data">
          <div v-if="data.data.is_system == true">
            <i class="pi pi-check text-blue-400" style="font-size: 1.5rem"></i>
          </div>
          <div v-else></div>
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
              v-if="
                store.state.user.is_super == true ||
                store.state.user.user_id == Tem.data.created_by ||
                store.state.user.is_admin
              "
            ></Button>
            <Button
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-trash"
              @click="delTem(Tem.data)"
              v-tooltip.top="'Xóa'"
              v-if="
                store.state.user.is_super == true ||
                store.state.user.user_id == Tem.data.created_by ||
                (store.state.user.role_id == 'admin' &&
                  store.state.user.organization_id == Tem.data.organization_id)
              "
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

  <Dialog
    :header="headerDialog"
    v-model:visible="displayBasic"
    :style="{ width: '35vw' }"
    :closable="true"
    :modal="true"
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12">
          <label class="col-3 text-left p-0"
            >Loại quyết định <span class="redsao">(*)</span></label
          >
          <InputText
            v-model="type_decision.type_decision_name"
            spellcheck="false"
            class="col-9 ip36 px-2"
            :class="{
              'p-invalid': v$.type_decision_name.$invalid && submitted,
            }"
            :disabled="checkDisabled"
          />
        </div>
        <div style="display: flex" class="field col-12 md:col-12">
          <div class="col-3 text-left"></div>
          <small
            v-if="
              (v$.type_decision_name.$invalid && submitted) ||
              v$.type_decision_name.$pending.$response
            "
            class="col-9 p-error"
          >
            <span class="col-12 p-0">{{
              v$.type_decision_name.required.$message
                .replace("Value", "Tên loại quyết định")
                .replace("is required", "không được để trống")
            }}</span>
          </small>
        </div>
        <div class="col-12 field md:col-12 flex">
          <div class="field col-4 md:col-4 p-0 align-items-center flex">
            <div class="col-9 text-left p-0">STT</div>
            <InputNumber
              v-model="type_decision.is_order"
              class="col-3 ip36 p-0"
              :disabled="checkDisabled"
            />
          </div>
          <div class="field col-4 md:col-4 p-0 align-items-center flex">
            <div class="col-6 text-center p-0">Trạng thái</div>
            <InputSwitch
              v-model="type_decision.status"
              :disabled="checkDisabled"
            />
          </div>
          <div
            class="field col-4 md:col-4 p-0 align-items-center flex"
            v-if="store.getters.user.is_super"
          >
            <div class="col-6 text-center p-0">Hệ thống</div>
            <InputSwitch v-model="type_decision.is_system" />
          </div>
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-3 text-left p-0">Mẫu quyết định </label>
          <Dropdown
            :filter="true"
            v-model="type_decision.report_key"
            :options="listTypeContract"
            optionLabel="label"
            optionValue="value"
            optionGroupLabel="label" optionGroupChildren="items"
            class="col-9"
            panelClass="d-design-dropdown"
            placeholder="Chọn mẫu quyết định"
      
          />
        </div>
        <!-- <div class="col-12 p-0" v-if="listFilesS.filter((x) => x.is_system == true).length>0">
          <DataTable
            :value="listFilesS.filter((x) => x.is_system == true)"
            filterDisplay="menu"
            filterMode="lenient"
            scrollHeight="flex"
            :showGridlines="true"
            :paginator="false"
            :row-hover="true"
            columnResizeMode="fit"
          >
            <Column field="code" header="File mẫu hệ thống">
              <template #body="item">
                <div class="p-0 d-style-hover" style="width: 100%; border-radius: 10px">
                  <div class="w-full flex align-items-center">
                    <div class="flex w-full text-900">
                      <div
                        v-if="item.data.is_image"
                        class="align-items-center flex"
                      >
                        <Image
                          :src="basedomainURL + item.data.file_path"
                          alt=""
                          width="70"
                          height="50"
                          style="
                            object-fit: contain;
                            border: 1px solid #ccc;
                            width: 70px;
                            height: 50px;
                          "
                          preview
                          class="pr-2"
                        />
                        <div class="ml-2" style="word-break: break-all">
                          {{ item.data.file_name }}
                        </div>
                      </div>
                      <div v-else>
                        <a
                          :href="basedomainURL + item.data.file_path"
                          download
                          class="w-full no-underline cursor-pointer text-900"
                        >
                          <div class="align-items-center flex">
                            <div>
                              <img
                                :src="
                                  basedomainURL +
                                  '/Portals/Image/file/' +
                                  item.data.file_path.substring(
                                    item.data.file_path.lastIndexOf('.') + 1
                                  ) +
                                  '.png'
                                "
                                style="
                                  width: 70px;
                                  height: 50px;
                                  object-fit: contain;
                                "
                                alt=""
                              />
                            </div>
                            <div class="ml-2" style="word-break: break-all">
                              {{ item.data.file_name }}
                            </div>
                          </div>
                        </a>
                      </div>
                    </div>
                    <div
                      class="w-3rem align-items-center d-style-hover-1"
                      v-if="store.getters.user.is_super"
                    >
                      <Button
                        icon="pi pi-times"
                        class="p-button-rounded  bg-red-300 border-none"
                        @click="deleteFileH(item.data)"
                      />
                    </div>
                  </div>
                </div>
              </template>
            </Column>
          </DataTable>
        </div>
        <div class="col-12 p-0" v-if="listFilesS.filter((x) => x.is_system == false).length>0">
          <DataTable
            :value="listFilesS.filter((x) => x.is_system == false)"
            filterDisplay="menu"
            filterMode="lenient"
            scrollHeight="flex"
            :showGridlines="true"
            :paginator="false"
            :row-hover="true"
            columnResizeMode="fit"
          >
            <Column field="code" header="  File mẫu Đơn vị">
              <template #body="item">
                <div class="p-0 d-style-hover" style="width: 100%; border-radius: 10px">
                  <div class="w-full flex align-items-center">
                    <div class="flex w-full text-900">
                      <div
                        v-if="item.data.is_image"
                        class="align-items-center flex"
                      >
                        <Image
                          :src="basedomainURL + item.data.file_path"
                          alt=""
                          width="70"
                          height="50"
                          style="
                            object-fit: contain;
                            border: 1px solid #ccc;
                            width: 70px;
                            height: 50px;
                          "
                          preview
                          class="pr-2"
                        />
                        <div class="ml-2" style="word-break: break-all">
                          {{ item.data.file_name }}
                        </div>
                      </div>
                      <div v-else>
                        <a
                          :href="basedomainURL + item.data.file_path"
                          download
                          class="w-full no-underline cursor-pointer text-900"
                        >
                          <div class="align-items-center flex">
                            <div>
                              <img
                                :src="
                                  basedomainURL +
                                  '/Portals/Image/file/' +
                                  item.data.file_path.substring(
                                    item.data.file_path.lastIndexOf('.') + 1
                                  ) +
                                  '.png'
                                "
                                style="
                                  width: 70px;
                                  height: 50px;
                                  object-fit: contain;
                                "
                                alt=""
                              />
                            </div>
                            <div class="ml-2" style="word-break: break-all">
                              <div class="ml-2" style="word-break: break-all">
                          <div style="word-break: break-all">
                            {{ item.data.file_name }}
                          </div>
                          <div
                            v-if="store.getters.user.is_super"
                            style="
                              word-break: break-all;
                              font-size: 11px;
                              font-style: italic;
                            "
                          >
                            {{ item.data.organization_name }}
                          </div>
                        </div>
                            </div>
                          </div>
                        </a>
                      </div>
                    </div>
                    <div
                      class="w-3rem align-items-center d-style-hover-1"
                      v-if="
                    store.getters.user.organization_id == item.data.organization_id
                  "
                    >
                      <Button
                        icon="pi pi-times"
                        class="p-button-rounded  bg-red-300 border-none"
                        @click="deleteFileH(item.data)"
                      />
                    </div>
                  </div>
                </div>
              </template>
            </Column>
          </DataTable>
        </div>
        <div class="col-12 field   ">File mẫu</div>    
        <div class="w-full col-12 field  ">
          <FileUpload
            chooseLabel="Chọn File"
            :showUploadButton="false"
            :showCancelButton="false"
            :multiple="false"
            accept=".doc,.docx"
            :maxFileSize="524288000"
            @select="onUploadFile"
            @remove="removeFile"

            :fileLimit="1"

            :invalidFileSizeMessage="'{0}: Dung lượng File không được lớn hơn {1}'"
          >
            <template #empty>
              <p class="p-0 m-0 text-500">Kéo thả hoặc chọn File.</p>
            </template>
          </FileUpload>
        </div> -->
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
    