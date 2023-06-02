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
  headerDialog: String,
  displayDialog: Boolean,
  closeDialog: Function,
  isAdd: Boolean,
  profile: Object,
  approve: Boolean,
  dictionarys: Array,
  initData: Function,
});

const display = ref(props.displayDialog);

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
const listPlaceDetails4 = ref([]);

//Declare
const options = ref({
  loading: true,
  history_id: null,
});
const genders = ref([
  { value: 1, text: "Nam" },
  { value: 2, text: "Nữ" },
  { value: 3, text: "Khác" },
]);
const marital_status = ref([
  { value: 0, text: "Độc thân" },
  { value: 1, text: "Kết hôn" },
  { value: 2, text: "Ly hôn" },
]);
const model = ref({});
const datachilds = ref([]);
const files = ref([]);
const historys = ref([]);
const history = ref({});
const history_status = ref([
  { value: -1, title: "Bản gốc", bgc: "#f7dc6f", tc: "#495057" },
  { value: 0, title: "Dự thảo", bgc: "#dddddd", tc: "#495057" },
  { value: 1, title: "Chờ duyệt", bgc: "#d6eaf8", tc: "#495057" },
  { value: 2, title: "Đã duyệt", bgc: "#afe362", tc: "#495057" },
  { value: 3, title: "Trả lại", bgc: "#f5b7b1", tc: "#495057" },
]);

//function
const goHistory = (item) => {
  options.value.history_id = item.history_id;
  history.value = item;
  initData(true);
};
const sendProfile = (rf, is_approve) => {
  if (rf) {
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
  }
  options.value.loading = true;
  let formData = new FormData();
  formData.append("ids", JSON.stringify([history.value.history_id]));
  formData.append("content", "");
  formData.append("is_approve", is_approve);
  formData.append(
    "approve_date",
    moment(new Date()).format("YYYY-MM-DDTHH:mm:ss")
  );
  axios
    .put(baseURL + "/api/hrm_profile/send_profile_history", formData, config)
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
      switch (is_approve) {
        case 1:
          toast.success("Gửi thành công!");
          break;
        case 2:
          toast.success("Duyệt thành công!");
          break;
        case 3:
          toast.success("Trả lại thành công!");
          break;
        default:
          break;
      }

      initHistory();
      props.initData(false);
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
  if (submitted.value) submitted.value = true;
};

