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
  reward: Object,
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
  reward_name_fake1: {
    required,
    $errors: [
      {
        $property: "reward_name_fake1",
        $validator: "required",
        $message: "Tên khen thưởng không được để trống!",
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
  reward_code: {
    required,
    $errors: [
      {
        $property: "reward_code",
        $validator: "required",
        $message: "Số lượng tuyển không được để trống!",
      },
    ],
  },
};
const listFilesS = ref([]);
const reward = ref({
  reward_name_fake1: [],
  is_recruitment_proposal: null,
  reward_code: null,
  reward_name_fake2: {},
});
const submitted = ref(false);
const list_users_training = ref([]);
const list_schedule = ref([]);
const loadData = () => {
  if (props.checkadd == true) {
    list_users_training.value = [];
    list_schedule.value = [];
    reward.value = props.reward;
  } else {
    axios
      .post(
        baseURL + "/api/hrm_ca_SQL/getData",
        {
          str: encr(
            JSON.stringify({
              proc: "hrm_reward_get",
              par: [
                {
                  par: "reward_id",
                  va: props.reward.reward_id,
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
          reward.value = data[0];

          if (reward.value.decision_date)
            reward.value.decision_date = new Date(reward.value.decision_date);
          if (reward.value.effective_date)
            reward.value.effective_date = new Date(reward.value.effective_date);
 
        }

        if (data1) {
          listFilesS.value = data1;
        }
      })
      .catch((error) => {});
  }
};
const saveData = ( ) => {
  submitted.value = true;
   
  
  if (
    reward.value.reward_level_id == null ||
    reward.value.reward_title_id == null
  ) {
    return;
  }
  if (reward.value.reward_type == 1) {
    if (reward.value.reward_name_fake1.length == 0) {
      return;
    }
  } else if (reward.value.reward_type == 2)
    if (reward.value.reward_name_fake2 == {}) {
      return;
    }

  if (reward.value.reward_type == 1)
    if (reward.value.reward_name_fake1.length > 0)
      reward.value.reward_name = reward.value.reward_name_fake1.toString();
  if (reward.value.reward_type == 2) {
    reward.value.reward_name = "";
    let str="";
    if (reward.value.reward_name_fake2)
      Object.keys(reward.value.reward_name_fake2).forEach((key) => {
        reward.value.reward_name += str+ key;
        str=",";
        return;
      });
  }
  let formData = new FormData();
  for (var i = 0; i < filesList.value.length; i++) {
    let file = filesList.value[i];
    formData.append("image", file);
  }

  formData.append("hrm_reward", JSON.stringify(reward.value));
  formData.append("hrm_files", JSON.stringify(listFilesS.value));
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  if (props.checkadd) {
    axios
      .post(baseURL + "/api/hrm_reward/add_hrm_reward", formData, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Thêm thông tin khen thưởng thành công!");

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
      .put(baseURL + "/api/hrm_reward/update_hrm_reward", formData, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa thông tin khen thưởng thành công!");

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
const v$ = useVuelidate(rules, reward);
const listRewardLevel= ref([]);
const listPosition = ref([]);
const listRewardTitle = ref([]);
const listDepartmentTree = ref();
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
      console.log(error);
    });
  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_rec_proposal_list_all",
            par: [
              { par: "status", va: 2 },
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
      listProposal.value = [];
      data.forEach((element) => {
        listProposal.value.push({
          name: element.recruitment_proposal_name,
          code: element.recruitment_proposal_id,
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
            proc: "hrm_ca_reward_level_list",
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
      listRewardLevel.value = [];
      data.forEach(element => {
        listRewardLevel.value.push({
          name: element.reward_level_name,
          code: element.reward_level_id,
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
            proc: "hrm_ca_reward_title_list",
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
      listRewardTitle.value = [];
      data.forEach((element, i) => {
        listRewardTitle.value.push({
          name: element.reward_title_name,
          code: element.reward_title_id,
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

const listTypeReward = ref([
  { name: "Khen thưởng nhân sự", code: 1 },
  { name: "Khen thưởng phòng ban", code: 2 },
]);
const onChangeReward = () => {
  console.log("sssppp");
};
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
      <div class="grid formgrid m-2 mb-0">
        <div class="field col-12 md:col-12 flex format-center">
          <div class="col-6 p-0">
            <SelectButton
              v-model="reward.reward_type"
              :options="listTypeReward"
              optionLabel="name"
              optionValue="code"
              @change="onChangeReward()"
            />
          </div>
        </div>
        <div class="col-12 field p-0 text-lg font-bold">Thông tin chung</div>
        <div class="col-12 p-0" v-if="reward.reward_type == 1">
          <div class="col-12 field flex p-0 align-items-center">
            <div class="w-10rem">
              Nhân sự <span class="redsao pl-1"> (*)</span>
            </div>
            <div style="width: calc(100% - 10rem)">
              <div class="col-12 p-0">
                <div class="p-inputgroup">
                  <MultiSelect
                    v-model="reward.reward_name_fake1"
                    :options="listDropdownUserGive"
                    optionLabel="name"
                    optionValue="code"
                    placeholder="-------- Chọn người nhận khen thưởng --------"
                    panelClass="d-design-dropdown"
                    class="w-full p-0 d-tree-input"
                    :class="{
                      'p-invalid':
                        reward.reward_name_fake1.length == 0 && submitted,
                    }"
                    display="chip"
                  >
                    <template #option="slotProps">
                      <div class="country-item flex align-items-center w-full">
                        <div class="grid w-full p-0">
                          <div
                            class="field p-0 col-12 flex m-0 cursor-pointer align-items-center"
                          >
                            <div class="w-1rem mx-2 p-0 align-items-center">
                              <Avatar
                                style="color: #fff"
                                v-bind:label="
                                  slotProps.option.avatar
                                    ? ''
                                    : slotProps.option.name.substring(
                                        slotProps.option.name.lastIndexOf(' ') +
                                          1,
                                        slotProps.option.name.lastIndexOf(' ') +
                                          2
                                      )
                                "
                                :image="basedomainURL + slotProps.option.avatar"
                                size="small"
                                :style="
                                  slotProps.option.avatar
                                    ? 'background-color: #2196f3'
                                    : 'background:' +
                                      bgColor[slotProps.option.name.length % 7]
                                "
                                shape="circle"
                                @error="
                                  $event.target.src =
                                    basedomainURL + '/Portals/Image/nouser1.png'
                                "
                              />
                            </div>
                            <div
                              style="width: calc(100% - 1rem)"
                              class="p-0 ml-3 align-items-center"
                            >
                              <div class="pt-2">
                                <div class="font-bold">
                                  {{ slotProps.option.name }}
                                </div>
                                <div
                                  class="flex w-full text-sm font-italic text-500"
                                >
                                  <div>
                                    {{ slotProps.option.position_name }}
                                  </div>
                                </div>
                              </div>
                            </div>
                          </div>
                        </div>
                      </div>
                    </template>
                  </MultiSelect>
                </div>
              </div>
            </div>
          </div>
          <div
            class="col-12 p-0 field flex"
            v-if="reward.reward_name_fake1.length == 0 && submitted"
          >
            <div class="p-0 col-12">
              <div class="col-12 p-0 flex">
                <div class="w-10rem"></div>
                <small style="width: calc(100% - 10rem)">
                  <span style="color: red" class="w-full"
                    >Nhân sự được khen thưởng không được để trống!</span
                  >
                </small>
              </div>
            </div>
          </div>
        </div>
        <div class="col-12 p-0" v-if="reward.reward_type == 2">
          <div class="col-12 field flex p-0 align-items-center">
            <div class="w-10rem">
              Phòng ban <span class="redsao pl-1"> (*)</span>
            </div>
            <div style="width: calc(100% - 10rem)">
              <div class="col-12 p-0">
                <div class="p-inputgroup">
                  <TreeSelect
                    v-model="reward.reward_name_fake2"
                    :options="listDepartmentTree"
                    :class="{
                      'p-invalid':
                       Object.keys(reward.reward_name_fake2).length == 0 && submitted,
                    }"
                    :showClear="true"
                    :max-height="200"
                    optionLabel="data.organization_name"
                    optionValue="data.department_id"
                    panelClass="d-design-dropdown"
                    class="w-full d-tree-input "
                    selectionMode="checkbox"
                    placeholder="-------- Chọn phòng ban khen thưởng--------"
                    display="chip"
                  >
                  </TreeSelect>
                </div>
              </div>
            </div>
          </div>
          <div
            class="col-12 p-0 field flex"
            v-if="  Object.keys(reward.reward_name_fake2).length == 0 && submitted"
          >
            <div class="p-0 col-12">
              <div class="col-12 p-0 flex">
                <div class="w-10rem"></div>
                <small style="width: calc(100% - 10rem)">
                  <span style="color: red" class="w-full"
                    >Phòng ban được khen thưởng không được để trống!</span
                  >
                </small>
              </div>
            </div>
          </div>
        </div>
        <div class="col-12 field p-0 flex text-left align-items-center">
          <div class="col-6 p-0 flex text-left align-items-center">
            <div class="w-10rem">
              Cấp khen thưởng <span class="redsao pl-1"> (*)</span>
            </div>
            <div style="width: calc(100% - 10rem)">
              <Dropdown
                :filter="true"
                v-model="reward.reward_level_id"
                :options="listRewardLevel"
                optionLabel="name"
                optionValue="code"
                class="w-full"
                panelClass="d-design-dropdown"
                placeholder="Chọn cấp khen thưởng"
                :class="{
                  'p-invalid': reward.reward_level_id == null && submitted,
                }"
              />
            </div>
          </div>
          <div class="col-6 p-0 flex text-left align-items-center">
            <div class="w-10rem pl-3">
              Danh hiệu <span class="redsao pl-1"> (*)</span>
            </div>
            <div style="width: calc(100% - 10rem)">
              <Dropdown
                :filter="true"
                v-model="reward.reward_title_id"
                :options="listRewardTitle"
                optionLabel="name"
                optionValue="code"
                class="w-full"
                panelClass="d-design-dropdown"
                placeholder="Chọn danh hiệu khen thưởng"
                :class="{
                  'p-invalid': reward.reward_title_id == null && submitted,
                }"
              />
            </div>
          </div>
        </div>
        <div
          class="col-12 p-0 field flex"
          v-if="
            (reward.reward_level_id == null && submitted) ||
            (reward.reward_title_id == null && submitted)
          "
        >
          <div class="p-0 col-6">
            <div
              class="col-12 p-0 flex"
              v-if="reward.reward_level_id == null && submitted"
            >
              <div class="w-10rem"></div>
              <small style="width: calc(100% - 10rem)">
                <span style="color: red" class="w-full"
                  >Cấp khen thưởng không được để trống!</span
                >
              </small>
            </div>
          </div>
          <div class="p-0 col-6">
            <div
              class="col-12 p-0 flex"
              v-if="reward.reward_title_id == null && submitted"
            >
              <div class="w-10rem"></div>
              <small style="width: calc(100% - 10rem)">
                <span style="color: red" class="w-full"
                  >Danh hiệu không được để trống!</span
                >
              </small>
            </div>
          </div>
        </div>

        <div class="col-12 field p-0 flex text-left align-items-center">
          <div class="col-6 p-0 flex text-left align-items-center">
            <div class="w-10rem">Ngày quyết định</div>
            <div style="width: calc(100% - 10rem)">
              <Calendar
                class="w-full"
                id="basic_purchase_date"
                v-model="reward.decision_date"
                autocomplete="off"
                :showIcon="true"
                placeholder="dd/mm/yyyy"
              />
            </div>
          </div>
          <div class="col-6 p-0 flex text-left align-items-center">
            <div class="w-10rem pl-3">Ngày hiệu lực</div>
            <div style="width: calc(100% - 10rem)">
              <Calendar
                class="w-full"
                placeholder="dd/mm/yyyy"
                id="basic_purchase_date"
                v-model="reward.effective_date"
                autocomplete="off"
               
                :showIcon="true"
              />
            </div>
          </div>
        </div>
        <div class="col-12 field p-0 flex text-left align-items-center">
          <div class="col-6 p-0 flex text-left align-items-center">
            <div class="w-10rem">Số quyết định</div>
            <div style="width: calc(100% - 10rem)">
              <InputText
                class="w-full px-2"
                v-model="reward.reward_number"
                placeholder="Nhập số quyết định"
              />
            </div>
          </div>
          <div class="col-6 p-0 flex text-left align-items-center">
            <div class="w-10rem pl-3">Giá trị</div>
            <div style="width: calc(100% - 10rem)">
              <InputNumber
                class="w-full"
                v-model="reward.reward_cost"
                placeholder="Nhập giá trị khen thưởng"
                suffix=" VNĐ"
              />
            </div>
          </div>
        </div>
        <div class="col-12 field p-0 flex text-left align-items-center">
          <div class="w-10rem">Nội dung</div>
          <div style="width: calc(100% - 10rem)">
            <Textarea
              :autoResize="true"
              rows="3"
              placeholder="Nhập nội dung khen thưởng"
              cols="30"
              v-model="reward.reward_content"
              class="w-full"
              :style="
                reward.reward_content ? 'background-color:white !important' : ''
              "
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

          <div class="col-12 p-0">
            <div
              class="p-0 w-full flex"
              v-for="(item, index) in listFilesS"
              :key="index"
            >
              <div class="p-0" style="width: 100%; border-radius: 10px">
                <div class="w-full py-3 flex align-items-center">
                  <div class="flex w-full">
                    <div v-if="item.is_image" class="align-items-center flex">
                      <Image
                        :src="basedomainURL + item.file_path"
                        :alt="item.file_name"
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
                        {{ item.file_name }}
                      </div>
                    </div>
                    <div v-else>
                      <a
                        :href="basedomainURL + item.file_path"
                        download
                        class="w-full no-underline cursor-pointer"
                      >
                        <div class="align-items-center flex">
                          <div>
                            <img
                              :src="
                                basedomainURL +
                                '/Portals/Image/file/' +
                                item.file_path.substring(
                                  item.file_path.lastIndexOf('.') + 1
                                ) +
                                '.png'
                              "
                              style="
                                width: 70px;
                                height: 50px;
                                object-fit: contain;
                              "
                              :alt="item.file_name"
                            />
                          </div>
                          <div class="ml-2" style="word-break: break-all">
                            {{ item.file_name }}
                          </div>
                        </div>
                      </a>
                    </div>
                  </div>
                  <div class="w-3rem align-items-center">
                    <Button
                      icon="pi pi-times"
                      class="p-button-rounded p-button-danger"
                      @click="deleteFileH(item)"
                    />
                  </div>
                </div>
              </div>
            </div>
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
          @click="saveData()"
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
