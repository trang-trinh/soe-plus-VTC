<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { encr } from "../../../util/function";
const cryoptojs = inject("cryptojs");
import moment from "moment";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import treeuser from "../../../components/user/treeuser.vue";
import { de } from "date-fns/locale";
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
const ruleprocedure = {
  procedureform_name: {
    required,
    $errors: [
      {
        $property: "procedureform_name",
        $validator: "required",
        $message: "Tên quy trình không được để trống!",
      },
    ],
  },
};
const rulesign = {
  signform_name: {
    required,
    $errors: [
      {
        $property: "signform_name",
        $validator: "required",
        $message: "Tên nhóm duyệt không được để trống!",
      },
    ],
  },
};
const toast = useToast();

//Declare
const isFirst = ref(true);
const procedures = ref([]);
const signs = ref([]);
const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);
const options = ref({
  loading: false,
  user_id: store.getters.user.user_id,
  searchprocedure: "",
  searchsign: "",
  totalprocedure: 0,
  totalsign: 0,
  sort: "created_date desc",
  orderBy: "desc",
  procedureform_id: null,
});
const types = ref([
  { value: 0, title: "Duyệt tuần tự" },
  { value: 1, title: "Duyệt một trong nhiều" },
  //{ value: 2, title: "Duyệt ngẫu nhiên" },
]);
const selectedKeyProcedure = ref([]);
const selectedKeySign = ref([]);

watch(selectedKeyProcedure, () => {
  goProcedure(selectedKeyProcedure.value);
});

//Model procedure
const modelprocedure = ref({});
const vp$ = useVuelidate(ruleprocedure, modelprocedure);

//Model procedure
const modelsign = ref({});
const vs$ = useVuelidate(rulesign, modelsign);

//Function

//Function add
const isAdd = ref(false);
const submitted = ref(false);
const headerDialogProcedure = ref();
const headerDialogSign = ref();
const displayDialogProcedure = ref(false);
const displayDialogSign = ref(false);

