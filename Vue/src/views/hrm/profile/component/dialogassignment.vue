<script setup>
import moment from "moment";
import { onMounted, inject, ref } from "vue";
import { encr } from "../../../../util/function";
import { useToast } from "vue-toastification";

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
  isAdd: Boolean,
  profile_id: String,
  assignment: Object,
  initData: Function,
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

//Declare
const submitted = ref(false);
function CreateGuid() {
  function _p8(s) {
    var p = (Math.random().toString(16) + "000000000").substr(2, 8);
    return s ? "-" + p.substr(0, 4) + "-" + p.substr(4, 4) : p;
  }
  return _p8() + _p8(true) + _p8(true) + _p8();
}
const model = ref({
  profile_id: props.profile_id,
  is_active: true,
  is_main: true,
});

//Function
const saveModel = (is_continue) => {
  submitted.value = true;
  if (!model.value.start_date || !model.value.organization_id) {
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
  var obj = { ...model.value };
  if (obj.manager_user != null) {
    obj.manager_user_id = obj.manager_user.profile_id;
  }
  if (obj.start_date != null) {
    obj.start_date = moment(obj.start_date).format("YYYY-MM-DDTHH:mm:ssZZ");
  }
  if (obj.end_date != null) {
    obj.end_date = moment(obj.end_date).format("YYYY-MM-DDTHH:mm:ssZZ");
  }
  if (obj["professional_works"] != null) {
    obj["professional_works"] = obj["professional_works"]
      .map((x) => x)
      .join(",");
  }
  let formData = new FormData();
  formData.append("isAdd", props.isAdd);
  formData.append("model", JSON.stringify(obj));
  axios
    .put(baseURL + "/api/hrm_assignment/update_assignment", formData, config)
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
      toast.success(
        props.isAdd ? "Thêm mới thành công!" : "Cập nhật thành công!"
      );
      if (is_continue) {
      } else {
        props.closeDialog();
      }
      props.initData(true);
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
const initDictionary = () => {
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_contract_dictionary",
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
          if (dictionarys.value[0] && dictionarys.value[0].length > 0) {
            model.value.employment =
              dictionarys.value[0] != null
                ? dictionarys.value[0][0].address
                : "";
          }
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
            proc: "hrm_profile_assignment_get",
            par: [{ par: "assignment_id", va: props.assignment.assignment_id }],
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
          if (tbs[0] != null && tbs[0].length > 0) {
            model.value = tbs[0][0];
            if (model.value["manager_users"] != null) {
              model.value["manager_user"] = JSON.parse(
                model.value["manager_users"]
              )[0];
            }
            if (model.value["start_date"] != null) {
              model.value["start_date"] = new Date(model.value["start_date"]);
            }
            if (model.value["end_date"] != null) {
              model.value["end_date"] = new Date(model.value["end_date"]);
            }
            if (model.value["professional_works"] != null) {
              model.value["professional_works"] = model.value[
                "professional_works"
              ]
                .split(",")
                .map((x) => parseInt(x));
            }
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
    if (!props.isAdd) {
      initData(true);
    }
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
          <div class="row">
            <div class="col-6 md:col-6">
              <div class="form-group">
                <label>Hiệu lực từ ngày <span class="redsao">(*)</span></label>
                <div>
                  <Calendar
                    :showIcon="true"
                    class="ip36"
                    autocomplete="on"
                    inputId="time24"
                    :class="{
                      'p-invalid': !model.start_date && submitted,
                    }"
                    v-model="model.start_date"
                    placeholder="DD/MM/YYYY"
                  />
                </div>
                <div v-if="!model.start_date && submitted">
                  <small class="p-error">
                    <span class="col-12 p-0"
                      >Hiệu lực từ ngày không được để trống</span
                    >
                  </small>
                </div>
              </div>
            </div>
            <div class="col-6 md:col-6">
              <div class="form-group">
                <label>Hiệu lực đến ngày</label>
                <div>
                  <Calendar
                    :showIcon="true"
                    class="ip36"
                    autocomplete="on"
                    inputId="time24"
                    v-model="model.end_date"
                    placeholder="DD/MM/YYYY"
                    @date-select="changeEndDate()"
                  />
                </div>
              </div>
            </div>
            <div class="col-6 md:col-6">
              <div class="form-group">
                <label>Công ty <span class="redsao">(*)</span></label>
                <Dropdown
                  :options="dictionarys[12]"
                  :filter="true"
                  :showClear="true"
                  :editable="false"
                  v-model="model.organization_id"
                  optionLabel="organization_name"
                  optionValue="organization_id"
                  placeholder="Chọn công ty"
                  class="ip36"
                >
                  <template #option="slotProps">
                    <div class="country-item flex align-items-center">
                      <div class="pt-1 pl-2">
                        {{ slotProps.option.organization_name }}
                      </div>
                    </div>
                  </template>
                </Dropdown>
              </div>
            </div>
            <div class="col-6 md:col-6">
              <div class="form-group">
                <label>Phòng ban </label>
                <Dropdown
                  :options="dictionarys[3]"
                  :filter="true"
                  :showClear="true"
                  :editable="false"
                  v-model="model.department_id"
                  optionLabel="department_name"
                  optionValue="department_id"
                  placeholder="Chọn phòng ban"
                  class="ip36"
                >
                  <template #option="slotProps">
                    <div class="country-item flex align-items-center">
                      <div class="pt-1 pl-2">
                        {{ slotProps.option.department_name }}
                      </div>
                    </div>
                  </template>
                </Dropdown>
              </div>
            </div>
            <div class="col-6 md:col-6">
              <div class="form-group">
                <label>Chức danh </label>
                <Dropdown
                  :options="dictionarys[16]"
                  :filter="true"
                  :showClear="true"
                  :editable="false"
                  v-model="model.title_id"
                  optionLabel="title_name"
                  optionValue="title_id"
                  placeholder="Chọn chức danh"
                  class="ip36"
                >
                  <template #option="slotProps">
                    <div class="country-item flex align-items-center">
                      <div class="pt-1 pl-2">
                        {{ slotProps.option.title_name }}
                      </div>
                    </div>
                  </template>
                </Dropdown>
              </div>
            </div>
            <div class="col-6 md:col-6">
              <div class="form-group">
                <label>Chức vụ </label>
                <Dropdown
                  :options="dictionarys[5]"
                  :filter="true"
                  :showClear="true"
                  :editable="false"
                  v-model="model.position_id"
                  optionLabel="position_name"
                  optionValue="position_id"
                  placeholder="Chọn chức vụ"
                  class="ip36"
                >
                  <template #option="slotProps">
                    <div class="country-item flex align-items-center">
                      <div class="pt-1 pl-2">
                        {{ slotProps.option.position_name }}
                      </div>
                    </div>
                  </template>
                </Dropdown>
              </div>
            </div>

            <div class="col-12 md:col-12">
              <div class="form-group">
                <label>Mô tả công việc </label>
                <Textarea
                  v-model="model.description"
                  :autoResize="true"
                  rows="5"
                  cols="30"
                  spellcheck="false"
                  class="ip36"
                  maxlength="500"
                />
              </div>
            </div>
            <div class="col-6 md:col-6">
              <div class="form-group">
                <label>Công việc chuyên môn </label>
                <MultiSelect
                  :options="dictionarys[11]"
                  v-model="model.professional_works"
                  optionLabel="professional_work_name"
                  optionValue="professional_work_id"
                  placeholder="Chọn công việc chuyên môn"
                  class="w-full limit-width"
                  style="min-height: 36px"
                >
                  <template #value="slotProps">
                    <div
                      class="p-dropdown-car-value flex text-justify"
                      v-if="slotProps.value"
                    >
                      <div>
                        <div
                          v-for="(item, index) in slotProps.value"
                          :key="index"
                          class="p-1"
                          style="word-break: break-word"
                        >
                          <span
                            v-if="index > 0 && index < slotProps.value.length"
                            >,
                          </span>
                          <span
                            v-if="
                              dictionarys[11] != null &&
                              dictionarys[11].findIndex(
                                (x) => x.professional_work_id === parseInt(item)
                              ) !== -1
                            "
                          >
                            {{
                              dictionarys[11].find(
                                (x) =>
                                  x["professional_work_id"] === parseInt(item)
                              ).professional_work_name
                            }}
                          </span>
                        </div>
                      </div>
                    </div>
                    <span v-else>
                      {{ slotProps.placeholder }}
                    </span>
                  </template>
                </MultiSelect>
              </div>
            </div>
            <div class="col-6 md:col-6">
              <div class="form-group">
                <label>Nơi làm việc </label>
                <InputText
                  v-model="model.employment"
                  spellcheck="false"
                  class="ip36"
                  maxLength="500"
                />
              </div>
            </div>
            <div class="col-6 md:col-6">
              <div class="form-group">
                <label>Hình thức làm việc </label>
                <Dropdown
                  :options="dictionarys[6]"
                  :filter="true"
                  :showClear="true"
                  :editable="false"
                  v-model="model.formality_id"
                  optionLabel="formality_name"
                  optionValue="formality_id"
                  placeholder="Chọn hình thức"
                  class="ip36"
                >
                  <template #option="slotProps">
                    <div class="country-item flex align-items-center">
                      <div class="pt-1 pl-2">
                        {{ slotProps.option.formality_name }}
                      </div>
                    </div>
                  </template>
                </Dropdown>
              </div>
            </div>
            <div class="col-6 md:col-6">
              <div class="form-group">
                <label>Người quản lý</label>
                <Dropdown
                  :options="dictionarys[8]"
                  :filter="true"
                  :showClear="true"
                  :editable="false"
                  optionLabel="profile_user_name"
                  placeholder="Chọn người quản lý"
                  v-model="model.manager_user"
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
                                  : (
                                      slotProps.value.profile_user_name ?? ''
                                    ).substring(0, 1)
                              "
                              v-bind:image="
                                slotProps.value.avatar
                                  ? basedomainURL + slotProps.value.avatar
                                  : basedomainURL + '/Portals/Image/noimg.jpg'
                              "
                              style="background-color: #2196f3"
                              :style="{
                                background:
                                  bgColor[slotProps.value.is_order % 7],
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
                            <span>{{ slotProps.value.profile_user_name }}</span>
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
                              : slotProps.option.profile_user_name.substring(
                                  0,
                                  1
                                )
                          "
                          v-bind:image="
                            slotProps.option.avatar
                              ? basedomainURL + slotProps.option.avatar
                              : basedomainURL + '/Portals/Image/noimg.jpg'
                          "
                          style="background-color: #2196f3"
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
                            {{ slotProps.option.profile_user_name }}
                          </div>
                          <div class="description">
                            <div>
                              <span>{{ slotProps.option.profile_code }}</span
                              ><span v-if="slotProps.option.department_name">
                                | {{ slotProps.option.department_name }}</span
                              >
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
            <div class="col-6 md:col-6">
              <div class="form-group">
                <label>Loại nhân sự </label>
                <Dropdown
                  :options="dictionarys[14]"
                  :filter="true"
                  :showClear="true"
                  :editable="false"
                  v-model="model.personel_groups_id"
                  optionLabel="personel_groups_name"
                  optionValue="personel_groups_id"
                  placeholder="Chọn loại nhân sự"
                  class="ip36"
                >
                  <template #option="slotProps">
                    <div class="country-item flex align-items-center">
                      <div class="pt-1 pl-2">
                        {{ slotProps.option.personel_groups_name }}
                      </div>
                    </div>
                  </template>
                </Dropdown>
              </div>
            </div>
            <div class="col-6 md:col-6">
              <div class="form-group">
                <label>Cấp nhân sự </label>
                <Dropdown
                  :options="dictionarys[15]"
                  :filter="true"
                  :showClear="true"
                  :editable="false"
                  v-model="model.personnel_level_id"
                  optionLabel="personnel_level_name"
                  optionValue="personnel_level_id"
                  placeholder="Chọn cấp nhân sự"
                  class="ip36"
                >
                  <template #option="slotProps">
                    <div class="country-item flex align-items-center">
                      <div class="pt-1 pl-2">
                        {{ slotProps.option.personnel_level_name }}
                      </div>
                    </div>
                  </template>
                </Dropdown>
              </div>
            </div>
            <div class="col-4 md:col-4">
              <div class="form-group">
                <div
                  class="field-checkbox flex justify-content-center"
                  style="height: 100%"
                >
                  <InputSwitch v-model="model.is_main" />
                  <label for="binary">Làm chính</label>
                </div>
              </div>
            </div>
            <div class="col-4 md:col-4">
              <div class="form-group">
                <div
                  class="field-checkbox flex justify-content-center"
                  style="height: 100%"
                >
                  <InputSwitch v-model="model.is_experiment" />
                  <label for="binary">Kiêm nghiệm</label>
                </div>
              </div>
            </div>
            <div class="col-4 md:col-4">
              <div class="form-group">
                <div
                  class="field-checkbox flex justify-content-center"
                  style="height: 100%"
                >
                  <InputSwitch v-model="model.is_active" />
                  <label for="binary">Hiện hành</label>
                </div>
              </div>
            </div>
            <!-- <div class="col-6 md:col-6">
          <div class="form-group">
            <label>STT </label>
            <InputNumber
 v-model="model.is_order" class="ip36" />
          </div>
        </div>
        <div class="col-6 md:col-6 format-center">
          <div class="form-group">
            <div
              class="field-checkbox flex justify-content-center"
              style="height: 100%"
            >
              <InputSwitch v-model="model.status" />
              <label for="binary">Hiển thị</label>
            </div>
          </div>
        </div> -->
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
      <Button
        v-if="props.isAdd"
        label="Lưu và tiếp tục"
        icon="pi pi-check"
        @click="saveModel(true)"
      />
      <Button
        v-if="!props.isView"
        label="Lưu"
        icon="pi pi-check"
        @click="saveModel()"
      />
    </template>
  </Dialog>
</template>
<style scoped>
@import url(./stylehrm.css);
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
