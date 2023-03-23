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
  config_process_name: {
    required,
    $errors: [
      {
        $property: "config_process_name",
        $validator: "required",
        $message: "Tên quy trình không được để trống!",
      },
    ],
  },
};
const rulesign = {
  config_procerduce_name: {
    required,
    $errors: [
      {
        $property: "config_procerduce_name",
        $validator: "required",
        $message: "Tên nhóm duyệt không được để trống!",
      },
    ],
  },
};
const toast = useToast();

//Declare
const isFirst = ref(true);
const listdatas = ref([]);
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
  searchText: "",
  searchsign: "",
  totalRecords: 0,
  totalsign: 0,
  sort: "created_date desc",
  orderBy: "desc",
  config_process_id: null,
  pageno: 0,
  pagesize: 20,
});
const types = ref([
  { value: 0, title: "Duyệt tuần tự" },
  { value: 1, title: "Duyệt một trong nhiều" },
  { value: 2, title: "Duyệt ngẫu nhiên" },
]);

const listModules = ref([
  { value: "QTDX", title: "Đề xuất" },
  { value: "QTCD", title: "Chiến dịch" },
  { value: "QTUV", title: "Ứng viên" },
]);
const selectedKeyProcedure = ref([]);
const selectedKeySign = ref([]);

watch(selectedKeyProcedure, () => {
  goProcedure(selectedKeyProcedure.value);
});

//Model procedure
const configProcess = ref({});
const vp$ = useVuelidate(ruleprocedure, configProcess);

