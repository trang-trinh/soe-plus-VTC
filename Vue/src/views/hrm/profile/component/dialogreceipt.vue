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
const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);
const reciptstatus = ref([
  { status: 2, title: "Đã nộp đủ" },
  { status: 1, title: "Nộp còn thiếu" },
  { status: 0, title: "Chưa nộp" },
]);
const receipts = ref([]);
const selectedNodes = ref([]);

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
  var receipts = [];
  if (selectedNodes.value != null && selectedNodes.value.length > 0) {
    receipts = [...selectedNodes.value];
    receipts.forEach((x) => {
      if (x["receipt_date"] != null) {
        x["receipt_date"] = moment(x["receipt_date"]).format(
          "YYYY-MM-DDTHH:mm:ssZZ"
        );
      }
    });
  }
  let formData = new FormData();
  formData.append("profile_id", props.profile["profile_id"]);
  formData.append("receipt_status", props.profile["receipt_status"] || 1);
  formData.append("receipts", JSON.stringify(receipts));
  axios
    .put(baseURL + "/api/hrm_profile/update_profile_receipt", formData, config)
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
            proc: "hrm_profile_receipt_get",
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
          receipts.value = tbs[0];
          receipts.value.forEach((x) => {
            if (x["receipt_date"] != null) {
              x["receipt_date"] = new Date(x["receipt_date"]);
            }
          });
        }
      }
      swal.close();
      selectedNodes.value = receipts.value.filter((x) => x["is_active"]);
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
    v-model:visible="props.displayDialog"
    :style="{ width: '50vw' }"
    :maximizable="true"
    :closable="false"
    style="z-index: 9000"
  >
    <form @submit.prevent="" name="submitform">
      <Toolbar class="outline-none surface-0 border-none">
        <template #start>
          <div
            v-for="(value, key) in reciptstatus"
            :key="key"
            class="field-radiobutton mr-5"
          >
            <RadioButton
              :inputId="'status' + key"
              name="city"
              :value="value.status"
              v-model="props.profile.receipt_status"
            />
            <label for="city1">{{ value.title }}</label>
          </div>
        </template>
      </Toolbar>
      <div class="grid formgrid m-2">
        <div class="col-12 md:col-12">
          <DataTable
            :value="receipts"
            :scrollable="true"
            :lazy="true"
            :rowHover="true"
            :showGridlines="true"
            :globalFilterFields="['receipt_name']"
            v-model:selection="selectedNodes"
            dataKey="receipt_id"
            scrollHeight="flex"
            filterDisplay="menu"
            filterMode="lenient"
            paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
            responsiveLayout="scroll"
          >
            <Column
              selectionMode="multiple"
              headerStyle="text-align:center;max-width:50px"
              bodyStyle="text-align:center;max-width:50px"
              class="align-items-center justify-content-center text-center"
            ></Column>
            <Column
              field="receipt_name"
              header="Danh sách giấy tờ"
              headerStyle="max-width:auto;"
            >
              <template #body="slotProps">
                <span>{{ slotProps.data.receipt_name }}</span>
              </template>
            </Column>
            <Column
              field="receipt_date"
              header="Ngày tiếp nhận"
              headerStyle="text-align:center;max-width:150px;height:50px"
              bodyStyle="text-align:center;max-width:150px;"
              class="align-items-center justify-content-center text-center"
            >
              <template #body="slotProps">
                <Calendar
                  class="ip36"
                  id="icon"
                  v-model="slotProps.data.receipt_date"
                  :showIcon="true"
                  placeholder="dd/mm/yyyy"
                />
              </template>
            </Column>
            <Column
              field="receipt_name"
              header="Ghi chú"
              headerStyle="text-align:center;max-width:300px;height:50px"
              bodyStyle="text-align:center;max-width:300px;"
              class="align-items-center justify-content-center text-center"
            >
              <template #body="slotProps">
                <Textarea
                  :autoResize="true"
                  rows="1"
                  v-model="slotProps.data.note"
                  style="width: 100%"
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
.p-overlaypanel {
  z-index: 99999;
}
</style>
