<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import { encr, checkURL } from "../../../util/function.js";
import moment from "moment";
//Khai báo
const router = inject("router");
const cryoptojs = inject("cryptojs");
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const isDynamicSQL = ref(false);
const isDetail = ref(false);
const file_detail = ref();
const data_log = ref();
const layout = ref("list");
const first = ref(0);
const list_users= ref([]);
const list_profiles= ref([]);
const isTopView = ref(false)
const list_types = ref([
  { img: "/Portals/file/pdf.png", label: "PDF", type: 1 },
  { img: "/Portals/file/png.png", label: "Ảnh", type: 2 },
  { img: "/Portals/file/docx.png", label: "Word, Excel", type: 3 },
  { img: "/Portals/file/044-file-43.png", label: "Khác", type: 4 },
]);
const filterType = ref();
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const filters = ref({
  global: { value: null, matchMode: FilterMatchMode.CONTAINS },
  file_name: {
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
const total_file = ref(0);
const total_size = ref(0);
const itemButMores = ref([
  {
    label: "Xem thông tin",
    icon: "pi pi-info-circle",
    command: (event) => {
      viewFile(file.value);
    },
  },
  {
    label: "Tải xuống",
    icon: "pi pi-download",
    command: (event) => {
      downloadFile(file.value);
    },
  },
]);
const downloadFile = (file)=>{
  var url = baseURL + file.file_path;
  var name = file.file_name || ("file_download"+ file.file_type);
  const a = document.createElement("a");
  a.href =
    basedomainURL +
    "/Viewer/DownloadFile?url=" +
    file.file_path +
    "&title=" +
    name;
  a.download = name;
  a.target = "_blank";
  a.click();
  a.remove();
}
const dataDetail = ref();
const ModalShowFile = ref(false);
const fileNamDetail = ref('File số hóa');
const viewFile = (data)=>{
  updateView(data.file_id);
  fileNamDetail.value = data.file_name;
  dataDetail.value = data;
  ModalShowFile.value = true;
}
const file = ref();
const menuButMores = ref();
const toggleMores = (event, u) => {
  file.value = u;
  menuButMores.value.toggle(event);
};
//Lấy số bản ghi
const loadCount = () => {
  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_file_count1",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
                { par: "search", va: options.value.search },
                { par: "type_files", va: type_files.value },
                { par: "profiles", va: profiles.value },
                { par: "users", va: users.value },
                { par: "start_date", va: options.value.start_date },
                { par: "end_date", va: options.value.end_date },
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
    .catch((error) => { });
};
//Lấy dữ liệu
const loadData = (rf, is_filter) => {

  if (options.value.users != null && options.value.users.length > 0) {
    users.value = options.value.users.map((x) => x["user_id"]).join(",");
    }
    else  users.value = null;
    if (options.value.profiles != null && options.value.profiles.length > 0) {
      profiles.value = options.value.profiles.map((x) => x["profile_id"]).join(",");
    } else profiles.value = null;
    if (options.value.type_files != null && options.value.type_files.length > 0) {
      type_files.value = options.value.type_files.map((x) => x["type"]).join(",");
    } else type_files.value = null;
  if(is_filter) loadTudien();
  if (rf) {
    if (isDynamicSQL.value) {
      loadDataSQL();
      return false;
    }
    if (rf) {
        loadCount();
    }
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
    axios
      .post(
        baseURL + "/api/hrm_ca_SQL/getData",
        {
          str: encr(
            JSON.stringify({
              proc: "hrm_file_list2",
              par: [
                { par: "user_id", va: store.getters.user.user_id },
                { par: "search", va: options.value.search },
                { par: "type_files", va: type_files.value },
                { par: "profiles", va: profiles.value },
                { par: "users", va: users.value },
                { par: "start_date", va: options.value.start_date },
                { par: "end_date", va: options.value.end_date },
                { par: "pageNo", va: options.value.PageNo },
                { par: "pageSize", va: options.value.PageSize },
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
        let data = JSON.parse(response.data.data)[0];
        if (isFirst.value) isFirst.value = false;
        if(data.length>0){
          data.forEach((item, i) => {
          item.STT = options.value.PageNo * options.value.PageSize + i + 1;
          item.file_name_grid = item.file_name;
          if (item.file_name.length > 30)
          item.file_name_grid = item.file_name.substring(0, 30) + "...";
          //context label
          item.capacityMB = formatBytes(item.file_size)
          item.labelContext =
          item.file_name +
          (item.full_name ? "\nNgười tạo: " + item.full_name : "") +
          "\nNgày sửa cuối: " +
          moment(new Date(item.modified_date || item.created_date)).format(
            "DD/MM/YYYY hh:mm"
          ) +
          (item.capacityMB
            ? ""
            : "\nSize: " + item.capacityMB);
        });
        if(isfilter.value == true ) isTopView.value = false;
          if(options.value.PageNo== 0 && data.length>6 && !isfilter.value){
            datatop.value = data.slice(0,4);
            datalists.value = data
            isTopView.value = true;
          }
          else{
            datalists.value = data;
          } 
        }
        else {
          datalists.value = [];
          isTopView.value = false;
        }
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
  debugger
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

const selectedStamps = ref();
const submitted = ref(false);
const isSaveTem = ref(false);
const datalists = ref();
const datatop = ref();
const toast = useToast();
const basedomainURL = baseURL;
const checkDelList = ref(false);
const options = ref({
  IsNext: true,
  sort: "created_date",
  search: "",
  PageNo: 0,
  PageSize: 50,
  loading: true,
  totalRecords: null,
  type: null
});
var dataCol = [];
const chartDatapie = ref({
  labels: ["Pdf", "Ảnh", "Word, Excel", "Khác"],
  // datasets: [
  //   {
  //     data: [],
  //     backgroundColor: ["#689F38", "#0086f0", "#9C27B0", "#FBC02D"],
  //     hoverBackgroundColor: ["#81C784", "#64B5F6", "#D382E1", "#ece484"],
  //   },
  // ],
});
const lightOptions = ref({
  plugins: {
    legend: {
      labels: {
        color: "#495057",
      },
    },
  },
});
//Hiển thị dialog
const headerDialog = ref();
const displayBasic = ref(false);

//Thêm bản ghi

const sttStamp = ref(1);
const checkIsmain = ref(true);
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
const itemclick = ref()
const goFile = (item) => {
  itemclick.value = item.file_id;
 // updateView(item.file_id);
  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_file_get1",
            par: [{ par: "file_id", va: item.file_id }],
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
        file_detail.value = data[0];
      }
      let data1 = JSON.parse(response.data.data)[1];
      if (data1.length > 0) {
        data_log.value = data1;
      }
      isDetail.value = true;
    })
    .catch((error) => { });
}
const updateView = (id) => {
  axios({
    method: "put",
    url: baseURL + "/api/HrmFile/Update_View",
    data: { file_id: id },
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })

  // axios
  // .put(baseURL + "/api/HrmFile/Update_View", id, config)
  // .then((response) => {
  // });
}
const displayChart = ref(true);
const loadTudien = () => {
  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_file_dictionary1",
            par: [
                { par: "user_id", va: store.getters.user.user_id },
                { par: "search", va: options.value.search },
                { par: "type_files", va: type_files.value },
                { par: "profiles", va: profiles.value },
                { par: "users", va: users.value },
                { par: "start_date", va: options.value.start_date },
                { par: "end_date", va: options.value.end_date },
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
      if (data[0].length > 0) {
        dataCol[0] = data[0].find((x) => x.file_type == "Pdf")
          ? data[0].find((x) => x.file_type == "Pdf").count_type
          : 0;
        dataCol[1] = data[0].find((x) => x.file_type == "Image")
          ? data[0].find((x) => x.file_type == "Image").count_type
          : 0;
        dataCol[2] = data[0].find((x) => x.file_type == "Word")
          ? data[0].find((x) => x.file_type == "Word").count_type
          : 0;
        dataCol[3] = data[0].find((x) => x.file_type == "More")
          ? data[0].find((x) => x.file_type == "More").count_type
          : 0;
        chartDatapie.value.datasets = [];
          displayChart.value= true;
          setTimeout(() => {
          lightOptions.value.plugins.legend.display = true;
          chartDatapie.value.datasets.push({
            data: [],
            backgroundColor: ["#EE7E79", "#83ECC6", "#84B7F9", "#F5CD7C"],
            hoverBackgroundColor: ["#de5e58", "#56e7b2", "#4c96f6", "#f2c05a"],
          });
          chartDatapie.value.datasets[0].data = dataCol;
        }, 100);          
      }
      else displayChart.value= false;
      if (data[1].length > 0) {
        total_file.value = data[1][0].total_file;
      }
      if (data[1].length > 0) {
        total_size.value = data[1][0].total_size;
      }
      if (data[2].length > 0) {
        list_users.value = data[2];
      }
      if (data[3].length > 0) {
        list_profiles.value = data[3];
      }
    })
    .catch((error) => { 
      debugger
    });
};
const first_module = ref(0);
const filterTrangthai = ref();
const item_hover = ref();
const hoverItem = (id)=>{
  item_hover.value = id;
}
const leaveItem = ()=>{
  item_hover.value = null;
}
const clearDetail = ()=>{
  selectedStamps.value = null;
  isDetail.value = false;
  itemclick.value = null;
}
const goProfile = (item) => {
  router.push({
    name: "profileinfo",
    params: { id: generateUUID()},
    query: { id: item.profile_id_key },
  });
};
const changeView = (item)=>{
  layout.value = item;
  options.value.PageNo= 0;
  if(item == 'grid'){
    options.value.PageSize= 54;
  } 
  else{
    options.value.PageSize= 50;
  } 
  loadData(true, true)
}
watch(selectedStamps, () => {
  if(selectedStamps.value){
    goFile(selectedStamps.value);
  }
});
// const op = ref();
// const toggle = (event) => {
//   op.value.toggle(event);
// };
const onRefresh = ()=>{
   options.value = {
  IsNext: true,
  sort: "created_date",
  search: "",
  PageNo: 0,
  PageSize: 50,
  loading: true,
  totalRecords: null,
  type: null,
  type_files: [],
  users: [],
  profiles: [],
  start_date: null,
  end_date: null,
  };
  layout.value = 'list';
  isfilter.value = false;
  loadData(true, true);
}
//filter
const opfilter = ref();
const isfilter =ref(false)
const toggleFilter = (event) => {
  opfilter.value.toggle(event);
};
const resetFilter = () => {
  options.value.type_files = [];
  options.value.users = [];
  options.value.profiles = [];
  options.value.start_date = null;
  options.value.end_date = null;
};
const removeFilter = (idx, array) => {
  array.splice(idx, 1);
};
const users = ref(null);
const profiles = ref(null);
const type_files = ref(null);
const filter = (event) => {
  opfilter.value.toggle(event);
  isfilter.value = true;
  loadData(true, true);
};
const formatBytes = (bytes, decimals = 2) => {
  if (bytes === 0) return "0 Bytes";

  const k = 1024;
  const dm = decimals < 0 ? 0 : decimals;
  const sizes = ["Bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"];

  const i = Math.floor(Math.log(bytes) / Math.log(k));

  return parseFloat((bytes / Math.pow(k, i)).toFixed(dm)) + " " + sizes[i];
};
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
onMounted(() => {
  loadData(true);
  loadTudien();
  return {};
});
</script>
<template>
  <div class="main-layout true flex-grow-1 p-2 pb-0 pr-0">
    <div class="flex body-content">
      <div class="flex-1" v-if="datalists">
        <Toolbar class="w-full custoolbar">
            <template #start>
              <div class="header-bar">
                <Toolbar class="outline-none surface-0 border-none pb-1 ml-3">
                <template #start>
                  <span class="p-input-icon-left">
                    <i class="pi pi-search" />
                    <InputText
                      @keyup.enter="loadData(true)"
                      v-model="options.search"
                      type="text"
                      spellcheck="false"
                      :placeholder="'Tìm kiếm'"
                    />
                  </span>
                  <Button
                    @click="toggleFilter($event)"
                    type="button"
                    class="ml-2 p-button-outlined p-button-secondary"
                    aria:haspopup="true"
                    aria-controls="overlay_panel"
                  >
                    <div>
                      <span class="mr-2"><i class="pi pi-filter"></i></span>
                      <span class="mr-2">Lọc dữ liệu</span>
                      <span><i class="pi pi-chevron-down"></i></span>
                    </div>
                  </Button>
                  <OverlayPanel
                    :showCloseIcon="false"
                    ref="opfilter"
                    appendTo="body"
                    class="p-0 m-0"
                    id="overlay_panel"
                    style="width: 400px"
                  >
                  <div class="grid formgrid m-0">
                    <div
                      class="col-12 md:col-12 p-0"
                      :style="{
                        minHeight: 'unset',
                        maxheight: 'calc(100vh - 300px)',
                        overflow: 'auto',
                      }"
                    > 
                    <div class="row">
                      <div class="col-12 md:col-12">
                        <div class="form-group">
                          <label>Loại</label>
                          <MultiSelect
                            :options="list_types"
                            :filter="true"
                            :showClear="true"
                            :editable="false"
                            v-model="options.type_files"
                            optionLabel="label"
                            placeholder="Chọn loại"
                            class="w-full limit-width"
                            style="min-height: 36px"
                            panelClass="d-design-dropdown"
                          >
                            <template #value="slotProps">
                              <ul
                                class="p-ulchip"
                                v-if="
                                  slotProps.value && slotProps.value.length > 0
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
                                        <span>{{ value.label }}</span>
                                      </div>
                                      <span
                                        tabindex="0"
                                        class="p-chip-remove-icon pi pi-times-circle format-flex-center"
                                        @click="
                                          removeFilter(
                                            index,
                                            options.type_files
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
                     <div class="col-12 md:col-12">
                      <div class="form-group">
                        <label>Nhân sự</label>
                        <MultiSelect
                          :options="list_profiles"
                          v-model="options.profiles"
                          :filter="true"
                          :showClear="true"
                          :editable="false"
                          optionLabel="profile_user_name"
                          placeholder="Chọn nhân sự"
                          class="w-full limit-width"
                          style="min-height: 36px"
                          panelClass="d-design-dropdown"
                        >
                          <template #value="slotProps">
                            <ul
                              class="p-ulchip"
                              v-if="
                                slotProps.value && slotProps.value.length > 0
                              "
                            >
                              <li
                                class="p-lichip"
                                v-for="(value, index) in slotProps.value"
                                :key="index"
                              >
                                <Chip
                                  :image="value.avatar"
                                  :label="value.profile_user_name"
                                  class="mr-2 mb-2 px-3 py-2"
                                >
                                  <div class="flex">
                                    <div class="format-flex-center">
                                      <Avatar
                                        v-bind:label="
                                          value.avatar
                                            ? ''
                                            : (
                                                value.profile_user_name ?? ''
                                              ).substring(0, 1)
                                        "
                                        v-bind:image="
                                          value.avatar
                                            ? basedomainURL + value.avatar
                                            : basedomainURL +
                                              '/Portals/Image/noimg.jpg'
                                        "
                                        :style="{
                                          background:
                                            bgColor[value.is_order % 7],
                                          color: '#ffffff',
                                          width: '2rem',
                                          height: '2rem',
                                        }"
                                        class="mr-2 text-avatar"
                                        size="xlarge"
                                        shape="circle"
                                      />
                                    </div>
                                    <div class="format-flex-center text-left">
                                      <span>{{ value.profile_user_name }}</span>
                                    </div>
                                    <span
                                      tabindex="0"
                                      class="p-chip-remove-icon pi pi-times-circle format-flex-center"
                                      @click="
                                        removeFilter(index, options.profiles);
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
                                      : slotProps.option.profile_user_name.substring(
                                          0,
                                          1
                                        )
                                  "
                                  v-bind:image="
                                    slotProps.option.avatar
                                      ? basedomainURL + slotProps.option.avatar
                                      : basedomainURL +
                                        '/Portals/Image/noimg.jpg'
                                  "
                                  :style="{
                                    background:
                                      bgColor[slotProps.option.is_order % 7],
                                    color: '#ffffff',
                                    width: '3rem',
                                    height: '3rem',
                                    fontSize: '1.4rem !important',
                                  }"
                                  class="text-avatar m-0"
                                  size="xlarge"
                                  shape="circle"
                                />
                              </div>
                              <div class="format-center text-left ml-3">
                                <div>
                                  <div class="mb-1">
                                    {{ slotProps.option.profile_user_name }}
                                  </div>
                                  <div class="description">
                                    <div>
                                      <span>{{
                                        slotProps.option.profile_code
                                      }}</span
                                      ><span
                                        v-if="slotProps.option.department_name"
                                      >
                                        |
                                        {{
                                          slotProps.option.department_name
                                        }}</span
                                      >
                                    </div>
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
                        <label>Người tạo</label>
                        <MultiSelect
                          :options="list_users"
                          v-model="options.users"
                          :filter="true"
                          :showClear="true"
                          :editable="false"
                          optionLabel="full_name"
                          placeholder="Chọn người tạo"
                          class="w-full limit-width"
                          style="min-height: 36px"
                          panelClass="d-design-dropdown"
                        >
                          <template #value="slotProps">
                            <ul
                              class="p-ulchip"
                              v-if="
                                slotProps.value && slotProps.value.length > 0
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
                                        :style="{
                                          background:
                                            bgColor[value.is_order % 7],
                                          color: '#ffffff',
                                          width: '2rem',
                                          height: '2rem',
                                        }"
                                        class="mr-2 text-avatar"
                                        size="xlarge"
                                        shape="circle"
                                      />
                                    </div>
                                    <div class="format-flex-center text-left">
                                      <span>{{ value.full_name }}</span>
                                    </div>
                                    <span
                                      tabindex="0"
                                      class="p-chip-remove-icon pi pi-times-circle format-flex-center"
                                      @click="
                                        removeFilter(index, options.users);
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
                                      : slotProps.option.profile_user_name.substring(
                                          0,
                                          1
                                        )
                                  "
                                  v-bind:image="
                                    slotProps.option.avatar
                                      ? basedomainURL + slotProps.option.avatar
                                      : basedomainURL +
                                        '/Portals/Image/noimg.jpg'
                                  "
                                  :style="{
                                    background:
                                      bgColor[slotProps.option.is_order % 7],
                                    color: '#ffffff',
                                    width: '3rem',
                                    height: '3rem',
                                    fontSize: '1.4rem !important',
                                  }"
                                  class="text-avatar m-0"
                                  size="xlarge"
                                  shape="circle"
                                />
                              </div>
                              <div class="format-center text-left ml-3">
                                <div>
                                  <div class="mb-1">
                                    {{ slotProps.option.full_name }}
                                  </div>
                                  <div class="description">
                                    <div>
                                      <span>{{
                                        slotProps.option.user_id
                                      }}</span
                                      ><span
                                        v-if="slotProps.option.department_name"
                                      >
                                        |
                                        {{
                                          slotProps.option.department_name
                                        }}</span
                                      >
                                    </div>
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
                      <div class="form-group m-0">
                        <label>Ngày tạo</label>
                      </div>
                    </div>
                    <div class="col-6 md:col-6">
                      <div class="form-group">
                        <Calendar
                          :showIcon="true"
                          class="ip36"
                          autocomplete="on"
                          inputId="time24"
                          v-model="options.start_date"
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
                          v-model="options.end_date"
                          placeholder="Đến ngày"
                        />
                      </div>
                    </div>
                    </div>
                    <div class="col-12 md:col-12 p-0">
                      <Toolbar
                          class="border-none surface-0 outline-none px-0 pb-0 w-full"
                        >
                          <template #start>
                            <Button
                              @click="resetFilter()"
                              class="p-button-outlined"
                              label="Bỏ chọn"
                            ></Button>
                          </template>
                          <template #end>
                            <Button @click="filter($event)" label="Lọc"></Button>
                          </template>
                        </Toolbar>
                      </div>
                    </div>
                    </div>                         
                  </OverlayPanel>
                </template>
                <template #end>
                </template>
              </Toolbar>
                <!-- <div class="flex w-full p-3">
                  <div class="w-15rem mr-2">
                    <Dropdown v-model="filterType" :options="list_types" optionLabel="label" placeholder="Kho dữ liệu"
                      class="w-full" showClear="true" @change="loadData(true)">
                      <template #value="slotProps">
                        <div class="flex align-items-center" v-if="slotProps.value">
                          <img class="icon-modules" v-bind:src="basedomainURL + slotProps.value.img" />
                          <div class="ml-2">
                            {{ slotProps.value.label }}
                          </div>
                        </div>
                        <span v-else>
                          {{ slotProps.placeholder }}
                        </span>
                      </template>
                      <template #option="slotProps">
                        <div class="country-item flex">
                          <img class="icon-modules" v-bind:src="basedomainURL + slotProps.option.img" />
                          <div style="margin-left: 5px">
                            {{ slotProps.option.label }}
                          </div>
                        </div>
                      </template>
                    </Dropdown>
                  </div>
                  <div class="w-15rem mr-2">
                    <div class="w-full flex">
                      <span class="w-full p-input-icon-left ">
                        <i class="pi pi-search" />
                        <InputText type="text" style="height:32px" v-model="options.search" spellcheck="false"
                          @keyup.enter="loadData(true)" placeholder="Tìm kiếm" />
                      </span>
                    </div>
                  </div>
                </div> -->
              </div>
            </template>
            <template #end>
                <!-- <DataViewLayoutOptions v-model="layout" /> -->
                <div class="p-dataview-layout-options p-selectbutton p-buttonset">
                  <button class="p-button p-button-icon-only" :class="layout== 'list'?'p-highlight':''" type="button" @click="changeView('list')">
                    <i class="pi pi-bars"></i>
                  </button>
                  <button class="p-button p-button-icon-only" :class="layout== 'grid'?'p-highlight':''"  type="button" @click="changeView('grid')">
                    <i class="pi pi-th-large"></i>
                  </button>
                </div>
                <Button
                  class="mr-2 ml-2 p-button-outlined p-button-secondary"
                  icon="pi pi-refresh"
                  @click="onRefresh()"
                />
              </template>
          </Toolbar>
          <div class="w-full" v-if="isTopView">
            <h3 class="ml-3 my-2">Gần đây</h3>
            <div class="flex">
              <div v-for="(item, index) in datatop" :key="index" class="col-3" >
               <div  @click="goFile(item)"
                  @mouseover="hoverItem(item.file_id)"
                  @mouseleave="leaveItem()"
                  v-on:dblclick="viewFile(item)"
                  :title="item.labelContext"
                class="m-2 p-2 cursor-pointer item-top-hover relative" style="background-color:#f1f6fc;border-radius: 15px;height: 95%;">
                <Button
                    v-show="item.file_id == item_hover"
                    icon="pi pi-ellipsis-h"
                    class="p-button-rounded p-button-text absolute btn-more"
                    @click="toggleMores($event, item)"
                    aria-haspopup="true"
                    style="top:6px !important"
                    aria-controls="overlay_More"
                  />
                <div class="format-center text-1line my-2 mr-4" style="">{{item.file_name }}</div>
                <div class="item-content">
                  <div class="mx-2 bg-white py-2">
                        <Image
                          v-if="item.is_image"
                          height="110"
                          class="w-full cursor-pointer"
                          v-bind:src="
                            item.file_path
                              ? basedomainURL + item.file_path
                              : basedomainURL + '/Portals/Image/noimg.jpg'
                          "
                        />
                        <img
                          v-else
                          class="w-full cursor-pointer"
                          style="height: 110px; object-fit: contain"
                          v-bind:src="
                            basedomainURL +
                            '/Portals/file/' +
                            item.file_type.replace('.','') +
                            '.png'
                          "
                          @error="
                            $event.target.src =
                              basedomainURL + '/Portals/Image/noimg.jpg'
                          "
                        />
                      </div>
                </div>
              </div>
              </div>
            </div>
          </div>
          <DataTable
          v-if="layout== 'list'"
          class="w-full p-datatable-sm e-sm cursor-pointer"
          :value="datalists"
          :class="isTopView?'over-scroll-list':'over-scroll'"
          v-model:filters="filters"
          :showGridlines="true"
          filterMode="lenient"
          :paginator="'true'"
          :rows="options.PageSize"
          filterDisplay="menu"
          selectionMode="single"
          paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
          :scrollable="true"
          scrollHeight="flex"
          responsiveLayout="scroll"
          v-model:selection="selectedStamps"
          pageLinkSize="4"
          :globalFilterFields="[
            'file_name'
          ]"
          v-model:first="first_module"
          v-on:dblclick="viewFile(selectedStamps)"
          @page="onPage($event)"
        >

          <Column field="file_type" class="align-items-center justify-content-center text-center"
            headerStyle="text-align:center;max-width:50px;min-width:50px;height:50px"
            bodyStyle="text-align:center;max-width:50px;min-width:50px">
            <template #body="{ data }">
              <img style="height: 90%; object-fit: contain" v-bind:src="
                basedomainURL + '/Portals/file/' + data.file_type + '.png'
              " @error="
                $event.target.src = basedomainURL + '/Portals/Image/noimg.jpg'
              " />
            </template>
          </Column>

          <Column field="file_name" header="Tên file số hóa" headerStyle="text-align:left;height:50px"
            bodyStyle="text-align:left;word-break:break-word">
          </Column>
          <Column field="file_size" header="Kích cỡ"
            headerStyle="text-align:center;max-width:200px;min-width:150px;height:50px"
            bodyStyle="text-align:center;max-width:200px;min-width:150px"
            class="align-items-center justify-content-center text-center">
            <template #body="{ data }">
              {{ formatBytes(data.file_size) }}
            </template>
          </Column>
          <Column field="profile_name" header="Nhân sự"
            headerStyle="text-align:center;max-width:200px;min-width:150px;height:50px"
            bodyStyle="text-align:center;max-width:200px;min-width:150px;"
            class="align-items-center justify-content-center text-center">
            <template #body="slotProps">
            <b @click="goProfile(slotProps.data)" class="hover">{{
              slotProps.data.profile_name
            }}</b>
          </template>
          </Column>
          <Column field="created_date" header="Ngày/ Người tạo"
            headerStyle="text-align:left;max-width:170px;min-width:170px;height:50px"
            bodyStyle="text-align:left;max-width:170px;min-width:170px;;max-height:60px"
            class="">
            <template #body="slotProps">
              <span class="mr-2">{{
                moment(new Date(slotProps.data.created_date)).format(
                  "DD/MM/YYYY "
                )
              }}</span>
              <div>
                <Avatar v-bind:label="
                  slotProps.data.avatar
                    ? ''
                    : slotProps.data.last_name.substring(0, 1)
                " v-bind:image="
                  slotProps.data.avatar
                    ? basedomainURL + slotProps.data.avatar
                    : basedomainURL + '/Portals/Image/noimg.jpg'
                " style="background-color: #2196f3;color: #ffffff;width: 2rem;height: 2rem;font-size: 1rem !important;" :style="{
                      background: bgColor[slotProps.data.is_order % 7],
                    }" class="text-avatar" size="xlarge" shape="circle" v-tooltip.top="slotProps.data.full_name" />
              </div>
            </template>
          </Column>
          <Column field="created_date" header=""
            headerStyle="text-align:center;max-width:60px;min-width:60px;height:50px"
            bodyStyle="text-align:center;max-width:60px;min-width:60px;;max-height:50px"
            class="align-items-center justify-content-center text-center">
            <template #body="slotProps">
              <Button
              icon="pi pi-ellipsis-h"
              class="p-button-text p-button-secondary ml-2"
              @click="toggleMores($event, slotProps.data)"
              aria-haspopup="true"
              aria-controls="overlay_More"
            />
              </template>
          </Column>
          <template #empty>
            <div class="
                align-items-center
                justify-content-center
                p-4
                text-center
                m-auto
              " v-if="!isFirst">
              <img src="../../../assets/background/nodata.png" height="144" />
              <h3 class="m-1">Không có dữ liệu</h3>
            </div>
          </template>
          </DataTable>
          <div
          v-if="layout== 'grid'"
            class="col-12 p-0 overflow-y-auto grid-9"
          >
          <div class="header-top">
            <span class="font-bold flex ml-5 text-lg">Danh sách files</span>
          </div>
          <DataView
            class="w-full h-full e-sm flex flex-column p-dataview-unset"
            :value="datalists"
            :class="isTopView?'over-scroll-grid':'over-scroll-grid-notop'"
            :layout="layout"
            :paginator="'true'"
            rows="36"
            responsiveLayout="scroll"
            :scrollable="false"
            paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
            v-model:first="first"
            >
            <template #grid="slotProps">
                <div
                  style="width: 100%"
                  class="md:col-2 col-2 card-content cursor-pointer relative"
                  :title="slotProps.data.labelContext"
                  @click="goFile(slotProps.data)"
                  @mouseover="hoverItem(slotProps.data.file_id)"
                  @mouseleave="leaveItem()"
                  v-on:dblclick="viewFile(slotProps.data)"
                >
                 <Button
                    v-show="slotProps.data.file_id == item_hover"
                    icon="pi pi-ellipsis-h"
                    class="p-button-rounded p-button-text absolute btn-more"
                    @click="toggleMores($event, slotProps.data)"
                    aria-haspopup="true"
                    style=""
                    aria-controls="overlay_More"
                  />
                  <Card class="no-paddcontent p-0 item-hover" :class="itemclick== slotProps.data.file_id? 'item-click':''">
                    <template #title>
                      <div class="grid-item">
                        <Image
                          v-if="slotProps.data.is_image"
                          height="110"
                          class="w-full cursor-pointer"
                          v-bind:src="
                            slotProps.data.file_path
                              ? basedomainURL + slotProps.data.file_path
                              : basedomainURL + '/Portals/Image/noimg.jpg'
                          "
                        />
                        <img
                          v-else
                          class="w-full cursor-pointer"
                          style="height: 110px; object-fit: contain"
                          v-bind:src="
                            basedomainURL +
                            '/Portals/file/' +
                            slotProps.data.file_type.replace('.','') +
                            '.png'
                          "
                          @error="
                            $event.target.src =
                              basedomainURL + '/Portals/Image/noimg.jpg'
                          "
                        />
                      </div>
                    </template>
                    <template #content>
                      <div
                        class="
                          format-center
                          mx-2
                          text-3line text-title
                          my-2
                        "
                      >
                        {{ slotProps.data.file_name_grid }}
                      </div>
                    </template>
                  </Card>
                </div>
              </template>
              <template #empty>
                <div
                  class="
                    align-items-center
                    justify-content-center
                    p-4
                    text-center
                  "
                >
                  <img src="../../../assets/background/nodata.png" height="144" />
                  <h3 class="m-1">Không có dữ liệu</h3>
                </div>
              </template>
          </DataView>
          </div>
      </div>
      <div style="width: 320px !important; border-left: 1px solid rgba(0, 0, 0, 0.1);overflow: hidden;">
        <div v-if="!isDetail">
          <div class="header-bar w-full format-center" style="border-bottom: 1px solid rgba(0, 0, 0, 0.1);">
            <h3>Kho số hóa: {{ total_file || 0}} files</h3>
          </div>
          <div class="body-right format-center" v-if="displayChart">
            <Chart type="pie" style="width: 90% !important" :data="chartDatapie" :options="lightOptions" />
          </div>
          <div class=" format-center w-full" v-if="displayChart">
            <h4>Tổng dung lượng: {{ formatBytes(total_size) }}</h4>
          </div>
        </div>
        <div v-else class="p-3">
          <div class="field col-12 pl-0 flex">
            <div class="col-8 font-bold text-lg">
              Thông tin chung
            </div>
            <div class="col-4 text-right">
              <i class="pi pi-times-circle text-3xl cursor-pointer" @click="clearDetail()"></i>
            </div>
          </div>
          <div class="field col-12 flex">
            <div class="col-3 p-0 flex" style="align-items:center">Người tạo: </div>
            <div class="col-9 p-0 flex">
              <Avatar v-bind:label="
                file_detail.avatar ? '' : file_detail.last_name.substring(0, 1)
              " v-bind:image="basedomainURL + file_detail.avatar" style="background-color: #2196f3;
                            color: #ffffff;
                            width: 2rem;
                            height: 2rem;
                          " :style="{
                            background: bgColor[file_detail.last_name.length % 7],
                          }" class="mr-2" size="xlarge" shape="circle" />
              <div class="text-bold">{{ file_detail.full_name }} </div>
            </div>
          </div>
          <div class="field col-12 flex">
            <div class="col-3 p-0 flex" style="align-items:center">Ngày tạo: </div>
            <div class="col-9 p-0 text-bold">{{ moment(new Date(file_detail.created_date)).format("DD/MM/YYYY ") }} </div>
          </div>
          <div class="field col-12 flex">
            <div class="col-3 p-0 flex" style="align-items:center">Kích thước: </div>
            <div class="col-9 p-0 text-bold">{{ formatBytes(file_detail.file_size) }} </div>
          </div>
          <div class="field col-12 flex">
            <div class="col-3 p-0 flex" style="align-items:center">Vị trí hồ sơ: </div>
            <div class="col-9 p-0 text-bold">
              {{ file_detail.type_name}}
            </div>
          </div>
          <div class="field col-12 font-bold text-lg pl-0 pb-3">Thông tin truy cập</div>
          <div class="scroll-right">
            <div v-for="(item, index) in data_log" :key="index" class="flex mb-3"
              :style="(index == data_log.length - 1) ? '' : 'border-bottom:2px solid #eee'">
              <div class="log-image">
                <div class="group-sign">
                  <div style="display: inline-block; position: relative; z-index: 1;">
                    <Avatar v-bind:label="
                      item.avatar ? '' : item.last_name.substring(0, 1)
                    " v-bind:image="basedomainURL + item.avatar" style="
                            background-color: #2196f3;
                            color: #ffffff;
                            width: 4rem;
                            height: 4rem;
                          " :style="{
                            background: bgColor[index % 7],
                          }" class="mr-2" size="xlarge" shape="circle" />
                  </div>
                  <span class="sign-date description" v-if="item.position_name">{{ item.position_name }} </span>
                </div>
              </div>
              <div class="log-detail">
                <div>
                  <h4 class="m-0 mb-2">{{ item.full_name }}</h4>
                  <div class="mt-2">Ngày xem: <span class="description">{{ moment(new
                    Date(item.created_date)).format("DD/MM/YYYY HH:mm") }}</span></div>
                  <div class="mt-2">IP: <span class="description">{{ item.created_ip }}</span></div>
                  <div class="mt-2">Thiết bị: <span class="description">{{ item.from_device }}</span></div>
                </div>
              </div>
            </div>

          </div>
        </div>
      </div>
    </div>
  </div>
  <Dialog
    v-model:visible="ModalShowFile"
    :header="fileNamDetail"
    :modal="true"
    :closable="true"
    :style="{ width: '70vw' }"
    :maximizable="true"
    :autoZIndex="true"
      >
        <div class="grid formgrid m-2 h-full">
          <div v-if="dataDetail" class="w-full format-center">
             <img
              v-if="
                'gif,jpeg,png,jpg,.gif,.jpeg,.png,.jpg'.includes(dataDetail.file_type.toLowerCase())
              "
              style="width: 100%; min-height: 66vh; height: 100%"
              class="w-full cursor-pointer"
              :src="
                dataDetail.file_path
                  ? basedomainURL + dataDetail.file_path
                  : basedomainURL + '/Portals/Image/noimg.jpg'
              "
            />
            <video
              v-if="
                'mp4,flv,mov,wmv,.mp4,.flv,.mov,.wmv'.includes(dataDetail.file_type.toLowerCase())
              "
              style="width: 100%; min-height: 66vh; height: 100%"
              controls
              :src="basedomainURL + dataDetail.file_path"
            ></video>
            <audio
              style="width: 100%; margin: 0px auto"
              controls
              v-if="
                'mp3,wma,aac,flac,alac,wav,.mp3,.wma,.aac,.flac,.alac,.wav'.includes(
                  dataDetail.file_type.toLowerCase()
                )
              "
            >
              <source :src="basedomainURL + dataDetail.file_path" />
            </audio>
            <iframe
              v-if="
                'pptx,ppt,doc,docx,xls,xlsx, pdf, txt, .pptx,.ppt,.doc,.docx,.xls,.xlsx,.pdf,.txt'.includes(
                  dataDetail.file_type.toLowerCase()
                )
              "
              allowfullscreen
              :src="
                basedomainURL +
                '/Viewer/?title=' +
                dataDetail.file_name +
                '&url=' +
                dataDetail.file_path
              "
              style="width: 100%; min-height: 66vh; height: 100%"
              title="Iframe Example"
            >
            </iframe>
          </div>
        </div>
      </Dialog>
  <Menu
    id="overlay_More"
    ref="menuButMores"
    :model="itemButMores"
    :popup="true"
  />
</template>
    
<style scoped>
  @import url(../profile/component/stylehrm.css);

  .header-top{
    height:50px; 
    border-top: 1px solid #e9ecef!important;
    border-bottom: 1px solid #e9ecef!important;
    display: flex;
    align-items: center;

  }
.item-hover:hover{
  background-color: #f0f8ff!important;
}
.item-top-hover:hover{
  background-color: #cce9ff!important;
}
.item-click{
  background-color: #cce9ff!important;
}
.btn-more{
  right:2px;
   top:2px; 
   border-radius: 50%;
   color:#607D8B;
  border:1px solid #607D8B;
  width: 22px !important;
  height:22px !important;
}
.scroll-right {
  overflow: auto;
  max-height: calc(100vh - 350px);
  min-height: calc(100vh - 350px);
}
.hover:hover {
  color: #0078d4;
}
.text-bold {
  color: #000000;
}

.header-bar {
  background-color: #fff !important;
  height: 57px !important;
  display:flex;
}

.body-content {
  background: #fff;
}
.over-scroll{
  min-height: calc(100vh - 150px);
  max-height: calc(100vh - 150px);
  overflow: auto;
}
.over-scroll-list{
  min-height: calc(100vh - 360px);
  max-height: calc(100vh - 360px);
  overflow: auto;
}
.over-scroll-grid{
  min-height: calc(100vh - 390px);
  max-height: calc(100vh - 390px);
  overflow: auto;
}
.over-scroll-grid-notop{
  min-height: calc(100vh - 200px);
  max-height: calc(100vh - 200px);
  overflow: auto;
}
.text-1line {
  text-overflow: ellipsis;
  overflow: hidden;
  column-gap: initial;
  -webkit-line-clamp: 1;
  display: -webkit-box;
  -webkit-box-orient: vertical;
}
.field {
  margin-bottom: 0.75rem;
}

.log-image {
  max-width: 50px;
  min-width: 50px;
  position: relative;
}

.group-sign {
  text-align: center;
  position: relative;
}

.sign-date {
  display: block;
  clear: both;
  padding: 5px;
  margin: .5em 0;
  /* background: #eee;
  border-radius: 40px; */
  position: absolute;
  top: 54px;
  width: 100%;
  z-index: 1;
  text-align: center;
  font-size: 11px;
}

.log-detail {
  position: relative;
  flex-grow: 1;
  min-height: 80px;
}

.description {
  color: #aaa;
  font-size: 12px;
}

.signuser-detail>div,
.log-detail>div {
  /* border: 1px solid #eee;
  border-radius: 10px; */
  /* padding: 10px 15px; */
  margin: 0 10px 10px;
}
</style>
<style lang="scss" scoped>
::v-deep(.p-datatable-wrapper) {

  th {
    height: 50px;
    border: none !important;
    border-top: 1px solid #e9ecef !important;
    border-bottom: 1px solid #e9ecef !important;
    background: #fff !important;
  }
}

::v-deep(.p-selectable-row) {

  td {
  height: 50px;
  border: none !important;
  border-top: 1px solid #e9ecef !important;
  border-bottom: 1px solid #e9ecef !important;
}

}
::v-deep(.p-datatable-header) {
  padding: 0px !important;
  background: #fff !important;
  border-width: 0px !important;
}
.p-toolbar{
  padding: 0px !important;
}
::v-deep(.grid-9) {
  .grid {
    grid-template-columns: repeat(9, 1fr);
  }
}
::v-deep(.p-dataview-content) {
  .grid {
    display: grid !important;
  }
}
::v-deep(.grid-item) {
  img {
    width: 100%;
    object-fit: contain;
  }
}

::v-deep(.p-card) {
    box-shadow: none!important;
    background: #fff;
    height: 100%;
  .p-card-body {
    padding: 0.5rem !important;
  }

  .p-card-title {
    margin-bottom: 0 !important;
  }
}

::v-deep(.vue-simple-context-menu) {
  .vue-simple-context-menu__item {
    min-width: 130px !important;
    font-size: 14px;
  }
}

::v-deep(.p-dataview) {
  .p-dataview-header {
    padding: 0 0 !important;
  }

  .p-breadcrumb {
    padding: 0 !important;
    border-left: 4px solid rgb(0 122 212) !important;
  }
}

::v-deep(.p-dataview-content) {
  .grid {
    display: grid !important;
  }
}
::v-deep(.form-group) {
  .p-multiselect .p-multiselect-label,
  .p-dropdown .p-dropdown-label,
  .p-treeselect .p-treeselect-label {
    height: 100%;
    display: flex;
    align-items: center;
  }
  .p-chip img {
    margin: 0;
  }
  .p-avatar-text {
    font-size: 1rem;
  }
}
</style>