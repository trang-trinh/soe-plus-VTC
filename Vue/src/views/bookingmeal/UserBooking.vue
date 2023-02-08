<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import ClassicEditor from "@ckeditor/ckeditor5-build-classic";
import moment from "moment";
import { encr } from "../../util/function.js";
const cryoptojs = inject("cryptojs");
//Khai báo
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const basedomainURL = baseURL;
const rules = {
  title: {
    required,
  },
};
const first = ref(1);
//Lọc
const treestatus = ref([
  { label: "Tất cả", value: null },
  { label: "Đăng ký cơm", value: true },
  { label: "Không đăng ký cơm", value: false },
]);
const showFilter = ref(false);
const filterTrangthai = ref();
const treedonvis = ref();
const selectCapcha = ref();
selectCapcha.value = {};
selectCapcha.value[store.getters.user.organization_id] = true;
const filterButs = ref();
const checkFilter = ref(false);
const toggleFilter = (event) => {
  filterButs.value.toggle(event);
};
const filterBooking = () => {
  let keys = Object.keys(selectCapcha.value);
  options.value.department_id = parseInt(keys[0]);
  checkFilter.value = true;
  loadData(true);
};
const refilterBooking = () => {
  checkFilter.value = false;
  selectCapcha.value[store.getters.user.organization_id] = true;
  options.value.department_id = store.getters.user.organization_id;
  filterTrangthai.value = null;
  loadData(true);
};
//Refresh
const onRefersh = () => {
  options.value = {
    IsNext: true,
    search: "",
    pageno: 1,
    pagesize: 20,
    user_id: store.getters.user.user_id,
    department_id: store.getters.user.organization_id,
  };
  checkFilter.value = false;
  selectCapcha.value[store.getters.user.organization_id] = true;
  options.value.department_id = store.getters.user.organization_id;
  filterTrangthai.value = null;
  first.value = 1;
  loadData(true);
};
//Phân trang dữ liệu
const isPaginator = ref(false);
const onPage = (event) => {
  if (event.rows != options.value.pagesize) {
    options.value.pagesize = event.rows;
  }

  options.value.pageno = event.page + 1;
  loadData();
};
const filterSQL = ref([]);
const isDynamicSQL = ref(false);
const filters = ref({
  global: { value: null, matchMode: FilterMatchMode.CONTAINS },
  title: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
  start_date: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.DATE_IS }],
  },
  is_hot: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.EQUALS }],
  },
  status: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.EQUALS }],
  },
});
//Sort
const onSort = (event) => {
  options.value.sort =
    event.sortField + (event.sortOrder == 1 ? " ASC" : " DESC");
  if (event.sortField != "video_id") {
    options.value.sort +=
      ",video_id " + (event.sortOrder == 1 ? " ASC" : " DESC");
  }
  isDynamicSQL.value = true;
  loadData();
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

  options.value.pageno = 1;
  first.value = 1;
  options.value.id = null;
  isDynamicSQL.value = true;
  loadData(true);
};

