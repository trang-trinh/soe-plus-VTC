<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import { encr } from "../../util/function.js";
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
  emote_name: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
});
const rules = {
  emote_name: {
    required,
    $errors: [
      {
        $property: "emote_name",
        $validator: "required",
        $message: "Tên emote không được để trống!",
      },
    ],
  },
};
const opColor = ref();
let pcolor = "";
const toggleColor = (event, pc) => {
  opColor.value.toggle(event);
  pcolor = pc;
};
const changeColor = (color) => {
  emote.value.text_color = color.hex;
  if (!color.hex.includes("#")) opColor.value.hide();
};
//Thêm log

const addLog = (log) => {
  axios.post(baseURL + "/api/Proc/AddLog", log, config);
};
//Lấy số bản ghi
const loadCount = () => {
  axios
    .post(
      baseURL + "/api/DictionaryProc/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "ca_emotes_count",
            par: [{ par: "search", va: options.value.SearchText }],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        options.value.totalRecords = data[0].totalRecords;
        sttStamp.value = data[0].totalRecords + 1;
      }
    })
    .catch((error) => {
      addLog({
        title: "Lỗi Console loadCount",
        controller: "TemView.vue",
        log_content: error.message,
      });
    });
};
//Lấy dữ liệu emote
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
        baseURL + "/api/DictionaryProc/getData",
        {
          str: encr(
            JSON.stringify({
              proc: "ca_emotes_list",
              par: [
                { par: "pageno", va: options.value.PageNo },
                { par: "pagesize", va: options.value.PageSize },
                { par: "search", va: options.value.SearchText },
                { par: "status", va: options.value.Status },
              ],
            }),
            SecretKey,
            cryoptojs,
          ).toString(),
        },
        config,
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
        addLog({
          title: "Lỗi Console loadData",
          controller: "TemView.vue",
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

    options.value.id = datalists.value[datalists.value.length - 1].emote_id;
    options.value.IsNext = true;
  } else if (event.page < options.value.PageNo) {
    //Trang trước
    options.value.id = datalists.value[0].emote_id;
    options.value.IsNext = false;
  }
  options.value.PageNo = event.page;
  loadData(true);
};

const emote = ref({
  emote_name: "",
  emote_file: "",
  status: true,
  is_default: false,
  is_order: 1,
});
const selectedStamps = ref();
const submitted = ref(false);
const v$ = useVuelidate(rules, emote);
const isSaveTem = ref(false);
const datalists = ref();
const toast = useToast();
const basedomainURL = baseURL;
const checkDelList = ref(false);

