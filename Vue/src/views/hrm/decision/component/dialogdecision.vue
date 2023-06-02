<script setup>
import { onMounted, inject, ref } from "vue";
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
  key: Number,
  headerDialog: String,
  displayDialog: Boolean,
  closeDialog: Function,
  isAdd: Boolean,
  isCopy: Boolean,
  isView: Boolean,
  type_decision: Object,
  decision: Object,
  initData: Function,
});

//Declare
const display = ref(props.displayDialog);
const options = ref({
  loading: true,
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
const dictionarys = ref([]);
const reasons = ref([]);

//Function
const submitted = ref(false);
const model = ref({
  type_decision_id: props.type_decision.type_decision_id,
  type_decision_code: props.type_decision.type_decision_code,
  is_multiple: props.type_decision.is_multiple,
  decision_date: new Date(),
  start_date: new Date(),
});
const headerDialog = ref();
const displayDialog = ref(false);
const files = ref([]);
const saveModel = (is_continue) => {
  submitted.value = true;
  if (
    !model.value.profile ||
    !model.value.decision_code ||
    !model.value.decision_date
  ) {
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

  var obj = JSON.parse(JSON.stringify(model.value));
  if (obj.profile != null) {
    obj.profiles = obj.profile.map((x) => x.profile_id).join(",");
  }
  if (obj.decision_date != null) {
    obj.decision_date = moment(obj.decision_date).format("YYYY-MM-DDTHH:mm:ss");
  }
  if (obj.start_date != null) {
    obj.start_date = moment(obj.start_date).format("YYYY-MM-DDTHH:mm:ss");
  }
  if (obj.end_date != null) {
    obj.end_date = moment(obj.end_date).format("YYYY-MM-DDTHH:mm:ss");
  }
  var check = dictionarys.value[1].findIndex(
    (x) => x["discipline_id"] === (obj["discipline_id"] || "")
  );
  if (check === -1) {
    obj["reason"] = obj["discipline_id"] || "";
    obj["discipline_id"] = null;
  }

  let formData = new FormData();
  formData.append("isAdd", props.isAdd);
  formData.append("model", JSON.stringify(obj));
  for (var i = 0; i < files.value.length; i++) {
    let file = files.value[i];
    formData.append("files", file);
  }
  if (props.isCopy) {
    for (var i = 0; i < model.value.files.length; i++) {
      let file = files.value[i];
      formData.append("files", file);
    }
  }
  axios
    .put(baseURL + "/api/hrm_decision/update_decision", formData, config)
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
        model.decision_code = "";
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
const changeDate = () => {
  if (model.value.start_date > model.value.end_date) {
    model.value.end_date = null;
    swal.fire({
      title: "Thông báo!",
      text: "Hiệu lực từ ngày không được nhỏ hơn ngày hết hạn!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
};

//Function file
const removeFile = (event) => {
  files.value = [];
  event.files.forEach((element) => {
    files.value.push(element);
  });
};
const selectFile = (event) => {
  files.value = [];
  event.files.forEach((element) => {
    files.value.push(element);
  });
};
const deleteFile = (item, idx) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá tệp đính kèm này không!",
      icon: "warning",
      showCancelButton: true,
      confirmButtonColor: "#3085d6",
      cancelButtonColor: "#d33",
      confirmButtonText: "Có",
      cancelButtonText: "Không",
    })
    .then((result) => {
      if (result.isConfirmed) {
        swal.fire({
          width: 110,
          didOpen: () => {
            swal.showLoading();
          },
        });
        if (item["file_id"] != null) {
          var ids = [item["file_id"]];
          axios
            .delete(baseURL + "/api/hrm_decision/delete_file", {
              headers: { Authorization: `Bearer ${store.getters.token}` },
              data: ids,
            })
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
              if (ids.length > 0) {
                ids.forEach((element, i) => {
                  var idx = model.value.files.findIndex(
                    (x) => x.file_id == element
                  );
                  if (idx != -1) {
                    model.value.files.splice(idx, 1);
                  }
                });
              }
              swal.close();
              toast.success("Xoá tệp đính kèm thành công!");
            })
            .catch((error) => {
              swal.close();
              addLog({
                title: "Lỗi Console delItem",
                controller: "boardroom.vue",
                logcontent: error.message,
                loai: 2,
              });
              if (error.status === 401) {
                swal.fire({
                  title: "Thông báo!",
                  text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
                  icon: "error",
                  confirmButtonText: "OK",
                });
                return;
              }
            });
        } else {
          props.model.files.splice(idx, 1);
          swal.close();
        }
      }
    });
};
const removeMember = (arr, index) => {
  arr.splice(index, 1);
};

//init
const initDictionary = () => {
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_decision_dictionary",
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
          reasons.value = tbs[1].filter(
            (x) => x.type_decision_id === props.type_decision.type_decision_id
          );
        }
      }
    });
};
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
            proc: "hrm_decision_gets",
            par: [{ par: "decision_id", va: props.decision.decision_id }],
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
            if (model.value.profiles != null) {
              model.value.profile = JSON.parse(model.value.profile);
            }
            if (
              model.value.discipline_id == null &&
              model.value.reason != null
            ) {
              model.value.discipline_id = model.value.reason;
            }
            if (model.value.decision_date != null) {
              model.value.decision_date = new Date(model.value.decision_date);
            }
            if (model.value.start_date != null) {
              model.value.start_date = new Date(model.value.start_date);
            }
            if (model.value.end_date != null) {
              model.value.end_date = new Date(model.value.end_date);
            }
          } else {
            model.value = {};
          }
          if (tbs[1] != null && tbs[1].length > 0) {
            model.value.files = tbs[1];
          }
        }
      }
      swal.close();
      if (options.value.loading) options.value.loading = false;
    })
    .catch((error) => {
      swal.close();
      if (options.value.loading) options.value.loading = false;
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
    if (!props.isAdd || props.isCopy) {
      initData(true);
    }  
  }
});
</script>
<template>
  <Dialog
    :header="props.headerDialog"
    v-model:visible="display"
    :style="{ width: '70vw' }"
    :maximizable="true"
    :closable="true"
   
    :modal="true"
  >
    <form @submit.prevent="" name="submitform">
      <div class="grid formgrid m-2">
        <div class="col-6 md:col-6">
          <div class="row">
            <div class="col-12 md:col-12">
              <div class="form-group">
                <h3 class="m-0">Thông tin chung</h3>
              </div>
            </div>
            <div class="col-6 md:col-6">
              <div class="form-group">
                <label>Số quyết định <span class="redsao">(*)</span></label>
                <InputText
                  v-model="model.decision_code"
                  spellcheck="false"
                  class="ip36"
                  :style="{ backgroundColor: '#FEF9E7', fontWeight: 'bold' }"
                  :class="{
                    'p-invalid': !model.decision_code && submitted,
                  }"
                />
                <div v-if="!model.decision_code && submitted">
                  <small class="p-error">
                    <span class="col-12 p-0"
                      >Mã hợp đồng không được để trống</span
                    >
                  </small>
                </div>
              </div>
            </div>
            <!-- <div class="col-12 md:col-12">
              <div class="form-group">
                <label>Loại quyết định <span class="redsao">(*)</span></label>
                <Dropdown
                  :options="dictionarys[3]"
                  :filter="true"
                  :showClear="true"
                  :editable="false"
                  v-model="model.type_decision_id"
                  optionLabel="type_decision_name"
                  optionValue="type_decision_id"
                  placeholder="Chọn loại quyết định"
                  class="ip36"
                  :class="{
                    'p-invalid': !model.type_decision_id && submitted,
                  }"
                >
                  <template #option="slotProps">
                    <div class="country-item flex align-items-center">
                      <div class="pt-1 pl-2">
                        {{ slotProps.option.type_decision_name }}
                      </div>
                    </div>
                  </template>
                </Dropdown>
                <div v-if="!model.type_decision_id && submitted">
                  <small class="p-error">
                    <span class="col-12 p-0"
                      >Loại quyết định không được để trống</span
                    >
                  </small>
                </div>
              </div>
            </div> -->
            <div class="col-6 md:col-6">
              <div class="form-group">
                <label>Ngày quyết định <span class="redsao">(*)</span></label>
                <div>
                  <Calendar
                    :showIcon="true"
                    class="ip36"
                    autocomplete="on"
                    inputId="time24"
                    :class="{
                      'p-invalid': !model.decision_date && submitted,
                    }"
                    v-model="model.decision_date"
                    placeholder="DD/MM/YYYY"
                  />
                </div>
                <div v-if="!model.decision_date && submitted">
                  <small class="p-error">
                    <span class="col-12 p-0"
                      >Ngày quyết định không được để trống</span
                    >
                  </small>
                </div>
              </div>
            </div>
            <div class="col-12 md:col-12">
              <div class="form-group">
                <label>Nhân sự <span class="redsao">(*)</span></label>
                <MultiSelect
                  :options="dictionarys[2]"
                  :filter="true"
                  :showClear="true"
                  :editable="false"
                  :selectionLimit="model.is_multiple ? '' : 1"
                  optionLabel="full_name"
                  placeholder="Chọn nhân sự"
                  v-model="model.profile"
                  class="ip36"
                  style="height: auto; min-height: 36px"
                >
                  <template #value="slotProps">
                    <ul
                      class="p-ulchip"
                      v-if="slotProps.value && slotProps.value.length > 0"
                    >
                      <li
                        class="p-lichip"
                        v-for="(value, index) in slotProps.value"
                        :key="index"
                      >
                        <Chip
                          :image="value.avatar"
                          :label="value.profile_name"
                          class="mr-2 mb-2 pl-0"
                        >
                          <div class="flex">
                            <div class="format-flex-center">
                              <Avatar
                                v-bind:label="
                                  value.avatar
                                    ? ''
                                    : (value.profile_last_name ?? '').substring(
                                        0,
                                        1
                                      )
                                "
                                v-bind:image="
                                  value.avatar
                                    ? basedomainURL + value.avatar
                                    : basedomainURL + '/Portals/Image/noimg.jpg'
                                "
                                style="
                                  background-color: #2196f3;
                                  color: #ffffff;
                                  width: 2rem;
                                  height: 2rem;
                                "
                                :style="{
                                  background: bgColor[index % 7],
                                }"
                                class="mr-2 text-avatar"
                                size="xlarge"
                                shape="circle"
                              />
                            </div>
                            <div class="format-flex-center">
                              <span>{{ value.profile_name }}</span>
                            </div>
                            <span
                              tabindex="0"
                              class="p-chip-remove-icon pi pi-times-circle format-flex-center"
                              @click="removeMember(model.profile, index)"
                            ></span>
                          </div>
                        </Chip>
                      </li>
                    </ul>
                    <span v-else> {{ slotProps.placeholder }} </span>
                  </template>
                  <template #option="slotProps">
                    <div v-if="slotProps.option" class="flex">
                      <div class="format-center">
                        <Avatar
                          v-bind:label="
                            slotProps.option.avatar
                              ? ''
                              : slotProps.option.profile_last_name.substring(
                                  0,
                                  1
                                )
                          "
                          v-bind:image="
                            slotProps.option.avatar
                              ? basedomainURL + slotProps.option.avatar
                              : basedomainURL + '/Portals/Image/noimg.jpg'
                          "
                          :style="{
                            backgroundColor: bgColor[slotProps.index % 7],
                            color: '#ffffff',
                            width: '3rem',
                            height: '3rem',
                            fontSize: '1.2rem !important',
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
                    <span v-else> Chưa có dữ liệu tuần </span>
                  </template>
                </MultiSelect>
                <div v-if="!model.profile && submitted">
                  <small class="p-error">
                    <span class="col-12 p-0">Nhân sự không được để trống</span>
                  </small>
                </div>
              </div>
            </div>
            <div class="col-12 md:col-12">
              <div class="form-group">
                <label>Nơi lập </label>
                <InputText
                  v-model="model.place"
                  spellcheck="false"
                  class="ip36"
                />
              </div>
            </div>
            <div class="col-12 md:col-12">
              <div class="form-group">
                <label>Cấp ra QĐ </label>
                <Dropdown
                  :options="dictionarys[0]"
                  :filter="true"
                  :showClear="true"
                  :editable="false"
                  v-model="model.position_id"
                  optionLabel="position_name"
                  optionValue="position_id"
                  placeholder="Chọn cấp ra QĐ"
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
                <label>Lý do </label>
                <Dropdown
                  :options="reasons"
                  :filter="true"
                  :showClear="true"
                  :editable="true"
                  v-model="model.discipline_id"
                  optionLabel="discipline_name"
                  optionValue="discipline_id"
                  placeholder="Chọn lý do"
                  class="ip36"
                >
                  <template #option="slotProps">
                    <div class="country-item flex align-items-center">
                      <div class="pt-1 pl-2">
                        {{ slotProps.option.discipline_name }}
                      </div>
                    </div>
                  </template>
                </Dropdown>
              </div>
            </div>
            <div class="col-6 md:col-6">
              <div class="form-group">
                <label>Hiệu lực từ ngày</label>
                <div>
                  <Calendar
                    :showIcon="true"
                    class="ip36"
                    autocomplete="on"
                    inputId="time24"
                    v-model="model.start_date"
                    placeholder="DD/MM/YYYY"
                    @date-select="changeDate()"
                  />
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
                    @date-select="changeDate()"
                  />
                </div>
              </div>
            </div>
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="row">
            <template v-if="props.type_decision.decision_code">
              <div class="col-12 md:col-12">
                <div class="form-group">
                  <h3 class="m-0">Thông tin bổ nhiệm</h3>
                </div>
              </div>
              <div class="col-12 md:col-12">
                <div class="form-group">
                  <h3 class="m-0">Điều chỉnh lương phụ cấp</h3>
                </div>
              </div>
            </template>
            <div class="col-12 md:col-12">
              <div class="form-group">
                <label>Mô tả</label>
                <Textarea
                  v-model="model.description"
                  :autoResize="true"
                  rows="5"
                  cols="30"
                />
              </div>
            </div>
            <div class="col-12 md:col-12">
              <div class="form-group">
                <label><h3 class="m-0">Tài liệu đính kèm</h3></label>
                <FileUpload
                  :multiple="true"
                  :show-upload-button="false"
                  :show-cancel-button="true"
                  @remove="removeFile"
                  @select="selectFile"
                  name="demo[]"
                  url="./upload.php"
                  accept=""
                  choose-label="Chọn tệp"
                  cancel-label="Hủy"
                >
                  <template #empty>
                    <p>Kéo thả tệp đính kèm vào đây.</p>
                  </template>
                </FileUpload>
                <div v-if="model.files != null && model.files.length > 0">
                  <DataView
                    :lazy="true"
                    :value="model.files"
                    :rowHover="true"
                    :scrollable="true"
                    class="w-full h-full ptable p-datatable-sm flex flex-column"
                    layout="list"
                    responsiveLayout="scroll"
                  >
                    <template #list="slotProps">
                      <div class="w-full">
                        <Toolbar class="w-full">
                          <template #start>
                            <div
                              @click="goFile(slotProps.data)"
                              class="flex align-items-center"
                            >
                              <img
                                class="mr-2"
                                :src="
                                  basedomainURL +
                                  '/Portals/Image/file/' +
                                  slotProps.data.file_type +
                                  '.png'
                                "
                                style="object-fit: contain"
                                width="40"
                                height="40"
                              />
                              <span style="line-height: 1.5">
                                {{ slotProps.data.file_name }}</span
                              >
                            </div>
                          </template>
                          <template #end>
                            <Button
                              icon="pi pi-times"
                              class="p-button-rounded p-button-danger"
                              @click="
                                deleteFile(slotProps.data, slotProps.data.index)
                              "
                            />
                          </template>
                        </Toolbar>
                      </div>
                    </template>
                  </DataView>
                </div>
              </div>
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
