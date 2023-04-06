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
  users: Array,
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
const injections = ref([
  { id: 1, title: "Mũi 1" },
  { id: 2, title: "Mũi 2" },
  { id: 3, title: "Mũi 3" },
  { id: 4, title: "Mũi 4" },
  { id: 5, title: "Mũi 5" },
  { id: 6, title: "Mũi 6" },
  { id: 7, title: "Mũi 7" },
  { id: 8, title: "Mũi 8" },
  { id: 9, title: "Mũi 9" },
  { id: 10, title: "Mũi 10" },
]);
const type_vaccines = ref([
  { id: "Vaccine Abdala (AICA-Cuba)", title: "Vaccine Abdala (AICA-Cuba)" },
  { id: "Vaccine Hayat-Vax", title: "Vaccine Hayat-Vax" },
  { id: "Covit-19 Vaccine Janssen", title: "Covit-19 Vaccine Janssen" },
  {
    id: "Spikevax (Covit-19 vaccine Modena)",
    title: "Spikevax (Covit-19 vaccine Modena)",
  },
  { id: "Comirnaty (Pfizer BioNtech)", title: "Comirnaty (Pfizer BioNtech)" },
  { id: "Vero-cell (của Sinopharm)", title: "Vero-cell (của Sinopharm)" },
  { id: "AZD1222 (của AstraZeneca)", title: "AZD1222 (của AstraZeneca)" },
  { id: "Sputnik-V (của Gamalaya)", title: "Sputnik-V (của Gamalaya)" },
]);
const health = ref({});
const vaccines = ref([]);

