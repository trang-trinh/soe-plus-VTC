<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { encr, autoFillDate } from "../../../../util/function.js";
import moment from "moment";
const cryoptojs = inject("cryptojs");
const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios");

const basedomainURL = baseURL;
const config = {
  headers: {
    Authorization: `Bearer ${store.getters.token}`,
  },
};
const toast = useToast();
const props = defineProps({
  headerDialog: String,
  displayBasic: Boolean,
  campaign: Object,

  checkadd: Boolean,
  closeDialog: Function,
  view: Boolean,
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

const rules = {
  campaign_name: {
    required,
    $errors: [
      {
        $property: "campaign_name",
        $validator: "required",
        $message: "Tên chiến dịch không được để trống!",
      },
    ],
  },
  num_vacancies: {
    required,
    $errors: [
      {
        $property: "num_vacancies",
        $validator: "required",
        $message: "Số lượng tuyển không được để trống!",
      },
    ],
  },
  campaign_code: {
    required,
    $errors: [
      {
        $property: "campaign_code",
        $validator: "required",
        $message: "Số lượng tuyển không được để trống!",
      },
    ],
  },
};
const listFilesS = ref([]);
const campaign = ref({
  campaign_name: null,
  is_recruitment_proposal: null,
  user_verify: null,
  user_follows: null,
  num_vacancies: null,
  expected_cost: null,
  start_date: null,
  end_date: null,
  rec_vacancies: null,
  rec_position_id: null,
  rec_formality_id: null,
  rec_salary_from: null,
  rec_workplace: null,
  rec_salary_to: null,
  rec_recruitment_deadline: null,
  rec_number_vacancies: null,
  rec_candidate_sheet_id: null,
  can_academic_level_id: null,
  can_specialization_id: null,
  can_experience_id: null,
  can_language_level_id: null,
  can_age_from: null,
  can_age_to: null,
  can_gender: 1,
  can_height_from: null,
  can_height_to: null,
  can_weight_to: null,
  can_weight_from: null,
  job_description: null,
});
const submitted = ref(false);
const list_users_training = ref([]);
const list_schedule = ref([]);
const loadData = () => {
  if (props.checkadd == true) {
    list_users_training.value = [];
    list_schedule.value = [];
    campaign.value = props.campaign;
  } else {
    axios
      .post(
        baseURL + "/api/hrm_ca_SQL/getData",
        {
          str: encr(
            JSON.stringify({
              proc: "hrm_campaign_get",
              par: [
                {
                  par: "campaign_id",
                  va: props.campaign.campaign_id,
                },
              ],
            }),
            SecretKey,
            cryoptojs
          ).toString(),
        },
        config
      )
      .then((response) => {
        let data = JSON.parse(response.data.data)[0];
        let data1 = JSON.parse(response.data.data)[1];
        if (data) {
          campaign.value = data[0];

          if (campaign.value.start_date)
            campaign.value.start_date = new Date(campaign.value.start_date);
          if (campaign.value.end_date)
            campaign.value.end_date = new Date(campaign.value.end_date);
          if (campaign.value.rec_recruitment_deadline)
            campaign.value.rec_recruitment_deadline = new Date(
              campaign.value.rec_recruitment_deadline
            );
          campaign.value.user_verify_fake =
            campaign.value.user_verify.split(",");
          campaign.value.user_follows_fake =
            campaign.value.user_follows.split(",");
        }
     
        if (data1) {
          listFilesS.value = data1;
        }
      })
      .catch((error) => {});
  }
};
const saveData = (isFormValid) => {
  submitted.value = true;
  if (!isFormValid) {
    return;
  }
  if (
    campaign.value.rec_vacancies == null ||
    campaign.value.user_verify_fake == null ||
    campaign.value.rec_recruitment_deadline == null
  ) {
    return;
  }

  if (campaign.value.campaign_name.length > 250) {
    swal.fire({
      title: "Error!",
      text: "Tên chiến dịch không được vượt quá 250 ký tự!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }

  if (campaign.value.user_verify_fake.length > 0)
    campaign.value.user_verify = campaign.value.user_verify_fake.toString();
  if (campaign.value.user_follows_fake.length > 0)
    campaign.value.user_follows = campaign.value.user_follows_fake.toString();

  let formData = new FormData();
  for (var i = 0; i < filesList.value.length; i++) {
    let file = filesList.value[i];
    formData.append("image", file);
  }

  formData.append("hrm_campaign", JSON.stringify(campaign.value));
  formData.append("hrm_files", JSON.stringify(listFilesS.value));
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  if (props.checkadd) {
    axios
      .post(baseURL + "/api/hrm_campaign/add_hrm_campaign", formData, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Thêm thông tin chiến dịch thành công!");

          props.closeDialog();
        } else {
          swal.fire({
            title: "Error!",
            text: response.data.ms,
            icon: "error",
            confirmButtonText: "OK",
          });
        }
      })
      .catch((error) => {
        swal.close();
        swal.fire({
          title: "Error!",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
      });
  } else {
    axios
      .put(baseURL + "/api/hrm_campaign/update_hrm_campaign", formData, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa thông tin chiến dịch thành công!");

          props.closeDialog();
        } else {
          swal.fire({
            title: "Error!",
            text: response.data.ms,
            icon: "error",
            confirmButtonText: "OK",
          });
        }
      })
      .catch((error) => {
        swal.close();
        swal.fire({
          title: "Error!",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
      });
  }
};
 
 
const v$ = useVuelidate(rules, campaign);
const listVacancies = ref([]);
const listPosition = ref([]);
const listFormality = ref([]);

const listAcademic_level = ref([]);
const listSpecialization = ref([]);
const listExperience = ref([]);
const listLanguage_level = ref([]);

const listStatus = ref([
  { name: "Lên kế hoạch", code: 1 },
  { name: "Đang thực hiện", code: 2 },
  { name: "Đã hoàn thành", code: 3 },
  { name: "Tạm dừng", code: 4 },
  { name: "Đã hủy", code: 5 },
]);
const listGender = ref([
  { name: "Nam", code: 1 },
  { name: "Nữ", code: 2 },
  { name: "Khác", code: 3 },
]);

const checkShow = ref(false);
const checkShow2 = ref(false);
const checkShow3 = ref(false);

const showHidePanel = (type) => {
  if (type == 1) {
    if (checkShow.value == true) {
      checkShow.value = false;
    } else {
      checkShow.value = true;
    }
  }
  if (type == 2) {
    if (checkShow2.value == true) {
      checkShow2.value = false;
    } else {
      checkShow2.value = true;
    }
  }
  if (type == 3) {
    if (checkShow3.value == true) {
      checkShow3.value = false;
    } else {
      checkShow3.value = true;
    }
  }
};
 
const deleteFileH = (value) => {
  listFilesS.value = listFilesS.value.filter((x) => x.file_id != value.file_id);
};
const treedonvis = ref();
const listProposal = ref([]);
const initTudien = () => {
  axios
      .post(
        baseURL + "/api/hrm_ca_SQL/getData",
        {
          str: encr(
            JSON.stringify({
              proc: "hrm_rec_proposal_list_all",
              par: [
            
                { par: "status", va:2 },
                { par: "user_id", va: store.getters.user.user_id },
              ],
            }),
            SecretKey,
            cryoptojs
          ).toString(),
        },
        config
      )
      .then((response) => {
        let data = JSON.parse(response.data.data)[0];
        listProposal.value=[];
        data.forEach(element => {
          listProposal.value.push({name:element.recruitment_proposal_name, code:element.recruitment_proposal_id});
        });
 
      })
      .catch((error) => {
   console.log(error);
      });

  
  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_device_department",
            par: [{ par: "user_id", va: store.getters.user.user_id }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data[1].length > 0) {
        let obj = renderTreeDV(
          data[1],
          "organization_id",
          "organization_name",
          "đơn vị"
        );

        treedonvis.value = obj.arrtreeChils;
      }
    })
    .catch((error) => {});

  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_ca_work_position_list",
            par: [
              { par: "pageno", va: 0 },
              { par: "pagesize", va: 100000 },
              { par: "user_id", va: store.getters.user.user_id },
              { par: "status", va: true },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      listVacancies.value = [];
      data.forEach((element, i) => {
        listVacancies.value.push({
          name: element.work_position_name,
          code: element.work_position_id,
        });
      });
    })
    .catch((error) => {
      console.log(error);
    });
  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "ca_positions_list",
            par: [
              { par: "pageno", va: 0 },
              { par: "pagesize", va: 100000 },
              { par: "user_id", va: store.getters.user.user_id },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      listPosition.value = [];
      data.forEach((element, i) => {
        listPosition.value.push({
          name: element.position_name,
          code: element.position_id,
        });
      });
    })
    .catch((error) => {
      console.log(error);
    });

  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_ca_formality_list",
            par: [
              { par: "pageno", va: 0 },
              { par: "pagesize", va: 100000 },
              { par: "user_id", va: store.getters.user.user_id },
              { par: "status", va: true },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      listFormality.value = [];
      data.forEach((element, i) => {
        listFormality.value.push({
          name: element.formality_name,
          code: element.formality_id,
        });
      });
    })
    .catch((error) => {
      console.log(error);
    });
  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_ca_academic_level_list",
            par: [
              { par: "pageno", va: 0 },
              { par: "pagesize", va: 100000 },
              { par: "user_id", va: store.getters.user.user_id },
              { par: "status", va: true },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      listAcademic_level.value = [];
      data.forEach((element, i) => {
        listAcademic_level.value.push({
          name: element.academic_level_name,
          code: element.academic_level_id,
        });
      });
    })
    .catch((error) => {
      console.log(error);
    });
  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_ca_specialization_list",
            par: [
              { par: "pageno", va: 0 },
              { par: "pagesize", va: 100000 },
              { par: "user_id", va: store.getters.user.user_id },
              { par: "status", va: true },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      listSpecialization.value = [];
      data.forEach((element, i) => {
        listSpecialization.value.push({
          name: element.specialization_name,
          code: element.specialization_id,
        });
      });
    })
    .catch((error) => {
      console.log(error);
    });

  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_ca_language_level_list",
            par: [
              { par: "pageno", va: 0 },
              { par: "pagesize", va: 100000 },
              { par: "user_id", va: store.getters.user.user_id },
              { par: "status", va: true },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      listLanguage_level.value = [];
      data.forEach((element, i) => {
        listLanguage_level.value.push({
          name: element.language_level_name,
          code: element.language_level_id,
        });
      });
    })
    .catch((error) => {
      console.log(error);
    });
  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_ca_experience_list",
            par: [
              { par: "pageno", va: 0 },
              { par: "pagesize", va: 100000 },
              { par: "user_id", va: store.getters.user.user_id },
              { par: "status", va: true },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      listExperience.value = [];
      data.forEach((element, i) => {
        listExperience.value.push({
          name: element.experience_name,
          code: element.experience_id,
        });
      });
    })
    .catch((error) => {
      console.log(error);
    });
};
const renderTreeDV = (data, id, name, title) => {
  let arrChils = [];
  let arrtreeChils = [];
  data
    .filter((x) => x.parent_id == null)
    .forEach((m, i) => {
      m.IsOrder = i + 1;
      let om = { key: m[id], data: m };
      const rechildren = (mm, pid) => {
        let dts = data.filter((x) => x.parent_id == pid);
        if (dts.length > 0) {
          if (!mm.children) mm.children = [];
          dts.forEach((em) => {
            let om1 = { key: em[id], data: em };
            rechildren(om1, em[id]);
            mm.children.push(om1);
          });
        }
      };
      rechildren(om, m[id]);
      arrChils.push(om);
      //
      om = { key: m[id], data: m[id], label: m[name] };
      const retreechildren = (mm, pid) => {
        let dts = data.filter((x) => x.parent_id == pid);
        if (dts.length > 0) {
          if (!mm.children) mm.children = [];
          dts.forEach((em) => {
            let om1 = { key: em[id], data: em[id], label: em[name] };
            retreechildren(om1, em[id]);
            mm.children.push(om1);
          });
        }
      };
      retreechildren(om, m[id]);
      arrtreeChils.push(om);
    });
  return { arrChils: arrChils, arrtreeChils: arrtreeChils };
};
const filesList = ref([]);
let fileSize = [];
const onUploadFile = (event) => {
  fileSize = [];
  filesList.value = [];

  var ms = false;

  event.files.forEach((fi) => {
    let formData = new FormData();
    formData.append("fileupload", fi);
    axios({
      method: "post",
      url: baseURL + `/api/chat/ScanFileUpload`,
      data: formData,
      headers: {
        Authorization: `Bearer ${store.getters.token}`,
      },
    })
      .then((response) => {
        if (response.data.err != "1") {
          if (fi.size > 100 * 1024 * 1024) {
            ms = true;
          } else {
            filesList.value.push(fi);
            fileSize.push(fi.size);
          }
        } else {
          filesList.value = filesList.value.filter((x) => x.name != fi.name);
          swal.fire({
            title: "Cảnh báo",
            text: "File bị xóa do tồn tại mối đe dọa với hệ thống!",
            icon: "warning",
            confirmButtonText: "OK",
          });
        }
        if (ms) {
          swal.fire({
            icon: "warning",
            type: "warning",
            title: "Thông báo",
            text: "Bạn chỉ được upload file có dung lượng tối đa 100MB!",
          });
        }
      })
      .catch(() => {
        swal.fire({
          title: "Thông báo",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
      });
  });
};
const removeFile = (event) => {
  filesList.value = filesList.value.filter((a) => a != event.file);
};

const listLimit = ref([
  {
    name: "Nội bộ",
    code: 1,
  },
  {
    name: "Bên ngoài",
    code: 2,
  },
]);

const listDataUsers = ref([]);
const listDataUsersSave = ref([]);
const loadUserProfiles = () => {
  listDataUsers.value = [];

  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_profile_list_all",
            par: [
              
              { par: "user_id", va: store.getters.user.user_id },
            
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];

      data.forEach((element, i) => {
        listDataUsers.value.push({
          profile_user_name: element.profile_user_name,
          code:element.profile_id,
          avatar: element.avatar,
          department_name: element.department_name,
          department_id: element.department_id,
          work_position_name: element.work_position_name,
          position_name: element.position_name,

          organization_id: element.organization_id,
        });
      });
      listDataUsersSave.value = [...listDataUsers.value];
    })
    .catch((error) => {
      console.log(error);

      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
 
const   displayBasic=ref(false);
//Thêm bản ghi
 
onMounted(() => {
  loadData();
  initTudien();
 
  loadUserProfiles();
  displayBasic.value=props.displayBasic;
  return {};
});
</script>
<template>
  <Dialog
    :header="props.headerDialog"
    v-model:visible=" displayBasic"
    :style="{ width: '65vw' }"
    :maximizable="true"
    :modal="true"
    :closable="true"
    @hide="props.closeDialog"
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="col-12 field p-0 text-lg font-bold">
          Thông tin chiến dịch
        </div>
        <div class="col-12 field flex p-0 align-items-center">
        <div class="col-6   flex p-0 align-items-center">
          <div class="w-10rem">
            Mã chiến dịch<span class="redsao pl-1"> (*)</span>
          </div>
          <div style="width: calc(100% - 10rem)">
            <div class="col-12 p-0">
              <div class="p-inputgroup">
                <InputText
                 
           
                  v-model="campaign.campaign_code"
                  class="w-full"
                  :style="{ backgroundColor: '#FEF9E7', fontWeight: 'bold' }"
                  :class="{
                    'p-invalid': v$.campaign_code.$invalid && submitted,
                  }"
                />
              </div>
            </div>
          </div>
        </div>
        <div class="col-6    flex p-0 align-items-center">
         
            <div class="w-10rem pl-3">Đề xuất</div>
            <div style="width: calc(100% - 10rem)">
              <Dropdown
                  v-model="campaign.recruitment_proposal_id"
                  :options="listProposal"
                  optionLabel="name"
                  optionValue="code"
                  class="w-full"
                  panelClass="d-design-dropdown"
                  placeholder="Đề xuất tuyển dụng"
                />
              </div>
          </div>
        </div>
        <div
          class="col-12 p-0 field flex"
          v-if="
            (v$.campaign_code.$invalid && submitted) ||
            v$.campaign_code.$pending.$response
          "
        >
          <div class="p-0 col-12">
            <div class="col-12 p-0 flex">
              <div class="w-10rem"></div>
              <small style="width: calc(100% - 10rem)">
                <span style="color: red" class="w-full">{{
                  v$.campaign_code.required.$message
                    .replace("Value", "Mã chiến dịch")
                    .replace("is required", "không được để trống!")
                }}</span>
              </small>
            </div>
          </div>
        </div>
        <div class="col-12 field flex p-0 align-items-center">
          <div class="w-10rem">
            Tên chiến dịch<span class="redsao pl-1"> (*)</span>
          </div>
          <div style="width: calc(100% - 10rem)">
            <div class="col-12 p-0">
              <div class="p-inputgroup">
                <Textarea
                  :autoResize="true"
                  rows="1" placeholder="Nhập tên chiến dịch"
                  cols="30"
                  v-model="campaign.campaign_name"
                  class="w-full"
                  :style="
                    campaign.campaign_name
                      ? 'background-color:white !important'
                      : ''
                  "
                  :class="{
                    'p-invalid': v$.campaign_name.$invalid && submitted,
                  }"
                />
              </div>
            </div>
          </div>
        </div>
        <div
          class="col-12 p-0 field flex"
          v-if="
            (v$.campaign_name.$invalid && submitted) ||
            v$.campaign_name.$pending.$response
          "
        >
          <div class="p-0 col-12">
            <div class="col-12 p-0 flex">
              <div class="w-10rem"></div>
              <small style="width: calc(100% - 10rem)">
                <span style="color: red" class="w-full">{{
                  v$.campaign_name.required.$message
                    .replace("Value", "Tên chiến dịch")
                    .replace("is required", "không được để trống!")
                }}</span>
              </small>
            </div>
          </div>
        </div>
        <div class="col-12 field p-0 flex text-left align-items-center">
          <div class="col-6 p-0 flex text-left align-items-center">
            <div class="w-10rem">
              Người phụ trách <span class="redsao pl-1"> (*)</span>
            </div>
            <div style="width: calc(100% - 10rem)">
              <MultiSelect :filter="true"
                v-model="campaign.user_verify_fake"
                :options="listDataUsers"
                optionLabel="profile_user_name"
                optionValue="code"
                placeholder="-------- Chọn người phụ trách --------"
                panelClass="d-design-dropdown"
                class="w-full p-0 d-tree-input"
                :class="{
                  'p-invalid': campaign.user_verify_fake == null && submitted,
                }"
                display="chip"
              >
               <template #option="slotProps">
                  <div v-if="slotProps.option" class="flex">
                    <div class="format-center">
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
                          color: #ffffff;
                          width: 3rem;
                          height: 3rem;
                          font-size: 1.4rem !important;
                        "
                        :style="{
                          background:
                            bgColor[
                              slotProps.option.profile_user_name.length % 7
                            ],
                        }"
                        size="xlarge"
                        shape="circle"
                      />
                    </div>
                    <div class="format-center text-left ml-3">
                      <div>
                        <div class="mb-1 font-bold">
                          {{ slotProps.option.profile_user_name }}
                        </div>
                        <div class="description">
                          <div>
                            <span v-if="slotProps.option.position_name">{{
                              slotProps.option.position_name
                            }}</span>
                            <span v-else>{{
                              slotProps.option.profile_code
                            }}</span>

                            <span v-if="slotProps.option.department_name">
                              | {{ slotProps.option.department_name }}</span
                            >
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>
                  <span v-else> Chưa có dữ liệu </span>
                </template>
              </MultiSelect>
            </div>
          </div>
          <div class="col-6 p-0 flex text-left align-items-center">
            <div class="w-10rem pl-3">Người theo dõi</div>
            <div style="width: calc(100% - 10rem)">
              <MultiSelect   :filter="true"
                v-model="campaign.user_follows_fake"
                :options="listDataUsers"
                optionLabel="profile_user_name"
                optionValue="code"
                placeholder="-------- Chọn người theo dõi --------"
                panelClass="d-design-dropdown"
                class="w-full p-0 d-tree-input"
                display="chip"
              >
              <template #option="slotProps">
                  <div v-if="slotProps.option" class="flex">
                    <div class="format-center">
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
                          color: #ffffff;
                          width: 3rem;
                          height: 3rem;
                          font-size: 1.4rem !important;
                        "
                        :style="{
                          background:
                            bgColor[
                              slotProps.option.profile_user_name.length % 7
                            ],
                        }"
                        size="xlarge"
                        shape="circle"
                      />
                    </div>
                    <div class="format-center text-left ml-3">
                      <div>
                        <div class="mb-1 font-bold">
                          {{ slotProps.option.profile_user_name }}
                        </div>
                        <div class="description">
                          <div>
                            <span v-if="slotProps.option.position_name">{{
                              slotProps.option.position_name
                            }}</span>
                            <span v-else>{{
                              slotProps.option.profile_code
                            }}</span>

                            <span v-if="slotProps.option.department_name">
                              | {{ slotProps.option.department_name }}</span
                            >
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>
                  <span v-else> Chưa có dữ liệu </span>
                </template>
              </MultiSelect>
            </div>
          </div>
        </div>
        <div
          class="col-12 p-0 field flex"
          v-if="campaign.user_verify_fake == null && submitted"
        >
          <div class="p-0 col-6">
            <div class="col-12 p-0 flex">
              <div class="w-10rem"></div>
              <small style="width: calc(100% - 10rem)">
                <span style="color: red" class="w-full"
                  >Người phụ trách không được để trống!</span
                >
              </small>
            </div>
          </div>
        </div>
        <div class="col-12 field p-0 flex text-left align-items-center">
          <div class="col-6 p-0 flex text-left align-items-center">
            <div class="w-10rem">
              Số lượng tuyển<span class="redsao pl-1"> (*)</span>
            </div>
            <div style="width: calc(100% - 10rem)">
              <InputNumber
                class="w-full"
                suffix=" Người" placeholder="Nhập số lượng tuyển"
                v-model="campaign.num_vacancies"
                :class="{
                  'p-invalid': campaign.num_vacancies == null && submitted,
                }"
              />
            </div>
          </div>
          <div class="col-6 p-0 flex text-left align-items-center">
            <div class="w-10rem pl-3">Chi phí dự kiến</div>
            <div style="width: calc(100% - 10rem)">
              <InputNumber
                v-model="campaign.expected_cost"
                class="w-full"
                inputId="locale-german" locale="de-DE"  
                placeholder="Nhập chi phí dự kiến"
              />
            </div>
          </div>
        </div>
        <div
          class="col-12 p-0 field flex"
          v-if="campaign.num_vacancies == null && submitted"
        >
          <div class="p-0 col-6">
            <div class="col-12 p-0 flex">
              <div class="w-10rem"></div>
              <small style="width: calc(100% - 10rem)">
                <span style="color: red" class="w-full"
                  >Số lượng tuyển không được để trống!
                </span>
              </small>
            </div>
          </div>
        </div>

        <div class="col-12 field p-0 flex text-left align-items-center">
          <div class="col-6 p-0 flex text-left align-items-center">
            <div class="w-10rem">Ngày bắt đầu</div>
            <div style="width: calc(100% - 10rem)">
              <Calendar
              @blur="autoFillDate(campaign,'start_date')"
              id="start_date"
                class="w-full"
        
                v-model="campaign.start_date" :showOnFocus="false"
                autocomplete="on"
                :showIcon="true"
                placeholder="dd/mm/yyyy"
              />
            </div>
          </div>
          <div class="col-6 p-0 flex text-left align-items-center">
            <div class="w-10rem pl-3">Ngày kết thúc</div>
            <div style="width: calc(100% - 10rem)">
              <Calendar
                class="w-full"
                placeholder="dd/mm/yyyy"
                @blur="autoFillDate(campaign,'end_date')"
              id="end_date"
                v-model="campaign.end_date" :showOnFocus="false"
                autocomplete="on"
                :minDate="
                  campaign.start_date ? new Date(campaign.start_date) : null
                "
                :showIcon="true"
              />
            </div>
          </div>
        </div>

        <div class="col-12 field p-0 text-lg font-bold">
          Thông tin vị trí tuyển
        </div>

        <div class="col-12 field p-0 flex text-left align-items-center">
          <div class="col-6 p-0 flex text-left align-items-center">
            <div class="w-10rem">
              Vị trí<span class="redsao pl-1"> (*)</span>
            </div>
            <div style="width: calc(100% - 10rem)">
              <Dropdown
                :filter="true"
                v-model="campaign.rec_vacancies"
                :options="listVacancies"
                optionLabel="name"
                optionValue="code"
                class="w-full"
                panelClass="d-design-dropdown"
                placeholder="Chọn vị trí"
                :class="{
                  'p-invalid': campaign.rec_vacancies == null && submitted,
                }"
              />
            </div>
          </div>
          <div class="col-6 p-0 flex text-left align-items-center">
            <div class="w-10rem pl-3">Chức vụ</div>
            <div style="width: calc(100% - 10rem)">
              <Dropdown
                :filter="true"
                v-model="campaign.rec_position_id"
                :options="listPosition"
                optionLabel="name"
                optionValue="code"
                class="w-full"
                placeholder="Chọn chức vụ"
                panelClass="d-design-dropdown"
              />
            </div>
          </div>
        </div>
        <div
          class="col-12 p-0 field flex"
          v-if="campaign.rec_vacancies == null && submitted"
        >
          <div class="p-0 col-6">
            <div class="col-12 p-0 flex">
              <div class="w-10rem"></div>
              <small style="width: calc(100% - 10rem)">
                <span style="color: red" class="w-full"
                  >Vị trí không được để trống!
                </span>
              </small>
            </div>
          </div>
        </div>

        <div class="col-12 flex p-0">
          <div class="col-6 p-0">
            <div class="col-12 field flex p-0 text-left align-items-center">
              <div class="w-10rem">Hình thức làm việc</div>
              <div style="width: calc(100% - 10rem)">
                <Dropdown   :filter="true"
                  v-model="campaign.rec_formality_id"
                  :options="listFormality"
                  optionLabel="name"
                  optionValue="code"
                  class="w-full"
                  panelClass="d-design-dropdown"
                  placeholder="Chọn hình thức làm việc"
                />
              </div>
            </div>
          </div>
          <div class="col-6 flex p-0 text-center align-items-center">
            <div class="col-12 field flex p-0 text-left align-items-center">
              <div class="w-10rem pl-3">Nơi làm việc</div>
              <div style="width: calc(100% - 10rem)">
                <InputText v-model="campaign.rec_workplace" class="w-full" 
                
                placeholder="Nhập nơi làm việc"
                />
              </div>
            </div>
          </div>
        </div>

        <div class="col-12 flex p-0">
          <div class="col-6 p-0">
            <div class="col-12 field flex p-0 text-left align-items-center">
              <div class="w-10rem">Mức lương (từ)</div>
              <div style="width: calc(100% - 10rem)">
                <InputNumber
                  v-model="campaign.rec_salary_from"
                  :min="0"
                  class="w-full d-input-design-number"
                  inputId="locale-german" locale="de-DE"  
                  placeholder="Từ"
                />
              </div>
            </div>
          </div>
          <div class="col-6 flex p-0 text-center align-items-center">
            <div class="col-12 field flex p-0 text-left align-items-center">
              <div class="w-10rem pl-3">Mức lương (đến)</div>
              <div style="width: calc(100% - 10rem)">
                <InputNumber
                  v-model="campaign.rec_salary_to"
                  :min="
                    campaign.rec_salary_from
                      ? campaign.rec_salary_from + 1
                      : null
                  "
                  class="w-full d-input-design-number"
                  inputId="locale-german" locale="de-DE"  
                  placeholder="Đến"
                />
              </div>
            </div>
          </div>
        </div>
        <div class="col-12 flex p-0">
          <div class="col-6 p-0">
            <div class="col-12 field flex p-0 text-left align-items-center">
              <div class="w-10rem">
                Hạn tuyển<span class="redsao pl-1"> (*)</span>
              </div>
              <div style="width: calc(100% - 10rem)">
                <Calendar
                  class="w-full"
                  @blur="autoFillDate(campaign,'rec_recruitment_deadline')"
              id="rec_recruitment_deadline"
                  placeholder="dd/mm/yyyy"
                  v-model="campaign.rec_recruitment_deadline"
                  autocomplete="on" :showOnFocus="false"
                  :showIcon="true"
                  :class="{
                    'p-invalid':
                      campaign.rec_recruitment_deadline == null && submitted,
                  }"
                  :minDate="
                    campaign.start_date ? new Date(campaign.start_date) : null
                  "
                />
              </div>
            </div>
          </div>
          <div class="col-6 flex p-0 text-center align-items-center">
            <div class="col-12 field flex p-0 text-left align-items-center">
              <div class="w-10rem pl-3">Số lượng</div>
              <div style="width: calc(100% - 10rem)">
                <InputNumber
                  v-model="campaign.rec_number_vacancies"
                  :min="0"
                  class="w-full d-input-design-number"
                  suffix=" Người"
                  placeholder="Số lượng"
                />
              </div>
            </div>
          </div>
        </div>
        <div
          class="col-12 p-0 field flex"
          v-if="campaign.rec_recruitment_deadline == null && submitted"
        >
          <div class="p-0 col-6">
            <div class="col-12 p-0 flex">
              <div class="w-10rem"></div>
              <small style="width: calc(100% - 10rem)">
                <span style="color: red" class="w-full"
                  >Ngày bắt đầu không được để trống!
                </span>
              </small>
            </div>
          </div>
        </div>
        <!-- <div class="col-12 flex p-0">
            
              <div class="col-12 field flex p-0 text-left align-items-center">
                <div class="w-10rem">
             Mẫu đánh giá
                </div>
                <div style="width: calc(100% - 10rem)">
                         <Dropdown
                  v-model="campaign.rec_candidate_sheet_id"
                  :options="listStatus"
                  optionLabel="name"
                  optionValue="code"
                  class="w-full"
                  panelClass="d-design-dropdown"
                    placeholder="Chọn mẫu đánh giá ứng viên"
                />
                </div>
           
            </div>
         
          </div> -->
        <div class="col-12 field p-0 text-lg font-bold">Yêu cầu ứng viên</div>

        <div class="col-12 field p-0 flex text-left align-items-center">
          <div class="col-6 p-0 flex text-left align-items-center">
            <div class="w-10rem">Trình độ</div>
            <div style="width: calc(100% - 10rem)">
              <Dropdown   :filter="true"
                v-model="campaign.can_academic_level_id"
                :options="listAcademic_level"
                optionLabel="name"
                optionValue="code"
                class="w-full"
                panelClass="d-design-dropdown"
                placeholder="Chọn trình độ"
              />
            </div>
          </div>
          <div class="col-6 p-0 flex text-left align-items-center">
            <div class="w-10rem pl-3">Chuyên ngành</div>
            <div style="width: calc(100% - 10rem)">
              <Dropdown   :filter="true"
                v-model="campaign.can_specialization_id"
                :options="listSpecialization"
                optionLabel="name"
                optionValue="code"
                class="w-full"
                panelClass="d-design-dropdown"
                placeholder="Chọn chuyên ngành"
              />
            </div>
          </div>
        </div>

        <div class="col-12 flex p-0">
          <div class="col-6 p-0">
            <div class="col-12 field flex p-0 text-left align-items-center">
              <div class="w-10rem">Kinh nghiệm</div>
              <div style="width: calc(100% - 10rem)">
                <Dropdown   :filter="true"
                  v-model="campaign.can_experience_id"
                  :options="listExperience"
                  optionLabel="name"
                  optionValue="code"
                  class="w-full"
                  panelClass="d-design-dropdown"
                  placeholder="Chọn kinh nghiệm"
                />
              </div>
            </div>
          </div>
          <div class="col-6 flex p-0 text-center align-items-center">
            <div class="col-12 field flex p-0 text-left align-items-center">
              <div class="w-10rem pl-3">Ngoại ngữ</div>
              <div style="width: calc(100% - 10rem)">
                <Dropdown   :filter="true"
                  v-model="campaign.can_language_level_id"
                  :options="listLanguage_level"
                  optionLabel="name"
                  optionValue="code"
                  class="w-full"
                  panelClass="d-design-dropdown"
                  placeholder="Chọn trình độ ngoại ngữ"
                />
              </div>
            </div>
          </div>
        </div>

        <div class="col-12 flex p-0">
          <div class="col-6 p-0">
            <div class="col-12 field flex p-0 text-left align-items-center">
              <div class="w-10rem">Tuổi</div>
              <div style="width: calc(100% - 10rem)" class="flex">
                <div class="w-full mr-2">
                  <InputNumber
                    v-model="campaign.can_age_from"
                    class="w-full"
                    :min="0"
                    suffix=" Tuổi"
                    placeholder="Từ"
                  />
                </div>
                <div class="w-full">
                  <InputNumber
                    v-model="campaign.can_age_to"
                    class="w-full"
                    :min="
                      campaign.can_age_from ? campaign.can_age_from + 1 : null
                    "
                    suffix=" Tuổi"
                    placeholder="Đến"
                  />
                </div>
              </div>
            </div>
          </div>
          <div class="col-6 flex p-0 text-center align-items-center">
            <div class="col-12 field flex p-0 text-left align-items-center">
              <div class="w-10rem pl-3">Giới tính</div>
              <div style="width: calc(100% - 10rem)">
                <Dropdown 
                  v-model="campaign.can_gender"
                  :options="listGender"
                  optionLabel="name"
                  optionValue="code"
                  class="w-full"
                  panelClass="d-design-dropdown"
                  placeholder="Chọn giới tính"
                />
              </div>
            </div>
          </div>
        </div>
        <div class="col-12 flex p-0">
          <div class="col-6 p-0">
            <div class="col-12 field flex p-0 text-left align-items-center">
              <div class="w-10rem">Chiều cao</div>
              <div style="width: calc(100% - 10rem)" class="flex">
                <div class="w-full mr-2">
                  <InputNumber
                    v-model="campaign.can_height_from"
                    :min="0"
                    class="w-full"
                    suffix=" Cm"
                    placeholder="Từ"
                  />
                </div>
                <div class="w-full">
                  <InputNumber
                    v-model="campaign.can_height_to"
                    :min="
                      campaign.can_height_from
                        ? campaign.can_height_from + 1
                        : null
                    "
                    class="w-full"
                    suffix=" Cm"
                    placeholder="Đến"
                  />
                </div>
              </div>
            </div>
          </div>
          <div class="col-6 p-0">
            <div class="col-12 field flex p-0 text-left align-items-center">
              <div class="w-10rem pl-3">Cân nặng</div>
              <div style="width: calc(100% - 10rem)" class="flex">
                <div class="w-full mr-2">
                  <InputNumber
                    v-model="campaign.can_weight_from"
                    :min="0"
                    class="w-full"
                    suffix=" Kg"
                    placeholder="Từ"
                  />
                </div>
                <div class="w-full">
                  <InputNumber
                    v-model="campaign.can_weight_to"
                    :min="
                      campaign.can_weight_from
                        ? campaign.can_weight_from + 1
                        : null
                    "
                    class="w-full"
                    suffix=" Kg"
                    placeholder="Đến"
                  />
                </div>
              </div>
            </div>
          </div>
        </div>
        <div class="col-12 field flex p-0 text-left align-items-center">
          <div class="w-10rem">Mô tả công việc</div>
          <div style="width: calc(100% - 10rem)" class="flex">
            <Textarea
              :autoResize="true"
              rows="1"
              cols="40"
              v-model="campaign.job_description"
              class="w-full"
            />
          </div>
        </div>

        <div class="col-12 field p-0 text-lg font-bold">File đính kèm</div>
        <div class="w-full col-12 field p-0">
          <FileUpload
            chooseLabel="Chọn File"
            :showUploadButton="false"
            :showCancelButton="false"
            :multiple="false"
            :maxFileSize="524288000"
            @select="onUploadFile"
            @remove="removeFile"
            :invalidFileSizeMessage="'{0}: Dung lượng File không được lớn hơn {1}'"
          >
            <template #empty>
              <p class="p-0 m-0 text-500">Kéo thả hoặc chọn File.</p>
            </template>
          </FileUpload>

          <div class="col-12 p-0" v-if="listFilesS.length > 0">
              <DataTable
                :value="listFilesS"
                filterDisplay="menu"
                filterMode="lenient"
                scrollHeight="flex"
                :showGridlines="true"
                :paginator="false"
                :row-hover="true"
                columnResizeMode="fit"
              >
                <Column field="code" header="  File đính kèm">
                  <template #body="item">
                    <div
                      class="p-0 d-style-hover"
                      style="width: 100%; border-radius: 10px"
                    >
                      <div class="w-full flex align-items-center">
                        <div class="flex w-full text-900">
                          <div
                            v-if="item.data.is_image"
                            class="align-items-center flex"
                          >
                            <Image
                              :src="basedomainURL + item.data.file_path"
                              alt=""
                              width="70"
                              height="50"
                              style="
                                object-fit: contain;
                                border: 1px solid #ccc;
                                width: 70px;
                                height: 50px;
                              "
                              preview
                              class="pr-2"
                            />
                            <div class="ml-2" style="word-break: break-all">
                              {{ item.data.file_name }}
                            </div>
                          </div>
                          <div v-else>
                            <a
                              :href="basedomainURL + item.data.file_path"
                              download
                              class="w-full no-underline cursor-pointer text-900"
                            >
                              <div class="align-items-center flex">
                                <div>
                                  <img
                                    :src="
                                      basedomainURL +
                                      '/Portals/Image/file/' +
                                      item.data.file_path.substring(
                                        item.data.file_path.lastIndexOf('.') + 1
                                      ) +
                                      '.png'
                                    "
                                    style="
                                      width: 70px;
                                      height: 50px;
                                      object-fit: contain;
                                    "
                                    alt=""
                                  />
                                </div>
                                <div class="ml-2" style="word-break: break-all">
                                  <div
                                    class="ml-2"
                                    style="word-break: break-all"
                                  >
                                    <div style="word-break: break-all">
                                      {{ item.data.file_name }}
                                    </div>
                                    <div
                                      v-if="store.getters.user.is_super"
                                      style="
                                        word-break: break-all;
                                        font-size: 11px;
                                        font-style: italic;
                                      "
                                    >
                                      {{ item.data.organization_name }}
                                    </div>
                                  </div>
                                </div>
                              </div>
                            </a>
                          </div>
                        </div>
                        <div
                          class="w-3rem align-items-center d-style-hover-1"
                          v-if="
                            store.getters.user.organization_id ==
                            item.data.organization_id
                          "
                        >
                          <Button
                            icon="pi pi-times"
                            class="p-button-rounded bg-red-300 border-none"
                            @click="deleteFileH(item.data)"
                          />
                        </div>
                      </div>
                    </div>
                  </template>
                </Column>
              </DataTable>
            </div>
        </div>
      </div>
    </form>
    <template #footer>
      <div class="pt-3" v-if="!view">
        <Button
          label="Hủy"
          icon="pi pi-times"
          @click="props.closeDialog"
          class="p-button-outlined"
        />

        <Button
          label="Lưu"
          icon="pi pi-check"
          @click="saveData(!v$.$invalid)"
          autofocus
        />
      </div>
    </template>
  </Dialog>
