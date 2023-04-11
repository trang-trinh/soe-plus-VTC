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
  candidate_source: {
    required,
    $errors: [
      {
        $property: "candidate_source",
        $validator: "required",
        $message: "Tên ứng viên không được để trống!",
      },
    ],
  },
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
const list_academic_level = ref([]);
const list_work_experience = ref([]);
const loadData = () => {
  if (props.checkadd == true) {
    list_users_family.value = [];
    list_academic_level.value = [];
    list_work_experience.value = [];
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
        let data4 = JSON.parse(response.data.data)[4];
        if (data) {
          candidate.value = data[0];

          if (candidate.value.candidate_place) {
            onFilterPlace({ value: candidate.value.candidate_place }, 1);
          }
          if (candidate.value.candidate_domicile) {
            onFilterPlace({ value: candidate.value.candidate_domicile }, 2);
          }
          if (candidate.value.resident_curent_address) {
            onFilterPlace(
              { value: candidate.value.resident_curent_address },
              4
            );
          }
          if (candidate.value.resident_address) {
            onFilterPlace({ value: candidate.value.resident_address }, 3);
          }
          if (candidate.value.candidate_birthday)
            candidate.value.candidate_birthday = new Date(
              candidate.value.candidate_birthday
            );
          if (candidate.value.candidate_identity_date)
            candidate.value.candidate_identity_date = new Date(
              candidate.value.candidate_identity_date
            );
          if (candidate.value.start_date)
            candidate.value.start_date = new Date(candidate.value.start_date);
          if (candidate.value.end_date)
            candidate.value.end_date = new Date(candidate.value.end_date);
          if (candidate.value.registration_deadline)
            candidate.value.registration_deadline = new Date(
              candidate.value.registration_deadline
            );
          candidate.value.candidate_phone_fake =
            candidate.value.candidate_phone.split(",");
          candidate.value.candidate_email_fake =
            candidate.value.candidate_email.split(",");
        }

        data1.forEach((element) => {
          if (element.birthday) element.birthday = new Date(element.birthday);
          list_users_family.value.push(element);
        });
        data2.forEach((element) => {
          if (element.start_date)
            element.start_date = new Date(element.start_date);
          if (element.end_date) element.end_date = new Date(element.end_date);
        });
        list_academic_level.value = data2;

        if (data3) {
          data3.forEach((element) => {
            if (element.start_date)
              element.start_date = new Date(element.start_date);
            if (element.end_date) element.end_date = new Date(element.end_date);
          });
          list_work_experience.value = data3;
        }

        if (data4) {
          listFilesS.value = data4;
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

  if (candidate.value.candidate_source == null) {
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
  if (candidate.value.candidate_phone_fake)
    candidate.value.candidate_phone =
      candidate.value.candidate_phone_fake.toString();
  if (candidate.value.candidate_email_fake)
    candidate.value.candidate_email =
      candidate.value.candidate_email_fake.toString();

  let formData = new FormData();
  for (var i = 0; i < filesList.value.length; i++) {
    let file = filesList.value[i];
    formData.append("image", file);
  }
  
  formData.append("candidate_avatar", files);
  
  formData.append("hrm_candidate", JSON.stringify(candidate.value));
  formData.append(
    "hrm_candidate_family",
    JSON.stringify(list_users_family.value)
  );
  formData.append(
    "hrm_candidate_academic",
    JSON.stringify(list_academic_level.value)
  );
  formData.append(
    "list_work_experience",
    JSON.stringify(list_work_experience.value)
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
        console.log(error);
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
const listUsers = ref([]); 
const v$ = useVuelidate(rules, candidate);

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
  } else if (type == 2) {
    let obj = {
      is_order: list_users_family.value.length + 1,
      relationship_id: null,

      full_name: null,
      birthday: null,
      major_id: null,
      phone_number: null,
      address: null,
      organization_id: store.getters.user.organization_id,
      status: true,
    };
    list_users_family.value.push(obj);

    checkShow2.value = true;
  } else if (type == 3) {
    let obj = {
      is_order: list_academic_level.value.length + 1,
      start_date: null,
      degree_id: null,
      end_date: null,
      learning_place_id: null,
      specialization_id: null,
      form_training: null,
      organization_id: store.getters.user.organization_id,
      status: true,
    };
    list_academic_level.value.push(obj);
    checkShow3.value = true;
  } else if (type == 4) {
    let obj = {
      is_order: list_work_experience.value.length + 1,
      start_date: null,
      end_date: null,
      company_name: null,
      position_id: null,
      reference_person: null,
      phone_number: null,
      work_des: null,
      organization_id: store.getters.user.organization_id,
      status: true,
    };
    list_work_experience.value.push(obj);

    checkShow4.value = true;
  } else if (type == 5) {
    checkShow5.value = true;
  }
};
const deleteFileH = (value) => {
  listFilesS.value = listFilesS.value.filter((x) => x.file_id != value.file_id);
};
const treedonvis = ref();
const listDiaDanh = ref([]);
const listDiaDanhSave = ref([]);

// const onChangeCandidatePlace = (value, type) => {
//   let idPlace = null;

//   for (const key in value) {
//     if (Object.hasOwnProperty.call(value, key)) {
//       idPlace = key;
//     }
//   }

//   var strPlace = "";
//   var placeSelected = listDiaDanhSave.value.find(
//     (x) => x.place_id == Number(idPlace)
//   );
//   strPlace += placeSelected.name;

//   if (placeSelected.parent_id) {
//     var renPP = (parent_id) => {
//       var dd = listDiaDanhSave.value.find(
//         (x) => x.place_id == Number(parent_id)
//       );

//       if (dd) {
//         strPlace += ", " + dd.name;
//         if (dd.parent_id) {
//           renPP(dd.parent_id);
//         }
//       }
//       return strPlace;
//     };
//     strPlace = renPP(placeSelected.parent_id);
//   }
//   if (type == 1) candidate.value.candidate_place = strPlace;
//   else if (type == 2) candidate.value.candidate_domicile = strPlace;
//   else if (type == 3) candidate.value.resident_address = strPlace;
//   else if (type == 4) candidate.value.resident_curent_address = strPlace;
// };
const renderTreePlace = (data, id, name, title) => {
  let arrChils = [];
  let arrtreeChils = [];
  data
    .filter((x) => x.parent_id == null)
    .forEach((m, i) => {
      m.IsOrder = i + 1;
      let om = { key: m[id], data: m };
      // const rechildren = (mm, pid) => {
      //   let dts = data.filter((x) => x.parent_id == pid);
      //   if (dts.length > 0) {
      //     if (!mm.children) mm.children = [];
      //     dts.forEach((em) => {
      //       let om1 = { key: em[id], data: em };
      //       rechildren(om1, em[id]);
      //       mm.children.push(om1);
      //     });
      //   }
      // };
      // rechildren(om, m[id]);
      // arrChils.push(om);
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
const listSources = ref([
  { name: "Nguồn 1", code: 1 },
  { name: "Nguồn 2", code: 2 },
  { name: "Nguồn 3", code: 3 },
  { name: "Nguồn 4", code: 4 },
  { name: "Khác", code: 5 },
]);
const listIdentityPlace = ref([]);
const listMarital = ref([
  { name: "Chưa kết hôn", code: 1 },
  { name: "Đang có vợ/chồng", code: 2 },
  { name: "Góa", code: 3 },
  { name: "Ly hôn/Ly thân", code: 4 },
  { name: "Khác", code: 5 },
]);
const listNationality = ref([]);
const listGenders = ref([
  { name: "Nam", code: 1 },
  { name: "Nữ", code: 2 },
  { name: "Khác", code: 3 },
]);
const listMilitary = ref([
  { name: "Chưa tham gia", code: 1 },
  { name: "Đã tham gia", code: 2 },
  { name: "Không đủ điều kiện tham gia", code: 3 },
  { name: "Không cần tham gia", code: 4 },
  { name: "Khác", code: 5 },
]);

const listRelationship = ref([]);
const listSpecialization = ref([]);
const listAcademicLevel = ref([]);
const listLearningPlace = ref([]);
const listFormTraining = ref([]);
const listPositions = ref([]);
const listCampaigns = ref([]);
const listPlaceDetails = ref([]);
const listPlaceDetails1 = ref([]);
const listPlaceDetails2 = ref([]);
const listPlaceDetails4 = ref([]);
const listPlaceDetails3 = ref([]);
const onFilterPlace = (event, type) => {
  var stc = event.value;
  if (event.value == "") {
    stc = null;
  }
  debugger
  axios
    .post(
      baseURL + "/api/DictionaryProc/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "ca_place_details_list",
            par: [
              { par: "search", va: stc },
              { par: "pageno", va: 0 },
              { par: "pagesize", va: 100 },
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
      if (type == 1) listPlaceDetails.value = [];
      if (type == 2) listPlaceDetails1.value = [];
      if (type == 3) listPlaceDetails2.value = [];
      if (type == 4) listPlaceDetails3.value = [];

      if (type == 5) listPlaceDetails4.value = [];
      data.forEach((element, i) => {
        if (type == 1)
          listPlaceDetails.value.push({
            name: element.name,
          });
        if (type == 2)
          listPlaceDetails1.value.push({
            name: element.name,
          });
        if (type == 3)
          listPlaceDetails2.value.push({
            name: element.name,
          });
        if (type == 4)
          listPlaceDetails3.value.push({
            name: element.name,
          });
        if (type == 5)
          listPlaceDetails4.value.push({
            name: element.name,
          });
      });
    })
    .catch((error) => {
      console.log(error);
      toast.error("Tải dữ liệu không thành công!");
    });
};
const initTudien = () => {
  axios
    .post(
      baseURL + "/api/DictionaryProc/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "ca_place_details_list",
            par: [
              { par: "search", va: null },
              { par: "pageno", va: 0 },
              { par: "pagesize", va: 100 },
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
      listPlaceDetails.value = [];
      listPlaceDetails1.value = [];
      listPlaceDetails2.value = [];
      listPlaceDetails3.value = [];
      listPlaceDetails4.value = [];
      data.forEach((element, i) => {
        listPlaceDetails.value.push({
          name: element.name,
        });
      });
      listPlaceDetails1.value = [...listPlaceDetails.value];
      listPlaceDetails2.value = [...listPlaceDetails.value];
      listPlaceDetails3.value = [...listPlaceDetails.value];
      listPlaceDetails4.value = [...listPlaceDetails.value];
    })
    .catch((error) => {
      console.log(error);
      toast.error("Tải dữ liệu không thành công!");
    });

  axios
    .post(
      baseURL + "/api/DictionaryProc/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_ca_identity_place_list",
            par: [
              { par: "pageno", va: 0 },
              { par: "pagesize", va: 10000 },
              { par: "user_id", va: store.state.user.user_id },
              { par: "status", va: null },
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
      listIdentityPlace.value = [];

      data.forEach((element, i) => {
        listIdentityPlace.value.push({
          name: element.identity_place_name,
          code: element.identity_place_id,
        });
      });
    })
    .catch((error) => {
      console.log(error);
      toast.error("Tải dữ liệu không thành công!");
    });

  listCampaigns.value = [];
  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_campaign_list",
            par: [
              { par: "pageno", va: 0 },
              { par: "pagesize", va: 10000 },
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
        listCampaigns.value.push({
          name: element.campaign_name,
          code: element.campaign_id,
        });
      });
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
    });

  axios
    .post(
      baseURL + "/api/DictionaryProc/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "doc_ca_positions_list",
            par: [
              { par: "pageno", va: 0 },
              { par: "pagesize", va: 10000 },
              { par: "user_id", va: store.state.user.user_id },
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
      listPositions.value = [];

      data.forEach((element, i) => {
        listPositions.value.push({
          name: element.position_name,
          code: element.position_id,
        });
      });
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
    });

  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_ca_form_traning_list",
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
      listFormTraining.value = [];

      data.forEach((element, i) => {
        listFormTraining.value.push({
          name: element.form_traning_name,
          code: element.form_traning_id,
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
            proc: "hrm_ca_learning_place_list",
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
      listLearningPlace.value = [];
      data.forEach((element, i) => {
        listLearningPlace.value.push({
          name: element.learning_place_name,
          code: element.learning_place_id,
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
      listAcademicLevel.value = [];
      data.forEach((element, i) => {
        listAcademicLevel.value.push({
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
            proc: "hrm_ca_relationship_list",
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
      listRelationship.value = [];
      data.forEach((element, i) => {
        listRelationship.value.push({
          name: element.relationship_name,
          code: element.relationship_id,
        });
      });
    })
    .catch((error) => {
      console.log(error);
    });
  axios
    .post(
      baseURL + "/api/DictionaryProc/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_d_ca_places_list",
            par: [{ par: "status", va: true }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data[0].length > 0) {
        listDiaDanhSave.value = [...data[0]];

        let obj = renderTreePlace(data[0], "place_id", "name", "Địa danh");

        listDiaDanh.value = obj.arrtreeChils;
      }
    })

    .catch((error) => {
      console.log(error);
      toast.error("Tải dữ liệu không thành công!");
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
            proc: "hrm_ca_nationality_list",
            par: [
              { par: "pageno", va: 0 },
              { par: "pagesize", va: 300 },
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
      listNationality.value = [];
      data.forEach((element, i) => {
        listNationality.value.push({
          name: element.nationality_name,
          code: element.nationality_id,
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
    list_academic_level.value.splice(
      list_academic_level.value.lastIndexOf(item),
      1
    );
  }
};
//Thêm bản ghi
const chooseImage = (id) => {
  document.getElementById(id).click();
};
let files = [];
const handleFileUpload = (event) => {
  files = event.target.files[0];
  var output = document.getElementById("logoTem");
  output.src = URL.createObjectURL(event.target.files[0]);
  output.onload = function () {
    URL.revokeObjectURL(output.src); // free memory
  };
};
const deleteImage = () => {
  files = [];
  candidate.value.candidate_avatar = null;
  files = "";
};
//Thêm bản ghi
const displayBasic=ref(false);

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
    v-model:visible="displayBasic"
    :style="{ width: '60vw' }"
    :maximizable="true"
    :modal="true"
    :closable="true"
    @hide="props.closeDialog"
  >
    <form>
      <div class="grid formgrid m-2">
      
        <div class="col-12 p-0 border-1 border-300 border-solid">
          <div
            class="w-full surface-100 flex border-bottom-1 border-200 cursor-pointer"
            @click="showHidePanel(1)"
          >
            <div class="font-bold flex align-items-center w-full p-3">
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
              <div class="pl-2">Thông tin chung</div>
            </div>
          </div>

          <div class="col-12 flex p-0 text-center align-items-center px-3 pt-3">
            <div class="col-3 field md:col-3 format-center">
              <div class="form-group">
                <div class="inputanh2 relative mb-2  ">
                  <img
                    @click=" chooseImage('imgAvatar')"
                    id="logoTem"
                    v-bind:src="
                      candidate.candidate_avatar
                        ? basedomainURL + candidate.candidate_avatar
                        : basedomainURL + '/Portals/Image/noimg.jpg'
                    "
                  />
                  <Button
                    v-if="candidate.candidate_avatar"
                    style="width: 2rem; height: 2rem"
                    icon="pi pi-times"
                    @click=" deleteImage( )"
                    class="p-button-rounded absolute top-0 right-0 cursor-pointer"
                  />
                  <input
                    id="imgAvatar"
                    type="file"
                    accept="image/*"
                    @change="handleFileUpload($event)"
                    style="display: none"
                  />
                </div>
              </div>
            </div>
            <div class="col-9 field p-0 text-left align-items-center">
              <div class="col-12 field flex p-0 align-items-center">
                <div class="w-10rem">Chiến dịch</div>
                <div style="width: calc(100% - 10rem)">
                  <Dropdown
                    v-model="candidate.campaign_id"
                    :options="listCampaigns"
                    optionLabel="name"
                    optionValue="code"
                    placeholder="Chọn chiến dịch ứng viên Apply"
                    class="w-full"
                  />
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
                     
                          :style="{ backgroundColor: '#FEF9E7', fontWeight: 'bold' }"
                          :class="{
                            'p-invalid':
                              v$.candidate_code.$invalid && submitted,
                          }"
                        />
                      </div>
                    </div>
                  </div>
                </div>
                <div class="col-6 flex p-0 align-items-center">
                  <div class="w-7rem pl-3">
                    Nguồn <span class="redsao pl-1"> (*)</span>
                  </div>
                  <div style="width: calc(100% - 7rem)">
                    <div class="col-12 p-0">
                      <div class="p-inputgroup">
                        <Dropdown
                          v-model="candidate.candidate_source"
                          :options="listSources"
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
                <div
                  class="col-6 p-0 flex"
                  v-if="v$.candidate_code.$invalid && submitted"
                >
                  <div class="w-7rem"></div>
                  <small style="width: calc(100% - 7rem)">
                    <span style="color: red" class="w-full">{{
                      v$.candidate_code.required.$message
                        .replace("Value", "Mã ứng viên")
                        .replace("is required", "không được để trống!")
                    }}</span>
                  </small>
                </div>
                <div class="col-6 p-0 flex" v-else></div>
                <div
                  class="col-6 p-0 flex"
                  v-if="candidate.candidate_source == null && submitted"
                >
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
                          placeholder="Nhập họ và tên ứng viên"
                          :style="
                            candidate.candidate_name
                              ? 'background-color:white !important'
                              : ''
                          "
                          :class="{
                            'p-invalid':
                              v$.candidate_name.$invalid && submitted,
                          }"
                        />
                      </div>
                    </div>
                  </div>
                </div>

                <div class="col-6 flex p-0 align-items-center">
                  <div class="col-7 flex p-0 align-items-center">
                    <div class="w-7rem pl-3">Ngày sinh</div>
                    <div style="width: calc(100% - 7rem)">
                      <Calendar
                        class="w-full"
                        v-model="candidate.candidate_birthday"
                        autocomplete="off"
                        placeholder="dd/mm/yyyy"
                        :showIcon="true"
                        :maxDate="new Date()"
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
                            :options="listGenders"
                            optionLabel="name"
                            optionValue="code"
                            class="w-full"
                          />
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
              <div
                class="col-12 p-0 field flex"
                v-if="v$.candidate_name.$invalid && submitted"
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
            </div>
          </div>

          <div class="col-12 field p-0 flex text-left align-items-center px-3">
            <div class="w-10rem">Nơi sinh</div>
            <div style="width: calc(100% - 10rem)">
              <Dropdown
                v-model="candidate.candidate_place"
                :options="listPlaceDetails"
                optionLabel="name"
                optionValue="name"
                class="w-full"
                placeholder="Xã phường, Quận huyện, Tỉnh thành"
                panelClass="d-design-dropdown"
                :filter="true"
                :editable="true"
                @filter="onFilterPlace($event, 1)"
              />
              <!-- <TreeSelect
              :options="listDiaDanh"
              v-model="candidate.candidate_place_fake"
              placeholder="Xã phường, Quận huyện, Tỉnh thành"
              optionLabel="name"
              optionValue="place_id"
              style="min-height: 36px"
              class="w-full"
              @change="onChangeCandidatePlace($event, 1)"
              :editable="true"
            >
              <template #value="slotProps">
                <div
                  class="p-ulchip"
                  v-if="slotProps.value && slotProps.value.length > 0"
                >
                  {{ candidate.candidate_place }}
                </div>
                <span v-else>
                  {{ slotProps.placeholder }}
                </span>
              </template>
            </TreeSelect> -->
            </div>
          </div>
          <div class="col-12 field p-0 flex text-left align-items-center px-3">
            <div class="w-10rem">Nguyên quán</div>
            <div style="width: calc(100% - 10rem)">
              <Dropdown
                v-model="candidate.candidate_domicile"
                :options="listPlaceDetails1"
                optionLabel="name"
                optionValue="name"   :editable="true"
                class="w-full"
                placeholder="Xã phường, Quận huyện, Tỉnh thành"
                panelClass="d-design-dropdown"
                :filter="true"
                @filter="onFilterPlace($event, 2)"
              />
              <!-- <TreeSelect
              :options="listDiaDanh"
              v-model="candidate.candidate_domicile_fake"
              placeholder="Xã phường, Quận huyện, Tỉnh thành"
              optionLabel="name"
              optionValue="place_id"
              style="min-height: 36px"
              class="w-full"
              @change="onChangeCandidatePlace($event, 2)"
              :editable="true"
            >
              <template #value="slotProps">
                <div
                  class="p-ulchip"
                  v-if="slotProps.value && slotProps.value.length > 0"
                >
                  {{ candidate.candidate_domicile }}
                </div>
                <span v-else>
                  {{ slotProps.placeholder }}
                </span>
              </template>
            </TreeSelect> -->
            </div>
          </div>

          <div class="col-12 field p-0 flex text-left align-items-center px-3">
            <div class="col-6 p-0 flex text-left align-items-center">
              <div class="w-10rem">CMT/Căn cước</div>
              <div style="width: calc(100% - 10rem)">
                <InputMask
                  spellcheck="false"
                  class="w-full h-full d-design-it"
                  v-model="candidate.candidate_identity"
                  mask="9999-9999-9999"
                  placeholder="0202-XXXX-XXXX"
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
                  autocomplete="off"
                  placeholder="dd/mm/yyyy"
                  :showIcon="true"
                />
              </div>
            </div>
          </div>

          <div class="col-12 field p-0 flex text-left align-items-center px-3">
            <div class="w-10rem">Nơi cấp</div>
            <div style="width: calc(100% - 10rem)">
              <Dropdown
                v-model="candidate.candidate_identity_place"
                :options="listIdentityPlace"
                optionLabel="name"
                optionValue="code"
                class="w-full"
                placeholder="Chọn nơi cấp"
                panelClass="d-design-dropdown"
                :filter="true"
              />
            </div>
          </div>
          <div class="col-12 field p-0 flex text-left align-items-center px-3">
            <div class="col-6 p-0 flex text-left align-items-center">
              <div class="w-10rem">Tình trạng hôn nhân</div>
              <div style="width: calc(100% - 10rem)">
                <Dropdown
                  v-model="candidate.candidate_marital"
                  :options="listMarital"
                  optionLabel="name"
                  optionValue="code"
                  class="w-full"
                  panelClass="d-design-dropdown"
                  placeholder="Chọn tình trạng hôn nhân"
                />
              </div>
            </div>
            <div class="col-6 p-0 flex text-left align-items-center">
              <div class="w-10rem pl-3">Quốc tịch</div>
              <div style="width: calc(100% - 10rem)">
                <Dropdown
                  v-model="candidate.candidate_nationality"
                  :options="listNationality"
                  optionLabel="name"
                  optionValue="code"
                  placeholder="Chọn quốc tịch"
                  class="w-full"
                  panelClass="d-design-dropdown"
                />
              </div>
            </div>
          </div>
          <div class="col-12 field p-0 flex text-left align-items-center px-3">
            <div class="col-6 p-0 flex text-left align-items-center">
              <div class="col-6 p-0 flex text-left align-items-center">
                <div class="w-10rem">Chiều cao</div>
                <div style="width: calc(100% - 10rem)">
                  <InputNumber
                    v-model="candidate.candidate_height"
                    class="w-full"
                    suffix=" Cm"
                  />
                </div>
              </div>
              <div class="col-6 p-0 flex text-left align-items-center">
                <div class="w-10rem format-center">Cân nặng</div>
                <div style="width: calc(100% - 10rem)">
                  <InputNumber
                    v-model="candidate.candidate_weight"
                    class="w-full"
                    suffix=" Kg"
                  />
                </div>
              </div>
            </div>
            <div class="col-6 p-0 flex text-left align-items-center">
              <div class="w-10rem pl-3">Nghĩa vụ quân sự</div>
              <div style="width: calc(100% - 10rem)">
                <Dropdown
                  v-model="candidate.candidate_military"
                  :options="listMilitary"
                  optionLabel="name"
                  optionValue="code"
                  class="w-full"
                  panelClass="d-design-dropdown"
                  placeholder="Chọn nghĩa vụ quân sự"
                />
              </div>
            </div>
          </div>
          <div class="col-12 field p-0 flex text-left align-items-center px-3">
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
        </div>
        <div class="col-12 p-0 border-1 border-300 border-solid">
          <div
            class="w-full surface-100 flex border-bottom-1 border-200 cursor-pointer"
            @click="showHidePanel(1)"
          >
            <div class="font-bold flex align-items-center w-full p-3">
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
                <Chips
                  v-model="candidate.candidate_phone_fake"
                  class="w-full d-design-chips"
                  :placeholder="
                    candidate.candidate_phone_fake
                      ? ''
                      : 'Nhập số điện thoại, Enter để nhập nhiều'
                  "
                />
              </div>
            </div>
            <div class="col-12 field p-0 flex text-left align-items-center">
              <div class="w-10rem">Email</div>
              <div style="width: calc(100% - 10rem)">
                <Chips
                  v-model="candidate.candidate_email_fake"
                  class="w-full d-design-chips"
                  :placeholder="
                    candidate.candidate_email_fake
                      ? ''
                      : 'Nhập email, Enter để nhập nhiều'
                  "
                />
              </div>
              <!-- placeholder="Nhập số điện thoại, Enter để nhập nhiều" -->
            </div>
            <div class="col-12 field p-0 flex text-left align-items-center">
              <div class="col-6 p-0 flex text-left align-items-center">
                <div class="w-10rem">Thường trú</div>
                <div style="width: calc(100% - 10rem)">
                  <InputText v-model="candidate.resident" class="w-full" />
                </div>
              </div>
              <div class="col-6 p-0 flex text-left align-items-center">
                <div class="w-10rem pl-3">Địa chỉ</div>
                <div style="width: calc(100% - 10rem)">
                  <Dropdown
                    v-model="candidate.resident_address"
                    :options="listPlaceDetails2"
                    optionLabel="name"
                    optionValue="name"
                    class="w-full"
                    placeholder="Xã phường, Quận huyện, Tỉnh thành"
                    panelClass="d-design-dropdown"
                    :filter="true"   :editable="true"
                    @filter="onFilterPlace($event, 3)"
                  />
                  <!-- <TreeSelect
                    :options="listDiaDanh"
                    v-model="candidate.resident_address_fake"
                    placeholder="Xã phường, Quận huyện, Tỉnh thành"
                    optionLabel="name"
                    optionValue="place_id"
                    style="min-height: 36px"
                    class="w-full"
                    @change="onChangeCandidatePlace($event, 3)"
                    :editable="true"
                  >
                    <template #value="slotProps">
                      <div
                        class="p-ulchip"
                        v-if="slotProps.value && slotProps.value.length > 0"
                      >
                        {{ candidate.resident_address }}
                      </div>
                      <span v-else>
                        {{ slotProps.placeholder }}
                      </span>
                    </template>
                  </TreeSelect> -->
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
                    :options="listPlaceDetails3"
                    optionLabel="name"
                    optionValue="name"
                    class="w-full"
                    placeholder="Xã phường, Quận huyện, Tỉnh thành"
                    panelClass="d-design-dropdown"
                    :filter="true"   :editable="true"
                    @filter="onFilterPlace($event, 4)"
                  />
                  <!-- <TreeSelect
                    :options="listDiaDanh"
                    v-model="candidate.resident_curent_address_fake"
                    placeholder="Xã phường, Quận huyện, Tỉnh thành"
                    optionLabel="name"
                    optionValue="place_id"
                    style="min-height: 36px"
                    class="w-full"
                    @change="onChangeCandidatePlace($event, 4)"
                    :editable="true"
                  >
                    <template #value="slotProps">
                      <div
                        class="p-ulchip"
                        v-if="slotProps.value && slotProps.value.length > 0"
                      >
                        {{ candidate.resident_curent_address }}
                      </div>
                      <span v-else>
                        {{ slotProps.placeholder }}
                      </span>
                    </template>
                  </TreeSelect> -->
                </div>
              </div>
              <!-- placeholder="Nhập số điện thoại, Enter để nhập nhiều" -->
            </div>
          </div>
        </div>

        <div class="col-12 p-0 border-1 border-300 border-solid">
          <div
            class="w-full surface-100 flex border-bottom-1 border-200 cursor-pointer"
          >
            <div
              class="font-bold flex align-items-center w-full p-3"
              @click="showHidePanel(2)"
            >
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
            <div
              class="w-1 text-right p-3 hover"
              v-if="!view"
              @click="addRow_Item(2)"
            >
              <a class="hover" v-tooltip.top="'Thêm người thân'">
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
                  bodyStyle="width:250px;"
                  class="align-items-center justify-content-center"
                >
                  <template #body="slotProps">
                    <div class="w-full">
                      <Dropdown
                        v-model="slotProps.data.relationship_id"
                        :options="listRelationship"
                        optionLabel="name"
                        optionValue="code"
                        class="w-full"
                        panelClass="d-design-dropdown"
                        placeholder="Chọn mối quan hệ"
                        :filter="true"
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
                      <InputText
                        v-model="slotProps.data.full_name"
                        class="w-full"
                        placeholder="Nhập họ và tên"
                      />
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
                      autocomplete="off"
                      placeholder="dd/mm/yyyy"
                      :showIcon="true"
                    />
                  </template>
                </Column>
                <Column
                  field="end_date"
                  header="Nghề nghiệp"
                  headerStyle="text-align:center;width:250px;height:50px"
                  bodyStyle="width:250px;"
                  class="align-items-center justify-content-center"
                >
                  <template #body="slotProps">
                    <Dropdown
                      v-model="slotProps.data.major_id"
                      :options="listSpecialization"
                      optionLabel="name"
                      optionValue="code"
                      class="w-full"
                      panelClass="d-design-dropdown"
                      placeholder="Chọn nghề nghiệp"
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
                    <InputMask
                      spellcheck="false"
                      class="w-full h-full d-design-it"
                      v-model="slotProps.data.phone_number"
                      mask="9999-999-999"
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
                   
                    <Dropdown
                      v-model="slotProps.data.address"
                      :options="listPlaceDetails4"
                      optionLabel="name"
                      optionValue="name"
                      class="w-full"
                      placeholder="Nhập địa chỉ"
                      panelClass="d-design-dropdown"
                      :filter="true"
                      :editable="true"
                      @filter="onFilterPlace($event, 5)"
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
                      v-tooltip.top="'Xóa người thân'"
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
        <div class="col-12 p-0 border-1 border-300 border-solid cursor-pointer">
          <div class="w-full surface-100 flex border-bottom-1 border-200">
            <div
              class="font-bold flex align-items-center w-full p-3"
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
              <div class="pl-2">
                Trình độ học vấn
                <span v-if="list_academic_level.length > 0">
                  ( {{ list_academic_level.length }} )</span
                >
              </div>
            </div>
            <div
              class="w-1 text-right p-3 hover"
              v-if="!view"
              @click="addRow_Item(3)"
            >
              <a class="hover" v-tooltip.top="'Thêm trình độ học vấn'">
                <i class="pi pi-plus-circle" style="font-size: 18px"></i>
              </a>
            </div>
          </div>

          <div class="w-full px-0 pt-0" v-if="checkShow3 == true">
            <div
              style="overflow-x: scroll"
              v-if="list_academic_level.length > 0"
            >
              <DataTable
                :value="list_academic_level"
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
                  headerStyle="text-align:center;width:150px;height:50px"
                  bodyStyle="text-align:center;width:150px;"
                  class="align-items-center justify-content-center text-center"
                >
                  <template #body="slotProps">
                    <div class="w-full">
                      <Calendar
                        class="w-full"
                        id="basic_purchase_date"
                        v-model="slotProps.data.start_date"
                        autocomplete="off"
                        :showIcon="true"
                        placeholder="mm/yyyy"
                        view="month"
                        dateFormat="mm/yy"
                        :maxDate="
                          candidate.end_date
                            ? new Date(candidate.end_date)
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
                      v-model="slotProps.data.end_date"
                      autocomplete="off"
                      :showIcon="true"
                      view="month"
                      dateFormat="mm/yy"
                      placeholder="mm/yyyy"
                      :minDate="
                        candidate.start_date
                          ? new Date(candidate.start_date)
                          : null
                      "
                    />
                  </template>
                </Column>

                <Column
                  header="Bằng cấp, trình độ"
                  headerStyle="text-align:center;width:200px;height:50px"
                  bodyStyle="width:200px;"
                  class="align-items-center justify-content-center"
                >
                  <template #body="slotProps">
                    <Dropdown
                      v-model="slotProps.data.academic_level_id"
                      placeholder="Chọn trình độ"
                      :options="listAcademicLevel"
                      optionLabel="name"
                      class="w-full"
                      panelClass="d-design-dropdown"
                      optionValue="code"
                    />
                  </template>
                </Column>
                <Column
                  field="transfer_place"
                  header="Nơi đào tạo"
                  headerStyle="text-align:center;width:250px ;height:50px"
                  bodyStyle="text-align:center ;width:250px;"
                  class="align-items-center justify-content-center text-center"
                >
                  <template #body="slotProps">
                    <Dropdown
                      v-model="slotProps.data.learning_place_id"
                      :options="listLearningPlace"
                      optionLabel="name"
                      optionValue="code"
                      class="w-full"
                      panelClass="d-design-dropdown"
                      :editable="true"
                      placeholder="Chọn nơi đào tạo"
                    />
                  </template>
                </Column>
                <Column
                  field="transfer_place"
                  header="Chuyên ngành"
                  headerStyle="text-align:center;width:250px;height:50px"
                  bodyStyle="width:250px;"
                  class="align-items-center justify-content-center"
                >
                  <template #body="slotProps">
                    <Dropdown
                      v-model="slotProps.data.specialization_id"
                      :options="listSpecialization"
                      optionLabel="name"
                      optionValue="code"
                      class="w-full"
                      panelClass="d-design-dropdown"
                      placeholder="Chọn chuyên nghành"
                    />
                  </template>
                </Column>

                <Column
                  field="transfer_place"
                  header="Hình thức đào tạo"
                  headerStyle="text-align:center;width:250px ;height:50px"
                  bodyStyle="  width:250px;"
                  class="align-items-center justify-content-center"
                >
                  <template #body="slotProps">
                    <Dropdown
                      v-model="slotProps.data.form_training"
                      :options="listFormTraining"
                      optionLabel="name"
                      optionValue="code"
                      class="w-full"
                      placeholder="Chọn hình thức đào tạo"
                      panelClass="d-design-dropdown"
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
                      v-tooltip.top="'Xóa trình độ học vấn'"
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
        <div class="col-12 p-0 border-1 border-300 border-solid cursor-pointer">
          <div class="w-full surface-100 flex border-bottom-1 border-200">
            <div
              class="font-bold flex align-items-center w-full p-3"
              @click="showHidePanel(4)"
            >
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
                <span v-if="list_work_experience.length > 0">
                  ( {{ list_work_experience.length }} )</span
                >
              </div>
            </div>
            <div
              class="w-1 text-right p-3 hover"
              v-if="!view"
              @click="addRow_Item(4)"
            >
              <a class="hover" v-tooltip.top="'Thêm kinh nghiệm làm việc'">
                <i class="pi pi-plus-circle" style="font-size: 18px"></i>
              </a>
            </div>
          </div>

          <div class="w-full px-0 pt-0" v-if="checkShow4 == true">
            <div
              style="overflow-x: scroll"
              v-if="list_work_experience.length > 0"
            >
              <DataTable
                :value="list_work_experience"
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
                  headerStyle="text-align:center;width:150px;height:50px"
                  bodyStyle="text-align:center;width:150px;"
                  class="align-items-center justify-content-center text-center"
                >
                  <template #body="slotProps">
                    <div class="w-full">
                      <Calendar
                        class="w-full"
                        id="basic_purchase_date"
                        v-model="slotProps.data.start_date"
                        autocomplete="off"
                        :showIcon="true"
                        placeholder="mm/yyyy"
                        view="month"
                        dateFormat="mm/yy"
                        :maxDate="
                          candidate.end_date
                            ? new Date(candidate.end_date)
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
                      v-model="slotProps.data.end_date"
                      autocomplete="off"
                      :showIcon="true"
                      view="month"
                      dateFormat="mm/yy"
                      placeholder="mm/yyyy"
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
                  headerStyle="text-align:center;width:250px;height:50px"
                  bodyStyle="width:250px;"
                  class="align-items-center justify-content-center"
                >
                  <template #body="slotProps">
                    <InputText
                      spellcheck="false"
                      class="w-full"
                      v-model="slotProps.data.company_name"
                      placeholder="Nhập tên công ty"
                    />
                  </template>
                </Column>
                <Column
                  field="admission_place"
                  header="Vị trí"
                  headerStyle="text-align:center;width:200px;height:50px"
                  bodyStyle="width:200px;"
                  class="align-items-center justify-content-center"
                >
                  <template #body="slotProps">
                    <Dropdown
                      v-model="slotProps.data.position_id"
                      :options="listPositions"
                      optionLabel="name"
                      class="w-full"
                      optionValue="code"
                      panelClass="d-design-dropdown"
                      :filter="true"
                      placeholder="Chọn vị trí"
                    />
                  </template>
                </Column>
                <Column
                  header="Người tham chiếu"
                  headerStyle="text-align:center;width:200px ;height:50px"
                  bodyStyle="text-align:center ;width:200px;"
                  class="align-items-center justify-content-center text-center"
                >
                  <template #body="slotProps">
                    <InputText
                      spellcheck="false"
                      placeholder="Nhập họ và tên"
                      class="w-full"
                      v-model="slotProps.data.reference_person"
                    />
                  </template>
                </Column>
                <Column
                  field="transfer_place"
                  header="Điện thoại"
                  headerStyle="text-align:center;width:150px ;height:50px"
                  bodyStyle="text-align:center ;width:150px;"
                  class="align-items-center justify-content-center text-center"
                >
                  <template #body="slotProps">
                    <InputMask
                      spellcheck="false"
                      class="w-full h-full d-design-it"
                      v-model="slotProps.data.phone_number"
                      mask="9999-999-999"
                    />
                  </template>
                </Column>

                <Column
                  field="transfer_place"
                  header="Mô tả công việc"
                  headerStyle="text-align:center;width:300px ;height:50px"
                  bodyStyle="  width:300px;"
                  class="align-items-center justify-content-center"
                >
                  <template #body="slotProps">
                    <Textarea
                      :autoResize="true"
                      rows="1"
                      cols="30"
                      v-model="slotProps.data.work_des"
                      class="w-full"
                      placeholder="Nhập mô tả"
                      panelClass="d-design-dropdown"
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
                      v-tooltip.top="'Xóa kinh nghiệm'"
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
            class="w-full surface-100 flex border-bottom-1 border-200 cursor-pointer"
          >
            <div
              class="font-bold flex align-items-center w-full p-3"
              @click="showHidePanel(5)"
            >
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
          <div class="w-full p-3" v-if="checkShow5 == true">
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
                class="p-0 "
                style="width: 100%; border-radius: 10px"
              >
                <div class="w-full py-3 flex align-items-center">
                  <div class="flex w-full">
                    <div v-if="item.is_image" class="align-items-center flex ">
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
                      <div class="ml-2 " style="word-break: break-all;">
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
                          <div class="ml-2" style="word-break: break-all;">
                            {{ item.file_name }}
                          </div>
                        </div>
                      </a>
                    </div>
                  </div>
                  <div class="w-3rem align-items-center ">
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
.inputanh2 {
  border: 1px solid #ccc;
  width: 150px;
  cursor: pointer;
  padding: 5px;
}

.inputanh2 img {
  object-fit: cover;
  width: 100%;
  height: 100%;
}
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
