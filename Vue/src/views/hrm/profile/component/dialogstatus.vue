<script setup>
import moment from "moment";
import { onMounted, inject, ref } from "vue";
import { useToast } from "vue-toastification";
import { encr } from "../../../../util/function";
const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios");
const cryoptojs = inject("cryptojs");
const toast = useToast();
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const basedomainURL = baseURL;

//Get arguments
const props = defineProps({
  key: Number,
  headerDialog: String,
  displayDialog: Boolean,
  closeDialog: Function,
  profile: Object,
});
const display = ref(props.displayDialog);
const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);
const options = ref({
  loading: true,
  total: 0,
});
const status = ref([
  {
    status: 1,
    title: "Đang làm việc",
    icon: "",
    total: 0,
    bg_color: "#AFE362",
    text_color: "#fff",
  },
  {
    status: 2,
    title: "Nghỉ việc",
    icon: "",
    total: 0,
    bg_color: "#E74C3C",
    text_color: "#fff",
  },
  {
    status: 3,
    title: "Nghỉ thai sản",
    icon: "",
    total: 0,
    bg_color: "#9B59B6",
    text_color: "#fff",
  },
  {
    status: 4,
    title: "Nghỉ không lương",
    icon: "",
    total: 0,
    bg_color: "#E67E22",
    text_color: "#fff",
  },
  {
    status: 5,
    title: "Nghỉ đi học",
    icon: "",
    total: 0,
    bg_color: "#F1C40F",
    text_color: "#fff",
  },
  {
    status: 6,
    title: "Nghỉ khác",
    icon: "",
    total: 0,
    bg_color: "#7F8C8D",
    text_color: "#fff",
  },
]);
const object = ref({
  status: props.profile.status,
  start_date: props.profile.start_date || null,
  end_date: props.profile.end_date || null,
});
const datas = ref([]);