//function
const opstatus = ref();
const toggleStatus = (event) => {
  opstatus.value.toggle(event);
};
const submitted = ref(false);
const saveModel = () => {
  submitted.value = true;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  var vaccine = vaccines.value;
  vaccine.forEach((x) => {
    if (x["injection_date"] != null) {
      x["injection_date"] = moment(x["injection_date"]).format(
        "YYYY-MM-DDTHH:mm:ssZZ"
      );
    }
  });
  let formData = new FormData();
  formData.append("profile_id", props.profile["profile_id"]);
  formData.append("health", JSON.stringify(health.value));
  formData.append("vaccines", JSON.stringify(vaccine));
  axios
    .put(baseURL + "/api/hrm_profile/update_profile_health", formData, config)
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
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
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
  var obj = { vaccine_id: -1 };
  vaccines.value.push(obj);
};
const deleteRow = (idx) => {
  vaccines.value.splice(idx, 1);
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
            proc: "hrm_profile_helth_get",
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
          health.value = tbs[0][0];
        } else {
          health.value = {};
        }
        if (tbs[1] != null && tbs[1].length > 0) {
          tbs[1].forEach((x) => {
            if (x["injection_date"] != null) {
              x["injection_date"] = new Date(x["injection_date"]);
            }
          });
          vaccines.value = tbs[1];
        } else {
          vaccines.value = [{ vaccine_id: -1 }];
        }
      }
      swal.close();
    })
    .catch((error) => {
      swal.close();
      if (error && error.status === 401) {
        swal.fire({
          title: "Thông báo!",
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
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
    :style="{ width: '60vw' }"
    :maximizable="true"
    :closable="true"
    style="z-index: 9000"
  >
    <form @submit.prevent="" name="submitform">
      <div class="grid formgrid m-2">
        <div class="col-12 md:col-12">
          <div class="form-group">
            <h3 class="m-0">1. Thông tin chung</h3>
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Chiều cao</label>
            <InputText
              spellcheck="false"
              class="ip36"
              v-model="health.height"
              maxLength="50"
            />
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Cân nặng</label>
            <InputText
              v-model="health.weight"
              spellcheck="false"
              class="ip36"
              maxLength="50"
            />
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Nhóm máu</label>
            <InputText
              spellcheck="false"
              class="ip36"
              v-model="health.blood_group"
              maxLength="50"
            />
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Huyết áp</label>
            <InputText
              spellcheck="false"
              class="ip36"
              v-model="health.blood_pressure"
              maxLength="50"
            />
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Nhịp tim</label>
            <InputText
              spellcheck="false"
              class="ip36"
              v-model="health.heartbeat"
              maxLength="50"
            />
          </div>
        </div>
        <div class="col-12 md:col-12">
          <div class="form-group">
            <label>Ghi chú</label>
            <Textarea :autoResize="true" rows="4" v-model="health.note" />
          </div>
        </div>
        <div class="col-12 md:col-12">
          <div class="form-group">
            <div class="flex justify-content-between">
              <div>
                <h3 class="m-0">2. Thông tin tiêm Vắc xin</h3>
              </div>
              <div>
                <a
                  @click="
                    addRow();
                    $event.stopPropagation();
                  "
                  class="hover"
                  v-tooltip.top="'Thêm mới'"
                >
                  <i
                    class="pi pi-plus-circle"
                    data-v-62364173=""
                    style="font-size: 18px"
                  ></i>
                </a>
              </div>
            </div>
          </div>
        </div>
        <div class="col-12 md:col-12">
          <DataTable
            :value="vaccines"
            :scrollable="true"
            :lazy="true"
            :rowHover="true"
            :showGridlines="true"
            scrollDirection="both"
            ><Column
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
              field="injection_id"
              header="Mũi"
              headerStyle="text-align:center;width:150px;height:50px"
              bodyStyle="text-align:center;width:150px;"
              class="align-items-center justify-content-center text-center"
            >
              <template #body="slotProps">
                <div class="form-group m-0">
                  <Dropdown
                    :options="injections"
                    v-model="slotProps.data.injection_id"
                    optionLabel="title"
                    optionValue="id"
                    placeholder="Chọn mũi tiêm"
                    class="ip36"
                  />
                </div>
              </template>
            </Column>
            <Column
              field="injection_date"
              header="Ngày tiêm"
              headerStyle="text-align:center;width:150px;height:50px"
              bodyStyle="text-align:center;width:150px;"
              class="align-items-center justify-content-center text-center"
            >
              <template #body="slotProps">
                <Calendar
                  class="ip36"
                  id="icon"
                  v-model="slotProps.data.injection_date"
                  :showIcon="true"
                  placeholder="dd/mm/yyyy"
                />
              </template>
            </Column>
            <Column
              field="type_vaccine"
              header="Loại vắc xin"
              headerStyle="text-align:center;width:250px;height:50px"
              bodyStyle="text-align:center;width:250px;"
              class="align-items-center justify-content-center text-center"
            >
              <template #body="slotProps">
                <div class="form-group m-0">
                  <Dropdown
                    :filter="true"
                    :editable="true"
                    :options="type_vaccines"
                    v-model="slotProps.data.type_vaccine"
                    optionLabel="title"
                    optionValue="id"
                    placeholder="Chọn loại vắc xin"
                    class="ip36"
                    maxLength="500"
                  />
                </div>
              </template>
            </Column>
            <Column
              field="lot_number"
              header="Số lô"
              headerStyle="text-align:center;width:150px;height:50px"
              bodyStyle="text-align:center;width:150px;"
              class="align-items-center justify-content-center text-center"
            >
              <template #body="slotProps">
                <InputText
                  spellcheck="false"
                  class="ip36"
                  v-model="slotProps.data.lot_number"
                  maxLength="50"
                />
              </template>
            </Column>
            <Column
              field="vaccination_facility"
              header="Cơ sở tiêm chủng"
              headerStyle="text-align:center;width:250px;height:50px"
              bodyStyle="text-align:center;width:250px;"
              class="align-items-center justify-content-center text-center"
            >
              <template #body="slotProps">
                <Textarea
                  :autoResize="true"
                  rows="1"
                  class="ip36"
                  v-model="slotProps.data.vaccination_facility"
                  maxLength="500"
                />
              </template>
            </Column>
            <Column
              field="sign_user"
              header="Người ký"
              headerStyle="text-align:center;width:200px;height:50px"
              bodyStyle="text-align:center;width:200px;"
              class="align-items-center justify-content-center text-center"
            >
              <template #body="slotProps">
                <InputText
                  spellcheck="false"
                  class="ip36"
                  v-model="slotProps.data.sign_user"
                  maxLength="250"
                />
              </template>
            </Column>
            <Column
              field="sign_user_position"
              header="Chức vụ"
              headerStyle="text-align:center;width:200px;height:50px"
              bodyStyle="text-align:center;width:200px;"
              class="align-items-center justify-content-center text-center"
            >
              <template #body="slotProps">
                <InputText
                  spellcheck="false"
                  class="ip36"
                  v-model="slotProps.data.sign_user_position"
                  maxLength="50"
                />
              </template>
            </Column>
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
}
</style>
