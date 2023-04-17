<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { encr, checkURL } from "../../../../util/function.js";
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
  recruitment_proposal: Object,

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
  recruitment_proposal_name: {
    required,
    $errors: [
      {
        $property: "recruitment_proposal_name",
        $validator: "required",
        $message: "Tên đề xuất không được để trống!",
      },
    ],
  },
  recruitment_proposal_reason: {
    required,
    $errors: [
      {
        $property: "recruits_num",
        $validator: "required",
        $message: "Lý do tuyển không được để trống!",
      },
    ],
  },
  recruits_num: {
    required,
    $errors: [
      {
        $property: "recruits_num",
        $validator: "required",
        $message: "Số lượng tuyển không được để trống!",
      },
    ],
  },
};
const listFilesS = ref([]);
const recruitment_proposal = ref({
  recruitment_proposal_name: null,
  is_recruitment_proposal: null,
  user_verify: null,
  user_follows: null,
  recruits_num: null,
  expected_cost: null,
  start_date: null,
  end_date: null,
  work_position_id: null,
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
    recruitment_proposal.value = props.recruitment_proposal;
  } else {
    axios
      .post(
        baseURL + "/api/hrm_ca_SQL/getData",
        {
          str: encr(
            JSON.stringify({
              proc: "hrm_recruitment_proposal_get",
              par: [
                {
                  par: "recruitment_proposal_id",
                  va: props.recruitment_proposal.recruitment_proposal_id,
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
          recruitment_proposal.value = data[0];

          if (recruitment_proposal.value.start_date)
            recruitment_proposal.value.start_date = new Date(
              recruitment_proposal.value.start_date
            );
          if (recruitment_proposal.value.end_date)
            recruitment_proposal.value.end_date = new Date(
              recruitment_proposal.value.end_date
            );
          if (recruitment_proposal.value.department_id)
            recruitment_proposal.value.department_id_fake =  {};
            recruitment_proposal.value.department_id_fake[recruitment_proposal.value.department_id]=true;
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
    recruitment_proposal.value.work_position_id == null ||
    recruitment_proposal.value.end_date == null ||
    recruitment_proposal.value.recruits_num == null
  ) {
    return;
  }

  if (recruitment_proposal.value.recruitment_proposal_name.length > 500) {
    swal.fire({
      title: "Error!",
      text: "Tên đề xuất không được vượt quá 500 ký tự!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  if(recruitment_proposal.value.department_id_fake)
  Object.keys(recruitment_proposal.value.department_id_fake).forEach((key) => {
    recruitment_proposal.value.department_id = Number(key);
    return;
  });
   
  let formData = new FormData();
  for (var i = 0; i < filesList.value.length; i++) {
    let file = filesList.value[i];
    formData.append("image", file);
  }

  formData.append(
    "hrm_recruitment_proposal",
    JSON.stringify(recruitment_proposal.value)
  );
  formData.append("hrm_files", JSON.stringify(listFilesS.value));
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  if (props.checkadd) {
    axios
      .post(
        baseURL + "/api/hrm_recruitment_proposal/add_hrm_recruitment_proposal",
        formData,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Thêm thông tin đề xuất thành công!");

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
      .put(
        baseURL +
          "/api/hrm_recruitment_proposal/update_hrm_recruitment_proposal",
        formData,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa thông tin đề xuất thành công!");

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
const listDropdownUserGive = ref();
const listDropdownUserCheck = ref();
const listDropdownUser = ref();
const listUsers = ref([]);
const loadUser = () => {
  listUsers.value = [];
  listDropdownUser.value = [];
  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_users_list_dd",
            par: [
              { par: "search", va: null },
              { par: "user_id", va: store.getters.user.user_id },
              { par: "role_id", va: null },
              {
                par: "organization_id",
                va: store.getters.user.organization_id,
              },
              { par: "department_id", va: null },
              { par: "position_id", va: null },
              { par: "pageno", va: 1 },
              { par: "pagesize", va: 10000 },
              { par: "isadmin", va: null },
              { par: "status", va: null },
              { par: "start_date", va: null },
              { par: "end_date", va: null },
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
        listDropdownUser.value.push({
          name: element.full_name,
          code: element.user_id,

          avatar: element.avatar,
          department_name: element.department_name,
          department_id: element.department_id,
          role_name: element.role_name,
          position_name: element.position_name,
          phone_number: element.phone,
          organization_id: element.organization_id,
        });
        listUsers.value.push({ data: element, active: false });
      });
      listUsers.value = data;
      listDropdownUserGive.value = listDropdownUser.value;
      listDropdownUserCheck.value = listDropdownUser.value.filter(
        (x) => x.code != store.getters.user.user_id
      );
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
const v$ = useVuelidate(rules, recruitment_proposal);
const listVacancies = ref([]);
const listPosition = ref([]);
const listFormality = ref([]);

const listAcademic_level = ref([]);
const listSpecialization = ref([]);
const listExperience = ref([]);
const listLanguage_level = ref([]);

 
const listGender = ref([
  { name: "Nam", code: 1 },
  { name: "Nữ", code: 2 },
  { name: "Khác", code: 3 },
]);

 
 
const deleteFileH = (value) => {
  listFilesS.value = listFilesS.value.filter((x) => x.file_id != value.file_id);
};
const treedonvis = ref();
const listDepartmentTree = ref();

const renderTreeDV1 = (data, id, name, title, org_id) => {
  let arrtreeChils = [];
  if (org_id == "" || org_id == null) {
    data.forEach((m, i) => {
      let om = { key: m[id], data: m[id], label: m[name] };

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
  } else {
    let rew = Number(org_id);
    data
      .filter((x) => x.parent_id == rew)
      .forEach((m, i) => {
        let om = { key: m[id], data: m[id], label: m[name] };

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
  }
  return { arrtreeChils: arrtreeChils };
};
const initTudien = () => {
  listDepartmentTree.value = [];
  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_organization_list_d",
            par: [{ par: "user_id", va: store.getters.user.user_id }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        let obj = renderTreeDV1(
          data,
          "organization_id",
          "organization_name",
          "đơn vị",
          store.getters.user.organization_id
        );
        listDepartmentTree.value = obj.arrtreeChils;
      } else listDepartmentTree.value = [];
    })
    .catch((error) => {
      console.log(error
      );

      
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
            proc: "hrm_positions_list",
            par: [
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
            proc: "hrm_profile_list_filter",
            par: [
              { par: "search", va: null },
              { par: "user_id", va: store.getters.user.user_id },
              { par: "work_position_id", va: null },
              { par: "position_id", va: null },
              { par: "department_id", va: null },
              { par: "status", va: 1 },
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
          code: {
            profile_id: element.profile_id,
            avatar: element.avatar,
            profile_user_name: element.profile_user_name,
            department_name: element.department_name,
            department_id: element.department_id,
            work_position_name: element.work_position_name,
            position_name: element.position_name,
            position_id: element.position_id,
            work_position_id: element.work_position_id,
          },
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

const displayBasic = ref(false);
//Thêm bản ghi

onMounted(() => {
  loadData();
  initTudien();
  loadUser();
  loadUserProfiles();
  displayBasic.value = props.displayBasic;
  return {};
});
</script>
<template>
  <Dialog
    :header="props.headerDialog"
    v-model:visible="displayBasic"
    :style="{ width: '55vw' }"
    :maximizable="true"
    :modal="true"
    :closable="true"
    @hide="props.closeDialog"
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="col-12 field p-0 text-lg font-bold">Thông tin chung</div>

        <div class="col-12 field flex p-0 align-items-center">
          <div class="w-10rem">
            Tên đề xuất<span class="redsao pl-1"> (*)</span>
          </div>
          <div style="width: calc(100% - 10rem)">
            <div class="col-12 p-0">
              <div class="p-inputgroup">
                <Textarea
                  :autoResize="true"
                  rows="1"
                  placeholder="Nhập tên đề xuất"
                  cols="30"
                  v-model="recruitment_proposal.recruitment_proposal_name"
                  class="w-full"
                  :style="
                    recruitment_proposal.recruitment_proposal_name
                      ? 'background-color:white !important'
                      : ''
                  "
                  :class="{
                    'p-invalid':
                      v$.recruitment_proposal_name.$invalid && submitted,
                  }"
                />
              </div>
            </div>
          </div>
        </div>
        <div
          class="col-12 p-0 field flex"
          v-if="
            (v$.recruitment_proposal_name.$invalid && submitted) ||
            v$.recruitment_proposal_name.$pending.$response
          "
        >
          <div class="p-0 col-12">
            <div class="col-12 p-0 flex">
              <div class="w-10rem"></div>
              <small style="width: calc(100% - 10rem)">
                <span style="color: red" class="w-full">{{
                  v$.recruitment_proposal_name.required.$message
                    .replace("Value", "Tên đề xuất")
                    .replace("is required", "không được để trống!")
                }}</span>
              </small>
            </div>
          </div>
        </div>

        <div class="col-12 field flex p-0 align-items-center">
          <div class="w-10rem">
            Lý do tuyển <span class="redsao pl-1"> (*)</span>
          </div>
          <div style="width: calc(100% - 10rem)">
            <div class="col-12 p-0">
              <div class="p-inputgroup">
                <Textarea
                  :autoResize="true"
                  rows="1"
                  placeholder="Nhập lý do tuyển dụng"
                  cols="30"
                  v-model="recruitment_proposal.recruitment_proposal_reason"
                  class="w-full"
                  :style="
                    recruitment_proposal.recruitment_proposal_reason
                      ? 'background-color:white !important'
                      : ''
                  "
                  :class="{
                    'p-invalid':
                      v$.recruitment_proposal_reason.$invalid && submitted,
                  }"
                />
              </div>
            </div>
          </div>
        </div>
        <div
          class="col-12 p-0 field flex"
          v-if="
            (v$.recruitment_proposal_reason.$invalid && submitted) ||
            v$.recruitment_proposal_reason.$pending.$response
          "
        >
          <div class="p-0 col-12">
            <div class="col-12 p-0 flex">
              <div class="w-10rem"></div>
              <small style="width: calc(100% - 10rem)">
                <span style="color: red" class="w-full">{{
                  v$.recruitment_proposal_reason.required.$message
                    .replace("Value", "Lý do tuyển")
                    .replace("is required", "không được để trống!")
                }}</span>
              </small>
            </div>
          </div>
        </div>
        <div class="col-12 field p-0 flex text-left align-items-center">
          <div class="col-6 p-0 flex text-left align-items-center">
            <div class="w-10rem">
              Vị trí<span class="redsao pl-1"> (*)</span>
            </div>
            <div style="width: calc(100% - 10rem)">
              <Dropdown
                :filter="true"
                v-model="recruitment_proposal.work_position_id"
                :options="listVacancies"
                optionLabel="name"
                optionValue="code"
                class="w-full"
                panelClass="d-design-dropdown"
                placeholder="Chọn vị trí tuyển dụng"
                :class="{
                  'p-invalid':
                    recruitment_proposal.work_position_id == null && submitted,
                }"
              />
            </div>
          </div>
          <div class="col-6 p-0 flex text-left align-items-center">
            <div class="w-10rem pl-3">Chức vụ</div>
            <div style="width: calc(100% - 10rem)">
              <Dropdown
                :filter="true"
                v-model="recruitment_proposal.position_id"
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
          v-if="recruitment_proposal.work_position_id == null && submitted"
        >
          <div class="p-0 col-6">
            <div class="col-12 p-0 flex">
              <div class="w-10rem"></div>
              <small style="width: calc(100% - 10rem)">
                <span style="color: red" class="w-full"
                  >Vị trí tuyển không được để trống!
                </span>
              </small>
            </div>
          </div>
        </div>
        <div class="col-12 field p-0 flex text-left align-items-center">
          <div class="w-10rem">Phòng ban</div>
          <div style="width: calc(100% - 10rem)">
            <TreeSelect
              v-model="recruitment_proposal.department_id_fake"
              :options="listDepartmentTree"
              :showClear="true"
              :max-height="200"
              optionLabel="data.organization_name"
              optionValue="data.department_id"
              panelClass="d-design-dropdown"
              class="w-full"
              placeholder="Chọn phòng ban"
            >
            </TreeSelect>
          </div>
        </div>

        <div class="col-12 field p-0 flex text-left align-items-center">
          <div class="col-6 p-0 flex text-left align-items-center">
            <div class="w-10rem">Hạn tuyển từ</div>
            <div style="width: calc(100% - 10rem)">
              <Calendar
                class="w-full"
                id="basic_purchase_date"
                v-model="recruitment_proposal.start_date"
                autocomplete="off"
                :showIcon="true" :showOnFocus="false"
                placeholder="dd/mm/yyyy"
              />
            </div>
          </div>
          <div class="col-6 p-0 flex text-left align-items-center">
            <div class="w-10rem pl-3">
              Hạn tuyển đến<span class="redsao pl-1"> (*)</span>
            </div>
            <div style="width: calc(100% - 10rem)">
              <Calendar
                class="w-full"
                placeholder="dd/mm/yyyy"
                id="basic_purchase_date"
                v-model="recruitment_proposal.end_date"
                autocomplete="off" :showOnFocus="false"
                :minDate="
                  recruitment_proposal.start_date
                    ? new Date(recruitment_proposal.start_date)
                    : null
                "
                :showIcon="true"
                :class="{
                  'p-invalid':
                    recruitment_proposal.end_date == null && submitted,
                }"
              />
            </div>
          </div>
        </div>
        <div class="col-12 p-0 flex text-left align-items-center">
          <div class="col-6 p-0 flex text-left align-items-center"></div>
          <div class="col-6 p-0 flex text-left align-items-center">
            <div
              class="col-12 p-0 field flex"
              v-if="recruitment_proposal.end_date == null && submitted"
            >
              <div class="p-0 col-12">
                <div class="col-12 p-0 flex">
                  <div class="w-10rem"></div>
                  <small style="width: calc(100% - 10rem)">
                    <span style="color: red" class="w-full"
                      >Hạn tuyển không được để trống!
                    </span>
                  </small>
                </div>
              </div>
            </div>
          </div>
        </div>
        <div class="col-12 field p-0 flex text-left align-items-center">
          <div class="col-6 p-0 flex text-left align-items-center">
            <div class="col-6 p-0 flex text-left align-items-center">
              <div class="w-10rem">SL nhân sự hiện có</div>
              <div style="width: calc(100% - 10rem)">
                <InputNumber
                  class="w-full"
                  suffix=" Người"
                  disabled
                  v-model="recruitment_proposal.exist_personnel"
                />
              </div>
            </div>
            <div class="col-6 p-0 flex text-left align-items-center">
              <div class="w-10rem pl-3">Số lượng định biên</div>
              <div style="width: calc(100% - 10rem)">
                <InputNumber
                  class="w-full"
                  suffix=" Người"
                  disabled
                  v-model="recruitment_proposal.quantity_margin"
                />
              </div>
            </div>
          </div>
          <div class="col-6 p-0 flex text-left align-items-center">
            <div class="col-6 p-0 flex text-left align-items-center">
              <div class="w-10rem pl-3">
                Số lượng tuyển<span class="redsao pl-1"> (*)</span>
              </div>
              <div style="width: calc(100% - 10rem)">
                <InputNumber
                  class="w-full"
                  suffix=" Người"
                  :class="{
                    'p-invalid': v$.recruits_num.$invalid && submitted,
                  }"
                  v-model="recruitment_proposal.recruits_num"
                />
              </div>
            </div>
            <div class="col-6 p-0 flex text-left align-items-center">
              <div class="w-10rem pl-3">Tuyển đến khi đủ</div>
              <div style="width: calc(100% - 10rem)">
                <InputSwitch
                  class="w-4rem lck-checked"
                  v-model="recruitment_proposal.until_enough"
                />
              </div>
            </div>
          </div>
        </div>
        <div class="col-12 field p-0 flex text-left align-items-center">
          <div class="col-6 p-0 flex text-left align-items-center"></div>
          <div class="col-6 p-0 flex text-left align-items-center">
            <div
              class="col-12 p-0 field flex"
              v-if="recruitment_proposal.recruits_num == null && submitted"
            >
              <div class="p-0 col-12">
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
          </div>
        </div>

        <div class="col-12 field p-0 text-lg font-bold">Yêu cầu ứng viên</div>

        <div class="col-12 field p-0 flex text-left align-items-center">
          <div class="col-6 p-0 flex text-left align-items-center">
            <div class="w-10rem">Trình độ</div>
            <div style="width: calc(100% - 10rem)">
              <Dropdown
                v-model="recruitment_proposal.can_academic_level_id"
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
              <Dropdown
                v-model="recruitment_proposal.can_specialization_id"
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
                <Dropdown
                  v-model="recruitment_proposal.can_experience_id"
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
                <Dropdown
                  v-model="recruitment_proposal.can_language_level_id"
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
                    v-model="recruitment_proposal.can_age_from"
                    class="w-full"
                    :min="0"
                    suffix=" Tuổi"
                    placeholder="Từ"
                  />
                </div>
                <div class="w-full">
                  <InputNumber
                    v-model="recruitment_proposal.can_age_to"
                    class="w-full"
                    :min="
                      recruitment_proposal.can_age_from
                        ? recruitment_proposal.can_age_from + 1
                        : null
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
                  v-model="recruitment_proposal.can_gender"
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
                    v-model="recruitment_proposal.can_height_from"
                    :min="0"
                    class="w-full"
                    suffix=" Cm"
                    placeholder="Từ"
                  />
                </div>
                <div class="w-full">
                  <InputNumber
                    v-model="recruitment_proposal.can_height_to"
                    :min="
                      recruitment_proposal.can_height_from
                        ? recruitment_proposal.can_height_from + 1
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
                    v-model="recruitment_proposal.can_weight_from"
                    :min="0"
                    class="w-full"
                    suffix=" Kg"
                    placeholder="Từ"
                  />
                </div>
                <div class="w-full">
                  <InputNumber
                    v-model="recruitment_proposal.can_weight_to"
                    :min="
                      recruitment_proposal.can_weight_from
                        ? recruitment_proposal.can_weight_from + 1
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
              rows="3"
              cols="40"
              v-model="recruitment_proposal.job_description"
              class="w-full"
            />
          </div>
        </div>

        <div class="col-12 field p-0 text-lg font-bold"  >File đính kèm</div>
        <div class="w-full col-12 field p-0">
          <FileUpload  v-if="!view"
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
      <div class="pt-3">
        <Button
          label="Hủy"
          icon="pi pi-times"
          @click="props.closeDialog"
          class="p-button-outlined"
        />

        <Button  v-if="!view"
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
  