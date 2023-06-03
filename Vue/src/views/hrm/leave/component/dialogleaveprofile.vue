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
const newdate = new Date();
const model = ref({
  profile_id: props.profile.profile_id,
  tempyear: new Date(props.year, newdate.getMonth() + 1, newdate.getDay()),
});

//Function
const goYear = (date) => {
  initData(true);
};

const submitted = ref(false);
const saveModel = () => {
  submitted.value = true;
  if (!model.value.tempyear) {
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

  var obj = Object.assign({}, model.value);
  if (obj.tempyear != null) {
    obj.year = new Date(obj.tempyear).getFullYear();
  }
  let formData = new FormData();
  formData.append("model", JSON.stringify(obj));
  axios
    .put(baseURL + "/api/hrm_leave/update_leave_profile", formData, config)
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
const initData = (ref) => {
  if (ref) {
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
  }
  let year = new Date(model.value.tempyear).getFullYear();
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_leave_profile_get",
            par: [
              { par: "profile_id", va: props.profile.profile_id },
              { par: "year", va: year },
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
            model.value = tbs[0][0];
            model.value.tempyear = new Date(
              model.value.year,
              newdate.getMonth() + 1,
              newdate.getDay()
            );
          } else {
            model.value = {
              profile_id: props.profile.profile_id,
              tempyear: new Date(
                year,
                newdate.getMonth() + 1,
                newdate.getDay()
              ),
              month1: 0,
              month2: 0,
              month3: 0,
              month4: 0,
              month5: 0,
              month6: 0,
              month7: 0,
              month8: 0,
              month9: 0,
              month10: 0,
              month11: 0,
              month12: 0,
              inventory: 0,
              bonus: 0,
              seniority: 0,
            };
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
    :header="'Cập nhật ngày nghỉ phép nhân sự: ' + props.profile.profile_name"
    v-model:visible="display"
    :style="{ width: '40vw' }"
    :maximizable="true"
    :closable="true"
    style="z-index: 9000"
  >
    <form @submit.prevent="" name="submitform">
      <div class="grid formgrid m-2">
        <div class="col-12 md:col-12 p-0">
          <div class="row">
            <div class="col-6 md:col-6">
              <div class="form-group">
                <label>Phép năm</label>
                <Calendar
                  v-model="model.tempyear"
                  @date-select="goYear(model.tempyear)"
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
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Phép tồn năm trước</label>
            <InputNumber
              v-model="model.inventory"
              mode="decimal"
              :minFractionDigits="0"
              :maxFractionDigits="2"
              showButtons
              class="ip36"
            />
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Phép thưởng</label>
            <InputNumber
              v-model="model.bonus"
              mode="decimal"
              :minFractionDigits="0"
              :maxFractionDigits="2"
              :min="0"
              :max="100"
              showButtons
              class="ip36"
            />
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Tháng 1</label>
            <InputNumber
              v-model="model.month1"
              mode="decimal"
              :minFractionDigits="0"
              :maxFractionDigits="2"
              :min="0"
              :max="100"
              showButtons
              class="ip36"
            />
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Tháng 2</label>
            <InputNumber
              v-model="model.month2"
              mode="decimal"
              :minFractionDigits="0"
              :maxFractionDigits="2"
              :min="0"
              :max="100"
              showButtons
              class="ip36"
            />
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Tháng 3</label>
            <InputNumber
              v-model="model.month3"
              mode="decimal"
              :minFractionDigits="0"
              :maxFractionDigits="2"
              :min="0"
              :max="100"
              showButtons
              class="ip36"
            />
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Tháng 4</label>
            <InputNumber
              v-model="model.month4"
              mode="decimal"
              :minFractionDigits="0"
              :maxFractionDigits="2"
              :min="0"
              :max="100"
              showButtons
              class="ip36"
            />
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Tháng 5</label>
            <InputNumber
              v-model="model.month5"
              mode="decimal"
              :minFractionDigits="0"
              :maxFractionDigits="2"
              :min="0"
              :max="100"
              showButtons
              class="ip36"
            />
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Tháng 6</label>
            <InputNumber
              v-model="model.month6"
              mode="decimal"
              :minFractionDigits="0"
              :maxFractionDigits="2"
              :min="0"
              :max="100"
              showButtons
              class="ip36"
            />
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Tháng 7</label>
            <InputNumber
              v-model="model.month7"
              mode="decimal"
              :minFractionDigits="0"
              :maxFractionDigits="2"
              :min="0"
              :max="100"
              showButtons
              class="ip36"
            />
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Tháng 8</label>
            <InputNumber
              v-model="model.month8"
              mode="decimal"
              :minFractionDigits="0"
              :maxFractionDigits="2"
              :min="0"
              :max="100"
              showButtons
              class="ip36"
            />
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Tháng 9</label>
            <InputNumber
              v-model="model.month9"
              mode="decimal"
              :minFractionDigits="0"
              :maxFractionDigits="2"
              :min="0"
              :max="100"
              showButtons
              class="ip36"
            />
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Tháng 10</label>
            <InputNumber
              v-model="model.month10"
              mode="decimal"
              :minFractionDigits="0"
              :maxFractionDigits="2"
              :min="0"
              :max="100"
              showButtons
              class="ip36"
            />
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Tháng 11</label>
            <InputNumber
              v-model="model.month11"
              mode="decimal"
              :minFractionDigits="0"
              :maxFractionDigits="2"
              :min="0"
              :max="100"
              showButtons
              class="ip36"
            />
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Tháng 12</label>
            <InputNumber
              v-model="model.month12"
              mode="decimal"
              :minFractionDigits="0"
              :maxFractionDigits="2"
              :min="0"
              :max="100"
              showButtons
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
