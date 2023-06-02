<script setup>
import { onMounted, inject, ref, nextTick } from "vue";
import { encr } from "../../../../util/function";
import { useToast } from "vue-toastification";
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
  profile: Object,
  year: Number,
  initData: Function,
});
const display = ref(props.displayDialog);

//Declare
const options = ref({
  loading: false,
  user_id: store.getters.user.user_id,
  search: "",
  pageNo: 0,
  pageSize: 20,
  total: 0,
  sort: "created_date desc",
  orderBy: "DESC",
});
const newdate = new Date();
const selectedNodes = ref([]);
const datas = ref([]);
//Function

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
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_leave_profile_detail",
            par: [
              { par: "profile_id", va: props.profile.profile_id },
              { par: "year", va: props.year },
              { par: "pageNo", va: options.value.pageNo },
              { par: "pageSize", va: options.value.pageSize },
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
            datas.value = tbs[0];
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

const onPage = (event) => {
  options.value.pagesize = event.rows;
  options.value.pageno = event.page;
  initData(true);
};
</script>
<template>
  <Dialog
    :header="
      'Danh sách ngày nghỉ phép của ' +
      props.profile.profile_name +
      ' trong năm ' +
      props.year
    "
    v-model:visible="display"
    :style="{ width: '50vw' }"
    :maximizable="true"
    :closable="true"
    style="z-index: 9000"
  >
    <DataTable
      @page="onPage($event)"
      :value="datas"
      :lazy="true"
      :rowHover="true"
      :showGridlines="true"
      :paginator="true"
      :rows="options.pageSize"
      :rowsPerPageOptions="[20, 30, 50, 100, 200]"
      :totalRecords="options.total"
      v-model:selection="selectedNodes"
      dataKey="leave_id"
      scrollHeight="flex"
      filterDisplay="menu"
      filterMode="lenient"
      responsiveLayout="scroll"
      rowGroupMode="rowspan"
      groupRowsBy="leave_date_string"
    >
      <Column
        field="leave_date_string"
        header="Ngày tháng"
        headerStyle="text-align:center;width:150px;height:50px;"
        bodyStyle="text-align:center;width:150px;max-height:60px"
        class="align-items-center justify-content-center text-center"
      >
        <template #body="slotProps">
          <div class="format-grid-center style-day">
            <b>{{ slotProps.data.leave_date_name }}</b>
            <span>{{ slotProps.data.leave_date_string }}</span>
          </div>
        </template>
      </Column>
      <Column
        field="type_dayoff_name"
        header="Loại nghỉ"
        headerStyle="text-align:center;width:150px;height:50px;"
        bodyStyle="text-align:center;width:150px;max-height:60px"
        class="align-items-center justify-content-center text-center"
      >
      </Column>
      <Column
        field="is_type_name"
        header="Nội dung"
        headerStyle="height:50px;max-width:auto;min-width:150px;"
        bodyStyle="max-height:60px;"
      >
      </Column>
    </DataTable>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="props.closeDialog()"
        class="p-button-text"
      />
    </template>
  </Dialog>
</template>
<style scoped>
@import url(../../profile/component/stylehrm.css);
.table {
  width: 100%;
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
::v-deep(.avatar-item) {
  .p-avatar.p-avatar-lg {
    width: 3rem;
    height: 3rem;
  }
}
::v-deep(.is-close) {
  .p-panel-header {
    color: red;
  }
}
::v-deep(.text-right) {
  input {
    text-align: right;
  }
}
::v-deep(.limit-width) {
  .p-multiselect-label {
    white-space: normal !important;
  }
}
</style>
