<script setup>
import { onMounted, inject, ref, watch, nextTick } from "vue";
import { encr } from "../../../../util/function";
import { useToast } from "vue-toastification";
import moment from "moment";
import { groupBy } from "lodash";

const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios");
const config = {
  headers: {
    Authorization: `Bearer ${store.getters.token}`,
  },
};
const toast = useToast();
const cryoptojs = inject("cryptojs");
const basedomainURL = baseURL;
const basefileURL = fileURL;
const PASS_KEY = SecretKey;

//Get arguments
const props = defineProps({
  profile_id: String,
});

//Decalre
const isFunction = ref(false);
const newDate = new Date();
const options = ref({
  loading: true,
  user_id: store.getters.user.user_id,
  search: "",
  pageNo: 1,
  pageSize: 25,
  total: 0,
  sort: "created_date desc",
  orderBy: "desc",
  view: 2,
  tempyear: newDate,
  year: newDate.getFullYear(),
  month: newDate.getMonth() + 1,
  week: 0,
  day: newDate.getDate(),
});
const isFirst = ref(true);
const datas = ref([]);

//Declare dictionary
const months = ref([
  { month: 1, name: "Tháng 1" },
  { month: 2, name: "Tháng 2" },
  { month: 3, name: "Tháng 3" },
  { month: 4, name: "Tháng 4" },
  { month: 5, name: "Tháng 5" },
  { month: 6, name: "Tháng 6" },
  { month: 7, name: "Tháng 7" },
  { month: 8, name: "Tháng 8" },
  { month: 9, name: "Tháng 9" },
  { month: 10, name: "Tháng 10" },
  { month: 11, name: "Tháng 11" },
  { month: 12, name: "Tháng 12" },
]);
//init
const initView6 = (ref) => {
  datas.value = [];
  if (ref) {
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
  }
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_myprofile_get_6",
            par: [{ par: "profile_id", va: props.profile_id }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      if (response != null && response.data != null) {
        let data = JSON.parse(response.data.data);
        if (data != null) {
          if (data[0] != null && data[0].length > 0) {
            datas.value = data[0];
          } else {
            datas.value = [];
          }
        }
      }
      if (isFirst.value) isFirst.value = false;
      swal.close();
      if (options.value.loading) options.value.loading = false;
    })
    .catch((error) => {
      swal.close();
      if (options.value.loading) options.value.loading = false;
      if (error && error.status === 401) {
        swal.fire({
          title: "Thông báo!",
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
        return;
      } else {
        swal.fire({
          title: "Thông báo!",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
        return;
      }
    });
};
onMounted(() => {
  nextTick(() => {
    initView6(true);
  });
});
</script>
<template>
  <div class="surface-100" :style="{ overflow: 'hidden', display: 'grid' }">
    <div class="box-table gridline-custom scrollable-both-custom">
      <table id="table-leave" class="table-custom">
        <thead class="thead-custom">
          <tr>
            <th
              class="sticky text-center"
              :style="{ left: '0', top: '0', width: '100px' }"
            >
              Năm
            </th>
            <th
              class="text-center"
              :style="{ width: '70px' }"
              v-for="(item, month_key) in months"
            >
              {{ item.name }}
            </th>
            <th
              class="text-center"
              :style="{ top: '0', width: '90px', backgroundColor: '#FFFEEC' }"
            >
              Phép năm
            </th>
            <th
              class="text-center"
              :style="{ top: '0', width: '90px', backgroundColor: '#FFFEEC' }"
            >
              Phép tồn
            </th>
            <th
              class="text-center"
              :style="{ top: '0', width: '90px', backgroundColor: '#FFFEEC' }"
            >
              Phép thưởng
            </th>
            <th
              class="text-center"
              :style="{ top: '0', width: '90px', backgroundColor: '#FFFEEC' }"
            >
              Thâm niên
            </th>
            <th
              class="text-center"
              :style="{
                top: '0',
                width: '90px',
                backgroundColor: '#F2FBE6',
              }"
            >
              TỔNG SỐ
            </th>
            <th
              class="text-center"
              :style="{
                top: '0',
                width: '90px',
                backgroundColor: '#EEFAF5',
              }"
            >
              ĐÃ NGHỈ
            </th>
            <th
              class="text-center"
              :style="{
                top: '0',
                width: '90px',
                backgroundColor: '#FDF2F0',
              }"
            >
              CÒN LẠI
            </th>
          </tr>
        </thead>
        <tbody
          class="tbody-custom"
          v-for="(user, user_key) in datas"
          :key="user_key"
        >
          <tr @click="openDialogLeaveProfile(user)" class="hover">
            <td
              class="sticky"
              :style="{
                left: '0',
                width: '100px',
                background: '#f8f9fa',
                textAlign: 'center',
              }"
            >
              {{ user.year }}
            </td>
            <td
              class="text-center"
              :style="{
                width: '90px',
                backgroundColor: '#fff',
              }"
              v-for="(item, month_key) in months"
            >
              <span> {{ user["month" + item.month] }}</span>
            </td>
            <td
              class="text-center"
              :style="{
                width: '150px',
                backgroundColor: '#FFFEEC',
              }"
            >
              <span> {{ user.leaveYear }}</span>
            </td>
            <td
              class="text-center"
              :style="{
                width: '150px',
                backgroundColor: '#FFFEEC',
              }"
            >
              <span> {{ user.leaveInventory }}</span>
            </td>
            <td
              class="text-center"
              :style="{
                width: '150px',
                backgroundColor: '#FFFEEC',
              }"
            >
              <span> {{ user.leaveBonus }}</span>
            </td>
            <td
              class="text-center"
              :style="{
                width: '150px',
                backgroundColor: '#FFFEEC',
              }"
            >
              <span> {{ user.leaveSeniority }}</span>
            </td>
            <td
              class="text-center"
              :style="{
                width: '150px',
                backgroundColor: '#fff',
                backgroundColor: '#F2FBE6',
              }"
            >
              <b>{{ user.total }}</b>
            </td>
            <td
              class="text-center"
              :style="{
                width: '150px',
                backgroundColor: '#fff',
                backgroundColor: '#EEFAF5',
              }"
            >
              <b> {{ user.leaveAll }}</b>
            </td>
            <td
              class="text-center"
              :style="{
                width: '150px',
                backgroundColor: '#fff',
                backgroundColor: '#FDF2F0',
              }"
            >
              <b> {{ user.leaveRemain }}</b>
            </td>
          </tr>
        </tbody>
      </table>
      <div
        v-if="!options.loading && datas.length == 0"
        class="align-items-center justify-content-center p-4 text-center m-auto"
        style="
          display: flex;
          width: 100%;
          height: calc(100vh - 230px);
          background-color: #fff;
        "
      >
        <div>
          <img src="../../../../assets/background/nodata.png" height="144" />
          <h3 class="m-1">Không có dữ liệu</h3>
        </div>
      </div>
    </div>
  </div>
