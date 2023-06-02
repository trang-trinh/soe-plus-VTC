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
  pagenoAP: 0,
  pagesizeAP: 20,
  totalRecordsAP: 0,
});
const types = ref([
{ title: "Duyệt một nhiều", value: 1 },
  { title: "Duyệt tuần tự", value: 2 },
  { title: "Duyệt ngẫu nhiên", value: 3 },
]);

const listModules = ref([]);
const selectedKeyProcedure = ref([]);
const selectedKeySign = ref([]);

 

//Model procedure
const sys_config_process = ref({});
const vp$ = useVuelidate(ruleprocedure, sys_config_process);

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
  selectedKeyProcedure.value=[];
  sys_config_process.value = {
    status: true, 
    is_local: true,
    config_process_type: 1,
    is_order: options.value.is_orderProcess,
  };
  options.value.config_process_id=null;
  headerDialogProcedure.value = str;
  displayDialogProcedure.value = true;
};
const listUserA = ref([]);
const listApproveds = ref([]);
const approvedSelected = ref([]);
const openAddDialogSign = (str) => {
  isAdd.value = true;
  submitted.value = false;

  axios
    .post(
      baseURL + "/api/HRM_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_config_approved_list_module",
            par: [
              { par: "pageno", va: options.value.pagenoAP },
              { par: "pagesize", va: options.value.pagesizeAP },
              { par: "user_id", va: store.getters.user.user_id },
              { par: "module_key", va: "M13" },
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
        var data = response.data.data;
        if (data != null) {
          let tbs = JSON.parse(data);
          if (tbs[0] != null && tbs[0].length > 0) {
            listApproveds.value = tbs[0];
            if (listApproveds.value.length > 0) {
              listApproveds.value.forEach((element, i) => {
                element["STT"] = i + 1;

                if (element["signusers"] != null) {
                  element["signusers"] = JSON.parse(element["signusers"]);
                }

                if (element["signusers"] != null) {
                  element["signusers"].forEach((ilem) => {
                    if (ilem.is_order == "") ilem.is_order = null;
                    else ilem.is_order = Number(ilem.is_order);
                    if (ilem.approved_users_id == "")
                      ilem.approved_users_id = null;
                    else
                      ilem.approved_users_id = Number(ilem.approved_users_id);
                    if (ilem.department_id == "") ilem.department_id = null;
                    else ilem.department_id = Number(ilem.department_id);
                    if (ilem.avatar == "") ilem.avatar = null;
                    else ilem.avatar = ilem.avatar;
                  });
                }
              });
           
           
           
            }
          } else {
            listApproveds.value = [];
          }
          signs.value.forEach(ele => {
           if( listApproveds.value.find(x=>x.approved_groups_id==ele.approved_groups_id)!=null)
            approvedSelected.value.push(listApproveds.value.find(x=>x.approved_groups_id==ele.approved_groups_id));  
          });
          if (tbs.length == 2) {
            options.value.totalRecordsAP = tbs[1][0].total;
          }
        }
      }
      swal.close();
      if (isFirst.value) isFirst.value = false;
      if (options.value.loading) options.value.loading = false;
      submitted.value = false;
      headerDialogSign.value = str;
      displayDialogSign.value = true;
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
      console.log(error);
      return;
    });
};
const closeDialogProcedure = () => {
  sys_config_process.value = {
    status: true,
    config_process_type: 1,
     
  };
  displayDialogProcedure.value = false;
};
const closeDialogSign = () => {
  listApproveds.value = [];
  displayDialogSign.value = false;
};
const savesys_config_process = (isFormValid) => {
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
  if (sys_config_process.value.module_fake == null) {
    swal.fire({
      title: "Thông báo!",
      text: "Vui lòng điền đầy đủ thông tin trường bôi đỏ!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  if (sys_config_process.value.config_process_name.length > 250) {
    swal.fire({
      title: "Thông báo!",
      text: "Tên quy trình không vượt quá 250 ký tự!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  if (!sys_config_process.value.module_fake) {
    return;
  } else {
    let str = "";
    let kol = "";
    for (const key in sys_config_process.value.module_fake) {
      str += kol + key;
      kol = ",";
    }
    sys_config_process.value.module = str;
  }
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });

  let formData = new FormData();

  formData.append(
    "sys_config_process",
    JSON.stringify(sys_config_process.value)
  );
  if (isAdd.value) {
    axios
      .post(
        baseURL + "/api/sys_config_process/add_sys_config_process",
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
        baseURL + "/api/sys_config_process/update_sys_config_process",
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
const saveconfig_approved = () => {
  approvedSelected.value.forEach((element, i) => {
    if (
      signs.value.find(
        (x) => x.approved_groups_id == element.approved_groups_id
      ) == null
    ) {
      signs.value.push(element);
    }
  });
  signs.value.forEach((item, i) => {
    item.is_order = i + 1;
    item.config_process_id = options.value.config_process_id;
  });
  approvedSelected.value = [];
  let formData = new FormData();
  formData.append("sys_process_link_approved", JSON.stringify(signs.value));

  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });

  axios
    .post(
      baseURL + "/api/sys_process_link_approved/add_sys_process_link_approved",
      formData,
      config
    )
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Thêm nhóm duyệt thành công!");
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

  closeDialogSign();
};
const goProcedure = (event) => {
  
   sys_config_process.value = event.data;
 
  options.value.config_process_id = sys_config_process.value["config_process_id"];
  initSign(true);
};
const editProcedure = (item) => {
  submitted.value = false;
  isAdd.value = false;
  sys_config_process.value = item;

  headerDialogProcedure.value = "Cập nhật quy trình";
  
  options.value.config_process_id=null;
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

const onChangeModule = (event) => {
  for (const key in event) {
    if (key == -1) {
      const qrechildren = (mm) => {
        sys_config_process.value.module_fake[mm.key] = {
          checked: true,
          partialChecked: false,
        };
        if (mm.children) {
          mm.children.forEach((ol) => {
            qrechildren(ol);
          });
        }
      };
      listModules.value.forEach((element) => {
        qrechildren(element);
      });
    }
  }
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
      baseURL + "/api/sys_config_process/update_s_sys_config_process",
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
      baseURL + "/api/sys_config_approved/update_s_sys_config_approved",
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
            baseURL + "/api/sys_config_process/delete_sys_config_process",
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
        var ids = [item["config_approved_id"]];
        axios
          .delete(
            baseURL + "/api/sys_config_approved/delete_sys_config_approved",
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

const selectedUser = ref([]);
const is_one = ref(false);
const config_process_type = ref();
const headerDialogUser = ref();
const displayDialogUser = ref(false);
const displayAprroved = ref(false);
const showModalUser = () => {
  displayAprroved.value = true;
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
  selectedKeyProcedure.value=[];
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
            proc: "sys_config_process_list_module",
            par: [
              { par: "search", va: options.value.searchText },
              { par: "user_id", va: store.getters.user.user_id },
              { par: "module_key", va:"M13"},
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
            if (!element.module_fake) {
              element.module_fake = {};
              element.module.split(",").forEach((item) => {
                element.module_fake[item] = {
                  checked: true,
                  partialChecked: false,
                };
              });
            }
          });
        }
        if (data1) {
          options.value.totalRecords = data1[0].total;
          options.value.is_orderProcess=data1[0].total+1;
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
            proc: "sys_link_approved_list",
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
      editProcedure(sys_config_process.value);
    },
  },
  {
    label: "Xoá",
    icon: "pi pi-trash",
    command: (event) => {
      deleteProcedure(sys_config_process.value);
    },
  },
]);
const toggleMores = (event, item) => {
  sys_config_process.value = item;
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
const removeListUser = (value) => {
  signs.value = signs.value.filter(
    (x) => x.approved_groups_id != value.approved_groups_id
  );
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
const first = ref();
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
let arrr = [];
const initTudien = () => {
  arrr = [];
  axios
    .post(
      baseURL + "/api/Notify/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_modules_listmodulestop",
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
      if (data.length > 0) {
        arrr = data[0];
      }
    })
    .catch((error) => {
      if (error.status === 401) {
        swal.fire({
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
      }
    });

  axios
    .post(
      baseURL + "/api/Modules/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_modules_listbymodule_id",
            par: [{ par: "module_id", va: 235}],
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
        let obj = renderTree(data, "module_id", "module_name", "module");
        listModules.value = obj.arrtreeChils;
        initProcedure(true);
      }
    })
    .catch((error) => {
      console.log(error);
    });
};

const renderTree = (data, id, name, title) => {
  let arrChils = [];
  let arrtreeChils = [];

  data
    .filter(
      (x) =>
        x.parent_id == null &&
        x.module_key =="M13"
    )
    .forEach((m, i) => {
      m.IsOrder = i + 1;
      let om = { key: m[id], data: m };
      const rrechildren = (mm, pid) => {
        let dts = data.filter((x) => x.parent_id == pid);
        if (dts.length > 0) {
          if (!mm.children) mm.children = [];
          dts.forEach((em) => {
            let om1 = { key: em[id], data: em };
            rrechildren(om1, em[id]);
            mm.children.push(om1);
          });
        }
      };
      rrechildren(om, m[id]);
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
const selectionChangeOrder=(event)=>{


  let formData = new FormData();
  formData.append("sys_process_link_approved", JSON.stringify(signs.value));
 
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });

  axios
    .put(
      baseURL + "/api/sys_process_link_approved/update_sys_process_link_approved",
      formData,
      config
    )
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
    
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
onMounted(() => {
  initTudien();
  loadOrganization(store.getters.user.organization_id);
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
      <SplitterPanel :size="70" :minSize="35">
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
            @row-click=" goProcedure($event)"
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
              class="align-items-center justify-content-center text-center"
              headerStyle="text-align:center;height:50px ;max-width:400px;"
              bodyStyle="text-align:center;max-width:400px;"
              field="module"
              header="Modules"
            >
              <template #body="data">
                <div class="w-full">
                  <TreeSelect
                    panelClass="d-design-dropdown  d-tree-input d-tree-border"
                    class="w-full p-0 sel-placeholder d-tree-input d-tree-border"
                    v-model="data.data.module_fake"
                    :options="listModules"
                    selectionMode="checkbox"
                    optionLabel="data.module_name"
                    optionValue="data.module_id"
                    display="chip"
                  ></TreeSelect>
                </div>
              </template>
            </Column>
            <Column
              field="type_name"
              header="Kiểu duyệt"
              headerStyle="text-align:center;max-width:180px;height:50px"
              bodyStyle="text-align:center;max-width:180px;"
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
                :disabled="!data.data.is_local"
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
                  v-if="data.data.is_local &&(
                    store.state.user.is_super == true ||
                    store.state.user.user_id == data.data.created_by ||
                    (store.state.user.role_id == 'admin' &&
                      store.state.user.organization_id ==
                        data.data.organization_id))
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
      <SplitterPanel :size="30" :minSize="30">
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
                v-if="sys_config_process.is_local==true "
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
          <div class="d-lang-table" >
            <Panel   v-if="!(sys_config_process.is_local==true) ">
              <template #header><div class="font-bold">Danh sách nhóm duyệt </div></template>
              
             <div class="w-full p-0" v-for="(slotProps, index) in signs" :key="index">
               <Toolbar class="surface-0 m-0 p-0 border-0 w-full">
                  <template #start>
                    <div class="flex align-items-center">
                      <div class="format-flex-center">
                        <b class="p-3">{{ index  + 1 }} </b>
                      </div>
                      <div class="flex">
                        <div>
                          <div class="mb-2">
                            {{ slotProps.approved_group_name }}
                          </div>
                          <div v-if="slotProps.signusers">
                            <AvatarGroup
                              v-if="
                                slotProps.signusers &&
                                slotProps.signusers.length > 0
                              "
                            >
                              <Avatar
                                v-for="(
                                  elen, index1
                                ) in slotProps.signusers.slice(0, 3)"
                                v-bind:label="
                                  elen.avatar
                                    ? ''
                                    : elen.last_name.substring(0, 1)
                                "
                                v-bind:image="
                                  elen.avatar
                                    ? basedomainURL + elen.avatar
                                    : basedomainURL + '/Portals/Image/noimg.jpg'
                                "
                                v-tooltip.bottom="{
                                  value:
                                    elen.full_name +
                                    '<br/>' +
                                    elen.position_name +
                                    '<br/>' +
                                    elen.department_name,
                                  escape: true,
                                }"
                                :key="elen.user_id"
                                style="
                                  border: 2px solid white;
                                  color: white;
                                  width: 2.5rem;
                                  height: 2.5rem;
                                "
                                @error="
                                  basedomainURL + '/Portals/Image/noimg.jpg'
                                "
                                size="large"
                                shape="circle"
                                class="cursor-pointer"
                                :style="{ backgroundColor: bgColor[index1 % 7] }"
                              />
                              <Avatar
                                v-if="
                                  slotProps.signusers &&
                                  slotProps.signusers.length > 3
                                "
                                v-bind:label="
                                  '+' +
                                  (
                                    slotProps.signusers.length - 3
                                  ).toString()
                                "
                                shape="circle"
                                size="large"
                                style="
                                  background-color: #2196f3;
                                  color: #ffffff;
                                "
                                class="cursor-pointer"
                              />
                            </AvatarGroup>
                          </div>
                        </div>
                      </div>
                    </div>
                  </template>
            
                </Toolbar>
   </div>
</Panel>

            <OrderList     v-else   
              v-model="signs"
              listStyle="height: auto"
              dataKey="process_link_approved_id"
              @reorder="selectionChangeOrder($event)"
            >
              <template #header> Danh sách nhóm duyệt </template>
              <template #item="slotProps">
                <Toolbar class="surface-0 m-0 p-0 border-0 w-full">
                  <template #start>
                    <div class="flex align-items-center">
                      <div class="format-flex-center">
                        <b class="p-3">{{ slotProps.index + 1 }} </b>
                      </div>
                      <div class="flex">
                        <div>
                          <div class="mb-2">
                            {{ slotProps.item.approved_group_name }}
                          </div>
                          <div v-if="slotProps.item.signusers">
                            <AvatarGroup
                              v-if="
                                slotProps.item.signusers &&
                                slotProps.item.signusers.length > 0
                              "
                            >
                              <Avatar
                                v-for="(
                                  elen, index
                                ) in slotProps.item.signusers.slice(0, 3)"
                                v-bind:label="
                                  elen.avatar
                                    ? ''
                                    : elen.last_name.substring(0, 1)
                                "
                                v-bind:image="
                                  elen.avatar
                                    ? basedomainURL + elen.avatar
                                    : basedomainURL + '/Portals/Image/noimg.jpg'
                                "
                                v-tooltip.bottom="{
                                  value:
                                    elen.full_name +
                                    '<br/>' +
                                    elen.position_name +
                                    '<br/>' +
                                    elen.department_name,
                                  escape: true,
                                }"
                                :key="elen.user_id"
                                style="
                                  border: 2px solid white;
                                  color: white;
                                  width: 2.5rem;
                                  height: 2.5rem;
                                "
                                @error="
                                  basedomainURL + '/Portals/Image/noimg.jpg'
                                "
                                size="large"
                                shape="circle"
                                class="cursor-pointer"
                                :style="{ backgroundColor: bgColor[index % 7] }"
                              />
                              <Avatar
                                v-if="
                                  slotProps.item.signusers &&
                                  slotProps.item.signusers.length > 3
                                "
                                v-bind:label="
                                  '+' +
                                  (
                                    slotProps.item.signusers.length - 3
                                  ).toString()
                                "
                                shape="circle"
                                size="large"
                                style="
                                  background-color: #2196f3;
                                  color: #ffffff;
                                "
                                class="cursor-pointer"
                              />
                            </AvatarGroup>
                          </div>
                        </div>
                      </div>
                    </div>
                  </template>
                  <template #end>
                    <div   >
                      <Button 
                        icon="pi pi-trash"
                        class="p-button-rounded p-button-secondary p-button-text"
                        @click="removeListUser(slotProps.item)"
                      ></Button>
                    </div>
                  </template>
                </Toolbar>
              </template>
              <!-- <template #item="slotProps">
                <div class="flex">
                  <div class="format-flex-center">
                    <b class="p-3">{{ slotProps.index + 1 }}</b>
                  </div>
                  <div class="image-container pl-3 pr-2">
                    {{ slotProps.item }}
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
                    <a
                      @click="removeUser(slotProps.item, config_approved.users)"
                    >
                      <i class="pi pi-trash"></i>
                    </a>
                  </div>
                </div>
              </template> -->
            </OrderList>
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
              v-model="sys_config_process.config_process_name"
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
            Modules <span class="redsao">(*)</span>
          </div>
          <div class="col-9 p-0">
            <TreeSelect
              panelClass="d-design-dropdown  d-tree-input"
              class="w-full p-0 sel-placeholder d-tree-input"
              placeholder="--- Chọn Module ---"
              v-model="sys_config_process.module_fake"
              :options="listModules"
              @change="onChangeModule($event)"
              :showClear="true"
              selectionMode="checkbox"
              optionLabel="data.module_name"
              optionValue="data.module_id"
              display="chip"
              :class="{
                'p-invalid': !sys_config_process.module_fake && submitted,
              }"
            ></TreeSelect>
          </div>
        </div>
        <div
          class="col-12 p-0 field flex"
          v-if="sys_config_process.module_fake == null && submitted"
        >
          <div class="col-3 p-0"></div>
          <div class="col-9 p-0">
            <small class="p-error"> Modules không được để trống! </small>
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
              v-model="sys_config_process.config_process_type"
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
             
              <InputText v-model="sys_config_process.is_order" class="w-full" />
            </div>
          </div>
          <div class="col-6 p-0 align-items-center flex">
            <div class="col-6 p-0 text-center">Trạng thái</div>
            <div class="col-6 p-0">
              <InputSwitch
                class="w-4rem lck-checked"
                v-model="sys_config_process.status"
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
        @click="savesys_config_process(!vp$.$invalid)"
      />
    </template>
  </Dialog>

  <Dialog
    :header="headerDialogSign"
    v-model:visible="displayDialogSign"
    :style="{ width: '50vw' }"
    :closable="true"
    style="z-index: 1000"
    :modal="true"
  >
    <form>
      <div class="grid formgrid">
        <div class="col-12 md:col-12">
          <DataTable
            class="w-full p-datatable-sm e-sm"
            @page="onPage($event)"
            @filter="onFilter($event)"
            @sort="onSort($event)"
            v-model:filters="filters"
            removableSort
            filterDisplay="menu"
            filterMode="lenient"
            dataKey="approved_groups_id"
            responsiveLayout="scroll"
            :scrollable="true"
            scrollHeight="flex"
            :showGridlines="true"
            :rows="options.pagesizeAP"
            :lazy="true"
            :value="listApproveds"
            :loading="options.loading"
            :paginator="true"
            :totalRecords="options.totalRecordsAP"
            :row-hover="true"
            v-model:first="first"
            v-model:selection="approvedSelected"
            :pageLinkSize="options.pagesizeAP"
            paginatorTemplate="FirstPageLink PrevPageLink PageLinks  NextPageLink LastPageLink RowsPerPageDropdown"
            :rowsPerPageOptions="[20, 30, 50, 100, 200]"
            selectionMode="checkbox"
          >
            <template #header>
              <Toolbar class="d-toolbar custoolbar p-0 py-2 surface-50">
                <template #start>
                  <span class="p-input-icon-left">
                    <i class="pi pi-search" />
                    <InputText
                      v-model="options.search"
                      @keyup.enter="searchCard()"
                      type="text"
                      spellcheck="false"
                      placeholder="Tìm kiếm"
                    />
                  </span>
                </template>
              </Toolbar>
            </template>
            <Column
               selectionMode="multiple"  v-if="store.getters.user.is_super==true"
              headerStyle="text-align:center;max-width:75px;height:50px"
              bodyStyle="text-align:center;max-width:75px"
              class="align-items-center justify-content-center text-center"
            ></Column>
            <Column
              headerStyle="text-align:center;height:50px;  "
              bodyStyle="text-align:left; "
              field="approved_group_name"
              headerClass="align-items-center justify-content-center text-center"
              header="Nhóm duyệt"
            >
              <template #body="slotProps">
                <div>
                  <div class="mb-2">
                    {{ slotProps.data.approved_group_name }}
                  </div>
                  <div v-if="slotProps.data.signusers">
                    <AvatarGroup
                      v-if="
                        slotProps.data.signusers &&
                        slotProps.data.signusers.length > 0
                      "
                    >
                      <Avatar
                        v-for="(item, index) in slotProps.data.signusers.slice(
                          0,
                          3
                        )"
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
                          '+' + (slotProps.data.signusers.length - 3).toString()
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
              class="align-items-center justify-content-center text-center"
              headerStyle="text-align:center;height:50px;max-width:150px;"
              bodyStyle="text-align:center;max-width:150px; ;"
              header="Loại duyệt"
              headerClass="textoneline"
            >
              <template #body="data">
                <div>
                  {{
                    data.data.approved_type == 1
                      ? "Duyệt một nhiều"
                      : data.data.approved_type == 2
                      ? "Duyệt tuần tự"
                      : "Duyệt ngẫu nhiên"
                  }}
                </div>
              </template>
            </Column>
            <Column
              class="align-items-center justify-content-center text-center"
              headerStyle="text-align:center;max-width:150px;height:50px"
              bodyStyle="text-align:center;max-width:150px; "
              header="Duyệt phòng ban"
              headerClass="textoneline"
            >
              <template #body="data">
                <Checkbox
                  :disabled="true"
                  :binary="data.data.is_department"
                  v-model="data.data.is_department"
                />
              </template>
            </Column>
            <template #empty>
              <div
                class="align-items-center justify-content-center p-4 text-center m-auto"
                v-if="!isFirst"
              >
                <img src="../../../assets/background/nodata.png" height="144" />
                <h3 class="m-1">Không có dữ liệu</h3>
              </div>
            </template>
          </DataTable>
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

      <Button label="Chọn" icon="pi pi-check" @click="saveconfig_approved()" />
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
                          class="country-item country-item-value flex align-items-center"
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
.type1 {
  background-color: #ff8b4e;
}
.type2 {
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