<script setup>
import { onMounted, inject, ref, watch } from "vue";
import { encr } from "../../../util/function";
import { useToast } from "vue-toastification";
import dialogdecíion from "./component/dialogdecision.vue";
import framepreview from "../component/framepreview.vue";
import DocComponent from "../template/components/DocComponent.vue";
import moment from "moment";

const router = inject("router");
const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios");
const base_url = baseURL;
const config = {
  headers: {
    Authorization: `Bearer ${store.getters.token}`,
  },
};
const toast = useToast();
const cryoptojs = inject("cryptojs");
const basedomainURL = baseURL;

//Declare
const isFilter = ref(false);
const tabs = ref([
  { id: -1, title: "Tất cả", icon: "", total: 0 },
  { id: 0, title: "Chờ duyệt", icon: "", total: 0 },
  { id: 1, title: "Đã duyệt", icon: "", total: 0 },
  { id: 2, title: "Không duyệt", icon: "", total: 0 },
]);
const options = ref({
  loading: true,
  user_id: store.getters.user.user_id,
  search: "",
  pageNo: 1,
  pageSize: 25,
  total: 0,
  sort: "created_date desc",
  orderBy: "desc",
  tab: -1,
  filterContract_id: null,
  organizations: [],
  departments: [],
  type_contracts: [],
  work_positions: [],
  sign_start_date: null,
  sign_end_date: null,
  users: [],
  start_start_date: null,
  end_start_date: null,
  start_end_date: null,
  end_end_date: null,
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
const typestatus = ref([
  { value: 0, title: "Chờ duyệt", bg_color: "#0078d4", text_color: "#fff" },
  { value: 1, title: "Đã duyệt", bg_color: "#5FC57B", text_color: "#fff" },
  { value: 2, title: "Không duyệt", bg_color: "#DF5249", text_color: "#fff" },
]);
const liquidations = ref([
  { value: 0, title: "Thôi việc" },
  { value: 1, title: "Ký hợp đồng mới" },
  { value: 2, title: "Chấm dứt HĐLĐ" },
  { value: 3, title: "Chấm dứt HĐLĐ" },
  { value: 4, title: "Khác..." },
]);
const visibleSidebarDoc = ref(false);
const report = ref({ datadic: null });
const selectedNodes = ref({});
const selectedKeys = ref([]);
const expandedKeys = ref([]);
const isFirst = ref(true);
const datas = ref([]);
const counts = ref([]);
const dictionarys = ref([]);
const decision = ref({});

const menuButMores = ref();
const itemButMores = ref([
  {
    label: "Hiệu chỉnh nội dung",
    icon: "pi pi-pencil",
    command: (event) => {
      editItem(decision.value, "Chỉnh sửa quyết định");
    },
  },
  {
    label: "Nhân bản quyết định",
    icon: "pi pi-copy",
    command: (event) => {
      copyItem(decision.value, "Nhân bản quyết định");
    },
  },
  {
    label: "In quyết định",
    icon: "pi pi-print",
    command: (event) => {
      openDialogFrame(decision.value);
    },
  },
  {
    label: "Xoá",
    icon: "pi pi-trash",
    command: (event) => {
      deleteItem(decision.value);
    },
  },
]);
const toggleMores = (event, item) => {
  decision.value = item;
  decision.value.isEdit = true;
  menuButMores.value.toggle(event);
  selectedNodes.value = item;
  options.value["filterContract_id"] = selectedNodes.value["decision_id"];
};

// watch(selectedNodes, () => {
//   options.value["filterContract_id"] = selectedNodes.value["decision_id"];
// });

const configQuyetdinh = async (row) => {
  let strSQL = {
    query: false,
    proc: "hrm_decision_config",
    par: [
      {
        par: "decision_id",
        va: row.decision_id,
      },
      {
        par: "report_key",
        va: row.report_key,
      },
    ],
  };
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  const axResponse = await axios.post(
    baseURL + "/api/HRM_SQL/PostProc",
    {
      str: encr(JSON.stringify(strSQL), SecretKey, cryoptojs).toString(),
    },
    {
      headers: { Authorization: `Bearer ${store.getters.token}` },
    }
  );
 
  if (axResponse.status == 200) {
    if (axResponse.data.error) {
      toast.error("Không mở được bản ghi");
    } else {
      let dt = JSON.parse(axResponse.data.data);
      decision.value = dt[0][0];
      report.value = dt[1][0];
      report.value.datadic = [{ title: "Quyết định", data: decision.value }];
      // report.value.proc_name = `smartreport_decide_profile_list `;
      // report.value.proc_all = `smartreport_decide_profile_list_all`;
      report.value.proc_name = `decision_profile_list '${store.getters.user.user_id}', '${row.decision_id}'`;
      report.value.proc_all = `decision_profile_list_all '${store.getters.user.user_id}', '${row.decision_id}'`;
      let cg = {};
      if (report.value.report_config) {
        cg = JSON.parse(report.value.report_config);
      }
      cg.proc = {
        name: "hrm_decision_user_get",
        parameters: [
          {
            Parameter_name: "@decision_id",
            Type: "varchar",
            Length: 50,
            Param_order: 1,
          },
          {
            Parameter_name: "@decision_user_id",
            Type: "varchar",
            Length: 50,
            Param_order: 2,
          },
        ],
        sql: report.value.proc_name,
        data: JSON.stringify(cg.data),
        issql: true,
      };
      report.value.report_config = JSON.stringify(cg);
      visibleSidebarDoc.value = true;
    }
  }
  swal.close();
};
//export
const menuButs = ref();
const itemButs = ref([
  {
    label: "Export dữ liệu ra Excel",
    icon: "pi pi-file-excel",
    command: (event) => {
      //exportData("ExportExcel");
    },
  },
  {
    label: "Import dữ liệu từ Excel",
    icon: "pi pi-file-excel",
    command: (event) => {
      //exportData("ExportExcel");
    },
  },
]);
const toggleExport = (event) => {
  menuButs.value.toggle(event);
};

const menuAddItems = ref();
const itemAddItems = ref([]);
const toggleAddItem = (event) => {
  menuAddItems.value.toggle(event);
};

//Function
const componentKey = ref({});
const forceRerender = (type) => {
  if (!componentKey.value[type]) {
    componentKey.value[type] = 0;
  }
  componentKey.value[type] += 1;
};
function CreateGuid() {
  function _p8(s) {
    var p = (Math.random().toString(16) + "000000000").substr(2, 8);
    return s ? "-" + p.substr(0, 4) + "-" + p.substr(4, 4) : p;
  }
  return _p8() + _p8(true) + _p8(true) + _p8();
}
const activeTab = (tab) => {
  options.value.tab = tab.id;
  initData(true);
};
const opstatus = ref();
const toggleStatus = (item, event) => {
  decision.value = item;
  opstatus.value.toggle(event);
};
const goProfile = (item) => {
  if (item.profile_id != null) {
    router.push({
      name: "profileinfo",
      params: { id: item.decision_id },
      query: { id: item.profile_id },
    });
  }
};
const copyItem = (item, str) => {
  files.value = [];
  submitted.value = false;
  options.value.loading = true;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  isAdd.value = true;
  isCopy.value = true;
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_contract_get",
            par: [{ par: "decision_id", va: item.decision_id }],
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
          if (model.value["profiles"] != null) {
            model.value["profile"] = JSON.parse(model.value["profiles"])[0];
          }
          if (model.value["sign_users"] != null) {
            model.value["sign_user"] = JSON.parse(model.value["sign_users"])[0];
          }
          if (model.value["manager_users"] != null) {
            model.value["manager_user"] = JSON.parse(
              model.value["manager_users"]
            )[0];
          }
          if (model.value["start_date"] != null) {
            model.value["start_date"] = new Date(model.value["start_date"]);
          }
          if (model.value["end_date"] != null) {
            model.value["end_date"] = new Date(model.value["end_date"]);
          }
          if (model.value["sign_date"] != null) {
            model.value["sign_date"] = new Date(model.value["sign_date"]);
          }
          if (model.value["professional_works"] != null) {
            model.value["professional_works"] = model.value[
              "professional_works"
            ]
              .split(",")
              .map((x) => parseInt(x));
          }
        }
        if (tbs[1] != null && tbs[1].length > 0) {
          model.value["allowances"] = tbs[1];
          if (tbs[2] != null && tbs[2].length > 0) {
            var formalitys = tbs[2].filter((x) => x["is_type"] === 0);
            formalitys.forEach((x) => {
              if (x["allowance_formality_id"] == null) {
                x["allowance_formality_id"] = x["allowance_formality"];
              }
            });
            var wages = tbs[2].filter((x) => x["is_type"] === 1);
            wages.forEach((x) => {
              if (x["allowance_wage_id"] == null) {
                x["allowance_wage_id"] = x["allowance_wage"];
              }
            });
            model.value["allowances"].forEach((allowance) => {
              if (allowance["start_date"] != null) {
                allowance["start_date"] = new Date(allowance["start_date"]);
              }
              allowance.formalitys = formalitys.filter(
                (x) => x["allowance_id"] === allowance["allowance_id"]
              );
              allowance.wages = wages.filter(
                (x) => x["allowance_id"] === allowance["allowance_id"]
              );
            });
          }
        } else {
          model.value.allowances = [];
        }
        if (tbs[3] != null && tbs[3].length > 0) {
          model.value["files"] = tbs[3];
        }
      }
      swal.close();
      if (options.value.loading) options.value.loading = false;

      forceRerender(0);
      headerDialog.value = str;
      displayDialog.value = true;
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

const headerDialogFrame = ref();
const displayDialogFrame = ref(false);
const openDialogFrame = (item) => {
  forceRerender(1);
  headerDialogFrame.value = "Thông tin quyết định";
  configQuyetdinh(item);
  displayDialogFrame.value = true;
};
const closeDialogFrame = () => {
  forceRerender(1);
  displayDialogFrame.value = false;
};
const callbackFun = (obj) => {
  console.log(obj);
  saveDGQuyetdinhUser(obj);
};
const saveDGQuyetdinhUser = async (r) => {
  let strSQL = {
    query: false,
    proc: "decision_user_add",
    par: [
 
      { par: "decision_id", va: decision.value.decision_id },
      { par: "profile_id", va: r.profile_id },
      { par: "is_data", va: JSON.stringify(r.is_data) },
      { par: "user_id", va: store.getters.user.user_id },
      { par: "ip", va: store.getters.ip },
      { par: "organization_id", va: store.getters.user.organization_id },
    ],
  };
  console.log(strSQL);
  try {
    const axResponse = await axios.post(
      baseURL + "/api/HRM_SQL/getData",
      {
        str: encr(JSON.stringify(strSQL), SecretKey, cryoptojs).toString(),
      },
      {
        headers: { Authorization: `Bearer ${store.getters.token}` },
      }
    );
    if (axResponse.status == 200) {
    }  
  } catch (e) {
    console.log(e);
  }
};

//add model
const isAdd = ref(false);
const isCopy = ref(false);
const submitted = ref(false);
const model = ref({});
const headerDialog = ref();
const displayDialog = ref(false);
const files = ref([]);
const type_decision = ref({});
const openAddDialog = (type, str) => {
  type_decision.value = type;
  forceRerender(0);
  isAdd.value = true;
  isCopy.value = false;
  model.value = {
    type_decision_id: type.type_decision_id,
    type_decision_code: type.type_decision_code,
    profile: null,
    sign_user: null,
    contract_code: "",
    contract_name: "",
    employment:
      dictionarys.value[0] != null ? dictionarys.value[0][0].address : "",
    start_date: new Date(),
    sign_date: new Date(),
    status: 0,
    is_order: options.value.total + 1,
    allowances: [
      {
        allowance_id: CreateGuid(),
        start_date: new Date(),
        formalitys: [{}],
        wages: [{}],
      },
    ],
    files: [],
  };
  headerDialog.value = str;
  displayDialog.value = true;
};
const closeDialog = () => {
  forceRerender(0);
  displayDialog.value = false;
};
const editItem = (item, str) => {
  files.value = [];
  submitted.value = false;
  options.value.loading = true;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  isAdd.value = false;
  isCopy.value = false;
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_contract_get",
            par: [{ par: "decision_id", va: item.decision_id }],
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
          if (model.value["profiles"] != null) {
            model.value["profile"] = JSON.parse(model.value["profiles"])[0];
          }
          if (model.value["sign_users"] != null) {
            model.value["sign_user"] = JSON.parse(model.value["sign_users"])[0];
          }
          if (model.value["manager_users"] != null) {
            model.value["manager_user"] = JSON.parse(
              model.value["manager_users"]
            )[0];
          }
          if (model.value["start_date"] != null) {
            model.value["start_date"] = new Date(model.value["start_date"]);
          }
          if (model.value["end_date"] != null) {
            model.value["end_date"] = new Date(model.value["end_date"]);
          }
          if (model.value["sign_date"] != null) {
            model.value["sign_date"] = new Date(model.value["sign_date"]);
          }
          if (model.value["professional_works"] != null) {
            model.value["professional_works"] = model.value[
              "professional_works"
            ]
              .split(",")
              .map((x) => parseInt(x));
          }
        }
        if (tbs[1] != null && tbs[1].length > 0) {
          model.value["allowances"] = tbs[1];
          if (tbs[2] != null && tbs[2].length > 0) {
            var formalitys = tbs[2].filter((x) => x["is_type"] === 0);
            formalitys.forEach((x) => {
              if (x["allowance_formality_id"] == null) {
                x["allowance_formality_id"] = x["allowance_formality"];
              }
            });
            var wages = tbs[2].filter((x) => x["is_type"] === 1);
            wages.forEach((x) => {
              if (x["allowance_wage_id"] == null) {
                x["allowance_wage_id"] = x["allowance_wage"];
              }
            });
            model.value["allowances"].forEach((allowance) => {
              if (allowance["start_date"] != null) {
                allowance["start_date"] = new Date(allowance["start_date"]);
              }
              allowance.formalitys = formalitys.filter(
                (x) => x["allowance_id"] === allowance["allowance_id"]
              );
              allowance.wages = wages.filter(
                (x) => x["allowance_id"] === allowance["allowance_id"]
              );
            });
          }
        } else {
          model.value.allowances = [];
        }
        if (tbs[3] != null && tbs[3].length > 0) {
          model.value["files"] = tbs[3];
        }
      }
      swal.close();
      if (options.value.loading) options.value.loading = false;

      forceRerender(0);
      headerDialog.value = str;
      displayDialog.value = true;
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
const udpateStatusItem = (item) => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios
    .put(
      baseURL + "/api/position/updateStatusPosition",
      {
        str: encr(
          JSON.stringify({
            id: item.decision_id,
            status: !(item.status || false),
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      if (response.data.err === "1") {
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
        initData(true);
      }
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
const deleteItem = (item) => {
  if (item != null || options.value["filterContract_id"] != null) {
    swal
      .fire({
        title: "Thông báo",
        text: "Bạn có muốn xoá quyết định này không!",
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
          var ids = [];
          if (item != null) {
            ids = [item["decision_id"]];
          } else {
          }
          axios
            .delete(baseURL + "/api/hrm_decision/delete_decision", {
              headers: { Authorization: `Bearer ${store.getters.token}` },
              data: ids,
            })
            .then((response) => {
              if (response.data.err === "1" || response.data.err === "2") {
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
              toast.success("Xoá thành công!");
              initData(true);
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
        }
      });
  }
};
const onUpload = () => {};
const removeFile = (event) => {
  files.value = [];
  event.files.forEach((element) => {
    files.value.push(element);
  });
};
const selectFile = (event) => {
  files.value = [];
  event.files.forEach((element) => {
    files.value.push(element);
  });
};
const setStar = (item) => {
  submitted.value = true;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  let formData = new FormData();
  item.is_star = !(item.is_star || false);
  formData.append("is_star", item.is_star);
  formData.append("ids", JSON.stringify([item["decision_id"]]));
  axios
    .put(baseURL + "/api/hrm_contract/update_star_contract", formData, config)
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
      swal.close();
      toast.success("Cập nhật thành công!");
      //initData(true);
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
  if (submitted.value) submitted.value = true;
};
const setStatus = (status, event) => {
  if (status === 3) {
    openAddDialogLiquidation("Thanh lý quyết định");
  } else {
    updateStatus(decision.value, status, event);
  }
};
const updateStatus = (item, status, event, model) => {
  opstatus.value.toggle(event);
  closeDialogLiquidation();
  if (status === 3 && (!model || !model.date)) {
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
  let data = {
    id: item["decision_id"],
    status: status,
    content: model != null ? model.content : "",
    date: model != null ? model.date : null,
  };
  axios
    .put(baseURL + "/api/hrm_decision/update_status_decision", data, config)
    .then((response) => {
      if (response.data.err === "1") {
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
        initCount();
        initData(true);
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
const headerDialogLiquidation = ref();
const displayDialogLiquidation = ref(false);
const modelLiquidation = ref();
const openAddDialogLiquidation = (str) => {
  forceRerender(0);
  modelLiquidation.value = {
    content: "",
    date: null,
  };
  headerDialogLiquidation.value = str;
  displayDialogLiquidation.value = true;
};
const closeDialogLiquidation = () => {
  displayDialogLiquidation.value = false;
};

//Init
const initDictionary = () => {
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_decision_dictionary",
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
          dictionarys.value = tbs;

          if (tbs[3] != null && tbs[3].length > 0) {
            tbs[3].forEach((item) => {
              itemAddItems.value.push({
                label: item.type_decision_name,
                icon: "pi pi-plus",
                command: (event) => {
                  openAddDialog(item, "Thêm mới quyết định");
                },
              });
            });
          }
        }
      }
    });
};
const initCount = () => {
  if (isFilter.value) {
    initCountFilter();
    return;
  }
  tabs.value.forEach((x) => {
    x["total"] = 0;
  });
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_decision_count",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "search", va: options.value.search },
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
            counts.value = tbs[0];
            tabs.value.forEach((x) => {
              var idx = counts.value.findIndex((c) => c["status"] == x["id"]);
              if (idx !== -1) {
                x["total"] = counts.value[idx]["total"];
              }
            });
          }
        }
      }
    });
};
const initData = (ref) => {
  if (ref) {
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
  }
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_decision_list",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "search", va: options.value.search },
              { par: "pageNo", va: options.value.pageNo },
              { par: "pageSize", va: options.value.pageSize },
              { par: "tab", va: options.value.tab },
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
            data[0].forEach((item, i) => {
              item["STT"] = i + 1;
              if (item.decision_date != null) {
                item.decision_date = moment(
                  new Date(item.decision_date)
                ).format("DD/MM/YYYY");
              }
              if (item.start_date != null) {
                item.start_date = moment(new Date(item.start_date)).format(
                  "DD/MM/YYYY"
                );
              }
              if (item.end_date != null) {
                item.end_date = moment(new Date(item.end_date)).format(
                  "DD/MM/YYYY"
                );
              }
              var idx = typestatus.value.findIndex(
                (x) => x["value"] === item["status"]
              );
              if (idx != -1) {
                item["status_name"] = typestatus.value[idx]["title"];
                item["bg_color"] = typestatus.value[idx]["bg_color"];
                item["text_color"] = typestatus.value[idx]["text_color"];
              } else {
                item["status_name"] = "Chưa xác định";
                item["bg_color"] = "#bbbbbb";
                item["text_color"] = "#fff";
              }
            });
            datas.value = data[0];
            if (data[1] != null && data[1].length > 0) {
              options.value.total = data[1][0].total;
            }
          } else {
            datas.value = [];
            options.value.total = 0;
          }
        }
      }
      if (isFirst.value) isFirst.value = false;
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
const refresh = () => {
  selectedNodes.value = {};
  options.value = {
    loading: true,
    user_id: store.getters.user.user_id,
    search: "",
    pageNo: 1,
    pageSize: 25,
    total: 0,
    sort: "created_date desc",
    orderBy: "desc",
    tab: -1,
    filterContract_id: null,
    organizations: [],
    departments: [],
    type_contracts: [],
    work_positions: [],
    sign_start_date: null,
    sign_end_date: null,
    users: [],
    start_start_date: null,
    end_start_date: null,
    start_end_date: null,
    end_end_date: null,
  };
  isFilter.value = false;
  initData(true);
};
//page
const onPage = (event) => {
  if (event.rows != options.value.pageSize) {
    options.value.pageSize = event.rows;
  }
  options.value.pageNo = event.page + 1;
  initData(true);
};
onMounted(() => {
  initDictionary();
  initCount();
  initData(true);
});
</script>
<template>
  <div class="surface-100 p-2">
    <Toolbar class="outline-none surface-0 border-none">
      <template #start>
        <span class="p-input-icon-left">
          <i class="pi pi-search" />
          <InputText
            @keypress.enter="search()"
            v-model="options.search"
            type="text"
            spellcheck="false"
            placeholder="Tìm kiếm"
            class="input-search"
          />
        </span>
      </template>
      <template #end>
        <Button
          @click="toggleAddItem"
          label="Thêm mới"
          icon="pi pi-plus"
          class="mr-2"
        >
          <div>
            <span><i class="pi pi-plus mr-2"></i></span>
            <span class="mr-2">Thêm mới</span>
            <span><i class="pi pi-chevron-down"></i></span>
          </div>
        </Button>
        <Menu
          :model="itemAddItems"
          :popup="true"
          id="overlay_AddItem"
          ref="menuAddItems"
        />
        <Button
          @click="toggleExport"
          label="Tiện ích"
          icon="pi pi-file-excel"
          class="p-button-outlined p-button-secondary mr-2"
          aria-haspopup="true"
          aria-controls="overlay_Export"
        >
          <div>
            <span class="mr-2">Tiện ích</span>
            <span><i class="pi pi-chevron-down"></i></span>
          </div>
        </Button>
        <Menu
          :model="itemButs"
          :popup="true"
          id="overlay_Export"
          ref="menuButs"
        />
        <Button
          @click="refresh()"
          class="p-button-outlined p-button-secondary"
          icon="pi pi-refresh"
          v-tooltip.top="'Tải lại'"
        />
      </template>
    </Toolbar>
    <div class="tabview">
      <div class="tableview-nav-content">
        <ul class="tableview-nav">
          <li
            v-for="(tab, key) in tabs"
            :key="key"
            @click="activeTab(tab)"
            class="tableview-header"
            :class="{ highlight: options.tab === tab.id }"
          >
            <a>
              <i :class="tab.icon"></i>
              <span>{{ tab.title }} ({{ tab.total }})</span>
            </a>
          </li>
        </ul>
      </div>
    </div>
    <div class="d-lang-table">
      <DataTable
        @page="onPage($event)"
        :value="datas"
        :paginator="true"
        :rows="options.pageSize"
        :rowsPerPageOptions="[25, 50, 100, 200]"
        :totalRecords="options.total"
        :scrollable="true"
        :lazy="true"
        :rowHover="true"
        :showGridlines="false"
        :globalFilterFields="['decision_name']"
        v-model:selection="selectedNodes"
        selectionMode="single"
        dataKey="decision_id"
        scrollHeight="flex"
        filterDisplay="menu"
        filterMode="lenient"
        paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
        responsiveLayout="scroll"
      >
        <!-- <Column
          field="STT"
          header="STT"
          headerStyle="text-align:center;max-width:75px;height:50px"
          bodyStyle="text-align:center;max-width:75px;"
          class="align-items-center justify-content-center text-center"
        >
        </Column> -->
        <Column
          field="decision_code"
          header="Số QĐ"
          headerStyle="text-align:center;max-width:120px;height:50px"
          bodyStyle="text-align:center;max-width:120px;"
          class="align-items-center justify-content-center text-center"
        />
        <Column
          field="profile_user_name"
          header="Tên nhân sự"
          headerStyle="height:50px;max-width:auto;"
        >
          <template #body="slotProps">
            <b @click="goProfile(slotProps.data)" class="hover">{{
              slotProps.data.profile_user_name
            }}</b>
          </template>
        </Column>
        <Column
          field="position_name"
          header="Cấp ra QĐ"
          headerStyle="height:50px;max-width:auto;"
        >
        </Column>
        <Column
          field="type_decision_name"
          header="Loại quyết định"
          headerStyle="height:50px;max-width:auto;"
        >
        </Column>
        <Column
          field="decision_date"
          header="Ngày quyết định"
          headerStyle="text-align:center;max-width:120px;height:50px"
          bodyStyle="text-align:center;max-width:120px;"
          class="align-items-center justify-content-center text-center"
        >
          <template #body="slotProps">
            <span>{{ slotProps.data.decision_date }}</span>
          </template>
        </Column>
        <Column
          field="start_date"
          header="Ngày hiệu lực"
          headerStyle="text-align:center;max-width:120px;height:50px"
          bodyStyle="text-align:center;max-width:120px;"
          class="align-items-center justify-content-center text-center"
        >
          <template #body="slotProps">
            <span>{{ slotProps.data.start_date }}</span>
          </template>
        </Column>
        <Column
          field="end_date"
          header="Ngày hết hạn"
          headerStyle="text-align:center;max-width:120px;height:50px"
          bodyStyle="text-align:center;max-width:120px;"
          class="align-items-center justify-content-center text-center"
        >
          <template #body="slotProps">
            <span>{{ slotProps.data.end_date }}</span>
          </template>
        </Column>
        <Column
          field="status"
          header="Trạng thái"
          headerStyle="text-align:center;max-width:130px;height:50px"
          bodyStyle="text-align:center;max-width:130px;"
          class="align-items-center justify-content-center text-center"
        >
          <template #body="slotProps">
            <div
              class="m-2"
              @click="
                toggleStatus(slotProps.data, $event);
                $event.stopPropagation();
              "
              aria:haspopup="true"
              aria-controls="overlay_panel_status"
            >
              <Button
                :label="slotProps.data.status_name"
                icon="pi pi-chevron-down"
                iconPos="right"
                class="p-button-outlined"
                :style="{
                  borderColor: slotProps.data.bg_color,
                  // backgroundColor: slotProps.data.bg_color,
                  color: slotProps.data.bg_color,
                  borderRadius: '15px',
                  padding: '0.3rem 0.75rem !important',
                }"
              />
            </div>
            <OverlayPanel
              :showCloseIcon="false"
              ref="opstatus"
              appendTo="body"
              class="p-0 m-0"
              id="overlay_panel_status"
              style="width: 200px"
            >
              <div class="form-group">
                <label>Trạng thái</label>
                <Dropdown
                  :options="typestatus"
                  :filter="false"
                  :showClear="false"
                  :editable="false"
                  v-model="decision.status"
                  optionLabel="title"
                  optionValue="value"
                  placeholder="Chọn trạng thái"
                  class="ip36"
                  @change="setStatus(decision.status, $event)"
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
            </OverlayPanel>
          </template>
        </Column>
        <Column
          header=""
          headerStyle="text-align:center;max-width:50px"
          bodyStyle="text-align:center;max-width:50px"
          class="align-items-center justify-content-center text-center"
        >
          <template #body="slotProps">
            <Button
              icon="pi pi-ellipsis-h"
              class="p-button-rounded p-button-text"
              @click="toggleMores($event, slotProps.data)"
              aria-haspopup="true"
              aria-controls="overlay_More"
              v-tooltip.top="'Tác vụ'"
            />
          </template>
        </Column>
        <template #empty>
          <div
            class="align-items-center justify-content-center p-4 text-center m-auto"
            style="
              display: flex;
              width: 100%;
              height: calc(100vh - 231px);
              background-color: #fff;
            "
          >
            <div v-if="!options.loading && (!isFirst || options.total == 0)">
              <img src="../../../assets/background/nodata.png" height="144" />
              <h3 class="m-1">Không có dữ liệu</h3>
            </div>
          </div>
        </template>
      </DataTable>
    </div>
  </div>
  <Sidebar
    v-model:visible="visibleSidebarDoc"
    position="full"
    class="d-sidebar-full"
    @hide="initData(true)"
  >
    <template #header>
      <h2 class="p-0 m-0">
        <i class="pi pi-cog mr-2"></i>{{ decision.decision_name }}
      </h2>
    </template>
    <div style="padding: 0 20px">
      <DocComponent
        :isedit="true"
        :report="report"
        :callbackFun="callbackFun"
        :readonly="true"
      ></DocComponent>
    </div>
  </Sidebar>
  <dialogdecíion
    :key="componentKey['0']"
    :headerDialog="headerDialog"
    :displayDialog="displayDialog"
    :closeDialog="closeDialog"
    :isAdd="isAdd"
    :isCopy="isCopy"
    :isView="isView"
    :type_decision="type_decision"
    :decision="decision"
    :initData="initData"
  />
  <!-- <framepreview
    :key="componentKey['1']"
    :headerDialog="headerDialogFrame"
    :displayDialog="displayDialogFrame"
    :closeDialog="closeDialogFrame"
    :type="3"
    :model="decision"
  /> -->
  <Dialog
    :header="headerDialogLiquidation"
    v-model:visible="displayDialogLiquidation"
    :style="{ width: '30vw' }"
    :maximizable="true"
    :closable="false"
    style="z-index: 9000"
  >
    <form @submit.prevent="" name="submitform">
      <div class="grid formgrid m-2">
        <div class="col-12 md:col-12">
          <div class="form-group">
            <label>Ngày thanh lý <span class="redsao">(*)</span></label>
            <Calendar
              :showIcon="true"
              class="ip36"
              autocomplete="on"
              inputId="time24"
              :class="{
                'p-invalid': !modelLiquidation.date && submitted,
              }"
              :showOnFocus="false"
              v-model="modelLiquidation.date"
              placeholder="DD/MM/YYYY"
            />
            <div v-if="!modelLiquidation.date && submitted">
              <small class="p-error">
                <span class="col-12 p-0"
                  >Hiệu lực từ ngày không được để trống</span
                >
              </small>
            </div>
          </div>
        </div>
        <div class="col-12 md:col-12">
          <div class="form-group">
            <label>Nội dung</label>
            <Dropdown
              :options="liquidations"
              :filter="true"
              :showClear="true"
              :editable="true"
              v-model="modelLiquidation.content"
              optionLabel="title"
              optionValue="title"
              placeholder="Chọn kiểu thanh lý"
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
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="closeDialogLiquidation()"
        class="p-button-text"
      />
      <Button
        label="Lưu"
        icon="pi pi-check"
        @click="updateStatus(decision, 3, $event, modelLiquidation)"
      />
    </template>
  </Dialog>
  <Menu
    id="overlay_More"
    ref="menuButMores"
    :model="itemButMores"
    :popup="true"
  />
</template>
<style scoped>
@import url(../contract/component/stylehrm.css);
.d-lang-table {
  height: calc(100vh - 166px) !important;
  background-color: #fff;
}
.icon-star {
  color: #f4b400 !important;
}
.hover:hover {
  color: #0078d4;
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
</style>