</template>
<style scoped>
.box-table {
  height: calc(100vh - 230px) !important;
  background-color: #fff;
  overflow: auto;
}
.thead-custom > tr > th + th {
  border-left-width: 0;
}
.tbody-custom > tr + tr > td,
.tbody-custom > tr:first-child > td {
  border-top-width: 0;
}
.tbody-custom > tr > td + td {
  border-left-width: 0;
}
.table-custom {
  border-collapse: separate;
  border-spacing: 0;
  width: 100%;
  table-layout: fixed;
}
.thead-custom {
  position: sticky;
  top: 0;
  z-index: 2;
}
.thead-custom > tr > th {
  padding: 1rem 0.5rem;
  border: 1px solid #e9ecef;
  border-width: 0 0 1px 0;
  font-weight: 600;
  color: #495057;
  background: #f8f9fa;
  transition: box-shadow 0.2s;
}
.thead-custom > tr > th {
  border-width: 1px;
}
.tbody-custom > tr > td {
  border: 1px solid #e9ecef;
  border-width: 0 0 1px 0;
  padding: 1rem;
}
.tbody-custom > tr > td {
  border-width: 1px;
}

.scrollable-both-custom .thead-custom > tr > th,
.scrollable-both-custom .tbody-custom > tr > td,
.scrollable-both-custom .tfoot-custom > tr > td,
.p-datatable-scrollable-horizontal
  .p-datatable-thead
  > tr
  > th
  .p-datatable-scrollable-horizontal
  .p-datatable-tbody
  > tr
  > td,
.p-datatable-scrollable-horizontal .p-datatable-tfoot > tr > td {
  -webkit-box-flex: 0;
  -ms-flex: 0 0 auto;
  flex: 0 0 auto;
}
.sticky {
  display: sticky;
}
th.sticky {
  z-index: 2;
}
td.sticky {
  z-index: 1;
}
.btn-hover-true:hover {
  cursor: pointer;
  background: aliceblue;
  color: #000;
}
.btn-selected {
  background: aliceblue !important;
}
.icon-selected {
  transition: 0.5s ease;
  opacity: 1;
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  -ms-transform: translate(-50%, -50%);
  text-align: center;
}
.icon-selected > i {
  color: blue;
  font-size: 16px;
}
.style-0 {
  background-color: #52be80;
  color: #fff;
}
.style-1 {
  background-color: #eb984e;
  color: #fff;
}
.style-2 {
  background-color: #af7ac5;
  color: #fff;
}
.style-3 {
  background-color: #ec7063;
  color: #fff;
}

th.isHoliday {
  background-color: #f1948a !important;
  color: #fff !important;
}
.isHoliday {
  background-color: #f1948a;
  color: #fff;
}

.form-group {
  display: grid;
  margin-bottom: 1rem;
  flex: 1;
}

.form-group > label {
  margin-bottom: 0.5rem;
}

.form-group .p-multiselect .p-multiselect-label,
.form-group .p-dropdown .p-dropdown-label,
.form-group .p-treeselect .p-treeselect-label {
  height: 100%;
  display: flex;
  align-items: center;
}
.p-ulchip {
  margin: 0;
  margin-top: 0.5rem;
  padding: 0;
  list-style: none;
}

.p-lichip {
  float: left;
  white-space: normal;
}
.hover2:hover {
  cursor: pointer;
  color: #2196f3;
}
.hover:hover td {
  background-color: aliceblue !important;
}
</style>
<style lang="scss" scoped>
::v-deep(.d-lang-table) {
  .p-datatable-thead .justify-content-center .p-column-header-content {
    justify-content: center !important;
  }

  .p-datatable-table {
    position: absolute;
  }

  .p-datatable-thead {
    position: sticky;
    top: 0;
    z-index: 1;
  }
}
::v-deep(.form-group) {
  .p-multiselect .p-multiselect-label,
  .p-dropdown .p-dropdown-label {
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
::v-deep(.table) {
  border-collapse: collapse;
  width: 100%;
  table-layout: fixed;
  tr th {
    background-color: #f8f9fa;
  }
}
::v-deep(.border) {
  tr th,
  tr td {
    border: solid 1px rgba(0, 0, 0, 0.2);
    padding: 0.5rem;
  }
}
</style>