</template>
<style scoped>
#formprint {
  background: #fff !important;
}

#formprint * {
  font-family: "Times New Roman", Times, serif !important;
  font-size: 13pt;
}

.hover:hover {
  cursor: pointer;
  color: #2196f3 !important;
}

.title1,
.title1 * {
  font-size: 17pt !important;
}

.title2,
.title2 * {
  font-size: 16pt !important;
}

.title3,
.title3 * {
  font-size: 15pt !important;
}

.boder tr th,
.boder tr td {
  border: 1px solid #999999 !important;
  padding: 0.5rem;
}

table {
  min-width: 100% !important;
  page-break-inside: auto !important;
  border-collapse: collapse !important;
  table-layout: fixed !important;
}

thead {
  display: table-header-group !important;
}

tbody {
  display: table-header-group !important;
}

tr {
  -webkit-column-break-inside: avoid !important;
  page-break-inside: avoid !important;
}

tfoot {
  display: table-footer-group !important;
}

.uppercase,
.uppercase * {
  text-transform: uppercase !important;
}

.text-center {
  text-align: center !important;
}

.text-left {
  text-align: left !important;
}

.text-right {
  text-align: right !important;
}

.html p {
  margin: 0 !important;
  padding: 0 !important;
}
</style>
<style lang="scss" scoped>
::v-deep(.html) {
  p {
    margin: 0 !important;
    padding: 0 !important;
  }
}
</style>
