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
const dictionarys = ref([]);
const tags = ref([]);
const tagids = ref([]);
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
  let formData = new FormData();
  formData.append("profile_id", props.profile["profile_id"]);
  formData.append("tags", JSON.stringify(tagids.value));
  axios
    .put(baseURL + "/api/hrm_profile/update_profile_tags", formData, config)
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
const removeFilter = (idx, array, isTree) => {
  if (isTree) {
    array[idx["key"]]["checked"] = false;
  } else {
    array.splice(idx, 1);
  }
};

//init
const initDictionary = () => {
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_profile_tag_dictionary",
            par: [{ par: "user_id", va: store.getters.user.user_id }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      if (response != null && response.data != null) {
        var data = response.data.data;
        if (data != null) {
          let tbs = JSON.parse(data);
          dictionarys.value = tbs;
        }
      }
    });
};
const initData = (rf) => {
  if (rf) {
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
  }
  tags.value = [];
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_profile_tag_get",
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
          tags.value = tbs[0];
          tagids.value = tbs[0].map((x) => x.tags_id);
        } else {
          tags.value = [];
          tagids.value = [];
        }
      }
      swal.close();
      //selectedNodes.value = tags.value.filter((x) => x["is_active"]);
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
    initDictionary();
    initData(true);
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
        <div class="col-12 md:col-12">
          <div class="form-group">
            <MultiSelect
              :options="dictionarys[0]"
              :filter="true"
              :showClear="true"
              :editable="false"
              v-model="tagids"
              optionLabel="tags_name"
              optionValue="tags_id"
              placeholder="Chọn nhãn"
              class="w-full limit-width"
              style="min-height: 36px"
              panelClass="d-design-dropdown"
            >
              <template #value="slotProps">
                <div
                  class="p-dropdown-car-value flex text-justify"
                  v-if="slotProps.value"
                >
                  <div class="text-justify flex format-center">
                    <ul
                      class="p-ulchip"
                      v-if="
                        slotProps.value &&
                        slotProps.value.length > 0 &&
                        dictionarys[0] &&
                        dictionarys[0].length > 0
                      "
                    >
                      <li
                        class="p-lichip"
                        v-for="(item, index) in slotProps.value"
                        :key="index"
                      >
                        <Chip class="mr-2 mb-2 px-3 py-2">
                          <div class="flex">
                            <div class="pr-2 text-justify">
                              <i class="pi pi-tags"></i>
                            </div>
                            <div>
                              <span>
                                {{
                                  dictionarys[0].find(
                                    (x) => x["tags_id"] === item
                                  ).tags_name
                                }}</span
                              >
                            </div>
                          </div>
                        </Chip>
                      </li>
                    </ul>
                  </div>
                </div>
                <span v-else>
                  {{ slotProps.placeholder }}
                </span>
              </template>
            </MultiSelect>
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
