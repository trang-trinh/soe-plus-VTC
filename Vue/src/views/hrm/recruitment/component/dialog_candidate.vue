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
const emitter = inject("emitter");
const isDynamicSQL = ref(false);
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
  candidate: Object,

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
  candidate_code: {
    required,
    $errors: [
      {
        $property: "candidate_code",
        $validator: "required",
        $message: "Tên ứng viên không được để trống!",
      },
    ],
  },
  candidate_name: {
    required,
    $errors: [
      {
        $property: "candidate_code",
        $validator: "required",
        $message: "Tên ứng viên không được để trống!",
      },
    ],
  },
};
const listFilesS = ref([]);
const candidate = ref({});
const submitted = ref(false);
const list_users_family = ref([]);
const list_schedule = ref([]);
const loadData = () => {
  if (props.checkadd == true) {
    list_users_family.value = [];
    list_schedule.value = [];
    candidate.value = props.candidate;
  } else {
    axios
      .post(
        baseURL + "/api/hrm_ca_SQL/getData",
        {
          str: encr(
            JSON.stringify({
              proc: "hrm_candidate_get",
              par: [
                {
                  par: "candidate_id",
                  va: props.candidate.candidate_id,
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
        let data2 = JSON.parse(response.data.data)[2];
        let data3 = JSON.parse(response.data.data)[3];
        if (data) {
          candidate.value = data[0];

          if (candidate.value.start_date)
            candidate.value.start_date = new Date(candidate.value.start_date);
          if (candidate.value.end_date)
            candidate.value.end_date = new Date(candidate.value.end_date);
          if (candidate.value.registration_deadline)
            candidate.value.registration_deadline = new Date(
              candidate.value.registration_deadline
            );
          candidate.value.user_verify_fake =
            candidate.value.user_verify.split(",");
          candidate.value.user_follows_fake =
            candidate.value.user_follows.split(",");
        }
        candidate.value.organization_training_fake = {};
        candidate.value.organization_training_fake[
          candidate.value.organization_training
        ] = true;

        data1.forEach((element) => {
          element.data = {
            profile_id: element.profile_id,
            avatar: element.avatar,
            profile_user_name: element.profile_user_name,
            department_name: element.department_name,
            department_id: element.department_id,
            work_position_name: element.work_position_name,
            position_name: element.position_name,
            position_id: element.position_id,
            work_position_id: element.work_position_id,
          };
          list_users_family.value.push(element);
        });
        if (list_users_family.value.length > 0) {
          var arr = [...listDataUsersSave.value];
          list_users_family.value.forEach((element) => {
            arr = arr.filter((x) => x.code.profile_id != element.profile_id);
          });
          listDataUsers.value = arr;
        }
        data2.forEach((element) => {
          if (element.date_study)
            element.date_study = new Date(element.date_study);
          if (element.start_time)
            element.start_time = new Date(element.start_time);
          if (element.end_time) element.end_time = new Date(element.end_time);
        });
        list_schedule.value = data2;
        if (data3) {
          listFilesS.value = data3;
        }

        checkShow.value = true;
        checkShow2.value = true;
        checkShow3.value = true;
        checkShow4.value = true;
        checkShow5.value = true;
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
    candidate.value.start_date == null ||
    candidate.value.user_verify_fake == null ||
    candidate.value.form_training == null ||
    candidate.value.candidate_place == null
  ) {
    return;
  }
  if (
    list_users_family.value.filter(
      (x) => x.profile_id == null || x.profile_id == ""
    ).length > 0
  ) {
    return;
  }

  if (candidate.value.candidate_name.length > 250) {
    swal.fire({
      title: "Error!",
      text: "Tên ứng viên không được vượt quá 250 ký tự!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  if (candidate.value.candidate_code.length > 50) {
    swal.fire({
      title: "Error!",
      text: "Mã ứng viên không được vượt quá 50 ký tự!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  list_schedule.value.forEach((element) => {
    if (element.class_schedule_name)
      if (element.class_schedule_name.length >= 250) {
        swal.fire({
          title: "Error!",
          text: "Tồn tại tên nội dung ứng viên vượt quá 250 ký tự!",
          icon: "error",
          confirmButtonText: "OK",
        });
        return;
      } else return;
    if (element.phone_number)
      if (element.phone_number.length >= 11) {
        swal.fire({
          title: "Error!",
          text: "Số điện thoại giảng viên không được vượt quá 11 ký tự!",
          icon: "error",
          confirmButtonText: "OK",
        });
        return;
      }
    if (element.lecturers_name)
      if (element.lecturers_name.length >= 250) {
        swal.fire({
          title: "Error!",
          text: "Tên giảng viên không được vượt quá 250 ký tự!",
          icon: "error",
          confirmButtonText: "OK",
        });
        return;
      }
  });
  if (candidate.value.user_verify_fake.length > 0)
    candidate.value.user_verify = candidate.value.user_verify_fake.toString();
  if (candidate.value.user_follows_fake.length > 0)
    candidate.value.user_follows = candidate.value.user_follows_fake.toString();
  if (candidate.value.organization_training_fake)
    Object.keys(candidate.value.organization_training_fake).forEach((key) => {
      candidate.value.organization_training = Number(key);
    });

  let formData = new FormData();
  for (var i = 0; i < filesList.value.length; i++) {
    let file = filesList.value[i];
    formData.append("image", file);
  }

  formData.append("hrm_candidate", JSON.stringify(candidate.value));
  formData.append("hrm_students", JSON.stringify(list_users_family.value));
  formData.append("hrm_schedule", JSON.stringify(list_schedule.value));
  formData.append("hrm_files", JSON.stringify(listFilesS.value));
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  if (props.checkadd) {
    axios
      .post(baseURL + "/api/hrm_candidate/add_hrm_candidate", formData, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Thêm thông tin ứng viên thành công!");

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
        baseURL + "/api/hrm_candidate/update_hrm_candidate",
        formData,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa thông tin ứng viên thành công!");

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
const v$ = useVuelidate(rules, candidate);
const listFormTraining = ref([
  { name: "Bắt buộc", code: 1 },
  { name: "Đăng ký", code: 2 },
  { name: "Cả hai", code: 3 },
]);
const listStatus = ref([
  { name: "Lên kế hoạch", code: 1 },
  { name: "Đang thực hiện", code: 2 },
  { name: "Đã hoàn thành", code: 3 },
  { name: "Tạm dừng", code: 4 },
  { name: "Đã hủy", code: 5 },
]);
const listObjTraining = ref([
  { name: "Cấp lãnh đạo", code: 1 },
  { name: "Quản lý", code: 2 },
  { name: "Nhân viên", code: 3 },
]);

const checkShow = ref(false);
const checkShow2 = ref(false);
const checkShow3 = ref(false);
const checkShow4 = ref(false);
const checkShow5 = ref(false);
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
  if (type == 4) {
    if (checkShow4.value == true) {
      checkShow4.value = false;
    } else {
      checkShow4.value = true;
    }
  }
  if (type == 5) {
    if (checkShow5.value == true) {
      checkShow5.value = false;
    } else {
      checkShow5.value = true;
    }
  }
};
const addRow_Item = (type) => {
  //relative
  if (type == 1) {
    checkShow.value = true;
  }
  if (type == 2) {
    checkShow2.value = true;
    let obj = {
      is_order: list_users_family.value.length + 1,
      data: null,
      profile_user_name: null,
      work_position_id: null,
      position_name: null,
      position_id: null,
      work_position_name: null,
      note: null,
      department_name: null,
      profile_id: null,
    };
    list_users_family.value.push(obj);
    if (list_users_family.value.length > 0) {
      var arr = [...listDataUsersSave.value];
      list_users_family.value.forEach((element) => {
        arr = arr.filter((x) => x.code.profile_id != element.profile_id);
      });
      listDataUsers.value = arr;
    }
  }
  if (type == 3) {
    let obj = {
      is_order: list_schedule.value.length + 1,
      class_schedule_name: null,
      limit: 1,
      lecturers_name: null,
      phone_number: null,
      date_study: null,
      start_time: null,
      end_time: null,
      training_class_id: null,
    };
    list_schedule.value.push(obj);
    checkShow3.value = true;
  }
  if (type == 4) {
    checkShow4.value = true;
  }
  if (type == 5) {
    checkShow5.value = true;
  }
};
const deleteFileH = (value) => {
  listFilesS.value = listFilesS.value.filter((x) => x.file_id != value.file_id);
};
const treedonvis = ref();
const initTudien = () => {
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
            proc: "hrm_ca_enecting_group_list",
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
      listTrainingGroups.value = [];
      data.forEach((element, i) => {
        listTrainingGroups.value.push({
          name: element.enecting_group_name,
          code: element.enecting_group_id,
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
            proc: "hrm_ca_classroom_list",
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
      listClasroom.value = [];
      data.forEach((element, i) => {
        listClasroom.value.push({
          name: element.classroom_name,
          code: element.classroom_id,
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
const checkImg = (src) => {
  let allowedExtensions = /(\.jpg|\.jpeg|\.png|\.gif)$/i;
  return allowedExtensions.exec(src);
};
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
const changeLecturers = (value, index) => {
  if (value) {
    var arf = listDropdownUserGive.value.find((x) => x.code == value);
    list_schedule.value[index - 1].phone_number = arf.phone_number;
    list_schedule.value[index - 1].avatar = arf.avatar;
    list_schedule.value[index - 1].lecturers_name = arf.name;
  } else {
    list_schedule.value[index - 1].phone_number = null;
    list_schedule.value[index - 1].avatar = null;
    list_schedule.value[index - 1].lecturers_name = null;
  }
};
const changeUserTrainding = (data, index) => {
  if (data && list_users_family.value[index]) {
    list_users_family.value[index].is_order = index + 1;
    list_users_family.value[index].profile_id = data.profile_id;
    list_users_family.value[index].profile_user_name = data.profile_user_name;
    list_users_family.value[index].work_position_id = data.work_position_id;
    list_users_family.value[index].work_position_name =
      data.work_position_name;
    list_users_family.value[index].position_name = data.position_name;
    list_users_family.value[index].position_id = data.position_id;
    list_users_family.value[index].department_id = data.department_id;
    list_users_family.value[index].department_name = data.department_name;
  } else {
    list_users_family.value[index].profile_id = null;
    list_users_family.value[index].profile_user_name = null;
    list_users_family.value[index].work_position_id = null;
    list_users_family.value[index].work_position_name = null;
    list_users_family.value[index].position_name = null;
    list_users_family.value[index].position_id = null;
    list_users_family.value[index].department_id = null;
    list_users_family.value[index].department_name = null;
  }
  if (list_users_family.value.length > 0) {
    var arr = [...listDataUsersSave.value];
    list_users_family.value.forEach((element) => {
      arr = arr.filter((x) => x.code.profile_id != element.profile_id);
    });
    listDataUsers.value = arr;
  }
};
const listClasroom = ref([]);

const delRow_Item = (item, type) => {
  if (type == 1) {
    list_users_family.value.splice(
      list_users_family.value.lastIndexOf(item),
      1
    );
    if (list_users_family.value.length > 0) {
      var arr = [...listDataUsersSave.value];
      list_users_family.value.forEach((element) => {
        arr = arr.filter((x) => x.code.profile_id != element.profile_id);
      });
      listDataUsers.value = arr;
    }
  }
  if (type == 2) {
    list_schedule.value.splice(list_schedule.value.lastIndexOf(item), 1);
  }
};
//Thêm bản ghi
const listTrainingGroups = ref([]);
onMounted(() => {
  loadData();
  initTudien();
  loadUser();
  loadUserProfiles();

  return {};
});
</script>
<template>
  <Dialog
    :header="props.headerDialog"
    v-model:visible="props.displayBasic"
    :style="{ width: '60vw' }"
    :maximizable="true"
    :modal="true"
    :closable="false"
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="col-12 field p-0 text-lg font-bold">Thông tin chung</div>
        <div class="col-12 flex p-0 text-center align-items-center">
          <div class="col-12 field flex p-0 text-left align-items-center">
            <div class="w-10rem">Chiến dịch</div>
            <div style="width: calc(100% - 10rem)">
              <Dropdown
                v-model="candidate.campaign_id"
                :options="listTrainingGroups"
                optionLabel="name"
                optionValue="code"
                placeholder="Chọn chiến dịch ứng viên Apply"
                class="w-full"
              />
            </div>
          </div>
        </div>

        <div class="col-12 field flex p-0 align-items-center">
          <div class="col-6 flex p-0 align-items-center">
            <div class="w-10rem">
              Mã ứng viên<span class="redsao pl-1"> (*)</span>
            </div>
            <div style="width: calc(100% - 10rem)">
              <div class="col-12 p-0">
                <div class="p-inputgroup">
                  <InputText
                    v-model="candidate.candidate_code"
                    class="w-full"
                    :style="
                      candidate.candidate_code
                        ? 'background-color:white !important'
                        : ''
                    "
                    :class="{
                      'p-invalid': v$.candidate_code.$invalid && submitted,
                    }"
                  />
                </div>
              </div>
            </div>
          </div>
          <div class="col-6 flex p-0 align-items-center">
            <div class="w-10rem pl-3">
              Nguồn <span class="redsao pl-1"> (*)</span>
            </div>
            <div style="width: calc(100% - 10rem)">
              <div class="col-12 p-0">
                <div class="p-inputgroup">
                  <Dropdown
                    v-model="candidate.candidate_source"
                    :options="listTrainingGroups"
                    optionLabel="name"
                    optionValue="code"
                    placeholder="Chọn nguồn"
                    class="w-full"
                    :class="{
                      'p-invalid':
                        candidate.candidate_source == null && submitted,
                    }"
                  />
                </div>
              </div>
            </div>
          </div>
        </div>
        <div
          class="col-12 p-0 field flex"
          v-if="
            (v$.candidate_code.$invalid && submitted) ||
            (candidate.candidate_source == null && submitted)
          "
        >
          <div class="col-6 p-0 flex"   v-if="
            (v$.candidate_code.$invalid && submitted) ">
            <div class="w-10rem"></div>
            <small style="width: calc(100% - 10rem)">
              <span style="color: red" class="w-full">{{
                v$.candidate_code.required.$message
                  .replace("Value", "Mã ứng viên")
                  .replace("is required", "không được để trống!")
              }}</span>
            </small>
          </div>
          <div class="col-6 p-0 flex"  v-if="
            (candidate.candidate_source == null && submitted) ">
            <div class="w-10rem"></div>
            <small style="width: calc(100% - 10rem)">
              <span style="color: red" class="w-full">{{
                v$.candidate_source.required.$message
                  .replace("Value", "Nguồn")
                  .replace("is required", "không được để trống!")
              }}</span>
            </small>
          </div>
        </div>
        <div class="col-12 field flex p-0 align-items-center">
          <div class="col-6 flex p-0 align-items-center">
            <div class="w-10rem">
              Họ và tên<span class="redsao pl-1"> (*)</span>
            </div>
            <div style="width: calc(100% - 10rem)">
              <div class="col-12 p-0">
                <div class="p-inputgroup">
                  <InputText
                    v-model="candidate.candidate_name"
                    class="w-full"
                    :style="
                      candidate.candidate_name
                        ? 'background-color:white !important'
                        : ''
                    "
                    :class="{
                      'p-invalid': v$.candidate_name.$invalid && submitted,
                    }"
                  />
                </div>
              </div>
            </div>
          </div>

          <div class="col-6 flex p-0 align-items-center">
            <div class="col-7 flex p-0 align-items-center">
              <div class="w-10rem pl-3">Ngày sinh</div>
              <div style="width: calc(100% - 10rem)">
                <Calendar
                  class="w-full"
                  v-model="candidate.candidate_birthday"
                  autocomplete="on"
                  placeholder="dd/mm/yyyy"
                  :showIcon="true"
                />
              </div>
            </div>

            <div class="col-5 flex p-0 align-items-center">
              <div class="w-6rem pl-3">Giới tính</div>
              <div style="width: calc(100% - 6rem)">
                <div class="col-12 p-0">
                  <div class="p-inputgroup">
                    <Dropdown
                      v-model="candidate.candidate_gender"
                      :options="listTrainingGroups"
                      optionLabel="name"
                      optionValue="code"
                      class="w-full"
                      :class="{
                        'p-invalid':
                          candidate.candidate_gender == null && submitted,
                      }"
                    />
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
        <div
          class="col-12 p-0 field flex"
          v-if="
            (v$.candidate_name.$invalid && submitted)  
          "
        >
          <div class="col-6 p-0 flex">
            <div class="w-10rem"></div>
            <small style="width: calc(100% - 10rem)">
              <span style="color: red" class="w-full">{{
                v$.candidate_name.required.$message
                  .replace("Value", "Họ và tên")
                  .replace("is required", "không được để trống!")
              }}</span>
            </small>
          </div>
        
        </div>
        <div class="col-12 field p-0 flex text-left align-items-center">
          <div class="w-10rem">Nơi sinh</div>
          <div style="width: calc(100% - 10rem)">
            <Dropdown
              v-model="candidate.candidate_place"
              :options="listObjTraining"
              optionLabel="name"
              optionValue="code"
              placeholder="Xã phường, Quận huyện, Tỉnh thành"
              class="w-full"
              panelClass="d-design-dropdown"
               
            />
          </div>
        </div>
        <div class="col-12 field p-0 flex text-left align-items-center">
          <div class="w-10rem">Nguyên quán</div>
          <div style="width: calc(100% - 10rem)">
            <Dropdown
              v-model="candidate.candidate_domicile"
              :options="listObjTraining"
              optionLabel="name"
              optionValue="code"
              placeholder="Xã phường, Quận huyện, Tỉnh thành"
              class="w-full"
              panelClass="d-design-dropdown"
               
            />
          </div>
        </div>
        
        <div class="col-12 field p-0 flex text-left align-items-center">
          <div class="col-6 p-0 flex text-left align-items-center">
            <div class="w-10rem">CMT/Căn cước</div>
            <div style="width: calc(100% - 10rem)">
              <InputText
                v-model="candidate.candidate_identity"
                class="w-full"
                placeholder="0202XXXXXXXX"
              />
            </div>
          </div>
          <div class="col-6 p-0 flex text-left align-items-center">
            <div class="w-10rem pl-3">Ngày cấp</div>
            <div style="width: calc(100% - 10rem)">
              <Calendar
                class="w-full"
                id="basic_purchase_date"
                v-model="candidate.candidate_identity_date"
                autocomplete="on"
                placeholder="dd/mm/yyyy"
                :showIcon="true"
              />
            </div>
          </div>
        </div>
       

        <div class="col-12 field p-0 flex text-left align-items-center">
          <div class="w-10rem">Nơi cấp</div>
          <div style="width: calc(100% - 10rem)">
            <Dropdown
              v-model="candidate.candidate_identity_place"
              :options="listObjTraining"
              optionLabel="name"
              optionValue="code"
              class="w-full"
              panelClass="d-design-dropdown"
           
            />
          </div>
        </div>
        <div class="col-12 field p-0 flex text-left align-items-center">
          <div class="col-6 p-0 flex text-left align-items-center">
            <div class="w-10rem">Tình trạng hôn nhân</div>
            <div style="width: calc(100% - 10rem)">
              <Dropdown
                v-model="candidate.candidate_marital"
                :options="listObjTraining"
                optionLabel="name"
                optionValue="code"
                class="w-full"
                panelClass="d-design-dropdown"
           
              />
            </div>
          </div>
          <div class="col-6 p-0 flex text-left align-items-center">
            <div class="w-10rem pl-3">Quốc tịch</div>
            <div style="width: calc(100% - 10rem)">
              <Dropdown
                v-model="candidate.candidate_nationality"
                :options="listObjTraining"
                optionLabel="name"
                optionValue="code"
                class="w-full"
                panelClass="d-design-dropdown"
              
              />
            </div>
          </div>
        </div>
        <div class="col-12 field p-0 flex text-left align-items-center">
          <div class="col-6 p-0 flex text-left align-items-center">
            <div class="col-6 p-0 flex text-left align-items-center">
              <div class="w-10rem">Chiều cao</div>
              <div style="width: calc(100% - 10rem)">
               <InputNumber v-model="candidate.candidate_height"  class="w-full"  /> 
            
              </div>
            </div>
            <div class="col-6 p-0 flex text-left align-items-center">
              <div class="w-10rem format-center">Cân nặng</div>
              <div style="width: calc(100% - 10rem)">
                <InputNumber v-model="candidate.candidate_weight"  class="w-full"  /> 
               
              </div>
            </div>
          </div>
          <div class="col-6 p-0 flex text-left align-items-center">
            <div class="w-10rem pl-3">Nghĩa vụ quân sự</div>
            <div style="width: calc(100% - 10rem)">
              <Dropdown
                v-model="candidate.candidate_military"
                :options="listObjTraining"
                optionLabel="name"
                optionValue="code"
                class="w-full"
                panelClass="d-design-dropdown"
               
              />
            </div>
          </div>
        </div>
        <div class="col-12 field p-0 flex text-left align-items-center">
          <div class="w-10rem">Giới thiệu bản thân</div>
          <div style="width: calc(100% - 10rem)">
            <Textarea
              :autoResize="true"
              rows="1"
              cols="30"
              v-model="candidate.candidate_introduce"
              placeholder="Sở thích, điểm mạnh, điểm yếu"
              class="w-full"
              panelClass="d-design-dropdown"
            />
          </div>
        </div>

        <div class="col-12 p-0 border-1 border-300 border-solid">
          <div
            class="w-full surface-100 flex border-bottom-1 border-200 p-3 cursor-pointer"
            @click="showHidePanel(1)"
          >
            <div class="font-bold flex align-items-center w-full">
              <i
                class="pi pi-angle-right"
                v-if="checkShow == false"
                style="font-size: 1.25rem"
              ></i>
              <i
                class="pi pi-angle-down"
                v-if="checkShow == true"
                style="font-size: 1.25rem"
              ></i>
              <div class="pl-2">Thông tin liên hệ</div>
            </div>
            <div class="w-1 text-right" v-if="!view"></div>
          </div>

          <div class="w-full p-3" v-if="checkShow == true">
            <div class="col-12 field p-0 flex text-left align-items-center">
              <div class="w-10rem">Điện thoại</div>
              <div style="width: calc(100% - 10rem)">
                <Chips v-model="candidate.candidate_phone" class="w-full" />
              </div>
              <!-- placeholder="Nhập số điện thoại, Enter để nhập nhiều" -->
            </div>
            <div class="col-12 field p-0 flex text-left align-items-center">
              <div class="w-10rem">Email</div>
              <div style="width: calc(100% - 10rem)">
                <Chips v-model="candidate.candidate_email" class="w-full" />
              </div>
              <!-- placeholder="Nhập số điện thoại, Enter để nhập nhiều" -->
            </div>
            <div class="col-12 field p-0 flex text-left align-items-center">
              <div class="col-6 p-0 flex text-left align-items-center">
                <div class="w-10rem">Thường trú</div>
                <div style="width: calc(100% - 10rem)">
                  <InputText
                    v-model="candidate.resident"
                    class="w-full"
                  />
                </div>
              </div>
              <div class="col-6 p-0 flex text-left align-items-center">
                <div class="w-10rem pl-3">Địa chỉ</div>
                <div style="width: calc(100% - 10rem)">
                  <Dropdown
                    v-model="candidate.resident_address"
                    :options="listObjTraining"
                    optionLabel="name"
                    optionValue="code"
                    class="w-full"
                    panelClass="d-design-dropdown"
                
                  />
                </div>
              </div>
              <!-- placeholder="Nhập số điện thoại, Enter để nhập nhiều" -->
            </div>
            <div class="col-12 field p-0 flex text-left align-items-center">
              <div class="col-6 p-0 flex text-left align-items-center">
                <div class="w-10rem">Chỗ ở hiện tại</div>
                <div style="width: calc(100% - 10rem)">
                  <InputText
                    v-model="candidate.resident_current"
                    class="w-full"
                  />
                </div>
              </div>
              <div class="col-6 p-0 flex text-left align-items-center">
                <div class="w-10rem pl-3">Địa chỉ</div>
                <div style="width: calc(100% - 10rem)">
                  <Dropdown
                    v-model="candidate.resident_curent_address"
                    :options="listObjTraining"
                    optionLabel="name"
                    optionValue="code"
                    class="w-full"
                    panelClass="d-design-dropdown"
                   
                  />
                </div>
              </div>
              <!-- placeholder="Nhập số điện thoại, Enter để nhập nhiều" -->
            </div>
          </div>
        </div>

        <div class="col-12 p-0 border-1 border-300 border-solid">
          <div
            class="w-full surface-100 flex border-bottom-1 border-200 p-3 cursor-pointer"
            @click="showHidePanel(2)"
          >
            <div class="font-bold flex align-items-center w-full">
              <i
                class="pi pi-angle-right"
                v-if="checkShow2 == false"
                style="font-size: 1.25rem"
              ></i>
              <i
                class="pi pi-angle-down"
                v-if="checkShow2 == true"
                style="font-size: 1.25rem"
              ></i>
              <div class="pl-2">
                Thông tin gia đình
                <span v-if="list_users_family.length > 0">
                  ( {{ list_users_family.length }} )</span
                >
              </div>
            </div>
            <div class="w-1 text-right" v-if="!view">
              <a
                @click="addRow_Item(2)"
                class="hover"
                v-tooltip.top="'Thêm học viên'"
              >
                <i class="pi pi-plus-circle" style="font-size: 18px"></i>
              </a>
            </div>
          </div>

          <div class="w-full p-0" v-if="checkShow2 == true">
            <div v-if="list_users_family.length > 0">
              <DataTable
                :value="list_users_family"
                :scrollable="true"
                :lazy="true"
                :rowHover="true"
                :showGridlines="true"
                scrollDirection="both"
              >
                <Column
                  field="card_number"
                  header="STT"
                  headerStyle="text-align:center;width:70px;height:50px"
                  bodyStyle="text-align:center;width:70px;"
                  class="align-items-center justify-content-center text-center"
                >
                  <template #body="slotProps">
                    {{ slotProps.data.is_order }}
                 
                  </template>
                </Column>
                <Column
                  field="form"
                  header="Mối quan hệ"
                  headerStyle="text-align:center;width:250px;height:50px"
                  bodyStyle="text-align:center;width:250px;"
                  class="align-items-center justify-content-center text-center"
                >
                  <template #body="slotProps">
                    <div class="w-full">
                      <Dropdown
                    v-model="slotProps.data.relationship_id"
                    :options="listObjTraining"
                    optionLabel="name"
                    optionValue="code"
                    class="w-full"
                    panelClass="d-design-dropdown"
                   
                  />

                    </div>
                  </template>
                </Column>
                <Column
                  field="form"
                  header="Họ và tên"
                  headerStyle="text-align:center;width:250px;height:50px"
                  bodyStyle="text-align:center;width:250px;"
                  class="align-items-center justify-content-center text-center"
                >
                  <template #body="slotProps">
                    <div class="w-full">
<InputText v-model="slotProps.data.full_name" />

                    </div>
                  </template>
                </Column>
                <Column
                  field="start_date"
                  header="Ngày sinh"
                  headerStyle="text-align:center;width:200px;height:50px"
                  bodyStyle="text-align:center;width:200px;"
                  class="align-items-center justify-content-center text-center"
                >
                  <template #body="slotProps">
                    <Calendar
                  class="w-full"
                  v-model="slotProps.data.birthday"
                  autocomplete="on"
                  placeholder="dd/mm/yyyy"
                  :showIcon="true"
                />
                  </template>
                </Column>
                <Column
                  field="end_date"
                  header="Nghề nghiệp"
                  headerStyle="text-align:center;width:180px;height:50px"
                  bodyStyle="text-align:center;width:180px;"
                  class="align-items-center justify-content-center text-center"
                >
                  <template #body="slotProps">
                    <Dropdown
                    v-model="slotProps.data.major_id"
                    :options="listObjTraining"
                    optionLabel="name"
                    optionValue="code"
                    class="w-full"
                    panelClass="d-design-dropdown"
                   
                  />
                  </template>
                </Column>
                <Column
                  field="admission_place"
                  header="Điện thoại"
                  headerStyle="text-align:center;width:180px;height:50px"
                  bodyStyle="text-align:center;width:180px;"
                  class="align-items-center justify-content-center text-center"
                >
                  <template #body="slotProps">
                    <InputText
                      spellcheck="false"
                      class="w-full h-full d-design-it"
                      style="width: 170px"
                      v-model="slotProps.data.position_name"
                      disabled
                    />
                  </template>
                </Column>
                <Column
                  field="transfer_place"
                  header="Địa chỉ"
                  headerStyle="text-align:center;width:300px ;height:50px"
                  bodyStyle="text-align:center ;width:300px;"
                  class="align-items-center justify-content-center text-center"
                >
                  <template #body="slotProps">
                    <InputText
                      v-model="slotProps.data.note"
                      spellcheck="false"
                      type="text"
                      class="w-full h-full"
                      maxLength="250"
                    />
                  </template>
                </Column>
                <Column
                  header=""
                  headerStyle="text-align:center;width:50px"
                  bodyStyle="text-align:center;width:50px"
                  class="align-items-center justify-content-center text-center"
                >
                  <template #body="slotProps">
                    <a
                      @click="delRow_Item(slotProps.data, 1)"
                      class="hover cursor-pointer"
                      v-tooltip.top="'Xóa học viên'"
                    >
                      <i class="pi pi-times-circle" style="font-size: 18px"></i>
                    </a>
                  </template>
                </Column>
                <template #empty>
                  <div
                    class="align-items-center justify-content-center p-4 text-center m-auto"
                    style="display: flex; width: 100%; min-height: 200px"
                  ></div>
                </template>
              </DataTable>
            </div>
          </div>
        </div>
        <div
          class="col-12 p-0 border-1 border-300 border-solid cursor-pointer"
          @click="showHidePanel(3)"
        >
          <div class="w-full surface-100 flex border-bottom-1 border-200 p-3">
            <div class="font-bold flex align-items-center w-full">
              <i
                class="pi pi-angle-right"
                v-if="checkShow3 == false"
                style="font-size: 1.25rem"
              ></i>
              <i
                class="pi pi-angle-down"
                v-if="checkShow3 == true"
                style="font-size: 1.25rem"
              ></i>
              <div class="pl-2">
                Trình độ học vấn
                <span v-if="list_schedule.length > 0">
                  ( {{ list_schedule.length }} )</span
                >
              </div>
            </div>
            <div class="w-1 text-right" v-if="!view">
              <a
                @click="addRow_Item(3)"
                class="hover"
                v-tooltip.top="'Thêm lịch học'"
              >
                <i class="pi pi-plus-circle" style="font-size: 18px"></i>
              </a>
            </div>
          </div>

          <div class="w-full px-0 pt-0" v-if="checkShow3 == true">
            <div style="overflow-x: scroll" v-if="list_schedule.length > 0">
              <DataTable
                :value="list_schedule"
                :scrollable="true"
                :lazy="true"
                :rowHover="true"
                :showGridlines="true"
                scrollDirection="both"
              >
                <Column
                  field="card_number"
                  header="STT"
                  headerStyle="text-align:center;width:70px;height:50px"
                  bodyStyle="text-align:center;width:70px;"
                  class="align-items-center justify-content-center text-center"
                >
                  <template #body="slotProps">
                    {{ slotProps.data.is_order }}
                  </template>
                </Column>
                <Column
                  field="form"
                  header="Từ tháng"
                  headerStyle="text-align:center;width:250px;height:50px"
                  bodyStyle="text-align:center;width:250px;"
                  class="align-items-center justify-content-center text-center"
                >
                  <template #body="slotProps">
                    <div class="w-full">
                      <Calendar
                        class="w-full"
                        id="basic_purchase_date"
                        v-model="slotProps.data.date_study"
                        autocomplete="on"
                        :showIcon="true"
                        placeholder="dd/mm/yyyy"
                        :maxDate="
                          candidate.end_date
                            ? new Date(candidate.end_date)
                            : null
                        "
                        :minDate="
                          candidate.start_date
                            ? new Date(candidate.start_date)
                            : null
                        "
                      />
                    </div>
                  </template>
                </Column>
                <Column
                  field="start_date"
                  header="Đến tháng"
                  headerStyle="text-align:center;width:150px;height:50px"
                  bodyStyle=" width:150px;"
                  class="align-items-center justify-content-center"
                >
                  <template #body="slotProps">
                    <Calendar
                      class="w-full"
                      id="basic_purchase_date"
                      v-model="slotProps.data.date_study"
                      autocomplete="on"
                      :showIcon="true"
                      placeholder="dd/mm/yyyy"
                      :maxDate="
                        candidate.end_date ? new Date(candidate.end_date) : null
                      "
                      :minDate="
                        candidate.start_date
                          ? new Date(candidate.start_date)
                          : null
                      "
                    />
                  </template>
                </Column>

                <Column
                  field="admission_place"
                  header="Bằng cấp, trình độ"
                  headerStyle="text-align:center;width:150px;height:50px"
                  bodyStyle="text-align:center;width:150px;"
                  class="align-items-center justify-content-center text-center"
                >
                  <template #body="slotProps">
                    <InputText
                      spellcheck="false"
                      class="w-full"
                      v-model="slotProps.data.phone_number"
                    />
                  </template>
                </Column>
                <Column
                  field="transfer_place"
                  header="Nơi đào tạo"
                  headerStyle="text-align:center;width:180px ;height:50px"
                  bodyStyle="text-align:center ;width:180px;"
                  class="align-items-center justify-content-center text-center"
                >
                  <template #body="slotProps">
                    <InputText
                      spellcheck="false"
                      class="w-full"
                      v-model="slotProps.data.phone_number"
                    />
                  </template>
                </Column>
                <Column
                  field="transfer_place"
                  header="Chuyên ngành"
                  headerStyle="text-align:center;width:7rem ;height:50px"
                  bodyStyle="text-align:center ;width:7rem;"
                  class="align-items-center justify-content-center text-center"
                >
                  <template #body="slotProps">
                    <InputText
                      spellcheck="false"
                      class="w-full"
                      v-model="slotProps.data.phone_number"
                    />
                  </template>
                </Column>

                <Column
                  field="transfer_place"
                  header="Hình thức đào tạo"
                  headerStyle="text-align:center;width:16rem ;height:50px"
                  bodyStyle="  width:16rem;"
                  class="align-items-center justify-content-center"
                >
                  <template #body="slotProps">
                    <InputText
                      v-model="slotProps.data.note"
                      spellcheck="false"
                      type="text"
                      class="w-full h-full"
                      maxLength="250"
                    />
                  </template>
                </Column>
                <Column
                  header=""
                  headerStyle="text-align:center;width:50px"
                  bodyStyle="text-align:center;width:50px"
                  class="align-items-center justify-content-center text-center"
                >
                  <template #body="slotProps">
                    <a
                      @click="delRow_Item(slotProps.data, 2)"
                      class="hover cursor-pointer"
                      v-tooltip.top="'Xóa lịch học'"
                    >
                      <i class="pi pi-times-circle" style="font-size: 18px"></i>
                    </a>
                  </template>
                </Column>
                <template #empty>
                  <div
                    class="align-items-center justify-content-center p-4 text-center m-auto"
                    style="display: flex; width: 100%; min-height: 200px"
                  ></div>
                </template>
              </DataTable>
            </div>
          </div>
        </div>
        <div
          class="col-12 p-0 border-1 border-300 border-solid cursor-pointer"
          @click="showHidePanel(4)"
        >
          <div class="w-full surface-100 flex border-bottom-1 border-200 p-3">
            <div class="font-bold flex align-items-center w-full">
              <i
                class="pi pi-angle-right"
                v-if="checkShow4 == false"
                style="font-size: 1.25rem"
              ></i>
              <i
                class="pi pi-angle-down"
                v-if="checkShow4 == true"
                style="font-size: 1.25rem"
              ></i>
              <div class="pl-2">
                Kinh nghiệm làm việc
                <span v-if="list_schedule.length > 0">
                  ( {{ list_schedule.length }} )</span
                >
              </div>
            </div>
            <div class="w-1 text-right" v-if="!view">
              <a
                @click="addRow_Item(4)"
                class="hover"
                v-tooltip.top="'Thêm lịch học'"
              >
                <i class="pi pi-plus-circle" style="font-size: 18px"></i>
              </a>
            </div>
          </div>

          <div class="w-full px-0 pt-0" v-if="checkShow4 == true">
            <div style="overflow-x: scroll" v-if="list_schedule.length > 0">
              <DataTable
                :value="list_schedule"
                :scrollable="true"
                :lazy="true"
                :rowHover="true"
                :showGridlines="true"
                scrollDirection="both"
              >
                <Column
                  field="card_number"
                  header="STT"
                  headerStyle="text-align:center;width:70px;height:50px"
                  bodyStyle="text-align:center;width:70px;"
                  class="align-items-center justify-content-center text-center"
                >
                  <template #body="slotProps">
                    {{ slotProps.data.is_order }}
                  </template>
                </Column>
                <Column
                  field="form"
                  header="Từ tháng"
                  headerStyle="text-align:center;width:250px;height:50px"
                  bodyStyle="text-align:center;width:250px;"
                  class="align-items-center justify-content-center text-center"
                >
                  <template #body="slotProps">
                    <div class="w-full">
                      <Calendar
                        class="w-full"
                        id="basic_purchase_date"
                        v-model="slotProps.data.date_study"
                        autocomplete="on"
                        :showIcon="true"
                        placeholder="dd/mm/yyyy"
                        :maxDate="
                          candidate.end_date
                            ? new Date(candidate.end_date)
                            : null
                        "
                        :minDate="
                          candidate.start_date
                            ? new Date(candidate.start_date)
                            : null
                        "
                      />
                    </div>
                  </template>
                </Column>
                <Column
                  field="start_date"
                  header="Đến tháng"
                  headerStyle="text-align:center;width:150px;height:50px"
                  bodyStyle=" width:150px;"
                  class="align-items-center justify-content-center"
                >
                  <template #body="slotProps">
                    <Calendar
                      class="w-full"
                      id="basic_purchase_date"
                      v-model="slotProps.data.date_study"
                      autocomplete="on"
                      :showIcon="true"
                      placeholder="dd/mm/yyyy"
                      :maxDate="
                        candidate.end_date ? new Date(candidate.end_date) : null
                      "
                      :minDate="
                        candidate.start_date
                          ? new Date(candidate.start_date)
                          : null
                      "
                    />
                  </template>
                </Column>
                <Column
                  field="end_date"
                  header="Công ty"
                  headerStyle="text-align:center;width:16rem;height:50px"
                  bodyStyle="width:16rem;"
                  class="align-items-center justify-content-center"
                >
                  <template #body="slotProps">
                    <InputText
                      spellcheck="false"
                      class="w-full"
                      v-model="slotProps.data.lecturers_name"
                    />
                  </template>
                </Column>
                <Column
                  field="admission_place"
                  header="Vị trí"
                  headerStyle="text-align:center;width:150px;height:50px"
                  bodyStyle="text-align:center;width:150px;"
                  class="align-items-center justify-content-center text-center"
                >
                  <template #body="slotProps">
                    <InputText
                      spellcheck="false"
                      class="w-full"
                      v-model="slotProps.data.phone_number"
                    />
                  </template>
                </Column>
                <Column
                  field="transfer_place"
                  header="Người tham chiếu"
                  headerStyle="text-align:center;width:180px ;height:50px"
                  bodyStyle="text-align:center ;width:180px;"
                  class="align-items-center justify-content-center text-center"
                >
                  <template #body="slotProps">
                    <InputText
                      spellcheck="false"
                      class="w-full"
                      v-model="slotProps.data.phone_number"
                    />
                  </template>
                </Column>
                <Column
                  field="transfer_place"
                  header="Điện thoại"
                  headerStyle="text-align:center;width:7rem ;height:50px"
                  bodyStyle="text-align:center ;width:7rem;"
                  class="align-items-center justify-content-center text-center"
                >
                  <template #body="slotProps">
                    <InputText
                      spellcheck="false"
                      class="w-full"
                      v-model="slotProps.data.phone_number"
                    />
                  </template>
                </Column>

                <Column
                  field="transfer_place"
                  header="Mô tả công việc"
                  headerStyle="text-align:center;width:16rem ;height:50px"
                  bodyStyle="  width:16rem;"
                  class="align-items-center justify-content-center"
                >
                  <template #body="slotProps">
                    <InputText
                      v-model="slotProps.data.note"
                      spellcheck="false"
                      type="text"
                      class="w-full h-full"
                      maxLength="250"
                    />
                  </template>
                </Column>
                <Column
                  header=""
                  headerStyle="text-align:center;width:50px"
                  bodyStyle="text-align:center;width:50px"
                  class="align-items-center justify-content-center text-center"
                >
                  <template #body="slotProps">
                    <a
                      @click="delRow_Item(slotProps.data, 2)"
                      class="hover cursor-pointer"
                      v-tooltip.top="'Xóa lịch học'"
                    >
                      <i class="pi pi-times-circle" style="font-size: 18px"></i>
                    </a>
                  </template>
                </Column>
                <template #empty>
                  <div
                    class="align-items-center justify-content-center p-4 text-center m-auto"
                    style="display: flex; width: 100%; min-height: 200px"
                  ></div>
                </template>
              </DataTable>
            </div>
          </div>
        </div>
        <div class="col-12 p-0 border-1 border-300 border-solid">
          <div
            class="w-full surface-100 flex border-bottom-1 border-200 p-3 cursor-pointer"
            @click="showHidePanel(5)"
          >
            <div class="font-bold flex align-items-center w-full">
              <i
                class="pi pi-angle-right"
                v-if="checkShow5 == false"
                style="font-size: 1.25rem"
              ></i>
              <i
                class="pi pi-angle-down"
                v-if="checkShow5 == true"
                style="font-size: 1.25rem"
              ></i>
              <div class="pl-2">File đính kèm</div>
            </div>
          </div>
          <div class="w-full" v-if="checkShow5 == true">
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
                <div
                  class="p-0 surface-50"
                  style="width: 100%; border-radius: 10px"
                >
                  <Toolbar class="w-full py-3">
                    <template #start>
                      <div class="flex">
                        <div
                          v-if="item.is_image"
                          class="align-items-center flex"
                        >
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
                          <div class="ml-2">
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
                              <div class="ml-2">
                                {{ item.file_name }}
                              </div>
                            </div>
                          </a>
                        </div>
                      </div>
                    </template>
                    <template #end>
                      <Button
                        icon="pi pi-times"
                        class="p-button-rounded p-button-danger"
                        @click="deleteFileH(item)"
                      />
                    </template>
                  </Toolbar>
                </div>
              </div>
            </div>
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

        <Button
          v-if="!view"
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
