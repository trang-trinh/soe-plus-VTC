<script setup>
import { onMounted, inject, ref, watch } from "vue";
import { encr } from "../../../util/function";
import { useToast } from "vue-toastification";
import { useRoute } from "vue-router";
import dialogcontract from "../contract/component/dialogcontract.vue";
import framepreview from "../component/framepreview.vue";
import moment from "moment";

const route = useRoute();
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
  { id: 0, title: "Chưa hiệu lực", icon: "", total: 0 },
  { id: 1, title: "Đang hiệu lực", icon: "", total: 0 },
  { id: 2, title: "Hết hiệu lực", icon: "", total: 0 },
  { id: 3, title: "Đã thanh lý", icon: "", total: 0 },
  { id: 4, title: "Hết hạn trong tháng", icon: "", total: 0 },
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
  path: null,
  name: null,
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
  { value: 0, title: "Chưa hiệu lực", bg_color: "#0078d4", text_color: "#fff" },
  { value: 1, title: "Đang hiệu lực", bg_color: "#5FC57B", text_color: "#fff" },
  { value: 2, title: "Hết hiệu lực", bg_color: "#DF5249", text_color: "#fff" },
  { value: 3, title: "Đã thanh lý", bg_color: "#F39C12", text_color: "#fff" },
]);
const liquidations = ref([
  { value: 0, title: "Thôi việc" },
  { value: 1, title: "Ký hợp đồng mới" },
  { value: 2, title: "Chấm dứt HĐLĐ" },
  { value: 3, title: "Chấm dứt HĐLĐ" },
  { value: 4, title: "Khác..." },
]);
const selectedNodes = ref({});
const selectedKeys = ref([]);
const expandedKeys = ref([]);
const isFirst = ref(true);
const datas = ref([]);
const counts = ref([]);
const dictionarys = ref([]);
const contract = ref({});
const functions = ref({});
const rolefunctions = ref([]);

const menuButMores = ref();
const itemButMores = ref([
  {
    label: "Hiệu chỉnh nội dung",
    icon: "pi pi-pencil",
    command: (event) => {
      editItem(contract.value, "Chỉnh sửa hợp đồng");
    },
  },
  {
    label: "Nhân bản hợp đồng",
    icon: "pi pi-copy",
    command: (event) => {
      copyContract(contract.value, "Nhân bản hợp đồng");
    },
  },
  {
    label: "In hợp đồng",
    icon: "pi pi-print",
    command: (event) => {
      openDialogFrame(contract.value);
    },
  },
  {
    label: "Xoá",
    icon: "pi pi-trash",
    command: (event) => {
      deleteItem(contract.value);
    },
  },
]);
const toggleMores = (event, item) => {
  contract.value = item;
  menuButMores.value.toggle(event);
  selectedNodes.value = item;
  options.value["filterContract_id"] = selectedNodes.value["contract_id"];
};

// watch(selectedNodes, () => {
//   options.value["filterContract_id"] = selectedNodes.value["contract_id"];
// });