//function
const opstatus = ref();
const toggleStatus = (event) => {
  opstatus.value.toggle(event);
};
const submitted = ref(false);
const saveModel = () => {
  submitted.value = true;
  if (
    (!object.value.status && object.value.status !== 0) ||
    !object.value.start_date
  ) {
    swal.fire({
      title: "Thông báo!",
      text: "Vui lòng điền đầy đủ thông tin trường bôi đỏ!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  if (datas.value != null && datas.value.length > 0) {
    var isValid = false;
    datas.value.forEach((item) => {
      if (
        (!item.status && item.status !== 0) ||
        !item.start_date ||
        (!item.end_date && item.status !== 1)
      ) {
        isValid = true;
        return;
      }
    });
    if (isValid) {
      swal.fire({
        title: "Thông báo!",
        text: "Vui lòng điền đầy đủ thông tin trường bôi đỏ!",
        icon: "error",
        confirmButtonText: "OK",
      });
      return;
    }
  }
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  let formData = new FormData();
  formData.append("profile_id", props.profile["profile_id"]);
  formData.append("object", JSON.stringify(object.value));
  formData.append("datas", JSON.stringify(datas.value));
  axios
    .put(baseURL + "/api/hrm_profile/update_profile_status", formData, config)
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

const addRow = () => {
  datas.value.push({});
};
const deleteRow = (idx) => {
  datas.value.splice(idx, 1);
};

//init
const initData = (rf) => {
  if (rf) {
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
            proc: "hrm_profile_status_list",
            par: [{ par: "profile_id", va: props.profile["profile_id"] }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      var data = response.data.data;
      if (data != null) {
        var tbs = JSON.parse(data);
        if (tbs[0] != null && tbs[0].length > 0) {
          tbs[0].forEach((item) => {
            if (item.start_date != null) {
              item.start_date = new Date(item.start_date);
            }
            if (item.end_date != null) {
              item.end_date = new Date(item.end_date);
            }
          });
          datas.value = tbs[0];
        }
      }
      if (options.value.loading) options.value.loading = false;
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
  if (props.displayDialog) {
    initData(true);
  }
});
</script>
<template>
  <Dialog
    :header="props.headerDialog"
    v-model:visible="display"
    :style="{ width: '50vw' }"
    :maximizable="true"
    :closable="true"
    style="z-index: 9000"
  >
    <form @submit.prevent="" name="submitform">
      <div class="grid formgrid m-2">
        <div class="col-4 md:co-4">
          <div class="form-group">
            <label>Trạng thái <span class="redsao">(*)</span></label>
            <Dropdown
              :options="status"
              v-model="object.status"
              optionLabel="title"
              optionValue="status"
              placeholder="Chọn trạng thái"
              class="ip36"
            />
            <div v-if="!object.status && submitted">
              <small class="p-error">
                <span>Trạng thái không được để trống</span>
              </small>
            </div>
          </div>
        </div>
        <div class="col-4 md-col-4">
          <div class="form-group">
            <label>Từ ngày <span class="redsao">(*)</span></label>
            <Calendar
              class="ip36"
              id="icon"
              v-model="object.start_date"
              :showIcon="true"
              placeholder="dd/mm/yyyy"
            />
            <div v-if="!object.start_date && submitted">
              <small class="p-error">
                <span>Từ ngày không được để trống</span>
              </small>
            </div>
          </div>
        </div>
        <div class="col-4 md-col-4">
          <div class="form-group">
            <label>Đến ngày</label>
            <Calendar
              class="ip36"
              id="icon"
              v-model="object.end_date"
              :showIcon="true"
              placeholder="dd/mm/yyyy"
            />
          </div>
        </div>
        <div class="col-12 md:col-12">
          <div class="form-group">
            <label></label>
          </div>
        </div>
        <div class="col-12 md:col-12">
          <DataTable
            :value="datas"
            :scrollable="true"
            :lazy="true"
            :rowHover="true"
            :showGridlines="true"
            scrollHeight="flex"
            filterDisplay="menu"
            filterMode="lenient"
            responsiveLayout="scroll"
          >
            <Column
              header=""
              headerStyle="text-align:center;width:50px"
              bodyStyle="text-align:center;width:50px"
              class="align-items-center justify-content-center text-center"
            >
              <template #body="slotProps">
                <a
                  @click="deleteRow(slotProps.index)"
                  class="hover"
                  v-tooltip.top="'Xóa'"
                >
                  <i class="pi pi-times-circle" style="font-size: 18px"></i>
                </a>
              </template>
            </Column>
            <Column
              field="status"
              header="Trạng thái"
              headerStyle="text-align:center;height:50px"
              bodyStyle="text-align:center;"
              class="align-items-center justify-content-center text-center"
            >
              <template #body="slotProps">
                <div class="form-group m-0">
                  <Dropdown
                    :options="status"
                    v-model="slotProps.data.status"
                    optionLabel="title"
                    optionValue="status"
                    placeholder="Chọn trạng thái"
                    class="ip36"
                  />
                  <div v-if="!slotProps.data.status && submitted">
                    <small class="p-error">
                      <span>Trạng thái không được để trống</span>
                    </small>
                  </div>
                </div>
              </template>
            </Column>
            <Column
              field="start_date"
              header="Từ ngày"
              headerStyle="text-align:center;width:150px;height:50px"
              bodyStyle="text-align:center;width:150px;"
              class="align-items-center justify-content-center text-center"
            >
              <template #body="slotProps">
                <div class="form-group m-0">
                  <Calendar
                    class="ip36"
                    id="icon"
                    v-model="slotProps.data.start_date"
                    :showIcon="true"
                    placeholder="dd/mm/yyyy"
                  />
                  <div v-if="!slotProps.data.start_date && submitted">
                    <small class="p-error">
                      <span>Từ ngày không được để trống</span>
                    </small>
                  </div>
                </div>
              </template>
            </Column>
            <Column
              field="end_date"
              header="Đến ngày"
              headerStyle="text-align:center;width:150px;height:50px"
              bodyStyle="text-align:center;width:150px;"
              class="align-items-center justify-content-center text-center"
            >
              <template #body="slotProps">
                <div class="form-group m-0">
                  <Calendar
                    class="ip36"
                    id="icon"
                    v-model="slotProps.data.end_date"
                    :showIcon="true"
                    placeholder="dd/mm/yyyy"
                  />
                  <div
                    v-if="
                      !slotProps.data.end_date &&
                      slotProps.data.status !== 1 &&
                      submitted
                    "
                  >
                    <small class="p-error">
                      <span>Ngày đến không được để trống</span>
                    </small>
                  </div>
                </div>
              </template>
            </Column>
            <template #empty>
              <div
                class="align-items-center justify-content-center p-4 text-center m-auto"
                :style="{
                  display: 'flex',
                  width: '100%',
                  height: '100px',
                  backgroundColor: '#fff',
                }"
              >
                <div v-if="!options.loading && options.total === 0">
                  <h3 class="m-1">Không có dữ liệu</h3>
                </div>
              </div>
            </template>
          </DataTable>
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
.p-overlaypanel {
  z-index: 99999;
}
</style>
<style lang="scss" scoped>
::v-deep(.p-datatable) {
  table {
    border-collapse: collapse;
    min-width: 100%;
    table-layout: fixed;
  }

  .p-datatable-thead .justify-content-center .p-column-header-content {
    justify-content: center !important;
  }

  .p-datatable-thead .justify-content-left .p-column-header-content {
    justify-content: left !important;
  }

  .p-datatable-thead .justify-content-right .p-column-header-content {
    justify-content: right !important;
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
