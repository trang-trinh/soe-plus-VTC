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
const user = ref({});
const matchaccount = ref({
  account: null,
});
const dictionarys = ref([]);

//Function
const submitted = ref(false);
const saveModel = () => {
  submitted.value = true;
  let type = null;
  if (matchaccount.value.account != null) {
    type = 1;
  } else {
    type = 2;
    if (!user.value.user_id || !user.value.is_psword) {
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
  formData.append("type", type);
  formData.append("profile_id", props.profile.profile_id);
  if (type === 1) {
    formData.append("user_id", matchaccount.value.account.user_id);
  } else if (type === 2) {
    formData.append("user", JSON.stringify(user.value));
  }
  axios
    .put(baseURL + "/api/hrm_profile/match_account", formData, config)
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
      initData(true);
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
const initDictionary = () => {
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_matchaccount_dictionary",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "profile_id", va: props.profile.profile_id },
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
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_profile_matchaccount",
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
          matchaccount.value.account = tbs[0][0];
        } else {
          matchaccount.value.account = null;
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
            <h3 class="m-0">Chọn tài khoản (trường hợp đã có tài khoản trong hệ thống)</h3>
          </div>
        </div>
        <div class="col-12 md:col-12">
          <div class="form-group">
            <label>Tài khoản</label>
            <Dropdown
              :options="dictionarys[0]"
              :filter="true"
              :showClear="true"
              :editable="false"
              optionLabel="full_name"
              placeholder="Chọn nhân sự"
              v-model="matchaccount.account"
              class="ip36"
              style="height: auto; min-height: 36px"
            >
              <template #value="slotProps">
                <div class="mt-2" v-if="slotProps.value">
                  <Chip
                    :image="slotProps.value.avatar"
                    :label="slotProps.value.full_name"
                    class="mr-2 mb-2 pl-0"
                  >
                    <div class="flex">
                      <div class="format-flex-center">
                        <Avatar
                          v-bind:label="
                            slotProps.value.avatar
                              ? ''
                              : (slotProps.value.last_name ?? '').substring(
                                  0,
                                  1
                                )
                          "
                          v-bind:image="
                            slotProps.value.avatar
                              ? basedomainURL + slotProps.value.avatar
                              : basedomainURL + '/Portals/Image/noimg.jpg'
                          "
                          :style="{
                            background: bgColor[slotProps.value.is_order % 7],
                            color: '#ffffff',
                            width: '2rem',
                            height: '2rem',
                          }"
                          class="mr-2 text-avatar"
                          size="xlarge"
                          shape="circle"
                        />
                      </div>
                      <div class="format-flex-center">
                        <span>{{ slotProps.value.full_name }}</span>
                      </div>
                    </div>
                  </Chip>
                </div>
                <span v-else> {{ slotProps.placeholder }} </span>
              </template>
              <template #option="slotProps">
                <div v-if="slotProps.option" class="flex">
                  <div class="format-center">
                    <Avatar
                      v-bind:label="
                        slotProps.option.avatar
                          ? ''
                          : slotProps.option.last_name.substring(0, 1)
                      "
                      v-bind:image="
                        slotProps.option.avatar
                          ? basedomainURL + slotProps.option.avatar
                          : basedomainURL + '/Portals/Image/noimg.jpg'
                      "
                      :style="{
                        background: bgColor[slotProps.option.is_order % 7],
                        color: '#ffffff',
                        width: '3rem',
                        height: '3rem',
                        fontSize: '1.4rem !important',
                      }"
                      class="text-avatar"
                      size="xlarge"
                      shape="circle"
                    />
                  </div>
                  <div class="format-center text-left ml-3">
                    <div>
                      <div class="mb-1">
                        {{ slotProps.option.full_name }}
                      </div>
                      <div class="description">
                        <div>
                          <span>{{ slotProps.option.department_name }}</span>
                        </div>
                        <div>
                          <span>{{ slotProps.option.organization_name }}</span>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
                <span v-else> Chưa có dữ liệu </span>
              </template>
            </Dropdown>
          </div>
        </div>
        <div v-if="matchaccount.account == null" class="col-12 md:col-12">
          <div class="form-group">
            <h3 class="m-0">Tạo mới tài khoản (trường hợp chưa có tài khoản trong hệ thống)</h3>
          </div>
        </div>
        <div v-if="matchaccount.account == null" class="col-12 md:col-12">
          <div class="form-group">
            <label>Tên đăng nhập <span class="redsao">(*)</span></label>
            <InputText
              spellcheck="false"
              class="ip36"
              v-model="user.user_id"
              maxLength="50"
              :class="{
                'p-invalid': !user.user_id && submitted,
              }"
            />
            <div v-if="!user.user_id && submitted">
              <small class="p-error">
                <span>Tài khoản không được để trống</span>
              </small>
            </div>
          </div>
        </div>
        <div v-if="matchaccount.account == null" class="col-12 md:col-12">
          <div class="form-group">
            <label>Mật khẩu <span class="redsao">(*)</span></label>
            <Password
              style="cursor: pointer"
              v-model="user.is_psword"
              autocomplete="new-password"
              class="ip36"
              toggleMask
              :class="{
                'p-invalid': !user.is_psword && submitted,
              }"
            >
              <template #header>
                <h6>Chọn mật khẩu</h6>
              </template>
              <template #footer="sp">
                {{ sp.level }}
                <Divider />
                <p class="mt-2">Gợi ý</p>
                <ul class="pl-2 ml-2 mt-0" style="line-height: 1.5">
                  <li>Có ít nhất 1 chữ thường</li>
                  <li>Có ít nhất 1 chữ hoa</li>
                  <li>Có ít nhất 1 ký tự số</li>
                  <li>Tối thiểu 8 ký tự</li>
                </ul>
              </template>
            </Password>
            <div v-if="!user.is_psword && submitted">
              <small class="p-error">
                <span>Nật khẩuk hông được để trống</span>
              </small>
            </div>
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
::v-deep(.ip36) {
  input {
    width: 100%;
    height: 36px;
  }
}
</style>
