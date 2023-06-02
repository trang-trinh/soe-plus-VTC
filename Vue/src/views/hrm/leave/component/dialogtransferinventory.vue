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
  organization_id: String,
  initData: Function,
});
const display = ref(props.displayDialog);

//Declare
const model = ref({});

///Function
const submitted = ref(false);
const saveModel = () => {
  submitted.value = true;
  if (!model.value.start_year || !model.value.end_year) {
    swal.fire({
      title: "Thông báo!",
      text: "Vui lòng điền đầy đủ thông tin trường bôi đỏ!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });

  let formData = new FormData();
  var obj = Object.assign({}, model.value);
  if (obj.start_year != null) {
    obj.start = new Date(obj.start_year).getFullYear();
    formData.append("start", obj.start);
  }
  if (obj.end_year != null) {
    obj.end = new Date(obj.end_year).getFullYear();
    formData.append("end", obj.end);
  }
  formData.append("organization_id", props.organization_id || -1);
  axios
    .put(baseURL + "/api/hrm_leave/update_transfer_inventory", formData, config)
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

//init
onMounted(() => {
  if (display.value) {
    nextTick(() => {});
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
    <form @submit.prevent="" name="submitform">
      <div class="grid formgrid m-2">
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Từ năm</label>
            <Calendar
              v-model="model.start_year"
              @date-select="goYear(model.start_year)"
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
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Đến năm</label>
            <Calendar
              v-model="model.end_year"
              @date-select="goYear(model.end_year)"
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
        </div>
      </div>
    </form>
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