//filter
const search = () => {
  options.value.pageNo = 1;
  initCount();
  initData(true);
};
const opfilter = ref();
const toggleFilter = (event) => {
  opfilter.value.toggle(event);
};
const resetFilter = () => {
  options.value.organizations = [];
  options.value.departments = [];
  options.value.type_contracts = [];
  options.value.work_positions = [];
  options.value.sign_start_date = null;
  options.value.sign_end_date = null;
  options.value.users = [];
  options.value.start_start_date = null;
  options.value.end_start_date = null;
  options.value.start_end_date = null;
  options.value.end_end_date = null;
};
const removeFilter = (idx, array) => {
  array.splice(idx, 1);
};
const filter = (event) => {
  opfilter.value.toggle(event);
  isFilter.value = true;
  initCount();
  initData(true);
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
  contract.value = item;
  opstatus.value.toggle(event);
};
const goProfile = (item) => {
  router.push({
    name: "profileinfo",
    params: { id: item.contract_id },
    query: { id: item.profile_id },
  });
};
const copyContract = (item, str) => {
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
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_contract_get",
            par: [{ par: "contract_id", va: item.contract_id }],
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

const printViewContract = (row) => {
  if (row && row.report_key) {
    let o = { id: row.report_key, par: { contract_id: row.contract_id } };
    let url = encodeURIComponent(
      encr(JSON.stringify(o), SecretKey, cryoptojs).toString()
    );
    url =
      "https://doconline.soe.vn/report/" +
      url.replaceAll("%", "==") +
      "?v=" +
      new Date().getTime().toString();
    window.open(url);
  } else {
    swal.fire({
      title: "Thông báo!",
      text: "Chưa thiết lập mẫu in cho loại hợp đồng!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
};

const headerDialogFrame = ref();
const displayDialogFrame = ref(false);
const openDialogFrame = (item) => {
  forceRerender(1);
  headerDialogFrame.value = "Thông tin hợp đồng";
  displayDialogFrame.value = true;
};
const closeDialogFrame = () => {
  forceRerender(1);
  displayDialogFrame.value = false;
};

//add model
const isAdd = ref(false);
const submitted = ref(false);
const model = ref({});
const headerDialog = ref();
const displayDialog = ref(false);
const files = ref([]);
const openAddDialog = (str) => {
  forceRerender(0);
  isAdd.value = true;
  model.value = {
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
    wage_type: 1,
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
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_contract_get",
            par: [{ par: "contract_id", va: item.contract_id }],
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
          if (model.value["wage_start_date"] != null) {
            model.value["wage_start_date"] = new Date(
              model.value["wage_start_date"]
            );
          }
          if (model.value["wage_end_date"] != null) {
            model.value["wage_end_date"] = new Date(
              model.value["wage_end_date"]
            );
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
            id: item.contract_id,
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
        text: "Bạn có muốn xoá hợp đồng này không!",
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
            ids = [item["contract_id"]];
          } else {
          }
          axios
            .delete(baseURL + "/api/hrm_contract/delete_contract", {
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
  formData.append("ids", JSON.stringify([item["contract_id"]]));
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
    openAddDialogLiquidation("Thanh lý hợp đồng");
  } else {
    updateStatus(contract.value, status, event);
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
    id: item["contract_id"],
    status: status,
    content: model != null ? model.content : "",
    date: model != null ? model.date : null,
  };
  axios
    .put(baseURL + "/api/hrm_contract/update_status_contract", data, config)
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
const initRoleFunction = () => {
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_profile_rolefunction_get",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "is_link", va: options.value.path },
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
            let permissions = Object.entries(tbs[0][0]);
            for (const [key, value] of permissions) {
              functions.value[key] = value;
            }
          }
          if (tbs[1] != null && tbs[1].length > 0) {
            if (
              tbs[1][0].module_functions != null &&
              tbs[1][0].module_functions != ""
            ) {
              let module_functions = tbs[1][0].module_functions.split(",");
              for (var key in module_functions) {
                functions.value[module_functions[key]] = true;
              }
            }
          }
          rolefunctions.value = tbs;
        }
      }
    });
};
const initDictionary = () => {
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_contract_dictionary",
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
        }
      }
    });
};
const initCountFilter = () => {
  var organizations = null;
  if (
    options.value.organizations != null &&
    options.value.organizations.length > 0
  ) {
    organizations = options.value.organizations
      .map((x) => x["organization_id"])
      .join(",");
  }
  var departments = null;
  if (
    options.value.departments != null &&
    options.value.departments.length > 0
  ) {
    departments = options.value.departments
      .map((x) => x["department_id"])
      .join(",");
  }
  var type_contracts = null;
  if (
    options.value.type_contracts != null &&
    options.value.type_contracts.length > 0
  ) {
    type_contracts = options.value.type_contracts
      .map((x) => x["type_contract_id"])
      .join(",");
  }
  var work_positions = null;
  if (
    options.value.work_positions != null &&
    options.value.work_positions.length > 0
  ) {
    work_positions = options.value.work_positions
      .map((x) => x["work_position_id"])
      .join(",");
  }
  var users = null;
  if (options.value.users != null && options.value.users.length > 0) {
    users = options.value.users.map((x) => x["user_id"]).join(",");
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
            proc: "hrm_contract_count_filter_2",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "search", va: options.value.search },
              { par: "organizations", va: organizations },
              { par: "departments", va: departments },
              { par: "type_contracts", va: type_contracts },
              { par: "work_positions", va: work_positions },
              { par: "users", va: users },
              { par: "sign_start_date", va: options.value.sign_start_date },
              { par: "sign_end_date", va: options.value.sign_end_date },
              { par: "start_start_date", va: options.value.start_start_date },
              { par: "end_start_date", va: options.value.end_start_date },
              { par: "start_end_date", va: options.value.start_end_date },
              { par: "end_end_date", va: options.value.end_end_date },
              { par: "is_link", va: options.value.path },
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
            proc: "hrm_contract_count_2",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "search", va: options.value.search },
              { par: "is_link", va: options.value.path },
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
const initDataFilter = () => {
  var organizations = null;
  if (
    options.value.organizations != null &&
    options.value.organizations.length > 0
  ) {
    organizations = options.value.organizations
      .map((x) => x["organization_id"])
      .join(",");
  }
  var departments = null;
  if (
    options.value.departments != null &&
    options.value.departments.length > 0
  ) {
    departments = options.value.departments
      .map((x) => x["department_id"])
      .join(",");
  }
  var type_contracts = null;
  if (
    options.value.type_contracts != null &&
    options.value.type_contracts.length > 0
  ) {
    type_contracts = options.value.type_contracts
      .map((x) => x["type_contract_id"])
      .join(",");
  }
  var work_positions = null;
  if (
    options.value.work_positions != null &&
    options.value.work_positions.length > 0
  ) {
    work_positions = options.value.work_positions
      .map((x) => x["work_position_id"])
      .join(",");
  }
  var users = null;
  if (options.value.users != null && options.value.users.length > 0) {
    users = options.value.users.map((x) => x["user_id"]).join(",");
  }
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_contract_list_filter_2",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "search", va: options.value.search },
              { par: "pageNo", va: options.value.pageNo },
              { par: "pageSize", va: options.value.pageSize },
              { par: "tab", va: options.value.tab },
              { par: "organizations", va: organizations },
              { par: "departments", va: departments },
              { par: "type_contracts", va: type_contracts },
              { par: "work_positions", va: work_positions },
              { par: "users", va: users },
              { par: "sign_start_date", va: options.value.sign_start_date },
              { par: "sign_end_date", va: options.value.sign_end_date },
              { par: "start_start_date", va: options.value.start_start_date },
              { par: "end_start_date", va: options.value.end_start_date },
              { par: "start_end_date", va: options.value.start_end_date },
              { par: "end_end_date", va: options.value.end_end_date },
              { par: "is_link", va: options.value.path },
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
              if (item["sign_date"] != null) {
                item["sign_date"] = moment(new Date(item["sign_date"])).format(
                  "DD/MM/YYYY"
                );
              }
              if (item["start_date"] != null) {
                item["start_date"] = moment(
                  new Date(item["start_date"])
                ).format("DD/MM/YYYY");
              }
              if (item["end_date"] != null) {
                item["end_date"] = moment(new Date(item["end_date"])).format(
                  "DD/MM/YYYY"
                );
              }
              if (item["created_date"] != null) {
                item["created_date"] = moment(
                  new Date(item["created_date"])
                ).format("DD/MM/YYYY");
              }
              if (item["liquidation_date"] != null) {
                item["liquidation_date"] = moment(
                  new Date(item["liquidation_date"])
                ).format("DD/MM/YYYY");
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
const initData = (ref) => {
  if (ref) {
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
  }
  if (isFilter.value) {
    initDataFilter();
    return;
  }
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_contract_list_2",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "search", va: options.value.search },
              { par: "pageNo", va: options.value.pageNo },
              { par: "pageSize", va: options.value.pageSize },
              { par: "tab", va: options.value.tab },
              { par: "is_link", va: options.value.path },
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
              if (item["sign_date"] != null) {
                item["sign_date"] = moment(new Date(item["sign_date"])).format(
                  "DD/MM/YYYY"
                );
              }
              if (item["start_date"] != null) {
                item["start_date"] = moment(
                  new Date(item["start_date"])
                ).format("DD/MM/YYYY");
              }
              if (item["end_date"] != null) {
                item["end_date"] = moment(new Date(item["end_date"])).format(
                  "DD/MM/YYYY"
                );
              }
              if (item["created_date"] != null) {
                item["created_date"] = moment(
                  new Date(item["created_date"])
                ).format("DD/MM/YYYY");
              }
              if (item["liquidation_date"] != null) {
                item["liquidation_date"] = moment(
                  new Date(item["liquidation_date"])
                ).format("DD/MM/YYYY");
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
  options.value.pageNo = 1;
  options.value.pageSize = 25;
  isFilter.value = false;
  initData(true);
  initCount();
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
  options.value.path = route.path;
  options.value.name = route.name;
  initData(true);
  initCount();
  initDictionary();
  initRoleFunction();
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
        <Button
          @click="toggleFilter($event)"
          type="button"
          class="ml-2 p-button-outlined p-button-secondary"
          aria:haspopup="true"
          aria-controls="overlay_panel"
        >
          <div>
            <span class="mr-2"><i class="pi pi-filter"></i></span>
            <span class="mr-2">Lọc dữ liệu</span>
            <span><i class="pi pi-chevron-down"></i></span>
          </div>
        </Button>
        <OverlayPanel
          :showCloseIcon="false"
          ref="opfilter"
          appendTo="body"
          class="p-0 m-0"
          id="overlay_panel"
          style="width: 700px"
        >
          <div class="grid formgrid m-0">
            <div
              class="col-12 md:col-12 p-0"
              style="
                min-height: unset;
                max-height: calc(100vh - 300px);
                overflow: auto;
              "
            >
              <div class="row">
                <div class="col-6 md:col-6">
                  <div class="row">
                    <div class="col-12 md:col-12 p-0">
                      <div class="form-group">
                        <label>Đơn vị</label>
                        <MultiSelect
                          :options="dictionarys[12]"
                          :filter="true"
                          :showClear="true"
                          :editable="false"
                          v-model="options.organizations"
                          optionLabel="organization_name"
                          placeholder="Chọn đơn vị"
                          class="w-full limit-width"
                          style="min-height: 36px"
                          panelClass="d-design-dropdown"
                        >
                          <template #value="slotProps">
                            <ul
                              class="p-ulchip"
                              v-if="
                                slotProps.value && slotProps.value.length > 0
                              "
                            >
                              <li
                                class="p-lichip"
                                v-for="(value, index) in slotProps.value"
                                :key="index"
                              >
                                <Chip class="mr-2 mb-2 px-3 py-2">
                                  <div class="flex">
                                    <div>
                                      <span>{{ value.organization_name }}</span>
                                    </div>
                                    <span
                                      tabindex="0"
                                      class="p-chip-remove-icon pi pi-times-circle format-flex-center"
                                      @click="
                                        removeFilter(
                                          index,
                                          options.organizations
                                        );
                                        $event.stopPropagation();
                                      "
                                      v-tooltip.top="'Xóa'"
                                    ></span>
                                  </div>
                                </Chip>
                              </li>
                            </ul>
                            <span v-else>
                              {{ slotProps.placeholder }}
                            </span>
                          </template>
                        </MultiSelect>
                      </div>
                    </div>
                    <div class="col-12 md:col-12 p-0">
                      <div class="form-group">
                        <label>Phòng ban</label>
                        <MultiSelect
                          :options="dictionarys[3]"
                          :filter="true"
                          :showClear="true"
                          :editable="false"
                          v-model="options.departments"
                          optionLabel="department_name"
                          placeholder="Chọn phòng ban"
                          class="w-full limit-width"
                          style="min-height: 36px"
                          panelClass="d-design-dropdown"
                        >
                          <template #value="slotProps">
                            <ul
                              class="p-ulchip"
                              v-if="
                                slotProps.value && slotProps.value.length > 0
                              "
                            >
                              <li
                                class="p-lichip"
                                v-for="(value, index) in slotProps.value"
                                :key="index"
                              >
                                <Chip class="mr-2 mb-2 px-3 py-2">
                                  <div class="flex">
                                    <div>
                                      <span>{{ value.department_name }}</span>
                                    </div>
                                    <span
                                      tabindex="0"
                                      class="p-chip-remove-icon pi pi-times-circle format-flex-center"
                                      @click="
                                        removeFilter(
                                          index,
                                          options.departments
                                        );
                                        $event.stopPropagation();
                                      "
                                      v-tooltip.top="'Xóa'"
                                    ></span>
                                  </div>
                                </Chip>
                              </li>
                            </ul>
                            <span v-else>
                              {{ slotProps.placeholder }}
                            </span>
                          </template>
                        </MultiSelect>
                      </div>
                    </div>
                    <div class="col-12 md:col-12 p-0">
                      <div class="form-group">
                        <label>Loại hợp đồng</label>
                        <MultiSelect
                          :options="dictionarys[2]"
                          :filter="true"
                          :showClear="true"
                          :editable="false"
                          v-model="options.type_contracts"
                          optionLabel="type_contract_name"
                          placeholder="Chọn loại hợp đồng"
                          class="w-full limit-width"
                          style="min-height: 36px"
                          panelClass="d-design-dropdown"
                        >
                          <template #value="slotProps">
                            <ul
                              class="p-ulchip"
                              v-if="
                                slotProps.value && slotProps.value.length > 0
                              "
                            >
                              <li
                                class="p-lichip"
                                v-for="(value, index) in slotProps.value"
                                :key="index"
                              >
                                <Chip class="mr-2 mb-2 px-3 py-2">
                                  <div class="flex">
                                    <div>
                                      <span>{{
                                        value.type_contract_name
                                      }}</span>
                                    </div>
                                    <span
                                      tabindex="0"
                                      class="p-chip-remove-icon pi pi-times-circle format-flex-center"
                                      @click="
                                        removeFilter(
                                          index,
                                          options.type_contracts
                                        );
                                        $event.stopPropagation();
                                      "
                                      v-tooltip.top="'Xóa'"
                                    ></span>
                                  </div>
                                </Chip>
                              </li>
                            </ul>
                            <span v-else>
                              {{ slotProps.placeholder }}
                            </span>
                          </template>
                        </MultiSelect>
                      </div>
                    </div>
                    <div class="col-12 md:col-12 p-0">
                      <div class="form-group">
                        <label>Vị trí</label>
                        <MultiSelect
                          :options="dictionarys[4]"
                          :filter="true"
                          :showClear="true"
                          :editable="false"
                          v-model="options.work_positions"
                          optionLabel="work_position_name"
                          placeholder="Chọn vị trí làm việc"
                          class="w-full limit-width"
                          style="min-height: 36px"
                          panelClass="d-design-dropdown"
                        >
                          <template #value="slotProps">
                            <ul
                              class="p-ulchip"
                              v-if="
                                slotProps.value && slotProps.value.length > 0
                              "
                            >
                              <li
                                class="p-lichip"
                                v-for="(value, index) in slotProps.value"
                                :key="index"
                              >
                                <Chip class="mr-2 mb-2 px-3 py-2">
                                  <div class="flex">
                                    <div>
                                      <span>{{
                                        value.work_position_name
                                      }}</span>
                                    </div>
                                    <span
                                      tabindex="0"
                                      class="p-chip-remove-icon pi pi-times-circle format-flex-center"
                                      @click="
                                        removeFilter(
                                          index,
                                          options.work_positions
                                        );
                                        $event.stopPropagation();
                                      "
                                      v-tooltip.top="'Xóa'"
                                    ></span>
                                  </div>
                                </Chip>
                              </li>
                            </ul>
                            <span v-else>
                              {{ slotProps.placeholder }}
                            </span>
                          </template>
                        </MultiSelect>
                      </div>
                    </div>
                  </div>
                </div>
                <div class="col-6 md:col-6">
                  <div class="row">
                    <div class="col-12 md:col-12">
                      <div class="form-group m-0">
                        <label>Ngày ký</label>
                      </div>
                    </div>
                    <div class="col-6 md:col-6">
                      <div class="form-group">
                        <Calendar
                          :showIcon="true"
                          class="ip36"
                          autocomplete="on"
                          inputId="time24"
                          v-model="options.sign_start_date"
                          @date-select="changeSignDate()"
                          @input="changeSignDate()"
                          placeholder="Từ ngày"
                        />
                      </div>
                    </div>
                    <div class="col-6 md:col-6">
                      <div class="form-group">
                        <Calendar
                          :showIcon="true"
                          class="ip36"
                          autocomplete="on"
                          inputId="time24"
                          v-model="options.sign_end_date"
                          @date-select="changeSignDate()"
                          @input="changeSignDate()"
                          placeholder="Đến ngày"
                        />
                      </div>
                    </div>
                    <div class="col-12 md:col-12">
                      <div class="form-group">
                        <label>Người ký</label>
                        <MultiSelect
                          :options="dictionarys[8]"
                          v-model="options.users"
                          :filter="true"
                          :showClear="true"
                          :editable="false"
                          optionLabel="profile_user_name"
                          placeholder="Chọn người ký"
                          class="w-full limit-width"
                          style="min-height: 36px"
                          panelClass="d-design-dropdown"
                        >
                          <template #value="slotProps">
                            <ul
                              class="p-ulchip"
                              v-if="
                                slotProps.value && slotProps.value.length > 0
                              "
                            >
                              <li
                                class="p-lichip"
                                v-for="(value, index) in slotProps.value"
                                :key="index"
                              >
                                <Chip
                                  :image="value.avatar"
                                  :label="value.profile_user_name"
                                  class="mr-2 mb-2 px-3 py-2"
                                >
                                  <div class="flex">
                                    <div class="format-flex-center">
                                      <Avatar
                                        v-bind:label="
                                          value.avatar
                                            ? ''
                                            : (
                                                value.profile_user_name ?? ''
                                              ).substring(0, 1)
                                        "
                                        v-bind:image="
                                          value.avatar
                                            ? basedomainURL + value.avatar
                                            : basedomainURL +
                                              '/Portals/Image/noimg.jpg'
                                        "
                                        :style="{
                                          background:
                                            bgColor[value.is_order % 7],
                                          color: '#ffffff',
                                          width: '2rem',
                                          height: '2rem',
                                        }"
                                        class="mr-2 text-avatar"
                                        size="xlarge"
                                        shape="circle"
                                      />
                                    </div>
                                    <div class="format-flex-center text-left">
                                      <span>{{ value.profile_user_name }}</span>
                                    </div>
                                    <span
                                      tabindex="0"
                                      class="p-chip-remove-icon pi pi-times-circle format-flex-center"
                                      @click="
                                        removeFilter(index, options.users);
                                        $event.stopPropagation();
                                      "
                                      v-tooltip.top="'Xóa'"
                                    ></span>
                                  </div>
                                </Chip>
                              </li>
                            </ul>
                            <span v-else>
                              {{ slotProps.placeholder }}
                            </span>
                          </template>
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
                                      : basedomainURL +
                                        '/Portals/Image/noimg.jpg'
                                  "
                                  :style="{
                                    background:
                                      bgColor[slotProps.option.is_order % 7],
                                    color: '#ffffff',
                                    width: '3rem',
                                    height: '3rem',
                                    fontSize: '1.4rem !important',
                                  }"
                                  class="text-avatar m-0"
                                  size="xlarge"
                                  shape="circle"
                                />
                              </div>
                              <div class="format-center text-left ml-3">
                                <div>
                                  <div class="mb-1">
                                    {{ slotProps.option.profile_user_name }}
                                  </div>
                                  <div class="description">
                                    <div>
                                      <span>{{
                                        slotProps.option.profile_code
                                      }}</span
                                      ><span
                                        v-if="slotProps.option.department_name"
                                      >
                                        |
                                        {{
                                          slotProps.option.department_name
                                        }}</span
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
                    <div class="col-12 md:col-12">
                      <div class="form-group m-0">
                        <label>Ngày hiệu lực</label>
                      </div>
                    </div>
                    <div class="col-6 md:col-6">
                      <div class="form-group">
                        <Calendar
                          :showIcon="true"
                          class="ip36"
                          autocomplete="on"
                          inputId="time24"
                          v-model="options.start_start_date"
                          @date-select="changeStartDate()"
                          @input="changeStartDate()"
                          placeholder="Từ ngày"
                        />
                      </div>
                    </div>
                    <div class="col-6 md:col-6">
                      <div class="form-group">
                        <Calendar
                          :showIcon="true"
                          class="ip36"
                          autocomplete="on"
                          inputId="time24"
                          v-model="options.end_start_date"
                          @date-select="changeStartDate()"
                          @input="changeStartDate()"
                          placeholder="Đến ngày"
                        />
                      </div>
                    </div>
                    <div class="col-12 md:col-12">
                      <div class="form-group m-0">
                        <label>Ngày hết hạn</label>
                      </div>
                    </div>
                    <div class="col-6 md:col-6">
                      <div class="form-group">
                        <Calendar
                          :showIcon="true"
                          class="ip36"
                          autocomplete="on"
                          inputId="time24"
                          v-model="options.start_end_date"
                          @date-select="changeEndDate()"
                          @input="changeEndDate()"
                          placeholder="Từ ngày"
                        />
                      </div>
                    </div>
                    <div class="col-6 md:col-6">
                      <div class="form-group">
                        <Calendar
                          :showIcon="true"
                          class="ip36"
                          autocomplete="on"
                          inputId="time24"
                          v-model="options.end_end_date"
                          @date-select="changeEndDate()"
                          @input="changeEndDate()"
                          placeholder="Đến ngày"
                        />
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
            <div class="col-12 md:col-12 p-0">
              <Toolbar
                class="border-none surface-0 outline-none px-0 pb-0 w-full"
              >
                <template #start>
                  <Button
                    @click="resetFilter()"
                    class="p-button-outlined"
                    label="Bỏ chọn"
                  ></Button>
                </template>
                <template #end>
                  <Button @click="filter($event)" label="Lọc"></Button>
                </template>
              </Toolbar>
            </div>
          </div>
        </OverlayPanel>
      </template>
      <template #end>
        <Button
          v-if="functions.is_add"
          @click="openAddDialog('Thêm mới hợp đồng')"
          label="Thêm mới"
          icon="pi pi-plus"
          class="mr-2"
        />
        <!-- <Button
          icon="pi pi-trash"
          label="Xóa"
          :class="{
            'p-button-danger': options.filterContract_id != null,
            'btn-hidden p-button-danger': options.filterContract_id == null,
          }"
          @click="deleteItem()"
          class="mr-2"
        /> -->
        <Button
          v-if="functions.tienich"
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
        :globalFilterFields="['type_contract_name']"
        v-model:selection="selectedNodes"
        selectionMode="single"
        dataKey="contract_id"
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
          field="contract_code"
          header="Số HĐ"
          headerStyle="text-align:center;max-width:150px;height:50px"
          bodyStyle="text-align:center;max-width:150px;"
          class="align-items-center justify-content-center text-center"
        />
        <Column
          field="profile_user_name"
          header="Tên nhân sự"
          headerStyle="height:50px;max-width:auto;"
        >
          <template #body="slotProps">
            <b>{{ slotProps.data.profile_user_name }}</b>
          </template>
        </Column>
        <!-- <Column
          field="department_name"
          header="Phòng ban"
          headerStyle="text-align:center;max-width:120px;height:50px"
          bodyStyle="text-align:center;max-width:120px;"
          class="align-items-center justify-content-center text-center"
        >
          <template #body="slotProps">
            {{ slotProps.data.department_name }}
          </template>
        </Column> -->
        <Column
          field="type_contract_name"
          header="Loại hợp đồng"
          headerStyle="text-align:center;max-width:180px;height:50px"
          bodyStyle="text-align:center;max-width:180px;"
          class="align-items-center justify-content-center text-center"
        >
          <template #body="slotProps">
            {{ slotProps.data.type_contract_name }}
          </template>
        </Column>
        <Column
          field="sign_date"
          header="Ngày ký"
          headerStyle="text-align:center;max-width:100px;height:50px"
          bodyStyle="text-align:center;max-width:100px;"
          class="align-items-center justify-content-center text-center"
        >
          <template #body="slotProps">
            <span>{{ slotProps.data.sign_date }}</span>
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
        <!-- <Column
          field="sign_user_name"
          header="Người ký"
          headerStyle="text-align:center;max-width:120px;height:50px"
          bodyStyle="text-align:center;max-width:120px;"
          class="align-items-center justify-content-center text-center"
        >
          <template #body="slotProps">
            {{ slotProps.data.sign_user_name }}
          </template>
        </Column> -->
        <Column
          field="created_date"
          header="Ngày/Người lập"
          headerStyle="text-align:center;max-width:120px;height:50px"
          bodyStyle="text-align:center;max-width:120px;"
          class="align-items-center justify-content-center text-center"
        >
          <!-- <template #body="slotProps">
            <div class="flex justify-content-center">
              <span class="format-center mr-2">{{
                slotProps.data.created_date
              }}</span>
              <div>
                <Avatar
                  v-bind:label="
                    slotProps.data.avatar
                      ? ''
                      : slotProps.data.full_name.substring(0, 1)
                  "
                  v-bind:image="
                    slotProps.data.avatar
                      ? basedomainURL + slotProps.data.avatar
                      : basedomainURL + '/Portals/Image/noimg.jpg'
                  "
                  style="
                    background-color: #2196f3;
                    color: #ffffff;
                    width: 2rem;
                    height: 2rem;
                    font-size: 1rem !important;
                  "
                  :style="{
                    background: bgColor[slotProps.data.created_is_order % 7],
                  }"
                  class="text-avatar"
                  size="xlarge"
                  shape="circle"
                  v-tooltip.top="slotProps.data.full_name"
                />
              </div>
            </div>
          </template> -->
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
                if (slotProps.data.is_function) {
                  toggleStatus(slotProps.data, $event);
                  $event.stopPropagation();
                }
              "
              aria:haspopup="true"
              aria-controls="overlay_panel_status"
            >
              <Button
                :label="slotProps.data.status_name"
                :icon="slotProps.data.is_function ? 'pi pi-chevron-down' : ''"
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
                  v-model="contract.status"
                  optionLabel="title"
                  optionValue="value"
                  placeholder="Chọn trạng thái"
                  class="ip36"
                  @change="setStatus(contract.status, $event)"
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
              <div v-if="contract.status === 3" class="form-group">
                <label>Ngày thanh lý</label>
                <Calendar
                  :showIcon="true"
                  class="ip36"
                  autocomplete="on"
                  inputId="time24"
                  v-model="contract.liquidation_date"
                  placeholder="DD/MM/YYYY"
                  disabled="true"
                />
              </div>
              <div v-if="contract.status === 3" class="form-group m-0">
                <label>Nội dung</label>
                <Textarea
                  v-model="contract.liquidation_content"
                  :autoResize="true"
                  rows="2"
                  cols="30"
                  disabled="true"
                />
              </div>
            </OverlayPanel>
          </template>
        </Column>
        <Column
          header=""
          headerStyle="text-align:center;max-width:150px"
          bodyStyle="text-align:center;max-width:150px"
          class="align-items-center justify-content-center text-center"
        >
          <template #body="slotProps">
            <ul
              class="flex m-0 p-0 justify-content-right"
              style="list-style: none; justify-content: right"
            >
              <li>
                <Button
                  :icon="
                    slotProps.data.is_star ? 'pi pi-star-fill' : 'pi pi-star'
                  "
                  :class="{ 'icon-star': slotProps.data.is_star }"
                  class="p-button-rounded p-button-text"
                  @click="
                    () => {
                      if (slotProps.data.is_function) {
                        setStar(slotProps.data);
                        $event.stopPropagation();
                      } else {
                        swal.fire({
                          title: 'Thông báo!',
                          text: 'Bạn không có quyền sử dụng tính năng này!',
                          icon: 'error',
                          confirmButtonText: 'OK',
                        });
                        return;
                      }
                    }
                  "
                  v-tooltip.top="
                    slotProps.data.is_star ? 'Hợp đồng cần lưu ý' : ''
                  "
                  style="font-size: 15px; color: #000"
                />
              </li>
              <li v-if="slotProps.data.is_function">
                <Button
                  icon="pi pi-ellipsis-h"
                  class="p-button-rounded p-button-text"
                  @click="toggleMores($event, slotProps.data)"
                  aria-haspopup="true"
                  aria-controls="overlay_More"
                  v-tooltip.top="'Tác vụ'"
                />
              </li>
            </ul>
          </template>
        </Column>
        <template #empty>
          <div
            class="align-items-center justify-content-center p-4 text-center m-auto"
            style="
              display: flex;
              width: 100%;
              height: calc(100vh - 326px);
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
  <dialogcontract
    :key="componentKey['0']"
    :headerDialog="headerDialog"
    :displayDialog="displayDialog"
    :closeDialog="closeDialog"
    :isAdd="isAdd"
    :isView="false"
    :model="model"
    :files="files"
    :selectFile="selectFile"
    :removeFile="removeFile"
    :dictionarys="dictionarys"
    :initData="initData"
  />
  <framepreview
    :key="componentKey['1']"
    :headerDialog="headerDialogFrame"
    :displayDialog="displayDialogFrame"
    :closeDialog="closeDialogFrame"
    :type="2"
    :model="contract"
  />
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
        @click="updateStatus(contract, 3, $event, modelLiquidation)"
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
