<script setup>
import { onMounted, inject, ref, nextTick } from "vue";
import { encr } from "../../../../util/function";
import { useToast } from "vue-toastification";
import { groupBy } from "lodash";
import moment from "moment";

const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios");
const toast = useToast();
const cryoptojs = inject("cryptojs");
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const basedomainURL = baseURL;

//Get arguments
const props = defineProps({
  headerDialog: String,
  displayDialog: Boolean,
  closeDialog: Function,
  organization_id: String,
  year: Number,
  initData: Function,
});
const display = ref(props.displayDialog);

//Declare
const newdate = new Date();
const options = ref({
  search: "",
  tempyear: new Date(props.year, newdate.getMonth() + 1, newdate.getDay()),
});
const datas = ref([]);
//filter
const search = () => {
  initData(true);
};

///Function
const submitted = ref(false);
const saveModel = () => {
  submitted.value = true;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });

  let formData = new FormData();
  let data = [];
  datas.value.forEach((group) => {
    if (group.users && group.users.length > 0) {
      group.users.forEach((user) => {
        let obj = {
          profile_id: user.profile_id,
          leave: user.leave,
          leave_limit: user.leave_limit,
        };
        data.push(obj);
      });
    }
  });
  if (options.value.tempyear != null) {
    options.value.year = new Date(options.value.tempyear).getFullYear();
  }
  formData.append("year", options.value.year);
  formData.append("data", JSON.stringify(data));
  axios
    .put(baseURL + "/api/hrm_leave/update_lave_year", formData, config)
    .then((response) => {
      if (response.data.err === "1") {
        swal.fire({
          title: "Thông báo!",
          text: response.data.ms,
          icon: "error",
          confirmButtonText: "OK",
        });
        return;
      }
      swal.close();
      toast.success("Cập nhật thành công!");
      props.closeDialog();
      props.initData(true);
    })
    .catch((error) => {
      swal.close();
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
  if (submitted.value) submitted.value = true;
};
const changeLeave = (user) => {
  user.leave_limit = user.leave;
};

//init
const initData = (ref) => {
  if (ref) {
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
  }
  datas.value = [];
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_leave_year_list",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "search", va: options.value.search },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      if (response != null && response.data != null) {
        let data = response.data.data;
        if (data != null) {
          let tbs = JSON.parse(data);
          if (tbs[0] != null && tbs[0].length > 0) {
            var group = groupBy(tbs[0], "department_id");
            for (var k in group) {
              let obj = {
                department_id: k,
                department_name: group[k][0].department_name,
                users: group[k],
              };
              datas.value.push(obj);
            }
            var count = 1;
            datas.value.forEach((item) => {
              if (item.users != null && item.users.length > 0) {
                item.users.forEach((us) => {
                  us.stt = count++;
                });
              }
            });
          } else {
            datas.value = [];
          }
        }
      }
      swal.close();
    })
    .catch((error) => {
      swal.close();
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
  if (display.value) {
    nextTick(() => {
      initData(true);
    });
  }
});
</script>
<template>
  <Dialog
    :header="props.headerDialog"
    v-model:visible="display"
    :style="{ width: '40vw' }"
    :maximizable="true"
    :closable="true"
    style="z-index: 9000"
  >
    <Toolbar class="outline-none surface-0 border-none px-0">
      <template #start>
        <span class="p-input-icon-left mr-2">
          <i class="pi pi-search" />
          <InputText
            @keypress.enter="search()"
            v-model="options.search"
            type="text"
            spellcheck="false"
            placeholder="Tìm kiếm"
            class="ip36 input-search"
          />
        </span>
      </template>
      <template #end>
        <div class="form-group m-0">
          <Calendar
            v-model="options.tempyear"
            @date-select="goYear(options.tempyear)"
            :showIcon="true"
            :manualInput="false"
            inputId="yearpicker"
            :showOnFocus="false"
            view="year"
            dateFormat="'Năm' yy"
            placeholder="Chọn năm"
            class="ip36"
          />
        </div>
      </template>
    </Toolbar>
    <table id="table-leave" class="table-custom">
      <thead class="thead-custom">
        <tr>
          <th
            class="sticky text-center"
            :style="{ left: '0', top: '0', width: '50px' }"
          >
            STT
          </th>
          <th
            class="sticky text-left"
            :style="{ left: '50px', top: '0', minWidth: '200px' }"
          >
            Họ và tên
          </th>
          <th
            class="sticky text-center"
            :style="{
              right: '0',
              top: '0',
              width: '150px',
            }"
          >
            Phép năm
          </th>
          <th
            class="sticky text-center"
            :style="{
              right: '0',
              top: '0',
              width: '150px',
            }"
          >
            Phép năm thưc tế
          </th>
        </tr>
      </thead>
      <tbody
        class="tbody-custom"
        v-for="(group, group_key) in datas"
        :key="group_key"
      >
        <tr>
          <td
            colspan="4"
            class="sticky"
            :style="{
              left: 0,
              background: '#DEE6F0',
            }"
          >
            <b>{{ group.department_name }}</b>
          </td>
        </tr>
        <tr
          v-for="(user, user_key) in group.users"
          :key="user_key"
          class="hover"
        >
          <td
            class=""
            :style="{
              width: '50px',
              background: '#f8f9fa',
              textAlign: 'center',
            }"
          >
            {{ user.stt }}
          </td>
          <td
            class="text-left"
            :style="{
              minWidth: '200px',
              backgroundColor: '#fff',
            }"
          >
            <b>{{ user.profile_name }}</b>
          </td>
          <td
            class="text-center"
            :style="{
              width: '150px',
              backgroundColor: '#fff',
            }"
          >
            <InputNumber
              v-model="user.leave"
              mode="decimal"
              :minFractionDigits="0"
              :maxFractionDigits="2"
              :step="0.5"
              :min="0"
              :max="100"
              class="ip36 w-full"
              @input="changeLeave(user)"
            />
          </td>
          <td
            class="text-center"
            :style="{
              width: '150px',
              backgroundColor: '#fff',
            }"
          >
            <InputNumber
              v-model="user.leave_limit"
              mode="decimal"
              :minFractionDigits="0"
              :maxFractionDigits="2"
              :step="0.5"
              :min="0"
              :max="100"
              class="ip36 w-full"
            />
          </td>
        </tr>
      </tbody>
    </table>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="props.closeDialog()"
        class="p-button-text"
      />
      <Button label="Lưu" icon="pi pi-check" @click="saveModel()" />
    </template>
  </Dialog>
</template>
<style scoped>
.box-table {
  height: calc(100vh - 160px) !important;
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
  border-collapse: collapse;
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
.hover {
  cursor: pointer;
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
