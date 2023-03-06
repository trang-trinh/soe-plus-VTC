<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";

import moment from "moment";
import {  encr,checkURL } from "../../util/function.js";
const router = inject("router");
//Khai báo
const cryoptojs = inject("cryptojs");
 
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const basedomainURL = baseURL;
const selectedShowss = ref();
const checkDelList = ref(false);
const isFirstShows = ref(false);
const rules = {
  title: {
    required,
  },
};
const first = ref(0);
const taskDateFilter = ref();

const menu_ID = ref();
const menu_IDNode = ref();
const menu_IDNodeADD = ref();

//Lọc theo ngày

const todayClick = () => {
  taskDateFilter.value = [];
  taskDateFilter.value.push(new Date());
};
const delDayClick = () => {
  taskDateFilter.value = [];
  options.value.start_date = null;
  options.value.end_date = null;
  loadData(true);
};
const onDayClick = () => {
  if (taskDateFilter.value == null) taskDateFilter.value = [];
  else {
    options.value.start_date = taskDateFilter.value[0];
    options.value.end_date = taskDateFilter.value[1];
    loadData(true);
  }
};
// Upload, remove file
let files = [];
const onUploadFile = (event) => {
  event.files.forEach((element) => {
    files.push(element);
  });
};
const removeFile = (event) => {
  files = files.filter((a) => a != event.file);
};
const deleteFileCode = (value) => {
  shows.value.path = null;
  shows.value.file_name = null;
  file = [];
};
//Lấy file ảnh
const chonanh = (id) => {
  document.getElementById(id).click();
};
let avts = [];
const handleFileUpload = (event) => {
  avts = event.target.files;
  var output = document.getElementById("logoLang");
  output.src = URL.createObjectURL(event.target.files[0]);
  output.onload = function () {
    URL.revokeObjectURL(output.src); // free memory
  };checkImg.value=true;

};
const delAvatar1 = () => {
  avts=[];
  var output = document.getElementById("logoLang");
  output.src = baseURL  + '/Portals/Image/noimg.jpg' ;
  checkImg.value=false;
};
const checkImg=ref(false);
//Xóa shows

const delShows = (shows) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá shows này không!",
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
          .delete(baseURL + "/api/shows_main/delete_shows", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: shows != null ? [shows.shows_id] : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá shows thành công!");
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
                title: "Thông báo!",
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                icon: "error",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
};