//Model procedure
const config_approved = ref({});
const vs$ = useVuelidate(rulesign, config_approved);

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
  configProcess.value = {
    status: true,
    config_process_type: 0,
    is_order: 0,
  };
  if (options.value.totalRecords > 0) {
    configProcess.value.is_order = options.value.totalRecords + 1;
  } else {
    configProcess.value.is_order = 1;
  }
  headerDialogProcedure.value = str;
  displayDialogProcedure.value = true;
}; 
const listUserA=ref([]);
const openAddDialogSign = (str) => {
  isAdd.value = true;
  submitted.value = false;
  config_approved.value = {
    config_process_id: options.value.config_process_id,
    status: true,
    config_approved_type: 0,
    is_order: 0 
  };
  if (options.value.totalsign > 0) {
    config_approved.value.is_order = options.value.totalsign + 1;
  } else {
    config_approved.value.is_order = 1;
  }
  listUserA.value = [];
  datalistsD.value = datalistsDSave.value; 
  submitted.value = false;  
  headerDialogSign.value = str;
  displayDialogSign.value = true;
};
const closeDialogProcedure = () => {
  configProcess.value = {
    status: true,
    config_process_type: 0,
    is_order: 0,
  };
  displayDialogProcedure.value = false;
};
const closeDialogSign = () => {
  config_approved.value = {
    status: true,
    config_approved_type: 0,
    is_order: 0,
    users: [],
  };
  displayDialogSign.value = false;
};
const saveconfigProcess = (isFormValid) => {
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
  if (configProcess.value.config_process_pattern_fake == null) {
    swal.fire({
      title: "Thông báo!",
      text: "Vui lòng điền đầy đủ thông tin trường bôi đỏ!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  if (configProcess.value.config_process_name.length > 250) {
    swal.fire({
      title: "Thông báo!",
      text: "Tên quy trình không vượt quá 250 ký tự!",
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
  if (configProcess.value.config_process_pattern_fake) {
    configProcess.value.config_process_pattern =
      configProcess.value.config_process_pattern_fake.toString();
  }
  formData.append("hrm_config_process", JSON.stringify(configProcess.value));
  if (isAdd.value) {
    axios
      .post(
        baseURL + "/api/hrm_config_process/add_hrm_config_process",
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
        toast.success("Thêm quy trình thành công!");
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
  } else {
    axios
      .put(
        baseURL + "/api/hrm_config_process/update_hrm_config_process",
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
        toast.success("Sửa quy trình thành công!");
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
  }
};
const saveconfig_approved = (isFormValid) => {
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
  if (config_approved.value.config_procerduce_name.length > 500) {
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

  var obj = { ...config_approved.value };
  obj.is_approved_by_department = false;
  var us = [];
  obj.users.forEach((element, i) => {
    us.push({
      user_id: element.user_id,
      is_order: i + 1,
    });
  });
  let formData = new FormData();

  formData.append("hrm_config_approved", JSON.stringify(obj));
  formData.append("hrm_config_approved_users", JSON.stringify(us));
  if (isAdd.value) {
    axios
      .post(
        baseURL + "/api/hrm_config_approved/add_hrm_config_approved",
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
        toast.success("Thêm nhóm duyệt thành công!");
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
  } else {
    axios
      .put(
        baseURL + "/api/hrm_config_approved/update_hrm_config_approved",
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
        toast.success("Sửa nhóm duyệt thành công!");
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
  }
  if (submitted.value) submitted.value = true;
};
const goProcedure = (node) => {
  options.value.config_process_id = node["config_process_id"];
  initSign(true);
};
const editProcedure = (item) => {
  submitted.value = false;
  isAdd.value = false;
  configProcess.value = item;
  configProcess.value.config_process_pattern_fake =
    configProcess.value.config_process_pattern.split(",");
  headerDialogProcedure.value = "Cập nhật quy trình";
  displayDialogProcedure.value = true;
};
const editSign = (item) => {
  submitted.value = false;

  isAdd.value = false;

  config_approved.value = item;
  config_approved.value.users = item.signusers;
  headerDialogSign.value = "Cập nhật nhóm duyệt";
  displayDialogSign.value = true;

  // axios
  //   .post(
  //     baseURL + "/api/HRM_SQL/getData",
  //     {
  //       str: encr(
  //         JSON.stringify({
  //           proc: "calendar_duty_signform_get",
  //           par: [
  //             { par: "user_id", va: store.state.user.user_id },
  //             { par: "sign_id", va: item.signform_id },
  //           ],
  //         }),
  //         SecretKey,
  //         cryoptojs
  //       ).toString(),
  //     },
  //     config
  //   )
  //   .then((response) => {
  //     if (response != null && response.data != null) {
  //       var data = response.data.data;
  //       if (data != null) {
  //         var tbs = JSON.parse(data);
  //         config_approved.value = tbs[0][0];
  //         config_approved.value.users = tbs[1];
  //       }
  //     }
  //     swal.close();
  //     if (options.value.loading) options.value.loading = false;

  //     headerDialogSign.value = "Cập nhật nhóm duyệt";
  //     displayDialogSign.value = true;
  //   })
  //   .catch((error) => {
  //     toast.error("Tải dữ liệu không thành công!");
  //     if (options.value.loading) options.value.loading = false;

  //   });
};
const updateStatusProcedure = (item) => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  let data = {
    IntID: item.config_process_id,
    BitTrangthai: item.status || false,
  };
  axios
    .put(
      baseURL + "/api/hrm_config_process/update_s_hrm_config_process",
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
  let data = {
    IntID: item.config_approved_id,
    BitTrangthai: item.status || false,
  };
  axios
    .put(
      baseURL + "/api/hrm_config_approved/update_s_hrm_config_approved",
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
        var ids = [item["config_process_id"]];
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
                var idx = listdatas.value.findIndex(
                  (x) => x.config_process_id == element
                );
                if (idx != -1) {
                  listdatas.value.splice(idx, 1);
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
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
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
        var ids = [item["config_approved_id"]];
        axios
          .delete(
            baseURL + "/api/hrm_config_approved/delete_hrm_config_approved",
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
                var idx = signs.value.findIndex(
                  (x) => x.signform_id == element
                );
                if (idx != -1) {
                  signs.value.splice(idx, 1);
                }
              });
            }
            swal.close();
            toast.success("Xoá nhóm duyệt thành công!");
            initSign(true);
            if (options.value.loading) options.value.loading = false;
          })
          .catch((error) => {
            swal.close();
            if (options.value.loading) options.value.loading = false;
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
const config_process_type = ref();
const headerDialogUser = ref();
const displayDialogUser = ref(false);
const showModalUser = (one, type) => {
  if (type != null) {
    switch (type) {
      case 0:
        selectedUser.value = [...config_approved.value.users];
        headerDialogUser.value = "Chọn người duyệt";
        break;
      default:
        break;
    }
  }

  is_one.value = one;
  config_process_type.value = type;
  displayDialogUser.value = true;
  forceRerender();
};
const choiceUser = () => {
  if (config_process_type.value != null) {
    switch (config_process_type.value) {
      case 0:
        var notexist = selectedUser.value.filter(
          (a) =>
            config_approved.value.users.findIndex(
              (b) => b["user_id"] === a["user_id"]
            ) === -1
        );
        if (notexist.length > 0) {
          config_approved.value.users =
            config_approved.value.users.concat(notexist);
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
const searchText = () => {
  initProcedure(true);
};
const searchSign = () => {
  initSign(true);
};

//init
const refreshProcedure = () => {
  options.value.searchText = "";
  initProcedure(true);
};
const refreshSign = () => {
  options.value.searchsign = "";
  initSign(true);
};
const displayDialogProcess = ref(false);
const onShowDetailsP = () => {
  displayDialogProcess.value = true;
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

  axios
    .post(
      baseURL + "/api/calendar/get_datas",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_config_process_list",
            par: [
              { par: "search", va: options.value.searchText },
              { par: "user_id", va: store.getters.user.user_id },

              { par: "pageno", va: options.value.pageno },
              { par: "pagesize", va: options.value.pagesize },
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
      if (response != null && response.data != null) {
        let data = JSON.parse(response.data.data)[0];
        let data1 = JSON.parse(response.data.data)[1];
        if (isFirst.value) isFirst.value = false;
        data.forEach((element, i) => {
          element.STT = options.value.pageno * options.value.pagesize + i + 1;
        });
        listdatas.value = data;
        if (listdatas.value.length > 0) {
          listdatas.value.forEach((element, i) => {
            element["STT"] = i + 1;
            var idx = types.value.findIndex(
              (x) => x["value"] === element["config_process_type"]
            );
            if (idx !== -1) {
              element["type_name"] = types.value[idx]["title"];
            } else {
              element["type_name"] = "Chưa xác định";
            }

            if (element.config_process_pattern) {
              element.config_process_pattern_name = "";
              let str = "";
              element.config_process_pattern.split(",").forEach((item) => {
                element.config_process_pattern_name +=
                  str + listModules.value.find((x) => x.value == item).title;
                str = ", ";
              });
            }
          });
        }
        if (data1) {
          options.value.totalRecords = data1[0].total;
        }
      } else {
        listdatas.value = [];
      }

      swal.close();
      if (isFirst.value) isFirst.value = false;
      if (options.value.loading) options.value.loading = false;
    })
    .catch((error) => {
      if (options.value.loading) options.value.loading = false;
      console.log(error);
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
      baseURL + "/api/HRM_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_config_approved_list",
            par: [
              { par: "search", va: options.value.searchsign },
              { par: "config_process_id", va: options.value.config_process_id },
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
        var data = response.data.data;
        if (data != null) {
          let tbs = JSON.parse(data);
          if (tbs[0] != null && tbs[0].length > 0) {
            signs.value = tbs[0];
            if (signs.value.length > 0) {
              signs.value.forEach((element, i) => {
                element["STT"] = i + 1;
                var idx = types.value.findIndex(
                  (x) => x["value"] === element["config_approved_type"]
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

const menuButMores = ref();
const itemButMores = ref([
  {
    label: "Hiệu chỉnh nội dung",
    icon: "pi pi-pencil",
    command: (event) => {
      editProcedure(configProcess.value);
    },
  },
  {
    label: "Xoá",
    icon: "pi pi-trash",
    command: (event) => {
      deleteProcedure(configProcess.value);
    },
  },
]);
const toggleMores = (event, item) => {
  configProcess.value = item;
  menuButMores.value.toggle(event);
  //selectedNodes.value = item;
};

const menuButMores_Sign = ref();
const itemButMores_Sign = ref([
  {
    label: "Hiệu chỉnh nội dung",
    icon: "pi pi-pencil",
    command: (event) => {
      editSign(config_approved.value);
    },
  },
  {
    label: "Xoá",
    icon: "pi pi-trash",
    command: (event) => {
      deleteSign(config_approved.value);
    },
  },
]);
const toggleMores_Sign = (event, item) => {
  config_approved.value = item;
  menuButMores_Sign.value.toggle(event);
  //selectedNodes.value = item;
};
const selectedNodes = ref();
const onChangeSWT = () => {
  if (config_approved.value.is_approved_by_department == true) {
    config_approved.value.users = [];
  } else {
    config_approved.value.users = [];
  }
};

const liUserCF = ref([]);
const rechildren = (data) => {
  if (data.data != null) {
    if (data.data.userM)
      liUserCF.value.push({
        approved_user_id: data.data.userM,
        department_id: data.data.organization_id,
      });
    if (data.children)
      data.children.forEach((em) => {
        rechildren(em);
      });
  }
};
const displayAssets = ref(false);
const showListAssets = () => {
  liUserCF.value = [];
  displayAssets.value = true;
};

let selectedTreeU = null;
const showTreeUser = (value) => {
  checkMultile.value = true;
  selectedTreeU = value;
  displayDialogUser.value = true;
};
const checkMultile = ref(false); 
const choiceUserD = () => {
  if (checkMultile.value == true)
    datalistsD.value.forEach((m, i) => {
      let om = { key: m.key, data: m };
      if (m.key == selectedTreeU.organization_id) {
        m.data.userM = selectedUser.value[0].user_id;
        return;
      } else {
        let check = false;
        const rechildren = (mm) => {
          if (mm.key == selectedTreeU.organization_id) {
            mm.data.data.userM = selectedUser.value[0].user_id;
            check = true;
            return;
          } else {
            if (mm.data.children) {
              let dts = mm.data.children;

              if (dts.length > 0) {
                dts.forEach((em) => {
                  let om1 = { key: em.key, data: em };
                  if (check) return;
                  rechildren(om1, em.key);
                });
              }
            }
          }
        };
        if (check) return;
        rechildren(om, m.key);
      }
    });
  else {
    selectedUser.value.forEach((element) => {
      listUserA.value.push(element);
    });
  }

  closeDialogUser();
};
const onSelectDevice = () => {
  datalistsD.value.forEach((dataT) => {
    rechildren(dataT);
  });
  displayAssets.value = false;
};

const datalistsD = ref();

const loadOrganization = (value) => {
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
          value
        );
        datalistsD.value = obj.arrChils;
        datalistsDSave.value = obj.arrChils;

        expandListD(datalistsD.value);
      }
    })
    .catch((error) => {
      options.value.loading = false;
    });
};
const datalistsDSave = ref();


const expandListD = (data) => {
  for (let node of data) {
    expandedKeys.value[node.key] = true;
    expandNode(node);
  }
};
const expandNode = (node) => {
  if (node.children && node.children.length) {
    expandedKeys.value[node.key] = true;
    for (let child of node.children) {
      expandNode(child);
    }
  }
};


onMounted(() => {
  
  loadOrganization(store.getters.user.organization_id);
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
                @keypress.enter="searchText()"
                v-model="options.searchText"
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
            :value="listdatas"
            :loading="options.loading"
            :totalRecords="options.totalRecords"
            :lazy="true"
            :rowHover="true"
            :showGridlines="true"
            :scrollable="true"
            v-model:selection="selectedKeyProcedure"
            selectionMode="single"
            dataKey="config_process_id"
            scrollHeight="flex"
            filterDisplay="menu"
            filterMode="lenient"
            responsiveLayout="scroll"
          >
            <Column
              field="STT"
              header="STT"
              headerStyle="text-align:center;max-width:50px;height:50px"
              bodyStyle="text-align:center;max-width:50px;"
              class="align-items-center justify-content-center text-center"
            >
            </Column>
            <Column
              field="config_process_name"
              header="Tên quy trình"
              headerStyle="height:50px;max-width:auto;"
              headerClass="align-items-center justify-content-center text-center"
            >
            </Column>

            <Column
              field="config_process_pattern_name"
              header="Loại quy trình"
              headerStyle="text-align:center;max-width:150px;height:50px"
              bodyStyle="text-align:center;max-width:150px;"
              class="align-items-center justify-content-center text-center"
            >
            </Column>
            <Column
              field="type_name"
              header="Kiểu duyệt"
              headerStyle="text-align:center;max-width:120px;height:50px"
              bodyStyle="text-align:center;max-width:120px;"
              class="align-items-center justify-content-center text-center"
            >
              <template #body="slotProps">
                <div class="format-flex-center">
                  <Tag
                    class="px-3 py-1"
                    :value="slotProps.data.type_name"
                    :class="'type' + slotProps.data.config_process_type"
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
              header=""
              headerStyle="text-align:center;max-width:50px"
              bodyStyle="text-align:center;max-width:50px"
              class="align-items-center justify-content-center text-center format-center"
            >
              <template #body="data">
                <div
                  v-if="
                    store.state.user.is_super == true ||
                    store.state.user.user_id == data.data.created_by ||
                    (store.state.user.role_id == 'admin' &&
                      store.state.user.organization_id ==
                        data.data.organization_id)
                  "
                >
                  <Button
                    icon="pi pi-ellipsis-h"
                    class="p-button-rounded p-button-text ml-2"
                    @click="toggleMores($event, data.data)"
                    aria-haspopup="true"
                    aria-controls="overlay_More"
                    v-tooltip.top="'Tác vụ'"
                  />
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
        <div v-if="options.config_process_id != null">
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
                @click="onShowDetailsP()"
                class="p-button-outlined p-button-secondary mr-2"
                icon="pi pi-info"
                v-tooltip.top="'Chi tiết quy trình'"
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
                field="STT"
                header="STT"
                headerStyle="text-align:center;max-width:50px;height:50px"
                bodyStyle="text-align:center;max-width:50px;"
                class="align-items-center justify-content-center text-center"
              >
              </Column>
              <Column
                field="config_procerduce_name"
                header="Tên nhóm"
                headerStyle="height:50px;max-width:auto;"
              >
                <template #body="slotProps">
                  <div>
                    <div class="mb-2">
                      {{ slotProps.data.config_procerduce_name }}
                    </div>
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
                          v-tooltip.bottom="{
                            value:
                              item.full_name +
                              '<br/>' +
                              item.position_name +
                              '<br/>' +
                              item.department_name,
                            escape: true,
                          }"
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
                      :class="'type' + slotProps.data.config_approved_type"
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
                header=""
                headerStyle="text-align:center;max-width:50px;height:50px"
                bodyStyle="text-align:center;max-width:50px;"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="data">
                  <div
                    v-if="
                      store.state.user.is_super == true ||
                      store.state.user.user_id == data.data.created_by ||
                      (store.state.user.role_id == 'admin' &&
                        store.state.user.organization_id ==
                          data.data.organization_id)
                    "
                  >
                    <Button
                      icon="pi pi-ellipsis-h"
                      class="p-button-rounded p-button-text ml-2"
                      @click="toggleMores_Sign($event, data.data)"
                      aria-haspopup="true"
                      aria-controls="overlay_More_Sign"
                      v-tooltip.top="'Tác vụ'"
                    />
               
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
    :style="{ width: '35vw' }"
    :closable="true"
    style="z-index: 1000"
    :modal="true"
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="col-12 p-0 field md:col-12 flex align-items-center">
          <div class="col-3 text-left p-0">
            Tên quy trình <span class="redsao">(*)</span>
          </div>
          <div class="col-9 p-0">
            <InputText
              v-model="configProcess.config_process_name"
              spellcheck="false"
              class="w-full"
              :class="{
                'p-invalid': vp$.config_process_name.$invalid && submitted,
              }"
            />
          </div>
        </div>
        <div
          class="col-12 p-0 field flex"
          v-if="
            (vp$.config_process_name.$invalid && submitted) ||
            vp$.config_process_name.$pending.$response
          "
        >
          <div class="col-3 p-0"></div>
          <div class="col-9 p-0">
            <small class="p-error">
              <span class="col-12 p-0">{{
                vp$.config_process_name.required.$message
                  .replace("Value", "Tên quy trình")
                  .replace("is required", "không được để trống!")
              }}</span>
            </small>
          </div>
        </div>
        <div class="col-12 p-0 field md:col-12 flex align-items-center">
          <div class="col-3 text-left p-0">
            Loại quy trình <span class="redsao">(*)</span>
          </div>
          <div class="col-9 p-0">
            <MultiSelect
              v-model="configProcess.config_process_pattern_fake"
              :options="listModules"
              optionLabel="title"
              optionValue="value"
              display="chip"
              placeholder="--- Chọn loại quy trình ---"
              panelClass="d-design-dropdown  d-tree-input"
              class="w-full p-0 sel-placeholder d-tree-input"
              :class="{
                'p-invalid':
                  configProcess.config_process_pattern_fake == null &&
                  submitted,
              }"
            />
          </div>
        </div>
        <div
          class="col-12 p-0 field flex"
          v-if="configProcess.config_process_pattern_fake == null && submitted"
        >
          <div class="col-3 p-0"></div>
          <div class="col-9 p-0">
            <small class="p-error"> Loại quy trình không được để trống! </small>
          </div>
        </div>
        <div class="col-12 p-0 field md:col-12 flex align-items-center">
          <div class="col-3 text-left p-0">Kiểu duyệt</div>
          <div class="col-9 p-0">
            <Dropdown
              :options="types"
              :filter="true"
              :showClear="false"
              :editable="false"
              v-model="configProcess.config_process_type"
              optionLabel="title"
              optionValue="value"
              placeholder="Chọn kiểu duyệt"
              class="w-full"
            >
            </Dropdown>
          </div>
        </div>
        <div class="col-12 p-0 field md:col-12 flex align-items-center">
          <div class="col-6 p-0 align-items-center flex">
            <div class="col-6 p-0">STT</div>
            <div class="col-6 p-0">
              <InputText v-model="configProcess.is_order" class="w-full" />
            </div>
          </div>
          <div class="col-6 p-0 align-items-center flex">
            <div class="col-6 p-0 text-center">Trạng thái</div>
            <div class="col-6 p-0">
              <InputSwitch
                class="w-4rem lck-checked"
                v-model="configProcess.status"
              />
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
        class="p-button-outlined"
      />

      <Button
        label="Lưu"
        icon="pi pi-check"
        @click="saveconfigProcess(!vp$.$invalid)"
      />
    </template>
  </Dialog>

  <Dialog
    @hide="closeDialog"
    :header="headerDialog"
    v-model:visible="displayBasic"
    :maximizable="true"
    :style="{ width: '35vw' }"
    :modal="true"
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="col-12 field flex p-0 pb-2 align-items-center">
          <div class="col-3 p-0">
            Tên nhóm duyệt<span class="redsao pl-1"> (*)</span>
          </div>
          <div class="col-9 p-0">
            <InputText
              v-model="config_approved.config_procerduce_name"
              spellcheck="false"
              class="w-full"
              :class="{
                'p-invalid': vs$.config_procerduce_name.$invalid && submitted,
              }"
            />
          </div>
        </div>
        <div
          v-if="
            (v$.config_procerduce_name.$invalid && submitted) ||
            v$.config_procerduce_name.$pending.$response
          "
          class="col-12 field p-0 flex"
        >
          <div class="col-3 p-0"></div>
          <small class="col-9 p-0">
            <span style="color: red" class="w-full">{{
              v$.config_procerduce_name.required.$message
                .replace("Value", "Tên nhóm duyệt")
                .replace("is required", "không được để trống!")
            }}</span>
          </small>
        </div>

        <div class="col-12 p-0 field md:col-12 flex align-items-center"   v-if="!config_approved.is_approved_by_department">
          <div class="w-10rem text-left p-0">Kiểu duyệt</div>
          <div style="width: calc(100% - 10rem)">
            <Dropdown
              :options="types"
              :filter="true"
              :showClear="false"
              :editable="false"
              v-model="config_approved.config_approved_type"
              optionLabel="title"
              optionValue="value"
              placeholder="Chọn kiểu duyệt"
              class="ip36"
            >
            </Dropdown>
          </div>
        </div>
       
        <div class="field p-0 pb-2 col-12 md:col-12 flex">
          <div class="col-6 p-0 flex align-items-center">
            <div class="col-6 p-0">Nhóm duyệt mặc định</div>
            <div class="col-6 p-0">
              <InputSwitch
                class="w-4rem lck-checked"
                v-model="device_approved_group.is_default"
              />
            </div>
          </div>
          <div class="col-6 p-0 flex align-items-center">
            <div class="col-6 p-0">Trả lại người duyệt trước</div>
            <div class="col-6 p-0">
              <InputSwitch
                class="w-4rem lck-checked"
                v-model="device_approved_group.is_return_created"
              />
            </div>
          </div>
        </div>
        <div class="field p-0 pb-2 col-12 md:col-12 flex">
          <div class="col-6 p-0 flex align-items-center">
            <div class="col-6 p-0">Duyệt theo phòng ban</div>
            <div class="col-6 p-0">
              <InputSwitch
                class="w-4rem lck-checked"
                @change="onChangeSWT()"
                v-model="device_approved_group.is_approved_by_department"
              />
            </div>
          </div>
          <div class="col-6 p-0 flex align-items-center">
            <div class="col-6 p-0">Trạng thái</div>
            <div class="col-6 p-0">
              <InputSwitch
                class="w-4rem lck-checked"
                v-model="device_approved_group.status"
              />
            </div>
          </div>
        </div>
        <div
          class="field p-0 pb-2 col-12 md:col-12 flex"
          v-if="device_approved_group.module == 'TS_PhieuSuaChua'"
        >
          <div class="col-6 p-0 flex align-items-center">
            <div class="col-6 p-0">Trả người đề nghị sửa</div>
            <div class="col-6 p-0">
              <InputSwitch
                class="w-4rem lck-checked"
                v-model="device_approved_group.is_suggest_repair"
              />
            </div>
          </div>
          <div class="col-6 p-0 flex align-items-center">
            <div class="col-6 p-0">Đánh giá thiết bị sửa chữa</div>
            <div class="col-6 p-0">
              <InputSwitch
                class="w-4rem lck-checked"
                v-model="device_approved_group.repair_property_assessment"
              />
            </div>
          </div>
        </div>

        <div
          v-if="device_approved_group.is_approved_by_department"
          class="field p-0 pb-2 col-12 md:col-12 flex"
        >
          <div
            class="
              col-6
              p-0
              flex
              align-items-center
              cursor-pointer
              text-blue-500
            "
            @click="showListAssets"
          >
            <i class="pi pi-plus-circle pr-2"></i> Cấu hình người duyệt theo
            phòng ban
          </div>
        </div>
        <div class="field p-0 pb-2 col-12 md:col-12 flex" v-else>
          <div
            class="
              col-6
              p-0
              flex
              align-items-center
              cursor-pointer
              text-blue-500
            "
            @click="showListUser"
          >
            <i class="pi pi-plus-circle pr-2"></i> Cấu hình người duyệt
          </div>
        </div>
        <div
          v-if="!device_approved_group.is_approved_by_department"
          class="field p-0 pb-2 col-12 md:col-12"
        >
          <!-- style="display: grid; grid-template-columns: repeat(2, 1fr)" -->
          <OrderList
            v-model="listUserA"
            listStyle="height:auto"
            class="w-full"
            dataKey="id"
          >
            <template #header> Danh sách người duyệt </template>
            <template #item="slotProps">
              <Toolbar class="surface-100 m-0 p-0 border-0 w-full">
                <template #start>
                  <div class="flex align-items-center">
                    <Avatar
                      v-bind:label="
                        slotProps.item.avatar
                          ? ''
                          : slotProps.item.full_name.substring(
                              slotProps.item.full_name.lastIndexOf(' ') + 1,
                              slotProps.item.full_name.lastIndexOf(' ') + 2
                            )
                      "
                      :image="basedomainURL + slotProps.item.avatar"
                      class="w-2rem h-2rem"
                      size="large"
                      :style="
                        slotProps.item.avatar
                          ? 'background-color: #2196f3'
                          : 'background:' +
                            bgColor[slotProps.item.full_name.length % 7]
                      "
                      shape="circle"
                      @error="
                        $event.target.src =
                          basedomainURL + '/Portals/Image/nouser1.png'
                      "
                    />
                    <div class="pt-1 pl-2">
                      {{ slotProps.item.full_name }}
                    </div>
                  </div>
                </template>
                <template #end>
                  <div>
                    <Button
                      icon="pi pi-times"
                      class="
                        p-button-rounded p-button-secondary p-button-text
                        ml-1
                      "
                      @click="removeListUser(slotProps.item)"
                    ></Button>
                  </div>
                </template>
              </Toolbar>
            </template>
          </OrderList>

          <!-- <div
            v-for="(item, index) in listUserA"
            :key="index"
            class="m-2 ml-0 mt-0 w-full"
          >
            <div class="country-item w-full flex align-items-center">
              <Toolbar
                class="surface-100 m-0 p-0 border-0 w-full"
                style="border-radius: 20px"
              >
                <template #start>
                  <div class="flex align-items-center">
                    <Avatar
                      v-bind:label="
                        item.avatar
                          ? ''
                          : item.full_name.substring(
                              item.full_name.lastIndexOf(' ') + 1,
                              item.full_name.lastIndexOf(' ') + 2,
                            )
                      "
                      :image="basedomainURL + item.avatar"
                      class="w-2rem h-2rem"
                      size="large"
                      :style="
                        item.avatar
                          ? 'background-color: #2196f3'
                          : 'background:' + bgColor[item.full_name.length % 7]
                      "
                      shape="circle"
                      @error="
                        $event.target.src =
                          basedomainURL + '/Portals/Image/nouser1.png'
                      "
                    />
                    <div class="pt-1 pl-2">
                      {{ item.full_name }}
                    </div>
                  </div>
                </template>
                <template #end>
                  <div>
                    <Button
                      icon="pi pi-times"
                      class="p-button-rounded p-button-secondary p-button-text ml-1"
                      @click="removeListUser(item)"
                    ></Button>
                  </div>
                </template>
              </Toolbar>
            </div>
          </div> -->
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="closeDialogDC()"
        class="p-button-outlined"
      />

      <Button
        @click="saveHandover(!v$.$invalid)"
        label="Lưu"
        icon="pi pi-check"
        autofocus
      />
    </template>
  </Dialog>
  <Dialog
    :header="headerDialogSign"
    v-model:visible="displayDialogSign"
    :style="{ width: '30vw' }"
    :closable="true"
    style="z-index: 1000"
    :modal="true"
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="col-12 p-0 field md:col-12 flex align-items-center">
          <div class="w-10rem text-left p-0">
            Tên nhóm duyệt <span class="redsao">(*)</span>
          </div>
          <div style="width: calc(100% - 10rem)">
           
          </div>
        </div>
        <div
          v-if="
            (vs$.config_procerduce_name.$invalid && submitted) ||
            vs$.config_procerduce_name.$pending.$response
          "
          class="col-12 p-0 field md:col-12 flex align-items-center"
        >
          <div class="w-10rem text-left p-0"></div>
          <div class="p-0" style="width: calc(100% - 10rem)">
            <small class="p-error">
              <span class="col-12 p-0">{{
                vs$.config_procerduce_name.required.$message
                  .replace("Value", "Tên nhóm duyệt")
                  .replace("is required", "không được để trống")
              }}</span>
            </small>
          </div>
        </div>
      
        <div class="col-12 p-0 field flex align-items-center">
          <div class="col-6 p-0 flex align-items-center">
            <div class="w-10rem text-left p-0">Duyệt phòng ban</div>
            <div style="width: calc(100% - 10rem)">
              <InputSwitch @change="onChangeSWT"
                v-model="config_approved.is_approved_by_department"
                class="ml-0 w-4rem lck-checked"
              />
            </div>
          </div>
          <div class="col-6 p-0 flex align-items-center">
            <div class="w-15rem text-center p-0 pl-3">Trả lại người tạo</div>
            <div class="w-full">
              <InputSwitch
                v-model="config_approved.is_return_created"
                class="ml-3 w-4rem lck-checked"
              />
            </div>
          </div>
        </div>
        <div class="col-12 p-0 field flex align-items-center">
          <div class="col-6 p-0 flex align-items-center">
            <div class="w-10rem text-left p-0">Thứ tự duyệt</div>
            <div style="width: calc(100% - 10rem)">
              <InputText v-model="config_approved.is_order" class="w-full" />
            </div>
          </div>
          <div class="col-6 p-0 flex align-items-center">
            <div class="w-15rem text-left p-0 pl-3">Trạng thái</div>
            <div class="w-full">
              <InputSwitch
                v-model="config_approved.status"
                class="ml-3 w-4rem lck-checked"
              />
            </div>
          </div>
        </div>
        <div
          class="col-12 p-0 field flex align-items-center"
          v-if="!config_approved.is_approved_by_department"
        >
          <div
            style="cursor: pointer; color: #2196f3"
            @click="showModalUser(false, 0)"
          >
            <i
              class="pi pi-plus-circle ml-2"
              v-tooltip.top="'Chọn người dùng'"
            ></i>
            <span class="pl-2"> Cấu hình người duyệt </span>
          </div>
        </div>
        <div
          v-else
          class="field p-0 pb-2 col-12 md:col-12 flex"
        >
          <div
            class="
              col-6
              p-0
              flex
              align-items-center
              cursor-pointer
              text-blue-500
            "
            @click="showListAssets"
          >
            <i class="pi pi-plus-circle pr-2"></i> Cấu hình người duyệt theo
            phòng ban
          </div>
        </div>
        <div class="col-12 md:col-12" v-if="!config_approved.is_approved_by_department">
          <OrderList
            v-model="config_approved.users"
            listStyle="height:auto"
            dataKey="user_id"
          >
            <template #header> Danh sách người duyệt </template>
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
                  <span class="text-left">{{ slotProps.item.full_name }}</span>
                  <span class="text-left" style="color: #aaa">{{
                    slotProps.item.position_name
                  }}</span>
                </div>
                <div class="format-flex-center">
                  <a @click="removeUser(slotProps.item, config_approved.users)">
                    <i class="pi pi-trash"></i>
                  </a>
                </div>
              </div>
            </template>
          </OrderList>
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="closeDialogSign()"
        class="p-button-outlined"
      />

      <Button
        label="Lưu"
        icon="pi pi-check"
        @click="saveconfig_approved(!vs$.$invalid)"
      />
    </template>
  </Dialog>

  <Dialog
    header="Chi tiết quy trình"
    v-model:visible="displayDialogProcess"
    :style="{ width: '50vw' }"
    :closable="true"
    style="z-index: 1000"
    :modal="true"
  >
    <form>
      <div class="grid formgrid m-2">fff</div>
    </form>
  </Dialog>
  
  <Dialog
    header="Cập nhật nhóm duyệt phòng ban"
    v-model:visible="displayAssets"
    :maximizable="true"
    :style="{ width: '50vw' }"
    :modal="true"
  >
    <div>
      <div class="true flex-grow-1 p-2" id="scrollTop">
        <div class="grid p-0">
          <div class="col-12 field format-center">
            <TreeTable :expandedKeys="expandedKeys" :value="datalistsD">
              <!-- <Column 
              bodyClass="w-3rem"
              headerStyle="text-align:center;width:50px"
              bodyStyle="text-align:center;width:50px" :expander="true"></Column> -->
              <Column class="w-7" field="organization_name" :expander="true">
                <template #header>
                  <div class="w-full format-center">
                    <i class="pi pi-building pr-2"></i> Phòng ban
                  </div>
                </template>
              </Column>
              <Column class="w-5">
                <template #header>
                  <div class="w-full format-center">
                    <i class="pi pi-user pr-2"></i> Người duyệt
                  </div>
                </template>
                <template #body="data">
                  <div class="w-full flex">
                    <Button
                      @click="showTreeUser(data.node.data)"
                      v-tooltip.top="'Chọn người duyệt'"
                      icon="pi pi-user-plus"
                      class="p-button-rounded w-3rem mx-3"
                    ></Button>

                    <Dropdown
                      v-model="data.node.data.userM"
                      :options="listDropdownUserGive"
                      :filter="true"
                      optionLabel="name"
                      optionValue="code"
                      class="w-full p-design-dropdown"
                      placeholder="Chọn người duyệt "
                      :showClear="true"
                      :virtualScrollerOptions="{
                        lazy: true,
                        itemSize: 1,
                        showLoader: true,
                        loading: loadingUser,
                        delay: 250,
                      }"
                      @click="onFilterUserDropdown(data.node.data)"
                    >
                      <template #value="slotProps">
                        <div
                          class="
                            country-item country-item-value
                            flex
                            align-items-center
                          "
                          v-if="slotProps.value"
                        >
                          <Avatar
                            v-bind:label="
                              listDropdownUser.filter(
                                (x) => x.code == slotProps.value
                              )[0].avatar
                                ? ''
                                : listDropdownUser
                                    .filter((x) => x.code == slotProps.value)[0]
                                    .name.substring(
                                      listDropdownUser
                                        .filter(
                                          (x) => x.code == slotProps.value
                                        )[0]
                                        .name.lastIndexOf(' ') + 1,
                                      listDropdownUser
                                        .filter(
                                          (x) => x.code == slotProps.value
                                        )[0]
                                        .name.lastIndexOf(' ') + 2
                                    )
                            "
                            :image="
                              basedomainURL +
                              listDropdownUser.filter(
                                (x) => x.code == slotProps.value
                              )[0].avatar
                            "
                            class="w-2rem h-2rem mr-2"
                            size="large"
                            :style="
                              listDropdownUser.filter(
                                (x) => x.code == slotProps.value
                              )[0].avatar
                                ? 'background-color: #2196f3'
                                : 'background:' +
                                  bgColor[
                                    listDropdownUser.filter(
                                      (x) => x.code == slotProps.value
                                    )[0].name.length % 7
                                  ]
                            "
                            shape="circle"
                            @error="
                              $event.target.src =
                                basedomainURL + '/Portals/Image/nouser1.png'
                            "
                          />
                          <div>
                            {{
                              listDropdownUser.filter(
                                (x) => x.code == slotProps.value
                              )[0].name
                            }}
                          </div>
                        </div>
                        <span v-else>
                          {{ slotProps.placeholder }}
                        </span>
                      </template>
                      <template #option="slotProps">
                        <div class="country-item flex align-items-center">
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
                            class="w-3rem h-3rem"
                            size="large"
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
                          <div class="pt-1 pl-2">
                            {{ slotProps.option.name }}
                          </div>
                        </div>
                      </template>
                    </Dropdown>
                  </div>
                </template>
              </Column>
            </TreeTable>
          </div>
        </div>
      </div>

      <div class="p-0" id="scrollDM">
        <Toolbar class="p-2 surface-0 border-none">
          <template #end>
            <Button
              @click="hideSelectDevice()"
              label="Hủy"
              icon="pi pi-times"
              class="mr-2 p-button-outlined"
            />
            <Button
              @click="onSelectDevice()"
              label="Chọn"
              icon="pi pi-check"
              autofocus
            />
          </template>
        </Toolbar>
      </div>
    </div>
  </Dialog>
  <Menu
    id="overlay_More"
    ref="menuButMores"
    :model="itemButMores"
    :popup="true"
  />
  <Menu
    id="overlay_More_Sign"
    ref="menuButMores_Sign"
    :model="itemButMores_Sign"
    :popup="true"
  />
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