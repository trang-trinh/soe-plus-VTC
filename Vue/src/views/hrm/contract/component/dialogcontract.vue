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
  key: Number,
  headerDialog: String,
  displayDialog: Boolean,
  closeDialog: Function,
  isAdd: Boolean,
  isView: Boolean,
  model: Object,
  files: Array,
  selectFile: Function,
  removeFile: Function,
  dictionarys: Array,
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

//Function
const saveModel = (is_continue) => {
  submitted.value = true;
  if (
    !props.model.profile ||
    !props.model.type_contract_id ||
    !props.model.contract_code ||
    !props.model.start_date
  ) {
    swal.fire({
      title: "Thông báo!",
      text: "Vui lòng điền đầy đủ thông tin trường bôi đỏ!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  if (
    props.model.contract_name != null &&
    props.model.contract_name.length > 500
  ) {
    swal.fire({
      title: "Thông báo!",
      text: "Loại hợp đồng không được vượt quá 500 ký tự!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  if (props.model.employment != null && props.model.employment.length > 500) {
    swal.fire({
      title: "Thông báo!",
      text: "Nơi làm việc không được vượt quá 500 ký tự!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  if (props.model.note != null && props.model.note.length > 500) {
    swal.fire({
      title: "Thông báo!",
      text: "Ghi chú không được vượt quá 500 ký tự!",
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
  var obj = { ...props.model };
  var checkcontract = dictionarys.value[2].findIndex(
    (x) => x["type_contract_id"] === (obj["type_contract_id"] || "")
  );
  if (checkcontract === -1) {
    obj["contract_name"] = obj["type_contract_id"] || "";
    obj["type_contract_id"] = null;
  }
  if (obj.profile != null) {
    obj.profile_id = obj.profile.profile_id;
  }
  if (obj.sign_user != null) {
    obj.sign_user_id = obj.sign_user.profile_id;
  }
  if (obj.manager_user != null) {
    obj.manager_user_id = obj.manager_user.profile_id;
  }
  if (obj.start_date != null) {
    obj.start_date = moment(obj.start_date).format("YYYY-MM-DDTHH:mm:ssZZ");
  }
  if (obj.end_date != null) {
    obj.end_date = moment(obj.end_date).format("YYYY-MM-DDTHH:mm:ssZZ");
  }
  if (obj.sign_date != null) {
    obj.sign_date = moment(obj.sign_date).format("YYYY-MM-DDTHH:mm:ssZZ");
  }
  if (obj["professional_works"] != null) {
    obj["professional_works"] = obj["professional_works"]
      .map((x) => x)
      .join(",");
  }
  //Phụ cấp
  var allowances = [];
  var allowance_details = [];
  if (obj["allowances"] != null && obj["allowances"].length > 0) {
    obj["allowances"].forEach((allowance) => {
      allowances.push({
        allowance_id: allowance["allowance_id"],
        start_date: allowance["start_date"],
        note: allowance["note"],
      });
      if (
        allowance["formalitys"] != null &&
        allowance["formalitys"].length > 0
      ) {
        allowance["formalitys"].forEach((formality) => {
          var checkformality = dictionarys.value[8].findIndex(
            (x) =>
              x["allowance_formality_id"] ===
              (formality["allowance_formality_id"] || "")
          );
          if (checkformality === -1) {
            formality["allowance_formality"] =
              formality["allowance_formality_id"] || "";
            formality["allowance_formality_id"] = null;
          }
          allowance_details.push({
            allowance_id: allowance["allowance_id"],
            allowance_formality_id: formality["allowance_formality_id"],
            allowance_formality: formality["allowance_formality"],
            money: formality["money"],
            is_type: 0,
          });
        });
      }
      if (allowance["wages"] != null && allowance["wages"].length > 0) {
        allowance["wages"].forEach((wage) => {
          var checkwage = dictionarys.value[9].findIndex(
            (x) => x["allowance_wage_id"] === (wage["allowance_wage_id"] || "")
          );
          if (checkwage === -1) {
            wage["allowance_wage"] = wage["allowance_wage_id"] || "";
            wage["allowance_wage_id"] = null;
          }
          allowance_details.push({
            allowance_id: allowance["allowance_id"],
            allowance_wage_id: wage["allowance_wage_id"],
            allowance_wage: wage["allowance_wage"],
            money: wage["money"],
            is_type: 1,
          });
        });
      }
    });
  }
  let formData = new FormData();
  formData.append("isAdd", props.isAdd);
  formData.append("model", JSON.stringify(obj));
  formData.append("allowances", JSON.stringify(allowances));
  formData.append("allowance_details", JSON.stringify(allowance_details));
  for (var i = 0; i < props.files.length; i++) {
    let file = props.files[i];
    formData.append("files", file);
  }
  axios
    .put(baseURL + "/api/hrm_contract/update_contract", formData, config)
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
        props.model.profile = null;
        props.model.contract_code = "";
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
            .delete(baseURL + "/api/hrm_contract/delete_file", {
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
                  var idx = props.model.files.findIndex(
                    (x) => x.file_id == element
                  );
                  if (idx != -1) {
                    props.model.files.splice(idx, 1);
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

const pushAllowance = () => {
  props.model.allowances.push({
    allowance_id: CreateGuid(),
    start_date: new Date(),
    formalitys: [{}],
    wages: [{}],
  });
};

const removeAllwance = (allowance) => {
  var idx = props.model.allowances.findIndex(
    (x) => x["allowance_id"] === allowance["allowance_id"]
  );
  if (idx !== -1) {
    props.model.allowances.splice(idx, 1);
  }
};

const pushAllowanceDetail = (array, is_type) => {
  array.push({ is_type: is_type });
};

const removeAllowanceDetail = (array, idx) => {
  if (idx !== -1) {
    array.splice(idx, 1);
  }
};
//init
const genCode = () => {
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_contract_gencode",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "type_contract_id", va: props.model.type_contract_id },
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
          if (tbs[0] != null && tbs[0].length > 0) {
            props.model.contract_code = tbs[0][0].contract_code;
          }
        }
      }
    });
};
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
            props.model.employment =
              dictionarys.value[0] != null
                ? dictionarys.value[0][0].address
                : "";
          }
        }
      }
    });
};
onMounted(() => {
  if (props.displayDialog) {
    initDictionary();
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
    style="z-index: 9000"
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
            <div class="col-12 md:col-12">
              <div class="form-group">
                <label>Nhân sự <span class="redsao">(*)</span></label>
                <Dropdown
                  :options="dictionarys[1]"
                  :filter="true"
                  :showClear="true"
                  :editable="false"
                  optionLabel="profile_user_name"
                  placeholder="Chọn nhân sự"
                  v-model="props.model.profile"
                  class="ip36"
                  style="height: auto; min-height: 36px"
                  :class="{
                    'p-invalid': !props.model.profile && submitted,
                  }"
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
                          style="
                            background-color: #2196f3;
                            color: #ffffff;
                            width: 3rem;
                            height: 3rem;
                            font-size: 1.4rem !important;
                          "
                          :style="{
                            background: bgColor[slotProps.option.is_order % 7],
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
                <div v-if="!props.model.profile && submitted">
                  <small class="p-error">
                    <span class="col-12 p-0">Nhân sự không được để trống</span>
                  </small>
                </div>
              </div>
            </div>
            <div class="col-12 md:col-12">
              <div class="form-group">
                <label>Loại hợp đồng <span class="redsao">(*)</span></label>
                <Dropdown
                  @change="genCode()"
                  :options="dictionarys[2]"
                  :filter="true"
                  :showClear="true"
                  :editable="false"
                  v-model="props.model.type_contract_id"
                  optionLabel="type_contract_name"
                  optionValue="type_contract_id"
                  placeholder="Chọn hợp đồng"
                  class="ip36"
                  :class="{
                    'p-invalid': !props.model.type_contract_id && submitted,
                  }"
                  :style="{
                    whiteSpace: 'nowrap',
                    overflow: 'hidden',
                    textOverflow: 'ellipsis',
                  }"
                >
                  <template #option="slotProps">
                    <div class="country-item flex align-items-center">
                      <div class="pt-1 pl-2">
                        {{ slotProps.option.type_contract_name }}
                      </div>
                    </div>
                  </template>
                </Dropdown>
                <div v-if="!props.model.type_contract_id && submitted">
                  <small class="p-error">
                    <span class="col-12 p-0"
                      >Loại hợp đồng không được để trống</span
                    >
                  </small>
                </div>
              </div>
            </div>
            <div class="col-12 md:col-12">
              <div class="form-group">
                <label>Số hợp đồng <span class="redsao">(*)</span></label>
                <InputText
                  v-model="props.model.contract_code"
                  spellcheck="false"
                  class="ip36"
                  :style="{ backgroundColor: '#FEF9E7', fontWeight: 'bold' }"
                  maxLength="250"
                  :class="{
                    'p-invalid': !props.model.contract_code && submitted,
                  }"
                />
                <div v-if="!props.model.contract_code && submitted">
                  <small class="p-error">
                    <span class="col-12 p-0"
                      >Số hợp đồng không được để trống</span
                    >
                  </small>
                </div>
              </div>
            </div>
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
                      'p-invalid': !props.model.start_date && submitted,
                    }"
                    v-model="props.model.start_date"
                    placeholder="DD/MM/YYYY"
                  />
                </div>
                <div v-if="!props.model.start_date && submitted">
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
                    v-model="props.model.end_date"
                    placeholder="DD/MM/YYYY"
                    @date-select="changeEndDate()"
                  />
                </div>
              </div>
            </div>
            <div class="col-6 md:col-6">
              <div class="form-group">
                <label>Ngày ký</label>
                <div>
                  <Calendar
                    :showIcon="true"
                    class="ip36"
                    autocomplete="on"
                    inputId="time24"
                    v-model="props.model.sign_date"
                  />
                </div>
              </div>
            </div>
            <div class="col-6 md:col-6">
              <div class="form-group">
                <label>Người ký</label>
                <Dropdown
                  :options="dictionarys[8]"
                  :filter="true"
                  :showClear="true"
                  :editable="false"
                  optionLabel="profile_user_name"
                  placeholder="Chọn người ký"
                  v-model="props.model.sign_user"
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
            <div class="col-12 md:col-12">
              <div class="form-group">
                <label>Nơi ký </label>
                <InputText
                  v-model="props.model.sign_address"
                  spellcheck="false"
                  class="ip36"
                  maxLangth="500"
                />
              </div>
            </div>
            <div class="col-12 md:col-12">
              <div class="form-group">
                <label>Ghi chú</label>
                <Textarea
                  v-model="props.model.note"
                  :autoResize="true"
                  rows="5"
                  cols="30"
                  spellcheck="false"
                  class="ip36"
                  maxlength="500"
                />
              </div>
            </div>
            <!-- <div class="col-12 md:col-12">
              <div class="form-group">
                <h3 class="m-0">Thông tin phân công nhân sự</h3>
              </div>
            </div>
            <div class="col-12 md:col-12">
              <div class="form-group">
                <label>Phòng ban </label>
                <Dropdown
                  :options="dictionarys[3]"
                  :filter="true"
                  :showClear="true"
                  :editable="false"
                  v-model="props.model.department_id"
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
            <div class="col-12 md:col-12">
              <div class="form-group">
                <label>Chức danh </label>
                <Dropdown
                  :options="dictionarys[16]"
                  :filter="true"
                  :showClear="true"
                  :editable="false"
                  v-model="props.model.title_id"
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
            <div class="col-12 md:col-12">
              <div class="form-group">
                <label>Chức vụ </label>
                <Dropdown
                  :options="dictionarys[5]"
                  :filter="true"
                  :showClear="true"
                  :editable="false"
                  v-model="props.model.position_id"
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
                <label>Công việc chuyên môn </label>
                <MultiSelect
                  :options="dictionarys[11]"
                  v-model="props.model.professional_works"
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
            <div class="col-12 md:col-12">
              <div class="form-group">
                <label>Mô tả công việc </label>
                <Textarea
                  v-model="props.model.description"
                  :autoResize="true"
                  rows="5"
                  cols="30"
                  spellcheck="false"
                  class="ip36"
                  maxlength="500"
                />
              </div>
            </div>
            <div class="col-12 md:col-12">
              <div class="form-group">
                <label>Nơi làm việc </label>
                <InputText
                  v-model="props.model.employment"
                  spellcheck="false"
                  class="ip36"
                />
              </div>
            </div>
            <div class="col-12 md:col-12">
              <div class="form-group">
                <label>Hình thức làm việc </label>
                <Dropdown
                  :options="dictionarys[6]"
                  :filter="true"
                  :showClear="true"
                  :editable="false"
                  v-model="props.model.formality_id"
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
            <div class="col-12 md:col-12">
              <div class="form-group">
                <label>Người quản lý</label>
                <Dropdown
                  :options="dictionarys[8]"
                  :filter="true"
                  :showClear="true"
                  :editable="false"
                  optionLabel="profile_user_name"
                  placeholder="Chọn người quản lý"
                  v-model="props.model.manager_user"
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
                  v-model="props.model.personel_groups_id"
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
                  v-model="props.model.personnel_level_id"
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
            </div> -->

            <!-- <div class="col-6 md:col-6">
          <div class="form-group">
            <label>STT </label>
            <InputNumber
 v-model="props.model.is_order" class="ip36" />
          </div>
        </div>
        <div class="col-6 md:col-6 format-center">
          <div class="form-group">
            <div
              class="field-checkbox flex justify-content-center"
              style="height: 100%"
            >
              <InputSwitch v-model="props.model.status" />
              <label for="binary">Hiển thị</label>
            </div>
          </div>
        </div> -->
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="row">
            <div class="col-12 md:col-12">
              <div class="form-group">
                <h3 class="m-0">Lương và phụ cấp</h3>
              </div>
            </div>
            <div class="col-6 md:col-6 format-center">
              <div class="form-group">
                <div
                  class="field-checkbox flex justify-content-center"
                  style="height: 100%"
                >
                  <RadioButton
                    :inputId="'wage_type1'"
                    name="wage_type"
                    v-model="props.model.wage_type"
                    :value="1"
                  />
                  <label for="binary">Lương khoán</label>
                </div>
              </div>
            </div>
            <div class="col-6 md:col-6 format-center">
              <div class="form-group">
                <div
                  class="field-checkbox flex justify-content-center"
                  style="height: 100%"
                >
                  <RadioButton
                    :inputId="'wage_type2'"
                    name="wage_type"
                    v-model="props.model.wage_type"
                    :value="2"
                  />
                  <label for="binary">Lương hệ số</label>
                </div>
              </div>
            </div>
            <div class="col-6 md:col-6">
              <div class="form-group">
                <label>Dự kiến từ ngày <span class="redsao">(*)</span></label>
                <div>
                  <Calendar
                    :showIcon="true"
                    class="ip36"
                    autocomplete="on"
                    inputId="time24"
                    :class="{
                      'p-invalid': !props.model.wage_start_date && submitted,
                    }"
                    v-model="props.model.wage_start_date"
                    placeholder="DD/MM/YYYY"
                  />
                </div>
                <div v-if="!props.model.wage_start_date && submitted">
                  <small class="p-error">
                    <span class="col-12 p-0"
                      >Dự kiến từ ngày không được để trống</span
                    >
                  </small>
                </div>
              </div>
            </div>
            <div class="col-6 md:col-6">
              <div class="form-group">
                <label>Dự kiến đến ngày</label>
                <div>
                  <Calendar
                    :showIcon="true"
                    class="ip36"
                    autocomplete="on"
                    inputId="time24"
                    v-model="props.model.wage_end_date"
                    placeholder="DD/MM/YYYY"
                    @date-select="changeEndDate()"
                  />
                </div>
              </div>
            </div>
            <div v-if="props.model.wage_type === 1" class="col-12 md:col-12">
              <div class="form-group">
                <label>Tiền lương</label>
                <InputNumber
                  v-model="props.model.wage"
                  mode="decimal"
                  :minFractionDigits="0"
                  :maxFractionDigits="2"
                  placeholder="Số tiền"
                  class="ip36 text-right"
                />
              </div>
            </div>
            <div
              v-if="props.model.wage_type === 2"
              class="col-12 md:col-12 p-0"
            >
              <div class="row">
                <div class="col-6 md:col-6">
                  <div class="form-group">
                    <label>Ngạch lương </label>
                    <Dropdown
                      :options="dictionarys[7]"
                      :filter="true"
                      :showClear="true"
                      :editable="false"
                      v-model="props.model.wage_id"
                      optionLabel="wage_name"
                      optionValue="wage_id"
                      placeholder="Chọn ngạch lương"
                      class="ip36"
                    >
                      <template #option="slotProps">
                        <div class="country-item flex align-items-center">
                          <div class="pt-1 pl-2">
                            {{ slotProps.option.wage_name }}
                          </div>
                        </div>
                      </template>
                    </Dropdown>
                  </div>
                </div>
                <div class="col-6 md:col-6">
                  <div class="form-group">
                    <label>Bậc lương</label>
                    <InputNumber
                      showButtons
                      v-model="props.model.wage_level"
                      mode="decimal"
                      locale="vi-VN"
                      :minFractionDigits="0"
                      :maxFractionDigits="2"
                      class="ip36"
                    />
                  </div>
                </div>
                <div class="col-6 md:col-6">
                  <div class="form-group">
                    <label>Hệ số lương </label>
                    <Dropdown
                      :options="dictionarys[13]"
                      :filter="true"
                      :showClear="true"
                      :editable="false"
                      v-model="props.model.coef_salary_id"
                      optionLabel="coef_salary_name"
                      optionValue="coef_salary_id"
                      placeholder="Chọn hệ số lương"
                      class="ip36"
                    >
                      <template #option="slotProps">
                        <div class="country-item flex align-items-center">
                          <div class="pt-1 pl-2">
                            {{ slotProps.option.coef_salary_name }}
                          </div>
                        </div>
                      </template>
                    </Dropdown>
                  </div>
                </div>
                <div class="col-6 md:col-6">
                  <div class="form-group">
                    <label>Tiền lương TTV</label>
                    <InputNumber
                      v-model="props.model.wage"
                      mode="decimal"
                      :minFractionDigits="0"
                      :maxFractionDigits="2"
                      placeholder="Số tiền"
                      class="ip36 text-right"
                    />
                  </div>
                </div>
              </div>
            </div>
            <div
              v-for="(allowance, key) in props.model.allowances"
              :key="key"
              class="col-12 md:col-12 p-0"
            >
              <table class="table">
                <thead>
                  <tr>
                    <td>
                      <div class="col-12 md:col-12">
                        <label>Từ ngày</label>
                      </div>
                    </td>
                    <td>
                      <div class="col-12 md:col-12">
                        <label>Mô tả</label>
                      </div>
                    </td>
                    <td>
                      <ul
                        v-if="
                          !props.isView &&
                          key === props.model.allowances.length - 1
                        "
                        class="flex p-0 m-0 format-center"
                        style="list-style: none"
                      >
                        <li>
                          <a
                            @click="pushAllowance()"
                            class="hover"
                            v-tooltip.top="'Thêm lương phụ cấp'"
                          >
                            <i
                              class="pi pi-plus-circle"
                              style="font-size: 18px"
                            ></i>
                          </a>
                        </li>
                      </ul>
                    </td>
                  </tr>
                </thead>
                <tbody>
                  <tr>
                    <td>
                      <div class="col-12 md:col-12">
                        <div class="form-group">
                          <Calendar
                            :showIcon="true"
                            class="ip36"
                            autocomplete="on"
                            inputId="time24"
                            v-model="allowance.start_date"
                          />
                        </div>
                      </div>
                    </td>
                    <td>
                      <div class="col-12 md:col-12">
                        <div class="form-group">
                          <InputText
                            v-model="allowance.note"
                            spellcheck="false"
                            placeholder="Viết mô tả"
                            class="ip36"
                          />
                        </div>
                      </div>
                    </td>
                    <td>
                      <div
                        v-if="props.model.allowances.length > 1"
                        class="col-12 md:col-12"
                      >
                        <div class="form-group">
                          <ul
                            class="flex p-0 format-center"
                            style="list-style: none"
                          >
                            <li>
                              <a
                                @click="removeAllwance(allowance)"
                                class="hover"
                                v-tooltip.top="'Xóa lương phụ cấp'"
                              >
                                <i
                                  class="pi pi-times-circle"
                                  style="font-size: 18px"
                                ></i>
                              </a>
                            </li>
                          </ul>
                        </div>
                      </div>
                    </td>
                  </tr>
                </tbody>
              </table>
              <div class="ml-6">
                <!-- <table
                  v-for="(formality, key) in allowance.formalitys"
                  :key="key"
                  class="table"
                >
                  <thead>
                    <tr>
                      <td>
                        <div class="col-12 md:col-12">
                          <label>Hình thức lương</label>
                        </div>
                      </td>
                      <td>
                        <div class="col-12 md:col-12">
                          <label>Số tiền</label>
                        </div>
                      </td>
                      <td>
                        <ul
                          v-if="
                            !props.isView &&
                            key === allowance.formalitys.length - 1
                          "
                          class="flex p-0 m-0 format-center"
                          style="list-style: none"
                        >
                          <li>
                            <a
                              @click="
                                pushAllowanceDetail(allowance.formalitys, 0)
                              "
                              class="hover"
                              v-tooltip.top="'Thêm hình thức lương'"
                            >
                              <i
                                class="pi pi-plus-circle"
                                style="font-size: 18px"
                              ></i>
                            </a>
                          </li>
                        </ul>
                      </td>
                    </tr>
                  </thead>
                  <tbody>
                    <tr>
                      <td>
                        <div class="col-12 md:col-12">
                          <div class="form-group">
                            <Dropdown
                              :options="dictionarys[9]"
                              :filter="true"
                              :showClear="true"
                              :editable="true"
                              v-model="formality.allowance_formality_id"
                              optionLabel="allowance_formality_name"
                              optionValue="allowance_formality_id"
                              placeholder="Chọn hình thức"
                              class="ip36"
                            >
                              <template #option="slotProps">
                                <div
                                  class="country-item flex align-items-center"
                                >
                                  <div class="pt-1 pl-2">
                                    {{
                                      slotProps.option.allowance_formality_name
                                    }}
                                  </div>
                                </div>
                              </template>
                            </Dropdown>
                          </div>
                        </div>
                      </td>
                      <td>
                        <div class="col-12 md:col-12">
                          <div class="form-group">
                            <InputNumber
                              v-model="formality.money"
                              mode="decimal"
                              :minFractionDigits="0"
                              :maxFractionDigits="2"
                              placeholder="Nhập lương"
                              class="ip36 text-right"
                            />
                          </div>
                        </div>
                      </td>
                      <td>
                        <div
                          v-if="allowance.formalitys.length > 1"
                          class="col-12 md:col-12"
                        >
                          <div class="form-group">
                            <ul
                              class="flex p-0 format-center"
                              style="list-style: none"
                            >
                              <li>
                                <a
                                  @click="
                                    removeAllowanceDetail(
                                      allowance.formalitys,
                                      key
                                    )
                                  "
                                  class="hover"
                                  v-tooltip.top="'Xóa hình thức lương'"
                                >
                                  <i
                                    class="pi pi-times-circle"
                                    style="font-size: 18px"
                                  ></i>
                                </a>
                              </li>
                            </ul>
                          </div>
                        </div>
                      </td>
                    </tr>
                  </tbody>
                </table> -->
                <table
                  v-for="(wage, key) in allowance.wages"
                  :key="key"
                  class="table"
                >
                  <thead>
                    <tr>
                      <td>
                        <div class="col-12 md:col-12">
                          <label>Phụ cấp</label>
                        </div>
                      </td>
                      <td>
                        <div class="col-12 md:col-12">
                          <label v-if="props.model.wage_type === 1"
                            >Số tiền</label
                          >
                          <label v-else>Hệ số</label>
                        </div>
                      </td>
                      <td>
                        <ul
                          v-if="
                            !props.isView && key === allowance.wages.length - 1
                          "
                          class="flex p-0 m-0 format-center"
                          style="list-style: none"
                        >
                          <li>
                            <a
                              @click="pushAllowanceDetail(allowance.wages, 1)"
                              class="hover"
                              v-tooltip.top="'Thêm phụ cấp lương'"
                            >
                              <i
                                class="pi pi-plus-circle"
                                style="font-size: 18px"
                              ></i>
                            </a>
                          </li>
                        </ul>
                      </td>
                    </tr>
                  </thead>
                  <tbody>
                    <tr>
                      <td>
                        <div class="col-12 md:col-12">
                          <div class="form-group">
                            <Dropdown
                              :options="dictionarys[10]"
                              :filter="true"
                              :showClear="true"
                              :editable="true"
                              v-model="wage.allowance_wage_id"
                              optionLabel="allowance_wage_name"
                              optionValue="allowance_wage_id"
                              placeholder="Chọn phụ cấp"
                              class="ip36"
                            >
                              <template #option="slotProps">
                                <div
                                  class="country-item flex align-items-center"
                                >
                                  <div class="pt-1 pl-2">
                                    {{ slotProps.option.allowance_wage_name }}
                                  </div>
                                </div>
                              </template>
                            </Dropdown>
                          </div>
                        </div>
                      </td>
                      <td>
                        <div class="col-12 md:col-12">
                          <div class="form-group">
                            <InputNumber
                              v-model="wage.money"
                              mode="decimal"
                              :minFractionDigits="0"
                              :maxFractionDigits="2"
                              placeholder="Số tiền"
                              class="ip36 text-right"
                            />
                          </div>
                        </div>
                      </td>
                      <td>
                        <div
                          v-if="allowance.wages.length > 1"
                          class="col-12 md:col-12"
                        >
                          <div class="form-group">
                            <ul
                              class="flex p-0 format-center"
                              style="list-style: none"
                            >
                              <li>
                                <a
                                  @click="
                                    removeAllowanceDetail(allowance.wages, key)
                                  "
                                  class="hover"
                                  v-tooltip.top="'Xóa phụ cấp lương'"
                                >
                                  <i
                                    class="pi pi-times-circle"
                                    style="font-size: 18px"
                                  ></i>
                                </a>
                              </li>
                            </ul>
                          </div>
                        </div>
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>
            <div class="col-12 md:col-12">
              <div class="form-group">
                <label
                  ><h3 class="m-0">
                    Tài liệu đính kèm (quyết định,...)
                  </h3></label
                >
                <FileUpload
                  v-if="!props.isView"
                  :multiple="true"
                  :show-upload-button="false"
                  :show-cancel-button="true"
                  @remove="props.removeFile"
                  @select="props.selectFile"
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
                <div
                  v-if="
                    props.model.files != null && props.model.files.length > 0
                  "
                >
                  <DataView
                    :lazy="true"
                    :value="props.model.files"
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
