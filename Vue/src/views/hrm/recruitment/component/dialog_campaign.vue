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
  training_emps: Object,

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
  training_emps_code: {
    required,
    $errors: [
      {
        $property: "training_emps_code",
        $validator: "required",
        $message: "Tên đào tạo không được để trống!",
      },
    ],
  },
  training_emps_name: {
    required,
    $errors: [
      {
        $property: "training_emps_code",
        $validator: "required",
        $message: "Tên đào tạo không được để trống!",
      },
    ],
  },
};
const listFilesS = ref([]);
const training_emps = ref({});
const submitted = ref(false);
const list_users_training = ref([]);
const list_schedule = ref([]);
const loadData = () => {
  if (props.checkadd == true) {
    list_users_training.value = [];
    list_schedule.value = [];
    training_emps.value = props.training_emps;
  } else {
    axios
      .post(
        baseURL + "/api/hrm_ca_SQL/getData",
        {
          str: encr(
            JSON.stringify({
              proc: "hrm_training_emps_get",
              par: [
                {
                  par: "training_emps_id",
                  va: props.training_emps.training_emps_id,
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
          training_emps.value = data[0];

          if (training_emps.value.start_date)
            training_emps.value.start_date = new Date(
              training_emps.value.start_date
            );
          if (training_emps.value.end_date)
            training_emps.value.end_date = new Date(
              training_emps.value.end_date
            );
          if (training_emps.value.registration_deadline)
            training_emps.value.registration_deadline = new Date(
              training_emps.value.registration_deadline
            );
          training_emps.value.user_verify_fake =
            training_emps.value.user_verify.split(",");
          training_emps.value.user_follows_fake =
            training_emps.value.user_follows.split(",");
        }
        training_emps.value.organization_training_fake = {};
        training_emps.value.organization_training_fake[
          training_emps.value.organization_training
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
          list_users_training.value.push(element);
        });
        if (list_users_training.value.length > 0) {
          var arr = [...listDataUsersSave.value];
          list_users_training.value.forEach((element) => {
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
    training_emps.value.start_date == null ||
    training_emps.value.user_verify_fake == null ||
    training_emps.value.form_training == null ||
    training_emps.value.obj_training == null
  ) {
    return;
  }
  if (
    list_users_training.value.filter(
      (x) => x.profile_id == null || x.profile_id == ""
    ).length > 0
  ) {
    return;
  }

  if (training_emps.value.training_emps_name.length > 250) {
    swal.fire({
      title: "Error!",
      text: "Tên đào tạo không được vượt quá 250 ký tự!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  if (training_emps.value.training_emps_code.length > 50) {
    swal.fire({
      title: "Error!",
      text: "Mã đào tạo không được vượt quá 50 ký tự!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  list_schedule.value.forEach((element) => {
     if(element.class_schedule_name)
    if (element.class_schedule_name.length >= 250) {
      swal.fire({
        title: "Error!",
        text: "Tồn tại tên nội dung đào tạo vượt quá 250 ký tự!",
        icon: "error",
        confirmButtonText: "OK",
      });
      return;
    }
    else
    return
    if(element.phone_number)
    if (element.phone_number.length >= 11) {
      swal.fire({
        title: "Error!",
        text: "Số điện thoại giảng viên không được vượt quá 11 ký tự!",
        icon: "error",
        confirmButtonText: "OK",
      });
      return;
    }if(element.lecturers_name)
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
  if (training_emps.value.user_verify_fake.length > 0)
    training_emps.value.user_verify =
      training_emps.value.user_verify_fake.toString();
  if (training_emps.value.user_follows_fake.length > 0)
    training_emps.value.user_follows =
      training_emps.value.user_follows_fake.toString();
  if (training_emps.value.organization_training_fake)
    Object.keys(training_emps.value.organization_training_fake).forEach(
      (key) => {
        training_emps.value.organization_training = Number(key);
      }
    );

  let formData = new FormData();
  for (var i = 0; i < filesList.value.length; i++) {
    let file = filesList.value[i];
    formData.append("image", file);
  }

  formData.append("hrm_training_emps", JSON.stringify(training_emps.value));
  formData.append("hrm_students", JSON.stringify(list_users_training.value));
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
      .post(
        baseURL + "/api/hrm_training_emps/add_hrm_training_emps",
        formData,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Thêm thông tin đào tạo thành công!");

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
        baseURL + "/api/hrm_training_emps/update_hrm_training_emps",
        formData,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa thông tin đào tạo thành công!");

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
const v$ = useVuelidate(rules, training_emps);
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
const addRow_Item = (type) => {
  //relative
  if (type == 1) {
    checkShow.value = true;

    let obj = {
      is_order: list_users_training.value.length + 1,
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
    list_users_training.value.push(obj);
    if (list_users_training.value.length > 0) {
      var arr = [...listDataUsersSave.value];
      list_users_training.value.forEach((element) => {
        arr = arr.filter((x) => x.code.profile_id != element.profile_id);
      });
      listDataUsers.value = arr;
    }
  }
  if (type == 2) {
    checkShow2.value = true;
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
  }
  if (type == 3) {
    checkShow3.value = true;
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
  if (data && list_users_training.value[index]) {
    list_users_training.value[index].is_order = index + 1;
    list_users_training.value[index].profile_id = data.profile_id;
    list_users_training.value[index].profile_user_name = data.profile_user_name;
    list_users_training.value[index].work_position_id = data.work_position_id;
    list_users_training.value[index].work_position_name =
      data.work_position_name;
    list_users_training.value[index].position_name = data.position_name;
    list_users_training.value[index].position_id = data.position_id;
    list_users_training.value[index].department_id = data.department_id;
    list_users_training.value[index].department_name = data.department_name;
  } else {
    list_users_training.value[index].profile_id = null;
    list_users_training.value[index].profile_user_name = null;
    list_users_training.value[index].work_position_id = null;
    list_users_training.value[index].work_position_name = null;
    list_users_training.value[index].position_name = null;
    list_users_training.value[index].position_id = null;
    list_users_training.value[index].department_id = null;
    list_users_training.value[index].department_name = null;
  }
  if (list_users_training.value.length > 0) {
    var arr = [...listDataUsersSave.value];
    list_users_training.value.forEach((element) => {
      arr = arr.filter((x) => x.code.profile_id != element.profile_id);
    });
    listDataUsers.value = arr;
  }
};
const listClasroom = ref([]);

const delRow_Item = (item, type) => {
  if (type == 1) {
    list_users_training.value.splice(
      list_users_training.value.lastIndexOf(item),
      1
    );
    if (list_users_training.value.length > 0) {
      var arr = [...listDataUsersSave.value];
      list_users_training.value.forEach((element) => {
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
    :style="{ width: '55vw' }"
    :maximizable="true"
    :modal="true"
    :closable="false"
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="col-12 field p-0 text-lg font-bold">Thông tin chiến dịch</div>
        
      
        <div class="col-12 field flex p-0 align-items-center">
          <div class="w-10rem">
            Tên chiến dịch<span class="redsao pl-1"> (*)</span>
          </div>
          <div style="width: calc(100% - 10rem)">
            <div class="col-12 p-0">
              <div class="p-inputgroup">
                <Textarea
                  :autoResize="true"
                  rows="1"
                  cols="30"
                  v-model="training_emps.training_emps_name"
                  class="w-full"
                  :style="
                    training_emps.training_emps_name
                      ? 'background-color:white !important'
                      : ''
                  "
                  :class="{
                    'p-invalid': v$.training_emps_name.$invalid && submitted,
                  }"
                />
              </div>
            </div>
          </div>
        </div>
        <div
          class="col-12 p-0 field flex"
          v-if="
            (v$.training_emps_name.$invalid && submitted) ||
            v$.training_emps_name.$pending.$response
          "
        >
          <div class="p-0 col-12">
            <div class="col-12 p-0 flex">
              <div class="w-10rem"></div>
              <small style="width: calc(100% - 10rem)">
                <span style="color: red" class="w-full">{{
                  v$.training_emps_name.required.$message
                    .replace("Value", "Tên khóa đào tạo")
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
              <MultiSelect
                v-model="training_emps.user_verify_fake"
                :options="listDropdownUserGive"
                optionLabel="name"
                optionValue="code"
                placeholder="-------- Chọn người phụ trách --------"
                panelClass="d-design-dropdown"
                class="w-full p-0 d-tree-input"
                :class="{
                  'p-invalid':
                    training_emps.user_verify_fake == null && submitted,
                }"
                display="chip"
              >
                <template #option="slotProps">
                  <div class="country-item flex align-items-center">
                    <div class="grid w-full p-0">
                      <div
                        class="
                          field
                          p-0
                          py-1
                          col-12
                          flex
                          m-0
                          cursor-pointer
                          align-items-center
                        "
                      >
                        <div class="col-1 mx-2 p-0 align-items-center">
                          <Avatar
                            v-bind:label="
                              slotProps.option.avatar
                                ? ''
                                : slotProps.option.name.substring(
                                    slotProps.option.name.lastIndexOf(' ') + 1,
                                    slotProps.option.name.lastIndexOf(' ') + 2
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
                        <div class="col-11 p-0 ml-3 align-items-center">
                          <div class="pt-2">
                            <div class="font-bold">
                              {{ slotProps.option.name }}
                            </div>
                            <div
                              class="flex w-full text-sm font-italic text-500"
                            >
                              <div>{{ slotProps.option.position_name }}</div>
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
          <div class="col-6 p-0 flex text-left align-items-center">
            <div class="w-10rem pl-3">Người theo dõi</div>
            <div style="width: calc(100% - 10rem)">
              <MultiSelect
                v-model="training_emps.user_follows_fake"
                :options="listDropdownUserGive"
                optionLabel="name"
                optionValue="code"
                placeholder="-------- Chọn người theo dõi --------"
                panelClass="d-design-dropdown"
                class="w-full p-0 d-tree-input"
                display="chip"
              >
                <template #option="slotProps">
                  <div class="country-item flex align-items-center">
                    <div class="grid w-full p-0">
                      <div
                        class="
                          field
                          p-0
                          py-1
                          col-12
                          flex
                          m-0
                          cursor-pointer
                          align-items-center
                        "
                      >
                        <div class="col-1 mx-2 p-0 align-items-center">
                          <Avatar
                            v-bind:label="
                              slotProps.option.avatar
                                ? ''
                                : slotProps.option.name.substring(
                                    slotProps.option.name.lastIndexOf(' ') + 1,
                                    slotProps.option.name.lastIndexOf(' ') + 2
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
                        <div class="col-11 p-0 ml-3 align-items-center">
                          <div class="pt-2">
                            <div class="font-bold">
                              {{ slotProps.option.name }}
                            </div>
                            <div
                              class="flex w-full text-sm font-italic text-500"
                            >
                              <div>{{ slotProps.option.position_name }}</div>
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
        <div
          class="col-12 p-0 field flex"
          v-if="training_emps.user_verify_fake == null && submitted"
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
                <InputNumber class="w-full" suffix=" Người" v-model="training_emps.obj_training"  />
              <!-- <Dropdown
                v-model="training_emps.obj_training"
                :options="listObjTraining"
                optionLabel="name"
                optionValue="code"
                placeholder="----- Chọn đối tượng đào tạo -----"
                class="sel-placeholder w-full"
                panelClass="d-design-dropdown"
                :class="{
                  'p-invalid': training_emps.obj_training == null && submitted,
                }"
              /> -->
            </div>
          </div>
          <div class="col-6 p-0 flex text-left align-items-center">
            <div class="w-10rem pl-3">Chi phí dự kiến</div>
            <div style="width: calc(100% - 10rem)">
               <InputNumber
                  v-model="training_emps.expense"
                  class="w-full"
                   suffix=" VND"
                />
            </div>
          </div>
        </div>
        <div
          class="col-12 p-0 field flex"
          v-if="training_emps.obj_training == null && submitted"
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
            <div class="w-10rem">
              Ngày bắt đầu
            </div>
            <div style="width: calc(100% - 10rem)">
              <Calendar
                class="w-full"
                id="basic_purchase_date"
                v-model="training_emps.start_date"
                autocomplete="on"
                :showIcon="true"
              />
            </div>
          </div>
          <div class="col-6 p-0 flex text-left align-items-center">
            <div class="w-10rem pl-3">Ngày kết thúc</div>
            <div style="width: calc(100% - 10rem)">
              <Calendar
                class="w-full"
                id="basic_purchase_date"
                v-model="training_emps.end_date"
                autocomplete="on"
                :minDate="
                  training_emps.start_date
                    ? new Date(training_emps.start_date)
                    : null
                "
                :showIcon="true"
              />
            </div>
          </div>
        </div>
        <div
          class="col-12 p-0 field flex"
          v-if="training_emps.start_date == null && submitted"
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
    <div class="col-12 field p-0 text-lg font-bold">Thông tin vị trí tuyển</div>
   
        <div class="col-12 field p-0 flex text-left align-items-center">
           <div class="col-6 p-0 flex text-left align-items-center">
            <div class="w-10rem ">Vị trí<span class="redsao pl-1"> (*)</span></div>
            <div style="width: calc(100% - 10rem)">
              <Dropdown
                v-model="training_emps.position"
                :options="listStatus"
                optionLabel="name"
                optionValue="code"
                class="sel-placeholder w-full"
                panelClass="d-design-dropdown"
              />
            </div>
          </div>
          <div class="col-6 p-0 flex text-left align-items-center">
            <div class="w-10rem pl-3">Chức vụ</div>
            <div style="width: calc(100% - 10rem)">
               <Dropdown
                v-model="training_emps.position"
                :options="listStatus"
                optionLabel="name"
                optionValue="code"
                class="sel-placeholder w-full"
                panelClass="d-design-dropdown"
              />
            </div>
          </div>
        </div>

       
           <div class="col-12 flex p-0">
              <div class="col-6 p-0">
            <div class="col-12 field flex p-0 text-left align-items-center">
              <div class="w-10rem">
             Hình thức làm việc
              </div>
              <div style="width: calc(100% - 10rem)">
                    <Dropdown
                v-model="training_emps.position"
                :options="listStatus"
                optionLabel="name"
                optionValue="code"
                class="sel-placeholder w-full"
                panelClass="d-design-dropdown"
              />
              </div>
            </div>
          </div>
          <div class="col-6 flex p-0 text-center align-items-center">
            <div class="col-12 field flex p-0 text-left align-items-center">
              <div class="w-10rem pl-3">
               Nơi làm việc
              </div>
              <div style="width: calc(100% - 10rem)">
                   <InputText
                  v-model="training_emps.expense"
                  class="w-full"
                   
                />
              </div>
            </div>
          </div>
        </div>
       
        <div class="col-12 flex p-0">
              <div class="col-6 p-0">
            <div class="col-12 field flex p-0 text-left align-items-center">
              <div class="w-10rem">
             Mức lương (từ)
              </div>
              <div style="width: calc(100% - 10rem)">
                <InputNumber
                  v-model="training_emps.expense"
                  class="w-full d-input-design-number"
                   suffix=" VND"
                   placeholder="Từ"
               
                />
              </div>
            </div>
          </div>
          <div class="col-6 flex p-0 text-center align-items-center">
            <div class="col-12 field flex p-0 text-left align-items-center">
              <div class="w-10rem pl-3">
               Mức lương (đến)
              </div>
              <div style="width: calc(100% - 10rem)">
                    <InputNumber
                  v-model="training_emps.expense"
                  class="w-full d-input-design-number"
                   suffix=" VND"
                   placeholder="Đến"
               
                />
              </div>
            </div>
          </div>
        </div>
      
        
        <div class="col-12 p-0 border-1 border-300 border-solid">
          <div class="w-full surface-100 flex border-bottom-1 border-200 p-3">
            <div
              class="font-bold flex align-items-center w-full cursor-pointer"
              @click="showHidePanel(3)"
            >
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
              <div class="pl-2">File đính kèm</div>
            </div>
          </div>
          <div class="w-full" v-if="checkShow3 == true">
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