const toast = useToast();
const isFirst = ref(true);
const datalists = ref([]);
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
  sort: "video_id DESC",
  search: "",
  pageno: 1,
  pagesize: 20,
  loading: true,
  totalRecords: 0,
  department_id: store.getters.user.organization_id,
});
const video = ref({});
const booking = ref({
  listdates: [],
});
const user = ref({});
const v$ = useVuelidate(rules, video);
//METHOD
const loadCount = () => {
  axios
    .post(
        baseURL + "/api/BookingMeal/GetDataProc",
        {
          str: encr(JSON.stringify({
            proc: "booking_user_count",
        par: [
          { par: "user_id", va: store.getters.user.user_id },
          { par: "search", va: options.value.search },
          { par: "department_id", va: options.value.department_id },
          { par: "status", va: filterTrangthai.value },
        ],
          }), SecretKey, cryoptojs
          ).toString()
        },
        config
      )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        options.value.totalRecords = data[0].totalRecords;
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
//list
const loadData = (rf) => {
  if (rf) {
    options.value.loading = true;
    loadCount();
  }
  axios
    .post(
        baseURL + "/api/BookingMeal/GetDataProc",
        {
          str: encr(JSON.stringify({
            proc: "booking_user_list",
        par: [
          { par: "search", va: options.value.search },
          { par: "user_id", va: store.getters.user.user_id },
          { par: "pageno", va: options.value.pageno || 1 },
          { par: "pagesize", va: options.value.pagesize || 20 },
          { par: "department_id", va: options.value.department_id },
          { par: "status", va: filterTrangthai.value },
        ],
          }), SecretKey, cryoptojs
          ).toString()
        },
        config
      )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        data.forEach((element, i) => {
          element.is_sort =
            (options.value.pageno - 1) * options.value.pagesize + i + 1;
        });
        datalists.value = data;
        // var count = 1;
        // datalists.value.unshift({
        //   isTotal: true,
        //   full_name: data[0].organization_name,
        // });
        // for (let i = 0; i < data.length - 1; i++) {
        //   if (data[i].department_id !== data[i + 1].department_id) {
        //     let obj = {
        //       isTotal: true,
        //       full_name: data[i + 1].organization_name,
        //     };
        //     datalists.value.splice(i + count + 1, 0, obj);
        //     datalists.value.join();
        //     count++;
        //   }
        // }
      } else {
        datalists.value = [];
      }
      // options.value.pagesize =  options.value.pagesize + datalists.value.filter(x => x.isTotal == true).length;
      if (isFirst.value) isFirst.value = false;
      options.value.loading = false;
    })
    .catch((error) => {
      console.log(error);
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
const initTudien = () => {
  axios
    .post(
        baseURL + "/api/BookingMeal/GetDataProc",
        {
          str: encr(JSON.stringify({
            proc: "booking_meal_dictionary",
        par: [{ par: "user_id", va: store.getters.user.user_id }],
          }), SecretKey, cryoptojs
          ).toString()
        },
        config
      )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data[0].length > 0) {
        let obj = renderTreeDV(
          data[0],
          "organization_id",
          "organization_name",
          "phòng ban"
        );
        treedonvis.value = obj.arrtreeChils;
      }
    })
    .catch((error) => {});
};
//
const onCheckBox = (md) => {
  let ids = [md.user_id];
  let tts = [!md.is_booking];
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: "put",
    url: baseURL + "/api/BookingMeal/Update_statusUser",
    data: { ids: ids, tts: tts },
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật trạng thái thành công!");
        // loadRole();
        if (!md) selectedNodes.value = [];
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
};
//Khai báo function
const renderTreeDV = (data, id, name, title) => {
  let arrChils = [];
  let arrtreeChils = [];
  data
    .filter((x) => x.parent_id == null)
    .forEach((m, i) => {
      m.IsOrder = i + 1;
      let om = { key: m[id], data: m };
      const rechildren = (mm, pid) => {
        let dts = data.filter((x) => x.parent_id == pid);
        if (dts.length > 0) {
          if (!mm.children) mm.children = [];
          dts.forEach((em) => {
            let om1 = { key: em[id], data: em };
            rechildren(om1, em[id]);
            mm.children.push(om1);
          });
        }
      };
      rechildren(om, m[id]);
      arrChils.push(om);
      //
      om = { key: m[id], data: m[id], label: m[name] };
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
  return { arrChils: arrChils, arrtreeChils: arrtreeChils };
};
//Tìm kiếm
onMounted(() => {
  loadData(true);
  initTudien();
  return {};
});
</script>

<template>
  <div class="d-container">
    <div class="d-lang-header">
      <h3 class="d-module-title">
        <i class="pi pi-id-card"></i> Danh sách nhân sự đăng ký cơm ({{
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
            @keyup.enter="loadData(true)"
            type="text"
            spellcheck="false"
            placeholder="Tìm kiếm"
          />
          <Button
            :class="
              checkFilter ? 'ml-2' : 'ml-2 p-button-secondary p-button-outlined'
            "
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
          >
            <div class="grid formgrid m-2">
              <div class="field col-12 md:col-12 flex align-items-center">
                <div class="col-4 p-0">Phòng ban:</div>
                <TreeSelect
                  class="col-8 p-0"
                  v-model="selectCapcha"
                  :options="treedonvis"
                  :showClear="true"
                  :max-height="200"
                  placeholder="Chọn phòng ban"
                  optionLabel="organization_name"
                  optionValue="organization_id"
                >
                </TreeSelect>
              </div>
              <div class="field col-12 md:col-12 flex align-items-center">
                <div class="col-4 p-0">Trạng thái:</div>
                <Dropdown
                  class="col-8 p-0"
                  v-model="filterTrangthai"
                  :options="treestatus"
                  :max-height="200"
                  placeholder="Tất cả"
                  optionLabel="label"
                  optionValue="value"
                >
                </Dropdown>
              </div>
              <div class="col-12 field p-0">
                <Toolbar class="toolbar-filter">
                  <template #start>
                    <Button
                      @click="refilterBooking"
                      class="p-button-outlined"
                      label="Xóa"
                    ></Button>
                  </template>
                  <template #end>
                    <Button @click="filterBooking" label="Lọc"></Button>
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
          placeholder="Tất cả video"
        ></TreeSelect> -->
      </template>
      <template #end>
        <Button
          class="mr-2 p-button-outlined p-button-secondary"
          icon="pi pi-refresh"
          @click="onRefersh"
        />
      </template>
    </Toolbar>
    <div class="d-lang-table">
      <DataTable
        class="w-full p-datatable-sm e-sm"
        :value="datalists"
        dataKey="user_id"
        :showGridlines="true"
        :rowHover="true"
        currentPageReportTemplate=""
        responsiveLayout="scroll"
        :scrollable="true"
        scrollHeight="flex"
        rowGroupMode="subheader"
        groupRowsBy="organization_name"
        :rows="options.pagesize"
        :lazy="true"
        :loading="options.loading"
        :paginator="true"
        :rowsPerPageOptions="[20, 30, 50, 100, 200]"
        :totalRecords="options.totalRecords"
        @nodeSelect="onNodeSelect"
        @nodeUnselect="onNodeUnselect"
        @page="onPage($event)"
        @filter="onFilter($event)"
        @sort="onSort($event)"
        v-model:first="first"
      >
        <template #groupheader="slotProps">
          <i class="pi pi-building mr-2"></i>
          {{ slotProps.data.organization_name }}
        </template>
        <Column
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;max-width:70px;height:50px"
          bodyStyle="text-align:center;max-width:70px"
          field="is_order"
          header="STT"
        >
          <template #body="data">
            <div :style="data.data.status == 0?  'color:red': data.data.is_booking == 1 ? 'color:#0d89ec' : ''">
              {{ data.data.is_sort }}
            </div>
          </template>
        </Column>
        <Column
          headerStyle="text-align:center;height:50px;"
          bodyStyle="text-align:left;"
          field="full_name"
          header="Họ tên nhân sự"
        >
          <template #body="data">
            <div :style="data.data.status == 0?  'color:red': data.data.is_booking == 1 ? 'color:#0d89ec' : ''">
              {{ data.data.full_name }}
            </div>
          </template>
          <!-- <template #filter="{ filterModel }">
            <InputText
              type="text"
              v-model="filterModel.value"
              class="p-column-filter"
              placeholder="Tài khoản"
            />
          </template> -->
        </Column>
        <Column
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;height:50px;max-width:150px"
          bodyStyle="text-align:center;max-width:150px"
          field="position_name"
          header="Chức vụ"
        >
          <template #body="data">
            <div :style="data.data.status == 0?  'color:red': data.data.is_booking == 1 ? 'color:#0d89ec' : ''">
              {{ data.data.position_name }}
            </div>
          </template>
        </Column>
        <Column
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;height:50px;max-width:150px"
          bodyStyle="text-align:center;;max-width:150px"
          field="phone"
          header="Số điện thoại"
        >
          <template #body="data">
            <div :style="data.data.status == 0?  'color:red': data.data.is_booking == 1 ? 'color:#0d89ec' : ''">
              {{ data.data.phone }}
            </div>
          </template>
        </Column>
        <Column
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;height:50px;max-width:150px"
          bodyStyle="text-align:center;;max-width:150px"
          header="Đăng ký cơm"
          field="is_booking"
        >
          <template #body="md">
            <Checkbox
            v-if="md.data.status !== 0"
              :binary="true"
              v-model="md.data.is_booking"
              @click="onCheckBox(md.data)"
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
  </div>
</template>
<style scoped>
.p-calendar-w-btn {
  padding: 0px !important;
}
.icon-modules {
  width: 18px;
  height: 18px;
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
  height: 120px;
  cursor: pointer;
  padding: 1px;
  object-fit: contain;
  background-color: #eeeeee;
}
.ipnone {
  display: none;
}

.d-avatar-video {
  position: relative;
  width: 100%;
  height: 350px;
}
.d-avatar-video img {
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
::v-deep(.item-video) {
  .p-toolbar-group-left {
    flex: 11;
  }
  .p-toolbar-group-right {
    flex: 1;
  }
}
</style>