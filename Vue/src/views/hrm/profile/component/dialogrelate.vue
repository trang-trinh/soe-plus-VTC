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
const relate = ref({});
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
  var obj = JSON.parse(JSON.stringify(relate.value));
  if (obj["relate_time"] != null) {
    obj["relate_time"] = moment(obj["relate_time"]).format(
      "YYYY-MM-DDTHH:mm:ssZZ"
    );
  }
  let formData = new FormData();
  formData.append("profile_id", props.profile["profile_id"]);
  if (relate.value["profile"] != null) {
    relate.value["relate_id"] = relate.value["profile"]["profile_id"];
    // delete relate.value["profile"];
  }
  formData.append("relate", JSON.stringify(relate.value));
  axios
    .put(baseURL + "/api/hrm_profile/update_profile_relate", formData, config)
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
            proc: "hrm_profile_relate_edit",
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
          relate.value = tbs[0][0];
          if (relate.value["relate_time"] != null) {
            relate.value["relate_time"] = new Date(relate.value["relate_time"]);
          }
          if (relate.value["relate_id"]) {
            relate.value["profile"] = {
              profile_id: relate.value["relate_id"],
              profile_user_name: relate.value["profile_user_name"],
              avatar: relate.value["avatar"],
            };
          }
        } else {
          relate.value = {};
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
    v-model:visible="props.displayDialog"
    :style="{ width: '40vw' }"
    :maximizable="true"
    :closable="false"
    style="z-index: 9000"
  >
    <form @submit.prevent="" name="submitform">
      <div class="grid formgrid m-2">
        <div class="col-12 md:col-12">
          <div class="form-group">
            <label>Họ và tên</label>
            <Dropdown
              :options="props.users"
              :filter="true"
              :showClear="true"
              :editable="false"
              optionLabel="profile_user_name"
              placeholder="Chọn"
              v-model="relate.profile"
              class="ip36"
              style="height: auto; min-height: 36px"
            >
              <template #value="slotProps">
                <div class="mt-2" v-if="slotProps.value">
                  <Chip
                    :image="slotProps.value.avatar"
                    :label="slotProps.value.profile_user_name"
                    class="mr-2 mb-2 pl-0"
                  >
                    <div class="flex">
                      <div class="format-flex-center">
                        <Avatar
                          v-bind:label="
                            slotProps.value.avatar
                              ? ''
                              : (
                                  slotProps.value.profile_user_name ?? ''
                                ).substring(0, 1)
                          "
                          v-bind:image="
                            slotProps.value.avatar
                              ? basedomainURL + slotProps.value.avatar
                              : basedomainURL + '/Portals/Image/noimg.jpg'
                          "
                          style="
                            background-color: #2196f3;
                            color: #ffffff;
                            width: 2rem;
                            height: 2rem;
                          "
                          :style="{
                            background: bgColor[1 % 7],
                          }"
                          class="mr-2 text-avatar"
                          size="xlarge"
                          shape="circle"
                        />
                      </div>
                      <div class="format-center">
                        <span>{{ slotProps.value.profile_user_name }}</span>
                      </div>
                    </div>
                  </Chip>
                </div>
                <span v-else> {{ slotProps.placeholder }} </span>
              </template>
              <template #option="slotProps">
                <div v-if="slotProps.option" class="flex">
                  <div class="format-center mr-3">
                    <Avatar
                      v-bind:label="
                        slotProps.option.avatar
                          ? ''
                          : slotProps.option.profile_user_name.substring(0, 1)
                      "
                      v-bind:image="
                        slotProps.option.avatar
                          ? basedomainURL + slotProps.option.avatar
                          : basedomainURL + '/Portals/Image/noimg.jpg'
                      "
                      style="
                        background-color: #2196f3;
                        color: #ffffff;
                        width: 3rem;
                        height: 3rem;
                        font-size: 1.4rem !important;
                      "
                      :style="{
                        background: bgColor[slotProps.index % 7],
                      }"
                      class="text-avatar"
                      size="xlarge"
                      shape="circle"
                    />
                  </div>
                  <div class="format-center">
                    {{ slotProps.option.profile_user_name }}
                  </div>
                </div>
                <span v-else> Chưa có dữ liệu </span>
              </template>
            </Dropdown>
          </div>
        </div>
        <div class="col-12 md:col-12">
          <div class="form-group">
            <label>Ngày kết hôn</label>
            <Calendar
              class="ip36"
              id="icon"
              v-model="relate.relate_time"
              :showIcon="true"
              placeholder="dd/mm/yyyy"
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
.p-overlaypanel {
  z-index: 99999;
}
</style>
<style lang="scss" scoped>
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
