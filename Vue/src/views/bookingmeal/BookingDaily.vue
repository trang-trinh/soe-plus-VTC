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
const weekPickerFilter = ref();
const datalists = ref([]);
const listUserSelected = ref([]);
const displayDetail = ref(false);
const total_booking_days = ref(0);
const titleModal = ref("");
const totalrecords = ref(0);
var working_days = [];
const daysUser = ref([]);
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const toast = useToast();
const options = ref({
  Start_Date: null,
  End_Date: null,
  loading: true,
  search: "",
  PageNo: 1,
  PageSize: 40,
  user_id: store.getters.user.user_id,
  status: null,
  loading: true,
  department_id: store.getters.user.organization_id,
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
const onPage = (event) => {
  options.value.PageNo = event.page + 1;
  options.value.PageSize = event.rows;
  loadData();
};
//Lọc
const treestatus = ref([
  { label: "Tất cả", value: 0 },
  { label: "Ăn đầy đủ", value: 1 },
  { label: "Có cắt cơm", value: 2 },
]);
const filterTrangthai = ref(0);
const treedonvis = ref();
const selectCapcha = ref();
selectCapcha.value = {};
selectCapcha.value[store.getters.user.organization_id] = true;
const filterButs = ref();
const checkFilter = ref(false);
const checkFilterDate = ref(false);
const toggleFilter = (event) => {
  filterButs.value.toggle(event);
};
const dataTime = ref([]);
//loc date
const filterMonth = ref();
const taskDateFilter = ref();
const todayClick = () => {
  // taskDateFilter.value = [];
  // taskDateFilter.value.push(new Date());
  options.value.Start_Date = new Date();
  options.value.End_Date = null;
  loadData();
};
const toggleFilterMonth = (event) => {
  filterMonth.value.toggle(event);
};
const onFilterMonth = (check) => {
  checkFilterDate.value = true;
  taskDateFilter.value = [];
  if (check) {
    if (weekPickerFilter.value) {
      options.value.Start_Date = weekPickerFilter.value[0];
      options.value.End_Date = weekPickerFilter.value[1];
    }
  } else {
    if (monthPickerFilter.value) {
      weekPickerFilter.value = [];
      let day = new Date(
        monthPickerFilter.value.year,
        monthPickerFilter.value.month + 1,
        0
      ).getDate();
      options.value.Start_Date = new Date(
        monthPickerFilter.value.month +
          1 +
          "/01" +
          "/" +
          monthPickerFilter.value.year
      );
      options.value.End_Date = new Date(
        monthPickerFilter.value.month +
          1 +
          "/" +
          day +
          "/" +
          monthPickerFilter.value.year
      );
    }
  }
  loadData();
  filterMonth.value.hide();
};
const monthPickerFilter = ref();

const delDayClick = () => {
  taskDateFilter.value = [];
  options.value.Start_Date = null;
  options.value.End_Date = null;
  loadData(true);
};
const onDayClick = () => {
  if (monthPickerFilter.value) monthPickerFilter.value = null;
  if (weekPickerFilter.value) weekPickerFilter.value = null;
  if (taskDateFilter.value == null) taskDateFilter.value = [];
  else {
    options.value.Start_Date = taskDateFilter.value[0];
    options.value.End_Date = taskDateFilter.value[1];
    loadData();
  }
};
const onCleanFilterMonth = () => {
  if (weekPickerFilter.value) weekPickerFilter.value = null;
  if (monthPickerFilter.value) monthPickerFilter.value = null;
  // options.value.Start_Date = null;
  // options.value.End_Date = null;
  checkFilterDate.value = false;
  loadData(true);
};
const filterBooking = () => {
  let keys = Object.keys(selectCapcha.value);
  options.value.department_id = parseInt(keys[0]);
  checkFilter.value = true;
  loadData();
};
const refilterBooking = () => {
  checkFilter.value = false;
  selectCapcha.value[store.getters.user.organization_id] = true;
  options.value.department_id = store.getters.user.organization_id;
  filterTrangthai.value = 0;
  loadData();
};
const loadData = (f) => {
  total_booking_days.value = 0;
  var day_off = [];
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  if (f) {
    const today = new Date();
    const firstDay = getFirstDayOfWeek(today);
    const lastDay = new Date(firstDay);
    lastDay.setDate(lastDay.getDate() + 6);
    options.value.Start_Date = firstDay;
    options.value.End_Date = lastDay;
  }
  working_days.forEach((item) => {
    let dt = new Date(item);
    if (dt >= options.value.Start_Date && dt <= options.value.End_Date) {
      let dateString = moment(dt).format("MM/DD/yyyy");
      day_off.push(dateString);
    }
  });
  axios
    .post(
      baseURL + "/api/BookingMeal/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "booking_daily_list",
            par: [
              { par: "search", va: options.value.search },
              { par: "pageno", va: options.value.PageNo },
              { par: "pagesize", va: options.value.PageSize },
              { par: "start_date", va: options.value.Start_Date },
              { par: "end_date", va: options.value.End_Date },
              { par: "user_id", va: store.getters.user.user_id },
              { par: "department_id", va: options.value.department_id },
              { par: "status", va: filterTrangthai.value },
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
      let data2 = JSON.parse(response.data.data)[2];
      swal.close();
      if (data1.length > 0) {
        dataTime.value = data1[0].days.split(",");
      }
      if (data.length > 0) {
        data.forEach((item, index) => {
          let arrDays = [];
          item.status_booking = 0;
          item.booking_days = dataTime.value.length - day_off.length;
          if (item.listdate != null) {
            arrDays = item.listdate.split(",");
            item.booking_days = item.booking_days - arrDays.length;
            item.status_booking = 2;
            //so ngay an
            if (day_off.length + day_off.length == dataTime.value.length) {
              item.status_booking = 2;
              // 0 - khong cat; 1- cat it nhat 1 ngay ; 2 - cat tat ca
            }
            let is_trim = false;
            if (arrDays.length > 2) {
              arrDays = arrDays.slice(0, 2);
              is_trim = true;
            }
            arrDays.forEach((day, index) => {
              arrDays[index] = moment(day).format("DD-MM").toString();
            });
            item.dateString = arrDays.join(", ");
            if (is_trim) item.dateString += "...";
          }
          // item.booking_days =
          //   dataTime.value.length - day_off.length - arrDays.length;
          total_booking_days.value += item.booking_days;
          item.listdate = arrDays;
          item.is_order = index;
        });
        datalists.value = data;
      } else {
        datalists.value = [];
      }
      if (data2.length > 0) {
        totalrecords.value = data2[0].totalrecords;
      }
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
const initConfig = () => {
  axios
    .get(baseURL + "/api/BookingMeal/GetConfig", {
      headers: { Authorization: `Bearer ${store.getters.token}` },
    })
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        if (response.data.data) {
          working_days = response.data.data.working_days;
          loadData(true);
        }
      } else {
        loadData(true);
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
      }
    });
};
const showDetail = (md) => {
  titleModal.value = "Ngày cắt cơm - " + md.full_name;
  displayDetail.value = true;
  axios
    .post(
      baseURL + "/api/BookingMeal/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "booking_users_get_detail",
            par: [
              { par: "user_id", va: md.user_id },
              { par: "start_date", va: options.value.Start_Date },
              { par: "end_date", va: options.value.End_Date },
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
      if (data.length > 0) {
        daysUser.value = data;
      }
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
//refresh
const onRefresh = () => {
  options.value.search = null;
  checkFilter.value = false;
  checkFilterDate.value = false;
  selectCapcha.value[store.getters.user.organization_id] = true;
  options.value.department_id = store.getters.user.organization_id;
  filterTrangthai.value = 0;
  if (monthPickerFilter.value) monthPickerFilter.value = null;
  if (weekPickerFilter.value) weekPickerFilter.value = null;
  taskDateFilter.value = [];
  listUserSelected.value = [];
  loadData(true);
};
const closedisplayDetail = () => {
  displayDetail.value = false;
};
//load tu dien
const initTudien = () => {
  axios
    .post(
      baseURL + "/api/BookingMeal/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "booking_meal_dictionary",
            par: [{ par: "user_id", va: store.getters.user.user_id }],
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
// def
function getFirstDayOfWeek(d) {
  const date = new Date(d);
  const day = date.getDay();

  const diff = date.getDate() - day + (day === 0 ? -6 : 1);

  return new Date(date.setDate(diff));
}
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
//format number
onMounted(() => {
  initConfig();
  initTudien();
  return {};
});
</script>

<template>
  <div class="main-layout true flex-grow-1 p-2">
    <DataView
      class="w-full h-full e-sm"
      :lazy="true"
      :value="datalists"
      layout="grid"
      :loading="options.loading"
      :paginator="true"
      :rows="options.PageSize"
      :totalRecords="totalrecords"
      @page="onPage($event)"
      paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
      :rowsPerPageOptions="[40, 80, 200, 400]"
      currentPageReportTemplate=""
      responsiveLayout="scroll"
      :scrollable="true"
    >
      <template #header>
        <div class="w-full format-center">
          <h2 class="pt-0 mt-3 text-blue-700" v-if="options.End_Date != null">
            Bảng theo dõi nhân sự báo suất ăn từ ngày
            {{ moment(new Date(options.Start_Date)).format("DD/MM/YYYY") }} -
            {{ moment(new Date(options.End_Date)).format("DD/MM/YYYY") }}
            ({{ totalrecords }} nhân sự)
          </h2>
          <h2 class="pt-0 mt-3 text-blue-700" v-else>
            Bảng theo dõi nhân sự báo suất ăn ngày
            {{ moment(new Date(options.Start_Date)).format("DD/MM/YYYY") }}
            ({{ totalrecords }} nhân sự)
          </h2>
        </div>
        <div class="w-full format-center">
          <h3 class="text-700 pt-0 mt-0">
            (Tổng {{ total_booking_days }} suất)
          </h3>
        </div>
        <Toolbar class="w-full custoolbar">
          <template #start>
            <span class="p-input-icon-left">
              <i class="pi pi-search" />
              <InputText
                v-model="options.search"
                @keyup.enter="loadData()"
                type="text"
                spellcheck="false"
                placeholder="Tìm kiếm"
              />
              <Button
                :class="
                  checkFilter
                    ? 'ml-2'
                    : 'ml-2 p-button-secondary p-button-outlined'
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
                      placeholder="Chọn trạng thái"
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
          </template>

          <template #end>
            <Button
              class="mr-2 p-button-sm p-button-outlined p-button-secondary"
              @click="onRefresh()"
              icon="pi pi-refresh"
            />
            <Calendar
              placeholder="Lọc theo ngày"
              id="range"
              v-model="taskDateFilter"
              :showIcon="true"
              selectionMode="range"
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

            <Button
              type="button"
              :class="
                checkFilterDate
                  ? 'mx-2'
                  : 'mx-2 p-button-secondary p-button-outlined'
              "
              icon="pi pi-filter"
              @click="toggleFilterMonth($event)"
              aria-haspopup="true"
              aria-controls="overlay_panelMonth"
            />
          </template>
        </Toolbar>
      </template>
      <template #grid="slotProps">
        <div style="padding: 0; width: 12.5%" class="col-2 md:col-2 item-video">
          <Card
            class="no-paddcontent cursor-pointer"
            @click="
              slotProps.data.status_booking == 2
                ? showDetail(slotProps.data)
                : ''
            "
          >
            <template #title>
              <div
                class="col-12 flex"
                style="justify-content: center; position: relative"
              >
                <img
                  v-if="slotProps.data.avatar"
                  class="col-8 p-0"
                  style="border-radius: 5px"
                  :src="basedomainURL + slotProps.data.avatar"
                  @error="
                    $event.target.src =
                      basedomainURL + '/Portals/Image/nouser1.png'
                  "
                />
                <div
                  class="ava col-8"
                  v-if="!slotProps.data.avatar"
                  :style="{
                    background: bgColor[slotProps.data.is_order % 7],
                  }"
                >
                  <span
                    style="color: #fff; font-size: 1.5rem; font-weight: 500"
                    >{{
                      slotProps.data.full_name
                        .split(" ")
                        .at(-1)
                        .substring(0, 1)
                        .toUpperCase()
                    }}</span
                  >
                </div>
                <Button
                  v-tooltip.top="'Số suất đăng ký'"
                  :style="
                    slotProps.data.status_booking == 2
                      ? 'background: red;border:1px solid red'
                      : 'background:green;border:1px solid green'
                  "
                  v-bind:label="slotProps.data.booking_days"
                  class="p-button-rounded text-xs dot-status"
                />
              </div>
            </template>
            <template #content>
              <div
                class="col-12 text-center text-lg"
                :style="
                  slotProps.data.status_booking == 2
                    ? 'color: red'
                    : 'color:green'
                "
              >
                {{ slotProps.data.full_name }}
              </div>
              <div class="col-12 text-center text-lg" style="">
                {{ slotProps.data.dateString }}
              </div>
            </template>
          </Card>
        </div>
      </template>
    </DataView>
  </div>
  <Menu
    id="overlay_More"
    ref="menuButMores"
    :model="itemButMores"
    :popup="true"
  />
  <Dialog
    :header="titleModal"
    v-model:visible="displayDetail"
    :style="{ width: '760px' }"
    :maximizable="true"
    :closable="true"
    :autoZIndex="true"
  >
    <DataTable
      class="w-full p-datatable-sm e-sm"
      @nodeSelect="onNodeSelect"
      @nodeUnselect="onNodeUnselect"
      filterMode="lenient"
      dataKey="booking_date_id"
      responsiveLayout="scroll"
      :scrollable="true"
      scrollHeight="flex"
      :showGridlines="true"
      rows="20"
      :lazy="true"
      :value="daysUser"
      :loading="options.loading"
      :row-hover="true"
      v-model:first="first"
    >
      <Column
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:70px;height:50px"
        bodyStyle="text-align:center;max-width:70px"
        field="is_order"
        header="STT"
      >
      </Column>
      <Column
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;height:50px;max-width:150px"
        bodyStyle="text-align:center;max-width:150px"
        field="booking_date"
        header="Ngày cắt cơm"
      >
        <template #body="data">
          <div class="text-3line text-left">
            {{ moment(new Date(data.data.booking_date)).format("DD/MM/YYYY") }}
          </div>
        </template>
      </Column>
      <Column
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;height:50px"
        bodyStyle="text-align:center;"
        field="reason"
        header="Lý do"
      >
        <template #body="data">
          <div class="text-3line text-left">
            {{ data.data.reason }}
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
          <img src="../../assets/background/nodata.png" height="144" />
          <h3 class="m-1">Không có dữ liệu</h3>
        </div>
      </template>
    </DataTable>
    <template #footer>
      <Button
        label="Huỷ"
        icon="pi pi-times"
        @click="closedisplayDetail"
        class="p-button-raised p-button-secondary"
      />
    </template>
  </Dialog>
  <OverlayPanel
    ref="filterMonth"
    appendTo="body"
    class="p-0 m-0"
    :showCloseIcon="false"
    id="overlay_panelMonth"
    style="width: 300px"
  >
    <div class="grid formgrid m-0">
      <div class="flex field col-12 p-0">
        <Datepicker
          @closed="onFilterMonth(true)"
          selectText="Thực hiện"
          cancelText="Hủy"
          class="w-full"
          locale="vi"
          placeholder=" Lọc theo tuần"
          v-model="weekPickerFilter"
          weekPicker
        >
          <template #clear-icon>
            <Button
              @click="onCleanFilterMonth"
              icon="pi pi-times"
              class="p-button-rounded p-button-text"
            />
          </template>
          <template #input-icon>
            <Button class="mr-2 p-button-text" icon="pi pi-calendar" />
          </template>
        </Datepicker>
      </div>

      <div class="flex field col-12 p-0">
        <Datepicker
          @closed="onFilterMonth(false)"
          class="w-full"
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
      </div>
    </div>
  </OverlayPanel>
</template>
<style scoped>
.dot-status {
  height: unset !important;
  padding: 0.08rem 0.54rem !important;
  font-size: 10px !important;
  position: absolute;
  right: calc(50% - 38px);
  top: 0px;
}
.p-avatar {
  font-size: 1.5rem !important;
}
.card-content > .no-paddcontent {
  height: 100% !important;
}
.ava {
  width: 56px;
  height: 56px;
  background-color: #2196f3;
  border-radius: 10%;
  display: flex;
  justify-content: center;
  align-items: center;
}
.toolbar-filter {
  border: unset;
  outline: unset;
  background-color: #fff;
  padding-bottom: 0px;
}
</style>
<style lang="scss" scoped>
::v-deep(.p-dataview) {
  .p-dataview-content {
    background: #fff;
  }
}
::v-deep(.p-dataview) {
  .p-dataview-content {
    max-height: calc(100vh - 200px);
    // min-height: calc(100vh - 170px);
  }
}
::v-deep(.p-dataview-content) {
  .p-card-body {
    padding: 0 !important;
  }
  .p-card-title img {
    object-fit: cover;
    width: 56px;
    height: 56px;
  }
  .p-card {
    box-shadow: none !important;
    background: #fff;
    padding-top: 0.75rem;
    height: 100%;
  }
  .p-card:hover {
    background-color: #f0f8ff !important;
  }
  .p-card .p-card-title {
    margin-bottom: 0;
  }
}
::v-deep(.p-dataview) {
  .p-dataview-header {
    background: #fff;
  }
}
</style>