const filters = ref({
  global: { value: null, matchMode: FilterMatchMode.CONTAINS },
  title: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
});
//Phân trang dữ liệu
const onPage = (event) => {

  options.value.pagesize = event.rows;
  options.value.pageno = event.page;
  loadDataSQL();
};
const filterSQL = ref([]);
const isDynamicSQL = ref(false);
const loadDataSQL = () => {
  let data = {
    id: "shows_id",
    next: options.value.IsNext,
    sqlO: options.value.sort,
    Search: options.value.search,
    PageNo: options.value.pageno,
    PageSize: options.value.pagesize,
    fieldSQLS: filterSQL.value,
  };

  options.value.loading = true;
  axios
    .post(baseURL + "/api/SQL/Filtershows_main", data, config)
    .then((response) => {
      let dt = JSON.parse(response.data.data);

      let data = dt[0];

      if (data.length > 0) {
        data.forEach((element, i) => {
          element.is_order = i + 1;
          if (!element.created_date_show) element.created_date_show = null;
          element.created_date_show = moment(
            new Date(element.created_date)
          ).format("DD/MM/YYYY");
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
//Sort
const onSort = (event) => {
  first.value=0;
  options.value.pageno=0; 
  if( event.sortField==null){
  isDynamicSQL.value = false;
  loadData();
  }
  else{
  options.value.sort =
    event.sortField + (event.sortOrder == 1 ? " ASC" : " DESC");
  if (event.sortField != "shows_id") {
    options.value.sort +=
      ",shows_id " + (event.sortOrder == 1 ? " ASC" : " DESC");
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
  isDynamicSQL.value = true;
  loadData(true);
};
//DropDown

const onDropDown = (value) => {
  let data = {
    IntID: value.shows_id,
    TextID: value.shows_id + "",
    IntTrangthai: value.status,
    BitTrangthai: false,
  };
  axios
    .put(baseURL + "/api/shows_main/update_status", data, config)
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Sửa trạng thái thành công!");
        loadData(false);
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
//Checkbox
const onCheckBox = (value) => {
  let data = {
    IntID: value.shows_id,
    TextID: value.shows_id + "",
    IntTrangthai: 0,
    BitTrangthai: value.is_hot,
    check: true,
  };
  axios
    .put(baseURL + "/api/shows_main/update_ishot", data, config)
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật tin thành công!");
        loadData(false);
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
//Xóa nhiều
const deleteList = () => {
  let listId = new Array(selectedShowss.length);

  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xóa danh sách media này không!",
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
        selectedShowss.value.forEach((item) => {
          listId.push(item.shows_id);
        });
        axios
          .delete(baseURL + "/api/shows_main/delete_shows", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: listId != null ? listId : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá shows thành công!");
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
                title: "Thông báo!",
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                icon: "error",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
};
const delAvatar = () => {
  shows.value.image = null;
};
//Lấy file shows

const toast = useToast();
const isFirst = ref(true);
const datalists = ref();
const isSaveShows = ref(false);
const sttShows = ref(1);
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
  sort: "shows_id DESC",
  search: "",
  pageno: 0,
  pagesize: 20,
  datefilter: 1,
  loading: true,
  totalRecords: null,
});
const shows = ref({
  is_order: 1,
  title: "",
  des: "",
  contents: "",
  image: "",
  is_hot: false,
  IsLang: store.getters.langid,
  Menu_ID: store.state.idShows,
  shows_type: 0,
  key_words: "",
  IsWriter: "",
  start_date: "",
  end_date: null,
  status: false,
});
const v$ = useVuelidate(rules, shows);
const loaiTinTuc = ref([
  { name: "Shows", code: 0 },
  { name: "Thông báo", code: 1 },
]);
const options_status = ref([
  { name: "Chưa duyệt", code: 1 },
  { name: "Đã duyệt", code: 2 },
  { name: "Đã đóng", code: 3 },
  { name: "Không duyệt", code: 4 },
]);
const danhMuc = ref();
//METHOD
const displayDetails = ref(false);
const openDetails = (data) => {
  displayDetails.value = true;
  shows.value = data;
};
const closeDetails = () => {
  displayDetails.value = false;
  shows.value = {};
};
const checkFilter = ref(false);
const hideFilter = () => {
  if (!options.value.status != null) checkFilter.value = false;
};
const loadCount = () => {
   axios
    .post(
          baseURL + "/api/device_card/getData",
        {
          str: encr(
            JSON.stringify({
        proc: "shows_main_count",
        par: [
          { par: "user_id", va: store.getters.user.user_id },
          { par: "key_words", va: options.value.key_words },
          { par: "search", va: options.value.search },
          { par: "status", va: options.value.status },
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
        options.value.totalRecords = data[0].totalRecords;
        sttShows.value =data[0].totalRecords + 1;
      } else options.value.totalRecords = 0;
    })
    .catch((error) => {
      addLog({
        title: "Lỗi Console loadCount",
        controller: "SQLView.vue",
        logcontent: error.message,
        loai: 2,
      });
    });
};
const resetRelate = () => {
  shows.value.relate_id = [];
};
const saveShows = (isFormValid) => {
  submitted.value = true;
  if (!isFormValid) {
    return;
  }
  if(shows.value.title.length>=500){
      swal.fire({
          title: "Thông báo",
          text: "Tên Powerpoint không được vượt quá 500 kí tự!",
          icon: "error",
          confirmButtonText: "OK",
        });
      return
  }
  if (
    files.length == 0 &&
    (shows.value.file_name == null || shows.value.file_name == "")
  ) {
    swal.fire({
      title: "Thông báo!",
      text: "Chưa tải tệp tải lên!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
 
  let formData = new FormData();
  for (var i = 0; i < files.length; i++) {
    let file = files[i];
    formData.append("shows", file);
  }
  for (var i = 0; i < avts.length; i++) {
    let file = avts[i];
    formData.append("avatar", file);
  }
  if (shows.value.key_words_mode != null) {
    shows.value.key_words = shows.value.key_words_mode.toString();
  }
  avts = [];
  files = [];
  if (shows.value.organization_id == null) {
    if (store.getters.user.organization_id == 1 && store.getters.user.IsSupper)
      shows.value.organization_id = null;
    else shows.value.organization_id = store.getters.user.organization_id;
  }
  // var menuid = Object.keys(menu_IDNodeADD.value);
  // shows.value.Menu_ID = menuid[0];
  formData.append("shows", JSON.stringify(shows.value));
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  if (!isSaveShows.value) {
    axios
      .post(baseURL + "/api/shows_main/add_shows", formData, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Thêm shows thành công!");
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
      .put(baseURL + "/api/shows_main/update_shows", formData, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa shows thành công!");
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
watch(selectedShowss, () => {
  if (selectedShowss.value.length > 0) {
    checkDelList.value = true;
  } else {
    checkDelList.value = false;
  }
});
const StartDateConvert = ref(new Date("1970/01/01"));
//Sửa bản ghi
const editShows = (data) => {
  submitted.value = false;

  displayBasic.value = true;
  files = [];
  if (data.key_words != null && data.key_words.length > 1) {
    if (!Array.isArray(data.key_words)) {
      if (!data.key_words_mode) data.key_words_mode = data.key_words.split(",");
    }
  }

  // menu_IDNodeADD.value[data.Menu_ID] = true;
  let contentFake = data.contents;
  data.contents = "";
  shows.value = data;
  headerDialog.value = "Sửa shows";
  isSaveShows.value = true;
  setTimeout(() => {
    shows.value.contents = contentFake;
    isFirstShows.value = true;
  }, "1500");
};
//Hiển thị dialog
const headerDialog = ref();
const displayBasic = ref(false);
const removableSort = ref(false);
const refreshData = () => {
  options.value.status = null;
  options.value.search = null;
  removableSort.value = true;
  first.value=0;
  selectedShowss.value=[];
  options.value.pageno=0;
  filterSQL.value=[];
  filters.value={
  global: { value: null, matchMode: FilterMatchMode.CONTAINS },
  title: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
};
isDynamicSQL.value=false;
  loadData(true);
};
const openBasic = (str) => {
  // loadRelate();
  shows.value = {
    shows_type: 0,
    status: 1,
    is_order: sttShows.value,
    is_comment: true,
    is_file_upload: true,
    organization_id: store.getters.user.organization_id,
  };
  // menu_IDNodeADD.value = {};
  // menu_IDNodeADD.value[danhMucAdd.value[0].key] = true;
  files.value = [];
  submitted.value = false;
  headerDialog.value = str;
  isSaveShows.value = false;
  displayBasic.value = true;
};
const closeDialog = () => {
  isFirstShows.value = false;
  //loadRelate();
  displayBasic.value = false;
  loadData(false);
};
const listMenu = ref();
const relateNew = ref([]);
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
        proc: "shows_main_list",
        par: [
          { par: "user_id", va: store.getters.user.user_id },
         
          { par: "key_words", va: options.value.key_words },
          { par: "search", va: options.value.search },
          { par: "status", va: options.value.status },
          { par: "datefilter", va: options.value.datefilter },
          { par: "pageno", va: options.value.pageno },
          { par: "pagesize", va: options.value.pagesize },
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
        relateNew.value = [];
         
        data.forEach((element, i) => {
          element.is_order =
            options.value.pageno * options.value.pagesize + i + 1;
          if (element.start_date != null) {
            var date = element.start_date.split(" ");
            element.start_date = date[0];
          }
          if (element.end_date != null) {
            var date1 = element.end_date.split(" ");
            element.end_date = date1[0];
          }

          relateNew.value.push({
            name: element.title,
            code: element.shows_id,
          });

          if (!element.created_date_show) element.created_date_show = null;
          element.created_date_show = moment(
            new Date(element.created_date)
          ).format("DD/MM/YYYY");
        });

        Array.from(new Set(relateNew.value));
      
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
//Khai báo function
const toggleFilter = (event) => {
  filterButs.value.toggle(event);
};
const selectTree = () => {
  var menuid = Object.keys(menu_IDNode.value);
  menu_ID.value = menuid[0];
  loadData(true);
};
const filterIsHot = ref();
const filterTrangthai = ref();
const showFilter = ref(false);
const reFilterShows = () => {
  options.value.status = null;
  checkFilter.value = false;
  loadData(true);
  // filterTrangthai.value = null;
  // filterShows();
  // showFilter.value = false;
};
const filterShows = () => {
  checkFilter.value = true;
  showFilter.value = false;
  if ( options.value.status != null) {
    let filterS = {
      filterconstraints: [
        { value: options.value.status, matchMode: "equals" },
      ],
      filteroperator: "and",
      key: "status",
    };
    filterSQL.value.push(filterS);
  }
  loadDataSQL();
};
//Tìm kiếm
const searchShows = () => {
  loadDataSQL();
};
onMounted(() => {  
  if (!checkURL(window.location.pathname, store.getters.listModule)) {
     //router.back();
  }
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
    <div class="d-lang-header">
      <h3 class="d-module-title">
        <i class="pi pi-id-card"></i> Danh sách media ({{
          options.totalRecords
        }})
      </h3>
    </div>
    <Toolbar class="d-toolbar">
      <template #start>
        <span class="p-input-icon-left">
          <i class="pi pi-search" />
          <InputText
            v-model="options.search"
            @keyup.enter="searchShows()"
            
            type="text"
            spellcheck="false"
            placeholder="Tìm kiếm"
          />
          <Button
            :class="
              options.status != null && checkFilter
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
            ref="filterButs"
            appendTo="body"
            :showCloseIcon="false"
            id="overlay_panelS"
            style="width: 350px"
            :breakpoints="{ '960px': '20vw' }"
            @hide="hideFilter"
          >
            <div class="grid formgrid m-2">
              <div class="field col-12 md:col-12 flex align-items-center">
                <div class="col-4 p-0">Trạng thái:</div>
                <Dropdown
                  v-model="options.status"
                  :options="options_status"
                  optionLabel="name"
                  optionValue="code"
                  placeholder="Trạng thái"
                  class="col-8 p-0"
                />
              </div>
              <div class="col-12 field p-0">
                <Toolbar class="toolbar-filter">
                  <template #start>
                    <Button
                      @click="reFilterShows"
                      class="p-button-outlined"
                      label="Xóa"
                    ></Button>
                  </template>
                  <template #end>
                    <Button @click="filterShows" label="Lọc"></Button>
                  </template>
                </Toolbar>
              </div>
            </div>
          </OverlayPanel>
        </span>
        <!-- <TreeSelect
          style="margin-left: 24px; min-width: 200px"
          @change="selectTree()"
          v-model="menu_IDNode"
          :options="danhMuc"
          placeholder="Tất cả shows"
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
          @click="openBasic('Thêm mới')"
          label="Thêm mới"
          icon="pi pi-plus"
          class="mr-2"
        />
        <Button
          class="mr-2 p-button-outlined p-button-secondary"
          icon="pi pi-refresh"
          @click="refreshData()"
        />

    
      </template>
    </Toolbar>
    <div class="d-lang-table">
      <DataTable
        class="w-full p-datatable-sm e-sm"
        @page="onPage($event)"
        @filter="onFilter($event)"
        @sort="onSort($event)"
        v-model:filters="filters"
        filterDisplay="menu"
        filterMode="lenient"
        removableSort
        dataKey="shows_id"
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
        v-model:selection="selectedShowss"
        :pageLinkSize="options.pagesize"
        paginatorTemplate="FirstPageLink PrevPageLink PageLinks  NextPageLink LastPageLink    RowsPerPageDropdown"
        :rowsPerPageOptions="[20, 30, 50, 100, 200]"
      >
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
          <template #body="data">
            <div
              :style="
                data.data.status == 2
                  ? 'color:#689F38'
                  : data.data.status == 3
                  ? 'color:#FBC02D'
                  : data.data.status == 4
                  ? 'color:#D32F2F'
                  : ''
              "
            >
              {{ data.data.is_order }}
            </div>
          </template>
        </Column>
        <Column
          headerStyle="text-align:left;height:50px"
          bodyStyle="text-align:left;"
          field="title"
          header="Tiêu đề"
          :sortable="true"
        >
          <template #body="data">
            <div
              :style="
                data.data.status == 2
                  ? 'color:#689F38'
                  : data.data.status == 3
                  ? 'color:#FBC02D'
                  : data.data.status == 4
                  ? 'color:#D32F2F'
                  : ''
              "
            >
              {{ data.data.title }}
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
          headerStyle="text-align:center;max-width:120px;height:50px"
          bodyStyle="text-align:center;max-width:120px"
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
          headerStyle="text-align:center;max-width:160px;height:50px"
          bodyStyle="text-align:center;max-width:160px"
          field="created_date_show"
          header="Ngày tạo"
        >
          <template #body="data">
            <div
              :style="
                data.data.status == 2
                  ? 'color:#689F38'
                  : data.data.status == 3
                  ? 'color:#FBC02D'
                  : data.data.status == 4
                  ? 'color:#D32F2F'
                  : ''
              "
            >
              {{ data.data.created_date_show }}
            </div>
          </template>
        </Column>
        <Column
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;max-width:180px;height:50px"
          bodyStyle="text-align:center;max-width:180px"
          field="status"
          header="Trạng thái"
        >
          <template #body="data">
            <Dropdown
              @change="onDropDown(data.data)"
              class="col-11"
              v-model="data.data.status"
              :options="options_status"
              optionLabel="name"
              optionValue="code"
            >
              <template #value="slotProps">
                <div class="p-dropdown-car-value" v-if="slotProps.value">
                  <span v-if="slotProps.value == 1">Chưa duyệt</span>
                  <span v-if="slotProps.value == 2" style="color: #689f38"
                    >Đã duyệt</span
                  >
                  <span v-if="slotProps.value == 3" style="color: #fbc02d"
                    >Đã đóng</span
                  >
                  <span v-if="slotProps.value == 4" style="color: #d32f2f"
                    >Không duyệt</span
                  >
                </div>
                <span v-else>
                  {{ slotProps.placeholder }}
                </span>
              </template>
            </Dropdown>
          </template>
        </Column>
        <Column
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;max-width:150px;height:50px"
          bodyStyle="text-align:center;max-width:150px"
          header="Chức năng"
        >
          <template #body="data">
            <Button
              v-tooltip.top="'Chi tiết'"
              @click="openDetails(data.data)"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-info-circle"
              :style="
                data.data.status == 2
                  ? 'color:#689F38'
                  : data.data.status == 3
                  ? 'color:#FBC02D'
                  : data.data.status == 4
                  ? 'color:#D32F2F'
                  : ''
              "
            ></Button>
            <Button
              v-tooltip.top="'Sửa shows'"
              @click="editShows(data.data)"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-pencil"
              :style="
                data.data.status == 2
                  ? 'color:#689F38'
                  : data.data.status == 3
                  ? 'color:#FBC02D'
                  : data.data.status == 4
                  ? 'color:#D32F2F'
                  : ''
              "
            ></Button>
            <Button
              v-tooltip.top="'Xóa shows'"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-trash"
              :style="
                data.data.status == 2
                  ? 'color:#689F38'
                  : data.data.status == 3
                  ? 'color:#FBC02D'
                  : data.data.status == 4
                  ? 'color:#D32F2F'
                  : ''
              "
              @click="delShows(data.data)"
            ></Button>
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
  <Sidebar v-model:visible="displayDetails" style="width:70vw !important"  class="p-sidebar-lg" position="left"> 
<div class="grid formgrid m-2">
        <div class="col-12 flex p-0">
          <div class="format-center pr-2">
            <i class="pi pi-images pr-2" style="font-size: 2rem"></i>
          </div>
          <div class="col-11 p-0">
            <div class="w-full font-bold text-2xl pb-1">
              {{ shows.title }}
            </div>
            <div class="text-md text-600 flex">
              Ngày tạo:
              {{
                moment(new Date(shows.created_date)).format("HH:mm DD/MM/YYYY")
              }}
              | {{ shows.created_name }} |
              <div class="justify-content-center flex pr-2">
                <div>
                  <i class="pi pi-comment px-1" style="font-size: 16px"></i>
                </div>
                <div>
                  Bình luận ({{
                    shows.comment_count ? shows.comment_count : 0
                  }})
                </div>
              </div>
            </div>
          </div>
        </div>
        <div class="col-12 surface-400 p-0" style="height: calc(100vh - 100px)">
          <iframe
            :src="
              basedomainURL +
              '/Viewer/?title=' +
              shows.pathname +
              '&url=' +
              shows.path
            "
            height="100%"
            width="100%"
            title="Iframe Example"
          >
          </iframe>
        </div>
      </div>
</Sidebar>

  <!-- <Dialog
    class="p-dialog-unset"
    header="Chi tiết"
    v-model:visible="displayDetails"
    :maximizable="true"
    :style="{ width: '60vw' }"
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="col-12 flex p-0">
          <div class="format-center pr-2">
            <i class="pi pi-images pr-2" style="font-size: 2rem"></i>
          </div>
          <div class="col-11 p-0">
            <div class="w-full font-bold text-2xl pb-1">
              {{ shows.title }}
            </div>
            <div class="text-md text-600 flex">
              Ngày tạo:
              {{
                moment(new Date(shows.created_date)).format("HH:mm DD/MM/YYYY")
              }}
              | {{ shows.created_name }} |
              <div class="justify-content-center flex pr-2">
                <div>
                  <i class="pi pi-comment px-1" style="font-size: 16px"></i>
                </div>
                <div>
                  Bình luận ({{
                    shows.comment_count ? shows.comment_count : 0
                  }})
                </div>
              </div>
            </div>
          </div>
        </div>
        <div class="col-12 surface-400 p-0" style="height: 70vh">
          <iframe
            :src="
              basedomainURL +
              '/Viewer/?title=' +
              shows.pathname +
              '&url=' +
              shows.path
            "
            height="100%"
            width="100%"
            title="Iframe Example"
          >
          </iframe>
        </div>
      </div>
    </form>
    <template #footer>
      <Button @click="closeDetails" label="Đóng" icon="pi pi-times" autofocus />
    </template>
  </Dialog> -->
  <Dialog
    @hide="closeDialog"
    :header="headerDialog"
    v-model:visible="displayBasic"
    :maximizable="true"
    :style="{ width: '50vw' }"
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="col-12 p-0 field flex">
          <div class="col-9 p-0">
            <div class="col-12 p-0 flex align-items-center field">
              <label class="text-left w-8rem"
                >Tiêu đề <span class="redsao">(*)</span></label
              >
              <InputText
                style="width: calc(100% - 8rem)"
                spellcheck="false"
                v-model="shows.title"
              />
            </div>
            <div class="col-12 p-0 field flex">
              <div class="w-8rem"></div>
              <small
                v-if="
                  (v$.title.$invalid && submitted) ||
                  v$.title.$pending.$response
                "
                style="width: calc(100% - 8rem)"
              >
                <span style="color: red" class="w-full">{{
                  v$.title.required.$message
                    .replace("Value", "Tiêu đề")
                    .replace("is required", "không được để trống")
                }}</span>
              </small>
            </div>
            <div class="col-12 p-0 flex align-items-center field">
              <label class="text-left w-8rem">Từ khóa</label>
              <Chips
                style="width: calc(100% - 8rem)"
                v-model="shows.key_words_mode"
                placeholder="Ấn Enter sau mỗi từ khóa!"
              />
            </div>
          </div>
          <div class="col-3 p-0 flex justify-content-center">
            <div class="col-9 format-center pr-0 pb-0 relative">
              <img
                v-tooltip.top="'Ảnh đại diện'"
                @click="chonanh('AnhLang')"
                class="inputanh p-0"
                id="logoLang"
                v-bind:src="
                  shows.image
                    ? basedomainURL + shows.image
                    : basedomainURL + '/Portals/Image/noimg.jpg'
                "
              />
              <Button
                style="
                  right: -5px !important;
                  width: 20px !important;
                  height: 20px !important;
                "
                v-if="checkImg"
                icon="pi pi-times"
                @click="delAvatar1"
                class="p-button-rounded absolute top-0 right-0"
              />
              <Button
                style="
                  right: -5px !important;
                  width: 20px !important;
                  height: 20px !important;
                "
                v-if="isSaveShows && shows.image"
                icon="pi pi-times"
                @click="delAvatar"
                class="p-button-rounded absolute top-0 right-0"
              />
            </div>
            <input
              class="ipnone"
              id="AnhLang"
              type="file"
              accept=".png,.jpg,.jpeg,.gif,.raw"
              @change="handleFileUpload"
            />
          </div>
        </div>
        <div class="flex field col-12 p-0">
          <label class="text-left w-8rem">File upload <span class="redsao">(*)</span></label>
          <div class="p-0" style="width: calc(100% - 8rem)">
            <FileUpload
              chooseLabel="Chọn File"
              :showUploadButton="false"
              :showCancelButton="false"
              :multiple="false"
              :fileLimit="1" 
              accept=".ppt, .pptx, .zip"
              :maxFileSize="524288000"
              @select="onUploadFile"
              @remove="removeFile"
              :invalidFileSizeMessage="'{0}: Dung lượng File không được lớn hơn {1}'"
            >
              <template #empty>
                <p class="p-0 m-0 text-500">
                  Chỉ hỗ trợ File dạng *.ppt, *.pptx, *.zip( Chuyển đổi từ PPT => HTML5)
                </p>
              </template>
            </FileUpload>
          </div>
        </div>
        <div class="col-12 p-0 flex field" v-if="shows.path">
          <label class="w-8rem"></label>
          <div class="p-0" style="width: calc(100% - 8rem)">
            <Toolbar class="w-full py-3">
              <template #start>
                <div class="flex">
                  <img
                    :src="basedomainURL + '/Portals/Image/mp4.png'"
                    style="object-fit: contain"
                    width="50"
                    height="50"
                  />
                  <span style="line-height: 50px"> {{ shows.file_name }}</span>
                </div>
              </template>
              <template #end>
                <Button
                  icon="pi pi-times"
                  class="p-button-rounded p-button-danger"
                  @click="deleteFileCode(item)"
                />
              </template>
            </Toolbar>
          </div>
        </div>
        <div class="field col-12 flex p-0">
          <div class="col-4 p-0 align-items-center flex">
            <div class="w-8rem">Trạng thái</div>
            <Dropdown
              style="width: calc(100% - 8rem)"
              v-model="shows.status"
              :options="options_status"
              optionLabel="name"
              optionValue="code"
              :disabled="!isSaveShows"
            />
          </div>
          <div class="col-1"></div>
          <div class="col-4 p-0 align-items-center flex">
            <div class="w-8rem pl-2">Bình luận</div>

            <InputSwitch class="col-3" v-model="shows.is_comment" />
          </div>
          <div class="col-2 p-0 align-items-center flex">
            <div class="w-8rem text-left">STT</div>
            <InputText
              style="width: calc(100% - 0rem)"
              v-model="shows.is_order"
              spellcheck="false"
            />
          </div>
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="closeDialog()"
        class="p-button-text"
      />

      <Button
        @click="saveShows(!v$.$invalid)"
        label="Lưu"
        icon="pi pi-check"
        autofocus
      />
    </template>
  </Dialog>
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
  margin: 0px 8px 0px 8px;
  height: calc(100vh - 150px);
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
.d-btn-delete {
  background-color: rgb(237, 114, 84);
  border: 1px solid rgb(214, 125, 125);
}
.d-btn-delete:hover {
  background-color: rgb(255, 0, 0);
  border: 1px solid rgb(214, 125, 125);
}
.d-btn-infor {
  background-color: rgb(56, 180, 187);
  border: 1px solid rgb(106, 173, 139);
}
.d-btn-infor:hover {
  background-color: rgb(125, 221, 150);
  border: 1px solid rgb(214, 125, 125);
}
.d-btn-edit:hover {
  background-color: rgb(63, 46, 252);
}
.inputanh {
  border: 1px solid #ccc;
  width: 100%;
  height: 100px;
  cursor: pointer;
  padding: 1px;
  object-fit: contain;
  background-color: #eeeeee;
}
.ipnone {
  display: none;
}

.d-avatar-shows {
  position: relative;
  width: 100%;
  height: 350px;
}
.d-avatar-shows img {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  object-fit: contain;
}
.toolbar-filter {
  border: unset;
  outline: unset;
  background-color: #fff;
  padding-bottom: 0px;
}
@keyframes p-progress-spinner-color {
  100%,
  0% {
    stroke: #858585 !important;
  }
  40% {
    stroke: #858585 !important;
  }
  66% {
    stroke: #858585 !important;
  }
  80%,
  90% {
    stroke: #858585 !important;
  }
}
</style>
<style lang="scss" scoped>
::v-deep(.p-dialog.p-dialog-unset) {
  .p-dialog-header {
    padding: 0 !important;
  }
}
</style>