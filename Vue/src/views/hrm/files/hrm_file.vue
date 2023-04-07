<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import { encr, checkURL } from "../../../util/function.js";
import moment from "moment";
//Khai báo

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
const viewFile = (data)=>{
  updateView(data.file_id);

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
            proc: "hrm_file_count",
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
    .catch((error) => { });
};
//Lấy dữ liệu
const loadData = (rf) => {
  debugger
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
    options.value.type = filterType.value ? filterType.value.type : null;
    axios
      .post(
        baseURL + "/api/hrm_ca_SQL/getData",
        {
          str: encr(
            JSON.stringify({
              proc: "hrm_file_list1",
              par: [
                { par: "user_id", va: store.getters.user.user_id },
                { par: "search", va: options.value.search },
                { par: "type", va: options.value.type },
                { par: "pageno", va: options.value.PageNo },
                { par: "pagesize", va: options.value.PageSize },
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

const selectedStamps = ref();
const submitted = ref(false);
const isSaveTem = ref(false);
const datalists = ref();
const toast = useToast();
const basedomainURL = baseURL;
const checkDelList = ref(false);
const options = ref({
  IsNext: true,
  sort: "created_date",
  search: "",
  PageNo: 0,
  PageSize: 20,
  loading: true,
  totalRecords: null,
  type: null
});
var dataCol = [];
const chartDatapie = ref({
  labels: ["Pdf", "Ảnh", "Word, Excel", "Khác"],
  datasets: [
    {
      data: [],
      backgroundColor: ["#689F38", "#0086f0", "#9C27B0", "#FBC02D"],
      hoverBackgroundColor: ["#81C784", "#64B5F6", "#D382E1", "#ece484"],
    },
  ],
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
const loadTudien = () => {
  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_file_dictionary",
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
        dataCol[0] = data.find((x) => x.file_type == "Pdf")
          ? data.find((x) => x.file_type == "Pdf").count_type
          : 0;
        dataCol[1] = data.find((x) => x.file_type == "Image")
          ? data.find((x) => x.file_type == "Image").count_type
          : 0;
        dataCol[2] = data.find((x) => x.file_type == "Word")
          ? data.find((x) => x.file_type == "Word").count_type
          : 0;
        dataCol[3] = data.find((x) => x.file_type == "More")
          ? data.find((x) => x.file_type == "More").count_type
          : 0;
        chartDatapie.value.datasets[0].data = dataCol;
      }
      let data2 = JSON.parse(response.data.data)[1];
      if (data.length > 0) {
        total_file.value = data2[0].total_file;
      }
      let data3 = JSON.parse(response.data.data)[2];
      if (data.length > 0) {
        total_size.value = data2[0].total_size;
      }
    })
    .catch((error) => { });
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
watch(selectedStamps, () => {
  if(selectedStamps.value){
    goFile(selectedStamps.value);
  }
});
const op = ref();
const toggle = (event) => {
  op.value.toggle(event);
};
const formatBytes = (bytes, decimals = 2) => {
  if (bytes === 0) return "0 Bytes";

  const k = 1024;
  const dm = decimals < 0 ? 0 : decimals;
  const sizes = ["Bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"];

  const i = Math.floor(Math.log(bytes) / Math.log(k));

  return parseFloat((bytes / Math.pow(k, i)).toFixed(dm)) + " " + sizes[i];
};
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
                <div class="flex w-full p-3">
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
                </div>
              </div>
            </template>
            <template #end>
                <!-- <DataViewLayoutOptions v-model="layout" /> -->
                <div class="p-dataview-layout-options p-selectbutton p-buttonset">
                  <button class="p-button p-button-icon-only" :class="layout== 'list'?'p-highlight':''" type="button" @click="changeView(layout = 'list')">
                    <i class="pi pi-bars"></i>
                  </button>
                  <button class="p-button p-button-icon-only" :class="layout== 'grid'?'p-highlight':''"  type="button" @click="changeView(layout = 'grid')">
                    <i class="pi pi-th-large"></i>
                  </button>
                  <!-- <Button
                  class="p-button-outlined p-button-secondary"
                  icon="pi pi-bars"
                  @click="onRefresh"
                   />
                  <Button
                    class="p-button-outlined p-button-secondary"
                    icon="pi pi-th-large"
                    @click="onRefresh"
                  /> -->
                </div>
                <Button
                  class="mr-2 ml-2 p-button-outlined p-button-secondary"
                  icon="pi pi-refresh"
                  @click="onRefresh"
                />
              </template>
          </Toolbar>
          <DataTable
          v-if="layout== 'list'"
          class="w-full p-datatable-sm e-sm cursor-pointer"
          :value="datalists"
          v-model:filters="filters"
          :showGridlines="true"
          filterMode="lenient"
          :paginator="datalists && datalists.length > 20"
          :rows="20"
          filterDisplay="menu"
          selectionMode="single"
          paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
          :rowsPerPageOptions="[20, 30, 50, 100, 200]"
          :scrollable="true"
          scrollHeight="flex"
          responsiveLayout="scroll"
          v-model:selection="selectedStamps"
          :globalFilterFields="[
            'file_name'
          ]"
          v-model:first="first_module"
        >
        <!-- <DataView
          class=""
          :value="datalists"
          :layout="layout"
          paginator="false"
          :rows="layout == 'grid' ? '36' : '10'"
          responsiveLayout="scroll"
          :scrollable="false"
          paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
          v-model:first="first"
        > -->

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
            class="align-items-center justify-content-center text-center"></Column>
          <Column field="created_date" header="Ngày/ Người tạo"
            headerStyle="text-align:center;max-width:200px;min-width:200px;height:50px"
            bodyStyle="text-align:center;max-width:200px;min-width:200px;;max-height:60px"
            class="align-items-center justify-content-center text-center">
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
                " style="
                      background-color: #2196f3;
                      color: #ffffff;
                      width: 2rem;
                      height: 2rem;
                      font-size: 1rem !important;
                    " :style="{
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
            ref="target"
            class="col-12 p-0 overflow-y-auto grid-9"
            style="height: calc(100vh - 115px)"
          >
          <DataView
            v-if="layout== 'grid'"
            class="w-full h-full e-sm flex flex-column p-dataview-unset"
            :value="datalists"
            :layout="layout"
            :paginator="datalists.length > 20"
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
                            slotProps.data.is_filepath
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
                          font-semibold
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
                  <img src="../../assets/background/nodata.png" height="144" />
                  <h3 class="m-1">Không có dữ liệu</h3>
                </div>
              </template>
          </DataView>
          </div>
      </div>
      <div style="width: 320px !important; border-left: 1px solid rgba(0, 0, 0, 0.1);overflow: hidden;">
        <div v-if="!isDetail">
          <div class="header-bar w-full format-center" style="border-bottom: 1px solid rgba(0, 0, 0, 0.1);">
            <h3>Kho số hóa: {{ total_file }} files</h3>
          </div>
          <div class="body-right format-center">
            <Chart type="pie" style="width: 90% !important" :data="chartDatapie" :options="lightOptions" />
          </div>
          <div class=" format-center w-full ">
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
              {{ file_detail.is_type == 0 ? 'Hồ sơ' : file_detail.is_type == 1 ? 'Hợp đồng' : file_detail.is_type == 2 ? 'Đào tạo':''}}
            </div>
          </div>
          <div class="field col-12 font-bold text-lg pl-0 pb-3">Thông tin truy cập</div>
          <div class="scroll">
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
                  <span class="sign-date description">{{ item.position_name }} </span>
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
    header="Chi tiết"
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
.item-hover:hover{
  background-color: #f0f8ff!important;
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
.scroll {
  overflow: auto;
  max-height: calc(100vh - 408px);
  min-height: calc(100vh - 408px);
}

.text-bold {
  color: #000000;
}

.header-bar {
  background-color: #fff !important;
  height: 57px !important;
}

.body-content {
  background: #fff;
  min-height: calc(100vh - 70px);
  max-height: calc(100vh - 70px);
  overflow: auto;
}

.icon-modules {
  width: 16px;
  height: 16px;
}

.field {
  margin-bottom: 0.75rem;
}

.log-image {
  max-width: 90px;
  min-width: 90px;
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

  th,
  td {
    height: 50px;
    border: none !important;
    border-top: 1px solid #e9ecef !important;
    border-bottom: 1px solid #e9ecef !important;
  }

  th {
    background: #fff !important;
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
</style>