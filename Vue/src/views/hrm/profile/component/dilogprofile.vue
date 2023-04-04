<script setup>
import { onMounted, inject, ref } from "vue";
import { encr } from "../../../../util/function";
import { useToast } from "vue-toastification";
import moment from "moment";
const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios");
const toast = useToast();
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const cryoptojs = inject("cryptojs");
const basedomainURL = baseURL;

//Get arguments
const props = defineProps({
  key: Number,
  headerDialog: String,
  displayDialog: Boolean,
  closeDialog: Function,
  isAdd: Boolean,
  model: Object,
  files: Array,
  chooseImage: Function,
  deleteImage: Function,
  handleFileAvtUpload: Function,
  selectFile: Function,
  removeFile: Function,
  addRow: Function,
  deleteRow: Function,
  datachilds: Array,
  dictionarys: Array,
  genders: Array,
  places: Array,
  marital_status: Array,
  initData: Function,
});

//Declare dictionary
const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);
const listPlaceDetails1 = ref([]);
const listPlaceDetails2 = ref([]);
const listPlaceDetails3 = ref([]);

//function
const submitted = ref(false);
const saveModel = (is_continue) => {
  submitted.value = true;
  if (
    !props.model.profile_id ||
    !props.model.profile_user_name ||
    !props.model.birthday ||
    !props.model.select_birthplace
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
  var obj = { ...props.model };
  if (obj["select_birthplace"] != null) {
    // obj["birthplace_id"] =
    //   Object.keys(obj["select_birthplace"])[0] == -1
    //     ? null
    //     : Object.keys(obj["select_birthplace"])[0];
    var checkname = listPlaceDetails1.value.findIndex(
      (x) => x["place_details_id"] === (obj["select_birthplace"] || "")
    );
    if (checkname === -1) {
      obj["birthplace_name"] = obj["select_birthplace"] || "";
      obj["birthplace_id"] = null;
    } else {
      obj["birthplace_id"] = obj["select_birthplace"];
    }
  }
  if (obj["select_birthplace_origin"] != null) {
    // obj["birthplace_origin_id"] =
    //   Object.keys(obj["select_birthplace_origin"])[0] == -1
    //     ? null
    //     : Object.keys(obj["select_birthplace_origin"])[0];
    var checkname = listPlaceDetails2.value.findIndex(
      (x) => x["place_details_id"] === (obj["select_birthplace_origin"] || "")
    );
    if (checkname === -1) {
      obj["birthplace_origin_name"] = obj["select_birthplace_origin"] || "";
      obj["birthplace_origin_id"] = null;
    } else {
      obj["birthplace_origin_id"] = obj["select_birthplace_origin"];
    }
  }
  if (obj["select_place_register_permanent"] != null) {
    // obj["place_register_permanent"] =
    //   Object.keys(obj["select_place_register_permanent"])[0] == -1
    //     ? null
    //     : Object.keys(obj["select_place_register_permanent"])[0];
    // obj["place_register_permanent_name"] = Object.keys(
    //   obj["select_place_register_permanent"]
    // )[1];
    var checkname = listPlaceDetails3.value.findIndex(
      (x) =>
        x["place_details_id"] === (obj["select_place_register_permanent"] || "")
    );
    if (checkname === -1) {
      obj["place_register_permanent_name"] =
        obj["select_place_register_permanent"] || "";
      obj["place_register_permanent"] = null;
    } else {
      obj["place_register_permanent"] = obj["select_place_register_permanent"];
    }
  }
  let formData = new FormData();
  formData.append("isAdd", props.isAdd);
  formData.append("model", JSON.stringify(obj));
  formData.append("relative", JSON.stringify(props.datachilds[1]));
  formData.append("skill", JSON.stringify(props.datachilds[2]));
  formData.append("clan_history", JSON.stringify(props.datachilds[3]));
  formData.append("experience", JSON.stringify(props.datachilds[4]));
  for (var i = 0; i < props.files.length; i++) {
    let file = props.files[i];
    if (file["key"] === "avatar") {
      formData.append("avatar", file);
    } else {
      formData.append("files", file);
    }
  }
  axios
    .put(baseURL + "/api/hrm_profile/update_profile", formData, config)
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
        props.model.contract_no = "";
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
const initPlaceFilter = (event, type) => {
  var stc = event.value;
  if (event.value == "") {
    stc = null;
  }
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "ca_place_details_list",
            par: [
              { par: "search", va: stc },
              { par: "pageno", va: 0 },
              { par: "pagesize", va: 50 },
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
        let data = JSON.parse(response.data.data);
        if (data != null) {
          if (data[0] != null && data[0].length > 0) {
            if (type == 1) {
              listPlaceDetails1.value = JSON.parse(JSON.stringify(data[0]));
            } else if (type == 2) {
              listPlaceDetails2.value = JSON.parse(JSON.stringify(data[0]));
            } else if (type == 3) {
              listPlaceDetails3.value = JSON.parse(JSON.stringify(data[0]));
            }
          } else {
            if (type == 1) {
              listPlaceDetails1.value = [];
            } else if (type == 2) {
              listPlaceDetails2.value = [];
            } else if (type == 3) {
              listPlaceDetails3.value = [];
            }
          }
        }
      }
    });
};
onMounted(() => {
  if (props.displayDialog && props.model != null) {
    initPlaceFilter({ value: props.model.birthplace_name }, 1);
    initPlaceFilter({ value: props.model.birthplace_origin_name }, 2);
    initPlaceFilter({ value: props.model.place_register_permanent_name }, 3);
  }
});
</script>
<template>
  <Dialog
    :header="props.headerDialog"
    v-model:visible="props.displayDialog"
    :style="{ width: '72vw' }"
    :maximizable="true"
    :closable="false"
    style="z-index: 9000"
  >
    <form @submit.prevent="" name="submitform">
      <div class="grid formgrid m-2">
        <div class="col-12 md:col-12">
          <Accordion class="w-full" :activeIndex="0">
            <!-- 1. Thông tin chung -->
            <AccordionTab>
              <template #header>
                <span>1. Thông tin chung</span>
              </template>
              <div class="col-12 md:col-12 p-0">
                <div class="row">
                  <div class="col-3 md:col-3 format-center">
                    <div class="form-group">
                      <div
                        class="inputanh2 relative mb-2"
                        style="margin: 0 auto"
                      >
                        <img
                          v-tooltip.top="'Chọn ảnh hồ sơ'"
                          @click="props.chooseImage('imgAvatar')"
                          id="avatar"
                          v-bind:src="
                            props.model.avatar
                              ? basedomainURL + props.model.avatar
                              : basedomainURL + '/Portals/Image/noimg.jpg'
                          "
                        />
                        <Button
                          v-if="props.model.avatar || props.model.isDisplayAvt"
                          style="width: 2rem; height: 2rem"
                          icon="pi pi-times"
                          @click="props.deleteImage('avatar')"
                          class="p-button-rounded absolute top-0 right-0 cursor-pointer"
                        />
                        <input
                          id="imgAvatar"
                          type="file"
                          accept="image/*"
                          @change="props.handleFileAvtUpload($event, 'avatar')"
                          style="display: none"
                        />
                      </div>
                    </div>
                  </div>
                  <div class="col-9 md:col-9 p-0">
                    <div class="row">
                      <div class="col-6 md:col-6">
                        <div class="form-group">
                          <label
                            >Mã nhân sự <span class="redsao">(*)</span></label
                          >
                          <InputText
                            spellcheck="false"
                            class="ip36"
                            v-model="props.model.profile_id"
                            v-bind:disabled="!props.isAdd"
                          />
                          <div v-if="!props.model.profile_id && submitted">
                            <small class="p-error">
                              <span>Mã nhân sự không được để trống</span>
                            </small>
                          </div>
                        </div>
                      </div>
                      <div class="col-6 md:col-6">
                        <div class="form-group">
                          <label>Mã chấm công</label>
                          <InputText
                            spellcheck="false"
                            class="ip36"
                            v-model="props.model.check_in_id"
                          />
                        </div>
                      </div>
                      <div class="col-6 md:col-6">
                        <div class="form-group">
                          <label>Mã quản lý cấp trên</label>
                          <InputText
                            spellcheck="false"
                            class="ip36"
                            v-model="props.model.superior_id"
                          />
                        </div>
                      </div>
                      <div class="col-6 md:col-6">
                        <div class="form-group">
                          <label>Ngày tuyển dụng</label>
                          <Calendar
                            class="ip36"
                            id="icon"
                            v-model="props.model.recruitment_date"
                            :showIcon="true"
                            placeholder="dd/mm/yyyy"
                          />
                        </div>
                      </div>
                      <div class="col-6 md:col-6">
                        <div class="form-group">
                          <label
                            >Họ và tên <span class="redsao">(*)</span></label
                          >
                          <InputText
                            spellcheck="false"
                            class="ip36"
                            v-model="props.model.profile_user_name"
                          />
                          <div
                            v-if="!props.model.profile_user_name && submitted"
                          >
                            <small class="p-error">
                              <span>Họ và tên không được để trống</span>
                            </small>
                          </div>
                        </div>
                      </div>
                      <div class="col-6 md:col-6">
                        <div class="form-group">
                          <label>Tên gọi khác</label>
                          <InputText
                            spellcheck="false"
                            class="ip36"
                            v-model="props.model.profile_nick_name"
                          />
                        </div>
                      </div>
                      <div class="col-6 md:col-6">
                        <div class="form-group">
                          <label
                            >Ngày sinh <span class="redsao">(*)</span></label
                          >
                          <Calendar
                            class="ip36"
                            id="icon"
                            v-model="props.model.birthday"
                            :showIcon="true"
                            placeholder="dd/mm/yyyy"
                          />
                          <div v-if="!props.model.birthday && submitted">
                            <small class="p-error">
                              <span>Ngày sinh không được để trống</span>
                            </small>
                          </div>
                        </div>
                      </div>
                      <div class="col-6 md:col-6">
                        <div class="form-group">
                          <label>Giới tính</label>
                          <Dropdown
                            :options="props.genders"
                            v-model="props.model.gender"
                            optionLabel="text"
                            optionValue="value"
                            placeholder="Chọn giới tính"
                            class="ip36"
                          />
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
              <div class="col-12 md:col-12">
                <div class="form-group">
                  <label>Nơi sinh</label>
                  <Dropdown
                    @filter="initPlaceFilter($event, 1)"
                    :options="listPlaceDetails1"
                    :filter="true"
                    :editable="true"
                    :showClear="true"
                    v-model="props.model.select_birthplace"
                    optionLabel="name"
                    optionValue="name"
                    class="ip36"
                    placeholder="Xã phường, Quận huyện, Tỉnh thành"
                    panelClass="d-design-dropdown"
                  />
                  <!-- <TreeSelect
                    :options="props.places"
                    :showClear="true"
                    :max-height="200"
                    v-model="props.model.select_birthplace"
                    placeholder="Chọn nơi sinh"
                    optionLabel="name"
                    optionValue="place_id"
                    class="ip36"
                  >
                  </TreeSelect> -->
                </div>
              </div>
              <div class="col-12 md:col-12">
                <div class="form-group">
                  <label>Quê quán</label>
                  <Dropdown
                    @filter="initPlaceFilter($event, 2)"
                    :options="listPlaceDetails2"
                    :filter="true"
                    :editable="true"
                    :showClear="true"
                    v-model="props.model.select_birthplace_origin"
                    optionLabel="name"
                    optionValue="name"
                    class="ip36"
                    placeholder="Xã phường, Quận huyện, Tỉnh thành"
                    panelClass="d-design-dropdown"
                  />
                  <!-- <TreeSelect
                    :options="props.places"
                    :showClear="true"
                    :max-height="200"
                    v-model="props.model.select_birthplace_origin"
                    placeholder="Chọn quê quán"
                    optionLabel="name"
                    optionValue="place_id"
                    class="ip36"
                  >
                  </TreeSelect> -->
                </div>
              </div>
              <div class="col-12 md:col-12">
                <div class="form-group">
                  <label>Nơi đăng ký HKTT</label>
                  <Dropdown
                    @filter="initPlaceFilter($event, 3)"
                    :options="listPlaceDetails3"
                    :filter="true"
                    :editable="true"
                    :showClear="true"
                    v-model="props.model.select_place_register_permanent"
                    optionLabel="name"
                    optionValue="name"
                    class="ip36"
                    placeholder="Xã phường, Quận huyện, Tỉnh thành"
                    panelClass="d-design-dropdown"
                  />
                  <!-- <TreeSelect
                    :options="props.places"
                    :showClear="true"
                    :max-height="200"
                    v-model="props.model.select_place_register_permanent"
                    placeholder="Chọn nơi đăng ký"
                    optionLabel="name"
                    optionValue="place_id"
                    class="ip36"
                  >
                  </TreeSelect> -->
                </div>
              </div>
              <div class="col-12 md:col-12 p-0">
                <div class="row">
                  <div class="col-3 md:col-3">
                    <div class="form-group">
                      <label>Loại giấy tờ</label>
                      <Dropdown
                        :showClear="true"
                        :options="props.dictionarys[0]"
                        optionLabel="identity_papers_name"
                        optionValue="identity_papers_id"
                        placeholder="Chọn loại"
                        class="ip36"
                        v-model="props.model.identity_papers_id"
                      />
                    </div>
                  </div>
                  <div class="col-3 md:col-3">
                    <div class="form-group">
                      <label>Số</label>
                      <InputText
                        spellcheck="false"
                        class="ip36"
                        v-model="props.model.identity_papers_code"
                        maxLength="50"
                      />
                    </div>
                  </div>
                  <div class="col-3 md:col-3">
                    <div class="form-group">
                      <label>Ngày cấp</label>
                      <Calendar
                        class="ip36"
                        id="icon"
                        v-model="props.model.identity_date_issue"
                        :showIcon="true"
                        placeholder="dd/mm/yyyy"
                      />
                    </div>
                  </div>
                  <div class="col-3 md:col-3">
                    <div class="form-group">
                      <label>Ngày hết hạn</label>
                      <Calendar
                        class="ip36"
                        id="icon"
                        v-model="props.model.identity_end_date_issue"
                        :showIcon="true"
                        placeholder="dd/mm/yyyy"
                      />
                    </div>
                  </div>
                  <div class="col-4 md:col-4">
                    <div class="form-group">
                      <label>Nơi cấp</label>
                      <Dropdown
                        :options="props.dictionarys[17]"
                        :showClear="true"
                        :filter="true"
                        v-model="props.model.identity_place_id"
                        placeholder="Chọn nơi cấp"
                        optionLabel="identity_place_name"
                        optionValue="identity_place_id"
                        class="ip36"
                      />
                    </div>
                  </div>
                  <div class="col-4 md:col-4">
                    <div class="form-group">
                      <label>Quốc tịch</label>
                      <Dropdown
                        :showClear="true"
                        :options="props.dictionarys[1]"
                        optionLabel="nationality_name"
                        optionValue="nationality_id"
                        placeholder="Chọn quốc tịch"
                        class="ip36"
                        v-model="props.model.nationality_id"
                        :filter="true"
                      />
                    </div>
                  </div>
                  <div class="col-4 md:col-4">
                    <div class="form-group">
                      <label>Tình trạng hôn nhân</label>
                      <Dropdown
                        :showClear="true"
                        :options="props.marital_status"
                        optionLabel="text"
                        optionValue="value"
                        placeholder="Chọn trạng thái"
                        class="ip36"
                        v-model="props.model.marital_status"
                      />
                    </div>
                  </div>
                  <div class="col-4 md:col-4">
                    <div class="form-group">
                      <label>Dân tộc</label>
                      <Dropdown
                        :showClear="true"
                        :options="props.dictionarys[2]"
                        optionLabel="ethnic_name"
                        optionValue="ethnic_id"
                        placeholder="Chọn dân tộc"
                        class="ip36"
                        v-model="props.model.ethnic_id"
                        :filter="true"
                      />
                    </div>
                  </div>
                  <div class="col-4 md:col-4">
                    <div class="form-group">
                      <label>Tôn giáo</label>
                      <Dropdown
                        :showClear="true"
                        :options="props.dictionarys[3]"
                        optionLabel="religion_name"
                        optionValue="religion_id"
                        placeholder="Chọn tôn giáo"
                        class="ip36"
                        v-model="props.model.religion_id"
                        :filter="true"
                      />
                    </div>
                  </div>
                  <div class="col-4 md:col-4">
                    <div class="form-group">
                      <label>Mã số thuế</label>
                      <InputText
                        spellcheck="false"
                        class="ip36"
                        v-model="props.model.tax_code"
                        maxLength="50"
                      />
                    </div>
                  </div>
                  <div class="col-4 md:col-4">
                    <div class="form-group">
                      <label>Ngân hàng</label>
                      <Dropdown
                        :showClear="true"
                        :options="props.dictionarys[4]"
                        optionLabel="bank_name"
                        optionValue="bank_id"
                        placeholder="Chọn ngân hàng"
                        class="ip36"
                        v-model="props.model.bank_id"
                        :filter="true"
                      />
                    </div>
                  </div>
                  <div class="col-4 md:col-4">
                    <div class="form-group">
                      <label>Số tài khoản</label>
                      <InputText
                        spellcheck="false"
                        class="ip36"
                        v-model="props.model.bank_number"
                        maxLength="50"
                      />
                    </div>
                  </div>
                  <div class="col-4 md:col-4">
                    <div class="form-group">
                      <label>Tên tài khoản</label>
                      <InputText
                        spellcheck="false"
                        class="ip36"
                        v-model="props.model.bank_account"
                        maxLength="50"
                      />
                    </div>
                  </div>
                </div>
              </div>
            </AccordionTab>
          </Accordion>
          <Accordion class="w-full" :multiple="true">
            <AccordionTab>
              <template #header>
                <!-- <i class="pi pi-book mr-2"></i> -->
                <span>2. Trình độ học vấn</span>
              </template>
              <div class="col-12 md:col-12">
                <div class="row">
                  <div class="col-6 md:col-6">
                    <div class="form-group">
                      <label>Trình độ phổ thông</label>
                      <Dropdown
                        :showClear="true"
                        :options="props.dictionarys[5]"
                        optionLabel="cultural_level_name"
                        optionValue="cultural_level_id"
                        placeholder="Chọn trình độ"
                        class="ip36"
                        v-model="props.model.cultural_level_id"
                      />
                    </div>
                  </div>
                  <div class="col-6 md:col-6">
                    <div class="form-group">
                      <label>Trình độ học vấn cao nhất</label>
                      <Dropdown
                        :showClear="true"
                        :options="props.dictionarys[6]"
                        optionLabel="academic_level_name"
                        optionValue="academic_level_id"
                        placeholder="Chọn trình độ"
                        class="ip36"
                        v-model="props.model.academic_level_id"
                      />
                    </div>
                  </div>
                  <div class="col-6 md:col-6">
                    <div class="form-group">
                      <label>Chuyên ngành học</label>
                      <Dropdown
                        :showClear="true"
                        :options="props.dictionarys[7]"
                        optionLabel="specialization_name"
                        optionValue="specialization_id"
                        placeholder="Chọn ngành"
                        class="ip36"
                        v-model="props.model.specialization_id"
                      />
                    </div>
                  </div>
                  <div class="col-6 md:col-6">
                    <div class="form-group">
                      <label>Quản lý nhà nước</label>
                      <Dropdown
                        :showClear="true"
                        :options="props.dictionarys[14]"
                        optionLabel="management_state_name"
                        optionValue="management_state_id"
                        placeholder="Chọn cấp"
                        class="ip36"
                        v-model="props.model.management_state_id"
                      />
                    </div>
                  </div>
                  <div class="col-4 md:col-4">
                    <div class="form-group">
                      <label>Lý luận chính trị</label>
                      <Dropdown
                        :showClear="true"
                        :options="props.dictionarys[8]"
                        optionLabel="political_theory_name"
                        optionValue="political_theory_id"
                        placeholder="Chọn cấp"
                        class="ip36"
                        v-model="props.model.political_theory_id"
                      />
                    </div>
                  </div>
                  <div class="col-4 md:col-4">
                    <div class="form-group">
                      <label>Ngoại ngữ</label>
                      <Dropdown
                        :showClear="true"
                        :options="props.dictionarys[9]"
                        optionLabel="language_level_name"
                        optionValue="language_level_id"
                        placeholder="Chọn cấp"
                        class="ip36"
                        v-model="props.model.language_level_id"
                      />
                    </div>
                  </div>
                  <div class="col-4 md:col-4">
                    <div class="form-group">
                      <label>Tin học</label>
                      <Dropdown
                        :showClear="true"
                        :options="props.dictionarys[10]"
                        optionLabel="informatic_level_name"
                        optionValue="informatic_level_id"
                        placeholder="Chọn cấp"
                        class="ip36"
                        v-model="props.model.informatic_level_id"
                      />
                    </div>
                  </div>
                </div>
              </div>
            </AccordionTab>
            <AccordionTab>
              <template #header>
                <!-- <i class="pi pi-info-circle mr-2"></i> -->
                <span>3. Thông tin liên hệ</span>
              </template>
              <div class="col-12 md:col-12">
                <div class="row">
                  <div class="col-6 md:col-6">
                    <div class="form-group">
                      <label>Số điện thoại</label>
                      <InputMask
                        v-model="props.model.phone"
                        mask="9999999999"
                        placeholder="__________"
                        class="ip36"
                      />
                    </div>
                  </div>
                  <div class="col-6 md:col-6">
                    <div class="form-group">
                      <label>Email</label>
                      <InputText
                        spellcheck="false"
                        class="ip36"
                        v-model="props.model.email"
                        maxLength="50"
                      />
                    </div>
                  </div>
                  <div class="col-12 md:col-12">
                    <div class="form-group">
                      <label>Thường trú</label>
                      <InputText
                        spellcheck="false"
                        class="ip36"
                        v-model="props.model.place_permanent"
                        maxLength="500"
                      />
                    </div>
                  </div>
                  <div class="col-12 md:col-12">
                    <div class="form-group">
                      <label>Chỗ ở hiện nay</label>
                      <InputText
                        spellcheck="false"
                        class="ip36"
                        v-model="props.model.place_residence"
                        maxLength="500"
                      />
                    </div>
                  </div>
                  <div class="col-12 md:col-12">
                    <div class="form-group">
                      <label class="m-0">Khi cần báo tin cho:</label>
                    </div>
                  </div>
                  <div class="col-4 md:col-4">
                    <div class="form-group">
                      <label>Họ và tên</label>
                      <InputText
                        spellcheck="false"
                        class="ip36"
                        v-model="props.model.involved_name"
                        maxLength="50"
                      />
                    </div>
                  </div>
                  <div class="col-4 md:col-4">
                    <div class="form-group">
                      <label>Số điện thoại</label>
                      <InputText
                        spellcheck="false"
                        class="ip36"
                        v-model="props.model.involved_phone"
                        maxLength="50"
                      />
                    </div>
                  </div>
                  <div class="col-4 md:col-4">
                    <div class="form-group">
                      <label>Mối quan hệ</label>
                      <Dropdown
                        :showClear="true"
                        :options="props.dictionarys[11]"
                        optionLabel="relationship_name"
                        optionValue="relationship_id"
                        placeholder="Chọn quan hệ"
                        v-model="props.model.relationship_id"
                        class="ip36"
                        style="
                          white-space: nowrap;
                          overflow: hidden;
                          text-overflow: ellipsis;
                        "
                      />
                    </div>
                  </div>
                  <div class="col-12 md:col-12">
                    <div class="form-group">
                      <label>Địa chỉ</label>
                      <InputText
                        spellcheck="false"
                        class="ip36"
                        v-model="props.model.involved_place"
                        maxLength="500"
                      />
                    </div>
                  </div>
                </div>
              </div>
            </AccordionTab>
            <!-- 4. Thông tin gia đình, người phụ thuộc -->
            <AccordionTab>
              <template #header>
                <Toolbar class="w-full custoolbar p-0 font-bold">
                  <template #start>
                    <!-- <i class="pi pi-users mr-2"></i> -->
                    <span
                      >4. Thông tin gia đình, người phụ thuộc</span
                    ></template
                  >
                  <template #end>
                    <a
                      @click="
                        props.addRow(1);
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
                  </template>
                </Toolbar>
              </template>
              <div class="col-12 md:col-12 p-0">
                <div style="">
                  <DataTable
                    :value="props.datachilds[1]"
                    :scrollable="true"
                    :lazy="true"
                    :rowHover="true"
                    :showGridlines="true"
                    scrollDirection="both"
                    class="empty-full"
                  >
                    <Column
                      header=""
                      headerStyle="text-align:center;width:50px"
                      bodyStyle="text-align:center;width:50px"
                      class="align-items-center justify-content-center text-center"
                    >
                      <template #body="slotProps">
                        <a
                          @click="props.deleteRow(1, slotProps.index)"
                          class="hover"
                          v-tooltip.top="'Xóa'"
                        >
                          <i
                            class="pi pi-times-circle"
                            style="font-size: 18px"
                          ></i>
                        </a>
                      </template>
                    </Column>
                    <Column
                      field="relative_name"
                      header="Họ tên"
                      headerStyle="text-align:center;width:180px;height:50px"
                      bodyStyle="text-align:center;width:180px;"
                      class="align-items-center justify-content-center text-center"
                    >
                      <template #body="slotProps">
                        <InputText
                          v-model="slotProps.data.relative_name"
                          spellcheck="false"
                          type="text"
                          class="ip36"
                          maxLength="25"
                        />
                      </template>
                    </Column>
                    <Column
                      field="relationship_id"
                      header="Quan hệ"
                      headerStyle="text-align:center;width:170px;height:50px"
                      bodyStyle="text-align:center;width:170px;"
                      class="align-items-center justify-content-center text-center"
                    >
                      <template #body="slotProps">
                        <div class="form-group m-0">
                          <Dropdown
                            :showClear="true"
                            :options="props.dictionarys[11]"
                            optionLabel="relationship_name"
                            optionValue="relationship_id"
                            placeholder="Chọn quan hệ"
                            v-model="slotProps.data.relationship_id"
                            class="ip36"
                            style="
                              white-space: nowrap;
                              overflow: hidden;
                              text-overflow: ellipsis;
                            "
                          />
                        </div>
                      </template>
                    </Column>
                    <Column
                      field="identification_date_issue"
                      header="Năm sinh"
                      headerStyle="text-align:center;width:120px;height:50px"
                      bodyStyle="text-align:center;width:120px;"
                      class="align-items-center justify-content-center text-center"
                    >
                      <template #body="slotProps">
                        <Calendar
                          v-model="slotProps.data.identification_date_issue"
                          :showIcon="false"
                          class="ip36"
                          placeholder="dd/mm/yyyy"
                        />
                      </template>
                    </Column>
                    <Column
                      field="phone"
                      header="SĐT"
                      headerStyle="text-align:center;width:120px;height:50px"
                      bodyStyle="text-align:center;width:120px;"
                      class="align-items-center justify-content-center text-center"
                    >
                      <template #body="slotProps">
                        <InputMask
                          v-model="slotProps.data.phone"
                          mask="9999999999"
                          placeholder="__________"
                          class="ip36"
                        />
                      </template>
                    </Column>
                    <Column
                      field="tax_code"
                      header="Mã số thuế"
                      headerStyle="text-align:center;width:150px;height:50px"
                      bodyStyle="text-align:center;width:150px;"
                      class="align-items-center justify-content-center text-center"
                    >
                      <template #body="slotProps">
                        <InputText
                          v-model="slotProps.data.tax_code"
                          spellcheck="false"
                          type="text"
                          class="ip36"
                          maxLength="50"
                        />
                      </template>
                    </Column>
                    <Column
                      field="identification_citizen"
                      header="CCCD/Hộ chiếu"
                      headerStyle="text-align:center;width:150px;height:50px"
                      bodyStyle="text-align:center;width:150px;"
                      class="align-items-center justify-content-center text-center"
                    >
                      <template #body="slotProps">
                        <InputText
                          v-model="slotProps.data.identification_citizen"
                          spellcheck="false"
                          type="text"
                          class="ip36"
                          maxLength="50"
                        />
                      </template>
                    </Column>
                    <Column
                      field="identification_date_issue"
                      header="Ngày cấp"
                      headerStyle="text-align:center;width:120px;height:50px"
                      bodyStyle="text-align:center;width:120px;"
                      class="align-items-center justify-content-center text-center"
                    >
                      <template #body="slotProps">
                        <Calendar
                          v-model="slotProps.data.identification_date_issue"
                          :showIcon="false"
                          class="ip36"
                          placeholder="dd/mm/yyyy"
                        />
                      </template>
                    </Column>
                    <Column
                      field="identification_place_issue"
                      header="Nơi cấp"
                      headerStyle="text-align:center;width:150px;height:50px"
                      bodyStyle="text-align:center;width:150px;"
                      class="align-items-center justify-content-center text-center"
                    >
                      <template #body="slotProps">
                        <InputText
                          v-model="slotProps.data.identification_place_issue"
                          spellcheck="false"
                          type="text"
                          class="ip36"
                          maxLength="250"
                        />
                      </template>
                    </Column>
                    <Column
                      field="is_dependent"
                      header="Phụ thuộc"
                      headerStyle="text-align:center;width:150px;height:50px"
                      bodyStyle="text-align:center;width:150px;"
                      class="align-items-center justify-content-center text-center"
                    >
                      <template #body="slotProps">
                        <div class="form-group m-0">
                          <Dropdown
                            :options="[
                              { value: 1, title: 'Có phụ thuộc' },
                              { value: 0, title: 'Không phụ thuộc' },
                            ]"
                            :filter="false"
                            :showClear="true"
                            :editable="false"
                            v-model="slotProps.data.is_dependent"
                            optionLabel="title"
                            optionValue="value"
                            placeholder="Chọn trạng thái"
                            class="ip36"
                            style="
                              white-space: nowrap;
                              overflow: hidden;
                              text-overflow: ellipsis;
                            "
                          >
                            <template #option="slotProps">
                              <div class="country-item flex align-items-center">
                                <div class="pt-1 pl-2">
                                  {{ slotProps.option.title }}
                                </div>
                              </div>
                            </template>
                          </Dropdown>
                        </div>
                      </template>
                    </Column>
                    <Column
                      field="start_date"
                      header="Từ ngày"
                      headerStyle="text-align:center;width:120px;height:50px"
                      bodyStyle="text-align:center;width:120px;"
                      class="align-items-center justify-content-center text-center"
                    >
                      <template #body="slotProps">
                        <Calendar
                          v-model="slotProps.data.start_date"
                          :showIcon="false"
                          class="ip36"
                          placeholder="dd/mm/yyyy"
                        />
                      </template>
                    </Column>
                    <Column
                      field="end_date"
                      header="Đến ngày"
                      headerStyle="text-align:center;width:120px;height:50px"
                      bodyStyle="text-align:center;width:120px;"
                      class="align-items-center justify-content-center text-center"
                    >
                      <template #body="slotProps">
                        <Calendar
                          v-model="slotProps.data.end_date"
                          :showIcon="false"
                          class="ip36"
                          placeholder="dd/mm/yyyy"
                        />
                      </template>
                    </Column>
                    <Column
                      field="info"
                      header="Thông tin cơ bản"
                      headerStyle="text-align:center;width:150px;height:50px"
                      bodyStyle="text-align:center;width:150px;"
                      class="align-items-center justify-content-center text-center"
                    >
                      <template #body="slotProps">
                        <InputText
                          v-model="slotProps.data.info"
                          spellcheck="false"
                          type="text"
                          class="ip36"
                        />
                      </template>
                    </Column>
                    <Column
                      field="note"
                      header="Ghi chú"
                      headerStyle="text-align:center;width:150px;height:50px"
                      bodyStyle="text-align:center;width:150px;"
                      class="align-items-center justify-content-center text-center"
                    >
                      <template #body="slotProps">
                        <InputText
                          v-model="slotProps.data.note"
                          spellcheck="false"
                          type="text"
                          class="ip36"
                        />
                      </template>
                    </Column>
                    <template #empty>
                      <div
                        class="align-items-center justify-content-center p-4 text-center m-auto"
                        style="display: flex; width: 100%"
                      ></div>
                    </template>
                  </DataTable>
                </div>
              </div>
            </AccordionTab>
            <!-- 5. Quá trình đào tạo, bồi dưỡng về chuyên môn, nghiệp vụ, lý luận chính trị, ngoại ngữ, tin học -->
            <AccordionTab>
              <template #header>
                <Toolbar class="w-full custoolbar p-0 font-bold">
                  <template #start>
                    <!-- <i class="pi pi-replay mr-2"></i> -->
                    <span
                      >5. Quá trình đào tạo, bồi dưỡng về chuyên môn, nghiệp vụ,
                      lý luận chính trị, ngoại ngữ, tin học</span
                    ></template
                  >
                  <template #end>
                    <a
                      @click="
                        props.addRow(2);
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
                  </template>
                </Toolbar>
              </template>
              <div class="col-12 md:col-12 p-0">
                <div style="">
                  <DataTable
                    :value="props.datachilds[2]"
                    :scrollable="true"
                    :lazy="true"
                    :rowHover="true"
                    :showGridlines="true"
                    scrollDirection="both"
                    class="empty-full"
                  >
                    <Column
                      header=""
                      headerStyle="text-align:center;width:50px"
                      bodyStyle="text-align:center;width:50px"
                      class="align-items-center justify-content-center text-center"
                    >
                      <template #body="slotProps">
                        <a
                          @click="props.deleteRow(2, slotProps.index)"
                          class="hover"
                          v-tooltip.top="'Xóa'"
                        >
                          <i
                            class="pi pi-times-circle"
                            style="font-size: 18px"
                          ></i>
                        </a>
                      </template>
                    </Column>
                    <Column
                      field="university_name"
                      header="Tên trường"
                      headerStyle="text-align:center;width:180px;height:50px"
                      bodyStyle="text-align:center;width:180px;"
                      class="align-items-center justify-content-center text-center"
                    >
                      <template #body="slotProps">
                        <InputText
                          v-model="slotProps.data.university_name"
                          spellcheck="false"
                          type="text"
                          class="ip36"
                          maxLength="250"
                        />
                      </template>
                    </Column>
                    <Column
                      field="specialized"
                      header="Chuyên ngành"
                      headerStyle="text-align:center;width:170px;height:50px"
                      bodyStyle="text-align:center;width:170px;"
                      class="align-items-center justify-content-center text-center"
                    >
                      <template #body="slotProps">
                        <div class="form-group m-0">
                          <Dropdown
                            :showClear="true"
                            :options="props.dictionarys[18]"
                            optionLabel="specialization_name"
                            optionValue="specialization_id"
                            placeholder="Chọn chuyên ngành"
                            v-model="slotProps.data.specialized"
                            class="ip36"
                            style="
                              white-space: nowrap;
                              overflow: hidden;
                              text-overflow: ellipsis;
                            "
                          />
                        </div>
                      </template>
                    </Column>
                    <Column
                      field="start_date"
                      header="Từ tháng, năm"
                      headerStyle="text-align:center;width:120px;height:50px"
                      bodyStyle="text-align:center;width:120px;"
                      class="align-items-center justify-content-center text-center"
                    >
                      <template #body="slotProps">
                        <Calendar
                          v-model="slotProps.data.start_date"
                          :showIcon="false"
                          view="month"
                          dateFormat="mm/yy"
                          class="ip36"
                          placeholder="mm/yyyy"
                        />
                      </template>
                    </Column>
                    <Column
                      field="end_date"
                      header="Đến tháng, năm"
                      headerStyle="text-align:center;width:120px;height:50px"
                      bodyStyle="text-align:center;width:120px;"
                      class="align-items-center justify-content-center text-center"
                    >
                      <template #body="slotProps">
                        <Calendar
                          v-model="slotProps.data.end_date"
                          :showIcon="false"
                          view="month"
                          dateFormat="mm/yy"
                          class="ip36"
                          placeholder="mm/yyyy"
                        />
                      </template>
                    </Column>
                    <Column
                      field="form_traning_id"
                      header="Hình thức đào tạo"
                      headerStyle="text-align:center;width:170px;height:50px"
                      bodyStyle="text-align:center;width:170px;"
                      class="align-items-center justify-content-center text-center"
                    >
                      <template #body="slotProps">
                        <div class="form-group m-0">
                          <Dropdown
                            :showClear="true"
                            :options="props.dictionarys[12]"
                            optionLabel="form_traning_name"
                            optionValue="form_traning_id"
                            placeholder="Chọn hình thức"
                            v-model="slotProps.data.form_traning_id"
                            class="ip36"
                            style="
                              white-space: nowrap;
                              overflow: hidden;
                              text-overflow: ellipsis;
                            "
                          />
                        </div>
                      </template>
                    </Column>
                    <Column
                      field="certificate_id"
                      header="Văn bằng, chứng chỉ"
                      headerStyle="text-align:center;width:170px;height:50px"
                      bodyStyle="text-align:center;width:170px;"
                      class="align-items-center justify-content-center text-center"
                    >
                      <template #body="slotProps">
                        <div class="form-group m-0">
                          <Dropdown
                            :showClear="true"
                            :options="props.dictionarys[13]"
                            optionLabel="certificate_name"
                            optionValue="certificate_id"
                            placeholder="Chọn văn bằng"
                            v-model="slotProps.data.certificate_id"
                            class="ip36"
                            style="
                              white-space: nowrap;
                              overflow: hidden;
                              text-overflow: ellipsis;
                            "
                          />
                        </div>
                      </template>
                    </Column>
                    <Column
                      field="certificate_start_date"
                      header="Ngày hiệu lực"
                      headerStyle="text-align:center;width:120px;height:50px"
                      bodyStyle="text-align:center;width:120px;"
                      class="align-items-center justify-content-center text-center"
                    >
                      <template #body="slotProps">
                        <Calendar
                          v-model="slotProps.data.certificate_start_date"
                          :showIcon="false"
                          class="ip36"
                          placeholder="dd/mm/yyyy"
                        />
                      </template>
                    </Column>
                    <Column
                      field="certificate_end_date"
                      header="Ngày hết hiệu lực"
                      headerStyle="text-align:center;width:120px;height:50px"
                      bodyStyle="text-align:center;width:120px;"
                      class="align-items-center justify-content-center text-center"
                    >
                      <template #body="slotProps">
                        <Calendar
                          v-model="slotProps.data.certificate_end_date"
                          :showIcon="false"
                          class="ip36"
                          placeholder="dd/mm/yyyy"
                        />
                      </template>
                    </Column>
                    <Column
                      field="certificate_key_code"
                      header="Số hiệu"
                      headerStyle="text-align:center;width:150px;height:50px"
                      bodyStyle="text-align:center;width:150px;"
                      class="align-items-center justify-content-center text-center"
                    >
                      <template #body="slotProps">
                        <InputText
                          v-model="slotProps.data.certificate_key_code"
                          spellcheck="false"
                          type="text"
                          class="ip36"
                          maxLength="50"
                        />
                      </template>
                    </Column>
                    <Column
                      field="certificate_version"
                      header="Phiên bản"
                      headerStyle="text-align:center;width:150px;height:50px"
                      bodyStyle="text-align:center;width:150px;"
                      class="align-items-center justify-content-center text-center"
                    >
                      <template #body="slotProps">
                        <InputText
                          v-model="slotProps.data.certificate_version"
                          spellcheck="false"
                          type="text"
                          class="ip36"
                          maxLength="25"
                        />
                      </template>
                    </Column>
                    <Column
                      field="certificate_release_time"
                      header="Lần phát hành"
                      headerStyle="text-align:center;width:150px;height:50px"
                      bodyStyle="text-align:center;width:150px;"
                      class="align-items-center justify-content-center text-center"
                    >
                      <template #body="slotProps">
                        <InputText
                          v-model="slotProps.data.certificate_release_time"
                          spellcheck="false"
                          type="text"
                          class="ip36"
                          maxLength="25"
                        />
                      </template>
                    </Column>
                    <template #empty>
                      <div
                        class="align-items-center justify-content-center p-4 text-center m-auto"
                        style="display: flex; width: 100%"
                      ></div>
                    </template>
                  </DataTable>
                </div>
              </div>
            </AccordionTab>
            <!-- 6. Lịch sử Đảng viên -->
            <AccordionTab>
              <template #header>
                <Toolbar class="w-full custoolbar p-0 font-bold">
                  <template #start>
                    <!-- <i class="pi pi-replay mr-2"></i> -->
                    <span>6. Lịch sử Đảng viên</span></template
                  >
                  <template #end>
                    <a
                      @click="
                        props.addRow(3);
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
                  </template>
                </Toolbar>
              </template>
              <div class="col-12 md:col-12 p-0">
                <div style="">
                  <DataTable
                    :value="props.datachilds[3]"
                    :scrollable="true"
                    :lazy="true"
                    :rowHover="true"
                    :showGridlines="true"
                    scrollDirection="both"
                    class="empty-full"
                  >
                    <Column
                      header=""
                      headerStyle="text-align:center;width:50px"
                      bodyStyle="text-align:center;width:50px"
                      class="align-items-center justify-content-center text-center"
                    >
                      <template #body="slotProps">
                        <a
                          @click="props.deleteRow(3, slotProps.index)"
                          class="hover"
                          v-tooltip.top="'Xóa'"
                        >
                          <i
                            class="pi pi-times-circle"
                            style="font-size: 18px"
                          ></i>
                        </a>
                      </template>
                    </Column>
                    <Column
                      field="card_number"
                      header="Số thẻ"
                      headerStyle="text-align:center;width:180px;height:50px"
                      bodyStyle="text-align:center;width:180px;"
                      class="align-items-center justify-content-center text-center"
                    >
                      <template #body="slotProps">
                        <InputText
                          v-model="slotProps.data.card_number"
                          spellcheck="false"
                          type="text"
                          class="ip36"
                          maxLength="50"
                        />
                      </template>
                    </Column>
                    <Column
                      field="form"
                      header="Hình thức"
                      headerStyle="text-align:center;width:170px;height:50px"
                      bodyStyle="text-align:center;width:170px;"
                      class="align-items-center justify-content-center text-center"
                    >
                      <template #body="slotProps">
                        <div class="form-group m-0">
                          <Dropdown
                            :showClear="true"
                            :editable="false"
                            :options="[
                              { value: 0, title: 'Dự bị' },
                              { value: 1, title: 'Chính thức' },
                              { value: 1, title: 'Điều chuyển' },
                            ]"
                            optionLabel="title"
                            optionValue="value"
                            placeholder="Chọn chuyên ngành"
                            v-model="slotProps.data.form"
                            class="ip36"
                            style="
                              white-space: nowrap;
                              overflow: hidden;
                              text-overflow: ellipsis;
                            "
                          />
                        </div>
                      </template>
                    </Column>
                    <Column
                      field="start_date"
                      header="Từ ngày"
                      headerStyle="text-align:center;width:120px;height:50px"
                      bodyStyle="text-align:center;width:120px;"
                      class="align-items-center justify-content-center text-center"
                    >
                      <template #body="slotProps">
                        <Calendar
                          v-model="slotProps.data.start_date"
                          :showIcon="false"
                          class="ip36"
                          placeholder="dd/mm/yyyy"
                        />
                      </template>
                    </Column>
                    <Column
                      field="end_date"
                      header="Đến ngày"
                      headerStyle="text-align:center;width:120px;height:50px"
                      bodyStyle="text-align:center;width:120px;"
                      class="align-items-center justify-content-center text-center"
                    >
                      <template #body="slotProps">
                        <Calendar
                          v-model="slotProps.data.end_date"
                          :showIcon="false"
                          class="ip36"
                          placeholder="dd/mm/yyyy"
                        />
                      </template>
                    </Column>
                    <Column
                      field="admission_place"
                      header="Nơi kết nạp"
                      headerStyle="text-align:center;width:180px;height:50px"
                      bodyStyle="text-align:center;width:180px;"
                      class="align-items-center justify-content-center text-center"
                    >
                      <template #body="slotProps">
                        <InputText
                          v-model="slotProps.data.admission_place"
                          spellcheck="false"
                          type="text"
                          class="ip36"
                          maxLength="250"
                        />
                      </template>
                    </Column>
                    <Column
                      field="transfer_place"
                      header="Nơi điều chuyển"
                      headerStyle="text-align:center;width:180px;height:50px"
                      bodyStyle="text-align:center;width:180px;"
                      class="align-items-center justify-content-center text-center"
                    >
                      <template #body="slotProps">
                        <InputText
                          v-model="slotProps.data.transfer_place"
                          spellcheck="false"
                          type="text"
                          class="ip36"
                          maxLength="250"
                        />
                      </template>
                    </Column>
                    <template #empty>
                      <div
                        class="align-items-center justify-content-center p-4 text-center m-auto"
                        style="display: flex; width: 100%"
                      ></div>
                    </template>
                  </DataTable>
                </div>
              </div>
            </AccordionTab>
            <!-- 7. Lịch sử tham gia quân đội -->
            <AccordionTab>
              <template #header>
                <!-- <i class="pi pi-chart-line mr-2"></i> -->
                <span>7. Lịch sử tham gia quân đội</span>
              </template>
              <div class="col-12 md:col-12">
                <div class="row">
                  <div class="col-6 md:col-6">
                    <div class="form-group">
                      <label>Ngày nhập ngũ</label>
                      <Calendar
                        :showIcon="true"
                        v-model="props.model.military_start_date"
                        class="ip36"
                        id="icon"
                      />
                    </div>
                  </div>
                  <div class="col-6 md:col-6">
                    <div class="form-group">
                      <label>Ngày xuất ngũ</label>
                      <Calendar
                        :showIcon="true"
                        v-model="props.model.military_end_date"
                        class="ip36"
                        id="icon"
                      />
                    </div>
                  </div>
                  <div class="col-6 md:col-6">
                    <div class="form-group">
                      <label>Quân hàm cao nhất</label>
                      <InputText
                        spellcheck="false"
                        class="ip36"
                        v-model="props.model.military_rank"
                        maxLength="50"
                      />
                    </div>
                  </div>
                  <div class="col-6 md:col-6">
                    <div class="form-group">
                      <label>Danh hiệu cao nhất</label>
                      <InputText
                        spellcheck="false"
                        class="ip36"
                        v-model="props.model.military_title"
                        maxLength="250"
                      />
                    </div>
                  </div>
                  <div class="col-6 md:col-6">
                    <div class="form-group">
                      <label>Sở trường công tác</label>
                      <InputText
                        spellcheck="false"
                        class="ip36"
                        v-model="props.model.military_forte"
                        maxLength="250"
                      />
                    </div>
                  </div>
                  <div class="col-6 md:col-6">
                    <div class="form-group">
                      <label>Sức khỏe</label>
                      <InputText
                        spellcheck="false"
                        class="ip36"
                        v-model="props.model.military_health"
                        maxLength="250"
                      />
                    </div>
                  </div>
                  <div class="col-6 md:col-6">
                    <div class="form-group">
                      <label>Khen thưởng</label>
                      <InputText
                        spellcheck="false"
                        class="ip36"
                        v-model="props.model.military_reward"
                        maxLength="250"
                      />
                    </div>
                  </div>
                  <div class="col-6 md:col-6">
                    <div class="form-group">
                      <label>Kỷ luật</label>
                      <InputText
                        spellcheck="false"
                        class="ip36"
                        v-model="props.model.military_discipline"
                        maxLength="250"
                      />
                    </div>
                  </div>
                  <div class="col-6 md:col-6">
                    <div class="form-group">
                      <label>Thương binh hạng</label>
                      <InputText
                        spellcheck="false"
                        class="ip36"
                        v-model="props.model.military_veterans_rank"
                        maxLength="50"
                      />
                    </div>
                  </div>
                  <div class="col-6 md:col-6">
                    <div class="form-group">
                      <label>Con gia đình chính sách</label>
                      <InputText
                        spellcheck="false"
                        class="ip36"
                        v-model="props.model.military_policy_family"
                        maxLength="250"
                      />
                    </div>
                  </div>
                </div>
              </div>
            </AccordionTab>
            <!-- 8. Kinh nghiệm làm việc -->
            <AccordionTab>
              <template #header>
                <Toolbar class="w-full custoolbar p-0 font-bold">
                  <template #start>
                    <span
                      >8. Quá trình công tác (đơn vị cũ)/Kinh nghiệm làm
                      việc</span
                    ></template
                  >
                  <template #end>
                    <a
                      @click="
                        props.addRow(4);
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
                  </template>
                </Toolbar>
              </template>
              <div class="col-12 md:col-12 p-0">
                <div style="">
                  <DataTable
                    :value="props.datachilds[4]"
                    :scrollable="true"
                    :lazy="true"
                    :rowHover="true"
                    :showGridlines="true"
                    scrollDirection="both"
                    class="empty-full"
                  >
                    <Column
                      header=""
                      headerStyle="text-align:center;width:50px"
                      bodyStyle="text-align:center;width:50px"
                      class="align-items-center justify-content-center text-center"
                    >
                      <template #body="slotProps">
                        <a
                          @click="props.deleteRow(4, slotProps.index)"
                          class="hover"
                          v-tooltip.top="'Xóa'"
                        >
                          <i
                            class="pi pi-times-circle"
                            style="font-size: 18px"
                          ></i>
                        </a>
                      </template>
                    </Column>
                    <Column
                      field="start_date"
                      header="Từ tháng, năm"
                      headerStyle="text-align:center;width:120px;height:50px"
                      bodyStyle="text-align:center;width:120px;"
                      class="align-items-center justify-content-center text-center"
                    >
                      <template #body="slotProps">
                        <Calendar
                          v-model="slotProps.data.start_date"
                          :showIcon="false"
                          view="month"
                          dateFormat="mm/yy"
                          class="ip36"
                          placeholder="mm/yyyy"
                        />
                      </template>
                    </Column>
                    <Column
                      field="end_date"
                      header="Đến tháng, năm"
                      headerStyle="text-align:center;width:120px;height:50px"
                      bodyStyle="text-align:center;width:120px;"
                      class="align-items-center justify-content-center text-center"
                    >
                      <template #body="slotProps">
                        <Calendar
                          v-model="slotProps.data.end_date"
                          :showIcon="false"
                          view="month"
                          dateFormat="mm/yy"
                          class="ip36"
                          placeholder="mm/yyyy"
                        />
                      </template>
                    </Column>
                    <Column
                      field="company"
                      header="Công ty, đơn vị"
                      headerStyle="text-align:center;width:180px;height:50px"
                      bodyStyle="text-align:center;width:180px;"
                      class="align-items-center justify-content-center text-center"
                    >
                      <template #body="slotProps">
                        <InputText
                          v-model="slotProps.data.company"
                          spellcheck="false"
                          type="text"
                          class="ip36"
                          maxLength="250"
                        />
                      </template>
                    </Column>
                    <Column
                      field="address"
                      header="Địa chỉ"
                      headerStyle="text-align:center;width:180px;height:50px"
                      bodyStyle="text-align:center;width:180px;"
                      class="align-items-center justify-content-center text-center"
                    >
                      <template #body="slotProps">
                        <InputText
                          v-model="slotProps.data.address"
                          spellcheck="false"
                          type="text"
                          class="ip36"
                          maxLength="500"
                        />
                      </template>
                    </Column>
                    <Column
                      field="role"
                      header="Vị trí"
                      headerStyle="text-align:center;width:150px;height:50px"
                      bodyStyle="text-align:center;width:150px;"
                      class="align-items-center justify-content-center text-center"
                    >
                      <template #body="slotProps">
                        <InputText
                          v-model="slotProps.data.role"
                          spellcheck="false"
                          type="text"
                          class="ip36"
                          maxLength="50"
                        />
                      </template>
                    </Column>
                    <Column
                      field="wage"
                      header="Mức lương"
                      headerStyle="text-align:center;width:150px;height:50px"
                      bodyStyle="text-align:center;width:150px;"
                      class="align-items-center justify-content-center text-center"
                    >
                      <template #body="slotProps">
                        <InputText
                          v-model="slotProps.data.wage"
                          spellcheck="false"
                          type="text"
                          class="ip36"
                          maxLength="500"
                        />
                      </template>
                    </Column>
                    <Column
                      field="reference_name"
                      header="Người tham chiếu"
                      headerStyle="text-align:center;width:150px;height:50px"
                      bodyStyle="text-align:center;width:150px;"
                      class="align-items-center justify-content-center text-center"
                    >
                      <template #body="slotProps">
                        <InputText
                          v-model="slotProps.data.reference_name"
                          spellcheck="false"
                          type="text"
                          class="ip36"
                          maxLength="50"
                        />
                      </template>
                    </Column>
                    <Column
                      field="reference_phone"
                      header="SĐT"
                      headerStyle="text-align:center;width:120px;height:50px"
                      bodyStyle="text-align:center;width:120px;"
                      class="align-items-center justify-content-center text-center"
                    >
                      <template #body="slotProps">
                        <InputMask
                          v-model="slotProps.data.reference_phone"
                          mask="9999999999"
                          placeholder="__________"
                          class="ip36"
                        />
                      </template>
                    </Column>
                    <Column
                      field="description"
                      header="Mô tả công việc"
                      headerStyle="text-align:center;width:200px;height:50px"
                      bodyStyle="text-align:center;width:200px;"
                      class="align-items-center justify-content-center text-center"
                    >
                      <template #body="slotProps">
                        <InputText
                          v-model="slotProps.data.description"
                          spellcheck="false"
                          type="text"
                          class="ip36"
                          maxLength="500"
                        />
                      </template>
                    </Column>
                    <Column
                      field="reason"
                      header="Lý do nghỉ việc"
                      headerStyle="text-align:center;width:200px;height:50px"
                      bodyStyle="text-align:center;width:200px;"
                      class="align-items-center justify-content-center text-center"
                    >
                      <template #body="slotProps">
                        <InputText
                          v-model="slotProps.data.reason"
                          spellcheck="false"
                          type="text"
                          class="ip36"
                          maxLength="500"
                        />
                      </template>
                    </Column>
                    <template #empty>
                      <div
                        class="align-items-center justify-content-center p-4 text-center m-auto"
                        style="display: flex; width: 100%"
                      ></div>
                    </template>
                  </DataTable>
                </div>
              </div>
            </AccordionTab>
            <!-- Đặc điểm lịch sử bản thân -->
            <AccordionTab>
              <template #header>
                <!-- <i class="pi pi-chart-line mr-2"></i> -->
                <span>9. Đặc điểm lịch sử bản thân</span>
              </template>
              <div class="col-12 md:col-12">
                <div class="form-group">
                  <label>Nhập thông tin</label>
                  <Textarea
                    :autoResize="true"
                    rows="4"
                    placeholder="Khai rõ: Bị bắt, bị tù, bản thân có làm việc trong chế độ cũ"
                    v-model="props.model.biography_first"
                  />
                </div>
              </div>
              <div class="col-12 md:col-12">
                <div class="form-group">
                  <label>Nhập thông tin</label>
                  <Textarea
                    :autoResize="true"
                    rows="4"
                    placeholder="Tham gia hoặc có quan hệ với các tổ chức chính trị, kinh tế, xã hội nào ở nước ngoài"
                    v-model="props.model.biography_second"
                  />
                </div>
              </div>
              <div class="col-12 md:col-12">
                <div class="form-group">
                  <label>Nhập thông tin</label>
                  <Textarea
                    :autoResize="true"
                    rows="4"
                    placeholder="Có thân nhân ở nước ngoài (làm gì, địa chỉ)"
                    v-model="props.model.biography_third"
                  />
                </div>
              </div>
            </AccordionTab>
            <!-- 10.	Đính kèm khác (file số hóa liên quan) -->
            <AccordionTab>
              <template #header>
                <!-- <i class="pi pi-chart-line mr-2"></i> -->
                <span> 10. Đính kèm khác (file số hóa liên quan)</span>
              </template>
              <div class="col-12 md:col-12">
                <div class="form-group">
                  <label>Tải file lên </label>
                  <FileUpload
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
                                  deleteFile(
                                    slotProps.data,
                                    slotProps.data.index
                                  )
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
            </AccordionTab>
          </Accordion>
        </div>
        <div class="col-12 md:col-12 mt-2">
          <div class="form-group">
            <label>Ghi chú</label>
            <Textarea :autoResize="true" rows="4" v-model="props.model.note" />
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
@import url(./stylehrm.css);
</style>
<style lang="scss" scoped>
::v-deep(.p-datatable) {
  table {
    border-collapse: collapse;
    min-width: 100%;
    table-layout: fixed;
  }
}
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
::v-deep(.empty-full) {
  .p-datatable-emptymessage td {
    width: 100% !important;
  }
}
</style>