const openAddDialogProcedure = (str) => {
  isAdd.value = true;
  submitted.value = false;
  modelprocedure.value = {
    status: true,
    is_type: 0,
    is_order: 0,
  };
  if (options.value.totalprocedure > 0) {
    modelprocedure.value.is_order = options.value.totalprocedure + 1;
  } else {
    modelprocedure.value.is_order = 1;
  }
  headerDialogProcedure.value = str;
  displayDialogProcedure.value = true;
};
const openAddDialogSign = (str) => {
  isAdd.value = true;
  submitted.value = false;
  modelsign.value = {
    procedureform_id: options.value.procedureform_id,
    status: true,
    is_type: 0,
    is_step: 0,
    users: [],
  };
  if (options.value.totalsign > 0) {
    modelsign.value.is_step = options.value.totalsign + 1;
  } else {
    modelsign.value.is_step = 1;
  }
  headerDialogSign.value = str;
  displayDialogSign.value = true;
};
const closeDialogProcedure = () => {
  modelprocedure.value = {
    status: true,
    is_type: 0,
    is_order: 0,
  };
  displayDialogProcedure.value = false;
};
const closeDialogSign = () => {
  modelsign.value = {
    status: true,
    is_type: 0,
    is_step: 0,
    users: [],
  };
  displayDialogSign.value = false;
};
const saveModelProcedure = (isFormValid) => {
  submitted.value = true;
  if (!isFormValid) {
    swal.fire({
      title: "Thông báo!",
      text: "Vui lòng điền đầy đủ thông tin trường bôi đỏ!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  if (modelprocedure.value.procedureform_name.length > 500) {
    swal.fire({
      title: "Thông báo!",
      text: "Tên quy trình không vượt quá 500 ký tự!",
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

  let formData = new FormData();
  formData.append("isAdd", isAdd.value);
  formData.append("model", JSON.stringify(modelprocedure.value));
  axios
    .put(
      baseURL + "/api/calendar_dutyconfig/update_duty_procedureform",
      formData,
      config,
    )
    .then((response) => {
      if (response.data.err === "1") {
        swal.close();
        if (options.value.loading) options.value.loading = false;
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
        isAdd.value
          ? "Thêm quy trình thành công!"
          : "Cập nhật quy trình thành công!",
      );
      initProcedure(true);
      closeDialogProcedure();
    })
    .catch((error) => {
      swal.close();
      swal.fire({
        title: "Thông báo!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
      return;
    });
  if (submitted.value) submitted.value = true;
};
const saveModelSign = (isFormValid) => {
  submitted.value = true;
  if (!isFormValid) {
    swal.fire({
      title: "Thông báo!",
      text: "Vui lòng điền đầy đủ thông tin trường bôi đỏ!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  if (modelsign.value.signform_name.length > 500) {
    swal.fire({
      title: "Thông báo!",
      text: "Tên nhóm duyệt không vượt quá 500 ký tự!",
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

  var obj = { ...modelsign.value };
  var us = obj.users.map((x) => x["user_id"]);

  let formData = new FormData();
  formData.append("isAdd", isAdd.value);
  formData.append("model", JSON.stringify(obj));
  formData.append("users", JSON.stringify(us));
  axios
    .put(
      baseURL + "/api/calendar_dutyconfig/update_duty_signform",
      formData,
      config
    )
    .then((response) => {
      if (response.data.err === "1") {
        swal.close();
        if (options.value.loading) options.value.loading = false;
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
        isAdd.value
          ? "Thêm nhóm duyệt thành công!"
          : "Cập nhật nhóm duyệt thành công!",
      );
      initSign(true);
      closeDialogSign();
    })
    .catch((error) => {
      swal.close();
      swal.fire({
        title: "Thông báo!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
  if (submitted.value) submitted.value = true;
};
const goProcedure = (node) => {
  options.value.procedureform_id = node["procedureform_id"];
  initSign(true);
};
const editProcedure = (item) => {
  submitted.value = false;
  options.value.loading = true;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  isAdd.value = false;
  axios
    .post(
      baseURL + "/api/calendar/get_datas",
      {
        str: encr(
          JSON.stringify({
            proc: "calendar_duty_procedureform_get",
            par: [
              { par: "user_id", va: store.state.user.user_id },
              { par: "procedureform_id", va: item.procedureform_id },
            ],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      if (response != null && response.data != null) {
        var data = response.data.data;
        if (data != null) {
          var tbs = JSON.parse(data);
          modelprocedure.value = tbs[0][0];
        }
      }
      swal.close();
      if (options.value.loading) options.value.loading = false;

      headerDialogProcedure.value = "Cập nhật quy trình";
      displayDialogProcedure.value = true;
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      if (options.value.loading) options.value.loading = false;
      addLog({
        title: "Lỗi Console editItem",
        controller: "boardroom.vue",
        logcontent: error.message,
        loai: 2,
      });
      if (error && error.status === 401) {
        swal.fire({
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
        return;
      }
    });
};
const editSign = (item) => {
  submitted.value = false;
  options.value.loading = true;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  isAdd.value = false;
  axios
    .post(
      baseURL + "/api/calendar/get_datas",
      {
        str: encr(
          JSON.stringify({
            proc: "calendar_duty_signform_get",
            par: [
              { par: "user_id", va: store.state.user.user_id },
              { par: "sign_id", va: item.signform_id },
            ],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      if (response != null && response.data != null) {
        var data = response.data.data;
        if (data != null) {
          var tbs = JSON.parse(data);
          modelsign.value = tbs[0][0];
          modelsign.value.users = tbs[1];
        }
      }
      swal.close();
      if (options.value.loading) options.value.loading = false;

      headerDialogSign.value = "Cập nhật nhóm duyệt";
      displayDialogSign.value = true;
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      if (options.value.loading) options.value.loading = false;
      addLog({
        title: "Lỗi Console editItem",
        controller: "boardroom.vue",
        logcontent: error.message,
        loai: 2,
      });
      if (error && error.status === 401) {
        swal.fire({
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const updateStatusProcedure = (item) => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  let data = { id: item.procedureform_id, status: !(item.status || false) };
  axios
    .put(
      baseURL + "/api/calendar_dutyconfig/update_status_duty_procedureform",
      data,
      config,
    )
    .then((response) => {
      if (response.data.err === "1") {
        swal.close();
        swal.fire({
          title: "Thông báo!",
          text: response.data.ms,
          icon: "error",
          confirmButtonText: "OK",
        });
        return;
      } else {
        swal.close();
        toast.success("Cập nhật trạng thái thành công!");
        initProcedure(true);
      }
    })
    .catch((error) => {
      swal.close();
      swal.fire({
        title: "Thông báo!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};
const updateStatusSign = (item) => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  let data = { id: item.signform_id, status: !(item.status || false) };
  axios
    .put(
      baseURL + "/api/calendar_dutyconfig/update_status_duty_signform",
      data,
      config
    )
    .then((response) => {
      if (response.data.err === "1") {
        swal.close();
        swal.fire({
          title: "Thông báo!",
          text: response.data.ms,
          icon: "error",
          confirmButtonText: "OK",
        });
        return;
      } else {
        swal.close();
        toast.success("Cập nhật trạng thái thành công!");
        initSign(true);
      }
    })
    .catch((error) => {
      swal.close();
      swal.fire({
        title: "Thông báo!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
      return;
    });
};
const deleteProcedure = (item) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá quy trình này không!",
      icon: "warning",
      showCancelButton: true,
      confirmButtonColor: "#3085d6",
      cancelButtonColor: "#d33",
      confirmButtonText: "Có",
      cancelButtonText: "Không",
    })
    .then((result) => {
      if (result.isConfirmed) {
        options.value.loading = true;
        swal.fire({
          width: 110,
          didOpen: () => {
            swal.showLoading();
          },
        });
        var ids = [item["procedureform_id"]];
        axios
          .delete(
            baseURL + "/api/calendar_dutyconfig/delete_duty_procedureform",
            {
              headers: { Authorization: `Bearer ${store.getters.token}` },
              data: ids,
            }
          )
          .then((response) => {
            if (response.data.err === "1") {
              swal.close();
              if (options.value.loading) options.value.loading = false;
              swal.fire({
                title: "Thông báo!",
                text: response.data.ms,
                icon: "error",
                confirmButtonText: "OK",
              });
              return;
            }
            //initData(true);
            if (ids.length > 0) {
              ids.forEach((element, i) => {
                var idx = procedures.value.findIndex(
                  (x) => x.procedureform_id == element,
                );
                if (idx != -1) {
                  procedures.value.splice(idx, 1);
                }
              });
            }
            swal.close();
            toast.success("Xoá quy trình thành công!");
            if (options.value.loading) options.value.loading = false;
          })
          .catch((error) => {
            swal.close();
            if (options.value.loading) options.value.loading = false;
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
      }
    });
};
const deleteSign = (item) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá nhóm duyệt này không!",
      icon: "warning",
      showCancelButton: true,
      confirmButtonColor: "#3085d6",
      cancelButtonColor: "#d33",
      confirmButtonText: "Có",
      cancelButtonText: "Không",
    })
    .then((result) => {
      if (result.isConfirmed) {
        options.value.loading = true;
        swal.fire({
          width: 110,
          didOpen: () => {
            swal.showLoading();
          },
        });
        var ids = [item["signform_id"]];
        axios
          .delete(baseURL + "/api/calendar_dutyconfig/delete_duty_signform", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: ids,
          })
          .then((response) => {
            if (response.data.err === "1") {
              swal.close();
              if (options.value.loading) options.value.loading = false;
              swal.fire({
                title: "Thông báo!",
                text: response.data.ms,
                icon: "error",
                confirmButtonText: "OK",
              });
              return;
            }
            //initData(true);
            if (ids.length > 0) {
              ids.forEach((element, i) => {
                var idx = signs.value.findIndex(
                  (x) => x.signform_id == element,
                );
                if (idx != -1) {
                  signs.value.splice(idx, 1);
                }
              });
            }
            swal.close();
            toast.success("Xoá nhóm duyệt thành công!");
            if (options.value.loading) options.value.loading = false;
          })
          .catch((error) => {
            swal.close();
            if (options.value.loading) options.value.loading = false;
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
      }
    });
};
const removeUser = (item, us) => {
  var idx = us.findIndex((x) => x["user_id"] === item["user_id"]);
  if (idx != -1) {
    us.splice(idx, 1);
  }
};

//Function choice user
// reload component
const componentKey = ref(0);
const forceRerender = () => {
  componentKey.value += 1;
};
const selectedUser = ref([]);
const is_one = ref(false);
const is_type = ref();
const headerDialogUser = ref();
const displayDialogUser = ref(false);
const showModalUser = (one, type) => {
  if (type != null) {
    switch (type) {
      case 0:
        selectedUser.value = [...modelsign.value.users];
        headerDialogUser.value = "Chọn người duyệt";
        break;
      default:
        break;
    }
  }

  is_one.value = one;
  is_type.value = type;
  displayDialogUser.value = true;
  forceRerender();
};
const choiceUser = () => {
  if (is_type.value != null) {
    switch (is_type.value) {
      case 0:
        var notexist = selectedUser.value.filter(
          (a) =>
            modelsign.value.users.findIndex(
              (b) => b["user_id"] === a["user_id"]
            ) === -1
        );
        if (notexist.length > 0) {
          modelsign.value.users = modelsign.value.users.concat(notexist);
        }
        break;
      default:
        break;
    }
  }
  closeDialogUser();
};
const closeDialogUser = () => {
  displayDialogUser.value = false;
};

//Function filter
const searchProcedure = () => {
  initProcedure(true);
};
const searchSign = () => {
  initSign(true);
};

//init
const refreshProcedure = () => {
  options.value.searchprocedure = "";
  initProcedure(true);
};
const refreshSign = () => {
  options.value.searchsign = "";
  initSign(true);
};
const initProcedure = (rf) => {
  if (rf) {
    options.value.loading = true;
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
  }
  if (isDynamicSQL.value) {
    initDataSQL();
    return;
  }

  axios
    .post(
      baseURL + "/api/calendar/get_datas",
      {
        str: encr(
          JSON.stringify({
            proc: "calendar_duty_procedureform_list",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "search", va: options.value.searchprocedure },
            ],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      if (response != null && response.data != null) {
        var data = response.data.data;
        if (data != null) {
          let tbs = JSON.parse(data);
          if (tbs[0] != null && tbs[0].length > 0) {
            procedures.value = tbs[0];
            if (procedures.value.length > 0) {
              procedures.value.forEach((element, i) => {
                element["STT"] = i + 1;
                var idx = types.value.findIndex(
                  (x) => x["value"] === element["is_type"]
                );
                if (idx !== -1) {
                  element["type_name"] = types.value[idx]["title"];
                } else {
                  element["type_name"] = "Chưa xác định";
                }
              });
            }
          } else {
            procedures.value = [];
          }
          if (tbs.length == 2) {
            options.value.totalprocedure = tbs[1][0].total;
          }
        }
      }
      swal.close();
      if (isFirst.value) isFirst.value = false;
      if (options.value.loading) options.value.loading = false;
    })
    .catch((error) => {
      if (options.value.loading) options.value.loading = false;
      swal.close();
      swal.fire({
        title: "Thông báo!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
      return;
    });
};
const initSign = (rf) => {
  if (rf) {
    options.value.loading = true;
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
  }
  axios
    .post(
      baseURL + "/api/calendar/get_datas",
      {
        str: encr(
          JSON.stringify({
            proc: "calendar_duty_signform_list",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "search", va: options.value.searchsign },
              { par: "procedureform_id", va: options.value.procedureform_id },
            ],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      if (response != null && response.data != null) {
        var data = response.data.data;
        if (data != null) {
          let tbs = JSON.parse(data);
          if (tbs[0] != null && tbs[0].length > 0) {
            signs.value = tbs[0];
            if (signs.value.length > 0) {
              signs.value.forEach((element, i) => {
                element["STT"] = i + 1;
                var idx = types.value.findIndex(
                  (x) => x["value"] === element["is_type"]
                );
                if (idx !== -1) {
                  element["type_name"] = types.value[idx]["title"];
                } else {
                  element["type_name"] = "Chưa xác định";
                }
                if (element["signusers"] != null) {
                  element["signusers"] = JSON.parse(element["signusers"]);
                }
              });
            }
          } else {
            signs.value = [];
          }
          if (tbs.length == 2) {
            options.value.totalsign = tbs[1][0].total;
          }
        }
      }
      swal.close();
      if (isFirst.value) isFirst.value = false;
      if (options.value.loading) options.value.loading = false;
    })
    .catch((error) => {
      if (options.value.loading) options.value.loading = false;
      swal.close();
      swal.fire({
        title: "Thông báo!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
      return;
    });
};

onMounted(() => {
  initProcedure(true);
});

emitter.on("emitData", (obj) => {
  if (obj.type != null) {
    switch (obj.type) {
      case "choiceusers":
        displayDialogUser.value = obj.data["displayDialog"];
        if (obj.data["submit"]) {
          choiceUser();
        }
        break;
      default:
        break;
    }
  }
});
</script>
<template>
  <div class="surface-100 p-3 calendar">
    <Splitter style="height: 100%">
      <SplitterPanel :size="50" :minSize="35">
        <Toolbar class="outline-none surface-0 border-none">
          <template #start>
            <span class="p-input-icon-left">
              <i class="pi pi-search" />
              <InputText
                @keypress.enter="searchProcedure()"
                v-model="options.searchprocedure"
                type="text"
                spellcheck="false"
                placeholder=" Tìm kiếm tên quy trình"
              />
            </span>
          </template>
          <template #end>
            <Button
              @click="openAddDialogProcedure('Thêm mới quy trình')"
              label="Thêm mới"
              icon="pi pi-plus"
              class="mr-2"
            />
            <Button
              @click="refreshProcedure()"
              class="p-button-outlined p-button-secondary"
              icon="pi pi-refresh"
              v-tooltip.top="'Tải lại'"
            />
          </template>
        </Toolbar>
        <div class="d-lang-table">
          <DataTable
            @sort="onSort($event)"
            :value="procedures"
            :loading="options.loading"
            :totalRecords="options.totalprocedure"
            :lazy="true"
            :rowHover="true"
            :showGridlines="true"
            :scrollable="true"
            v-model:selection="selectedKeyProcedure"
            selectionMode="single"
            dataKey="procedureform_id"
            scrollHeight="flex"
            filterDisplay="menu"
            filterMode="lenient"
            responsiveLayout="scroll"
          >
            <Column
              field="procedureform_name"
              header="Tên quy trình"
              headerStyle="height:50px;max-width:auto;"
            >
            </Column>
            <Column
              field="type_name"
              header="Kiểu duyệt"
              headerStyle="text-align:center;max-width:150px;height:50px"
              bodyStyle="text-align:center;max-width:150px;"
              class="align-items-center justify-content-center text-center"
            >
              <template #body="slotProps">
                <div class="format-flex-center">
                  <Tag
                    class="px-3 py-1"
                    :value="slotProps.data.type_name"
                    :class="'type' + slotProps.data.is_type"
                    style="
                      font-size: 11px;
                      min-width: max-content;
                      color: #fff;
                      border-radius: 25px;
                      height: max-content;
                    "
                  ></Tag>
                </div>
              </template>
            </Column>
            <Column
              field="status"
              header="Trạng thái"
              headerStyle="text-align:center;max-width:90px;height:50px"
              bodyStyle="text-align:center;max-width:90px;"
              class="align-items-center justify-content-center text-center"
            >
              <template #body="data">
                <Checkbox
                  :binary="data.data.status"
                  v-model="data.data.status"
                  @click="updateStatusProcedure(data.data)"
                />
              </template>
            </Column>
            <Column
              header="Chức năng"
              headerStyle="text-align:center;max-width:125px;height:50px"
              bodyStyle="text-align:center;max-width:125px;"
              class="align-items-center justify-content-center text-center"
            >
              <template #body="data">
                <div v-if="data.data.isFunciton">
                  <Button
                    @click="editProcedure(data.data)"
                    class="p-button-rounded p-button-secondary p-button-outlined mx-1"
                    type="button"
                    icon="pi pi-pencil"
                    v-tooltip="'Sửa'"
                  ></Button>
                  <Button
                    @click="deleteProcedure(data.data)"
                    class="p-button-rounded p-button-secondary p-button-outlined mx-1"
                    type="button"
                    v-tooltip="'Xóa'"
                    icon="pi pi-trash"
                  ></Button>
                </div>
              </template>
            </Column>
            <template #empty>
              <div
                class="align-items-center justify-content-center p-4 text-center m-auto"
                v-if="!isFirst || options.total == 0"
                style="display: flex; height: calc(100vh - 195px)"
              >
                <div>
                  <img
                    src="../../../assets/background/nodata.png"
                    height="144"
                  />
                  <h3 class="m-1">Không có dữ liệu</h3>
                </div>
              </div>
            </template>
          </DataTable>
        </div>
      </SplitterPanel>
      <SplitterPanel :size="50" :minSize="35">
        <div v-if="options.procedureform_id != null">
          <Toolbar class="outline-none surface-0 border-none">
            <template #start>
              <span class="p-input-icon-left">
                <i class="pi pi-search" />
                <InputText
                  @keypress.enter="searchSign()"
                  v-model="options.searchsign"
                  type="text"
                  spellcheck="false"
                  placeholder=" Tìm kiếm tên nhóm"
                />
              </span>
            </template>
            <template #end>
              <Button
                @click="openAddDialogSign('Thêm mới nhóm duyệt')"
                label="Thêm mới"
                icon="pi pi-plus"
                class="mr-2"
              />
              <Button
                @click="refreshSign()"
                class="p-button-outlined p-button-secondary"
                icon="pi pi-refresh"
                v-tooltip.top="'Tải lại'"
              />
            </template>
          </Toolbar>
          <div class="d-lang-table">
            <DataTable
              @sort="onSort($event)"
              :value="signs"
              :loading="options.loading"
              :totalRecords="options.total"
              :lazy="true"
              :rowHover="true"
              :showGridlines="true"
              :scrollable="true"
              v-model:selection="selectedNodes"
              dataKey="signform_id"
              scrollHeight="flex"
              filterDisplay="menu"
              filterMode="lenient"
              responsiveLayout="scroll"
            >
              <Column
                field="signform_name"
                header="Tên nhóm"
                headerStyle="height:50px;max-width:auto;"
              >
                <template #body="slotProps">
                  <div>
                    <div class="mb-2">{{ slotProps.data.signform_name }}</div>
                    <div>
                      <AvatarGroup
                        v-if="
                          slotProps.data.signusers &&
                          slotProps.data.signusers.length > 0
                        "
                      >
                        <Avatar
                          v-for="(
                            item, index
                          ) in slotProps.data.signusers.slice(0, 3)"
                          v-bind:label="
                            item.avatar ? '' : item.last_name.substring(0, 1)
                          "
                          v-bind:image="
                            item.avatar
                              ? basedomainURL + item.avatar
                              : basedomainURL + '/Portals/Image/noimg.jpg'
                          "
                          v-tooltip.bottom="
                            item.full_name +
                            '<br>' +
                            item.position_name +
                            '<br>' +
                            item.department_name
                          "
                          :key="item.user_id"
                          style="
                            border: 2px solid white;
                            color: white;
                            width: 2.5rem;
                            height: 2.5rem;
                          "
                          @click="onTaskUserFilter(item)"
                          @error="basedomainURL + '/Portals/Image/noimg.jpg'"
                          size="large"
                          shape="circle"
                          class="cursor-pointer"
                          :style="{ backgroundColor: bgColor[index % 7] }"
                        />
                        <Avatar
                          v-if="
                            slotProps.data.signusers &&
                            slotProps.data.signusers.length > 3
                          "
                          v-bind:label="
                            '+' +
                            (slotProps.data.signusers.length - 3).toString()
                          "
                          shape="circle"
                          size="large"
                          style="background-color: #2196f3; color: #ffffff"
                          class="cursor-pointer"
                        />
                      </AvatarGroup>
                    </div>
                  </div>
                </template>
              </Column>
              <Column
                field="type_name"
                header="Kiểu duyệt"
                headerStyle="text-align:center;max-width:150px;height:50px"
                bodyStyle="text-align:center;max-width:150px;"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="slotProps">
                  <div class="format-flex-center">
                    <Tag
                      class="px-3 py-1"
                      :value="slotProps.data.type_name"
                      :class="'type' + slotProps.data.is_type"
                      style="
                        font-size: 11px;
                        min-width: max-content;
                        color: #fff;
                        border-radius: 25px;
                        height: max-content;
                      "
                    ></Tag>
                  </div>
                </template>
              </Column>
              <Column
                field="status"
                header="Trạng thái"
                headerStyle="text-align:center;max-width:90px;height:50px"
                bodyStyle="text-align:center;max-width:90px;"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="data">
                  <Checkbox
                    :binary="data.data.status"
                    v-model="data.data.status"
                    @click="updateStatusSign(data.data)"
                  />
                </template>
              </Column>
              <Column
                header="Chức năng"
                headerStyle="text-align:center;max-width:125px;height:50px"
                bodyStyle="text-align:center;max-width:125px;"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="data">
                  <div v-if="data.data.isFunciton">
                    <Button
                      @click="editSign(data.data)"
                      class="p-button-rounded p-button-secondary p-button-outlined mx-1"
                      type="button"
                      icon="pi pi-pencil"
                      v-tooltip="'Sửa'"
                    ></Button>
                    <Button
                      @click="deleteSign(data.data)"
                      class="p-button-rounded p-button-secondary p-button-outlined mx-1"
                      type="button"
                      v-tooltip="'Xóa'"
                      icon="pi pi-trash"
                    ></Button>
                  </div>
                </template>
              </Column>
              <template #empty>
                <div
                  class="align-items-center justify-content-center p-4 text-center m-auto"
                  v-if="!isFirst || options.total == 0"
                  style="display: flex; height: calc(100vh - 195px)"
                >
                  <div>
                    <img
                      src="../../../assets/background/nodata.png"
                      height="144"
                    />
                    <h3 class="m-1">Không có dữ liệu</h3>
                  </div>
                </div>
              </template>
            </DataTable>
          </div>
        </div>
        <div
          v-else
          class="flex align-items-center justify-content-center"
          style="height: 100%"
        >
          <h3 class="m-1">Bạn chưa chọn quy trình</h3>
        </div>
      </SplitterPanel>
    </Splitter>
  </div>

  <!--Dialog-->
  <Dialog
    :header="headerDialogProcedure"
    v-model:visible="displayDialogProcedure"
    :style="{ width: '40vw' }"
    :closable="true"
    style="z-index: 1000"
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="col-12 md:col-12">
          <div class="form-group">
            <label class="col-3 text-left p-0"
              >Tên quy trình <span class="redsao">(*)</span></label
            >
            <InputText
              v-model="modelprocedure.procedureform_name"
              spellcheck="false"
              class="col-8 ip36 px-2"
              :class="{
                'p-invalid': vp$.procedureform_name.$invalid && submitted,
              }"
            />
            <div
              v-if="
                (vp$.procedureform_name.$invalid && submitted) ||
                vp$.procedureform_name.$pending.$response
              "
            >
              <small class="p-error">
                <span class="col-12 p-0">{{
                  vp$.procedureform_name.required.$message
                    .replace("Value", "Tên quy trình")
                    .replace("is required", "không được để trống")
                }}</span>
              </small>
            </div>
          </div>
        </div>
        <div class="col-12 md:col-12">
          <div class="form-group">
            <label>Kiểu duyệt</label>
            <Dropdown
              :options="types"
              :filter="true"
              :showClear="false"
              :editable="false"
              v-model="modelprocedure.is_type"
              optionLabel="title"
              optionValue="value"
              placeholder="Chọn kiểu duyệt"
              class="ip36"
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
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Số thứ tự </label>
            <InputText v-model="modelprocedure.is_order" class="col-6 ip36" />
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group" style="height: 100%">
            <div
              class="field-checkbox flex justify-content-center"
              style="height: 100%"
            >
              <InputSwitch v-model="modelprocedure.status" />
              <label for="binary">Kích hoạt</label>
            </div>
          </div>
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="closeDialogProcedure()"
        class="p-button-text"
      />

      <Button
        label="Lưu"
        icon="pi pi-check"
        @click="saveModelProcedure(!vp$.$invalid)"
      />
    </template>
  </Dialog>

  <Dialog
    :header="headerDialogSign"
    v-model:visible="displayDialogSign"
    :style="{ width: '40vw' }"
    :closable="true"
    style="z-index: 1000"
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="col-12 md:col-12">
          <div class="form-group">
            <label class="col-3 text-left p-0"
              >Tên nhóm duyệt <span class="redsao">(*)</span></label
            >
            <InputText
              v-model="modelsign.signform_name"
              spellcheck="false"
              class="col-8 ip36 px-2"
              :class="{
                'p-invalid': vs$.signform_name.$invalid && submitted,
              }"
            />
            <div
              v-if="
                (vs$.signform_name.$invalid && submitted) ||
                vs$.signform_name.$pending.$response
              "
            >
              <small class="p-error">
                <span class="col-12 p-0">{{
                  vs$.signform_name.required.$message
                    .replace("Value", "Tên nhóm duyệt")
                    .replace("is required", "không được để trống")
                }}</span>
              </small>
            </div>
          </div>
        </div>
        <div class="col-12 md:col-12">
          <div class="form-group">
            <label>Kiểu duyệt</label>
            <Dropdown
              :options="types"
              :filter="true"
              :showClear="false"
              :editable="false"
              v-model="modelsign.is_type"
              optionLabel="title"
              optionValue="value"
              placeholder="Chọn kiểu duyệt"
              class="ip36"
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
        <div class="col-12 md:col-12">
          <div class="form-group">
            <label
              >Người duyệt
              <i
                class="pi pi-user-plus ml-2"
                v-tooltip.top="'Chọn người dùng'"
                @click="showModalUser(false, 0)"
                style="cursor: pointer; color: #2196f3"
              ></i>
            </label>
            <OrderList
              v-model="modelsign.users"
              listStyle="height:auto"
              dataKey="user_id"
            >
              <template #header> Danh sách </template>
              <template #item="slotProps">
                <div class="flex">
                  <div class="format-flex-center">
                    <b class="p-3">{{ slotProps.index + 1 }}</b>
                  </div>
                  <div class="image-container pl-3 pr-2">
                    <Avatar
                      v-bind:label="
                        slotProps.item.avatar
                          ? ''
                          : slotProps.item.last_name.substring(0, 1)
                      "
                      v-bind:image="
                        slotProps.item.avatar
                          ? basedomainURL + slotProps.item.avatar
                          : basedomainURL + '/Portals/Image/noimg.jpg'
                      "
                      :key="slotProps.item.user_id"
                      style="border: 2px solid white; color: white"
                      @error="basedomainURL + '/Portals/Image/noimg.jpg'"
                      size="large"
                      shape="circle"
                      class="cursor-pointer bg-blue-200"
                    />
                  </div>
                  <div
                    class="format-grid-center justify-content-start"
                    style="flex: 1"
                  >
                    <span class="text-left">{{
                      slotProps.item.full_name
                    }}</span>
                    <span class="text-left" style="color: #aaa">{{
                      slotProps.item.position_name
                    }}</span>
                  </div>
                  <div class="format-flex-center">
                    <a @click="removeUser(slotProps.item, modelsign.users)">
                      <i class="pi pi-trash"></i>
                    </a>
                  </div>
                </div>
              </template>
            </OrderList>
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Thứ tự duyệt </label>
            <InputText v-model="modelsign.is_step" class="col-6 ip36" />
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group" style="height: 100%">
            <div
              class="field-checkbox flex justify-content-center"
              style="height: 100%"
            >
              <InputSwitch v-model="modelsign.status" />
              <label for="binary">Kích hoạt</label>
            </div>
          </div>
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="closeDialogSign()"
        class="p-button-text"
      />

      <Button
        label="Lưu"
        icon="pi pi-check"
        @click="saveModelSign(!vs$.$invalid)"
      />
    </template>
  </Dialog>

  <treeuser
    :key="componentKey"
    :headerDialog="headerDialogUser"
    :displayDialog="displayDialogUser"
    :closeDialog="closeDialogUser"
    :one="is_one"
    :selected="selectedUser"
    :choiceUser="choiceUser"
  />
  <!--Dialog-->
</template>
<style scoped>
.d-lang-table {
  height: calc(100vh - 130px);
  overflow-y: auto;
}
.inputanh {
  border: 1px solid #ccc;
  width: 96px;
  height: 96px;
  cursor: pointer;
  padding: 1px;
}
.ipnone {
  display: none;
}
.inputanh img {
  object-fit: cover;
  width: 100%;
  height: 100%;
}
.format-flex-center {
  display: flex;
  align-items: center;
  justify-content: center;
  text-align: center;
}
.format-grid-center {
  display: grid;
  align-items: center;
  justify-content: center;
  text-align: center;
}
.style-day {
  line-height: 1.8rem;
}
.style-day.false {
  color: #2196f3 !important;
}
.style-day.true {
  color: red !important;
}
.form-group {
  display: grid;
  margin-bottom: 1rem;
}
.form-group > label {
  margin-bottom: 0.5rem;
}
.ip36 {
  width: 100%;
}
.p-ulchip {
  margin: 0;
  margin-top: 0.5rem;
  padding: 0;
  list-style: none;
}
.p-lichip {
  float: left;
}
.p-multiselect-label {
  height: 100%;
  display: flex;
  align-items: center;
}
.type0 {
  background-color: #ff8b4e;
}
.type1 {
  background-color: #33c9dc;
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
</style>