//
const submitted = ref(false);
const saveModel = (rf) => {
  submitted.value = true;
  if (
    !model.value.profile_code ||
    !model.value.profile_user_name ||
    !model.value.birthday
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
  options.value.loading = true;
  var obj = { ...model.value };
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
  if (obj["select_place_residence"] != null) {
    var checkname = listPlaceDetails4.value.findIndex(
      (x) => x["place_details_id"] === (obj["select_place_residence"] || "")
    );
    if (checkname === -1) {
      obj["place_residence_name"] = obj["select_place_residence"] || "";
      obj["place_residence_id"] = null;
    } else {
      obj["place_residence_id"] = obj["select_place_residence"];
    }
  }
  let formData = new FormData();
  formData.append("isAdd", props.isAdd);
  formData.append("model", JSON.stringify(obj));
  formData.append("relative", JSON.stringify(datachilds.value[1] || []));
  formData.append("skill", JSON.stringify(datachilds.value[2] || []));
  formData.append("clan_history", JSON.stringify(datachilds.value[3] || []));
  formData.append("experience", JSON.stringify(datachilds.value[4] || []));
  for (var i = 0; i < files.value.length; i++) {
    let file = files.value[i];
    if (file["key"] === "avatar") {
      formData.append("avatar", file);
    } else {
      formData.append("files", file);
    }
  }
  axios
    .put(baseURL + "/api/hrm_profile/update_profile_history", formData, config)
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
      toast.success(
        props.isAdd ? "Thêm mới thành công!" : "Cập nhật thành công!"
      );
      if (rf) {
        sendProfile(true);
      } else {
        swal.close();
        initHistory();
        props.initData(false);
      }
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
  if (submitted.value) submitted.value = true;
};
const clickChange = ref(false);
const chooseImage = (id) => {
  clickChange.value = true;
  document.getElementById(id).click();
};
const handleFileAvtUpload = (event, id) => {
  if (event.target.files[0] != null) {
    files.value.push(event.target.files[0]);
  }
  if (files.value && files.value.length > 0) {
    files.value.forEach((f) => {
      f.key = id;
    });
  }
  model.value["isDisplayAvt"] = true;
  var output = document.getElementById(id);
  output.src = URL.createObjectURL(event.target.files[0]);
  output.onload = function () {
    URL.revokeObjectURL(output.src);
  };
};
const deleteImage = (id) => {
  if (id === "avatar") {
    if (files.value && files.value.length > 0) {
      files.value = files.value.filter((x) => x["key"] !== id);
    }
    model.value["isDisplayAvt"] = false;
    clickChange.value = false;
    var output = document.getElementById(id);
    output.src = basedomainURL + "/Portals/Image/noimg.jpg";
    model.value["avatar"] = null;
  }
};
const removeFile = (event) => {
  files.value = files.value.filter((x) => x["key"] === "avatar");
};
const selectFile = (event) => {
  event.files.forEach((element) => {
    files.value.push(element);
  });
};
const deleteRow = (type, idx) => {
  datachilds.value[type].splice(idx, 1);
};

//
const displayDialog1 = ref(false);
const displayDialog2 = ref(false);
const displayDialog3 = ref(false);
const displayDialog4 = ref(false);
const modeldetail = ref({});
const openAddRow = (type) => {
  modeldetail.value = {};
  if (type === 1) {
    displayDialog1.value = true;
  } else if (type === 2) {
    displayDialog2.value = true;
  } else if (type === 3) {
    displayDialog3.value = true;
  } else if (type === 4) {
    displayDialog4.value = true;
  }
};
const saveRow = (type, isContinue) => {
  if (datachilds.value[type] == null) {
    datachilds[type] = [];
  }
  datachilds.value[type].unshift(modeldetail.value);
  if (modeldetail.value.is_man_degree && modeldetail.value.academic_level_id) {
    var idx = props.dictionarys[6].findIndex(
      (x) => x.academic_level_id === modeldetail.value.academic_level_id
    );
    if (idx !== -1) {
      model.value.cultural_level_max =
        props.dictionarys[6][idx].academic_level_name;
    }
  }
  if (!isContinue) {
    closeDialog();
  }
};
const closeDialog = () => {
  displayDialog1.value = false;
  displayDialog2.value = false;
  displayDialog3.value = false;
  displayDialog4.value = false;
};

//init
const genCode = () => {
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_profile_gencode",
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
          if (tbs[0] != null && tbs[0].length > 0) {
            model.value.profile_code = tbs[0][0].profile_code;
            model.value.superior_id = tbs[0][0].superior_id;
          }
        }
      }
    });
};
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
            } else if (type == 4) {
              listPlaceDetails4.value = JSON.parse(JSON.stringify(data[0]));
            }
          } else {
            if (type == 1) {
              listPlaceDetails1.value = [];
            } else if (type == 2) {
              listPlaceDetails2.value = [];
            } else if (type == 3) {
              listPlaceDetails3.value = [];
            } else if (type == 4) {
              listPlaceDetails4.value = [];
            }
          }
        }
      }
    });
};
const initData = (rf) => {
  if (rf) {
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
  }
  datachilds.value = [];
  files.value = [];
  submitted.value = false;
  options.value.loading = true;
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_myprofile_by_history",
            par: [
              { par: "profile_id", va: props.profile.profile_id },
              { par: "history_id", va: options.value.history_id },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      var data = response.data.data;
      if (data != null) {
        var tbs = JSON.parse(data);
        if (tbs[0] != null && tbs[0].length > 0) {
          model.value = tbs[0][0];
          model.value["select_birthplace"] = model.value["birthplace_name"];
          model.value["select_birthplace_origin"] =
            model.value["birthplace_origin_name"];
          model.value["select_place_register_permanent"] =
            model.value["place_register_permanent_name"];
          model.value["select_place_residence"] =
            model.value["place_residence_name"];
          if (model.value["recruitment_date"] != null) {
            model.value["recruitment_date"] = new Date(
              model.value["recruitment_date"]
            );
          }
          if (model.value["birthday"] != null) {
            model.value["birthday"] = new Date(model.value["birthday"]);
          }
          if (model.value["identity_date_issue"] != null) {
            model.value["identity_date_issue"] = new Date(
              model.value["identity_date_issue"]
            );
          }
          if (model.value["identity_end_date_issue"] != null) {
            model.value["identity_end_date_issue"] = new Date(
              model.value["identity_end_date_issue"]
            );
          }
          if (model.value["partisan_date"] != null) {
            model.value["partisan_date"] = new Date(
              model.value["partisan_date"]
            );
          }
          if (model.value["partisan_joindate"] != null) {
            model.value["partisan_joindate"] = new Date(
              model.value["partisan_joindate"]
            );
          }
          if (model.value["organization_joindate"] != null) {
            model.value["organization_joindate"] = new Date(
              model.value["organization_joindate"]
            );
          }
          if (model.value.bevy_date != null) {
            model.value.bevy_date = new Date(model.value.bevy_date);
          }
          if (model.value.military_start_date != null) {
            model.value.military_start_date = new Date(
              model.value.military_start_date
            );
          }
          if (model.value.military_end_date != null) {
            model.value.military_end_date = new Date(
              model.value.military_end_date
            );
          }
          if (model.value["sign_date"] != null) {
            model.value["sign_date"] = new Date(model.value["sign_date"]);
          }
          if (model.value["partisan_main_date"] != null) {
            model.value["partisan_main_date"] = new Date(
              model.value["partisan_main_date"]
            );
          }
        }
        if (tbs[1] != null && tbs[1].length > 0) {
          tbs[1].forEach((x) => {
            if (x["identification_date_issue"] != null) {
              x["identification_date_issue"] = new Date(
                x["identification_date_issue"]
              );
            }
            if (x["start_date"] != null) {
              x["start_date"] = new Date(x["start_date"]);
            }
            if (x["end_date"] != null) {
              x["end_date"] = new Date(x["end_date"]);
            }
            if (x["birthday"] != null) {
              x["birthday"] = new Date(x["birthday"]);
            }
          });
          datachilds.value[1] = tbs[1];
        } else {
          datachilds.value[1] = [];
        }
        if (tbs[2] != null && tbs[2].length > 0) {
          tbs[2].forEach((x) => {
            if (x["certificate_start_date"] != null) {
              x["certificate_start_date"] = new Date(
                x["certificate_start_date"]
              );
            }
            if (x["certificate_end_date"] != null) {
              x["certificate_end_date"] = new Date(x["certificate_end_date"]);
            }
          });
          datachilds.value[2] = tbs[2];
        } else {
          datachilds.value[2] = [];
        }
        if (tbs[3] != null && tbs[3].length > 0) {
          tbs[3].forEach((x) => {
            if (x["start_date"] != null) {
              x["start_date"] = new Date(x["start_date"]);
            }
            if (x["end_date"] != null) {
              x["end_date"] = new Date(x["end_date"]);
            }
          });
          datachilds.value[3] = tbs[3];
        } else {
          datachilds.value[3] = [];
        }
        if (tbs[4] != null && tbs[4].length > 0) {
          datachilds.value[4] = tbs[4];
        } else {
          datachilds.value[4] = [];
        }
        if (tbs[5] != null && tbs[5].length > 0) {
          model.value["files"] = tbs[5];
        } else {
          model.value["files"] = [];
        }

        initPlaceFilter({ value: model.value.birthplace_name }, 1);
        initPlaceFilter({ value: model.value.birthplace_origin_name }, 2);
        initPlaceFilter(
          { value: model.value.place_register_permanent_name },
          3
        );
        initPlaceFilter({ value: model.value.place_residence_name }, 4);
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
const initHistory = (rf) => {
  if (rf) {
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
  }
  options.value.loading = true;
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_profile_history_list",
            par: [{ par: "profile_id", va: props.profile.profile_id }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      var data = response.data.data;
      if (data != null) {
        var tbs = JSON.parse(data);
        if (tbs[0] != null && tbs[0].length > 0) {
          tbs[0].forEach((item) => {
            if (item.history_id == null) {
              item.history_id = null;
            }
            if (item.created_date != null) {
              item.created_date_string = moment(
                new Date(item.created_date)
              ).format("DD/MM/YYYY");
            }
            if (item.approve_date != null) {
              item.approve_date_string = moment(
                new Date(item.approve_date)
              ).format("HH:mm DD/MM/YYYY");
            }
            if (item.is_approve != null) {
            }
            var idx = history_status.value.findIndex(
              (x) => x.value == item.is_approve
            );
            if (idx !== -1) {
              item.approve_name = history_status.value[idx].title;
              item.bgc = history_status.value[idx].bgc;
              item.tc = history_status.value[idx].tc;
            }
          });
          historys.value = tbs[0];
          options.value.history_id = historys.value[0].history_id || null;
          history.value = historys.value[0];
        } else {
          historys.value = [];
        }
        initData(true);
      }
      if (rf) {
        swal.close();
      }
      if (options.value.loading) options.value.loading = false;
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
};
onMounted(() => {
  if (props.displayDialog) {
    if (!props.isAdd) {
      initHistory();
    } else {
      genCode();
    }
  }
});
</script>
<template>
  <Sidebar
    v-model:visible="display"
    position="right"
    :modal="true"
    :dismissable="true"
    :showCloseIcon="false"
    :autoZIndex="true"
    class="position-relative profile-edit"
    :style="{ width: '82vw !important' }"
  >
    <template #header>
      <Toolbar class="outline-none surface-0 border-none w-full">
        <template #start>
          <h3 class="m-0">{{ props.headerDialog }}</h3>
        </template>
        <template #end>
          <Button
            @click="props.closeDialog()"
            icon="pi pi-times"
            class="p-button-rounded p-button-text"
          />
        </template>
      </Toolbar>
    </template>
    <form @submit.prevent="" name="submitform">
      <div class="grid formgrid m-2">
        <div class="p-0" :class="props.isAdd == true ? '' : 'col-3 md:col-3'">
          <div class="row">
            <div
              v-if="historys && historys.length > 0"
              class="col-12 md:col-12 p-0"
            >
              <Timeline
                :value="historys"
                align="alternate"
                class="customized-timeline"
              >
                <template #marker="slotProps">
                  <span
                    class="flex w-2rem h-2rem align-items-center justify-content-center text-white border-circle z-1 shadow-1"
                    :style="{
                      backgroundColor: '#0078d4',
                    }"
                  >
                    <i class="pi pi-clock"></i>
                  </span>
                </template>
                <template #content="slotProps">
                  <Card
                    @click="goHistory(slotProps.item)"
                    :style="{
                      backgroundColor: slotProps.item.bgc,
                      color: slotProps.item.tc,
                    }"
                    :class="{
                      'profile-history-active':
                        slotProps.item.history_id === options.history_id,
                    }"
                    class="profile-history mb-5"
                  >
                    <template #subtitle>
                      <div
                        class="w-full text-left flex justify-content-between"
                      >
                        <div :style="{ color: '#000' }">
                          <span v-if="slotProps.item.is_root">Bản gốc</span>
                          <span v-else
                            >Lần thay đổi: {{ slotProps.item.is_order }}</span
                          >
                        </div>
                        <div
                          v-tooltip.top="'Ngày tạo'"
                          :style="{ fontSize: '12px' }"
                        >
                          <span>{{ slotProps.item.created_date_string }}</span>
                        </div>
                      </div>
                    </template>
                    <template #content>
                      <div class="w-full text-left">
                        <div v-if="slotProps.item.is_approve > 1">
                          <div class="mb-2">
                            Người duyệt:
                            <Avatar
                              v-bind:label="
                                slotProps.item.avatar
                                  ? ''
                                  : (slotProps.item.profile_last_name ?? '')
                                      .substring(0, 1)
                                      .toUpperCase()
                              "
                              v-bind:image="
                                slotProps.item.avatar
                                  ? basedomainURL + slotProps.item.avatar
                                  : basedomainURL + '/Portals/Image/noimg.jpg'
                              "
                              v-tooltip.top="slotProps.item.profile_name"
                              :style="{
                                background: '#ffffff',
                                color: bgColor[slotProps.index % 7],
                                width: '2rem',
                                height: '2rem',
                                fontSize: '1rem !important',
                                borderRadius: '50%',
                              }"
                              size="xlarge"
                              class="border-radius"
                            />
                          </div>
                          <div class="mb-2">
                            Ngày duyệt:
                            <span>{{
                              slotProps.item.approve_date_string
                            }}</span>
                          </div>
                          <div class="mb-2">
                            Nội dung:
                            <span>{{ slotProps.item.approve_content }}</span>
                          </div>
                        </div>
                        <div>
                          Trạng thái:
                          <span>{{ slotProps.item.approve_name }}</span>
                        </div>
                      </div>
                    </template>
                  </Card>
                </template>
              </Timeline>
            </div>
            <div v-else-if="!options.loading" class="col-12 md:col-12 p-0">
              <div class="description format-center">
                Hiện chưa có lịch sử ghi nhận
              </div>
            </div>
          </div>
        </div>
        <div
          class="col-9 md:col-9"
          :class="props.isAdd ? 'col-12 md:col-12' : 'col-9 md:col-9'"
        >
          <Accordion class="w-full" :activeIndex="0">
            <!-- 1. Thông tin chung -->
            <AccordionTab>
              <template #header>
                <span>1. Thông tin chung</span>
              </template>
              <div class="col-12 md:col-12 p-0">
                <div class="row">
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
                              @click="chooseImage('imgAvatar')"
                              id="avatar"
                              v-bind:src="
                                model.avatar
                                  ? basedomainURL + model.avatar
                                  : basedomainURL + '/Portals/Image/noimg.jpg'
                              "
                            />
                            <Button
                              v-if="model.avatar || model.isDisplayAvt"
                              style="width: 2rem; height: 2rem"
                              icon="pi pi-times"
                              @click="deleteImage('avatar')"
                              class="p-button-rounded absolute top-0 right-0 cursor-pointer"
                            />
                            <input
                              id="imgAvatar"
                              type="file"
                              accept="image/*"
                              @change="handleFileAvtUpload($event, 'avatar')"
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
                                >Mã nhân sự
                                <span class="redsao">(*)</span></label
                              >
                              <InputText
                                spellcheck="false"
                                class="ip36"
                                v-model="model.profile_code"
                                :style="{
                                  backgroundColor: '#FEF9E7',
                                  fontWeight: 'bold',
                                }"
                                :class="{
                                  'p-invalid': !model.profile_code && submitted,
                                }"
                                maxLength="250"
                                disabled
                              />
                              <div v-if="!model.profile_code && submitted">
                                <small class="p-error">
                                  <span class="col-12 p-0"
                                    >Mã nhân sự không được để trống</span
                                  >
                                </small>
                              </div>
                            </div>
                          </div>
                          <div class="col-6 md:col-6">
                            <div class="form-group">
                              <label>Ngày tuyển dụng</label>
                              <Calendar
                                class="ip36"
                                id="icon"
                                v-model="model.recruitment_date"
                                :showIcon="true"
                                placeholder="dd/mm/yyyy"
                                disabled
                              />
                            </div>
                          </div>
                          <div class="col-6 md:col-6">
                            <div class="form-group">
                              <label
                                >Họ và tên
                                <span class="redsao">(*)</span></label
                              >
                              <InputText
                                spellcheck="false"
                                class="ip36"
                                v-model="model.profile_user_name"
                                :style="{ fontWeight: 'bold' }"
                                :class="{
                                  'p-invalid': !model.profile_code && submitted,
                                }"
                                disabled
                              />
                              <div v-if="!model.profile_user_name && submitted">
                                <small class="p-error">
                                  <span>Họ và tên không được để trống</span>
                                </small>
                              </div>
                            </div>
                          </div>
                          <div class="col-6 md:col-6">
                            <div class="form-group">
                              <label>Mã quản lý cấp trên</label>
                              <InputText
                                spellcheck="false"
                                class="ip36"
                                v-model="model.superior_id"
                                maxLength="50"
                                disabled
                              />
                            </div>
                          </div>
                          <div class="col-6 md:col-6">
                            <div class="form-group">
                              <label
                                >Ngày sinh
                                <span class="redsao">(*)</span></label
                              >
                              <Calendar
                                class="ip36"
                                id="icon"
                                v-model="model.birthday"
                                :showIcon="true"
                                placeholder="dd/mm/yyyy"
                                :class="{
                                  'p-invalid': !model.profile_code && submitted,
                                }"
                              />
                              <div v-if="!model.birthday && submitted">
                                <small class="p-error">
                                  <span>Ngày sinh không được để trống</span>
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
                                v-model="model.check_in_id"
                                maxLength="50"
                                disabled
                              />
                            </div>
                          </div>
                          <div class="col-6 md:col-6">
                            <div class="form-group">
                              <label>Tên gọi khác</label>
                              <InputText
                                spellcheck="false"
                                class="ip36"
                                v-model="model.profile_nick_name"
                              />
                            </div>
                          </div>
                          <div class="col-6 md:col-6">
                            <div class="form-group">
                              <label>Giới tính</label>
                              <Dropdown
                                :options="genders"
                                v-model="model.gender"
                                optionLabel="text"
                                optionValue="value"
                                placeholder="Chọn giới tính"
                                class="ip36"
                                :style="{
                                  whiteSpace: 'nowrap',
                                  overflow: 'hidden',
                                  textOverflow: 'ellipsis',
                                }"
                              />
                            </div>
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>
                  <div class="col-6 md:col-6">
                    <div class="form-group">
                      <label>Nơi sinh</label>
                      <Dropdown
                        @filter="initPlaceFilter($event, 1)"
                        :options="listPlaceDetails1"
                        :filter="true"
                        :editable="true"
                        :showClear="true"
                        v-model="model.select_birthplace"
                        optionLabel="name"
                        optionValue="place_details_id"
                        class="ip36"
                        placeholder="Xã phường, Quận huyện, Tỉnh thành"
                        panelClass="d-design-dropdown"
                        :style="{
                          whiteSpace: 'nowrap',
                          overflow: 'hidden',
                          textOverflow: 'ellipsis',
                        }"
                      />
                    </div>
                  </div>
                  <div class="col-6 md:col-6">
                    <div class="form-group">
                      <label>Quê quán</label>
                      <Dropdown
                        @filter="initPlaceFilter($event, 2)"
                        :options="listPlaceDetails2"
                        :filter="true"
                        :editable="true"
                        :showClear="true"
                        v-model="model.select_birthplace_origin"
                        optionLabel="name"
                        optionValue="place_details_id"
                        class="ip36"
                        placeholder="Xã phường, Quận huyện, Tỉnh thành"
                        panelClass="d-design-dropdown"
                        :style="{
                          whiteSpace: 'nowrap',
                          overflow: 'hidden',
                          textOverflow: 'ellipsis',
                        }"
                      />
                    </div>
                  </div>
                  <div class="col-12 md:col-12">
                    <div class="form-group m-0">
                      <label>Nơi đăng ký HKTT</label>
                    </div>
                  </div>
                  <div class="col-12 md:col-12 p-0">
                    <div class="row">
                      <div class="col-6 md:col-6">
                        <div class="form-group">
                          <InputText
                            spellcheck="false"
                            class="ip36"
                            v-model="model.place_register_permanent_first"
                            maxLength="500"
                            placeholder="Số nhà/đường phố"
                          />
                        </div>
                      </div>
                      <div class="col-6 md:col-6">
                        <div class="form-group">
                          <Dropdown
                            @filter="initPlaceFilter($event, 3)"
                            :options="listPlaceDetails3"
                            :filter="true"
                            :editable="true"
                            :showClear="true"
                            v-model="model.select_place_register_permanent"
                            optionLabel="name"
                            optionValue="place_details_id"
                            class="ip36"
                            placeholder="Xã phường, Quận huyện, Tỉnh thành"
                            panelClass="d-design-dropdown"
                            :style="{
                              whiteSpace: 'nowrap',
                              overflow: 'hidden',
                              textOverflow: 'ellipsis',
                            }"
                          />
                        </div>
                      </div>
                    </div>
                  </div>
                  <div class="col-12 md:col-12 p-0">
                    <div class="row">
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
                            v-model="model.nationality_id"
                            :filter="true"
                            :style="{
                              whiteSpace: 'nowrap',
                              overflow: 'hidden',
                              textOverflow: 'ellipsis',
                            }"
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
                            v-model="model.ethnic_id"
                            :filter="true"
                            :style="{
                              whiteSpace: 'nowrap',
                              overflow: 'hidden',
                              textOverflow: 'ellipsis',
                            }"
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
                            v-model="model.religion_id"
                            :filter="true"
                            :style="{
                              whiteSpace: 'nowrap',
                              overflow: 'hidden',
                              textOverflow: 'ellipsis',
                            }"
                          />
                        </div>
                      </div>
                      <div class="col-4 md:col-4">
                        <div class="form-group">
                          <label>Loại giấy tờ</label>
                          <Dropdown
                            :showClear="true"
                            :options="props.dictionarys[0]"
                            optionLabel="identity_papers_name"
                            optionValue="identity_papers_id"
                            placeholder="Chọn loại"
                            class="ip36"
                            v-model="model.identity_papers_id"
                            :style="{
                              whiteSpace: 'nowrap',
                              overflow: 'hidden',
                              textOverflow: 'ellipsis',
                            }"
                          />
                        </div>
                      </div>
                      <div class="col-4 md:col-4">
                        <div class="form-group">
                          <label>Số</label>
                          <InputText
                            spellcheck="false"
                            class="ip36"
                            v-model="model.identity_papers_code"
                            maxLength="50"
                          />
                        </div>
                      </div>
                      <div class="col-2 md:col-2">
                        <div class="form-group">
                          <label>Ngày cấp</label>
                          <Calendar
                            class="ip36"
                            id="icon"
                            v-model="model.identity_date_issue"
                            :showIcon="true"
                            placeholder="dd/mm/yyyy"
                          />
                        </div>
                      </div>
                      <div class="col-2 md:col-2">
                        <div class="form-group">
                          <label>Ngày hết hạn</label>
                          <Calendar
                            class="ip36"
                            id="icon"
                            v-model="model.identity_end_date_issue"
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
                            v-model="model.identity_place_id"
                            placeholder="Chọn nơi cấp"
                            optionLabel="identity_place_name"
                            optionValue="identity_place_id"
                            class="ip36"
                            :style="{
                              whiteSpace: 'nowrap',
                              overflow: 'hidden',
                              textOverflow: 'ellipsis',
                            }"
                          />
                        </div>
                      </div>
                      <div class="col-4 md:col-4">
                        <div class="form-group">
                          <label>Mã số thuế</label>
                          <InputText
                            spellcheck="false"
                            class="ip36"
                            v-model="model.tax_code"
                            maxLength="50"
                          />
                        </div>
                      </div>
                      <div class="col-4 md:col-4">
                        <div class="form-group">
                          <label>Tình trạng hôn nhân</label>
                          <Dropdown
                            :showClear="true"
                            :options="marital_status"
                            optionLabel="text"
                            optionValue="value"
                            placeholder="Chọn trạng thái"
                            class="ip36"
                            v-model="model.marital_status"
                            :style="{
                              whiteSpace: 'nowrap',
                              overflow: 'hidden',
                              textOverflow: 'ellipsis',
                            }"
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
                            v-model="model.bank_id"
                            :filter="true"
                            :style="{
                              whiteSpace: 'nowrap',
                              overflow: 'hidden',
                              textOverflow: 'ellipsis',
                            }"
                          />
                        </div>
                      </div>
                      <div class="col-4 md:col-4">
                        <div class="form-group">
                          <label>Số tài khoản</label>
                          <InputText
                            spellcheck="false"
                            class="ip36"
                            v-model="model.bank_number"
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
                            v-model="model.bank_account"
                            maxLength="250"
                          />
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </AccordionTab>
          </Accordion>
          <Accordion class="w-full" :multiple="true">
            <!-- 2. Thông tin liên hệ -->
            <AccordionTab>
              <template #header>
                <!-- <i class="pi pi-info-circle mr-2"></i> -->
                <span>2. Thông tin liên hệ</span>
              </template>
              <div class="col-12 md:col-12">
                <div class="row">
                  <div class="col-4 md:col-4">
                    <div class="form-group">
                      <label>Di động</label>
                      <InputMask
                        v-model="model.phone"
                        mask="9999999999"
                        placeholder="__________"
                        class="ip36"
                      />
                    </div>
                  </div>
                  <div class="col-4 md:col-4">
                    <div class="form-group">
                      <label>Số điện thoại cố định</label>
                      <InputMask
                        v-model="model.fax"
                        mask="9999999999"
                        placeholder="__________"
                        class="ip36"
                      />
                    </div>
                  </div>
                  <div class="col-4 md:col-4">
                    <div class="form-group">
                      <label>Email</label>
                      <InputText
                        spellcheck="false"
                        class="ip36"
                        v-model="model.email"
                        maxLength="50"
                      />
                    </div>
                  </div>
                  <div class="col-12 md:col-12">
                    <div class="form-group m-0">
                      <label>Chỗ ở hiện nay </label>
                    </div>
                  </div>
                  <div class="col-6 md:col-6">
                    <div class="form-group">
                      <InputText
                        spellcheck="false"
                        class="ip36"
                        v-model="model.place_permanent"
                        maxLength="500"
                        placeholder="Số nhà/đường phố"
                      />
                    </div>
                  </div>
                  <div class="col-6 md:col-6">
                    <div class="form-group">
                      <Dropdown
                        @filter="initPlaceFilter($event, 4)"
                        :options="listPlaceDetails4"
                        :filter="true"
                        :editable="true"
                        :showClear="true"
                        v-model="model.select_place_residence"
                        optionLabel="name"
                        optionValue="place_details_id"
                        class="ip36"
                        placeholder="Xã phường, Quận huyện, Tỉnh thành"
                        panelClass="d-design-dropdown"
                        :style="{
                          whiteSpace: 'nowrap',
                          overflow: 'hidden',
                          textOverflow: 'ellipsis',
                        }"
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
                        v-model="model.involved_name"
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
                        v-model="model.involved_phone"
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
                        v-model="model.relationship_id"
                        class="ip36"
                        :style="{
                          whiteSpace: 'nowrap',
                          overflow: 'hidden',
                          textOverflow: 'ellipsis',
                        }"
                      />
                    </div>
                  </div>
                  <div class="col-12 md:col-12">
                    <div class="form-group">
                      <label>Địa chỉ</label>
                      <InputText
                        spellcheck="false"
                        class="ip36"
                        v-model="model.involved_place"
                        maxLength="500"
                      />
                    </div>
                  </div>
                </div>
              </div>
            </AccordionTab>
            <!-- 3. Thông tin tuyển dụng -->
            <AccordionTab>
              <template #header>
                <!-- <i class="pi pi-info-circle mr-2"></i> -->
                <span>3. Thông tin tuyển dụng</span>
              </template>
              <div class="col-12 md:col-12">
                <div class="row">
                  <div class="col-6 md:col-6">
                    <div class="form-group">
                      <label>Ngày tuyển dụng lần đầu</label>
                      <Calendar
                        class="ip36"
                        id="icon"
                        v-model="model.recruitment_date_first"
                        :showIcon="true"
                        placeholder="dd/mm/yyyy"
                      />
                    </div>
                  </div>
                  <div class="col-6 md:col-6">
                    <div class="form-group">
                      <label>Hình thức tuyển dụng</label>
                      <InputText
                        spellcheck="false"
                        class="ip36"
                        v-model="model.recruitment_form"
                        maxLength="500"
                      />
                    </div>
                  </div>
                  <div class="col-6 md:col-6">
                    <div class="form-group">
                      <label
                        >Nghề nghiệp bản thân trước khi được tuyển dụng</label
                      >
                      <InputText
                        spellcheck="false"
                        class="ip36"
                        v-model="model.job_before_recruitment"
                        maxLength="500"
                      />
                    </div>
                  </div>
                  <div class="col-6 md:col-6">
                    <div class="form-group">
                      <label>Vị trí tuyển dụng</label>
                      <InputText
                        spellcheck="false"
                        class="ip36"
                        v-model="model.recruitment_position"
                        maxLength="500"
                      />
                    </div>
                  </div>
                  <div class="col-12 md:col-12">
                    <div class="form-group">
                      <label>Tuyển dụng lại:</label>
                    </div>
                  </div>
                  <div class="col-6 md:col-6 format-center">
                    <div class="form-group m-0">
                      <div
                        class="field-checkbox flex justify-content-center"
                        style="height: 100%"
                      >
                        <InputSwitch v-model="model.is_re_recruitment" />
                        <label for="binary">Tuyển dụng lại</label>
                      </div>
                    </div>
                  </div>
                  <div v-if="model.is_re_recruitment" class="col-6 md:col-6">
                    <div class="form-group">
                      <label>Lần tuyển dụng</label>
                      <InputNumber
                        v-model="model.recruitment_number"
                        inputId="minmax"
                        :min="0"
                        showButtons
                        class="ip36"
                      />
                    </div>
                  </div>
                  <div v-if="model.is_re_recruitment" class="col-12 md:col-12">
                    <div class="row">
                      <div class="col-6 md:col-6">
                        <div class="form-group">
                          <label>Ngày tuyển dụng lại</label>
                          <Calendar
                            class="ip36"
                            id="icon"
                            v-model="model.re_recruitment_date"
                            :showIcon="true"
                            placeholder="dd/mm/yyyy"
                          />
                        </div>
                      </div>
                      <div class="col-6 md:col-6">
                        <div class="form-group">
                          <label>Hình thức tuyển dụng</label>
                          <InputText
                            spellcheck="false"
                            class="ip36"
                            v-model="model.re_recruitment_form"
                            maxLength="500"
                          />
                        </div>
                      </div>
                      <div class="col-6 md:col-6">
                        <div class="form-group">
                          <label
                            >Nghề nghiệp bản thân trước khi được tuyển
                            dụng</label
                          >
                          <InputText
                            spellcheck="false"
                            class="ip36"
                            v-model="model.job_before_re_recruitment"
                            maxLength="500"
                          />
                        </div>
                      </div>
                      <div class="col-6 md:col-6">
                        <div class="form-group">
                          <label>Vị trí tuyển dụng</label>
                          <InputText
                            spellcheck="false"
                            class="ip36"
                            v-model="model.re_recruitment_position"
                            maxLength="500"
                          />
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </AccordionTab>
            <!-- 4. Trình độ học vấn - Quá trình đào tạo -->
            <AccordionTab>
              <template #header>
                <Toolbar class="w-full custoolbar p-0 font-bold">
                  <template #start>
                    <!-- <i class="pi pi-replay mr-2"></i> -->
                    <span
                      >4. Trình độ học vấn - Quá trình đào tạo</span
                    ></template
                  >
                  <template #end>
                    <a
                      @click="
                        openAddRow(2);
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
              <div class="col-12 md:col-12">
                <div class="row">
                  <div class="col-12 md:col-12">
                    <div class="form-group">
                      <label>4.1 Trình độ học vấn:</label>
                    </div>
                  </div>
                  <div class="col-4 md:col-4">
                    <div class="form-group">
                      <label>Trình độ phổ thông</label>
                      <Dropdown
                        :showClear="true"
                        :options="props.dictionarys[5]"
                        optionLabel="cultural_level_name"
                        optionValue="cultural_level_id"
                        placeholder="Chọn trình độ"
                        class="ip36"
                        v-model="model.cultural_level_id"
                        :style="{
                          whiteSpace: 'nowrap',
                          overflow: 'hidden',
                          textOverflow: 'ellipsis',
                        }"
                      />
                    </div>
                  </div>
                  <div class="col-4 md:col-4">
                    <div class="form-group">
                      <label>Trình độ học vấn cao nhất</label>
                      <InputText
                        spellcheck="false"
                        class="ip36"
                        v-model="model.cultural_level_max"
                        maxLength="500"
                        disabled
                      />
                    </div>
                  </div>
                  <div class="col-4 md:col-4">
                    <div class="form-group">
                      <label>Quản lý nhà nước</label>
                      <Dropdown
                        :showClear="true"
                        :options="props.dictionarys[14]"
                        optionLabel="management_state_name"
                        optionValue="management_state_id"
                        placeholder="Chọn cấp"
                        class="ip36"
                        v-model="model.management_state_id"
                        :style="{
                          whiteSpace: 'nowrap',
                          overflow: 'hidden',
                          textOverflow: 'ellipsis',
                        }"
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
                        v-model="model.political_theory_id"
                        :style="{
                          whiteSpace: 'nowrap',
                          overflow: 'hidden',
                          textOverflow: 'ellipsis',
                        }"
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
                        v-model="model.language_level_id"
                        :style="{
                          whiteSpace: 'nowrap',
                          overflow: 'hidden',
                          textOverflow: 'ellipsis',
                        }"
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
                        v-model="model.informatic_level_id"
                        :style="{
                          whiteSpace: 'nowrap',
                          overflow: 'hidden',
                          textOverflow: 'ellipsis',
                        }"
                      />
                    </div>
                  </div>
                  <div class="col-12 md:col-12">
                    <div class="form-group">
                      <label>4.2 Quá trình đào tạo:</label>
                    </div>
                  </div>
                  <div class="col-12 md:col-12 p-0">
                    <!-- <div
                      class="row"
                      v-for="(item, index) in datachilds[2]"
                    >
                      <Toolbar
                        class="w-full custoolbar p-0 font-bold"
                        :style="{
                          background: 'rgb(222, 230, 240) !important',
                          padding: '5px !important',
                          marginBottom: '1rem',
                        }"
                      >
                        <template #start></template>
                        <template #end>
                          <a
                            @click="deleteRow(2, index)"
                            class="hover"
                            v-tooltip.top="'Xóa'"
                          >
                            <i
                              class="pi pi-times-circle"
                              style="font-size: 18px"
                            ></i>
                          </a>
                        </template>
                      </Toolbar>
                      <div class="col-12 md:col-12">
                        <div class="form-group">
                          <label>Văn bằng:</label>
                        </div>
                      </div>
                      <div class="col-3 md:col-3">
                        <div class="form-group">
                          <label>Từ tháng, năm</label>
                          <Calendar
                            v-model="item.start_date"
                            :showIcon="false"
                            view="month"
                            dateFormat="mm/yy"
                            class="ip36"
                            placeholder="mm/yyyy"
                          />
                        </div>
                      </div>
                      <div class="col-3 md:col-3">
                        <div class="form-group">
                          <label>Đến tháng, năm</label>
                          <Calendar
                            v-model="item.end_date"
                            :showIcon="false"
                            view="month"
                            dateFormat="mm/yy"
                            class="ip36"
                            placeholder="mm/yyyy"
                          />
                        </div>
                      </div>
                      <div class="col-6 md:col-6">
                        <div class="form-group">
                          <label>Trình độ chuyên môn</label>
                          <Dropdown
                            :showClear="true"
                            :options="props.dictionarys[6]"
                            optionLabel="academic_level_name"
                            optionValue="academic_level_id"
                            placeholder="Chọn trình độ"
                            class="ip36"
                            v-model="item.academic_level_id"
                            :style="{
                              whiteSpace: 'nowrap',
                              overflow: 'hidden',
                              textOverflow: 'ellipsis',
                            }"
                          />
                        </div>
                      </div>
                      <div class="col-6 md:col-6">
                        <div class="form-group">
                          <label>Chuyên ngành</label>
                          <Dropdown
                            :showClear="true"
                            :filter="true"
                            :options="props.dictionarys[18]"
                            optionLabel="specialization_name"
                            optionValue="specialization_id"
                            placeholder="Chọn chuyên ngành"
                            v-model="item.specialized"
                            class="ip36"
                            :style="{
                              whiteSpace: 'nowrap',
                              overflow: 'hidden',
                              textOverflow: 'ellipsis',
                            }"
                          />
                        </div>
                      </div>
                      <div class="col-6 md:col-6">
                        <div class="form-group">
                          <label>Nơi đào tạo</label>
                          <Dropdown
                            :showClear="true"
                            :editable="true"
                            :filter="true"
                            :options="props.dictionarys[27]"
                            optionLabel="learning_place_name"
                            optionValue="learning_place_name"
                            placeholder="Chọn nơi đào tạo"
                            class="ip36"
                            v-model="item.university_name"
                            maxLength="250"
                            :style="{
                              whiteSpace: 'nowrap',
                              overflow: 'hidden',
                              textOverflow: 'ellipsis',
                            }"
                          />
                        </div>
                      </div>
                      <div class="col-6 md:col-6">
                        <div class="form-group">
                          <label>Hệ đào tạo</label>
                          <Dropdown
                            :showClear="true"
                            :options="props.dictionarys[12]"
                            optionLabel="form_traning_name"
                            optionValue="form_traning_id"
                            placeholder="Chọn hệ đào tạo"
                            v-model="item.form_traning_id"
                            class="ip36"
                            :style="{
                              whiteSpace: 'nowrap',
                              overflow: 'hidden',
                              textOverflow: 'ellipsis',
                            }"
                          />
                        </div>
                      </div>
                      <div class="col-6 md:col-6">
                        <div class="form-group">
                          <label>Năm tốt nghiệp</label>
                          <Calendar
                            v-model="item.graduation_year"
                            :showIcon="false"
                            view="year"
                            dateFormat="yy"
                            class="ip36"
                            placeholder="yyyy"
                          />
                        </div>
                      </div>
                      <div class="col-6 md:col-6">
                        <div class="form-group">
                          <label>Xếp loại</label>
                          <Dropdown
                            :showClear="true"
                            :editable="true"
                            :filter="true"
                            :options="[
                              { value: 1, title: 'Xuất sắc' },
                              { value: 2, title: 'Giỏi' },
                              { value: 3, title: 'Khá' },
                              { value: 4, title: 'TB Khá' },
                              { value: 5, title: 'Trung bình' },
                            ]"
                            optionLabel="title"
                            optionValue="title"
                            placeholder="Chọn xếp loại"
                            class="ip36"
                            v-model="item.rating"
                            maxLength="250"
                            :style="{
                              whiteSpace: 'nowrap',
                              overflow: 'hidden',
                              textOverflow: 'ellipsis',
                            }"
                          />
                        </div>
                      </div>
                      <div class="col-6 md:col-6">
                        <div class="form-group">
                          <label>Ngày cấp bằng</label>
                          <Calendar
                            class="ip36"
                            id="icon"
                            v-model="item.degree_date"
                            :showIcon="true"
                            placeholder="dd/mm/yyyy"
                          />
                        </div>
                      </div>
                      
                      <div class="col-6 md:col-6 format-center">
                        <div class="form-group m-0">
                          <div
                            class="field-checkbox flex justify-content-center"
                            style="height: 100%"
                          >
                            <InputSwitch v-model="item.is_man_degree" />
                            <label for="binary">Bằng cấp chính</label>
                          </div>
                        </div>
                      </div>
                      <div class="col-12 md:col-12">
                        <div class="form-group">
                          <label>Chứng chỉ:</label>
                        </div>
                      </div>
                      <div class="col-6 md:col-6">
                        <div class="form-group">
                          <label>Chứng chỉ</label>
                          <Dropdown
                            :showClear="true"
                            :options="props.dictionarys[13]"
                            optionLabel="certificate_name"
                            optionValue="certificate_id"
                            placeholder="Chọn văn bằng"
                            v-model="item.certificate_id"
                            class="ip36"
                            :style="{
                              whiteSpace: 'nowrap',
                              overflow: 'hidden',
                              textOverflow: 'ellipsis',
                            }"
                          />
                        </div>
                      </div>
                      <div class="col-6 md:col-6">
                        <div class="form-group">
                          <label>Số hiệu</label>
                          <InputText
                            v-model="item.certificate_key_code"
                            spellcheck="false"
                            type="text"
                            class="ip36"
                            maxLength="50"
                          />
                        </div>
                      </div>
                      <div class="col-6 md:col-6">
                        <div class="form-group">
                          <label>Phiên bản</label>
                          <InputText
                            v-model="item.certificate_version"
                            spellcheck="false"
                            type="text"
                            class="ip36"
                            maxLength="25"
                          />
                        </div>
                      </div>
                      <div class="col-6 md:col-6">
                        <div class="form-group">
                          <label>Lần phát hành</label>
                          <InputText
                            v-model="item.certificate_release_time"
                            spellcheck="false"
                            type="text"
                            class="ip36"
                            maxLength="25"
                          />
                        </div>
                      </div>
                      <div class="col-6 md:col-6">
                        <div class="form-group">
                          <label>Ngày hiệu lực</label>
                          <Calendar
                            v-model="item.certificate_start_date"
                            :showIcon="false"
                            class="ip36"
                            placeholder="dd/mm/yyyy"
                          />
                        </div>
                      </div>
                      <div class="col-6 md:col-6">
                        <div class="form-group">
                          <label>Ngày hết hiệu lực</label>
                          <Calendar
                            v-model="item.certificate_end_date"
                            :showIcon="false"
                            class="ip36"
                            placeholder="dd/mm/yyyy"
                          />
                        </div>
                      </div>
                    </div> -->
                    <DataTable
                      :value="datachilds[2]"
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
                            @click="deleteRow(2, slotProps.index)"
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
                          <!-- <Calendar
                            v-model="slotProps.data.start_date"
                            :showIcon="false"
                            view="month"
                            dateFormat="mm/yy"
                            class="ip36"
                            placeholder="mm/yyyy"
                          /> -->
                          <InputText
                            v-model="slotProps.data.start_date"
                            class="ip36"
                            maxLength="250"
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
                          <!-- <Calendar
                            v-model="slotProps.data.end_date"
                            :showIcon="false"
                            view="month"
                            dateFormat="mm/yy"
                            class="ip36"
                            placeholder="mm/yyyy"
                          /> -->
                          <InputText
                            v-model="slotProps.data.end_date"
                            class="ip36"
                            maxLength="250"
                          />
                        </template>
                      </Column>
                      <Column
                        field="academic_level_id"
                        header="Trình độ chuyên môn"
                        headerStyle="text-align:center;width:180px;height:50px"
                        bodyStyle="text-align:center;width:180px;"
                        class="align-items-center justify-content-center text-center"
                      >
                        <template #body="slotProps">
                          <div class="form-group m-0">
                            <Dropdown
                              :showClear="true"
                              :options="props.dictionarys[6]"
                              optionLabel="academic_level_name"
                              optionValue="academic_level_id"
                              placeholder="Chọn trình độ"
                              class="ip36"
                              v-model="slotProps.data.academic_level_id"
                              :style="{
                                whiteSpace: 'nowrap',
                                overflow: 'hidden',
                                textOverflow: 'ellipsis',
                              }"
                            />
                          </div>
                        </template>
                      </Column>
                      <Column
                        field="specialized"
                        header="Chuyên ngành"
                        headerStyle="text-align:center;width:180px;height:50px"
                        bodyStyle="text-align:center;width:180px;"
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
                              :style="{
                                whiteSpace: 'nowrap',
                                overflow: 'hidden',
                                textOverflow: 'ellipsis',
                              }"
                            />
                          </div>
                        </template>
                      </Column>
                      <Column
                        field="university_name"
                        header="Nơi đào tạo"
                        headerStyle="text-align:center;width:180px;height:50px"
                        bodyStyle="text-align:center;width:180px;"
                        class="align-items-center justify-content-center text-center"
                      >
                        <template #body="slotProps">
                          <Dropdown
                            :showClear="true"
                            :editable="true"
                            :filter="true"
                            :options="props.dictionarys[27]"
                            optionLabel="learning_place_name"
                            optionValue="learning_place_name"
                            placeholder="Chọn nơi đào tạo"
                            class="ip36"
                            v-model="slotProps.data.university_name"
                            maxLength="250"
                          />
                        </template>
                      </Column>
                      <Column
                        field="form_traning_id"
                        header="Hệ đào tạo"
                        headerStyle="text-align:center;width:180px;height:50px"
                        bodyStyle="text-align:center;width:180px;"
                        class="align-items-center justify-content-center text-center"
                      >
                        <template #body="slotProps">
                          <div class="form-group m-0">
                            <Dropdown
                              :showClear="true"
                              :options="props.dictionarys[12]"
                              optionLabel="form_traning_name"
                              optionValue="form_traning_id"
                              placeholder="Chọn hệ đào tạo"
                              v-model="slotProps.data.form_traning_id"
                              class="ip36"
                              :style="{
                                whiteSpace: 'nowrap',
                                overflow: 'hidden',
                                textOverflow: 'ellipsis',
                              }"
                            />
                          </div>
                        </template>
                      </Column>
                      <Column
                        field="graduation_year"
                        header="Năm tốt nghiệp"
                        headerStyle="text-align:center;width:120px;height:50px"
                        bodyStyle="text-align:center;width:120px;"
                        class="align-items-center justify-content-center text-center"
                      >
                        <template #body="slotProps">
                          <!-- <Calendar
                            v-model="slotProps.data.graduation_year"
                            :showIcon="false"
                            view="year"
                            dateFormat="yy"
                            class="ip36"
                            placeholder="yyyy"
                          /> -->
                          <InputText
                            v-model="slotProps.data.graduation_year"
                            class="ip36"
                          />
                        </template>
                      </Column>
                      <Column
                        field="rating"
                        header="Xếp loại"
                        headerStyle="text-align:center;width:180px;height:50px"
                        bodyStyle="text-align:center;width:180px;"
                        class="align-items-center justify-content-center text-center"
                      >
                        <template #body="slotProps">
                          <Dropdown
                            :showClear="true"
                            :editable="true"
                            :filter="true"
                            :options="[
                              { value: 1, title: 'Xuất sắc' },
                              { value: 2, title: 'Giỏi' },
                              { value: 3, title: 'Khá' },
                              { value: 4, title: 'TB Khá' },
                              { value: 5, title: 'Trung bình' },
                            ]"
                            optionLabel="title"
                            optionValue="title"
                            placeholder="Chọn xếp loại"
                            class="ip36"
                            v-model="slotProps.data.rating"
                            maxLength="250"
                          />
                        </template>
                      </Column>
                      <Column
                        field="degree_date"
                        header="Ngày cấp bằng"
                        headerStyle="text-align:center;width:120px;height:50px"
                        bodyStyle="text-align:center;width:120px;"
                        class="align-items-center justify-content-center text-center"
                      >
                        <template #body="slotProps">
                          <!-- <Calendar
                            v-model="slotProps.data.degree_date"
                            :showIcon="false"
                            view="day"
                            dateFormat="dd/mm/yy"
                            class="ip36"
                            placeholder="dd/mm/yyyy"
                          /> -->
                          <InputText
                            v-model="slotProps.data.degree_date"
                            class="ip36"
                            maxLength="250"
                          />
                        </template>
                      </Column>

                      <Column
                        field="is_man_degree"
                        header="Bằng cấp chính"
                        headerStyle="text-align:center;width:120px;height:50px"
                        bodyStyle="text-align:center;width:120px;"
                        class="align-items-center justify-content-center text-center"
                      >
                        <template #body="slotProps">
                          <div class="form-group">
                            <div
                              class="field-checkbox flex justify-content-center"
                              style="height: 100%"
                            >
                              <InputSwitch
                                v-model="slotProps.data.is_man_degree"
                              />
                              <label for="binary">Bằng cấp chính</label>
                            </div>
                          </div>
                        </template>
                      </Column>
                      <Column
                        field="certificate_id"
                        header="Chứng chỉ"
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
                              :style="{
                                whiteSpace: 'nowrap',
                                overflow: 'hidden',
                                textOverflow: 'ellipsis',
                              }"
                            />
                          </div>
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
                      <template #empty>
                        <div
                          class="align-items-center justify-content-center p-4 text-center m-auto"
                          style="display: flex; width: 100%"
                        ></div>
                      </template>
                    </DataTable>
                  </div>
                </div>
              </div>
            </AccordionTab>
            <!-- 5 Thông tin đảng, tham gia TCCT- XH -->
            <AccordionTab>
              <template #header>
                <Toolbar class="w-full custoolbar p-0 font-bold">
                  <template #start>
                    <!-- <i class="pi pi-replay mr-2"></i> -->
                    <span>5 Thông tin đảng, tham gia TCCT- XH</span></template
                  >
                  <template #end>
                    <!-- <a
                      @click="
                        addRow(3);
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
                    </a> -->
                  </template>
                </Toolbar>
              </template>
              <!-- <div class="col-12 md:col-12 p-0">
                <div style="">
                  <DataTable
                    :value="datachilds[3]"
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
                          @click="deleteRow(3, slotProps.index)"
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
                            :style="{
                              whiteSpace: 'nowrap',
                              overflow: 'hidden',
                              textOverflow: 'ellipsis',
                            }"
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
              </div> -->
              <div class="col-12 md:col-12 p-0">
                <div class="row">
                  <div class="col-12 md:col-12">
                    <div class="form-group">
                      <label><b>Tham gia đoàn thanh niên</b></label>
                    </div>
                  </div>
                  <div class="col-6 md:col-6">
                    <div class="form-group">
                      <label>Số thẻ Đoàn</label>
                      <InputText
                        spellcheck="false"
                        class="ip36"
                        v-model="model.bevy_code"
                        maxLength="50"
                      />
                    </div>
                  </div>
                  <div class="col-6 md:col-6">
                    <div class="form-group">
                      <label>Ngày vào Đoàn</label>
                      <Calendar
                        :showIcon="true"
                        v-model="model.bevy_date"
                        class="ip36"
                        id="icon"
                        placeholder="dd/mm/yyyy"
                      />
                    </div>
                  </div>
                  <div class="col-6 md:col-6">
                    <div class="form-group">
                      <label>Nơi vào Đoàn</label>
                      <InputText
                        spellcheck="false"
                        class="ip36"
                        v-model="model.bevy_address"
                        maxLength="500"
                      />
                    </div>
                  </div>
                  <div class="col-6 md:col-6">
                    <div class="form-group">
                      <label>Chức vụ Đoàn hiện tại</label>
                      <InputText
                        spellcheck="false"
                        class="ip36"
                        v-model="model.bevy_position"
                        maxLength="500"
                      />
                    </div>
                  </div>
                  <div class="col-12 md:col-12">
                    <div class="form-group">
                      <label><b>Thông tin Đảng</b></label>
                    </div>
                  </div>
                  <div class="col-6 md:col-6 format-center">
                    <div class="form-group">
                      <div
                        class="field-checkbox flex justify-content-center"
                        style="height: 100%"
                      >
                        <InputSwitch v-model="model.is_partisan" />
                        <label for="binary">Là Đảng viên</label>
                      </div>
                    </div>
                  </div>
                  <div class="col-6 md:col-6">
                    <div class="form-group">
                      <label>Ngạch công chức (viên chức)</label>
                      <Dropdown
                        :showClear="true"
                        :options="props.dictionarys[26]"
                        optionLabel="newname"
                        optionValue="newname"
                        placeholder="Chọn ngạch công chức (viên chức)"
                        class="ip36"
                        v-model="model.civil_servant_rank_name"
                        :style="{
                          whiteSpace: 'nowrap',
                          overflow: 'hidden',
                          textOverflow: 'ellipsis',
                        }"
                      />
                    </div>
                  </div>

                  <div class="col-6 md:col-6">
                    <div class="form-group">
                      <label>Số thẻ Đảng</label>
                      <InputText
                        spellcheck="false"
                        class="ip36"
                        v-model="model.card_partisan"
                        maxLength="50"
                      />
                    </div>
                  </div>
                  <div class="col-3 md:col-3">
                    <div class="form-group">
                      <label>Ngày vào Đảng</label>
                      <Calendar
                        :showIcon="true"
                        v-model="model.partisan_date"
                        class="ip36"
                        id="icon"
                        placeholder="dd/mm/yyyy"
                      />
                    </div>
                  </div>
                  <div class="col-3 md:col-3">
                    <div class="form-group">
                      <label>Ngày vào Đảng chính thức</label>
                      <Calendar
                        :showIcon="true"
                        v-model="model.partisan_main_date"
                        class="ip36"
                        id="icon"
                        placeholder="dd/mm/yyyy"
                      />
                    </div>
                  </div>
                  <div class="col-6 md:col-6">
                    <div class="form-group">
                      <label>Nơi kết nạp</label>
                      <InputText
                        spellcheck="false"
                        class="ip36"
                        v-model="model.partisan_address"
                        maxLength="500"
                      />
                    </div>
                  </div>
                  <div class="col-6 md:col-6">
                    <div class="form-group">
                      <label>Tính phí Đảng tại đơn vị</label>
                      <InputNumber
                        v-model="model.partisan_fee"
                        mode="decimal"
                        :minFractionDigits="0"
                        :maxFractionDigits="2"
                        class="ip36"
                      />
                    </div>
                  </div>
                  <div class="col-6 md:col-6">
                    <div class="form-group">
                      <label>Chị bộ sinh hoạt Đảng</label>
                      <InputText
                        spellcheck="false"
                        class="ip36"
                        v-model="model.partisan_branch"
                        maxLength="500"
                      />
                    </div>
                  </div>
                  <div class="col-6 md:col-6">
                    <div class="form-group">
                      <label>Đảng bộ chính thức</label>
                      <InputText
                        spellcheck="false"
                        class="ip36"
                        v-model="model.partisan_official"
                        maxLength="500"
                      />
                    </div>
                  </div>
                  <div class="col-3 md:col-3">
                    <div class="form-group">
                      <label>Ngày tham gia cách mạng</label>
                      <Calendar
                        :showIcon="true"
                        v-model="model.partisan_joindate"
                        class="ip36"
                        id="icon"
                        placeholder="dd/mm/yyyy"
                      />
                    </div>
                  </div>
                  <div class="col-3 md:col-3">
                    <div class="form-group">
                      <label>Ngày tham gia tổ chức</label>
                      <Calendar
                        :showIcon="true"
                        v-model="model.organization_joindate"
                        class="ip36"
                        id="icon"
                        placeholder="dd/mm/yyyy"
                      />
                    </div>
                  </div>
                  <div class="col-6 md:col-6">
                    <div class="form-group">
                      <label>Công việc trong tổ chức</label>
                      <InputText
                        spellcheck="false"
                        class="ip36"
                        v-model="model.organization_task"
                        maxLength="500"
                      />
                    </div>
                  </div>
                  <div class="col-6 md:col-6">
                    <div class="form-group">
                      <label>Danh hiệu</label>
                      <InputText
                        spellcheck="false"
                        class="ip36"
                        v-model="model.appellation"
                        maxLength="500"
                      />
                    </div>
                  </div>
                  <div class="col-6 md:col-6">
                    <div class="form-group">
                      <label>Huy hiệu</label>
                      <InputText
                        spellcheck="false"
                        class="ip36"
                        v-model="model.armorial"
                        maxLength="500"
                      />
                    </div>
                  </div>
                </div>
              </div>
            </AccordionTab>
            <!-- 6. Thông tin tham gia quân đội -->
            <AccordionTab>
              <template #header>
                <!-- <i class="pi pi-chart-line mr-2"></i> -->
                <span>6. Thông tin tham gia quân đội</span>
              </template>
              <div class="col-12 md:col-12">
                <div class="row">
                  <div class="col-6 md:col-6">
                    <div class="form-group">
                      <label>Ngày nhập ngũ</label>
                      <Calendar
                        :showIcon="true"
                        v-model="model.military_start_date"
                        class="ip36"
                        id="icon"
                        placeholder="dd/mm/yyyy"
                      />
                    </div>
                  </div>
                  <div class="col-6 md:col-6">
                    <div class="form-group">
                      <label>Ngày xuất ngũ</label>
                      <Calendar
                        :showIcon="true"
                        v-model="model.military_end_date"
                        class="ip36"
                        id="icon"
                        placeholder="dd/mm/yyyy"
                      />
                    </div>
                  </div>
                  <div class="col-6 md:col-6">
                    <div class="form-group">
                      <label>Quân hàm cao nhất</label>
                      <InputText
                        spellcheck="false"
                        class="ip36"
                        v-model="model.military_rank"
                        maxLength="50"
                      />
                    </div>
                  </div>
                  <div class="col-6 md:col-6">
                    <div class="form-group">
                      <label>Danh hiệu phong tặng cao nhất</label>
                      <Dropdown
                        :showClear="true"
                        :editable="true"
                        :options="[
                          { value: 1, title: 'Anh hùng lao động' },
                          { value: 2, title: 'Anh hùng lực lượng vũ trang' },
                          { value: 3, title: 'Nhà giáo' },
                          { value: 4, title: 'Thầy thuốc' },
                          { value: 5, title: 'Nghệ sĩ nhân dân ưu tú' },
                        ]"
                        optionLabel="title"
                        optionValue="title"
                        placeholder="Chọn danh hiệu phong tặng cao nhất"
                        class="ip36"
                        v-model="model.military_title"
                        maxLength="250"
                        :style="{
                          whiteSpace: 'nowrap',
                          overflow: 'hidden',
                          textOverflow: 'ellipsis',
                        }"
                      />
                    </div>
                  </div>
                  <!-- <div class="col-6 md:col-6">
                    <div class="form-group">
                      <label>Sở trường công tác</label>
                      <InputText
                        spellcheck="false"
                        class="ip36"
                        v-model="model.military_forte"
                        maxLength="250"
                      />
                    </div>
                  </div> -->
                  <!-- <div class="col-6 md:col-6">
                    <div class="form-group">
                      <label>Sức khỏe</label>
                      <InputText
                        spellcheck="false"
                        class="ip36"
                        v-model="model.military_health"
                        maxLength="250"
                      />
                    </div>
                  </div> -->

                  <div class="col-6 md:col-6">
                    <div class="form-group">
                      <label>Thương binh hạng</label>
                      <InputText
                        spellcheck="false"
                        class="ip36"
                        v-model="model.military_veterans_rank"
                        maxLength="50"
                      />
                    </div>
                  </div>
                  <div class="col-6 md:col-6">
                    <div class="form-group">
                      <label>Con gia đình chính sách</label>
                      <Dropdown
                        :showClear="true"
                        :editable="true"
                        :options="[
                          { value: 1, title: 'Con thương binh' },
                          { value: 2, title: 'Con liệt sĩ' },
                          { value: 3, title: 'Người nhiễm chất độc da cam' },
                          { value: 4, title: 'Dioxin' },
                        ]"
                        optionLabel="title"
                        optionValue="title"
                        placeholder="Là con gia đình chính sách"
                        class="ip36"
                        v-model="model.military_policy_family"
                        maxLength="250"
                        :style="{
                          whiteSpace: 'nowrap',
                          overflow: 'hidden',
                          textOverflow: 'ellipsis',
                        }"
                      />
                    </div>
                  </div>
                </div>
              </div>
            </AccordionTab>
            <!-- 7. Kinh nghiệm làm việc -->
            <AccordionTab>
              <template #header>
                <Toolbar class="w-full custoolbar p-0 font-bold">
                  <template #start>
                    <span
                      >7. Quá trình công tác trước khi vào đơn vị</span
                    ></template
                  >
                  <template #end>
                    <a
                      @click="
                        openAddRow(4);
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
                <div class="row">
                  <div class="col-6 md:col-6 format-center">
                    <div class="form-group m-0">
                      <label
                        >Thời gian làm việc trong khối nhà nước trước khi vào
                        công ty:
                      </label>
                    </div>
                  </div>
                  <div class="col-3 md:col-3">
                    <div class="form-group">
                      <label>Năm</label>
                      <InputNumber
                        v-model="model.seniority_year"
                        inputId="minmax"
                        :min="0"
                        :max="100"
                        showButtons
                        class="ip36"
                      />
                    </div>
                  </div>
                  <div class="col-3 md:col-3">
                    <div class="form-group">
                      <label>Tháng</label>
                      <InputNumber
                        v-model="model.seniority_month"
                        inputId="minmax"
                        :min="0"
                        :max="11"
                        showButtons
                        class="ip36"
                      />
                    </div>
                  </div>
                </div>
                <div>
                  <DataTable
                    :value="datachilds[4]"
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
                          @click="deleteRow(4, slotProps.index)"
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
                        <!-- <Calendar
                          v-model="slotProps.data.start_date"
                          :showIcon="false"
                          view="month"
                          dateFormat="mm/yy"
                          class="ip36"
                          placeholder="mm/yyyy"
                        /> -->
                        <InputText
                          v-model="slotProps.data.start_date"
                          class="ip36"
                          maxLength="250"
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
                        <!-- <Calendar
                          v-model="slotProps.data.end_date"
                          :showIcon="false"
                          view="month"
                          dateFormat="mm/yy"
                          class="ip36"
                          placeholder="mm/yyyy"
                        /> -->
                        <InputText
                          v-model="slotProps.data.end_date"
                          class="ip36"
                          maxLength="250"
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
            <!-- 8. Thông tin gia đình, người phụ thuộc -->
            <AccordionTab>
              <template #header>
                <Toolbar class="w-full custoolbar p-0 font-bold">
                  <template #start>
                    <!-- <i class="pi pi-users mr-2"></i> -->
                    <span
                      >8. Thông tin gia đình, người phụ thuộc</span
                    ></template
                  >
                  <template #end>
                    <a
                      @click="
                        openAddRow(1);
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
                    :value="datachilds[1]"
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
                          @click="deleteRow(1, slotProps.index)"
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
                      field="is_type"
                      header="Quan hệ gia đình"
                      headerStyle="text-align:center;width:170px;height:50px"
                      bodyStyle="text-align:center;width:170px;"
                      class="align-items-center justify-content-center text-center"
                    >
                      <template #body="slotProps">
                        <div class="form-group m-0">
                          <Dropdown
                            :showClear="true"
                            :options="[
                              { value: 1, title: 'Về bản thân' },
                              { value: 2, title: 'Về bên vợ/chồng' },
                            ]"
                            optionLabel="title"
                            optionValue="value"
                            placeholder="Chọn quan hệ"
                            v-model="slotProps.data.is_type"
                            class="ip36"
                            :style="{
                              whiteSpace: 'nowrap',
                              overflow: 'hidden',
                              textOverflow: 'ellipsis',
                            }"
                          />
                        </div>
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
                            :style="{
                              whiteSpace: 'nowrap',
                              overflow: 'hidden',
                              textOverflow: 'ellipsis',
                            }"
                          />
                        </div>
                      </template>
                    </Column>
                    <Column
                      field="birthday"
                      header="Năm sinh"
                      headerStyle="text-align:center;width:120px;height:50px"
                      bodyStyle="text-align:center;width:120px;"
                      class="align-items-center justify-content-center text-center"
                    >
                      <template #body="slotProps">
                        <Calendar
                          v-model="slotProps.data.birthday"
                          :showIcon="false"
                          inputMask="9999"
                          view="year"
                          dateFormat="yy"
                          class="ip36"
                          placeholder="yyyy"
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
                            :style="{
                              whiteSpace: 'nowrap',
                              overflow: 'hidden',
                              textOverflow: 'ellipsis',
                            }"
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
                    <Column
                      field="note"
                      header="Cùng cơ quan"
                      headerStyle="text-align:center;width:150px;height:50px"
                      bodyStyle="text-align:center;width:150px;"
                      class="align-items-center justify-content-center text-center"
                    >
                      <template #body="slotProps">
                        <div class="form-group m-0">
                          <div
                            class="field-checkbox flex justify-content-center"
                            style="height: 100%"
                          >
                            <InputSwitch v-model="slotProps.data.is_company" />
                          </div>
                        </div>
                      </template>
                    </Column>
                    <Column
                      field="note"
                      header="Đã mất"
                      headerStyle="text-align:center;width:150px;height:50px"
                      bodyStyle="text-align:center;width:150px;"
                      class="align-items-center justify-content-center text-center"
                    >
                      <template #body="slotProps">
                        <div class="form-group m-0">
                          <div
                            class="field-checkbox flex justify-content-center"
                            style="height: 100%"
                          >
                            <InputSwitch v-model="slotProps.data.is_die" />
                          </div>
                        </div>
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
              <div class="col-12 md:col-12 p-0">
                <div class="row">
                  <div class="col-4 md:col-4">
                    <div class="form-group">
                      <label>Thành phần gia đình xuất thân</label>
                      <InputText
                        spellcheck="false"
                        class="ip36"
                        v-model="model.family_member"
                        maxLength="500"
                      />
                    </div>
                  </div>

                  <div class="col-4 md:col-4">
                    <div class="form-group">
                      <label>Công việc đã làm lâu nhất</label>
                      <InputText
                        spellcheck="false"
                        class="ip36"
                        v-model="model.task_longest"
                        maxLength="500"
                      />
                    </div>
                  </div>
                  <div class="col-4 md:col-4">
                    <div class="form-group">
                      <label>Sở trường công tác</label>
                      <InputText
                        spellcheck="false"
                        class="ip36"
                        v-model="model.mission_forte"
                        maxLength="500"
                      />
                    </div>
                  </div>
                  <div class="col-12 md:col-12">
                    <div class="form-group">
                      <label>Khen thưởng</label>
                      <Textarea
                        :autoResize="true"
                        rows="4"
                        v-model="model.military_reward"
                      />
                    </div>
                  </div>
                  <div class="col-12 md:col-12">
                    <div class="form-group">
                      <label>Kỷ luật</label>
                      <Textarea
                        :autoResize="true"
                        rows="4"
                        v-model="model.military_discipline"
                      />
                    </div>
                  </div>
                  <div class="col-12 md:col-12">
                    <div class="form-group">
                      <label
                        >Khai rõ: Bị bắt, bị tù, bản thân có làm việc trong chế
                        độ cũ</label
                      >
                      <Textarea
                        :autoResize="true"
                        rows="4"
                        placeholder="Nhập thông tin"
                        v-model="model.biography_first"
                      />
                    </div>
                  </div>
                  <div class="col-12 md:col-12">
                    <div class="form-group">
                      <label
                        >Tham gia hoặc có quan hệ với các tổ chức chính trị,
                        kinh tế, xã hội nào ở nước ngoài</label
                      >
                      <Textarea
                        :autoResize="true"
                        rows="4"
                        placeholder="Nhập thông tin"
                        v-model="model.biography_second"
                      />
                    </div>
                  </div>
                  <div class="col-12 md:col-12">
                    <div class="form-group">
                      <label>Có thân nhân ở nước ngoài (làm gì, địa chỉ)</label>
                      <Textarea
                        :autoResize="true"
                        rows="4"
                        placeholder="Nhập thông tin"
                        v-model="model.biography_third"
                      />
                    </div>
                  </div>
                  <div class="col-12 md:col-12">
                    <div class="form-group">
                      <label
                        >Nhật xét, đánh giá của cơ quan, đơn vị quản lý và sử
                        dụng cán bộ, công chức</label
                      >
                      <Textarea
                        :autoResize="true"
                        rows="4"
                        placeholder="Nhập thông tin"
                        v-model="model.note"
                      />
                    </div>
                  </div>
                </div>
              </div>
            </AccordionTab>
            <!-- 10.	Đính kèm khác (file số hóa liên quan) -->
            <AccordionTab>
              <template #header>
                <!-- <i class="pi pi-chart-line mr-2"></i> -->
                <span> 10. Nguồn thu nhập chính của gia đình</span>
              </template>
              <div class="col-12 md:col-12 p-0">
                <div class="row">
                  <div class="col-6 md:col-6">
                    <div class="form-group">
                      <label>Lương gia đình</label>
                      <InputNumber
                        showButtons
                        v-model="model.salary_family"
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
                      <label>Nguồn khác</label>
                      <InputNumber
                        showButtons
                        v-model="model.salary_orther"
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
                      <label>Được cấp, được thuê, loại nhà</label>
                      <InputText
                        spellcheck="false"
                        class="ip36"
                        v-model="model.type_rent"
                        maxLength="500"
                      />
                    </div>
                  </div>
                  <div class="col-6 md:col-6">
                    <div class="form-group">
                      <label>Diện tích nhà sử dụng</label>
                      <InputNumber
                        showButtons
                        v-model="model.area_level"
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
                      <label>Nhà tự mua, loại nhà</label>
                      <InputText
                        spellcheck="false"
                        class="ip36"
                        v-model="model.type_house"
                        maxLength="500"
                      />
                    </div>
                  </div>
                  <div class="col-6 md:col-6">
                    <div class="form-group">
                      <label>Diện tích nhà mua</label>
                      <InputNumber
                        showButtons
                        v-model="model.area_buy"
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
                      <label>Diện tích đất được cấp</label>
                      <InputNumber
                        showButtons
                        v-model="model.area_granted"
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
                      <label>Diện tích đất tự mua</label>
                      <InputNumber
                        showButtons
                        v-model="model.area_buy_yourself"
                        mode="decimal"
                        locale="vi-VN"
                        :minFractionDigits="0"
                        :maxFractionDigits="2"
                        class="ip36"
                      />
                    </div>
                  </div>
                  <div class="col-4 md:col-4">
                    <div class="form-group">
                      <label>Tổng diện tích</label>
                      <InputNumber
                        showButtons
                        v-model="model.area_manufacture"
                        mode="decimal"
                        locale="vi-VN"
                        :minFractionDigits="0"
                        :maxFractionDigits="2"
                        class="ip36"
                      />
                    </div>
                  </div>
                  <div class="col-4 md:col-4">
                    <div class="form-group">
                      <label>Địa điểm ký</label>
                      <InputText
                        spellcheck="false"
                        class="ip36"
                        v-model="model.sign_address"
                        maxLength="500"
                      />
                    </div>
                  </div>
                  <div class="col-4 md:col-4">
                    <div class="form-group">
                      <label>Ngày ký</label>
                      <Calendar
                        class="ip36"
                        id="icon"
                        v-model="model.sign_date"
                        :showIcon="true"
                        placeholder="dd/mm/yyyy"
                      />
                    </div>
                  </div>
                </div>
              </div>
            </AccordionTab>
            <!-- 11.	Đính kèm khác (file số hóa liên quan) -->
            <AccordionTab>
              <template #header>
                <!-- <i class="pi pi-chart-line mr-2"></i> -->
                <span> 11. Đính kèm khác (file số hóa liên quan)</span>
              </template>
              <div class="col-12 md:col-12">
                <div class="form-group">
                  <label>Tải file lên </label>
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
      </div>
    </form>
    <Toolbar
      class="outline-none surface-0 border-none w-full"
      :style="{
        position: 'sticky',
        zIndex: '1',
        bottom: '0',
        background: '#fff',
      }"
    >
      <template #start>
        <Button
          label="Hủy"
          icon="pi pi-times"
          @click="props.closeDialog()"
          class="p-button-text"
      /></template>
      <template #end>
        <Button
          v-if="props.approve === true && history.is_approve === 1"
          label="Duyệt hồ sơ"
          class="mr-2"
          icon="pi pi-check"
          @click="sendProfile(true, 2)"
        />
        <Button
          v-if="props.approve === true && history.is_approve === 1"
          label="Trả lại"
          class="mr-2 p-button-danger"
          icon="pi pi-undo"
          @click="sendProfile(true, 3)"
        />
        <Button
          v-if="history.is_approve === 0"
          label="Gửi duyệt"
          class="mr-2"
          icon="pi pi-send"
          @click="sendProfile(true, 1)"
        />
        <Button
          v-if="history.is_approve === 0"
          label="Lưu và gửi duyệt"
          class="mr-2"
          icon="pi pi-check"
          @click="saveModel(true)"
        />
        <Button
          v-if="!props.approve"
          label="Lưu"
          icon="pi pi-check"
          @click="saveModel()"
        />
      </template>
    </Toolbar>
  </Sidebar>

  <!--Dialog 1-->
  <Dialog
    header="Quá trình công tác trước khi vào đơn vị"
    v-model:visible="displayDialog1"
    :style="{ width: '60vw' }"
    :maximizable="true"
    :closable="true"
    style="z-index: 9001"
  >
    <form @submit.prevent="" name="submitform">
      <div class="grid formgrid m-2">
        <div class="col-4 md:col-4">
          <div class="form-group">
            <label>Quan hệ gia đình</label>
            <Dropdown
              :showClear="true"
              :options="[
                { value: 1, title: 'Về bản thân' },
                { value: 2, title: 'Về bên vợ/chồng' },
              ]"
              optionLabel="title"
              optionValue="value"
              placeholder="Chọn quan hệ"
              v-model="modeldetail.is_type"
              class="ip36"
              :style="{
                whiteSpace: 'nowrap',
                overflow: 'hidden',
                textOverflow: 'ellipsis',
              }"
            />
          </div>
        </div>
        <div class="col-4 md:col-4">
          <div class="form-group">
            <label>Họ tên</label>
            <InputText
              v-model="modeldetail.relative_name"
              spellcheck="false"
              type="text"
              class="ip36"
              maxLength="25"
            />
          </div>
        </div>
        <div class="col-4 md:col-4">
          <div class="form-group">
            <label>Quan hệ</label>
            <Dropdown
              :showClear="true"
              :options="props.dictionarys[11]"
              optionLabel="relationship_name"
              optionValue="relationship_id"
              placeholder="Chọn quan hệ"
              v-model="modeldetail.relationship_id"
              class="ip36"
              :style="{
                whiteSpace: 'nowrap',
                overflow: 'hidden',
                textOverflow: 'ellipsis',
              }"
            />
          </div>
        </div>
        <div class="col-4 md:col-4">
          <div class="form-group">
            <label>Năm sinh</label>
            <Calendar
              v-model="modeldetail.birthday"
              :showIcon="false"
              inputMask="9999"
              view="year"
              dateFormat="yy"
              class="ip36"
              placeholder="yyyy"
            />
          </div>
        </div>
        <div class="col-4 md:col-4">
          <div class="form-group">
            <label>Điện thoại</label>
            <InputMask
              v-model="modeldetail.phone"
              mask="9999999999"
              placeholder="__________"
              class="ip36"
            />
          </div>
        </div>
        <div class="col-4 md:col-4">
          <div class="form-group">
            <label>Mã số thuế</label>
            <InputText
              v-model="modeldetail.tax_code"
              spellcheck="false"
              type="text"
              class="ip36"
              maxLength="50"
            />
          </div>
        </div>
        <div class="col-3 md:col-3">
          <div class="form-group">
            <label>CCCD/Hộ chiếu</label>
            <InputText
              v-model="modeldetail.identification_citizen"
              spellcheck="false"
              type="text"
              class="ip36"
              maxLength="50"
            />
          </div>
        </div>
        <div class="col-3 md:col-3">
          <div class="form-group">
            <label>Ngày cấp</label>
            <Calendar
              v-model="modeldetail.identification_date_issue"
              :showIcon="true"
              class="ip36"
              placeholder="dd/mm/yyyy"
            />
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Nơi cấp</label>
            <InputText
              v-model="modeldetail.identification_place_issue"
              spellcheck="false"
              type="text"
              class="ip36"
              maxLength="250"
            />
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Phụ thuộc</label>
            <Dropdown
              :options="[
                { value: 1, title: 'Có phụ thuộc' },
                { value: 0, title: 'Không phụ thuộc' },
              ]"
              :filter="false"
              :showClear="true"
              :editable="false"
              v-model="modeldetail.is_dependent"
              optionLabel="title"
              optionValue="value"
              placeholder="Chọn trạng thái"
              class="ip36"
              :style="{
                whiteSpace: 'nowrap',
                overflow: 'hidden',
                textOverflow: 'ellipsis',
              }"
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
        </div>
        <div class="col-3 md:col-3">
          <div class="form-group">
            <label>Từ ngày</label>
            <Calendar
              v-model="modeldetail.start_date"
              :showIcon="true"
              class="ip36"
              placeholder="dd/mm/yyyy"
            />
          </div>
        </div>
        <div class="col-3 md:col-3">
          <div class="form-group">
            <label>Đến ngày</label>
            <Calendar
              v-model="modeldetail.end_date"
              :showIcon="true"
              class="ip36"
              placeholder="dd/mm/yyyy"
            />
          </div>
        </div>
        <div div class="col-6 md:col-6">
          <div class="form-group">
            <label>Thông tin cơ bản</label>
            <InputText
              v-model="modeldetail.info"
              spellcheck="false"
              type="text"
              class="ip36"
            />
          </div>
        </div>
        <div div class="col-6 md:col-6">
          <div class="form-group">
            <label>Ghi chú</label>
            <InputText
              v-model="modeldetail.note"
              spellcheck="false"
              type="text"
              class="ip36"
            />
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <div
              class="field-checkbox flex justify-content-center"
              style="height: 100%"
            >
              <InputSwitch v-model="modeldetail.is_company" />
              <label for="binary">Cùng cơ quan</label>
            </div>
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <div
              class="field-checkbox flex justify-content-center"
              style="height: 100%"
            >
              <InputSwitch v-model="modeldetail.is_die" />
              <label for="binary">Đã mất</label>
            </div>
          </div>
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="closeDialog()"
        class="p-button-text"
      />
      <Button
        label="Lưu và tiếp tục"
        icon="pi pi-check"
        @click="saveRow(1, true)"
      />
      <Button label="Lưu" icon="pi pi-check" @click="saveRow(1)" />
    </template>
  </Dialog>
  <!--Dialog 2-->
  <Dialog
    header="Quá trình đào tạo"
    v-model:visible="displayDialog2"
    :style="{ width: '60vw' }"
    :maximizable="true"
    :closable="true"
    style="z-index: 9001"
  >
    <form @submit.prevent="" name="submitform">
      <div class="grid formgrid m-2">
        <div class="col-12 md:col-12">
          <div class="form-group">
            <label>Văn bằng:</label>
          </div>
        </div>
        <div class="col-3 md:col-3">
          <div class="form-group">
            <label>Từ tháng, năm</label>
            <!-- <Calendar
              v-model="modeldetail.start_date"
              :showIcon="false"
              view="month"
              dateFormat="mm/yy"
              class="ip36"
              placeholder="mm/yyyy"
            /> -->
            <InputText
              v-model="modeldetail.start_date"
              class="ip36"
              maxLength="250"
            />
          </div>
        </div>
        <div class="col-3 md:col-3">
          <div class="form-group">
            <label>Đến tháng, năm</label>
            <!-- <Calendar
              v-model="model.end_date"
              :showIcon="false"
              view="month"
              dateFormat="mm/yy"
              class="ip36"
              placeholder="mm/yyyy"
            /> -->
            <InputText
              v-model="modeldetail.end_date"
              class="ip36"
              maxLength="250"
            />
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Trình độ chuyên môn</label>
            <Dropdown
              :showClear="true"
              :options="props.dictionarys[6]"
              optionLabel="academic_level_name"
              optionValue="academic_level_id"
              placeholder="Chọn trình độ"
              class="ip36"
              v-model="modeldetail.academic_level_id"
              :style="{
                whiteSpace: 'nowrap',
                overflow: 'hidden',
                textOverflow: 'ellipsis',
              }"
            />
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Chuyên ngành</label>
            <Dropdown
              :showClear="true"
              :filter="true"
              :options="props.dictionarys[18]"
              optionLabel="specialization_name"
              optionValue="specialization_id"
              placeholder="Chọn chuyên ngành"
              v-model="modeldetail.specialized"
              class="ip36"
              :style="{
                whiteSpace: 'nowrap',
                overflow: 'hidden',
                textOverflow: 'ellipsis',
              }"
            />
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Nơi đào tạo</label>
            <Dropdown
              :showClear="true"
              :editable="true"
              :filter="true"
              :options="props.dictionarys[27]"
              optionLabel="learning_place_name"
              optionValue="learning_place_name"
              placeholder="Chọn nơi đào tạo"
              class="ip36"
              v-model="modeldetail.university_name"
              maxLength="250"
              :style="{
                whiteSpace: 'nowrap',
                overflow: 'hidden',
                textOverflow: 'ellipsis',
              }"
            />
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Hệ đào tạo</label>
            <Dropdown
              :showClear="true"
              :options="props.dictionarys[12]"
              optionLabel="form_traning_name"
              optionValue="form_traning_id"
              placeholder="Chọn hệ đào tạo"
              v-model="modeldetail.form_traning_id"
              class="ip36"
              :style="{
                whiteSpace: 'nowrap',
                overflow: 'hidden',
                textOverflow: 'ellipsis',
              }"
            />
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Năm tốt nghiệp</label>
            <!-- <Calendar
              v-model="modeldetail.graduation_year"
              :showIcon="false"
              view="year"
              dateFormat="yy"
              class="ip36"
              placeholder="yyyy"
            /> -->
            <InputText
              v-model="modeldetail.graduation_year"
              class="ip36"
              maxLength="250"
            />
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Xếp loại</label>
            <Dropdown
              :showClear="true"
              :editable="true"
              :filter="true"
              :options="[
                { value: 1, title: 'Xuất sắc' },
                { value: 2, title: 'Giỏi' },
                { value: 3, title: 'Khá' },
                { value: 4, title: 'TB Khá' },
                { value: 5, title: 'Trung bình' },
              ]"
              optionLabel="title"
              optionValue="title"
              placeholder="Chọn xếp loại"
              class="ip36"
              v-model="modeldetail.rating"
              maxLength="250"
              :style="{
                whiteSpace: 'nowrap',
                overflow: 'hidden',
                textOverflow: 'ellipsis',
              }"
            />
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Ngày cấp bằng</label>
            <!-- <Calendar
              class="ip36"
              id="icon"
              v-model="modeldetail.degree_date"
              :showIcon="true"
              placeholder="dd/mm/yyyy"
            /> -->
            <InputText
              v-model="modeldetail.degree_date"
              class="ip36"
              maxLength="250"
            />
          </div>
        </div>
        <div class="col-6 md:col-6 format-center">
          <div class="form-group m-0">
            <div
              class="field-checkbox flex justify-content-center"
              style="height: 100%"
            >
              <InputSwitch v-model="modeldetail.is_man_degree" />
              <label for="binary">Bằng cấp chính</label>
            </div>
          </div>
        </div>
        <div class="col-12 md:col-12">
          <div class="form-group">
            <label>Chứng chỉ:</label>
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Chứng chỉ</label>
            <Dropdown
              :showClear="true"
              :options="props.dictionarys[13]"
              optionLabel="certificate_name"
              optionValue="certificate_id"
              placeholder="Chọn văn bằng"
              v-model="modeldetail.certificate_id"
              class="ip36"
              :style="{
                whiteSpace: 'nowrap',
                overflow: 'hidden',
                textOverflow: 'ellipsis',
              }"
            />
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Số hiệu</label>
            <InputText
              v-model="modeldetail.certificate_key_code"
              spellcheck="false"
              type="text"
              class="ip36"
              maxLength="50"
            />
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Phiên bản</label>
            <InputText
              v-model="modeldetail.certificate_version"
              spellcheck="false"
              type="text"
              class="ip36"
              maxLength="25"
            />
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Lần phát hành</label>
            <InputText
              v-model="modeldetail.certificate_release_time"
              spellcheck="false"
              type="text"
              class="ip36"
              maxLength="25"
            />
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Ngày hiệu lực</label>
            <Calendar
              v-model="modeldetail.certificate_start_date"
              :showIcon="false"
              class="ip36"
              placeholder="dd/mm/yyyy"
            />
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Ngày hết hiệu lực</label>
            <Calendar
              v-model="modeldetail.certificate_end_date"
              :showIcon="false"
              class="ip36"
              placeholder="dd/mm/yyyy"
            />
          </div>
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="closeDialog()"
        class="p-button-text"
      />
      <Button
        label="Lưu và tiếp tục"
        icon="pi pi-check"
        @click="saveRow(2, true)"
      />
      <Button label="Lưu" icon="pi pi-check" @click="saveRow(2)" />
    </template>
  </Dialog>
  <!--Dialog 4-->
  <Dialog
    header="Quá trình công tác trước khi vào đơn vị"
    v-model:visible="displayDialog4"
    :style="{ width: '60vw' }"
    :maximizable="true"
    :closable="true"
    style="z-index: 9001"
  >
    <form @submit.prevent="" name="submitform">
      <div class="grid formgrid m-2">
        <div class="col-3 md:col-3">
          <div class="form-group">
            <label>Từ tháng, năm</label>
            <!-- <Calendar
              v-model="modeldetail.start_date"
              :showIcon="false"
              view="month"
              dateFormat="mm/yy"
              class="ip36"
              placeholder="mm/yyyy"
            /> -->
            <InputText
              v-model="modeldetail.start_date"
              class="ip36"
              maxLength="250"
            />
          </div>
        </div>
        <div class="col-3 md:col-3">
          <div class="form-group">
            <label>Đến tháng, năm</label>
            <!-- <Calendar
              v-model="modeldetail.end_date"
              :showIcon="false"
              view="month"
              dateFormat="mm/yy"
              class="ip36"
              placeholder="mm/yyyy"
            /> -->
            <InputText
              v-model="modeldetail.end_date"
              class="ip36"
              maxLength="250"
            />
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Công ty, đơn vị</label>
            <InputText
              v-model="modeldetail.company"
              spellcheck="false"
              type="text"
              class="ip36"
              maxLength="250"
            />
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Địa chỉ</label>
            <InputText
              v-model="modeldetail.address"
              spellcheck="false"
              type="text"
              class="ip36"
              maxLength="500"
            />
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Vị trí</label>
            <InputText
              v-model="modeldetail.role"
              spellcheck="false"
              type="text"
              class="ip36"
              maxLength="50"
            />
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Mức lương</label>
            <InputText
              v-model="modeldetail.wage"
              spellcheck="false"
              type="text"
              class="ip36"
              maxLength="500"
            />
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Người tham chiếu</label>
            <InputText
              v-model="modeldetail.reference_name"
              spellcheck="false"
              type="text"
              class="ip36"
              maxLength="50"
            />
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Điện thoại</label>
            <InputMask
              v-model="modeldetail.reference_phone"
              mask="9999999999"
              placeholder="__________"
              class="ip36"
            />
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Mô tả công việc</label>
            <InputText
              v-model="modeldetail.description"
              spellcheck="false"
              type="text"
              class="ip36"
              maxLength="500"
            />
          </div>
        </div>
        <div class="col-12 md:col-12">
          <div class="form-group">
            <label>Lý do nghỉ việc</label>
            <InputText
              v-model="modeldetail.reason"
              spellcheck="false"
              type="text"
              class="ip36"
              maxLength="500"
            />
          </div>
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="closeDialog()"
        class="p-button-text"
      />
      <Button
        label="Lưu và tiếp tục"
        icon="pi pi-check"
        @click="saveRow(4, true)"
      />
      <Button label="Lưu" icon="pi pi-check" @click="saveRow(4)" />
    </template>
  </Dialog>
</template>
<style scoped>
@import url(../../profile/component/stylehrm.css);
.profile-history {
  cursor: pointer;
  box-shadow: none;
}
.profile-history-active {
  box-shadow: 0 0 0 2px #fbc02d, inset 0 0 0 0px #fff !important;
}

.profile-history:hover {
  box-shadow: 5px 5px 10px #0000003b !important;
}
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
::v-deep(.p-timeline) {
  .p-timeline-event .p-timeline-event-opposite {
    display: none !important;
  }
  .p-timeline-event:nth-child(even) {
    flex-direction: row;
  }
  .p-timeline-event .p-timeline-event-content .p-card-content {
    padding: 1rem !important;
  }
}
::v-deep(.border-radius) {
  img {
    border-radius: 50%;
  }
}
</style>
