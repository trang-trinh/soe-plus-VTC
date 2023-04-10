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
  recCalendar: Object,

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
  rec_calendar_name: {
    required,
    $errors: [
      {
        $property: "rec_calendar_name",
        $validator: "required",
        $message: "Tên lịch phỏng vấn không được để trống!",
      },
    ],
  },
};
const listFilesS = ref([]);
const recCalendar = ref({});
const submitted = ref(false);
const list_users_recCalendar = ref([]);

const loadData = () => {
  if (props.checkadd == true) {
    list_users_recCalendar.value = [];

    recCalendar.value = props.recCalendar;
  } else {
    axios
      .post(
        baseURL + "/api/hrm_ca_SQL/getData",
        {
          str: encr(
            JSON.stringify({
              proc: "hrm_rec_calendar_get",
              par: [
                {
                  par: "rec_calendar_id",
                  va: props.recCalendar.rec_calendar_id,
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

        if (data) {
          recCalendar.value = data[0];
          if (recCalendar.value.rec_calendar_date)
            recCalendar.value.rec_calendar_date = new Date(
              recCalendar.value.rec_calendar_date
            );
          if (recCalendar.value.rec_time_start)
            recCalendar.value.rec_time_start = new Date(
              recCalendar.value.rec_time_start
            );
          if (recCalendar.value.rec_time_end)
            recCalendar.value.rec_time_end = new Date(
              recCalendar.value.rec_time_end
            );

          recCalendar.value.interviewers_fake =
            recCalendar.value.interviewers.split(",");
        }

        data1.forEach((element) => {
          if (element.time_recruitment)
            element.time_recruitment = new Date(element.time_recruitment);

          if (element.candidate_code) {
            element.candidate_code_fake = listCandidate.value.find(
              (x) => x.code == element.candidate_code
            );
             
            if (listCandidate.value.length > 0) {
              var arr = [...listCandidateSave];
              arr = arr.filter(
                  (x) => x.code != element.candidate_code_fake.code
                );
             
              listCandidate.value = arr;
            }
          }
          list_users_recCalendar.value.push(element);
        });
        if (data2) {
          listFilesS.value = data2;
        }
        checkShow.value = true;
        checkShow2.value = true;

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
    recCalendar.value.campaign_id == null ||
    recCalendar.value.rec_calendar_name == null ||
    recCalendar.value.rec_calendar_date == null ||
    recCalendar.value.interviewers_fake == null
  ) {
    return;
  }

  if (recCalendar.value.interviewers_fake) {
    recCalendar.value.interviewers =
      recCalendar.value.interviewers_fake.toString();
  }
  let formData = new FormData();
  for (var i = 0; i < filesList.value.length; i++) {
    let file = filesList.value[i];
    formData.append("image", file);
  }

  formData.append("hrm_rec_calendar", JSON.stringify(recCalendar.value));
  formData.append(
    "hrm_rec_candidate",
    JSON.stringify(list_users_recCalendar.value)
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
        baseURL + "/api/hrm_rec_calendar/add_hrm_rec_calendar",
        formData,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Thêm thông tin lịch phỏng vấn thành công!");

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
        baseURL + "/api/hrm_rec_calendar/update_hrm_rec_calendar",
        formData,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa thông tin lịch phỏng vấn thành công!");

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
          code: element.profile_id,
          avatar: element.avatar,
          department_name: element.department_name,
          department_id: element.department_id,
          work_position_name: element.work_position_name,
          position_name: element.position_name,
          profile_code: element.profile_code,
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

const v$ = useVuelidate(rules, recCalendar);

const checkShow = ref(false);
const checkShow2 = ref(false);

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
      is_order: list_users_recCalendar.value.length + 1,
      relationship_id: null,

      full_name: null,
      birthday: null,
      major_id: null,
      phone_number: null,
      address: null,
      organization_id: store.getters.user.organization_id,
      status: true,
    };
    list_users_recCalendar.value.push(obj);

    checkShow2.value = true;
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

const renderTreePlace = (data, id, name, title) => {
  let arrChils = [];
  let arrtreeChils = [];
  data
    .filter((x) => x.parent_id == null)
    .forEach((m, i) => {
      m.IsOrder = i + 1;
      let om = { key: m[id], data: m };

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

const listIdentityPlace = ref([]);

const listNationality = ref([]);

const listRelationship = ref([]);
const listSpecialization = ref([]);
const listAcademicLevel = ref([]);
const listLearningPlace = ref([]);
const listFormTraining = ref([]);
const listPositions = ref([]);
const listCampaigns = ref([]);

const listCandidate = ref([]);
const onSelectCandidate = (value, index) => {
  var obj = listCandidate.value.find((x) => x.code == value.code);
  list_users_recCalendar.value[index].full_name = obj.name;
  list_users_recCalendar.value[index].candidate_code = obj.code;
  listPhoneNumber.value = [];
  if (obj.phone_number)
    obj.phone_number.split(",").forEach((element) => {
      listPhoneNumber.value.push({
        name: element,
      });
    });

  if (listCandidate.value.length > 0) {
    var arr = [...listCandidateSave];

    list_users_recCalendar.value.forEach((element) => {
      arr = arr.filter((x) => x.code != element.candidate_code_fake.code);
    });
    listCandidate.value = arr;
  }
};
const onFilterCandidate = (event) => {
  var stc = event.value;
  if (event.value == "") {
    stc = null;
  }
  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_candidate_list",
            par: [
              { par: "pageno", va: 0 },
              { par: "pagesize", va: 100 },
              { par: "user_id", va: store.getters.user.user_id },
              { par: "search", va: stc },
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
      listCandidate.value = [];
      data.forEach((element) => {
        listCandidate.value.push({
          name: element.candidate_name,
          code: element.candidate_id,
          phone_number: element.candidate_phone,
        });
      });
    })
    .catch((error) => {
      console.log(error);
      toast.error("Tải dữ liệu không thành công!");
    });
};
const listPhoneNumber = ref([]);
const listClasroom = ref([]);
var listCandidateSave = [];
const initTudien = () => {

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
      data.forEach((element) => {
        listClasroom.value.push({
          name: element.classroom_name,
          code: element.classroom_id,
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
            proc: "hrm_candidate_list",
            par: [
              { par: "pageno", va: 0 },
              { par: "pagesize", va: 100 },
              { par: "user_id", va: store.getters.user.user_id },
              { par: "search", va: null },
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

      data.forEach((element, i) => {
        listCandidate.value.push({
          name: element.candidate_name,
          code: element.candidate_id,
          phone_number: element.candidate_phone,
        });
      });
      listCandidateSave = [...listCandidate.value];
    })
    .catch((error) => {
      console.log(error);
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;
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
 

const delRow_Item = (item, type) => {
  if (type == 1) {
    list_users_recCalendar.value.splice(
      list_users_recCalendar.value.lastIndexOf(item),
      1
    );
    if (list_users_recCalendar.value.length > 0) {
      var arr = [...listDataUsersSave.value];
      list_users_recCalendar.value.forEach((element) => {
        arr = arr.filter((x) => x.code.profile_id != element.profile_id);
      });
      listDataUsers.value = arr;
    }
  }
};
//Thêm bản ghi
const displayBasic = ref(false);
onMounted(() => {
  initTudien();
  loadData();
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
    :style="{ width: '40vw' }"
    :maximizable="true"
    :modal="true"
    :closable="true"
  >
    <form>
      <div class="grid formgrid m-2 mt-0 pt-0">
        <div class="col-12 field p-0 text-lg font-bold">Thông tin chung</div>

        <div class="col-12 field flex p-0 text-left align-items-center">
          <div class="w-11rem">
            Chiến dịch<span class="redsao pl-1"> (*)</span>
          </div>
          <div style="width: calc(100% - 11rem)">
            <Dropdown
              v-model="recCalendar.campaign_id"
              :options="listCampaigns"
              optionLabel="name"
              optionValue="code"
              placeholder="Chọn chiến dịch tuyển dụng"
              class="w-full"
              :class="{
                'p-invalid': recCalendar.campaign_id == null && submitted,
              }"
            />
          </div>
        </div>
        <div
          class="col-12 p-0 field flex"
          v-if="recCalendar.campaign_id == null && submitted"
        >
          <div class="w-11rem"></div>
          <small style="width: calc(100% - 11rem)">
            <span style="color: red" class="w-full"
              >Chiến dịch tuyển dụng không được để trống!</span
            >
          </small>
        </div>

        <div class="col-12 field flex p-0 align-items-center">
          <div class="w-11rem">
            Tên lịch phỏng vấn<span class="redsao pl-1"> (*)</span>
          </div>
          <div style="width: calc(100% - 11rem)">
            <div class="col-12 p-0">
              <div class="p-inputgroup">
                <InputText
                  v-model="recCalendar.rec_calendar_name"
                  class="w-full"
                  placeholder="Nhập tên lịch phỏng vấn"
                  :style="
                    recCalendar.rec_calendar_name
                      ? 'background-color:white !important'
                      : ''
                  "
                  :class="{
                    'p-invalid': v$.rec_calendar_name.$invalid && submitted,
                  }"
                />
              </div>
            </div>
          </div>
        </div>
        <div
          class="col-12 p-0 field flex"
          v-if="v$.rec_calendar_name.$invalid && submitted"
        >
          <div class="w-11rem"></div>
          <small style="width: calc(100% - 11rem)">
            <span style="color: red" class="w-full">{{
              v$.rec_calendar_name.required.$message
                .replace("Value", "Tên lịch phỏng vấn")
                .replace("is required", "không được để trống!")
            }}</span>
          </small>
        </div>
        <div class="col-12 field flex p-0 align-items-center">
          <div class="w-11rem">
            Ngày phỏng vấn<span class="redsao pl-1"> (*)</span>
          </div>
          <div style="width: calc(100% - 11rem)">
            <div class="col-12 p-0 flex">
              <div class="col-5 flex p-0 align-items-center">
                <div class="p-inputgroup">
                  <Calendar
                    class="w-full"
                    v-model="recCalendar.rec_calendar_date"
                    autocomplete="off"
                    placeholder="dd/mm/yyyy"
                    :showIcon="true"
                    :class="{
                      'p-invalid':
                        recCalendar.rec_calendar_date == null && submitted,
                    }"
                  />
                </div>
              </div>

              <div class="col-7 flex p-0 align-items-center">
                <div class="col-6 flex p-0 align-items-center">
                  <div class="w-3rem pl-3">Từ</div>
                  <div style="width: calc(100% - 3rem)">
                    <span class="p-input-icon-left">
                      <i class="pi pi-search" />
                      <Calendar
                        v-model="recCalendar.rec_time_start"
                        :showTime="true"
                        :timeOnly="true"
                        hourFormat="24"
                        placeholder="hh:mm"
                        icon="pi pi-clock"
                        :showIcon="true"
                      />
                    </span>
                  </div>
                </div>
                <div class="col-6 flex p-0 align-items-center">
                  <div class="w-3rem pl-2">Đến</div>
                  <div style="width: calc(100% - 3rem)">
                    <div class="col-12 p-0">
                      <div class="p-inputgroup">
                        <Calendar
                          v-model="recCalendar.rec_time_end"
                          :showTime="true"
                          :timeOnly="true"
                          :minDate="recCalendar.rec_time_start"
                          hourFormat="24"
                          placeholder="hh:mm"
                          icon="pi pi-clock"
                          :showIcon="true"
                        />
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div
          class="col-12 field p-0 flex text-left align-items-center"
          v-if="recCalendar.rec_calendar_date == null && submitted"
        >
          <div class="w-11rem"></div>
          <div style="width: calc(100% - 11rem)">
            <div class="col-12 p-0 flex">
              <small style="width: calc(100% - 11rem)">
                <span style="color: red" class="w-full"
                  >Ngày phỏng vấn không được để trống!</span
                >
              </small>
            </div>
          </div>
        </div>
        <div class="col-12 field p-0 flex text-left align-items-center">
          <div class="w-11rem">
            Người phỏng vấn<span class="redsao pl-1"> (*)</span>
          </div>
          <div style="width: calc(100% - 11rem)">
            <MultiSelect
              v-model="recCalendar.interviewers_fake"
              :options="listDataUsers"
              optionLabel="profile_user_name"
              optionValue="code"
              placeholder="Chọn người phỏng vấn "
              panelClass="d-design-dropdown"
              class="w-full p-0"
              :class="{
                'p-invalid': recCalendar.interviewers_fake == null && submitted,
              }"
              :filter="true"
              display="chip"
            >
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

        <div
          class="col-12 field p-0 flex text-left align-items-center"
          v-if="recCalendar.interviewers_fake == null && submitted"
        >
          <div class="w-11rem"></div>
          <div style="width: calc(100% - 11rem)">
            <div class="col-12 p-0 flex">
              <small style="width: calc(100% - 11rem)">
                <span style="color: red" class="w-full"
                  >Người phỏng vấn không được để trống!</span
                >
              </small>
            </div>
          </div>
        </div>
        <div class="col-12 p-0 flex field align-items-center">
          <div class="w-11rem">Phòng phỏng vấn</div>
          <div style="width: calc(100% - 11rem)">
            <!-- <InputText v-model="recCalendar.rec_calendar_room" class="w-full">
            </InputText> -->
            <Dropdown
              v-model="recCalendar.rec_calendar_room"
              :options="listClasroom"
              optionLabel="name"
              optionValue="name"
              class="w-full"
              placeholder="Chọn phòng phỏng vấn"
              panelClass="d-design-dropdown"
              :filter="true"
              
            />
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
                Danh sách ứng viên tham gia
                <span v-if="list_users_recCalendar.length > 0">
                  ( {{ list_users_recCalendar.length }} )</span
                >
              </div>
            </div>
            <div
              class="w-1 text-right p-3 hover"
              v-if="!view"
              @click="addRow_Item(2)"
            >
              <a class="hover" v-tooltip.top="'Thêm lịch phỏng vấn'">
                <i class="pi pi-plus-circle" style="font-size: 18px"></i>
              </a>
            </div>
          </div>

          <div class="w-full p-0" v-if="checkShow2 == true">
            <div v-if="list_users_recCalendar.length > 0">
              <DataTable
                :value="list_users_recCalendar"
                :scrollable="true"
                :lazy="true"
                :rowHover="true"
                :showGridlines="true"
                scrollDirection="both"
              >
                <Column
                  field="card_number"
                  header="STT"
                  headerStyle="text-align:center;width:60px;height:50px"
                  bodyStyle="text-align:center;width:60px;"
                  class="align-items-center justify-content-center text-center"
                >
                  <template #body="slotProps">
                    {{ slotProps.data.is_order }}
                  </template>
                </Column>

                <Column
                  field="form"
                  header="Họ và tên"
                  headerStyle="text-align:center;width:250px ;height:50px"
                  bodyStyle="  width:250px;"
                  class="align-items-center justify-content-center"
                >
                  <template #body="slotProps">
                    <div class="w-full">
                      <Dropdown
                        v-model="slotProps.data.candidate_code_fake"
                        :options="listCandidate"
                        optionLabel="name"
                        class="w-full"
                        placeholder="Chọn ứng viên"
                        :filter="true"
                        @filter="onFilterCandidate($event)"
                        @change="
                          onSelectCandidate(
                            slotProps.data.candidate_code_fake,
                            slotProps.data.is_order - 1
                          )
                        "
                      >
                        <template #value="slotPropsD">
                          <div class=" " v-if="slotPropsD.value">
                            <div class="flex w-full align-items-center pr-2">
                              <div class="px-2">
                                {{ slotProps.data.full_name }}
                              </div>
                            </div>
                          </div>
                          <span v-else> {{ slotPropsD.placeholder }} </span>
                        </template>
                      </Dropdown>
                    </div>
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
                    <Dropdown
                      v-model="slotProps.data.phone_number"
                      :options="listPhoneNumber"
                      optionLabel="name"
                      optionValue="name"
                      class="w-full"
                      placeholder="Chọn điện thoại"
                      panelClass="d-design-dropdown"
                      :editable="true"
                    />
                  </template>
                </Column>
                <Column
                  field="transfer_place"
                  header="Giờ phỏng vấn"
                  headerStyle="text-align:center;width:140px ;height:50px"
                  bodyStyle="text-align:center ;width:140px;"
                  class="align-items-center justify-content-center text-center"
                >
                  <template #body="slotProps">
                    <Calendar
                      v-model="slotProps.data.time_recruitment"
                      :showTime="true"
                      placeholder="hh:mm"
                      :timeOnly="true"
                      hourFormat="24"
                      icon="pi pi-clock"
                      :showIcon="true"
                      :minDate="recCalendar.rec_time_start"
                      :maxDate="recCalendar.rec_time_end"
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
                      v-tooltip.top="'Xóa ứng viên'"
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
                class="p-0"
                style="width: 100%; border-radius: 10px"
              >
                <div class="w-full py-3 flex align-items-center ">
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