const options = ref({
  IsNext: true,
  sort: "emote_id",
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
  emote.value = {
    emote_name: "",
    emote_file: "",
    status: true,
    is_default: false,
    is_order: sttStamp.value,
  };
  if (store.state.user.is_admin) {
    emote.value.organization_id = 0;
  } else {
    emote.value.organization_id = 1;
  }
  checkIsmain.value = false;
  isSaveTem.value = false;
  headerDialog.value = str;
  displayBasic.value = true;
};
const closeDialog = () => {
  emote.value = {
    emote_name: "",
    emote_file: "",
    status: true,
    is_default: false,
    is_order: 1,
  };
  files = [];
  displayBasic.value = false;
  loadData(true);
};
//Lấy file logo
const chonanh = (id) => {
  document.getElementById(id).click();
};
const handleFileUpload = (event) => {
  files = event.target.files;
  var output = document.getElementById("logoTem");
  output.src = URL.createObjectURL(event.target.files[0]);
  output.onload = function () {
    URL.revokeObjectURL(output.src); // free memory
  };
};
//Thêm bản ghi
let files = [];
const sttStamp = ref(1);
const saveData = (isFormValid) => {
  submitted.value = true;
  if (!isFormValid) {
    return;
  }
  let formData = new FormData();
  for (var i = 0; i < files.length; i++) {
    let file = files[i];
    formData.append("islogo", file);
  }
  formData.append("emote", JSON.stringify(emote.value));
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  if (!isSaveTem.value) {
    axios
      .post(baseURL + "/api/ca_emotes/Add_emote", formData, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Thêm emote thành công!");
          loadData(true);
          files = [];
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
      .put(baseURL + "/api/ca_emotes/Update_Stamp", formData, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa emote thành công!");
          loadData(true);
          files = [];
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
  emote.value = dataTem;

  if (emote.value.is_default) {
    checkIsmain.value = false;
  } else {
    checkIsmain.value = true;
  }
  headerDialog.value = "Sửa biểu cảm";
  isSaveTem.value = true;
  displayBasic.value = true;
  if (store.state.user.is_admin) {
    emote.value.organization_id = 0;
  } else {
    emote.value.organization_id = 1;
  }
};
//Xóa bản ghi
const delTem = (Tem) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá emote này không!",
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
        if (Tem.is_default) {
          toast.error("Không được xóa emote chính!");
          swal.close();
          return;
        } else {
          axios
            .delete(baseURL + "/api/ca_emotes/Delete_emote", {
              headers: { Authorization: `Bearer ${store.getters.token}` },
              data: Tem != null ? [Tem.emote_id] : 1,
            })
            .then((response) => {
              swal.close();
              if (response.data.err != "1") {
                swal.close();
                toast.success("Xoá emote thành công!");
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
      }
    });
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
    label: "Nhập Excel",
    icon: "pi pi-file-excel",
    command: (event) => {
      ImportExcel("ImportExcel");
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
        excelname: "DANH SÁCH TEM",
        proc: "ca_emote_listexport",
        par: [{ par: "search", va: options.value.SearchText }],
      },
      config,
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
          //window.open(baseURL + response.data.path);
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
//Sort
const onSort = (event) => {
  options.value.sort =
    event.sortField + (event.sortOrder == 1 ? " ASC" : " DESC");
  if (event.sortField == "STT") {
    options.value.sort = "is_order" + (event.sortOrder == 1 ? " ASC" : " DESC");
  }

  isDynamicSQL.value = true;
  loadDataSQL();
};
const filterSQL = ref([]);
const isFirst = ref(true);
const loadDataSQL = () => {
  let data = {
    sqlS: filterTrangthai.value != null ? filterTrangthai.value : null,
    sqlF: filterPhanloai.value != null ? filterPhanloai.value : null,
    sqlO: options.value.sort,
    Search: options.value.SearchText,
    PageNo: options.value.PageNo,
    PageSize: options.value.PageSize,
  };

  options.value.loading = true;
  axios
    .post(baseURL + "/api/SQL/Filter_Stamp", data, config)
    .then((response) => {
      let dt = JSON.parse(response.data.data);
      let data = dt[0];
      if (data.length > 0) {
        data.forEach((element, i) => {
          element.STT = options.value.PageNo * options.value.PageSize + i + 1;
        });
        if (options.value.sort == "is_order DESC") {
          data.forEach((element, i) => {
            element.STT =
              options.value.totalRecords -
              options.value.PageNo * options.value.PageSize -
              i;
          });
        }
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
//Tìm kiếm
const searchStamp = (event) => {
  if (event.code == "Enter") {
    options.value.loading = true;
    loadData(true);
  }
};
const refreshStamp = () => {
  styleObj.value = "";
  options.value.SearchText = "";
  filterPhanloai.value = "";
  filterTrangthai.value = "";
  options.value.loading = true;
  isDynamicSQL.value = false;
  loadData(true);
};
const onFilter = (event) => {
  filterSQL.value = [];

  for (const [key, value] of Object.entries(event.filters)) {
    if (key != "global") {
      let obj = {
        key: key != "emote_name" ? "emote_name" : key,
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
      IntID: value.emote_id,
      TextID: value.emote_id + "",
      IntTrangthai: 1,
      BitTrangthai: value.status,
    };
    axios
      .put(baseURL + "/api/ca_emotes/Update_TrangthaiStamp", data, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa trạng thái emote thành công!");
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
      IntID: value.emote_id,
      TextID: value.emote_id + "",
      BitMain: value.is_default,
    };
    axios
      .put(baseURL + "/api/ca_emotes/Update_DefaultStamp", data1, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa trạng thái emote thành công!");
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
      toast.error("Không được xóa emote mặc định!");
      checkD = true;
      return;
    }
  });
  if (!checkD) {
    swal
      .fire({
        title: "Thông báo",
        text: "Bạn có muốn xoá emote này không!",
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
            listId.push(item.emote_id);
          });
          axios
            .delete(baseURL + "/api/ca_emotes/Delete_emote", {
              headers: { Authorization: `Bearer ${store.getters.token}` },
              data: listId != null ? listId : 1,
            })
            .then((response) => {
              swal.close();
              if (response.data.err != "1") {
                swal.close();
                toast.success("Xoá emote thành công!");
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
const phanLoai = ref([
  { name: "Hệ thống", code: 0 },
  { name: "Đơn vị", code: 1 },
]);
const filterPhanloai = ref();
const filterTrangthai = ref();

const reFilterEmail = () => {
  filterPhanloai.value = null;
  filterTrangthai.value = null;
  filterFileds();
};
const filterFileds = () => {
  styleObj.value = style.value;
  isDynamicSQL.value = true;
  loadData(true);
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
const styleObj = ref();
const style = ref({
  "background-color": "#2196F3 !important",
  color: "#fff !important",
  " border": "1px solid #5ca7e3 !important",
});
const deleteemote_file = () => {
  files = [];
  emote.value.emote_file = null;
  files = "";
};
onMounted(() => {
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
    handleFileUpload,
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
  <div class="main-layout true flex-grow-1 p-2">
    <DataTable
      @page="onPage($event)"
      @sort="onSort($event)"
      @filter="onFilter($event)"
      v-model:filters="filters"
      filterDisplay="menu"
      filterMode="lenient"
      :filters="filters"
      :globalFilterFields="['emote_name']"
      :scrollable="true"
      scrollHeight="flex"
      :showGridlines="true"
      columnResizeMode="fit"
      :lazy="true"
      :totalRecords="options.totalRecords"
      :loading="options.loading"
      :reorderableColumns="true"
      :value="datalists"
      v-model:rows="options.PageSize"
      paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
      :rowsPerPageOptions="[20, 30, 50, 100, 200]"
      :paginator="true"
      dataKey="emote_id"
      responsiveLayout="scroll"
      v-model:selection="selectedStamps"
      :row-hover="true"
    >
      <template #header>
        <h3 class="module-title mt-0 ml-1 mb-2">
          <i class="pi pi-globe"></i> Danh sách biểu cảm ({{
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
                class="ml-2 p-button-outlined p-button-secondary"
                icon="pi pi-filter"
                @click="toggle"
                aria:haspopup="true"
                aria-controls="overlay_panel"
                :style="[styleObj]"
                v-tooltip="'Bộ lọc'"
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
                      Phân loại
                    </div>
                    <div class="col-8">
                      <Dropdown
                        class="col-12 p-0 m-0"
                        v-model="filterPhanloai"
                        :options="phanLoai"
                        optionLabel="name"
                        optionValue="code"
                        placeholder="Phân loại"
                      />
                    </div>
                  </div>

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
                        <Button
                          @click="filterFileds"
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
              v-if="checkDelList"
              @click="deleteList()"
              label="Xóa"
              icon="pi pi-trash"
              class="mr-2 p-button-danger"
            />
            <Button
              @click="openBasic('Thêm biểu cảm')"
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
          </template> </Toolbar
      ></template>

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
        headerStyle="text-align:center;max-width:120px;height:50px"
        bodyStyle="text-align:center;max-width:120px"
        :sortable="true"
      ></Column>

      <Column
        field="emote_name"
        header="Tên biểu cảm"
        :sortable="true"
        headerStyle="text-align:left;height:50px"
        bodyStyle="text-align:left"
      >
      </Column>

      <Column
        field="emote_file"
        header="Hình ảnh"
        headerStyle="text-align:center;max-width:150px;height:50px"
        bodyStyle="text-align:center;max-width:150px"
        class="align-items-center justify-content-center text-center"
      >
        <template #body="logo">
          <img
            style="object-fit: contain; border: unset; outline: unset"
            width="100"
            height="50"
            alt=" "
            v-bind:src="
              logo.data.emote_file
                ? basedomainURL + logo.data.emote_file
                : basedomainURL + '/Portals/emote_file/noimg.jpg'
            "
          />
        </template>
      </Column>
      <Column
        field="status"
        header="Hiển thị"
        headerStyle="text-align:center;max-width:150px;height:50px"
        bodyStyle="text-align:center;max-width:150px"
        class="align-items-center justify-content-center text-center"
      >
        <template #body="data">
          <Checkbox
            :binary="true"
            v-model="data.data.status"
            @click="onCheckBox(data.data, true, true)"
          /> </template
      ></Column>

      <Column
        field="organization_id"
        header="Hệ thống"
        headerStyle="text-align:center;max-width:125px;height:50px"
        bodyStyle="text-align:center;max-width:125px;"
        class="align-items-center justify-content-center text-center"
      >
        <template #body="data">
          <div v-if="data.data.organization_id == 0">
            <i
              class="pi pi-check text-blue-400"
              style="font-size: 1.5rem"
            ></i>
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
          <Button
            @click="editTem(Tem.data)"
            class="p-button-rounded p-button-secondary p-button-outlined mx-1"
            type="button"
            icon="pi pi-pencil"
            v-tooltip="'Sửa'"
          ></Button>
          <Button
            class="p-button-rounded p-button-secondary p-button-outlined mx-1"
            type="button"
            icon="pi pi-trash"
            @click="delTem(Tem.data)"
            v-tooltip="'Xóa'"
          ></Button>
        </template>
      </Column>
      <template #empty>
        <div
          class="align-items-center justify-content-center p-4 text-center m-auto"
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

  <Dialog
    :header="headerDialog"
    v-model:visible="displayBasic"
    :style="{ width: '40vw' }"
    :closable="false"
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12">
          <label class="col-3 text-left"
            >Tên biểu cảm <span class="redsao">(*)</span></label
          >
          <InputText
            v-model="emote.emote_name"
            spellcheck="false"
            class="col-9 ip36 px-2"
            :class="{ 'p-invalid': v$.emote_name.$invalid && submitted }"
          />
        </div>
        <div
          style="display: flex"
          class="field col-12 md:col-12"
        >
          <div class="col-3 text-left"></div>
          <small
            v-if="
              (v$.emote_name.$invalid && submitted) ||
              v$.emote_name.$pending.$response
            "
            class="col-9 p-error"
          >
            <span class="col-12 p-0">{{
              v$.emote_name.required.$message
                .replace("Value", "Tên emote")
                .replace("is required", "không được để trống")
            }}</span>
          </small>
        </div>
        <div
          style="display: flex"
          class="col-12 field md:col-12"
        >
          <div class="col-9">
            <div class="field col-12 md:col-12 p-0">
              <label class="col-4 text-left p-0">STT</label>
              <InputNumber
                v-model="emote.is_order"
                class="col-8 ip36 p-0"
              />
            </div>
            <div
              v-if="checkIsmain"
              class="field col-12 md:col-12 p-0"
            >
              <label
                style="vertical-align: text-bottom"
                class="col-4 text-left p-0"
                >Mặc định
              </label>
              <InputSwitch v-model="emote.is_default" />
            </div>
            <div class="col-12 field p-0">
              <div class="field col-12 md:col-12 p-0 flex">
                <div class="pb-2 col-4 p-0 line-height-4 field">Màu chữ</div>
                <Button
                  class="p-button-rounded p-button-outlined p-button-secondary col-6 field"
                  :style="{
                    backgroundColor: emote.text_color,
                    color: emote.text_color ? 'transparent' : '#333',
                    border: '1px solid #ccc',
                  }"
                  type="button"
                  icon="pi pi-palette"
                  @click="toggleColor($event, 'text_color')"
                />

                <OverlayPanel ref="opColor">
                  <ColorPicker
                    theme="dark"
                    @changeColor="changeColor"
                    :sucker-hide="true"
                  />
                </OverlayPanel>
              </div>
            </div>

            <div class="field col-12 md:col-12 p-0">
              <label
                style="vertical-align: text-bottom"
                class="col-4 text-left p-0"
                >Trạng thái
              </label>
              <InputSwitch v-model="emote.status" />
            </div>
          </div>
          <div
            class="col-3 align-items-center justify-content-center text-center"
          >
            <div class="field">
              <label>Hình ảnh</label>
              <div
                class="inputanh"
                @click="chonanh('AnhTem')"
              >
                <img
                  id="logoTem"
                  v-bind:src="
                    emote.emote_file
                      ? basedomainURL + emote.emote_file
                      : basedomainURL + '/Portals/Image/noimg.jpg'
                  "
                />
              </div>
              <input
                class="ipnone"
                id="AnhTem"
                type="file"
                accept="emote_file/*"
                @change="handleFileUpload"
              />
              <Button
                label="Xóa ảnh"
                class="p-1 m-3"
                @click="deleteemote_file()"
                v-if="isSaveTem"
              />
            </div>
          </div>
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="closeDialog"
        class="p-button-text"
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
